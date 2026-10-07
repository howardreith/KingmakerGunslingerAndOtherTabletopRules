using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed descriptor/inventory policy shared by the request and its tests.
    internal static class ElementalCharacterTraitSaveContract
    {
        internal const string Scenario = "elemental-character-traits-owned-save";
        internal const string Seed = "KMG_AUTOMATION_WORKING";
        internal static bool ValidTransaction(string id)
        { return id != null && Regex.IsMatch(id, @"\A[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}\z"); }
        internal static string Name(string id)
        {
            if (!ValidTransaction(id)) throw new ArgumentException("Exact unpredictable transaction required.");
            return "KMG_TRAITS_0142_" + id;
        }
        internal static bool ValidPhase(string phase)
        { return phase == "prepare" || phase == "verify-remove" || phase == "verify-absent"; }
        internal static bool MatchesFile(string name, string file)
        { return name != null && file != null && Regex.IsMatch(file, @"\AManual_[0-9]+_" + Regex.Escape(name) + @"\.zks\z"); }
        internal static string InputName(string id, string phase)
        {
            if (!ValidPhase(phase)) throw new ArgumentException("Closed phase required.");
            return phase == "prepare" ? Seed : Name(id);
        }
        internal static bool MayWrite(string id, string phase, string name, bool exactLease, bool exactArtifact)
        { return ValidTransaction(id) && (phase == "prepare" || phase == "verify-remove") &&
            name == Name(id) && exactLease && exactArtifact; }
        internal static bool MayDelete(string id, string name, bool absentInitially, bool exactLease)
        { return ValidTransaction(id) && name == Name(id) && absentInitially && exactLease; }
        internal static T[] OwnedAreas<T>(IEnumerable<T> global,IEnumerable<T> attached) where T:class
        { return global.Concat(attached).Where(a=>a!=null).Distinct().ToArray(); }
        internal static bool MatchesProcessStart(DateTime actual,DateTime expected)
        { return actual.ToUniversalTime().Ticks==expected.ToUniversalTime().Ticks; }
        internal static bool LeaseUnexpired(DateTime now,DateTime expiry)
        { return expiry.ToUniversalTime()>now.ToUniversalTime(); }
        internal static bool MayPrepareNativeClone(int routines,bool firstPreparation,bool freshDescriptor,
            bool exactManualIdentity,bool noSaver,bool originalOwnedFileExact)
        { return routines==1 && firstPreparation && freshDescriptor && exactManualIdentity &&
            noSaver && originalOwnedFileExact; }
        internal static bool Next(string prior, string next, int priorPid, int currentPid)
        { return priorPid > 0 && currentPid > 0 && priorPid != currentPid &&
            ((prior == "prepare" && next == "verify-remove") || (prior == "verify-remove" && next == "verify-absent")); }
        internal static bool InventoryPreserved(IDictionary<string,string> before, IDictionary<string,string> after,
            string ownedPath, bool allowOwned)
        {
            if (before == null || after == null || (ownedPath != null && before.ContainsKey(ownedPath))) return false;
            return before.All(p => after.ContainsKey(p.Key) && after[p.Key] == p.Value) &&
                after.Keys.All(p => before.ContainsKey(p) || (allowOwned && p == ownedPath));
        }
        internal static bool ExactSaveWitness(IEnumerable<string> facts, IEnumerable<string> stable)
        { return facts != null && stable != null && facts.All(stable.Contains); }
    }
}
