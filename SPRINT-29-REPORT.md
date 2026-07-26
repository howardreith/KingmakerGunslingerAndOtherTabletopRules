# Sprint 29 report — complete maintenance loop and qualification automation

## Entry decision

Sprint 28 was accepted from the supplied Kingmaker evidence. The player-facing full-round Overhaul action was available only for an exact equipped Wrecked Test Musket with a Firearm Repair Kit, interruption consumed nothing, completed delivery preserved exact item identity while changing empty/Wrecked to empty/Broken, one kit was consumed, repeat use was rejected, Reload became available, and the state persisted across save/load without reported faults.

Sprint 29 entry was therefore approved.

## Delivered vertical slice

Sprint 29 completes the Test Musket maintenance sequence:

```text
empty/Wrecked
  -- full-round Overhaul + one Repair Kit -->
empty/Broken
  -- full-round Repair + one Repair Kit -->
empty/Normal
  -- full-round Reload + powder + Lead Ball -->
loaded/Normal
```

The new `Repair Test Musket` ability is personal, extraordinary, full-round, and granted by Firearm Proficiency alongside Reload and Overhaul.

Repair resolves only for exactly one equipped empty/Broken Test Musket with at least one Firearm Repair Kit. It consumes the kit only during completed delivery, changes the exact same item to empty/Normal, advances its repository revision once, and creates no ammunition. Normal, Wrecked, loaded Broken, missing-kit, missing-inventory, and ambiguous equipped-target cases fail without mutation.

The transaction verifies the exact inventory and item writes. A later failure attempts to restore both the pre-operation item state and Repair Kit count, and rollback refuses to overwrite an unexpected concurrent state.

## Qualification automation

A deterministic process-local fixture now:

- targets exactly one equipped Test Musket;
- creates or preserves a second independent visible Test Musket;
- normalizes the target to empty/Wrecked;
- normalizes the second item to empty/Normal;
- ensures two Repair Kits plus one powder-and-ball pair;
- captures exact item identities, revisions, state, resources, completion counters, fault totals, and duplicate totals; and
- prints a concise PASS/FAIL matrix for `FixtureReady`, `OverhaulPassed`, `RepairPassed`, and `MaintenanceLoopPassed`.

A one-command immediate diagnostic executes the complete transaction loop and records every checkpoint. It deliberately bypasses action economy. Manual action-bar testing remains required for full-round timing, interruption, and presentation.

## Preserved boundaries

Sprint 29 retains:

- the item-owned inert `BlueprintWeaponEnchantment` state carrier;
- exact marked-firearm isolation from native Heavy Crossbows;
- condition-preserving Broken reload;
- loaded-round consumption and empty/Wrecked attack rejection;
- natural 1–2 misfire detection and deterministic forced rolls;
- Normal-to-Broken and Broken-to-Wrecked exact-item transitions;
- native Reflex and damage resolution for the five-foot second-misfire burst;
- exact same-item Wrecked-to-Broken Overhaul;
- two-step destructive cleanup confirmation; and
- process-local diagnostics without using them as persistence state.

The rejected `ItemEntityWeapon.UniqueId` vault was not revived.

## Deliberate exclusions

Sprint 29 does not add generic definition-driven action selection, new firearm types, scatter attacks, advanced capacities, Rapid Reload, Quick Clear, Gunslinger class progression, grit, deeds, vendors, crafting, production assets, magical firearms, or firearm-using enemies.

Those remain bounded to later vertical slices, beginning with the generic action runtime in Sprint 30.

## Qualification discipline

The final candidate is required to pass:

- exact Kingmaker 2.1.7b private-reference Release compilation twice;
- C# 7.3, .NET Framework 4.7, AnyCPU, and warnings as errors;
- byte-identical DLL and PDB output across same-output-path builds;
- the complete dependency-free test suite three times with zero failures and byte-identical output;
- strict standalone UMM package validation with exactly eight entries and one project-owned binary;
- private-reference redistribution audit;
- authoritative source archive validation; and
- complete milestone archive checksum validation.

Runtime acceptance remains pending until the exact standalone 0.0.29 package passes `SMOKE-TEST-GUIDE-0.0.29.md`.
