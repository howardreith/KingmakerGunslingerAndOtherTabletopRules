<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r01-review part=1/1 sha256=52dc6f0a5c05a6cc5b0c9aa05026003ea7074c6481ae012c1597b22528d6671e -->
### Codex GPT-6.2 Astra — review — round 1 — candidate `287bd73ebb`

**Decision:** `changes_required` — The candidate repairs the simulated donor-mutation failure and protects ordinary weapon pricing after a failed startup. The focused gate passed on the exact candidate. Readiness is blocked because validation accepts incorrect effective spear mechanics whenever a foreign Harmony owner is present, and a later validation failure can leave native selectors pointing at rolled-back blueprints. The save-compatibility and selected-summon evidence also falls short of the mission.

#### Findings

**R1-F1** · P1 · defect · blocks readiness — A foreign patch owner can waive any effective spear invariant
- Location: `src/KingmakerGunslinger/ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs:202`; acceptance: 3.A.4, 3.A.5, 4.1.1
- Trigger: An external postfix returns an incorrect range, damage, handedness or fighter group for the owned spear while its serialized field is correct.
- Expected: Enforce the fixed effective profile. Any compatibility exception needs a specific supported rule and evidence that the observed result satisfies it.
- Actual: Classify downgrades every getter mismatch to a warning if any foreign owner appears in patch metadata. The S-3 session explicitly requires startup to succeed with 7 ft effective reach against an expected 6 ft. The same rule accepts a spear-only FighterGroup override to Polearms. Patch metadata establishes neither causation nor the warning's claim that the rule applies to every weapon.
- Why it blocks / does not: The mission requires a genuinely wrong spear profile to fail validation. Players and engine consumers use the effective getter values.
- Requested correction: Remove foreign patch presence as a general exemption. Keep the owner names as diagnostic context and update the warning, tests and session expectations to match the supported behavior.
- Requested evidence: Use a native getter patch limited to the owned spear that returns an invalid value while its serialized field and donor remain correct. Verify a bounded failure identifying the predicate and owner. Retain the successful mutated-donor construction case.

**R1-F2** · P1 · defect · blocks readiness — Late spear validation can escape selector rollback
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:220`; acceptance: 3.C.3, 3.C.4, 3.C.5, 4.1.7
- Trigger: The first clone validation passes, then validation of the registered type or the final donor fingerprint fails after selector publication.
- Expected: Restore all native selector and familiarity changes when registration fails, including failures after the owned type has received its final GUID.
- Actual: Register publishes proficiency, finesse and familiarity changes before its final profile and donor checks. If one throws, Register never returns the publication handle to BlueprintBootstrap. The registry can remove the owned blueprints while the native selections still reference them. The injected corruption currently fails at the earlier Configure validation, before publication.
- Why it blocks / does not: A legitimate late validation failure can leave dangling options in native character-building data after the reported rollback.
- Requested correction: Complete fallible profile checks before publication or retain local rollback ownership until the bootstrap transaction has acquired it. Ensure the handoff also covers failure during named-spear registration.
- Requested evidence: Induce a failure after the owned type has been registered and compare the native proficiency and finesse arrays, familiarity components and selector runtime state to their pre-registration values.

**R1-F3** · P2 · defect · blocks readiness — A throwing getter aborts construction of the diagnostic report
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:573`; acceptance: 3.A.2, 3.A.4, 4.1.2
- Trigger: A modded getter throws for the donor or the owned type while Observe is collecting values.
- Expected: A failing owned getter produces a bounded fatal observation with the predicate, expected value, exception and available owners. An unavailable donor comparison can be recorded as such. One failed initialization reports all observed discrepancies.
- Actual: Getter invokes getterText(type) before looking up patch owners and invokes getterText(donor) even for a passing predicate. An exception from either call aborts Observe and loses the predicate report. A donor-only exception can therefore abort startup for a correctly configured clone. Also, DescribeFailure prints only Fatal, so a simultaneous external getter discrepancy disappears from the failed-initialization record.
- Why it blocks / does not: The diagnostic path introduces a new dependency on live donor getters and can conceal precisely the external interaction the mission requires it to identify.
- Requested correction: Capture exceptions narrowly around individual diagnostic getter reads, distinguish donor observations from owned invariants, and emit the complete set of failed observations in the one failure record.
- Requested evidence: Exercise a donor-only throwing getter and an owned throwing getter alongside a second invalid predicate. Check startup disposition, predicate names, available patch owners and the count of detailed failure messages.

