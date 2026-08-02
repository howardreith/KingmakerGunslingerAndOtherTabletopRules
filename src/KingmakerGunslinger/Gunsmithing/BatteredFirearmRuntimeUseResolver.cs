using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmRuntimeUseResolver
    {
        private readonly KingmakerFirearmItemIdentityProvider _items =
            new KingmakerFirearmItemIdentityProvider();
        private readonly KingmakerBatteredFirearmOwnershipPartProvider _ownership =
            new KingmakerBatteredFirearmOwnershipPartProvider();

        internal BatteredFirearmUseDecision Evaluate(ItemEntityWeapon item,
            UnitEntityData user, FirearmCondition actualCondition,
            int ordinarySaleValueGold)
        {
            if (item == null) throw new ArgumentNullException("item");
            FirearmItemId itemId;
            string reason;
            if (!_items.TryGetIdentity(item, out itemId, out reason) || itemId == null)
                throw new InvalidOperationException(reason ??
                    "The firearm exposes no exact engine identity.");

            UnitPartBatteredFirearmOwnership part;
            OriginatingUnitId ownerId;
            if (!_ownership.TryGetExisting(out part) || part == null ||
                !part.TryGetOwner(itemId, out ownerId))
                return BatteredFirearmUsePolicy.Evaluate(false, false,
                    actualCondition, ordinarySaleValueGold);

            if (user == null || string.IsNullOrWhiteSpace(user.UniqueId))
                throw new InvalidOperationException(
                    "A battered firearm user exposes no stable Kingmaker unit identity.");
            var userId = new OriginatingUnitId(user.UniqueId.Trim());
            return BatteredFirearmUsePolicy.Evaluate(true,
                ownerId.Equals(userId), actualCondition, ordinarySaleValueGold);
        }
    }
}
