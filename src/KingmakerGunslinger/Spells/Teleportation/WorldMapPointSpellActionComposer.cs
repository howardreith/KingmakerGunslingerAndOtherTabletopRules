using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class WorldMapPointSpellAction
    {
        internal WorldMapPointSpellAction(TeleportDestinationSnapshot destination, string originId,
            TeleportCastSourceSnapshot source, bool showBook, int scrollVariant = 0, bool readerResolved = true)
        { Destination = destination; OriginId = originId; Source = source; ShowBook = showBook; ScrollVariant = scrollVariant; ReaderResolved = readerResolved; }
        internal TeleportDestinationSnapshot Destination { get; private set; }
        internal string OriginId { get; private set; }
        internal TeleportCastSourceSnapshot Source { get; private set; }
        internal bool ShowBook { get; private set; }
        internal int ScrollVariant { get; private set; }
        internal bool ReaderResolved { get; private set; }
        // UI focus belongs to the spell/variant. Source.Key still binds a real reader.
        internal string Key { get { return Destination.Id + "/" + (Source.Kind == TeleportCastSourceKind.Scroll ?
            WorldMapPointSpellActionComposer.ScrollKey(Source) : Source.Key); } }
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
        internal static string ScrollKey(TeleportCastSourceSnapshot value)
        { return value.ScrollGroupId ?? "scroll:" + value.BookId + ":" + (int)value.Spell + ":" + value.CasterLevel + ":" + value.SpellLevel; }
        internal static WorldMapPointSpellActions<T> Compose<T>(IReadOnlyList<T> native,
            TeleportDestinationSnapshot destination, string originId, TeleportCastBlock blocks,
            IEnumerable<TeleportCastSourceSnapshot> sources, TeleportForbiddenDestinationCatalog catalog,
            bool capitalEstablished, string nativeCapitalId)
        {
            if (native == null) throw new ArgumentNullException("native");
            if (sources == null) throw new ArgumentNullException("sources");
            var empty = new WorldMapPointSpellAction[0];
            if (blocks != TeleportCastBlock.None || !TeleportDestinationPolicy.EvaluateSafety(destination, originId, catalog).Eligible)
                return new WorldMapPointSpellActions<T>(native, empty);
            var usable = sources.Where(TeleportCastAvailabilityPolicy.Usable)
                .GroupBy(value => value.Key + (value.Kind == TeleportCastSourceKind.Scroll ? "/" + ScrollKey(value) : ""), StringComparer.Ordinal)
                .Where(group => group.All(value => value.Uses == group.First().Uses && value.Kind == group.First().Kind &&
                    value.SpellLevel == group.First().SpellLevel && value.CasterLevel == group.First().CasterLevel && value.PartyOrder == group.First().PartyOrder))
                .Select(group => group.First()).Where(value => value.Spell == TeleportSpellKind.WordOfRecall
                    ? WordOfRecallDestinationPolicy.Matches(destination.Id, capitalEstablished, nativeCapitalId)
                    : TeleportDestinationPolicy.Evaluate(destination, originId, catalog).Eligible).ToArray();
            var actions = usable.Where(value => value.Kind != TeleportCastSourceKind.Scroll)
                .Select(value => new WorldMapPointSpellAction(destination, originId, value,
                    usable.Any(other => other.Kind != TeleportCastSourceKind.Scroll && other.CasterId == value.CasterId &&
                        other.Spell == value.Spell && other.BookId != value.BookId))).ToList();
            var groups = usable.Where(value => value.Kind == TeleportCastSourceKind.Scroll).GroupBy(ScrollKey, StringComparer.Ordinal)
                .Where(group => group.All(value => value.Uses == group.First().Uses && value.Spell == group.First().Spell &&
                    value.CasterLevel == group.First().CasterLevel && value.SpellLevel == group.First().SpellLevel))
                .OrderBy(group => group.First().Spell).ThenBy(group => group.Key, StringComparer.Ordinal).ToArray();
            foreach (var group in groups)
            {
                var winner = TeleportScrollReaderPolicy.Select(group);
                // An unsupported comparison still has one stable group row. Its
                // activation reports why no best reader can be bound, without a cast.
                var display = winner ?? group.OrderBy(value => value.PartyOrder).ThenBy(value => value.CasterId, StringComparer.Ordinal).First();
                var variants = groups.Where(value => value.First().Spell == display.Spell).ToArray();
                int variant = variants.Length > 1 ? Array.IndexOf(variants, group) + 1 : 0;
                actions.Add(new WorldMapPointSpellAction(destination, originId, display, false, variant, winner != null));
            }
            return new WorldMapPointSpellActions<T>(native, actions.OrderBy(value => value.Source.Spell)
                .ThenBy(value => value.Source.Kind == TeleportCastSourceKind.Scroll ? 1 : 0)
                .ThenBy(value => value.Source.Kind == TeleportCastSourceKind.Scroll ? 0 : value.Source.PartyOrder)
                .ThenBy(value => value.Source.Kind == TeleportCastSourceKind.Scroll ? ScrollKey(value.Source) : value.Source.CasterId, StringComparer.Ordinal)
                .ThenBy(value => value.Source.BookId, StringComparer.Ordinal));
        }
    }
}
