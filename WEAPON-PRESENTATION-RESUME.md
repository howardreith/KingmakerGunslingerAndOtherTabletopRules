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
- Published branched-spear qualification documentation commit:
  `185c3a74bde8d5768c97ffb4565c0e171c02798a`
  (`docs(presentation): record calibrated spear qualification`).
- Published Eastern held/stored calibration implementation commit:
  `8aeef5e7fb2ef976e7ca5cbe82ba44d50b01401b`
  (`fix(presentation): calibrate eastern held and stored rigs`).
- Published Eastern qualification documentation commit:
  `54e4af1f16351700282aba2b8a4f47b5433384f3`
  (`docs(presentation): qualify eastern held and stored rigs`).
- Published transition/locomotion fixture commit:
  `897ec7359cc4d8f9ea1260c04ecccc93c164ce39`
  (`test(presentation): qualify transitions and locomotion`).
- Published Eastern clone-only sheath repair commit:
  `754ae076de0c02b5dd1e62691ba5905aa363432c`
  (`fix(presentation): replace detached eastern donor sheaths`).
- Published guarded firearm reload fixture commit:
  `c0f193c1fa75741043b8be753bbc61ecb829bb9a`
  (`test(presentation): capture firearm reload frames`).
- Published handgun held/firing calibration commit:
  `e7e333c8da9f78064a2d99133004af4ef315515c`
  (`fix(presentation): calibrate handgun held and firing rigs`).
- Version remains `0.0.88`; do not bump until the complete cosmetic package is
  qualified.
- The current implementation adds a shared full-frame semantic contract,
  basis-calibrates the service Pistol and Revolver, and gives all three long
  guns deterministic source frames plus independent donor-calibrated held and
  stored presentation. All three branched spears now have mesh-grounded source
  frames, native-derived held/stored bases, and held-only support-hand IK. All
  12 Eastern variants now have renderer-grounded source frames, measured
  family-donor held bases, independent stored prefabs, and held-only Nodachi IK;
  runtime replaces the donor clone's held and belt model fields and clears only
  that clone's redundant sheath field. Native donor sheaths and all other donor
  fields remain unchanged. A guarded production-ability fixture now captures
  the complete sampled Reload Firearm delivery window for all seven firearm
  variants without changing firearm mechanics or presentation transforms. All
  four held handgun variants now use the exact native PiercingOneHanded
  Shortspear grip/roll frame plus a measured donor-relative visible-model
  firing correction, with guarded ready, acted-fire, and valid dual-wield
  evidence.

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

## Qualified calibrated Eastern held and stored checkpoint

Commit `8aeef5e7fb2ef976e7ca5cbe82ba44d50b01401b` is published. The
schema-3 source generator emits actual-mesh `Grip`, `Tip`, `Butt`,
`BladeNormal`, `Edge`, and `Stored` semantics for all 12 FBXs plus butt-side
`Support` for Nodachi. It validates the semantic endpoints against evaluated
renderer geometry and records source `+Z` forward, `+Y` blade normal, and `-X`
cutting edge. The Unity 2018.4.10f1 builder accounts for the FBX reflection,
solves a right-handed source-to-donor basis on the visible child, keeps every
equipment root identity, and emits exact held/`Stored` pairs.

Measured donors remain Scimitar, Bastard Sword, and Greatsword. Held
translation maps source grip to the donor grip. Stored translation separately
maps `StoredMount` to the measured donor renderer-center anchor. Runtime loads
all 24 prefabs transactionally and changes only `m_WeaponModel` and
`m_WeaponBeltModel` on donor clones, preserving animation, trail, sound,
attachment slots, sheath, timing, and all other visual fields. Nodachi alone
uses held-only left-hand IK at the native `-0.169 m` butt-side station.

The first static run failed closed at
`20260821T0629124884888Z-weapon-presentation-evidence` while recreating the
then-preserved native Greatsword sheath: a runtime-added `EquipmentOffsets` had a
null `m_SlotOffsets`, which native `GetOffsets` enumerates for non-hand slots.
The narrow correction initializes an empty slot-offset array. It neither adds
an offset nor clears the sheath at that historical checkpoint, and it leaves
the root identity. The
immediate dirty-source retry completed all 56 captures and wrote PASS after the
generic 120-second wrapper deadline; clean commit-bound reruns below supersede
that wrapper race.

Exact calibrated source FBX SHA-256 values are:

