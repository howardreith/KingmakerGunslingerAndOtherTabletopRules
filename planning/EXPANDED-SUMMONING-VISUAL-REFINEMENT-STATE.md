# Expanded Summoning visual refinement: lane state

Lane-specific resume record for the visual-refinement mission (handoff
`Expanded_Summoning_Visual_Refinement_Handoff.zip`, SHA-256
`521ced2667504e7693fa4b91b202a5cf8a5feba6bac4c1afed58f930a4b300eb`, prepared
2026-10-16). It does not replace any other lane's state. The issue ledger is
[EXPANDED-SUMMONING-VISUAL-REFINEMENT-LEDGER.json](EXPANDED-SUMMONING-VISUAL-REFINEMENT-LEDGER.json).
The method, the diagnosis and the regeneration commands are in
[continuous-bodies/SOURCE.md](../assets-source/original-models/continuous-bodies/SOURCE.md).

## Status at this checkpoint (2026-10-16, DATA)

Three statuses are kept apart. No test result changes the owner's acceptance.

| Item | Technical | Visual evidence | Owner acceptance |
| --- | --- | --- | --- |
| VIS-005 Dire Rat | source and asset rebuilt, revised after one internal review; focused gate PASS | native before; offline after on exact shipped bytes and posed source | pending |
| VIS-024 Rhinoceros | source and asset rebuilt, revised after one internal review; focused gate PASS | native before; offline after on exact shipped bytes and posed source | pending |
| VIS-001 Tyrannosaurus | source and asset rebuilt, revised after one internal review; focused gate PASS | native before; offline after on exact shipped bytes and posed source | pending |
| other 35 items | open, owned elsewhere or accepted control (see ledger) | owner screenshots only | pending |

**Native after-evidence has not run.** The rebuilt assets have not yet been
seen in the game. See "Open decision" below.

## Baseline

- `master` = `origin/master` = `2eed3944f6eab657f9030d33acae6d81eb5f10ae`,
  version 0.0.157, clean.
- Open PRs (re-read 2026-10-17 02:0x UTC): only #49, draft, head
  `e2871a6dc2d07e006916927e8fc440b884f48611`, Sprint 25 Brachiosaurus and
  Elephant, targeting 0.0.158. Of its 74 files this branch shares one:
  `tests/KingmakerGunslinger.DomainTests/ExpandedSummoningSprint24Tests.cs`,
  where #49 edits catalog counts (around lines 61-75 and 251-265) and this
  branch edits the Tyrannosaurus art pins (around 321-338). Different hunks.
  #49 does not touch any generator, asset runtime, Sprint 11/12 test, the
  creature-review runner or the development-candidate validator.
- Re-read before this checkpoint: `origin/master` unchanged; #49 head moved
  to `c31301832a24f3eb0caea9aaa55aaa1b80b6a210` (85 files). The shared file is
  still only `ExpandedSummoningSprint24Tests.cs`, in different hunks (#49 at
  61-77 and 251-269, this branch at 319-339). #49's `1515ca7e15` independently
  records the same development-candidate refusal at its version.
- Branch `codex/expanded-summoning-visual-refinement-pilot`, worktree
  `C:\Dev\KingmakerGunslingerLab\worktrees\expanded-summoning-visual-refinement-pilot`.
- Machine-local preconditions copied into the worktree, gitignored, never
  committed: `GamePath.props` and the `artifacts/inspection/bodyguard-native`
  IL dump (the same environment precondition recorded in
  `docs/ALTERNATE-SUMMONING-SOURCES.md`).
- Tool: Blender 4.5.10 LTS was not installed on DATA. The official portable
  `blender-4.5.10-windows-x64.zip` (SHA-256
  `ef6d846b8015f47ade6df3f9322ce17419080a5d922fa562b6c966064fe30dce`, matching
  download.blender.org's published checksum) is unpacked at
  `C:\Dev\.toolchains\blender-4.5.10-windows-x64`.

## Native baseline (before)

One release-path build of unchanged master in this worktree, used only to
observe the old assets and take the bind captures; it is not a release:
`Build-Local.ps1 -ReferenceBundleDir <private refresh-2026-10-15 bundle>`,
1 min 47 s, package `06a04e2777f910e5b348b26e7b2ac007adcc5092e4e2ee7b0039ea5d54f7bca1`,
DLL `29406df98182fcf9852fd1378e4d83ddd093c71410df00763edb279fe35e2270`,
MVID `d9d31de1-24a9-4496-a741-f6b695546264`, commit `2eed3944f6`.

The scenario wrapper's `-PackagePath` route is refused by the launcher (that
branch requires `-ReuseInstalledArtifact`), so the session used the documented
explicit route: `Deploy-Local.ps1 -PassThru`, four
`Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact` runs, then
`Restore-Live-Mod.ps1` in a `finally`. Live tree before and after:
254 files, `5C3E4AD4…BFBB` both (exact restore). Total 8 min 58 s.

