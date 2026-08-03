using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Firing
{
    internal enum EmptyFirearmCommandDisposition
    {
        Allow = 1,
        RejectUnloaded = 2,
        RejectWrecked = 3,
        RejectAmbiguous = 4,
        QueueReload = 5
    }

    internal static class EmptyFirearmAttackPolicy
    {
        internal static EmptyFirearmCommandDisposition Evaluate(
            bool hasExactFirearm, bool ambiguousFirearms,
            FirearmState state, bool autoReloadEnabled,
            bool reloadIsLegal)
        {
            if (ambiguousFirearms)
                return EmptyFirearmCommandDisposition.RejectAmbiguous;
            if (!hasExactFirearm) return EmptyFirearmCommandDisposition.Allow;
            if (state == null) throw new ArgumentNullException("state");
            if (state.Condition == FirearmCondition.Wrecked)
                return EmptyFirearmCommandDisposition.RejectWrecked;
            if (!state.IsEmpty) return EmptyFirearmCommandDisposition.Allow;
            return autoReloadEnabled && reloadIsLegal
                ? EmptyFirearmCommandDisposition.QueueReload
                : EmptyFirearmCommandDisposition.RejectUnloaded;
        }
    }
}
