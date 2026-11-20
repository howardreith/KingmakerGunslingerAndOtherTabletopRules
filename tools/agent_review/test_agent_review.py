"""Behavior tests for the agent-review controller.

Real cooperating components: temporary Git repositories with a bare origin,
the controller's state, OS lock, registry, publication ledger, archive commits
and checks. Only the external model (Codex/Claude) and GitHub boundaries are
stubbed, and every stubbed response below is a labeled simulation, not a model
exchange.

Run: python -m unittest discover -s tools/agent_review -p "test_*.py" -v
"""
import argparse
import json
import os
import re
import shutil
import socket
import subprocess
import stat
import tempfile
import uuid
import unittest
from pathlib import Path

import agent_review as ar

HERE = Path(__file__).resolve().parent
OPUS = "claude-opus-5-5"
FOREGROUND_SESSION = "fixture-foreground-session"


def git(cwd, *args):
    return subprocess.run(["git", "-C", str(cwd), *args], check=True, capture_output=True, text=True).stdout.strip()


class FakeGitHub:
    """Simulated GitHub boundary backed by the temporary bare origin."""

    def __init__(self, env):
        self.env, self.comments_list, self.next_id = env, [], 1000
        self.lose_next_response = 0
        self.fail_next = 0
        self.pr_overrides = {}
        self.body = ""
        self.required = []
        self.check_results = {}

    def viewer(self):
        return self.pr_overrides.get("viewer", "owner")

    def pr(self, number):
        head = git(self.env.origin, "rev-parse", f"refs/heads/{self.env.branch}")
        data = {"number": number, "url": f"https://github.com/owner/repo/pull/{number}", "state": "OPEN",
                "isDraft": True, "headRefName": self.env.branch, "headRefOid": head, "baseRefName": "master",
                "author": {"login": "owner"}, "reviewRequests": [], "body": self.body, "isCrossRepository": False,
                "headRepository": {"name": "repo"}, "headRepositoryOwner": {"login": "owner"}}
        data.update({k: v for k, v in self.pr_overrides.items() if k != "viewer"})
        return data

    def comments(self, number):
        return [dict(c) for c in self.comments_list]

    def post_comment(self, number, body_file):
        if self.fail_next:
            self.fail_next -= 1
            raise RuntimeError("simulated transient GitHub failure")
        body = Path(body_file).read_text(encoding="utf-8")
        self.next_id += 1
        record = {"id": self.next_id, "user": "owner", "body": body, "url": f"https://example.test/c/{self.next_id}"}
        self.comments_list.append(record)
        if self.lose_next_response:
            self.lose_next_response -= 1
            raise TimeoutError("simulated lost response after the server stored the comment")
        return {"id": record["id"], "url": record["url"]}

    def required_checks(self, branch):
        return list(self.required)

    def commit_checks(self, sha):
        return list(self.check_results.get(sha, []))

    def edit_body(self, number, body_file):
        self.body = Path(body_file).read_text(encoding="utf-8")

    def headings(self):
        return [c["body"].splitlines()[1] for c in self.comments_list]


class FakeReviewer:
    """Simulated Codex boundary. Each scripted item is a labeled simulated response."""

    def __init__(self):
        self.script, self.calls, self.prov = [], [], {}
        self.during_run = None

    @property
    def requested(self):
        return {"model": "gpt-6.2-astra", "effort": "high", "sandbox": "read-only"}

    def run(self, prompt, workdir, schema, result_path, events_path, stderr_path, timeout_s, skip_git=False):
        item = self.script.pop(0)
        request = json.loads((Path(result_path).parent / "request.json").read_text(encoding="utf-8"))
        self.calls.append({"request": request, "prompt": prompt, "workdir": str(workdir)})
        assert Path(workdir).is_dir() and git(workdir, "rev-parse", "HEAD") == request["H"]
        if self.during_run:
            self.during_run()
        thread = f"thread-{len(self.calls)}"
        self.prov[thread] = {"rollout_found": True, "observed_model": item.get("model", "gpt-6.2-astra"),
                             "observed_effort": item.get("effort", "high"),
                             "turn_models": [item.get("model", "gpt-6.2-astra")],
                             "turn_efforts": item.get("turn_efforts", ["high"]), "sandbox": "read-only",
                             "approval_policy": "never", "network": "restricted", "cli_version": "simulated",
                             "usage": None, "plan_type": None, "subagent_suspected": False, "source": "SIMULATED"}
        Path(stderr_path).write_text(item.get("stderr", ""), encoding="utf-8")
        Path(events_path).write_text(json.dumps({"type": "thread.started", "thread_id": thread}) + "\n", encoding="utf-8")
        if "raw" in item:
            Path(result_path).write_text(item["raw"], encoding="utf-8")
        else:
            result = review_result(request, item["decision"], item.get("findings", []), item.get("assessments", []))
            for key, value in item.get("override", {}).items():
                result[key] = value
            for key in item.get("drop", []):
                result.pop(key)
            Path(result_path).write_text(json.dumps(result), encoding="utf-8")
        return {"exit_code": item.get("exit_code", 0), "timed_out": False, "thread_id": thread, "duration_s": 0.1}

    def provenance(self, thread_id):
        return self.prov.get(thread_id, {"rollout_found": False})


def finding(fid, blocks=True, category="defect"):
    return {"id": fid, "priority": "P1", "category": category, "blocks_readiness": blocks,
            "blocking_rationale": "simulated", "title": f"simulated finding {fid}", "trigger": "t", "expected": "e",
            "actual": "a", "location": {"file": "feature.txt", "line": 1}, "acceptance_ids": ["X1"],
            "requested_correction": "c", "requested_evidence": "v"}


def review_result(request, decision, findings=(), assessments=()):
    return {"schema_version": "agent-review.review.v1", "request_id": request["request_id"], "round": request["round"],
            "checkpoint_id": request["checkpoint_id"], "checkpoint_type": request["checkpoint_type"],
            "candidate_commit": request["H"], "decision": decision, "decision_rationale": "simulated response",
            "findings": list(findings),
            "prior_finding_assessments": [{"finding_id": f, "assessment": a, "rationale": "simulated"}
                                          for f, a in assessments],
            "evidence_assessment": {"inspected": ["feature.txt"], "executed_verification": [],
                                    "source_inspection_only": ["feature.txt"], "required_gaps": [], "limitations": []},
            "summary": {"delivered_behavior": "simulated", "scope_reviewed": "simulated", "pending_later_phase_work": [],
                        "unresolved_questions": [], "remaining_owner_review": ["simulated visual check"]}}


