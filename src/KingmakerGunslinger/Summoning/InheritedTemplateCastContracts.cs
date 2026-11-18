using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// DEF-05 second review, engine-free judges for the representative native
    /// casts and the alternate child census. Each returns the list of failures
    /// (empty when the contract holds). The runtime observers feed them what
    /// they read from the engine; the domain tests feed them mutations.
    /// </summary>
    internal static class InheritedSourceLedger
    {
        /// <summary>
        /// Correction 1. events: every native spend or restore on the caster's
        /// own sources during the cast window, as "kind|key|amount" with kind one
        /// of spend-spell, restore-spontaneous, spend-resource, restore-resource.
        /// before/after: every source counter ("spontaneous|book|level",
        /// "slot|book|index", "resource|guid") and identity ("identity|..." as
        /// a value hash). expectedKey: the one counter that must fall, by
        /// expectedAmount, through exactly one spend event; null for "nothing".
        /// </summary>
        internal static IList<string> Judge(IList<string> events, IDictionary<string, int> before,
            IDictionary<string, int> after, string expectedKey, int expectedAmount, string expectedEvent)
        {
            var failures = new List<string>();
            if (events == null || before == null || after == null) { failures.Add("missing-observation"); return failures; }
            foreach (string key in before.Keys.Union(after.Keys).OrderBy(value => value, StringComparer.Ordinal))
            {
                int was, now;
                bool hadBefore = before.TryGetValue(key, out was), hasAfter = after.TryGetValue(key, out now);
                if (!hadBefore || !hasAfter) { failures.Add("source-identity-changed:" + key); continue; }
                int expectedDelta = key == expectedKey ? expectedAmount : 0;
                if (was - now != expectedDelta) failures.Add("source-changed:" + key + ":" + was + "->" + now);
            }
            if (expectedKey == null)
            {
                foreach (string value in events) failures.Add("unexpected-event:" + value);
                return failures;
            }
            if (expectedAmount < 1) failures.Add("non-positive-expected-debit:" + expectedAmount);
            if (!before.ContainsKey(expectedKey) || before[expectedKey] < expectedAmount)
                failures.Add("expected-source-unavailable:" + expectedKey);
            int matching = events.Count(value => value == expectedEvent);
            if (matching != 1) failures.Add("expected-spend-events:" + matching);
            foreach (string value in events.Where(value => value != expectedEvent)) failures.Add("unexpected-event:" + value);
            return failures;
        }
    }

    /// <summary>One spawn action's count expression and unit, as read by SummonSignatureReader.</summary>
    internal sealed class InheritedSpawnExpression
    {
        internal InheritedSpawnExpression(string unit, string dice, string countKind, int countValue,
            string bonusKind, int bonusValue, string bonusRank)
        {
            Unit = unit; Dice = dice; CountKind = countKind; CountValue = countValue;
            BonusKind = bonusKind; BonusValue = bonusValue; BonusRank = bonusRank;
        }

        internal string Unit { get; private set; }
        internal string Dice { get; private set; }
        internal string CountKind { get; private set; }
        internal int CountValue { get; private set; }
        internal string BonusKind { get; private set; }
        internal int BonusValue { get; private set; }
        internal string BonusRank { get; private set; }
    }

    internal static class InheritedQuantityContract
    {
        /// <summary>
        /// Correction 2. The native forms (memory of the 2.1.7b library, and
        /// the retained player paths): a single row is Zero dice with a Simple
        /// bonus of 1; a quantity row is one D3 or one D4 (Simple count 1) plus
        /// the Rank bonus ProjectilesCount (where Superior Summoning applies),
        /// whose value for this caster is evaluated natively before the cast:
        /// 0 for d3 and 1 for d4+1. Returns the expression's inclusive range.
        /// </summary>
        internal static IList<string> Expression(SummonMultiplicity multiplicity, string expectedUnit,
            IList<InheritedSpawnExpression> spawns, int expectedBranches, int evaluatedRankBonus,
            out int minimum, out int maximum)
        {
            var failures = new List<string>();
            minimum = 0; maximum = 0;
            if (spawns == null || spawns.Count == 0) { failures.Add("no-spawn-branch"); return failures; }
            if (spawns.Count != expectedBranches)
                failures.Add("spawn-branches:" + spawns.Count + "/" + expectedBranches);
            for (int index = 0; index < spawns.Count; index++)
            {
                InheritedSpawnExpression spawn = spawns[index];
                string where = "branch" + index + ":";
                if (!string.Equals(spawn.Unit, expectedUnit, StringComparison.Ordinal))
                    failures.Add(where + "wrong-unit:" + spawn.Unit);
                if (multiplicity == SummonMultiplicity.One)
                {
                    if (spawn.Dice != "Zero" || spawn.BonusKind != "Simple" || spawn.BonusValue != 1)
                        failures.Add(where + "not-one:" + spawn.Dice + "+" + spawn.BonusKind + spawn.BonusValue);
                    continue;
                }
                string die = multiplicity == SummonMultiplicity.OneD3 ? "D3" : "D4";
                int bonus = multiplicity == SummonMultiplicity.OneD3 ? 0 : 1;
                if (spawn.Dice != die || spawn.CountKind != "Simple" || spawn.CountValue != 1)
                    failures.Add(where + "wrong-die:" + spawn.CountKind + spawn.CountValue + spawn.Dice + "/1" + die);
                if (spawn.BonusKind != "Rank" || spawn.BonusRank != "ProjectilesCount")
                    failures.Add(where + "wrong-bonus-form:" + spawn.BonusKind + ":" + spawn.BonusRank);
                if (evaluatedRankBonus != bonus)
                    failures.Add(where + "wrong-bonus-value:" + evaluatedRankBonus + "/" + bonus);
            }
            if (multiplicity == SummonMultiplicity.One) { minimum = 1; maximum = 1; }
            else if (multiplicity == SummonMultiplicity.OneD3) { minimum = 1 + evaluatedRankBonus; maximum = 3 + evaluatedRankBonus; }
            else { minimum = 1 + evaluatedRankBonus; maximum = 4 + evaluatedRankBonus; }
            return failures;
        }

        /// <summary>The actual output: within the expression's range, distinct live units of the expected blueprint, nothing foreign.</summary>
        internal static IList<string> Output(int minimum, int maximum, string expectedUnit,
            IList<string> capturedUnits, IList<string> capturedIds, IList<bool> live)
        {
            var failures = new List<string>();
            int count = capturedUnits == null ? 0 : capturedUnits.Count;
            if (count < minimum || count > maximum) failures.Add("quantity:" + count + " outside " + minimum + ".." + maximum);
            if (capturedIds != null && capturedIds.Distinct(StringComparer.Ordinal).Count() != capturedIds.Count)
                failures.Add("duplicate-spawn-event");
            for (int index = 0; index < count; index++)
            {
                if (!string.Equals(capturedUnits[index], expectedUnit, StringComparison.Ordinal))
                    failures.Add("foreign-unit:" + capturedUnits[index]);
                if (live == null || index >= live.Count || !live[index]) failures.Add("not-live:" + index);
            }
            return failures;
        }
    }

    internal static class InheritedDurationContract
    {
        /// <summary>
        /// Correction 3. observations: (unit id, base seconds, bonus seconds)
        /// from the summon rule, one per spawned unit. An empty expectation
        /// (cancellation) requires no observation.
        /// </summary>
        internal static IList<string> Judge(IList<string> spawnedUnitIds,
            IList<Tuple<string, double, double>> observations, double expectedBase, double expectedBonus)
        {
            var failures = new List<string>();
            observations = observations ?? new List<Tuple<string, double, double>>();
            spawnedUnitIds = spawnedUnitIds ?? new List<string>();
            if (observations.Count != spawnedUnitIds.Count)
                failures.Add("duration-observations:" + observations.Count + "/" + spawnedUnitIds.Count);
            foreach (string unit in spawnedUnitIds)
            {
                Tuple<string, double, double>[] mine = observations.Where(value => value.Item1 == unit).ToArray();
                if (mine.Length != 1) { failures.Add("duration-for-unit:" + unit + ":" + mine.Length); continue; }
                if (Math.Abs(mine[0].Item2 - expectedBase) > 0.05 || Math.Abs(mine[0].Item3 - expectedBonus) > 0.05)
                    failures.Add("duration:" + unit + ":" + mine[0].Item2 + "+" + mine[0].Item3 + "/" + expectedBase + "+" + expectedBonus);
            }
            foreach (Tuple<string, double, double> value in observations.Where(value => !spawnedUnitIds.Contains(value.Item1)))
                failures.Add("duration-for-other-unit:" + value.Item1);
            return failures;
        }
    }

    internal static class InheritedBuffSetContract
    {
        /// <summary>
        /// Correction 4. expectation: "none", "celestial" or "fiendish".
        /// buffs: every buff identity on the unit, with repeats.
        /// projectTemplates and the two smite sets: every registered KMG
        /// summon template and celestial/fiendish smite identity.
        /// </summary>
        internal static IList<string> Judge(string expectation, IList<string> buffs, ICollection<string> nativeIds,
            ICollection<string> projectTemplates, ICollection<string> celestialSmites, ICollection<string> fiendishSmites,
            string expectedTemplate, string expectedSmite)
        {
            var failures = new List<string>();
            buffs = buffs ?? new List<string>();
            string[] native = buffs.Where(nativeIds.Contains).ToArray();
            string[] templates = buffs.Where(projectTemplates.Contains).ToArray();
            string[] celestial = buffs.Where(celestialSmites.Contains).ToArray();
            string[] fiendish = buffs.Where(fiendishSmites.Contains).ToArray();
            if (native.Length != 0) failures.Add("native-template:" + string.Join(",", native));
            if (expectation == "none")
            {
                if (templates.Length != 0) failures.Add("project-template:" + string.Join(",", templates));
                if (celestial.Length + fiendish.Length != 0) failures.Add("project-smite:" + string.Join(",", celestial.Concat(fiendish).ToArray()));
                return failures;
            }
            string[] ownSmites = expectation == "celestial" ? celestial : fiendish;
            string[] otherSmites = expectation == "celestial" ? fiendish : celestial;
            if (string.IsNullOrEmpty(expectedTemplate) || !templates.SequenceEqual(new[] { expectedTemplate }))
                failures.Add("template-set:" + string.Join(",", templates) + "/" + expectedTemplate);
            if (string.IsNullOrEmpty(expectedSmite) || !ownSmites.SequenceEqual(new[] { expectedSmite }))
                failures.Add("smite-set:" + string.Join(",", ownSmites) + "/" + expectedSmite);
            if (otherSmites.Length != 0) failures.Add("opposite-smite:" + string.Join(",", otherSmites));
            return failures;
        }
    }

    /// <summary>The identities one caster-chosen project template action wires, as read from a child graph.</summary>
    internal sealed class InheritedDynamicTemplateView
    {
        internal InheritedDynamicTemplateView(string celestialTemplate, string fiendishTemplate, string celestialSmite,
            string fiendishSmite, string[] replacedNative)
        {
            CelestialTemplate = celestialTemplate; FiendishTemplate = fiendishTemplate;
            CelestialSmite = celestialSmite; FiendishSmite = fiendishSmite;
            ReplacedNative = replacedNative ?? new string[0];
        }

        internal string CelestialTemplate { get; private set; }
        internal string FiendishTemplate { get; private set; }
        internal string CelestialSmite { get; private set; }
        internal string FiendishSmite { get; private set; }
        internal string[] ReplacedNative { get; private set; }
    }

    /// <summary>
    /// DEF-05 third review: the deliberate project template expected for a
    /// templated control, derived independently of the child graph under test.
    /// The band is restated from the established unit-HD contract (below 5 Low,
    /// 5 to 10 Mid, above 10 High; SummonTemplateBandPolicy agrees, checked by a
    /// domain test) from the unit's own class levels, and the identities are the
    /// stable registered symbols KMG.Summoning.Template.{Celestial,Fiendish}.Band
    /// and KMG.Summoning.Smite.{Celestial,Fiendish}.Available. The graph check
    /// and the live buff-set check (InheritedBuffSetContract, fed these expected
    /// identities) fail independently.
    /// </summary>
    internal static class InheritedDeliberateTemplateContract
    {
        internal static string Band(int hitDice)
        {
            if (hitDice < 0) throw new ArgumentOutOfRangeException("hitDice");
            return hitDice < 5 ? "Low" : hitDice <= 10 ? "Mid" : "High";
        }

        internal static IList<string> Graph(IList<InheritedDynamicTemplateView> actions, string celestialTemplate,
            string fiendishTemplate, string celestialSmite, string fiendishSmite, ICollection<string> nativeIds)
        {
            var failures = new List<string>();
            if (actions == null || actions.Count != 1)
            {
                failures.Add("dynamic-template-actions:" + (actions == null ? 0 : actions.Count));
                if (actions == null || actions.Count == 0) return failures;
            }
            InheritedDynamicTemplateView action = actions[0];
            Action<string, string, string> same = (name, actual, expected) =>
            {
                if (string.IsNullOrEmpty(expected) || !string.Equals(actual, expected, StringComparison.Ordinal))
                    failures.Add(name + ":" + (actual ?? "<null>") + "/" + (expected ?? "<unresolved>"));
            };
            same("celestial-template", action.CelestialTemplate, celestialTemplate);
            same("fiendish-template", action.FiendishTemplate, fiendishTemplate);
            same("celestial-smite", action.CelestialSmite, celestialSmite);
            same("fiendish-smite", action.FiendishSmite, fiendishSmite);
            if (action.ReplacedNative.Length != nativeIds.Count || action.ReplacedNative.Any(id => id == null || !nativeIds.Contains(id)) ||
                action.ReplacedNative.Distinct(StringComparer.Ordinal).Count() != action.ReplacedNative.Length)
                failures.Add("replaced-native:" + string.Join(",", action.ReplacedNative.Select(id => id ?? "<null>").ToArray()));
            return failures;
        }
    }

    internal static class InheritedChildSetContract
    {
        /// <summary>
        /// Correction 5. expected: projection symbol to (placement key, spell
        /// family). actual: builder creature-child symbol to (placement key or
        /// null, spell family or null).
        /// </summary>
        internal static IList<string> Compare(string family, SummonFamily spellFamily,
            IDictionary<string, Tuple<string, SummonFamily?>> expected,
            IDictionary<string, Tuple<string, SummonFamily?>> actual)
        {
            var failures = new List<string>();
            foreach (string symbol in expected.Keys.Where(value => !actual.ContainsKey(value)).OrderBy(value => value, StringComparer.Ordinal))
                failures.Add(family + "|" + symbol + ":missing-child");
            foreach (string symbol in actual.Keys.Where(value => !expected.ContainsKey(value)).OrderBy(value => value, StringComparer.Ordinal))
                failures.Add(family + "|" + symbol + ":extra-child");
            foreach (var entry in actual.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                if (entry.Value == null || entry.Value.Item1 == null) failures.Add(family + "|" + entry.Key + ":null-placement");
                else if (entry.Value.Item2 != spellFamily) failures.Add(family + "|" + entry.Key + ":wrong-placement-family");
            }
            // A family may map two of its tiers to one placement; a duplicate is more actual children
            // for a placement than the projection expects.
            Dictionary<string, int> expectedPerPlacement = expected.Values.Where(value => value != null && value.Item1 != null)
                .GroupBy(value => value.Item1).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            foreach (var shared in actual.Where(entry => entry.Value != null && entry.Value.Item1 != null)
                .GroupBy(entry => entry.Value.Item1))
            {
                int allowed;
                if (!expectedPerPlacement.TryGetValue(shared.Key, out allowed)) allowed = 1;
                if (shared.Count() > allowed)
                    failures.Add(family + "|" + shared.Key + ":duplicate-placement:" + string.Join(",", shared.Select(entry => entry.Key).ToArray()));
            }
            return failures;
        }
    }
}
