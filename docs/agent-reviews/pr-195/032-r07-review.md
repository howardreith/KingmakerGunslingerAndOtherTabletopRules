<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r07-review part=1/1 sha256=bfe7c864153daa948be2b62fe820d5ec4f2bf20a56d11aee6989fecae0011084 -->
### Codex GPT-6.2 Astra — review — round 7 — candidate `7f44ae6e74`

**Decision:** `changes_required` — All four round-six corrections are verified. Three requirements remain open. The multi-launch runner checks for competing sessions only once, the new item-damage failure omits required diagnostic context, and the saved-spear check predates the change to registered item damage.

#### Findings

**R7-F1** · P1 · required_gap · blocks readiness — Later native launches can proceed after another session takes the host
- Location: `scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1:232`; acceptance: 4.2
- Trigger: Another Kingmaker process starts or a sibling lab acquires a runtime lock after the spear session's initial preflight.
- Expected: Each fresh launch verifies that the host is available. Registry restoration proceeds under transaction ownership once the host is safe.
- Actual: Assert-LcPreflight runs once before staging. The loop calls Start-LcKingmakerCopy for each launch without rechecking the process or foreign locks. The finalizer waits up to 60 seconds and then calls Restore-LcRegistry even if a Kingmaker process is still running. The session owns loader-compat.lock while the generic runtime launcher uses compatibility.lock.
- Why it blocks / does not: On the shared DATA host, a later launch can overlap another session and cleanup can overwrite that session's game settings. The successful restoration fields describe the completed runs and do not exercise this contention path.
- Requested correction: Use the shared runtime ownership mechanism for the transaction and check for a foreign game or configured lock immediately before each launch. If contention prevents safe restoration, retain the journal and ownership information for recovery.
- Requested evidence: Use disposable process and lock fixtures to introduce a peer after launch one. Show that launch two is withheld, registry restoration waits for a safe boundary, and normal completion or recovery releases the owned state.

**R7-F2** · P2 · defect · blocks readiness — Item-damage failures omit the donor value and getter patch owners
- Location: `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs:939`; acceptance: 3.A.2, 4.1.2
- Trigger: The registered spear type passes its profile check, but BlueprintItemWeapon.BaseDamage returns the wrong dice or throws for an owned item.
- Expected: The failure identifies the item and predicate, expected and actual damage, the corresponding donor value and relevant getter patch owners in one bounded diagnostic.
- Actual: DamageProblem returns overrideDice, baseDamage and expectedBaseDamage. It has no donor input or patch-owner lookup. A getter exception contributes its full message directly to the startup error, bypassing the type-profile diagnostic's length bound.
- Why it blocks / does not: The new item-level predicate can abort the same blueprint initialization transaction. Its failure omits the information the mission requires to diagnose a patched getter.
- Requested correction: Give the item-damage failure the same bounded diagnostic treatment as the type predicates. Capture donor damage as comparison context, preserve the owned failure if the donor read throws, and report patch owners for BlueprintItemWeapon.BaseDamage.
- Requested evidence: Use a control that changes or throws from BaseDamage on one registered spear item while its weapon type stays correct. Record one failure identifying the item, expected and actual result, donor result and harness patch owner.

**R7-F3** · P2 · evidence_gap · blocks readiness — The fresh-process reload does not cover the corrected spear item damage
- Location: `tools/LoaderCompatHarness/SpearRepairProbes.cs:1106`; acceptance: 3.C.5, 4.1.7
- Trigger: An existing save containing the released foundation spear is loaded after the change to item-level damage overrides.
- Expected: A fresh process running the corrected product loads the released spear and observes 1d8 through the loaded item's blueprint, alongside the established disabled-publication controls.
- Actual: Persistence session P ran on 8ee7eabaeda0, before the item override was cleared. Its reload probe reads spear.Blueprint.Type for damage. X6-1 checks the corrected BlueprintItemWeapon.BaseDamage through a blueprint lookup, without reloading a saved inventory spear.
- Why it blocks / does not: Round six changed construction of the released item referenced by saves. The earlier reload and the current blueprint lookup leave the affected saved-item observation uncovered.
- Requested correction: Extend the existing reload observation to read the inventory spear's BlueprintItemWeapon.BaseDamage and override flag. Exercise that focused load on the corrected package using an authorized disposable fixture.
- Requested evidence: Record the package and DLL identities, loaded save identity, released spear GUID, override flag and effective item damage with Gunslinger disabled. Include the existing battered-owner, price and restoration results.

