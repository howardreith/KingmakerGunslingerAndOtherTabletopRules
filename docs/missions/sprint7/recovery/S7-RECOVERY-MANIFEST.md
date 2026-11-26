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

## Recovery status (2026-11-26)

**C1 handed off (scoped):** AWAITING_OWNER_REVIEW with handoff_current=true after review 5 of 5 (decision
ready_for_owner_review, no open findings; R4-F1 to R4-F7 verified). **Reviewed H `5a56d480a2827e0ddd3918e30d32a07c7554e112`;
archive A `2c67a049ee33e0b29621fd29a8b25608ed31e956`.** Handoff scope: implementation and domain behavior only, under the
revised acceptance boundary of C1 amendment 2; no accepted diagnostic limitations remained (R4-F5 and R4-F6 were fixed,
not rebutted); **native execution of the changed evaluator is deferred to C3's combined qualification** and is not claimed
by C1. The historical 224949Z native FAIL stays a FAIL. C1's branch is frozen from its archive A while C2 and C3 are under
review.

**C1 accepted by the owner (2026-11-25, C2 amendment 1)** for its amended implementation/domain scope only, at reviewed H
`5a56d480a2827e0ddd3918e30d32a07c7554e112` and archive A `2c67a049ee33e0b29621fd29a8b25608ed31e956`. C3 still owns native
execution of the revised evaluator. C1's branch and archive stay frozen.

