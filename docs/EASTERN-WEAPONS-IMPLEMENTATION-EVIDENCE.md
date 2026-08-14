# Eastern Weapons implementation evidence

## Investigation authority

- Verified base commit: `4ffd15b09992bd9cee9d330eee0a650ad2c94661`
- Branch: `codex/eastern-weapons`
- Baseline version: `0.0.79`
- Correct landed spear proficiency contract: **Weapon Proficiency (Elven Branched Spear)**
- Eastern proficiency children will use the same **Weapon Proficiency
  (Family)** structure.

The save-free guarded runtime scenario is:

```text
observe-eastern-weapon-contracts
```

It inventories installed weapon donors, native enchantments, proficiency and
grip rules, active equipment and combat-event boundaries, damage-size and
polymorph surfaces, coup-de-grace construction, selectors, fighter groups,
vendor tables, campaign loot, and direct blueprint owners. It performs no
selector, vendor, loot, blueprint, unit, inventory, input, or save mutation.

No production category value, blueprint identity, donor, enchantment identity,
bespoke-effect hook, model donor, price, vendor, or fixed-loot target will be
selected until this exact observer has run successfully against the packaged
baseline-compatible candidate and its structured evidence has been reviewed.

## Baseline qualification

| Surface | Result |
| --- | --- |
| Repository validation | PASS |
| Complete domain/reflection suite | PASS, 1,033/1,033 |
| Clean exact-reference Release build | PASS |
| Build-output validation | PASS |

## Runtime observation

Pending the first coherent observer build, package validation, commit, guarded
publication, and fresh Steam App ID 640820 launch. Exact run ID, loaded identity,
blueprint identities, native component contracts, and campaign reachability
evidence will be recorded here after the structured result is reviewed.

The observer source checkpoint passed repository validation, the complete
1,034-test domain/reflection suite, clean exact-reference Release build,
build-output validation, deterministic package creation, and strict standalone
package validation. Live observation remains pending until the coherent source
checkpoint is committed and published.

The first guarded attempt, run
`20260814T1104529826303Z-b2fcb4605fd84bbabd97ad2bf6af9aa2`, timed out before a
result after the active compatibility stack delayed the first runner update.
It performed no save interaction. That ambiguous attempt is not qualification
evidence. The observer was narrowed and instrumented for a fresh retry.

## Successful installed-contract observation

- Run ID: `20260814T1110588439047Z-7f131097a8ca48ac916f675e77b57c47`
- Source commit: `41b7687079f380a044ffed3a0bf0d3dac771228e`
- Status: PASS
- Loaded version: `0.0.79`
- DLL SHA-256:
  `89C06B4F29190A919512FB0FF5D275C054FC04D940749ECB0828EE6C6499FF4E`
- DLL MVID: `52f2dfea-aaf1-4cda-9c6e-c3f06270c3a7`
- Save interaction: none
- Automatic exit: requested and completed
- Installed blueprint count: 105,859
- Observer phase timings: library 13 ms; weapons/enchantments 425 ms;
  selectors/rules 23,158 ms; campaign inventory 43,509 ms

The exact native donor weapon-type identities found by the live graph are:

| Donor type | GUID | Damage | Critical | Group | Weight | Hand contract |
| --- | --- | --- | --- | --- | ---: | --- |
| Kukri | `006ecd4715809b343b7001e859e3ddb2` | 1d4 | 18-20/x2 | Light Blades | 2 | light |
| Shortsword | `a7da36e0e7bb60e42b9f23462ce2f4fc` | 1d6 | 19-20/x2 | Light Blades | 2 | light |
| Rapier | `2ece38f30500f454b8569136221e55b0` | 1d6 | 18-20/x2 | Light Blades | 2 | one-handed |
| Scimitar | `d9fbec4637d71bd4ebc977628de3daf3` | 1d6 | 18-20/x2 | Heavy Blades | 4 | one-handed |
| Longsword | `d56c44bc9eb10204c8b386a02c7eed21` | 1d8 | 19-20/x2 | Heavy Blades | 4 | one-handed |
| Bastard Sword | `d2fe2c5516b56f04da1d5ea51ae3ddfe` | 1d10 | 19-20/x2 | Heavy Blades | 6 | native special/two-hand-capable |
| Falchion | `6ddc9acbbb6e40746a6a1671df1f7b47` | 2d4 | 18-20/x2 | Heavy Blades | 8 | two-handed |
| Greatsword | `5f824fbb0766a3543bbd6ae50248688f` | 2d6 | 19-20/x2 | Heavy Blades | 8 | two-handed |

All observed melee donor ranges were 2 feet; no donor selection changes the
locked non-reach Eastern profiles.

The first-pass exact native enchantment identities are:

