# Elven Branched Spear implementation evidence

Status: investigation in progress on `codex/elven-branched-spear`.

This report records the evidence used to implement the Elven Branched Spear. It
is intentionally updated as each read-only inventory and qualification stage
completes. A name, remembered GUID, or successful build is not treated as
runtime proof.

## Repository safety gate

- Repository root: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`.
  The directory name is shortened locally, but `origin` is
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`, so
  this is the project named by the mission.
- Selected base: `6357b8cb27b92f6974ff61409c7aaffb7f2c3cdc` (`Merge Expanded
  Summoning`). Its second parent is the completed Expanded Summoning branch tip
  `b0d9ed28f354f3fa7c58f419fe9c13e68c1d3f3f`.
- At the gate, local `master` and `origin/master` were identical, divergence was
  `0 0`, and `git status --short` was empty after `git fetch --all --prune`.
- Feature branch: `codex/elven-branched-spear`. No branch with that name existed
  before the gate.
- Baseline on 2026-08-13: version-aware repository validation passed and all
  1,018 dependency-free domain/reflection tests passed in a clean Release test
  build.

## Engine assembly evidence

The installed Kingmaker `Assembly-CSharp` contract is preserved in
`artifacts/inspection/assembly-classlist.txt`. The following findings come from
that local IL inventory and will be corroborated by the guarded read-only
runtime inventory before identities are selected.

### Weapon category and type

- Native `WeaponCategory` values occupy `0` through `0x4a` in the supported
  assembly. The final native value is `ThrowingAxe`. The spear therefore needs
  a project-owned category value; reusing a native spear, fauchard, or elven
  curve blade category would violate the one-category requirement and corrupt
  ordinary feat semantics.
- `WeaponSubCategory` provides the native classification surfaces needed here:
  `Melee`, `Finessable`, `TwoHanded`, `Exotic`, and `Metal`. It also distinguishes
  `Light`, `Martial`, `Simple`, `Thrown`, and the one-handed Grace families. The
  custom category must return true only for its actual classifications; in
  particular it must remain false for `Light`, `Martial`,
  `OneHandedPiercing`, and `OneHandedSlashing`.
- `BlueprintWeaponType` stores the category, attack type and range, base damage,
  damage form, critical edge and multiplier, fighter group, weight, handedness,
  animation/visual parameters, localized presentation, icon, and inherent
  enchantments. The repository already has a narrow private-field adapter in
  `Blueprints/WeaponTypeMechanicalAccess.cs`; any extension will stay limited to
  the fields needed by this weapon rather than widening firearm state.
- `BlueprintParametrizedFeature` extracts weapon-category parameters by walking
  `EnumUtils.GetValues<WeaponCategory>()`, applying its configured
  `WeaponSubCategory`, and obtaining the display name from native stat strings.
  A non-native enum value is consequently not auto-discovered. Integration must
  append one ordinary `FeatureUIData` parameter to compatible existing selectors
  and leave incompatible selectors unchanged. The selection cache also requires
  an idempotent invalidation/publication boundary.

### Proficiency and racial familiarity

- `UnitProficiency` stores exact `WeaponCategory` values. Blanket native Martial
  Weapon Proficiency is an explicit category grant, so a new exotic category is
  not accidentally granted to a martial-proficient non-elf.
- Exotic Weapon Proficiency is parameterized and can grant the exact custom
  category after the standard selector exposes it. Downstream parameterized
  prerequisites compare the selected `FeatureParam`, so they should retain their
  native prerequisite behavior.
- The native elven familiarity feature and its actual race grants still require
  guarded blueprint inventory. The implementation will append the exact custom
  category to that feature if the runtime contract confirms the expected native
  `AddProficiencies` mechanism. It will not introduce a `race == Elf` bypass.

### Dexterity attack and damage surfaces

- Native Weapon Finesse (`90e54424d682d104ab36436bd527af09`) uses an
  `AttackStatReplacement` constrained by `WeaponSubCategory.Finessable`.
  Classifying only the spear category as finessable allows Dexterity to attack
  while leaving the weapon two-handed. Weapon Finesse does not replace the
  damage stat.
- In the installed supported profile, native Rogue Finesse Training is a static
  child-feature selection. Its children use `WeaponTypeDamageStatReplacement`
  for an exact category. The Elven Curve Blade child confirms
  `OnlyOneHanded = false` and `TwoHandedBonus = true`; this is the native
  two-handed comparison contract. Call of the Wild also contains generic
  `DamageGrace`-family code, so the compatibility audit must keep those two
  implementations distinct.
- Native `WeaponDamageStatReplacement` is owner-enchantment scoped and supports
  a `RequiresFinesse` guard. This is the engine surface used by Agile candidates;
  its exact blueprint configuration remains part of the guarded inventory.
