<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r03-review part=1/1 sha256=dad7583f8859ad3a58fc3368b84feb2db5bc3913a036f45a8c59d775899589e0 -->
### Codex GPT-6.2 Astra — review — round 3 — candidate `89c7229e20`

**Decision:** `changes_required` — All four pending findings are verified fixed. Two cleanup paths still violate OWNER AMENDMENT 1. Abandoning a cast can advance unrelated entity creation, and a witness left subscribed can report a later cast’s identified summon and foreign control as unknown.

Continuation for Claude

This is review 3 of 3. Preserve the mandatory stop and await owner direction on the existing enrollment. If further work is authorized, confine cast abandonment to the exact owned unit and use the positive matches and explicit control identities available across the probe when classifying final residuals. Preserve the original unsubscribe error, genuine unknowns, the one-request guard and the shared terminal snapshot. Exercise both paths through ProbeSlot with controlled boundaries. Any changed harness needs the two focused gates and the short lifecycle session on candidate 3 within the shared allowance. C3 depends on a current C2 handoff.

#### Findings

**R3-F1** · P1 · defect · blocks readiness — Cast abandonment advances the global entity-creation queue
- Location: `tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs:310`; acceptance: R5-F1
- Trigger: The slot unwinds an active cast while an unrelated entity is waiting for registration.
- Expected: Abandonment requests cleanup for the exact owned summon and reports its terminal state while leaving foreign pending registrations untouched.
- Actual: The held `unregistered summons` callback calls `Game.Instance.EntityCreator.Tick()` before it even reads `handle.Summoned`. Cast disposal and the enclosing probe’s unwind both reach this callback. The tick advances the shared creation queue, including when this cast has no summon. The abandonment fixture’s `unrelatedPending` object never participates in that queue.
- Why it blocks / does not: OWNER AMENDMENT 1 explicitly requires abandonment to leave unrelated engine work alone. A foreign entity can enter game state during cleanup without appearing in the probe’s outcome.
- Requested correction: Remove the unconditional creator tick from the abandonment path. Use the retained summon reference for its guarded request and let final settlement report any pending or unreadable state.
- Requested evidence: Through the held cast-release path and `ProbeSlot.Abandon`, leave a foreign creation queued. Verify that it stays pending, the owned unit receives one request and a terminal result or named residual, and the original timeout survives.

**R3-F2** · P2 · defect · blocks readiness — Final reconciliation can report another witness’s identified units as unknown
- Location: `tools/LoaderCompatHarness/TrumpetCueVerdict.cs:402`; acceptance: R5-F1
- Trigger: Early unsubscribe fails before detaching the pending witness, and the lifecycle proceeds to the later spawn-detection case with both witnesses receiving same-caster results.
- Expected: Final cleanup recognizes the exact summon and foreign control the probe has identified. It retains the unsubscribe error and names any genuinely unattributed unit.
- Actual: Each final `ReconcileLatest` classifies unknowns using only its own witness’s `Owned` and `Known` lists. The pending witness can therefore add an `ownership unknown` failure for the same summon that the spawn witness positively matched, and for the foreign control explicitly added to `spawnWitness.Known`. Both observations can first arrive after the pending witness’s early reconciliation.
- Why it blocks / does not: The unsubscribe error correctly refuses admission. The cumulative cleanup report nevertheless identifies accounted units as unresolved, contrary to the amendment’s requirement to account for positive correlation and explicit foreign identities before reporting residual unknowns.
- Requested correction: Resolve final unknowns by reference using the positive cast matches and explicit control identities from all relevant witnesses. Preserve each witness’s observation history, independent fixture-teardown ownership and genuine earlier failures.
- Requested evidence: Keep the first witness subscribed after its failed early release. Deliver the same real-spawn reference to both witnesses and identify the foreign control in the second. Assert one owned request, an untouched foreign control during reconciliation, and a named failure only for a separate genuinely unknown reference alongside the original unsubscribe error. Verify the next Begin is refused.