| Contract | GUID | Effective cost | Exact native component |
| --- | --- | ---: | --- |
| Masterwork | `6b38844e2bffbac48b63036b66e735be` | 0 | `WeaponMasterwork` |
| Enhancement +1 | `d42fc23b92c640846ac137dc26e000d4` | 1 | `WeaponEnhancementBonus(1, Stack=false)` |
| Enhancement +2 | `eb2faccc4c9487d43b3575d7e77ff3f5` | 2 | `WeaponEnhancementBonus(2, Stack=false)` |
| Enhancement +3 | `80bb8a737579e35498177e1e3c75899b` | 3 | `WeaponEnhancementBonus(3, Stack=false)` |
| Enhancement +4 | `783d7d496da6ac44f9511011fc5f1979` | 4 | `WeaponEnhancementBonus(4, Stack=false)` |
| Enhancement +5 | `bdba267e951851449af552aa9f9e3992` | 5 | `WeaponEnhancementBonus(5, Stack=false)` |
| Flaming | `30f90becaaac51f41bf56641966c4121` | 1 | `WeaponEnergyDamageDice(1d6 fire)` |
| Frost | `421e54078b7719d40915ce0672511d0b` | 1 | `WeaponEnergyDamageDice(1d6 cold)` |
| Agile | `a36ad92c51789b44fa8a1c5c116a1328` | 1 | `WeaponDamageStatReplacement(Dexterity, RequiresFinesse=true)` |
| Keen | `102a9c8c9b7a75e4fb5844e79deaf4c0` | 1 | `WeaponCriticalEdgeIncrease` |
| Ghost Touch | `47857e1a5a3ec1a46adf6491b1423b4f` | 1 | `WeaponReality(Ghost)` |
| Shock | `7bda5277d36ad114f9f9fd21d0dab658` | 1 | `WeaponEnergyDamageDice(1d6 electricity)` |
| Thundering | `690e762f7704e1f4aa1ac69ef0ce6a96` | 1 | `WeaponEnergyDamageDice(1d6 sonic)` |
| Holy | `28a9964d81fedae44bae3ca45710c140` | 2 | `WeaponDamageAgainstAlignment(Evil, Holy)` |
| Brilliant Energy | `66e9e299c9002ea4bb65b6f300e43770` | 4 | `BrilliantEnergy`; misses Undead and Construct facts |
| Speed | `f1c0c50108025d546b2554674ea1c006` | 3 | `WeaponExtraAttack(Number=1, Haste=true)` |

## Targeted mechanic follow-up

- Run ID: `20260814T1119161920060Z-d07fac81ae644db0ac092e1fa3cfa3fe`
- Source commit: `34f3093118ef028242f39e3f63e497a9c16a7580`
- Status: PASS
- Loaded version: `0.0.79`
- DLL SHA-256:
  `57B42B4F18FC05614AC7078564CB2D0A83536480A1C97CF3BBA1DA771FD32A7E`
- DLL MVID: `5337c8ba-2d31-4c60-a39f-34017ce40339`
- Save interaction: none
- Automatic exit: requested and completed

The targeted pass examined all 136 loaded weapon types, 600 mechanic
blueprints, and 191 relevant loaded CLR types. Existing categories occupy the
native range `0..74` plus the accepted Elven Branched Spear value
`0x004B4D47`. The next three deterministic values are unoccupied across every
loaded weapon type:

| Family | Stable category value |
| --- | ---: |
| Wakizashi | `0x004B4D48` (`4934984`) |
| Katana | `0x004B4D49` (`4934985`) |
| Nodachi | `0x004B4D4A` (`4934986`) |

Production registration must still fail closed if any subsequently loaded
weapon type owns one of these values.

The authoritative installed hand contract is
`ItemEntityWeapon.HoldInTwoHands`. `BlueprintWeaponType` exposes
`IsOneHandedWhichCanBeUsedWithTwoHands`, while an equipped `HandSlot` exposes
its paired slot, shield state, and current weapon. Katana proficiency and
Moonlit Crossing will therefore consume the same live `HoldInTwoHands` result,
not animation, transforms, or timing.

Call of the Wild augments the native Bastard Sword proficiency child with
`CanHoldIn1Hand`, `FullProficiency`, and
`PrerequisiteFullExoticProficiency`. Those optional identities will not be
copied or mutated. The KMG katana policy remains independent and uses the live
grip result while preserving optional-mod behavior.

Native weapon-size changes are rule-owned:
`MeleeWeaponSizeChange` handles `RuleCalculateWeaponStats`, whose public
contract supplies `IncreaseWeaponSize`, `DecreaseWeaponSize`, `WeaponSize`,
and `WeaponDamageDiceOverride`. Unfixed Form can consequently add one native
size step at the weapon-stat boundary without changing unit size, reach,
model, or animation.

