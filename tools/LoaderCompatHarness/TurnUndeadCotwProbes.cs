using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>Substitute channel-engine types for driving the product's CotW adapter through its failure boundary.</summary>
    internal static class TurnUndeadDriftEngine
    {
        // A valid DC holder first (so the adapter mutates a list), then a required member missing: no sacred_conduit.
        public static BlueprintFeature improved_channel;
    }

    internal static class TurnUndeadOptionalNullEngine
    {
        public static BlueprintFeature improved_channel;
        public static BlueprintFeature sacred_conduit;
        public static BlueprintBuff consecrate_buff; // left null: an optional holder CotW did not create
        public static BlueprintUnitFact[] channel_resistances;
    }

    internal sealed partial class TurnUndeadProbe
    {
        private TurnUndeadProbe abandonNested;
        private bool injectCleanupFailure;

        // ---------------------------------------------------------------- menu placement (independent of child.Parent)

        /// <summary>
        /// The wrapper each source's Turn child must sit under, resolved from the observed grant graph: the ability
        /// granted by the source feature whose AbilityVariants contains that source's native harm. Null when the
        /// profile has no such wrapper (the direct action is then the visible entry).
        /// </summary>
        private BlueprintAbility ExpectedWrapper(SourceExpectation expected, out int candidates)
        {
            var feature = Blueprint<BlueprintFeature>(expected.Feature);
            var harm = Blueprint<BlueprintAbility>(expected.NativeHarm);
            BlueprintAbility[] wrappers = feature.GetComponents<AddFacts>().SelectMany(a => a.Facts ?? new BlueprintUnitFact[0])
                .OfType<BlueprintAbility>().Where(a =>
                {
                    AbilityVariants v = a.GetComponent<AbilityVariants>();
                    return v != null && v.Variants != null && v.Variants.Contains(harm);
                }).Distinct().ToArray();
            candidates = wrappers.Length;
            return wrappers.Length == 1 ? wrappers[0] : null;
        }

        private List<string> MenuPlacementProblems(JArray observed)
        {
            var issues = new List<string>();
            var allWrappers = new List<BlueprintAbility>();
            foreach (SourceExpectation e in ExpectedSources) { int n; BlueprintAbility w = ExpectedWrapper(e, out n); if (w != null) allWrappers.Add(w); }
            foreach (SourceExpectation expected in ExpectedSources)
            {
                object source = SourceOf(expected.Kind);
                var child = (BlueprintAbility)Member(source, "MenuChild");
                var direct = (BlueprintAbility)Member(source, "Direct");
                int candidates;
                BlueprintAbility wrapper = ExpectedWrapper(expected, out candidates);
                AbilityVariants variants = wrapper == null ? null : wrapper.GetComponent<AbilityVariants>();
                int inExpected = variants == null ? 0 : variants.Variants.Count(v => v == child);
                int inOthers = allWrappers.Where(w => w != wrapper).Sum(w => w.GetComponent<AbilityVariants>().Variants.Count(v => v == child));
                if (observed != null)
                    observed.Add(new JObject
                    {
                        ["kind"] = expected.Kind, ["wrapperCandidates"] = candidates,
                        ["expectedWrapper"] = wrapper == null ? null : wrapper.AssetGuid + ":" + wrapper.name,
                        ["childParent"] = child.Parent == null ? null : child.Parent.AssetGuid + ":" + child.Parent.name,
                        ["childInExpectedWrapper"] = inExpected, ["childInOtherWrappers"] = inOthers,
                        ["wrapperVariants"] = variants == null ? 0 : variants.Variants.Length, ["directHidden"] = direct.Hidden
                    });
                if (candidates > 1) issues.Add(expected.Kind + ": " + candidates + " positive-harm wrappers");
                if (wrapper != null)
                {
                    if (child.Parent != wrapper) issues.Add(expected.Kind + ": child parent is " + (child.Parent == null ? "none" : child.Parent.name) + ", expected " + wrapper.name);
                    if (inExpected != 1) issues.Add(expected.Kind + ": child appears " + inExpected + " times in " + wrapper.name);
                    if (!direct.Hidden) issues.Add(expected.Kind + ": direct action visible beside the menu child");
                }
                else
                {
                    if (child.Parent != null) issues.Add(expected.Kind + ": child has a parent but the profile has no wrapper");
                    if (direct.Hidden) issues.Add(expected.Kind + ": direct action hidden without a menu child");
                }
                if (inOthers != 0) issues.Add(expected.Kind + ": child appears in another source's wrapper");
            }
            return issues;
        }

        // ---------------------------------------------------------------- CotW adapter failure boundary

        private sealed class Snapshot
        {
            internal readonly Dictionary<string, object[]> Lists = new Dictionary<string, object[]>();
            internal readonly Dictionary<string, object> Values = new Dictionary<string, object>();
        }

        private Snapshot Capture(Type realEngine)
        {
            var snap = new Snapshot();
            foreach (SourceExpectation e in ExpectedSources)
            {
                object source = SourceOf(e.Kind);
                var child = (BlueprintAbility)Member(source, "MenuChild");
                var direct = (BlueprintAbility)Member(source, "Direct");
                int n;
                BlueprintAbility wrapper = ExpectedWrapper(e, out n);
                snap.Values[e.Kind + ".parent"] = child.Parent;
                snap.Values[e.Kind + ".hidden"] = direct.Hidden;
                if (wrapper != null) snap.Lists[e.Kind + ".variants"] = wrapper.GetComponent<AbilityVariants>().Variants.Cast<object>().ToArray();
            }
            foreach (string field in new[] { "improved_channel", "sacred_conduit", "consecrate_buff" })
            {
                var holder = realEngine.GetField(field, Any).GetValue(null) as BlueprintUnitFact;
                if (holder == null) continue;
                int i = 0;
                foreach (BlueprintComponent c in holder.ComponentsArray.Where(c => c != null && c.GetType().Name == "IncreaseSpecifiedSpellsDC"))
                    snap.Lists[field + "." + (i++)] = ((IEnumerable)c.GetType().GetField("spells", Any).GetValue(c)).Cast<object>().ToArray();
            }
            var facts = realEngine.GetField("channel_resistances", Any).GetValue(null) as IEnumerable<BlueprintUnitFact>;
            foreach (BlueprintUnitFact fact in facts ?? new BlueprintUnitFact[0])
            {
                int i = 0;
                foreach (BlueprintComponent c in fact.ComponentsArray.Where(c => c != null && (c.GetType().Name == "SavingThrowBonusAgainstSpecificSpells" || c.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells")))
                    snap.Lists["resistance." + fact.AssetGuid + "." + (i++)] = ((IEnumerable)c.GetType().GetField("Spells", Any).GetValue(c)).Cast<object>().ToArray();
            }
            return snap;
        }

        /// <summary>Differences between two snapshots: lists compared by exact element identity and order.</summary>
        private static List<string> Diff(Snapshot a, Snapshot b)
        {
            var diff = new List<string>();
            foreach (string key in a.Values.Keys.Union(b.Values.Keys))
            {
                object x, y; a.Values.TryGetValue(key, out x); b.Values.TryGetValue(key, out y);
                if (!Equals(x, y)) diff.Add(key);
            }
            foreach (string key in a.Lists.Keys.Union(b.Lists.Keys))
            {
                object[] x, y; a.Lists.TryGetValue(key, out x); b.Lists.TryGetValue(key, out y);
                if (x == null || y == null || x.Length != y.Length || x.Where((v, i) => !ReferenceEquals(v, y[i])).Any()) diff.Add(key);
            }
            return diff;
        }

        private IEnumerable<object> CotwRollback()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            Type module = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            Type adapter = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadCotwPublication", true);
            MethodInfo publish = adapter.GetMethod("PublishWithEngine", Any);
            MethodInfo rollback = adapter.GetMethod("Rollback", Any);
            object live = module.GetProperty("CotwPublication", Any).GetValue(null, null);
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null || live == null || publish == null || rollback == null) { problems.Add("CotW, the live publication or the adapter seam is unavailable"); yield break; }
            Type real = cotw.GetType("CallOfTheWild.ChannelEnergyEngine", true);
            var steps = new JObject();
            result["cotwRollback"] = steps;

            Snapshot published = Capture(real);
            List<string> livePlacement = MenuPlacementProblems(null);
            steps["livePlacementProblems"] = new JArray(livePlacement.Cast<object>().ToArray());
            if (livePlacement.Count != 0) problems.Add("live CotW placement failed the checker: " + string.Join("; ", livePlacement.ToArray()));

            // 1. Roll the live publication back: the original CotW state.
            rollback.Invoke(live, null);
            Snapshot original = Capture(real);
            steps["liveRollbackChanged"] = new JArray(Diff(published, original).Cast<object>().ToArray());
            List<string> removed = MenuPlacementProblems(null);
            steps["checkerAfterRemoval"] = new JArray(removed.Cast<object>().ToArray());
            if (removed.Count < ExpectedSources.Length) problems.Add("the placement checker accepted removed menu children");
            yield return null;

            // 2. Drift after mutation: menu children placed and improved_channel appended, then sacred_conduit missing.
            TurnUndeadDriftEngine.improved_channel = (BlueprintFeature)real.GetField("improved_channel", Any).GetValue(null);
            string driftError = null;
            try { publish.Invoke(null, new object[] { set, typeof(TurnUndeadDriftEngine), "drift-test" }); }
            catch (TargetInvocationException e) { driftError = e.InnerException == null ? e.Message : e.InnerException.Message; }
            Snapshot afterDrift = Capture(real);
            List<string> driftDiff = Diff(original, afterDrift);
            steps["drift"] = new JObject
            {
                ["threw"] = driftError != null, ["message"] = driftError,
                ["differencesFromOriginal"] = new JArray(driftDiff.Cast<object>().ToArray())
            };
            if (driftError == null || driftError.IndexOf("sacred_conduit", StringComparison.Ordinal) < 0)
                problems.Add("the drift engine did not fail on the missing sacred_conduit member: " + (driftError ?? "no exception"));
            if (driftDiff.Count != 0) problems.Add("drift rollback left changes: " + string.Join(", ", driftDiff.ToArray()));
            yield return null;

            // 3. Optional-null holder, then an already-populated second publication, then their rollbacks.
            TurnUndeadOptionalNullEngine.improved_channel = (BlueprintFeature)real.GetField("improved_channel", Any).GetValue(null);
            TurnUndeadOptionalNullEngine.sacred_conduit = (BlueprintFeature)real.GetField("sacred_conduit", Any).GetValue(null);
            TurnUndeadOptionalNullEngine.consecrate_buff = null;
            TurnUndeadOptionalNullEngine.channel_resistances = ((IEnumerable<BlueprintUnitFact>)real.GetField("channel_resistances", Any).GetValue(null)).ToArray();
            object first = publish.Invoke(null, new object[] { set, typeof(TurnUndeadOptionalNullEngine), "optional-null-test" });
            Snapshot afterFirst = Capture(real);
            object second = publish.Invoke(null, new object[] { set, typeof(TurnUndeadOptionalNullEngine), "already-populated-test" });
            Snapshot afterSecond = Capture(real);
            List<string> placementOptional = MenuPlacementProblems(null);
            steps["optionalNull"] = new JObject
            {
                ["evidence"] = new JArray(((IEnumerable<string>)Member(first, "Evidence")).Cast<object>().ToArray()),
                ["placementProblems"] = new JArray(placementOptional.Cast<object>().ToArray()),
                ["secondPublicationEvidence"] = new JArray(((IEnumerable<string>)Member(second, "Evidence")).Cast<object>().ToArray()),
                ["secondPublicationChanged"] = new JArray(Diff(afterFirst, afterSecond).Cast<object>().ToArray())
            };
            if (!((IEnumerable<string>)Member(first, "Evidence")).Contains("consecrate_buff=absent")) problems.Add("the optional-null holder was not reported absent");
            if (placementOptional.Count != 0) problems.Add("optional-null publication placement failed: " + string.Join("; ", placementOptional.ToArray()));
            if (Diff(afterFirst, afterSecond).Count != 0) problems.Add("an already-populated publication changed state");
            rollback.Invoke(second, null);
            rollback.Invoke(first, null);
            List<string> optionalDiff = Diff(original, Capture(real));
            if (optionalDiff.Count != 0) problems.Add("optional-null rollback left changes: " + string.Join(", ", optionalDiff.ToArray()));
            yield return null;

            // 4. Restore the live integration with the real engine; it must equal the published state.
            object restored = publish.Invoke(null, new object[] { set, real, "restored" });
            List<string> restoredDiff = Diff(published, Capture(real));
            steps["restoredDifferencesFromPublished"] = new JArray(restoredDiff.Cast<object>().ToArray());
            if (restoredDiff.Count != 0) problems.Add("re-publication differs from the original publication: " + string.Join(", ", restoredDiff.ToArray()));

            // 5. Swapped parents must fail the checker; restoring them must pass.
            var cleric = (BlueprintAbility)Member(SourceOf("Cleric"), "MenuChild");
            var paladin = (BlueprintAbility)Member(SourceOf("Paladin"), "MenuChild");
            BlueprintAbility clericParent = cleric.Parent, paladinParent = paladin.Parent;
            cleric.Parent = paladinParent; paladin.Parent = clericParent;
            List<string> swapped = MenuPlacementProblems(null);
            cleric.Parent = clericParent; paladin.Parent = paladinParent;
            List<string> unswapped = MenuPlacementProblems(null);
            steps["checkerSwapped"] = new JArray(swapped.Cast<object>().ToArray());
            steps["checkerUnswapped"] = new JArray(unswapped.Cast<object>().ToArray());
            if (!swapped.Any(s => s.StartsWith("Cleric")) || !swapped.Any(s => s.StartsWith("Paladin"))) problems.Add("the placement checker accepted swapped parents");
            if (unswapped.Count != 0) problems.Add("placement after unswapping failed: " + string.Join("; ", unswapped.ToArray()));
            steps["restoredAvailability"] = (string)Member(restored, "Availability");
        }

        // ---------------------------------------------------------------- interrupted abandon check

        private IEnumerable<object> AbandonInterrupt()
        {
            var cases = new JArray();
            result["abandonInterrupt"] = cases;
            foreach (bool inject in new[] { false, true })
            {
                Game.Instance.IsPaused = false;
                var outer = new TurnUndeadProbe("abandon") { injectNestedCleanupFailureForTest = inject };
                int polls = 0;
                while (polls++ < 600 && !(outer.abandonNested != null && outer.abandonNested.silenced.Count > 0))
                {
                    string verdict = outer.Poll();
                    if (verdict != null) { problems.Add("the outer abandon step finished before interruption: " + verdict); yield break; }
                    if (!(outer.abandonNested != null && outer.abandonNested.silenced.Count > 0)) yield return null;
                }
                TurnUndeadProbe nested = outer.abandonNested;
                UnitEntityData[] silencedUnits = nested.silenced.ToArray();
                UnitEntityData[] spawned = nested.created.ToArray();
                bool pausedAtInterrupt = Game.Instance.IsPaused;
                CleanupReport report = outer.Release("interrupted after the nested probe's first yield");
                int frames = 0;
                while (frames < 30 && spawned.Any(u => u != null && !u.Destroyed)) { frames++; yield return null; }
                var row = new JObject
                {
                    ["injectedNestedFailure"] = inject, ["polls"] = polls, ["pausedAtInterrupt"] = pausedAtInterrupt,
                    ["pauseAfterRelease"] = Game.Instance.IsPaused, ["silenced"] = silencedUnits.Length,
                    ["brainsRestored"] = silencedUnits.Count(u => u != null && !u.Destroyed && u.IsBrainActive),
                    ["nestedSubscriptionReleased"] = nested.subscription == null, ["outerReleasedNested"] = outer.abandonNested == null,
                    ["spawned"] = spawned.Length, ["spawnedRemaining"] = spawned.Count(u => u != null && !u.Destroyed),
                    ["cleanupAttempted"] = new JArray(report.Attempted.Cast<object>().ToArray()),
                    ["cleanupFailures"] = new JArray(report.Failures.Cast<object>().ToArray()), ["cleanupRestored"] = report.Restored
                };
                cases.Add(row);
                string label = inject ? "injected-failure case" : "clean case";
                if (!pausedAtInterrupt) problems.Add(label + ": the nested probe had not paused the game at the interruption");
                if (Game.Instance.IsPaused) problems.Add(label + ": the prior pause flag (false) was not restored");
                if (silencedUnits.Any(u => u != null && !u.Destroyed && !u.IsBrainActive)) problems.Add(label + ": silenced brains left off");
                if (nested.subscription != null) problems.Add(label + ": the nested rule subscription was left open");
                if (spawned.Any(u => u != null && !u.Destroyed)) problems.Add(label + ": spawned units left");
                if (!inject && !report.Restored) problems.Add("clean case: the cleanup verdict reported failures");
                if (inject && (report.Restored || !report.Failures.Any(f => f.StartsWith("nested probe")))) problems.Add("injected-failure case: a failed nested cleanup still produced a restored verdict");
            }
        }

        private bool injectNestedCleanupFailureForTest;

        // ---------------------------------------------------------------- generic component dump (contracts extract)

        private static JToken Dump(object value, int depth)
        {
            if (value == null) return null;
            if (value is string || value is bool || value is int || value is float || value is double || value is long) return JToken.FromObject(value);
            if (value is Enum) return value.ToString();
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return blueprint.AssetGuid + ":" + blueprint.name;
            if (value is Kingmaker.Utility.Feet) return ((Kingmaker.Utility.Feet)value).Value + " ft";
            if (depth <= 0) return value.GetType().Name;
            var list = value as IEnumerable;
            if (list != null) return new JArray(list.Cast<object>().Take(80).Select(v => (object)Dump(v, depth - 1)).ToArray());
            var obj = new JObject { ["$type"] = value.GetType().Name };
            var component = value as BlueprintComponent;
            if (component != null) obj["$name"] = component.name;
            for (Type t = value.GetType(); t != null && t != typeof(BlueprintComponent) && t != typeof(UnityEngine.ScriptableObject) && t != typeof(object); t = t.BaseType)
                foreach (FieldInfo field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsNotSerialized || field.Name.Contains("<") || obj[field.Name] != null) continue;
                    if (!field.IsPublic && field.GetCustomAttributes(typeof(UnityEngine.SerializeField), true).Length == 0) continue;
                    try { obj[field.Name] = Dump(field.GetValue(value), depth - 1); }
                    catch (Exception e) { obj[field.Name] = "unreadable: " + e.GetType().Name; }
                }
            return obj;
        }

        private static JObject DumpFact(BlueprintScriptableObject fact)
        {
            if (fact == null) return null;
            return new JObject
            {
                ["guid"] = fact.AssetGuid, ["name"] = fact.name,
                ["components"] = new JArray(fact.ComponentsArray.Where(c => c != null).Select(c => (object)Dump(c, 5)).ToArray())
            };
        }
    }
}