**C2** (PR #222, enrollment `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08`) started from C1's archive A on
`codex/s7-recovery-c2-ghaele-late-units`, targeting C1's branch. Its short native lifecycle session passed on the existing
candidate-3 package with the rebuilt harness (no product candidate): the positive pending verdict null; the post-disposal
late control "late-owned-summon" with the late unit requested once by the witness reconciliation and observed destroyed;
the real-spawn witness's foreign Ghaele known, not unknown; nothing unsettled after any witness or at the end; isolation
restored; transaction completed and owner files verified. Window agreed with the other Kingmaker session (00:28Z–00:31Z). After C2's
first review (R1-F1 to R1-F8), the round-2 harness `bf5d74a12487a92ac4d00a792a17a1ecaa41407308728ee7cc3d4d9f405163cd`
(built from `44fd90a8d1`) was requalified by `20261126T005206Z-…-s7-ghaele-lifecycle` PASS on the same package: settlement now
the engine's Destroyed mark; the late unit requested by the witness reconciliation and destroyed; both foreign controls alive
and unrequested after reconciliation; nothing unsettled after any witness, at the end or at final release; isolation
restored; transaction completed, owner files verified, registry restored (window 00:52Z–00:54Z).

**C2 STOPPED: BLOCKED at its review cap (2 of 2).** Review 2 on H `525a27751b95fd51f3080b2a7865719532fff298` returned
changes_required; the controller set status BLOCKED ("review-round cap reached for final (2/2); findings are preserved for an
owner-authorized extension"). R1-F1 to R1-F6 and R1-F8 were verified fixed. Open findings:
- **R2-F1** (P1, blocks): if the early unsubscribe throws, the witness stays subscribed after its only reconciliation has
  been consumed; a correlated unit delivered afterwards is held only by the witness, so final release (which drains only the
  disposable inventory) gives it no destruction request and no per-unit result.
- **R2-F2** (P2, blocks): `LateResultObservation.Unknown` excludes only known controls, so a unit seen unattributed and later
  positively correlated is adopted, settled and still reported "ownership unknown", a false isolation failure.
- **R1-F7** (P2, blocks, fix rejected): the abandonment regression supplies its own final release array instead of running
  `GhaeleLifetimeProbe`'s finalizer (or one shared sequence).
- **R2-F3** (P3, non-blocking): the R5-F1 finding-map row and the ledger note pointed only to the first C2 run; corrected in
  this records update (both runs are now listed).

The C2 handoff is not current, so **C3 has not started** (no branch, mission or enrollment). Nothing was retried, re-enrolled
or extended. Resuming C2 needs an owner decision (for example `resume --max-rounds N` after an amendment).

Correction: the chat handoff at that stop listed R2-F2, R1-F7 and R2-F3 but omitted **R2-F1 (P1, blocking)**. This manifest
listed it; all three blocking findings (R2-F1, R2-F2, R1-F7) remained required. The R2-F3 identities were checked against the
two committed session receipts: `20261126T002852Z` git `c5fe26d146`, harness `0a038e40c6c1…`; `20261126T005206Z` git
`44fd90a8d1`, harness `bf5d74a12487…`; both on package `49f8b88c…6413`, both PASS.

**C2 amendment 1 (owner, 2026-11-25): late-unit cleanup.** Saved verbatim at
`owner-guidance/S7-RECOVERY-C2-AMENDMENT-1-2026-11-25.md` (message sha256
`6acde0306dbe04787f541d68c064c6ef6420ac532f149abbbc056b967fd18774`). Applied with `resume --amend-mission` (captured
2026-11-26T02:43:00Z as cumulative owner-amendments revision 1, sha256
`d62882e3355874e552829475113faf7afcc304a36c387d04ca29850cd08d4626`; the enrollment stayed BLOCKED at 2/2), then a separate
`resume --max-rounds 3` (ACTIVE, develop, cap 3). Both instruction parts read. No allowance or candidate ceiling changed.
Correction (one pass, external harness only):
- R2-F1: `WitnessCaller.ReconcileLatest` reconciles a witness's latest observations; observations are never cleared, and the
  shared final release reconciles every witness again, so arrivals after a failed unsubscribe are adopted and requested once
  (per-reference guard) or named unknown; the unsubscribe error stays.
- R2-F2: `LateResultObservation.Unknown` is computed by reference after removing positively correlated units and known
  controls; fixture-teardown registration still does not attribute a unit.
- R1-F7: `LateUnitFinalRelease.Release` is the one final sequence (reconcile every witness, request remaining units, one
  terminal snapshot for row and outcome, further releases) called by `GhaeleLifetimeProbe.Release` and by the domain fixture.
Native requalification of the new harness `4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61` (built from
`56e359f2cc`): `20261126T024816Z-…-s7-ghaele-lifecycle` PASS on the same candidate-3 package (window 02:48Z–02:50Z): the late
unit requested once (by the witness reconciliation, before any fallback) and destroyed; both foreign controls alive and
unrequested after settlement; all three witnesses' final reconciliations ran in the slot's final release; nothing unsettled
after any witness, at the end or at final release; zero cleanup failures; isolation restored; transaction completed, owner
files verified, registry restored. `005206Z` stays tied to harness `bf5d74a1…63cd` from `44fd90a8d1`. Neither run executes
C1's changed evaluator (old product package). The failed-unsubscribe and slot-abandonment cases are domain-only, by design.

**C2 STOPPED AGAIN: BLOCKED at its amended review cap (3 of 3).** Review 3 on H `89c7229e208a498e71caa91e7e4225e867c0d5d8`
(report prepared under owner amendments revision 1, sha256 `d62882e3…4626`) returned changes_required; the controller set
status BLOCKED ("review-round cap reached for final (3/3)"). R2-F1, R2-F2, R1-F7 and R2-F3 were verified fixed. (A first
submission of that report was refused by the controller, nothing changed, because the amendment parts had been read into a
file rather than displayed in the session; they were displayed and the same report resubmitted.) Open findings:
- **R3-F1** (P1, blocks): the held `unregistered summons` release of each owned cast calls `Game.Instance.EntityCreator.Tick()`
  before reading the cast's own summon, on cast disposal and on the probe's unwind, so abandonment can advance unrelated
  entity creation. The tick predates C2: it came from parent round-4 commit `9733a0c9b7` (in the frozen #219 head) and was
  not changed by C2.
- **R3-F2** (P2, blocks): each witness's final `ReconcileLatest` classifies unknowns from its own observation only, so a
  witness left subscribed after a failed unsubscribe can name as "ownership unknown" a summon another witness positively
  matched, or a foreign control another witness knows. This follows from C2's R2-F1 repair (final re-reconciliation).
- **R3-F3** (P3, non-blocking): the domain test's `rowList` helper treats a missing settlement field as an empty array.

The C2 handoff is not current, so **C3 has not started**. Nothing was retried, re-enrolled or extended; validation spend is
unchanged by the stop (12 min 6.5 s of 45, the round-3 controller gates included).

**C2 amendment 2 (owner, 2026-11-25): cast release and shared attribution.** Saved verbatim at
`owner-guidance/S7-RECOVERY-C2-AMENDMENT-2-2026-11-25.md` (message sha256
`51ab2989ed42cdc1d67b1518c1e16e4fae9ac7fe604b5022ffcc79fd1d541e2e`). Applied with `resume --amend-mission` (captured
2026-11-26T03:29:00Z as cumulative owner-amendments revision 2, sha256
`c94d3ad031dd6b5f781374cccd1d6c198b3819061749b2680f9c2134810ac2ee`; still BLOCKED at 3/3), then a separate
`resume --max-rounds 5` (ACTIVE, develop, cap 5). `status --instructions` and all three parts were displayed in the session.
Correction (external harness only):
- R3-F1: the held "unregistered summons" release is the shared `OwnedCastRelease.Hold` (production `CastOwned` and the domain
  fixture): it requests exactly the cast's retained summon (not when registered by the probe or already destroyed) through the
  one-request owner, and runs no creation tick, drain or frame; a summon still pending creation stays a requested reference
  whose settlement is read (unresolved, a named residual). The forward-progression creator tick in `CastOwned` is unchanged.
- R3-F2: `LateResultAttribution`, a probe-local view of every witness observation, classifies each witness's unattributed
  references in every reconciliation (early, held unwind, final): positively matched by any witness = correlated (requested
  once), explicitly known by any witness = control (left alone), otherwise unknown (named).
- R3-F3: the domain test requires an actual array at each asserted settlement field.
Native requalification of harness `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1` (built from
`d631d283aa`): `20261126T033419Z-…-s7-ghaele-lifecycle` PASS on the same candidate-3 package (window 03:34Z–03:36Z). Without
the cleanup creator tick, the real-spawn abandonment still destroyed exactly its own summon (one cast-release request,
`lateSummonsDestroyed` 1, the engine's Destroyed mark read); the late unit requested once by the witness reconciliation and
destroyed; foreign controls alive and unrequested after settlement; every cast's held release and all three final
reconciliations ran; nothing unsettled; zero cleanup failures; isolation restored; transaction completed, owner files
verified, registry restored. `024816Z` stays tied to `56e359f2cc` / harness `4180621b…db61` at its own scope. The queued
foreign creation and failed-unsubscribe controls are domain-only, by design.

Review 4 of 5 on H `fd5a009a575cc282bcb90e96113c3d3c98fa449b` returned changes_required: R3-F2 and R3-F3 verified; two
domain-evidence gaps open, **R3-F1** (P2, the simulated creation queue was not connected to the exercised cast boundary) and
**R4-F1** (P2, the registered foreign control's final request had no observable origin). Round-5 correction (domain test
only; harness source and session bytes unchanged, so session `20261126T033419Z` with harness `d77754ba…b4f1` is reused under
C2 amendment 2): the fixture's cast boundary is the retained-summon read the shared held release performs, and a detector
control that models the former tick there registers the queued foreign unit through the same `ProbeSlot.Abandon` path,
while the actual no-summon, conclusive and pending-creation cases leave it queued; the two-witness fixture registers a
teardown-only sentinel ahead of the foreign control and records request order, which must be summon, sentinel, foreign
(or summon alone when the control is not registered) in all eight variants.

## C1 history

**Current state (C1 amendment 2, 2026-11-25):** the owner narrowed C1's acceptance boundary for bounded closure and
raised the cap to five reviews on the same enrollment (`<lab>/owner-guidance/S7-RECOVERY-C1-AMENDMENT-2-2026-11-25.md`;
controller amendment revision 2, sha256 `cc70f3e9…7e74`; applied with `resume --amend-mission`, then `resume --max-rounds 5`).
C1 qualifies the evaluator's acceptance/rejection behavior for the current collector, live-object versus persisted-row
equivalence, the named identity/missing-evidence/lifecycle controls, the round-4 outcome corrections and the retained rows
at their supported scopes. Complete forensic validation of arbitrarily edited historical traces, a general chronology model
for old collectors beyond the named controls, and exact diagnostic wording for inputs that already cannot characterize are
withdrawn as C1 prerequisites. A refused characterization means only that the evidence does not establish the claim.
Round 5 addresses R4-F1 to R4-F4 and R4-F7 and also fixes the R4-F5 and R4-F6 distinctions (local fixes, so no rebuttal).

Corrections to earlier summaries: round 3 left R2-F3 open (it was verified only in round 4), and the recovery ledger
recorded six failed validation attempts at that point (two in round 2, four in round 4), not five.

**State after round 4 (historical): C1 was BLOCKED at its amended four-review cap** ("review-round cap reached for final
(4/4)"; enrollment `kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34`, PR #221). Round 3 on `d71bdf2e60`
verified R2-F1, R2-F2 and R2-F4 to R2-F6, kept R2-F3 open and raised R3-F1 to R3-F6. Round 4 on the reviewed head
**`2f975db075`** verified R2-F3 and R3-F1 to R3-F6 and raised seven more evaluator findings, which stay open:
- R4-F1: a first stop credited after the cue had already completed (Retire skips a completed cue), and a stop already
  present in the retirement frame before the first sample, still characterize;
- R4-F2: window samples that would require re-entering the expired window loop are accepted;
- R4-F3: for round-6 rows the phase timeline mark plus the declared window is used as a proven lower bound, although the
  mark is not the loop's start;
- R4-F4: equal UTC readings are treated as a reversed observation order;
- R4-F5: the one-attempt limit is not applied to the pre-disposal survivor counts;
- R4-F6: an unavailable settled completion is not kept distinct from a recorded null;
- R4-F7: the recorded stop cause is not checked before a stop is credited to the retirement.
C1 amendment 1 authorized two additional reviews only; this is a mandatory stop. C1 has no scoped handoff and no
archive A, so **C2 and C3 are not started** (held at their prerequisites). The changed evaluator was never executed in
game. Before round 4 the controller paused once on a failed PR-comment publication (`r03-dispositions`); it was confirmed
absent on the PR and resent through `resume --resend-undelivered`, as the controller directs. Continuing needs a new
owner decision.

**Round-2 state (historical, before C1 amendment 1):** C1 was BLOCKED at its two-review cap ("review-round cap reached for
final (2/2)", branch head `bb670b13f4`). Round 1 on `10802bc0ea` raised R1-F1 to R1-F8; round 2 on the reviewed head
`e4c9ed9c5e` verified all eight and raised six more evaluator findings:
- R2-F1: a selected ID is matched to its source inventory by ID only, not to the actual cue record (posting frame and time);
- R2-F2: withheld callbacks are compared with the periodic samples by count, not at the times the collector sampled them
  (a valid late callback is refused; a callback predating the cue is accepted);
- R2-F3: an earlier recorded completion of the other cue, and the survivor's stop time after disposal, are not reconciled;
- R2-F4: one unselected failed post (playing ID 0) in a source inventory makes the whole inventory unreadable;
- R2-F5: a negative stop count is read as an observed stop;
- R2-F6: new rows still name the removed `CallbackLossSettled` as their evaluator.
The owner instruction made this a mandatory stop: no new enrollment or extra repair PR, no raised limit. C1 had no
scoped handoff and no archive A, so C2 and C3 were not started (each held at its prerequisite).

**C1 amendment 1** (owner, 2026-11-25; `<lab>/owner-guidance/S7-RECOVERY-C1-AMENDMENT-1-2026-11-25.md`, controller
amendment revision 1, sha256 `dd15fc44…6ebb`) raised C1's cap to four reviews on the same enrollment (applied with
`resume --amend-mission`, then `resume --max-rounds 4`) and clarified the observation contract for R2-F1 to R2-F6. The
shared allowance is unchanged: 45 minutes and two candidates across C1 to C3. C1 stays implementation/domain only; its
changed evaluator is executed in game only by C3. The contract is written beside the evaluator in
`Sprint7ObserverPolicy.cs` (one posting identified by playing ID, event, posting frame and time; lifecycle fields only
advance; snapshots S0 inventory, S1 selection, S2 retirement frame, W samples, S3 settled and window end, S4 disposal).

## Finding map

| Parent finding | Repair PR | Scope | Evidence (to be filled by the child) | Depends on | Remaining integration gate |
|---|---|---|---|---|---|
| R7-F1 live and persisted callback verdicts disagree | C1 `codex/s7-recovery-c1-cue-evaluator` (PR #221) | Strict evaluator: absent, undefined, unreadable and out-of-range vs explicit null in both representations; one row-based round-7 entry point for the in-game collector and every reader | `sprint7-visual.impact-cue-playback-verdicts`: the persisted `…224949Z` row and its collector-shaped live counterpart (cue records through the collector's own `CueRow`, String-typed nulls, never serialized) both characterize; omitted fields are "missing" in both | #219 | Native execution of the changed evaluator: C3's combined qualification |
| R7-F6 settled verdict not bound to cue IDs and stop count | C1 (PR #221) | Identity binding and stop/callback reconciliation across every snapshot the collector emitted | Same case: substituted retirement-frame, window-end, disposal and settled-other IDs, a consistent survivor substitution the source inventory contradicts, the same ID twice, invalid and oversized IDs; positive retirement-frame or through-window stop counts, stop reasons, stop results, stop times and window-sample ends; placeholder or uncounted withheld callbacks; all refuse, in both forms. Retained round-6 rows are judged through `CallbackLossRound6` by their own collector generation (`Round6First` for `f3bfab1303`, `Round6Second` for `740d94a995`) | #219 | As R7-F1 |
| R6-F5 round-7 expenditure and harness suffix | C1 (records disposition only) | Confirm the frozen head's record and planning row from existing receipts | This manifest, below | #219 | Parent records reconciliation at integration |
| R5-F1 unaccounted late Ghaele | C2 `codex/s7-recovery-c2-ghaele-late-units` (PR #222) | Caller reconciles every correlated late unit with the destruction owner; outstanding units fail ProbeSlot isolation | Domain `sprint7-visual.ghaele-late-units-reach-slot` (ProbeSlot / Poll / Release / unwind); native `20261126T002852Z-…-s7-ghaele-lifecycle` PASS (round 1, harness `0a038e40`; round 2: `20261126T005206Z-…-s7-ghaele-lifecycle` PASS, harness `bf5d74a1…63cd` from `44fd90a8d1`; round 3 under C2 amendment 1: `20261126T024816Z-…-s7-ghaele-lifecycle` PASS, harness `4180621b…db61` from `56e359f2cc`; C2 BLOCKED at 3/3 with R3-F1 and R3-F2 blocking, R3-F3 open; round 4 under C2 amendment 2: `20261126T033419Z-…-s7-ghaele-lifecycle` PASS, harness `d77754ba…b4f1` from `d631d283aa`) on candidate-3 package `49f8b88c…6413` / DLL `5969f596…e2c3` with harness `0a038e40…71ca9` built from `c5fe26d146` (`docs/evidence/sprint7/recovery-c2/`) | C1 | C2 harness identity carried into C3 and the parent |
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
| C1 r2 | Controller gates on `e4c9ed9c5e`: `sprint7-visual` 22 tests 4.1 s, `summon-playtest-r6` 11 tests 3.5 s, `sprint22-` 11 tests 4.0 s, `native-profile` 9 tests 3.5 s (receipts 2.3 + 1.8 + 2.2 + 1.8 = 8.1 s) | PASS | 16.2 s batch wall | 1 min 43.4 s | 43 min 16.6 s |
| C1 r3 | Focused gate `sprint7-visual` (local, amendment-1 source) | PASS, 22 tests | 19.1 s | 2 min 2.5 s | 42 min 57.5 s |
| C1 r3 | Controller gates on `d71bdf2e60`: `sprint7-visual` 22 tests 4.9 s, `summon-playtest-r6` 11 tests 4.5 s, `sprint22-` 11 tests 4.9 s, `native-profile` 9 tests 4.2 s (receipts 2.8 + 2.1 + 2.9 + 2.2 = 10.0 s) | PASS | 19.7 s batch wall | 2 min 22.2 s | 42 min 37.8 s |
| C1 r4 | Focused gate `sprint7-visual` (local) | FAIL: compile error (a local name clash) | 11.4 s | 2 min 33.6 s | 42 min 26.4 s |
| C1 r4 | Focused gate `sprint7-visual` (local) | FAIL: an expectation superseded by the stricter stop/count agreement | 17.8 s | 2 min 51.4 s | 42 min 8.6 s |
| C1 r4 | Focused gate `sprint7-visual` (local) | FAIL: a path label in one expectation | 3.4 s | 2 min 54.8 s | 42 min 5.2 s |
| C1 r4 | Focused gate `sprint7-visual` (local) | FAIL: check order (a pre-disposal stop reported as inconsistent fields) | 17.6 s | 3 min 12.4 s | 41 min 47.6 s |
| C1 r4 | Focused gate `sprint7-visual` (local) | PASS, 22 tests | 17.4 s | 3 min 29.8 s | 41 min 30.2 s |
| C1 r4 | Controller gates on `2f975db075`: `sprint7-visual` 22 tests 4.2 s, `summon-playtest-r6` 11 tests 4.1 s, `sprint22-` 11 tests 4.5 s, `native-profile` 9 tests 3.8 s (receipts 2.3 + 2.1 + 2.4 + 1.9 = 8.7 s) | PASS | 17.5 s batch wall | 3 min 47.3 s | 41 min 12.7 s |
| C1 r5 | Focused gate `sprint7-visual` (local, amendment-2 source) | FAIL: an existing diagnostic-only assertion (input kept; now asserts refusal) | 18.4 s | 4 min 5.7 s | 40 min 54.3 s |
| C1 r5 | Focused gate `sprint7-visual` (local) | PASS, 22 tests | 3.4 s | 4 min 9.1 s | 40 min 50.9 s |
| C1 r5 | Controller gates on `5a56d480a2`: `sprint7-visual` 22 tests 4.1 s, `summon-playtest-r6` 11 tests 3.4 s, `sprint22-` 11 tests 3.9 s, `native-profile` 9 tests 3.3 s (receipts 2.1 + 1.7 + 2.1 + 1.7 = 7.6 s) | PASS | 15.5 s batch wall | 4 min 24.6 s | 40 min 35.4 s |
| C2 | Harness build (`c5fe26d146` source) | PASS, `0a038e40c6c1da95e59bd943e5972df8940e3b74158eacebe88dcab7ed571ca9` | 2.4 s | 4 min 27.0 s | 40 min 33.0 s |
| C2 | Focused gate `sprint7-visual` (local, C2 source) | PASS, 23 tests | 18.3 s | 4 min 45.3 s | 40 min 14.7 s |
| C2 | Session `20261126T002852Z-…-s7-ghaele-lifecycle` (candidate-3 package, harness `0a038e40…`) | PASS | 2 min 20 s | 7 min 5.3 s | 37 min 54.7 s |
| C2 r1 | Controller gates on `192166b9ba`: `sprint7-visual` 23 tests 3.8 s, `loader-compat-probe` 5 tests 3.3 s (receipts 2.1 + 1.7 = 3.8 s) | PASS | 7.7 s batch wall | 7 min 13.0 s | 37 min 47.0 s |
| C2 r2 | Harness build (C2 round-2 source) | PASS | 2.2 s | 7 min 15.2 s | 37 min 44.8 s |
| C2 r2 | Focused gate `sprint7-visual` (local, C2 round-2 source) | PASS, 23 tests | 3.9 s | 7 min 19.1 s | 37 min 40.9 s |
| C2 r2 | Session `20261126T005206Z-…-s7-ghaele-lifecycle` (candidate-3 package, harness `bf5d74a1…63cd` from `44fd90a8d1`) | PASS | 2 min 14 s | 9 min 33.1 s | 35 min 26.9 s |
| C2 r2 | Controller gates on `525a27751b`: `sprint7-visual` 23 tests 4.0 s, `loader-compat-probe` 5 tests 3.4 s (receipts 2.1 + 1.6 = 3.7 s) | PASS | 7.4 s batch wall | 9 min 40.5 s | 35 min 19.5 s |
| C2 r3 | Focused gate `sprint7-visual` (local, amendment-1 source) | PASS, 23 tests | 4.0 s | 9 min 44.5 s | 35 min 15.5 s |
| C2 r3 | Harness build (amendment-1 source) | PASS, `4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61` | 1.3 s | 9 min 45.8 s | 35 min 14.2 s |
| C2 r3 | Session `20261126T024816Z-…-s7-ghaele-lifecycle` (candidate-3 package, harness `4180621b…db61` from `56e359f2cc`) | PASS | 2 min 12 s | 11 min 57.8 s | 33 min 2.2 s |
| C2 r3 | Controller gates on `89c7229e20`: `sprint7-visual` 23 tests 4.7 s, `loader-compat-probe` 5 tests 4.0 s (receipts 2.8 + 2.0 = 4.8 s) | PASS | 8.7 s batch wall | 12 min 6.5 s | 32 min 53.5 s |
| C2 r4 | Focused gate `sprint7-visual` (local, amendment-2 source) | PASS, 23 tests | 4.0 s | 12 min 10.5 s | 32 min 49.5 s |
| C2 r4 | Harness build (amendment-2 source) | PASS, `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1` | 1.2 s | 12 min 11.7 s | 32 min 48.3 s |
| C2 r4 | Session `20261126T033419Z-…-s7-ghaele-lifecycle` (candidate-3 package, harness `d77754ba…b4f1` from `d631d283aa`) | PASS | 2 min 7 s | 14 min 18.7 s | 30 min 41.3 s |
| C2 r4 | Controller gates on `fd5a009a57`: `sprint7-visual` 23 tests 4.2 s, `loader-compat-probe` 5 tests 3.4 s (receipts 2.4 + 1.7 = 4.1 s) | PASS | 7.6 s batch wall | 14 min 26.3 s | 30 min 33.7 s |
| C2 r5 | Focused gate `sprint7-visual` (local, round-5 domain-test source) | PASS, 23 tests | 3.8 s | 14 min 30.1 s | 30 min 29.9 s |

Notes:
- Through C1 (to 2026-11-26 00:24Z) no product package, harness build or game session had run in the recovery; C2 then
  built its harness four times and ran four short lifecycle sessions on the existing candidate-3 package (rows above).
- Failed validation attempts so far: seven (two in C1 round 2, four in C1 round 4, one in C1 round 5).
- Earlier spend stays separate: 65 min 42 s in parent rounds 6 and 7. New product candidates used: 0 of 2.
- Reserve before the C2 round-3 session: C3 still needs one combined Build-Local candidate and its affected native
  qualification (the effects session, estimated at about 18 minutes, plus the build and gates). After the planned C2 short
  session (about 2 min 15 s) and controller gates, about 32 minutes remain for C3.
- Reserve before the C2 round-4 session: C3 is estimated at about 25 minutes (combined build, focused gates, effects
  qualification); after this session (about 2 min 15 s) and the round-4 controller gates about 30 min 25 s remain, leaving
  about 5 minutes of margin for one short C2 rerun if round 5 needs one.
