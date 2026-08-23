using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.AidAnotherCompatibility;

namespace KingmakerGunslinger.DomainTests
{
    internal static class AidAnotherCompatibilityTests
    {
        internal static void GrantMatrixIsExact()
        {
            AssertGrant(false, false, 0, 2, AidAnotherHelpfulVariant.None);
            AssertGrant(true, false, 0, 3, AidAnotherHelpfulVariant.Combat);
            AssertGrant(false, true, 0, 4, AidAnotherHelpfulVariant.Halfling);
            AssertGrant(true, true, 0, 4, AidAnotherHelpfulVariant.Both);
            AssertGrant(false, false, 1, 3, AidAnotherHelpfulVariant.None);
            AssertGrant(false, false, 2, 4, AidAnotherHelpfulVariant.None);
            AssertGrant(true, false, 2, 5, AidAnotherHelpfulVariant.Combat);
            AssertGrant(false, true, 2, 6,
                AidAnotherHelpfulVariant.Halfling);
            AssertGrant(true, true, 2, 6, AidAnotherHelpfulVariant.Both);
        }

        internal static void GrantValidationFailsClosed()
        {
            Assertions.True(!AidAnotherGrantResolver.Resolve(null).Valid,
                "A null canonical grant request did not fail closed.");
            AidAnotherGrantResolution baseline = Resolve(false, false, 0);
            Assertions.True(baseline.Valid && baseline.FinalGrant >=
                baseline.BaseGrant,
                "A valid grant fell below the normal baseline.");
            var wrongBase = new AidAnotherGrantRequest
            {
                BaseGrant = 3,
                SourceMode = AidAnotherGrantSourceMode.CotwCanonical
            };
            Assertions.True(!AidAnotherGrantResolver.Resolve(wrongBase).Valid,
                "A changed CotW base grant was guessed instead of blocked.");
            var negative = new AidAnotherGrantRequest
            {
                BaseGrant = 2,
                NonHelpfulIncrement = -1,
                SourceMode = AidAnotherGrantSourceMode.CotwCanonical
            };
            Assertions.True(!AidAnotherGrantResolver.Resolve(negative).Valid,
                "A negative canonical increment was accepted.");
            var overflow = new AidAnotherGrantRequest
            {
                BaseGrant = 2,
                CombatHelpfulOwned = true,
                NonHelpfulIncrement = int.MaxValue,
                SourceMode = AidAnotherGrantSourceMode.CotwCanonical
            };
            Assertions.True(!AidAnotherGrantResolver.Resolve(overflow).Valid,
                "An overflowing Aid Another grant was accepted.");
            string evidence = Resolve(true, true, 2).Describe();
            foreach (string token in new[] { "canonicalSourceMode=",
                "baseGrant=2", "helpfulVariant=Both",
                "helpfulIncrement=2", "nonHelpfulIncrement=2",
                "finalSuccessfulGrant=6" })
                Assertions.True(evidence.Contains(token),
                    "Structured grant evidence lacks: " + token);
        }

        internal static void PublicationIsIdempotentAndExact()
        {
            var other = new Identity("other");
            var helpful = new Identity("helpful");
            Identity[] canonical = new[] { other, other };
            Identity[] selection = new[] { other };
            Identity[] canonicalBefore = canonical;
            Identity[] selectionBefore = selection;
            var transaction = new HelpfulPublicationTransaction()
                .Append("canonical", () => canonical, value => canonical = value,
                    helpful, value => value.Id, true)
                .Append("selection", () => selection, value => selection = value,
                    helpful, value => value.Id, false);
            transaction.Commit();
            Assertions.True(canonical.Length == 3 &&
                ReferenceEquals(canonical[0], other) &&
                ReferenceEquals(canonical[1], other) &&
                ReferenceEquals(canonical[2], helpful),
                "Canonical foreign multiplicity/order was not preserved.");
            Assertions.True(selection.Length == 2 &&
                ReferenceEquals(selection[1], helpful),
                "Helpful was not appended once to the selection.");
            Identity[] canonicalAfter = canonical;
            Identity[] selectionAfter = selection;
            transaction.Commit();
            Assertions.True(ReferenceEquals(canonicalAfter, canonical) &&
                ReferenceEquals(selectionAfter, selection),
                "Repeated commit appended Helpful again.");
            transaction.Rollback();
            Assertions.True(ReferenceEquals(canonicalBefore, canonical) &&
                ReferenceEquals(selectionBefore, selection),
                "Successful rollback did not restore exact original arrays.");

            Identity[] laterValues = new[] { other };
            var later = new Identity("later");
            var preserve = new HelpfulPublicationTransaction()
                .Append("preserve-later", () => laterValues,
                    value => laterValues = value, helpful,
                    value => value.Id, false);
            preserve.Commit();
            laterValues = laterValues.Concat(new[] { later }).ToArray();
            preserve.Rollback();
            Assertions.True(laterValues.Length == 2 &&
                ReferenceEquals(laterValues[0], other) &&
                ReferenceEquals(laterValues[1], later),
                "Rollback did not preserve a proven later foreign append.");
        }

