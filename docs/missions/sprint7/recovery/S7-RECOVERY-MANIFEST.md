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

**C2 handed off (scoped):** review 5 of 5 returned ready_for_owner_review (no open findings; R3-F1 and R4-F1 verified). A first
`finalize` was refused, nothing published, because another session's runtime transaction lock was present
(`compatibility-state/compatibility.lock`, transaction `runtime-20261126T035746Z`, a stalled run of a different Gunslinger
session, which that session recovered); after it cleared, `resume --reason` (recording that cause) completed the handoff:
AWAITING_OWNER_REVIEW with handoff_current=true. **Reviewed H `da811b123c5720ad2d174db17142d8f4ef8fa5d6`; archive A
`eab90a5ac82f661ecb903c5456d951a57a085eb9`.** Handoff scope: the external-harness correction of R5-F1 and its qualification
(harness `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1`, session `20261126T033419Z` on candidate 3). C2's
branch is frozen from its archive A while C3 is under review.

**C3** (`codex/s7-recovery-c3-effects-ownership`) starts from C2's archive A, targeting C2's branch, under its original
mission, a two-review cap, the shared allowance (30 min 21.4 s remaining) and the candidate ceiling (0 of 2 used).

**C3 combined native qualification.** Enrollment `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d` (PR #223).
The first combined candidate (`988d40bb1e`, package `42424017…9484`) failed session `20261126T051633Z` on three checks, two
concrete defects of the qualification itself: the Dust abandonment driver read a recolour before its release wait, and the new
pre-delivery step displaced the unowned-execution control past its 3-round Sickened (that run's callback-loss verdict, overlap,
set-up controls and pre-delivery release passed). The authorized contingency candidate (`6abd692fa7`, package
`de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46`, DLL `ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a`)
passed `20261126T054228Z-…-s7-c3-fx`, 41/41 checks: the phase's one isolation restored with all seven cells admitted; all
four drivers proven with no unexpected child failure; the declared overlap reached both playing IDs (544, 545) on its first
same-frame delivery (constructed setup); C1's revised evaluator judged the callback-loss row in game: verdict null
(characterized). **C1's deferred native check is fulfilled here** (C1's branch is not amended): the persisted row of that same
run, committed as `docs/evidence/sprint7/recovery-c3/20261126T054228Z-callback-loss-row.json` (source sha256 recorded), and its
collector-shaped live counterpart receive the same null verdict (`sprint7-visual.combined-run-verdicts-agree`). The Ghaele
correction stays qualified at C2's harness identity; this session staged the same harness (`d77754ba…b4f1`) and ran no Ghaele
step. The historical native FAILs (including 224949Z) are unchanged. Window 05:34Z–05:48Z, agreed with the other sessions.

**C3 STOPPED (mandatory stop: the required remaining validation cannot fit).** Review 1 of 2 on H
`7fff8b34f6d925d91e80beddb6501cfad249e33d` returned changes_required with thirteen findings; the enrollment stays ACTIVE in
develop at 1/2 and is not resubmitted. Blocking product defects in the effects contract (all in RuntimeTesting, so any
correction changes the DLL): R1-F1 phase/fixture teardown can repeat a failed cue-source retirement (the one-request record
is local to the observer); R1-F2 Dust unsubscribes its condition ledger before its retained executions settle, so an effect
delivered later is not reconciled; R1-F3 an early (pre-delivery) command-release failure reaches only a check, not isolation;
R1-F4 an unreadable process read or the step-4 cancellation can lose a process before ownership; R1-F5 Harmony count reads
gate rollback, normal unhook and repair, and can replace the setup error (and HookCount reads null patch info as zero);
R1-F6 the driver's proof omits unreported owners and matches expected failures by substring; R1-F7 a non-throwing failed
repair on disposal is not published; R1-F8 the final phase verdict ignores a refused cell when a residual settles later;
R1-F9 save-log and recorder subscriptions precede their cleanup scope; R1-F10 a disposal-only exception still reports cell
completion. Evidence gap R1-F11: the paired-fault and retirement tests supply outcomes beyond the production failure boundary.
R1-F12 was only disclosed (below) and R1-F13 only partly answered (below); neither was closed by that records update. Why this is a stop: the recovery has spent 42 min 28.9 s of
its 45 minutes and both authorized product candidates; correcting R1-F1 to R1-F11 needs a further product candidate and an
affected native effects session (the last build took 6 min 58 s and the session 6 min 18 s) beyond both the allowance and the
ceiling, and the reviewer states that a further package requires new owner authority. Nothing was retried, re-enrolled or
extended. The two C3 candidates, both session receipts and their evidence extracts are preserved.

**C3 amendment 1 (owner, 2026-11-26): complete the ownership repair and handoff.** Saved verbatim at
`owner-guidance/S7-RECOVERY-C3-AMENDMENT-1-2026-11-26.md` (message sha256
`5294e5c6e920fe6c1d606148a8aa14bcbf53939f25ff6e7a50ebb7f7b89fa59f`). Applied to the existing enrollment with
`resume --amend-mission` (captured 2026-11-26T12:18:28Z as cumulative owner-amendments revision 1, sha256
`c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233`), then a separate `resume --max-rounds 4` (four C3 reviews in
total; round 1 spent). All three instruction parts were displayed in the session. Amended ceilings: validation 75 minutes in
total (42 min 28.9 s spent at the amendment, 32 min 31.1 s left); recovery product candidates 4 in total (2 used; recovery
candidate 3 targeted; candidate 4 only for a concrete defect found qualifying candidate 3 or in its scoped review). Numbering
is separate from the parent's candidate 3 that C2 reused.

