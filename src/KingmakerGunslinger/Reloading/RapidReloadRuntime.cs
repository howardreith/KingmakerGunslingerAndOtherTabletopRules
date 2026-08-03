using System;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal static class RapidReloadRuntime
    {
        private static readonly object Gate = new object();
        private static FirearmKind[] _kinds;
        private static BlueprintFeature[] _choices;

        internal static void Configure(FirearmKind[] kinds, BlueprintFeature[] choices)
        {
            if (kinds == null || choices == null || kinds.Length != choices.Length)
                throw new ArgumentException("Rapid Reload choice metadata is incomplete.");
            lock (Gate)
            {
                _kinds = (FirearmKind[])kinds.Clone();
                _choices = (BlueprintFeature[])choices.Clone();
            }
        }

        internal static bool HasMatchingChoice(UnitDescriptor unit, FirearmKind kind)
        {
            if (unit == null) return false;
            lock (Gate)
            {
                if (_kinds == null || _choices == null) return false;
                for (int i = 0; i < _kinds.Length; i++)
                    if (_kinds[i] == kind) return unit.HasFact(_choices[i]);
                return false;
            }
        }
    }
}
