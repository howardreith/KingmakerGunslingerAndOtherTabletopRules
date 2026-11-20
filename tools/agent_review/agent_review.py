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
POSITIVE_ASSESSMENTS = {"fix_verified", "rebuttal_accepted", "deferral_accepted"}
DECISIONS_BY_TYPE = {
    "intermediate": {"changes_required", "checkpoint_passed", "blocked"},
    "final": {"changes_required", "ready_for_owner_review", "blocked"},
}
CLAUDE_SESSION_ENV = (
    "CLAUDE_CODE_CHILD_SESSION", "CLAUDE_CODE_SESSION_ID", "CLAUDE_CODE_MESSAGING_SOCKET",
    "CLAUDE_CODE_MESSAGING_TOKEN", "CLAUDE_CODE_CHILD_NOTES_DIR", "CLAUDE_CODE_SESSION_ATTENDED",
    "CLAUDE_CODE_ENTRYPOINT", "CLAUDE_CODE_EXECPATH",
)
USAGE_LIMIT_RE = re.compile(r"usage limit|rate.?limit(ed)? (reached|exceeded)|quota|insufficient credits|model .*not (available|supported|found)", re.I)
TOKEN_RE = re.compile(r"gh[pousr]_[A-Za-z0-9]{36,}|github_pat_[A-Za-z0-9_]{60,}|sk-[A-Za-z0-9_-]{32,}|-----BEGIN [A-Z ]*PRIVATE KEY-----")


# --------------------------------------------------------------------------- errors

class Stop(Exception):
    """A visible workflow stop: BLOCKED (impediment) or PAUSED (resumable)."""

    def __init__(self, status: str, reason: str):
        assert status in ("BLOCKED", "PAUSED")
        super().__init__(f"{status}: {reason}")
        self.status, self.reason = status, reason


class UsageError(Exception):
    """Refused before any state change (bad input, wrong target, lock held)."""


class CommandError(Exception):
    def __init__(self, argv, proc):
        tail = (proc.stderr or proc.stdout or "").strip()[-800:]
        super().__init__(f"{Path(str(argv[0])).name} {' '.join(map(str, argv[1:3]))} exited {proc.returncode}: {tail}")
        self.returncode, self.stderr = proc.returncode, proc.stderr


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


def sanitize(text: str):
    """Redact credentials and local home paths from publishable text."""
    count = 0

    def _redact(match):
        nonlocal count
        count += 1
        return "[REDACTED-CREDENTIAL]"

    text = TOKEN_RE.sub(_redact, text)
    home = str(Path.home())
    for variant in {home, home.replace("\\", "/"), home.replace("\\", "\\\\")}:
        text = re.sub(re.escape(variant), "~", text, flags=re.I)
    return text, count


def short(sha) -> str:
    return (sha or "")[:10]


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
            expected = _TYPES[name]
            if name == "integer" and isinstance(value, bool):
                continue
            if isinstance(value, expected):
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
    behavioral = {rel: h for rel, h in hashes.items() if not rel.startswith(("projects/", "plans/"))}
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
        return self("status", "--porcelain") == ""

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


def same_path(a, b) -> bool:
    return os.path.normcase(os.path.abspath(str(a))) == os.path.normcase(os.path.abspath(str(b)))


ARCHIVE_NAME_RE = r"\d{3}-[a-z0-9][a-z0-9-]*\.(md|json)"


def check_archive_only(git: Git, source: str, archive: str, transcript_dir: str):
    """True only if source..archive adds new inert transcript records and nothing else."""
    if source == archive:
        return True, []
    if not git.is_ancestor(source, archive):
        return False, [f"{short(source)} is not an ancestor of {short(archive)}"]
    pattern = re.compile(rf"^{re.escape(transcript_dir.strip('/'))}/pr-\d+/{ARCHIVE_NAME_RE}$")
    reasons = _additions_only(git, source, archive, pattern)
    # Every intermediate commit must also be a pure addition, so a record added and
    # later edited inside the range cannot pass as a net addition. Merges are refused.
    for line in git("rev-list", "--reverse", "--parents", f"{source}..{archive}").splitlines():
        ids = line.split()
        if len(ids) != 2:
            reasons.append(f"{short(ids[0])} is a merge or root commit")
            continue
        reasons += [f"{short(ids[0])}: {r}" for r in _additions_only(git, ids[1], ids[0], pattern)]
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
            reasons.append(f"{path}: outside the transcript record pattern")
    return reasons


# --------------------------------------------------------------------------- GitHub boundary

class GitHub:
    """gh CLI boundary. Bodies always travel in files; never through a shell."""

    def __init__(self, repo: str, gh: str = "gh"):
        self.repo, self.gh = repo, resolve_exe(gh)

    def _json(self, *args):
        return json.loads(run([self.gh, *args], timeout=180).stdout or "null")

    def viewer(self) -> str:
        return self._json("api", "user")["login"]

    def pr(self, number: int) -> dict:
        return self._json("pr", "view", str(number), "--repo", self.repo, "--json",
                          "number,url,state,isDraft,headRefName,headRefOid,baseRefName,author,reviewRequests,body")

    def create_draft_pr(self, head, base, title, body_file) -> int:
        out = run([self.gh, "pr", "create", "--repo", self.repo, "--draft", "--head", head, "--base", base,
                   "--title", title, "--body-file", str(body_file)], timeout=180).stdout.strip()
        match = re.search(r"/pull/(\d+)", out)
        if not match:
            raise CommandError(["gh", "pr", "create"], subprocess.CompletedProcess([], 1, out, ""))
        return int(match.group(1))

    def edit_body(self, number, body_file):
        run([self.gh, "pr", "edit", str(number), "--repo", self.repo, "--body-file", str(body_file)], timeout=180)

    def comments(self, number):
        out = run([self.gh, "api", "--paginate", f"repos/{self.repo}/issues/{number}/comments",
                   "--jq", ".[] | {id: .id, user: .user.login, body: .body, url: .html_url} | @json"], timeout=300).stdout
        return [json.loads(line) for line in out.splitlines() if line.strip()]

    def post_comment(self, number, body_file) -> dict:
        data = self._json("api", f"repos/{self.repo}/issues/{number}/comments", "-F", f"body=@{body_file}")
        return {"id": data["id"], "url": data["html_url"]}

    def checks(self, number):
        proc = run([self.gh, "pr", "checks", str(number), "--repo", self.repo, "--json", "name,state,bucket,link"],
                   check=False, timeout=180)
        if proc.returncode != 0 and "no checks" in (proc.stderr + proc.stdout).lower():
            return []
        if proc.returncode not in (0, 8):  # 8 = checks pending
            raise CommandError(["gh", "pr", "checks"], proc)
        return json.loads(proc.stdout or "[]")


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
                sandbox = (payload.get("sandbox_policy") or {}).get("type")
                info["sandbox"] = sandbox or info["sandbox"]
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
        info["turn_models"] = sorted({m for m in info["turn_models"] if m})
        info["turn_efforts"] = sorted({e for e in info["turn_efforts"] if e})
        return info


