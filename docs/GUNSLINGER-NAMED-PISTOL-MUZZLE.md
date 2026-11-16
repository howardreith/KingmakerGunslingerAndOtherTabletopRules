# The named pistols' muzzle position: an observer that compared two frames (REP-03)

Status: **development change on a review branch. Not merged, not released.** Branch
`codex/rep03-named-pistol-muzzle`, stacked on draft PR #172 at its accepted head `73c2a0e4bc` (which is stacked on
#171, #170 and #169). The published v0.0.191 does not contain it. Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T8, inventory record REP-03 (audit
finding GS-AUD-0047). Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`.

## Disposition

**Observer defect, corrected. The asset satisfies the established contract and was not changed.**

`pistol-authored-marker-contract` compared the root `Muzzle`'s position in the prefab root frame with `KMG_Muzzle`'s
coordinate in the source model's frame. Those are different frames for the two named pistols, so the comparison
failed. Compared in one frame, the root `Muzzle` of both Duelist's Rebuttal and The Last Word is at its authored
marker to within 0.0000001 m, the grip is at the declared held anchor and the bore points along the declared held
forward. No bundle, model, identity, variant mapping or gameplay changed.

One thing found on the way is recorded and not classified: the engine launches the firearm projectile from the
unit's left-palm locator, not from the weapon's muzzle. See "What supplies the projectile origin".

## The frames

| Frame | What it is | Where it appears |
|---|---|---|
| Source model frame | the model as its generator authors it: grip at the origin, muzzle on +Z, metres | `generate_firearm_pistol_variants.py`, `MARKERS` |
| Visual frame | the imported source model, as the child `Visual` of the prefab | `KMG_*` markers under `<prefab>/Visual` |
| Prefab root frame | the frame the game attaches to the hand | the root's own children `Muzzle`, `Grip`, `Butt`; `Visual`'s local placement |
| World | the scene | live samples of a wielded weapon |

The bundle builder (`tools/unity/BuildFirearmBundles.cs`) reads the imported markers, places `Visual` inside the root
and creates the root markers from the same source points (`TransformAnchoredPoint`). For the named pistols the spec is
`PiercingOneHandedHeld(MarkerAuthored(Anchored(...)))`: the Visual is rotated so that the source forward and up land
on the native one-handed held donor's firearm forward and up, and translated so that the grip lands on the donor's
held position (`WeaponPresentationDonorFrames`).

## The same point, frame by frame

Both pistols give the same numbers; they share their markers and their adapter. Values are from the corrected
scenario on candidate `381a748f8da0` (run `20261116T124509Z`) unless a source is named.

| Step | `KMG_Grip` / `Grip` | `KMG_Muzzle` / `Muzzle` | Source |
|---|---|---|---|
| 1. Authored, source model frame | (0, 0, 0) | (0, 0, 0.264) | the generator's `MARKERS`; its build report records the same for both variants |
| 2. Imported, Visual frame (`Visual.InverseTransformPoint(marker.position)`) | (0, 0, 0) | (0, 0.0000001, 0.264) | read from the loaded prefab. The FBX export (`axis_forward=-Z`, `axis_up=Y`) and Unity's import leave the coordinates as authored |
| 3. Visual placement in the prefab root frame | position (-0.003, -0.0209996, 0.002); rotation quaternion (-0.4603184, -0.439661, 0.7537291, -0.1633943); scale (1, 1, 1) | | read from the loaded prefab |
| 4. Authored marker carried into the prefab root frame (step 3 applied to step 2) | (-0.003, -0.0209996, 0.002) | (-0.1482618, -0.2356838, 0.0520571) | computed by the contract |
| 5. Root marker, prefab root frame (`root/Grip`, `root/Muzzle` local position) | (-0.003, -0.0209996, 0.002) | (-0.1482618, -0.2356837, 0.0520571) | read from the loaded prefab |
| 4 against 5 | difference 0 | difference 0.0000001 m | |
| 6. Wielded | the prefab root is a child of the right hand's `R_WeaponBone` at local position (0, 0, 0), rotation (0, 0, 0); live grip-to-muzzle distance 0.264 m | | run `20261116T124232Z` |

`Butt` follows the same way: authored (0, 0, -0.075), in the root frame (0.0382676, 0.0399902, -0.0122208), difference
0.

The old expectation, (0, 0, 0.264), is step 1. The observed value it was compared with,
(-0.148261815, -0.2356837, 0.0520571172), is step 5. They are 0.35 m apart and both are right.

An independent check of step 5, made before any code changed: the declared donor anchor plus 0.264 m along the
declared held forward, computed outside the game from the constants in `WeaponPresentationDonorFrames`
(`PiercingOneHandedHeldPosition`, `PiercingOneHandedHeldEuler`, the -0.468 up and +0.184 right correction), gives
(-0.14826184, -0.23568373, 0.05205714). The observed root `Muzzle` differs from that by 0.00000005 m.

## Why the old comparison failed

- 2026-08-15, `a2ab46363d` ("Add exact named pistol visual variants") added both pistols as
  `MarkerAuthored(Anchored(...))` with the Visual at the identity. The prefab root frame and the source model frame
  were then the same frame, and the assertion `Muzzle.localPosition == (0, 0, 0.264)` was written and held.
- 2026-08-21, `e7e333c8da` ("calibrate handgun held and firing rigs") wrapped both specs in
  `PiercingOneHandedHeld`, which places them in the native one-handed held donor frame, and rebuilt the bundle. The
  root markers moved with the Visual, as they should. The assertion was not revisited and has failed since. The
  firearms bundle has not changed since that commit.

The assertion therefore described the first design, not a defect in the second. Resetting the root `Muzzle` to
(0, 0, 0.264) would have put it 0.35 m from the muzzle of the model.

## The contract now

`Presentation/AuthoredMarkerRigContract.cs` (engine-free). A position is compared only with a position in the same
frame. Each relationship is its own assertion; the aggregate keeps the name `pistol-authored-marker-contract`.

| Assertion | Relationship | Where the expectation comes from |
|---|---|---|
| `pistol-authored-marker-set` | exactly one each of `KMG_Grip`, `KMG_Support`, `KMG_Butt`, `KMG_Muzzle` under `Visual` | the generator's marker contract |
| `pistol-required-transforms` | `Visual` and the root's `Grip`, `Muzzle`, `Butt` are present; the Visual placement is finite with positive scale | what the other relationships read |
| `pistol-support-target-policy` | no `SupportHandTarget` on the root | a one-handed rig (the bundle builder and the runtime rig check both refuse one) |
| `pistol-imported-marker-geometry` | the imported markers are the declared distances apart: grip to muzzle 0.264 m, grip to butt 0.075 m, butt to muzzle 0.339 m | the generator's source coordinates. Distances do not depend on the frame, so this needs no placement |
| `pistol-root-markers-in-held-frame` | each root marker is at its authored marker carried through the Visual placement into the prefab root frame, within 0.0001 m. Reports expected, observed and difference for each | the imported marker and the Visual placement, which are separate objects from the root marker |
| `pistol-held-donor-frame` | the root `Grip` is at the donor's held position within 0.0001 m, and the bore from `Grip` to `Muzzle` is within 0.05 degrees of the declared held forward | `WeaponPresentationDonorFrames`, the constants the builder is declared to use |

The tolerances are the ones already in use: 0.0001 m was the old assertion's, 0.05 degrees is the builder's own
solved-rotation tolerance. Nothing observed in a prefab supplies an expectation, and the observed coordinate appears
in no constant.

The original failure is kept as recorded: PR #172's runs `20261116T030922Z`, `20261116T031851Z` and
`20261116T052756Z` all show `exact=False` with the muzzle at (-0.148261815, -0.2356837, 0.0520571172) against
(0, 0, 0.264).

## What supplies the projectile origin

An object named `Muzzle` being in the right place does not show that anything fires from it. Traced:

- **The engine.** `ProjectileController.CreateView` (Kingmaker 2.1.7b, decompiled) sets the launch position to the
  transform under the launcher's view named by the projectile blueprint's `SourceBone`
  (`Launcher.View.transform.FindChildRecursive(SourceBone)`).
- **The firearm projectile.** `KMG_FirearmProjectile` is a clone of the native light crossbow's projectile
  (`FirearmProjectileBlueprints`). The new assertion `firearm-projectile-launch-source` reads it in the game:
  `sourceBone=Locator_PalmFX_Left`, corpulence offset 0, and none of the seven item variant prefabs carries a
  transform of that name.
- **The mod.** The root `Muzzle` is read by the load-time rig check (`FirearmAssetRuntime`, as the tip of the
  semantic frame) and by the evidence scenarios. Nothing in the mod spawns an effect at it.

So the prefab's `Muzzle` is a structural marker. It is not the projectile's launch origin, for the named pistols or
for any other firearm.

Where that source bone is relative to the muzzle, at the frame each attack acted (run `20261116T124232Z`; computed
from the live hierarchy by the engine's rule, not read from a launched projectile):

| Weapon | Source bone to muzzle | Along the bore |
|---|---|---|
| Service pistol (unaffected control) | 1.78 m | 1.50 m |
| Duelist's Rebuttal | 1.69 m | 1.47 m |
| The Last Word | 1.70 m | 1.48 m |
| Revolver | 1.53 m | 0.86 m |

The source bone is on the unit's left hand (`.../L_Hand/Locator_PalmFX_Left`), outside the weapon model, for all
four. This is the same for the control as for the named pistols, so it is not REP-03 and was not repaired here. What
a player sees of it was not examined: the mod intends to hide the cloned bolt's renderers
(`FirearmProjectileVisualPatch`), and whether that takes effect was not checked. It is recorded as NEW-09.

## What changed

Commits `cd1c1284de` and `381a748f8d`.

- `Presentation/AuthoredMarkerRigContract.cs`: the contract above, with small point and placement types so that it
  runs without the engine.
- `RuntimeTestRunner.cs`: `disposable-firearm-visual-rigs` reads each named pistol's markers, Visual placement and
  root markers and reports the six relationships and the aggregate; `firearm-projectile-launch-source` is new. The
  scenario goes from 67 assertions to 74.
- `WeaponPresentationEvidenceScenario.cs`, two additions to the existing handgun motion evidence:
  - each live firearm sample records where the engine's projectile source bone is relative to the muzzle;
  - the attack animation advances on Unity's scaled time, which stands still while the game is paused. In the
    disposable route the save loads paused, so every attack stayed at its first frame. When the game is paused at the
    first attack sample, the scenario now releases time during each attack sequence, holds the pause between them and
    restores it at cleanup. Where the game is not paused, nothing changes.
- Two loader-compat sessions: `named-pistol-marker-contract.json` and `handgun-motion-evidence.json`.

## Focused tests

`.\scripts\Invoke-KmgGate.ps1 -Filter 'named-pistol-muzzle;presentation-collection;presentation-audit;firearm-fit;weapon-visual;weapon-presentation'`:
production compiled; 40 of 3,041 registered tests selected, 0 failures (15 s). Both candidate builds ran the same
selection, 0 failures.

Nine tests are new (`named-pistol-muzzle.*`). They call the production evaluation. Expected positions are worked out
by hand for quarter-turn placements, where a rotation can be read off: a quarter turn about +Y carries source +Z onto
+X, so a source point (0, 0, z) lands at the placement's position plus (z, 0, 0).

| Test | What it shows |
|---|---|
| `declared-source-markers-are-the-generators-markers` | the source coordinates the contract declares are the generator's build report's, for both variants; the butt-to-muzzle length is 0.339 m |
| `valid-rig-in-a-rotated-frame-passes-for-both-variants` | a rig placed by a quarter turn and a translation passes all six relationships for Duelist and Last Word, with expected and observed reported in the prefab root frame |
| `raw-source-coordinate-is-not-the-root-position` | under that placement a root `Muzzle` at (0, 0, 0.264) fails, 1.9130844 m from the authored marker; with the Visual at the identity the same coordinate passes, which is the one case the retired comparison fitted |
| `displaced-root-muzzle-fails-despite-correct-marker-counts` | 0.01 m along the bore fails the root-marker relationship alone; 0.01 m to the side also puts the bore 2.169 degrees off; 0.00005 m passes |
| `missing-and-duplicated-markers-are-named` | `KMG_Butt` absent and `KMG_Muzzle` twice are named; the relationships that need them are reported as not evaluated; `Grip` is still evaluated and holds |
| `support-target-and-required-transforms-are-their-own-results` | a support target fails its policy alone; a missing root `Muzzle` is named; an unreadable prefab fails everything with its reason |
| `held-frame-is-judged-apart-from-marker-agreement` | a self-consistent rig 0.02 m off the anchor, or turned so the bore is 90 degrees off, fails the held frame and nothing else |
| `imported-geometry-must-keep-the-declared-distances` | a model placed at twice its size with the root markers following fails the geometry relationship alone |
| `combined-outcomes-report-each-prefab` | one sound and one displaced prefab are each reported; the aggregate names the failed relationship; no prefab inspected is not a pass |

## Native results

Disposable game copy, loader profile `owner-0.33.0`. Final candidate `381a748f8da0`.

**`disposable-firearm-visual-rigs`** (session `named-pistol-marker-contract.json`, no other mods, no save, run
`20261116T124509Z`, 78 s): **PASS, 74 of 74.** It failed 1 of 67 on both of PR #172's candidates.

| Assertion | Pistol.Duelist | Pistol.LastWord |
|---|---|---|
| `pistol-authored-marker-set` | 1, 1, 1, 1 | 1, 1, 1, 1 |
| `pistol-required-transforms` | present | present |
| `pistol-support-target-policy` | none | none |
| `pistol-imported-marker-geometry` | 0.264, 0.075, 0.3390001 m | 0.264, 0.075, 0.3390001 m |
| `pistol-root-markers-in-held-frame` | differences 0, 0.0000001, 0 m | differences 0, 0.0000001, 0 m |
| `pistol-held-donor-frame` | grip difference 0; bore 0.0000039 degrees | grip difference 0; bore 0.0000039 degrees |
| `pistol-authored-marker-contract` | `exact=True;failed=<none>;bothNamedPistolsInspected=True` | |

`firearm-item-visual-roster` (16 of 16) and `firearm-item-visual-bindings` (16 bound) pass as before. The same
scenario on the first candidate `cd1c1284de5e` (run `20261116T123018Z`) gave the same 74 of 74.

**`weapon-presentation-handgun-motion-evidence`** (session `handgun-motion-evidence.json`, the working-save fixture
and the owner's twelve other mods staged in the copy, run `20261116T124232Z`, 145 s): **PASS, 8 of 8.** 74 records
for six variants; the game's own attack command acted for all six; each of the four firearms fired exactly once with
no fault; all eight dual-wield layouts pass; cleanup exact.

| Weapon | Grip to muzzle, live | Bore against target direction, ready | at the acted frame | Muzzle leads grip toward target |
|---|---|---|---|---|
| Service pistol (control) | 0.4032 m | 0.9768 | 0.9926 | yes |
| Duelist's Rebuttal | 0.2640 m | 0.9899 | 0.9858 | yes |
| The Last Word | 0.2640 m | 0.9866 | 0.9821 | yes |
| Revolver (control) | 0.2859 m | 0.9834 | 0.9913 | yes |

The native shortspear control sits under `R_WeaponBone` at local position (-0.003, -0.021, 0.002) and rotation
(9.712032, 123.546196, 178.825317), which are the donor constants the adapter declares. The scenario's own threshold
for the acted frame is 0.95.

This agrees with the scenario's runs of 2026-08-21 on the same bundle bytes (`planning/WEAPON-PRESENTATION-MATRIX.md`,
V18 and V24: 8 of 8, minimum acted dot 0.977 and 0.978).

Two earlier runs of this scenario did not complete and are kept:

| Run | Candidate | Result | Why |
|---|---|---|---|
| `20261116T121455Z` | PR #172's `bb205b774b58` | FAIL, 3 of 8 | the game was paused after load; the hand position was identical across all 96 attack updates and no attack acted, for the native controls as well |
| `20261116T123150Z` | `cd1c1284de5e` | FAIL, 2 of 9 | with time left running after the first sequence, the service pistol fired and acted (dot 0.9959) but the game then discharged Duelist's Rebuttal before its sample; the empty-firearm guard refused its attack command |

## Identities

| Item | Identity |
|---|---|
| Product commits | `cd1c1284de` (contract, observer, tests, sessions), `381a748f8d` (time released during each attack sequence only) |
| Final candidate | `381a748f8da0`, 411 s; package SHA-256 `5f901dc59c38d4eb7c66a27c56a37262ea39330f6cfd67b4db5ea98d2123cacd`, DLL `69e3d4d5838de30ef662892196b1b1c9c1bc45a8fe1f5442b9f2421737115908` |
| First candidate | `cd1c1284de5e`, 418 s; package `2f616d352439736552047fecdecc51d3e81b2ee0ad95e5c9ae48744d5fed9d38`, DLL `a76f98d64c0b714d5eb1c5b42ffb692502ccbab3b5e52ae6707887a4326e67d5` |
| Firearms bundle | `assets/bundles/kingmakergunslinger.firearms`, SHA-256 `b3cffb49ba32af10db12470401a58f6dff0ead9f219f87e41d9ec138d62fbaeb`, unchanged since `e7e333c8da` |
| Source models | `pistol-duelist.fbx` `D39F645A949CC8F42386FE852C632A360B50F7E19C13BEDDEDA9714F01B8BBE3`; `pistol-last-word.fbx` `BB8CCB51034D2EE66C293E7E5D7BEEC3F0F17340CF7660DD21CB220091AFFDEB`; unchanged |
| Session files | `named-pistol-marker-contract.json` `7ece881f17a3d09e961c574a7e9f97ffbe051223537d55e01870af81620a50f2`; `handgun-motion-evidence.json` `68cd141f305419cd64db27149017862833e489680865e24b9d4e4abca618b6e5` |
| Harness | unchanged since `df7f11156a`; DLL `50989f667d8f3fcd4c77b59a3881a5b2d961cae0cfa2f6dc53f337e7aa38cbf5` |
| Fixture | the visual-rig session uses none. The handgun session uses copies of the two automation saves (working `b29c1ccb...`, baseline `cc7cbb0d...`) and twelve of the owner's mods from the copy's snapshot |

All five sessions reported the owner's 95 save files and installation unchanged and the registry restored.

Validation time: about 24 minutes (gate runs about 35 s in all; two candidate builds 418 and 411 s; visual-rig
sessions 73 and 78 s; handgun sessions 146, 100 and 145 s).

## Limits and untested claims

- Structural alignment and the live bore are established. How either pistol looks is not: the 74 contact sheets were
  written and not reviewed, and no visual approval is claimed.
- The projectile's actual launch position was not read from a launched projectile. The source-bone figures are the
  engine's rule applied to the live hierarchy at the sampled frame.
- Whether the firearm projectile is visible in flight, and so whether a player sees it leave from the left hand, was
  not examined (NEW-09).
- The live samples use the scenario's default Medium male actor. Other bodies, locomotion, transitions and reload
  are outside it.
- The handgun scenario ran with twelve of the owner's mods in the disposable copy; the visual-rig scenario ran with
  none. Neither ran through the live guarded launcher or in the owner's installation.
- The time-release in the motion scenario was exercised only where the game was paused at the first attack sample.
  The long-gun, spear and Eastern motion scenarios share that code and were not run.
- No failing marker relationship was produced in the game; those reports are covered by the domain tests.
- The complete domain suite was not run for this cut.
