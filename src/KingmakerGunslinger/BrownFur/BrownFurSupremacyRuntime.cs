using Kingmaker.UnitLogic.Abilities;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurSupremacyRuntime
    {
        private static readonly BrownFurSupremacyScopeTracker<AbilityData,
            AbilityExecutionContext> Scopes =
                new BrownFurSupremacyScopeTracker<AbilityData,
                    AbilityExecutionContext>();

        internal static int ActiveScopeCount
        { get { return Scopes.ActiveScopeCount; } }

        internal static bool Begin(string transactionIdentity,
            AbilityData ability)
        { return Scopes.Begin(transactionIdentity, ability); }

        internal static bool TryApply(AbilityData ability,
            AbilityExecutionContext context)
        {
            if (ability == null || context == null || context.Params == null)
                return false;
            bool addExtend;
            bool matched = Scopes.TryResolve(ability, context,
                context.Params.HasMetamagic(Metamagic.Extend), out addExtend);
            if (addExtend)
                context.Params.Metamagic |= Metamagic.Extend;
            return matched;
        }

        internal static int ModifiedContextCount(string transactionIdentity)
        { return Scopes.ModifiedContextCount(transactionIdentity); }

        internal static bool Release(string transactionIdentity)
        { return Scopes.Release(transactionIdentity); }

        internal static void Clear()
        { Scopes.Clear(); }
    }
}
