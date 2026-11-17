using System;
using System.Runtime.CompilerServices;
using Kingmaker.RuleSystem.Rules;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.Archetypes
{
    /// <summary>
    /// REP-02 diagnostic: the individual d6 results the Up Close and Deadly
    /// handler drew for a miss, kept against the exact attack and its owner,
    /// so that an observer can compare the emitted fixed packet with half of
    /// the full roll without deriving the roll from the packet. Records only
    /// what the handler already drew, in the same order; it draws nothing and
    /// changes no outcome. Weak keys: completed attacks are not retained.
    /// </summary>
    internal static class UpCloseAndDeadlyDeliveryRecords
    {
        private sealed class MissRoll
        {
            internal string OwnerId;
            internal int[] Dice;
        }

        private static readonly object Gate = new object();
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, MissRoll> MissRolls =
            new ConditionalWeakTable<RuleAttackWithWeapon, MissRoll>();

        internal static void RecordMissRoll(RuleAttackWithWeapon attack, string ownerId, int[] dice)
        {
            if (attack == null || dice == null) return;
            try
            {
                lock (Gate)
                {
                    MissRolls.Remove(attack);
                    MissRolls.Add(attack, new MissRoll { OwnerId = ownerId, Dice = (int[])dice.Clone() });
                }
            }
            catch (Exception)
            {
                // A diagnostic record never affects the delivery.
            }
        }

        /// <summary>The recorded dice for this exact attack and owner, or null when none were recorded.</summary>
        internal static int[] MissRollFor(RuleAttackWithWeapon attack, string ownerId)
        {
            if (attack == null) return null;
            lock (Gate)
            {
                MissRoll value;
                if (!MissRolls.TryGetValue(attack, out value) ||
                    !string.Equals(value.OwnerId, ownerId, StringComparison.Ordinal)) return null;
                return (int[])value.Dice.Clone();
            }
        }

        /// <summary>Reflection-friendly read for the loader-compat probe: {"dice":[...]} or null.</summary>
        internal static string MissRollJson(object attack, string ownerId)
        {
            int[] dice = MissRollFor(attack as RuleAttackWithWeapon, ownerId);
            return dice == null ? null : new JObject { ["dice"] = new JArray(dice) }.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
