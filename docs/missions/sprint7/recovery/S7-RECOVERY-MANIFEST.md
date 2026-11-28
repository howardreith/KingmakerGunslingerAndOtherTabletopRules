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
  subscribed), and terminal teardown runs it once and names "not settled at teardown". (Dated: under C3 amendment 3 §3B the
  terminal action's own unresolved results are named at once, and an obligation whose reads then establish absence and
  subscription release is observed settled without erasing those failures; see the amendment-3 corrections below.)
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
  the other actor and combat exit on both. (Dated, coverage at `03a78d4987`: the follow-up audit found the passive callback
  test-defined (A1-F5) and combat exit unguarded (A1-F6); this statement overstated both. Current coverage: the amendment-3
  corrections below.)
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
- Verification follow-up (the one authorized), 2026-11-26 16:56:03Z–17:14:06Z, on
  `03a78d498749b499ac8c5f97617ca282311c7591`: **corrections_required**. Verified A1-F3, A1-F7, A1-F8. Not verified:
  - A1-F1: no case drives missed notifications plus recovery between the initial release and a zero-live census.
  - A1-F2: `ledgerHeldUntilSettlement` is still inferred from incomplete reconciliation, and a throwing late release followed
    by an empty read and a successful unsubscribe still discharges the terminal action (the late sweep's completion is not tracked).
  - A1-F4: the test chooses the controls and wires the retirement publication itself; removing the same-caster entry or the
    publication from the Dust caller would stay green.
  - A1-F5: the test defines its own passive callback; deleting the phase registration would stay green.
  - A1-F6: native `LeaveCombat` can issue a return move (`UnitCombatState.ReturnToStartingPositionIfNeeded`) whose
    `UnitCommands.Run` clears the queue, so a foreign queued command can disappear despite the withheld maintenance.
  - A1-F9: the active map rows still disagree with the controller's `rounds/r03/result-1.json` bindings (the reviewer cites
    R4-F1: R2-F7, R2-F8, R1-F11; R7-F2: R3-F1, R3-F2, R2-F7, R3-F3, R3-F4, R1-F11; R7-F4: R3-F3, R1-F11; R7-F5: R3-F1, R3-F4;
    R1-F9 to C3 §5, R1-F13 to §8, R3-F5 to §§6–8), and the focused-coverage statement overstates the passive-callback and
    combat-exit coverage. (The bindings recorded above were read from the posted round-3 PR comment; the two sources differ.)
    (Withdrawn 2026-11-26 under C3 amendment 3: a one-off extraction shows the structured JSON of the published review and the
    controller's `rounds/r03/result-1.json` carry identical `acceptance_ids` for the same request and commit; the earlier map was
    a hand-transcription error, not a difference between sources.)
  New blocking findings:
  - A2-F1 (A1-F3/R3-F3): the first `TerminatePending` in `PhaseTeardownSettlement` finalizes an outstanding retirement before a
    later fallback retirement's destruction ticks could settle it, and `Publish` never re-reads a published target.
  - A2-F2 (A1-F1/R3-F2): the exact-execution presence count (`PresentFrom`, used by the pre-delivery driver's safety readback)
    filters out unresolved attributions and can report zero.
  - A2-F3 (A1-F5/R2-F7/R3-F2): the phase retries a passive command's interruption before the transferred Dust owner can capture
    a recovered process, so a clearing interruption can lose the recoverable execution identity.
  The audit results are committed under `docs/evidence/sprint7/recovery-c3/pre-build-audit/` (initial `b31359cb…36d9`,
  follow-up `7f29a05a…2384`).

**C3 STOPPED at the pre-build audit gate (C3 amendment 2 §5), 2026-11-26.** The authorized follow-up did not clear the audit:
it still identifies blocking C3 defects and untested failure boundaries. Under §5 ("Do not spend candidate 4 while this bounded
audit still identifies a blocking C3 defect or an untested real failure boundary. Preserve and report an unresolved audit result
if the authorized follow-up cannot clear it"), recovery candidate 4 was **not** built, no mechanics session ran, and formal
review 4 was **not** used (§6 reserves it for the full corrected-byte result; no code-only submission). No code was changed
after the audited commit `03a78d4987`. Controller: ACTIVE/develop at 3/4 under amendments revision 2 (unchanged; not
finalized, no archive A). Remaining obligation: correct A1-F1, A1-F2, A1-F4, A1-F5, A1-F6, A1-F9 and A2-F1 to A2-F3, then
build and qualify candidate 4 and use review 4. That needs an owner decision, because no further audit follow-up is
authorized and nothing else can clear the gate.

**C3 amendment 3 (owner, 2026-11-26): replace the exhausted audit prerequisite.** Saved verbatim at
`owner-guidance/S7-RECOVERY-C3-AMENDMENT-3-2026-11-26.md` (message sha256
`a82cb5ce8ba6c84f4b216c49d4739c0d287cc47e1c53affee1ea2f4aa83cf34b`). Applied with `resume --amend-mission` (captured
2026-11-26T22:27:31Z as cumulative owner-amendments revision 3, sha256
`a64ab24d9e7445495e895b5d7f851bbf6f846722f2ffba1c8f0bc7d48a47ef1a`; ACTIVE/develop at 3/4, `--max-rounds` unchanged);
`status --instructions` parts 1–6 displayed in the session. Remote identities were verified unchanged first (#219 `0542b0592a`,
C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 `38f49b22b3`). The amendment replaces amendment 2 §5's clean-audit prerequisite; both
audits stay evidence with their original verdicts (A1-F3, A1-F7 and A1-F8 verified by the follow-up at their stated scopes). No
further audit was run. Ceilings unchanged: 90 minutes total, four candidates, four formal reviews.

Corrections under C3 amendment 3 (code `d9a07ef301`; focused gate `sprint7-visual` PASS, 25 tests; **not yet run in game**).
Audit labels are evidence identifiers, not controller findings; the formal round-3 finding each serves is in brackets. Each
line separates the code correction, the focused test and what is not claimed:
- **A1-F1** [R3-F2] (§3A). Code: unchanged tri-state attribution. Focused (new): the release's process read throws, so the
  initial release cannot attribute the missed condition; the exact already-ended process becomes readable at the zero-live
  census; the immediate reconciliation removes exactly that condition; `removedAtCleanup` 0, `settledAtCleanup` true, no
  observed delivery, foreign and pre-existing (same-context) instances intact.
- **A2-F2** [R3-F2] (§3A). Code: `Sprint7ConditionOwner.PresentFrom` returns unknown (null) when the present conditions, a
  present condition's attribution or its execution cannot be read; known-foreign conditions stay excluded; the Dust cell's
  `PresentFrom` uses it (an unreadable process context is also unknown); the pre-delivery driver's criterion is
  `ExecutionConditionsReleased` (ended and a known zero). Focused: a throwing condition-context read yields null and the
  criterion fails; a known-foreign condition gives zero; the readable instance is one exact match.
- **A1-F2** [R3-F1, R3-F4] (§3B). Code: `ReleaseOwnedEffects` tracks whether the late sweep completed (`lateSweepCompleted`); a
  sweep that threw keeps the obligation unresolved and is not retried by settlement reads (its failure stays published), and the
  one terminal attempt remains (`late condition sweep not completed` is named if it fails again); removals the ledger recorded as
  failed are never requested again; `subscriptionReleased`/`ledgerHeldUntilSettlement` come from the unsubscribe outcome itself.
  `Sprint7CellIsolation.RunTerminators` names a terminal action's unresolved results at once; `TerminatePending` then re-reads,
  so a settled obligation is observed settled (the earlier failures stay; qualification still fails) and anything live or
  unreadable is "not settled at teardown". Focused (new): the throwing sweep with an empty read and a released subscription is
  not discharged and not retried, then the terminal attempt completes it; an unreadable final read with a successful
  unsubscribe reports the subscription released; a failed removal is not retried and ends named. Two earlier cases (R2-F2,
  R3-F4) were updated to this terminal reading.
- **A2-F1** [R3-F3] (§3C). Code: `PhaseTeardownSettlement` reads the residuals, runs each pending obligation's teardown action
  (a handed-over Dust reconciliation reads its controls and retires the outside target while it exists) with every readback kept
  executable, then the fallback retirement of units never requested and its publication, then `TerminatePending` (re-read, then
  name). Focused (new): A requested earlier and still present settles during B's fallback retirement and is observed settled;
  one request each for A, B and C; the never-settling C is named; the teardown action ran before the fallback. The forced
  teardown case (settles true/false) still passes through the production Dust control declaration.
- **A1-F4** [R3-F3] (§3D). Code: `DustControlSettlement` is the Dust cell's production declaration (caster-behind required once
  placed or held; same-caster required once due), the outside target's one request through the phase record and the
  publication to the cell's isolation; the Dust caller now calls it. Focused: driven deferred through the driver's handoff
  (throwing caster-behind read once, named; the due same-caster control still released; one outside retirement whose residual
  is forwarded to the phase; refused admission), and on the ordinary (due, no handle: named, next cell refused) and stopped
  (not due: reported, next cell admitted) paths.
- **A1-F5, A2-F3** [R2-F7, R3-F2] (§3E). Code: `RegisterCellCommand` registers a Dust command with its owner and in the phase
  record together with the owner's capture, before the single native run, on the passive route (inside R6RunPassive, now
  through `IssueAndRun`) and both direct routes; `Sprint7IssuedCommands.Release` runs that capture on its own before each phase
  interruption. Focused (one cohesive case): the production issue path with both owners registered at the run boundary and the
  foreign same-caster command in neither; the cell's process read unavailable and its interruption failing; cleanup handed to
  the phase; the process readable; the phase retry captures it before its interruption clears the field; the deferred
  reconciliation removes the late condition through the retained process; the original failure stays.
- **A1-F6** [R2-F7] (§3F). Code: queue maintenance and combat exit each run only while the actor's queue holds no command outside
  the record, otherwise they are withheld, reported (`commandCleanupWithheld`) and not recorded as performed
  (`commandCleanupPerformed`); the fixture caster's and hostile's combat exit use the same guard (`ExitCombat`). The native
  mechanism (`UnitCombatState.LeaveCombat` → `ReturnToStartingPositionIfNeeded` → `UnitCommands.Run` clearing the queue) is
  audit-sourced: read in the 2.1.7b reference by the follow-up audit, not executed here. Focused: distinct owned and foreign
  command objects; a combat-exit boundary that clears the queue if invoked; A's foreign work keeps identity and order, B and the
  hostile are still cleaned up although A's interruption failed.
- **A1-F9** [R1-F13] (§4). Records: the map below is regenerated from a one-off extraction of the published round-3 structured
  review (`acceptance_ids`, request `…-r03-db7304`, commit `f0ca78cbab`), cross-checked identical with the controller's
  `rounds/r03/result-1.json` (sha256 `7fe59f08…9760`); the earlier "sources differ" note is withdrawn (dated above).

None of these is a native observation or a formal verification; candidate 4's mechanics session and formal review 4 remain.

