using System;
using System.IO;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>The optional restart-required FavoredClassIntegration.json profile.</summary>
    internal static class FavoredClassSettingsTests
    {
        private const string Valid =
            "{\"schemaVersion\":1,\"integration\":true,\"firstParty\":true,\"adaptations\":false," +
            "\"thirdParty\":true,\"mostlyHuman\":false}";

        internal static void ValidFileSetsEveryControl()
        {
            FavoredClassSettingsResult result = FavoredClassSettings.Parse(Valid);
            Assertions.Equal(FavoredClassSettingsSource.File, result.Source, "Valid source.");
            FavoredClassProfileState profile = result.Profile;
            Assertions.True(profile.IntegrationEnabled && profile.FirstParty && !profile.Adaptations &&
                profile.ThirdParty && !profile.MostlyHuman, "Every control is read.");
            Assertions.True(profile.Offers(FavoredClassProfile.FirstParty) &&
                !profile.Offers(FavoredClassProfile.Adaptation) &&
                profile.Offers(FavoredClassProfile.ThirdParty) &&
                !profile.Offers(FavoredClassProfile.None), "Offers follows the controls.");
        }

        internal static void DisabledIntegrationOffersNothing()
        {
            FavoredClassProfileState profile = FavoredClassSettings.Parse(Valid.Replace(
                "\"integration\":true", "\"integration\":false")).Profile;
            Assertions.False(profile.IntegrationEnabled, "Integration off.");
            foreach (FavoredClassProfile route in new[] { FavoredClassProfile.FirstParty,
                FavoredClassProfile.Adaptation, FavoredClassProfile.ThirdParty })
                Assertions.False(profile.Offers(route), "No route is offered without the integration.");
        }

        internal static void DefaultsAndInvalidFilesUseTheCharterProfile()
        {
            FavoredClassProfileState defaults = FavoredClassProfileState.Defaults;
            Assertions.True(defaults.IntegrationEnabled && defaults.FirstParty && defaults.Adaptations &&
                !defaults.ThirdParty && defaults.MostlyHuman, "Charter defaults.");
            string absent = Path.Combine(Path.GetTempPath(), "kmg-fcb-settings-" + Guid.NewGuid().ToString("N"));
            FavoredClassSettingsResult none = FavoredClassSettings.Load(absent);
            Assertions.Equal(FavoredClassSettingsSource.Defaults, none.Source, "Absent file uses defaults.");
            foreach (string invalid in new[]
            {
                "not json",
                "[]",
                Valid.Replace("\"thirdParty\":true", "\"thirdParty\":\"yes\""),
                Valid.Replace("\"schemaVersion\":1", "\"schemaVersion\":2"),
                Valid.Replace(",\"mostlyHuman\":false", string.Empty),
                Valid.Replace("}", ",\"extra\":true}")
            })
            {
                FavoredClassSettingsResult result = FavoredClassSettings.Parse(invalid);
                Assertions.Equal(FavoredClassSettingsSource.Invalid, result.Source, "Invalid: " + invalid);
                Assertions.False(result.Profile.ThirdParty, "An invalid file never enables the third-party profile.");
                Assertions.True(result.Profile.IntegrationEnabled,
                    "An invalid file never silently disables earned mechanics.");
                Assertions.False(string.IsNullOrEmpty(result.Detail), "Invalid files are reported.");
            }
        }
    }
}
