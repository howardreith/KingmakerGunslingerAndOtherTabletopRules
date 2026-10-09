using System;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed descriptor/inventory policy shared by the request and its tests.
    internal static class WeaponFindabilitySaveContract
    {
        internal const string Scenario = "weapon-findability-owned-save";
        internal const string Seed = "KMG_AUTOMATION_WORKING";
        internal static bool ValidTransaction(string id)
        { return id != null && Regex.IsMatch(id, @"\A[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}\z"); }
        internal static string Name(string id)
        {
            if (!ValidTransaction(id)) throw new ArgumentException("Exact unpredictable transaction required.");
            return "KMG_WEAPONS_0143_" + id;
        }
        internal static bool ValidPhase(string phase)
        { return phase == "prepare" || phase == "verify"; }
        internal static bool MatchesFile(string name, string file)
        { return name != null && file != null && Regex.IsMatch(file, @"\AManual_[0-9]+_" + Regex.Escape(name) + @"\.zks\z"); }
        internal static string InputName(string id, string phase)
        {
            if (!ValidPhase(phase)) throw new ArgumentException("Closed phase required.");
            return phase == "prepare" ? Seed : Name(id);
        }
        internal static bool MayWrite(string id, string phase, string name, bool exactLease, bool exactArtifact)
        { return ValidTransaction(id) && phase == "prepare" &&
            name == Name(id) && exactLease && exactArtifact; }
        internal static bool MayDelete(string id, string name, bool absentInitially, bool exactLease)
        { return ValidTransaction(id) && name == Name(id) && absentInitially && exactLease; }
        internal static bool MatchesProcessStart(DateTime actual,DateTime expected)
        { return actual.ToUniversalTime().Ticks==expected.ToUniversalTime().Ticks; }
        internal static bool LeaseUnexpired(DateTime now,DateTime expiry)
        { return expiry.ToUniversalTime()>now.ToUniversalTime(); }
        internal static bool Next(string prior, string next, int priorPid, int currentPid)
        { return priorPid > 0 && currentPid > 0 && priorPid != currentPid &&
            (prior == "prepare" && next == "verify"); }
    }
}
