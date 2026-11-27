<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r02-review part=1/2 sha256=6ccc1e868b8d8c1c07a0a10af9c0dd95a3797802f7fa93a5f7b12553fcfd6ee4 -->
_Part 1 of 2._

### Codex GPT-6.2 Astra — review — round 2 — candidate `89d4efa347`

**Decision:** `changes_required` — All four Focused gates passed on this commit. Source review still found paths that lose cleanup ownership, repeat a retirement, or publish a successful observation without establishing it. In particular, the new Dust control can deliver the effect one frame before the disposal it describes as pre-delivery. The corrected binary also needs the native qualification required by OWNER AMENDMENT 1.

## Continuation for Claude

Fix the concrete caller paths in the findings before building recovery candidate 3. Carry subscriptions, exact processes, pending condition cleanup and source retirements through their last owner. Make independent cleanup continue after a failure. Record completion only after its publication succeeds. Take the Dust delivery observation at the disposal boundary and keep both foreign controls present through the cleanup they test.

Exercise those paths through the production orchestration with controlled boundary failures. Correct the mechanical assertion and integration prechecks, then run the affected Focused gates with production compilation. Within the existing enrollment and amended allowance, qualify one combined candidate with the mechanics-only session. The next formal report needs the corrected-byte native observations, same-run verdict comparison, restoration receipts and updated ledger. Keep the work within C3 runtime verification, its directly affected callers, tests and records. Preserve the frozen stack, C1 and C2 acceptances, historical failures and existing owner presentation material.

#### Findings

**R2-F1** · P1 · evidence_gap · blocks readiness — The corrected C3 bytes still need native qualification
- Location: `scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json:15`; acceptance: C3 §5, OWNER AMENDMENT 1 §§4–5
- Trigger: Evaluating the final checkpoint for owner readiness.
- Expected: One identified corrected package completes the mechanical qualification on the admitted fixture.
- Actual: The newest native receipt runs the earlier 6abd692fa7 product. H changes compiled ownership, runner and observation code. Its receipts cover compilation and domain tests only.
- Why it blocks / does not: Final C3 acceptance requires engine observations on the delivered correction. The owner expressly allowed this source review before that run.
- Requested correction: After the offline corrections and affected gates, build recovery candidate 3 and run the authorized mechanics-only session.
- Requested evidence: Bind source, package, DLL, harness, fixture, collector and reader to the run. Show all declared parent cells admitted and completed, separate restoration results, the affected native ownership controls, real constructed cue posts, agreement of live, persisted and collector-shaped verdicts, and transaction readbacks for owner files, saves, settings and registry.

**R2-F2** · P1 · defect · blocks readiness — Disposing the driver can strand the Dust condition ledger
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1127`; acceptance: R7-F2, R7-F5, OWNER AMENDMENT 1 §3B–D
- Trigger: The driver exits while its child is waiting for a retained execution to settle.
- Expected: Pending cleanup remains executable through phase and fixture teardown, with an explicit unresolved result if settlement cannot be established.
- Actual: ReleaseOwnedEffects puts late removal, unsubscription and foreign-control cleanup inside a child residual. Sprint7Abandonment’s finally publishes a description and failures, then reports the driver. It does not transfer that residual to the phase. Disposing the driver during its wait leaves a subscribed ledger that the outer phase cannot settle, even if the execution ends on the next frame.
- Why it blocks / does not: A failed verdict prevents further admission but leaves the cleanup action without a caller. Late conditions and the subscription can survive scenario teardown.
- Requested correction: Carry pending child reconciliation to an enclosing owner on disposal or timeout. Give terminal teardown a bounded cleanup path that preserves the original failure and records any remaining uncertainty.
- Requested evidence: Extend the recovery-wait disposal case. After disposing the driver, deliver an attributable condition and end the retained process. Drive only the outer cleanup and show removal of the exact condition, one unsubscription, preservation of the foreign control through reconciliation and the original failed verdict.

**R2-F3** · P1 · defect · blocks readiness — The pre-delivery control samples one frame before disposal
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1092`; acceptance: R7-F2, OWNER AMENDMENT 1 §§3B,5
- Trigger: A pending Dust cast delivers after setting the probe’s ready flag and before the driver resumes.
- Expected: The recorded execution and delivery state describe the instant the cell is disposed. The receipt distinguishes effects already present from effects delivered afterward.
- Actual: S7Dust records processStarted and deliveredBeforeDisposal, sets ready and yields. The driver checks Ready before sequence.MoveNext and unconditionally yields the step that sets it. The engine gets another frame before sequence.Dispose. A delivery in that frame can be removed by initial cleanup while the row still says delivery had not occurred. The final check accepts an empty remaining list without distinguishing that case.
- Why it blocks / does not: The next package could pass the named pre-delivery control without exercising pre-delivery disposal.
- Requested correction: Dispose in the same step that establishes readiness or recheck the exact execution at disposal and reject a missed boundary. Bind subsequent delivery and removal observations to that execution.
- Requested evidence: Exercise delivery between the readiness signal and the next outer step. That case must not qualify as pre-delivery disposal. In the native receipt, identify the execution, the disposal-time state and the observed late delivery or cancellation outcome.

