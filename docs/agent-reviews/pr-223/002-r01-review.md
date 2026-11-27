<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r01-review part=1/1 sha256=da58b690fff4b5212707f6640ef4360ab1bea27f0900ba60325e7e75758d4621 -->
### Codex GPT-6.2 Astra — review — round 1 — candidate `7fff8b34f6`

**Decision:** `changes_required` — The combined run establishes C1’s live and persisted verdict agreement. C3 still has failure paths where teardown repeats a retirement, loses a command-release error, or admits later work with owned effects unaccounted for. The 41 passing checks did not exercise those paths.

## Continuation for Claude

Keep one ownership record from acquisition through cell disposal, phase teardown and the final verdict. Carry early command failures forward, reconcile effects from retained Dust executions before reuse, and attempt hook rollback independently of diagnostic reads. Make the abandonment driver consume the complete child result and preserve the outcome of its own repair on disposal. Exercise the production cleanup path with controlled failing operations, including both direct retirements and the paired Dust fault. Record the qualification scope and the conditional integration plan for the owner.

The shared ledger reports 42m28.9s spent, 2m31.1s remaining and both authorized product candidates used. The required RuntimeTesting corrections change the DLL. Preserve the existing receipts and report the native qualification stop for any corrected bytes. A further package requires new owner authority. Keep the current enrollment, frozen prerequisites, accepted C1/C2 scopes and historical failures intact.

#### Findings

**R1-F1** · P1 · defect · blocks readiness — Phase teardown can repeat a failed cue-source retirement
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3334`; acceptance: R7-F4, C3 §5
- Trigger: Either direct callback-source retirement throws or returns with the unit still alive.
- Expected: Each cue source has one retirement attempt across all teardown layers, a named result and an explicit settlement observation. Every other owned release is attempted.
- Actual: Sprint7OwnedRequests is local to S7CallbackLoss. Phase teardown passes every still-live entry in owned to DisposeExpandedSummoningUnits again. If the first cleanup threw before fixture.Created.Remove, fixture teardown can also revisit the source. The bulk cleanup stops at the first exception, before later units and the phase result are recorded. RequestRecorded also increments sourcesRetiredHere when the request throws.
- Why it blocks / does not: The one-request guarantee ends at the observer boundary. A failing retirement can be repeated and can prevent independent cleanup.
- Requested correction: Carry per-source attempt and settlement state into phase and fixture teardown. Release outstanding units independently, distinguish attempts from observed retirements, and publish the final result even when a release throws.
- Requested evidence: Exercise each direct source retirement throwing while that source remains alive, through final teardown. Show one attempt for the failed source, the original error, independent releases, settlement by identity and failed isolation.

**R1-F2** · P1 · defect · blocks readiness — Dust stops observing conditions while an owned execution can still deliver them
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3101`; acceptance: R7-F2, C3 §5
- Trigger: The ordinary Dust iterator is disposed after execution starts and before its first effect is delivered.
- Expected: An ordinary cell disposal accounts for effects delivered by its exact retained executions through settlement, including effects delivered after command interruption.
- Actual: S7Dust releases currently visible conditions and unsubscribes the ledger before waiting for retained processes. Its residual subsequently reads only IsEnded. A Sickened application delivered after unsubscribe is neither acquired nor released. The native pre-delivery check keeps the ledger subscribed during its wait and explicitly clears conditions afterward.
- Why it blocks / does not: The next cell can start with a condition left by the previous cell even though its retained execution has ended.
- Requested correction: Reconcile conditions attributable to the retained process at settlement before permitting reuse. Preserve foreign conditions and keep isolation failed when the owned effects cannot be accounted for.
- Requested evidence: Dispose the enclosing ordinary Dust cell before delivery, deliver an owned condition while its process is pending, and verify removal of that instance, survival of a foreign control and the admission decision.

**R1-F3** · P1 · defect · blocks readiness — An early command-release failure reaches a check but misses phase isolation
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3005`; acceptance: R4-F1, R7-F2
- Trigger: The pre-delivery release throws and the command later finishes or is successfully released.
- Expected: Every failed release attempt immediately enters the cell’s cumulative isolation result and survives a later successful cleanup.
- Actual: The pre-delivery call to EndUnfinished stores its failures only in preFailures and a diagnostic check. The finalizer starts a fresh commandFailures list. If the command finishes before final cleanup, that pass returns no error. Sprint25Check records the earlier failed check but does not fail the isolation used by Sequence.
- Why it blocks / does not: A named release exception can disappear from admission and phase restoration after the visible command settles.
- Requested correction: Publish early and final command-release failures through the same cumulative owner before yielding again.
- Requested evidence: Make the early interruption or queue update throw, then let the command finish. Verify that the original failure persists, the next phase cell is refused and final isolation fails.

**R1-F4** · P1 · defect · blocks readiness — Some Dust interruptions can clear a process before ownership is established
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:842`; acceptance: R7-F2
- Trigger: Process lookup throws, or a nominally pre-execution cancellation runs after the engine has assigned ExecutionProcess.
- Expected: Every owned interruption preserves the current process or records an unreadable process by command identity. A genuinely unstarted command can still have a known null process.
- Actual: Capture catches a processOf exception and substitutes null. ReleaseEach can then interrupt the command and lose its only process reference without recording the unreadable acquisition. The ordinary Dust cancellation at line 2814 also calls InterruptAll directly after yielding a frame, bypassing capture if execution has already begun.
- Why it blocks / does not: An unobserved live process can be omitted from the residuals that govern admission.
- Requested correction: Route the ordinary cancellation through the command owner and carry capture errors into its cumulative result while continuing independent releases.
- Requested evidence: Exercise an unreadable process accessor followed by field clearing, and a cancellation where execution starts during the yielded frame. Verify the exact process or named unknown, independent command release and the resulting guard decision.

