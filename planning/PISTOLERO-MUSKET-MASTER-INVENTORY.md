# Pistolero and Musket Master Pre-implementation Inventory

Recorded before archetype feature implementation on branch
`codex/pistolero-musket-master-archetypes`, starting at published checkpoint
`8ade461eab25f8fc2b068d8a739aa8ee1044f850`.

## Starting equipment and battered ownership

- `Blueprints/GunslingerClassBlueprints.cs` creates the base class with exact
  `StartingItems` order: production Pistol, black powder, lead ball, gunsmith
  kit. Validation requires all four exact references. This base array is not to
  be mutated.
- `Gunsmithing/GunslingerStartingFirearmOwnershipPatch.cs` is an exact Harmony
  prefix/postfix on `LevelUpHelper.AddStartingItems(UnitDescriptor)`. It admits
  only the receiver whose maximum class is the exact Gunslinger class, snapshots
  shared inventory by reference identity plus powder/ball counts, observes a
  newly added production Pistol, accepts the detached no-delta path, requires
  native +1/+1 ammunition, adds 19/19, binds battered origin, and rolls back only
  the added ammunition on failure. It is hard-coded to Pistol and has no
  archetype/choice resolver or wrong-starter rejection.
- `BatteredFirearmOriginRuntime` stores owner identity in one exact item
  enchantment `MechanicsContext.MaybeCaster`, rejects multiple tokens and
  rebinding, and is idempotent for the same exact item/owner. The blueprint is
  project-owned and resolved through `BlueprintBootstrap.BatteredOrigin`.
- Existing focused source/runtime coverage includes Sprint 34 starting-item
  observers and `scripts/Test-Sprint87StartingFirearmBinding.ps1`; runtime
  runner calls the exact native helper and observes shared-inventory deltas.

### Exact installed Kingmaker 2.1.7b contract

Read-only `ildasm` inspection of installed `Assembly-CSharp.dll` proves:

- `BlueprintArchetype` exposes public fields
  `bool ReplaceStartingEquipment`, `int StartingGold`, and
  `BlueprintItem[] StartingItems`, plus private exact parent-class field
  `m_ParentClass` and `GetParentClass()`.
- `LevelUpHelper.AddStartingItems(UnitDescriptor)` has the exact one-argument
  static signature targeted by the patch. It gets the receiver's maximum class
  and `ClassData`, selects the first committed archetype whose native predicate
  qualifies, uses the archetype's `StartingGold` and `StartingItems` when found,
  otherwise the class fields, calls `unit.Inventory.Add(BlueprintItem)` once per
  array entry, then attempts native equipment-slot insertion. It does not clone
  or substitute item blueprints.
- The method exits before grants for a receiver that is neither the main
  character nor a custom companion. This exactly explains the existing
  detached-character-creation no-delta observation and confirms that the
  project observer must not synthesize missing items.
- Native grants add one entity/stack unit for each array entry. The existing
  project top-up from native +1/+1 to 20/20 therefore remains required.

This contract directly supports the mandatory Musket Master design:
`ReplaceStartingEquipment = true` with exact archetype `StartingItems` and an
observer that resolves the expected firearm from committed archetype state.

## Class/archetype architecture and bootstrap

- `GunslingerClassBlueprints.Register` creates the class, all base deeds,
  progression, hard-coded deed summaries, two True Grit level-20 selections,
  then registers Mysterious Stranger. Its dependency signature currently
  receives full proficiency and only the starting Pistol plus supplies.
- Mysterious Stranger uses the required native structure: exact parent set
  through `m_ParentClass`, explicit `RemoveFeatures`/`AddFeatures`, and append to
  `cls.Archetypes`. Its append is currently simple `Concat`, so final archetype
  work must replace this with deterministic identity-aware append and exact
  prior-array rollback without changing its GUID or replacement behavior.
- `BlueprintBootstrap` is a one-time fail-closed transaction with registry
  rollback plus exact class/feat/vendor publication rollback. Current expected
  registered count is 206. Initialization order is proficiency, feats, test and
  production firearms, state/items/actions, Gunslinger class, action grants,
  presentation/icons, then publication. New registration must split creation
  and final wiring where needed to avoid proficiency/feat/class cycles.
- `blueprints/blueprints.json` is the stable symbol/GUID authority and the
  non-SDK `KingmakerGunslinger.csproj` has an explicit compile list. Both counts
  and validators must advance transactionally; no existing symbol may change.

## Firearm identity, handedness, proficiency, and feats

- Exact firearm identity is one `FirearmDefinitionComponent` on the project
  weapon type. `ProductionFirearmWeaponSpec.IsTwoHanded` already validates
  Pistol/Revolver false and Musket/Blunderbuss/Rifle true, but the kind list is
  embedded in its constructor. Donor crossbow category is presentation-only.
- `FirearmProficiencyBlueprints` preserves stable hidden full-proficiency symbol
  `KMG.Firearms.FirearmProficiency`. It currently grants the one Reload ability
  and Scatter Shot ability through one `AddFacts` component.
- `FirearmProficiencyRestriction` currently stores only one required full fact
  and permits equipment solely by that rank. Every production firearm contains
  exactly one such restriction; validation currently expects the one full fact.
  Test Musket uses the same component but is development-only and must not enter
  archetype starter logic.
- `FirearmFeatBlueprints` publishes one custom Rapid Reload selection with five
  exact per-kind choices and five hidden parameter features for native Weapon
  Focus/dependent parametrized menus. Publication appends exact missing project
  identities to current native selections and captures exact prior arrays.
