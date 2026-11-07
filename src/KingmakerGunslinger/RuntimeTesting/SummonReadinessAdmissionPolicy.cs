using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Repair R2 LIFE-01: the closed request scope for the matched readiness
    /// cases on the existing same-turn Acadamae / ordinary-control scenarios.
    /// <c>readinessCreature</c> names one of four reported creatures;
    /// <c>combatEntry=surprise</c> is admitted only with an Acadamae creature
    /// case and asks the fixture to open combat through a real party attack so
    /// the engine itself decides surprise. Pure, so admission is tested.
    /// </summary>
    internal static class SummonReadinessAdmissionPolicy
    {
        internal const string SurpriseEntry = "surprise";
        internal static readonly string[] Creatures =
            { "pteranodon", "fire-mephit", "lion", "hell-hound" };

        /// <summary>Null when admitted; otherwise the refusal code.</summary>
        internal static string Refusal(bool acadamae, bool nativeControl,
            bool creaturePresent, string creature, bool combatEntryPresent,
            string combatEntry)
        {
            if (creaturePresent && (!acadamae && !nativeControl ||
                    creature == null || !Creatures.Contains(creature, StringComparer.Ordinal)))
                return "readiness-creature-invalid";
            if (combatEntryPresent && (!acadamae || !creaturePresent ||
                    !string.Equals(combatEntry, SurpriseEntry, StringComparison.Ordinal)))
                return "readiness-combat-entry-invalid";
            return null;
        }

        /// <summary>Exact parameter count including saveName.</summary>
        internal static int ParameterCount(bool creaturePresent, bool combatEntryPresent)
        {
            return 1 + (creaturePresent ? 1 : 0) + (combatEntryPresent ? 1 : 0);
        }
    }
}
