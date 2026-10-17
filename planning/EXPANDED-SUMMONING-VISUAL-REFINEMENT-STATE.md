# Expanded Summoning visual refinement: lane state

Lane-specific resume record for the visual-refinement mission (handoff
`Expanded_Summoning_Visual_Refinement_Handoff.zip`, SHA-256
`521ced2667504e7693fa4b91b202a5cf8a5feba6bac4c1afed58f930a4b300eb`, prepared
2026-10-16). It does not replace any other lane's state. The issue ledger is
[EXPANDED-SUMMONING-VISUAL-REFINEMENT-LEDGER.json](EXPANDED-SUMMONING-VISUAL-REFINEMENT-LEDGER.json).
The method, the diagnosis and the regeneration commands are in
[continuous-bodies/SOURCE.md](../assets-source/original-models/continuous-bodies/SOURCE.md).

## Status at this checkpoint (2026-10-17, DATA)

Three statuses are kept apart. No test result changes the owner's acceptance.

| Item | Technical | Visual evidence | Owner acceptance |
| --- | --- | --- | --- |
| VIS-005 Dire Rat | rebuilt, revised after one internal review; focused gate PASS | native after: PASS (runs 1 and 2); offline renders of shipped bytes | accepted from the offline review page 2026-10-17 |
| VIS-024 Rhinoceros | rebuilt, revised after review, recoloured after run 1 | native after: PASS (runs 1 and 2) | accepted from the offline review page 2026-10-17 |
| VIS-001 Tyrannosaurus | rebuilt, revised after review | native after: body in frame, lit and intact; the review assertion fails only on the pre-existing observer limitation, identical on master | accepted from the offline review page 2026-10-17 |
| VIS-003 Woolly Rhinoceros | rebuilt; focused gate PASS | native after: PASS (run 2) | provisional for release |
| VIS-017 Aurochs | rebuilt; focused gate PASS | native after: PASS (run 2) | provisional for release |
| VIS-023 Bison | rebuilt; focused gate PASS | native after: PASS (run 2) | provisional for release |
| VIS-010 Hyena | rebuilt; focused gate PASS | native after: PASS (runs 3 and 4) | provisional for release |
| VIS-009 Goblin Dog | rebuilt; focused gate PASS | native after: PASS (runs 3 and 4) | provisional for release |
| VIS-014 Wolverine | rebuilt; focused gate PASS | native after: PASS (run 3) | provisional for release |
| VIS-033 Shadow Mastiff | rebuilt; focused gate PASS | native after: PASS (run 3) | provisional for release |
| VIS-006 Poisonous Frog | rebuilt and recoloured green and black; focused gate PASS | native after: PASS (run 3) | provisional for release (the owner questioned the orange) |
| VIS-027 Deinonychus | rebuilt; focused gate PASS | native after: body in frame, lit and intact; the review assertion fails only on the Tyrannosaurus's observer limitation (same donor) (run 5) | provisional for release |
| VIS-002 Triceratops | rebuilt; focused gate PASS | native after: PASS (run 5) | provisional for release |
| VIS-028 Ankylosaurus | rebuilt, club bone-tan; focused gate PASS | native after: PASS (run 5) | provisional for release |
| VIS-012 Crocodile | targeted: exported outward, repainted; focused gate PASS | native after: PASS (run 6) | provisional for release |
| VIS-034 Dire Crocodile | targeted: exported outward, repainted; focused gate PASS | native after: PASS (run 6) | provisional for release |
| VIS-011 Giant Ant (Worker) | minor polish: outward, carried tubes, thorax rooted; focused gate PASS | native after: PASS (run 7) | provisional for release |
| VIS-015 Giant Ant (Soldier) | minor polish: outward, carried tubes, mandibles rooted; focused gate PASS | native after: PASS (run 7) | provisional for release |
| VIS-021 Giant Ant (Drone) | minor polish: outward, carried tubes, mandibles rooted; focused gate PASS | native after: PASS (run 7) | provisional for release |
| VIS-026 Giant Scorpion | targeted: outward, telson rooted; focused gate PASS | native after: PASS (run 8) | provisional for release |
| VIS-035 Bebelith | control, unchanged | native control review PASS (run 7) | accepted control (owner) |
| VIS-013 Dire Bat | rebuilt; focused gate PASS | native after: PASS, flight included (run 9) | provisional for release |
| VIS-020 Pteranodon | rebuilt; focused gate PASS | native after: PASS (run 9) | provisional for release |
| VIS-022 Giant Wasp | rebuilt; focused gate PASS | native after: PASS, flight included (run 9) | provisional for release |
| VIS-019 Ape | rebuilt, coat darkened after run 11; focused gate PASS | native after: PASS (runs 11 and 12) | provisional for release |
| VIS-025 Dire Ape | rebuilt, approved palette kept; focused gate PASS | native after: PASS (run 11) | provisional for release |
| VIS-030 Xill | rebuilt; identity confirmed (run 10); focused gate PASS | native after: PASS (run 11) | provisional for release |
| VIS-008 Wolf | presentation change: KMG summons drop the summoned-creature glow; focused build tests PASS | native after: PASS, own dark coat, rim and tint off (run 15; gold in run 14) | provisional for release |
| VIS-037 Ghaele | the same change; glow off (run 15) | the review still fails on the Ghaele's own rising dissolve and a move that did not start | provisional for release |
| VIS-036 Frost Giant, VIS-038 Thanadaemon | covered by the same policy (native options and adapters; domain test) | not observed: the creature review cannot cast them | provisional for release |
| VIS-004 Dire Rat icon | owner-approved edit adopted as the production source (`b6af1725ff`); icon catalog and summon icon tests PASS | menu-size sheet; native: the Dire Rat summons and renders (run 20) | accepted 2026-10-17 |
| VIS-029 Salamander | presentation change: travelling tail gait; owned body starts undissolved; focused gate PASS | native after: PASS (runs 16 and 19), white streaks gone; spear and tail slap used in a real full attack, 15/15 | provisional for release |
| other 5 items | open or owned elsewhere (see ledger) | owner screenshots only | pending |
| Eagle, Stirge, Fire Beetle, Giant Stag Beetle, Girallon, Giant Crab, Stegosaurus (not inventory items) | inside-out first exports repaired outward in place (`930474b4e9`); focused build tests PASS | native after: PASS, solid and lit (run 20) | provisional for release |

