using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmOwnershipRecord
    {
        internal BatteredFirearmOwnershipRecord(FirearmItemId itemId,
            OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            ItemId = itemId.Value;
            OwnerId = ownerId.Value;
        }

        internal string ItemId { get; private set; }
        internal string OwnerId { get; private set; }

        internal BatteredFirearmOwnershipRecord Clone()
        {
            return new BatteredFirearmOwnershipRecord(
                new FirearmItemId(ItemId), new OriginatingUnitId(OwnerId));
        }
    }
}
