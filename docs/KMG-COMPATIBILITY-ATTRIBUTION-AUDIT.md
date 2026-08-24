# KMG Compatibility Attribution Audit

## Result

This audit found no KMG production defect in any assigned track. No
compatibility repair, blueprint identity change, asset rebuild, or balance
change was made. After the audit handoff, the repository owner separately
authorized release `0.0.97`, promoting this report and the already-qualified
guarded diagnostics without changing the audit classifications.

| Track | Final classification | Result |
|---|---|---|
| A: Favored Class / Helpful | Not reproduced | The reported `ComponentAppliedOnceOnLevelUp.OnFactActivate` exception was absent from every controlled process. KMG Helpful publication was structurally valid, transactional, and exactly-once. The four observed Favored Class JSON failures are External. |
| B: polymorph / view teardown | Not reproduced | Apply, replace, restore, deactivate, disposal, and `UnitFxVisibilityManager.Update` fingerprints were all zero in repeated behavior-negative and behavior-positive processes. |
| C: KMG asset warnings | External | Every reproduced warning family was independent of KMG asset-family enablement. KMG's complete bundle inventory contained no unsupported shader, particle system, missing component, lightmapped renderer, zero-area readable mesh, or material lacking the `_MainTex` property. The reported zero-area family was not reproduced. |

"External" in this report means external to the KMG-controlled behavior or
bundle under test. It does not identify which game or third-party component
emitted a warning unless the evidence does so directly.

## Starting condition

The audit began only after the firearm-audio restoration was merged and released
on `master`.

| Field | Starting value |
|---|---|
| Starting `master` and `origin/master` | `59f5c102b462668bf6b852a0bc7f64b95e37f5cd` |
| Release tag / version | `v0.0.96` / `0.0.96` |
| Release commit subject | `release: prepare 0.0.96 firearm audio restoration` |
| Mission branch | `codex/kmg-compatibility-attribution-audit`, created from current `origin/master` |
| Installed UMM ID / version | `KingmakerGunslinger` / `0.0.96` |
| Installed released DLL SHA-256 | `E6DE01153A63C0509DF32ED287AD0C03DBD82F57A13BA188E5ACE49DBD366C77` |
| Installed released DLL MVID | `345e1230-d7c9-4e64-b375-c9fd9f5da310` |
| Installed released package SHA-256 | `7224DA57FE9FE1F609CBEBD7560BE80F1C5470A322A3091FF6F49F81E09F2E84` |
| Starting deployment manifest | `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260824T1157063660001Z\deployment.json` |
| Starting `FeatureModules.json` | schema 8; all nine modules `true`; 286 bytes |
| Starting `FeatureModules.json` SHA-256 | `28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59` |

`git fetch --prune origin` completed before branching. The tracked worktree
was clean, and local `master` and `origin/master` were identical. The released
`Info.json`, release notes, changelog, package metadata, and firearm-audio
qualification all identified version 0.0.96.

The ordinary installed mod inventory at startup was:

| UMM mod | Version |
|---|---|
| Bag of Tricks | 1.16.4 |
| Call of the Wild | 1.14.4c-2.1 |
| Cheat Menu | 1.2.3 |
| Kingmaker Buff Planner | 0.0.11 |
| Kingmaker Dice Roller | 0.1.2 |
| Kingmaker Gunslinger | 0.0.96 |
| Races Unleashed | 1.0.11 |
| Tweak or Treat | 1.1.0 |
| ZFavoredClass | 1.3.1 |

The exact controlled dependency fingerprints were:

