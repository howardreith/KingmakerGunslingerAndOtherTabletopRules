# DATA 0.0.142 transplant manifest

Status: inventory completed; production transplant NOT STARTED because the released
base fails its inherited catalog gate and Whiteout fails objective 32-pixel review.
Classifications below are proposed dispositions, not a claim that a file was ported.

Base: `97f0a966b3219ce0122626a492529b509e1db880` (exact released v0.0.141).
Read-only source: `38691cff652d1ed38314984c9ad74549639f3802`.
DATA-only provenance range: `5482db429bd3c4009a031aa733a3091edfa5fe5e..38691cff652d1ed38314984c9ad74549639f3802`.

All 21 DATA-range commits and every path touched by each are enumerated below.
No whole-branch merge, rebase, squash, or whole-range cherry-pick is permitted.

## Rules

- PORT: copy only the final qualified non-Summoning source after prerequisite gates.
- PORT-WITH-MANUAL-RECONCILIATION: retain the release-base file and apply only DATA hunks;
  append own compile/test/scenario entries without importing or reordering inherited entries.
- DOCUMENTATION-ONLY: historical provenance or current design; preserve its artifact scope;
  never turn historical PASS into qualification of this release candidate.
- TEST/RUNTIME-EVIDENCE-ONLY: non-Summoning checks/fixtures; port only after exact base API audit.
- DROP-PHASE2A-DEPENDENCY: do not copy later Phase 2A validators or their count pins.
- DROP-EXPANDED-SUMMONING: retain the released base bytes; never import the changed path.
- SUPERSEDED: no active Lunge source/test port; only its historical blocker stays in documentation.

## Coherent source sequence

| Original authority | Commit classification | Result |
| --- | --- | --- |
| `f1155cb158188a1264f8b53ac41a64f514fe099a` docs: reconcile weekend research with the followup mission contracts | DOCUMENTATION-ONLY | NOT PORTED |
| `3cbeb4c6766eb7a500a9fbbc58d0d1e28768d5aa` feat(items): qualify normalized firearm descriptions on phase2a | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `4b3b6ae450f071ea07a746764d339f0612b3a563` feat(vendors): qualify thematic Model D stock redistribution | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `db2ab71ee3450c12c8b7bc1de527209ed893cd0e` test(whiteout): qualify unpublished policy and native weather observation | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `d4afa40947741c2f8c7a15024375ad4618550a9b` test(whiteout): bound native weather qualification with read-only catalog | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `c092a060a5c01f963e5954d6e6f1882bd16b6114` docs: close out qualified Gunslinger followup and integration handoff | DOCUMENTATION-ONLY | NOT PORTED |
| `b28b5786d10a94f3257fafa5cf02bbd50471d182` docs: close clean-tip provenance and record manual acceptance gates | DOCUMENTATION-ONLY | NOT PORTED |
| `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e` feat(traits): qualify unpublished elemental mechanics foundations | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `6a4dc1b26350c1045171b42099ef2ac5584e303d` docs: close elemental trait foundation qualification | DOCUMENTATION-ONLY | NOT PORTED |
| `bd2dddbb743a70c59d3d8c6b5b6ae4baf1f2eeb5` docs(research): map Lunge timing blocker and exact flight carrier | DOCUMENTATION-ONLY | NOT PORTED |
| `20099a6cb250eba51dc37f3f57fbb94bc53b1e79` feat(traits): qualify unpublished Aerial Observer foundation | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `a0992df35a28d0049ad25b3f746f0d465ed5426a` fix(traits): maintain unpublished flight bonus for passive Perception | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `87e42d2252192d66a9aeab56b86b9d4d81b62c69` docs: record Lunge blocker and qualified Aerial Observer handoff | DOCUMENTATION-ONLY | NOT PORTED |
| `98f0f690a331d365a5361c3013d7034722a37ffe` docs(research): record Whiteout native fixture restoration blocker | DOCUMENTATION-ONLY | NOT PORTED |
| `5c59150a748023d8bc7195df9bfd63ca4a2079b2` docs: record Whiteout unpublished foundation blocker handoff | DOCUMENTATION-ONLY | NOT PORTED |
| `b97a0a47ca454009e892d9849d5c51aca56940c7` test(runtime): qualify disposable-process Whiteout weather fixture | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `db3725d7eb665731c3b8c238217cd39f2d0a65b8` feat(traits): add unpublished Whiteout native foundation | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `fc352c24fcbcc5f6e1c3be7b8c72bd9c21720300` docs: record qualified Whiteout continuation handoff | DOCUMENTATION-ONLY | NOT PORTED |
| `15c2c0ed16c4356311184bd6bfc198fd8023be1b` docs: finalize cumulative DATA content stack and integration forecast | DOCUMENTATION-ONLY | NOT PORTED |
| `05237b10f0a2311c0118e1866bb0fc7a1a447de0` feat(traits): add dormant elemental race-trait publication readiness | PORT-WITH-MANUAL-RECONCILIATION | NOT PORTED |
| `38691cff652d1ed38314984c9ad74549639f3802` docs: record elemental trait publication-readiness handoff | DOCUMENTATION-ONLY | NOT PORTED |

