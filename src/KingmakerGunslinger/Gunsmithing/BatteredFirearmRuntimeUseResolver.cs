using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmRuntimeUseResolver
    {
        internal BatteredFirearmUseDecision Evaluate(ItemEntityWeapon item,
            UnitEntityData user, FirearmCondition actualCondition,
            int ordinarySaleValueGold)
        {
            if (item == null) throw new ArgumentNullException("item");
            UnitEntityData owner;
            if (!BatteredFirearmOriginRuntime.TryGetOwner(item, out owner))
                return BatteredFirearmUsePolicy.Evaluate(false, false,
                    actualCondition, ordinarySaleValueGold);

            if (user == null || string.IsNullOrWhiteSpace(user.UniqueId))
                throw new InvalidOperationException(
                    "A battered firearm user exposes no stable Kingmaker unit identity.");
            return BatteredFirearmUsePolicy.Evaluate(true,
                BatteredFirearmOriginRuntime.SameStableOwner(owner, user),
                actualCondition,
                ordinarySaleValueGold);
        }
    }
}