- Call of the Wild DLL: `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
  MVID `8caab254-aacf-4811-8093-44b9184e6e53`.
- ZFavoredClass DLL:
  `DCD3ADF98D1A04C30D772381E7C56CE4BEFF35A98BCEA165AFF206A2F0AAC26C`,
  MVID `3efd38e7-8682-4b4d-8d53-e368a3664919`.
- ZFavoredClass `enable_traits=true` settings SHA-256:
  `BDCEED77D2BF4A31DD9E4EEB64EF9D55A42EF59D23F46ABCB1DDBCC6EF66754B`.

## Evidence boundary and source fingerprints

The originally reported full-stack log was not present as an immutable retained
artifact at audit start. Its approximate 40/52/16/9/4/3 warning counts therefore
remain unverified and were not treated as ground truth. The closest retained
full-stack log was independently normalized:

`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260823T1250159827166Z-human-in-harms-way-regression\output_log-human-0.0.93.txt`

Its SHA-256 is
`855CE57C81B5D5A07A1CFAB928C79BE8CED8AEADA54EAC357DDB722FE4A0AEE6`
and its byte length is 676,357.

| Normalized family | Exact retained-log count |
|---|---:|
| Unsupported-shader "all passes removed" | 10 |
| Unsupported-shader fallback | 10 |
| Unsupported-shader GPU error | 10 |
| Invalid particle mesh/read-write | 39 |
| Missing serialized script | 4 |
| Lightmap-mode mismatch | 2 |
| Zero-surface-area particle mesh | 0 |
| Material missing `_MainTex` | 5 |
| `ComponentAppliedOnceOnLevelUp.OnFactActivate` | 0 |
| Any assigned polymorph/view fingerprint | 0 |

The ten shader names in each paired shader category normalized to:
`PF/Decals/ScreenSpaceDecal` 1, `PF/Particles` 3, `PF/Standard` 2,
`PF/StandardDynamic` 3, and `PF/Unlit` 1.

Fresh Favored Class controls independently reproduced these four exact startup
failures once per process:

| ZFavoredClass record | Missing GUID | Exact failure seam | Ownership |
|---|---|---|---|
| `Custom/bonus_charmed_life.json` | `57b8c544535f40918881cf4000021a40` | `KeyNotFoundException` from `CallOfTheWild.ExtensionMethods.Get<T>` during ZFavoredClass record load | Not present in KMG source or blueprint manifest |
| `Custom/bonus_panache.json` | `89b0082da456424380b5c541f63eee41` | same | Not present in KMG |
| `PrestigiousSpellcaster/arcane_archer.json` | `0fbf5e3fe02f4db19492659dc8a3c411` | same | Not present in KMG |
| `PrestigiousSpellcaster/deadeye_devotee.json` | `d8b4700568c14c25a31e77c0cd636175` | same | Not present in KMG |

Those four GUIDs occur in the installed ZFavoredClass JSON records and nowhere
in the KMG repository. KMG does not supply, remove, replace, or publish any of
them.

## Track A static ownership map

### Resolved Favored Class surfaces

KMG resolves, verifies, and observes the following exact ZFavoredClass surfaces:

| Surface | GUID / contract |
|---|---|
| Combat Traits | `43d763957f364315b5fff85f9e91ca51` |
| Racial Traits | `331ed3c4a988415785f71a37b826d0f1` |
| Equipment Traits | `af37d78d7bc5451d943b63356f438949` |
| First trait selection | `34e2812e0f8241bb9e1bee5240c9eb2e` |
| Second trait selection | `5253dcee502a49249bdd8bfdfe525e9f` |
| Adopted trait selection | `987e573c15e241c285e0fa1d5ac0a0a2` |
| Additional trait selection | `6a1f65b204a74c22b0f47e1e2c808441` |
| ZFavoredClass halfling Helpful | `c9bd9f6cc24f41e684a68e6510afc726` |
| Halfling race donor | `b0c3ef2729c498f47970bb50fa1acd30` |
| Lifecycle seam | exact static `ZFavoredClass.Traits.load(bool)` postfix, plus first-update reconciliation |

Resolution is fail-closed when the UMM entry, assembly, settings, blueprint
identity, selection structure, or component contract is absent or ambiguous.

### KMG entries that can reach Favored Class selections

| KMG entry | Blueprint name / GUID | Target | Components and references | Timing, idempotency, and rollback | Level-up lifecycle conclusion |
|---|---|---|---|---|---|
| `KMG.Traits.HelpfulCombat` | `KMG_HelpfulCombat_Trait` / `e4b29a7c8d5f4c1796ab03e1f72d8456` | ZFavoredClass Combat Traits `Features` and `AllFeatures`; also the exact CotW Aid Another contributor list | Initially no components. Reconciliation adds exactly one `PrerequisiteNoFeature` referencing ZFavoredClass Helpful; it adds the reciprocal exclusion to ZFavoredClass Helpful. No fact-grant, selection, level-up-only, or `ComponentAppliedOnceOnLevelUp` component. | Reconciled after exact CotW and Favored Class lifecycle callbacks. Publication compares blueprint identity and exact counts, appends uniquely, validates every surface, commits as one transaction, and restores original arrays/components on failure. It is omitted from the foreign selection when `bodyguard-feats` is off, traits are disabled, or an exact dependency contract is unavailable. | The feature may exist as a normal fact outside a level-up controller, but it contains no activation logic requiring a controller. KMG does not call or attach ZFavoredClass's level-up-only component. |
| `KMG.Traits.HeirloomWeapon.Nodachi.Selection` | `KMG_HeirloomWeapon_Nodachi_Selection` / `5ae9f898e45846d19d3802caf91e06b6` | ZFavoredClass Equipment Traits `AllFeatures` only | `AddStartingEquipment` references the exact masterwork Nodachi; one self-referencing `PrerequisiteNoFeature` prevents duplication. Children: proficiency `af205733f7fe49838edb37cdf1b90cbb` (`PrerequisiteNotProficient` + `AddProficiencies`), AOO `4caf60ed8b264701a3965288a65eebc2` (`PrerequisiteProficiency` + KMG AOO bonus), CMB `e17fafa6f75641f8a2e3fe4b6f71da78` (`PrerequisiteProficiency` + KMG carrier), hidden bonus `1a7a5d985fe740cc8442f04f0fe814d8` (`AddStatBonus`). | Published on the first UMM update after the complete `LoadDictionary` postfix chain, after exact Equipment Traits validation. Append is unique by identity; count must be exactly one. The transaction retains the original array and rolls it back on failure. It is not published if Eastern Weapons or the exact Favored Class contract is absent. | None of the selection or child components is `ComponentAppliedOnceOnLevelUp`. All references are non-null and validated before foreign publication. |

KMG does not publish a separate race-trait Helpful feature. It observes the
ZFavoredClass-owned halfling Helpful feature only to preserve mutual exclusion
and Aid Another value semantics.

### Aid Another, Bodyguard, and In Harm's Way

KMG's CotW adapter resolves the exact Aid Another rank configuration and adds
KMG Helpful as one contributor. It patches only the exact
`ContextRankConfig.GetValue` instance used by the two validated CotW Aid Another
buffs; it does not alter fact activation or level-up state.

The KMG-owned related identities are:

| Object | GUID | Components / lifecycle |
|---|---|---|
| Bodyguard | `b2baa3384b4d4328848cc07933b513be` | One native Combat Reflexes `PrerequisiteFeature` and one `AddFacts` grant of Use Bodyguard |
| Use Bodyguard | `ac31a9d5d34140978b7e778dc8d1e226` | Free, off-by-default activatable; applies the inert marker |
| Bodyguard marker | `a78147a3655f429883ad88e761ff9438` | Hidden persistent marker; no level-up component |
| In Harm's Way | `e481f30c8b6940e1b596e121443aa01e` | One KMG Bodyguard prerequisite and one `AddFacts` grant of Use In Harm's Way |
| Use In Harm's Way | `ca1e74f0e60747209a8b7cf3737243ea` | Free, off-by-default activatable; applies the inert marker |
| In Harm's Way marker | `57603d0b215e4ac6862bcdf9b5583568` | Hidden persistent marker; no level-up component |
| Immediate-action pending marker | `a92164067bad3a85b1da48db5a787686` | KMG-owned action-economy debt marker |
| Immediate-action charged-turn marker | `326e183f7791e83a38337c6a6d7a8644` | KMG-owned swift-action denial marker |

Bodyguard and In Harm's Way publish transactionally to native basic/fighter feat
catalogs, not to a Favored Class trait selection. Their mechanics consume the
resolved Aid Another value but do not invoke Favored Class activation code.

Static searches found no KMG construction, attachment, reflection lookup, or
Harmony patch for
`ZFavoredClass.NewMechanics.ComponentAppliedOnceOnLevelUp`. They also found no
null KMG feature reference, duplicate foreign selection entry, or incompatible
component reuse.

## Track B static ownership map

### Brown Fur Transmuter

All 25 permanent Brown Fur identities were audited:

| Symbol | GUID | Type |
|---|---|---|
| `KMG.BrownFur.Archetype` | `aafa6e62241bb14582de5f587c179329` | BlueprintArchetype |
| `KMG.BrownFur.PowerfulChange.Feature` | `b3bbed7e12463e4c434cd81eda7ab2dd` | BlueprintFeature |
| `KMG.BrownFur.PowerfulChange.SelectionAbility` | `48e76b097fc71f586d442a308eb11f87` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Strength.Ability` | `d2cb2236a6dc31b7ed70e27dc12d5a8a` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Dexterity.Ability` | `d16c77bcbff53fd3c1555869017bab3e` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Constitution.Ability` | `54a2f74043e000047041f273d1e559ad` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Intelligence.Ability` | `a6d77c07804e16d41a3c172c7f09f4ca` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Wisdom.Ability` | `84faeefe28992744fbf19b62e2eccb08` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Charisma.Ability` | `649b8ea4f5a155141bef5f9827675739` | BlueprintAbility |
| `KMG.BrownFur.PowerfulChange.Strength.Activatable` | `16c06d016437be9e9e6dac6211ff30a5` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Dexterity.Activatable` | `d1f274d1a129eedd8ef44efdb3426d7f` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Constitution.Activatable` | `434573bfac3915b1a611a1452917d1d9` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Intelligence.Activatable` | `bbef0eaabb277fcf2cbb22a82076e4f7` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Wisdom.Activatable` | `2e7cfb55db278e75a7bca01ac52e4100` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Charisma.Activatable` | `deac03b22537cb6f05c8323a384e9b93` | BlueprintActivatableAbility |
| `KMG.BrownFur.PowerfulChange.Strength.Buff` | `958e93bc70e6ae048e2e96193423915a` | BlueprintBuff |
| `KMG.BrownFur.PowerfulChange.Dexterity.Buff` | `aba507d99e1b4d6c6bda9233f708eb64` | BlueprintBuff |
| `KMG.BrownFur.PowerfulChange.Constitution.Buff` | `cea64eb942b294360344824a3795a351` | BlueprintBuff |
| `KMG.BrownFur.PowerfulChange.Intelligence.Buff` | `5bb5dd956df4d7bc2cf03e02bbd28d5f` | BlueprintBuff |
| `KMG.BrownFur.PowerfulChange.Wisdom.Buff` | `81ce31c8f868e0db5c4aa8a8e9cf1656` | BlueprintBuff |
| `KMG.BrownFur.PowerfulChange.Charisma.Buff` | `9fe5998e93963fec5ae91aed6a060ef0` | BlueprintBuff |
| `KMG.BrownFur.ShareTransmutation.Feature` | `b7e929dac874cd22d173ee8f4fe0bfa4` | BlueprintFeature |
| `KMG.BrownFur.ShareTransmutation.Activatable` | `8641e6c39ff133ad71f669e35e1ee688` | BlueprintActivatableAbility |
| `KMG.BrownFur.ShareTransmutation.Buff` | `215a03a25c8ff8b76114bf7513869d6c` | BlueprintBuff |
| `KMG.BrownFur.TransmutationSupremacy.Feature` | `c69cd7091219708f981272f2ac057135` | BlueprintFeature |

