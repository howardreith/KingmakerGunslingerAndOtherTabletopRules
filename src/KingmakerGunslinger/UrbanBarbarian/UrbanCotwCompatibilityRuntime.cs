using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal static class UrbanCotwCompatibilityRuntime
    {
        private const string CotwAssemblyName = "CallOfTheWild";
        private const string SupportedMvid =
            "8caab254-aacf-4811-8093-44b9184e6e53";
        private const string SupportedSha256 =
            "4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915";

        internal static void Reconcile(BlueprintBuff nativeRage,
            BlueprintBuff urbanRage)
        {
            UrbanCotwCompatibilityDecision decision;
            try
            {
                Assembly[] matches = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(value => string.Equals(value.GetName().Name,
                        CotwAssemblyName, StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                    decision = UrbanCotwCompatibilityPolicy.Evaluate(
                        UrbanCotwSurface.Absent, true, true, false);
                else if (matches.Length != 1)
                    decision = UrbanCotwCompatibilityPolicy.Evaluate(
                        UrbanCotwSurface.Ambiguous, false, false, false,
                        "cotw-assembly-count=" + matches.Length);
                else
                    decision = EvaluatePresent(matches[0], nativeRage, urbanRage);
            }
            catch (Exception exception)
            {
                decision = UrbanCotwCompatibilityPolicy.Evaluate(
                    UrbanCotwSurface.Unknown, false, false, false,
                    "runtime-inspection-exception=" +
                    exception.GetType().FullName);
            }
            UrbanCotwCompatibilityStatusRegistry.Update(decision);
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("urban-barbarian", "cotw.status",
                    decision.Diagnostic);
        }

        private static UrbanCotwCompatibilityDecision EvaluatePresent(
            Assembly cotw, BlueprintBuff nativeRage, BlueprintBuff urbanRage)
        {
            string mvid = cotw.ManifestModule.ModuleVersionId.ToString();
            string hash = Hash(cotw.Location);
            if (!string.Equals(mvid, SupportedMvid,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(hash, SupportedSha256,
                    StringComparison.OrdinalIgnoreCase))
                return UrbanCotwCompatibilityPolicy.Evaluate(
                    UrbanCotwSurface.Unknown, false, false, false,
                    "cotw-assembly-identity:mvid=" + mvid + ";sha256=" + hash);

            string[] nativeTypes = Types(nativeRage);
            string[] urbanTypes = Types(urbanRage);
            bool nativeLifecycle = nativeTypes.Length == 16 &&
                nativeTypes.Skip(12).All(value => string.Equals(value,
                    "CallOfTheWild.NewMechanics.FeatureReplacement",
                    StringComparison.Ordinal));
            bool marker = urbanTypes.Count(value => string.Equals(value,
                    "Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions",
                    StringComparison.Ordinal)) == 1 &&
                urbanTypes.Count(value => string.Equals(value,
                    "Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent",
                    StringComparison.Ordinal)) == 1;
            bool duplicate = urbanTypes.Any(value => value.StartsWith(
                "CallOfTheWild.", StringComparison.Ordinal));
            return UrbanCotwCompatibilityPolicy.Evaluate(
                nativeLifecycle && marker && !duplicate ?
                    UrbanCotwSurface.Supported : UrbanCotwSurface.Unknown,
                nativeLifecycle, marker, duplicate,
                !nativeLifecycle ? "finalized-native-rage-tail" :
                    (!marker ? "urban-rage-marker" : "duplicate-cotw-behavior"));
        }

        private static string[] Types(BlueprintBuff buff)
        {
            return (buff == null || buff.ComponentsArray == null ?
                new BlueprintComponent[0] : buff.ComponentsArray)
                .Select(value => value == null ? "<null>" :
                    value.GetType().FullName).ToArray();
        }

        private static string Hash(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return "<unavailable>";
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