- Native `EquipmentWeaponTypeDamageStatReplacement` and
  `WeaponTypeDamageStatReplacement` are generic category-aware replacement
  surfaces and require a full blueprint-use audit.
- The replacement surfaces call `OverrideDamageBonusStat`; competing legitimate
  replacements therefore choose one statistic rather than adding Dexterity
  twice. This remains subject to realistic integrated tests with Agile and
  Finesse Training together.
- Call of the Wild's local inspected source contains category-aware weapon
  training and grace implementations, including explicit suppression when Agile
  or native `DamageGrace` already owns the replacement. Every concrete optional
  source still needs classification by its own light/one-handed/free-hand/named
  restrictions before it can be declared compatible.

### Attack-of-opportunity provenance

- `RuleAttackWithWeapon.IsAttackOfOpportunity` distinguishes an AoO from an
  ordinary attack but does not encode the provocation reason.
- Nonmovement provocations enter through
  `UnitCombatEngagementController.ProvokeAttackOfOpportunity` or the separate
  forced-AoO queue and ultimately call `UnitCombatState.AttackOfOpportunity`.
- Movement disengagement has a narrower boundary:
  `UnitCombatEngagementController.TickUnit` observes the engagement transition
  and calls `UnitCombatState.Disengage(target)`. `Disengage` checks
  `ShouldAttackOnDisengage` and then creates the AoO through
  `AttackOfOpportunity`.
- `AttackOfOpportunity` queues a `UnitAttackOfOpportunity`; its `OnAction`
  synchronously creates the `RuleAttackWithWeapon`, sets
  `IsAttackOfOpportunity = true`, and triggers the attack.
- The safe correlation strategy is therefore request-local and identity-based:
  establish an exception-safe scope at `Disengage`, mark only the exact
  `UnitAttackOfOpportunity` constructed within that scope, and expose that mark
  only while the marked command executes. The attack modifier must additionally
  require `IsAttackOfOpportunity` and the exact equipped spear. Spellcasting,
  ranged-attack, standing, forced, ordinary, charge, and out-of-turn attacks do
  not traverse that marked construction boundary. This avoids inferring cause
  from turn ownership, recent movement, animation, or distance.

## Repository conventions

### Foundation runtime qualification checkpoint (2026-08-13)

- Guarded Steam `mod-load-smoke` run
  `20260813T2234240789658Z-e241b2c36ab34ae1afe5882ddc1615a9`
  passed from evidence directory
  `runtime-evidence/20260813T2234240633094Z-mod-load-smoke`.
- The loaded DLL SHA-256 was
  `3c5879f6759b86e93672587c936ef99e1b97323f276db7b718285527c7bae3d1`;
  the qualified local-runtime package SHA-256 was
  `d1c35c8b8279848774af4677f98939c5bd16c352ed61714c35ee258bf370b4fd`.
- The first guarded attempt failed closed during owned validation because
  native magic weapons report `IsMasterwork = false`; the registry rolled back
  all eleven registrations. Comparison with native +1 weapons showed that the
  enhancement enchantment supersedes the separate nonmagical masterwork
  enchantment. The corrected validation follows that native representation,
  and the subsequent guarded run passed. This is registration evidence, not
  yet the required combat, selector, Dexterity, or save/load qualification.

### Feature modules and registration

- `FeatureModuleConfiguration`, `FeatureModuleSettingsStore`,
  `FeatureModulePublicationPlan`, and `FeatureModuleUi` are the current module
  boundary. Existing semantic IDs use lower-case hyphens, so the project key is
  `elven-branched-spears`, label `Elven Branched Spears`, default `true`.
- Settings are immutable for a running process. All stable spear blueprints must
  register regardless of the setting; the setting gates only new selector,
  vendor, loot, and presentation publication. This preserves owned items and
  selected features when a later run starts with the module disabled.
- `blueprints/blueprints.json` is the authoritative deterministic identity
  ledger. New 32-character lower-case IDs must be checked against the complete
  registered and reserved set under all module profiles.

### Campaign publication

- Vendor publication uses append-only, rollback-capable, reference-idempotent
  merging through `Acquisition/VendorCatalogPublication.cs` and the existing
  vendor blueprint adapters.
- Proven existing campaign identities include capital Smith table
  `7de959347266092448d8a72089ef9778`; its documented owners are
  `CapitalOwlbearAttack_Blacksmith` (`ba7a...`) and `VerdelBlacksmith`
  (`478862...`). Existing documentation also records chapter-generic tables
  `03139...`, `b3bc...`, and `fc01...`, but their ownership/timing remains
  unqualified and will not be guessed.
- Proven fixed-loot patterns exist in `RareFirearmCampaignLootBlueprints.cs`,
  including exact container/table identities for Vordakai's Tomb, Pitax, and the
  House at the Edge of Time. Early campaign placement identities still require
  a read-only runtime graph inventory before publication.

