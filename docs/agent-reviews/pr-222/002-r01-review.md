<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r01-review part=1/1 sha256=b5b4d14598e6d8fa60156bee85da398e39ff9d2294cf502beb7565dd706f7dac -->
### Codex GPT-6.2 Astra — review — round 1 — candidate `192166b9ba`

**Decision:** `changes_required` — Both focused gates and the short native lifecycle session passed. R5-F1 remains open because the slot can admit another probe while a newly spawned unit is unaccounted for or the witness still reports unknown ownership. Cleanup exceptions can also prevent independent units from being reconciled or reported.

## Continuation for Claude

Keep the correction in the C2 external harness. Preserve the read-only witness, the verified post-disposal delivery order, and one destruction request per owned unit. Carry witness identities through release even when unsubscription fails. Use a conclusive native settlement read and publish the same observation to the evidence row and the cumulative cleanup result. Exercise the production caller through ProbeSlot with controlled engine boundaries, including a foreign control that participates in the observation. Update the shared allowance before validation. Run the named focused gates on the corrected commit and requalify only the short lifecycle session on candidate 3 with the rebuilt harness hash and restoration receipt. C3 retains the combined product qualification.

#### Findings

**R1-F1** · P1 · defect · blocks readiness — A newly spawned unit can pass settlement before it enters State.Units
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:84`; acceptance: R5-F1
- Trigger: The slot is abandoned after the witness receives an additional spawn and before the entity creator registers it.
- Expected: The caller establishes terminal destruction for the exact unit or records its identity as unresolved.
- Actual: The reader returns true whenever State.Units omits the unit, even with Destroyed false. The repository records live spawned units missing from that collection before EntityCreator.Tick in docs/GUNSLINGER-SPRINT6-C-GRAPPLE-TERMINAL-CLEANUP.md at line 193. SpawnDisposable also returns before verifying native registration. A late result in that interval can therefore produce an empty unsettled list.
- Why it blocks / does not: A unit still pending creation can be counted as gone, allowing ProbeSlot to report restored isolation.
- Requested correction: Use a native terminal-state check that distinguishes a pending or detached live unit from a destroyed one. Treat an inconclusive read as unresolved on ordinary release and abandonment.
- Requested evidence: Drive a correlated unit with Destroyed false and native registration pending through slot release. Show one request, a named unresolved result and refused admission, alongside a genuinely destroyed control that restores isolation.

**R1-F2** · P1 · defect · blocks readiness — A disposable registration can hide an unknown witness identity
- Location: `tools/LoaderCompatHarness/TrumpetCueVerdict.cs:309`; acceptance: R5-F1
- Trigger: A same-caster result is neither positively correlated nor identified as a known foreign control, but its unit is already registered for fixture disposal.
- Expected: An identity still classified as unknown at release produces a named cleanup failure. Positive correlation or explicit knowledge of a foreign control can resolve that classification.
- Actual: Watch passes witness.Unknown after excluding explicit known controls. RecordUncertain then silently drops every entry already in the general disposable inventory. The injected late unit is deliberately registered there before delivery. A failed attribution can therefore leave an ID in the witness's unknown array while cleanup reports restored.
- Why it blocks / does not: Registration for eventual fixture teardown does not establish which cast produced the observed result. The resulting uncertainty can disappear from the outcome used for admission.
- Requested correction: Resolve witness provenance explicitly before recording residual unknowns. Preserve independently authorized fixture teardown without using membership in its disposal list to clear an unknown observation.
- Requested evidence: Pass an already-registered disposable through an unattributed same-caster observation. Verify that the witness release leaves it alone, the unknown ID survives independent teardown in the cumulative failure, and the next probe is refused.

**R1-F3** · P1 · defect · blocks readiness — A failed unsubscribe skips reconciliation of observed units
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:1005`; acceptance: R5-F1
- Trigger: Early release or abandonment removes a witness whose native unsubscribe throws.
- Expected: Reconciliation of results already observed runs even when releasing the subscription fails. Both failures remain available to the slot.
- Actual: EventBus.Unsubscribe runs before LateResultReconciliation.Release in one callback. If it throws, the resource owner records the witness error after removing the callback from its held list. An additional unit present only in witness.Owned is never adopted and receives no request from ReleaseRemaining.
- Why it blocks / does not: The slot stops on the unsubscribe error, but independent owned units can remain alive without a destruction attempt or an identified cleanup result.
- Requested correction: Make unsubscription and reconciliation independently attempted operations. Preserve the unsubscribe exception and reconcile all observed correlated and unknown identities during early release and unwind.
- Requested evidence: Inject an unsubscribe failure after an extra correlated result arrives. Show its single request, its settlement or named residual, the original unsubscribe error, continued independent cleanup and refused admission.

