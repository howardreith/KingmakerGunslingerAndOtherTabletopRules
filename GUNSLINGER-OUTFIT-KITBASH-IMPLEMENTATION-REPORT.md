# Gunslinger Outfit Kitbash Implementation Report

Status: native candidate selected at 88/100 and integrated through a focused
production policy. The published commit-bound package, canonical guarded
working-save load, and complete static equipment/rebuild matrix pass; motion,
outfit persistence, and final package qualification remain open.

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

Selected production candidate: coherent native Magus base plus its one native
accessory.

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
small arcane color accents and pending equipment/animation qualification
account for withheld points. The accepted all-race matrix raises the score from
81 to 88 by awarding full race/gender coverage. This checkpoint precedes the
focused production binding.

## Production changes

`GunslingerClassAppearanceCatalog` owns the exact selected male/female IDs and
2/22 defaults without game dependencies. It validates lower-hex shape, exact
counts, and duplicates on every defensive-copy access. The game-facing
`GunslingerClassAppearance` resolves all four native entities before assigning
anything, constructs new direct-link arrays/link objects and a new empty shared
array, then assigns the new Gunslinger blueprint atomically.

`GunslingerClassBlueprints.CreateClass` now calls this policy in place of the
five Fighter appearance assignments. Fighter remains intentionally resolved
only for starting gold and the existing native combat-feat selection path.
Production never resolves or mutates the Magus class blueprint and retains no
donor mutable-array alias.

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

That proof completed at
`20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix` on published
commit `8b8d0b17aa90318425404efac56f6977bb2ad11c`, MVID
`3595f627-40de-4b76-830b-99920d2838ac`. The terminal PASS covers 9 races,
18 gender fixtures, 36 palette records, 72 PNGs/180 views, 18 restorations,
exact 265/265 global-unit and 3/3 party snapshots, no save call, and no
production mutation. The exact pet relationship occurred on male HalfElf;
the earlier female-Elf attribution was provisional and is superseded.

Mechanical completion exposed a distinct visual-fixture defect. Direct review
of every PNG showed that native NPC prefab meshes can carry clothes, shields,
bows, quivers, capes, or large weapons outside `CharacterAvatar`. Those baked
objects survived the avatar-only setup and obscured or replaced the intended
outfit in several cells. The complete batch is therefore visually rejected;
it makes no negative compatibility finding about the Magus pair and does not
change the provisional score or production.

Read-only installed-game API inspection found the correct deterministic
replacement. `BlueprintRaceVisualPreset` supplies the exact skin and gender
skeleton; `DollState` owns the character-generation appearance and class;
`DollData.CreateData()` records the exact entity IDs; and
`DollData.CreateUnitView(false)` creates the native player view accepted by
`EntityCreationController.SpawnUnit`. The repaired matrix uses that view,
requires a nonempty exact doll baseline and zero unexpected entities, removes
items from every body slot, and requires both weapon-model channels to be
empty. The descriptor blueprint remains a disposable same-race/gender native
source and production blueprints remain untouched.

The repair passes repository validation, all 1365 tests, installed-reference
compilation, clean Release build/package creation, and explicit strict package
validation. Pre-publication package SHA-256 is
`04f13af8fd17a0d9e18611e13c3cc3d27d83f6c7cf1e7dca3b05e094e5f73d18`;
DLL SHA-256 is
`d3ec07a2238ff2c062686dfc4e570ee602afaa716a26ddfa01607cb2627653bc`.
A clean commit-bound guarded rerun and direct review of all replacement images
remain mandatory.

The first such run,
`20260831T0013309100348Z-gunslinger-outfit-finalist-race-matrix`, failed safely
before spawning male Aasimar. Commit
`b67ec5444d4b3ef8480007c10fb2d73bab3c031e` assumed that the preset visual
`RaceId` must equal the progression `BlueprintRace.RaceId`. Zero images were
created; working-save identity, no-save/no-production boundaries, exact
265-unit/3-party cleanup, and automatic exit all passed.

Installed IL provides the correction rather than a heuristic. Native
`DollState.Validate` selects the first serialized `BlueprintRace.Presets`
entry, while `DollData.CreateUnitView` loads `RacePreset.Skin` with the
preset's own `RaceId`. The implementation now mirrors those calls, retains
completeness checks on that first preset, and records progression and visual
race identities separately. Repository validation, 1365 tests, clean
installed-reference compilation/package construction, and strict validation
pass. Pre-publication package SHA-256 is
`e6af511660abba47fd22dae853f6875ed31c1bd68607cc60440fe640f62c9502`;
DLL SHA-256 is
`edbc636195bd0b1fe80e41df7bdf532236502135da819570e07980a99a645824`.
No candidate disposition or production state changes from this early
instrumentation failure.

The corrected visual-race retry at
`20260831T0026335779530Z-gunslinger-outfit-finalist-race-matrix` loaded
published commit `55c487cc460c4950305d47e3c679bf8e858c943d`, exact DLL
SHA-256
`1be20efa6c457eb8da426b54f67598c3529cfc76c5c62454eea7ce9654e1897c`,
and MVID `e496489f-2fbc-47cf-a4c1-da914eda915a`. It failed at the first
male-Aasimar spawn before a fixture, screenshot, or outfit mutation. The
guarded working-save boundary, no save API, exact cleanup, no production
mutation, and automatic exit all passed.

Read-only installed IL makes the root cause exact. `DollData.CreateUnitView`
gets the `UnitEntityView` from the gender-specific doll, instantiates it, gets
and configures its root `Character`, and returns the template. It does not set
the `CharacterAvatar` backing field. `UnitEntityView.OnDataAttached` performs
that assignment later with `GetComponentInChildren<Character>()`. The repaired
probe therefore validates `dollView.GetComponent<Character>()` before
`SpawnUnit` and retains its post-spawn `_actor.View.CharacterAvatar` gate.
Focused tests lock both sides of this lifecycle boundary.

Repository validation, 1365/1365 domain tests, clean installed-reference
Release construction, package creation, and strict package validation pass.
The pre-publication package SHA-256 is
`024d0c2b89a6e561b4c8d6eecc67e6f30c6b85941b893db7f9dcc6d5d22b0f2e`;
DLL SHA-256 is
`3cf170e14b0dc96910b093ee0737e713fd7d0c432a20cd59971c36dfc7be7d42`.
The runtime and visual matrix gates remain open; this failure changes neither
candidate score nor production.

