using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class TeleportationContextTests
    {
        private const string Origin = "00000000000000000000000000000001";
        private const string Target = "00000000000000000000000000000002";
        private const string Book = "00000000000000000000000000000003";
        private const string Book2 = "00000000000000000000000000000004";
        private static readonly Action[] Native = { () => { }, () => { }, () => { } };
        private static TeleportForbiddenDestinationCatalog Catalog(params string[] ids)
        { return new TeleportForbiddenDestinationCatalog(1, ids.Select(id => new KeyValuePair<string,string>(id, "fixture-native-prohibition"))); }
        private static TeleportDestinationSnapshot Point(string id = Target, TeleportPointKind kind = TeleportPointKind.Location,
            TeleportDestinationFacts facts = TeleportDestinationFacts.Required, int visits = 1, bool nativeVisited = false)
        { return new TeleportDestinationSnapshot(id, "A displayed name", kind, facts, nativeVisited, visits); }
        private static TeleportCastSourceSnapshot Source(TeleportSpellKind spell = TeleportSpellKind.Teleport,
            string caster = "caster-a", int party = 0, string book = Book, int uses = 1,
            TeleportCastSourceKind kind = TeleportCastSourceKind.Prepared,
            TeleportCastSourceFacts facts = TeleportCastSourceFacts.Required | TeleportCastSourceFacts.PreparedUse)
        { return new TeleportCastSourceSnapshot(caster, party, caster, book, book == Book ? "Wizard" : "Sorcerer", spell, kind,
            spell == TeleportSpellKind.Teleport ? 5 : spell == TeleportSpellKind.GreaterTeleport ? 7 : 6, uses, facts); }
        private static WorldMapPointSpellActions<Action> Compose(TeleportDestinationSnapshot point,
            IEnumerable<TeleportCastSourceSnapshot> sources, TeleportCastBlock blocks = TeleportCastBlock.None,
            bool capital = false, string capitalId = WordOfRecallDestinationPolicy.CapitalId,
            TeleportForbiddenDestinationCatalog catalog = null)
        { return WorldMapPointSpellActionComposer.Compose(Native, point, Origin, blocks, sources, catalog ?? Catalog(), capital, capitalId); }
        private static WorldMapPointSpellAction ActionRow(TeleportSpellKind spell = TeleportSpellKind.Teleport,
            TeleportCastSourceKind kind = TeleportCastSourceKind.Prepared)
        { return new WorldMapPointSpellAction(Point(), Origin, Source(spell, kind: kind), false); }
        private static void NativeOnly(WorldMapPointSpellActions<Action> result)
        {
            Assertions.True(ReferenceEquals(Native, result.NativeActions), "Exact native action collection retained.");
            Assertions.False(result.HasSpellActions, "No chooser, empty section, or disabled action is requested.");
            Assertions.Equal(0, result.SpellActions.Count, "No magical rows.");
        }
        internal static void NoSourcePreservesNative()
        {
            NativeOnly(Compose(Point(), new TeleportCastSourceSnapshot[0]));
            NativeOnly(Compose(Point(), new[] { Source(uses: 0) }));
            int nativeCalls = 0;
            Action original = () => nativeCalls++;
            Action inspect = () => nativeCalls += 10;
            var actions = new[] { original, inspect };
            var result = WorldMapPointSpellActionComposer.Compose(actions, Point(), Origin, TeleportCastBlock.None,
                new[] { Source() }, Catalog(), false, null);
            Assertions.True(ReferenceEquals(original, result.NativeActions[0]) && ReferenceEquals(inspect, result.NativeActions[1]),
                "Travel and campaign-specific callbacks are unchanged, including order and native default.");
            Assertions.Equal(0, nativeCalls, "Composition and dismissal invoke no action.");
            result.NativeActions[0]();
            Assertions.Equal(1, nativeCalls, "Choosing native travel invokes its original operation once.");
        }
        internal static void InvalidPointsPreserveNative()
        {
            var sources = new[] { Source(), Source(TeleportSpellKind.GreaterTeleport), Source(TeleportSpellKind.WordOfRecall) };
            NativeOnly(Compose(Point(visits: 0), sources));
            NativeOnly(Compose(Point(Origin), sources));
            NativeOnly(Compose(Point(), sources, catalog: Catalog(Target)));
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.OlegId, visits: 0),
                sources.Where(value => value.Spell != TeleportSpellKind.WordOfRecall)));
            NativeOnly(Compose(Point(visits: 0, nativeVisited: true), sources));
            NativeOnly(Compose(Point(visits: -1, nativeVisited: true), sources));
        }
        internal static void SupportedKindsAndNativeVisit()
        {
            foreach (TeleportPointKind kind in Enum.GetValues(typeof(TeleportPointKind)))
                if (kind != TeleportPointKind.Unknown)
                    Assertions.True(TeleportDestinationPolicy.Evaluate(Point(kind: kind), Origin, Catalog()).Eligible,
                        "Stable visited settlements, crossroads and persistent non-area kinds share positive eligibility.");
            foreach (bool nativeVisited in new[] { false, true })
            {
                var point = Point(visits: 0, nativeVisited: nativeVisited);
                Assertions.Equal(TeleportDestinationReason.Unvisited,
                    TeleportDestinationPolicy.Evaluate(point, Origin, Catalog()).Reason,
                    "Live native state cannot substitute for persisted arrival evidence.");
                NativeOnly(Compose(point, new[] { Source(), Source(TeleportSpellKind.GreaterTeleport) }));
            }
            var ledger = TeleportFamiliarityState.Parse("1|1");
            ledger.MigrateLegacy(new[] { Target });
            NativeOnly(Compose(Point(visits: ledger.Count(Target), nativeVisited: true),
                new[] { Source(), Source(TeleportSpellKind.GreaterTeleport) }));
            var arrival = new TeleportOrdinaryArrivalObservation(0,
                new[] { new TeleportRouteBoundary(Target, 10, true) });
            foreach (string id in arrival.Complete(10, true, Target)) ledger.RecordOrdinaryArrival(id);
            foreach (string id in arrival.Complete(10, true, Target)) ledger.RecordOrdinaryArrival(id);
            Assertions.Equal(1, ledger.Count(Target), "An actual completed boundary credits exactly once.");
            Assertions.Equal(2, Compose(Point(visits: ledger.Count(Target)),
                new[] { Source(), Source(TeleportSpellKind.GreaterTeleport) }).SpellActions.Count,
                "An ordinary arrival enables both Teleport families even without a native flag.");
            string saved = "1|1|" + Target + ":7";
            var existing = TeleportFamiliarityState.Parse(saved);
            existing.MigrateLegacy(new[] { Target, Book2 });
            Assertions.Equal(saved, existing.Serialize(), "Existing v0.0.118 payload and migration flag are unchanged.");
            Assertions.Equal(2, Compose(Point(visits: existing.Count(Target)),
                new[] { Source(), Source(TeleportSpellKind.GreaterTeleport) }).SpellActions.Count,
                "Existing positive historical counts retain exact eligibility.");
        }
        internal static void DestinationReasonsAreExact()
        {
            var reasons = new Dictionary<TeleportDestinationFacts, TeleportDestinationReason> {
                { TeleportDestinationFacts.Persistent, TeleportDestinationReason.NotPersistent },
                { TeleportDestinationFacts.Revealed, TeleportDestinationReason.Unrevealed },
                { TeleportDestinationFacts.Active, TeleportDestinationReason.Inactive },
                { TeleportDestinationFacts.Current, TeleportDestinationReason.Removed },
                { TeleportDestinationFacts.PlacementSupported, TeleportDestinationReason.MissingAnchor },
                { TeleportDestinationFacts.SameGlobalMap, TeleportDestinationReason.IncompatibleMap },
                { TeleportDestinationFacts.CampaignAllowed, TeleportDestinationReason.CampaignProhibition },
                { TeleportDestinationFacts.Selectable, TeleportDestinationReason.NotSelectable } };
            foreach (var pair in reasons)
            {
                var decision = TeleportDestinationPolicy.Evaluate(Point(facts: TeleportDestinationFacts.Required & ~pair.Key), Origin, Catalog());
                Assertions.Equal(pair.Value, decision.Reason, "Each positive fact has a specific rejection reason.");
                Assertions.True(decision.Diagnostic.Contains(Target), "Technical diagnostic retains stable identity.");
            }
            Assertions.Equal(TeleportDestinationReason.Transient, TeleportDestinationPolicy.Evaluate(
                Point(facts: TeleportDestinationFacts.Required | TeleportDestinationFacts.Transient), Origin, Catalog()).Reason, "Synthetic markers excluded.");
            Assertions.Equal(TeleportDestinationReason.UnstableIdentity, TeleportDestinationPolicy.Evaluate(Point("localized name"), Origin, Catalog()).Reason, "Names are never identity.");
            Assertions.Equal(TeleportDestinationReason.UnknownPointKind, TeleportDestinationPolicy.Evaluate(Point(kind: (TeleportPointKind)999), Origin, Catalog()).Reason, "Unknown kinds fail closed.");
            Assertions.Equal(TeleportDestinationReason.MissingOrigin, TeleportDestinationPolicy.Evaluate(Point(), null, Catalog()).Reason, "An unanchored origin cannot cast.");
        }
        internal static void ForbiddenCatalogIsStableAndVersioned()
        {
            var catalog = Catalog(Target);
            Assertions.Equal(1, catalog.Version, "Explicit catalog version.");
            Assertions.True(catalog.Contains(Target), "Exact stable ID match.");
            Assertions.False(catalog.Contains("A displayed name"), "Display names never match exclusions.");
            Assertions.Throws<ArgumentException>(() => Catalog("A displayed name"), "Catalog rejects localized keys.");
            Assertions.Throws<ArgumentException>(() => Catalog(Target, Target), "Ambiguous duplicate exclusions fail closed.");
        }
        internal static void ActionsOrderAndCounts()
        {
            var result = Compose(Point(WordOfRecallDestinationPolicy.OlegId), new[] {
                Source(TeleportSpellKind.Teleport), Source(TeleportSpellKind.GreaterTeleport), Source(TeleportSpellKind.WordOfRecall),
                Source(TeleportSpellKind.Teleport, "caster-b", 1, uses: 2), Source(TeleportSpellKind.Teleport, "caster-c", 0) });
            Assertions.True(ReferenceEquals(Native, result.NativeActions), "Native ordering/default is untouched.");
            Assertions.Equal("WordOfRecall,GreaterTeleport,Teleport,Teleport,Teleport", string.Join(",", result.SpellActions.Select(row => row.Source.Spell)), "Spell order.");
            Assertions.Equal("caster-a,caster-c,caster-b", string.Join(",", result.SpellActions.Skip(2).Select(row => row.Source.CasterId)), "Party order, then stable caster ID.");
            Assertions.Equal(2, result.SpellActions[4].Source.Uses, "Current source count retained for presentation.");
            Assertions.Equal(1, Compose(Point(), new[] { Source() }).SpellActions.Count, "One Teleport source appends one row.");
            Assertions.Equal(1, Compose(Point(), new[] { Source(TeleportSpellKind.GreaterTeleport) }).SpellActions.Count, "One Greater source appends one row.");
        }
        internal static void RecallUsesOnlyLockedDestination()
        {
            var recall = new[] { Source(TeleportSpellKind.WordOfRecall) };
            Assertions.Equal(1, Compose(Point(WordOfRecallDestinationPolicy.OlegId, visits: 0), recall).SpellActions.Count,
                "Pre-capital Oleg is a fixed sanctuary, independent of Teleport familiarity.");
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.OlegId, visits: -1), recall));
            NativeOnly(Compose(Point(), recall));
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.CapitalId), recall));
            Assertions.Equal(1, Compose(Point(WordOfRecallDestinationPolicy.CapitalId, visits: 0), recall, capital: true).SpellActions.Count,
                "Established capital retains exact sanctuary eligibility without a Teleport ledger count.");
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.OlegId), recall, capital: true));
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.OlegId), recall, capital: true, capitalId: null));
            NativeOnly(Compose(Point(WordOfRecallDestinationPolicy.CapitalId, facts: TeleportDestinationFacts.Required & ~TeleportDestinationFacts.Active), recall, capital: true));
            Assertions.Equal<string>(null, WordOfRecallDestinationPolicy.Resolve(true, "Capital"), "No display-name fallback.");
            Assertions.Equal<string>(null, WordOfRecallDestinationPolicy.Resolve(true, WordOfRecallDestinationPolicy.OlegId), "Post-capital Oleg is never a fallback.");
            Assertions.Equal<string>(null, WordOfRecallDestinationPolicy.Resolve(true, Target), "An arbitrary stable target cannot become the capital.");
        }
        internal static void DistinctBooksAndEquivalentVariants()
        {
            var first = Source();
            var second = Source(book: Book2, uses: 2, kind: TeleportCastSourceKind.Spontaneous,
                facts: TeleportCastSourceFacts.Required | TeleportCastSourceFacts.Known);
            var result = Compose(Point(), new[] { second, first, first, second });
            Assertions.Equal(2, result.SpellActions.Count, "Equivalent AbilityData variants produce one row per caster/book/spell.");
            Assertions.True(result.SpellActions.All(row => row.ShowBook), "Distinct books require an unambiguous book label.");
            Assertions.Equal(Book, result.SpellActions[0].Source.BookId, "Stable spellbook order.");
            NativeOnly(Compose(Point(), new[] { first, Source(uses: 2) }));
            Assertions.Equal(2, Compose(Point(), new[] { second, first, first }).SpellActions.Count, "Repeated composition does not accumulate rows.");
        }
        internal static void ReopeningRecalculatesAvailability()
        {
            var first = Compose(Point(), new[] { Source(uses: 2) });
            var second = Compose(Point(), new[] { Source(uses: 1) });
            Assertions.Equal(2, first.SpellActions[0].Source.Uses, "Old immutable snapshot.");
            Assertions.Equal(1, second.SpellActions[0].Source.Uses, "New composition uses current state.");
            NativeOnly(Compose(Point(), new[] { Source(uses: 0) }));
        }
        internal static void EveryWorldMapBlockOmitsActions()
        {
            foreach (TeleportCastBlock block in Enum.GetValues(typeof(TeleportCastBlock)))
                if (block != TeleportCastBlock.None) NativeOnly(Compose(Point(), new[] { Source() }, block));
        }
        internal static void OnlyRealUsableSpellSources()
        {
            foreach (TeleportCastSourceFacts fact in new[] { TeleportCastSourceFacts.ActiveParty,
                TeleportCastSourceFacts.LivingAvailableCaster, TeleportCastSourceFacts.OwnedSpellbook,
                TeleportCastSourceFacts.BookUsable, TeleportCastSourceFacts.ExactSpell, TeleportCastSourceFacts.RealResource,
                TeleportCastSourceFacts.PreparedUse })
                NativeOnly(Compose(Point(), new[] { Source(facts: (TeleportCastSourceFacts.Required | TeleportCastSourceFacts.PreparedUse) & ~fact) }));
            foreach (TeleportCastSourceFacts fact in new[] { TeleportCastSourceFacts.Item, TeleportCastSourceFacts.Metamagic, TeleportCastSourceFacts.Synthetic })
                NativeOnly(Compose(Point(), new[] { Source(facts: TeleportCastSourceFacts.Required | TeleportCastSourceFacts.PreparedUse | fact) }));
            NativeOnly(Compose(Point(), new[] { Source(kind: TeleportCastSourceKind.Spontaneous) }));
            NativeOnly(Compose(Point(), new[] { Source(kind: TeleportCastSourceKind.Unknown) }));
            Assertions.Equal(1, Compose(Point(), new[] { Source(kind: TeleportCastSourceKind.Spontaneous,
                facts: TeleportCastSourceFacts.Required | TeleportCastSourceFacts.Known) }).SpellActions.Count, "Known spell and real slot source.");
        }
    }
}