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

The finalist race/gender matrix is now implemented behind the exact guarded
`gunslinger-outfit-finalist-race-matrix` request. It dynamically derives the
installed player-race catalog, selects native same-race/same-gender body
donors, validates the exact ordered Magus pair through native
`BlueprintCharacterClass.LoadClothes`, samples two proven-valid palettes,
captures both required framings without a weapon, and restores entity order,
ramps, and saved links before disposing each actor. It is request-local,
save-free, and production-mutation-free. It expects 18 fixtures, 36 records,
72 images, and 18 restorations.

Repository validation, installed-reference Release compilation, all 1365
tests, clean Release construction, standalone/local package validation, and
163 quiescent runtime-preflight checks pass. The current local-runtime package
SHA-256 is
`cdd85e981f9847b0259a965506db457af98818d25aaf7c87d619022eae9559dc`;
the DLL SHA-256 is
`36cf201fca3040c3a7b9a35f4253207d87b5480b3f13b1df14897860fdb02b7b`.
This closes only the source/package gate. Installed-game race-grid rendering,
direct image review, overlays, motion, production binding, and persistence
remain open.

The first installed finalist-matrix attempt at
`20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix` loaded exact
published commit `fe86bce4484d45ca8f6a6f7070bfd7942fd5a0fc`. It safely
completed both palettes for male Aasimar, then stopped before applying the
outfit to female Aasimar because the selected native donor's original avatar
state was not exactly restorable. Guard, working-save boundary, game identity,
cleanup, no-save behavior, no-production-mutation behavior, and automatic exit
all passed. The two partial records/four images do not qualify the race grid or
change the score.

The repair treats native body blueprints as deterministic candidates rather
than assumed-safe fixtures. It filters each donor to canonical player-race
size (Small for Gnome/Halfling, Medium otherwise), preserves all exact
race/gender/size matches, and runs a full entity-order/ramp/saved-link
round-trip before acceptance. Failed disposable actors are recorded and
retired before trying the next candidate. Every accepted fixture carries an
explicit round-trip diagnostic in the ignored index. Focused source tests
require this fail-closed selection behavior.

The repaired source passes repository validation, installed-game compilation,
all 1365 domain/reflection tests, clean Release construction, deterministic
packaging, strict standalone and local-runtime validation, and 163 runtime
preflight checks once the known immediate-post-build timestamp window was
quiescent. The pre-publication local-runtime package SHA-256 is
`255de7da0529767b089d65fbd9638fb4964020a562797f1c6048d3315014c624`;
DLL SHA-256 is
`c9840e31c00997b9c6d50b6f6b044175cbe34165d3f00414ce90fc7781040bef`.
A clean published rerun remains required.

That published rerun at
`20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix` exercised
all six deterministic canonical-size female Aasimar sources. Each reached a
live avatar, rig, and renderer but began with zero equipment entities, so the
probe rejected every one under its `missing-avatar-or-empty-snapshot` branch.
The common failure proves the candidate retry path and isolates the remaining
defect to zero-length snapshot interpretation; it is not visual evidence about
the Magus entities.

The follow-up correction treats an empty sequence as a first-class original
state: it must restore to zero entities with unchanged saved links, while a
null avatar remains invalid. Nonempty entity order and ramp comparisons are
unchanged, and fallback cleanup now verifies empty baselines too. Installed
game compilation, all 1365 tests, clean packaging, and strict package
validation pass. Pre-publication package SHA-256 is
`3b7e2deb7b96dac8e62eba66d1628af2355e0ab2c4ff4259ab245e5710b3168a`;
DLL SHA-256 is
`8621f5402e652fbdc1b3eb7d0657d0450f3f5c00cfd861a02961c5563cb0e46f`.

The next run at
`20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix` completed
every intended fixture and render. Dynamic race coverage, all 18 ordered
native link pairs, donor probes, 36 palettes/records, 72 images/180 views, and
18/18 restorations passed. The only failure was terminal cleanup: the global
unit-reference collection did not return to its initial set within 360 update
ticks, although party, save, production, and actor-null boundaries remained
intact. Therefore the result and images remain unaccepted.

The next instrumentation change is evidence-only. It preserves the exact
global cleanup criterion and emits initial/current counts plus described
missing and unexpected unit/party references at terminal cleanup. This will
identify whether the delta is a disposable actor leak or unrelated engine/
third-party churn before cleanup semantics change. All 1365 tests and strict
clean package validation pass. Pre-publication diagnostic package SHA-256 is
`368140973c5e42aacf420168159b30b4a48fe26c7476984a282f621b529721f2`;
DLL SHA-256 is
`93edda11b82111e8a76c1c2298e7260ae142e8d1c68ba127e004b6cef7ea24aa`.

The published diagnostic run at
`20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix` resolved the
delta without relaxing cleanup. It again passed all matrix work, then reported
265 initial and 266 final global units, no missing unit, exact 3-member party,
and a cleared actor. The sole unexpected unit was `Leopard`, blueprint
`AnimalCompanionUnitLeopard` (`54cf380dee486ff42b803174d1b9da1b`), after the
native female-Elf `StartGamePregenRangerUnit` donor had been exercised.

The cleanup repair is relationship-scoped. It captures only the active
disposable actor's exact installed-game `UnitDescriptor.Pet` reference, rejects
any reference present in the pre-run snapshot, retires only the captured
request-owned dependent, and records its before/after registration state. The
unchanged acceptance condition still demands exact restoration of the full
global-unit and party reference sets. Repository validation, all 1365 tests,
clean Release construction, and strict package validation pass; the
pre-publication package is
`ddb92778082adc354b1e574abad9a467a10246c17cefa75ab61281f410feab62`
and its DLL is
`af8262f6593053ceadf56af84c26e56e61d38964b816ed39896ce7b5f7885b39`.
The matrix and its images remain unaccepted until the published repair proves
the exact pet relationship, retirement, and strict final snapshot in game.

## Uncertainty

The supplied external mission-package path was absent at intake. A
manifest-matching pre-existing untracked package was inspected provisionally
without modifying or publishing it. The path discrepancy remains explicit.
Eleven ordinary-isometric images in the first batch were tagged low pixel
density; the accepted rerun reduced that to eight with zero low-density
preview captures. Paired preview-like images were directly usable, but final
evidence must improve or explicitly retain that limitation.
