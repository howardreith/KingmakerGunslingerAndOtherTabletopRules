# Weapon Presentation Calibration Mission

## Objective

Qualify and correct only the cosmetic held and stored presentation of project
firearms, Elven branched spears, and Eastern weapons. The mission is complete
only after every production visual variant has a valid semantic frame, focused
and full automated validation passes, a clean installable package passes, and
guarded Steam runtime evidence covers the required visual matrix without a
mechanical regression.

Starting master is `7af4375238b2492857a131eefdf909b38a000a05` and the only
authorized mission branch is `codex/weapon-presentation-calibration`.

## Scope boundary

Permitted changes are presentation assets, semantic presentation markers,
presentation-only blueprint fields, gated diagnostics, runtime visual fixtures,
focused tests, package version surfaces at final qualification, and curated
mission evidence. Damage, range, reload rules, misfire rules, item economics,
class/feat mechanics, enhancement, blueprint identity, save compatibility, and
all other gameplay systems are protected.

Equipment roots remain identity transformed. Visible-model children receive
mesh-frame correction. Muzzle/projectile meaning is not moved to compensate for
visual placement. Native donor blueprints are never mutated globally. Held and
stored models are calibrated independently. Inherited attachment slots,
animation styles, trails, sounds, and timing are preserved unless exact engine
evidence proves a presentation-only replacement is required.

## Coordinate-frame contract

Each production visual must define a nondegenerate right-handed frame rather
than relying on a forward vector alone:

- Firearms: `Grip`, `Muzzle`, `Butt`, `WeaponUp`, and
  `SupportHandTarget` for long guns.
- Elven branched spears: `Grip`, physical `Tip`, `Butt`,
  `SupportHandTarget`, and `HeadUp` or an equivalent branch-facing axis.
- Eastern weapons: `Grip`, physical `Tip`, `Butt`/pommel, and
  `BladeNormal` (with support target where the donor uses one).

The source basis is derived from the normalized forward axis and an authored,
orthonormalized secondary axis. The target basis comes from a native donor
control. Translation then maps the transformed source grip to the donor grip.
Serialized Euler angles may be output, but they are not unexplained authority.

Validators reject missing, nonfinite, collinear, reflected, reversed,
renderer-disconnected, or incompatible held/stored frames, as well as
nonidentity equipment roots and invalid support-hand intervals.

## Runtime and evidence contract

All real launches use the guarded `-kmgRuntimeTestRequest` path and Steam App ID
640820. `KMG_AUTOMATION_BASELINE` is immutable. Only
`KMG_AUTOMATION_WORKING`, `KMG_COSMETIC_*`, and mission autosaves are
disposable. Screenshots are direct evidence for cosmetic claims; structured
assertions remain required for projectile direction, attack/reload execution,
blueprint identity, and other protected mechanics.

Every source checkpoint receives focused tests, repository validation, the
complete domain suite, a clean Release package build, package validation, and
the relevant guarded runtime scenario before commit. Every coherent commit is
pushed with the repository policy helper. No PR, merge, force-push, history
rewrite, or public release is authorized.

## Phase order

1. Baseline qualification and before evidence.
2. Semantic-frame and native-donor diagnostics.
3. Pistols and revolvers.
4. Muskets, blunderbusses, and rifles.
5. Elven branched spears.
6. Eastern held presentation.
7. Eastern stored presentation.
8. Integration coverage, final version/package, and complete runtime matrix.

Checkpoint completion is not mission completion. Work continues to the next
open row in `planning/WEAPON-PRESENTATION-MATRIX.md` unless a mission hard stop
remains after safe alternatives are exhausted.

## Current qualified scope

Presentation calibration is implemented for all 22 production variants.
Default-Medium-male guarded evidence currently covers held, independently
stored, family-appropriate ready/attack states, locomotion, body-relative
turning, and equip/unequip transitions. Eastern custom clones use their complete
independent stored prefabs without also recreating a donor sheath; native donor
blueprints retain their own sheaths unchanged. That repair is published at
`754ae076de0c02b5dd1e62691ba5905aa363432c` and clean-qualified by E5/V15/V16.
All seven production firearm variants now also have clean commit-bound Reload
Firearm evidence at `c0f193c1fa75741043b8be753bbc61ecb829bb9a`: each exact
production `UnitUseAbility` reaches its acted frame, 14 fixed samples cover the
full-round delivery window, six capacity-one transactions load exactly once,
no discharge occurs, and request-local state is restored exactly. The Advanced
Revolver reaches the same acted delivery but retains its pre-existing exact
fail-closed rollback because the current item-token carrier cannot represent a
six-round state; this cosmetic mission records and does not alter that
mechanical boundary. All four production handguns now additionally derive
their complete held frame from the exact native PiercingOneHanded Shortspear
control plus a measured donor-relative firing correction. Clean exact-commit
V18 at `e7e333c8da9f78064a2d99133004af4ef315515c` accepts combat-ready,
event-aligned firing, and both valid firearm/Shortsword dual-wield layouts on
the default Medium male. Every custom attack fired exactly once with no fault
or remaining round; the minimum acted physical-muzzle/target dot was
`0.9768526`, and equipment roots, grip anchors, projectile semantics, native
donors, and gameplay fields remained unchanged. Stored handgun disposition,
previously the last family-specific gap, is now explicitly hidden for all four
production variants by the exact live-item `ShowItem(bool)` lifecycle while
stored and restored by the native lifecycle while held or shown in the
inventory doll room. Clean exact-commit V19/V20 at
`d77db3711dcb9bffdc3a65c52d2c3f364392b093` prove the hidden/visible/hidden
equip round trip without renderer scanning, donor mutation, or gameplay
changes. Armor/cloak interaction and female, Small, and Enlarged fixtures
remain. Version remains `0.0.88` until those rows and the final package
qualify.
