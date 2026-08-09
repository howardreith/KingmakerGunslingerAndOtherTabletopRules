using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Cord
{
    internal static class CordConditionRuntime
    {
        [System.ThreadStatic] private static UnitState _fatigueBypass;
        private static readonly ConditionalWeakTable<Buff, object> ExhaustionSources =
            new ConditionalWeakTable<Buff, object>();
        private static readonly object Marker = new object();

        internal static bool Prefix(UnitState state, UnitCondition condition, Buff source)
        {
            if (state == null || ReferenceEquals(_fatigueBypass, state)) return true;
            if (condition != UnitCondition.Fatigued &&
                condition != UnitCondition.Exhausted) return true;
            if (!HasExactEquippedCord(state)) return true;
            if (condition == UnitCondition.Fatigued && source != null &&
                ExhaustionSources.Remove(source)) return false;

            DealNonlethalEquivalent(state);
            if (condition == UnitCondition.Exhausted)
            {
                if (source != null)
                {
                    try { ExhaustionSources.Add(source, Marker); }
                    catch (System.ArgumentException) { }
                }
                try
                {
                    _fatigueBypass = state;
                    state.AddCondition(UnitCondition.Fatigued, source);
                }
                finally { _fatigueBypass = null; }
            }
            return false;
        }

        private static bool HasExactEquippedCord(UnitState state)
        {
            return state.Owner != null && state.Owner.Unit != null &&
                state.Owner.Unit.Body != null && state.Owner.Unit.Body.Belt != null &&
                state.Owner.Unit.Body.Belt.HasItem &&
                ReferenceEquals(state.Owner.Unit.Body.Belt.Item.Blueprint,
                    BlueprintBootstrap.CordOfStubbornResolve);
        }

        private static void DealNonlethalEquivalent(UnitState state)
        {
            int roll = Rulebook.Trigger(new RuleRollDice(state.Owner.Unit,
                new DiceFormula(1, DiceType.D6))).Result;
            int amount = System.Math.Min(roll,
                System.Math.Max(0, state.Owner.Unit.HPLeft - 1));
            if (amount == 0) return;
            var direct = new DirectDamage(new DiceFormula(0, DiceType.D6), amount);
            var damage = new RuleDealDamage(state.Owner.Unit, state.Owner.Unit,
                new DamageBundle(direct)) {
                    DisablePrecisionDamage = true,
                    IgnoreDamageReduction = true
                };
            Rulebook.Trigger(damage);
        }
    }

    [HarmonyPatch(typeof(UnitState), "AddCondition",
        typeof(UnitCondition), typeof(Buff))]
    internal static class CordAddConditionPatch
    {
        private static bool Prefix(UnitState __instance, UnitCondition condition,
            Buff sourceBuff)
        { return CordConditionRuntime.Prefix(__instance, condition, sourceBuff); }
    }
}