None contains a `Polymorph` component, changes `Prefab`, replaces a view,
attaches persistent FX, creates a summoned unit, or mutates a transformation
donor in place. The activatable marker buffs use an empty/default `PrefabLink`
and contain no FX or view component. Their lifetimes follow the activatable
toggle and KMG cleanup is request/scope-idempotent.

Brown Fur observes valid native/CotW transmutation casts. Its only polymorph
seam is a type-name classification of
`Kingmaker.UnitLogic.Buffs.Polymorph` so that KMG can adjust numeric
ability-score modifiers added by an already-owned cast. The relevant patches
are bounded to `ModifiableValue.AddModifier`, `BuffCollection.AddBuff`, and
`Buff.Remove`. KMG does not patch `Polymorph.Transition`,
`TryReplaceView`, `RestoreView`, fact activation/deactivation, unit disposal,
area unloading, animal-companion disposal, or `UnitFxVisibilityManager`.

The deterministic positive fixture used native Beast Shape II:

- wrapper ability `5d4028eb28a106d4691ed1b92bbb1915`;
- dire-wolf variant `6ceb82df566a42c8a77ccb7b76b09c1b`;
- native dire-wolf polymorph buff `8dc6510d31614345a8c718208fbac1f8`.

Both fresh processes applied the native buff, observed the exact +6 polymorph
Strength modifier, removed it, cleared all KMG scopes/reservations, and removed
the disposable units.

### Expanded Summoning

Expanded Summoning shallow-clones the exact donor unit's native `Prefab` link
and deep-clones/sanitizes unit components. It supplies no custom view prefab and
contains no `Polymorph`, `TryReplaceView`, `RestoreView`, persistent unit FX,
animal-companion lifecycle, or area-disposal component. It does not mutate a
donor blueprint in place.

The existing exact 55-donor runtime inventory is:

`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260813T2059275174812Z-observe-expanded-summoning-inventory`

Run `20260813T2059275174812Z-b38b55c019e4401c8ce14440f5ada4ed`
passed with runtime-result SHA-256
`E4B8ACD5F9FD9F9F9C2BD8EABACCBF2F89A3AEC5BE85A166401DAB4BB7C06D0B`:
55 exact donors, zero missing donors, zero donor-component isolation failures,
and zero occurrences of Polymorph, TryReplaceView, RestoreView, or
UnitFxVisibilityManager components. B4 was therefore excluded as required by
the mission's static-seam rule.

