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
with physical mesh endpoints and quantitative hand contacts. `V8` is calibrated
static spear held/stored evidence; `V9` is calibrated combat-ready/acted-thrust
evidence with native-comparable support contacts. `V10` is the clean calibrated
Eastern held/stored matrix; `V11` is the clean combat-ready/acted-attack matrix
for all 12 Eastern variants plus their three native controls; `E3` is the same
artifact's all-30-item identity, donor-preservation, and protected-mechanics
qualification. `V12` is all 22 variants plus six native controls in active
equip/unequip transitions, native locomotion, and 90-degree body-relative
turning on the default Medium male; its clean review exposed detached inherited
sheaths on custom Eastern clones. `E4` is the post-repair all-30-item donor and
mechanics qualification. `V13` is the post-repair transition/motion matrix with
an explicit custom-null/native-retained sheath assertion. `V14` is the post-
repair held/stored matrix. `E5`, `V15`, and `V16` are their clean exact-commit
counterparts at published repair commit `754ae076`. None makes an uncaptured
handgun ready/fire/dual-wield, armor/cloak, sex, or size claim. `V17-D` is the
expanded dirty-source firearm reload diagnostic; `V17` is its clean exact-
commit counterpart at `c0f193c1`, covering all seven production firearms with
reload-ready, 14 fixed samples through update 240, and one event-aligned acted
frame per variant. `V18-D` is the calibrated dirty-source handgun diagnostic;
`V18` is its clean exact-commit counterpart at `e7e333c8`, covering all four
production handguns in combat-ready, nine fixed attack samples, an exact acted
discharge frame, and both valid firearm/Shortsword dual-wield layouts. `V19`
is the clean exact-commit static matrix at `d77db371`, proving all four
production handguns intentionally hidden while stored and visible while held.
`V20` is the matching transition/motion matrix, proving each handgun hidden
before equip, visible while held, and hidden again after unequip. These runs
make no broader body/armor claim.