# --------------------------------------------------------------------------- developer boundary

class ClaudeDeveloper:
    """Headless Claude Code boundary: explicit model/effort on every launch and resume."""

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

    def session_exists(self, session_id) -> bool:
        return bool(self._transcripts(session_id))

    def _transcripts(self, session_id):
        base = (self.config_dir or Path.home() / ".claude") / "projects"
        return list(base.glob(f"*/{session_id}.jsonl")) if base.exists() else []

    def argv(self, session_id, resume):
        schema = json.dumps(strict_schema_copy(read_json(self.schema_path)), separators=(",", ":"))
        args = [self.exe, "-p", "--model", self.cfg["model"], "--effort", self.cfg["effort"]]
        args += ["--resume", session_id] if resume else ["--session-id", session_id]
        return args + ["--output-format", "stream-json", "--verbose", "--permission-mode", "dontAsk",
                       "--settings", str(self.settings_path), "--strict-mcp-config", "--json-schema", schema]

    def run(self, prompt, session_id, resume, cwd, log_path, stderr_path, soft_deadline_s, on_start=None):
        started = time.monotonic()
        with open(log_path, "w", encoding="utf-8") as out, open(stderr_path, "w", encoding="utf-8") as err:
            flags = subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0
            proc = subprocess.Popen(self.argv(session_id, resume), cwd=cwd, env=self.env(), stdin=subprocess.PIPE,
                                    stdout=out, stderr=err, text=True, encoding="utf-8", creationflags=flags)
            if on_start:
                on_start(proc.pid)
            proc.stdin.write(prompt)
            proc.stdin.close()
            deadline_exceeded = False
            while True:
                try:
                    proc.wait(timeout=30)
                    break
                except subprocess.TimeoutExpired:
                    # Soft deadline: never kill the developer; it may own a runtime restoration.
                    if time.monotonic() - started > soft_deadline_s:
                        deadline_exceeded = True
                except KeyboardInterrupt:
                    continue  # finish the child's own transaction; use `stop` to pause at the boundary
        parsed = self.parse(log_path)
        parsed.update({"exit_code": proc.returncode, "deadline_exceeded": deadline_exceeded,
                       "duration_s": round(time.monotonic() - started, 1),
                       "observed_effort": self.observed_effort(session_id)})
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
                if model and model not in out["main_models"]:
                    out["main_models"].append(model)
            elif obj.get("type") == "result":
                out.update(subtype=obj.get("subtype"), is_error=bool(obj.get("is_error")),
                           structured_output=obj.get("structured_output"), result=obj.get("result"),
                           permission_denials=obj.get("permission_denials") or [],
                           model_usage=obj.get("modelUsage") or {}, cost_usd=obj.get("total_cost_usd"))
        return out

    def observed_effort(self, session_id):
        values = set()
        for path in self._transcripts(session_id):
            text = path.read_text(encoding="utf-8", errors="replace")
            values.update(re.findall(r'"(?:effort|perTurnEffort)":"([a-z]+)"', text))
        return sorted(values)


# --------------------------------------------------------------------------- lock

