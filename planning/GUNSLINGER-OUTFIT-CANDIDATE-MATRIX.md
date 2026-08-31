# Gunslinger Outfit Candidate Matrix

Status: `magus-complete` is bound through a focused production appearance
policy after accepted Human weapon-state rendering and an accepted guarded
18-cell race/gender matrix. The published production commit, clean commit-bound
package, canonical guarded working-save load, and complete static production
equipment/rebuild matrix pass. Motion/animation and outfit-persistence gates
remain open; this is not final mission qualification.

## Scoring contract

| Criterion | Weight |
|---|---:|
| Silhouette and immediate Gunslinger readability | 30 |
| Black-powder swashbuckler/frontier-officer/privateer coherence | 25 |
| Body, clipping, animation, and equipment compatibility | 20 |
| Race and gender coverage | 15 |
| Color-ramp behavior and visual quality | 10 |

Threshold: 75/100 with no hard rejection. A score remains provisional until
both structured compatibility evidence and direct installed-game rendering
exist.

## Reference-derived objective observations

| Reference role | Curated observation |
|---|---|
| Rejected current male/female | Shared blue Fighter tunic, red diagonal sash, broad belt, dark trousers, and boots read as a generic martial class; male/female identity is consistent but not distinctive. |
| Native Barbarian benchmark | Strong asymmetry, exposed/lightly armored silhouette, fur/leather massing, and red accents create immediate class identity; barbarian vocabulary itself is excluded. |
| Native Paladin benchmark | Long layered garment, pale/dark value blocks, and shoulder treatment remain class-readable at preview distance; holy/heavy-knight vocabulary is excluded. |
| Privateer inspiration | Long split coat, white undersleeves, dark leather, red scarf, belts/pouches, bracers, and brass detail; literal hat and baked weapons are not required elements. |
| Armored frontier inspiration | Practical layered leather/cloth, asymmetric shoulder defense, pouches, gloves, and muted earth tones; heavy bulk and helmet dependence are risks. |
| Legendary inspiration | Dark fitted torso/trousers, burgundy scarf/coat lining, cartridge-like waist detail, gloves, and boots form the relevant vocabulary; literal Western hat is excluded. |
| Tricorne inspiration | Long dark coat, diagonal chest straps, shoulder piece, fitted trousers, and practical muted palette form the relevant vocabulary; mandatory tricorne and weapons are excluded. |

## Discovery streams

| Stream | Base hypothesis | Accent hypothesis | API inventory | Rendered M/F | Race grid | Weapon states | Equipment override | Current score | Disposition |
|---|---|---|---|---|---|---|---|---:|---|
| A | Inquisitor or Magus fitted base | Alchemist/Rogue utility or strap | Loaded exact M/F links; Inquisitor cap and cape separated | Magus pass | 9 races x 2 genders directly accepted | No weapon/pistol/musket/blunderbuss pass | Accepted 16-state M/F production matrix | 88 | Advance coherent Magus pair |
| B | Ranger or Slayer practical leather | Native class accessory set | Loaded exact M/F links; cap/cape separable | Ranger and Slayer pass | 9 race IDs inventoried | No weapon/pistol/musket pass | Pending | 70 (Slayer) | Retain Slayer as reserve |
| C | Bard coat/waistcoat hypothesis | Native Bard accessory set | Loaded exact M/F links | Pass | 9 race IDs inventoried | No weapon/pistol/musket pass | Pending | Below top three | Dominant traveler pack/commoner identity |
| D | Alchemist or other strong single class outfit | Native same-class accessories | Loaded exact M/F links | Pass | 9 race IDs inventoried | No weapon/pistol/musket pass | Pending | Below top three | Dominant Alchemist tank/apron identity |
| E | Item-linked compatible base | At most one item-linked accent | 163 live item sources inventoried | Pending | Exact matrix inventoried | Pending | Pending | - | Defer until class renders |
| F | NPC/raw/orphaned entity | At most one proven accent | 361 bounded live raw sources inventoried | Pending | Direct proof required | Pending | Pending | - | Last resort |