### Asset pipeline

- `scripts/Prepare-UnityAssets.ps1`, `tools/unity/BuildFirearmBundles.cs`,
  `Assets/FirearmAssetRuntime.cs`, and `Assets/FirearmPresentationProfile.cs`
  are firearm-specific and will not receive a spear `FirearmKind`.
- The exact authorized Unity project exists at
  `C:/Dev/KingmakerGunslingerLab/unity-asset-build/KingmakerGunslinger-2018.4.10f1`.
- No optional donor directory exists at
  `C:/Dev/KingmakerGunslingerLab/asset-sources/elven-branched-spear`; therefore
  no third-party model or license is currently eligible for use.
- No `blender` executable is currently on `PATH`. Standard installation paths
  still need inspection. Mechanics will use a native reach-polearm equipment
  entity as a mandatory fallback and will not depend on the custom bundle.

## Runtime inventory still required before identity selection

The guarded save-free observer was designed to record, without mutating a save:

- longspear, fauchard, glaive, bardiche, elven curve blade, and suitable native
  spear item/type identities, mechanical fields, equipment entities, icons,
  animation styles, grip data, and pricing construction;
- masterwork, cold iron, Agile, Keen, Corrosive, Speed, enhancement, dodge,
  entangled, movement-speed, and saving-throw donor contracts;
- native elven familiarity and all actual race/archetype grants;
- every native parameterized weapon-category selector and the component and
  prerequisite consumers attached to it;
- every blueprint using a compatible attack- or damage-stat replacement and all
  deliberate one-handed/light/free-hand/named exclusions;
- proven Act I through final-act vendor ownership and fixed-loot targets.

No production identity or campaign target was selected before this observer
evidence was captured and reviewed. Production identities below are the result
of that gate.

## Guarded contract inventory results