- Wakizashi Classic `A121C0BD1010B4083A29644D49DDD61020829AFAB5687E6D0249AC1EC80543D0`;
  Petal `5A7C77D2C382ACC71C1AD0DCF9D48B7E5A5C271933B163D0BC8EF4C83CC9979C`;
  Moon `4EDB462134FA8BDE867A0AAD42E4109746317F9BBA27C0C4AF2347755A5DF2FA`;
  Capstone `5031D238E65D6D57A859E4FBF2DEEB5AA800D0F58D0376E6EA1C31935DB98233`.
- Katana Classic `CD9047823503CBE1367FD6667FC0835BF6A578376210E8B9CD48C1ECD02234B8`;
  Reed `2579AFE8BB2B18EDB274B3CBF323C815368F77A0A1543756CE72667CCF48F3BF`;
  Regal `D3B7D032F170CE97447A6AC815D79D5608CC910E148ED2185D5AD663027EB753`;
  Capstone `4E58F7AB12FE45787BA0F3C4860E7F3E8EB759F4519A481AB2987FA6DBB0664E`.
- Nodachi Classic `87BDFBBE364865F3C9A824C9F74FD0E5823045328C096FAE2EDF5A105F211A3E`;
  Cleaver `7A689899A799A4AB8060FCEB9691586259AAA80713D1697651058C1A3F35CAD0`;
  Titan `AC28942CED2F76060EC75323353AF7BA11DC98958F5CC0E62173BDBAA87C918F`;
  Capstone `ACFA2048E8A339AA3670A7CDF240A2E8A3F8E9187A9E3828EDA9ED0E8D3CFB84`.

Two forced Unity builds produced the identical 365,592-byte Eastern bundle,
SHA-256
`AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`.
Logs are `eastern-presentation-build-3.log` and
`eastern-presentation-build-4.log` under the authorized Unity project's
`Logs` directory. Repository validation, all 1,164 Release domain tests,
exact-reference compilation, output/SoundBank checks, strict standalone package
validation, `Build-Local.ps1`, and runtime preflight 121/121 pass.

The final clean 0.0.88 local-runtime package is
`artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`,
22,411,475 bytes, SHA-256
`0AC692C8D3F5EFC8D7A15968BBA8B791C6F4885D8A17156B8F8AFF2695927A5B`.
Its DLL SHA-256 is
`CCF8F81C0025762CD52835A6949848652C255F45EC7B895B083ABA4AD368B8FB`;
MVID is `3e3d7594-5eab-4c58-b739-0e9e04e5326f`. The verified deployment manifest is
`runtime-evidence/deployments/20260821T0655065885306Z/deployment.json`.

Clean commit-bound guarded Steam evidence passes:

- `20260821T0655066469058Z-weapon-presentation-evidence`: 9/9; 56
  held/stored PNG/JSON pairs; 224 views; result/index SHA-256
  `57582D42D5893709EA97B29BB6DD1B881661AA923E70FDD51D6C06D224D32AFD` /
  `05ADD4CD0C2BA202BE20089548C41B2D383A64175CBB55ADB8F3DC839B7E336D`.
- `20260821T0657502514655Z-weapon-presentation-eastern-motion-evidence`: 6/6;
  150 combat-ready/attack PNG/JSON pairs; 600 views; all 15 commands acted;
  result/index SHA-256
  `242062B3D515D1FD0697DC235285E2ADC674EFD3FB05C14BBECF524D256837A1` /
  `31266732D4D8D96B0085F6185A7379BB301BB9D4695A0C5EA29FD8B441A50084`.
- `20260821T0701587480686Z-disposable-eastern-weapons-combat`: 21/21 live
  identity, exact held/stored pair, donor preservation, all-30-item resolution,
  protected combat mechanics, and cleanup assertions; result SHA-256
  `0327284F9E9516B870E23FCA1C8021FD81B4F7CAF8DFF3E206CA7097416F0EE5`.

Nodachi support-hand distance ranges/averages over ten samples are Classic
`0.005465..0.123422 m` / `0.077418 m`, Cleaver
`0.013458..0.132114 m` / `0.081420 m`, Titan
`0.075276..0.141783 m` / `0.093722 m`, Capstone
`0.035726..0.124915 m` / `0.084998 m`, versus native Greatsword
`0.076106..0.137768 m` / `0.093011 m`. Katana and Nodachi grips coincide with
the donor weapon-bone origin; Wakizashi retains the measured Scimitar grip
offset. Direct review of every variant's held, stored, and an acted frame
accepts grip, tip polarity, blade roll, cutting-edge plane, two-hand Nodachi
contact, variant identity, and no severe persistent clipping on the default
Medium male. Movement/turning, equip/unequip transitions, armor/cloak, female,
Small, and Enlarged remain open and are not inferred.

