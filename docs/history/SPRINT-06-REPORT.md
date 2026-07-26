# Sprint 6 completion report — Test Musket blueprints

**Milestone:** `0.0.6-s06-test-musket`
**Date:** 2026-07-12
**Artifact class:** source milestone; not a Unity Mod Manager install package

## Goal

Register a clone-derived Test Musket weapon type and item, attach the Sprint 5 firearm-definition marker, preserve the native Heavy Crossbow assets, and introduce no later firearm mechanics or player acquisition path.

## Delivered behavior

At Kingmaker's one-time blueprint lifecycle callback, the mod now:

1. validates the checked-in blueprint manifest;
2. performs the Sprint 5 marker round-trip probe;
3. registers the hidden diagnostic feature;
4. resolves native Heavy Crossbow source blueprints by fixed game GUID;
5. confirms the source blueprints have the exact expected runtime types;
6. discovers a readable/writable `BlueprintItemWeapon` member whose value is `BlueprintWeaponType`;
7. clones the native weapon type with `UnityEngine.Object.Instantiate`;
8. appends exactly one named `FirearmDefinitionComponent` to the clone;
9. registers the custom Test Musket weapon type under its stable manifest GUID;
10. clones the native Standard Heavy Crossbow item;
11. rewires only the cloned item to the custom weapon type;
12. registers the custom Test Musket item under its stable manifest GUID;
13. validates marker round-trip, clone identities, internal names, item/type relation, and source immutability;
14. commits only if all three custom registrations are present.

Any exception after registration begins invokes best-effort reverse-order rollback of every blueprint registered by that initialization transaction. A rollback failure is logged separately without hiding the original initialization failure.

## Stable identifiers activated

| Symbol | GUID | Type |
|---|---|---|
| `KMG.Diagnostic.InitializedFeature` | `6294cc6964914ea7bf450d5ef82fadde` | `BlueprintFeature` |
| `KMG.Test.TestMusketWeaponType` | `6e499550b44c41b3a1ef0693904a46b8` | `BlueprintWeaponType` |
| `KMG.Test.TestMusketItem` | `09641295ceea4c558400c43df2ddf1f9` | `BlueprintItemWeapon` |

All six other manifest IDs remain reserved and unchanged.

## Native clone sources

| Role | GUID |
|---|---|
| Heavy Crossbow weapon type | `36d0551b8a28587438a47fcbbf53c083` |
| Standard Heavy Crossbow item | `19a5092244dcf99478dcd73c974828b1` |

These source IDs are validated at runtime. Missing IDs, null assets, derived/unexpected runtime types, or an unexpected item/type relationship fail initialization before the Test Musket is accepted.

## Test Musket firearm definition

```text
Era: Early
Kind: Musket
Capacity: 1
Range increment: 40 feet
Misfire value: 2 (natural 1–2 once misfire rules exist)
Base reload: FullRound
Requires free hand: true
Rounds per reload action: 1
Scatter: false
```

The definition is passive metadata in this sprint. It does not yet change attacks.

## Scope controls

The source contains no:

- inventory insertion;
- vendor or loot registration;
- starting-equipment registration;
- character-creation reference;
- proficiency feature;
- touch-AC rule handler;
- ammunition or reload action;
- misfire or broken-state handler;
- player-visible debug control;
- mutable per-item firearm state.

## Validation performed in this environment

- C# syntax parsing for all project and test files.
- PowerShell syntax parsing for all scripts.
- JSON and XML parsing.
- JSON Schema validation of the blueprint manifest.
- Stable GUID continuity against Sprint 5.
- Three-active-blueprint manifest checks.
- Independent modeled tests for lookup, clone mutation isolation, marker cardinality, item rewiring, source preservation, collision failure, and reverse rollback.
- Static source-boundary checks for forbidden acquisition and future-mechanic tokens.
- Package binary/proprietary-file scan.
- ZIP CRC, path traversal, single-root, and SHA-256 verification.

The dependency-free C# test project now declares 40 tests. It could not be executed here because no .NET Framework compiler/runtime toolchain is installed.

## Runtime status

**Not READY FOR KINGMAKER.** No compiled DLL or UMM-installable ZIP is included. Runtime compilation and in-game execution remain unverified because the required installed Kingmaker/UMM assemblies and Windows build environment are unavailable here.
