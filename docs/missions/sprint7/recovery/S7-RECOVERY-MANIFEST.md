# Sprint 7 recovery manifest (three scoped repair PRs)

Authority: owner instruction "SPRINT 7 RECOVERY — THREE SCOPED REPAIR PRS", 2026-11-25. It is stored outside the worktree at
`<lab>/owner-guidance/S7-RECOVERY-THREE-REPAIR-PRS-2026-11-25.md` and reproduced in each child mission under this folder.

## Frozen parent

| Item | Identity |
|---|---|
| PR | #219, draft, `codex/s7-visual-audio-remediation`, base `master` |
| Frozen head | `0542b0592a668aad0c62c66d5f2dc862460818fe` (verified on the remote, 2026-11-25) |
| Enrollment | `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`: BLOCKED, review cap 7/7, not resumed, not cancelled |
| Round-7 reviewed H | `3e2baea4b486f3f416895127e4c67797968a8f28` |
| Round-7 review | https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7147252673 |
| Archive A | none (the parent never reached finalize) |
| Candidate 3 source | `9c39b1ff7f95c995ead0ecdfca2e679e62506825` |
| Candidate 3 package / DLL | `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413` / `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3` |
| Round-7 harness | `ee7742a493eb3879cffa30c69879592a51da60a3b7205a902832d8e4395ea373` |

Every parent finding, amendment, session, FAIL, budget and package identity stays as recorded in
`docs/GUNSLINGER-SPRINT7-RECORD.md`. Parent findings stay historically open until reconciled explicitly; a child's PASS is
not a retrospective parent PASS. Accepted scope stays accepted: R4-F5, the bounded Ghaele scope, the controller-admission
disposition, the approved wording and the released Sickened mechanics. Visual and listening acceptance remain the owner's.

## Finding map

| Parent finding | Repair PR | Scope | Evidence (to be filled by the child) | Depends on | Remaining integration gate |
|---|---|---|---|---|---|
| R7-F1 live and persisted callback verdicts disagree | C1 `codex/s7-recovery-c1-cue-evaluator` (PR #221) | Strict evaluator: absent vs explicit null in both representations | `sprint7-visual.impact-cue-playback-verdicts`: the persisted `…224949Z` row and its collector-shaped live counterpart (cue records through the collector's own `CueRow`, String-typed nulls, never serialized) both characterize; omitted fields are "missing" in both | #219 | Native execution of the changed evaluator: C3's combined qualification |
| R7-F6 settled verdict not bound to cue IDs and stop count | C1 (PR #221) | Identity binding; explicit survivor stop count consumed | Same case: substituted window-end, disposal and settled-other IDs, the same ID selected twice, an invalid ID, a nonzero or omitted explicit stop count all refuse, in both forms; retained round-6 rows judged by their own collector's fields (`Round6`) | #219 | As R7-F1 |
| R6-F5 round-7 expenditure and harness suffix | C1 (records disposition only) | Confirm the frozen head's record and planning row from existing receipts | This manifest, below | #219 | Parent records reconciliation at integration |
| R5-F1 unaccounted late Ghaele | C2 `codex/s7-recovery-c2-ghaele-late-units` | Caller reconciles every correlated late unit with the destruction owner; outstanding units fail ProbeSlot isolation | C2 tests and short lifecycle session on candidate 3 with a rebuilt harness | C1 | C2 harness identity carried into C3 and the parent |
| R4-F1 independent release failures past the abandonment drivers | C3 `codex/s7-recovery-c3-effects-ownership` | Complete child outcome in both drivers | C3 tests and combined native qualification | C2 | Combined candidate |
| R7-F2 ordinary Dust executions interrupted before retention | C3 | Retain the exact process before interruption; independent command releases | As R4-F1 | C2 | As R4-F1 |
| R7-F3 incomplete hook setup rollback | C3 | Register the acquisition attempt before partial success; rollback outcome to the isolation | As R4-F1 | C2 | As R4-F1 |
| R7-F4 failed direct cue-source retirement | C3 | Request and result kept apart; one request, named failure | As R4-F1 | C2 | As R4-F1 |
| R7-F5 Dust driver disposed during recovery | C3 | Unwind-safe repair of the exact injected condition | As R4-F1 | C2 | As R4-F1 |

Dependency order: #219 (frozen) ← C1 ← C2 ← C3. The children are not independently mergeable to master.

## R6-F5 disposition (records only, no build or run)

Confirmed at the frozen head `0542b0592a` from the existing receipts:
- the record carries 65 min 42 s for rounds 6 and 7: 39 min 4 s plus 25 min 58 s of round-7 commands plus the round-7
  gates at 40.1 s wrapper wall time, whose receipts record 19.1 s (sum of the eleven `duration_s` fields);
- 118 selected-test executions on `3e2baea4b4`: 22 `sprint7-visual` and 96 neighbour, as in the eleven gate logs;
- the harness suffix `…a373` everywhere (the wrong `…a395` was corrected in `0542b0592a`);
- the planning row states the BLOCKED stop, the open findings and 65 min 42 s of 75.

## Recovery validation ledger

The recovery has one aggregate allowance of 45 minutes of new validation-command wall time across C1 to C3, and at most
two new product candidates (one targeted). The old unused 9 min 18 s is not added. Earlier spend stays separate: 65 min
42 s in rounds 6 and 7, and all earlier rounds as recorded.

| Child | Command | Result | Time | Running total |
|---|---|---|---|---|
| C1 | Focused gate `sprint7-visual` (local, uncommitted C1 source) | PASS, 22 tests | 25.3 s | 25.3 s |
