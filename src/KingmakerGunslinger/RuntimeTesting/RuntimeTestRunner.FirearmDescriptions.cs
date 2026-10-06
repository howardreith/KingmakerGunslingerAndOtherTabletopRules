using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only, save-free observation of registered native localization
        // getters. Source scans and policy composition cannot establish this.
        private RuntimeTestResult RunFirearmDescriptionObservation()
        {
            var assertions = new List<RuntimeTestAssertion>();
            const string direct = "This firearm fires lead balls driven by black powder. It can misfire, and it must be reloaded to fire again. ";
            const string scatter = "This firearm fires lead balls driven by black powder, or pellets in a 15-foot cone with Scatter Shot. It can misfire, and it must be reloaded to fire again. ";
            const string pistolRange = "Attacks with this firearm are resolved against touch AC out to 20 ft. (its first range increment), and against normal AC at greater distances.";
            const string scatterRange = "Attacks with a lead ball are resolved against touch AC out to 10 ft. (its first range increment), and against normal AC at greater distances.";
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItemWeapon blunderbuss = BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item;
            BlueprintItemWeapon lastWord = BlueprintBootstrap.MagicFirearms.Require(MagicFirearmBlueprints.TheLastWordSymbol).Item;
            ObserveDescription(assertions, "pistol", pistol, direct + pistolRange);
            ObserveDescription(assertions, "blunderbuss", blunderbuss, scatter + scatterRange);
            ObserveDescription(assertions, "last-word", lastWord,
                FirearmEnchantmentItemText.Reliable + " " + FirearmEnchantmentItemText.Seeking + " " + pistolRange);
            string last = lastWord.Description;
            assertions.Add(Assertion("last-word-complete-properties-once", "both complete property clauses exactly once",
                last, Occurrences(last, FirearmEnchantmentItemText.Reliable) == 1 &&
                Occurrences(last, FirearmEnchantmentItemText.Seeking) == 1,
                "registered BlueprintItemWeapon.Description; complete Reliable minimum-0/natural-1 and Seeking sight/target/other-defense limits"));
            string cone = blunderbuss.Description;
            assertions.Add(Assertion("blunderbuss-lead-ball-attack-scope", "Scatter Shot cone prose; only lead-ball attacks receive penetration wording",
                cone, cone.Contains(scatter) && cone.Contains(scatterRange) &&
                !cone.Contains("Scatter Shot attacks") && !cone.Contains("separate cone rules"),
                "exact resolved text; no claim that Scatter Shot rolls an attack"));
            RuntimeTestResult result = CreateResult(assertions.All(a => a.Status == "PASS") ? "PASS" : "FAIL", assertions, null);
            result.Diagnostics.Add("HumanTooltipReview=NOT_PERFORMED; read-only registered blueprint/localization getters; no save required, UI input, fixture mutation or save-writing call");
            return result;
        }

        private static void ObserveDescription(List<RuntimeTestAssertion> assertions,
            string key, BlueprintItemWeapon item, string expected)
        {
            string resolved = item.Description;
            assertions.Add(Assertion(key + "-resolved-description", expected, resolved,
                string.Equals(expected, resolved, StringComparison.Ordinal),
                "registered blueprint=" + item.AssetGuid + ";nativeName=" + item.name + ";getter=BlueprintItemWeapon.Description"));
            assertions.Add(Assertion(key + "-spacing-punctuation", "no duplicate whitespace/punctuation or trailing whitespace", resolved,
                !string.IsNullOrWhiteSpace(resolved) && resolved == resolved.Trim() &&
                !Regex.IsMatch(resolved, @"(\s{2,}|[.!?]{2,}|[ \t]+[.,;!?])"),
                "actual final localized string in memory; not a source-text scan or visual-layout approval"));
        }

        private static int Occurrences(string text, string clause)
        {
            int count = 0;
            for (int index = 0; (index = text.IndexOf(clause, index, StringComparison.Ordinal)) >= 0; index += clause.Length)
                count++;
            return count;
        }
    }
}
