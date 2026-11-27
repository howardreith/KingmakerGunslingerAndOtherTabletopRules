<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r03-review part=1/1 sha256=87a5cb8ca53902b31ffa8a65d16e99dccf3665645d735f2188f12e441691dfeb -->
### Codex GPT-6.2 Astra — review — round 3 — candidate `f0ca78cbab`

**Decision:** `changes_required` — Recovery candidate 3 passed the mechanics session. All seven required cells completed, and the receipt shows an actual Dust delivery after disposal. Failure paths still allow an attributable condition to escape cleanup, a transferred cleanup error to miss the phase result, and a foreign command to be interrupted. These defects block the ownership handoff.

Continuation for Claude

Keep the corrections within C3’s effects runner, focused tests and recovery records. Give deferred cleanup a live destination for every failure and residual it produces. Reconcile Dust conditions against recovered execution identities, preserve cleanup ownership when a settlement read is uncertain, and make foreign-control diagnostics independent of release. Finish command-specific teardown, phase setup reporting and condition-failure identification. Drive the failing operations through the production callers identified in the findings.

Update the finding map and reconcile the validation ledger before the next expensive command. The reported durations give 15 min 23.0 s remaining through the round-3 gates. Under OWNER AMENDMENT 1, estimate the affected Focused checks, contingency build, mechanics session and final gates together. If they fit, qualify the corrected compiled bytes with candidate 4 and preserve the same-run verdict, required-cell, physical-restoration and transaction evidence. If they cannot fit, record the concrete stop in the existing enrollment. Preserve the frozen parent, the accepted C1 and C2 evidence identities, and the existing visual and listening materials.

#### Findings

**R3-F1** · P1 · defect · blocks readiness — Transferred cleanup still publishes to the departed child
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:760`; acceptance: R7-F2, R7-F5, OWNER AMENDMENT 1 §3D
- Trigger: The driver exits with reconciliation pending, and reconciliation produces a failure or another residual after the phase takes ownership.
- Expected: The phase receives all failures and new residuals produced while it executes transferred cleanup.
- Actual: TransferPendingTo moves the delegates. ReleaseOwnedEffects and the Dust afterSettlement callback still close over the child isolation. A later unsubscribe or foreign-control failure goes into Child.Failures after the driver has copied Unexpected to the phase. A newly pending outside-target retirement also goes into the departed child after its only transfer. The phase can record the reconciliation as settled when the condition list is empty.
- Why it blocks / does not: The original driver failure remains visible, but the phase loses the later failure identity and can lose the obligation to observe the target’s retirement.
- Requested correction: Carry the result destination with the transferred work, or keep consuming the child’s subsequent failures and residuals until reconciliation finishes.
- Requested evidence: Dispose the driver during its wait, then make unsubscription fail or outside-target retirement remain pending during phase settlement. Show the exact late result in the phase alongside the original driver failure.

**R3-F2** · P1 · defect · blocks readiness — Recovering a process does not recover a delivery missed while it was unreadable
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:2561`; acceptance: R7-F2, OWNER AMENDMENT 1 §3B
- Trigger: Both delivery notifications occur while the owned process lookup is unreadable, and its exact identity becomes readable before settlement.
- Expected: Once the exact process becomes readable, settlement reconciles present conditions attributable to its context and preserves pre-existing and foreign instances.
- Actual: The delivery handler skips unreadable getters and forgets the unmatched context. EndedOf can subsequently recover the exact process, but S7DustConditions.Owned checks only contexts saved during delivery. Release, RemainingIdentities and PresentFrom can all exclude a still-present buff whose SourceAbilityContext matches the recovered process.
- Why it blocks / does not: Execution settlement and an empty ledger can admit another cell while an attributable Sickened remains.
- Requested correction: Reconcile missed delivery attribution against retained process contexts when their identities become available. Keep genuinely unresolved attribution visible to admission.
- Requested evidence: Deliver a condition while its command’s process is unreadable, then recover and end that process. Show removal of the matching condition, survival of a foreign condition, and the resulting admission decision.

**R2-F7** · P1 · defect · blocks readiness — Phase teardown still interrupts every command on an owned summon
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3634`; acceptance: R4-F1, R7-F2, OWNER AMENDMENT 1 §3B
- Trigger: The phase unwinds while a summon has another owner’s queued command, or an interruption on that summon throws.
- Expected: Only identified phase commands are interrupted, and independent command and combat releases continue after a failure.
- Actual: Teardown now visits later actors after an actor fails, but still calls InterruptAll(command => true) on each surviving summon. The Dust cell explicitly runs a separate owner’s command on such a summon at line 3191. An interruption exception also skips the queue update and combat exit for that actor. The focused case uses actor names and an unlisted foreign actor.
- Why it blocks / does not: Owning the summon does not establish ownership of every queued command. Teardown can cancel the foreign control and skip independent cleanup.
- Requested correction: Use the exact commands issued by the effects runner during teardown and guard independent combat releases separately.
- Requested evidence: Through the production teardown operation, make one interruption throw and give a later actor both an owned and a foreign command. Show the owned release, untouched foreign command, attempted combat cleanup and named failure.

**R3-F3** · P1 · defect · blocks readiness — Foreign-control diagnostics can bypass admission or skip independent releases
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3290`; acceptance: R7-F2, R7-F4, OWNER AMENDMENT 1 §§3B,5
- Trigger: The ordinary Dust control cannot identify the expected same-caster buff, or a control presence read throws during settlement.
- Expected: Each control required by the running path has a named presence result. All independent releases and retirement publication run even when one result is unreadable.
- Actual: The control list omits a required instance when its reference is null. The ordinary Dust assertion then fails, but Sprint25Check only records the assertion and the isolation guard can admit the next cell. For acquired controls, lines 3290–3291 repeat the presence reads outside ControlsKept. A throw there exits before either control release, outside-target retirement or retirement publication. The separately spawned outside target has no entry in the phase’s owned-summon list.
- Why it blocks / does not: A missing observation can bypass admission, and a diagnostic read can prevent cleanup of an independent fixture resource.
- Requested correction: Read each required control once for the settlement verdict, retain missing or unreadable outcomes in isolation, and execute each release and retirement publication independently of those diagnostics.
- Requested evidence: Exercise a missing required same-caster control and a throwing presence read with both controls acquired. Show failed admission, cleanup of the independent control, and exactly one accounted outside-target retirement, including deferred settlement.

