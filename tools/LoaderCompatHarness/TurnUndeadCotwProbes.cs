using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>Substitute channel-engine types; their holders are isolated fixture objects, never live CotW blueprints.</summary>
    internal static class TurnUndeadIsolatedEngine
    {
        public static BlueprintFeature improved_channel;
        public static BlueprintFeature sacred_conduit;
        public static BlueprintBuff consecrate_buff;
        public static BlueprintUnitFact[] channel_resistances;
    }

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

    /// <summary>An engine type without a channel registry: the registry reader must report it unreadable.</summary>
    internal static class TurnUndeadMissingRegistryEngine
    {
    }

    internal sealed partial class TurnUndeadProbe
    {
        /// <summary>A test cleanup failure this probe's own Release throws (null: none).</summary>
        private string injectedCleanupFailure;
        /// <summary>The cleanup failure an abandon probe gives its nested first-turn probe (null: none).</summary>
        private string injectedNestedCleanupFailure;

        // ---------------------------------------------------------------- menu placement (independent of child.Parent)

        private sealed class PlacementCase
        {
            internal string Kind;
            internal BlueprintFeature Feature;
            internal BlueprintAbility Harm, Child, Direct, ExtraHarm, ExtraChild;
        }

        /// <summary>Live cases: the source feature and native harm come from the literal expectations, not the product.</summary>
        private List<PlacementCase> LiveCases() => ExpectedSources.Select(e =>
        {
            object source = SourceOf(e.Kind);
            return new PlacementCase
            {
                Kind = e.Kind, Feature = Blueprint<BlueprintFeature>(e.Feature), Harm = Blueprint<BlueprintAbility>(e.NativeHarm),
                Child = (BlueprintAbility)Member(source, "MenuChild"), Direct = (BlueprintAbility)Member(source, "Direct"),
                ExtraHarm = (BlueprintAbility)Member(source, "ExtraHarm"), ExtraChild = (BlueprintAbility)Member(source, "ExtraMenuChild")
            };
        }).ToList();

        private static List<PlacementCase> CasesOf(object anySet) => ((Array)Member(anySet, "Sources")).Cast<object>().Select(source => new PlacementCase
        {
            Kind = Member(Member(source, "Contract"), "Kind").ToString(), Feature = (BlueprintFeature)Member(source, "SourceFeature"),
            Harm = (BlueprintAbility)Member(source, "NativeHarm"), Child = (BlueprintAbility)Member(source, "MenuChild"),
            Direct = (BlueprintAbility)Member(source, "Direct"), ExtraHarm = (BlueprintAbility)Member(source, "ExtraHarm"),
            ExtraChild = (BlueprintAbility)Member(source, "ExtraMenuChild")
        }).ToList();

        /// <summary>
        /// The wrapper a source's Turn child must sit under, resolved from the grant graph: the ability granted by the
        /// source feature whose AbilityVariants contains that source's harm. Null when the profile has no such wrapper.
        /// </summary>
        private static BlueprintAbility ExpectedWrapper(BlueprintFeature feature, BlueprintAbility harm, out int candidates)
        {
            BlueprintAbility[] wrappers = feature.GetComponents<AddFacts>().SelectMany(a => a.Facts ?? new BlueprintUnitFact[0])
                .OfType<BlueprintAbility>().Where(a =>
                {
                    AbilityVariants v = a.GetComponent<AbilityVariants>();
                    return v != null && v.Variants != null && v.Variants.Contains(harm);
                }).Distinct().ToArray();
            candidates = wrappers.Length;
            return wrappers.Length == 1 ? wrappers[0] : null;
        }

        private static List<string> PlacementProblems(List<PlacementCase> cases, JArray observed)
        {
            var issues = new List<string>();
            var allWrappers = new List<BlueprintAbility>();
            foreach (PlacementCase c in cases) { int n; BlueprintAbility w = ExpectedWrapper(c.Feature, c.Harm, out n); if (w != null) allWrappers.Add(w); }
            foreach (PlacementCase c in cases)
            {
                int candidates;
                BlueprintAbility wrapper = ExpectedWrapper(c.Feature, c.Harm, out candidates);
                AbilityVariants variants = wrapper == null ? null : wrapper.GetComponent<AbilityVariants>();
                Func<BlueprintAbility, int> inExpected = child => variants == null ? 0 : variants.Variants.Count(v => v == child);
                Func<BlueprintAbility, int> inOthers = child => allWrappers.Where(w => w != wrapper).Sum(w => w.GetComponent<AbilityVariants>().Variants.Count(v => v == child));
                if (observed != null)
                    observed.Add(new JObject
                    {
                        ["kind"] = c.Kind, ["wrapperCandidates"] = candidates,
                        ["expectedWrapper"] = wrapper == null ? null : wrapper.AssetGuid + ":" + wrapper.name,
                        ["childParent"] = c.Child.Parent == null ? null : c.Child.Parent.AssetGuid + ":" + c.Child.Parent.name,
                        ["childInExpectedWrapper"] = inExpected(c.Child), ["childInOtherWrappers"] = inOthers(c.Child),
                        ["wrapperVariants"] = variants == null ? 0 : variants.Variants.Length, ["directHidden"] = c.Direct.Hidden,
                        ["extraChild"] = c.ExtraChild == null ? null : new JObject
                        {
                            ["parent"] = c.ExtraChild.Parent == null ? null : c.ExtraChild.Parent.AssetGuid, ["inExpectedWrapper"] = inExpected(c.ExtraChild),
                            ["inOtherWrappers"] = inOthers(c.ExtraChild), ["wrapperHasExtraHarm"] = variants != null && variants.Variants.Contains(c.ExtraHarm)
                        }
                    });
                if (candidates > 1) issues.Add(c.Kind + ": " + candidates + " positive-harm wrappers");
                if (wrapper != null)
                {
                    if (c.Child.Parent != wrapper) issues.Add(c.Kind + ": child parent is " + (c.Child.Parent == null ? "none" : c.Child.Parent.name) + ", expected " + wrapper.name);
                    if (inExpected(c.Child) != 1) issues.Add(c.Kind + ": child appears " + inExpected(c.Child) + " times in " + wrapper.name);
                    if (!c.Direct.Hidden) issues.Add(c.Kind + ": direct action visible beside the menu child");
                }
                else
                {
                    if (c.Child.Parent != null) issues.Add(c.Kind + ": child has a parent but the profile has no wrapper");
                    if (c.Direct.Hidden) issues.Add(c.Kind + ": direct action hidden without a menu child");
                }
                if (inOthers(c.Child) != 0) issues.Add(c.Kind + ": child appears in another source's wrapper");
                if (c.ExtraChild != null)
                {
                    if (wrapper == null || !variants.Variants.Contains(c.ExtraHarm)) issues.Add(c.Kind + ": extra-channel harm is not in the source wrapper");
                    else
                    {
                        if (c.ExtraChild.Parent != wrapper) issues.Add(c.Kind + ": extra-channel child parent is " + (c.ExtraChild.Parent == null ? "none" : c.ExtraChild.Parent.name));
                        if (inExpected(c.ExtraChild) != 1) issues.Add(c.Kind + ": extra-channel child appears " + inExpected(c.ExtraChild) + " times in " + wrapper.name);
                    }
                    if (inOthers(c.ExtraChild) != 0) issues.Add(c.Kind + ": extra-channel child appears in another source's wrapper");
                }
            }
            return issues;
        }

        // ---------------------------------------------------------------- snapshots

        private sealed class Snapshot
        {
            internal readonly Dictionary<string, object[]> Lists = new Dictionary<string, object[]>();
            internal readonly Dictionary<string, object> Values = new Dictionary<string, object>();
        }

        private static Snapshot Capture(List<PlacementCase> cases, Type engine)
        {
            var snap = new Snapshot();
            foreach (PlacementCase c in cases)
            {
                int n;
                BlueprintAbility wrapper = ExpectedWrapper(c.Feature, c.Harm, out n);
                snap.Values[c.Kind + ".parent"] = c.Child.Parent;
                snap.Values[c.Kind + ".hidden"] = c.Direct.Hidden;
                if (c.ExtraChild != null) snap.Values[c.Kind + ".extraParent"] = c.ExtraChild.Parent;
                if (wrapper != null) snap.Lists[c.Kind + ".variants"] = wrapper.GetComponent<AbilityVariants>().Variants.Cast<object>().ToArray();
            }
            foreach (string field in new[] { "improved_channel", "sacred_conduit", "consecrate_buff" })
            {
                FieldInfo info = engine.GetField(field, Any);
                var holder = info == null ? null : info.GetValue(null) as BlueprintUnitFact;
                if (holder == null) continue;
                int i = 0;
                foreach (BlueprintComponent c in holder.ComponentsArray.Where(c => c != null && c.GetType().Name == "IncreaseSpecifiedSpellsDC"))
                    snap.Lists[field + "." + (i++)] = ((IEnumerable)c.GetType().GetField("spells", Any).GetValue(c)).Cast<object>().ToArray();
            }
            FieldInfo resistances = engine.GetField("channel_resistances", Any);
            foreach (BlueprintUnitFact fact in (resistances == null ? null : resistances.GetValue(null) as IEnumerable<BlueprintUnitFact>) ?? new BlueprintUnitFact[0])
            {
                int i = 0;
                foreach (BlueprintComponent c in fact.ComponentsArray.Where(c => c != null && (c.GetType().Name == "SavingThrowBonusAgainstSpecificSpells" || c.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells")))
                    snap.Lists["resistance." + fact.AssetGuid + fact.name + "." + (i++)] = ((IEnumerable)c.GetType().GetField("Spells", Any).GetValue(c)).Cast<object>().ToArray();
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

        private static JArray Strings(IEnumerable<string> values) => new JArray(values.Cast<object>().ToArray());

        // ---------------------------------------------------------------- live CotW publication: observed, never mutated

        private Snapshot liveBaseline;
        private Type liveEngine;
        private object liveHandle;
        private int liveUndo;

        private static int UndoCount(object publication) =>
            publication == null ? -1 : ((ICollection)publication.GetType().GetField("_undo", Any).GetValue(publication)).Count;

        private void RecordLiveBaseline(Type engine)
        {
            Type module = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            liveEngine = engine;
            liveHandle = module.GetProperty("CotwPublication", Any).GetValue(null, null);
            liveUndo = UndoCount(liveHandle);
            liveBaseline = Capture(LiveCases(), engine);
        }

        /// <summary>
        /// The live graph and the module's publication handle (same object, same undo actions) against the baseline.
        /// The probes never touch the live publication, so any difference is a failed isolation.
        /// </summary>
        private List<string> LiveDifferences()
        {
            var issues = new List<string>();
            if (liveBaseline == null) return issues;
            Type module = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            object handle = module.GetProperty("CotwPublication", Any).GetValue(null, null);
            if (!ReferenceEquals(handle, liveHandle)) issues.Add("the module's CotW publication handle changed");
            if (UndoCount(handle) != liveUndo) issues.Add("the module's CotW publication holds " + UndoCount(handle) + " undo actions, expected " + liveUndo);
            issues.AddRange(Diff(liveBaseline, Capture(LiveCases(), liveEngine)).Select(d => "live CotW state changed: " + d));
            return issues;
        }

        private void VerifyLiveCotwUnchanged()
        {
            List<string> issues = LiveDifferences();
            liveBaseline = null;
            if (issues.Count != 0) throw new InvalidOperationException(string.Join("; ", issues.ToArray()));
        }

        // ---------------------------------------------------------------- isolated publication fixture

        private readonly List<UnityEngine.Object> isolatedObjects = new List<UnityEngine.Object>();
        private bool isolatedRolledBack;

        private T Fresh<T>(string name) where T : ScriptableObject
        {
            var value = ScriptableObject.CreateInstance<T>();
            value.name = "KmgTurnUndeadIsolated_" + name;
            isolatedObjects.Add(value);
            return value;
        }

        private BlueprintComponent FreshComponent(Type type, string name)
        {
            var value = (BlueprintComponent)ScriptableObject.CreateInstance(type);
            value.name = "KmgTurnUndeadIsolated_" + name;
            isolatedObjects.Add(value);
            return value;
        }

        private void ReleaseIsolatedFixture()
        {
            foreach (Type engine in new[] { typeof(TurnUndeadIsolatedEngine), typeof(TurnUndeadDriftEngine), typeof(TurnUndeadOptionalNullEngine) })
                foreach (FieldInfo field in engine.GetFields(BindingFlags.Static | BindingFlags.Public)) field.SetValue(null, null);
            foreach (UnityEngine.Object value in isolatedObjects) if (value != null) UnityEngine.Object.DestroyImmediate(value);
            isolatedObjects.Clear();
        }

        /// <summary>
        /// A private copy of the publication surface: per source a fresh feature granting a fresh wrapper whose variants
        /// hold a fresh harm stand-in, fresh direct and child abilities (the paladin also gets an extra-channel harm and
        /// child), all wrapped in the product's own set types so the product's adapter runs unchanged against it.
        /// </summary>
        private object BuildIsolatedSet(Assembly product)
        {
            Type setType = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadBlueprintSet", true);
            Type sourceType = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadSourceBlueprints", true);
            const BindingFlags ctor = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Array live = (Array)Member(set, "Sources");
            Array isolated = Array.CreateInstance(sourceType, live.Length);
            for (int i = 0; i < live.Length; i++)
            {
                object contract = Member(live.GetValue(i), "Contract");
                string kind = Member(contract, "Kind").ToString();
                var harm = Fresh<BlueprintAbility>(kind + "_Harm");
                var wrapper = Fresh<BlueprintAbility>(kind + "_Wrapper");
                var variants = (AbilityVariants)FreshComponent(typeof(AbilityVariants), kind + "_Variants");
                variants.Variants = new[] { harm };
                wrapper.ComponentsArray = new BlueprintComponent[] { variants };
                var grant = (AddFacts)FreshComponent(typeof(AddFacts), kind + "_Grant");
                grant.Facts = new BlueprintUnitFact[] { wrapper };
                var feature = Fresh<BlueprintFeature>(kind + "_Feature");
                feature.ComponentsArray = new BlueprintComponent[] { grant };
                object source = Activator.CreateInstance(sourceType, ctor, null,
                    new object[] { contract, feature, harm, Fresh<BlueprintAbility>(kind + "_Direct"), Fresh<BlueprintAbility>(kind + "_Child") }, null);
                if (kind == "Paladin")
                {
                    var extraHarm = Fresh<BlueprintAbility>(kind + "_ExtraHarm");
                    variants.Variants = new[] { harm, extraHarm };
                    sourceType.GetMethod("AttachExtraRoute", Any).Invoke(source, new object[] { extraHarm, Fresh<BlueprintAbility>(kind + "_ExtraChild") });
                }
                isolated.SetValue(source, i);
            }
            return Activator.CreateInstance(setType, ctor, null,
                new object[] { Fresh<BlueprintFeature>("Feat"), Fresh<BlueprintBuff>("Fleeing"), isolated }, null);
        }

        /// <summary>Isolated modifier holders built from the loaded provider's own component types.</summary>
        private void PopulateIsolatedEngines(Type real)
        {
            var realImproved = (BlueprintUnitFact)real.GetField("improved_channel", Any).GetValue(null);
            Type dcType = realImproved.ComponentsArray.First(c => c != null && c.GetType().Name == "IncreaseSpecifiedSpellsDC").GetType();
            Func<BlueprintUnitFact, string, BlueprintUnitFact> holder = (fact, name) =>
            {
                BlueprintComponent component = FreshComponent(dcType, name + "_Dc");
                dcType.GetField("spells", Any).SetValue(component, new[] { Fresh<BlueprintAbility>(name + "_Listed") });
                fact.ComponentsArray = new[] { component };
                return fact;
            };
            TurnUndeadIsolatedEngine.improved_channel = (BlueprintFeature)holder(Fresh<BlueprintFeature>("ImprovedChannel"), "ImprovedChannel");
            TurnUndeadIsolatedEngine.sacred_conduit = (BlueprintFeature)holder(Fresh<BlueprintFeature>("SacredConduit"), "SacredConduit");
            TurnUndeadIsolatedEngine.consecrate_buff = (BlueprintBuff)holder(Fresh<BlueprintBuff>("Consecrate"), "Consecrate");
            var resistances = new List<BlueprintUnitFact>();
            var native = Fresh<BlueprintFeature>("ResistanceNative");
            var nativeList = (SavingThrowBonusAgainstSpecificSpells)FreshComponent(typeof(SavingThrowBonusAgainstSpecificSpells), "ResistanceNative_List");
            nativeList.Spells = new[] { Fresh<BlueprintAbility>("ResistanceNative_Listed") };
            native.ComponentsArray = new BlueprintComponent[] { nativeList };
            resistances.Add(native);
            Type contextType = ((IEnumerable<BlueprintUnitFact>)real.GetField("channel_resistances", Any).GetValue(null)).SelectMany(f => f.ComponentsArray)
                .Where(c => c != null && c.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells").Select(c => c.GetType()).FirstOrDefault();
            if (contextType != null)
            {
                var context = Fresh<BlueprintFeature>("ResistanceContext");
                BlueprintComponent list = FreshComponent(contextType, "ResistanceContext_List");
                contextType.GetField("Spells", Any).SetValue(list, new[] { Fresh<BlueprintAbility>("ResistanceContext_Listed") });
                context.ComponentsArray = new[] { list };
                resistances.Add(context);
            }
            TurnUndeadIsolatedEngine.channel_resistances = resistances.ToArray();
            TurnUndeadDriftEngine.improved_channel = TurnUndeadIsolatedEngine.improved_channel;
            TurnUndeadOptionalNullEngine.improved_channel = TurnUndeadIsolatedEngine.improved_channel;
            TurnUndeadOptionalNullEngine.sacred_conduit = TurnUndeadIsolatedEngine.sacred_conduit;
            TurnUndeadOptionalNullEngine.consecrate_buff = null;
            TurnUndeadOptionalNullEngine.channel_resistances = TurnUndeadIsolatedEngine.channel_resistances;
        }

        // ---------------------------------------------------------------- CotW adapter boundary (isolated fixture)

        private IEnumerable<object> CotwRollback()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            Type module = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            Type adapter = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadCotwPublication", true);
            MethodInfo publish = adapter.GetMethod("PublishWithEngine", Any);
            MethodInfo rollback = adapter.GetMethod("Rollback", Any);
            MethodInfo reported = module.GetMethod("PublishCotwWithEngine", Any);
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null || publish == null || rollback == null || reported == null) { problems.Add("CotW or a product seam is unavailable"); yield break; }
            Type real = cotw.GetType("CallOfTheWild.ChannelEnergyEngine", true);
            var steps = new JObject();
            result["cotwRollback"] = steps;

            // The live publication is observed only: its graph and the module's handle must be unchanged at the end.
            RecordLiveBaseline(real);
            if (liveHandle == null) problems.Add("the module holds no live CotW publication");
            List<string> livePlacement = PlacementProblems(LiveCases(), null);
            steps["live"] = new JObject { ["handleUndoActions"] = liveUndo, ["placementProblems"] = Strings(livePlacement) };
            if (livePlacement.Count != 0) problems.Add("live CotW placement failed the checker: " + string.Join("; ", livePlacement.ToArray()));

            object isolated = BuildIsolatedSet(product);
            List<PlacementCase> cases = CasesOf(isolated);
            PopulateIsolatedEngines(real);
            Type engine = typeof(TurnUndeadIsolatedEngine);
            Snapshot original = Capture(cases, engine);

            // 1. Publication and rollback on the isolated surface.
            object first = publish.Invoke(null, new object[] { isolated, engine, "isolated" });
            Snapshot published = Capture(cases, engine);
            List<string> publishedChanges = Diff(original, published);
            List<string> placed = PlacementProblems(cases, null);
            rollback.Invoke(first, null);
            List<string> rollbackDiff = Diff(original, Capture(cases, engine));
            List<string> removed = PlacementProblems(cases, null);
            steps["isolatedPublication"] = new JObject
            {
                ["evidence"] = Strings((IEnumerable<string>)Member(first, "Evidence")), ["changed"] = Strings(publishedChanges),
                ["placementProblems"] = Strings(placed), ["rollbackDifferences"] = Strings(rollbackDiff), ["checkerAfterRemoval"] = Strings(removed)
            };
            if (placed.Count != 0) problems.Add("isolated publication failed the placement checker: " + string.Join("; ", placed.ToArray()));
            if (publishedChanges.Count == 0) problems.Add("the isolated publication changed nothing");
            if (rollbackDiff.Count != 0) problems.Add("isolated rollback left changes: " + string.Join(", ", rollbackDiff.ToArray()));
            if (cases.Count(c => removed.Any(r => r.StartsWith(c.Kind + ":"))) != cases.Count) problems.Add("the placement checker accepted removed menu children");
            isolatedRolledBack = true;
            yield return null;

            // 2. Drift after mutation, through the product's own reporting boundary.
            object outcome = reported.Invoke(null, new object[] { isolated, typeof(TurnUndeadDriftEngine), "isolated-drift-test" });
            List<string> driftDiff = Diff(original, Capture(cases, engine));
            string status = (string)Member(outcome, "StatusText"), warning = (string)Member(outcome, "Warning");
            int visible = (int)Member(outcome, "VisibleDirectActions");
            steps["driftReported"] = new JObject
            {
                ["publication"] = Member(outcome, "Publication") == null ? "none" : "present", ["status"] = status, ["warning"] = warning,
                ["failure"] = (string)Member(outcome, "Failure"), ["visibleDirectActions"] = visible,
                ["directHidden"] = new JArray(cases.Select(c => (object)c.Direct.Hidden).ToArray()), ["differencesFromOriginal"] = Strings(driftDiff)
            };
            if (Member(outcome, "Publication") != null || status == null || !status.StartsWith("cotw=failed"))
                problems.Add("the drift case did not report failed CotW availability: " + status);
            if (warning == null || warning.IndexOf("sacred_conduit", StringComparison.Ordinal) < 0 || warning.IndexOf("reduced compatibility", StringComparison.Ordinal) < 0)
                problems.Add("the reduced-compatibility warning does not name the missing sacred_conduit seam: " + warning);
            if (visible != cases.Count || cases.Any(c => c.Direct.Hidden)) problems.Add("the direct fallback actions are not all visible after the drift");
            if (driftDiff.Count != 0) problems.Add("drift rollback left changes: " + string.Join(", ", driftDiff.ToArray()));
            yield return null;

            // 3. Optional-null holder through the reporting boundary, then an already-populated second publication.
            object optionalOutcome = reported.Invoke(null, new object[] { isolated, typeof(TurnUndeadOptionalNullEngine), "isolated-optional-null-test" });
            object optional = Member(optionalOutcome, "Publication");
            if (optional == null) { problems.Add("the optional-null publication failed: " + Member(optionalOutcome, "Warning")); yield break; }
            Snapshot afterOptional = Capture(cases, engine);
            object second = publish.Invoke(null, new object[] { isolated, typeof(TurnUndeadOptionalNullEngine), "already-populated-test" });
            List<string> secondDiff = Diff(afterOptional, Capture(cases, engine));
            List<string> optionalPlacement = PlacementProblems(cases, null);
            rollback.Invoke(second, null);
            rollback.Invoke(optional, null);
            List<string> optionalDiff = Diff(original, Capture(cases, engine));
            steps["optionalNull"] = new JObject
            {
                ["status"] = (string)Member(optionalOutcome, "StatusText"), ["placementProblems"] = Strings(optionalPlacement),
                ["secondPublicationEvidence"] = Strings((IEnumerable<string>)Member(second, "Evidence")), ["secondPublicationChanged"] = Strings(secondDiff),
                ["rollbackDifferences"] = Strings(optionalDiff)
            };
            if (!((IEnumerable<string>)Member(optional, "Evidence")).Contains("consecrate_buff=absent")) problems.Add("the optional-null holder was not reported absent");
            if (optionalPlacement.Count != 0) problems.Add("optional-null publication placement failed: " + string.Join("; ", optionalPlacement.ToArray()));
            if (secondDiff.Count != 0) problems.Add("an already-populated publication changed state");
            if (optionalDiff.Count != 0) problems.Add("optional-null rollback left changes: " + string.Join(", ", optionalDiff.ToArray()));

            // 4. Re-publication equals the first; swapped parents fail the checker; rollback restores the original.
            object again = publish.Invoke(null, new object[] { isolated, engine, "isolated-again" });
            List<string> againDiff = Diff(published, Capture(cases, engine));
            PlacementCase cleric = cases.Single(c => c.Kind == "Cleric"), paladin = cases.Single(c => c.Kind == "Paladin");
            BlueprintAbility clericParent = cleric.Child.Parent, paladinParent = paladin.Child.Parent;
            cleric.Child.Parent = paladinParent; paladin.Child.Parent = clericParent;
            List<string> swapped = PlacementProblems(cases, null);
            cleric.Child.Parent = clericParent; paladin.Child.Parent = paladinParent;
            List<string> unswapped = PlacementProblems(cases, null);
            rollback.Invoke(again, null);
            List<string> finalDiff = Diff(original, Capture(cases, engine));
            steps["republication"] = new JObject
            {
                ["differencesFromFirst"] = Strings(againDiff), ["checkerSwapped"] = Strings(swapped), ["checkerUnswapped"] = Strings(unswapped),
                ["finalRollbackDifferences"] = Strings(finalDiff)
            };
            if (againDiff.Count != 0) problems.Add("re-publication differs from the first publication: " + string.Join(", ", againDiff.ToArray()));
            if (!swapped.Any(s => s.StartsWith("Cleric")) || !swapped.Any(s => s.StartsWith("Paladin"))) problems.Add("the placement checker accepted swapped parents");
            if (unswapped.Count != 0) problems.Add("placement after unswapping failed: " + string.Join("; ", unswapped.ToArray()));
            if (finalDiff.Count != 0) problems.Add("the final isolated rollback left changes: " + string.Join(", ", finalDiff.ToArray()));

            // 5. Normal completion: the live graph and the module's working rollback handle are untouched.
            List<string> live = LiveDifferences();
            steps["liveAfter"] = new JObject { ["handleUndoActions"] = UndoCount(module.GetProperty("CotwPublication", Any).GetValue(null, null)), ["differences"] = Strings(live) };
            problems.AddRange(live);
        }

        /// <summary>
        /// Interrupts the CotW boundary probe at its first yield (immediately after the isolated rollback) through a
        /// probe slot, and requires a restored verdict, no remaining fixture objects or substitute holders, and an
        /// unchanged live publication.
        /// </summary>
        private IEnumerable<object> CotwRollbackInterrupt()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null) { problems.Add("CotW is not loaded"); yield break; }
            RecordLiveBaseline(cotw.GetType("CallOfTheWild.ChannelEnergyEngine", true));
            var slot = new ProbeSlot();
            childSlots.Add(slot);
            var child = new TurnUndeadProbe("cotw-rollback");
            const string owner = "cotw-rollback (interrupted)";
            slot.Begin(owner, child);
            ProbeOutcome outcome = null;
            bool interrupted = false;
            int objectsAtInterrupt = 0, polls = 0;
            while (outcome == null && polls++ < 600)
            {
                if (child.isolatedRolledBack)
                {
                    objectsAtInterrupt = child.isolatedObjects.Count;
                    interrupted = true;
                    outcome = slot.Abandon("interrupted immediately after the isolated rollback");
                    break;
                }
                outcome = slot.Poll(owner);
                if (outcome == null) yield return null;
            }
            if (outcome == null) { outcome = slot.Abandon("did not reach the interruption point"); problems.Add("the CotW boundary probe never reached its rollback yield"); }
            bool holdersCleared = typeof(TurnUndeadIsolatedEngine).GetFields(BindingFlags.Static | BindingFlags.Public).All(f => f.GetValue(null) == null) &&
                typeof(TurnUndeadDriftEngine).GetFields(BindingFlags.Static | BindingFlags.Public).All(f => f.GetValue(null) == null);
            List<string> live = LiveDifferences();
            result["cotwRollbackInterrupt"] = new JObject
            {
                ["interrupted"] = interrupted, ["polls"] = polls, ["isolatedObjectsAtInterrupt"] = objectsAtInterrupt,
                ["isolatedObjectsRemaining"] = child.isolatedObjects.Count, ["substituteHoldersCleared"] = holdersCleared,
                ["isolationRestored"] = outcome.IsolationRestored, ["cleanupAttempted"] = Strings(outcome.Cleanup.Attempted),
                ["cleanupFailures"] = Strings(outcome.Cleanup.Failures), ["liveDifferences"] = Strings(live)
            };
            if (!interrupted) problems.Add("the CotW boundary probe finished before the interruption: " + outcome.Failure);
            if (!outcome.IsolationRestored) problems.Add("the interrupted CotW boundary probe reported unrestored isolation: " + string.Join("; ", outcome.Cleanup.Failures.ToArray()));
            if (child.isolatedObjects.Count != 0 || !holdersCleared) problems.Add("the interrupted probe left isolated fixture objects or substitute holders");
            problems.AddRange(live);
            RecordSlotOutcome(outcome, "interrupted");
        }

        // ---------------------------------------------------------------- nested cleanup through probe slots

        private sealed class DriverCase
        {
            internal bool Interrupt;
            internal string Inject;
        }

        /// <summary>The abandon-interrupt cases; null runs the standard three.</summary>
        private List<DriverCase> driverCases;
        /// <summary>Set while the driver waits for a closed child's deferred unit destruction (an interruption point).</summary>
        private bool awaitingDestruction;
        private bool stoppedOnCleanupFailure;
        private UnitEntityData[] lastSilenced = new UnitEntityData[0], lastSpawned = new UnitEntityData[0];

        /// <summary>
        /// Abandon probes run through the harness's own ProbeSlot: by default interrupted after the nested probe's
        /// first yield (clean, and with an injected nested cleanup failure) and run to normal completion with the
        /// injected failure. A closed child's outcome is merged into this driver's own cleanup record at once, before
        /// any yield, continue or later case. Only the driver's own injected marker is expected; any other cleanup
        /// failure is genuine: it is reported, reaches this driver's isolation verdict, and no further case starts.
        /// The real state (pause flag, brains, rule subscription, spawned units) must be restored in every case; a
        /// nested cleanup failure must reach the slot's outcome as unrestored isolation and refuse the next probe.
        /// </summary>
        private IEnumerable<object> AbandonInterrupt()
        {
            droppedChildMarker = InjectedMarker;
            var cases = new JArray();
            result["abandonInterrupt"] = cases;
            List<DriverCase> plans = driverCases ?? new List<DriverCase>
            {
                new DriverCase { Interrupt = true, Inject = null },
                new DriverCase { Interrupt = true, Inject = InjectedMarker },
                new DriverCase { Interrupt = false, Inject = InjectedMarker }
            };
            foreach (DriverCase plan in plans)
            {
                Game.Instance.IsPaused = false;
                var slot = new ProbeSlot();
                childSlots.Add(slot);
                var probe = new TurnUndeadProbe("abandon") { injectedNestedCleanupFailure = plan.Inject };
                string owner = "abandon-" + (plan.Interrupt ? "interrupted" : "completed") +
                    (plan.Inject == null ? "" : plan.Inject == InjectedMarker ? "-injected" : "-distinct");
                slot.Begin(owner, probe);
                ProbeOutcome outcome = null;
                TurnUndeadProbe nested = null;
                lastSilenced = new UnitEntityData[0];
                lastSpawned = new UnitEntityData[0];
                bool pausedAtInterrupt = false;
                int polls = 0;
                while (outcome == null && polls++ < 2400)
                {
                    TurnUndeadProbe current = probe.CurrentChild;
                    if (current != null)
                    {
                        nested = current;
                        if (current.silenced.Count > 0) lastSilenced = current.silenced.ToArray();
                        if (current.created.Count > 0) lastSpawned = current.created.ToArray();
                    }
                    if (plan.Interrupt && current != null && current.silenced.Count > 0)
                    {
                        pausedAtInterrupt = Game.Instance.IsPaused;
                        outcome = slot.Abandon("interrupted after the nested probe's first yield");
                        break;
                    }
                    outcome = slot.Poll(owner);
                    if (outcome == null) yield return null;
                }
                if (outcome == null) { outcome = slot.Abandon("timed out"); problems.Add(owner + ": the abandon probe did not finish"); }
                // The closed child's outcome belongs to this driver now, before any yield, continue or later case.
                RecordSlotOutcome(outcome, plan.Interrupt ? "interrupted" : "completed");
                string[] genuine = outcome.Cleanup.Failures.Where(f => f.IndexOf(InjectedMarker, StringComparison.Ordinal) < 0).ToArray();
                if (genuine.Length != 0)
                {
                    stoppedOnCleanupFailure = true;
                    problems.Add(owner + ": genuine cleanup failure; no further case runs: " + string.Join("; ", genuine));
                }
                // The window in which an earlier driver lost a closed child's outcome: it always lasts at least one
                // frame (and until deferred unit destruction completes), so an interruption can land in it.
                awaitingDestruction = true;
                int frames = 0;
                do { frames++; yield return null; }
                while (frames < 30 && lastSpawned.Any(u => u != null && !u.Destroyed));
                awaitingDestruction = false;
                string nextRefusal = null;
                try
                {
                    slot.Begin("after " + owner, new TurnUndeadProbe("contracts"));
                    slot.Terminate("after " + owner, "not run");
                }
                catch (InvalidOperationException e) { nextRefusal = e.Message; }
                var row = new JObject
                {
                    ["case"] = owner, ["polls"] = polls, ["pausedAtInterrupt"] = pausedAtInterrupt,
                    ["pauseAfterRelease"] = Game.Instance.IsPaused, ["silenced"] = lastSilenced.Length,
                    ["brainsRestored"] = lastSilenced.Count(u => u != null && !u.Destroyed && u.IsBrainActive),
                    ["nestedSubscriptionReleased"] = nested != null && nested.subscription == null,
                    ["spawned"] = lastSpawned.Length, ["spawnedRemaining"] = lastSpawned.Count(u => u != null && !u.Destroyed),
                    ["testFailure"] = outcome.TestFailure, ["isolationRestored"] = outcome.IsolationRestored,
                    ["cleanupFailures"] = Strings(outcome.Cleanup.Failures), ["nextProbeRefusal"] = nextRefusal
                };
                cases.Add(row);
                if (nested == null) problems.Add(owner + ": no nested probe was observed");
                else
                {
                    if (plan.Interrupt && !pausedAtInterrupt) problems.Add(owner + ": the nested probe had not paused the game at the interruption");
                    if (Game.Instance.IsPaused) problems.Add(owner + ": the prior pause flag (false) was not restored");
                    if (lastSilenced.Length == 0 || lastSilenced.Any(u => u != null && !u.Destroyed && !u.IsBrainActive)) problems.Add(owner + ": silenced brains were not observed restored");
                    if (nested.subscription != null) problems.Add(owner + ": the nested rule subscription was left open");
                    if (lastSpawned.Any(u => u != null && !u.Destroyed)) problems.Add(owner + ": spawned units left");
                    if (plan.Inject == null && (!outcome.IsolationRestored || nextRefusal != null)) problems.Add(owner + ": a clean release did not restore isolation: " + outcome.Failure);
                    if (plan.Inject != null)
                    {
                        if (outcome.IsolationRestored) problems.Add(owner + ": a failed nested cleanup produced a restored isolation verdict");
                        if (plan.Inject == InjectedMarker && !TurnUndeadObserverRules.OnlyInjected(outcome.Cleanup.Failures, InjectedMarker))
                            problems.Add(owner + ": cleanup failures other than the injected one: " + string.Join("; ", outcome.Cleanup.Failures.ToArray()));
                        if (nextRefusal == null || nextRefusal.IndexOf("isolation was not restored", StringComparison.Ordinal) < 0) problems.Add(owner + ": the next probe was not refused after unrestored isolation");
                        if (!plan.Interrupt && string.IsNullOrEmpty(outcome.TestFailure)) problems.Add(owner + ": the completed abandon check did not report the nested cleanup failure");
                    }
                }
                if (stoppedOnCleanupFailure) yield break;
            }
        }

        /// <summary>
        /// The driver's own recording, checked from outside through a further probe slot. Its abandon child carries a
        /// cleanup failure the driver does not expect (the distinct marker). (1) The driver is interrupted after that
        /// child's slot has closed, while it waits for deferred unit destruction: the outer outcome must carry the
        /// failure as unrestored isolation and refuse the next probe. (2) The driver continues instead, with a second
        /// case queued: it must stop before that case, with the same unrestored verdict. Real state must be restored.
        /// This check expects only the distinct marker, so it does not count it against itself.
        /// </summary>
        private IEnumerable<object> AbandonInterruptRecording()
        {
            droppedChildMarker = DistinctMarker;
            var cases = new JArray();
            result["abandonInterruptRecording"] = cases;
            foreach (bool interrupt in new[] { true, false })
            {
                Game.Instance.IsPaused = false;
                var slot = new ProbeSlot();
                childSlots.Add(slot);
                var driver = new TurnUndeadProbe("abandon-interrupt")
                {
                    driverCases = interrupt
                        ? new List<DriverCase> { new DriverCase { Interrupt = false, Inject = DistinctMarker } }
                        : new List<DriverCase> { new DriverCase { Interrupt = false, Inject = DistinctMarker }, new DriverCase { Interrupt = true, Inject = null } }
                };
                string owner = interrupt ? "driver-interrupted-during-deferred-destruction" : "driver-continued-after-failure";
                slot.Begin(owner, driver);
                ProbeOutcome outcome = null;
                bool interrupted = false;
                int polls = 0;
                while (outcome == null && polls++ < 2400)
                {
                    if (interrupt && driver.awaitingDestruction)
                    {
                        interrupted = true;
                        outcome = slot.Abandon("interrupted after the driver's child slot closed, while destruction is deferred");
                        break;
                    }
                    outcome = slot.Poll(owner);
                    if (outcome == null) yield return null;
                }
                if (outcome == null) { outcome = slot.Abandon("timed out"); problems.Add(owner + ": the driver did not finish"); }
                RecordSlotOutcome(outcome, owner);
                UnitEntityData[] silencedUnits = driver.lastSilenced, spawned = driver.lastSpawned;
                int frames = 0;
                while (frames < 30 && spawned.Any(u => u != null && !u.Destroyed)) { frames++; yield return null; }
                string nextRefusal = null;
                try
                {
                    slot.Begin("after " + owner, new TurnUndeadProbe("contracts"));
                    slot.Terminate("after " + owner, "not run");
                }
                catch (InvalidOperationException e) { nextRefusal = e.Message; }
                var driverRows = (JArray)((JObject)driver.Result)["abandonInterrupt"];
                var row = new JObject
                {
                    ["case"] = owner, ["interruptedDuringDeferredDestruction"] = interrupted, ["polls"] = polls,
                    ["driverCasesStarted"] = driverRows == null ? 0 : driverRows.Count + (interrupted ? 1 : 0),
                    ["driverStoppedOnFailure"] = driver.stoppedOnCleanupFailure,
                    ["testFailure"] = outcome.TestFailure, ["isolationRestored"] = outcome.IsolationRestored,
                    ["cleanupFailures"] = Strings(outcome.Cleanup.Failures), ["nextProbeRefusal"] = nextRefusal,
                    ["pauseAfterRelease"] = Game.Instance.IsPaused, ["silenced"] = silencedUnits.Length,
                    ["brainsRestored"] = silencedUnits.Count(u => u != null && !u.Destroyed && u.IsBrainActive),
                    ["spawnedRemaining"] = spawned.Count(u => u != null && !u.Destroyed)
                };
                cases.Add(row);
                if (interrupt && !interrupted) problems.Add(owner + ": the driver was never caught waiting for deferred destruction");
                if (outcome.IsolationRestored) problems.Add(owner + ": a genuine child cleanup failure produced a restored driver verdict");
                if (!outcome.Cleanup.Failures.Any(f => f.IndexOf(DistinctMarker, StringComparison.Ordinal) >= 0)) problems.Add(owner + ": the distinct cleanup failure did not reach the driver's outcome");
                if (!TurnUndeadObserverRules.OnlyInjected(outcome.Cleanup.Failures, DistinctMarker)) problems.Add(owner + ": unexpected cleanup failures: " + string.Join("; ", outcome.Cleanup.Failures.ToArray()));
                if (nextRefusal == null || nextRefusal.IndexOf("isolation was not restored", StringComparison.Ordinal) < 0) problems.Add(owner + ": the next probe was not refused");
                if (!interrupt && (!driver.stoppedOnCleanupFailure || driverRows == null || driverRows.Count != 1)) problems.Add(owner + ": the driver did not stop before its second case");
                if (!interrupt && (outcome.TestFailure == null || outcome.TestFailure.IndexOf("no further case runs", StringComparison.Ordinal) < 0)) problems.Add(owner + ": the driver's verdict does not report the genuine cleanup failure");
                if (Game.Instance.IsPaused) problems.Add(owner + ": the prior pause flag (false) was not restored");
                if (silencedUnits.Length == 0 || silencedUnits.Any(u => u != null && !u.Destroyed && !u.IsBrainActive)) problems.Add(owner + ": silenced brains were not observed restored");
                if (spawned.Any(u => u != null && !u.Destroyed)) problems.Add(owner + ": spawned units left");
            }
        }

        // ---------------------------------------------------------------- CotW registration failure boundary (isolated donors)

        /// <summary>
        /// Drives the product's own DC-calculator plan and paladin extra-route builder (the code registration uses)
        /// with isolated donors: healthy copies keep the CotW channel progression; a donor with an unsupported
        /// calculator shape (two calculators) makes a core source fall back to its class level with a report, and
        /// makes the optional extra route unavailable with a report, without an exception; a failing registration
        /// delegate is contained the same way. The live registration is then checked intact.
        /// </summary>
        private IEnumerable<object> CotwRegistrationBoundary()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            Type blueprints = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadBlueprints", true);
            Type sourceType = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadSourceBlueprints", true);
            MethodInfo plan = blueprints.GetMethod("PlanDcCalculator", Any);
            MethodInfo build = blueprints.GetMethod("BuildPaladinExtra", Any);
            Type module = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            if (plan == null || build == null || !CotwLoaded) { problems.Add("a product registration seam or CotW is unavailable"); yield break; }
            var steps = new JObject();
            result["registrationBoundary"] = steps;
            Func<BlueprintAbility, string, bool, BlueprintAbility> isolatedDonor = (source, name, drift) =>
            {
                BlueprintAbility copy = UnityEngine.Object.Instantiate(source);
                copy.name = "KmgTurnUndeadIsolated_" + name;
                isolatedObjects.Add(copy);
                if (drift)
                {
                    var second = (ContextCalculateAbilityParamsBasedOnClass)FreshComponent(typeof(ContextCalculateAbilityParamsBasedOnClass), name + "_SecondDc");
                    copy.ComponentsArray = copy.ComponentsArray.Concat(new BlueprintComponent[] { second }).ToArray();
                }
                return copy;
            };
            Func<object, JObject> describePlan = value =>
            {
                string status = (string)Member(value, "Status");
                var create = (Func<BlueprintComponent>)Member(value, "Create");
                BlueprintComponent component = create();
                isolatedObjects.Add(component);
                StatType stat;
                bool fixedStat;
                string[] classes = CalculatorShape(component, out stat, out fixedStat);
                return new JObject { ["status"] = status, ["calculator"] = component.GetType().Name, ["classes"] = new JArray(classes ?? new string[0]), ["stat"] = stat.ToString() };
            };

            // ---- core source calculator plan (cleric), healthy and drifted
            object clericContract = Member(SourceOf("Cleric"), "Contract");
            var clericHarm = Blueprint<BlueprintAbility>(ExpectedSources[0].NativeHarm);
            var clericClass = Blueprint<BlueprintCharacterClass>(ClericClass);
            JObject healthyCore = describePlan(plan.Invoke(null, new object[] { clericContract, "IsolatedCleric", isolatedDonor(clericHarm, "HealthyCleric", false), clericClass, true }));
            JObject driftCore = describePlan(plan.Invoke(null, new object[] { clericContract, "IsolatedCleric", isolatedDonor(clericHarm, "DriftCleric", true), clericClass, true }));
            string strictCore = null;
            try { plan.Invoke(null, new object[] { clericContract, "IsolatedCleric", isolatedDonor(clericHarm, "StrictCleric", true), clericClass, false }); }
            catch (TargetInvocationException e) { strictCore = e.InnerException == null ? e.Message : e.InnerException.Message; }
            steps["coreCalculator"] = new JObject { ["healthy"] = healthyCore, ["drift"] = driftCore, ["strictModeRefusal"] = strictCore };
            string[] cotwClasses = new[] { ClericClass }.Concat(CotwProgressionClasses).ToArray();
            if ((string)healthyCore["status"] != "progression" || !healthyCore["classes"].Select(c => (string)c).SequenceEqual(cotwClasses))
                problems.Add("a healthy cleric donor did not keep its CotW channel progression: " + healthyCore.ToString(Newtonsoft.Json.Formatting.None));
            if (!((string)driftCore["status"]).StartsWith("fallback") || (string)driftCore["calculator"] != "ContextCalculateAbilityParamsBasedOnClass" ||
                !driftCore["classes"].Select(c => (string)c).SequenceEqual(new[] { ClericClass }) || (string)driftCore["stat"] != "Charisma")
                problems.Add("a drifted cleric donor did not fall back to the cleric level with a report: " + driftCore.ToString(Newtonsoft.Json.Formatting.None));
            if (strictCore == null || strictCore.IndexOf("2 DC calculators", StringComparison.Ordinal) < 0) problems.Add("the strict plan did not refuse the drifted donor: " + strictCore);
            yield return null;

            // ---- optional paladin extra route, healthy, drifted, and with a failing registration
            object paladinSource = SourceOf("Paladin");
            object paladinContract = Member(paladinSource, "Contract");
            var paladinFeature = (BlueprintFeature)Member(paladinSource, "SourceFeature");
            var extraHarm = Blueprint<BlueprintAbility>(CotwPaladinExtraHarm);
            var extraPool = Blueprint<BlueprintAbilityResource>(CotwPaladinExtraResource);
            var paladinClass = Blueprint<BlueprintCharacterClass>(ExpectedSources[1].ClassGuid);
            var undead = Blueprint<BlueprintFeature>(UndeadType);
            Sprite icon = Direct("Paladin").Icon;
            const BindingFlags ctor = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Func<object> isolatedPaladin = () => Activator.CreateInstance(sourceType, ctor, null, new object[]
                { paladinContract, paladinFeature, Fresh<BlueprintAbility>("Paladin_Harm"), Fresh<BlueprintAbility>("Paladin_Direct"), Fresh<BlueprintAbility>("Paladin_Child") }, null);
            var built = new List<BlueprintAbility>();
            Func<Func<BlueprintAbility>, BlueprintAbility> register = factory =>
            {
                BlueprintAbility ability = factory();
                built.Add(ability);
                isolatedObjects.Add(ability);
                return ability;
            };
            Func<Func<BlueprintAbility>, BlueprintAbility> failingRegister = factory => { throw new InvalidOperationException("isolated registration failure"); };
            var routes = new JObject();
            foreach (string kind in new[] { "healthy", "drift", "failingRegistration" })
            {
                object paladin = isolatedPaladin();
                BlueprintAbility donor = isolatedDonor(extraHarm, kind == "drift" ? "DriftExtra" : kind == "healthy" ? "HealthyExtra" : "FailingExtra", kind == "drift");
                int before = built.Count;
                string status = null, thrown = null;
                try
                {
                    status = (string)build.Invoke(null, new object[] { paladinContract, paladin, donor, extraPool, paladinClass, Feat, undead, Fleeing, icon,
                        kind == "failingRegistration" ? failingRegister : register });
                }
                catch (TargetInvocationException e) { thrown = (e.InnerException ?? e).GetType().Name + ": " + (e.InnerException ?? e).Message; }
                var child = (BlueprintAbility)Member(paladin, "ExtraMenuChild");
                var row = new JObject { ["status"] = status, ["thrown"] = thrown, ["childBuilt"] = built.Count - before, ["attached"] = child != null };
                if (child != null)
                {
                    BlueprintComponent calculator = child.ComponentsArray.FirstOrDefault(c => c is ContextAbilityParamsCalculator);
                    StatType stat;
                    bool fixedStat;
                    row["classes"] = new JArray(CalculatorShape(calculator, out stat, out fixedStat) ?? new string[0]);
                    AbilityResourceLogic logic = child.GetComponents<AbilityResourceLogic>().FirstOrDefault();
                    row["resource"] = logic == null || logic.RequiredResource == null ? null : logic.RequiredResource.AssetGuid + " x" + logic.Amount;
                }
                routes[kind] = row;
                if (thrown != null) problems.Add("the extra-route builder threw (" + kind + "): " + thrown);
                else if (kind == "healthy" && (status != "present" || child == null || !row["classes"].Select(c => (string)c).SequenceEqual(new[] { ExpectedSources[1].ClassGuid }.Concat(CotwProgressionClasses)) ||
                    (string)row["resource"] != CotwPaladinExtraResource + " x1"))
                    problems.Add("a healthy extra donor did not build the progression child: " + row.ToString(Newtonsoft.Json.Formatting.None));
                else if (kind == "drift" && (status == null || !status.StartsWith("drift") || status.IndexOf("2 DC calculators", StringComparison.Ordinal) < 0 || child != null || built.Count != before))
                    problems.Add("a drifted extra donor was not reported unavailable without construction: " + row.ToString(Newtonsoft.Json.Formatting.None));
                else if (kind == "failingRegistration" && (status == null || !status.StartsWith("drift") || status.IndexOf("isolated registration failure", StringComparison.Ordinal) < 0 || child != null))
                    problems.Add("a failing extra-route registration was not contained: " + row.ToString(Newtonsoft.Json.Formatting.None));
            }
            steps["extraRoute"] = routes;
            yield return null;

            // ---- the live registration stays intact
            object liveSet = module.GetProperty("Blueprints", Any).GetValue(null, null);
            var identities = new JObject
            {
                ["moduleStatus"] = (string)module.GetProperty("Status", Any).GetValue(null, null),
                ["feat"] = Feat.AssetGuid, ["fleeing"] = Fleeing.AssetGuid,
                ["dcSummary"] = (string)Member(liveSet, "DcSummary"), ["extraRouteStatus"] = (string)Member(liveSet, "ExtraRouteStatus")
            };
            var actions = new JArray();
            foreach (SourceExpectation e in ExpectedSources)
                foreach (string role in new[] { "Direct", "MenuChild" })
                {
                    var ability = (BlueprintAbility)Member(SourceOf(e.Kind), role);
                    actions.Add(e.Kind + "." + role + "=" + ability.AssetGuid + (ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(ability.AssetGuid), ability) ? "" : " (not resolvable)"));
                }
            identities["sourceActions"] = actions;
            steps["liveRegistration"] = identities;
            if (Feat.AssetGuid != "fd2d5083cb294a9396c390e8d39f4efd" || !ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(Feat.AssetGuid), Feat))
                problems.Add("the core feat is not resolvable at its stable GUID");
            if (!ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(Fleeing.AssetGuid), Fleeing)) problems.Add("the flight buff is not resolvable");
            if (actions.Any(a => ((string)a).EndsWith("(not resolvable)"))) problems.Add("a native source action is not resolvable: " + actions.ToString(Newtonsoft.Json.Formatting.None));
            if ((string)identities["dcSummary"] != "cleric:progression,paladin:progression,hospitaler:progression,empyreal:progression") problems.Add("the live DC summary is " + identities["dcSummary"]);
        }

        // ---------------------------------------------------------------- CotW gameplay: progression DC and paladin payment

        private sealed class ProbeCastResult
        {
            internal bool Available;
            internal string Problem;
            internal UnitUseAbility Command;
        }

        private UnitEntityData CotwCaster(string label, Vector3 at)
        {
            UnitEntityData unit = SpawnFriendly(label, at);
            unit.Descriptor.Stats.Charisma.BaseValue = 16;
            return unit;
        }

        /// <summary>A menu child cast the way the variant menu does: converted from the caster's wrapper ability.</summary>
        private ProbeCastResult CastVariant(UnitEntityData caster, BlueprintAbility child, out int seq)
        {
            Ability wrapperFact = child.Parent == null ? null : caster.Descriptor.Abilities.GetAbility(child.Parent);
            seq = Seq;
            if (wrapperFact == null) return new ProbeCastResult { Problem = "the caster has no " + (child.Parent == null ? "parent wrapper" : child.Parent.name) + " for " + child.name };
            var data = new AbilityData(new AbilityData(wrapperFact), child);
            var cast = new ProbeCastResult { Available = data.IsAvailableForCast };
            if (!cast.Available) return cast;
            var command = new UnitUseAbility(data, new TargetWrapper(caster));
            command.IgnoreCooldown(null);
            caster.Commands.InterruptAll(true);
            caster.Commands.Run(command);
            cast.Command = command;
            return cast;
        }

        private static bool VariantAvailable(UnitEntityData unit, BlueprintAbility child)
        {
            Ability wrapperFact = child.Parent == null ? null : unit.Descriptor.Abilities.GetAbility(child.Parent);
            return wrapperFact != null && new AbilityData(new AbilityData(wrapperFact), child).IsAvailableForCast;
        }

        /// <summary>Every tracked unit that rolled a save against this cast's source ability since <paramref name="seq"/>.</summary>
        private JArray Recipients(int seq) => new JArray(trace.Events
            .Where(e => (int)e["seq"] > seq && (string)e["what"] == "save" && (string)e["sourceAbility"] == turnSource)
            .Select(e => (object)new JObject { ["unit"] = e["unit"], ["dc"] = e["dc"], ["passed"] = e["passed"], ["natural"] = e["natural"] }).ToArray());

        private static IEnumerable<object> WaitFor(ProbeCastResult cast)
        {
            if (cast.Command == null) yield break;
            foreach (object step in Settle(0.0, 30f, () => cast.Command.IsFinished && (cast.Command.ExecutionProcess == null || cast.Command.ExecutionProcess.IsEnded))) yield return step;
        }

        /// <summary>
        /// CotW profile gameplay through the Turn menu children: (1) the DC follows the channel's CotW progression,
        /// counting Holy Vindicator levels but not fighter levels; (2) the paladin's two payment routes each spend only
        /// their own pool at their own cost, and the extra-channel route is unavailable without extra-channel uses.
        /// Disclosed fixture: Holy Vindicator levels added directly (prerequisites are not the subject), alignment set
        /// lawful good, Extra Channel (Paladin) added as a fact, pools restored to their maximum, lowered Will totals.
        /// </summary>
        private IEnumerable<object> CotwGameplay()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null) { problems.Add("CotW is not loaded"); yield break; }
            SilenceArea();
            Game.Instance.IsPaused = true;
            var clericClass = Blueprint<BlueprintCharacterClass>(ClericClass);
            var holyVindicator = Blueprint<BlueprintCharacterClass>(CotwProgressionClasses[0]);
            var channel = Blueprint<BlueprintFeature>(ChannelPositive);

            // ---- DC progression
            BlueprintAbility clericChild = (BlueprintAbility)Member(SourceOf("Cleric"), "MenuChild");
            turnSource = clericChild.AssetGuid;
            var progression = new JArray();
            result["dcProgression"] = progression;
            foreach (bool vindicator in new[] { true, false })
            {
                float side = vindicator ? 6f : -6f;
                UnitEntityData caster = CotwCaster(vindicator ? "VindicatorCleric" : "FighterCleric", Spot(0f, side));
                Level(caster, clericClass, 5);
                if (!caster.Descriptor.HasFact(channel)) caster.Descriptor.AddFact(channel);
                if (vindicator)
                {
                    caster.Descriptor.Progression.AddClassLevel(holyVindicator);
                    caster.Descriptor.Progression.AddClassLevel(holyVindicator);
                }
                else Level(caster, Blueprint<BlueprintCharacterClass>(FighterClass), 2);
                caster.Descriptor.AddFact(Feat);
                UnitEntityData skeleton = SpawnNative(Skeleton, "skeleton", Spot(2.5f, side));
                skeleton.Stats.SaveWill.BaseValue = -100;
                yield return null;
                int clericLevel = caster.Descriptor.Progression.GetClassLevel(clericClass);
                int vindicatorLevel = caster.Descriptor.Progression.GetClassLevel(holyVindicator);
                int expectedDc = 10 + (clericLevel + vindicatorLevel) / 2 + caster.Stats.Charisma.Bonus;
                int seq;
                ProbeCastResult cast = CastVariant(caster, clericChild, out seq);
                foreach (object step in WaitFor(cast)) yield return step;
                string label = vindicator ? "cleric 5 / Holy Vindicator 2" : "cleric 5 / fighter 2";
                var row = new JObject
                {
                    ["build"] = label, ["clericLevel"] = clericLevel, ["holyVindicatorLevel"] = vindicatorLevel,
                    ["characterLevel"] = caster.Descriptor.Progression.CharacterLevel, ["charismaBonus"] = caster.Stats.Charisma.Bonus,
                    ["expectedDc"] = expectedDc, ["available"] = cast.Available, ["castProblem"] = cast.Problem, ["saves"] = Saves(skeleton, seq),
                    ["recipients"] = Recipients(seq)
                };
                progression.Add(row);
                if (vindicator && vindicatorLevel != 2) problems.Add("fixture: the Holy Vindicator levels were not added");
                if (cast.Problem != null || !cast.Available) { problems.Add(label + ": the cleric Turn menu child could not be cast: " + (cast.Problem ?? "unavailable")); continue; }
                row["outcome"] = Judge(label + ": skeleton", skeleton, true, seq, expectedDc);
            }

            // ---- Paladin payment routes
            object paladinSource = SourceOf("Paladin");
            var paladinChild = (BlueprintAbility)Member(paladinSource, "MenuChild");
            var extraChild = (BlueprintAbility)Member(paladinSource, "ExtraMenuChild");
            if (extraChild == null) { problems.Add("the CotW paladin extra-channel child is not registered"); yield break; }
            FieldInfo extraFeatField = SafeTypes(cotw).Select(t => t.GetField("paladin_channel_extra", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).FirstOrDefault(f => f != null);
            var extraFeat = extraFeatField == null ? null : extraFeatField.GetValue(null) as BlueprintFeature;
            if (extraFeat == null) { problems.Add("CotW's Extra Channel (Paladin) feat was not found"); yield break; }
            var layOnHands = Blueprint<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            var extraPool = Blueprint<BlueprintAbilityResource>(CotwPaladinExtraResource);
            var paladinClass = Blueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var paladinChannel = Blueprint<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");
            var payment = new JObject();
            result["paladinPayment"] = payment;
            UnitEntityData withExtra = null, withoutExtra = null;
            foreach (bool extra in new[] { true, false })
            {
                UnitEntityData paladin = CotwCaster(extra ? "ExtraPaladin" : "PlainPaladin", Spot(-4f, extra ? 3f : -3f));
                paladin.Descriptor.Alignment.Set(Alignment.LawfulGood);
                Level(paladin, paladinClass, 4);
                if (!paladin.Descriptor.HasFact(paladinChannel)) paladin.Descriptor.AddFact(paladinChannel);
                paladin.Descriptor.AddFact(Feat);
                if (extra) paladin.Descriptor.AddFact(extraFeat);
                yield return null;
                paladin.Descriptor.Resources.Restore(layOnHands);
                if (paladin.Descriptor.Resources.ContainsResource(extraPool)) paladin.Descriptor.Resources.Restore(extraPool);
                if (extra) withExtra = paladin; else withoutExtra = paladin;
            }
            payment["setup"] = new JObject
            {
                ["paladinLevel"] = withExtra.Descriptor.Progression.GetClassLevel(paladinClass), ["alignment"] = withExtra.Descriptor.Alignment.Value.ToString(),
                ["layOnHands"] = withExtra.Descriptor.Resources.GetResourceAmount(layOnHands), ["extraPool"] = withExtra.Descriptor.Resources.GetResourceAmount(extraPool),
                ["wrapper"] = paladinChild.Parent == null ? null : paladinChild.Parent.AssetGuid + ":" + paladinChild.Parent.name,
                ["extraChildParent"] = extraChild.Parent == null ? null : extraChild.Parent.AssetGuid,
                ["withoutFeat"] = new JObject
                {
                    ["extraPool"] = withoutExtra.Descriptor.Resources.GetResourceAmount(extraPool), ["layOnHands"] = withoutExtra.Descriptor.Resources.GetResourceAmount(layOnHands),
                    ["extraChildAvailable"] = VariantAvailable(withoutExtra, extraChild), ["layOnHandsChildAvailable"] = VariantAvailable(withoutExtra, paladinChild)
                }
            };
            if (extraChild.Parent == null || extraChild.Parent != paladinChild.Parent) problems.Add("the extra-channel child is not under the paladin channel wrapper");
            if (withExtra.Descriptor.Resources.GetResourceAmount(extraPool) < 1) problems.Add("fixture: the Extra Channel (Paladin) feat gave no extra-channel uses");
            if (VariantAvailable(withoutExtra, extraChild)) problems.Add("the extra-channel Turn child is castable without extra-channel uses");
            if (!VariantAvailable(withoutExtra, paladinChild)) problems.Add("the lay on hands Turn child is not castable for a paladin with lay on hands uses");
            int dc = 10 + withExtra.Descriptor.Progression.GetClassLevel(paladinClass) / 2 + withExtra.Stats.Charisma.Bonus;
            foreach (BlueprintAbility child in new[] { extraChild, paladinChild })
            {
                bool viaExtra = child == extraChild;
                turnSource = child.AssetGuid;
                UnitEntityData skeleton = SpawnNative(Skeleton, "skeleton", Spot(-1.5f, viaExtra ? 4.5f : 1.5f));
                skeleton.Stats.SaveWill.BaseValue = -100;
                yield return null;
                int layBefore = withExtra.Descriptor.Resources.GetResourceAmount(layOnHands), extraBefore = withExtra.Descriptor.Resources.GetResourceAmount(extraPool);
                int seq;
                ProbeCastResult cast = CastVariant(withExtra, child, out seq);
                foreach (object step in WaitFor(cast)) yield return step;
                int layAfter = withExtra.Descriptor.Resources.GetResourceAmount(layOnHands), extraAfter = withExtra.Descriptor.Resources.GetResourceAmount(extraPool);
                string label = viaExtra ? "extra-channel route" : "lay on hands route";
                var row = new JObject
                {
                    ["available"] = cast.Available, ["castProblem"] = cast.Problem, ["layOnHands"] = new JArray(layBefore, layAfter),
                    ["extraPool"] = new JArray(extraBefore, extraAfter), ["expectedDc"] = dc, ["saves"] = Saves(skeleton, seq),
                    ["recipients"] = Recipients(seq), ["effectCheckedOn"] = skeleton.UniqueId
                };
                payment[viaExtra ? "extraRoute" : "layOnHandsRoute"] = row;
                if (cast.Problem != null || !cast.Available) { problems.Add(label + ": not castable: " + (cast.Problem ?? "unavailable")); continue; }
                if (viaExtra && (extraAfter != extraBefore - 1 || layAfter != layBefore)) problems.Add(label + " spent lay on hands " + layBefore + "->" + layAfter + " and extra " + extraBefore + "->" + extraAfter + ", expected one extra use only");
                if (!viaExtra && (layAfter != layBefore - 2 || extraAfter != extraBefore)) problems.Add(label + " spent lay on hands " + layBefore + "->" + layAfter + " and extra " + extraBefore + "->" + extraAfter + ", expected two lay on hands uses only");
                row["outcome"] = Judge(label + ": skeleton", skeleton, true, seq, dc);
            }
        }

        private static IEnumerable<Type> SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }
    }
}
