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
(run `20261128T105226Z`) and
[`contracts-cotw.json`](evidence/turn-undead/contracts-cotw.json) (run
`20261128T105459Z`; identical method IL with CotW loaded). Each extract names
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

Selected CotW profile boundary (`contracts-cotw.json`,
`callOfTheWild.positiveHarmEntries`): Call of the Wild 1.14.4c registers 20
plain positive-harm channel entries. The four sources above are supported.
The other 16 entries, from 13 CotW features, are unsupported (no Turn
action, not prerequisites): Antipaladin (positive), Evangelist, Oracle
Channel revelation, Oracle Spirit Guide life spirit, Priest of the Fallen,
Shaman life spirit, Shaman Spirit Warden, Shaman Witch Doctor, Spirit
Summoner life spirit, Spirit Whisperer life spirit, Sunsinger, Warpriest
and Witch. Whether any of them should qualify is an open owner decision
recorded here, not implemented. Negative channels, the positive-permission fact
`ChannelPositiveAllowed`, the wizard necromancer turning power and items do not
qualify. No source pool is created, refilled or shared.

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
| Selective Channeling | Neither native nor CotW implements a targeting rule; Turn affects every undead in range, allies included | blueprint/CotW read | |
| Caster | An undead channeler is a target of its own Turn, like any undead in the native burst (no caster exception) | native `undead-caster`: an undead cleric made one Will save at DC 15 against its own Turn; a living cleric is never a target | a failed undead channeler flees from visible enemies like any other target |
| Overlap | Native `FrightenedUndead` stacking kept by the clone | engine | a new failed save replaces the effect |
| Menu | CotW: source child under that source's harm wrapper, direct action hidden; vanilla: direct action. A CotW contract drift (ambiguous wrapper, missing field) rolls back only the CotW changes and logs `[turn-undead][cotw.unavailable]`; the direct actions stay | CotW read (`IncreaseSpecifiedSpellsDC.spells`, `ContextSavingThrowBonusAgainstSpecificSpells.Spells` are public fields in the installed 1.14.4c DLL) | PENDING native |
| Module | Optional default-ON `turn-undead` key outside the 13-module matrix. A file without the key (earlier releases) keeps it ON. Generated profiles write it explicitly and turn it OFF in the all-off profile (`New-KmgFeatureModuleSettingsDocument`, compatibility runner) | `turn-undead.settings-key` (legacy default, OFF round trip, generated all-off load); `Test-FeatureModuleCompatibilityParameters.ps1` (8228 checks: OFF only in the all-off mask, both generators identical) | owner may prefer a full matrix module |

## Call of the Wild seams (review targets)

