# Sprint 7 completion report

## Milestone

```text
0.0.7-s07-proficiency-controls
```

## Result

Sprint 7 is complete as a **source milestone**.

It adds the dedicated Firearm Proficiency blueprint, applies a strict item-level proficiency restriction to the custom Test Musket, and exposes four manual Unity Mod Manager development controls for a future disposable-campaign smoke test.

It does **not** include a compiled DLL or a Unity Mod Manager install archive and is therefore not ready for Kingmaker under the user's test-readiness definition.

## Implemented content

### Firearm Proficiency

```text
Symbol: KMG.Firearms.FirearmProficiency
GUID:   5148f69223044799800b65732b6cabea
Type:   BlueprintFeature
Ranks:  1
UI:     hidden
```

The feature is component-free and becomes the single permission fact referenced by the Test Musket restriction and future class/feat content.

### Proficiency restriction

`FirearmProficiencyRestriction` derives from Kingmaker's `EquipmentRestriction` and permits equip only when the target `UnitDescriptor` contains the exact Firearm Proficiency feature.

The restriction is appended only to the custom Test Musket item clone. Native Heavy Crossbow type and item components, names, and type linkage are snapshotted and verified unchanged.

### Blueprint transaction

Initialization now registers four custom blueprints in one fail-closed transaction:

1. hidden diagnostic feature;
2. hidden Firearm Proficiency feature;
3. Test Musket weapon type;
4. Test Musket item.

The registry rejects collisions before object creation, uses non-replacing dictionary insertion, verifies exact object identity, and rolls back owned registrations in reverse order when a later step fails.

### Manual development controls

After successful bootstrap, the mod attaches a UMM options panel with buttons to:

- grant Firearm Proficiency to the selected unit;
- add one Test Musket to shared inventory;
- remove unequipped Test Muskets from shared inventory;
- print equipped-firearm diagnostics.

No operation runs automatically. Every command requires initialized blueprints and an active campaign, catches exceptions, logs an operation-specific result, and verifies mutations where the runtime surface permits.

### Runtime reflection adapter

A narrow development-only reflection layer resolves:

- `Kingmaker.Game.Instance` and player state;
- selected unit or main character;
- unit descriptor and feature query/grant methods;
- shared inventory and add/remove methods;
- item blueprint references;
- equipped weapon paths;
- firearm definition markers.

The adapter supports inherited private/public members, first-non-null candidate selection, dotted paths, enumeration, and compatible overload invocation. It rejects by-reference/out methods and fails closed when no supported contract is available.

### Tests and validation model

The dependency-free test project now declares 50 cases:

- 40 immutable firearm-domain cases;
- 10 reflection-helper cases.

The portable Python validator independently models seven scenarios:

1. proficiency registered before the restricted item;
2. exactly one firearm marker;
3. exactly one item-level proficiency restriction;
4. unproficient denial and proficient allowance;
5. native type/item preservation;
6. pre-factory collision rejection;
7. reverse rollback of all four owned registrations.

## Stable IDs

All nine previously reserved GUID values remain unchanged. Sprint 7 changes only the status of `KMG.Firearms.FirearmProficiency` from reserved to active.

```text
Manifest entries: 9
Active entries:   4
Reserved entries: 5
Runtime generation allowed: false
```

## Validation completed in this environment

- Portable Sprint 7 validator passed.
- Blueprint manifest passed Draft 7 JSON Schema validation.
- All 29 C# files parsed with no tree-sitter syntax errors.
- All 11 PowerShell scripts parsed with no tree-sitter syntax errors.
- All JSON and MSBuild/XML documents parsed.
- Markdown local links resolved.
- UTF-8, final-newline, NUL, and trailing-whitespace checks passed.
- No DLL, executable, PDB, MDB, local game path, or runtime fingerprint is present.
- Source and milestone packaging are validated by checksum and ZIP integrity checks during final packaging.

## Runtime status and open gates

The current environment does not include:

- Windows MSBuild;
- the .NET Framework 4.7 targeting pack;
- installed Kingmaker managed assemblies;
- installed Unity Mod Manager/Harmony12 assemblies;
- a running Kingmaker process.

Accordingly, this milestone does not claim:

- semantic compilation against the target APIs;
- successful UMM load or Harmony patching;
- actual blueprint registration;
- UMM panel rendering;
- feature grant or inventory mutation;
- nonproficient equip denial;
- proficient positive-path equip;
- save/load compatibility;
- a compiled or installable package.

The runtime-contract script and `TESTING.md` define the local build and in-game acceptance gates.

## Known architectural limitation

The Test Musket still inherits the Heavy Crossbow category. The item-level gate establishes firearm permission, but it does not yet prove that Firearm Proficiency replaces native Heavy Crossbow proficiency during attacks. The initial positive-path runtime test should use a martial-proficient unit. Sprint 8 will instrument the attack pipeline before a permanent category adapter or touch-AC rule is introduced.

## Scope explicitly excluded

- Touch AC or any combat result mutation.
- Ammunition or loaded state.
- Reload actions.
- Misfire, broken, or explosion behavior.
- Persistent per-item state.
- Gunslinger class content.
- Vendors, loot, starting equipment, or normal acquisition.
- Custom icons, models, audio, projectiles, or animations.

## Next sprint

Sprint 8 is bounded to read-only combat-pipeline instrumentation. It must observe firearm attacks and correlate AC, natural roll, distance, result, and attack source without changing any outcome.
