using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    // Desktop action-bar Spells tab: the rows ElementalSourceChooserRuntime.End
    // stages after native FillSlots. A row stands in for one native mechanism
    // (MechanicActionBarSlotMemorizedSpell/SpontaneusSpell) and carries its
    // source blueprint. Consolidation follows the production identity gate in
    // Collect: only an original standalone Summon Elemental rank parent
    // (ElementalSourceChooserPolicy.Parent != null) can leave its row.
    internal static class Cleanup2ElementalLauncherStagingTests
    {
        private class Row
        {
            internal readonly string Guid, Name;
            internal Row(string guid, string name) { Guid = guid; Name = name; }
            public override string ToString() { return Name; }
        }
        private sealed class Launcher : Row { internal Launcher() : base(null, "Elementals") { } }

        private const string MediumElemental = "e42b1dbff4262c6469a9ff0a6ce730e3";
        private const string SmallElemental = "970c6db48ff0c6f43afc9dbb48780d03";
        private static readonly string SummonMonsterII = ElementalSourceChooserPolicy.Rank(2).MonsterParentGuid;
        private static readonly string SummonMonsterIV = ElementalSourceChooserPolicy.Rank(4).MonsterParentGuid;
        private static readonly string SummonNaturesAllyIV = ElementalSourceChooserPolicy.Rank(4).NaturesAllyParentGuid;

        private static List<Row> Consolidated(IEnumerable<Row[]> levels)
        { return levels.SelectMany(value => value).Where(value => ElementalSourceChooserPolicy.Parent(value.Guid) != null).ToList(); }

        private static string Render(Row[] row) { return string.Join(", ", row.Select(value => value.ToString())); }

        internal static void ReportedFourthLevelRowWithoutOriginalTileGetsNoLauncher()
        {
            // Leinna's reported level-4 popup: four native tiles, the last a
            // prepared Summon Monster IV whose published Medium elemental
            // children the chooser collects. No original standalone tile.
            var levels = new List<Row[]>
            {
                new[] { new Row("cantrip", "Cantrip") },
                new[] { new Row("l1", "Magic Missile") },
                new Row[0],
                new[] { new Row("l3", "Fireball") },
                new[] { new Row("a", "Spell A"), new Row("b", "Spell B"), new Row("c", "Spell C"),
                    new Row(SummonMonsterIV, "Summon Monster IV") }
            };
            var native = levels.Select(value => value.ToArray()).ToList();
            var consolidated = Consolidated(levels);
            Assertions.True(ElementalSourceChooserPolicy.Parent(SummonMonsterIV) == null && ElementalSourceChooserPolicy.Parent(SummonNaturesAllyIV) == null,
                "Summon Monster IV and Summon Nature's Ally IV are mixed-creature menus, never consolidated tiles.");
            Assertions.Equal(0, consolidated.Count, "The reported row has no original Summon Elemental tile.");
            int built = 0;
            var staged = ElementalSourceChooserPolicy.StageLauncherRows<Row>(levels, 4, consolidated, () => { built++; return new Launcher(); });
            Assertions.True(staged == null,
                "With nothing to consolidate the native rows are left as they are; staged level 4 was: " +
                (staged == null ? "<unchanged>" : Render(staged[4])));
            Assertions.Equal(0, built, "No launcher slot (visible or empty) is constructed.");
            Assertions.True(levels.Zip(native, (left, right) => left.SequenceEqual(right)).All(value => value),
                "Native rows, including the Summon Monster IV tile and its own variant popup, are untouched.");
        }

        internal static void LegacyPreparedOriginalTileKeepsTheLauncherInItsPlace()
        {
            var summonMonster = new Row(SummonMonsterIV, "Summon Monster IV");
            var legacy = new Row(MediumElemental, "Summon Medium Elemental");
            var levels = new List<Row[]>
            {
                new[] { new Row("cantrip", "Cantrip") },
                new[] { new Row("a", "Spell A"), summonMonster, legacy, new Row("c", "Spell C") }
            };
            var consolidated = Consolidated(levels);
            Assertions.True(consolidated.Count == 1 && ReferenceEquals(consolidated[0], legacy),
                "A prepared original Summon Medium Elemental is the consolidated tile.");
            var staged = ElementalSourceChooserPolicy.StageLauncherRows<Row>(levels, 1, consolidated, () => new Launcher());
            Assertions.True(staged != null, "A legacy prepared original spell keeps its action-bar casting route.");
            Assertions.Equal("Spell A, Summon Monster IV, Elementals, Spell C", Render(staged[1]),
                "The launcher takes the original tile's place; Summon Monster IV and other spells keep their order.");
            Assertions.Equal(1, staged.SelectMany(value => value).OfType<Launcher>().Count(), "Exactly one launcher.");
            Assertions.Equal("Cantrip", Render(staged[0]), "Other levels are unchanged.");
        }

        internal static void LowerSourceLevelReceivesTheLauncherAndOriginalTileLeaves()
        {
            // A Summon Monster II source sets the minimum level; the original
            // Medium tile on level 4 is still the consolidated one.
            var levels = new List<Row[]>
            {
                new[] { new Row(SummonMonsterII, "Summon Monster II"), new Row("b", "Spell B") },
                new[] { new Row(MediumElemental, "Summon Medium Elemental"), new Row("c", "Spell C") }
            };
            var staged = ElementalSourceChooserPolicy.StageLauncherRows<Row>(levels, 0, Consolidated(levels), () => new Launcher());
            Assertions.Equal("Summon Monster II, Spell B, Elementals", Render(staged[0]), "Launcher appended at the minimum source level.");
            Assertions.Equal("Spell C", Render(staged[1]), "Only the consolidated original tile leaves level 4.");
        }

        internal static void EveryNativeRefillStagesOneFreshLauncherWithoutWritingRows()
        {
            // FillSlots rebuilds native rows on every refill (reopen, level or
            // character switch). Staging never writes them and never reuses an
            // earlier launcher.
            Func<List<Row[]>> fill = () => new List<Row[]>
            { new[] { new Row(SmallElemental, "Summon Small Elemental"), new Row(SummonMonsterII, "Summon Monster II") } };
            var first = fill(); var firstNative = first[0].ToArray();
            var a = ElementalSourceChooserPolicy.StageLauncherRows<Row>(first, 0, Consolidated(first), () => new Launcher());
            var second = fill();
            var b = ElementalSourceChooserPolicy.StageLauncherRows<Row>(second, 0, Consolidated(second), () => new Launcher());
            Assertions.True(first[0].SequenceEqual(firstNative), "The native input row is not written.");
            Assertions.Equal("Elementals, Summon Monster II", Render(a[0]), "First fill: one launcher in the original tile's place.");
            Assertions.Equal("Elementals, Summon Monster II", Render(b[0]), "Refill: one launcher, no duplicate.");
            Assertions.False(ReferenceEquals(a[0][0], b[0][0]), "Each refill owns its own launcher.");
            var none = new List<Row[]> { new[] { new Row(SummonMonsterII, "Summon Monster II") } };
            Assertions.True(ElementalSourceChooserPolicy.StageLauncherRows<Row>(none, 0, Consolidated(none), () => new Launcher()) == null,
                "A later refill without an original tile drops the launcher instead of keeping a stale one.");
            Assertions.Throws<InvalidOperationException>(() => ElementalSourceChooserPolicy.StageLauncherRows<Row>(first, 3,
                Consolidated(first), () => new Launcher()), "A missing destination level refuses, so End restores native rows.");
        }
    }
}
