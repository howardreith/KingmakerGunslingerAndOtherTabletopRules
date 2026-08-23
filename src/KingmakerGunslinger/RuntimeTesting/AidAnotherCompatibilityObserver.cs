using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded, save-free observation of the optional CotW/Favored Class Aid
    /// Another contract. When CotW is present it evaluates both real Aid Another
    /// buffs and their one shared ContextRankConfig against disposable units.
    /// </summary>
    internal static class AidAnotherCompatibilityObserver
    {
        private const string EvidenceFileName =
            "aid-another-compatibility-contracts.json";
        private const string HelpfulCombatGuid =
            "e4b29a7c8d5f4c1796ab03e1f72d8456";

        private sealed class GrantEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("rank", Order = 2)] public int Rank { get; set; }
            [JsonProperty("attackBonus", Order = 3)]
            public int AttackBonus { get; set; }
            [JsonProperty("armorClass", Order = 4)]
            public int ArmorClass { get; set; }
        }

        private sealed class AttackEvidence
        {
            internal int AttackBonus { get; set; }
            internal int ArmorClass { get; set; }
        }

        private sealed class Evidence
        {
            [JsonProperty("cotwStatus", Order = 1)]
            public string CotwStatus { get; set; }
            [JsonProperty("favoredClassStatus", Order = 2)]
            public string FavoredClassStatus { get; set; }
            [JsonProperty("publicationStatus", Order = 3)]
            public string PublicationStatus { get; set; }
            [JsonProperty("detail", Order = 4)] public string Detail { get; set; }
            [JsonProperty("cotwFingerprint", Order = 5)]
            public string CotwFingerprint { get; set; }
            [JsonProperty("favoredClassFingerprint", Order = 6)]
            public string FavoredClassFingerprint { get; set; }
            [JsonProperty("contributors", Order = 7)]
            public string[] Contributors { get; set; }
            [JsonProperty("consumers", Order = 8)]
            public string[] Consumers { get; set; }
            [JsonProperty("publicationEvidence", Order = 9)]
            public string[] PublicationEvidence { get; set; }
            [JsonProperty("grantValues", Order = 10)]
            public GrantEvidence[] GrantValues { get; set; }
            [JsonProperty("harmonyContract", Order = 11)]
            public string HarmonyContract { get; set; }
            [JsonProperty("fixtureCleaned", Order = 12)]
            public bool FixtureCleaned { get; set; }
            [JsonProperty("easternLatePublication", Order = 13)]
            public string EasternLatePublication { get; set; }
            [JsonProperty("equipmentTraits", Order = 14)]
            public string EquipmentTraits { get; set; }
            [JsonProperty("tweakOrTreatFingerprint", Order = 15)]
            public string TweakOrTreatFingerprint { get; set; }
            [JsonProperty("heirloomNodachiPublication", Order = 16)]
            public string HeirloomNodachiPublication { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var warnings = new List<string>();
            AidAnotherCompatibilityStatus status =
                AidAnotherCompatibilityStatusRegistry.Current;
            CotwAidAnotherContract cotw =
                AidAnotherOptionalExtensionCoordinator.CotwContract;
            FavoredClassTraitContract favored =
                AidAnotherOptionalExtensionCoordinator.FavoredClassContract;
            BodyguardFeatBlueprintSet bodyguard =
                BlueprintBootstrap.BodyguardFeats;
            bool cotwLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(
                value => string.Equals(value.GetName().Name,
                    CotwAidAnotherResolver.AssemblyName,
                    StringComparison.Ordinal));
            bool favoredLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(
                value => string.Equals(value.GetName().Name,
                    FavoredClassTraitResolver.AssemblyName,
                    StringComparison.Ordinal));
            bool tweakLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(
                value => string.Equals(value.GetName().Name,
                    TweakOrTreatHeirloomResolver.AssemblyName,
                    StringComparison.Ordinal));
            EasternWeaponMartialPublication martial =
                EasternWeaponLatePublicationCoordinator.Publication;
            TweakOrTreatHeirloomContract tweak =
                EasternWeaponLatePublicationCoordinator.TweakOrTreatContract;
            EasternWeaponBlueprintSet eastern =
                BlueprintBootstrap.EasternWeapons;
            int nodachiNative = BlueprintBootstrap.Library == null ? -1 :
                EasternWeaponMartialPublication.CountNodachiOnNative(
                    BlueprintBootstrap.Library);

            var evidence = new Evidence {
                CotwStatus = status.CotwStatus,
                FavoredClassStatus = status.FavoredClassStatus,
                PublicationStatus = status.PublicationStatus,
                Detail = status.Detail,
                CotwFingerprint = cotw == null ? string.Empty :
                    cotw.Fingerprint,
                FavoredClassFingerprint = favored == null ? string.Empty :
                    favored.Fingerprint,
                Contributors = DescribeContributors(cotw),
                Consumers = DescribeConsumers(cotw),
                PublicationEvidence = AidAnotherOptionalExtensionCoordinator
                    .PublicationEvidence,
                GrantValues = new GrantEvidence[0],
                HarmonyContract = DescribeHarmony(context, cotw),
                FixtureCleaned = true,
                EasternLatePublication = "attempted=" +
                    EasternWeaponLatePublicationCoordinator.Attempted +
                    ";published=" +
                    EasternWeaponLatePublicationCoordinator.Published +
                    ";earlyNodachi=" +
                    EasternWeaponLatePublicationCoordinator.EarlyNodachiCount +
                    ";nativeNodachi=" + nodachiNative + ";broadFacts=" +
                    (martial == null ? 0 : martial.BroadFacts.Length) +
                    ";failedCheck=" +
                    EasternWeaponLatePublicationCoordinator.FailedCheck,
                EquipmentTraits = DescribeEquipment(favored),
                TweakOrTreatFingerprint = tweak == null ? string.Empty :
                    tweak.Fingerprint,
                HeirloomNodachiPublication = "published=" +
                    EasternWeaponLatePublicationCoordinator.HeirloomPublished +
                    ";count=" + CountHeirloom(favored, eastern) +
                    ";failedCheck=" +
                    EasternWeaponLatePublicationCoordinator
                        .HeirloomFailedCheck
            };

            Add(assertions, "eastern-late-martial-publication",
                "first-update publication observed zero early Nodachi and leaves exactly one on every broad martial fact",
                evidence.EasternLatePublication,
                EasternWeaponLatePublicationCoordinator.Attempted &&
                    EasternWeaponLatePublicationCoordinator.Published &&
                    EasternWeaponLatePublicationCoordinator.EarlyNodachiCount ==
                        0 && nodachiNative == 1 && martial != null &&
                    martial.BroadFacts.Length > 0 &&
                    string.IsNullOrEmpty(
                        EasternWeaponLatePublicationCoordinator.FailedCheck),
                "live native AddProficiencies arrays and late publication transaction");

            Add(assertions, "aid-another-kmg-identity",
                "registered KMG combat Helpful identity",
                bodyguard == null ? "missing" : bodyguard.HelpfulCombat.AssetGuid,
                bodyguard != null && string.Equals(
                    bodyguard.HelpfulCombat.AssetGuid,
                    HelpfulCombatGuid,
                    StringComparison.Ordinal),
                "stable KMG blueprint registration is independent of optional publication");

            if (cotw == null)
            {
                string observed = "cotwLoaded=" + cotwLoaded +
                    ";favoredLoaded=" + favoredLoaded + ";" +
                    status.CotwStatus + ";" + status.FavoredClassStatus +
                    ";" + status.PublicationStatus;
                Add(assertions, "aid-another-standalone-isolation",
                    "both optional assemblies absent and standalone +2 active",
                    observed, !cotwLoaded && !favoredLoaded &&
                    status.Cotw == OptionalAidAnotherAvailability.Absent &&
                    status.FavoredClass == OptionalAidAnotherAvailability.Absent &&
                    !status.OrdinaryAidIntegrated && !status.HelpfulPublished &&
                    AidAnotherGrantResolver.Standalone(false).FinalGrant == 2,
                    "live AppDomain, compatibility status, and shared grant resolver");
                Add(assertions, "aid-another-no-foreign-mutation",
                    "no foreign publication transaction",
                    string.Join("|", evidence.PublicationEvidence),
                    evidence.PublicationEvidence.Length == 0,
                    "coordinator-owned publication evidence");
            }
            else
            {
                ObserveCompatible(context, cotw, favored, bodyguard, evidence,
                    assertions, diagnostics);
            }

            if (favored != null)
            {
                bool shouldPublish = favored.TraitsEnabled &&
                    context.FeatureModules.Active.EasternWeapons;
                Add(assertions, "favored-full-trait-construction",
                    "Combat, Race, Equipment, two top-level Trait choices, Additional Traits, and halfling Helpful are fully constructed",
                    evidence.EquipmentTraits,
                    favored.EquipmentTraits.AllFeatures != null &&
                    favored.EquipmentTraits.AllFeatures.Length >= 20 &&
                    favored.FirstTrait.AllFeatures.Count(value =>
                        ReferenceEquals(value, favored.EquipmentTraits)) == 1 &&
                    favored.SecondTrait.AllFeatures.Count(value =>
                        ReferenceEquals(value, favored.EquipmentTraits)) == 1 &&
                    favored.RaceTraits.AllFeatures.Count(value =>
                        ReferenceEquals(value,
                            favored.HalflingHelpful)) == 1,
                    "exact installed ZFavoredClass static blueprint graph");
                Add(assertions, "heirloom-nodachi-publication",
                    shouldPublish ?
                        "one KMG Nodachi choice in Equipment Traits" :
                        "no new Nodachi choice while traits/module publication is disabled",
                    evidence.HeirloomNodachiPublication,
                    EasternWeaponLatePublicationCoordinator
                        .HeirloomPublished == shouldPublish &&
                    CountHeirloom(favored, eastern) ==
                        (shouldPublish ? 1 : 0),
                    "live foreign Equipment Trait array by reference and AssetGuid");
                Add(assertions, "tweak-or-treat-heirloom-contract",
                    tweakLoaded ?
                        "Tweak or Treat Heirloom reconciliation completed before KMG append" :
                        "Tweak or Treat absent without blocking Favored/KMG",
                    "loaded=" + tweakLoaded + ";" +
                        evidence.TweakOrTreatFingerprint,
                    tweakLoaded ? tweak != null &&
                        tweak.TransformedRacialChoices == 5 : tweak == null,
                    "live UMM assembly and transformed Favored Equipment Trait prerequisites");
            }

            Add(assertions, "aid-another-save-free",
                "no save, input, or persistent character mutation",
                "request-local disposable units only;fixtureCleaned=" +
                    evidence.FixtureCleaned,
                evidence.FixtureCleaned,
                "fixture restores the exact global unit snapshot and never invokes save/input APIs");

            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            diagnostics.Add("compatibilityEvidenceSha256=" + Hash(path));
            bool pass = assertions.All(value => value.Status ==
                RuntimeTestStatuses.Pass);
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";mvid=" +
                    context.Assembly.ManifestModule.ModuleVersionId +
                    ";sha256=" + Hash(context.Assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = warnings, ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string> { path },
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void ObserveCompatible(ModContext context,
            CotwAidAnotherContract cotw, FavoredClassTraitContract favored,
            BodyguardFeatBlueprintSet bodyguard, Evidence evidence,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            BlueprintFeature[] contributors = cotw.ReadFeatureList();
            int combatMultiplicity = Count(contributors,
                bodyguard.HelpfulCombat);
            int halflingMultiplicity = contributors.Count(value => value !=
                null && string.Equals(value.AssetGuid,
                    FavoredClassTraitResolver.HalflingHelpfulGuid,
                    StringComparison.Ordinal));
            BlueprintFeature benevolent = contributors.FirstOrDefault(value =>
                value != null && string.Equals(value.AssetGuid,
                    CotwAidAnotherResolver.BenevolentFeatureGuid,
                    StringComparison.Ordinal));
            int benevolentMultiplicity = Count(contributors, benevolent);

            Add(assertions, "aid-another-cotw-compatible",
                "exact CotW shared FeatureList/BonusValue/step-2 contract",
                cotw.Fingerprint,
                statusCompatible() && statusIntegrated() &&
                    ReferenceEquals(AidAnotherGrantRuntime.CanonicalContract,
                        cotw),
                "live coordinator resolution and exact-reference runtime binding");
            Add(assertions, "aid-another-consumers-shared",
                "attack and AC buffs each reference the same config exactly once",
                string.Join("|", evidence.Consumers),
                cotw.Buffs.Length == 2 && cotw.Buffs.All(value =>
                    value.ComponentsArray.Count(component => ReferenceEquals(
                        component, cotw.Configuration)) == 1),
                "live CotW blueprint component arrays");
            Add(assertions, "aid-another-contributor-multiplicity",
                "KMG Helpful once; Benevolent twice; halfling Helpful absent or twice",
                "combat=" + combatMultiplicity + ";benevolent=" +
                    benevolentMultiplicity + ";halfling=" +
                    halflingMultiplicity + ";all=" + string.Join("|",
                        evidence.Contributors),
                combatMultiplicity == 1 && benevolent != null &&
                    benevolentMultiplicity == 2 &&
                    (favored == null && halflingMultiplicity == 0 ||
                     favored != null && halflingMultiplicity == 2),
                "canonical feature list preserves exact reference multiplicity");
            Add(assertions, "aid-another-harmony-gate",
                "one KMG postfix on ContextRankConfig.GetValue(MechanicsContext)",
                evidence.HarmonyContract,
                evidence.HarmonyContract.Contains("postfixes=1") &&
                    evidence.HarmonyContract.Contains(
                        typeof(AidAnotherContextRankPatch).FullName),
                "Harmony 1.2 live patch registry and exact contract identity");

            var grants = new List<GrantEvidence>();
            BodyguardCombatFixture fixture = null;
            bool cleaned = false;
            try
            {
                fixture = new BodyguardCombatFixture();
                fixture.ClearModes();
                UnitEntityData helper = fixture.ProtectorOne;
                foreach (BlueprintFeature contributor in contributors
                    .Distinct().ToArray())
                    fixture.SetAidContributor(helper, contributor, false);

                grants.Add(Measure(cotw, fixture, helper, "base", 2));
                fixture.SetCombatHelpful(helper, true);
                grants.Add(Measure(cotw, fixture, helper,
                    "combat-helpful", 3));
                fixture.SetAidContributor(helper, benevolent, true);
                grants.Add(Measure(cotw, fixture, helper,
                    "combat-helpful-plus-benevolent", 5));
                fixture.SetCombatHelpful(helper, false);
                grants.Add(Measure(cotw, fixture, helper,
                    "benevolent", 4));
                fixture.SetAidContributor(helper, benevolent, false);

                if (favored != null)
                {
                    fixture.SetAidContributor(helper,
                        favored.HalflingHelpful, true);
                    grants.Add(Measure(cotw, fixture, helper,
                        "halfling-helpful", 4));
                    fixture.SetCombatHelpful(helper, true);
                    grants.Add(Measure(cotw, fixture, helper,
                        "dual-helpful", 4));
                    fixture.SetAidContributor(helper, benevolent, true);
                    grants.Add(Measure(cotw, fixture, helper,
                        "dual-helpful-plus-benevolent", 6));
                }
            }
            finally
            {
                if (fixture != null)
                {
                    fixture.Dispose();
                    cleaned = fixture.Cleaned;
                }
                evidence.FixtureCleaned = cleaned;
            }
            evidence.GrantValues = grants.ToArray();
            string observed = string.Join("|", grants.Select(value =>
                value.Name + "=" + value.Rank + "/" + value.AttackBonus +
                    "/" + value.ArmorClass).ToArray());
            Add(assertions, "aid-another-live-values",
                "each shared rank, attack buff, and AC buff equals its expected grant",
                observed,
                grants.Count >= 4 && grants.All(value => value.Rank ==
                    value.AttackBonus && value.Rank == value.ArmorClass),
                "actual CotW ContextRankConfig plus AttackTypeAttackBonus and ACBonusAgainstAttacks rule events");
            Add(assertions, "aid-another-publication-state",
                favored != null && favored.TraitsEnabled &&
                    context.FeatureModules.Active.BodyguardFeats ?
                    "combat Helpful published once" :
                    "combat Helpful not offered; canonical existing-owner integration retained",
                statusText(),
                favored == null ? !currentStatus().HelpfulPublished :
                    currentStatus().HelpfulPublished ==
                        (favored.TraitsEnabled && context.FeatureModules.Active
                            .BodyguardFeats),
                "live Favored Class selection state and compatibility status");
            diagnostics.Add(cotw.Fingerprint);
            if (favored != null) diagnostics.Add(favored.Fingerprint);

            bool statusCompatible()
            { return currentStatus().Cotw ==
                OptionalAidAnotherAvailability.Compatible; }
            bool statusIntegrated()
            { return currentStatus().OrdinaryAidIntegrated; }
            AidAnotherCompatibilityStatus currentStatus()
            { return AidAnotherCompatibilityStatusRegistry.Current; }
            string statusText()
            { return currentStatus().PublicationStatus + ";" +
                currentStatus().Detail; }
        }

        private static GrantEvidence Measure(CotwAidAnotherContract cotw,
            BodyguardCombatFixture fixture, UnitEntityData helper, string name,
            int expected)
        {
            var context = new MechanicsContext(helper, helper.Descriptor,
                cotw.OrdinaryAbility, null, new TargetWrapper(
                    fixture.ProtectorTwo));
            int rank = cotw.Configuration.GetValue(context);
            ItemEntityWeapon weapon = fixture.ProtectorTwo.Body.PrimaryHand
                .Weapon;
            AttackEvidence before = ResolveMeleeAttack(fixture.ProtectorTwo,
                fixture.Target, weapon);
            Buff attackBuff = fixture.ProtectorTwo.Descriptor.Buffs.AddBuff(
                cotw.Buffs[0], context, null);
            if (attackBuff == null) throw new InvalidOperationException(
                "CotW attack Aid Another buff was rejected.");

            var armorContext = new MechanicsContext(helper, helper.Descriptor,
                cotw.OrdinaryAbility, null, new TargetWrapper(fixture.Target));
            Buff armorBuff = fixture.Target.Descriptor.Buffs.AddBuff(
                cotw.Buffs[1], armorContext, null);
            if (armorBuff == null) throw new InvalidOperationException(
                "CotW AC Aid Another buff was rejected.");
            AttackEvidence after;
            try
            {
                after = ResolveMeleeAttack(fixture.ProtectorTwo,
                    fixture.Target, weapon);
            }
            finally
            {
                if (fixture.ProtectorTwo.Descriptor.Buffs.RawFacts.Contains(
                    attackBuff)) attackBuff.Remove();
                if (fixture.Target.Descriptor.Buffs.RawFacts.Contains(
                    armorBuff)) armorBuff.Remove();
            }
            int attackGrant = after.AttackBonus - before.AttackBonus;
            int armorGrant = after.ArmorClass - before.ArmorClass;
            if (rank != expected || attackGrant != expected ||
                armorGrant != expected)
                throw new InvalidOperationException("CotW Aid Another case '" +
                    name + "' expected " + expected + " but observed rank=" +
                    rank + ", attack=" + attackGrant + ", AC=" + armorGrant +
                    ".");
            return new GrantEvidence { Name = name, Rank = rank,
                AttackBonus = attackGrant, ArmorClass = armorGrant };
        }

        private static AttackEvidence ResolveMeleeAttack(
            UnitEntityData initiator, UnitEntityData target,
            ItemEntityWeapon weapon)
        {
            // RuleAttackWithWeapon.AttackBonusPenalty is a positive
            // subtraction in Kingmaker 2.1.7b.
            var attack = new RuleAttackWithWeapon(initiator, target, weapon,
                1000);
            string control;
            BodyguardQualificationControl.Arm(2);
            try { Rulebook.Trigger(attack); }
            finally { control = BodyguardQualificationControl
                .DescribeAndClear(); }
            if (attack.AttackRoll == null || !control.Contains(
                    "incomingConsumed=1") || attack.AttackRoll.IsHit)
                throw new InvalidOperationException(
                    "CotW Aid Another probe did not resolve as one forced-miss " +
                    "native melee attack: " + control + ".");
            return new AttackEvidence {
                AttackBonus = attack.AttackRoll.AttackBonus,
                ArmorClass = attack.AttackRoll.TargetAC
            };
        }

        private static int Count(IEnumerable<BlueprintFeature> values,
            BlueprintFeature feature)
        {
            if (values == null || feature == null) return 0;
            return values.Count(value => ReferenceEquals(value, feature) ||
                value != null && string.Equals(value.AssetGuid,
                    feature.AssetGuid, StringComparison.Ordinal));
        }

        private static string[] DescribeContributors(
            CotwAidAnotherContract contract)
        {
            BlueprintFeature[] values = contract == null ? null :
                contract.ReadFeatureList();
            return (values ?? new BlueprintFeature[0]).Select((value, index) =>
                index + ":" + value.AssetGuid + ":" + value.name).ToArray();
        }

        private static string[] DescribeConsumers(CotwAidAnotherContract contract)
        {
            return contract == null ? new string[0] : contract.Buffs.Select(
                value => value.AssetGuid + ":" + value.name + ":configRefs=" +
                    value.ComponentsArray.Count(component => ReferenceEquals(
                        component, contract.Configuration))).ToArray();
        }

        private static string DescribeHarmony(ModContext context,
            CotwAidAnotherContract contract)
        {
            MethodInfo method = typeof(ContextRankConfig).GetMethod("GetValue",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic, null, new[] { typeof(MechanicsContext) },
                null);
            if (method == null) return "target=<missing>";
            Patches patches = context.Harmony.GetPatchInfo(method);
            Patch[] owned = patches == null ? new Patch[0] : patches.Postfixes
                .Where(value => value.patch != null && value.patch.DeclaringType ==
                    typeof(AidAnotherContextRankPatch)).ToArray();
            return "target=" + method.DeclaringType.FullName + "." +
                method.Name + ";postfixes=" + owned.Length + ";owner=" +
                (owned.Length == 0 ? "<missing>" :
                    owned[0].patch.DeclaringType.FullName) +
                ";exactConfig=" + (contract != null && ReferenceEquals(
                    AidAnotherGrantRuntime.CanonicalContract, contract));
        }

        private static string DescribeEquipment(
            FavoredClassTraitContract favored)
        {
            if (favored == null) return "favored=<absent>";
            return "equipmentGuid=" + favored.EquipmentTraits.AssetGuid +
                ";equipmentChoices=" +
                (favored.EquipmentTraits.AllFeatures == null ? 0 :
                    favored.EquipmentTraits.AllFeatures.Length) +
                ";top1Equipment=" + favored.FirstTrait.AllFeatures.Count(
                    value => ReferenceEquals(value,
                        favored.EquipmentTraits)) + ";top2Equipment=" +
                favored.SecondTrait.AllFeatures.Count(value =>
                    ReferenceEquals(value, favored.EquipmentTraits)) +
                ";combatChoices=" + (favored.CombatTraits.AllFeatures == null ?
                    0 : favored.CombatTraits.AllFeatures.Length) +
                ";raceChoices=" + (favored.RaceTraits.AllFeatures == null ? 0 :
                    favored.RaceTraits.AllFeatures.Length) +
                ";traitsEnabled=" + favored.TraitsEnabled;
        }

        private static int CountHeirloom(FavoredClassTraitContract favored,
            EasternWeaponBlueprintSet eastern)
        {
            if (favored == null || eastern == null ||
                eastern.HeirloomNodachi == null ||
                favored.EquipmentTraits.AllFeatures == null) return 0;
            return favored.EquipmentTraits.AllFeatures.Count(value =>
                ReferenceEquals(value,
                    eastern.HeirloomNodachi.Selection) || value != null &&
                string.Equals(value.AssetGuid,
                    eastern.HeirloomNodachi.Selection.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string source)
        {
            assertions.Add(new RuntimeTestAssertion { Name = id,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = source });
        }
    }
}