class FakeDeveloper:
    """Simulated Claude boundary: scripted headless actions commit real changes."""

    def __init__(self, env):
        self.env, self.script, self.calls, self.sessions = env, [], [], set()
        self.foreground = {"models": [OPUS], "efforts": ["high"]}

    @property
    def requested(self):
        return {"model": "opus", "effort": "high"}

    def session_exists(self, session_id):
        return session_id in self.sessions

    def session_provenance(self, session_id):
        return {"found": True, "session_id": session_id, "source": "SIMULATED", **self.foreground}

    def run(self, prompt, session_id, resume, cwd, log_path, stderr_path, soft_deadline_s, on_start=None,
            on_soft_deadline=None, grace_s=0, protected=None):
        self.calls.append({"session_id": session_id, "resume": resume, "prompt": prompt})
        Path(stderr_path).write_text("", encoding="utf-8")
        self.sessions.add(session_id)
        action = self.script.pop(0)
        report = action(self.env)
        model = getattr(action, "model", OPUS)
        return {"init_model": model, "api_key_source": "none", "session_id": getattr(action, "session", session_id),
                "main_models": [model], "result": "", "structured_output": report, "permission_denials": [],
                "model_usage": {}, "cost_usd": 0.0, "is_error": False, "subtype": "success",
                "exit_code": getattr(action, "exit_code", 0), "deadline_exceeded": False, "terminated": False,
                "duration_s": 0.1, "observed_effort": ["high"]}


def report(env, checkpoint, dispositions=(), status="checkpoint_ready", restoration="not_applicable", validation=None):
    return {"schema_version": "agent-review.developer.v1", "status": status, "checkpoint_id": checkpoint,
            "candidate_commit": git(env.work, "rev-parse", "HEAD"), "summary": "simulated developer report",
            "validation": validation or [{"command": "python -m unittest", "required": True, "result": "pass",
                                          "duration": "1s", "scope_and_limits": "simulated"}],
            "dispositions": list(dispositions), "evidence_refs": [], "runtime_restoration": restoration,
            "blocker": None, "owner_decision_needed": None}


def fixed(fid, sha):
    return {"finding_id": fid, "disposition": "fixed", "commits": [sha], "evidence": "simulated focused test"}


class Env:
    def __init__(self, mode="foreground", max_rounds=None, cfg_overrides=None, plan=None, mission_text=None,
                 extra_files=None, extra_wf_files=None):
        # Plain mkdir (inherited ACLs), not tempfile.mkdtemp: on Windows mkdtemp applies an owner-only
        # ACL that the Codex sandbox identity cannot read, which would make the live smoke unrepresentative.
        root = Path(tempfile.gettempdir()) / f"agent-review-test-{uuid.uuid4().hex[:10]}"
        root.mkdir()
        self.root = root
        self._saved_env = {k: os.environ.get(k) for k in ("AGENT_REVIEW_HOME", "CLAUDE_CODE_SESSION_ID")}
        os.environ["AGENT_REVIEW_HOME"] = str(root / "home")
        os.environ["CLAUDE_CODE_SESSION_ID"] = FOREGROUND_SESSION
        self.origin, self.work, self.branch = root / "origin.git", root / "work", "codex/feature"
        subprocess.run(["git", "init", "--quiet", "--bare", "-b", "master", str(self.origin)], check=True)
        subprocess.run(["git", "clone", "--quiet", str(self.origin), str(self.work)], check=True, capture_output=True)
        for key, value in (("user.name", "Test"), ("user.email", "test@example.invalid"), ("core.autocrlf", "false")):
            git(self.work, "config", key, value)
        git(self.work, "checkout", "--quiet", "-b", "master")
        self.base_root = self.commit("README.md", "base\n", "base")
        for name, text in (extra_files or {}).items():
            self.commit(name, text, f"add {name}")
        git(self.work, "push", "--quiet", "origin", "master")
        git(self.work, "checkout", "--quiet", "-b", self.branch)
        self.commit("feature.txt", "v1\n", "feature v1")
        self.push()
        self.wf = root / "wf"
        shutil.copytree(HERE, self.wf, ignore=shutil.ignore_patterns("__pycache__", "test_*"))
        cfg = ar.read_json(HERE / "projects" / "kingmaker-gunslinger.json")
        cfg.update(project_id="t", repository="owner/repo", expected_actor="owner",
                   allowed_remote_urls=[git(self.work, "remote", "get-url", "origin")],
                   push_command=["git", "-C", "{repo_root}", "push", "--quiet", "origin", "HEAD:refs/heads/{branch}"],
                   state_root=str(root / "state"), runtime_lock_paths=[str(root / "runtime.lock")])
        cfg["limits"].update(retry_backoff_seconds=[0, 0])
        for key, value in (cfg_overrides or {}).items():
            cfg[key] = value
        for rel, data in (extra_wf_files or {}).items():
            ar.atomic_write_json(self.wf / rel, data)
        ar.atomic_write_json(self.wf / "projects" / "test.json", cfg)
        plan = plan or {"schema_version": "agent-review.plan.v1", "mission_id": "fixture", "title": "Fixture mission",
                        "checkpoints": [{"id": "phase-1", "type": "intermediate", "scope": "s", "exit_conditions": ["e"]},
                                        {"id": "final", "type": "final", "scope": "s", "exit_conditions": ["e"]}]}
        ar.atomic_write_json(self.wf / "plans" / "test.json", plan)
        self.mission = root / "mission.md"
        self.mission.write_text(mission_text or "# Fixture mission\n", encoding="utf-8")
        self.gh, self.reviewer, self.developer = FakeGitHub(self), FakeReviewer(), FakeDeveloper(self)
        self.dir, _ = ar.enroll(self.args(mode=mode, max_rounds=max_rounds), github=self.gh, root=self.wf)

    def args(self, **overrides):
        base = dict(project=str(self.wf / "projects" / "test.json"), worktree=str(self.work),
                    plan=str(self.wf / "plans" / "test.json"), mission=str(self.mission), mode="foreground", pr=7,
                    title=None, target=None, max_rounds=None, state_root=None)
        base.update(overrides)
        return argparse.Namespace(**base)

    def controller(self, cls=ar.Controller):
        return cls(self.dir, github=self.gh, reviewer=self.reviewer, developer=self.developer,
                   sleep=lambda s: None, workflow_root=self.wf)

    def commit(self, name, text, message):
        path = self.work / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        git(self.work, "add", name)
        git(self.work, "commit", "--quiet", "-m", message)
        return git(self.work, "rev-parse", "HEAD")

    def push(self):
        git(self.work, "push", "--quiet", "origin", f"HEAD:refs/heads/{self.branch}")

    def cleanup(self):
        subprocess.run(["git", "-C", str(self.work), "worktree", "prune"], capture_output=True)
        for key, value in self._saved_env.items():
            if value is None:
                os.environ.pop(key, None)
            else:
                os.environ[key] = value
        def _writable(func, path, _exc):
            os.chmod(path, stat.S_IWRITE)  # git marks object files read-only on Windows
            func(path)
        shutil.rmtree(self.root, onexc=_writable)


