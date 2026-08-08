using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal enum EffectiveReloadAction
    {
        Unknown = 0,
        Free = 1,
        Move = 2,
        Standard = 3,
        FullRound = 4
    }

    internal static class ReloadActionEconomy
    {
        internal static EffectiveReloadAction Evaluate(
            FirearmDefinition definition, bool hasMatchingRapidReload)
        {
            return Evaluate(definition, false, hasMatchingRapidReload);
        }

        internal static EffectiveReloadAction Evaluate(
            FirearmDefinition definition, bool fastMusketAvailable,
            bool hasMatchingRapidReload)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            EffectiveReloadAction action = Convert(definition.Reload.BaseAction);
            if (fastMusketAvailable && FirearmHandednessPolicy.Matches(
                    definition.Kind, FirearmHandedness.TwoHanded) &&
                action == EffectiveReloadAction.FullRound)
                action = EffectiveReloadAction.Standard;
            return hasMatchingRapidReload ? ReduceOneStep(action) : action;
        }

        private static EffectiveReloadAction ReduceOneStep(
            EffectiveReloadAction action)
        {
            switch (action)
            {
                case EffectiveReloadAction.FullRound:
                    return EffectiveReloadAction.Standard;
                case EffectiveReloadAction.Standard:
                    return EffectiveReloadAction.Move;
                case EffectiveReloadAction.Move:
                case EffectiveReloadAction.Free:
                    return EffectiveReloadAction.Free;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }
        }

        private static EffectiveReloadAction Convert(ReloadActionType action)
        {
            switch (action)
            {
                case ReloadActionType.Move: return EffectiveReloadAction.Move;
                case ReloadActionType.Standard: return EffectiveReloadAction.Standard;
                case ReloadActionType.FullRound: return EffectiveReloadAction.FullRound;
                default: throw new ArgumentOutOfRangeException("action");
            }
        }
    }
}
