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
| Native blueprint leads | Community 2.1.4 archive, sha256 `6ddf3397...df60` (historical discovery data only). Installed 2.1.7b values for every blueprint Turn uses are the in-game contracts extract below; a full 2.1.7b undead census is pending |

## Engine contracts read from the installed 2.1.7b assembly

Two kinds of evidence, kept apart. Reference inspection: the installed
`Assembly-CSharp.dll` (sha256 `3b6450ff...5afb`) decompiled with ilspycmd 9.1
(`ilspycmd -p -o <dir> Assembly-CSharp.dll`; CotW types with
`ilspycmd -t <type> CallOfTheWild.dll`), which gives the behavior column.
In-game extract: the harness `contracts` mode, run inside the game on the
candidate, hashes each named method's IL from the loaded assembly and reads
every blueprint value Turn depends on:
[`docs/evidence/turn-undead/contracts-vanilla.json`](evidence/turn-undead/contracts-vanilla.json)
(run `20261128T115645Z`) and
[`contracts-cotw.json`](evidence/turn-undead/contracts-cotw.json) (run
`20261128T115425Z`; identical method IL with CotW loaded). Each extract names
the SHA-256 of the lab records it came from.

| Seam | Installed behavior (reference inspection) | Consequence | In-game extract (method IL sha256) |
| --- | --- | --- | --- |
| `RuleCalculateAbilityParams` | Without a calculator: caster level = character level, spell level = CL/2, statistic Charisma | Native channels use character level; Turn uses `ContextCalculateAbilityParamsBasedOnClass` (source class level, its statistic) | `OnTrigger` `da1f5455...`; `ContextCalculateAbilityParamsBasedOnClass.Calculate` `08b22ff3...`; `AbilityData.CalculateParams` (preview, no calculators) `4c712e79...` |
| `BuffStatusCondition` | `AddCondition`/`RemoveCondition` count; `OnNewRound` saves unconditionally with `Reason = Fact` | Owned `TurnUndeadFleeingState` mirrors it with an intelligent-only save | `OnNewRound` `7749d2cd...` |
| `UnitState` conditions | Reference counted | Ending Turn's effect never clears another fear source | reference inspection only |
| `UnitFearController` | A Frightened unit with conscious, visible enemies in its memory panics and runs ~20 m away; works in real time and turn-based | Native flight is away from visible enemies, not specifically the channeler; an allied undead with no visible enemies does not move | `TickOnUnit` `ba172395...` |
| `SavingThrowBonusAgainstSpecificSpells` | Matches `evt.Reason.Context.SourceAbility` against its list | Initial (Reason = context) and repeat (Reason = buff fact, context SourceAbility = the Turn ability) saves both match once Turn identities are listed | `OnEventAboutToTrigger` `2d3d7eff...` |
| `BuffDescriptorImmunity` / `SpellImmunityToSpellDescriptor` | Descriptor-based | The owned buff and abilities carry no descriptor, the native `FrightenedUndead` exception; undead keep every other immunity | reference inspection only |
| `AbilityTargetsAround` | All units within radius with LOS, then conditions; includes the caster | Turn condition: `UndeadType` only, so an undead channeler is itself a target | `Select` `1de1511c...` |
| `AddFeatureIfHasFact` | Applies on activation and every level-up; never removes | Grants a source's action when that source is present, including a later source | `OnFactActivate` and `HandleUnitGainLevel` `7ebb2355...` |
| `AbilityCasterHasFacts` | `NeedsAll` only tests the last fact | Two single-fact checkers (feat, source) | `CorrectCaster` `daed9202...` |
| `AbilityShowIfCasterHasFact` | Variant-menu visibility | Menu children shown only with feat and source | reference inspection only |
| `Fact.PreSave/PostLoad` | `[JsonProperty]` fields by component name | Flight state fields are primitives on `$KMG_TurnUndead_FleeingState` | reference inspection only |

## Source contract table

Values are the in-game contracts extract on the installed 2.1.7b blueprints
(`contracts-vanilla.json`, `sources[]`): each native harm donor is a 30-foot
`Any` burst spending the listed resource and cost; each source feature's
`AddFacts` grants that harm (vanilla) or CotW's wrapper (CotW profile). The
same mode asserts every constructed Turn action (direct and menu child)
against literal expectations written in the probe, independently of the
product catalog: DC calculator class and statistic, resource and cost, the
30-foot burst with the native target type and exactly the `UndeadType`
filter, the two caster-fact checks (feat; source feature), the alignment
check, and Will negates -> flight for 1 minute. Registration also refuses to
run on donor drift (`TurnUndeadBlueprints.ValidateNativeSource`).