class Base(unittest.TestCase):
    mode = "foreground"

    def setUp(self):
        self.env = Env(self.mode)
        self.addCleanup(self.env.cleanup)

    def submit(self, ctl, checkpoint, dispositions=()):
        """Foreground developer: submit a report, then run one review round."""
        error = ctl.accept_report(report(self.env, checkpoint, dispositions))
        self.assertIsNone(error)
        ctl.transition("review", "test submit")
        return ctl.review_round()

    def to_finalize(self, ctl):
        self.env.reviewer.script += [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        self.submit(ctl, "phase-1")
        self.assertEqual("ready_for_owner_review", self.submit(ctl, "final"))


def run_env(mode, fn):
    env = Env(mode)
    try:
        return fn(env)
    finally:
        env.cleanup()


class HeadlessCycleTests(Base):
    mode = "headless"

    def test_intermediate_advances_then_findings_fix_and_owner_handoff(self):
        """WF03 (fixture), WF12, WF09 positive, WF11: full simulated cycle with a correction round."""
        env = self.env
        fix = {}

        def phase1(e):
            e.commit("phase1.txt", "p1\n", "phase 1")
            e.push()
            return report(e, "phase-1")

        def final_work(e):
            e.commit("final.txt", "f\n", "final work")
            e.push()
            return report(e, "final")

        def correct(e):
            fix["sha"] = e.commit("final.txt", "fixed\n", "fix R2-F1")
            e.push()
            return report(e, "final", [fixed("R2-F1", fix["sha"])])
        env.developer.script = [phase1, final_work, correct]
        env.reviewer.script = [{"decision": "checkpoint_passed"},
                               {"decision": "changes_required", "findings": [finding("R2-F1")]},
                               {"decision": "ready_for_owner_review", "assessments": [("R2-F1", "fix_verified")]}]
        ctl = env.controller()
        self.assertEqual("AWAITING_OWNER_REVIEW", ctl.run_loop())

        s = ctl.state
        self.assertEqual({"phase-1": 1, "final": 2}, s["rounds_completed"])
        self.assertEqual([False, True, True], [c["resume"] for c in env.developer.calls])
        self.assertEqual(1, len({c["session_id"] for c in env.developer.calls}))
        self.assertEqual(3, s["developer"]["invocations"])
        self.assertIn("R2-F1", env.developer.calls[2]["prompt"])
        self.assertTrue(all(r["status"] == "accepted" for r in s["rounds"]))
        headings = env.gh.headings()
        expected = ["Claude Opus — review request — round 1", "Codex GPT-6.2 Astra — review — round 1",
                    "Claude Opus — review request — round 2", "Codex GPT-6.2 Astra — review — round 2",
                    "Claude Opus — finding dispositions — round 2", "Claude Opus — review request — round 3",
                    "Codex GPT-6.2 Astra — review — round 3", "Workflow — awaiting owner review"]
        self.assertEqual(len(expected), len(headings))
        for want, got in zip(expected, headings):
            self.assertIn(want, got)
        source = s["ready_review"]["H"]
        archive = git(env.work, "rev-parse", "HEAD")
        self.assertNotEqual(source, archive)
        self.assertEqual(archive, git(env.origin, "rev-parse", f"refs/heads/{env.branch}"))
        self.assertEqual((True, []), ar.check_archive_only(ar.Git(env.work), source, archive, "docs/agent-reviews", 7,
                                                           s["archived"]))
        records = sorted(p.name for p in (env.work / "docs/agent-reviews/pr-7").iterdir())
        self.assertEqual(10, len(records))  # 7 published bodies + 3 review envelopes; final receipt not archived
        self.assertTrue(records[0].startswith("001-r01-request"))
        self.assertIn(f"Reviewed source `{source}`", env.gh.body)
        self.assertIn("no required status checks", env.gh.comments_list[-1]["body"])
        self.assertEqual("done", s["phase"])

    def test_headless_invocation_identity_and_budget(self):
        """F17: wrong session or non-zero exit is not accepted; budget is charged before launch."""
        env = self.env

        def other_session(e):
            return report(e, "phase-1")
        other_session.session = "11111111-2222-3333-4444-555555555555"
        env.developer.script = [other_session]
        ctl = env.controller()
        with self.assertRaises(ar.Stop) as stop:
            ctl.develop_step()
        self.assertIn("not the enrolled", stop.exception.reason)

        def failed_exit(e):
            return report(e, "phase-1")
        failed_exit.exit_code = 1
        env.developer.script = [failed_exit]
        self.assertIn("exit 1", env.controller().develop_step())

        def crash(e):
            raise RuntimeError("simulated host crash during the invocation")
        env.developer.script = [crash]
        ctl = env.controller()
        before = ctl.state["developer"]["invocations"]
        with self.assertRaises(RuntimeError):
            ctl.develop_step()
        restarted = env.controller()
        self.assertEqual(before + 1, restarted.state["developer"]["invocations"])
        env.developer.script = [lambda e: report(e, "phase-1")]
        restarted.develop_step()
        self.assertIn("interrupted", env.developer.calls[-1]["prompt"])

    def test_wrong_developer_model_blocks(self):
        def sonnet(e):
            return report(e, "phase-1")
        sonnet.model = "claude-sonnet-5-5"
        self.env.developer.script = [sonnet]
        with self.assertRaises(ar.Stop) as stop:
            self.env.controller().develop_step()
        self.assertEqual("BLOCKED", stop.exception.status)
        self.assertIn("claude-sonnet-5-5", stop.exception.reason)


class ArchiveCheckTests(Base):
    def test_only_new_plain_records_are_exempt(self):
        """WF09: transcript-only exemption is path-, operation-, mode- and PR-checked."""
        env, g = self.env, ar.Git(self.env.work)
        h = git(env.work, "rev-parse", "HEAD")
        a1 = env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "x\n", "archive")
        self.assertEqual((True, []), ar.check_archive_only(g, h, a1, "docs/agent-reviews", 7))
        cases = {
            "edit earlier record": lambda: env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "changed\n", "edit"),
            "source change": lambda: env.commit("feature.txt", "sneaky\n", "src"),
            "bad name": lambda: env.commit("docs/agent-reviews/pr-7/notes.txt", "x\n", "name"),
            "outside dir": lambda: env.commit("docs/other/001-r01-review.md", "x\n", "dir"),
            "other PR": lambda: env.commit("docs/agent-reviews/pr-8/002-r01-review.md", "x\n", "pr"),
        }
        for label, make in cases.items():
            with self.subTest(label):
                git(env.work, "reset", "--quiet", "--hard", a1)
                bad = make()
                ok, reasons = ar.check_archive_only(g, h, bad, "docs/agent-reviews", 7)
                self.assertFalse(ok)
                self.assertTrue(reasons)
        git(env.work, "reset", "--quiet", "--hard", a1)
        path = env.work / "docs/agent-reviews/pr-7/002-r01-envelope.json"
        path.write_text("{}\n", encoding="utf-8")
        git(env.work, "add", str(path))
        git(env.work, "update-index", "--chmod=+x", str(path))
        git(env.work, "commit", "--quiet", "-m", "exec")
        ok, reasons = ar.check_archive_only(g, h, git(env.work, "rev-parse", "HEAD"), "docs/agent-reviews", 7)
        self.assertFalse(ok)
        self.assertIn("mode", reasons[0])
        self.assertFalse(ar.check_archive_only(g, a1, h, "docs/agent-reviews", 7)[0])  # not an ancestor

    def test_inventory_binds_exemption_to_controller_records(self):
        """F13: a well-named record the controller did not write is not exempt."""
        env, g = self.env, ar.Git(self.env.work)
        h = git(env.work, "rev-parse", "HEAD")
        a = env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "forged\n", "forged record")
        self.assertTrue(ar.check_archive_only(g, h, a, "docs/agent-reviews", 7)[0])
        ok, reasons = ar.check_archive_only(g, h, a, "docs/agent-reviews", 7, {})
        self.assertFalse(ok)
        self.assertIn("not a record the controller wrote", reasons[0])
        ok, reasons = ar.check_archive_only(g, h, a, "docs/agent-reviews", 7,
                                            {"docs/agent-reviews/pr-7/001-r01-review.md": ar.sha256_text("real\n")})
        self.assertIn("content differs", reasons[0])

    def test_unapproved_transcript_in_candidate_blocks_review(self):
        env = self.env
        env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "forged\n", "developer wrote a record")
        env.push()
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "t")
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertIn("unapproved transcript", stop.exception.reason)