## Every commit/path disposition

### `f1155cb158188a1264f8b53ac41a64f514fe099a`

docs: reconcile weekend research with the followup mission contracts

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `assets-source/original-icons/icon-catalog.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `docs/research/CHARACTER-RACE-TRAITS-FEASIBILITY.md` | DOCUMENTATION-ONLY |
| `docs/research/MOD-ITEM-AVAILABILITY-AUDIT.csv` | DOCUMENTATION-ONLY |
| `docs/research/MOD-ITEM-AVAILABILITY-AUDIT.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-WEATHER-FEASIBILITY.md` | DOCUMENTATION-ONLY |

### `3cbeb4c6766eb7a500a9fbbc58d0d1e28768d5aa`

feat(items): qualify normalized firearm descriptions on phase2a

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/Acquisition/ProgressionWeaponCatalog.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Firearms/FirearmEnchantmentItemText.cs` | PORT |
| `src/KingmakerGunslinger/Firearms/FirearmPenetrationPresentation.cs` | PORT |
| `src/KingmakerGunslinger/Firearms/MidgameFirearmCatalog.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmDescriptions.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/FirearmItemDescriptionTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/MidgameFirearmTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `4b3b6ae450f071ea07a746764d339f0612b3a563`

feat(vendors): qualify thematic Model D stock redistribution

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `assets-source/original-icons/icon-catalog.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `docs/ICON-ART-GUIDE.md` | DOCUMENTATION-ONLY |
| `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` | DOCUMENTATION-ONLY |
| `docs/research/MOD-ITEM-AVAILABILITY-AUDIT.csv` | DOCUMENTATION-ONLY |
| `docs/research/MOD-ITEM-AVAILABILITY-AUDIT.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/Acquisition/VendorCatalogPublication.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/BokkenFirearmSupplyVendorBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/CapitalVendorBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/EasternWeaponCampaignBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearCampaignBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/OlegFirearmSupplyCleanupBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Blueprints/OlegFirearmVendorBlueprints.cs` | PORT |
| `src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ModelDVendors.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/BetterVendorsProgressionTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/ModelDVendorTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/PaperCartridgeFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/UnifiedFirearmRepairTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `tools/validate_gunslinger_fixes102.py` | PORT-WITH-MANUAL-RECONCILIATION |
| `tools/validate_sprint60.py` | PORT-WITH-MANUAL-RECONCILIATION |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `db2ab71ee3450c12c8b7bc1de527209ed893cd0e`

test(whiteout): qualify unpublished policy and native weather observation

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/ElementalRaces/WhiteoutPolicy.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutWeather.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutPolicyTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `d4afa40947741c2f8c7a15024375ad4618550a9b`

test(whiteout): bound native weather qualification with read-only catalog

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-WEATHER-FEASIBILITY.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutCatalog.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutPolicyTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `c092a060a5c01f963e5954d6e6f1882bd16b6114`

docs: close out qualified Gunslinger followup and integration handoff

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `README.md` | DOCUMENTATION-ONLY |
| `docs/EASTERN-WEAPONS-PLACEMENT-MANIFEST.md` | DOCUMENTATION-ONLY |
| `docs/ELVEN-BRANCHED-SPEAR-PLACEMENT-MANIFEST.md` | DOCUMENTATION-ONLY |
| `docs/GUNSLINGER-ACQUISITION-REBALANCE.md` | DOCUMENTATION-ONLY |
| `docs/RARE-FIREARMS-MANUAL-ACCEPTANCE.md` | DOCUMENTATION-ONLY |
| `docs/research/MOD-ITEM-AVAILABILITY-AUDIT.md` | DOCUMENTATION-ONLY |

### `b28b5786d10a94f3257fafa5cf02bbd50471d182`

docs: close clean-tip provenance and record manual acceptance gates

| Path | Classification |
| --- | --- |
| `CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md` | DOCUMENTATION-ONLY |
| `docs/RARE-FIREARMS-MANUAL-ACCEPTANCE.md` | DOCUMENTATION-ONLY |

### `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e`

feat(traits): qualify unpublished elemental mechanics foundations

| Path | Classification |
| --- | --- |
| `CODEX-ELEMENTAL-RACE-TRAIT-FOUNDATIONS-HANDOFF-2026-10-05.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/test-domain.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/StoicDignityPolicy.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedRaceTraitFoundations.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/FieryGlareFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/StoicDignityFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `6a4dc1b26350c1045171b42099ef2ac5584e303d`

