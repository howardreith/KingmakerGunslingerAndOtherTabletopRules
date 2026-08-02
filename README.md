# Kingmaker Gunslinger

Version `0.0.31-s31-early-firearm-catalog` develops the first production early-firearm catalog for Pathfinder: Kingmaker 2.1.7b from the runtime-qualified Sprint 30 baseline.

## Current vertical slice

The build provides the complete base Gunslinger progression, production early
and advanced firearms, stackable Black Powder Charges and Lead Balls, atomic
component consumption, range-limited touch AC, exact item-owned firearm state,
loaded-round enforcement, misfire condition transitions, and same-item
maintenance. Historical Test Musket fixtures remain development-only.

The retained Test Musket diagnostic fixture has one round, a 40-foot range
increment, natural 1–2 misfire, full-round reload requiring a free hand, and a
5-foot misfire burst. It is not distributed as production equipment.

A first misfire consumes the loaded round, forces a miss, and changes only the exact firearm from Normal to Broken. A second misfire from Broken changes the exact firearm to Wrecked and resolves a native Reflex DC 12 plus base weapon-damage burst against every unique qualified unit in five feet, with the exact wielder included once and last.

## Complete maintenance loop

Firearm Proficiency now grants three separate full-round abilities:

```text
Overhaul Firearm: empty/Wrecked + one Repair Kit → empty/Broken
Repair Firearm:   empty/Broken + one Repair Kit → empty/Normal
Reload Firearm:   empty + powder + Lead Ball → loaded
```

Overhaul and Repair are distinct personal extraordinary actions. Each mutates
only during completed delivery, consumes exactly one Firearm Repair Kit,
preserves the same exact runtime item and item-owned state token, and creates no
ammunition. Repair rejects Wrecked, Normal, or loaded Broken firearms without
mutation.

Reload remains a separate full-round operation and is the only maintenance-loop step that consumes Black Powder and a Lead Ball.

## Accelerated qualification harness

Sprint 29 adds a deterministic development fixture and PASS/FAIL matrix. It prepares one exact equipped Test Musket as empty/Wrecked, preserves or creates a second independent empty/Normal Test Musket, ensures two Repair Kits plus one powder-and-ball pair, captures process-local identities and counters, and validates each checkpoint:

```text
FixtureReady → OverhaulPassed → RepairPassed → MaintenanceLoopPassed
```

A one-command immediate diagnostic runs the entire transaction loop without action economy for fast regression checks. The action-bar abilities must still be tested separately for real full-round delivery and interruption behavior.

The item-owned inert `BlueprintWeaponEnchantment` token remains the authoritative state carrier. The rejected `ItemEntityWeapon.UniqueId` vault is not used.

## Installation

Install only the standalone Unity Mod Manager ZIP. Do not install the source archive, complete milestone archive, private reference bundle, compiler package, or framework reference assemblies.

Read `INSTALLATION-COMPATIBILITY.md` before installing, updating, removing, or
using this mod with other gameplay mods. In particular, back up saves before
updates and do not remove the mod from a campaign that has used its content;
there is no uninstall-safe-save claim. `SMOKE-TEST-GUIDE.md` remains the
mechanical diagnostic guide.

## Production equipment and fallback presentation

The production Pistol, Musket, Advanced Rifle, and Advanced Revolver are
available from the qualified capital vendor route alongside powder, lead balls,
and repair kits. Blunderbuss remains unavailable until its numeric scatter-cone
distance is authorized and runtime-qualified.

The core package intentionally uses installed crossbow-compatible fallback
assets under ADR-0007. Pistol/Revolver use Light Crossbow presentation;
Musket/Blunderbuss/Rifle use Heavy Crossbow presentation. Their icons, models,
animations, sounds, equipment attachment behavior, and projectiles therefore
look and sound like crossbows. No custom firearm art, audio, animation, model,
or projectile asset is bundled.

## Direction after Sprint 29

Reload, Overhaul, and Repair use one marker-first exact-equipped-firearm context
and definition-driven policy. Stable historical symbols and compatibility
adapter type names are retained for save and code compatibility; their visible
abilities are production-generic.

## Deliberate deferrals

Custom firearm assets, authorized numeric scatter delivery, crafting, magical
firearms, firearm-using enemies, and dual-wield presentation polish remain
outside the current qualified build.