## Owner decisions, 2026-10-17

- **Development-candidate pins removed.** `tools/validate_development_candidate.py`
  now resolves its baseline from the release tag `v<active version>` instead of
  a pinned commit, and no longer pins the exact bytes of released art (released
  Sprint 24 routes must stay public and their assets present and packaged).
  Commit `5641fb79d0`; the owner ran the commit because the auto-mode
  classifier refused it. Release validators keep their own pins. See
  `docs/DEVELOPMENT-CANDIDATES.md`.
- **Art style approved.** On the offline review page the owner wrote: "They
  look good. I think you can proceed with this art style for all remaining
  updates." The pilot's acceptance is recorded from that; each later batch is
  pending with the style approved.
- **Dire Rat icon approved.** "The new dire rat icon looks great. Go ahead
  with it." The pilot was adopted unchanged (`b6af1725ff`).
- **Repairs accepted provisionally for release.** "Let's assume that your
  repairs are good. Move forward with them. I will test everything in a big
  review after we release and refine anything that needs addressing after."
  Every changed item is recorded as provisional for release, not as accepted;
  the inside-out bodies found outside the review list were repaired on the
  same word, except the Bebelith, which the owner had accepted as the control.

## Alpha release, 2026-10-17

On the owner's instruction ("Please cut an alpha release. No need to run all
the tests for a normal release. I will go through the new models and do a
manual review."), prerelease `v0.0.157-visual-refinement-alpha.1` was
published, not marked Latest; `master` and the stable `v0.0.157` are
unchanged. Annotated tag `818846aa83` at `95a5c2e2f0`, the commit whose exact
package ran in guarded review `20261017T2049460194047Z`. Assets:
`KingmakerGunslinger-0.0.157-visual-refinement-alpha.1.zip` (the development
candidate `a2f00e7b…3c5a` renamed, 373 members, Info.json 0.0.157, verified
after download), `release-manifest.json` and `SHA256SUMS.txt`. The full test
pass was not run. The separate `v0.0.157-brown-fur-alpha.1` prerelease, from
another lane, is a different build; neither alpha contains the other's changes.

## Baseline

- `master` = `origin/master` = `2eed3944f6eab657f9030d33acae6d81eb5f10ae`,
  version 0.0.157, clean (re-read 2026-10-17).
- Open PRs: only #49, draft, Sprint 25 Brachiosaurus and Elephant, base
  `master` `2eed3944f6`. Re-read at its head
  `ee5d6fd0cd992be97df3d90f4c384fed4e6b45fc` (2026-10-17, 123 files): it
  overlaps this branch in 14 files - both icon manifests
  (`assets-source/.../expanded-summoning/icon-manifest.json`,
  `assets/game/icons/expanded-summoning/icon-manifest.json`), the icon catalog
  and `tools/test_icon_catalog.py`, `scripts/Build-Local.ps1`,
  `scripts/validate-package.ps1`, the mod project, the domain-test project and
  `Program.cs`, and `ExpandedSummoningSprint14Tests.cs`, `Sprint16Tests.cs`,
  `Sprint23Tests.cs`, `Sprint24Tests.cs` and `Sprint20RulesTests.cs`. (The
  earlier note at head `e7d35f17c3` named only the Sprint 24 test file.)
  Integration must keep both the approved Dire Rat edit and Sprint 25's icon
  additions, regenerate the shared provenance manifest and repin its hash in
  the icon catalog and `tools/test_icon_catalog.py`.
- Branch `codex/expanded-summoning-visual-refinement-pilot`, worktree
  `C:\Dev\KingmakerGunslingerLab\worktrees\expanded-summoning-visual-refinement-pilot`.
- Machine-local preconditions copied into the worktree, gitignored, never
  committed: `GamePath.props` and the `artifacts/inspection/bodyguard-native`
  IL dump.
- Tool: Blender 4.5.10 LTS, official portable zip (SHA-256
  `ef6d846b8015f47ade6df3f9322ce17419080a5d922fa562b6c966064fe30dce`) at
  `C:\Dev\.toolchains\blender-4.5.10-windows-x64`.

## Native baseline (before)

One release-path build of unchanged master (`Build-Local.ps1`, package
`06a04e27…bca1`, commit `2eed3944f6`), used only to observe the old assets and
take the bind captures. Explicit route: `Deploy-Local.ps1 -PassThru`,
`Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact` per scenario, then
`Restore-Live-Mod.ps1` in a `finally`; live tree 254 files `5C3E4AD4…BFBB`
before and after.

| Scenario | Evidence directory | Result |
| --- | --- | --- |
| observe-expanded-summoning-sprint24-theropod-census | `20261017T0224053339821Z-…` | PASS; Lizardfolk census |
| disposable-expanded-summoning-native-profile | `20261017T0225090480213Z-…` | FAIL on three pre-existing mechanics assertions; wrote the Dog, Wolf, Worg, Giant Poisonous Frog, Giant Spider and Pteranodon captures |
| working-save-expanded-summoning-creature-review (pilot keys) | `20261017T0226573833203Z-…` | FAIL only on the Tyrannosaurus observer limitation below |
| disposable-expanded-summoning-rules | `20261017T0228570223946Z-…` | FAIL on pre-existing trample/cloud/web/combat-mode assertions; wrote the Horse and Mastodon captures |

