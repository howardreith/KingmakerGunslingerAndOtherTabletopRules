# Turn Undead implementation record

The record of what was actually observed. `PENDING` means not yet observed and
is not a result. Native evidence entries name their exact candidate.

## Candidate and environment

| Field | Observed value |
| --- | --- |
| Repository / target | `howardreith/KingmakerGunslingerAndOtherTabletopRules`, `master` |
| Branch / worktree / base | `codex/turn-undead`, `C:/Dev/KingmakerGunslingerLab/worktrees/turn-undead`, base `52755b617c79328a972ae5b74bc05784422e332d` |
| Controller | `C:/Dev/agent-review` package `85dece1c...` (fingerprint `6d613def...`), plan `turn-undead` (packet `turn-undead.plan.json`, sha256 `7880dd60...`), enrollment `kmg-codex-turn-undead-20261127192607-f909`, draft PR #230, foreground |
| Developer session | foreground Claude Code session `1fd92a7c-752f-420d-afe7-c3c3cb3641b6`; controller `foreground_session.problems: []` (claude-opus-5-5, effort high, permission mode auto) |
| Game | Pathfinder: Kingmaker installed build `2021-May-25 14:14 a7e887a5...` (2.1.7b); `Assembly-CSharp.dll` sha256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` |
| Owner CotW profile | Call of the Wild `1.14.4c-2.1`, `CallOfTheWild.dll` sha256 `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` (owner install and loader-compat pristine snapshot identical). The packet's reference source is 1.14.5 (`1332fb0d`); contracts below were re-read from the installed 1.14.4c DLL. |
| Native blueprint leads | Community 2.1.4 archive, sha256 `6ddf3397...df60` (discovery only; native census on 2.1.7b pending) |

## Engine contracts read from the installed 2.1.7b assembly

| Seam | Installed behavior | Consequence |
| --- | --- | --- |
| `RuleCalculateAbilityParams` | Without a calculator: caster level = character level, spell level = CL/2, statistic Charisma | Native channels use character level; Turn uses `ContextCalculateAbilityParamsBasedOnClass` (source class level, its statistic) |
| `BuffStatusCondition` | `AddCondition`/`RemoveCondition` count; `OnNewRound` saves unconditionally with `Reason = Fact` | Owned `TurnUndeadFleeingState` mirrors it with an intelligent-only save |
| `UnitState` conditions | Reference counted | Ending Turn's effect never clears another fear source |
| `UnitFearController` | A Frightened unit with conscious, visible enemies in its memory panics and runs ~20 m away; works in real time and turn-based | Native flight is away from visible enemies, not specifically the channeler; an allied undead with no visible enemies does not move |
| `SavingThrowBonusAgainstSpecificSpells` | Matches `evt.Reason.Context.SourceAbility` against its list | Initial (Reason = context) and repeat (Reason = buff fact, context SourceAbility = the Turn ability) saves both match once Turn identities are listed |
| `BuffDescriptorImmunity` / `SpellImmunityToSpellDescriptor` | Descriptor-based | The owned buff and abilities carry no descriptor, the native `FrightenedUndead` exception; undead keep every other immunity |
| `AbilityTargetsAround` | All units within radius with LOS, then conditions; includes the caster | Turn condition: `UndeadType` and not the caster |
| `AddFeatureIfHasFact` | Applies on activation and every level-up; never removes | Grants a source's action when that source is present, including a later source |
| `AbilityCasterHasFacts` | `NeedsAll` only tests the last fact | Two single-fact checkers (feat, source) |
| `AbilityShowIfCasterHasFact` | Variant-menu visibility | Menu children shown only with feat and source |
| `Fact.PreSave/PostLoad` | `[JsonProperty]` fields by component name | Flight state fields are primitives on `$KMG_TurnUndead_FleeingState` |

## Source contract table

Every value was read from the installed blueprints and printed channel text
(2.1.4 archive leads; exact ability/resource/cost re-validated at registration
by `TurnUndeadBlueprints.ValidateNativeSource`, which refuses to register on
drift).

| Source | Qualifying feature | DC level | DC statistic | Resource / cost | Restrictions | Direct / menu child | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Positive cleric | `ChannelEnergyFeature` `a79013ff` | cleric level | Charisma (printed "1/2 the cleric's level + Charisma") | `ChannelEnergyResource` `5e2bba3e`, 1 | none beyond the positive channel choice | `KMG.Feats.TurnUndead.Cleric.Ability` / `.MenuChild` | native harm `279447a6`; `turn-undead.source-catalog`; native first-turn PENDING |
| Ordinary paladin | `ChannelEnergyPaladinFeature` `cb6d55dd` (gained at 4th) | paladin level | Charisma (printed) | `LayOnHandsResource` `9dedf41d`, 2 | Lawful Good caster (`AbilityCasterAlignment` copied from native harm) | `...Paladin.Ability` / `.MenuChild` | native harm `4937473d`; PENDING native |
| Hospitaler | `ChannelEnergyHospitalerFeature` `a9ab1bbc` (gained at 4th) | paladin (hospitaler) level | Charisma (printed "1/2 the hospitaler's level") | `ChannelEnergyHospitalerResource` `b0e0c771`, 1 | Lawful Good caster | `...Hospitaler.Ability` / `.MenuChild` | native harm `cc17243b`; PENDING native |
| Empyreal sorcerer | `ChannelEnergyEmpyrealFeature` `7d49d7f5` (gained at 9th) | sorcerer level | Wisdom (printed; native `ReplaceAbilityDC` targets only the native heal/harm actions, so Turn sets Wisdom itself) | `ChannelEnergyEmpyrealResource` `f9af9354`, 1 | none | `...Empyreal.Ability` / `.MenuChild` | native harm `e1536ee2`; PENDING native |

Damage dice progressions (Hospitaler starting at 4th, Empyreal at 9th) are not
used for the DC. Negative channels, the positive-permission fact
`ChannelPositiveAllowed`, the wizard necromancer turning power and items do not
qualify. No source pool is created, refilled or shared.

## Classification inventory

Policy: `src/KingmakerGunslinger/TurnUndead/TurnUndeadClassificationPolicy.cs`.
Reviewed fixture: `tests/fixtures/turn-undead/native-undead-classification.json`
(139 native undead units from the 2.1.4 archive; 41 mindless).

| Rule | Mind | Identities / evidence |
| --- | --- | --- |
| Unit type Skeleton `b5c30928`, Zombie `0bd298db`, CyclopZombie `e2fc4028`, PhantomArmor `fc9f65bc` | Mindless | Bestiary Int — families; Int 10 in KM data is not evidence |
| Unit type Ghost `4d34f9d7`, Spectre `446e7ea9`, Devourer `285140dc` | Intelligent | intelligent families |
| Zombie-lord feature lists (44) and the Big Narlmarches ghoul list | Intelligent | zombie lords (Int 13 tabletop), Dunsward tomb spellcasting zombies, Davik Nettle, ghoul |
| 24 skeletal-champion units and the graveknight inside the Skeleton type | Intelligent | skeletal champion stat block (Int 9) |
| Dread zombie cyclopes outside the CyclopZombie type (2 units) | Mindless | zombie family |
| Owner-review defaults (6 units): Skeleton Mage, Bloody Bones Beast, Lonely Warrior, four dread zombie cyclops priests | Mindless (family kept) | they cast spells or carry class levels; owner confirmation requested |
| `SkeletonSummoned` (summoned skeleton) | Intelligent, owner review | it uses the skeletal champion stat block (Int 9); confirm whether the summoning spell's mindless skeleton is intended |
| Anything else (named undead, liches, vampires, third-party) | Intelligent | `unclassified-default-intelligent`, logged with the unit type |

Intelligence scores, names and AI brains are never consulted. Ordinary
skeleton/zombie (Int 10, Dumb Monster Brain) and the zombie lord (Int 12, same
type and brain) are the asserted counterexamples.

## New stable identities and art consumers

| Manifest symbol | Type | Role | Icon / reason |
| --- | --- | --- | --- |
| `KMG.Feats.TurnUndead.Feat` | BlueprintFeature | general feat (Feat group only), published to `BasicFeatSelection` when the setting is ON | `turn-undead-feat` |
| `KMG.Feats.TurnUndead.Fleeing` | BlueprintBuff | owned flight effect | native Fear icon kept (condition icon); no third original requested |
| `KMG.Feats.TurnUndead.{Cleric,Paladin,Hospitaler,Empyreal}.Ability` | BlueprintAbility | granted, pinnable source action | `turn-undead-action` |
| `KMG.Feats.TurnUndead.{Cleric,Paladin,Hospitaler,Empyreal}.MenuChild` | BlueprintAbility | never granted; placed under a loaded provider's positive-harm wrapper | `turn-undead-action` |

GUIDs: `blueprints/blueprints.json` tail (ten entries, milestone "Turn Undead feat").

Art: masters byte-preserved (`9805f019...` feat, `6f5d9353...` action) under
`assets-source/original-icons/icon-overhaul-v2/production/sources/`. Canonical
scoped GDI+ exports: feat `b210edd2022830277de7d3777e45d4403aee17ace1bc7a5656869a48a5a9610d`
(64x64 RGBA, 2747 of 4096 pixels fully transparent), action
`c65c5052a355649be1d13ca27e3637d11002cfd452680892a19b38a1b4a26e07`
(128x128 RGBA, opaque). These differ from the packet's Pillow exports
(`3b69de40...`, `868d1044...`), which were not used. Catalog: two concepts,
nine exact consumers, coverage counts updated; `validate_icon_catalog.py` PASS.
Native UI presentation: PENDING. Owner visual approval: not asserted.

Packaging: the two icon exports are new packaged files, so `scripts/package.ps1`'s
fixed base count moves from 278 to 280 (the strict package validator already
derives the icon set from the catalog's runtime exports).

## Rules and adaptations

| Question | Decision | Evidence | Limitation |
| --- | --- | --- | --- |
| Panic | Native Frightened condition through `UnitFearController` | engine read | flees from visible enemies; held items not dropped |
| Repeat saves | Owned round save, intelligent only, at the native buff tick; DC fixed at application | engine read; tests | native timing |
| Immunity | No descriptors on the owned buff/ability (native turning exception) | engine read | separate Frightened condition immunity still blocks |
| Resistance / bonuses | Native ChannelResistance4 list extended; CotW resistances, Improved Channel (+2), Sacred Conduit (+1), Consecrate (+3) extended when CotW is present | engine/CotW read | Glory domain (+2 only "to halve the damage") and damage-only items do not apply. Xavorn's Cross: the native `XavornsCrossFeature` (`35f473c4`) is `IncreaseSpellDC` +2 on `ChannelNegativeEnergy` only, and the installed CotW 1.14.4c does not modify it (no `Xavorn` symbol in the DLL), so it never applies to a positive-only Turn. Consecrate's separate -1 save penalty to undead in its area is a target-side modifier; a CotW-present observation must report the caster DC and the target save total separately |
| Selective Channeling | Neither native nor CotW implements a targeting rule; Turn affects every undead in range, allies included | blueprint/CotW read | |
| Caster | The channeler is excluded (cannot flee from itself) | adaptation | |
| Overlap | Native `FrightenedUndead` stacking kept by the clone | engine | a new failed save replaces the effect |
| Menu | CotW: source child under that source's harm wrapper, direct action hidden; vanilla: direct action. A CotW contract drift (ambiguous wrapper, missing field) rolls back only the CotW changes and logs `[turn-undead][cotw.unavailable]`; the direct actions stay | CotW read (`IncreaseSpecifiedSpellsDC.spells`, `ContextSavingThrowBonusAgainstSpecificSpells.Spells` are public fields in the installed 1.14.4c DLL) | PENDING native |
| Module | Optional default-ON `turn-undead` key; not part of the guarded 13-module runtime matrix | settings tests | owner may prefer a full matrix module |

## Call of the Wild seams (review targets)

| Seam | Installed behavior | How Turn meets it | Evidence |
| --- | --- | --- | --- |
| DC bonus matching | `IncreaseSpecifiedSpellsDC` matches the executed ability or its parent | the Turn identities themselves are appended (the menu child's parent is a CotW wrapper not in the list) | CotW read; native PENDING |
| Resistance matching | `ContextSavingThrowBonusAgainstSpecificSpells` matches `Reason.Context.SourceAbility` exactly and reads the caster from that context for bypass features | initial save `Reason` = the Turn ability context; repeat save `Reason` = the flight buff, whose context `SourceAbility` is the Turn ability and whose caster is the channeler | engine/CotW read; native initial and repeat resistance PENDING |
| Timing | CotW builds its channel engine and later its new spells (Consecrate) inside its `LoadDictionary` postfix | the adapter runs on the first UMM update after core bootstrap, i.e. after the whole synchronous `LoadDictionary` postfix chain has returned, so every CotW field is final; a field still null is recorded as `=absent`, not treated as incompatibility | code read; native CotW-present log PENDING |
| Fallback | a contract drift throws inside the adapter | the adapter restores every recorded change (variants, parent links, direct visibility, every list) and the module logs `[turn-undead][cotw.unavailable]` with `cotw=failed` in the completion line. That state is baseline Turn Undead with reduced CotW compatibility (no submenu, no CotW DC/resistance), never reported as full compatibility | code read |

## Validation time

Development candidate builds on DATA, measured: `Build-Local -DevelopmentPurpose turn-undead-first -DevelopmentTestFilter turn-undead` at `6da197e9` ran 20:27-00:31 (4 h 04 min) and stopped at packaging (fixed in `d37f4af63`); the rebuild at `d37f4af63` ran 00:46-04:14 (3 h 28 min) and produced package `4fd152379c5ad7993dfd4522047d2122d956174a7bbe5a1db19290eadc2a2b48`, DLL `c53594af8d385fb355a8f7574e25243788599027ef01acd920cbc1cfbf3b95bc`. Almost all of the time is the repository-validation wrapper's per-file git object reads: the shared object store held 4,517 packs (~18k files in `objects/pack`; listing it took ~45 s, so each object-reading git command took ~58 s with no CPU and no lazy fetch). This is far outside the 15/30-minute validation budget; the owner authorized a repack after the rebuild. Native observation launches only after a candidate exists and its runtime transaction is admitted; the budget applies to each validation phase, and the build duration is reported here rather than repeated unchanged.

## Acceptance evidence index

| IDs | Result | Evidence and candidate | Limits |
| --- | --- | --- | --- |
| TU-01 | PASS | intake commit `5236e51a`; INTAKE.md; enrollment above | |
| TU-02 | PASS (contracts) | this record; installed assembly hash | native census PENDING |
| TU-03 | PASS (mapping) | source table; `turn-undead.source-catalog` | native per-source PENDING |
| TU-04 | PASS (policy) | inventory; `turn-undead.classification-*` | 2.1.7b census PENDING |
| others | NOT RUN | | |