## Catalog checkpoint

Guarded run `20260830T2012181937219Z` passed with candidate-set identity
`dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`.
It loaded 1,206 unique entities and resolved all 4,878 links with no inspection
errors. This proves inventory and structural metadata, not appearance.

The first render batch will compare six coherent native class presentations:

1. Bard complete native presentation;
2. Alchemist complete native presentation;
3. Magus complete native presentation;
4. Ranger base/accessories with the cap and cape omitted;
5. Rogue base/accessories with the cap and cape omitted;
6. Slayer base/accessories with the cap omitted.

Inquisitor remains a reserve fitted-base candidate. Its default cap hides head
top and hair, while Ranger, Rogue, and Slayer caps hide hair and ears. Those cap
links are structurally excluded from the first serious batch. Cape links remain
separate and may be tested later as a single accent only after the base renders.
No visual score is assigned from names or metadata.

## Serious candidate ranking

Guarded run `20260830T2130124467293Z` rendered and directly inspected all 48
cases and 96 images from candidate set
`ef38c5c841510df7f03bbf68a8ca9e7fbef3f3403369022505449cb038d347be`.
Every case had an exact held-weapon state, usable preview framing, exact
request-local restoration, no save-writing API, and no production-blueprint
mutation. The in-game result passed all ten assertions. Accepting rerun
`20260830T2158516580621Z` reproduced the exact candidate-set identity, passed
both in-game and outer orchestration, exited automatically, and preserved the
same scored order after direct inspection of all 96 new images.

| Rank | Candidate-set ID | Exact assets | M/F identity | Coverage | Compatibility | Colors | Score | Evidence | Decision |
|---:|---|---|---|---|---|---|---:|---|---|
| 1 | `magus-complete` | 2 M / 2 F below | Strong fitted open torso, split waist tails, bracers, belts, and boots; controlled arcane detail | Human M/F weapon/equipment cases plus accepted 9x2 no-weapon grid | Static weapon, armor, headgear/hair, cloak, backpack, and rebuild states accepted; motion and persistence pending | 35x35 valid ramps; native 2/22 and alternate accepted across 9x2 | 88 (26/23/15/15/9) | Preview-like four-view and ordinary isometric; all 72 race-grid and 64 production-matrix PNGs directly inspected | Approved for focused production integration |
| 2 | `rogue-capless-capeless` | 2 M / 2 F below | Clean fitted dark coat/tunic, diagonal straps, restrained burgundy; less distinctive | Human M/F rendered; 9x2 grid inventoried | Clean Human weapon/ramp cases; animation/equipment pending | 35x35 base ramps; native 31/22 and alternate rendered | 75 (23/20/16/8/8) | Same guarded matrix | Runner-up if Magus fails |
| 3 | `slayer-capless` | 3 M / 3 F below | Long layered garment and asymmetric shoulder; heavier and more armored than desired | Human M/F rendered; 9x2 grid inventoried | Clean Human weapon/ramp cases; animation/equipment pending | 37x37 ramps; native 35/36 and alternate rendered | 70 (21/17/15/8/9) | Same guarded matrix | Reserve; below production threshold |

Score components are recorded in rubric order: silhouette, thematic
coherence, compatibility, race/gender coverage, and color quality. The
accepted complete race/gender matrix awards the remaining seven coverage
points. The complete static matrix now proves equipment-overlay and rebuild
behavior. Five compatibility points remain deliberately withheld as one
conservative block until motion/animation and persistence evidence also pass;
no partial increment is inferred inside the combined 20-point criterion.

Exact selected and runner-up assets:

- Magus male base `6df8f61725a84294c8661bb9585eca97` and accessory
  `4c59d2b9740930145a27a4c693217d22`;
- Magus female base `beba0e0c7dcd5c64d97d767be3e72995` and accessory
  `a93ead19aae8afc4794c54f5bcf73168`;
- Rogue male accessory `b1c62eff2287d9a4fbbf76c345d58840` and base
  `d019e95d4a8a8474aa4e03489449d6ee`;
