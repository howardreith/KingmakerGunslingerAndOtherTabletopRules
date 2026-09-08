using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class WorldMapPointSpellAction
    {
        internal WorldMapPointSpellAction(TeleportDestinationSnapshot destination, string originId,
            TeleportCastSourceSnapshot source, bool showBook)
        { Destination = destination; OriginId = originId; Source = source; ShowBook = showBook; }
        internal TeleportDestinationSnapshot Destination { get; private set; }
        internal string OriginId { get; private set; }
        internal TeleportCastSourceSnapshot Source { get; private set; }
        internal bool ShowBook { get; private set; }
        internal string Key { get { return Destination.Id + "/" + Source.Key; } }
    }
    internal sealed class WorldMapPointSpellActions<T>
    {
        internal WorldMapPointSpellActions(IReadOnlyList<T> native, IEnumerable<WorldMapPointSpellAction> spells)
        { NativeActions = native; SpellActions = Array.AsReadOnly(spells.ToArray()); }
        internal IReadOnlyList<T> NativeActions { get; private set; }
        internal IReadOnlyList<WorldMapPointSpellAction> SpellActions { get; private set; }
        internal bool HasSpellActions { get { return SpellActions.Count > 0; } }
    }
    internal static class WorldMapPointSpellActionComposer
    {
        internal static WorldMapPointSpellActions<T> Compose<T>(IReadOnlyList<T> native,
            TeleportDestinationSnapshot destination, string originId, TeleportCastBlock blocks,
            IEnumerable<TeleportCastSourceSnapshot> sources, TeleportForbiddenDestinationCatalog catalog,
            bool capitalEstablished, string nativeCapitalId)
        {
            if (native == null) throw new ArgumentNullException("native");
            if (sources == null) throw new ArgumentNullException("sources");
            var empty = new WorldMapPointSpellAction[0];
            // No enumeration, UI construction, continuation or resource operation on the native-only path.
            if (blocks != TeleportCastBlock.None || !TeleportDestinationPolicy.Evaluate(destination, originId, catalog).Eligible)
                return new WorldMapPointSpellActions<T>(native, empty);
            var usable = sources.Where(TeleportCastAvailabilityPolicy.Usable).GroupBy(value => value.Key, StringComparer.Ordinal)
                // Conflicting equivalent variants are ambiguous: omit the pool rather than guess its count.
                .Where(group => group.All(value => value.Uses == group.First().Uses &&
                    value.Kind == group.First().Kind && value.SpellLevel == group.First().SpellLevel &&
                    value.PartyOrder == group.First().PartyOrder))
                .Select(group => group.First()).Where(value => value.Spell != TeleportSpellKind.WordOfRecall ||
                    WordOfRecallDestinationPolicy.Matches(destination.Id, capitalEstablished, nativeCapitalId))
                .OrderBy(value => value.Spell).ThenBy(value => value.PartyOrder)
                .ThenBy(value => value.CasterId, StringComparer.Ordinal).ThenBy(value => value.BookId, StringComparer.Ordinal).ToArray();
            var actions = usable.Select(value => new WorldMapPointSpellAction(destination, originId, value,
                usable.Any(other => other.CasterId == value.CasterId && other.Spell == value.Spell && other.BookId != value.BookId)));
            return new WorldMapPointSpellActions<T>(native, actions);
        }
    }
}