The save-free `observe-elven-branched-spear-contracts` scenario ran through the
Steam App ID 640820 harness and produced PASS at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260813T2206508050890Z-observe-elven-branched-spear-contracts`.
It inspected 105,821 registered blueprints without selecting or writing a save.
The loaded assembly SHA-256 was
`5C8BD04262866E371FC9F0421328CCAADDF04B1205A494947A7AA0AFE76E8C4D`.

### Chosen mechanical and presentation donors

- Standard Longspear item `f28f6031c2908d84d945865a80f67177` and type
  `fa2dd17cbde7d3f4aa918d467c30516e` provide the exact 1d8, piercing,
  20/x3, two-handed, six-foot engine reach, `Spears` fighter group, and
  `PiercingTwoHanded` animation contract. Its native fallback model is
  `TH_LongspearKnight1`, icon is `LongspearKnight1_RB_3`, and it uses the native
  two-handed piercing equip, remove, whoosh, inventory, and impact sounds.
- Standard Glaive `f83415c0e7ea1994d8a7f3dec8f5a861` confirms the locked
  10-pound weight and reach polearm item presentation but has slashing damage,
  an axe animation, and the `Polearms` group. It is evidence for weight only,
  not the primary mechanical donor.
- Fauchard type `7a40899c4defec94bb9c291bde74f1a8` carries an inherent
  Trip enchantment, an 18-20 threat range, and an axe animation. Bardiche type
  `b1cbf457fd471d148b39ae56667f405a` is 1d10, 19-20/x2, 14 pounds,
  and axe-animated. Neither is a safe clone donor because those inherited
  mechanics conflict with the locked profile.
- Elven Curve Blade type `b5e6838ad2a62b146b49619bcf9f42aa` proves a native
  two-handed finesse-compatible family and uses `SlashingTwoHanded`, but it is
  non-reach, 1d10, 18-20/x2, and `BladesHeavy`. It is a comparison fixture, not
  the spear category or reach donor.

### Native construction contracts

- Masterwork enchantment `6b38844e2bffbac48b63036b66e735be` is present on
  native nonmagical masterwork donor items. `BlueprintItemWeapon.IsMasterwork`
  derives from this enchantment component rather than the item name. Native
  magical weapons instead carry only their enhancement enchantment and report
  `IsMasterwork = false`; their magic enhancement supersedes the nonmagical
  masterwork attack bonus. The spear follows that native construction while
  retaining the tabletop masterwork surcharge in its declared price.
- Native cold-iron weapons set `m_OverrideDamageType = true` and store an
  item-level `DamageTypeDescription`; direct observation reports, for example,
  `Physical:form=Piercing:material=ColdIron` on native cold-iron piercing
  weapons. They do not use a native cold-iron enchantment. The similarly named
  `e5990dc76d2a613409916071c898eee8` enchantment observed in the combined
  profile belongs to Call of the Wild and must not be added on top of the native
  override.
- Native generic weapon enchantments are: Agile
  `a36ad92c51789b44fa8a1c5c116a1328`, Keen
  `102a9c8c9b7a75e4fb5844e79deaf4c0`, Corrosive
  `633b38ff1d11de64a91d490c683ab1c8`, Speed
  `f1c0c50108025d546b2554674ea1c006`, and enhancement +1 through +5
  `d42fc23b92c640846ac137dc26e000d4`,
  `eb2faccc4c9487d43b3575d7e77ff3f5`,
  `80bb8a737579e35498177e1e3c75899b`,
  `783d7d496da6ac44f9511011fc5f1979`, and
  `bdba267e951851449af552aa9f9e3992`.
- Agile has one native `WeaponDamageStatReplacement` with enchantment cost 1;
  its IL requires the exact owning weapon, the native Weapon Finesse mechanics
  flag, and a strictly better Dexterity bonus before it calls
  `OverrideDamageBonusStat`. Speed has one native `WeaponExtraAttack` and cost
  3. These blueprints will be referenced, not reimplemented.

### Proficiency, race, and selector shapes

- Elven Weapon Familiarity is `03fd1e043fc678a4baf73fe67c3780ce`.
  Its exact `AddProficiencies` component currently grants Longbow, Longsword,
  Rapier, Shortbow, and Dueling Sword. Native Elf race
  `25a5878d125338244896ebd3238226c8` grants that feature directly.
- Native Half-Elf race `b3646842ffbd01643ab4dac7479b20b0` grants Keen Senses,
  Elven Immunities, and Adaptability, but not Elven Weapon Familiarity. The spear
  will therefore follow actual feature ownership: Elf qualifies; Half-Elf does
  not unless another source grants the familiarity feature.
- Exotic Weapon Proficiency is the static selection
  `9a01b6815d6c3684cb25f30b8bf20932`. Its native children each contain exact
  `AddProficiencies`, `PrerequisiteNotProficient`, and starting-equipment
  category contracts. The integration must publish one ordinary child feature
  for the new category into this existing selection; it cannot be implemented
  through the parameterized Weapon Focus patch alone.
- Rogue Finesse Training is the static selection
  `b78d146cea711a84598f0acef69462ea`. The native Elven Curve Blade child
  `04f3b956e5a5cf649bce83774e0bfe4a` proves that a two-handed finesse weapon is
  legitimate. Each child uses `WeaponTypeDamageStatReplacement` with Dexterity,
  `OnlyOneHanded = false`, and `TwoHandedBonus = true`; the spear child must use
  that same native component and selector.
- The installed parameterized selector inventory confirms ordinary integration
  for Weapon Focus `1e1f627d26ad36f43bbd26cc2bf8ac7e`, Greater Weapon Focus
  `09c9e82965fb4334b984a1e9df3bd088`, Improved Critical
  `f4201c85a991369408740c6888362e20`, Weapon Specialization
  `31470b17e8446ae4ea0dacd6c5817d86`, Greater Weapon Specialization
  `7cf5edc65e785a24f9cf93af987d66b3`, Sword Saint Chosen Weapon
  `c0b4ec0175e3ff940a45fc21f318a39a`, and Weapon Mastery
  `38ae5ac04463a8947b7c06a6c72dd6bb`.
- Fencing Grace and Slashing Grace are separately constrained to
  `OneHandedPiercing` and `OneHandedSlashing`; they remain deliberate
  exclusions. Point-Blank Master is ranged-only. Those menus will not receive
  the spear.

### Campaign identity evidence

- Act I fixed-loot candidates are positively tied to area
  `StagLordFort` (`083fa331576e43047b3159d32c3474e5`), including the existing
  +1-glaive chest `59cb0ac65b4093440ad341b9a2f372cf`. Old Sycamore's first
  dungeon level is `fb4cffc2c4279e141a1c877785bce7ac`, with exact good-loot
  container `04dfba3e7e2465c4b8b1bdd6ea9a8697`.
- Pitax Town area `87eb9e17e4d796741960c48b0226e124` and Irovetti Palace area
  `bf9dbc2998849ee40bbdba9cb40a7d4c` have exact later-campaign loot records.
  The established fixed-loot publication adapter already uses verified Irovetti
  targets `b34367a637010f743815aed5875152bd` and
  `485300a2036a763499aa77ebac1f83c6`.
- House at the Edge of Time is `13e7006bce054ce4e82b5064b2f3f8ff`;
  Final Dungeon levels are `a39dc445480d9b9419aaa3f8df2d1ed1`,
  `f85f5e240e68b14438992554f5234d57`, and
  `0bc7e9d236228564ba1f7f3da61d8e91`. These are late/final placement
  candidates only; the placement manifest will name a target after exact
  append behavior and reachability are tested.