- Rogue female accessory `345af8eabd450524ab364e7a7c6f1044` and base
  `c6757746d62b78f46a92020110dfe088`;
- Slayer male accessory/base/native accessory
  `096463cb26b8c3343874d2a2a1a752f6`,
  `bf0f3ba364295e14eb5f2b285cea16b0`, and
  `9e98bd43dc04964409db62644ace4b15`;
- Slayer female accessory/base/native accessory
  `24230460eaff3fe49b0e186873c38218`,
  `5eeabb19544a9ae41a8b26075933ef8d`, and
  `50b6ed92792f308479a07f8d9052c6d5`.

## Hard-rejection log

No enumerated hard rejection is established for the best three from this
Human batch. Bard was omitted because its dominant traveler pack and plain
tunic do not read as a Gunslinger. Alchemist was omitted because its large
tank and apron remain unmistakably Alchemist. Ranger was omitted because its
large bedroll/backpack and fur-heavy leg silhouette read as a wilderness
class and create a high-risk double-backpack interaction. These are
below-threshold shortlist decisions, not claims about untested race or
equipment compatibility.

## Final selection

`magus-complete` is the evidence-selected production candidate. Its coherent native
base-plus-one-accessory presentation best matches the privateer/swashbuckler
brief without a literal hat, baked weapon, or generic Fighter silhouette.
The all-race no-weapon gate is accepted at 88/100. Focused production binding
is now authorized; final acceptance remains contingent on animation,
equipment-overlay, rebuild, and persistence evidence.

The first finalist-only gate discovers player races from
`BlueprintRoot.Instance.Progression.CharacterRaces`, selects deterministic
native `BlueprintUnit` donors for both genders, and independently validates
that native Magus `LoadClothes` returns the exact ordered pair above for every
cell. It is bounded to 36 no-weapon records (native and alternate palette),
72 PNGs (preview-like and ordinary isometric), and 18 exact restorations.
Repository validation, game-reference compilation, all 1365 tests, clean
Release packaging, strict package validation, and 163 runtime-preflight checks
pass. This is instrumentation qualification, not race-grid visual acceptance;
the guarded Steam-backed run and direct review remain open.

### First finalist matrix result

The first clean published run,
`20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix`, is a safe
diagnostic FAIL, not visual evidence against the Magus outfit. Male Aasimar
completed native/default and alternate palettes with two records and four
images. The selected female Aasimar body donor
`LibraryNPC02` (`967f70edf50093242949489c50c5fb65`) could not reproduce its
original avatar entity state, so the gate stopped before applying the outfit.
Cleanup and auto-exit passed; save and production mutation remained false.

The same diagnostic listed a Medium `StartGamePregenClericUnit` as the initial
female Halfling source. Donor discovery now enforces canonical player-race
size and tries all exact race/gender/size matches in deterministic order. A
donor is admitted only after exact original entity order, both ramps, and
saved links survive a request-local round trip; every rejection is preserved
in structured evidence. The repaired package has SHA-256
`255de7da0529767b089d65fbd9638fb4964020a562797f1c6048d3315014c624`
and DLL SHA-256
`c9840e31c00997b9c6d50b6f6b044175cbe34165d3f00414ce90fc7781040bef`.
All 1365 tests and the quiescent 163-check preflight pass. Scores and finalist
disposition remain unchanged until a complete rerun is directly reviewed.

The published retry
`20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix` confirmed
that all six canonical-size female Aasimar donors reached a live rig and then
reported the same `originalEntityCount=0` probe result. This is a fixture
instrumentation defect, not an outfit incompatibility: an empty original
equipment sequence has a well-defined exact order and no ramp entries. The
probe now requires it to remain empty and preserve saved links, while a null
avatar still fails. The pre-publication repaired package is
`3b7e2deb7b96dac8e62eba66d1628af2355e0ab2c4ff4259ab245e5710b3168a`
(DLL `8621f5402e652fbdc1b3eb7d0657d0450f3f5c00cfd861a02961c5563cb0e46f`).
No score or ranking changes from this nonvisual failure.