The mechanics failures are on the unchanged master build, outside this lane.
Captures (machine-local, `C:\Dev\KingmakerGunslingerLab\private\visual-refinement\captures`):
Dog `c651dda7…4270`, Mastodon `ce02946c…e414`, Horse `83540b02…1ef8`, Wolf
`76805093…4c7c`, Worg `b6c2d2cf…2fa8`, Giant Poisonous Frog `b54385e7…6379`,
Giant Spider `60074cc2…a19b`, Pteranodon `7566f6f3…2a84`, Lizardfolk census
`6443b580…5d`, Hodag survey `33ce3ad7…a1c1` (run 4).

## Native after-evidence

Every run: development candidate built with `Build-Local.ps1
-DevelopmentPurpose <p> -DevelopmentTestFilter '<focused filter>'
-ReferenceBundleDir <private refresh bundle>`, deployed purpose-bound, the
named disposable working save `KMG_AUTOMATION_WORKING` read and never written,
live mod restored exactly (254 files, `5C3E4AD4…BFBB` before and after).

| Run | Candidate | Scenario | Time | Result |
| --- | --- | --- | --- | --- |
| 1 | `5641fb79d0cf` (package `37ff4956…2f73`, 67 s build) | creature review: dire-rat, rhinoceros, tyrannosaurus | 3:11 | Rat and Rhinoceros PASS every assertion; Tyrannosaurus as below |
| 1 | same | disposable-expanded-summoning-visual-contracts | 1:16 | PASS |
| 2 | `ae09d7a9d55a` (package `af13f5df…dbe6`, 66 s build) | creature review: the six refined keys | 5:21 | five PASS every assertion; Tyrannosaurus as below |
| 2 | same | disposable-expanded-summoning-visual-contracts | 1:15 | PASS |
| 3 | `655d288eae21` (package `56ff1682…0ba2`, 69 s build) | creature review: five small quadrupeds, Rhinoceros, Woolly Rhinoceros | 6:26 | PASS (54 assertions) |
| 3 | same | disposable-expanded-summoning-visual-contracts | 1:20 | PASS |
| 4 | `c6d53f636527` (package `6788e161…88de`, 66 s build) | observe-expanded-summoning-sprint22-contracts (no save) | 0:58 | PASS; wrote the Hodag survey (`sprint22-ankylosaurus-contracts.json`, `33ce3ad7…a1c1`) for the Ankylosaurus |
| 4 | same | creature review: Hyena, Goblin Dog, Woolly Rhinoceros | 3:19 | PASS; close views now sharp and clear of walls |
| 5 | `7defdfb2f295` (package `f4aa1e03…50df`, 72 s build) | creature review: Deinonychus, Triceratops, Ankylosaurus | 3:04 | Triceratops and Ankylosaurus PASS every assertion; Deinonychus as the Tyrannosaurus below |
| 5 | same | disposable-expanded-summoning-visual-contracts | 1:16 | PASS |
| 6 | `dd82130f3890` (package `e4735192…d7d6`, 66 s build) | creature review: Crocodile, Dire Crocodile | 2:40 | PASS (22 assertions); solid and lit, no head hole |
| 6 | same | disposable-expanded-summoning-visual-contracts | 1:17 | PASS |
| 7 | `04144ec1457a` (package `8d65e0b3…a91a`, 65 s build) | observe-expanded-summoning-arachnid-census (no save) | 0:59 | PASS; retook the Sprint 20 census for the scorpion (`a62bb768…7269`) |
| 7 | same | creature review: the three ants and the Bebelith control | 3:17 | PASS |
| 7 | same | disposable-expanded-summoning-visual-contracts | 1:16 | PASS |
| 8 | `fba63c250028` (package `2c0ad6c4…fa1c`, 66 s build) | creature review: Giant Scorpion | 1:51 | PASS (9 assertions) |
| 8 | same | disposable-expanded-summoning-visual-contracts | 1:16 | PASS |
| 9 | `bde6b6d1bfe4` (package `f1e24baa…bf41`, 65 s build) | creature review: Dire Bat, Pteranodon, Giant Wasp | 3:18 | PASS (27 assertions, flight and doorway travel included) |
| 9 | same | disposable-expanded-summoning-visual-contracts | 1:15 | PASS |
| 10 | `69435567008c` (package `a99449ba…3168`, 66 s build) | observe-expanded-summoning-primate-census (no save) | 0:59 | PASS; census byte-identical to the recorded capture |
| 10 | same | creature review: Ape, Dire Ape, Xill (before) | 1:52 | PASS; the `xill` key attaches the Xill body |
| 11 | `cad14078e1cc` (package `deafd054…e15f`, 65 s build) | creature review: Ape, Dire Ape, Xill | 3:09 | PASS (13 assertions); the Ape's grey read as cream |
| 11 | same | disposable-expanded-summoning-visual-contracts | 1:15 | PASS |
| 12 | `a29f6edd5bff` (package `d36bb2b6…a517`) | creature review: Ape, Wolf, Ghaele | 2:21 | Ape and Wolf PASS; Ghaele FAIL (dissolving during the review); golden-rim evidence |
| 13 | `22cbb8a5695e` (package `73cfb282…9d7e`) | creature review: Wolf, Ghaele, Ice Mephit | 1:56 | Wolf and Ice Mephit PASS, Ghaele FAIL as in run 12; the glow patch on the SummonedUnitBuff changed nothing |
| 13 | same | disposable-expanded-summoning-visual-contracts | 1:19 | PASS |
| 14 | `dc5b84838cc1` (package `c2140645…f223`, diagnostics) | creature review: Wolf, Salamander | 1:51 | PASS; the SummonedUnitBuff spawns no effect, the glow is the SummonedCreatureVisual buff's; Salamander locomotion and material evidence |
| 15 | `01eb9e3aad53` (package `bc7ccc75…5d66`, 66 s build) | creature review: Wolf, Ghaele, Ice Mephit | 2:02 | Wolf PASS with the glow gone; Ice Mephit PASS, glow kept; Ghaele FAIL (own dissolve; move did not start) with the glow gone |
| 16 | `185e379a7418` (package `8db4fd08…9940`, 67 s build) | creature review: Salamander (refinement sequences added) | 2:04 | PASS (10 assertions); tail gait 334 frames, full speed, peak offset 0.12; close views show white streaks |
| 16 | same | disposable-expanded-summoning-serpentine-bodies | 1:28 | PASS 15/15: full native attack with two native spear clips and the owned tail slap, contact, destruction |
| 17 | `491e60249f43` (package `ef57b67b…5f47`) | the same two | 2:05, 1:26 | PASS; owned matte masks texture applied, streaks unchanged (reverted) |
| 18 | `c1b87bce72dc` (package `a26e0331…82e6`) | creature review: Salamander | 2:04 | PASS; cape rim keyword applied, streaks unchanged (reverted) |
| 19 | `58123c87fa9e` (package `6f778ca1…0812`) | the same two as run 16 | 2:03, 1:26 | PASS (10 and 15/15); body undissolved, streaks gone |
| 20 | `930474b4e924` (package `25f9ed3b…5295`, 113 focused build tests) | creature review: Eagle, Stirge, Fire Beetle, Giant Stag Beetle | 3:39 | PASS (31 assertions); repaired bodies solid and lit, the Fire Beetle's eyes glowing |
| 20 | same | creature review: Girallon, Giant Crab, Stegosaurus, Dire Rat | 3:40 | PASS (22 assertions) |
| 20 | same | disposable-expanded-summoning-visual-contracts | 1:17 | PASS (15) |
| 21 | `95a5c2e2f013` (package `a2f00e7b…3c5a`) | creature review: Stirge, Giant Crab | 2:50 | PASS (18): the Stirge's attached-prey review still PASS, then its full packet (9 stills, 108 tracking, 72 low side frames, 5.43 m); the Giant Crab's packet complete under the stricter check (9 stills, 108 frames, 4.91 m) |

