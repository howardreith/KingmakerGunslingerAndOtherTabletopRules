using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurShareTargetingScenario
    {
        private const string FileName = "brown-fur-share-targeting.json";
        private const string PersonalSpellGuid =
            "3481906baed9487e8403e91a2e9d010a";
        private const float ThirtyFeetMeters = 9.144f;

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("spellGuid", Order = 1)] public string SpellGuid { get; set; }
            [JsonProperty("rangeBefore", Order = 2)] public string RangeBefore { get; set; }
            [JsonProperty("flagsBefore", Order = 3)] public string FlagsBefore { get; set; }
            [JsonProperty("baselineAnchor", Order = 4)] public string BaselineAnchor { get; set; }
            [JsonProperty("baselineCanTarget", Order = 5)] public bool BaselineCanTarget { get; set; }
            [JsonProperty("baselineApproachMeters", Order = 6)] public float BaselineApproachMeters { get; set; }
            [JsonProperty("touchBegan", Order = 7)] public bool TouchBegan { get; set; }
            [JsonProperty("touchAnchor", Order = 8)] public string TouchAnchor { get; set; }
            [JsonProperty("touchCanTarget", Order = 9)] public bool TouchCanTarget { get; set; }
            [JsonProperty("touchRejectsDifferentTarget", Order = 10)] public bool TouchRejectsDifferentTarget { get; set; }
            [JsonProperty("touchApproachMeters", Order = 11)] public float TouchApproachMeters { get; set; }
            [JsonProperty("touchReleased", Order = 12)] public bool TouchReleased { get; set; }
            [JsonProperty("restoredAnchor", Order = 13)] public string RestoredAnchor { get; set; }
            [JsonProperty("restoredCanTarget", Order = 14)] public bool RestoredCanTarget { get; set; }
            [JsonProperty("capstoneBegan", Order = 15)] public bool CapstoneBegan { get; set; }
            [JsonProperty("capstoneAnchor", Order = 16)] public string CapstoneAnchor { get; set; }
            [JsonProperty("capstoneCanTarget", Order = 17)] public bool CapstoneCanTarget { get; set; }
            [JsonProperty("capstoneApproachMeters", Order = 18)] public float CapstoneApproachMeters { get; set; }
            [JsonProperty("capstoneDeltaMeters", Order = 19)] public float CapstoneDeltaMeters { get; set; }
            [JsonProperty("capstoneReleased", Order = 20)] public bool CapstoneReleased { get; set; }
            [JsonProperty("rangeAfter", Order = 21)] public string RangeAfter { get; set; }
            [JsonProperty("flagsAfter", Order = 22)] public string FlagsAfter { get; set; }
            [JsonProperty("activeScopesAfter", Order = 23)] public int ActiveScopesAfter { get; set; }
            [JsonProperty("unitsRemoved", Order = 24)] public bool UnitsRemoved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { SpellGuid = PersonalSpellGuid };
            UnitEntityData caster = null;
            UnitEntityData target = null;
            UnitEntityData other = null;
            bool casterRegistered = false;
            bool targetRegistered = false;
            bool otherRegistered = false;
            string stage = "contract";
            try
            {
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible)
                    throw new InvalidOperationException(
                        "Compatible Call of the Wild contract is unavailable.");
                BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(PersonalSpellGuid);
                if (spell == null || spell.Range != AbilityRange.Personal)
                    throw new InvalidOperationException(
                        "The exact Personal Transmutation fixture is unavailable.");
                stage = "units";
                var source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                other = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                casterRegistered = Game.Instance.State.Units.All.Add(caster);
                targetRegistered = Game.Instance.State.Units.All.Add(target);
                otherRegistered = Game.Instance.State.Units.All.Add(other);
                if (!casterRegistered || !targetRegistered || !otherRegistered)
                    throw new InvalidOperationException(
                        "Disposable Share targeting units were not registered.");

                var data = new AbilityData(spell, caster.Descriptor);
                var exact = new TargetWrapper(target);
                var different = new TargetWrapper(other);
                evidence.RangeBefore = spell.Range.ToString();
                evidence.FlagsBefore = Flags(spell);
                stage = "baseline";
                evidence.BaselineAnchor = data.TargetAnchor.ToString();
                evidence.BaselineCanTarget = data.CanTarget(exact);
                evidence.BaselineApproachMeters =
                    data.GetApproachDistance(target);

                stage = "touch";
                evidence.TouchBegan = BrownFurShareTargetingRuntime.Begin(
                    "share-runtime-touch", data, target,
                    BrownFurShareDelivery.Touch);
                evidence.TouchAnchor = data.TargetAnchor.ToString();
                evidence.TouchCanTarget = data.CanTarget(exact);
                evidence.TouchRejectsDifferentTarget = !data.CanTarget(different);
                evidence.TouchApproachMeters = data.GetApproachDistance(target);
                evidence.TouchReleased = BrownFurShareTargetingRuntime.Release(
                    "share-runtime-touch");
                evidence.RestoredAnchor = data.TargetAnchor.ToString();
                evidence.RestoredCanTarget = data.CanTarget(exact);

                stage = "capstone";
                evidence.CapstoneBegan = BrownFurShareTargetingRuntime.Begin(
                    "share-runtime-capstone", data, target,
                    BrownFurShareDelivery.ThirtyFeet);
                evidence.CapstoneAnchor = data.TargetAnchor.ToString();
                evidence.CapstoneCanTarget = data.CanTarget(exact);
                evidence.CapstoneApproachMeters =
                    data.GetApproachDistance(target);
                evidence.CapstoneDeltaMeters = evidence.CapstoneApproachMeters -
                    evidence.BaselineApproachMeters;
                evidence.CapstoneReleased = BrownFurShareTargetingRuntime.Release(
                    "share-runtime-capstone");
                evidence.RangeAfter = spell.Range.ToString();
                evidence.FlagsAfter = Flags(spell);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception.GetType().FullName + ":" + exception.Message);
            }
            finally
            {
                BrownFurShareTargetingRuntime.Clear();
                evidence.ActiveScopesAfter =
                    BrownFurShareTargetingRuntime.ActiveScopeCount;
                if (otherRegistered) Game.Instance.State.Units.All.Remove(other);
                if (targetRegistered) Game.Instance.State.Units.All.Remove(target);
                if (casterRegistered) Game.Instance.State.Units.All.Remove(caster);
                if (other != null) other.Dispose();
                if (target != null) target.Dispose();
                if (caster != null) caster.Dispose();
                evidence.UnitsRemoved = (other == null ||
                    !Game.Instance.State.Units.All.Contains(other)) &&
                    (target == null ||
                    !Game.Instance.State.Units.All.Contains(target)) &&
                    (caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster));
            }

            Add(assertions, "share-targeting-baseline",
                "Personal spell remains self-only before scope",
                evidence.BaselineAnchor + ";canTarget=" +
                    evidence.BaselineCanTarget,
                evidence.RangeBefore == AbilityRange.Personal.ToString() &&
                    evidence.BaselineAnchor != "Unit" &&
                    !evidence.BaselineCanTarget,
                "real installed Personal Transmutation with CotW patches active");
            Add(assertions, "share-targeting-touch",
                "exact target accepted with Unit anchor and native contact distance",
                evidence.TouchAnchor + ";canTarget=" + evidence.TouchCanTarget +
                    ";differentRejected=" +
                    evidence.TouchRejectsDifferentTarget + ";distance=" +
                    evidence.TouchApproachMeters,
                evidence.TouchBegan && evidence.TouchAnchor == "Unit" &&
                    evidence.TouchCanTarget &&
                    evidence.TouchRejectsDifferentTarget && Nearly(
                        evidence.TouchApproachMeters,
                        evidence.BaselineApproachMeters),
                "after-CotW postfix result overrides for exact AbilityData/target");
            Add(assertions, "share-targeting-release",
                "release restores CotW result",
                evidence.RestoredAnchor + ";canTarget=" +
                    evidence.RestoredCanTarget,
                evidence.TouchReleased &&
                    evidence.RestoredAnchor == evidence.BaselineAnchor &&
                    evidence.RestoredCanTarget == evidence.BaselineCanTarget,
                "transaction-local cleanup without blueprint mutation");
            Add(assertions, "share-targeting-thirty-feet",
                "exact target accepted and approach distance adds 9.144 meters",
                evidence.CapstoneAnchor + ";canTarget=" +
                    evidence.CapstoneCanTarget + ";delta=" +
                    evidence.CapstoneDeltaMeters,
                evidence.CapstoneBegan && evidence.CapstoneAnchor == "Unit" &&
                    evidence.CapstoneCanTarget && Nearly(
                        evidence.CapstoneDeltaMeters, ThirtyFeetMeters) &&
                    evidence.CapstoneReleased,
                "native corporeal radii retained plus exact 30-foot spell range");
            Add(assertions, "share-targeting-isolation-cleanup",
                "blueprint unchanged, scopes zero, units removed",
                "range=" + evidence.RangeBefore + "/" + evidence.RangeAfter +
                    ";flags=" + evidence.FlagsBefore + "/" +
                    evidence.FlagsAfter + ";scopes=" +
                    evidence.ActiveScopesAfter + ";units=" +
                    evidence.UnitsRemoved,
                evidence.RangeBefore == evidence.RangeAfter &&
                    evidence.FlagsBefore == evidence.FlagsAfter &&
                    evidence.ActiveScopesAfter == 0 && evidence.UnitsRemoved,
                "no global spell mutation or leaked per-cast state");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("shareTargetingSha256=" + Hash(path));
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string Flags(BlueprintAbility value)
        {
            return "self=" + value.CanTargetSelf + ";friends=" +
                value.CanTargetFriends + ";enemies=" + value.CanTargetEnemies +
                ";point=" + value.CanTargetPoint;
        }

        private static bool Nearly(float left, float right)
        { return Math.Abs(left - right) <= 0.001f; }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    item.Key == key);
            return value == null ? string.Empty : value.Value;
        }
    }
}