**R2-F4** · P1 · defect · blocks readiness — The same-caster foreign condition is removed before the cleanup it tests
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3202`; acceptance: R7-F2, OWNER AMENDMENT 1 §§3B,5
- Trigger: The ordinary Dust cell releases its conditions after the separate same-caster execution.
- Expected: Both exact foreign instances remain observable through the owned release and any deferred settlement. Their survival is established before the fixture removes its controls.
- Actual: The finalizer records unownedKept, removes the same-caster control and destroys its outside target before calling ReleaseOwnedEffects. The passing assertion therefore cannot show that owned cleanup spared that control. For the separate caster control, the check at line 3267 also accepts a missing settlement observation through `!= false`.
- Why it blocks / does not: A cleanup that would remove a foreign condition can pass the very control meant to detect it.
- Requested correction: Keep the controls and their targets alive through reconciliation. Check their exact identities afterward, publish missing or unreadable results to isolation, then clean up the fixture-owned controls.
- Requested evidence: Show the ordinary and deferred release paths with both controls present afterward. A controlled release that removes the same-caster foreign instance must fail the result consumed by admission.

**R2-F5** · P1 · defect · blocks readiness — Ordinary source retirement bypasses the shared request record
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:1964`; acceptance: R7-F4, OWNER AMENDMENT 1 §3A
- Trigger: The normal-callback Ankylosaurus control or Dust source-loss control retires its source.
- Expected: Every affected direct retirement has one attempt, a named result and settlement accounting across its remaining owners.
- Actual: The ordinary Ankylosaurus retirement calls DisposeExpandedSummoningUnits directly. Dust source loss does the same at line 3013. Both units are in the phase-owned list, but neither direct call enters the retirement record. If the call throws or returns with the unit live, phase teardown can issue it again. The outside Dust target also receives a direct cleanup without a settlement read before reuse.
- Why it blocks / does not: The callback-loss pair now has one-request ownership. Other direct releases in the same required mechanics session can still be retried or go unverified.
- Requested correction: Route these direct releases through the applicable ownership record. Preserve the unit identity until settlement and continue independent releases.
- Requested evidence: Make the ordinary source retirement throw or return with the source alive, then drive phase and fixture teardown. Show one request, its original result, independent releases and the unsettled identity in the verdict.

**R2-F6** · P1 · defect · blocks readiness — Settlement and delivery still depend on a mutable command field after capture
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:929`; acceptance: R7-F2, OWNER AMENDMENT 1 §3B
- Trigger: A process becomes readable during settlement or another owned command is unreadable during a late delivery.
- Expected: Once a command’s exact process is observed, settlement and delivery attribution can use that retained association independently of mutable command fields.
- Actual: EndedOf adds a recovered process to a flat list, then consults processOf(command) again on every poll. If the command field clears after that recovery, its named residual stays unknown even when the retained process ends. At effect delivery, S7DustConditions also reads command fields in FirstOrDefault before checking retained contexts. One throwing getter can skip attribution for a different, known execution.
- Why it blocks / does not: The guard can refuse an execution that has demonstrably settled, and a late buff from a known execution can escape the ledger.
- Requested correction: Retain the command-to-process association and use it for subsequent reads. Isolate unreadable command lookups so they cannot hide delivery by an independently retained process.
- Requested evidence: Cover an initially unreadable command whose process becomes readable, whose field then clears, and whose retained process ends. Also deliver an effect for a known retained process while another command getter throws. Verify settlement and exact effect accounting in both cases.

**R2-F7** · P1 · defect · blocks readiness — One combat cleanup failure skips the remaining actors
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3536`; acceptance: R4-F1, R7-F2, OWNER AMENDMENT 1 §3B
- Trigger: An interruption or queue update fails during final phase teardown.
- Expected: Each independent owned command or combat release is attempted and attributed. Unrelated commands are preserved.
- Actual: The phase wraps its entire combat foreach in one restore action. A failure on the first unit skips queue and combat cleanup for later units. InterruptAll also selects every command on each visited actor, including the fixture caster and hostile.
- Why it blocks / does not: The enclosing teardown bypasses the independent, identity-scoped release behavior established inside the Dust cell.
- Requested correction: Use the phase’s known command and actor identities and isolate independent teardown operations. Carry each failure into the final result with its identity.
- Requested evidence: Make one actor’s interruption throw while another has an owned command and a foreign command. Show the later owned release, the untouched foreign command and the named final failure.

**R2-F8** · P1 · defect · blocks readiness — Early condition-release errors can be lost during final reporting
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:2567`; acceptance: R4-F1, OWNER AMENDMENT 1 §§3B,3D
- Trigger: A condition removal fails and the Dust finalizer encounters a second exception before copying its local failure array.
- Expected: Each early release failure reaches the cumulative owner when it occurs and survives subsequent cleanup or diagnostic errors.
- Actual: An early Sickened removal failure is appended to cellFailures or releaseFailures. It reaches isolation only at line 3250, after further operations including owner.UnfinishedLeft(). If one of those reads throws, the original failure is lost from the cumulative result. FailedRemovals suppresses a later removal attempt that might otherwise expose it again.
- Why it blocks / does not: A child can report the later exception while losing the original injected or independent condition-release failure.
- Requested correction: Publish early condition-release failures at the release boundary, preserving their exact identities and operation. Keep diagnostic rows without publishing the same failure twice.
- Requested evidence: Combine an early condition-removal exception with a later throwing cleanup read. Show both original errors in child and phase reporting and refusal of subsequent work.

**R1-F5** · P1 · defect · blocks readiness — Hook rollback now runs, but diagnostic failures are discarded
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1278`; acceptance: R7-F3, OWNER AMENDMENT 1 §3C
- Trigger: A Harmony count throws before rollback or normal disposal, and a later count succeeds.
- Expected: Preserve the original setup error, each rollback error and each failed readback with its target and stage. Continue all applicable releases.
- Actual: RollbackSetup now attempts unhook independently, but both count catches discard their exceptions. If only the first count fails, SetupFailure can describe rollback as complete without retaining the read error. S7DisposeTrace similarly accepts an unreadable pre-dispose count when the final count is zero.
- Why it blocks / does not: An independent Harmony inspection error can disappear from the sticky result, including beside an intentional setup fault.
- Requested correction: Carry readback exceptions alongside nullable counts and publish them through the caller’s cumulative result. Preserve the individual normal-disposal errors as well.
- Requested evidence: Have the first count throw, let independent unpatches run and let the final count return zero. Also exercise failed rollback. The original setup, read and release errors must remain distinguishable and the unexpected read failure must prevent clean isolation.

