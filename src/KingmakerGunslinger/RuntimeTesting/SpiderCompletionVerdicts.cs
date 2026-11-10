using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Pure verdicts for the Spider Summoner poison exposures. An exposure
    /// names its expected poisons by identity ("expected": feature, the state
    /// buff the feature applies on a failed save, and that state's saves per
    /// tick). Every first save and every continuing save is attributed to one
    /// expected poison, and every expected poison needs its own complete
    /// coverage. Anything missing, unattributed or repeated is a named problem.
    /// </summary>
    internal static class SpiderPoisonObservationVerdict
    {
        internal sealed class ExpectedPoison
        {
            internal string Feature; internal string State; internal int PerTick;
        }

        internal static List<ExpectedPoison> Expected(JObject exposure, out string problem)
        {
            problem = null;
            var result = new List<ExpectedPoison>();
            JArray expected = exposure?["expected"] as JArray;
            if (expected == null || expected.Count == 0) { problem = "no-expected-poisons"; return result; }
            foreach (JObject item in expected.OfType<JObject>())
            {
                string feature = Text(item["feature"]), state = Text(item["state"]);
                JToken perTick = item["perTick"];
                if (feature == null) { problem = "expected-without-feature"; return result; }
                if (state == null) { problem = "no-state-identity:" + feature; return result; }
                if (perTick == null || perTick.Type != JTokenType.Integer || (int)perTick < 1) { problem = "no-continuing-save-shape:" + feature; return result; }
                result.Add(new ExpectedPoison { Feature = feature, State = state, PerTick = (int)perTick });
            }
            if (result.Count != expected.Count) problem = "malformed-expected";
            else if (result.Select(value => value.Feature).Distinct(StringComparer.Ordinal).Count() != result.Count) problem = "repeated-expected-feature";
            else if (result.Select(value => value.State).Distinct(StringComparer.Ordinal).Count() != result.Count) problem = "shared-expected-state";
            return result;
        }

        /// <summary>The one first save attributed to this feature, or why not.</summary>
        internal static string FirstSave(JObject exposure, string feature, out int dc)
        {
            dc = 0;
            JObject[] saves = Saves(exposure, "firstSaves").Where(item => Text(item["reasonFact"]) == feature).ToArray();
            if (saves.Length != 1) return "first-saves:" + feature + "=" + saves.Length;
            JToken number = saves[0]["dc"];
            if (number == null || number.Type != JTokenType.Integer) return "first-dc-missing:" + feature;
            dc = (int)number;
            return null;
        }

        /// <summary>
        /// Null when each expected poison has exactly one failed first save,
        /// exactly one applied state, and exactly its per-tick number of
        /// continuing saves from that state at its own first-save DC, with no
        /// save from anything else; otherwise the first reason why not.
        /// </summary>
        internal static string AttributionProblem(JObject exposure, IEnumerable<string> requiredFeatures)
        {
            if (exposure == null) return "no-exposure";
            if (Text(exposure["error"]) != null) return "error:" + Text(exposure["error"]);
            string problem;
            List<ExpectedPoison> expected = Expected(exposure, out problem);
            if (problem != null) return problem;
            foreach (string required in requiredFeatures ?? Enumerable.Empty<string>())
                if (!expected.Any(value => value.Feature == required)) return "required-poison-not-expected:" + required;
            var features = new HashSet<string>(expected.Select(value => value.Feature), StringComparer.Ordinal);
            var states = new HashSet<string>(expected.Select(value => value.State), StringComparer.Ordinal);
            JObject[] first = Saves(exposure, "firstSaves").ToArray(), continuing = Saves(exposure, "continuingSaves").ToArray();
            foreach (JObject save in first) if (!features.Contains(Text(save["reasonFact"]) ?? "")) return "unattributed-first-save:" + (Text(save["reasonFact"]) ?? "null");
            foreach (JObject save in continuing) if (!states.Contains(Text(save["reasonFact"]) ?? "")) return "unattributed-continuing-save:" + (Text(save["reasonFact"]) ?? "null");
            string[] applied = (exposure["poisonStates"] as JArray)?.Select(Text).ToArray() ?? new string[0];
            foreach (string state in applied) if (!states.Contains(state ?? "")) return "unexpected-poison-state:" + (state ?? "null");
            foreach (ExpectedPoison poison in expected)
            {
                int dc;
                string firstProblem = FirstSave(exposure, poison.Feature, out dc);
                if (firstProblem != null) return firstProblem;
                JObject save = first.First(value => Text(value["reasonFact"]) == poison.Feature);
                if (save["passed"] == null || save["passed"].Type != JTokenType.Boolean || (bool)save["passed"]) return "first-save-not-failed:" + poison.Feature;
                int appliedCount = applied.Count(value => value == poison.State);
                if (appliedCount != 1) return "states:" + poison.State + "=" + appliedCount;
                JObject[] own = continuing.Where(value => Text(value["reasonFact"]) == poison.State).ToArray();
                if (own.Length != poison.PerTick) return "continuing-saves:" + poison.State + "=" + own.Length + "/" + poison.PerTick;
                foreach (JObject tick in own)
                {
                    JToken value = tick["dc"];
                    if (value == null || value.Type != JTokenType.Integer) return "continuing-dc-missing:" + poison.State;
                    if ((int)value != dc) return "continuing-dc:" + poison.State + ":" + dc + "->" + (int)value;
                }
            }
            return null;
        }

        /// <summary>Null when every expected poison's first-save DC is its
        /// baseline plus the bonus; a missing baseline is a named problem.</summary>
        internal static string BonusProblem(JObject exposure, Func<string, int?> baseline, int bonus)
        {
            string problem;
            List<ExpectedPoison> expected = Expected(exposure, out problem);
            if (problem != null) return problem;
            foreach (ExpectedPoison poison in expected)
            {
                int dc;
                string firstProblem = FirstSave(exposure, poison.Feature, out dc);
                if (firstProblem != null) return firstProblem;
                int? without = baseline(poison.Feature);
                if (without == null) return "no-baseline:" + poison.Feature;
                if (dc != without.Value + bonus) return "dc:" + poison.Feature + ":" + without.Value + "->" + dc;
            }
            return null;
        }

        /// <summary>The feat summon against its matched unmarked summon: the
        /// same expected poisons, each completely attributed, each first DC
        /// exactly the unmarked one plus the bonus.</summary>
        internal static string MatchedProblem(JObject marked, JObject unmarked, IEnumerable<string> requiredFeatures, int bonus)
        {
            string[] required = (requiredFeatures ?? Enumerable.Empty<string>()).ToArray();
            string problem = AttributionProblem(marked, required);
            if (problem != null) return "marked:" + problem;
            problem = AttributionProblem(unmarked, required);
            if (problem != null) return "unmarked:" + problem;
            string ignored;
            var a = new HashSet<string>(Expected(marked, out ignored).Select(value => value.Feature + "/" + value.State + "/" + value.PerTick), StringComparer.Ordinal);
            if (!a.SetEquals(Expected(unmarked, out ignored).Select(value => value.Feature + "/" + value.State + "/" + value.PerTick))) return "expected-poisons-differ";
            return BonusProblem(marked, feature =>
            {
                int dc;
                return FirstSave(unmarked, feature, out dc) == null ? (int?)dc : null;
            }, bonus);
        }

        /// <summary>A species exposure against its own verified unbonused
        /// parameters ("unbonusedDc", recalculated without the marker).</summary>
        internal static string UnbonusedProblem(JObject exposure, IEnumerable<string> requiredFeatures, int bonus)
        {
            string problem = AttributionProblem(exposure, requiredFeatures);
            if (problem != null) return problem;
            JObject unbonused = exposure["unbonusedDc"] as JObject;
            return BonusProblem(exposure, feature =>
            {
                JToken value = unbonused?[feature];
                return value != null && value.Type == JTokenType.Integer ? (int?)(int)value : null;
            }, bonus);
        }

        private static IEnumerable<JObject> Saves(JObject exposure, string key)
        {
            JArray saves = exposure?[key] as JArray;
            return saves == null ? Enumerable.Empty<JObject>() : saves.OfType<JObject>();
        }

        internal static string Text(JToken token)
        { return token == null || token.Type == JTokenType.Null ? null : token.Type == JTokenType.String ? (string)token : token.ToString(); }
    }

    /// <summary>
    /// Pure verdict for the creatures a spider route cast produced: the
    /// expected count, every one the named spider with exactly one benefit,
    /// the family's template behavior (one KMG template for Summon Monster;
    /// none and the caster's alignment for Summon Nature's Ally; never a
    /// native template) and a live, finite summon lifetime.
    /// </summary>
    internal static class SpiderRouteObservationVerdict
    {
        internal static string UnitsProblem(JArray units, int expectedCount, string unitGuid, bool monsterFamily)
        {
            if (units == null) return "no-units-recorded";
            if (units.Count != expectedCount) return "count:" + units.Count + "/" + expectedCount;
            foreach (JObject unit in units.OfType<JObject>())
            {
                string guid = SpiderPoisonObservationVerdict.Text(unit["unit"]);
                if (guid != unitGuid) return "creature:" + (guid ?? "null");
                if (Int(unit["markers"]) != 1) return "markers:" + (Int(unit["markers"])?.ToString() ?? "null");
                if (Int(unit["nativeTemplates"]) != 0) return "native-templates:" + (Int(unit["nativeTemplates"])?.ToString() ?? "null");
                int? templates = Int(unit["templates"]);
                if (templates != (monsterFamily ? 1 : 0)) return "templates:" + (templates?.ToString() ?? "null");
                if (!monsterFamily && SpiderPoisonObservationVerdict.Text(unit["alignment"]) != SpiderPoisonObservationVerdict.Text(unit["casterAlignment"])) return "alignment:" + SpiderPoisonObservationVerdict.Text(unit["alignment"]);
                JToken seconds = unit["secondsLeft"];
                if (seconds == null || (seconds.Type != JTokenType.Float && seconds.Type != JTokenType.Integer)) return "no-lifetime";
                double left = (double)seconds;
                if (!(left > 0 && left < 100000)) return "lifetime:" + left;
            }
            return units.OfType<JObject>().Count() == units.Count ? null : "malformed-unit";
        }

        private static int? Int(JToken token)
        { return token != null && token.Type == JTokenType.Integer ? (int?)(int)token : null; }
    }

    /// <summary>
    /// The duration the engine's own summon rule gave each creature: the
    /// caster level's rounds, twice that when the spell context is extended.
    /// Each record carries <c>casterLevel</c>, <c>extended</c> and
    /// <c>durationSeconds</c> from one <c>RuleSummonUnit</c>.
    /// </summary>
    internal static class SpiderSummonDurationVerdict
    {
        internal static string Problem(JArray rules, int expectedCasterLevel, bool expectedExtend)
        {
            if (rules == null || rules.Count == 0) return "no-summon-rule";
            foreach (JToken token in rules)
            {
                JObject rule = token as JObject;
                if (rule == null) return "malformed-rule";
                int? level = rule["casterLevel"] != null && rule["casterLevel"].Type == JTokenType.Integer ? (int?)(int)rule["casterLevel"] : null;
                if (level != expectedCasterLevel) return "caster-level:" + rule["casterLevel"] + "!=" + expectedCasterLevel;
                if (rule["extended"] == null || rule["extended"].Type != JTokenType.Boolean || (bool)rule["extended"] != expectedExtend) return "extend:" + rule["extended"];
                double expected = 6d * expectedCasterLevel * (expectedExtend ? 2 : 1);
                JToken seconds = rule["durationSeconds"];
                if (seconds == null || (seconds.Type != JTokenType.Float && seconds.Type != JTokenType.Integer) || System.Math.Abs((double)seconds - expected) > 0.01)
                    return "duration:" + seconds + "!=" + expected;
            }
            return null;
        }
    }
}
