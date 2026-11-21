<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r04-request part=1/1 sha256=a5660b22601bf51066954ec8455466e7a1ed114bee74861e0d0a460223f2a7d9 -->
### Claude Opus — review request — round 4 — candidate `a7a008ee84`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-agent-review-workflow-20261120170436-r04-50061e` |
| Checkpoint | `controller-core` (intermediate) |
| Candidate H | `a7a008ee849d4971f3e5930f5369a8dc37c03654` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `agent-review-setup` sha256 `0ae972cfa8` |
| Workflow | `704fc9ee26` |
| Developer observed | foreground session `8d9c01f2-9d44-41c8-a695-d47ecffee8d2`: models ['claude-opus-5-5'], effort ['high'] (CLI-recorded transcript) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `high`, read-only sandbox |
| Round budget | 3 of 5 |

**Scope:** Controller, versioned reviewer/developer schemas, prompts, model/permission configuration and behavior tests (tools/agent_review/).

**Exit conditions:**
- Controller enforces candidate identity (H/B/M), explicit model selection with requested/observed provenance, schema/request validation, single-writer lock, idempotent publication, bounded retries, round budget, stop/resume and the transcript-only archive check.
- Behavior tests with real temporary Git repositories cover protocol rows WF04-WF10 and WF12; only the model and GitHub boundaries are stubbed.
- No path lets a malformed, failed, wrong-model or stale review produce readiness.

**Developer summary:**

Round-2 corrections for all 22 open findings in 4adde14fc0, each with a focused regression test (63 behavior tests, see the candidate-bound receipt in evidence/). The real-model live smoke was re-run on this candidate's headless path; its result is listed in evidence_refs when available. Live smoke run 5 on 4adde14 exposed a prompt weakness (a real developer treated two denied compound commands as total shell denial and reported blocked); the candidate's prompts now list allowed prefixes and the rule for simple commands, and run 6 reached AWAITING_OWNER_REVIEW with real Opus and Astra.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python -m unittest discover -s tools/agent_review -p "test_*.py" -v (clean detached worktree of the candidate) | yes | pass | see receipt | Behavior suite; real Git/state/OS locks/archive commits; model and GitHub boundaries simulated with labeled responses. |
| AGENT_REVIEW_LIVE=1 python -m unittest tools/agent_review/test_live_smoke.py (run 6) | no | pass | 64s | Real headless Claude Opus + real Codex gpt-6.2-astra on a harmless temporary repository; GitHub simulated. Run on the working tree equal to this candidate's tools/agent_review. |

**Evidence references:**
- C:/Dev/KingmakerGunslingerLab/agent-review-state/kmg-codex-agent-review-workflow-20261120170436/evidence/test-receipt-a7a008ee849d4971f3e5930f5369a8dc37c03654.md
- C:/Dev/KingmakerGunslingerLab/agent-review-state/live-smoke-2026-11-20-run6/live-smoke-summary.json

**Prior findings to assess:** `R1-F1`, `R1-F11`, `R1-F12`, `R1-F13`, `R1-F14`, `R1-F16`, `R1-F17`, `R1-F18`, `R1-F19`, `R1-F22`, `R1-F3`, `R1-F4`, `R1-F6`, `R1-F7`, `R1-F8`, `R1-F9`, `R3-F1`, `R3-F2`, `R3-F3`, `R3-F4`, `R3-F5`, `R3-F6`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