## Track C offline and runtime ownership inventory

All bundles were built for `StandaloneWindows64` with Unity `2018.4.10f1`.
The source builders strip Camera and Light components and bind Unity Standard
materials. The installed and packaged bundle bytes were unchanged by this
mission:

| Family | Bundle / SHA-256 / bytes | Paths / prefabs | Runtime inventory |
|---|---|---:|---|
| C-FIREARMS | `kingmakergunslinger.firearms` / `B3CFFB49BA32AF10DB12470401A58F6DFF0EAD9F219F87E41D9EC138D62FBAEB` / 18,172,963 | 19 / 14 | 128 renderers, 128 materials, 128 meshes |
| C-SPEARS | `kingmakergunslinger.elvenbranchedspear` / `A59DC61CE246A7F5931F22494C4C52CE39C6E96312F3448FB9138A0AC0D7DC9B` / 127,369 | 6 / 6 | 90 renderers, 90 materials, 90 meshes |
| C-EASTERN | `kingmakergunslinger.easternweapons` / `AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B` / 365,592 | 24 / 24 | 380 renderers, 404 materials, 380 meshes |
| Manifest | `asset-bundle-manifest.json` / `B566364E31BAE00F1D54037035CB7F82F92430AA94842B94E1EDD4A8E7DC168F` | schema 1 | Unity/build target and every prefab binding recorded |

The 49 exact asset paths were:

- Firearms: five `assets/approvedaudio/*.wav` clips, plus
  `blunderbuss`, `blunderbussbelt`, `musket`, `musketbelt`,
  `musketclearancestock`, `musketminimalcontrol`, `musketpassthrough`,
  `pistol`, `pistolbelt`, `pistolduelist`, `pistollastword`, `revolver`,
  `rifle`, and `riflebelt` prefabs beneath `assets/approvedmodels/`.
- Spears: `elvenbranchedspear`, `elvenbranchedspearback`,
  `elvenbranchedspearcrown`, `elvenbranchedspearcrownback`,
  `elvenbranchedspearthorn`, and `elvenbranchedspearthornback` beneath
  `assets/elvenbranchedspear/`.
- Eastern Weapons: held/stored pairs for `wakizashi`, `wakizashipetal`,
  `wakizashimoon`, `wakizashicapstone`, `katana`, `katanareed`,
  `katanaregal`, `katanacapstone`, `nodachi`, `nodachicleaver`,
  `nodachititan`, and `nodachicapstone` beneath
  `assets/easternweapons/`.

The complete row-level inventory of every asset path, instantiated prefab,
component, renderer, material, shader name and `isSupported` value, mesh
readability/vertices/triangles/surface area, lightmap index, and cleanup result
is retained at:

`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260824T1341156537512Z-observe-kmg-compatibility-asset-attribution\kmg-compatibility-asset-attribution.json`

That file has SHA-256
`49F08838FC60BB1CAF90A6789E56D790267EF7B6DF76F00088CFA4567D768A0D`.
Its totals are 49 paths, 44 prefabs, 598 renderers, 622 materials, and 598
meshes. Every material used supported `Standard` and had the `_MainTex`
property. Every mesh was readable and had positive surface area. Counts were
zero for ParticleSystem, ParticleSystemRenderer, missing serialized component,
Camera, Light, lightmapped renderer, unsupported/non-Standard shader, missing
`_MainTex`, inspection error, and zero-area readable mesh. Every temporary
instance was destroyed before PASS; no save API or player setting was used.

## Controlled configurations and restoration

Every process launched through Steam App ID 640820 with the repository-owned
guarded request mechanism. No direct `Kingmaker.exe` launch, OCR, mouse
automation, valued save, or baseline-save write was used.

| Matrix ID | Profile and exact KMG behavior |
|---|---|
| A1 | Not run: a KMG-absent process cannot service the KMG-owned guarded observer. A2 supplies the cheapest valid behavior-negative publication control. |
| A2 | CotW 1.14.4c-2.1 + ZFavoredClass 1.3.1 + KMG; `bodyguard-feats=false`, all other modules true. Helpful is absent from the foreign Combat Traits arrays. |
| A3 | Same dependencies; only `bodyguard-feats=true` and every other KMG feature module false. |
| A4 | Same dependencies; all nine KMG modules true. |
| A5 | Not run: no exact affected save/level-up trigger was retained, and expanding the mod set cannot substitute for that trigger after zero target frames in A2-A4. |
| B1 | Not run for the same guarded-runner reason; no KMG-absent result is claimed. |
| B2 | All-loadable local profile; Brown Fur false, Expanded Summoning false, other modules true; same `KMG_AUTOMATION_WORKING` save. |
| B3 | Same; Brown Fur true, Expanded Summoning false; same save plus exact native Beast Shape II fixture. |
| B4 | Excluded after static/runtime inventory proved no Expanded Summoning polymorph/view seam. |
| B5/B6 | Both candidate modules and the complete KMG feature set true; these resolve to the same all-nine-true bytes. Same save, repeated fresh processes. |
| B7 | The all-loadable local profile was used for B2/B3/B6. The historical reported trigger was unavailable, so additional unrelated mods were not added after the target fingerprints remained zero. |
| C1 | KMG-only; all three KMG custom-asset families request-locally suppressed. |
| C2 | KMG-only; firearms enabled, spears/eastern suppressed. |
| C3 | KMG-only; spears enabled, firearms/eastern suppressed. |
| C4 | KMG-only; Eastern Weapons enabled, firearms/spears suppressed. |
| C5 | KMG-only; all three families enabled. |
| C6 | Not needed: C1-C5 and the independent retained full-stack recount settled KMG-bundle attribution. |

The request-local C suppression is unavailable during ordinary gameplay, does
not modify `FeatureModules.json`, supplies no player-facing option, writes no
save state, and disappears on the next normal launch.

