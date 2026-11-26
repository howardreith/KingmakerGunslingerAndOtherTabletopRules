<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r02-review part=1/1 sha256=7b4b87bab87d579c36c33fb9b584157a57cf413a155fb934b62bb6cb346af5b3 -->
### Codex GPT-6.2 Astra — review — round 2 — candidate `525a27751b`

**Decision:** `changes_required` — A failed unsubscribe can leave the witness receiving results after its only reconciliation. Final cleanup then misses those units. The caller can also report a positively identified unit as unknown, and the abandonment test still supplies its own final cleanup sequence.

## Continuation for Claude

Preserve the mandatory stop at review 2 of 2 and report these findings to the owner. C3 requires a current C2 handoff. If the owner authorizes further C2 work, retain witness observations through final cleanup, resolve ownership by unit identity, and exercise the production finalizer through ProbeSlot. The focused evidence should show what happens to a result delivered after a failed unsubscribe, including its request, settlement and effect on admission. Keep any requalification within the external harness, the existing candidate-3 package, the short lifecycle session and the shared recovery allowance.

#### Findings

**R2-F1** · P1 · defect · blocks readiness — Final cleanup misses results received after a failed unsubscribe
- Location: `tools/LoaderCompatHarness/TrumpetCueVerdict.cs:372`; acceptance: R5-F1
- Trigger: Early unsubscribe throws before detaching the witness, then another result arrives while the same probe is still running.
- Expected: Final release accounts for every result the witness received before the slot ended and retains the unsubscribe error.
- Actual: ReleaseNow removes each held action before invoking it. WitnessCaller consumes its reconciliation even when unsubscribe throws. The still-subscribed witness can append further identities, but GhaeleLifetimeProbe.Release only drains and settles the disposable inventory. A newly observed unit held solely by the witness never enters that inventory.
- Why it blocks / does not: The original unsubscribe error blocks the next probe, but an identified owned unit can remain alive without a destruction request or a per-unit cleanup result.
- Requested correction: Keep the observations of a possibly subscribed witness available to final cleanup. Reconcile later arrivals before final settlement, using the existing per-unit request guard.
- Requested evidence: Fail early unsubscribe while leaving delivery active. Deliver an additional correlated unit and an unknown unit before slot termination. Show one request and a terminal result or named residual for the correlated unit, a named failure for the unknown, the original unsubscribe error, foreign survival and refused admission.

**R2-F2** · P2 · defect · blocks readiness — Positive attribution does not clear an earlier unknown observation
- Location: `tools/LoaderCompatHarness/TrumpetCueVerdict.cs:346`; acceptance: R5-F1
- Trigger: One witness receives an unattributed result and a later positively matched result for the same unit before release.
- Expected: Positive attribution resolves that unit's earlier uncertainty. A unit with only a fixture-teardown registration remains unknown.
- Actual: Observed.Unknown excludes only Known controls. When the same reference appears in Unattributed and Owned, Release adopts and requests it, then RecordUncertain reports that its ownership is unknown and it was left in place.
- Why it blocks / does not: A conclusively attributed and successfully settled unit can produce a false cleanup failure and prevent the next probe from starting.
- Requested correction: Compute residual unknowns by reference after accounting for positively correlated units and explicit foreign controls. Preserve the independent fixture-teardown rule.
- Requested evidence: Supply an unattributed observation followed by a positively correlated observation for the same reference. Verify one request, successful settlement, an empty residual-unknown set and clean admission. Retain the registered-but-unattributed refusal case.