**R1-F5** · P1 · defect · blocks readiness — An unreadable Harmony count can prevent rollback and replace the setup error
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:2059`; acceptance: R7-F3
- Trigger: Harmony inspection fails after a callback or projectile hook has partially installed, or while a repair is being assembled.
- Expected: Attempt rollback for each known attempted hook by owner identity. Preserve the setup error, each rollback error and explicit known or unreadable readbacks independently.
- Actual: The projectile constructor reads OwnedHooks before Unhook and again before preserving the setup exception. A read failure can skip rollback or replace the original error. Callback Unhook also gates the release on Hooks(), and projectile Repair reads all counts while assembling its release list. One unreadable target can therefore skip even an already identified release. HookCount additionally returns zero for a listed method whose patch information is null.
- Why it blocks / does not: The requested unreadable-rollback path can leave installed hooks, lose the setup cause, or report an ambiguous read as zero.
- Requested correction: Decouple release attempts from diagnostic hook counts in construction, normal disposal and outer repair. Treat unavailable patch information as unknown and retain both the original and secondary errors.
- Requested evidence: At the shared rollback boundary, make a hook count fail before and after an unpatch. Show every applicable target was attempted, the original setup error survives, and an unreadable result prevents clean isolation.

**R1-F6** · P1 · defect · blocks readiness — The abandonment driver can accept an incomplete child outcome
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1014`; acceptance: R4-F1, C3 §5
- Trigger: The injected fault is present alongside an unreported owner or another failure sharing part of its diagnostic text.
- Expected: Recovery exempts only the injected operation and its exact known residual. Every owner must report, and every independent error must reach the phase.
- Actual: Proven checks pending residuals but omits Child.Unreported. Unexpected and ExpectedMissing use substring matching on whole failure messages. For example, the expected count text ending in 'remaining: 1' also accepts 'remaining: 10'. An independent error containing an expected marker can likewise be exempted. The Dust safety readback does not check that exact remaining count.
- Why it blocks / does not: Repairing the intended injected condition can still produce a successful parent verdict with an incomplete or independently failed child.
- Requested correction: Use the complete child outcome. Bind allowed failures to the operation, identity and expected value, preserve independent components of aggregated errors, and require the unreported-owner list to be empty.
- Requested evidence: Run the driver with an unreported owner alongside the intended fault, with a different residual count, and with an independent command error containing an expected phrase. Each must refuse the later phase cell.

**R1-F7** · P2 · defect · blocks readiness — Disposal during recovery omits the result of a nonthrowing failed repair
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1021`; acceptance: R7-F5
- Trigger: The Dust driver is disposed during its wait and removing the injected condition returns without establishing disappearance.
- Expected: Every exit records the attempt and available result for the exact injected condition, including a remaining or unreadable instance.
- Actual: On unwind, RepairOnce calls the removal and records thrown errors. The exact leftover count and disappearance check live only in Unsafe on the ordinary path. If Remove returns with the buff still present, the static list is cleared, repairFailures can be empty and the phase records only a generic incomplete proof.
- Why it blocks / does not: The driver does attempt repair and fails the phase, but its disposal record loses a failed or unverified repair of the exact injected condition.
- Requested correction: Publish the exact repair result from the unwind path as well. A stopped scenario can retain a named unresolved result without retrying the removal or advancing the fixture.
- Requested evidence: Dispose during the recovery wait with a removal that returns while the exact buff remains. Show one attempt, the remaining or unknown result, the original child fault and the untouched foreign effect.

**R1-F8** · P1 · defect · blocks readiness — Final settlement can turn a refused phase into a passing result
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:3337`; acceptance: C3 §5, R4-F1
- Trigger: A residual outlives the next-cell admission window and settles after the sequence has stopped.
- Expected: Record eventual physical restoration and whether the required phase sequence completed. A refused required cell must keep the qualification incomplete.
- Actual: Sequence records Refused and exits on an unsettled residual. ReviewSprint7Effects writes a stopped string, settles again and bases the final assertion on the current Restored value. Restored omits Refused. If the residual clears in the final wait or teardown, the phase can pass with required later cells skipped. The enclosing product verdict counts only assertions that were emitted.
- Why it blocks / does not: A partial run can be presented as successful qualification even when the real guard refused later work.
- Requested correction: Make the final phase verdict consume the admission outcome, including a required cell that was refused or skipped. Preserve the separate later recovery observation.
- Requested evidence: Keep a residual live through the admission deadline, settle it during final restoration, and show that the unit or process is restored while the incomplete phase still fails.

