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
            return Evaluate(definition, fastMusketAvailable, hasMatchingRapidReload, 0);
        }

        internal static EffectiveReloadAction Evaluate(
            FirearmDefinition definition, bool fastMusketAvailable,
            bool hasMatchingRapidReload, int ammunitionStepReduction)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (ammunitionStepReduction < 0 || ammunitionStepReduction > 3)
                throw new ArgumentOutOfRangeException("ammunitionStepReduction");
            EffectiveReloadAction action = Convert(definition.Reload.BaseAction);
            if (fastMusketAvailable && FirearmHandednessPolicy.Matches(
                    definition.Kind, FirearmHandedness.TwoHanded) &&
                action == EffectiveReloadAction.FullRound)
                action = EffectiveReloadAction.Standard;
            if (hasMatchingRapidReload) action = ReduceOneStep(action);
            for (int step = 0; step < ammunitionStepReduction; step++) action = ReduceOneStep(action);
            return action;
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