class Lock:
    def __init__(self, path: Path, command: str):
        self.path, self.command, self.held = path, command, False

    def __enter__(self):
        record = {"pid": os.getpid(), "host": socket.gethostname(), "started": now_iso(), "command": self.command}
        for _ in range(2):
            try:
                fd = os.open(self.path, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
            except FileExistsError:
                try:
                    holder = read_json(self.path)
                except (ValueError, OSError):
                    holder = {}
                if holder.get("host") == socket.gethostname() and not pid_alive(holder.get("pid")):
                    self.path.unlink(missing_ok=True)  # stale lock from a dead controller on this host
                    continue
                raise UsageError(f"enrollment is locked by pid {holder.get('pid')} on {holder.get('host')} "
                                 f"({holder.get('command')}, since {holder.get('started')}); one writer at a time")
            with os.fdopen(fd, "w", encoding="utf-8") as handle:
                json.dump(record, handle)
            self.held = True
            return self
        raise UsageError("could not acquire enrollment lock")

    def __exit__(self, *exc):
        if self.held:
            self.path.unlink(missing_ok=True)
            self.held = False


# --------------------------------------------------------------------------- controller

class Controller:
    def __init__(self, enrollment_dir, github=None, reviewer=None, developer=None, sleep=time.sleep,
                 workflow_root: Path = HERE):
        self.dir = Path(enrollment_dir)
        self.state_path = self.dir / "state.json"
        if not self.state_path.exists():
            raise UsageError(f"no enrollment at {self.dir}")
        self.state = read_json(self.state_path)
        if self.state.get("schema_version") != STATE_SCHEMA:
            raise UsageError("unsupported enrollment state version")
        self.cfg = self.state["project"]
        self.wf = workflow_root
        self.git = Git(self.state["repo_root"])
        self._gh, self._reviewer, self._developer = github, reviewer, developer
        self.sleep = sleep
        self.stop_path = self.dir / "STOP"
        self._viewer = None

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
            self._developer = ClaudeDeveloper(self.cfg["developer"], self.wf / self.cfg["developer"]["settings"],
                                              self.wf / REPORT_SCHEMA_FILE)
        return self._developer

    # -- state ---------------------------------------------------------------
    def save(self):
        self.state["updated_at"] = now_iso()
        atomic_write_json(self.state_path, self.state)

    def transition(self, to, why):
        self.state.setdefault("transitions", []).append(
            {"at": now_iso(), "from": self.state.get("phase"), "to": to, "why": why})
        self.state["phase"] = to
        self.save()

    @property
    def checkpoint(self):
        plan = self.state["plan"]["checkpoints"]
        return plan[min(self.state["checkpoint_index"], len(plan) - 1)]

    @property
    def limits(self):
        return self.state["limits"]

    def lock(self, command):
        return Lock(self.dir / "controller.lock", command)

    # -- identity checks -----------------------------------------------------
    def viewer(self):
        if self._viewer is None:
            self._viewer = self.gh.viewer()
        return self._viewer

    def verify_inputs(self):
        fingerprint = workflow_fingerprint(self.wf)
        if fingerprint != self.state["workflow"]["fingerprint"]:
            raise Stop("PAUSED", f"workflow version changed ({short(self.state['workflow']['fingerprint'])} -> "
                                 f"{short(fingerprint)}); review it, then run `activate`")
        if not same_path(self.git.toplevel(), self.state["repo_root"]):
            raise UsageError("developer worktree root moved")
        if self.git.branch() != self.state["branch"]:
            raise Stop("PAUSED", f"worktree is on {self.git.branch()}, not the enrolled {self.state['branch']}")
        if self.git.remote_url() not in self.cfg["allowed_remote_urls"]:
            raise UsageError("origin is not an approved remote for this project")
        if self.viewer() != self.cfg["expected_actor"]:
            raise UsageError(f"GitHub actor {self.viewer()} is not the expected {self.cfg['expected_actor']}")
        pr = self.pr_view()
        mission = self.state["mission"]
        if mission.get("path") and Path(mission["path"]).exists() and sha256_file(mission["path"]) != mission["sha256"]:
            raise Stop("BLOCKED", "the enrolled mission file changed; mission changes need an owner decision")
        return pr

    def pr_view(self):
        number = self.state["pr"]["number"]
        pr = self.gh.pr(number)
        if f"/{self.cfg['repository']}/pull/{number}" not in pr["url"]:
            raise UsageError("PR does not belong to the enrolled repository")
        if pr["headRefName"] != self.state["branch"] or pr["baseRefName"] != self.state["target"]:
            raise UsageError("PR head/base do not match the enrollment")
        if pr["state"] != "OPEN":
            raise Stop("PAUSED", f"PR #{number} is {pr['state']}")
        if not pr["isDraft"]:
            raise Stop("PAUSED", f"PR #{number} is no longer a draft; the workflow only operates on drafts")
        return pr

    def capture_candidate(self):
        if not self.git.clean():
            raise Stop("PAUSED", "developer worktree has uncommitted changes; commit and push a coherent checkpoint")
        head = self.git.head()
        remote = self.git.remote_head(self.state["branch"])
        if remote != head:
            raise Stop("PAUSED", f"candidate {short(head)} is not the pushed branch head ({short(remote)}); "
                                 "push through the guarded wrapper first")
        pr = None
        for attempt in range(6):
            pr = self.pr_view()
            if pr["headRefOid"] == head:
                break
            self.sleep(5)
        if pr["headRefOid"] != head:
            raise Stop("PAUSED", f"PR head {short(pr['headRefOid'])} does not match candidate {short(head)}")
        base = self.git.fetch(self.state["target"])
        merge_base = self.git("merge-base", head, base)
        return {"H": head, "B": base, "M": merge_base}

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
            found = self.reconcile(number, part["marker"])
            attempts = 0
            while not found:
                attempts += 1
                try:
                    found = self.gh.post_comment(number, part["body_file"])
                except Exception as exc:  # lost response or transient failure: reconcile before retrying
                    found = self.reconcile(number, part["marker"])
                    if found:
                        break
                    if attempts >= self.limits["publication_attempts"]:
                        raise Stop("PAUSED", f"publication of {key} failed ({exc}); the preserved message will be "
                                             "reconciled and sent on resume")
                    self.sleep(self.limits["retry_backoff_seconds"][min(attempts - 1, 1)])
            part["comment_id"], part["url"] = found["id"], found["url"]
            self.save()
        record["state"], record["posted_at"] = "posted", now_iso()
        self.save()
        return record

    def reconcile(self, number, marker):
        for comment in self.gh.comments(number):
            body = comment.get("body") or ""
            if body.startswith(marker) and comment.get("user") == self.cfg["expected_actor"]:
                return {"id": comment["id"], "url": comment["url"]}
        return None

    def publish_status(self, status, reason):
        key = f"status-{len(self.state.get('transitions', []))}-{status.lower()}"
        cp = self.checkpoint
        body = (f"### Workflow — {status.lower().replace('_', ' ')} — checkpoint {cp['id']}\n\n"
                f"{reason}\n\n"
                f"- Enrollment: `{self.state['enrollment_id']}`\n"
                f"- Phase: `{self.state.get('phase')}`; rounds completed for this checkpoint: "
                f"{self.state['rounds_completed'].get(cp['id'], 0)}/{self.limits['max_review_rounds_per_checkpoint']}\n"
                f"- Inspect: `python {self.wf / 'agent_review.py'} status --enrollment {self.state['enrollment_id']}`\n\n"
                f"_Posted by the agent-review controller through the owner's GitHub account. Automated status; "
                f"not a human review._\n")
        try:
            self.publish(key, body)
        except Exception as exc:  # status visibility is best effort; state still records it
            self.state.setdefault("notes", []).append(f"{now_iso()} status publication failed: {exc}")
            self.save()

    # -- developer reports -----------------------------------------------------
    def accept_report(self, report):
        """Validate a developer report; returns an error string for the developer, or None."""
        errors = validate_schema(report, read_json(self.wf / REPORT_SCHEMA_FILE))
        if errors:
            return "report does not match agent-review.developer.v1: " + "; ".join(errors[:8])
        cp = self.checkpoint
        if report["checkpoint_id"] != cp["id"]:
            return f"report is for checkpoint {report['checkpoint_id']}, but the active checkpoint is {cp['id']}"
        self.state.setdefault("reports", []).append({"at": now_iso(), "report": report})
        self.state["last_report"] = report
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
        pending = set(self.state.get("pending_findings", []))
        disposed = {d["finding_id"] for d in report["dispositions"]}
        if pending - disposed:
            return "missing dispositions for findings: " + ", ".join(sorted(pending - disposed))
        unknown = disposed - set(self.state.get("findings", {}))
        if unknown:
            return "dispositions reference unknown findings: " + ", ".join(sorted(unknown))
        for item in report["dispositions"]:
            if item["disposition"] == "fixed":
                if not item["commits"]:
                    return f"{item['finding_id']} is marked fixed without a commit"
                for commit in item["commits"]:
                    if not self.git.ok("cat-file", "-e", f"{commit}^{{commit}}") or not self.git.is_ancestor(commit, head):
                        return f"{item['finding_id']} cites {commit}, which is not in the candidate history"
        for item in report["dispositions"]:
            ledger = self.state["findings"][item["finding_id"]]
            ledger.setdefault("dispositions", []).append({"round": self.last_round_no(), **item})
        self.save()
        self.publish_dispositions(report)
        if any(d["disposition"] == "unresolved_blocker" for d in report["dispositions"]):
            raise Stop("BLOCKED", "developer recorded an unresolved blocker; see the disposition comment")
        return None

    def last_round_no(self):
        return self.state["rounds"][-1]["round"] if self.state.get("rounds") else 0

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
        lines.append(f"\n_Model-generated developer response (requested {self.cfg['developer']['model']} / "
                     f"{self.cfg['developer']['effort']}), posted by the agent-review controller through the owner's "
                     f"GitHub account; not a human review._\n")
        self.publish(f"r{n:02d}-dispositions", "\n".join(lines))

    @staticmethod
    def validation_md(report):
        if not report["validation"]:
            return "\n**Validation:** none reported."
        rows = ["\n**Validation (developer-reported):**\n", "| Command | Result | Duration | Scope and limits |",
                "|---|---|---|---|"]
        for v in report["validation"]:
            cells = [v["command"], v["result"], v["duration"], v["scope_and_limits"]]
            rows.append("| " + " | ".join(c.replace("|", "\\|").replace("\n", " ") for c in cells) + " |")
        return "\n".join(rows)

    # -- review round ----------------------------------------------------------
    def review_round(self):
        cp = self.checkpoint
        rnd = self.state.get("active_round")
        if rnd and rnd["status"] == "validated":
            return self.finish_round(rnd)  # publish preserved result without rerunning a successful review
        completed = self.state["rounds_completed"].get(cp["id"], 0)
        if completed >= self.limits["max_review_rounds_per_checkpoint"]:
            raise Stop("BLOCKED", f"review-round cap reached for {cp['id']} "
                                  f"({completed}/{self.limits['max_review_rounds_per_checkpoint']})")
        candidate = self.capture_candidate()
        if not (rnd and rnd["status"] in ("requested", "reviewing") and rnd["H"] == candidate["H"]
                and rnd["checkpoint_id"] == cp["id"]):
            if rnd and rnd["status"] in ("requested", "reviewing"):
                rnd["status"] = "superseded"
            number = len(self.state["rounds"]) + 1
            rnd = {"round": number, "request_id": f"{self.state['enrollment_id']}-r{number:02d}-{uuid.uuid4().hex[:6]}",
                   "checkpoint_id": cp["id"], "checkpoint_type": cp["type"], **candidate,
                   "pending_findings": list(self.state.get("pending_findings", [])),
                   "created_at": now_iso(), "status": "requested", "attempts": []}
            self.state["rounds"].append(rnd)
            self.state["active_round"] = rnd
            self.save()
        self.publish(f"r{rnd['round']:02d}-request", self.request_body(rnd))
        rnd_dir = self.dir / "rounds" / f"r{rnd['round']:02d}"
        rnd_dir.mkdir(parents=True, exist_ok=True)
        rnd["status"] = "reviewing"
        self.save()
        envelope = None
        while len(rnd["attempts"]) < self.limits["max_review_attempts_per_round"]:
            envelope = self.review_attempt(rnd, rnd_dir, len(rnd["attempts"]) + 1)
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
        pr = self.pr_view()
        if pr["headRefOid"] != rnd["H"] or self.git.remote_head(self.state["branch"]) != rnd["H"]:
            rnd["status"] = "historical"
            self.state["active_round"] = None
            self.save()
            self.publish(f"r{rnd['round']:02d}-review", self.review_body(rnd, envelope, stale=True))
            raise Stop("PAUSED", f"branch head moved during review of {short(rnd['H'])}; that review is historical. "
                                 "Only the enrolled writer may push; reconcile and resume")
        rnd["status"], rnd["envelope_path"] = "validated", envelope["path"]
        self.save()
        return self.finish_round(rnd)

    def review_attempt(self, rnd, rnd_dir, n):
        attempt = {"n": n, "started": now_iso()}
        rnd["attempts"].append(attempt)
        self.save()
        snapshot = rnd_dir / f"snapshot-{n}"
        if snapshot.exists():
            self.remove_snapshot(snapshot)
        self.git("worktree", "add", "--detach", str(snapshot), rnd["H"])
        try:
            inputs = self.write_review_inputs(rnd, rnd_dir)
            schema = rnd_dir / "review-schema.json"
            atomic_write_json(schema, strict_schema_copy(read_json(self.wf / REVIEW_SCHEMA_FILE)))
            result_path = rnd_dir / f"result-{n}.json"
            result_path.unlink(missing_ok=True)
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

    def write_review_inputs(self, rnd, rnd_dir):
        exclude = f":(exclude){self.cfg['transcript_dir'].strip('/')}"
        diff = self.git("diff", "--no-ext-diff", rnd["M"], rnd["H"], "--", ".", exclude)
        names = self.git("diff", "--name-status", "--no-renames", rnd["M"], rnd["H"], "--", ".", exclude)
        write_text(rnd_dir / "feature.diff", diff + "\n")
        write_text(rnd_dir / "changed-files.txt", names + "\n")
        ledger = {fid: self.state["findings"][fid] for fid in self.state.get("findings", {})}
        atomic_write_json(rnd_dir / "prior-findings.json", {"pending_for_assessment": rnd["pending_findings"],
                                                            "ledger": ledger})
        atomic_write_json(rnd_dir / "developer-report.json", self.state.get("last_report") or {})
        atomic_write_json(rnd_dir / "request.json", {k: rnd[k] for k in (
            "round", "request_id", "checkpoint_id", "checkpoint_type", "H", "B", "M")})
        return {"diff": rnd_dir / "feature.diff", "names": rnd_dir / "changed-files.txt",
                "prior": rnd_dir / "prior-findings.json", "report": rnd_dir / "developer-report.json",
                "diff_lines": diff.count("\n") + 1}

    def validate_review(self, rnd, result_path, info, prov, stderr_path):
        reasons, fatal = [], False
        requested = self.reviewer.requested
        result = None
        raw = b""
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
                result = json.loads(raw.decode("utf-8"))
            except ValueError as exc:
                reasons.append(f"malformed JSON: {exc}")
        if result is not None:
            errors = validate_schema(result, read_json(self.wf / REVIEW_SCHEMA_FILE))
            reasons += [f"schema: {e}" for e in errors[:12]]
            if not errors:
                for key, ours in (("request_id", rnd["request_id"]), ("round", rnd["round"]),
                                  ("checkpoint_id", rnd["checkpoint_id"]), ("checkpoint_type", rnd["checkpoint_type"]),
                                  ("candidate_commit", rnd["H"])):
                    if result[key] != ours:
                        reasons.append(f"echoed {key} {result[key]!r} does not match controller value {ours!r}")
                if result["decision"] not in DECISIONS_BY_TYPE[rnd["checkpoint_type"]]:
                    reasons.append(f"decision {result['decision']} is not valid for a {rnd['checkpoint_type']} checkpoint")
                blocking = [f for f in result["findings"] if f["blocks_readiness"]]
                if result["decision"] in ("checkpoint_passed", "ready_for_owner_review") and blocking:
                    reasons.append("a passing decision lists findings that block readiness")
                if result["decision"] == "changes_required" and not blocking:
                    reasons.append("changes_required without any finding that blocks readiness")
                ids = [f["id"] for f in result["findings"]]
                if len(ids) != len(set(ids)):
                    reasons.append("duplicate finding IDs")
                assessed = {a["finding_id"] for a in result["prior_finding_assessments"]}
                missing = set(rnd["pending_findings"]) - assessed
                if missing:
                    reasons.append("prior findings not assessed: " + ", ".join(sorted(missing)))
        if not prov["rollout_found"]:
            reasons.append("reviewer provenance unavailable (no session rollout for the thread)")
        else:
            if prov["observed_model"] != requested["model"] or any(m != requested["model"] for m in prov["turn_models"]):
                reasons.append(f"wrong reviewer model: observed {prov['observed_model']} {prov['turn_models']}, "
                               f"required {requested['model']}")
                fatal = True
            if prov["observed_effort"] != requested["effort"]:
                reasons.append(f"reviewer effort {prov['observed_effort']} != required {requested['effort']}")
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

    def finish_round(self, rnd):
        envelope = read_json(rnd["envelope_path"])
        result = envelope["result"]
        record = self.publish(f"r{rnd['round']:02d}-review", self.review_body(rnd, envelope))
        envelope["published"] = [{"comment_id": p["comment_id"], "url": p["url"]} for p in record["parts"]]
        atomic_write_json(rnd["envelope_path"], envelope)
        cp = self.checkpoint
        self.state["rounds_completed"][cp["id"]] = self.state["rounds_completed"].get(cp["id"], 0) + 1
        ledger = self.state.setdefault("findings", {})
        for a in result["prior_finding_assessments"]:
            if a["finding_id"] in ledger:
                ledger[a["finding_id"]].setdefault("assessments", []).append({"round": rnd["round"], **a})
                ledger[a["finding_id"]]["status"] = "resolved" if a["assessment"] in POSITIVE_ASSESSMENTS else "open"
        for f in result["findings"]:
            entry = ledger.setdefault(f["id"], {"first_round": rnd["round"], "rounds_open": 0})
            entry.update({k: f[k] for k in ("title", "priority", "category", "blocks_readiness")})
            entry.update(status="open", last_round=rnd["round"])
        for fid, entry in ledger.items():
            entry["rounds_open"] = entry.get("rounds_open", 0) + 1 if entry["status"] == "open" else 0
        open_ids = sorted(fid for fid, e in ledger.items() if e["status"] == "open")
        rnd.update(status="accepted", decision=result["decision"], open_findings=open_ids,
                   response_sha256=envelope["response_sha256"])
        self.state["active_round"] = None
        self.state["pending_findings"] = open_ids
        decision = result["decision"]
        self.save()
        if decision == "changes_required":
            previous = [r for r in self.state["rounds"] if r.get("status") == "accepted" and r is not rnd
                        and r["checkpoint_id"] == rnd["checkpoint_id"]]
            blocking_now = sorted(fid for fid in open_ids if ledger[fid]["blocks_readiness"])
            if previous and previous[-1]["H"] == rnd["H"] and sorted(
                    fid for fid in previous[-1].get("open_findings", []) if ledger[fid]["blocks_readiness"]) == blocking_now:
                raise Stop("BLOCKED", "no progress: the same blocking findings remain on an unchanged candidate")
            stuck = [fid for fid in blocking_now if ledger[fid]["rounds_open"] >= self.limits["no_progress_rounds"]]
            if stuck:
                raise Stop("BLOCKED", f"no progress: {', '.join(stuck)} open for "
                                      f"{self.limits['no_progress_rounds']} consecutive reviews")
            self.transition("develop", f"round {rnd['round']} requires changes")
        elif decision == "checkpoint_passed":
            self.state["checkpoint_index"] += 1
            self.transition("develop", f"checkpoint {cp['id']} passed; advancing to {self.checkpoint['id']}")
        elif decision == "ready_for_owner_review":
            self.state["ready_review"] = {"round": rnd["round"], "H": rnd["H"], "B": rnd["B"], "M": rnd["M"],
                                          "summary": result["summary"]}
            self.transition("finalize", f"round {rnd['round']} ready for owner review")
        else:
            raise Stop("BLOCKED", f"reviewer blocked: {result['decision_rationale']}")
        return decision

    # -- bodies ------------------------------------------------------------------
    def request_body(self, rnd):
        report = self.state.get("last_report") or {}
        prov = self.cfg["reviewer"]
        lines = [f"### Claude Opus — review request — round {rnd['round']} — candidate `{short(rnd['H'])}`\n",
                 "| Field | Value |", "|---|---|",
                 f"| Request ID | `{rnd['request_id']}` |",
                 f"| Checkpoint | `{rnd['checkpoint_id']}` ({rnd['checkpoint_type']}) |",
                 f"| Candidate H | `{rnd['H']}` |", f"| Target B (`{self.state['target']}`) | `{rnd['B']}` |",
                 f"| Merge base M | `{rnd['M']}` |",
                 f"| Mission | `{self.state['plan']['mission_id']}` sha256 `{short(self.state['mission']['sha256'])}` |",
                 f"| Workflow | `{short(self.state['workflow']['fingerprint'])}` |",
                 f"| Reviewer requested | Codex `{prov['model']}`, effort `{prov['effort']}`, read-only sandbox |",
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

    def review_body(self, rnd, envelope, stale=False):
        result = envelope["result"]
        prov = envelope["invocation"]["observed"]
        heading = (f"### Codex GPT-6.2 Astra — review — round {rnd['round']} — candidate `{short(rnd['H'])}`"
                   + (" — HISTORICAL (branch head moved during review)" if stale else ""))
        lines = [heading, "", f"**Decision:** `{result['decision']}` — {result['decision_rationale']}", ""]
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
                  f"| Model | `{envelope['invocation']['requested']['model']}` | `{prov['observed_model']}` |",
                  f"| Reasoning effort | `{envelope['invocation']['requested']['effort']}` | `{prov['observed_effort']}` |",
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
                       "transcript_dir": self.cfg["transcript_dir"],
                       "pending_findings": ", ".join(rnd["pending_findings"]) or "none"})
        return fields

    # -- headless developer loop ---------------------------------------------------------
    def develop_step(self, correction=None):
        dev = self.state["developer"]
        if dev["invocations"] >= self.limits["max_developer_invocations"]:
            raise Stop("BLOCKED", f"developer invocation budget exhausted ({dev['invocations']})")
        if pid_alive(dev.get("active_pid")):
            raise Stop("PAUSED", f"developer session {dev['session_id']} is still running (pid {dev['active_pid']})")
        fields = self.common_fields()
        fields.update({"stop_file": str(self.stop_path), "worktree": self.state["repo_root"],
                       "push_command": " ".join(self.cfg["push_command"]).replace("{repo_root}", self.state["repo_root"]),
                       "transcript_dir": self.cfg["transcript_dir"], "correction": correction or "",
                       "enrollment_id": self.state["enrollment_id"]})
        if dev.get("interrupted"):
            template, fields["situation"] = "developer-continue.md", (
                "Your previous invocation was interrupted before it returned a report. Reconcile the worktree "
                "(do not discard anything), complete or restore any runtime transaction, then continue.")
        elif self.state.get("pending_findings") and self.state["rounds"]:
            n = self.last_round_no()
            fields["review_path"] = str(self.dir / "rounds" / f"r{n:02d}")
            template, fields["situation"] = "developer-continue.md", (
                f"Review round {n} returned findings. The full structured review is in "
                f"{self.state['rounds'][-1].get('envelope_path')} (field `result`). Dispose of every pending finding: "
                + ", ".join(self.state["pending_findings"]))
        elif dev["started"]:
            template, fields["situation"] = "developer-continue.md", (
                "The previous checkpoint passed independent review. Continue with the active checkpoint below.")
        else:
            template, fields["situation"] = "developer-start.md", ""
        if correction:
            template = "developer-continue.md"
            fields["situation"] = f"The controller rejected your last report: {correction}. Fix that and report again."
        prompt = self.render(template, fields)
        n = dev["invocations"] + 1
        log_dir = self.dir / "developer"
        log_dir.mkdir(parents=True, exist_ok=True)
        write_text(log_dir / f"prompt-{n:03d}.md", prompt)
        resume = dev["started"] and self.developer.session_exists(dev["session_id"])
        dev["interrupted"] = True  # cleared only when the invocation returns
        self.save()

        def on_start(pid):
            dev["active_pid"] = pid
            self.save()

        out = self.developer.run(prompt, dev["session_id"], resume, self.state["repo_root"],
                                 log_dir / f"stream-{n:03d}.jsonl", log_dir / f"stderr-{n:03d}.txt",
                                 self.cfg["developer"]["soft_deadline_minutes"] * 60, on_start)
        dev.update(active_pid=None, started=True, interrupted=False, invocations=n)
        dev.setdefault("log", []).append({k: out.get(k) for k in (
            "init_model", "main_models", "observed_effort", "api_key_source", "cost_usd", "model_usage",
            "permission_denials", "exit_code", "duration_s", "deadline_exceeded", "subtype")})
        self.save()
        if out["init_model"] is None:
            tail = (log_dir / f"stderr-{n:03d}.txt").read_text(encoding="utf-8", errors="replace").strip()[-400:]
            raise Stop("BLOCKED", f"developer invocation {n} did not start a session (exit {out['exit_code']}): {tail}")
        allowed = re.compile(self.cfg["developer"]["allowed_model_pattern"])
        models = [out["init_model"], *out["main_models"]]
        if any(not m or not allowed.match(m) for m in models):
            raise Stop("BLOCKED", f"developer ran on {models}, not an allowed {self.cfg['developer']['model']} model")
        efforts = out["observed_effort"]
        if efforts and efforts != [self.cfg["developer"]["effort"]]:
            raise Stop("BLOCKED", f"developer effort observed {efforts}, required {self.cfg['developer']['effort']}")
        if out["deadline_exceeded"]:
            self.state.setdefault("notes", []).append(f"{now_iso()} developer invocation {n} exceeded its soft deadline")
        if out["is_error"] or not isinstance(out["structured_output"], dict):
            return f"invocation {n} ended without a structured report ({out['subtype']}); return the report"
        error = self.accept_report(out["structured_output"])
        if out["deadline_exceeded"] and not error:
            raise Stop("PAUSED", f"developer invocation {n} exceeded its soft deadline; review progress before resuming")
        return error

    def resume(self, reason, max_rounds=None):
        """Owner-initiated continuation after a stop; counters and session persist."""
        if self.state["status"] == "AWAITING_OWNER_REVIEW":
            raise UsageError("enrollment already handed off for owner review")
        self.stop_path.unlink(missing_ok=True)
        active = self.state.get("active_round")
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
        if self.state["phase"] in ("review", "finalize"):
            return self.review_round() if self.state["phase"] == "review" else self.finalize()
        return "ACTIVE (foreground: continue with `review --report`)"

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

    # -- archive and readiness ---------------------------------------------------------
    def archive(self, reason):
        if not self.git.clean():
            raise Stop("PAUSED", "cannot archive records: developer worktree has uncommitted changes")
        head = self.git.head()
        if self.git.remote_head(self.state["branch"]) != head:
            raise Stop("PAUSED", "cannot archive records: local branch is not the pushed head")
        rel_dir = f"{self.cfg['transcript_dir'].strip('/')}/pr-{self.state['pr']['number']}"
        folder = Path(self.state["repo_root"]) / rel_dir
        archived = self.state.setdefault("archived", {})
        for rel, digest in archived.items():
            path = Path(self.state["repo_root"]) / rel
            if not path.exists() or sha256_file(path) != digest:
                raise Stop("BLOCKED", f"archived record {rel} was modified or removed; records are append-only")
        existing = [int(p.name[:3]) for p in folder.glob("[0-9][0-9][0-9]-*")] if folder.exists() else []
        seq = max(existing, default=0) + 1
        written = []
        archived_keys = set(self.state.setdefault("archived_keys", []))
        pubs = sorted(self.state.get("publications", {}).values(), key=lambda p: p["seq"])
        for pub in pubs:
            if pub["state"] != "posted" or pub["key"] in archived_keys or pub["key"].startswith("final-"):
                continue
            body = "\n\n---\n\n".join(Path(p["body_file"]).read_text(encoding="utf-8") for p in pub["parts"])
            written.append((f"{seq:03d}-{pub['key']}.md", body, pub["key"]))
            seq += 1
            if pub["key"].endswith("-review"):
                n = int(pub["key"][1:3])
                rnd = next((r for r in self.state["rounds"] if r["round"] == n and r.get("envelope_path")), None)
                if rnd:
                    env_text, _ = sanitize(json.dumps(read_json(rnd["envelope_path"]), indent=2, sort_keys=True))
                    written.append((f"{seq:03d}-r{n:02d}-envelope.json", env_text + "\n", f"r{n:02d}-envelope"))
                    seq += 1
        if not written:
            return head
        paths = []
        for name, text, key in written:
            path = folder / name
            if path.exists():
                raise Stop("BLOCKED", f"archive record {name} already exists")
            write_text(path, text)
            paths.append(f"{rel_dir}/{name}")
        self.git("add", "--", *paths)
        self.git("commit", "--quiet", "-m", f"Archive agent review records for PR #{self.state['pr']['number']} ({reason})")
        new_head = self.git.head()
        ok, reasons = check_archive_only(self.git, head, new_head, self.cfg["transcript_dir"])
        if not ok:
            raise Stop("BLOCKED", "archive commit failed its own transcript-only check: " + "; ".join(reasons))
        argv = [a.replace("{repo_root}", self.state["repo_root"]).replace("{branch}", self.state["branch"])
                for a in self.cfg["push_command"]]
        proc = run(argv, cwd=self.state["repo_root"], check=False, timeout=600)
        if proc.returncode != 0 or self.git.remote_head(self.state["branch"]) != new_head:
            raise Stop("PAUSED", f"guarded push of archive commit {short(new_head)} failed: "
                                 f"{(proc.stderr or proc.stdout).strip()[-400:]}")
        for (name, text, key), rel in zip(written, paths):
            archived[rel] = sha256_file(Path(self.state["repo_root"]) / rel)
            archived_keys.add(key)
        self.state["archived_keys"] = sorted(archived_keys)
        self.state.setdefault("archive_commits", []).append({"commit": new_head, "source": head, "reason": reason})
        self.save()
        return new_head

    def readiness(self, archive_head):
        ready = self.state.get("ready_review")
        reasons, notes = [], []
        if not ready:
            return ["no accepted final review with decision ready_for_owner_review"], notes
        if self.state["plan"]["checkpoints"][self.state["checkpoint_index"]]["type"] != "final":
            reasons.append("the active checkpoint is not the final checkpoint")
        ok, why = check_archive_only(self.git, ready["H"], archive_head, self.cfg["transcript_dir"])
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
        mission = self.state["mission"]
        if mission.get("path") and Path(mission["path"]).exists() and sha256_file(mission["path"]) != mission["sha256"]:
            reasons.append("mission file changed after enrollment")
        checks = self.gh.checks(self.state["pr"]["number"])
        if not checks:
            notes.append("no hosted checks are configured for this PR")
        for c in checks:
            if c.get("bucket") not in ("pass", "skipping"):
                reasons.append(f"hosted check {c.get('name')} is {c.get('state')}")
        base_now = self.git.fetch(self.state["target"])
        if base_now != ready["B"]:
            notes.append(f"target {self.state['target']} advanced from {short(ready['B'])} to {short(base_now)} after "
                         f"review; the reviewed diff (M={short(ready['M'])}..H) is unchanged, but the owner should "
                         "reconcile before merging")
        return reasons, notes

    def finalize(self):
        archive_head = self.archive("final")
        reasons, notes = self.readiness(archive_head)
        if reasons:
            raise Stop("BLOCKED", "not ready for owner review: " + "; ".join(reasons))
        ready = self.state["ready_review"]
        summary = ready["summary"]
        open_nonblocking = sorted(fid for fid, e in self.state.get("findings", {}).items()
                                  if not e["blocks_readiness"] and e["status"] != "resolved")
        dev_log = self.state["developer"].get("log", [])
        lines = [f"### Workflow — awaiting owner review — source `{short(ready['H'])}`, archive `{short(archive_head)}`\n",
                 f"- Reviewed source candidate H: `{ready['H']}` (review round {ready['round']}, "
                 f"Codex `{self.cfg['reviewer']['model']}` / `{self.cfg['reviewer']['effort']}`)",
                 f"- Archive commit A: `{archive_head}` — "
                 + ("identical to H." if archive_head == ready["H"] else
                    "adds only append-only transcript records under "
                    f"`{self.cfg['transcript_dir']}/pr-{self.state['pr']['number']}/` (path/mode-checked)."),
                 f"- Merge base M: `{ready['M']}`; target at review B: `{ready['B']}`",
                 f"- Developer: Claude `{self.cfg['developer']['model']}` / `{self.cfg['developer']['effort']}`; "
                 f"observed models {sorted({m for e in dev_log for m in (e.get('main_models') or [])}) or 'n/a (foreground session)'}",
                 f"- Review rounds: " + ", ".join(f"{k}: {v}" for k, v in self.state["rounds_completed"].items()),
                 "", f"**Delivered behavior (reviewer summary):** {summary['delivered_behavior']}", "",
                 "**Remaining owner review:**", *([f"- {x}" for x in summary["remaining_owner_review"]] or ["- none listed"]),
                 "", "**Open non-blocking items:** " + (", ".join(open_nonblocking) or "none"),
                 "", "**Notes:**", *([f"- {n}" for n in notes] or ["- none"]), "",
                 "This is readiness for the owner's review only. It does not merge, release, or grant visual "
                 "acceptance. The PR remains a draft with no requested reviewers.", "",
                 "_Automated workflow status posted through the owner's GitHub account; not a human approval._"]
        body = "\n".join(lines) + "\n"
        self.publish(f"final-status-{short(archive_head)}", body)
        self.update_pr_body(ready["H"], archive_head)
        self.state.update(status="AWAITING_OWNER_REVIEW", status_reason=f"source {ready['H']}, archive {archive_head}")
        self.transition("done", "owner review handoff posted")
        return "AWAITING_OWNER_REVIEW"

    def update_pr_body(self, source, archive):
        pr = self.pr_view()
        start, end = "<!-- agent-review:status:start -->", "<!-- agent-review:status:end -->"
        section = (f"{start}\n**Agent review status:** awaiting owner review. Reviewed source `{source}`; "
                   f"archive head `{archive}` (transcript-only successor). Records: "
                   f"`{self.cfg['transcript_dir']}/pr-{self.state['pr']['number']}/`.\n{end}")
        body = pr.get("body") or ""
        body = re.sub(re.escape(start) + ".*?" + re.escape(end), lambda _: section, body, flags=re.S) \
            if start in body else body.rstrip() + "\n\n" + section
        path = self.dir / "outbox" / "pr-body.md"
        write_text(path, sanitize(body)[0])
        self.gh.edit_body(self.state["pr"]["number"], path)

    # -- status ---------------------------------------------------------------------------
    def status(self):
        s = self.state
        lock = self.dir / "controller.lock"
        holder = read_json(lock) if lock.exists() else None
        try:
            fp_ok = workflow_fingerprint(self.wf) == s["workflow"]["fingerprint"]
        except Stop:
            fp_ok = False
        return {
            "enrollment": s["enrollment_id"], "status": s["status"], "status_reason": s.get("status_reason"),
            "phase": s["phase"], "mode": s["mode"], "pr": s["pr"]["url"], "branch": s["branch"],
            "checkpoint": self.checkpoint["id"], "checkpoint_type": self.checkpoint["type"],
            "rounds_completed": s["rounds_completed"], "round_limit": self.limits["max_review_rounds_per_checkpoint"],
            "last_round": {k: s["rounds"][-1].get(k) for k in ("round", "status", "decision", "H")} if s["rounds"] else None,
            "open_findings": sorted(f for f, e in s.get("findings", {}).items() if e["status"] == "open"),
            "developer_session": s["developer"]["session_id"], "developer_invocations": s["developer"]["invocations"],
            "developer_running": pid_alive(s["developer"].get("active_pid")),
            "lock": holder, "stop_requested": self.stop_path.exists(),
            "pending_publications": sorted(k for k, p in s.get("publications", {}).items() if p["state"] != "posted"),
            "workflow_fingerprint_matches": fp_ok, "updated_at": s.get("updated_at"),
        }