docs: close elemental trait foundation qualification

| Path | Classification |
| --- | --- |
| `CODEX-ELEMENTAL-RACE-TRAIT-FOUNDATIONS-HANDOFF-2026-10-05.md` | DOCUMENTATION-ONLY |

### `bd2dddbb743a70c59d3d8c6b5b6ae4baf1f2eeb5`

docs(research): map Lunge timing blocker and exact flight carrier

| Path | Classification |
| --- | --- |
| `docs/research/AERIAL-OBSERVER-FLIGHT-CONTRACT.md` | DOCUMENTATION-ONLY |
| `docs/research/LUNGE-ENGINE-CONTRACT.md` | DOCUMENTATION-ONLY |

### `20099a6cb250eba51dc37f3f57fbb94bc53b1e79`

feat(traits): qualify unpublished Aerial Observer foundation

| Path | Classification |
| --- | --- |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/ElementalRaces/AerialObserverMechanics.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/AerialObserverPolicy.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/UnpublishedAerialObserverFoundationFactory.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedAerialObserverFoundation.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/AerialObserverFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/LungeEngineContractTests.cs` | SUPERSEDED |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `a0992df35a28d0049ad25b3f746f0d465ed5426a`

fix(traits): maintain unpublished flight bonus for passive Perception

| Path | Classification |
| --- | --- |
| `docs/research/AERIAL-OBSERVER-FLIGHT-CONTRACT.md` | DOCUMENTATION-ONLY |
| `src/KingmakerGunslinger/ElementalRaces/AerialObserverMechanics.cs` | PORT |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedAerialObserverFoundation.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/AerialObserverFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `87e42d2252192d66a9aeab56b86b9d4d81b62c69`

docs: record Lunge blocker and qualified Aerial Observer handoff

| Path | Classification |
| --- | --- |
| `CODEX-LUNGE-AERIAL-OBSERVER-FOUNDATIONS-HANDOFF-2026-10-05.md` | DOCUMENTATION-ONLY |

### `98f0f690a331d365a5361c3013d7034722a37ffe`

docs(research): record Whiteout native fixture restoration blocker

| Path | Classification |
| --- | --- |
| `docs/research/WHITEOUT-NATIVE-ATTACK-CONTRACT.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-NATIVE-WEATHER-CONTRACT.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-WEATHER-FEASIBILITY.md` | DOCUMENTATION-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutNativeContractTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `5c59150a748023d8bc7195df9bfd63ca4a2079b2`

docs: record Whiteout unpublished foundation blocker handoff

| Path | Classification |
| --- | --- |
| `CODEX-WHITEOUT-UNPUBLISHED-FOUNDATION-HANDOFF-2026-10-05.md` | DOCUMENTATION-ONLY |

### `b97a0a47ca454009e892d9849d5c51aca56940c7`

test(runtime): qualify disposable-process Whiteout weather fixture

| Path | Classification |
| --- | --- |
| `docs/research/WHITEOUT-NATIVE-WEATHER-CONTRACT.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedWhiteoutFoundation.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutContinuationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `db3725d7eb665731c3b8c238217cd39f2d0a65b8`

