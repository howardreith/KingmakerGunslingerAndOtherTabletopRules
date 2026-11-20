#!/usr/bin/env python3
"""agent-review controller: Claude Opus develops, Codex GPT-6.2 Astra reviews.

The controller is the deterministic party. It owns enrollment, candidate
identity (H/B/M), model launch settings and provenance checks, reviewer-output
validation, the single-writer lock, idempotent GitHub publication, bounded
retries and round budgets, transcript archive commits and readiness.

Models never publish. Comment text is never executed. See
docs/AGENT-REVIEW-WORKFLOW.md for architecture, permissions and recovery.
"""
from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import re
import shutil
import socket
import string
import subprocess
import sys
import time
import uuid
from pathlib import Path

PROTOCOL = "agent-review.v1"
STATE_SCHEMA = "agent-review.state.v1"
REVIEW_SCHEMA_FILE = "review-result.v1.schema.json"
REPORT_SCHEMA_FILE = "developer-report.v1.schema.json"
HERE = Path(__file__).resolve().parent
COMMENT_LIMIT = 60000
EXIT_OK, EXIT_BLOCKED, EXIT_PAUSED, EXIT_USAGE = 0, 2, 3, 4

# Owner-selected models (protocol section 3). Pinned in code so a caller-supplied
# project file cannot substitute another model; changing them is a reviewed
# workflow change that must be re-activated.
POLICY = {
    "developer": {"model": "opus", "effort": "high", "allowed_model_pattern": "^claude-opus-"},
    "reviewer": {"model": "gpt-6.2-astra", "effort": "high"},
}
POSITIVE_ASSESSMENTS = {"fix_verified", "rebuttal_accepted", "deferral_accepted"}
REJECTING_ASSESSMENTS = {"fix_rejected", "rebuttal_rejected", "deferral_rejected", "still_open"}
COMPATIBLE_ASSESSMENTS = {
    "fixed": {"fix_verified", "fix_rejected", "still_open"},
    "rebutted": {"rebuttal_accepted", "rebuttal_rejected", "still_open"},
    "deferred_optional": {"deferral_accepted", "deferral_rejected", "still_open"},
    "unresolved_blocker": {"still_open"},
    None: {"still_open"},
}
DECISIONS_BY_TYPE = {
    "intermediate": {"changes_required", "checkpoint_passed", "blocked"},
    "final": {"changes_required", "ready_for_owner_review", "blocked"},
}
CLAUDE_SESSION_ENV = (
    "CLAUDE_CODE_CHILD_SESSION", "CLAUDE_CODE_SESSION_ID", "CLAUDE_CODE_MESSAGING_SOCKET",
    "CLAUDE_CODE_MESSAGING_TOKEN", "CLAUDE_CODE_CHILD_NOTES_DIR", "CLAUDE_CODE_SESSION_ATTENDED",
    "CLAUDE_CODE_ENTRYPOINT", "CLAUDE_CODE_EXECPATH",
)
USAGE_LIMIT_RE = re.compile(r"usage limit|rate.?limit(ed)? (reached|exceeded)|quota|insufficient credits|"
                            r"model .*not (available|supported|found)", re.I)
TOKEN_RE = re.compile(r"gh[pousr]_[A-Za-z0-9]{36,}|github_pat_[A-Za-z0-9_]{60,}|sk-[A-Za-z0-9_-]{32,}")
PEM_BLOCK_RE = re.compile(r"-----BEGIN ([A-Z0-9 ]*)PRIVATE KEY-----.*?-----END \1PRIVATE KEY-----", re.S)
PEM_BEGIN_RE = re.compile(r"-----BEGIN [A-Z0-9 ]*PRIVATE KEY-----")


# --------------------------------------------------------------------------- errors

class Stop(Exception):
    """A visible workflow stop: BLOCKED (impediment) or PAUSED (resumable)."""

    def __init__(self, status: str, reason: str):
        assert status in ("BLOCKED", "PAUSED")
        super().__init__(f"{status}: {reason}")
        self.status, self.reason = status, reason


class UsageError(Exception):
    """Refused: bad input, wrong target or lock held. Nothing is published."""


class CommandError(Exception):
    def __init__(self, argv, proc):
        tail = (proc.stderr or proc.stdout or "").strip()[-800:]
        super().__init__(f"{Path(str(argv[0])).name} {' '.join(map(str, argv[1:3]))} exited {proc.returncode}: {tail}")
        self.returncode, self.stderr = proc.returncode, proc.stderr or ""


# --------------------------------------------------------------------------- utilities

def now_iso() -> str:
    return dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_text(text: str) -> str:
    return sha256_bytes(text.encode("utf-8"))


def sha256_file(path) -> str:
    return sha256_bytes(Path(path).read_bytes())


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8"))


