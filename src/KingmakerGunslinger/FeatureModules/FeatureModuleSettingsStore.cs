using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.FeatureModules
{
    internal static class FeatureModuleSettingsStore
    {
        internal const int CurrentSchemaVersion = 10;
        internal const string FileName = "FeatureModules.json";

        internal static FeatureModuleSettingsState Load(string modPath,
            Action<string> warning = null, Func<DateTime> utcNow = null)
        {
            if (string.IsNullOrWhiteSpace(modPath))
                throw new ArgumentException("A mod path is required.", "modPath");
            string path = System.IO.Path.Combine(modPath, FileName);
            if (!File.Exists(path))
                return new FeatureModuleSettingsState(
                    FeatureModuleConfiguration.Defaults, path, "missing-defaults", false);
            try
            {
                JObject root = JObject.Parse(File.ReadAllText(path));
                JToken schemaToken = root["schemaVersion"];
                int schema = schemaToken == null ? 0 : RequireInteger(schemaToken,
                    "schemaVersion");
                if (schema > CurrentSchemaVersion)
                    throw new UnsupportedFeatureModuleSchemaException(
                        "Unsupported future feature-module settings schema " +
                        schema + ".");
                if (schema < 0)
                    throw new JsonException("Unsupported feature-module settings schema " + schema + ".");
                bool gunslinger = ReadDefaultOn(root, FeatureModuleConfiguration.GunslingerId);
                bool acadamae = ReadDefaultOn(root,
                    FeatureModuleConfiguration.AcadamaeGraduateId);
                bool shieldOther = ReadDefaultOn(root,
                    FeatureModuleConfiguration.ShieldOtherId);
                bool expandedSummoning = ReadDefaultOn(root,
                    FeatureModuleConfiguration.ExpandedSummoningId);
                bool elvenBranchedSpears = ReadDefaultOn(root,
                    FeatureModuleConfiguration.ElvenBranchedSpearsId);
                bool easternWeapons = ReadDefaultOn(root,
                    FeatureModuleConfiguration.EasternWeaponsId);
                bool brownFurTransmuter = ReadDefaultOn(root,
                    FeatureModuleConfiguration.BrownFurTransmuterId);
                bool urbanBarbarian = ReadDefaultOn(root,
                    FeatureModuleConfiguration.UrbanBarbarianId);
                bool bodyguardFeats = ReadDefaultOn(root,
                    FeatureModuleConfiguration.BodyguardFeatsId);
                bool protectionFromAlignmentControlImmunity = ReadDefaultOn(root,
                    FeatureModuleConfiguration
                        .ProtectionFromAlignmentControlImmunityId);
                bool elementalRaces = ReadDefaultOff(root,
                    FeatureModuleConfiguration.ElementalRacesId);
                var state = new FeatureModuleSettingsState(
                    new FeatureModuleConfiguration(gunslinger, acadamae,
                        shieldOther, expandedSummoning, elvenBranchedSpears,
                        easternWeapons, brownFurTransmuter, urbanBarbarian,
                        bodyguardFeats,
                        protectionFromAlignmentControlImmunity,
                        elementalRaces), path,
                    schema < CurrentSchemaVersion ? "migrated-schema-" + schema :
                        "settings", false);
                if (schema < CurrentSchemaVersion) Save(state);
                return state;
            }
            catch (Exception exception)
            {
                if (exception is UnsupportedFeatureModuleSchemaException) throw;
                DateTime now = (utcNow ?? (() => DateTime.UtcNow))();
                string quarantine = path + ".malformed." +
                    now.ToString("yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture);
                string evidence;
                try
                {
                    File.Copy(path, quarantine, false);
                    evidence = "quarantined=" + quarantine;
                }
                catch (Exception copyException)
                {
                    evidence = "quarantine-failed=" + copyException.GetType().FullName +
                        ":" + copyException.Message;
                }
                if (warning != null) warning("Malformed feature-module settings at " +
                    path + "; recovered mixed defaults (existing modules ON, " +
                    "Elemental Races OFF); " + evidence + "; error=" +
                    exception.GetType().FullName + ":" + exception.Message);
                return new FeatureModuleSettingsState(
                    FeatureModuleConfiguration.Defaults, path,
                    "malformed-recovery", true);
            }
        }

        internal static void Save(FeatureModuleSettingsState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            JObject root = new JObject
            {
                ["schemaVersion"] = CurrentSchemaVersion,
                [FeatureModuleConfiguration.GunslingerId] = state.Pending.Gunslinger,
                [FeatureModuleConfiguration.AcadamaeGraduateId] =
                    state.Pending.AcadamaeGraduate,
                [FeatureModuleConfiguration.ShieldOtherId] = state.Pending.ShieldOther,
                [FeatureModuleConfiguration.ExpandedSummoningId] =
                    state.Pending.ExpandedSummoning,
                [FeatureModuleConfiguration.ElvenBranchedSpearsId] =
                    state.Pending.ElvenBranchedSpears,
                [FeatureModuleConfiguration.EasternWeaponsId] =
                    state.Pending.EasternWeapons,
                [FeatureModuleConfiguration.BrownFurTransmuterId] =
                    state.Pending.BrownFurTransmuter,
                [FeatureModuleConfiguration.UrbanBarbarianId] =
                    state.Pending.UrbanBarbarian,
                [FeatureModuleConfiguration.BodyguardFeatsId] =
                    state.Pending.BodyguardFeats,
                [FeatureModuleConfiguration
                    .ProtectionFromAlignmentControlImmunityId] =
                    state.Pending.ProtectionFromAlignmentControlImmunity,
                [FeatureModuleConfiguration.ElementalRacesId] =
                    state.Pending.ElementalRaces
            };
            string temporary = state.Path + ".tmp";
            string backup = state.Path + ".previous";
            File.WriteAllText(temporary, root.ToString(Formatting.Indented));
            try
            {
                if (File.Exists(state.Path)) File.Replace(temporary, state.Path, backup, true);
                else File.Move(temporary, state.Path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static bool ReadDefaultOn(JObject root, string key)
        {
            JToken token = root[key];
            if (token == null) return true;
            if (token.Type != JTokenType.Boolean)
                throw new JsonException("Feature-module key '" + key + "' must be boolean.");
            return token.Value<bool>();
        }

        private static bool ReadDefaultOff(JObject root, string key)
        {
            JToken token = root[key];
            if (token == null) return false;
            if (token.Type != JTokenType.Boolean)
                throw new JsonException("Feature-module key '" + key +
                    "' must be boolean.");
            return token.Value<bool>();
        }

        private static int RequireInteger(JToken token, string key)
        {
            if (token.Type != JTokenType.Integer)
                throw new JsonException("Feature-module key '" + key + "' must be integer.");
            return token.Value<int>();
        }

        private sealed class UnsupportedFeatureModuleSchemaException : JsonException
        {
            internal UnsupportedFeatureModuleSchemaException(string message) :
                base(message) { }
        }
    }
}
