using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurReservoirDebit
    {
        internal static BrownFurReservoirDebitResult TryDebitExact(
            UnitDescriptor owner, BlueprintAbilityResource resource, int cost)
        {
            if (owner == null || resource == null)
                return new BrownFurReservoirDebitResult(false,
                    "reservoir-owner-or-resource-missing", -1, -1, -1,
                    false, false);
            return BrownFurExactDebitPolicy.TryDebitExact(cost,
                () => owner.Resources.ContainsResource(resource),
                () => owner.Resources.GetResourceAmount(resource),
                amount => owner.Resources.HasEnoughResource(resource, amount),
                amount => owner.Resources.Spend(resource, amount),
                amount => owner.Resources.Restore(resource, amount));
        }
    }
}