**R3-F4** · P1 · defect · blocks readiness — Immediate settlement discards an unreadable result
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1363`; acceptance: R7-F2, R7-F5, OWNER AMENDMENT 1 §§3B,3D
- Trigger: An execution reads ended during the cleanup census and becomes unreadable when the immediate settlement delegate checks it again.
- Expected: Immediate reconciliation either runs to completion or retains an executable cleanup obligation.
- Actual: ReleaseOwnedEffects reads end state in Live(), again while building the execution row, and again inside settle(). When the first reads say ended and the final read is unknown, line 1363 ignores settle()’s result, records ledgerHeldUntilSettlement=false and installs no reconciliation residual or terminator. The caller can report a subscribed ledger with no remaining action to release it.
- Why it blocks / does not: The newly added terminal cleanup only runs for registered residuals. This branch can strand the subscription and control cleanup.
- Requested correction: Use a consistent end-state observation for the immediate path, or retain the cleanup whenever the attempted settlement returns unknown or pending.
- Requested evidence: Use a controlled end-state reader that reports ended and subsequently throws. Show that reconciliation runs exactly once or remains owned through terminal teardown, with an accurate subscription result.

**R1-F9** · P2 · defect · blocks readiness — Baseline placement can still fail outside phase setup reporting
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3594`; acceptance: C3 §5, OWNER AMENDMENT 1 §3C
- Trigger: Ground lookup or hostile translocation throws after the phase subscriptions have succeeded.
- Expected: An unexpected phase setup failure reaches local isolation by operation and survives cleanup of the earlier acquisitions.
- Actual: The subscriptions now have release owners. The next phase setup operation, ground lookup and hostile placement, runs outside S7PhaseAcquire. If it throws, the finalizer can publish an empty isolation failure list and physical restoration true while only the outer poller sees the setup exception.
- Why it blocks / does not: The phase’s own cumulative outcome still omits a setup failure specifically required by the prior finding.
- Requested correction: Include the baseline placement in the phase’s guarded setup and retain its original exception in the phase result.
- Requested evidence: Fail placement after the recorder and save handler subscribe. Show both releases, the placement error in phase isolation and incomplete required cells.

**R2-F8** · P2 · defect · blocks readiness — Early removal failures still depend on exception text to identify the condition
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1538`; acceptance: R4-F1, OWNER AMENDMENT 1 §§3B,3D
- Trigger: A real Buff.Remove throws an exception whose message does not include the buff identity.
- Expected: The early failure records the removal operation, exact condition identity and original exception when it occurs.
- Actual: The early paths now publish returned failures immediately. AcquisitionLedger.Release keeps the failed object privately but returns only exception type and message. The injected fault supplies its own buff identity in the message. An ordinary removal exception can therefore reach the phase with no indication of which buff failed, particularly if that buff later expires.
- Why it blocks / does not: The requested resource identity is still lost for a real removal error independent of the injected fault.
- Requested correction: Attach the affected buff’s identity at the removal boundary and publish that result once. Preserve exact matching for the intentional fault.
- Requested evidence: Make an ordinary removal throw a message without an ID, then fail a later cleanup read. Show both causes in the enclosing outcome, with the removal bound to the correct instance.

**R1-F11** · P2 · evidence_gap · blocks readiness — The focused retirement control supplies the state its caller must prove
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:1834`; acceptance: R4-F1, R7-F2, R7-F4, OWNER AMENDMENT 1 §3E
- Trigger: The focused case is used as evidence that a failed retirement cannot be retried by the outer fixture.
- Expected: Controlled failures traverse the same retirement handoff and fixture teardown decisions used by the real callers. A case claiming an independent read failure observes that failure executing.
- Actual: RetiringObserver still supplies its own direct requests and unwinding sequence. The test constructs the fixture’s attempted list from sources.Requested, while production separately populates fixture.RetirementAttempted in the retire delegate. Omitting that production write would leave the test green. The context-read test also inserts the throwing process after the matching process, so RetainsContext returns before the intended throw.
- Why it blocks / does not: The passing receipt cannot detect a broken retirement handoff or establish the reported throwing-getter case.
- Requested correction: Exercise the production operation that records a fixture retirement attempt before destruction. Use that actual state through outer teardown. Arrange the context-read test so the throwing getter executes before the successful match.
- Requested evidence: Provide a candidate-bound Focused receipt for both direct retirement failure positions showing one request, the original error, independent cleanup, settlement and refusal. Assert that the throwing context getter was called and the known execution was still attributed.