| Source | Qualifying feature | DC level | DC statistic | Resource / cost | Restrictions | Direct / menu child | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Positive cleric | `ChannelEnergyFeature` `a79013ff` | cleric level | Charisma (printed "1/2 the cleric's level + Charisma") | `ChannelEnergyResource` `5e2bba3e`, 1 | none beyond the positive channel choice | `KMG.Feats.TurnUndead.Cleric.Ability` / `.MenuChild` | native harm `279447a6`; contracts extract; native first turn (cleric) |
| Ordinary paladin | `ChannelEnergyPaladinFeature` `cb6d55dd` (gained at 4th) | paladin level | Charisma (printed) | `LayOnHandsResource` `9dedf41d`, 2 | Lawful Good caster (`AbilityCasterAlignment` copied from native harm) | `...Paladin.Ability` / `.MenuChild` | native harm `4937473d`; contracts extract; native gameplay PENDING |
| Hospitaler | `ChannelEnergyHospitalerFeature` `a9ab1bbc` (gained at 4th) | paladin (hospitaler) level | Charisma (printed "1/2 the hospitaler's level") | `ChannelEnergyHospitalerResource` `b0e0c771`, 1 | Lawful Good caster | `...Hospitaler.Ability` / `.MenuChild` | native harm `cc17243b`; contracts extract; native gameplay PENDING |
| Empyreal sorcerer | `ChannelEnergyEmpyrealFeature` `7d49d7f5` (gained at 9th) | sorcerer level | Wisdom (printed; native `ReplaceAbilityDC` targets only the native heal/harm actions, so Turn sets Wisdom itself) | `ChannelEnergyEmpyrealResource` `f9af9354`, 1 | none | `...Empyreal.Ability` / `.MenuChild` | native harm `e1536ee2`; contracts extract; native gameplay PENDING |

Damage dice progressions (Hospitaler starting at 4th, Empyreal at 9th) are not
used for the DC.