The Tyrannosaurus assertion fails identically on unchanged master: the capture
check reads the first skinned renderer, which for the two-renderer Lizardfolk
donor is the suppressed `_ammunition04` accessory, and its scripted move stops
about 4 m short. The body is in frame, lit and intact. Pre-existing observer
limitation; not caused by the new mesh. The Deinonychus, on the same donor,
fails the same way in run 5 (`rendererEnabled=false`, `mesh=_ammunition04` in
all four captures, the move about 3 m short); its refinement sequences, close
views and cleanup pass and the body renders intact.

Findings from run 1, fixed before run 2: the Rhinoceros's 0.41 grey hide read
as tan in the game's warm torchlight (now a darker, slightly cool slate); and
most close views of the large pilots were drawn as the game's blue occluded-unit
silhouette behind the room's walls (the review now clips geometry in front of
the creature). Run 2 cleared the Aurochs and Bison views; run 3 showed the
rhinoceroses still partly walled at a bounding-sphere clip and the small
creatures blurred by the party camera's depth of field. Since `c6d53f6365` the
clip sits at the nearest corner of the creature's bounding box and depth of
field is off for each refinement render (restored after); run 4 confirmed
sharp, unwalled views (a straight-rear view of the Woolly Rhinoceros still
clips a doorway).

Evidence: the runs' directories under `runtime-evidence` (machine-local);
`<key>-refine-view-*.png` close views, `-refine-move-out/-back/-attack/-hit-*`
tracking frames, the review captures and the donor bind captures.

## What changed

- `assets-source/original-models/continuous-bodies/`: body construction,
  weighting, painting, outward export, review and shipped-bytes renderers,
  orientation audit. Since the ungulate batch also `Volume.shell` and `relax`
  (masses meeting over tenths of a metre are blended on a coarser union) and
  `fan_triangles` dropping zero-volume duplicate faces. The Dire Rat and
  Tyrannosaurus rebuild byte-identically with the extended library.
- Builders: `build_dire_rat.py`, `build_rhinoceros.py` (Rhinoceros and, with
  `--kind woolly-rhinoceros`, the Woolly Rhinoceros), `build_cattle.py` (Aurochs
  and Bison), `build_tyrannosaurus.py`, `build_worg_frame.py` (Hyena, Goblin
  Dog, Wolverine, Shadow Mastiff), `build_poisonous_frog.py`,
  `build_deinonychus.py`, `build_triceratops.py` and `build_ankylosaurus.py`.
  The library's `tube` carries ring frames along a bent path (the
  Deinonychus's folded arms). The crocodilians were a targeted repair in
  their own generator (`dd82130f38`): the same geometry, UVs and weights,
  exported outward after the shared writer, and a lighter, irregular
  painting. The ants (`04144ec145`) and the scorpion (`fba63c2500`) were
  targeted repairs in their generators too: outward export through the
  library's `restore_outward_winding`, a parallel-transport `carried_tube`
  where the shared tube's +Y frame hint folded body-axis tubes, and rooted
  joins (ant thorax and mandibles, scorpion telson). The flyers
  (`bde6b6d1bf`) are continuous bodies with membrane wings: the library's
  `sheet` closes a span-by-chord grid into a thin solid and `membrane_grid`
  keeps the first flyers' weighting, so the eagle's wing animation folds it.
  The primates (`cad14078e1`, `a29f6edd5b`) are continuous bodies on the
  Troll guard frame binding exactly the 56 reviewed drivers; the primate
  attachment's winding marker rule now requires `outward-visual-refinement`
  of the Ape, Dire Ape and Xill and keeps the old marker for the unchanged
  Girallon. The legacy generators refuse the kinds
  they no longer build and name the new builders; their helpers stay for the
  creatures and families that still import them.
