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
            if (definition == null) throw new ArgumentNullException("definition");
            if (!hasMatchingRapidReload)
                return Convert(definition.Reload.BaseAction);
            if (definition.Era == FirearmEra.Advanced)
                return EffectiveReloadAction.Free;
            return definition.Reload.BaseAction == ReloadActionType.FullRound
                ? EffectiveReloadAction.Standard
                : EffectiveReloadAction.Move;
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