| Scenario | Evidence directory | Time | Result |
| --- | --- | --- | --- |
| observe-expanded-summoning-sprint24-theropod-census | `20261017T0224053339821Z-…` | 1:04 | PASS; wrote the Lizardfolk census |
| disposable-expanded-summoning-native-profile | `20261017T0225090480213Z-…` | 1:48 | FAIL on three pre-existing mechanics assertions on unchanged master (caster-level duration 0/248; Sprint 14 trip defences; Sprint 15 defences); wrote the Dog capture |
| working-save-expanded-summoning-creature-review (dire-rat, rhinoceros, tyrannosaurus) | `20261017T0226573833203Z-…` | 2:00 | FAIL only on the Tyrannosaurus capture check: it reads the first skinned renderer, which for the two-renderer Lizardfolk donor is the deliberately suppressed `_ammunition04` accessory (`rendererEnabled=false`); the body is in frame, lit and intact. Observer limitation, pre-existing |
| disposable-expanded-summoning-rules | `20261017T0228570223946Z-…` | 3:42 | FAIL on pre-existing trample/cloud/web/combat-mode assertions on unchanged master; wrote the Mastodon capture |

The mechanics failures above are on the unchanged master build, outside this
lane and not investigated here. The baseline renders confirm the export defect
in game: `rhinoceros-review-oblique-south.png` shows the selection outline
enclosing floor seen through the head and a leg.

Captures (machine-local, `C:\Dev\KingmakerGunslingerLab\private\visual-refinement\captures`):
Dog `c651dda7…4270`, Mastodon `ce02946c…e414` (the 26 controls of the attached
Woolly Rhinoceros body), Lizardfolk census `6443b580…5d`. Full hashes are in
each family `SOURCE.md`.

## What changed

- New pilot-only library `assets-source/original-models/continuous-bodies/`
  (body construction, weighting, painting, outward export, review renderer,
  shipped-bytes renderer and orientation audit). No legacy generator imports
  it; the shared exporter in `pteranodon/generate_pteranodon.py` is unchanged,
  so no other creature's generator output changes.
- New builders `build_dire_rat.py`, `build_rhinoceros.py`,
  `build_tyrannosaurus.py`; the legacy generators refuse those three kinds
  (three-line guard in each `main()`, nothing else changed).
- Regenerated `assets/sprint12-quadrupeds/dire-rat-*`, `assets/ungulates/rhinoceros-*`,
  `assets/sprint24-theropods/tyrannosaurus-*`. Every other packaged asset is
  byte-identical to master (unchanged controls: Hyena, Goblin Dog, Aurochs,
  Bison, Woolly Rhinoceros, Deinonychus and all other originals).
  Current identities (SHA-256, revised build):

  | Asset | Mesh | Albedo | Vertices / triangles / parts |
  | --- | --- | --- | --- |
  | Dire Rat | `cdc262ab43da…3fbf4` | `746cb3c4b0c4…6f422` | 4,899 / 6,452 / 16 |
  | Rhinoceros | `c6c1c9236236…ae969` | `91042f178fb2…1c584` | 5,948 / 7,728 / 20 |
  | Tyrannosaurus | `d9fe3f0ac21d…fe320` | `ff2b250fd753…7c7af` | 7,093 / 8,956 / 50 |

  Full hashes are in each family `SOURCE.md` and the ledger.
- Domain tests: new hash pins in the Sprint 11, 12 and 24 visual tests, plus
  `OriginalMeshOrientation` (in `ExpandedSummoningSprint12Tests.cs`), which
  decodes the shipped bytes and requires closed, consistently wound, outward
  components lit by agreeing normals, with synthetic positive and negative
  fixtures. No registration or project change.
- `RuntimeTestRunner.ExpandedSummoningCreatureReview.cs`: for the three pilot
  keys in single mode only, the creature review now also records the donor's
  pre-swap bind frame (Dog, Mastodon), nine close still views placed around the
  creature's own facing, and tracking frame sequences of a native move out and
  back (two turns), a native attack and a hit reaction. Camera, render targets
  and pause state are restored. **Compiled; not yet executed natively.**

### Internal visual review round

A read-only reviewer agent compared the first rebuild against the owner's
screenshots and returned CHANGES_REQUIRED. The findings and what was done
(commit `feb879d19e`):

- Frozen Tyrannosaurus arms, claws and rat jaw left behind in offline poses:
  a preview artifact. The heat solve clears the deform flag on bones it does
  not weight, and Blender then ignores their vertex groups; `heat_weights`
  restores the flag, so offline poses now show the weights that ship. The
  shipped weights were never affected.
