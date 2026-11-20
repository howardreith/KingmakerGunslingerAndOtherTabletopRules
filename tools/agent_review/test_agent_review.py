"""Behavior tests for the agent-review controller.

Real cooperating components: temporary Git repositories with a bare origin,
the controller's state, lock, publication ledger, archive commits and checks.
Only the external model (Codex/Claude) and GitHub boundaries are stubbed, and
every stubbed response below is a labeled simulation, not a model exchange.

Run: python -m unittest discover -s tools/agent_review -p "test_*.py" -v
"""
import argparse
import json
import os
import shutil
import socket
import subprocess
import tempfile
import unittest
from pathlib import Path

import agent_review as ar

HERE = Path(__file__).resolve().parent
OPUS = "claude-opus-5-5"


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
        self.edits = 0

    def viewer(self):
        return self.pr_overrides.get("viewer", "owner")

    def pr(self, number):
        head = git(self.env.origin, "rev-parse", f"refs/heads/{self.env.branch}")
        data = {"number": number, "url": f"https://github.com/owner/repo/pull/{number}", "state": "OPEN",
                "isDraft": True, "headRefName": self.env.branch, "headRefOid": head, "baseRefName": "master",
                "author": {"login": "owner"}, "reviewRequests": [], "body": self.body}
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

    def checks(self, number):
        return []

    def edit_body(self, number, body_file):
        self.body = Path(body_file).read_text(encoding="utf-8")
        self.edits += 1

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
                             "observed_effort": item.get("effort", "high"), "turn_models": [item.get("model", "gpt-6.2-astra")],
                             "turn_efforts": ["high"], "sandbox": "read-only", "approval_policy": "never",
                             "network": "restricted", "cli_version": "simulated", "usage": None, "plan_type": None,
                             "subagent_suspected": False, "source": "SIMULATED"}
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
    """Simulated headless Claude boundary: scripted actions commit real changes."""

    def __init__(self, env):
        self.env, self.script, self.calls, self.sessions = env, [], [], set()

    @property
    def requested(self):
        return {"model": "opus", "effort": "high"}

    def session_exists(self, session_id):
        return session_id in self.sessions

    def run(self, prompt, session_id, resume, cwd, log_path, stderr_path, soft_deadline_s, on_start=None):
        self.calls.append({"session_id": session_id, "resume": resume, "prompt": prompt})
        Path(stderr_path).write_text("", encoding="utf-8")
        self.sessions.add(session_id)
        action = self.script.pop(0)
        report = action(self.env)
        model = getattr(action, "model", OPUS)
        return {"init_model": model, "api_key_source": "none", "session_id": session_id, "main_models": [model],
                "result": "", "structured_output": report, "permission_denials": [], "model_usage": {},
                "cost_usd": 0.0, "is_error": False, "subtype": "success", "exit_code": 0,
                "deadline_exceeded": False, "duration_s": 0.1, "observed_effort": ["high"]}


def report(env, checkpoint, dispositions=(), status="checkpoint_ready", restoration="not_applicable"):
    return {"schema_version": "agent-review.developer.v1", "status": status, "checkpoint_id": checkpoint,
            "candidate_commit": git(env.work, "rev-parse", "HEAD"), "summary": "simulated developer report",
            "validation": [{"command": "python -m unittest", "result": "pass", "duration": "1s",
                            "scope_and_limits": "simulated"}],
            "dispositions": list(dispositions), "evidence_refs": [], "runtime_restoration": restoration,
            "blocker": None, "owner_decision_needed": None}