def write_text(path, text: str):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def atomic_write_json(path, obj):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_name(path.name + f".{os.getpid()}.tmp")
    with open(tmp, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(obj, handle, indent=2, sort_keys=True)
        handle.write("\n")
        handle.flush()
        os.fsync(handle.fileno())
    os.replace(tmp, path)


def expand(value: str) -> str:
    return os.path.expandvars(os.path.expanduser(value))


def resolve_exe(name: str) -> str:
    candidate = expand(name)
    if os.path.isabs(candidate):
        return candidate
    found = shutil.which(candidate)
    if not found:
        raise UsageError(f"Executable not found on PATH: {name}")
    return found


def run(argv, cwd=None, input_text=None, env=None, timeout=300, check=True):
    proc = subprocess.run([str(a) for a in argv], cwd=cwd, input=input_text, capture_output=True,
                          text=True, encoding="utf-8", errors="replace", env=env, timeout=timeout)
    if check and proc.returncode != 0:
        raise CommandError(argv, proc)
    return proc


def pid_alive(pid) -> bool:
    if not pid:
        return False
    if os.name == "nt":
        import ctypes
        handle = ctypes.windll.kernel32.OpenProcess(0x1000, False, int(pid))
        if not handle:
            return False
        code = ctypes.c_ulong()
        ok = ctypes.windll.kernel32.GetExitCodeProcess(handle, ctypes.byref(code))
        ctypes.windll.kernel32.CloseHandle(handle)
        return bool(ok) and code.value == 259
    try:
        os.kill(int(pid), 0)
        return True
    except OSError:
        return False


def kill_tree(proc):
    if os.name == "nt":
        subprocess.run(["taskkill", "/PID", str(proc.pid), "/T", "/F"], capture_output=True)
    else:
        proc.kill()


PEM_PARTIAL_RE = re.compile(r"-----BEGIN [A-Z0-9 ]*PRIVATE KEY-----(?:\\n|\r?\n)?(?:[A-Za-z0-9+/=]{16,}(?:\\n|\r?\n)?)*")


def sanitize(text: str, strict: bool = True):
    """Redact credentials and local home paths.

    strict (anything published or committed): an incomplete private-key block stops publication.
    non-strict (local model inputs such as the developer tool transcript): the marker and any
    key-like lines after it are redacted instead, since that text still reaches the model provider."""
    count = 0

    def _redact(_match):
        nonlocal count
        count += 1
        return "[REDACTED-CREDENTIAL]"

    text = PEM_BLOCK_RE.sub(_redact, text)
    if PEM_BEGIN_RE.search(text):
        if strict:
            raise Stop("BLOCKED", "publishable text contains an incomplete private-key block; refusing to publish it")
        text = PEM_PARTIAL_RE.sub(_redact, text)
    text = TOKEN_RE.sub(_redact, text)
    home = str(Path.home())
    for variant in {home, home.replace("\\", "/"), home.replace("\\", "\\\\")}:
        text = re.sub(re.escape(variant), "~", text, flags=re.I)
    return text, count


def normalize_body(text: str) -> str:
    return "\n".join(line.rstrip() for line in (text or "").replace("\r\n", "\n").split("\n")).strip()


def short(sha) -> str:
    return (sha or "")[:10]


def agent_home() -> Path:
    return Path(expand(os.environ.get("AGENT_REVIEW_HOME", "~/.agent-review")))


def branch_key(repository: str, branch: str) -> str:
    return re.sub(r"[^A-Za-z0-9._-]+", "_", f"{repository}__{branch}")


# --------------------------------------------------------------------------- schema

_TYPES = {"string": str, "integer": int, "boolean": bool, "array": list, "object": dict, "null": type(None)}


def validate_schema(value, schema, path="$"):
    """Validate the JSON-Schema subset used by this workflow's schemas."""
    errors = []
    types = schema.get("type")
    if types is not None:
        names = types if isinstance(types, list) else [types]
        ok = False
        for name in names:
            if name == "integer" and isinstance(value, bool):
                continue
            if isinstance(value, _TYPES[name]):
                ok = True
        if not ok:
            return [f"{path}: expected {names}, got {type(value).__name__}"]
    if "enum" in schema and value not in schema["enum"]:
        errors.append(f"{path}: {value!r} not in {schema['enum']}")
    if isinstance(value, dict):
        props = schema.get("properties", {})
        for key in schema.get("required", []):
            if key not in value:
                errors.append(f"{path}: missing required '{key}'")
        if schema.get("additionalProperties") is False:
            for key in value:
                if key not in props:
                    errors.append(f"{path}: unexpected property '{key}'")
        for key, sub in props.items():
            if key in value:
                errors.extend(validate_schema(value[key], sub, f"{path}.{key}"))
    if isinstance(value, list) and "items" in schema:
        for index, item in enumerate(value):
            errors.extend(validate_schema(item, schema["items"], f"{path}[{index}]"))
    return errors


def strict_schema_copy(schema):
    """Model-facing copy without annotation keywords ($comment etc.)."""
    if isinstance(schema, dict):
        return {k: strict_schema_copy(v) for k, v in schema.items() if not k.startswith("$")}
    if isinstance(schema, list):
        return [strict_schema_copy(v) for v in schema]
    return schema


# --------------------------------------------------------------------------- workflow version

def workflow_files(root: Path):
    files = []
    for path in sorted(root.rglob("*")):
        if not path.is_file() or "__pycache__" in path.parts or path.name.startswith("test_"):
            continue
        if path.name == "MANIFEST.json" or path.suffix not in (".py", ".json", ".md"):
            continue
        files.append(path.relative_to(root).as_posix())
    return files


def compute_fingerprint(root: Path):
    """(activation fingerprint, all-file hashes).

    The fingerprint covers behavioral files (controller, schemas, prompts, developer
    settings). Project configs and mission plans are snapshotted into each enrollment,
    so adding a plan does not pause running enrollments.
    """
    hashes = {rel: sha256_file(root / rel) for rel in workflow_files(root)}
    behavioral = {rel: h for rel, h in hashes.items()
                  if not rel.startswith(("projects/", "plans/")) and rel != "README.md"}
    digest = sha256_text("\n".join(f"{rel}:{h}" for rel, h in sorted(behavioral.items())))
    return digest, hashes


def workflow_fingerprint(root: Path) -> str:
    """Return the activated workflow fingerprint, verifying an installed manifest."""
    digest, hashes = compute_fingerprint(root)
    manifest = root / "MANIFEST.json"
    if manifest.exists():
        recorded = read_json(manifest)
        if recorded.get("files") != hashes or recorded.get("fingerprint") != digest:
            raise Stop("PAUSED", f"installed workflow at {root} differs from its MANIFEST; reinstall or review the change")
    return digest


# --------------------------------------------------------------------------- git

class Git:
    def __init__(self, root):
        self.root = str(root)

    def __call__(self, *args, check=True, input_text=None):
        return run(["git", "-C", self.root, *args], check=check, input_text=input_text).stdout.strip()

    def ok(self, *args) -> bool:
        return run(["git", "-C", self.root, *args], check=False).returncode == 0

    def head(self):
        return self("rev-parse", "HEAD")

    def branch(self):
        return self("symbolic-ref", "--quiet", "--short", "HEAD")

    def toplevel(self):
        return self("rev-parse", "--show-toplevel")

    def clean(self) -> bool:
        return self("status", "--porcelain", "--untracked-files=all") == ""

    def remote_url(self):
        return self("remote", "get-url", "origin")

    def remote_head(self, branch):
        out = self("ls-remote", "--heads", "origin", f"refs/heads/{branch}")
        return out.split()[0] if out else None

    def fetch(self, branch):
        self("fetch", "--quiet", "origin", f"+refs/heads/{branch}:refs/remotes/origin/{branch}")
        return self("rev-parse", f"refs/remotes/origin/{branch}")

    def is_ancestor(self, a, b) -> bool:
        return self.ok("merge-base", "--is-ancestor", a, b)

    def blob_sha256(self, commit, path):
        proc = subprocess.run(["git", "-C", self.root, "show", f"{commit}:{path}"], capture_output=True)
        return sha256_bytes(proc.stdout) if proc.returncode == 0 else None

    def tree_files(self, commit, folder):
        return [p for p in self("ls-tree", "-r", "--name-only", commit, "--", folder).splitlines() if p]


def same_path(a, b) -> bool:
    return os.path.normcase(os.path.abspath(str(a))) == os.path.normcase(os.path.abspath(str(b)))


ARCHIVE_NAME_RE = r"\d{3}-[a-z0-9][a-z0-9-]*\.(md|json)"


def check_archive_only(git: Git, source: str, archive: str, transcript_dir: str, pr_number: int, expected=None):
    """True only if every commit in source..archive adds new inert records for this PR and nothing else.

    With ``expected`` (path -> sha256 of the controller's recorded records), every
    added path must be one the controller wrote, with identical content.
    """
    if source == archive:
        return True, []
    if not git.is_ancestor(source, archive):
        return False, [f"{short(source)} is not an ancestor of {short(archive)}"]
    pattern = re.compile(rf"^{re.escape(transcript_dir.strip('/'))}/pr-{int(pr_number)}/{ARCHIVE_NAME_RE}$")
    reasons = _additions_only(git, source, archive, pattern)
    for line in git("rev-list", "--reverse", "--parents", f"{source}..{archive}").splitlines():
        ids = line.split()
        if len(ids) != 2:
            reasons.append(f"{short(ids[0])} is a merge or root commit")
            continue
        reasons += [f"{short(ids[0])}: {r}" for r in _additions_only(git, ids[1], ids[0], pattern)]
    if expected is not None and not reasons:
        added = [ln.partition("\t")[2] for ln in git("diff", "--raw", "--no-renames", source, archive).splitlines() if ln]
        for path in added:
            if path not in expected:
                reasons.append(f"{path}: not a record the controller wrote")
            elif git.blob_sha256(archive, path) != expected[path]:
                reasons.append(f"{path}: content differs from the controller's record")
    return not reasons, reasons


def _additions_only(git: Git, a: str, b: str, pattern):
    reasons = []
    raw = git("diff", "--raw", "--no-renames", "--no-ext-diff", a, b)
    for line in filter(None, raw.splitlines()):
        meta, _, path = line.partition("\t")
        fields = meta.split()
        old_mode, new_mode, status = fields[0].lstrip(":"), fields[1], fields[4]
        if status != "A":
            reasons.append(f"{status} {path}: only new records may be added")
        elif old_mode != "000000" or new_mode != "100644":
            reasons.append(f"{path}: mode {new_mode} is not a plain file")
        elif not pattern.match(path):
            reasons.append(f"{path}: outside this PR's transcript record pattern")
    return reasons


# --------------------------------------------------------------------------- GitHub boundary

class GitHub:
    """gh CLI boundary. Bodies always travel in files; never through a shell."""

    PR_FIELDS = ("number,url,state,isDraft,headRefName,headRefOid,baseRefName,author,reviewRequests,body,"
                 "isCrossRepository,headRepository,headRepositoryOwner")

    def __init__(self, repo: str, gh: str = "gh"):
        self.repo, self.gh = repo, resolve_exe(gh)

    def _json(self, *args):
        return json.loads(run([self.gh, *args], timeout=180).stdout or "null")

    def _lines(self, *args):
        out = run([self.gh, *args], timeout=300).stdout
        return [json.loads(line) for line in out.splitlines() if line.strip()]

    def viewer(self) -> str:
        return self._json("api", "user")["login"]

    def pr(self, number: int) -> dict:
        return self._json("pr", "view", str(number), "--repo", self.repo, "--json", self.PR_FIELDS)

    def create_draft_pr(self, head, base, title, body_file) -> int:
        out = run([self.gh, "pr", "create", "--repo", self.repo, "--draft", "--head", head, "--base", base,
                   "--title", title, "--body-file", str(body_file)], timeout=180).stdout.strip()
        match = re.search(rf"/{re.escape(self.repo)}/pull/(\d+)", out)
        if not match:
            raise UsageError(f"could not identify the created PR from: {out[-200:]}")
        return int(match.group(1))

    def edit_body(self, number, body_file):
        run([self.gh, "pr", "edit", str(number), "--repo", self.repo, "--body-file", str(body_file)], timeout=180)

    def comments(self, number):
        return self._lines("api", "--paginate", f"repos/{self.repo}/issues/{number}/comments",
                           "--jq", ".[] | {id: .id, user: .user.login, body: .body, url: .html_url} | @json")

    def post_comment(self, number, body_file) -> dict:
        data = self._json("api", f"repos/{self.repo}/issues/{number}/comments", "-F", f"body=@{body_file}")
        return {"id": data["id"], "url": data["html_url"]}

    def required_checks(self, branch):
        """Required status-check contexts from branch protection and rulesets. Errors propagate."""
        contexts = set()
        proc = run([self.gh, "api", f"repos/{self.repo}/branches/{branch}/protection/required_status_checks"],
                   check=False, timeout=180)
        if proc.returncode == 0:
            data = json.loads(proc.stdout or "{}")
            contexts.update(data.get("contexts") or [])
            contexts.update(c.get("context") for c in data.get("checks") or [] if c.get("context"))
        elif not re.search(r"not protected|required status checks not enabled|HTTP 404", proc.stderr + proc.stdout, re.I):
            raise CommandError(["gh", "api", "protection"], proc)
        for rule in self._json("api", f"repos/{self.repo}/rules/branches/{branch}") or []:
            if rule.get("type") == "required_status_checks":
                for item in (rule.get("parameters") or {}).get("required_status_checks") or []:
                    contexts.add(item.get("context"))
        return sorted(c for c in contexts if c)

    def commit_checks(self, sha):
        runs = self._lines("api", "--paginate", f"repos/{self.repo}/commits/{sha}/check-runs",
                           "--jq", ".check_runs[] | {name: .name, status: .status, conclusion: .conclusion} | @json")
        statuses = self._lines("api", f"repos/{self.repo}/commits/{sha}/status",
                               "--jq", ".statuses[] | {name: .context, status: \"completed\", conclusion: .state} | @json")
        return runs + statuses


# --------------------------------------------------------------------------- reviewer boundary

class CodexReviewer:
    """Codex CLI boundary with explicit model, effort and read-only sandbox."""

    def __init__(self, cfg: dict):
        self.cfg = cfg
        self.exe = resolve_exe(cfg.get("executable", "codex"))
        self.home = Path(expand(cfg.get("codex_home", "~/.codex")))

    @property
    def requested(self):
        return {"model": self.cfg["model"], "effort": self.cfg["effort"], "sandbox": "read-only"}

    def version(self):
        return run([self.exe, "--version"], timeout=60).stdout.strip()

    def auth_route(self):
        proc = run([self.exe, "login", "status"], check=False, timeout=60)
        text = (proc.stdout + proc.stderr).strip().splitlines()
        return text[0] if text else "unknown"

    def argv(self, workdir, schema, result, skip_git=False):
        args = [self.exe, "-a", "never", "exec", "--ignore-user-config", "-m", self.cfg["model"],
                "-c", f'model_reasoning_effort="{self.cfg["effort"]}"']
        for item in self.cfg.get("extra_config", []):
            args += ["-c", item]
        args += ["--sandbox", "read-only"]
        for feature in self.cfg.get("disabled_features", []):
            args += ["--disable", feature]
        if skip_git:
            args.append("--skip-git-repo-check")
        return args + ["-C", str(workdir), "--output-schema", str(schema), "-o", str(result), "--json", "-"]

    def run(self, prompt, workdir, schema, result_path, events_path, stderr_path, timeout_s, skip_git=False):
        started = time.monotonic()
        with open(events_path, "w", encoding="utf-8") as out, open(stderr_path, "w", encoding="utf-8") as err:
            flags = subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0
            proc = subprocess.Popen(self.argv(workdir, schema, result_path, skip_git), stdin=subprocess.PIPE,
                                    stdout=out, stderr=err, text=True, encoding="utf-8", creationflags=flags)
            timed_out = False
            try:
                proc.communicate(prompt, timeout=timeout_s)
            except subprocess.TimeoutExpired:
                timed_out = True
                kill_tree(proc)  # reviewer is read-only: safe to terminate
                proc.wait()
        thread_id = None
        for line in Path(events_path).read_text(encoding="utf-8", errors="replace").splitlines():
            try:
                event = json.loads(line)
            except ValueError:
                continue
            if event.get("type") == "thread.started":
                thread_id = event.get("thread_id")
                break
        return {"exit_code": proc.returncode, "timed_out": timed_out, "thread_id": thread_id,
                "duration_s": round(time.monotonic() - started, 1)}

    def provenance(self, thread_id):
        info = {"rollout_found": False, "observed_model": None, "observed_effort": None, "turn_models": [],
                "turn_efforts": [], "sandbox": None, "approval_policy": None, "network": None,
                "cli_version": None, "usage": None, "plan_type": None, "subagent_suspected": False,
                "source": "Codex session rollout (CLI-recorded thread settings; not model-authored)"}
        if not thread_id:
            return info
        matches = list((self.home / "sessions").rglob(f"rollout-*{thread_id}.jsonl"))
        if not matches:
            return info
        info["rollout_found"] = True
        for line in matches[0].read_text(encoding="utf-8", errors="replace").splitlines():
            try:
                obj = json.loads(line)
            except ValueError:
                continue
            kind, payload = obj.get("type"), obj.get("payload") or {}
            ptype = payload.get("type") if isinstance(payload, dict) else None
            if kind == "session_meta":
                info["cli_version"] = payload.get("cli_version")
            elif kind == "turn_context":
                info["turn_models"].append(payload.get("model"))
                info["turn_efforts"].append(payload.get("effort"))
                info["sandbox"] = (payload.get("sandbox_policy") or {}).get("type") or info["sandbox"]
            elif ptype == "thread_settings_applied":
                settings = payload.get("thread_settings", {})
                info["observed_model"] = settings.get("model")
                info["observed_effort"] = settings.get("reasoning_effort")
                info["approval_policy"] = settings.get("approval_policy")
                info["network"] = (settings.get("permission_profile") or {}).get("network")
            elif ptype == "token_count":
                info["usage"] = (payload.get("info") or {}).get("total_token_usage")
                info["plan_type"] = (payload.get("rate_limits") or {}).get("plan_type")
            elif kind == "response_item" and ptype in ("function_call", "custom_tool_call"):
                blob = json.dumps(payload)
                if "spawn_agent" in blob or "followup_task" in blob:
                    info["subagent_suspected"] = True
        info["turn_models"] = sorted({m or "<missing>" for m in info["turn_models"]})
        info["turn_efforts"] = sorted({e or "<missing>" for e in info["turn_efforts"]})
        return info


# --------------------------------------------------------------------------- developer boundary

class ClaudeDeveloper:
    """Claude Code boundary: explicit model/effort on every headless launch and resume."""

    def __init__(self, cfg: dict, settings_path: Path, schema_path: Path):
        self.cfg, self.settings_path, self.schema_path = cfg, settings_path, schema_path
        self.exe = resolve_exe(cfg.get("executable", "claude"))
        self.config_dir = Path(expand(cfg["config_dir"])) if cfg.get("config_dir") else None

    @property
    def requested(self):
        return {"model": self.cfg["model"], "effort": self.cfg["effort"]}

    def env(self):
        env = {k: v for k, v in os.environ.items() if k not in CLAUDE_SESSION_ENV}
        if self.config_dir:
            env["CLAUDE_CONFIG_DIR"] = str(self.config_dir)
        env["CLAUDE_CODE_SUBAGENT_MODEL"] = self.cfg["model"]
        return env

    def version(self):
        return run([self.exe, "--version"], env=self.env(), timeout=60).stdout.strip()

    def _transcripts(self, session_id):
        base = (self.config_dir or Path.home() / ".claude") / "projects"
        return list(base.glob(f"*/{session_id}.jsonl")) if base.exists() else []

    def session_exists(self, session_id) -> bool:
        return bool(self._transcripts(session_id))

    def session_provenance(self, session_id):
        """CLI-recorded models (main thread) and effort settings for a session transcript."""
        models, efforts, paths = set(), set(), self._transcripts(session_id)
        for path in paths:
            for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
                efforts.update(re.findall(r'"(?:effort|perTurnEffort)":"([a-z]+)"', line))
                try:
                    obj = json.loads(line)
                except ValueError:
                    continue
                if obj.get("type") == "assistant" and not obj.get("isSidechain"):
                    model = (obj.get("message") or {}).get("model")
                    if model and model != "<synthetic>":
                        models.add(model)
        return {"found": bool(paths), "session_id": session_id, "models": sorted(models), "efforts": sorted(efforts),
                "source": "Claude Code session transcript (CLI-recorded; not model-authored)"}

    def tool_transcript(self, session_id, since_iso, limit_chars=150000):
        """CLI-recorded shell commands and their outputs from the developer session since ``since_iso``.

        This is execution evidence the read-only reviewer can inspect (it cannot rerun commands)."""
        uses, results = [], {}
        for path in self._transcripts(session_id):
            for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
                try:
                    obj = json.loads(line)
                except ValueError:
                    continue
                if obj.get("isSidechain") or (obj.get("timestamp") or "") < since_iso:
                    continue
                for item in (obj.get("message") or {}).get("content") or []:
                    if not isinstance(item, dict):
                        continue
                    if item.get("type") == "tool_use" and item.get("name") in ("Bash", "PowerShell"):
                        uses.append((obj.get("timestamp"), item["name"], item["id"], (item.get("input") or {}).get("command", "")))
                    elif item.get("type") == "tool_result":
                        content = item.get("content")
                        if isinstance(content, list):
                            content = "\n".join(c.get("text", "") for c in content if isinstance(c, dict))
                        results[item.get("tool_use_id")] = (bool(item.get("is_error")), str(content or ""))
        blocks = []
        for stamp, tool, use_id, command in uses:
            is_error, output = results.get(use_id, (None, "(no recorded result)"))
            if len(output) > 3000:
                output = output[:1500] + "\n[... truncated ...]\n" + output[-1500:]
            status = "no result" if is_error is None else ("error" if is_error else "ok")
            blocks.append(f"### {stamp} · {tool} · {status}\n\n```text\n$ {command}\n{output}\n```\n")
        text, total = [], 0
        for block in reversed(blocks):  # keep the most recent evidence within the limit
            if total + len(block) > limit_chars:
                text.append(f"_[{len(blocks) - len(text)} earlier commands omitted for length]_\n")
                break
            text.append(block)
            total += len(block)
        header = (f"# Developer tool transcript (CLI-recorded, not model-authored)\n\nSession `{session_id}`, "
                  f"commands since {since_iso}: {len(blocks)}.\n\n")
        return header + "\n".join(reversed(text))

    def argv(self, session_id, resume):
        schema = json.dumps(strict_schema_copy(read_json(self.schema_path)), separators=(",", ":"))
        args = [self.exe, "-p", "--model", self.cfg["model"], "--effort", self.cfg["effort"]]
        args += ["--resume", session_id] if resume else ["--session-id", session_id]
        return args + ["--output-format", "stream-json", "--verbose", "--permission-mode", "dontAsk",
                       "--settings", str(self.settings_path), "--strict-mcp-config", "--json-schema", schema]

    def run(self, prompt, session_id, resume, cwd, log_path, stderr_path, soft_deadline_s, on_start=None,
            on_soft_deadline=None, grace_s=3600, protected=lambda: False):
        """Run one invocation. Soft deadline: cooperative stop request. After the grace
        period the developer is terminated only if no runtime transaction is active."""
        started = time.monotonic()
        deadline_exceeded = terminated = False
        with open(log_path, "w", encoding="utf-8") as out, open(stderr_path, "w", encoding="utf-8") as err:
            flags = subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0
            proc = subprocess.Popen(self.argv(session_id, resume), cwd=cwd, env=self.env(), stdin=subprocess.PIPE,
                                    stdout=out, stderr=err, text=True, encoding="utf-8", creationflags=flags)
            if on_start:
                on_start(proc.pid)
            proc.stdin.write(prompt)
            proc.stdin.close()
            while True:
                try:
                    proc.wait(timeout=30)
                    break
                except subprocess.TimeoutExpired:
                    elapsed = time.monotonic() - started
                    if elapsed > soft_deadline_s and not deadline_exceeded:
                        deadline_exceeded = True
                        if on_soft_deadline:
                            on_soft_deadline()
                    if deadline_exceeded and elapsed > soft_deadline_s + grace_s and not protected():
                        kill_tree(proc)
                        proc.wait()
                        terminated = True
                        break
                except KeyboardInterrupt:
                    continue  # the child owns its transaction; use `stop` to pause at the boundary
        parsed = self.parse(log_path)
        prov = self.session_provenance(session_id)
        parsed.update({"exit_code": proc.returncode, "deadline_exceeded": deadline_exceeded, "terminated": terminated,
                       "duration_s": round(time.monotonic() - started, 1), "observed_effort": prov["efforts"]})
        return parsed

    @staticmethod
    def parse(log_path):
        out = {"init_model": None, "api_key_source": None, "session_id": None, "main_models": [],
               "result": None, "structured_output": None, "permission_denials": [], "model_usage": {},
               "cost_usd": None, "is_error": True, "subtype": None}
        for line in Path(log_path).read_text(encoding="utf-8", errors="replace").splitlines():
            try:
                obj = json.loads(line)
            except ValueError:
                continue
            if obj.get("type") == "system" and obj.get("subtype") == "init":
                out.update(init_model=obj.get("model"), api_key_source=obj.get("apiKeySource"),
                           session_id=obj.get("session_id"))
            elif obj.get("type") == "assistant" and not obj.get("parent_tool_use_id"):
                model = (obj.get("message") or {}).get("model")
                if model and model != "<synthetic>" and model not in out["main_models"]:
                    out["main_models"].append(model)
            elif obj.get("type") == "result":
                out.update(subtype=obj.get("subtype"), is_error=bool(obj.get("is_error")),
                           structured_output=obj.get("structured_output"), result=obj.get("result"),
                           permission_denials=obj.get("permission_denials") or [],
                           model_usage=obj.get("modelUsage") or {}, cost_usd=obj.get("total_cost_usd"))
        return out


# --------------------------------------------------------------------------- lock

def _os_lock(handle):
    handle.seek(0)
    if os.name == "nt":
        import msvcrt
        msvcrt.locking(handle.fileno(), msvcrt.LK_NBLCK, 1)
    else:
        import fcntl
        fcntl.flock(handle.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)


def _os_unlock(handle):
    handle.seek(0)
    if os.name == "nt":
        import msvcrt
        msvcrt.locking(handle.fileno(), msvcrt.LK_UNLCK, 1)
    else:
        import fcntl
        fcntl.flock(handle.fileno(), fcntl.LOCK_UN)


class Lock:
    """Process-safe exclusive lock (OS byte-range/flock). The kernel releases it when
    the holder exits, so a crashed controller never leaves a stale claim behind."""

    def __init__(self, path: Path, command: str):
        self.path, self.command, self.handle = Path(path), command, None
        self.holder = self.path.with_name(self.path.name + ".holder.json")

    def __enter__(self):
        self.path.parent.mkdir(parents=True, exist_ok=True)
        handle = open(self.path, "a+", encoding="utf-8")
        try:
            _os_lock(handle)
        except OSError:
            handle.close()
            try:
                who = read_json(self.holder)
            except (ValueError, OSError):
                who = {}
            raise UsageError(f"locked by pid {who.get('pid')} on {who.get('host')} ({who.get('command')}, since "
                             f"{who.get('started')}); one controller writer per branch at a time")
        self.handle = handle
        atomic_write_json(self.holder, {"pid": os.getpid(), "host": socket.gethostname(), "started": now_iso(),
                                        "command": self.command})
        return self

    def __exit__(self, *exc):
        if self.handle:
            self.holder.unlink(missing_ok=True)
            _os_unlock(self.handle)
            self.handle.close()
            self.handle = None

    def held_elsewhere(self) -> bool:
        try:
            with self:
                return False
        except UsageError:
            return True


# --------------------------------------------------------------------------- controller

class Controller:
    def __init__(self, enrollment_dir, github=None, reviewer=None, developer=None, sleep=time.sleep,
                 workflow_root: Path = HERE):
        self.dir = Path(enrollment_dir)
        self.state_path = self.dir / "state.json"
        if not self.state_path.exists():
            raise UsageError(f"no enrollment at {self.dir}")
        self.wf = workflow_root
        self._gh, self._reviewer, self._developer = github, reviewer, developer
        self.sleep = sleep
        self.stop_path = self.dir / "STOP"
        self._viewer = None
        self.identity_ok = False
        self.reload()

    def reload(self):
        self.state = read_json(self.state_path)
        if self.state.get("schema_version") != STATE_SCHEMA:
            raise UsageError("unsupported enrollment state version")
        self._migrate()
        self.cfg = self.state["project"]
        self.git = Git(self.state["repo_root"])

    def _migrate(self):
        """Upgrade state written by the first controller-core candidate (active_round was a copy)."""
        s = self.state
        if isinstance(s.get("active_round"), dict):
            copy = s["active_round"]
            s["active_round"] = None if copy.get("status") in ("accepted", "historical", "superseded") else copy["round"]
        for key, default in (("archived", {}), ("archived_keys", []), ("archive_pending", None),
                             ("last_report_for", None), ("foreground_session", None)):
            s.setdefault(key, default)

    # -- lazy boundaries ---------------------------------------------------
    @property
    def gh(self):
        if self._gh is None:
            self._gh = GitHub(self.cfg["repository"], self.cfg.get("gh", "gh"))
        return self._gh

    @property
    def reviewer(self):
        if self._reviewer is None:
            self._reviewer = CodexReviewer(self.cfg["reviewer"])
        return self._reviewer

    @property
    def developer(self):
        if self._developer is None:
            self._developer = ClaudeDeveloper(self.cfg["developer"], self.dir / "developer-settings.json",
                                              self.wf / REPORT_SCHEMA_FILE)
        return self._developer

    # -- state ---------------------------------------------------------------
    def save(self):
        self.state["updated_at"] = now_iso()
        atomic_write_json(self.state_path, self.state)

    def transition(self, to, why, save=True):
        self.state.setdefault("transitions", []).append(
            {"at": now_iso(), "from": self.state.get("phase"), "to": to, "why": why})
        self.state["phase"] = to
        if save:
            self.save()

    @property
    def checkpoint(self):
        plan = self.state["plan"]["checkpoints"]
        return plan[min(self.state["checkpoint_index"], len(plan) - 1)]

    @property
    def limits(self):
        return self.state["limits"]

    def lock(self, command):
        key = branch_key(self.cfg["repository"], self.state["branch"])
        return Lock(agent_home() / "locks" / f"{key}.lock", command)

    def active(self):
        n = self.state.get("active_round")
        return self.state["rounds"][n - 1] if n else None

    def last_round_no(self):
        return self.state["rounds"][-1]["round"] if self.state.get("rounds") else 0

    # -- identity checks -----------------------------------------------------
    def viewer(self):
        if self._viewer is None:
            self._viewer = self.gh.viewer()
        return self._viewer

    def verify_inputs(self):
        """Pre-mutation guard: target identity first, then workflow version and mission."""
        if not same_path(self.git.toplevel(), self.state["repo_root"]):
            raise UsageError("developer worktree root moved")
        if self.git.remote_url() not in self.cfg["allowed_remote_urls"]:
            raise UsageError("origin is not an approved remote for this project")
        if self.viewer() != self.cfg["expected_actor"]:
            raise UsageError(f"GitHub actor {self.viewer()} is not the expected {self.cfg['expected_actor']}")
        pr = self.pr_view()
        self.identity_ok = True
        if self.git.branch() != self.state["branch"]:
            raise Stop("PAUSED", f"worktree is on {self.git.branch()}, not the enrolled {self.state['branch']}")
        fingerprint = workflow_fingerprint(self.wf)
        if fingerprint != self.state["workflow"]["fingerprint"]:
            raise Stop("PAUSED", f"workflow version changed ({short(self.state['workflow']['fingerprint'])} -> "
                                 f"{short(fingerprint)}); review it, then run `activate`")
        problem = self.mission_problem()
        if problem:
            raise Stop("BLOCKED", problem)
        settings = self.dir / "developer-settings.json"
        if self.state.get("developer_settings_sha256") and (
                not settings.exists() or sha256_file(settings) != self.state["developer_settings_sha256"]):
            raise Stop("BLOCKED", "the enrollment's developer permission file is missing or changed")
        return pr

    def mission_problem(self):
        mission = self.state["mission"]
        snapshot = Path(mission["snapshot"])
        if not snapshot.exists() or sha256_file(snapshot) != mission["sha256"]:
            return "the mission snapshot passed to the agents is missing or changed"
        if Path(mission["path"]).exists() and sha256_file(mission["path"]) != mission["sha256"]:
            return "the enrolled mission file changed; mission changes need an owner decision"
        return None

    def pr_view(self):
        number = self.state["pr"]["number"]
        pr = self.gh.pr(number)
        owner, name = self.cfg["repository"].split("/")
        if pr.get("number") != number or not pr["url"].endswith(f"/{self.cfg['repository']}/pull/{number}"):
            raise UsageError("PR does not belong to the enrolled repository and number")
        if pr.get("isCrossRepository") or (pr.get("headRepositoryOwner") or {}).get("login") != owner or \
                (pr.get("headRepository") or {}).get("name") != name:
            raise UsageError("PR head is not in the enrolled repository (cross-repository PRs are refused)")
        if pr["headRefName"] != self.state["branch"] or pr["baseRefName"] != self.state["target"]:
            raise UsageError("PR head/base do not match the enrollment")
        if pr["state"] != "OPEN":
            raise Stop("PAUSED", f"PR #{number} is {pr['state']}")
        if not pr["isDraft"]:
            raise Stop("PAUSED", f"PR #{number} is no longer a draft; the workflow only operates on drafts")
        return pr

    def assert_no_active_developer(self):
        dev = self.state["developer"]
        if pid_alive(dev.get("active_pid")):
            raise Stop("PAUSED", f"developer session {dev['session_id']} is still running (pid {dev['active_pid']})")

    def capture_candidate(self):
        self.assert_no_active_developer()
        if not self.git.clean():
            raise Stop("PAUSED", "developer worktree has uncommitted changes; commit and push a coherent checkpoint")
        head = self.git.head()
        remote = self.git.remote_head(self.state["branch"])
        if remote != head:
            raise Stop("PAUSED", f"candidate {short(head)} is not the pushed branch head ({short(remote)}); "
                                 "push through the guarded wrapper first")
        pr = None
        for _ in range(6):
            pr = self.pr_view()
            if pr["headRefOid"] == head:
                break
            self.sleep(5)
        if pr["headRefOid"] != head:
            raise Stop("PAUSED", f"PR head {short(pr['headRefOid'])} does not match candidate {short(head)}")
        base = self.git.fetch(self.state["target"])
        return {"H": head, "B": base, "M": self.git("merge-base", head, base)}

    def freshness(self, rnd):
        """Reasons the round's comparison inputs are no longer current (empty = fresh)."""
        reasons = []
        pr = self.pr_view()
        if pr["headRefOid"] != rnd["H"]:
            reasons.append(f"PR head is {short(pr['headRefOid'])}, not {short(rnd['H'])}")
        remote = self.git.remote_head(self.state["branch"])
        if remote != rnd["H"]:
            reasons.append(f"branch head is {short(remote)}, not {short(rnd['H'])}")
        base = self.git.fetch(self.state["target"])
        merge_base = self.git("merge-base", rnd["H"], base)
        if merge_base != rnd["M"]:
            reasons.append(f"merge base moved {short(rnd['M'])} -> {short(merge_base)}")
        elif base != rnd["B"]:
            rnd["B_at_acceptance"] = base  # recorded reconciliation: the reviewed diff M..H is unchanged
        return reasons

    def verify_transcripts(self, commit):
        """Records the controller archived must be intact; no other records may appear."""
        folder = f"{self.cfg['transcript_dir'].strip('/')}/pr-{self.state['pr']['number']}"
        archived = self.state.get("archived", {})
        for rel, digest in archived.items():
            if self.git.blob_sha256(commit, rel) != digest:
                raise Stop("BLOCKED", f"archived record {rel} was modified or removed in {short(commit)}")
        foreign = [p for p in self.git.tree_files(commit, folder) if p not in archived]
        if foreign:
            raise Stop("BLOCKED", "unapproved transcript records in the candidate: " + ", ".join(foreign[:5]))

    # -- publication -----------------------------------------------------------
    def marker(self, key, index, total, digest):
        return f"<!-- agent-review:v1 enrollment={self.state['enrollment_id']} key={key} part={index}/{total} sha256={digest} -->"

    def publish(self, key, body):
        pubs = self.state.setdefault("publications", {})
        record = pubs.get(key)
        if record is None:
            text, redactions = sanitize(body)
            chunks = [text[i:i + COMMENT_LIMIT] for i in range(0, len(text), COMMENT_LIMIT)] or [""]
            parts = []
            for index, chunk in enumerate(chunks, 1):
                if len(chunks) > 1:
                    chunk = f"_Part {index} of {len(chunks)}._\n\n{chunk}"
                digest = sha256_text(chunk)
                marker = self.marker(key, index, len(chunks), digest)
                path = self.dir / "outbox" / f"{key}-part{index}.md"
                write_text(path, f"{marker}\n{chunk}")
                parts.append({"marker": marker, "body_file": str(path), "sha256": digest, "comment_id": None,
                              "url": None})
            record = {"key": key, "parts": parts, "state": "pending", "redactions": redactions,
                      "created_at": now_iso(), "seq": len(pubs) + 1}
            pubs[key] = record
            self.save()  # persist the outgoing message before sending
        if record["state"] == "posted":
            return record
        number = self.state["pr"]["number"]
        for part in record["parts"]:
            if part["comment_id"]:
                continue
            body = Path(part["body_file"]).read_text(encoding="utf-8")
            attempts, found = 0, None
            while not found:
                attempts += 1
                try:
                    found = self.reconcile(number, part["marker"], body)
                    if not found:
                        found = self.gh.post_comment(number, part["body_file"])
                except Stop:
                    raise
                except Exception as exc:  # lost response, listing failure or transient error
                    if attempts >= self.limits["publication_attempts"]:
                        raise Stop("PAUSED", f"publication of {key} failed ({exc}); the preserved message will be "
                                             "reconciled and sent on resume")
                    self.sleep(self.limits["retry_backoff_seconds"][min(attempts - 1, 1)])
            part["comment_id"], part["url"] = found["id"], found["url"]
            self.save()
        record["state"], record["posted_at"] = "posted", now_iso()
        self.save()
        return record

    def reconcile(self, number, marker, body):
        """Find our already-posted comment: expected actor, exact marker and identical body."""
        matches = [c for c in self.gh.comments(number)
                   if c.get("user") == self.cfg["expected_actor"] and (c.get("body") or "").startswith(marker)]
        exact = [c for c in matches if normalize_body(c["body"]) == normalize_body(body)]
        if len(exact) == 1:
            return {"id": exact[0]["id"], "url": exact[0]["url"]}
        if matches:
            raise Stop("PAUSED", f"PR comment(s) {[c['id'] for c in matches]} carry the marker for this message but "
                                 "their bodies differ or are duplicated; resolve manually before publishing")
        return None

    def publish_status(self, status, reason):
        if not self.identity_ok:
            return  # never publish to a target whose identity was not established
        key = f"status-{len(self.state.get('transitions', []))}-{status.lower()}"
        cp = self.checkpoint
        body = (f"### Workflow — {status.lower().replace('_', ' ')} — checkpoint {cp['id']}\n\n"
                f"{reason}\n\n"
                f"- Enrollment: `{self.state['enrollment_id']}`\n"
                f"- Phase: `{self.state.get('phase')}`; rounds completed for this checkpoint: "
                f"{self.state['rounds_completed'].get(cp['id'], 0)}/{self.limits['max_review_rounds_per_checkpoint']}\n"
                f"- Inspect: `python C:/Dev/agent-review/agent_review.py status --enrollment {self.state['enrollment_id']}`\n\n"
                f"_Posted by the agent-review controller through the owner's GitHub account. Automated status; "
                f"not a human review._\n")
        try:
            self.publish(key, body)
        except Exception as exc:  # status visibility is best effort; state still records it
            self.state.setdefault("notes", []).append(f"{now_iso()} status publication failed: {exc}")
            self.save()

    # -- developer reports -----------------------------------------------------
    def verify_foreground_developer(self):
        """Bind a foreground report to the enrolled interactive Claude session's recorded metadata."""
        expected = self.state.get("foreground_session")
        current = os.environ.get("CLAUDE_CODE_SESSION_ID")
        if not expected or current != expected:
            raise UsageError(f"foreground reports must come from the enrolled Claude session {expected}; "
                             f"this process reports session {current}")
        prov = self.developer.session_provenance(expected)
        allowed = re.compile(self.cfg["developer"]["allowed_model_pattern"])
        if not prov["found"] or not prov["models"] or any(not allowed.match(m) for m in prov["models"]):
            raise Stop("BLOCKED", f"foreground session models {prov['models']} are not all "
                                  f"{self.cfg['developer']['model']} (transcript found: {prov['found']})")
        if prov["efforts"] != [self.cfg["developer"]["effort"]]:
            raise Stop("BLOCKED", f"foreground session effort {prov['efforts']} != {self.cfg['developer']['effort']}")
        return prov

    def accept_report(self, report):
        """Validate a developer report; returns an error string for the developer, or None."""
        errors = validate_schema(report, read_json(self.wf / REPORT_SCHEMA_FILE))
        if errors:
            return "report does not match agent-review.developer.v1: " + "; ".join(errors[:8])
        cp = self.checkpoint
        if report["checkpoint_id"] != cp["id"]:
            return f"report is for checkpoint {report['checkpoint_id']}, but the active checkpoint is {cp['id']}"
        provenance = self.verify_foreground_developer() if self.state["mode"] == "foreground" else \
            (self.state["developer"].get("log") or [{}])[-1]
        self.state.setdefault("reports", []).append({"at": now_iso(), "report": report})
        if report["status"] != "checkpoint_ready" or report["runtime_restoration"] == "pending":
            self.state["last_report"] = report  # stops are remembered; ready reports only once accepted
            self.state["last_report_for"] = None
        self.save()
        if report["runtime_restoration"] == "pending":
            raise Stop("PAUSED", "developer reports a runtime restoration still pending; it must finish first")
        if report["status"] == "blocked":
            self.publish_dispositions(report)
            raise Stop("BLOCKED", f"developer blocker: {report['blocker'] or report['summary']}")
        if report["status"] == "needs_owner_decision":
            self.publish_dispositions(report)
            raise Stop("PAUSED", f"owner decision required: {report['owner_decision_needed'] or report['summary']}")
        if report["status"] == "paused":
            raise Stop("PAUSED", f"developer paused: {report['summary']}")
        head = self.git.head()
        if report["candidate_commit"] != head:
            return f"candidate_commit {report['candidate_commit']} is not the worktree HEAD {head}"
        failing = [v["command"] for v in report["validation"] if v["required"] and v["result"] != "pass"]
        if failing:
            return "required validation is not passing: " + "; ".join(failing) + " (fix it or report blocked)"
        pending = set(self.state.get("pending_findings", []))
        ledger = self.state.get("findings", {})
        disposed = [d["finding_id"] for d in report["dispositions"]]
        if len(disposed) != len(set(disposed)):
            return "duplicate dispositions for the same finding"
        if pending - set(disposed):
            return "missing dispositions for findings: " + ", ".join(sorted(pending - set(disposed)))
        unknown = set(disposed) - set(ledger)
        if unknown:
            return "dispositions reference unknown findings: " + ", ".join(sorted(unknown))
        for item in report["dispositions"]:
            entry = ledger[item["finding_id"]]
            if item["disposition"] == "deferred_optional" and entry["blocks_readiness"]:
                return f"{item['finding_id']} blocks readiness and cannot be deferred as optional"
            if item["disposition"] == "fixed":
                if not item["commits"]:
                    return f"{item['finding_id']} is marked fixed without a commit"
                for commit in item["commits"]:
                    if not self.git.ok("cat-file", "-e", f"{commit}^{{commit}}") or not self.git.is_ancestor(commit, head):
                        return f"{item['finding_id']} cites {commit}, which is not in the candidate history"
        for item in report["dispositions"]:
            ledger[item["finding_id"]].setdefault("dispositions", []).append({"round": self.last_round_no(), **item})
        self.state.update(last_report=report, last_report_for=head, last_report_provenance=provenance)
        self.save()
        self.publish_dispositions(report)
        if any(d["disposition"] == "unresolved_blocker" for d in report["dispositions"]):
            raise Stop("BLOCKED", "developer recorded an unresolved blocker; see the disposition comment")
        return None

    def publish_dispositions(self, report):
        if not report["dispositions"]:
            return
        n = self.last_round_no()
        lines = [f"### Claude Opus — finding dispositions — round {n}\n",
                 f"Candidate after corrections: `{report['candidate_commit']}`\n"]
        for d in report["dispositions"]:
            commits = ", ".join(f"`{short(c)}`" for c in d["commits"]) or "—"
            lines.append(f"- **{d['finding_id']}** — `{d['disposition']}` — commits: {commits}\n  {d['evidence']}")
        lines.append(self.validation_md(report))
        lines.append(f"\n_Model-generated developer response (Claude {self.cfg['developer']['model']} / "
                     f"{self.cfg['developer']['effort']}), posted by the agent-review controller through the owner's "
                     f"GitHub account; not a human review._\n")
        key, pubs, suffix = f"r{n:02d}-dispositions", self.state.get("publications", {}), 1
        while key in pubs and pubs[key]["state"] == "posted":  # a resubmission gets its own visible comment
            suffix += 1
            key = f"r{n:02d}-dispositions-{suffix}"
        self.publish(key, "\n".join(lines))

    @staticmethod
    def validation_md(report):
        if not report["validation"]:
            return "\n**Validation:** none reported."
        rows = ["\n**Validation (developer-reported):**\n", "| Command | Required | Result | Duration | Scope and limits |",
                "|---|---|---|---|---|"]
        for v in report["validation"]:
            cells = [v["command"], "yes" if v["required"] else "no", v["result"], v["duration"], v["scope_and_limits"]]
            rows.append("| " + " | ".join(c.replace("|", "\\|").replace("\n", " ") for c in cells) + " |")
        return "\n".join(rows)

    # -- review round ----------------------------------------------------------
    def review_round(self):
        """Run (or resume) one review round. Returns the decision, or 'needs_report'."""
        self.check_stop_request()
        cp = self.checkpoint
        rnd = self.active()
        if rnd and rnd["status"] == "validated":
            return self.finish_round(rnd)  # publish a preserved result without rerunning a successful review
        completed = self.state["rounds_completed"].get(cp["id"], 0)
        if completed >= self.limits["max_review_rounds_per_checkpoint"]:
            raise Stop("BLOCKED", f"review-round cap reached for {cp['id']} "
                                  f"({completed}/{self.limits['max_review_rounds_per_checkpoint']})")
        candidate = self.capture_candidate()
        self.verify_transcripts(candidate["H"])
        reuse = (rnd and rnd["status"] in ("requested", "reviewing") and rnd["H"] == candidate["H"]
                 and rnd["M"] == candidate["M"] and rnd["checkpoint_id"] == cp["id"])
        if not reuse:
            if rnd:
                rnd["status"] = "superseded"
                self.state["active_round"] = None
            if self.state.get("last_report_for") != candidate["H"]:
                self.state["develop_context"] = "needs_report"
                self.transition("develop", f"no accepted developer report for candidate {short(candidate['H'])}")
                return "needs_report"
            number = len(self.state["rounds"]) + 1
            rnd = {"round": number, "request_id": f"{self.state['enrollment_id']}-r{number:02d}-{uuid.uuid4().hex[:6]}",
                   "checkpoint_id": cp["id"], "checkpoint_type": cp["type"], **candidate,
                   "pending_findings": list(self.state.get("pending_findings", [])),
                   "report": self.state["last_report"], "developer_provenance": self.state.get("last_report_provenance"),
                   "evidence_since": self.state["rounds"][-1]["created_at"] if self.state["rounds"] else self.state["created_at"],
                   "created_at": now_iso(), "status": "requested", "attempts": [], "attempt_seq": 0}
            self.state["rounds"].append(rnd)
            self.state["active_round"] = number
            self.save()
        elif rnd["B"] != candidate["B"]:
            rnd.setdefault("B_history", []).append(rnd["B"])
            rnd["B"] = candidate["B"]  # same M: the reviewed diff is unchanged; record the newer target tip
        self.publish(f"r{rnd['round']:02d}-request", self.request_body(rnd))
        rnd_dir = self.dir / "rounds" / f"r{rnd['round']:02d}"
        rnd_dir.mkdir(parents=True, exist_ok=True)
        rnd["status"] = "reviewing"
        self.save()
        envelope = None
        while len(rnd["attempts"]) < self.limits["max_review_attempts_per_round"]:
            self.check_stop_request()
            envelope = self.review_attempt(rnd, rnd_dir)
            if envelope["ok"]:
                break
            if envelope["fatal"]:
                raise Stop("BLOCKED", "review rejected: " + "; ".join(envelope["reasons"]) +
                           " (no model downgrade is attempted)")
            if len(rnd["attempts"]) < self.limits["max_review_attempts_per_round"]:
                self.sleep(self.limits["retry_backoff_seconds"][min(len(rnd["attempts"]) - 1, 1)])
        if not envelope or not envelope["ok"]:
            raise Stop("BLOCKED", f"no valid review after {len(rnd['attempts'])} attempts: " +
                       "; ".join(envelope["reasons"] if envelope else []))
        rnd["status"], rnd["envelope_path"] = "validated", envelope["path"]
        self.save()
        return self.finish_round(rnd)

    def review_attempt(self, rnd, rnd_dir):
        rnd["attempt_seq"] = n = rnd.get("attempt_seq", 0) + 1  # monotonic across owner-granted budgets
        attempt = {"n": n, "started": now_iso()}
        rnd["attempts"].append(attempt)
        self.save()
        snapshot = rnd_dir / f"snapshot-{n}"
        if snapshot.exists():
            self.remove_snapshot(snapshot)
        self.git("-c", "core.longpaths=true", "worktree", "add", "--detach", str(snapshot), rnd["H"])
        try:
            inputs = self.write_review_inputs(rnd, rnd_dir, snapshot)
            schema = rnd_dir / "review-schema.json"
            atomic_write_json(schema, strict_schema_copy(read_json(self.wf / REVIEW_SCHEMA_FILE)))
            result_path = rnd_dir / f"result-{n}.json"
            prompt = self.render("reviewer.md", self.reviewer_fields(rnd, inputs, snapshot))
            write_text(rnd_dir / f"prompt-{n}.md", prompt)
            info = self.reviewer.run(prompt, snapshot, schema, result_path, rnd_dir / f"events-{n}.jsonl",
                                     rnd_dir / f"stderr-{n}.txt", self.cfg["reviewer"]["timeout_minutes"] * 60)
        finally:
            self.remove_snapshot(snapshot)
        prov = self.reviewer.provenance(info.get("thread_id"))
        envelope = self.validate_review(rnd, result_path, info, prov, rnd_dir / f"stderr-{n}.txt")
        envelope["path"] = str(rnd_dir / f"envelope-{n}.json")
        atomic_write_json(envelope["path"], envelope)
        attempt.update({"ended": now_iso(), "ok": envelope["ok"], "fatal": envelope["fatal"],
                        "reasons": envelope["reasons"], "thread_id": info.get("thread_id")})
        self.save()
        return envelope

    def remove_snapshot(self, snapshot: Path):
        if not str(snapshot.resolve()).startswith(str(self.dir.resolve())):
            raise UsageError("refusing to remove a path outside the enrollment directory")
        if not self.git.ok("worktree", "remove", "--force", str(snapshot)):
            shutil.rmtree(snapshot, ignore_errors=True)
            self.git.ok("worktree", "prune")

    def write_review_inputs(self, rnd, rnd_dir, snapshot):
        exclude = f":(exclude){self.cfg['transcript_dir'].strip('/')}"
        diff = self.git("diff", "--no-ext-diff", rnd["M"], rnd["H"], "--", ".", exclude)
        names = self.git("diff", "--name-status", "--no-renames", rnd["M"], rnd["H"], "--", ".", exclude)
        write_text(rnd_dir / "feature.diff", diff + "\n")
        write_text(rnd_dir / "changed-files.txt", names + "\n")
        atomic_write_json(rnd_dir / "prior-findings.json", {"pending_for_assessment": rnd["pending_findings"],
                                                            "ledger": self.state.get("findings", {})})
        atomic_write_json(rnd_dir / "developer-report.json", {"report": rnd["report"],
                                                              "developer_provenance": rnd.get("developer_provenance")})
        atomic_write_json(rnd_dir / "request.json", {k: rnd[k] for k in (
            "round", "request_id", "checkpoint_id", "checkpoint_type", "H", "B", "M")})
        session = self.state.get("foreground_session") if self.state["mode"] == "foreground" \
            else self.state["developer"]["session_id"]
        transcript, _ = sanitize(self.developer.tool_transcript(session, rnd.get("evidence_since") or self.state["created_at"]),
                                 strict=False)
        write_text(rnd_dir / "developer-tool-transcript.md", transcript)
        # The reviewer's sandbox is always granted its workspace, but not necessarily other paths
        # (e.g. owner-only ACLs). Give it copies inside the snapshot; they are not part of the candidate.
        inbox = snapshot / ".agent-review-inputs"
        inbox.mkdir(exist_ok=True)
        for name in ("feature.diff", "changed-files.txt", "prior-findings.json", "developer-report.json", "request.json",
                     "developer-tool-transcript.md"):
            shutil.copy2(rnd_dir / name, inbox / name)
        shutil.copy2(self.state["mission"]["snapshot"], inbox / ("mission" + Path(self.state["mission"]["snapshot"]).suffix))
        return {"diff": inbox / "feature.diff", "names": inbox / "changed-files.txt",
                "prior": inbox / "prior-findings.json", "report": inbox / "developer-report.json",
                "mission": inbox / ("mission" + Path(self.state["mission"]["snapshot"]).suffix),
                "diff_lines": diff.count("\n") + 1}

    def validate_review(self, rnd, result_path, info, prov, stderr_path):
        reasons, fatal = [], False
        requested = self.reviewer.requested
        result, parsed, raw = None, False, b""
        if info.get("timed_out"):
            reasons.append("reviewer timed out")
        if info.get("exit_code") != 0:
            reasons.append(f"reviewer process exited {info.get('exit_code')}")
        stderr = Path(stderr_path).read_text(encoding="utf-8", errors="replace") if Path(stderr_path).exists() else ""
        if USAGE_LIMIT_RE.search(stderr):
            reasons.append("reviewer reported a usage limit or model availability error")
            fatal = True
        if Path(result_path).exists():
            raw = Path(result_path).read_bytes()
        if not raw.strip():
            reasons.append("no final structured message")
        else:
            try:
                result, parsed = json.loads(raw.decode("utf-8")), True
            except ValueError as exc:
                reasons.append(f"malformed JSON: {exc}")
        if parsed:
            errors = validate_schema(result, read_json(self.wf / REVIEW_SCHEMA_FILE))
            reasons += [f"schema: {e}" for e in errors[:12]]
            if not errors:
                reasons += self.semantic_problems(rnd, result)
        if not prov["rollout_found"]:
            reasons.append("reviewer provenance unavailable (no session rollout for the thread)")
        else:
            if prov["observed_model"] != requested["model"] or any(m != requested["model"] for m in prov["turn_models"]):
                reasons.append(f"wrong reviewer model: observed {prov['observed_model']} {prov['turn_models']}, "
                               f"required {requested['model']}")
                fatal = True
            if prov["observed_effort"] != requested["effort"] or any(e != requested["effort"] for e in prov["turn_efforts"]):
                reasons.append(f"reviewer effort {prov['observed_effort']} {prov['turn_efforts']} != "
                               f"required {requested['effort']}")
                fatal = True
            if prov["sandbox"] not in (None, "read-only") or prov["approval_policy"] not in (None, "never"):
                reasons.append(f"reviewer sandbox/approval not read-only/never: {prov['sandbox']}/{prov['approval_policy']}")
                fatal = True
            if prov["subagent_suspected"]:
                reasons.append("reviewer attempted to spawn sub-agents whose model cannot be verified")
        return {"protocol": PROTOCOL, "ok": not reasons, "fatal": fatal, "reasons": reasons,
                "request_id": rnd["request_id"], "round": rnd["round"], "checkpoint_id": rnd["checkpoint_id"],
                "checkpoint_type": rnd["checkpoint_type"], "repository": self.cfg["repository"],
                "pr": self.state["pr"]["number"], "H": rnd["H"], "B": rnd["B"], "M": rnd["M"],
                "mission": {"id": self.state["plan"]["mission_id"], "sha256": self.state["mission"]["sha256"]},
                "workflow_fingerprint": self.state["workflow"]["fingerprint"],
                "validated_at": now_iso(), "invocation": {"requested": requested, "observed": prov, **info},
                "response_sha256": sha256_bytes(raw) if raw else None, "result": result if not reasons else None,
                "raw_result_if_rejected": raw.decode("utf-8", "replace")[:4000] if reasons else None}

    def semantic_problems(self, rnd, result):
        reasons = []
        for key, ours in (("request_id", rnd["request_id"]), ("round", rnd["round"]),
                          ("checkpoint_id", rnd["checkpoint_id"]), ("checkpoint_type", rnd["checkpoint_type"]),
                          ("candidate_commit", rnd["H"])):
            if result[key] != ours:
                reasons.append(f"echoed {key} {result[key]!r} does not match controller value {ours!r}")
        decision = result["decision"]
        passing = decision in ("checkpoint_passed", "ready_for_owner_review")
        if decision not in DECISIONS_BY_TYPE[rnd["checkpoint_type"]]:
            reasons.append(f"decision {decision} is not valid for a {rnd['checkpoint_type']} checkpoint")
        blocking = [f for f in result["findings"] if f["blocks_readiness"]]
        if passing and blocking:
            reasons.append("a passing decision lists findings that block readiness")
        if passing and result["evidence_assessment"]["required_gaps"]:
            reasons.append("a passing decision lists required evidence gaps")
        if decision == "changes_required" and not blocking:
            reasons.append("changes_required without any finding that blocks readiness")
        ids = [f["id"] for f in result["findings"]]
        if len(ids) != len(set(ids)):
            reasons.append("duplicate finding IDs")
        ledger = self.state.get("findings", {})
        pending = set(rnd["pending_findings"])
        assessed = [a["finding_id"] for a in result["prior_finding_assessments"]]
        if len(assessed) != len(set(assessed)):
            reasons.append("duplicate prior-finding assessments")
        if pending - set(assessed):
            reasons.append("prior findings not assessed: " + ", ".join(sorted(pending - set(assessed))))
        if set(assessed) - pending:
            reasons.append("assessments for findings that were not pending: " + ", ".join(sorted(set(assessed) - pending)))
        open_after = {fid for fid, e in ledger.items() if e["status"] == "open"} | set(ids)
        for a in result["prior_finding_assessments"]:
            entry = ledger.get(a["finding_id"])
            if not entry:
                continue
            last = (entry.get("dispositions") or [{}])[-1].get("disposition")
            if a["assessment"] not in COMPATIBLE_ASSESSMENTS[last]:
                reasons.append(f"{a['finding_id']}: assessment {a['assessment']} does not answer disposition {last}")
            if a["assessment"] == "deferral_accepted" and entry["blocks_readiness"]:
                reasons.append(f"{a['finding_id']}: a finding that blocks readiness cannot be resolved by deferral")
            if a["assessment"] in REJECTING_ASSESSMENTS and a["finding_id"] not in ids:
                reasons.append(f"{a['finding_id']}: rejected/open prior finding must be restated in findings")
            if a["assessment"] in POSITIVE_ASSESSMENTS and a["finding_id"] not in ids:
                open_after.discard(a["finding_id"])
        blocking_after = [fid for fid in open_after
                          if (ledger.get(fid) or {}).get("blocks_readiness") or fid in {f["id"] for f in blocking}]
        if passing and blocking_after:
            reasons.append("blocking findings would remain open after this passing decision: " + ", ".join(sorted(blocking_after)))
        return reasons

    def finish_round(self, rnd):
        """Accept a validated round: freshness re-check, publication, then one atomic state update."""
        stale = self.freshness(rnd)
        envelope = read_json(rnd["envelope_path"])
        if stale:
            rnd["status"], rnd["stale_reasons"] = "historical", stale
            self.state["active_round"] = None
            self.state["develop_context"] = "needs_report"
            self.transition("develop", "review became historical: " + "; ".join(stale))
            self.publish(f"r{rnd['round']:02d}-review", self.review_body(rnd, envelope, stale=stale))
            raise Stop("PAUSED", f"the review of {short(rnd['H'])} is historical ({'; '.join(stale)}); only the "
                                 "enrolled writer may push. Reconcile, then submit a report for the current candidate")
        result = envelope["result"]
        record = self.publish(f"r{rnd['round']:02d}-review", self.review_body(rnd, envelope))
        envelope["published"] = [{"comment_id": p["comment_id"], "url": p["url"]} for p in record["parts"]]
        atomic_write_json(rnd["envelope_path"], envelope)
        cp = self.checkpoint
        ledger = self.state.setdefault("findings", {})
        for a in result["prior_finding_assessments"]:
            entry = ledger[a["finding_id"]]
            entry.setdefault("assessments", []).append({"round": rnd["round"], **a})
            entry["status"] = "resolved" if a["assessment"] in POSITIVE_ASSESSMENTS else "open"
        for f in result["findings"]:
            entry = ledger.setdefault(f["id"], {"first_round": rnd["round"], "rounds_open": 0})
            entry.update({k: f[k] for k in ("title", "priority", "category", "blocks_readiness")})
            entry.update(status="open", last_round=rnd["round"])
        for entry in ledger.values():
            entry["rounds_open"] = entry.get("rounds_open", 0) + 1 if entry["status"] == "open" else 0
        open_ids = sorted(fid for fid, e in ledger.items() if e["status"] == "open")
        decision = result["decision"]
        rnd.update(status="accepted", decision=decision, open_findings=open_ids,
                   response_sha256=envelope["response_sha256"], accepted_at=now_iso())
        self.state["rounds_completed"][cp["id"]] = self.state["rounds_completed"].get(cp["id"], 0) + 1
        self.state.update(active_round=None, pending_findings=open_ids)
        stop = None
        if decision == "changes_required":
            previous = [r for r in self.state["rounds"] if r.get("status") == "accepted" and r is not rnd
                        and r["checkpoint_id"] == rnd["checkpoint_id"]]
            blocking_now = sorted(fid for fid in open_ids if ledger[fid]["blocks_readiness"])
            stuck = [fid for fid in blocking_now if ledger[fid]["rounds_open"] >= self.limits["no_progress_rounds"]]
            if previous and previous[-1]["H"] == rnd["H"] and sorted(
                    fid for fid in previous[-1].get("open_findings", []) if ledger[fid]["blocks_readiness"]) == blocking_now:
                stop = Stop("BLOCKED", "no progress: the same blocking findings remain on an unchanged candidate")
            elif stuck:
                stop = Stop("BLOCKED", f"no progress: {', '.join(stuck)} open for "
                                       f"{self.limits['no_progress_rounds']} consecutive reviews")
            self.state["develop_context"] = "findings"
            self.transition("develop", f"round {rnd['round']} requires changes", save=False)
        elif decision == "checkpoint_passed":
            self.state["checkpoint_index"] += 1
            self.state["develop_context"] = "advanced"
            self.transition("develop", f"checkpoint {cp['id']} passed; advancing to {self.checkpoint['id']}", save=False)
        elif decision == "ready_for_owner_review":
            self.state["ready_review"] = {"round": rnd["round"], "H": rnd["H"], "B": rnd.get("B_at_acceptance", rnd["B"]),
                                          "M": rnd["M"], "summary": result["summary"]}
            self.transition("finalize", f"round {rnd['round']} ready for owner review", save=False)
        else:
            stop = Stop("BLOCKED", f"reviewer blocked: {result['decision_rationale']}")
        self.save()  # one write: acceptance, ledger, counters, next phase
        if stop:
            raise stop
        return decision

    # -- bodies ------------------------------------------------------------------
    def provenance_line(self, prov):
        if not prov:
            return "not recorded"
        if "models" in prov:
            return (f"foreground session `{prov.get('session_id')}`: models {prov['models']}, effort {prov['efforts']} "
                    "(CLI-recorded transcript)")
        return (f"headless session `{self.state['developer']['session_id']}`: init `{prov.get('init_model')}`, "
                f"API models {prov.get('main_models')}, effort {prov.get('observed_effort')}")

    def request_body(self, rnd):
        report = rnd.get("report") or {}
        rev = self.cfg["reviewer"]
        lines = [f"### Claude Opus — review request — round {rnd['round']} — candidate `{short(rnd['H'])}`\n",
                 "| Field | Value |", "|---|---|",
                 f"| Request ID | `{rnd['request_id']}` |",
                 f"| Checkpoint | `{rnd['checkpoint_id']}` ({rnd['checkpoint_type']}) |",
                 f"| Candidate H | `{rnd['H']}` |", f"| Target B (`{self.state['target']}`) | `{rnd['B']}` |",
                 f"| Merge base M | `{rnd['M']}` |",
                 f"| Mission | `{self.state['plan']['mission_id']}` sha256 `{short(self.state['mission']['sha256'])}` |",
                 f"| Workflow | `{short(self.state['workflow']['fingerprint'])}` |",
                 f"| Developer observed | {self.provenance_line(rnd.get('developer_provenance'))} |",
                 f"| Reviewer requested | Codex `{rev['model']}`, effort `{rev['effort']}`, read-only sandbox |",
                 f"| Round budget | {self.state['rounds_completed'].get(rnd['checkpoint_id'], 0) + 1} of "
                 f"{self.limits['max_review_rounds_per_checkpoint']} |",
                 "", f"**Scope:** {self.checkpoint['scope']}", "",
                 "**Exit conditions:**", *[f"- {c}" for c in self.checkpoint["exit_conditions"]], ""]
        if report:
            lines += ["**Developer summary:**", "", report.get("summary", ""), self.validation_md(report)]
            if report.get("evidence_refs"):
                lines += ["", "**Evidence references:**", *[f"- {e}" for e in report["evidence_refs"]]]
        if rnd["pending_findings"]:
            lines += ["", "**Prior findings to assess:** " + ", ".join(f"`{f}`" for f in rnd["pending_findings"])]
        lines += ["", "_Posted by the agent-review controller through the owner's GitHub account on behalf of the "
                      "Claude Opus developer. Automated request; not a human review._"]
        return "\n".join(lines) + "\n"

    def review_body(self, rnd, envelope, stale=None):
        result = envelope["result"]
        prov = envelope["invocation"]["observed"]
        heading = (f"### Codex GPT-6.2 Astra — review — round {rnd['round']} — candidate `{short(rnd['H'])}`"
                   + (" — HISTORICAL" if stale else ""))
        lines = [heading, ""]
        if stale:
            lines += [f"**Historical:** comparison inputs changed before acceptance ({'; '.join(stale)}). "
                      "This review is preserved but is not a current verdict.", ""]
        lines += [f"**Decision:** `{result['decision']}` — {result['decision_rationale']}", ""]
        if result["findings"]:
            lines.append("#### Findings\n")
            for f in result["findings"]:
                loc = f["location"]["file"] or "—"
                if f["location"]["line"]:
                    loc += f":{f['location']['line']}"
                lines += [f"**{f['id']}** · {f['priority']} · {f['category']} · "
                          f"{'blocks readiness' if f['blocks_readiness'] else 'non-blocking'} — {f['title']}",
                          f"- Location: `{loc}`; acceptance: {', '.join(f['acceptance_ids']) or '—'}",
                          f"- Trigger: {f['trigger']}", f"- Expected: {f['expected']}", f"- Actual: {f['actual']}",
                          f"- Why it blocks / does not: {f['blocking_rationale']}",
                          f"- Requested correction: {f['requested_correction']}",
                          f"- Requested evidence: {f['requested_evidence']}", ""]
        else:
            lines.append("#### Findings\n\nNone.\n")
        if result["prior_finding_assessments"]:
            lines.append("#### Prior finding assessments\n")
            lines += [f"- **{a['finding_id']}** — `{a['assessment']}` — {a['rationale']}"
                      for a in result["prior_finding_assessments"]]
            lines.append("")
        ev = result["evidence_assessment"]
        lines.append("#### Evidence assessment\n")
        for label, key in (("Inspected", "inspected"), ("Executed verification", "executed_verification"),
                           ("Source inspection only", "source_inspection_only"), ("Required gaps", "required_gaps"),
                           ("Limitations", "limitations")):
            lines.append(f"- **{label}:** " + ("; ".join(ev[key]) if ev[key] else "none"))
        s = result["summary"]
        lines += ["", "#### Summary\n", f"- **Delivered behavior:** {s['delivered_behavior']}",
                  f"- **Scope reviewed:** {s['scope_reviewed']}",
                  "- **Pending later-phase work:** " + ("; ".join(s["pending_later_phase_work"]) or "none"),
                  "- **Unresolved questions:** " + ("; ".join(s["unresolved_questions"]) or "none"),
                  "- **Remaining owner review:** " + ("; ".join(s["remaining_owner_review"]) or "none"), "",
                  "#### Provenance (controller-recorded)\n", "| | Requested | Observed |", "|---|---|---|",
                  f"| Model | `{envelope['invocation']['requested']['model']}` | `{prov['observed_model']}` (turns {prov['turn_models']}) |",
                  f"| Reasoning effort | `{envelope['invocation']['requested']['effort']}` | `{prov['observed_effort']}` (turns {prov['turn_efforts']}) |",
                  f"| Sandbox / approvals | read-only / never | `{prov['sandbox']}` / `{prov['approval_policy']}` |",
                  "", f"Codex CLI `{prov['cli_version']}`, thread `{envelope['invocation'].get('thread_id')}`, "
                      f"duration {envelope['invocation'].get('duration_s')}s, response sha256 "
                      f"`{envelope['response_sha256']}`, request `{rnd['request_id']}`. Observed values come from the "
                      "Codex session rollout's recorded thread settings, not from the model's own statements; the "
                      "serving backend's model identity is not separately exposed.", "",
                  "<details><summary>Complete structured review (agent-review.review.v1)</summary>\n",
                  "```json", json.dumps(result, indent=2), "```", "</details>", "",
                  "_Model-generated review by Codex using the requested model, posted by the agent-review controller "
                  "through the owner's GitHub account. Not a human review or approval._"]
        return "\n".join(lines) + "\n"

    # -- prompts -------------------------------------------------------------------
    def render(self, name, fields):
        template = (self.wf / "prompts" / name).read_text(encoding="utf-8")
        return string.Template(template).safe_substitute(fields)

    def common_fields(self):
        cp = self.checkpoint
        plan = self.state["plan"]
        later = [c["id"] + ": " + c["scope"] for c in plan["checkpoints"][self.state["checkpoint_index"] + 1:]]
        return {
            "repository": self.cfg["repository"], "pr_number": self.state["pr"]["number"],
            "pr_url": self.state["pr"]["url"], "branch": self.state["branch"], "target": self.state["target"],
            "mission_id": plan["mission_id"], "mission_title": plan.get("title", plan["mission_id"]),
            "mission_path": self.state["mission"]["snapshot"], "mission_sha256": self.state["mission"]["sha256"],
            "checkpoint_id": cp["id"], "checkpoint_type": cp["type"], "checkpoint_scope": cp["scope"],
            "exit_conditions": "\n".join(f"- {c}" for c in cp["exit_conditions"]),
            "later_checkpoints": "\n".join(f"- {c}" for c in later) or "- none (this is the final checkpoint)",
            "instructions": "\n".join(f"- {p}" for p in self.cfg.get("instructions", [])),
            "plan_notes": plan.get("notes", ""),
        }

    def reviewer_fields(self, rnd, inputs, snapshot):
        fields = self.common_fields()
        fields.update({"request_id": rnd["request_id"], "round": rnd["round"], "H": rnd["H"], "B": rnd["B"],
                       "M": rnd["M"], "snapshot": str(snapshot), "diff_path": str(inputs["diff"]),
                       "changed_files_path": str(inputs["names"]), "prior_findings_path": str(inputs["prior"]),
                       "developer_report_path": str(inputs["report"]), "diff_lines": inputs["diff_lines"],
                       "mission_path": str(inputs["mission"]),
                       "transcript_dir": self.cfg["transcript_dir"],
                       "pending_findings": ", ".join(rnd["pending_findings"]) or "none"})
        return fields

    # -- headless developer loop ---------------------------------------------------------
    def runtime_active(self):
        return any(Path(expand(p)).exists() for p in self.cfg.get("runtime_lock_paths", []))

    def develop_step(self, correction=None):
        dev = self.state["developer"]
        self.assert_no_active_developer()
        if dev["invocations"] >= self.limits["max_developer_invocations"]:
            raise Stop("BLOCKED", f"developer invocation budget exhausted ({dev['invocations']})")
        fields = self.common_fields()
        fields.update({"stop_file": str(self.stop_path), "worktree": self.state["repo_root"],
                       "push_command": " ".join(push_argv(self.cfg, self.wf, self.state["enrollment_id"]))
                       .replace("{repo_root}", self.state["repo_root"]).replace("{branch}", self.state["branch"]),
                       "transcript_dir": self.cfg["transcript_dir"], "correction": "",
                       "enrollment_id": self.state["enrollment_id"]})
        resume = self.developer.session_exists(dev["session_id"])
        context = self.state.get("develop_context")
        if correction:
            situation = f"The controller rejected your last report: {correction}. Fix that and report again."
        elif dev.get("launch") and not dev["launch"].get("returned"):
            situation = ("Your previous invocation was interrupted before it returned a report. Reconcile the worktree "
                         "(do not discard anything), complete or restore any runtime transaction, then continue.")
        elif context == "findings":
            n = self.last_round_no()
            situation = (f"Review round {n} returned findings. The full structured review is in "
                         f"{self.state['rounds'][-1].get('envelope_path')} (field `result`). Dispose of every pending "
                         "finding: " + ", ".join(self.state["pending_findings"]))
        elif context == "advanced":
            situation = "The previous checkpoint passed independent review. Continue with the active checkpoint below."
        elif context == "needs_report":
            situation = ("The branch head changed or a review became historical. Make sure the worktree holds the "
                         "candidate you intend, validate it, and report it for review.")
        elif context == "reopened":
            situation = "The owner-review handoff was superseded by a later change. Validate and report the candidate."
        else:
            situation = ""
        if self.state.get("pending_findings") and context != "findings":
            situation += " Pending findings still need dispositions: " + ", ".join(self.state["pending_findings"])
        fields["situation"] = situation
        prompt = self.render("developer-continue.md" if resume else "developer-start.md", fields)
        n = dev["invocations"] + 1
        log_dir = self.dir / "developer"
        log_dir.mkdir(parents=True, exist_ok=True)
        write_text(log_dir / f"prompt-{n:03d}.md", prompt)
        # Charge the budget and record launch intent before dispatch.
        dev.update(invocations=n, launch={"n": n, "at": now_iso(), "resume": resume, "returned": False})
        self.save()

        def on_start(pid):
            dev["active_pid"] = pid
            self.save()

        def on_soft_deadline():
            write_text(self.stop_path, f"soft deadline reached for developer invocation {n} ({now_iso()})\n")

        out = self.developer.run(prompt, dev["session_id"], resume, self.state["repo_root"],
                                 log_dir / f"stream-{n:03d}.jsonl", log_dir / f"stderr-{n:03d}.txt",
                                 self.cfg["developer"]["soft_deadline_minutes"] * 60, on_start,
                                 on_soft_deadline=on_soft_deadline,
                                 grace_s=self.cfg["developer"].get("grace_minutes", 60) * 60,
                                 protected=self.runtime_active)
        dev.update(active_pid=None, started=True)
        dev["launch"]["returned"] = True
        dev.setdefault("log", []).append({k: out.get(k) for k in (
            "init_model", "main_models", "observed_effort", "api_key_source", "cost_usd", "model_usage",
            "permission_denials", "exit_code", "duration_s", "deadline_exceeded", "terminated", "subtype",
            "session_id")})
        self.save()
        if out.get("terminated"):
            raise Stop("PAUSED", f"developer invocation {n} exceeded its deadline and grace period without an active "
                                 "runtime transaction and was terminated; inspect the worktree before resuming")
        if out["init_model"] is None:
            tail = (log_dir / f"stderr-{n:03d}.txt").read_text(encoding="utf-8", errors="replace").strip()[-400:]
            raise Stop("BLOCKED", f"developer invocation {n} did not start a session (exit {out['exit_code']}): {tail}")
        if out["session_id"] != dev["session_id"]:
            raise Stop("BLOCKED", f"developer ran in session {out['session_id']}, not the enrolled {dev['session_id']}")
        allowed = re.compile(self.cfg["developer"]["allowed_model_pattern"])
        models = [out["init_model"], *out["main_models"]]
        if any(not m or not allowed.match(m) for m in models):
            raise Stop("BLOCKED", f"developer ran on {models}, not an allowed {self.cfg['developer']['model']} model")
        efforts = out["observed_effort"]
        if efforts and efforts != [self.cfg["developer"]["effort"]]:
            raise Stop("BLOCKED", f"developer effort observed {efforts}, required {self.cfg['developer']['effort']}")
        if out["exit_code"] != 0 or out["is_error"] or not isinstance(out["structured_output"], dict):
            return (f"invocation {n} ended without a valid structured report (exit {out['exit_code']}, "
                    f"{out['subtype']}); return the report")
        error = self.accept_report(out["structured_output"])
        if out["deadline_exceeded"] and not error:
            raise Stop("PAUSED", f"developer invocation {n} exceeded its soft deadline; review progress before resuming")
        return error

    def check_stop_request(self):
        if self.stop_path.exists():
            raise Stop("PAUSED", "stop requested: " + self.stop_path.read_text(encoding="utf-8").strip())

    def run_loop(self):
        if self.state["mode"] != "headless":
            raise UsageError("this enrollment uses foreground mode; use `review --report` and `finalize`")
        correction = None
        while True:
            self.check_stop_request()
            self.verify_inputs()
            phase = self.state["phase"]
            if phase == "develop":
                correction = self.develop_step(correction)
                if correction:
                    continue
                self.transition("review", "developer reported a checkpoint")
            elif phase == "review":
                self.review_round()
            elif phase == "finalize":
                return self.finalize()
            elif phase == "done":
                return "AWAITING_OWNER_REVIEW"
            else:
                raise UsageError(f"unknown phase {phase}")

    def resume(self, reason, max_rounds=None):
        """Owner-initiated continuation after a stop; counters and session persist."""
        if self.state["status"] == "AWAITING_OWNER_REVIEW":
            moved = self.handoff_superseded()
            if not moved:
                raise UsageError("the owner-review handoff is still current; nothing to resume")
            self.reopen(moved)
        self.stop_path.unlink(missing_ok=True)
        active = self.active()
        if active and active["status"] in ("requested", "reviewing"):
            # failed attempts stay recorded; the owner's resume grants a fresh attempt budget
            active["earlier_attempts"] = active.get("earlier_attempts", []) + active["attempts"]
            active["attempts"] = []
        if max_rounds:
            self.state["limits"]["max_review_rounds_per_checkpoint"] = int(max_rounds)
        self.state.update(status="ACTIVE", status_reason=f"resumed: {reason}")
        self.transition(self.state["phase"], f"owner resume: {reason}")
        if self.state["mode"] == "headless":
            return self.run_loop()
        if self.state["phase"] == "review":
            result = self.review_round()
            if result == "needs_report":
                raise Stop("PAUSED", "submit a developer report for the current candidate (`review --report`)")
            return result
        if self.state["phase"] == "finalize":
            return self.finalize()
        return "ACTIVE (foreground: continue with `review --report`)"

    # -- archive and readiness ---------------------------------------------------------
    def transcript_folder(self):
        return f"{self.cfg['transcript_dir'].strip('/')}/pr-{self.state['pr']['number']}"

    def archive(self, reason):
        """Commit unarchived records as a transcript-only commit; restartable at every step."""
        self.assert_no_active_developer()
        if self.state.get("archive_pending"):
            return self.complete_archive(self.state["archive_pending"])
        if not self.git.clean():
            raise Stop("PAUSED", "cannot archive records: developer worktree has uncommitted changes")
        head = self.git.head()
        if self.git.remote_head(self.state["branch"]) != head:
            raise Stop("PAUSED", "cannot archive records: local branch is not the pushed head")
        self.verify_transcripts(head)
        rel_dir = self.transcript_folder()
        folder = Path(self.state["repo_root"]) / rel_dir
        existing = [int(p.name[:3]) for p in folder.glob("[0-9][0-9][0-9]-*")] if folder.exists() else []
        seq = max(existing, default=0) + 1
        archived_keys = set(self.state.get("archived_keys", []))
        files = []
        for pub in sorted(self.state.get("publications", {}).values(), key=lambda p: p["seq"]):
            if pub["state"] != "posted" or pub["key"] in archived_keys or pub["key"].startswith("final-"):
                continue
            body = "\n\n---\n\n".join(Path(p["body_file"]).read_text(encoding="utf-8") for p in pub["parts"])
            files.append((f"{rel_dir}/{seq:03d}-{pub['key']}.md", body, pub["key"]))
            seq += 1
            if pub["key"].endswith("-review"):
                n = int(pub["key"][1:3])
                rnd = next((r for r in self.state["rounds"] if r["round"] == n and r.get("envelope_path")), None)
                if rnd:
                    env_text, _ = sanitize(json.dumps(read_json(rnd["envelope_path"]), indent=2, sort_keys=True))
                    files.append((f"{rel_dir}/{seq:03d}-r{n:02d}-envelope.json", env_text + "\n", f"r{n:02d}-envelope"))
                    seq += 1
        if not files:
            return head
        pending = {"parent": head, "reason": reason, "commit": None,
                   "files": [{"rel": rel, "sha256": sha256_text(text), "key": key} for rel, text, key in files]}
        self.state["archive_pending"] = pending
        self.save()  # intent recorded before any write
        for rel, text, _ in files:
            path = Path(self.state["repo_root"]) / rel
            if path.exists():
                raise Stop("BLOCKED", f"archive record {rel} already exists")
            write_text(path, text)
        return self.complete_archive(pending)

    def complete_archive(self, pending):
        root = Path(self.state["repo_root"])
        rels = [f["rel"] for f in pending["files"]]
        if pending["commit"] is None:
            if self.git.head() != pending["parent"]:
                raise Stop("BLOCKED", "branch moved during an interrupted archive; inspect before resuming")
            dirty = {line[3:] for line in self.git("status", "--porcelain", "--untracked-files=all").splitlines() if line}
            if dirty - set(rels):
                raise Stop("BLOCKED", "unexpected changes alongside an interrupted archive: " + ", ".join(sorted(dirty - set(rels))))
            for f in pending["files"]:
                path = root / f["rel"]
                if not path.exists() or sha256_file(path) != f["sha256"]:
                    raise Stop("BLOCKED", f"interrupted archive record {f['rel']} is missing or changed")
            self.git("add", "--", *rels)
            self.git("commit", "--quiet", "-m",
                     f"Archive agent review records for PR #{self.state['pr']['number']} ({pending['reason']})")
            pending["commit"] = self.git.head()
            self.save()
        commit = pending["commit"]
        if self.git.head() != commit:
            raise Stop("BLOCKED", f"local branch moved away from archive commit {short(commit)}")
        expected = {f["rel"]: f["sha256"] for f in pending["files"]}
        ok, reasons = check_archive_only(self.git, pending["parent"], commit, self.cfg["transcript_dir"],
                                         self.state["pr"]["number"], expected)
        if not ok:
            raise Stop("BLOCKED", "archive commit failed its transcript-only check: " + "; ".join(reasons))
        remote = self.git.remote_head(self.state["branch"])
        if remote != commit:
            if remote != pending["parent"]:
                raise Stop("BLOCKED", f"remote branch moved to {short(remote)} during archive")
            if self.cfg.get("push_command"):
                argv = [a.replace("{repo_root}", self.state["repo_root"]).replace("{branch}", self.state["branch"])
                        for a in self.cfg["push_command"]]
                proc = run(argv, cwd=self.state["repo_root"], check=False, timeout=600)
                detail = (proc.stderr or proc.stdout).strip()[-400:] if proc.returncode else ""
            else:
                try:
                    guarded_push(self.state, self.cfg)
                    detail = ""
                except (UsageError, CommandError) as exc:
                    detail = str(exc)
            if detail or self.git.remote_head(self.state["branch"]) != commit:
                raise Stop("PAUSED", f"guarded push of archive commit {short(commit)} failed (resume retries it): "
                                     f"{detail}")
        archived = self.state.setdefault("archived", {})
        archived.update(expected)
        self.state["archived_keys"] = sorted(set(self.state.get("archived_keys", [])) | {f["key"] for f in pending["files"]})
        self.state.setdefault("archive_commits", []).append({"commit": commit, "source": pending["parent"],
                                                             "reason": pending["reason"]})
        self.state["archive_pending"] = None
        self.save()
        return commit

    def readiness(self, archive_head):
        ready = self.state.get("ready_review")
        reasons, notes = [], []
        if not ready:
            return ["no accepted final review with decision ready_for_owner_review"], notes
        if self.state["plan"]["checkpoints"][self.state["checkpoint_index"]]["type"] != "final":
            reasons.append("the active checkpoint is not the final checkpoint")
        ok, why = check_archive_only(self.git, ready["H"], archive_head, self.cfg["transcript_dir"],
                                     self.state["pr"]["number"], self.state.get("archived", {}))
        if not ok:
            reasons.append("material change after the reviewed candidate: " + "; ".join(why))
        pr = self.pr_view()
        if pr["headRefOid"] != archive_head:
            reasons.append(f"PR head {short(pr['headRefOid'])} is not {short(archive_head)}")
        if pr.get("reviewRequests"):
            reasons.append("human reviewers are requested on the PR; the workflow ends without review requests")
        ledger = self.state.get("findings", {})
        unresolved = sorted(fid for fid, e in ledger.items() if e["blocks_readiness"] and e["status"] != "resolved")
        if unresolved:
            reasons.append("blocking findings without reviewer-accepted dispositions: " + ", ".join(unresolved))
        report = self.state.get("last_report") or {}
        if report.get("runtime_restoration") == "pending":
            reasons.append("runtime restoration is pending")
        if self.state.get("last_report_for") != ready["H"]:
            reasons.append("the accepted developer report is not bound to the reviewed candidate")
        failing = [v["command"] for v in report.get("validation", []) if v.get("required") and v["result"] != "pass"]
        if failing:
            reasons.append("required validation not passing: " + "; ".join(failing))
        problem = self.mission_problem()
        if problem:
            reasons.append(problem)
        base_now = self.git.fetch(self.state["target"])
        merge_base = self.git("merge-base", ready["H"], base_now)
        if merge_base != ready["M"]:
            reasons.append(f"merge base moved {short(ready['M'])} -> {short(merge_base)}; the reviewed comparison is stale")
        elif base_now != ready["B"]:
            notes.append(f"target {self.state['target']} advanced {short(ready['B'])} -> {short(base_now)} after review; "
                         f"the reviewed diff M={short(ready['M'])}..H is unchanged (recorded reconciliation). Rebase "
                         "before merging would need a new review.")
        try:
            required = self.gh.required_checks(self.state["target"])
            results = self.gh.commit_checks(ready["H"])
        except Exception as exc:
            reasons.append(f"hosted-check inventory unavailable: {exc}")
        else:
            if not required:
                notes.append(f"no required status checks on {self.state['target']} (no branch protection or ruleset "
                             "requires one); verified, not assumed")
            for context in required:
                passed = [c for c in results if c["name"] == context and c["status"] == "completed"
                          and c["conclusion"] in ("success", "neutral", "skipped")]
                if not passed:
                    reasons.append(f"required check {context} has not passed on reviewed source {short(ready['H'])}")
            failed = [c["name"] for c in results if c["conclusion"] in ("failure", "error", "timed_out", "cancelled")]
            if failed:
                reasons.append("failing checks on the reviewed source: " + ", ".join(sorted(set(failed))))
        return reasons, notes

    def invalidate_ready(self, why):
        ready = self.state.get("ready_review")
        self.state.setdefault("superseded_ready", []).append({**(ready or {}), "why": why, "at": now_iso()})
        self.state.update(ready_review=None, last_report_for=None, develop_context="needs_report")
        self.transition("develop", "ready result invalidated: " + why)
        raise Stop("PAUSED", f"material change after the ready review ({why}); the new candidate needs a developer "
                             "report and a fresh review")

    def finalize(self):
        self.check_stop_request()
        ready = self.state.get("ready_review")
        if not ready:
            raise Stop("PAUSED", "no ready review to finalize")
        if self.state.get("archive_pending"):
            self.complete_archive(self.state["archive_pending"])  # finish an interrupted archive transaction first
        head = self.git.head()
        ok, why = check_archive_only(self.git, ready["H"], head, self.cfg["transcript_dir"], self.state["pr"]["number"],
                                     self.state.get("archived", {}))
        if not ok:
            self.invalidate_ready("; ".join(why))
        archive_head = self.archive("final")
        reasons, notes = self.readiness(archive_head)
        if any(r.startswith(("material change", "merge base moved")) for r in reasons):
            self.invalidate_ready("; ".join(reasons))
        if reasons:
            raise Stop("BLOCKED", "not ready for owner review: " + "; ".join(reasons))
        summary = ready["summary"]
        open_nonblocking = sorted(fid for fid, e in self.state.get("findings", {}).items()
                                  if not e["blocks_readiness"] and e["status"] != "resolved")
        lines = [f"### Workflow — awaiting owner review — source `{short(ready['H'])}`, archive `{short(archive_head)}`\n",
                 f"- Reviewed source candidate H: `{ready['H']}` (review round {ready['round']}, "
                 f"Codex `{self.cfg['reviewer']['model']}` / `{self.cfg['reviewer']['effort']}`)",
                 f"- Archive commit A: `{archive_head}` — "
                 + ("identical to H." if archive_head == ready["H"] else
                    "adds only the controller's append-only transcript records under "
                    f"`{self.transcript_folder()}/` (path-, mode-, inventory- and hash-checked)."),
                 f"- Merge base M: `{ready['M']}`; target at review B: `{ready['B']}`",
                 f"- Developer: Claude `{self.cfg['developer']['model']}` / `{self.cfg['developer']['effort']}`; "
                 f"observed {self.provenance_line(self.state.get('last_report_provenance'))}",
                 "- Review rounds: " + ", ".join(f"{k}: {v}" for k, v in self.state["rounds_completed"].items()),
                 "", f"**Delivered behavior (reviewer summary):** {summary['delivered_behavior']}", "",
                 "**Remaining owner review:**", *([f"- {x}" for x in summary["remaining_owner_review"]] or ["- none listed"]),
                 "", "**Open non-blocking items:** " + (", ".join(open_nonblocking) or "none"),
                 "", "**Notes:**", *([f"- {n}" for n in notes] or ["- none"]), "",
                 "This is readiness for the owner's review only. It does not merge, release, or grant visual "
                 "acceptance. The PR remains a draft with no requested reviewers.", "",
                 "_Automated workflow status posted through the owner's GitHub account; not a human approval._"]
        self.publish(f"final-status-{short(archive_head)}", "\n".join(lines) + "\n")
        self.update_pr_body(f"awaiting owner review. Reviewed source `{ready['H']}`; archive head `{archive_head}` "
                            "(transcript-only successor).")
        self.state.update(status="AWAITING_OWNER_REVIEW", status_reason=f"source {ready['H']}, archive {archive_head}",
                          handoff={"H": ready["H"], "A": archive_head, "at": now_iso()})
        self.transition("done", "owner review handoff posted")
        return "AWAITING_OWNER_REVIEW"

    def handoff_superseded(self):
        """None if the handoff is current; otherwise a description of what moved."""
        handoff = self.state.get("handoff") or {}
        pr = self.gh.pr(self.state["pr"]["number"])
        if pr["headRefOid"] == handoff.get("A") and pr.get("isDraft") and pr["state"] == "OPEN":
            return None
        return f"PR head {short(pr['headRefOid'])} (handoff archive {short(handoff.get('A'))}), state {pr['state']}"

    def reopen(self, moved):
        self.state.setdefault("superseded_handoffs", []).append({**self.state.get("handoff", {}), "why": moved,
                                                                 "at": now_iso()})
        self.state.setdefault("superseded_ready", []).append(self.state.get("ready_review"))
        self.state.update(ready_review=None, last_report_for=None, handoff=None, develop_context="reopened")
        self.transition("develop", f"handoff superseded: {moved}")
        self.publish_status("REOPENED", f"The owner-review handoff was superseded ({moved}); review reopened. The "
                                        "earlier receipt is preserved above and is no longer current.")
        self.update_pr_body(f"handoff superseded ({moved}); review reopened.")

    def update_pr_body(self, text):
        pr = self.pr_view()
        start, end = "<!-- agent-review:status:start -->", "<!-- agent-review:status:end -->"
        section = (f"{start}\n**Agent review status:** {text} Records: `{self.transcript_folder()}/`.\n{end}")
        body = pr.get("body") or ""
        body = re.sub(re.escape(start) + ".*?" + re.escape(end), lambda _: section, body, flags=re.S) \
            if start in body else body.rstrip() + "\n\n" + section
        path = self.dir / "outbox" / "pr-body.md"
        write_text(path, sanitize(body)[0])
        self.gh.edit_body(self.state["pr"]["number"], path)

    # -- status ---------------------------------------------------------------------------
    def status(self, remote=False):
        s = self.state
        lock = self.lock("status")
        try:
            holder = read_json(lock.holder) if lock.holder.exists() else None
        except (ValueError, OSError):
            holder = {"unreadable": str(lock.holder)}
        try:
            fp_ok = workflow_fingerprint(self.wf) == s["workflow"]["fingerprint"]
        except Stop:
            fp_ok = False
        out = {
            "enrollment": s["enrollment_id"], "status": s["status"], "status_reason": s.get("status_reason"),
            "phase": s["phase"], "mode": s["mode"], "pr": s["pr"]["url"], "branch": s["branch"],
            "checkpoint": self.checkpoint["id"], "checkpoint_type": self.checkpoint["type"],
            "rounds_completed": s["rounds_completed"], "round_limit": self.limits["max_review_rounds_per_checkpoint"],
            "last_round": {k: s["rounds"][-1].get(k) for k in ("round", "status", "decision", "H")} if s["rounds"] else None,
            "open_findings": sorted(f for f, e in s.get("findings", {}).items() if e["status"] == "open"),
            "developer_session": s["developer"]["session_id"], "developer_invocations": s["developer"]["invocations"],
            "developer_running": pid_alive(s["developer"].get("active_pid")),
            "lock_holder": holder, "stop_requested": self.stop_path.exists(),
            "pending_publications": sorted(k for k, p in s.get("publications", {}).items() if p["state"] != "posted"),
            "archive_pending": bool(s.get("archive_pending")),
            "workflow_fingerprint_matches": fp_ok, "updated_at": s.get("updated_at"),
        }
        if remote and s["status"] == "AWAITING_OWNER_REVIEW":
            try:
                moved = self.handoff_superseded()
                out["handoff_current"] = moved is None
                if moved:
                    out["handoff_superseded_by"] = moved
            except Exception as exc:
                out["handoff_current"] = f"unknown: {exc}"
        return out


# --------------------------------------------------------------------------- enrollment

def trusted_dirs(root: Path, folder: str):
    """Owner-controlled config locations: the installed workflow and the machine-level home."""
    return [(root / folder).resolve(), (agent_home() / folder).resolve()]


def _trusted(root: Path, folder: str, name_or_path: str) -> Path:
    bases = trusted_dirs(root, folder)
    path = Path(name_or_path)
    if path.suffix == ".json":
        path = path.resolve()
        if path.parent not in bases or not path.is_file():
            raise UsageError(f"{name_or_path} is not a {folder[:-1]} in {[str(b) for b in bases]}")
        return path
    found = [b / f"{name_or_path}.json" for b in bases if (b / f"{name_or_path}.json").is_file()]
    if len(found) != 1:
        raise UsageError(f"{folder[:-1]} '{name_or_path}' must exist in exactly one of {[str(b) for b in bases]}")
    return found[0]


def compose_developer_settings(cfg, root: Path, enrollment_id: str):
    """Generic base permissions plus the project's extras, filled for this enrollment."""
    base = read_json(root / "settings" / "developer.base.json")["permissions"]
    extra = cfg["developer"].get("permissions") or {}
    subs = {"{helper}": f"{Path(root).resolve().as_posix()}/agent_review.py", "{enrollment}": enrollment_id,
            "{transcript_dir}": cfg["transcript_dir"].strip("/")}

    def fill(rules):
        out = []
        for rule in rules:
            for key, value in subs.items():
                rule = rule.replace(key, value)
            out.append(rule)
        return out
    return {"permissions": {"allow": fill(base["allow"] + extra.get("allow", [])),
                            "deny": fill(base["deny"] + extra.get("deny", []))}}


def push_argv(cfg, root: Path, enrollment_id: str):
    if cfg.get("push_command"):
        return list(cfg["push_command"])
    # Literal "python" so the developer's command matches its narrow permission rule exactly.
    return ["python", f"{Path(root).resolve().as_posix()}/agent_review.py", "push", "--enrollment", enrollment_id]


def guarded_push(state, cfg):
    """Built-in guarded push for repositories without their own wrapper: enrolled branch only,
    clean tree, approved origin, fast-forward, no credentials in the feature diff, never force."""
    git = Git(state["repo_root"])
    branch = state["branch"]
    if not same_path(git.toplevel(), state["repo_root"]) or git.branch() != branch:
        raise UsageError(f"push must run in the enrolled worktree on {branch}")
    if not re.match(cfg["branch_pattern"], branch) or branch == state["target"]:
        raise UsageError(f"{branch} is not a pushable feature branch")
    for marker in ("MERGE_HEAD", "CHERRY_PICK_HEAD", "REVERT_HEAD", "rebase-merge", "rebase-apply"):
        path = Path(git("rev-parse", "--git-path", marker))
        if not path.is_absolute():
            path = Path(state["repo_root"]) / path
        if path.exists():
            raise UsageError(f"unfinished Git operation: {marker}")
    if not git.clean():
        raise UsageError("push requires a clean worktree")
    if git.remote_url() not in cfg["allowed_remote_urls"]:
        raise UsageError("origin is not an approved remote")
    head = git.head()
    remote = git.remote_head(branch)
    if remote and not git.is_ancestor(remote, head):
        raise UsageError("the remote branch is not an ancestor; reconcile without force")
    base = git.fetch(state["target"])
    for path in git("diff", "--name-only", "--diff-filter=AM", git("merge-base", head, base), head).splitlines():
        blob = subprocess.run(["git", "-C", state["repo_root"], "show", f"{head}:{path}"], capture_output=True).stdout
        text = blob.decode("utf-8", "ignore")
        if TOKEN_RE.search(text) or PEM_BEGIN_RE.search(text):
            raise UsageError(f"possible credential in {path}; refusing to push")
    try:
        git("push", "--quiet", "origin", f"HEAD:refs/heads/{branch}")
    except CommandError as exc:
        raise UsageError(f"git push failed: {exc}")
    if git.remote_head(branch) != head:
        raise UsageError("remote verification failed after push")
    return head


def check_policy(cfg):
    for role, required in POLICY.items():
        for key, value in required.items():
            if cfg[role].get(key) != value:
                raise UsageError(f"project {role}.{key} is {cfg[role].get(key)!r}; owner policy requires {value!r}")


def load_project(name_or_path, root=HERE):
    path = _trusted(root, "projects", name_or_path)
    cfg = read_json(path)
    if cfg.get("schema_version") != "agent-review.project.v1":
        raise UsageError(f"{path} is not an agent-review project config")
    check_policy(cfg)
    return cfg


def load_plan(name_or_path, root=HERE):
    path = _trusted(root, "plans", name_or_path)
    plan = read_json(path)
    cps = plan.get("checkpoints") or []
    if plan.get("schema_version") != "agent-review.plan.v1" or not cps:
        raise UsageError(f"{path} is not an agent-review plan")
    if cps[-1]["type"] != "final" or any(c["type"] != "intermediate" for c in cps[:-1]):
        raise UsageError("plan must end with exactly one final checkpoint; earlier checkpoints are intermediate")
    if len({c["id"] for c in cps}) != len(cps):
        raise UsageError("checkpoint IDs must be unique")
    return plan


def plan_for_mission(plan, mission: Path):
    """A generic plan (mission_id 'mission') takes its identity from the mission file name."""
    if plan.get("mission_id") != "mission":
        return plan
    slug = re.sub(r"[^a-z0-9]+", "-", mission.stem.lower()).strip("-") or "mission"
    return {**plan, "mission_id": slug, "title": mission.stem}


def registry_path(repository, branch):
    return agent_home() / "registry" / f"{branch_key(repository, branch)}.json"


def enroll(args, github=None, root=HERE):
    cfg = load_project(args.project, root)
    plan = load_plan(args.plan, root)
    git = Git(args.worktree)
    top = git.toplevel()
    branch = git.branch()
    if not re.match(cfg["branch_pattern"], branch):
        raise UsageError(f"branch {branch} is outside the approved pattern {cfg['branch_pattern']}")
    if git.remote_url() not in cfg["allowed_remote_urls"]:
        raise UsageError("origin is not an approved remote for this project")
    gh = github or GitHub(cfg["repository"], cfg.get("gh", "gh"))
    if gh.viewer() != cfg["expected_actor"]:
        raise UsageError("GitHub actor does not match the project's expected actor")
    foreground_session = None
    if args.mode == "foreground":
        foreground_session = os.environ.get("CLAUDE_CODE_SESSION_ID")
        if not foreground_session:
            raise UsageError("foreground enrollment must be run from the Claude session that will develop "
                             "(CLAUDE_CODE_SESSION_ID is not set)")
    mission = Path(args.mission).resolve()
    if not mission.is_file():
        raise UsageError(f"mission not found: {mission}")
    lock = Lock(agent_home() / "locks" / f"{branch_key(cfg['repository'], branch)}.lock", "enroll")
    with lock:
        reg = registry_path(cfg["repository"], branch)
        if reg.exists():
            prior = read_json(reg)
            prior_state = Path(prior["dir"]) / "state.json"
            if prior_state.exists() and read_json(prior_state).get("status") not in ("AWAITING_OWNER_REVIEW", "CANCELLED"):
                raise UsageError(f"branch {branch} already has an active enrollment: {prior['enrollment_id']}")
        state_root = Path(expand(args.state_root or cfg.get("state_root") or str(agent_home() / "state")))
        state_root.mkdir(parents=True, exist_ok=True)
        slug = re.sub(r"[^a-z0-9]+", "-", branch.lower()).strip("-")
        enrollment_id = f"{cfg['project_id']}-{slug}-{dt.datetime.now().strftime('%Y%m%d%H%M%S')}-{uuid.uuid4().hex[:4]}"
        folder = state_root / enrollment_id
        folder.mkdir()
        try:
            return _enroll_into(folder, enrollment_id, cfg, plan, args, git, gh, mission, top, branch, reg, root,
                                foreground_session)
        except BaseException:
            shutil.rmtree(folder, ignore_errors=True)  # a refused enrollment leaves no partial state
            raise


def _enroll_into(folder, enrollment_id, cfg, plan, args, git, gh, mission, top, branch, reg, root, foreground_session):
    snapshot = folder / "mission" / mission.name
    snapshot.parent.mkdir()
    shutil.copy2(mission, snapshot)
    digest = sha256_file(mission)
    if sha256_file(snapshot) != digest:
        raise UsageError("mission snapshot does not match the mission file")
    plan = plan_for_mission(plan, mission)
    settings_path = folder / "developer-settings.json"
    atomic_write_json(settings_path, compose_developer_settings(cfg, root, enrollment_id))
    target = args.target or cfg["default_target_branch"]
    if args.pr:
        number = int(args.pr)
    else:
        if git.remote_head(branch) != git.head() or not git.clean():
            raise UsageError("push a clean branch head through the guarded wrapper before creating the PR")
        body = folder / "outbox" / "pr-create.md"
        write_text(body, sanitize(
            f"Draft PR for mission `{plan['mission_id']}` under the agent-review workflow "
            f"(Claude {cfg['developer']['model']} / {cfg['developer']['effort']} developer, Codex "
            f"{cfg['reviewer']['model']} / {cfg['reviewer']['effort']} independent reviewer).\n\n"
            "Review requests, model-generated reviews, developer dispositions and status are posted as PR comments "
            "by the controller through the owner's account; none is a human review. Records are archived under "
            f"`{cfg['transcript_dir']}/`.\n\n<!-- agent-review:status:start -->\n**Agent review status:** enrolled."
            "\n<!-- agent-review:status:end -->\n")[0])
        number = gh.create_draft_pr(branch, target, args.title or f"[draft] {plan.get('title', plan['mission_id'])}",
                                    body)
    pr = gh.pr(number)
    owner, name = cfg["repository"].split("/")
    if pr.get("number") != number or not pr["url"].endswith(f"/{cfg['repository']}/pull/{number}"):
        raise UsageError("PR is not in the project's repository")
    if pr.get("isCrossRepository") or (pr.get("headRepositoryOwner") or {}).get("login") != owner or \
            (pr.get("headRepository") or {}).get("name") != name:
        raise UsageError("PR head is not in the project's repository")
    if pr["headRefName"] != branch or pr["baseRefName"] != target or not pr["isDraft"] or pr["state"] != "OPEN":
        raise UsageError(f"PR #{number} is not an open draft from {branch} into {target}")
    if pr.get("reviewRequests"):
        raise UsageError("PR has requested reviewers; the workflow requires none")
    limits = dict(cfg["limits"])
    if args.max_rounds:
        limits["max_review_rounds_per_checkpoint"] = int(args.max_rounds)
    state = {
        "schema_version": STATE_SCHEMA, "protocol": PROTOCOL, "enrollment_id": enrollment_id,
        "project_id": cfg["project_id"], "project": cfg, "plan": plan, "mode": args.mode,
        "foreground_session": foreground_session,
        "repo_root": top, "branch": branch, "target": target,
        "pr": {"number": number, "url": pr["url"]},
        "mission": {"path": str(mission), "sha256": digest, "snapshot": str(snapshot)},
        "workflow": {"fingerprint": workflow_fingerprint(root), "root": str(root), "activated_at": now_iso()},
        "developer_settings_sha256": sha256_file(settings_path),
        "limits": limits, "checkpoint_index": 0, "status": "ACTIVE", "phase": "develop",
        "developer": {"session_id": str(uuid.uuid4()), "started": False, "invocations": 0, "active_pid": None},
        "rounds": [], "active_round": None, "rounds_completed": {}, "findings": {}, "pending_findings": [],
        "publications": {}, "archived": {}, "archived_keys": [], "archive_pending": None,
        "created_at": now_iso(), "transitions": [],
    }
    atomic_write_json(folder / "state.json", state)
    atomic_write_json(reg, {"enrollment_id": enrollment_id, "dir": str(folder), "repository": cfg["repository"],
                            "branch": branch, "at": now_iso()})
    return folder, state


# --------------------------------------------------------------------------- preflight

def preflight(cfg, root=HERE):
    """Real bounded calls to both configured models; records requested vs observed metadata."""
    import tempfile
    out = {"at": now_iso(), "project": cfg["project_id"]}
    work = Path(tempfile.mkdtemp(prefix="agent-review-preflight-"))
    atomic_write_json(work / "developer-settings.json", compose_developer_settings(cfg, root, "preflight"))
    dev = ClaudeDeveloper(cfg["developer"], work / "developer-settings.json", root / REPORT_SCHEMA_FILE)
    sid = str(uuid.uuid4())
    dev_out = dev.run('Return a report with schema_version "agent-review.developer.v1", status "paused", '
                      'checkpoint_id "preflight", candidate_commit null, summary "preflight", empty arrays, '
                      'runtime_restoration "not_applicable", blocker null, owner_decision_needed null. Run no tools.',
                      sid, False, work, work / "claude.jsonl", work / "claude.err", 600)
    allowed = re.compile(cfg["developer"]["allowed_model_pattern"])
    out["developer"] = {"cli": dev.version(), "requested": dev.requested, "session_id": sid,
                        "observed_model_init": dev_out["init_model"], "observed_models_api": dev_out["main_models"],
                        "observed_effort_transcript": dev_out["observed_effort"],
                        "api_key_source": dev_out["api_key_source"], "model_usage": dev_out["model_usage"],
                        "structured_output_ok": isinstance(dev_out["structured_output"], dict),
                        "ok": bool(dev_out["init_model"]) and all(allowed.match(m or "") for m in
                                                                   [dev_out["init_model"], *dev_out["main_models"]])
                        and dev_out["observed_effort"] == [cfg["developer"]["effort"]]}
    rev = CodexReviewer(cfg["reviewer"])
    schema = work / "schema.json"
    atomic_write_json(schema, {"type": "object", "additionalProperties": False, "required": ["ok"],
                               "properties": {"ok": {"type": "boolean"}}})
    info = rev.run('Return {"ok": true}. Run no commands.', work, schema, work / "codex.json", work / "codex.jsonl",
                   work / "codex.err", 600, skip_git=True)
    prov = rev.provenance(info["thread_id"])
    out["reviewer"] = {"cli": rev.version(), "auth": rev.auth_route(), "requested": rev.requested, "run": info,
                       "observed": prov, "ok": info["exit_code"] == 0 and prov["observed_model"] == cfg["reviewer"]["model"]
                       and prov["observed_effort"] == cfg["reviewer"]["effort"] and prov["sandbox"] == "read-only"}
    out["limitations"] = [
        "Claude: model observed from API response message metadata; effort observed from the session transcript's "
        "recorded effort setting (the init event does not report effort).",
        "Codex: model/effort observed from the session rollout's recorded thread settings; the serving backend's "
        "model identity is not separately exposed by the CLI.",
        "Cost: Claude reports list-price cost even on subscription auth; Codex reports tokens and plan type only. "
        "No precise combined cost is claimed.",
    ]
    shutil.rmtree(work, ignore_errors=True)
    return out


# --------------------------------------------------------------------------- install

def install(dest: Path, source: Path = HERE):
    digest, hashes = compute_fingerprint(source)
    locks = agent_home() / "locks"
    for lock_file in locks.glob("*.lock") if locks.exists() else []:
        if Lock(lock_file, "install-check").held_elsewhere():
            raise UsageError(f"an active controller holds {lock_file}; stop it before reinstalling")
    dest.mkdir(parents=True, exist_ok=True)
    old = dest / "MANIFEST.json"
    if old.exists():
        for rel in read_json(old).get("files", {}):
            if rel not in hashes:
                (dest / rel).unlink(missing_ok=True)
    for rel in hashes:
        (dest / rel).parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source / rel, dest / rel)
    commit = run(["git", "-C", str(source), "rev-parse", "HEAD"], check=False).stdout.strip()
    dirty = run(["git", "-C", str(source), "status", "--porcelain", "--", "."], check=False).stdout.strip() != ""
    atomic_write_json(old, {"fingerprint": digest, "files": hashes, "source_commit": commit,
                            "source_dirty": dirty, "installed_at": now_iso()})
    return digest