# --------------------------------------------------------------------------- enrollment

def load_project(name_or_path, root=HERE):
    path = Path(name_or_path)
    if not path.exists():
        path = root / "projects" / f"{name_or_path}.json"
    cfg = read_json(path)
    if cfg.get("schema_version") != "agent-review.project.v1":
        raise UsageError(f"{path} is not an agent-review project config")
    return cfg


def load_plan(name_or_path, root=HERE):
    path = Path(name_or_path)
    if not path.exists():
        path = root / "plans" / f"{name_or_path}.json"
    plan = read_json(path)
    cps = plan.get("checkpoints") or []
    if plan.get("schema_version") != "agent-review.plan.v1" or not cps:
        raise UsageError(f"{path} is not an agent-review plan")
    if cps[-1]["type"] != "final" or any(c["type"] != "intermediate" for c in cps[:-1]):
        raise UsageError("plan must end with exactly one final checkpoint; earlier checkpoints are intermediate")
    if len({c["id"] for c in cps}) != len(cps):
        raise UsageError("checkpoint IDs must be unique")
    return plan


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
    mission = Path(args.mission).resolve()
    if not mission.is_file():
        raise UsageError(f"mission not found: {mission}")
    state_root = Path(expand(args.state_root or cfg["state_root"]))
    state_root.mkdir(parents=True, exist_ok=True)
    for other in state_root.glob("*/state.json"):
        data = read_json(other)
        if data.get("branch") == branch and data.get("project_id") == cfg["project_id"] and \
                data.get("status") not in ("AWAITING_OWNER_REVIEW", "CANCELLED"):
            raise UsageError(f"branch {branch} already has an active enrollment: {data['enrollment_id']}")
    slug = re.sub(r"[^a-z0-9]+", "-", branch.lower()).strip("-")
    enrollment_id = f"{cfg['project_id']}-{slug}-{dt.datetime.now().strftime('%Y%m%d%H%M%S')}"
    folder = state_root / enrollment_id
    folder.mkdir()
    snapshot = folder / "mission" / mission.name
    snapshot.parent.mkdir()
    shutil.copy2(mission, snapshot)
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
            "Review requests, model-generated reviews, developer dispositions and status are posted as PR comments by "
            "the controller through the owner's account; none is a human review. Records are archived under "
            f"`{cfg['transcript_dir']}/`.\n\n<!-- agent-review:status:start -->\n**Agent review status:** enrolled."
            "\n<!-- agent-review:status:end -->\n")[0])
        number = gh.create_draft_pr(branch, target, args.title or f"[draft] {plan.get('title', plan['mission_id'])}", body)
    pr = gh.pr(number)
    if pr["headRefName"] != branch or pr["baseRefName"] != target or not pr["isDraft"] or pr["state"] != "OPEN":
        raise UsageError(f"PR #{number} is not an open draft from {branch} into {target}")
    if f"/{cfg['repository']}/pull/{number}" not in pr["url"]:
        raise UsageError("PR is not in the project's repository")
    if pr.get("reviewRequests"):
        raise UsageError("PR has requested reviewers; the workflow requires none")
    limits = dict(cfg["limits"])
    if args.max_rounds:
        limits["max_review_rounds_per_checkpoint"] = int(args.max_rounds)
    state = {
        "schema_version": STATE_SCHEMA, "protocol": PROTOCOL, "enrollment_id": enrollment_id,
        "project_id": cfg["project_id"], "project": cfg, "plan": plan, "mode": args.mode,
        "repo_root": top, "branch": branch, "target": target,
        "pr": {"number": number, "url": pr["url"]},
        "mission": {"path": str(mission), "sha256": sha256_file(mission), "snapshot": str(snapshot)},
        "workflow": {"fingerprint": workflow_fingerprint(root), "root": str(root), "activated_at": now_iso()},
        "limits": limits, "checkpoint_index": 0, "status": "ACTIVE", "phase": "develop",
        "developer": {"session_id": str(uuid.uuid4()), "started": False, "invocations": 0, "active_pid": None},
        "rounds": [], "rounds_completed": {}, "findings": {}, "pending_findings": [], "publications": {},
        "created_at": now_iso(), "transitions": [],
    }
    atomic_write_json(folder / "state.json", state)
    return folder, state


