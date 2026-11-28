# Turn Undead source map

Research was checked on 2026-11-27. Links identify the primary rules, source code and authoring contracts used to prepare this mission. Read current installed instructions at intake; pinned links make the research reproducible and do not establish the current state of the owner's machine.

## 1. Rules

| Source | Use in this mission |
| --- | --- |
| [Turn Undead — Archives of Nethys](https://aonprd.com/FeatDisplay.aspx?ItemName=Turn%20Undead) | Base general feat: positive-channel prerequisite, expenditure, area, Will save, flight and intelligent recovery. Do not import the separate mythic section. |
| [Turn Undead — owner's d20PFSRD link](https://www.d20pfsrd.com/feats/general-feats/turn-undead/) | Requested feature reference; cross-check terminology against the base rules. |
| [Core Rulebook feats](https://legacy.aonprd.com/coreRulebook/feats.html) | General-feat context and channel-related feat text. |
| [Cleric class](https://aonprd.com/ClassDisplay.aspx?ItemName=Cleric) | Positive/negative channel choice, normal source scaling and Charisma-based channel rules. |
| [Paladin class](https://aonprd.com/ClassDisplay.aspx?ItemName=Paladin) | Real channel acquisition and underlying Lay on Hands expenditure. |
| [Hospitaler archetype](https://aonprd.com/ArchetypeDisplay.aspx?FixedName=Paladin%20Hospitaler) | Separate channel resource and archetype source contract. |
| [Empyreal bloodline](https://aonprd.com/WildbloodedDisplay.aspx?FixedName=Empyreal) | Source-specific casting/channel differences; compare with installed game parameter overrides. |
| [Improved Channel](https://aonprd.com/FeatDisplay.aspx?ItemName=Improved%20Channel) | Applicable increase to channel saving-throw DC; verify optional provider registration. |
| [Selective Channeling](https://aonprd.com/FeatDisplay.aspx?ItemName=Selective%20Channeling) | Target-selection exception. Do not silently replace the base feat's all-undead targeting with enemy-only behavior. |
| [Core Rulebook FAQ](https://paizo.com/paizo/faq/v5748nruor1fm) | Independent channel sources/multiclass progression; do not sum unrelated class levels by assumption. |
| [Universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html) | Channel resistance and undead-specific rule interactions. |
| [Conditions glossary](https://legacy.aonprd.com/coreRulebook/glossary.html) | Panic semantics; distinguish full tabletop condition from the native fleeing adaptation chosen for this mission. |

The packet paraphrases the relevant mechanics and gives implementation decisions. It does not bundle a rulebook or pretend to resolve every source-specific engine discrepancy in advance.

## 2. Native blueprint evidence

Source: [KingmakerDataminer blueprint release](https://github.com/spacehamster/KingmakerDataminer/releases/tag/blueprints), specifically [Blueprints2.1.4.zip](https://github.com/spacehamster/KingmakerDataminer/releases/download/blueprints/Blueprints2.1.4.zip).

Archive SHA-256 inspected during research:

`6ddf33970fb9ed3c127dd8f30b6416d7d23f607303ca61a31aab1972ed43df60`

This is community-published data for **Kingmaker 2.1.4**, not direct proof of the supported 2.1.7b installation or any modded runtime. It is not included in the ZIP. The technical contract contains the small set of identifiers and structural observations needed to locate the actual definitions in an authorized installation.

| Definition | Historical identifier | Why inspect it |
| --- | --- | --- |
| `TurnUndeadNecromancy` | `71b8898b1d26d654b9a3eeac87e3e2f8` | Native supernatural area/save/flight donor; wizard binding must be replaced privately |
| `FrightenedUndead` | `1265c0b27ef559c4e980a900dfc68d1e` | Native undead-specific flight; unconditional per-round saving is a known donor hazard |
| `UndeadType` | `734a29b693e9ec346ba2951b27987e33` | Actual undead identity rather than negative-energy affinity |
| `UndeadImmunities` | `8a75eb16bfff86949a4ddcb3dd2f83ae` | Preserve ordinary undead immunities while using the narrow native exception |
| `ChannelResistance4` | `a9ac84c6f48b491438f91bb237bc9212` | Ability-specific list; a new Turn identity needs deliberate resistance integration |
| General feat selection | `247a4068296e8be42890143f451b4b45` | Publication lead confirmed by current project publication code |

The technical contract also records source resource/ability identifiers and counterexamples for mindless classification. Do not allocate new gameplay GUIDs from these native IDs or overwrite these definitions.

## 3. Tabletop Rules Expansion source

Repository: [howardreith/KingmakerGunslingerAndOtherTabletopRules](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules). Inspected `master`: **`52755b617c79328a972ae5b74bc05784422e332d`**.

| Pinned source | Purpose |
| --- | --- |
| [AGENTS.md](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/AGENTS.md) | Project operating instructions; read current version at intake |
| [Validation policy](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/VALIDATION-POLICY.md) | Focused gates, budgets, evidence and safe runtime procedures |
| [Review workflow](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/AGENT-REVIEW-WORKFLOW.md) | Repository side of the controller workflow |
| [FeatureModuleConfiguration.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs) | Existing discrete module pattern; no assumed generic extra-feat bucket |
| [BlueprintBootstrap.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs) | Registration versus publication and save hydration |
| [AcadamaeFeatCatalogPublication.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/AcadamaeFeatCatalogPublication.cs) | General feat selection arrays, deduplication and rollback |
| [BodyguardFeatCatalogPublication.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/BodyguardFeatCatalogPublication.cs) | Another bounded feature publication example |
| [BlueprintRegistry.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/BlueprintRegistry.cs) | Stable owned definitions and identity checks |
| [BlueprintManifest.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/BlueprintManifest.cs) | Current manifest handling; allocate new identities through its established process |
| [GorgonPetrification.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Summoning/GorgonPetrification.cs) | Saved recovery context, native round behavior, and avoiding duplicate buff-application events |
| [ExpandedSummoningSpecialBuilder.ControlEffects32.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.ControlEffects32.cs) | Stable saved component-name conventions |
| [ExpandedSummoningSpecialBuilder.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs) | Existing native panic/fleeing convention; do not copy generic descriptors blindly |

Inspect the current runtime documentation named by `AGENTS.md`, including the Windows autonomous-testing, native-observation and disposable-fixture procedures. Historic lab paths identify where to look, not authorization to act on any machine or protected save.

## 4. Optional Call of the Wild seams

Repository: [Holic75/KingmakerRebalance](https://github.com/Holic75/KingmakerRebalance). Inspected source: **`1332fb0db844b7863f484ca978bea2349fe49769`**. Installed mod version/profile remains an implementation-time observation.

- [ChannelEnergyEngine.cs](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/ChannelEnergyEngine.cs): `init()` constructs the relevant channel wrappers. Inspect modifier registration and `storeChannel` side effects before selecting a narrow adapter.
- [Common.cs](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Common.cs): wrapper creation and native component-construction conventions. This is a source reference, not a request to copy the helper library.
- [Extra Channel reference art](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Icons/FeatIcons/Icon_Channel_Extra.png): visual vocabulary for a channel-related emblem. The source image is not distributed in this package.

Do not label CotW reference images as verified native Owlcat sprites. Do not register the feat as a normal damaging channel merely to obtain broad automatic compatibility transformations.

## 5. Art and runtime binding

These links use the same TRE source commit as section 3:

- [Icon-art guide](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/ICON-ART-GUIDE.md) and [reference index](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/art/ICON-REFERENCE-INDEX.md).
- [Canonical icon catalog](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/assets-source/original-icons/icon-catalog.json).
- [Scoped production exporter](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/tools/icon-art/Export-IconProduction.ps1).
- [ProjectAssetIcons.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs) and [OwnedIconAssignments.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/OwnedIconAssignments.cs).
- [Rapid Reload emblem anchor](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/assets/game/icons/rapid-reload.png) and [Fire Affinity painted anchor](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/assets/game/icons/fire-affinity.png).

The package includes only its two newly generated concepts and mechanical exports/review derivatives. Exact actual prompts, reference hashes and output hashes appear in the asset provenance files. Native menu screenshots, in-game presentation validation and owner approval are not claimed.

## 6. Development process

Repository: [howardreith/development-process](https://github.com/howardreith/development-process). Inspected `main`: **`a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b`**, the foreground-development baseline.

- [Maintained operating guide](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/README.md).
- [Behavioral protocol](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/docs/GITHUB-AGENT-REVIEW-PROTOCOL.md).
- [Kingmaker project configuration](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/projects/kingmaker-gunslinger.json).
- [Controller CLI and trusted plan loader](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/agent_review.py).
- [Developer report schema](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/developer-report.v1.schema.json).
- [Single-plan example](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/plans/single.json).

The included custom plan follows the inspected schema. It is a proposal for this mission, not an enrollment, controller upgrade, altered project configuration, owner-signed approval, or fabricated review record.