- Dire Rat: thicker forelegs, tail runs back and down, darker underjaw, pink
  only on the naked tail.
- Rhinoceros: head sank under a skin hood when lowered (hard Head cut-off on
  the neck sides, chest dome wrapping the head's base; Neck had no skin);
  ball-shaped muzzle; ears sunk to nubs; flat-toned horns read as holes when
  seen end-on. Fixed with smooth Neck/Head ramps, a shorter chest dome, a
  narrower neck, one tapering skull with a square lip, painted nostrils and
  mouth line, taller ears and root-to-tip horn shading. Weight-painted
  close-ups confirmed each cause before the change.
- Tyrannosaurus: arms keep 16-34 % of their own swing (was 10-26 %), thighs
  tucked into the hips, softer neck-to-skull ramp.

Remaining from that review, for the owner: the Rhinoceros rump is still a
broad round mass from behind, and a head lowered by 30° about the Mastodon's
high Head pivot still swings the skull back under the chest; how far the
native attack actually lowers it is unknown until the native run.

No identity, GUID, blueprint, statistic, size, reach, collision, attack,
timing, AI, duration, template, adapter, material path or runtime binding
code changed. Mesh-data schema 2 is unchanged (an informational `winding` field
is added and ignored by the loader).

## Validation

| Check | Command | Time | Result |
| --- | --- | --- | --- |
| Focused gate (first rebuild, `4d9294916a`) | `scripts/Invoke-KmgGate.ps1 -Filter 'sprint12-hidden-foundation;sprint11-original-ungulate-visuals;sprint24-original-art;pteranodon.'` | 11.6 s | production compilation PASS; 6 of 2,646 registered selected; 6 PASS |
| Focused gate (after review, `feb879d19e` content) | the same | 3.1 s | production compilation PASS; 6 of 2,646 selected; 6 PASS |
| Negative probe (temporary, not committed) | the same invariant on the three superseded meshes from `HEAD` | - | all three rejected: Tyrannosaurus 37/37 inward, normal agreement 0.011; Rhinoceros 14/22 inward, 184 reversed-cap edges, 0.002; Dire Rat 12/23 inward, 210 edges, 0.009 |
| Determinism | each builder twice from clean Blender processes, `PYTHONHASHSEED=0` | 4-10 s each | byte-identical mesh and albedo for all three |
| Exporter guard | every build | - | all components closed, consistent, outward |

Offline evidence (machine-local, under the session scratchpad and
`private\visual-refinement`): matched before/after renders of the exact old and
new shipped bytes with back-face culling (party angle, rear, side); bind-pose
textured and clay views; offline poses that rotate donor bones (stride, head
turn, bite or head toss, and a 60° Tyrannosaurus arm swing). Offline poses are
authoring checks, not native animation evidence.

Deferred: native after-evidence (below); hit, death and despawn on the new
meshes; the `disposable-expanded-summoning-visual-contracts` regression run;
full domain suite, repository wrapper, package and any release qualification.

## Open decision (owner)

A development candidate is the only route that lets these unreleased assets
run natively without misrepresenting them as the 0.0.157 release, and it is
currently refused for two reasons:

1. `tools/validate_development_candidate.py` is pinned to the v0.0.156 baseline
   and refuses any 0.0.157 tree. `docs/DEVELOPMENT-CANDIDATES.md` already says
   to re-pin it to the released v0.0.157 commit (`8726ec737a`) "in the next
   branch that needs a development candidate, with its corruption fixtures".
2. It also enforces the released Sprint 24 art hashes through the frozen
   v0.0.155 validator, which pins the superseded Tyrannosaurus bytes. The same
   pin reaches the v0.0.157 release validator.

Both changes alter what a validator accepts, so this session's safety controls
stopped them (and the edits were fully reverted). They need the owner's
explicit decision: re-pin only (lets the Rat and Rhinoceros run, but only from
a commit whose Tyrannosaurus bytes are unchanged), or re-pin plus an explicit,
reviewed way to refresh released Sprint 24 art in a development candidate
(lets all three run on the current commit). With either, the next step is one
development build and one guarded creature-review run of the three pilot keys,
which also executes the new observer for the first time.

## Next

1. Owner decision above; then native after-evidence and the hit/death and
   visual-contracts checks on the candidate.
2. Owner aesthetic review of the pilot.
3. First family batch after acceptance: ungulates (Woolly Rhinoceros, Aurochs,
   Bison) on the Rhinoceros method, with Triceratops as the dependency control
   for the shared hoof helpers. The ledger's orientation audit shows 31 shipped
   original meshes still exported inward; only the Viper, Constrictor Snake and
   Salamander were exported outward. The owner accepted the two snakes'
   appearance, and the Salamander report concerns lit patches and sliding, not
   holes.