- Current identities (SHA-256; full hashes in each family `SOURCE.md` and the
  ledger):

  | Asset | Mesh | Albedo | Vertices / triangles / parts |
  | --- | --- | --- | --- |
  | Dire Rat | `cdc262ab43da…3fbf4` | `746cb3c4b0c4…6f422` | 4,899 / 6,452 / 16 |
  | Rhinoceros | `3bcd3bcfa0a7…b5224` | `e8d172fdb09e…70bbf` | 5,948 / 7,728 / 20 |
  | Woolly Rhinoceros | `a53793a0283f…a1265` | `fcdfb0c4e092…4e2c6` | 5,951 / 8,128 / 20 |
  | Aurochs | `e1cd846250eb…cc3e9` | `78d99e63a792…93e01` | 6,365 / 9,280 / 17 |
  | Bison | `7195063b5a13…f3641` | `1d7b9e28ba28…30441` | 7,226 / 10,480 / 17 |
  | Tyrannosaurus | `d9fe3f0ac21d…fe320` | `ff2b250fd753…7c7af` | 7,093 / 8,956 / 50 |
  | Hyena | `bb02ff0e93fe…b07ef` | `0b92a99aeb02…7d404` | 6,088 / 8,122 / 28 |
  | Goblin Dog | `fa07b9407427…9710b` | `dc8a53f09f0c…5789c` | 6,052 / 7,644 / 27 |
  | Wolverine | `f2c4385b53a3…ffb6d6` | `131a49db8264…22bd7` | 6,121 / 8,124 / 28 |
  | Shadow Mastiff | `fdb05b264035…51862` | `f335df805a3e…84246` | 6,160 / 8,044 / 27 |
  | Poisonous Frog | `17529a2781d6…9d79a` | `ce040536c0a3…6211e` | 5,071 / 6,976 / 3 |
  | Deinonychus | `b085f640a33b…ecdf6` | `9e60ec305e08…8780f` | 7,037 / 9,192 / 46 |
  | Triceratops | `f99f5d350a3a…f49fa` | `eb458f118a7f…7f9d0` | 7,618 / 10,356 / 30 |
  | Ankylosaurus | `3eb7585e6f41…a9ca9` | `2cdd8bc7e913…98bac` | 11,520 / 14,560 / 75 |
  | Crocodile | `6615b7680876…96ef9` | `108668f562d9…37c85` | 2,342 / 4,188 / 124 |
  | Dire Crocodile | `d4507796f688…daf1e` | `86488f581289…62d3c` | 2,342 / 4,188 / 124 |
  | Giant Ant (Worker) | `e6e3df7b4771…500fb` | unchanged | 890 / 1,720 / 15 |
  | Giant Ant (Soldier) | `0eb9a2ea8954…48d1a` | unchanged | 906 / 1,748 / 16 |
  | Giant Ant (Drone) | `2aa85130a354…f86e8` | unchanged | 1,066 / 2,052 / 20 |
  | Giant Scorpion | `1b48a74e4296…bba7f` | unchanged | 1,226 / 2,352 / 25 |
  | Dire Bat | `b04e9dd146b1…b7cce` | `ffb7ed2bcf2a…b9a4b` | 9,115 / 12,596 / 40 |
  | Pteranodon | `42627bc93837…24345` | `0d9e25209…37455` | 8,905 / 12,080 / 34 |
  | Giant Wasp | `a32d454cff58…205c5` | `2d26b8dad843…a8943` | 6,502 / 9,404 / 18 |
  | Ape | `2f0196eca297…631ee` | `85c7f480eeac…6c7d9` | 24 parts |
  | Dire Ape | `d0c1d0324cf7…8700a` | `37bdc23e705b…8e219` | 24 parts |
  | Xill | `d7a0cb0ade14…1a243` | `eca0206533fa…999fa` | 46 parts |

- Packaging (`7defdfb2f2`): `scripts/Build-Local.ps1` no longer rewrites the
  Ankylosaurus mesh to the released v0.0.151 CRLF bytes (it refused any other
  export); the refined body ships as committed and `scripts/validate-package.ps1`
  pins its mesh and albedo. The Sprint 23 released-controls test follows.
- Domain tests: hash pins in the Sprint 9, 10, 11, 12, 13, 14, 16, 20, 23 and 24 visual tests and the Pteranodon mesh-data tests (whose atlas check now expects the single `body` region);
  `OriginalMeshOrientation` (in `ExpandedSummoningSprint12Tests.cs`) requires
  closed, consistently wound, outward components lit by agreeing normals for
  every refined body (all four ungulates now), with synthetic positive and
  negative fixtures.
- `RuntimeTestRunner.ExpandedSummoningCreatureReview.cs`: for refined keys in
  single mode, the creature review also records the donor's pre-swap bind
  frame, nine close still views around the creature's own facing (near clip at
  its bounding box, depth of field off), and tracking frames of a native move
  out and back, an attack and a hit reaction. Camera, near clip, depth of field,
  render targets and pause state are restored. Keys: every refined creature.