Published lifecycle commit `08bfed17843adf348b210883b6f929b1af7c5678`
then ran at
`20260831T0044105199782Z-gunslinger-outfit-finalist-race-matrix` with
exact DLL SHA-256
`9ebe80a42b3711dcc874357792da4b5a2e797eb0db18cd7a8d7f7d9a5e374db8`
and MVID `f8abfd0e-59da-48c0-a796-15f085984c32`. It advanced beyond the
prior pre-attachment failure. All five male-Aasimar donors then presented an
exact rig/body/no-weapon contract but no doll entity references or renderers,
so the scenario failed closed before any capture. Cleanup, save, production,
and exit boundaries passed.

The installed `SpawnUnit(BlueprintUnit, UnitEntityView, ...)` IL assigns a
unique ID and then calls `UnityEngine.Object.Instantiate` on the supplied
view before `SpawnEntityWithView`. That is correct for asset prefabs but not
for an already-instantiated, runtime-configured `DollData` view: the second
clone retained the hierarchy while losing `Character` runtime equipment
state. The public `SpawnEntityWithView` method directly calls
`CreateEntityData`, attaches data, registers the entity, and does not clone.
The repaired fixture uses that direct path, mirrors native blueprint/ID/
transform setup, verifies reference-identical view ownership, and retains
failure-only local destruction.

Focused contracts require this handoff and prohibit the old double clone.
Repository validation, all 1365 domain tests, clean installed-reference Release
construction, packaging, and strict validation pass. Pre-publication package
SHA-256 is
`d1dfe7cf3697e5757ce0bc86d7f0e2af72a621e98e4021c5ff5101511885a0ec`;
DLL SHA-256 is
`5462960ebbfd8815523b2132e84d7b2377dfc52a051b2ccdb04a646bf33e7108`.
The matrix and direct visual gates remain open.

## Neutral-body false-positive correction

Guarded evidence
`20260831T0058130079392Z-gunslinger-outfit-finalist-race-matrix`
loaded published commit
`141c6a8e1fcdacdb61164113ac77a6191b16254e` and its exact
commit-bound DLL. All structured matrix, restoration, cleanup, no-save, and
no-production-mutation assertions passed. Direct inspection of all 72
captures overruled that mechanical result: both female-Human palettes retained
an oversized two-handed sword from the disposable donor.

The failing fixture recorded `AmiriLevel20_Companion`,
`clearedSlotItemCount=14`, `rendererCount=2`, and
`noWeaponModels=true`. Item removal had cleared the current
`HandsEquipment` references, but it occurred after entity creation and
did not prove destruction of an already-created renderer. The batch is
visually rejected; `magus-complete` remains provisional at 81/100,
and production is unchanged.

Installed reflection establishes a safer pre-creation boundary:
`BlueprintUnit.UnitBody` is public-constructible and explicitly owns
all starting hand sets, armor/accessories, limbs, and quick slots. The scenario
now replaces only the disposable cloned donor's body with a new neutral body
before registration, preserves its native hidden `EmptyHandWeapon`,
sets starting inventory and body arrays empty, and rejects the donor if any
slot item nevertheless exists after spawn. The source blueprint is never
written, and no donor name or identifier is used by production or harness
logic.

Focused tests require clone-before-neutralization ordering, zero created slot
items, source immutability, and generic rejection. Repository validation,
1365/1365 domain tests, clean installed-reference Release build/package, and
strict package validation pass. Pre-publication package SHA-256 is
`be1b6048c299f1d996db1091372c8e6c43863f51bae7b287ee58ca76f3c92bbb`;
DLL SHA-256 is
`68489bd17dd3bb363bbf53464beda0f7011cc10a7725212b31ef60127c80e13d`.
The replacement matrix and full direct image review remain open.

## Accepted all-race visual checkpoint

Published commit `47d6c55f6742219dac07824b08e1daa1c23309a1` passed the
guarded Steam-backed run
`20260831T0125478276325Z-gunslinger-outfit-finalist-race-matrix` with
exact DLL SHA-256
`57f9d7dec390cae8f53a78fadb9bd8c5cadb30368c97b5eadd8e454806ce285c`
and MVID `1bace4ca-657e-4d4b-bccf-d9ee4933876e`. It completed all nine
installed player races for both genders, both systematic palettes, preview-like
and isometric paths, and exact restoration/cleanup boundaries. All accepted
fixtures used request-local neutral bodies and created zero equipment items.

Direct inspection of all 72 replacement images accepts the batch. The prior
female-Human greatsword is absent in both palettes and both camera paths; the
full grid has intact bodies/materials, expected race features, no donor gear,
and consistent ramp behavior. This closes race/gender/color/no-weapon
selection evidence, not final equipment, motion, rebuild, persistence, or
production qualification.

## Production-binding local validation

Two new executable/reflection cases bring the deterministic suite to 1367.
They cover exact ordered identifiers/defaults, defensive copies,
null/malformed/duplicate/count failure, resource-resolution ordering, fresh
link/shared arrays, factory/project wiring, and removal of every Fighter
appearance alias. The active validator count and inherited static evidence are
1367, including repaired propagation across the only missing inherited edge.

Repository validation, all 1367/1367 Release tests, a clean installed-reference
Release build, package construction, and strict standalone validation pass.
The pre-publication package SHA-256 is
`34d9a7005fd9f535c33e460d7b4e23dc94553dbbcd34ee45540aeff167476df0`;
DLL SHA-256 is
`6f039e773910a314f6abf46e2bd0d87d737660abd898d1ea7bd58918d11893eb`.
Version remains 0.0.110. A commit-bound guarded runtime load remains mandatory.

## Published production load checkpoint

The production binding was committed and policy-published as
`bf3e052cb3a91691e214ec9a87c025f25f380c2d`. A clean commit-bound local-runtime
build passed repository validation, all 1367 tests, exact installed-reference
Release construction, and strict package validation. Its package SHA-256 is
`4a91c92b9f842b7744adf707a2149ae13a4cc1ec70733979ad453406548a6c61`;
the DLL SHA-256 is
`78c8a7e8d8c1372bea930e4a48b4211ef4941974a062c1dbb707b0a8b7a1b8f5`,
and its MVID is `41fd1851-9dec-4adf-87eb-0e79763d5e02`.

