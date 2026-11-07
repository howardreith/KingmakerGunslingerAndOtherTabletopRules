# Summoned Creature Manual Playtest Repair — R3 status

**R3 CHECKPOINT — PARTIAL; NOT COMPLETE.** One development candidate, one
guarded save-free native run on DATA. 9 of 10 R3 native predicates PASS;
`r3-ui-ghaele-ray-named` FAIL. Visual (screenshot) evidence for the initiative
tracker and action bar is unavailable: every capture was covered by the
new-game prologue storybook. UI-05 has a newly observed tooltip-text defect.
A narrow R3 repair is required before R4.

Records: [MISSION](MISSION.md), [ISSUE-REGISTER](ISSUE-REGISTER.md)
(UI-01..UI-06), [R2-STATUS](R2-STATUS.md).

## Intake (DATA, 2026-11-07)

- Base: `codex/summon-playtest-r2-lifecycle` head
  `ba254d734a8b515bfe6d4bda9e96538266882d5a` (draft PR #112, R2 closed by
  owner acceptance, not merged). Branch `codex/summon-playtest-r3-ui`.
  The R3 PR depends on #112.
- Installed live mod before: version 0.0.140, live tree 254 files,
  SHA-256 `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`.
- Native settings snapshot: 66 values. Protected saves: 95 files hashed
  (KMG_AUTOMATION_BASELINE immutable; this phase is save-free and loads no save).

## Scope

Presentation and metadata only (UI-01..UI-06). No mechanics changed; nothing
from R4+; R1/R2 not reopened. Native donor blueprints are read, never written.

## Changes

- **UI-01 portraits.** Consumer traced: the turn-based initiative tracker
  (`InitiativeTrackerUnitVM` / `UnitOrderHorizontalPCView`) reads
  `unit.Portrait.SmallPortrait`; `UnitUISettings.Portrait` resolves
  `m_CustomPortrait`, then `m_Portrait`, then `Blueprint.PortraitSafe`. The
  unit-level fields are serialized into saves, so they are left empty. KMG
  summon units were cloned from native donors by `CopyBlueprintFields` and so
  carried the donor's `BlueprintPortrait` (Dire Rat → wolf etc.). New
  `ExpandedSummoningPortraitBuilder` gives each registered KMG summon unit its
  own runtime `BlueprintPortrait` built from its approved 128 px creature icon
  (`ExpandedSummoningProjectIcons.Require(key)`), center-cropped to 98×128 to
  match the native 185×242 small-portrait aspect; only the KMG unit's private
  `m_Portrait` is set. Donor portrait objects/sprites are untouched; no global
  getter patch; native overlays (HP bar, status, selection frame) are untouched
  because only the sprite source changes. Policy: `SummonPortraitPolicy`.
- **UI-02 Lantern Archon.** Light Ray → native Searing Light icon
  (`bf0accce250381a44b857d4af6c8e10d`, sprite `SearingLight`); Twin Light Rays
  → icon of native `1fca0ba2fdfe2994a8c8bc1f0f2fc5b1` (sprite `ScorchingRay`).
  Distinct icons for the two actions.