**C3 native qualification of the corrected bytes (recovery candidate 4 of 4).** Built once from records head `6cef765a8a`
(compiled source identical to code commit `d9a07ef301`; the records commit changes only `docs/`): package
`242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93`, DLL
`a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2` (Build-Local `-DevelopmentPurpose s7-recovery-c3`, 25 selected
tests). Harness C2's `d77754ba…b4f1` (unchanged; staged DLL hash read back), fixture the admitted disposable outdoor native Default
fixture of `disposable-expanded-summoning-sprint32-native-review`, selection `summon-s7-effects-mechanics` (session file
`s7-c3-mechanics.json`, unchanged, its seven required cells declared before the run). Session
`20261126T232851Z-owner-0.33.0-s7-c3-mechanics`: PASS, 43/43 checks (window 23:28Z–23:35Z, agreed with the other session on DATA).
- Required cells: all seven admitted and completed (`s7-effects-required-cells-completed`); separately, physical restoration true
  (`s7-effects-owned-state-restored`): no failure, pending, unreadable or unreported entry; one recovery (the ordinary Dust
  cell's reconciliation obligation observed settled after its execution ended).
- Drivers: callback-abandoned, callback-setup-failed, trace-setup-failed, dust-pre-delivery-disposed and dust-abandoned all proven
  (no unexpected, none missing, repair after the guard). Their `transferredResiduals` are empty: this run is **not** native
  qualification of failures published after a handoff; those boundaries are focused-test coverage only.
- Ordinary disposal before delivery: disposed in readiness frame 11797 with execution
  `KMG_Summoning_Special_DustMephit_Breath#958676480` started, not ended, not delivered (presence 0 known); later that execution
  delivered one Sickened (acquired 1), released at settlement (presence from it a known 0, `ExecutionConditionsReleased`); the
  ledger was held until settlement (`ledgerHeldUntilSettlement` true from the subscription's own state), the late sweep
  completed, nothing remained, the subscription was then released.
- Dust controls through the production declaration: the caster-behind and same-caster controls read present after the owned
  release; no control failure; both foreign controls kept through settlement.
- Command cleanup: `commandCleanupPerformed` lists the guarded queue-maintenance and combat-exit callbacks for the eight phase
  summons and the combat-exit callback for the fixture's hostile and caster: each was permitted by the guard and returned. The
  native combat-exit delegate calls `LeaveCombat` only when `IsInCombat` is true, so the receipt does **not** establish ten actual
  `LeaveCombat` calls (corrected after review 4, R4-F2; no per-actor invocation evidence exists). Nothing was withheld
  (`commandCleanupWithheld` empty: no actor held foreign queued work, so the withholding branch is focused-test coverage only).
- Callback loss (constructed setup, labelled as such): declared same-frame delivery 0 posted real cues with both playing (IDs 471,
  472, frame 5333); C1's evaluator judged the row live: verdict null (characterized). The committed persisted row
  `docs/evidence/sprint7/recovery-c3/20261126T232851Z-callback-loss-row.json` (source sha256 `220396e6…`) and its collector-shaped
  counterpart receive the same verdict (`sprint7-visual.combined-run-verdicts-agree`).
- Retirements: 19 requests through the phase's one record (8 by the phase's fallback), none failed.
- Restoration: transaction completed; owner files verified; owner saves and install unchanged; game settings applied and
  registry restored. Mechanics-only: no frame captures, native-look comparison or listening samples.
Candidate 3's `20261126T134425Z` PASS stays bound to source `66b0ff3f14` and its own package/DLL; it does not qualify these bytes.

**C3 STOPPED at the authorized limit: formal review 4 returned changes_required (2026-11-27 00:00Z).** The first submission
(23:39Z) was refused by the controller before any review ran (revision 3 not verifiably displayed: the instruction parts had been
shown through a filter that altered characters and elided part 5); all six parts were then displayed raw and the unchanged report
resubmitted. Review 4, request `…-r04-69cc0c`, on H `43dd44a2bafad0455ab74b6393515c9bae4fd27a`: changes_required; controller
BLOCKED in develop, "review-round cap reached for final (4/4); findings are preserved for an owner-authorized extension". Verified:
R3-F1, R3-F2, R3-F3, R3-F4, R3-F5, R2-F8, R1-F9, R1-F11. Open:
- **R4-F1** (P1, product defect, blocks readiness): a Dust command whose process still reads null (a successful null read) and
  whose first cell interruption throws is neither a retained process nor an unreadable unknown, so the reconciliation can settle
  and discharge before the command starts; a later attributable Sickened would then have no owned sweep. Requested: keep such
  still-unfinished commands in the settlement decision after an unsuccessful release (settling only on established cancellation
  or execution settlement), with a focused case through child disposal, a phase interruption that clears the field, and late
  delivery. This needs changed compiled code, hence another package and another formal review: **not authorized** at that stop
  (candidates 4/4, reviews 4/4; authorized later by C3 amendment 4).
- **R2-F7** (P1, evidence gap): the focused passive case calls `IssueAndRun` with a registration lambda written in the test; the
  production Dust passive binding is a separate lambda in the runner, so removing its phase registration would stay green.
  Requested: make the production passive submission, with its choice of both owners, callable with a controlled Run boundary.
  This changes runtime source (and so the qualified bytes): not done under the stop.
- **R4-F2** (P2, records): corrected above (combat-exit callbacks permitted and returned; `LeaveCombat` conditional on
  `IsInCombat`).
- **R1-F13** (P2, records): the dependency paragraph is corrected above (amendment-2 check dated as history; one current state).
- **R4-F3** (P2, records): the candidate-4 pre-command estimates. Disclosure from the retained session record: before the build
  (23:20Z) the estimate was "about 8 min for the build, 6 min for the session and under 1 min for gates, well inside the 28:43.9
  remaining" (actual 8 min 8.9 s, 5 min 59 s, 4.9 s + 18.5 s); that estimate covered the build, session and final gates together.
  Before the session the remaining allowance (20 min 35.0 s) was stated, with no separate re-estimate. These are statements in the
  operator session's progress messages, not in the committed ledger; the reviewer could not verify them from the supplied inputs.
Remaining obligations at that stop (each then needed new owner authority; C3 amendment 4, below, later authorized them): correct
R4-F1 in product code with its focused control; make the passive submission production-callable for R2-F7; build and natively
qualify the corrected bytes (another package); obtain a further formal review; then finalize. Unused validation allowance at the
stop: 14 min 12.6 s of 90. Preserved: candidate 4's native PASS, candidate 3's PASS, both audit verdicts, every earlier FAIL and
refusal, and the frozen #219 / C1 / C2 stack.

**C3 amendment 4 (owner, 2026-11-26): correct C3/R4-F1 and C3/R2-F7, recovery candidate 5, formal review 5.** Saved verbatim at
`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-4-2026-11-26.md`. Remote heads re-verified unchanged first
(#219 `0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 `3890a5bfad`). Applied through `resume --amend-mission` (2026-11-27
01:45:38Z): **revision 4, digest `93878063c5ea61f94e5bef314e64cf72e15b1ee83e6abe2701500e495bf65ccf`** (message sha256
`a59724615e2efba13e2f5845ebf96aef7a9fae3fef98a03ef7662efcd7023e2b`); the controller stayed BLOCKED ("review-round budget for
final is exhausted (4/4); the owner amendment is applied but no further development is authorized without an explicit owner
extension"). A separate `resume --max-rounds 6` (owner-authorized by the same amendment) then left the enrollment ACTIVE in
develop at final 4/6. All eight instruction parts were displayed raw, one command each. The amendment raises the shared
validation allowance to 105 minutes total (29 min 12.6 s remaining at the amendment), authorizes recovery candidate 5 and formal
review 5, and restricts round 6 to unchanged-runtime evidence corrections. Parent #219's R4-F1 and this C3 review's R4-F1 are
different findings: below, "C3/R4-F1" is the round-4 C3 finding (bound to parent #219 R7-F2, per the structured result); "#219
R4-F1" is the parent finding of the same label, bound to C3/R2-F7 (corrected after review 5, R1-F13; it earlier read "the parent
finding it is bound to", which was wrong).

Corrections under C3 amendment 4 (code `9d99563ec7`), by evidence class:
- **C3/R4-F1, code correction.** `Sprint7OwnedCommands` now distinguishes, for each owned command: an exact retained execution
  (its end governs, even if the command later finishes or its mutable field clears); no execution retained and the command able
  to start (a **pending** command, kept as a named residual with the reconciliation, its subscription and its terminal action);
  cancellation before execution positively established (settles at once, with no invented execution or residual); and an
  unreadable process or terminal state (unknown, named, through the existing bounded terminal accounting). `StartSettled`
  attempts process recovery before reading the terminal fact; a successful null process read is never that fact, and the
  interruption-eligibility predicate (`Unfinished`, false also for a missing or destroyed executor) is not consulted for it.
  `ReleaseOwnedEffects` attempts the immediate settlement only when nothing is live, unknown or pending; its settlement predicate
  includes the pending commands. The original interruption error stays a sticky qualification failure.
- **Engine fact behind the cancellation declaration, source inspection only** (decompiled Kingmaker 2.1.7b `Assembly-CSharp`,
  the lab's private reference copy, not executed): `UnitUseAbility.ExecutionProcess` has a private setter assigned only in
  `OnAction`, which `UnitCommand.Tick` calls immediately before setting `IsActed`; `UnitCommand.Interrupt` sets result Interrupt
  and finishes the command. The Dust owner therefore declares cancellation before execution as finished, never acted and no
  process. `SetIsActed` is public and a turn-based `ForceFinish` sets it without acting; either makes cancellation
  not established (conservative: the command stays pending until terminal accounting).
- **C3/R2-F7, code correction.** `SubmitCellCommand` is the one production submission: it registers the exact command with the
  cell's owner and in the phase's issued-command record (with that owner's capture, run before any phase interruption) and then
  performs the single native run; it refuses a missing owner before the run. The runner's adapter `S7SubmitDust` passes the Dust
  cell's owner, `_s7Issued` and the native `Commands.Run`; the Dust passive route (`R6RunPassive` hands its run to it), the direct
  route and the owning cast all submit through it. The three unrelated R6 passive callers keep the plain native run. `IssueAndRun`
  is removed. The adapter's connection is established by source inspection and by candidate 5's native execution, not by a
  source-string test.
- **Focused behavior (domain, `sprint7-visual.effects-ownership-recovery`).** The amendment-4 §2C scenario through
  `SubmitCellCommand` with only the native run controlled: at the single run the exact command is in both owners and the separate
  same-caster command in neither; a successful null process read and an interruption that throws without finishing; the child is
  disposed through `Sprint7Abandonment` and the pending command, the reconciliation and the subscription reach the phase; the
  phase retry captures the exact live execution into the Dust owner before its interruption clears the field and finishes the
  command; a condition delivered while that execution is live is not discharged by the finished command; after it ends,
  settlement removes exactly that condition (no observed delivery fabricated), keeps the foreign control and a pre-existing
  instance with the same context, releases the subscription, and the original interruption error still fails the qualification.
  Controls: established cancellation before execution settles at cleanup (no pending command or residual, subscription released,
  next cell admitted); an unreadable terminal state stays unknown and is named "not settled at teardown" with the teardown
  reconciliation; a submission without the phase record is refused before its run; the existing throwing-getter handoff and
  phase-retry case now runs through `SubmitCellCommand`.
- **Mutation controls (deliberate, reverted byte-exact; their runs are in the ledger):** with the phase registration removed from
  `SubmitCellCommand`, the focused gate fails on the existing handoff case (registration at the run boundary); with the pending
  state disabled, it fails on scenario step (2). So the controls detect both regressions.
- **Native:** recovery candidate 5 (below). Its rows report `pendingCommands` for every Dust release; in that run they were empty,
  so the null-before-start branch was **not** observed natively: the injected fault is focused evidence only.

**C3 native qualification of the amendment-4 bytes (recovery candidate 5 of 5).** Built once from records head `511db044b1`
(compiled source identical to code commit `9d99563ec7`; the records commit changes only `docs/` and the evidence crosscheck):
package `15307748f65c4dee26e08239f35ff9c2a543855bfb5ea22716c39fcf62ea638e`, DLL
`1dde22e025548941acf107e6e1866f43c61629cc303019e63c7fd7cbb88aba7e` (Build-Local `-DevelopmentPurpose s7-recovery-c3`, 25 selected
tests). Harness C2's `d77754ba…b4f1` (unchanged; staged DLL hash read back), the same admitted disposable outdoor native Default
fixture, selection `summon-s7-effects-mechanics` (session file `s7-c3-mechanics.json`, unchanged, seven required cells declared
before the run). Session `20261127T022022Z-owner-0.33.0-s7-c3-mechanics` (run from records head `c1329dd21d`; window 02:20Z–02:26Z,
agreed with both other sessions on DATA): PASS, 43/43 checks.
- Required cells: all seven admitted and completed (`s7-effects-required-cells-completed`); separately, physical restoration true
  (`s7-effects-owned-state-restored`): no failure, pending, unreadable or unreported entry; one recovery (the ordinary Dust cell's
  reconciliation obligation observed settled).
- Command ownership through the corrected submission: every **cell-owned** Dust command went through `SubmitCellCommand`; the
  deliberately unowned same-caster control cast is submitted directly (`late.Commands.Run(separate)`) and its row records its
  Sickened applied and outside the ledger (`unownedExecution`: `ownedByLedger` false), as that control requires (scoped after
  review 5, R5-F4); the ordinary Dust release settled at cleanup with `pendingCommands` empty, 0 live, the late sweep
  completed and the subscription released; no owned command failure, none left running.
- Drivers: callback-abandoned, callback-setup-failed, trace-setup-failed, dust-pre-delivery-disposed and dust-abandoned all proven;
  their `transferredResiduals` are empty (no native claim about failures after a handoff).
- Ordinary disposal before delivery: disposed in readiness frame 11808 with execution
  `KMG_Summoning_Special_DustMephit_Breath#-649063296` started and no condition from it present (known 0) at disposal; after
  settlement that execution had delivered one Sickened (acquired 1), released (present from it a known 0); the ledger was held
  until settlement, the late sweep completed, the subscription was then released; `pendingCommands` empty.
- Command cleanup: `commandCleanupPerformed` lists 18 permitted-and-returned callbacks (queue maintenance and combat exit for the
  eight phase summons, combat exit for the fixture's hostile and caster; `LeaveCombat` itself runs only when `IsInCombat`);
  `commandCleanupWithheld` empty.
- Callback loss (constructed setup, labelled as such): declared same-frame delivery 0 posted real cues with both playing (IDs 522,
  523, frame 5471); C1's evaluator judged the row live: verdict null. The committed persisted row
  `docs/evidence/sprint7/recovery-c3/20261127T022022Z-callback-loss-row.json` (source sha256 `56f42596…`) and its collector-shaped
  counterpart receive the same verdict (`sprint7-visual.combined-run-verdicts-agree`).
- Retirements: 19 requests through the phase's one record (8 by the phase's fallback), none failed.
- Restoration: transaction completed; owner files verified; owner saves and install unchanged; game settings applied and registry
  restored (readback verified). Mechanics-only: no frame captures, native-look comparison or listening samples.
Candidate 4's `20261126T232851Z` PASS stays at its own scope (source `6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`; an
already-started process; empty `transferredResiduals`); it does not qualify these bytes or the pre-start failure.

**(History, dated 2026-11-27 03:26Z; superseded by C3 amendment 5 below.) C3 STOPPED at the amendment-4 limit: formal review 5 returned changes_required.** Review 5, request `…-r05-e19c0d` on H
`ed02905b7f544071bc0f13b19601762717d0c28e` (round 5 of 6): changes_required; the controller stayed ACTIVE in develop at 5/6 (no
controller stop: the limit is the owner's amendment, not the round ceiling). Verified: C3/R4-F1, C3/R2-F7, C3/R4-F2, C3/R4-F3. Open:
- **R5-F1** (P1, defect, blocks readiness; source inspection by the reviewer, not observed natively): in the reconciliation's
  settlement predicate the checks of retained processes and unknown commands finish before `Pending()` captures again; a read in
  that later capture that finds a live process or throws moves the command into a collection already checked, so the predicate
  can fall through to the sweep and unsubscribe. Requested: one consistent settlement decision over the complete owned-command
  set, with a focused case (successful null read, then an unavailable read in the same settlement pass, then recovery, delivery
  and removal). This is a compiled RuntimeTesting change.
- **R5-F2** (P2, defect, blocks readiness): a command whose first process read threw stays unknown after later successful null
  reads, so `StartSettled` and the residual's `EndedOf` never consult the cancellation fact; a clean pre-execution cancellation
  after a transient read failure would refuse the next cell and fail at teardown. Requested: let positive cancellation evidence
  resolve an earlier unreadable acquisition (preserving uncertainty when the current read fails). A compiled RuntimeTesting change.
- **R5-F3** (P2, evidence gap): the amendment-4 scenario asserts the original interruption error on the child and only
  `!phase.Restored` on the phase; it should assert the exact forwarded failure in `phase.Failures`. A domain-test change; not made
  under the stop (it would need a validation run, and the round it could serve is unavailable while R5-F1/R5-F2 are open).
- **R1-F13** (P2, records): the parent-binding sentence and the candidate-4 footer are corrected above in this stop commit.
- **R5-F4** (P2, records): the candidate-5 submission statement is scoped above to cell-owned Dust commands in this stop commit.
Authority check: amendment 4 §4: "If review 5 requires another runtime/harness correction or another package, stop." R5-F1 and
R5-F2 need changed compiled code under `RuntimeTesting/`, hence another package; round 6 is reserved for unchanged-runtime evidence
corrections, so it is not used. No recovery candidate 6 or further review is authorized. Remaining obligations (each needs new
owner authority): correct R5-F1 and R5-F2 in `Sprint7ObserverPolicy.cs` with their focused cases; strengthen the R5-F3 assertion;
build and natively qualify the corrected bytes; obtain a further formal review; then finalize. Unused validation allowance:
1 min 54.1 s of 105. Preserved: candidate 5's and candidate 4's native PASSes at their own scopes, every earlier FAIL and refusal,
both audit verdicts, and the frozen #219 / C1 / C2 stack.

**C3 amendment 5 (owner, 2026-11-26): correct the settlement decision, recovery candidate 6, formal review 6 (conditional 7).**
Saved verbatim at `C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-5-2026-11-26.md`. Remote heads
re-verified unchanged first (#219 `0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 `bfeec8d8a9`). Applied through
`resume --amend-mission` (2026-11-27 04:55:08Z): **revision 5, digest `d27230135af499980a6940f5addd487438401ed417ea9967b3616385a5888fc8`**
(message sha256 `9a43b3ca7fe2cc6a6dfb518e6ee39fa1fe6967e1554234e484ce38831df31aa9`); ACTIVE in develop at final 5/6. All ten
instruction parts were displayed raw, one command each, in this session. A separate `resume --max-rounds 7` (owner-authorized by
the same amendment) left the enrollment ACTIVE in develop at final 5/7. The amendment supersedes amendment 4's five-candidate
ceiling and its unchanged-runtime restriction of round 6: 145 minutes of validation in total (41 min 54.1 s remaining at the
amendment), six recovery candidates in total, seven formal reviews in total (round 7 conditional on unchanged-runtime work).

Correction under C3 amendment 5 (code `360ce86f54`), by evidence class. R5-F1 and R5-F2 are compiled RuntimeTesting settlement
defects found by the reviewer's source inspection; no ordinary player-facing Dust gameplay defect was reproduced.
- **R5-F1, R5-F2, code correction (`Sprint7ObserverPolicy.cs`).** One observation per command per settlement evaluation
  (wording clarified under C3 amendment 6: an individual command residual and the aggregate reconciliation may evaluate
  separately; each consumer accounts for the observations it makes, and no global snapshot is taken for a whole phase pass),
  `Sprint7OwnedCommands.Observe`, made from a single process read consumed into the owner (`ReadProcess`): a retained or (by
  that read) newly recovered exact execution governs by its end, live or unreadable keeping cleanup owned even after the
  command finishes or clears its field; a failed read is unknown; a successful null read settles only when the command's terminal
  facts establish cancellation before execution (finished, never acted), stays pending while it can start, and is unknown when
  those facts cannot be read. A successful read (null included) removes the command from the current unknown set; the earlier
  failure stays in `UnreadableObserved` (history), and interruption and release failures stay sticky in `Failures` and the
  isolation. The cleanup census (`pendingCommands`, `ownedExecutions`, `ownedExecutionsLive` and the residuals), each command
  residual (the unreadable-command and pending-command residuals both read `Settlement`, the same observation) and the
  reconciliation's settlement predicate (`ObserveAll`, then `Aggregate` over the complete set: unreadable if any is unreadable,
  open if any is live or can start, settled only when all are) use the same classification, so no read moves a command into a
  collection already checked and a process recovered during a poll takes part in that poll's decision. Each poll records its
  observations in the row (`settlementObservations`). `StartSettled`, `EndedOf` and `Pending()` are replaced (the amendment-4
  record above describes them as they were). The diagnostic `Live()` uses the same observations.
- **Cancellation adapter (runner `S7DustConditions`).** The delegate now reads only the terminal facts (`IsFinished &&
  !IsActed`); "no execution" is the successful null process read of the same observation, so the adapter no longer makes a second
  mutable `ExecutionProcess` read whose result was discarded. The engine contract (finished, never acted, no execution) is
  unchanged and stays source-derived from the decompiled 2.1.7b assembly, not executed.
- **Unchanged:** `SubmitCellCommand` and its ownership-before-submission order, the deliberately unowned same-caster control
  (submitted directly), foreign commands, the three unrelated R6 passive callers, the executable late-condition sweep,
  attribution reconciliation, subscription ownership, bounded terminal handling and exact failure forwarding.
- **Focused behavior (domain, `sprint7-visual.effects-ownership-recovery`, through `SubmitCellCommand`, the real
  `Sprint7Abandonment` transfer and `ReleaseOwnedEffects`; only the native boundaries are scripted, each scripted process read
  counted so a case shows its adverse observation was made):** (A) after the failed interruption and transfer, a poll in which
  the residual reads null and the reconciliation then reads unreadable, and the reverse order: neither poll sweeps or
  unsubscribes; the exact execution is then recovered, delivers an attributable condition while live and ends; settlement
  removes exactly that condition, the foreign control and the separate same-caster command stay untouched, and the forwarded
  original failure is in the phase. (B) Within one poll the residual reads null and the reconciliation's read finds a new
  execution whose end is unreadable: it governs that decision at once; then the command finishes, never acted, with its field
  cleared while the retained execution is live: still owned (no cancellation inferred); its end settles both. (C) A transient
  process-read failure at cleanup (capture and census) registers the unreadable-command residual; at the next admission the
  readable null with established cancellation settles that residual and the reconciliation, the subscription is released, no
  residual remains and the next cell is admitted; the failure stays in the history. (D) A command whose process read keeps
  failing although it finished without acting, and one with an unproven cancellation, stay unresolved with their identities
  through polls; the next cell is refused; terminal accounting names both and the teardown reconciliation. **R5-F3:** the
  amendment-4 transferred-cleanup scenario now asserts, after physical cleanup has completed (nothing pending), that the
  enclosing phase's failures contain the exact forwarded interruption failure ("dust driver: unexpected child failure: s7-dust:
  owned command passive: …"), and that the check rejects a phase holding only the generic driver failure. Retained: the
  amendment-4 scenario, the cancellation control, the unreadable-terminal-state control, the throwing-getter handoff case and the
  earlier R2-F6/R3-F4 cases (the R3-F4 case's scripted end-state read now throws at the second read, the corrected census
  reading each command once; the earlier implementation read twice).
- **Native:** recovery candidate 6 was built from `962bf27694` (package `6c71d928…d55a`, DLL `6af754eb…1411`). Its build's
  outer duration exceeded the amendment-5 allowance and the session was not run then; C3 amendment 6 authorizes reusing that package
  for its mechanics session (see the amendment-6 paragraph and the C3 current state). The new uncertainty transitions and failures
  after transfer are focused evidence only unless that run actually shows them.

**C3 amendment 6 (owner, 2026-11-27): continue from the validation-budget stop with candidate 6.** Saved verbatim at
`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-6-2026-11-27.md` (message sha256
`d1a7c784400d4799051046b0be37e5cf35652b3a34e35473f92484d7390a0b05`). Remote heads re-verified unchanged first (#219
`0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 `a78dba163b`). Applied through `resume --amend-mission` (2026-11-27
13:18:52Z): **revision 6, digest `cd777793657fff1b2b8ff3e7320afa42449e9c25ccda70cef17bd10d64048656`**; ACTIVE in develop at
final 5/7 (the ceiling was already seven, so no ceiling operation was used). All eleven instruction parts were displayed raw,
one command each, in this session. The allowance becomes 360 minutes in total with 319 min 19.8 s charged (40 min 40.2 s
remaining); candidates stay six of six (no rebuild), formal reviews five of seven (review 7 conditional on unchanged runtime).
The owner's independent source review found no new blocking defect in the correction; it does not replace formal review 6.

Timing records check (amendment 6 §4; one bounded reading of the existing build log, the package directory and the
receipt; no rerun, instrumentation or host investigation). Established: the build log was created at 05:23:01.1Z (the outer
command's start stamp 05:23:01Z) and last written at 08:58:45.8Z; the receipt's `generatedAtUtc` is 08:58:45Z; the outer
invocation returned at 08:58:46Z, under 1 s after the last write. The build process itself therefore ran for essentially the whole
215 min 44.9 s; there was no measurable delay between its exit and the outer completion. The result was collected afterwards by
a background-task notification whose delivery time is not recorded. Intermediate file stamps bracket the interval:
`development-validation.json` (log line 22, after the early Python suites, including the 47-test packaging suite at 60.2 s
against 1.6 to 2.0 s in builds 2 to 5) was written at 05:37:44.2Z, 14 min 43.1 s in; the selected domain tests ran
08:56:00.5Z to 08:56:02.8Z (2.0 s inner); the DLL was written at 08:56:28.2Z and the package at 08:56:37.9Z. About 3 h
18 min fall between line 22 and the selected domain tests (offline gate, contract and fixture compilation steps); the log carries
no timestamps in that span. Unknown: which step or steps took that time, and why. The packaging suite's extra 58.2 s explains
only a small part; peer activity is not established as the cause. Charged baseline kept at 319 min 19.8 s (outer time); no
process-only duration different from the outer time was found, so nothing is reclaimed.

Peer coordination for the amendment-5/6 validation commands (dated from this session's cross-session message record; corrected
after review 6, R6-F1). Under the existing peer protocol other sessions on DATA may run Kingmaker copies; a game-copy session
needs an agreed no-Kingmaker window, and C3 amendment 5 also asks that build and gate periods be coordinated.
- Standing statement before the amendment-5 gates: at 02:26:52Z, on the release of the candidate-5 window, kingmakermountedcombat-0f
  wrote that its session was still offline-only and that it would ask first if it ever needed the game. The two local
  amendment-5 correction gates (FAIL 05:04:58Z–05:05:22Z, PASS 05:05:58Z–05:06:03Z) ran under that statement without a fresh
  exchange.
- Candidate-6 build: a roster check at 05:15:35Z listed two peers, dev-a6 (a shell session, not contacted) and
  kingmakermountedcombat-0f. Request 05:15:44Z (Build-Local about 13 min, then the session and gates within about 30 min); reply
  05:16:02Z "Free": no Kingmaker on DATA during the window, and over the next 30 minutes only short builds (about 30 s each),
  offline harness and component runs, focused gates and one background model review. The build ran 05:23:01Z–08:58:46Z. The
  arrangement covered roughly 05:16Z–05:46Z by its own terms; **for about 05:46Z–08:58:46Z no exchange took place and no peer
  status was checked, so coordination for that interval cannot be verified**, and what other work ran on DATA then is unknown.
  The window was released at 09:04:26Z.
- Candidate-6 session and final gates (H `e5b1e4af85`): request to kingmakermountedcombat-0f 13:20:21Z; reply 13:20:38Z: DATA is
  ours, no Kingmaker launch and no game or MSBuild work until released, with a CPU-only Python unit suite (stated as started
  "about 13:30Z" in a message received at 13:20:38Z, so its actual start is uncertain; about 16 minutes) running meanwhile. The
  session ran 13:30:19Z–13:36:36Z, the local gate 13:39:46Z–13:39:52Z and the controller gate batch 13:50:55Z–13:58:08Z; released
  13:58:21Z. **No roster check was made at 13:20Z** (the last was at 05:15:35Z). A roster check at 14:51Z lists a third session,
  kingmakerskipcinematic-1b, started about an hour earlier; it was not contacted for the 13:20Z–13:58Z window, so that window's
  coordination is verified for kingmakermountedcombat-0f only. Asked afterwards (reply received about 14:53Z), kingmakerskipcinematic-1b stated that it did not run Kingmaker between 13:30Z and 13:37Z; its first game launch that day was its guarded run 20261127T140338Z at 14:03:38Z, after this window had been released, using its own disposable copy with the owner's install and saves unchanged. That is the peer's after-the-fact account, not a contemporaneous check.
- Review-7 gates: roster check 14:51Z (dev-a6 idle, kingmakermountedcombat-0f idle, kingmakerskipcinematic-1b busy); requests
  to kingmakermountedcombat-0f and kingmakerskipcinematic-1b about 14:51Z; kingmakermountedcombat-0f replied "free" (idle, no
  Kingmaker, builds or gates during the window); kingmakerskipcinematic-1b replied "free" about 14:53Z (no Kingmaker or guarded run
  active, new launches held until released; it listed its runs that afternoon as 14:03Z to 14:55Z, the last ending about 14:55Z),
  and the gates were started after 14:55Z.
- Review-8 gates (C3 amendment 7): at 17:38:39Z kingmakerskipcinematic-1b held its runtime lock (run started 17:38:12Z, Kingmaker
  running), and the controller's first amendment-7 admission was refused for that lock with nothing captured. Roster check about
  17:39Z (dev-a6 shell, kingmakermountedcombat-0f idle, kingmakerskipcinematic-1b busy); requests to both about 17:39Z for about
  10 minutes after that run restored (controller resume and the four gates; no Kingmaker launch). kingmakermountedcombat-0f replied
  "free" about 17:39Z; the lock was gone and no Kingmaker process ran at 17:39:48Z; kingmakerskipcinematic-1b replied "free"
  about 17:40Z (run finished and restored; it holds runtime locks, launches and builds until released). The release follows the
  gates and is recorded in the review-8 developer report, not here, so the gate head stays unchanged.

**C3 native qualification of the amendment-5 bytes (recovery candidate 6 of 6, under C3 amendment 6).** Built once from records
head `962bf27694` (compiled source identical to code commit `360ce86f54`; the records commit changes only `docs/`): package
`6c71d928ea4293945af43a4b33f81238047176b07ed10fc88edee342649cd55a`, DLL
`6af754eb06f72a261916d2dcd6e5fea125b62b13dcc9eeae53ad000e94341411` (Build-Local `-DevelopmentPurpose s7-recovery-c3`, 25 selected
tests), reused without a rebuild: the package hash re-read before the run matched the receipt and its `.sha256`, and the session
read back the staged package and DLL hashes. Harness C2's `d77754ba…b4f1` (unchanged; staged DLL hash read back), the same admitted
disposable outdoor native Default fixture, loader profile `owner-0.33.0`, selection `summon-s7-effects-mechanics` (session file
`s7-c3-mechanics.json`, unchanged, seven required cells declared before the run). Session `20261127T133020Z-owner-0.33.0-s7-c3-mechanics` (run from
records head `2ea63edb22`, the build source and later records kept distinct; window 13:30Z–13:37Z, granted by the other session on
DATA, whose CPU-only Python suite was running meanwhile): PASS, 43/43 checks.
- Required cells: all seven (`s7-ankylosaurus`, `s7-callback-abandoned`, `s7-callback-setup-failed`, `s7-trace-setup-failed`,
  `s7-dust`, `s7-dust-pre-delivery-disposed`, `s7-dust-abandoned`) admitted and completed (`requiredCellsCompleted` true); separately,
  physical restoration true: no failure, pending, unreadable or unreported entry; one recovery (the ordinary Dust cell's
  reconciliation observed settled); `restoreErrors` empty.
- Settlement observations (new in these bytes): the ordinary Dust release recorded one observation per owned command in its
  settlement evaluation, fifteen in all: fourteen `execution ended` and one `cancelled before execution` (a readable null process on
  a command that had finished without acting). That cancellation branch of the adapter therefore ran natively once, and the cell
  settled at cleanup with nothing remaining, 0 live, the late sweep completed and the subscription released. This executes the
  classification on a real engine command; the engine contract that such a command cannot start later remains source-derived (the
  run shows no later execution from it, which is consistent with it but does not prove it). No unreadable observation occurred.
- Pending commands and transferred residuals: `pendingCommands` is exported, and empty, for the ordinary Dust release
  (`dust.ownedEffects`) and the pre-delivery driver's release (`dustPreDeliveryDisposed.dust.ownedEffects`). The abandoned driver's row
  exports no `pendingCommands` field (corrected after review 6, R6-F2; the earlier wording listed it); its execution census
  (`dustAbandoned.dust`) shows one owned execution, `KMG_Summoning_Special_DustMephit_Breath#-1747779200`, live at cleanup
  (`ownedExecutionsLive` 1, `liveExecutionsAtCleanup` 1, `executionsSettledBeforeReuse` true), `ownedCommandsEnded` 1,
  `ownedCommandsLeft` 0 and no owned-command failure. Its child residuals are the two entries of
  `dustAbandoned.childAfterDisposal.pending`, `s7-dust: owned execution KMG_Summoning_Special_DustMephit_Breath#-1747779200` and
  `s7-dust: owned conditions reconciled after settlement`; both settled, as `dustAbandoned.childAfterResume.recoveries` records
  (`… observed settled` for each), with `childAfterResume.pending` empty (stop-commit correction after review 7, not formally reviewed). Separately,
  `dustAbandoned.expected` is the injected-failure classification (the injected Sickened removal failure and the one owned condition
  it left), with `unexpected`, `unsafe` and `expectedMissing` empty and the injected leftover gone after repair (`leftoverGone`
  true, no repair failure). The five drivers (callback-abandoned, callback-setup-failed, trace-setup-failed, dust-pre-delivery-disposed, dust-abandoned) are
  all proven with empty `transferredResiduals`. So the native run did not reach the pending, null-to-unreadable, null-to-live or
  transient-failure-then-cancellation transitions, or failures after a handoff; those remain focused evidence (cases A–D, R5-F3).
- Command ownership: every cell-owned Dust command goes through `SubmitCellCommand` in these bytes; the deliberately unowned
  same-caster control cast is submitted directly and its row records its Sickened applied and outside the ledger
  (`unownedExecution`: `ownedByLedger` false); no owned command failure, none left running (`ownedCommandsLeft` 0).
- Ordinary disposal before delivery: disposed in readiness frame 11091 with execution
  `KMG_Summoning_Special_DustMephit_Breath#-473504512` started and no condition from it present (known 0) at disposal; at settlement
  (frame 11470) that execution had ended and delivered one Sickened (acquired 1), released (present from it a known 0); the ledger
  was held until settlement, the late sweep completed, the subscription was released, the foreign Sickened control intact; the next
  cell was admitted and completed.
- Command cleanup: `commandCleanupPerformed` lists 18 permitted-and-returned callbacks (`LeaveCombat` itself runs only when
  `IsInCombat`); `commandCleanupWithheld` empty.
- Callback loss (constructed setup, labelled as such): declared same-frame delivery posted real cues from the production handler,
  both playing (IDs 537 survivor, 538 other, frame 5312); C1's evaluator judged the row live: verdict null. The committed persisted
  row `docs/evidence/sprint7/recovery-c3/20261127T133020Z-callback-loss-row.json` (source sha256 `ab456698…de2d`) and its
  collector-shaped counterpart receive the same verdict (`sprint7-visual.combined-run-verdicts-agree`, local focused gate PASS).
- Retirements: 19 requests through the phase's one record (8 by the phase's fallback at teardown), none failed.
- Restoration (receipts in the session summary): transaction `0771c4fc32b24273900c38ae36205ec7` succeeded; owner integrity verified,
  owner saves and owner install unchanged; game settings applied; registry restored (readback verified, sha256
  `5ffd5922…6b38`); runtime lease `runtime-20261127T133020Z-d8682d2c…`. Mechanics-only: no frame captures,
  native-look comparison or listening samples.
Candidate 5's `20261127T022022Z` PASS stays at its own scope (source `511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`);
it does not qualify these bytes.

**C3 current state (pre-review-8 snapshot, dated 2026-11-27 17:41Z, under C3 amendment 7; this block and the round-6 and round-7 tables below are the
current records).** Controller ACTIVE in develop at final 7/8: owner amendment 7 was admitted at 17:39:57Z as amendments revision 7
(sha256 `af773cd7996f9445ffd9207c112ccb14be96c5497f5203052cea0ae94c3b7982`), and a separate `resume --max-rounds 8` raised the
review ceiling from seven to eight for formal review 8 of the records-only stop-commit corrections. Recovery candidate 6 built (PASS) from records head `962bf27694` (code
`360ce86f54`): package `6c71d928ea4293945af43a4b33f81238047176b07ed10fc88edee342649cd55a`, DLL
`6af754eb06f72a261916d2dcd6e5fea125b62b13dcc9eeae53ad000e94341411`, 25 selected tests; its build exceeded the amendment-5 allowance
(the dated stop at `a78dba163b` is history), and amendment 6 authorized reusing it. Its mechanics session `20261127T133020Z`
passed (43/43, seven required cells admitted and completed, physical restoration separately true; see the qualification paragraph
above); formal review 6 (H `e5b1e4af85`, all four controller gates PASS) verified R5-F1, R5-F2, R5-F3, R1-F13 and R5-F4
and opened two records findings, R6-F1 (P2, peer coordination documented only for the game session) and R6-F2 (P3, cite the
abandoned driver's census); both were corrected in records for the conditional review 7. Formal review
7 (records-only H `74c3d43288`, all four controller gates PASS) verified R6-F1, rejected the optional R6-F2 citation and opened
R7-F1 (P2 records, blocks readiness: the integration row and reconciliation paragraph still presented the pre-review-6 state);
the controller then stopped BLOCKED at the 7/7 cap (history). Both were corrected in records in the stop commit `db2a90a3fd`
(stop-commit correction after review 7, not formally reviewed). **Pending formal review 8:** R7-F1 (readiness-blocking records
finding) and R6-F2 (optional, nonblocking); both stay formally open until the controller records their disposition. Formal reviews:
seven completed of eight authorized (reviewed H `7fff8b34f6`, `89d4efa347`, `f0ca78cbab`, `43dd44a2ba`, `ed02905b7f`,
`e5b1e4af85`, `74c3d43288`); the latest completed review is round 7 on `74c3d43288`. The
qualified package, harness and native evidence are unchanged; no archive A and no handoff. Recovery candidates:
six spent of six. Validation: see the ledger. Settlement correction: code
`360ce86f54` (focused gate PASS, 25 tests). No C3 archive A and no handoff. Source, reviewed H, records head and archive A stay
distinct identities. Candidate 5 and its PASS stay at their scope: build source `511db044b1`, package `15307748…638e`, DLL
`1dde22e0…ba7e`, harness `d77754ba…b4f1`, session `20261127T022022Z` (43/43, seven required cells admitted and completed,
physical restoration separately true), run from records head `c1329dd21d`; it had no native pending commands, empty
`transferredResiduals` in its five drivers, and its late-delivery check began with an observed execution, so it does not natively
qualify the pre-start uncertainty transitions or failures after transfer.

Round-5 finding map (history since review 6, which verified all five rows below; kept for their dispositions; one-off extraction of the accepted structured round-5 result, identical in the published review and
the controller's `result-1.json`, request `…-r05-e19c0d`, H `ed02905b7f`;
`docs/evidence/sprint7/recovery-c3/round5-acceptance-ids-crosscheck.json`). Review 5's verifications of C3/R4-F1, C3/R2-F7,
C3/R4-F2 and C3/R4-F3 stand.

| C3 finding (round 5) | Bindings (`acceptance_ids`) | Correction and evidence (formal review 6 verified all five on H `e5b1e4af85`) |
|---|---|---|
| R5-F1 (P1 defect) | #219 R7-F2; C3 §5; amendment 4 §2A | Code `360ce86f54`: one observation per command per settlement evaluation (each consumer accounts for its own observations; no phase-wide snapshot), one decision over the complete set; focused (A), (B), (D); native: candidate 6 regression PASS (`20261127T133020Z`; settlement observations ended/cancelled only, no pending or transferred residual reached natively) |
| R5-F2 (P2 defect) | #219 R7-F2; C3 §5; amendment 4 §§2A,2C | Code `360ce86f54`: a readable null replaces earlier uncertainty in the census, both command residuals and the reconciliation; focused (C), (D); native: candidate 6 regression PASS (`20261127T133020Z`; the cancellation branch ran once natively; the null-after-uncertainty transition is focused only) |
| R5-F3 (P2 evidence gap) | C3 §5; amendment 4 §2C | Domain test at `360ce86f54`: the exact forwarded interruption failure in the phase after physical cleanup; rejects a generic-only phase |
| R1-F13 (P2 records) | C3 §8; amendment 2 §4; amendment 3 §4; amendment 4 §3 | Stop-commit corrections kept (C3/R4-F1 binds #219 R7-F2; candidate 4 ran `6cef765a8a`); this current-state block and map; the round-4 map and pre-review-5 status prose marked history |
| R5-F4 (P2 evidence gap) | C3 §6; amendment 3 §4; amendment 4 §§2B,4 | Stop-commit scoping kept: `SubmitCellCommand` coverage is for cell-owned Dust commands; the separate same-caster control submits directly (`unownedExecution`) |

Round-6 findings (review 6, H `e5b1e4af85`; records only, corrected for the conditional review 7 with runtime, harness and package
unchanged; review 7 verified R6-F1 and rejected R6-F2, corrected again in the stop commit):

| C3 finding (round 6) | Requested | Correction (records) |
|---|---|---|
| R6-F1 (P2 evidence gap, blocks readiness) | A dated account of the peer coordination for the candidate-6 build and final gates, identifying any interval that cannot be verified | Peer-coordination paragraph under C3 amendment 6: dated exchanges for every amendment-5/6 validation command; the build's coordination is unverifiable for about 05:46Z–08:58:46Z; the 13:20Z–13:58Z window is verified for kingmakermountedcombat-0f only (no roster check then; kingmakerskipcinematic-1b's after-the-fact account recorded) |
| R6-F2 (P3 optional) | Cite the abandoned driver's exported census for its pending-command result | Candidate-6 qualification paragraph: the abandoned driver exports no `pendingCommands`; its execution census cited; after review 7 its residuals cited from `childAfterDisposal.pending` and their settlement from `childAfterResume.recoveries` (stop-commit correction after review 7, not formally reviewed) |

Round-7 finding (review 7, H `74c3d43288`; the open findings, pending formal review 8, are R6-F2 and R7-F1):

| C3 finding (round 7) | Requested | Correction (records) |
|---|---|---|
| R7-F1 (P2 required gap, blocks readiness) | Use the current-state block as the reference for the C3 integration row and reconciliation paragraph; date the round-5 table as historical, keeping its verified dispositions | Integration row, dependency paragraph and finding-reconciliation paragraph now refer to the C3 current state (BLOCKED 7/7 at the stop commit; now the pre-review-8 snapshot); the round-5 map is labelled history (stop-commit correction after review 7, not formally reviewed) |

(History, dated 2026-11-27 before C3 amendment 5; superseded by the round-5 pending-finding map in the C3 current state below.)
Pending-finding map after formal review 4 (one-off extraction of the accepted structured round-4 result, identical in the
published review and the controller's `result-1.json`, request `…-r04-69cc0c`, H `43dd44a2ba`;
`docs/evidence/sprint7/recovery-c3/round4-acceptance-ids-crosscheck.json`). Audit labels are evidence identifiers, not controller IDs.
Formal review 5 verified C3/R4-F1, C3/R2-F7, C3/R4-F2 and C3/R4-F3 and kept C3/R1-F13 open.

| C3 finding (round 4) | Bindings (`acceptance_ids`) | Disposition (at formal review 5) |
|---|---|---|
| C3/R4-F1 (P1 defect) | #219 R7-F2; C3 §5; amendment 1 §3B; amendment 3 §3E | Code `9d99563ec7` + focused scenario and controls (above); candidate 5 (`20261127T022022Z`, PASS) ran the corrected bytes (the null-before-start branch itself not observed natively); formal review 5 verified it; its new findings R5-F1/R5-F2 are adjacent settlement defects (stop record below) |
| C3/R2-F7 (P1 evidence gap) | #219 R4-F1, #219 R7-F2; amendment 2 §§3C,3F; amendment 3 §3E | Code `9d99563ec7` (production submission) + focused case and mutation control; adapter by source inspection and candidate 5's native run (`20261127T022022Z`, every cell-owned Dust command submitted through it; the unowned same-caster control submitted directly); formal review 5 verified it |
| C3/R4-F2 (P2 records) | C3 §6; amendment 3 §§3F,4 | Records corrected at `3890a5bfad` and retained: ten combat-exit callbacks permitted and returned, `LeaveCombat` conditional on `IsInCombat`; withholding list empty; foreign-queue result focused only |
| C3/R1-F13 (P2 records) | C3 §8; amendment 1 §§3F,7; amendment 2 §4; amendment 3 §4 | Records reconciled under amendment 4: the finding-reconciliation paragraph and the mission-only footer are marked historical with one current status; this map is current |
| C3/R4-F3 (P2 records) | C3 §7; amendment 1 §6; amendment 3 §6 | Disposition accepted by the owner (amendment 4 §3): the historical candidate-4 pre-build estimate exists only in the operator session's progress messages and is unverifiable from the committed record; no separate pre-session estimate was made. Prospective estimates for candidate 5 are recorded in the ledger before execution |

Formal round 4's verifications stay: R3-F1 to R3-F5, R2-F8, R1-F9 and R1-F11 (not reopened; the correction touches the pending
branch, the submission and their focused cases only).

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
| #223 C3 | `codex/s7-recovery-c3-effects-ownership` | `7fff8b34f6` (round 1), `89d4efa347` (round 2), `f0ca78cbab` (round 3), `43dd44a2ba` (round 4), `ed02905b7f` (round 5), `e5b1e4af85` (round 6), `74c3d43288` (round 7) | none | ACTIVE/develop at 7/8 under C3 amendment 7 (pre-review-8 snapshot; see the C3 current state); review 6 (`e5b1e4af85`) verified R5-F1 to R5-F4 and R1-F13; review 7 (`74c3d43288`) verified R6-F1 and left R6-F2 and R7-F1 open (records; corrected in the stop commit `db2a90a3fd`, pending formal review 8); settlement correction `360ce86f54` | `6abd692fa7` (candidate `de63d6f4…4b46`, `20261126T054228Z`); `66b0ff3f14` (recovery candidate 3, `473f2f5a…8366`, `20261126T134425Z`); `6cef765a8a` (recovery candidate 4, `242f9a27…0d93`, `20261126T232851Z`); `511db044b1` (recovery candidate 5, `15307748…638e`, `20261127T022022Z`); `962bf27694` (recovery candidate 6, `6c71d928…d55a`, `20261127T133020Z`) |

Dependency checks (2026-11-26): `0542b0592a` is an ancestor of `2c67a049ee`, which is an ancestor of `eab90a5ac8`, which is
an ancestor of `7fff8b34f6`; every remote branch head matched the identities above when checked (2026-11-26, before C3
amendment 1). C1's and C2's current heads differ from their reviewed H only by the controller's archive records. C3's head at
that check (`254c08c53d`) differed from its round-1 H by records only. (History, dated: under C3 amendment 1 the compiled
code changed in `82dc667c93` and `89d4efa347`, reviewed in round 2; the round-2 corrections were built as recovery candidate 3
from `66b0ff3f14` and qualified by `20261126T134425Z`; round 3 reviewed `f0ca78cbab`.) (History, dated C3 amendment 2,
superseded: the remote heads were re-verified unchanged at that amendment (#219 `0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`,
C3 records head `80d224bd56`); the amendment-2 corrections were then to be built as recovery candidate 4 and qualified by one
mechanics session before formal review 4.) (History, dated 2026-11-27 after formal review 4, superseded by C3 amendment 4: the
heads were re-verified unchanged at C3 amendment 3 (#219 `0542b0592a`, C1 `2c67a049ee`, C2 `eab90a5ac8`, C3 `38f49b22b3`);
recovery candidate 4 was built from `6cef765a8a` (= code `d9a07ef301` plus records) and qualified by `20261126T232851Z`; formal
review 4 on H `43dd44a2ba` returned changes_required and the controller stopped BLOCKED at the 4/4 review cap.) (History,
dated 2026-11-27 under C3 amendment 4, superseded: the heads were re-verified unchanged at that amendment (#219 `0542b0592a`, C1
`2c67a049ee`, C2 `eab90a5ac8`, C3 records head `3890a5bfad`); the C3/R4-F1 and C3/R2-F7 correction was code `9d99563ec7`, built
once as recovery candidate 5 from `511db044b1` and qualified by `20261127T022022Z`; formal review 5 on H `ed02905b7f` returned
changes_required with two compiled-code defects (R5-F1, R5-F2) and the work stopped at the amendment-4 limit.) Current
(2026-11-27, C3 amendment 5): the heads were re-verified unchanged at the amendment (#219 `0542b0592a`, C1 `2c67a049ee`, C2
`eab90a5ac8`, C3 `bfeec8d8a9`); the settlement correction is code `360ce86f54`. Current (2026-11-27, C3 amendment 6): heads
re-verified unchanged (C3 `a78dba163b`); recovery candidate 6 (`962bf27694`) was qualified by `20261127T133020Z`; reviews 6 and 7
followed; review 7 stopped C3 at the 7/7 cap, and owner amendment 7 resumed it at 7/8 for formal review 8 (C3 current state,
pre-review-8 snapshot). C3
has no archive A and no handoff, so the integration operation below cannot start for C3 until a handoff exists. Source, reviewed
H, current records head and archive A stay separate identities. The stack is linear on the frozen parent, so stacking the children onto #219 needs no conflict
resolution while #219 stays frozen; that says nothing about integration with `master`, which has moved independently and may
conflict.

Finding reconciliation (current, pre-review-8 snapshot, dated 2026-11-27 17:41Z, under C3 amendment 7): review 5 verified C3/R4-F1, C3/R2-F7, C3/R4-F2
and C3/R4-F3; review 6 verified R5-F1 to R5-F4 and R1-F13 (round-5 map, now history); review 7 verified R6-F1; R6-F2 and R7-F1
remain open, corrected in records in the stop commit and pending formal review 8 (C3 current state, above). (History, dated before formal review 5: formal review 4 verified R3-F1 to R3-F5, R2-F8, R1-F9 and
R1-F11; the round-4 pending findings were then corrected in code `9d99563ec7` and in records and qualified by recovery candidate 5,
for review 5.) Parent bindings: #219 R7-F1, R7-F6 and R6-F5 (C1, owner-accepted), R5-F1 (C2, owner-accepted),
#219 R4-F1 and R7-F2 to R7-F5 (C3, in this enrollment).

Finding reconciliation (history, dated 2026-11-26 under C3 amendments 2–3, superseded by the current paragraph above): parent R7-F1, R7-F6 (C1, owner-accepted; its same-run
native criterion fulfilled and accepted at `20261126T054228Z` on `6abd692fa7`), R6-F5 (C1 records), R5-F1 (C2, owner-accepted
at harness `d77754ba…b4f1` / `20261126T033419Z`), R4-F1 and R7-F2 to R7-F5 (C3: the round-2 corrections qualified natively by
recovery candidate 3 at `20261126T134425Z`, at its stated scope; the amendment-2 corrections of C3 review findings R3-F1 to
R3-F4, R2-F7, R2-F8, R1-F9 and R1-F11, and the amendment-3 corrections of the audit evidence A1-F1, A1-F2, A1-F4 to A1-F6 and
A2-F1 to A2-F3, are in source and focused tests, qualified natively at their stated scope by recovery candidate 4
(`20261126T232851Z`), and await formal review 4;
R1-F13 and R3-F5 are records). (History: at the review-3 no-progress stop these findings were open.) (History: the earlier C3 stop at round 1, before C3 amendment 1, had R1-F1 to R1-F11 open; the round-1
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
| R4-F1 independent release failures past the abandonment drivers | C3 `codex/s7-recovery-c3-effects-ownership` (PR #223) | Complete child outcome in every driver (`Sprint7Abandonment`); unexpected child failures reach the phase by name and survive the repair | Domain `sprint7-visual.effects-ownership-recovery` (paired injected fault plus independent command-release failure through the phase sequence); native `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7`: all four drivers proven, none unexpected | C2 | Current (2026-11-26, C3 amendment 3): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); round-3 findings bound here (`acceptance_ids` extracted from the published structured review, identical in the controller result): R2-F7, R2-F8, R1-F11; corrected in source and focused tests (amendment 2 at `03a78d4987`, amendment 3 at `d9a07ef301`; audit evidence A1-F*/A2-F* mapped in the C3 records), qualified natively at its stated scope by recovery candidate 4 (`6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`) in `20261126T232851Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`; nothing withheld); formal review 4 (H `43dd44a2ba`): changes_required with C3/R4-F1, R2-F7, R4-F2, R4-F3 and R1-F13 open; under C3 amendment 4 (revision 4; the enrollment was then ACTIVE at 4/6) C3/R4-F1 and C3/R2-F7 are corrected in code `9d99563ec7` with focused evidence and the records findings are resolved; qualified at its stated scope by recovery candidate 5 (`511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`) in `20261127T022022Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; `pendingCommands` empty, so the null-before-start branch is focused evidence only), formal review 5 (H `ed02905b7f`) verified the C3/R4-F1 and C3/R2-F7 corrections and opened R5-F1 to R5-F4 (two compiled-code settlement defects), R1-F13 still open; the amendment-5 correction and its qualification and review: the C3 current state (no archive, no handoff). (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F2 ordinary Dust executions interrupted before retention | C3 (PR #223) | `Sprint7OwnedCommands`: process retained before interruption; commands released independently; ledger owns executions by retained process | Domain (failing interruption does not skip the next; cleared field keeps the process); native pre-delivery release in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` (process retained, ended, none live) | C2 | Current (2026-11-26, C3 amendment 3): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); round-3 findings bound here (`acceptance_ids` extracted from the published structured review, identical in the controller result): R3-F1, R3-F2, R2-F7, R3-F3, R3-F4, R1-F11; corrected in source and focused tests (amendment 2 at `03a78d4987`, amendment 3 at `d9a07ef301`; audit evidence A1-F*/A2-F* mapped in the C3 records), qualified natively at its stated scope by recovery candidate 4 (`6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`) in `20261126T232851Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`; nothing withheld); formal review 4 (H `43dd44a2ba`): changes_required with C3/R4-F1, R2-F7, R4-F2, R4-F3 and R1-F13 open; under C3 amendment 4 (revision 4; the enrollment was then ACTIVE at 4/6) C3/R4-F1 and C3/R2-F7 are corrected in code `9d99563ec7` with focused evidence and the records findings are resolved; qualified at its stated scope by recovery candidate 5 (`511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`) in `20261127T022022Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; `pendingCommands` empty, so the null-before-start branch is focused evidence only), formal review 5 (H `ed02905b7f`) verified the C3/R4-F1 and C3/R2-F7 corrections and opened R5-F1 to R5-F4 (two compiled-code settlement defects), R1-F13 still open; the amendment-5 correction and its qualification and review: the C3 current state (no archive, no handoff). (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F3 incomplete hook setup rollback | C3 (PR #223) | `Sprint7HookAttempt` registered before set-up, readback by identity; `IsolatedCell` sticky cell exceptions; caller-chosen trace identity | Domain (failed rollback, null result, unreadable readback, sticky exception); native callback and trace set-up-failed controls in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` (set-up and rollback errors, readback 1 and 2 hooks, next cell refused, repaired to 0) | C2 | Current (2026-11-26, C3 amendment 3): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); no outstanding round-3 binding (`acceptance_ids` extracted from the published structured review, identical in the controller result); unchanged since candidate 3, qualified natively at its stated scope by recovery candidate 4 (`6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`) in `20261126T232851Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`; nothing withheld); formal review 4 (H `43dd44a2ba`): changes_required with C3/R4-F1, R2-F7, R4-F2, R4-F3 and R1-F13 open; under C3 amendment 4 (revision 4; the enrollment was then ACTIVE at 4/6) C3/R4-F1 and C3/R2-F7 are corrected in code `9d99563ec7` with focused evidence and the records findings are resolved; qualified at its stated scope by recovery candidate 5 (`511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`) in `20261127T022022Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; `pendingCommands` empty, so the null-before-start branch is focused evidence only), formal review 5 (H `ed02905b7f`) verified the C3/R4-F1 and C3/R2-F7 corrections and opened R5-F1 to R5-F4 (two compiled-code settlement defects), R1-F13 still open; the amendment-5 correction and its qualification and review: the C3 current state (no archive, no handoff). (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F4 failed direct cue-source retirement | C3 (PR #223) | `Sprint7OwnedRequests`: one request per source, failure named and sticky, unsettled source a residual | Domain (throwing retirement through the phase sequence); native retirements in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` | C2 | Current (2026-11-26, C3 amendment 3): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); round-3 findings bound here (`acceptance_ids` extracted from the published structured review, identical in the controller result): R3-F3, R1-F11; corrected in source and focused tests (amendment 2 at `03a78d4987`, amendment 3 at `d9a07ef301`; audit evidence A1-F*/A2-F* mapped in the C3 records), qualified natively at its stated scope by recovery candidate 4 (`6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`) in `20261126T232851Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`; nothing withheld); formal review 4 (H `43dd44a2ba`): changes_required with C3/R4-F1, R2-F7, R4-F2, R4-F3 and R1-F13 open; under C3 amendment 4 (revision 4; the enrollment was then ACTIVE at 4/6) C3/R4-F1 and C3/R2-F7 are corrected in code `9d99563ec7` with focused evidence and the records findings are resolved; qualified at its stated scope by recovery candidate 5 (`511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`) in `20261127T022022Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; `pendingCommands` empty, so the null-before-start branch is focused evidence only), formal review 5 (H `ed02905b7f`) verified the C3/R4-F1 and C3/R2-F7 corrections and opened R5-F1 to R5-F4 (two compiled-code settlement defects), R1-F13 still open; the amendment-5 correction and its qualification and review: the C3 current state (no archive, no handoff). (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |
| R7-F5 Dust driver disposed during recovery | C3 (PR #223) | Repair of the exact injected condition once on every exit, including disposal in the recovery wait; failed repair kept | Domain (disposal in the recovery wait, successful and failed repair, foreign kept); native Dust driver in `20261126T054228Z-…-s7-c3-fx` PASS (41/41 checks) on candidate `de63d6f4…4b46` / DLL `ef66c870…b57a` from `6abd692fa7` | C2 | Current (2026-11-26, C3 amendment 3): qualified at its stated scope by recovery candidate 3 (`66b0ff3f14`, package `473f2f5a…8366`, DLL `8e574477…7d07`, harness `d77754ba…b4f1`) in `20261126T134425Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`, so no native transfer-failure claim); round-3 findings bound here (`acceptance_ids` extracted from the published structured review, identical in the controller result): R3-F1, R3-F4; corrected in source and focused tests (amendment 2 at `03a78d4987`, amendment 3 at `d9a07ef301`; audit evidence A1-F*/A2-F* mapped in the C3 records), qualified natively at its stated scope by recovery candidate 4 (`6cef765a8a`, package `242f9a27…0d93`, DLL `a08d06b6…91c2`) in `20261126T232851Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; driver rows with empty `transferredResiduals`; nothing withheld); formal review 4 (H `43dd44a2ba`): changes_required with C3/R4-F1, R2-F7, R4-F2, R4-F3 and R1-F13 open; under C3 amendment 4 (revision 4; the enrollment was then ACTIVE at 4/6) C3/R4-F1 and C3/R2-F7 are corrected in code `9d99563ec7` with focused evidence and the records findings are resolved; qualified at its stated scope by recovery candidate 5 (`511db044b1`, package `15307748…638e`, DLL `1dde22e0…ba7e`) in `20261127T022022Z-…-s7-c3-mechanics` (43 checks; seven required cells completed; physical restoration; `pendingCommands` empty, so the null-before-start branch is focused evidence only), formal review 5 (H `ed02905b7f`) verified the C3/R4-F1 and C3/R2-F7 corrections and opened R5-F1 to R5-F4 (two compiled-code settlement defects), R1-F13 still open; the amendment-5 correction and its qualification and review: the C3 current state (no archive, no handoff). (History: the earlier stops had R1-F1 to R1-F11, then R3-F1 to R3-F5 with R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13, open.) |

C3 findings bound only to C3 mission sections (round-3 `acceptance_ids`, extracted and cross-checked as above): R1-F9 → C3 §5
(guarded phase set-up), R1-F13 → C3 §8 (these records), R3-F5 → C3 §§6–8 (ledger arithmetic). (Dated, withdrawn: an earlier
version of this paragraph bound R3-F4 to §5, R1-F11 to §8 and R1-F13 to §§6–8 by hand transcription.) All are in
source/tests/records at the current head; recovery candidate 4's session `20261126T232851Z` ran source `6cef765a8a` (before the
compiled amendment-4 correction) and recovery candidate 5's session `20261127T022022Z` ran `511db044b1` (= code `9d99563ec7` plus
records; corrected after review 5, R1-F13); formal review 4
verified R1-F9 and R3-F5 and kept R1-F13 open; formal review 5 kept it open (two statements, corrected in the stop commit) and C3
amendment 5 completes it (current round-5 pending-finding map, C3 current state); the
`20261126T134425Z` PASS stays bound to candidate 3 (source `66b0ff3f14`), distinct from reviewed head `f0ca78cbab`, the records
commits, the correction commits (`a84d0b0d07`, `03a78d4987`, `d9a07ef301`) and the eventual archive A.

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
| C3 r4 | (stop at the pre-build audit gate: no build, session or controller gates run; model-review time outside the clock) | | | 60 min 46.5 s | 29 min 13.5 s unused |
| C3 r4 | Focused gate `sprint7-visual` (local, amendment-3 corrections) | FAIL, 24/25: the earlier R2-F2 terminal case still asserted the old terminal reading (obligation always "not settled at teardown"); assertion updated to §3B | 21.7 s | 61 min 8.2 s | 28 min 51.8 s |
| C3 r4 | Focused gate `sprint7-visual` (local) | FAIL, 24/25: rerun on the unchanged test (the correcting edit had been refused as stale and was not applied) | 2.3 s | 61 min 10.5 s | 28 min 49.5 s |
| C3 r4 | Focused gate `sprint7-visual` (local, = code `d9a07ef301`) | PASS, 25 tests | 5.6 s | 61 min 16.1 s | 28 min 43.9 s |
| C3 r4 | Build-Local `-DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual` on `6cef765a8a` (recovery candidate 4 of 4: package `242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93`, DLL `a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2`; 25 selected tests) | PASS | 8 min 8.9 s | 69 min 25.0 s | 20 min 35.0 s |
| C3 r4 | Session `20261126T232851Z-…-s7-c3-mechanics` (recovery candidate 4, C2 harness `d77754ba…b4f1`) | PASS, 43/43 checks; required cells completed; restoration completed, owner files and saves verified, registry restored | 5 min 59 s | 75 min 24.0 s | 14 min 36.0 s |
| C3 r4 | Focused gate `sprint7-visual` (local, with the candidate-4 same-run agreement) | PASS, 25 tests | 4.9 s | 75 min 28.9 s | 14 min 31.1 s |
| C3 r4 | Controller focused gates on H `43dd44a2ba` (`sprint7-visual`, `summon-playtest-r6`, `native-profile`, `loader-compat-probe`; receipts 3.8 + 1.9 + 2.1 + 2.1 = 9.9 s) | PASS, 4/4 | 18.5 s (batch) | 75 min 47.4 s | 14 min 12.6 s |
| C3 r4 | `review` submission (23:39Z) | refused by the controller (revision 3 not verifiably displayed); no validation launched | — | 75 min 47.4 s | 14 min 12.6 s |
| C3 r4 | Formal review 4 (model review, outside the clock) | changes_required; controller BLOCKED at the 4/4 cap | — | 75 min 47.4 s | 14 min 12.6 s unused |
| C3 r5 | (C3 amendment 4: allowance raised from 90 to 105 minutes total; 75 min 47.4 s spent) | | | 75 min 47.4 s | 29 min 12.6 s of 105 |
| C3 r5 | Focused gate `sprint7-visual` (local, amendment-4 corrections) | FAIL, 24/25: the new unreadable-state control read the isolation's unreadable list before any settlement pass had populated it; a settle read was added before the assertion | 23.0 s | 76 min 10.4 s | 28 min 49.6 s |
| C3 r5 | Focused gate `sprint7-visual` (local, = code `9d99563ec7`) | PASS, 25 tests (production compilation passed) | 5.0 s | 76 min 15.4 s | 28 min 44.6 s |
| C3 r5 | Mutation control: focused gate with the phase registration removed from `SubmitCellCommand` (reverted byte-exact) | FAIL as intended: the existing handoff case's registration assertion | 23.2 s | 76 min 38.6 s | 28 min 21.4 s |
| C3 r5 | Mutation control: focused gate with the pending state disabled (reverted byte-exact) | FAIL as intended: scenario step (2) | 23.8 s | 77 min 2.4 s | 27 min 57.6 s |
| C3 r5 | Prospective estimate before recovery candidate 5 (recorded here before the build): Build-Local about 8 min 10 s (candidate 4: 8 min 8.9 s), mechanics session about 6 min (5 min 59 s), focused gate after the evidence about 10 s, controller gate batch about 20 s (18.5 s): about 14 min 40 s together, leaving about 13 min 15 s of margin | estimate | — | 77 min 2.4 s | 27 min 57.6 s |
| C3 r5 | Build-Local `-DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual` on `511db044b1` (recovery candidate 5 of 5: package `15307748f65c4dee26e08239f35ff9c2a543855bfb5ea22716c39fcf62ea638e`, DLL `1dde22e025548941acf107e6e1866f43c61629cc303019e63c7fd7cbb88aba7e`; 25 selected tests; window agreed with both other sessions, which ran offline builds meanwhile) | PASS | 12 min 40.7 s | 89 min 43.1 s | 15 min 16.9 s |
| C3 r5 | Prospective re-estimate before the candidate-5 session (recorded here before it runs): mechanics session about 6 min (5 min 59 s), focused gate after the evidence about 10 s, controller gate batch about 20 s: about 6 min 30 s together, leaving about 8 min 45 s of margin | estimate | — | 89 min 43.1 s | 15 min 16.9 s |
| C3 r5 | Session `20261127T022022Z-…-s7-c3-mechanics` (recovery candidate 5, C2 harness `d77754ba…b4f1`) | PASS, 43/43 checks; required cells completed; restoration completed, owner files and saves verified, registry restored | 6 min 10.6 s | 95 min 53.7 s | 9 min 6.3 s |
| C3 r5 | Focused gate `sprint7-visual` (local, with the candidate-5 same-run agreement) | PASS, 25 tests | 19.8 s | 96 min 13.5 s | 8 min 46.5 s |
| C3 r5 | Controller focused gates on H `ed02905b7f` (`sprint7-visual`, `summon-playtest-r6`, `native-profile`, `loader-compat-probe`; receipts 53.4 + 49.7 + 51.3 + 49.0 = 203.4 s, of which the gate logs show about 3 s of compilation and tests each; the rest controller overhead while two other sessions ran offline builds) | PASS, 4/4 | 412.4 s (batch; estimated about 20 s) | 103 min 5.9 s | 1 min 54.1 s |
| C3 r5 | Formal review 5 (model review, outside the clock) | changes_required (R5-F1, R5-F2 compiled-code defects); work stopped at the amendment-4 limit | — | 103 min 5.9 s | 1 min 54.1 s unused |
| C3 r6 | (C3 amendment 5: allowance raised from 105 to 145 minutes total; 103 min 5.9 s spent) | | | 103 min 5.9 s | 41 min 54.1 s of 145 |
| C3 r6 | Focused gate `sprint7-visual` (local, amendment-5 correction) | FAIL, 24/25: the existing R3-F4 case scripted its throwing end-state read for the old census's two reads (`ThrowAfter = 2`); the corrected census reads once, so the throw came too late; arrangement set to 1 (amendment 5 §3). Time from the log file's creation and last write (the outer timer failed: `bc` missing) | 23.6 s | 103 min 29.5 s | 41 min 30.5 s |
| C3 r6 | Focused gate `sprint7-visual` (local, = code `360ce86f54`) | PASS, 25 tests (production compilation passed) | 5.4 s | 103 min 34.9 s | 41 min 25.1 s |
| C3 r6 | Prospective estimate before recovery candidate 6 (recorded here before the build), from amendment 5's reference durations: Build-Local about 12 min 41 s, mechanics session about 6 min 11 s, evidence comparison and focused gate about 20 s, controller final-gate batch about 6 min 52 s: about 26 min 4 s together, leaving about 15 min 21 s; a conditional round-7 gate batch (about 6 min 52 s) would leave about 8 min 29 s | estimate | — | 103 min 34.9 s | 41 min 25.1 s |
| C3 r6 | Build-Local `-DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual` on `962bf27694` (recovery candidate 6 of 6: package `6c71d928ea4293945af43a4b33f81238047176b07ed10fc88edee342649cd55a`, DLL `6af754eb06f72a261916d2dcd6e5fea125b62b13dcc9eeae53ad000e94341411`; 25 selected tests) | PASS. Outer command 05:23:01Z to 08:58:46Z, against the estimated 12 min 41 s. Its step output matches candidate 5's build except that the Python packaging suite's 47 tests took 60.2 s (2.0 s before); the System event log shows no sleep, resume or boot event in the period. Cause not established; as amendment 5 directs, no performance investigation was started, and concurrent peer work is at most a possible contribution | 215 min 44.9 s | 319 min 19.8 s | exceeded by 174 min 19.8 s |
| C3 r6 | Stop under amendment 5 ("If a concrete remaining obligation or the actual time allowance prevents this sequence, preserve the exact state and report it without extending your authority"), recorded 2026-11-27 after 08:58:46Z: candidate 6's mechanics session, the evidence comparison, the controller final gates and formal review 6 were not run; no same-package rerun, no candidate 7, no controller submission and no counter, threshold or state edit | stop | — | 319 min 19.8 s | exceeded by 174 min 19.8 s |
| C3 r6 | (C3 amendment 6: allowance raised from 145 to 360 minutes total; 319 min 19.8 s charged, the build's outer time kept as charged after the records check) | | | 319 min 19.8 s | 40 min 40.2 s of 360 |
| C3 r6 | Prospective estimate before candidate 6's mechanics session (recorded before launch): session about 6 min 11 s, evidence comparison and local focused gate about 30 s, controller final-gate batch about 6 min 52 s: about 13 min 33 s together, leaving about 27 min 7 s; a conditional round-7 gate batch (about 6 min 52 s) would leave about 20 min 15 s. The candidate-6 build showed a several-fold overrun is possible; a stop is preserved if the remaining work cannot fit | estimate | — | 319 min 19.8 s | 40 min 40.2 s |
| C3 r6 | Session `20261127T133020Z-owner-0.33.0-s7-c3-mechanics` (recovery candidate 6, reused; C2 harness `d77754ba…b4f1`) | PASS, 43/43 checks; seven required cells completed; physical restoration true; transaction succeeded, owner files and saves verified, registry restored | 6 min 16.4 s | 325 min 36.2 s | 34 min 23.8 s |
| C3 r6 | Focused gate `sprint7-visual` (local, with the candidate-6 same-run agreement) | PASS, 25 tests | 5.7 s | 325 min 41.9 s | 34 min 18.1 s |
| C3 r6 | Prospective estimate before the controller final gates on this head (recorded before them): batch about 6 min 52 s, leaving about 27 min 26 s; a conditional round-7 batch would leave about 20 min 34 s | estimate | — | 325 min 41.9 s | 34 min 18.1 s |
| C3 r6 | Controller focused gates on H `e5b1e4af85` (`sprint7-visual` 106.4 s, `summon-playtest-r6` 118.4 s, `native-profile` 113.0 s, `loader-compat-probe` 93.6 s outer; receipts 58.7 + 58.0 + 55.2 + 46.0 = 217.9 s, worktree unchanged; the rest controller overhead outside the receipts) | PASS, 4/4 | 432.3 s (batch; estimated about 6 min 52 s) | 332 min 54.2 s | 27 min 5.8 s |
| C3 r6 | Formal review 6 (model review, outside the clock) | changes_required: R5-F1, R5-F2, R5-F3, R1-F13, R5-F4 verified; R6-F1 (P2 records) and R6-F2 (P3 records) opened | — | 332 min 54.2 s | 27 min 5.8 s |
| C3 r7 | Prospective estimate before the round-7 controller gates (records-only corrections; recorded before them): batch about 7 min 12 s (the round-6 batch), leaving about 19 min 54 s | estimate | — | 332 min 54.2 s | 27 min 5.8 s |
| C3 r7 | Controller focused gates on H `74c3d43288` (`sprint7-visual` 86.2 s, `summon-playtest-r6` 85.8 s, `native-profile` 91.6 s, `loader-compat-probe` 96.4 s outer; receipts 46.4 + 42.6 + 48.2 + 48.4 = 185.6 s, worktree unchanged) | PASS, 4/4 | 361.0 s (batch; estimated about 7 min 12 s) | 338 min 55.2 s | 21 min 4.8 s |
| C3 r7 | Formal review 7 (model review, outside the clock) | changes_required at the cap: R6-F1 verified; R6-F2 (P3) rejected; R7-F1 (P2 records) opened; controller BLOCKED 7/7. Stop commit: records only, no validation | — | 338 min 55.2 s | 21 min 4.8 s unused |
| C3 r8 | (C3 amendment 7: formal-review ceiling raised from seven to eight for formal review 8 of the records-only stop-commit corrections; allowance unchanged at 360 minutes, candidates six of six; no validation for the admission) | | | 338 min 55.2 s | 21 min 4.8 s |
| C3 r8 | Prospective estimate before the round-8 controller gates (records-only submission head; recorded before them): batch about 7 min 12 s (the larger of the last two batches, 361.0 s and 432.3 s), leaving about 13 min 53 s; the actual batch is recorded in the review-8 developer report (this manifest stays a pre-review-8 snapshot so the gate head is unchanged) | estimate | — | 338 min 55.2 s | 21 min 4.8 s |

Notes:
- Through C1 (to 2026-11-26 00:24Z) no product package, harness build or game session had run in the recovery; C2 then
  built its harness four times and ran four short lifecycle sessions on the existing candidate-3 package (rows above).
- Failed validation attempts so far: twenty-two (two in C1 round 2, four in C1 round 4, one in C1 round 5, fifteen in C3; seventeen through the review-3 stop, eighteen at C3 amendment 3, twenty at C3 amendment 4, twenty-one at C3 amendment 5, one since); refused controller submissions with no validation launched are listed in their rows. The two amendment-4 mutation controls failed by design (the controls detecting the removed behavior) and are listed in their rows, not counted as failed attempts.
- R3-F5 (records): a 1.0 s arithmetic drift from the 45 min 5.1 s row was corrected on 2026-11-26 in every later running total; the round-3 report's 59 min 36.0 s should read 59 min 37.0 s and its receipt subtotal 8.9 s.
- Earlier spend stays separate: 65 min 42 s in parent rounds 6 and 7. New product candidates used: 6 of 6 (C3's first combined candidate, its contingency, recovery candidate 3 under C3 amendment 1, recovery candidate 4 under C3 amendment 3, recovery candidate 5 under C3 amendment 4, and recovery candidate 6 under C3 amendment 5, built once and reused under C3 amendment 6 without a rebuild); candidate 7 is not authorized.
- Reserve before the C2 round-3 session: C3 still needs one combined Build-Local candidate and its affected native
  qualification (the effects session, estimated at about 18 minutes, plus the build and gates). After the planned C2 short
  session (about 2 min 15 s) and controller gates, about 32 minutes remain for C3.
- Reserve before the C2 round-4 session: C3 is estimated at about 25 minutes (combined build, focused gates, effects
  qualification); after this session (about 2 min 15 s) and the round-4 controller gates about 30 min 25 s remain, leaving
  about 5 minutes of margin for one short C2 rerun if round 5 needs one.
