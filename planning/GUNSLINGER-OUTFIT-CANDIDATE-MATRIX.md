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