- **UI-03 unnamed blast.** Owner identified: native `AzataGhaelLightRay`
  `33e8997912cf76b4c99dca0445082804` (no name, no description, no icon) on the
  KMG Ghaele `KMG.Summoning.Unit.GhaeleAzata`
  (`e7eccabdc8cd474d86475586792c03c7`), cloned from native
  `AzataGhaelSummoned` `bc8ca1437c0f48948b317b7e64febf0d`. Not the Trumpet
  Blast (already named/iconed; Personal range). Repair attempt: an owned clone
  `KMG.Summoning.Special.GhaeleAzata.LightRay`
  (`c3224deccbd349fe833283e7cd3ed712`, "Light Ray", description generated from
  the ray's own data, Searing Light icon), AI action
  `…LightRayAi` (`e11f40cad76e4fceb48849d5770320be`), brain `…Brain`
  (`271b9ed69a024e9dbc3bf951ad3ce792`), swapped into the KMG unit's own
  references. **Not effective natively** (see Results).
- **UI-04 Unholy Nimbus.** Native Unholy Aura ability icon
  (`47f9cb1c367a5e4489cfa32fce290f86`, sprite `ProtectionFromGood`).
- **UI-05 Brachiosaurus Overrun.** The builder no longer clears the copied
  icon: native overrun ability icon, falling back to the installed Greater
  Overrun feat icon, then creature art. Native result: sprite `Longstrider`.
- **UI-06 null rows.** Cheetah Sprint: Duration "1 round", Saving Throw
  "None". Unholy Nimbus: "Bursts after 1 round; dazed 1d10 rounds" /
  "Will negates (non-demons)". Sibling Trumpet Blast (same fresh-shell
  pattern): "1d4 rounds" / "Fortitude negates".
- Identities: 3 new private identities (above) appended; SpecialIdentityCount
  789 → 792; icon-catalog registry SHA updated; identity-count pins updated.
- Native observer: save-free Sprint32 phase `summon-playtest-r3-ui`
  (`RuntimeTestRunner.SummonPlaytestR3Ui.cs`).

## Validation (R3 ledger, separate from R2)

| Step | Seconds | Result |
| --- | ---: | --- |
| Focused gate compile attempts (3 failing: usings/API) | 18.0 | fixed |
| Focused gate (compile + tests) | 17.9 | PASS |
| Full domain run (pin discovery) | 79.9 | 2858; 2 identity-count pins FAIL → updated |
| Focused gate reruns (identity pins) | 6.2 | 96/96 PASS |
| `validate_icon_catalog.py` + `test_icon_catalog.py` | 12.0 | PASS (30 tests) |
| Candidate build (`Build-Local -DevelopmentPurpose summon-playtest-r3`, explicit `-ReferenceBundleDir`) | 391 | PASS, 13 selected tests |
| Guarded native phase incl. deploy/restore | 136 | 9/10 PASS, 1 FAIL |
| **Total** | **≈661 (≈11.0 min)** | target 15 min, ceiling 30 min |

Candidate: source `42cd2e26978e8747ab6a7f4059ae9f92b52ac163`,
`KingmakerGunslinger-0.0.179-development-summon-playtest-r3-42cd2e26978e.zip`,
package SHA-256 `8dc227c35d5a5cfc43bf8104bcfb1b0ae8cf4038fe3d8e161aa2cca38371b5d2`,
DLL SHA-256 `36e952bf24aeb63dddbc43a0f2c997577259131fa77cbd1627762b3d7251de96`.
Focused filter `summon-playtest-r3;summon-playtest-r2;expanded-summoning.sprint32-native-admission;expanded-summoning.sprint18-append-only-identities;expanded-summoning.identities-exact`.

Native run `20261107T2141577269449Z-f62d35d82770459b87ca3113fae6c7e9`
(evidence local/private), loaded mod 0.0.179, 77 s in game:

| Predicate | Result | Observation |
| --- | --- | --- |
| r3-ui-portrait-roster | PASS | 134 registered units (133 published + withheld Stirge), 0 wrong |
| r3-ui-donor-portraits-untouched | PASS | 72 distinct donors, 0 touched |
| r3-ui-initiative-portraits-live | PASS | 7 live summons: `unit.Portrait.SmallPortrait` = own art; no unit-level portrait |
| r3-ui-lantern-rays | PASS | `SearingLight` / `ScorchingRay` |
| r3-ui-ghaele-ray-named | **FAIL** | `factSwaps=0;brainSwap=False;nativeBrain=AzataGhaelBrain`; native blank ray still on the KMG Ghaele; clone not granted |
| r3-ui-nalfeshnee-nimbus | PASS | icon `ProtectionFromGood`; real rows |
| r3-ui-brachiosaurus-overrun | PASS (predicate) | icon `Longstrider` — see defect below |
| r3-ui-cheetah-sprint | PASS | "1 round" / "None" |
| r3-ui-trumpet-blast-sibling | PASS | "1d4 rounds" / "Fortitude negates" |
| sprint32-fixture-restored / profile-restored | PASS | |

Turn-based captures: all 7 representatives reached their own `Acting` turn in
round 1 with the tracker's portrait source reporting their own art, and 7 PNGs
were captured. **Personally inspected: every image shows the new-game prologue
storybook ("The Road to Glory") covering the screen; no initiative tracker or
action bar is visible.** These screenshots are not visual evidence for any UI
item. Hover tooltips were not captured (hover capture is admitted only for
Cleanup 2 phases), so tooltip rows are verified from live blueprint strings.

## Findings and limitations

1. **UI-03 FAIL.** The native ray does not reach the KMG Ghaele through the
   unit's `AddFacts` array or a unit-level `AddFacts` component, so no swap
   occurred. The actual grant path (for example a nested feature,
   `AddClassLevels` selection, or another component) is unidentified. The
   owned clone/AI/brain identities exist but are unused.
2. **Latent donor-mutation hazard in the UI-03 code.** `CopyBlueprintFields`
   clones `ComponentsArray` as an array but shares the component objects with
   the donor, so the component-level swap would write into the native donor's
   `AddFacts` component if it ever matched. It matched nothing on this run
   (no donor was changed), but it must be removed or made clone-first.
3. **UI-05 tooltip text defect (new).** Overrun now carries the native overrun
   icon `Longstrider`, identical to Trample's. Both Overrun and Trample show
   Duration "Trample" and the trample maneuver description in the Saving Throw
   row (inherited native field text). Not a literal `<null>`, but wrong.
4. **Visual gap.** No native screen of the initiative tracker or action bar;
   the save-free fixture starts under the prologue storybook overlay.
5. Sibling inspection: the only rows without Duration/Saving Throw objects on
   the 7 representatives are native activatable toggles (Power Attack, Fight
   Defensively, Combat Expertise, Bull Rush, Mobility, Ghaele Gaze, Reject
   Rage), which do not carry those fields. Native `RejectRageActivatableAbility`
   has an empty icon; native, out of scope.
6. Unholy Nimbus reuses Unholy Aura's art (sprite name `ProtectionFromGood`);
   owner visual approval is separate.

## Restoration

Guarded wrapper restored the live mod tree to
`5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB` (0.0.140,
verified). Settings restored (66 values). 95 protected save files
byte-identical before/after. No Kingmaker process remains. No lease left in
recovery.

## Portrait audit (UI-01)

Consumer binding: initiative tracker → `unit.Portrait.SmallPortrait` →
`UnitUISettings.Portrait` → (`m_CustomPortrait` ∅) → (`m_Portrait` ∅) →
`BlueprintUnit.PortraitSafe` → KMG `m_Portrait` (R3). "Before" is the donor
unit/portrait the KMG unit inherited; "after" is the KMG portrait sprite read
natively from the live library.

| Key | Published | KMG unit | Unit GUID | Before (donor unit / portrait) | After (small sprite) | Native |
| --- | --- | --- | --- | --- | --- | --- |
| `dire-rat` | yes | `KMG_Summoning_Unit_DireRat` | `e7e43c80489a48e1b562fe6ac11c33ee` | `DogSummoned` / `Dog` | `KMG_SummonPortrait_dire-rat` | PASS |
| `dog` | yes | `KMG_Summoning_Unit_Dog` | `e8c90cb29374455cb6301e4fa7d1f837` | `DogSummoned` / `Dog` | `KMG_SummonPortrait_dog` | PASS |
| `eagle` | yes | `KMG_Summoning_Unit_Eagle` | `7383db28c1d74dce98533ddc257a2e3c` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_eagle` | PASS |
| `poisonous-frog` | yes | `KMG_Summoning_Unit_PoisonousFrog` | `e1a8e5e206154dd48b6aca1d4262e8e7` | `CR4_GiantFrogPoisonous` / `GiantFrog` | `KMG_SummonPortrait_poisonous-frog` | PASS |
| `pony` | yes | `KMG_Summoning_Unit_Pony` | `67a661d7b39e44bc811ad9ff59120110` | `PonySummoned` / `Empty_Portrait` | `KMG_SummonPortrait_pony` | PASS |
| `fire-beetle` | yes | `KMG_Summoning_Unit_FireBeetle` | `714b04102eae4cd2be200a8f09f04bc3` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_fire-beetle` | PASS |
| `giant-centipede` | yes | `KMG_Summoning_Unit_GiantCentipede` | `baf9e8f829e9410db8f3d200bb62a2c6` | `CR0.5_GiantCentipedeStandard` / `Centipede` | `KMG_SummonPortrait_giant-centipede` | PASS |
| `wolf` | yes | `KMG_Summoning_Unit_Wolf` | `48d7740de99b4040bad6b6493a91f42b` | `WolfSummon` / `WolfBlack` | `KMG_SummonPortrait_wolf` | PASS |
| `giant-frog` | yes | `KMG_Summoning_Unit_GiantFrog` | `ca53b4bba8f04497acf3d36440a6e942` | `GiantFrogSummoned` / `GiantFrog` | `KMG_SummonPortrait_giant-frog` | PASS |
| `giant-spider` | yes | `KMG_Summoning_Unit_GiantSpider` | `4a3cd49e751448c8b8836485b262fdf1` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-spider` | PASS |
| `small-air-elemental` | yes | `KMG_Summoning_Unit_SmallAirElemental` | `5184af966fa84c6ebf42f6f941d5d533` | `SummonedAirElementalSmall` / `ElementalAir` | `KMG_SummonPortrait_small-air-elemental` | PASS |
| `small-earth-elemental` | yes | `KMG_Summoning_Unit_SmallEarthElemental` | `1830865b016a486db1de7f99e8b9f997` | `SummonedEarthElementalSmall` / `ElementalEarth` | `KMG_SummonPortrait_small-earth-elemental` | PASS |
| `small-fire-elemental` | yes | `KMG_Summoning_Unit_SmallFireElemental` | `6a95de017d0949708fd32548207f8195` | `SummonedFireElementalSmall` / `ElementalFire` | `KMG_SummonPortrait_small-fire-elemental` | PASS |
| `small-water-elemental` | yes | `KMG_Summoning_Unit_SmallWaterElemental` | `daa6a4f74317475da5a42c83082b7cdd` | `SummonedWaterElementalSmall` / `ElementalWater` | `KMG_SummonPortrait_small-water-elemental` | PASS |
| `goblin-dog` | yes | `KMG_Summoning_Unit_GoblinDog` | `f1584066792a436fa3a5ba0b3731b481` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_goblin-dog` | PASS |
| `hyena` | yes | `KMG_Summoning_Unit_Hyena` | `ec12fab8be5c412d8ee824d15a6621d0` | `WolfSummon` / `WolfBlack` | `KMG_SummonPortrait_hyena` | PASS |
| `horse` | yes | `KMG_Summoning_Unit_Horse` | `629fa53bf40d4b4b9828f57cf81194e9` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_horse` | PASS |
| `giant-ant-worker` | yes | `KMG_Summoning_Unit_GiantAntWorker` | `9db94712f7b540679b8ff650dbaf746a` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-worker` | PASS |
| `boar` | yes | `KMG_Summoning_Unit_Boar` | `d98699ea265441a09c5b9b51769ef7ce` | `CR2_BoarStandard` / `Boar` | `KMG_SummonPortrait_boar` | PASS |
| `leopard` | yes | `KMG_Summoning_Unit_Leopard` | `f36aad1d463444c69950f219457b894f` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_leopard` | PASS |
| `monitor-lizard` | yes | `KMG_Summoning_Unit_MonitorLizard` | `10b148d216364b509cd2c665a47b2950` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_monitor-lizard` | PASS |
| `cheetah` | yes | `KMG_Summoning_Unit_Cheetah` | `6eb29e76792c41c5ab9e3812fe4b66e0` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_cheetah` | PASS |
| `crocodile` | yes | `KMG_Summoning_Unit_Crocodile` | `ed3fa562802b418ab062a7da622874da` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_crocodile` | PASS |
| `dire-bat` | yes | `KMG_Summoning_Unit_DireBat` | `d867cb795b5640219a8362661f447697` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_dire-bat` | PASS |
| `wolverine` | yes | `KMG_Summoning_Unit_Wolverine` | `f640d3e77d7d4a0d8de3351129cd7148` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_wolverine` | PASS |
| `giant-ant-soldier` | yes | `KMG_Summoning_Unit_GiantAntSoldier` | `ef2cd2abd3254cb2a90952253add918a` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-soldier` | PASS |
| `lantern-archon` | yes | `KMG_Summoning_Unit_LanternArchon` | `02f8e9c6c91549deaded9ef667399449` | `CR6_WillOWispStandart` / `Empty_Portrait` | `KMG_SummonPortrait_lantern-archon` | PASS |
| `giant-crab` | yes | `KMG_Summoning_Unit_GiantCrab` | `5965d242ffe1409d98e38f2793543aad` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-crab` | PASS |
| `dire-boar` | yes | `KMG_Summoning_Unit_DireBoar` | `1710475cee544d9d858b05b24fb3ad4c` | `DireBoarSummoned` / `Boar` | `KMG_SummonPortrait_dire-boar` | PASS |
| `dire-wolf` | yes | `KMG_Summoning_Unit_DireWolf` | `d2e7b46ea8994f7085063abac3775142` | `DireWolfSummon` / `WolfBlack` | `KMG_SummonPortrait_dire-wolf` | PASS |
| `grizzly-bear` | yes | `KMG_Summoning_Unit_GrizzlyBear` | `f7370368039e41ba8f88e8218cfb39d0` | `CR4_BearStandard` / `Bear` | `KMG_SummonPortrait_grizzly-bear` | PASS |
| `medium-air-elemental` | yes | `KMG_Summoning_Unit_MediumAirElemental` | `d8e45b6e036b4723a0fc2c1c074fcaea` | `SummonedAirElementalMedium` / `ElementalAir` | `KMG_SummonPortrait_medium-air-elemental` | PASS |
| `medium-earth-elemental` | yes | `KMG_Summoning_Unit_MediumEarthElemental` | `fc989a8d68e24250bb613b8c7ac20238` | `SummonedEarthElementalMedium` / `ElementalEarth` | `KMG_SummonPortrait_medium-earth-elemental` | PASS |
| `medium-fire-elemental` | yes | `KMG_Summoning_Unit_MediumFireElemental` | `c75e8614029540dda8a985c7cba4ac00` | `SummonedFireElementalMedium` / `ElementalFire` | `KMG_SummonPortrait_medium-fire-elemental` | PASS |
| `medium-water-elemental` | yes | `KMG_Summoning_Unit_MediumWaterElemental` | `d1889a6ad861465cacd64508669ef7da` | `SummonedWaterElementalMedium` / `ElementalWater` | `KMG_SummonPortrait_medium-water-elemental` | PASS |
| `air-mephit` | yes | `KMG_Summoning_Unit_AirMephit` | `9de31a332f814795b7258bd1d3813678` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_air-mephit` | PASS |
| `earth-mephit` | yes | `KMG_Summoning_Unit_EarthMephit` | `2ddb201a43134893bb626aba291a83eb` | `MephitEarthSummoned` / `Mephit_03` | `KMG_SummonPortrait_earth-mephit` | PASS |
| `fire-mephit` | yes | `KMG_Summoning_Unit_FireMephit` | `a841652d2c7242d6849a9b74982710f8` | `MephitFireSummoned` / `Mephit_04` | `KMG_SummonPortrait_fire-mephit` | PASS |
| `water-mephit` | yes | `KMG_Summoning_Unit_WaterMephit` | `556264c328834ffcbd0dddd86a93ac2b` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_water-mephit` | PASS |
| `dust-mephit` | yes | `KMG_Summoning_Unit_DustMephit` | `30f6615badf14d21b41d6468d5c93ac3` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_dust-mephit` | PASS |
| `ice-mephit` | yes | `KMG_Summoning_Unit_IceMephit` | `b37cdd78c8cc43be8799e9ffd6953ba2` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_ice-mephit` | PASS |
| `magma-mephit` | yes | `KMG_Summoning_Unit_MagmaMephit` | `ce9551f1413d4fdcb59941734d4546b1` | `MephitFireSummoned` / `Mephit_04` | `KMG_SummonPortrait_magma-mephit` | PASS |
| `ooze-mephit` | yes | `KMG_Summoning_Unit_OozeMephit` | `7cbf19a6c83f4b8e849d17b8a54e6d51` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_ooze-mephit` | PASS |
| `salt-mephit` | yes | `KMG_Summoning_Unit_SaltMephit` | `8c8fe6eb536b450a9a2c10b8cbef9fbe` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_salt-mephit` | PASS |
| `steam-mephit` | yes | `KMG_Summoning_Unit_SteamMephit` | `047f68ee53554d8ab1702de189ecd85d` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_steam-mephit` | PASS |
| `lion` | yes | `KMG_Summoning_Unit_Lion` | `14c7cbb0f32f4e30bbefaeaccba10269` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_lion` | PASS |
| `tiger` | yes | `KMG_Summoning_Unit_Tiger` | `5182125341e34ae98491eaf991bc0ef3` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_tiger` | PASS |
| `pteranodon` | yes | `KMG_Summoning_Unit_Pteranodon` | `c9a94142c9164ab793f7a06ae3fdcf56` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_pteranodon` | PASS |
| `hell-hound` | yes | `KMG_Summoning_Unit_HellHound` | `f8c0664ec7204643be3ab7a1ac16a391` | `HellhoundSummoned` / `HellHound` | `KMG_SummonPortrait_hell-hound` | PASS |
| `owlbear` | yes | `KMG_Summoning_Unit_Owlbear` | `7bde6f94222c47f687ed68c752cb24cb` | `CR8_OwlbearStandard` / `OwlBearBrown` | `KMG_SummonPortrait_owlbear` | PASS |
| `giant-ant-drone` | yes | `KMG_Summoning_Unit_GiantAntDrone` | `9387ef0ca1cd403a985893a85a6c7806` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-drone` | PASS |
| `giant-stag-beetle` | yes | `KMG_Summoning_Unit_GiantStagBeetle` | `fc294cc691284525814324ac5bc5bf75` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-stag-beetle` | PASS |
| `large-air-elemental` | yes | `KMG_Summoning_Unit_LargeAirElemental` | `6fce34709ee54f5a85fd75ea5c6d30be` | `SummonedAirElementalLarge` / `ElementalAir` | `KMG_SummonPortrait_large-air-elemental` | PASS |
| `large-earth-elemental` | yes | `KMG_Summoning_Unit_LargeEarthElemental` | `4fee8d80c1944b1f96d905aaf15c61b3` | `SummonedEarthElementalLarge` / `ElementalEarth` | `KMG_SummonPortrait_large-earth-elemental` | PASS |
| `large-fire-elemental` | yes | `KMG_Summoning_Unit_LargeFireElemental` | `e9371f3a9bdd4f8780db58cdefb95292` | `SummonedFireElementalLarge` / `ElementalFire` | `KMG_SummonPortrait_large-fire-elemental` | PASS |
| `large-water-elemental` | yes | `KMG_Summoning_Unit_LargeWaterElemental` | `2c6483c5025d4dcba441cc4c805fa784` | `SummonedWaterElementalLarge` / `ElementalWater` | `KMG_SummonPortrait_large-water-elemental` | PASS |
| `dire-lion` | yes | `KMG_Summoning_Unit_DireLion` | `56c64aa6765a4c37a1b30c0c5b31427b` | `SmilodonSummoned` / `Smilodon` | `KMG_SummonPortrait_dire-lion` | PASS |
| `ankylosaurus` | yes | `KMG_Summoning_Unit_Ankylosaurus` | `10c80d5cfa594332bd3e5127799e426f` | `CR6_HodagStandard` / `Hodag` | `KMG_SummonPortrait_ankylosaurus` | PASS |
| `bralani-azata` | yes | `KMG_Summoning_Unit_BralaniAzata` | `fb640167f05541f2963de3d31e217d11` | `AzataBralaniSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_bralani-azata` | PASS |
| `salamander` | yes | `KMG_Summoning_Unit_Salamander` | `f8fb103168d74b4c93182437e5d2b4e4` | `CR1_LizardfolkStandard` / `Lizardfolk` | `KMG_SummonPortrait_salamander` | PASS |
| `cyclops` | yes | `KMG_Summoning_Unit_Cyclops` | `a85dfadbca3340f4aad99c1cc36e40b3` | `CR5_CyclopStandard` / `Cyclop2H` | `KMG_SummonPortrait_cyclops` | PASS |
| `dire-bear` | yes | `KMG_Summoning_Unit_DireBear` | `11b15a81cea1498babfdb57af1b53c41` | `CR7_BearDire` / `Bear` | `KMG_SummonPortrait_dire-bear` | PASS |
| `dire-tiger` | yes | `KMG_Summoning_Unit_DireTiger` | `d15ee151c2274f9f86ab523f111bc3af` | `SmilodonSummoned` / `Smilodon` | `KMG_SummonPortrait_dire-tiger` | PASS |
| `huge-air-elemental` | yes | `KMG_Summoning_Unit_HugeAirElemental` | `f6b8223178ca4ec7b9b8aac61f1fd0f8` | `SummonedAirElementalHuge` / `ElementalAir` | `KMG_SummonPortrait_huge-air-elemental` | PASS |
| `huge-earth-elemental` | yes | `KMG_Summoning_Unit_HugeEarthElemental` | `f8ba7baab64e42b2951d52e227a20b4f` | `SummonedEarthElementalHuge` / `ElementalEarth` | `KMG_SummonPortrait_huge-earth-elemental` | PASS |
| `huge-fire-elemental` | yes | `KMG_Summoning_Unit_HugeFireElemental` | `fe3b06c44a2d4d36be8f549b14042f73` | `SummonedFireElementalHuge` / `ElementalFire` | `KMG_SummonPortrait_huge-fire-elemental` | PASS |
| `huge-water-elemental` | yes | `KMG_Summoning_Unit_HugeWaterElemental` | `974d58335c9149b687140d09ef8e47a2` | `SummonedWaterElementalHuge` / `ElementalWater` | `KMG_SummonPortrait_huge-water-elemental` | PASS |
| `elephant` | yes | `KMG_Summoning_Unit_Elephant` | `9dd8544097234f05bcd35d400d91b510` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_elephant` | PASS |
| `erinyes-devil` | yes | `KMG_Summoning_Unit_ErinyesDevil` | `bf08d0f66ffc4ed78dd40e5f7e68ae9d` | `CR8_ErinyesDevilStandard` / `Empty_Portrait` | `KMG_SummonPortrait_erinyes-devil` | PASS |
| `invisible-stalker` | yes | `KMG_Summoning_Unit_InvisibleStalker` | `3cd7b4f2c65b4d35929dd4d969dfaa41` | `SummonedAirElementalMedium` / `ElementalAir` | `KMG_SummonPortrait_invisible-stalker` | PASS |
| `shadow-demon` | yes | `KMG_Summoning_Unit_ShadowDemon` | `627c1841a5eb4e32b1a94c0f43ec8a60` | `SoulEaterSummoned` / `SoulEater` | `KMG_SummonPortrait_shadow-demon` | PASS |
| `shadow-mastiff` | yes | `KMG_Summoning_Unit_ShadowMastiff` | `c0c955211eda4de18df0dcb4870eede2` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_shadow-mastiff` | PASS |
| `succubus` | yes | `KMG_Summoning_Unit_Succubus` | `0c908145873f4b67a188397ca5f46da1` | `CR7_Nymph` / `Nymph` | `KMG_SummonPortrait_succubus` | PASS |
| `shambling-mound` | yes | `KMG_Summoning_Unit_ShamblingMound` | `c0680a67c42f4d17b3b55c011a8f9c56` | `CR6_ShamblingMound` / `ShamblingMound` | `KMG_SummonPortrait_shambling-mound` | PASS |
| `greater-air-elemental` | yes | `KMG_Summoning_Unit_GreaterAirElemental` | `c35e1370ca28417098d6a3b9835b0ba5` | `SummonedAirElementalGreater` / `ElementalAir` | `KMG_SummonPortrait_greater-air-elemental` | PASS |
| `greater-earth-elemental` | yes | `KMG_Summoning_Unit_GreaterEarthElemental` | `17c4fcc106df43af980fcea7a0877e04` | `SummonedEarthElementalGreater` / `ElementalEarth` | `KMG_SummonPortrait_greater-earth-elemental` | PASS |
| `greater-fire-elemental` | yes | `KMG_Summoning_Unit_GreaterFireElemental` | `7c7c5326601c4b2e8237c33969c47914` | `SummonedFireElementalGreater` / `ElementalFire` | `KMG_SummonPortrait_greater-fire-elemental` | PASS |
| `greater-water-elemental` | yes | `KMG_Summoning_Unit_GreaterWaterElemental` | `72d45b40b6764ab090a3dff639aa678a` | `SummonedWaterElementalGreater` / `ElementalWater` | `KMG_SummonPortrait_greater-water-elemental` | PASS |
| `mastodon` | yes | `KMG_Summoning_Unit_Mastodon` | `e129ffc2768d47c5bee61bb99b0c8703` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_mastodon` | PASS |
| `roc` | yes | `KMG_Summoning_Unit_Roc` | `439e955cb0fd41daafd0478d3641615a` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_roc` | PASS |
| `dire-crocodile` | yes | `KMG_Summoning_Unit_DireCrocodile` | `95bd522bb3a444d49986ba12b7822f10` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_dire-crocodile` | PASS |
| `bebelith` | yes | `KMG_Summoning_Unit_Bebelith` | `9d7d36e71ea141258509b8a32557577e` | `CR7_GiantSpiderDoombringing` / `GiantSpider` | `KMG_SummonPortrait_bebelith` | PASS |
| `giant-flytrap` | yes | `KMG_Summoning_Unit_GiantFlytrap` | `287490e71bed46bd92adef60489ecb4c` | `CR10_GiantFlytrapStandard` / `GiantFlytrap` | `KMG_SummonPortrait_giant-flytrap` | PASS |
| `elder-air-elemental` | yes | `KMG_Summoning_Unit_ElderAirElemental` | `1243adb4aecc4eb5ae54b2b54cfe1dec` | `SummonedAirElementalElder` / `ElementalAir` | `KMG_SummonPortrait_elder-air-elemental` | PASS |
| `elder-earth-elemental` | yes | `KMG_Summoning_Unit_ElderEarthElemental` | `2bdfdec402384e689351bad9a0d2d8af` | `SummonedEarthElementalElder` / `ElementalEarth` | `KMG_SummonPortrait_elder-earth-elemental` | PASS |
| `elder-fire-elemental` | yes | `KMG_Summoning_Unit_ElderFireElemental` | `20116595e2554c758a29b3b32553f0e5` | `SummonedFireElementalElder` / `ElementalFire` | `KMG_SummonPortrait_elder-fire-elemental` | PASS |
| `elder-water-elemental` | yes | `KMG_Summoning_Unit_ElderWaterElemental` | `54acc1934a55479ca43e826ed6e7d14c` | `SummonedWaterElementalElder` / `ElementalWater` | `KMG_SummonPortrait_elder-water-elemental` | PASS |
| `purple-worm` | yes | `KMG_Summoning_Unit_PurpleWorm` | `8739fe771e5b4f3fa3acbb10dd969892` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_purple-worm` | PASS |
| `ghaele-azata` | yes | `KMG_Summoning_Unit_GhaeleAzata` | `e7eccabdc8cd474d86475586792c03c7` | `AzataGhaelSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_ghaele-azata` | PASS |
| `pixie` | yes | `KMG_Summoning_Unit_Pixie` | `396881ade24e4ddba188dc2e7ff481f9` | `CR1_Nixie` / `Nixie` | `KMG_SummonPortrait_pixie` | PASS |
| `giant-wasp` | yes | `KMG_Summoning_Unit_GiantWasp` | `156a063aaec7449992d2021d53341085` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_giant-wasp` | PASS |
| `stirge` | no (withheld) | `KMG_Summoning_Unit_Stirge` | `c49f994ea2a840d98108b2848781774c` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_stirge` | PASS |
| `aurochs` | yes | `KMG_Summoning_Unit_Aurochs` | `40bf3f1e2b094e83ba2ab498c3d5d603` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_aurochs` | PASS |
| `bison` | yes | `KMG_Summoning_Unit_Bison` | `d1f0439382454d908f2086dda33eafcc` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_bison` | PASS |
| `rhinoceros` | yes | `KMG_Summoning_Unit_Rhinoceros` | `e0d2c0f0589e467792aff319fdb2b6f9` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_rhinoceros` | PASS |
| `woolly-rhinoceros` | yes | `KMG_Summoning_Unit_WoollyRhinoceros` | `8c4a8e045ca844a8bd42f614707b8e74` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_woolly-rhinoceros` | PASS |
| `viper` | yes | `KMG_Summoning_Unit_Viper` | `d8be82543ab64dc988c33e9f13608bad` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_viper` | PASS |
| `constrictor-snake` | yes | `KMG_Summoning_Unit_ConstrictorSnake` | `f1a2eadf588e4c3b9fb670724d706364` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_constrictor-snake` | PASS |
| `ape` | yes | `KMG_Summoning_Unit_Ape` | `c53c3e23097e4f25a411c50ce2868c60` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_ape` | PASS |
| `dire-ape` | yes | `KMG_Summoning_Unit_DireApe` | `5482b49785a3492aa7d29a4ee575cd66` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_dire-ape` | PASS |
| `giant-scorpion` | yes | `KMG_Summoning_Unit_GiantScorpion` | `4b5ec54e8e7a4e57b838541006affd3d` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-scorpion` | PASS |
| `girallon` | yes | `KMG_Summoning_Unit_Girallon` | `b9187f2a07a74c84b07d5a34280753a6` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_girallon` | PASS |
| `xill` | yes | `KMG_Summoning_Unit_Xill` | `440bd760511f4bdd9515ff59a0c5eb33` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_xill` | PASS |
| `triceratops` | yes | `KMG_Summoning_Unit_Triceratops` | `ce540cd4b3b74861a4a91458d742a692` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_triceratops` | PASS |
| `stegosaurus` | yes | `KMG_Summoning_Unit_Stegosaurus` | `39cab5be4dc148339f4ce8b0157bd716` | `CR6_HodagStandard` / `Hodag` | `KMG_SummonPortrait_stegosaurus` | PASS |
| `deinonychus` | yes | `KMG_Summoning_Unit_Deinonychus` | `10a13f87e3664e30b642a3c59cc8c700` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_deinonychus` | PASS |
| `tyrannosaurus` | yes | `KMG_Summoning_Unit_Tyrannosaurus` | `04b1a75a76544a9581ee832045f8227a` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_tyrannosaurus` | PASS |
| `brachiosaurus` | yes | `KMG_Summoning_Unit_Brachiosaurus` | `9093aef39ecc416092ef68655a7e56fb` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_brachiosaurus` | PASS |
| `gorgon` | yes | `KMG_Summoning_Unit_Gorgon` | `ce2fc58942b54e558208ea67d595cb7b` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_gorgon` | PASS |
| `bulette` | yes | `KMG_Summoning_Unit_Bulette` | `f84a0babcc864630bae225b2019d6bba` | `CR7_BuletteStandart` / `Bulette` | `KMG_SummonPortrait_bulette` | PASS |
| `griffon` | yes | `KMG_Summoning_Unit_Griffon` | `4b55d1f8da5e4a3ea23f9a95191f8737` | `CR5_ManticoreStandard` / `Manticore` | `KMG_SummonPortrait_griffon` | PASS |
| `hill-giant` | yes | `KMG_Summoning_Unit_HillGiant` | `58770207fd8c4084a0d9addbac591616` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_hill-giant` | PASS |
| `stone-giant` | yes | `KMG_Summoning_Unit_StoneGiant` | `956501547dd54479b329a315aab07a05` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_stone-giant` | PASS |
| `fire-giant` | yes | `KMG_Summoning_Unit_FireGiant` | `65481592185641c99c502e21ba9a5fa2` | `FrostGiantSummoned` / `FrostGiant` | `KMG_SummonPortrait_fire-giant` | PASS |
| `ettin` | yes | `KMG_Summoning_Unit_Ettin` | `2481bc73abd64ef0ae581b48438b5404` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_ettin` | PASS |
| `cloud-giant` | yes | `KMG_Summoning_Unit_CloudGiant` | `ff4696c6336141a3b2236cb0a22c86f3` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_cloud-giant` | PASS |
| `storm-giant` | yes | `KMG_Summoning_Unit_StormGiant` | `4b1143140c614a0aaaa1118c8a44c9ae` | `FrostGiantSummoned` / `FrostGiant` | `KMG_SummonPortrait_storm-giant` | PASS |
| `lemure` | yes | `KMG_Summoning_Unit_Lemure` | `80fd27e965b943b5b307a8c0a45cfb96` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_lemure` | PASS |
| `dretch` | yes | `KMG_Summoning_Unit_Dretch` | `717d3fdec4284e42b21efe83bc03eb25` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_dretch` | PASS |
| `hound-archon` | yes | `KMG_Summoning_Unit_HoundArchon` | `4685038956fc47c7b18d5acf1d7d8d7b` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_hound-archon` | PASS |
| `trumpet-archon` | yes | `KMG_Summoning_Unit_TrumpetArchon` | `b6359294f13a4adab626d14c5ededd50` | `MovanicDevaSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_trumpet-archon` | PASS |
| `astral-deva` | yes | `KMG_Summoning_Unit_AstralDeva` | `2814e9aa55e34dda8f3322d84a5fe5e2` | `MovanicDevaSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_astral-deva` | PASS |
| `satyr` | yes | `KMG_Summoning_Unit_Satyr` | `b0dab767cb2948ca9f918bf7d7c3dbb4` | `CR4_SatyrShortbowStandard` / `Empty_Portrait` | `KMG_SummonPortrait_satyr` | PASS |
| `lillend-azata` | yes | `KMG_Summoning_Unit_LillendAzata` | `71a8adf916b4409680023e853d28583b` | `OwlbearAttack_CapitalGuardCaptain1` / `Empty_Portrait` | `KMG_SummonPortrait_lillend-azata` | PASS |
| `bearded-devil` | yes | `KMG_Summoning_Unit_BeardedDevil` | `182032f380314a7581d0016c443945a2` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_bearded-devil` | PASS |
| `kyton-chain-devil` | yes | `KMG_Summoning_Unit_KytonChainDevil` | `96045f5c18ef4f4396a6c466a2be1a92` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_kyton-chain-devil` | PASS |
| `barbed-devil` | yes | `KMG_Summoning_Unit_BarbedDevil` | `a8ed4e16ec4b48a5bdfdc47f7f1b9cd0` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_barbed-devil` | PASS |
| `bone-devil` | yes | `KMG_Summoning_Unit_BoneDevil` | `74652de839a94feba3c2fb67b536437c` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_bone-devil` | PASS |
| `babau` | yes | `KMG_Summoning_Unit_Babau` | `1512476cf6674704b6b0bcc3b67b48d9` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_babau` | PASS |
| `hezrou` | yes | `KMG_Summoning_Unit_Hezrou` | `0c97e35a3e8e4d2db937a23775ea13ac` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_hezrou` | PASS |
| `vrock` | yes | `KMG_Summoning_Unit_Vrock` | `5f4aa097fc3e4fa4a627ce5d5be6509e` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_vrock` | PASS |
| `glabrezu` | yes | `KMG_Summoning_Unit_Glabrezu` | `3824e7589ae548f79a3eb8369f50359f` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_glabrezu` | PASS |
| `nalfeshnee` | yes | `KMG_Summoning_Unit_Nalfeshnee` | `9930f78084f0fe78d1de849aed58b753` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_nalfeshnee` | PASS |
| `ice-devil` | yes | `KMG_Summoning_Unit_IceDevil` | `6b5b789a08584fab84a00e7da42e1d2c` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_ice-devil` | PASS |