**R1-F9** · P1 · defect · blocks readiness — Effects setup can throw before its acquired subscriptions are protected
- Location: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs:2583`; acceptance: C3 §5
- Trigger: A later setup operation fails after the save log or phase recorder has subscribed.
- Expected: The cleanup scope owns each acquisition attempt before a later setup operation can throw, including partially successful subscriptions.
- Actual: S7Dust subscribes saveLog and constructs the self-subscribing condition ledger before entering its cleanup try. A failure reading Present() or subscribing the ledger strands the already-acquired save log. ReviewSprint7Effects similarly mutates hostile hit points and subscribes the recorder before constructing the save handler and entering its phase try.
- Why it blocks / does not: Recording a setup exception in the cell runner cannot release a subscription for which the cleanup scope never started.
- Requested correction: Establish cleanup ownership before the first mutation or subscription in the affected cell and enclosing phase. Retain a releasable identity across partial subscription failure and publish the setup and cleanup outcomes.
- Requested evidence: Inject failure after the first subscription and during the next acquisition. Verify independent unsubscription and restoration, preservation of the original error and failed admission.

**R1-F10** · P2 · defect · blocks readiness — Cell completion can report success after disposal failed
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1079`; acceptance: C3 §5
- Trigger: Disposal is the cell’s only exception, or producing its completion record throws.
- Expected: Each observable cell or runner exception reaches isolation, and the completion record reflects any disposal failure.
- Actual: IsolatedCell records a Dispose exception in isolation but still passes a null failure to completed, which can emit a successful cell check. Reading steps.Current and invoking completed are outside the exception capture. The production completion callback writes a progress receipt, so a reporting exception can escape without reaching the sticky isolation result.
- Why it blocks / does not: The runner can publish conflicting completion and isolation results or lose an unexpected runner failure from the final phase record.
- Requested correction: Carry disposal failures into the completion result and record failures from reading the current step or publishing completion before they leave the runner.
- Requested evidence: Use a throwing disposer and a throwing completion callback. Confirm the recorded cell result, named isolation failure and final phase decision agree.

**R1-F11** · P2 · evidence_gap · blocks readiness — The paired-fault and retirement tests supply outcomes past the production failure boundary
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:1455`; acceptance: R4-F1, R7-F2, R7-F4
- Trigger: The focused test is used to claim end-to-end coverage of the independent release failure and either direct retirement throwing.
- Expected: Controlled failures originate at the release operation and travel through the same publication, admission and final cleanup path used by the effects runner.
- Actual: The paired-fault test uses an empty Inject callback and directly inserts finished failure strings into isolation. The command interruption test runs separately. RetiringCell is a test-owned copy of the retirement sequence, exercises only the other source throwing, and ends before phase and fixture teardown. These tests pass even if production drops an early failure or retries the source at final release.
- Why it blocks / does not: The required acquisition-to-admission evidence is missing at the boundaries where the production defects occur.
- Requested correction: Drive the shared production release and teardown path with controlled native operations. Include the injected condition fault plus an actual throwing command release, and both positions of the direct source failure.
- Requested evidence: Focused gate output on the corrected source with assertions for request counts, independent cleanup, error identities, exact residuals, next-cell refusal and the final verdict.

**R1-F12** · P2 · required_gap · blocks readiness — The C3 qualification selection also collects presentation material
- Location: `scripts/compatibility/loader-compat/sessions/s7-c3-fx.json:15`; acceptance: C3 §6
- Trigger: Running the added s7-c3-fx session reaches the inherited capture and listening-sample steps.
- Expected: C3 collects the mechanical ownership and evaluator observations and reuses the existing visual and private listening material.
- Actual: The new session selects the full presentation-producing effects phase. Its passing receipt records 56, 58 and 58 new Dust cast frames. The phase also runs the native-look comparison and posts five impact and seven Trumpet listening samples. Those comparison steps are absent from the session’s stated qualification scope.
- Why it blocks / does not: The mission directs reuse of existing taste evidence and requires scope deviations to be disclosed.
- Requested correction: Record the capture that occurred and its cost within the existing run. Scope the C3 qualification selection to avoid fresh presentation capture and comparison playback while preserving the enclosing admission check.
- Requested evidence: Identify the C3 selection and its mechanical observations, retain the original session receipts, and identify the existing packet used for owner review. This records correction needs no new taste collection.

**R1-F13** · P2 · required_gap · blocks readiness — The recovery handoff gives the stack order but lacks the requested integration plan
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:241`; acceptance: C3 §8
- Trigger: The owner uses the final C3 handoff to decide what can be integrated with #219.
- Expected: The handoff identifies the frozen inputs, outstanding decisions, evidence to reconcile and the steps the owner could authorize to integrate the reviewed stack.
- Actual: The manifest lists the dependency order and finding map. Its C3 integration entries still say 'Combined candidate'. Neither the manifest nor the checkpoint report gives the owner a concrete procedure for reconciling the reviewed corrections with frozen #219.
- Why it blocks / does not: The conditional integration plan is an explicit final deliverable and can be written before a C3 archive exists.
- Requested correction: Add the conditional integration plan and current C3 stop. Distinguish product source, reviewed heads and controller archives, and reserve the parent resumption or integration decision for the owner.
- Requested evidence: A records-only handoff covering #219, #221, #222 and #223, their dependency checks, the finding reconciliation and the remaining qualification decision.