Selected CotW profile boundary (`contracts-cotw.json`, `callOfTheWild`).
Qualifying features are exactly the four above. Call of the Wild 1.14.4c
registers 533 channel entries of every form (`channelEntriesByType`: plain,
quick, smite, swift, cone, line, Holy Vindicator shield, Back to the Grave);
none is a Turn identity (`channelEntriesThatAreTurn` 0). Of its 20 plain
positive-harm entries (`positiveHarmEntries`), 5 belong to the four
qualifying features. The paladin has two: the native `ChannelEnergyPaladinHarm`
(`LayOnHandsResource` x2, the contract Turn binds to) and CotW's
`PaladinChannelEnergyHarmUndeadExtra` (`PaladinExtraChannelResource` x1, the
pool CotW's Extra Channel gives paladins). Turn spends only lay on hands x2;
whether extra-channel uses should also fuel a paladin's Turn is an open owner
decision. The other 15 entries, from 13 CotW features, are unsupported (no
Turn action, not prerequisites): Antipaladin (positive, 2 entries),
Evangelist, Oracle Channel revelation, Oracle Spirit Guide life spirit,
Priest of the Fallen, Shaman life spirit, Shaman Spirit Warden, Shaman Witch
Doctor, Spirit Summoner life spirit, Spirit Whisperer life spirit, Sunsinger,
Warpriest (2 entries) and Witch. Whether any of them should qualify is an
open owner decision recorded here, not implemented. Negative channels, the positive-permission fact
`ChannelPositiveAllowed`, the wizard necromancer turning power and items do not
qualify. No source pool is created, refilled or shared.

## Installed contract fields (in-game extract)

From `contracts-vanilla.json` `nativeFacts` (full component dumps) and
`contracts-cotw.json` `callOfTheWild`:

| Blueprint | Observed fields | Consequence for Turn |
| --- | --- | --- |
| Native harm donors (4) | `AbilityTargetsAround` 30 ft, `Any`, no target condition; effect-level save `Unknown`; effect: `Conditional(CasterHasFact SelectiveChannel fd30c694)` -> `Conditional(IsEnemy, HasFact UndeadType)` -> ... `ContextActionSavingThrow(Will)`, else `Conditional(HasFact UndeadType)` -> ... `ContextActionSavingThrow(Will)`; no `SpellDescriptorComponent` | Turn copies the burst and this selective structure (save inside each branch, enemy-only with Selective Channel) and filters targets to `UndeadType` |
| Wizard Turn Undead `71b8898b` (native turning donor) | 30 ft `Any` burst with target condition `HasFact(UndeadType)`; `AbilityEffectRunAction` Will -> `ContextActionConditionalSaved`; `NecromancySchoolTurnUndeadResource` x1; no descriptor | Turn's target filter matches the native turning power; the wizard power is untouched |
| `FrightenedUndead` `1265c0b2` | `BuffStatusCondition` Frightened, SaveType Will, SaveEachRound; no descriptor | the flight clone keeps Replace stacking and replaces the logic with the owned intelligent-only state |
| `UndeadType` -> `UndeadImmunities` `8a75eb16` | condition immunities Nauseated, Fatigued, Sickened (not Frightened); `BuffDescriptorImmunity` masks 6597704040704 and 4366631152 (the latter ignored by Bloodline Undead Arcana); `SpellImmunityToSpellDescriptor` masks 2199657529600 and 4366631152; drain and negative-energy immunity | the Turn actions and flight effect carry no descriptor and Frightened is not condition-immune, so undead remain turnable while every other immunity stands |
| `ChannelEnergyEmpyrealFeature` | `ReplaceAbilityDC` Wisdom on `ChannelEnergyEmpyrealHarm` and `ChannelEnergyEmpyrealHeal` only | the override does not reach Turn, so the Empyreal Turn's own calculator uses Wisdom |
| `ChannelResistance4` `a9ac84c6` | one `SavingThrowBonusAgainstSpecificSpells`: the 4 native harms plus the 8 Turn identities | channel resistance applies to Turn saves (native observation PENDING) |
| CotW DC holders | `IncreaseSpecifiedSpellsDC.BonusDC`: Improved Channel +2, Sacred Conduit +1, Consecrate +3; each lists all 8 Turn identities (`turnMembership`) | CotW DC bonuses apply to Turn (native observation PENDING) |
| CotW channel resistances | 13 facts (ChannelResistance4, Undead Bolster, Oracle/Shaman/Spirit deathly-being and revelation facts), each listing all 8 Turn identities | CotW channel resistance applies to Turn (native observation PENDING) |

## Classification inventory

Policy: `src/KingmakerGunslinger/TurnUndead/TurnUndeadClassificationPolicy.cs`.
Reviewed fixture: `tests/fixtures/turn-undead/native-undead-classification.json`
(139 native undead units, 41 mindless). Provenance: historical data from the
community 2.1.4 blueprint archive, labelled as such; the installed-2.1.7b
census is pending. Native 2.1.7b classification is observed so far only for
the ordinary skeleton (`mindless-family`) and the level-1 zombie lord
(`intelligent-identity-fact`) in the first turn.

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
| Selective Channeling | The native harm rule: with the caster's Selective Channel feat only enemy undead are affected and excluded allies roll nothing; without it every undead in range, allies included. (An earlier version of this record said no native rule existed; the in-game dump showed otherwise and Turn now mirrors it, reading the fact from the installed donor.) | native `selective` step (run `20261128T115645Z`): without the feat an enemy and an allied skeleton were both turned; with it only the enemy, the ally made no save | |
| Caster | An undead channeler is a target of its own Turn, like any undead in the native burst (no caster exception) | native `undead-caster`: an undead cleric made one Will save at DC 15 against its own Turn; a living cleric is never a target | a failed undead channeler flees from visible enemies like any other target |
| Overlap | Native `FrightenedUndead` stacking kept by the clone | engine | a new failed save replaces the effect |
| Menu | CotW: source child under that source's harm wrapper, direct action hidden; vanilla: direct action. A CotW contract drift (ambiguous wrapper, missing field) rolls back only the CotW changes and logs `[turn-undead][cotw.unavailable]`; the direct actions stay | CotW read (`IncreaseSpecifiedSpellsDC.spells`, `ContextSavingThrowBonusAgainstSpecificSpells.Spells` are public fields in the installed 1.14.4c DLL) | PENDING native |
| Module | Optional default-ON `turn-undead` key outside the 13-module matrix. A file without the key (earlier releases) keeps it ON. Generated profiles carry an independent `turnUndead` value (module matrix `-TurnUndead`, compatibility runner parameter, shared `New-KmgFeatureModuleSettingsDocument`; stripped before the product observation); without it they write OFF only in the all-off profile | `Test-FeatureModuleCompatibilityParameters.ps1` (8240 checks, generator side of `tests/fixtures/turn-undead/generated-profiles/*`); `turn-undead.settings-key` (legacy default, OFF round trip, the same generated files loaded) | owner may prefer a full matrix module |

## Call of the Wild seams (review targets)

| Seam | Installed behavior | How Turn meets it | Evidence |
| --- | --- | --- | --- |
| DC bonus matching | `IncreaseSpecifiedSpellsDC` matches the executed ability or its parent | the Turn identities themselves are appended (the menu child's parent is a CotW wrapper not in the list) | CotW read; native PENDING |
| Resistance matching | `ContextSavingThrowBonusAgainstSpecificSpells` matches `Reason.Context.SourceAbility` exactly and reads the caster from that context for bypass features | initial save `Reason` = the Turn ability context; repeat save `Reason` = the flight buff, whose context `SourceAbility` is the Turn ability and whose caster is the channeler | engine/CotW read; native initial and repeat resistance PENDING |
| Timing | CotW builds its channel engine and later its new spells (Consecrate) inside its `LoadDictionary` postfix | the adapter runs in publication, on the first UMM update after core bootstrap reports ready (no update cap), i.e. after the whole synchronous `LoadDictionary` postfix chain has returned, so every CotW field is final | CotW run `20261128T115425Z`: completion line `cotw=present 1.14.4c-2.1`, four menu children placed, no drift |
| Registration lifecycle | | registration of the ten saved identities waits for the core ready or terminal-failure state and is a separate phase: a publication failure (resistance, CotW, feat offer) rolls back only its own foreign changes, keeps the identities resolvable and logs `[turn-undead][publication.failed]` (`TurnUndeadLifecyclePolicy`) | `turn-undead.lifecycle` |
| Seam shape | each reflected member is required contract, its value optional | a missing or mistyped member, a missing `ChannelEnergyEngine`, a present holder without `IncreaseSpecifiedSpellsDC`, or a resistance fact without an ability-specific list is drift; a member whose value CotW never created is `=absent` (`TurnUndeadCotwSeamPolicy`) | `turn-undead.cotw-seams` |
| Failure boundary (observed) | | `cotw-rollback` (run `20261128T115425Z`): after the live publication was rolled back, a substitute engine with a valid Improved Channel holder and no `sacred_conduit` member threw "Call of the Wild DC seam sacred_conduit drifted (field missing)" after placing all four menu children and appending to Improved Channel; every child parent, wrapper variant list (element identity and order), direct visibility and list matched the pre-publication snapshot exactly. An optional-null `consecrate_buff` was reported `=absent`; an already-populated second publication changed nothing; re-publication equalled the original; removed and swapped children failed the independent placement checker | `TurnUndeadCotwPublication.PublishWithEngine` |
| Fallback | drift throws inside the adapter | the adapter restores every recorded change (variants, parent links, direct visibility, every list) and the module logs `[turn-undead][cotw.unavailable]` naming the missing behavior, with `cotw=failed` in the completion line. That state is baseline Turn Undead with reduced CotW compatibility (no submenu, no CotW DC/resistance), never reported as full compatibility | code read; CotW run logged `cotw=present` with no drift warning |

## Validation time

Development candidate builds on DATA, measured: `Build-Local -DevelopmentPurpose turn-undead-first -DevelopmentTestFilter turn-undead` at `6da197e9` ran 20:27-00:31 (4 h 04 min) and stopped at packaging (fixed in `d37f4af63`); the rebuild at `d37f4af63` ran 00:46-04:14 (3 h 28 min) and produced package `4fd152379c5ad7993dfd4522047d2122d956174a7bbe5a1db19290eadc2a2b48`, DLL `c53594af8d385fb355a8f7574e25243788599027ef01acd920cbc1cfbf3b95bc`. Almost all of the time is the repository-validation wrapper's per-file git object reads: the shared object store held 4,517 packs (~18k files in `objects/pack`; listing it took ~45 s, so each object-reading git command took ~58 s with no CPU and no lazy fetch). This is far outside the 15/30-minute validation budget; the owner authorized a repack after the rebuild. After `git repack -a -d --cruft` (04:28-04:34, 3 packs, 1.02 GiB) the same lookup takes 0.02 s. The first native session attempt at `d37f4af63` was refused at admission before anything was acquired, because the probe negative control and this record (then under planning/) had been committed after the candidate; every file outside docs/ is a candidate material input. The record moved under docs/ and the candidate was rebuilt with the probe change. Native observation launches only after a candidate exists and its runtime transaction is admitted; the budget applies to each validation phase, and the build duration is reported here rather than repeated unchanged.

## Native evidence (intermediate checkpoint)

Machine-generated extract of every admitted Turn Undead run, with source-record
hashes, restoration receipts and probe results:
[`docs/evidence/turn-undead/native-runs.json`](evidence/turn-undead/native-runs.json),
produced by `docs/evidence/turn-undead/extract_native_evidence.py` from the
lab's retained session and harness records. Raw logs, saves and profiles stay
in the lab.

| Field | Value |
| --- | --- |
| Runs | vanilla `20261128T115645Z-umm-0.31.1-turn-undead-first` (steps contracts, first-turn, undead-caster, selective, abandon, abandon-interrupt; PASS 142 s) and Call of the Wild alone `20261128T115425Z-umm-0.31.1-turn-undead-cotw-contracts` (contracts, cotw-rollback; PASS 100 s) |
| Candidate | producer commit `e06386e19d7d1b42b61d341a9ff9f5db96c7f4d2`, package `6e692d4e119658866b5e4aaf0d74c19deb9533ebb01dd9a1072b5f19b1b25f4f`, DLL `a2c0f2191815cad61964ca4500a95ccd545c32338c42dd9d136ca5bec10636ab` (the loaded DLL, `identity.productSha256`); product sources identical to `29d08f7d1` |
| Runtime | disposable copy `game-copies/loader-compat`, profile `umm-0.31.1` (UMM `2e038ff2...`, Harmony12 `aa1cd483...`), `locale=enGB`; fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`referenceFixture.sha256AtExtraction` `0be64b3f...a249`; each session staged its own copy) |
| Observer | `tools/LoaderCompatHarness` at the producer commit (`TurnUndeadProbes.cs`, `TurnUndeadContractProbes.cs`, `TurnUndeadCotwProbes.cs`); harness DLL `224116ace1b325a105c4cf3b404b5b56a16bfc2385695e73a43301f4d1900841` |
| Restoration | vanilla: lease `runtime-20261128T115652Z-4304...`, transaction `403080b8...`; CotW: lease `runtime-20261128T115433Z-0285...`, transaction `4231171c...`. Both: transaction succeeded, ownership completed, registry restored and verified, owner integrity verified (owner saves and install unchanged); every probe step `isolationRestored`, cleanup without failures |
| Fixture disclosure | pre-existing units (25 at the first turn) silenced and restored on release; units that had a command (2 at the first turn) were interrupted, which is not restorable and is contained to the disposable launch (`cleanupScope`); spawned units brain-off; Will totals set on spawned undead; positive channel, feat and Selective Channel added as facts |

Observed in the vanilla run (unit identities and values in `native-runs.json`):

- Completion line `registered=10;featPublished=True;resistance=a9ac84c6...:4+8;cotw=absent;menuChildren=0`; no failure, publication-failure or drift lines. Contracts: no problems.
- First turn, cleric 5 / fighter 2, Charisma +3: the feat granted exactly the cleric action without changing the pool (6). Negative control: same build, pool 6, no feat: no Turn action and the blueprint fails its caster checks; the feat holder passes them.
- One `UnitUseAbility` command (Success): pool 6 -> 5. Three values are kept apart: the blueprint's localized texts "Will negates" and "1 minute" (strings, not a rendered tooltip), the `AbilityData.CalculateParams()` preview 16 (that path runs no ability calculator), and the rolled DC 15 on every Turn save (10 + 5/2 + 3). Skeleton fail, zombie lord fail, Will-raised zombie lord pass (no effect); the living channeler made no save; an undead 16.0 m away (beyond 30 feet, not at the boundary) and a living hostile 3.0 m away were unaffected; no Turn damage or healing and no other damage to tracked units.
- Persisted flight state: skeleton `mindless-family`, zombie lord `intelligent-identity-fact`, DC 15, source = the cleric action.
- 13 s real time: skeleton 2.40 -> 18.57 m, zombie lord 4.51 -> 20.84 m; skeleton no repeat save; zombie lord two failed round saves 6 s apart, then a success after its Will was raised ended the effect. The skeleton's effect ended 61.9 s after the cast, unrefreshed.
- Undead caster: an undead cleric made exactly one Will save (DC 15, passed) against its own Turn; a living control in range was untouched.
- Selective: without Selective Channel an enemy and an allied (player-faction) skeleton were both turned; with it only the enemy, and the ally rolled no save.
- Abandon: a suspended first-turn probe's release restored the prior pause flag, 15/15 brains and its spawned unit. Abandon-interrupt: interrupting the abandon check itself after its nested probe's first yield restored pause, brains, the nested rule subscription and the spawned unit; with an injected nested cleanup failure the same state was restored but the verdict was correctly not restored.

Other admitted attempts, all restored (details in `native-runs.json`): `094042Z`, `095145Z`, `095946Z` (probe/fixture faults), `100756Z` (PASS before round 1), `104421Z` (abandon judged in the same frame), `115645Z`/`105459Z` (PASS at round 1), `112755Z`/`113029Z` and `113902Z`/`114135Z` (PASS during round 2, before the dump and selective corrections), `115155Z` (fixture-inconclusive: the zombie lord rolled a natural 20 on its second repeat save, an automatic success, so its effect ended as the rules require; rerun unchanged as `115645Z`). One attempt at `d37f4af63` was refused at admission before anything was acquired.

## Acceptance evidence index

Statuses at this intermediate checkpoint cover only what was measured; anything
not measured stays PENDING. Evidence: `native-runs.json` (run named) and the
contracts extracts.

| IDs | Result | Evidence and candidate | Limits |
| --- | --- | --- | --- |
| TU-01 | PASS | intake commit `5236e51a`; INTAKE.md; enrollment above | |
| TU-02 | PARTIAL | reference inspection + in-game contracts extract (method IL, full donor/immunity/override/resistance dumps) on `e06386e1` | full 2.1.7b undead census PENDING |
| TU-03 | PARTIAL | source table; contracts extract asserts all four constructed sources | paladin/Hospitaler/Empyreal gameplay PENDING |
| TU-04 | PARTIAL | policy and reviewed historical (2.1.4) inventory; native 2.1.7b skeleton and level-1 zombie lord classification | installed-2.1.7b census PENDING |
| TU-06 | PARTIAL | `115645Z` negative control: positive channel without the feat has no Turn action and fails caster checks | negative-only cleric, non-channel class and permission-only fixtures through normal acquisition PENDING |
| TU-08 | PARTIAL | `115645Z`: one cleric activation affecting several undead spent one use (6 -> 5) | paladin two uses, Hospitaler, Empyreal PENDING |
| TU-13 | PARTIAL | `115645Z`: one real-time `UnitUseAbility` command succeeded once | standard-action spending, attack-of-opportunity behavior and turn-based PENDING |
| TU-14 | PARTIAL | `115645Z`: in-range undead affected; an undead 16.0 m away and an in-range living hostile unaffected; `undead-caster`: an undead channeler targeted, a living one never | the 30-foot boundary itself not probed |
| TU-16 | PASS (cleric, real time) | `115645Z` selective: without Selective Channel allied and enemy undead are turned; with it only enemy undead, allies roll nothing (the native harm rule) | turn-based PENDING |
| TU-17 | PASS | `115645Z`: success prevents, failure applies; mindless and intelligent both saved initially | |
| TU-18 | PASS | `115645Z`: mindless skeleton fled the whole minute with no repeat save | |
| TU-19 | PARTIAL | `115645Z`: round saves 6 s apart, failure retains, success ends | resource after recovery not measured; turn-based PENDING |
| TU-20 | PASS | `115645Z`: unrefreshed end 61.9 s after the cast | |
| TU-21 | PARTIAL | `115645Z`: cleric 5 + fighter 2, Cha +3 -> rolled DC 15 (unrelated levels excluded); constructed calculator class/statistic asserted for all sources | odd/even levels and other statistics PENDING; the tooltip exposes no number |
| TU-24 | PARTIAL | `115645Z`: no Turn damage or healing | allegiance and altered-HP group PENDING |
| TU-28 | PARTIAL | vanilla direct action, source and icon identity; blueprint save/duration strings recorded separately from the preview DC 16 and the rolled DC 15 | rendered tooltip, UI and hotbar presentation PENDING |
| TU-32 | PARTIAL | vanilla run (no dependency) and CotW run (menu placement checked against the grant graph, DC/resistance list membership, failure boundary observed) | CotW gameplay, DC and resistance observations PENDING |
| TU-33 | PARTIAL | CotW run: none of the 533 channel entries of any form (quick, smite, swift, cone, line, ...) is a Turn identity | existing heal/harm action behavior and the CotW negative-affinity rewrite PENDING |
| others | NOT RUN | | |