For every Eastern row below, `V13` supersedes the visual transition/motion
claim from `V12`, `V14` supersedes the stored/held presentation check from
`V10`, and `E4` supersedes the custom-clone donor/sheath contract from `E3`.
Clean exact-commit `E5,V15,V16` supersede dirty-source `E4,V13,V14`. The
historical evidence identifiers remain in each row's chain; current acceptance
additionally requires `E5,V15,V16`.

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
| Firearm | Pistol.Service | `Pistol` | intentionally hidden; no compatible belt/sheath prefab | exact Shortspear PiercingOneHanded basis + Light Crossbow ranged control | physical source `-Z`; solved donor-relative frame | source `+Y`; donor up orthonormalized to corrected forward | PASS-S donor grip anchor; OBS-PASS default male hand contact in ready/fire/both dual slots | PASS-S physical muzzle/root `Z=+0.4032`; projectile association preserved; V18 acted dot `0.9802467` | N/A | OBS-PASS default male; no pelvis-spanning model | OBS-PASS native low-ready, dot `0.947816968`; both dual layouts plausible | PASS-S fired once/fault zero/rounds zero; OBS-PASS physical muzzle leads exact acted frame | PASS-S exact Standard command, one-round transaction, zero discharge; OBS-PASS V17 clear hip throughout acted/fixed samples | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS exact hidden policy; body-only stored sheet and hidden/visible/hidden native transition round trip | OBS-PASS held/hidden-stored/ready/fire/reload/dual/move/turn/transitions default male | NC | NC | NC | semantic endpoints; deterministic bundle; F3 65/65; V12/V17 7/7; V18 8/8; V19 10/10; V20 9/9; 1,164 tests | V3/V12/V17/V18/V19/V20 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V12,V17,V18,V19,V20 | OPEN |
| Firearm | Pistol.Duelist | `PistolDuelist` | intentionally hidden; no compatible belt/sheath prefab | exact Shortspear PiercingOneHanded basis + Light Crossbow ranged control | source `+Z`; solved donor-relative frame | source `+Y`; donor up orthonormalized to corrected forward | PASS-S donor grip anchor; OBS-PASS default male hand contact in ready/fire/both dual slots | PASS-S physical muzzle `Z=+0.264`; projectile association preserved; V18 acted dot `0.9768526` | N/A | OBS-PASS distinct silhouette | OBS-PASS native low-ready, dot `0.9025886`; both dual layouts plausible | PASS-S fired once/fault zero/rounds zero; OBS-PASS physical muzzle leads exact acted frame | PASS-S exact Standard command, one-round transaction, zero discharge; OBS-PASS V17 distinct clear held model | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS exact hidden policy; body-only stored sheet and hidden/visible/hidden native transition round trip | OBS-PASS held/hidden-stored/ready/fire/reload/dual/move/turn/transitions default male | NC | NC | NC | semantic endpoints; deterministic bundle; exact variant identity; V12/V17 7/7; V18 8/8; V19 10/10; V20 9/9; 1,164 tests | V3/V12/V17/V18/V19/V20 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V12,V17,V18,V19,V20 | OPEN |
| Firearm | Pistol.LastWord | `PistolLastWord` | intentionally hidden; no compatible belt/sheath prefab | exact Shortspear PiercingOneHanded basis + Light Crossbow ranged control | source `+Z`; solved donor-relative frame | source `+Y`; donor up orthonormalized to corrected forward | PASS-S donor grip anchor; OBS-PASS default male hand contact in ready/fire/both dual slots | PASS-S physical muzzle `Z=+0.264`; projectile association preserved; V18 acted dot `0.993026257` | N/A | OBS-PASS distinct silhouette | OBS-PASS native low-ready, dot `0.9334571`; both dual layouts plausible | PASS-S fired once/fault zero/rounds zero; OBS-PASS physical muzzle leads exact acted frame | PASS-S exact Standard command, one-round transaction, zero discharge; OBS-PASS V17 distinct clear held model | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS exact hidden policy; body-only stored sheet and hidden/visible/hidden native transition round trip | OBS-PASS held/hidden-stored/ready/fire/reload/dual/move/turn/transitions default male | NC | NC | NC | semantic endpoints; deterministic bundle; exact variant identity; V12/V17 7/7; V18 8/8; V19 10/10; V20 9/9; 1,164 tests | V3/V12/V17/V18/V19/V20 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V12,V17,V18,V19,V20 | OPEN |
| Firearm | Revolver.Service | `Revolver` | intentionally hidden; no compatible belt/sheath prefab | exact Shortspear PiercingOneHanded basis + Light Crossbow ranged control | physical source `+X`; solved donor-relative frame | source `+Y`; donor up orthonormalized to corrected forward | PASS-S component-bounds-derived grip at donor anchor; OBS-PASS default male hand contact in ready/fire/both dual slots | PASS-S physical muzzle forward and projectile association preserved; V18 acted dot `0.9818531` | N/A | OBS-PASS default male; no pelvis-spanning model | OBS-PASS native low-ready, dot `0.9041647`; both dual layouts plausible | PASS-S fired once/fault zero/rounds zero; OBS-PASS physical muzzle leads exact acted frame | PASS-S exact Move command, acted delivery, zero discharge, and exact six-round fail-closed rollback; OBS-PASS V17 clear held model | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS exact hidden policy; body-only stored sheet and hidden/visible/hidden native transition round trip | OBS-PASS held/hidden-stored/ready/fire/reload/dual/move/turn/transitions default male | NC | NC | NC | semantic endpoints; deterministic bundle; F3 65/65; V12/V17 7/7; V18 8/8; V19 10/10; V20 9/9; 1,164 tests | V3/V12/V17/V18/V19/V20 reviewed | capacity-six mechanical carrier outside cosmetic scope, sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V12,V17,V18,V19,V20 | OPEN |
| Firearm | Musket.Service | `Musket` | independent `MusketBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured trigger-wrist grip at identity root; OBS-PASS dominant hand/stock relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible muzzle tracks target | PASS-S fore-end interval; OBS-PASS `0.118081..0.143733 m`, average `0.131895 m`, versus native average `0.132578 m` | OBS-PASS no persistent torso traversal | OBS-PASS stock lifts to shoulder line without severe torso clipping | PASS-S fired once; OBS-PASS physical muzzle leads and stock remains plausibly shouldered | PASS-S exact FullRound command, acted at update 219, one-round transaction, zero discharge; OBS-PASS V17 native action releases the support hand transiently and returns to plausible two-hand ready without persistent torso penetration | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent diagonal back mount | OBS-PASS held/stored/ready/fire/reload/move/turn/transitions default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; V12/V17 7/7; 1,164 tests | V5/V6/V12/V17 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6,V12,V17 | OPEN |
| Firearm | Blunderbuss.Service | `Blunderbuss` | independent `BlunderbussBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured stock-wrist grip at identity root; OBS-PASS dominant-hand relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible bell tracks target | PASS-S fore-end interval; OBS-PASS `0.107004..0.141727 m`, average `0.125596 m` | OBS-PASS no persistent torso traversal | OBS-PASS shoulder presentation; bell remains clear of support hand | PASS-S fired once; OBS-PASS bell leads and stock remains outside severe torso clipping | PASS-S exact FullRound command, acted at update 226, one-round transaction, zero discharge; OBS-PASS V17 native action releases the support hand transiently and returns to plausible two-hand ready without persistent torso penetration | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent diagonal back mount; bulky but no severe persistent clipping | OBS-PASS held/stored/ready/fire/reload/move/turn/transitions default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; V12/V17 7/7; 1,164 tests | V5/V6/V12/V17 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6,V12,V17 | OPEN |
| Firearm | Rifle.Service | `Rifle` | independent `RifleBelt` BackMount | Heavy Crossbow | original physical `+X`; derivative/root `+Z` | original physical `+Z`; derivative/root `+Y` (`WeaponUp`) | PASS-S measured trigger/lever-wrist grip at identity root; OBS-PASS dominant-hand relationship | renderer-bound muzzle PASS-S; V6 one discharge/no fault and visible muzzle tracks target | PASS-S fore-end interval; OBS-PASS `0.115776..0.144329 m`, average `0.133205 m` | OBS-PASS no stock/barrel traversal through body | OBS-PASS stock lifts toward shoulder without severe clipping | PASS-S fired once; OBS-PASS physical muzzle leads and receiver remains plausibly outside torso | PASS-S exact Move command, acted at update 66, one-round transaction, zero discharge; OBS-PASS V17 clear acted frame and independent `RifleBelt` return by update 240 | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent diagonal `RifleBelt` back mount | OBS-PASS held/stored/ready/fire/reload/move/turn/transitions default male | NC | NC | NC | deterministic source/bundle; semantic contract; V6 motion 6/6; V12/V17 7/7; 1,164 tests | V5/V6/V12/V17 reviewed | sex/size, armor/cloak | F1,F2,V1,V2,F3,V3,V4,V5,V6,V12,V17 | OPEN |
| Elven branched spear | ClassicBranch | `ElvenBranchedSpear` | independent `ElvenBranchedSpearBack` | Longspear | physical source `+Z`; donor-derived held basis | source/head normal `+Y`; donor-derived held roll | PASS-S `Grip` coincides `R_WeaponBone`; OBS-PASS both hands on shaft | PASS-S renderer-bound physical head; 10/10 motion and 3/3 acted samples tip-leading | PASS-S native station `0.593016 m`; OBS-PASS `0.101770..0.181390 m`, average `0.130179 m`, versus native `0.126062 m` | OBS-PASS donor-matched two-hand idle | OBS-PASS branch roll and shaft contacts | PASS-S/OBS-PASS 3/3 acted samples tip-leading with plausible thrust | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent donor-derived diagonal back mount | OBS-PASS held/stored/ready/thrust/move/turn/transitions default male | NC | NC | NC | mesh-grounded source; deterministic bundle; S3 24/24; V8 9/9; V9 6/6; V12 7/7; 1,164 tests | V8/V9/V12 reviewed | sex/size, armor/cloak | S1,S2,S3,V1,V5,V7,V8,V9,V12 | OPEN |
| Elven branched spear | ThornBranch | `ElvenBranchedSpearThorn` | independent `ElvenBranchedSpearThornBack` | Longspear | physical source `+Z`; donor-derived held basis | source/head normal `+Y`; donor-derived held roll | PASS-S `Grip` coincides `R_WeaponBone`; OBS-PASS both hands on shaft | PASS-S renderer-bound physical head; 10/10 motion and 4/4 acted samples tip-leading | PASS-S native station `0.593016 m`; OBS-PASS `0.107490..0.146483 m`, average `0.123882 m`, versus native `0.126062 m` | OBS-PASS donor-matched two-hand idle | OBS-PASS branch roll and shaft contacts | PASS-S/OBS-PASS 4/4 acted samples tip-leading with plausible thrust | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent donor-derived diagonal back mount | OBS-PASS held/stored/ready/thrust/move/turn/transitions default male | NC | NC | NC | mesh-grounded source; deterministic bundle; S3 24/24; V8 9/9; V9 6/6; V12 7/7; 1,164 tests | V8/V9/V12 reviewed | sex/size, armor/cloak | S1,S2,S3,V1,V5,V7,V8,V9,V12 | OPEN |
| Elven branched spear | CrownBranch | `ElvenBranchedSpearCrown` | independent `ElvenBranchedSpearCrownBack` | Longspear | physical source `+Z`; donor-derived held basis | source/head normal `+Y`; donor-derived held roll | PASS-S `Grip` coincides `R_WeaponBone`; OBS-PASS both hands on shaft | PASS-S renderer-bound physical head; 10/10 motion and 4/4 acted samples tip-leading | PASS-S native station `0.593016 m`; OBS-PASS `0.108711..0.157084 m`, average `0.124882 m`, versus native `0.126062 m` | OBS-PASS donor-matched two-hand idle | OBS-PASS branch roll and shaft contacts | PASS-S/OBS-PASS 4/4 acted samples tip-leading with plausible thrust | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent donor-derived diagonal back mount | OBS-PASS held/stored/ready/thrust/move/turn/transitions default male | NC | NC | NC | mesh-grounded source; deterministic bundle; S3 24/24; V8 9/9; V9 6/6; V12 7/7; 1,164 tests | V8/V9/V12 reviewed | sex/size, armor/cloak | S1,S2,S3,V1,V5,V7,V8,V9,V12 | OPEN |
| Wakizashi | Classic | `Wakizashi` | independent `WakizashiStored` | Scimitar | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S measured Scimitar grip; OBS-PASS hand contact | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | N/A | OBS-PASS blade plane and grip | OBS-PASS valid light-blade ready pose | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent Scimitar-derived belt mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic 24-prefab bundle; E3 21/21; V10 9/9; V11/V12 6/6 and 7/7; 1,164 tests | V10/V11/V12 reviewed | dual-wield, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Wakizashi | Petal | `WakizashiPetal` | independent `WakizashiPetalStored` | Scimitar | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S measured Scimitar grip; OBS-PASS hand contact | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | N/A | OBS-PASS distinct Petal held identity | OBS-PASS valid light-blade ready pose | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching belt mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | dual-wield, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Wakizashi | Moon | `WakizashiMoon` | independent `WakizashiMoonStored` | Scimitar | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S measured Scimitar grip; OBS-PASS hand contact | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | N/A | OBS-PASS distinct Moon held identity | OBS-PASS valid light-blade ready pose | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching belt mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | dual-wield, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Wakizashi | Capstone | `WakizashiCapstone` | independent `WakizashiCapstoneStored` | Scimitar | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S measured Scimitar grip; OBS-PASS hand contact | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | N/A | OBS-PASS distinct capstone held identity | OBS-PASS valid light-blade ready pose | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching belt mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | dual-wield, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Katana | Classic | `Katana` | independent `KatanaStored` | Bastard Sword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S grip at donor weapon-bone origin; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | donor animation; no custom IK; OBS-PASS two-hand contact | OBS-PASS donor-matched blade plane | OBS-PASS Bastard Sword ready frame | PASS-S command acted; OBS-PASS donor-matched sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent Bastard Sword-derived back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic 24-prefab bundle; E3 21/21; V10 9/9; V11/V12 6/6 and 7/7; 1,164 tests | V10/V11/V12 reviewed | one-hand visual, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Katana | Reed | `KatanaReed` | independent `KatanaReedStored` | Bastard Sword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | donor animation; no custom IK; OBS-PASS two-hand contact | OBS-PASS distinct Reed identity | OBS-PASS Bastard Sword ready frame | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | one-hand visual, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Katana | Regal | `KatanaRegal` | independent `KatanaRegalStored` | Bastard Sword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | donor animation; no custom IK; OBS-PASS two-hand contact | OBS-PASS distinct Regal identity | OBS-PASS Bastard Sword ready frame | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | one-hand visual, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Katana | Capstone | `KatanaCapstone` | independent `KatanaCapstoneStored` | Bastard Sword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | donor animation; no custom IK; OBS-PASS two-hand contact | OBS-PASS distinct capstone identity | OBS-PASS Bastard Sword ready frame | PASS-S command acted; OBS-PASS sampled slash plane | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | one-hand visual, sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Nodachi | Classic | `Nodachi` | independent `NodachiStored` | Greatsword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S grip at donor weapon-bone origin; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | PASS-S native `-0.169 m` butt-side station; OBS-PASS `0.005465..0.123422 m`, avg `0.077418 m`, native avg `0.093011 m` | OBS-PASS donor-matched plane | OBS-PASS both hands on handle | PASS-S command acted; OBS-PASS two-hand sampled slash | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent Greatsword-derived back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic 24-prefab bundle; safe sheath offsets; E3 21/21; V10 9/9; V11/V12 6/6 and 7/7; 1,164 tests | V10/V11/V12 reviewed | sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Nodachi | Cleaver | `NodachiCleaver` | independent `NodachiCleaverStored` | Greatsword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | PASS-S native butt-side station; OBS-PASS `0.013458..0.132114 m`, avg `0.081420 m` | OBS-PASS distinct Cleaver identity | OBS-PASS both hands on handle | PASS-S command acted; OBS-PASS two-hand sampled slash | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; safe sheath offsets; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Nodachi | Titan | `NodachiTitan` | independent `NodachiTitanStored` | Greatsword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | PASS-S native butt-side station; OBS-PASS `0.075276..0.141783 m`, avg `0.093722 m` | OBS-PASS distinct Titan identity | OBS-PASS both hands on handle | PASS-S command acted; OBS-PASS two-hand sampled slash | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; safe sheath offsets; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |
| Nodachi | Capstone | `NodachiCapstone` | independent `NodachiCapstoneStored` | Greatsword | physical source `+Z`; donor-derived held basis | source blade normal `+Y`, edge `-X`; donor-derived roll | PASS-S donor weapon-bone grip; OBS-PASS | PASS-S renderer-bound physical tip; canonical blade frame in all 10 motion states | PASS-S native butt-side station; OBS-PASS `0.035726..0.124915 m`, avg `0.084998 m` | OBS-PASS distinct capstone identity | OBS-PASS both hands on handle | PASS-S command acted; OBS-PASS two-hand sampled slash | N/A | PASS-S native movement and transitions; OBS-PASS locomotion, 90-degree turn, equip/unequip | OBS-PASS independent matching back mount | OBS-PASS held/stored/ready/attack/move/turn/transitions default male | NC | NC | NC | schema-3 mesh semantics; deterministic pair; safe sheath offsets; E3/V10/V11/V12 PASS | V10/V11/V12 reviewed | sex/size, armor/cloak | E1,E2,E3,V1,V2,V10,V11,V12 | OPEN |

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
- S3: `20260821T0517404957120Z-disposable-elven-branched-spear-combat`
  (calibrated source/bundle/runtime frame, held-only IK, independent stored,
  native-donor preservation, identity, and mechanics; 24/24 assertions; result
  SHA-256
  `9BA7F08F144ABFD4DA95BD479444DA60E7963187205A1DA19294F817D01CC6C3`).