- `tools/validate_development_candidate.py`, its fixtures and
  `docs/DEVELOPMENT-CANDIDATES.md` (owner decision above).
- The summoned-creature glow (`22cbb8a569`, `dc5b84838c`, `01eb9e3aad`):
  `Summoning/ExpandedSummoningSummonGlowPatch.cs` and `SummonGlowPolicy.cs`.
  The base game's summoned unit blueprints carry a permanent
  SummonedCreatureVisual buff (`706c182e86d9be848b59ddccca73d13e`) whose
  effect loops a golden rim light and two warm tints on the materials; KMG
  units cloned from those blueprints inherit it. A postfix on
  `Buff.SpawnParticleEffect` for that buff or the SummonedUnitBuff marks the
  visual buff's rim and tint animations finished for units summoned through a
  KMG ability (spell variants, native options, adapters) or KMG summon units,
  unless the unit has a registered rim variant (the mephits and the lion keep
  the glow their look is built on). The material controllers then revert to
  the unit's own rim and tint. The setups stay enabled because the effect
  object is pooled, and nothing is destroyed. Base-game summons are unchanged.
  The first version patched only the SummonedUnitBuff, which spawns no effect
  (run 13 showed no change; run 14's diagnostics found the real buff). The
  creature review records `summonGlow=` and `buffEffects=` per unit.
- The Salamander (`185e379a74`, `58123c87fa`; details in
  `sprint17-serpents/SOURCE.md`): `Summoning/SalamanderTailGait.cs` and the
  human attachment's `LateUpdate` move the ten owned tail bones along a
  horizontal travelling wave while the slap is not playing (0.04 at idle,
  0.12 at 2 m/s; waist and heights fixed; `check_tail_gait.py` checks the
  shipped mesh offline), and the owned body material drops the frozen
  emissive dissolve edge it inherited before the material controller adopts
  it. The mesh, paint, spear, slap clip and every mechanic are unchanged. The
  creature review gives the Salamander close views and tracking frames and
  records `tailGait=` and the material's roughness and dissolve terms.
- The Dire Rat icon (`8ff9893b2f` pilot, adopted in `b6af1725ff` on the
  owner's approval): `assets-source/original-icons/expanded-summoning/edits/dire-rat/`
  crops the original painting closer on the rat and fades the medallion into
  a dark vignette. `tools/New-ExpandedSummoningIcons.ps1` reads approved edits
  from a per-key map; the original `sources/dire-rat.png` stays byte for byte
  and the provenance manifest records it as `originalSourceFile`. New 128 px
  export `15970071…7b07`; the icon catalog and `tools/test_icon_catalog.py`
  repin the provenance manifest (`35a64cb6…79ae`).
- Outward repairs (`930474b4e9`): `continuous-bodies/repair_outward_winding.py`
  repairs the Eagle, Stirge, Fire Beetle, Giant Stag Beetle, Girallon, Giant
  Crab and Stegosaurus first exports in place (each triangle wound to agree
  with its authored normals; closed parts left with negative volume turned,
  normals included). The Girallon and Giant Crab declare the outward marker,
  which `PrimateVisualPolicy` now requires of the Girallon as well.
  `RepairedBodyTests` pins the seven; `scripts/validate-package.ps1` and the
  Sprint 23 test repin the Girallon, Giant Crab and Stegosaurus. Run 20 gave
  close views and tracking sequences for six of the seven; a single Stirge
  review went from its attached-prey phase straight to cleanup, so its
  refinement packet was first produced in run 21 after the route fix below.
  Details and before/after hashes in each family `SOURCE.md` and the ledger's
  `additionalRepairs`.
- Review harness (`95a5c2e2f0`, after the independent review of
  `17eb252777`): a single Stirge review now enters the refinement phase after
  its attached-prey phase (which restores the prey and detaches the Stirge)
  and before cleanup. `RuntimeTesting/VisualRefinementReviewPolicy.cs` (moved
  out of the runner into a plain file) defines a complete packet: all nine
  close stills written, all 108 tracking frames, for the Stirge 72 low side
  frames through the two move legs, and at least 0.75 m of travel. The
  assertion names whatever is missing; before, it counted only the tracking
  frames and travel, so a packet with failed stills could pass.
  `VisualRefinementReviewTests` pins the Stirge route and fails packets with a
  missing still, frame or travel.

No identity, GUID, blueprint, statistic, size, reach, collision, attack,
timing, AI, duration, template, adapter, material path or runtime binding code
changed. Mesh-data schema 2 is unchanged.

### Internal visual review round (pilot)

A read-only reviewer agent returned CHANGES_REQUIRED on the first rebuild; all
findings were addressed in `feb879d19e` (details in each family `SOURCE.md`
and the ledger): offline-preview deform flags, Rat forelegs/tail/colour,
Rhinoceros hood/muzzle/ears/horns, Tyrannosaurus arm share/thighs/neck.

## Validation

| Check | Command | Time | Result |
| --- | --- | --- | --- |
| Focused gate (pilot, `feb879d19e` content) | `scripts/Invoke-KmgGate.ps1 -Filter 'sprint12-hidden-foundation;sprint11-original-ungulate-visuals;sprint24-original-art;pteranodon.'` | 3.1 s | production compilation PASS; 6 PASS |
| Development validator fixtures | `python tools/test_development_candidate_validator.py` | 1.0 s | 23 PASS |
| Development repository wrapper | `scripts/validate-repository.ps1 -DevelopmentPurpose visual-refinement-pilot` | 36 s | PASS (63 development-candidate contract checks) |
| Focused gate (ungulates, `ae09d7a9d5`) | the same filter plus `sprint11-donor` | 10.6 s | production compilation PASS; 7 PASS |
| Determinism | each builder twice from clean Blender processes, `PYTHONHASHSEED=0` | 7-17 s per creature | byte-identical mesh and albedo for all six |
| Exporter guard and audit | every build | - | all components closed, consistent, outward, normals agreeing |
| Native | runs 1 and 2 above | 7 min each | as tabled |
| Focused gate (armored dinosaurs, `7defdfb2f2`) | `scripts/Invoke-KmgGate.ps1 -Filter 'sprint12-hidden-foundation;sprint13-original-visuals;sprint11-original-ungulate-visuals;sprint11-donor;sprint22-original-asset;sprint23-original-art;sprint23-released-controls;sprint24-original-art;pteranodon'` | 11.6 s | production compilation PASS; 12 PASS |
| Native | run 5 above | 4:46 | as tabled |
| Focused gate (crocodilians, `dd82130f38`) | the armored-dinosaur filter plus `sprint16-original-assets;sprint16-bone-policy` | 11.5 s | production compilation PASS; 14 PASS |
| Determinism (crocodilians) | old generator reproduced the shipped bytes; new painter and generator twice from clean Blender processes | 25 s per creature | byte-identical |
| Native | run 6 above | 4:22 | as tabled |
| Focused gate (ants, `04144ec145`) | Sprint 14 six-legs, painting, allowlist, corrupt-mesh and loaded-insect cases plus the visual filter | 11 s | production compilation PASS; 17 PASS |
| Focused gate (scorpion, `fba63c2500`) | adds `sprint20-original-body;sprint20-review-wired;sprint20-census-wired` | 10 s | production compilation PASS; 14 PASS |
| Determinism (ants, scorpion) | old generators reproduced the shipped bytes; new exports twice from clean Blender processes | 10-15 s per creature | byte-identical |
| Native | runs 7 and 8 above | 6:00 and 3:34 | as tabled |
| Focused gate (flyers, `bde6b6d1bf`) | adds `sprint10-wasp-original-visual;sprint9-dire-bat-asset;pteranodon` | 10 s | production compilation PASS; 10 PASS |
| Determinism (flyers) | each builder twice from clean Blender processes; the library refactor rebuilt the Dire Bat byte-identically | 25-40 s per creature | byte-identical |
| Native | run 9 above | 4:59 | as tabled |
| Focused gate (primates, `cad14078e1`, `a29f6edd5b`) | `sprint18-;sprint19-` plus the visual filter | 11 s | production compilation PASS; 36 then 29 PASS |
| Native | runs 10-12 above | 3:17, 4:51, 2:48 | as tabled |
| Focused build tests (glow, `01eb9e3aad`) | `Build-Local.ps1 -DevelopmentPurpose visual-refinement -DevelopmentTestFilter 'summon-glow'` | 66 s | production compilation PASS; 2 PASS |
| Native | runs 13-15 above | 3:15, 1:51, 2:02 | as tabled |
| Focused gate (Salamander, `58123c87fa` content) | `scripts/Invoke-KmgGate.ps1 -Filter 'salamander-tail-gait;salamander-undissolved-body;sprint17-salamander;sprint17-human;summon-glow'` | 8-10 s | production compilation PASS; selected tests PASS |
| Focused build tests (run 19 candidate) | `Build-Local.ps1 -DevelopmentPurpose visual-refinement -DevelopmentTestFilter 'salamander-tail-gait;salamander-undissolved-body;sprint17-salamander;summon-glow'` | 67 s | 42 PASS |
| Offline tail gait | `blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint17-serpents/check_tail_gait.py -- assets/sprint17-serpents/salamander-human-mesh.json` | 6 s | PASS |
| Native | runs 16-19 above | 3:32, 3:31, 2:04, 3:29 | as tabled |
| Icon checks (Dire Rat, `b6af1725ff`) | `python -B tools/validate_icon_catalog.py`; `python -B tools/test_icon_catalog.py`; `tools/New-ExpandedSummoningIcons.ps1 -VerifyOnly` (Windows PowerShell 5.1); gate filter `expanded-summoning.icon-;ledger-icons;sprint12-;sprint16-icon;sprint17-snake-icon;sprint10-wasp-published-icon;sprint23-released;sprint24-` | 6 s, 6 s, 5 s, 10 s | PASS; 29 catalog fixture tests |
| Focused gate (repairs, `930474b4e9` content) | `scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.'` | 16 s | production compilation PASS; 467 PASS |
| Focused build tests (run 20 candidate) | `-DevelopmentTestFilter 'repaired-first-exports;sprint18-;sprint19-;sprint21-;sprint23-;sprint14-;sprint10-;icon-'` | 66 s | 113 PASS |
| Native | run 20 above | 8:36 | as tabled |
| Focused gate (harness, `95a5c2e2f0`) | `scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.'` | 16 s | production compilation PASS; 469 PASS (two new harness cases) |
| Focused build tests (run 21 candidate) | `-DevelopmentTestFilter 'refinement-;repaired-first-exports'` | 66 s | 3 PASS |
| Native | run 21 above | 2:50 | as tabled |
| Development repository wrapper (`95a5c2e2f0` content) | `scripts/validate-repository.ps1 -DevelopmentPurpose visual-refinement` | 41 s | PASS |
| Development repository wrapper (`930474b4e9` content) | `scripts/validate-repository.ps1 -DevelopmentPurpose visual-refinement` | 35 s | PASS (63 development-candidate contract checks, icon catalog and fixtures) |

## Remaining limitations

- Tyrannosaurus and Deinonychus: the creature-review observer reads the
  suppressed accessory renderer and its move stops short; a pre-existing
  observer limitation, to be fixed in the observer, not the mesh.
- Triceratops: the Huge body fills its close views, and the near clip at its
  bounding box cuts into the room around it; an observer framing limit.
- Ghaele: its materials dissolve progressively while it is reviewed (0.05,
  0.133, 0.175 in run 12; 0.009, 0.04, 0.118, 0.156 in run 15, with a gold
  dissolve colour), which fails the review's intactness assertion; in run 15
  its scripted move also did not start. The dissolve's source is not
  identified and is left unchanged; the glow change does not touch it.
- Salamander: 19 of 12,924 vertex-normal corners of the shipped human/tail
  export oppose their triangles (a small pleat in the waist seam, two
  left-hand fingers); a normal-only repair cannot light the pleat and a
  geometric one needs the private human capture, which is not on this
  machine. The gait is a horizontal wave on the authored coil; the coil does
  not straighten behind the body while travelling.
- Frost Giant and Thanadaemon: the glow policy covers their KMG native
  options and adapters, but the creature review cannot cast them, so whether
  their base blueprints carry the SummonedCreatureVisual buff is unobserved.
- Flyers: the refinement close views frame the whole wingspan, so the camera
  sits far back and the room's walls hide most of them; the party-camera
  frames show the bodies. The idle and attack poses are the Giant Eagle's.
- Ants: the six legs still follow the Giant Spider's chains, so they keep
  the donor's splayed stance; moving the leg geometry off those chains would
  leave the limbs away from where the animation plants the bones.
- Dire Crocodile: at its 2x Gargantuan view scale several close views put
  the body behind the room's walls (the game's blue occluded-unit
  silhouette); an observer framing limit.
- Repaired bodies: where a repaired first export's authored normals
  disagree with its topology, a few edges stay one-way (the Giant Crab alone
  is strictly closed and consistent); every repaired body is lit from the
  front and renders solid. The Bebelith, the owner's accepted control, still
  ships its unchanged inward first export. The Stegosaurus was missed from the
  first list of inside-out bodies and found in the final audit.
- Stirge wings (run 21): an offline backface-culling inspection raised wing
  gaps as a lead. Natively, the outer face of the near wing (low side
  frames), the inner faces of both wings (front and rear stills) and their
  upper surfaces (top still) render as continuous solid panels, with nothing
  seen through them; no asset correction was made. Limits: the Stirge spans
  only about 60-100 px in these frames (its framing follows renderer
  bounds), several low side frames are walled (the game's blue occluded-unit
  silhouette or an empty frame where the side camera stands behind a wall),
  and the wings stay largely raised through the move legs, so little beat is
  seen.
- Release handoff (independently confirmed against this branch). The active
  release validator `tools/validate_alternate_summoning_release157.py` fails
  this branch in three separate classes, each checked on its own:
  1. Inherited art: it dispatches the Sprint 24 art-owner check
     (`validate_expanded_summoning_sprint24154.validate_art_and_package_owners`),
     which requires the superseded Tyrannosaurus and Deinonychus bytes.
  2. Production-source scope: seven changed files lie outside its frozen
     v0.0.157 scope - `RuntimeTestRunner.ExpandedSummoningCreatureReview.cs`,
     `RuntimeTesting/VisualRefinementReviewPolicy.cs`,
     `Summoning/ExpandedSummoningSummonGlowPatch.cs`, `SummonGlowPolicy.cs`,
     `PrimateVisualPolicy.cs`, `SalamanderHumanVisualAttachment.cs` and
     `SalamanderTailGait.cs`.
  3. Exact project contract: four compile entries it does not accept -
     `SummonGlowPolicy.cs`, `ExpandedSummoningSummonGlowPatch.cs`,
     `SalamanderTailGait.cs` and `RuntimeTesting/VisualRefinementReviewPolicy.cs`.
  The four historical art contracts and what they expect: `sprint24154`
  (earlier Tyrannosaurus and Deinonychus hashes; the only one v0.0.157
  exercises against this tree), `sprint23152` (earlier Triceratops and
  Stegosaurus hashes; run only at Sprint 24's census stage), `sprint18147`
  (the earlier Ape and Dire Ape winding marker) and `sprint19148` (the earlier
  Girallon and Xill winding marker). They describe released versions and are
  not expected to pass against this tree; they stay as they are. The next
  release needs its own reviewed validation contract that admits the refined
  assets (with their new hashes and markers), these seven production files
  and four compile entries, and the record and icon-manifest changes - not
  edits to the historical files. The development wrapper runs only those
  validators' synthetic fixtures, which pass.
- Rhinoceros: the rump is still a broad, round mass from behind. Lowering the
  head by 30° about the Mastodon's high head pivot swings the skull under the
  chest offline; the native attack did not show it.
- Bison: lowering the head by 35° about the Horse's high neck pivots tucks the
  face toward the chest offline; to watch in native frames.
- The game lights albedos warmly and brightly; neutral greys are kept darker.

## Next

1. Release (owner): the lane is a pushed development checkpoint. Merge,
   version selection and the next release's validation contract (above) stay
   with the owner and the release lane; integration with Sprint 25 (#49) must
   keep both icon changes. The owner reviews everything in the game after the
   release and the lane refines what that review finds.
2. Still unresolved and kept visible: the Ghaele's own rising dissolve and its
   review move (glow removed, native visual review unresolved); the Frost
   Giant and Thanadaemon (covered by the glow policy, not observed natively);
   the Bebelith stays the accepted unchanged control.
3. Optional: Viper/Constrictor locomotion (VIS-007, VIS-018). Roster-owned:
   Elephant, Lantern Archon, Invisible Stalker.
