using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only, guarded proof that the installed 2.1.7b engine still exposes
    /// every native contract used by Bodyguard and In Harm's Way.
    /// </summary>
    internal static class BodyguardNativeContractObserver
    {
        private const string EvidenceFileName =
            "bodyguard-native-contracts.json";
        private const string ExpectedSupportedGameVersion = "2.1.7b";
        private const string ExpectedAssemblySha256 =
            "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb";
        private const string ExpectedAssemblyMvid =
            "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        private const BindingFlags ExactInstance = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private sealed class Evidence
        {
            [JsonProperty("gameVersion", Order = 1)]
            public string GameVersion { get; set; }
            [JsonProperty("unityApplicationVersion", Order = 2)]
            public string UnityApplicationVersion { get; set; }
            [JsonProperty("gameVersionSource", Order = 3)]
            public string GameVersionSource { get; set; }
            [JsonProperty("assemblyPath", Order = 4)]
            public string AssemblyPath { get; set; }
            [JsonProperty("assemblySha256", Order = 5)]
            public string AssemblySha256 { get; set; }
            [JsonProperty("assemblyMvid", Order = 6)]
            public string AssemblyMvid { get; set; }
            [JsonProperty("combatReflexes", Order = 7)]
            public string CombatReflexes { get; set; }
            [JsonProperty("aooContract", Order = 8)]
            public List<string> AooContract { get; set; }
            [JsonProperty("swiftContract", Order = 9)]
            public List<string> SwiftContract { get; set; }
            [JsonProperty("turnContract", Order = 10)]
            public List<string> TurnContract { get; set; }
            [JsonProperty("flatFootedContract", Order = 11)]
            public List<string> FlatFootedContract { get; set; }
            [JsonProperty("threatContract", Order = 12)]
            public List<string> ThreatContract { get; set; }
            [JsonProperty("aidContract", Order = 13)]
            public List<string> AidContract { get; set; }
            [JsonProperty("acBreakdownContract", Order = 14)]
            public List<string> AcBreakdownContract { get; set; }
            [JsonProperty("deliveryContract", Order = 15)]
            public string DeliveryContract { get; set; }
            [JsonProperty("harmonyTargets", Order = 16)]
            public List<string> HarmonyTargets { get; set; }
            [JsonProperty("publication", Order = 17)]
            public List<string> Publication { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            Assembly gameAssembly = typeof(RuleAttackRoll).Assembly;
            string gamePath = Path.GetFullPath(gameAssembly.Location);
            string gameHash = Hash(gamePath);
            string gameMvid = gameAssembly.ManifestModule.ModuleVersionId
                .ToString("D");
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            BlueprintFeature combatReflexes = set == null ? null :
                set.CombatReflexes;

            MethodInfo attackOfOpportunity = typeof(UnitCombatState).GetMethod(
                "AttackOfOpportunity", ExactInstance, null,
                new[] { typeof(UnitEntityData), typeof(bool) }, null);
            PropertyInfo aooCount = typeof(UnitCombatState).GetProperty(
                "AttackOfOpportunityCount", ExactInstance);
            PropertyInfo canAoo = typeof(UnitCombatState).GetProperty(
                "CanAttackOfOpportunity", ExactInstance);
            PropertyInfo swift = typeof(UnitCombatState.Cooldowns).GetProperty(
                "SwiftAction", ExactInstance);
            MethodInfo hasSwift = typeof(UnitEntityData).GetMethod(
                "HasSwiftAction", BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                Type.EmptyTypes, null);
            MethodInfo cooldownClear = typeof(UnitCombatState.Cooldowns)
                .GetMethod("Clear", ExactInstance, null, Type.EmptyTypes,
                    null);
            MethodInfo turnPrepare = typeof(TurnController).GetMethod(
                "Prepare", ExactInstance, null, Type.EmptyTypes, null);
            MethodInfo turnDispose = typeof(TurnController).GetMethod(
                "Dispose", ExactInstance, null, Type.EmptyTypes, null);
            MethodInfo forceToEnd = typeof(TurnController).GetMethod(
                "ForceToEnd", ExactInstance, null, new[] { typeof(bool) },
                null);
            MethodInfo delay = typeof(TurnController).GetMethod(
                "DelayInitiaive", ExactInstance, null,
                new[] { typeof(UnitEntityData) }, null);
            ConstructorInfo flatFootedConstructor = typeof(
                RuleCheckTargetFlatFooted).GetConstructor(new[] {
                    typeof(UnitEntityData), typeof(UnitEntityData) });
            MethodInfo flatFootedTrigger = typeof(RuleCheckTargetFlatFooted)
                .GetMethod("OnTrigger", ExactInstance, null,
                    new[] { typeof(RulebookEventContext) }, null);
            MethodInfo isReach = typeof(UnitEngagementExtension).GetMethod(
                "IsReach", BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic, null, new[] { typeof(UnitEntityData),
                    typeof(UnitEntityData), typeof(WeaponSlot) }, null);
            MethodInfo getThreatHand = typeof(UnitEngagementExtension)
                .GetMethods(BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic).SingleOrDefault(value =>
                        value.Name == "GetThreatHand" &&
                        value.ReturnType == typeof(WeaponSlot));
            MethodInfo applyEffect = InHarmsWayDeliveryAccess
                .AbilityApplyEffectTarget as MethodInfo;
            FieldInfo acBonusSources = typeof(RuleCalculateAC).GetField(
                "BonusSources", ExactInstance);
            ConstructorInfo bonusSourceConstructor = typeof(BonusSource)
                .GetConstructor(new[] { typeof(int), typeof(Fact) });
            Type attackLogMessage = gameAssembly.GetType(
                "Kingmaker.Blueprints.Root.Strings.GameLog.AttackLogMessage",
                false);
            MethodInfo appendArmorClassBreakdown = attackLogMessage == null ?
                null : attackLogMessage.GetMethod("AppendArmorClassBreakdown",
                    ExactInstance, null, new[] { typeof(StringBuilder),
                        typeof(RuleCalculateAC) }, null);
            Type statBreakdown = gameAssembly.GetType(
                "Kingmaker.UI.Common.StatModifiersBreakdown", false);
            MethodInfo addBonusSources = statBreakdown == null ? null :
                statBreakdown.GetMethod("AddBonusSources",
                    BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null,
                    new[] { typeof(List<BonusSource>) }, null);

            var evidence = new Evidence {
                GameVersion = ExpectedSupportedGameVersion,
                UnityApplicationVersion = Application.version ?? string.Empty,
                GameVersionSource = "exact Assembly-CSharp SHA-256 and MVID contract",
                AssemblyPath = gamePath,
                AssemblySha256 = gameHash,
                AssemblyMvid = gameMvid,
                CombatReflexes = DescribeCombatReflexes(combatReflexes),
                AooContract = DescribeMembers(attackOfOpportunity, aooCount,
                    canAoo),
                SwiftContract = DescribeMembers(swift, hasSwift),
                TurnContract = DescribeMembers(cooldownClear, turnPrepare,
                    turnDispose, forceToEnd, delay),
                FlatFootedContract = DescribeMembers(flatFootedConstructor,
                    flatFootedTrigger),
                ThreatContract = DescribeMembers(isReach, getThreatHand),
                AidContract = DescribeMembers(
                    typeof(RuleCalculateAttackBonus).GetConstructors()
                        .SingleOrDefault(value => value.GetParameters().Length ==
                            4 && value.GetParameters()[0].ParameterType ==
                            typeof(UnitEntityData)),
                    typeof(RuleRollDice).GetMethod("Override", ExactInstance,
                        null, new[] { typeof(int) }, null)),
                AcBreakdownContract = DescribeMembers(acBonusSources,
                    bonusSourceConstructor, appendArmorClassBreakdown,
                    addBonusSources),
                DeliveryContract = InHarmsWayDeliveryAccess
                    .ContractDescription,
                HarmonyTargets = ObserveHarmony(context),
                Publication = DescribePublication(set)
            };
            evidence.SwiftContract.Add("HasSwiftAction.il=" +
                MethodIl(hasSwift));
            evidence.TurnContract.Add("Cooldowns.Clear.il=" +
                MethodIl(cooldownClear));
            evidence.TurnContract.Add("TurnController.Prepare.il=" +
                MethodIl(turnPrepare));

            Add(assertions, "bodyguard-game-build",
                "Kingmaker 2.1.7b Assembly-CSharp exact SHA-256 and MVID",
                "supportedVersion=" + evidence.GameVersion +
                    ";unityApplicationVersion=" +
                    evidence.UnityApplicationVersion + ";source=" +
                    evidence.GameVersionSource + ";sha256=" + gameHash +
                    ";mvid=" + gameMvid,
                string.Equals(gameHash, ExpectedAssemblySha256,
                    StringComparison.Ordinal) &&
                string.Equals(gameMvid, ExpectedAssemblyMvid,
                    StringComparison.OrdinalIgnoreCase),
                "live loaded Assembly-CSharp identity");
            Add(assertions, "bodyguard-combat-reflexes-native",
                "exact native Combat Reflexes GUID/name and flat-footed condition component",
                evidence.CombatReflexes,
                combatReflexes != null && string.Equals(
                    combatReflexes.AssetGuid,
                    BodyguardFeatBlueprints.CombatReflexesGuid,
                    StringComparison.Ordinal) && string.Equals(
                    combatReflexes.name,
                    BodyguardFeatBlueprints.CombatReflexesInternalName,
                    StringComparison.Ordinal) &&
                combatReflexes.ComponentsArray != null &&
                combatReflexes.Groups.Contains(FeatureGroup.Feat) &&
                combatReflexes.Groups.Contains(FeatureGroup.CombatFeat) &&
                combatReflexes.ComponentsArray.OfType<AddCondition>().Any(
                    value => string.Equals(value.Condition.ToString(),
                        "AttackOfOpportunityBeforeInitiative",
                        StringComparison.Ordinal)),
                "exact library lookup and native feature components");
            Add(assertions, "bodyguard-native-aoo-economy",
                "mutable AoO count, native CanAttackOfOpportunity, and bool simulate path",
                string.Join("|", evidence.AooContract.ToArray()),
                attackOfOpportunity != null &&
                    attackOfOpportunity.ReturnType == typeof(bool) &&
                    aooCount != null && aooCount.CanRead && aooCount.CanWrite &&
                    aooCount.PropertyType == typeof(int) && canAoo != null &&
                    canAoo.PropertyType == typeof(bool),
                "live UnitCombatState reflection");
            Add(assertions, "bodyguard-native-swift-economy",
                "shared mutable SwiftAction cooldown and HasSwiftAction exact cooldown-only implementation",
                string.Join("|", evidence.SwiftContract.ToArray()),
                swift != null && swift.CanRead && swift.CanWrite &&
                    swift.PropertyType == typeof(float) && hasSwift != null &&
                    hasSwift.ReturnType == typeof(bool) &&
                    hasSwift.GetMethodBody() != null &&
                    hasSwift.GetMethodBody().GetILAsByteArray().Length == 27,
                "live UnitCombatState.Cooldowns reflection");
            Add(assertions, "bodyguard-native-turn-economy",
                "actual-turn start clears native cooldowns, actual-turn completion is observable, and delay is distinct",
                string.Join("|", evidence.TurnContract.ToArray()),
                cooldownClear != null && turnPrepare != null &&
                    turnDispose != null && forceToEnd != null &&
                    delay != null && cooldownClear.ReturnType == typeof(void) &&
                    turnPrepare.ReturnType == typeof(void) &&
                    turnDispose.ReturnType == typeof(void),
                "live TurnController and UnitCombatState.Cooldowns reflection");
            Add(assertions, "bodyguard-native-flat-footed",
                "target-aware RuleCheckTargetFlatFooted constructor and trigger remain exact",
                string.Join("|", evidence.FlatFootedContract.ToArray()),
                flatFootedConstructor != null && flatFootedTrigger != null &&
                    flatFootedTrigger.ReturnType == typeof(void),
                "live RuleCheckTargetFlatFooted reflection");
            Add(assertions, "bodyguard-native-threat",
                "native edge/reach hand query accepts protector, attacker, and WeaponSlot",
                string.Join("|", evidence.ThreatContract.ToArray()),
                isReach != null && isReach.ReturnType == typeof(bool) &&
                    getThreatHand != null,
                "live UnitEngagementExtension reflection");
            Add(assertions, "bodyguard-native-aid-calculation",
                "target-aware RuleCalculateAttackBonus and isolated native RuleRollD20 override",
                string.Join("|", evidence.AidContract.ToArray()),
                evidence.AidContract.Count == 2 &&
                    evidence.AidContract.All(value =>
                        value.IndexOf("<missing>", StringComparison.Ordinal) < 0),
                "live native rule constructors/methods");
            Add(assertions, "bodyguard-native-ac-breakdown",
                "RuleCalculateAC stores named BonusSources rendered by the native attack-detail AC breakdown",
                string.Join("|", evidence.AcBreakdownContract.ToArray()) +
                    "|bodyguardName=" + (set == null ? "<missing>" :
                        set.Bodyguard.Name),
                acBonusSources != null && acBonusSources.FieldType ==
                    typeof(List<BonusSource>) &&
                    bonusSourceConstructor != null &&
                    appendArmorClassBreakdown != null &&
                    addBonusSources != null && set != null &&
                    string.Equals(set.Bodyguard.Name, "Bodyguard",
                        StringComparison.Ordinal),
                "live RuleCalculateAC, BonusSource, AttackLogMessage, and StatModifiersBreakdown reflection");
            Add(assertions, "bodyguard-delivery-contract",
                "all weapon, rule-event, and ability ApplyEffect redirection seams available",
                evidence.DeliveryContract,
                InHarmsWayDeliveryAccess.ContractAvailable &&
                    applyEffect != null && applyEffect.IsStatic &&
                    applyEffect.IsPrivate,
                "fail-closed InHarmsWayDeliveryAccess reflection contract");
            Add(assertions, "bodyguard-harmony-contract",
                "all attack/delivery and immediate-action lifecycle targets patched by this mod",
                string.Join("|", evidence.HarmonyTargets.ToArray()),
                HasPatch(evidence.HarmonyTargets, "RuleAttackRoll.OnTrigger") &&
                    HasPatch(evidence.HarmonyTargets,
                        "RuleCalculateAC.OnTrigger") &&
                    HasPatch(evidence.HarmonyTargets,
                        "RulebookEventContext.PopEvent") &&
                    HasPatch(evidence.HarmonyTargets,
                        "AbilityDeliveryTarget.set_AttackRoll") &&
                    HasPatch(evidence.HarmonyTargets,
                        "AbilityExecutionProcess.ApplyEffect") &&
                    HasPatch(evidence.HarmonyTargets,
                        "ElementsContextData.Dispose") &&
                    HasPatch(evidence.HarmonyTargets,
                        "TurnController.Prepare") &&
                    HasPatch(evidence.HarmonyTargets,
                        "UnitCombatState+Cooldowns.Clear") &&
                    HasPatch(evidence.HarmonyTargets,
                        "TurnController.Dispose") &&
                    HasPatch(evidence.HarmonyTargets,
                        "UnitEntityData.HasSwiftAction") &&
                    HasPatch(evidence.HarmonyTargets,
                        "UnitEntityData.PostLoad") &&
                    HasPatch(evidence.HarmonyTargets,
                        "CombatController.HandlePartyCombatStateChanged"),
                "live Harmony 1.2 prefix/postfix/transpiler registry");
            Add(assertions, "bodyguard-publication-live",
                "both feats singular in general and Fighter selections when module active",
                string.Join("|", evidence.Publication.ToArray()),
                context.FeatureModules.Active.BodyguardFeats && set != null &&
                    evidence.Publication.Count == 4 &&
                    evidence.Publication.All(value => value.EndsWith("=1",
                        StringComparison.Ordinal)),
                "live native selection arrays");
            Add(assertions, "bodyguard-observer-read-only",
                "no input, save, unit, action, or combat mutation",
                "reflection, blueprint reads, Harmony registry reads", true,
                "guarded observer does not create or mutate game state");

            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("nativeContractSha256=" + Hash(path));
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
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = evidence.GameVersion,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string DescribeCombatReflexes(BlueprintFeature feature)
        {
            if (feature == null) return "<missing>";
            return "guid=" + feature.AssetGuid + ";name=" + feature.name +
                ";groups=" + string.Join(",", feature.Groups.Select(value =>
                    value.ToString()).ToArray()) + ";components=" +
                string.Join("|", (feature.ComponentsArray ??
                    new Kingmaker.Blueprints.BlueprintComponent[0]).Select(
                        DescribeObject).ToArray());
        }

        private static string DescribeObject(object value)
        {
            if (value == null) return "<null>";
            var parts = new List<string> { value.GetType().FullName };
            foreach (FieldInfo field in value.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic).OrderBy(item => item.Name,
                    StringComparer.Ordinal))
            {
                object fieldValue;
                try { fieldValue = field.GetValue(value); }
                catch { continue; }
                if (fieldValue == null || fieldValue is string ||
                    fieldValue.GetType().IsPrimitive || fieldValue.GetType().IsEnum)
                    parts.Add(field.Name + "=" + (fieldValue ?? "<null>"));
            }
            return string.Join(",", parts.ToArray());
        }

        private static List<string> DescribePublication(
            BodyguardFeatBlueprintSet set)
        {
            var result = new List<string>();
            if (set == null) return result;
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    BodyguardFeatCatalogPublication.BasicFeatSelectionGuid,
                    "native basic feat selection");
            BlueprintFeatureSelection fighter = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    BodyguardFeatCatalogPublication
                        .FighterCombatFeatSelectionGuid,
                    "native Fighter combat feat selection");
            result.Add("basic.bodyguard=" + Count(basic, set.Bodyguard));
            result.Add("basic.inHarmsWay=" + Count(basic, set.InHarmsWay));
            result.Add("fighter.bodyguard=" + Count(fighter, set.Bodyguard));
            result.Add("fighter.inHarmsWay=" + Count(fighter,
                set.InHarmsWay));
            return result;
        }

        private static int Count(BlueprintFeatureSelection selection,
            BlueprintFeature feature)
        {
            return (selection.Features ?? new BlueprintFeature[0]).Count(value =>
                ReferenceEquals(value, feature) || value != null &&
                string.Equals(value.AssetGuid, feature.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static List<string> ObserveHarmony(ModContext context)
        {
            var result = new List<string>();
            foreach (MethodBase target in context.Harmony.GetPatchedMethods())
            {
                Patches patches = context.Harmony.GetPatchInfo(target);
                if (patches == null) continue;
                IEnumerable<Patch> all = patches.Prefixes
                    .Concat(patches.Postfixes)
                    .Concat(patches.Transpilers);
                if (!all.Any(value => value.patch != null &&
                    value.patch.DeclaringType != null &&
                    IsBodyguardPatchOwner(value.patch.DeclaringType))) continue;
                result.Add(Signature(target));
            }
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        private static bool IsBodyguardPatchOwner(Type type)
        {
            if (type == null) return false;
            if (type.Namespace == "KingmakerGunslinger.BodyguardFeats")
                return true;
            return type.FullName ==
                    "KingmakerGunslinger.Diagnostics.RuleAttackRollFirearmPatch" ||
                type.FullName ==
                    "KingmakerGunslinger.Diagnostics.RuleCalculateAcFirearmPatch";
        }

        private static bool HasPatch(IEnumerable<string> values, string suffix)
        {
            return values.Any(value => value.IndexOf(suffix,
                StringComparison.Ordinal) >= 0);
        }

        private static List<string> DescribeMembers(params MemberInfo[] members)
        {
            return members.Select(value => value == null ? "<missing>" :
                Signature(value)).ToList();
        }

        private static string MethodIl(MethodInfo method)
        {
            if (method == null || method.GetMethodBody() == null)
                return "<missing>";
            byte[] bytes = method.GetMethodBody().GetILAsByteArray();
            return bytes == null ? "<missing>" :
                Convert.ToBase64String(bytes);
        }

        private static string Signature(MemberInfo member)
        {
            var method = member as MethodBase;
            if (method != null)
                return method.DeclaringType.FullName + "." + method.Name +
                    "(" + string.Join(",", method.GetParameters().Select(
                        value => value.ParameterType.FullName).ToArray()) + ")";
            var property = member as PropertyInfo;
            if (property != null)
                return property.DeclaringType.FullName + "." + property.Name +
                    ":" + property.PropertyType.FullName + ";read=" +
                    property.CanRead + ";write=" + property.CanWrite;
            return member.DeclaringType.FullName + "." + member.Name;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", "").ToLowerInvariant();
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = id,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = evidence });
        }
    }
}