Owner acceptances recorded here (C3 only; the C1 and C2 branches are not amended):
- **C1 (#221)**, reviewed H `5a56d480a2827e0ddd3918e30d32a07c7554e112`, archive A `2c67a049ee33e0b29621fd29a8b25608ed31e956`:
  implementation/domain acceptance stands, and the owner accepts fulfilment of its deferred same-run native evaluator
  criterion by session `20261126T054228Z` on product source `6abd692fa7ee84604ef8122569f84a57e3b3c1a2`, package
  `de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46`, DLL
  `ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a` (in-game row and committed persisted row agree; persisted
  and live-shaped interpretations agree; constructed event scheduling explicit). It belongs to those bytes and that run.
- **C2 (#222)**, reviewed H `da811b123c5720ad2d174db17142d8f4ef8fa5d6`, archive A `eab90a5ac82f661ecb903c5456d951a57a085eb9`:
  the owner accepts its external-harness implementation and stated native lifecycle scope, including session
  `20261126T033419Z` with harness `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1` on the unchanged parent
  candidate-3 package. C2 is not rerun because C3 changes.
- **Parent #219** stays frozen at `0542b0592a668aad0c62c66d5f2dc862460818fe` (a records head, not an archive A; no parent archive
  exists), its enrollment BLOCKED at 7/7.

**C3 correction (one coherent pass, before the next package; not yet run in the game).** For R1-F1 to R1-F13:
- R1-F1: one probe-wide `Sprint7OwnedRequests` record per phase, shared by the observer's direct retirements, its unwinding
  release and the phase teardown (`ReleaseOutstanding`, each unit on its own); a request is attempted once whether it threw or
  returned (`Returned` is not settlement); failures and settlement residuals are published once by identity; the fixture's
  outer teardown skips units whose retirement the phase attempted (`RetirementAttempted`, narrow to that path) and lists them.
- R1-F2: `ReleaseOwnedEffects` keeps the condition ledger subscribed while any retained execution is live and reconciles at
  settlement (late owned conditions released, new remaining ones named, then unsubscribe and the foreign-control check); with
  nothing live it settles at cleanup without a second release.
- R1-F3: command-release failures are published the moment they happen and kept cumulatively by `Sprint7OwnedCommands`.
- R1-F4: an unreadable process is a named unknown residual of its command; the step-4 cancellation and the early-exit cast go
  through the command owner (process captured first).
- R1-F5: hook releases follow the attempt (callback `patchAttempted`, trace patched list, repairs by listing), never a count;
  `RollbackSetup` runs the unhook whatever the counts read; unreadable counts and missing patch info are unknown; the original
  set-up error is kept with the rollback errors.
- R1-F6: `Sprint7Abandonment` requires every child owner to report and exempts only exact expected failures (operation,
  identity and outcome, e.g. the injected removal of a named Sickened instance); no substring matching; hook dispose failures
  are no longer aggregated.
- R1-F7: the driver publishes its repair result on every exit; `RepairExact` names a removal that returns with the item present
  or unreadable; injected identities are captured at injection.
- R1-F8: `RequiredCellsComplete`: the phase's required cells (declared per selection) must each be admitted and complete; a
  separate check reports physical restoration.
- R1-F9: `Sprint7SetupScope` owns the Dust cell's save-log subscription and the phase's hit-point, recorder and saves set-up
  before each runs; the condition ledger exists before its subscription is attempted.
- R1-F10: `IsolatedCell` records enumeration, Current, disposal and completion-publication failures consistently; a
  disposal-only failure is not a completed cell.
- R1-F11: `sprint7-visual.effects-ownership-recovery` drives these production functions with controlled failing boundaries
  (paired injected removal plus an actual failing interruption, residual-count and phrase counterexamples, an unreported
  owner, disposal in the recovery wait with a non-throwing failed repair, late delivery after ordinary disposal, early release
  failure, unreadable process, refused required cell then restoration, disposal/Current/completion failures, both retirement
  failure positions through the phase teardown, partial set-up, rollback with unreadable counts).
- R1-F12: an explicit mechanics-only phase, `summon-s7-effects-mechanics` (session `s7-c3-mechanics`): no frame captures, no
  native-look comparison (a plain final owned cast instead), no listening samples; required cells declared before the run.
- R1-F13: the integration plan below is completed.
A new native control, `s7-dust-pre-delivery-disposed`, disposes an ordinary Dust cell after its own execution started and
before delivery and expects the next cell admitted after settlement. **The newly changed compiled code has not run in the
game**; C3 still needs its native qualification on recovery candidate 3.

**C3 review 2 (of 4)** on H `89d4efa34732aa85c0a3975c5770865e245e6766` returned changes_required: R1-F1 to R1-F4, R1-F6 to R1-F8 and
R1-F12 verified; R1-F5, R1-F9, R1-F10, R1-F11, R1-F13 rejected; new R2-F1 (native qualification outstanding) and R2-F2 to R2-F9.
Offline corrections before recovery candidate 3 (not yet run in game):
- R2-F2: residuals carry a bounded teardown action; a driver hands its child's pending residuals to the phase on exit; the
  phase terminates any still pending at teardown (owned conditions released, ledger unsubscribed, uncertainty named).
- R2-F3: the driver disposes in the same step the inner cell establishes readiness and reads the exact execution at the
  disposal itself (`AtDisposal`); the pre-delivery control rejects a later frame, a delivery at disposal or another execution,
  and re-reads that execution's conditions after settlement.
- R2-F4: both foreign controls and the outside target stay in place through the owned release and settlement; they are read by
  identity (`ControlsKept`, missing or unreadable is a failure) and only then released; the settlement read must be true.
- R2-F5: the Ankylosaurus normal-control retirement, the Dust source-loss retirement and the outside target go through the
  phase's one retirement record (`S7Retire`), with settlement published.
- R2-F6: `Sprint7OwnedCommands` binds each command's exact process once observed; delivery attribution reads retained processes
  first, each guarded, and each command field on its own.
- R2-F7: phase combat teardown per actor (`ReleaseEach`); only the phase's own summons have their commands interrupted.
- R2-F8: condition-release failures (between casts and in the early-exit cast) are published where they happen.
- R2-F9: the casts expectation is profile-specific (`DustCastsExpectation`); the required cells come from `RequiredEffectsCells`.
- R1-F5: `RollbackSetup` and `S7DisposeTrace` keep each count failure; the hook readback keeps its error text.
- R1-F9: `Sprint26Saves` can be constructed without subscribing; the phase's and Dust cell's saves are listed, then subscribed
  through the set-up scope; the attack log is under a scope; phase set-up failures are named in the phase result.
- R1-F10: a cell is a successful completion only after its completion publication succeeds.
- R1-F11: the focused case adds the fixture's own teardown decision (`FixtureTeardownCandidates`), a real handler identity with a
  partial subscription, and a failing unpatch beside an independent release through `RunReleases`.
- R1-F13: the integration pre-checks and identities above are corrected.

**C3 native qualification of the corrected bytes (recovery candidate 3).** Product source `66b0ff3f14ef` (= the reviewed-2
corrections `b989a045d1` plus records), package `473f2f5a78229f3e2e15c3d97b863f2201f44ae372d2485de1fdb05010618366`, DLL
`8e574477cce11621ca2ec88fdeed358cb9f8f76ff8ced423e154e3c86d277d07`, harness C2's `d77754ba…b4f1` (unchanged), fixture the admitted
disposable outdoor native Default fixture of `disposable-expanded-summoning-sprint32-native-review`, selection
`summon-s7-effects-mechanics` (session `s7-c3-mechanics`). Session `20261126T134425Z-…-s7-c3-mechanics`: PASS, 43/43 checks
(window 13:36Z–13:50Z, agreed with the other sessions).
- Required parent cells (declared before the run): all seven admitted and completed (`s7-effects-required-cells-completed`);
  separately, physical restoration true (`s7-effects-owned-state-restored`): the phase isolation restored with no failure,
  pending or unreadable residual; one recovery (the Dust outside target's retirement settled).
- Drivers: callback-abandoned, callback-setup-failed, trace-setup-failed, dust-pre-delivery-disposed and dust-abandoned all
  proven, with no unexpected and no missing expected failure, repair after the guard, nothing transferred to the phase.
- Ordinary disposal before delivery: disposed in the readiness frame 11195 with the exact execution
  `KMG_Summoning_Special_DustMephit_Breath#-376666880` started, not ended and not delivered; after the disposal that execution
  delivered one Sickened late (acquired from that execution: 1), released at settlement (present from it: 0); the ledger held
  until settlement, then unsubscribed; the caster-behind control kept through settlement.
- Dust cell: both foreign controls read present after the owned release (no control failure), pre-delivery release retained
  and settled; no frames, no native-look comparison, no samples (mechanics-only selection; casts record "not captured").
- Callback loss (constructed setup): declared same-frame delivery 0 posted real cues with both IDs playing (534, 535); C1's
  evaluator judged the row live: verdict null (characterized). The committed persisted row
  `docs/evidence/sprint7/recovery-c3/20261126T134425Z-callback-loss-row.json` (source sha256 recorded) and its collector-shaped
  counterpart receive the same verdict (`sprint7-visual.combined-run-verdicts-agree`).
- Retirements: 19 requests through the phase's one record, none failed, all settled.

**C3 STOPPED AGAIN: BLOCKED by the controller's no-progress rule (mandatory stop).** Review 3 of 4 on H
`f0ca78cbab9b57583379a575208a15304aae7eb1` (prepared under owner amendments revision 1 sha256 `c91a5261…6233`) returned
changes_required, and the controller set status BLOCKED: "no progress: R1-F11, R1-F13, R1-F9 open for 3 consecutive reviews"
(rounds 3/4; phase develop). Not resumed: C3 amendment 1 authorizes no further extension, and a stop is preserved and reported.
Verified in review 3: R2-F1 (recovery candidate 3's native qualification: 43/43, required cells completed, separate physical
restoration, real late delivery after pre-delivery disposal released at settlement, constructed cue posts, same-run verdict
agreement, transaction readbacks), R2-F2 to R2-F6, R2-F9, R1-F5, R1-F10. Open:
- **R3-F1** (P1): cleanup transferred from a driver's child to the phase still publishes its later failures and new residuals
  (for example an unsubscribe failure or the outside target's retirement) to the departed child isolation.
- **R3-F2** (P1): a delivery missed while the owning command's process was unreadable is not reconciled once that process
  becomes readable (the ledger attributes conditions only by contexts saved at delivery).
- **R2-F7** (P1): phase teardown still interrupts every command on an owned summon (including a separate owner's queued
  command) and an interruption failure skips that actor's queue update and combat exit.
- **R3-F3** (P1): the Dust controls' diagnostics: a missing same-caster control is not named in isolation, a repeated presence
  read can throw before the independent control releases and the outside target's retirement, and the outside target is not
  in the phase's owned list.
- **R3-F4** (P1): the immediate settlement path ignores an unreadable result from its last end-state read and leaves no
  reconciliation residual or terminator.
- **R1-F9** (P2): the baseline ground lookup and hostile placement run outside the phase's guarded set-up reporting.
- **R2-F8** (P2): an ordinary condition-removal exception is published without the condition's identity.
- **R1-F11** (P2): the focused retirement control builds the fixture's attempted list itself instead of using the production
  write (`RetirementAttempted` in the retire action); the context-read test places the throwing getter after the match.
- **R1-F13** (P2): the active integration plan and finding map still describe the pre-amendment stop (this records update
  dates that stop as history; see below).
- **R3-F5** (P2): ledger arithmetic, corrected above (records only, not reviewed).
Every correction among R3-F1 to R3-F4, R2-F7, R1-F9, R2-F8 and R1-F11 changes compiled or test code; after it the corrected bytes
would need recovery candidate 4 and another mechanics session. At the stop: 59 min 37.0 s of 75 spent (15 min 23.0 s left;
the last build took 8 min 0.9 s and the session 6 min 3 s), 3 of 4 candidates used, 3 of 4 reviews used. The enrollment's state
is preserved; nothing was retried, resumed, re-enrolled or extended.

**C3 amendment 2 (owner, 2026-11-26): continue past the no-progress stop.** Saved verbatim at
`owner-guidance/S7-RECOVERY-C3-AMENDMENT-2-2026-11-26.md` (message sha256 `bbde9e73…dc2c2`). Applied to the same enrollment with
`resume --amend-mission` and a reason recording the owner's authorization to continue after the no-progress stop
(review ceiling unchanged at four; `--max-rounds` not changed). Controller amendments revision 2, sha256
`2b84ac74880bdc892048034d87e21609af86c067f82eb7bda9738d2f8dc75481` (captured 2026-11-26T16:16:07Z); all four instruction
parts displayed and read in the enrolled developer session. Allowances: validation 75 → 90 min total (59 min 37.0 s spent at
the amendment, 30 min 23.0 s left); candidates stay 4 total (3 used; candidate 4 is the last); formal reviews stay 4 total
(3 used; review 4 is the last, for the full corrected-byte result). A bounded independent read-only pre-build audit (initial
pass plus one verification follow-up) is separately authorized; it is not a controller round or an approval.

Retained at its actual scope: the PASS of `20261126T134425Z` on source `66b0ff3f14`, package `473f2f5a…8366`, DLL
`8e574477…7d07`, harness `d77754ba…b4f1` (43 checks, all seven required cells admitted and completed, separate physical
restoration, real late Sickened delivery and removal after pre-delivery disposal, same-run live/persisted agreement). Its five
driver rows have empty `transferredResiduals` arrays: it is **not** native qualification of failures or new residuals produced
after a driver hands pending cleanup to the phase. Those boundaries are covered by focused tests through the production
orchestration (below).

Corrections under C3 amendment 2 (source and focused tests; **not yet run in game**; they need recovery candidate 4):
- **R3-F1** (A): `Sprint7CellIsolation.TransferPendingTo` now also forwards every later `Fail` and `Residual` of the departed
  child to the enclosing owner (prefixed), so cleanup that outlives a driver publishes to the phase it keeps consuming; the
  child keeps its own history and the intentional-fault classification is not replayed (only post-transfer publications are
  forwarded). Focused: a driver disposed mid-cell hands its reconciliation to the phase; the reconciliation's later unsubscribe
  failure and a new outside-target residual reach the phase beside the original refusal, and the forwarded residual is
  consumed by the phase's settlement.
- **R3-F4** (A): `ReleaseOwnedEffects` registers one reconciliation obligation, with its bounded teardown action, before any
  settlement attempt on both paths; the immediate path settles it through `SettleResidual` and it leaves only when settlement
  is established. Focused: census reads the execution ended, the settlement read throws; the obligation stays (ledger still
  subscribed), and terminal teardown runs it once and names "not settled at teardown".
- **R3-F2** (B): ownership attribution is the production `Sprint7ObserverPolicy.AttributedTo` (observed delivery **or** the
  context of a retained owned process, including one recovered after missed notifications), and settlement always reconciles
  once more. Observed deliveries (`Delivered()`, the pre-delivery probe) stay a separate collection. Focused: a condition
  delivered while the process was unreadable is released once the process is recovered and ended; the foreign control stays;
  no observed delivery is fabricated.
- **R2-F7** (C): the effects runner records each command it issues (`S7Run`) and the phase teardown releases only those, by
  reference (`ReleaseIssuedCommands`), with interruption, queue maintenance and combat exit each attempted on their own; the
  same-caster control command is deliberately not recorded; the callback observer's teardown interrupts nothing (its
  observation-time stilling of its own two freshly summoned sources is unchanged from the qualified run). After the pre-build
  audit (A1-F5, A1-F6): the record is the shared `Sprint7IssuedCommands`, the passive Dust/Ice route registers through its
  issue callback, and an actor's queue maintenance (which starts the queue head in 2.1.7b) runs only while its queue holds no
  command outside the record; otherwise it is withheld and reported. Focused: commands registered both ways; one interruption
  fails and the other still runs; another owner's command queued on the same actor is never started; queue maintenance runs on
  the other actor and combat exit on both.
- **R3-F3** (D): Dust declares its required controls per path (the caster-behind control once placed; the same-caster control
  once the separate execution was due), reads each once (present, absent or unreadable), and derives diagnostics, verdict and
  releases from that one result (`ControlResults`); a required control with no handle is a named failure. The outside target is
  added to the phase's owned list at acquisition, so phase teardown retires it through the one retirement record if no cell did.
- **R1-F9** (D): the baseline ground lookup and hostile placement run inside `S7PhaseAcquire`, whose release restores the
  hostile's position. Focused: a placement failure after both subscriptions stays in the phase result while every release
  runs newest first.
- **R2-F8** (E): `AcquisitionLedger.Release` takes an identity function and binds each removal failure to its exact instance
  at the boundary ("removal of <id>: <exception>"); the injected fault is recognised by that identity, not by message text.
  Focused: an engine exception with no ID is named by instance and survives a later read failure; the next cell is refused.
- **R1-F11** (F): the production retirement is `RetireRecorded` (the fixture's `RetirementAttempted` collection written
  before disposal) and the observer's unwinding release is the shared `ReleaseObserverSources`; the focused retirement case
  passes the collection that operation wrote through `FixtureTeardownCandidates` (both failure positions covered); the
  context-read case arranges the throwing getter before the successful match and asserts it ran.
- **R1-F13** (records): the active integration table, dependency paragraph, finding reconciliation and C3 finding-map rows
  below are brought to this state; the earlier stops stay as dated history.
- **R3-F5** (records): arithmetic kept as corrected (59 min 37.0 s at the amendment; receipts 8.9 s; batch 17.3 s;
  seventeen failed attempts through the stop); later attempts add to it in the ledger.

**Bounded pre-build audit (C3 amendment 2 §5; not a controller round, not an approval).** Reviewer configuration as the
controller's: Codex CLI 0.170.0, `gpt-6.2-astra`, effort max, read-only sandbox, approvals never, the project's disabled features.
- Initial pass, 2026-11-26 16:27:47Z–16:44:04Z, on `a84d0b0d073055da8c192ba6e3edf0880644f550`: corrections_required, nine blocking
  findings. Verified: R3-F1's ordinary forwarding, R3-F4's registration, R1-F11, R3-F5.
  - A1-F1 (R3-F2): a throwing retained-context read turned an unresolved condition into a confirmed non-match. Corrected:
    `MatchContext`/`AttributedTo` are tri-state; `Sprint7ConditionOwner` never releases an unresolved condition and names it in
    the remaining identities ("attribution unresolved: …"), which refuses admission and blocks settlement until it resolves.
  - A1-F2 (R3-F1, R3-F4): an empty condition list could discharge the reconciliation while its unsubscribe had failed, and
    `settledAtCleanup` could be written true at teardown. Corrected: the obligation is established only when the late release,
    the remaining read and the subscription release all succeeded; the terminator makes one more attempt of each unfinished
    operation and names what remains; `settledAtCleanup` is true only when the immediate attempt settled.
  - A1-F3 (R3-F1, R3-F3, R3-F4): phase teardown retired the outside target before a deferred reconciliation read its control,
    and a residual published by a terminator could stay pending. Corrected: `PhaseTeardownSettlement` settles/terminates
    pending residuals first, then retires outstanding units, publishes, and terminates again; `TerminatePending` consumes
    residuals published during terminal cleanup (bounded passes).
  - A1-F4 (R3-F3): the control test fed literal results. Corrected: production `SettleControls` (Dust's settlement) is driven
    through deferred settlement with a throwing read (run once), a released second control, a control not due, a missing
    required handle, and one outside-target retirement through the real `Sprint7OwnedRequests`.
  - A1-F5, A1-F6 (R2-F7): passive casts bypassed the record; queue maintenance could start a foreign queued command. Corrected
    as described under R2-F7 above (also applied to the cell-level owned-command interruption and the attack loops).
  - A1-F7 (R1-F9): the test published the expected error itself. Corrected: production `EffectsPhaseSetup` (the phase's set-up
    sequence) publishes it; the test drives that sequence.
  - A1-F8 (R2-F8): the test supplied the identity and publication. Corrected: the early release is production
    `Sprint7ConditionOwner.ReleaseAndPublish` (Dust's clear and early-exit cast); the test drives it.
  - A1-F9 (R1-F13): map rows misbound; R2-F7 text overstated. Corrected above and in the finding map.

R1-F12 (scope disclosure, records only): the `s7-c3-fx` selection ran the whole inherited effects phase. Besides the
mechanical ownership and evaluator observations, its passing run `20261126T054228Z` also produced the phase's presentation
material: the Dust cast frame captures (`r6-frames/s7-dust-cast-0..2`, `s7-dust-native-look`; the reviewer counts 56, 58
and 58 cast frames), the native-look comparison cast, and the five impact and seven Trumpet listening samples posted by the
observer (as did the failed run `20261126T051633Z`). That capture was inherited, not sought; it cost time only inside the two
recorded sessions; it is not taste evidence and is not offered as such. (This disclosure alone did not close R1-F12; the
selection itself is corrected under C3 amendment 1, below.) Owner visual and listening review continues to use
the existing material: the parent's model/desktop/Trumpet evidence and `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r5.zip`
(SHA-256 `2b5f461050807c6593de62a397e0ac16606f6d952729d999e425c88ddfed2565`). A completed C3 should select only the
mechanical cells (no frame capture, native-look playback or samples) while keeping the enclosing admission check.

## Conditional integration plan (records only; every step needs the owner's authorization)

Frozen inputs and reviewed identities (product source, reviewed head and controller archive kept apart):

| PR | Branch | Reviewed H | Archive A | State | Product source that ran natively |
|---|---|---|---|---|---|
| #219 | `codex/s7-visual-audio-remediation` | `3e2baea4b4` (round 7) | n/a; frozen head `0542b0592a` | BLOCKED 7/7 | `9c39b1ff7f` (candidate 3) |
| #221 C1 | `codex/s7-recovery-c1-cue-evaluator` | `5a56d480a2` | `2c67a049ee` | owner-accepted (implementation/domain) | none of its own; its evaluator ran in C3's candidate `6abd692fa7` |
| #222 C2 | `codex/s7-recovery-c2-ghaele-late-units` | `da811b123c` | `eab90a5ac8` | AWAITING_OWNER_REVIEW, handoff current | external harness `d77754ba…b4f1` over candidate 3 |
| #223 C3 | `codex/s7-recovery-c3-effects-ownership` | `7fff8b34f6` (round 1), `89d4efa347` (round 2), `f0ca78cbab` (round 3); round 4 pending | none yet | ACTIVE/develop at 3/4 under C3 amendment 2 (after the no-progress stop) | `6abd692fa7` (candidate `de63d6f4…4b46`, `20261126T054228Z`); `66b0ff3f14` (recovery candidate 3, `473f2f5a…8366`, `20261126T134425Z`); the amendment-2 corrections await recovery candidate 4 |

Dependency checks (2026-11-26): `0542b0592a` is an ancestor of `2c67a049ee`, which is an ancestor of `eab90a5ac8`, which is
an ancestor of `7fff8b34f6`; every remote branch head matched the identities above when checked (2026-11-26, before C3
amendment 1). C1's and C2's current heads differ from their reviewed H only by the controller's archive records. C3's head at
that check (`254c08c53d`) differed from its round-1 H by records only. (History, dated: under C3 amendment 1 the compiled
code changed in `82dc667c93` and `89d4efa347`, reviewed in round 2; the round-2 corrections were built as recovery candidate 3
from `66b0ff3f14` and qualified by `20261126T134425Z`; round 3 reviewed `f0ca78cbab`.) Current (2026-11-26, C3 amendment 2):
the remote heads were re-verified unchanged at the amendment (#219 `0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 records
head `80d224bd56`); the amendment-2 corrections change compiled code again and are to be built as recovery candidate 4 and
qualified by one mechanics session before formal review 4. Source, reviewed H, current records head and archive A stay
separate identities. The stack is linear on the frozen parent, so stacking the children onto #219 needs no conflict
resolution while #219 stays frozen; that says nothing about integration with `master`, which has moved independently and may
conflict.

Finding reconciliation (current, 2026-11-26 under C3 amendment 2): parent R7-F1, R7-F6 (C1, owner-accepted; its same-run
native criterion fulfilled and accepted at `20261126T054228Z` on `6abd692fa7`), R6-F5 (C1 records), R5-F1 (C2, owner-accepted
at harness `d77754ba…b4f1` / `20261126T033419Z`), R4-F1 and R7-F2 to R7-F5 (C3: the round-2 corrections qualified natively by
recovery candidate 3 at `20261126T134425Z`, at its stated scope; the amendment-2 corrections of C3 review findings R3-F1 to
R3-F4, R2-F7, R2-F8, R1-F9 and R1-F11 are in source and focused tests and still need recovery candidate 4's mechanics session
and formal review 4; R1-F13 and R3-F5 are records). (History: at the review-3 no-progress stop these findings were open.) (History: the earlier C3 stop at round 1, before C3 amendment 1, had R1-F1 to R1-F11 open; the round-1
native evidence was `20261126T054228Z` only.) Historical native FAILs and accepted interpretations stay as recorded.

Proposed integration operation (to run only under a later owner authorization; one authorization may cover the routine
read-only checks):
1. Pre-checks (read-only): #219's head is still its frozen records head `0542b0592a` (it has no archive A); C1's remote head is
   its archive A `2c67a049ee` and its base is #219's frozen head `0542b0592a`; C2's remote head is its archive A `eab90a5ac8`
   and its base is C1's archive A; C3's remote head is its final archive A and its base is C2's archive A `eab90a5ac8`;
   `status --remote` shows C2 and C3 AWAITING_OWNER_REVIEW with handoff_current=true.
2. Integrate bottom-up onto the frozen parent without rewriting reviewed history, with the destination named at each step:
   fast-forward the parent branch `codex/s7-visual-audio-remediation` (PR #219's head) to C1's archive A, then to C2's archive
   A, then to C3's final archive A (each a strict descendant of the last), or equivalently merge #221 into #219's branch, then
   #222, then #223, with merge commits that keep every reviewed commit. Either keeps every reviewed H and A reachable; no child
   is merged into its prerequisite's branch.
3. Then the remaining gates, each an owner decision: resuming or re-reviewing #219 (its 7/7 enrollment is BLOCKED; any
   continuation needs owner authority), the parent's own final validation boundary on the integrated head (its records:
   65 min 42 s in rounds 6–7, kept apart from this recovery's ledger), and the separate integration of #219 with `master`
   (conflicts possible; not covered by the linear-stack check).
4. Visual approval (T2/T3/T6) and listening choices (T5/T7) remain the owner's, on the existing material; merge to master,
   release and tags stay out of scope until the owner decides.

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
| R7-F1 live and persisted callback verdicts disagree | C1 `codex/s7-recovery-c1-cue-evaluator` (PR #221) | Strict evaluator: absent, undefined, unreadable and out-of-range vs explicit null in both representations; one row-based round-7 entry point for the in-game collector and every reader | `sprint7-visual.impact-cue-playback-verdicts`: the persisted `…224949Z` row and its collector-shaped live counterpart (cue records through the collector's own `CueRow`, String-typed nulls, never serialized) both characterize; omitted fields are "missing" in both | #219 | C1 deferred native check FULFILLED by C3: `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` judged the row live (verdict null, characterized) and `sprint7-visual.combined-run-verdicts-agree` gives the persisted row and its live-shaped counterpart the same verdict; C1 branch unchanged |
| R7-F6 settled verdict not bound to cue IDs and stop count | C1 (PR #221) | Identity binding and stop/callback reconciliation across every snapshot the collector emitted | Same case: substituted retirement-frame, window-end, disposal and settled-other IDs, a consistent survivor substitution the source inventory contradicts, the same ID twice, invalid and oversized IDs; positive retirement-frame or through-window stop counts, stop reasons, stop results, stop times and window-sample ends; placeholder or uncounted withheld callbacks; all refuse, in both forms. Retained round-6 rows are judged through `CallbackLossRound6` by their own collector generation (`Round6First` for `f3bfab1303`, `Round6Second` for `740d94a995`) | #219 | C1 deferred native check FULFILLED by C3: `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` judged the row live (verdict null, characterized) and `sprint7-visual.combined-run-verdicts-agree` gives the persisted row and its live-shaped counterpart the same verdict; C1 branch unchanged |
| R6-F5 round-7 expenditure and harness suffix | C1 (records disposition only) | Confirm the frozen head's record and planning row from existing receipts | This manifest, below | #219 | Parent records reconciliation at integration |
| R5-F1 unaccounted late Ghaele | C2 `codex/s7-recovery-c2-ghaele-late-units` (PR #222) | Caller reconciles every correlated late unit with the destruction owner; outstanding units fail ProbeSlot isolation | Domain `sprint7-visual.ghaele-late-units-reach-slot` (ProbeSlot / Poll / Release / unwind); native `20261126T002852Z-…-s7-ghaele-lifecycle` PASS (round 1, harness `0a038e40`; round 2: `20261126T005206Z-…-s7-ghaele-lifecycle` PASS, harness `bf5d74a1…63cd` from `44fd90a8d1`; round 3 under C2 amendment 1: `20261126T024816Z-…-s7-ghaele-lifecycle` PASS, harness `4180621b…db61` from `56e359f2cc`; C2 BLOCKED at 3/3 with R3-F1 and R3-F2 blocking, R3-F3 open; round 4 under C2 amendment 2: `20261126T033419Z-…-s7-ghaele-lifecycle` PASS, harness `d77754ba…b4f1` from `d631d283aa`) on candidate-3 package `49f8b88c…6413` / DLL `5969f596…e2c3` with harness `0a038e40…71ca9` built from `c5fe26d146` (`docs/evidence/sprint7/recovery-c2/`) | C1 | C2 harness identity carried into C3 and the parent |
| R4-F1 independent release failures past the abandonment drivers | C3 `codex/s7-recovery-c3-effects-ownership` (PR #223) | Complete child outcome in every driver (`Sprint7Abandonment`); unexpected child failures reach the phase by name and survive the repair | Domain `sprint7-visual.effects-ownership-recovery` (paired injected fault plus independent command-release failure through the phase sequence); native `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7`: all four drivers proven, none unexpected | C2 | Current (2026-11-26, C3 amendment 2): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); amendment-2 corrections bound by round 3 (R3-F2 recovered-context and unresolved attribution; R1-F9 guarded phase set-up sequence; R2-F8 removal bound to its instance, published once) in source and focused tests (`sprint7-visual.effects-ownership-recovery`), awaiting recovery candidate 4 and formal review 4. (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F2 ordinary Dust executions interrupted before retention | C3 (PR #223) | `Sprint7OwnedCommands`: process retained before interruption; commands released independently; ledger owns executions by retained process | Domain (failing interruption does not skip the next; cleared field keeps the process); native pre-delivery release in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` (process retained, ended, none live) | C2 | Current (2026-11-26, C3 amendment 2): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); amendment-2 corrections bound by round 3 (R3-F1 later outcomes forwarded to the phase; R3-F2 recovered-context and unresolved attribution; R2-F7 issued-command record with the queue guard; R3-F3 declared controls read once; R2-F8 removal identity) in source and focused tests (`sprint7-visual.effects-ownership-recovery`), awaiting recovery candidate 4 and formal review 4. (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F3 incomplete hook setup rollback | C3 (PR #223) | `Sprint7HookAttempt` registered before set-up, readback by identity; `IsolatedCell` sticky cell exceptions; caller-chosen trace identity | Domain (failed rollback, null result, unreadable readback, sticky exception); native callback and trace set-up-failed controls in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` (set-up and rollback errors, readback 1 and 2 hooks, next cell refused, repaired to 0) | C2 | Current (2026-11-26, C3 amendment 2): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); amendment-2 corrections bound by round 3 (none bound in round 3 (unchanged since candidate 3)) in source and focused tests (`sprint7-visual.effects-ownership-recovery`), awaiting recovery candidate 4 and formal review 4. (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F4 failed direct cue-source retirement | C3 (PR #223) | `Sprint7OwnedRequests`: one request per source, failure named and sticky, unsettled source a residual | Domain (throwing retirement through the phase sequence); native retirements in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` | C2 | Current (2026-11-26, C3 amendment 2): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); amendment-2 corrections bound by round 3 (R2-F7 issued-command record with the queue guard; R2-F8 removal identity) in source and focused tests (`sprint7-visual.effects-ownership-recovery`), awaiting recovery candidate 4 and formal review 4. (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F5 Dust driver disposed during recovery | C3 (PR #223) | Repair of the exact injected condition once on every exit, including disposal in the recovery wait; failed repair kept | Domain (disposal in the recovery wait, successful and failed repair, foreign kept); native Dust driver in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` | C2 | Current (2026-11-26, C3 amendment 2): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); amendment-2 corrections bound by round 3 (R3-F3 declared controls read once, outside target owned from acquisition and retired once before the fallback) in source and focused tests (`sprint7-visual.effects-ownership-recovery`), awaiting recovery candidate 4 and formal review 4. (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |

C3 findings bound to C3 mission sections rather than a parent row (round-3 `acceptance_ids`): R3-F4 (C3 §5; one reconciliation
obligation, immediate and deferred), R1-F11 (C3 §8; production retirement record through the fixture decision), R1-F13 (C3 §§6–8;
these records). R3-F5 is the ledger arithmetic (records). All are in source/tests/records at the current head and await recovery
candidate 4's mechanics session and formal review 4; the `20261126T134425Z` PASS stays bound to candidate 3 (source `66b0ff3f14`),
distinct from reviewed head `f0ca78cbab`, records commit `80d224bd56`, the correction commits and the eventual archive A.

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
| C2 r5 | Controller gates on `da811b123c`: `sprint7-visual` 23 tests 4.7 s, `loader-compat-probe` 5 tests 3.8 s (receipts 2.7 + 1.9 = 4.6 s) | PASS | 8.5 s batch wall | 14 min 38.6 s | 30 min 21.4 s |
| C3 | Focused gate `sprint7-visual` (local, C3 source) | FAIL: compile error (an unassigned focused-injection field in production) | 15.6 s | 14 min 54.2 s | 30 min 5.8 s |
| C3 | Focused gate `sprint7-visual` (local) | FAIL: compile error (driver fields unassigned in the domain project before its test existed) | 19.4 s | 15 min 13.6 s | 29 min 46.4 s |
| C3 | Focused gate `sprint7-visual` (local) | PASS, 24 tests | 3.9 s | 15 min 17.5 s | 29 min 42.5 s |
| C3 | Build-Local `-DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual` on `988d40bb1e` (product candidate 1 of 2: package `424240171ea7e16c529d423dd61267a90e2c14c97ac214bfbd715d7f519d9484`, DLL `eac800c3ed003cd2e3aab03008d6aeb5c43f0ecf4b3bf690cd87d0518df3c339`; 24 selected tests) | PASS | 7 min 48.6 s | 23 min 6.1 s | 21 min 53.9 s |
| C3 | Session `20261126T051633Z-…-s7-c3-fx` (candidate `42424017…9484`, C2 harness `d77754ba…b4f1`) | FAIL: 3 checks (Dust cell unowned-execution control expired before cleanup, displaced by the new pre-delivery step; Dust abandonment driver read recolours live before their release wait; hence phase isolation, samples refused); restoration completed, owner files verified | 5 min 28 s | 28 min 34.1 s | 16 min 25.9 s |
| C3 | Focused gate `sprint7-visual` (local, contingency correction) | PASS, 24 tests | 20.2 s | 28 min 54.3 s | 16 min 5.7 s |
| C3 | Build-Local (same arguments) on `6abd692fa7` (product candidate 2 of 2, the authorized contingency: package `de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46`, DLL `ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a`; 24 selected tests) | PASS | 6 min 58.4 s | 35 min 52.7 s | 9 min 7.3 s |
| C3 | Session `20261126T054228Z-…-s7-c3-fx` (candidate `de63d6f4…4b46`, C2 harness `d77754ba…b4f1`) | PASS, 41/41 checks; restoration completed, owner files verified, registry restored | 6 min 18 s | 42 min 10.7 s | 2 min 49.3 s |
| C3 | Focused gate `sprint7-visual` (local, with the same-run agreement test) | PASS, 25 tests | 3.9 s | 42 min 14.6 s | 2 min 45.4 s |
| C3 r1 | Controller gates on `7fff8b34f6`: `sprint7-visual` 25 tests 3.9 s, `summon-playtest-r6` 11 tests 3.2 s, `native-profile` 9 tests 3.5 s, `loader-compat-probe` 5 tests 3.1 s (receipts 2.2 + 1.6 + 1.8 + 1.6 = 7.2 s) | PASS | 14.3 s batch wall | 42 min 28.9 s | 2 min 31.1 s |
| C3 a1 | (C3 amendment 1: allowance raised to 75 min total; remaining 32 min 31.1 s) | | | 42 min 28.9 s | 32 min 31.1 s of 75 |
| C3 a1 | Focused gate `sprint7-visual` (local, correction source) | FAIL: compile errors in the domain project (old focused test against the new driver API) | 16.9 s | 42 min 45.8 s | 32 min 14.2 s |
| C3 a1 | Focused gate `sprint7-visual` (local) | FAIL: compile errors (unassigned test fields) | 19.3 s | 43 min 5.1 s | 31 min 54.9 s |
| C3 a1 | Focused gate `sprint7-visual` (local) | PASS, 25 tests (one test waited out a 30 s settle window) | 34.2 s | 43 min 39.3 s | 31 min 20.7 s |
| C3 a1 | Focused gate `sprint7-visual` (local, settle windows shortened) | PASS, 25 tests | 5.4 s | 43 min 44.7 s | 31 min 15.3 s |
| C3 a1 | Focused gate `sprint7-visual` (local, immediate-settlement correction) | FAIL: compile error (a test assertion inserted between if and else) | 17.1 s | 44 min 1.8 s | 30 min 58.2 s |
| C3 a1 | Focused gate `sprint7-visual` (local) | PASS, 25 tests | 4.9 s | 44 min 6.7 s | 30 min 53.3 s |
| C3 r2 | Controller gates on `89d4efa347`: `sprint7-visual` 25 tests 5.2 s, `summon-playtest-r6` 11 tests 3.4 s, `native-profile` 9 tests 3.8 s, `loader-compat-probe` 5 tests 3.8 s (receipts 3.5 + 1.7 + 2.1 + 2.0 = 9.3 s) | PASS | 16.7 s batch wall | 44 min 23.4 s | 30 min 36.6 s |
| C3 r2 | (A first round-2 report submission was refused by the controller, nothing launched: a required validation marked not_run) | refused | 0 s | 44 min 23.4 s | 30 min 36.6 s |
| C3 r2+ | Focused gate `sprint7-visual` (local, round-2 corrections) | FAIL: compile error (new driver hook unassigned in the domain project) | 24.4 s | 44 min 47.8 s | 30 min 12.2 s |
| C3 r2+ | Focused gate `sprint7-visual` (local) | FAIL: compile error (a test local name clash) | 4.2 s | 44 min 52.0 s | 30 min 8.0 s |
| C3 r2+ | Focused gate `sprint7-visual` (local) | FAIL: one test assumption (the ledger never acquires a preexisting instance) | 6.9 s | 44 min 58.9 s | 30 min 1.1 s |
| C3 r2+ | Focused gate `sprint7-visual` (local) | FAIL: one test reused another case's out variables | 6.2 s | 45 min 5.1 s | 29 min 54.9 s |
| C3 r2+ | Focused gate `sprint7-visual` (local) | PASS, 25 tests | 5.9 s | 45 min 11.0 s | 29 min 49.0 s |
| C3 r3 | (estimate before recovery candidate 3: build about 7 min, mechanics-only session about 7 min, final controller gates about 0.5 min; about 15 min left after, enough for one contingency) | | | 45 min 11.0 s | 29 min 49.0 s |
| C3 r3 | Build-Local `-DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual` on `66b0ff3f14` (recovery candidate 3 of 4: package `473f2f5a78229f3e2e15c3d97b863f2201f44ae372d2485de1fdb05010618366`, DLL `8e574477cce11621ca2ec88fdeed358cb9f8f76ff8ced423e154e3c86d277d07`; 25 selected tests) | PASS | 8 min 0.9 s | 53 min 11.9 s | 21 min 48.1 s |
| C3 r3 | Session `20261126T134425Z-…-s7-c3-mechanics` (recovery candidate 3, C2 harness `d77754ba…b4f1`) | PASS, 43/43 checks; required cells completed; restoration completed, owner files and saves verified, registry restored | 6 min 3 s | 59 min 14.9 s | 15 min 45.1 s |
| C3 r3 | Focused gate `sprint7-visual` (local, with the candidate-3 same-run agreement) | PASS, 25 tests | 4.8 s | 59 min 19.7 s | 15 min 40.3 s |
| C3 r3 | Controller gates on `f0ca78cbab`: `sprint7-visual` 25 tests 5.8 s, `summon-playtest-r6` 11 tests 3.5 s, `native-profile` 9 tests 3.6 s, `loader-compat-probe` 5 tests 3.9 s (receipts 3.6 + 1.7 + 1.8 + 1.8 = 8.9 s; the round-3 report said 6.9 s in error) | PASS | 17.3 s batch wall | 59 min 37.0 s | 15 min 23.0 s |
| C3 amendment 2 | Allowance raised from 75 to 90 min total (59 min 37.0 s spent; remaining shown against 90 from here) | | | 59 min 37.0 s | 30 min 23.0 s |
| C3 r4 | Focused gate `sprint7-visual` (local, amendment-2 corrections) | FAIL, 24/25: the new R3-F4 case arranged its throwing end-state read one read too late (`ThrowAfter = 3`; the census makes two reads), so settlement succeeded; test arrangement corrected to 2 | 24.4 s | 60 min 1.4 s | 29 min 58.6 s |
| C3 r4 | Focused gate `sprint7-visual` (local) | PASS, 25 tests | 5.1 s | 60 min 6.5 s | 29 min 53.5 s |
| C3 r4 | Focused gate `sprint7-visual` (local, after restoring the observer's observation-time source stilling) | PASS, 25 tests | 20.3 s | 60 min 26.8 s | 29 min 33.2 s |
| C3 r4 | Focused gate `sprint7-visual` (local, pre-build audit A1-F1 to A1-F9 corrections) | PASS, 25 tests | 19.7 s | 60 min 46.5 s | 29 min 13.5 s |

Notes:
- Through C1 (to 2026-11-26 00:24Z) no product package, harness build or game session had run in the recovery; C2 then
  built its harness four times and ran four short lifecycle sessions on the existing candidate-3 package (rows above).
- Failed validation attempts so far: eighteen (two in C1 round 2, four in C1 round 4, one in C1 round 5, eleven in C3; seventeen through the review-3 stop, one since); refused controller submissions with no validation launched are listed in their rows.
- R3-F5 (records): a 1.0 s arithmetic drift from the 45 min 5.1 s row was corrected on 2026-11-26 in every later running total; the round-3 report's 59 min 36.0 s should read 59 min 37.0 s and its receipt subtotal 8.9 s.
- Earlier spend stays separate: 65 min 42 s in parent rounds 6 and 7. New product candidates used: 3 of 4 (C3's first combined candidate, its contingency, and recovery candidate 3 under C3 amendment 1).
- Reserve before the C2 round-3 session: C3 still needs one combined Build-Local candidate and its affected native
  qualification (the effects session, estimated at about 18 minutes, plus the build and gates). After the planned C2 short
  session (about 2 min 15 s) and controller gates, about 32 minutes remain for C3.
- Reserve before the C2 round-4 session: C3 is estimated at about 25 minutes (combined build, focused gates, effects
  qualification); after this session (about 2 min 15 s) and the round-4 controller gates about 30 min 25 s remain, leaving
  about 5 minutes of margin for one short C2 rerun if round 5 needs one.