After all 163 preflight checks passed, the canonical guarded Steam 640820 run
`20260831T0159136175513Z-working-save-smoke` loaded that exact assembly and
reached terminal `PASS`. It proved one uniquely correlated
`KMG_AUTOMATION_WORKING`, a distinct protected baseline, complete catalog and
receiver-bound action correlation, load completion, stable post-load
fingerprint, no save-writing API, hook removal, and automatic exit. The stable
fingerprint records game ID `dce769e0-229c-4bfd-b8ea-e2d572bf8472` and party
count 3. This closes only the commit-bound safe-load checkpoint; it makes no
visual, override, animation, rebuild, or outfit-persistence claim.

## Production compatibility harness local checkpoint

The new `gunslinger-outfit-production-compatibility` scenario evaluates the
exact production Gunslinger appearance instead of recreating the selection
from a native class. Before fixture creation it resolves the bound class,
compares the ordered male/female entity resources and default colors against
the Gunslinger-owned catalog, and repeats that resource-resolution boundary
for every installed player race and both genders.

Two request-local Human `DollState`/`DollData` fixtures call native
`SetClass(_gunslingerClass)` and capture a 16-state sequence per gender. The
sequence spans native/alternate ramps, no weapon, held pistol, held musket,
stored inactive musket, held blunderbuss, light/heavy armor overrides and
removal rebuilds, tricorn and restored hair, cloak and removal rebuild,
backpack visibility, and final clean rebuild. Each state records exact
link-backed entity references, body slots, ramps, saved links, production
blueprint immutability, and paired preview-like/isometric renders. The expected
batch is 32 sidecars, 64 ignored PNGs, and 160 views.

No production reflection was introduced. Harness-only reflection is narrowed
to exact installed signatures for private hair enumeration and backpack state;
all mutations use installed public game paths. Actor/global cleanup is exact,
and save-writing APIs are forbidden. The scenario deliberately leaves motion,
fire/reload/melee, and save persistence to later independently guarded gates.

One focused case raises the suite to 1368. Repository validation, all
1368/1368 Release tests, a clean installed-reference Release build, production
firearm/SoundBank checks, package creation, and strict standalone validation
pass. Dirty-tree package SHA-256 is
`b6da46f4c1a7c61fab0625762b46f5f7c222f6d478811300fdfa041512f409d6`;
DLL SHA-256 is
`1ca246f477ed3ccbd6ef7a194fc90a5b5a14671d2334bbbaf0a08b76236b9d8`.
These local results establish only source and packaging readiness.

## Stored-musket runtime diagnostic and harness correction

The first published production matrix run,
`20260831T0304180367838Z-gunslinger-outfit-production-compatibility`,
loaded commit `82361d31d2b0d7d278046161c13ee503aff6d51a`, DLL SHA-256
`5265ce9925c4c5b3dd4b2ef90bd0f14d5707edd9d871d9959e77bb060c943562`,
and MVID `3b3ab851-e16f-48e6-b900-43c4d78b2558`. It completed default,
alternate-color, held-pistol, and held-musket states before a terminal failure
at `musket-stored-inactive`. It still passed exact installed-game identity,
all 18 race/gender class-link rows, working-save/no-write protection,
production-blueprint immutability, exact global cleanup, and automatic exit.

The failed condition required a stored musket to have no renderable weapon
model. That is inconsistent with the already-qualified production firearm
presentation policy: long guns retain visible stored presentation, while only
specific handgun profiles intentionally hide it. Every outfit-specific state
at the failure point was exact. Consequently the four-record/eight-image batch
is rejected as incomplete harness evidence and says nothing adverse about the
selected clothing.

The correction does not touch production class or firearm behavior. It makes
the established stored/held resolver available to runtime-test peers, resolves
the exact active presentation for each equipped firearm, requires the inactive
musket to be out of combat and renderable, includes it in contact-sheet bounds,
and records presentation role/renderability. The focused contract prevents
regression to a hidden-long-gun assumption.

Repository validation, all 1368 tests, clean Release construction, package
creation, and strict standalone validation pass. Dirty-tree package SHA-256 is
`beed41bbe74601d8d0f499c2ff5dff340f3e90822e02d4fa2e0f25cd69ab6baa`;
DLL SHA-256 is
`397f4c6a5a9069ae07e0ee2cfd195aa88d2a8fa1d82edaed49b983f68efa3396`.
The complete commit-bound rerun and review remain mandatory.

## Native-doll settlement diagnostic and correction

The stored-musket correction was published as
`453f54732c05be6141d3eec259e4c46325f047e0`. Guarded run
`20260831T0319410552031Z-gunslinger-outfit-production-compatibility`
loaded its exact DLL SHA-256
`d9be26094a0eb8fd6f86dcff5572e85756ff311f1db12d22699ca4311c2b1388`
and MVID `0c09675f-81e2-44f2-b98d-f14dd0ee619e`, then failed before
the first capture because the selected male-Human hair entity was absent after
production application. Class entities, saved links, empty weapon state,
production links over all nine races/both genders, blueprint immutability,
save protection, cleanup, and exit were exact. With zero generated images,
this run has no visual acceptance or rejection value.

Installed method inspection attributes the nondeterminism to harness ordering.
The native doll view is materialized through `DollData.CreateUnitView`, but
attached `UnitEntityView`/`Character` lifecycle work continues afterward. The
compatibility harness mutated the avatar immediately after its basic view
objects appeared; the already-accepted race harness instead waited until every
resolved `DollData` entity survived a bounded native settle window.

The corrected compatibility harness adopts that proven boundary. Before any
snapshot or production addition, it requires descriptor/DollData reference
identity, every resolved doll entity, selected hair, exact humanoid rig,
active renderers, and no held/stored weapon for at least the existing
30-update minimum. Timeout diagnostics include the active entity names. This
changes only guarded evidence timing; production appearance and game behavior
remain untouched.

Repository validation and all 1368/1368 tests pass. A clean installed-
reference Release build, firearm/SoundBank checks, package creation, and
strict standalone package validation pass. Dirty-tree package SHA-256 is
`f7e0b896470a4fc120e6d9f8d7166ca1d6bdfaf7a94c53b1545ba73b12ea073c`;
DLL SHA-256 is
`79f5f5138ea94c37b202d21b9320513a1986c78975d9fd3dd78bd8eeb1e8dd76`;
MVID is `1e6d17a7-bb7c-4e5a-b36f-19e64b59969c`. A published,
commit-bound complete rerun and direct review remain mandatory.

