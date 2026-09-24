using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// Critical Focus's native contribution to one attack's confirmation
    /// roll, computed exactly as its own CriticalConfirmationBonus
    /// components compute it (zero without the active feat), so an earned
    /// nonstacking confirmation bonus adds only its excess.
    /// </summary>
    internal static class FavoredClassCriticalFocus
    {
        internal static int Contribution(UnitDescriptor owner, BlueprintFeature criticalFocus,
            RuleAttackRoll evt)
        {
            if (owner == null || criticalFocus == null || evt == null || evt.Weapon == null)
                return 0;
            var feature = owner.Progression.Features.GetFact(criticalFocus) as Feature;
            if (feature == null || !feature.Active)
                return 0;
            int total = 0;
            foreach (CriticalConfirmationBonus component in
                criticalFocus.ComponentsArray.OfType<CriticalConfirmationBonus>())
            {
                int value = component.Value.Calculate(feature.Context) + component.Bonus;
                bool range = !component.CheckWeaponRangeType ||
                    AttackTypeAttackBonus.CheckRangeType(evt.Weapon.Blueprint, component.Type);
                if ((!component.OnlyPositiveValue || value > component.Bonus) && range)
                    total = checked(total + value);
            }
            return total;
        }
    }
}
