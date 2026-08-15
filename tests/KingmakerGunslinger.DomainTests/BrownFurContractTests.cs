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