## Accepted production compatibility matrix

The readiness correction was committed and policy-published as
`59eb7a97d6c1278f1e4e0d351aa6d4557b2db566`. Its commit-bound package
SHA-256 is
`e15546c561d244f5f29517bec79f71025713cbd79530238ff69232f38fb18394`;
the exact loaded DLL SHA-256 is
`10f1beaf90eb6f5578ab5c8c09f9d10b219d587bb2adb11b308a959a7a422b26`;
MVID is `780b053b-acb8-4716-a5b5-87b578e356e0`.

Guarded Steam 640820 run
`20260831T0344513197562Z-gunslinger-outfit-production-compatibility`
completed with terminal `PASS`. The installed-game index records the exact
2.1.7b assembly identity, all nine dynamic player races and both genders, 18
exact production-link rows, one male and one female Human production doll, 16
reversible states per fixture, 32 sidecars, 64 captures, 160 views, and 2/2
exact original-avatar restorations. The working save was named exactly and no
save-writing API ran. Production blueprint arrays, links, and colors remained
unchanged, and request-local actor/global cleanup plus automatic exit passed.

All 64 captures were directly reviewed. The production outfit retains intact
geometry, materials, hair, and class identity under default and alternate
colors; pistol, musket held/stored, and blunderbuss presentation; light/heavy
armor; tricorn; cloak; backpack; every removal transition; and the final
rebuild for both genders. No hard rejection or stale geometry was observed.
Eight female isometric captures retain low-density warnings, but each remains
legible (minimum 11,278 meaningful pixels) and has a clear four-view preview.

A separate read-only invariant pass compared all 32 sidecars byte-for-state
with their index records, rehashed all 64 PNGs, and checked exact class assets,
35x35 ramp bounds, color applications, hair/saved links, slot transitions,
weapon roles, renderers, restoration, blueprint immutability, and save guards.
It reported zero issues. Static equipment and rebuild compatibility is thus
accepted. The combined compatibility score stays at 15/20 pending independent
motion and persistence gates; no partial points are invented inside the five-
point withheld block.

## Production motion harness

The separately guarded `gunslinger-outfit-production-motion` scenario is now
source-qualified. Installed API inspection established the concrete native
path: `UnitMoveTo` and same-area `ForcedPath` drive movement;
`UnitMovementAgentBase` reports velocity/displacement and accepts a nullable
speed override; `UnitAnimationManager` selects exact Slow/Normal locomotion;
`ForceLookAt` drives the body-relative turn; `UnitAttack` drives pistol,
musket, and native Shortsword attacks; and `AbilityData`/`UnitUseAbility`
drives the actual production Reload Firearm implementation. The exact
locomotion enum namespace was confirmed from installed metadata.

The existing production session was made partial only to share its already-
accepted DollState/DollData materialization, production link/ramp/hair checks,
avatar snapshots, and cleanup utilities. Static compatibility behavior remains
on its original request path. The new request has a distinct allowlist entry,
working-save restriction, runner instance, progress/index names, 1,800-second
collector, result assertions, and focused source contract.

Each gender contributes 27 records: one unarmed idle; one slow walk; one
normal run; one right turn; five frames each for pistol, musket, and Shortsword
attack (ready, updates 1/12/36, acted); and eight reload frames (ready, updates
1/12/36/96/160/240, acted). The complete expected batch is 54 sidecars, 54
four-view PNGs, and 216 labeled views. Every record reasserts exact production
entities/default ramps/hair/saved links/rig and immutable blueprint state.
Dynamic acceptance additionally requires live command ownership, acted
animation, firearm discharge or reload counters, exact ammunition consumption,
nonzero movement velocity/displacement, distinct run speed, and a 60-degree
or greater native turn.

Cleanup is symmetric on pass or exception: item slots and firearm state are
cleared; actor, target, target dependent, and blueprint clones are removed;
original avatar entity order/ramps/saved links and movement settings are
restored; powder and ball return to exact pre-request counts; and global unit
and party references must match their initial snapshots. No save API exists in
the scenario.

Repository validation, all `1369/1369` Release domain/reflection tests, clean
Release build, firearm/SoundBank checks, strict standalone packaging, and all
169 runtime preflight assertions pass. The pre-commit package SHA-256 is
`00c80de81ff7acc218c1bbf08e51623950281f90e74e5750fee685da48b6e9be`;
DLL SHA-256 is
`c60baee8be07590b39c30a8685bde51e277bb13d8f9d0b226fb9f3950a1e4abd`;
MVID is `a9e50b0b-b2e1-42f4-aa91-c9cdf98d4c5c`. These local identities
do not establish runtime correctness. Commit-bound guarded execution and
direct inspection of all 54 PNGs remain mandatory before persistence work.

### First motion execution and repair

The first commit-bound guarded execution used published commit
`3071fe38a61b79131f96f965053e7bc058ce209f` and package SHA-256
`5eb5da0e740b3d84801c256721f921b636db5471d676cd00de98e99f245d2db7`.
Evidence `20260831T0455599323551Z-gunslinger-outfit-production-motion`
returned terminal `FAIL` at 28/54 records. Male idle, locomotion, turn, pistol
and musket fire, production reload, and Shortsword melee all completed their
native evidence schedules; female production materialization and idle also
completed. The female walk did not start because the clean-combat guard saw a
cached player combat state left after retirement of the male combat pair.

This partial batch is diagnostic only. The run still proved exact game/mod
identity, no save API, unchanged production blueprint, restored ammunition,
exact global-unit/party cleanup, and automatic exit. Reflection plus IL over
the authorized installed assembly found registered
`UnitCombatJoinController.Tick()`. It runs the public
`Player.UpdateIsInCombat()` character-list/group recomputation and raises the
party-combat event if that value changes. The narrow repair adds a clean
pre-run snapshot and a per-fixture native controller-tick record after actor
and target `LeaveCombat` plus disposal. It fails if player,
party-combatant, or turn-based state differs afterward, and retains the
original locomotion guard with more specific evidence. Source, build, runtime
rerun, and image review are still pending for this repair.

