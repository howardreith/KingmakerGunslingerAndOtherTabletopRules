# Weapon Presentation Acceptance Matrix

Status values: `PASS-S` is structured/mechanical evidence only; `OBS-DEFECT`
means state-labelled imagery directly reproduces a defect; `OBS-OPEN` means the
state is captured but not accepted; `OBS-PASS` means the named captured state
is visually acceptable only for the explicitly named fixture; `NC` means not
captured; `N/A` means not applicable. Source axes describe authored source
assets before any accepted donor conversion. `V1` is unchanged-asset
default-Medium-male evidence. `V2` adds six exact live native donor controls
and model-local frame data. `V3` is the semantic-frame/handgun checkpoint on
the same default Medium male; `F3` is its 65-assertion live firearm rig
contract. `V4` adds native combat-ready and acted-attack sampling for the three
long guns plus Heavy Crossbow control. `V5` is the calibrated static long-gun
run; `V6` is the calibrated combat-ready/acted-fire run with quantitative rig
contacts. `V7` is the unchanged-spear combat-ready/acted-thrust diagnostic
with physical mesh endpoints and quantitative hand contacts. None makes an
uncaptured reload, movement, sex, or size claim.

## Measured native donor controls

| Donor | Exact control item | Held prefab | Stored presentation | Forward | Secondary axis | Support / head / tip evidence | Evidence |
|---|---|---|---|---|---|---|---|
| Light Crossbow | `511c97c1ea111444aa186b1a58496664` | `TH_CrossbowLightArmy_Normal` | same weapon model, independent attachment transform; native quiver also present | `+Z` | up `+Y` | left-hand target `(-0.0250,-0.0240,0.3570)`; warhead near `Z=0.4410` | V2 |
| Heavy Crossbow | `19a5092244dcf99478dcd73c974828b1` | `TH_CrossbowHeavy` | same weapon model, independent attachment transform; native quiver also present | `+Z` | up `+Y` | left-hand target `(-0.0310,-0.0510,0.3740)` | V2 |
| Longspear | `f28f6031c2908d84d945865a80f67177` | `TH_LongspearKnight1` | same weapon model, independent attachment transform | `+Y` | head/blade plane `YZ`; normal `+X` | warhead `Y=0.9053`; head trail `Y=0.9820`; left-hand target `Y=-0.1680` | V2 |
| Scimitar | `2ca0329871f14a27922370f17ea4d15d` | `OH_ScimitarBandits` | held model plus independent native scabbard/attachment | `+Y` | blade normal `+X`; edge side `-Z` | trail start `Y=0.0900`; end `Y=0.6850`; tip curves toward `-Z` | V2 |
| Bastard Sword | `7b8a4a452f11022488b1c7bfb0ed7746` | `OH_SwordBastardArmy` | held model plus independent native scabbard/attachment | `+Y` | blade normal `+X` | trail start `Y=0.1256`; end `Y=1.1667` | V2 |
| Greatsword | `0782c8ca4b6c4634a0f6dabbed796211` | `TH_GreatswordBarbarian` | held model plus independent native scabbard/attachment | `+Y` | blade normal `+X` | trail start `Y=0.2130`; end `Y=1.3950`; left-hand target `Y=-0.1690` | V2 |

