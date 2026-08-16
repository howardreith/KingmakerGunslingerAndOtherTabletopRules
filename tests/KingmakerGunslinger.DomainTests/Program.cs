using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Recovery;
using KingmakerGunslinger.Persistence;
using KingmakerGunslinger.Scatter;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Grit;
using KingmakerGunslinger.Feats;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static readonly TestCase[] Cases =
        {
            Case("brown-fur.progression-normal", BrownFurContractTests.NormalProgressionIsExact),
            Case("brown-fur.progression-balance", BrownFurContractTests.BalanceProgressionIsExact),
            Case("brown-fur.progression-unknown", BrownFurContractTests.UnknownProgressionsFailClosed),
            Case("brown-fur.contract-absent", BrownFurContractTests.AbsentCotwIsUnavailable),
            Case("brown-fur.contract-compatible", BrownFurContractTests.CompleteContractIsCompatible),
            Case("brown-fur.contract-required-surfaces", BrownFurContractTests.EveryRequiredSurfaceFailsClosed),
            Case("brown-fur.contract-ambiguous-progression", BrownFurContractTests.AmbiguousProgressionBlocksContract),
            Case("brown-fur.contract-idempotent", BrownFurContractTests.ContractPolicyIsIdempotent),
            Case("brown-fur.runtime-resolver-contract", BrownFurContractTests.RuntimeResolverUsesExactOptionalContract),
            Case("brown-fur.inventory-classification-generic", BrownFurInventoryClassificationTests.GenericContractsAreDeterministic),
            Case("brown-fur.inventory-classification-carriers", BrownFurInventoryClassificationTests.EveryBonusCarrierFamilyIsSupported),
            Case("brown-fur.inventory-classification-durations", BrownFurInventoryClassificationTests.NamedAndNoOpDurationsAreExact),
            Case("brown-fur.inventory-classification-fail-closed", BrownFurInventoryClassificationTests.UnknownStructuresFailClosed),
            Case("brown-fur.identities-permanent", BrownFurIdentityTests.PermanentLedgerIsExact),
            Case("brown-fur.identities-manifest", BrownFurIdentityTests.ManifestActiveEntriesMatchLedger),
            Case("brown-fur.intent-empty", BrownFurPlayerIntentTests.EmptyStateIsUnrequested),
            Case("brown-fur.intent-six-stats", BrownFurPlayerIntentTests.EachAbilityScoreIsExclusive),
            Case("brown-fur.intent-ambiguous", BrownFurPlayerIntentTests.AmbiguousScoresFailClosed),
            Case("brown-fur.intent-share-capstone", BrownFurPlayerIntentTests.ShareAndCapstoneOwnershipAreIndependent),
            Case("brown-fur.intent-feature-ownership", BrownFurPlayerIntentTests.PendingMarkersRequireTheirFeatures),
            Case("brown-fur.intent-runtime-facts", BrownFurContractTests.PlayerIntentRuntimeUsesExactFacts),
            Case("brown-fur.archetype-normal", BrownFurArchetypePlanTests.NormalProgressionIsExact),
            Case("brown-fur.archetype-balance", BrownFurArchetypePlanTests.BalanceProgressionIsExact),
            Case("brown-fur.archetype-unknown", BrownFurArchetypePlanTests.UnknownProgressionCannotBuildShell),
            Case("brown-fur.archetype-blueprint-contract", BrownFurArchetypePlanTests.BlueprintBuilderRetainsPlayerContract),
            Case("brown-fur.presentation-human-review-repair", BrownFurArchetypePlanTests.HumanReviewPresentationRepairIsExplicit),
            Case("brown-fur.targeting-pre-command-repair", BrownFurArchetypePlanTests.PreCommandTargetingRepairIsScoped),
            Case("brown-fur.publication-idempotent", BrownFurPublicationTransactionTests.PublishesAdditivelyAndIdempotently),
            Case("brown-fur.publication-failure-rollback", BrownFurPublicationTransactionTests.FailureRollsBackEveryOwnedSurface),
            Case("brown-fur.publication-preserve-later", BrownFurPublicationTransactionTests.RollbackPreservesProvenLaterAppend),
            Case("brown-fur.publication-ambiguous-rollback", BrownFurPublicationTransactionTests.RollbackRefusesAmbiguousMutation),
            Case("brown-fur.publication-guid-conflict", BrownFurPublicationTransactionTests.ConflictingGuidRollsBackRegisteredIdentities),
            Case("brown-fur.cast-powerful-six-stats", BrownFurCastTests.PowerfulChangeSupportsEachAbilityScore),
            Case("brown-fur.cast-powerful-invalid-stat", BrownFurCastTests.PowerfulChangeRejectsInvalidStatWithoutCost),
            Case("brown-fur.cast-powerful-arcanist-slot", BrownFurCastTests.PowerfulChangeRequiresArcanistSpellSlot),
            Case("brown-fur.cast-share-willing-creature", BrownFurCastTests.ShareTransmutationUsesWillingCreatureContract),
            Case("brown-fur.cast-share-willingness-delivery", BrownFurCastTests.ShareWillingnessAndDeliveryAreExact),
            Case("brown-fur.cast-share-relationship-policy", BrownFurCastTests.ShareRelationshipFactsFailClosed),
            Case("brown-fur.cast-powerful-capstone", BrownFurCastTests.PowerfulChangeIncreaseUsesCapstoneValue),
            Case("brown-fur.cast-combined-cost", BrownFurCastTests.CombinedUseCostsExactlyTwo),
            Case("brown-fur.cast-supremacy-duration", BrownFurCastTests.SupremacyExtendsOnlyEligibleDuration),
            Case("brown-fur.cast-transaction-debit", BrownFurCastTests.TransactionDebitsExactlyOnce),
            Case("brown-fur.cast-cancel-interrupt", BrownFurCastTests.CancellationAndInterruptionAreAtomic),
            Case("brown-fur.cast-lifecycle-exact", BrownFurCastTests.CastLifecycleIsExact),
            Case("brown-fur.cast-lifecycle-cancel", BrownFurCastTests.CastLifecycleCancellationIsAtomic),
            Case("brown-fur.cast-lifecycle-concurrent", BrownFurCastTests.CastLifecyclesAreIsolated),
            Case("brown-fur.cast-lifecycle-release", BrownFurCastTests.CastLifecycleReleasesExactlyOnce),
            Case("brown-fur.cast-reservation-concurrent", BrownFurCastTests.ReservoirReservationsAreAtomic),
            Case("brown-fur.cast-reservation-terminal", BrownFurCastTests.ReservoirReservationsReleaseOnEveryTerminalCommit),
            Case("brown-fur.cast-commit-coordinator", BrownFurCastTests.CommitCoordinatorIsAtomic),
            Case("brown-fur.cast-commit-rejection", BrownFurCastTests.CommitCoordinatorRejectionCleansUp),
            Case("brown-fur.cast-commit-no-process", BrownFurCastTests.CommitCoordinatorCompletesWithoutProcess),
            Case("brown-fur.cast-reservoir-exact", BrownFurCastTests.ReservoirDebitIsExact),
            Case("brown-fur.cast-reservoir-reject", BrownFurCastTests.ReservoirDebitRejectsBeforeSpend),
            Case("brown-fur.cast-reservoir-rollback", BrownFurCastTests.ReservoirDebitRollsBackAnomaly),
            Case("brown-fur.modifier-descriptor", BrownFurCastTests.ModifierAdjustmentPreservesDescriptor),
            Case("brown-fur.modifier-fail-closed", BrownFurCastTests.ModifierAdjustmentFailsClosed),
            Case("brown-fur.modifier-exactly-once", BrownFurCastTests.ModifierAdjustmentIsExactlyOnce),
            Case("brown-fur.modifier-concurrent", BrownFurCastTests.ModifierTransactionsAreIsolated),
            Case("brown-fur.modifier-persistence-exact", BrownFurCastTests.ModifierPersistenceMatchesExactly),
            Case("brown-fur.modifier-persistence-fail-closed", BrownFurCastTests.ModifierPersistenceFailsClosed),
            Case("brown-fur.modifier-ordinary-recast", BrownFurCastTests.OrdinaryRecastRestoresOriginalTypedValue),
            Case("brown-fur.share-scope-exact", BrownFurCastTests.ShareTargetingScopeIsExact),
            Case("brown-fur.share-scope-concurrent", BrownFurCastTests.ShareTargetingScopesAreIsolated),
            Case("brown-fur.supremacy-scope-exact", BrownFurCastTests.SupremacyScopeIsExactAndNonStacking),
            Case("brown-fur.supremacy-scope-concurrent", BrownFurCastTests.SupremacyScopesAreIsolated),
            Case("brown-fur.bonus-plan-static", BrownFurCastTests.StaticBonusAdapterPlanIsExact),
            Case("brown-fur.bonus-plan-polymorph", BrownFurCastTests.PolymorphBonusAdapterPlanIsExact),
            Case("brown-fur.bonus-plan-size", BrownFurCastTests.SizeBonusAdapterPlanIsExact),
            Case("brown-fur.bonus-plan-fail-closed", BrownFurCastTests.BonusAdapterPlanFailsClosed),
            Case("eastern-weapons.investigation-guard", EasternWeaponsInvestigationTests.EvidenceAndObserverRemainInvestigationOnly),
            Case("elven-branched-spear.investigation-guard", ElvenBranchedSpearInvestigationTests.EvidenceReportAndObserverAreGuarded),
            Case("elven-branched-spear.foundation-catalog", ElvenBranchedSpearCatalogTests.LockedProfileAndFoundationCatalogAreExact),
            Case("elven-branched-spear.foundation-source-contracts", ElvenBranchedSpearCatalogTests.FoundationSourceContractsAreExact),
            Case("elven-branched-spear.named-catalog-and-triggers", ElvenBranchedSpearCatalogTests.NamedCatalogAndTriggerPoliciesAreExact),
            Case("elven-branched-spear.named-blueprint-contracts", ElvenBranchedSpearCatalogTests.NamedBlueprintSourceContractsAreExact),
            Case("elven-branched-spear.campaign-publication", ElvenBranchedSpearCatalogTests.CampaignPublicationContractsAreExact),
            Case("elven-branched-spear.original-asset-pipeline", ElvenBranchedSpearCatalogTests.OriginalAssetPipelineContractsAreExact),
            Case("elven-branched-spear.runtime-combat-scenario-contracts", ElvenBranchedSpearCatalogTests.RuntimeCombatScenarioContractsAreExact),
            Case("elven-branched-spear.development-grant-contracts", ElvenBranchedSpearCatalogTests.DevelopmentGrantContractsAreExact),
            Case("elven-branched-spear.working-save-persistence-contracts", ElvenBranchedSpearCatalogTests.WorkingSavePersistenceContractsAreExact),
            Case("elven-branched-spear.release-identity", ElvenBranchedSpearCatalogTests.ReleaseIdentityIsSpearSpecific),
            Case("elven-branched-spear.category-display", ElvenBranchedSpearCatalogTests.CategoryDisplayNeverLeaksRawIdentity),
            Case("elven-branched-spear.selector-presentation", ElvenBranchedSpearCatalogTests.SelectorPresentationMatchesNativePolicies),
            Case("elven-branched-spear.exotic-proficiency-presentation", ElvenBranchedSpearCatalogTests.ExoticProficiencyPresentationIsNativeOrdered),
            Case("elven-branched-spear.btsl-publication", ElvenBranchedSpearCatalogTests.BeneathStolenLandsPublicationContractsAreExact),
            Case("weapon-visual-audit.spear-runtime-mapping", WeaponVisualMappingAuditTests.RuntimeCatalogMatchesApprovedSpearVariants),
            Case("weapon-visual-audit.eastern-runtime-mapping", WeaponVisualMappingAuditTests.RuntimeCatalogMatchesApprovedEasternVariants),
            Case("weapon-visual-audit.firearm-runtime-mapping", WeaponVisualMappingAuditTests.RuntimeCatalogMatchesApprovedFirearmVariants),
            Case("firearm-fit.generated-candidates", FirearmFitAssetTests.GeneratedCandidatesAreExactAndReproducible),
            Case("firearm-fit.generated-pistol-variants", FirearmFitAssetTests.GeneratedPistolVariantsAreExactAndReproducible),
            Case("firearm-fit.pistol-item-runtime-contract", FirearmFitAssetTests.PistolItemVariantRuntimeContractIsExact),
            Case("firearm-fit.marker-import-fails-closed", FirearmFitAssetTests.MarkerImporterFailsClosed),
            Case("firearm-fit.diagnostic-runtime-boundary", FirearmFitAssetTests.DiagnosticRuntimeBoundaryIsExact),
            Case("firearm-fit.production-binding-frozen", FirearmFitAssetTests.ProductionBindingRemainsFrozen),
            Case("repair-runtime.immutable-reuse", RepairRuntimePolicyTests.ImmutableArtifactReuseIsFailClosed),
            Case("repair-runtime.boundary-generic", RepairRuntimePolicyTests.BoundaryMatrixIsGeneric),
            Case("expanded-summoning.roster-and-placements", ExpandedSummoningCatalogTests.FrozenRosterAndPlacementCounts),
            Case("expanded-summoning.quantity-same-kind", ExpandedSummoningCatalogTests.QuantityRulesAreExactAndSameKind),
            Case("expanded-summoning.alignment-policies", ExpandedSummoningCatalogTests.AlignmentPoliciesAreFamilyScoped),
            Case("expanded-summoning.catalog-guards", ExpandedSummoningCatalogTests.CatalogGuardsInvalidSpecs),
            Case("expanded-summoning.merge-idempotent", SummonPublicationPolicyTests.MergePreservesOrderAndIsIdempotent),
            Case("expanded-summoning.merge-conflicts", SummonPublicationPolicyTests.MergeDeduplicatesExistingAndRejectsConflicts),
            Case("expanded-summoning.native-reconciliation", SummonPublicationPolicyTests.NativeDuplicateCatalogIsExact),
            Case("expanded-summoning.display-order", SummonPublicationPolicyTests.DisplayOrderGroupsSinglesBeforeQuantities),
            Case("expanded-summoning.icon-catalog", SummonPublicationPolicyTests.IconCatalogCoversEveryCreature),
            Case("expanded-summoning.icon-manifest-files", ExpandedSummoningPresentationTests.OriginalIconManifestMatchesFiles),
            Case("expanded-summoning.icon-exclusive-source", ExpandedSummoningPresentationTests.OriginalIconSourceContractIsExclusive),
            Case("expanded-summoning.icon-cache-package", ExpandedSummoningPresentationTests.RuntimeIconCacheAndPackagePathsAreExact),
            Case("expanded-summoning.sna-naming-scale", ExpandedSummoningPresentationTests.SnaWrappersNamingAndScaleAreExact),
            Case("expanded-summoning.player-path-family-parents", ExpandedSummoningPresentationTests.PlayerPathHarnessUsesFamilyParentOffsets),
            Case("expanded-summoning.transaction-rollback", SummonPublicationPolicyTests.TransactionRollsBackExactReferences),
            Case("expanded-summoning.transaction-unsafe-rollback", SummonPublicationPolicyTests.TransactionRefusesUnsafeRollback),
            Case("expanded-summoning.sanitizer-forbidden", SummonUnitSanitizationPolicyTests.RemovesEveryForbiddenCampaignSurface),
            Case("expanded-summoning.sanitizer-replacements", SummonUnitSanitizationPolicyTests.RetainsCombatAndRequiresSafeReplacement),
            Case("expanded-summoning.sanitizer-malformed", SummonUnitSanitizationPolicyTests.RejectsMalformedInventories),
            Case("expanded-summoning.sanitizer-runtime-members", SummonUnitSanitizationPolicyTests.RuntimeMemberNamesFailClosedOnProhibitedPowers),
            Case("expanded-summoning.identities-exact", ExpandedSummoningIdentityCatalogTests.FoundationLedgerIsExactAndDeterministic),
            Case("expanded-summoning.natural-low-tier-profiles", ExpandedSummoningIdentityCatalogTests.LowTierNaturalProfilesAreExact),
            Case("expanded-summoning.natural-tier-three-four-profiles", ExpandedSummoningIdentityCatalogTests.TierThreeFourNaturalProfilesAreExact),
            Case("expanded-summoning.natural-tier-five-seven-profiles", ExpandedSummoningIdentityCatalogTests.TierFiveSevenNaturalProfilesAreExact),
            Case("expanded-summoning.identities-template-scope", ExpandedSummoningIdentityCatalogTests.TemplateExecutionsAreFamilyScoped),
            Case("expanded-summoning.template-hd-bands", ExpandedSummoningIdentityCatalogTests.TemplateHitDiceBandsAreExact),
            Case("expanded-summoning.template-smite", ExpandedSummoningIdentityCatalogTests.TemplateSmitePolicyIsBoundedAndOpposed),
            Case("expanded-summoning.runtime-alignment", ExpandedSummoningIdentityCatalogTests.RuntimeAlignmentPolicyIsFamilyScopedAndExact),
            Case("expanded-summoning.identities-logical-coverage", ExpandedSummoningIdentityCatalogTests.SymbolsEncodeEveryLogicalPlacement),
            Case("expanded-summoning.donors-exact", ExpandedSummoningIdentityCatalogTests.DonorsCoverEveryFrozenCreature),
            Case("expanded-summoning.special-profiles", ExpandedSummoningIdentityCatalogTests.NativeReuseAndLanternProfilesAreExact),
            Case("expanded-summoning.ability-builder", ExpandedSummoningIdentityCatalogTests.AbilityBuilderPreservesNativeGraphContracts),
            Case("expanded-summoning.runtime-publication", ExpandedSummoningIdentityCatalogTests.RuntimePublicationIsAdditiveAndTransactional),
            Case("expanded-summoning.unit-component-isolation", ExpandedSummoningIdentityCatalogTests.RuntimeUnitComponentsAreReferenceIsolated),
            Case("expanded-summoning.template-blueprints", ExpandedSummoningIdentityCatalogTests.TemplateBlueprintsUseNativeBoundedMechanics),
            Case("expanded-summoning.player-path-harness", ExpandedSummoningIdentityCatalogTests.PlayerPathHarnessUsesRealSpellbookParents),
            Case("feature-settings.defaults-and-legacy", FeatureModuleSettingsTests.DefaultsAndLegacyAreOn),
            Case("feature-settings.one-hundred-twenty-eight-combinations", FeatureModuleSettingsTests.OneHundredTwentyEightCombinationsRoundTrip),
            Case("feature-settings.malformed-recovery", FeatureModuleSettingsTests.MalformedRecoversAndQuarantines),
            Case("feature-settings.active-snapshot", FeatureModuleSettingsTests.ActiveSnapshotIsImmutable),
            Case("feature-settings.value-semantics", FeatureModuleSettingsTests.ValueSemanticsIncludeBrownFur),
            Case("feature-settings.brown-fur-status", FeatureModuleSettingsTests.BrownFurStatusDistinguishesIntentAndDependency),
            Case("feature-settings.matrix-counts", FeatureModuleSettingsTests.SevenModuleMatrixCountsAreExact),
            Case("feature-modules.publication-plans", FeatureModuleSettingsTests.PublicationPlansAreIndependent),
            Case("feature-modules.runtime-matrix", FeatureModuleSettingsTests.RuntimeMatrixUsesAuthoritativeSevenModuleCatalog),
            Case("eastern-weapons.locked-category-profiles", EasternWeaponFoundationTests.LockedCategoryProfilesAreExact),
            Case("eastern-weapons.generic-catalog", EasternWeaponFoundationTests.GenericCatalogIsExact),
            Case("eastern-weapons.category-collision", EasternWeaponFoundationTests.RegistryFailsClosedOnCollisions),
            Case("eastern-weapons.generic-blueprint-source", EasternWeaponFoundationTests.GenericBlueprintSourceContractsAreExact),
            Case("eastern-weapons.proficiency-selectors-groups", EasternWeaponFoundationTests.ProficiencySelectorAndGroupContractsAreExact),
            Case("eastern-weapons.named-native-catalog", EasternWeaponFoundationTests.NamedNativeCatalogIsExact),
            Case("eastern-weapons.named-bespoke-effects", EasternWeaponFoundationTests.NamedBespokeEffectContractsAreExact),
            Case("eastern-weapons.campaign-publication", EasternWeaponFoundationTests.CampaignPublicationIsExactAndTransactional),
            Case("eastern-weapons.development-controls", EasternWeaponFoundationTests.DevelopmentControlsAreExactAndInventoryOnly),
            Case("eastern-weapons.working-save-persistence", EasternWeaponFoundationTests.WorkingSavePersistenceContractsAreExact),
            Case("eastern-weapons.original-asset-pipeline", EasternWeaponFoundationTests.OriginalAssetPipelineIsExactAndFailSafe),
            Case("eastern-weapons.combat-scenario-contract", EasternWeaponFoundationTests.CombatScenarioUsesLiveRulesAndCleansUp),
            Case("eastern-weapons.arms-armor-grip-bridge", EasternWeaponFoundationTests.ArmsArmorGripBridgeIsExactAndOptional),
            Case("eastern-weapons.cotw-focused-weapon", EasternWeaponFoundationTests.CallOfTheWildFocusedWeaponIsExactAndOptional),
            Case("shield-other.damage-split", ShieldOtherPolicyTests.DamageSplitBoundariesAndConservation),
            Case("shield-other.damage-guards", ShieldOtherPolicyTests.DamageSplitGuards),
            Case("shield-other.link-validity", ShieldOtherPolicyTests.LinkValidityMatrix),
            Case("shield-other.close-range", ShieldOtherPolicyTests.CloseRangeScaling),
            Case("shield-other.blueprint-contract", ShieldOtherPolicyTests.BlueprintIdentityAndContractSource),
            Case("shield-other.spell-list-policy", ShieldOtherPolicyTests.SpellListMergeAndRollbackPolicy),
            Case("shield-other.base-publication", ShieldOtherPolicyTests.BasePublicationSourceContract),
            Case("shield-other.optional-publication", ShieldOtherPolicyTests.OptionalPublicationSourceContract),
            Case("shield-other.link-component", ShieldOtherPolicyTests.LinkComponentSourceContract),
            Case("shield-other.damage-runtime", ShieldOtherPolicyTests.DamageRuntimeSourceContract),
            Case("shield-other.runtime-module-request", ShieldOtherPolicyTests.RuntimeModuleRequestSourceContract),
            Case("acadamae.eligibility-matrix", AcadamaeCordPolicyTests.AcadamaeEligibilityMatrix),
            Case("acadamae.multi-round-and-dc", AcadamaeCordPolicyTests.AcadamaeMultiRoundAndDc),
            Case("acadamae.prerequisite-matrix", AcadamaeCordPolicyTests.AcadamaePrerequisiteMatrix),
            Case("acadamae.native-identity-contracts", AcadamaeCordPolicyTests.AcadamaeNativeIdentityContracts),
            Case("acadamae.invocation-correlation", AcadamaeCordPolicyTests.AcadamaeInvocationCorrelation),
            Case("acadamae.mode-fatigue-source-contracts", AcadamaeCordPolicyTests.AcadamaeModeAndFatigueSourceContracts),
            Case("acadamae.mode-identity-contracts", AcadamaeCordPolicyTests.AcadamaeModeIdentityContracts),
            Case("cord.project-icon-contract", AcadamaeCordPolicyTests.CordProjectIconContract),
            Case("cord.fatigue-and-exhaustion", AcadamaeCordPolicyTests.CordFatigueAndExhaustion),
            Case("cord.damage-boundaries", AcadamaeCordPolicyTests.CordDamageBoundaries),
            Case("cord.native-condition-source-contract", AcadamaeCordPolicyTests.CordNativeConditionSourceContract),
            Case("paper-foundation.profiles-exact", PaperCartridgeFoundationTests.ProfilesAreExact),
            Case("paper-foundation.compatibility-definition-driven", PaperCartridgeFoundationTests.CompatibilityIsDefinitionDriven),
            Case("paper-foundation.unknown-fails-closed", PaperCartridgeFoundationTests.UnknownIdentityFailsClosed),
            Case("paper-foundation.tokens-round-trip", PaperCartridgeFoundationTests.PaperTokensRoundTrip),
            Case("paper-foundation.old-tokens-exact", PaperCartridgeFoundationTests.OldTokensRemainExact),
            Case("paper-foundation.item-source-contract", PaperCartridgeFoundationTests.BlueprintSourceContract),
            Case("paper-reload.action-matrix", PaperCartridgeFoundationTests.ActionMatrix),
            Case("paper-reload.no-fallback", PaperCartridgeFoundationTests.NoFallback),
            Case("paper-reload.atomic-sources", PaperCartridgeFoundationTests.AtomicSources),
            Case("paper-reload.transaction-success", PaperCartridgeFoundationTests.PaperReloadTransactionSuccess),
            Case("paper-reload.state-failure-rollback", PaperCartridgeFoundationTests.PaperStateFailureRestoresInventory),
            Case("paper-reload.mixed-identity-rejected", PaperCartridgeFoundationTests.MixedIdentityRejected),
            Case("paper-mode.source-contract", PaperCartridgeFoundationTests.ModeSourceContract),
            Case("paper-lightning.dynamic-actions", PaperCartridgeFoundationTests.LightningReloadDynamicActions),
            Case("paper-full-attack.reload-branches", PaperCartridgeFoundationTests.FullAttackReloadBranches),
            Case("paper-misfire.authoritative-order", PaperCartridgeFoundationTests.MisfireAuthoritativeOrder),
            Case("paper-misfire.central-consumers", PaperCartridgeFoundationTests.MisfireCentralConsumers),
            Case("paper-crafting.shared-transaction-contract", PaperCartridgeFoundationTests.CraftingSharedTransactionContract),
            Case("paper-vendors.normalization-contract", PaperCartridgeFoundationTests.VendorNormalizationContract),
            Case("seeking.exact-failed-concealment", RareFirearmSeekingTests.ExactFailedConcealmentBypasses),
            Case("seeking.native-success", RareFirearmSeekingTests.NativeSuccessRemainsNative),
            Case("seeking.wrong-check", RareFirearmSeekingTests.WrongCheckFailsClosed),
            Case("seeking.wrong-item", RareFirearmSeekingTests.WrongItemFailsClosed),
            Case("seeking.missing-context", RareFirearmSeekingTests.MissingContextFailsClosed),
            Case("archetype-handedness.catalog-exact", ArchetypeFoundationTests.HandednessCatalogExact),
            Case("archetype-handedness.family-matching", ArchetypeFoundationTests.HandednessFamilyMatching),
            Case("archetype-handedness.unknown-fails-closed", ArchetypeFoundationTests.HandednessUnknownFailsClosed),
            Case("archetype-proficiency.full-permits-all", ArchetypeFoundationTests.FullProficiencyPermitsAll),
            Case("archetype-proficiency.one-handed-exact", ArchetypeFoundationTests.OneHandedProficiencyExact),
            Case("archetype-proficiency.two-handed-exact", ArchetypeFoundationTests.TwoHandedProficiencyExact),
            Case("archetype-proficiency.absent-and-marker-fail-closed", ArchetypeFoundationTests.ProficiencyFailsClosed),
            Case("archetype-proficiency.action-access", ArchetypeFoundationTests.ScopedActionAccess),
            Case("firearm-proficiency.publication-policy", FirearmProficiencyPublicationTests.PublicationPolicyIsCompatibilityOnly),
            Case("firearm-proficiency.stable-identities", FirearmProficiencyPublicationTests.StableIdentitiesRemainExact),
            Case("firearm-proficiency.runtime-contract", FirearmProficiencyPublicationTests.RuntimeScenariosExerciseRealOwnersAndArchetypes),
            Case("firearm-proficiency.respec-reconciliation", FirearmProficiencyPublicationTests.ScopedRespecReconciliationIsLegacySafe),
            Case("weapon-visual-audit.complete-identities", WeaponVisualMappingAuditTests.CoversEveryActiveCustomWeaponIdentity),
            Case("weapon-visual-audit.required-contracts", WeaponVisualMappingAuditTests.RecordsEveryRequiredVisualContract),
            Case("weapon-visual-audit.variant-vocabulary", WeaponVisualMappingAuditTests.VariantVocabularyIsBoundedAndFamilySafe),
            Case("archetype-starter.precedence", ArchetypeFoundationTests.StartingFirearmPrecedence),
            Case("archetype-starter.exact-kind", ArchetypeFoundationTests.StartingFirearmExactKind),
            Case("archetype-training.thresholds-and-families", ArchetypeFoundationTests.TrainingThresholdsAndFamilies),
            Case("archetype-training.overlap-and-negative-dex", ArchetypeFoundationTests.TrainingOverlapAndNegativeDexterity),
            Case("archetype-reload.fast-musket-matrix", ArchetypeFoundationTests.FastMusketReloadMatrix),
            Case("archetype-range.effective-boundaries", ArchetypeFoundationTests.EffectiveRangeContextBoundaries),
            Case("archetype-pistolero.up-close-policy", ArchetypeFoundationTests.UpCloseAndDeadlyPolicyContract),
            Case("archetype-pistolero.twin-shot-policy", ArchetypeFoundationTests.TwinShotPolicyContract),
            Case("archetype-musket-master.native-starter-skeleton", MusketMasterStarterSkeleton),
            Case("archetype-pistolero.replacement-skeleton", PistoleroReplacementSkeleton),
            Case("audio.catalog-exact", FirearmAudioTests.CatalogExact),
            Case("audio.manifest-validation", FirearmAudioTests.ManifestValidation),
            Case("audio.staging-lifecycle", FirearmAudioTests.StagingLifecycle),
            Case("audio.state-machine", FirearmAudioTests.StateMachineLifecycle),
            Case("audio.discharge-route-shape", FirearmAudioDischargeRouteShape),
            Case("vendor-publication.append", VendorPublicationAppendsExactReferences),
            Case("vendor-publication.idempotent", VendorPublicationIsIdempotent),
            Case("vendor-publication.ambiguity", VendorPublicationRejectsAmbiguity),
            Case("vendor-publication.rollback", VendorPublicationRollbackRestoresNativeReferences),
            Case("battered.owner-normal", Sprint83Tests.OwnerNormal),
            Case("battered.owner-broken", Sprint83Tests.OwnerBroken),
            Case("battered.nonowner-normal", Sprint83Tests.NonOwnerNormal),
            Case("battered.nonowner-broken", Sprint83Tests.NonOwnerBroken),
            Case("battered.nonowner-wrecked", Sprint83Tests.NonOwnerWrecked),
            Case("battered.ordinary", Sprint83Tests.OrdinaryFirearm),
            Case("battered.invalid-inputs", Sprint83Tests.InvalidInputs),
            Case("battered-ownership.bind", Sprint83Tests.OwnershipBind),
            Case("battered-ownership.idempotent", Sprint83Tests.OwnershipIdempotent),
            Case("battered-ownership.conflict", Sprint83Tests.OwnershipConflict),
            Case("battered-ownership.isolation", Sprint83Tests.OwnershipIsolation),
            Case("battered-ownership.snapshot", Sprint83Tests.OwnershipSnapshot),
            Case("battered-ownership.invalid", Sprint83Tests.OwnershipInvalid),
            Case("battered-ownership.remove", Sprint83Tests.OwnershipRemove),
            Case("battered.discharge-effective-wrecked", Sprint83Tests.EffectiveWreckedDischarge),
            Case("battered.misfire-effective-broken", Sprint83Tests.EffectiveBrokenMisfire),
            Case("battered.misfire-effective-advanced", Sprint83Tests.EffectiveBrokenAdvancedMisfire),
            Case("class.chassis-constants", ClassChassisConstants),
            Case("class.chassis-exact-rows", ClassChassisExactRows),
            Case("class.chassis-complete-monotonic", ClassChassisCompleteMonotonic),
            Case("class.chassis-save-formulas", ClassChassisSaveFormulas),
            Case("class.chassis-invalid-level", ClassChassisInvalidLevel),
            Case("class.chassis-level-value", ClassChassisLevelValueSemantics),
            Case("grit.maximum-wisdom-minimum", GritMaximumWisdomMinimum),
            Case("grit.daily-reset-exact", GritDailyResetExact),
            Case("grit.state-invalid-bounds", GritStateRejectsInvalidBounds),
            Case("grit.reconcile-clamp-no-refill", GritReconcileClampsWithoutRefill),
            Case("grit.spend-applied", GritSpendApplied),
            Case("grit.spend-insufficient-atomic", GritSpendInsufficientAtomic),
            Case("grit.restore-applied-capped", GritRestoreAppliedAndCapped),
            Case("grit.restore-at-maximum-atomic", GritRestoreAtMaximumAtomic),
            Case("grit.duplicate-spend", GritDuplicateSpendRejected),
            Case("grit.duplicate-restore", GritDuplicateRestoreRejected),
            Case("grit.unit-gates-isolated", GritUnitGatesAreIsolated),
            Case("grit.invalid-transactions", GritInvalidTransactionsRejected),
            Case("grit.recovery-critical-eligible", GritRecoveryCriticalEligible),
            Case("grit.recovery-kill-eligible", GritRecoveryKillEligible),
            Case("grit.recovery-outcome-required", GritRecoveryOutcomeRequired),
            Case("grit.recovery-context-fail-closed", GritRecoveryContextFailsClosed),
            Case("grit.recovery-target-exclusions", GritRecoveryTargetExclusions),
            Case("grit.recovery-half-level-boundary", GritRecoveryHalfLevelBoundary),
            Case("grit.recovery-invalid-request", GritRecoveryInvalidRequestRejected),
            Case("full-attack-reload.free-eligible", FullAttackAutoReloadEligibleFreeAction),
            Case("full-attack-reload.nonfree-ended", FullAttackAutoReloadInterruptsNonFreeActions),
            Case("full-attack-reload.loaded-capacity", FullAttackAutoReloadContinuesLoadedCapacity),
            Case("full-attack-reload.same-weapon-next-attack", FullAttackAutoReloadRequiresSameWeaponAndNextAttack),
            Case("full-attack-reload.wrecked-ended", FullAttackAutoReloadInterruptsWreckedFirearm),
            Case("full-attack-reload.invalid-inputs", FullAttackAutoReloadRejectsInvalidInputs),
            Case("deadeye.second-increment-cost-one", DeadeyeSecondIncrementCostsOne),
            Case("deadeye.cost-scales", DeadeyeCostScalesBeyondFirst),
            Case("deadeye.first-increment-no-spend", DeadeyeFirstIncrementDoesNotSpend),
            Case("deadeye.insufficient-atomic", DeadeyeInsufficientGritFailsAtomic),
            Case("deadeye.context-fail-closed", DeadeyeContextFailsClosed),
            Case("deadeye.blunderbuss-and-invalid-distance", DeadeyeBlunderbussOrdinaryRangeAndInvalidDistance),
            Case("deadeye.invalid-input", DeadeyeInvalidInputRejected),
            Case("dodge.move-exact", GunslingerDodgeMoveExact),
            Case("dodge.prone-exact", GunslingerDodgeProneExact),
            Case("dodge.ranged-only", GunslingerDodgeRequiresRangedTrigger),
            Case("dodge.armor-exact", GunslingerDodgeArmorExact),
            Case("dodge.load-exact", GunslingerDodgeLoadExact),
            Case("dodge.insufficient-atomic", GunslingerDodgeInsufficientAtomic),
            Case("dodge.invalid-input", GunslingerDodgeInvalidInput),
            Case("dodge.already-prone", GunslingerDodgeAlreadyProneRejected),
            Case("quick-clear.standard-exact", QuickClearStandardExact),
            Case("quick-clear.move-exact", QuickClearMoveExact),
            Case("quick-clear.grit-required", QuickClearGritRequired),
            Case("quick-clear.context-fail-closed", QuickClearContextFailsClosed),
            Case("quick-clear.invalid-input", QuickClearInvalidInput),
            Case("nimble.exact-levels", NimbleExactLevels),
            Case("nimble-between-levels", NimbleBetweenLevels),
            Case("nimble-armor-gate", NimbleArmorGate),
            Case("nimble-dexterity-loss", NimbleDexterityLoss),
            Case("nimble-invalid-input", NimbleInvalidInput),
            Case("initiative.positive-grit", GunslingerInitiativePositiveGrit),
            Case("initiative.zero-grit", GunslingerInitiativeZeroGrit),
            Case("initiative.invalid-input", GunslingerInitiativeInvalidInput),
            Case("bonus-feats.exact-levels", BonusFeatsExactLevels),
            Case("bonus-feats.non-levels", BonusFeatsRejectOtherLevels),
            Case("bonus-feats.invalid-levels", BonusFeatsRejectInvalidLevels),
            Case("gun-training.exact-levels", GunTrainingExactLevels),
            Case("gun-training.damage-kind", GunTrainingDamageKind),
            Case("gun-training.damage-modifiers", GunTrainingDamageModifiers),
            Case("gun-training.misfire", GunTrainingMisfirePolicy),
            Case("gun-training.invalid", GunTrainingInvalidInputs),
            Case("dead-shot.bab-cadence", DeadShotBabCadence),
            Case("dead-shot.preconditions", DeadShotPreconditionsAtomic),
            Case("dead-shot.hit-dice", DeadShotHitAndDiceAggregation),
            Case("dead-shot.misfire", DeadShotMisfireAggregation),
            Case("dead-shot.critical", DeadShotCriticalAggregation),
            Case("dead-shot.invalid", DeadShotInvalidInputs),
            Case("startling-shot.eligible", StartlingShotEligible),
            Case("startling-shot.preconditions", StartlingShotPreconditionsAtomic),
            Case("startling-shot.invalid", StartlingShotInvalidInputs),
            Case("targeting-head.eligible", TargetingHeadEligible),
            Case("targeting-head.preconditions", TargetingHeadPreconditions),
            Case("targeting-head.hit-rider", TargetingHeadHitRider),
            Case("targeting-head.rider-gates", TargetingHeadRiderGates),
            Case("targeting-head.invalid", TargetingHeadInvalid),
            Case("targeting-torso.threat-range", TargetingTorsoThreatRange),
            Case("targeting-torso.threat-gates", TargetingTorsoThreatGates),
            Case("targeting-torso.threat-invalid", TargetingTorsoThreatInvalid),
            Case("targeting-legs.eligible-rider", TargetingLegsEligibleRider),
            Case("targeting-arms.eligible-rider", TargetingArmsEligibleRider),
            Case("targeting-arms.rider-gates", TargetingArmsRiderGates),
            Case("targeting-legs.rider-gates", TargetingLegsRiderGates),
            Case("targeting-legs.rider-observations", TargetingLegsRiderObservations),
            Case("bleeding-wound.all-choices", BleedingWoundAllChoices),
            Case("bleeding-wound.marker-consumption", BleedingWoundMarkerConsumption),
            Case("bleeding-wound.gates", BleedingWoundGates),
            Case("bleeding-wound.invalid", BleedingWoundInvalid),
            Case("expert-loading.suppresses-broken-misfire", ExpertLoadingSuppressesBrokenMisfire),
            Case("expert-loading.insufficient-grit", ExpertLoadingInsufficientGritFailsClosed),
            Case("expert-loading.exact-gates", ExpertLoadingGatesAreExact),
            Case("expert-loading.invalid", ExpertLoadingInvalidInputFailsClosed),
            Case("lightning-reload.available", LightningReloadAvailableWithoutGritSpend),
            Case("lightning-reload.grit-and-round", LightningReloadRequiresPositiveGritAndRoundAvailability),
            Case("lightning-reload.broken", LightningReloadPreservesEligibleBrokenState),
            Case("lightning-reload.unit-isolation", LightningReloadUnitUseIsIndependent),
            Case("lightning-reload.gates", LightningReloadRejectsStateAndResourceGates),
            Case("lightning-reload.invalid", LightningReloadInvalidInputRejected),
            Case("evasive.positive-grit", EvasivePositiveGritAtLevelFifteen),
            Case("evasive.zero-grit", EvasiveZeroGritRemovesBenefits),
            Case("evasive.level-and-stable", EvasiveLevelGateAndStableState),
            Case("evasive.unit-isolation", EvasiveUnitStateIsIndependent),
            Case("evasive.invalid", EvasiveInvalidInputRejected),
            Case("menacing-shot.eligible", MenacingShotEligibleExactValues),
            Case("menacing-shot.dc", MenacingShotLevelAndWisdomDc),
            Case("menacing-shot.gates", MenacingShotFirearmAndGritGates),
            Case("menacing-shot.radius", MenacingShotLivingRadiusBoundary),
            Case("slingers-luck.saving", SlingersLuckSavingThrowCostAndSecondResult),
            Case("slingers-luck.skill", SlingersLuckSkillCheckCostAndSecondResult),
            Case("slingers-luck.kind-level", SlingersLuckKindAndLevelGates),
            Case("slingers-luck.fixed-cost", SlingersLuckGritGatesAreFixed),
            Case("slingers-luck.marker-duplicate", SlingersLuckMarkerAndDuplicateGates),
            Case("slingers-luck.invalid", SlingersLuckInvalidInputRejected),
            Case("cheat-death.lethal", CheatDeathLethalApplies),
            Case("cheat-death.all-grit", CheatDeathAllGritCosts),
            Case("cheat-death.positive-hp", CheatDeathPositiveHitPointsRejected),
            Case("cheat-death.gates", CheatDeathResourceAndLevelGates),
            Case("cheat-death.target-duplicate", CheatDeathTargetAndDuplicateGates),
            Case("cheat-death.invalid", CheatDeathInvalidInputRejected),
            Case("stunning-shot.eligible", StunningShotEligibleHit),
            Case("stunning-shot.miss-immunity", StunningShotMissAndImmunity),
            Case("stunning-shot.gates", StunningShotResourceAndLevelGates),
            Case("stunning-shot.isolation", StunningShotWeaponAndOwnerIsolation),
            Case("stunning-shot.duplicate", StunningShotDuplicateGate),
            Case("stunning-shot.invalid", StunningShotInvalidInput),
            Case("deaths-shot.critical", DeathsShotCritical),
            Case("deaths-shot.noncritical", DeathsShotNoncritical),
            Case("deaths-shot.gates", DeathsShotGates),
            Case("mysterious-stranger.grit", MysteriousStrangerGrit),
            Case("mysterious-stranger.focused-aim", MysteriousStrangerFocusedAim),
            Case("mysterious-stranger.lucky", MysteriousStrangerLucky),
            Case("mysterious-stranger.fortune", MysteriousStrangerFortune),
            Case("mysterious-stranger.clipping-shot", MysteriousStrangerClippingShot),
            Case("playtest-reload.base-actions", ReloadActionsBaseProfiles),
            Case("playtest-reload.rapid-actions", ReloadActionsRapidReload),
            Case("playtest-reload.wrong-choice", ReloadActionsWrongChoice),
            Case("playtest-reload.invalid", ReloadActionsInvalid),
            Case("empty-command.loaded-allows", EmptyCommandLoadedAllows),
            Case("empty-command.unloaded-rejects", EmptyCommandUnloadedRejects),
            Case("empty-command.wrecked-rejects", EmptyCommandWreckedRejects),
            Case("empty-command.auto-queues", EmptyCommandAutoQueuesLegalReload),
            Case("empty-command.ambiguous-rejects", EmptyCommandAmbiguousRejects),
            Case("dependent-feats.attack-kind", DependentFeatAttackKind),
            Case("dependent-feats.damage-kind", DependentFeatDamageKind),
            Case("dependent-feats.critical-kind", DependentFeatCriticalKind),
            Case("dependent-feats.wrong-kind", DependentFeatWrongKind),
            Case("dependent-feats.invalid", DependentFeatInvalid),
            Case("third-playtest.native-parent-only", ThirdPlaytestNativeParentOnly),
            Case("third-playtest.native-icon-guard", ThirdPlaytestNativeIconGuard),
            Case("third-playtest.firearm-parameter-menu", ThirdPlaytestFirearmParameterMenu),
            Case("third-playtest.legacy-wrapper-hidden", ThirdPlaytestLegacyWrapperHidden),
            Case("third-playtest.reload-one-public", ThirdPlaytestReloadOnePublic),
            Case("third-playtest.reload-dynamic-action", ThirdPlaytestReloadDynamicAction),
            Case("third-playtest.dodge-no-prone", ThirdPlaytestDodgeNoProne),
            Case("third-playtest.dodge-one-round-two-ac", ThirdPlaytestDodgeOneRoundTwoAc),
            Case("third-playtest.grit-shared-ui", ThirdPlaytestGritSharedUi),
            Case("third-playtest.empty-command-preconstruction", ThirdPlaytestEmptyPreconstruction),
            Case("fourth-playtest.overhaul-maintenance", FourthPlaytestOverhaulMaintenance),
            Case("fourth-playtest.condition-presentation", FourthPlaytestConditionPresentation),
            Case("fifth-playtest.item-visual-model", FifthPlaytestItemVisualModel),
            Case("fifth-playtest.audible-shot-emitter", FifthPlaytestAudibleShotEmitter),
            Case("native-rig.runtime-capability", NativeRigRuntimeCapability),
            Case("native-rig.readiness-fails-closed", NativeRigReadinessFailsClosed),
            Case("native-rig.observer-guarded", NativeRigObserverGuarded),
            Case("native-rig.calibration-session", NativeRigCalibrationSession),
            Case("native-rig.calibration-export", NativeRigCalibrationExport),
            Case("native-rig.animation-allowlist", NativeRigAnimationAllowlist),
            Case("native-rig.calibration-native-refresh", NativeRigCalibrationNativeRefresh),
            Case("native-rig.musket-candidate", NativeRigMusketCandidate),
            Case("native-rig.long-gun-candidates", NativeRigLongGunCandidates),
            Case("native-rig.short-gun-candidates", NativeRigShortGunCandidates),
            Case("native-rig.obsolete-scan-retired", NativeRigObsoleteScanRetired),
            Case("native-rig.holster-hidden-exact", NativeRigHolsterHiddenExact),
            Case("native-rig.visibility-repair", NativeRigVisibilityRepair),
            Case("true-grit.catalog", TrueGritCatalogExact),
            Case("true-grit.pair-uniqueness", TrueGritPairUniqueness),
            Case("true-grit.one-cost", TrueGritOneCostBoundary),
            Case("true-grit.two-cost", TrueGritTwoCostReduction),
            Case("true-grit.positive-gate", TrueGritPositiveGateRemoval),
            Case("true-grit.variable-cheat-death", TrueGritVariableAndCheatDeath),
            Case("true-grit.unselected-isolation", TrueGritUnselectedIsolation),
            Case("true-grit.invalid", TrueGritInvalidInput),
            Case("menacing-shot.atomic", MenacingShotRejectionsAreAtomic),
            Case("menacing-shot.invalid", MenacingShotInvalidInputRejected),
            Case("pistol-whip.handedness", PistolWhipHandednessExact),
            Case("pistol-whip.context", PistolWhipContextFailsClosed),
            Case("pistol-whip.insufficient", PistolWhipInsufficientAtomic),
            Case("pistol-whip.invalid-input", PistolWhipInvalidInput),
            Case("stop-bleeding.eligible", StopBleedingEligible),
            Case("stop-bleeding.context", StopBleedingContextFailsClosed),
            Case("stop-bleeding.atomic", StopBleedingRejectionsAtomic),
            Case("stop-bleeding.invalid-input", StopBleedingInvalidInput),
            Case("valid.early-musket", ValidEarlyMusket),
            Case("factory.early-musket-fresh-instances", FactoryEarlyMusketFreshInstances),
            Case("factory.early-musket-canonical-equality", FactoryEarlyMusketCanonicalEquality),
            Case("factory.early-pistol-fresh-instances", FactoryEarlyPistolFreshInstances),
            Case("factory.early-pistol-canonical-equality", FactoryEarlyPistolCanonicalEquality),
            Case("factory.early-blunderbuss-fresh-instances", FactoryEarlyBlunderbussFreshInstances),
            Case("factory.early-blunderbuss-ordinary-range", FactoryEarlyBlunderbussOrdinaryRange),
            Case("factory.early-blunderbuss-fixed-range-accessible", FactoryEarlyBlunderbussFixedRangeAccessible),
            Case("catalog.pistol-exact", CatalogPistolExact),
            Case("catalog.musket-exact", CatalogMusketExact),
            Case("catalog.blunderbuss-exact", CatalogBlunderbussExact),
            Case("catalog.factories-fresh", CatalogFactoriesAreFresh),
            Case("catalog.blunderbuss-dual-mode-fireable", CatalogBlunderbussDualModeFireable),
            Case("catalog.handedness-rejected", CatalogHandednessMismatchRejected),
            Case("catalog.format", CatalogFormattingDeterministic),
            Case("scatter.plan-empty", ScatterPlanEmpty),
            Case("scatter.distance-missing-rejected", ScatterDistanceMissingRejected),
            Case("scatter.distance-exact-conversion", ScatterDistanceExactConversion),
            Case("scatter.distance-pnp-blunderbuss-authority", ScatterDistancePnPBlunderbussAuthority),
            Case("scatter.distance-nonscatter-rejected", ScatterDistanceNonScatterRejected),
            Case("scatter.distance-step-rejected", ScatterDistanceStepRejected),
            Case("scatter.distance-bounds-rejected", ScatterDistanceBoundsRejected),
            Case("scatter.plan-singleton", ScatterPlanSingleton),
            Case("scatter.plan-filter-outside-wielder", ScatterPlanFiltersOutsideAndWielder),
            Case("scatter.plan-dedupe-reference", ScatterPlanDeduplicatesReference),
            Case("scatter.plan-preserve-value-equal", ScatterPlanDoesNotDeduplicateValueEquality),
            Case("scatter.plan-stable-order", ScatterPlanStableOrder),
            Case("scatter.plan-unknown-fails-closed", ScatterPlanUnknownGeometryFailsClosed),
            Case("scatter.plan-null-candidate", ScatterPlanNullCandidateRejected),
            Case("scatter.candidate-value-unit", ScatterCandidateValueUnitRejected),
            Case("scatter.candidate-invalid-distance", ScatterCandidateInvalidDistanceRejected),
            Case("scatter.volley-empty", ScatterVolleyEmpty),
            Case("scatter.volley-separate-rolls", ScatterVolleySeparateRolls),
            Case("scatter.volley-all-misfire", ScatterVolleyAllMisfire),
            Case("scatter.volley-some-misfire", ScatterVolleySomeMisfire),
            Case("scatter.volley-critical-counts", ScatterVolleyCriticalCounts),
            Case("scatter.volley-damage-exclusions", ScatterVolleyDamageExclusions),
            Case("scatter.volley-nonscatter-rejected", ScatterVolleyNonScatterRejected),
            Case("scatter.volley-missing-roll-rejected", ScatterVolleyMissingRollRejected),
            Case("scatter.volley-duplicate-roll-rejected", ScatterVolleyDuplicateRollRejected),
            Case("scatter.volley-unplanned-roll-rejected", ScatterVolleyUnplannedRollRejected),
            Case("scatter.discharge-zero-targets-once", ScatterDischargeZeroTargetsOnce),
            Case("scatter.discharge-one-target-once", ScatterDischargeOneTargetOnce),
            Case("scatter.discharge-many-targets-once", ScatterDischargeManyTargetsOnce),
            Case("scatter.discharge-prerequisite-rejection", ScatterDischargePrerequisiteRejection),
            Case("scatter.discharge-empty", ScatterDischargeEmpty),
            Case("scatter.discharge-wrecked", ScatterDischargeWrecked),
            Case("scatter.discharge-nonscatter-rejected", ScatterDischargeNonScatterRejected),
            Case("scatter.explosion-triple", ScatterExplosionTriple),
            Case("scatter.explosion-partial-misfire-rejected", ScatterExplosionPartialMisfireRejected),
            Case("scatter.explosion-empty-volley-rejected", ScatterExplosionEmptyVolleyRejected),
            Case("scatter.explosion-none", ScatterExplosionNone),
            Case("scatter.explosion-ordinary-single", ScatterExplosionOrdinarySingle),
            Case("scatter.explosion-ordinary-volley-rejected", ScatterExplosionOrdinaryVolleyRejected),
            Case("capacity.reload-empty-to-full", CapacityReloadEmptyToFull),
            Case("capacity.reload-partial-top-up", CapacityReloadPartialTopUp),
            Case("capacity.reload-full-rejected", CapacityReloadFullRejected),
            Case("capacity.reload-insufficient-atomic", CapacityReloadInsufficientAtomic),
            Case("capacity.reload-mixed-ammunition-rejected", CapacityReloadMixedAmmunitionRejected),
            Case("capacity.reload-write-failure-rolls-back", CapacityReloadWriteFailureRollsBackBatch),
            Case("capacity.policy-partial-available", CapacityPolicyPartialAvailable),
            Case("capacity.policy-full-rejected", CapacityPolicyFullRejected),
            Case("advanced.factory-rifle-exact", AdvancedFactoryRifleExact),
            Case("advanced.factory-revolver-exact", AdvancedFactoryRevolverExact),
            Case("advanced.catalog-rifle-exact", AdvancedCatalogRifleExact),
            Case("advanced.catalog-revolver-exact", AdvancedCatalogRevolverExact),
            Case("advanced.factories-fresh", AdvancedFactoriesFresh),
            Case("capacity.tokens-six-round-complete", CapacityTokensSixRoundComplete),
            Case("capacity.tokens-round-trip", CapacityTokensRoundTrip),
            Case("capacity.tokens-legacy-stable", CapacityTokensLegacyStable),
            Case("capacity.tokens-invalid-capacity", CapacityTokensInvalidCapacity),
            Case("advanced.misfire-normal-preserves-rounds", AdvancedMisfireNormalPreservesRounds),
            Case("advanced.misfire-broken-no-explosion", AdvancedMisfireBrokenNoExplosion),
            Case("capacity.early-broken-misfire-wrecks", CapacityEarlyBrokenMisfireWrecks),
            Case("capacity.vault-six-round-restart", CapacityVaultSixRoundRestart),
            Case("capacity.vault-two-item-isolation", CapacityVaultTwoItemIsolation),
            Case("capacity.repeated-discharge-isolated", CapacityRepeatedDischargeIsolated),
            Case("valid.early-pistol", ValidEarlyPistol),
            Case("valid.early-blunderbuss", ValidEarlyBlunderbuss),
            Case("valid.advanced-pistol", ValidAdvancedPistol),
            Case("valid.advanced-rifle", ValidAdvancedRifle),
            Case("valid.advanced-revolver", ValidAdvancedRevolver),
            Case("equality.firearm-definition", FirearmDefinitionValueEquality),
            Case("equality.reload-profile", ReloadProfileValueEquality),
            Case("equality.different-definition", DifferentDefinitionsAreNotEqual),
            Case("equality.different-misfire-burst", DifferentMisfireBurstDefinitionsAreNotEqual),
            Case("format.deterministic", DeterministicFormatting),
            Case("invalid.unknown-era", InvalidUnknownEra),
            Case("invalid.undefined-era", InvalidUndefinedEra),
            Case("invalid.unknown-kind", InvalidUnknownKind),
            Case("invalid.undefined-kind", InvalidUndefinedKind),
            Case("invalid.capacity-zero", InvalidCapacityZero),
            Case("invalid.capacity-too-large", InvalidCapacityTooLarge),
            Case("invalid.range-too-small", InvalidRangeTooSmall),
            Case("invalid.range-not-five-foot-step", InvalidRangeNotFiveFootStep),
            Case("invalid.range-too-large", InvalidRangeTooLarge),
            Case("invalid.misfire-zero", InvalidMisfireZero),
            Case("invalid.misfire-too-large", InvalidMisfireTooLarge),
            Case("invalid.misfire-burst-too-small", InvalidMisfireBurstTooSmall),
            Case("invalid.misfire-burst-not-five-foot-step", InvalidMisfireBurstNotFiveFootStep),
            Case("invalid.misfire-burst-too-large", InvalidMisfireBurstTooLarge),
            Case("invalid.null-reload", InvalidNullReload),
            Case("invalid.reload-unknown-action", InvalidReloadUnknownAction),
            Case("invalid.reload-undefined-action", InvalidReloadUndefinedAction),
            Case("invalid.reload-zero-rounds", InvalidReloadZeroRounds),
            Case("invalid.reload-too-many-rounds", InvalidReloadTooManyRounds),
            Case("invalid.reload-exceeds-capacity", InvalidReloadExceedsCapacity),
            Case("invalid.scatter-non-blunderbuss", InvalidScatterNonBlunderbuss),
            Case("invalid.blunderbuss-without-scatter", InvalidBlunderbussWithoutScatter),
            Case("invalid.advanced-musket", InvalidAdvancedMusket),
            Case("invalid.advanced-blunderbuss", InvalidAdvancedBlunderbuss),
            Case("invalid.early-rifle", InvalidEarlyRifle),
            Case("invalid.early-revolver", InvalidEarlyRevolver),
            Case("invalid.revolver-capacity-one", InvalidRevolverCapacityOne),
            Case("invalid.early-pistol-wrong-reload", InvalidEarlyPistolWrongReload),
            Case("invalid.early-musket-wrong-reload", InvalidEarlyMusketWrongReload),
            Case("invalid.early-blunderbuss-wrong-reload", InvalidEarlyBlunderbussWrongReload),
            Case("invalid.advanced-wrong-reload", InvalidAdvancedWrongReload),
            Case("ammo.snapshot.valid", AmmunitionSnapshotValid),
            Case("ammo.snapshot.capture", AmmunitionSnapshotCapture),
            Case("ammo.snapshot.value-equality", AmmunitionSnapshotValueEquality),
            Case("ammo.snapshot.format", AmmunitionSnapshotFormat),
            Case("ammo.snapshot.negative-powder", AmmunitionSnapshotNegativePowderRejected),
            Case("ammo.snapshot.negative-ball", AmmunitionSnapshotNegativeBallRejected),
            Case("ammo.snapshot.unknown-component", AmmunitionSnapshotUnknownComponentRejected),
            Case("ammo.snapshot.null-inventory", AmmunitionSnapshotNullInventoryRejected),
            Case("ammo.snapshot.negative-store-count", AmmunitionSnapshotNegativeStoreCountRejected),
            Case("ammo.transaction.success", AmmunitionTransactionSuccess),
            Case("ammo.transaction.multiple-success", AmmunitionTransactionMultipleCounts),
            Case("ammo.transaction.missing-powder", AmmunitionTransactionMissingPowder),
            Case("ammo.transaction.missing-ball", AmmunitionTransactionMissingBall),
            Case("ammo.transaction.empty", AmmunitionTransactionEmpty),
            Case("ammo.transaction.null-inventory", AmmunitionTransactionNullInventoryRejected),
            Case("ammo.transaction.first-remove-failure-rolls-back", AmmunitionTransactionFirstRemoveFailureRollsBack),
            Case("ammo.transaction.second-remove-failure-rolls-back", AmmunitionTransactionSecondRemoveFailureRollsBack),
            Case("ammo.transaction.after-mutation-failure-rolls-back", AmmunitionTransactionAfterMutationFailureRollsBack),
            Case("ammo.transaction.verification-failure-rolls-back", AmmunitionTransactionVerificationFailureRollsBack),
            Case("ammo.transaction.rollback-failure-surfaced", AmmunitionTransactionRollbackFailureSurfaced),
            Case("ammo.result.success-validation", AmmunitionResultSuccessValidation),
            Case("ammo.result.insufficient-validation", AmmunitionResultInsufficientValidation),
            Case("ammo.result.unknown-status", AmmunitionResultUnknownStatusRejected),
            Case("ammo.result.null-snapshots", AmmunitionResultNullSnapshotsRejected),
            Case("ammo.result.format", AmmunitionResultFormat),
            Case("reload.transaction.success", ReloadTransactionSuccess),
            Case("reload.transaction.already-loaded", ReloadTransactionAlreadyLoaded),
            Case("reload.transaction.broken-loads", ReloadTransactionBroken),
            Case("reload.transaction.loaded-broken-already-loaded", ReloadTransactionLoadedBrokenAlreadyLoaded),
            Case("reload.transaction.wrecked", ReloadTransactionWrecked),
            Case("reload.transaction.missing-powder", ReloadTransactionMissingPowder),
            Case("reload.transaction.missing-ball", ReloadTransactionMissingBall),
            Case("reload.transaction.null-state-store", ReloadTransactionNullStateStore),
            Case("reload.transaction.null-inventory", ReloadTransactionNullInventory),
            Case("reload.transaction.null-rules", ReloadTransactionNullRules),
            Case("reload.transaction.null-ammunition", ReloadTransactionNullAmmunition),
            Case("reload.transaction.null-state", ReloadTransactionNullState),
            Case("reload.transaction.incompatible-ammunition", ReloadTransactionIncompatibleAmmunition),
            Case("reload.transaction.state-write-failure-restores-inventory", ReloadTransactionStateWriteFailureRestoresInventory),
            Case("reload.transaction.post-state-mutation-failure-restores-both", ReloadTransactionPostStateMutationFailureRestoresBoth),
            Case("reload.transaction.state-rollback-failure-surfaced", ReloadTransactionStateRollbackFailureSurfaced),
            Case("reload.transaction.inventory-rollback-failure-surfaced", ReloadTransactionInventoryRollbackFailureSurfaced),
            Case("reload.result.success-validation", ReloadResultSuccessValidation),
            Case("reload.result.success-broken-validation", ReloadResultSuccessBrokenValidation),
            Case("reload.result.rejected-validation", ReloadResultRejectedValidation),
            Case("reload.result.unknown-status", ReloadResultUnknownStatus),
            Case("reload.result.null-values", ReloadResultNullValues),
            Case("reload.result.format", ReloadResultFormat),
            Case("overhaul-kit.snapshot.valid", OverhaulKitSnapshotValid),
            Case("overhaul-kit.snapshot.capture", OverhaulKitSnapshotCapture),
            Case("overhaul-kit.snapshot.equality", OverhaulKitSnapshotEquality),
            Case("overhaul-kit.snapshot.format", OverhaulKitSnapshotFormat),
            Case("overhaul-kit.snapshot.negative", OverhaulKitSnapshotNegativeRejected),
            Case("overhaul-kit.snapshot.null-inventory", OverhaulKitSnapshotNullInventoryRejected),
            Case("overhaul-kit.snapshot.negative-store-count", OverhaulKitSnapshotNegativeStoreCountRejected),
            Case("overhaul.transaction.success", OverhaulTransactionSuccess),
            Case("overhaul.transaction.normal-rejected", OverhaulTransactionNormalRejected),
            Case("overhaul.transaction.broken-rejected", OverhaulTransactionBrokenRejected),
            Case("overhaul.transaction.missing-kit", OverhaulTransactionMissingKit),
            Case("overhaul.transaction.null-state-store", OverhaulTransactionNullStateStore),
            Case("overhaul.transaction.null-inventory", OverhaulTransactionNullInventory),
            Case("overhaul.transaction.null-state", OverhaulTransactionNullState),
            Case("overhaul.transaction.state-write-failure-restores-kit", OverhaulTransactionStateWriteFailureRestoresKit),
            Case("overhaul.transaction.post-state-mutation-failure-restores-both", OverhaulTransactionPostStateMutationFailureRestoresBoth),
            Case("overhaul.transaction.state-rollback-failure-surfaced", OverhaulTransactionStateRollbackFailureSurfaced),
            Case("overhaul.transaction.inventory-rollback-failure-surfaced", OverhaulTransactionInventoryRollbackFailureSurfaced),
            Case("overhaul.transaction.post-remove-failure-restores-kit", OverhaulTransactionPostRemoveFailureRestoresKit),
            Case("overhaul.result.success", OverhaulResultSuccess),
            Case("overhaul.result.rejected", OverhaulResultRejected),
            Case("overhaul.result.unknown-status", OverhaulResultUnknownStatus),
            Case("overhaul.result.null-snapshots", OverhaulResultNullSnapshots),
            Case("overhaul.runtime-result.success", OverhaulRuntimeResultSuccess),
            Case("overhaul.runtime-result.identity-mismatch", OverhaulRuntimeResultIdentityMismatch),
            Case("overhaul.runtime-result.revision-mismatch", OverhaulRuntimeResultRevisionMismatch),
            Case("repair.transaction.success", RepairTransactionSuccess),
            Case("repair.transaction.normal-rejected", RepairTransactionNormalRejected),
            Case("repair.transaction.wrecked-rejected", RepairTransactionWreckedRejected),
            Case("repair.transaction.loaded-broken-rejected", RepairTransactionLoadedBrokenRejected),
            Case("repair.transaction.missing-kit", RepairTransactionMissingKit),
            Case("repair.transaction.null-state-store", RepairTransactionNullStateStore),
            Case("repair.transaction.null-inventory", RepairTransactionNullInventory),
            Case("repair.transaction.null-state", RepairTransactionNullState),
            Case("repair.transaction.state-write-failure-restores-kit", RepairTransactionStateWriteFailureRestoresKit),
            Case("repair.transaction.post-state-mutation-failure-restores-both", RepairTransactionPostStateMutationFailureRestoresBoth),
            Case("repair.transaction.state-rollback-failure-surfaced", RepairTransactionStateRollbackFailureSurfaced),
            Case("repair.transaction.inventory-rollback-failure-surfaced", RepairTransactionInventoryRollbackFailureSurfaced),
            Case("repair.transaction.post-remove-failure-restores-kit", RepairTransactionPostRemoveFailureRestoresKit),
            Case("repair.result.success", RepairResultSuccess),
            Case("repair.result.rejected", RepairResultRejected),
            Case("repair.result.unknown-status", RepairResultUnknownStatus),
            Case("repair.runtime-result.success", RepairRuntimeResultSuccess),
            Case("repair.runtime-result.identity-mismatch", RepairRuntimeResultIdentityMismatch),
            Case("repair.runtime-result.revision-mismatch", RepairRuntimeResultRevisionMismatch),
            Case("maintenance.fixture-pass", MaintenanceFixturePass),
            Case("maintenance.overhaul-pass", MaintenanceOverhaulPass),
            Case("maintenance.repair-pass", MaintenanceRepairPass),
            Case("maintenance.loop-pass", MaintenanceLoopPass),
            Case("maintenance.second-item-mutation-fails", MaintenanceSecondItemMutationFails),
            Case("maintenance.resource-drift-fails", MaintenanceResourceDriftFails),
            Case("maintenance.fault-delta-fails", MaintenanceFaultDeltaFails),
            Case("maintenance.duplicate-delta-fails", MaintenanceDuplicateDeltaFails),
            Case("maintenance.identity-change-fails", MaintenanceIdentityChangeFails),
            Case("maintenance.session-lifecycle", MaintenanceSessionLifecycle),
            Case("maintenance.report-format", MaintenanceReportFormat),
            Case("discharge.loaded-normal", DischargeLoadedNormal),
            Case("discharge.loaded-broken", DischargeLoadedBroken),
            Case("discharge.multiple-rounds", DischargeMultipleRounds),
            Case("discharge.empty-normal", DischargeEmptyNormal),
            Case("discharge.empty-broken", DischargeEmptyBroken),
            Case("discharge.wrecked", DischargeWrecked),
            Case("discharge.null-state", DischargeNullState),
            Case("discharge.result-fired-validation", DischargeResultFiredValidation),
            Case("discharge.result-empty-validation", DischargeResultEmptyValidation),
            Case("discharge.result-wrecked-validation", DischargeResultWreckedValidation),
            Case("discharge.result-unknown-status", DischargeResultUnknownStatus),
            Case("discharge.result-null-state", DischargeResultNullState),
            Case("discharge.result-format", DischargeResultFormat),
            Case("misfire.natural-one-forces-miss", MisfireNaturalOneForcesMiss),
            Case("misfire.natural-two-forces-miss", MisfireNaturalTwoForcesMiss),
            Case("misfire.above-threshold-preserves-hit", MisfireAboveThresholdPreservesHit),
            Case("misfire.above-threshold-preserves-native-miss", MisfireAboveThresholdPreservesNativeMiss),
            Case("misfire.threshold-twenty", MisfireThresholdTwenty),
            Case("misfire.decision-format", MisfireDecisionFormat),
            Case("misfire-condition.ordinary-normal", MisfireConditionOrdinaryNormalUnchanged),
            Case("misfire-condition.ordinary-broken", MisfireConditionOrdinaryBrokenUnchanged),
            Case("misfire-condition.normal-to-broken", MisfireConditionNormalToBroken),
            Case("misfire-condition.broken-to-wrecked", MisfireConditionBrokenToWrecked),
            Case("misfire-condition.loaded-rejected", MisfireConditionLoadedStateRejected),
            Case("misfire-condition.wrecked-rejected", MisfireConditionWreckedStateRejected),
            Case("misfire-condition.null-decision", MisfireConditionNullDecisionRejected),
            Case("misfire-condition.null-state", MisfireConditionNullStateRejected),
            Case("misfire-condition.format", MisfireConditionFormat),
            Case("misfire-condition.misfire-none-rejected", MisfireConditionMisfireWithoutTransitionRejected),
            Case("misfire-condition.ordinary-transition-rejected", MisfireConditionOrdinaryTransitionRejected),
            Case("misfire-condition.unknown-transition-rejected", MisfireConditionUnknownTransitionRejected),
            Case("explosion.ordinary-normal-none", ExplosionOrdinaryNormalNone),
            Case("explosion.ordinary-broken-none", ExplosionOrdinaryBrokenNone),
            Case("explosion.normal-to-broken-none", ExplosionNormalToBrokenNone),
            Case("explosion.broken-to-wrecked-damages-burst", ExplosionBrokenToWreckedDamagesBurst),
            Case("explosion.reflex-dc-twelve", ExplosionReflexDcTwelve),
            Case("explosion.null-condition", ExplosionNullConditionRejected),
            Case("explosion.format", ExplosionDecisionFormat),
            Case("explosion.decision.broken-to-wrecked-none-rejected", ExplosionBrokenToWreckedNoneRejected),
            Case("explosion.decision.normal-to-broken-burst-rejected", ExplosionNormalToBrokenBurstRejected),
            Case("explosion.decision.unknown-disposition", ExplosionUnknownDispositionRejected),
            Case("explosion-target.candidate-valid", ExplosionTargetCandidateValid),
            Case("explosion-target.candidate-null-unit", ExplosionTargetCandidateNullUnitRejected),
            Case("explosion-target.candidate-value-unit", ExplosionTargetCandidateValueUnitRejected),
            Case("explosion-target.candidate-blank-identity", ExplosionTargetCandidateBlankIdentityRejected),
            Case("explosion-target.candidate-blank-name", ExplosionTargetCandidateBlankNameRejected),
            Case("explosion-target.candidate-negative-distance", ExplosionTargetCandidateNegativeDistanceRejected),
            Case("explosion-target.candidate-nonfinite-distance", ExplosionTargetCandidateNonfiniteDistanceRejected),
            Case("explosion-target.plan-exact-only", ExplosionTargetPlanExactOnly),
            Case("explosion-target.plan-orders-and-wielder-last", ExplosionTargetPlanOrdersAndWielderLast),
            Case("explosion-target.plan-dedupes-exact-wielder", ExplosionTargetPlanDedupesExactWielder),
            Case("explosion-target.plan-dedupes-nearby-reference", ExplosionTargetPlanDedupesNearbyReference),
            Case("explosion-target.plan-stable-tie-order", ExplosionTargetPlanStableTieOrder),
            Case("explosion-target.plan-null-exact", ExplosionTargetPlanNullExactRejected),
            Case("explosion-target.plan-exact-flag-required", ExplosionTargetPlanExactFlagRequired),
            Case("explosion-target.plan-null-nearby", ExplosionTargetPlanNullNearbyRejected),
            Case("explosion-target.plan-null-candidate", ExplosionTargetPlanNullCandidateRejected),
            Case("explosion-target.plan-nearby-exact-flag", ExplosionTargetPlanNearbyExactFlagRejected),
            Case("explosion-target.plan-constructor-wielder-last", ExplosionTargetPlanConstructorRequiresWielderLast),
            Case("explosion-target.result-format", ExplosionTargetResultFormat),
            Case("explosion-target.result-half-flag", ExplosionTargetResultInvalidHalfFlagRejected),
            Case("explosion-target.result-roll", ExplosionTargetResultInvalidRollRejected),
            Case("explosion-target.result-distance", ExplosionTargetResultInvalidDistanceRejected),
            Case("explosion-target.result-negative-hp", ExplosionTargetResultAllowsNegativeHitPoints),
            Case("misfire.invalid-roll-zero", MisfireInvalidRollZero),
            Case("misfire.invalid-roll-twenty-one", MisfireInvalidRollTwentyOne),
            Case("misfire.zero-threshold-natural-one-native-miss", MisfireZeroThresholdNaturalOneNativeMiss),
            Case("misfire.invalid-threshold-twenty-one", MisfireInvalidThresholdTwentyOne),
            Case("forced-roll.empty", ForcedRollQueueEmpty),
            Case("forced-roll.set-consume", ForcedRollQueueSetConsume),
            Case("forced-roll.replace", ForcedRollQueueReplace),
            Case("forced-roll.cancel", ForcedRollQueueCancel),
            Case("forced-roll.cancel-empty", ForcedRollQueueCancelEmpty),
            Case("forced-roll.consume-empty", ForcedRollQueueConsumeEmpty),
            Case("forced-roll.invalid-zero", ForcedRollQueueInvalidZero),
            Case("forced-roll.invalid-twenty-one", ForcedRollQueueInvalidTwentyOne),
            Case("misfire-patch.roll-setter-exact", MisfirePatchRollSetterExact),
            Case("misfire-patch.roll-setter-public", MisfirePatchRollSetterPublicRejected),
            Case("misfire-patch.roll-setter-wrong-entry", MisfirePatchRollSetterWrongEntryRejected),
            Case("misfire-patch.roll-setter-inherited", MisfirePatchRollSetterInheritedRejected),
            Case("misfire-patch.roll-setter-null-method", MisfirePatchRollSetterNullMethodRejected),
            Case("misfire-patch.roll-setter-null-rule-type", MisfirePatchRollSetterNullRuleTypeRejected),
            Case("misfire-patch.roll-setter-null-entry-type", MisfirePatchRollSetterNullEntryTypeRejected),
            Case("misfire-patch.success-exact", MisfirePatchSuccessExact),
            Case("misfire-patch.success-private", MisfirePatchSuccessPrivateRejected),
            Case("misfire-patch.success-wrong-argument", MisfirePatchSuccessWrongArgumentRejected),
            Case("misfire-patch.success-wrong-return", MisfirePatchSuccessWrongReturnRejected),
            Case("misfire-patch.success-static", MisfirePatchSuccessStaticRejected),
            Case("misfire-patch.success-generic", MisfirePatchSuccessGenericRejected),
            Case("misfire-patch.success-inherited", MisfirePatchSuccessInheritedRejected),
            Case("misfire-patch.success-null-method", MisfirePatchSuccessNullMethodRejected),
            Case("misfire-patch.success-null-rule-type", MisfirePatchSuccessNullRuleTypeRejected),
            Case("event-gate.first-and-duplicate", EventGateFirstAndDuplicate),
            Case("event-gate.reference-identity", EventGateReferenceIdentity),
            Case("event-gate.null", EventGateNull),
            Case("event-gate.value-type", EventGateValueType),
            Case("patch-target.exact-contract", PatchTargetExactContract),
            Case("patch-target.null-method", PatchTargetNullMethodRejected),
            Case("patch-target.zero-argument", PatchTargetZeroArgumentRejected),
            Case("patch-target.wrong-context", PatchTargetWrongContextRejected),
            Case("patch-target.multiple-arguments", PatchTargetMultipleArgumentsRejected),
            Case("patch-target.static", PatchTargetStaticRejected),
            Case("patch-target.generic", PatchTargetGenericRejected),
            Case("patch-target.non-void", PatchTargetNonVoidRejected),
            Case("patch-target.null-context", PatchTargetNullContextRejected),
            Case("token-reconcile.none", TokenReconcileNoToken),
            Case("token-reconcile.preserved", TokenReconcilePreserved),
            Case("token-reconcile.restore", TokenReconcileRestore),
            Case("token-reconcile.appeared-conflict", TokenReconcileAppearedConflict),
            Case("token-reconcile.changed-conflict", TokenReconcileChangedConflict),
            Case("token-reconcile.multiple-before-conflict", TokenReconcileMultipleBeforeConflict),
            Case("token-reconcile.multiple-after-conflict", TokenReconcileMultipleAfterConflict),
            Case("token-reconcile.null-before", TokenReconcileNullBefore),
            Case("token-reconcile.blank-token", TokenReconcileBlankToken),
            Case("token-reconcile.format-defensive-copy", TokenReconcileFormatAndDefensiveCopy),
            Case("state.ammunition-id.valid", StateAmmunitionIdValid),
            Case("state.ammunition-id.value-equality", StateAmmunitionIdValueEquality),
            Case("state.ammunition-id.uppercase-rejected", StateAmmunitionIdUppercaseRejected),
            Case("state.ammunition-id.null", StateAmmunitionIdNullRejected),
            Case("state.ammunition-id.empty", StateAmmunitionIdEmptyRejected),
            Case("state.ammunition-id.leading-separator", StateAmmunitionIdLeadingSeparatorRejected),
            Case("state.ammunition-id.whitespace", StateAmmunitionIdWhitespaceRejected),
            Case("state.ammunition-id.too-long", StateAmmunitionIdTooLongRejected),
            Case("state.rules.valid", StateRulesValid),
            Case("state.rules.sorted-copy", StateRulesReturnsSortedCopy),
            Case("state.rules.capacity-zero", StateRulesCapacityZeroRejected),
            Case("state.rules.capacity-too-large", StateRulesCapacityTooLargeRejected),
            Case("state.rules.null-collection", StateRulesNullCollectionRejected),
            Case("state.rules.empty-collection", StateRulesEmptyCollectionRejected),
            Case("state.rules.null-entry", StateRulesNullEntryRejected),
            Case("state.rules.duplicate", StateRulesDuplicateRejected),
            Case("state.empty.canonical", StateEmptyCanonical),
            Case("state.value-equality", StateValueEquality),
            Case("state.format.deterministic", StateFormattingDeterministic),
            Case("state.invalid-schema", StateInvalidSchemaRejected),
            Case("state.invalid-negative-rounds", StateNegativeRoundsRejected),
            Case("state.invalid-loaded-without-ammo", StateLoadedWithoutAmmunitionRejected),
            Case("state.invalid-empty-with-ammo", StateEmptyWithAmmunitionRejected),
            Case("state.invalid-unknown-condition", StateUnknownConditionRejected),
            Case("state.invalid-wrecked-loaded", StateWreckedLoadedRejected),
            Case("state.load.empty", StateLoadEmpty),
            Case("state.load.partial-to-capacity", StateLoadPartialToCapacity),
            Case("state.load.broken", StateLoadBroken),
            Case("state.load.over-capacity", StateLoadOverCapacityRejected),
            Case("state.load.incompatible", StateLoadIncompatibleRejected),
            Case("state.load.mixed", StateLoadMixedRejected),
            Case("state.load.zero-rounds", StateLoadZeroRoundsRejected),
            Case("state.load.null-state", StateLoadNullStateRejected),
            Case("state.load.null-rules", StateLoadNullRulesRejected),
            Case("state.load.invalid-existing-state", StateLoadInvalidExistingStateRejected),
            Case("state.fire.consumes-one", StateFireConsumesOne),
            Case("state.fire.final-clears-ammo", StateFireFinalRoundClearsAmmunition),
            Case("state.fire.broken", StateFireBroken),
            Case("state.fire.empty-rejected", StateFireEmptyRejected),
            Case("state.fire.wrecked-rejected", StateFireWreckedRejected),
            Case("state.misfire.normal-to-broken", StateMisfireNormalToBroken),
            Case("state.misfire.broken-to-wrecked", StateMisfireBrokenToWrecked),
            Case("state.misfire.wrecked-rejected", StateMisfireWreckedRejected),
            Case("state.repair.broken-to-normal", StateRepairBrokenToNormal),
            Case("state.repair.normal-rejected", StateRepairNormalRejected),
            Case("state.repair.wrecked-rejected", StateRepairWreckedRejected),
            Case("state.overhaul.wrecked-to-broken", StateOverhaulWreckedToBroken),
            Case("state.overhaul.normal-rejected", StateOverhaulNormalRejected),
            Case("state.overhaul.broken-rejected", StateOverhaulBrokenRejected),
            Case("state.wreck.normal-clears-load", StateWreckNormalClearsLoad),
            Case("state.wreck.idempotent", StateWreckIsIdempotent),
            Case("state.codec.empty-roundtrip", StateCodecEmptyRoundTrip),
            Case("state.codec.loaded-broken-roundtrip", StateCodecLoadedBrokenRoundTrip),
            Case("state.codec.canonical-dto", StateCodecCanonicalDto),
            Case("state.codec.wrong-schema", StateCodecWrongSchemaRejected),
            Case("state.codec.unknown-condition", StateCodecUnknownConditionRejected),
            Case("state.codec.case-sensitive-condition", StateCodecCaseSensitiveCondition),
            Case("state.codec.over-capacity", StateCodecOverCapacityRejected),
            Case("state.codec.incompatible-ammo", StateCodecIncompatibleAmmunitionRejected),
            Case("state.codec.loaded-missing-ammo", StateCodecLoadedMissingAmmunitionRejected),
            Case("state.codec.empty-with-ammo", StateCodecEmptyWithAmmunitionRejected),
            Case("state.codec.wrecked-loaded", StateCodecWreckedLoadedRejected),
            Case("state.codec.null-data", StateCodecNullDataRejected),
            Case("state.codec.null-rules", StateCodecNullRulesRejected),
            Case("token.definition.valid", TokenDefinitionValid),
            Case("token.definition.value-equality", TokenDefinitionValueEquality),
            Case("token.definition.null-state", TokenDefinitionNullStateRejected),
            Case("token.definition.empty-id", TokenDefinitionEmptyIdRejected),
            Case("token.definition.uppercase-id", TokenDefinitionUppercaseIdRejected),
            Case("token.catalog.count", TokenCatalogContainsFourDefinitions),
            Case("token.catalog.order", TokenCatalogDefinitionsAreSorted),
            Case("token.catalog.defensive-copy", TokenCatalogDefinitionsAreDefensiveCopy),
            Case("token.catalog.absence-default", TokenCatalogAbsenceMeansEmptyNormal),
            Case("token.catalog.loaded-normal", TokenCatalogLoadedNormalRoundTrip),
            Case("token.catalog.broken-empty", TokenCatalogBrokenEmptyRoundTrip),
            Case("token.catalog.broken-loaded", TokenCatalogBrokenLoadedRoundTrip),
            Case("token.catalog.wrecked", TokenCatalogWreckedRoundTrip),
            Case("token.catalog.unknown", TokenCatalogUnknownRejected),
            Case("token.catalog.duplicate-payload", TokenCatalogDuplicatePayloadRejected),
            Case("token.catalog.null-payload", TokenCatalogNullPayloadRejected),
            Case("token.catalog.null-entry", TokenCatalogNullEntryRejected),
            Case("token.catalog.encode-null", TokenCatalogEncodeNullRejected),
            Case("token.catalog.unsupported-state", TokenCatalogUnsupportedStateRejected),
            Case("token.catalog.duplicate-id", TokenCatalogDuplicateIdRejected),
            Case("token.catalog.duplicate-state", TokenCatalogDuplicateStateRejected),
            Case("token.catalog.default-token", TokenCatalogDefaultDefinitionRejected),
            Case("token.catalog.require-unknown", TokenCatalogRequireUnknownRejected),
            Case("token.catalog.contains", TokenCatalogContainsKnownOnly),
            Case("repository.reference-comparer.same", RepositoryReferenceComparerSame),
            Case("repository.reference-comparer.distinct-value-equal", RepositoryReferenceComparerDistinctValueEqual),
            Case("repository.unseen-empty", RepositoryUnseenCreatesEmpty),
            Case("repository.same-reference-stable-entry", RepositorySameReferenceStableEntry),
            Case("repository.distinct-equal-separate-entries", RepositoryDistinctEqualSeparateEntries),
            Case("repository.set-isolated", RepositorySetIsIsolated),
            Case("repository.set-increments-revision", RepositorySetIncrementsRevision),
            Case("repository.noop-set-no-revision", RepositoryNoOpSetDoesNotIncrementRevision),
            Case("repository.transition-increments-revision", RepositoryTransitionIncrementsRevision),
            Case("repository.transition-rejected-preserves", RepositoryRejectedTransitionPreservesState),
            Case("repository.transition-null-preserves", RepositoryNullTransitionResultPreservesState),
            Case("repository.try-get-missing-no-create", RepositoryTryGetMissingDoesNotCreate),
            Case("repository.snapshot-immutable", RepositorySnapshotRemainsImmutable),
            Case("repository.remove-existing", RepositoryRemoveExisting),
            Case("repository.remove-missing", RepositoryRemoveMissing),
            Case("repository.remove-readd-new-entry", RepositoryRemoveAndReaddCreatesNewEntry),
            Case("repository.null-key", RepositoryNullKeyRejected),
            Case("repository.value-key", RepositoryValueKeyRejected),
            Case("repository.null-state", RepositoryNullStateRejected),
            Case("repository.null-transition", RepositoryNullTransitionRejected),
            Case("repository.counters", RepositoryCountersAreDeterministic),
            Case("item-state.exact-initializes-empty", ItemStateExactFirearmInitializesEmpty),
            Case("item-state.native-rejected-no-entry", ItemStateNativeWeaponRejectedWithoutEntry),
            Case("item-state.ambiguous-rejected-no-entry", ItemStateAmbiguousWeaponRejectedWithoutEntry),
            Case("item-state.blueprint-rejected-no-entry", ItemStateBlueprintRejectedWithoutEntry),
            Case("item-state.canonical-key", ItemStateUsesCanonicalItemKey),
            Case("item-state.two-firearms-independent", ItemStateTwoFirearmsRemainIndependent),
            Case("item-state.set-metadata", ItemStateSetPreservesMetadata),
            Case("item-state.transition", ItemStateTransitionUsesRepository),
            Case("item-state.get-existing-missing-no-create", ItemStateGetExistingMissingDoesNotCreate),
            Case("item-state.forget", ItemStateForgetRemovesEntry),
            Case("item-state.format-deterministic", ItemStateFormattingIsDeterministic),
            Case("token-repository.unseen-empty", TokenRepositoryUnseenIsEmpty),
            Case("token-repository.set-loaded", TokenRepositorySetLoadedWritesToken),
            Case("token-repository.set-broken", TokenRepositorySetBrokenWritesToken),
            Case("token-repository.reset-clears", TokenRepositoryResetClearsToken),
            Case("token-repository.two-independent", TokenRepositoryTwoItemsRemainIndependent),
            Case("token-repository.value-equal-independent", TokenRepositoryValueEqualItemsRemainIndependent),
            Case("token-repository.revision", TokenRepositoryRevisionIncrements),
            Case("token-repository.noop", TokenRepositoryNoOpDoesNotWrite),
            Case("token-repository.transition", TokenRepositoryTransitionCommits),
            Case("token-repository.transition-rejected", TokenRepositoryRejectedTransitionPreserves),
            Case("token-repository.transition-null", TokenRepositoryNullTransitionPreserves),
            Case("token-repository.try-get-empty", TokenRepositoryTryGetEmptyDoesNotCreate),
            Case("token-repository.try-get-token", TokenRepositoryTryGetPersistedTokenReconstructs),
            Case("token-repository.unknown-token", TokenRepositoryUnknownTokenFailsClosed),
            Case("token-repository.duplicate-token", TokenRepositoryDuplicateTokensFailClosed),
            Case("token-repository.replace-failure", TokenRepositoryReplaceFailurePreserves),
            Case("token-repository.concurrent-change", TokenRepositoryConcurrentChangeFailsClosed),
            Case("token-repository.corrupt-write", TokenRepositoryCorruptWriteDetected),
            Case("token-repository.unsupported-state", TokenRepositoryUnsupportedStatePreserves),
            Case("token-repository.remove-token", TokenRepositoryRemoveClearsToken),
            Case("token-repository.remove-missing", TokenRepositoryRemoveMissingReturnsFalse),
            Case("token-repository.remove-metadata", TokenRepositoryRemoveMetadataReturnsTrue),
            Case("token-repository.snapshot-immutable", TokenRepositorySnapshotRemainsImmutable),
            Case("token-repository.counters", TokenRepositoryCounters),
            Case("token-repository.null-key", TokenRepositoryNullKeyRejected),
            Case("token-repository.value-key", TokenRepositoryValueKeyRejected),
            Case("token-repository.null-state", TokenRepositoryNullStateRejected),
            Case("token-repository.null-transition", TokenRepositoryNullTransitionRejected),
            Case("vault-data.clone-null", VaultDataCloneNull),
            Case("vault-data.clone-independent", VaultDataCloneIsIndependent),
            Case("vault-data.equal", VaultDataEquality),
            Case("vault-data.not-equal", VaultDataInequality),
            Case("vault-data.describe", VaultDataDescription),
            Case("vault-repository.unseen-empty", VaultRepositoryUnseenIsEmpty),
            Case("vault-repository.set-loaded", VaultRepositorySetLoadedWritesRecord),
            Case("vault-repository.reconstruct", VaultRepositoryPersistedRecordReconstructs),
            Case("vault-repository.reset-clears", VaultRepositoryResetClearsRecord),
            Case("vault-repository.two-independent", VaultRepositoryTwoItemsRemainIndependent),
            Case("vault-repository.value-equal-independent", VaultRepositoryValueEqualItemsRemainIndependent),
            Case("vault-repository.revision", VaultRepositoryRevisionIncrements),
            Case("vault-repository.noop", VaultRepositoryNoOpDoesNotWrite),
            Case("vault-repository.transition", VaultRepositoryTransitionCommits),
            Case("vault-repository.transition-rejected", VaultRepositoryRejectedTransitionPreserves),
            Case("vault-repository.transition-null", VaultRepositoryNullTransitionPreserves),
            Case("vault-repository.try-get-empty", VaultRepositoryTryGetEmptyDoesNotCreate),
            Case("vault-repository.corrupt-read", VaultRepositoryCorruptReadFailsClosed),
            Case("vault-repository.replace-failure", VaultRepositoryReplaceFailurePreserves),
            Case("vault-repository.concurrent-change", VaultRepositoryConcurrentChangeFailsClosed),
            Case("vault-repository.corrupt-write", VaultRepositoryCorruptWriteDetected),
            Case("vault-repository.remove-record", VaultRepositoryRemoveClearsRecord),
            Case("vault-repository.remove-missing", VaultRepositoryRemoveMissingReturnsFalse),
            Case("vault-repository.remove-metadata", VaultRepositoryRemoveMetadataReturnsTrue),
            Case("vault-repository.snapshot-immutable", VaultRepositorySnapshotRemainsImmutable),
            Case("vault-repository.counters", VaultRepositoryCounters),
            Case("vault-repository.null-key", VaultRepositoryNullKeyRejected),
            Case("vault-repository.value-key", VaultRepositoryValueKeyRejected),
            Case("vault-repository.null-state", VaultRepositoryNullStateRejected),
            Case("vault-repository.null-transition", VaultRepositoryNullTransitionRejected),
            Case("migration.no-token", MigrationNoTokenDelegates),
            Case("migration.loaded-normal", MigrationLoadedNormal),
            Case("migration.broken-empty", MigrationBrokenEmpty),
            Case("migration.broken-loaded", MigrationBrokenLoaded),
            Case("migration.wrecked", MigrationWrecked),
            Case("migration.same-state-cleanup", MigrationSameStateCleansRedundantToken),
            Case("migration.conflict", MigrationConflictPreservesBoth),
            Case("migration.unknown-token", MigrationUnknownTokenPreservesEvidence),
            Case("migration.duplicate-token", MigrationDuplicateTokenPreservesEvidence),
            Case("migration.vault-write-failure", MigrationVaultWriteFailurePreservesToken),
            Case("migration.token-clear-failure", MigrationTokenClearFailureRollsBackVault),
            Case("migration.rollback-failure", MigrationRollbackFailureIsCounted),
            Case("migration.set-after", MigrationSetAfterMigration),
            Case("migration.transition-after", MigrationTransitionAfterMigration),
            Case("migration.try-get", MigrationTryGetMigrates),
            Case("migration.remove", MigrationRemoveMigratesThenDeletes),
            Case("migration.two-independent", MigrationTwoItemsRemainIndependent),
            Case("migration.value-equal-independent", MigrationValueEqualItemsRemainIndependent),
            Case("migration.no-repeat", MigrationRunsOnlyWhileTokenExists),
            Case("migration.snapshot-format", MigrationSnapshotFormatting),
            Case("migration.null-key", MigrationNullKeyRejected),
            Case("migration.null-state", MigrationNullStateRejected),
            Case("migration.null-transition", MigrationNullTransitionRejected),
            Case("identity.valid", ItemIdentityValid),
            Case("identity.uppercase-canonical", ItemIdentityUppercaseCanonicalized),
            Case("identity.guid-constructor", ItemIdentityGuidConstructor),
            Case("identity.value-equality", ItemIdentityValueEquality),
            Case("identity.inequality", ItemIdentityInequality),
            Case("identity.order", ItemIdentityOrdinalOrder),
            Case("identity.empty", ItemIdentityEmptyRejected),
            Case("identity.compact", ItemIdentityCompactRejected),
            Case("identity.braces", ItemIdentityBracesRejected),
            Case("identity.whitespace", ItemIdentityWhitespaceRejected),
            Case("identity.null", ItemIdentityNullRejected),
            Case("identity.try-create-valid", ItemIdentityTryCreateValid),
            Case("identity.try-create-invalid", ItemIdentityTryCreateInvalid),
            Case("identity.operator-null", ItemIdentityNullOperators),
            Case("identity-vault.write-read", IdentityVaultWriteRead),
            Case("identity-vault.reconstruct-same-id", IdentityVaultReconstructedObjectReadsState),
            Case("identity-vault.different-ids", IdentityVaultDifferentIdsIndependent),
            Case("identity-vault.value-equal-different-ids", IdentityVaultValueEqualItemsDifferentIds),
            Case("identity-vault.value-equal-same-id", IdentityVaultValueEqualItemsSameId),
            Case("identity-vault.reset-removes", IdentityVaultResetRemoves),
            Case("identity-vault.compare-failure", IdentityVaultCompareFailurePreserves),
            Case("identity-vault.remove", IdentityVaultRemove),
            Case("identity-vault.remove-missing", IdentityVaultRemoveMissing),
            Case("identity-vault.provider-reject-read", IdentityVaultProviderRejectsRead),
            Case("identity-vault.provider-reject-write", IdentityVaultProviderRejectsWrite),
            Case("identity-vault.provider-null", IdentityVaultProviderNullIdentity),
            Case("identity-vault.defensive-read", IdentityVaultDefensiveRead),
            Case("identity-vault.defensive-write", IdentityVaultDefensiveWrite),
            Case("identity-vault.count", IdentityVaultCount),
            Case("identity-repository.reconstruct", IdentityRepositoryReconstructs),
            Case("identity-repository.two-independent", IdentityRepositoryTwoIndependent),
            Case("identity-repository.same-id", IdentityRepositorySameIdAcrossObjects),
            Case("identity-repository.noop", IdentityRepositoryNoOp),
            Case("identity-migration.snapshot-format", IdentityMigrationSnapshotFormatting),
            Case("identity-migration.snapshot-properties", IdentityMigrationSnapshotProperties),
            Case("reflection.private-field", ReflectionReadsPrivateField),
            Case("reflection.static-property", ReflectionReadsStaticProperty),
            Case("reflection.first-non-null-member", ReflectionFindsFirstNonNullMember),
            Case("reflection.path", ReflectionReadsPath),
            Case("reflection.enumerate", ReflectionEnumeratesCollection),
            Case("reflection.invoke-exact", ReflectionInvokesExactOverload),
            Case("reflection.invoke-optional-default", ReflectionSuppliesOptionalDefault),
            Case("reflection.invoke-trailing-null", ReflectionSuppliesTrailingNull),
            Case("reflection.invoke-required-bool", ReflectionInvokesRequiredBoolean),
            Case("reflection.invoke-incompatible", ReflectionRejectsIncompatibleMethod),
            Case("range.zero-is-first", RangeZeroIsFirstIncrement),
            Case("range.within-first", RangeWithinFirstIncrement),
            Case("range.exact-boundary", RangeExactBoundaryIsFirstIncrement),
            Case("range.second", RangeSecondIncrement),
            Case("range.multiple", RangeMultipleIncrements),
            Case("range.invalid-negative", RangeRejectsNegativeDistance),
            Case("range.invalid-zero-increment", RangeRejectsZeroIncrement),
            Case("range.invalid-infinite", RangeRejectsInfiniteDistance),
            Case("trace.native-marker-ignored", TraceNativeMarkerIsIgnored),
            Case("trace.ambiguous-marker-ignored", TraceAmbiguousMarkerIsIgnored),
            Case("trace.exact-firearm-starts", TraceExactFirearmStarts),
            Case("trace.child-joins-parent", TraceChildJoinsParent),
            Case("trace.duplicate-callback", TraceDuplicateCallbackIsCounted),
            Case("trace.unrelated-child-ignored", TraceUnrelatedChildIsIgnored),
            Case("trace.weapon-root-completes", TraceWeaponRootCompletes),
            Case("trace.attack-roll-root-completes", TraceAttackRollRootCompletes),
            Case("trace.reset-clears-active", TraceResetClearsActive),
            Case("trace.id-source-must-be-unique", TraceIdSourceMustBeUnique),
            Case("observation.copies-fields", ObservationCopiesFields),
            Case("observation.exact-requires-one-marker", ObservationExactRequiresOneMarker),
            Case("formatter.fields-sorted", FormatterSortsFields),
            Case("formatter.sanitizes-one-line", FormatterSanitizesOneLine),
            Case("formatter.completion-counts-duplicates", FormatterCompletionCountsDuplicates),
            Case("ac.native-ordinary", ArmorClassNativeWeaponUsesOrdinary),
            Case("ac.ambiguous-ordinary", ArmorClassAmbiguousMarkerUsesOrdinary),
            Case("ac.missing-definition", ArmorClassMissingDefinitionUsesOrdinary),
            Case("ac.zero-distance-touch", ArmorClassZeroDistanceUsesTouch),
            Case("ac.close-touch", ArmorClassCloseRangeUsesTouch),
            Case("ac.boundary-touch", ArmorClassBoundaryUsesTouch),
            Case("ac.boundary-float-noise-touch", ArmorClassBoundaryFloatNoiseUsesTouch),
            Case("ac.distant-ordinary", ArmorClassDistantRangeUsesOrdinary),
            Case("ac.deadeye-distant-touch", ArmorClassDeadeyeDistantUsesTouch),
            Case("ac.deadeye-context-preserved", ArmorClassDeadeyePreservesContext),
            Case("ac.cover-preserved", ArmorClassPreservesCoverAdjustment),
            Case("ac.flat-footed-preserved", ArmorClassPreservesFlatFootedAdjustment),
            Case("ac.equal-no-write", ArmorClassEqualValuesRequireNoWrite),
            Case("ac.already-applied", ArmorClassAlreadyAppliedIsSkipped),
            Case("ac.advanced-fails-closed", ArmorClassAdvancedFirearmFailsClosed),
            Case("ac.blunderbuss-first-increment-touch", ArmorClassBlunderbussFirstIncrementTouch),
            Case("ac.invalid-distance", ArmorClassInvalidDistanceFailsClosed),
            Case("ac.negative-distance", ArmorClassNegativeDistanceFailsClosed),
            Case("ac.infinite-distance", ArmorClassInfiniteDistanceFailsClosed),
            Case("ac.overflow", ArmorClassOverflowFailsClosed),
            Case("ac-access.participants", ArmorClassAccessReadsParticipants),
            Case("ac-access.distance", ArmorClassAccessReadsDistance),
            Case("ac-access.values", ArmorClassAccessReadsValues),
            Case("ac-access.private-property", ArmorClassAccessWritesPrivateProperty),
            Case("ac-access.field", ArmorClassAccessWritesField),
            Case("ac-access.ambiguous-rejected", ArmorClassAccessRejectsAmbiguousTargetAc),
            Case("evidence.catalog-count", EvidenceCatalogCount),
            Case("evidence.catalog-severity-counts", EvidenceCatalogSeverityCounts),
            Case("evidence.catalog-order", EvidenceCatalogOrder),
            Case("evidence.catalog-unique-ids", EvidenceCatalogUniqueIds),
            Case("evidence.catalog-reproduction-ids", EvidenceCatalogReproductionIds),
            Case("evidence.catalog-require-known", EvidenceCatalogRequireKnown),
            Case("evidence.catalog-require-unknown", EvidenceCatalogRequireUnknown),
            Case("evidence.observation-valid", EvidenceObservationValid),
            Case("evidence.observation-invalid-sequence", EvidenceObservationInvalidSequence),
            Case("evidence.observation-unknown-step", EvidenceObservationUnknownStep),
            Case("evidence.observation-nonutc", EvidenceObservationNonUtc),
            Case("evidence.observation-invalid-hash", EvidenceObservationInvalidHash),
            Case("evidence.observation-uppercase-hash", EvidenceObservationUppercaseHash),
            Case("evidence.evaluation-empty", EvidenceEvaluationEmpty),
            Case("evidence.evaluation-critical-fail", EvidenceEvaluationCriticalFail),
            Case("evidence.evaluation-blocked", EvidenceEvaluationBlocked),
            Case("evidence.evaluation-single-pass-incomplete", EvidenceEvaluationSinglePassIncomplete),
            Case("evidence.evaluation-reproduced-go", EvidenceEvaluationReproducedGo),
            Case("evidence.evaluation-high-fail-warning", EvidenceEvaluationHighFailWarning),
            Case("evidence.evaluation-latest-fail", EvidenceEvaluationLatestFail),
            Case("evidence.evaluation-same-run-not-reproduced", EvidenceEvaluationSameRunNotReproduced),
            Case("evidence.evaluation-null-observation", EvidenceEvaluationNullObservation),
            Case("evidence.evaluation-duplicate-sequence", EvidenceEvaluationDuplicateSequence),
            Case("evidence.evaluation-format", EvidenceEvaluationFormat),
            Case("preflight.probe-negative-bootstrap-count", PreflightProbeNegativeBootstrapCount),
            Case("preflight.probe-negative-blueprint-count", PreflightProbeNegativeBlueprintCount),
            Case("preflight.probe-invalid-expected-count", PreflightProbeInvalidExpectedCount),
            Case("preflight.probe-negative-identity-count", PreflightProbeNegativeIdentityCount),
            Case("preflight.check-only-i01-i02", PreflightCheckOnlyI01I02),
            Case("preflight.report-order-required", PreflightReportOrderRequired),
            Case("preflight.report-all-passed", PreflightReportAllPassed),
            Case("preflight.report-require", PreflightReportRequire),
            Case("preflight.evaluate-pass-guid", PreflightEvaluatePassGuid),
            Case("preflight.evaluate-pass-string", PreflightEvaluatePassString),
            Case("preflight.evaluate-bootstrap-blocked", PreflightEvaluateBootstrapBlocked),
            Case("preflight.evaluate-bootstrap-not-initialized", PreflightEvaluateBootstrapNotInitialized),
            Case("preflight.evaluate-bootstrap-duplicate-init", PreflightEvaluateBootstrapDuplicateInitialization),
            Case("preflight.evaluate-bootstrap-count-mismatch", PreflightEvaluateBootstrapCountMismatch),
            Case("preflight.evaluate-identity-blocked", PreflightEvaluateIdentityBlocked),
            Case("preflight.evaluate-identity-missing", PreflightEvaluateIdentityMissing),
            Case("preflight.evaluate-identity-duplicate", PreflightEvaluateIdentityDuplicate),
            Case("preflight.evaluate-identity-unreadable", PreflightEvaluateIdentityUnreadable),
            Case("preflight.evaluate-identity-unsupported-type", PreflightEvaluateIdentityUnsupportedType),
            Case("preflight.format", PreflightFormat),
            Case("generic.reload-normal", GenericReloadNormal),
            Case("generic.reload-broken", GenericReloadBroken),
            Case("generic.reload-wrecked-rejected", GenericReloadWreckedRejected),
            Case("generic.reload-loaded-rejected", GenericReloadLoadedRejected),
            Case("generic.reload-missing-resources", GenericReloadMissingResourcesRejected),
            Case("generic.overhaul-wrecked", GenericOverhaulWrecked),
            Case("generic.overhaul-broken-rejected", GenericOverhaulBrokenRejected),
            Case("generic.repair-broken", GenericRepairBroken),
            Case("generic.repair-loaded-rejected", GenericRepairLoadedRejected),
            Case("generic.repair-missing-kit", GenericRepairMissingKitRejected),
            Case("generic.unknown-action", GenericUnknownActionRejected),
            Case("reload-profile.ammunition-identity", ReloadProfileAmmunitionIdentity)
        };

        private static int Main()
        {
            int failures = 0;
            Console.WriteLine("Kingmaker Gunslinger domain, firearm-state, and combat-rule tests");

            foreach (TestCase testCase in Cases)
            {
                try
                {
                    testCase.Body();
                    Console.WriteLine("PASS " + testCase.Name);
                }
                catch (Exception exception)
                {
                    failures++;
                    Console.Error.WriteLine("FAIL " + testCase.Name + ": " + exception);
                }
            }

            Console.WriteLine(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Completed {0} tests; failures={1}.",
                    Cases.Length,
                    failures));
            return failures == 0 ? 0 : 1;
        }

        private static string ThirdPlaytestSource(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void MusketMasterStarterSkeleton()
        {
            string archetypeSource = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/MusketMasterBlueprints.cs");
            string source = archetypeSource + ThirdPlaytestSource(
                "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs");
            foreach (string token in new[] {
                "archetype.ReplaceStartingEquipment = true",
                "productionFirearms.Musket.Item",
                "basicAmmunition.BlackPowder",
                "basicAmmunition.LeadBall",
                "gunsmithingSupplies.GunsmithKit",
                "Entry(1, g.Proficiencies, g.Dodge.Feature, g.DeedTiers[0])",
                "Entry(3, g.UtilityShot.Feature)",
                "Entry(5, g.GunTraining.Selection)",
                "Entry(17, g.GunTraining.Selection)",
                "Entry(5, training.Musket)",
                "Entry(17, training.Musket)" })
                Assertions.True(source.Contains(token),
                    "Musket Master skeleton lacks exact token: " + token);
            Assertions.False(archetypeSource.Contains("TestMusket"),
                "Musket Master skeleton references the development Test Musket.");
            Assertions.False(archetypeSource.Contains("HeavyCrossbow"),
                "Musket Master skeleton references the donor Heavy Crossbow.");
            string observerSource = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            foreach (string token in new[] {
                "musket-master-registration",
                "musket-master-starting-items",
                "musket-master-replacement-rows",
                "musket-master-starter-resolver",
                "steady-aim-blueprint-contract",
                "GunslingerStartingFirearmResolver.MatchesConfiguration" })
                Assertions.True(observerSource.Contains(token),
                    "Musket Master runtime observer lacks exact token: " + token);
            int classObserver = observerSource.IndexOf(
                "private RuntimeTestResult RunClassBlueprintContractObservation()",
                StringComparison.Ordinal);
            int creationObserver = observerSource.IndexOf(
                "private RuntimeTestResult RunCharacterCreationContractObservation()",
                StringComparison.Ordinal);
            Assertions.True(classObserver >= 0 && creationObserver > classObserver &&
                observerSource.Substring(classObserver,
                    creationObserver - classObserver).Contains(
                        "AddMusketMasterBlueprintAssertions(assertions)"),
                "Musket Master assertions are not invoked by the exact class observer.");
        }

        private static void PistoleroReplacementSkeleton()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/PistoleroBlueprints.cs");
            foreach (string token in new[] {
                "result.ReplaceStartingEquipment = false",
                "Entry(1, g.Proficiencies, g.Deadeye.Feature, g.DeedTiers[0])",
                "Entry(7, g.StartlingShot.Feature, g.DeedTiers[1])",
                "Entry(11, g.BleedingWound.Feature, g.DeedTiers[2])",
                "Entry(7, g.Deadeye.Feature, tiers[1])",
                "Entry(5, training.Pistol)",
                "Entry(17, training.Pistol)",
                "Pistolero Deeds — Level" })
                Assertions.True(source.Contains(token),
                    "Pistolero skeleton lacks exact token: " + token);
            Assertions.False(source.Contains("ProductionFirearms.Musket"),
                "Pistolero skeleton references the Musket starter.");
            Assertions.False(source.Contains("new BlueprintItem"),
                "Pistolero skeleton manufactures starting equipment.");
            string observer = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            foreach (string token in new[] { "pistolero-registration",
                "pistolero-replacement-rows", "pistolero-starter-resolver",
                "project-archetype-order", "up-close-and-deadly-blueprint-contract",
                "twin-shot-knockdown-blueprint-contract",
                "AddPistoleroBlueprintAssertions(assertions)" })
                Assertions.True(observer.Contains(token),
                    "Pistolero guarded observer lacks token: " + token);
        }

        private static void ThirdPlaytestNativeParentOnly()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/FirearmFeatBlueprints.cs");
            Assertions.True(source.Contains(
                "publication.Publish(set.RapidReload") &&
                !source.Contains("set.RapidReload, set.ExoticWeaponProficiency"),
                "Rapid Reload is not the sole authorized standalone firearm feat.");
            Assertions.False(source.Contains(
                "set.WeaponFocus, set.NativeWeaponFocusWithFirearms"),
                "A hidden Weapon Focus compatibility wrapper is still published at top level.");
        }

        private static void ThirdPlaytestNativeIconGuard()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs");
            Assertions.True(source.Contains(
                "if (!factName.StartsWith(\"KMG_\", StringComparison.Ordinal)) return;"),
                "Native blueprint icons are not guarded from repainting.");
        }

        private static void ThirdPlaytestFirearmParameterMenu()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs");
            Assertions.True(source.Contains("new FeatureParam(parameter)") &&
                source.Contains("GetFullSelectionItems") &&
                source.Contains("\"ExtractSelectionItems\"") &&
                source.Contains("IEnumerable<IFeatureSelectionItem>") &&
                source.Contains("NativeFirearmParametrizedBonus") &&
                source.Contains("OrderBy(value => value == null ? string.Empty : value.Name") &&
                source.Contains("StringComparer.CurrentCultureIgnoreCase") &&
                source.Contains("return kind.ToString();") &&
                !source.Contains("Advanced Rifle"),
                "Native firearm parameters are not merged alphabetically with the exact firearm labels.");
        }

        private static void ThirdPlaytestLegacyWrapperHidden()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/FirearmFeatBlueprints.cs");
            Assertions.True(source.Contains("wrapper.HideInUI = true") &&
                source.Contains("dependentSelections[family].HideInUI = true"),
                "Legacy feat wrappers are still player-facing.");
        }

        private static void ThirdPlaytestReloadOnePublic()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/ReloadTestMusketAbilityBlueprints.cs");
            Assertions.True(!source.Contains("new AbilityVariants") &&
                source.Contains("CreateDynamic"),
                "Reload still exposes its four implementation variants.");
        }

        private static void ThirdPlaytestReloadDynamicAction()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Reloading/ReloadAbilityPresentationPatches.cs");
            Assertions.True(source.Contains("get_ActionType") &&
                source.Contains("get_RuntimeActionType") &&
                source.Contains("get_RequireFullRoundAction"),
                "The public reload ability does not expose its real action cost.");
        }

        private static void ThirdPlaytestDodgeNoProne()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/GunslingerDodgeRuntime.cs") +
                ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/GunslingerDodgeProneAbilityLogic.cs");
            Assertions.True(!source.Contains("AddCondition(UnitCondition.Prone") &&
                source.Contains("m_ArmorClassBuff"),
                "Gunslinger's Dodge still applies Prone.");
        }

        private static void ThirdPlaytestDodgeOneRoundTwoAc()
        {
            string blueprints = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/GunslingerDodgeBlueprints.cs");
            string modifier = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/GunslingerDodgeArmorClassBonus.cs");
            string expiration = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/GunslingerDodgeExpirationPatch.cs");
            Assertions.True(blueprints.Contains("GunslingerDodgeArmorClassBonus") &&
                blueprints.Contains("AbilityCasterHasNoFacts") &&
                blueprints.Contains("AbilityEffectRunAction") &&
                blueprints.Contains("ContextActionApplyBuff") &&
                blueprints.Contains("DurationRate.Rounds") &&
                blueprints.Contains("DiceType.Zero") &&
                blueprints.Contains("DiceCountValue = 0") &&
                blueprints.Contains("BonusValue = 1") &&
                blueprints.Contains("applyBuff.ToCaster = true") &&
                blueprints.Contains("applyBuff.Permanent = false") &&
                blueprints.Contains("applyBuff.IsNotDispelable = true") &&
                blueprints.Contains("applyBuff.AsChild = false") &&
                blueprints.Contains("result.IsClassFeature = false") &&
                blueprints.Contains("ability.ComponentsArray.Length != 4") &&
                !blueprints.Contains(
                    "GunslingerDodgeProneAbilityLogic.Create(marker, armorClassBuff)") &&
                modifier.Contains("internal const int Bonus = 2") &&
                modifier.Contains("ModifierDescriptor.Dodge") &&
                modifier.Contains("Owner.Stats.AC.AddModifier") &&
                modifier.Contains("Owner.Stats.AC.RemoveModifier") &&
                expiration.Contains("HarmonyPatch(typeof(BuffCollection), \"Tick\")") &&
                expiration.Contains("__instance.GetBuff(dodge.ArmorClassBuff)") &&
                expiration.Contains("expiredByTimeLeft") &&
                expiration.Contains("expiredByEndTime") &&
                expiration.Contains("__instance.RemoveFact(buff)") &&
                expiration.Contains("expiration.guard.removed"),
                "The adapted Dodge bonus is not +2 dodge AC with bounded expiration.");
        }

        private static void ThirdPlaytestGritSharedUi()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Grit/GritAbilityUiIntegration.cs");
            Assertions.True(source.Contains("AbilityResourceLogic") &&
                source.Contains("RequiredResource = grit") &&
                source.Contains("IsSpendResource = true") &&
                source.Contains("class GritAbilityResourceUiLogic") &&
                source.Contains("public override void Spend"),
                "Paid deeds do not expose the shared native counter without double-spending.");
        }

        private static void ThirdPlaytestEmptyPreconstruction()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Firing/EmptyFirearmAttackCommandPatch.cs");
            Assertions.True(source.Contains("UnitAttack.CreateAttackCommand") &&
                source.Contains("result = null") && source.Contains("return false") &&
                source.Contains("typeof(UnitUseAbility).GetMethod(\"OnEnded\"") &&
                source.Contains("ReferenceEquals(resolved.Weapon, pending.FirearmWeapon)") &&
                source.Contains("turn.ActionsStates.Standard.CanUse") &&
                source.Contains("return !isTurnBased || (hasCurrentTurn && standardActionAvailable)") &&
                source.Contains("executor.Commands.AddToQueue(attack)"),
                "Empty firearm rejection or exact-item native reload continuation is incomplete.");
        }

        private static void FourthPlaytestOverhaulMaintenance()
        {
            string logic = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/OverhaulTestMusketAbilityLogic.cs");
            string runtime = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/OverhaulTestMusketRuntime.cs");
            string blueprint = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/OverhaulTestMusketAbilityBlueprints.cs");
            Assertions.True(logic.Contains("WorkDurationSeconds = 60f") &&
                logic.Contains("TimeController.GameTime < completion") &&
                logic.Contains("ReferenceEquals(completed.Weapon, start.Weapon)") &&
                runtime.Contains("caster.Unit.IsInCombat") &&
                blueprint.Contains("one uninterrupted minute out of combat") &&
                blueprint.Contains("\"1 minute\""),
                "Overhaul is not a one-minute, out-of-combat, exact-item atomic action.");
        }

        private static void FourthPlaytestConditionPresentation()
        {
            string normal = FirearmConditionPresentation.Describe(FirearmCondition.Normal);
            string broken = FirearmConditionPresentation.Describe(FirearmCondition.Broken);
            string wrecked = FirearmConditionPresentation.Describe(FirearmCondition.Wrecked);
            Assertions.True(normal.Contains("Normal") && normal.Contains("ordinary use"),
                "Normal firearm condition presentation is not meaningful.");
            Assertions.True(broken.Contains("Broken") && broken.Contains("increases by 4") &&
                broken.Contains("Quick Clear") && broken.Contains("Repair Firearm"),
                "Broken firearm condition presentation omits mechanical recovery guidance.");
            Assertions.True(wrecked.Contains("Wrecked") && wrecked.Contains("cannot fire or reload") &&
                wrecked.Contains("one uninterrupted minute") && wrecked.Contains("out of combat"),
                "Wrecked firearm condition presentation omits restrictions or Overhaul guidance.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmConditionPresentation.Describe((FirearmCondition)99),
                "Unknown firearm conditions must fail closed.");
            FirearmDefinition pistol = FirearmDefinitions.CreateEarlyPistol();
            string normalQualities = FirearmConditionPresentation.DescribeQualities(
                pistol, FirearmState.CreateEmpty());
            string brokenQualities = FirearmConditionPresentation.DescribeQualities(
                pistol, FirearmStateMachine.ApplyMisfireDamage(
                    FirearmState.CreateEmpty()));
            Assertions.True(normalQualities.Contains("Firearm, Early, One-Handed") &&
                normalQualities.Contains("Capacity 1") &&
                normalQualities.Contains("Condition: Normal") &&
                !normalQualities.Contains("<null>"),
                "Normal firearm qualities are incomplete.");
            Assertions.True(brokenQualities.Contains("Misfire 5") &&
                brokenQualities.Contains("Condition: Broken") &&
                !brokenQualities.Contains("<null>"),
                "Broken firearm qualities are incomplete.");
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmConditionPresentation.DescribeQualities(null,
                    FirearmState.CreateEmpty()),
                "Null firearm definition qualities must fail closed.");
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmConditionPresentation.DescribeQualities(pistol, null),
                "Null firearm state qualities must fail closed.");
            string quickClear = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/QuickClearAbilityLogic.cs");
            Assertions.True(quickClear.Contains("exactly one Broken firearm") &&
                quickClear.Contains("at least 1 Grit") &&
                quickClear.Contains("Wrecked firearms require Overhaul Firearm"),
                "Quick Clear unavailable guidance is not player-readable.");
            string combatLog = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Firearms/FirearmConditionCombatLog.cs");
            string misfire = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs");
            string deadShot = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/DeadShotRuntime.cs");
            string scatter = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Scatter/ScatterShotRuntime.cs");
            string quickClearRuntime = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Deeds/QuickClearRuntime.cs");
            string repair = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/RepairTestMusketRuntime.cs");
            string overhaul = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/OverhaulTestMusketRuntime.cs");
            Assertions.True(combatLog.Contains(
                    "EventBus.RaiseEvent<IWarningNotificationUIHandler>") &&
                combatLog.Contains("handler.HandleWarning(message, false)") &&
                combatLog.Contains("condition: {1} -> {2} ({3}).") &&
                misfire.Contains("FirearmConditionCombatLog.Publish") &&
                deadShot.Contains("FirearmConditionCombatLog.Publish") &&
                scatter.Contains("FirearmConditionCombatLog.Publish") &&
                quickClearRuntime.Contains("FirearmConditionCombatLog.Publish") &&
                repair.Contains("FirearmConditionCombatLog.Publish") &&
                overhaul.Contains("FirearmConditionCombatLog.Publish"),
                "Not every production condition transition publishes one native combat-log notification.");
        }

        private static void FifthPlaytestItemVisualModel()
        {
            string production = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs");
            string observer = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(production.Contains(
                    "FirearmWeaponPresentation.Apply(clone, spec.Definition,") &&
                production.Contains("FirearmProjectileBlueprints.Register(registry, lightType)") &&
                observer.Contains("itemVisual=\" + itemVisual") &&
                observer.Contains("itemMatch && itemVisual && itemIconDistinct"),
                "Firearm item-level hand-slot visuals can regress to the inherited crossbow model.");
        }

        private static void FifthPlaytestAudibleShotEmitter()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Audio/FirearmSoundRuntime.cs");
            string assets = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs");
            Assertions.True(source.Contains("AkSoundEngine.PostEvent") &&
                source.Contains("AkBankManager.LoadBank") &&
                source.Contains("AK_INVALID_PLAYING_ID") == false &&
                source.Contains("id!=0") &&
                !assets.Contains("AudioSource") && !assets.Contains("AudioClip") &&
                !assets.Contains("PlayOneShot") && !assets.Contains("KMG_FirearmAudio"),
                "Firearm discharge must use native Wwise and retain no Unity playback backend.");
        }

        private static void NativeRigRuntimeCapability()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs");
            Assertions.True(source.Contains("TryPrepareRig") &&
                source.Contains("root-not-identity") &&
                source.Contains("muzzle-not-forward-positive-z") &&
                source.Contains("support-target-missing") &&
                source.Contains("support-target-implausible") &&
                source.Contains("prefab.AddComponent<EquipmentOffsets>()") &&
                source.Contains("offsets.IkTargetLeftHand = support") &&
                source.Contains("Replace(Capabilities, capabilities)") &&
                source.Contains("HasValidatedPrefab"),
                "Runtime rig preparation must validate each prefab independently and assign exact native left-hand IK before publication.");
        }

        private static void NativeRigReadinessFailsClosed()
        {
            string profile = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs");
            Assertions.True(profile.Contains("NativeFallback = 0") &&
                profile.Contains("AutonomousCandidate = 1") &&
                profile.Contains("HumanAccepted = 2") &&
                profile.Contains("FirearmAssetRuntime.HasValidatedPrefab(Kind)") &&
                profile.Contains("FirearmKind.Musket, FirearmPresentationReadiness.AutonomousCandidate") &&
                !profile.Contains("FirearmKind.Musket, FirearmPresentationReadiness.HumanAccepted"),
                "Presentation readiness must require validated capability; Musket is autonomous-candidate only and no weapon may claim human acceptance.");
        }

        private static void NativeRigObserverGuarded()
        {
            string catalog = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");
            string runner = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            string automation = ThirdPlaytestSource(
                "scripts/RuntimeAutomation.Common.ps1");
            Assertions.True(catalog.Contains(
                    "observe-native-firearm-rig-contracts") &&
                runner.Contains("RunNativeFirearmRigContractObservation") &&
                runner.Contains("EquipmentOffsets.IkTargetLeftHand") &&
                runner.Contains("DestroyImmediate(lightInstance)") &&
                runner.Contains("DestroyImmediate(heavyInstance)") &&
                runner.Contains("production-readiness-remains-fallback") &&
                automation.Contains("'observe-native-firearm-rig-contracts'") &&
                automation.Contains("ReadinessBehavior = 'mod-load'"),
                "Native donor observation must be allowlisted, save-free, cleanup-owned, and retain production fallback.");
        }

        private static void NativeRigCalibrationSession()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Development/FirearmVisualCalibration.cs");
            Assertions.True(source.Contains("Dictionary<FirearmKind, FirearmCalibrationState> Session") &&
                source.Contains("ExactEquippedFirearmResolver.TryResolve") &&
                source.Contains("FindUnique") && source.Contains("EquipmentOffsets") &&
                source.Contains("ReferenceEquals(offsets.IkTargetLeftHand, support)") &&
                source.Contains("capability.VisualPosition.HasValue") &&
                source.Contains("capability.VisualScale.HasValue") &&
                source.Contains("capability.ButtPosition.HasValue") &&
                source.Contains("butt.localPosition = state.ButtPosition") &&
                source.Contains("ResetAll()"),
                "Calibration must be per-kind, exact-firearm filtered, instance-local, native-IK checked, and resettable.");
        }

        private static void NativeRigCalibrationExport()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Development/FirearmVisualCalibration.cs");
            Assertions.True(source.Contains("SchemaVersion = 1") &&
                source.Contains("humanAccepted\\\": false") &&
                source.Contains("development\", \"firearm-calibration") &&
                source.Contains("new UTF8Encoding(false)") &&
                source.Contains("IsFinite()"),
                "Calibration export must be deterministic, finite, development-scoped, and never claim human acceptance.");
        }

        private static void NativeRigAnimationAllowlist()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Development/FirearmVisualCalibration.cs");
            Assertions.True(source.Contains("WeaponAnimationStyle.PiercingOneHanded") &&
                source.Contains("WeaponAnimationStyle.Fencing") &&
                source.Contains("WeaponAnimationStyle.Dagger") &&
                source.Contains("WeaponAnimationStyle.Crossbow") &&
                !source.Contains("ThrownStraight"),
                "The calibration lab must expose only the mission allowlist and exclude ThrownStraight.");
        }

        private static void NativeRigCalibrationNativeRefresh()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Development/FirearmVisualCalibration.cs");
            Assertions.True(source.Contains("Dictionary<FirearmKind, GameObject> NativeModels") &&
                source.Contains("NativeModels[kind] = visual.Model") &&
                source.Contains("unit.View.HandsEquipment.UpdateAll()") &&
                source.Contains("native fallback restored") &&
                !source.Contains("NativeModels[kind] = null"),
                "Development toggle must retain and restore the non-null native model through the native hands-equipment refresh.");
        }

        private static void NativeRigMusketCandidate()
        {
            string profile = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs");
            string runner = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(profile.Contains(
                    "FirearmKind.Musket, FirearmPresentationReadiness.AutonomousCandidate") &&
                !profile.Contains("FirearmKind.Musket, FirearmPresentationReadiness.HumanAccepted") &&
                runner.Contains("RunDisposableFirearmVisualRigs") &&
                runner.Contains("id + \"-native-left-hand-ik\"") &&
                runner.Contains("human-visual-gate") &&
                runner.Contains("grip/clipping/scale/pose/animation require human review"),
                "Musket may be an autonomous candidate only with guarded structural IK evidence and an explicit human visual gate.");
        }

        private static void NativeRigLongGunCandidates()
        {
            string profile = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs");
            string runner = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(profile.Contains("FirearmKind.Blunderbuss, FirearmPresentationReadiness.AutonomousCandidate") &&
                profile.Contains("FirearmKind.Rifle, FirearmPresentationReadiness.AutonomousCandidate") &&
                runner.Contains("AppendLongGunRigAssertions(assertions, FirearmKind.Blunderbuss") &&
                runner.Contains("AppendLongGunRigAssertions(assertions, FirearmKind.Rifle") &&
                runner.Contains("ReferenceEquals(offsets.IkTargetLeftHand, support)"),
                "Blunderbuss and Rifle require independent candidate profiles and exact per-instance native IK assertions.");
        }

        private static void NativeRigShortGunCandidates()
        {
            string profile = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs");
            string runner = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(profile.Contains("FirearmKind.Pistol, FirearmPresentationReadiness.AutonomousCandidate") &&
                profile.Contains("FirearmKind.Revolver, FirearmPresentationReadiness.AutonomousCandidate") &&
                profile.Contains("WeaponAnimationStyle.PiercingOneHanded") &&
                runner.Contains("AppendShortGunRigAssertions(assertions, FirearmKind.Pistol") &&
                runner.Contains("AppendShortGunRigAssertions(assertions, FirearmKind.Revolver") &&
                runner.Contains("support == null") && !profile.Contains("ThrownStraight"),
                "Pistol and Revolver require independent no-support rigs and the allowlisted PiercingOneHanded candidate, never ThrownStraight.");
        }

        private static void NativeRigObsoleteScanRetired()
        {
            string retired = System.IO.Path.Combine(Environment.CurrentDirectory,
                "src/KingmakerGunslinger/Assets/FirearmVisualEquipmentHandler.cs");
            string project = ThirdPlaytestSource(
                "src/KingmakerGunslinger/KingmakerGunslinger.csproj");
            Assertions.True(!System.IO.File.Exists(retired) &&
                !project.Contains("FirearmVisualEquipmentHandler"),
                "The whole-character renderer-name scan must remain deleted and uncompiled.");
        }

        private static void NativeRigHolsterHiddenExact()
        {
            string profile = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs");
            string presentation = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/FirearmWeaponPresentation.cs");
            string lifecycle = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmHiddenHolsterPatch.cs");
            Assertions.True(profile.Contains("enum FirearmHolsterPolicy") &&
                profile.Contains("NativeFallback = 0") &&
                profile.Contains("Custom = 1") &&
                profile.Contains("Hidden = 2") &&
                profile.Contains("FirearmKind.Musket") &&
                profile.Contains("FirearmHolsterPolicy.Hidden, null, false") &&
                presentation.Contains("if (profile.HideHolsteredModel)") &&
                presentation.Contains("Set(visual, \"m_WeaponBeltModel\", null)") &&
                presentation.Contains("Set(visual, \"m_WeaponSheathModel\", null)") &&
                presentation.Contains("Materialize(visual, \"m_PossibleAttachSlots\", source.AttachSlots)") &&
                !presentation.Contains("SetEmptyCollection") &&
                !presentation.Contains("Set(visual, \"m_OverrideAttachSlots\", true)") &&
                lifecycle.Contains("ReattachSheath") &&
                lifecycle.Contains("DestroySheathModel") &&
                lifecycle.Contains("KingmakerFirearmRuntimeItemResolver") &&
                lifecycle.Contains("profile.Holster != FirearmHolsterPolicy.Hidden") &&
                lifecycle.Contains("!profile.IsLongGun") &&
                presentation.Contains("Native crossbow") &&
                !presentation.Contains("GetComponentsInChildren<Renderer>") &&
                !lifecycle.Contains("Renderer"),
                "Hidden long-gun holsters must clear attach models and exact firearm sheath lifecycle without renderer scanning or native donor mutation.");
        }

        private static void NativeRigVisibilityRepair()
        {
            string builder = ThirdPlaytestSource("tools/unity/BuildFirearmBundles.cs");
            string runtime = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs");
            string runner = ThirdPlaytestSource(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(builder.Contains(
                    "new Vector3(0f, 180f, 180f), 0.24f") &&
                builder.Contains("RetainHighestDetailRenderers(visual, spec)") &&
                builder.Contains("policy=retain-lod0-and-remove-lodgroup") &&
                builder.Contains("KMG_RIG_RENDERER") &&
                builder.Contains("mesh=") && builder.Contains("materials=") &&
                builder.Contains("MakeHeldLongGunMeshesTwoSided") &&
                builder.Contains("policy=opaque-standard-with-reversed-backfaces") &&
                builder.Contains("material.shader = Shader.Find(\"Standard\")") &&
                builder.Contains("vertices=") && builder.Contains("normals=") &&
                builder.Contains("ValidateVisibleScales") &&
                builder.Contains("KMG_RIG_BINDING") &&
                builder.Contains("KMG_RIG_TRANSFORM") &&
                builder.Contains("KMG_RIG_BOUNDS") &&
                builder.Contains("RemoveDuplicatePreviewGeometry") &&
                builder.Contains("retain-unsuffixed-low-poly-assembly") &&
                builder.Contains("\"model.dae\"") &&
                builder.Contains("\"Final2 Sketchfab.fbx\"") &&
                builder.Contains("Pistol equipped Visual must carry the isolated 180-degree roll correction") &&
                runtime.Contains("lod-group-present") &&
                runtime.Contains("negative-mirrored-zero-or-nonfinite-scale") &&
                runtime.Contains("renderer-disabled-or-inactive") &&
                runtime.Contains("non-opaque-standard-shader") &&
                runner.Contains("DescribeFirearmRenderers") &&
                runner.Contains("-visible-renderers") &&
                runner.Contains("visibleCount=") &&
                runner.Contains("boundsSize=") &&
                runner.Contains("runtime-loaded AssetBundle prefab renderer/material/bounds audit"),
                "Finishing repair must prove source binding, isolate Pistol roll, clean Revolver duplicates, retain LOD0, generate opaque two-sided long-gun geometry, audit hierarchy/bounds, and reject inactive renderers.");
            Assertions.True(builder.Contains(
                    "Spec(\"Pistol\", \"Pistol\", \"model.dae\", false, false,\n            new Vector3(0f, 0f, 0.1632f), new Vector3(0f, 180f, 180f), 0.24f") &&
                builder.Contains("HasSemanticAnchors") &&
                builder.Contains("SourceGripPoint") &&
                builder.Contains("SourceSupportPoint") &&
                builder.Contains("SourceButtPoint") &&
                builder.Contains("SourceMuzzlePoint") &&
                builder.Contains("AnchorRelativeToGrip") &&
                builder.Contains("KMG_RIG_ANCHORS") &&
                builder.Contains("new Vector3(0.0400f, 0f, 0f)") &&
                builder.Contains("new Vector3(-0.1000f, -0.0122f, -0.0074f)") &&
                builder.Contains("new Vector3(0.0100f, 0f, -0.00316f)") &&
                builder.Contains("new Vector3(0.1300f, 0f, 0f)") &&
                runtime.Contains("semantic-length-or-butt-implausible") &&
                runner.Contains("-semantic-anchors") &&
                runner.Contains("long-gun-relative-semantic-length") &&
                runner.Contains("pistol-human-accepted-held-freeze") &&
                runner.Contains("narrow 2026-08-07 held-appearance freeze; other states unaccepted"),
                "Accepted Pistol freeze or source-space long-gun semantic anchor contract is missing.");
            Assertions.True(!System.IO.File.Exists(System.IO.Path.Combine(
                Environment.CurrentDirectory,
                "tools/unity/KmgDoubleSidedDiffuse.shader")),
                "The runtime-invisible custom held shader must remain retired.");
        }

        private static void FirearmAudioDischargeRouteShape()
        {
            string ordinary=ThirdPlaytestSource("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs");
            string scatter=ThirdPlaytestSource("src/KingmakerGunslinger/Scatter/ScatterShotRuntime.cs");
            string dead=ThirdPlaytestSource("src/KingmakerGunslinger/Deeds/DeadShotRuntime.cs");
            string startling=ThirdPlaytestSource("src/KingmakerGunslinger/Deeds/StartlingShotRuntime.cs");
            string menacing=ThirdPlaytestSource("src/KingmakerGunslinger/Deeds/MenacingShotAbilityLogic.cs");
            string bleeding=ThirdPlaytestSource("src/KingmakerGunslinger/Deeds/StopBleedingRuntime.cs");
            Assertions.True(ordinary.Contains("ordinary-attack") && ordinary.Contains("if (!decision.IsMisfire)") &&
                scatter.Contains("scatter-shot") && scatter.Contains("if (volley.AllRollsMisfire)") &&
                dead.Contains("if (!outcome.Misfires)") && dead.Contains("\"dead-shot\"") &&
                startling.Contains("RecordApplied();") && startling.Contains("\"startling-shot\"") &&
                menacing.Contains("completed = true;") && menacing.Contains("\"menacing-shot\"") &&
                bleeding.Contains("RecordApplied();") && bleeding.Contains("\"stop-bleeding\""),
                "A committed physical discharge route is missing its post-commit Wwise notification gate.");
        }

        private static TestCase Case(string name, Action body)
        {
            return new TestCase(name, body);
        }

        private static ReloadProfile StandardReload(int rounds)
        {
            return new ReloadProfile(ReloadActionType.Standard, true, rounds);
        }

        private static ReloadProfile FullRoundReload(int rounds)
        {
            return new ReloadProfile(ReloadActionType.FullRound, true, rounds);
        }

        private static ReloadProfile MoveReload(int rounds)
        {
            return new ReloadProfile(ReloadActionType.Move, true, rounds);
        }

        private static FirearmDefinition EarlyMusket()
        {
            return FirearmDefinitions.CreateEarlyMusket();
        }

        private static AmmunitionId LeadBall()
        {
            return new AmmunitionId("kmg.ammunition.lead-ball");
        }

        private static AmmunitionId PaperCartridge()
        {
            return new AmmunitionId("kmg.ammunition.paper-cartridge");
        }

        private static AmmunitionId AlchemicalRound()
        {
            return new AmmunitionId("kmg.ammunition.alchemical-round");
        }

        private static FirearmStateRules StateRules(int capacity, params AmmunitionId[] ammunition)
        {
            return new FirearmStateRules(capacity, ammunition);
        }

        private static FirearmState LoadedState(
            int rounds,
            AmmunitionId ammunition,
            FirearmCondition condition)
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                rounds,
                ammunition,
                condition);
        }

        private static FirearmStateTokenCatalog TokenCatalog()
        {
            return FirearmStateTokenCatalog.CreateCapacityOneDiagnostic();
        }

        private static FirearmState TokenLoadedNormal()
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal);
        }

        private static FirearmState TokenBrokenEmpty()
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Broken);
        }

        private static FirearmState TokenBrokenLoaded()
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Broken);
        }

        private static FirearmState TokenWrecked()
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Wrecked);
        }

        private static void AssertTransitionError(
            FirearmStateTransitionError expected,
            Action action,
            string message)
        {
            FirearmStateTransitionException exception =
                Assertions.Throws<FirearmStateTransitionException>(action, message);
            Assertions.Equal(expected, exception.Error, message + " Error code mismatch.");
        }

        private static void AmmunitionSnapshotValid()
        {
            var snapshot = new BasicAmmunitionInventorySnapshot(3, 2);
            Assertions.Equal(3, snapshot.BlackPowderCharges, "Powder count mismatch.");
            Assertions.Equal(2, snapshot.LeadBalls, "Lead-ball count mismatch.");
            Assertions.Equal(2, Math.Min(snapshot.BlackPowderCharges, snapshot.LeadBalls), "Complete-load count mismatch.");
            Assertions.True(snapshot.HasOneLoad, "A 3/2 snapshot must contain a complete load.");
        }

        private static void AmmunitionSnapshotCapture()
        {
            var inventory = new FakeBasicAmmunitionInventory(4, 7);
            BasicAmmunitionInventorySnapshot snapshot =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            Assertions.Equal(4, snapshot.BlackPowderCharges, "Captured powder count mismatch.");
            Assertions.Equal(7, snapshot.LeadBalls, "Captured lead-ball count mismatch.");
        }

        private static void AmmunitionSnapshotValueEquality()
        {
            var first = new BasicAmmunitionInventorySnapshot(4, 7);
            var second = new BasicAmmunitionInventorySnapshot(4, 7);
            var different = new BasicAmmunitionInventorySnapshot(4, 6);
            Assertions.Equal(first, second, "Equal ammunition snapshots must compare equal.");
            Assertions.Equal(first.GetHashCode(), second.GetHashCode(), "Equal snapshots need equal hashes.");
            Assertions.False(first.Equals(different), "Different snapshots must not compare equal.");
        }

        private static void AmmunitionSnapshotFormat()
        {
            Assertions.Equal(
                "blackPowder=5; leadBalls=3; completeLoads=3",
                new BasicAmmunitionInventorySnapshot(5, 3).ToString(),
                "Ammunition snapshot formatting changed.");
        }

        private static void AmmunitionSnapshotNegativePowderRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new BasicAmmunitionInventorySnapshot(-1, 0),
                "Negative powder count must be rejected.");
        }

        private static void AmmunitionSnapshotNegativeBallRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new BasicAmmunitionInventorySnapshot(0, -1),
                "Negative lead-ball count must be rejected.");
        }

        private static void AmmunitionSnapshotUnknownComponentRejected()
        {
            var snapshot = new BasicAmmunitionInventorySnapshot(1, 1);
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => snapshot.Count((BasicAmmunitionComponent)99),
                "Unknown ammunition components must be rejected.");
        }

        private static void AmmunitionSnapshotNullInventoryRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => BasicAmmunitionInventorySnapshot.Capture(null),
                "A null ammunition inventory must be rejected.");
        }

        private static void AmmunitionSnapshotNegativeStoreCountRejected()
        {
            var inventory = new FakeBasicAmmunitionInventory(1, 1)
            {
                ReportNegativePowder = true
            };
            Assertions.Throws<InvalidOperationException>(
                () => BasicAmmunitionInventorySnapshot.Capture(inventory),
                "A runtime inventory returning a negative count must fail closed.");
        }

        private static void AmmunitionTransactionSuccess()
        {
            var inventory = new FakeBasicAmmunitionInventory(1, 1);
            BasicAmmunitionTransactionResult result =
                new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory);
            Assertions.True(result.Succeeded, "A complete load must be consumed.");
            Assertions.Equal(BasicAmmunitionTransactionStatus.Consumed, result.Status, "Transaction status mismatch.");
            Assertions.Equal(new BasicAmmunitionInventorySnapshot(1, 1), result.Before, "Before snapshot mismatch.");
            Assertions.Equal(new BasicAmmunitionInventorySnapshot(0, 0), result.After, "After snapshot mismatch.");
            Assertions.Equal(0, inventory.Powder, "Powder should be consumed exactly once.");
            Assertions.Equal(0, inventory.Balls, "Lead ball should be consumed exactly once.");
        }

        private static void AmmunitionTransactionMultipleCounts()
        {
            var inventory = new FakeBasicAmmunitionInventory(8, 5);
            BasicAmmunitionTransactionResult result =
                new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory);
            Assertions.True(result.Succeeded, "A complete load must be consumed from larger stacks.");
            Assertions.Equal(7, inventory.Powder, "Large powder stack decrement mismatch.");
            Assertions.Equal(4, inventory.Balls, "Large lead-ball stack decrement mismatch.");
        }

        private static void AmmunitionTransactionMissingPowder()
        {
            var inventory = new FakeBasicAmmunitionInventory(0, 4);
            BasicAmmunitionTransactionResult result =
                new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory);
            Assertions.False(result.Succeeded, "Missing powder must reject the transaction.");
            Assertions.Equal(BasicAmmunitionTransactionStatus.InsufficientComponents, result.Status, "Insufficient status mismatch.");
            Assertions.Equal(0, inventory.RemoveCalls, "A rejected transaction must perform no removes.");
            Assertions.Equal(0, inventory.Powder, "Rejected transaction changed powder.");
            Assertions.Equal(4, inventory.Balls, "Rejected transaction changed lead balls.");
        }

        private static void AmmunitionTransactionMissingBall()
        {
            var inventory = new FakeBasicAmmunitionInventory(4, 0);
            BasicAmmunitionTransactionResult result =
                new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory);
            Assertions.False(result.Succeeded, "Missing lead ball must reject the transaction.");
            Assertions.Equal(0, inventory.RemoveCalls, "A rejected transaction must perform no removes.");
            Assertions.Equal(4, inventory.Powder, "Rejected transaction changed powder.");
            Assertions.Equal(0, inventory.Balls, "Rejected transaction changed lead balls.");
        }

        private static void AmmunitionTransactionEmpty()
        {
            var inventory = new FakeBasicAmmunitionInventory(0, 0);
            BasicAmmunitionTransactionResult result =
                new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory);
            Assertions.False(result.Succeeded, "An empty inventory must reject the transaction.");
            Assertions.Equal(result.Before, result.After, "Rejected empty transaction must preserve counts.");
        }

        private static void AmmunitionTransactionNullInventoryRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(null),
                "A null inventory must be rejected.");
        }

        private static void AmmunitionTransactionFirstRemoveFailureRollsBack()
        {
            var inventory = new FakeBasicAmmunitionInventory(3, 3)
            {
                ThrowOnRemoveCall = 1
            };
            BasicAmmunitionTransactionException exception =
                Assertions.Throws<BasicAmmunitionTransactionException>(
                    () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory),
                    "A first-remove failure must be surfaced.");
            Assertions.False(exception.RollbackFailed, "No-op rollback should succeed.");
            Assertions.Equal(3, inventory.Powder, "First-remove failure changed powder.");
            Assertions.Equal(3, inventory.Balls, "First-remove failure changed lead balls.");
        }

        private static void AmmunitionTransactionSecondRemoveFailureRollsBack()
        {
            var inventory = new FakeBasicAmmunitionInventory(3, 3)
            {
                ThrowOnRemoveCall = 2
            };
            BasicAmmunitionTransactionException exception =
                Assertions.Throws<BasicAmmunitionTransactionException>(
                    () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory),
                    "A second-remove failure must be surfaced.");
            Assertions.False(exception.RollbackFailed, "Rollback should restore the first component.");
            Assertions.Equal(3, inventory.Powder, "Rollback did not restore powder.");
            Assertions.Equal(3, inventory.Balls, "Rollback changed lead balls.");
        }

        private static void AmmunitionTransactionAfterMutationFailureRollsBack()
        {
            var inventory = new FakeBasicAmmunitionInventory(3, 3)
            {
                ThrowOnRemoveCall = 2,
                MutateBeforeRemoveFailure = true
            };
            BasicAmmunitionTransactionException exception =
                Assertions.Throws<BasicAmmunitionTransactionException>(
                    () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory),
                    "A post-mutation failure must be surfaced.");
            Assertions.False(exception.RollbackFailed, "Rollback should restore both components.");
            Assertions.Equal(3, inventory.Powder, "Rollback did not restore powder.");
            Assertions.Equal(3, inventory.Balls, "Rollback did not restore lead balls.");
        }

        private static void AmmunitionTransactionVerificationFailureRollsBack()
        {
            var inventory = new FakeBasicAmmunitionInventory(4, 4)
            {
                ExtraPowderRemovedOnFirstRemove = 1
            };
            BasicAmmunitionTransactionException exception =
                Assertions.Throws<BasicAmmunitionTransactionException>(
                    () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory),
                    "An incorrect runtime decrement must fail verification.");
            Assertions.False(exception.RollbackFailed, "Verification rollback should succeed.");
            Assertions.Equal(4, inventory.Powder, "Verification rollback did not restore powder.");
            Assertions.Equal(4, inventory.Balls, "Verification rollback did not restore lead balls.");
        }

        private static void AmmunitionTransactionRollbackFailureSurfaced()
        {
            var inventory = new FakeBasicAmmunitionInventory(2, 2)
            {
                ThrowOnRemoveCall = 2,
                ThrowOnAdd = true
            };
            BasicAmmunitionTransactionException exception =
                Assertions.Throws<BasicAmmunitionTransactionException>(
                    () => new BasicAmmunitionTransactionService().TryConsumeOneLoad(inventory),
                    "A rollback failure must be surfaced distinctly.");
            Assertions.True(exception.RollbackFailed, "Rollback failure flag was not set.");
            Assertions.True(exception.RollbackException != null, "Rollback exception was not retained.");
            Assertions.Equal(1, inventory.Powder, "Synthetic rollback failure should leave the partial mutation observable.");
            Assertions.Equal(2, inventory.Balls, "Synthetic rollback failure changed the unmutated component.");
        }

        private static void AmmunitionResultSuccessValidation()
        {
            var result = new BasicAmmunitionTransactionResult(
                BasicAmmunitionTransactionStatus.Consumed,
                new BasicAmmunitionInventorySnapshot(4, 6),
                new BasicAmmunitionInventorySnapshot(3, 5));
            Assertions.True(result.Succeeded, "Consumed result must report success.");
            Assertions.Throws<ArgumentException>(
                () => new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.Consumed,
                    new BasicAmmunitionInventorySnapshot(4, 6),
                    new BasicAmmunitionInventorySnapshot(2, 5)),
                "Consumed result must reject an incorrect decrement.");
        }

        private static void AmmunitionResultInsufficientValidation()
        {
            var before = new BasicAmmunitionInventorySnapshot(0, 6);
            var result = new BasicAmmunitionTransactionResult(
                BasicAmmunitionTransactionStatus.InsufficientComponents,
                before,
                before);
            Assertions.False(result.Succeeded, "Insufficient result must report failure.");
            Assertions.Throws<ArgumentException>(
                () => new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.InsufficientComponents,
                    before,
                    new BasicAmmunitionInventorySnapshot(0, 5)),
                "Insufficient result must reject changed counts.");
        }

        private static void AmmunitionResultUnknownStatusRejected()
        {
            var snapshot = new BasicAmmunitionInventorySnapshot(1, 1);
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new BasicAmmunitionTransactionResult(
                    (BasicAmmunitionTransactionStatus)99,
                    snapshot,
                    snapshot),
                "Unknown transaction status must be rejected.");
        }

        private static void AmmunitionResultNullSnapshotsRejected()
        {
            var snapshot = new BasicAmmunitionInventorySnapshot(1, 1);
            Assertions.Throws<ArgumentNullException>(
                () => new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.InsufficientComponents,
                    null,
                    snapshot),
                "Null before snapshot must be rejected.");
            Assertions.Throws<ArgumentNullException>(
                () => new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.InsufficientComponents,
                    snapshot,
                    null),
                "Null after snapshot must be rejected.");
        }

        private static void AmmunitionResultFormat()
        {
            var result = new BasicAmmunitionTransactionResult(
                BasicAmmunitionTransactionStatus.Consumed,
                new BasicAmmunitionInventorySnapshot(2, 3),
                new BasicAmmunitionInventorySnapshot(1, 2));
            Assertions.Equal(
                "status=Consumed; before=[blackPowder=2; leadBalls=3; completeLoads=2]; after=[blackPowder=1; leadBalls=2; completeLoads=1]",
                result.ToString(),
                "Transaction result formatting changed.");
        }

        private static FirearmStateRules BasicMusketStateRules()
        {
            return new FirearmStateRules(
                1,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall });
        }

        private static FirearmReloadResult Reload(
            FakeFirearmReloadStateStore stateStore,
            FakeBasicAmmunitionInventory inventory)
        {
            return new FirearmReloadTransactionService().TryReloadOneBasicRound(
                stateStore,
                inventory,
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall);
        }

        private static void ReloadTransactionSuccess()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeBasicAmmunitionInventory(4, 3);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.True(result.Succeeded, "A complete basic load must reload an empty Normal firearm.");
            Assertions.Equal(FirearmReloadStatus.Loaded, result.Status, "Reload status mismatch.");
            Assertions.Equal(1, stateStore.State.LoadedRounds, "The exact firearm must contain one round.");
            Assertions.Equal(FirearmStateTokenCatalog.DiagnosticLeadBall, stateStore.State.LoadedAmmunition, "Loaded ammunition mismatch.");
            Assertions.Equal(3, inventory.Powder, "Reload consumed an incorrect powder count.");
            Assertions.Equal(2, inventory.Balls, "Reload consumed an incorrect Lead Ball count.");
            Assertions.Equal(1, stateStore.ReplaceCalls, "Successful reload must write state exactly once.");
        }

        private static void ReloadTransactionAlreadyLoaded()
        {
            FirearmState loaded = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            var stateStore = new FakeFirearmReloadStateStore(loaded);
            var inventory = new FakeBasicAmmunitionInventory(4, 4);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.Equal(FirearmReloadStatus.AlreadyLoaded, result.Status, "Already-loaded status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Rejected reload must not write firearm state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Rejected reload must not consume inventory.");
        }

        private static void ReloadTransactionBroken()
        {
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            var stateStore = new FakeFirearmReloadStateStore(broken);
            var inventory = new FakeBasicAmmunitionInventory(2, 2);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.True(result.Succeeded, "An empty Broken firearm must remain loadable.");
            Assertions.Equal(FirearmReloadStatus.Loaded, result.Status, "Broken reload status mismatch.");
            Assertions.Equal(FirearmCondition.Broken, stateStore.State.Condition, "Reload repaired the Broken firearm unexpectedly.");
            Assertions.Equal(1, stateStore.State.LoadedRounds, "Broken reload did not load exactly one round.");
            Assertions.Equal(FirearmStateTokenCatalog.DiagnosticLeadBall, stateStore.State.LoadedAmmunition, "Broken reload ammunition mismatch.");
            Assertions.Equal(1, inventory.Powder, "Broken reload consumed an incorrect powder count.");
            Assertions.Equal(1, inventory.Balls, "Broken reload consumed an incorrect Lead Ball count.");
            Assertions.Equal(1, stateStore.ReplaceCalls, "Broken reload must write state exactly once.");
        }

        private static void ReloadTransactionLoadedBrokenAlreadyLoaded()
        {
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmState loadedBroken = FirearmStateMachine.Load(
                broken,
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            var stateStore = new FakeFirearmReloadStateStore(loadedBroken);
            var inventory = new FakeBasicAmmunitionInventory(2, 2);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.Equal(FirearmReloadStatus.AlreadyLoaded, result.Status, "Loaded Broken firearm status mismatch.");
            Assertions.Equal(loadedBroken, stateStore.State, "Already-loaded Broken rejection changed firearm state.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Already-loaded Broken firearm must not be written.");
            Assertions.Equal(0, inventory.RemoveCalls, "Already-loaded Broken firearm consumed ammunition.");
        }

        private static void ReloadTransactionWrecked()
        {
            FirearmState wrecked = FirearmStateMachine.Wreck(FirearmState.CreateEmpty());
            var stateStore = new FakeFirearmReloadStateStore(wrecked);
            var inventory = new FakeBasicAmmunitionInventory(2, 2);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.Equal(FirearmReloadStatus.Wrecked, result.Status, "Wrecked status mismatch.");
            Assertions.Equal(0, inventory.RemoveCalls, "Wrecked rejection consumed ammunition.");
        }

        private static void ReloadTransactionMissingPowder()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeBasicAmmunitionInventory(0, 3);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.Equal(FirearmReloadStatus.InsufficientBlackPowder, result.Status, "Missing-powder status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Missing powder must not write firearm state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Missing powder must not remove inventory.");
        }

        private static void ReloadTransactionMissingBall()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeBasicAmmunitionInventory(3, 0);
            FirearmReloadResult result = Reload(stateStore, inventory);
            Assertions.Equal(FirearmReloadStatus.InsufficientLeadBall, result.Status, "Missing-ball status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Missing Lead Ball must not write firearm state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Missing Lead Ball must not remove inventory.");
        }

        private static void ReloadTransactionNullStateStore()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadTransactionService().TryReloadOneBasicRound(
                    null,
                    new FakeBasicAmmunitionInventory(1, 1),
                    BasicMusketStateRules(),
                    FirearmStateTokenCatalog.DiagnosticLeadBall),
                "A null reload state store must be rejected.");
        }

        private static void ReloadTransactionNullInventory()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadTransactionService().TryReloadOneBasicRound(
                    new FakeFirearmReloadStateStore(FirearmState.CreateEmpty()),
                    null,
                    BasicMusketStateRules(),
                    FirearmStateTokenCatalog.DiagnosticLeadBall),
                "A null reload inventory must be rejected.");
        }

        private static void ReloadTransactionNullRules()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadTransactionService().TryReloadOneBasicRound(
                    new FakeFirearmReloadStateStore(FirearmState.CreateEmpty()),
                    new FakeBasicAmmunitionInventory(1, 1),
                    null,
                    FirearmStateTokenCatalog.DiagnosticLeadBall),
                "Null firearm rules must be rejected.");
        }

        private static void ReloadTransactionNullAmmunition()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadTransactionService().TryReloadOneBasicRound(
                    new FakeFirearmReloadStateStore(FirearmState.CreateEmpty()),
                    new FakeBasicAmmunitionInventory(1, 1),
                    BasicMusketStateRules(),
                    null),
                "Null ammunition identity must be rejected.");
        }

        private static void ReloadTransactionNullState()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ReturnNullOnRead = true
            };
            Assertions.Throws<InvalidOperationException>(
                () => Reload(stateStore, new FakeBasicAmmunitionInventory(1, 1)),
                "A null firearm state must fail closed.");
        }

        private static void ReloadTransactionIncompatibleAmmunition()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeBasicAmmunitionInventory(1, 1);
            Assertions.Throws<FirearmStateTransitionException>(
                () => new FirearmReloadTransactionService().TryReloadOneBasicRound(
                    stateStore,
                    inventory,
                    BasicMusketStateRules(),
                    new AmmunitionId("kmg.ammunition.incompatible")),
                "Incompatible ammunition must be rejected before mutation.");
            Assertions.Equal(0, inventory.RemoveCalls, "Incompatible ammunition consumed inventory.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Incompatible ammunition changed firearm state.");
        }

        private static void ReloadTransactionStateWriteFailureRestoresInventory()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ThrowOnReplaceCall = 1
            };
            var inventory = new FakeBasicAmmunitionInventory(3, 3);
            FirearmReloadTransactionException exception =
                Assertions.Throws<FirearmReloadTransactionException>(
                    () => Reload(stateStore, inventory),
                    "A firearm-state write failure must be surfaced.");
            Assertions.True(exception.RollbackSucceeded, "Inventory rollback should succeed.");
            Assertions.Equal(FirearmState.CreateEmpty(), stateStore.State, "State-write failure changed firearm state.");
            Assertions.Equal(3, inventory.Powder, "State-write failure did not restore powder.");
            Assertions.Equal(3, inventory.Balls, "State-write failure did not restore Lead Balls.");
        }

        private static void ReloadTransactionPostStateMutationFailureRestoresBoth()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true
            };
            var inventory = new FakeBasicAmmunitionInventory(3, 3);
            FirearmReloadTransactionException exception =
                Assertions.Throws<FirearmReloadTransactionException>(
                    () => Reload(stateStore, inventory),
                    "A post-state-mutation failure must be surfaced.");
            Assertions.True(exception.RollbackSucceeded, "Both resources should be restored.");
            Assertions.Equal(FirearmState.CreateEmpty(), stateStore.State, "Firearm rollback failed.");
            Assertions.Equal(3, inventory.Powder, "Powder rollback failed.");
            Assertions.Equal(3, inventory.Balls, "Lead Ball rollback failed.");
            Assertions.Equal(2, stateStore.ReplaceCalls, "The state store should receive write and rollback calls.");
        }

        private static void ReloadTransactionStateRollbackFailureSurfaced()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true,
                ThrowOnSecondReplace = true
            };
            var inventory = new FakeBasicAmmunitionInventory(3, 3);
            FirearmReloadTransactionException exception =
                Assertions.Throws<FirearmReloadTransactionException>(
                    () => Reload(stateStore, inventory),
                    "A state rollback failure must be surfaced.");
            Assertions.True(exception.StateRollbackException != null, "State rollback exception was not retained.");
            Assertions.True(exception.InventoryRollbackException == null, "Inventory should still restore successfully.");
            Assertions.False(exception.RollbackSucceeded, "Rollback success must be false.");
            Assertions.Equal(3, inventory.Powder, "Inventory was not restored despite state rollback failure.");
            Assertions.Equal(3, inventory.Balls, "Inventory was not restored despite state rollback failure.");
        }

        private static void ReloadTransactionInventoryRollbackFailureSurfaced()
        {
            var stateStore = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ThrowOnReplaceCall = 1
            };
            var inventory = new FakeBasicAmmunitionInventory(3, 3)
            {
                ThrowOnAdd = true
            };
            FirearmReloadTransactionException exception =
                Assertions.Throws<FirearmReloadTransactionException>(
                    () => Reload(stateStore, inventory),
                    "An inventory rollback failure must be surfaced.");
            Assertions.True(exception.InventoryRollbackException != null, "Inventory rollback exception was not retained.");
            Assertions.True(exception.StateRollbackException == null, "Unchanged firearm state should not fail rollback.");
            Assertions.False(exception.RollbackSucceeded, "Rollback success must be false.");
            Assertions.Equal(FirearmState.CreateEmpty(), stateStore.State, "Inventory rollback failure changed firearm state.");
        }

        private static void ReloadResultSuccessValidation()
        {
            FirearmState loaded = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            var result = new FirearmReloadResult(
                FirearmReloadStatus.Loaded,
                FirearmState.CreateEmpty(),
                loaded,
                new BasicAmmunitionInventorySnapshot(2, 2),
                new BasicAmmunitionInventorySnapshot(1, 1));
            Assertions.True(result.Succeeded, "Loaded result must report success.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmReloadResult(
                    FirearmReloadStatus.Loaded,
                    FirearmState.CreateEmpty(),
                    loaded,
                    new BasicAmmunitionInventorySnapshot(2, 2),
                    new BasicAmmunitionInventorySnapshot(0, 1)),
                "Loaded result must require exact component consumption.");
        }

        private static void ReloadResultSuccessBrokenValidation()
        {
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmState loadedBroken = FirearmStateMachine.Load(
                broken,
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            var result = new FirearmReloadResult(
                FirearmReloadStatus.Loaded,
                broken,
                loadedBroken,
                new BasicAmmunitionInventorySnapshot(2, 2),
                new BasicAmmunitionInventorySnapshot(1, 1));
            Assertions.True(result.Succeeded, "A condition-preserving Broken reload must report success.");
            Assertions.Equal(FirearmCondition.Broken, result.AfterState.Condition, "Successful Broken reload changed condition.");

            FirearmState loadedNormal = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            Assertions.Throws<ArgumentException>(
                () => new FirearmReloadResult(
                    FirearmReloadStatus.Loaded,
                    broken,
                    loadedNormal,
                    new BasicAmmunitionInventorySnapshot(2, 2),
                    new BasicAmmunitionInventorySnapshot(1, 1)),
                "A successful Broken reload must not silently repair the firearm.");
        }

        private static void ReloadResultRejectedValidation()
        {
            var inventory = new BasicAmmunitionInventorySnapshot(0, 2);
            var result = new FirearmReloadResult(
                FirearmReloadStatus.InsufficientBlackPowder,
                FirearmState.CreateEmpty(),
                FirearmState.CreateEmpty(),
                inventory,
                inventory);
            Assertions.False(result.Succeeded, "Rejected result must not report success.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmReloadResult(
                    FirearmReloadStatus.InsufficientBlackPowder,
                    FirearmState.CreateEmpty(),
                    FirearmState.CreateEmpty(),
                    inventory,
                    new BasicAmmunitionInventorySnapshot(0, 1)),
                "Rejected result must preserve inventory.");
        }

        private static void ReloadResultUnknownStatus()
        {
            var inventory = new BasicAmmunitionInventorySnapshot(1, 1);
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmReloadResult(
                    (FirearmReloadStatus)99,
                    FirearmState.CreateEmpty(),
                    FirearmState.CreateEmpty(),
                    inventory,
                    inventory),
                "Unknown reload status must be rejected.");
        }

        private static void ReloadResultNullValues()
        {
            var inventory = new BasicAmmunitionInventorySnapshot(1, 1);
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadResult(
                    FirearmReloadStatus.AlreadyLoaded,
                    null,
                    FirearmState.CreateEmpty(),
                    inventory,
                    inventory),
                "Null before state must be rejected.");
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmReloadResult(
                    FirearmReloadStatus.AlreadyLoaded,
                    FirearmState.CreateEmpty(),
                    FirearmState.CreateEmpty(),
                    null,
                    inventory),
                "Null before inventory must be rejected.");
        }

        private static void ReloadResultFormat()
        {
            FirearmState loaded = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                BasicMusketStateRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                1);
            string text = new FirearmReloadResult(
                FirearmReloadStatus.Loaded,
                FirearmState.CreateEmpty(),
                loaded,
                new BasicAmmunitionInventorySnapshot(2, 3),
                new BasicAmmunitionInventorySnapshot(1, 2)).ToString();
            Assertions.True(text.Contains("status=Loaded"), "Reload result format lost status.");
            Assertions.True(text.Contains("rounds=1"), "Reload result format lost loaded state.");
            Assertions.True(text.Contains("blackPowder=1"), "Reload result format lost inventory state.");
        }

        private static FirearmState WreckedState()
        {
            return FirearmStateMachine.Wreck(FirearmState.CreateEmpty());
        }

        private static FirearmState BrokenState()
        {
            return FirearmStateMachine.OverhaulWrecked(WreckedState());
        }

        private static FirearmOverhaulResult Overhaul(
            FakeFirearmOverhaulStateStore stateStore,
            FakeRepairKitInventory inventory)
        {
            return new FirearmOverhaulTransactionService()
                .TryOverhaulWreckedToBroken(stateStore, inventory);
        }

        private static void OverhaulKitSnapshotValid()
        {
            var snapshot = new RepairKitInventorySnapshot(2);
            Assertions.Equal(2, snapshot.RepairKits, "Repair-kit count mismatch.");
            Assertions.True(snapshot.HasOneKit, "A positive repair-kit count must satisfy the one-kit requirement.");
        }

        private static void OverhaulKitSnapshotCapture()
        {
            var inventory = new FakeRepairKitInventory(3);
            RepairKitInventorySnapshot snapshot = RepairKitInventorySnapshot.Capture(inventory);
            Assertions.Equal(3, snapshot.RepairKits, "Captured repair-kit count mismatch.");
            Assertions.Equal(1, inventory.CountCalls, "Snapshot capture must read inventory exactly once.");
        }

        private static void OverhaulKitSnapshotEquality()
        {
            var first = new RepairKitInventorySnapshot(4);
            var second = new RepairKitInventorySnapshot(4);
            var different = new RepairKitInventorySnapshot(3);
            Assertions.True(first.Equals(second), "Equal repair-kit snapshots were not value-equal.");
            Assertions.Equal(first.GetHashCode(), second.GetHashCode(), "Equal snapshots must share a hash code.");
            Assertions.False(first.Equals(different), "Different repair-kit snapshots were treated as equal.");
            Assertions.False(first.Equals(null), "A repair-kit snapshot must not equal null.");
        }

        private static void OverhaulKitSnapshotFormat()
        {
            Assertions.Equal(
                "repairKits=5",
                new RepairKitInventorySnapshot(5).ToString(),
                "Repair-kit snapshot formatting changed.");
        }

        private static void OverhaulKitSnapshotNegativeRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new RepairKitInventorySnapshot(-1),
                "Negative repair-kit counts must be rejected.");
        }

        private static void OverhaulKitSnapshotNullInventoryRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => RepairKitInventorySnapshot.Capture(null),
                "A null repair-kit inventory must be rejected.");
        }

        private static void OverhaulKitSnapshotNegativeStoreCountRejected()
        {
            var inventory = new FakeRepairKitInventory(0) { ReportNegativeCount = true };
            Assertions.Throws<InvalidOperationException>(
                () => RepairKitInventorySnapshot.Capture(inventory),
                "A negative inventory adapter count must fail closed.");
        }

        private static void OverhaulTransactionSuccess()
        {
            FirearmState wrecked = WreckedState();
            var stateStore = new FakeFirearmOverhaulStateStore(wrecked);
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulResult result = Overhaul(stateStore, inventory);
            Assertions.True(result.Succeeded, "A Wrecked firearm with one kit must overhaul successfully.");
            Assertions.Equal(FirearmOverhaulStatus.Overhauled, result.Status, "Overhaul status mismatch.");
            Assertions.Equal(FirearmCondition.Broken, stateStore.State.Condition, "Overhaul did not stop at Broken.");
            Assertions.True(stateStore.State.IsEmpty, "Overhaul created ammunition unexpectedly.");
            Assertions.Equal(1, inventory.Kits, "Overhaul did not consume exactly one repair kit.");
            Assertions.Equal(1, stateStore.ReplaceCalls, "Successful overhaul must write exact-item state once.");
            Assertions.Equal(1, inventory.RemoveCalls, "Successful overhaul must consume inventory once.");
        }

        private static void OverhaulTransactionNormalRejected()
        {
            var stateStore = new FakeFirearmOverhaulStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulResult result = Overhaul(stateStore, inventory);
            Assertions.Equal(FirearmOverhaulStatus.NotWrecked, result.Status, "Normal rejection status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Normal rejection wrote firearm state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Normal rejection consumed a repair kit.");
        }

        private static void OverhaulTransactionBrokenRejected()
        {
            FirearmState broken = BrokenState();
            var stateStore = new FakeFirearmOverhaulStateStore(broken);
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulResult result = Overhaul(stateStore, inventory);
            Assertions.Equal(FirearmOverhaulStatus.NotWrecked, result.Status, "Broken rejection status mismatch.");
            Assertions.Equal(broken, stateStore.State, "Broken rejection changed state.");
            Assertions.Equal(2, inventory.Kits, "Broken rejection consumed a kit.");
        }

        private static void OverhaulTransactionMissingKit()
        {
            FirearmState wrecked = WreckedState();
            var stateStore = new FakeFirearmOverhaulStateStore(wrecked);
            var inventory = new FakeRepairKitInventory(0);
            FirearmOverhaulResult result = Overhaul(stateStore, inventory);
            Assertions.Equal(FirearmOverhaulStatus.InsufficientRepairKit, result.Status, "Missing-kit status mismatch.");
            Assertions.Equal(wrecked, stateStore.State, "Missing-kit rejection changed firearm state.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Missing-kit rejection wrote state.");
        }

        private static void OverhaulTransactionNullStateStore()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmOverhaulTransactionService().TryOverhaulWreckedToBroken(
                    null,
                    new FakeRepairKitInventory(1)),
                "A null overhaul state store must be rejected.");
        }

        private static void OverhaulTransactionNullInventory()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmOverhaulTransactionService().TryOverhaulWreckedToBroken(
                    new FakeFirearmOverhaulStateStore(WreckedState()),
                    null),
                "A null overhaul inventory must be rejected.");
        }

        private static void OverhaulTransactionNullState()
        {
            var stateStore = new FakeFirearmOverhaulStateStore(WreckedState()) { ReturnNullOnRead = true };
            Assertions.Throws<InvalidOperationException>(
                () => Overhaul(stateStore, new FakeRepairKitInventory(1)),
                "A null exact-item state must fail closed.");
        }

        private static void OverhaulTransactionStateWriteFailureRestoresKit()
        {
            FirearmState wrecked = WreckedState();
            var stateStore = new FakeFirearmOverhaulStateStore(wrecked) { ThrowOnReplaceCall = 1 };
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulTransactionException exception = Assertions.Throws<FirearmOverhaulTransactionException>(
                () => Overhaul(stateStore, inventory),
                "A state-write failure must surface as a transaction failure.");
            Assertions.True(exception.RollbackSucceeded, "Pre-mutation state failure should restore inventory cleanly.");
            Assertions.Equal(wrecked, stateStore.State, "State-write failure changed firearm state.");
            Assertions.Equal(2, inventory.Kits, "State-write failure did not restore the repair kit.");
        }

        private static void OverhaulTransactionPostStateMutationFailureRestoresBoth()
        {
            FirearmState wrecked = WreckedState();
            var stateStore = new FakeFirearmOverhaulStateStore(wrecked)
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true
            };
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulTransactionException exception = Assertions.Throws<FirearmOverhaulTransactionException>(
                () => Overhaul(stateStore, inventory),
                "A post-mutation state failure must roll back both resources.");
            Assertions.True(exception.RollbackSucceeded, "Both resources should have rolled back successfully.");
            Assertions.Equal(wrecked, stateStore.State, "Firearm state was not rolled back.");
            Assertions.Equal(2, inventory.Kits, "Repair-kit count was not rolled back.");
            Assertions.Equal(2, stateStore.ReplaceCalls, "State rollback should perform one compensating replacement.");
        }

        private static void OverhaulTransactionStateRollbackFailureSurfaced()
        {
            var stateStore = new FakeFirearmOverhaulStateStore(WreckedState())
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true,
                ThrowOnSecondReplace = true
            };
            var inventory = new FakeRepairKitInventory(2);
            FirearmOverhaulTransactionException exception = Assertions.Throws<FirearmOverhaulTransactionException>(
                () => Overhaul(stateStore, inventory),
                "A state rollback failure must be surfaced.");
            Assertions.True(exception.StateRollbackException != null, "State rollback failure was not retained.");
            Assertions.True(exception.InventoryRollbackException == null, "Inventory should still restore.");
            Assertions.False(exception.RollbackSucceeded, "Rollback success must be false.");
            Assertions.Equal(2, inventory.Kits, "Inventory did not restore after state rollback failure.");
        }

        private static void OverhaulTransactionInventoryRollbackFailureSurfaced()
        {
            var stateStore = new FakeFirearmOverhaulStateStore(WreckedState()) { ThrowOnReplaceCall = 1 };
            var inventory = new FakeRepairKitInventory(2) { ThrowOnAdd = true };
            FirearmOverhaulTransactionException exception = Assertions.Throws<FirearmOverhaulTransactionException>(
                () => Overhaul(stateStore, inventory),
                "An inventory rollback failure must be surfaced.");
            Assertions.True(exception.InventoryRollbackException != null, "Inventory rollback failure was not retained.");
            Assertions.True(exception.StateRollbackException == null, "Unchanged firearm state should not fail rollback.");
            Assertions.False(exception.RollbackSucceeded, "Rollback success must be false.");
            Assertions.Equal(WreckedState(), stateStore.State, "Inventory rollback failure changed firearm state.");
        }

        private static void OverhaulTransactionPostRemoveFailureRestoresKit()
        {
            FirearmState wrecked = WreckedState();
            var stateStore = new FakeFirearmOverhaulStateStore(wrecked);
            var inventory = new FakeRepairKitInventory(2)
            {
                ThrowOnRemoveCall = 1,
                MutateBeforeRemoveFailure = true
            };
            FirearmOverhaulTransactionException exception = Assertions.Throws<FirearmOverhaulTransactionException>(
                () => Overhaul(stateStore, inventory),
                "A post-remove failure must be surfaced.");
            Assertions.True(exception.RollbackSucceeded, "Post-remove failure should restore inventory.");
            Assertions.Equal(2, inventory.Kits, "Post-remove failure did not restore the kit.");
            Assertions.Equal(wrecked, stateStore.State, "Post-remove failure changed firearm state.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "State should not be written after inventory failure.");
        }

        private static void OverhaulResultSuccess()
        {
            FirearmState wrecked = WreckedState();
            FirearmState broken = FirearmStateMachine.OverhaulWrecked(wrecked);
            var result = new FirearmOverhaulResult(
                FirearmOverhaulStatus.Overhauled,
                wrecked,
                broken,
                new RepairKitInventorySnapshot(2),
                new RepairKitInventorySnapshot(1));
            Assertions.True(result.Succeeded, "Successful result did not report success.");
            Assertions.True(result.ToString().Contains("status=Overhauled"), "Success format lost status.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmOverhaulResult(
                    FirearmOverhaulStatus.Overhauled,
                    wrecked,
                    FirearmState.CreateEmpty(),
                    new RepairKitInventorySnapshot(2),
                    new RepairKitInventorySnapshot(1)),
                "Success must require empty/Broken final state.");
        }

        private static void OverhaulResultRejected()
        {
            FirearmState state = BrokenState();
            var inventory = new RepairKitInventorySnapshot(3);
            var result = new FirearmOverhaulResult(
                FirearmOverhaulStatus.NotWrecked,
                state,
                state,
                inventory,
                inventory);
            Assertions.False(result.Succeeded, "Rejected result reported success.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmOverhaulResult(
                    FirearmOverhaulStatus.NotWrecked,
                    state,
                    WreckedState(),
                    inventory,
                    inventory),
                "Rejected result must preserve exact state.");
        }

        private static void OverhaulResultUnknownStatus()
        {
            FirearmState state = WreckedState();
            var inventory = new RepairKitInventorySnapshot(1);
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmOverhaulResult(
                    (FirearmOverhaulStatus)99,
                    state,
                    state,
                    inventory,
                    inventory),
                "Unknown overhaul status must be rejected.");
        }

        private static void OverhaulResultNullSnapshots()
        {
            FirearmState state = WreckedState();
            var inventory = new RepairKitInventorySnapshot(1);
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmOverhaulResult(
                    FirearmOverhaulStatus.NotWrecked,
                    null,
                    state,
                    inventory,
                    inventory),
                "Null before state must be rejected.");
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmOverhaulResult(
                    FirearmOverhaulStatus.NotWrecked,
                    state,
                    state,
                    null,
                    inventory),
                "Null before inventory must be rejected.");
        }

        private static void OverhaulRuntimeResultSuccess()
        {
            FirearmState wrecked = WreckedState();
            FirearmState broken = FirearmStateMachine.OverhaulWrecked(wrecked);
            var transaction = new FirearmOverhaulResult(
                FirearmOverhaulStatus.Overhauled,
                wrecked,
                broken,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            FirearmItemStateSnapshot before = OverhaulRuntimeSnapshot(12, 7, 0x1234, wrecked);
            FirearmItemStateSnapshot after = OverhaulRuntimeSnapshot(12, 8, 0x1234, broken);
            var result = new FirearmOverhaulRuntimeResult(transaction, before, after);
            Assertions.True(result.Succeeded, "Runtime result did not report success.");
            Assertions.True(result.ToString().Contains("revision=7->8"), "Runtime result lost revision proof.");
            Assertions.True(result.ToString().Contains("exactItemPreserved=True"), "Runtime result lost exact-item proof.");
        }

        private static void OverhaulRuntimeResultIdentityMismatch()
        {
            FirearmState wrecked = WreckedState();
            FirearmState broken = FirearmStateMachine.OverhaulWrecked(wrecked);
            var transaction = new FirearmOverhaulResult(
                FirearmOverhaulStatus.Overhauled,
                wrecked,
                broken,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            Assertions.Throws<ArgumentException>(
                () => new FirearmOverhaulRuntimeResult(
                    transaction,
                    OverhaulRuntimeSnapshot(12, 7, 0x1234, wrecked),
                    OverhaulRuntimeSnapshot(13, 8, 0x1234, broken)),
                "Changed repository identity must be rejected.");
        }

        private static void OverhaulRuntimeResultRevisionMismatch()
        {
            FirearmState wrecked = WreckedState();
            FirearmState broken = FirearmStateMachine.OverhaulWrecked(wrecked);
            var transaction = new FirearmOverhaulResult(
                FirearmOverhaulStatus.Overhauled,
                wrecked,
                broken,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            Assertions.Throws<ArgumentException>(
                () => new FirearmOverhaulRuntimeResult(
                    transaction,
                    OverhaulRuntimeSnapshot(12, 7, 0x1234, wrecked),
                    OverhaulRuntimeSnapshot(12, 9, 0x1234, broken)),
                "Revision jumps other than one must be rejected.");
        }

        private static FirearmItemStateSnapshot OverhaulRuntimeSnapshot(
            long entryId,
            int revision,
            int runtimeReferenceHash,
            FirearmState state)
        {
            var resolved = new ResolvedFirearmItem(
                new object(),
                FirearmDefinitions.CreateEarlyMusket(),
                "Test Musket",
                "runtime",
                "KMG_TestMusket_Item",
                "item-guid",
                "KMG_TestMusket_WeaponType",
                "type-guid");
            var repository = new FirearmStateRepositorySnapshot(
                entryId,
                revision,
                "Synthetic.ItemEntityWeapon",
                runtimeReferenceHash,
                state);
            return new FirearmItemStateSnapshot(resolved, repository);
        }

        private static void FactoryEarlyMusketFreshInstances()
        {
            FirearmDefinition first = FirearmDefinitions.CreateEarlyMusket();
            FirearmDefinition second = FirearmDefinitions.CreateEarlyMusket();
            Assertions.False(ReferenceEquals(first, second), "The canonical factory must return a fresh definition.");
            Assertions.False(ReferenceEquals(first.Reload, second.Reload), "The canonical factory must return a fresh reload profile.");
        }

        private static void FactoryEarlyMusketCanonicalEquality()
        {
            FirearmDefinition expected = new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Musket,
                1,
                40,
                2,
                5,
                FullRoundReload(1),
                false);
            Assertions.Equal(expected, FirearmDefinitions.CreateEarlyMusket(), "Canonical early-musket definition changed.");
        }

        private static void FactoryEarlyPistolFreshInstances()
        {
            FirearmDefinition first = FirearmDefinitions.CreateEarlyPistol();
            FirearmDefinition second = FirearmDefinitions.CreateEarlyPistol();
            Assertions.False(ReferenceEquals(first, second),
                "The canonical pistol factory must return a fresh definition.");
            Assertions.False(ReferenceEquals(first.Reload, second.Reload),
                "The canonical pistol factory must return a fresh reload profile.");
        }

        private static void FactoryEarlyPistolCanonicalEquality()
        {
            var expected = new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Pistol,
                1,
                20,
                1,
                5,
                StandardReload(1),
                false);
            Assertions.Equal(expected, FirearmDefinitions.CreateEarlyPistol(),
                "Canonical early-pistol definition changed.");
        }

        private static void FactoryEarlyBlunderbussFreshInstances()
        {
            FirearmDefinition first = FirearmDefinitions.CreateEarlyBlunderbuss();
            FirearmDefinition second = FirearmDefinitions.CreateEarlyBlunderbuss();
            Assertions.False(ReferenceEquals(first, second),
                "The canonical blunderbuss factory must return a fresh definition.");
            Assertions.False(ReferenceEquals(first.Reload, second.Reload),
                "The canonical blunderbuss factory must return a fresh reload profile.");
            Assertions.Equal(first, second,
                "Fresh canonical blunderbuss definitions must compare equal.");
        }

        private static void FactoryEarlyBlunderbussOrdinaryRange()
        {
            FirearmDefinition definition = FirearmDefinitions.CreateEarlyBlunderbuss();
            Assertions.Equal(FirearmEra.Early, definition.Era, "Era mismatch.");
            Assertions.Equal(FirearmKind.Blunderbuss, definition.Kind, "Kind mismatch.");
            Assertions.Equal(1, definition.Capacity, "Capacity mismatch.");
            Assertions.True(definition.HasFixedRangeIncrement,
                "The ordinary Blunderbuss bullet mode must expose its 10-foot increment.");
            Assertions.Equal<int?>(10, definition.FixedRangeIncrementFeet,
                "Blunderbuss ordinary range mismatch.");
            Assertions.Equal(2, definition.MisfireValue, "Misfire mismatch.");
            Assertions.Equal(10, definition.MisfireBurstRadiusFeet, "Burst mismatch.");
            Assertions.Equal(ReloadActionType.FullRound, definition.Reload.BaseAction,
                "Reload mismatch.");
            Assertions.True(definition.IsScatter, "Blunderbuss must be scatter.");
            Assertions.Equal(
                "Early Blunderbuss; capacity=1; range=10ft; misfire=1-2; misfireBurst=10ft; reload=(FullRound; freeHand=True; roundsPerAction=1); scatter=True",
                definition.ToString(),
                "Dual-mode Blunderbuss formatting changed.");
        }

        private static void FactoryEarlyBlunderbussFixedRangeAccessible()
        {
            FirearmDefinition definition = FirearmDefinitions.CreateEarlyBlunderbuss();
            Assertions.Equal(10, definition.RangeIncrementFeet,
                "Ordinary Blunderbuss bullet mode must use a 10-foot increment.");
        }

        private static void ValidEarlyMusket()
        {
            FirearmDefinition definition = EarlyMusket();
            Assertions.Equal(FirearmEra.Early, definition.Era, "Era mismatch.");
            Assertions.Equal(FirearmKind.Musket, definition.Kind, "Kind mismatch.");
            Assertions.Equal(1, definition.Capacity, "Capacity mismatch.");
            Assertions.Equal(40, definition.RangeIncrementFeet, "Range mismatch.");
            Assertions.Equal(2, definition.MisfireValue, "Misfire mismatch.");
            Assertions.Equal(5, definition.MisfireBurstRadiusFeet, "Misfire burst radius mismatch.");
            Assertions.Equal(ReloadActionType.FullRound, definition.Reload.BaseAction, "Reload mismatch.");
            Assertions.False(definition.IsScatter, "Musket must not be scatter.");
        }

        private static void ValidEarlyPistol()
        {
            FirearmDefinition definition = new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Pistol,
                2,
                20,
                1,
                5,
                StandardReload(1),
                false);
            Assertions.Equal(2, definition.Capacity, "Double-barrel capacity must be preserved.");
        }

        private static void ValidEarlyBlunderbuss()
        {
            FirearmDefinition definition = new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Blunderbuss,
                1,
                15,
                2,
                5,
                FullRoundReload(1),
                true);
            Assertions.True(definition.IsScatter, "Blunderbuss must be scatter.");
        }

        private static void ValidAdvancedPistol()
        {
            FirearmDefinition definition = new FirearmDefinition(
                FirearmEra.Advanced,
                FirearmKind.Pistol,
                1,
                30,
                1,
                5,
                MoveReload(1),
                false);
            Assertions.Equal(ReloadActionType.Move, definition.Reload.BaseAction, "Advanced reload mismatch.");
        }

        private static void ValidAdvancedRifle()
        {
            FirearmDefinition definition = new FirearmDefinition(
                FirearmEra.Advanced,
                FirearmKind.Rifle,
                1,
                80,
                1,
                5,
                MoveReload(1),
                false);
            Assertions.Equal(FirearmKind.Rifle, definition.Kind, "Rifle kind mismatch.");
        }

        private static void ValidAdvancedRevolver()
        {
            FirearmDefinition definition = new FirearmDefinition(
                FirearmEra.Advanced,
                FirearmKind.Revolver,
                6,
                20,
                1,
                5,
                MoveReload(1),
                false);
            Assertions.Equal(6, definition.Capacity, "Revolver capacity mismatch.");
        }

        private static void FirearmDefinitionValueEquality()
        {
            FirearmDefinition left = EarlyMusket();
            FirearmDefinition right = EarlyMusket();
            Assertions.True(left.Equals(right), "Equal definitions must compare equal.");
            Assertions.True(left == right, "Equality operator must use value equality.");
            Assertions.False(left != right, "Inequality operator must use value equality.");
            Assertions.Equal(left.GetHashCode(), right.GetHashCode(), "Equal definitions require equal hashes.");
        }

        private static void ReloadProfileValueEquality()
        {
            ReloadProfile left = FullRoundReload(1);
            ReloadProfile right = FullRoundReload(1);
            Assertions.True(left.Equals(right), "Equal reload profiles must compare equal.");
            Assertions.True(left == right, "Reload equality operator mismatch.");
            Assertions.Equal(left.GetHashCode(), right.GetHashCode(), "Equal reload profiles require equal hashes.");
        }

        private static void DifferentDefinitionsAreNotEqual()
        {
            FirearmDefinition musket = EarlyMusket();
            FirearmDefinition pistol = new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Pistol,
                1,
                20,
                1,
                5,
                StandardReload(1),
                false);
            Assertions.False(musket.Equals(pistol), "Different definitions must not compare equal.");
            Assertions.True(musket != pistol, "Inequality operator mismatch.");
        }

        private static void DifferentMisfireBurstDefinitionsAreNotEqual()
        {
            FirearmDefinition fiveFeet = EarlyMusket();
            FirearmDefinition tenFeet = NewDefinition(
                FirearmEra.Early,
                FirearmKind.Musket,
                1,
                40,
                2,
                FullRoundReload(1),
                false,
                10);
            Assertions.False(
                fiveFeet.Equals(tenFeet),
                "Definitions with different misfire burst radii compared equal.");
            Assertions.True(
                fiveFeet != tenFeet,
                "Misfire burst radius was omitted from definition inequality.");
        }

        private static void DeterministicFormatting()
        {
            string expected = "Early Musket; capacity=1; range=40ft; misfire=1-2; misfireBurst=5ft; reload=(FullRound; freeHand=True; roundsPerAction=1); scatter=False";
            Assertions.Equal(expected, EarlyMusket().ToString(), "Definition formatting changed.");
        }

        private static void InvalidUnknownEra()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Unknown, FirearmKind.Pistol, 1, 20, 1, StandardReload(1), false),
                "Unknown era must fail.");
        }

        private static void InvalidUndefinedEra()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition((FirearmEra)99, FirearmKind.Pistol, 1, 20, 1, StandardReload(1), false),
                "Undefined era must fail.");
        }

        private static void InvalidUnknownKind()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Unknown, 1, 20, 1, StandardReload(1), false),
                "Unknown kind must fail.");
        }

        private static void InvalidUndefinedKind()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, (FirearmKind)99, 1, 20, 1, StandardReload(1), false),
                "Undefined kind must fail.");
        }

        private static void InvalidCapacityZero()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 0, 20, 1, StandardReload(1), false),
                "Zero capacity must fail.");
        }

        private static void InvalidCapacityTooLarge()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 65, 20, 1, StandardReload(1), false),
                "Oversized capacity must fail.");
        }

        private static void InvalidRangeTooSmall()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 0, 1, StandardReload(1), false),
                "Zero range must fail.");
        }

        private static void InvalidRangeNotFiveFootStep()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 22, 1, StandardReload(1), false),
                "Non-grid range must fail.");
        }

        private static void InvalidRangeTooLarge()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 1005, 1, StandardReload(1), false),
                "Oversized range must fail.");
        }

        private static void InvalidMisfireZero()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 20, 0, StandardReload(1), false),
                "Zero misfire value must fail.");
        }

        private static void InvalidMisfireTooLarge()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 20, 21, StandardReload(1), false),
                "Misfire above d20 must fail.");
        }

        private static void InvalidMisfireBurstTooSmall()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(
                    FirearmEra.Early,
                    FirearmKind.Pistol,
                    1,
                    20,
                    1,
                    StandardReload(1),
                    false,
                    0),
                "A zero-foot misfire burst radius was accepted.");
        }

        private static void InvalidMisfireBurstNotFiveFootStep()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(
                    FirearmEra.Early,
                    FirearmKind.Pistol,
                    1,
                    20,
                    1,
                    StandardReload(1),
                    false,
                    6),
                "A non-grid misfire burst radius was accepted.");
        }

        private static void InvalidMisfireBurstTooLarge()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                NewDefinition(
                    FirearmEra.Early,
                    FirearmKind.Pistol,
                    1,
                    20,
                    1,
                    StandardReload(1),
                    false,
                    105),
                "An oversized misfire burst radius was accepted.");
        }

        private static void InvalidNullReload()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 20, 1, null, false),
                "Null reload must fail.");
        }

        private static void InvalidReloadUnknownAction()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new ReloadProfile(ReloadActionType.Unknown, true, 1),
                "Unknown reload action must fail.");
        }

        private static void InvalidReloadUndefinedAction()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new ReloadProfile((ReloadActionType)99, true, 1),
                "Undefined reload action must fail.");
        }

        private static void InvalidReloadZeroRounds()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new ReloadProfile(ReloadActionType.Standard, true, 0),
                "Zero reload rounds must fail.");
        }

        private static void InvalidReloadTooManyRounds()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new ReloadProfile(ReloadActionType.Standard, true, 65),
                "Oversized reload batch must fail.");
        }

        private static void InvalidReloadExceedsCapacity()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 20, 1, StandardReload(2), false),
                "Reload batch above capacity must fail.");
        }

        private static void InvalidScatterNonBlunderbuss()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Musket, 1, 40, 2, FullRoundReload(1), true),
                "Scatter musket must fail.");
        }

        private static void InvalidBlunderbussWithoutScatter()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Blunderbuss, 1, 15, 2, FullRoundReload(1), false),
                "Non-scatter blunderbuss must fail.");
        }

        private static void InvalidAdvancedMusket()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Advanced, FirearmKind.Musket, 1, 40, 1, MoveReload(1), false),
                "Advanced musket kind must fail.");
        }

        private static void InvalidAdvancedBlunderbuss()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Advanced, FirearmKind.Blunderbuss, 1, 15, 1, MoveReload(1), true),
                "Advanced blunderbuss kind must fail.");
        }

        private static void InvalidEarlyRifle()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Rifle, 1, 80, 1, FullRoundReload(1), false),
                "Early rifle kind must fail.");
        }

        private static void InvalidEarlyRevolver()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Revolver, 6, 20, 1, StandardReload(1), false),
                "Early revolver kind must fail.");
        }

        private static void InvalidRevolverCapacityOne()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Advanced, FirearmKind.Revolver, 1, 20, 1, MoveReload(1), false),
                "Single-shot revolver must fail.");
        }

        private static void InvalidEarlyPistolWrongReload()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Pistol, 1, 20, 1, FullRoundReload(1), false),
                "Early pistol must use a standard base reload.");
        }

        private static void InvalidEarlyMusketWrongReload()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Musket, 1, 40, 2, StandardReload(1), false),
                "Early musket must use a full-round base reload.");
        }

        private static void InvalidEarlyBlunderbussWrongReload()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Early, FirearmKind.Blunderbuss, 1, 15, 2, StandardReload(1), true),
                "Early blunderbuss must use a full-round base reload.");
        }

        private static void InvalidAdvancedWrongReload()
        {
            Assertions.Throws<ArgumentException>(() =>
                NewDefinition(FirearmEra.Advanced, FirearmKind.Rifle, 1, 80, 1, StandardReload(1), false),
                "Advanced firearms must use a move base reload.");
        }

        private static FirearmState LoadedState(
            int rounds,
            FirearmCondition condition)
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                rounds,
                LeadBall(),
                condition);
        }

        private static void DischargeLoadedNormal()
        {
            FirearmState before = LoadedState(1, FirearmCondition.Normal);
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(FirearmDischargeStatus.Fired, result.Status, "Loaded firearm status mismatch.");
            Assertions.Equal(1, result.RoundsConsumed, "Loaded firearm must consume one round.");
            Assertions.False(result.ShouldForceMiss, "Loaded firearm must not be forced to miss.");
            Assertions.Equal(FirearmState.CreateEmpty(), result.After, "Final loaded round must leave the firearm empty and Normal.");
            Assertions.Equal(before, result.Before, "Discharge result must retain the immutable before-state.");
        }

        private static void DischargeLoadedBroken()
        {
            FirearmState before = LoadedState(1, FirearmCondition.Broken);
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(FirearmDischargeStatus.Fired, result.Status, "Loaded Broken firearm status mismatch.");
            Assertions.Equal(FirearmCondition.Broken, result.After.Condition, "Discharge must preserve Broken condition.");
            Assertions.Equal(0, result.After.LoadedRounds, "The Broken firearm's final round was not consumed.");
            Assertions.False(result.ShouldForceMiss, "A loaded Broken firearm may discharge.");
        }

        private static void DischargeMultipleRounds()
        {
            FirearmState before = LoadedState(3, FirearmCondition.Normal);
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(2, result.After.LoadedRounds, "One attack roll must consume exactly one of several rounds.");
            Assertions.Equal(before.LoadedAmmunition, result.After.LoadedAmmunition, "Remaining rounds must retain their ammunition identity.");
        }

        private static void DischargeEmptyNormal()
        {
            FirearmState before = FirearmState.CreateEmpty();
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(FirearmDischargeStatus.Empty, result.Status, "Empty firearm status mismatch.");
            Assertions.True(result.ShouldForceMiss, "An empty firearm must force a miss.");
            Assertions.Equal(0, result.RoundsConsumed, "An empty firearm cannot consume a round.");
            Assertions.True(ReferenceEquals(before, result.After), "An empty rejection must preserve the exact immutable state object.");
        }

        private static void DischargeEmptyBroken()
        {
            FirearmState before = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Broken);
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(FirearmDischargeStatus.Empty, result.Status, "Empty Broken firearm status mismatch.");
            Assertions.True(result.ShouldForceMiss, "An empty Broken firearm must force a miss.");
            Assertions.Equal(FirearmCondition.Broken, result.After.Condition, "Empty rejection must preserve Broken condition.");
        }

        private static void DischargeWrecked()
        {
            FirearmState before = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Wrecked);
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(before);
            Assertions.Equal(FirearmDischargeStatus.Wrecked, result.Status, "Wrecked firearm status mismatch.");
            Assertions.True(result.ShouldForceMiss, "A Wrecked firearm must force a miss.");
            Assertions.True(ReferenceEquals(before, result.After), "Wrecked rejection must preserve state.");
        }

        private static void DischargeNullState()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmDischargeService().Evaluate(null),
                "A null firearm state was accepted for discharge.");
        }

        private static void DischargeResultFiredValidation()
        {
            FirearmState before = LoadedState(1, FirearmCondition.Normal);
            FirearmState after = FirearmStateMachine.Fire(before);
            var valid = new FirearmDischargeResult(
                FirearmDischargeStatus.Fired,
                before,
                after,
                1,
                false);
            Assertions.Equal(after, valid.After, "Valid fired result was not retained.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Fired,
                    before,
                    after,
                    0,
                    false),
                "A fired result that consumed no round was accepted.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Fired,
                    before,
                    after,
                    1,
                    true),
                "A fired result that also forces a miss was accepted.");
        }

        private static void DischargeResultEmptyValidation()
        {
            FirearmState empty = FirearmState.CreateEmpty();
            var valid = new FirearmDischargeResult(
                FirearmDischargeStatus.Empty,
                empty,
                empty,
                0,
                true);
            Assertions.True(valid.ShouldForceMiss, "Valid empty result did not force a miss.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Empty,
                    empty,
                    empty,
                    0,
                    false),
                "An empty result that does not force a miss was accepted.");
        }

        private static void DischargeResultWreckedValidation()
        {
            FirearmState wrecked = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Wrecked);
            var valid = new FirearmDischargeResult(
                FirearmDischargeStatus.Wrecked,
                wrecked,
                wrecked,
                0,
                true);
            Assertions.Equal(wrecked, valid.After, "Valid wrecked result was not retained.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Wrecked,
                    FirearmState.CreateEmpty(),
                    FirearmState.CreateEmpty(),
                    0,
                    true),
                "A non-wrecked state was accepted as a wrecked result.");
        }

        private static void DischargeResultUnknownStatus()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmDischargeResult(
                    (FirearmDischargeStatus)999,
                    FirearmState.CreateEmpty(),
                    FirearmState.CreateEmpty(),
                    0,
                    true),
                "An undefined discharge status was accepted.");
        }

        private static void DischargeResultNullState()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Empty,
                    null,
                    FirearmState.CreateEmpty(),
                    0,
                    true),
                "A null discharge before-state was accepted.");
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmDischargeResult(
                    FirearmDischargeStatus.Empty,
                    FirearmState.CreateEmpty(),
                    null,
                    0,
                    true),
                "A null discharge after-state was accepted.");
        }

        private static void DischargeResultFormat()
        {
            FirearmDischargeResult result = new FirearmDischargeService().Evaluate(
                LoadedState(1, FirearmCondition.Normal));
            string text = result.ToString();
            Assertions.True(text.Contains("status=Fired"), "Discharge format omitted status.");
            Assertions.True(text.Contains("roundsConsumed=1"), "Discharge format omitted consumed rounds.");
            Assertions.True(text.Contains("forceMiss=False"), "Discharge format omitted miss decision.");
        }

        private static void MisfireNaturalOneForcesMiss()
        {
            FirearmMisfireDecision decision = new FirearmMisfireService().Evaluate(
                1,
                EarlyMusket().MisfireValue,
                true);
            Assertions.True(decision.IsMisfire, "Natural 1 was not classified as a misfire.");
            Assertions.False(decision.FinalSuccess, "Natural 1 did not force the final result to miss.");
            Assertions.True(decision.NativeSuccess, "The native success input was not retained.");
            Assertions.Equal(1, decision.NaturalRoll, "Natural roll mismatch.");
            Assertions.Equal(2, decision.MisfireValue, "Test Musket misfire threshold mismatch.");
        }

        private static void MisfireNaturalTwoForcesMiss()
        {
            FirearmMisfireDecision decision = new FirearmMisfireService().Evaluate(
                2,
                EarlyMusket().MisfireValue,
                true);
            Assertions.True(decision.IsMisfire, "Natural 2 was not classified as a Test Musket misfire.");
            Assertions.False(decision.FinalSuccess, "Natural 2 did not force the final result to miss.");
        }

        private static void MisfireAboveThresholdPreservesHit()
        {
            FirearmMisfireDecision decision = new FirearmMisfireService().Evaluate(
                3,
                EarlyMusket().MisfireValue,
                true);
            Assertions.False(decision.IsMisfire, "Natural 3 was incorrectly classified as a Test Musket misfire.");
            Assertions.True(decision.FinalSuccess, "A native hit above the misfire threshold was changed.");
        }

        private static void MisfireAboveThresholdPreservesNativeMiss()
        {
            FirearmMisfireDecision decision = new FirearmMisfireService().Evaluate(
                20,
                EarlyMusket().MisfireValue,
                false);
            Assertions.False(decision.IsMisfire, "Natural 20 was incorrectly classified as a Test Musket misfire.");
            Assertions.False(decision.FinalSuccess, "A native miss above the misfire threshold was changed to a hit.");
        }

        private static void MisfireThresholdTwenty()
        {
            FirearmMisfireDecision decision = new FirearmMisfireService().Evaluate(
                20,
                FirearmDefinition.MaximumMisfireValue,
                true);
            Assertions.True(decision.IsMisfire, "The maximum supported misfire threshold did not include natural 20.");
            Assertions.False(decision.FinalSuccess, "A threshold-20 misfire did not force a miss.");
        }

        private static void MisfireDecisionFormat()
        {
            string text = new FirearmMisfireService().Evaluate(2, 2, true).ToString();
            Assertions.Equal(
                "naturalD20=2; misfireRange=1-2; nativeSuccess=True; misfired=True; finalSuccess=False",
                text,
                "Misfire decision format changed.");
        }

        private static void MisfireConditionOrdinaryNormalUnchanged()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmState state = FirearmState.CreateEmpty();
            FirearmMisfireConditionDecision decision =
                new FirearmMisfireConditionService().Evaluate(roll, state);
            Assertions.Equal(FirearmMisfireConditionTransition.None, decision.Transition, "An ordinary Normal roll changed condition.");
            Assertions.Equal(state, decision.Before, "The ordinary decision lost its input state.");
            Assertions.Equal(state, decision.After, "An ordinary roll mutated the firearm state.");
            Assertions.False(decision.ChangesCondition, "An ordinary roll reported a condition mutation.");
        }

        private static void MisfireConditionOrdinaryBrokenUnchanged()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(20, 2, true);
            FirearmState state = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision decision =
                new FirearmMisfireConditionService().Evaluate(roll, state);
            Assertions.Equal(FirearmMisfireConditionTransition.None, decision.Transition, "An ordinary Broken roll changed condition.");
            Assertions.Equal(FirearmCondition.Broken, decision.After.Condition, "An ordinary roll repaired or wrecked the Broken firearm.");
            Assertions.Equal(0, decision.After.LoadedRounds, "An ordinary roll loaded the Broken firearm.");
        }

        private static void MisfireConditionNormalToBroken()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(2, 2, true);
            FirearmMisfireConditionDecision decision =
                new FirearmMisfireConditionService().Evaluate(
                    roll,
                    FirearmState.CreateEmpty());
            Assertions.Equal(FirearmMisfireConditionTransition.NormalToBroken, decision.Transition, "A Normal misfire did not become Broken.");
            Assertions.Equal(FirearmCondition.Normal, decision.Before.Condition, "Normal-to-Broken input condition mismatch.");
            Assertions.Equal(FirearmCondition.Broken, decision.After.Condition, "Normal-to-Broken output condition mismatch.");
            Assertions.Equal(0, decision.After.LoadedRounds, "A Normal misfire restored the discharged round.");
            Assertions.Equal<AmmunitionId>(null, decision.After.LoadedAmmunition, "A Normal misfire retained ammunition in an empty firearm.");
            Assertions.True(decision.ChangesCondition, "A Normal misfire did not report a condition mutation.");
        }

        private static void MisfireConditionBrokenToWrecked()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, false);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision decision =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            Assertions.Equal(FirearmMisfireConditionTransition.BrokenToWrecked, decision.Transition, "A Broken misfire did not become Wrecked.");
            Assertions.Equal(FirearmCondition.Broken, decision.Before.Condition, "Broken-to-Wrecked input condition mismatch.");
            Assertions.Equal(FirearmCondition.Wrecked, decision.After.Condition, "Broken-to-Wrecked output condition mismatch.");
            Assertions.Equal(0, decision.After.LoadedRounds, "A Wrecked firearm retained a discharged round.");
            Assertions.Equal<AmmunitionId>(null, decision.After.LoadedAmmunition, "A Wrecked firearm retained ammunition.");
        }

        private static void MisfireConditionLoadedStateRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            Assertions.Throws<ArgumentException>(
                () => new FirearmMisfireConditionService().Evaluate(
                    roll,
                    LoadedState(1, LeadBall(), FirearmCondition.Normal)),
                "Condition damage accepted a state whose round had not discharged.");
        }

        private static void MisfireConditionWreckedStateRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            Assertions.Throws<ArgumentException>(
                () => new FirearmMisfireConditionService().Evaluate(roll, TokenWrecked()),
                "A Wrecked firearm was accepted as a successfully discharged attack.");
        }

        private static void MisfireConditionNullDecisionRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmMisfireConditionService().Evaluate(null, FirearmState.CreateEmpty()),
                "A null natural-roll decision was accepted.");
        }

        private static void MisfireConditionNullStateRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmMisfireConditionService().Evaluate(roll, null),
                "A null post-discharge state was accepted.");
        }

        private static void MisfireConditionFormat()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(2, 2, true);
            string text = new FirearmMisfireConditionService()
                .Evaluate(roll, FirearmState.CreateEmpty())
                .ToString();
            Assertions.Equal(
                "conditionTransition=NormalToBroken; conditionBefore=Normal; effectiveCondition=Normal; conditionAfter=Broken; stateBefore=[schema=1; rounds=0; ammunition=<none>; condition=Normal]; stateAfter=[schema=1; rounds=0; ammunition=<none>; condition=Broken]",
                text,
                "Misfire condition decision format changed.");
        }

        private static void MisfireConditionMisfireWithoutTransitionRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            FirearmState state = FirearmState.CreateEmpty();
            Assertions.Throws<ArgumentException>(
                () => new FirearmMisfireConditionDecision(
                    roll,
                    state,
                    state,
                    FirearmMisfireConditionTransition.None),
                "A detected misfire was accepted without condition damage.");
        }

        private static void MisfireConditionOrdinaryTransitionRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmState before = FirearmState.CreateEmpty();
            FirearmState after = FirearmStateMachine.ApplyMisfireDamage(before);
            Assertions.Throws<ArgumentException>(
                () => new FirearmMisfireConditionDecision(
                    roll,
                    before,
                    after,
                    FirearmMisfireConditionTransition.NormalToBroken),
                "An ordinary roll was accepted with misfire condition damage.");
        }

        private static void MisfireConditionUnknownTransitionRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmState state = FirearmState.CreateEmpty();
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmMisfireConditionDecision(
                    roll,
                    state,
                    state,
                    (FirearmMisfireConditionTransition)99),
                "An unknown misfire condition transition was accepted.");
        }

        private static void ExplosionOrdinaryNormalNone()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(
                    roll,
                    FirearmState.CreateEmpty());
            FirearmExplosionDecision decision =
                new FirearmExplosionService().Evaluate(condition);
            Assertions.Equal(FirearmExplosionDisposition.None, decision.Disposition, "An ordinary Normal roll scheduled an explosion.");
            Assertions.False(decision.RequiresBurstDamage, "An ordinary Normal roll required burst damage.");
        }

        private static void ExplosionOrdinaryBrokenNone()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(20, 2, true);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            FirearmExplosionDecision decision =
                new FirearmExplosionService().Evaluate(condition);
            Assertions.Equal(FirearmExplosionDisposition.None, decision.Disposition, "An ordinary Broken roll scheduled an explosion.");
            Assertions.False(decision.RequiresBurstDamage, "An ordinary Broken roll required burst damage.");
        }

        private static void ExplosionNormalToBrokenNone()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(
                    roll,
                    FirearmState.CreateEmpty());
            FirearmExplosionDecision decision =
                new FirearmExplosionService().Evaluate(condition);
            Assertions.Equal(FirearmMisfireConditionTransition.NormalToBroken, condition.Transition, "The fixture did not produce NormalToBroken.");
            Assertions.Equal(FirearmExplosionDisposition.None, decision.Disposition, "A first misfire scheduled an explosion.");
            Assertions.False(decision.RequiresBurstDamage, "A first misfire required burst damage.");
        }

        private static void ExplosionBrokenToWreckedDamagesBurst()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(2, 2, true);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            FirearmExplosionDecision decision =
                new FirearmExplosionService().Evaluate(condition);
            Assertions.Equal(FirearmMisfireConditionTransition.BrokenToWrecked, condition.Transition, "The fixture did not produce BrokenToWrecked.");
            Assertions.Equal(FirearmExplosionDisposition.DamageBurst, decision.Disposition, "A second misfire did not select burst damage.");
            Assertions.True(decision.RequiresBurstDamage, "A second misfire did not require burst damage.");
            Assertions.Equal(FirearmCondition.Wrecked, decision.Condition.After.Condition, "The explosion decision did not retain the Wrecked result.");
            Assertions.Equal(0, decision.Condition.After.LoadedRounds, "The explosion decision restored the discharged round.");
        }

        private static void ExplosionReflexDcTwelve()
        {
            Assertions.Equal(
                12,
                FirearmExplosionService.ReflexSaveDifficultyClass,
                "The bounded second-misfire Reflex save DC changed.");
        }

        private static void ExplosionNullConditionRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmExplosionService().Evaluate(null),
                "A null misfire-condition decision was accepted by explosion policy.");
        }

        private static void ExplosionDecisionFormat()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, false);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            string text = new FirearmExplosionService().Evaluate(condition).ToString();
            Assertions.Equal(
                "explosionDisposition=DamageBurst; requiresBurstDamage=True; reflexDC=12; conditionTransition=BrokenToWrecked",
                text,
                "Explosion decision format changed.");
        }

        private static void ExplosionBrokenToWreckedNoneRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionDecision(
                    condition,
                    FirearmExplosionDisposition.None),
                "A BrokenToWrecked second misfire was accepted without burst damage.");
        }

        private static void ExplosionNormalToBrokenBurstRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(
                    roll,
                    FirearmState.CreateEmpty());
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionDecision(
                    condition,
                    FirearmExplosionDisposition.DamageBurst),
                "A first NormalToBroken misfire was accepted with explosion damage.");
        }

        private static void ExplosionUnknownDispositionRejected()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(
                    roll,
                    FirearmState.CreateEmpty());
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmExplosionDecision(
                    condition,
                    (FirearmExplosionDisposition)99),
                "An unknown explosion disposition was accepted.");
        }

        private static void ExplosionTargetCandidateValid()
        {
            object unit = new object();
            var candidate = new FirearmExplosionTargetCandidate(
                unit,
                " unit-1 ",
                " Ally ",
                1.25f,
                false);
            Assertions.True(
                ReferenceEquals(unit, candidate.Unit),
                "The target candidate lost its exact unit reference.");
            Assertions.Equal(
                "unit-1",
                candidate.StableIdentity,
                "The target identity was not normalized.");
            Assertions.Equal(
                "Ally",
                candidate.DisplayName,
                "The target display name was not normalized.");
            Assertions.Equal(
                1.25f,
                candidate.DistanceMeters,
                "The target distance changed.");
            Assertions.False(
                candidate.IsExactWielder,
                "A nearby candidate became the exact wielder.");
        }

        private static void ExplosionTargetCandidateNullUnitRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmExplosionTargetCandidate(
                    null,
                    "unit-1",
                    "Ally",
                    1f,
                    false),
                "A null explosion target unit was accepted.");
        }

        private static void ExplosionTargetCandidateValueUnitRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetCandidate(
                    42,
                    "unit-1",
                    "Ally",
                    1f,
                    false),
                "A value-type explosion target was accepted.");
        }

        private static void ExplosionTargetCandidateBlankIdentityRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetCandidate(
                    new object(),
                    " ",
                    "Ally",
                    1f,
                    false),
                "A blank stable target identity was accepted.");
        }

        private static void ExplosionTargetCandidateBlankNameRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetCandidate(
                    new object(),
                    "unit-1",
                    " ",
                    1f,
                    false),
                "A blank explosion target display name was accepted.");
        }

        private static void ExplosionTargetCandidateNegativeDistanceRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmExplosionTargetCandidate(
                    new object(),
                    "unit-1",
                    "Ally",
                    -0.01f,
                    false),
                "A negative explosion target distance was accepted.");
        }

        private static void ExplosionTargetCandidateNonfiniteDistanceRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmExplosionTargetCandidate(
                    new object(),
                    "unit-1",
                    "Ally",
                    float.NaN,
                    false),
                "A NaN explosion target distance was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmExplosionTargetCandidate(
                    new object(),
                    "unit-1",
                    "Ally",
                    float.PositiveInfinity,
                    false),
                "An infinite explosion target distance was accepted.");
        }

        private static void ExplosionTargetPlanExactOnly()
        {
            FirearmExplosionTargetCandidate wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            FirearmExplosionTargetPlan plan =
                new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new FirearmExplosionTargetCandidate[0]);
            Assertions.Equal(1, plan.TargetCount, "The exact-only plan had the wrong target count.");
            Assertions.Equal(0, plan.ObservedCandidates, "The exact-only plan observed nearby candidates.");
            Assertions.Equal(0, plan.DuplicateCandidates, "The exact-only plan found duplicates.");
            Assertions.True(
                ReferenceEquals(wielder, plan.Targets[0]),
                "The exact-only plan did not retain the exact wielder.");
        }

        private static void ExplosionTargetPlanOrdersAndWielderLast()
        {
            var wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            var farther = ExplosionCandidate(
                new object(),
                "unit-c",
                "Farther",
                1.5f,
                false);
            var tieB = ExplosionCandidate(
                new object(),
                "unit-b",
                "Tie B",
                0.5f,
                false);
            var tieA = ExplosionCandidate(
                new object(),
                "unit-a",
                "Tie A",
                0.5f,
                false);
            FirearmExplosionTargetPlan plan =
                new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new[] { farther, tieB, tieA });
            Assertions.Equal(4, plan.TargetCount, "The ordered plan lost a target.");
            Assertions.True(ReferenceEquals(tieA, plan.Targets[0]), "The stable tie order was wrong at index zero.");
            Assertions.True(ReferenceEquals(tieB, plan.Targets[1]), "The stable tie order was wrong at index one.");
            Assertions.True(ReferenceEquals(farther, plan.Targets[2]), "The farther target was not ordered after nearer targets.");
            Assertions.True(ReferenceEquals(wielder, plan.Targets[3]), "The exact wielder was not last.");
        }

        private static void ExplosionTargetPlanDedupesExactWielder()
        {
            object unit = new object();
            var wielder = ExplosionCandidate(
                unit,
                "wielder",
                "Wielder",
                0f,
                true);
            var queryCopy = ExplosionCandidate(
                unit,
                "wielder",
                "Wielder",
                0f,
                false);
            FirearmExplosionTargetPlan plan =
                new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new[] { queryCopy });
            Assertions.Equal(1, plan.TargetCount, "The query's exact-wielder duplicate was retained.");
            Assertions.Equal(1, plan.ObservedCandidates, "The exact-wielder query candidate was not observed.");
            Assertions.Equal(1, plan.DuplicateCandidates, "The exact-wielder query candidate was not counted as a duplicate.");
        }

        private static void ExplosionTargetPlanDedupesNearbyReference()
        {
            var wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            object nearbyUnit = new object();
            var first = ExplosionCandidate(
                nearbyUnit,
                "nearby-a",
                "Nearby A",
                1f,
                false);
            var duplicate = ExplosionCandidate(
                nearbyUnit,
                "nearby-b",
                "Nearby B",
                2f,
                false);
            FirearmExplosionTargetPlan plan =
                new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new[] { first, duplicate });
            Assertions.Equal(2, plan.TargetCount, "The nearby duplicate changed the unique target count.");
            Assertions.Equal(2, plan.ObservedCandidates, "The nearby duplicate was not observed.");
            Assertions.Equal(1, plan.DuplicateCandidates, "The nearby duplicate was not counted.");
            Assertions.True(ReferenceEquals(first, plan.Targets[0]), "The first nearby candidate was not retained.");
        }

        private static void ExplosionTargetPlanStableTieOrder()
        {
            var wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            var second = ExplosionCandidate(
                new object(),
                "same-id",
                "Zulu",
                1f,
                false);
            var first = ExplosionCandidate(
                new object(),
                "same-id",
                "Alpha",
                1f,
                false);
            FirearmExplosionTargetPlan plan =
                new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new[] { second, first });
            Assertions.True(ReferenceEquals(first, plan.Targets[0]), "Display name did not break a complete distance/identity tie.");
            Assertions.True(ReferenceEquals(second, plan.Targets[1]), "The stable display-name tie order was wrong.");
        }

        private static void ExplosionTargetPlanNullExactRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmExplosionTargetPlanService().Build(
                    null,
                    new FirearmExplosionTargetCandidate[0]),
                "A null exact wielder was accepted by the target planner.");
        }

        private static void ExplosionTargetPlanExactFlagRequired()
        {
            FirearmExplosionTargetCandidate notWielder = ExplosionCandidate(
                new object(),
                "unit",
                "Unit",
                0f,
                false);
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetPlanService().Build(
                    notWielder,
                    new FirearmExplosionTargetCandidate[0]),
                "An unmarked exact wielder was accepted by the target planner.");
        }

        private static void ExplosionTargetPlanNullNearbyRejected()
        {
            FirearmExplosionTargetCandidate wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    null),
                "A null nearby-target sequence was accepted.");
        }

        private static void ExplosionTargetPlanNullCandidateRejected()
        {
            FirearmExplosionTargetCandidate wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new FirearmExplosionTargetCandidate[] { null }),
                "A null nearby target candidate was accepted.");
        }

        private static void ExplosionTargetPlanNearbyExactFlagRejected()
        {
            FirearmExplosionTargetCandidate wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            FirearmExplosionTargetCandidate invalidNearby = ExplosionCandidate(
                new object(),
                "other",
                "Other",
                1f,
                true);
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetPlanService().Build(
                    wielder,
                    new[] { invalidNearby }),
                "A nearby candidate claiming exact-wielder status was accepted.");
        }

        private static void ExplosionTargetPlanConstructorRequiresWielderLast()
        {
            FirearmExplosionTargetCandidate wielder = ExplosionCandidate(
                new object(),
                "wielder",
                "Wielder",
                0f,
                true);
            FirearmExplosionTargetCandidate nearby = ExplosionCandidate(
                new object(),
                "nearby",
                "Nearby",
                1f,
                false);
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetPlan(
                    new[] { wielder, nearby },
                    1,
                    0),
                "A target plan with the exact wielder before a nearby target was accepted.");
        }

        private static void ExplosionTargetResultFormat()
        {
            var result = new FirearmExplosionTargetResult(
                "Ally",
                "unit-1",
                1.25f,
                false,
                13,
                23,
                true,
                true,
                8,
                8,
                4,
                20,
                16);
            Assertions.Equal(
                "target=Ally; unitId=unit-1; distanceMeters=1.25; exactWielder=False; reflexNaturalD20=13; reflexTotal=23; reflexPassed=True; halfBecauseSavingThrow=True; damageBeforeDifficulty=8; damageWithoutReduction=8; appliedDamage=4; hpBefore=20; hpAfter=16; hpLoss=4",
                result.ToString(),
                "Per-target explosion result formatting changed.");
        }

        private static void ExplosionTargetResultInvalidHalfFlagRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmExplosionTargetResult(
                    "Ally",
                    "unit-1",
                    1f,
                    false,
                    10,
                    12,
                    true,
                    false,
                    8,
                    8,
                    4,
                    20,
                    16),
                "A save result inconsistent with the native half-damage flag was accepted.");
        }

        private static void ExplosionTargetResultInvalidRollRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => NewExplosionTargetResult(0, 1f),
                "Natural d20 zero was accepted for target evidence.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => NewExplosionTargetResult(21, 1f),
                "Natural d20 twenty-one was accepted for target evidence.");
        }

        private static void ExplosionTargetResultInvalidDistanceRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => NewExplosionTargetResult(10, float.NaN),
                "NaN target-result distance was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => NewExplosionTargetResult(10, -1f),
                "Negative target-result distance was accepted.");
        }

        private static void ExplosionTargetResultAllowsNegativeHitPoints()
        {
            var result = new FirearmExplosionTargetResult(
                "Wielder",
                "unit-1",
                0f,
                true,
                1,
                1,
                false,
                false,
                10,
                10,
                10,
                -1,
                -11);
            Assertions.Equal(-1, result.HitPointsBefore, "Negative pre-damage HP evidence was not retained.");
            Assertions.Equal(-11, result.HitPointsAfter, "Negative post-damage HP evidence was not retained.");
            Assertions.True(result.ToString().Contains("hpLoss=10"), "Negative HP evidence formatted the loss incorrectly.");
        }

        private static void MisfireInvalidRollZero()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmMisfireService().Evaluate(0, 2, true),
                "Natural d20 zero was accepted.");
        }

        private static void MisfireInvalidRollTwentyOne()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmMisfireService().Evaluate(21, 2, true),
                "Natural d20 twenty-one was accepted.");
        }

        private static void MisfireZeroThresholdNaturalOneNativeMiss()
        {
            FirearmMisfireDecision nativeMiss =
                new FirearmMisfireService().Evaluate(1, 0, false);
            Assertions.False(nativeMiss.IsMisfire,
                "A zero effective threshold classified natural 1 as a misfire.");
            Assertions.False(nativeMiss.FinalSuccess,
                "A zero threshold converted native natural-1 miss into a hit.");
            Assertions.True(nativeMiss.ToString().Contains("misfireRange=none"),
                "Zero-threshold diagnostics displayed an impossible range.");
        }

        private static void MisfireInvalidThresholdTwentyOne()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmMisfireService().Evaluate(1, 21, true),
                "Misfire threshold twenty-one was accepted.");
        }

        private static void ForcedRollQueueEmpty()
        {
            var queue = new ForcedNaturalRollQueue();
            Assertions.Equal<int?>(null, queue.Pending, "A new forced-roll queue was not empty.");
        }

        private static void ForcedRollQueueSetConsume()
        {
            var queue = new ForcedNaturalRollQueue();
            Assertions.Equal<int?>(null, queue.Set(2), "The first queued roll unexpectedly replaced a value.");
            Assertions.Equal<int?>(2, queue.Pending, "The queued roll was not visible.");
            int naturalRoll;
            Assertions.True(queue.TryConsume(out naturalRoll), "The queued roll was not consumed.");
            Assertions.Equal(2, naturalRoll, "The consumed roll was incorrect.");
            Assertions.Equal<int?>(null, queue.Pending, "The queue was not empty after consumption.");
        }

        private static void ForcedRollQueueReplace()
        {
            var queue = new ForcedNaturalRollQueue();
            queue.Set(1);
            Assertions.Equal<int?>(1, queue.Set(20), "Replacing a queued roll did not return the previous value.");
            Assertions.Equal<int?>(20, queue.Pending, "The replacement roll was not retained.");
        }

        private static void ForcedRollQueueCancel()
        {
            var queue = new ForcedNaturalRollQueue();
            queue.Set(3);
            Assertions.Equal<int?>(3, queue.Cancel(), "Cancel did not return the queued roll.");
            Assertions.Equal<int?>(null, queue.Pending, "Cancel did not clear the queue.");
        }

        private static void ForcedRollQueueCancelEmpty()
        {
            var queue = new ForcedNaturalRollQueue();
            Assertions.Equal<int?>(null, queue.Cancel(), "Canceling an empty queue returned a value.");
        }

        private static void ForcedRollQueueConsumeEmpty()
        {
            var queue = new ForcedNaturalRollQueue();
            int naturalRoll;
            Assertions.False(queue.TryConsume(out naturalRoll), "An empty queue reported a consumed roll.");
            Assertions.Equal(0, naturalRoll, "An empty queue returned a nonzero sentinel.");
        }

        private static void ForcedRollQueueInvalidZero()
        {
            var queue = new ForcedNaturalRollQueue();
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => queue.Set(0),
                "Forced natural d20 zero was accepted.");
            Assertions.Equal<int?>(null, queue.Pending, "An invalid forced roll mutated the queue.");
        }

        private static void ForcedRollQueueInvalidTwentyOne()
        {
            var queue = new ForcedNaturalRollQueue();
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => queue.Set(21),
                "Forced natural d20 twenty-one was accepted.");
            Assertions.Equal<int?>(null, queue.Pending, "An invalid forced roll mutated the queue.");
        }

        private static void MisfirePatchRollSetterExact()
        {
            MethodInfo method = typeof(ExactMisfireRuleAttackRoll)
                .GetProperty(
                    "Roll",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .GetSetMethod(true);
            Assertions.True(
                FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(ExactMisfireRuleAttackRoll),
                    typeof(FakeRollEntry)),
                "The exact private Roll setter contract was rejected.");
        }

        private static void MisfirePatchRollSetterPublicRejected()
        {
            MethodInfo method = typeof(PublicRollSetterRuleAttackRoll)
                .GetProperty(
                    "Roll",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .GetSetMethod(true);
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(PublicRollSetterRuleAttackRoll),
                    typeof(FakeRollEntry)),
                "A public Roll setter was accepted.");
        }

        private static void MisfirePatchRollSetterWrongEntryRejected()
        {
            MethodInfo method = typeof(WrongRollEntrySetterRuleAttackRoll)
                .GetProperty(
                    "Roll",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .GetSetMethod(true);
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(WrongRollEntrySetterRuleAttackRoll),
                    typeof(FakeRollEntry)),
                "A Roll setter with the wrong entry type was accepted.");
        }

        private static void MisfirePatchRollSetterInheritedRejected()
        {
            MethodInfo method = typeof(BaseRollSetterRuleAttackRoll)
                .GetProperty(
                    "Roll",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .GetSetMethod(true);
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(InheritedRollSetterRuleAttackRoll),
                    typeof(FakeRollEntry)),
                "An inherited Roll setter was accepted for the derived rule type.");
        }

        private static void MisfirePatchRollSetterNullMethodRejected()
        {
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    null,
                    typeof(ExactMisfireRuleAttackRoll),
                    typeof(FakeRollEntry)),
                "A null Roll setter was accepted.");
        }

        private static void MisfirePatchRollSetterNullRuleTypeRejected()
        {
            MethodInfo method = typeof(ExactMisfireRuleAttackRoll)
                .GetProperty("Roll")
                .GetSetMethod(true);
            Assertions.Throws<ArgumentNullException>(
                () => FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    null,
                    typeof(FakeRollEntry)),
                "A null RuleAttackRoll type was accepted for the Roll setter contract.");
        }

        private static void MisfirePatchRollSetterNullEntryTypeRejected()
        {
            MethodInfo method = typeof(ExactMisfireRuleAttackRoll)
                .GetProperty("Roll")
                .GetSetMethod(true);
            Assertions.Throws<ArgumentNullException>(
                () => FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(ExactMisfireRuleAttackRoll),
                    null),
                "A null RollEntry type was accepted for the Roll setter contract.");
        }

        private static void MisfirePatchSuccessExact()
        {
            MethodInfo method = typeof(ExactMisfireRuleAttackRoll).GetMethod(
                "IsSuccessRoll",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assertions.True(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(ExactMisfireRuleAttackRoll)),
                "The exact IsSuccessRoll(int) contract was rejected.");
        }

        private static void MisfirePatchSuccessPrivateRejected()
        {
            MethodInfo method = typeof(PrivateSuccessRuleAttackRoll).GetMethod(
                "IsSuccessRoll",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(PrivateSuccessRuleAttackRoll)),
                "A private IsSuccessRoll method was accepted.");
        }

        private static void MisfirePatchSuccessWrongArgumentRejected()
        {
            MethodInfo method = typeof(WrongArgumentSuccessRuleAttackRoll).GetMethod("IsSuccessRoll");
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(WrongArgumentSuccessRuleAttackRoll)),
                "An IsSuccessRoll method with the wrong argument type was accepted.");
        }

        private static void MisfirePatchSuccessWrongReturnRejected()
        {
            MethodInfo method = typeof(WrongReturnSuccessRuleAttackRoll).GetMethod("IsSuccessRoll");
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(WrongReturnSuccessRuleAttackRoll)),
                "An IsSuccessRoll method with the wrong return type was accepted.");
        }

        private static void MisfirePatchSuccessStaticRejected()
        {
            MethodInfo method = typeof(StaticSuccessRuleAttackRoll).GetMethod("IsSuccessRoll");
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(StaticSuccessRuleAttackRoll)),
                "A static IsSuccessRoll method was accepted.");
        }

        private static void MisfirePatchSuccessGenericRejected()
        {
            MethodInfo method = typeof(GenericSuccessRuleAttackRoll).GetMethod("IsSuccessRoll");
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(GenericSuccessRuleAttackRoll)),
                "A generic IsSuccessRoll method was accepted.");
        }

        private static void MisfirePatchSuccessInheritedRejected()
        {
            MethodInfo method = typeof(BaseSuccessRuleAttackRoll).GetMethod(
                "IsSuccessRoll",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(InheritedSuccessRuleAttackRoll)),
                "An inherited IsSuccessRoll method was accepted for the derived rule type.");
        }

        private static void MisfirePatchSuccessNullMethodRejected()
        {
            Assertions.False(
                FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    null,
                    typeof(ExactMisfireRuleAttackRoll)),
                "A null IsSuccessRoll method was accepted.");
        }

        private static void MisfirePatchSuccessNullRuleTypeRejected()
        {
            MethodInfo method = typeof(ExactMisfireRuleAttackRoll).GetMethod("IsSuccessRoll");
            Assertions.Throws<ArgumentNullException>(
                () => FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    null),
                "A null RuleAttackRoll type was accepted for the IsSuccessRoll contract.");
        }

        private static void EventGateFirstAndDuplicate()
        {
            var gate = new ReferenceEventGate();
            var eventObject = new object();
            Assertions.True(gate.TryMark(eventObject), "First event observation was rejected.");
            Assertions.False(gate.TryMark(eventObject), "Duplicate event observation was accepted.");
        }

        private static void EventGateReferenceIdentity()
        {
            var gate = new ReferenceEventGate();
            var first = new ValueEqualItem(7);
            var second = new ValueEqualItem(7);
            Assertions.True(gate.TryMark(first), "First value-equal event was rejected.");
            Assertions.True(gate.TryMark(second), "Distinct value-equal event was not treated independently.");
        }

        private static void EventGateNull()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new ReferenceEventGate().TryMark(null),
                "A null event identity was accepted.");
        }

        private static void EventGateValueType()
        {
            Assertions.Throws<ArgumentException>(
                () => new ReferenceEventGate().TryMark(42),
                "A boxed value-type event identity was accepted.");
        }

        private static void PatchTargetExactContract()
        {
            Assertions.True(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(ExactRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "The exact Kingmaker OnTrigger contract was rejected.");
        }

        private static void PatchTargetNullMethodRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    null,
                    typeof(FakeRulebookEventContext)),
                "A null method was accepted as a patch target.");
        }

        private static void PatchTargetZeroArgumentRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(ZeroArgumentRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "The obsolete zero-argument OnTrigger shape was accepted.");
        }

        private static void PatchTargetWrongContextRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(WrongContextRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "An OnTrigger callback with the wrong context type was accepted.");
        }

        private static void PatchTargetMultipleArgumentsRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(MultipleArgumentRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "A multi-argument OnTrigger callback was accepted.");
        }

        private static void PatchTargetStaticRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(StaticRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "A static OnTrigger callback was accepted.");
        }

        private static void PatchTargetGenericRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(GenericRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "A generic OnTrigger callback was accepted.");
        }

        private static void PatchTargetNonVoidRejected()
        {
            Assertions.False(
                RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(NonVoidRuleEvent).GetMethod("OnTrigger"),
                    typeof(FakeRulebookEventContext)),
                "A non-void OnTrigger callback was accepted.");
        }

        private static void PatchTargetNullContextRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => RuleEventPatchContract.IsCompatibleOnTrigger(
                    typeof(ExactRuleEvent).GetMethod("OnTrigger"),
                    null),
                "A null rule-event context type was accepted.");
        }

        private static FirearmStateTokenReconciliationService Reconciler()
        {
            return new FirearmStateTokenReconciliationService();
        }

        private static void TokenReconcileNoToken()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                Array.Empty<string>(),
                Array.Empty<string>());
            Assertions.Equal(FirearmStateTokenReconciliationAction.NoToken, result.Action, "No-token action mismatch.");
            Assertions.Equal(null, result.TokenToRestore, "No-token decision requested restoration.");
        }

        private static void TokenReconcilePreserved()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                new[] { "loaded" },
                new[] { "loaded" });
            Assertions.Equal(FirearmStateTokenReconciliationAction.Preserved, result.Action, "Preserved action mismatch.");
            Assertions.Equal(null, result.TokenToRestore, "Preserved token requested restoration.");
        }

        private static void TokenReconcileRestore()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                new[] { "loaded" },
                Array.Empty<string>());
            Assertions.Equal(FirearmStateTokenReconciliationAction.RestoreMissing, result.Action, "Restore action mismatch.");
            Assertions.Equal("loaded", result.TokenToRestore, "Wrong state token selected for restoration.");
        }

        private static void TokenReconcileAppearedConflict()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                Array.Empty<string>(),
                new[] { "loaded" });
            Assertions.Equal(FirearmStateTokenReconciliationAction.Conflict, result.Action, "Appeared-token conflict was not detected.");
            Assertions.Equal(null, result.TokenToRestore, "Conflict must not request restoration.");
        }

        private static void TokenReconcileChangedConflict()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                new[] { "loaded" },
                new[] { "broken" });
            Assertions.Equal(FirearmStateTokenReconciliationAction.Conflict, result.Action, "Changed-token conflict was not detected.");
        }

        private static void TokenReconcileMultipleBeforeConflict()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                new[] { "loaded", "broken" },
                Array.Empty<string>());
            Assertions.Equal(FirearmStateTokenReconciliationAction.Conflict, result.Action, "Multiple before tokens were not rejected.");
        }

        private static void TokenReconcileMultipleAfterConflict()
        {
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(
                new[] { "loaded" },
                new[] { "loaded", "broken" });
            Assertions.Equal(FirearmStateTokenReconciliationAction.Conflict, result.Action, "Multiple after tokens were not rejected.");
        }

        private static void TokenReconcileNullBefore()
        {
            Assertions.Throws<ArgumentNullException>(
                () => Reconciler().Evaluate(null, Array.Empty<string>()),
                "A null before-token collection was accepted.");
        }

        private static void TokenReconcileBlankToken()
        {
            Assertions.Throws<ArgumentException>(
                () => Reconciler().Evaluate(new[] { " " }, Array.Empty<string>()),
                "A blank state-token ID was accepted.");
        }

        private static void TokenReconcileFormatAndDefensiveCopy()
        {
            string[] before = { "loaded" };
            string[] after = Array.Empty<string>();
            FirearmStateTokenReconciliationDecision result = Reconciler().Evaluate(before, after);
            before[0] = "changed-after-construction";
            Assertions.Equal("loaded", result.Before[0], "Decision did not defensively copy before tokens.");
            string text = result.ToString();
            Assertions.True(text.Contains("action=RestoreMissing"), "Reconciliation format omitted action.");
            Assertions.True(text.Contains("restore=loaded"), "Reconciliation format omitted restoration token.");
        }

        private static void StateAmmunitionIdValid()
        {
            AmmunitionId ammunition = new AmmunitionId("kmg.ammunition:lead_ball-1");
            Assertions.Equal("kmg.ammunition:lead_ball-1", ammunition.Value, "Ammunition ID value mismatch.");
            Assertions.Equal(ammunition.Value, ammunition.ToString(), "Ammunition ID formatting mismatch.");
        }

        private static void StateAmmunitionIdValueEquality()
        {
            AmmunitionId left = LeadBall();
            AmmunitionId right = LeadBall();
            Assertions.True(left.Equals(right), "Equal ammunition IDs must compare equal.");
            Assertions.True(left == right, "Ammunition equality operator mismatch.");
            Assertions.False(left != right, "Ammunition inequality operator mismatch.");
            Assertions.Equal(left.GetHashCode(), right.GetHashCode(), "Equal ammunition IDs require equal hashes.");
            Assertions.True(left.CompareTo(PaperCartridge()) < 0, "Ammunition ordering must use ordinal value order.");
        }

        private static void StateAmmunitionIdUppercaseRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                new AmmunitionId("Kmg.ammunition.lead-ball"),
                "Uppercase ammunition IDs must fail.");
        }

        private static void StateAmmunitionIdNullRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                new AmmunitionId(null),
                "Null ammunition IDs must fail.");
        }

        private static void StateAmmunitionIdEmptyRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                new AmmunitionId(string.Empty),
                "Empty ammunition IDs must fail.");
        }

        private static void StateAmmunitionIdLeadingSeparatorRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                new AmmunitionId(".kmg.ammunition.lead-ball"),
                "A leading separator must fail.");
        }

        private static void StateAmmunitionIdWhitespaceRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                new AmmunitionId("kmg.ammunition lead-ball"),
                "Whitespace in an ammunition ID must fail.");
        }

        private static void StateAmmunitionIdTooLongRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new AmmunitionId(new string('a', AmmunitionId.MaximumLength + 1)),
                "Oversized ammunition IDs must fail.");
        }

        private static void StateRulesValid()
        {
            FirearmStateRules rules = StateRules(2, LeadBall(), PaperCartridge());
            Assertions.Equal(2, rules.Capacity, "State-rule capacity mismatch.");
            Assertions.Equal(2, rules.CompatibleAmmunitionCount, "State-rule ammunition count mismatch.");
            Assertions.True(rules.IsCompatible(LeadBall()), "Lead ball must be compatible.");
            Assertions.False(rules.IsCompatible(AlchemicalRound()), "Unlisted ammunition must be incompatible.");
            Assertions.False(rules.IsCompatible(null), "Null ammunition must be incompatible.");
        }

        private static void StateRulesReturnsSortedCopy()
        {
            FirearmStateRules rules = StateRules(2, PaperCartridge(), LeadBall());
            AmmunitionId[] first = rules.GetCompatibleAmmunition();
            Assertions.Equal(LeadBall(), first[0], "Compatible ammunition must be sorted.");
            Assertions.Equal(PaperCartridge(), first[1], "Compatible ammunition sort mismatch.");
            first[0] = AlchemicalRound();
            AmmunitionId[] second = rules.GetCompatibleAmmunition();
            Assertions.Equal(LeadBall(), second[0], "Returned ammunition arrays must be defensive copies.");
            Assertions.Equal(
                "capacity=2; compatible=[kmg.ammunition.lead-ball,kmg.ammunition.paper-cartridge]",
                rules.ToString(),
                "State-rule formatting changed.");
        }

        private static void StateRulesCapacityZeroRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StateRules(0, LeadBall()),
                "Zero state capacity must fail.");
        }

        private static void StateRulesCapacityTooLargeRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StateRules(FirearmDefinition.MaximumCapacity + 1, LeadBall()),
                "Oversized state capacity must fail.");
        }

        private static void StateRulesNullCollectionRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                new FirearmStateRules(1, null),
                "A null compatibility collection must fail.");
        }

        private static void StateRulesEmptyCollectionRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                StateRules(1, new AmmunitionId[0]),
                "An empty compatibility collection must fail.");
        }

        private static void StateRulesNullEntryRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                StateRules(1, LeadBall(), null),
                "A null compatibility entry must fail.");
        }

        private static void StateRulesDuplicateRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                StateRules(1, LeadBall(), LeadBall()),
                "Duplicate compatibility IDs must fail.");
        }

        private static void StateEmptyCanonical()
        {
            FirearmState state = FirearmState.CreateEmpty();
            Assertions.Equal(FirearmState.CurrentSchemaVersion, state.SchemaVersion, "State schema mismatch.");
            Assertions.Equal(0, state.LoadedRounds, "Empty state round count mismatch.");
            Assertions.Equal<AmmunitionId>(null, state.LoadedAmmunition, "Empty state must not have ammunition.");
            Assertions.Equal(FirearmCondition.Normal, state.Condition, "Empty state condition mismatch.");
            Assertions.True(state.IsEmpty, "Canonical empty state must report empty.");
        }

        private static void StateValueEquality()
        {
            FirearmState left = LoadedState(2, LeadBall(), FirearmCondition.Broken);
            FirearmState right = LoadedState(2, LeadBall(), FirearmCondition.Broken);
            Assertions.True(left.Equals(right), "Equal firearm states must compare equal.");
            Assertions.True(left == right, "Firearm-state equality operator mismatch.");
            Assertions.False(left != right, "Firearm-state inequality operator mismatch.");
            Assertions.Equal(left.GetHashCode(), right.GetHashCode(), "Equal firearm states require equal hashes.");
        }

        private static void StateFormattingDeterministic()
        {
            FirearmState state = LoadedState(1, LeadBall(), FirearmCondition.Broken);
            Assertions.Equal(
                "schema=1; rounds=1; ammunition=kmg.ammunition.lead-ball; condition=Broken",
                state.ToString(),
                "Firearm-state formatting changed.");
        }

        private static void StateInvalidSchemaRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new FirearmState(2, 0, null, FirearmCondition.Normal),
                "Unsupported state schemas must fail.");
        }

        private static void StateNegativeRoundsRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new FirearmState(FirearmState.CurrentSchemaVersion, -1, null, FirearmCondition.Normal),
                "Negative loaded rounds must fail.");
        }

        private static void StateLoadedWithoutAmmunitionRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                new FirearmState(FirearmState.CurrentSchemaVersion, 1, null, FirearmCondition.Normal),
                "Loaded state without ammunition must fail.");
        }

        private static void StateEmptyWithAmmunitionRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                new FirearmState(FirearmState.CurrentSchemaVersion, 0, LeadBall(), FirearmCondition.Normal),
                "Empty state retaining ammunition must fail.");
        }

        private static void StateUnknownConditionRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new FirearmState(FirearmState.CurrentSchemaVersion, 0, null, FirearmCondition.Unknown),
                "Unknown state condition must fail.");
        }

        private static void StateWreckedLoadedRejected()
        {
            Assertions.Throws<ArgumentException>(() =>
                LoadedState(1, LeadBall(), FirearmCondition.Wrecked),
                "A wrecked firearm cannot remain loaded.");
        }

        private static void StateLoadEmpty()
        {
            FirearmState original = FirearmState.CreateEmpty();
            FirearmState loaded = FirearmStateMachine.Load(original, StateRules(1, LeadBall()), LeadBall(), 1);
            Assertions.True(original.IsEmpty, "Load must not mutate the original state.");
            Assertions.Equal(1, loaded.LoadedRounds, "Load transition round count mismatch.");
            Assertions.Equal(LeadBall(), loaded.LoadedAmmunition, "Load transition ammunition mismatch.");
            Assertions.Equal(FirearmCondition.Normal, loaded.Condition, "Load transition condition changed.");
        }

        private static void StateLoadPartialToCapacity()
        {
            FirearmState original = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            FirearmState loaded = FirearmStateMachine.Load(original, StateRules(3, LeadBall()), LeadBall(), 2);
            Assertions.Equal(1, original.LoadedRounds, "Partial load mutated the original state.");
            Assertions.Equal(3, loaded.LoadedRounds, "Partial load did not reach capacity.");
        }

        private static void StateLoadBroken()
        {
            FirearmState broken = LoadedState(0, null, FirearmCondition.Broken);
            FirearmState loaded = FirearmStateMachine.Load(broken, StateRules(1, LeadBall()), LeadBall(), 1);
            Assertions.Equal(FirearmCondition.Broken, loaded.Condition, "Loading a broken firearm changed condition.");
            Assertions.Equal(1, loaded.LoadedRounds, "Broken firearm did not load.");
        }

        private static void StateLoadOverCapacityRejected()
        {
            FirearmState original = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            AssertTransitionError(
                FirearmStateTransitionError.CapacityExceeded,
                () => FirearmStateMachine.Load(original, StateRules(1, LeadBall()), LeadBall(), 1),
                "Over-capacity load must fail.");
            Assertions.Equal(1, original.LoadedRounds, "Rejected load mutated the original state.");
        }

        private static void StateLoadIncompatibleRejected()
        {
            FirearmState original = FirearmState.CreateEmpty();
            AssertTransitionError(
                FirearmStateTransitionError.IncompatibleAmmunition,
                () => FirearmStateMachine.Load(original, StateRules(1, LeadBall()), PaperCartridge(), 1),
                "Incompatible ammunition must fail.");
            Assertions.True(original.IsEmpty, "Rejected incompatible load mutated the original state.");
        }

        private static void StateLoadMixedRejected()
        {
            FirearmState original = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            AssertTransitionError(
                FirearmStateTransitionError.MixedAmmunition,
                () => FirearmStateMachine.Load(
                    original,
                    StateRules(2, LeadBall(), PaperCartridge()),
                    PaperCartridge(),
                    1),
                "Mixed loaded ammunition must fail.");
            Assertions.Equal(LeadBall(), original.LoadedAmmunition, "Rejected mixed load mutated ammunition.");
        }

        private static void StateLoadZeroRoundsRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmStateMachine.Load(
                    FirearmState.CreateEmpty(),
                    StateRules(1, LeadBall()),
                    LeadBall(),
                    0),
                "Zero-round load must fail.");
        }

        private static void StateLoadNullStateRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmStateMachine.Load(null, StateRules(1, LeadBall()), LeadBall(), 1),
                "Null load state must fail.");
        }

        private static void StateLoadNullRulesRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmStateMachine.Load(FirearmState.CreateEmpty(), null, LeadBall(), 1),
                "Null load rules must fail.");
        }

        private static void StateLoadInvalidExistingStateRejected()
        {
            FirearmState invalidForRules = LoadedState(2, LeadBall(), FirearmCondition.Normal);
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateMachine.Load(invalidForRules, StateRules(1, LeadBall()), LeadBall(), 1),
                "A state already exceeding supplied capacity must fail.");
        }

        private static void StateFireConsumesOne()
        {
            FirearmState original = LoadedState(2, LeadBall(), FirearmCondition.Normal);
            FirearmState fired = FirearmStateMachine.Fire(original);
            Assertions.Equal(2, original.LoadedRounds, "Fire transition mutated original state.");
            Assertions.Equal(1, fired.LoadedRounds, "Fire transition must consume exactly one round.");
            Assertions.Equal(LeadBall(), fired.LoadedAmmunition, "Remaining loaded ammunition was lost.");
        }

        private static void StateFireFinalRoundClearsAmmunition()
        {
            FirearmState fired = FirearmStateMachine.Fire(
                LoadedState(1, LeadBall(), FirearmCondition.Normal));
            Assertions.Equal(0, fired.LoadedRounds, "Final fire transition did not empty firearm.");
            Assertions.Equal<AmmunitionId>(null, fired.LoadedAmmunition, "Final round must clear ammunition identity.");
            Assertions.True(fired.IsEmpty, "Final fire transition must report empty.");
        }

        private static void StateFireBroken()
        {
            FirearmState fired = FirearmStateMachine.Fire(
                LoadedState(1, LeadBall(), FirearmCondition.Broken));
            Assertions.Equal(FirearmCondition.Broken, fired.Condition, "Firing changed broken condition.");
            Assertions.True(fired.IsEmpty, "Broken firearm fire must consume one round.");
        }

        private static void StateFireEmptyRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.Empty,
                () => FirearmStateMachine.Fire(FirearmState.CreateEmpty()),
                "Empty firearm fire must fail.");
        }

        private static void StateFireWreckedRejected()
        {
            FirearmState wrecked = LoadedState(0, null, FirearmCondition.Wrecked);
            AssertTransitionError(
                FirearmStateTransitionError.Wrecked,
                () => FirearmStateMachine.Fire(wrecked),
                "Wrecked firearm fire must fail.");
        }

        private static void StateMisfireNormalToBroken()
        {
            FirearmState original = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            FirearmState damaged = FirearmStateMachine.ApplyMisfireDamage(original);
            Assertions.Equal(FirearmCondition.Normal, original.Condition, "Misfire damage mutated original state.");
            Assertions.Equal(FirearmCondition.Broken, damaged.Condition, "Normal firearm did not become broken.");
            Assertions.Equal(1, damaged.LoadedRounds, "Normal-to-broken transition must preserve payload.");
            Assertions.Equal(LeadBall(), damaged.LoadedAmmunition, "Normal-to-broken transition lost ammunition.");
        }

        private static void StateMisfireBrokenToWrecked()
        {
            FirearmState damaged = FirearmStateMachine.ApplyMisfireDamage(
                LoadedState(1, LeadBall(), FirearmCondition.Broken));
            Assertions.Equal(FirearmCondition.Wrecked, damaged.Condition, "Broken firearm did not become wrecked.");
            Assertions.True(damaged.IsEmpty, "Wrecked firearm must be empty.");
            Assertions.Equal<AmmunitionId>(null, damaged.LoadedAmmunition, "Wrecked firearm retained ammunition.");
        }

        private static void StateMisfireWreckedRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.Wrecked,
                () => FirearmStateMachine.ApplyMisfireDamage(
                    LoadedState(0, null, FirearmCondition.Wrecked)),
                "Repeated misfire damage on a wrecked firearm must fail.");
        }

        private static void StateRepairBrokenToNormal()
        {
            FirearmState original = LoadedState(1, LeadBall(), FirearmCondition.Broken);
            FirearmState repaired = FirearmStateMachine.Repair(original);
            Assertions.Equal(FirearmCondition.Broken, original.Condition, "Repair mutated original state.");
            Assertions.Equal(FirearmCondition.Normal, repaired.Condition, "Broken firearm did not become normal.");
            Assertions.Equal(1, repaired.LoadedRounds, "Repair must preserve loaded rounds.");
            Assertions.Equal(LeadBall(), repaired.LoadedAmmunition, "Repair must preserve loaded ammunition.");
        }

        private static void StateRepairNormalRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.NotBroken,
                () => FirearmStateMachine.Repair(FirearmState.CreateEmpty()),
                "Repairing a normal firearm must fail.");
        }

        private static void StateRepairWreckedRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.Wrecked,
                () => FirearmStateMachine.Repair(
                    LoadedState(0, null, FirearmCondition.Wrecked)),
                "A wrecked firearm must not silently repair to normal.");
        }

        private static void StateOverhaulWreckedToBroken()
        {
            FirearmState original = LoadedState(0, null, FirearmCondition.Wrecked);
            FirearmState overhauled = FirearmStateMachine.OverhaulWrecked(original);
            Assertions.Equal(FirearmCondition.Wrecked, original.Condition, "Overhaul mutated the original state.");
            Assertions.True(original.IsEmpty, "Original Wrecked state must remain empty.");
            Assertions.Equal(FirearmCondition.Broken, overhauled.Condition, "Wrecked firearm did not become Broken.");
            Assertions.True(overhauled.IsEmpty, "Overhauled firearm must remain empty.");
            Assertions.Equal<AmmunitionId>(null, overhauled.LoadedAmmunition, "Overhaul must not manufacture ammunition.");
        }

        private static void StateOverhaulNormalRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.NotWrecked,
                () => FirearmStateMachine.OverhaulWrecked(FirearmState.CreateEmpty()),
                "Overhauling a Normal firearm must fail.");
        }

        private static void StateOverhaulBrokenRejected()
        {
            AssertTransitionError(
                FirearmStateTransitionError.NotWrecked,
                () => FirearmStateMachine.OverhaulWrecked(
                    LoadedState(0, null, FirearmCondition.Broken)),
                "Overhauling a Broken firearm must fail.");
        }

        private static void StateWreckNormalClearsLoad()
        {
            FirearmState original = LoadedState(2, LeadBall(), FirearmCondition.Normal);
            FirearmState wrecked = FirearmStateMachine.Wreck(original);
            Assertions.Equal(2, original.LoadedRounds, "Wreck transition mutated original state.");
            Assertions.Equal(FirearmCondition.Wrecked, wrecked.Condition, "Wreck transition condition mismatch.");
            Assertions.True(wrecked.IsEmpty, "Wreck transition must clear loaded rounds.");
        }

        private static void StateWreckIsIdempotent()
        {
            FirearmState original = LoadedState(0, null, FirearmCondition.Wrecked);
            FirearmState result = FirearmStateMachine.Wreck(original);
            Assertions.True(ReferenceEquals(original, result), "Wrecking an already wrecked state should be idempotent.");
        }

        private static void StateCodecEmptyRoundTrip()
        {
            FirearmState state = FirearmState.CreateEmpty();
            FirearmStateData data = FirearmStateCodec.ToData(state);
            FirearmState restored = FirearmStateCodec.FromData(data, StateRules(1, LeadBall()));
            Assertions.Equal(state, restored, "Empty firearm-state round trip mismatch.");
        }

        private static void StateCodecLoadedBrokenRoundTrip()
        {
            FirearmState state = LoadedState(2, LeadBall(), FirearmCondition.Broken);
            FirearmStateData data = FirearmStateCodec.ToData(state);
            FirearmState restored = FirearmStateCodec.FromData(data, StateRules(2, LeadBall()));
            Assertions.Equal(state, restored, "Loaded broken firearm-state round trip mismatch.");
        }

        private static void StateCodecCanonicalDto()
        {
            FirearmStateData data = FirearmStateCodec.ToData(
                LoadedState(1, LeadBall(), FirearmCondition.Broken));
            Assertions.Equal(1, data.SchemaVersion, "DTO schema mismatch.");
            Assertions.Equal(1, data.LoadedRounds, "DTO round count mismatch.");
            Assertions.Equal(LeadBall().Value, data.LoadedAmmunitionId, "DTO ammunition mismatch.");
            Assertions.Equal("broken", data.Condition, "DTO condition token mismatch.");
            Assertions.Equal(
                "{\"schemaVersion\":1,\"loadedRounds\":1,\"loadedAmmunitionId\":\"kmg.ammunition.lead-ball\",\"condition\":\"broken\"}",
                data.ToString(),
                "DTO canonical formatting changed.");
        }

        private static void StateCodecWrongSchemaRejected()
        {
            FirearmStateData data = EmptyStateData();
            data.SchemaVersion = 2;
            Assertions.Throws<NotSupportedException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Unsupported DTO schemas must fail.");
        }

        private static void StateCodecUnknownConditionRejected()
        {
            FirearmStateData data = EmptyStateData();
            data.Condition = "jammed";
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Unknown DTO condition tokens must fail.");
        }

        private static void StateCodecCaseSensitiveCondition()
        {
            FirearmStateData data = EmptyStateData();
            data.Condition = "Normal";
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "DTO condition tokens must be case-sensitive.");
        }

        private static void StateCodecOverCapacityRejected()
        {
            FirearmStateData data = LoadedStateData(2, LeadBall().Value, "normal");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "DTO rounds above capacity must fail.");
        }

        private static void StateCodecIncompatibleAmmunitionRejected()
        {
            FirearmStateData data = LoadedStateData(1, PaperCartridge().Value, "normal");
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Incompatible DTO ammunition must fail.");
        }

        private static void StateCodecLoadedMissingAmmunitionRejected()
        {
            FirearmStateData data = LoadedStateData(1, null, "normal");
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Loaded DTO without ammunition must fail.");
        }

        private static void StateCodecEmptyWithAmmunitionRejected()
        {
            FirearmStateData data = LoadedStateData(0, LeadBall().Value, "normal");
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Empty DTO retaining ammunition must fail.");
        }

        private static void StateCodecWreckedLoadedRejected()
        {
            FirearmStateData data = LoadedStateData(1, LeadBall().Value, "wrecked");
            Assertions.Throws<ArgumentException>(() =>
                FirearmStateCodec.FromData(data, StateRules(1, LeadBall())),
                "Loaded wrecked DTO must fail.");
        }

        private static void StateCodecNullDataRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmStateCodec.FromData(null, StateRules(1, LeadBall())),
                "Null state DTO must fail.");
        }

        private static void StateCodecNullRulesRejected()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmStateCodec.FromData(EmptyStateData(), null),
                "Null DTO rules must fail.");
        }

        private static void TokenDefinitionValid()
        {
            var definition = new FirearmStateTokenDefinition("kmg.state.valid", TokenLoadedNormal());
            Assertions.Equal("kmg.state.valid", definition.TokenId, "Token ID mismatch.");
            Assertions.Equal(TokenLoadedNormal(), definition.State, "Token state mismatch.");
        }

        private static void TokenDefinitionValueEquality()
        {
            var first = new FirearmStateTokenDefinition("kmg.state.valid", TokenLoadedNormal());
            var second = new FirearmStateTokenDefinition("kmg.state.valid", TokenLoadedNormal());
            Assertions.Equal(first, second, "Equal token definitions must compare by value.");
            Assertions.Equal(first.GetHashCode(), second.GetHashCode(), "Equal token definitions need equal hashes.");
        }

        private static void TokenDefinitionNullStateRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmStateTokenDefinition("kmg.state.valid", null),
                "A token definition needs a state.");
        }

        private static void TokenDefinitionEmptyIdRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmStateTokenDefinition("", TokenLoadedNormal()),
                "An empty token ID must be rejected.");
        }

        private static void TokenDefinitionUppercaseIdRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmStateTokenDefinition("KMG.state.invalid", TokenLoadedNormal()),
                "Uppercase token IDs must be rejected.");
        }

        private static void TokenCatalogContainsFourDefinitions()
        {
            Assertions.Equal(6, TokenCatalog().Definitions.Count,
                "The capacity-one catalog must include four legacy and two paper states.");
        }

        private static void TokenCatalogDefinitionsAreSorted()
        {
            string[] actual = TokenCatalog().Definitions.Select(value => value.TokenId).ToArray();
            string[] sorted = actual.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.Equal(string.Join("|", sorted), string.Join("|", actual), "Token definitions must be deterministic.");
        }

        private static void TokenCatalogDefinitionsAreDefensiveCopy()
        {
            FirearmStateTokenCatalog catalog = TokenCatalog();
            var copy = catalog.Definitions as FirearmStateTokenDefinition[];
            Assertions.True(copy != null, "The test expects an array-backed defensive copy.");
            copy[0] = null;
            Assertions.True(catalog.Definitions[0] != null, "Mutating a returned definitions array changed the catalog.");
        }

        private static void TokenCatalogAbsenceMeansEmptyNormal()
        {
            FirearmStateTokenCatalog catalog = TokenCatalog();
            Assertions.Equal(FirearmState.CreateEmpty(), catalog.Decode(Array.Empty<string>()), "Token absence must be empty/normal.");
            Assertions.Equal(null, catalog.Encode(FirearmState.CreateEmpty()), "Empty/normal must encode as token absence.");
        }

        private static void TokenCatalogLoadedNormalRoundTrip()
        {
            AssertTokenRoundTrip(TokenLoadedNormal(), FirearmStateTokenCatalog.LoadedNormalTokenId);
        }

        private static void TokenCatalogBrokenEmptyRoundTrip()
        {
            AssertTokenRoundTrip(TokenBrokenEmpty(), FirearmStateTokenCatalog.BrokenEmptyTokenId);
        }

        private static void TokenCatalogBrokenLoadedRoundTrip()
        {
            AssertTokenRoundTrip(TokenBrokenLoaded(), FirearmStateTokenCatalog.BrokenLoadedTokenId);
        }

        private static void TokenCatalogWreckedRoundTrip()
        {
            AssertTokenRoundTrip(TokenWrecked(), FirearmStateTokenCatalog.WreckedTokenId);
        }

        private static void AssertTokenRoundTrip(FirearmState state, string tokenId)
        {
            FirearmStateTokenCatalog catalog = TokenCatalog();
            Assertions.Equal(tokenId, catalog.Encode(state), "Encoded token mismatch.");
            Assertions.Equal(state, catalog.Decode(new[] { tokenId }), "Decoded state mismatch.");
        }

        private static void TokenCatalogUnknownRejected()
        {
            Assertions.Throws<NotSupportedException>(
                () => TokenCatalog().Decode(new[] { "kmg.state.v99.future" }),
                "Future tokens must fail closed.");
        }

        private static void TokenCatalogDuplicatePayloadRejected()
        {
            Assertions.Throws<InvalidDataException>(
                () => TokenCatalog().Decode(new[]
                {
                    FirearmStateTokenCatalog.LoadedNormalTokenId,
                    FirearmStateTokenCatalog.BrokenEmptyTokenId
                }),
                "More than one state token must be rejected.");
        }

        private static void TokenCatalogNullPayloadRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => TokenCatalog().Decode(null),
                "A null token collection must be rejected.");
        }

        private static void TokenCatalogNullEntryRejected()
        {
            Assertions.Throws<InvalidDataException>(
                () => TokenCatalog().Decode(new string[] { null }),
                "A null token entry must be rejected.");
        }

        private static void TokenCatalogEncodeNullRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => TokenCatalog().Encode(null),
                "A null state must be rejected.");
        }

        private static void TokenCatalogUnsupportedStateRejected()
        {
            FirearmState unsupported = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                new AmmunitionId("kmg.ammunition.other"),
                FirearmCondition.Normal);
            Assertions.Throws<NotSupportedException>(
                () => TokenCatalog().Encode(unsupported),
                "An unsupported ammunition state must fail closed.");
        }

        private static void TokenCatalogDuplicateIdRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmStateTokenCatalog(new[]
                {
                    new FirearmStateTokenDefinition("kmg.state.same", TokenLoadedNormal()),
                    new FirearmStateTokenDefinition("kmg.state.same", TokenBrokenEmpty())
                }),
                "Duplicate token IDs must be rejected.");
        }

        private static void TokenCatalogDuplicateStateRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmStateTokenCatalog(new[]
                {
                    new FirearmStateTokenDefinition("kmg.state.one", TokenLoadedNormal()),
                    new FirearmStateTokenDefinition("kmg.state.two", TokenLoadedNormal())
                }),
                "Duplicate encoded states must be rejected.");
        }

        private static void TokenCatalogDefaultDefinitionRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmStateTokenCatalog(new[]
                {
                    new FirearmStateTokenDefinition("kmg.state.default", FirearmState.CreateEmpty())
                }),
                "The default state must remain token absence.");
        }

        private static void TokenCatalogRequireUnknownRejected()
        {
            Assertions.Throws<KeyNotFoundException>(
                () => TokenCatalog().RequireDefinition("kmg.state.missing"),
                "Unknown catalog definitions must be rejected.");
        }

        private static void TokenCatalogContainsKnownOnly()
        {
            FirearmStateTokenCatalog catalog = TokenCatalog();
            Assertions.True(catalog.ContainsToken(FirearmStateTokenCatalog.WreckedTokenId), "Known token missing.");
            Assertions.False(catalog.ContainsToken("kmg.state.future"), "Unknown token accepted.");
            Assertions.False(catalog.ContainsToken(null), "Null token accepted.");
        }

        private static void RepositoryReferenceComparerSame()
        {
            object item = new object();
            var comparer = (IEqualityComparer<object>)ReferenceIdentityComparer.Instance;
            Assertions.True(comparer.Equals(item, item), "The same reference must compare equal.");
            Assertions.Equal(
                comparer.GetHashCode(item),
                comparer.GetHashCode(item),
                "Reference hash must be stable for one live object.");
        }

        private static void RepositoryReferenceComparerDistinctValueEqual()
        {
            object first = new ValueEqualItem(7);
            object second = new ValueEqualItem(7);
            var comparer = (IEqualityComparer<object>)ReferenceIdentityComparer.Instance;
            Assertions.True(first.Equals(second), "Test values must be value-equal.");
            Assertions.False(
                comparer.Equals(first, second),
                "Distinct value-equal references must remain distinct.");
        }

        private static void RepositoryUnseenCreatesEmpty()
        {
            var repository = new WeakFirearmStateRepository();
            FirearmStateRepositorySnapshot snapshot = repository.GetOrCreate(new object());
            Assertions.Equal(FirearmState.CreateEmpty(), snapshot.State, "Unseen item must begin empty/normal.");
            Assertions.Equal(0, snapshot.Revision, "New entry revision must be zero.");
            Assertions.Equal(1L, repository.CreatedEntryCount, "One entry should be created.");
        }

        private static void RepositorySameReferenceStableEntry()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot first = repository.GetOrCreate(item);
            FirearmStateRepositorySnapshot second = repository.GetOrCreate(item);
            Assertions.Equal(first.EntryId, second.EntryId, "Same item reference must retain entry ID.");
            Assertions.Equal(1L, repository.CreatedEntryCount, "Same reference must not create twice.");
        }

        private static void RepositoryDistinctEqualSeparateEntries()
        {
            var repository = new WeakFirearmStateRepository();
            object first = new ValueEqualItem(9);
            object second = new ValueEqualItem(9);
            FirearmStateRepositorySnapshot firstSnapshot = repository.GetOrCreate(first);
            FirearmStateRepositorySnapshot secondSnapshot = repository.GetOrCreate(second);
            Assertions.True(
                firstSnapshot.EntryId != secondSnapshot.EntryId,
                "Distinct value-equal items must receive separate entries.");
            Assertions.Equal(2L, repository.CreatedEntryCount, "Two exact references should create two entries.");
        }

        private static void RepositorySetIsIsolated()
        {
            var repository = new WeakFirearmStateRepository();
            object first = new object();
            object second = new object();
            repository.GetOrCreate(second);
            FirearmState loaded = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            repository.Set(first, loaded);
            Assertions.Equal(loaded, repository.GetOrCreate(first).State, "First item state mismatch.");
            Assertions.Equal(FirearmState.CreateEmpty(), repository.GetOrCreate(second).State, "Second item leaked state.");
        }

        private static void RepositorySetIncrementsRevision()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            repository.GetOrCreate(item);
            FirearmStateRepositorySnapshot updated = repository.Set(
                item,
                LoadedState(1, LeadBall(), FirearmCondition.Normal));
            Assertions.Equal(1, updated.Revision, "Changed set must increment revision.");
            Assertions.Equal(1L, repository.MutationCount, "Changed set must increment mutation count.");
        }

        private static void RepositoryNoOpSetDoesNotIncrementRevision()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot updated = repository.Set(item, FirearmState.CreateEmpty());
            Assertions.Equal(0, updated.Revision, "Equal set must remain revision zero.");
            Assertions.Equal(0L, repository.MutationCount, "Equal set must not count as mutation.");
        }

        private static void RepositoryTransitionIncrementsRevision()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRules rules = StateRules(1, LeadBall());
            FirearmStateRepositorySnapshot updated = repository.Transition(
                item,
                state => FirearmStateMachine.Load(state, rules, LeadBall(), 1));
            Assertions.Equal(1, updated.Revision, "Changed transition must increment revision.");
            Assertions.Equal(1, updated.State.LoadedRounds, "Transition must store returned state.");
        }

        private static void RepositoryRejectedTransitionPreservesState()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot before = repository.GetOrCreate(item);
            Assertions.Throws<FirearmStateTransitionException>(
                () => repository.Transition(item, FirearmStateMachine.Fire),
                "Rejected transition must escape unchanged.");
            FirearmStateRepositorySnapshot after = repository.GetOrCreate(item);
            Assertions.Equal(before.State, after.State, "Rejected transition changed state.");
            Assertions.Equal(before.Revision, after.Revision, "Rejected transition changed revision.");
            Assertions.Equal(0L, repository.MutationCount, "Rejected transition counted as mutation.");
        }

        private static void RepositoryNullTransitionResultPreservesState()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot before = repository.GetOrCreate(item);
            Assertions.Throws<InvalidOperationException>(
                () => repository.Transition(item, state => null),
                "Null transition result must fail.");
            FirearmStateRepositorySnapshot after = repository.GetOrCreate(item);
            Assertions.Equal(before.State, after.State, "Null transition result changed state.");
            Assertions.Equal(before.Revision, after.Revision, "Null transition result changed revision.");
        }

        private static void RepositoryTryGetMissingDoesNotCreate()
        {
            var repository = new WeakFirearmStateRepository();
            FirearmStateRepositorySnapshot snapshot;
            Assertions.False(repository.TryGet(new object(), out snapshot), "Missing item unexpectedly resolved.");
            Assertions.Equal(null, snapshot, "Missing snapshot must be null.");
            Assertions.Equal(0L, repository.CreatedEntryCount, "TryGet must not create.");
        }

        private static void RepositorySnapshotRemainsImmutable()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot before = repository.GetOrCreate(item);
            repository.Set(item, LoadedState(1, LeadBall(), FirearmCondition.Normal));
            Assertions.True(before.State.IsEmpty, "Earlier snapshot must remain empty.");
            Assertions.Equal(0, before.Revision, "Earlier snapshot revision must remain unchanged.");
        }

        private static void RepositoryRemoveExisting()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            repository.GetOrCreate(item);
            Assertions.True(repository.Remove(item), "Existing entry should be removed.");
            FirearmStateRepositorySnapshot snapshot;
            Assertions.False(repository.TryGet(item, out snapshot), "Removed entry should not resolve.");
            Assertions.Equal(1L, repository.RemovalCount, "Removal counter mismatch.");
        }

        private static void RepositoryRemoveMissing()
        {
            var repository = new WeakFirearmStateRepository();
            Assertions.False(repository.Remove(new object()), "Missing entry should not report removal.");
            Assertions.Equal(0L, repository.RemovalCount, "Missing removal must not increment counter.");
        }

        private static void RepositoryRemoveAndReaddCreatesNewEntry()
        {
            var repository = new WeakFirearmStateRepository();
            object item = new object();
            FirearmStateRepositorySnapshot first = repository.Set(
                item,
                LoadedState(1, LeadBall(), FirearmCondition.Normal));
            repository.Remove(item);
            FirearmStateRepositorySnapshot second = repository.GetOrCreate(item);
            Assertions.True(first.EntryId != second.EntryId, "Re-added item must receive a new entry ID.");
            Assertions.Equal(FirearmState.CreateEmpty(), second.State, "Re-added item must return to canonical empty state.");
        }

        private static void RepositoryNullKeyRejected()
        {
            var repository = new WeakFirearmStateRepository();
            Assertions.Throws<ArgumentNullException>(
                () => repository.GetOrCreate(null),
                "Null repository key must fail.");
        }

        private static void RepositoryValueKeyRejected()
        {
            var repository = new WeakFirearmStateRepository();
            Assertions.Throws<ArgumentException>(
                () => repository.GetOrCreate(17),
                "Boxed value repository key must fail.");
        }

        private static void RepositoryNullStateRejected()
        {
            var repository = new WeakFirearmStateRepository();
            Assertions.Throws<ArgumentNullException>(
                () => repository.Set(new object(), null),
                "Null repository state must fail.");
        }

        private static void RepositoryNullTransitionRejected()
        {
            var repository = new WeakFirearmStateRepository();
            Assertions.Throws<ArgumentNullException>(
                () => repository.Transition(new object(), null),
                "Null repository transition must fail.");
        }

        private static void RepositoryCountersAreDeterministic()
        {
            var repository = new WeakFirearmStateRepository();
            object first = new object();
            object second = new object();
            repository.GetOrCreate(first);
            repository.GetOrCreate(second);
            repository.Set(first, LoadedState(1, LeadBall(), FirearmCondition.Normal));
            repository.Set(first, LoadedState(1, LeadBall(), FirearmCondition.Normal));
            repository.Remove(second);
            Assertions.Equal(2L, repository.CreatedEntryCount, "Created-entry count mismatch.");
            Assertions.Equal(1L, repository.MutationCount, "Mutation count mismatch.");
            Assertions.Equal(1L, repository.RemovalCount, "Removal count mismatch.");
        }

        private static void ItemStateExactFirearmInitializesEmpty()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "musket-a");
            FirearmItemStateSnapshot snapshot = service.GetOrCreate(item);
            Assertions.True(snapshot.Repository.State.IsEmpty, "Exact firearm must initialize empty.");
            Assertions.Equal(1L, repository.CreatedEntryCount, "Exact firearm should create one entry.");
        }

        private static void ItemStateNativeWeaponRejectedWithoutEntry()
        {
            AssertRejectedCandidateDoesNotCreate("native Heavy Crossbow");
        }

        private static void ItemStateAmbiguousWeaponRejectedWithoutEntry()
        {
            AssertRejectedCandidateDoesNotCreate("ambiguous firearm marker count=2");
        }

        private static void ItemStateBlueprintRejectedWithoutEntry()
        {
            AssertRejectedCandidateDoesNotCreate("blueprint is not a runtime item");
        }

        private static void ItemStateUsesCanonicalItemKey()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object wrapper = new object();
            object direct = new object();
            object canonicalItem = new object();
            resolver.Register(wrapper, canonicalItem, "musket-a");
            resolver.Register(direct, canonicalItem, "musket-a");
            FirearmItemStateSnapshot first = service.GetOrCreate(wrapper);
            FirearmItemStateSnapshot second = service.GetOrCreate(direct);
            Assertions.Equal(first.Repository.EntryId, second.Repository.EntryId, "Resolver canonical key was ignored.");
            Assertions.Equal(1L, repository.CreatedEntryCount, "Canonical item should create one entry.");
        }

        private static void ItemStateTwoFirearmsRemainIndependent()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object first = new object();
            object second = new object();
            resolver.Register(first, first, "musket-a");
            resolver.Register(second, second, "musket-b");
            service.Set(first, LoadedState(1, LeadBall(), FirearmCondition.Normal));
            service.Set(second, FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty()));
            FirearmItemStateSnapshot firstState = service.GetOrCreate(first);
            FirearmItemStateSnapshot secondState = service.GetOrCreate(second);
            Assertions.True(firstState.Repository.EntryId != secondState.Repository.EntryId, "Distinct firearms shared an entry.");
            Assertions.Equal(1, firstState.Repository.State.LoadedRounds, "First firearm lost load.");
            Assertions.Equal(FirearmCondition.Broken, secondState.Repository.State.Condition, "Second firearm lost damage.");
        }

        private static void ItemStateSetPreservesMetadata()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "named-musket");
            FirearmItemStateSnapshot snapshot = service.Set(
                item,
                LoadedState(1, LeadBall(), FirearmCondition.Normal));
            Assertions.Equal("named-musket", snapshot.ItemDisplayName, "Display name metadata mismatch.");
            Assertions.Equal("item-blueprint-named-musket", snapshot.ItemBlueprintId, "Blueprint metadata mismatch.");
            Assertions.Equal(EarlyMusket(), snapshot.Definition, "Definition metadata mismatch.");
        }

        private static void ItemStateTransitionUsesRepository()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "musket-a");
            FirearmItemStateSnapshot snapshot = service.Transition(
                item,
                FirearmStateMachine.ApplyMisfireDamage);
            Assertions.Equal(FirearmCondition.Broken, snapshot.Repository.State.Condition, "Transition result mismatch.");
            Assertions.Equal(1L, repository.MutationCount, "Service transition did not use repository mutation.");
        }

        private static void ItemStateGetExistingMissingDoesNotCreate()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "musket-a");
            FirearmItemStateSnapshot snapshot;
            string reason;
            Assertions.False(service.TryGetExisting(item, out snapshot, out reason), "Missing existing state unexpectedly resolved.");
            Assertions.Equal(null, snapshot, "Missing existing snapshot must be null.");
            Assertions.True(reason.Contains("no existing firearm-state entry"), "Missing-state reason mismatch.");
            Assertions.Equal(0L, repository.CreatedEntryCount, "TryGetExisting must not create.");
        }

        private static void ItemStateForgetRemovesEntry()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "musket-a");
            long firstId = service.GetOrCreate(item).Repository.EntryId;
            Assertions.True(service.Forget(item), "Existing firearm entry was not forgotten.");
            long secondId = service.GetOrCreate(item).Repository.EntryId;
            Assertions.True(firstId != secondId, "Forgotten firearm reused its old entry ID.");
        }

        private static void ItemStateFormattingIsDeterministic()
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object item = new object();
            resolver.Register(item, item, "musket-a");
            FirearmItemStateSnapshot snapshot = service.GetOrCreate(item);
            string first = snapshot.ToString();
            string second = snapshot.ToString();
            Assertions.Equal(first, second, "Item-state formatting must be deterministic.");
            Assertions.True(first.Contains("kmg-item-000001"), "Repository identity missing from diagnostic format.");
            Assertions.True(first.Contains("rounds=0"), "State missing from diagnostic format.");
        }

        private static void AssertRejectedCandidateDoesNotCreate(string reason)
        {
            WeakFirearmStateRepository repository;
            FakeFirearmRuntimeItemResolver resolver;
            FirearmItemStateService service = CreateItemStateService(out repository, out resolver);
            object candidate = new object();
            resolver.Reject(candidate, reason);
            FirearmItemStateSnapshot snapshot;
            string actualReason;
            Assertions.False(service.TryGetOrCreate(candidate, out snapshot, out actualReason), "Rejected candidate unexpectedly resolved.");
            Assertions.Equal(null, snapshot, "Rejected candidate snapshot must be null.");
            Assertions.Equal(reason, actualReason, "Rejection reason mismatch.");
            Assertions.Equal(0L, repository.CreatedEntryCount, "Rejected candidate created repository state.");
        }

        private static FirearmItemStateService CreateItemStateService(
            out WeakFirearmStateRepository repository,
            out FakeFirearmRuntimeItemResolver resolver)
        {
            repository = new WeakFirearmStateRepository();
            resolver = new FakeFirearmRuntimeItemResolver();
            return new FirearmItemStateService(resolver, repository);
        }

        private static FirearmStateData EmptyStateData()
        {
            return LoadedStateData(0, null, "normal");
        }

        private static FirearmStateData LoadedStateData(
            int rounds,
            string ammunitionId,
            string condition)
        {
            return new FirearmStateData
            {
                SchemaVersion = FirearmState.CurrentSchemaVersion,
                LoadedRounds = rounds,
                LoadedAmmunitionId = ammunitionId,
                Condition = condition
            };
        }

        private static TokenBackedFirearmStateRepository CreateTokenRepository(
            FakeFirearmStateTokenStore store)
        {
            return new TokenBackedFirearmStateRepository(store, TokenCatalog());
        }

        private static void TokenRepositoryUnseenIsEmpty()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            Assertions.Equal(FirearmState.CreateEmpty(), repository.GetOrCreate(new object()).State, "Unseen item was not empty.");
        }

        private static void TokenRepositorySetLoadedWritesToken()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId, store.Single(item), "Loaded token mismatch.");
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(item).State, "Loaded state did not read back.");
        }

        private static void TokenRepositorySetBrokenWritesToken()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenBrokenEmpty());
            Assertions.Equal(FirearmStateTokenCatalog.BrokenEmptyTokenId, store.Single(item), "Broken token mismatch.");
        }

        private static void TokenRepositoryResetClearsToken()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            repository.Set(item, FirearmState.CreateEmpty());
            Assertions.Equal(0, store.ReadTokenIds(item).Count, "Reset left a token on the item.");
        }

        private static void TokenRepositoryTwoItemsRemainIndependent()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object first = new object();
            object second = new object();
            repository.Set(first, TokenLoadedNormal());
            repository.Set(second, TokenBrokenEmpty());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "First item state migrated.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Second item state migrated.");
        }

        private static void TokenRepositoryValueEqualItemsRemainIndependent()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object first = new ValueEqualItem(42);
            object second = new ValueEqualItem(42);
            repository.Set(first, TokenLoadedNormal());
            repository.Set(second, TokenBrokenEmpty());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "Reference identity was not preserved.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Value-equal item shared state.");
        }

        private static void TokenRepositoryRevisionIncrements()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            FirearmStateRepositorySnapshot first = repository.GetOrCreate(item);
            FirearmStateRepositorySnapshot second = repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(first.Revision + 1, second.Revision, "Revision did not increment.");
        }

        private static void TokenRepositoryNoOpDoesNotWrite()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, FirearmState.CreateEmpty());
            Assertions.Equal(0, store.ReplaceCount, "A no-op state assignment wrote a token.");
            Assertions.Equal(0L, repository.MutationCount, "A no-op assignment incremented mutations.");
        }

        private static void TokenRepositoryTransitionCommits()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            FirearmStateRepositorySnapshot snapshot = repository.Transition(item, state => TokenBrokenEmpty());
            Assertions.Equal(TokenBrokenEmpty(), snapshot.State, "Transition did not commit.");
        }

        private static void TokenRepositoryRejectedTransitionPreserves()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.Throws<InvalidOperationException>(
                () => repository.Transition(item, state => { throw new InvalidOperationException("reject"); }),
                "Rejected transition should escape.");
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(item).State, "Rejected transition changed the state.");
        }

        private static void TokenRepositoryNullTransitionPreserves()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.Throws<InvalidOperationException>(
                () => repository.Transition(item, state => null),
                "Null transition must be rejected.");
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(item).State, "Null transition changed the state.");
        }

        private static void TokenRepositoryTryGetEmptyDoesNotCreate()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            FirearmStateRepositorySnapshot snapshot;
            Assertions.False(repository.TryGet(new object(), out snapshot), "Unseen tokenless item unexpectedly existed.");
            Assertions.Equal(0L, repository.CreatedEntryCount, "TryGet created metadata for a tokenless item.");
        }

        private static void TokenRepositoryTryGetPersistedTokenReconstructs()
        {
            var store = new FakeFirearmStateTokenStore();
            object item = new object();
            store.Seed(item, FirearmStateTokenCatalog.WreckedTokenId);
            var repository = CreateTokenRepository(store);
            FirearmStateRepositorySnapshot snapshot;
            Assertions.True(repository.TryGet(item, out snapshot), "Persisted token was not discovered.");
            Assertions.Equal(TokenWrecked(), snapshot.State, "Persisted token decoded incorrectly.");
        }

        private static void TokenRepositoryUnknownTokenFailsClosed()
        {
            var store = new FakeFirearmStateTokenStore();
            object item = new object();
            store.Seed(item, "kmg.state.v99.future");
            var repository = CreateTokenRepository(store);
            Assertions.Throws<NotSupportedException>(
                () => repository.GetOrCreate(item),
                "Unknown token should fail closed.");
        }

        private static void TokenRepositoryDuplicateTokensFailClosed()
        {
            var store = new FakeFirearmStateTokenStore();
            object item = new object();
            store.Seed(
                item,
                FirearmStateTokenCatalog.LoadedNormalTokenId,
                FirearmStateTokenCatalog.BrokenEmptyTokenId);
            var repository = CreateTokenRepository(store);
            Assertions.Throws<InvalidDataException>(
                () => repository.GetOrCreate(item),
                "Duplicate state tokens should fail closed.");
        }

        private static void TokenRepositoryReplaceFailurePreserves()
        {
            var store = new FakeFirearmStateTokenStore { ThrowOnReplace = true };
            var repository = CreateTokenRepository(store);
            object item = new object();
            Assertions.Throws<InvalidOperationException>(
                () => repository.Set(item, TokenLoadedNormal()),
                "Store failure should escape.");
            Assertions.Equal(0, store.ReadTokenIds(item).Count, "Failed replacement mutated the token set.");
        }

        private static void TokenRepositoryConcurrentChangeFailsClosed()
        {
            var store = new FakeFirearmStateTokenStore();
            object item = new object();
            var repository = CreateTokenRepository(store);
            store.BeforeReplace = candidate => store.Seed(candidate, FirearmStateTokenCatalog.BrokenEmptyTokenId);
            Assertions.Throws<InvalidOperationException>(
                () => repository.Set(item, TokenLoadedNormal()),
                "An unexpected concurrent token change must fail closed.");
            Assertions.Equal(FirearmStateTokenCatalog.BrokenEmptyTokenId, store.Single(item), "Concurrent state was overwritten.");
        }

        private static void TokenRepositoryCorruptWriteDetected()
        {
            var store = new FakeFirearmStateTokenStore { CorruptWrites = true };
            var repository = CreateTokenRepository(store);
            object item = new object();
            Assertions.Throws<NotSupportedException>(
                () => repository.Set(item, TokenLoadedNormal()),
                "A corrupt store write must fail verification.");
        }

        private static void TokenRepositoryUnsupportedStatePreserves()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            FirearmState unsupported = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                new AmmunitionId("kmg.ammunition.other"),
                FirearmCondition.Normal);
            Assertions.Throws<NotSupportedException>(
                () => repository.Set(item, unsupported),
                "Unrepresentable state must be rejected.");
            Assertions.Equal(0, store.ReadTokenIds(item).Count, "Unsupported state mutated the token set.");
        }

        private static void TokenRepositoryRemoveClearsToken()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.True(repository.Remove(item), "Removing a stateful item should report a change.");
            Assertions.Equal(0, store.ReadTokenIds(item).Count, "Remove did not clear the item token.");
        }

        private static void TokenRepositoryRemoveMissingReturnsFalse()
        {
            var repository = CreateTokenRepository(new FakeFirearmStateTokenStore());
            Assertions.False(repository.Remove(new object()), "Removing an unseen tokenless item should return false.");
        }

        private static void TokenRepositoryRemoveMetadataReturnsTrue()
        {
            var repository = CreateTokenRepository(new FakeFirearmStateTokenStore());
            object item = new object();
            repository.GetOrCreate(item);
            Assertions.True(repository.Remove(item), "Removing existing metadata should report a change.");
        }

        private static void TokenRepositorySnapshotRemainsImmutable()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            FirearmStateRepositorySnapshot before = repository.GetOrCreate(item);
            repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(FirearmState.CreateEmpty(), before.State, "Earlier snapshot mutated.");
        }

        private static void TokenRepositoryCounters()
        {
            var store = new FakeFirearmStateTokenStore();
            var repository = CreateTokenRepository(store);
            object item = new object();
            repository.GetOrCreate(item);
            repository.Set(item, TokenLoadedNormal());
            repository.Remove(item);
            Assertions.Equal(1L, repository.CreatedEntryCount, "Created counter mismatch.");
            Assertions.Equal(1L, repository.MutationCount, "Mutation counter mismatch.");
            Assertions.Equal(1L, repository.RemovalCount, "Removal counter mismatch.");
        }

        private static void TokenRepositoryNullKeyRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateTokenRepository(new FakeFirearmStateTokenStore()).GetOrCreate(null),
                "Null item key must be rejected.");
        }

        private static void TokenRepositoryValueKeyRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => CreateTokenRepository(new FakeFirearmStateTokenStore()).GetOrCreate(7),
                "Value-type item key must be rejected.");
        }

        private static void TokenRepositoryNullStateRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateTokenRepository(new FakeFirearmStateTokenStore()).Set(new object(), null),
                "Null state must be rejected.");
        }

        private static void TokenRepositoryNullTransitionRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateTokenRepository(new FakeFirearmStateTokenStore()).Transition(new object(), null),
                "Null transition must be rejected.");
        }

        private static FirearmStateRules DiagnosticStateRules()
        {
            return new FirearmStateRules(
                1,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall });
        }

        private static VaultBackedFirearmStateRepository CreateVaultRepository(
            FakeFirearmStateVaultStore store)
        {
            return new VaultBackedFirearmStateRepository(
                store,
                DiagnosticStateRules());
        }

        private static MigratingFirearmStateRepository CreateMigrationRepository(
            out FakeFirearmStateVaultStore vaultStore,
            out FakeFirearmStateTokenStore tokenStore)
        {
            vaultStore = new FakeFirearmStateVaultStore();
            tokenStore = new FakeFirearmStateTokenStore();
            var vaultRepository = CreateVaultRepository(vaultStore);
            return new MigratingFirearmStateRepository(
                vaultRepository,
                vaultStore,
                tokenStore,
                TokenCatalog());
        }

        private static void VaultDataCloneNull()
        {
            Assertions.Equal(
                null,
                FirearmStateDataUtility.Clone(null),
                "A null vault DTO should clone to null.");
        }

        private static void VaultDataCloneIsIndependent()
        {
            FirearmStateData original = LoadedStateData(1, FirearmStateTokenCatalog.DiagnosticLeadBall.Value, "normal");
            FirearmStateData clone = FirearmStateDataUtility.Clone(original);
            clone.Condition = "broken";
            Assertions.Equal("normal", original.Condition, "Mutating a clone changed the original DTO.");
            Assertions.False(ReferenceEquals(original, clone), "DTO clone reused the original reference.");
        }

        private static void VaultDataEquality()
        {
            FirearmStateData left = LoadedStateData(1, FirearmStateTokenCatalog.DiagnosticLeadBall.Value, "broken");
            FirearmStateData right = LoadedStateData(1, FirearmStateTokenCatalog.DiagnosticLeadBall.Value, "broken");
            Assertions.True(FirearmStateDataUtility.AreEqual(left, right), "Equivalent DTOs were not equal.");
            Assertions.True(FirearmStateDataUtility.AreEqual(null, null), "Two absent DTOs were not equal.");
        }

        private static void VaultDataInequality()
        {
            FirearmStateData left = LoadedStateData(1, FirearmStateTokenCatalog.DiagnosticLeadBall.Value, "normal");
            FirearmStateData right = LoadedStateData(0, null, "broken");
            Assertions.False(FirearmStateDataUtility.AreEqual(left, right), "Different DTOs compared equal.");
            Assertions.False(FirearmStateDataUtility.AreEqual(left, null), "A present DTO compared equal to absence.");
        }

        private static void VaultDataDescription()
        {
            string description = FirearmStateDataUtility.Describe(
                LoadedStateData(1, FirearmStateTokenCatalog.DiagnosticLeadBall.Value, "normal"));
            Assertions.True(description.Contains("rounds=1"), "DTO description omitted rounds.");
            Assertions.True(description.Contains("condition=normal"), "DTO description omitted condition.");
            Assertions.Equal("<absent>", FirearmStateDataUtility.Describe(null), "Absent DTO description mismatch.");
        }

        private static void VaultRepositoryUnseenIsEmpty()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            Assertions.Equal(FirearmState.CreateEmpty(), repository.GetOrCreate(new object()).State, "Unseen vault item was not empty.");
        }

        private static void VaultRepositorySetLoadedWritesRecord()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            FirearmStateData data;
            Assertions.True(store.TryRead(item, out data), "Loaded state did not create a vault record.");
            Assertions.Equal(TokenLoadedNormal(), FirearmStateCodec.FromData(data, DiagnosticStateRules()), "Vault state mismatch.");
        }

        private static void VaultRepositoryPersistedRecordReconstructs()
        {
            var store = new FakeFirearmStateVaultStore();
            object item = new object();
            store.Seed(item, FirearmStateCodec.ToData(TokenBrokenLoaded()));
            var repository = CreateVaultRepository(store);
            Assertions.Equal(TokenBrokenLoaded(), repository.GetOrCreate(item).State, "A persisted vault record did not reconstruct.");
        }

        private static void VaultRepositoryResetClearsRecord()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            repository.Set(item, FirearmState.CreateEmpty());
            FirearmStateData ignored;
            Assertions.False(store.TryRead(item, out ignored), "Canonical empty state left a persisted vault record.");
        }

        private static void VaultRepositoryTwoItemsRemainIndependent()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object first = new object();
            object second = new object();
            repository.Set(first, TokenLoadedNormal());
            repository.Set(second, TokenBrokenEmpty());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "First vault item changed.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Second vault item changed.");
        }

        private static void VaultRepositoryValueEqualItemsRemainIndependent()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object first = new ValueEqualItem(5);
            object second = new ValueEqualItem(5);
            repository.Set(first, TokenLoadedNormal());
            repository.Set(second, TokenBrokenEmpty());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "Value-equal first item collapsed.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Value-equal second item collapsed.");
        }

        private static void VaultRepositoryRevisionIncrements()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            FirearmStateRepositorySnapshot first = repository.GetOrCreate(item);
            FirearmStateRepositorySnapshot second = repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(first.Revision + 1, second.Revision, "Vault revision did not increment.");
        }

        private static void VaultRepositoryNoOpDoesNotWrite()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object item = new object();
            FirearmStateRepositorySnapshot first = repository.Set(item, TokenLoadedNormal());
            int writes = store.ReplaceCount;
            FirearmStateRepositorySnapshot second = repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(writes, store.ReplaceCount, "No-op state wrote the vault.");
            Assertions.Equal(first.Revision, second.Revision, "No-op state advanced revision.");
        }

        private static void VaultRepositoryTransitionCommits()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            FirearmStateRepositorySnapshot snapshot = repository.Transition(
                item,
                FirearmStateMachine.ApplyMisfireDamage);
            Assertions.Equal(TokenBrokenLoaded(), snapshot.State, "Vault transition did not commit.");
        }

        private static void VaultRepositoryRejectedTransitionPreserves()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.Throws<FirearmStateTransitionException>(
                () => repository.Transition(item, FirearmStateMachine.Repair),
                "Repairing a normal firearm should be rejected.");
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(item).State, "Rejected transition changed vault state.");
        }

        private static void VaultRepositoryNullTransitionPreserves()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.Throws<InvalidOperationException>(
                () => repository.Transition(item, state => null),
                "Null transition result was accepted.");
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(item).State, "Null transition changed state.");
        }

        private static void VaultRepositoryTryGetEmptyDoesNotCreate()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            FirearmStateRepositorySnapshot snapshot;
            Assertions.False(repository.TryGet(new object(), out snapshot), "Missing vault item unexpectedly existed.");
            Assertions.Equal(0L, repository.CreatedEntryCount, "TryGet created metadata for a missing item.");
        }

        private static void VaultRepositoryCorruptReadFailsClosed()
        {
            var store = new FakeFirearmStateVaultStore();
            object item = new object();
            FirearmStateData corrupt = LoadedStateData(0, null, "normal");
            corrupt.SchemaVersion = 99;
            store.Seed(item, corrupt);
            var repository = CreateVaultRepository(store);
            Assertions.Throws<NotSupportedException>(
                () => repository.GetOrCreate(item),
                "Unsupported persisted schema did not fail closed.");
        }

        private static void VaultRepositoryReplaceFailurePreserves()
        {
            var store = new FakeFirearmStateVaultStore { ThrowOnReplace = true };
            var repository = CreateVaultRepository(store);
            object item = new object();
            Assertions.Throws<InvalidOperationException>(
                () => repository.Set(item, TokenLoadedNormal()),
                "Synthetic vault replacement failure did not escape.");
            FirearmStateData ignored;
            Assertions.False(store.TryRead(item, out ignored), "Failed replacement changed the vault.");
        }

        private static void VaultRepositoryConcurrentChangeFailsClosed()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object item = new object();
            store.BeforeReplace = candidate => store.Seed(candidate, FirearmStateCodec.ToData(TokenBrokenEmpty()));
            Assertions.Throws<InvalidOperationException>(
                () => repository.Set(item, TokenLoadedNormal()),
                "Concurrent vault change was overwritten.");
            FirearmStateData data;
            Assertions.True(store.TryRead(item, out data), "Concurrent state disappeared.");
            Assertions.Equal(TokenBrokenEmpty(), FirearmStateCodec.FromData(data, DiagnosticStateRules()), "Concurrent state was changed.");
        }

        private static void VaultRepositoryCorruptWriteDetected()
        {
            var store = new FakeFirearmStateVaultStore { CorruptWrites = true };
            var repository = CreateVaultRepository(store);
            Assertions.Throws<InvalidOperationException>(
                () => repository.Set(new object(), TokenLoadedNormal()),
                "A corrupt vault write was not detected.");
        }

        private static void VaultRepositoryRemoveClearsRecord()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = CreateVaultRepository(store);
            object item = new object();
            repository.Set(item, TokenLoadedNormal());
            Assertions.True(repository.Remove(item), "Existing vault state was not removed.");
            FirearmStateData ignored;
            Assertions.False(store.TryRead(item, out ignored), "Vault record remained after removal.");
        }

        private static void VaultRepositoryRemoveMissingReturnsFalse()
        {
            Assertions.False(
                CreateVaultRepository(new FakeFirearmStateVaultStore()).Remove(new object()),
                "Removing a missing vault item returned true.");
        }

        private static void VaultRepositoryRemoveMetadataReturnsTrue()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            repository.GetOrCreate(item);
            Assertions.True(repository.Remove(item), "Process-local vault metadata was not removed.");
        }

        private static void VaultRepositorySnapshotRemainsImmutable()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            FirearmStateRepositorySnapshot before = repository.GetOrCreate(item);
            repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(FirearmState.CreateEmpty(), before.State, "Earlier vault snapshot changed after mutation.");
        }

        private static void VaultRepositoryCounters()
        {
            var repository = CreateVaultRepository(new FakeFirearmStateVaultStore());
            object item = new object();
            repository.GetOrCreate(item);
            repository.Set(item, TokenLoadedNormal());
            repository.Remove(item);
            Assertions.Equal(1L, repository.CreatedEntryCount, "Vault created counter mismatch.");
            Assertions.Equal(1L, repository.MutationCount, "Vault mutation counter mismatch.");
            Assertions.Equal(1L, repository.RemovalCount, "Vault removal counter mismatch.");
        }

        private static void VaultRepositoryNullKeyRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateVaultRepository(new FakeFirearmStateVaultStore()).GetOrCreate(null),
                "Null vault key was accepted.");
        }

        private static void VaultRepositoryValueKeyRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => CreateVaultRepository(new FakeFirearmStateVaultStore()).GetOrCreate(7),
                "Value-type vault key was accepted.");
        }

        private static void VaultRepositoryNullStateRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateVaultRepository(new FakeFirearmStateVaultStore()).Set(new object(), null),
                "Null vault state was accepted.");
        }

        private static void VaultRepositoryNullTransitionRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => CreateVaultRepository(new FakeFirearmStateVaultStore()).Transition(new object(), null),
                "Null vault transition was accepted.");
        }

        private static void AssertLegacyMigration(FirearmState state, string tokenId)
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, tokenId);
            FirearmStateRepositorySnapshot snapshot = repository.GetOrCreate(item);
            Assertions.Equal(state, snapshot.State, "Migrated state mismatch.");
            Assertions.Equal(null, tokenStore.Single(item), "Legacy token remained after migration.");
            FirearmStateData data;
            Assertions.True(vaultStore.TryRead(item, out data), "Migration created no vault record.");
            Assertions.Equal(state, FirearmStateCodec.FromData(data, DiagnosticStateRules()), "Persisted migrated state mismatch.");
            Assertions.Equal(1L, repository.MigrationSnapshot.MigratedItemCount, "Migration counter mismatch.");
        }

        private static void MigrationNoTokenDelegates()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            Assertions.Equal(FirearmState.CreateEmpty(), repository.GetOrCreate(new object()).State, "No-token item did not delegate to empty vault state.");
            Assertions.Equal(0L, repository.MigrationSnapshot.ObservedLegacyTokenCount, "No-token read incremented migration observations.");
        }

        private static void MigrationLoadedNormal()
        {
            AssertLegacyMigration(TokenLoadedNormal(), FirearmStateTokenCatalog.LoadedNormalTokenId);
        }

        private static void MigrationBrokenEmpty()
        {
            AssertLegacyMigration(TokenBrokenEmpty(), FirearmStateTokenCatalog.BrokenEmptyTokenId);
        }

        private static void MigrationBrokenLoaded()
        {
            AssertLegacyMigration(TokenBrokenLoaded(), FirearmStateTokenCatalog.BrokenLoadedTokenId);
        }

        private static void MigrationWrecked()
        {
            AssertLegacyMigration(TokenWrecked(), FirearmStateTokenCatalog.WreckedTokenId);
        }

        private static void MigrationSameStateCleansRedundantToken()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            vaultStore.Seed(item, FirearmStateCodec.ToData(TokenBrokenLoaded()));
            tokenStore.Seed(item, FirearmStateTokenCatalog.BrokenLoadedTokenId);
            Assertions.Equal(TokenBrokenLoaded(), repository.GetOrCreate(item).State, "Same-state cleanup changed state.");
            Assertions.Equal(null, tokenStore.Single(item), "Redundant legacy token remained.");
            Assertions.Equal(1L, repository.MigrationSnapshot.RedundantTokenCleanupCount, "Redundant cleanup counter mismatch.");
            Assertions.Equal(0L, repository.MigrationSnapshot.MigratedItemCount, "Same-state cleanup counted as migration.");
        }

        private static void MigrationConflictPreservesBoth()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            vaultStore.Seed(item, FirearmStateCodec.ToData(TokenBrokenEmpty()));
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            Assertions.Throws<InvalidOperationException>(
                () => repository.GetOrCreate(item),
                "Conflicting carriers did not fail closed.");
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId, tokenStore.Single(item), "Conflict removed legacy evidence.");
            FirearmStateData data;
            Assertions.True(vaultStore.TryRead(item, out data), "Conflict removed vault evidence.");
            Assertions.Equal(TokenBrokenEmpty(), FirearmStateCodec.FromData(data, DiagnosticStateRules()), "Conflict changed vault evidence.");
            Assertions.Equal(1L, repository.MigrationSnapshot.ConflictCount, "Conflict counter mismatch.");
        }

        private static void MigrationUnknownTokenPreservesEvidence()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, "kmg.state.v99.future");
            Assertions.Throws<NotSupportedException>(
                () => repository.GetOrCreate(item),
                "Unknown legacy token was accepted.");
            Assertions.Equal("kmg.state.v99.future", tokenStore.Single(item), "Unknown token evidence was removed.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Unknown token created a vault record.");
        }

        private static void MigrationDuplicateTokenPreservesEvidence()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(
                item,
                FirearmStateTokenCatalog.LoadedNormalTokenId,
                FirearmStateTokenCatalog.BrokenEmptyTokenId);
            Assertions.Throws<InvalidDataException>(
                () => repository.GetOrCreate(item),
                "Duplicate legacy tokens were accepted.");
            Assertions.Equal(2, tokenStore.ReadTokenIds(item).Count, "Duplicate token evidence was changed.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Duplicate tokens created a vault record.");
        }

        private static void MigrationVaultWriteFailurePreservesToken()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            vaultStore.ThrowOnReplace = true;
            Assertions.Throws<InvalidOperationException>(
                () => repository.GetOrCreate(item),
                "Migration vault-write failure did not escape.");
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId, tokenStore.Single(item), "Vault failure removed legacy token.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Failed vault migration left a record.");
        }

        private static void MigrationTokenClearFailureRollsBackVault()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            tokenStore.ThrowOnClear = true;
            Assertions.Throws<InvalidOperationException>(
                () => repository.GetOrCreate(item),
                "Legacy token-clear failure did not escape.");
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId, tokenStore.Single(item), "Failed cleanup lost legacy token.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Failed cleanup did not roll back vault write.");
            Assertions.Equal(0L, repository.MigrationSnapshot.RollbackFailureCount, "Successful rollback counted as failure.");
        }

        private static void MigrationRollbackFailureIsCounted()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            tokenStore.ThrowOnClear = true;
            vaultStore.ThrowOnRemove = true;
            Assertions.Throws<InvalidOperationException>(
                () => repository.GetOrCreate(item),
                "Synthetic migration rollback failure did not escape.");
            Assertions.Equal(1L, repository.MigrationSnapshot.RollbackFailureCount, "Rollback failure was not counted.");
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId, tokenStore.Single(item), "Rollback failure lost token evidence.");
            FirearmStateData data;
            Assertions.True(vaultStore.TryRead(item, out data), "Rollback failure unexpectedly removed vault evidence.");
        }

        private static void MigrationSetAfterMigration()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.BrokenEmptyTokenId);
            FirearmStateRepositorySnapshot snapshot = repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(TokenLoadedNormal(), snapshot.State, "Set after migration failed.");
            Assertions.Equal(null, tokenStore.Single(item), "Set after migration left token.");
        }

        private static void MigrationTransitionAfterMigration()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            FirearmStateRepositorySnapshot snapshot = repository.Transition(
                item,
                FirearmStateMachine.Fire);
            Assertions.Equal(FirearmState.CreateEmpty(), snapshot.State, "Transition after migration failed.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Empty result retained vault record.");
        }

        private static void MigrationTryGetMigrates()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.WreckedTokenId);
            FirearmStateRepositorySnapshot snapshot;
            Assertions.True(repository.TryGet(item, out snapshot), "TryGet did not migrate a legacy token.");
            Assertions.Equal(TokenWrecked(), snapshot.State, "TryGet migration state mismatch.");
        }

        private static void MigrationRemoveMigratesThenDeletes()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.BrokenEmptyTokenId);
            Assertions.True(repository.Remove(item), "Remove did not delete migrated state.");
            Assertions.Equal(null, tokenStore.Single(item), "Remove left legacy token.");
            FirearmStateData ignored;
            Assertions.False(vaultStore.TryRead(item, out ignored), "Remove left vault record.");
        }

        private static void MigrationTwoItemsRemainIndependent()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object first = new object();
            object second = new object();
            tokenStore.Seed(first, FirearmStateTokenCatalog.LoadedNormalTokenId);
            tokenStore.Seed(second, FirearmStateTokenCatalog.BrokenEmptyTokenId);
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "First migration state mismatch.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Second migration state mismatch.");
            Assertions.Equal(2L, repository.MigrationSnapshot.MigratedItemCount, "Independent migration count mismatch.");
        }

        private static void MigrationValueEqualItemsRemainIndependent()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object first = new ValueEqualItem(8);
            object second = new ValueEqualItem(8);
            tokenStore.Seed(first, FirearmStateTokenCatalog.LoadedNormalTokenId);
            tokenStore.Seed(second, FirearmStateTokenCatalog.WreckedTokenId);
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "Value-equal first migration collapsed.");
            Assertions.Equal(TokenWrecked(), repository.GetOrCreate(second).State, "Value-equal second migration collapsed.");
        }

        private static void MigrationRunsOnlyWhileTokenExists()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            object item = new object();
            tokenStore.Seed(item, FirearmStateTokenCatalog.LoadedNormalTokenId);
            repository.GetOrCreate(item);
            repository.GetOrCreate(item);
            Assertions.Equal(1L, repository.MigrationSnapshot.ObservedLegacyTokenCount, "Migration repeated after token cleanup.");
            Assertions.Equal(1L, repository.MigrationSnapshot.MigratedItemCount, "Migration count repeated.");
        }

        private static void MigrationSnapshotFormatting()
        {
            var snapshot = new FirearmStateMigrationSnapshot(1, 2, 3, 4, 5, 6);
            string text = snapshot.ToString();
            Assertions.True(text.Contains("migrated=2"), "Migration snapshot omitted migrated count.");
            Assertions.True(text.Contains("rollbackFailures=6"), "Migration snapshot omitted rollback failures.");
        }

        private static void MigrationNullKeyRejected()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            Assertions.Throws<ArgumentNullException>(
                () => repository.GetOrCreate(null),
                "Null migration key was accepted.");
        }

        private static void MigrationNullStateRejected()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            Assertions.Throws<ArgumentNullException>(
                () => repository.Set(new object(), null),
                "Null migration state was accepted.");
        }

        private static void MigrationNullTransitionRejected()
        {
            FakeFirearmStateVaultStore vaultStore;
            FakeFirearmStateTokenStore tokenStore;
            MigratingFirearmStateRepository repository = CreateMigrationRepository(out vaultStore, out tokenStore);
            Assertions.Throws<ArgumentNullException>(
                () => repository.Transition(new object(), null),
                "Null migration transition was accepted.");
        }

        private const string ItemIdentityAValue = "11111111-1111-4111-8111-111111111111";
        private const string ItemIdentityBValue = "22222222-2222-4222-8222-222222222222";

        private static FirearmItemId ItemIdentityA()
        {
            return new FirearmItemId(ItemIdentityAValue);
        }

        private static FirearmItemId ItemIdentityB()
        {
            return new FirearmItemId(ItemIdentityBValue);
        }

        private static void ItemIdentityValid()
        {
            FirearmItemId identity = ItemIdentityA();
            Assertions.Equal(ItemIdentityAValue, identity.Value, "Canonical identity mismatch.");
            Assertions.Equal(ItemIdentityAValue, identity.ToString(), "Identity formatting mismatch.");
        }

        private static void ItemIdentityUppercaseCanonicalized()
        {
            FirearmItemId identity = new FirearmItemId(ItemIdentityAValue.ToUpperInvariant());
            Assertions.Equal(ItemIdentityAValue, identity.Value, "Uppercase GUID was not canonicalized.");
        }

        private static void ItemIdentityGuidConstructor()
        {
            Guid value = Guid.Parse(ItemIdentityAValue);
            FirearmItemId identity = new FirearmItemId(value);
            Assertions.Equal(value, identity.GuidValue, "GUID constructor changed the value.");
        }

        private static void ItemIdentityValueEquality()
        {
            FirearmItemId left = ItemIdentityA();
            FirearmItemId right = new FirearmItemId(ItemIdentityAValue.ToUpperInvariant());
            Assertions.True(left == right, "Equal item identities did not compare equal.");
            Assertions.Equal(left.GetHashCode(), right.GetHashCode(), "Equal identities have different hashes.");
        }

        private static void ItemIdentityInequality()
        {
            Assertions.True(ItemIdentityA() != ItemIdentityB(), "Different identities compared equal.");
        }

        private static void ItemIdentityOrdinalOrder()
        {
            Assertions.True(ItemIdentityA().CompareTo(ItemIdentityB()) < 0, "Identity ordering is not deterministic.");
            Assertions.True(ItemIdentityA().CompareTo(null) > 0, "A non-null identity should sort after null.");
        }

        private static void ItemIdentityEmptyRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmItemId(Guid.Empty),
                "The empty engine identity must be rejected.");
        }

        private static void ItemIdentityCompactRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmItemId("11111111111141118111111111111111"),
                "A compact GUID must not be accepted as the persistence contract.");
        }

        private static void ItemIdentityBracesRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmItemId("{" + ItemIdentityAValue + "}"),
                "A braced GUID must not be accepted as the persistence contract.");
        }

        private static void ItemIdentityWhitespaceRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new FirearmItemId(" " + ItemIdentityAValue),
                "Whitespace around an identity must be rejected.");
        }

        private static void ItemIdentityNullRejected()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmItemId((string)null),
                "A null identity must be rejected.");
        }

        private static void ItemIdentityTryCreateValid()
        {
            FirearmItemId identity;
            string reason;
            Assertions.True(
                FirearmItemId.TryCreate(ItemIdentityAValue, out identity, out reason),
                "A valid identity should be created.");
            Assertions.Equal(ItemIdentityA(), identity, "TryCreate returned the wrong identity.");
            Assertions.Equal(null, reason, "Successful TryCreate returned a rejection reason.");
        }

        private static void ItemIdentityTryCreateInvalid()
        {
            FirearmItemId identity;
            string reason;
            Assertions.False(
                FirearmItemId.TryCreate("not-an-id", out identity, out reason),
                "An invalid identity should be rejected.");
            Assertions.Equal(null, identity, "Rejected identity was not null.");
            Assertions.True(!string.IsNullOrWhiteSpace(reason), "Rejected identity had no reason.");
        }

        private static void ItemIdentityNullOperators()
        {
            FirearmItemId left = null;
            FirearmItemId right = null;
            Assertions.True(left == right, "Two null identities should compare equal.");
            Assertions.True(ItemIdentityA() != null, "A real identity should not compare equal to null.");
        }

        private static IdentityBackedFirearmStateVaultStore CreateIdentityVault(
            FakeFirearmItemIdentityProvider provider,
            FakeFirearmStateIdentityRecordStore records)
        {
            return new IdentityBackedFirearmStateVaultStore(provider, records);
        }

        private static void IdentityVaultWriteRead()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            FirearmStateData loaded = FirearmStateCodec.ToData(TokenLoadedNormal());
            vault.Replace(item, null, loaded);
            FirearmStateData read;
            Assertions.True(vault.TryRead(item, out read), "Identity record was not readable.");
            Assertions.True(FirearmStateDataUtility.AreEqual(loaded, read), "Identity record payload changed.");
        }

        private static void IdentityVaultReconstructedObjectReadsState()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object original = new object();
            object reconstructed = new object();
            provider.Register(original, ItemIdentityA());
            provider.Register(reconstructed, ItemIdentityA());
            vault.Replace(original, null, FirearmStateCodec.ToData(TokenBrokenLoaded()));
            FirearmStateData read;
            Assertions.True(vault.TryRead(reconstructed, out read), "Reconstructed object did not resolve the same record.");
            Assertions.Equal("broken", read.Condition, "Reconstructed object read the wrong state.");
        }

        private static void IdentityVaultDifferentIdsIndependent()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new object();
            object second = new object();
            provider.Register(first, ItemIdentityA());
            provider.Register(second, ItemIdentityB());
            vault.Replace(first, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            vault.Replace(second, null, FirearmStateCodec.ToData(TokenBrokenEmpty()));
            FirearmStateData firstRead;
            FirearmStateData secondRead;
            vault.TryRead(first, out firstRead);
            vault.TryRead(second, out secondRead);
            Assertions.False(FirearmStateDataUtility.AreEqual(firstRead, secondRead), "Different engine identities shared state.");
        }

        private static void IdentityVaultValueEqualItemsDifferentIds()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new ValueEqualItem(7);
            object second = new ValueEqualItem(7);
            provider.Register(first, ItemIdentityA());
            provider.Register(second, ItemIdentityB());
            vault.Replace(first, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            FirearmStateData ignored;
            Assertions.False(vault.TryRead(second, out ignored), "Value equality aliased different engine identities.");
        }

        private static void IdentityVaultValueEqualItemsSameId()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new ValueEqualItem(7);
            object reconstructed = new ValueEqualItem(7);
            provider.Register(first, ItemIdentityA());
            provider.Register(reconstructed, ItemIdentityA());
            vault.Replace(first, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            FirearmStateData read;
            Assertions.True(vault.TryRead(reconstructed, out read), "Same engine identity did not survive object reconstruction.");
        }

        private static void IdentityVaultResetRemoves()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            FirearmStateData loaded = FirearmStateCodec.ToData(TokenLoadedNormal());
            vault.Replace(item, null, loaded);
            vault.Replace(item, loaded, null);
            FirearmStateData ignored;
            Assertions.False(vault.TryRead(item, out ignored), "Reset did not remove the identity record.");
        }

        private static void IdentityVaultCompareFailurePreserves()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            FirearmStateData loaded = FirearmStateCodec.ToData(TokenLoadedNormal());
            vault.Replace(item, null, loaded);
            Assertions.Throws<InvalidOperationException>(
                () => vault.Replace(item, FirearmStateCodec.ToData(TokenBrokenEmpty()), null),
                "A stale expected value should fail closed.");
            FirearmStateData read;
            vault.TryRead(item, out read);
            Assertions.True(FirearmStateDataUtility.AreEqual(loaded, read), "Compare failure changed persisted state.");
        }

        private static void IdentityVaultRemove()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            vault.Replace(item, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            Assertions.True(vault.Remove(item), "Existing identity record was not removed.");
            Assertions.Equal(0, vault.RecordCount, "Removed identity record still counted.");
        }

        private static void IdentityVaultRemoveMissing()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            Assertions.False(vault.Remove(item), "Removing a missing identity record should return false.");
        }

        private static void IdentityVaultProviderRejectsRead()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var vault = CreateIdentityVault(provider, new FakeFirearmStateIdentityRecordStore());
            object item = new object();
            provider.Reject(item, "synthetic identity rejection");
            FirearmStateData ignored;
            Assertions.Throws<InvalidOperationException>(
                () => vault.TryRead(item, out ignored),
                "Identity rejection must not be treated as an empty state.");
        }

        private static void IdentityVaultProviderRejectsWrite()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var vault = CreateIdentityVault(provider, new FakeFirearmStateIdentityRecordStore());
            object item = new object();
            provider.Reject(item, "synthetic identity rejection");
            Assertions.Throws<InvalidOperationException>(
                () => vault.Replace(item, null, FirearmStateCodec.ToData(TokenLoadedNormal())),
                "A rejected identity must not create a record.");
        }

        private static void IdentityVaultProviderNullIdentity()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var vault = CreateIdentityVault(provider, new FakeFirearmStateIdentityRecordStore());
            object item = new object();
            provider.RegisterNull(item);
            FirearmStateData ignored;
            Assertions.Throws<InvalidOperationException>(
                () => vault.TryRead(item, out ignored),
                "A provider success with null identity must fail closed.");
        }

        private static void IdentityVaultDefensiveRead()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            vault.Replace(item, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            FirearmStateData first;
            vault.TryRead(item, out first);
            first.Condition = "wrecked";
            FirearmStateData second;
            vault.TryRead(item, out second);
            Assertions.Equal("normal", second.Condition, "Mutating a read DTO changed stored state.");
        }

        private static void IdentityVaultDefensiveWrite()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            FirearmStateData source = FirearmStateCodec.ToData(TokenLoadedNormal());
            vault.Replace(item, null, source);
            source.Condition = "wrecked";
            FirearmStateData read;
            vault.TryRead(item, out read);
            Assertions.Equal("normal", read.Condition, "Mutating a source DTO changed stored state.");
        }

        private static void IdentityVaultCount()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new object();
            object second = new object();
            provider.Register(first, ItemIdentityA());
            provider.Register(second, ItemIdentityB());
            vault.Replace(first, null, FirearmStateCodec.ToData(TokenLoadedNormal()));
            vault.Replace(second, null, FirearmStateCodec.ToData(TokenBrokenEmpty()));
            Assertions.Equal(2, vault.RecordCount, "Identity record count mismatch.");
        }

        private static void IdentityRepositoryReconstructs()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object original = new object();
            object reconstructed = new object();
            provider.Register(original, ItemIdentityA());
            provider.Register(reconstructed, ItemIdentityA());
            var firstRepository = new VaultBackedFirearmStateRepository(vault, DiagnosticStateRules());
            firstRepository.Set(original, TokenBrokenLoaded());
            var afterRestartRepository = new VaultBackedFirearmStateRepository(vault, DiagnosticStateRules());
            FirearmStateRepositorySnapshot snapshot;
            Assertions.True(afterRestartRepository.TryGet(reconstructed, out snapshot), "Reconstructed item did not restore persisted state.");
            Assertions.Equal(TokenBrokenLoaded(), snapshot.State, "Reconstructed repository state mismatch.");
        }

        private static void IdentityRepositoryTwoIndependent()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new object();
            object second = new object();
            provider.Register(first, ItemIdentityA());
            provider.Register(second, ItemIdentityB());
            var repository = new VaultBackedFirearmStateRepository(vault, DiagnosticStateRules());
            repository.Set(first, TokenLoadedNormal());
            repository.Set(second, TokenBrokenEmpty());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(first).State, "First identity changed.");
            Assertions.Equal(TokenBrokenEmpty(), repository.GetOrCreate(second).State, "Second identity changed.");
        }

        private static void IdentityRepositorySameIdAcrossObjects()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object first = new object();
            object reconstructed = new object();
            provider.Register(first, ItemIdentityA());
            provider.Register(reconstructed, ItemIdentityA());
            var repository = new VaultBackedFirearmStateRepository(vault, DiagnosticStateRules());
            repository.Set(first, TokenLoadedNormal());
            Assertions.Equal(TokenLoadedNormal(), repository.GetOrCreate(reconstructed).State, "Objects with the same identity did not share durable state.");
        }

        private static void IdentityRepositoryNoOp()
        {
            var provider = new FakeFirearmItemIdentityProvider();
            var records = new FakeFirearmStateIdentityRecordStore();
            var vault = CreateIdentityVault(provider, records);
            object item = new object();
            provider.Register(item, ItemIdentityA());
            var repository = new VaultBackedFirearmStateRepository(vault, DiagnosticStateRules());
            repository.Set(item, TokenLoadedNormal());
            int writes = records.ReplaceCount;
            repository.Set(item, TokenLoadedNormal());
            Assertions.Equal(writes, records.ReplaceCount, "No-op assignment wrote the identity vault again.");
        }

        private static void IdentityMigrationSnapshotFormatting()
        {
            var snapshot = new FirearmStateIdentityMigrationSnapshot(7, 3, 1, 2, 1, 4, 1);
            string text = snapshot.ToString();
            Assertions.True(text.Contains("legacyObserved=7"), "Migration snapshot omitted observed count.");
            Assertions.True(text.Contains("unresolvedPreserved=2"), "Migration snapshot omitted unresolved count.");
            Assertions.True(text.Contains("rollbackFailures=1"), "Migration snapshot omitted rollback count.");
        }

        private static void IdentityMigrationSnapshotProperties()
        {
            var snapshot = new FirearmStateIdentityMigrationSnapshot(7, 3, 1, 2, 1, 4, 1);
            Assertions.Equal(7L, snapshot.ObservedLegacyRecords, "Observed count mismatch.");
            Assertions.Equal(3L, snapshot.MigratedRecords, "Migrated count mismatch.");
            Assertions.Equal(1L, snapshot.RedundantRecordsRemoved, "Redundant count mismatch.");
            Assertions.Equal(2L, snapshot.UnresolvedRecordsPreserved, "Unresolved count mismatch.");
            Assertions.Equal(1L, snapshot.Conflicts, "Conflict count mismatch.");
            Assertions.Equal(4L, snapshot.Failures, "Failure count mismatch.");
            Assertions.Equal(1L, snapshot.RollbackFailures, "Rollback count mismatch.");
        }

        private static void ReflectionReadsPrivateField()
        {
            var source = new ReflectionDerived();
            object value;
            Assertions.True(
                ReflectionAccess.TryGetMember(source, "_hidden", out value),
                "Private inherited field was not resolved.");
            Assertions.Equal("secret", value as string, "Private field value mismatch.");
        }

        private static void ReflectionReadsStaticProperty()
        {
            object value;
            Assertions.True(
                ReflectionAccess.TryGetMember(typeof(ReflectionStatic), "Instance", out value),
                "Static property was not resolved through a Type target.");
            Assertions.Equal("singleton", value as string, "Static property value mismatch.");
        }

        private static void ReflectionFindsFirstNonNullMember()
        {
            var source = new ReflectionNullableMembers();
            object value;
            string member;
            Assertions.True(
                ReflectionAccess.TryGetFirstNonNullMember(
                    source,
                    new[] { "First", "Second" },
                    out value,
                    out member),
                "The first non-null member was not resolved.");
            Assertions.Equal("Second", member, "Wrong non-null member was selected.");
            Assertions.Equal("ready", value as string, "Non-null member value mismatch.");
        }

        private static void ReflectionReadsPath()
        {
            var source = new ReflectionRoot();
            object value;
            Assertions.True(
                ReflectionAccess.TryGetPath(source, "Child.Value", out value),
                "Nested member path was not resolved.");
            Assertions.Equal(17, (int)value, "Nested member value mismatch.");
        }

        private static void ReflectionEnumeratesCollection()
        {
            int count = 0;
            int sum = 0;
            foreach (object value in ReflectionAccess.Enumerate(new[] { 2, 3, 5 }))
            {
                count++;
                sum += (int)value;
            }

            Assertions.Equal(3, count, "Reflection enumeration count mismatch.");
            Assertions.Equal(10, sum, "Reflection enumeration value mismatch.");
        }

        private static void ReflectionInvokesExactOverload()
        {
            var target = new ReflectionMethods();
            object result;
            string method;
            Assertions.True(
                ReflectionAccess.TryInvokeAny(
                    target,
                    new[] { "Apply" },
                    new[] { new object[] { "musket", 2 } },
                    out result,
                    out method),
                "Exact compatible overload was not invoked.");
            Assertions.Equal("musket:2", result as string, "Exact overload result mismatch.");
            Assertions.True(method.EndsWith(".Apply", StringComparison.Ordinal), "Resolved method name mismatch.");
        }

        private static void ReflectionSuppliesOptionalDefault()
        {
            var target = new ReflectionMethods();
            object result;
            string method;
            Assertions.True(
                ReflectionAccess.TryInvokeAny(
                    target,
                    new[] { "Optional" },
                    new[] { new object[] { "powder" } },
                    out result,
                    out method),
                "Method with an optional trailing value was not invoked.");
            Assertions.Equal("powder:4", result as string, "Optional default was not supplied.");
        }

        private static void ReflectionSuppliesTrailingNull()
        {
            var target = new ReflectionMethods();
            object result;
            string method;
            Assertions.True(
                ReflectionAccess.TryInvokeAny(
                    target,
                    new[] { "Contextual" },
                    new[] { new object[] { "shot" } },
                    out result,
                    out method),
                "Method with a nullable trailing context was not invoked.");
            Assertions.Equal("shot:none", result as string, "Trailing null was not supplied.");
        }

        private static void ReflectionInvokesRequiredBoolean()
        {
            var target = new ReflectionMethods();
            object result;
            string method;
            Assertions.True(
                ReflectionAccess.TryInvokeAny(
                    target,
                    new[] { "RequiredFlag" },
                    new[] { new object[] { "musket", false } },
                    out result,
                    out method),
                "Method with a required Boolean argument was not invoked.");
            Assertions.Equal("musket:False", result as string, "Required Boolean result mismatch.");
        }

        private static void ReflectionRejectsIncompatibleMethod()
        {
            var target = new ReflectionMethods();
            object result;
            string method;
            Assertions.False(
                ReflectionAccess.TryInvokeAny(
                    target,
                    new[] { "Apply" },
                    new[] { new object[] { new object(), "wrong" } },
                    out result,
                    out method),
                "An incompatible overload must not be invoked.");
            Assertions.Equal(0, target.InvocationCount, "Incompatible method changed target state.");
        }

        private static void RangeZeroIsFirstIncrement()
        {
            Assertions.Equal(1, FirearmRangeMath.CalculateIncrement(0d, 12d), "Zero distance must be first increment.");
        }

        private static void RangeWithinFirstIncrement()
        {
            Assertions.Equal(1, FirearmRangeMath.CalculateIncrement(11.9d, 12d), "Close distance must be first increment.");
        }

        private static void RangeExactBoundaryIsFirstIncrement()
        {
            Assertions.Equal(1, FirearmRangeMath.CalculateIncrement(12d, 12d), "Exact boundary must remain first increment.");
        }

        private static void RangeSecondIncrement()
        {
            Assertions.Equal(2, FirearmRangeMath.CalculateIncrement(12.001d, 12d), "Distance past the boundary must be second increment.");
        }

        private static void RangeMultipleIncrements()
        {
            Assertions.Equal(4, FirearmRangeMath.CalculateIncrement(36.1d, 12d), "Multiple-increment calculation mismatch.");
        }

        private static void RangeRejectsNegativeDistance()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmRangeMath.CalculateIncrement(-0.1d, 12d),
                "Negative distance must fail.");
        }

        private static void RangeRejectsZeroIncrement()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmRangeMath.CalculateIncrement(1d, 0d),
                "Zero increment length must fail.");
        }

        private static void RangeRejectsInfiniteDistance()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmRangeMath.CalculateIncrement(double.PositiveInfinity, 12d),
                "Infinite distance must fail.");
        }

        private static void TraceNativeMarkerIsIgnored()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceDecision decision = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 101, null, false, 0),
                false);
            Assertions.False(decision.Accepted, "Marker-free native weapon must not start a trace.");
            Assertions.Equal(0, correlator.ActiveTraceCount, "Ignored native weapon created active state.");
        }

        private static void TraceAmbiguousMarkerIsIgnored()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceDecision decision = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 102, null, false, 2),
                false);
            Assertions.False(decision.Accepted, "Ambiguous marker count must not start a trace.");
        }

        private static void TraceExactFirearmStarts()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceDecision decision = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 103, null, true, 1),
                false);
            Assertions.True(decision.Accepted, "Exact firearm observation was not accepted.");
            Assertions.True(decision.Created, "Exact firearm observation did not create a trace.");
            Assertions.Equal(1L, decision.TraceId, "Unexpected first trace ID.");
            Assertions.Equal(1, correlator.ActiveTraceCount, "Trace was not retained as active.");
        }

        private static void TraceChildJoinsParent()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceDecision root = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 104, null, true, 1),
                false);
            CombatTraceDecision child = correlator.Observe(
                Observation(CombatTraceStage.AttackRoll, CombatTracePhase.Before, 105, 104, false, -1),
                false);
            Assertions.True(child.Accepted, "Nested attack-roll observation did not join its firearm parent.");
            Assertions.False(child.Created, "Nested observation incorrectly created a second trace.");
            Assertions.Equal(root.TraceId, child.TraceId, "Nested observation joined the wrong trace.");
        }

        private static void TraceDuplicateCallbackIsCounted()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceObservation observation = Observation(
                CombatTraceStage.WeaponAttack,
                CombatTracePhase.Before,
                106,
                null,
                true,
                1);
            CombatTraceDecision first = correlator.Observe(observation, false);
            CombatTraceDecision second = correlator.Observe(observation, false);
            Assertions.Equal(1, first.CallbackOrdinal, "First callback ordinal mismatch.");
            Assertions.Equal(2, second.CallbackOrdinal, "Duplicate callback ordinal mismatch.");
            Assertions.True(second.Record.IsDuplicate, "Duplicate record was not identified.");
        }

        private static void TraceUnrelatedChildIsIgnored()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceDecision child = correlator.Observe(
                Observation(CombatTraceStage.ArmorClass, CombatTracePhase.Before, 107, 999, false, -1),
                false);
            Assertions.False(child.Accepted, "Unrelated AC event must not create a trace.");
        }

        private static void TraceWeaponRootCompletes()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 108, null, true, 1),
                false);
            CombatTraceDecision completed = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.After, 108, null, true, 1),
                true);
            Assertions.True(completed.Completed, "Weapon root did not complete on its after observation.");
            Assertions.Equal(0, correlator.ActiveTraceCount, "Completed trace remained active.");
            Assertions.Equal(2, completed.CompletedTrace.Records.Count, "Completed trace record count mismatch.");
        }

        private static void TraceAttackRollRootCompletes()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            correlator.Observe(
                Observation(CombatTraceStage.AttackRoll, CombatTracePhase.Before, 109, null, true, 1),
                false);
            CombatTraceDecision completed = correlator.Observe(
                Observation(CombatTraceStage.AttackRoll, CombatTracePhase.After, 109, null, true, 1),
                true);
            Assertions.True(completed.Completed, "Standalone firearm attack-roll trace did not complete.");
            Assertions.Equal(CombatTraceStage.AttackRoll, completed.CompletedTrace.RootStage, "Wrong root stage.");
        }

        private static void TraceResetClearsActive()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 110, null, true, 1),
                false);
            Assertions.Equal(1, correlator.Reset(), "Reset did not report the active trace.");
            Assertions.Equal(0, correlator.ActiveTraceCount, "Reset retained active trace state.");
        }

        private static void TraceIdSourceMustBeUnique()
        {
            CombatTraceCorrelator correlator = new CombatTraceCorrelator(() => 1L);
            correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 111, null, true, 1),
                false);
            Assertions.Throws<InvalidOperationException>(
                () => correlator.Observe(
                    Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.Before, 112, null, true, 1),
                    false),
                "Duplicate trace ID must fail closed.");
        }

        private static void ObservationCopiesFields()
        {
            var fields = new Dictionary<string, string> { { "weapon", "musket" } };
            CombatTraceObservation observation = new CombatTraceObservation(
                CombatTraceStage.WeaponAttack,
                CombatTracePhase.Before,
                113,
                null,
                true,
                1,
                fields);
            fields["weapon"] = "mutated";
            Assertions.Equal("musket", observation.Fields["weapon"], "Observation retained mutable source dictionary state.");
        }

        private static void ObservationExactRequiresOneMarker()
        {
            Assertions.Throws<ArgumentException>(
                () => new CombatTraceObservation(
                    CombatTraceStage.WeaponAttack,
                    CombatTracePhase.Before,
                    114,
                    null,
                    true,
                    2,
                    null),
                "Exact firearm observation with two markers must fail.");
        }

        private static void FormatterSortsFields()
        {
            var fields = new Dictionary<string, string>
            {
                { "zeta", "last" },
                { "alpha", "first" }
            };
            CombatTraceRecord record = new CombatTraceRecord(
                new CombatTraceObservation(
                    CombatTraceStage.AttackRoll,
                    CombatTracePhase.Before,
                    115,
                    114,
                    false,
                    -1,
                    fields),
                1);
            string formatted = CombatTraceFormatter.FormatRecord(7L, record);
            Assertions.True(
                formatted.IndexOf("; alpha=first", StringComparison.Ordinal) <
                formatted.IndexOf("; zeta=last", StringComparison.Ordinal),
                "Trace fields were not formatted in ordinal order.");
        }

        private static void FormatterSanitizesOneLine()
        {
            var fields = new Dictionary<string, string>
            {
                { "source", "line1;line2\nline3" }
            };
            CombatTraceRecord record = new CombatTraceRecord(
                new CombatTraceObservation(
                    CombatTraceStage.AttackRoll,
                    CombatTracePhase.After,
                    116,
                    null,
                    false,
                    -1,
                    fields),
                1);
            string formatted = CombatTraceFormatter.FormatRecord(8L, record);
            Assertions.False(formatted.Contains("\n"), "Formatted trace contains a newline.");
            Assertions.True(formatted.Contains("source=line1,line2 line3"), "Trace value sanitization mismatch.");
        }

        private static void FormatterCompletionCountsDuplicates()
        {
            CombatTraceCorrelator correlator = NewCorrelator();
            CombatTraceObservation before = Observation(
                CombatTraceStage.WeaponAttack,
                CombatTracePhase.Before,
                117,
                null,
                true,
                1);
            correlator.Observe(before, false);
            correlator.Observe(before, false);
            CombatTraceDecision completed = correlator.Observe(
                Observation(CombatTraceStage.WeaponAttack, CombatTracePhase.After, 117, null, true, 1),
                true);
            string formatted = CombatTraceFormatter.FormatComplete(completed.CompletedTrace);
            Assertions.Equal(1, completed.CompletedTrace.DuplicateCallbackCount, "Duplicate callback count mismatch.");
            Assertions.True(formatted.Contains("duplicateCallbacks=1"), "Completion formatter omitted duplicate count.");
        }


        private static void ArmorClassNativeWeaponUsesOrdinary()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                false,
                0,
                EarlyMusket(),
                1d,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "A marker-free weapon must use ordinary AC.");
            Assertions.Equal("not-exact-firearm", decision.Reason, "Native-weapon reason mismatch.");
        }

        private static void ArmorClassAmbiguousMarkerUsesOrdinary()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                false,
                2,
                EarlyMusket(),
                1d,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "An ambiguous marker must fail closed.");
        }

        private static void ArmorClassMissingDefinitionUsesOrdinary()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                null,
                1d,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "A missing definition must fail closed.");
            Assertions.Equal("missing-firearm-definition", decision.Reason, "Missing-definition reason mismatch.");
        }

        private static void ArmorClassZeroDistanceUsesTouch()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                0d,
                20,
                12,
                20,
                false);
            Assertions.True(decision.UsesTouchArmorClass, "Zero distance must be inside the first range increment.");
            Assertions.Equal(12, decision.SelectedTargetArmorClass, "Zero-distance selected AC mismatch.");
        }

        private static void ArmorClassCloseRangeUsesTouch()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                6d,
                20,
                12,
                20,
                false);
            Assertions.True(decision.UsesTouchArmorClass, "A close early-firearm shot must use touch AC.");
            Assertions.True(decision.ShouldWriteTargetArmorClass, "A different touch AC must be written.");
            Assertions.Equal(12, decision.SelectedTargetArmorClass, "Close-range selected AC mismatch.");
            Assertions.Equal(-8, decision.Adjustment, "Close-range AC delta mismatch.");
            Assertions.Equal(1, decision.RangeIncrement, "Close-range increment mismatch.");
        }

        private static void ArmorClassBoundaryUsesTouch()
        {
            double boundary = 40d * FirearmArmorClassService.MetersPerFoot;
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                boundary,
                18,
                11,
                18,
                false);
            Assertions.True(decision.UsesTouchArmorClass, "The exact first-increment boundary must use touch AC.");
            Assertions.Equal(1, decision.RangeIncrement, "Boundary increment mismatch.");
        }

        private static void ArmorClassBoundaryFloatNoiseUsesTouch()
        {
            double floatNoise =
                (40d * FirearmArmorClassService.MetersPerFoot) + 0.0000005d;
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                floatNoise,
                18,
                11,
                18,
                false);
            Assertions.True(
                decision.UsesTouchArmorClass,
                "Sub-millimeter float noise at the boundary must not move the shot into the second increment.");
            Assertions.Equal(1, decision.RangeIncrement, "Float-noise boundary increment mismatch.");
        }

        private static void ArmorClassDistantRangeUsesOrdinary()
        {
            double outside = (40d * FirearmArmorClassService.MetersPerFoot) + 0.001d;
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                outside,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "A shot beyond the first increment must use ordinary AC.");
            Assertions.Equal(2, decision.RangeIncrement, "Distant increment mismatch.");
            Assertions.Equal(20, decision.SelectedTargetArmorClass, "Distant AC must remain unchanged.");
        }

        private static void ArmorClassDeadeyeDistantUsesTouch()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true, 1, EarlyMusket(),
                (40d * FirearmArmorClassService.MetersPerFoot) + 0.001d,
                20, 12, 20, false, true);
            Assertions.True(decision.UsesTouchArmorClass,
                "Authorized Deadeye did not extend touch AC beyond the first increment.");
            Assertions.Equal(2, decision.RangeIncrement, "Deadeye range increment changed.");
            Assertions.Equal(12, decision.SelectedTargetArmorClass,
                "Deadeye did not select touch AC.");
            Assertions.Equal("touch-ac-deadeye", decision.Reason,
                "Deadeye selection reason changed.");
        }

        private static void ArmorClassDeadeyePreservesContext()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true, 1, EarlyMusket(),
                (80d * FirearmArmorClassService.MetersPerFoot) + 0.001d,
                20, 12, 24, false, true);
            Assertions.Equal(16, decision.SelectedTargetArmorClass,
                "Deadeye did not preserve the contextual AC delta.");
            Assertions.Equal(-8, decision.Adjustment,
                "Deadeye contextual adjustment changed.");
        }

        private static void ArmorClassPreservesCoverAdjustment()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                2d,
                20,
                12,
                24,
                false);
            Assertions.Equal(16, decision.SelectedTargetArmorClass, "The +4 contextual cover adjustment was not preserved.");
            Assertions.Equal(-8, decision.Adjustment, "Cover-preservation delta mismatch.");
        }

        private static void ArmorClassPreservesFlatFootedAdjustment()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                2d,
                20,
                14,
                16,
                false);
            Assertions.Equal(10, decision.SelectedTargetArmorClass, "The contextual flat-footed adjustment was not preserved.");
        }

        private static void ArmorClassEqualValuesRequireNoWrite()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                2d,
                15,
                15,
                19,
                false);
            Assertions.True(decision.UsesTouchArmorClass, "Touch AC should still be selected when values are equal.");
            Assertions.False(decision.ShouldWriteTargetArmorClass, "Equal ordinary/touch AC must not require a write.");
            Assertions.Equal(19, decision.SelectedTargetArmorClass, "Contextual target AC changed unexpectedly.");
        }

        private static void ArmorClassAlreadyAppliedIsSkipped()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                2d,
                20,
                12,
                12,
                true);
            Assertions.False(decision.UsesTouchArmorClass, "A stamped AC event must not be adjusted twice.");
            Assertions.Equal("already-applied", decision.Reason, "Duplicate-application reason mismatch.");
        }

        private static void ArmorClassAdvancedFirearmFailsClosed()
        {
            FirearmDefinition advanced = new FirearmDefinition(
                FirearmEra.Advanced,
                FirearmKind.Pistol,
                1,
                30,
                1,
                5,
                MoveReload(1),
                false);
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                advanced,
                2d,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "Advanced-firearm penetration is outside Sprint 9.");
            Assertions.Equal("advanced-firearm-not-implemented", decision.Reason, "Advanced-firearm reason mismatch.");
        }

        private static void ArmorClassBlunderbussFirstIncrementTouch()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                FirearmDefinitions.CreateEarlyBlunderbuss(),
                2d,
                20,
                12,
                20,
                false);
            Assertions.True(decision.UsesTouchArmorClass,
                "An ordinary Blunderbuss bullet inside its first increment must use touch AC.");
            Assertions.Equal("touch-ac-first-range-increment", decision.Reason,
                "Blunderbuss first-increment reason mismatch.");
        }

        private static void ArmorClassInvalidDistanceFailsClosed()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                double.NaN,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "An invalid distance must retain ordinary AC.");
            Assertions.Equal("invalid-range-input", decision.Reason, "Invalid-distance reason mismatch.");
        }

        private static void ArmorClassNegativeDistanceFailsClosed()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                -0.01d,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "A negative distance must retain ordinary AC.");
            Assertions.Equal("invalid-range-input", decision.Reason, "Negative-distance reason mismatch.");
        }

        private static void ArmorClassInfiniteDistanceFailsClosed()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                double.PositiveInfinity,
                20,
                12,
                20,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "An infinite distance must retain ordinary AC.");
            Assertions.Equal("invalid-range-input", decision.Reason, "Infinite-distance reason mismatch.");
        }

        private static void ArmorClassOverflowFailsClosed()
        {
            FirearmArmorClassDecision decision = SelectArmorClass(
                true,
                1,
                EarlyMusket(),
                2d,
                int.MinValue,
                int.MaxValue,
                0,
                false);
            Assertions.False(decision.UsesTouchArmorClass, "An overflowing AC delta must fail closed.");
            Assertions.Equal("armor-class-overflow", decision.Reason, "Overflow reason mismatch.");
        }

        private static void ArmorClassAccessReadsParticipants()
        {
            FakeUnit initiator = new FakeUnit(20, 12, 5d);
            FakeUnit target = new FakeUnit(18, 11, 5d);
            var rule = new FakeRuleCalculateAcProperty(initiator, target, 18);
            object actualInitiator;
            object actualTarget;
            Assertions.True(
                KingmakerArmorClassAccess.TryReadParticipants(rule, out actualInitiator, out actualTarget),
                "Participants were not resolved.");
            Assertions.True(ReferenceEquals(initiator, actualInitiator), "Initiator mismatch.");
            Assertions.True(ReferenceEquals(target, actualTarget), "Target mismatch.");
        }

        private static void ArmorClassAccessReadsDistance()
        {
            FakeUnit initiator = new FakeUnit(20, 12, 7.25d);
            FakeUnit target = new FakeUnit(18, 11, 0d);
            double distance;
            Assertions.True(
                KingmakerArmorClassAccess.TryReadDistanceMeters(initiator, target, out distance),
                "DistanceTo was not invoked.");
            Assertions.Equal(7.25d, distance, "Distance mismatch.");
        }

        private static void ArmorClassAccessReadsValues()
        {
            FakeUnit target = new FakeUnit(23, 14, 0d);
            int ordinary;
            int touch;
            Assertions.True(
                KingmakerArmorClassAccess.TryReadTargetArmorClasses(target, out ordinary, out touch),
                "Ordinary and touch AC were not resolved.");
            Assertions.Equal(23, ordinary, "Ordinary AC mismatch.");
            Assertions.Equal(14, touch, "Touch AC mismatch.");
        }

        private static void ArmorClassAccessWritesPrivateProperty()
        {
            FakeUnit unit = new FakeUnit(20, 12, 0d);
            var rule = new FakeRuleCalculateAcProperty(unit, unit, 20);
            int before;
            string member;
            Assertions.True(
                KingmakerArmorClassAccess.TryReadTargetArmorClass(rule, out before, out member),
                "Private-set TargetAC property was not readable.");
            Assertions.Equal(20, before, "Initial property AC mismatch.");
            Assertions.True(
                KingmakerArmorClassAccess.TryWriteTargetArmorClass(rule, 12, out member),
                "Private-set TargetAC property was not writable.");
            Assertions.Equal(12, rule.ReadTargetArmorClass(), "Private-set TargetAC property did not change.");
        }

        private static void ArmorClassAccessWritesField()
        {
            FakeUnit unit = new FakeUnit(20, 12, 0d);
            var rule = new FakeRuleCalculateAcField(unit, unit, 20);
            string member;
            Assertions.True(
                KingmakerArmorClassAccess.TryWriteTargetArmorClass(rule, 12, out member),
                "TargetAC field was not writable.");
            Assertions.Equal(12, rule.TargetAC, "TargetAC field did not change.");
        }

        private static void ArmorClassAccessRejectsAmbiguousTargetAc()
        {
            FakeUnit unit = new FakeUnit(20, 12, 0d);
            var rule = new FakeRuleCalculateAcAmbiguous(unit, unit, 20);
            int value;
            string member;
            Assertions.False(
                KingmakerArmorClassAccess.TryReadTargetArmorClass(rule, out value, out member),
                "Two writable TargetAC candidates must be rejected.");
            Assertions.Equal(20, rule.ReadBaseTargetArmorClass(), "Ambiguous base property changed unexpectedly.");
        }

        private static FirearmArmorClassDecision SelectArmorClass(
            bool isExactFirearm,
            int markerCount,
            FirearmDefinition definition,
            double distanceMeters,
            int ordinaryArmorClass,
            int touchArmorClass,
            int currentTargetArmorClass,
            bool alreadyApplied)
        {
            return SelectArmorClass(isExactFirearm, markerCount, definition,
                distanceMeters, ordinaryArmorClass, touchArmorClass,
                currentTargetArmorClass, alreadyApplied, false);
        }

        private static FirearmArmorClassDecision SelectArmorClass(
            bool isExactFirearm,
            int markerCount,
            FirearmDefinition definition,
            double distanceMeters,
            int ordinaryArmorClass,
            int touchArmorClass,
            int currentTargetArmorClass,
            bool alreadyApplied,
            bool deadeyeAuthorized)
        {
            return FirearmArmorClassService.Select(
                new FirearmArmorClassRequest(
                    isExactFirearm,
                    markerCount,
                    definition,
                    distanceMeters,
                    ordinaryArmorClass,
                    touchArmorClass,
                    currentTargetArmorClass,
                    alreadyApplied,
                    deadeyeAuthorized));
        }

        private static CombatTraceCorrelator NewCorrelator()
        {
            long next = 0L;
            return new CombatTraceCorrelator(() => ++next);
        }

        private static CombatTraceObservation Observation(
            CombatTraceStage stage,
            CombatTracePhase phase,
            int eventIdentity,
            int? parentEventIdentity,
            bool exactFirearm,
            int markerCount)
        {
            return new CombatTraceObservation(
                stage,
                phase,
                eventIdentity,
                parentEventIdentity,
                exactFirearm,
                markerCount,
                new Dictionary<string, string>
                {
                    { "weapon", exactFirearm ? "Test Musket" : "<unavailable>" },
                    { "weaponType", exactFirearm ? "KMG_TestMusket_WeaponType" : "<unavailable>" },
                    { "firearmDefinition", exactFirearm ? EarlyMusket().ToString() : "<unavailable>" }
                });
        }

        private static FirearmDefinition NewDefinition(
            FirearmEra era,
            FirearmKind kind,
            int capacity,
            int range,
            int misfire,
            ReloadProfile reload,
            bool scatter,
            int misfireBurstRadiusFeet = 5)
        {
            return new FirearmDefinition(
                era,
                kind,
                capacity,
                range,
                misfire,
                misfireBurstRadiusFeet,
                reload,
                scatter);
        }

        private static FirearmExplosionTargetCandidate ExplosionCandidate(
            object unit,
            string stableIdentity,
            string displayName,
            float distanceMeters,
            bool isExactWielder)
        {
            return new FirearmExplosionTargetCandidate(
                unit,
                stableIdentity,
                displayName,
                distanceMeters,
                isExactWielder);
        }

        private static FirearmExplosionTargetResult NewExplosionTargetResult(
            int naturalRoll,
            float distanceMeters)
        {
            return new FirearmExplosionTargetResult(
                "Ally",
                "unit-1",
                distanceMeters,
                false,
                naturalRoll,
                12,
                false,
                false,
                8,
                8,
                8,
                20,
                12);
        }

        private class ReflectionBase
        {
            private readonly string _hidden = "secret";

            internal string ReadHiddenForCompiler()
            {
                return _hidden;
            }
        }

        private static void EvidenceCatalogCount()
        {
            Assertions.Equal(35, PersistenceMatrixCatalog.All.Count, "The persistence matrix must contain all 35 rows.");
        }

        private static void EvidenceCatalogSeverityCounts()
        {
            Assertions.Equal(
                30,
                PersistenceMatrixCatalog.All.Count(step => step.Severity == PersistenceEvidenceSeverity.Critical),
                "The persistence matrix must contain 30 Critical rows.");
            Assertions.Equal(
                5,
                PersistenceMatrixCatalog.All.Count(step => step.Severity == PersistenceEvidenceSeverity.High),
                "The persistence matrix must contain 5 High rows.");
        }

        private static void EvidenceCatalogOrder()
        {
            for (int index = 0; index < PersistenceMatrixCatalog.All.Count; index++)
            {
                Assertions.Equal(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture, "I{0:D2}", index + 1),
                    PersistenceMatrixCatalog.All[index].Id,
                    "Persistence-matrix rows must retain deterministic numeric order.");
            }
        }

        private static void EvidenceCatalogUniqueIds()
        {
            Assertions.Equal(
                PersistenceMatrixCatalog.All.Count,
                PersistenceMatrixCatalog.All.Select(step => step.Id).Distinct(StringComparer.Ordinal).Count(),
                "Persistence-matrix row IDs must be unique.");
        }

        private static void EvidenceCatalogReproductionIds()
        {
            string[] expected = { "I03", "I10", "I11", "I13", "I15", "I19", "I23" };
            string[] actual = PersistenceMatrixCatalog.All
                .Where(step => step.RequiresReproduction)
                .Select(step => step.Id)
                .ToArray();
            Assertions.Equal(
                string.Join(",", expected),
                string.Join(",", actual),
                "Only the designated lifecycle rows should require independent reproduction.");
        }

        private static void EvidenceCatalogRequireKnown()
        {
            Assertions.Equal("I01", PersistenceMatrixCatalog.Require(" I01 ").Id, "Known row lookup should trim input.");
        }

        private static void EvidenceCatalogRequireUnknown()
        {
            Assertions.Throws<KeyNotFoundException>(
                () => PersistenceMatrixCatalog.Require("I99"),
                "Unknown persistence-matrix rows must fail closed.");
        }

        private static void EvidenceObservationValid()
        {
            PersistenceEvidenceObservation observation = Evidence("I01", PersistenceEvidenceStatus.Pass, 1, "run-001");
            Assertions.Equal(1L, observation.Sequence, "Observation sequence should be retained.");
            Assertions.Equal("I01", observation.StepId, "Observation row should be retained.");
            Assertions.Equal(PersistenceEvidenceStatus.Pass, observation.Status, "Observation status should be retained.");
        }

        private static void EvidenceObservationInvalidSequence()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => Evidence("I01", PersistenceEvidenceStatus.Pass, 0, "run-001"),
                "Observation sequence zero must be rejected.");
        }

        private static void EvidenceObservationUnknownStep()
        {
            Assertions.Throws<KeyNotFoundException>(
                () => Evidence("I99", PersistenceEvidenceStatus.Pass, 1, "run-001"),
                "Unknown observation rows must be rejected.");
        }

        private static void EvidenceObservationNonUtc()
        {
            Assertions.Throws<ArgumentException>(
                () => new PersistenceEvidenceObservation(
                    1,
                    "I01",
                    PersistenceEvidenceStatus.Pass,
                    "2026-07-13T08:00:00-04:00",
                    "run-001",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty),
                "Evidence timestamps with a nonzero offset must be rejected.");
        }

        private static void EvidenceObservationInvalidHash()
        {
            Assertions.Throws<ArgumentException>(
                () => new PersistenceEvidenceObservation(
                    1,
                    "I01",
                    PersistenceEvidenceStatus.Pass,
                    "2026-07-13T12:00:00Z",
                    "run-001",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "not-a-hash",
                    string.Empty),
                "Malformed save hashes must be rejected.");
        }

        private static void EvidenceObservationUppercaseHash()
        {
            string uppercase = new string('A', 64);
            PersistenceEvidenceObservation observation = new PersistenceEvidenceObservation(
                1,
                "I01",
                PersistenceEvidenceStatus.Pass,
                "2026-07-13T12:00:00Z",
                "run-001",
                string.Empty,
                string.Empty,
                string.Empty,
                uppercase,
                uppercase);
            Assertions.Equal(new string('a', 64), observation.SaveBeforeSha256, "Hashes should be canonicalized to lowercase.");
            Assertions.Equal(new string('a', 64), observation.SaveAfterSha256, "Hashes should be canonicalized to lowercase.");
        }

        private static void EvidenceEvaluationEmpty()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                Enumerable.Empty<PersistenceEvidenceObservation>());
            Assertions.Equal(PersistenceGateDecision.NoGoIncomplete, evaluation.Decision, "An empty evidence set cannot open the gate.");
            Assertions.Equal(30, evaluation.CriticalIncomplete, "All Critical rows should remain incomplete.");
        }

        private static void EvidenceEvaluationCriticalFail()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                new[] { Evidence("I01", PersistenceEvidenceStatus.Fail, 1, "run-001") });
            Assertions.Equal(PersistenceGateDecision.NoGoFailed, evaluation.Decision, "A Critical failure must close the gate as failed.");
            Assertions.Equal(1, evaluation.CriticalFailed, "One Critical failure should be counted.");
        }

        private static void EvidenceEvaluationBlocked()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                new[] { Evidence("I01", PersistenceEvidenceStatus.Blocked, 1, "run-001") });
            Assertions.Equal(PersistenceGateDecision.NoGoIncomplete, evaluation.Decision, "A blocked Critical row remains incomplete.");
            Assertions.True(evaluation.BlockingStepIds.Contains("I01"), "The blocked row should be reported as a blocker.");
        }

        private static void EvidenceEvaluationSinglePassIncomplete()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                CompleteCriticalEvidence(includeSecondReproductionRun: false, includeHighFailure: false));
            Assertions.Equal(PersistenceGateDecision.NoGoIncomplete, evaluation.Decision, "One pass cannot satisfy reproduction rows.");
            Assertions.Equal(7, evaluation.CriticalIncomplete, "All seven reproduction rows should remain incomplete.");
        }

        private static void EvidenceEvaluationReproducedGo()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                CompleteCriticalEvidence(includeSecondReproductionRun: true, includeHighFailure: false));
            Assertions.Equal(PersistenceGateDecision.Go, evaluation.Decision, "All Critical rows plus reproduced rows should open the gate.");
            Assertions.Equal(30, evaluation.CriticalPassed, "All Critical rows should pass.");
        }

        private static void EvidenceEvaluationHighFailWarning()
        {
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(
                CompleteCriticalEvidence(includeSecondReproductionRun: true, includeHighFailure: true));
            Assertions.Equal(PersistenceGateDecision.Go, evaluation.Decision, "A High-severity failure should warn without closing the gate.");
            Assertions.Equal(1, evaluation.HighFailed, "The High-severity failure should be counted.");
            Assertions.True(evaluation.Warnings.Any(value => value.StartsWith("I31", StringComparison.Ordinal)), "The High-severity failure should be identified.");
        }

        private static void EvidenceEvaluationLatestFail()
        {
            List<PersistenceEvidenceObservation> observations = CompleteCriticalEvidence(true, false);
            long next = observations.Max(item => item.Sequence) + 1;
            observations.Add(Evidence("I01", PersistenceEvidenceStatus.Fail, next, "run-003"));
            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(observations);
            Assertions.Equal(PersistenceGateDecision.NoGoFailed, evaluation.Decision, "The latest Critical result should control current status.");
        }

        private static void EvidenceEvaluationSameRunNotReproduced()
        {
            List<PersistenceEvidenceObservation> observations = CompleteCriticalEvidence(false, false);
            long next = observations.Max(item => item.Sequence) + 1;
            foreach (PersistenceMatrixStepDefinition step in PersistenceMatrixCatalog.All.Where(item => item.RequiresReproduction))
            {
                observations.Add(Evidence(step.Id, PersistenceEvidenceStatus.Pass, next++, "run-001"));
            }

            PersistenceEvidenceEvaluation evaluation = PersistenceEvidenceEvaluator.Evaluate(observations);
            Assertions.Equal(PersistenceGateDecision.NoGoIncomplete, evaluation.Decision, "Repeated passes in one run do not satisfy independent reproduction.");
            Assertions.Equal(7, evaluation.CriticalIncomplete, "All reproduction rows should still require another run ID.");
        }

        private static void EvidenceEvaluationNullObservation()
        {
            Assertions.Throws<ArgumentException>(
                () => PersistenceEvidenceEvaluator.Evaluate(new PersistenceEvidenceObservation[] { null }),
                "Null evidence entries must be rejected.");
        }

        private static void EvidenceEvaluationDuplicateSequence()
        {
            Assertions.Throws<InvalidOperationException>(
                () => PersistenceEvidenceEvaluator.Evaluate(new[]
                {
                    Evidence("I01", PersistenceEvidenceStatus.Pass, 1, "run-001"),
                    Evidence("I02", PersistenceEvidenceStatus.Pass, 1, "run-001")
                }),
                "Evidence sequence numbers must be globally unique.");
        }

        private static void EvidenceEvaluationFormat()
        {
            string text = PersistenceEvidenceEvaluator.Evaluate(
                Enumerable.Empty<PersistenceEvidenceObservation>()).ToString();
            Assertions.True(text.Contains("decision=NoGoIncomplete"), "Evaluation formatting should expose the decision.");
            Assertions.True(text.Contains("criticalIncomplete=30"), "Evaluation formatting should expose incomplete Critical rows.");
        }

        private static void PreflightProbeNegativeBootstrapCount()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new PersistenceRuntimePreflightProbeData(
                    true, true, -2, 8, 8, string.Empty,
                    true, 1, true, "System.Guid", string.Empty),
                "Bootstrap counts below the unavailable sentinel must be rejected.");
        }

        private static void PreflightProbeNegativeBlueprintCount()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new PersistenceRuntimePreflightProbeData(
                    true, true, 1, -2, 8, string.Empty,
                    true, 1, true, "System.Guid", string.Empty),
                "Blueprint counts below the unavailable sentinel must be rejected.");
        }

        private static void PreflightProbeInvalidExpectedCount()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new PersistenceRuntimePreflightProbeData(
                    true, true, 1, 8, 0, string.Empty,
                    true, 1, true, "System.Guid", string.Empty),
                "The preflight must require a positive expected blueprint count.");
        }

        private static void PreflightProbeNegativeIdentityCount()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new PersistenceRuntimePreflightProbeData(
                    true, true, 1, 8, 8, string.Empty,
                    true, -2, true, "System.Guid", string.Empty),
                "Identity counts below the unavailable sentinel must be rejected.");
        }

        private static void PreflightCheckOnlyI01I02()
        {
            Assertions.Throws<ArgumentException>(
                () => new PersistenceRuntimePreflightCheck(
                    "I03",
                    PersistenceEvidenceStatus.Pass,
                    "not eligible"),
                "Only the trusted I01/I02 checks may bypass manual snapshots.");
        }

        private static void PreflightReportOrderRequired()
        {
            Assertions.Throws<ArgumentException>(
                () => new PersistenceRuntimePreflightReport(new[]
                {
                    new PersistenceRuntimePreflightCheck("I02", PersistenceEvidenceStatus.Pass, "identity"),
                    new PersistenceRuntimePreflightCheck("I01", PersistenceEvidenceStatus.Pass, "bootstrap")
                }),
                "Preflight reports must retain deterministic I01/I02 order.");
        }

        private static void PreflightReportAllPassed()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe());
            Assertions.True(report.AllPassed, "A valid bootstrap and identity contract should pass both checks.");
            Assertions.Equal(2, report.Checks.Count, "The preflight report should contain exactly two checks.");
        }

        private static void PreflightReportRequire()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe());
            Assertions.Equal(PersistenceEvidenceStatus.Pass, report.Require("I01").Status, "I01 should be addressable.");
            Assertions.Throws<KeyNotFoundException>(
                () => report.Require("I03"),
                "Unknown preflight rows must be rejected.");
        }

        private static void PreflightEvaluatePassGuid()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityType: "System.Guid"));
            Assertions.Equal(PersistenceEvidenceStatus.Pass, report.Require("I01").Status, "I01 should pass.");
            Assertions.Equal(PersistenceEvidenceStatus.Pass, report.Require("I02").Status, "Guid UniqueId should pass.");
        }

        private static void PreflightEvaluatePassString()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityType: "System.String"));
            Assertions.Equal(PersistenceEvidenceStatus.Pass, report.Require("I02").Status, "String UniqueId should pass.");
        }

        private static void PreflightEvaluateBootstrapBlocked()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(
                    bootstrapInspectionSucceeded: false,
                    bootstrapInitializationCount: -1,
                    registeredBlueprintCount: -1));
            Assertions.Equal(PersistenceEvidenceStatus.Blocked, report.Require("I01").Status, "Unavailable bootstrap inspection should block I01.");
        }

        private static void PreflightEvaluateBootstrapNotInitialized()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(bootstrapInitialized: false));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I01").Status, "A completed inspection with no initialization should fail I01.");
        }

        private static void PreflightEvaluateBootstrapDuplicateInitialization()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(bootstrapInitializationCount: 2));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I01").Status, "Multiple initializations should fail I01.");
        }

        private static void PreflightEvaluateBootstrapCountMismatch()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(registeredBlueprintCount: 7));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I01").Status, "A registration-count mismatch should fail I01.");
        }

        private static void PreflightEvaluateIdentityBlocked()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityInspectionSucceeded: false, identityMemberCount: -1));
            Assertions.Equal(PersistenceEvidenceStatus.Blocked, report.Require("I02").Status, "Unavailable identity inspection should block I02.");
        }

        private static void PreflightEvaluateIdentityMissing()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityMemberCount: 0, identityReadable: false, identityType: string.Empty));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I02").Status, "A missing UniqueId should fail I02.");
        }

        private static void PreflightEvaluateIdentityDuplicate()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityMemberCount: 2));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I02").Status, "Duplicate UniqueId members should fail I02.");
        }

        private static void PreflightEvaluateIdentityUnreadable()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityReadable: false));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I02").Status, "An unreadable UniqueId should fail I02.");
        }

        private static void PreflightEvaluateIdentityUnsupportedType()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightEvaluator.Evaluate(
                PreflightProbe(identityType: "System.Int32"));
            Assertions.Equal(PersistenceEvidenceStatus.Fail, report.Require("I02").Status, "An unsupported UniqueId type should fail I02.");
        }

        private static void PreflightFormat()
        {
            string text = PersistenceRuntimePreflightEvaluator.Evaluate(PreflightProbe()).ToString();
            Assertions.True(text.StartsWith("I01=Pass[", StringComparison.Ordinal), "Preflight formatting should begin with I01.");
            Assertions.True(text.Contains(" | I02=Pass["), "Preflight formatting should include I02 second.");
        }

        private static PersistenceRuntimePreflightProbeData PreflightProbe(
            bool bootstrapInspectionSucceeded = true,
            bool bootstrapInitialized = true,
            int bootstrapInitializationCount = 1,
            int registeredBlueprintCount = 8,
            bool identityInspectionSucceeded = true,
            int identityMemberCount = 1,
            bool identityReadable = true,
            string identityType = "System.Guid")
        {
            return new PersistenceRuntimePreflightProbeData(
                bootstrapInspectionSucceeded,
                bootstrapInitialized,
                bootstrapInitializationCount,
                registeredBlueprintCount,
                8,
                "observations=1",
                identityInspectionSucceeded,
                identityMemberCount,
                identityReadable,
                identityType,
                "member=Kingmaker.Items.ItemEntity.UniqueId");
        }

        private static PersistenceEvidenceObservation Evidence(
            string stepId,
            PersistenceEvidenceStatus status,
            long sequence,
            string runId)
        {
            return new PersistenceEvidenceObservation(
                sequence,
                stepId,
                status,
                "2026-07-13T12:00:00Z",
                runId,
                string.Empty,
                "before",
                "after",
                string.Empty,
                string.Empty);
        }

        private static List<PersistenceEvidenceObservation> CompleteCriticalEvidence(
            bool includeSecondReproductionRun,
            bool includeHighFailure)
        {
            var observations = new List<PersistenceEvidenceObservation>();
            long sequence = 1;
            foreach (PersistenceMatrixStepDefinition step in PersistenceMatrixCatalog.All
                .Where(item => item.Severity == PersistenceEvidenceSeverity.Critical))
            {
                observations.Add(Evidence(step.Id, PersistenceEvidenceStatus.Pass, sequence++, "run-001"));
            }

            if (includeSecondReproductionRun)
            {
                foreach (PersistenceMatrixStepDefinition step in PersistenceMatrixCatalog.All.Where(item => item.RequiresReproduction))
                {
                    observations.Add(Evidence(step.Id, PersistenceEvidenceStatus.Pass, sequence++, "run-002"));
                }
            }

            if (includeHighFailure)
            {
                observations.Add(Evidence("I31", PersistenceEvidenceStatus.Fail, sequence, "run-001"));
            }

            return observations;
        }

        private sealed class ReflectionDerived : ReflectionBase
        {
        }

        private static class ReflectionStatic
        {
            internal static string Instance
            {
                get { return "singleton"; }
            }
        }

        private sealed class ReflectionNullableMembers
        {
            internal string First
            {
                get { return null; }
            }

            internal string Second
            {
                get { return "ready"; }
            }
        }

        private sealed class ReflectionRoot
        {
            internal ReflectionRoot()
            {
                Child = new ReflectionChild();
            }

            internal ReflectionChild Child { get; private set; }
        }

        private sealed class ReflectionChild
        {
            internal int Value
            {
                get { return 17; }
            }
        }

        private sealed class ReflectionMethods
        {
            internal int InvocationCount { get; private set; }

            private string Apply(string value, int count)
            {
                InvocationCount++;
                return value + ":" + count;
            }

            private string Optional(string value, int count = 4)
            {
                InvocationCount++;
                return value + ":" + count;
            }

            private string Contextual(string value, object context)
            {
                InvocationCount++;
                return value + ":" + (context == null ? "none" : "set");
            }

            private string RequiredFlag(string value, bool flag)
            {
                InvocationCount++;
                return value + ":" + flag;
            }
        }


        private sealed class FakeUnit
        {
            private readonly double _distanceMeters;

            internal FakeUnit(int ordinaryArmorClass, int touchArmorClass, double distanceMeters)
            {
                Stats = new FakeStats(ordinaryArmorClass, touchArmorClass);
                _distanceMeters = distanceMeters;
            }

            internal FakeStats Stats { get; private set; }

            private double DistanceTo(FakeUnit target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                return _distanceMeters;
            }
        }

        private sealed class FakeStats
        {
            internal FakeStats(int ordinaryArmorClass, int touchArmorClass)
            {
                AC = new FakeArmorClass(ordinaryArmorClass, touchArmorClass);
            }

            internal FakeArmorClass AC { get; private set; }
        }

        private sealed class FakeArmorClass
        {
            internal FakeArmorClass(int ordinaryArmorClass, int touchArmorClass)
            {
                ModifiedValue = ordinaryArmorClass;
                Touch = touchArmorClass;
            }

            internal int ModifiedValue { get; private set; }

            internal int Touch { get; private set; }
        }

        private sealed class FakeRuleCalculateAcProperty
        {
            internal FakeRuleCalculateAcProperty(FakeUnit initiator, FakeUnit target, int targetArmorClass)
            {
                Initiator = initiator;
                Target = target;
                TargetAC = targetArmorClass;
            }

            internal FakeUnit Initiator { get; private set; }

            internal FakeUnit Target { get; private set; }

            internal int TargetAC { get; private set; }

            internal int ReadTargetArmorClass()
            {
                return TargetAC;
            }
        }

        private sealed class FakeRuleCalculateAcField
        {
            internal FakeRuleCalculateAcField(FakeUnit initiator, FakeUnit target, int targetArmorClass)
            {
                Initiator = initiator;
                Target = target;
                TargetAC = targetArmorClass;
            }

            internal readonly FakeUnit Initiator;
            internal readonly FakeUnit Target;
            internal int TargetAC;
        }

        private class FakeRuleCalculateAcBase
        {
            internal FakeRuleCalculateAcBase(int targetArmorClass)
            {
                TargetAC = targetArmorClass;
            }

            internal int TargetAC { get; set; }

            internal int ReadBaseTargetArmorClass()
            {
                return TargetAC;
            }
        }

        private sealed class FakeRuleCalculateAcAmbiguous : FakeRuleCalculateAcBase
        {
            internal FakeRuleCalculateAcAmbiguous(FakeUnit initiator, FakeUnit target, int targetArmorClass)
                : base(targetArmorClass)
            {
                Initiator = initiator;
                Target = target;
                TargetAC = targetArmorClass;
            }

            internal FakeUnit Initiator { get; private set; }

            internal FakeUnit Target { get; private set; }

            internal new int TargetAC { get; set; }
        }

        private sealed class FakeFirearmItemIdentityProvider : IFirearmItemIdentityProvider
        {
            private readonly Dictionary<object, FirearmItemId> _identities =
                new Dictionary<object, FirearmItemId>(ReferenceIdentityComparer.Instance);
            private readonly Dictionary<object, string> _rejections =
                new Dictionary<object, string>(ReferenceIdentityComparer.Instance);
            private readonly HashSet<object> _nullIdentities =
                new HashSet<object>(ReferenceIdentityComparer.Instance);

            internal int CallCount { get; private set; }

            internal void Register(object item, FirearmItemId identity)
            {
                _identities[item] = identity;
            }

            internal void RegisterNull(object item)
            {
                _nullIdentities.Add(item);
            }

            internal void Reject(object item, string reason)
            {
                _rejections[item] = reason;
            }

            public bool TryGetIdentity(
                object itemInstance,
                out FirearmItemId identity,
                out string rejectionReason)
            {
                CallCount++;
                if (_nullIdentities.Contains(itemInstance))
                {
                    identity = null;
                    rejectionReason = null;
                    return true;
                }

                if (_identities.TryGetValue(itemInstance, out identity))
                {
                    rejectionReason = null;
                    return true;
                }

                if (!_rejections.TryGetValue(itemInstance, out rejectionReason))
                {
                    rejectionReason = "unregistered synthetic item identity";
                }

                identity = null;
                return false;
            }
        }

        private sealed class FakeFirearmStateIdentityRecordStore : IFirearmStateIdentityRecordStore
        {
            private readonly Dictionary<string, FirearmStateData> _records =
                new Dictionary<string, FirearmStateData>(StringComparer.Ordinal);

            internal int ReplaceCount { get; private set; }

            public int RecordCount
            {
                get { return _records.Count; }
            }

            public bool TryRead(FirearmItemId itemId, out FirearmStateData data)
            {
                FirearmStateData stored;
                if (!_records.TryGetValue(itemId.Value, out stored))
                {
                    data = null;
                    return false;
                }

                data = FirearmStateDataUtility.Clone(stored);
                return true;
            }

            public void Replace(
                FirearmItemId itemId,
                FirearmStateData expectedData,
                FirearmStateData targetData)
            {
                FirearmStateData current;
                _records.TryGetValue(itemId.Value, out current);
                if (!FirearmStateDataUtility.AreEqual(current, expectedData))
                {
                    throw new InvalidOperationException("Expected identity record mismatch.");
                }

                ReplaceCount++;
                if (targetData == null)
                {
                    _records.Remove(itemId.Value);
                }
                else
                {
                    _records[itemId.Value] = FirearmStateDataUtility.Clone(targetData);
                }
            }

            public bool Remove(FirearmItemId itemId)
            {
                return _records.Remove(itemId.Value);
            }
        }

        private sealed class FakeFirearmStateVaultStore : IFirearmStateVaultStore
        {
            private readonly Dictionary<object, FirearmStateData> _records =
                new Dictionary<object, FirearmStateData>(ReferenceIdentityComparer.Instance);

            internal bool ThrowOnReplace { get; set; }

            internal bool ThrowOnRemove { get; set; }

            internal bool CorruptWrites { get; set; }

            internal Action<object> BeforeReplace { get; set; }

            internal int ReplaceCount { get; private set; }

            internal int RemoveCount { get; private set; }

            public int RecordCount
            {
                get { return _records.Count; }
            }

            public bool TryRead(object itemInstance, out FirearmStateData data)
            {
                FirearmStateData stored;
                if (!_records.TryGetValue(itemInstance, out stored))
                {
                    data = null;
                    return false;
                }

                data = FirearmStateDataUtility.Clone(stored);
                return true;
            }

            public void Replace(
                object itemInstance,
                FirearmStateData expectedData,
                FirearmStateData targetData)
            {
                Action<object> before = BeforeReplace;
                BeforeReplace = null;
                if (before != null)
                {
                    before(itemInstance);
                }

                FirearmStateData current;
                _records.TryGetValue(itemInstance, out current);
                if (!FirearmStateDataUtility.AreEqual(current, expectedData))
                {
                    throw new InvalidOperationException("Expected vault data mismatch.");
                }

                ReplaceCount++;
                if (ThrowOnReplace)
                {
                    throw new InvalidOperationException("Synthetic vault replacement failure.");
                }

                if (targetData == null)
                {
                    _records.Remove(itemInstance);
                    return;
                }

                _records[itemInstance] = CorruptWrites
                    ? FirearmStateCodec.ToData(TokenBrokenEmpty())
                    : FirearmStateDataUtility.Clone(targetData);
            }

            public bool Remove(object itemInstance)
            {
                if (ThrowOnRemove)
                {
                    throw new InvalidOperationException("Synthetic vault removal failure.");
                }

                bool removed = _records.Remove(itemInstance);
                if (removed)
                {
                    RemoveCount++;
                }

                return removed;
            }

            internal void Seed(object itemInstance, FirearmStateData data)
            {
                if (data == null)
                {
                    _records.Remove(itemInstance);
                }
                else
                {
                    _records[itemInstance] = FirearmStateDataUtility.Clone(data);
                }
            }
        }

        private sealed class FakeFirearmStateTokenStore : IFirearmStateTokenStore
        {
            private readonly Dictionary<object, List<string>> _tokens =
                new Dictionary<object, List<string>>(ReferenceIdentityComparer.Instance);

            internal bool ThrowOnReplace { get; set; }

            internal bool CorruptWrites { get; set; }

            internal bool ThrowOnClear { get; set; }

            internal Action<object> BeforeReplace { get; set; }

            internal int ReplaceCount { get; private set; }

            internal int ClearCount { get; private set; }

            public IReadOnlyList<string> ReadTokenIds(object itemInstance)
            {
                List<string> values;
                if (!_tokens.TryGetValue(itemInstance, out values))
                {
                    return Array.Empty<string>();
                }

                return values.ToArray();
            }

            public void ReplaceToken(
                object itemInstance,
                string expectedCurrentTokenId,
                string targetTokenId)
            {
                Action<object> before = BeforeReplace;
                BeforeReplace = null;
                if (before != null)
                {
                    before(itemInstance);
                }

                string current = Single(itemInstance);
                if (!string.Equals(current, expectedCurrentTokenId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Expected token mismatch.");
                }

                ReplaceCount++;
                if (ThrowOnReplace)
                {
                    throw new InvalidOperationException("Synthetic replacement failure.");
                }

                if (CorruptWrites)
                {
                    Seed(itemInstance, "kmg.state.v99.corrupt");
                    return;
                }

                if (targetTokenId == null)
                {
                    _tokens.Remove(itemInstance);
                }
                else
                {
                    Seed(itemInstance, targetTokenId);
                }
            }

            public bool ClearTokens(object itemInstance)
            {
                if (ThrowOnClear)
                {
                    throw new InvalidOperationException("Synthetic token-clear failure.");
                }

                bool removed = _tokens.Remove(itemInstance);
                if (removed)
                {
                    ClearCount++;
                }

                return removed;
            }

            internal void Seed(object itemInstance, params string[] tokenIds)
            {
                _tokens[itemInstance] = new List<string>(tokenIds ?? Array.Empty<string>());
            }

            internal string Single(object itemInstance)
            {
                IReadOnlyList<string> values = ReadTokenIds(itemInstance);
                if (values.Count == 0)
                {
                    return null;
                }

                if (values.Count != 1)
                {
                    throw new InvalidOperationException("Synthetic store contains multiple tokens.");
                }

                return values[0];
            }
        }

        private sealed class FakeFirearmReloadStateStore : IFirearmReloadStateStore
        {
            private FirearmState _state;

            internal FakeFirearmReloadStateStore(FirearmState state)
            {
                _state = state ?? throw new ArgumentNullException("state");
            }

            internal FirearmState State { get { return _state; } }
            internal int ReadCalls { get; private set; }
            internal int ReplaceCalls { get; private set; }
            internal int ThrowOnReplaceCall { get; set; }
            internal bool MutateBeforeReplaceFailure { get; set; }
            internal bool ThrowOnSecondReplace { get; set; }
            internal bool ReturnNullOnRead { get; set; }

            public FirearmState Read()
            {
                ReadCalls++;
                return ReturnNullOnRead ? null : _state;
            }

            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (expectedCurrent == null)
                {
                    throw new ArgumentNullException("expectedCurrent");
                }

                if (replacement == null)
                {
                    throw new ArgumentNullException("replacement");
                }

                ReplaceCalls++;
                if (_state != expectedCurrent)
                {
                    throw new InvalidOperationException("Synthetic expected-current mismatch.");
                }

                bool shouldThrow = ReplaceCalls == ThrowOnReplaceCall ||
                    (ReplaceCalls == 2 && ThrowOnSecondReplace);
                if (shouldThrow && !MutateBeforeReplaceFailure)
                {
                    throw new InvalidOperationException("Synthetic state-replace failure.");
                }

                _state = replacement;
                if (shouldThrow)
                {
                    throw new InvalidOperationException("Synthetic post-mutation state-replace failure.");
                }
            }
        }

        private sealed class FakeFirearmOverhaulStateStore : IFirearmOverhaulStateStore
        {
            private FirearmState _state;

            internal FakeFirearmOverhaulStateStore(FirearmState state)
            {
                _state = state ?? throw new ArgumentNullException("state");
            }

            internal FirearmState State { get { return _state; } }
            internal int ReadCalls { get; private set; }
            internal int ReplaceCalls { get; private set; }
            internal int ThrowOnReplaceCall { get; set; }
            internal bool MutateBeforeReplaceFailure { get; set; }
            internal bool ThrowOnSecondReplace { get; set; }
            internal bool ReturnNullOnRead { get; set; }

            public FirearmState Read()
            {
                ReadCalls++;
                return ReturnNullOnRead ? null : _state;
            }

            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (expectedCurrent == null || replacement == null)
                {
                    throw new ArgumentNullException(expectedCurrent == null ? "expectedCurrent" : "replacement");
                }

                ReplaceCalls++;
                if (_state != expectedCurrent)
                {
                    throw new InvalidOperationException("Synthetic overhaul expected-current mismatch.");
                }

                bool shouldThrow = ReplaceCalls == ThrowOnReplaceCall ||
                    (ReplaceCalls == 2 && ThrowOnSecondReplace);
                if (shouldThrow && !MutateBeforeReplaceFailure)
                {
                    throw new InvalidOperationException("Synthetic overhaul state-replace failure.");
                }

                _state = replacement;
                if (shouldThrow)
                {
                    throw new InvalidOperationException("Synthetic overhaul post-mutation state-replace failure.");
                }
            }
        }

        private sealed class FakeRepairKitInventory : IRepairKitInventory
        {
            private int _kits;

            internal FakeRepairKitInventory(int kits)
            {
                if (kits < 0)
                {
                    throw new ArgumentOutOfRangeException("kits");
                }

                _kits = kits;
            }

            internal int Kits { get { return _kits; } }
            internal int CountCalls { get; private set; }
            internal int AddCalls { get; private set; }
            internal int RemoveCalls { get; private set; }
            internal int ThrowOnRemoveCall { get; set; }
            internal bool MutateBeforeRemoveFailure { get; set; }
            internal bool ThrowOnAdd { get; set; }
            internal bool ReportNegativeCount { get; set; }

            public int Count()
            {
                CountCalls++;
                return ReportNegativeCount ? -1 : _kits;
            }

            public void Add(int amount)
            {
                ValidateAmount(amount);
                AddCalls++;
                if (ThrowOnAdd)
                {
                    throw new InvalidOperationException("Synthetic repair-kit add failure.");
                }

                checked { _kits += amount; }
            }

            public void Remove(int amount)
            {
                ValidateAmount(amount);
                RemoveCalls++;
                bool shouldThrow = ThrowOnRemoveCall > 0 && RemoveCalls == ThrowOnRemoveCall;
                if (shouldThrow && !MutateBeforeRemoveFailure)
                {
                    throw new InvalidOperationException("Synthetic repair-kit remove failure.");
                }

                if (_kits < amount)
                {
                    throw new InvalidOperationException("Synthetic repair-kit underflow.");
                }

                _kits -= amount;
                if (shouldThrow)
                {
                    throw new InvalidOperationException("Synthetic post-mutation repair-kit remove failure.");
                }
            }

            private static void ValidateAmount(int amount)
            {
                if (amount <= 0)
                {
                    throw new ArgumentOutOfRangeException("amount");
                }
            }
        }

        private sealed class FakeBasicAmmunitionInventory : IBasicAmmunitionInventory
        {
            private int _powder;
            private int _balls;

            internal FakeBasicAmmunitionInventory(int powder, int balls)
            {
                if (powder < 0 || balls < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "powder",
                        "Synthetic inventory counts must be nonnegative.");
                }

                _powder = powder;
                _balls = balls;
            }

            internal int Powder
            {
                get { return _powder; }
            }

            internal int Balls
            {
                get { return _balls; }
            }

            internal int RemoveCalls { get; private set; }

            internal int AddCalls { get; private set; }

            internal int ThrowOnRemoveCall { get; set; }

            internal bool MutateBeforeRemoveFailure { get; set; }

            internal bool ThrowOnAdd { get; set; }

            internal int ExtraPowderRemovedOnFirstRemove { get; set; }

            internal bool ReportNegativePowder { get; set; }

            public int Count(BasicAmmunitionComponent component)
            {
                switch (component)
                {
                    case BasicAmmunitionComponent.BlackPowderCharge:
                        return ReportNegativePowder ? -1 : _powder;
                    case BasicAmmunitionComponent.LeadBall:
                        return _balls;
                    default:
                        throw new ArgumentOutOfRangeException("component");
                }
            }

            public void Add(BasicAmmunitionComponent component, int amount)
            {
                ValidateAmount(amount);
                AddCalls++;
                if (ThrowOnAdd)
                {
                    throw new InvalidOperationException("Synthetic add failure.");
                }

                switch (component)
                {
                    case BasicAmmunitionComponent.BlackPowderCharge:
                        checked { _powder += amount; }
                        break;
                    case BasicAmmunitionComponent.LeadBall:
                        checked { _balls += amount; }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("component");
                }
            }

            public void Remove(BasicAmmunitionComponent component, int amount)
            {
                ValidateAmount(amount);
                RemoveCalls++;
                bool shouldThrow = ThrowOnRemoveCall > 0 &&
                    RemoveCalls == ThrowOnRemoveCall;
                if (shouldThrow && !MutateBeforeRemoveFailure)
                {
                    throw new InvalidOperationException("Synthetic remove failure.");
                }

                int actualAmount = amount;
                if (component == BasicAmmunitionComponent.BlackPowderCharge &&
                    RemoveCalls == 1 &&
                    ExtraPowderRemovedOnFirstRemove > 0)
                {
                    actualAmount = checked(actualAmount + ExtraPowderRemovedOnFirstRemove);
                }

                switch (component)
                {
                    case BasicAmmunitionComponent.BlackPowderCharge:
                        if (_powder < actualAmount)
                        {
                            throw new InvalidOperationException("Synthetic powder underflow.");
                        }

                        _powder -= actualAmount;
                        break;
                    case BasicAmmunitionComponent.LeadBall:
                        if (_balls < actualAmount)
                        {
                            throw new InvalidOperationException("Synthetic lead-ball underflow.");
                        }

                        _balls -= actualAmount;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("component");
                }

                if (shouldThrow)
                {
                    throw new InvalidOperationException("Synthetic post-mutation remove failure.");
                }
            }

            private static void ValidateAmount(int amount)
            {
                if (amount <= 0)
                {
                    throw new ArgumentOutOfRangeException("amount");
                }
            }
        }

        private struct FakeRollEntry
        {
            public int Value { get; set; }
        }

        private class ExactMisfireRuleAttackRoll
        {
            public FakeRollEntry Roll { get; private set; }

            public bool IsSuccessRoll(int d20)
            {
                return d20 >= 10;
            }
        }

        private sealed class PublicRollSetterRuleAttackRoll
        {
            public FakeRollEntry Roll { get; set; }
        }

        private sealed class WrongRollEntrySetterRuleAttackRoll
        {
            public int Roll { get; private set; }
        }

        private class BaseRollSetterRuleAttackRoll
        {
            public FakeRollEntry Roll { get; private set; }
        }

        private sealed class InheritedRollSetterRuleAttackRoll : BaseRollSetterRuleAttackRoll
        {
        }

        private sealed class PrivateSuccessRuleAttackRoll
        {
            private bool IsSuccessRoll(int d20)
            {
                return d20 >= 10;
            }
        }

        private sealed class WrongArgumentSuccessRuleAttackRoll
        {
            public bool IsSuccessRoll(long d20)
            {
                return d20 >= 10L;
            }
        }

        private sealed class WrongReturnSuccessRuleAttackRoll
        {
            public int IsSuccessRoll(int d20)
            {
                return d20;
            }
        }

        private sealed class StaticSuccessRuleAttackRoll
        {
            public static bool IsSuccessRoll(int d20)
            {
                return d20 >= 10;
            }
        }

        private sealed class GenericSuccessRuleAttackRoll
        {
            public bool IsSuccessRoll<T>(int d20)
            {
                return d20 >= 10;
            }
        }

        private class BaseSuccessRuleAttackRoll
        {
            public bool IsSuccessRoll(int d20)
            {
                return d20 >= 10;
            }
        }

        private sealed class InheritedSuccessRuleAttackRoll : BaseSuccessRuleAttackRoll
        {
        }

        private sealed class FakeRulebookEventContext
        {
        }

        private sealed class DifferentRulebookEventContext
        {
        }

        private sealed class ExactRuleEvent
        {
            public void OnTrigger(FakeRulebookEventContext context)
            {
            }
        }

        private sealed class ZeroArgumentRuleEvent
        {
            public void OnTrigger()
            {
            }
        }

        private sealed class WrongContextRuleEvent
        {
            public void OnTrigger(DifferentRulebookEventContext context)
            {
            }
        }

        private sealed class MultipleArgumentRuleEvent
        {
            public void OnTrigger(
                FakeRulebookEventContext context,
                object extra)
            {
            }
        }

        private sealed class StaticRuleEvent
        {
            public static void OnTrigger(FakeRulebookEventContext context)
            {
            }
        }

        private sealed class GenericRuleEvent
        {
            public void OnTrigger<T>(FakeRulebookEventContext context)
            {
            }
        }

        private sealed class NonVoidRuleEvent
        {
            public int OnTrigger(FakeRulebookEventContext context)
            {
                return 0;
            }
        }

        private sealed class ValueEqualItem
        {
            private readonly int _value;

            internal ValueEqualItem(int value)
            {
                _value = value;
            }

            public override bool Equals(object obj)
            {
                ValueEqualItem other = obj as ValueEqualItem;
                return other != null && other._value == _value;
            }

            public override int GetHashCode()
            {
                return _value;
            }
        }

        private sealed class FakeFirearmRuntimeItemResolver : IFirearmRuntimeItemResolver
        {
            private readonly Dictionary<object, ResolvedFirearmItem> _resolved =
                new Dictionary<object, ResolvedFirearmItem>(ReferenceIdentityComparer.Instance);
            private readonly Dictionary<object, string> _rejected =
                new Dictionary<object, string>(ReferenceIdentityComparer.Instance);

            internal void Register(
                object candidate,
                object itemInstance,
                string name)
            {
                _resolved.Add(
                    candidate,
                    new ResolvedFirearmItem(
                        itemInstance,
                        EarlyMusket(),
                        name,
                        "runtime-" + name,
                        "item-blueprint-" + name,
                        "item-blueprint-" + name,
                        "weapon-type-" + name,
                        "weapon-type-id-" + name));
            }

            internal void Reject(object candidate, string reason)
            {
                _rejected.Add(candidate, reason);
            }

            public bool TryResolve(
                object candidate,
                out ResolvedFirearmItem firearm,
                out string rejectionReason)
            {
                if (_resolved.TryGetValue(candidate, out firearm))
                {
                    rejectionReason = null;
                    return true;
                }

                if (!_rejected.TryGetValue(candidate, out rejectionReason))
                {
                    rejectionReason = "unregistered fake candidate";
                }

                firearm = null;
                return false;
            }
        }

        private sealed class TestCase
        {
            internal TestCase(string name, Action body)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("A test name is required.", "name");
                }

                Name = name;
                Body = body ?? throw new ArgumentNullException("body");
            }

            internal string Name { get; private set; }

            internal Action Body { get; private set; }
        }
    }
}
