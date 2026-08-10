# Feature Modules, Acadamae Graduate, and Cord Implementation Report

Status: COMPLETE

Base is `7a99ce5ac6d6976212310f997bd39ddfe4a57935` (0.0.74). The release candidate is 0.0.75 / `0.0.75-feature-modules-acadamae-graduate` on `codex/feature-modules-acadamae-graduate`.

## Delivered architecture

- `FeatureModuleSettingsStore` loads one schema-versioned JSON settings file before blueprint publication. Missing, legacy, or malformed input defaults to ON/ON; malformed bytes are retained as `.malformed-*`; saves use atomic replacement.
- `FeatureModuleConfiguration` is the immutable active process snapshot. The composed UMM GUI displays active and saved-next-restart values, shows restart-required drift, and draws the existing development panel afterward.
- The identity layer always registers 250 active project blueprints. Publication transactions independently gate the Gunslinger class/feats/parameters/acquisition surfaces and Acadamae feat/Cord vendor row without unregistering identities.
- Reconciliation merges against current native/foreign arrays by exact reference/GUID, preserves unrelated order and entries, rejects duplicates, is idempotent, and does not republish disabled modules.

## Acadamae Graduate

`KMG.Feats.AcadamaeGraduate` / `7939ff087cb843729448589ba2de19f1` is a rank-one general feat. Its prerequisite adapter requires an actual or pending Wizard level, a specialist school, and no Universalist/replaced-specialization/Conjuration-opposition state. It uses exact native blueprint identities, including supported local Call of the Wild shapes, rather than localized names.

Per invocation, the casting adapter requires a real prepared arcane spellbook cast, Conjuration school, Summoning descriptor, feat ownership, and effective Full-Round casting. `AbilityData.RequireFullRoundAction`, runtime action type, command construction, command lifetime, and successful `RuleCastSpell` completion share an exact command/ability correlation. A successful accelerated cast triggers one native Fortitude save at DC 15 + actual spell level; failure applies the canonical native Fatigued buff. Cancellation and unrelated/repeated queries do not consume or leak markers.

## Cord of Stubborn Resolve

`KMG.Items.CordOfStubbornResolve` / `c4b804d9ebf941b4842b0a461a2b6b6d` is a belt-slot item costing 15,000 gp and weighing one pound. It uses the native equipment enhancement-stat contract for +2 Constitution.

The exact equipped item intercepts canonical Fatigued buffs at `BuffCollection.TriggerRuleApplyBuff` and direct Fatigued/Exhausted conditions at `UnitState.AddCondition`. Fatigue is rejected after one d6 substitution; exhaustion is rejected, deals one d6, and applies one bypassed native Fatigued condition. Thread-scoped tokens prevent recursion and duplicate rolls. Kingmaker 2.1.7b exposes no usable native nonlethal accumulator, so the authorized fallback deals untyped self-damage capped at HP minus one and reports itself as nonlethal-equivalent.

The Cord is published once, count one, to `SmithVendorTable` (`7de959347266092448d8a72089ef9778`), whose exact capital owners are `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith`. No other merchant, loot, BTSL, or crafting path is added.

## Final qualification

Repository/source/build/package gates pass with 967/967 deterministic tests. All four standalone module combinations pass with a constant 250-identity set. The exact release commit passed consecutive standalone integration, consecutive highest-risk combined-profile integration, and two guarded `KMG_AUTOMATION_WORKING` smoke runs. Exact Call of the Wild, Arms & Armor, Toggle Custom Soundpacks, qualified combined, and high-risk combined transactions pass and restore exact state; transaction and run IDs are in the journal and qualification document.

## 0.0.76 playtest repair addendum

The accepted 0.0.75 architecture remains intact. Version 0.0.76 appends two
always-loaded mode identities (252 active total), makes Acadamae acceleration a
default-OFF per-character native activatable, makes failed-save canonical
Fatigued root-context and permanent until native rest, and replaces the Cord's
donor sprite with original project art. Exact evidence is retained in
`docs/ACADAMAE-PLAYTEST-REPAIR-QUALIFICATION.md`.

The qualified package built from exact release source `8fea913d7baba880496012576878ac95eaadc74d` has SHA-256 `f05edc2d2176969b33132b367c5734e60248ba06aaf8a44ecd0c2cc3fb51e7f7`; its DLL SHA-256 is `ee6b4d1d255871d6037574ba421c1e55c5cf7aa1730ebe96b71ee1bf16178df2`.

Next concrete action: retain the clean published branch unmerged for user review.