**R7-F4** · P3 · optional_polish · non-blocking — Two historical statements still read as claims about the final candidate
- Location: `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md:427`; acceptance: 4.2, 6
- Trigger: An owner uses the investigation's detailed sections to compare package revisions or interpret a current failure.
- Expected: Historical source-equivalence statements name their endpoint, and the present-tense diagnostic example uses the current count.
- Actual: Section 4.2 says src/ and tests/ are unchanged from 18361607eda8 to the final candidate. Git shows four changed files across those directories. Section 3.1 also illustrates the current message with 17 checks although the product now runs 18.
- Why it blocks / does not: The corrected opening, package hashes and controller receipt identify the current product unambiguously.
- Requested correction: Scope the round-five comparison to its checkpoint and update the current diagnostic example to 18. Preserve the 17-check quotations from earlier runs.
- Requested evidence: A text check against Git history and the X6 diagnostic is sufficient.

#### Prior finding assessments

- **R6-F1** — `fix_verified` — Foundation, named and progression construction clear the inherited dice override and validate effective item damage. X6-1 completed the item-API assertions for the 2d6 native donor, 1d8 foundation spear and 1d8 Boughkeeper.
- **R6-F2** — `fix_verified` — The registered type now checks the live Enchantments collection against the movement enchantment by object identity. X6-2 recorded the intact backing field, empty getter result, harness patch owner, exactly one profile failure and successful selector restoration.
- **R6-F3** — `fix_verified` — The validator checks GetSubCategories separately from HasSubCategory. X6-3 recorded Simple only in the returned list, one list failure naming both patch owners, and successful selector restoration.
- **R6-F4** — `fix_verified` — The opening now leads with product dfc86851a8 and package revision 0a926ce1d8bd, then identifies the round-five and round-two artifacts. Those identities match Git history and the recorded package hashes.

#### Evidence assessment

- **Inspected:** The mission, repository validation and workflow instructions, M..H feature diff, developer report, CLI transcript, controller gate receipt and prior findings ledger.; Spear type and item construction, foundation and named registration, progression variants, selector publication and rollback, battered-origin pricing, and the Expanded Summoning publication and menu hooks.; The loader-compat runner and shared helpers, all eight spear session definitions, the native probes, focused domain tests and committed investigation record.
- **Executed verification:** Read-only Git checks confirmed HEAD, a clean feature diff under git diff --check, and identical product, test and harness source trees at native package revision 0a926ce1d8bd and the review commit. The supplied mission and committed mission matched the controller's SHA-256.; The controller receipt at 7f44ae6e7415 records successful production compilation and 29 of 29 selected elven-branched-spear tests in 1.8 seconds. The transcript also records the guarded push of that commit.; The developer transcript identifies the tested ZIP as 83c936d58bdda1e7e09b6988e612c4550287d8b0d2ba212a281001e1965fcb75 and the loaded product DLL as 5e7d69d17e9dfd5509c4b32e0ef40cedfed4c12e8d13a1ea5df1ccd33ee3fe7d. X6 passed 3 launches, C6 passed 3, R6 passed 2 and S6 passed 12. Each session summary recorded registry restoration and unchanged owner saves and installation.; S6-2 recorded 25 price queries and 25 tooltip fills per ordinary weapon after failed startup. The four prices were 1, 4, 5 and 13 gp, with matching tooltip values and zero throws. R6 passed the new sorcerer's visible-menu, selected Fire Beetle, cast-success and spawned-unit assertions under UMM 0.31.1 with the four available reported mods.; The prior verified persistence session loaded a saved spear, battered pistol and controls in a fresh process with Gunslinger disabled on package 8ee7eabaeda0. It established the battered owner's identity and 22 gp price on that revision.
- **Source inspection only:** The runner's behavior when a foreign process or lock appears after session preflight, including the final registry restoration path.; The diagnostic emitted by DamageProblem for a wrong or throwing item-level getter. X6-1 exercised successful normalization of the donor override.; The strict origin-creation and ownership paths still require the token, and the summoning menu hook checks for a published variants component before applying its layout.
- **Required gaps:** A focused contention check covering a peer appearing between native launches and safe recovery while that peer is active.; A bounded item-level BaseDamage failure showing the donor comparison and the implicated getter's patch owners.; A fresh-process observation of effective damage on the inventory spear loaded with the corrected product.
- **Limitations:** The reporter's raw log, ArmsArmor 1.0.10 and full 43-mod installation were unavailable. The simulated donor mutation establishes a failure mechanism. The reporter's precise failing predicate and its cause remain unknown.; The read-only review used controller receipts, recorded shell output and earlier verified findings for execution evidence. Some transcript projections are truncated.; The round-three probe-lifecycle and selection controls used the earlier product package. The original report's exact empty-stub appearance was not independently observed.

