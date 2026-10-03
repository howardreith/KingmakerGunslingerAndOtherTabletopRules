using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Player-facing firearm item description contracts: every mod firearm
    /// item description is non-empty, mechanically scoped, free of internal
    /// implementation notes, and composed from the shared sentence constants.
    /// </summary>
    internal static class FirearmItemDescriptionTests
    {
        private static readonly string[] ProhibitedPhrases =
        {
            "ft. base",
            "separate cone rules",
            "ordinary direct fire",
            "after other increases",
            "Penetration: Touch AC within",
            "Uses black powder and lead balls"
        };

        private static readonly string[] DescriptionBearingSources =
        {
            "Firearms/FirearmPenetrationPresentation.cs",
            "Firearms/FirearmEnchantmentItemText.cs",
            "Firearms/MidgameFirearmCatalog.cs",
            "Blueprints/ProductionFirearmBlueprints.cs",
            "Blueprints/MagicFirearmBlueprints.cs",
            "Acquisition/ProgressionWeaponCatalog.cs"
        };

        internal static void PenetrationTextIsNaturalAndExact()
        {
            AssertPenetration(FirearmDefinitions.CreateEarlyPistol(), 20d,
                "its first range increment", false);
            AssertPenetration(FirearmDefinitions.CreateEarlyMusket(), 40d,
                "its first range increment", false);
            AssertPenetration(FirearmDefinitions.CreateEarlyBlunderbuss(), 10d,
                "its first range increment", true);
            AssertPenetration(FirearmDefinitions.CreateAdvancedRifle(), 400d,
                "its first five range increments", false);
            AssertPenetration(FirearmDefinitions.CreateAdvancedRevolver(), 100d,
                "its first five range increments", false);
        }

        internal static void EnchantmentSentencesStayTruthful()
        {
            string reliable = FirearmEnchantmentItemText.Reliable;
            string seeking = FirearmEnchantmentItemText.Seeking;
            Assertions.True(reliable.Contains(
                    "misfire value is 1 lower than it would otherwise be") &&
                reliable.Contains("to a minimum of 0") &&
                reliable.Contains("A natural 1 still misses."),
                "The Reliable sentence lost its exact limits.");
            Assertions.True(seeking.Contains(
                    "ignore the miss chance from concealment") &&
                seeking.Contains("could not otherwise target") &&
                seeking.Contains("other defenses still apply"),
                "The Seeking sentence lost its exact limits.");
            Assertions.False(reliable.Contains("never misfires") ||
                reliable.Contains("never misses"),
                "The Reliable sentence overpromises.");
            Assertions.False(seeking.Contains("reveals unseen creatures") ||
                seeking.Contains("bypasses other defenses"),
                "The Seeking sentence overpromises.");
        }

        internal static void EveryItemDescriptionResolves()
        {
            foreach (MidgameFirearmSpec spec in MidgameFirearmCatalog.Entries)
            {
                Assertions.True(!string.IsNullOrWhiteSpace(spec.Description) &&
                    !string.IsNullOrWhiteSpace(spec.Flavor),
                    spec.DisplayName + " lost its authored property/flavor text.");
                AssertCompose(spec.DisplayName, spec.Description,
                    spec.Kind == FirearmKind.Blunderbuss);
            }
            string combined = FirearmEnchantmentItemText.Reliable + " " +
                FirearmEnchantmentItemText.Seeking;
            Assertions.True(combined.Contains("misfire value is 1 lower") &&
                combined.Contains("miss chance from concealment"),
                "The Last Word's combined property text is incomplete.");
            AssertCompose("The Last Word", combined, false);
            Assertions.Equal(FirearmEnchantmentItemText.Reliable,
                ProgressionWeaponCatalog.ReliableItemDescription,
                "Vendor progression items no longer share the Reliable sentence.");
        }

        internal static void SourcesCarryNoInternalNotes()
        {
            foreach (string relative in DescriptionBearingSources)
                AssertSourceClean(relative);
            string presentation = ReadSource(
                "Firearms/FirearmPenetrationPresentation.cs");
            Assertions.True(presentation.Contains(
                    "FirearmPenetrationRangePolicy") &&
                presentation.Contains("EffectivePenetrationRangeFeet"),
                "The penetration sentence must still be derived from the real range policy.");
            string shared = ReadSource("Firearms/FirearmEnchantmentItemText.cs");
            Assertions.True(shared.Contains("internal const string Reliable") &&
                shared.Contains("internal const string Seeking"),
                "Shared enchantment sentences must remain compile-time constants.");
            string production = ReadSource(
                "Blueprints/ProductionFirearmBlueprints.cs");
            Assertions.True(production.Contains(
                    "localizationStem + \".Description\"") &&
                production.Contains("localizationStem + \".Flavor\""),
                "Base item localization keys must keep their stable composition.");
            string magic = ReadSource("Blueprints/MagicFirearmBlueprints.cs");
            Assertions.True(magic.Contains("spec.Symbol + \".Description\"") &&
                magic.Contains("spec.Symbol + \".Flavor\"") &&
                magic.Contains("FirearmEnchantmentItemText.Reliable") &&
                magic.Contains("FirearmEnchantmentItemText.Seeking"),
                "Magic item keys/composition or shared sentences drifted.");
            string midgame = ReadSource("Firearms/MidgameFirearmCatalog.cs");
            Assertions.True(midgame.Contains(
                    "FirearmEnchantmentItemText.Reliable") &&
                midgame.Contains("FirearmEnchantmentItemText.Seeking"),
                "The merchant catalog stopped using the shared sentences.");
            string progression = ReadSource(
                "Acquisition/ProgressionWeaponCatalog.cs");
            Assertions.True(progression.Contains(
                    "FirearmEnchantmentItemText.Reliable"),
                "Vendor progression stopped using the shared Reliable sentence.");
        }

        private static void AssertPenetration(FirearmDefinition definition,
            double expectedFeet, string expectedWindow, bool scatter)
        {
            string text = FirearmPenetrationPresentation.Describe(definition);
            string feet = expectedFeet.ToString("0") + " ft.";
            Assertions.True(!string.IsNullOrWhiteSpace(text) &&
                text.Contains(feet) && text.Contains(expectedWindow) &&
                text.Contains("touch AC") &&
                text.Contains("normal AC at greater distances"),
                definition.Kind + " penetration sentence is incomplete.");
            Assertions.Equal(
                text.Split('(').Length, text.Split(')').Length,
                definition.Kind + " penetration parentheses are unbalanced.");
            if (scatter)
                Assertions.True(text.StartsWith("Attacks with a lead ball",
                    StringComparison.Ordinal),
                    "Scatter penetration must stay scoped to lead-ball attacks.");
            else
                Assertions.True(text.StartsWith("Attacks with this firearm",
                    StringComparison.Ordinal),
                    "Ordinary penetration must name the firearm's attacks.");
            foreach (string phrase in ProhibitedPhrases)
                Assertions.False(text.Contains(phrase),
                    definition.Kind + " penetration exposes '" + phrase + "'.");
        }

        private static void AssertCompose(string displayName,
            string propertySentence, bool scatter)
        {
            string composed = propertySentence + " " +
                FirearmPenetrationPresentation.Describe(
                    scatter ? FirearmDefinitions.CreateEarlyBlunderbuss() :
                        FirearmDefinitions.CreateEarlyPistol());
            Assertions.True(!string.IsNullOrWhiteSpace(composed) &&
                !composed.Contains("  "),
                displayName + " composed description is malformed.");
        }

        private static void AssertSourceClean(string relative)
        {
            string source = ReadSource(relative);
            foreach (string phrase in ProhibitedPhrases)
                Assertions.False(source.Contains(phrase),
                    relative + " still contains prohibited internal text '" +
                    phrase + "'.");
        }

        private static string ReadSource(string relative)
        {
            DirectoryInfo root = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (root != null &&
                !File.Exists(Path.Combine(root.FullName, "Info.json")))
                root = root.Parent;
            Assertions.True(root != null, "Repository fixture root.");
            string[] parts = new[] { root.FullName, "src", "KingmakerGunslinger" }
                .Concat(relative.Split('/')).ToArray();
            return File.ReadAllText(Path.Combine(parts));
        }
    }
}
