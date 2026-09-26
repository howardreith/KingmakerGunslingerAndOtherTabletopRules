using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// The effective profile of the favored-class integration for this
    /// process. Publication changes are restart-required, so the profile is
    /// fixed once the first update resolves it.
    /// </summary>
    internal sealed class FavoredClassProfileState
    {
        internal FavoredClassProfileState(bool integrationEnabled, bool firstParty,
            bool adaptations, bool thirdParty, bool mostlyHuman)
        {
            IntegrationEnabled = integrationEnabled;
            FirstParty = firstParty;
            Adaptations = adaptations;
            ThirdParty = thirdParty;
            MostlyHuman = mostlyHuman;
        }

        /// <summary>
        /// Charter defaults: integration, faithful catalog, adaptations and
        /// Mostly Human on; third-party profile off.
        /// </summary>
        internal static FavoredClassProfileState Defaults
        {
            get { return new FavoredClassProfileState(true, true, true, false, true); }
        }

        internal bool IntegrationEnabled { get; private set; }
        internal bool FirstParty { get; private set; }
        internal bool Adaptations { get; private set; }
        internal bool ThirdParty { get; private set; }
        internal bool MostlyHuman { get; private set; }

        /// <summary>Whether new choices may be offered through a route of this profile.</summary>
        internal bool Offers(FavoredClassProfile profile)
        {
            if (!IntegrationEnabled)
                return false;
            switch (profile)
            {
                case FavoredClassProfile.FirstParty:
                    return FirstParty;
                case FavoredClassProfile.Adaptation:
                    return Adaptations;
                case FavoredClassProfile.ThirdParty:
                    return ThirdParty;
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            return "integration=" + Flag(IntegrationEnabled) + ",firstParty=" + Flag(FirstParty) +
                ",adaptations=" + Flag(Adaptations) + ",thirdParty=" + Flag(ThirdParty) +
                ",mostlyHuman=" + Flag(MostlyHuman);
        }

        private static string Flag(bool value)
        {
            return value ? "on" : "off";
        }
    }

    internal enum FavoredClassSettingsSource
    {
        /// <summary>No settings file: the charter defaults.</summary>
        Defaults,
        /// <summary>A valid settings file.</summary>
        File,
        /// <summary>An unreadable or malformed file: the charter defaults, reported.</summary>
        Invalid
    }

    internal sealed class FavoredClassSettingsResult
    {
        internal FavoredClassSettingsResult(FavoredClassProfileState profile,
            FavoredClassSettingsSource source, string detail)
        {
            Profile = profile;
            Source = source;
            Detail = detail;
        }

        internal FavoredClassProfileState Profile { get; private set; }
        internal FavoredClassSettingsSource Source { get; private set; }
        internal string Detail { get; private set; }

        public override string ToString()
        {
            return "source=" + Source + ";profile=" + Profile + (Detail == null ? string.Empty : ";detail=" + Detail);
        }
    }

    /// <summary>
    /// Optional FavoredClassIntegration.json beside the mod, read once when
    /// the integration attaches (restart-required). The file is never
    /// written by the mod. It must contain exactly the schema version and
    /// the five Boolean controls; anything else is reported and the charter
    /// defaults apply, so a malformed file can neither enable the
    /// third-party profile nor silently disable earned mechanics.
    /// </summary>
    internal static class FavoredClassSettings
    {
        internal const string FileName = "FavoredClassIntegration.json";
        internal const int SchemaVersion = 1;

        private static readonly string[] BooleanKeys =
            { "integration", "firstParty", "adaptations", "thirdParty", "mostlyHuman" };

        internal static FavoredClassSettingsResult Load(string modDirectory)
        {
            if (string.IsNullOrEmpty(modDirectory))
                return new FavoredClassSettingsResult(FavoredClassProfileState.Defaults,
                    FavoredClassSettingsSource.Defaults, "no mod directory");
            string path = Path.Combine(modDirectory, FileName);
            if (!File.Exists(path))
                return new FavoredClassSettingsResult(FavoredClassProfileState.Defaults,
                    FavoredClassSettingsSource.Defaults, null);
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception exception)
            {
                return Invalid("unreadable: " + exception.GetType().Name);
            }
            return Parse(text);
        }

        internal static FavoredClassSettingsResult Parse(string json)
        {
            JObject root;
            try
            {
                root = JObject.Parse(json ?? string.Empty);
            }
            catch (JsonException exception)
            {
                return Invalid("malformed JSON: " + exception.Message);
            }
            var expected = new HashSet<string>(BooleanKeys, StringComparer.Ordinal) { "schemaVersion" };
            string[] names = root.Properties().Select(property => property.Name).ToArray();
            string[] unknown = names.Where(name => !expected.Contains(name)).ToArray();
            string[] missing = expected.Where(name => !names.Contains(name, StringComparer.Ordinal)).ToArray();
            if (unknown.Length != 0 || missing.Length != 0)
                return Invalid("keys differ: unknown=" + string.Join(",", unknown) + ";missing=" +
                    string.Join(",", missing));
            if (root["schemaVersion"].Type != JTokenType.Integer ||
                (int)root["schemaVersion"] != SchemaVersion)
                return Invalid("unsupported schemaVersion");
            if (BooleanKeys.Any(key => root[key].Type != JTokenType.Boolean))
                return Invalid("every control must be true or false");
            return new FavoredClassSettingsResult(new FavoredClassProfileState(
                (bool)root["integration"], (bool)root["firstParty"], (bool)root["adaptations"],
                (bool)root["thirdParty"], (bool)root["mostlyHuman"]), FavoredClassSettingsSource.File, null);
        }

        private static FavoredClassSettingsResult Invalid(string detail)
        {
            return new FavoredClassSettingsResult(FavoredClassProfileState.Defaults,
                FavoredClassSettingsSource.Invalid, detail);
        }
    }
}