- V8: `20260821T0520508017635Z-weapon-presentation-evidence` (calibrated
  Classic/Thorn/Crown held and stored within the exact 22-variant plus 6-control
  matrix; 56 PNG/JSON pairs, 224 views, 9/9 assertions, no blank sheets, exact
  cleanup; result/index SHA-256
  `80BE05F0D94040446163D53DC434C9B59E62594CD4269F5D0294E60B92F48AC2` /
  `22D045AFA893D5A34A611C50297842D95F37F3A77E3A90CF03D1A3C83645ECF2`;
  default Medium male only).
- V9: `20260821T0525081495864Z-weapon-presentation-spear-motion-evidence`
  (calibrated Classic, Thorn, Crown, and native Longspear in combat-ready plus
  nine attack updates; 40 PNG/JSON pairs, 160 views, 6/6 assertions, all 40
  physical-tip-leading, all 15 acted samples physical-tip-leading, exact
  cleanup, native-comparable hand contacts; result/index SHA-256
  `CFB570DC4A726DE0182DDEB6A8F834B23282CB8066AA83DDB9ACB21A8F159CA8` /
  `E1D0096EA98CD9CEC0CE553C323FF9E0C4EC3017355AF528A5C7780F9D0A2CE9`;
  default Medium male only).
- V10: `20260821T0655066469058Z-weapon-presentation-evidence` (clean
  commit-bound calibrated Eastern held/stored evidence within the exact 22-
  variant plus six-native-control matrix; 56 PNG/JSON pairs, 224 views, 9/9
  assertions, no blank or low-density sheets, exact cleanup; result/index
  SHA-256
  `57582D42D5893709EA97B29BB6DD1B881661AA923E70FDD51D6C06D224D32AFD` /
  `05ADD4CD0C2BA202BE20089548C41B2D383A64175CBB55ADB8F3DC839B7E336D`;
  default Medium male only).
