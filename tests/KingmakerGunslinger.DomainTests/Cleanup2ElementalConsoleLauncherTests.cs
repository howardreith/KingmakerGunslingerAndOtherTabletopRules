using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    // S5 T2 / DEF-06: the controller spell selector's one native row list.
    // A Row stands in for one native ActionBarSelectorItemVm and carries its
    // source blueprint; consolidation follows the production identity gate
    // (an original standalone Summon Elemental rank parent). Every case uses
    // ElementalConsoleLauncherPolicy and ElementalConsoleResultPolicy directly.
    internal static class Cleanup2ElementalConsoleLauncherTests
    {
        private class Row
        {
            internal readonly string Guid, Name;
            internal int Disposed;
            internal Row(string guid, string name) { Guid = guid; Name = name; }
            public override string ToString() { return Name; }
        }
        private sealed class Launcher : Row { internal Launcher() : base(null, "Elementals") { } }

        private const string SmallElemental = "970c6db48ff0c6f43afc9dbb48780d03";
        private const string MediumElemental = "e42b1dbff4262c6469a9ff0a6ce730e3";
        private static readonly string SummonMonsterII = ElementalSourceChooserPolicy.Rank(2).MonsterParentGuid;
        private static readonly string SummonNaturesAllyIV = ElementalSourceChooserPolicy.Rank(4).NaturesAllyParentGuid;

        private static List<Row> Eligible(IEnumerable<Row> rows)
        { return rows.Where(value => ElementalSourceChooserPolicy.Parent(value.Guid) != null).ToList(); }

        private static string Render(IEnumerable<Row> rows) { return string.Join(", ", rows.Select(value => value.ToString())); }

        private sealed class Transaction
        {
            internal readonly List<string> Events = new List<string>();
            internal int Built, Registered, Unregistered;
            internal Launcher Last;
            internal Func<Row> Factory() { return () => { Built++; Events.Add("build"); return Last = new Launcher(); }; }
            internal string Commit(ElementalConsoleStaging<Row> staging, List<Row> live, Func<string> revalidate = null,
                Action<IList<Row>, IList<Row>> write = null)
            {
                return ElementalConsoleLauncherPolicy.Commit<Row>(staging, live, revalidate ?? (() => null),
                    write ?? ((target, rows) => { Events.Add("write"); ElementalConsoleLauncherPolicy.Write(target, rows); }),
                    () => { Registered++; Events.Add("register"); }, () => { Unregistered++; Events.Add("unregister"); },
                    value => { value.Disposed++; Events.Add("dispose-owned:" + value.Name); },
                    value => { value.Disposed++; Events.Add("dispose-native:" + value.Name); });
            }
        }

        private static bool Exact(List<Row> live, Row[] original)
        { return live.Count == original.Length && live.Select((value, index) => ReferenceEquals(value, original[index])).All(value => value); }

        // 1, 3: no consolidation, or no sources: nothing is built and nothing is touched.
        internal static void NoConsolidationKeepsTheNativeSelectorExactly()
        {
            var summonMonster = new Row(SummonMonsterII, "Summon Monster II");
            var natures = new Row(SummonNaturesAllyIV, "Summon Nature's Ally IV");
            var other = new Row("magic-missile", "Magic Missile");
            var live = new List<Row> { other, summonMonster, natures };
            Row[] original = live.ToArray();
            var transaction = new Transaction();
            var staging = ElementalConsoleLauncherPolicy.Stage<Row>(live, Eligible(live), 8, transaction.Factory());
            Assertions.False(staging.Admitted, "Sources from Summon Monster/Nature's Ally rows with no original elemental row get no launcher.");
            Assertions.Equal("no-original-row-consolidated", staging.Reason, "The refusal names the missing consolidation.");
            Assertions.Equal(0, transaction.Built, "The launcher factory (and the context it builds) is never called.");
            Assertions.True(Exact(live, original), "Every native row keeps its exact reference and order.");
            Assertions.True(live.All(value => value.Disposed == 0), "No native row is disposed.");
            Assertions.Equal(0, transaction.Registered, "No console context is registered.");
            Assertions.True(Throws(() => transaction.Commit(staging, live)), "A refused staging can never commit.");
            Assertions.True(Exact(live, original), "A refused commit attempt writes nothing.");

            var small = new Row(SmallElemental, "Summon Small Elemental");
            var withOriginal = new List<Row> { other, small, summonMonster };
            Row[] before = withOriginal.ToArray();
            var none = ElementalConsoleLauncherPolicy.Stage<Row>(withOriginal, Eligible(withOriginal), 0, transaction.Factory());
            Assertions.True(!none.Admitted && none.Reason == "no-sources", "Empty sources get no launcher even when an original row is present.");
            var empty = ElementalConsoleLauncherPolicy.Stage<Row>(new List<Row>(), new List<Row>(), 0, transaction.Factory());
            Assertions.True(!empty.Admitted, "Empty sources and an empty selector get no launcher.");
            Assertions.Equal(0, transaction.Built, "No launcher was built for any empty-source shape.");
            Assertions.True(Exact(withOriginal, before) && small.Disposed == 0, "The original row is untouched and not disposed.");
        }

        // 2: removed rows that are not exact members of this selector.
        internal static void ForeignRemovedRowsAreRefused()
        {
            var small = new Row(SmallElemental, "Summon Small Elemental");
            var other = new Row("magic-missile", "Magic Missile");
            var live = new List<Row> { other, small };
            Row[] original = live.ToArray();
            var transaction = new Transaction();
            var lookalike = new Row(SmallElemental, "Summon Small Elemental (another selector)");
            var foreign = ElementalConsoleLauncherPolicy.Stage<Row>(live, new List<Row> { lookalike }, 4, transaction.Factory());
            Assertions.True(!foreign.Admitted && foreign.Reason == "removed-row-not-exactly-once-in-original-selector",
                "A removed row from another selector (same blueprint, other object) is refused.");
            var partly = ElementalConsoleLauncherPolicy.Stage<Row>(live, new List<Row> { small, lookalike }, 4, transaction.Factory());
            Assertions.False(partly.Admitted, "One exact member plus one foreign row is still refused.");
            var duplicated = ElementalConsoleLauncherPolicy.Stage<Row>(live, new List<Row> { small, small }, 4, transaction.Factory());
            Assertions.True(!duplicated.Admitted && duplicated.Reason == "removed-rows-not-distinct", "A row removed twice is refused.");
            var twice = new List<Row> { small, other, small };
            Assertions.False(ElementalConsoleLauncherPolicy.Stage<Row>(twice, new List<Row> { small }, 4, transaction.Factory()).Admitted,
                "A row that occurs twice in the selector has no exact position.");
            Assertions.Equal(0, transaction.Built, "No launcher is built for a refused staging.");
            Assertions.True(Exact(live, original) && small.Disposed == 0 && lookalike.Disposed == 0, "Nothing is mutated or disposed.");
        }

        // 4, 5, 6: exactly one launcher at the first consolidated position; everything else exact.
        internal static void OneLauncherTakesTheFirstConsolidatedPosition()
        {
            var transaction = new Transaction();
            var a = new Row("magic-missile", "Magic Missile");
            var medium = new Row(MediumElemental, "Summon Medium Elemental");
            var b = new Row("fireball", "Fireball");
            var single = new List<Row> { a, medium, b };
            var one = ElementalConsoleLauncherPolicy.Stage<Row>(single, Eligible(single), 4, transaction.Factory());
            Assertions.True(one.Admitted && one.Position == 1, "One valid original row admits the launcher at its exact position.");
            Assertions.Equal("Magic Missile, Elementals, Fireball", Render(one.Staged), "The launcher replaces the original row in place.");
            Assertions.True(ReferenceEquals(one.Staged[0], a) && ReferenceEquals(one.Staged[2], b), "Retained rows keep their exact references.");

            var small = new Row(SmallElemental, "Summon Small Elemental");
            var summonMonster = new Row(SummonMonsterII, "Summon Monster II");
            var natures = new Row(SummonNaturesAllyIV, "Summon Nature's Ally IV");
            var foreign = new Row("cotw-foreign", "Foreign Mod Spell");
            var large = new Row("2e8ef4e6a7a5b3a4b9a0f4c3d1e2a5b6", "Unrelated Native Spell");
            var mixed = new List<Row> { a, summonMonster, small, foreign, natures, medium, large, b };
            Row[] original = mixed.ToArray();
            List<Row> eligible = Eligible(mixed);
            Assertions.True(eligible.Count == 2 && eligible.Contains(small) && eligible.Contains(medium),
                "Only the standalone elemental originals are eligible in the mixed menu.");
            var staging = ElementalConsoleLauncherPolicy.Stage<Row>(mixed, eligible, 12, transaction.Factory());
            Assertions.True(staging.Admitted && staging.Position == 2, "The launcher takes the first consolidated position.");
            Assertions.Equal("Magic Missile, Summon Monster II, Elementals, Foreign Mod Spell, Summon Nature's Ally IV, Unrelated Native Spell, Fireball",
                Render(staging.Staged), "Summon Monster/Nature's Ally source rows, foreign and unrelated rows keep their exact order.");
            Assertions.Equal(1, staging.Staged.OfType<Launcher>().Count(), "Exactly one launcher.");
            var retained = staging.Staged.Where(value => !(value is Launcher)).ToArray();
            Assertions.True(retained.SequenceEqual(original.Where(value => !eligible.Contains(value))),
                "Every retained row is the exact original object, in the original order.");
            Assertions.True(Exact(mixed, original), "Staging never writes the live list.");
        }

        // 7: stale selector, owner, unit or rows before commit.
        internal static void StaleSelectorBeforeCommitWritesNothing()
        {
            foreach (string reason in new[] { "selector-changed", "owner-changed", "unit-changed", "spellbooks-changed" })
            {
                var small = new Row(SmallElemental, "Summon Small Elemental");
                var a = new Row("magic-missile", "Magic Missile");
                var live = new List<Row> { a, small };
                Row[] original = live.ToArray();
                var transaction = new Transaction();
                var staging = ElementalConsoleLauncherPolicy.Stage<Row>(live, Eligible(live), 4, transaction.Factory());
                string outcome = transaction.Commit(staging, live, () => reason);
                Assertions.Equal("refuse-stale-console-collection-no-write:" + reason, outcome, "The stale reason is named: " + reason);
                Assertions.True(Exact(live, original), "No write when " + reason);
                Assertions.Equal(1, transaction.Last.Disposed, "The owned launcher is disposed once when " + reason);
                Assertions.True(small.Disposed == 0 && a.Disposed == 0, "Native rows are not disposed when " + reason);
                Assertions.Equal(0, transaction.Registered, "No context is registered when " + reason);
            }
            var row = new Row(SmallElemental, "Summon Small Elemental");
            var rows = new List<Row> { row };
            var t = new Transaction();
            var staged = ElementalConsoleLauncherPolicy.Stage<Row>(rows, Eligible(rows), 4, t.Factory());
            var intruder = new Row("foreign", "Foreign Row");
            rows.Add(intruder);
            Assertions.Equal("refuse-stale-console-collection-no-write:selector-rows-changed", t.Commit(staged, rows),
                "A foreign change to the selector rows before commit refuses.");
            Assertions.True(rows.Count == 2 && ReferenceEquals(rows[1], intruder) && row.Disposed == 0, "The foreign change is kept and nothing native is disposed.");
        }

        // 8: an injected partial write failure.
        internal static void PartialCommitFailureRollsBackOnlyItsOwnState()
        {
            var small = new Row(SmallElemental, "Summon Small Elemental");
            var a = new Row("magic-missile", "Magic Missile");
            var b = new Row("fireball", "Fireball");
            var live = new List<Row> { a, small, b };
            Row[] original = live.ToArray();
            var transaction = new Transaction();
            var staging = ElementalConsoleLauncherPolicy.Stage<Row>(live, Eligible(live), 4, transaction.Factory());
            string outcome = transaction.Commit(staging, live, null, (target, rows) =>
            { target.Clear(); target.Add(rows[0]); target.Add(rows[1]); throw new InvalidOperationException("injected"); });
            Assertions.True(outcome.StartsWith("console-commit-failed:InvalidOperationException;originalRestored=True", StringComparison.Ordinal),
                "A partial write this transaction still owns is rolled back: " + outcome);
            Assertions.True(Exact(live, original), "The exact original references and order are restored.");
            Assertions.True(small.Disposed == 0 && a.Disposed == 0 && b.Disposed == 0, "Native rows (removed or retained) are never disposed on failure.");
            Assertions.True(transaction.Last.Disposed == 1 && transaction.Unregistered == 1, "The owned launcher is disposed and the context unregistered.");

            var foreignTransaction = new Transaction();
            var foreignLive = new List<Row> { a, small, b };
            var foreignStaging = ElementalConsoleLauncherPolicy.Stage<Row>(foreignLive, Eligible(foreignLive), 4, foreignTransaction.Factory());
            var foreignRow = new Row("foreign", "Foreign Row");
            string foreignOutcome = foreignTransaction.Commit(foreignStaging, foreignLive, null, (target, rows) =>
            { target.Clear(); target.Add(foreignRow); throw new InvalidOperationException("foreign write"); });
            Assertions.True(foreignOutcome.Contains("originalRestored=False") && foreignOutcome.Contains("foreign-mutation-not-overwritten"),
                "A foreign mutation during the failure is not overwritten: " + foreignOutcome);
            Assertions.True(foreignLive.Count == 1 && ReferenceEquals(foreignLive[0], foreignRow), "The foreign state is left as found.");
            Assertions.True(small.Disposed == 0, "The removed native row is still not disposed.");
        }

        // 9: a complete commit disposes removed native rows once each, after the write.
        internal static void SuccessfulCommitDisposesRemovedRowsOnceAfterWrite()
        {
            var small = new Row(SmallElemental, "Summon Small Elemental");
            var medium = new Row(MediumElemental, "Summon Medium Elemental");
            var a = new Row("magic-missile", "Magic Missile");
            var live = new List<Row> { small, a, medium };
            var transaction = new Transaction();
            var staging = ElementalConsoleLauncherPolicy.Stage<Row>(live, Eligible(live), 8, transaction.Factory());
            string outcome = transaction.Commit(staging, live);
            Assertions.Equal("native-console-elementals-ready;removed=2;position=0", outcome, "The commit completes.");
            Assertions.Equal("Elementals, Magic Missile", Render(live), "The live list is the staged list.");
            Assertions.True(small.Disposed == 1 && medium.Disposed == 1 && a.Disposed == 0, "Each removed native row is disposed exactly once; retained rows never.");
            Assertions.Equal("build,register,write,dispose-native:Summon Small Elemental,dispose-native:Summon Medium Elemental",
                string.Join(",", transaction.Events.ToArray()), "Native disposal happens only after the complete write.");
            Assertions.True(transaction.Last.Disposed == 0 && transaction.Unregistered == 0 && ReferenceEquals(live[0], transaction.Last),
                "The launcher and its context stay current.");
        }

        // 10: the controller Result routes. Only a current, owned, validated leaf with a free targeting owner
        // reaches native pin or use; cancellation and refusals spend nothing and never dispatch twice.
        internal static void ResultRoutesSpendOnlyAnExactCurrentSource()
        {
            string outcome; int validations = 0;
            Func<string> valid = () => { validations++; return null; };
            Func<bool> free = () => true;
            Assertions.Equal(ElementalConsoleResultRoute.NativePassThrough, ElementalConsoleResultPolicy.Route(false, false, false, true,
                false, valid, true, free, out outcome), "A native row keeps the native Result.");
            Assertions.Equal(ElementalConsoleResultRoute.RefuseStale, ElementalConsoleResultPolicy.Route(true, true, false, true,
                true, valid, true, free, out outcome), "A stale node refuses.");
            Assertions.Equal("refuse-stale-console-node-no-spend", outcome, "The stale refusal spends nothing.");
            Assertions.Equal(ElementalConsoleResultRoute.RefuseStale, ElementalConsoleResultPolicy.Route(true, false, true, true,
                true, valid, true, free, out outcome), "A node of another action-bar owner refuses.");
            Assertions.Equal(0, validations, "A stale node never validates or reaches its source.");
            Assertions.Equal(ElementalConsoleResultRoute.RestoreModuleOff, ElementalConsoleResultPolicy.Route(true, true, true, false,
                true, valid, true, free, out outcome), "Module off restores the native selector.");
            Assertions.Equal(ElementalConsoleResultRoute.RefuseSource, ElementalConsoleResultPolicy.Route(true, true, true, true,
                true, () => "This source is unavailable or exhausted.", true, free, out outcome), "An exhausted source refuses.");
            Assertions.True(outcome.Contains("exhausted"), "The exhausted refusal names the reason.");
            Assertions.Equal(ElementalConsoleResultRoute.RefuseSource, ElementalConsoleResultPolicy.Route(true, true, true, true,
                true, () => null, true, () => false, out outcome), "Use refuses while another targeting owner is active.");
            Assertions.Equal("refuse-console-source-no-spend:foreign-targeting-owner", outcome, "The foreign targeting owner is named.");
            Assertions.Equal(ElementalConsoleResultRoute.PinSource, ElementalConsoleResultPolicy.Route(true, true, true, true,
                true, () => null, false, () => false, out outcome), "Confirm (execute=false) pins without needing the targeting owner.");
            Assertions.Equal(ElementalConsoleResultRoute.UseSource, ElementalConsoleResultPolicy.Route(true, true, true, true,
                true, () => null, true, free, out outcome), "Use (execute=true) of a validated current source reaches the native Result.");
            Assertions.Equal(ElementalConsoleResultRoute.Navigate, ElementalConsoleResultPolicy.Route(true, true, true, true,
                false, valid, true, free, out outcome), "Rank and element nodes navigate.");

            var source = new object(); int dispatches = 0; bool current = true;
            var cancelled = new ElementalSourceSelection<object>(new[] { source }, () => current, value => null, value => dispatches++);
            cancelled.Select(source); cancelled.Cancel();
            Assertions.False(cancelled.Cast(), "A cancelled selection cannot dispatch.");
            var stale = new ElementalSourceSelection<object>(new[] { source }, () => current, value => null, value => dispatches++);
            stale.Select(source); current = false;
            Assertions.False(stale.Cast(), "A stale selection cannot dispatch.");
            current = true;
            var used = new ElementalSourceSelection<object>(new[] { source }, () => current, value => null, value => dispatches++);
            used.Select(source);
            Assertions.True(used.Cast() && !used.Cast(), "One use dispatches once; a repeated submit does not.");
            Assertions.Equal(1, dispatches, "Exactly one dispatch across cancel, stale and repeated use.");
        }

        private static bool Throws(Action action)
        {
            try { action(); return false; }
            catch (InvalidOperationException) { return true; }
        }
    }
}
