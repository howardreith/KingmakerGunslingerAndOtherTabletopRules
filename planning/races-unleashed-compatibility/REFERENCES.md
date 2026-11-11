# Spider Summoner baseline and source references

Prepared November 9, 2026. These records ground the mission; they are not instructions to restore an old installation or repeat an old test campaign.

## 1. Repository lineage

Repository: [howardreith/KingmakerGunslingerAndOtherTabletopRules](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules).

- Reviewed cleric PR head: `e8b0237e348dea9855b071f42d6611c8e185c05c`.
- [PR #122](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/122) merged November 9, 2026, as `2f5703f7c9987bbc25699b205495d53f00101924`.
- Master observed at mission preparation: `daf87b545693fc5000e7897b3835ccb80037797f`, merging [PR #123](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/123), v0.0.181 version/release preparation.
- The inspected changes after the reviewed cleric head were release/version/validation records, not summoning gameplay changes. [Comparison](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/compare/e8b0237e348dea9855b071f42d6611c8e185c05c...daf87b545693fc5000e7897b3835ccb80037797f).

Release preparation and actual publication are distinct. Refresh the live release state before execution. Never overwrite an existing release or infer permission to publish from a merged preparation PR.

## 2. Historical compatibility evidence

The [deferred handoff](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/daf87b545693fc5000e7897b3835ccb80037797f/planning/races-unleashed-compatibility/MISSION.md) records:

- RU ID `RacesUnleashed`, recorded version `1.0.11`, DLL SHA-256 `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0`.
- Combined DATA installation with 16 enabled mods, including Call of the Wild; no standalone RU reproduction.
- Tested KMG source `efc75b8cc0b08da3220d9d1151860899d94adb99`, v0.0.180 development.
- Run `20261109T2033140995495Z-99cc4bd7d946443e95754fd706afc0fa`, **FAIL, 70 PASS / 1 FAIL**. The remaining census failure was an explicit owner-accepted exception, not a passing result.
- Ogre's non-evil Bralani effect was actually cast. Its evil Redcap branch was inspected but not cast. Tarantula appeared in a Herald menu but was not cast. Crab/Widow casts and their specific missing Herald popups were not observed in that final run.

Private evidence is under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\`; the handoff gives exact run subdirectories. Reuse immutable relevant observations where sufficient, but new gameplay/observer bytes need affected new observations. Do not commit private raw evidence or proprietary assemblies.

The [cleric status](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/daf87b545693fc5000e7897b3835ccb80037797f/planning/summon-cleric-alignment/STATUS.md) records the detailed protections and limits. The [alphabetization status](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/daf87b545693fc5000e7897b3835ccb80037797f/planning/summon-alphabetical-menus/STATUS.md) records eight late RU rows and the existing one-time reconciliation after its `LoadDictionary` postfix at priority 300.

## 3. Exact known identities

Spider Summoner feature, required and visible on the recorded rows: `bd03ca4637e74d4caefc66961093f0a0`.

The complete ability strings below are not conventional 32-character GUIDs. Do not truncate or parse them into a different identity format.

| Original SM row | Full ability identity | Intended unit |
| --- | --- | --- |
| Giant Crab Spider I | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterIGiantCrabSpider` | `b7e1252281ee46ea8dcd045aa1e56ee9` |
| Giant Black Widow IV | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterGiantBlackWidow` | `9b1fda47e300491ab44011be9f8ef755` |
| Ogre Spider V | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterOgreSpider` | `4a205de6d159483d99eba5aa05dd8f48` |
| Giant Tarantula VII | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterGiantTarantula` | `2ea0fc9b62634a1b99073119c02e673f` |

Discover and record the actual four SNA identities; do not synthesize them by replacing substrings in the SM IDs.

Known legacy Ogre condition: caster Evil, positive, AND. Legacy true branch unit Redcap `af8bcff074b93c84b97938e486184222`; false branch Bralani `58574e8d1d4dc464c976f396d9115b1a`. Copied native row `0964bf88b582bed41b74e79596c4f6d9`.

Known legacy template effects:

| Row | Evil branch | Other branch |
| --- | --- | --- |
| Crab Spider | Fiendish I `c707fdbe58d0c614a89a872b76777f4b` | Celestial I `83a8a909e7ad19b4fa57e306884cc8bd` |
| Black Widow | Fiendish I `c707fdbe58d0c614a89a872b76777f4b` | Celestial II `ec906031bb8766f42a37a52cf1d89836` |

These are baseline effects to recognize and normalize, not an instruction to preserve the old lack of neutral choice or its donor-dependent template strength.

## 4. Main implementation references

All paths below are under `src/KingmakerGunslinger/` in the current repository. Inspect current definitions before editing; names identify relevant existing mechanisms, not a mandated file-count or class design.

| Area | Relevant files |
| --- | --- |
| Foreign contracts and cache | `Summoning/SummonForeignRowContracts.cs`, `Summoning/SummonClericAlignmentGate.cs` |
| Shared alignment policy | `Summoning/SummonClericAlignmentPolicy.cs` |
| Conversion provenance | `Summoning/DruidSummonConversionPolicy.cs`, `Summoning/DruidSummonConversionVariantPatch.cs` |
| Publication and reconciliation | `Blueprints/ExpandedSummoningPublication.cs`, `Summoning/AlternateSummoningCoordinator.cs` |
| Clone, recursive spawn replacement, dynamic templates | `Blueprints/ExpandedSummoningAbilityBuilder.cs` |
| Template selection and HD bands | `Summoning/SummonTemplateSelectionPolicy.cs`, `Summoning/SummonTemplateBandPolicy.cs`, `Summoning/ContextActionApplySummonTemplateByCaster.cs`, `Summoning/SummonAlignmentRuntimePolicy.cs` |
| Neutral toggle | `Blueprints/ExpandedSummoningAlignmentModeBlueprints.cs` |
| Quantity generation and ordinary Giant Spider | `Summoning/ExpandedSummoningCatalog.cs` |
| Existing web and poison | `Blueprints/ExpandedSummoningSpecialBuilder.cs`, `Blueprints/ExpandedSummoningNaturalBuilder.cs` |
| Graph reader | `Blueprints/SummonSignatureReader.cs` |
| Stable registration and module boundaries | `Bootstrap/BlueprintBootstrap.cs`, current identity/registration and module-publication components |
| Existing observer | `RuntimeTesting/RuntimeTestRunner.SummonClericAlignment.cs` |
| Existing focused tests | `tests/KingmakerGunslinger.DomainTests/SummonClericAlignmentTests.cs` relative to repository root, plus relevant conversion/template/publication tests |

Known traps in the inspected implementation:

- `ClassifyDirect` treats a verified untemplated row as `Fixed(None)`. Whitelisting template actions without changing that result is incorrect.
- Foreign classification caches by row object and library. An in-place repair can leave cached legacy semantics.
- The existing post-spawn summary compresses some apply-buff properties. Verify relevant targets and flags, not just the buff GUID.
- The observer expects `verified.Count == 3` and `governedContracts.Count == 1`, and an actual Bralani from the old Ogre row. These are obsolete final expectations for the new mission.
- Ordinary Giant Spider is already SM II/SNA II. Its KMG unit is `4a3cd49e751448c8b8836485b262fdf1`; donor is `9e120b5e0ad3c794491c049aa24b9fde`.
- `ConfigureSummonWeb` is shared with Bebelith. The ordinary Giant Spider web uses a ranged touch attack, no initial save, and native held-state buff `a719abac0ea0ce346b401060754cc1c0`. Trace its actual context and break-free DC before choosing a hook.
- Native poison `094714bb08f4e1943a8e9d2384ebe573` and spider donors are reused by other creatures. They do not independently establish feat eligibility.
- A pre-cast parameter result can differ from resolving `MechanicsContext` parameters. Verify actual saves and held effects.

## 5. Rules and upstream sources

- [Races Unleashed description](https://www.nexusmods.com/pathfinderkingmaker/mods/144?tab=description): lists Spider Summoner under Drow feats. The public description is not proof of the installed action graph.
- [Races Unleashed files](https://www.nexusmods.com/pathfinderkingmaker/mods/144?tab=files): advertises 1.1.1. Reconcile against the recorded installed metadata and actual hash.
- [Official legacy PRD Spider Summoner](https://legacy.aonprd.com/advancedRaceGuide/featuredRaces/drow.html#spider-summoner): feat scope, four additions, template choice, and poison/web benefit.
- [Official legacy PRD Summon Monster](https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html): quantity progression and starred-creature spell-alignment semantics.
- [Official legacy PRD Summon Nature's Ally](https://legacy.aonprd.com/coreRulebook/spells/summonNatureSAlly.html): quantity progression and family-specific alignment semantics.
- [Official legacy PRD Cleric](https://legacy.aonprd.com/coreRulebook/classes/cleric.html): opposed caster/deity spell restrictions.

The investigation did not locate a public RU source repository. Use the authorized installed assembly and actual loaded game graphs for implementation facts; do not assume an unverified fork or a guessed repository matches the installed mod. The host connection was offline during package authoring, so this package contains code-informed requirements and historical native evidence, not a new native acceptance result.