class Env:
    def __init__(self, mode="foreground", max_rounds=None):
        self.tmp = tempfile.TemporaryDirectory(prefix="agent-review-test-")
        root = Path(self.tmp.name)
        self.origin, self.work, self.branch = root / "origin.git", root / "work", "codex/feature"
        subprocess.run(["git", "init", "--quiet", "--bare", "-b", "master", str(self.origin)], check=True)
        subprocess.run(["git", "clone", "--quiet", str(self.origin), str(self.work)], check=True, capture_output=True)
        for key, value in (("user.name", "Test"), ("user.email", "test@example.invalid"), ("core.autocrlf", "false")):
            git(self.work, "config", key, value)
        git(self.work, "checkout", "--quiet", "-b", "master")
        self.commit("README.md", "base\n", "base")
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
                   state_root=str(root / "state"))
        cfg["limits"].update(retry_backoff_seconds=[0, 0])
        ar.atomic_write_json(self.wf / "projects" / "test.json", cfg)
        plan = {"schema_version": "agent-review.plan.v1", "mission_id": "fixture", "title": "Fixture mission",
                "checkpoints": [{"id": "phase-1", "type": "intermediate", "scope": "s", "exit_conditions": ["e"]},
                                {"id": "final", "type": "final", "scope": "s", "exit_conditions": ["e"]}]}
        ar.atomic_write_json(self.wf / "plans" / "test.json", plan)
        self.mission = root / "mission.md"
        self.mission.write_text("# Fixture mission\n", encoding="utf-8")
        self.gh, self.reviewer, self.developer = FakeGitHub(self), FakeReviewer(), FakeDeveloper(self)
        args = argparse.Namespace(project=str(self.wf / "projects" / "test.json"), worktree=str(self.work),
                                  plan=str(self.wf / "plans" / "test.json"), mission=str(self.mission), mode=mode,
                                  pr=7, title=None, target=None, max_rounds=max_rounds, state_root=None)
        self.dir, _ = ar.enroll(args, github=self.gh, root=self.wf)

    def controller(self):
        return ar.Controller(self.dir, github=self.gh, reviewer=self.reviewer, developer=self.developer,
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
        self.tmp.cleanup()


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
            return report(e, "final", [{"finding_id": "R2-F1", "disposition": "fixed", "commits": [fix["sha"]],
                                        "evidence": "simulated focused test"}])
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
        self.assertIn("R2-F1", env.developer.calls[2]["prompt"])
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
        self.assertEqual((True, []), ar.check_archive_only(ar.Git(env.work), source, archive, "docs/agent-reviews"))
        records = sorted(p.name for p in (env.work / "docs/agent-reviews/pr-7").iterdir())
        self.assertEqual(10, len(records))  # 7 published bodies + 3 review envelopes; final receipt not archived
        self.assertTrue(records[0].startswith("001-r01-request"))
        self.assertIn(f"Reviewed source `{source}`", env.gh.body)
        self.assertEqual("done", s["phase"])


class ArchiveCheckTests(Base):
    def test_only_new_plain_records_are_exempt(self):
        """WF09: transcript-only exemption is path-, operation- and mode-checked."""
        env, g = self.env, ar.Git(self.env.work)
        h = git(env.work, "rev-parse", "HEAD")
        a1 = env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "x\n", "archive")
        self.assertEqual((True, []), ar.check_archive_only(g, h, a1, "docs/agent-reviews"))
        cases = {
            "edit earlier record": lambda: env.commit("docs/agent-reviews/pr-7/001-r01-review.md", "changed\n", "edit"),
            "source change": lambda: env.commit("feature.txt", "sneaky\n", "src"),
            "bad name": lambda: env.commit("docs/agent-reviews/pr-7/notes.txt", "x\n", "name"),
            "outside dir": lambda: env.commit("docs/other/001-r01-review.md", "x\n", "dir"),
        }
        for label, make in cases.items():
            with self.subTest(label):
                git(env.work, "reset", "--quiet", "--hard", a1)
                bad = make()
                ok, reasons = ar.check_archive_only(g, h, bad, "docs/agent-reviews")
                self.assertFalse(ok)
                self.assertTrue(reasons)
        git(env.work, "reset", "--quiet", "--hard", a1)
        path = env.work / "docs/agent-reviews/pr-7/002-r01-envelope.json"
        path.write_text("{}\n", encoding="utf-8")
        git(env.work, "add", str(path))
        git(env.work, "update-index", "--chmod=+x", str(path))
        git(env.work, "commit", "--quiet", "-m", "exec")
        ok, reasons = ar.check_archive_only(g, h, git(env.work, "rev-parse", "HEAD"), "docs/agent-reviews")
        self.assertFalse(ok)
        self.assertIn("mode", reasons[0])
        self.assertFalse(ar.check_archive_only(g, a1, h, "docs/agent-reviews")[0])  # not an ancestor


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
        self.assertIn("HISTORICAL", env.gh.headings()[-1])

    def test_material_commit_after_ready_blocks_handoff(self):
        """WF04/WF09: a ready verdict does not cover a later material change."""
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        ctl = env.controller()
        self.submit(ctl, "phase-1")
        self.submit(ctl, "final")
        self.assertEqual("finalize", ctl.state["phase"])
        env.commit("feature.txt", "late change\n", "material change after review")
        env.push()
        with self.assertRaises(ar.Stop) as stop:
            ctl.finalize()
        self.assertEqual("BLOCKED", stop.exception.status)
        self.assertIn("material change", stop.exception.reason)
        self.assertNotEqual("AWAITING_OWNER_REVIEW", ctl.state["status"])

    def test_report_for_unpushed_or_dirty_candidate_is_refused(self):
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
        "usage limit (fatal)": ([{"decision": "checkpoint_passed", "stderr": "You've hit your usage limit", "exit_code": 1}], "usage limit", 1),
        "malformed JSON": ([{"raw": "{not json"}] * 3, "malformed JSON", 3),
        "truncated output": ([{"raw": '{"schema_version": "agent-review.review.v1", "decision": "check'}] * 3, "malformed", 3),
        "empty output": ([{"raw": ""}] * 3, "no final structured message", 3),
        "missing required field": ([{"decision": "checkpoint_passed", "drop": ["evidence_assessment"]}] * 3, "missing required", 3),
        "failed process": ([{"decision": "checkpoint_passed", "exit_code": 1}] * 3, "exited 1", 3),
        "mismatched request id": ([{"decision": "checkpoint_passed", "override": {"request_id": "forged"}}] * 3, "does not match", 3),
        "final decision on intermediate": ([{"decision": "ready_for_owner_review"}] * 3, "not valid for a intermediate", 3),
        "pass with blocking finding": ([{"decision": "checkpoint_passed", "findings": [finding("R1-F1")]}] * 3, "passing decision", 3),
    }

    def test_invalid_reviews_block_without_counting_a_round(self):
        for label, (script, needle, attempts) in self.CASES.items():
            with self.subTest(label):
                env = Env("foreground")
                try:
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
                finally:
                    env.cleanup()

    def test_transient_failure_then_valid_review_counts_once(self):
        env = Env("foreground")
        self.addCleanup(env.cleanup)
        env.reviewer.script = [{"raw": "{oops"}, {"decision": "checkpoint_passed"}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "test")
        self.assertEqual("checkpoint_passed", ctl.review_round())
        self.assertEqual(2, len(ctl.state["rounds"][-1]["attempts"]))
        self.assertEqual({"phase-1": 1}, ctl.state["rounds_completed"])