The final controller-based source revision passes repository validation,
`1369/1369` Release tests, a clean installed-reference Release build, strict
standalone packaging, firearm/SoundBank validation, and the settled-tree
169-check runtime preflight. The first preflight immediately after each clean
build observed only a transient artifact-tree fingerprint change; all
backup/evidence/CIM/process guards remained unchanged, and the identical
settled rerun passed. Pre-commit package SHA-256 is
`7de0fc0ce93a703907a10d5862368083765dae831cd74487073988128538889d`;
DLL SHA-256 is
`b378256b722350bc9128b491e7f0d8e8f3a2b630bdccefe4664fb5c80f84e18f`;
MVID is `b4bf5593-d05b-41d5-b92c-d6ad1eff1356`. Commit-bound runtime
replacement evidence and direct image review remain pending.

### Second motion execution and native group-retirement repair

Published commit `fe24655acd4516e334796524ab7a3f40fd633888`, package
SHA-256
`5228a562f65fbb2b694ec617548e71d1b713c3fea35d93789834b36eccebd44e`,
and DLL SHA-256
`ba1638817210bfa9b2d163356465719cc0d22e941947286c3d399bf3f236a9dc`
ran through Steam 640820 as evidence
`20260831T0521459019080Z-gunslinger-outfit-production-motion`. The run
completed all 27 male records, then failed the exact combat boundary. The
request-created hostile had propagated combat to all three baseline party
members; retiring only the disposable actor and target left player, party,
and turn-based observations at `true/3/true`. The join controller therefore
preserved a real live group state. No partial capture is accepted.

Installed metadata and IL resolved `UnitCombatLeaveController.Tick()` as the
missing registered stage. Its group path calls full
`UnitEntityData.LeaveCombat()` and the unit event lifecycle; the existing
registered join tick then recomputes player combat and raises the party event.
The repair resolves both controllers, invokes them in leave-then-join order,
and requires exact equality with the clean pre-run boundary. It does not
write combat flags, spoof an event, call a save API, or weaken any guard.
Repository validation, the focused invariant, complete `1369/1369` Release
suite, clean build, strict package and firearm/audio checks, and settled
169-check preflight pass. Pre-commit package SHA-256 is
`b3598b28366eb82161b66b1e65144430c9461380dc93dc3dd2bb15db9fd7fbb3`;
DLL SHA-256 is
`9f717b6c8d08f39cd67635bfc5e635543e38d60a38ce215a0a5c4f590cadfa41`;
MVID is `6e2a6987-f89a-42c1-a3ad-e1635a47b796`. Commit-bound replacement
runtime evidence remains pending.

### Third motion execution and full unit-event repair

Commit `df4f3f04f55bbbdfe56ef113f723f89af23fa62a` ran as exact package
SHA-256
`fa29aab259ef800d0db3ab11ccf6bd3b82999760778733523ef2737dfec348dc`
and DLL SHA-256
`876879b6ab7f1cd2a376e8f43ed74109722f4841eb335179c20dad463ad0b651`
in guarded evidence
`20260831T0539205863874Z-gunslinger-outfit-production-motion`. The male
27-record matrix completed, but the final player/party/turn-based state stayed
`true/3/true`; the partial batch is rejected.

Installed IL isolates the cause: low-level
`UnitCombatState.LeaveCombat()` changes state without raising the unit combat
event. Full `UnitEntityData.LeaveCombat()` raises that event after native AI,
equipment, and audio cleanup, allowing the subscribed turn-based controller
to remove the disposable participants. A registered turn-based tick refreshes
`HasEnemyInCombat`; group leave and player recomputation can then complete.
The implementation now uses that full lifecycle for all request-local units,
records exact enemy/history/sorted-unit caches, and requires the ordered
turn-based, group-leave, then player-recompute sequence. Installed-reference
compilation, repository validation, and `1369/1369` tests pass. Clean
pre-commit packaging and strict firearm/audio validation also pass: package
SHA-256
`ae22f6d1804ef1d4b9677d0a55c57dd3371c0340b63284f76e94e7bd8b5120f3`,
DLL SHA-256
`d0ba5261d5cf26d0b57534f060fbcba7407b1c4f0c421230f99ea8de2dcdcd75`,
and MVID `40e11afc-987d-4755-a057-df54bbfd09bf`. The first preflight
reported only the documented artifact-tree stabilization sentinel; the
identical settled-tree rerun passed all 169 checks. Exact-commit replacement
runtime evidence remains pending.

### Fourth motion execution and request-local faction isolation

Published commit `f127e1f25f0d6d562a27a56ce9fe23f9b1ab8044` ran through
Steam 640820 with package SHA-256
`66d97da08b4615991210cf74e5f0784d1de3c8910dfcecac78d779ec96f6dbed`,
DLL SHA-256
`ea7c0b4931fbd32587aa9451b2c3475613bb866cc3658ad9dc67b63abfe7229e`,
and MVID `1f1de511-e4f9-4f52-98e0-ec2127a56494`. Evidence
`20260831T0601202638447Z-gunslinger-outfit-production-motion` completed nine
male records, including all pistol frames and one acted discharge, then failed
before musket attack because the preceding action was no longer quiescent.
No image from the partial batch is accepted.

Installed `UnitEntityData.GroupId` IL proves a real player-faction actor joins
the directly-controllable group. Installed join/leave IL also proves a live,
conscious remembered hostile can re-enlist a group between actions. The
exception cleanup preserved save, inventory, unit, target, blueprint, and exit
guards but correctly rejected player and turn-based combat still being true.

The repair replaces that party-coupled fixture. It clones two native factions
request-locally, clears player-control/global-hostility flags, gives only the
two clones mutual `AttackFactions`, and verifies actor/target hostility while
both directions to the real save anchor remain non-hostile. Each attack gets a
fresh target retired before the next action. Every tick and capture requires
the original player, party, turn-based enemy/history, and sorted-unit caches;
cleanup removes only request-local memory links and destroys every clone.
Installed-reference compilation, repository validation, and all `1369/1369`
tests pass. Clean Release/package, strict firearm/audio validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit package SHA-256 is
`78e8a067544d097c158aa77ce014fa9ccc0caf9863a6d2d9691492c7821cfd9c`,
DLL SHA-256 is
`db27ce97885fbba43df32c5bc804fde1ef81d3e6ed45c521c1bfd7386616cd9d`,
and MVID is `f4bc8c6e-c148-4890-818c-34dba4f32f1a`. Exact-commit runtime
replacement remains pending.

### Fifth motion execution and loaded-scene isolation