## Qualified transition and locomotion fixture checkpoint

The guarded `weapon-presentation-transition-motion-evidence` fixture now
covers all 22 production variants plus Light Crossbow, Heavy Crossbow,
Longspear, Scimitar, Bastard Sword, and Greatsword controls. Each case captures
an active native equip transition, native navmesh-backed movement, a native
90-degree body-relative turn, and an active native unequip transition from
front, right-side, rear, and front-right-three-quarter views. It uses the
equipment view's own combat-state transition guard without joining real combat,
so `Game.Player.IsInCombat` and turn-based movement gating remain untouched.

The dirty-source qualification run passed at
`20260821T0837326051191Z-weapon-presentation-transition-motion-evidence` with
7/7 assertions, 112 PNG/JSON pairs, 448 labelled views, exact request-local
cleanup, no save call, and automatic exit. Every `UnitMoveTo` was accepted and
produced nonzero velocity plus `0.367545..0.372974 m` displacement; every held
presentation followed a `89.99999..90.00001` degree turn; and every equip and
unequip capture found the matching native coroutine active. Result/index
SHA-256 are
`6F877A4ADA88F7D49CD4745514F2BF6D705B14FECB1E64668863CA2F52B2CF8B` /
`0A25959DCEB32254ACF609F6D7127575913AEEF7B109F67F7C2015E41A22D2F1`.
The tested package is 22,418,038 bytes, SHA-256
`9F08E75EACAB8FFB4A7CDEC4A49F7CD1A3F77E9B01A4A00A9EFCE229085E47DB`;
DLL SHA-256 is
`F6E7934CAB20D0C86B5C42D01AD0C0D30FABB4BAFE8299A5454BAAEC3039D5DE`.

The dirty run's structural evidence remains valid, and the generic wrapper's
deadline race is superseded by a clean commit-bound PASS at
`20260821T0901591703709Z-weapon-presentation-transition-motion-evidence`.
Direct review of that clean run accepts firearm and spear motion/transition
states, but it exposed detached inherited donor scabbards on custom Katana and
Nodachi rear views. Eastern V12 visual acceptance is therefore superseded by
the repair qualification below.

## Eastern custom-clone sheath repair published and clean-qualified

The current narrow change clears `m_WeaponSheathModel` only on each validated
custom Eastern `WeaponVisualParameters` clone. All 12 variants already own a
complete independent stored prefab, so the inherited donor sheath duplicated
the stored role and could float detached during held/transition states. Native
Scimitar, Bastard Sword, and Greatsword donor blueprints remain unchanged and
retain their exact sheaths. Held/stored models, animation, trails, sounds,
attachment slots, timing, item identity, roots, transforms, and mechanics are
otherwise unchanged.

Current qualification passes repository validation, all 1,164 Release domain
tests, clean Release/package creation, strict package validation, and runtime
preflight 124/124 after one known immediate-post-build timestamp retry. The
tested dirty-source package is 22,418,707 bytes, SHA-256
`1C64964A70861C742948164D2FE9DBBE325172E6064215CC837AA304B78C3232`;
DLL SHA-256 is
`7DC0261D8DDAFCCF9AB68091B128099A4F7196FC266A63647A72C01C8F6D40CD`.

- `20260821T0916061387506Z-disposable-eastern-weapons-combat`: PASS 21/21;
  all 30 custom items are sheath-free, all native family donors retain their
  sheath, protected mechanics and cleanup pass; result SHA-256
  `B8566D16AACF4F78808145B5694A1C5A039BE79F8CC7EF5D5D683F03E2F5FB40`.
- `20260821T0918521143567Z-weapon-presentation-transition-motion-evidence`:
  PASS 8/8; custom sheath `48/48` null, native controls `12/12` non-null; 112
  PNG/JSON pairs, 448 views; result/index SHA-256
  `DE63A46EDBB4DC68BBAAB6901A8A584003BD314F6262C76EEFDD25457CA4C353` /
  `82B862659418D2AA2F2B201E737CC8FD99C72B3A9BB7316197CC6ABC00598660`.
- `20260821T0925383218065Z-weapon-presentation-evidence`: PASS 9/9; 56 exact
  stored/held PNG/JSON pairs, 224 views; result/index SHA-256
  `D6A2E2F45AED132ABBFFA5469DEB06798521F57376660E14092756E2CC359CF2` /
  `25ADAF37BD7951B289626D5A3C6576D9324A8BE4B259017E6D830657961736CE`.

Before/after review confirms all detached scabbards are gone, every custom
stored model remains present, and native donor-control scabbards remain
attached. Those dirty-source runs are superseded by clean exact-commit
qualification of published commit
`754ae076de0c02b5dd1e62691ba5905aa363432c`.