**R1-F4** · P2 · evidence_gap · blocks readiness — Disabled-publication pricing was tested only on a newly bound pistol
- Location: `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md:342`; acceptance: 3.B.2, 3.C.5, 4.1.5, 4.1.7
- Trigger: An existing save containing affected items is opened with the repaired product and Gunslinger publication disabled.
- Expected: A fresh process resolves persisted items through the released identities and retains the battered marker, original owner and fixed price with Gunslinger publication disabled.
- Actual: The reporter-stack session restarts to apply settings, then constructs and binds a new pistol in memory. It never reloads that item or resolves a previously saved spear. The investigation waives the check because registration order and GUIDs are unchanged, although the repair changes construction during registration and the price-inspection path used by loaded items.
- Why it blocks / does not: New-item pricing proves the disabled-publication case for newly bound objects. It leaves the mission's existing-item compatibility requirement unverified.
- Requested correction: Use an approved isolated fixture containing a released spear and a bound battered firearm for one focused fresh-process load. Replace the waiver with the observed result.
- Requested evidence: Record the persisted item and blueprint identities, resolved spear profile, owner/token match, fixed battered price, ordinary control price, module state and restoration result.

**R1-F5** · P2 · evidence_gap · blocks readiness — The summon probe bypasses creature selection and accepts any summon
- Location: `tools/LoaderCompatHarness/SpearRepairProbes.cs:710`; acceptance: 3.C.1, 3.C.2, 4.1.6
- Trigger: A visible creature slot has a broken selection action or produces a different creature from the one displayed.
- Expected: Selecting the visible choice through the native menu starts the intended cast and produces the matching creature. The failed-startup control requires the native unexpanded route.
- Actual: The probe opens the parent menu but reads a child's AbilityData, hides the menu, and constructs UnitUseAbility directly. It passes when the command finishes and any unit has the caster as summoner. It records the command result and creature names without checking success or matching the creature to the selected row. In the failed-startup case, a parent with variants falls through to the expanded-menu branch and can also pass.
- Why it blocks / does not: The reported PASS can survive a broken child-click handler, a choice that summons the wrong creature, or an unexpectedly expanded failed-startup spell.
- Requested correction: Drive the visible child slot's selection and targeting path, require a successful cast and match the observed unit to the selected entry. Give the native-single control an explicit expectation that rejects expanded menus.
- Requested evidence: For the reporter-shaped candidate session, record the selected row, ability submitted by the UI, command result and matching summoned unit. For the failure control, record zero variants, a closed menu and the native summon.

**R1-F6** · P2 · defect · blocks readiness — The sorcerer probe can report success or isolation after failed cleanup
- Location: `tools/LoaderCompatHarness/HarnessRunner.cs:237`; acceptance: 4.1.6, 4.2
- Trigger: The summon step times out while waiting for UI or casting, or a cleanup action throws.
- Expected: Every terminal path releases the probe's owned state exactly once. A failed release marks isolation unrestored and stops the launch.
- Actual: The sorcerer probe is held outside the harness's ProbeSlot. EndStepProbes clears its reference on timeout without invoking Cleanup. On normal completion Cleanup records exceptions in cleanupFailures, but Step returns its earlier successful verdict. The harness can therefore report restored isolation or continue after leaving a caster, party entry, summon or changed pause state behind.
- Why it blocks / does not: These probes mutate the disposable game state used for verification. A timeout or failed cleanup must affect the run verdict before another step can proceed.
- Requested correction: Use the existing owned-probe lifecycle for SorcererSummonProbe, or provide equivalent release and verdict propagation on success, exception, step timeout and harness termination.
- Requested evidence: Show a forced timeout after party insertion invokes cleanup, and that an injected release failure prevents PASS or continuation. Confirm normal completion still restores the fixture state.