**R1-F7** · P2 · evidence_gap · blocks readiness — The abandonment regression still substitutes the final cleanup sequence
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:990`; acceptance: R5-F1
- Trigger: The regression is used to qualify final settlement when the enclosing probe is abandoned.
- Expected: The controlled abandonment case executes the production sequence that requests remaining units, reads settlement and returns cleanup to ProbeSlot.
- Actual: LateUnitProbe.Release constructs its own ReleaseRemaining and Settle action array. GhaeleLifetimeProbe.Release constructs a separate array. Omitting final settlement from the production array would leave the domain abandonment assertion green. Its source check only matches the ordinary SettleOwned call.
- Why it blocks / does not: The mission requires behavioral evidence through the caller that determines isolation on abandonment. The native session covers ordinary completion of that caller.
- Requested correction: Exercise the production finalizer from the slot regression, either directly with controlled native operations or through one final release sequence shared by the production probe and the fixture.
- Requested evidence: Abandon before ordinary settlement. Show that the actual final row and ProbeSlot carry the same unresolved unit, the original timeout survives, independent cleanup runs and the next Begin is refused. A terminal read that becomes readable on the next call would also distinguish the repaired single-snapshot behavior.

**R2-F3** · P3 · optional_polish · non-blocking — The handoff map points to the first C2 harness
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:109`; acceptance: R5-F1
- Trigger: A reader uses the parent-finding map to select the Ghaele harness identity for integration.
- Expected: The finding map identifies the harness used for the reviewed source and preserves the earlier run as history.
- Actual: The R5-F1 row names only `002852Z` and harness `0a038e40` from round 1. The status section and session receipts identify `005206Z` and harness `bf5d74a1` for round 2. The ledger note at line 171 also says C2 ran one session although it lists both runs.
- Why it blocks / does not: The correct artifact identity and both runs are recoverable from the same manifest and the receipts.
- Requested correction: Add the round-2 source, harness and session to the R5-F1 mapping and update the session count in the ledger note.
- Requested evidence: A text check against the two committed session receipts is sufficient.

#### Prior finding assessments

- **R1-F1** — `fix_verified` — Terminal returns unresolved for an undestroyed unit absent from State.Units. The controlled pending-registration case recorded one request, a named unresolved unit and refusal of the next probe.
- **R1-F2** — `fix_verified` — RecordUncertain now retains the unknown identity even when the fixture owns its teardown. The registered and unregistered controls exercise both outcomes. R2-F2 concerns a unit with subsequent positive attribution.
- **R1-F3** — `fix_verified` — Subscription release and reconciliation run as independent held actions. The focused case preserves the unsubscribe exception and settles the extra unit observed before that exception. R2-F1 concerns later delivery.
- **R1-F4** — `fix_verified` — Final settlement no longer invokes the global destroyer tick. It reads each requested unit and records a pending or unreadable result by identity. Removing the forced drain eliminates its exception path.
- **R1-F5** — `fix_verified` — LateResultReconciliation.Settle takes one terminal snapshot and passes it to both the evidence row and RecordUnsettled. The focused fixture verifies agreement for a consistently unresolved unit.
- **R1-F6** — `fix_verified` — The lifecycle check requires the flag sampled before fallback. Foreign survival is read after settlement, and the late and real-spawn native rows explicitly record the foreign control unrequested at that boundary.
- **R1-F7** — `fix_rejected` — The regression now uses the production WitnessCaller and a participating foreign control. Its own final action array still determines the abandonment result, leaving the production finalizer outside the failure test.
- **R1-F8** — `fix_verified` — The pre-C2 statement is dated through C1 and all expenditure rows are in one Markdown table. The separate round-2 handoff discrepancy is recorded as R2-F3.

#### Evidence assessment

