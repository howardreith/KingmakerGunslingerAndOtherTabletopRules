using System;
using System.Globalization;
using System.Reflection;
using UnityModManagerNet;

namespace KingmakerGunslinger.Bootstrap
{
    /// <summary>
    /// Small structured-log adapter around Unity Mod Manager's logger.
    /// </summary>
    internal sealed class ModLogger
    {
        private readonly UnityModManager.ModEntry.ModLogger _logger;
        private readonly string _modId;
        private readonly string _version;

        private ModLogger(
            UnityModManager.ModEntry.ModLogger logger,
            string modId,
            string version)
        {
            _logger = logger;
            _modId = modId;
            _version = version;
        }

        internal static ModLogger Create(UnityModManager.ModEntry modEntry, Assembly assembly)
        {
            if (modEntry == null)
            {
                throw new ArgumentNullException("modEntry");
            }

            if (modEntry.Logger == null)
            {
                throw new InvalidOperationException("Unity Mod Manager did not provide a logger.");
            }

            if (modEntry.Info == null || string.IsNullOrWhiteSpace(modEntry.Info.Id))
            {
                throw new InvalidOperationException("Unity Mod Manager did not provide a valid mod ID.");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            return new ModLogger(modEntry.Logger, modEntry.Info.Id, GetVersion(assembly));
        }

        internal void Info(string phase, string eventName, string message)
        {
            Write("INFO", phase, eventName, message);
        }

        internal void Debug(string phase, string eventName, string message)
        {
            Write("DEBUG", phase, eventName, message);
        }

        internal void Warning(string phase, string eventName, string message)
        {
            Write("WARN", phase, eventName, message);
        }

        internal void Failure(string phase, string eventName, string message, Exception exception)
        {
            string detail = message;
            if (exception != null)
            {
                detail = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} Exception={1}: {2}",
                    message,
                    exception.GetType().FullName,
                    exception.Message);
            }

            Write("ERROR", phase, eventName, detail);

            if (exception != null && !string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                Write("TRACE", phase, eventName + ".stack", exception.StackTrace);
            }
        }

        private void Write(string level, string phase, string eventName, string message)
        {
            try
            {
                _logger.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[KMG][{0}][{1}][{2}][{3}][{4}] {5}",
                    _modId,
                    _version,
                    level,
                    NormalizeToken(phase),
                    NormalizeToken(eventName),
                    NormalizeMessage(message)));
            }
            catch
            {
                // Logging is diagnostic; it must never destabilize the game loader.
            }
        }

        private static string GetVersion(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute attribute = attributes[0] as AssemblyInformationalVersionAttribute;
                if (attribute != null && !string.IsNullOrWhiteSpace(attribute.InformationalVersion))
                {
                    return attribute.InformationalVersion;
                }
            }

            Version version = assembly.GetName().Version;
            return version == null ? "unknown" : version.ToString();
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unspecified";
            }

            return value.Trim().Replace(" ", "-").Replace("[", "(").Replace("]", ")");
        }

        private static string NormalizeMessage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "(no message)";
            }

            return value
                .Replace("\r\n", " | ")
                .Replace("\n", " | ")
                .Replace("\r", " | ")
                .Trim();
        }
    }
}
