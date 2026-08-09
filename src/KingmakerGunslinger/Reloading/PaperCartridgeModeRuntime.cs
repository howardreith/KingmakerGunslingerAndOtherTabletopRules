using System.Linq;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Reloading
{
    internal static class PaperCartridgeModeRuntime
    {
        internal static bool IsActive(UnitDescriptor unit, BlueprintBuff marker)
        {
            return unit != null && marker != null && unit.Buffs != null &&
                unit.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    value != null && ReferenceEquals(value.Blueprint, marker));
        }
    }
}
