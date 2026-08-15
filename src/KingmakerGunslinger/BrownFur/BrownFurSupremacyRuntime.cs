using System;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurSupremacyRuntime
    {
        private const string ResonatingWordGuid =
            "df7d13c967bce6a40bec3ba7c9f0e64c";
        private const string ObsidianFlowGuid =
            "e48638596c955a74c8a32dbc90b518c1";
        private static readonly string[] EarthTremorManualAdapterGuids = {
            "91266b6d2a4cfd6b8e1549bc2381d12",
        };

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

        internal static bool TryDoubleNonstandardDuration(
            ContextDurationValue duration, MechanicsContext context,
            ref Rounds result)
        {
            if (duration == null || context == null ||
                context.SourceAbilityContext == null ||
                !Scopes.WasModified(context.SourceAbilityContext) ||
                context.SourceAbility == null || context.SpellSchool !=
                    SpellSchool.Transmutation) return false;
            string guid = context.SourceAbility.AssetGuid;
            bool supported = string.Equals(guid, ResonatingWordGuid,
                    StringComparison.Ordinal) && duration.Rate ==
                    DurationRate.Rounds ||
                string.Equals(guid, ObsidianFlowGuid,
                    StringComparison.Ordinal) && duration.Rate ==
                    DurationRate.Hours ||
                string.Equals(guid, EarthTremorManualAdapterGuids[0],
                    StringComparison.Ordinal) && duration.Rate ==
                    DurationRate.Hours;
            if (!supported) return false;
            result = result * 2;
            return true;
        }

        internal static bool Release(string transactionIdentity)
        { return Scopes.Release(transactionIdentity); }

        internal static void Clear()
        { Scopes.Clear(); }
    }
}