### FeatureModules transactions

| Configuration | Retained staged SHA-256 | State |
|---|---|---|
| Ordinary / A4 / B5-B6 / C1-C5 | `28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59` | all nine true |
| A2 | `1456298DAC0597EFB02E7E4ECC36608D4824CDBA2FC42EEAECDFA1FC0DD64912` | Bodyguard false; other eight true |
| A3 strict | `5958BF834B819B3E4719497FF3BC73A95D6093F3083E27A937EDF93DA4AA798A` | Bodyguard true; other eight false |
| B2 | `447CDC6097FB7CB0DF5A3AFA619F120341958E823213B15FB4116F7DBEA6461B` | Brown Fur and Expanded Summoning false; others true |
| B3 | `DB44373C9F73BC19AFE79A5D8AA9093AC2331E9454416FD9FC902C470F6DD80B` | Brown Fur true, Expanded Summoning false; others true |

Successful profile transaction IDs were
`compat-20260824T133438Z-eb6b04f07adb`,
`compat-20260824T133629Z-e63341f9ee4b`,
`compat-20260824T133822Z-d352279d9c17`,
`compat-20260824T133942Z-6a4318aa5d88`,
`compat-20260824T134102Z-c45012841efb`,
`compat-20260824T134423Z-037aebd89499`,
`compat-20260824T135009Z-9f7f78593519`,
`compat-20260824T135658Z-93aa068e8695`,
`compat-20260824T140329Z-2c9996547998`,
`compat-20260824T141448Z-0b1cdd1589c7`, and
`compat-20260824T144234Z-a20b8bdce5ba`.

Every transaction began with ordinary hash
`28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59`
and restored the exact original bytes to that hash. The final A3 restoration
record is
`C:\Dev\KingmakerGunslingerLab\compatibility-state\compat-20260824T144234Z-a20b8bdce5ba\feature-modules-restoration.json`.

One attempted A3 invocation,
`compat-20260824T144001Z-643177552730`, failed closed before launch because its
reusable build attestation predated the diagnostics commit. It restored exact
configuration bytes and contributed no runtime result.

Incidental warning counts were stable within each dependency profile. This is
additional attribution evidence, not a claim that those profiles own the
warnings:

| Processes | Unsupported all-passes/fallback/GPU | Particle | Missing script | Lightmap | Zero area | Missing `_MainTex` |
|---|---:|---:|---:|---:|---:|---:|
| All six A2/A3/A4 observers | 3/3/3 | 0 | 4 | 1 | 0 | 0 |
| All eight B2/B3/B5-B6 save/fixture processes | 4/4/4 | 0 | 7 | 1 | 0 | 0 |
| All five KMG-only C processes | 0/0/0 | 0 | 4 | 1 | 0 | 0 |

## Track A runtime results

The same save-free observer trigger was used throughout. A2 and strict A3 were
each repeated in fresh processes; the full configuration was also observed in
two fresh processes.

| Run | Feature state | Combat / equipment result | Four JSON files | Target component count | Runtime-result / observer / raw-log SHA-256 |
|---|---|---|---:|---:|---|
| A2-1 `20260824T1346283313831Z-1034c0eeb8054170b486926c6fd4f0ce` | Bodyguard off | Combat 14; KMG Helpful 0; equipment 50 with KMG Nodachi 1 | 1 each | 0 | `3B2CE7DD1DFF7E2ECB8E51FD85E9A1F70EFE4D855483AEB6B81989AE5469AD81` / `A06BE6E89B9B251E7016424D41027D8D743905299255BDB7BBD4CAA2B189C708` / `A6FC5376B80FCD31BB799FFABD879A1E49A7389A5C7081C7DC7ED4F27990CD67` |
| A2-2 `20260824T1348091157905Z-2c619619e6d9482392332e29627785cc` | same | same | 1 each | 0 | `FB4DF3853AC70F7067878F0BF208B8AD98321B3A28443255D57FD95F2DAC2EA5` / `A06BE6E89B9B251E7016424D41027D8D743905299255BDB7BBD4CAA2B189C708` / `986FAA047BB9312FEF001623B058E634145C28A855E6D9BCCD57004F1D8C5274` |
| A3-1 `20260824T1444383330694Z-eafd223e60c94498ae561ab751b0976d` | only Bodyguard on | Combat 15; KMG Helpful exactly 1; equipment 49 with KMG Nodachi 0 | 1 each | 0 | `7CA622CBF5DD679949FA8B200C0B4DD62D49178B5B58F7998B064FF0CA07E904` / `95F120E755153E6FFDA3F2ABAE2CB924277A78E0C6D708EC0107DF0CD28888B3` / `3F8A9D8848679FFE8F737710E5647D27C06DF829988984A893379D77A728831D` |
| A3-2 `20260824T1446172755872Z-c12b88e2186f4de8ae0f266d90671791` | same | same | 1 each | 0 | `181C4774DC9312FE47FBCB0CD03182B6993431C5874852F42C80C91A7D861B82` / `95F120E755153E6FFDA3F2ABAE2CB924277A78E0C6D708EC0107DF0CD28888B3` / `C28B520F55DB432135F7F78669EE04421FBED03B72268C5A81DF7B14619AD32E` |
| A4-1 `20260824T1352112297107Z-0aca5021d2e4499f91e00583ad24c364` | all on | Combat 15, KMG Helpful 1; equipment 50, KMG Nodachi 1 | 1 each | 0 | `BB29444269A691C732C1BA1F28000BCC6EC8182B6DB5A35DF4AB6D16D56C30B3` / `83F9AF57C49A75D0B7AAEFB76195B11645C7B8CFEA8A2471D434F49C5DD4B56F` / `5B2725496841572A4EA768E9C09C7C2F60994B7E4BA112971ECFC2406D536054` |
| A4-2 `20260824T1353519963783Z-dcd898fac0234332aae1413050fb37c4` | same | same | 1 each | 0 | `5CA17E28101C9C21C5D3CA1581E57B59F97F3C7E7C5B350BA4B777E95C3D021F` / `83F9AF57C49A75D0B7AAEFB76195B11645C7B8CFEA8A2471D434F49C5DD4B56F` / `ADCF25D6773C1A93CA174570CF0E8D8A437FC36A01B04E6D65675AD370C140AB` |