The next published run,
`20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix`, produced
the complete intended matrix: all 9 installed races, both genders, exact
native Magus links, 36 palette records, 72 images, 180 views, and 18/18 exact
restorations passed with zero donor rejection. Final cleanup alone timed out
because the global unit reference set differed from its initial snapshot;
party, save, and production boundaries stayed exact. The images remain
unaccepted pending cleanup attribution. A diagnostic-only rerun will describe
the precise missing/unexpected references before any ownership-scoped cleanup
decision; the strict global criterion is unchanged for that run.

The diagnostic rerun
`20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix` again passed
the complete render matrix and isolated the cleanup delta to one native
`AnimalCompanionUnitLeopard`, with no missing global reference, exact party,
and no remaining direct fixture actor. This is fixture-lifecycle evidence, not
an outfit score change. The repair recognizes ownership only through the
active donor actor's exact `UnitDescriptor.Pet` reference and retains strict
whole-snapshot equality. `magus-complete` remains provisional at 81/100; its
race-grid images remain unaccepted pending a clean published rerun and direct
inspection.

The clean relationship-scoped rerun,
`20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix`, reached
terminal PASS at published commit
`8b8d0b17aa90318425404efac56f6977bb2ad11c`: 9 races, 18 gender cells,
36 records, 72 PNGs/180 views, 18 restorations, exact cleanup, and no save or
production mutation. Its exact dependent record places the request-owned
Leopard on the male-HalfElf fixture, superseding the earlier female-Elf
inference.

All 72 images were then inspected directly. The batch is visually rejected
because several NPC donor prefabs supplied baked non-avatar clothing and
equipment, including shields, bows, quivers, capes, and large weapons. Some
cells showed the intended Magus pair cleanly, but the contaminated fixtures do
not permit a fair all-race compatibility or aesthetic judgment. No candidate
score changes: `magus-complete` stays provisional at 81/100 and production is
unchanged.

The rerun fixture now uses Kingmaker's native character-generation
`DollState`/`DollData` view for the exact race preset and gender, clears all
body slots, and rejects an empty, unexpected, or weapon-bearing baseline. All
1365 tests, clean installed-reference Release build, and strict package
validation pass for this repair. A complete published rerun and inspection of
every replacement image are required before this matrix can affect ranking or
selection.

The first published neutral-doll attempt,
`20260831T0013309100348Z-gunslinger-outfit-finalist-race-matrix`, failed before
rendering because its resolver conflated Aasimar's progression race with the
native preset visual race. This is an instrumentation failure and changes no
candidate score. Installed IL proves that character generation chooses
serialized `race.Presets[0]` and loads skin with `RacePreset.RaceId`; the
corrected fixture mirrors and records those separate identities. All 1365
tests and the clean strict package gate pass. `magus-complete` remains
provisional at 81/100 pending the replacement matrix and direct review.

The published visual-race retry
`20260831T0026335779530Z-gunslinger-outfit-finalist-race-matrix` loaded commit
`55c487cc460c4950305d47e3c679bf8e858c943d` and failed before its first
male-Aasimar fixture or screenshot. Installed IL shows that the probe
incorrectly treated pre-spawn `dollView.CharacterAvatar` as a
`DollData.CreateUnitView` guarantee. The returned template instead owns a root
`Character`; `UnitEntityView.OnDataAttached` assigns `CharacterAvatar` after
spawn. The corrected probe checks the root component before spawn and the
runtime avatar afterward. All 1365 tests and clean strict packaging pass.
No candidate was rendered or compared in the failed run, so
`magus-complete` remains provisional at 81/100 and production remains
unchanged.

The next published run,
`20260831T0044105199782Z-gunslinger-outfit-finalist-race-matrix`, proved
the root-component lifecycle repair but rejected all five male-Aasimar donors
before rendering: each had exact body identity, rig, size, and empty weapon
models, while the expected doll entities and renderers were absent. Installed
IL shows that `SpawnUnit` cloned the already-instantiated doll view a second
time, losing its runtime `Character` equipment state. The corrected fixture
uses public native `SpawnEntityWithView` and requires that the registered actor
own that exact view reference. All 1365 tests and clean strict packaging pass.
No outfit image or comparison resulted, so `magus-complete` remains
provisional at 81/100 and production is still unchanged.