class PublicationTests(Base):
    def test_lost_response_and_duplicate_publication_are_reconciled(self):
        """WF06 and WF10 (forged marker by another account is ignored)."""
        env = self.env
        ctl = env.controller()
        # A foreign account copies a would-be marker prefix and shell text; it must not satisfy reconciliation.
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

        # Crash after sending but before recording: on restart the marker is found and nothing is reposted.
        restarted = env.controller()
        restarted.state["publications"]["r01-request"]["state"] = "pending"
        restarted.state["publications"]["r01-request"]["parts"][0]["comment_id"] = None
        restarted.publish("r01-request", "ignored: the preserved outgoing body is reused")
        self.assertEqual(1, len([c for c in env.gh.comments_list if c["user"] == "owner"]))
        self.assertEqual(ours[0]["id"], restarted.state["publications"]["r01-request"]["parts"][0]["comment_id"])

    def test_failed_publication_pauses_with_preserved_result_then_publishes_without_rerun(self):
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}]
        ctl = env.controller()
        ctl.accept_report(report(env, "phase-1"))
        ctl.transition("review", "test")
        real_post = env.gh.post_comment
        calls = {"n": 0}

        def post(number, body_file):
            calls["n"] += 1
            if calls["n"] >= 2:  # request succeeds; review publication fails repeatedly
                raise RuntimeError("simulated outage")
            return real_post(number, body_file)
        env.gh.post_comment = post
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertEqual("PAUSED", stop.exception.status)
        self.assertEqual("validated", ctl.state["active_round"]["status"])
        env.gh.post_comment = real_post
        resumed = env.controller()
        self.assertEqual("checkpoint_passed", resumed.review_round())
        self.assertEqual(1, len(env.reviewer.calls))  # successful review not rerun
        self.assertEqual({"phase-1": 1}, resumed.state["rounds_completed"])

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
        keys = list(restarted.state["publications"])
        self.assertEqual(len(keys), len(set(keys)))

    def test_single_writer_lock_and_stale_lock_recovery(self):
        ctl = self.env.controller()
        with ctl.lock("run"):
            with self.assertRaises(ar.UsageError):
                with self.env.controller().lock("review"):
                    pass
        lock = self.env.dir / "controller.lock"
        lock.write_text(json.dumps({"pid": 999999, "host": socket.gethostname(), "command": "run"}), encoding="utf-8")
        with ctl.lock("run"):
            self.assertEqual(os.getpid(), json.loads(lock.read_text(encoding="utf-8"))["pid"])
        self.assertFalse(lock.exists())

    def test_running_developer_session_is_never_resumed_concurrently(self):
        ctl = self.env.controller()
        ctl.state["developer"]["active_pid"] = os.getpid()
        with self.assertRaises(ar.Stop) as stop:
            ctl.develop_step()
        self.assertIn("still running", stop.exception.reason)
        self.assertEqual([], self.env.developer.calls)

    def test_wrong_developer_model_blocks(self):
        def sonnet(e):
            return report(e, "phase-1")
        sonnet.model = "claude-sonnet-5-5"
        self.env.developer.script = [sonnet]
        with self.assertRaises(ar.Stop) as stop:
            self.env.controller().develop_step()
        self.assertEqual("BLOCKED", stop.exception.status)
        self.assertIn("claude-sonnet-5-5", stop.exception.reason)