**R3-F3** · P3 · optional_polish · non-blocking — Clean-settlement assertions accept a missing evidence field
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:1028`; acceptance: R5-F1
- Trigger: A successful cleanup omits `unsettledAfterWitness` or `unsettledAtFinalRelease`.
- Expected: A clean-settlement assertion requires a recorded empty array.
- Actual: The new `rowList` helper converts a missing field to an empty array. Assertions expecting zero residuals therefore also pass when the settlement field is omitted.
- Why it blocks / does not: The current finalizer writes the array, and the qualifying native row contains it. This affects the regression’s ability to detect a future omission.
- Requested correction: Require a `JArray` at each asserted settlement field before reading its entries.
- Requested evidence: The existing focused case should accept an explicit empty array and fail if the field is absent.

#### Prior finding assessments

- **R1-F7** — `fix_verified` — The production probe and domain fixture now call the same `LateUnitFinalRelease.Release`. Case C reached it through `ProbeSlot.Abandon` before ordinary settlement. A single changing terminal read put `extra` in both the final row and slot failure. The timeout and destruction error survived, independent cleanup ran and the next Begin was refused.
- **R2-F1** — `fix_verified` — Final release revisits every retained caller after held cleanup. In case A, the still-subscribed witness received a correlated unit and an unknown after early reconciliation. The correlated unit received one request, the unknown was named and left alone, and the original unsubscribe error reached the slot. The pending variant named the later unit in both row and outcome.
- **R2-F2** — `fix_verified` — For the reported same-witness trigger, positive attribution now removes the exact reference from residual unknowns. Case B recorded one request, terminal destruction and restored isolation. The teardown-only control still produced the required uncertainty failure. R3-F2 concerns observations split across two active witnesses.
- **R2-F3** — `fix_verified` — The finding map and ledger identify all three C2 runs. The session receipts match `002852Z` to `c5fe26d146` and harness `0a038e40`, `005206Z` to `44fd90a8d1` and `bf5d74a1`, and `024816Z` to `56e359f2cc` and `4180621b`. The manifest also restores the omitted R2-F1 to the stop record.

#### Evidence assessment

- **Inspected:** The controller mission, OWNER AMENDMENT 1, pending findings ledger, developer report, transcript notice, repository instructions and full feature diff.; The native witness and cast-release paths, shared finalizer, ProbeSlot admission, domain fixture, lifecycle session definition, three C2 session and launch records, and recovery manifest.
- **Executed verification:** Controller receipts and matching log hashes confirm production compilation and both focused gates on H. `sprint7-visual` passed 23 tests in a 2.8-second receipt, including `ghaele-late-units-reach-slot`. `loader-compat-probe` passed 5 tests in a 2.0-second receipt.; The committed native records identify session `20261126T024816Z` on `owner-0.33.0`, harness `4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61` from `56e359f2cc536f6f0434507cf465760df85c2e44`, candidate-3 package `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413` and product DLL `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`. Only records changed between that harness source and H.; The native row records a reconciliation request before fallback, exactly one request for `cfec1089-f7c4-4f23-8a52-0e601a4fcb46`, and terminal destruction. The late and real-spawn controls were alive and unrequested after settlement. All three final witness reconciliations ran, final settlement was empty, and the slot reported restored isolation. Transaction `b7008ac8a66a4efa827996f0e40e1153` records verified owner files and saves and matching registry readback.; Read-only Git and hash checks confirmed H, the merge base, the mission and amendment digests, and the round-3 diff. The full feature diff’s whitespace check identified only Markdown line breaks in the verbatim, hash-matched mission.
- **Source inspection only:** Traced the held `CastOwned` callback through iterator disposal and `Owned.ReleaseAll`. The repository’s `CompleteOwnedRegistration` implementation documents the shared `m_ToCreate` queue advanced by `EntityCreator.Tick()`.; Traced a failed pending-witness unsubscribe into the later real-spawn case. The two witnesses share the destruction owner and retain separate positive-match and known-control lists. The focused failure fixture constructs one witness.
- **Required gaps:** Demonstrate that abandoning the actual held cast cleanup leaves a foreign pending creation queued while accounting for the owned unit.; Demonstrate final ownership classification with two overlapping witnesses, including a positively identified summon, an explicitly identified foreign control and a genuinely unknown reference.
- **Limitations:** The reviewer used the read-only snapshot. The controller supplied gate receipts and logs and withheld the mixed foreground shell transcript.; The native run covers ordinary slot completion with a controlled post-disposal notification. The failed-unsubscribe and enclosing-slot abandonment cases ran in the domain gate.; The committed ledger reaches 11m57.8s before the final gates. The developer reports 8.7s of additional wrapper time, totaling 12m06.5s of 45m and leaving 32m53.5s. C2 used zero new product candidates.

#### Summary

- **Delivered behavior:** The external harness shares its final release sequence with the domain fixture, reconciles later arrivals after failed unsubscribe, and settles requested units from one terminal snapshot. The short native lifecycle passed on the rebuilt harness and reused candidate-3 package.
- **Scope reviewed:** Final C2 external-harness checkpoint for parent R5-F1 under OWNER AMENDMENT 1, including ordinary release, failed early unsubscribe, abandonment, slot admission and artifact identity.
- **Pending later-phase work:** C3’s effects isolation and same-run live-versus-persisted evaluator qualification depend on a current C2 handoff.
- **Unresolved questions:** none
- **Remaining owner review:** Decide the disposition of C2 at its authorized three-review cap.; Visual and listening acceptance and eventual integration of the frozen parent stack remain owner decisions.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 1, sha256 `d62882e3355874e552829475113faf7afcc304a36c387d04ca29850cd08d4626`.

Codex CLI `0.170.0`, thread `01a215c5-edb5-7c93-bf5c-8887522a7dd2`, duration 628.0s, response sha256 `759af91c73d4e62380d8875c24c30aec72013c33e8a3d382a65dad491b17fc6b`, request `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r03-ec7375`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "89c7229e208a498e71caa91e7e4225e867c0d5d8",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "All four pending findings are verified fixed. Two cleanup paths still violate OWNER AMENDMENT 1. Abandoning a cast can advance unrelated entity creation, and a witness left subscribed can report a later cast\u2019s identified summon and foreign control as unknown.\n\nContinuation for Claude\n\nThis is review 3 of 3. Preserve the mandatory stop and await owner direction on the existing enrollment. If further work is authorized, confine cast abandonment to the exact owned unit and use the positive matches and explicit control identities available across the probe when classifying final residuals. Preserve the original unsubscribe error, genuine unknowns, the one-request guard and the shared terminal snapshot. Exercise both paths through ProbeSlot with controlled boundaries. Any changed harness needs the two focused gates and the short lifecycle session on candidate 3 within the shared allowance. C3 depends on a current C2 handoff.",
  "evidence_assessment": {
    "executed_verification": [
      "Controller receipts and matching log hashes confirm production compilation and both focused gates on H. `sprint7-visual` passed 23 tests in a 2.8-second receipt, including `ghaele-late-units-reach-slot`. `loader-compat-probe` passed 5 tests in a 2.0-second receipt.",
      "The committed native records identify session `20261126T024816Z` on `owner-0.33.0`, harness `4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61` from `56e359f2cc536f6f0434507cf465760df85c2e44`, candidate-3 package `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413` and product DLL `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`. Only records changed between that harness source and H.",
      "The native row records a reconciliation request before fallback, exactly one request for `cfec1089-f7c4-4f23-8a52-0e601a4fcb46`, and terminal destruction. The late and real-spawn controls were alive and unrequested after settlement. All three final witness reconciliations ran, final settlement was empty, and the slot reported restored isolation. Transaction `b7008ac8a66a4efa827996f0e40e1153` records verified owner files and saves and matching registry readback.",
      "Read-only Git and hash checks confirmed H, the merge base, the mission and amendment digests, and the round-3 diff. The full feature diff\u2019s whitespace check identified only Markdown line breaks in the verbatim, hash-matched mission."
    ],
    "inspected": [
      "The controller mission, OWNER AMENDMENT 1, pending findings ledger, developer report, transcript notice, repository instructions and full feature diff.",
      "The native witness and cast-release paths, shared finalizer, ProbeSlot admission, domain fixture, lifecycle session definition, three C2 session and launch records, and recovery manifest."
    ],
    "limitations": [
      "The reviewer used the read-only snapshot. The controller supplied gate receipts and logs and withheld the mixed foreground shell transcript.",
      "The native run covers ordinary slot completion with a controlled post-disposal notification. The failed-unsubscribe and enclosing-slot abandonment cases ran in the domain gate.",
      "The committed ledger reaches 11m57.8s before the final gates. The developer reports 8.7s of additional wrapper time, totaling 12m06.5s of 45m and leaving 32m53.5s. C2 used zero new product candidates."
    ],
    "required_gaps": [
      "Demonstrate that abandoning the actual held cast cleanup leaves a foreign pending creation queued while accounting for the owned unit.",
      "Demonstrate final ownership classification with two overlapping witnesses, including a positively identified summon, an explicitly identified foreign control and a genuinely unknown reference."
    ],
    "source_inspection_only": [
      "Traced the held `CastOwned` callback through iterator disposal and `Owned.ReleaseAll`. The repository\u2019s `CompleteOwnedRegistration` implementation documents the shared `m_ToCreate` queue advanced by `EntityCreator.Tick()`.",
      "Traced a failed pending-witness unsubscribe into the later real-spawn case. The two witnesses share the destruction owner and retain separate positive-match and known-control lists. The focused failure fixture constructs one witness."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "The held `unregistered summons` callback calls `Game.Instance.EntityCreator.Tick()` before it even reads `handle.Summoned`. Cast disposal and the enclosing probe\u2019s unwind both reach this callback. The tick advances the shared creation queue, including when this cast has no summon. The abandonment fixture\u2019s `unrelatedPending` object never participates in that queue.",
      "blocking_rationale": "OWNER AMENDMENT 1 explicitly requires abandonment to leave unrelated engine work alone. A foreign entity can enter game state during cleanup without appearing in the probe\u2019s outcome.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Abandonment requests cleanup for the exact owned summon and reports its terminal state while leaving foreign pending registrations untouched.",
      "id": "R3-F1",
      "location": {
        "file": "tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs",
        "line": 310
      },
      "priority": "P1",
      "requested_correction": "Remove the unconditional creator tick from the abandonment path. Use the retained summon reference for its guarded request and let final settlement report any pending or unreadable state.",
      "requested_evidence": "Through the held cast-release path and `ProbeSlot.Abandon`, leave a foreign creation queued. Verify that it stays pending, the owned unit receives one request and a terminal result or named residual, and the original timeout survives.",
      "title": "Cast abandonment advances the global entity-creation queue",
      "trigger": "The slot unwinds an active cast while an unrelated entity is waiting for registration."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "Each final `ReconcileLatest` classifies unknowns using only its own witness\u2019s `Owned` and `Known` lists. The pending witness can therefore add an `ownership unknown` failure for the same summon that the spawn witness positively matched, and for the foreign control explicitly added to `spawnWitness.Known`. Both observations can first arrive after the pending witness\u2019s early reconciliation.",
      "blocking_rationale": "The unsubscribe error correctly refuses admission. The cumulative cleanup report nevertheless identifies accounted units as unresolved, contrary to the amendment\u2019s requirement to account for positive correlation and explicit foreign identities before reporting residual unknowns.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Final cleanup recognizes the exact summon and foreign control the probe has identified. It retains the unsubscribe error and names any genuinely unattributed unit.",
      "id": "R3-F2",
      "location": {
        "file": "tools/LoaderCompatHarness/TrumpetCueVerdict.cs",
        "line": 402
      },
      "priority": "P2",
      "requested_correction": "Resolve final unknowns by reference using the positive cast matches and explicit control identities from all relevant witnesses. Preserve each witness\u2019s observation history, independent fixture-teardown ownership and genuine earlier failures.",
      "requested_evidence": "Keep the first witness subscribed after its failed early release. Deliver the same real-spawn reference to both witnesses and identify the foreign control in the second. Assert one owned request, an untouched foreign control during reconciliation, and a named failure only for a separate genuinely unknown reference alongside the original unsubscribe error. Verify the next Begin is refused.",
      "title": "Final reconciliation can report another witness\u2019s identified units as unknown",
      "trigger": "Early unsubscribe fails before detaching the pending witness, and the lifecycle proceeds to the later spawn-detection case with both witnesses receiving same-caster results."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "The new `rowList` helper converts a missing field to an empty array. Assertions expecting zero residuals therefore also pass when the settlement field is omitted.",
      "blocking_rationale": "The current finalizer writes the array, and the qualifying native row contains it. This affects the regression\u2019s ability to detect a future omission.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "A clean-settlement assertion requires a recorded empty array.",
      "id": "R3-F3",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 1028
      },
      "priority": "P3",
      "requested_correction": "Require a `JArray` at each asserted settlement field before reading its entries.",
      "requested_evidence": "The existing focused case should accept an explicit empty array and fail if the field is absent.",
      "title": "Clean-settlement assertions accept a missing evidence field",
      "trigger": "A successful cleanup omits `unsettledAfterWitness` or `unsettledAtFinalRelease`."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F7",
      "rationale": "The production probe and domain fixture now call the same `LateUnitFinalRelease.Release`. Case C reached it through `ProbeSlot.Abandon` before ordinary settlement. A single changing terminal read put `extra` in both the final row and slot failure. The timeout and destruction error survived, independent cleanup ran and the next Begin was refused."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F1",
      "rationale": "Final release revisits every retained caller after held cleanup. In case A, the still-subscribed witness received a correlated unit and an unknown after early reconciliation. The correlated unit received one request, the unknown was named and left alone, and the original unsubscribe error reached the slot. The pending variant named the later unit in both row and outcome."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F2",
      "rationale": "For the reported same-witness trigger, positive attribution now removes the exact reference from residual unknowns. Case B recorded one request, terminal destruction and restored isolation. The teardown-only control still produced the required uncertainty failure. R3-F2 concerns observations split across two active witnesses."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F3",
      "rationale": "The finding map and ledger identify all three C2 runs. The session receipts match `002852Z` to `c5fe26d146` and harness `0a038e40`, `005206Z` to `44fd90a8d1` and `bf5d74a1`, and `024816Z` to `56e359f2cc` and `4180621b`. The manifest also restores the omitted R2-F1 to the stop record."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r03-ec7375",
  "round": 3,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The external harness shares its final release sequence with the domain fixture, reconciles later arrivals after failed unsubscribe, and settles requested units from one terminal snapshot. The short native lifecycle passed on the rebuilt harness and reused candidate-3 package.",
    "pending_later_phase_work": [
      "C3\u2019s effects isolation and same-run live-versus-persisted evaluator qualification depend on a current C2 handoff."
    ],
    "remaining_owner_review": [
      "Decide the disposition of C2 at its authorized three-review cap.",
      "Visual and listening acceptance and eventual integration of the frozen parent stack remain owner decisions."
    ],
    "scope_reviewed": "Final C2 external-harness checkpoint for parent R5-F1 under OWNER AMENDMENT 1, including ordinary release, failed early unsubscribe, abandonment, slot admission and artifact identity.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