class ArchiveRecoveryTests(Base):
    def test_push_failure_then_resume_reuses_the_same_archive_commit(self):
        """F12: archive intent is persisted; a failed push is retried without duplicating records."""
        env = self.env
        ctl = env.controller()
        self.to_finalize(ctl)
        good_push = list(ctl.cfg["push_command"])
        ctl.cfg["push_command"] = ["git", "-C", "{repo_root}", "push", "--quiet", "no-such-remote", "HEAD"]
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertIn("resume retries", stop.exception.reason)
        pending = ctl.state["archive_pending"]
        self.assertIsNotNone(pending["commit"])
        restarted = env.controller()
        restarted.cfg["push_command"] = good_push
        self.assertEqual("AWAITING_OWNER_REVIEW", restarted.finalize())
        self.assertEqual(pending["commit"], git(env.origin, "rev-parse", f"refs/heads/{env.branch}"))
        self.assertEqual(1, len(restarted.state["archive_commits"]))
        names = [p.name for p in (env.work / "docs/agent-reviews/pr-7").iterdir()]
        self.assertEqual(len(names), len({n[4:] for n in names}))  # no record archived twice

    def test_crash_after_writing_records_commits_them_on_restart(self):
        env = self.env

        class Crashing(ar.Controller):
            def complete_archive(self, pending):
                raise KeyboardInterrupt("simulated crash after the records were written")
        ctl = env.controller(Crashing)
        self.to_finalize(ctl)
        with self.assertRaises(KeyboardInterrupt):
            ctl.finalize()
        self.assertFalse(ar.Git(env.work).clean())
        restarted = env.controller()
        self.assertEqual("AWAITING_OWNER_REVIEW", restarted.finalize())
        self.assertTrue(ar.Git(env.work).clean())
        self.assertIsNone(restarted.state["archive_pending"])


class StaleCandidateTests(Base):
    def test_head_moving_during_review_makes_result_historical(self):
        """WF04: a moved material head cannot reuse the result."""
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}]

        def other_writer():
            env.commit("feature.txt", "other\n", "concurrent push")
            env.push()
        env.reviewer.during_run = other_writer
        ctl = env.controller()
        with self.assertRaises(ar.Stop) as stop:
            self.submit(ctl, "phase-1")
        self.assertEqual("PAUSED", stop.exception.status)
        self.assertEqual("historical", ctl.state["rounds"][-1]["status"])
        self.assertEqual({}, ctl.state["rounds_completed"])
        self.assertEqual(0, ctl.state["checkpoint_index"])
        self.assertEqual("develop", ctl.state["phase"])
        self.assertIn("HISTORICAL", env.gh.headings()[-1])
        # F2: the next round needs a report bound to the new head.
        ctl.transition("review", "t")
        self.assertEqual("needs_report", ctl.review_round())

    def test_preserved_result_is_rechecked_on_resume(self):
        """F1/F11: a validated-but-unpublished result is re-validated for freshness before acceptance."""
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "t")
        real_post, calls = env.gh.post_comment, {"n": 0}

        def post(number, body_file):
            calls["n"] += 1
            if calls["n"] >= 2:
                raise RuntimeError("simulated outage")
            return real_post(number, body_file)
        env.gh.post_comment = post
        with self.assertRaises(ar.Stop):
            ctl.review_round()
        env.gh.post_comment = real_post
        env.commit("feature.txt", "moved\n", "push while paused")
        env.push()
        restarted = env.controller()
        self.assertEqual("validated", restarted.active()["status"])
        with self.assertRaises(ar.Stop) as stop:
            restarted.review_round()
        self.assertIn("historical", stop.exception.reason)
        self.assertEqual({}, restarted.state["rounds_completed"])
        self.assertEqual(1, len(env.reviewer.calls))

    def test_material_commit_after_ready_invalidates_the_ready_result(self):
        """WF04/WF09/F2: a ready verdict does not cover a later material change."""
        env = self.env
        ctl = env.controller()
        self.to_finalize(ctl)
        env.commit("feature.txt", "late change\n", "material change after review")
        env.push()
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertEqual("PAUSED", stop.exception.status)
        self.assertIn("material change", stop.exception.reason)
        self.assertEqual("develop", ctl.state["phase"])
        self.assertIsNone(ctl.state["ready_review"])
        self.assertEqual(1, len(ctl.state["superseded_ready"]))

    def test_moved_merge_base_invalidates_the_ready_result(self):
        """F1: a changed comparison input (merge base) cannot keep a ready verdict."""
        env = Env("foreground", extra_files={"second.txt": "2\n"})
        self.addCleanup(env.cleanup)
        ctl = env.controller()
        env.reviewer.script = [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "t")
        ctl.review_round()
        ctl.accept_report(report(env, "final"))
        ctl.transition("review", "t")
        ctl.review_round()
        git(env.work, "push", "--quiet", "--force", "origin", f"{env.base_root}:refs/heads/master")
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertIn("merge base moved", stop.exception.reason)
        self.assertEqual("develop", ctl.state["phase"])

    def test_report_for_unpushed_candidate_is_refused(self):
        env = self.env
        ctl = env.controller()
        env.commit("feature.txt", "local only\n", "unpushed")
        self.assertIsNone(ctl.accept_report(report(env, "phase-1")))
        ctl.transition("review", "test")
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertIn("not the pushed branch head", stop.exception.reason)