feat(traits): add unpublished Whiteout native foundation

| Path | Classification |
| --- | --- |
| `docs/research/WHITEOUT-NATIVE-ATTACK-CONTRACT.md` | DOCUMENTATION-ONLY |
| `scripts/RuntimeAutomation.Common.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/ElementalRaces/WhiteoutFoundationPolicy.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/WhiteoutNativeMechanics.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedWhiteoutFoundation.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutAttackFixture.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `src/KingmakerGunslinger/RuntimeTesting/WhiteoutGuardedDiagnostics.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutNativeFoundationTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutPolicyTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `fc352c24fcbcc5f6e1c3be7b8c72bd9c21720300`

docs: record qualified Whiteout continuation handoff

| Path | Classification |
| --- | --- |
| `CODEX-WHITEOUT-DISPOSABLE-PROCESS-CONTINUATION-HANDOFF-2026-10-05.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-NATIVE-ATTACK-CONTRACT.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-NATIVE-WEATHER-CONTRACT.md` | DOCUMENTATION-ONLY |
| `docs/research/WHITEOUT-WEATHER-FEASIBILITY.md` | DOCUMENTATION-ONLY |

### `15c2c0ed16c4356311184bd6bfc198fd8023be1b`

docs: finalize cumulative DATA content stack and integration forecast

| Path | Classification |
| --- | --- |
| `DATA-CONTENT-STACK-FINAL-STATUS-2026-10-06.md` | DOCUMENTATION-ONLY |
| `docs/integration/DATA-CONTENT-STACK-INTEGRATION-FORECAST-2026-10-06.md` | DOCUMENTATION-ONLY |

### `05237b10f0a2311c0118e1866bb0fc7a1a447de0`

feat(traits): add dormant elemental race-trait publication readiness

| Path | Classification |
| --- | --- |
| `DATA-CONTENT-STACK-FINAL-STATUS-2026-10-06.md` | DOCUMENTATION-ONLY |
| `assets-source/original-icons/icon-overhaul-v2/production/briefs/aerial-observer.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `assets-source/original-icons/icon-overhaul-v2/production/briefs/fiery-glare.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `assets-source/original-icons/icon-overhaul-v2/production/briefs/stoic-dignity.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `assets-source/original-icons/icon-overhaul-v2/production/briefs/whiteout.json` | PORT-WITH-MANUAL-RECONCILIATION |
| `docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-COPY-2026-10-06.md` | DOCUMENTATION-ONLY |
| `docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-PLAN-2026-10-06.json` | DOCUMENTATION-ONLY |
| `docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-READINESS-2026-10-06.md` | DOCUMENTATION-ONLY |
| `docs/integration/DATA-CONTENT-STACK-INTEGRATION-FORECAST-2026-10-06.md` | DOCUMENTATION-ONLY |
| `src/KingmakerGunslinger/ElementalRaces/DormantElementalCharacterTraitFactory.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitAssetGate.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitCatalog.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitOwnedGrant.cs` | PORT |
| `src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitPublicationTransaction.cs` | PORT |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/ElementalCharacterTraitReadinessTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/Program.cs` | PORT-WITH-MANUAL-RECONCILIATION |
| `tests/KingmakerGunslinger.DomainTests/WhiteoutPolicyTests.cs` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/inspect_elemental_character_trait_icons.py` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/test_elemental_character_trait_icon_intake.py` | TEST/RUNTIME-EVIDENCE-ONLY |
| `tools/validate_expanded_summoning_phase2a141.py` | DROP-PHASE2A-DEPENDENCY |
| `validation/static-validation.json` | PORT-WITH-MANUAL-RECONCILIATION |

### `38691cff652d1ed38314984c9ad74549639f3802`

docs: record elemental trait publication-readiness handoff

| Path | Classification |
| --- | --- |
| `CODEX-ELEMENTAL-RACE-TRAIT-PUBLICATION-READINESS-HANDOFF-2026-10-06.md` | DOCUMENTATION-ONLY |