        internal static void PublicationFailureRestoresEveryArray()
        {
            var original = new Identity("original");
            var helpful = new Identity("helpful");
            Identity[] first = new[] { original };
            Identity[] second = new[] { original };
            Identity[] firstBefore = first;
            Identity[] secondBefore = second;
            bool failForward = true;
            var transaction = new HelpfulPublicationTransaction()
                .Append("first", () => first, value => first = value, helpful,
                    value => value.Id, false)
                .Append("second", () => second, value =>
                {
                    if (failForward)
                    {
                        failForward = false;
                        throw new InvalidOperationException("fixture");
                    }
                    second = value;
                }, helpful, value => value.Id, false);
            bool failed = false;
            try { transaction.Commit(); }
            catch (InvalidOperationException) { failed = true; }
            Assertions.True(failed && ReferenceEquals(firstBefore, first) &&
                ReferenceEquals(secondBefore, second),
                "Partial failure did not restore every exact original array.");

            var conflict = new HelpfulPublicationTransaction();
            Identity[] values = new[] { new Identity("helpful") };
            conflict.Append("conflict", () => values, value => values = value,
                helpful, value => value.Id, false);
            bool conflictRejected = false;
            try { conflict.Commit(); }
            catch (InvalidOperationException) { conflictRejected = true; }
            Assertions.True(conflictRejected && values.Length == 1 &&
                !ReferenceEquals(values[0], helpful),
                "A same-GUID foreign object conflict was not rejected before mutation.");
        }

        internal static void HelpfulBlueprintAndIdentityAreExact()
        {
            string blueprint = Read("src", "KingmakerGunslinger", "Blueprints",
                "HelpfulCombatBlueprints.cs");
            foreach (string token in new[] { "KMG.Traits.HelpfulCombat",
                "KMG_HelpfulCombat_Trait", "DisplayName = \"Helpful\"",
                "grant your ally a +3 bonus instead of a +2 bonus",
                "feature.Ranks = 1", "feature.HideInUI = false",
                "feature.IsClassFeature = false",
                "FeatureGroup.Trait",
                "feature.ComponentsArray = Array.Empty<BlueprintComponent>()" })
                Assertions.True(blueprint.Contains(token),
                    "Combat Helpful blueprint lacks: " + token);
            string manifest = Read("blueprints", "blueprints.json");
            Assertions.True(manifest.Split(new[] { "KMG.Traits.HelpfulCombat" },
                    StringSplitOptions.None).Length == 2 && manifest.Contains(
                    "e4b29a7c8d5f4c1796ab03e1f72d8456"),
                "Combat Helpful manifest identity is missing or duplicated.");
            Assertions.True(manifest.Split(new[] {
                    "c9bd9f6cc24f41e684a68e6510afc726" },
                    StringSplitOptions.None).Length == 1,
                "KMG duplicated the foreign halfling Helpful identity.");
        }

        internal static void CotwStructuralContractIsExact()
        {
            string resolver = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility", "CotwAidAnotherResolver.cs");
            string patch = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility", "AidAnotherContextRankPatch.cs");
            foreach (string token in new[] { "CallOfTheWild.Rebalance",
                "createAidAnother", "aid_another_config",
                "aid_another_buffs", "ContextRankBaseValueType.FeatureList",
                "ContextRankProgression.BonusValue",
                "91c27d7593614e06a22c0d74106377f6",
                "fd60ba2291144d9a89890dfb1fec561a",
                "ab00871bf2914b3ba492fdb2f1af8875",
                "e24a160d13b549e8a36c219e686ac319",
                "ReferenceEquals(component, configuration)" })
                Assertions.True(resolver.Contains(token),
                    "CotW exact contract lacks: " + token);
            Assertions.True(patch.Contains(
                    "AidAnotherGrantRuntime.TryOverrideCanonical") &&
                patch.Contains("HarmonyAfter(CotwAidAnotherResolver.ModId)") &&
                patch.Contains("typeof(ContextRankConfig), \"GetValue\"") &&
                !patch.Contains("TargetMethods"),
                "Ordinary Aid Another correction is not exact-instance gated.");
            string runtime = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility", "AidAnotherGrantRuntime.cs");
            Assertions.True(runtime.Contains(
                    "if (_canonicalFailureReported) return false") &&
                runtime.Contains("_canonicalFailureReported = true"),
                "A broken canonical contract can spam per-calculation failures.");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.True(!project.Contains(
                    "<Reference Include=\"CallOfTheWild\"") &&
                !project.Contains("<Reference Include=\"ZFavoredClass\""),
                "An optional mod became a compile-time dependency.");
        }