Published commit `1d2b1f8865b5ec12e57ea7dcc1ad25a8762eb63c` ran through
Steam 640820 with package SHA-256
`8102f48085bed0830f746c52042e5b05e6a603dc36de49c556b052ec30863e71`,
DLL SHA-256
`65c530ec491759987d026d86cb4400197eccd209cdb2ba641e774940edd22925`,
and MVID `f420093c-fef2-4a76-ad47-21e79bbc5c2b`. Evidence
`20260831T0637014594621Z-gunslinger-outfit-production-motion` produced four
clean noncombat male records, then failed before its first pistol capture when
the boundary became `player=True/False;party=0/0;turnBased=True/False;units=2/0`.
No partial image is accepted. Cleanup still passed with exact save, inventory,
target, faction, global-unit, blueprint, hook, and automatic-exit protection.

The isolated faction result did not justify treating that transition as benign.
Exact installed IL shows `Player.UpdateCharacterLists` enumerates both party
references and `CrossSceneState`. `Player.AddCharacterToLists` adds any
in-game, non-detached, non-ex-companion cross-scene unit to
`m_ControllableCharacters` without checking faction or direct-control flags.
`Player.UpdateIsInCombat` then sets the global property from every controllable
unit's group; turn-based mode in turn reads that property. The shared doll
spawn used the live party anchor's cross-scene holding state, so the fixture
was still player-coupled even though its faction and group were not.

The corrected motion path resolves
`PersistentState.LoadedAreaState.MainState`, requires it to be the exact loaded
scene and not `Player.CrossSceneState`, and routes only motion actors through
that scene. It refreshes and snapshots canonical controllable and cross-scene
reference sets and requires them to remain exact at actor/target creation,
every tick and capture, target retirement, combat reconciliation, and cleanup.
Sidecars require both disposable units to be area-local and absent from
controllable characters. Installed-reference compilation, repository
validation, and all `1369/1369` tests pass. Clean Release/package and strict
firearm/audio validation pass. The first preflight reported only the known
artifact-tree stabilization sentinel; the identical rerun passed all 169
checks. Pre-commit package SHA-256 is
`2c6bdf7ffe6901ef33ddf5ab908e195cb3ce0675d93fc974b8c2798de9a30077`,
DLL SHA-256 is
`81a315c486dae914ec04c63bd0079be1780c626d5031416c0f5c0c0d7ecf6651`,
and MVID is `6ed1466d-9131-4b83-84e6-5f86c156a20f`. Exact-commit
replacement runtime evidence remains pending.

### Sixth motion execution and request-local loaded-scene container

Published commit `27bc24ae9ce5b84d3eb8760741833697ed52a911` ran through
Steam 640820 with package SHA-256
`9c97279edf78fb4f7540667b3e983b2c5b5b0b5ec98604c3fdea3b0e4bec3413`,
DLL SHA-256
`37c764f27e63f984fd09b9ec80d465372e997e693269716b8b61e66f07eb98a3`,
and MVID `38f7a207-baa3-4ee8-8774-c8d3de192b92`. Evidence
`20260831T1215532823796Z-gunslinger-outfit-production-motion` failed before
record 1 because the area-owned male fixture attached without its DollData
entity set or hair. The player and turn-based states remained false, player
lists stayed exact, the then-current global-unit cleanup assertion passed,
and no image was accepted. That assertion did not cover the save-backed
state's own entity list; no save API ran and the process exited.

Read-only installed IL establishes a narrower solution. A
`SceneEntitiesState` can own an entity without being registered in
`AreaPersistentState`; its live-scene predicate is only a lookup of its
`SceneName`. Conversely, `EntityDataBase.Dispose` does not remove the entity
from the owning state. The repaired harness constructs a disposable state
with the exact active `MainState.SceneName` and `SkipSerialize=true`, proves
it is distinct from both `MainState` and `CrossSceneState`, and routes actor
and target through it. This preserves the loaded Unity scene for pathing and
rendering without mutating either persistence graph. Cleanup uses native
`RemoveEntityData`, requires the container empty between genders, and disposes
the still-empty container terminally.

Installed-reference compilation, repository validation, all `1369/1369`
tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel.
Pre-commit package SHA-256 is
`64d07b6d3aa843aefb185cd2a07e4dce860ea46e522770e9eff7e9d16988981e`,
DLL SHA-256 is
`582e306bae50394eca161705b425c847bc08ba36e59ab23b36ac6fdfdd91a0d3`,
and MVID is `43f248b1-be23-43f8-aaf9-78cb02a8f9cd`. Exact-commit
replacement runtime evidence remains pending.

### Seventh motion execution and detached native attack planning

Published commit `b27438c7fd38d4e588a47b05b5e2329fb3676932` ran through
Steam 640820 with package SHA-256
`788dcf4d89fac23941f79d9cca54db5f673bb5405125ca5e8817ef24553056e8`,
DLL SHA-256
`9a0d1a9d671697f9a5a46c366cb6fe29af83dc528530e805987c55791ff21456`,
and MVID `60f2fd26-9d78-401d-94d1-69a1c393afbe`. Evidence
`20260831T1253077289617Z-gunslinger-outfit-production-motion` reached ten
male records: exact locomotion, turn, and the full pistol attack schedule all
ran, including one acted discharge. It then failed before the real musket
attack because the target-placement probe had already acted and consumed that
weapon's only round. The musket-ready sidecar records zero loaded rounds,
total fired count two, and a live `UnitAttack`; production correctly rejected
the resulting unloaded request. No partial capture is accepted.

This run validates the request-local loaded-scene architecture. Exact
game/build/save identity, no-save behavior, blueprint immutability, player
lists and combat boundary, scene emptiness/disposal, cleanup, and automatic
exit passed. Every sidecar retained the exact production outfit and neutral,
non-controllable request-local unit contracts.

Exact installed IL distinguishes planning from execution. `UnitAttack.Init`
sets the executor, creates the native attack plan, and computes approach
radius. `UnitCommands.Run` calls that method and additionally registers the
command, allowing normal ticks to start and act it. The repair uses direct
native initialization for target-placement probing, requires the probe absent
from actor command slots before and after positioning, and reserves `Run` for
the separately constructed command being evidenced. Structured sidecars and
terminal attack contracts now require the detached-probe fact.

Installed-reference compilation, repository validation, all `1369/1369`
tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel. Pre-commit
local-runtime package SHA-256 is
`31498b7bed5b9532d0a208cda645b744cdfefa30b2a7246fab472696da7f0ce1`,
DLL SHA-256 is
`877b451e4a4a62751b3d1b75c217e24c2b66c857ec4d973e28bdfc5e23ef100d`,
and MVID is `2ada3432-6aa8-4a77-81b1-934fe1a698f0`. Exact-commit
replacement runtime evidence remains pending.

