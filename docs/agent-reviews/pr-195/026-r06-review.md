<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r06-review part=1/1 sha256=eb4a14145bcb07de8cf44767ae1333f624b054ea5f6f61b5ea49a7a697ce14aa -->
### Codex GPT-6.2 Astra — review — round 6 — candidate `0b3a9533cd`

**Decision:** `changes_required` — Both pending fixes are verified. The focused gate passed, and the native runs support the pricing and summoning repairs. Three paths can still pass startup with an incorrect spear profile. The item can inherit the donor’s damage dice, the movement-enchantment check reads only its backing field, and the category check misses changes to the list returned by GetSubCategories.

#### Findings

**R6-F1** · P2 · defect · blocks readiness — Owned spear items can inherit the donor’s damage-dice override
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:855`; acceptance: 3.A.4, 3.A.5, 4.1.1
- Trigger: A mod changes the Standard Longspear item’s damage dice before spear registration.
- Expected: Foundation and named spear items use the catalog’s 1d8 base damage. The native donor retains its own override.
- Actual: Registration clones the native item. Configure and ConfigureNamed set m_OverrideDamageType but leave m_OverrideDamageDice and m_DamageDice inherited. ValidateFoundation checks BaseDamage on the weapon type. An item-level 2d6 override therefore survives a passing 1d8 type check.
- Why it blocks / does not: A mutable donor can still change the damage of the equippable spear while startup reports the locked profile as valid.
- Requested correction: Clear the inherited damage-dice override on owned spear items that use the family profile, and validate the resulting BlueprintItemWeapon.BaseDamage.
- Requested evidence: In the isolated fixture, give the native Standard Longspear item a 2d6 override. Show its effective damage remains 2d6 after registration and a foundation spear and a named spear each report 1d8 through the item API.

**R6-F2** · P2 · defect · blocks readiness — The movement-enchantment check can pass when the live getter returns no enchantment
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:567`; acceptance: 3.A.2, 3.A.5, 4.1.1, 4.1.2
- Trigger: A getter patch removes the inherent enchantment from the registered spear’s returned collection.
- Expected: The registered spear’s effective enchantment collection contains the released movement-accuracy enchantment. A discrepancy identifies the returned value, donor context and getter patch owners.
- Actual: Observe reads m_Enchantments, converts entries to names, and supplies the same text as both the stored and effective result. It never reads BlueprintWeaponType.Enchantments. The separate component check examines the registered enchantment object without establishing that the getter returns it.
- Why it blocks / does not: The spear can lose the enchantment that supplies its movement-provoked attack bonus and proficiency handling while passing every profile check.
- Requested correction: Keep the serialized check and inspect the live Enchantments collection after GUID assignment. Compare the entry with the registered enchantment by identity. Use the existing bounded diagnostic path for a mismatch or throwing getter.
- Requested evidence: Use a spear-only getter control that returns an empty collection while the backing field remains correct. Verify one movement-enchantment diagnostic naming the patch owner and confirm selector restoration.

