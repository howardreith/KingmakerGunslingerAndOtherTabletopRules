"""Live headless smoke: REAL Claude Opus developer and REAL Codex GPT-6.2 Astra reviewer.

Skipped unless AGENT_REVIEW_LIVE=1. It exercises the exact `run` continuation
path used for feature missions (headless Claude launch with explicit session,
dontAsk permission profile, structured report, controller review round, Astra
review, finalize and archive) against a harmless temporary repository. Only
GitHub is simulated (FakeGitHub, clearly not a real PR); both model exchanges
are real and consume subscription quota.

    $env:AGENT_REVIEW_LIVE = "1"; $env:AGENT_REVIEW_LIVE_OUT = "<dir>"
    python -m unittest tools/agent_review/test_live_smoke.py -v
"""
import json
import os
import time
import unittest
from pathlib import Path

import agent_review as ar
from test_agent_review import Env, git

MISSION = """# Fixture mission: calculator helper

Add `calc.py` exposing `add(a, b)` that returns the sum of two numbers, and
`test_calc.py` with a `unittest` test covering integers and a negative number.
Run the test with `python -m unittest test_calc`. Commit, then push with the
push command your instructions give. Keep the change minimal; nothing else is required.
"""

EXTRA_PERMISSIONS = {"allow": ["Bash(python -m unittest*)", "PowerShell(python -m unittest*)"], "deny": []}

PLAN = {"schema_version": "agent-review.plan.v1", "mission_id": "live-smoke", "title": "Live smoke fixture",
        "notes": "Harmless fixture repository; no game or product code.",
        "checkpoints": [{"id": "final", "type": "final", "scope": "The complete fixture mission.",
                         "exit_conditions": ["calc.add exists and is tested; the test passes; work is pushed."]}]}


@unittest.skipUnless(os.environ.get("AGENT_REVIEW_LIVE") == "1", "live model smoke; set AGENT_REVIEW_LIVE=1")
class LiveHeadlessSmoke(unittest.TestCase):
    def test_real_models_headless_cycle_with_simulated_github(self):
        base = ar.read_json(Path(__file__).resolve().parent / "projects" / "kingmaker-gunslinger.json")
        developer = dict(base["developer"], permissions=EXTRA_PERMISSIONS, soft_deadline_minutes=30)
        limits = dict(base["limits"], max_review_rounds_per_checkpoint=2, max_developer_invocations=4,
                      retry_backoff_seconds=[10, 30])
        # No push_command: the developer uses the controller's built-in guarded push helper (general-repo path).
        env = Env("headless", cfg_overrides={"push_command": None, "developer": developer, "limits": limits,
                                             "instructions": []},
                  plan=PLAN, mission_text=MISSION, extra_files={".gitignore": "__pycache__/\n"})
        self.addCleanup(env.cleanup)
        ctl = ar.Controller(env.dir, github=env.gh, sleep=time.sleep, workflow_root=env.wf)
        started = time.monotonic()
        try:
            outcome = ctl.run_loop()
        except ar.Stop as stop:
            outcome = f"{stop.status}: {stop.reason}"
        summary = {"outcome": outcome, "duration_s": round(time.monotonic() - started),
                   "developer_log": ctl.state["developer"].get("log"),
                   "rounds": [{k: r.get(k) for k in ("round", "status", "decision", "H", "attempts")}
                              for r in ctl.state["rounds"]],
                   "findings": ctl.state.get("findings"),
                   "simulated_github_headings": env.gh.headings(),
                   "files_on_branch": git(env.work, "ls-tree", "-r", "--name-only", "HEAD").splitlines()}
        out = os.environ.get("AGENT_REVIEW_LIVE_OUT")
        if out:
            Path(out).mkdir(parents=True, exist_ok=True)
            Path(out, "live-smoke-summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
            for comment in env.gh.comments_list:
                Path(out, f"comment-{comment['id']}.md").write_text(comment["body"], encoding="utf-8")
        print(json.dumps({k: summary[k] for k in ("outcome", "duration_s", "simulated_github_headings")}, indent=2))
        self.assertEqual("AWAITING_OWNER_REVIEW", outcome)
        self.assertTrue(all(m.startswith("claude-opus-") for e in summary["developer_log"] for m in e["main_models"]))
        self.assertIn("calc.py", summary["files_on_branch"])


if __name__ == "__main__":
    unittest.main()