### Eighth motion execution and native command retirement

Published commit `5d520bbccaff98e09a9a94c3fa2c59811cd2f0ca` ran through
Steam 640820 with package SHA-256
`a703f089ff28cc83c3d835df36de1180950d668b5230a6bcef9a7cc9fcf7eb6b`,
DLL SHA-256
`7e8c1619acec69da73f10f6e5f3f6089a5d571163077fe9533193c4976763548`,
and MVID `8b7060eb-cefe-4ac9-8be1-62d61a0e1974`. Evidence
`20260831T1330408485246Z-gunslinger-outfit-production-motion` again reached
ten male records. Crucially, pistol and musket preparation both proved their
readiness probes detached, validating the seventh-attempt implementation. The
pistol command acted and discharged exactly once, yet its update-36 sidecar
still contained a running `UnitAttack`. The musket-ready sidecar then recorded
zero loaded rounds, fired count two, and the same active command. A fresh
musket command was correctly rejected as unloaded. No partial capture is
accepted.

Exact installed IL isolates a different lifecycle defect. Base
`UnitCommand.IsInterruptible` remains false while a started, acted animation
has not finished, and `UnitCommands.InterruptAll(true)` retains such a command.
The old harness completed its evidence contract at update 36, then changed
equipment, combat, and target state despite that retained command. The game
log's equipment-animation null reference immediately after pistol discharge,
followed by a second firearm round consumption after the switch, corroborates
the surviving-command sequence.

The implementation now has separate evidence-complete and retirement-ready
conditions. It continues native ticks inside the existing 360-update bound
until the attack is no longer running or is natively interruptible. Outcome
JSON records readiness, current running/interruptible state, and update count;
the terminal attack contract requires readiness. Ordinary interruption then
runs, followed by a hard zero-running-command gate before weapon, target, or
combat mutation. Inter-action transient cleanup requires the same zero state,
and frame sidecars enumerate running command types. A focused source test
enforces interrupt, gate, then teardown ordering.

Installed-reference compilation, repository validation, all `1369/1369`
tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel. Pre-commit
local-runtime package SHA-256 is
`17d46838be9b31b3fecda29ef582f2aae2cfc422e2f5c25be41f3d58811f2dbb`,
DLL SHA-256 is
`e1b154a9e2c35348d6b6d67cd9fa8274c4764ffa5604335a24e680ada14b5844`,
and MVID is `bbd56913-905f-4d32-8546-cc3926bdaa2f`. Exact-commit
replacement runtime evidence remains pending.

### Ninth/tenth motion execution and finished-slot eviction

Published commit `0dbdaf2b283bbb6245939d4078c26f90d94d01ff` ran twice
through Steam 640820 with package SHA-256
`a8a6ae85f171e1c5140f17794830b0d11b64b4154af21a755332dd784ee570ca`,
DLL SHA-256
`585e4abf748225398f13c02afbd62313e2111fd46137cd57415af331925efd40`,
and MVID `a6768c5d-46e6-4fef-b45c-c2b958989d4e`. The first evidence set,
`20260831T1401393847532Z-gunslinger-outfit-production-motion`, failed before
record one at an intermittent male native-doll settle boundary. All guard,
build/save identity, no-save, immutable-blueprint, exact cleanup, and exit
contracts passed. Two prior exact fixtures had passed that pre-action boundary,
so one unchanged-commit retry was used to distinguish a transient settle miss
from the command-lifecycle defect.

The retry,
`20260831T1407213494923Z-gunslinger-outfit-production-motion`, reached ten
male records. The pistol acted and discharged, remained running and
non-interruptible at update 36, then eventually satisfied the new retirement
gate. The following musket-ready frame showed zero running commands but still
contained a resident `UnitAttack`; its musket had zero loaded rounds and the
total firearm count had advanced twice. The production unloaded-firearm guard
correctly refused to construct the next attack. No partial capture is accepted.

Exact installed IL establishes the missing distinction. `UnitCommands.Raw`
may retain a finished command because `InterruptAll(bool)` skips
`IsFinished` entries without clearing their slots. Public
`RemoveFinishedAndUpdateQueue()` removes those finished raw entries. The prior
zero-running gate therefore admitted a still-resident pistol command that
later interacted with the newly equipped musket.

The implementation now rejects a nonempty native queue before teardown,
interrupts, executes `RemoveFinishedAndUpdateQueue`, and requires
`slotEvicted`, `Commands.Empty`, and empty running/resident/queued collections
before changing weapon, target, combat, or avatar state. Attack outcomes record
all post-retirement collections; frame sidecars record resident, running, and
queued types; inter-action cleanup repeats the entire empty-container contract.
Focused source tests enforce this precise order.

Installed-reference compilation, repository validation, all `1369/1369`
tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel. Pre-commit
local-runtime package SHA-256 is
`fca9cf06fb1fb6a3e967eb7414c3ffb4ac679d639695ab81faf146788921e274`,
DLL SHA-256 is
`1d2d17ffe350388308fab4aa62d81637d378ce44c2408ec8c2d34c365a3a418a`,
and MVID is `d983a009-2e6c-41aa-ba32-56b9c20487f9`. Exact-commit
replacement runtime evidence remains pending.

### Eleventh motion execution and doll-attachment lifecycle instrumentation

Published commit `4ef28f65577d09329536a905976b405cac4562ef` ran through
Steam 640820 with package SHA-256
`6f849b89c4ffba745585d268c1a1ff12c83074b2e5f80d13853e91e3c6c77a34`,
DLL SHA-256
`871a89190537624f150356e381b106cb162b70a215936c780913642096cb01c4`,
and MVID `10e8676b-e8d8-48f4-b4a1-210d0afe0d2f`. Evidence
`20260831T1438053243232Z-gunslinger-outfit-production-motion` failed before
record one. The male Human fixture reached its bounded settle timeout with no
active DollData entities and no hair. Exact guard, build/game, disposable-save
and no-save, immutable-blueprint, player state, structural cleanup,
request-local scene disposal, and automatic-exit contracts passed. No image
exists or is accepted.