The published direct-view run,
`20260831T0058130079392Z-gunslinger-outfit-finalist-race-matrix`,
passed every structured count, restoration, cleanup, save, and production
assertion at commit `141c6a8e1fcdacdb61164113ac77a6191b16254e`.
Direct inspection of all 72 images nevertheless rejects the batch: the
female-Human native and alternate palette captures visibly include an
oversized two-handed sword. Its fixture used
`AmiriLevel20_Companion` and reported
`clearedSlotItemCount=14` while the narrower hands-equipment model
check incorrectly reported no weapon.

This is fixture contamination, not evidence against the Magus clothing pair.
`magus-complete` remains provisional at 81/100; no ranking or
production selection changes. The replacement harness gives the disposable
clone an empty request-local `UnitBody` before spawn and rejects any
later-created slot item without hardcoding the contaminated donor. All
1365/1365 tests and clean strict packaging pass. A complete published rerun
and direct inspection of every replacement frame are required before this
matrix may affect the score.

### Accepted neutral-body replacement matrix

Published commit `47d6c55f6742219dac07824b08e1daa1c23309a1` produced the
accepted guarded run
`20260831T0125478276325Z-gunslinger-outfit-finalist-race-matrix` through
Steam App ID 640820. The exact loaded DLL SHA-256 was
`57f9d7dec390cae8f53a78fadb9bd8c5cadb30368c97b5eadd8e454806ce285c`
with MVID `1bace4ca-657e-4d4b-bccf-d9ee4933876e`. All nine dynamically
discovered player races and both genders completed: 18 neutral request-local
fixtures, 36 native/alternate palette records, 72 PNGs/180 views, and 18/18
exact restorations. Every accepted fixture created zero slot items and exposed
no weapon model. Exact party/global-unit cleanup, no save API, no production
mutation, hooks removal, and automatic process exit passed.

All 72 ignored captures were directly inspected through eight labeled review
boards outside the repository. The female-Human native and alternate preview
and isometric cells are free of the previously inherited greatsword. Across
the full matrix, body geometry and materials remain intact; hair, ears, horns,
and tails remain visible where expected; no donor clothing or weapon survives;
and native 2/22 versus alternate color treatment is consistent. The matrix is
therefore visually accepted. `magus-complete` advances from provisional
81/100 to 88/100 by receiving the remaining seven race/gender coverage points.

### Production binding and canonical load checkpoint

Published commit `bf3e052cb3a91691e214ec9a87c025f25f380c2d` binds the exact
accepted pair through a Gunslinger-owned catalog and atomic resolver. The clean
commit-bound package SHA-256 is
`4a91c92b9f842b7744adf707a2149ae13a4cc1ec70733979ad453406548a6c61`;
its loaded DLL SHA-256 is
`78c8a7e8d8c1372bea930e4a48b4211ef4941974a062c1dbb707b0a8b7a1b8f5`
with MVID `41fd1851-9dec-4adf-87eb-0e79763d5e02`.

Guarded Steam run `20260831T0159136175513Z-working-save-smoke` passed exact
working-save catalog, receiver correlation, load completion, stable
post-load fingerprint, no-save-write, hook-removal, and automatic-exit gates.
This removes the commit-bound load uncertainty only. It adds no compatibility
points and does not close the equipment, motion, rebuild, or outfit-persistence
criteria.

### Production compatibility harness local gate

The guarded `gunslinger-outfit-production-compatibility` scenario is now
source-qualified against the actual production Gunslinger blueprint. It
validates the exact selected pair/defaults over all installed player races,
then uses native Human character-generation dolls for 16 deterministic states
per gender: both palettes, empty/held/stored firearm combinations, light and
heavy armor overrides/removals, tricorn and hair restoration, cloak
override/removal, backpack visibility/removal, and repeated appearance
rebuilds. Paired preview-like and isometric capture should produce 32 records,
64 PNGs, and 160 views.