**R1-F4** · P1 · defect · blocks readiness — Final settlement depends on an unconditional global destroyer tick
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:102`; acceptance: R5-F1
- Trigger: Final release encounters foreign pending destruction or the destroyer throws before per-unit readback.
- Expected: Final release confines any forced engine work to owned resources and attempts each requested unit's settlement read even if the engine drain fails.
- Actual: Final release unconditionally calls the global EntityDestroyer.Tick and then calls RecordUnsettled in the same action. The tick can advance unrelated pending destruction, even with an empty owned set. If it throws, the report contains the generic action error and skips the per-unit reads. The repository's ElementalCompletionSceneScenario checks pending scene work before advancing this controller.
- Why it blocks / does not: Final cleanup can act on foreign work or lose the identities of the units whose settlement the mission requires it to report.
- Requested correction: Use normal engine progression or establish that a forced drain is confined to owned work. Record any drain failure independently and still publish per-unit unresolved results.
- Requested evidence: Cover abandonment with unrelated pending destruction and with a failing drain. Verify foreign work is preserved and that the slot receives both the drain error and the affected owned identities.

**R1-F5** · P2 · defect · blocks readiness — The evidence row and slot use separate terminal reads
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:1016`; acceptance: R5-F1
- Trigger: A settlement read is unreadable at the deadline and succeeds when immediately retried.
- Expected: The evidence row and cleanup outcome consume the same terminal observation for each requested identity.
- Actual: SettleOwned writes the evidence row from Unsettled(), then RecordUnsettled() queries every unit again. A terminal row can name a present or unreadable unit while the second read succeeds and contributes no cumulative failure.
- Why it blocks / does not: A unit observed unresolved at the end of the allowed wait can be lost from the decision that admits the next probe.
- Requested correction: Capture the terminal settlement results once and use that snapshot to populate both the row and sticky cleanup failures.
- Requested evidence: Use a controlled reader whose terminal read is unreadable and whose following read succeeds. Show that the emitted row and slot outcome retain the same unresolved identity.

**R1-F6** · P2 · defect · blocks readiness — Fallback cleanup and an early foreign read can satisfy the native verdict
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:805`; acceptance: R5-F1
- Trigger: Reconciliation skips the seeded late unit, or affects the foreign control after its survival was sampled.
- Expected: The lifecycle verdict requires the witness caller's request and checks foreign survival after reconciliation, while the foreign fixture is still expected to be alive.
- Actual: The late case records lateUnitDestroyRequestedByReconciliation, calls the fallback Destroy, and gates its verdict only on the post-fallback request flag. It also samples foreignPreserved before the witness release that can now destroy units. The supplied run recorded the intended request, but the verdict would also accept fallback-only cleanup or foreign loss during that release.
- Why it blocks / does not: The short native qualification can pass when the specific caller behavior it is meant to qualify regresses.
- Requested correction: Include the pre-fallback request observation in the verdict. Check the identified foreign control after witness release and settlement, before its separately owned teardown.
- Requested evidence: Show that a skipped reconciliation request fails the lifecycle check even when fallback cleanup succeeds, and that the corrected native row records the foreign control alive and unrequested at the post-release boundary.

**R1-F7** · P2 · evidence_gap · blocks readiness — The failure tests substitute the caller that coordinates native cleanup
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:960`; acceptance: R5-F1
- Trigger: The new regression is used to establish foreign survival or failure handling in the production Ghaele caller.
- Expected: Controlled tests exercise the release orchestration used by GhaeleLifetimeProbe, with native operations replaced at their boundaries and an actual known foreign observation.
- Actual: LateUnitProbe uses the real slot and destruction helper, but manually fills the witness lists and substitutes its own release and settlement sequence. Its Foreign object never enters the observed controls. The link back to GhaeleLifetimeProbe is a two-substring source assertion. Those tests can stay green when the production unsubscribe or final-drain ordering drops work.
- Why it blocks / does not: The mission requires behavioral evidence through the actual caller and admission path for material failure cases.
- Requested correction: Drive the production caller's release and settlement sequence from the domain tests. Supply the observed owned, known-foreign and unknown identities, and assert the resulting slot outcome on completion and abandonment.
- Requested evidence: Provide focused receipts for the corrected caller showing one request per owned identity, survival of the participating foreign control, continued independent cleanup, named residuals and refused next admission.