## Entire readiness-versus-release forbidden-path inventory

These paths are outside DATA's transplant scope even when they are absent from
the narrower 21-commit DATA range. They remain byte-identical to the released base.

- DROP-EXPANDED-SUMMONING: `EXPANDED-SUMMONING-PHASE2-AUTONOMOUS-STATE.md`.
- DROP-EXPANDED-SUMMONING: `EXPANDED-SUMMONING-PHASE2-EVIDENCE-INDEX.md`.
- DROP-EXPANDED-SUMMONING: `EXPANDED-SUMMONING-PHASE2-IMPLEMENTATION-REPORT.md`.
- DROP-EXPANDED-SUMMONING: `EXPANDED-SUMMONING-PHASE2-INVENTORY-RECONCILIATION.md`.
- DROP-EXPANDED-SUMMONING: `EXPANDED-SUMMONING-PHASE2-JOURNAL.md`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-icons/expanded-summoning/icon-manifest.json`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-icons/expanded-summoning/sources/shadow-mastiff.png`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint12-quadrupeds/SOURCE.md`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint12-quadrupeds/generate_sprint12_quadrupeds.py`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint13-creatures/SOURCE.md`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint13-creatures/generate_sprint13_creatures.py`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint13-creatures/paint_sprint13_albedo.py`.
- DROP-EXPANDED-SUMMONING: `assets-source/original-models/sprint13-creatures/render_sprint13_review.py`.
- DROP-EXPANDED-SUMMONING: `assets/game/icons/expanded-summoning/icon-manifest.json`.
- DROP-EXPANDED-SUMMONING: `assets/game/icons/expanded-summoning/shadow-mastiff.png`.
- DROP-EXPANDED-SUMMONING: `assets/sprint12-quadrupeds/hyena-mesh.json`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/poisonous-frog-albedo.png`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/poisonous-frog-mesh.json`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/shadow-mastiff-albedo.png`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/shadow-mastiff-mesh.json`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/wolverine-albedo.png`.
- DROP-EXPANDED-SUMMONING: `assets/sprint13-creatures/wolverine-mesh.json`.
- DROP-EXPANDED-SUMMONING: `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`.
- DROP-EXPANDED-SUMMONING: `planning/EXPANDED-SUMMONING-ROSTER.md`.
- DROP-EXPANDED-SUMMONING: `planning/EXPANDED-SUMMONING-SPRINT12-DISEASE-LIFETIME-CONTRACT.md`.
- DROP-EXPANDED-SUMMONING: `planning/EXPANDED-SUMMONING-SPRINT12-RULES-AND-DONORS.md`.
- DROP-EXPANDED-SUMMONING: `planning/EXPANDED-SUMMONING-SPRINT13-RULES-AND-DONORS.md`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeDonors.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint12.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint13.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningSprint12CombatComponents.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/ExpandedSummoningSprint13CombatComponents.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/SummonInjuryDiseasePolicy.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/SummonRagePolicy.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/SummonShadowMastiffPolicy.cs`.
- DROP-EXPANDED-SUMMONING: `src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs`.

## Required dependency audit before any later production port

No production file has been copied and no generic dependency has been introduced.
A complete type/member dependency audit has therefore not been claimed.
For every proposed PORT file, enumerate references absent in v0.0.141 and classify
EXISTING-0.0.141-EQUIVALENT, GENERIC-SAFE-INFRASTRUCTURE,
POST-RELEASE-SUMMONING-ONLY, UNNECESSARY-TEST-DEPENDENCY or BLOCKER.
Only a minimal generic dependency may be introduced; no Summoning subsystem may
be copied to support DATA. Runtime fixtures must use qualified base-safe actors;
their prior PASS records do not qualify a new artifact.

## Blocked intake and protected base

The exact released base fails its untouched icon-catalog delegated hashes.
Repairing a delegated Summoning authority or importing a later manifest is
outside the immutable release-baseline boundary. No validator/hash was weakened.
The independently observed Whiteout source fails the required 32px silhouette/
attack reading. No regeneration, crop, substitute or partial-four publication
is authorized here. Both blockers are recorded in the integration handoff.