class LimitTests(Base):
    def test_round_cap_blocks_visibly(self):
        """WF08: exhausted limits yield a visible blocked status."""
        env = Env("foreground", max_rounds=1)
        self.addCleanup(env.cleanup)
        env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}]
        ctl = env.controller()
        self.assertEqual("changes_required", (lambda: (ctl.accept_report(report(env, "phase-1")),
                                                       ctl.transition("review", "t"), ctl.review_round())[2])())
        sha = env.commit("feature.txt", "fix\n", "fix")
        env.push()
        self.assertIsNone(ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "fixed", "commits": [sha], "evidence": "simulated"}])))
        ctl.transition("review", "t")
        with self.assertRaises(ar.Stop) as stop:
            ctl.review_round()
        self.assertIn("review-round cap", stop.exception.reason)
        ctl.publish_status(stop.exception.status, stop.exception.reason)
        self.assertIn("Workflow — blocked", env.gh.headings()[-1])

    def test_missing_dispositions_and_bad_citations_are_refused(self):
        env = self.env
        env.reviewer.script = [{"decision": "changes_required", "findings": [finding("R1-F1")]}]
        ctl = env.controller()
        self.submit(ctl, "phase-1")
        self.assertIn("missing dispositions", ctl.accept_report(report(env, "phase-1")))
        self.assertIn("without a commit", ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "fixed", "commits": [], "evidence": "x"}])))
        self.assertIn("not in the candidate history", ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "fixed", "commits": ["0" * 40], "evidence": "x"}])))
        self.assertIn("unknown findings", ctl.accept_report(report(env, "phase-1", [
            {"finding_id": "R1-F1", "disposition": "rebutted", "commits": [], "evidence": "x"},
            {"finding_id": "NOPE", "disposition": "rebutted", "commits": [], "evidence": "x"}])))

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

    def test_pending_restoration_and_unresolved_findings_prevent_readiness(self):
        env = self.env
        ctl = env.controller()
        with self.assertRaises(ar.Stop) as stop:
            ctl.accept_report(report(env, "phase-1", restoration="pending"))
        self.assertIn("restoration", stop.exception.reason)
        head = git(env.work, "rev-parse", "HEAD")
        ctl.state.update(checkpoint_index=1, ready_review={"round": 1, "H": head, "B": head, "M": head, "summary": {}},
                         findings={"R1-F1": {"blocks_readiness": True, "status": "open"}})
        reasons, _ = ctl.readiness(head)
        self.assertTrue(any("R1-F1" in r for r in reasons))
        self.assertTrue(any("restoration" in r for r in reasons))