**R1-F7** · P2 · evidence_gap · blocks readiness — The baseline verdict does not enforce the reported repeated tooltip failure
- Location: `tools/LoaderCompatHarness/SpearRepairProbes.cs:468`; acceptance: 3.A.3, 4.1.3, 6
- Trigger: Only some baseline queries throw, or tooltip construction fails for a reason unrelated to the battered-origin exception.
- Expected: The baseline verdict and handoff distinguish repeated matching missing-token failures from intermittent or unrelated exceptions.
- Actual: In throws mode, Judge requires only one price exception and one tooltip exception per item. It checks the missing-token text only in firstPriceError. One matching price exception and one unrelated tooltip exception across five attempts pass the same verdict as the documented 5/5 reproduction.
- Why it blocks / does not: The supplied CLI summary establishes a passing session under that weaker assertion. It does not establish the committed claim about every baseline query and its cause.
- Requested correction: Compare both failure counts with the requested repeat count and verify the exception cause on both paths. Align the investigation record with what the observations establish.
- Requested evidence: Supply per-item price and tooltip failure counts and matching exception observations. Reuse immutable rows where they contain the needed information, and narrow the claim or rerun only the affected baseline where they do not.

#### Evidence assessment

- **Inspected:** Controller mission, prior-findings ledger, repository instructions, feature diff, candidate history, investigation record, developer transcript, focused-gate receipt and log.; Spear construction and profile policy, registry GUID assignment, selector publication and rollback, bootstrap failure logging, battered-origin inspection, and Expanded Summoning publication and menu hooks.; The three native session definitions, session runner, new probes, existing probe lifecycle, and focused domain cases.
- **Executed verification:** The controller receipt and matching log hash confirm production compilation and 25 of 3158 tests passing with the elven-branched-spear filter at 287bd73ebbe28723b135bdba0ed4ddbc407c5b5f. The receipt records an unchanged worktree and 1.9 seconds elapsed.; The developer transcript records 98 pricing-neighbor tests and 43 summoning-neighbor tests passing before the final commit. It also records the guarded push of the final candidate.; The build transcript identifies development package SHA-256 b7b4d619d5e020bf259d4576f52820ebea890a66727992a2a21c99ab25b7083e and DLL SHA-256 031dc659498f0c6a591f45abc22bf13ed1837f5babfcb8b34148b9af8c9e130a at 56e00163e9d1. Git history confirms the production source is identical at that commit and the reviewed candidate.; The transcript records PASS for the v0.0.192 simulated baseline, the three candidate simulations, and both four-mod reporter-stack sessions under UMM 0.31.1 with harness commit 4ba6cab678. The candidate failure scenario required 25 successful price and tooltip fills per ordinary item and exactly one unavailable-state warning. The session runner included registry restoration and owner save/install integrity in its final verdict.
- **Source inspection only:** The sale-price fallback is confined to inspection when the token is absent and bootstrap is pending or failed. Successful initialization still uses the original owner lookup. Creation, transfer, manifest identities and summon roster code are unchanged.; The shared getter classification, validation order relative to selector publication, and native probe false-pass paths were traced in source.
- **Required gaps:** Show that an invalid effective getter result fails under a foreign patch, that late registration failure restores native selectors, and that failed observations produce complete bounded diagnostics.; Verify persisted spear and battered-item identities in a fresh process with Gunslinger publication disabled.; Verify selection through the creature menu and the identity of the resulting summon. Make the failed-startup menu and baseline exception verdicts enforce their documented expectations.; Verify that timeout and cleanup failures from the sorcerer probe reach the harness isolation verdict.
- **Limitations:** The reporter's raw log and full 43-mod environment were unavailable. The four available reported mods did not reproduce the failure on v0.0.192. The simulated mutation establishes a failure mechanism, while the reporter's exact predicate and cause remain unknown.; The review snapshot contains the gate evidence and CLI session summaries. Detailed native rows, screenshots and per-launch logs remained machine-local. Exact observations beyond the passing assertions are developer-reported.; The reviewer performed read-only inspection. The native sessions ran against the identified development package. The final commit changed the investigation record.

