# ADR-0014 — Use a dedicated feature and item-level equipment restriction for firearm proficiency

- **Status:** Accepted for Sprint 7
- **Date:** 2026-07-12

## Context

Kingmaker has no native firearm proficiency. The Test Musket temporarily reuses the Heavy Crossbow category for engine compatibility, but firearm identity and permission must not be inferred from that category.

The first development build needs a deterministic way to distinguish proficient and nonproficient characters without changing native Heavy Crossbows or adding the Gunslinger class prematurely.

Options considered:

1. Treat Heavy Crossbow proficiency as firearm proficiency.
2. Grant Heavy Crossbow proficiency together with a cosmetic firearm feature.
3. Apply an attack-roll penalty through a future rule handler.
4. Create a dedicated Firearm Proficiency feature and require it through an item-level `EquipmentRestriction`.

## Decision

Create one stable, hidden `BlueprintFeature` named `KMG.Firearms.FirearmProficiency` and append one custom `EquipmentRestriction` to each firearm item blueprint.

For Sprint 7, the restriction denies equipping a firearm unless `UnitDescriptor.GetFeature(requiredProficiency)` returns a fact.

The Heavy Crossbow category remains an implementation adapter only. Native Heavy Crossbow blueprints are not modified, and ordinary Heavy Crossbow proficiency is not granted by the custom feature.

## Consequences

### Positive

- Firearm permission is explicit and stable.
- The same feature can later be granted by Gunslinger levels, feats, archetypes, or NPC templates.
- Native crossbows remain unchanged.
- The restriction is local to firearm items and easy to verify.
- A nonproficient negative path is deterministic before combat-rule patches exist.
- No global enum or category mutation is required.

### Negative

- Equip denial is stricter than tabletop nonproficiency penalties.
- The inherited Heavy Crossbow category may still impose its own attack proficiency behavior after the custom gate passes.
- Each firearm item must carry the restriction or be produced by a factory that guarantees it.
- The feature is hidden and development-only until localization and normal grant paths exist.

## Rejected alternatives

### Heavy Crossbow proficiency is firearm proficiency

Rejected because it makes every qualifying Heavy Crossbow user a firearm user and entangles future firearm feats with unrelated crossbows.

### Grant native Heavy Crossbow proficiency

Rejected because it leaks proficiency to ordinary Heavy Crossbows and makes removal/multiclass behavior difficult to reason about.

### Attack-roll penalty only

Deferred. It requires a combat-pipeline hook that Sprint 7 intentionally does not introduce. It would also allow characters to equip a development firearm before firearm permission can be isolated and tested.

### New runtime weapon category

Rejected because `WeaponCategory` is a compiled enum and inventing values is unsafe for serialization, UI, feats, and save compatibility.

## Follow-up

- Sprint 8 instruments attack and proficiency behavior.
- A later adapter must ensure Firearm Proficiency, rather than Heavy Crossbow proficiency, governs firearm attacks.
- Player-facing localization, icon, descriptions, and normal grant paths arrive with class/feat content.
- Every future firearm item factory must validate exactly one firearm definition marker and exactly one proficiency restriction.
