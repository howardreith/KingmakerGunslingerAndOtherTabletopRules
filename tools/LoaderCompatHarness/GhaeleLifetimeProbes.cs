using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 7 T1 / REP-04 (review R1-F2): an ordinary Ghaele timeline on whatever product package the session loads
    /// (the released v0.0.195 package for the baseline). The harness, not the product, observes: it casts the published
    /// Ghaele summon as a native UnitUseAbility from the main character (caster level 20 only; an existing ability is
    /// borrowed and its overrides restored, a granted one released by instance), keeps the arrival and appearance,
    /// spawns the native donor AzataGhaelSummoned without any summon, and reads with time unpaused until the summon's
    /// native expiry: the enabled body renderer's _Dissolve and _DissolveCutout, the appearance and lifetime buffs,
    /// every native dissolve animation on the body controller with its driven materials, and an ordinary move.
    /// The fixture save has no hostile, so no attack is observed here.
    /// </summary>
    internal sealed class GhaeleLifetimeProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string SummonAbility = "KMG_Summoning_Ability_SM_Tier9_GhaeleAzata_One";
        private const string DonorGuid = "bc8ca1437c0f48948b317b7e64febf0d";

        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly OwnedProbeResources owned = new OwnedProbeResources();
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private string verdict;
        private Type runner;

        public GhaeleLifetimeProbe(string mode) { result["mode"] = mode; script = Run().GetEnumerator(); }
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

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("probe iterator", () => script.Dispose()),
                new KeyValuePair<string, Action>("held state", () =>
                {
                    CleanupReport held = owned.ReleaseAll();
                    if (!held.Restored) throw new InvalidOperationException(string.Join("; ", held.Failures.ToArray()));
                }),
                new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); })
            });
        }

        private static SkinnedMeshRenderer Body(UnitEntityData unit)
        {
            return unit == null || unit.View == null ? null : unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.enabled && value.sharedMesh != null).OrderByDescending(value => value.sharedMesh.vertexCount).FirstOrDefault();
        }

        private static float Read(Renderer renderer, string property)
        {
            if (renderer == null) return -1f;
            float max = -1f;
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.HasProperty(property)) max = Mathf.Max(max, material.GetFloat(property));
            return max;
        }

        private static Buff BuffOf(UnitEntityData unit, BlueprintScriptableObject blueprint)
        {
            return unit == null || unit.Destroyed ? null : unit.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, blueprint));
        }

        // Every native dissolve animation on the unit's controllers, with its driven materials' own _Dissolve.
        private static JArray Controllers(UnitEntityData unit)
        {
            var rows = new JArray();
            if (unit == null || unit.View == null) return rows;
            foreach (StandardMaterialController controller in unit.View.GetComponentsInChildren<StandardMaterialController>(true))
            {
                var settings = typeof(StandardMaterialController).GetField("m_MaterialSettings", Any).GetValue(controller) as IDictionary;
                object driver = typeof(StandardMaterialController).GetField("m_DissolveController", Any).GetValue(controller);
                var animations = driver == null ? null : driver.GetType().GetField("Animations", Any).GetValue(driver) as IList;
                var driven = new JArray();
                if (settings != null)
                    foreach (object key in settings.Keys)
                    {
                        var material = key as Material;
                        if (material != null) driven.Add(new JObject { ["material"] = material.GetInstanceID(), ["dissolve"] = material.HasProperty("_Dissolve") ? material.GetFloat("_Dissolve") : -1f });
                    }
                var animationRows = new JArray();
                if (animations != null)
                    foreach (object animation in animations)
                    {
                        var row = new JObject();
                        foreach (string name in new[] { "IsStarted", "IsDelayed", "Delay", "Lifetime", "LoopAnimation", "DissolveCutout", "PlayBackwards", "CurrentDissolve" })
                        {
                            FieldInfo field = animation.GetType().GetField(name, Any);
                            PropertyInfo property = animation.GetType().GetProperty(name, Any);
                            object value = field != null ? field.GetValue(animation) : property != null ? property.GetValue(animation, null) : null;
                            row[name] = value == null ? null : JToken.FromObject(value);
                        }
                        animationRows.Add(row);
                    }
                rows.Add(new JObject { ["controller"] = controller.GetInstanceID(), ["animations"] = animationRows, ["driven"] = driven });
            }
            return rows;
        }

        private static string Signature(JToken animation)
        {
            return string.Format(CultureInfo.InvariantCulture, "delay={0:F2} lifetime={1:F2} loop={2} cutout={3} backwards={4}",
                (double)animation["Delay"], (double)animation["Lifetime"], (bool)animation["LoopAnimation"], (bool)animation["DissolveCutout"], (bool)animation["PlayBackwards"]);
        }

        private IEnumerable<object> Run()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            result["productVersion"] = ProductProbes.Entry().Info.Version;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var ability = library.GetAllBlueprints().OfType<BlueprintAbility>().FirstOrDefault(value => value.name == SummonAbility);
            var donorBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(DonorGuid);
            if (ability == null || donorBlueprint == null) { problems.Add("the published Ghaele summon or the native donor is missing"); yield break; }
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            bool paused = Game.Instance.IsPaused;
            owned.Hold("pause", () => Game.Instance.IsPaused = paused);
            Game.Instance.IsPaused = false;
            try
            {
                foreach (object step in Observe(main, ability, donorBlueprint)) yield return step;
            }
            finally
            {
                CleanupReport held = owned.ReleaseAll();
                if (!held.Restored) problems.Add("restore failures: " + string.Join("; ", held.Failures.ToArray()));
                result["pauseRestored"] = Game.Instance.IsPaused == paused;
            }
        }

        private IEnumerable<object> Observe(UnitEntityData main, BlueprintAbility ability, BlueprintUnit donorBlueprint)
        {
            // The summon ability: an existing one is borrowed, a granted one released by instance.
            Ability existing = main.Descriptor.Abilities.GetAbility(ability);
            Ability granted = existing == null ? main.Descriptor.AddFact(ability) as Ability : null;
            Ability used = existing ?? granted;
            if (used == null) { problems.Add("the summon could not be granted"); yield break; }
            AbilityParams priorOverride = used.OverrideParams;
            owned.Hold("summon ability", () =>
            {
                used.OverrideParams = priorOverride;
                if (granted != null && ReferenceEquals(main.Descriptor.Abilities.GetAbility(ability), granted)) main.Descriptor.Abilities.RemoveFact(granted);
            });
            used.OverrideParams = new AbilityParams { CasterLevel = 20, SpellLevel = 9 };
            var before = new HashSet<UnitEntityData>(Game.Instance.State.Units);
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            Vector3 point = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + forward * 3f);
            var command = new UnitUseAbility(new AbilityData(used), new TargetWrapper(point));
            var cast = new JObject { ["ability"] = ability.name, ["borrowed"] = existing != null, ["granted"] = granted != null };
            result["cast"] = cast;
            main.Commands.Run(command);
            float deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return null;
            Game.Instance.EntityCreator.Tick();
            cast["result"] = command.Result.ToString(); cast["finished"] = command.IsFinished;
            UnitEntityData ghaele = Game.Instance.State.Units.FirstOrDefault(value => !before.Contains(value) && value.Blueprint != null && value.Blueprint.name.IndexOf("GhaeleAzata", StringComparison.Ordinal) >= 0);
            cast["spawned"] = ghaele == null ? null : ghaele.Blueprint.name;
            if (!command.IsFinished || command.Result != Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success || ghaele == null)
            { problems.Add("the published summon did not complete with a Ghaele"); yield break; }
            created.Add(ghaele);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { ghaele, false });
            UnitEntityData donor = Game.Instance.EntityCreator.SpawnUnit(donorBlueprint,
                ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - forward * 3f), Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            created.Add(donor);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { donor, false });

            BlueprintScriptableObject appearBuff = BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff;
            BlueprintScriptableObject lifeBuff = BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
            TimeSpan start = Game.Instance.Player.GameTime;
            double appearedAt = -1, lifetimeEnd = -1, removedAt = -1, lastSample = -1, lastController = -10, maxCutout = -1, maxDissolve = -1;
            bool moved = false; Vector3 moveFrom = Vector3.zero; double travel = -1;
            var samples = new JArray(); var controllers = new JArray(); var donorControllers = new JArray();
            result["samples"] = samples; result["controllers"] = controllers; result["donorControllers"] = donorControllers;
            float wall = Time.realtimeSinceStartup + 240f;
            while (Time.realtimeSinceStartup < wall)
            {
                double t = (Game.Instance.Player.GameTime - start).TotalSeconds;
                bool alive = !ghaele.Destroyed && ghaele.View != null && Game.Instance.State.Units.Contains(ghaele);
                if (!alive) { removedAt = t; break; }
                Buff appear = BuffOf(ghaele, appearBuff), life = BuffOf(ghaele, lifeBuff);
                if (life != null) lifetimeEnd = (life.EndTime - start).TotalSeconds;
                if (appear == null && appearedAt < 0) appearedAt = t;
                SkinnedMeshRenderer body = Body(ghaele);
                float dissolve = Read(body, "_Dissolve"), cutout = Read(body, "_DissolveCutout");
                bool inLife = appearedAt >= 0 && t >= appearedAt + 1 && (lifetimeEnd < 0 || t < lifetimeEnd - .5);
                if (inLife) { maxCutout = Math.Max(maxCutout, cutout); maxDissolve = Math.Max(maxDissolve, dissolve); }
                if (t - lastSample >= .5)
                {
                    lastSample = t;
                    samples.Add(new JObject { ["t"] = Math.Round(t, 2), ["appear"] = appear != null, ["remaining"] = life == null ? (double?)null : Math.Round((life.EndTime - Game.Instance.Player.GameTime).TotalSeconds, 2),
                        ["body"] = body == null ? null : body.name, ["dissolve"] = Math.Round(dissolve, 3), ["dissolveCutout"] = Math.Round(cutout, 3), ["visible"] = body != null && body.enabled && body.isVisible });
                }
                if (t - lastController >= 10)
                {
                    lastController = t;
                    controllers.Add(new JObject { ["t"] = Math.Round(t, 1), ["controllers"] = Controllers(ghaele) });
                    if (!donor.Destroyed) donorControllers.Add(new JObject { ["t"] = Math.Round(t, 1), ["controllers"] = Controllers(donor) });
                }
                if (!moved && appearedAt >= 0 && t > 8)
                {
                    moved = true; moveFrom = ghaele.Position;
                    Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
                    ghaele.Commands.Run(new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(ghaele.Position, ghaele.Position + right * 4f), .3f));
                }
                if (moved && travel < 0 && t > 16) travel = Vector3.Distance(moveFrom, ghaele.Position);
                yield return null;
            }

            // Phase-independent attribution: every animation on the summon in life is one the donor also runs, none
            // is a cutout dissolve, and each driven material's _Dissolve equals the animation's own current value.
            var donorSignatures = new HashSet<string>(donorControllers.SelectMany(row => row["controllers"]).SelectMany(c => c["animations"]).Select(Signature));
            int inLifeSnapshots = 0, consistent = 0;
            foreach (JToken row in controllers.Where(row => (double)row["t"] >= appearedAt + 1 && (lifetimeEnd < 0 || (double)row["t"] < lifetimeEnd - .5)))
            {
                inLifeSnapshots++;
                bool ok = true;
                foreach (JToken controller in row["controllers"])
                    foreach (JToken animation in controller["animations"])
                    {
                        if (!donorSignatures.Contains(Signature(animation)) || (bool)animation["DissolveCutout"]) ok = false;
                        if ((bool)animation["IsStarted"] && !(bool)animation["IsDelayed"])
                            foreach (JToken material in controller["driven"])
                                if ((float)material["dissolve"] >= 0 && Math.Abs((float)material["dissolve"] - (float)animation["CurrentDissolve"]) > .001) ok = false;
                    }
                if (ok) consistent++;
            }
            result["timeline"] = new JObject { ["appearanceEndedAt"] = Math.Round(appearedAt, 2), ["lifetimeEndsAt"] = Math.Round(lifetimeEnd, 2), ["removedAt"] = Math.Round(removedAt, 2),
                ["maxBodyDissolveCutoutInLife"] = maxCutout, ["maxBodyDissolveInLife"] = maxDissolve, ["moveTravel"] = Math.Round(travel, 2),
                ["donorSignatures"] = new JArray(donorSignatures), ["snapshotsInLife"] = inLifeSnapshots, ["consistentSnapshots"] = consistent };
            if (appearedAt < 0) problems.Add("the appearance did not complete");
            if (maxCutout < 0 || maxCutout > .02) problems.Add("the body showed a cutout dissolve in life: " + maxCutout);
            if (removedAt < 0 || lifetimeEnd < 0 || removedAt < lifetimeEnd - 1 || removedAt > lifetimeEnd + 15) problems.Add("the summon was not removed at its native expiry");
            if (travel <= 2) problems.Add("the ordinary move did not travel");
            if (donorSignatures.Count == 0 || inLifeSnapshots < 5 || consistent != inLifeSnapshots) problems.Add("the in-life dissolve is not the donor's native loop");
        }
    }
}