`PowerAttackFeature:9972f33f977fc724c838e59641b2fca5` contains exactly one
`AddFacts` grant of
`PowerAttackToggleAbility:a7b339e4f6ff93a4697df5d7a87ff619` and exactly one
`PowerAttackWatcher` referencing that same toggle. The production resolver
validates both links before Mountain-Sunder consumes the toggle's live
`ActivatableAbility.IsRunning` state. `RuleAttackRoll` exposes the originating
weapon attack, hit result, critical-roll state, and confirmed-critical state.
These are the exact boundaries required for Mountain-Sunder and Falling Petal.

No native enchantment or mechanic blueprint implementing Mighty Cleaving was
present under either the property name or an alternate cleaving-enchantment
component contract. The ordinary native Cleave and Cleaving Finish features
are unrelated character facts and will not be substituted. No native Impact
or Lead Blades enchantment was present; Unfixed Form will use the native
weapon-size rule surface directly.

The installed coup-de-grace implementation is an ability/action graph rather
than a dedicated coup-de-grace rule. Its DC is assembled through contextual
rank properties and `ContextSetAbilityParams(Add10ToDC=true)`; no weapon
enchantment, weapon-stat, damage, saving-throw, or dedicated coup-de-grace
event provides an exact virtual-damage-only DC interception point. Deadly is
therefore omitted for this release, with the required disposition:

```text
DEFERRED  ENGINE HAS NO RELIABLE COUP-DE-GRACE DC HOOK

## Arms and Armor grip compatibility

The exact authorized Arms and Armor 1.0.10 source and live profile contain no
Katana, Wakizashi, or Nodachi provider. They add Temple Sword and Orc Hornbow,
so no same-name proficiency bridge is applicable.

Arms and Armor does replace versatile-weapon grip authority. Its exact loaded
contract consists of
`ArmsArmor.Helpers.IsExoticTwoHandedMartialWeapon(BlueprintItemWeapon)` and
`ArmsArmor.ItemEntityWeaponPatch.IsTwoHanded(ItemEntityWeapon, UnitDescriptor)`.
The former hard-codes Bastard Sword, Dwarven Waraxe, and Estoc; the latter feeds
its `HoldInTwoHands` and active hand-slot behavior. The first Eastern live run
therefore observed Katana forced one-handed even with an empty offhand.

KMG now installs a reflection-only, fail-closed postfix on both exact methods
when and only when the single `ArmsArmor` assembly and exact signatures are
present. It classifies only the exact registered KMG Katana type and resolves
that type's grip from the actual active primary/offhand slots. No foreign
blueprint, GUID, category, or weapon is changed, and there is no compile-time
optional-mod dependency.

Passing live repair run
`20260814T1626264154920Z-659ee31c63844b15a53f60366ffd55d6`
observed two-handed Katana with an empty offhand, one-handed Katana after exact
offhand insertion, correct grip-dependent proficiency, mutually exclusive
Moonlit Crossing modes, and complete request-local cleanup.
```

## Selected campaign publication contracts

The exact installed merchant identities are Oleg
`f720440559fc00949900bfa1575196ac`, capital blacksmith
`7de959347266092448d8a72089ef9778`, Dire Narlmarches village trader
`f072a8f6889b5f345b7f4e7c74cb3e4c`, and Pitax town trader
`e5ab1fccf37c55f41a20a80c6ba6a460`. The four optional BTSL weapon tables are
`a6bae621a7bd96b4fb3c1511cd2f9fac`,
`08e090bb2038e3d47be56d8752d5dcaf`,
`45f027c06962df249b8c014a4b4e95e3`, and
`420f1da6c2523f64eba810b9b484f60f`.

The selected main-path fixed containers are the accepted Stag Lord Fort,
Goblin King Fort, Vordakai Tomb level 2, and Final Dungeon targets at GUIDs
`59cb0ac65b4093440ad341b9a2f372cf`,
`70c4615a8d667dc4cb740c22ee7b5eed`,
`193b1222846a0114197e716cb35d3ce8`, and
`7e6448d1d8a7e4f4d9cc340b8f15e732`. Production selection and exact item rows
are recorded in `EASTERN-WEAPONS-PLACEMENT-MANIFEST.md`.