# --------------------------------------------------------------------------- preflight

def preflight(cfg, root=HERE):
    """Real bounded calls to both configured models; records requested vs observed metadata."""
    import tempfile
    out = {"at": now_iso(), "project": cfg["project_id"]}
    work = Path(tempfile.mkdtemp(prefix="agent-review-preflight-"))
    dev = ClaudeDeveloper(cfg["developer"], root / cfg["developer"]["settings"], root / REPORT_SCHEMA_FILE)
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
    dest.mkdir(parents=True, exist_ok=True)
    old = dest / "MANIFEST.json"
    if old.exists():
        for state_file in _state_files(dest):
            lock = state_file.parent / "controller.lock"
            if lock.exists() and pid_alive(read_json(lock).get("pid")):
                raise UsageError(f"an active controller holds {lock}; stop it before reinstalling")
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


def _state_files(root: Path):
    roots = set()
    for project in (root / "projects").glob("*.json"):
        try:
            roots.add(Path(expand(read_json(project)["state_root"])))
        except (KeyError, ValueError):
            continue
    return [f for r in roots if r.exists() for f in r.glob("*/state.json")]


# --------------------------------------------------------------------------- CLI

def find_enrollment(value, root=HERE):
    path = Path(value)
    if path.is_dir():
        return path
    for state_file in _state_files(root):
        if state_file.parent.name == value:
            return state_file.parent
    raise UsageError(f"enrollment not found: {value}")