class InvalidReviewTests(unittest.TestCase):
    """WF05: no failure shape produces a decision or readiness."""

    CASES = {
        "wrong model (fatal, no retry)": ([{"decision": "checkpoint_passed", "model": "gpt-6.1-sol"}], "wrong reviewer model", 1),
        "wrong effort (fatal)": ([{"decision": "checkpoint_passed", "effort": "low"}], "effort", 1),
        "contradictory turn effort (fatal)": ([{"decision": "checkpoint_passed", "turn_efforts": ["high", "low"]}], "effort", 1),
        "usage limit (fatal)": ([{"decision": "checkpoint_passed", "stderr": "You've hit your usage limit", "exit_code": 1}], "usage limit", 1),
        "malformed JSON": ([{"raw": "{not json"}] * 3, "malformed JSON", 3),
        "JSON null": ([{"raw": "null"}] * 3, "expected ['object']", 3),
        "truncated output": ([{"raw": '{"schema_version": "agent-review.review.v1", "decision": "check'}] * 3, "malformed", 3),
        "empty output": ([{"raw": ""}] * 3, "no final structured message", 3),
        "missing required field": ([{"decision": "checkpoint_passed", "drop": ["evidence_assessment"]}] * 3, "missing required", 3),
        "failed process": ([{"decision": "checkpoint_passed", "exit_code": 1}] * 3, "exited 1", 3),
        "mismatched request id": ([{"decision": "checkpoint_passed", "override": {"request_id": "forged"}}] * 3, "does not match", 3),
        "final decision on intermediate": ([{"decision": "ready_for_owner_review"}] * 3, "not valid for a intermediate", 3),
        "pass with blocking finding": ([{"decision": "checkpoint_passed", "findings": [finding("R1-F1")]}] * 3, "passing decision", 3),
        "pass with required evidence gap": ([{"decision": "checkpoint_passed", "override": {"evidence_assessment": {
            "inspected": [], "executed_verification": [], "source_inspection_only": [],
            "required_gaps": ["native observation missing"], "limitations": []}}}] * 3, "required evidence gaps", 3),
    }

    def test_invalid_reviews_block_without_counting_a_round(self):
        for label, (script, needle, attempts) in self.CASES.items():
            with self.subTest(label):
                def check(env):
                    env.reviewer.script = [dict(item) for item in script]
                    ctl = env.controller()
                    ctl.accept_report(report(env, "phase-1"))
                    ctl.transition("review", "test")
                    with self.assertRaises(ar.Stop) as stop:
                        ctl.review_round()
                    self.assertEqual("BLOCKED", stop.exception.status)
                    self.assertIn(needle, stop.exception.reason)
                    rnd = ctl.state["rounds"][-1]
                    self.assertEqual(attempts, len(rnd["attempts"]))
                    self.assertEqual({}, ctl.state["rounds_completed"])
                    self.assertNotIn("decision", rnd)
                    self.assertFalse(any("Codex GPT-6.2 Astra — review" in h for h in env.gh.headings()))
                run_env("foreground", check)

    def test_transient_failure_then_valid_review_counts_once_with_monotonic_attempts(self):
        env = Env("foreground")
        self.addCleanup(env.cleanup)
        env.reviewer.script = [{"raw": "{oops"}, {"decision": "checkpoint_passed"}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "test")
        self.assertEqual("checkpoint_passed", ctl.review_round())
        rnd = ctl.state["rounds"][-1]
        self.assertEqual([1, 2], [a["n"] for a in rnd["attempts"]])
        self.assertEqual({"phase-1": 1}, ctl.state["rounds_completed"])
        self.assertTrue((ctl.dir / "rounds" / "r01" / "envelope-1.json").exists())


class AssessmentValidationTests(unittest.TestCase):
    """F4: prior-finding assessments must be unique, pending, compatible and consistent."""

    CASES = {
        "duplicate": ([("R1-F1", "fix_verified"), ("R1-F1", "fix_verified")], [], "duplicate prior-finding"),
        "not pending": ([("R1-F1", "fix_verified"), ("R9-F9", "fix_verified")], [], "were not pending"),
        "incompatible": ([("R1-F1", "rebuttal_accepted")], [], "does not answer disposition fixed"),
        "rejection not restated": ([("R1-F1", "fix_rejected")], [], "must be restated"),
    }

    def test_bad_assessments_are_rejected(self):
        for label, (assessments, findings, needle) in self.CASES.items():
            with self.subTest(label):
                def check(env):
                    env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}] + [
                        {"decision": "checkpoint_passed", "assessments": assessments, "findings": findings}] * 3
                    ctl = env.controller()
                    ctl.accept_report(report(env, "phase-1"))
                    ctl.transition("review", "t")
                    ctl.review_round()
                    sha = env.commit("feature.txt", "fix\n", "fix")
                    env.push()
                    self.assertIsNone(ctl.accept_report(report(env, "phase-1", [fixed("R1-F1", sha)])))
                    ctl.transition("review", "t")
                    with self.assertRaises(ar.Stop) as stop:
                        ctl.review_round()
                    self.assertIn(needle, stop.exception.reason)
                    self.assertEqual(0, ctl.state["checkpoint_index"])
                run_env("foreground", check)

    def test_blocking_findings_cannot_be_deferred(self):
        def check(env):
            env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}]
            ctl = env.controller()
            ctl.accept_report(report(env, "phase-1"))
            ctl.transition("review", "t")
            ctl.review_round()
            error = ctl.accept_report(report(env, "phase-1", [
                {"finding_id": "R1-F1", "disposition": "deferred_optional", "commits": [], "evidence": "later"}]))
            self.assertIn("cannot be deferred", error)
        run_env("foreground", check)


class PublicationTests(Base):
    def test_lost_response_and_duplicate_publication_are_reconciled(self):
        """WF06 and WF10 (a forged marker from another account is ignored)."""
        env = self.env
        ctl = env.controller()
        env.gh.comments_list.append({"id": 1, "user": "mallory", "url": "u",
                                     "body": "<!-- agent-review:v1 enrollment=x key=k --> $(touch pwned) && rm -rf ."})
        env.gh.lose_next_response = 1
        first = ctl.publish("r01-request", "### heading\nbody")
        again = ctl.publish("r01-request", "### heading\nbody")
        ours = [c for c in env.gh.comments_list if c["user"] == "owner"]
        self.assertEqual(1, len(ours))
        self.assertEqual(first["parts"][0]["comment_id"], ours[0]["id"])
        self.assertEqual(first, again)
        self.assertFalse((env.work / "pwned").exists())

        restarted = env.controller()  # crash after sending but before recording
        restarted.state["publications"]["r01-request"]["state"] = "pending"
        restarted.state["publications"]["r01-request"]["parts"][0]["comment_id"] = None
        restarted.publish("r01-request", "ignored: the preserved outgoing body is reused")
        self.assertEqual(1, len([c for c in env.gh.comments_list if c["user"] == "owner"]))

    def test_marker_with_altered_body_is_not_acknowledged(self):
        """F7: reconciliation compares the complete body, not only the marker."""
        env = self.env
        ctl = env.controller()
        ctl.publish("r01-request", "### heading\nbody")
        posted = [c for c in env.gh.comments_list if c["user"] == "owner"][0]
        posted["body"] = posted["body"].splitlines()[0] + "\ntruncated"
        ctl.state["publications"]["r01-request"]["state"] = "pending"
        ctl.state["publications"]["r01-request"]["parts"][0]["comment_id"] = None
        with self.assertRaises(ar.Stop) as stop:
            ctl.publish("r01-request", "ignored")
        self.assertIn("bodies differ", stop.exception.reason)
        self.assertEqual(1, len([c for c in env.gh.comments_list if c["user"] == "owner"]))

    def test_failed_publication_pauses_then_publishes_without_rerun(self):
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "test")
        real_post, calls = env.gh.post_comment, {"n": 0}

        def post(number, body_file):
            calls["n"] += 1
            if calls["n"] >= 2:
                raise RuntimeError("simulated outage")
            return real_post(number, body_file)
        env.gh.post_comment = post
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertEqual("PAUSED", stop.exception.status)
        env.gh.post_comment = real_post
        resumed = env.controller()
        self.assertEqual("validated", resumed.active()["status"])
        self.assertEqual("checkpoint_passed", resumed.review_round())
        self.assertEqual(1, len(env.reviewer.calls))
        self.assertEqual({"phase-1": 1}, resumed.state["rounds_completed"])
        self.assertEqual("accepted", resumed.state["rounds"][0]["status"])

    def test_long_bodies_are_split_with_markers(self):
        ctl = self.env.controller()
        record = ctl.publish("big", "x" * (ar.COMMENT_LIMIT + 10))
        self.assertEqual(2, len(record["parts"]))
        self.assertTrue(all(c["body"].startswith("<!-- agent-review:v1") for c in self.env.gh.comments_list))


