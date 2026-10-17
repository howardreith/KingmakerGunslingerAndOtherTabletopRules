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
| VIS-003 Woolly Rhinoceros | rebuilt; focused gate PASS | native after: PASS (run 2) | pending (style approved) |
| VIS-017 Aurochs | rebuilt; focused gate PASS | native after: PASS (run 2) | pending (style approved) |
| VIS-023 Bison | rebuilt; focused gate PASS | native after: PASS (run 2) | pending (style approved) |
| VIS-010 Hyena | rebuilt; focused gate PASS | native after: PASS (runs 3 and 4) | pending (style approved) |
| VIS-009 Goblin Dog | rebuilt; focused gate PASS | native after: PASS (runs 3 and 4) | pending (style approved) |
| VIS-014 Wolverine | rebuilt; focused gate PASS | native after: PASS (run 3) | pending (style approved) |
| VIS-033 Shadow Mastiff | rebuilt; focused gate PASS | native after: PASS (run 3) | pending (style approved) |
| VIS-006 Poisonous Frog | rebuilt and recoloured green and black; focused gate PASS | native after: PASS (run 3) | pending (style approved; the owner questioned the orange) |
| VIS-027 Deinonychus | rebuilt; focused gate PASS | native after: body in frame, lit and intact; the review assertion fails only on the Tyrannosaurus's observer limitation (same donor) (run 5) | pending (style approved) |
| VIS-002 Triceratops | rebuilt; focused gate PASS | native after: PASS (run 5) | pending (style approved) |
| VIS-028 Ankylosaurus | rebuilt, club bone-tan; focused gate PASS | native after: PASS (run 5) | pending (style approved) |
| VIS-012 Crocodile | targeted: exported outward, repainted; focused gate PASS | native after: PASS (run 6) | pending (style approved) |
| VIS-034 Dire Crocodile | targeted: exported outward, repainted; focused gate PASS | native after: PASS (run 6) | pending (style approved) |
| other 22 items | open, owned elsewhere or accepted control (see ledger) | owner screenshots only | pending |

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

## Baseline

- `master` = `origin/master` = `2eed3944f6eab657f9030d33acae6d81eb5f10ae`,
  version 0.0.157, clean (re-read 2026-10-17).
- Open PRs: only #49, draft, Sprint 25 Brachiosaurus and Elephant, targeting
  0.0.158, head `e7d35f17c38d1343d9195041bf66ad0ad06afd3d` (94 files,
  re-read 2026-10-17). The only shared file is
  `ExpandedSummoningSprint24Tests.cs`, in different hunks (#49 at 61-77,
  251-269 and 432-451; this branch at 319-339).
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
  painting. The legacy generators refuse the kinds
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

- Packaging (`7defdfb2f2`): `scripts/Build-Local.ps1` no longer rewrites the
  Ankylosaurus mesh to the released v0.0.151 CRLF bytes (it refused any other
  export); the refined body ships as committed and `scripts/validate-package.ps1`
  pins its mesh and albedo. The Sprint 23 released-controls test follows.
- Domain tests: hash pins in the Sprint 11, 12, 13, 16, 23 and 24 visual tests;
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

## Remaining limitations

- Tyrannosaurus and Deinonychus: the creature-review observer reads the
  suppressed accessory renderer and its move stops short; a pre-existing
  observer limitation, to be fixed in the observer, not the mesh.
- Triceratops: the Huge body fills its close views, and the near clip at its
  bounding box cuts into the room around it; an observer framing limit.
- Dire Crocodile: at its 2x Gargantuan view scale several close views put
  the body behind the room's walls (the game's blue occluded-unit
  silhouette); an observer framing limit.
- Found, not changed: these original bodies outside the owner's review list
  also ship inside out (all or most parts inward): Fire Beetle, Giant Stag
  Beetle, Giant Crab, Girallon, Eagle and Stirge, and the Bebelith control.
  The same outward-export repair would apply; it waits for the owner's word.
- Release lane: the released v0.0.155 validator (inherited by the v0.0.157
  release validator) still pins the superseded Tyrannosaurus and Deinonychus
  bytes; the next release's validator must accept the refined art.
- Rhinoceros: the rump is still a broad, round mass from behind. Lowering the
  head by 30° about the Mastodon's high head pivot swings the skull under the
  chest offline; the native attack did not show it.
- Bison: lowering the head by 35° about the Horse's high neck pivots tucks the
  face toward the chest offline; to watch in native frames.
- The game lights albedos warmly and brightly; neutral greys are kept darker.

## Next

1. Arthropods (in progress): the three ants (outward export; the Worker's
   thorax and the Soldier's and Drone's mandibles rooted in the head) and the
   Giant Scorpion (outward export; the telson, which stopped about 5 cm short
   of the tail, rooted on it). The Bebelith is the control: inspected, not
   changed.
2. Then, in order: flying (Dire Bat, Pteranodon, Giant Wasp), primates and
   four-armed (Ape, Dire Ape, Xill), the material and shine investigations
   (Wolf, Frost Giant, Ghaele, Thanadaemon), the Salamander, and the Dire Rat
   icon (VIS-004, icon rules).
   Optional: Viper/Constrictor locomotion. Roster-owned: Elephant, Lantern
   Archon, Invisible Stalker.
