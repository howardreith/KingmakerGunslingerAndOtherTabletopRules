# Gunslinger Outfit Candidate Matrix

Status: Human candidate rendering and scoring complete; `magus-complete` is
the provisional finalist. The guarded 18-cell race/gender matrix is
source/package-qualified and awaiting installed-game execution; no production
asset identifier is approved until the equipment, animation, and persistence
qualification passes.

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

| Stream | Base hypothesis | Accent hypothesis | API inventory | Rendered M/F | Race grid | Weapon states | Equipment override | Provisional score | Disposition |
|---|---|---|---|---|---|---|---|---:|---|
| A | Inquisitor or Magus fitted base | Alchemist/Rogue utility or strap | Loaded exact M/F links; Inquisitor cap and cape separated | Magus pass | 9 race IDs inventoried | No weapon/pistol/musket pass | Pending | 81 | Advance coherent Magus pair |
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
| 1 | `magus-complete` | 2 M / 2 F below | Strong fitted open torso, split waist tails, bracers, belts, and boots; controlled arcane detail | Human M/F rendered; 9x2 grid inventoried | No missing geometry/material defect across Human weapon/ramp cases; animation/equipment pending | 35x35 valid ramps; native 2/22 and alternate rendered | 81 (26/23/15/8/9) | Preview-like four-view and ordinary isometric, all four cases per gender | Provisional finalist; run full matrix |
| 2 | `rogue-capless-capeless` | 2 M / 2 F below | Clean fitted dark coat/tunic, diagonal straps, restrained burgundy; less distinctive | Human M/F rendered; 9x2 grid inventoried | Clean Human weapon/ramp cases; animation/equipment pending | 35x35 base ramps; native 31/22 and alternate rendered | 75 (23/20/16/8/8) | Same guarded matrix | Runner-up if Magus fails |
| 3 | `slayer-capless` | 3 M / 3 F below | Long layered garment and asymmetric shoulder; heavier and more armored than desired | Human M/F rendered; 9x2 grid inventoried | Clean Human weapon/ramp cases; animation/equipment pending | 37x37 ramps; native 35/36 and alternate rendered | 70 (21/17/15/8/9) | Same guarded matrix | Reserve; below production threshold |

Score components are recorded in rubric order: silhouette, thematic
coherence, compatibility, race/gender coverage, and color quality. Coverage
and compatibility points remain deliberately withheld for unrendered races,
animations, and equipment overlays.

Exact provisional-finalist assets:

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

`magus-complete` is the provisional full-matrix finalist. Its coherent native
base-plus-one-accessory presentation best matches the privateer/swashbuckler
brief without a literal hat, baked weapon, or generic Fighter silhouette.
Production remains unchanged pending exhaustive race, gender, color,
animation, equipment-overlay, rebuild, and persistence evidence.

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