#### Summary

- **Delivered behavior:** The repaired spear constructs with its intended profile under the simulated donor mutations. Deliberately invalid type profiles fail visibly and restore selector state. Ordinary weapon tooltips retain native prices after failed startup. With the available reporter mods and settings, a new sorcerer opened the creature menu and summoned the selected Fire Beetle.
- **Scope reviewed:** The complete final mission for PR #195, including the fixed product, affected native scenarios, runtime protection, evidence provenance and all four pending prior findings.
- **Pending later-phase work:** none
- **Unresolved questions:** Which predicate failed in the reporter's 43-mod installation, and what changed it, remain unknown.
- **Remaining owner review:** Judge player-facing presentation and decide whether to merge or release after the required corrections.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a2003f-91a1-7de1-8502-b2902b6d1942`, duration 691.5s, response sha256 `6516a5a64b0bca5040b6bf5dad8678b08ed45cb7ab9115911633ef352cbbaa9e`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r07-bfa818`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "7f44ae6e74155cc012670812a65507e0b25a8d8d",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "All four round-six corrections are verified. Three requirements remain open. The multi-launch runner checks for competing sessions only once, the new item-damage failure omits required diagnostic context, and the saved-spear check predates the change to registered item damage.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git checks confirmed HEAD, a clean feature diff under git diff --check, and identical product, test and harness source trees at native package revision 0a926ce1d8bd and the review commit. The supplied mission and committed mission matched the controller's SHA-256.",
      "The controller receipt at 7f44ae6e7415 records successful production compilation and 29 of 29 selected elven-branched-spear tests in 1.8 seconds. The transcript also records the guarded push of that commit.",
      "The developer transcript identifies the tested ZIP as 83c936d58bdda1e7e09b6988e612c4550287d8b0d2ba212a281001e1965fcb75 and the loaded product DLL as 5e7d69d17e9dfd5509c4b32e0ef40cedfed4c12e8d13a1ea5df1ccd33ee3fe7d. X6 passed 3 launches, C6 passed 3, R6 passed 2 and S6 passed 12. Each session summary recorded registry restoration and unchanged owner saves and installation.",
      "S6-2 recorded 25 price queries and 25 tooltip fills per ordinary weapon after failed startup. The four prices were 1, 4, 5 and 13 gp, with matching tooltip values and zero throws. R6 passed the new sorcerer's visible-menu, selected Fire Beetle, cast-success and spawned-unit assertions under UMM 0.31.1 with the four available reported mods.",
      "The prior verified persistence session loaded a saved spear, battered pistol and controls in a fresh process with Gunslinger disabled on package 8ee7eabaeda0. It established the battered owner's identity and 22 gp price on that revision."
    ],
    "inspected": [
      "The mission, repository validation and workflow instructions, M..H feature diff, developer report, CLI transcript, controller gate receipt and prior findings ledger.",
      "Spear type and item construction, foundation and named registration, progression variants, selector publication and rollback, battered-origin pricing, and the Expanded Summoning publication and menu hooks.",
      "The loader-compat runner and shared helpers, all eight spear session definitions, the native probes, focused domain tests and committed investigation record."
    ],
    "limitations": [
      "The reporter's raw log, ArmsArmor 1.0.10 and full 43-mod installation were unavailable. The simulated donor mutation establishes a failure mechanism. The reporter's precise failing predicate and its cause remain unknown.",
      "The read-only review used controller receipts, recorded shell output and earlier verified findings for execution evidence. Some transcript projections are truncated.",
      "The round-three probe-lifecycle and selection controls used the earlier product package. The original report's exact empty-stub appearance was not independently observed."
    ],
    "required_gaps": [
      "A focused contention check covering a peer appearing between native launches and safe recovery while that peer is active.",
      "A bounded item-level BaseDamage failure showing the donor comparison and the implicated getter's patch owners.",
      "A fresh-process observation of effective damage on the inventory spear loaded with the corrected product."
    ],
    "source_inspection_only": [
      "The runner's behavior when a foreign process or lock appears after session preflight, including the final registry restoration path.",
      "The diagnostic emitted by DamageProblem for a wrong or throwing item-level getter. X6-1 exercised successful normalization of the donor override.",
      "The strict origin-creation and ownership paths still require the token, and the summoning menu hook checks for a published variants component before applying its layout."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Assert-LcPreflight runs once before staging. The loop calls Start-LcKingmakerCopy for each launch without rechecking the process or foreign locks. The finalizer waits up to 60 seconds and then calls Restore-LcRegistry even if a Kingmaker process is still running. The session owns loader-compat.lock while the generic runtime launcher uses compatibility.lock.",
      "blocking_rationale": "On the shared DATA host, a later launch can overlap another session and cleanup can overwrite that session's game settings. The successful restoration fields describe the completed runs and do not exercise this contention path.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "Each fresh launch verifies that the host is available. Registry restoration proceeds under transaction ownership once the host is safe.",
      "id": "R7-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1",
        "line": 232
      },
      "priority": "P1",
      "requested_correction": "Use the shared runtime ownership mechanism for the transaction and check for a foreign game or configured lock immediately before each launch. If contention prevents safe restoration, retain the journal and ownership information for recovery.",
      "requested_evidence": "Use disposable process and lock fixtures to introduce a peer after launch one. Show that launch two is withheld, registry restoration waits for a safe boundary, and normal completion or recovery releases the owned state.",
      "title": "Later native launches can proceed after another session takes the host",
      "trigger": "Another Kingmaker process starts or a sibling lab acquires a runtime lock after the spear session's initial preflight."
    },
    {
      "acceptance_ids": [
        "3.A.2",
        "4.1.2"
      ],
      "actual": "DamageProblem returns overrideDice, baseDamage and expectedBaseDamage. It has no donor input or patch-owner lookup. A getter exception contributes its full message directly to the startup error, bypassing the type-profile diagnostic's length bound.",
      "blocking_rationale": "The new item-level predicate can abort the same blueprint initialization transaction. Its failure omits the information the mission requires to diagnose a patched getter.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The failure identifies the item and predicate, expected and actual damage, the corresponding donor value and relevant getter patch owners in one bounded diagnostic.",
      "id": "R7-F2",
      "location": {
        "file": "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
        "line": 939
      },
      "priority": "P2",
      "requested_correction": "Give the item-damage failure the same bounded diagnostic treatment as the type predicates. Capture donor damage as comparison context, preserve the owned failure if the donor read throws, and report patch owners for BlueprintItemWeapon.BaseDamage.",
      "requested_evidence": "Use a control that changes or throws from BaseDamage on one registered spear item while its weapon type stays correct. Record one failure identifying the item, expected and actual result, donor result and harness patch owner.",
      "title": "Item-damage failures omit the donor value and getter patch owners",
      "trigger": "The registered spear type passes its profile check, but BlueprintItemWeapon.BaseDamage returns the wrong dice or throws for an owned item."
    },
    {
      "acceptance_ids": [
        "3.C.5",
        "4.1.7"
      ],
      "actual": "Persistence session P ran on 8ee7eabaeda0, before the item override was cleared. Its reload probe reads spear.Blueprint.Type for damage. X6-1 checks the corrected BlueprintItemWeapon.BaseDamage through a blueprint lookup, without reloading a saved inventory spear.",
      "blocking_rationale": "Round six changed construction of the released item referenced by saves. The earlier reload and the current blueprint lookup leave the affected saved-item observation uncovered.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "A fresh process running the corrected product loads the released spear and observes 1d8 through the loaded item's blueprint, alongside the established disabled-publication controls.",
      "id": "R7-F3",
      "location": {
        "file": "tools/LoaderCompatHarness/SpearRepairProbes.cs",
        "line": 1106
      },
      "priority": "P2",
      "requested_correction": "Extend the existing reload observation to read the inventory spear's BlueprintItemWeapon.BaseDamage and override flag. Exercise that focused load on the corrected package using an authorized disposable fixture.",
      "requested_evidence": "Record the package and DLL identities, loaded save identity, released spear GUID, override flag and effective item damage with Gunslinger disabled. Include the existing battered-owner, price and restoration results.",
      "title": "The fresh-process reload does not cover the corrected spear item damage",
      "trigger": "An existing save containing the released foundation spear is loaded after the change to item-level damage overrides."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "Section 4.2 says src/ and tests/ are unchanged from 18361607eda8 to the final candidate. Git shows four changed files across those directories. Section 3.1 also illustrates the current message with 17 checks although the product now runs 18.",
      "blocking_rationale": "The corrected opening, package hashes and controller receipt identify the current product unambiguously.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "Historical source-equivalence statements name their endpoint, and the present-tense diagnostic example uses the current count.",
      "id": "R7-F4",
      "location": {
        "file": "docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md",
        "line": 427
      },
      "priority": "P3",
      "requested_correction": "Scope the round-five comparison to its checkpoint and update the current diagnostic example to 18. Preserve the 17-check quotations from earlier runs.",
      "requested_evidence": "A text check against Git history and the X6 diagnostic is sufficient.",
      "title": "Two historical statements still read as claims about the final candidate",
      "trigger": "An owner uses the investigation's detailed sections to compare package revisions or interpret a current failure."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F1",
      "rationale": "Foundation, named and progression construction clear the inherited dice override and validate effective item damage. X6-1 completed the item-API assertions for the 2d6 native donor, 1d8 foundation spear and 1d8 Boughkeeper."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F2",
      "rationale": "The registered type now checks the live Enchantments collection against the movement enchantment by object identity. X6-2 recorded the intact backing field, empty getter result, harness patch owner, exactly one profile failure and successful selector restoration."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F3",
      "rationale": "The validator checks GetSubCategories separately from HasSubCategory. X6-3 recorded Simple only in the returned list, one list failure naming both patch owners, and successful selector restoration."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F4",
      "rationale": "The opening now leads with product dfc86851a8 and package revision 0a926ce1d8bd, then identifies the round-five and round-two artifacts. Those identities match Git history and the recorded package hashes."
    }
  ],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r07-bfa818",
  "round": 7,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The repaired spear constructs with its intended profile under the simulated donor mutations. Deliberately invalid type profiles fail visibly and restore selector state. Ordinary weapon tooltips retain native prices after failed startup. With the available reporter mods and settings, a new sorcerer opened the creature menu and summoned the selected Fire Beetle.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Judge player-facing presentation and decide whether to merge or release after the required corrections."
    ],
    "scope_reviewed": "The complete final mission for PR #195, including the fixed product, affected native scenarios, runtime protection, evidence provenance and all four pending prior findings.",
    "unresolved_questions": [
      "Which predicate failed in the reporter's 43-mod installation, and what changed it, remain unknown."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
