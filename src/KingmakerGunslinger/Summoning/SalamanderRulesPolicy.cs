using System;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Summoning
{
    // Printed racial contributions, not final-stat overrides. Native ability,
    // feat, template, equipment and temporary modifiers remain live.
    internal static class SalamanderRulesPolicy
    {
        internal const string UnitGuid = "f8fb103168d74b4c93182437e5d2b4e4";
        internal const string UnitName = "KMG_Summoning_Unit_Salamander";
        internal const string TailGuid = "93e097b8d3db42d3a37656502899e1a9";
        internal const int BaseHitPoints = 44;
        internal const int GoodSave = 6;
        internal const int PoorSave = 2;
        internal const int PerceptionRanks = 8;
        internal const int TailReachFeet = 10;

        internal static bool IsOwner(string guid, string name)
        { return guid == UnitGuid && name == UnitName; }

        internal static void AllocateLandRanks(ref int mobility, ref int perception, ref int persuasion)
        {
            if (mobility != 0 || perception != 0 || persuasion != 0)
                throw new InvalidOperationException("Salamander racial ranks must start unallocated.");
            perception = PerceptionRanks;
        }

        internal static int ConstrictStrengthBonus(int liveStrengthBonus)
        { return liveStrengthBonus + Math.Max(0, liveStrengthBonus) / 2; }
    }

    // Duplicate subscribers/re-entrant delivery cannot add the same heat
    // packet twice. Each new weapon-stat calculation gets its own packet;
    // an existing flaming enchantment is neither removed nor mistaken for it.
    internal sealed class SalamanderHeatClaims
    {
        private readonly ConditionalWeakTable<object, object> _rules =
            new ConditionalWeakTable<object, object>();
        internal bool TryClaim(object rule, bool exactOwner, bool exactWeapon)
        {
            if (rule == null || !exactOwner || !exactWeapon) return false;
            lock (_rules)
            {
                object previous;
                if (_rules.TryGetValue(rule, out previous)) return false;
                _rules.Add(rule, new object());
                return true;
            }
        }
    }
}