The focused contract plus all 1368 tests, clean Release construction, package
creation, and strict package validation pass. The dirty-tree package SHA-256
is `b6da46f4c1a7c61fab0625762b46f5f7c222f6d478811300fdfa041512f409d6`.
This changes no score: the harness has not yet run in the installed game, its
images have not been reviewed, and motion plus persistence are explicitly
outside its scope. `magus-complete` remains selected at 88/100.

### Stored-musket diagnostic

Published compatibility run
`20260831T0304180367838Z-gunslinger-outfit-production-compatibility`
loaded exact commit `82361d31d2b0d7d278046161c13ee503aff6d51a` and stopped
cleanly after four male-Human states. The matrix rejected the inactive musket
solely because its model remained renderable while `HandsEquipment` was out of
combat. Exact production entities, hair, ramps, saved links, body slot,
blueprint immutability, cleanup, and no-save boundaries all passed.

The repository's already-qualified presentation contract requires visible
stored presentation for long guns; hidden storage is reserved for designated
handgun profiles. The partial run is therefore a harness diagnostic, not a
candidate defect. The repaired harness reuses the established resolver,
requires and frames the visible stored musket, and records its presentation
role. All 1368 tests and clean strict packaging pass. No score changes:
`magus-complete` remains selected at 88/100 pending the full replacement run
and direct review.

### Native-doll settlement diagnostic

The corrected stored-musket commit
`453f54732c05be6141d3eec259e4c46325f047e0` was exercised in guarded run
`20260831T0319410552031Z-gunslinger-outfit-production-compatibility`.
It failed before the first capture when selected male-Human hair was absent
after the harness applied production clothing without first proving the whole
native doll had settled. All 18 production race/gender link rows, class
entities, saved links, blueprint immutability, save protection, cleanup, and
exit remained exact. The same hair survived the preceding run, and this run
produced zero images.

The evidence is therefore a fixture-ordering diagnostic, not a candidate
penalty. The harness now requires every resolved `DollData` entity and the
selected hair to survive the bounded native settle window before any snapshot
or mutation, matching the accepted all-race harness. All 1368 tests and clean
strict packaging pass locally. No score changes: `magus-complete` remains
selected at 88/100 pending a complete terminal-PASS matrix and direct review.

### Accepted production compatibility matrix

Published readiness commit
`59eb7a97d6c1278f1e4e0d351aa6d4557b2db566` passed the complete guarded
Steam 640820 matrix at
`20260831T0344513197562Z-gunslinger-outfit-production-compatibility`.
The commit-bound package SHA-256 is
`e15546c561d244f5f29517bec79f71025713cbd79530238ff69232f38fb18394`;
the loaded DLL SHA-256 is
`10f1beaf90eb6f5578ab5c8c09f9d10b219d587bb2adb11b308a959a7a422b26`
with MVID `780b053b-acb8-4716-a5b5-87b578e356e0`.

All 18 installed race/gender production-link rows were exact. Both Human
fixtures completed all 16 states, producing 32 matching records/sidecars, 64
hash-verified PNGs, and 160 labeled views; both original avatar states were
restored exactly. Direct inspection of every capture accepts default and
alternate colors, pistol, held and stored musket, blunderbuss, light/heavy
armor override and removal, tricorn and hair restoration, cloak, backpack,
and final rebuild for both genders. There is no missing geometry, broken
material, severe clipping, baked duplication, or stale override. Eight female
isometric captures carry the conservative low-density flag, but each has
11,278 or more meaningful pixels, its paired four-view preview is not low
density, and every image was directly legible.

An independent read-only reconciliation found zero issues: 32/32 sidecars
equal their index records, 64/64 file byte counts and SHA-256 values match,
all production entities and ramps are exact, all prior states clear, native
hair and saved links survive, and save/API, blueprint-mutation, cleanup, and
restoration guards pass. This closes static equipment and rebuild evidence.
The score remains 88/100 until the deliberately bundled motion/animation and
persistence evidence completes the remaining compatibility criterion.
