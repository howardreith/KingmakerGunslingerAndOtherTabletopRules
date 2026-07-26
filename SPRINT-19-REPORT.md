# Sprint 19 report — proficiency hotfix and item-token persistence probe

## Runtime evidence consumed

The first real Kingmaker run established that version 0.0.18 installed, loaded, rendered its UMM panel and registered all eight expected blueprints exactly once. The trusted preflight recorded I01 as PASS. It also recorded I02 as FAIL because `ItemEntityWeapon` exposes no inherited member named `UniqueId` in Kingmaker 2.1.7b.

The identity-keyed UnitPart carrier is therefore rejected rather than patched around with an unstable fallback.

## Corrections

- Replaced the reflection-heavy selected-unit path with the exact Kingmaker `SelectionManager.Instance` / `GetSingleSelectedUnit()` contract and a main-character fallback.
- Grants Firearm Proficiency through the exact `UnitDescriptor.Progression.Features.AddFeature(feature, null)` API and verifies the resulting rank.
- Proficiency equipment checks now query `Progression.Features.HasFact` directly.
- Moved the last development-command result to the top of the UMM panel.
- Clarified that the Test Musket still displays as a Heavy Crossbow placeholder.

## Persistence pivot

- Disabled the engine-item-identity UnitPart vault as the active state carrier.
- Activated `TokenBackedFirearmStateRepository`.
- Encodes non-default firearm state as exactly one passive `BlueprintWeaponEnchantment` on the exact item.
- Uses absence of a token for canonical Empty / Normal state.
- Makes dynamic enchantment creation explicit against Kingmaker's `AddEnchantment(blueprint, null, null)` contract.
- Reworked the A-D fixture to verify three item-owned tokens and one token-free firearm without inventing a fake item identity.

## Build evidence

- Compiled against the user-exported Kingmaker 2.1.7b / UMM 0.32.4 reference bundle.
- .NET Framework 4.7 reference surface.
- C# 7.3.
- Release optimization.
- Warnings as errors.
- Compiler exit code 0 and empty stderr.
- 373 dependency-free tests executed three times; 0 failures; output byte-identical.

## Remaining gate

The item-token carrier remains provisional until a real Kingmaker save, complete process restart and reload preserve the four A-D states without loss, duplication or transfer.