- V11: `20260821T0657502514655Z-weapon-presentation-eastern-motion-evidence`
  (all 12 calibrated Eastern variants plus native Scimitar, Bastard Sword, and
  Greatsword in combat-ready plus nine attack updates; 150 PNG/JSON pairs, 600
  views, 6/6 assertions, all 15 commands acted, all physical blade frames
  nondegenerate/orthogonal with canonical cutting-edge polarity, exact cleanup;
  result/index SHA-256
  `242062B3D515D1FD0697DC235285E2ADC674EFD3FB05C14BBECF524D256837A1` /
  `31266732D4D8D96B0085F6185A7379BB301BB9D4695A0C5EA29FD8B441A50084`;
  default Medium male only).
- E3: `20260821T0701587480686Z-disposable-eastern-weapons-combat` (clean exact-
  artifact all-30-item identity, held/stored pair, donor-field preservation,
  protected combat mechanics, and cleanup qualification; 21/21 assertions;
  result SHA-256
  `0327284F9E9516B870E23FCA1C8021FD81B4F7CAF8DFF3E206CA7097416F0EE5`).
- V12-D:
  `20260821T0837326051191Z-weapon-presentation-transition-motion-evidence`
  (all 22 production variants plus six native controls in active native equip
  and unequip transitions, navmesh-backed movement, and a 90-degree body-
  relative turn; 112 PNG/JSON pairs, 448 views, 7/7 assertions, every movement
  accepted with nonzero velocity and measurable displacement, exact cleanup,
  no save; result/index SHA-256
  `6F877A4ADA88F7D49CD4745514F2BF6D705B14FECB1E64668863CA2F52B2CF8B` /
  `0A25959DCEB32254ACF609F6D7127575913AEEF7B109F67F7C2015E41A22D2F1`;
  default Medium male only; dirty-source diagnostic).
