# Expanded Summoning Sprint 18 state — Ape and Dire Ape

Controlling state for this branch only. Compact by design: the historical
Expanded Summoning journal is not copied here. The frozen primary-source
contract lives in `planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json` and the
source-gate evidence in
`planning/EXPANDED-SUMMONING-SPRINT18-SOURCE-EVIDENCE.json`.

## Current state, 2026-10-09 — BODIES AUTHORED; RUNTIME REVIEW NOT RUN

One sprint, one branch, one PR, one release. Ape (Gorilla) and Dire Ape
(Gigantopithecus) only. Girallon, Xill, Giant Scorpion, Bebelith, Giant Crab,
Sprint 19 and any broader phase are out of scope and were not started.

**Exact candidate `bd21f2447b3e68fbb95759292a1991320529f4e0`**, since corrected
by `aa3d910a`. The complete source, build and package gate passed on the clean
committed candidate, and the guarded runtime harness is now proved end to end on
it. The donor census has run and chosen the rig, both original bodies are
authored and wired, and the Sprint 18 runtime review remains.

The push-policy blocker is resolved: the owner authorized adding this sprint's
branch and the three later ones, the branch is pushed and PR #32 is open.

## Intake, 2026-10-09

| Check | Result |
| --- | --- |
| Latest stable release | `v0.0.146` |
| `origin/master` and `v0.0.146` tag target | `da782d2297d6cf361ac30a5e0ea07348d3b7f7d7` |
| Master delta vs expected baseline | NONE |
| Branch base | `da782d22` (exact released master) |
| Worktree at intake | clean |
| Live source owner | none |
| Kingmaker process | none, at intake and throughout |
| Runtime lease / lock | none (`compatibility-state/compatibility.lock` absent) |
| Staging transaction | none in flight |
| Installation baseline | 136 files, `Info.json` 0.0.117, tree SHA256 `2043B4587E5D25197AE873BBB69A90C367F836D86499F25892D225016F93C593` |
| Save baseline | 118 saves; `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING` present |

## Exact candidate

| | |
| --- | --- |
| Commit | `bd21f2447b3e68fbb95759292a1991320529f4e0` |
| Source state SHA256 | `b3c25e810d540c1507efa968c33c6157d254d8ba6469799eaece538259221f6f` |
| Version | `0.0.147-expanded-summoning-sprint18` |
| Package | `KingmakerGunslinger-0.0.147-expanded-summoning-sprint18.zip`, 336 members |
| Package SHA256 | `205cd6585f477f5d33769115cd942b4a9aa1d91fe1a4ccd3ea8935203e378ebd` |
| DLL SHA256 | `9b64c911e67d87bd58c5bfa644c0b11502424f25d2341da5ec3c112fbcd8c2ec` |
| DLL MVID | `ff9911e3-7061-4fc4-8d5a-f7fa1b234955` |

## Gates

| Gate | Result |
| --- | --- |
| Version-aware repository validation wrapper | PASS |
| Sprint 18 corruption fixtures | PASS 13/13 |
| Repository validator regression fixtures | PASS 3/3 |
| Icon catalog fixtures | PASS 29/29 |
| Complete unfiltered domain suite | PASS 2517/2517 — 10 Sprint 18 rules cases, 7 original-body cases, 2 ledger-bound cases |
| Clean exact-reference Release build | PASS, 14 exact private references |
| Build output validation | PASS |
| Strict standalone package validation | PASS, 340 members (the four original body files) |
| Expanded Summoning orchestration | PASS, 168 assertions |
| Guarded harness end to end | PASS — deploy, Steam 640820 launch, mod load 0.0.147, scenario PASS, exact restoration |
| Guarded primate donor census | PASS — 288 discovered, 28 surveyed, 28 captured, 9/9 assertions |
| Original Ape and Dire Ape bodies | PASS offline — 6/6 Blender fixtures, 24 review sheets each |
| Batched Sprint 18 runtime review | **NOT RUN** |
| Publication gate | **NOT RUN** |
| Release closure matrix | **NOT RUN** |

## Frozen arithmetic, derived from source

| | Before | After registration | After publication |
| --- | ---: | ---: | ---: |
| Unique creatures | 97 | 99 | 99 |
| Summon Monster roster / placements | 88 / 506 | 90 / 519 | 90 / 519 |
| Summon Nature's Ally roster / placements | 86 / 502 | 88 / 515 | 88 / 515 |
| Registered generated placements | 1008 | 1034 | 1034 |
| Suppressed | 0 | 26 | 0 |
| Published generated placements | 1008 | 1008 | 1034 |
| Retained native wrappers | 29 | 29 | 29 |
| Visible choices | 1037 | 1037 | 1063 |
| Ledger entries | 2922 | 2980 | 2980 |
| Project icon concepts | 109 | 111 | 111 |

Ape: 7 Summon Monster + 7 Nature's Ally = 14 roots (parent tiers 3-9).
Dire Ape: 6 + 6 = 12 roots (parent tiers 4-9). Sprint total 26.

## What is implemented

