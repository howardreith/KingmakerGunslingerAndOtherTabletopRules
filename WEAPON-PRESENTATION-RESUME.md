# Weapon Presentation Calibration Resume

## Exact state

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`.
- Starting master/origin SHA:
  `7af4375238b2492857a131eefdf909b38a000a05`.
- Branch: `codex/weapon-presentation-calibration`, based on that exact origin
  commit.
- Published baseline evidence commit:
  `baa426f491ad7a63a9a2dc52c7236e5f4c4b5afd`
  (`test(presentation): add guarded baseline visual evidence`).
- Published baseline documentation commit:
  `1d2a2c40fdaa911e7fe4d4b2f6c28852f8d8379f`
  (`docs(presentation): record baseline visual qualification`).
- Published native-donor diagnostic commit:
  `07c11236d2047af63fc6aeccfb51be99b06fe708`
  (`test(presentation): capture native donor frames`).
- Published native-donor documentation commit:
  `c1ea80690e51688e79e50f73cf42ef55c9333236`
  (`docs(presentation): record native donor frames`).
- Published semantic-frame/handgun commit:
  `e2aba9d24cebbf38aadc236044c84f641a69534c`
  (`fix(presentation): validate frames and calibrate handguns`).
- Published semantic-frame/handgun qualification documentation commit:
  `77f47341ae86bc40fe898886660572f6343a7c97`
  (`docs(presentation): qualify semantic frames and handguns`).
- Published guarded long-gun motion diagnostic commit:
  `0e9c2902b255f0e091093e10f03655965d441123`
  (`feat(presentation): add guarded long-gun motion evidence`).
- Published long-gun defect documentation commit:
  `e530a8ac0e3d79c671ed24e0f14e3a220908867f`
  (`docs(presentation): record long-gun motion defects`).
- Published long-gun calibration implementation commit:
  `b672406ebbb8af8340d723d074ff8a69bd0ffe25`
  (`fix(presentation): calibrate long-gun held and stored rigs`).
- Published long-gun qualification documentation commit:
  `3366a052b2037bbd0d796b776bca8feef68b0dce`
  (`docs(presentation): record calibrated long-gun qualification`).
- Published branched-spear thrust diagnostic commit:
  `4e66d30afb1030849f7dcedb61669f84d79bf7bb`
  (`test(presentation): capture branched-spear thrust frames`).
- Published branched-spear defect documentation commit:
  `d9ba208d5c30d29ee7cb9b8c41a1e78ba531cebc`
  (`docs(presentation): record branched-spear thrust defects`).
- Published branched-spear grip interpretation correction:
  `3ff4d4400d67c7efda25fbdd0c58870a96f3f0a4`
  (`docs(presentation): correct spear grip interpretation`).
- Published branched-spear calibration implementation commit:
  `a1d45c630502d873debf89ac56562568739c5d58`
  (`fix(presentation): calibrate branched-spear held and stored rigs`).
- Version remains `0.0.88`; do not bump until the complete cosmetic package is
  qualified.
- The current implementation adds a shared full-frame semantic contract,
  basis-calibrates the service Pistol and Revolver, and gives all three long
  guns deterministic source frames plus independent donor-calibrated held and
  stored presentation. All three branched spears now have mesh-grounded source
  frames, native-derived held/stored bases, and held-only support-hand IK.
  Eastern bundles expose complete frames but retain their known incorrect
  mappings until the next family-specific phase.

## Qualified unchanged baseline

- Repository validation: PASS.
- Complete Release domain suite: PASS, 1,163 tests / 0 failures.
- Release compilation: PASS.
- Clean Release package build and explicit package validation: PASS.
- `Build-Local.ps1`: PASS; no deployment performed.
- Runtime preflight: PASS, 112 checks.
- Native firearm donor/capability observer: PASS at
  `20260820T2220444335411Z-observe-native-firearm-rig-contracts`.
- Exact stored/held visual matrix: PASS at
  `20260820T2307109303617Z-weapon-presentation-evidence`:
  44/44 exact native models, 44 PNG/JSON pairs, 176 views, no blank or
  low-density sheets, exact request cleanup.
- Fixture runtime DLL SHA-256:
  `36B28263DA564418C1421F02847891BAD0C2C7A8B50F17A1CE9BB63E8C95CADA`.
- Fixture local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.
- Clean release/local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.

The baseline visibly reproduces pelvis-clipping/misdirected handguns,
low-diagonal torso/leg-crossing long guns, horizontal shoulder storage,
one-hand low spear idle/back mounting, and inconsistent Eastern blade planes.
See `WEAPON-PRESENTATION-JOURNAL.md` and matrix evidence key `V1`.

## Qualified native-donor diagnostic

The published diagnostic commit changes exactly two source/test files:

- `src/KingmakerGunslinger/RuntimeTesting/WeaponPresentationEvidenceScenario.cs`
- `tests/KingmakerGunslinger.DomainTests/WeaponPresentationMissionTests.cs`

They add six exact native controls and model-local bounds/locator reporting to
the existing guarded scenario. Runtime preflight passes 112 checks, repository
validation passes, the clean Release domain suite passes 1,163 tests / 0
failures, clean Release packaging and explicit package validation pass, and
`Build-Local.ps1` passes without deployment. The guarded Steam run passed at:

`20260820T2345261164438Z-weapon-presentation-evidence`

That run contains 56 exact stored/held pairs and 224 views. It proves native
crossbow forward `+Z` / up `+Y`; native Longspear forward `+Y`; native sword
forward `+Y` with blade plane `YZ` and blade normal `+X`; and distinct native
held/stored attachment transforms. It also passes a 6/6 held/stored local
geometry invariant using actual mesh-local bounds at a documented `0.00001`
tolerance. Runtime DLL SHA-256 is
`EA0774E877274437ED63CA94E55FB8FA50CE183C0DBBE566BD2E13EFE8A2617E`.
The local-runtime package SHA-256 is
`24DF7A916343948A8515FE699B14B367914D97FD8D062D1007B9D34212AB098A`.

## Qualified semantic-frame and handgun checkpoint

Commit `e2aba9d24cebbf38aadc236044c84f641a69534c` is published on the
mission branch. It requires identity roots, complete forward/secondary frames,
positive non-reflected hierarchy scales, polarity, support intervals,
renderer-bound endpoints, and independent held/stored transforms. The service
Pistol maps physical source `-Z/+Y` to donor `+Z/+Y`; Revolver maps physical
source `+X/+Y` to donor `+Z/+Y` from its actual component bounds. All three
bundles reproduced byte-identically across two Unity 2018.4.10f1 builds.

Repository validation, the clean 1,164-test Release suite, clean Release
packaging, strict validation, and Build-Local pass. Runtime evidence passes at
`20260821T0034448996480Z-weapon-presentation-evidence` (9/9; 224 views) and
`20260821T0040087523551Z-disposable-firearm-visual-rigs` (65/65). Package/DLL
SHA-256 are `32BAA60B1427EF9880DC986A8030D2E83EBEC42465079C8CA4E2E823232ABF13`
and `E6240EB8B5BE1B93FF7400F4EDCFD9479C7C795F8AB9F90AAF1F21D2A51BF421`.

## Qualified guarded long-gun motion diagnostic

Commit `0e9c2902b255f0e091093e10f03655965d441123` is published on the
mission branch. The request-gated `weapon-presentation-motion-evidence`
scenario uses the exact working save only as an area anchor, creates and
removes its own Medium male combat pair, issues real navmesh-ready native
`UnitAttack` commands, requires each animation to reach `IsActed`, and records
per-firearm discharge deltas. Its final guarded Steam run passes at:

`20260821T0210452969596Z-weapon-presentation-motion-evidence`

The run captured Musket, Blunderbuss, Rifle, and native Heavy Crossbow in
combat-ready plus nine fixed attack samples each: 40 PNG/JSON pairs and 160
views. All four commands were installed, start-ready, running, and acted. Each
firearm fired once, faulted zero times, and consumed its round. Exact cleanup
and automatic exit passed. Result/index SHA-256 are
`6955A29800592B07B0008051F7990F9E2D3B581C77DCD9D066E220B531B0F374`
and `B3D2348D4F6EA3AE2E782D533F63F1E3B590E41C4C1C68524AA0E03316AC80E4`.

Visual review proves the current long-gun defect in ready and acted-attack
states on the default Medium male: Musket crosses the pelvis/lower torso,
Blunderbuss spans the waist then upper torso/neck, and Rifle crosses the hips
then upper torso. The native control lifts along the shoulder line. This does
not claim reload, locomotion, storage-transition, sex, or size acceptance.

Current source gates pass: runtime preflight 115, repository validation,
1,164/1,164 Release domain tests, clean Release build, strict package
validation, and Build-Local. Package/DLL SHA-256 are
`8A01BB7E6B1952AEEA14A9675987EEB481BB7BFBA9F73E3807D322757F01A1D7`
and `2B7EC67E836D7290E71AE62D5621172F1231FC88340B23D2CE3118275158382E`.

## Qualified calibrated long-gun checkpoint

Commit `b672406ebbb8af8340d723d074ff8a69bd0ffe25` is published on the
mission branch. Musket, Blunderbuss, and Rifle now use deterministic physical
source measurements, canonical `+Z/+Y` semantic frames, identity equipment
roots, the measured native Heavy Crossbow held basis, plausible trigger-wrist
grips, renderer-bound butt/muzzle endpoints, and fore-end support targets.
`MusketBelt`, `BlunderbussBelt`, and the new `RifleBelt` independently use the
measured native stored basis.

The derivative FBX SHA-256 values are Musket
`C5E2EA93E903782BF3110E50C1D6677C4E7C109248651495192D8B6063F73A0A`,
Blunderbuss
`45DD00FD88D7CE1B66690E1A1B6FFE732A343F3C728D84B4FF8956F1F4F4197C`,
and Rifle
`9D9288D04DEED70A6CA7AA321A2107B0F482431A082A1E2EDF4B50CB14742072`.
The twice-reproduced firearm bundle SHA-256 is
`5FA2D053EDC75B8BC7F64C296CE8A4EBB166B4A9C956C0CCFE7278E5ABFCB49E`.

Repository validation, all 1,164 Release domain tests, clean Release build,
output and SoundBank checks, strict package validation, and Build-Local pass.
The exact tested package/DLL SHA-256 values are
`EBE81ABDF3879FCE501A9E9FB2AE71E214765274040F4165020AFDC21577FB2C`
and `AB69C222DCEF85D3DC819E3138C99B3D47808B72F299E1F0E860710C98D02BDA`.

Final exact-package evidence passes at:

- `20260821T0413290534687Z-weapon-presentation-evidence` (9/9; 56
  PNG/JSON pairs; 224 views; held/stored).
- `20260821T0416419128426Z-weapon-presentation-motion-evidence` (6/6;
  40 PNG/JSON pairs; 160 views; ready plus nine acted-attack samples).

Every firearm fired exactly once with no fault, and the visible physical
muzzle/bell led toward the target. Across ten motion samples, support-hand
distance averages were Musket `0.131895 m`, Blunderbuss `0.125596 m`, Rifle
`0.133205 m`, and native Heavy Crossbow `0.132578 m`. Visual review accepts
dominant-hand grip, support hand, stock/shoulder, visible direction, and
independent stored presentation for the captured default-Medium-male states.
Reload, locomotion/turning, transitions, armor/cloak, female, Small, and
Enlarged coverage remain open.

## Qualified branched-spear thrust diagnostic

Commit `4e66d30afb1030849f7dcedb61669f84d79bf7bb` is published. The guarded
`weapon-presentation-spear-motion-evidence` scenario covers Classic, Thorn,
Crown, and native Longspear in combat-ready plus nine fixed native-attack
samples on a disposable default Medium male. It resolves physical endpoints
from renderer-bound custom `Tip`/`Butt` markers and the native
`TH_LongspearKnight1` mesh, and fails unless every acted sample leads with the
physical tip.

Repository validation, all 1,164 Release domain tests, clean Release build,
strict package validation, and `Build-Local.ps1` pass. The exact package/DLL
SHA-256 values are
`F65B2036F42435D41865A63D997CBE5F65404ACEF9EDAFA00F6BCF08D62CEEE6`
and `4969DC1CC84D6B699C8CF89F4AC8832603264691447FF0210C795AB45F86CC65`.
Final guarded Steam evidence passes 6/6 at
`20260821T0448131191263Z-weapon-presentation-spear-motion-evidence`: 40
PNG/JSON pairs, 160 views, 14/14 acted samples tip-leading, exact cleanup, and
no save call. Result/index SHA-256 are
`4CC5BA985C01D4E6960C5711859486206C12AE6406BD0FF6BD7D4787E099D664`
and `BAE8D79ED48118737FA938D09262F01EB578F6DAEE855CE294A62A74EAF3FE76`.

This evidence disproves the stale current-source polarity hypothesis: local
source/donor axis labels cannot be compared without the live attachment and
animation transforms, and every acted custom thrust currently leads with the
physical spearhead. Follow-up frame analysis establishes that custom `Grip`
coincides exactly with `R_WeaponBone`; the `0.105720 m` `R_Hand` distance is
the native-equivalent radial palm/bone offset from a shaft centerline, not a
grip translation defect. The actual captured defects are custom
support-target averages of `0.279584..0.287448 m` versus native `0.127318 m`
and the V5 near-horizontal shoulder-spanning stored presentation. Preserve the
working physical polarity and grip anchor while repairing support, roll, and
the independent back frame.

## Qualified calibrated branched-spear checkpoint

Commit `a1d45c630502d873debf89ac56562568739c5d58` is published. All three
project-owned FBXs now carry mesh-grounded grip, support, physical-tip,
physical-butt, head-normal, and renderer-center markers. The source generator
validates those markers against evaluated geometry. The builder maps the full
source basis to measured native Longspear held and stored bases, keeps the
equipment root identity, aligns the held grip to the weapon-bone origin,
aligns stored renderer center to the donor BeltModel anchor, and assigns
held-only left-hand IK at the native `0.593016 m` station.

The deterministic bundle SHA-256 is
`A59DC61CE246A7F5931F22494C4C52CE39C6E96312F3448FB9138A0AC0D7DC9B`.
Repository validation, all 1,164 Release domain tests, clean Release build,
build-output/SoundBank checks, and strict package validation pass. Exact tested
package/DLL SHA-256 values are
`97B2F5FF735F7BF141740652F7FED392F1CC6A3267D3D3C070041DC280BD4E45`
and `DFEB9E71B034448F735EF00492CCD143AFBE3F63E09C015D6EE5598AAA638682`.

Runtime evidence passes at:

- `20260821T0517404957120Z-disposable-elven-branched-spear-combat` (24/24);
- `20260821T0520508017635Z-weapon-presentation-evidence` (9/9; 56 sheets;
  224 views; held/stored);
- `20260821T0525081495864Z-weapon-presentation-spear-motion-evidence` (6/6;
  40 sheets; 160 views; ready plus nine attack updates).

All 40 motion samples and all 15 acted samples lead with the physical head.
Custom support-hand averages `0.123882..0.130179 m` match native Longspear
`0.126062 m`; every custom weapon-bone/grip error is zero. Visual review accepts
held idle, stored, combat-ready, and sampled thrust states for Classic, Thorn,
and Crown on the default Medium male without severe persistent clipping.
Movement/transitions, armor/cloak, female, Small, and Enlarged remain open.

## Next concrete actions

1. Inspect the current Eastern generator/bundle/runtime path and historical
   Eastern authoring evidence, then add mesh-grounded grip, tip, butt/pommel,
   and blade-normal markers for every production variant.
2. Derive Wakizashi, Katana, and Nodachi held bases from their measured native
   Scimitar, Bastard Sword, and Greatsword donors, preserving family-specific
   animation and two-hand contracts.
3. Author independently calibrated stored prefabs and qualify held/stored plus
   native-attack evidence without waiting for checkpoint confirmation.
4. Before final acceptance, extend runtime coverage to reload,
   locomotion/turning, transitions, armor/cloak, and representative sex/size
   fixtures, including the calibrated long guns and branched spears.

## Supported hypotheses requiring donor confirmation

- Branched-spear physical head is source `+Z` and source head normal is `+Y`.
  V7 proved the live polarity; the calibrated checkpoint preserves it while
  mapping the full basis to measured native held/stored frames. V8/V9 accept
  support, roll, storage, and captured default-Medium-male presentation. Only
  broader motion/body matrix coverage remains.
- Eastern physical tip is source `+Z`, cutting edge is `-X`, blade normal is
  `+Y`; the measured donor target is forward `+Y`, blade normal `+X`, and edge
  side `-Z`. Identity presentation is therefore not a valid basis conversion.
- Long-gun V4 defects are superseded for the states captured by V5/V6. Musket,
  Blunderbuss, and Rifle are now basis-derived and renderer-endpoint verified;
  their uncaptured reload, locomotion, transition, and body-matrix states
  remain evidence gaps rather than transform hypotheses.

## Safety/publication

Recheck status, stage only explicit mission paths, and commit only on the
feature branch after all required checks. Never merge, rebase published
commits, amend a pushed commit, force-push, create a PR, or create a release.
After every coherent commit and before any handoff run exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```