In A3/A4, the five publication surfaces were exact: CotW contributor list
6 -> 7, KMG Helpful components 0 -> 1, ZFavoredClass Helpful components 1 -> 2,
Combat Traits `Features` 0 -> 1, and Combat Traits `AllFeatures` 14 -> 15. The
transaction committed once. The KMG contributor was one exact blueprint
identity; the pre-existing CotW/ZFavoredClass contributor multiplicities were
preserved.

Verdicts:

- Four Favored Class JSON failures: **External**. The missing GUIDs and files
  are not KMG-owned.
- `ComponentAppliedOnceOnLevelUp.OnFactActivate` exceptions: **Not
  reproduced**. No affected save, character, fact, or exact level-up trigger was
  available, so the absence of frames is not promoted to an External verdict.
- KMG Helpful publication validity: **no KMG defect found**. Both absence and
  presence controls behaved exactly, with non-null references, no duplicate,
  no level-up-only component, and exact rollback support.

## Track B runtime results

Every listed warning collector recorded the ordered target vector
`Transition/TryReplaceView/RestoreView/OnFactActivate/OnFactDeactivate/UnitDescriptor.Dispose/UnitFxVisibilityManager.Update = 0/0/0/0/0/0/0`.

| Configuration / run | Trigger | Runtime-result / summary / raw-log SHA-256 |
|---|---|---|
| B2-1 `20260824T1358597319940Z-1b38ed93ae5c43f186d061c09f2530b9` | `working-save-smoke` | `9FF2A2E9B4AB3F22A62077C3DE5081CE0DF87A944DE0E865DF2855D9B0F0D6A0` / `E891DCAE719F30D2FD66EA650D618C38479940E8E4E6401153206C1CA0B5B41B` / `D9F805BD5FAA62CAC2DC3E6F050B7D07A588CDF2CCA8AD2EF213CA3B9A9BBB0E` |
| B2-2 `20260824T1401009122860Z-cfeee7d4a7ee4513a3890e7560d372a9` | same | `AE1E8D2DFA33CED04343688601A4E2282554E6655B946CAB19FB7D2650976393` / `2CEB45B375F66DCDEB3FFB9AC4B4AF94360A4F468D3F129FE8BE2E156F959262` / `D96F8F045F73B4E0E8305F61B3A66F48C5FAB2D2C24215962B142D496905F4DA` |
| B3-1 `20260824T1405297774342Z-622f51db5ae440bcb7183ec595065420` | same save, Brown Fur on | `FC8D3B6ADCB3525D0E1DB8EFC6E335D4D23C3CAF922CF33C08A1A0A25A304642` / `666133471AE75AA76FABF9DDA87CD6AA665315C9074C0DE4AB1A9A341088B6D1` / `6163A6F5573DE49A3015D44BAA2BCC75FA92A39E51BDC988E1790632552866D1` |
| B3-2 `20260824T1407291453921Z-50c3ea3292ae45708d13dc08c92af122` | same | `9E7A41AC16E8666148B3E53706E8FBCBDDEEDE0826C318895A856F0D599AEE05` / `60F206DC2C5313CC4D4DD81D6C2D5AABFBFD95CBA78BCD6DAC8F946E923A3CF9` / `E681C4A084C4F6BD834B152EA9CF36BD5B71205F6C82CE7D84643E606FCBFF23` |
| B3 native-1 `20260824T1409304534074Z-1d599b2c8fcd4825b0629d51b751ce31` | exact native Beast Shape II fixture | `3A6F3A7554831D5DB1681490884D1AE25FAE2468F0D3F68FA41DE4B608050AEE` / `4FEDBB59666E27405B215D5128A0CC424E4CF86C6D860DFA532502DCEF5D8C6F` / `2F5DC2439C5C8C201A5906BB790D526198E1633B33466B656144750FDB7B1A16`; fixture `F94790B569A2DC972F69B73EE82A3C5314BAAA19A23900AC2CD41EB7EA67D0A8` |
| B3 native-2 `20260824T1411338623873Z-874b27b9754e4c0a86626076b20439bb` | same, fresh process | `74A1C6A5D4EB468241F8CAA111DADC08C27657E80B414BB9F6BA3FCCC4C34889` / `A88695746B89AF571CC58AD27E44EC27AA31F1B07C5012849174D8D380B12121` / `3BFC1F5F9D648180228E54067418BE0BCAA0B9D4567FCD6E067AF61ABA0BD5C1`; fixture `16F276BC3850D710297CAA00BE6E4E1575AA95B6A18817663038379BD3233439` |
| B5/B6-1 `20260824T1416482686886Z-582b919989fd455d806d0ede0aacdfb9` | all modules, same save | `7ED7B2EEAAA4906E516747C0C7A190F27B9FE54F7A0EBA9180FA7F570EC39ED1` / `C4213AD162547163304608D9567237C12396E584C066CA9C376D7BAF911B1DE4` / `25F2B30B976EAAE879B1EE39D5CCCDCDB5AF36A8E3B1680809253186A9E83076` |
| B5/B6-2 `20260824T1418466773676Z-51462edafbcd4f698cdf9e6a3edfe1c5` | same, fresh process | `173ABD423BFA18D02C5AAD96D6B0DB0D0118509CA22B9E0E88FADD10588C5244` / `2CA806B3E1B8ED3FA9D34C350121F454BF7BFD215817870D2EE068F53CB2E6B4` / `0F3488FF39A322FECF90C93BBBE2F58C70B66950639CBDC3B2BE75B26E3DCEC3` |

The active all-loadable profile versions were Arms & Armor 1.0.10, Call of the
Wild 1.14.4c-2.1, KMG 0.0.96, and Toggle Custom Soundpacks 1.0.1.