- V12:
  `20260821T0901591703709Z-weapon-presentation-transition-motion-evidence`
  (clean exact-commit rerun at `897ec735`; 7/7, 112 PNG/JSON pairs, 448 views;
  result/index SHA-256
  `7C32187C6148C438E5B40C18BA2C55E7154433D1EFA8D62176AAC345F70EBCA4` /
  `F6AF2475F8AD5706A61CEEDB2EF69B17C389553ADFD84EB7D238AEE979B1652F`;
  direct review accepts firearm/spear states but marks custom Eastern rear-view
  transitions `OBS-DEFECT` because inherited donor scabbards float detached).
- E4: `20260821T0916061387506Z-disposable-eastern-weapons-combat` (post-repair
  dirty-source all-30-item identity and donor contract; custom sheaths null,
  native family sheaths retained, protected mechanics and cleanup; 21/21;
  result SHA-256
  `B8566D16AACF4F78808145B5694A1C5A039BE79F8CC7EF5D5D683F03E2F5FB40`).
- V13:
  `20260821T0918521143567Z-weapon-presentation-transition-motion-evidence`
  (post-repair dirty-source matrix; 8/8, 112 PNG/JSON pairs, 448 views; custom
  Eastern sheath null 48/48, native controls retained 12/12; direct review
  accepts all 12 Eastern turned/relevant transition frames and native controls;
  result/index SHA-256
  `DE63A46EDBB4DC68BBAAB6901A8A584003BD314F6262C76EEFDD25457CA4C353` /
  `82B862659418D2AA2F2B201E737CC8FD99C72B3A9BB7316197CC6ABC00598660`).