**R1-F13** · P2 · required_gap · blocks readiness — The active integration map still describes the pre-amendment stop
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:325`; acceptance: C3 §8, OWNER AMENDMENT 1 §§3F,7
- Trigger: An owner follows the conditional integration plan and its original-finding-to-evidence map.
- Expected: The active integration plan and finding map identify the source that ran, the evidence for each repair and the findings that remain open.
- Actual: The ancestry checks now use the correct bases and name the integration destination. The active plan still describes candidate 3 as awaiting build and qualification. Its finding reconciliation and rows 415–419 say C3 stopped with R1-F1 through R1-F11 open and cite only the older 6abd692fa7 native run.
- Why it blocks / does not: The handoff map gives a different qualification state from the candidate-3 receipt and the recorded round-2 dispositions.
- Requested correction: Date the earlier stop as history and update the active plan and C3 map to the actual candidate-3 session and current review dispositions.
- Requested evidence: Cross-check the active map against the round-2 ledger and the 20261126T134425Z receipt. Keep the reviewed head, product source and eventual archive distinct.

**R3-F5** · P2 · required_gap · blocks readiness — The shared ledger overstates the remaining allowance
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:507`; acceptance: C3 §§6–8, OWNER AMENDMENT 1 §§6–7
- Trigger: Calculating the budget available for contingency candidate 4.
- Expected: The running total follows the recorded durations and the receipt subtotal agrees with the four controller receipts.
- Actual: 45 min 5.1 s plus 5.9 s is 45 min 11.0 s. The ledger records 45 min 10.0 s and carries the shortfall forward. Using the report’s 17.3 s final batch gives 59 min 37.0 s spent and 15 min 23.0 s remaining. The four receipt durations total 8.9 s, while the report gives 6.9 s.
- Why it blocks / does not: The contingency decision requires an accurate remaining allowance and a clear distinction between wrapper wall time and receipt time.
- Requested correction: Correct the cumulative ledger from that row forward and carry the reconciled batch and receipt figures into the next estimate and report.
- Requested evidence: Reconcile the recorded durations arithmetically. Existing receipts suffice.

#### Prior finding assessments

- **R1-F10** — `fix_verified` — IsolatedCell adds a successful completion only after disposal and completion publication succeed. The Focused case asserts that a throwing writer leaves the cell out of Completed and preserves its publication failure.
- **R1-F11** — `fix_rejected` — The partial-subscription and unpatch controls now fail at the corresponding operations. The retirement control still reconstructs the fixture-attempt state independently of the production caller that must populate it.
- **R1-F13** — `fix_rejected` — C1 now checks against #219’s frozen records head, C2 and C3 check against predecessor archives, and each operation names the parent branch. The active plan and finding map still present superseded build and review status.
- **R1-F5** — `fix_verified` — RollbackSetup retains both count errors alongside independent unpatch failures. Normal trace disposal propagates count errors, and the hook-attempt readback preserves the exception text. The Focused case covers a first read failure followed by zero.
- **R1-F9** — `fix_rejected` — Both Sprint26Saves callers retain the exact handler before subscription, and the Ankylosaurus attack log is acquired inside cleanup scope. Baseline placement still bypasses the requested phase-local setup-error publication.
- **R2-F1** — `fix_verified` — The mechanics session ran the corrected product from 66b0ff3f14 with the identified C2 harness. Its 43 passing checks, seven completed cells, separate restoration result, constructed cue posts, same-run row and transaction readbacks close the prior missing-run finding.
- **R2-F2** — `fix_verified` — Pending reconciliation now has an enclosing caller after driver disposal and a bounded terminal action. The Focused case drives phase settlement alone and observes late removal and one unsubscription. R3-F1 covers the distinct loss of results produced after that transfer.
- **R2-F3** — `fix_verified` — The driver reads readiness immediately after the child step and disposes in the same frame. The native receipt binds the disposal-time and post-settlement observations to execution KMG_Summoning_Special_DustMephit_Breath#-376666880 and records the late delivery.
- **R2-F4** — `fix_verified` — For the original ordering defect, both acquired foreign references survive the ordinary owned release and are checked before fixture cleanup. The native row records both present, and the caster assertion requires an explicit true result. R3-F3 covers missing handles and exceptions in the new readback path.
- **R2-F5** — `fix_verified` — The normal Ankylosaurus retirement and Dust source-loss retirement now enter the phase’s one-request record. The outside target also receives a recorded request and settlement check. The native receipt shows 19 requests and identifies the outside target’s eventual settlement. R1-F11 covers the remaining test gap.
- **R2-F6** — `fix_verified` — EndedOf uses the bound process after the command field clears. Delivery scans retained contexts first and guards each lookup, so an independently known execution remains reachable. R1-F11 identifies the unexecuted throwing-getter assertion, and R3-F2 covers a delivery whose own process becomes identifiable later.
- **R2-F7** — `fix_rejected` — A failure on one actor now allows later actors to run, and the fixture caster and hostile receive combat-exit handling. The unconditional interrupt predicate on each summon still selects foreign commands, and an interruption failure skips that actor’s remaining combat cleanup.
- **R2-F8** — `fix_rejected` — Both early release sites now publish immediately, so a later diagnostic cannot erase the error text. The result still loses the requested exact condition identity when the engine exception omits it.
- **R2-F9** — `fix_verified` — The runner selects the profile-specific Dust expectation. The mechanics run records framingMeasured=false and each cast as uncaptured. The presentation profile retains its framing assertion.

