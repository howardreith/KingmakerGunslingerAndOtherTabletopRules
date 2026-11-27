<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r01-request part=1/1 sha256=fa33066569c08cdbf2093c9d81bbbf0b43e0040893cc49e9a5710f17e6f6c5fd -->
### Claude Opus — review request — round 1 — candidate `7fff8b34f6`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r01-0331ea` |
| Checkpoint | `final` (final) |
| Candidate H | `7fff8b34f6d925d91e80beddb6501cfad249e33d` |
| Target B (`codex/s7-recovery-c2-ghaele-late-units`) | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Merge base M | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Mission | `s7-recovery-c3-mission` sha256 `b95649a59a` |
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

Sprint 7 recovery child C3, review 1 of 2, on H=7fff8b34f6, PR #223 targeting C2's branch at its archive A eab90a5ac8 (C2 AWAITING_OWNER_REVIEW, handoff current; C1 owner-accepted). Parent findings R4-F1 and R7-F2 to R7-F5 repaired together on the effects phase's acquisition-to-admission contract, reusing its isolation and sequence: IsolatedCell (cell setup/execution/disposal exceptions are sticky isolation failures); Sprint7Abandonment (every driver decides on the complete child outcome, unexpected child failures reach the phase by name and survive the repair, the exact injected condition is repaired once on every exit including disposal during the recovery wait, failed repairs kept, a run-to-completion mode, a declared settle wait before the safety readback); Sprint7OwnedCommands (process retained before interruption, independent command releases, the Dust ledger owns executions by retained process); Sprint7HookAttempt and a caller-chosen projectile-trace identity (attempt registered before set-up, explicit readback by identity, null result or unreadable readback never zero); Sprint7OwnedRequests (one retirement request per cue source, failure named and sticky, unsettled source a residual). The two-cue overlap is reached by declared same-frame delivery of fresh resolved tail hits to each source's own production handler (constructed setup, real playing IDs). Evidence: focused sprint7-visual.effects-ownership-recovery (the five findings through the real orchestration with controlled boundaries) and sprint7-visual.combined-run-verdicts-agree; combined native qualification 20261126T054228Z PASS 41/41 on the contingency candidate after the first candidate's session found two concrete qualification defects (both recorded). C1's deferred native check is fulfilled: the revised evaluator judged the row live (null) and the persisted row of the same run, and its live-shaped counterpart, receive the same verdict; recorded in the manifest without amending C1. Ghaele stays at C2's harness identity. Recovery validation 42 min 28.9 s of 45; candidates 2 of 2.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter sprint7-visual | yes | pass | 3.9s | 25 tests on H, production compilation; receipt 2.2s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter summon-playtest-r6 | yes | pass | 3.2s | 11 tests on H, production compilation; receipt 1.6s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter native-profile | yes | pass | 3.5s | 9 tests on H, production compilation; receipt 1.8s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter loader-compat-probe | yes | pass | 3.1s | 5 tests on H, production compilation; receipt 1.6s; worktree unchanged. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local; four runs) | no | pass | 15.6s + 19.4s + 3.9s + 20.2s + 3.9s | Two FAILED on compile errors (an unassigned focused-injection field in production; driver fields unassigned in the domain project before its test existed); then PASS 24 tests; PASS 24 after the contingency correction; PASS 25 with the same-run agreement test. All counted. |
| Build-Local -DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual (candidate 1 of 2) on 988d40bb1e | no | pass | 7m48.6s | Package 424240171ea7e16c529d423dd61267a90e2c14c97ac214bfbd715d7f519d9484, DLL eac800c3ed003cd2e3aab03008d6aeb5c43f0ecf4b3bf690cd87d0518df3c339; 24 selected tests. |
| Invoke-LoaderCompatSession.ps1 s7-c3-fx.json on candidate 1 (owner-0.33.0, C2 harness d77754ba) | no | fail | 5m28s | Session 20261126T051633Z FAILED 3 checks: s7-dust-cell-conditions-released (the unowned-execution control's 3-round Sickened expired before cleanup because the new pre-delivery step ran after it), s7-dust-abandoned-unwound (the shared driver read "recolours live: 1" before the Dust release wait the old driver had) and hence s7-effects-owned-state-restored (samples refused). In that run the callback-loss verdict (null), the declared overlap, the callback abandoned and set-up-failed and trace set-up-failed controls and the pre-delivery release passed. Restoration completed, owner files verified. |
| Build-Local (same arguments; candidate 2 of 2, the authorized contingency) on 6abd692fa7 | no | pass | 6m58.4s | Package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46, DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a; 24 selected tests. Correction: Sprint7Abandonment.AfterRepair (a declared settle wait before the safety readback; the Dust driver uses S7WaitDustReleased) and the pre-delivery step moved before the unowned-execution control. |
| Invoke-LoaderCompatSession.ps1 s7-c3-fx.json on candidate 2 (owner-0.33.0, C2 harness d77754ba) | no | pass | 6m18s | Session 20261126T054228Z PASS, 41/41 checks; candidate de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46 / DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a from 6abd692fa7 (H differs only in tests and records). Effects phase only (the R6 Trumpet/Vrock launch is unaffected and not repeated). The phase isolation restored with all seven cells admitted; all four drivers proven with no unexpected child failure; overlap schedule delivery 0 same frame, both playing (IDs 544, 545); live callback-loss verdict null (C1's CallbackLossRound7); pre-delivery release: process started, not delivered, retained, ended after the wait, none live; unowned control kept. Transaction completed, owner files verified, registry restored. Window 05:34Z-05:48Z agreed with the other sessions on DATA. |
| Shared recovery ledger | no | pass | 42m28.9s | Cumulative recovery validation after these gates: 42 min 28.9 s of 45, remaining 2 min 31.1 s (the committed ledger stops at 42 min 14.6 s, before these controller gates: 14.3s batch wall, receipts 7.2s). Product candidates 2 of 2 (no further package or native session is possible within the allowance and ceiling). Failed validation attempts in the recovery: ten (three in C3). |

**Evidence references:**
- docs/missions/sprint7/recovery/S7-RECOVERY-C3-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c3/MANIFEST.json
- docs/evidence/sprint7/recovery-c3/20261126T054228Z-owner-0.33.0-s7-c3-fx-01-fx.json
- docs/evidence/sprint7/recovery-c3/20261126T054228Z-callback-loss-row.json
- docs/evidence/sprint7/recovery-c3/20261126T051633Z-owner-0.33.0-s7-c3-fx-01-fx.json
- src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs
- src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs
- scripts/compatibility/loader-compat/sessions/s7-c3-fx.json

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