- V14: `20260821T0925383218065Z-weapon-presentation-evidence` (post-repair
  dirty-source exact held/stored matrix; 9/9, 56 PNG/JSON pairs, 224 views;
  every Eastern stored prefab remains visible and independently calibrated;
  result/index SHA-256
  `D6A2E2F45AED132ABBFFA5469DEB06798521F57376660E14092756E2CC359CF2` /
  `25ADAF37BD7951B289626D5A3C6576D9324A8BE4B259017E6D830657961736CE`).
- E5: `20260821T0942301027834Z-disposable-eastern-weapons-combat` (clean
  exact-commit `754ae076` all-30-item identity/donor/mechanics qualification;
  custom sheaths null, native family sheaths retained; 21/21; result SHA-256
  `C311F1DD7FA4E82230F5183AF7BE3E12883A2A65DF549A4065EBE8A1580BBDAA`).
- V15:
  `20260821T0944317567220Z-weapon-presentation-transition-motion-evidence`
  (clean exact-commit `754ae076` matrix; 8/8, 112 PNG/JSON pairs, 448 views;
  custom Eastern sheath null 48/48, native controls retained 12/12; direct
  review accepts all 12 Eastern turned-right sheets; result/index SHA-256
  `A1A5E9FD2B952201A5D3C6D8C2E34D5B4AB200BE00FBD2FE8B6E907648D6B435` /
  `66D5E896557E81F1909D08DEC03C42FA73391D05374B6AAA3D7E517B93DBC912`).