The clean artifact is 22,418,712 bytes, SHA-256
`82BFCA3C009BC6BCA8DC0CC23E0B89985153B727AB7848D64EE520C8BA12C3AE`;
the 3,568,128-byte DLL SHA-256 is
`80465348626E6B07570D357FBC89FE1C977E10E88290364B50D135B853C4421F`
with MVID `5a93d383-fb11-4e66-8bcd-fdd46f8137ef`. Exact clean runs are:

- `20260821T0942301027834Z-disposable-eastern-weapons-combat`: PASS 21/21;
  result SHA-256
  `C311F1DD7FA4E82230F5183AF7BE3E12883A2A65DF549A4065EBE8A1580BBDAA`.
- `20260821T0944317567220Z-weapon-presentation-transition-motion-evidence`:
  PASS 8/8; 112 PNG/JSON pairs, 448 views, custom sheaths null 48/48 and
  native controls retained 12/12; result/index SHA-256
  `A1A5E9FD2B952201A5D3C6D8C2E34D5B4AB200BE00FBD2FE8B6E907648D6B435` /
  `66D5E896557E81F1909D08DEC03C42FA73391D05374B6AAA3D7E517B93DBC912`.
- `20260821T0948158393773Z-weapon-presentation-evidence`: PASS 9/9; 56
  held/stored PNG/JSON pairs, 224 views; result/index SHA-256
  `15F2C61FD4F58471254733E493567A176F8B2795E6F19C721B27F50F9C7CD37D` /
  `3DB3CC5D77DBC850250E9F562DF52E5F1ED89E2B8D9196C2304C03B4D9C7F1E5`.

Direct clean review covered all 12 Eastern turned-right sheets, all 12 custom
stored sheets, exact before/after defect frames, and all three stored native
donor controls. No detached sheath remains; custom stored models and native
donor sheaths remain present. No save API was called.

## Qualified guarded firearm reload checkpoint

Published commit `c0f193c1fa75741043b8be753bbc61ecb829bb9a` adds the guarded
`weapon-presentation-reload-evidence` fixture. It executes the real production
Reload Firearm ability and native command lifecycle for Pistol, Duelist Pistol,
Last Word, Revolver, Musket, Blunderbuss, and Rifle. Each case captures
reload-ready, 14 fixed updates through 240, and an event-aligned acted frame.
The fixture performs exact request-local state/inventory cleanup and never
calls a save or projectile discharge path.

Qualification passes runtime preflight 127/127, repository validation, all
1,164 Release domain tests, clean Release compilation/package creation, strict
package validation, and `Build-Local.ps1`. The authoritative clean exact-commit
Steam App ID 640820 run is:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T1043103685398Z-weapon-presentation-reload-evidence/`

It passes 7/7 assertions in 188,253 ms with 112 PNG/JSON pairs, 448 labelled
views, and all seven exact production commands acted. Six capacity-one
transactions load exactly once with zero discharge. Advanced Revolver reaches
its acted delivery but retains the pre-existing exact fail-closed rollback
because the active token carrier cannot represent a six-round state; ammunition
and state remain unchanged. That mechanical limitation is outside this cosmetic
mission.

Result/index SHA-256 are
`5BAFCEF840A1A8C012CAC43E14F514CD7D77D4F1A86B2BEF1F79C844E4138F38` /
`013A8E10A3E184FE3B9C1CCDE04DAA7CF4EAE43FB8DC64DDBE0C4E482674908E`.
The 22,426,625-byte package SHA-256 is
`E7CDDA3D58A1C942F7A56C9DE5CA59711D7DF27DAF3FF1B8153AC6B10928BA30`;
the 3,595,776-byte DLL SHA-256 is
`07A68E8303040AF585B57558C7E65A50A9854340FC5291F58676C49B250E26B5`
with MVID `6be83c36-acb0-47a9-bbe0-34bdb408165c`. The exact deployment manifest is
`runtime-evidence/deployments/20260821T1043103064297Z/deployment.json`.

Direct review accepts sampled reload presentation for all seven firearms on
the default Medium male. Handguns remain clear at the hip. Musket and
Blunderbuss transiently release the support hand during the generic `Self`
action, remain outside persistent torso traversal, and return to plausible
two-hand ready. Rifle's acted frame is clear and its late samples use the
independent `RifleBelt` back model. No weapon root, muzzle, projectile, reload
mechanic, or gameplay field changed.

## Qualified handgun held and firing checkpoint

Published commit `e7e333c8da9f78064a2d99133004af4ef315515c` adds the guarded
`weapon-presentation-handgun-motion-evidence` fixture and the final held basis
for Pistol Service, Duelist, Last Word, and Revolver Service. The exact native
`OH_SpearShortCommon` PiercingOneHanded frame supplies grip and roll. After the
rendered actor—not only logical actor state—is target-facing, the authored
visible-model forward is derived as `donorForward - 0.468 * donorUp + 0.184 *
donorRight`; up is the donor secondary axis orthonormalized against it. This
changes no equipment root, projectile/muzzle association, attack mechanic,
item identity, or native donor.

Unity 2018.4.10f1 rebuilt the 18,172,963-byte firearm bundle, SHA-256
`B3CFFB49BA32AF10DB12470401A58F6DFF0EAD9F219F87E41D9EC138D62FBAEB`.
Qualification passes repository validation, all 1,164 Release domain tests,
runtime preflight 130/130, clean Release compilation/package creation, strict
package validation, and the clean guarded Steam run at:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T1423438224380Z-weapon-presentation-handgun-motion-evidence/`