- **Ape**: exactly two primary slams on a project-owned 1d6 slam weapon, each at
  the printed bonus with the whole Strength modifier. No bite, no claw, no rend,
  no invented fear or roar. Large, Space 10 / Reach 10, so no reduced-reach
  carrier; nothing printed grants a bonus against trip, so no trip-defence
  carrier.
- **Dire Ape**: one primary bite and two primary claws in the layout this
  project already qualified for the grizzly bear, dire bear and owlbear, so all
  three are at the printed bonus with full Strength.
- **Rend**: the engine's own `RendFeature` deals 1d4 plus one and a half times
  the live Strength modifier as a single damage event, which is the printed
  1d4+6 unmodified. A bounded Dire-Ape-owned gate decides only whether an attack
  is that rend, keyed on the live attack command, and holds no serializable
  state. It is unreachable by any other creature.
- **Skills**: exact profile-controlled ranks (Ape one Mobility, one Perception;
  Dire Ape one each of Mobility, Perception, Stealth). The generic priority-list
  allocation is switched off for both.
- **Icons**: two original project-owned 128x128 paintings rendered by the
  project's own procedural Blender creature tool. Visibly distinct in colour,
  silhouette, muzzle and hands.

## Honest omissions carried by this sprint

- `ORDINARY_MAP_LAND_USE_SCOPE` — the 30-foot climb speed and the printed Climb
  skill (+14 Ape, +16 Dire Ape, including the +8 racial bonus). The printed rank
  that bought Climb is not reallocated; Mobility is not inflated and Athletics is
  not raised to simulate the racial bonus.
- `PASSIVE_CREATURE_SENSES_UNMODELED` — low-light vision and scent on both apes,
  with no substitute sense and no vision-range override.
- **Rend animation variant**: the engine selects it from its command-level rend
  gate, which this sprint does not use, so the variant is not played. The rend is
  still a visible damage event. Presentation deviation, nothing invented.
- **Knuckle-walking gait NOT authored**: the engine plays the donor rig's own
  clips, which belong to a large upright biped. The geometry is an ape; the
  locomotion and strike timing are the donor's. A faithful primate gait would be
  a general animation system, which this sprint does not build.

## Defect found and fixed by the first guarded launch

The first guarded run of this candidate **FAILED**, and found a real defect no
offline gate could see. Appending the Ape and Dire Ape took
`blueprints/blueprints.json` to 2980 entries and 1,054,189 bytes, just past the
one-mebibyte corruption bound `BlueprintManifest.Load` enforces, so the loader
threw and **the whole mod refused to initialize**. The Shield Other,
Teleportation, Eastern Weapons and Favored Class failures in the same log are a
cascade from that one cause.

`aa3d910a` raises the bound to four mebibytes — still a corruption guard, not a
product limit — and adds `BlueprintManifestSizeTests`, which measures the
shipped ledger against the constant offline and fails while a quarter of the
bound remains, so the next sprint to approach it is warned by a test rather than
by a dead mod. The re-run passed.

| Run | Evidence | Status |
| --- | --- | --- |
| First | `20261009T1431541884818Z-observe-expanded-summoning-native-donors` | FAIL — ledger bound |
| Re-run | `20261009T1444049672874Z-observe-expanded-summoning-native-donors` | PASS |

Both runs restored the owner installation exactly.

## Guarded primate donor census — PASS

Mission section 4, run read-only. Evidence
`20261009T1721285353210Z-observe-expanded-summoning-primate-census`, all nine
assertions PASS, `discovered=288 surveyed=28 captured=28 credibleHeight=28`. The
scenario resolves the four contract anchors exactly, discovers candidates by
term, dedupes by prefab asset id, caps the survey at the contract's 28, and
loads each view prefab **detached and read-only**: nothing is instantiated, no
actor is spawned, no save is touched and no native asset is modified, so there
is nothing to clean up.

It re-confirms in the live library what the offline pre-census found: **no
primate unit type and no `RendFeature` consumer**. Both bodies must therefore be
original project-owned geometry.

**Chosen rig: Troll `CR10_FerociousTrollGuard`**,
`b98735a1737ae494dbe5cbeca1c7c083`, prefab `0bc98460fca38964aae3af6ad5c655ee`,
Large, view scale 0.7, 62 bones, root `Pelvis`, bind height 4.378. Of the 28
surveyed rigs it is the only Large one actually built like an ape: an upright
Pelvis-rooted spine, long two-segment arms ending in real hands with a thumb and
three fingers, a separate `Jaw_01`, and legs with toes. That is what carries
knuckle locomotion and two-handed slams for the Ape and bite and claw contacts
for the Dire Ape. One rig serves both; the two silhouettes are separated by
geometry, not by rig.

| Rejected | Why |
| --- | --- |
| Owlbear (the provisional donor) | 29 bones, no hands, no jaw, no toes; a quadruped bear |
| Athach | 64 bones but Huge, and equally handless |
| Cyclops / hill, frost, stone giants | 48-bone upright humanoid, but a human gait and no ape arm length |
| Wild Hunt, zombies and other UMA humans | human gait; forbidden by the contract |