class StopResumeLockTests(Base):
    mode = "headless"

    def test_stop_then_resume_preserves_session_rounds_and_publications(self):
        """WF07."""
        env = self.env

        def phase1(e):
            e.commit("phase1.txt", "p1\n", "phase 1")
            e.push()
            (e.dir / "STOP").write_text("owner request\n", encoding="utf-8")  # owner runs `stop` mid-invocation
            return report(e, "phase-1")

        def final_work(e):
            e.commit("final.txt", "f\n", "final")
            e.push()
            return report(e, "final")
        env.developer.script = [phase1, final_work]
        env.reviewer.script = [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        ctl = env.controller()
        with self.assertRaises(ar.Stop) as stop:
            ctl.run_loop()
        self.assertEqual("PAUSED", stop.exception.status)
        self.assertEqual("review", ctl.state["phase"])
        posted_before = len(env.gh.comments_list)

        restarted = env.controller()  # new process
        self.assertEqual("AWAITING_OWNER_REVIEW", restarted.resume("owner resumed"))
        self.assertEqual([False, True], [c["resume"] for c in env.developer.calls])
        self.assertEqual(1, len({c["session_id"] for c in env.developer.calls}))
        self.assertEqual({"phase-1": 1, "final": 1}, restarted.state["rounds_completed"])
        self.assertGreater(len(env.gh.comments_list), posted_before)

    def test_os_lock_is_exclusive_and_survives_dead_holders(self):
        """F8: one writer per branch; a dead holder's record never blocks."""
        ctl = self.env.controller()
        with ctl.lock("run") as held:
            with self.assertRaises(ar.UsageError):
                with self.env.controller().lock("review"):
                    pass
            self.assertEqual(os.getpid(), json.loads(held.holder.read_text(encoding="utf-8"))["pid"])
        lock = ctl.lock("run")
        lock.holder.write_text(json.dumps({"pid": 999999, "host": socket.gethostname(), "command": "run"}),
                               encoding="utf-8")
        with lock:
            self.assertEqual(os.getpid(), json.loads(lock.holder.read_text(encoding="utf-8"))["pid"])
        self.assertFalse(lock.holder.exists())

    def test_running_developer_session_is_never_resumed_concurrently(self):
        ctl = self.env.controller()
        ctl.state["developer"]["active_pid"] = os.getpid()
        with self.assertRaises(ar.Stop) as stop:
            ctl.develop_step()
        self.assertIn("still running", stop.exception.reason)
        with self.assertRaises(ar.Stop):
            ctl.archive("manual")
        self.assertEqual([], self.env.developer.calls)

    def test_operational_failure_is_persisted_as_a_visible_stop(self):
        """F18."""
        ctl = self.env.controller()

        def boom():
            raise ar.CommandError(["gh", "api"], subprocess.CompletedProcess([], 1, "", "simulated network failure"))
        self.assertEqual(ar.EXIT_PAUSED, ar.guarded(ctl, "run", boom))
        persisted = ar.read_json(self.env.dir / "state.json")
        self.assertEqual("PAUSED", persisted["status"])
        self.assertIn("operational failure", persisted["status_reason"])
        self.assertIn("Workflow — paused", self.env.gh.headings()[-1])


class LimitTests(Base):
    def test_round_cap_blocks_visibly(self):
        """WF08: exhausted limits yield a visible blocked status."""
        env = Env("foreground", max_rounds=1)
        self.addCleanup(env.cleanup)
        env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "t")
        self.assertEqual("changes_required", ctl.review_round())
        sha = env.commit("feature.txt", "fix\n", "fix")
        env.push()
        self.assertIsNone(ctl.accept_report(report(env, "phase-1", [fixed("R1-F1", sha)])))
        ctl.transition("review", "t")
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertIn("review-round cap", stop.exception.reason)
        ctl.verify_inputs()
        ctl.publish_status(stop.exception.status, stop.exception.reason)
        self.assertIn("Workflow — blocked", env.gh.headings()[-1])

    def test_missing_dispositions_bad_citations_and_failing_required_validation_are_refused(self):
        env = self.env
        env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}]
        ctl = env.controller()
        self.submit(ctl, "phase-1")
        self.assertIn("missing dispositions", ctl.accept_report(report(env, "phase-1")))
        self.assertIn("without a commit", ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "fixed", "commits": [], "evidence": "x"}])))
        self.assertIn("not in the candidate history", ctl.accept_report(report(env, "phase-1", [fixed("R1-F1", "0" * 40)])))
        self.assertIn("unknown findings", ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "rebutted", "commits": [], "evidence": "x"},
            {"finding_id": "NOPE", "disposition": "rebutted", "commits": [], "evidence": "x"}])))
        failing = [{"command": "focused gate", "required": True, "result": "fail", "duration": "1s", "scope_and_limits": "x"}]
        self.assertIn("required validation", ctl.accept_report(report(env, "phase-1", validation=failing)))

    def test_no_progress_on_unchanged_candidate_blocks(self):
        env = self.env
        env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]},
                               {"decision": "changes_required", "findings": [finding("R1-F1")],
                                "assessments": [("R1-F1", "rebuttal_rejected")]}]
        ctl = env.controller()
        self.submit(ctl, "phase-1")
        with self.assertRaises(ar.Stop) as stop:
            self.submit(ctl, "phase-1", [{"finding_id": "R1-F1", "disposition": "rebutted", "commits": [],
                                          "evidence": "simulated rebuttal"}])
        self.assertIn("no progress", stop.exception.reason)
        self.assertEqual("accepted", ctl.state["rounds"][-1]["status"])

    def test_pending_restoration_unresolved_findings_and_required_checks_prevent_readiness(self):
        env = self.env
        ctl = env.controller()
        with self.assertRaises(ar.Stop) as stop:
            ctl.accept_report(report(env, "phase-1", restoration="pending"))
        self.assertIn("restoration", stop.exception.reason)
        head = git(env.work, "rev-parse", "HEAD")
        base = git(env.work, "merge-base", head, "origin/master")
        env.gh.required = ["ci/test"]
        ctl.state.update(checkpoint_index=1, ready_review={"round": 1, "H": head, "B": base, "M": base, "summary": {}},
                         findings={"R1-F1": {"blocks_readiness": True, "status": "open"}})
        reasons, _ = ctl.readiness(head)
        self.assertTrue(any("R1-F1" in r for r in reasons))
        self.assertTrue(any("restoration" in r for r in reasons))
        self.assertTrue(any("ci/test" in r for r in reasons))
        env.gh.check_results[head] = [{"name": "ci/test", "status": "completed", "conclusion": "success"}]
        self.assertFalse(any("ci/test" in r for r in ctl.readiness(head)[0]))