This repeats attempt 9's pre-action boundary, so the unchanged implementation
will not be retried. Installed IL establishes the useful observation points:
`DollData.CreateUnitView(false)` instantiates the native character and resolves
and adds each equipment-entity ID; `SpawnEntityWithView` queues ownership; and
`UnitEntityView.OnDataAttached` subsequently obtains and starts the character,
then updates body/class equipment and rebuilds its outfit.

The implementation now retains the original template `Character` and records
its lifecycle immediately after creation, after spawn but before the entity
tick, after attachment, and at the settle timeout. Structured diagnostics
include `ResourcesLibrary.Preloading`, Unity object identity, raw, active, and
saved equipment counts, expected DollData ID count, active names, and whether
the attached character is the original instance. This is bounded,
request-local observation only; it does not add game or save mutations.
Fallback cleanup explicitly clears the retained references. Focused tests
require all four diagnostics in causal order.

Installed-reference compilation, repository validation, all `1369/1369`
tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel. Pre-commit
local-runtime package SHA-256 is
`aa512f88878ef88d7486176080552f6ff3ac237f540a3f042d49d75842227112`,
DLL SHA-256 is
`0c97e7c7a7c450fa93fef6fcc42a523809302c9dc01934352ab06530cdc0583b`,
and MVID is `47392b4f-cbc0-450f-9b72-82b284e578c7`. Exact-commit attempt 12
remains pending.

### Twelfth motion execution and request-local AI isolation

Published commit `2e73bf3035860ffc940c31f4e5c090b0f5d5df2e` ran through
Steam 640820 with package SHA-256
`6d4e6b3aa27658e958f7010937a7d62e481988eb4eb5967fdc8d719dfbd94d5f`,
DLL SHA-256
`90c727bcbd90ac962e7dd406c6bbc0c8f16f55ac05ff4ba8f812a9ff0e1f205d`,
and MVID `6d5eafa3-919a-40fc-a39c-9206ab6ca58f`. Evidence
`20260831T1509405239304Z-gunslinger-outfit-production-motion` reached 10/54
male records before the musket's native command was rejected as unloaded.
All guard, build/game, disposable-save/no-save, immutable-blueprint,
request-local scene, structural-cleanup, and exit contracts passed. No partial
image is accepted.

The lifecycle diagnostic resolved its question positively: the same avatar
instance (`-654738`) retained five raw/active native entities before spawn,
after spawn before tick, and after data attachment; preloading was false,
saved count zero, and expected outfit-ID count four throughout. The outfit
was neither empty nor replaced.

At musket ready, no harness attack was installed and the readiness probe was
detached, but resident and running command collections contained `UnitAttack`.
The firearm had zero rounds and total discharge count two. Because the prior
pistol teardown had synchronously passed its empty-container contract, this
was a newly issued command. The request-local actor and target blueprint clones
retained the source NPC brain, and ordinary `JoinCombat`/`Engage` activated it.

The repair clears `BlueprintUnit.Brain` only on those two disposable clones,
preserving native combat, commands, weapon, animation, outfit, faction, and
save behavior. It fails before accepting a ready frame unless the command
container is empty, rejects any harness `UnitAttack` with an `AiAction`, emits
clone-brain and active-command ownership fields in every sidecar, and requires
those facts in terminal reconciliation. Installed-reference compilation and
all `1369/1369` tests pass, as do clean Release/package construction, strict
package/firearm/audio validation, and the settled 169-check preflight. The
first pass reported only the documented artifact-tree stabilization sentinel.
Pre-commit package SHA-256 is
`b3c73cf63e68fa3cb4aff086bd236accf3769e69f53a5afe7c259776139d76e2`,
DLL SHA-256 is
`3c95c8c5115135023023ff74d4c77cbc3aaf90ff7c0ca742c3e397e1741c839d`,
and MVID is `5a199838-48eb-49a2-8b92-7ca8d0dfabe2`. Commit-bound runtime
replacement remains pending.

## Uncertainty

The supplied external mission-package path was absent at intake. A
manifest-matching pre-existing untracked package was inspected provisionally
without modifying or publishing it. The path discrepancy remains explicit.
Eleven ordinary-isometric images in the first batch were tagged low pixel
density; the accepted race rerun reduced that to eight with zero low-density
preview captures. The accepted production matrix likewise has eight
conservative female-isometric warnings, each directly legible with at least
11,278 meaningful pixels and a clear paired preview. The limitation is
explicitly retained while motion and persistence evidence remain open.

### Thirteenth motion execution and pre-creation resource gate

Published commit `934785962bb4ef752993add5558d20cb751f1c7d` ran through
Steam 640820 with package SHA-256
`a53c3314dd6aeb5d4ee13a8f0b5615d93325212062f1c5916ef0aa9460f88e5f`,
DLL SHA-256
`af2af437dd06f55c1316305190e02aee86f95a1d0f0c2364b48b4eb032c7fff1`,
and MVID `0e441834-5f14-41d6-b1ad-15d46b4f976e`. Evidence
`20260831T1548069712324Z-gunslinger-outfit-production-motion` failed before
record one; every environment, save, blueprint, cleanup, scene, and exit guard
passed, and no partial image is accepted.

The four-point lifecycle evidence makes this occurrence conclusive. Global
resource preloading was true when the native doll view was constructed, and
the same avatar contained zero entities through attachment. Preloading ended
during the later settle loop, but that object stayed empty. Installed
`DollData.CreateUnitView` IL loops the IDs and calls
`TryGetResource<EquipmentEntity>(id, false)`; installed `TryGetResource` IL
returns null immediately when preloading is true. There is no deferred add in
the construction path.

The implementation now delays construction itself until preloading is false,
bounded by the existing 360-update safety window. It hard-fails if the state
changes at the creation line, emits per-fixture wait and creation-state fields,
and requires the gate in static compatibility and motion terminal assertions.
The later settle contract remains unchanged. Installed-reference compilation
and all `1369/1369` tests pass, as do clean Release/package construction,
strict package/firearm/audio validation, and the settled 169-check preflight.
Its first pass reported only the documented artifact-tree stabilization
sentinel. Pre-commit package SHA-256 is
`8aba976c9550a3c09b95539dee11d7825362169b0933b546837cb2e34d25c378`,
DLL SHA-256 is
`379f0bc2a1612065b3ae53539b391f11ac20161be18b4a0dfb0f47bba8803a89`,
and MVID is `5a3b66e8-97b3-4d55-b7e0-db500ca82c96`. An exact-commit replacement
run remains pending.
