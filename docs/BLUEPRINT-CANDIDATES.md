# Vanilla Blueprint Candidate Plan

## Rule

No Wrath GUID and no remembered Kingmaker GUID is accepted as a dependency without verification against the final Kingmaker 2.1.7b blueprint library.

Sprint 1 identifies **roles**, not unverified identifiers.

## Candidate roles for the first vertical slice

| Role | Desired native behavior | Why needed |
|---|---|---|
| Heavy crossbow `BlueprintWeaponType` | Two-handed ranged weapon, crossbow animation style, projectile support | Primary Test Musket clone candidate |
| Standard heavy crossbow `BlueprintItemWeapon` | Spawnable/equippable item with working visuals | Item shell and inventory behavior |
| Light or hand-crossbow-like weapon type, if present | One-handed or compact presentation | Future pistol comparison |
| Crossbow proficiency feature/restriction | Example of weapon-use gating | Firearm proficiency construction |
| Crossbow projectile or visual parameters | Known ranged launch behavior | Placeholder shot presentation |
| Standard-action item/extraordinary ability | Reliable combat action and targeting | Reload action prototype |
| Stackable mundane inventory item | Stable count, transfer, vendor, save behavior | Powder and bullet base |
| Invisible feature or harmless feature | No combat side effects | Initialization proof |
| Weapon enchantment blueprint | Item-attached event component | Touch-AC attachment prototype |

Merchant tables, class roots, feat lists, and starting-equipment lists are deliberately deferred until their milestone.

## Discovery procedure

### Static inspection

Using the installed final game's assemblies and a decompiler:

1. Confirm relevant enum members and their serialized integer values.
2. Locate the blueprint library and registration methods.
3. Confirm the exact signatures of rule events.
4. Inspect item entity serialization fields and extension points.
5. Confirm whether a custom item part can be serialized.
6. Confirm inventory item identity fields and copy/clone behavior.

### Runtime diagnostic

A development-only diagnostic should enumerate candidate blueprints and log:

```text
GUID
asset name
runtime type
weapon category
fighter group
is ranged
is two-handed
range
damage dice
critical profile
animation style
visual prototype
projectile blueprint(s)
components
enchantments
proficiency restriction
```

Logging should be filtered by expected runtime type and names containing `Crossbow`, not dump the entire blueprint database by default.

### Selection criteria

A clone candidate is accepted only if:

- it exists on the target build;
- its runtime type is exactly the expected type;
- it equips and attacks without a new asset bundle;
- its projectile and animation references do not null;
- cloning does not mutate the original;
- the selected category does not create unacceptable feat or UI leakage;
- the blueprint can be located deterministically across supported storefronts.

## Category verification

Before choosing `HandCrossbow`, answer:

1. Does `WeaponCategory.HandCrossbow` exist in Kingmaker 2.1.7b?
2. Does any native item or blueprint use it?
3. Does character animation support it?
4. Which feats and selectors enumerate it?
5. Does the equipment UI classify it correctly?
6. Does it serialize without coercion?
7. Is it treated as one-handed, ranged, or light in all relevant rules?

If any answer is unsafe, use a verified crossbow category only as a low-level shell and gate every firearm rule with the custom marker.

## Custom reservations

`blueprints/blueprints.json` reserves the first custom IDs. The `LoadedStateToken` reservation is diagnostic: it allows an enchantment persistence experiment but does not decide the final state mechanism.

## Foundation/Sprint 4 output expected

The first runtime diagnostic package should produce:

```text
diagnostics/environment.json
diagnostics/assembly-hashes.txt
diagnostics/weapon-categories.txt
diagnostics/crossbow-candidates.json
diagnostics/item-serialization-notes.md
```

Until those exist, the package must not publish a Kingmaker clone GUID as fact.
