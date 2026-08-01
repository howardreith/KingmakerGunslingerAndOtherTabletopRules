using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Grit
{
    /// <summary>
    /// Supplies the Wisdom portion of the Gunslinger grit maximum. The resource's
    /// base value is one, so this component adds only the amount above that floor.
    /// </summary>
    public sealed class GritResourceAmountBonus : OwnedGameLogicComponent<UnitDescriptor>,
        IResourceAmountBonusHandler, IUnitSubscriber
    {
        public BlueprintAbilityResource Resource;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            Fact fact = Fact;
            if (fact == null || !fact.Active || Resource == null || resource != Resource ||
                Owner == null || Owner.Stats == null)
                return;

            ModifiableValueAttributeStat wisdom =
                Owner.Stats.GetStat(StatType.Wisdom) as ModifiableValueAttributeStat;
            if (wisdom == null)
                return;

            int wisdomModifier = wisdom.Bonus;
            if (wisdomModifier > 1)
                bonus += wisdomModifier - 1;
        }
    }
}