# --------------------------------------------------------------------------- onboarding

def github_slug(url: str):
    match = re.match(r"^(?:https://github\.com/|git@github\.com:)([^/]+)/([^/]+?)(?:\.git)?$", url.strip())
    return f"{match.group(1)}/{match.group(2)}" if match else None


def init_project(args, root=HERE, github_factory=GitHub):
    """Write a project config for a repository under the machine-level home (~/.agent-review/projects)."""
    if not re.fullmatch(r"[a-z0-9][a-z0-9-]*", args.name):
        raise UsageError("project name must be lowercase letters, digits and dashes")
    git = Git(args.repo)
    top = git.toplevel()
    url = git.remote_url()
    slug = github_slug(url)
    if not slug:
        raise UsageError(f"origin {url} is not a GitHub repository")
    gh = github_factory(slug)
    actor = gh.viewer()
    default_branch = gh._json("repo", "view", slug, "--json", "defaultBranchRef")["defaultBranchRef"]["name"]
    target = agent_home() / "projects" / f"{args.name}.json"
    if target.exists() and not args.force:
        raise UsageError(f"{target} exists; pass --force to replace it")
    if (root / "projects" / f"{args.name}.json").exists():
        raise UsageError(f"a shipped project named {args.name} already exists")
    owner, name = slug.split("/")
    prefix = args.branch_prefix.rstrip("/") + "/"
    in_nima = Path(top).resolve().as_posix().lower().startswith("c:/dev/nima")
    config_dir = args.claude_config_dir or ("%USERPROFILE%/.claude" if in_nima else "%USERPROFILE%/.claude-personal")
    instructions = [p for p in ("AGENTS.md", "CLAUDE.md", "CONTRIBUTING.md") if (Path(top) / p).exists()]
    shipped = read_json(root / "projects" / "kingmaker-gunslinger.json")
    cfg = {
        "schema_version": "agent-review.project.v1", "project_id": args.name, "repository": slug,
        "allowed_remote_urls": sorted({f"https://github.com/{owner}/{name}.git", f"git@github.com:{owner}/{name}.git", url}),
        "default_target_branch": default_branch, "branch_pattern": f"^{re.escape(prefix)}[a-z0-9][a-z0-9._/-]*$",
        "expected_actor": actor, "transcript_dir": "docs/agent-reviews", "instructions": instructions,
        "developer": {"executable": "%USERPROFILE%/.local/bin/claude.exe", "config_dir": config_dir,
                      **POLICY["developer"], "soft_deadline_minutes": 480, "grace_minutes": 60,
                      "permissions": {"allow": [r for cmd in args.allow for r in (f"Bash({cmd}*)", f"PowerShell({cmd}*)")],
                                      "deny": []}},
        "reviewer": {**shipped["reviewer"], **POLICY["reviewer"]},
        "limits": shipped["limits"], "runtime_lock_paths": [],
    }
    if args.push_command:
        cfg["push_command"] = json.loads(args.push_command)
    check_policy(cfg)
    atomic_write_json(target, cfg)
    return target, cfg


