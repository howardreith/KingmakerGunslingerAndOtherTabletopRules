using System;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class FcbPersistenceIdentity
    {
        internal const string Scenario =
            "disposable-word-of-recall-favored-class-persistence";
        internal const string Working = "KMG_AUTOMATION_WORKING";
        internal static bool ValidTransaction(string value)
        { return value != null && Regex.IsMatch(value, @"\A[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}\z"); }
        internal static bool ValidPhase(string phase)
        { return phase == "prepare" || phase == "verify"; }
        internal static string Name(string transaction, string phase)
        {
            if (!ValidTransaction(transaction) || !ValidPhase(phase))
                throw new ArgumentException("Unproven Favored Class persistence identity.");
            return "KMG_FCB_PERSISTENCE_" + transaction + "_" + phase;
        }
        internal static bool MatchesFile(string name, string file)
        {
            return name != null && file != null && Regex.IsMatch(file,
                @"\AManual_[0-9]+_" + Regex.Escape(name) + @"\.zks\z");
        }
    }
}
