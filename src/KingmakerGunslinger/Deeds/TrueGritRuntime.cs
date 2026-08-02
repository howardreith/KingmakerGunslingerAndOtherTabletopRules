using System;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Deeds
{
    internal static class TrueGritRuntime
    {
        private static readonly TrueGritService Policy = new TrueGritService();

        internal static bool IsSelected(UnitDescriptor owner, TrueGritDeed deed)
        {
            if (owner == null || BlueprintBootstrap.GunslingerClass == null ||
                BlueprintBootstrap.GunslingerClass.TrueGrit == null) return false;
            return owner.HasFact(BlueprintBootstrap.GunslingerClass.TrueGrit
                .ChoiceFor(deed));
        }

        internal static TrueGritDecision Evaluate(UnitDescriptor owner,
            TrueGritDeed deed, int ordinaryCost, bool positiveGritNoSpend)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            int current = owner.Resources.GetResourceAmount(
                BlueprintBootstrap.GunslingerClass.Grit.Resource);
            return Policy.Evaluate(new TrueGritRequest(current, ordinaryCost,
                IsSelected(owner, deed), positiveGritNoSpend));
        }
    }
}
