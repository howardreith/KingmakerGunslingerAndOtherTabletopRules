<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 key=r01-request part=1/1 sha256=705e40e4b63561c2fb9a82566fc91866e065b03fade0b2cd76e72ac6ecc95f56 -->
### Claude Opus — review request — round 1 — candidate `10802bc0ea`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34-r01-4bf66b` |
| Checkpoint | `final` (final) |
| Candidate H | `10802bc0ea21fe5051c08724804428bad9557920` |
| Target B (`codex/s7-visual-audio-remediation`) | `0542b0592a668aad0c62c66d5f2dc862460818fe` |
| Merge base M | `0542b0592a668aad0c62c66d5f2dc862460818fe` |
| Mission | `s7-recovery-c1-mission` sha256 `01b1fa7d63` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 1 of 2 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C1 (owner instruction 2026-11-25, three scoped repair PRs) on H=10802bc0ea, PR #221 targeting the frozen #219 branch (0542b0592a). Inherited parent findings R7-F1 and R7-F6 (round-7 review https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7147252673); R6-F5 disposition recorded in the recovery manifest from existing receipts (no build or run). R7-F1: Sprint7ObserverPolicy.CallbackLossSettled now separates an absent required property ("missing:<path>") from an explicit null, and recognizes explicit null in the collector's in-memory form (any JValue whose value is null, including String-typed) and in the serialized form; Utc/Text helpers follow the same rule. R7-F6: the selected survivor and other playing IDs must be valid and distinct and must match the settled other record and both terminal survivor records; a Round7 collector row must carry otherRetirementSettled.survivorStopAttemptsThroughWindow (absent is unknown, nonzero fails); Round6 rows (collectors f3bfab1303/740d94a995, which emitted no count) are judged on their own window-end cue stop fields. Stop requests, completion callbacks and native playback queries remain separate inputs; terminal playback rules unchanged. S7Cue now delegates to the shared CueRow producer so the domain case builds a genuinely collector-shaped live row. The expectation that reproduced the wrong live verdict is replaced by the corrected one; the recorded 224949Z in-run FAIL stays as history. ACCEPTANCE BOUNDARY: code and domain behavior only. The changed compiled evaluator has NOT been executed in game; its native qualification is owned by C3's combined candidate and run. No product package, harness or session in C1.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter sprint7-visual | yes | pass | 5.0s | 22 tests on H. Includes impact-cue-playback-verdicts: the persisted 224949Z row and its collector-shaped live counterpart (cue records built by the collector's own CueRow, String-typed nulls, never serialized) both characterize; omitted required fields are missing in both forms; actual stop requests, stop times, completion callbacks and ends after disposal refuse; substituted window-end, disposal and settled-other IDs, a twice-selected ID, an invalid ID and a nonzero or omitted explicit survivor stop count refuse; the accepted terminal playback rules still refuse playing/unknown/missing; a stop request without the other source's native end is not its retirement; retained round-6 rows judged by their own collector fields (202648Z characterized, 200320Z not-both-playing); the 223809Z/224949Z in-run verdicts stay recorded as history. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter summon-playtest-r6 | yes | pass | 6.2s | 11 tests on H. Cast cue lifecycle (cue record shape is shared through CueRow). |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter sprint22- | yes | pass | 5.4s | 11 tests on H. Ankylosaurus tail control and impact cue bound. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter native-profile | yes | pass | 4.7s | 9 tests on H. Native profile policy (RuntimeTesting compiled). |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, uncommitted C1 source) | no | pass | 25.3s | 22 tests. Counted in the shared 45-minute recovery allowance (recovery total after the controller gates: 48.2s wall; receipts inside the gates 10.9s). |
| Native execution of the changed evaluator | no | not_run | 0s | Deliberately deferred by the owner instruction: C3's combined native qualification executes this evaluator in game. No product package, harness build or game session in C1. |

**Evidence references:**
- docs/missions/sprint7/recovery/S7-RECOVERY-C1-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/r7/MANIFEST.json
- docs/evidence/sprint7/r6/MANIFEST.json
- docs/GUNSLINGER-SPRINT7-RECORD.md
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7147252673

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
