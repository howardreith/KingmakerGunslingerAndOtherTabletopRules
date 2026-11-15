using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.Localization;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Minimal manifest-time localization adapter for custom blueprint text. Kingmaker
    /// replaces localization packs when the process starts, so registration is repeated
    /// during each blueprint bootstrap. Existing identical values are accepted; conflicting
    /// values fail closed rather than silently replacing another mod's key.
    /// </summary>
    internal static class LocalizationService
    {
        private const BindingFlags InstanceFields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFields =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo KeyField =
            typeof(LocalizedString).GetField("m_Key", InstanceFields);
        private static readonly FieldInfo ShouldProcessField =
            typeof(LocalizedString).GetField("m_ShouldProcess", InstanceFields);
        private static readonly FieldInfo CurrentPackField =
            typeof(LocalizationManager).GetField("CurrentPack", StaticFields);
        private static readonly FieldInfo CurrentPackFastField =
            typeof(LocalizationManager).GetField("CurrentPackFast", StaticFields);

        internal static LocalizedString Create(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A localization key is required.", "key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (KeyField == null || KeyField.FieldType != typeof(string))
            {
                throw new MissingFieldException(
                    typeof(LocalizedString).FullName,
                    "m_Key");
            }

            int registeredPackCount = 0;
            registeredPackCount += RegisterInPack(CurrentPackField, key, value);
            registeredPackCount += RegisterInPack(CurrentPackFastField, key, value);
            if (registeredPackCount == 0)
            {
                throw new InvalidOperationException(
                    "Kingmaker's current localization pack is unavailable during blueprint initialization.");
            }

            var result = new LocalizedString();
            KeyField.SetValue(result, key);
            if (ShouldProcessField != null && ShouldProcessField.FieldType == typeof(bool))
            {
                ShouldProcessField.SetValue(result, false);
            }

            if (!string.Equals(result.Key, key, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The localized-string key could not be assigned exactly.");
            }

            return result;
        }

        /// <summary>
        /// What this process displays for a localized string that was never
        /// stored. Kingmaker 2.1.7b returns "&lt;null&gt;"; another mod or a
        /// modified conversion may return something else, including nothing.
        /// </summary>
        internal static string ResolveUnassigned()
        {
            LocalizedString unassigned = null;
            return unassigned;
        }

        /// <summary>
        /// One-line localization context for a diagnostic record: the active
        /// locale and how unassigned text resolves in this process.
        /// </summary>
        internal static string DescribeEnvironment()
        {
            try
            {
                LocalizationPack pack = CurrentPackField == null ? null :
                    CurrentPackField.GetValue(null) as LocalizationPack;
                string unassigned = ResolveUnassigned();
                return "locale=" + LocalizationManager.CurrentLocale +
                    ";pack-locale=" + (pack == null ? "<none>" : pack.Locale.ToString()) +
                    ";unassigned-text-resolves-to=" +
                    (unassigned == null ? "<null-reference>" : "\"" + unassigned + "\"");
            }
            catch (Exception exception)
            {
                return "localization-environment-unavailable=" +
                    exception.GetType().Name;
            }
        }

        private static int RegisterInPack(
            FieldInfo packField,
            string key,
            string value)
        {
            if (packField == null || !typeof(LocalizationPack).IsAssignableFrom(packField.FieldType))
            {
                return 0;
            }

            LocalizationPack pack = packField.GetValue(null) as LocalizationPack;
            if (pack == null)
            {
                return 0;
            }

            if (pack.Strings == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker's current localization pack has no string dictionary.");
            }

            string existing;
            if (pack.Strings.TryGetValue(key, out existing))
            {
                if (!string.Equals(existing, value, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Localization key collision for '" + key + "'.");
                }

                return 1;
            }

            pack.Strings.Add(key, value);
            return 1;
        }
    }
}
