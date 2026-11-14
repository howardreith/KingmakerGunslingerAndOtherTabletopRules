using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Opens the loader's window and draws a mod's registered OnGUI on ordinary IMGUI passes, the way the window does
    /// for its selected mod. A pass counts only if the mod's callback threw nothing and the loader logged no exception
    /// for any mod while the probe ran. Adapted from the Kingmaker Cutscene Skip lab harness (SettingsDrawProbe).
    /// </summary>
    internal sealed class PanelProbe : MonoBehaviour
    {
        public string ModId { get; set; }

        public int Draws { get; private set; }

        public int Repaints { get; private set; }

        public string Failure { get; private set; }

        public int Calls { get; private set; }

        public int ExitGui { get; private set; }

        private void OnGUI()
        {
            Calls++;
            UnityModManager.ModEntry entry = UnityModManager.modEntries.SingleOrDefault(e => e.Info != null && e.Info.Id == ModId);
            if (entry?.OnGUI == null)
            {
                Failure = ModId + " has no OnGUI";
                return;
            }

            try
            {
                GUILayout.BeginArea(new Rect(20, 20, 900, 700));
                entry.OnGUI(entry);
                GUILayout.EndArea();
                if (LoggedExceptions.First != null) return;
                Draws++;
                if (Event.current.type == EventType.Repaint) Repaints++;
            }
            catch (Exception exception)
            {
                if (exception is ExitGUIException) { ExitGui++; throw; }
                if (Failure == null) Failure = exception.ToString();
            }
        }
    }

    /// <summary>
    /// Records the first exception any mod or the loader reports through UMM's loggers while armed. Mods catch and log
    /// their own GUI failures, so a failure need not reach the caller.
    /// </summary>
    internal static class LoggedExceptions
    {
        private static bool armed;

        public static string First { get; private set; }

        public static int Patched { get; private set; }

        public static void Install(HarmonyInstance harmony)
        {
            const BindingFlags Own = BindingFlags.Static | BindingFlags.NonPublic;
            foreach (Type type in new[] { typeof(UnityModManager.Logger), typeof(UnityModManager.ModEntry.ModLogger) })
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.Name == "LogException" && m.DeclaringType == type))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    string prefix = parameters.Length == 1 && parameters[0].ParameterType == typeof(Exception) ? nameof(PrefixException)
                        : parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(Exception) ? nameof(PrefixKeyException)
                        : null;
                    if (prefix == null) continue;
                    harmony.Patch(method, new HarmonyMethod(typeof(LoggedExceptions).GetMethod(prefix, Own)), null);
                    Patched++;
                }
            }
        }

        public static void Arm()
        {
            First = null;
            armed = true;
        }

        public static void Disarm() => armed = false;

        private static void PrefixException(Exception __0) => Record("LogException: " + __0);

        private static void PrefixKeyException(string __0, Exception __1) => Record("LogException(" + __0 + "): " + __1);

        private static void Record(string text)
        {
            if (armed && First == null) First = text;
        }
    }
}