That run is bound to exact commit `e7e333c8`, passes 8/8 assertions in 176,118
ms, and contains 74 PNG/JSON pairs / 296 labelled views. Every custom handgun
fires exactly once with zero fault and zero rounds remaining; all six custom/
native attack cases act; all eight valid firearm/Shortsword dual layouts pass;
and cleanup is exact. Minimum ready and acted physical-bore dots are
`0.9025886` and `0.9768526`. Result/index SHA-256 are
`E8AA374C339E90AEF540E4495A0C75495B05DF36EA503AD2804D7DE9B051BDEC` /
`10F8F54093D788F517191FFE677883D253EACC7C1DAE3222AD0A7F0F827FA7F6`.
The exact 22,434,348-byte package SHA-256 is
`E69FCF5C6B7FAAA4EE0439D279579B80821A0819BCA224411FB62B811C2A239E`;
the 3,619,328-byte DLL SHA-256 is
`F34B5655AFD270E1886338909271B44936BC0A5B2B2CFDE949BE356CD577E7AC`
with MVID `16ae7063-60b2-4b3a-ac00-a340de0984c2`.

Direct review accepts all four ready sheets, all four acted sheets, and all
eight custom dual-wield sheets on the default Medium male: plausible grip,
physical muzzle leading, distinct variants, native body-relative animation,
and no severe persistent torso or forearm penetration. Stored handgun
disposition and the broader armor/body matrix remain open.

## Next concrete actions

1. Determine and qualify the actual stored handgun path independently from the
   held basis. Prefer an explicit acceptable stored prefab; otherwise hide the
   stored model with a documented exact-firearm policy rather than exposing a
   severe native-slot mismatch.
2. Construct narrower request-local female Medium, Small, and Enlarged visual
   fixtures and add representative armor/cloak coverage without relying on
   manual save mutation.
3. Run the complete final runtime matrix, select the next unused patch version
   only after all visual rows qualify, produce and hash the final clean package,
   update all version surfaces, and publish the remaining coherent commits.

## Supported hypotheses requiring donor confirmation

- Branched-spear physical head is source `+Z` and source head normal is `+Y`.
  V7 proved the live polarity; the calibrated checkpoint preserves it while
  mapping the full basis to measured native held/stored frames. V8/V9 accept
  support, roll, storage, and captured default-Medium-male presentation. Only
  broader motion/body matrix coverage remains.
- Eastern physical tip is source `+Z`, cutting edge is `-X`, blade normal is
  `+Y`; the measured donor target is forward `+Y`, blade normal `+X`, and edge
  side `-Z`. V10/V11 and E3 confirm the implemented full-basis conversion and
  independent stored models. Post-repair E4/V13/V14 additionally confirm
  clone-only sheath replacement, unchanged native donor sheaths, and accepted
  stored/motion/transition presentation for all 12 variants on the captured
  fixture. Only the broader body matrix remains.
- Long-gun V4 defects are superseded for the states captured by V5/V6. Musket,
  Blunderbuss, and Rifle are now basis-derived and renderer-endpoint verified;
  V12 accepts default-Medium-male locomotion, turning, and transitions, and V17
  accepts their sampled production reload actions. The broader body matrix
  remains an evidence gap rather than a transform hypothesis.

## Safety/publication

Recheck status, stage only explicit mission paths, and commit only on the
feature branch after all required checks. Never merge, rebase published
commits, amend a pushed commit, force-push, create a PR, or create a release.
After every coherent commit and before any handoff run exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```
