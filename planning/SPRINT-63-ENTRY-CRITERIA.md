# Sprint 63 production fallback presentation entry criteria

## Authority

Mission 4.5 requires sensible fallback visuals, animations, sounds, and
projectiles where custom assets are unavailable. ADR-0007 explicitly authorizes
crossbow-compatible presentation for the core mod and keeps independently
replaceable custom assets optional. ADR-0013 requires isolated native blueprint
clones rather than native mutation.

## Observable contract

- Pistol and Advanced Revolver retain the exact installed Light Crossbow icon,
  equipment presentation, animation style, projectile sequence, model/sheath/
  belt references, attach-slot behavior, and sound fields.
- Musket, unavailable Blunderbuss, and Advanced Rifle retain the corresponding
  exact Heavy Crossbow presentation contract.
- Firearm marker/mechanics remain authoritative; inherited crossbow
  presentation never changes firearm identity, range, handedness, damage,
  criticals, proficiency, capacity, or availability.
- Each source blueprint and its presentation members remain unchanged.
- Missing, ambiguous, or structurally changed installed members fail closed.
- The package contains no custom model, animation, sound, projectile, icon, or
  proprietary asset. Player documentation clearly labels the fallback limits.

## Qualification

- Add a guarded, save-free, non-initiating observer that compares every named
  production firearm against its exact Light/Heavy Crossbow source and records
  nonempty projectile and readable icon evidence.
- Repository validation, complete 831-test suite, clean exact-reference Release
  build, strict package validation, request/preflight tests, and staged safety
  audits must pass.
- Exact mod-load PASS and two independent fresh-process observer PASS runs are
  required before the row becomes runtime-qualified.

## Non-goals

No custom art or asset bundle, firearm-shaped 3D model, new sound, animation
controller, dual-wield polish, scatter enablement, mechanical change, or
proprietary asset redistribution is authorized.
