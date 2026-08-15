using System.Collections.Generic;

namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurCastSourceKind
    {
        Unknown = 0,
        Spellbook = 1,
        Item = 2,
        SpellLike = 3,
        Supernatural = 4
    }

    internal enum BrownFurOriginalRange
    {
        Unknown = 0,
        Personal = 1,
        Touch = 2,
        Other = 3
    }

    internal enum BrownFurDurationKind
    {
        Unknown = 0,
        Instantaneous = 1,
        Permanent = 2,
        Timed = 3
    }

    internal sealed class BrownFurCastRequest
    {
        internal bool CasterOwnsBrownFur { get; set; }
        internal bool IsGenuineSpell { get; set; }
        internal bool IsTransmutation { get; set; }
        internal BrownFurCastSourceKind SourceKind { get; set; }
        internal bool UsesArcanistSpellSlot { get; set; }
        internal bool HasPowerfulChange { get; set; }
        internal bool HasPowerfulChangeCapstone { get; set; }
        internal bool HasShareTransmutation { get; set; }
        internal bool HasShareThirtyFootCapstone { get; set; }
        internal bool HasTransmutationSupremacy { get; set; }
        internal bool PowerfulChangeRequested { get; set; }
        internal BrownFurAbilityScore SelectedAbilityScore { get; set; }
        internal ISet<BrownFurAbilityScore> PositiveAbilityBonuses { get; set; }
        internal bool BonusAdapterAvailable { get; set; }
        internal bool ShareTransmutationRequested { get; set; }
        internal BrownFurOriginalRange OriginalRange { get; set; }
        internal BrownFurShareTargetRequest ShareTarget { get; set; }
        internal bool TargetAdapterAvailable { get; set; }
        internal BrownFurDurationKind DurationKind { get; set; }
        internal bool AlreadyExtended { get; set; }
        internal bool DurationAdapterAvailable { get; set; }
        internal int ReservoirPoints { get; set; }
    }
}