- `NativeFirearmFeatIntegration` transactionally appends project firearm
  parameters to native menus and applies attack/damage/critical adapters by
  exact marker kind. It currently has no scoped-proficiency prerequisite gate.
- There is no existing Exotic Weapon Proficiency (Firearms) equivalent. The
  mission therefore requires one public combat feat granting the existing full
  fact, with exact installed BAB prerequisite and duplicate-prevention contract
  inspected before blueprint construction.

## Training and misfire

- `GunTrainingBlueprints` provides one obligatory selection at levels
  5/9/13/17 with five exact-kind facts. Each fact owns a
  `GunTrainingDamage` subscriber that independently adds Dexterity damage.
- `GunTrainingPolicy` contains exact-kind damage arithmetic and Broken +2 versus
  untrained +4 misfire arithmetic. It rejects Wrecked and unsupported kinds.
- Duplicate `HasGunTraining` scans exist in ordinary misfire, Scatter Shot, and
  Dead Shot runtime. The latter two also independently inspect exact-kind facts.
  This must become one authoritative highest-entitlement service with one-event
  idempotence and shared ordinary/Dead Shot/misfire callers.

## Reload policy

- `ReloadActionEconomy.Evaluate(definition, hasMatchingRapidReload)` is the one
  pure central policy. Without the feat it returns the definition's base action;
  with it, advanced becomes Free, early FullRound becomes Standard, and other
  early profiles become Move.
- Callers are Reload ability availability/logic, all three AbilityData/command
  presentation patches, and `FreeActionFullAttackReloadPatch`. Each resolves
  exact equipped firearm and matching Rapid Reload at the point of use.
- Lightning Reload has its separate per-round marker/service and currently
  supersedes ordinary flow through existing runtime availability. Fast Musket
  must extend this same policy and all caller state reads rather than add an
  ability or cached action.

## Range, attacks, and deeds

- `FirearmArmorClassRuntime` keeps a per-thread exact attack stack and weak event
  stamps; `FirearmArmorClassService` computes increment from immutable
  `FirearmDefinition.RangeIncrementFeet` and applies touch AC only in the first
  increment or when Deadeye authorized.
- `DeadeyeRuntime` arms one exact firearm attack, consumes only on an eligible
  discharge, and calls `DeadeyeService` with distance and immutable definition.
  Steady Aim needs an exact per-attack effective-range context consumed by both
  services before these calculations.
- Existing native range/max-range and penalty inspection remains required before
  the Steady Aim adapter. No shared blueprint/type mutation is acceptable.
- Mysterious Stranger Clipping Shot demonstrates a free-action armed buff and
  hit/miss post-resolution `RuleDealDamage` with `Modifier = 0.5f`; it is a
  structural precedent only. Up Close and Deadly requires its own precision,
  immunity, fixed-cost, misfire/scatter, critical, Dead Shot, rollback, and
  duplicate-event semantics.
- Targeting Legs provides the existing prone delivery path to extract/reuse for
  Twin Shot. Dead Shot already exposes probe/final-delivery discrimination.
- Bleeding Wound, Deadeye, Dead Shot, Targeting Legs, and Mysterious Stranger
  runtimes are isolated blueprint/policy/runtime adapters suitable as structural
  precedents; none may be cloned by identity.

## True Grit, summaries, icons, and action bar

- `TrueGritCatalog` is currently a fixed base-deed catalog; blueprint choices
  are not ownership-filtered. Runtime cost/positive-grit semantics are central
  in `TrueGritRuntime`/`TrueGritService`. It must be expanded with exact deed
  ownership prerequisites and real runtime use for Focused Aim, Twin Shot,
  Steady Aim, and Fast Musket only.
- `DeedTierBlueprints` descriptions are base hard-coded summaries. Archetypes
  require stable substituted summary facts at affected tiers without mutating
  base descriptions.
- `PlayerFacingPresentation` and `ProjectAssetIcons` traverse base progression
  and selected known children, with one-off special casing for Mysterious
  Stranger content. They must become generic recursive traversal over
  archetype additions, selections, abilities, and buffs.

## Compatibility and runtime harness

- The merged profile schema/catalog, sentinel-owned staging/restoration wrapper,
  root/catalog/chargen observer, and exact-profile dispositions are authoritative.
  Profile IDs/mod keys/UMM IDs/evidence classifications are immutable absent new
  evidence; only package pins and eligible scenario lists advance.
- `OptionalModCompatibilityObserver` is read-only and validates current exact
  class/root/chargen/archetype identities without third-party mutation. It must
  report all three project archetypes exactly once while preserving unrelated
  entries and all existing CotW 46-class assertions.
- `EvasiveBlueprints.PreservesCurrentComponentContract` compares each clone to
  the current donor's ordered component types, not vanilla counts. This repair
  is mandatory and must remain untouched in semantics.
- Runtime scenario registration is centralized in
  `RuntimeTestScenarioCatalog`, request validation, runner dispatch, preflight,
  and compatibility allowlists. New scenarios must follow guarded Steam App ID
  640820, structured evidence, no-save-write, disposable-fixture conventions.

## Inventory conclusion and next action

The native archetype starting-equipment contract is proven safe for the
mandatory Musket grant. No hard stop exists. The first source phase is the
canonical firearm handedness policy plus scoped proficiency foundations and
focused domain tests, followed by the generalized expected-starter resolver and
native grant observer. Archetype feature construction remains deferred until
these shared contracts pass the complete source/build/package gate.