### Observer correction made during this census

The first run failed 27 of 28. The shared Sprint 17 bind-capture helper refuses
an entire view when any one renderer carries an incomplete bind frame, and the
Bloodmoon Wild Hunt Monarch has a cloth cape renderer like that. Corrected
request-locally: Sprint 18 reads bind poses with its own fully read-only reader
that records such renderers in `skippedRenderers` and fails only when a view has
no usable renderer at all. Sprint 17's survey is untouched, no requirement is
weakened, nothing in the game is mutated, and the failed run's evidence is kept.

An earlier attempt at that fix disabled the offending renderer on the loaded
prefab. That would have mutated a native asset, so it was discarded before it
ran.

## Original bodies — authored

Both apes now have original project-owned geometry: one generated mesh and one
painted albedo each, in `assets/sprint18-primates/`. The generator, painter,
review renderer and offline fixtures are in
`assets-source/original-models/sprint18-primates/`, with provenance in its
`SOURCE.md`. The only game-sourced input is the bind-pose skeleton the census
captured; no native vertex, index, texture pixel, shader or animation curve is
read, and none is redistributed.

| | Ape | Dire Ape |
| --- | ---: | ---: |
| Vertices / triangles | 2720 / 3146 | 3552 / 4106 |
| Closed shells | 51 | 67 |
| Driver bones | 56 | 56 |
| Max influences per vertex | 2 | 2 |

The Ape is a near-black silverback with a heavy brow, a low sagittal crest, a
broad flat face and nails. The Dire Ape is a bulkier russet Gigantopithecus with
a tall crest, a straw-pale shoulder ruff, long canines for its 1d6 bite and
heavy curved claws for its two primary 1d4 claws. The feature list is the
printed routine, not decoration: the Ape has no claw attack, so it has no claws.

Five donor bones carry no geometry, and the generator, the offline fixtures, the
runtime policy and the repository validator each refuse a mesh that weights one:
`Tail_01` and `Tail_02`, because an ape has no tail, and `Tongue_01` through
`Tongue_03`, because neither printed routine has a tongue attack.

The triangle winding is checked rather than assumed. The generator computes the
signed volume of every connected shell and refuses to export a negative one, and
the offline fixtures repeat that check independently. Sprint 17 needed the
opposite convention for its coiled snake bodies; these are measured, and come
out the other way.

`ExpandedSummoningPrimateViewPatch` reaches only the two hidden apes, and only
when immutable identity, blueprint name and donor prefab all agree.
`PrimateVisualAttachment` swaps one instance's body mesh, bones and material,
blanks that instance's equipment skin, and restores every native reference on
release. Nothing shared is modified and no native component is disabled.

## Remaining work

1. Author and run the batched Sprint 18 hidden-candidate runtime review.
2. Publish the 26 roots, merge PR #32 and release v0.0.147.

None of these is an owner decision; the owner has authorized the guarded runtime
work. They are remaining engineering.

Neither ape can release independently: both share the same unauthored visual
seam and the same unrun runtime review.

## Machine state

Installation is byte-identical to intake after every transaction: 136 files,
tree SHA256 `2043B458…93C593`, `Info.json` 0.0.117. Saves unchanged at 118. Two
guarded deployments of the candidate were made and each was restored from its
own backup; no development build remains deployed, no Kingmaker process is
running, the runtime lease is released and no save was read or written.

One build input was repaired: `UnityEngine.PhysicsModule.dll` was missing from
the local private reference bundle and was copied read-only from the installed
Steam runtime, which is the same source the project's own export script uses.
Its bytes now hash identically to the installed file; the other 13 references
were already identical and none changed. Nothing in the repository, the
installed mod or the game was modified.

## Progress

| Mission section | State |
| --- | --- |
| 1 Intake and branch | DONE — branch pushed, draft PR #32 open against master |
| 2 Freeze primary-source contract | DONE |
| 3 Registration, identity, publication | REGISTERED AND WITHHELD; publication NOT RUN |
| 4 Donor and rig audit | DONE — guarded census PASS; Troll rig chosen on recorded evidence |
| 5 Original visuals | DONE offline — both bodies authored, reviewed and wired; runtime NOT PROVED |
| 6 Mechanics | IMPLEMENTED; source-proved; runtime NOT PROVED |
| 7 Skills, senses, omitted movement | IMPLEMENTED; source-proved; live totals NOT PROVED |
| 8 Tests and cadence | DONE — 10 Sprint 18 cases, 13 validator fixtures |
| 9 Hidden candidate qualification | Source half DONE; runtime half NOT RUN |
| 10 Publication | NOT RUN |
| 11 Release closure | NOT RUN |
| 12 Merge and release | NOT RUN |

## Candidate budget

| Budget | Maximum | Used |
| --- | ---: | ---: |
| Hidden candidate | 1 | 0 |
| Product-correction candidate | 1 | 0 |
| Observation-only correction | 1 | 0 |

## Human review

`HumanReview: NOT_PERFORMED_NONBLOCKING`. Technical PASS is not owner visual
approval. Sprint 19 was not started.