#### Evidence assessment

- **Inspected:** Controller mission and OWNER AMENDMENT 1 with matching SHA-256 digests, prior-finding ledger, developer report, transcript notice, gate receipts and logs.; AGENTS.md, docs/VALIDATION-POLICY.md, docs/AGENT-REVIEW-WORKFLOW.md, the M..H feature diff and the corrections since round-2 H.; Effects profile dispatch, phase and fixture teardown, Dust command and condition ownership, hook acquisition and rollback, abandonment and settlement, focused tests, both session selections and the recovery manifest.; The retained 20261126T051633Z failure, the owner-accepted 20261126T054228Z evidence and the corrected-byte 20261126T134425Z launch, session and callback records.
- **Executed verification:** Read-only Git checks confirmed H and its merge base. The complete supplied feature diff and the Git-generated diff have the same blob hash, 178884231f17e34cd463e90fca032b04d565c60f. Only tests and records differ between packaged source 66b0ff3f14efdd29c80f5f9091bec3521eacfc6d and H.; All four controller receipts identify H, exit code 0 and an unchanged worktree. Their log hashes match. Production compilation passed and the filters ran 25 sprint7-visual, 11 summon-playtest-r6, 9 native-profile and 5 loader-compat-probe tests. The 50 selected executions had zero failures.; The committed native extracts for 20261126T134425Z record 43/43 checks on package 473f2f5a78229f3e2e15c3d97b863f2201f44ae372d2485de1fdb05010618366, DLL 8e574477cce11621ca2ec88fdeed358cb9f8f76ff8ced423e154e3c86d277d07 and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The seven declared, admitted and completed cell lists match. Physical restoration passed separately. All five drivers reported their expected outcomes.; The native pre-delivery row identifies the same execution at disposal in frame 11195 and after settlement. It records zero deliveries at disposal, one attributable Sickened acquired afterward, one removal, zero still present and an ended subscription. The ordinary Dust row records both foreign controls present after owned release.; Constructed production-handler delivery posted IDs 534 and 535 in frame 5301 with both playing. I compared the complete committed callback row with the row in the launch extract and checked their source-hash attribution. The H-bound combined-run-verdicts-agree test passed for the persisted and collector-shaped forms of the recorded null live verdict.; Transaction 515e592a9d884068bc0b951befe3f61a reports success, unchanged owner installation and saves, and matching settings-registry backup and readback hashes. The fixture receipt records exact party and unit references. The mechanics extract has three uncaptured casts and no native-look or listening-sample rows.
- **Source inspection only:** The late-publication, recovered-execution, control-read, immediate-settlement and mixed-command failure paths below were traced in production source. The successful native receipt contains no instance of those failures.; The source confirms which operations the passing focused controls call, including where a test constructs its own fixture-attempt list or returns before reaching a throwing getter.
- **Required gaps:** Focused evidence through the actual retirement-to-fixture handoff, command-specific teardown, missing or unreadable controls, delayed execution attribution, and failures generated by transferred cleanup.; Consistent current finding/evidence records and reconciled validation totals before using the remaining contingency.; Qualification of any subsequently changed compiled bytes under the existing amended ceilings.
- **Limitations:** The review used the detached read-only snapshot. Build and game results come from the recorded receipts. The foreground transcript withholds the full developer shell log, so the outer command wall times are reported measurements.; All five native drivers completed with empty transferredResiduals arrays. The run establishes the observed engine path. Disposal during a recovery wait and failures after ownership transfer depend on focused controls and source inspection.; C1’s accepted native criterion belongs to session 20261126T054228Z on source 6abd692fa7. C2’s accepted lifecycle result belongs to session 20261126T033419Z on the parent candidate-3 package and C2 harness.

#### Summary

- **Delivered behavior:** Candidate 3 demonstrates same-frame pre-delivery disposal, late Sickened removal, hook rollback controls, constructed two-source cue overlap and agreement of live and persisted verdicts in the enclosing mechanics runner. The successful run and restoration receipts are valid for those bytes. The unresolved ownership and reporting defects prevent final handoff.
- **Scope reviewed:** Final C3 checkpoint for draft PR #223 against frozen C2 archive eab90a5ac8, including all 14 pending findings, the corrected callers and evidence required by OWNER AMENDMENT 1.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Visual acceptance for T2, T3 and T6 and listening choices for T5 and T7 using the existing materials.; Any later stack integration, parent-review resumption, master integration or release decision.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 1, sha256 `c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233`.