**R6-F3** · P2 · defect · blocks readiness — Classification validation misses the separate GetSubCategories result
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:604`; acceptance: 3.A.1, 3.A.2, 3.A.5, 4.1.1
- Trigger: A late postfix changes only the concrete category list returned for the spear.
- Expected: GetSubCategories returns the five concrete spear classifications. HasSubCategory agrees on those classifications and preserves its separate None wildcard.
- Actual: SubCategories checks every HasSubCategory result. ElvenBranchedSpearCategoryRuntime also patches WeaponCategoryExtension.GetSubCategories independently. A later postfix can add Simple to that returned array while the product’s HasSubCategory postfix continues to answer false for Simple.
- Why it blocks / does not: Callers that enumerate the spear’s classifications can receive a different profile from callers that query membership, with startup accepting both.
- Requested correction: Compare the registered category’s live GetSubCategories result with the intended concrete set. Report donor context and the owners of that method when its result disagrees.
- Requested evidence: Add Simple only to the list returned by GetSubCategories in the isolated harness. Show that membership still rejects Simple and that startup fails once with the list discrepancy and its patch owner.

**R6-F4** · P3 · optional_polish · non-blocking — The investigation opens with a superseded product revision
- Location: `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md:12`; acceptance: 4.2, 6
- Trigger: An owner uses the opening identity list to select the reviewed product.
- Expected: The opening identity list identifies the current product revision and labels earlier artifacts by round.
- Actual: The introduction names cc8da0e71f as the product revision and says later commits change only the harness and record. Section 4.2 correctly identifies the product change at 195d17ce19 and the package built at 18361607ed.
- Why it blocks / does not: Section 4.2 and the controller receipt already identify the tested artifacts. The misleading introduction can be corrected from existing records.
- Requested correction: Lead with the current product revision and native package. Label cc8da0e71f and 8ee7eabaed as the earlier product and package revisions.
- Requested evidence: Check the revised opening against Git history and the package hashes already recorded in section 4.2.

#### Prior finding assessments

- **R5-F1** — `fix_verified` — The membership check now covers every defined WeaponSubCategory and the None wildcard. Session 20261121T185458Z recorded one spear-only Simple addition, one failure naming Simple and both patch owners, an absent owned spear and a passing selector-restoration check. The healthy and Finessable-removal controls passed.
- **R5-F2** — `fix_verified` — The projection preserves array values and enumerates ordinary weapons individually. The transcript shows problems as [], a single active mod as a one-element array in the shape check, and four distinct pricing rows with one-element sellPrices and tooltipSellPrices arrays.

#### Evidence assessment

- **Inspected:** The fixed mission, repository instructions, validation policy, review workflow, feature diff, developer transcript, gate receipt and prior findings ledger.; Spear type and item construction, named and progression variants, selector publication and rollback, battered-origin pricing, Expanded Summoning publication and menu hooks, and the native harness assertions.
- **Executed verification:** The controller receipt for 0b3a9533cd records production compilation and 28 of 28 selected spear tests passing in 1.9 seconds, with exit code 0 and an unchanged worktree. I verified the supplied gate log’s SHA-256.; The developer transcript records 56 of 56 neighboring tests passing on the working product change. The development package built at 18361607ed passed strict UMM validation. Its ZIP SHA-256 was c96e1b3c94cf13707cb8f73368681e61150e23f0526542d050979bdc3707f20e and its product DLL SHA-256 was f0fb25cb6027a83ed4265b880122dbd0cc573ca1f04f4eb4f74552c3bc4c8330. I verified that the product, tests, harness and session definitions match the final candidate.; On the isolated Kingmaker 2.1.7b and UMM 0.31.1 copy, the three classification controls, the twelve-launch simulated session and the two-launch reporter-shaped session recorded PASS. Each session reported registry restoration, 95 unchanged owner saves and an unchanged owner installation.; After induced initialization failure, each of four ordinary weapons completed 25 price queries and 25 tooltip fills with zero throws. The sell prices were 1, 4, 5 and 13 gp, with matching tooltip values. The session passed its exactly-once warning assertion. The reporter-shaped session passed its visible menu-row, selected Fire Beetle ability, successful cast and spawned-unit checks.
- **Source inspection only:** The item adapter leaves the donor’s damage-dice override in place. The profile and persistence observers inspect the weapon type’s base damage.; The movement-enchantment predicate uses serialized names for both observations. The category predicate queries HasSubCategory, while the product also exposes the concrete list through a separate GetSubCategories patch.; The pricing exception is confined to inspection when the origin blueprint is unavailable. Binding and owner lookup retain their strict checks. Expanded Summoning’s menu hook checks for a published parent.
- **Required gaps:** Verify effective item damage after changing the native Standard Longspear item’s dice override.; Verify failure when the registered spear’s live Enchantments getter omits its required enchantment.; Verify failure when GetSubCategories disagrees with HasSubCategory for the registered spear.
- **Limitations:** The reporter’s raw log and exact 43-mod installation were unavailable. The four available reported mods and simulated donor changes establish behavior only in those configurations.; Fresh-process persistence, probe cleanup and selection restoration were exercised on the earlier 8ee7eabaed package. The later native sessions covered startup, diagnostics, pricing, summoning and classification. The earlier results remain attributable to their original package.; This review used the read-only snapshot and recorded execution evidence. The three new profile findings are source findings.

#### Summary

- **Delivered behavior:** The repaired weapon type kept its intended reach and fighter group under the simulated donor mutation. Deliberately failed startup preserved ordinary prices and tooltip data. With the reporter’s settings and four available reported mods, a new sorcerer opened the creature menu and successfully summoned the selected Fire Beetle.
- **Scope reviewed:** The complete spear startup, tooltip pricing and Expanded Summoning repair for draft PR #195, including registration, publication, saved-item handling and the evidence for both pending findings.
- **Pending later-phase work:** none
- **Unresolved questions:** The exact failing predicate and the cause in the reporter’s 43-mod installation remain unknown.
- **Remaining owner review:** Visual acceptance of the menu and tooltip presentation, followed by any merge or release decision after the blocking findings are resolved.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a1ff92-e0c8-7d70-b183-51791f047c9e`, duration 600.0s, response sha256 `a4892efe2a5d652c89ec78d93a6e4c4ee2abca327bc41f15c207d8914db7ba39`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r06-1376f6`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "0b3a9533cda103cf64923d81319e2c3ef14b5008",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "Both pending fixes are verified. The focused gate passed, and the native runs support the pricing and summoning repairs. Three paths can still pass startup with an incorrect spear profile. The item can inherit the donor\u2019s damage dice, the movement-enchantment check reads only its backing field, and the category check misses changes to the list returned by GetSubCategories.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt for 0b3a9533cd records production compilation and 28 of 28 selected spear tests passing in 1.9 seconds, with exit code 0 and an unchanged worktree. I verified the supplied gate log\u2019s SHA-256.",
      "The developer transcript records 56 of 56 neighboring tests passing on the working product change. The development package built at 18361607ed passed strict UMM validation. Its ZIP SHA-256 was c96e1b3c94cf13707cb8f73368681e61150e23f0526542d050979bdc3707f20e and its product DLL SHA-256 was f0fb25cb6027a83ed4265b880122dbd0cc573ca1f04f4eb4f74552c3bc4c8330. I verified that the product, tests, harness and session definitions match the final candidate.",
      "On the isolated Kingmaker 2.1.7b and UMM 0.31.1 copy, the three classification controls, the twelve-launch simulated session and the two-launch reporter-shaped session recorded PASS. Each session reported registry restoration, 95 unchanged owner saves and an unchanged owner installation.",
      "After induced initialization failure, each of four ordinary weapons completed 25 price queries and 25 tooltip fills with zero throws. The sell prices were 1, 4, 5 and 13 gp, with matching tooltip values. The session passed its exactly-once warning assertion. The reporter-shaped session passed its visible menu-row, selected Fire Beetle ability, successful cast and spawned-unit checks."
    ],
    "inspected": [
      "The fixed mission, repository instructions, validation policy, review workflow, feature diff, developer transcript, gate receipt and prior findings ledger.",
      "Spear type and item construction, named and progression variants, selector publication and rollback, battered-origin pricing, Expanded Summoning publication and menu hooks, and the native harness assertions."
    ],
    "limitations": [
      "The reporter\u2019s raw log and exact 43-mod installation were unavailable. The four available reported mods and simulated donor changes establish behavior only in those configurations.",
      "Fresh-process persistence, probe cleanup and selection restoration were exercised on the earlier 8ee7eabaed package. The later native sessions covered startup, diagnostics, pricing, summoning and classification. The earlier results remain attributable to their original package.",
      "This review used the read-only snapshot and recorded execution evidence. The three new profile findings are source findings."
    ],
    "required_gaps": [
      "Verify effective item damage after changing the native Standard Longspear item\u2019s dice override.",
      "Verify failure when the registered spear\u2019s live Enchantments getter omits its required enchantment.",
      "Verify failure when GetSubCategories disagrees with HasSubCategory for the registered spear."
    ],
    "source_inspection_only": [
      "The item adapter leaves the donor\u2019s damage-dice override in place. The profile and persistence observers inspect the weapon type\u2019s base damage.",
      "The movement-enchantment predicate uses serialized names for both observations. The category predicate queries HasSubCategory, while the product also exposes the concrete list through a separate GetSubCategories patch.",
      "The pricing exception is confined to inspection when the origin blueprint is unavailable. Binding and owner lookup retain their strict checks. Expanded Summoning\u2019s menu hook checks for a published parent."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "3.A.4",
        "3.A.5",
        "4.1.1"
      ],
      "actual": "Registration clones the native item. Configure and ConfigureNamed set m_OverrideDamageType but leave m_OverrideDamageDice and m_DamageDice inherited. ValidateFoundation checks BaseDamage on the weapon type. An item-level 2d6 override therefore survives a passing 1d8 type check.",
      "blocking_rationale": "A mutable donor can still change the damage of the equippable spear while startup reports the locked profile as valid.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Foundation and named spear items use the catalog\u2019s 1d8 base damage. The native donor retains its own override.",
      "id": "R6-F1",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 855
      },
      "priority": "P2",
      "requested_correction": "Clear the inherited damage-dice override on owned spear items that use the family profile, and validate the resulting BlueprintItemWeapon.BaseDamage.",
      "requested_evidence": "In the isolated fixture, give the native Standard Longspear item a 2d6 override. Show its effective damage remains 2d6 after registration and a foundation spear and a named spear each report 1d8 through the item API.",
      "title": "Owned spear items can inherit the donor\u2019s damage-dice override",
      "trigger": "A mod changes the Standard Longspear item\u2019s damage dice before spear registration."
    },
    {
      "acceptance_ids": [
        "3.A.2",
        "3.A.5",
        "4.1.1",
        "4.1.2"
      ],
      "actual": "Observe reads m_Enchantments, converts entries to names, and supplies the same text as both the stored and effective result. It never reads BlueprintWeaponType.Enchantments. The separate component check examines the registered enchantment object without establishing that the getter returns it.",
      "blocking_rationale": "The spear can lose the enchantment that supplies its movement-provoked attack bonus and proficiency handling while passing every profile check.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The registered spear\u2019s effective enchantment collection contains the released movement-accuracy enchantment. A discrepancy identifies the returned value, donor context and getter patch owners.",
      "id": "R6-F2",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 567
      },
      "priority": "P2",
      "requested_correction": "Keep the serialized check and inspect the live Enchantments collection after GUID assignment. Compare the entry with the registered enchantment by identity. Use the existing bounded diagnostic path for a mismatch or throwing getter.",
      "requested_evidence": "Use a spear-only getter control that returns an empty collection while the backing field remains correct. Verify one movement-enchantment diagnostic naming the patch owner and confirm selector restoration.",
      "title": "The movement-enchantment check can pass when the live getter returns no enchantment",
      "trigger": "A getter patch removes the inherent enchantment from the registered spear\u2019s returned collection."
    },
    {
      "acceptance_ids": [
        "3.A.1",
        "3.A.2",
        "3.A.5",
        "4.1.1"
      ],
      "actual": "SubCategories checks every HasSubCategory result. ElvenBranchedSpearCategoryRuntime also patches WeaponCategoryExtension.GetSubCategories independently. A later postfix can add Simple to that returned array while the product\u2019s HasSubCategory postfix continues to answer false for Simple.",
      "blocking_rationale": "Callers that enumerate the spear\u2019s classifications can receive a different profile from callers that query membership, with startup accepting both.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "GetSubCategories returns the five concrete spear classifications. HasSubCategory agrees on those classifications and preserves its separate None wildcard.",
      "id": "R6-F3",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 604
      },
      "priority": "P2",
      "requested_correction": "Compare the registered category\u2019s live GetSubCategories result with the intended concrete set. Report donor context and the owners of that method when its result disagrees.",
      "requested_evidence": "Add Simple only to the list returned by GetSubCategories in the isolated harness. Show that membership still rejects Simple and that startup fails once with the list discrepancy and its patch owner.",
      "title": "Classification validation misses the separate GetSubCategories result",
      "trigger": "A late postfix changes only the concrete category list returned for the spear."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "The introduction names cc8da0e71f as the product revision and says later commits change only the harness and record. Section 4.2 correctly identifies the product change at 195d17ce19 and the package built at 18361607ed.",
      "blocking_rationale": "Section 4.2 and the controller receipt already identify the tested artifacts. The misleading introduction can be corrected from existing records.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The opening identity list identifies the current product revision and labels earlier artifacts by round.",
      "id": "R6-F4",
      "location": {
        "file": "docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md",
        "line": 12
      },
      "priority": "P3",
      "requested_correction": "Lead with the current product revision and native package. Label cc8da0e71f and 8ee7eabaed as the earlier product and package revisions.",
      "requested_evidence": "Check the revised opening against Git history and the package hashes already recorded in section 4.2.",
      "title": "The investigation opens with a superseded product revision",
      "trigger": "An owner uses the opening identity list to select the reviewed product."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F1",
      "rationale": "The membership check now covers every defined WeaponSubCategory and the None wildcard. Session 20261121T185458Z recorded one spear-only Simple addition, one failure naming Simple and both patch owners, an absent owned spear and a passing selector-restoration check. The healthy and Finessable-removal controls passed."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F2",
      "rationale": "The projection preserves array values and enumerates ordinary weapons individually. The transcript shows problems as [], a single active mod as a one-element array in the shape check, and four distinct pricing rows with one-element sellPrices and tooltipSellPrices arrays."
    }
  ],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r06-1376f6",
  "round": 6,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The repaired weapon type kept its intended reach and fighter group under the simulated donor mutation. Deliberately failed startup preserved ordinary prices and tooltip data. With the reporter\u2019s settings and four available reported mods, a new sorcerer opened the creature menu and successfully summoned the selected Fire Beetle.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Visual acceptance of the menu and tooltip presentation, followed by any merge or release decision after the blocking findings are resolved."
    ],
    "scope_reviewed": "The complete spear startup, tooltip pricing and Expanded Summoning repair for draft PR #195, including registration, publication, saved-item handling and the evidence for both pending findings.",
    "unresolved_questions": [
      "The exact failing predicate and the cause in the reporter\u2019s 43-mod installation remain unknown."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
