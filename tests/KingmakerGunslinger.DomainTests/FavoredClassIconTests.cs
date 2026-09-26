using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>Every published favored-class choice has a deterministic, non-blank icon.</summary>
    internal static class FavoredClassIconTests
    {
        private static readonly Regex Guid = new Regex("^[0-9a-f]{32}$");

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger" }.Concat(parts).ToArray()));
        }

        // Domain census: every leaf the catalog can publish maps to a donor.
        internal static void EveryLeafHasAnIconDonor()
        {
            int leaves = 0;
            foreach (FavoredClassLeafSpec leaf in FavoredClassLeafCatalog.AllLeaves())
            {
                leaves++;
                FavoredClassIconDonor donor = FavoredClassIconPolicy.For(leaf.EffectId, leaf.TargetKey);
                bool kmg = donor.Source != FavoredClassIconSource.Native && donor.Source != FavoredClassIconSource.Provider;
                Assertions.True(kmg ? donor.Guids.Length == 0 : donor.Guids.Length > 0 && donor.Guids.All(Guid.IsMatch),
                    leaf.Symbol + " has a well-formed icon donor.");
                Assertions.True(!string.IsNullOrWhiteSpace(donor.Reason), leaf.Symbol + " names what its icon represents.");
            }
            Assertions.Equal(FavoredClassLeafCatalog.AllLeaves().Count, leaves, "Every leaf was censused.");
            Assertions.Equal(FavoredClassIconSource.Provider,
                FavoredClassIconPolicy.For(FavoredClassCatalog.EffectSelectedRevelation, "FireBreath").Source,
                "Revelations use their own provider art.");
            Assertions.True(FavoredClassIconPolicy.For(FavoredClassCatalog.EffectSelectedRevelation, "BreathWeapon")
                .Guids.SequenceEqual(FavoredClassRevelationManifest.For("BreathWeapon").FeatureGuids),
                "A Dragon revelation tries every colour's feature.");
            Assertions.Equal(FavoredClassIconPolicy.CriticalFocusGuid,
                FavoredClassIconPolicy.For(FavoredClassCatalog.EffectFirearmConfirmation, null).Guids[0],
                "Firearm confirmation shows Critical Focus.");
            Assertions.Equal(FavoredClassIconPolicy.BarkskinGuid,
                FavoredClassIconPolicy.For(FavoredClassCatalog.EffectEidolonArmor, null).Guids.Single(),
                "Eidolon armor shows the native natural-armor bonus art (the provider's eidolon features have none).");
        }

        // The icon catalog records the same donor the policy assigns, and no
        // visible favored-class or Mostly Human consumer keeps a null placeholder.
        internal static void CatalogRecordsEveryDonor()
        {
            JObject catalog = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "assets-source", "original-icons", "icon-catalog.json")));
            var art = (JObject)catalog["currentArtSources"];
            var consumers = catalog["consumers"].Where(value => ((string)value["symbol"]).StartsWith(
                "KMG.FavoredClass.", StringComparison.Ordinal) || ((string)value["symbol"]).StartsWith(
                "KMG.MostlyHuman.", StringComparison.Ordinal)).ToDictionary(value => (string)value["symbol"]);
            Assertions.False(art.Properties().Any(value => value.Name == "fcb-null" || value.Name == "mostly-human-null"),
                "No null-icon art source remains.");
            foreach (FavoredClassLeafSpec leaf in FavoredClassLeafCatalog.AllLeaves())
            {
                JToken consumer;
                Assertions.True(consumers.TryGetValue(leaf.Symbol, out consumer), leaf.Symbol + " is cataloged.");
                FavoredClassIconDonor donor = FavoredClassIconPolicy.For(leaf.EffectId, leaf.TargetKey);
                string disposition = (string)consumer["disposition"];
                string source = (string)consumer["currentArt"];
                if (leaf.EffectId == FavoredClassCatalog.EffectPerformanceRange &&
                    !FavoredClassPerformanceManifest.For(leaf.TargetKey).Published)
                    Assertions.True(disposition == "hidden-internal" && source == "fcb-internal",
                        leaf.Symbol + " is registered but never shown.");
                else if (donor.Source == FavoredClassIconSource.Native)
                    Assertions.True(disposition == "native-semantic-reuse" &&
                        (string)art[source]["nativeDonorGuid"] == donor.Guids[0],
                        leaf.Symbol + " records its native donor.");
                else if (donor.Source == FavoredClassIconSource.Provider)
                    Assertions.True(disposition == "intentional-family-share" && source == "fcb-provider-donor",
                        leaf.Symbol + " records its provider donor.");
                else
                    Assertions.True(disposition == "intentional-family-share" && source == "fcb-kmg-family",
                        leaf.Symbol + " records its KMG family donor.");
            }
            foreach (KeyValuePair<string, JToken> pair in consumers.Where(value =>
                value.Key.StartsWith("KMG.MostlyHuman.", StringComparison.Ordinal) &&
                value.Key != "KMG.MostlyHuman.Identity"))
                Assertions.Equal(pair.Key.EndsWith(".Standard", StringComparison.Ordinal)
                    ? "mostly-human-parent-race" : "mostly-human-human-ancestry",
                    (string)pair.Value["currentArt"], pair.Key + " records its ancestry donor.");
        }

        // Runtime completion withholds any counter that would still be blank,
        // and Mostly Human is unpublished rather than blank.
        internal static void BlankIconsAreNeverPublished()
        {
            string icons = Source("FavoredClass", "FavoredClassLeafIcons.cs");
            Assertions.True(icons.Contains("withheld.Add(key);") && icons.Contains("access.SetIcon(leaf, icon);"),
                "Provider icons are assigned by exact identity; a missing icon withholds its counter.");
            string coordinator = Source("FavoredClass", "FavoredClassIntegrationCoordinator.cs");
            int complete = coordinator.IndexOf("FavoredClassLeafIcons.Complete(", StringComparison.Ordinal);
            int plan = coordinator.IndexOf("publication = FavoredClassPublication.Plan(", StringComparison.Ordinal);
            Assertions.True(complete > 0 && plan > complete, "Icons are completed before the publication is planned.");
            Assertions.True(coordinator.Contains("CompleteMostlyHumanIcons(context);") &&
                coordinator.Contains("set.Publication.Rollback();"),
                "Mostly Human icons are completed first; a blank choice unpublishes the trait.");
            string mostlyHuman = Source("ElementalRaces", "ElementalMostlyHumanIcons.cs");
            Assertions.True(mostlyHuman.Contains("Assign(access, race.Standard, parent, evidence);") &&
                mostlyHuman.Contains("Assign(access, race.Trait, humanIcon, evidence);") &&
                mostlyHuman.Contains("Assign(access, race.Selection, humanIcon, evidence);"),
                "Standard shows its parent race; the trait and selector show the Human ancestry.");
        }
    }
}
