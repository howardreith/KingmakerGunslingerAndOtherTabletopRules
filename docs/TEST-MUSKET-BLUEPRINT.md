# Test Musket blueprint contract

## Purpose

The Test Musket proves that Kingmaker can host a custom firearm identity while retaining the ordinary weapon/item pipeline. It is derived from native Heavy Crossbow blueprints so equip slots, attack commands, visuals, animations, projectile behavior, and ordinary statistics remain inherited until later sprints change them deliberately.

Sprint 7 added a dedicated firearm-proficiency gate and manual development access. Sprint 9 adds marker-scoped touch-AC selection inside the first firearm range increment.

## Source and clone identities

```text
Native Heavy Crossbow type:          36d0551b8a28587438a47fcbbf53c083
Custom Test Musket type:             6e499550b44c41b3a1ef0693904a46b8

Native Standard Heavy Crossbow item: 19a5092244dcf99478dcd73c974828b1
Custom Test Musket item:             09641295ceea4c558400c43df2ddf1f9

Firearm Proficiency feature:         5148f69223044799800b65732b6cabea
```

Custom internal names:

```text
KMG_TestMusket_WeaponType
KMG_TestMusket_Item
KMG_FirearmProficiency_Feature
```

## Clone strategy

`BlueprintCloneService` calls `UnityEngine.Object.Instantiate(source)` and assigns only the clone's internal name. `BlueprintRegistry` assigns the custom GUID and inserts the clone into both Kingmaker blueprint indexes.

The native sources are snapshotted before cloning. Final validation confirms:

- native internal names remain unchanged;
- native weapon-type component count and references remain unchanged;
- native item component count and references remain unchanged;
- the native item still references the native weapon type;
- neither custom object is reference-equal to its source.

## Firearm identity marker

The custom weapon type receives exactly one component named:

```text
$KMG_FirearmDefinition
```

Its immutable data reconstructs exactly to `FirearmDefinitions.CreateEarlyMusket()`:

```text
Era:                Early
Kind:               Musket
Capacity:           1
Range increment:    40 feet
Misfire value:      2, representing natural 1–2
Base reload:        Full-round
Free hand required: true
Rounds per action:  1
Scatter:            false
```

The marker is the authority for firearm rules. The inherited Heavy Crossbow category is only an engine/animation adapter.

## Firearm proficiency restriction

The custom item receives exactly one component named:

```text
$KMG_FirearmProficiencyRestriction
```

It references the exact registered `KMG.Firearms.FirearmProficiency` feature. A unit can equip the item only when `UnitDescriptor.GetFeature` returns that feature.

The restriction is attached to the item clone, not the native Standard Heavy Crossbow. Sprint 7's chosen negative behavior is strict equip denial.

Passing this gate does not yet remove category-based Heavy Crossbow nonproficiency during attacks. The first positive-path runtime test should use a martial-proficient unit.

## Item-to-type adapter

The exact Kingmaker member exposing an item's `BlueprintWeaponType` is verified rather than assumed. `WeaponBlueprintAccess`:

1. scans `BlueprintItemWeapon` and base classes;
2. accepts only readable and writable fields/properties of exact type `BlueprintWeaponType`;
3. prefers `Type`, `m_Type`, `WeaponType`, or `m_WeaponType`;
4. accepts one unambiguous non-preferred member as fallback;
5. fails when no compatible member or an ambiguous set exists;
6. reads back after assignment and requires reference equality.

`scripts/inspect-runtime-contracts.ps1` reports the candidate contract before compilation.

## Transaction boundary

The following four custom blueprints form one initialization transaction:

1. hidden diagnostic feature;
2. hidden Firearm Proficiency feature;
3. Test Musket weapon type;
4. Test Musket item.

If manifest resolution, source lookup, cloning, marker/restriction creation, item rewiring, registration, or final verification fails, `BlueprintRegistry.RollbackAll()` removes owned registrations in reverse order where possible.

The registry never replaces an existing dictionary entry and never generates a GUID.

## Development acquisition

Sprint 7 provides no starting equipment, vendor, loot, console command, or automatic inventory insertion.

After a locally compiled build successfully initializes, the UMM panel can manually:

- grant Firearm Proficiency to the selected unit;
- add one Test Musket to shared inventory;
- remove unequipped Test Muskets;
- print equipped-firearm metadata.

These controls are disposable-save infrastructure and are not part of the intended player experience.

## Gameplay status

The Test Musket remains a normal Heavy Crossbow-derived weapon attack, but an exact marker match now selects touch AC within its 40-foot first firearm range increment and ordinary AC beyond it. It still has no ammunition, loaded state, reload command, misfire, breakage, firearm sound, muzzle flash, or custom model.
