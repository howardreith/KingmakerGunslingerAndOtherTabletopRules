# Acadamae Graduate Spell and Prerequisite Inventory

Status: IN PROGRESS

Exact assembly findings (2026-08-09): `AbilityData` exposes `ActionType`, `RuntimeActionType`, `RequireFullRoundAction`, `Spellbook`, `SpellLevel`, `ParamSpellbook`, `ParamSpellSlot`, and `SpellSource`. `UnitUseAbility` owns the exact `AbilityData Spell`, `m_CastTime`, `OnAction`, and cast-command creation surface. Kingmaker represents Fatigued/Exhausted as both `UnitCondition` values and `SpellDescriptor.Fatigue`/`Exhausted`. Call of the Wild 1.14.4c-2.1 patches `AbilityData.ActionType` (`TurnActionMechanics.AbilityData_ActionType__Patch`) and `RuleCastSpell` (`MetamagicFeats+RuleCastSpell_OnTrigger_Patch`), so final implementation must compose with those exact owners.

Exact CotW bytes: UMM ID/version `CallOfTheWild` 1.14.4c-2.1; DLL SHA-256 `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`. Its supplied blueprint catalog contains no Acadamae Graduate or Cord identity/name collision. CotW adds/changes Wizard school content, including subschools, Pact Wizard, Opposition Research, and exact Conjuration opposition-research identity `f5a49f7d5896469fab8b64edf3723598`; these do not yet prove prerequisite equivalence.

Exact action/cast IL (2026-08-09): `AbilityData.ActionType` resolves item, Quicken, free-action, spontaneous-metamagic, then blueprint action state. `RuntimeActionType` starts from that result and applies the native combat cooldown conversion. `RequireFullRoundAction` is the independent full-round overlay on a Standard command and consults `BlueprintAbility.IsFullRoundAction`. The public two-argument `UnitUseAbility` constructor obtains `RuntimeActionType` and chains to `(CommandType, AbilityData, TargetWrapper)`. `UnitUseAbility.OnAction` performs all cancellation/availability/concentration checks before synchronously constructing and triggering the exact `RuleCastSpell`; only `RuleCastSpell.Success` proceeds to execution. `UnitUseAbility.OnEnded(bool)` is the authoritative bounded cleanup surface.

CotW composition (same exact DLL hash): two default-priority postfixes mutate `AbilityData.RequireFullRoundAction` (`SpellManipulationMechanics.AbilityData__RequireFullRoundAction__Patch` and `TurnActionMechanics.AbilityData__RequireFullRoundAction__Patch`), and `TurnActionMechanics.AbilityData_ActionType__Patch` mutates `ActionType`. Acadamae therefore declares an exact after-owner relationship, reads the final pre-Acadamae full-round result under a narrow thread-local inspection bypass, alters only that Boolean, arms only the constructed `UnitUseAbility`, scopes the active marker to `OnAction`, consumes the exact `AbilityData` once only after successful `RuleCastSpell`, and clears on action exit/end. UI getter queries never arm fatigue/save state.

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
| Respec/pending multiple opposition selections | Conjuration in any selected opposition state is ineligible | Adapter enumerates every matching `FeatureSelectionState`; prior last-only approach rejected because Wizard opposition selection can occur more than once |

Spell candidate table will record GUID, school, Summoning descriptor/component, lists, action type, full-round/multi-round representation, variants, metamagic, and prepared/spontaneous delivery for every installed base/CotW candidate. No English-name matching is permitted.

Next concrete action: build the realistic guarded Acadamae spellbook/cast scenario and validate the pending prerequisite adapter against real level-up state where safely constructible.
