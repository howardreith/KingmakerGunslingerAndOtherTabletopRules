# Sprint 63 production fallback presentation qualification

ADR-0007 authorizes the core mod to ship with crossbow-compatible fallback
presentation while custom firearm assets remain optional and independently
replaceable. Sprint 63 verifies that contract exactly rather than treating clone
ancestry as proof.

## Qualified mapping

- Pistol and Advanced Revolver inherit Standard Light Crossbow presentation.
- Musket, unavailable Blunderbuss, and Advanced Rifle inherit Standard Heavy
  Crossbow presentation.

The observer compares item icon, item hand-visual parameters, equipment entity
and alternatives, inventory sounds, type icon, projectile sequence, weapon
animation style, special animation, model/belt/sheath references, attach-slot
behavior, reach FX threshold, weapon sound size/type, miss/whoosh/equip/unequip
sounds, and visual inventory sounds. It does not instantiate an asset, animate
a unit, fire a projectile, load a save, or mutate a blueprint.

## Qualification evidence

Exact observer commit `82264b4` passed 831/831 tests, clean exact-reference
Release build, strict package validation, 38 request checks, 82 preflight
checks, and guarded mod load at
`20260802T0946237925580Z-mod-load-smoke`.

Independent fresh-process PASS runs:

- `20260802T0947439357619Z-observe-production-firearm-fallbacks`
- `20260802T0949087984758Z-observe-production-firearm-fallbacks`

Both runs observed all five mappings as exact, one inherited projectile per
firearm, and a non-null inherited icon. No save or gameplay state was touched.
Package/DLL SHA-256 are
`99dc424e24ecf853a1cc24ec87f938a11ff30cc433d02bf49089bc2391d79908` /
`ab541c2b01dff4d272cde383f3f13175e35294a64e90b40c73bf84a4ab710598`.

## Player-visible limitation

These are intentionally recognizable crossbow models, animations, sounds, and
projectiles. The core package contains no custom firearm model, icon, sound,
animation controller, or projectile asset. This is an accepted release fallback
under ADR-0007, not evidence that the inherited visuals depict firearms.