def guarded(controller, command, fn):
    """Run fn under the single-writer lock; convert stops into visible status."""
    with controller.lock(command):
        try:
            result = fn()
            print(json.dumps({"result": result, **controller.status()}, indent=2))
            return EXIT_OK
        except Stop as stop:
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
                            ("resume", "clear a stop/blocked state after owner action and continue"),
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
        if args.command == "enroll":
            folder, state = enroll(args)
            print(json.dumps({"enrollment": state["enrollment_id"], "dir": str(folder), "pr": state["pr"]["url"],
                              "mode": state["mode"]}, indent=2))
            return EXIT_OK
        controller = Controller(find_enrollment(args.enrollment))
        if args.command == "status":
            print(json.dumps(controller.status(), indent=2))
            return EXIT_OK
        if args.command == "stop":
            write_text(controller.stop_path, f"{args.reason} ({now_iso()})\n")
            print(json.dumps({"stop_requested": True, "note": "the controller pauses at the next safe boundary; an "
                              "active developer invocation finishes its own transaction first"}, indent=2))
            return EXIT_OK
        if args.command == "activate":
            with controller.lock("activate"):
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
                controller.verify_inputs()
                if controller.state["phase"] == "review":
                    return controller.review_round()
                if controller.state["phase"] != "develop":
                    raise UsageError(f"phase is {controller.state['phase']}; nothing to review")
                error = controller.accept_report(read_json(args.report))
                if error:
                    raise UsageError(error)
                controller.transition("review", "developer submitted a checkpoint report")
                return controller.review_round()
            return guarded(controller, "review", _review)
        if args.command == "finalize":
            def _finalize():
                controller.verify_inputs()
                if controller.state["phase"] != "finalize":
                    raise UsageError(f"phase is {controller.state['phase']}; finalize follows a ready review")
                return controller.finalize()
            return guarded(controller, "finalize", _finalize)
        if args.command == "archive":
            return guarded(controller, "archive", lambda: controller.archive("manual"))
    except UsageError as exc:
        print(json.dumps({"refused": str(exc)}, indent=2), file=sys.stderr)
        return EXIT_USAGE
    return EXIT_USAGE


if __name__ == "__main__":
    sys.exit(main())