**R1-F8** · P3 · optional_polish · non-blocking — The shared ledger retains a stale pre-C2 status
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:157`; acceptance: —
- Trigger: A reader consults the recovery ledger for the current harness and native validation spend.
- Expected: The current ledger distinguishes the historical C1 state from the C2 harness build and native session.
- Actual: The ledger still says that no harness build or game session has run. Its following rows record both C2 operations and sit below the prose outside the table.
- Why it blocks / does not: The session receipt and recorded costs identify the actual run.
- Requested correction: Date the historical statement or update it, and put the appended rows in a rendered ledger table.
- Requested evidence: A text and rendering check against the existing receipts is sufficient.

#### Evidence assessment

- **Inspected:** Verified HEAD, merge base, the enrolled mission SHA-256, the complete feature diff, the prior-findings ledger, and the applicable repository instructions.; Traced GhaeleLifetimeProbe, SummonWitness, LateResultReconciliation, OwnedDisposables, YieldedProbeLifecycle, ProbeSlot and HarnessRunner through ordinary release, iterator disposal, final cleanup and next-step admission.; Compared the domain controls, session definition, recovery manifest, controller gate receipts and structured native records. The launch and session identities, product hash and recorded raw-result hash agree across the supplied extracts.
- **Executed verification:** Controller receipts bind both focused gates to H with an unchanged worktree. Production compilation passed. The sprint7-visual filter passed 23 tests in a 2.1-second receipt, including ghaele-late-units-reach-slot. The loader-compat-probe filter passed 5 tests in a 1.7-second receipt.; Session 20261126T002852Z-owner-0.33.0-s7-ghaele-lifecycle records a PASS on candidate-3 package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413, product DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3, and harness 0a038e40c6c1da95e59bd943e5972df8940e3b74158eacebe88dcab7ed571ca9. The harness source tree at the session commit c5fe26d1465246b800678a09e6e75c25d06f490c is identical to the tree at H.; The native row records a null positive-pending verdict and the controlled late delivery after cast-owner disposal. Its owned ID 77eaaa28-5a51-4564-b549-f57c09253587 has one destruction attempt, a true reconciliation-request field, and a positive destruction readback. The real-spawn witness matched by execution. The session records successful transaction completion, verified owner files and saves, and matching registry backup and readback hashes.
- **Source inspection only:** The pending-registration ambiguity is supported by the settlement predicate and the repository's prior native observation of live spawned units absent from State.Units before the creator tick.; The unsubscribe and destroyer exception paths, suppression of registered unknowns, and separate terminal reads follow from the candidate's control flow. The new domain probe substitutes its own caller at these boundaries.
- **Required gaps:** Establish terminal settlement for a correlated unit whose native registration is still pending, and carry every unresolved witness identity into slot admission.; Demonstrate independent reconciliation and per-unit reporting across native release errors using the production caller.; Make the lifecycle verdict enforce the reconciliation request and foreign survival after release, then qualify the corrected harness on the existing short session.
- **Limitations:** The foreground shell transcript was withheld. The two controller receipts and committed native extracts establish the runs described above. The developer reports 7m13.0s of aggregate recovery validation and zero new product candidates.; The review was read-only. The C2 native session exercised successful settlement. The release faults below are source findings, not failures observed in that session.

#### Summary

- **Delivered behavior:** On successful witness release, the caller adopts correlated units, deduplicates destruction requests by reference, and records persistent settlement failures in the cleanup report consumed by ProbeSlot. The identified native session demonstrated the ordinary post-disposal control and completed transactional restoration.
- **Scope reviewed:** Final C2 external-harness correction for inherited R5-F1 in PR #222, based on C1 archive 2c67a049ee33e0b29621fd29a8b25608ed31e956. Combined effects and cue qualification belongs to C3.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Visual and listening acceptance of the existing Sprint 7 material.; Integration of the dependent repair stack with frozen PR #219 and any subsequent merge or release decision.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a21545-a429-7e53-83f4-d5662d6542c0`, duration 686.0s, response sha256 `31055d20da8cd8b2aec08b40cd9ebca769baecaf9beb50955dbdc61c5b5d35e7`, request `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r01-e7efe0`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "192166b9ba79161aaa2b05dc29012650ba68ee1e",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "Both focused gates and the short native lifecycle session passed. R5-F1 remains open because the slot can admit another probe while a newly spawned unit is unaccounted for or the witness still reports unknown ownership. Cleanup exceptions can also prevent independent units from being reconciled or reported.\n\n## Continuation for Claude\n\nKeep the correction in the C2 external harness. Preserve the read-only witness, the verified post-disposal delivery order, and one destruction request per owned unit. Carry witness identities through release even when unsubscription fails. Use a conclusive native settlement read and publish the same observation to the evidence row and the cumulative cleanup result. Exercise the production caller through ProbeSlot with controlled engine boundaries, including a foreign control that participates in the observation. Update the shared allowance before validation. Run the named focused gates on the corrected commit and requalify only the short lifecycle session on candidate 3 with the rebuilt harness hash and restoration receipt. C3 retains the combined product qualification.",
  "evidence_assessment": {
    "executed_verification": [
      "Controller receipts bind both focused gates to H with an unchanged worktree. Production compilation passed. The sprint7-visual filter passed 23 tests in a 2.1-second receipt, including ghaele-late-units-reach-slot. The loader-compat-probe filter passed 5 tests in a 1.7-second receipt.",
      "Session 20261126T002852Z-owner-0.33.0-s7-ghaele-lifecycle records a PASS on candidate-3 package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413, product DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3, and harness 0a038e40c6c1da95e59bd943e5972df8940e3b74158eacebe88dcab7ed571ca9. The harness source tree at the session commit c5fe26d1465246b800678a09e6e75c25d06f490c is identical to the tree at H.",
      "The native row records a null positive-pending verdict and the controlled late delivery after cast-owner disposal. Its owned ID 77eaaa28-5a51-4564-b549-f57c09253587 has one destruction attempt, a true reconciliation-request field, and a positive destruction readback. The real-spawn witness matched by execution. The session records successful transaction completion, verified owner files and saves, and matching registry backup and readback hashes."
    ],
    "inspected": [
      "Verified HEAD, merge base, the enrolled mission SHA-256, the complete feature diff, the prior-findings ledger, and the applicable repository instructions.",
      "Traced GhaeleLifetimeProbe, SummonWitness, LateResultReconciliation, OwnedDisposables, YieldedProbeLifecycle, ProbeSlot and HarnessRunner through ordinary release, iterator disposal, final cleanup and next-step admission.",
      "Compared the domain controls, session definition, recovery manifest, controller gate receipts and structured native records. The launch and session identities, product hash and recorded raw-result hash agree across the supplied extracts."
    ],
    "limitations": [
      "The foreground shell transcript was withheld. The two controller receipts and committed native extracts establish the runs described above. The developer reports 7m13.0s of aggregate recovery validation and zero new product candidates.",
      "The review was read-only. The C2 native session exercised successful settlement. The release faults below are source findings, not failures observed in that session."
    ],
    "required_gaps": [
      "Establish terminal settlement for a correlated unit whose native registration is still pending, and carry every unresolved witness identity into slot admission.",
      "Demonstrate independent reconciliation and per-unit reporting across native release errors using the production caller.",
      "Make the lifecycle verdict enforce the reconciliation request and foreign survival after release, then qualify the corrected harness on the existing short session."
    ],
    "source_inspection_only": [
      "The pending-registration ambiguity is supported by the settlement predicate and the repository's prior native observation of live spawned units absent from State.Units before the creator tick.",
      "The unsubscribe and destroyer exception paths, suppression of registered unknowns, and separate terminal reads follow from the candidate's control flow. The new domain probe substitutes its own caller at these boundaries."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "The reader returns true whenever State.Units omits the unit, even with Destroyed false. The repository records live spawned units missing from that collection before EntityCreator.Tick in docs/GUNSLINGER-SPRINT6-C-GRAPPLE-TERMINAL-CLEANUP.md at line 193. SpawnDisposable also returns before verifying native registration. A late result in that interval can therefore produce an empty unsettled list.",
      "blocking_rationale": "A unit still pending creation can be counted as gone, allowing ProbeSlot to report restored isolation.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The caller establishes terminal destruction for the exact unit or records its identity as unresolved.",
      "id": "R1-F1",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 84
      },
      "priority": "P1",
      "requested_correction": "Use a native terminal-state check that distinguishes a pending or detached live unit from a destroyed one. Treat an inconclusive read as unresolved on ordinary release and abandonment.",
      "requested_evidence": "Drive a correlated unit with Destroyed false and native registration pending through slot release. Show one request, a named unresolved result and refused admission, alongside a genuinely destroyed control that restores isolation.",
      "title": "A newly spawned unit can pass settlement before it enters State.Units",
      "trigger": "The slot is abandoned after the witness receives an additional spawn and before the entity creator registers it."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "Watch passes witness.Unknown after excluding explicit known controls. RecordUncertain then silently drops every entry already in the general disposable inventory. The injected late unit is deliberately registered there before delivery. A failed attribution can therefore leave an ID in the witness's unknown array while cleanup reports restored.",
      "blocking_rationale": "Registration for eventual fixture teardown does not establish which cast produced the observed result. The resulting uncertainty can disappear from the outcome used for admission.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An identity still classified as unknown at release produces a named cleanup failure. Positive correlation or explicit knowledge of a foreign control can resolve that classification.",
      "id": "R1-F2",
      "location": {
        "file": "tools/LoaderCompatHarness/TrumpetCueVerdict.cs",
        "line": 309
      },
      "priority": "P1",
      "requested_correction": "Resolve witness provenance explicitly before recording residual unknowns. Preserve independently authorized fixture teardown without using membership in its disposal list to clear an unknown observation.",
      "requested_evidence": "Pass an already-registered disposable through an unattributed same-caster observation. Verify that the witness release leaves it alone, the unknown ID survives independent teardown in the cumulative failure, and the next probe is refused.",
      "title": "A disposable registration can hide an unknown witness identity",
      "trigger": "A same-caster result is neither positively correlated nor identified as a known foreign control, but its unit is already registered for fixture disposal."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "EventBus.Unsubscribe runs before LateResultReconciliation.Release in one callback. If it throws, the resource owner records the witness error after removing the callback from its held list. An additional unit present only in witness.Owned is never adopted and receives no request from ReleaseRemaining.",
      "blocking_rationale": "The slot stops on the unsubscribe error, but independent owned units can remain alive without a destruction attempt or an identified cleanup result.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Reconciliation of results already observed runs even when releasing the subscription fails. Both failures remain available to the slot.",
      "id": "R1-F3",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 1005
      },
      "priority": "P1",
      "requested_correction": "Make unsubscription and reconciliation independently attempted operations. Preserve the unsubscribe exception and reconcile all observed correlated and unknown identities during early release and unwind.",
      "requested_evidence": "Inject an unsubscribe failure after an extra correlated result arrives. Show its single request, its settlement or named residual, the original unsubscribe error, continued independent cleanup and refused admission.",
      "title": "A failed unsubscribe skips reconciliation of observed units",
      "trigger": "Early release or abandonment removes a witness whose native unsubscribe throws."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "Final release unconditionally calls the global EntityDestroyer.Tick and then calls RecordUnsettled in the same action. The tick can advance unrelated pending destruction, even with an empty owned set. If it throws, the report contains the generic action error and skips the per-unit reads. The repository's ElementalCompletionSceneScenario checks pending scene work before advancing this controller.",
      "blocking_rationale": "Final cleanup can act on foreign work or lose the identities of the units whose settlement the mission requires it to report.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Final release confines any forced engine work to owned resources and attempts each requested unit's settlement read even if the engine drain fails.",
      "id": "R1-F4",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 102
      },
      "priority": "P1",
      "requested_correction": "Use normal engine progression or establish that a forced drain is confined to owned work. Record any drain failure independently and still publish per-unit unresolved results.",
      "requested_evidence": "Cover abandonment with unrelated pending destruction and with a failing drain. Verify foreign work is preserved and that the slot receives both the drain error and the affected owned identities.",
      "title": "Final settlement depends on an unconditional global destroyer tick",
      "trigger": "Final release encounters foreign pending destruction or the destroyer throws before per-unit readback."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "SettleOwned writes the evidence row from Unsettled(), then RecordUnsettled() queries every unit again. A terminal row can name a present or unreadable unit while the second read succeeds and contributes no cumulative failure.",
      "blocking_rationale": "A unit observed unresolved at the end of the allowed wait can be lost from the decision that admits the next probe.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The evidence row and cleanup outcome consume the same terminal observation for each requested identity.",
      "id": "R1-F5",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 1016
      },
      "priority": "P2",
      "requested_correction": "Capture the terminal settlement results once and use that snapshot to populate both the row and sticky cleanup failures.",
      "requested_evidence": "Use a controlled reader whose terminal read is unreadable and whose following read succeeds. Show that the emitted row and slot outcome retain the same unresolved identity.",
      "title": "The evidence row and slot use separate terminal reads",
      "trigger": "A settlement read is unreadable at the deadline and succeeds when immediately retried."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "The late case records lateUnitDestroyRequestedByReconciliation, calls the fallback Destroy, and gates its verdict only on the post-fallback request flag. It also samples foreignPreserved before the witness release that can now destroy units. The supplied run recorded the intended request, but the verdict would also accept fallback-only cleanup or foreign loss during that release.",
      "blocking_rationale": "The short native qualification can pass when the specific caller behavior it is meant to qualify regresses.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The lifecycle verdict requires the witness caller's request and checks foreign survival after reconciliation, while the foreign fixture is still expected to be alive.",
      "id": "R1-F6",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 805
      },
      "priority": "P2",
      "requested_correction": "Include the pre-fallback request observation in the verdict. Check the identified foreign control after witness release and settlement, before its separately owned teardown.",
      "requested_evidence": "Show that a skipped reconciliation request fails the lifecycle check even when fallback cleanup succeeds, and that the corrected native row records the foreign control alive and unrequested at the post-release boundary.",
      "title": "Fallback cleanup and an early foreign read can satisfy the native verdict",
      "trigger": "Reconciliation skips the seeded late unit, or affects the foreign control after its survival was sampled."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "LateUnitProbe uses the real slot and destruction helper, but manually fills the witness lists and substitutes its own release and settlement sequence. Its Foreign object never enters the observed controls. The link back to GhaeleLifetimeProbe is a two-substring source assertion. Those tests can stay green when the production unsubscribe or final-drain ordering drops work.",
      "blocking_rationale": "The mission requires behavioral evidence through the actual caller and admission path for material failure cases.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Controlled tests exercise the release orchestration used by GhaeleLifetimeProbe, with native operations replaced at their boundaries and an actual known foreign observation.",
      "id": "R1-F7",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 960
      },
      "priority": "P2",
      "requested_correction": "Drive the production caller's release and settlement sequence from the domain tests. Supply the observed owned, known-foreign and unknown identities, and assert the resulting slot outcome on completion and abandonment.",
      "requested_evidence": "Provide focused receipts for the corrected caller showing one request per owned identity, survival of the participating foreign control, continued independent cleanup, named residuals and refused next admission.",
      "title": "The failure tests substitute the caller that coordinates native cleanup",
      "trigger": "The new regression is used to establish foreign survival or failure handling in the production Ghaele caller."
    },
    {
      "acceptance_ids": [],
      "actual": "The ledger still says that no harness build or game session has run. Its following rows record both C2 operations and sit below the prose outside the table.",
      "blocking_rationale": "The session receipt and recorded costs identify the actual run.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The current ledger distinguishes the historical C1 state from the C2 harness build and native session.",
      "id": "R1-F8",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 157
      },
      "priority": "P3",
      "requested_correction": "Date the historical statement or update it, and put the appended rows in a rendered ledger table.",
      "requested_evidence": "A text and rendering check against the existing receipts is sufficient.",
      "title": "The shared ledger retains a stale pre-C2 status",
      "trigger": "A reader consults the recovery ledger for the current harness and native validation spend."
    }
  ],
  "prior_finding_assessments": [],
  "request_id": "kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r01-e7efe0",
  "round": 1,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "On successful witness release, the caller adopts correlated units, deduplicates destruction requests by reference, and records persistent settlement failures in the cleanup report consumed by ProbeSlot. The identified native session demonstrated the ordinary post-disposal control and completed transactional restoration.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Visual and listening acceptance of the existing Sprint 7 material.",
      "Integration of the dependent repair stack with frozen PR #219 and any subsequent merge or release decision."
    ],
    "scope_reviewed": "Final C2 external-harness correction for inherited R5-F1 in PR #222, based on C1 archive 2c67a049ee33e0b29621fd29a8b25608ed31e956. Combined effects and cue qualification belongs to C3.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