class TargetValidationTests(Base):
    def test_wrong_repository_branch_actor_or_pr_cannot_enroll_or_dispatch(self):
        """WF10."""
        env = self.env
        base = dict(project=str(env.wf / "projects" / "test.json"), worktree=str(env.work),
                    plan=str(env.wf / "plans" / "test.json"), mission=str(env.mission), mode="foreground", pr=7,
                    title=None, target=None, max_rounds=None, state_root=str(Path(env.tmp.name) / "state2"))
        git(env.work, "remote", "set-url", "origin", "https://github.com/someone/else.git")
        with self.assertRaises(ar.UsageError):
            ar.enroll(argparse.Namespace(**base), github=env.gh, root=env.wf)
        git(env.work, "remote", "set-url", "origin", str(env.origin))
        git(env.work, "checkout", "--quiet", "-b", "feature/not-allowed")
        with self.assertRaises(ar.UsageError):
            ar.enroll(argparse.Namespace(**base), github=env.gh, root=env.wf)
        git(env.work, "checkout", "--quiet", env.branch)
        env.gh.pr_overrides = {"viewer": "mallory"}
        with self.assertRaises(ar.UsageError):
            ar.enroll(argparse.Namespace(**base), github=env.gh, root=env.wf)
        env.gh.pr_overrides = {"headRefName": "codex/other"}
        with self.assertRaises(ar.UsageError):
            ar.enroll(argparse.Namespace(**base), github=env.gh, root=env.wf)
        with self.assertRaises(ar.UsageError):
            env.controller().verify_inputs()
        env.gh.pr_overrides = {"url": "https://github.com/evil/repo/pull/7"}
        with self.assertRaises(ar.UsageError):
            env.controller().verify_inputs()

    def test_second_enrollment_for_active_branch_is_refused(self):
        env = self.env
        args = argparse.Namespace(project=str(env.wf / "projects" / "test.json"), worktree=str(env.work),
                                  plan=str(env.wf / "plans" / "test.json"), mission=str(env.mission),
                                  mode="foreground", pr=7, title=None, target=None, max_rounds=None, state_root=None)
        with self.assertRaises(ar.UsageError):
            ar.enroll(args, github=env.gh, root=env.wf)

    def test_handoff_requires_draft_without_requested_reviewers(self):
        """WF11."""
        env = self.env
        env.reviewer.script = [{"decision": "checkpoint_passed"}, {"decision": "ready_for_owner_review"}]
        ctl = env.controller()
        self.submit(ctl, "phase-1")
        self.submit(ctl, "final")
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

    def test_changed_mission_or_workflow_version_stops_until_reviewed(self):
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
        env.mission.write_text("# Rewritten mission\n", encoding="utf-8")
        with self.assertRaises(ar.Stop) as stop:
            fresh.verify_inputs()
        self.assertEqual("BLOCKED", stop.exception.status)


class UnitTests(unittest.TestCase):
    def test_sanitize_redacts_tokens_and_home(self):
        text, count = ar.sanitize(f"ghp_{'a' * 36} at {Path.home()}\\x")
        self.assertEqual(1, count)
        self.assertNotIn("ghp_", text)
        self.assertNotIn(str(Path.home()), text)

    def test_shipped_schemas_validate_examples(self):
        req = {"request_id": "r", "round": 1, "checkpoint_id": "c", "checkpoint_type": "final", "H": "h"}
        schema = ar.read_json(HERE / ar.REVIEW_SCHEMA_FILE)
        self.assertEqual([], ar.validate_schema(review_result(req, "ready_for_owner_review"), schema))
        bad = review_result(req, "ready_for_owner_review")
        bad["round"] = True
        self.assertTrue(ar.validate_schema(bad, schema))

    def test_project_and_plans_load(self):
        cfg = ar.load_project("kingmaker-gunslinger")
        self.assertEqual("gpt-6.2-astra", cfg["reviewer"]["model"])
        self.assertEqual(("opus", "high"), (cfg["developer"]["model"], cfg["developer"]["effort"]))
        for name in ("black-tentacles", "agent-review-setup"):
            plan = ar.load_plan(name)
            self.assertEqual("final", plan["checkpoints"][-1]["type"])


if __name__ == "__main__":
    unittest.main()