#### Evidence assessment

- **Inspected:** The fixed request, enrolled mission and matching SHA-256, repository instructions, validation policy, workflow guide, developer report, foreground transcript notice and empty prior-findings ledger.; The 13-file feature diff against C2 archive eab90a5ac82f661ecb903c5456d951a57a085eb9, including the observer changes, registered domain tests, session selection, recovery manifest and both native runs.; The enclosing effects sequence, Dust and callback owners, Harmony rollback and readback, unit destruction helpers, fixture teardown, assertion aggregation and loader session verdict.
- **Executed verification:** Four controller receipts bind passing Focused gates and production compilation to 7fff8b34f6d925d91e80beddb6501cfad249e33d. The selections were sprint7-visual 25, summon-playtest-r6 11, native-profile 9 and loader-compat-probe 5. All 50 selected executions passed. The receipts total 7.2s inside the gates.; Session 20261126T051633Z recorded 37/40 passing checks on the first recovery package. The three failures were Dust condition cleanup, Dust abandonment and phase restoration. The transaction completed and verified the owner installation, saves and registry.; Session 20261126T054228Z recorded 41/41 passing checks and all seven phase cells admitted on product source 6abd692fa7ee84604ef8122569f84a57e3b3c1a2, package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46, DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The transaction and restoration readbacks passed. Changes from that product source to the reviewed commit comprise tests and records.; In that run, each source’s production handler was called once in frame 5416. IDs 544 and 545 were both observed playing, and the live evaluator returned null. I compared the committed callback row with the native receipt field by field. They agree, and the controller receipt confirms the same-run persisted-verdict test passed.; C2’s separate 20261126T033419Z lifecycle receipt records PASS on the original candidate-3 package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413 with harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1.
- **Source inspection only:** The failure mechanisms in R1-F1 through R1-F10 follow from the production callers and their cleanup order. They are distinct from the successful outcomes observed in 20261126T054228Z.; The finding map retains the historical parent failures, the R6-F5 expenditure and harness correction, and C2’s independent harness scope.
- **Required gaps:** Complete release, settlement and exception accounting through the actual effects and fixture teardown paths.; Focused evidence that real release failures and late effects reach both next-cell admission and the final verdict.; A scoped qualification profile and concrete owner integration handoff. Native qualification of changed product bytes is constrained by the exhausted candidate ceiling.
- **Limitations:** The review was read-only. Gate execution is confirmed from controller receipts and native execution from the committed structured run records. The foreground shell log was withheld, so the local build timings, pre-run estimates and 14.3s final gate-batch wall time could not be independently timed here.; The successful native pre-delivery check interrupted one command inside a still-running Dust cell. The command field retained the same process, and the process was still live immediately after interruption. Field clearing and recovery-driver disposal during a pending wait were exercised with controlled domain boundaries.; The existing native receipts apply to their identified product bytes. The two-cue observation used constructed scheduling. Owner visual and listening judgments remain separate.

#### Summary

