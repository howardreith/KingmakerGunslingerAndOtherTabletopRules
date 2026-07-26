# Sprint 27 report — item lifecycle and same-item recovery contract

Version: `0.0.27-s27-item-lifecycle-recovery-contract`  
Target: Pathfinder: Kingmaker 2.1.7b / UMM 0.32.4 / Harmony 1.2.0.1 / .NET Framework 4.7 / C# 7.3

## Entry decision

Sprint 26 is runtime-accepted from the supplied native-burst evidence and the user's explicit item-isolation confirmation. The evidence proves one five-foot native query, two unique planned/applied targets, independent Reflex saves and damage events, exact-wielder inclusion, empty/Wrecked final state, deduplication, and zero relevant faults. The later disappearance of the two inventory Test Muskets was consistent with the destructive cleanup diagnostic, not the explosion path; Sprint 27 hardens that control with a two-step confirmation.

## Exact Kingmaker contract findings

Inspection of the exact private 2.1.7b assembly established:

- `ItemsCollection.Remove(ItemEntity)` delegates to count-aware removal.
- Full removal calls `Extract`, clears `Collection`, removes the item from `HoldingSlot` through `ItemSlot.RemoveItem(true)`, updates slot indexing, and raises collection-removal bookkeeping.
- `ItemEntity.Dispose()` only disposes the item's enchantment collection and is not a safe inventory/equipment destruction API.
- `ItemsCollection.Add(BlueprintItem)` and native `ItemSwitch.RunAction()` create new `ItemEntity` instances through `ItemsEntityFactory.CreateEntity`.
- `ItemSwitch` may then remove the replaced item, so native replacement does not preserve exact runtime identity.
- No item-condition `Repair`, `Mending`, `MakeWhole`, or `Make Whole` type/method contract was found.
- `ItemRestoreValue` restores blueprint counts by adding items and is not same-item condition repair.

The exact method signatures, tokens, IL excerpts, assembly hash, and checksum index are retained under `evidence/sprint27-contracts/`.

## Decision

Sprint 27 does not automatically remove or replace a firearm after its second misfire.

The exact item remains empty/Wrecked. The bounded recovery candidate is a staged, same-item path:

```text
Wrecked --overhaul--> Broken --ordinary repair--> Normal
```

Only the first boundary is added for runtime contract testing. Player-facing cost, time, skill, action delivery, and ordinary repair gameplay remain deferred.

## Implementation

- Added `FirearmStateMachine.OverhaulWrecked`.
- The transition accepts only Wrecked state and returns empty/Broken.
- Added typed `NotWrecked` rejection.
- Added a development bridge command for the first exact equipped Wrecked firearm.
- The bridge verifies unchanged repository identity and runtime reference hash, exactly one revision increment, empty final load, and Broken final condition.
- The command removes no item, creates no replacement, grants no ammunition, consumes no resource, and does not silently repair to Normal.
- Replaced the one-click “remove all unequipped Test Muskets” control with arm/confirm/cancel controls and an explicit destructive warning.
- Retained all accepted reload, discharge, natural-roll, condition, burst, persistence, AC, proficiency, and ammunition behavior.
- Retained item-owned inert `BlueprintWeaponEnchantment` state tokens and did not revive the rejected `ItemEntityWeapon.UniqueId` vault.

## Dependency-free coverage

Three cases were added:

- Wrecked to empty/Broken succeeds without mutating the original immutable state or manufacturing ammunition.
- Normal state is rejected with `NotWrecked`.
- Broken state is rejected with `NotWrecked`.

The complete suite now declares 543 tests.

## Deliberate deferrals

Sprint 27 does not add automatic item destruction, player-facing repair, repair cost, repair resources, repair skill checks, Gunsmithing, Quick Clear, make whole, Rapid Reload, scatter triple damage, additional firearm blueprints, custom assets, magical firearms, vendors, crafting, grit, deeds, class progression, or enemy firearm AI.

## Qualification

**READY FOR KINGMAKER — Sprint 27 item-lifecycle recovery-contract smoke test**

- Exact Kingmaker 2.1.7b private-reference Release compile: passed.
- .NET Framework 4.7 / C# 7.3 / AnyCPU: passed.
- Warnings as errors: enabled and passed.
- Same-output-path compile runs: 2.
- DLL and PDB outputs: byte-identical.
- Dependency-free suite: 543 tests × 3 runs, 0 failures.
- Repeated test output: byte-identical.
- Standalone UMM package: 8 entries and exactly one project-owned binary.
- Private Kingmaker, Unity, UMM, Harmony, Newtonsoft, compiler, and framework assemblies redistributed: none.

Authoritative hashes:

```text
KingmakerGunslinger.dll
815f5425c777f05ee397bcb79dfa04e43e3ff4f4688a114a35cfca56e824ee96

KingmakerGunslinger.pdb
0912466136147d54f08d3c1178fd57408883e5e75736228b115928b03b68463b

Repeated test output
530bad4a96f1fd6f75c038becb39e317c62b7e824242090164d04a4271629f3a

Standalone UMM ZIP
9e00375d5144e87ea30505efdb9c639faedeb2b9f7d6f479bdaf8de3d9644a21
```


## Runtime status

The exact 0.0.27 standalone package must prove the same-item overhaul, second-item isolation, persistence, and cleanup-confirmation controls before Sprint 28 begins.
