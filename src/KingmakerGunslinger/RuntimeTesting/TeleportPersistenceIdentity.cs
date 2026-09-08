using System;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class TeleportPersistenceIdentity
    {
        internal const string Scenario = "disposable-teleportation-persistence";
        internal const string Working = "KMG_AUTOMATION_WORKING";
        internal static bool ValidTransaction(string value)
        { return value != null && Regex.IsMatch(value, @"\A[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}\z"); }
        internal static bool ValidPhase(string phase)
        { return phase == "A" || phase == "B" || phase == "C" || phase == "D"; }
        internal static string Name(string transaction, string phase)
        {
            if (!ValidTransaction(transaction) || !ValidPhase(phase) || phase == "D")
                throw new ArgumentException("Unproven disposable persistence identity.");
            return "KMG_TELEPORT_PERSISTENCE_" + transaction + "_" + phase;
        }
        internal static string InputName(string transaction, string phase)
        {
            if (!ValidTransaction(transaction) || !ValidPhase(phase))
                throw new ArgumentException("Unproven persistence phase.");
            return phase == "A" ? Working : Name(transaction, phase == "B" ? "A" : phase == "C" ? "B" : "C");
        }
        internal static bool MatchesFile(string name, string file)
        {
            return name != null && file != null && Regex.IsMatch(file,
                @"\AManual_[0-9]+_" + Regex.Escape(name) + @"\.zks\z");
        }
    }
}