Separate verdicts are all **Not reproduced**:

- polymorph apply/replace;
- polymorph restore/deactivation;
- unit/area disposal;
- `UnitFxVisibilityManager.Update`.

No source fingerprint containing the reported stack, affected unit identity,
blueprint identity, area, or active fact was retained. Consequently the clean
controls rule out a demonstrated KMG defect in these scenarios but do not
justify an External classification for the historical report.

## Track C runtime results

All five fresh KMG-only asset controls passed. Counts are
`unsupported all-passes/fallback/GPU | particle | missing script | lightmap |
zero area | missing _MainTex`.

| Run | Enabled family | Exact counts | Runtime-result / inventory / summary / raw-log SHA-256 |
|---|---|---|---|
| C1 `20260824T1334528565956Z-461bd2730bb841afa00e8a4eaaba3187` | none | `0/0/0 \| 0 \| 4 \| 1 \| 0 \| 0` | `CE5FE5A0793A98EDC115BFD20B22FAD64FFF509E474832522E2939DFDF0504DF` / `F59A678C7CD9B6E915BF0BD9E23D3D56CFE468A87DCB6FFB03AA7AC6A7ED7BC6` / `6AB06818938157121D3DCBE08284BA080017D214C6C848159A4FE5E48F1BFE2A` / `D04C0B6D968E2307B07C0F66222EF8B1A9040774776F924D6B60D92F5085185C` |
| C2 `20260824T1336427773056Z-97ac1a48ef2a44feab09a13d52d6d1d4` | firearms | same | `2A8CFCD4EB52A62F5EA391784EBD3BE8D49B2B6EFA765C6BA40FB5177B86E409` / `30724D3D919FB27698D4365BC74FEC342FAD3E4F0616B1BB733B157C9976E95F` / `FAFFBF9216865E630AFCF5665A3B70047B74A8B4597F747EF4FF11EA6BD041C5` / `5FCCB3B26EFF267561376BA9782D89109C76C6A8638C1D92C95C4EA19F14868B` |
| C3 `20260824T1338361085060Z-c6b408b525e940f18aeac96616068104` | spears | same | `D01964AC929FE86D616D03A40AAB59B0DD3670739A2C5B9E061F7A043A828223` / `A9546CB1B89445C9AA184D0BD3EC0BE462C9B43A7B7729AA9DD8AE29CECE17A1` / `34F384A4915AE5B8DCBFC973B839D485C89F7459D2393B6A9234C97DBBDF9DEF` / `8CC03394068FFEAAA3F8A0C4019CEEAD981D3E8AA84DEA47CB66EEDC54D805D7` |
| C4 `20260824T1339559788230Z-4b0d36bfd13d4600828f74cf80788e1d` | Eastern | same | `C4787CA57FF1C276904B7960D36FB432AD3A94E5BE4E71B9C8433334274F68F0` / `F0E9E67C3A24DA238B8301EE7682B107F80B3BDCE00A010885F0BCBCE2949BB9` / `F8E4DB284D007711C0DFB9C5F4EEC25AB28D3E3AD4236CB97BBE17338F8C0C52` / `2D5101E488A361208F44ED622C8AD792AD1FB812D8C58CED5442E5A0D944792F` |
| C5 `20260824T1341156573437Z-bf3b3c4e09c648f4a8fa216844ed7d52` | all three | same | `5E9469DF3C1FEDD0AEFFC3E679AF5BEEC7596AB2B6ADAB9A1BC0C38B43278995` / `49F08838FC60BB1CAF90A6789E56D790267EF7B6DF76F00088CFA4567D768A0D` / `43D0F424703ADB268A6E421521A20651B9C0A34028266FE97D70BA72BE866F53` / `A4EA29329C57EC47D5E51FACE126A0A2A6602DF7598143A8DFA9DD9584174598` |

The four missing-script messages in all five controls had the same normalized
`<null>` fingerprint. The one lightmap message in all five controls was:
"The loaded level has a different lightmaps mode than the current one.
Current: Directional. Loaded: Non-Directional. Will use: Directional."

Per-family verdicts:

| Warning family | Verdict | Decisive evidence |
|---|---|---|
| Unsupported shaders | External to KMG bundles | 0 in C1-C5; 10 paired occurrences in the retained multi-mod log; KMG inventory 0 unsupported/non-Standard materials |
| Particle mesh/read-write | External to KMG bundles | 0 in C1-C5; 39 in retained multi-mod log; KMG bundles contain no particle system or particle renderer |
| Missing serialized script | External | Exact count 4 survives C1 through C5; KMG inventory missing-component count 0 |
| Lightmap mismatch | External | Exact count 1 survives C1 through C5; KMG inventory has 0 lightmapped renderers, Cameras, or Lights |
| Zero-area particle mesh | Not reproduced | 0 in the retained log and all C controls; KMG has no particle mesh and 0 zero-area readable meshes |
| Missing material property | External to KMG bundles | 0 in C1-C5 and KMG inventory; 5 in retained multi-mod log |

## Attribution tooling retained

Commit `f110cc099a938767df637ab07767f900ab4320ee`
(`test(compat): add bounded KMG attribution diagnostics`) added:

- an exact five-value, guarded, request-local asset-family suppression plan;
- early request parsing after KMG identity evidence and before any KMG bundle
  load;
- a save-free asset inventory scenario;
- a post-exit raw-log collector with normalized, bounded fingerprints;
- exact installed-mod, module-state, DLL, bundle, and log hashes;
- compatibility-profile integration that collects before restoring and verifies
  exact `FeatureModules.json` bytes;
- source-state attestation for reusable local artifacts.

The tooling is test-request-only. It installs no broad Harmony observation,
does not suppress exceptions, does not patch any third-party implementation,
does not log per frame, and writes no player/save-owned state.

Files in that tooling commit:

- `scripts/Build-Local.ps1`
- `scripts/Deploy-Local.ps1`
- `scripts/Invoke-KingmakerRuntimeTest.ps1`
- `scripts/RuntimeAutomation.Common.ps1`
- `scripts/RuntimeHarness.Common.ps1`
- `scripts/Test-RuntimeScenarioPreflight.ps1`
- `scripts/compatibility/Collect-KmgCompatibilityAttributionLog.ps1`
- `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1`
- `src/KingmakerGunslinger/Assets/EasternWeaponAssetRuntime.cs`
- `src/KingmakerGunslinger/Assets/ElvenBranchedSpearAssetRuntime.cs`
- `src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs`
- `src/KingmakerGunslinger/Compatibility/CompatibilityAssetAttributionPlan.cs`
- `src/KingmakerGunslinger/KingmakerGunslinger.csproj`
- `src/KingmakerGunslinger/Main.cs`
- `src/KingmakerGunslinger/RuntimeTesting/CompatibilityAssetAttributionScenario.cs`
- `src/KingmakerGunslinger/RuntimeTesting/CompatibilityAttributionRuntimeControl.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs`
- `tests/KingmakerGunslinger.DomainTests/CompatibilityAttributionTests.cs`
- `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj`
- `tests/KingmakerGunslinger.DomainTests/Program.cs`

No compatibility fix commit exists because no KMG defect was proven.

## Tests, build, package, and deployment

Four focused tests were added:

- `compat-attribution.asset-plans`;
- `compat-attribution.asset-plan-fail-closed`;
- `compat-attribution.guarded-runtime-boundary`;
- `compat-attribution.bounded-inventory-and-logs`.

Final qualification results:

| Command / gate | Result |
|---|---|
| `.\scripts\validate-repository.ps1` | PASS |
| `.\scripts\test-domain.ps1 -Configuration Release -Clean` | PASS, 1,228/1,228 |
| `.\scripts\Test-RuntimeScenarioPreflight.ps1` | PASS, 141/141 |
| `.\scripts\Build-Local.ps1` | PASS; clean exact-reference Release build, build-output validation, focused supply-icon validation, production firearm manifest/SoundBank validation, deterministic package, strict standalone UMM validation |
| `.\scripts\validate-package.ps1` on the standalone package | PASS |
| `git diff --check` | PASS |

Two attested diagnostic binaries were used:

1. C1-C5, A2/A4, and B2/B3/B6 used the pre-commit source-state candidate:
   package `5DAB7784946A95FB8B1A40762C9F1125442DDA5259CE7377B6EFBFEDEF31BA7A`,
   DLL `09B97C0D39E8BA844EA4B206398BBA932F96541C818EBDF101D9C1D029B5D765`,
   MVID `59c24677-5bc1-4921-b4ef-668e40a1f103`, source-state
   `38ED4D1B9DC1790BB24F78BABB15C2163E02B4407B4201B24F5A85A446282074`.
   Its complete source content became `f110cc09` without a subsequent source
   edit.
2. Strict A3 used the clean committed candidate:
   package/DLL/MVID
   `BD0164A9A9A945994891570B066D9A3723984DFE1BA73B94CAA75CF339041CB6` /
   `429E6375B8EEE66DA2EFC9A6AED009C2CA40046E65F4AD10A9D6F182D8C2A2CD` /
   `5a1cb634-7afa-48e9-a2dd-418ebe1844c1`, source-state
   `9DC5814494B64DDAE724146B648E321FDB1855F74F000DA35DCE835B24E23133`.

The final deterministic standalone and local-runtime packages are byte
identical at SHA-256
`BD0164A9A9A945994891570B066D9A3723984DFE1BA73B94CAA75CF339041CB6`.
The build-local manifest SHA-256 is
`374E1CA517B2284A3049D98C1DC1E03998D80E04126C4C3D0102AD97E29301F3`.

Final transactional deployment:

- manifest:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260824T1442137033428Z\deployment.json`;
- manifest SHA-256:
  `64D3568F54D054784AFF4E9264E23AD55A16915C90BD2C8CF16546A6E39A3811`;
- installed DLL SHA-256:
  `429E6375B8EEE66DA2EFC9A6AED009C2CA40046E65F4AD10A9D6F182D8C2A2CD`;
- firearm SoundBank manifest:
  `BF57981AD5EC2CBF3149ECAFC3EF737D87BC9035B14BCCC7D254DCA8F991C62E`;
- firearm SoundBank:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`;
- installed bundle hashes: unchanged exact values recorded above;
- `FeatureModules.json` after deployment and all profiles:
  `28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59`.

The changed DLL is an unreleased diagnostic branch artifact, not a public
production release. Normal gameplay behavior is unchanged because the new
capability requires an exact guarded test request.

## Remaining uncertainty and human gate

- The originally reported exception-bearing log and its exact affected
  save/unit/fact identities were not retained. That prevents reproduction of
  the historical `ComponentAppliedOnceOnLevelUp` and polymorph/view
  fingerprints and is why those tracks are classified Not reproduced rather
  than External.
- A KMG-absent process cannot execute KMG's guarded evidence runner. This audit
  does not mislabel A1 or B1 as performed. The relevant behavior-negative A2
  and B2 controls are the cheapest mechanically valid guarded controls.
- The C inventory instantiated every bundle prefab in a save-free startup
  phase. It did not repeat every held/stored in-world visual acceptance case
  because no bundle repair occurred and the decisive family controls were
  clean.
- No further human gate is required for the "no KMG defect found" result. A
  future report containing the exact missing source log and disposable trigger
  could support a new reproduction attempt; it is not a prerequisite for this
  branch handoff.

## Scope confirmation

This mission did not modify any third-party mod, third-party settings
permanently, Pathfinder: Kingmaker binary, Unity/Wwise binary, save,
KMG_AUTOMATION_BASELINE, Kingmaker Dice Roller repository, unrelated
repository, `master`, tag, pull request, or public release. It did not merge,
rebase, force-push, reset, clean, or rewrite history. It did not change a
blueprint GUID, asset bundle, material, shader, mesh, prefab, sound asset, game
mechanic, or public version.