**R1-F9** · P1 · defect · blocks readiness — Partly constructed save handlers still have no release owner
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3499`; acceptance: C3 §5, OWNER AMENDMENT 1 §3C
- Trigger: EventBus.Subscribe partly succeeds and throws while a required effects handler is being constructed.
- Expected: Retain the exact handler before subscription can partly succeed, release each attempted subscription, and publish the original setup and cleanup results.
- Actual: Sprint26Saves subscribes `this` in its constructor. The phase adds the object to saves only after construction returns, and Dust assigns outsideSaves the same way. A partial subscription followed by a throw leaves the registered release with no handler to dispose. The Ankylosaurus attack log also subscribes before its cleanup try. A phase setup exception can reach the outer poller while the phase’s own isolation has no named setup failure.
- Why it blocks / does not: A surviving rule handler can affect later saves, and a pre-registered action over an empty list cannot release it.
- Requested correction: Separate handler construction from subscription for the affected callers, preserve existing behavior for other users, and put the attack-log acquisition inside its cleanup scope. Publish phase setup failures into the phase result.
- Requested evidence: At the production subscription boundary, register the handler and then throw. Show that the exact handler and earlier recorder are both unsubscribed, the original error is recorded, and required work is refused.

**R1-F10** · P2 · defect · blocks readiness — A failed completion publication still counts as a completed cell
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1187`; acceptance: C3 §5, OWNER AMENDMENT 1 §3D
- Trigger: The required cell reaches the end of its body and its completion receipt fails.
- Expected: Successful completion requires successful disposal and completion publication. All original and cleanup errors remain available.
- Actual: IsolatedCell adds the name to Completed before invoking the completion callback. If the callback throws for the last required cell, isolation fails but RequiredCellsComplete can still return true. The focused completion test explicitly expects the cell to stay completed.
- Why it blocks / does not: The required-cell and phase records give conflicting outcomes for a failed publication.
- Requested correction: Record successful completion after the callback returns successfully. Preserve the named publication failure and correct the focused expectation.
- Requested evidence: Make the last required cell’s completion writer throw. Show it was admitted, is absent from successful completions, fails required-cell completion and leaves the publication error in isolation.

**R1-F11** · P2 · evidence_gap · blocks readiness — Several controls still substitute test-owned cleanup for production cleanup
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:1780`; acceptance: R4-F1, R7-F2, R7-F4, OWNER AMENDMENT 1 §3E
- Trigger: Using the passing focused case as evidence for the complete failure-to-teardown path.
- Expected: Controlled failures travel through the same acquisition, release, fixture teardown and publication paths used by the effects runner.
- Actual: The paired command/removal test now throws at the operations. Other tests still bypass the relevant callers. RetiringObserver reproduces the retirement sequence in test code and stops before fixture teardown. SetupThenFail registers a known release that the self-subscribing save constructor cannot provide. The hook test returns a finished rollback-failure list from its unhook delegate.
- Why it blocks / does not: These tests pass with the production retry and partial-subscription defects present in H.
- Requested correction: Exercise the narrow production operations shared with the actual callers. Inject the failing subscribe, unpatch or retirement itself and drive the real final-release decision.
- Requested evidence: Provide an H-bound Focused receipt covering both direct retirement failures through outer teardown, partial subscription with the real identity lifetime, and one failing unpatch beside an independent release. Assert attempt counts, exact errors, settlement and admission.

**R2-F9** · P2 · defect · blocks readiness — The mechanics-only receipt would claim unmeasured framing
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:2810`; acceptance: C3 §6, OWNER AMENDMENT 1 §§3F,5
- Trigger: The mechanics selection completes the three Dust casts with clip capture disabled.
- Expected: The mechanical assertion describes only measured mechanics and recolour restoration. Framing is reported only in the profile that measures it.
- Actual: The mechanics branch makes visible true without a framing observation. The emitted s7-dust-casts assertion still says that the first cast was framed on an unobstructed bearing. The added domain case tests phase admission only.
- Why it blocks / does not: A mechanical PASS would certify a visual observation the selected profile expressly omits.
- Requested correction: Make the framing assertion or its expectation profile-specific and exercise the selection used by the enclosing runner.
- Requested evidence: Show that the mechanical selection emits its required checks without a framing PASS, clips, native-look comparison or listening samples. Preserve the existing presentation profile’s framing check.

