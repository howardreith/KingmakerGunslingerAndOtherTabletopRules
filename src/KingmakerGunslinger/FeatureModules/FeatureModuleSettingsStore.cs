using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.FeatureModules
{
    internal static class FeatureModuleSettingsStore
    {
        internal const int CurrentSchemaVersion = 3;
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
                if (schema < 0 || schema > CurrentSchemaVersion)
                    throw new JsonException("Unsupported feature-module settings schema " + schema + ".");
                bool gunslinger = ReadDefaultOn(root, FeatureModuleConfiguration.GunslingerId);
                bool acadamae = ReadDefaultOn(root,
                    FeatureModuleConfiguration.AcadamaeGraduateId);
                bool shieldOther = ReadDefaultOn(root,
                    FeatureModuleConfiguration.ShieldOtherId);
                bool expandedSummoning = ReadDefaultOn(root,
                    FeatureModuleConfiguration.ExpandedSummoningId);
                var state = new FeatureModuleSettingsState(
                    new FeatureModuleConfiguration(gunslinger, acadamae,
                        shieldOther, expandedSummoning), path,
                    schema < CurrentSchemaVersion ? "migrated-schema-" + schema :
                        "settings", false);
                if (schema < CurrentSchemaVersion) Save(state);
                return state;
            }
            catch (Exception exception)
            {
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
                    path + "; all modules default ON; " + evidence + "; error=" +
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
                    state.Pending.ExpandedSummoning
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

        private static int RequireInteger(JToken token, string key)
        {
            if (token.Type != JTokenType.Integer)
                throw new JsonException("Feature-module key '" + key + "' must be integer.");
            return token.Value<int>();
        }
    }
}
