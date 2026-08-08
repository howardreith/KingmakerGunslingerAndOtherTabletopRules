using System;
using System.Runtime.CompilerServices;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Rules
{
    internal static class EffectiveFirearmRangeRuntime
    {
        private sealed class Context
        {
            internal Context(int bonusFeet) { BonusFeet = bonusFeet; }
            internal int BonusFeet { get; private set; }
        }

        private static readonly ConditionalWeakTable<RuleAttackRoll, Context>
            Contexts = new ConditionalWeakTable<RuleAttackRoll, Context>();

        internal static bool Register(RuleAttackRoll attackRoll, int bonusFeet)
        {
            if (attackRoll == null) throw new ArgumentNullException("attackRoll");
            if (bonusFeet <= 0) throw new ArgumentOutOfRangeException("bonusFeet");
            lock (Contexts)
            {
                Context existing;
                if (Contexts.TryGetValue(attackRoll, out existing)) return false;
                Contexts.Add(attackRoll, new Context(bonusFeet));
                return true;
            }
        }

        internal static int GetBonusFeet(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return 0;
            lock (Contexts)
            {
                Context context;
                return Contexts.TryGetValue(attackRoll, out context)
                    ? context.BonusFeet : 0;
            }
        }
    }
}
