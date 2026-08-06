using System;
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
        public StatType Attribute = StatType.Wisdom;
        public int Minimum = 1;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            Fact fact = Fact;
            if (fact == null || !fact.Active || Resource == null || resource != Resource ||
                Owner == null || Owner.Stats == null)
                return;

            ModifiableValueAttributeStat stat =
                Owner.Stats.GetStat(Attribute) as ModifiableValueAttributeStat;
            if (stat == null)
                return;

            // For the base configuration this remains exactly wisdomModifier - 1;
            // archetypes may select another attribute and floor explicitly.
            int modifier = Math.Max(Minimum, stat.Bonus);
            if (modifier > Minimum)
                bonus += modifier - Minimum;
        }
    }
}