class TargetValidationTests(Base):
    def test_wrong_repository_branch_actor_or_pr_cannot_enroll_or_dispatch(self):
        """WF10, F15."""
        env = self.env
        args = env.args(state_root=str(env.root / "state2"))
        git(env.work, "remote", "set-url", "origin", "https://github.com/someone/else.git")
        with self.assertRaises(ar.UsageError):
            ar.enroll(args, github=env.gh, root=env.wf)
        git(env.work, "remote", "set-url", "origin", str(env.origin))
        git(env.work, "checkout", "--quiet", "-b", "feature/not-allowed")
        with self.assertRaises(ar.UsageError):
            ar.enroll(args, github=env.gh, root=env.wf)
        git(env.work, "checkout", "--quiet", env.branch)
        env.gh.pr_overrides = {"viewer": "mallory"}
        with self.assertRaises(ar.UsageError):
            ar.enroll(args, github=env.gh, root=env.wf)
        os.environ["AGENT_REVIEW_HOME"] = str(env.root / "home2")  # bypass the existing registry entry
        for override in ({"headRefName": "codex/other"}, {"isCrossRepository": True},
                         {"headRepositoryOwner": {"login": "fork-owner"}}, {"url": "https://github.com/evil/repo/pull/7"}):
            with self.subTest(override):
                env.gh.pr_overrides = override
                with self.assertRaises(ar.UsageError):
                    ar.enroll(args, github=env.gh, root=env.wf)
                with self.assertRaises(ar.UsageError):
                    env.controller().verify_inputs()

    def test_second_enrollment_for_active_branch_is_refused_across_state_roots(self):
        env = self.env
        with self.assertRaises(ar.UsageError) as refused:
            ar.enroll(env.args(state_root=str(env.root / "elsewhere")), github=env.gh, root=env.wf)
        self.assertIn("active enrollment", str(refused.exception))

    def test_configuration_comes_from_the_installed_root_and_owner_policy(self):
        """F14."""
        env = self.env
        outside = env.root / "evil.json"
        shutil.copy2(env.wf / "projects" / "test.json", outside)
        with self.assertRaises(ar.UsageError):
            ar.load_project(str(outside), env.wf)
        bad = ar.read_json(env.wf / "projects" / "test.json")
        bad["reviewer"]["model"] = "gpt-6.1-sol"
        ar.atomic_write_json(env.wf / "projects" / "bad.json", bad)
        with self.assertRaises(ar.UsageError) as refused:
            ar.load_project("bad", env.wf)
        self.assertIn("owner policy", str(refused.exception))
        with self.assertRaises(ar.UsageError):
            ar.find_enrollment(str(env.dir))

    def test_foreground_reports_are_bound_to_the_enrolled_session(self):
        """F16."""
        env = self.env
        ctl = env.controller()
        os.environ["CLAUDE_CODE_SESSION_ID"] = "someone-else"
        with self.assertRaises(ar.UsageError):
            ctl.accept_report(report(env, "phase-1"))
        os.environ["CLAUDE_CODE_SESSION_ID"] = FOREGROUND_SESSION
        env.developer.foreground = {"models": [OPUS, "claude-sonnet-5-5"], "efforts": ["high"]}
        with self.assertRaises(ar.Stop) as stop:
            ctl.accept_report(report(env, "phase-1"))
        self.assertIn("sonnet", stop.exception.reason)
        env.developer.foreground = {"models": [OPUS], "efforts": ["high", "low"]}
        with self.assertRaises(ar.Stop):
            ctl.accept_report(report(env, "phase-1"))
        env.developer.foreground = {"models": [OPUS], "efforts": ["high"]}
        self.assertIsNone(ctl.accept_report(report(env, "phase-1")))
        self.assertEqual(FOREGROUND_SESSION, ctl.state["last_report_provenance"]["session_id"])

    def test_handoff_requires_draft_without_requested_reviewers(self):
        """WF11."""
        env = self.env
        ctl = env.controller()
        self.to_finalize(ctl)
        env.gh.pr_overrides = {"reviewRequests": [{"login": "someone"}]}
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertIn("reviewers", stop.exception.reason)
        env.gh.pr_overrides = {"isDraft": False}
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertIn("no longer a draft", stop.exception.reason)
        env.gh.pr_overrides = {}
        self.assertEqual("AWAITING_OWNER_REVIEW", ctl.finalize())

    def test_superseded_handoff_is_detected_and_reopened(self):
        """F22."""
        env = self.env
        ctl = env.controller()
        self.to_finalize(ctl)
        self.assertEqual("AWAITING_OWNER_REVIEW", ctl.finalize())
        self.assertTrue(ctl.status(remote=True)["handoff_current"])
        env.commit("feature.txt", "after handoff\n", "material push after handoff")
        env.push()
        self.assertFalse(ctl.status(remote=True)["handoff_current"])
        ctl.verify_inputs()
        self.assertIn("ACTIVE", ctl.resume("owner pushed a change"))
        self.assertEqual("develop", ctl.state["phase"])
        self.assertEqual(1, len(ctl.state["superseded_handoffs"]))
        self.assertIn("superseded", env.gh.body)
        self.assertIn("Workflow — reopened", env.gh.headings()[-1])

    def test_changed_mission_snapshot_or_workflow_version_stops(self):
        """F21 and workflow activation."""
        env = self.env
        ctl = env.controller()
        ctl.verify_inputs()
        (env.wf / "prompts" / "reviewer.md").write_text("changed rules\n", encoding="utf-8")
        with self.assertRaises(ar.Stop) as stop:
            env.controller().verify_inputs()
        self.assertIn("activate", stop.exception.reason)
        ar.atomic_write_json(env.wf / "plans" / "another.json", {"x": 1})  # plans don't change the fingerprint
        fresh = env.controller()
        fresh.state["workflow"]["fingerprint"] = ar.workflow_fingerprint(env.wf)
        fresh.verify_inputs()
        Path(fresh.state["mission"]["snapshot"]).write_text("# Tampered snapshot\n", encoding="utf-8")
        with self.assertRaises(ar.Stop) as stop:
            fresh.verify_inputs()
        self.assertEqual("BLOCKED", stop.exception.status)
        self.assertIn("snapshot", stop.exception.reason)