- **Inspected:** The enrolled mission and its SHA-256, applicable repository instructions, full feature diff, developer report, transcript notice and all eight prior findings.; The Ghaele witness and its three lifecycle callers, OwnedDisposables, YieldedProbeLifecycle, ProbeSlot, HarnessRunner admission, the domain regression and both C2 session records.
- **Executed verification:** Controller receipts on H confirm production compilation and two passing Focused gates. `sprint7-visual` passed 23 tests in a 2.1-second gate, including `ghaele-late-units-reach-slot`. `loader-compat-probe` passed 5 tests in a 1.6-second gate.; Native session `20261126T005206Z-owner-0.33.0-s7-ghaele-lifecycle` records candidate-3 ZIP `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`, product DLL `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3` and harness `bf5d74a12487a92ac4d00a792a17a1ecaa41407308728ee7cc3d4d9f405163cd` from `44fd90a8d1415813271103af89463ee6bd872e93`. Git confirms the harness source tree and session definition are identical at that commit and H.; The native row records one request for late unit `02c64a2e-9801-4811-99e7-13fff588e22a`, a true pre-fallback request flag and terminal destruction. The late and real-spawn cases record their foreign controls alive and unrequested after settlement. Final cleanup had zero failures. The session receipt records completed restoration, unchanged owner installation and saves, and matching registry hashes.
- **Source inspection only:** The two ownership defects are traces through the changed caller. The recorded native run used successful unsubscriptions and distinct owned and foreign identities.; The single-snapshot implementation and pre-fallback verdict condition are visible in production source. The domain fixture used a consistently unresolved reader and checked the pre-fallback condition by searching source text.
- **Required gaps:** Account for identities received between an unsuccessful early unsubscribe and final release.; Resolve an earlier unattributed observation when the same unit subsequently receives positive attribution.; Demonstrate abandonment through the final cleanup sequence used by GhaeleLifetimeProbe, with the unresolved identity visible in both the final row and the admission outcome.
- **Limitations:** The foreground transcript supplies gate-receipt references only. Native observations come from the committed launch and session extracts. The reviewer performed read-only inspection.; The native session completed the enclosing probe normally. Its abandonment cases disposed individual cast owners. The injected late result was a labelled controlled notification.; The committed ledger ends at 9m33.1s. The developer reports 9m40.5s of the shared 45 minutes after the final gate wrappers, with zero new product packages.

#### Summary

