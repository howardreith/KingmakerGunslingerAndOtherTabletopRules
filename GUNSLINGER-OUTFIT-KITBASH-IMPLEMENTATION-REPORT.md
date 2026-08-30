# Gunslinger Outfit Kitbash Implementation Report

Status: provisional native finalist selected; production implementation is
intentionally unchanged pending exhaustive qualification.

## Intake

- Baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`
- Branch: `codex/gunslinger-class-outfit-kitbash`
- Intake version: `0.0.110`
- Installed-game target: Pathfinder: Kingmaker 2.1.7

## Verified current cause

The Gunslinger class registration still resolves the native Fighter blueprint,
and its creation path copies Fighter male, female, shared clothing, and default
color presentation fields. This explains the rejected generic Fighter
appearance. Exact assignment and array-aliasing evidence will be preserved in
the focused test baseline before replacement.

## Investigation

The installed 2.1.7b class-clothing path is now verified. Shared
`KingmakerEquipmentEntity` wrappers resolve gender/race-specific
`EquipmentEntityLink` values, followed by direct gender-specific class links.
Loaded `EquipmentEntity` records expose layer, body/outfit parts, special
cloak/backpack treatment, hiding flags, lower-material behavior, color
profiles, and primary/secondary ramps. Public avatar operations cover
add/remove, ramp application, rebuild, and saved-equipment restoration.

The guarded `gunslinger-outfit-audit` scenario now inventories the live class,
item-linked, and bounded raw resource streams without touching save-owned
state. Passing run `20260830T2012181937219Z` resolved 4,878/4,878 links across
3,816 class/item matrix rows and nine discovered player-race IDs for both
genders. It loaded 1,206 unique entities with zero inspection errors.

Deterministic evidence identities:

- candidate set:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`;
- ignored catalog:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.

Installed class-link evidence makes Bard, Alchemist, Magus, Ranger, Rogue,
Slayer, and Inquisitor concrete render donors. Ranger, Rogue, Slayer, and
Inquisitor default cap links structurally hide hair and/or ears. The guarded
renderer freezes 32 audited, gender-specific links for six serious
presentations while excluding Ranger/Rogue/Slayer caps and Ranger/Rogue capes.
These are investigation-only IDs; no production asset identifier is approved.

The renderer captures preview-like four-view contact sheets and elevated
isometric views for male and female Humans in native-default/no-weapon,
native-default/pistol, native-default/musket, and alternate-valid-ramp
no-weapon cases. It snapshots, restores, and verifies entity order, both ramp
arrays, and saved links around every candidate, then verifies request-local
cleanup. It cannot write a save. A disposable blueprint may omit the optional
live Progression equipment-class field, so the renderer preserves it when
present and otherwise resolves the exact audited native Fighter class already
used by production; both the source and the actual avatar/link intersection
are recorded.

The first complete installed-game batch rendered all 48 Human cases and 96
images for the six coherent native presentations. Direct inspection places
Magus complete first at 81/100, Rogue capless/capeless second at 75/100, and
Slayer capless third at 70/100. Bard, Alchemist, and Ranger remain outside the
shortlist because their dominant pack/apron/bedroll silhouettes do not satisfy
the Gunslinger brief. These are visual findings from native renders, not
resource-name inference.

## Selected appearance

Provisional finalist: coherent native Magus base plus its one native accessory.

- male: `EE_Magus_M_Any_Colorize`
  (`6df8f61725a84294c8661bb9585eca97`) plus
  `EE_MagusAccesories_M_Colorize`
  (`4c59d2b9740930145a27a4c693217d22`);
- female: `EE_Magus_F_Any_Colorize`
  (`beba0e0c7dcd5c64d97d767be3e72995`) plus
  `EE_MagusAccesories_F_Colorize`
  (`a93ead19aae8afc4794c54f5bcf73168`);
- guarded native defaults: primary 2, secondary 22; both entities expose
  35 primary and 35 secondary ramps.

The fitted torso, split waist tails, layered belts, bracers, and boots read as
a Golarion swashbuckler/privateer without a literal cowboy or pirate hat. The
small arcane color accents and pending full compatibility matrix account for
withheld points. This is not yet a production binding.

## Production changes

Pending evidence-based selection.

## Tests, build, package, and runtime

The audit checkpoint passes repository source validation, all 1,362
domain/reflection tests (including guarded boundary, deterministic/read-only
inventory, and evidence-manifest preservation), Release compilation, strict
standalone UMM package validation, and its Steam-backed guarded runtime
scenario. The first two runtime iterations failed closed and improved the
instrumentation; the third passed all nine assertions with no exceptions.

These checks prove the reusable audit and native catalog only. Candidate
rendering, aesthetic scoring, production binding, full compatibility
qualification, final clean build/package, and release runtime proof remain
pending. A build or domain-test pass is not visual proof.

The subsequent guarded-render checkpoint passes 160 runtime preflight checks,
repository validation, all 1,365 domain/reflection tests, exact-reference
Release compilation, deterministic package construction, and strict validation
of both standalone and local-runtime packages. Both packages have SHA-256
693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
the DLL SHA-256 is
17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
The complete Human render at
`20260830T2130124467293Z-gunslinger-outfit-candidate-render` passed its
in-game result with 48 exact records, 96 images, 12/12 restorations, no save
API, and no production mutation. Its generic outer 120-second collector
expired before rendering began, although the safe live scenario later passed
and exited automatically. A scenario-only 600-second collector ceiling and
focused regression assertion now prevent that orchestration mismatch. All
1365/1365 Release domain tests pass after the repair. An accepting rerun, full
finalist matrix, production binding, and final clean validation remain
required.
The subsequent full local gate passed repository validation, all 1365 tests,
exact-reference Release construction, deterministic packaging, and strict
standalone/local validation. Both packages have SHA-256
`2f515302e2d0263adccb837b4e4f079d1120fcb0074054fae9ba4093aef76849`;
the DLL SHA-256 is
`7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`.
Quiescent runtime preflight passes all 160 checks.

The accepting rerun at
`20260830T2158516580621Z-gunslinger-outfit-candidate-render` loaded exact
commit `8f47f2db723fdfe6146ca30c352ea83ba7d3589f` and passed both outer
orchestration and all 10 in-game assertions. It reproduced the exact candidate
set with 48 records, 96 images, 48 exact held states, 12/12 restorations, no
save API, no production mutation, and automatic process exit. Direct review
of all accepted images preserved the Human-stage ranking. The Human renderer
gate is closed; the exhaustive finalist matrix remains open.

## Uncertainty

The supplied external mission-package path was absent at intake. A
manifest-matching pre-existing untracked package was inspected provisionally
without modifying or publishing it. The path discrepancy remains explicit.
Eleven ordinary-isometric images in the first batch were tagged low pixel
density; the accepted rerun reduced that to eight with zero low-density
preview captures. Paired preview-like images were directly usable, but final
evidence must improve or explicitly retain that limitation.