        internal static void FavoredClassSourceContractIsFailClosed()
        {
            string resolver = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility", "FavoredClassTraitResolver.cs");
            foreach (string token in new[] { "ZFavoredClass",
                "ZFavoredClass.Traits", "load", "enable_traits",
                "43d763957f364315b5fff85f9e91ca51",
                "331ed3c4a988415785f71a37b826d0f1",
                "c9bd9f6cc24f41e684a68e6510afc726",
                "b0c3ef2729c498f47970bb50fa1acd30",
                "ZFavoredClass.NewMechanics.PrerequisiteRace",
                "CallOfTheWild.EvolutionMechanics.addSelection",
                "adopted.IgnorePrerequisites" })
                Assertions.True(resolver.Contains(token),
                    "Favored Class source-backed contract lacks: " + token);
            string investigation = Read("docs", "investigations",
                "aid-another-cotw-favored-class.md");
            Assertions.True(investigation.Contains(
                    "No `ZFavoredClass.dll`") && investigation.Contains(
                    "awaiting exact installed-binary and runtime qualification") &&
                investigation.Contains(
                    "56ec6c5fd34f0da037350f951383ca7f1a0c5e57"),
                "The missing Favored Class binary boundary is not recorded honestly.");
        }

        internal static void LifecycleAndStatusRemainOptional()
        {
            string coordinator = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility",
                "AidAnotherOptionalExtensionCoordinator.cs");
            foreach (string token in new[] { "FirstUpdate",
                "AfterCotwAidAnotherCreation", "AfterFavoredTraitsLoad",
                "_reconciling", "publication.idempotent",
                "MaximumPendingUpdateRetries = 2",
                "pending-contract-timeout:",
                "cotw-aid-another-feature-list",
                "favored-combat-features",
                "favored-combat-all-features", "PrerequisiteNoFeature",
                "context.FeatureModules.Active.BodyguardFeats",
                "AidAnotherGrantRuntime.Configure(null, null)",
                "unrelated KMG modules remain active" })
                Assertions.True(coordinator.Contains(token),
                    "Optional lifecycle coordinator lacks: " + token);
            var absent = new AidAnotherCompatibilityStatus(
                OptionalAidAnotherAvailability.Absent,
                OptionalAidAnotherAvailability.Absent, null, false, false,
                "fixture");
            Assertions.True(absent.CotwStatus.Contains("Absent") &&
                absent.FavoredClassStatus.Contains("Absent") &&
                !absent.HelpfulPublished,
                "Absent optional mods are not represented safely.");
            var disabled = new AidAnotherCompatibilityStatus(
                OptionalAidAnotherAvailability.Compatible,
                OptionalAidAnotherAvailability.Compatible, false, true, false,
                "fixture");
            Assertions.True(disabled.FavoredClassStatus.Contains(
                    "traits disabled") && disabled.PublicationStatus.Contains(
                    "not published"),
                "Traits-disabled compatibility status is ambiguous.");
        }

        internal static void GuardedRuntimeContractIsExact()
        {
            string observer = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "AidAnotherCompatibilityObserver.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            foreach (string token in new[] {
                "aid-another-compatibility-contracts.json",
                "aid-another-standalone-isolation",
                "aid-another-cotw-compatible",
                "aid-another-consumers-shared",
                "aid-another-contributor-multiplicity",
                "aid-another-harmony-gate",
                "aid-another-live-values",
                "combat-helpful-plus-benevolent",
                "dual-helpful-plus-benevolent",
                "AttackTypeAttackBonus and ACBonusAgainstAttacks",
                "attackBuff.Context.RecalculateRanks()",
                "armorBuff.Context.RecalculateRanks()",
                "fixture.Cleaned" })
                Assertions.True(observer.Contains(token),
                    "Guarded Aid Another observer lacks: " + token);
            Assertions.True(catalog.Contains(
                    "observe-aid-another-compatibility-contracts") &&
                catalog.Contains("disposable-helpful-bodyguard") &&
                runner.Contains("AidAnotherCompatibilityObserver.Run") &&
                runner.Contains("DisposableHelpfulBodyguard"),
                "Aid Another guarded scenarios are not dispatched exactly.");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.True(project.Contains(
                    "RuntimeTesting\\AidAnotherCompatibilityObserver.cs"),
                "The old-style main project does not compile the observer.");
        }

        private static AidAnotherGrantResolution Resolve(bool combat,
            bool halfling, int unrelated)
        {
            return AidAnotherGrantResolver.Resolve(new AidAnotherGrantRequest
            {
                BaseGrant = 2,
                CombatHelpfulOwned = combat,
                HalflingHelpfulOwned = halfling,
                NonHelpfulIncrement = unrelated,
                SourceMode = AidAnotherGrantSourceMode.CotwCanonical
            });
        }

        private static void AssertGrant(bool combat, bool halfling,
            int unrelated, int expected, AidAnotherHelpfulVariant variant)
        {
            AidAnotherGrantResolution resolution = Resolve(combat, halfling,
                unrelated);
            Assertions.True(resolution.Valid &&
                resolution.FinalGrant == expected &&
                resolution.HelpfulVariant == variant &&
                resolution.NonHelpfulIncrement == unrelated,
                "Aid Another grant matrix changed for combat=" + combat +
                ", halfling=" + halfling + ", unrelated=" + unrelated + ".");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        private sealed class Identity
        {
            internal Identity(string id) { Id = id; }
            internal string Id { get; private set; }
        }
    }
}
