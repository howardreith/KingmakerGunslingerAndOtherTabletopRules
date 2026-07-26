# Firearm proficiency and Sprint 7 development controls

## Purpose

Sprint 7 creates the smallest controlled path from registered firearm blueprints to an in-campaign validation attempt. It adds neither normal player acquisition nor firearm combat rules.

The design has three independent pieces:

1. A stable Firearm Proficiency fact.
2. An item-level equipment restriction on the Test Musket.
3. Manual UMM controls for disposable-save testing.

## Firearm Proficiency blueprint

```text
Symbol:        KMG.Firearms.FirearmProficiency
GUID:          5148f69223044799800b65732b6cabea
Runtime type:  BlueprintFeature
Internal name: KMG_FirearmProficiency_Feature
Ranks:         1
HideInUI:      true
Components:    none
```

The feature is intentionally component-free. It is a permission fact, not a bundle of attack bonuses or rule handlers.

It remains hidden because Sprint 7 has no localized name, icon, class progression, feat selection, or release-facing grant path. Later content must reference this same stable blueprint rather than create alternate firearm-proficiency flags.

## Item-level gate

The Test Musket item clone receives exactly one:

```text
FirearmProficiencyRestriction : EquipmentRestriction
```

The restriction serializes a reference to the dedicated proficiency feature and implements:

```text
CanBeEquippedBy(unit) =
  unit != null
  and required proficiency != null
  and unit.GetFeature(required proficiency) != null
```

### Selected consequence

The Sprint 7 consequence for nonproficiency is **equip denial**.

This was chosen because it is:

- easy to distinguish from the inherited Heavy Crossbow category;
- localized to the firearm item;
- impossible to forget when adding an attack modifier later;
- compatible with future classes and feats that grant one shared feature;
- reversible without mutating native weapon categories.

### What it does not solve

The custom restriction controls whether the item can be equipped. It does not yet rewrite Kingmaker's category-based attack proficiency checks.

Because the Test Musket still borrows Heavy Crossbow mechanics, a unit that passes the firearm gate could still inherit a Heavy Crossbow nonproficiency penalty during attack resolution. The initial positive-path test should therefore use a martial-proficient unit. A later adapter must separate firearm attack proficiency from ordinary Heavy Crossbow proficiency without granting the latter globally.

## Native-source isolation

The restriction is appended only after cloning the standard Heavy Crossbow item. Sprint 7 snapshots and verifies:

- native Heavy Crossbow type name;
- native standard-item name;
- native weapon-type component references;
- native item component references;
- native item's weapon-type reference.

Any mutation of the native source aborts initialization and rolls back owned registrations.

## UMM development panel

After successful loader/Harmony bootstrap, `Main.Load` assigns:

```text
modEntry.OnGUI = DevelopmentUi.OnGui
```

The panel contains four buttons.

### Grant Firearm Proficiency

Target resolution order:

1. selected unit through common Kingmaker selection-manager paths;
2. main character through common player members.

The bridge then:

1. resolves the unit descriptor;
2. checks whether the feature already exists;
3. tries compatible `AddFact`/`AddFeature` APIs;
4. falls back to compatible progression-feature APIs;
5. verifies that querying the descriptor returns the feature;
6. reports the exact selected method.

Repeated use is idempotent at the command level: it reports that the unit already has proficiency rather than knowingly adding a duplicate.

### Add one Test Musket

The bridge:

1. requires an active `Kingmaker.Game.Instance` and player state;
2. resolves shared inventory through common member names;
3. records the pre-operation count;
4. tries compatible add methods and argument shapes;
5. verifies either an increased inventory count or a returned created item;
6. reports the resolved method and verified count.

No item is added automatically at startup.

### Remove Test Muskets

The bridge enumerates the shared inventory, selects entities whose item blueprint is the custom Test Musket, and invokes a compatible remove method for each.

It deliberately does not search equipped slots, map entities, vendors, or arbitrary containers. Equipped copies must be unequipped before cleanup.

### Print equipped-firearm diagnostics

The bridge scans common current-hand and equipment-set paths. For each detected equipped weapon it:

1. resolves the item blueprint;
2. resolves the associated weapon type;
3. finds `FirearmDefinitionComponent` instances;
4. requires exactly one marker for a firearm;
5. logs the item, weapon type, and immutable definition;
6. reports whether the selected unit has Firearm Proficiency.

A non-firearm weapon is ignored.

## Reflection safety model

The development bridge uses reflection because the target assemblies are not available in the milestone environment and selection/inventory APIs can differ among Kingmaker builds.

Reflection is contained in `Development/ReflectionAccess.cs` and `KingmakerDevelopmentBridge.cs`. The UI does not perform reflection or direct game mutations.

Safety rules:

- No command runs automatically.
- No command runs before blueprint initialization.
- Missing runtime members or methods produce a failure result.
- By-reference/out methods are not selected.
- A method must have compatible supplied/default/nullable arguments.
- Mutations are verified after invocation where possible.
- Exceptions are contained and written to the KMG log.
- The bridge does not cache unit or inventory entities across commands.

## Save safety

Both grant and add operations introduce custom blueprint references into the current save. The UMM panel displays an explicit disposable-save warning.

Sprint 7 does not offer uninstall cleanup. Removing all visible test items does not remove the granted feature, nor does it prove every custom reference is absent. Test saves should be discarded after validation.

## Runtime acceptance

The source milestone cannot satisfy runtime acceptance. A locally compiled UMM package must demonstrate:

- the panel renders;
- controls fail cleanly at the main menu;
- an unproficient unit cannot equip the Test Musket;
- granting proficiency permits equip on a martial-proficient unit;
- duplicate grants do not duplicate the fact;
- shared-inventory add/remove operations are verified;
- diagnostics report exactly one firearm marker;
- native Heavy Crossbows remain unaffected;
- save/load does not crash.

See [`TESTING.md`](../TESTING.md) for the full matrix.

## Sprint 29 player-facing action grants

Firearm Proficiency grants three exact-firearm abilities through one missing-fact-restoring `AddFacts` component:

- **Reload Test Musket** — full-round, consumes one Black Powder Charge and one Lead Ball, and loads one round while preserving Normal or Broken condition.
- **Overhaul Test Musket** — full-round, consumes one Firearm Repair Kit on completed delivery, and changes the same exact empty/Wrecked item to empty/Broken.
- **Repair Test Musket** — full-round, consumes one Firearm Repair Kit on completed delivery, and changes the same exact empty/Broken item to empty/Normal.

All three abilities fail closed when exact item selection is absent or ambiguous. Overhaul and Repair do not replace the item or create ammunition, and Reload does not change condition. Development controls remain available for disposable-save setup, immediate transaction regression, and diagnosis, but the action-bar abilities are the authoritative player-facing timing path.
