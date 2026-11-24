using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 7 T1 / REP-04: an ordinary Ghaele timeline on whatever product package the session loads (the released
    /// v0.0.195 package for the baseline). The harness, not the product, observes.
    ///
    /// Mode "timeline": the published Ghaele summon is cast as one owned native UnitUseAbility from the main character
    /// (caster level 20 only, for that cast: an existing ability is borrowed and its parameters restored, or a temporary
    /// grant released by instance, as soon as the cast settles; R2-F4). The command and any summon it produces are owned
    /// from submission (R3-F5). The arrival and appearance are kept; the native donor AzataGhaelSummoned is spawned
    /// without any summon; time runs unpaused to the summon's native expiry. Readbacks: the enabled body renderer's
    /// _Dissolve and _DissolveCutout and material identities, the appearance and lifetime buffs, every native dissolve
    /// animation with its driven materials, an ordinary move, and (review R3-F1) an ordinary combat: a disposable
    /// hostile target (a DefaultPlayerCharacter copy under a loaded faction hostile to the Ghaele, brain off) is
    /// attacked by commanded UnitAttack, with the attack rolls, body readbacks and the lighting context (ambient,
    /// interpolated light probe, nearby lights, and the game camera's own party view through the product's capture
    /// helper) recorded idle and in combat. Attribution of the body value to the donor loop uses GhaeleAttribution
    /// (R3-F6).
    ///
    /// Mode "lifecycle": the cast's ownership boundaries (R2-F4, R3-F5): a borrowed ability with non-default parameters
    /// (restored, same instance, at the first post-cast observation); a temporary grant replaced during the cast (the
    /// replacement survives the loan's release); the cast abandoned with its command pending; the cast abandoned after
    /// its Ghaele spawned and before ordinary registration. No owned command or late summon may remain, and an
    /// unrelated unit's command survives.
    /// </summary>
    internal sealed class GhaeleLifetimeProbe : IOwnedProbe, IGlobalRulebookHandler<RuleAttackRoll>, IGlobalRulebookHandler<RuleSummonUnit>
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string SummonAbility = "KMG_Summoning_Ability_SM_Tier9_GhaeleAzata_One";
        private const string DonorGuid = "bc8ca1437c0f48948b317b7e64febf0d";

        private readonly string mode;
        private readonly string evidenceDirectory;
        private readonly JObject result = new JObject();
        private readonly OwnedProbeResources owned = new OwnedProbeResources();
        private readonly YieldedProbeLifecycle life;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<BlueprintUnit> createdBlueprints = new List<BlueprintUnit>();
        private Type runner;
        private UnitEntityData attacker;
        private JArray attackRolls;

        public GhaeleLifetimeProbe(string mode, string evidenceDirectory)
        {
            this.mode = mode;
            this.evidenceDirectory = evidenceDirectory;
            result["mode"] = mode;
            life = new YieldedProbeLifecycle(owned, lifecycle => Run());
        }

        public object Result => result;

        public string Poll()
        {
            string verdict = life.Poll();
            if (verdict != null) result["problems"] = new JArray(life.Problems.Cast<object>().ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            var releases = ProbeReleases.PerItem("disposable unit", created.Where(u => u != null), u => u.UniqueId, u => { if (!u.Destroyed) u.Destroy(); });
            releases.AddRange(ProbeReleases.PerItem("disposable blueprint", createdBlueprints, b => b.name + "#" + b.GetInstanceID(), b => UnityEngine.Object.Destroy(b)));
            CleanupReport report = life.Release(releases);
            result["problems"] = new JArray(life.Problems.Cast<object>().ToArray());
            return report;
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt) { }

        // R3-F5: the native summon rule carries the summoning context; the unit it produced belongs to the cast whose
        // command's execution that context is (or derives from).
        private readonly List<CastHandle> casts = new List<CastHandle>();
        public void OnEventAboutToTrigger(RuleSummonUnit evt) { }
        public void OnEventDidTrigger(RuleSummonUnit evt)
        {
            if (evt == null || evt.SummonedUnit == null || evt.Context == null) return;
            foreach (CastHandle handle in casts)
            {
                AbilityExecutionContext execution = handle.Command == null || handle.Command.ExecutionProcess == null ? null : handle.Command.ExecutionProcess.Context;
                if (execution != null && handle.Summoned == null && (ReferenceEquals(evt.Context, execution) || ReferenceEquals(evt.Context.SourceAbilityContext, execution)))
                    handle.Summoned = evt.SummonedUnit;
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (evt == null || attackRolls == null || attacker == null || !ReferenceEquals(evt.Initiator, attacker)) return;
            attackRolls.Add(new JObject { ["gameTime"] = Math.Round(Game.Instance.Player.GameTime.TotalSeconds, 2),
                ["target"] = evt.Target == null ? null : evt.Target.UniqueId, ["hit"] = evt.IsHit });
        }

        // ---------------------------------------------------------------- readbacks

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

        // Every native dissolve animation on the unit's controllers, with its driven materials' own _Dissolve, and the
        // enabled body renderer's own material identities.
        private static JObject Controllers(UnitEntityData unit, double t)
        {
            var rows = new JArray();
            SkinnedMeshRenderer body = Body(unit);
            var snapshot = new JObject { ["t"] = Math.Round(t, 1), ["controllers"] = rows,
                ["bodyMaterials"] = body == null ? null : new JArray(body.sharedMaterials.Where(m => m != null).Select(m => (object)m.GetInstanceID()).ToArray()) };
            if (unit == null || unit.View == null) return snapshot;
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
            return snapshot;
        }

        // The lighting the body actually stands in: ambient, the interpolated light probe at the body, nearby lights,
        // and the game camera's own party view (product capture helper, bound to the enabled body renderer).
        private JObject Lighting(UnitEntityData unit, string label)
        {
            SkinnedMeshRenderer body = Body(unit);
            var row = new JObject { ["label"] = label, ["gameTime"] = Math.Round(Game.Instance.Player.GameTime.TotalSeconds, 2),
                ["ambientMode"] = RenderSettings.ambientMode.ToString(), ["ambientIntensity"] = RenderSettings.ambientIntensity,
                ["ambientLight"] = Colour(RenderSettings.ambientLight) };
            try
            {
                SphericalHarmonicsL2 probe;
                LightProbes.GetInterpolatedProbe(unit.Position + Vector3.up, body, out probe);
                row["probeDcLuminance"] = Math.Round(.2126 * probe[0, 0] + .7152 * probe[1, 0] + .0722 * probe[2, 0], 4);
            }
            catch (Exception exception) { row["probeError"] = exception.GetType().Name; }
            row["lights"] = new JArray(UnityEngine.Object.FindObjectsOfType<Light>().Where(light => light != null && light.isActiveAndEnabled &&
                    (light.type == LightType.Directional || Vector3.Distance(light.transform.position, unit.Position) <= light.range))
                .Select(light => (object)new JObject { ["name"] = light.name, ["type"] = light.type.ToString(), ["intensity"] = Math.Round(light.intensity, 3),
                    ["distance"] = light.type == LightType.Directional ? (double?)null : Math.Round(Vector3.Distance(light.transform.position, unit.Position), 2),
                    ["colour"] = Colour(light.color) }).ToArray());
            MethodInfo capture = runner.GetMethod("WriteExpandedSummoningPartyCameraCapture", Any);
            if (capture != null && body != null && !string.IsNullOrEmpty(evidenceDirectory))
                row["partyCapture"] = (string)capture.Invoke(null, new object[] { unit, evidenceDirectory, "s7-ghaele-" + label + ".png", body.name });
            row["body"] = body == null ? null : body.name;
            row["bodyMesh"] = body == null || body.sharedMesh == null ? null : body.sharedMesh.name;
            row["bodyDissolve"] = Read(body, "_Dissolve");
            row["bodyDissolveCutout"] = Read(body, "_DissolveCutout");
            return row;
        }

        private static string Colour(Color value)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F3},{1:F3},{2:F3}", value.r, value.g, value.b);
        }

        private static bool IsKmgGhaele(UnitEntityData unit)
        {
            return unit != null && unit.Blueprint != null && unit.Blueprint.name.IndexOf("GhaeleAzata", StringComparison.Ordinal) >= 0;
        }

        private static bool LoanVerdict(UnitEntityData main, BlueprintAbility ability, CastHandle handle, JObject row)
        {
            Ability now = main.Descriptor.Abilities.GetAbility(ability);
            string reason;
            bool released = GhaeleLoan.Released(handle.Existing != null, now, handle.Existing, handle.Granted, now == null ? null : now.OverrideParams, handle.PriorParams, out reason);
            row["loan"] = new JObject { ["released"] = released, ["reason"] = reason, ["abilityPresent"] = now != null,
                ["sameInstance"] = handle.Existing != null ? ReferenceEquals(now, handle.Existing) : (bool?)null, ["grantRetained"] = handle.Granted != null ? ReferenceEquals(now, handle.Granted) : (bool?)null,
                ["overrideParams"] = now == null || now.OverrideParams == null ? null : new JObject { ["casterLevel"] = now.OverrideParams.CasterLevel, ["spellLevel"] = now.OverrideParams.SpellLevel } };
            return released;
        }

        private static bool SummonedBy(UnitEntityData unit, UnitEntityData summoner)
        {
            UnitPartSummonedMonster part = unit == null ? null : unit.Get<UnitPartSummonedMonster>();
            return part != null && ReferenceEquals(part.Summoner, summoner);
        }

        // ---------------------------------------------------------------- the owned cast (R2-F4, R3-F5)

        private sealed class CastHandle
        {
            internal Ability Used, Granted, Existing;
            internal AbilityParams PriorParams;
            internal UnitUseAbility Command;
            internal HashSet<UnitEntityData> Before;
            internal UnitEntityData Registered, Summoned;
            internal bool LoanReleased, CommandReleased;
            internal int LateSummonsDestroyed;
        }

        /// <summary>
        /// One cast of the published summon from <paramref name="main"/>, owned from submission. The ability loan lasts
        /// until the cast settles; the exact command and any Ghaele it produced are released on every earlier exit
        /// (timeout or abandonment while yielded): the command is interrupted (its execution detached) and a summon not
        /// yet registered is destroyed. Each release is held in the probe's resources, so a failure reaches the slot.
        /// </summary>
        private IEnumerable<object> CastOwned(UnitEntityData main, BlueprintAbility ability, string tag, JObject cast, CastHandle handle,
            Action<CastHandle> beforeLoanRelease, Action<UnitEntityData> register)
        {
            handle.Existing = main.Descriptor.Abilities.GetAbility(ability);
            handle.Granted = handle.Existing == null ? main.Descriptor.AddFact(ability) as Ability : null;
            handle.Used = handle.Existing ?? handle.Granted;
            if (handle.Used == null) { life.Problems.Add(tag + ": the summon could not be granted"); yield break; }
            handle.PriorParams = handle.Used.OverrideParams;
            string loan = tag + ": summon ability loan", command = tag + ": summon command", late = tag + ": unregistered summons";
            owned.Hold(loan, () =>
            {
                if (handle.LoanReleased) return;
                handle.LoanReleased = true;
                handle.Used.OverrideParams = handle.PriorParams;
                // Only the exact temporary grant, and only while it is still the unit's instance.
                if (handle.Granted != null && ReferenceEquals(main.Descriptor.Abilities.GetAbility(ability), handle.Granted)) main.Descriptor.Abilities.RemoveFact(handle.Granted);
            });
            handle.Used.OverrideParams = new AbilityParams { CasterLevel = 20, SpellLevel = 9 };
            handle.Before = new HashSet<UnitEntityData>(Game.Instance.State.Units);
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            Vector3 point = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + forward * 3f);
            handle.Command = new UnitUseAbility(new AbilityData(handle.Used), new TargetWrapper(point));
            casts.Add(handle);
            owned.Hold(late, () =>
            {
                // Only the unit this cast's own execution summoned, and only if it was never registered.
                Game.Instance.EntityCreator.Tick();
                UnitEntityData own = handle.Summoned;
                if (own != null && !ReferenceEquals(own, handle.Registered) && !own.Destroyed) { own.Destroy(); handle.LateSummonsDestroyed++; }
            });
            owned.Hold(command, () =>
            {
                handle.CommandReleased = true;
                UnitUseAbility ownedCommand = handle.Command;
                if (!ownedCommand.IsFinished) { main.Commands.InterruptAll(value => ReferenceEquals(value, ownedCommand)); main.Commands.RemoveFinishedAndUpdateQueue(); }
                if (ownedCommand.ExecutionProcess != null && !ownedCommand.ExecutionProcess.IsEnded) ownedCommand.ExecutionProcess.Detach();
            });
            cast["ability"] = ability.name; cast["borrowed"] = handle.Existing != null; cast["granted"] = handle.Granted != null;
            try
            {
                main.Commands.Run(handle.Command);
                float deadline = Time.realtimeSinceStartup + 30f;
                while (Time.realtimeSinceStartup < deadline && !(handle.Command.IsFinished && (handle.Command.ExecutionProcess == null || handle.Command.ExecutionProcess.IsEnded)))
                    yield return null;
                Game.Instance.EntityCreator.Tick();
                cast["result"] = handle.Command.Result.ToString(); cast["finished"] = handle.Command.IsFinished;
                cast["overrideDuringCast"] = handle.Used.OverrideParams == null ? null : new JObject { ["casterLevel"] = handle.Used.OverrideParams.CasterLevel, ["spellLevel"] = handle.Used.OverrideParams.SpellLevel };
                if (beforeLoanRelease != null) beforeLoanRelease(handle);
                // The cast has settled: the loan and the command end now, before any lifetime observation.
                owned.ReleaseNow(loan, null);
                owned.ReleaseNow(command, null);
                // One frame between settlement and registration (the summon is in the game, not yet registered).
                yield return null;
                UnitEntityData ghaele = handle.Summoned != null && !handle.Summoned.Destroyed && IsKmgGhaele(handle.Summoned) ? handle.Summoned : null;
                cast["spawned"] = ghaele == null ? null : ghaele.Blueprint.name;
                cast["summonedId"] = ghaele == null ? null : ghaele.UniqueId;
                if (!handle.Command.IsFinished || handle.Command.Result != Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success || ghaele == null)
                { life.Problems.Add(tag + ": the published summon did not complete with a Ghaele"); yield break; }
                handle.Registered = ghaele;
                register(ghaele);
            }
            finally
            {
                // Every exit: whatever of this cast is still held is released now, once (ReleaseNow finds nothing
                // already released); failures are kept in the probe's resources and reach the slot.
                owned.ReleaseNow(command, null);
                owned.ReleaseNow(loan, null);
                owned.ReleaseNow(late, null);
                cast["loanReleased"] = handle.LoanReleased; cast["commandReleased"] = handle.CommandReleased;
                cast["lateSummonsDestroyed"] = handle.LateSummonsDestroyed;
                casts.Remove(handle);
            }
        }

        // ---------------------------------------------------------------- run

        private IEnumerable<object> Run()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            result["productVersion"] = ProductProbes.Entry().Info.Version;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var ability = library.GetAllBlueprints().OfType<BlueprintAbility>().FirstOrDefault(value => value.name == SummonAbility);
            var donorBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(DonorGuid);
            if (ability == null || donorBlueprint == null) { life.Problems.Add("the published Ghaele summon or the native donor is missing"); yield break; }
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            bool paused = Game.Instance.IsPaused;
            owned.Hold("pause", () => Game.Instance.IsPaused = paused);
            Game.Instance.IsPaused = false;
            EventBus.Subscribe(this);
            owned.Hold("rule subscription", () => EventBus.Unsubscribe(this));
            try
            {
                IEnumerable<object> body = mode == "timeline" ? Observe(main, ability, donorBlueprint, library) :
                    mode == "lifecycle" ? Lifecycle(main, ability, donorBlueprint, library) : null;
                if (body == null) { life.Problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally
            {
                CleanupReport held = owned.ReleaseAll();
                if (!held.Restored) life.Problems.Add("restore failures: " + string.Join("; ", held.Failures.ToArray()));
                result["pauseRestored"] = Game.Instance.IsPaused == paused;
            }
        }

        private UnitEntityData SpawnDisposable(BlueprintUnit blueprint, Vector3 at)
        {
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(blueprint, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (unit == null) return null;
            created.Add(unit);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            return unit;
        }

        // A disposable target hostile to the Ghaele: a DefaultPlayerCharacter copy under the first loaded faction
        // (by asset GUID) that makes it the Ghaele's enemy; brain off, large hit points.
        private UnitEntityData SpawnHostile(LibraryScriptableObject library, UnitEntityData ghaele, Vector3 at, JObject row)
        {
            FieldInfo faction = typeof(BlueprintUnit).GetField("Faction", Any) ?? typeof(BlueprintUnit).GetField("m_Faction", Any);
            object playerFaction = faction.GetValue(BlueprintRoot.Instance.DefaultPlayerCharacter);
            int tried = 0;
            foreach (BlueprintScriptableObject candidateFaction in library.GetAllBlueprints().Where(value => value != null && value.GetType().Name == "BlueprintFaction")
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                if (ReferenceEquals(candidateFaction, playerFaction)) continue;
                if (++tried > 60) break;
                BlueprintUnit copy = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                copy.name = "KMG_Harness_S7_GhaeleHostileTarget";
                faction.SetValue(copy, candidateFaction);
                createdBlueprints.Add(copy);
                UnitEntityData target = SpawnDisposable(copy, at);
                if (target != null && ghaele.IsEnemy(target))
                {
                    target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    row["hostileFaction"] = candidateFaction.name; row["hostileFactionGuid"] = candidateFaction.AssetGuid; row["factionsTried"] = tried;
                    return target;
                }
                if (target != null) { created.Remove(target); target.Destroy(); }
            }
            row["factionsTried"] = tried;
            return null;
        }

        private IEnumerable<object> Observe(UnitEntityData main, BlueprintAbility ability, BlueprintUnit donorBlueprint, LibraryScriptableObject library)
        {
            var cast = new JObject(); result["cast"] = cast;
            UnitEntityData ghaele = null;
            var timelineHandle = new CastHandle();
            foreach (object step in CastOwned(main, ability, "timeline", cast, timelineHandle, null, unit => { ghaele = unit; created.Add(unit); })) yield return step;
            if (ghaele == null) yield break;
            // R2-F4 / R4-F3: at the first post-cast observation, judged against the exact state this cast held.
            if (!LoanVerdict(main, ability, timelineHandle, cast)) life.Problems.Add("the summon ability loan was not released to its prior state: " + cast["loan"]["reason"]);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { ghaele, false });
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            UnitEntityData donor = SpawnDisposable(donorBlueprint, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - forward * 3f));
            attacker = ghaele; attackRolls = new JArray();

            BlueprintScriptableObject appearBuff = BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff;
            BlueprintScriptableObject lifeBuff = BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
            TimeSpan start = Game.Instance.Player.GameTime;
            double appearedAt = -1, lifetimeEnd = -1, removedAt = -1, lastSample = -1, lastController = -10, maxCutout = -1, maxDissolve = -1;
            bool moved = false; Vector3 moveFrom = Vector3.zero; double travel = -1;
            var samples = new JArray(); var controllers = new JArray(); var donorControllers = new JArray(); var lighting = new JArray();
            var combat = new JObject { ["attackRolls"] = attackRolls };
            var attacks = new List<Kingmaker.UnitLogic.Commands.Base.UnitCommand>();
            var intake = new Dictionary<UnitEntityData, bool>();
            result["samples"] = samples; result["controllers"] = controllers; result["donorControllers"] = donorControllers;
            result["lighting"] = lighting; result["combat"] = combat;
            UnitEntityData hostile = null; bool combatOver = false, idleLit = false, combatLit = false; int autoPauses = 0;
            float wall = Time.realtimeSinceStartup + 240f;
            while (Time.realtimeSinceStartup < wall)
            {
                if (Game.Instance.IsPaused) { Game.Instance.IsPaused = false; autoPauses++; }
                double t = (Game.Instance.Player.GameTime - start).TotalSeconds;
                bool alive = !ghaele.Destroyed && ghaele.View != null && Game.Instance.State.Units.Contains(ghaele);
                if (!alive) { removedAt = t; break; }
                Buff appear = BuffOf(ghaele, appearBuff), lifeTime = BuffOf(ghaele, lifeBuff);
                if (lifeTime != null) lifetimeEnd = (lifeTime.EndTime - start).TotalSeconds;
                if (appear == null && appearedAt < 0) appearedAt = t;
                SkinnedMeshRenderer body = Body(ghaele);
                float dissolve = Read(body, "_Dissolve"), cutout = Read(body, "_DissolveCutout");
                bool inLife = appearedAt >= 0 && t >= appearedAt + 1 && (lifetimeEnd < 0 || t < lifetimeEnd - .5);
                if (inLife) { maxCutout = Math.Max(maxCutout, cutout); maxDissolve = Math.Max(maxDissolve, dissolve); }
                string phase = hostile != null && !combatOver ? "combat" : "idle";
                if (t - lastSample >= .5)
                {
                    lastSample = t;
                    samples.Add(new JObject { ["t"] = Math.Round(t, 2), ["phase"] = phase, ["appear"] = appear != null, ["remaining"] = lifeTime == null ? (double?)null : Math.Round((lifeTime.EndTime - Game.Instance.Player.GameTime).TotalSeconds, 2),
                        ["body"] = body == null ? null : body.name, ["dissolve"] = Math.Round(dissolve, 3), ["dissolveCutout"] = Math.Round(cutout, 3), ["visible"] = body != null && body.enabled && body.isVisible });
                }
                if (t - lastController >= 10)
                {
                    lastController = t;
                    JObject snapshot = Controllers(ghaele, t); snapshot["phase"] = phase;
                    controllers.Add(snapshot);
                    if (donor != null && !donor.Destroyed) donorControllers.Add(Controllers(donor, t));
                }
                if (!idleLit && appearedAt >= 0 && t > 4) { idleLit = true; lighting.Add(Lighting(ghaele, "idle")); }
                if (!moved && appearedAt >= 0 && t > 8)
                {
                    moved = true; moveFrom = ghaele.Position;
                    Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
                    ghaele.Commands.Run(new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(ghaele.Position, ghaele.Position + right * 4f), .3f));
                }
                if (moved && travel < 0 && t > 16) travel = Vector3.Distance(moveFrom, ghaele.Position);
                // Combat: from 20 s, commanded attacks on a disposable hostile until three attack rolls or 60 s.
                if (hostile == null && !combatOver && t > 20)
                {
                    // Intake: combat is started only when no unit this observation will engage is already fighting.
                    foreach (UnitEntityData unit in new[] { ghaele, main, donor }.Where(u => u != null)) intake[unit] = unit.CombatState.IsInCombat;
                    combat["intakeInCombat"] = new JArray(intake.Where(entry => entry.Value).Select(entry => (object)entry.Key.UniqueId).ToArray());
                    if (intake.Any(entry => entry.Value)) { combatOver = true; combat["unavailable"] = "incompatible intake: a unit was already in combat"; continue; }
                    Vector3 facing = Quaternion.Euler(0, ghaele.Orientation, 0) * Vector3.forward;
                    hostile = SpawnHostile(library, ghaele, ObstacleAnalyzer.TraceAlongNavmesh(ghaele.Position, ghaele.Position + facing * 1.6f), combat);
                    combat["startedAt"] = Math.Round(t, 2);
                    if (hostile == null) { combatOver = true; combat["unavailable"] = "no loaded faction made a hostile target for the Ghaele"; }
                    else
                    {
                        UnitEntityData target = hostile;
                        owned.Hold("combat state", () =>
                        {
                            // Only the probe's own attack commands, its own target, and the combat state this
                            // observation introduced (units that were not fighting at intake).
                            if (!ghaele.Destroyed) { ghaele.Commands.InterruptAll(value => attacks.Contains(value)); ghaele.Commands.RemoveFinishedAndUpdateQueue(); }
                            if (!target.Destroyed) { created.Remove(target); target.Destroy(); }
                            foreach (KeyValuePair<UnitEntityData, bool> entry in intake)
                                if (!entry.Value && entry.Key != null && !entry.Key.Destroyed && entry.Key.CombatState.IsInCombat) entry.Key.CombatState.LeaveCombat();
                        });
                    }
                }
                if (hostile != null && !combatOver)
                {
                    if (!ghaele.Commands.HasUnfinished() && attackRolls.Count < 3) { var attack = new UnitAttack(hostile); attacks.Add(attack); ghaele.Commands.Run(attack); }
                    if (!combatLit && attackRolls.Count >= 1) { combatLit = true; lighting.Add(Lighting(ghaele, "combat")); }
                    if (attackRolls.Count >= 3 || t > 60)
                    {
                        combatOver = true;
                        combat["endedAt"] = Math.Round(t, 2);
                        // An independent command on the main character, started before the combat teardown.
                        var independent = new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + Quaternion.Euler(0, main.Orientation, 0) * Vector3.back * 8f), .3f);
                        main.Commands.Run(independent);
                        combat["mainIndependentBefore"] = new JObject { ["held"] = main.Commands.Contains(independent), ["finished"] = independent.IsFinished };
                        var teardownFailures = new List<string>();
                        owned.ReleaseNow("combat state", teardownFailures);
                        combat["teardownFailures"] = new JArray(teardownFailures.Cast<object>().ToArray());
                        combat["mainIndependentAfter"] = new JObject { ["held"] = main.Commands.Contains(independent), ["finished"] = independent.IsFinished,
                            ["result"] = independent.IsFinished ? independent.Result.ToString() : null };
                        combat["ownedAttacks"] = attacks.Count; combat["ownedAttacksLeft"] = attacks.Count(value => !value.IsFinished && ghaele.Commands.Contains(value));

                    }
                }
                yield return null;
            }

            // R3-F6: positive attribution only.
            var inLifeRows = controllers.Where(row => (double)row["t"] >= appearedAt + 1 && (lifetimeEnd < 0 || (double)row["t"] < lifetimeEnd - .5)).ToList();
            JObject attribution = GhaeleAttribution.Evaluate(inLifeRows, donorControllers);
            result["timeline"] = new JObject { ["appearanceEndedAt"] = Math.Round(appearedAt, 2), ["lifetimeEndsAt"] = Math.Round(lifetimeEnd, 2), ["removedAt"] = Math.Round(removedAt, 2),
                ["maxBodyDissolveCutoutInLife"] = maxCutout, ["maxBodyDissolveInLife"] = maxDissolve, ["moveTravel"] = Math.Round(travel, 2),
                ["attribution"] = attribution, ["autoPausesCleared"] = autoPauses };
            combat["attackRollCount"] = attackRolls.Count;
            // Unit destruction completes after the call; read once the observation has run on.
            combat["targetDestroyed"] = hostile == null ? (bool?)null : hostile.Destroyed || !Game.Instance.State.Units.Contains(hostile);
            if (appearedAt < 0) life.Problems.Add("the appearance did not complete");
            if (maxCutout < 0 || maxCutout > .02) life.Problems.Add("the body showed a cutout dissolve in life: " + maxCutout);
            if (removedAt < 0 || lifetimeEnd < 0 || removedAt < lifetimeEnd - 1 || removedAt > lifetimeEnd + 15) life.Problems.Add("the summon was not removed at its native expiry");
            if (travel <= 2) life.Problems.Add("the ordinary move did not travel");
            if (!(bool)attribution["attributed"]) life.Problems.Add("the in-life dissolve is not positively attributed to the donor's native loop: " + attribution["positive"] + " positive, " + attribution["contradicting"] + " contradicting");
            if (attackRolls.Count < 1) life.Problems.Add("no ordinary combat was observed: " + (combat["unavailable"] ?? "no attack roll"));
            JToken independentAfter = combat["mainIndependentAfter"];
            if (independentAfter == null || !((bool)independentAfter["held"] || ((bool)independentAfter["finished"] && (string)independentAfter["result"] != "Interrupt")) ||
                (int)combat["ownedAttacksLeft"] != 0 || combat["targetDestroyed"] == null || combat["targetDestroyed"].Type == JTokenType.Null || !(bool)combat["targetDestroyed"])
                life.Problems.Add("the combat teardown did not release exactly the observation's own activity");
            // R4-F6: each lighting observation is judged on its own readbacks.
            foreach (JObject row in lighting.OfType<JObject>())
            {
                string reason;
                row["valid"] = GhaeleLighting.Valid(row, out reason); row["reason"] = reason;
            }
            if (!idleLit || !combatLit || lighting.OfType<JObject>().Count(row => (bool)row["valid"]) != 2)
                life.Problems.Add("the idle and combat lighting observations were not both valid");
        }

        // ---------------------------------------------------------------- lifecycle (R2-F4, R3-F5)

        private IEnumerable<object> Lifecycle(UnitEntityData main, BlueprintAbility ability, BlueprintUnit donorBlueprint, LibraryScriptableObject library)
        {
            var cases = new JObject(); result["cases"] = cases;
            // The unrelated command is started right before each abandonment and read right after it: still held by
            // its unit and not ended as interrupted.
            Func<UnitCommandProbe, JObject> commandState = probe => new JObject { ["held"] = probe.Unit.Commands.Contains(probe.Command),
                ["finished"] = probe.Command.IsFinished, ["result"] = probe.Command.IsFinished ? probe.Command.Result.ToString() : null };
            Func<JObject, bool> survived = state => (bool)state["held"] || ((bool)state["finished"] && (string)state["result"] != "Interrupt");
            if (main.Descriptor.Abilities.GetAbility(ability) != null) { life.Problems.Add("lifecycle: the main character already holds the summon; the cases need it absent"); yield break; }

            // An unrelated unit whose own command no release may touch.
            Vector3 back = Quaternion.Euler(0, main.Orientation, 0) * Vector3.back;
            UnitEntityData bystander = SpawnDisposable(donorBlueprint, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + back * 4f));
            foreach (object step in Frames(30)) yield return step;

            // 1. Borrowed, with non-default parameters, then with an existing caster level 20 override; and the
            //    timeline's own verdict against an injected incorrect restoration.
            foreach (var variant in new[] { new { Key = "borrowedNonDefault", Cl = 7, Sl = 3 }, new { Key = "borrowedCl20Override", Cl = 20, Sl = 9 } })
            {
                var borrowed = new JObject(); cases[variant.Key] = borrowed;
                Ability pre = main.Descriptor.AddFact(ability) as Ability;
                string setup = variant.Key + " setup";
                owned.Hold(setup, () => { if (pre != null && ReferenceEquals(main.Descriptor.Abilities.GetAbility(ability), pre)) main.Descriptor.Abilities.RemoveFact(pre); });
                var setupParams = new AbilityParams { CasterLevel = variant.Cl, SpellLevel = variant.Sl };
                if (pre != null) pre.OverrideParams = setupParams;
                UnitEntityData first = null;
                var handle = new CastHandle();
                foreach (object step in CastOwned(main, ability, variant.Key, borrowed, handle, null, unit => { first = unit; created.Add(unit); })) yield return step;
                bool released = LoanVerdict(main, ability, handle, borrowed);
                bool sameParams = pre != null && ReferenceEquals(pre.OverrideParams, setupParams) && setupParams.CasterLevel == variant.Cl && setupParams.SpellLevel == variant.Sl;
                // Injected incorrect restoration: the verdict must refuse it; then the exact parameters are put back.
                var injected = new JObject();
                if (pre != null) { pre.OverrideParams = new AbilityParams { CasterLevel = variant.Cl, SpellLevel = variant.Sl }; }
                bool refusesWrong = !LoanVerdict(main, ability, handle, injected);
                if (pre != null) pre.OverrideParams = setupParams;
                borrowed["injectedWrongRestore"] = injected["loan"];
                bool ok = first != null && released && sameParams && refusesWrong && borrowed["overrideDuringCast"] != null &&
                    borrowed["overrideDuringCast"].Type == JTokenType.Object && (int)borrowed["overrideDuringCast"]["casterLevel"] == 20;
                borrowed["verdictHeld"] = ok;
                if (!ok) life.Problems.Add("lifecycle." + variant.Key + ": the loan verdict did not hold against the exact prior state");
                owned.ReleaseNow(setup, null);
                if (first != null) { created.Remove(first); first.Destroy(); }
                foreach (object step in Frames(10)) yield return step;
            }

            // Missing borrowed instance: the verdict refuses an ability that vanished.
            {
                var missing = new JObject(); cases["borrowedMissing"] = missing;
                Ability pre = main.Descriptor.AddFact(ability) as Ability;
                var handle = new CastHandle { Existing = pre, PriorParams = pre == null ? null : pre.OverrideParams };
                if (pre != null) main.Descriptor.Abilities.RemoveFact(pre);
                missing["refused"] = !LoanVerdict(main, ability, handle, missing);
                if (!(bool)missing["refused"]) life.Problems.Add("lifecycle.borrowedMissing: the loan verdict accepted a vanished borrowed ability");
            }

            // 2. A temporary grant replaced during the cast: the replacement survives the loan's release.
            var replaced = new JObject(); cases["replacedGrant"] = replaced;
            Ability replacement = null; UnitEntityData second = null;
            owned.Hold("replacement setup", () => { if (replacement != null && ReferenceEquals(main.Descriptor.Abilities.GetAbility(ability), replacement)) main.Descriptor.Abilities.RemoveFact(replacement); });
            var replacedHandle = new CastHandle();
            foreach (object step in CastOwned(main, ability, "replaced", replaced, replacedHandle, handle =>
                {
                    main.Descriptor.Abilities.RemoveFact(handle.Granted);
                    replacement = main.Descriptor.AddFact(ability) as Ability;
                }, unit => { second = unit; created.Add(unit); })) yield return step;
            Ability afterReplace = main.Descriptor.Abilities.GetAbility(ability);
            bool replacedReleased = LoanVerdict(main, ability, replacedHandle, replaced);
            replaced["replacementSurvived"] = replacement != null && ReferenceEquals(afterReplace, replacement);
            replaced["replacementUntouched"] = replacement != null && replacement.OverrideParams == null;
            if (replaced["granted"] == null || !(bool)replaced["granted"] || !replacedReleased || !(bool)replaced["replacementSurvived"] || !(bool)replaced["replacementUntouched"])
                life.Problems.Add("lifecycle.replaced: the replacement grant did not survive the loan's release untouched");
            owned.ReleaseNow("replacement setup", null);
            if (second != null) { created.Remove(second); second.Destroy(); }
            foreach (object step in Frames(10)) yield return step;

            // 3. Abandoned with the command pending, while a foreign same-caster Ghaele from another execution exists.
            var pending = new JObject(); cases["abandonedPending"] = pending;
            var pendingHandle = new CastHandle();
            IEnumerator<object> pendingRun = CastOwned(main, ability, "pending", pending, pendingHandle, null, unit => created.Add(unit)).GetEnumerator();
            pendingRun.MoveNext();
            pending["actedBeforeAbandon"] = pendingHandle.Command != null && pendingHandle.Command.IsActed;
            pending["executionBeforeAbandon"] = pendingHandle.Command != null && pendingHandle.Command.ExecutionProcess != null;
            UnitEntityData foreignPending = SummonForeignGhaele(main, library);
            pending["foreignGhaele"] = foreignPending == null ? null : foreignPending.UniqueId;
            var pendingMove = new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(bystander.Position, bystander.Position + back * 12f), .3f);
            bystander.Commands.Run(pendingMove);
            var unrelated = new UnitCommandProbe { Unit = bystander, Command = pendingMove };
            pending["bystanderBeforeAbandon"] = commandState(unrelated);
            pendingRun.Dispose();
            pending["bystanderAfterAbandon"] = commandState(unrelated);
            foreach (object step in Seconds(6f)) yield return step;
            pending["ownedSummon"] = pendingHandle.Summoned == null ? null : pendingHandle.Summoned.UniqueId;
            pending["ownedCommandRemains"] = pendingHandle.Command != null && main.Commands.Contains(pendingHandle.Command) && !pendingHandle.Command.IsFinished;
            pending["foreignPreserved"] = foreignPending != null && !foreignPending.Destroyed && Game.Instance.State.Units.Contains(foreignPending);
            pending["abilityGrantRemains"] = main.Descriptor.Abilities.GetAbility(ability) != null;
            pending["bystanderCommandSurvived"] = (bool)pending["bystanderBeforeAbandon"]["held"] && survived((JObject)pending["bystanderAfterAbandon"]);
            if ((bool)pending["executionBeforeAbandon"] || (bool)pending["ownedCommandRemains"] || pendingHandle.Summoned != null || (bool)pending["abilityGrantRemains"] ||
                !(bool)pending["foreignPreserved"] || !(bool)pending["bystanderCommandSurvived"] || !(bool)pending["commandReleased"] || !(bool)pending["loanReleased"])
                life.Problems.Add("lifecycle.pending: the abandoned pending command was not settled exactly");
            if (foreignPending != null) { created.Remove(foreignPending); if (!foreignPending.Destroyed) foreignPending.Destroy(); }
            foreach (object step in Frames(10)) yield return step;

            // 4. Abandoned after the owned Ghaele spawned (identified by the native summon rule of the cast's own
            //    execution) and before ordinary registration, with a foreign same-caster Ghaele from another execution.
            var spawned = new JObject(); cases["abandonedAfterSpawn"] = spawned;
            var spawnHandle = new CastHandle();
            IEnumerator<object> spawnRun = CastOwned(main, ability, "spawned", spawned, spawnHandle, null, unit => created.Add(unit)).GetEnumerator();
            float until = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < until && spawnRun.MoveNext())
            {
                if (spawnHandle.Summoned != null) break;
                yield return null;
            }
            UnitEntityData early = spawnHandle.Summoned;
            spawned["spawnSeenBeforeRegistration"] = early != null && spawnHandle.Registered == null;
            spawned["ownedSummon"] = early == null ? null : early.UniqueId;
            UnitEntityData foreignSpawned = SummonForeignGhaele(main, library);
            spawned["foreignGhaele"] = foreignSpawned == null ? null : foreignSpawned.UniqueId;
            bystander.Commands.InterruptAll(value => true); bystander.Commands.RemoveFinishedAndUpdateQueue();
            var spawnMove = new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(bystander.Position, bystander.Position - back * 12f), .3f);
            bystander.Commands.Run(spawnMove);
            var unrelatedSpawn = new UnitCommandProbe { Unit = bystander, Command = spawnMove };
            spawned["bystanderBeforeAbandon"] = commandState(unrelatedSpawn);
            spawnRun.Dispose();
            spawned["bystanderAfterAbandon"] = commandState(unrelatedSpawn);
            foreach (object step in Seconds(4f)) yield return step;
            spawned["earlySummonDestroyed"] = early != null && (early.Destroyed || !Game.Instance.State.Units.Contains(early));
            spawned["foreignPreserved"] = foreignSpawned != null && !foreignSpawned.Destroyed && Game.Instance.State.Units.Contains(foreignSpawned);
            spawned["ownedCommandRemains"] = spawnHandle.Command != null && main.Commands.Contains(spawnHandle.Command) && !spawnHandle.Command.IsFinished;
            spawned["abilityGrantRemains"] = main.Descriptor.Abilities.GetAbility(ability) != null;
            spawned["bystanderCommandSurvived"] = (bool)spawned["bystanderBeforeAbandon"]["held"] && survived((JObject)spawned["bystanderAfterAbandon"]);
            if (!(bool)spawned["spawnSeenBeforeRegistration"] || !(bool)spawned["earlySummonDestroyed"] || !(bool)spawned["foreignPreserved"] ||
                foreignSpawned == null || ReferenceEquals(foreignSpawned, early) || (bool)spawned["ownedCommandRemains"] || (bool)spawned["abilityGrantRemains"] ||
                (int)spawned["lateSummonsDestroyed"] != 1 || !(bool)spawned["bystanderCommandSurvived"])
                life.Problems.Add("lifecycle.spawned: the abandoned cast did not destroy exactly its own summon");
            if (foreignSpawned != null) { created.Remove(foreignSpawned); if (!foreignSpawned.Destroyed) foreignSpawned.Destroy(); }
        }

        // A same-caster Ghaele produced by another execution: the product's own summon path (not this probe's command).
        private UnitEntityData SummonForeignGhaele(UnitEntityData main, LibraryScriptableObject library)
        {
            Assembly product = runner.Assembly;
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { "ghaele-azata", Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), main, variant, null, evidence });
            UnitEntityData unit = units != null && units.Length == 1 ? units[0] : null;
            if (unit != null) created.Add(unit);
            return unit;
        }

        private sealed class UnitCommandProbe { internal UnitEntityData Unit; internal Kingmaker.UnitLogic.Commands.Base.UnitCommand Command; }

        private static IEnumerable<object> Frames(int count) { for (int frame = 0; frame < count; frame++) yield return null; }

        private static IEnumerable<object> Seconds(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }
    }
}