- V16: `20260821T0948158393773Z-weapon-presentation-evidence` (clean exact-
  commit `754ae076` held/stored matrix; 9/9, 56 PNG/JSON pairs, 224 views;
  direct review accepts all 12 custom Eastern stored sheets and the three
  native stored donor controls; result/index SHA-256
  `15F2C61FD4F58471254733E493567A176F8B2795E6F19C721B27F50F9C7CD37D` /
  `3DB3CC5D77DBC850250E9F562DF52E5F1ED89E2B8D9196C2304C03B4D9C7F1E5`).
- V17-D:
  `20260821T1034434172396Z-weapon-presentation-reload-evidence` (expanded
  dirty-source reload diagnostic; all seven production firearms, reload-ready,
  14 fixed updates through 240, and one event-aligned acted capture per case;
  7/7 assertions, 112 PNG/JSON pairs, 448 views, exact cleanup and no save;
  result/index SHA-256
  `B4565FB34985E10C766ED82CBBD4A8DC17373576D23C35EB21D59DDD0DD5876F` /
  `51E9095F456FB4A977260BFCC45AA00189712D94E06FE87133CBA7C13D78A24A`).
- V17:
  `20260821T1043103685398Z-weapon-presentation-reload-evidence` (clean
  exact-commit `c0f193c1` rerun; 7/7 assertions, 112 PNG/JSON pairs, 448 views,
  all seven exact production commands acted, six one-round transactions loaded
  exactly once, zero discharges, and Advanced Revolver retained exact
  fail-closed rollback; result/index SHA-256
  `5BAFCEF840A1A8C012CAC43E14F514CD7D77D4F1A86B2BEF1F79C844E4138F38` /
  `013A8E10A3E184FE3B9C1CCDE04DAA7CF4EAE43FB8DC64DDBE0C4E482674908E`;
  default Medium male only).