The production transaction was exercised in fresh guarded processes. Enabled
run `20260814T1343180894067Z-9c6e5326e6fa4ee8a6f0761a7cd2af78`
observed 49 base-campaign merchant rows plus 48 generic rows in the four
installed BTSL tables, seven named merchant rows, and 11 named fixed-loot
rows. Disabled run
`20260814T1349013224092Z-26cb873bd080433ebe1bd5f3658f3061`
observed zero Eastern acquisition rows while retaining every persistent
identity. The settings transaction restored the original bytes exactly to
SHA-256 `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
Neither run accessed a save.

## Original model, icon, and runtime fallback contracts

Blender 4.5.10 LTS generated three original metric weapon sources with primary
grip at origin and +Z toward the tip. The final lengths are 0.76 m Wakizashi,
1.05 m Katana, and 1.58 m Nodachi across 39 mesh objects and 3,522 triangles.
Six exact 128x128 transparent RGBA runtime icons include three category icons
and three distinct capstone icons. Full source/output hashes are in
`EASTERN-WEAPONS-ASSET-PROVENANCE.md`.

The exact Unity 2018.4.10f1 builder emitted one dedicated three-prefab bundle,
SHA-256
`39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`.
Two consecutive force-rebuilds were byte-identical. Runtime candidate loading
requires exact prefab cardinality and family names, identity roots, all five
semantic children, finite plausible family bounds, enabled renderers, complete
opaque materials, and no cameras/lights. Rejection preserves the original
Kukri, Bastard Sword, or Falchion donor contract.

Enabled fresh-process run
`20260814T1420325300375Z-43bcb3114abe402b81663d0dfde65c13`
observed `custom:validated:3`, six distinct item icons, created one live
instance of each family, and destroyed all three before completion. Disabled
run `20260814T1423059037866Z-ff46339be9fd435893d8a4dd8c0b7694`
observed `native-fallback:module-disabled`, zero custom instances, and three
native donor icon families while retaining all persistent identities. Settings
restored exactly; neither run accessed a save. Subjective visual acceptance is
explicitly pending human review.

## Focused live combat contracts

The guarded save-free scenario `disposable-eastern-weapons-combat` passed as
run `20260814T1513175535242Z-6d95393a15c44b96a168cb21132fee19` under the
standalone compatibility transaction `compat-20260814T151226Z-02fa9debed07`.
It used live item entities, equipment changes, facts, native attacks, critical
confirmation, weapon-stat rules, Power Attack state, damage packets, and size
state. The exact observed proficiency attack bonuses were Wakizashi
`8/8/12`, Katana two-handed `8->12->12` and one-handed `8->12`, and Nodachi
`8->12`. The runtime localized the static options as `Weapon Proficiency
(Wakizashi)` and `Weapon Proficiency (Katana)`.

Wayfarer's Oath changed Initiative `5->7->5`; Falling Petal changed AC
`15->16->15` on a natively confirmed critical and did not add a second
application; Moonlit Crossing produced mutually exclusive one-handed Dodge and
two-handed damage applications; Mountain-Sunder produced `0->1->1->2` effect
applications across inactive, first-hit, repeated-hit, and reset-marker
controls; Unfixed Form produced exactly one native `Medium->Large` weapon-size
step only after current size differed from original size. All capstones remained
effective +10 or lower with exact Speed reference cardinality.

The same DLL then passed the complete accepted Elven Branched Spear combat
fixture as run `20260814T1515370965718Z-017e199cb77b4af891868cec2d3a840b`.
Both transactions restored their exact pre-run Mods trees, and neither accessed
a save. Expanded live controls required by the final mission remain tracked in
`EASTERN-WEAPONS-QUALIFICATION.md`.

## First-playtest Call of the Wild Focused Weapon contract

The installed optional selection is
`FocusedWeaponAdvancedWeaponTrainingFeatureSelection`
(`786bde5345a548408fade70b60a70482`). Its serialized `Features` array is empty;
native children and KMG children belong in merged `AllFeatures`. Native child
donors are Shortsword `29a6081e7f4d41fdb9e5da830dd32522`, Bastard Sword
`a13bcc2d98e4426cb017d4edfa05818c`, Greatsword
`70ecd8ffc4e64cce99eccaa2b509bf3d`, and Longspear
`266e9d03ef6e4da6aa56b599f9a6aebc`. Each donor owns exactly one prerequisite
for parameterized Weapon Focus `1e1f627d26ad36f43bbd26cc2bf8ac7e` and one
`ContextWeaponDamageDiceReplacementForSpecificCategory` component with the
native five-step `1d6, 1d8, 1d10, 2d6, 2d8` array.

KMG clones those exact optional components only while CotW is present,
retargets exact categories, and registers inert persistent placeholders when it
is absent. The live level-up contract is `ExtractSelectionItems` followed by
each child's native `BlueprintFeature.MeetsPrerequisites` evaluation; the
latter owns the exact Weapon Focus filter. Guarded CotW combat run
`20260814T2226301648118Z-disposable-eastern-weapons-combat` passed no-focus,
four individual matching-focus, and all-four controls. Real feature facts then
delivered CotW's rule handler through `RuleCalculateWeaponStats`, producing the
expected 2d8 high-level replacement once for Elven Branched Spear, Wakizashi,
Katana, and Nodachi. The run was save-free and transaction
`compat-20260814T222532Z-a19799fea40a` restored the Mods tree exactly.
