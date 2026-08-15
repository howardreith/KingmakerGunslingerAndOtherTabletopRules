using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurContractTests
    {
        internal static void NormalProgressionIsExact()
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(
                new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 });
            Assertions.True(decision.Compatible &&
                decision.Shape == CotwProgressionShape.Normal &&
                decision.PowerfulChangeReplacementLevel == 3 &&
                decision.ShareTransmutationReplacementLevel == 9,
                "Normal CotW progression must replace exploits at levels 3 and 9.");
        }

        internal static void BalanceProgressionIsExact()
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(
                new[] { 1, 4, 7, 10, 13, 16, 19 });
            Assertions.True(decision.Compatible &&
                decision.Shape == CotwProgressionShape.BalanceFixes &&
                decision.PowerfulChangeReplacementLevel == 4 &&
                decision.ShareTransmutationReplacementLevel == 10,
                "Balance-fixes CotW progression must replace exploits at 4 and 10.");
        }

        internal static void UnknownProgressionsFailClosed()
        {
            AssertRejected(null, "null schedule");
            AssertRejected(new int[0], "missing schedule");
            AssertRejected(new[] { 1, 3, 3, 9 }, "duplicate schedule");
            AssertRejected(new[] { 1, 5, 3, 9 }, "unordered schedule");
            AssertRejected(new[] { 0, 3, 9 }, "out-of-range schedule");
            AssertRejected(new[] { 1, 3, 5, 7, 9 }, "partial schedule");
            AssertRejected(new[] { 1, 3, 6, 9, 12, 15, 18 },
                "unknown future schedule");
        }

        internal static void AbsentCotwIsUnavailable()
        {
            CotwArcanistContractDecision decision =
                CotwArcanistContractPolicy.Evaluate(null);
            Assertions.True(decision.Availability ==
                CotwContractAvailability.Unavailable && !decision.IsCompatible,
                "Absent CotW must be unavailable rather than package-fatal.");
        }

        internal static void CompleteContractIsCompatible()
        {
            CotwArcanistContractDecision normal =
                CotwArcanistContractPolicy.Evaluate(Valid(false));
            CotwArcanistContractDecision balance =
                CotwArcanistContractPolicy.Evaluate(Valid(true));
            Assertions.True(normal.IsCompatible && balance.IsCompatible &&
                normal.Progression.Shape == CotwProgressionShape.Normal &&
                balance.Progression.Shape == CotwProgressionShape.BalanceFixes,
                "Both known structurally complete CotW contracts must be accepted.");
        }

        internal static void EveryRequiredSurfaceFailsClosed()
        {
            var checks = new Dictionary<string, Action<CotwArcanistContractCandidate>>
            {
                { "cotw-active", value => value.CotwActive = false },
                { "assembly-identity", value => value.AssemblyIdentityResolved = false },
                { "arcanist-class", value => value.ArcanistClassResolved = false },
                { "arcanist-progression", value => value.ArcanistProgressionResolved = false },
                { "casting-spellbook", value => value.CastingSpellbookResolved = false },
                { "memorization-spellbook", value => value.MemorizationSpellbookResolved = false },
                { "arcane-reservoir", value => value.ReservoirResolved = false },
                { "exploit-selection", value => value.ExploitSelectionResolved = false },
                { "magical-supremacy", value => value.MagicalSupremacyResolved = false },
                { "shared-spells-signature", value => value.SharedSpellsContractResolved = false },
                { "archetype-array", value => value.ArchetypeArrayResolved = false },
                { "transmutation-inventory", value => value.TransmutationInventoryResolved = false }
            };
            foreach (KeyValuePair<string, Action<CotwArcanistContractCandidate>> check in checks)
            {
                CotwArcanistContractCandidate candidate = Valid(false);
                check.Value(candidate);
                CotwArcanistContractDecision decision =
                    CotwArcanistContractPolicy.Evaluate(candidate);
                Assertions.True(!decision.IsCompatible &&
                    decision.Availability == CotwContractAvailability.Incompatible &&
                    decision.FailedCheck == check.Key,
                    "Missing contract surface did not fail closed: " + check.Key);
            }
        }

        internal static void AmbiguousProgressionBlocksContract()
        {
            CotwArcanistContractCandidate candidate = Valid(false);
            candidate.ExploitBearingLevels = new[] { 1, 3, 3, 9 };
            CotwArcanistContractDecision decision =
                CotwArcanistContractPolicy.Evaluate(candidate);
            Assertions.True(!decision.IsCompatible &&
                decision.FailedCheck.StartsWith("exploit-progression:",
                    StringComparison.Ordinal),
                "An ambiguous exploit schedule must block Brown-Fur publication.");
        }

        internal static void ContractPolicyIsIdempotent()
        {
            CotwArcanistContractCandidate candidate = Valid(true);
            CotwArcanistContractDecision first =
                CotwArcanistContractPolicy.Evaluate(candidate);
            CotwArcanistContractDecision second =
                CotwArcanistContractPolicy.Evaluate(candidate);
            Assertions.True(first.IsCompatible && second.IsCompatible &&
                first.Progression.Shape == second.Progression.Shape &&
                first.Progression.PowerfulChangeReplacementLevel ==
                    second.Progression.PowerfulChangeReplacementLevel &&
                first.Progression.ShareTransmutationReplacementLevel ==
                    second.Progression.ShareTransmutationReplacementLevel,
                "Repeated contract resolution must produce the same decision.");
        }

        internal static void RuntimeResolverUsesExactOptionalContract()
        {
            string root = Environment.CurrentDirectory;
            string brownFur = Path.Combine(root, "src", "KingmakerGunslinger",
                "BrownFur");
            string resolver = File.ReadAllText(Path.Combine(brownFur,
                "CotwArcanistResolver.cs"));
            string bridge = File.ReadAllText(Path.Combine(brownFur,
                "CotwSharedSpellsBridge.cs"));
            string coordinator = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurOptionalExtensionCoordinator.cs"));
            string main = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Main.cs"));
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurCotwContractObserver.cs"));
            string scenarios = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string runtimeCommon = File.ReadAllText(Path.Combine(root,
                "scripts", "RuntimeAutomation.Common.ps1"));
            string inventory = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurTransmutationInventory.cs"));
            string inventoryRecord = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurSpellInventoryRecord.cs"));
            string inventoryObserver = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurTransmutationInventoryObserver.cs"));
            string castEngineObserver = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurCastEngineContractObserver.cs"));
            string bonusCarrierScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurBonusCarrierScenario.cs"));
            string shareTargetingScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurShareTargetingScenario.cs"));
            string modifierRuntime = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurModifierAdjustmentRuntime.cs"));
            string modifierPatch = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurModifierAdjustmentPatch.cs"));
            string shareTargetingRuntime = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurShareTargetingRuntime.cs"));
            string shareTargetingPatches = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurShareTargetingPatches.cs"));
            string supremacyRuntime = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurSupremacyRuntime.cs"));
            string supremacyPatch = File.ReadAllText(Path.Combine(brownFur,
                "BrownFurSupremacyPatch.cs"));
            string supremacyScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurSupremacyScenario.cs"));
            string reservoirScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurReservoirScenario.cs"));
            string castExecutionRuntime = File.ReadAllText(Path.Combine(
                brownFur, "BrownFurCastExecutionRuntime.cs"));
            string castExecutionPatches = File.ReadAllText(Path.Combine(
                brownFur, "BrownFurCastExecutionPatches.cs"));
            string castExecutionScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurCastExecutionScenario.cs"));
            string arcanistSlotScenario = File.ReadAllText(Path.Combine(root,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurArcanistSlotScenario.cs"));
            string ilDisassembler = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "BrownFurIlDisassembler.cs"));
            Assertions.Equal(3, scenarios.Split(new[] {
                "DisposableBrownFurBonusCarriers" },
                StringSplitOptions.None).Length,
                "The disposable bonus carrier constant must be declared and " +
                "present exactly once in the in-process allowlist.");
            Assertions.Equal(3, scenarios.Split(new[] {
                "DisposableBrownFurShareTargeting" },
                StringSplitOptions.None).Length,
                "The disposable Share targeting constant must be declared and " +
                "present exactly once in the in-process allowlist.");
            Assertions.Equal(3, scenarios.Split(new[] {
                "DisposableBrownFurTransmutationSupremacy" },
                StringSplitOptions.None).Length,
                "The disposable Supremacy constant must be declared and " +
                "present exactly once in the in-process allowlist.");
            Assertions.Equal(3, scenarios.Split(new[] {
                "DisposableBrownFurCastExecution" },
                StringSplitOptions.None).Length,
                "The disposable cast execution constant must be declared and " +
                "present exactly once in the in-process allowlist.");
            Assertions.Equal(3, scenarios.Split(new[] {
                "DisposableBrownFurArcanistSlot" },
                StringSplitOptions.None).Length,
                "The disposable Arcanist slot constant must be declared and " +
                "present exactly once in the in-process allowlist.");
            foreach (string token in new[] { "arcanist_class",
                "arcanist_progression", "arcanist_spellbook",
                "memorization_spellbook", "arcane_reservoir_resource",
                "arcane_exploits", "magical_supremacy",
                "19c3cf3d51cf4cbf9a136a600c26585a",
                "2d28526efc2e4a9cb6a84c85267fb344",
                "0c21cfcab6ce4395bd4df330ab3cf715",
                "ab76417567444a6cb87d9d53e9752955",
                "3b775ee982444493b3de8f7bc31bd872",
                "2d86a417ab1542f98a8444b2b97d4951",
                "ContainsAtLevel", "ResolveExploitLevels",
                "ResolveTransmutations" })
                Assertions.True(resolver.Contains(token),
                    "CotW resolver lacks exact structural contract token: " + token);
            Assertions.True(resolver.Contains(
                "GetProperty(\"balance_fixes\"") && resolver.Contains(
                "property.GetIndexParameters().Length != 0"),
                "CotW resolver does not read the exact immutable balance setting property.");
            foreach (string token in new[] { "CallOfTheWild.SharedSpells",
                "canShareSpell", "isValidShareSpellTarget",
                "typeof(AbilityData)", "typeof(UnitEntityData)",
                "typeof(UnitDescriptor)", "matches.Length == 1" })
                Assertions.True(bridge.Contains(token),
                    "Shared Spells bridge lacks exact signature guard: " + token);
            foreach (string token in new[] { "createArcanistClass",
                "AfterCotwArcanistCreation", "HarmonyMethod(postfix)",
                "FirstUpdate", "OnUpdate -= FirstUpdate", "_reconciling",
                "contract.blocked", "Independent modules remain active",
                "DescribePatchOrder" })
                Assertions.True(coordinator.Contains(token),
                    "Optional coordinator lacks lifecycle/isolation guard: " + token);
            Assertions.True(main.Contains(
                "BrownFurOptionalExtensionCoordinator.Install(context)"),
                "Package bootstrap does not invoke isolated Brown-Fur coordination.");
            foreach (string token in new[] {
                "observe-brown-fur-cotw-contract", "cotw-contract-resolution",
                "cotw-progression-shape", "cotw-required-identities",
                "cotw-balance-setting-agrees-with-progression",
                "cotw-shared-spells-signatures",
                "cotw-transmutation-inventory-presence",
                "cotw-fingerprint-binary", "save-free-observer" })
                Assertions.True(observer.Contains(token) || scenarios.Contains(token) ||
                    runtimeCommon.Contains(token),
                    "Guarded CotW observer lacks structured evidence token: " + token);
            Assertions.False(Directory.GetFiles(brownFur, "*.cs")
                .Select(File.ReadAllText).Any(value =>
                    value.Contains("using CallOfTheWild")),
                "Brown-Fur acquired a compile-time CotW namespace dependency.");
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.False(project.Contains("CallOfTheWild.dll") ||
                project.Contains("Reference Include=\"CallOfTheWild"),
                "The package project acquired a compile-time CotW assembly reference.");
            foreach (string token in new[] { "CanonicalSpellGuid", "ParentGuid",
                "VariantGuids", "ConvertedFrom", "SpellLevels",
                "SpellbookSourceGuid", "TargetAnchor", "TargetRestrictions",
                "MetamagicSupport", "AppliedBuffs", "NestedActionGraph",
                "AbilityScoreBonuses", "ModifierDescriptors", "ValuePatterns",
                "PolymorphAndSizeComponents", "HardCodedToCaster",
                "SaveAndDispel", "ShareTransmutationCompatibility",
                "PowerfulChangeCompatibility",
                "TransmutationSupremacyCompatibility", "RequiredAdapter",
                "QualificationStatus", "Unexplained", "ExpandVariants",
                "ambiguous parents", "IsAppliedBuffPath",
                "action > traversedBuff", "BlueprintAbilityAreaEffect",
                "value is BlueprintScriptableObject",
                "DescribeAbilityBonusCarrier", "IsAbilityStat",
                "value == \"Strength\"", "value == \"Charisma\"",
                "depth > 24", ".Conditions", "positiveBonus",
                "IsPositive" })
                Assertions.True(inventory.Contains(token) ||
                    inventoryRecord.Contains(token),
                    "Transmutation inventory lacks required field/guard: " + token);
            foreach (string token in new[] {
                "observe-brown-fur-transmutation-inventory",
                "brown-fur-transmutation-spell-inventory.json",
                "inventory-complete-root-set",
                "inventory-variant-identities-singular",
                "inventory-required-fields", "inventory-publication-gate",
                "save-free-observer" })
                Assertions.True(inventoryObserver.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded Transmutation inventory lacks evidence token: " + token);
            foreach (string token in new[] {
                "observe-brown-fur-cast-engine-contract",
                "brown-fur-cast-engine-contract.json",
                "cast-engine-command-lifecycle",
                "cast-engine-canonicalization", "cast-engine-rule-commit",
                "cast-engine-slot-accounting",
                "cast-engine-modifier-registration",
                "cast-engine-modifier-source-provenance",
                "cast-engine-ability-bonus-carriers", "ModValue",
                "ModDescriptor", "SourceComponent", "MaybeContext",
                "AddContextStatBonus", "AddGenericStatBonus",
                "AddStatBonusAbilityValue", "ChangeUnitSize",
                "cast-engine-duration-context",
                "cast-engine-shared-spells-harmony", "GetPatchedMethods",
                "cast-engine-shared-spells-bodies", "SharedSpellsBodies",
                "cast-engine-cotw-targeting-bodies",
                "RelevantCotwTargetingBodies", "IsTargetingPatch",
                "AbilityData__CanTarget__Patch.Prefix",
                "AbilityData__TargetAnchor__Getter__Patch.Prefix",
                "cast-engine-native-delivery-bodies", "NativeDeliveryBodies",
                "DescribeNativeDeliveryBodies", "GetApproachDistance",
                "ShouldUnitApproach", "ApproachRadius",
                "cast-engine-commit-bodies", "CastCommitBodies",
                "DescribeCastCommitBodies", "SpendFromSpellbook",
                "typeof(AbilityData).GetMethod(\"Cast\"",
                "typeof(AbilityData).GetMethod(\"Spend\"",
                "typeof(RuleCastSpell).GetConstructor",
                "AbilityExecutionContext).GetConstructor",
                "cast-engine-execution-lifecycle",
                "AbilityExecutionProcess", "ExecutionLifecycleBodies",
                "DescribeExecutionLifecycleBodies", "CreateExecutionContext",
                "CalculateParams", "ProcessRoutine", "<Tick>b__12_0",
                "InstantDeliver", "AbilityExecutionController",
                "priority=", "before=", "after=", "save-free-observer" })
                Assertions.True(castEngineObserver.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded cast engine contract lacks evidence token: " + token);
            foreach (string token in new[] { "GetILAsByteArray",
                "OpCodes", "OperandType", "ResolveMember", "ResolveString",
                "InlineMethod", "InlineField", "InlineSwitch",
                "MethodBase", "info.IsGenericMethod",
                "info.GetGenericArguments()" })
                Assertions.True(ilDisassembler.Contains(token),
                    "CotW helper IL decoder lacks exact guard: " + token);
            foreach (string token in new[] { "AbilityExecutionContext",
                "context.Params.HasMetamagic(Metamagic.Extend)",
                "context.Params.Metamagic |= Metamagic.Extend",
                "ModifiedContextCount", "Scopes.Release", "Scopes.Clear" })
                Assertions.True(supremacyRuntime.Contains(token),
                    "Supremacy runtime lacks exact scope guard: " + token);
            foreach (string token in new[] { "CreateExecutionContext",
                "typeof(TargetWrapper)", "HarmonyAfter(\"CallOfTheWild\")",
                "BrownFurSupremacyRuntime.TryApply" })
                Assertions.True(supremacyPatch.Contains(token),
                    "Supremacy patch lacks exact execution ordering: " + token);
            foreach (string token in new[] {
                "disposable-brown-fur-transmutation-supremacy",
                "brown-fur-transmutation-supremacy.json",
                "supremacy-context-baseline",
                "supremacy-context-adds-extend-once",
                "supremacy-context-already-extended",
                "supremacy-context-release",
                "supremacy-context-duration",
                "ContextDurationValue", "DurationRate.Rounds",
                "DiceType.Zero", "DiceCountValue = 0",
                "timedDuration.Calculate", "ScopedDurationRounds == 10",
                "PreparedDurationRounds == 10",
                "supremacy-context-casting-time", "data.ActionType",
                "supremacy-context-isolation-cleanup",
                "data.CreateExecutionContext", "Metamagic.Extend",
                "ModifiedContextCount" })
                Assertions.True(supremacyScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded Supremacy fixture lacks evidence token: " + token);
            foreach (string token in new[] {
                "disposable-brown-fur-reservoir-accounting",
                "brown-fur-reservoir-accounting.json",
                "reservoir-contract-exact",
                "reservoir-disposable-owner",
                "reservoir-combined-debit-exact",
                "reservoir-restore-exact",
                "reservoir-insufficient-no-debit",
                "reservoir-missing-owner-no-debit",
                "reservoir-cleanup",
                "BrownFurReservoirDebit.TryDebitExact",
                "Resources.Add", "Resources.Remove" })
                Assertions.True(reservoirScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded reservoir fixture lacks evidence token: " + token);
            foreach (string token in new[] {
                "disposable-brown-fur-cast-execution",
                "brown-fur-cast-execution.json",
                "cast-execution-patch-order",
                "cast-execution-reservation-scopes",
                "cast-execution-commit-debit",
                "cast-execution-supremacy-context",
                "cast-execution-rollback-exact",
                "cast-execution-race-rejection",
                "cast-execution-spend-suppression",
                "cast-execution-insufficient-reservation",
                "cast-execution-cleanup", "RuleCastSpell",
                "BrownFurCastExecutionRuntime.Begin",
                "BrownFurCastExecutionRuntime.TryCommit",
                "BrownFurCastExecutionRuntime.RuleFailed",
                "BrownFurCastExecutionRuntime.SuppressedSpendCount",
                "AbilityData).GetMethod(\"Spend\"",
                "Resources.Spend", "Resources.Restore" })
                Assertions.True(castExecutionScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded cast execution fixture lacks evidence token: " +
                    token);
            foreach (string token in new[] {
                "disposable-brown-fur-arcanist-slot",
                "brown-fur-arcanist-slot.json",
                "arcanist-slot-spellbooks", "arcanist-slot-source",
                "arcanist-slot-combined-commit",
                "arcanist-slot-exception-rollback",
                "arcanist-slot-rejected-no-spend",
                "arcanist-slot-cleanup", "ApplyClassMechanics",
                "ApplyLevelup", "contract.CastingSpellbook",
                "contract.MemorizationSpellbook", "InvokeAbilitySpend(data)",
                "BrownFurCastExecutionRuntime.TryCommit",
                "BrownFurCastExecutionRuntime.SuppressedSpendCount" })
                Assertions.True(arcanistSlotScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded Arcanist slot fixture lacks evidence token: " +
                    token);
            foreach (string token in new[] {
                "disposable-brown-fur-bonus-carriers",
                "brown-fur-bonus-carriers.json", "AddStatBonus",
                "AddContextStatBonus", "AddStatBonusAbilityValue",
                "Polymorph", "AddGenericStatBonus+ChangeUnitSize",
                "modifier.Source", "modifier.ModValue",
                "modifier.ModDescriptor", "modifier.SourceComponent",
                "modifier.Source.MaybeContext", "applied.Context",
                "applied.Context.ParentContext",
                "applied.Context.MaybeCaster",
                "applied.Context.SourceAbility",
                "applied.Context.MainTarget.Unit",
                "applied.Context.Params.CasterLevel",
                "applied.IsFromSpell", "applied.Remove()",
                "external-isolation", "WeakerCompetition",
                "EqualCompetition", "StrongerCompetition",
                "OrdinaryToEnhancedValue",
                "EnhancedRetainedAfterRelease",
                "EnhancedToOrdinaryValue", "CapstoneModifierValue",
                "CompetitionFeature", "RemoveExactBuffs" })
                Assertions.True(bonusCarrierScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded bonus carrier fixture lacks evidence token: " + token);
            foreach (string token in new[] {
                "disposable-brown-fur-share-targeting",
                "brown-fur-share-targeting.json", "PersonalSpellGuid",
                "AbilityRange.Personal", "BaselineAnchor",
                "BaselineCanTarget", "TouchRejectsDifferentTarget",
                "TouchApproachMeters", "CapstoneDeltaMeters",
                "ThirtyFeetMeters", "BrownFurShareTargetingRuntime.Begin",
                "BrownFurShareTargetingRuntime.Release",
                "BrownFurShareTargetingRuntime.Clear", "RangeBefore",
                "RangeAfter", "ActiveScopesAfter", "UnitsRemoved" })
                Assertions.True(shareTargetingScenario.Contains(token) ||
                    scenarios.Contains(token) || runtimeCommon.Contains(token),
                    "Guarded Share targeting fixture lacks evidence token: " +
                    token);
            foreach (string token in new[] {
                "Dictionary<string, Scope>", "TransactionIdentity",
                "RootContext", "ParentContext", "MaybeCaster",
                "SourceAbility", "BuffGuids", "CarrierFamilies",
                "SelectedStat", "Tracker.TryAdjust", "modifier.ModValue =",
                "NormalizeGuid", "depth < 24", "ActiveScopeCount",
                "AdjustedModifierCount", "Release", "Clear" })
                Assertions.True(modifierRuntime.Contains(token),
                    "Powerful Change runtime scope lacks guard: " + token);
            foreach (string token in new[] {
                "ModifiableValue", "AddModifier", "HarmonyAfter",
                "CallOfTheWild", "TryAdjust", "catch" })
                Assertions.True(modifierPatch.Contains(token),
                    "Powerful Change modifier patch lacks guard: " + token);
            Assertions.False(modifierRuntime.Contains(
                "static MechanicsContext Current"),
                "Powerful Change must not retain one global current cast.");
            foreach (string token in new[] { "AbilityData", "UnitDescriptor",
                "UnitEntityData", "ActiveScopeCount", "Scopes.Begin",
                "TryOverrideAnchor", "TryOverrideTarget",
                "TryOverrideApproachDistance", "ThirtyFeetMeters",
                "nativeDistance + ThirtyFeetMeters", "Release", "Clear" })
                Assertions.True(shareTargetingRuntime.Contains(token),
                    "Share targeting runtime lacks exact scope guard: " + token);
            foreach (string token in new[] { "get_TargetAnchor", "CanTarget",
                "GetApproachDistance", "Postfix", "HarmonyAfter",
                "CallOfTheWild", "ref AbilityTargetAnchor", "ref bool",
                "ref float", "catch" })
                Assertions.True(shareTargetingPatches.Contains(token),
                    "Share targeting patch lacks exact interoperability guard: " +
                    token);
            foreach (string token in new[] {
                "BrownFurCastCommitCoordinator<UnitDescriptor",
                "BrownFurReservoirDebit.TryDebitExact",
                "BrownFurShareTargetingRuntime.Begin",
                "BrownFurSupremacyRuntime.Begin",
                "BrownFurModifierAdjustmentRuntime.Begin", "RestoreExact",
                "SuppressedSpends", "SuppressedSpendCommands",
                "Coordinator.FailRule", "Coordinator.ProcessTerminal",
                "Coordinator.Clear", "RecordPatchFailure", "LastFailure",
                "command.Result != UnitCommand.ResultType.Success" })
                Assertions.True(castExecutionRuntime.Contains(token),
                    "Cast execution boundary lacks exact guard: " + token);
            foreach (string token in new[] { "RuleCastSpell",
                "MethodType.Constructor", "HarmonyAfter(\"CallOfTheWild\")",
                "TryCommit", "AttachProcess", "Finalizer", "AbilityData",
                "\"Spend\"", "UnitUseAbility", "\"OnEnded\"",
                "AbilityExecutionProcess", "\"Tick\"",
                "RecordPatchFailure" })
                Assertions.True(castExecutionPatches.Contains(token),
                    "Cast execution patch lacks exact lifecycle guard: " +
                    token);
        }

        private static void AssertRejected(IEnumerable<int> levels, string label)
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(levels);
            Assertions.True(!decision.Compatible &&
                decision.Shape == CotwProgressionShape.Unknown &&
                decision.PowerfulChangeReplacementLevel == 0 &&
                decision.ShareTransmutationReplacementLevel == 0,
                label + " must fail closed without replacement levels.");
        }

        private static CotwArcanistContractCandidate Valid(bool balance)
        {
            return new CotwArcanistContractCandidate
            {
                CotwDetected = true,
                CotwActive = true,
                AssemblyIdentityResolved = true,
                ArcanistClassResolved = true,
                ArcanistProgressionResolved = true,
                CastingSpellbookResolved = true,
                MemorizationSpellbookResolved = true,
                ReservoirResolved = true,
                ExploitSelectionResolved = true,
                MagicalSupremacyResolved = true,
                SharedSpellsContractResolved = true,
                ArchetypeArrayResolved = true,
                TransmutationInventoryResolved = true,
                ExploitBearingLevels = balance
                    ? new[] { 1, 4, 7, 10, 13, 16, 19 }
                    : new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 }
            };
        }
    }
}
