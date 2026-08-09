# Acadamae Graduate Spell and Prerequisite Inventory

Status: IN PROGRESS

Exact assembly findings (2026-08-09): `AbilityData` exposes `ActionType`, `RuntimeActionType`, `RequireFullRoundAction`, `Spellbook`, `SpellLevel`, `ParamSpellbook`, `ParamSpellSlot`, and `SpellSource`. `UnitUseAbility` owns the exact `AbilityData Spell`, `m_CastTime`, `OnAction`, and cast-command creation surface. Kingmaker represents Fatigued/Exhausted as both `UnitCondition` values and `SpellDescriptor.Fatigue`/`Exhausted`. Call of the Wild 1.14.4c-2.1 patches `AbilityData.ActionType` (`TurnActionMechanics.AbilityData_ActionType__Patch`) and `RuleCastSpell` (`MetamagicFeats+RuleCastSpell_OnTrigger_Patch`), so final implementation must compose with those exact owners.

Exact CotW bytes: UMM ID/version `CallOfTheWild` 1.14.4c-2.1; DLL SHA-256 `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`. Its supplied blueprint catalog contains no Acadamae Graduate or Cord identity/name collision. CotW adds/changes Wizard school content, including subschools, Pact Wizard, Opposition Research, and exact Conjuration opposition-research identity `f5a49f7d5896469fab8b64edf3723598`; these do not yet prove prerequisite equivalence.

Required eligibility contract: actual/pending Wizard level 1+, true specialist, not Universalist, and Conjuration not forbidden. Cast eligibility additionally requires the exact feat, real prepared arcane spellbook invocation, Conjuration school, native Summoning marker, and effective time longer than Standard.

| Configuration | Expected | Exact evidence |
|---|---|---|
| Non-Wizard | Ineligible | Installed contract inspection pending |
| Pending level-1 specialist Wizard | Eligible if Conjuration allowed | Pending level-up contract inspection pending |
| Universalist Wizard | Ineligible | Exact specialization fact/selection pending |
| Conjuration specialist | Eligible | Exact school fact pending |
| Other specialist, Conjuration allowed | Eligible | Exact opposition facts pending |
| Specialist, Conjuration forbidden | Ineligible | Exact Conjuration opposition identity pending |
| Archetype giving up specialization | Ineligible | Base/CotW archetype inventory pending |
| Respec pending selections | Rule-dependent | Exact level-up state inspection pending |

Spell candidate table will record GUID, school, Summoning descriptor/component, lists, action type, full-round/multi-round representation, variants, metamagic, and prepared/spontaneous delivery for every installed base/CotW candidate. No English-name matching is permitted.

Next concrete action: inspect installed blueprint GUIDs, spellbook/school prerequisites, AbilityData action surfaces, UnitUseAbility, and RuleCastSpell lifecycle.
