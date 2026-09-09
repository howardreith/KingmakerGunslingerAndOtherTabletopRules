using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalNereidResponse { Unseen = 0, Affected = 1, Resisted = 2, Interrupted = 3 }
    internal enum ElementalNereidEntry { Ignore, Save, ApplyRemaining }

    internal static class ElementalNereidPolicy
    {
        internal const int RadiusFeet = 20;
        internal static int HalfLevel(int totalLevel) { return Math.Max(0, totalLevel) / 2; }
        internal static int DurationRounds(int totalLevel) { return Math.Max(1, HalfLevel(totalLevel)); }
        internal static int DifficultyClass(int totalLevel, int currentCharismaModifier)
        { return 10 + HalfLevel(totalLevel) + currentCharismaModifier; }

        internal static ElementalNereidEntry Enter(ElementalNereidResponse response, bool eligible, bool unexpired)
        {
            if (!eligible || !unexpired) return ElementalNereidEntry.Ignore;
            switch (response)
            {
                case ElementalNereidResponse.Unseen: return ElementalNereidEntry.Save;
                case ElementalNereidResponse.Affected: return ElementalNereidEntry.ApplyRemaining;
                default: return ElementalNereidEntry.Ignore;
            }
        }

        internal static Dictionary<string, int> RestoreResponses(IEnumerable<KeyValuePair<string, int>> saved)
        {
            if (saved == null) throw new ArgumentNullException("saved");
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var entry in saved)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || result.ContainsKey(entry.Key))
                    throw new ArgumentException("Owned Nereid responses require unique, nonempty unit identities.", "saved");
                // Preserve future unknown values: Enter ignores them, instead of
                // granting a fresh save or turning them into an affected target.
                result.Add(entry.Key, entry.Value);
            }
            return result;
        }

        internal const string Description = "Once per ordinary rest, as a standard action, create a " +
            "20-foot-radius supernatural aura centered on yourself for half your total character level in rounds " +
            "(minimum 1). Other humanoids, including allies, within native line of effect attempt one Will save " +
            "(DC 10 + half your total character level + your current Charisma modifier) when first exposed. " +
            "A failed save fascinates them while inside the aura. Leaving suspends the effect; returning does not " +
            "grant a new save or extend its original expiration. A successful save or an interruption prevents " +
            "this activation from fascinating that creature again. Obvious perceived threats break fascination; " +
            "a perceived hostile approach permits a new save. Sound alone does not trigger threat interruption. " +
            "An ally can use Shake Free at touch range as a " +
            "standard action. Fascination uses Kingmaker's native Fascinate condition behavior, not paralysis, " +
            "stun or control of the target.";
    }
}
