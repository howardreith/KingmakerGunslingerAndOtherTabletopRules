using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Engine-free guards for the shared grit UI recipient contract. The integration itself needs Kingmaker's blueprint
    /// types and is exercised in the game through
    /// scripts/compatibility/loader-compat/sessions/gunslinger-grit-ui-recipients.json. These tests keep that session's
    /// expected identities tied to the audited deeds and the released manifest, and keep displayed text out of the
    /// membership decision.
    /// </summary>
    internal static class GritUiRecipientContractTests
    {
        private const string Session =
            "scripts/compatibility/loader-compat/sessions/gunslinger-grit-ui-recipients.json";

        // The audit, by released manifest symbol. Shared: the button shows the
        // grit count and is disabled at zero; the deed spends by itself.
        private static readonly string[] SharedIndicator =
        {
            "KMG.Deeds.QuickClearMoveAbility",
            "KMG.Deeds.PistolWhipAbility",
            "KMG.Deeds.DeadShotAbility",
            "KMG.Deeds.TargetingArmsAbility",
            "KMG.Deeds.TargetingHeadAbility",
            "KMG.Deeds.TargetingTorsoAbility",
            "KMG.Deeds.TargetingLegsAbility",
            "KMG.Deeds.MenacingShotAbility",
            "KMG.Deeds.BleedingWoundHitPointsAbility",
            "KMG.Deeds.BleedingWoundStrengthAbility",
            "KMG.Deeds.BleedingWoundDexterityAbility",
            "KMG.Deeds.BleedingWoundConstitutionAbility",
            "KMG.Deeds.ExpertLoadingAbility",
            "KMG.Deeds.SlingersLuckSavingThrowAbility",
            "KMG.Deeds.SlingersLuckSkillCheckAbility",
            "KMG.Deeds.DeathsShotAbility",
            "KMG.Deeds.StunningShotAbility",
            "KMG.Archetypes.FocusedAimAbility",
            "KMG.Archetypes.ClippingShotAbility",
            "KMG.Archetypes.UpCloseAndDeadlyAbility",
            "KMG.Archetypes.TwinShotKnockdownAbility",
            "KMG.Archetypes.SteadyAimAbility"
        };

        // Built with their own grit resource logic, which also spends.
        private static readonly string[] OwnResourceLogic =
        {
            "KMG.Deeds.DeadeyeAbility",
            "KMG.Deeds.GunslingerDodgeProneAbility"
        };

        // Need grit to remain but spend none; their own logic gates them and
        // they have never carried the indicator.
        private static readonly string[] DeliberatelyAbsent =
        {
            "KMG.Deeds.QuickClearStandardAbility",
            "KMG.Deeds.UtilityShotStopBleedingAbility",
            "KMG.Deeds.StartlingShotAbility",
            "KMG.Deeds.LightningReloadAbility"
        };

        internal static void SessionExpectsExactlyTheAuditedIdentities()
        {
            Dictionary<string, JObject> manifest = Read("blueprints/blueprints.json")["entries"]
                .Cast<JObject>().ToDictionary(entry => (string)entry["guid"], entry => entry);
            JObject[] censuses = Read(Session)["launches"]
                .SelectMany(launch => launch["harness"] == null ? Enumerable.Empty<JToken>() :
                    launch["harness"]["steps"])
                .Cast<JObject>().Where(step => (string)step["op"] == "kmgGritUiCensus").ToArray();
            Assertions.True(censuses.Length >= 5,
                "The recipient session no longer censuses the stock, null, blank and isolation launches.");
            foreach (JObject census in censuses)
            {
                string label = (string)census["label"];
                Assertions.Equal("0", (string)census["expect"],
                    "Census " + label + " lets the grit UI pass read ability descriptions.");
                Symbols(manifest, ((string)census["type"]).Split(','), SharedIndicator,
                    "shared indicator, census " + label);
                Symbols(manifest, census["own"].Select(value => (string)value).ToArray(),
                    OwnResourceLogic, "own resource logic, census " + label);
            }

            HashSet<string> expected = new HashSet<string>(SharedIndicator.Concat(OwnResourceLogic));
            foreach (string symbol in DeliberatelyAbsent)
            {
                Assertions.True(manifest.Values.Any(entry => (string)entry["symbol"] == symbol),
                    "The audit names an ability the manifest does not hold: " + symbol);
                Assertions.False(expected.Contains(symbol),
                    "A deed that spends no grit is expected to carry a grit indicator: " + symbol);
            }
        }

        internal static void MembershipDoesNotComeFromDisplayedText()
        {
            string integration = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Grit", "GritAbilityUiIntegration.cs"));
            foreach (string forbidden in new[] { ".Description", "DisplayName", "GetAllBlueprints", "IndexOf(" })
                Assertions.False(integration.Contains(forbidden),
                    "The shared grit UI integration decides membership from displayed text or a library scan: " + forbidden);
        }

        private static void Symbols(Dictionary<string, JObject> manifest, string[] guids,
            string[] audited, string what)
        {
            Assertions.Equal(guids.Length, guids.Distinct().Count(), "Duplicate identity in " + what + ".");
            var symbols = new List<string>();
            foreach (string guid in guids)
            {
                Assertions.True(manifest.TryGetValue(guid, out JObject entry),
                    "Identity " + guid + " in " + what + " is not in the released manifest.");
                Assertions.Equal("BlueprintAbility", (string)entry["plannedType"],
                    "Identity " + guid + " in " + what + " is not an ability.");
                Assertions.Equal("active", (string)entry["status"],
                    "Identity " + guid + " in " + what + " is not active.");
                symbols.Add((string)entry["symbol"]);
            }

            string missing = string.Join(", ", audited.Except(symbols).ToArray());
            string extra = string.Join(", ", symbols.Except(audited).ToArray());
            Assertions.True(missing.Length == 0 && extra.Length == 0,
                "The session's " + what + " differs from the audit. Missing: [" + missing + "]. Unaudited: [" + extra + "].");
        }

        private static JObject Read(string relative)
        {
            return JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar))));
        }
    }
}