| Seam | Installed behavior | How Turn meets it | Evidence |
| --- | --- | --- | --- |
| DC bonus matching | `IncreaseSpecifiedSpellsDC` matches the executed ability or its parent | the Turn identities themselves are appended (the menu child's parent is a CotW wrapper not in the list) | CotW read; native PENDING |
| Resistance matching | `ContextSavingThrowBonusAgainstSpecificSpells` matches `Reason.Context.SourceAbility` exactly and reads the caster from that context for bypass features | initial save `Reason` = the Turn ability context; repeat save `Reason` = the flight buff, whose context `SourceAbility` is the Turn ability and whose caster is the channeler | engine/CotW read; native initial and repeat resistance PENDING |
| Timing | CotW builds its channel engine and later its new spells (Consecrate) inside its `LoadDictionary` postfix | the adapter runs in publication, on the first UMM update after core bootstrap reports ready (no update cap), i.e. after the whole synchronous `LoadDictionary` postfix chain has returned, so every CotW field is final | CotW run `20261128T105459Z`: completion line `cotw=present 1.14.4c-2.1`, four menu children placed, no drift |
| Registration lifecycle | | registration of the ten saved identities waits for the core ready or terminal-failure state and is a separate phase: a publication failure (resistance, CotW, feat offer) rolls back only its own foreign changes, keeps the identities resolvable and logs `[turn-undead][publication.failed]` (`TurnUndeadLifecyclePolicy`) | `turn-undead.lifecycle` |
| Seam shape | each reflected member is required contract, its value optional | a missing or mistyped member, a missing `ChannelEnergyEngine`, a present holder without `IncreaseSpecifiedSpellsDC`, or a resistance fact without an ability-specific list is drift; a member whose value CotW never created is `=absent` (`TurnUndeadCotwSeamPolicy`) | `turn-undead.cotw-seams` |
| Fallback | drift throws inside the adapter | the adapter restores every recorded change (variants, parent links, direct visibility, every list) and the module logs `[turn-undead][cotw.unavailable]` naming the missing behavior, with `cotw=failed` in the completion line. That state is baseline Turn Undead with reduced CotW compatibility (no submenu, no CotW DC/resistance), never reported as full compatibility | code read; CotW run logged `cotw=present` with no drift warning |

## Validation time

Development candidate builds on DATA, measured: `Build-Local -DevelopmentPurpose turn-undead-first -DevelopmentTestFilter turn-undead` at `6da197e9` ran 20:27-00:31 (4 h 04 min) and stopped at packaging (fixed in `d37f4af63`); the rebuild at `d37f4af63` ran 00:46-04:14 (3 h 28 min) and produced package `4fd152379c5ad7993dfd4522047d2122d956174a7bbe5a1db19290eadc2a2b48`, DLL `c53594af8d385fb355a8f7574e25243788599027ef01acd920cbc1cfbf3b95bc`. Almost all of the time is the repository-validation wrapper's per-file git object reads: the shared object store held 4,517 packs (~18k files in `objects/pack`; listing it took ~45 s, so each object-reading git command took ~58 s with no CPU and no lazy fetch). This is far outside the 15/30-minute validation budget; the owner authorized a repack after the rebuild. After `git repack -a -d --cruft` (04:28-04:34, 3 packs, 1.02 GiB) the same lookup takes 0.02 s. The first native session attempt at `d37f4af63` was refused at admission before anything was acquired, because the probe negative control and this record (then under planning/) had been committed after the candidate; every file outside docs/ is a candidate material input. The record moved under docs/ and the candidate was rebuilt with the probe change. Native observation launches only after a candidate exists and its runtime transaction is admitted; the budget applies to each validation phase, and the build duration is reported here rather than repeated unchanged.

## Native evidence (intermediate checkpoint)

Machine-generated extract of every admitted Turn Undead run, with source-record
hashes, restoration receipts and the probe results:
[`docs/evidence/turn-undead/native-runs.json`](evidence/turn-undead/native-runs.json),
produced by `docs/evidence/turn-undead/extract_native_evidence.py` from the
lab's retained session and harness records. Raw logs, saves and profiles stay
in the lab.

| Field | Value |
| --- | --- |
| Runs | `20261128T105226Z-umm-0.31.1-turn-undead-first` (vanilla; steps contracts, first-turn, undead-caster, abandon; PASS 136 s) and `20261128T105459Z-umm-0.31.1-turn-undead-cotw-contracts` (Call of the Wild alone; contracts; PASS 102 s) |
| Candidate | producer commit `d634f3bedd305e8161dc25e9c0133cf6e6a94ea8`, package `7affa28eaa957d98abf95913ec48ec471e737fa6a74c8b60ea7aeb832b6bf5d1`, DLL `7da2b4aeeb9a615aa966ef8132ca820a3d4d0e7ae8390a26c5a3d811aa409bf9` (loaded DLL identical, `identity.productSha256`); product sources identical to `5e95559c2` |
| Runtime | disposable copy `game-copies/loader-compat`, profile `umm-0.31.1` (UMM `2e038ff2...`, Harmony12 `aa1cd483...`), `locale=enGB`; fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks` (sha256 `0be64b3f58a0e0bea1b4951f0f6d145e27fd72e6da566acbf5b0b4aff8eda249`) |
| Observer | `tools/LoaderCompatHarness` at the producer commit (`TurnUndeadProbes.cs`, `TurnUndeadContractProbes.cs`); harness DLL `ea7a838d3f9399292c02cace33ba503c25a2c6c7fda35577c6e78f314b6c98c6` |
| Restoration (vanilla run) | lease `runtime-20261128T105234Z-74d7...`, transaction `31df9c79...`: transaction succeeded, ownership completed, registry restored and verified, owner integrity verified (95 owner saves and the owner install unchanged); every probe step `isolationRestored`, cleanup with no failures |
| Fixture disclosure | 25 pre-existing units (area NPCs and the party character) silenced and restored on release; 2 of them had a command interrupted, which is not restorable and is contained to the disposable launch (recorded as `cleanupScope`); spawned units brain-off; Will totals set on spawned undead; positive channel and feat added as facts |

Observed in the vanilla run:

- Completion line `registered=10;featPublished=True;resistance=a9ac84c6...:4+8;cotw=absent;menuChildren=0`; no failure, publication-failure or drift lines.
- Contracts: no problems (every constructed component matched its literal expectation).
- First turn, cleric 5 / fighter 2, Charisma +3: the feat granted exactly the cleric action and did not change the pool (6). Negative control (same build, pool 6, no feat): no Turn action, blueprint refused by its caster checks; the feat holder passes them.
- One `UnitUseAbility` command (Success): pool 6 -> 5. Initial Will saves at DC 15 (10 + 5/2 + 3; character level would give 16): skeleton fail, zombie lord fail, Will-raised zombie lord pass (no effect). The living channeler made no save. A skeleton 16.0 m away (beyond 30 feet, not at the boundary) and a living hostile 3.0 m away were unaffected. No Turn damage or healing; no other damage to tracked units.
- Persisted flight state: skeleton `mindless-family`, zombie lord `intelligent-identity-fact`, DC 15, source = the cleric action; both Frightened.
- 13 s real time: skeleton 2.30 -> 20.80 m, zombie lord 4.51 -> 20.80 m; skeleton no repeat save; zombie lord two failed round saves (DC 15, reason = the flight effect), then a success 4.7 s after its Will was raised ended the effect.
- The skeleton's effect ended 61.9 s after the cast, unrefreshed.
- Undead caster: an undead cleric made exactly one Will save (DC 15, passed) against its own Turn; the skeleton beside it was turned; a living control in range was untouched.
- Abandon: a first-turn probe released while suspended after pausing and silencing restored the prior pause flag (false), all 15 brains and its spawned unit (destroyed one frame later), with a clean cleanup verdict.
- `AbilityData.CalculateParams()` reports 16 (no ability calculators on that path); the tooltip shows only "Will negates", and every rolled save used 15.

Earlier admitted attempts, all restored (transaction succeeded, owner integrity verified, registry restored; details in `native-runs.json`): `20261128T094042Z` (preview-API assertion; far target collapsed to 2.11 m), `20261128T095145Z` (an area unit damaged the fleeing skeleton), `20261128T095946Z` (the party character attacked tracked units), `20261128T100756Z` (PASS on candidate `4b5f14e11297` before the round-1 corrections), `20261128T104421Z` (abandon check judged destruction in the same frame). One further attempt at `d37f4af63` was refused at admission before anything was acquired.

## Acceptance evidence index

Statuses at this intermediate checkpoint cover only what was measured; anything
not measured stays PENDING. Evidence: `native-runs.json` (run named) and the
contracts extracts.

| IDs | Result | Evidence and candidate | Limits |
| --- | --- | --- | --- |
| TU-01 | PASS | intake commit `5236e51a`; INTAKE.md; enrollment above | |
| TU-02 | PARTIAL | reference inspection + in-game contracts extract (method IL, blueprint values) on `d634f3be` | full 2.1.7b undead census PENDING |
| TU-03 | PARTIAL | source table; contracts extract asserts all four constructed sources | paladin/Hospitaler/Empyreal gameplay PENDING |
| TU-04 | PARTIAL | policy and reviewed historical (2.1.4) inventory; native 2.1.7b skeleton and level-1 zombie lord classification | installed-2.1.7b census PENDING |
| TU-06 | PARTIAL | `105226Z` negative control: positive channel without the feat has no Turn action and fails caster checks | negative-only cleric, non-channel class and permission-only fixtures through normal acquisition PENDING |
| TU-08 | PARTIAL | `105226Z`: one cleric activation affecting several undead spent one use (6 -> 5) | paladin two uses, Hospitaler, Empyreal PENDING |
| TU-13 | PARTIAL | `105226Z`: one real-time `UnitUseAbility` command succeeded once | standard-action spending, attack-of-opportunity behavior and turn-based PENDING |
| TU-14 | PARTIAL | `105226Z`: in-range undead affected; an undead 16.0 m away and an in-range living hostile unaffected; `undead-caster`: an undead channeler targeted, a living one never | the 30-foot boundary itself not probed |
| TU-17 | PASS | `105226Z`: success prevents, failure applies; mindless and intelligent both saved initially | |
| TU-18 | PASS | `105226Z`: mindless skeleton fled the whole minute with no repeat save | |
| TU-19 | PARTIAL | `105226Z`: round saves 6 s apart, failure retains, success ends | resource after recovery not measured; turn-based PENDING |
| TU-20 | PASS | `105226Z`: unrefreshed end 61.9 s after the cast | |
| TU-21 | PARTIAL | `105226Z`: cleric 5 + fighter 2, Cha +3 -> rolled DC 15 (unrelated levels excluded); constructed calculator class/statistic asserted for all sources | odd/even levels and other statistics PENDING; the tooltip exposes no number |
| TU-24 | PARTIAL | `105226Z`: no Turn damage or healing | allegiance and altered-HP group PENDING |
| TU-28 | PARTIAL | `105226Z`: vanilla direct action, source, tooltip texts and icon identity | UI/hotbar presentation PENDING |
| TU-32 | PARTIAL | vanilla run (CotW absent, no dependency) and CotW run (menu placement, no drift) | CotW gameplay, DC and resistance observations PENDING |
| TU-33 | PARTIAL | CotW run: none of the 20 positive-harm entries is a Turn identity (no generated quick/smite/cone/line forms) | existing heal/harm action behavior PENDING |
| others | NOT RUN | | |