- V18-D:
  `20260821T1414184583550Z-weapon-presentation-handgun-motion-evidence`
  (calibrated dirty-source diagnostic; 8/8 assertions, 74 PNG/JSON pairs, 296
  views, four exact custom discharges with zero faults, all eight dual layouts,
  minimum ready/acted dots `0.9075612` / `0.9751976`, exact cleanup; result/
  index SHA-256
  `748C1B9278FFA9B27A251AA0C3DBBBE417930589C88D0F5CEBAF8C68D7F18207` /
  `11CA80DB33995373D7C8CE6468158C42662E867A64FF3FE718D6EC282046B637`).
- V18:
  `20260821T1423438224380Z-weapon-presentation-handgun-motion-evidence`
  (clean exact-commit `e7e333c8` rerun; 8/8 assertions, 74 PNG/JSON pairs, 296
  views, all six native commands acted, all four custom firearms fired exactly
  once with zero faults/rounds, all eight dual layouts passed, minimum ready/
  acted dots `0.9025886` / `0.9768526`, exact cleanup; result/index SHA-256
  `E8AA374C339E90AEF540E4495A0C75495B05DF36EA503AD2804D7DE9B051BDEC` /
  `10F8F54093D788F517191FFE677883D253EACC7C1DAE3222AD0A7F0F827FA7F6`;
  default Medium male only).
- V19:
  `20260821T1458081081114Z-weapon-presentation-evidence`
  (clean exact-commit `d77db371` static matrix; 10/10 assertions, 56 exact
  states, 56 PNG/JSON pairs, 224 views, all four production handguns hidden
  while stored and visible while held, exact cleanup; result/index SHA-256
  `7C1139CD0F761AC125934AD8379FC9C0E7231AEA22F6AFD958664D73B2507FCD` /
  `838C0662301ECC7579E0DBD36C1BDDD748424654AFE25429CA93EDFC8F931F94`;
  default Medium male only).
- V20:
  `20260821T1500520935137Z-weapon-presentation-transition-motion-evidence`
  (clean exact-commit `d77db371` transition/motion matrix on the exact V19
  reusable artifact; 9/9 assertions, all 28 cases and 112 exact states, 112
  PNG/JSON pairs, 448 views, every production handgun hidden before equip,
  visible held, and hidden after unequip, exact cleanup; result/index SHA-256
  `5AE6B8F36629C57BB925BBB3A0FC14741BBC01CE72CD1C6A50D70A5D55C11EC6` /
  `7689B908D28D2F1667AAC2F528E23BE856085B45FE4626D3051D96CA720DAC4C`;
  default Medium male only).

All runtime evidence directories are under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`. V1/V2/V3 are direct cosmetic
evidence only for stored and held-idle on one default Medium male fixture. V4
is direct cosmetic evidence only for long-gun combat-ready and sampled attack
states on that fixture. V5/V6 supersede V4's long-gun visual defects only for
their captured default-Medium-male states. None upgrades an uncaptured state or
character configuration. V8/V9 supersede V7's branched-spear defects only for
their captured default-Medium-male states. V10/V11 supersede the V1 Eastern
blade-plane and shared-stored-transform defects only for their captured
default-Medium-male states; E3 adds structured identity/donor/mechanical non-
regression but no broader cosmetic claim. V12 adds locomotion, turning, and
equip/unequip evidence for the default Medium male, but its Eastern visual
claim is superseded by the detached-sheath defect and then by post-repair
E4/V13/V14 acceptance and clean exact-commit E5/V15/V16. Those runs do not
broaden equipment-loadout, armor, sex, or size coverage. V17 adds sampled
Reload Firearm presentation for all seven production firearms on that same
default Medium male. V18 accepts handgun ready, exact acted fire, and both valid
dual-wield layouts on that fixture. V19/V20 accept the intentionally hidden
stored policy and its native equip/unequip round trip for every production
handgun. They do not add broader character/loadout acceptance.