**R1-F13** · P2 · required_gap · blocks readiness — The integration precheck requires an archive that the parent does not have
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:286`; acceptance: C3 §8, OWNER AMENDMENT 1 §§3F,7
- Trigger: Applying the proposed dependency checks before owner-authorized integration.
- Expected: The conditional integration plan names valid inputs and distinguishes the current correction, reviewed H, native source and archive.
- Actual: The proposed precheck requires every child’s base to be its predecessor’s archive A. C1 starts at #219’s frozen records head and #219 has no archive. The adjacent description also attributes C3’s difference from round-1 H to later records, although 82dc667c93 and H change production code.
- Why it blocks / does not: The first dependency check cannot pass as written, and the active plan blurs which C3 source remains to be qualified.
- Requested correction: Check C1 against #219’s frozen head, C2 against C1 A and C3 against C2 A. Date the records-only description and identify the subsequent compiled correction. Make the intended destination explicit for every later integration operation.
- Requested evidence: Provide the corrected records-only plan with those ancestry checks and distinct identities. Preserve the separate owner decisions for parent review and integration with master.

#### Prior finding assessments

- **R1-F1** — `fix_verified` — For the two direct S7CallbackLoss positions in the original finding, the shared record marks the attempt before calling retirement, counts only returned requests, publishes failures and settlement, and prevents both fixture loops from retrying. R2-F5 identifies separate direct callers that bypass this record. R1-F11 covers the test boundary.
- **R1-F2** — `fix_verified` — Ordinary cell cleanup now retains the ledger while a known execution is pending and releases late conditions when polled through settlement. The focused case covers successful and failing late removal. R2-F2 concerns the separate loss of that polling owner, and R2-F3 concerns the native control’s timing.
- **R1-F3** — `fix_verified` — EndUnfinished uses the command owner’s immediate failure publisher. The passing focused case lets the command subsequently finish and verifies that the original interruption error still refuses the next cell.
- **R1-F4** — `fix_verified` — Step-4 cancellation now names the exact command and passes through capture before interruption. A failed process lookup that remains unresolved produces a named unknown residual. R2-F6 concerns a process recovered after that first unreadable lookup.
- **R1-F5** — `fix_rejected` — Attempted-hook release is independent of the count, and missing patch information for a listed method is unknown. The count catches still discard the read errors, and a later zero can conceal an earlier failed diagnostic from isolation.
- **R1-F6** — `fix_verified` — The driver requires every child owner to report and matches complete expected failure strings. The focused gate exercised an actual failing interruption alongside the removal fault, a different residual count, extra text and an unreported owner. Each prevented advancement.
- **R1-F7** — `fix_verified` — RepairExact rereads the identified leftover after a nonthrowing removal. The disposal-during-wait test verifies one repair attempt, the named still-present result, the original child fault and the surviving foreign control.
- **R1-F8** — `fix_verified` — The parent declares its required cells and checks admission, completion and refusal separately from final settlement. The focused case settles a residual after refusal and confirms that the required sequence stays incomplete.
- **R1-F9** — `fix_rejected` — The recorder, Dust save log and condition ledger have improved acquisition scopes. The actual Sprint26Saves constructor can still subscribe before either affected caller receives its identity, and phase-level setup errors can miss the local isolation record.
- **R1-F10** — `fix_rejected` — Throwing Current and disposal-only failures now reach the runner correctly. Completion publication still occurs after successful completion has already been recorded, so a last-cell reporting failure can leave the declared sequence complete.
- **R1-F11** — `fix_rejected` — The paired failure now originates at real release operations in the shared policy. The retirement and subscription tests still supply their own caller ordering, outer fixture teardown is absent, and the hook rollback test supplies a finished failure list.
- **R1-F12** — `fix_verified` — The new session selects a distinct mechanics phase through the enclosing dispatcher. Its code omits Dust clips, native-look comparison and listening samples, and the manifest discloses the earlier captures. R2-F9 addresses a retained assertion’s wording. R2-F1 covers execution of this selection.
- **R1-F13** — `fix_rejected` — The plan now gives a history-preserving order and identifies the master and owner gates. Its precondition still assumes a parent archive for C1, and its description of current C3 changes is stale.

#### Evidence assessment

- **Inspected:** The controller mission, OWNER AMENDMENT 1, developer report, transcript notice, all 13 prior findings, gate receipts and logs, repository instructions, feature diff and round-1-to-H correction.; The effects runner, callback and projectile hook owners, Dust command and condition owners, abandonment and admission code, shared fixture teardown, native phase dispatch, affected helper implementations and focused tests.; Both C3 session definitions, the historical native extracts and callback row, the C2 acceptance receipt, recovery manifest, validation ledger and proposed integration plan.
- **Executed verification:** Read-only Git checks confirmed H, merge base eab90a5ac82f661ecb903c5456d951a57a085eb9 and local ancestry from the frozen parent through the C1 and C2 archives. The mission and amendment file hashes match the request.; Verified the hashes of all four controller gate logs against their receipts. Each names H, reports successful production compilation, exits 0 and records an unchanged worktree. The filters were sprint7-visual (25 tests), summon-playtest-r6 (11), native-profile (9) and loader-compat-probe (5). All 50 selected executions passed. Receipt durations sum to 9.3 seconds.; Compared the committed 054228Z callback row with the corresponding launch extract. The rows match, including the null verdict and constructed same-frame posts with playing IDs 544 and 545. The H-bound domain gate also passed the persisted and collector-shaped comparison against that historical row.; The earlier C3 receipts retain 051633Z as FAIL with 37 of 40 checks passing and 054228Z as PASS with 41 of 41. Both session receipts record completed restoration. The accepted C2 receipt independently identifies session 033419Z, harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1 and parent package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413.
- **Source inspection only:** The mechanics phase reaches ReviewSprint7Effects and its branches omit clip construction, the native-look comparison and listening samples. The executed admission test covers the new phase name. The actual mechanical session has yet to run.; The retirement bypasses, partial subscription paths, timing of disposal, loss of pending cleanup and final publication order below follow from the committed callers and their helpers. The focused tests exercise only the portions described in the assessments.
- **Required gaps:** Close the production ownership, observation and publication defects below and exercise the affected paths at their real failure boundaries.; Qualify the corrected combined package through the mechanics-only runner, including required-cell completion, late Dust effects, foreign controls, cue verdict agreement and transactional restoration.; Correct the active integration prechecks and carry the final artifact identities, receipts and validation spend into the handoff.
- **Limitations:** This was a read-only review. The foreground shell transcript is withheld. The controller receipts establish the four H-bound gates. The reported 16.7-second gate batch wall time and local development runs come from the developer report and ledger.; The reported recovery spend after those gates is 44 minutes 23.4 seconds of 75 minutes, with 30 minutes 36.6 seconds remaining and two of four recovery candidates used. The committed ledger ends immediately before those gates.; OWNER AMENDMENT 1 accepts C1’s native evaluator result on product source 6abd692fa7ee84604ef8122569f84a57e3b3c1a2, package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46 and DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a. Corrected C3 native behavior remains unverified.

#### Summary

- **Delivered behavior:** Eight prior corrections are verified at source or domain scope. They include the callback-loss pair’s one-request retirement, immediate command-failure publication, exact injected-fault matching, named repair results, refusal-aware required-cell completion and the mechanics-only selection.
- **Scope reviewed:** Final C3 review 2 under OWNER AMENDMENT 1, covering the effects ownership correction, directly affected callers, focused controls, mechanical selection, evidence attribution and conditional stack handoff. C3 has no final archive or handoff.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Visual acceptance for T2, T3 and T6 and listening choices for T5 and T7 remain with the owner on the existing material.; After a current C3 handoff, the owner decides integration of the dependent stack, any resumption or re-review of frozen #219, and the master integration and release boundary.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 1, sha256 `c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233`.

Codex CLI `0.170.0`, thread `01a217e3-dbab-7071-bdea-d0d25bbe525e`, duration 1245.6s, response sha256 `a6d3162cc0ec6a3743e797a349756176fb7aafe11691e235a32d440305e7af9f`, request `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r02-8d4658`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "89d4efa34732aa85c0a3975c5770865e245e6766",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "All four Focused gates passed on this commit. Source review still found paths that lose cleanup ownership, repeat a retirement, or publish a successful observation without establishing it. In particular, the new Dust control can deliver the effect one frame before the disposal it describes as pre-delivery. The corrected binary also needs the native qualification required by OWNER AMENDMENT 1.\n\n## Continuation for Claude\n\nFix the concrete caller paths in the findings before building recovery candidate 3. Carry subscriptions, exact processes, pending condition cleanup and source retirements through their last owner. Make independent cleanup continue after a failure. Record completion only after its publication succeeds. Take the Dust delivery observation at the disposal boundary and keep both foreign controls present through the cleanup they test.\n\nExercise those paths through the production orchestration with controlled boundary failures. Correct the mechanical assertion and integration prechecks, then run the affected Focused gates with production compilation. Within the existing enrollment and amended allowance, qualify one combined candidate with the mechanics-only session. The next formal report needs the corrected-byte native observations, same-run verdict comparison, restoration receipts and updated ledger. Keep the work within C3 runtime verification, its directly affected callers, tests and records. Preserve the frozen stack, C1 and C2 acceptances, historical failures and existing owner presentation material.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git checks confirmed H, merge base eab90a5ac82f661ecb903c5456d951a57a085eb9 and local ancestry from the frozen parent through the C1 and C2 archives. The mission and amendment file hashes match the request.",
      "Verified the hashes of all four controller gate logs against their receipts. Each names H, reports successful production compilation, exits 0 and records an unchanged worktree. The filters were sprint7-visual (25 tests), summon-playtest-r6 (11), native-profile (9) and loader-compat-probe (5). All 50 selected executions passed. Receipt durations sum to 9.3 seconds.",
      "Compared the committed 054228Z callback row with the corresponding launch extract. The rows match, including the null verdict and constructed same-frame posts with playing IDs 544 and 545. The H-bound domain gate also passed the persisted and collector-shaped comparison against that historical row.",
      "The earlier C3 receipts retain 051633Z as FAIL with 37 of 40 checks passing and 054228Z as PASS with 41 of 41. Both session receipts record completed restoration. The accepted C2 receipt independently identifies session 033419Z, harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1 and parent package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413."
    ],
    "inspected": [
      "The controller mission, OWNER AMENDMENT 1, developer report, transcript notice, all 13 prior findings, gate receipts and logs, repository instructions, feature diff and round-1-to-H correction.",
      "The effects runner, callback and projectile hook owners, Dust command and condition owners, abandonment and admission code, shared fixture teardown, native phase dispatch, affected helper implementations and focused tests.",
      "Both C3 session definitions, the historical native extracts and callback row, the C2 acceptance receipt, recovery manifest, validation ledger and proposed integration plan."
    ],
    "limitations": [
      "This was a read-only review. The foreground shell transcript is withheld. The controller receipts establish the four H-bound gates. The reported 16.7-second gate batch wall time and local development runs come from the developer report and ledger.",
      "The reported recovery spend after those gates is 44 minutes 23.4 seconds of 75 minutes, with 30 minutes 36.6 seconds remaining and two of four recovery candidates used. The committed ledger ends immediately before those gates.",
      "OWNER AMENDMENT 1 accepts C1\u2019s native evaluator result on product source 6abd692fa7ee84604ef8122569f84a57e3b3c1a2, package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46 and DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a. Corrected C3 native behavior remains unverified."
    ],
    "required_gaps": [
      "Close the production ownership, observation and publication defects below and exercise the affected paths at their real failure boundaries.",
      "Qualify the corrected combined package through the mechanics-only runner, including required-cell completion, late Dust effects, foreign controls, cue verdict agreement and transactional restoration.",
      "Correct the active integration prechecks and carry the final artifact identities, receipts and validation spend into the handoff."
    ],
    "source_inspection_only": [
      "The mechanics phase reaches ReviewSprint7Effects and its branches omit clip construction, the native-look comparison and listening samples. The executed admission test covers the new phase name. The actual mechanical session has yet to run.",
      "The retirement bypasses, partial subscription paths, timing of disposal, loss of pending cleanup and final publication order below follow from the committed callers and their helpers. The focused tests exercise only the portions described in the assessments."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "C3 \u00a75",
        "OWNER AMENDMENT 1 \u00a7\u00a74\u20135"
      ],
      "actual": "The newest native receipt runs the earlier 6abd692fa7 product. H changes compiled ownership, runner and observation code. Its receipts cover compilation and domain tests only.",
      "blocking_rationale": "Final C3 acceptance requires engine observations on the delivered correction. The owner expressly allowed this source review before that run.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "One identified corrected package completes the mechanical qualification on the admitted fixture.",
      "id": "R2-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json",
        "line": 15
      },
      "priority": "P1",
      "requested_correction": "After the offline corrections and affected gates, build recovery candidate 3 and run the authorized mechanics-only session.",
      "requested_evidence": "Bind source, package, DLL, harness, fixture, collector and reader to the run. Show all declared parent cells admitted and completed, separate restoration results, the affected native ownership controls, real constructed cue posts, agreement of live, persisted and collector-shaped verdicts, and transaction readbacks for owner files, saves, settings and registry.",
      "title": "The corrected C3 bytes still need native qualification",
      "trigger": "Evaluating the final checkpoint for owner readiness."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "R7-F5",
        "OWNER AMENDMENT 1 \u00a73B\u2013D"
      ],
      "actual": "ReleaseOwnedEffects puts late removal, unsubscription and foreign-control cleanup inside a child residual. Sprint7Abandonment\u2019s finally publishes a description and failures, then reports the driver. It does not transfer that residual to the phase. Disposing the driver during its wait leaves a subscribed ledger that the outer phase cannot settle, even if the execution ends on the next frame.",
      "blocking_rationale": "A failed verdict prevents further admission but leaves the cleanup action without a caller. Late conditions and the subscription can survive scenario teardown.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Pending cleanup remains executable through phase and fixture teardown, with an explicit unresolved result if settlement cannot be established.",
      "id": "R2-F2",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1127
      },
      "priority": "P1",
      "requested_correction": "Carry pending child reconciliation to an enclosing owner on disposal or timeout. Give terminal teardown a bounded cleanup path that preserves the original failure and records any remaining uncertainty.",
      "requested_evidence": "Extend the recovery-wait disposal case. After disposing the driver, deliver an attributable condition and end the retained process. Drive only the outer cleanup and show removal of the exact condition, one unsubscription, preservation of the foreign control through reconciliation and the original failed verdict.",
      "title": "Disposing the driver can strand the Dust condition ledger",
      "trigger": "The driver exits while its child is waiting for a retained execution to settle."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,5"
      ],
      "actual": "S7Dust records processStarted and deliveredBeforeDisposal, sets ready and yields. The driver checks Ready before sequence.MoveNext and unconditionally yields the step that sets it. The engine gets another frame before sequence.Dispose. A delivery in that frame can be removed by initial cleanup while the row still says delivery had not occurred. The final check accepts an empty remaining list without distinguishing that case.",
      "blocking_rationale": "The next package could pass the named pre-delivery control without exercising pre-delivery disposal.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The recorded execution and delivery state describe the instant the cell is disposed. The receipt distinguishes effects already present from effects delivered afterward.",
      "id": "R2-F3",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1092
      },
      "priority": "P1",
      "requested_correction": "Dispose in the same step that establishes readiness or recheck the exact execution at disposal and reject a missed boundary. Bind subsequent delivery and removal observations to that execution.",
      "requested_evidence": "Exercise delivery between the readiness signal and the next outer step. That case must not qualify as pre-delivery disposal. In the native receipt, identify the execution, the disposal-time state and the observed late delivery or cancellation outcome.",
      "title": "The pre-delivery control samples one frame before disposal",
      "trigger": "A pending Dust cast delivers after setting the probe\u2019s ready flag and before the driver resumes."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,5"
      ],
      "actual": "The finalizer records unownedKept, removes the same-caster control and destroys its outside target before calling ReleaseOwnedEffects. The passing assertion therefore cannot show that owned cleanup spared that control. For the separate caster control, the check at line 3267 also accepts a missing settlement observation through `!= false`.",
      "blocking_rationale": "A cleanup that would remove a foreign condition can pass the very control meant to detect it.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Both exact foreign instances remain observable through the owned release and any deferred settlement. Their survival is established before the fixture removes its controls.",
      "id": "R2-F4",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3202
      },
      "priority": "P1",
      "requested_correction": "Keep the controls and their targets alive through reconciliation. Check their exact identities afterward, publish missing or unreadable results to isolation, then clean up the fixture-owned controls.",
      "requested_evidence": "Show the ordinary and deferred release paths with both controls present afterward. A controlled release that removes the same-caster foreign instance must fail the result consumed by admission.",
      "title": "The same-caster foreign condition is removed before the cleanup it tests",
      "trigger": "The ordinary Dust cell releases its conditions after the separate same-caster execution."
    },
    {
      "acceptance_ids": [
        "R7-F4",
        "OWNER AMENDMENT 1 \u00a73A"
      ],
      "actual": "The ordinary Ankylosaurus retirement calls DisposeExpandedSummoningUnits directly. Dust source loss does the same at line 3013. Both units are in the phase-owned list, but neither direct call enters the retirement record. If the call throws or returns with the unit live, phase teardown can issue it again. The outside Dust target also receives a direct cleanup without a settlement read before reuse.",
      "blocking_rationale": "The callback-loss pair now has one-request ownership. Other direct releases in the same required mechanics session can still be retried or go unverified.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every affected direct retirement has one attempt, a named result and settlement accounting across its remaining owners.",
      "id": "R2-F5",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 1964
      },
      "priority": "P1",
      "requested_correction": "Route these direct releases through the applicable ownership record. Preserve the unit identity until settlement and continue independent releases.",
      "requested_evidence": "Make the ordinary source retirement throw or return with the source alive, then drive phase and fixture teardown. Show one request, its original result, independent releases and the unsettled identity in the verdict.",
      "title": "Ordinary source retirement bypasses the shared request record",
      "trigger": "The normal-callback Ankylosaurus control or Dust source-loss control retires its source."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a73B"
      ],
      "actual": "EndedOf adds a recovered process to a flat list, then consults processOf(command) again on every poll. If the command field clears after that recovery, its named residual stays unknown even when the retained process ends. At effect delivery, S7DustConditions also reads command fields in FirstOrDefault before checking retained contexts. One throwing getter can skip attribution for a different, known execution.",
      "blocking_rationale": "The guard can refuse an execution that has demonstrably settled, and a late buff from a known execution can escape the ledger.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Once a command\u2019s exact process is observed, settlement and delivery attribution can use that retained association independently of mutable command fields.",
      "id": "R2-F6",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 929
      },
      "priority": "P1",
      "requested_correction": "Retain the command-to-process association and use it for subsequent reads. Isolate unreadable command lookups so they cannot hide delivery by an independently retained process.",
      "requested_evidence": "Cover an initially unreadable command whose process becomes readable, whose field then clears, and whose retained process ends. Also deliver an effect for a known retained process while another command getter throws. Verify settlement and exact effect accounting in both cases.",
      "title": "Settlement and delivery still depend on a mutable command field after capture",
      "trigger": "A process becomes readable during settlement or another owned command is unreadable during a late delivery."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a73B"
      ],
      "actual": "The phase wraps its entire combat foreach in one restore action. A failure on the first unit skips queue and combat cleanup for later units. InterruptAll also selects every command on each visited actor, including the fixture caster and hostile.",
      "blocking_rationale": "The enclosing teardown bypasses the independent, identity-scoped release behavior established inside the Dust cell.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each independent owned command or combat release is attempted and attributed. Unrelated commands are preserved.",
      "id": "R2-F7",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3536
      },
      "priority": "P1",
      "requested_correction": "Use the phase\u2019s known command and actor identities and isolate independent teardown operations. Carry each failure into the final result with its identity.",
      "requested_evidence": "Make one actor\u2019s interruption throw while another has an owned command and a foreign command. Show the later owned release, the untouched foreign command and the named final failure.",
      "title": "One combat cleanup failure skips the remaining actors",
      "trigger": "An interruption or queue update fails during final phase teardown."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,3D"
      ],
      "actual": "An early Sickened removal failure is appended to cellFailures or releaseFailures. It reaches isolation only at line 3250, after further operations including owner.UnfinishedLeft(). If one of those reads throws, the original failure is lost from the cumulative result. FailedRemovals suppresses a later removal attempt that might otherwise expose it again.",
      "blocking_rationale": "A child can report the later exception while losing the original injected or independent condition-release failure.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each early release failure reaches the cumulative owner when it occurs and survives subsequent cleanup or diagnostic errors.",
      "id": "R2-F8",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 2567
      },
      "priority": "P1",
      "requested_correction": "Publish early condition-release failures at the release boundary, preserving their exact identities and operation. Keep diagnostic rows without publishing the same failure twice.",
      "requested_evidence": "Combine an early condition-removal exception with a later throwing cleanup read. Show both original errors in child and phase reporting and refusal of subsequent work.",
      "title": "Early condition-release errors can be lost during final reporting",
      "trigger": "A condition removal fails and the Dust finalizer encounters a second exception before copying its local failure array."
    },
    {
      "acceptance_ids": [
        "R7-F3",
        "OWNER AMENDMENT 1 \u00a73C"
      ],
      "actual": "RollbackSetup now attempts unhook independently, but both count catches discard their exceptions. If only the first count fails, SetupFailure can describe rollback as complete without retaining the read error. S7DisposeTrace similarly accepts an unreadable pre-dispose count when the final count is zero.",
      "blocking_rationale": "An independent Harmony inspection error can disappear from the sticky result, including beside an intentional setup fault.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Preserve the original setup error, each rollback error and each failed readback with its target and stage. Continue all applicable releases.",
      "id": "R1-F5",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1278
      },
      "priority": "P1",
      "requested_correction": "Carry readback exceptions alongside nullable counts and publish them through the caller\u2019s cumulative result. Preserve the individual normal-disposal errors as well.",
      "requested_evidence": "Have the first count throw, let independent unpatches run and let the final count return zero. Also exercise failed rollback. The original setup, read and release errors must remain distinguishable and the unexpected read failure must prevent clean isolation.",
      "title": "Hook rollback now runs, but diagnostic failures are discarded",
      "trigger": "A Harmony count throws before rollback or normal disposal, and a later count succeeds."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75",
        "OWNER AMENDMENT 1 \u00a73C"
      ],
      "actual": "Sprint26Saves subscribes `this` in its constructor. The phase adds the object to saves only after construction returns, and Dust assigns outsideSaves the same way. A partial subscription followed by a throw leaves the registered release with no handler to dispose. The Ankylosaurus attack log also subscribes before its cleanup try. A phase setup exception can reach the outer poller while the phase\u2019s own isolation has no named setup failure.",
      "blocking_rationale": "A surviving rule handler can affect later saves, and a pre-registered action over an empty list cannot release it.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Retain the exact handler before subscription can partly succeed, release each attempted subscription, and publish the original setup and cleanup results.",
      "id": "R1-F9",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3499
      },
      "priority": "P1",
      "requested_correction": "Separate handler construction from subscription for the affected callers, preserve existing behavior for other users, and put the attack-log acquisition inside its cleanup scope. Publish phase setup failures into the phase result.",
      "requested_evidence": "At the production subscription boundary, register the handler and then throw. Show that the exact handler and earlier recorder are both unsubscribed, the original error is recorded, and required work is refused.",
      "title": "Partly constructed save handlers still have no release owner",
      "trigger": "EventBus.Subscribe partly succeeds and throws while a required effects handler is being constructed."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75",
        "OWNER AMENDMENT 1 \u00a73D"
      ],
      "actual": "IsolatedCell adds the name to Completed before invoking the completion callback. If the callback throws for the last required cell, isolation fails but RequiredCellsComplete can still return true. The focused completion test explicitly expects the cell to stay completed.",
      "blocking_rationale": "The required-cell and phase records give conflicting outcomes for a failed publication.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Successful completion requires successful disposal and completion publication. All original and cleanup errors remain available.",
      "id": "R1-F10",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1187
      },
      "priority": "P2",
      "requested_correction": "Record successful completion after the callback returns successfully. Preserve the named publication failure and correct the focused expectation.",
      "requested_evidence": "Make the last required cell\u2019s completion writer throw. Show it was admitted, is absent from successful completions, fails required-cell completion and leaves the publication error in isolation.",
      "title": "A failed completion publication still counts as a completed cell",
      "trigger": "The required cell reaches the end of its body and its completion receipt fails."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "R7-F4",
        "OWNER AMENDMENT 1 \u00a73E"
      ],
      "actual": "The paired command/removal test now throws at the operations. Other tests still bypass the relevant callers. RetiringObserver reproduces the retirement sequence in test code and stops before fixture teardown. SetupThenFail registers a known release that the self-subscribing save constructor cannot provide. The hook test returns a finished rollback-failure list from its unhook delegate.",
      "blocking_rationale": "These tests pass with the production retry and partial-subscription defects present in H.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Controlled failures travel through the same acquisition, release, fixture teardown and publication paths used by the effects runner.",
      "id": "R1-F11",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 1780
      },
      "priority": "P2",
      "requested_correction": "Exercise the narrow production operations shared with the actual callers. Inject the failing subscribe, unpatch or retirement itself and drive the real final-release decision.",
      "requested_evidence": "Provide an H-bound Focused receipt covering both direct retirement failures through outer teardown, partial subscription with the real identity lifetime, and one failing unpatch beside an independent release. Assert attempt counts, exact errors, settlement and admission.",
      "title": "Several controls still substitute test-owned cleanup for production cleanup",
      "trigger": "Using the passing focused case as evidence for the complete failure-to-teardown path."
    },
    {
      "acceptance_ids": [
        "C3 \u00a76",
        "OWNER AMENDMENT 1 \u00a7\u00a73F,5"
      ],
      "actual": "The mechanics branch makes visible true without a framing observation. The emitted s7-dust-casts assertion still says that the first cast was framed on an unobstructed bearing. The added domain case tests phase admission only.",
      "blocking_rationale": "A mechanical PASS would certify a visual observation the selected profile expressly omits.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The mechanical assertion describes only measured mechanics and recolour restoration. Framing is reported only in the profile that measures it.",
      "id": "R2-F9",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 2810
      },
      "priority": "P2",
      "requested_correction": "Make the framing assertion or its expectation profile-specific and exercise the selection used by the enclosing runner.",
      "requested_evidence": "Show that the mechanical selection emits its required checks without a framing PASS, clips, native-look comparison or listening samples. Preserve the existing presentation profile\u2019s framing check.",
      "title": "The mechanics-only receipt would claim unmeasured framing",
      "trigger": "The mechanics selection completes the three Dust casts with clip capture disabled."
    },
    {
      "acceptance_ids": [
        "C3 \u00a78",
        "OWNER AMENDMENT 1 \u00a7\u00a73F,7"
      ],
      "actual": "The proposed precheck requires every child\u2019s base to be its predecessor\u2019s archive A. C1 starts at #219\u2019s frozen records head and #219 has no archive. The adjacent description also attributes C3\u2019s difference from round-1 H to later records, although 82dc667c93 and H change production code.",
      "blocking_rationale": "The first dependency check cannot pass as written, and the active plan blurs which C3 source remains to be qualified.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The conditional integration plan names valid inputs and distinguishes the current correction, reviewed H, native source and archive.",
      "id": "R1-F13",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 286
      },
      "priority": "P2",
      "requested_correction": "Check C1 against #219\u2019s frozen head, C2 agains

---

<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r02-review part=2/2 sha256=026bc1dc718ff82ef27876e912081429cc62617edb64a955fc3667a03ea5a52b -->
_Part 2 of 2._

t C1 A and C3 against C2 A. Date the records-only description and identify the subsequent compiled correction. Make the intended destination explicit for every later integration operation.",
      "requested_evidence": "Provide the corrected records-only plan with those ancestry checks and distinct identities. Preserve the separate owner decisions for parent review and integration with master.",
      "title": "The integration precheck requires an archive that the parent does not have",
      "trigger": "Applying the proposed dependency checks before owner-authorized integration."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F1",
      "rationale": "For the two direct S7CallbackLoss positions in the original finding, the shared record marks the attempt before calling retirement, counts only returned requests, publishes failures and settlement, and prevents both fixture loops from retrying. R2-F5 identifies separate direct callers that bypass this record. R1-F11 covers the test boundary."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F2",
      "rationale": "Ordinary cell cleanup now retains the ledger while a known execution is pending and releases late conditions when polled through settlement. The focused case covers successful and failing late removal. R2-F2 concerns the separate loss of that polling owner, and R2-F3 concerns the native control\u2019s timing."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F3",
      "rationale": "EndUnfinished uses the command owner\u2019s immediate failure publisher. The passing focused case lets the command subsequently finish and verifies that the original interruption error still refuses the next cell."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F4",
      "rationale": "Step-4 cancellation now names the exact command and passes through capture before interruption. A failed process lookup that remains unresolved produces a named unknown residual. R2-F6 concerns a process recovered after that first unreadable lookup."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F5",
      "rationale": "Attempted-hook release is independent of the count, and missing patch information for a listed method is unknown. The count catches still discard the read errors, and a later zero can conceal an earlier failed diagnostic from isolation."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F6",
      "rationale": "The driver requires every child owner to report and matches complete expected failure strings. The focused gate exercised an actual failing interruption alongside the removal fault, a different residual count, extra text and an unreported owner. Each prevented advancement."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F7",
      "rationale": "RepairExact rereads the identified leftover after a nonthrowing removal. The disposal-during-wait test verifies one repair attempt, the named still-present result, the original child fault and the surviving foreign control."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F8",
      "rationale": "The parent declares its required cells and checks admission, completion and refusal separately from final settlement. The focused case settles a residual after refusal and confirms that the required sequence stays incomplete."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F9",
      "rationale": "The recorder, Dust save log and condition ledger have improved acquisition scopes. The actual Sprint26Saves constructor can still subscribe before either affected caller receives its identity, and phase-level setup errors can miss the local isolation record."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F10",
      "rationale": "Throwing Current and disposal-only failures now reach the runner correctly. Completion publication still occurs after successful completion has already been recorded, so a last-cell reporting failure can leave the declared sequence complete."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F11",
      "rationale": "The paired failure now originates at real release operations in the shared policy. The retirement and subscription tests still supply their own caller ordering, outer fixture teardown is absent, and the hook rollback test supplies a finished failure list."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F12",
      "rationale": "The new session selects a distinct mechanics phase through the enclosing dispatcher. Its code omits Dust clips, native-look comparison and listening samples, and the manifest discloses the earlier captures. R2-F9 addresses a retained assertion\u2019s wording. R2-F1 covers execution of this selection."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F13",
      "rationale": "The plan now gives a history-preserving order and identifies the master and owner gates. Its precondition still assumes a parent archive for C1, and its description of current C3 changes is stale."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r02-8d4658",
  "round": 2,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "Eight prior corrections are verified at source or domain scope. They include the callback-loss pair\u2019s one-request retirement, immediate command-failure publication, exact injected-fault matching, named repair results, refusal-aware required-cell completion and the mechanics-only selection.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Visual acceptance for T2, T3 and T6 and listening choices for T5 and T7 remain with the owner on the existing material.",
      "After a current C3 handoff, the owner decides integration of the dependent stack, any resumption or re-review of frozen #219, and the master integration and release boundary."
    ],
    "scope_reviewed": "Final C3 review 2 under OWNER AMENDMENT 1, covering the effects ownership correction, directly affected callers, focused controls, mechanical selection, evidence attribution and conditional stack handoff. C3 has no final archive or handoff.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