class GeneralRepositoryTests(unittest.TestCase):
    """Any repository under C:\\Dev: built-in push, composed permissions, onboarding, generic plan."""

    def setUp(self):
        self.env = Env("foreground", cfg_overrides={"push_command": None})
        self.addCleanup(self.env.cleanup)

    def test_builtin_guarded_push_refuses_unsafe_pushes(self):
        env = self.env
        ctl = env.controller()
        head = env.commit("feature.txt", "v2\n", "v2")
        self.assertEqual(head, ar.guarded_push(ctl.state, ctl.cfg))
        self.assertEqual(head, git(env.origin, "rev-parse", f"refs/heads/{env.branch}"))
        (env.work / "feature.txt").write_text("dirty\n", encoding="utf-8")
        with self.assertRaises(ar.UsageError):
            ar.guarded_push(ctl.state, ctl.cfg)
        git(env.work, "checkout", "--", "feature.txt")
        git(env.work, "reset", "--quiet", "--hard", "HEAD~1")
        env.commit("feature.txt", "diverged\n", "diverged")
        with self.assertRaises(ar.UsageError) as refused:
            ar.guarded_push(ctl.state, ctl.cfg)
        self.assertIn("not an ancestor", str(refused.exception))
        git(env.work, "reset", "--quiet", "--hard", head)
        env.commit("config.txt", f"token=ghp_{'b' * 36}\n", "oops")
        with self.assertRaises(ar.UsageError) as refused:
            ar.guarded_push(ctl.state, ctl.cfg)
        self.assertIn("credential", str(refused.exception))
        self.assertEqual(head, git(env.origin, "rev-parse", f"refs/heads/{env.branch}"))

    def test_archive_uses_builtin_push_without_a_wrapper(self):
        env = self.env
        ctl = env.controller()
        env.reviewer.script = [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        for checkpoint in ("phase-1", "final"):
            ctl.accept_report(report(env, checkpoint))
            ctl.transition("review", "t")
            ctl.review_round()
        self.assertEqual("AWAITING_OWNER_REVIEW", ctl.finalize())
        self.assertEqual(git(env.work, "rev-parse", "HEAD"), git(env.origin, "rev-parse", f"refs/heads/{env.branch}"))

    def test_developer_settings_are_composed_and_pinned(self):
        env = self.env
        ctl = env.controller()
        settings = ar.read_json(env.dir / "developer-settings.json")["permissions"]
        helper = f"{env.wf.resolve().as_posix()}/agent_review.py"
        self.assertIn(f"Bash(python {helper} push --enrollment {ctl.state['enrollment_id']})", settings["allow"])
        self.assertIn(f"Bash(python {helper} run*)", settings["deny"])
        self.assertIn("Edit(./docs/agent-reviews/**)", settings["deny"])
        self.assertIn("Bash(dotnet test *)", settings["allow"])  # project extra
        self.assertIn(helper, ctl.render("developer-start.md", {**ctl.common_fields(), "push_command":
                      " ".join(ar.push_argv(ctl.cfg, env.wf, ctl.state["enrollment_id"]))}))
        ctl.verify_inputs()
        (env.dir / "developer-settings.json").write_text('{"permissions": {"allow": ["Bash(*)"]}}', encoding="utf-8")
        with self.assertRaises(ar.Stop) as stop:
            ctl.verify_inputs()
        self.assertIn("permission file", stop.exception.reason)

    def test_init_project_onboards_a_repository_into_the_machine_home(self):
        env = self.env

        class FakeRepoGitHub:
            def __init__(self, slug):
                self.slug = slug

            def viewer(self):
                return "owner"

            def _json(self, *args):
                return {"defaultBranchRef": {"name": "main"}}
        git(env.work, "remote", "set-url", "origin", "https://github.com/owner/another-repo.git")
        (env.work / "CLAUDE.md").write_text("# instructions\n", encoding="utf-8")
        args = argparse.Namespace(repo=str(env.work), name="another-repo", branch_prefix="claude/", allow=["npm test"],
                                  push_command=None, claude_config_dir=None, force=False)
        path, cfg = ar.init_project(args, root=env.wf, github_factory=FakeRepoGitHub)
        self.assertEqual(Path(os.environ["AGENT_REVIEW_HOME"]) / "projects" / "another-repo.json", path)
        self.assertEqual(("owner/another-repo", "main", "owner"),
                         (cfg["repository"], cfg["default_target_branch"], cfg["expected_actor"]))
        self.assertTrue(re.match(cfg["branch_pattern"], "claude/new-feature"))
        self.assertFalse(re.match(cfg["branch_pattern"], "main"))
        self.assertEqual(["CLAUDE.md"], cfg["instructions"])
        self.assertIn("Bash(npm test*)", cfg["developer"]["permissions"]["allow"])
        self.assertNotIn("push_command", cfg)
        self.assertEqual(cfg, ar.load_project("another-repo", env.wf))
        with self.assertRaises(ar.UsageError):
            ar.init_project(args, root=env.wf, github_factory=FakeRepoGitHub)  # no silent overwrite

    def test_generic_plan_takes_identity_from_the_mission_file(self):
        plan = ar.load_plan("single")
        named = ar.plan_for_mission(plan, Path("C:/Dev/x/ADD-SEARCH-MISSION.md"))
        self.assertEqual(("add-search-mission", "ADD-SEARCH-MISSION"), (named["mission_id"], named["title"]))
        self.assertEqual("black-tentacles", ar.plan_for_mission(ar.load_plan("black-tentacles"), Path("m.md"))["mission_id"])


class UnitTests(unittest.TestCase):
    def test_sanitize_redacts_whole_credentials_and_home(self):
        """F20."""
        # Markers are assembled at runtime so this source file never contains a literal key header
        # (the repository's guarded push correctly refuses files that do).
        dashes, label = "-" * 5, "PRIVATE " + "KEY"
        key = f"{dashes}BEGIN RSA {label}{dashes}\nMIIEsecretbody\nmoresecret\n{dashes}END RSA {label}{dashes}"
        text, count = ar.sanitize(f"ghp_{'a' * 36} at {Path.home()}\\x\n{key}\n" + json.dumps({"k": key}))
        self.assertEqual(3, count)
        for secret in ("ghp_", "secretbody", "moresecret", label, str(Path.home())):
            self.assertNotIn(secret, text)
        with self.assertRaises(ar.Stop):
            ar.sanitize(f"{dashes}BEGIN {label}{dashes}\ntruncated body without an end marker")

    def test_shipped_schemas_validate_examples(self):
        req = {"request_id": "r", "round": 1, "checkpoint_id": "c", "checkpoint_type": "final", "H": "h"}
        schema = ar.read_json(HERE / ar.REVIEW_SCHEMA_FILE)
        self.assertEqual([], ar.validate_schema(review_result(req, "ready_for_owner_review"), schema))
        bad = review_result(req, "ready_for_owner_review")
        bad["round"] = True
        self.assertTrue(ar.validate_schema(bad, schema))

    def test_project_and_plans_load_under_owner_policy(self):
        cfg = ar.load_project("kingmaker-gunslinger")
        self.assertEqual(("gpt-6.2-astra", "high"), (cfg["reviewer"]["model"], cfg["reviewer"]["effort"]))
        self.assertEqual(("opus", "high"), (cfg["developer"]["model"], cfg["developer"]["effort"]))
        for name in ("black-tentacles", "agent-review-setup"):
            self.assertEqual("final", ar.load_plan(name)["checkpoints"][-1]["type"])


if __name__ == "__main__":
    unittest.main()