| Family | Exact weapon / visual variant | Held prefab | Stored prefab / actual baseline mount | Native donor | Source forward | Source up / blade normal / head up | Grip result | Tip / muzzle result | Support-hand result | Held-idle result | Combat-ready result | Attack / fire / thrust result | Reload result | Movement result | Stored result | Male Medium result | Female Medium | Small | Enlarged | Automated validation | Runtime visual status | Remaining uncertainty | Evidence | Acceptance |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Firearm | Pistol.Service | `Pistol` | no belt/sheath prefab; held model uses native front-belt attachment | Light Crossbow + PiercingOneHanded | physical source `-Z`; solved root `+Z` | source `+Y`; solved root `+Y` (`WeaponUp`) | PASS-S grip-derived root; OBS-PASS default male hand contact | PASS-S physical muzzle/root `Z=+0.4032`; one projectile preserved | N/A | OBS-PASS default male; no pelvis-spanning model | NC | mechanical PASS-S; visual NC | PASS-S | NC | OBS-PASS default male muzzle-down native slot | OBS-PASS held/stored only | NC | NC | NC | semantic endpoints; deterministic bundle; F3 65/65 | V3 held/stored reviewed | dual wield, ready/fire/reload/move, transitions, sex/size | F1,F2,V1,V2,F3,V3 | OPEN |
| Firearm | Pistol.Duelist | `PistolDuelist` | no belt/sheath prefab; held model uses native front-belt attachment | Light Crossbow + PiercingOneHanded | source/root `+Z` | source/root `+Y` (`WeaponUp`) | PASS-S root grip; OBS-PASS default male hand contact | PASS-S physical muzzle `Z=+0.264`; one projectile preserved | N/A | OBS-PASS default male; distinct silhouette | NC | mechanical PASS-S; visual NC | PASS-S | NC | OBS-OPEN native slot | OBS-PASS held only | NC | NC | NC | semantic endpoints; deterministic bundle; exact variant identity | V3 held/stored reviewed | dual wield, ready/fire/reload/move, transitions, sex/size | F1,F2,V1,V2,F3,V3 | OPEN |
| Firearm | Pistol.LastWord | `PistolLastWord` | no belt/sheath prefab; held model uses native front-belt attachment | Light Crossbow + PiercingOneHanded | source/root `+Z` | source/root `+Y` (`WeaponUp`) | PASS-S root grip; OBS-PASS default male hand contact | PASS-S physical muzzle `Z=+0.264`; one projectile preserved | N/A | OBS-PASS default male; distinct silhouette | NC | mechanical PASS-S; visual NC | PASS-S | NC | OBS-OPEN native slot | OBS-PASS held only | NC | NC | NC | semantic endpoints; deterministic bundle; exact variant identity | V3 held/stored reviewed | dual wield, ready/fire/reload/move, transitions, sex/size | F1,F2,V1,V2,F3,V3 | OPEN |
| Firearm | Revolver.Service | `Revolver` | no belt/sheath prefab; held model uses native front-belt attachment | Light Crossbow + PiercingOneHanded | physical source `+X`; solved root `+Z` | source `+Y`; solved root `+Y` (`WeaponUp`) | PASS-S `Grip_LP`-bounds-derived root; OBS-PASS default male | PASS-S physical muzzle/root forward; vertical bore offset retained; one projectile preserved | N/A | OBS-PASS default male; no pelvis-spanning model | NC | mechanical PASS-S; visual NC | PASS-S | NC | OBS-PASS default male muzzle-down native slot | OBS-PASS held/stored only | NC | NC | NC | semantic endpoints; deterministic bundle; F3 65/65 | V3 held/stored reviewed | dual wield, ready/fire/reload/move, transitions, sex/size | F1,F2,V1,V2,F3,V3 | OPEN |
| Firearm | Musket.Service | `Musket` | independent `MusketBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured trigger-wrist grip at identity root; OBS-PASS dominant hand/stock relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible muzzle tracks target | PASS-S fore-end interval; OBS-PASS `0.118081..0.143733 m`, average `0.131895 m`, versus native average `0.132578 m` | OBS-PASS no persistent torso traversal | OBS-PASS stock lifts to shoulder line without severe torso clipping | PASS-S fired once; OBS-PASS physical muzzle leads and stock remains plausibly shouldered | PASS-S mechanical; visual NC | NC | OBS-PASS independent diagonal back mount | OBS-PASS held/stored/ready/fire default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; 1,164 tests | V5/V6 reviewed | reload, locomotion/turning, transitions, sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6 | OPEN |
| Firearm | Blunderbuss.Service | `Blunderbuss` | independent `BlunderbussBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured stock-wrist grip at identity root; OBS-PASS dominant-hand relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible bell tracks target | PASS-S fore-end interval; OBS-PASS `0.107004..0.141727 m`, average `0.125596 m` | OBS-PASS no persistent torso traversal | OBS-PASS shoulder presentation; bell remains clear of support hand | PASS-S fired once; OBS-PASS bell leads and stock remains outside severe torso clipping | PASS-S mechanical; visual NC | NC | OBS-PASS independent diagonal back mount; bulky but no severe persistent clipping | OBS-PASS held/stored/ready/fire default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; 1,164 tests | V5/V6 reviewed | reload, locomotion/turning, transitions, sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6 | OPEN |
| Firearm | Rifle.Service | `Rifle` | independent `RifleBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured trigger/lever-wrist grip at identity root; OBS-PASS dominant-hand relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible muzzle tracks target | PASS-S fore-end interval; OBS-PASS `0.115776..0.144329 m`, average `0.133205 m` | OBS-PASS no stock/barrel traversal through body | OBS-PASS stock lifts toward shoulder without severe clipping | PASS-S fired once; OBS-PASS physical muzzle leads and receiver remains plausibly outside torso | PASS-S mechanical; visual NC | NC | OBS-PASS independent diagonal `RifleBelt` back mount | OBS-PASS held/stored/ready/fire default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; 1,164 tests | V5/V6 reviewed | reload, locomotion/turning, transitions, sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6 | OPEN |
| Elven branched spear | ClassicBranch | `ElvenBranchedSpear` | independent `ElvenBranchedSpearBack` | Longspear | physical source `+Z`; current held root `-Y` | source `+Y`; current `HeadUp` root `+Z` | PASS-S `Grip` coincides `R_WeaponBone`; `R_Hand` radial shaft offset `0.105720 m` is native-equivalent | PASS-S renderer-bound physical head; 4/4 acted samples tip-leading | OBS-DEFECT `0.251471..0.288240 m`, average `0.280264 m`, versus native `0.127318 m` average | OBS-DEFECT low/one-hand presentation | OBS-OPEN captured | PASS-S 4/4 acted samples tip-leading; OBS-OPEN roll/clipping | N/A | NC | OBS-DEFECT near-horizontal shoulder-spanning mount | OBS-DEFECT default male | NC | NC | NC | contracts/combat; semantic frame; V7 6/6 | V1/V5/V7 reviewed | support frame, branch roll, stored basis, broader body/motion matrix | S1,S2,V1,V5,V7 | OPEN |
| Elven branched spear | ThornBranch | `ElvenBranchedSpearThorn` | independent `ElvenBranchedSpearThornBack` | Longspear | physical source `+Z`; current held root `-Y` | source `+Y`; current `HeadUp` root `+Z` | PASS-S `Grip` coincides `R_WeaponBone`; `R_Hand` radial shaft offset `0.105720 m` is native-equivalent | PASS-S renderer-bound physical head; 3/3 acted samples tip-leading | OBS-DEFECT `0.221237..0.343453 m`, average `0.287448 m`, versus native `0.127318 m` average | OBS-DEFECT low/one-hand presentation | OBS-OPEN captured | PASS-S 3/3 acted samples tip-leading; OBS-OPEN roll/clipping | N/A | NC | OBS-DEFECT near-horizontal shoulder-spanning mount | OBS-DEFECT default male | NC | NC | NC | contracts/combat; semantic frame; V7 6/6 | V1/V5/V7 reviewed | support frame, branch roll, stored basis, broader body/motion matrix | S1,S2,V1,V5,V7 | OPEN |
| Elven branched spear | CrownBranch | `ElvenBranchedSpearCrown` | independent `ElvenBranchedSpearCrownBack` | Longspear | physical source `+Z`; current held root `-Y` | source `+Y`; current `HeadUp` root `+Z` | PASS-S `Grip` coincides `R_WeaponBone`; `R_Hand` radial shaft offset `0.105720 m` is native-equivalent | PASS-S renderer-bound physical head; 3/3 acted samples tip-leading | OBS-DEFECT `0.250676..0.298006 m`, average `0.279584 m`, versus native `0.127318 m` average | OBS-DEFECT low/one-hand presentation | OBS-OPEN captured | PASS-S 3/3 acted samples tip-leading; OBS-OPEN roll/clipping | N/A | NC | OBS-DEFECT near-horizontal shoulder-spanning mount | OBS-DEFECT default male | NC | NC | NC | contracts/combat; semantic frame; V7 6/6 | V1/V5/V7 reviewed | support frame, branch roll, stored basis, broader body/motion matrix | S1,S2,V1,V5,V7 | OPEN |
| Wakizashi | Classic | `Wakizashi` | inherited Scimitar mounting; no custom stored prefab | Scimitar | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | N/A | OBS-DEFECT blade nearly edge-on/front and behind leg | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | donor basis, roll, combat, independent stored | E1,E2,V1 | OPEN |
| Wakizashi | Petal | `WakizashiPetal` | inherited Scimitar mounting; no custom stored prefab | Scimitar | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | N/A | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Wakizashi | Moon | `WakizashiMoon` | inherited Scimitar mounting; no custom stored prefab | Scimitar | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | N/A | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Wakizashi | Capstone | `WakizashiCapstone` | inherited Scimitar mounting; no custom stored prefab | Scimitar | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | N/A | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Katana | Classic | `Katana` | inherited Bastard Sword mounting; no custom stored prefab | Bastard Sword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | donor-dependent; inactive in idle | OBS-DEFECT blade edge-on/front, crosses behind legs | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN vertical inherited back mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | donor basis, roll, two-hand combat, stored | E1,E2,V1 | OPEN |
| Katana | Reed | `KatanaReed` | inherited Bastard Sword mounting; no custom stored prefab | Bastard Sword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | donor-dependent; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Katana | Regal | `KatanaRegal` | inherited Bastard Sword mounting; no custom stored prefab | Bastard Sword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | donor-dependent; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Katana | Capstone | `KatanaCapstone` | inherited Bastard Sword mounting; no custom stored prefab | Bastard Sword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | donor-dependent; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, roll, combat, stored | E1,E2,V1 | OPEN |
| Nodachi | Classic | `Nodachi` | inherited Greatsword mounting; no custom stored prefab | Greatsword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | interval PASS-S; inactive in idle | OBS-DEFECT plane differs from Wakizashi/Katana | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | donor basis, two hands, combat, stored | E1,E2,V1 | OPEN |
| Nodachi | Cleaver | `NodachiCleaver` | inherited Greatsword mounting; no custom stored prefab | Greatsword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | interval PASS-S; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, two hands, combat, stored | E1,E2,V1 | OPEN |
| Nodachi | Titan | `NodachiTitan` | inherited Greatsword mounting; no custom stored prefab | Greatsword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | interval PASS-S; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, two hands, combat, stored | E1,E2,V1 | OPEN |
| Nodachi | Capstone | `NodachiCapstone` | inherited Greatsword mounting; no custom stored prefab | Greatsword | `+Z` | source `+Y`; marker missing | OBS-OPEN hand attachment | physical tip known; forward open | interval PASS-S; inactive in idle | OBS-DEFECT family blade-plane mismatch | NC | mechanical PASS-S; visual NC | N/A | NC | OBS-OPEN inherited mount | OBS-DEFECT | NC | NC | NC | contracts/combat/V1 PASS | stored + held captured | variant parity, two hands, combat, stored | E1,E2,V1 | OPEN |

## Evidence index

- F1: `20260820T2139121725602Z-disposable-firearm-visual-rigs`.
- F2: `20260820T2154423464217Z-disposable-production-firearm-switching`.
- S1: `20260820T2143164620170Z-observe-elven-branched-spear-contracts`.
- S2: `20260820T2150547350589Z-disposable-elven-branched-spear-combat`.
- E1: `20260820T2148107345614Z-observe-eastern-weapon-contracts`.
- E2: `20260820T2152440037492Z-disposable-eastern-weapons-combat`.
- V1: `20260820T2307109303617Z-weapon-presentation-evidence` (44 exact
  stored/held PNG/JSON pairs, 176 views, unchanged 0.0.88 assets).
- V2: `20260820T2345261164438Z-weapon-presentation-evidence` (the same 22
  production variants plus 6 exact native donor controls; 56 stored/held
  PNG/JSON pairs, 224 views, model-local locator data, and mesh-local bounds
  proven invariant across held/stored attachment at tolerance `0.00001`).
- F3: `20260821T0040087523551Z-disposable-firearm-visual-rigs` (65/65 live
  bundle, semantic rig, animation, IK, holster, projectile, identity, and
  cleanup assertions).
- V3: `20260821T0034448996480Z-weapon-presentation-evidence` (56 stored/held
  PNG/JSON pairs, 224 views, semantic-frame/handgun implementation, default
  Medium male, no blank or low-density sheets).
- V4: `20260821T0210452969596Z-weapon-presentation-motion-evidence` (Musket,
  Blunderbuss, Rifle, and native Heavy Crossbow in combat-ready plus fixed
  native-attack updates `1/4/8/12/18/24/36/60/96`; 40 PNG/JSON pairs, 160
  views, all commands start-ready/running/acted, each firearm fired once with
  no fault, exact cleanup; default Medium male only).
- V5: `20260821T0413290534687Z-weapon-presentation-evidence` (final exact-package calibrated
  long-gun implementation within the exact 22-variant plus 6-control matrix;
  56 stored/held PNG/JSON pairs, 224 views, 9/9 assertions, no blank sheets,
  exact cleanup; result SHA-256 `80AFD853265E79EFE58DCA43EDEA1BC77CC9A99DE4781599A483C18D64FFC974`;
  default Medium male only).
- V6: `20260821T0416419128426Z-weapon-presentation-motion-evidence` (final exact-package calibrated
  Musket, Blunderbuss, Rifle, and native Heavy Crossbow in combat-ready plus
  nine fixed attack updates; 40 PNG/JSON pairs, 160 views, 6/6 assertions,
  every command acted, every firearm discharged once with no fault, exact
  cleanup, and per-frame hand/stock contacts; result/index SHA-256
  `72B7D8088F0A98A59293B66E6AB8ED85E4D5F7F37F16B17E9044EB63EF807813` /
  `6AB9264D2666C7FCC715CF0EDAF2663018D626FC3E9CF44D82C3D094121C8BAB`;
  default Medium male only).
- V7: `20260821T0448131191263Z-weapon-presentation-spear-motion-evidence`
  (unchanged Classic, Thorn, and Crown production spears plus native
  Longspear in combat-ready and nine fixed attack updates; 40 PNG/JSON pairs,
  160 views, 6/6 assertions, 14/14 acted samples physical-tip-leading, exact
  cleanup; result/index SHA-256
  `4CC5BA985C01D4E6960C5711859486206C12AE6406BD0FF6BD7D4787E099D664` /
  `BAE8D79ED48118737FA938D09262F01EB578F6DAEE855CE294A62A74EAF3FE76`;
  default Medium male only).

All runtime evidence directories are under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`. V1/V2/V3 are direct cosmetic
evidence only for stored and held-idle on one default Medium male fixture. V4
is direct cosmetic evidence only for long-gun combat-ready and sampled attack
states on that fixture. V5/V6 supersede V4's long-gun visual defects only for
their captured default-Medium-male states. None upgrades an uncaptured state or
character configuration.