# --------------------------------------------------------------------------- CLI

def find_enrollment(value):
    if not re.fullmatch(r"[a-z0-9][a-z0-9-]*", value or ""):
        raise UsageError("pass an enrollment ID (as printed by enroll), not a path")
    folder = agent_home() / "registry"
    for entry in folder.glob("*.json") if folder.exists() else []:
        data = read_json(entry)
        if data.get("enrollment_id") == value:
            return Path(data["dir"])
    raise UsageError(f"enrollment {value} is not registered under {folder}")


def guarded(controller, command, fn, mutate=True, honor_stop=False):
    """Run fn under the single-writer lock with reloaded state and the pre-mutation guard."""
    with controller.lock(command):
        controller.reload()
        try:
            if mutate:
                controller.verify_inputs()
            if honor_stop:
                controller.check_stop_request()
            result = fn()
            print(json.dumps({"result": result, **controller.status()}, indent=2))
            return EXIT_OK
        except UsageError as exc:
            controller.state.setdefault("notes", []).append(f"{now_iso()} {command} refused: {exc}")
            controller.save()
            raise
        except (Stop, CommandError, subprocess.TimeoutExpired, OSError) as exc:
            stop = exc if isinstance(exc, Stop) else Stop("PAUSED", f"operational failure during {command}: {exc}")
            controller.state["status"], controller.state["status_reason"] = stop.status, stop.reason
            controller.state.setdefault("transitions", []).append(
                {"at": now_iso(), "from": controller.state.get("phase"), "to": controller.state.get("phase"),
                 "why": f"{stop.status}: {stop.reason}"})
            controller.save()
            controller.publish_status(stop.status, stop.reason)
            print(json.dumps({"stopped": stop.status, "reason": stop.reason, **controller.status()}, indent=2))
            return EXIT_BLOCKED if stop.status == "BLOCKED" else EXIT_PAUSED


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    sub = parser.add_subparsers(dest="command", required=True)
    p = sub.add_parser("install", help="copy this workflow version to a pinned location with a manifest")
    p.add_argument("--dest", required=True)
    p = sub.add_parser("preflight", help="real bounded calls to the configured developer and reviewer models")
    p.add_argument("--project", required=True)
    p.add_argument("--out")
    p = sub.add_parser("init-project", help="onboard a repository: write ~/.agent-review/projects/<name>.json")
    p.add_argument("--repo", required=True)
    p.add_argument("--name", required=True)
    p.add_argument("--branch-prefix", default="claude/")
    p.add_argument("--allow", action="append", default=[],
                   help="command prefix the headless developer may run (e.g. 'npm test'); repeatable")
    p.add_argument("--push-command", help="JSON argv for a repository-owned guarded push wrapper")
    p.add_argument("--claude-config-dir")
    p.add_argument("--force", action="store_true")
    p = sub.add_parser("push", help="developer helper: guarded push of the enrolled branch (no force)")
    p.add_argument("--enrollment", required=True)
    p = sub.add_parser("enroll", help="enroll a branch, draft PR, mission and plan")
    p.add_argument("--project", required=True)
    p.add_argument("--worktree", required=True)
    p.add_argument("--plan", required=True)
    p.add_argument("--mission", required=True)
    p.add_argument("--mode", choices=["headless", "foreground"], default="headless")
    p.add_argument("--pr", type=int)
    p.add_argument("--title")
    p.add_argument("--target")
    p.add_argument("--max-rounds", type=int)
    p.add_argument("--state-root")
    for name, help_text in (("run", "headless loop: develop, review, correct, finalize"),
                            ("resume", "continue after a stop or a superseded handoff, after owner action"),
                            ("status", "show enrollment status"), ("stop", "request a safe stop at the next boundary"),
                            ("activate", "accept a reviewed new workflow version for this enrollment"),
                            ("review", "foreground: submit a developer report and run one review round"),
                            ("finalize", "archive records, check readiness, post owner handoff"),
                            ("archive", "commit pending transcript records (transcript-only commit)")):
        p = sub.add_parser(name, help=help_text)
        p.add_argument("--enrollment", required=True)
        if name == "review":
            p.add_argument("--report", required=True)
        if name == "stop":
            p.add_argument("--reason", default="owner request")
        if name == "status":
            p.add_argument("--remote", action="store_true", help="also check whether a handoff is still current")
        if name == "resume":
            p.add_argument("--max-rounds", type=int)
            p.add_argument("--reason", required=True, help="what the owner changed or decided")
    args = parser.parse_args(argv)
    try:
        if args.command == "install":
            print(json.dumps({"installed": str(Path(args.dest).resolve()),
                              "fingerprint": install(Path(args.dest))}, indent=2))
            return EXIT_OK
        if args.command == "preflight":
            result = preflight(load_project(args.project))
            text = json.dumps(result, indent=2)
            if args.out:
                write_text(args.out, text + "\n")
            print(text)
            return EXIT_OK if result["developer"]["ok"] and result["reviewer"]["ok"] else EXIT_BLOCKED
        if args.command == "init-project":
            path, cfg = init_project(args)
            print(json.dumps({"project": cfg["project_id"], "config": str(path), "repository": cfg["repository"],
                              "target": cfg["default_target_branch"], "branch_pattern": cfg["branch_pattern"],
                              "push": "repository wrapper" if cfg.get("push_command") else "built-in guarded push"},
                             indent=2))
            return EXIT_OK
        if args.command == "push":
            controller = Controller(find_enrollment(args.enrollment))
            head = guarded_push(controller.state, controller.cfg)
            print(json.dumps({"pushed": head, "branch": controller.state["branch"]}, indent=2))
            return EXIT_OK
        if args.command == "enroll":
            folder, state = enroll(args)
            print(json.dumps({"enrollment": state["enrollment_id"], "dir": str(folder), "pr": state["pr"]["url"],
                              "mode": state["mode"]}, indent=2))
            return EXIT_OK
        controller = Controller(find_enrollment(args.enrollment))
        if args.command == "status":
            print(json.dumps(controller.status(remote=args.remote), indent=2))
            return EXIT_OK
        if args.command == "stop":
            write_text(controller.stop_path, f"{args.reason} ({now_iso()})\n")
            print(json.dumps({"stop_requested": True, "note": "the controller pauses at the next safe boundary; an "
                              "active developer invocation finishes its own transaction first"}, indent=2))
            return EXIT_OK
        if args.command == "activate":
            with controller.lock("activate"):
                controller.reload()
                new = workflow_fingerprint(controller.wf)
                old = controller.state["workflow"]["fingerprint"]
                controller.state["workflow"] = {"fingerprint": new, "root": str(controller.wf),
                                                "activated_at": now_iso(), "previous": old}
                controller.transition(controller.state["phase"], f"workflow activated {short(old)} -> {short(new)}")
            print(json.dumps({"activated": new, "previous": old}, indent=2))
            return EXIT_OK
        if args.command == "resume":
            return guarded(controller, "resume", lambda: controller.resume(args.reason, args.max_rounds))
        if controller.state["status"] in ("BLOCKED", "PAUSED", "AWAITING_OWNER_REVIEW") and args.command != "archive":
            raise UsageError(f"enrollment is {controller.state['status']}: {controller.state.get('status_reason')}; "
                             "use `resume --reason ...` after addressing it")
        if args.command == "run":
            return guarded(controller, "run", controller.run_loop)
        if args.command == "review":
            if controller.state["mode"] != "foreground":
                raise UsageError("headless enrollments are driven by `run`")

            def _review():
                if controller.state["phase"] == "review":
                    result = controller.review_round()
                elif controller.state["phase"] == "develop":
                    error = controller.accept_report(read_json(args.report))
                    if error:
                        raise UsageError(error)
                    controller.transition("review", "developer submitted a checkpoint report")
                    result = controller.review_round()
                else:
                    raise UsageError(f"phase is {controller.state['phase']}; nothing to review")
                if result == "needs_report":
                    raise Stop("PAUSED", "the submitted report does not match the current candidate; submit again")
                return result
            return guarded(controller, "review", _review, honor_stop=True)
        if args.command == "finalize":
            def _finalize():
                if controller.state["phase"] != "finalize":
                    raise UsageError(f"phase is {controller.state['phase']}; finalize follows a ready review")
                return controller.finalize()
            return guarded(controller, "finalize", _finalize, honor_stop=True)
        if args.command == "archive":
            return guarded(controller, "archive", lambda: controller.archive("manual"))
    except UsageError as exc:
        print(json.dumps({"refused": str(exc)}, indent=2), file=sys.stderr)
        return EXIT_USAGE
    return EXIT_USAGE


if __name__ == "__main__":
    sys.exit(main())
