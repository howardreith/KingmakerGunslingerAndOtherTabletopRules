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
| R7-F1 live and persisted callback verdicts disagree | C1 `codex/s7-recovery-c1-cue-evaluator` (PR #221) | Strict evaluator: absent, undefined, unreadable and out-of-range vs explicit null in both representations; one row-based round-7 entry point for the in-game collector and every reader | `sprint7-visual.impact-cue-playback-verdicts`: the persisted `…224949Z` row and its collector-shaped live counterpart (cue records through the collector's own `CueRow`, String-typed nulls, never serialized) both characterize; omitted fields are "missing" in both | #219 | Native execution of the changed evaluator: C3's combined qualification |
| R7-F6 settled verdict not bound to cue IDs and stop count | C1 (PR #221) | Identity binding and stop/callback reconciliation across every snapshot the collector emitted | Same case: substituted retirement-frame, window-end, disposal and settled-other IDs, a consistent survivor substitution the source inventory contradicts, the same ID twice, invalid and oversized IDs; positive retirement-frame or through-window stop counts, stop reasons, stop results, stop times and window-sample ends; placeholder or uncounted withheld callbacks; all refuse, in both forms. Retained round-6 rows are judged through `CallbackLossRound6` by their own collector generation (`Round6First` for `f3bfab1303`, `Round6Second` for `740d94a995`) | #219 | As R7-F1 |
| R6-F5 round-7 expenditure and harness suffix | C1 (records disposition only) | Confirm the frozen head's record and planning row from existing receipts | This manifest, below | #219 | Parent records reconciliation at integration |
| R5-F1 unaccounted late Ghaele | C2 `codex/s7-recovery-c2-ghaele-late-units` | Caller reconciles every correlated late unit with the destruction owner; outstanding units fail ProbeSlot isolation | C2 tests and short lifecycle session on candidate 3 with a rebuilt harness | C1 | C2 harness identity carried into C3 and the parent |
| R4-F1 independent release failures past the abandonment drivers | C3 `codex/s7-recovery-c3-effects-ownership` | Complete child outcome in both drivers | C3 tests and combined native qualification | C2 | Combined candidate |
| R7-F2 ordinary Dust executions interrupted before retention | C3 | Retain the exact process before interruption; independent command releases | As R4-F1 | C2 | As R4-F1 |
| R7-F3 incomplete hook setup rollback | C3 | Register the acquisition attempt before partial success; rollback outcome to the isolation | As R4-F1 | C2 | As R4-F1 |
| R7-F4 failed direct cue-source retirement | C3 | Request and result kept apart; one request, named failure | As R4-F1 | C2 | As R4-F1 |
| R7-F5 Dust driver disposed during recovery | C3 | Unwind-safe repair of the exact injected condition | As R4-F1 | C2 | As R4-F1 |

Dependency order: #219 (frozen) ← C1 ← C2 ← C3. The children are not independently mergeable to master.

## R6-F5 disposition (records only, no build or run)

Confirmed at the frozen head `0542b0592a` from the existing receipts, extracted (sanitized, with receipt and log hashes)
to `docs/evidence/sprint7/r7/GATES-3e2baea4b4.json`:
- the record carries 65 min 42 s for rounds 6 and 7: 39 min 4 s plus 25 min 58 s of round-7 commands plus the eleven
  round-7 gates at 40.1 s batch wall time (the sum of the per-call wrapper times is 37.6 s; the rest is loop and parsing
  overhead between calls), whose receipts record 19.1 s inside the gates (sum of the eleven `duration_s` fields);
- 118 selected-test executions on `3e2baea4b4`, read from the eleven gate logs: 22 `sprint7-visual` and 96 neighbour
  (summon-playtest-r3 5, -r4 4, -r6 11, -r7 2, sprint22- 11, sprint5- 4, salamander 42, native-admission 3,
  native-profile 9, loader-compat-probe 5), all passing;
- the harness suffix `…a373` everywhere (the wrong `…a395` was corrected in `0542b0592a`);
- the planning row states the BLOCKED stop, the open findings and 65 min 42 s of 75.

## Recovery validation ledger

The recovery has one aggregate allowance of 45 minutes of new validation-command wall time across C1 to C3, and at most
two new product candidates (one targeted). The old unused 9 min 18 s is not added. Earlier spend stays separate: 65 min
42 s in rounds 6 and 7, and all earlier rounds as recorded.

Wall time is measured around each command in the operator session; a batch of controller gates is measured around the
whole batch, which includes the loop and result-parsing overhead between calls, and its receipts' own `duration_s` are
listed beside it. Gates run on a head after its last ledger commit are carried by that round's developer report and by
the next ledger update (the next child's manifest, or the final handoff).

| Child | Command | Result | Time | Running total | Remaining of 45 min |
|---|---|---|---|---|---|
| C1 r1 | Focused gate `sprint7-visual` (local, uncommitted C1 source) | PASS, 22 tests | 25.3 s | 25.3 s | 44 min 34.7 s |
| C1 r1 | Controller gates on `10802bc0ea`: `sprint7-visual` 22 tests 5.0 s, `summon-playtest-r6` 11 tests 6.2 s, `sprint22-` 11 tests 5.4 s, `native-profile` 9 tests 4.7 s (per-call wrapper sum 21.3 s; receipts 2.9 + 2.6 + 3.0 + 2.4 = 10.9 s) | PASS | 22.9 s batch wall | 48.2 s | 44 min 11.8 s |
| C1 r2 | Focused gate `sprint7-visual` (local) | FAIL: one new assertion (an ID-problem reader reported a valid ID) | 19.0 s | 1 min 7.2 s | 43 min 52.8 s |
| C1 r2 | Focused gate `sprint7-visual` (local) | FAIL: this Newtonsoft build cannot parse an integer above Int64 (test fixture) | 16.6 s | 1 min 23.8 s | 43 min 36.2 s |
| C1 r2 | Focused gate `sprint7-visual` (local) | PASS, 22 tests | 3.4 s | 1 min 27.2 s | 43 min 32.8 s |