#### Summary

- **Delivered behavior:** The owned spear receives explicit mechanical fields. Native sessions support successful construction under the simulated donor mutation, useful fighter-group diagnostics for an invalid owned field, and native ordinary prices and tooltip fills after failure. The reporter-shaped session also supports visible creature choices and fixed pricing for a newly bound battered pistol with Gunslinger off.
- **Scope reviewed:** Complete final mission for draft PR #195, from merge base 2e61e78097 through candidate 287bd73ebb, including product behavior, guarded native evidence, test verdicts and the committed handoff.
- **Pending later-phase work:** none
- **Unresolved questions:** Which predicate failed in the reporter's installation and which interaction caused it remain unknown.; The simulated baseline supports an unexpanded native spell. A menu that opens empty was never independently observed.
- **Remaining owner review:** Any deliberate compatibility exception that changes the spear's fixed effective mechanics needs an owner design decision.; Merge and release remain owner decisions after the required corrections and controller handoff.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `high` | `high` (turns ['high']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a1fed1-4352-74a0-8d6b-6ad57413e28d`, duration 279.1s, response sha256 `b65a89be54a01a6c93f795a2651a3e62e3901707f65eee171ad838065f73c19d`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r01-ed1561`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "287bd73ebbe28723b135bdba0ed4ddbc407c5b5f",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The candidate repairs the simulated donor-mutation failure and protects ordinary weapon pricing after a failed startup. The focused gate passed on the exact candidate. Readiness is blocked because validation accepts incorrect effective spear mechanics whenever a foreign Harmony owner is present, and a later validation failure can leave native selectors pointing at rolled-back blueprints. The save-compatibility and selected-summon evidence also falls short of the mission.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt and matching log hash confirm production compilation and 25 of 3158 tests passing with the elven-branched-spear filter at 287bd73ebbe28723b135bdba0ed4ddbc407c5b5f. The receipt records an unchanged worktree and 1.9 seconds elapsed.",
      "The developer transcript records 98 pricing-neighbor tests and 43 summoning-neighbor tests passing before the final commit. It also records the guarded push of the final candidate.",
      "The build transcript identifies development package SHA-256 b7b4d619d5e020bf259d4576f52820ebea890a66727992a2a21c99ab25b7083e and DLL SHA-256 031dc659498f0c6a591f45abc22bf13ed1837f5babfcb8b34148b9af8c9e130a at 56e00163e9d1. Git history confirms the production source is identical at that commit and the reviewed candidate.",
      "The transcript records PASS for the v0.0.192 simulated baseline, the three candidate simulations, and both four-mod reporter-stack sessions under UMM 0.31.1 with harness commit 4ba6cab678. The candidate failure scenario required 25 successful price and tooltip fills per ordinary item and exactly one unavailable-state warning. The session runner included registry restoration and owner save/install integrity in its final verdict."
    ],
    "inspected": [
      "Controller mission, prior-findings ledger, repository instructions, feature diff, candidate history, investigation record, developer transcript, focused-gate receipt and log.",
      "Spear construction and profile policy, registry GUID assignment, selector publication and rollback, bootstrap failure logging, battered-origin inspection, and Expanded Summoning publication and menu hooks.",
      "The three native session definitions, session runner, new probes, existing probe lifecycle, and focused domain cases."
    ],
    "limitations": [
      "The reporter's raw log and full 43-mod environment were unavailable. The four available reported mods did not reproduce the failure on v0.0.192. The simulated mutation establishes a failure mechanism, while the reporter's exact predicate and cause remain unknown.",
      "The review snapshot contains the gate evidence and CLI session summaries. Detailed native rows, screenshots and per-launch logs remained machine-local. Exact observations beyond the passing assertions are developer-reported.",
      "The reviewer performed read-only inspection. The native sessions ran against the identified development package. The final commit changed the investigation record."
    ],
    "required_gaps": [
      "Show that an invalid effective getter result fails under a foreign patch, that late registration failure restores native selectors, and that failed observations produce complete bounded diagnostics.",
      "Verify persisted spear and battered-item identities in a fresh process with Gunslinger publication disabled.",
      "Verify selection through the creature menu and the identity of the resulting summon. Make the failed-startup menu and baseline exception verdicts enforce their documented expectations.",
      "Verify that timeout and cleanup failures from the sorcerer probe reach the harness isolation verdict."
    ],
    "source_inspection_only": [
      "The sale-price fallback is confined to inspection when the token is absent and bootstrap is pending or failed. Successful initialization still uses the original owner lookup. Creation, transfer, manifest identities and summon roster code are unchanged.",
      "The shared getter classification, validation order relative to selector publication, and native probe false-pass paths were traced in source."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "3.A.4",
        "3.A.5",
        "4.1.1"
      ],
      "actual": "Classify downgrades every getter mismatch to a warning if any foreign owner appears in patch metadata. The S-3 session explicitly requires startup to succeed with 7 ft effective reach against an expected 6 ft. The same rule accepts a spear-only FighterGroup override to Polearms. Patch metadata establishes neither causation nor the warning's claim that the rule applies to every weapon.",
      "blocking_rationale": "The mission requires a genuinely wrong spear profile to fail validation. Players and engine consumers use the effective getter values.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Enforce the fixed effective profile. Any compatibility exception needs a specific supported rule and evidence that the observed result satisfies it.",
      "id": "R1-F1",
      "location": {
        "file": "src/KingmakerGunslinger/ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs",
        "line": 202
      },
      "priority": "P1",
      "requested_correction": "Remove foreign patch presence as a general exemption. Keep the owner names as diagnostic context and update the warning, tests and session expectations to match the supported behavior.",
      "requested_evidence": "Use a native getter patch limited to the owned spear that returns an invalid value while its serialized field and donor remain correct. Verify a bounded failure identifying the predicate and owner. Retain the successful mutated-donor construction case.",
      "title": "A foreign patch owner can waive any effective spear invariant",
      "trigger": "An external postfix returns an incorrect range, damage, handedness or fighter group for the owned spear while its serialized field is correct."
    },
    {
      "acceptance_ids": [
        "3.C.3",
        "3.C.4",
        "3.C.5",
        "4.1.7"
      ],
      "actual": "Register publishes proficiency, finesse and familiarity changes before its final profile and donor checks. If one throws, Register never returns the publication handle to BlueprintBootstrap. The registry can remove the owned blueprints while the native selections still reference them. The injected corruption currently fails at the earlier Configure validation, before publication.",
      "blocking_rationale": "A legitimate late validation failure can leave dangling options in native character-building data after the reported rollback.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Restore all native selector and familiarity changes when registration fails, including failures after the owned type has received its final GUID.",
      "id": "R1-F2",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 220
      },
      "priority": "P1",
      "requested_correction": "Complete fallible profile checks before publication or retain local rollback ownership until the bootstrap transaction has acquired it. Ensure the handoff also covers failure during named-spear registration.",
      "requested_evidence": "Induce a failure after the owned type has been registered and compare the native proficiency and finesse arrays, familiarity components and selector runtime state to their pre-registration values.",
      "title": "Late spear validation can escape selector rollback",
      "trigger": "The first clone validation passes, then validation of the registered type or the final donor fingerprint fails after selector publication."
    },
    {
      "acceptance_ids": [
        "3.A.2",
        "3.A.4",
        "4.1.2"
      ],
      "actual": "Getter invokes getterText(type) before looking up patch owners and invokes getterText(donor) even for a passing predicate. An exception from either call aborts Observe and loses the predicate report. A donor-only exception can therefore abort startup for a correctly configured clone. Also, DescribeFailure prints only Fatal, so a simultaneous external getter discrepancy disappears from the failed-initialization record.",
      "blocking_rationale": "The diagnostic path introduces a new dependency on live donor getters and can conceal precisely the external interaction the mission requires it to identify.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A failing owned getter produces a bounded fatal observation with the predicate, expected value, exception and available owners. An unavailable donor comparison can be recorded as such. One failed initialization reports all observed discrepancies.",
      "id": "R1-F3",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 573
      },
      "priority": "P2",
      "requested_correction": "Capture exceptions narrowly around individual diagnostic getter reads, distinguish donor observations from owned invariants, and emit the complete set of failed observations in the one failure record.",
      "requested_evidence": "Exercise a donor-only throwing getter and an owned throwing getter alongside a second invalid predicate. Check startup disposition, predicate names, available patch owners and the count of detailed failure messages.",
      "title": "A throwing getter aborts construction of the diagnostic report",
      "trigger": "A modded getter throws for the donor or the owned type while Observe is collecting values."
    },
    {
      "acceptance_ids": [
        "3.B.2",
        "3.C.5",
        "4.1.5",
        "4.1.7"
      ],
      "actual": "The reporter-stack session restarts to apply settings, then constructs and binds a new pistol in memory. It never reloads that item or resolves a previously saved spear. The investigation waives the check because registration order and GUIDs are unchanged, although the repair changes construction during registration and the price-inspection path used by loaded items.",
      "blocking_rationale": "New-item pricing proves the disabled-publication case for newly bound objects. It leaves the mission's existing-item compatibility requirement unverified.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "A fresh process resolves persisted items through the released identities and retains the battered marker, original owner and fixed price with Gunslinger publication disabled.",
      "id": "R1-F4",
      "location": {
        "file": "docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md",
        "line": 342
      },
      "priority": "P2",
      "requested_correction": "Use an approved isolated fixture containing a released spear and a bound battered firearm for one focused fresh-process load. Replace the waiver with the observed result.",
      "requested_evidence": "Record the persisted item and blueprint identities, resolved spear profile, owner/token match, fixed battered price, ordinary control price, module state and restoration result.",
      "title": "Disabled-publication pricing was tested only on a newly bound pistol",
      "trigger": "An existing save containing affected items is opened with the repaired product and Gunslinger publication disabled."
    },
    {
      "acceptance_ids": [
        "3.C.1",
        "3.C.2",
        "4.1.6"
      ],
      "actual": "The probe opens the parent menu but reads a child's AbilityData, hides the menu, and constructs UnitUseAbility directly. It passes when the command finishes and any unit has the caster as summoner. It records the command result and creature names without checking success or matching the creature to the selected row. In the failed-startup case, a parent with variants falls through to the expanded-menu branch and can also pass.",
      "blocking_rationale": "The reported PASS can survive a broken child-click handler, a choice that summons the wrong creature, or an unexpectedly expanded failed-startup spell.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Selecting the visible choice through the native menu starts the intended cast and produces the matching creature. The failed-startup control requires the native unexpanded route.",
      "id": "R1-F5",
      "location": {
        "file": "tools/LoaderCompatHarness/SpearRepairProbes.cs",
        "line": 710
      },
      "priority": "P2",
      "requested_correction": "Drive the visible child slot's selection and targeting path, require a successful cast and match the observed unit to the selected entry. Give the native-single control an explicit expectation that rejects expanded menus.",
      "requested_evidence": "For the reporter-shaped candidate session, record the selected row, ability submitted by the UI, command result and matching summoned unit. For the failure control, record zero variants, a closed menu and the native summon.",
      "title": "The summon probe bypasses creature selection and accepts any summon",
      "trigger": "A visible creature slot has a broken selection action or produces a different creature from the one displayed."
    },
    {
      "acceptance_ids": [
        "4.1.6",
        "4.2"
      ],
      "actual": "The sorcerer probe is held outside the harness's ProbeSlot. EndStepProbes clears its reference on timeout without invoking Cleanup. On normal completion Cleanup records exceptions in cleanupFailures, but Step returns its earlier successful verdict. The harness can therefore report restored isolation or continue after leaving a caster, party entry, summon or changed pause state behind.",
      "blocking_rationale": "These probes mutate the disposable game state used for verification. A timeout or failed cleanup must affect the run verdict before another step can proceed.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every terminal path releases the probe's owned state exactly once. A failed release marks isolation unrestored and stops the launch.",
      "id": "R1-F6",
      "location": {
        "file": "tools/LoaderCompatHarness/HarnessRunner.cs",
        "line": 237
      },
      "priority": "P2",
      "requested_correction": "Use the existing owned-probe lifecycle for SorcererSummonProbe, or provide equivalent release and verdict propagation on success, exception, step timeout and harness termination.",
      "requested_evidence": "Show a forced timeout after party insertion invokes cleanup, and that an injected release failure prevents PASS or continuation. Confirm normal completion still restores the fixture state.",
      "title": "The sorcerer probe can report success or isolation after failed cleanup",
      "trigger": "The summon step times out while waiting for UI or casting, or a cleanup action throws."
    },
    {
      "acceptance_ids": [
        "3.A.3",
        "4.1.3",
        "6"
      ],
      "actual": "In throws mode, Judge requires only one price exception and one tooltip exception per item. It checks the missing-token text only in firstPriceError. One matching price exception and one unrelated tooltip exception across five attempts pass the same verdict as the documented 5/5 reproduction.",
      "blocking_rationale": "The supplied CLI summary establishes a passing session under that weaker assertion. It does not establish the committed claim about every baseline query and its cause.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The baseline verdict and handoff distinguish repeated matching missing-token failures from intermittent or unrelated exceptions.",
      "id": "R1-F7",
      "location": {
        "file": "tools/LoaderCompatHarness/SpearRepairProbes.cs",
        "line": 468
      },
      "priority": "P2",
      "requested_correction": "Compare both failure counts with the requested repeat count and verify the exception cause on both paths. Align the investigation record with what the observations establish.",
      "requested_evidence": "Supply per-item price and tooltip failure counts and matching exception observations. Reuse immutable rows where they contain the needed information, and narrow the claim or rerun only the affected baseline where they do not.",
      "title": "The baseline verdict does not enforce the reported repeated tooltip failure",
      "trigger": "Only some baseline queries throw, or tooltip construction fails for a reason unrelated to the battered-origin exception."
    }
  ],
  "prior_finding_assessments": [],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r01-ed1561",
  "round": 1,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The owned spear receives explicit mechanical fields. Native sessions support successful construction under the simulated donor mutation, useful fighter-group diagnostics for an invalid owned field, and native ordinary prices and tooltip fills after failure. The reporter-shaped session also supports visible creature choices and fixed pricing for a newly bound battered pistol with Gunslinger off.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Any deliberate compatibility exception that changes the spear's fixed effective mechanics needs an owner design decision.",
      "Merge and release remain owner decisions after the required corrections and controller handoff."
    ],
    "scope_reviewed": "Complete final mission for draft PR #195, from merge base 2e61e78097 through candidate 287bd73ebb, including product behavior, guarded native evidence, test verdicts and the committed handoff.",
    "unresolved_questions": [
      "Which predicate failed in the reporter's installation and which interaction caused it remain unknown.",
      "The simulated baseline supports an unexpanded native spell. A menu that opens empty was never independently observed."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
