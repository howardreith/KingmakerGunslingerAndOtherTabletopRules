using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 D2 (LIM-02), Bebelith marker: a DIAGNOSTIC CONTROL, not a pre-fix cold load. No admitted save from a
    /// pre-v0.0.182 build carries the retired KMG.Summoning.Special.Bebelith.DismantledArmor marker. The marker component
    /// has no persisted fields in v0.0.181 or now (same type), so a marker serialized by the current build is
    /// shape-equivalent to a historical one (source inference). "mark": a real Bebelith is summoned, an armored disposable
    /// character receives the marker exactly as v0.0.181 applied it (AddBuff with the Bebelith's combat-traits context,
    /// null duration), real native RuleCalculateAC runs before and after, the Bebelith then expires natively, and the
    /// game saves. "reload": a fresh process loads that save. Predeclared: AC with the marker equals AC without it; the
    /// marker component runs and adds no modifier; armor and its AC contribution are unchanged; the marker remains present
    /// exactly once (it has no duration) and deserializes to its component.
    /// </summary>
    internal sealed class LegacyMarkerProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal const string MarkerGuid = "736a349933f24cc2b11bc284b6e559cb";
        internal const string TraitsGuid = "d152ef1f0a2e42bea3e7a86228a67f2f";
        internal const string MarkedName = "KMG_D2_MarkedCharacter";
        private const string ArmorName = "ScalemailStandard";

        private static int componentCalls;
        private static int markerSourcedDuringRule;
        private static readonly HarmonyInstance Harmony = HarmonyInstance.Create("KmgLoaderCompatHarness.d2-marker");
        private static bool patched;

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private string verdict;
        private Assembly product;
        private Type runner;

        public LegacyMarkerProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure) => ProbeCleanup.RunAll(new[]
        {
            new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); })
        });

        // Runs right after the marker component's AboutToTrigger, while any modifier it added would still be on the stat
        // (temporary rule modifiers are removed when the rule ends), so this is where a contribution would be visible.
        private static void ComponentPostfix(RuleCalculateAC evt)
        {
            componentCalls++;
            if (evt == null || evt.Target == null) return;
            markerSourcedDuringRule += evt.Target.Descriptor.Stats.AC.Modifiers.Count(m => m != null && m.Source != null &&
                m.Source.Blueprint != null && m.Source.Blueprint.AssetGuid == MarkerGuid);
        }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            Type component = product.GetType("KingmakerGunslinger.Summoning.BebelithDismantledArmorComponent", true);
            if (!patched)
            {
                Harmony.Patch(component.GetMethod("OnEventAboutToTrigger", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly), null,
                    new HarmonyMethod(typeof(LegacyMarkerProbe).GetMethod(nameof(ComponentPostfix), BindingFlags.Static | BindingFlags.NonPublic)));
                patched = true;
            }
            result["scenario"] = "diagnostic control: constructed current-build marker (shape-equivalent by source), not a pre-fix cold load";
            yield return null;
            IEnumerable<object> body = mode == "mark" ? Mark() : mode == "reload" ? Reload() : null;
            if (body == null) { problems.Add("unknown mode " + mode); yield break; }
            foreach (object step in body) yield return step;
        }

        private BlueprintBuff Marker => ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(MarkerGuid);

        private Buff[] Markers(UnitEntityData unit) => unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => ReferenceEquals(b.Blueprint, Marker)).ToArray();

        /// <summary>One real native AC calculation against the character, with the marker component's callback counted.</summary>
        private JObject Ac(UnitEntityData attacker, UnitEntityData target, string label)
        {
            int before = componentCalls, sourcedBefore = markerSourcedDuringRule;
            var rule = Rulebook.Trigger(new RuleCalculateAC(attacker, target, Kingmaker.RuleSystem.AttackType.Melee));
            var mods = target.Descriptor.Stats.AC.Modifiers.ToArray();
            var row = new JObject
            {
                ["label"] = label, ["ruleTargetAC"] = rule.TargetAC, ["statAC"] = target.Descriptor.Stats.AC.ModifiedValue,
                ["armorModifiers"] = mods.Where(m => m.ModDescriptor == ModifierDescriptor.Armor).Sum(m => m.ModValue),
                ["markerModifiers"] = markerSourcedDuringRule - sourcedBefore,
                ["componentCalls"] = componentCalls - before,
                ["armor"] = target.Body.Armor.HasArmor ? target.Body.Armor.Armor.Blueprint.name : null,
                ["markers"] = Markers(target).Length
            };
            return row;
        }

        private IEnumerable<object> Mark()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward * 4f);
            // An armored disposable character (the marker's owner; no living Bebelith is needed later).
            UnitEntityData target = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            created.Add(target);
            Game.Instance.EntityCreator.Tick();
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { target, false });
            BlueprintItemArmor armorBp = ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintItemArmor>().FirstOrDefault(b => b.name == ArmorName);
            if (!target.Body.Armor.HasArmor && armorBp != null) target.Body.Armor.InsertItem(armorBp.CreateEntity());
            yield return null;
            // A real Bebelith through the product's execution ability; its combat-traits context is the historical parent context.
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { "bebelith", Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), main, variant, null, evidence });
            UnitEntityData bebelith = units.Length == 1 ? units[0] : null;
            if (bebelith == null) { problems.Add("mark: the Bebelith was not summoned"); yield break; }
            created.Add(bebelith);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { bebelith, false });
            Buff traits = bebelith.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => b.Blueprint.AssetGuid == TraitsGuid);
            yield return null;
            var rows = new JArray();
            result["ac"] = rows;
            JObject without = Ac(main, target, "withoutMarker");
            rows.Add(without);
            if (Marker == null || traits == null) { problems.Add("mark: marker blueprint or Bebelith traits missing"); yield break; }
            Buff applied = target.Descriptor.Buffs.AddBuff(Marker, traits.MaybeContext, null); // as v0.0.181: parent context, null duration
            result["applied"] = new JObject { ["buff"] = applied != null, ["caster"] = applied?.Context?.MaybeCaster?.UniqueId, ["bebelith"] = bebelith.UniqueId,
                ["endTime"] = applied == null ? null : applied.EndTime.ToString(), ["components"] = applied == null ? null : new JArray(applied.Components.Select(c => (object)c.GetType().Name).ToArray()) };
            yield return null;
            JObject with = Ac(main, target, "withMarker");
            rows.Add(with);
            // The Bebelith then expires natively (deadline moved to now, disclosed); the marker stays.
            Buff marker = bebelith.Descriptor.Buffs.Enumerable.FirstOrDefault(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            if (marker != null)
            {
                typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
                bebelith.Descriptor.Buffs.UpdateNextEvent();
                bebelith.Descriptor.Buffs.Tick();
            }
            float until = Time.realtimeSinceStartup + 15f;
            while (!bebelith.Destroyed && Time.realtimeSinceStartup < until) { Game.Instance.IsPaused = false; yield return null; }
            Game.Instance.IsPaused = true;
            result["bebelithDestroyed"] = bebelith.Destroyed;
            JObject afterExpiry = Ac(main, target, "withMarkerBebelithGone");
            rows.Add(afterExpiry);
            Expect(without, with, afterExpiry);
            if (!bebelith.Destroyed) problems.Add("mark: the Bebelith did not expire");
            target.Descriptor.CustomName = MarkedName;
            // The marked character stays owned by cleanup on every path (refusal, exception, success); cleanup runs
            // after the save has been written, so the saved state keeps it.
            result["saved"] = new JObject { ["id"] = target.UniqueId, ["ruleTargetAC"] = without["ruleTargetAC"] };
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            if (!allowed) { problems.Add("mark: native save admission refused"); yield break; }
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave("d2mark"), () => saved = true);
            float deadline = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < deadline) yield return null;
            result["saveFinished"] = saved && !Game.Instance.SaveManager.CommitInProgress;
            if (!(bool)result["saveFinished"]) problems.Add("mark: the save did not finish");
        }

        private void Expect(params JObject[] rows)
        {
            JObject baseline = rows[0];
            foreach (JObject row in rows.Skip(1))
            {
                string at = (string)row["label"];
                if ((int)row["ruleTargetAC"] != (int)baseline["ruleTargetAC"] || (int)row["statAC"] != (int)baseline["statAC"])
                    problems.Add(at + ": AC changed with the marker present: " + row.ToString(Formatting.None) + " vs " + baseline.ToString(Formatting.None));
                if ((int)row["armorModifiers"] != (int)baseline["armorModifiers"] || (string)row["armor"] != (string)baseline["armor"]) problems.Add(at + ": armor or its AC contribution changed");
                if ((int)row["markers"] != 1) problems.Add(at + ": the marker is not present exactly once");
                if ((int)row["markerModifiers"] != 0) problems.Add(at + ": the marker contributes an AC modifier");
                if ((int)row["componentCalls"] < 1) problems.Add(at + ": the marker component did not run during the AC calculation");
            }
            if ((string)baseline["armor"] == null) problems.Add("the character wears no armor; the control would not exercise an armor contribution");
        }

        private IEnumerable<object> Reload()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            UnitEntityData target = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == MarkedName);
            result["found"] = target != null;
            if (target == null) { problems.Add("reload: the marked character is missing"); yield break; }
            created.Add(target);
            Buff[] markers = Markers(target);
            result["loadedMarker"] = new JObject { ["count"] = markers.Length, ["guid"] = markers.FirstOrDefault()?.Blueprint.AssetGuid,
                ["components"] = new JArray(markers.SelectMany(b => b.Components).Select(c => (object)c.GetType().FullName).ToArray()),
                ["endTime"] = markers.FirstOrDefault()?.EndTime.ToString(), ["casterPresent"] = markers.FirstOrDefault()?.Context?.MaybeCaster != null };
            if (markers.Length != 1 || !markers[0].Components.Any(c => c.GetType().Name == "BebelithDismantledArmorComponent"))
                problems.Add("reload: the marker did not deserialize exactly once with its component");
            yield return null;
            var rows = new JArray();
            result["ac"] = rows;
            JObject first = Ac(main, target, "reloadFirst");
            rows.Add(first);
            float until = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < until) { Game.Instance.IsPaused = false; yield return null; }
            Game.Instance.IsPaused = true;
            JObject second = Ac(main, target, "reloadRepeat");
            rows.Add(second);
            foreach (JObject row in new[] { first, second })
            {
                if ((int)row["markers"] != 1) problems.Add((string)row["label"] + ": the marker is not present exactly once");
                if ((int)row["markerModifiers"] != 0) problems.Add((string)row["label"] + ": the marker contributes an AC modifier");
                if ((int)row["componentCalls"] < 1) problems.Add((string)row["label"] + ": the marker component did not run");
                if ((int)row["ruleTargetAC"] != (int)first["ruleTargetAC"]) problems.Add("reload: AC changed between repeated calculations");
            }
            if ((string)first["armor"] == null) problems.Add("reload: the character lost its armor");
        }
    }
}