- **Delivered behavior:** The caller adopts observed late units, deduplicates destruction requests and reports unresolved terminal reads to ProbeSlot. The short native lifecycle run qualified the successful release path on candidate 3. Results arriving after an unsuccessful early unsubscribe remain unaccounted for.
- **Scope reviewed:** Final C2 external-harness correction for parent R5-F1 on draft PR #222, including its affected callers, focused gates and lifecycle qualification.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Disposition of C2 at its two-review stop before the dependent C3 child can begin.; Stack integration and the retained visual and listening decisions.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a2155a-bbd5-78a0-b97f-6538739286df`, duration 634.3s, response sha256 `a6fa7883dab0d284eb022824c01df03f3b77e9025d0b74a96e36366efd96da02`, request `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r02-a67b64`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "525a27751b95fd51f3080b2a7865719532fff298",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "A failed unsubscribe can leave the witness receiving results after its only reconciliation. Final cleanup then misses those units. The caller can also report a positively identified unit as unknown, and the abandonment test still supplies its own final cleanup sequence.\n\n## Continuation for Claude\n\nPreserve the mandatory stop at review 2 of 2 and report these findings to the owner. C3 requires a current C2 handoff. If the owner authorizes further C2 work, retain witness observations through final cleanup, resolve ownership by unit identity, and exercise the production finalizer through ProbeSlot. The focused evidence should show what happens to a result delivered after a failed unsubscribe, including its request, settlement and effect on admission. Keep any requalification within the external harness, the existing candidate-3 package, the short lifecycle session and the shared recovery allowance.",
  "evidence_assessment": {
    "executed_verification": [
      "Controller receipts on H confirm production compilation and two passing Focused gates. `sprint7-visual` passed 23 tests in a 2.1-second gate, including `ghaele-late-units-reach-slot`. `loader-compat-probe` passed 5 tests in a 1.6-second gate.",
      "Native session `20261126T005206Z-owner-0.33.0-s7-ghaele-lifecycle` records candidate-3 ZIP `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`, product DLL `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3` and harness `bf5d74a12487a92ac4d00a792a17a1ecaa41407308728ee7cc3d4d9f405163cd` from `44fd90a8d1415813271103af89463ee6bd872e93`. Git confirms the harness source tree and session definition are identical at that commit and H.",
      "The native row records one request for late unit `02c64a2e-9801-4811-99e7-13fff588e22a`, a true pre-fallback request flag and terminal destruction. The late and real-spawn cases record their foreign controls alive and unrequested after settlement. Final cleanup had zero failures. The session receipt records completed restoration, unchanged owner installation and saves, and matching registry hashes."
    ],
    "inspected": [
      "The enrolled mission and its SHA-256, applicable repository instructions, full feature diff, developer report, transcript notice and all eight prior findings.",
      "The Ghaele witness and its three lifecycle callers, OwnedDisposables, YieldedProbeLifecycle, ProbeSlot, HarnessRunner admission, the domain regression and both C2 session records."
    ],
    "limitations": [
      "The foreground transcript supplies gate-receipt references only. Native observations come from the committed launch and session extracts. The reviewer performed read-only inspection.",
      "The native session completed the enclosing probe normally. Its abandonment cases disposed individual cast owners. The injected late result was a labelled controlled notification.",
      "The committed ledger ends at 9m33.1s. The developer reports 9m40.5s of the shared 45 minutes after the final gate wrappers, with zero new product packages."
    ],
    "required_gaps": [
      "Account for identities received between an unsuccessful early unsubscribe and final release.",
      "Resolve an earlier unattributed observation when the same unit subsequently receives positive attribution.",
      "Demonstrate abandonment through the final cleanup sequence used by GhaeleLifetimeProbe, with the unresolved identity visible in both the final row and the admission outcome."
    ],
    "source_inspection_only": [
      "The two ownership defects are traces through the changed caller. The recorded native run used successful unsubscriptions and distinct owned and foreign identities.",
      "The single-snapshot implementation and pre-fallback verdict condition are visible in production source. The domain fixture used a consistently unresolved reader and checked the pre-fallback condition by searching source text."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "ReleaseNow removes each held action before invoking it. WitnessCaller consumes its reconciliation even when unsubscribe throws. The still-subscribed witness can append further identities, but GhaeleLifetimeProbe.Release only drains and settles the disposable inventory. A newly observed unit held solely by the witness never enters that inventory.",
      "blocking_rationale": "The original unsubscribe error blocks the next probe, but an identified owned unit can remain alive without a destruction request or a per-unit cleanup result.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Final release accounts for every result the witness received before the slot ended and retains the unsubscribe error.",
      "id": "R2-F1",
      "location": {
        "file": "tools/LoaderCompatHarness/TrumpetCueVerdict.cs",
        "line": 372
      },
      "priority": "P1",
      "requested_correction": "Keep the observations of a possibly subscribed witness available to final cleanup. Reconcile later arrivals before final settlement, using the existing per-unit request guard.",
      "requested_evidence": "Fail early unsubscribe while leaving delivery active. Deliver an additional correlated unit and an unknown unit before slot termination. Show one request and a terminal result or named residual for the correlated unit, a named failure for the unknown, the original unsubscribe error, foreign survival and refused admission.",
      "title": "Final cleanup misses results received after a failed unsubscribe",
      "trigger": "Early unsubscribe throws before detaching the witness, then another result arrives while the same probe is still running."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "Observed.Unknown excludes only Known controls. When the same reference appears in Unattributed and Owned, Release adopts and requests it, then RecordUncertain reports that its ownership is unknown and it was left in place.",
      "blocking_rationale": "A conclusively attributed and successfully settled unit can produce a false cleanup failure and prevent the next probe from starting.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Positive attribution resolves that unit's earlier uncertainty. A unit with only a fixture-teardown registration remains unknown.",
      "id": "R2-F2",
      "location": {
        "file": "tools/LoaderCompatHarness/TrumpetCueVerdict.cs",
        "line": 346
      },
      "priority": "P2",
      "requested_correction": "Compute residual unknowns by reference after accounting for positively correlated units and explicit foreign controls. Preserve the independent fixture-teardown rule.",
      "requested_evidence": "Supply an unattributed observation followed by a positively correlated observation for the same reference. Verify one request, successful settlement, an empty residual-unknown set and clean admission. Retain the registered-but-unattributed refusal case.",
      "title": "Positive attribution does not clear an earlier unknown observation",
      "trigger": "One witness receives an unattributed result and a later positively matched result for the same unit before release."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "LateUnitProbe.Release constructs its own ReleaseRemaining and Settle action array. GhaeleLifetimeProbe.Release constructs a separate array. Omitting final settlement from the production array would leave the domain abandonment assertion green. Its source check only matches the ordinary SettleOwned call.",
      "blocking_rationale": "The mission requires behavioral evidence through the caller that determines isolation on abandonment. The native session covers ordinary completion of that caller.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The controlled abandonment case executes the production sequence that requests remaining units, reads settlement and returns cleanup to ProbeSlot.",
      "id": "R1-F7",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 990
      },
      "priority": "P2",
      "requested_correction": "Exercise the production finalizer from the slot regression, either directly with controlled native operations or through one final release sequence shared by the production probe and the fixture.",
      "requested_evidence": "Abandon before ordinary settlement. Show that the actual final row and ProbeSlot carry the same unresolved unit, the original timeout survives, independent cleanup runs and the next Begin is refused. A terminal read that becomes readable on the next call would also distinguish the repaired single-snapshot behavior.",
      "title": "The abandonment regression still substitutes the final cleanup sequence",
      "trigger": "The regression is used to qualify final settlement when the enclosing probe is abandoned."
    },
    {
      "acceptance_ids": [
        "R5-F1"
      ],
      "actual": "The R5-F1 row names only `002852Z` and harness `0a038e40` from round 1. The status section and session receipts identify `005206Z` and harness `bf5d74a1` for round 2. The ledger note at line 171 also says C2 ran one session although it lists both runs.",
      "blocking_rationale": "The correct artifact identity and both runs are recoverable from the same manifest and the receipts.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The finding map identifies the harness used for the reviewed source and preserves the earlier run as history.",
      "id": "R2-F3",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 109
      },
      "priority": "P3",
      "requested_correction": "Add the round-2 source, harness and session to the R5-F1 mapping and update the session count in the ledger note.",
      "requested_evidence": "A text check against the two committed session receipts is sufficient.",
      "title": "The handoff map points to the first C2 harness",
      "trigger": "A reader uses the parent-finding map to select the Ghaele harness identity for integration."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F1",
      "rationale": "Terminal returns unresolved for an undestroyed unit absent from State.Units. The controlled pending-registration case recorded one request, a named unresolved unit and refusal of the next probe."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F2",
      "rationale": "RecordUncertain now retains the unknown identity even when the fixture owns its teardown. The registered and unregistered controls exercise both outcomes. R2-F2 concerns a unit with subsequent positive attribution."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F3",
      "rationale": "Subscription release and reconciliation run as independent held actions. The focused case preserves the unsubscribe exception and settles the extra unit observed before that exception. R2-F1 concerns later delivery."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F4",
      "rationale": "Final settlement no longer invokes the global destroyer tick. It reads each requested unit and records a pending or unreadable result by identity. Removing the forced drain eliminates its exception path."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F5",
      "rationale": "LateResultReconciliation.Settle takes one terminal snapshot and passes it to both the evidence row and RecordUnsettled. The focused fixture verifies agreement for a consistently unresolved unit."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F6",
      "rationale": "The lifecycle check requires the flag sampled before fallback. Foreign survival is read after settlement, and the late and real-spawn native rows explicitly record the foreign control unrequested at that boundary."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F7",
      "rationale": "The regression now uses the production WitnessCaller and a participating foreign control. Its own final action array still determines the abandonment result, leaving the production finalizer outside the failure test."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F8",
      "rationale": "The pre-C2 statement is dated through C1 and all expenditure rows are in one Markdown table. The separate round-2 handoff discrepancy is recorded as R2-F3."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r02-a67b64",
  "round": 2,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The caller adopts observed late units, deduplicates destruction requests and reports unresolved terminal reads to ProbeSlot. The short native lifecycle run qualified the successful release path on candidate 3. Results arriving after an unsuccessful early unsubscribe remain unaccounted for.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Disposition of C2 at its two-review stop before the dependent C3 child can begin.",
      "Stack integration and the retained visual and listening decisions."
    ],
    "scope_reviewed": "Final C2 external-harness correction for parent R5-F1 on draft PR #222, including its affected callers, focused gates and lifecycle qualification.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
