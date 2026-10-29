using System;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalRules34
    {
        internal const int WoundBleed = 2;
        internal const int WoundSpellCureDc = 16;
        internal const int ChainReachFeet = 10;
        internal const int ControlledChainReachFeet = 25;
        internal const int GazeRangeFeet = 30;

        internal static int BeardStrengthBonus(int strength)
        { return strength > 0 ? strength / 2 : strength; }

        internal static int BeardDiseaseDc(int constitutionBonus)
        { return 10 + 6 / 2 + constitutionBonus; }

        internal static int GazeDc(int charismaBonus)
        { return 10 + 8 / 2 + charismaBonus; }
    }

    // The existing tracker owns the same two-distinct-claws/target/command
    // condition. Beard calls it AFTER a real hit, rather than setting IsRend
    // on a pending attack. Event claims reject duplicate notifications even
    // after the command has changed; this state is never serialized.
    internal sealed class BarbazuBeardSequence34
    {
        private readonly ClawRendTracker claws = new ClawRendTracker(2);
        private readonly ConditionalWeakTable<object, object> events =
            new ConditionalWeakTable<object, object>();

        internal bool Record(object rule, object startedCommand,
            object currentCommand, int claw, object target, bool hit)
        {
            if (rule == null) return false;
            object previous;
            if (events.TryGetValue(rule, out previous)) return false;
            events.Add(rule, new object());
            if (startedCommand == null ||
                !ReferenceEquals(startedCommand, currentCommand)) return false;
            if (!hit) {
                claws.RecordOutcome(startedCommand, claw, target, false);
                return false;
            }
            bool beard = claws.TryArm(startedCommand, claw, target);
            claws.RecordOutcome(startedCommand, claw, target, true);
            return beard;
        }

        internal void Reset() { claws.Reset(); }
    }
}
