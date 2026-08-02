using System;
using Kingmaker.Items;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Actions
{
    internal sealed class ExactEquippedFirearmContext
    {
        internal ExactEquippedFirearmContext(
            ItemEntityWeapon weapon,
            FirearmDefinition definition,
            FirearmItemStateSnapshot firearm,
            FirearmCondition effectiveCondition)
        {
            Weapon = weapon ?? throw new ArgumentNullException("weapon");
            Definition = definition ?? throw new ArgumentNullException("definition");
            Firearm = firearm ?? throw new ArgumentNullException("firearm");
            EffectiveCondition = effectiveCondition;
        }

        internal ItemEntityWeapon Weapon { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal FirearmItemStateSnapshot Firearm { get; private set; }
        internal FirearmCondition EffectiveCondition { get; private set; }
    }
}