- **Delivered behavior:** The combined candidate produced the declared same-frame cue overlap, executed C1’s revised evaluator in game and matched its persisted-row verdict. It also demonstrated the injected hook failures with readable counts and subsequent repair. C3’s complete effects-ownership contract remains open.
- **Scope reviewed:** Final review of PR #223 against C2’s archive, covering inherited R4-F1 and R7-F2 through R7-F5, C1’s deferred native check, C2 evidence identity, the shared budget and the integration handoff.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Decide how to handle native qualification of corrected product bytes after both authorized recovery candidates have been used.; Visual acceptance for T2, T3 and T6, and listening choices for T5 and T7 using the existing private packet.; Integration of the dependent corrections with frozen PR #219 and any subsequent decision on parent review, merge or release.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a21669-abf0-7651-a73d-3bb6c3cc56ea`, duration 997.6s, response sha256 `7956d6d2d19891a9a433f66dd23acfa44c36383c2a5cd391897534ad15a6f8df`, request `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r01-0331ea`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "7fff8b34f6d925d91e80beddb6501cfad249e33d",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The combined run establishes C1\u2019s live and persisted verdict agreement. C3 still has failure paths where teardown repeats a retirement, loses a command-release error, or admits later work with owned effects unaccounted for. The 41 passing checks did not exercise those paths.\n\n## Continuation for Claude\n\nKeep one ownership record from acquisition through cell disposal, phase teardown and the final verdict. Carry early command failures forward, reconcile effects from retained Dust executions before reuse, and attempt hook rollback independently of diagnostic reads. Make the abandonment driver consume the complete child result and preserve the outcome of its own repair on disposal. Exercise the production cleanup path with controlled failing operations, including both direct retirements and the paired Dust fault. Record the qualification scope and the conditional integration plan for the owner.\n\nThe shared ledger reports 42m28.9s spent, 2m31.1s remaining and both authorized product candidates used. The required RuntimeTesting corrections change the DLL. Preserve the existing receipts and report the native qualification stop for any corrected bytes. A further package requires new owner authority. Keep the current enrollment, frozen prerequisites, accepted C1/C2 scopes and historical failures intact.",
  "evidence_assessment": {
    "executed_verification": [
      "Four controller receipts bind passing Focused gates and production compilation to 7fff8b34f6d925d91e80beddb6501cfad249e33d. The selections were sprint7-visual 25, summon-playtest-r6 11, native-profile 9 and loader-compat-probe 5. All 50 selected executions passed. The receipts total 7.2s inside the gates.",
      "Session 20261126T051633Z recorded 37/40 passing checks on the first recovery package. The three failures were Dust condition cleanup, Dust abandonment and phase restoration. The transaction completed and verified the owner installation, saves and registry.",
      "Session 20261126T054228Z recorded 41/41 passing checks and all seven phase cells admitted on product source 6abd692fa7ee84604ef8122569f84a57e3b3c1a2, package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46, DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The transaction and restoration readbacks passed. Changes from that product source to the reviewed commit comprise tests and records.",
      "In that run, each source\u2019s production handler was called once in frame 5416. IDs 544 and 545 were both observed playing, and the live evaluator returned null. I compared the committed callback row with the native receipt field by field. They agree, and the controller receipt confirms the same-run persisted-verdict test passed.",
      "C2\u2019s separate 20261126T033419Z lifecycle receipt records PASS on the original candidate-3 package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413 with harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1."
    ],
    "inspected": [
      "The fixed request, enrolled mission and matching SHA-256, repository instructions, validation policy, workflow guide, developer report, foreground transcript notice and empty prior-findings ledger.",
      "The 13-file feature diff against C2 archive eab90a5ac82f661ecb903c5456d951a57a085eb9, including the observer changes, registered domain tests, session selection, recovery manifest and both native runs.",
      "The enclosing effects sequence, Dust and callback owners, Harmony rollback and readback, unit destruction helpers, fixture teardown, assertion aggregation and loader session verdict."
    ],
    "limitations": [
      "The review was read-only. Gate execution is confirmed from controller receipts and native execution from the committed structured run records. The foreground shell log was withheld, so the local build timings, pre-run estimates and 14.3s final gate-batch wall time could not be independently timed here.",
      "The successful native pre-delivery check interrupted one command inside a still-running Dust cell. The command field retained the same process, and the process was still live immediately after interruption. Field clearing and recovery-driver disposal during a pending wait were exercised with controlled domain boundaries.",
      "The existing native receipts apply to their identified product bytes. The two-cue observation used constructed scheduling. Owner visual and listening judgments remain separate."
    ],
    "required_gaps": [
      "Complete release, settlement and exception accounting through the actual effects and fixture teardown paths.",
      "Focused evidence that real release failures and late effects reach both next-cell admission and the final verdict.",
      "A scoped qualification profile and concrete owner integration handoff. Native qualification of changed product bytes is constrained by the exhausted candidate ceiling."
    ],
    "source_inspection_only": [
      "The failure mechanisms in R1-F1 through R1-F10 follow from the production callers and their cleanup order. They are distinct from the successful outcomes observed in 20261126T054228Z.",
      "The finding map retains the historical parent failures, the R6-F5 expenditure and harness correction, and C2\u2019s independent harness scope."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R7-F4",
        "C3 \u00a75"
      ],
      "actual": "Sprint7OwnedRequests is local to S7CallbackLoss. Phase teardown passes every still-live entry in owned to DisposeExpandedSummoningUnits again. If the first cleanup threw before fixture.Created.Remove, fixture teardown can also revisit the source. The bulk cleanup stops at the first exception, before later units and the phase result are recorded. RequestRecorded also increments sourcesRetiredHere when the request throws.",
      "blocking_rationale": "The one-request guarantee ends at the observer boundary. A failing retirement can be repeated and can prevent independent cleanup.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each cue source has one retirement attempt across all teardown layers, a named result and an explicit settlement observation. Every other owned release is attempted.",
      "id": "R1-F1",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3334
      },
      "priority": "P1",
      "requested_correction": "Carry per-source attempt and settlement state into phase and fixture teardown. Release outstanding units independently, distinguish attempts from observed retirements, and publish the final result even when a release throws.",
      "requested_evidence": "Exercise each direct source retirement throwing while that source remains alive, through final teardown. Show one attempt for the failed source, the original error, independent releases, settlement by identity and failed isolation.",
      "title": "Phase teardown can repeat a failed cue-source retirement",
      "trigger": "Either direct callback-source retirement throws or returns with the unit still alive."
    },
    {
      "acceptance_ids": [
        "R7-F2",
        "C3 \u00a75"
      ],
      "actual": "S7Dust releases currently visible conditions and unsubscribes the ledger before waiting for retained processes. Its residual subsequently reads only IsEnded. A Sickened application delivered after unsubscribe is neither acquired nor released. The native pre-delivery check keeps the ledger subscribed during its wait and explicitly clears conditions afterward.",
      "blocking_rationale": "The next cell can start with a condition left by the previous cell even though its retained execution has ended.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An ordinary cell disposal accounts for effects delivered by its exact retained executions through settlement, including effects delivered after command interruption.",
      "id": "R1-F2",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3101
      },
      "priority": "P1",
      "requested_correction": "Reconcile conditions attributable to the retained process at settlement before permitting reuse. Preserve foreign conditions and keep isolation failed when the owned effects cannot be accounted for.",
      "requested_evidence": "Dispose the enclosing ordinary Dust cell before delivery, deliver an owned condition while its process is pending, and verify removal of that instance, survival of a foreign control and the admission decision.",
      "title": "Dust stops observing conditions while an owned execution can still deliver them",
      "trigger": "The ordinary Dust iterator is disposed after execution starts and before its first effect is delivered."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2"
      ],
      "actual": "The pre-delivery call to EndUnfinished stores its failures only in preFailures and a diagnostic check. The finalizer starts a fresh commandFailures list. If the command finishes before final cleanup, that pass returns no error. Sprint25Check records the earlier failed check but does not fail the isolation used by Sequence.",
      "blocking_rationale": "A named release exception can disappear from admission and phase restoration after the visible command settles.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every failed release attempt immediately enters the cell\u2019s cumulative isolation result and survives a later successful cleanup.",
      "id": "R1-F3",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3005
      },
      "priority": "P1",
      "requested_correction": "Publish early and final command-release failures through the same cumulative owner before yielding again.",
      "requested_evidence": "Make the early interruption or queue update throw, then let the command finish. Verify that the original failure persists, the next phase cell is refused and final isolation fails.",
      "title": "An early command-release failure reaches a check but misses phase isolation",
      "trigger": "The pre-delivery release throws and the command later finishes or is successfully released."
    },
    {
      "acceptance_ids": [
        "R7-F2"
      ],
      "actual": "Capture catches a processOf exception and substitutes null. ReleaseEach can then interrupt the command and lose its only process reference without recording the unreadable acquisition. The ordinary Dust cancellation at line 2814 also calls InterruptAll directly after yielding a frame, bypassing capture if execution has already begun.",
      "blocking_rationale": "An unobserved live process can be omitted from the residuals that govern admission.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every owned interruption preserves the current process or records an unreadable process by command identity. A genuinely unstarted command can still have a known null process.",
      "id": "R1-F4",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 842
      },
      "priority": "P1",
      "requested_correction": "Route the ordinary cancellation through the command owner and carry capture errors into its cumulative result while continuing independent releases.",
      "requested_evidence": "Exercise an unreadable process accessor followed by field clearing, and a cancellation where execution starts during the yielded frame. Verify the exact process or named unknown, independent command release and the resulting guard decision.",
      "title": "Some Dust interruptions can clear a process before ownership is established",
      "trigger": "Process lookup throws, or a nominally pre-execution cancellation runs after the engine has assigned ExecutionProcess."
    },
    {
      "acceptance_ids": [
        "R7-F3"
      ],
      "actual": "The projectile constructor reads OwnedHooks before Unhook and again before preserving the setup exception. A read failure can skip rollback or replace the original error. Callback Unhook also gates the release on Hooks(), and projectile Repair reads all counts while assembling its release list. One unreadable target can therefore skip even an already identified release. HookCount additionally returns zero for a listed method whose patch information is null.",
      "blocking_rationale": "The requested unreadable-rollback path can leave installed hooks, lose the setup cause, or report an ambiguous read as zero.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Attempt rollback for each known attempted hook by owner identity. Preserve the setup error, each rollback error and explicit known or unreadable readbacks independently.",
      "id": "R1-F5",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 2059
      },
      "priority": "P1",
      "requested_correction": "Decouple release attempts from diagnostic hook counts in construction, normal disposal and outer repair. Treat unavailable patch information as unknown and retain both the original and secondary errors.",
      "requested_evidence": "At the shared rollback boundary, make a hook count fail before and after an unpatch. Show every applicable target was attempted, the original setup error survives, and an unreadable result prevents clean isolation.",
      "title": "An unreadable Harmony count can prevent rollback and replace the setup error",
      "trigger": "Harmony inspection fails after a callback or projectile hook has partially installed, or while a repair is being assembled."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "C3 \u00a75"
      ],
      "actual": "Proven checks pending residuals but omits Child.Unreported. Unexpected and ExpectedMissing use substring matching on whole failure messages. For example, the expected count text ending in 'remaining: 1' also accepts 'remaining: 10'. An independent error containing an expected marker can likewise be exempted. The Dust safety readback does not check that exact remaining count.",
      "blocking_rationale": "Repairing the intended injected condition can still produce a successful parent verdict with an incomplete or independently failed child.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery exempts only the injected operation and its exact known residual. Every owner must report, and every independent error must reach the phase.",
      "id": "R1-F6",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1014
      },
      "priority": "P1",
      "requested_correction": "Use the complete child outcome. Bind allowed failures to the operation, identity and expected value, preserve independent components of aggregated errors, and require the unreported-owner list to be empty.",
      "requested_evidence": "Run the driver with an unreported owner alongside the intended fault, with a different residual count, and with an independent command error containing an expected phrase. Each must refuse the later phase cell.",
      "title": "The abandonment driver can accept an incomplete child outcome",
      "trigger": "The injected fault is present alongside an unreported owner or another failure sharing part of its diagnostic text."
    },
    {
      "acceptance_ids": [
        "R7-F5"
      ],
      "actual": "On unwind, RepairOnce calls the removal and records thrown errors. The exact leftover count and disappearance check live only in Unsafe on the ordinary path. If Remove returns with the buff still present, the static list is cleared, repairFailures can be empty and the phase records only a generic incomplete proof.",
      "blocking_rationale": "The driver does attempt repair and fails the phase, but its disposal record loses a failed or unverified repair of the exact injected condition.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every exit records the attempt and available result for the exact injected condition, including a remaining or unreadable instance.",
      "id": "R1-F7",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1021
      },
      "priority": "P2",
      "requested_correction": "Publish the exact repair result from the unwind path as well. A stopped scenario can retain a named unresolved result without retrying the removal or advancing the fixture.",
      "requested_evidence": "Dispose during the recovery wait with a removal that returns while the exact buff remains. Show one attempt, the remaining or unknown result, the original child fault and the untouched foreign effect.",
      "title": "Disposal during recovery omits the result of a nonthrowing failed repair",
      "trigger": "The Dust driver is disposed during its wait and removing the injected condition returns without establishing disappearance."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75",
        "R4-F1"
      ],
      "actual": "Sequence records Refused and exits on an unsettled residual. ReviewSprint7Effects writes a stopped string, settles again and bases the final assertion on the current Restored value. Restored omits Refused. If the residual clears in the final wait or teardown, the phase can pass with required later cells skipped. The enclosing product verdict counts only assertions that were emitted.",
      "blocking_rationale": "A partial run can be presented as successful qualification even when the real guard refused later work.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Record eventual physical restoration and whether the required phase sequence completed. A refused required cell must keep the qualification incomplete.",
      "id": "R1-F8",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 3337
      },
      "priority": "P1",
      "requested_correction": "Make the final phase verdict consume the admission outcome, including a required cell that was refused or skipped. Preserve the separate later recovery observation.",
      "requested_evidence": "Keep a residual live through the admission deadline, settle it during final restoration, and show that the unit or process is restored while the incomplete phase still fails.",
      "title": "Final settlement can turn a refused phase into a passing result",
      "trigger": "A residual outlives the next-cell admission window and settles after the sequence has stopped."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75"
      ],
      "actual": "S7Dust subscribes saveLog and constructs the self-subscribing condition ledger before entering its cleanup try. A failure reading Present() or subscribing the ledger strands the already-acquired save log. ReviewSprint7Effects similarly mutates hostile hit points and subscribes the recorder before constructing the save handler and entering its phase try.",
      "blocking_rationale": "Recording a setup exception in the cell runner cannot release a subscription for which the cleanup scope never started.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The cleanup scope owns each acquisition attempt before a later setup operation can throw, including partially successful subscriptions.",
      "id": "R1-F9",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs",
        "line": 2583
      },
      "priority": "P1",
      "requested_correction": "Establish cleanup ownership before the first mutation or subscription in the affected cell and enclosing phase. Retain a releasable identity across partial subscription failure and publish the setup and cleanup outcomes.",
      "requested_evidence": "Inject failure after the first subscription and during the next acquisition. Verify independent unsubscription and restoration, preservation of the original error and failed admission.",
      "title": "Effects setup can throw before its acquired subscriptions are protected",
      "trigger": "A later setup operation fails after the save log or phase recorder has subscribed."
    },
    {
      "acceptance_ids": [
        "C3 \u00a75"
      ],
      "actual": "IsolatedCell records a Dispose exception in isolation but still passes a null failure to completed, which can emit a successful cell check. Reading steps.Current and invoking completed are outside the exception capture. The production completion callback writes a progress receipt, so a reporting exception can escape without reaching the sticky isolation result.",
      "blocking_rationale": "The runner can publish conflicting completion and isolation results or lose an unexpected runner failure from the final phase record.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each observable cell or runner exception reaches isolation, and the completion record reflects any disposal failure.",
      "id": "R1-F10",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1079
      },
      "priority": "P2",
      "requested_correction": "Carry disposal failures into the completion result and record failures from reading the current step or publishing completion before they leave the runner.",
      "requested_evidence": "Use a throwing disposer and a throwing completion callback. Confirm the recorded cell result, named isolation failure and final phase decision agree.",
      "title": "Cell completion can report success after disposal failed",
      "trigger": "Disposal is the cell\u2019s only exception, or producing its completion record throws."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "R7-F4"
      ],
      "actual": "The paired-fault test uses an empty Inject callback and directly inserts finished failure strings into isolation. The command interruption test runs separately. RetiringCell is a test-owned copy of the retirement sequence, exercises only the other source throwing, and ends before phase and fixture teardown. These tests pass even if production drops an early failure or retries the source at final release.",
      "blocking_rationale": "The required acquisition-to-admission evidence is missing at the boundaries where the production defects occur.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Controlled failures originate at the release operation and travel through the same publication, admission and final cleanup path used by the effects runner.",
      "id": "R1-F11",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 1455
      },
      "priority": "P2",
      "requested_correction": "Drive the shared production release and teardown path with controlled native operations. Include the injected condition fault plus an actual throwing command release, and both positions of the direct source failure.",
      "requested_evidence": "Focused gate output on the corrected source with assertions for request counts, independent cleanup, error identities, exact residuals, next-cell refusal and the final verdict.",
      "title": "The paired-fault and retirement tests supply outcomes past the production failure boundary",
      "trigger": "The focused test is used to claim end-to-end coverage of the independent release failure and either direct retirement throwing."
    },
    {
      "acceptance_ids": [
        "C3 \u00a76"
      ],
      "actual": "The new session selects the full presentation-producing effects phase. Its passing receipt records 56, 58 and 58 new Dust cast frames. The phase also runs the native-look comparison and posts five impact and seven Trumpet listening samples. Those comparison steps are absent from the session\u2019s stated qualification scope.",
      "blocking_rationale": "The mission directs reuse of existing taste evidence and requires scope deviations to be disclosed.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "C3 collects the mechanical ownership and evaluator observations and reuses the existing visual and private listening material.",
      "id": "R1-F12",
      "location": {
        "file": "scripts/compatibility/loader-compat/sessions/s7-c3-fx.json",
        "line": 15
      },
      "priority": "P2",
      "requested_correction": "Record the capture that occurred and its cost within the existing run. Scope the C3 qualification selection to avoid fresh presentation capture and comparison playback while preserving the enclosing admission check.",
      "requested_evidence": "Identify the C3 selection and its mechanical observations, retain the original session receipts, and identify the existing packet used for owner review. This records correction needs no new taste collection.",
      "title": "The C3 qualification selection also collects presentation material",
      "trigger": "Running the added s7-c3-fx session reaches the inherited capture and listening-sample steps."
    },
    {
      "acceptance_ids": [
        "C3 \u00a78"
      ],
      "actual": "The manifest lists the dependency order and finding map. Its C3 integration entries still say 'Combined candidate'. Neither the manifest nor the checkpoint report gives the owner a concrete procedure for reconciling the reviewed corrections with frozen #219.",
      "blocking_rationale": "The conditional integration plan is an explicit final deliverable and can be written before a C3 archive exists.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The handoff identifies the frozen inputs, outstanding decisions, evidence to reconcile and the steps the owner could authorize to integrate the reviewed stack.",
      "id": "R1-F13",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 241
      },
      "priority": "P2",
      "requested_correction": "Add the conditional integration plan and current C3 stop. Distinguish product source, reviewed heads and controller archives, and reserve the parent resumption or integration decision for the owner.",
      "requested_evidence": "A records-only handoff covering #219, #221, #222 and #223, their dependency checks, the finding reconciliation and the remaining qualification decision.",
      "title": "The recovery handoff gives the stack order but lacks the requested integration plan",
      "trigger": "The owner uses the final C3 handoff to decide what can be integrated with #219."
    }
  ],
  "prior_finding_assessments": [],
  "request_id": "kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r01-0331ea",
  "round": 1,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The combined candidate produced the declared same-frame cue overlap, executed C1\u2019s revised evaluator in game and matched its persisted-row verdict. It also demonstrated the injected hook failures with readable counts and subsequent repair. C3\u2019s complete effects-ownership contract remains open.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Decide how to handle native qualification of corrected product bytes after both authorized recovery candidates have been used.",
      "Visual acceptance for T2, T3 and T6, and listening choices for T5 and T7 using the existing private packet.",
      "Integration of the dependent corrections with frozen PR #219 and any subsequent decision on parent review, merge or release."
    ],
    "scope_reviewed": "Final review of PR #223 against C2\u2019s archive, covering inherited R4-F1 and R7-F2 through R7-F5, C1\u2019s deferred native check, C2 evidence identity, the shared budget and the integration handoff.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
