using System;
using System.Globalization;

namespace KingmakerGunslinger.Spells.Teleportation
{
    [Flags]
    internal enum TeleportCastBlock
    {
        None = 0, ModuleDisabled = 1, NotWorldMap = 2, MissingCanonicalParty = 4,
        UnanchoredOrigin = 8, Combat = 16, Dialogue = 32, Cutscene = 64,
        Loading = 128, AreaTransition = 256, Encounter = 512, KingdomOperation = 1024,
        Moving = 2048, AdvancingTravel = 4096, RelocationPending = 8192, UnknownState = 16384
    }
    internal enum TeleportCastSourceKind { Unknown, Prepared, Spontaneous }
    [Flags]
    internal enum TeleportCastSourceFacts
    {
        None = 0, ActiveParty = 1, LivingAvailableCaster = 2, OwnedSpellbook = 4,
        BookUsable = 8, ExactSpell = 16, RealResource = 32, Known = 64,
        PreparedUse = 128, Item = 256, Metamagic = 512, Synthetic = 1024,
        Required = ActiveParty | LivingAvailableCaster | OwnedSpellbook | BookUsable | ExactSpell | RealResource
    }
    internal sealed class TeleportCastSourceSnapshot
    {
        internal TeleportCastSourceSnapshot(string casterId, int partyOrder, string casterName,
            string bookId, string bookName, TeleportSpellKind spell, TeleportCastSourceKind kind,
            int spellLevel, int uses, TeleportCastSourceFacts facts)
        { CasterId = casterId; PartyOrder = partyOrder; CasterName = casterName; BookId = bookId;
            BookName = bookName; Spell = spell; Kind = kind; SpellLevel = spellLevel; Uses = uses; Facts = facts; }
        internal string CasterId { get; private set; }
        internal int PartyOrder { get; private set; }
        internal string CasterName { get; private set; }
        internal string BookId { get; private set; }
        internal string BookName { get; private set; }
        internal TeleportSpellKind Spell { get; private set; }
        internal TeleportCastSourceKind Kind { get; private set; }
        internal int SpellLevel { get; private set; }
        internal int Uses { get; private set; }
        internal TeleportCastSourceFacts Facts { get; private set; }
        internal string Key { get { return CasterId + "/" + BookId + "/" + ((int)Spell).ToString(CultureInfo.InvariantCulture); } }
        internal bool Has(TeleportCastSourceFacts fact) { return (Facts & fact) == fact; }
    }
    internal static class TeleportCastAvailabilityPolicy
    {
        internal static bool Usable(TeleportCastSourceSnapshot source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.CasterId) || source.CasterId.Contains("/") ||
                !TeleportDestinationPolicy.IsStableId(source.BookId) || source.PartyOrder < 0 ||
                string.IsNullOrWhiteSpace(source.CasterName) || source.Uses <= 0 || source.SpellLevel < 1 ||
                source.SpellLevel > 9 || !Enum.IsDefined(typeof(TeleportSpellKind), source.Spell) ||
                !source.Has(TeleportCastSourceFacts.Required) ||
                (source.Facts & (TeleportCastSourceFacts.Item | TeleportCastSourceFacts.Metamagic |
                    TeleportCastSourceFacts.Synthetic)) != 0) return false;
            return source.Kind == TeleportCastSourceKind.Prepared ? source.Has(TeleportCastSourceFacts.PreparedUse) :
                source.Kind == TeleportCastSourceKind.Spontaneous && source.Has(TeleportCastSourceFacts.Known);
        }
    }
}