Codex CLI `0.170.0`, thread `01a21822-1638-7480-b3a2-c04a42a65b60`, duration 1072.0s, response sha256 `7fe59f087752ff1f14712bcc4625c2f0d01268b37e08c60dcd54c16204079760`, request `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r03-db7304`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "f0ca78cbab9b57583379a575208a15304aae7eb1",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "Recovery candidate 3 passed the mechanics session. All seven required cells completed, and the receipt shows an actual Dust delivery after disposal. Failure paths still allow an attributable condition to escape cleanup, a transferred cleanup error to miss the phase result, and a foreign command to be interrupted. These defects block the ownership handoff.\n\nContinuation for Claude\n\nKeep the corrections within C3\u2019s effects runner, focused tests and recovery records. Give deferred cleanup a live destination for every failure and residual it produces. Reconcile Dust conditions against recovered execution identities, preserve cleanup ownership when a settlement read is uncertain, and make foreign-control diagnostics independent of release. Finish command-specific teardown, phase setup reporting and condition-failure identification. Drive the failing operations through the production callers identified in the findings.\n\nUpdate the finding map and reconcile the validation ledger before the next expensive command. The reported durations give 15 min 23.0 s remaining through the round-3 gates. Under OWNER AMENDMENT 1, estimate the affected Focused checks, contingency build, mechanics session and final gates together. If they fit, qualify the corrected compiled bytes with candidate 4 and preserve the same-run verdict, required-cell, physical-restoration and transaction evidence. If they cannot fit, record the concrete stop in the existing enrollment. Preserve the frozen parent, the accepted C1 and C2 evidence identities, and the existing visual and listening materials.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git checks confirmed H and its merge base. The complete supplied feature diff and the Git-generated diff have the same blob hash, 178884231f17e34cd463e90fca032b04d565c60f. Only tests and records differ between packaged source 66b0ff3f14efdd29c80f5f9091bec3521eacfc6d and H.",
      "All four controller receipts identify H, exit code 0 and an unchanged worktree. Their log hashes match. Production compilation passed and the filters ran 25 sprint7-visual, 11 summon-playtest-r6, 9 native-profile and 5 loader-compat-probe tests. The 50 selected executions had zero failures.",
      "The committed native extracts for 20261126T134425Z record 43/43 checks on package 473f2f5a78229f3e2e15c3d97b863f2201f44ae372d2485de1fdb05010618366, DLL 8e574477cce11621ca2ec88fdeed358cb9f8f76ff8ced423e154e3c86d277d07 and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The seven declared, admitted and completed cell lists match. Physical restoration passed separately. All five drivers reported their expected outcomes.",
      "The native pre-delivery row identifies the same execution at disposal in frame 11195 and after settlement. It records zero deliveries at disposal, one attributable Sickened acquired afterward, one removal, zero still present and an ended subscription. The ordinary Dust row records both foreign controls present after owned release.",
      "Constructed production-handler delivery posted IDs 534 and 535 in frame 5301 with both playing. I compared the complete committed callback row with the row in the launch extract and checked their source-hash attribution. The H-bound combined-run-verdicts-agree test passed for the persisted and collector-shaped forms of the recorded null live verdict.",
      "Transaction 515e592a9d884068bc0b951befe3f61a reports success, unchanged owner installation and saves, and matching settings-registry backup and readback hashes. The fixture receipt records exact party and unit references. The mechanics extract has three uncaptured casts and no native-look or listening-sample rows."
    ],
    "inspected": [
      "Controller mission and OWNER AMENDMENT 1 with matching SHA-256 digests, prior-finding ledger, developer report, transcript notice, gate receipts and logs.",
      "AGENTS.md, docs/VALIDATION-POLICY.md, docs/AGENT-REVIEW-WORKFLOW.md, the M..H feature diff and the corrections since round-2 H.",
      "Effects profile dispatch, phase and fixture teardown, Dust command and condition ownership, hook acquisition and rollback, abandonment and settlement, focused tests, both session selections and the recovery manifest.",
      "The retained 20261126T051633Z failure, the owner-accepted 20261126T054228Z evidence and the corrected-byte 20261126T134425Z launch, session and callback records."
    ],
    "limitations": [
      "The review used the detached read-only snapshot. Build and game results come from the recorded receipts. The foreground transcript withholds the full developer shell log, so the outer command wall times are reported measurements.",
      "All five native drivers completed with empty transferredResiduals arrays. The run establishes the observed engine path. Disposal during a recovery wait and failures after ownership transfer depend on focused controls and source inspection.",
      "C1\u2019s accepted native criterion belongs to session 20261126T054228Z on source 6abd692fa7. C2\u2019s accepted lifecycle result belongs to session 20261126T033419Z on the parent candidate-3 package and C2 harness."
    ],
    "required_gaps": [
      "Focused evidence through the actual retirement-to-fixture handoff, command-specific teardown, missing or unreadable controls, delayed execution attribution, and failures generated by transferred cleanup.",
      "Consistent current finding/evidence records and reconciled validation totals before using the remaining contingency.",
      "Qualification of any subsequently changed compiled bytes under the existing amended ceilings."
    ],
    "source_inspection_only": [
      "The late-publication, recovered-execution, control-read, immediate-settlement and mixed-command failure paths below were traced in production source. The successful native receipt contains no instance of those failures.",
      "The source confirms which operations the passing focused controls call, including where a test constructs its own fixture-attempt list or returns before reaching a throwing getter."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R7-F2",
        "R7-F5",
        "OWNER AMENDMENT 1 \u00a73D"
      ],
      "actual": "TransferPendingTo moves the delegates. ReleaseOwnedEffects and the Dust afterSettlement callback still close over the child isolation. A later unsubscribe or foreign-control failure goes into Child.Failures after the driver has copied Unexpected to the phase. A newly pending outside-target retirement also goes into the departed child after its only transfer. The phase can record the reconciliation as settled when the condition list is empty.",
      "blocking_rationale": "The original driver failure remains visible, but the phase loses the later failure identity and can lose the obligation to observe the target\u2019s retirement.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The phase receives all failures and new residuals produced while it executes transferred cleanup.",
      "id": "R3-F1",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 760
      },
      "priority": "P1",
      "requested_correction": "Carry the result destination with the transferred work, or keep consuming the child\u2019s subsequent failures and residuals until reconciliation finishes.",
      "requested_evidence": "Dispose the driver during its wait, then make unsubscription fail or outside-target retirement remain pending during phase settlement. Show the exact late result in the phase alongside the original driver failure.",
      "title": "Transferred cleanup still publishes to the departed child",
      "trigger": "The driver exits with reconciliation pending, and reconciliation produces a failure or another residual after the phase takes ownership."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a73B"
      ],
      "actual": "The delivery handler skips unreadable getters and forgets the unmatched context. EndedOf can subsequently recover the exact process, but S7DustConditions.Owned checks only contexts saved during delivery. Release, RemainingIdentities and PresentFrom can all exclude a still-present buff whose SourceAbilityContext matches the recovered process.",
      "blocking_rationale": "Execution settlement and an empty ledger can admit another cell while an attributable Sickened remains.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Once the exact process becomes readable, settlement reconciles present conditions attributable to its context and preserves pre-existing and foreign instances.",
      "id": "R3-F2",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 2561
      },
      "priority": "P1",
      "requested_correction": "Reconcile missed delivery attribution against retained process contexts when their identities become available. Keep genuinely unresolved attribution visible to admission.",
      "requested_evidence": "Deliver a condition while its command\u2019s process is unreadable, then recover and end that process. Show removal of the matching condition, survival of a foreign condition, and the resulting admission decision.",
      "title": "Recovering a process does not recover a delivery missed while it was unreadable",
      "trigger": "Both delivery notifications occur while the owned process lookup is unreadable, and its exact identity becomes readable before settlement."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "OWNER AMENDMENT 1 \u00a73B"
      ],
      "actual": "Teardown now visits later actors after an actor fails, but still calls InterruptAll(command => true) on each surviving summon. The Dust cell explicitly runs a separate owner\u2019s command on such a summon at line 3191. An interruption exception also skips the queue update and combat exit for that actor. The focused case uses actor names and an unlisted foreign actor.",
      "blocking_rationale": "Owning the summon does not establish ownership of every queued command. Teardown can cancel the foreign control and skip independent cleanup.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Only identified phase commands are interrupted, and independent command and combat releases continue after a failure.",
      "id": "R2-F7",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3634
      },
      "priority": "P1",
      "requested_correction": "Use the exact commands issued by the effects runner during teardown and guard independent combat releases separately.",
      "requested_evidence": "Through the production teardown operation, make one interruption throw and give a later actor both an owned and a foreign command. Show the owned release, untouched foreign command, attempted combat cleanup and named failure.",
      "title": "Phase teardown still interrupts every command on an owned summon",
      "trigger": "The phase unwinds while a summon has another owner\u2019s queued command, or an interruption on that summon throws."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "R7-F4",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,5"
      ],
      "actual": "The control list omits a required instance when its reference is null. The ordinary Dust assertion then fails, but Sprint25Check only records the assertion and the isolation guard can admit the next cell. For acquired controls, lines 3290\u20133291 repeat the presence reads outside ControlsKept. A throw there exits before either control release, outside-target retirement or retirement publication. The separately spawned outside target has no entry in the phase\u2019s owned-summon list.",
      "blocking_rationale": "A missing observation can bypass admission, and a diagnostic read can prevent cleanup of an independent fixture resource.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each control required by the running path has a named presence result. All independent releases and retirement publication run even when one result is unreadable.",
      "id": "R3-F3",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3290
      },
      "priority": "P1",
      "requested_correction": "Read each required control once for the settlement verdict, retain missing or unreadable outcomes in isolation, and execute each release and retirement publication independently of those diagnostics.",
      "requested_evidence": "Exercise a missing required same-caster control and a throwing presence read with both controls acquired. Show failed admission, cleanup of the independent control, and exactly one accounted outside-target retirement, including deferred settlement.",
      "title": "Foreign-control diagnostics can bypass admission or skip independent releases",
      "trigger": "The ordinary Dust control cannot identify the expected same-caster buff, or a control presence read throws during settlement."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "R7-F5",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,3D"
      ],
      "actual": "ReleaseOwnedEffects reads end state in Live(), again while building the execution row, and again inside settle(). When the first reads say ended and the final read is unknown, line 1363 ignores settle()\u2019s result, records ledgerHeldUntilSettlement=false and installs no reconciliation residual or terminator. The caller can report a subscribed ledger with no remaining action to release it.",
      "blocking_rationale": "The newly added terminal cleanup only runs for registered residuals. This branch can strand the subscription and control cleanup.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Immediate reconciliation either runs to completion or retains an executable cleanup obligation.",
      "id": "R3-F4",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1363
      },
      "priority": "P1",
      "requested_correction": "Use a consistent end-state observation for the immediate path, or retain the cleanup whenever the attempted settlement returns unknown or pending.",
      "requested_evidence": "Use a controlled end-state reader that reports ended and subsequently throws. Show that reconciliation runs exactly once or remains owned through terminal teardown, with an accurate subscription result.",
      "title": "Immediate settlement discards an unreadable result",
      "trigger": "An execution reads ended during the cleanup census and becomes unreadable when the immediate settlement delegate checks it again."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75",
        "OWNER AMENDMENT 1 \u00a73C"
      ],
      "actual": "The subscriptions now have release owners. The next phase setup operation, ground lookup and hostile placement, runs outside S7PhaseAcquire. If it throws, the finalizer can publish an empty isolation failure list and physical restoration true while only the outer poller sees the setup exception.",
      "blocking_rationale": "The phase\u2019s own cumulative outcome still omits a setup failure specifically required by the prior finding.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An unexpected phase setup failure reaches local isolation by operation and survives cleanup of the earlier acquisitions.",
      "id": "R1-F9",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3594
      },
      "priority": "P2",
      "requested_correction": "Include the baseline placement in the phase\u2019s guarded setup and retain its original exception in the phase result.",
      "requested_evidence": "Fail placement after the recorder and save handler subscribe. Show both releases, the placement error in phase isolation and incomplete required cells.",
      "title": "Baseline placement can still fail outside phase setup reporting",
      "trigger": "Ground lookup or hostile translocation throws after the phase subscriptions have succeeded."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "OWNER AMENDMENT 1 \u00a7\u00a73B,3D"
      ],
      "actual": "The early paths now publish returned failures immediately. AcquisitionLedger.Release keeps the failed object privately but returns only exception type and message. The injected fault supplies its own buff identity in the message. An ordinary removal exception can therefore reach the phase with no indication of which buff failed, particularly if that buff later expires.",
      "blocking_rationale": "The requested resource identity is still lost for a real removal error independent of the injected fault.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The early failure records the removal operation, exact condition identity and original exception when it occurs.",
      "id": "R2-F8",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1538
      },
      "priority": "P2",
      "requested_correction": "Attach the affected buff\u2019s identity at the removal boundary and publish that result once. Preserve exact matching for the intentional fault.",
      "requested_evidence": "Make an ordinary removal throw a message without an ID, then fail a later cleanup read. Show both causes in the enclosing outcome, with the removal bound to the correct instance.",
      "title": "Early removal failures still depend on exception text to identify the condition",
      "trigger": "A real Buff.Remove throws an exception whose message does not include the buff identity."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "R7-F4",
        "OWNER AMENDMENT 1 \u00a73E"
      ],
      "actual": "RetiringObserver still supplies its own direct requests and unwinding sequence. The test constructs the fixture\u2019s attempted list from sources.Requested, while production separately populates fixture.RetirementAttempted in the retire delegate. Omitting that production write would leave the test green. The context-read test also inserts the throwing process after the matching process, so RetainsContext returns before the intended throw.",
      "blocking_rationale": "The passing receipt cannot detect a broken retirement handoff or establish the reported throwing-getter case.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Controlled failures traverse the same retirement handoff and fixture teardown decisions used by the real callers. A case claiming an independent read failure observes that failure executing.",
      "id": "R1-F11",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 1834
      },
      "priority": "P2",
      "requested_correction": "Exercise the production operation that records a fixture retirement attempt before destruction. Use that actual state through outer teardown. Arrange the context-read test so the throwing getter executes before the successful match.",
      "requested_evidence": "Provide a candidate-bound Focused receipt for both direct retirement failure positions showing one request, the original error, independent cleanup, settlement and refusal. Assert that the throwing context getter was called and the known execution was still attributed.",
      "title": "The focused retirement control supplies the state its caller must prove",
      "trigger": "The focused case is used as evidence that a failed retirement cannot be retried by the outer fixture."
    },
    {
      "acceptance_ids": [
        "C3 \u00a78",
        "OWNER AMENDMENT 1 \u00a7\u00a73F,7"
      ],
      "actual": "The ancestry checks now use the correct bases and name the integration destination. The active plan still describes candidate 3 as awaiting build and qualification. Its finding reconciliation and rows 415\u2013419 say C3 stopped with R1-F1 through R1-F11 open and cite only the older 6abd692fa7 native run.",
      "blocking_rationale": "The handoff map gives a different qualification state from the candidate-3 receipt and the recorded round-2 dispositions.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The active integration plan and finding map identify the source that ran, the evidence for each repair and the findings that remain open.",
      "id": "R1-F13",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 325
      },
      "priority": "P2",
      "requested_correction": "Date the earlier stop as history and update the active plan and C3 map to the actual candidate-3 session and current review dispositions.",
      "requested_evidence": "Cross-check the active map against the round-2 ledger and the 20261126T134425Z receipt. Keep the reviewed head, product source and eventual archive distinct.",
      "title": "The active integration map still describes the pre-amendment stop",
      "trigger": "An owner follows the conditional integration plan and its original-finding-to-evidence map."
    },
    {
      "acceptance_ids": [
        "C3 \u00a7\u00a76\u20138",
        "OWNER AMENDMENT 1 \u00a7\u00a76\u20137"
      ],
      "actual": "45 min 5.1 s plus 5.9 s is 45 min 11.0 s. The ledger records 45 min 10.0 s and carries the shortfall forward. Using the report\u2019s 17.3 s final batch gives 59 min 37.0 s spent and 15 min 23.0 s remaining. The four receipt durations total 8.9 s, while the report gives 6.9 s.",
      "blocking_rationale": "The contingency decision requires an accurate remaining allowance and a clear distinction between wrapper wall time and receipt time.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The running total follows the recorded durations and the receipt subtotal agrees with the four controller receipts.",
      "id": "R3-F5",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 507
      },
      "priority": "P2",
      "requested_correction": "Correct the cumulative ledger from that row forward and carry the reconciled batch and receipt figures into the next estimate and report.",
      "requested_evidence": "Reconcile the recorded durations arithmetically. Existing receipts suffice.",
      "title": "The shared ledger overstates the remaining allowance",
      "trigger": "Calculating the budget available for contingency candidate 4."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F10",
      "rationale": "IsolatedCell adds a successful completion only after disposal and completion publication succeed. The Focused case asserts that a throwing writer leaves the cell out of Completed and preserves its publication failure."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F11",
      "rationale": "The partial-subscription and unpatch controls now fail at the corresponding operations. The retirement control still reconstructs the fixture-attempt state independently of the production caller that must populate it."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F13",
      "rationale": "C1 now checks against #219\u2019s frozen records head, C2 and C3 check against predecessor archives, and each operation names the parent branch. The active plan and finding map still present superseded build and review status."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F5",
      "rationale": "RollbackSetup retains both count errors alongside independent unpatch failures. Normal trace disposal propagates count errors, and the hook-attempt readback preserves the exception text. The Focused case covers a first read failure followed by zero."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F9",
      "rationale": "Both Sprint26Saves callers retain the exact handler before subscription, and the Ankylosaurus attack log is acquired inside cleanup scope. Baseline placement still bypasses the requested phase-local setup-error publication."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F1",
      "rationale": "The mechanics session ran the corrected product from 66b0ff3f14 with the identified C2 harness. Its 43 passing checks, seven completed cells, separate restoration result, constructed cue posts, same-run row and transaction readbacks close the prior missing-run finding."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F2",
      "rationale": "Pending reconciliation now has an enclosing caller after driver disposal and a bounded terminal action. The Focused case drives phase settlement alone and observes late removal and one unsubscription. R3-F1 covers the distinct loss of results produced after that transfer."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F3",
      "rationale": "The driver reads readiness immediately after the child step and disposes in the same frame. The native receipt binds the disposal-time and post-settlement observations to execution KMG_Summoning_Special_DustMephit_Breath#-376666880 and records the late delivery."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F4",
      "rationale": "For the original ordering defect, both acquired foreign references survive the ordinary owned release and are checked before fixture cleanup. The native row records both present, and the caster assertion requires an explicit true result. R3-F3 covers missing handles and exceptions in the new readback path."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F5",
      "rationale": "The normal Ankylosaurus retirement and Dust source-loss retirement now enter the phase\u2019s one-request record. The outside target also receives a recorded request and settlement check. The native receipt shows 19 requests and identifies the outside target\u2019s eventual settlement. R1-F11 covers the remaining test gap."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F6",
      "rationale": "EndedOf uses the bound process after the command field clears. Delivery scans retained contexts first and guards each lookup, so an independently known execution remains reachable. R1-F11 identifies the unexecuted throwing-getter assertion, and R3-F2 covers a delivery whose own process becomes identifiable later."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R2-F7",
      "rationale": "A failure on one actor now allows later actors to run, and the fixture caster and hostile receive combat-exit handling. The unconditional interrupt predicate on each summon still selects foreign commands, and an interruption failure skips that actor\u2019s remaining combat cleanup."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R2-F8",
      "rationale": "Both early release sites now publish immediately, so a later diagnostic cannot erase the error text. The result still loses the requested exact condition identity when the engine exception omits it."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F9",
      "rationale": "The runner selects the profile-specific Dust expectation. The mechanics run records framingMeasured=false and each cast as uncaptured. The presentation profile retains its framing assertion."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r03-db7304",
  "round": 3,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "Candidate 3 demonstrates same-frame pre-delivery disposal, late Sickened removal, hook rollback controls, constructed two-source cue overlap and agreement of live and persisted verdicts in the enclosing mechanics runner. The successful run and restoration receipts are valid for those bytes. The unresolved ownership and reporting defects prevent final handoff.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Visual acceptance for T2, T3 and T6 and listening choices for T5 and T7 using the existing materials.",
      "Any later stack integration, parent-review resumption, master integration or release decision."
    ],
    "scope_reviewed": "Final C3 checkpoint for draft PR #223 against frozen C2 archive eab90a5ac8, including all 14 pending findings, the corrected callers and evidence required by OWNER AMENDMENT 1.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
