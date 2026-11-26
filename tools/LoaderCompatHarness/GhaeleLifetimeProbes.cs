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
    ///
    /// Round 6 (reviews R4-F2, R5-F1, R5-F4, R5-F5): every disposable unit has one destruction-request owner
    /// (OwnedDisposables) and final releases are built after the iterator's cleanup; the combat teardown never calls
    /// LeaveCombat: it releases the probe's own attacks and target, preserves an independent engagement acquired after
    /// intake, and the participants must leave combat natively once that engagement's control is released; the main
    /// character's witness move is owned separately and settled after its readback; the pending abandonment is observed
    /// through its whole window by a separately subscribed, execution-correlated summon witness.
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
        private readonly OwnedDisposables<UnitEntityData> units;
        private readonly List<BlueprintUnit> createdBlueprints = new List<BlueprintUnit>();
        private Type runner;
        private UnitEntityData attacker;
        private JArray attackRolls;
        private BlueprintUnit hostileBlueprint;

        public GhaeleLifetimeProbe(string mode, string evidenceDirectory)
        {
            this.mode = mode;
            this.evidenceDirectory = evidenceDirectory;
            result["mode"] = mode;
            // One destruction-request owner per unit; every request, direct or final, and any failure of one is kept in
            // the probe's one cumulative outcome that ProbeSlot reads (owner amendment 3, R6-F1).
            // Recovery C2 (R5-F1): a request is not its result; settlement is read (gone from the game state) per unit.
            units = new OwnedDisposables<UnitEntityData>("disposable unit", u => u.UniqueId, u => { if (!u.Destroyed) u.Destroy(); }, owned.Outcome,
                u => u == null || u.Destroyed || !Game.Instance.State.Units.Contains(u));
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
            // Built after the iterator's cleanup: only units no earlier release requested (R5-F5), then the blueprints.
            CleanupReport report = life.Release(() => new[] { new KeyValuePair<string, Action>("disposable units", units.ReleaseRemaining),
                    // Settlement of every request, read after the destroyer runs; anything still present or unreadable is a named failure.
                    new KeyValuePair<string, Action>("disposable units settled", () => { Game.Instance.EntityDestroyer.Tick(); units.RecordUnsettled(); }) }
                .Concat(ProbeReleases.PerItem("disposable blueprint", createdBlueprints, b => b.name + "#" + b.GetInstanceID(), b => UnityEngine.Object.Destroy(b))).ToList());
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
            // R5-F3: the exact subject, its view, and its own EntityFader component (presence, identity, Visible), read here.
            EntityFader fader = unit == null || unit.View == null ? null : unit.View.GetComponent<EntityFader>();
            row["subject"] = GhaeleLighting.Subject(unit == null ? null : unit.UniqueId, unit == null || unit.View == null ? (int?)null : unit.View.GetInstanceID(),
                fader == null ? (int?)null : fader.GetInstanceID(), fader == null ? (bool?)null : fader.Visible);
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
                if (own != null && !ReferenceEquals(own, handle.Registered) && !own.Destroyed && units.Destroy(own)) handle.LateSummonsDestroyed++;
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
                    mode == "lifecycle" ? Lifecycle(main, ability, donorBlueprint, library) :
                    mode == "visibility" ? Visibility(main, ability, donorBlueprint) : null;
                if (body == null) { life.Problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
                // Recovery C2: every owned unit is requested here and its settlement observed while frames still run.
                units.ReleaseRemaining();
                foreach (object step in SettleOwned(result, "unsettledAtEnd", 5f)) yield return step;
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
            units.Add(unit);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            return unit;
        }

        // A disposable target hostile to the Ghaele: a DefaultPlayerCharacter copy under the first loaded faction
        // (by asset GUID) that makes it the Ghaele's enemy; brain off, large hit points.
        private UnitEntityData SpawnHostile(LibraryScriptableObject library, UnitEntityData ghaele, Vector3 at, JObject row)
        {
            if (hostileBlueprint != null)
            {
                // A second target under the faction already found.
                UnitEntityData again = SpawnDisposable(hostileBlueprint, at);
                if (again != null) again.Descriptor.Stats.HitPoints.BaseValue = 100000;
                return again;
            }
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
                    hostileBlueprint = copy;
                    target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    row["hostileFaction"] = candidateFaction.name; row["hostileFactionGuid"] = candidateFaction.AssetGuid; row["factionsTried"] = tried;
                    return target;
                }
                if (target != null) units.Destroy(target);
            }
            row["factionsTried"] = tried;
            return null;
        }

        private IEnumerable<object> Observe(UnitEntityData main, BlueprintAbility ability, BlueprintUnit donorBlueprint, LibraryScriptableObject library)
        {
            var cast = new JObject(); result["cast"] = cast;
            UnitEntityData ghaele = null;
            var timelineHandle = new CastHandle();
            foreach (object step in CastOwned(main, ability, "timeline", cast, timelineHandle, null, unit => { ghaele = unit; units.Add(unit); })) yield return step;
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
            UnitEntityData hostile = null, control = null; bool combatOver = false, idleLit = false, combatLit = false, controlReleased = false, witnessSettled = false;
            double controlReleasedAt = -1, nativeLeaveAt = -1;
            var controlAttacks = new List<Kingmaker.UnitLogic.Commands.Base.UnitCommand>();
            int autoPauses = 0;
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
                // Combat (R3-F1, R4-F2, R5-F4, R5-F5): from 20 s, commanded attacks on a disposable hostile until three
                // attack rolls or 60 s. After the first roll a second disposable hostile, owned as a separate independent
                // engagement control, engages the Ghaele with its own commanded attacks (an engagement acquired after a
                // clean intake). The teardown releases only the probe's own attacks and target; the control and the main
                // character's witness move must survive it; the witness is then settled, the control released, and every
                // participant idle at intake must leave combat natively. LeaveCombat is never called.
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
                            // Only the probe's own attack commands and its own target (one destruction request).
                            if (!ghaele.Destroyed) { ghaele.Commands.InterruptAll(value => attacks.Contains(value)); ghaele.Commands.RemoveFinishedAndUpdateQueue(); }
                            units.Destroy(target);
                        });
                    }
                }
                if (hostile != null && !combatOver)
                {
                    if (!ghaele.Commands.HasUnfinished() && attackRolls.Count < 3) { var attack = new UnitAttack(hostile); attacks.Add(attack); ghaele.Commands.Run(attack); }
                    if (!combatLit && attackRolls.Count >= 1) { combatLit = true; lighting.Add(Lighting(ghaele, "combat")); }
                    if (control == null && attackRolls.Count >= 1)
                    {
                        Vector3 side = Quaternion.Euler(0, ghaele.Orientation + 120f, 0) * Vector3.forward;
                        control = SpawnHostile(library, ghaele, ObstacleAnalyzer.TraceAlongNavmesh(ghaele.Position, ghaele.Position + side * 1.6f), combat);
                        combat["controlStartedAt"] = Math.Round(t, 2);
                        combat["control"] = control == null ? null : control.UniqueId;
                        if (control != null)
                        {
                            UnitEntityData engaged = control;
                            owned.Hold("independent engagement control", () =>
                            {
                                controlReleased = true;
                                if (!engaged.Destroyed) { engaged.Commands.InterruptAll(value => controlAttacks.Contains(value)); engaged.Commands.RemoveFinishedAndUpdateQueue(); }
                                units.Destroy(engaged);
                            });
                        }
                    }
                    if (control != null && !controlReleased && !control.Destroyed && !control.Commands.HasUnfinished())
                    { var controlAttack = new UnitAttack(ghaele); controlAttacks.Add(controlAttack); control.Commands.Run(controlAttack); }
                    if (attackRolls.Count >= 3 || t > 60)
                    {
                        combatOver = true;
                        combat["endedAt"] = Math.Round(t, 2);
                        // R5-F4: the witness move on the main character is owned on its own, registered before it is issued.
                        var witness = new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + Quaternion.Euler(0, main.Orientation, 0) * Vector3.back * 8f), .3f);
                        owned.Hold("witness move", () => witnessSettled = WitnessSettlement.Settle(
                            () => { main.Commands.InterruptAll(value => ReferenceEquals(value, witness)); main.Commands.RemoveFinishedAndUpdateQueue(); },
                            () => witness.IsFinished, () => main.Commands.Contains(witness)));
                        main.Commands.Run(witness);
                        combat["witnessBefore"] = CommandState(main, witness);
                        var teardownFailures = new List<string>();
                        owned.ReleaseNow("combat state", teardownFailures);
                        combat["teardownFailures"] = new JArray(teardownFailures.Cast<object>().ToArray());
                        combat["witnessAfterTeardown"] = CommandState(main, witness);
                        combat["ownedAttacks"] = attacks.Count; combat["ownedAttacksLeft"] = attacks.Count(value => !value.IsFinished && ghaele.Commands.Contains(value));
                        combat["targetDestroyRequested"] = units.WasRequested(hostile);
                        // What still fights after the teardown, and why: the independent control is preserved; any other
                        // live enemy in combat is an engagement the probe does not own, reported as a conflict, never cleared.
                        bool controlAlive = control != null && !control.Destroyed && !units.WasRequested(control);
                        combat["controlAfterTeardown"] = new JObject { ["preserved"] = controlAlive, ["inCombat"] = controlAlive && control.CombatState.IsInCombat,
                            ["ghaeleInCombat"] = !ghaele.Destroyed && ghaele.CombatState.IsInCombat };
                        var conflicts = new JArray();
                        foreach (UnitEntityData participant in intake.Where(entry => !entry.Value && entry.Key != null && !entry.Key.Destroyed && entry.Key.CombatState.IsInCombat).Select(entry => entry.Key))
                            foreach (UnitEntityData enemy in Game.Instance.State.Units.Where(u => u != null && !u.Destroyed && u.CombatState.IsInCombat && participant.IsEnemy(u) &&
                                !ReferenceEquals(u, hostile) && !ReferenceEquals(u, control)))
                                conflicts.Add(new JObject { ["participant"] = participant.UniqueId, ["engagement"] = enemy.UniqueId, ["blueprint"] = enemy.Blueprint.name });
                        combat["unownedEngagements"] = conflicts;
                        combat["inCombatAfterTeardown"] = new JArray(intake.Where(entry => entry.Key != null && !entry.Key.Destroyed && entry.Key.CombatState.IsInCombat).Select(entry => (object)entry.Key.UniqueId).ToArray());
                        // Settled right after its survival readback.
                        owned.ReleaseNow("witness move", teardownFailures);
                        combat["witnessSettled"] = witnessSettled;
                        owned.ReleaseNow("independent engagement control", teardownFailures);
                        combat["controlReleasedAt"] = Math.Round(t, 2); controlReleasedAt = t;
                        combat["releaseFailures"] = new JArray(teardownFailures.Cast<object>().ToArray());
                    }
                }
                // The participants leave combat by the native controller once nothing the probe introduced is fighting.
                if (controlReleasedAt >= 0 && nativeLeaveAt < 0 && intake.Where(entry => !entry.Value && entry.Key != null)
                        .All(entry => entry.Key.Destroyed || !entry.Key.CombatState.IsInCombat))
                    nativeLeaveAt = t;
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
            combat["controlDestroyed"] = control == null ? (bool?)null : control.Destroyed || !Game.Instance.State.Units.Contains(control);
            combat["nativeLeaveSeconds"] = nativeLeaveAt < 0 || controlReleasedAt < 0 ? (double?)null : Math.Round(nativeLeaveAt - controlReleasedAt, 2);
            if (combat["endedAt"] != null)
            {
                JToken after = combat["controlAfterTeardown"];
                string combatVerdict = GhaeleCombat.Verdict((int)combat["ownedAttacksLeft"], (bool)combat["targetDestroyRequested"],
                    after != null && (bool)after["preserved"], after != null && (bool)after["inCombat"] && (bool)after["ghaeleInCombat"],
                    ((JArray)combat["unownedEngagements"]).Count, Survived((JObject)combat["witnessAfterTeardown"]), (bool)combat["witnessSettled"],
                    nativeLeaveAt >= 0 && nativeLeaveAt - controlReleasedAt <= 20);
                combat["verdict"] = combatVerdict;
                if (combatVerdict != null || combat["targetDestroyed"] == null || combat["targetDestroyed"].Type == JTokenType.Null || !(bool)combat["targetDestroyed"] ||
                    combat["controlDestroyed"] == null || combat["controlDestroyed"].Type == JTokenType.Null || !(bool)combat["controlDestroyed"] ||
                    ((JArray)combat["releaseFailures"]).Count != 0 || ((JArray)combat["teardownFailures"]).Count != 0)
                    life.Problems.Add("the combat teardown did not release exactly the observation's own activity: " + (combatVerdict ?? "a target or control not destroyed, or a failed release"));
            }
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
                foreach (object step in CastOwned(main, ability, variant.Key, borrowed, handle, null, unit => { first = unit; units.Add(unit); })) yield return step;
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
                units.Destroy(first);
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
                }, unit => { second = unit; units.Add(unit); })) yield return step;
            Ability afterReplace = main.Descriptor.Abilities.GetAbility(ability);
            bool replacedReleased = LoanVerdict(main, ability, replacedHandle, replaced);
            replaced["replacementSurvived"] = replacement != null && ReferenceEquals(afterReplace, replacement);
            replaced["replacementUntouched"] = replacement != null && replacement.OverrideParams == null;
            if (replaced["granted"] == null || !(bool)replaced["granted"] || !replacedReleased || !(bool)replaced["replacementSurvived"] || !(bool)replaced["replacementUntouched"])
                life.Problems.Add("lifecycle.replaced: the replacement grant did not survive the loan's release untouched");
            owned.ReleaseNow("replacement setup", null);
            units.Destroy(second);
            foreach (object step in Frames(10)) yield return step;

            // 3. Abandoned with the command pending (R5-F1). The known foreign same-caster Ghaele is summoned first (the
            //    product path adds and removes the same ability), then the owned cast is queued and the unrelated command
            //    started; the exact command and loan are read immediately before disposal, after all of that. A separately
            //    subscribed witness, correlated to the command's execution, watches through the whole window.
            var pending = new JObject(); cases["abandonedPending"] = pending;
            UnitEntityData foreignPending = SummonForeignGhaele(main, library);
            pending["foreignGhaele"] = foreignPending == null ? null : foreignPending.UniqueId;
            foreach (object step in Frames(5)) yield return step;
            SummonWitness pendingWitness = Watch("pending late-spawn witness", main, foreignPending);
            var pendingHandle = new CastHandle();
            IEnumerator<object> pendingRun = CastOwned(main, ability, "pending", pending, pendingHandle, null, unit => units.Add(unit)).GetEnumerator();
            pendingRun.MoveNext();
            pendingWitness.Begin(pendingHandle.Command);
            var pendingMove = new UnitMoveTo(ObstacleAnalyzer.TraceAlongNavmesh(bystander.Position, bystander.Position + back * 12f), .3f);
            bystander.Commands.Run(pendingMove);
            var unrelated = new UnitCommandProbe { Unit = bystander, Command = pendingMove };
            pending["bystanderBeforeAbandon"] = commandState(unrelated);
            UnitUseAbility pendingCommand = pendingHandle.Command;
            Ability heldNow = main.Descriptor.Abilities.GetAbility(ability);
            bool commandPending = pendingCommand != null && main.Commands.Contains(pendingCommand) && !pendingCommand.IsFinished && !pendingCommand.IsActed && pendingCommand.ExecutionProcess == null;
            bool loanHeld = heldNow != null && ReferenceEquals(heldNow, pendingHandle.Used) && heldNow.OverrideParams != null && heldNow.OverrideParams.CasterLevel == 20;
            pending["beforeDispose"] = new JObject { ["commandPending"] = commandPending, ["loanHeld"] = loanHeld, ["grantIsCastInstance"] = heldNow != null && ReferenceEquals(heldNow, pendingHandle.Granted),
                ["overrideCasterLevel"] = heldNow == null || heldNow.OverrideParams == null ? (int?)null : heldNow.OverrideParams.CasterLevel };
            pendingRun.Dispose();
            pending["bystanderAfterAbandon"] = commandState(unrelated);
            foreach (object step in Watching(pendingWitness, 6f)) yield return step;
            pending["ownedCommandRemains"] = pendingCommand != null && main.Commands.Contains(pendingCommand) && !pendingCommand.IsFinished;
            pending["foreignPreserved"] = foreignPending != null && !foreignPending.Destroyed && Game.Instance.State.Units.Contains(foreignPending);
            pending["abilityGrantRemains"] = main.Descriptor.Abilities.GetAbility(ability) != null;
            pending["bystanderCommandSurvived"] = (bool)pending["bystanderBeforeAbandon"]["held"] && survived((JObject)pending["bystanderAfterAbandon"]);
            owned.ReleaseNow("pending late-spawn witness", null);
            pending["witness"] = pendingWitness.Describe();
            foreach (object step in SettleOwned(pending, "unsettledAfterWitness", 5f)) yield return step;
            string pendingVerdict = GhaelePending.Verdict(commandPending, loanHeld, (bool)pending["ownedCommandRemains"], (bool)pending["abilityGrantRemains"],
                (bool)pending["foreignPreserved"], pendingWitness.Owned.Count, pendingWitness.Unknown.Count, pendingWitness.Subscribed);
            pending["verdict"] = pendingVerdict;
            if (pendingVerdict != null || !(bool)pending["bystanderCommandSurvived"] || !(bool)pending["commandReleased"] || !(bool)pending["loanReleased"])
                life.Problems.Add("lifecycle.pending: the abandoned pending command was not settled exactly: " + (pendingVerdict ?? "bystander or release"));

            // 3b. The pending verdict's negative control (owner amendment 3, R5-F1), genuinely after disposal: a second
            //     owned cast is queued with the same known foreign control; zero owned results are established before
            //     disposal; the actual cast owner is disposed; a real disposable unit is registered with its own
            //     destruction owner; then a labelled controlled notification injection delivers to the witness's own
            //     handler a summon result correlated to that exact cast (a fresh execution context of the cast's own
            //     AbilityData, the rule not triggered). Every verdict input is the case's own readback; the verdict must
            //     reject the late result, the foreign control survives and the witness unsubscribes.
            var late = new JObject(); cases["abandonedPendingLateResult"] = late;
            var order = new JArray(); late["order"] = order;
            SummonWitness lateWitness = Watch("late-result witness", main, foreignPending);
            Action<string> mark = name => order.Add(new JObject { ["step"] = name, ["frame"] = Time.frameCount, ["witnessOwned"] = lateWitness.Owned.Count });
            var lateHandle = new CastHandle();
            IEnumerator<object> lateRun = CastOwned(main, ability, "pendingLate", late, lateHandle, null, unit => units.Add(unit)).GetEnumerator();
            lateRun.MoveNext();
            lateWitness.Begin(lateHandle.Command);
            mark("queued");
            UnitUseAbility lateCommand = lateHandle.Command;
            Ability lateHeld = main.Descriptor.Abilities.GetAbility(ability);
            bool lateCommandPending = lateCommand != null && main.Commands.Contains(lateCommand) && !lateCommand.IsFinished && !lateCommand.IsActed && lateCommand.ExecutionProcess == null;
            bool lateLoanHeld = lateHeld != null && ReferenceEquals(lateHeld, lateHandle.Used) && lateHeld.OverrideParams != null && lateHeld.OverrideParams.CasterLevel == 20;
            late["beforeDispose"] = new JObject { ["commandPending"] = lateCommandPending, ["loanHeld"] = lateLoanHeld, ["witnessOwned"] = lateWitness.Owned.Count };
            mark("before-disposal");
            lateRun.Dispose();
            mark("cast-owner-disposed");
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            UnitEntityData lateUnit = foreignPending == null ? null : SpawnDisposable(foreignPending.Blueprint, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + right * 3f));
            late["lateUnit"] = lateUnit == null ? null : lateUnit.UniqueId;
            late["lateUnitOwnedBeforeDelivery"] = lateUnit != null && units.Remaining.Contains(lateUnit);
            mark("late-unit-registered");
            if (lateUnit != null && lateCommand != null)
            {
                var lateContext = new AbilityExecutionContext(lateCommand.Spell, new AbilityParams { CasterLevel = 20, SpellLevel = 9 }, new TargetWrapper(lateUnit.Position));
                var lateRule = new RuleSummonUnit(main, lateUnit.Blueprint, lateUnit.Position, new Rounds(1), 20) { Context = lateContext };
                typeof(RuleSummonUnit).GetProperty("SummonedUnit", Any).GetSetMethod(true).Invoke(lateRule, new object[] { lateUnit });
                late["injection"] = "controlled notification injection (labelled, owner amendment 3 section 4): the witness handler receives a RuleSummonUnit, never triggered, whose context is a fresh execution context of the abandoned cast's own AbilityData and whose summoned unit is a real disposable unit registered with the probe's destruction owner before delivery";
                lateWitness.OnEventDidTrigger(lateRule);
            }
            mark("late-result-delivered");
            foreach (object step in Watching(lateWitness, 1f)) yield return step;
            late["ownedCommandRemains"] = lateCommand != null && main.Commands.Contains(lateCommand) && !lateCommand.IsFinished;
            late["abilityGrantRemains"] = main.Descriptor.Abilities.GetAbility(ability) != null;
            late["foreignPreserved"] = foreignPending != null && !foreignPending.Destroyed && Game.Instance.State.Units.Contains(foreignPending);
            owned.ReleaseNow("late-result witness", null);
            late["witness"] = lateWitness.Describe();
            string lateVerdict = GhaelePending.Verdict(lateCommandPending, lateLoanHeld, (bool)late["ownedCommandRemains"], (bool)late["abilityGrantRemains"],
                (bool)late["foreignPreserved"], lateWitness.Owned.Count, lateWitness.Unknown.Count, lateWitness.Subscribed);
            late["verdict"] = lateVerdict;
            // Recovery C2: the witness's release reconciled every correlated unit with the destruction owner (one request
            // each); the caller now observes their settlement, and anything still present or unreadable fails cleanup.
            late["lateUnitDestroyRequestedByReconciliation"] = lateUnit != null && units.WasRequested(lateUnit);
            if (lateUnit != null) units.Destroy(lateUnit);
            foreach (object step in SettleOwned(late, "unsettledAfterWitness", 5f)) yield return step;
            late["lateUnitDestroyRequested"] = lateUnit != null && units.WasRequested(lateUnit);
            late["lateUnitDestroyed"] = lateUnit != null && (lateUnit.Destroyed || !Game.Instance.State.Units.Contains(lateUnit));
            string[] expectedOrder = { "queued", "before-disposal", "cast-owner-disposed", "late-unit-registered", "late-result-delivered" };
            bool ordered = order.Select(entry => (string)entry["step"]).SequenceEqual(expectedOrder) &&
                order.Take(4).All(entry => (int)entry["witnessOwned"] == 0) && (int)order[4]["witnessOwned"] == 1;
            late["ordered"] = ordered;
            if (lateVerdict != "late-owned-summon" || !ordered || lateWitness.Owned.Count != 1 || !ReferenceEquals(lateWitness.Owned[0], lateUnit) ||
                (string)lateWitness.Describe()["matchedBy"][0] != "ability-data" || !(bool)late["lateUnitOwnedBeforeDelivery"] || !(bool)late["lateUnitDestroyRequested"] ||
                !(bool)late["lateUnitDestroyed"] || !(bool)late["commandReleased"] || !(bool)late["loanReleased"])
                life.Problems.Add("lifecycle.pendingLate: the post-disposal late result was not rejected by the pending verdict as required: " + (lateVerdict ?? "verdict passed"));
            units.Destroy(foreignPending);
            foreach (object step in Frames(10)) yield return step;

            // 4. Abandoned after the owned Ghaele spawned (identified by the native summon rule of the cast's own
            //    execution) and before ordinary registration, with a foreign same-caster Ghaele from another execution.
            var spawned = new JObject(); cases["abandonedAfterSpawn"] = spawned;
            var spawnHandle = new CastHandle();
            // The witness's real-time detection control: the same witness on a cast whose Ghaele does spawn.
            SummonWitness spawnWitness = Watch("spawn detection witness", main, null);
            IEnumerator<object> spawnRun = CastOwned(main, ability, "spawned", spawned, spawnHandle, null, unit => units.Add(unit)).GetEnumerator();
            spawnRun.MoveNext();
            spawnWitness.Begin(spawnHandle.Command);
            float until = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < until && spawnRun.MoveNext())
            {
                spawnWitness.Capture();
                if (spawnHandle.Summoned != null) break;
                yield return null;
            }
            UnitEntityData early = spawnHandle.Summoned;
            spawned["spawnSeenBeforeRegistration"] = early != null && spawnHandle.Registered == null;
            spawned["ownedSummon"] = early == null ? null : early.UniqueId;
            UnitEntityData foreignSpawned = SummonForeignGhaele(main, library);
            spawned["foreignGhaele"] = foreignSpawned == null ? null : foreignSpawned.UniqueId;
            if (foreignSpawned != null) spawnWitness.Known.Add(foreignSpawned);
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
            owned.ReleaseNow("spawn detection witness", null);
            spawned["witness"] = spawnWitness.Describe();
            foreach (object step in SettleOwned(spawned, "unsettledAfterWitness", 5f)) yield return step;
            if (spawnWitness.Owned.Count != 1 || !ReferenceEquals(spawnWitness.Owned[0], early) || spawnWitness.Subscribed)
                life.Problems.Add("lifecycle.spawned: the witness did not detect exactly the cast's own real spawn, or did not unsubscribe");
            if (!(bool)spawned["spawnSeenBeforeRegistration"] || !(bool)spawned["earlySummonDestroyed"] || !(bool)spawned["foreignPreserved"] ||
                foreignSpawned == null || ReferenceEquals(foreignSpawned, early) || (bool)spawned["ownedCommandRemains"] || (bool)spawned["abilityGrantRemains"] ||
                (int)spawned["lateSummonsDestroyed"] != 1 || !(bool)spawned["bystanderCommandSurvived"])
                life.Problems.Add("lifecycle.spawned: the abandoned cast did not destroy exactly its own summon");
            units.Destroy(foreignSpawned);
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
            if (unit != null) this.units.Add(unit);
            return unit;
        }

        private sealed class UnitCommandProbe { internal UnitEntityData Unit; internal Kingmaker.UnitLogic.Commands.Base.UnitCommand Command; }

        private static JObject CommandState(UnitEntityData unit, Kingmaker.UnitLogic.Commands.Base.UnitCommand command)
        {
            return new JObject { ["held"] = unit.Commands.Contains(command), ["finished"] = command.IsFinished, ["result"] = command.IsFinished ? command.Result.ToString() : null };
        }

        // Still held by its unit, or ended other than by interruption.
        private static bool Survived(JObject state)
        {
            return state != null && ((bool)state["held"] || ((bool)state["finished"] && (string)state["result"] != "Interrupt"));
        }

        /// <summary>
        /// R5-F1: a read-only witness of summons, subscribed on its own and kept through a whole observation window. A
        /// summon is the watched cast's when its rule's context is (or derives from) that command's execution; any other
        /// KMG Ghaele the same caster summons that is not a known unit is unattributed.
        /// </summary>
        /// Owner amendment 3: the witness holds its own correlation (GhaeleWitness): the exact AbilityData taken from the
        /// watched command when watching began, and every execution context of that command it has seen, retained.
        private sealed class SummonWitness : IGlobalRulebookHandler<RuleSummonUnit>
        {
            private UnitUseAbility command;
            private AbilityData data;
            private readonly List<object> executions = new List<object>();
            internal UnitEntityData Caster;
            internal readonly List<UnitEntityData> Known = new List<UnitEntityData>();
            internal readonly List<UnitEntityData> Owned = new List<UnitEntityData>();
            internal readonly List<string> MatchedBy = new List<string>();
            internal readonly List<UnitEntityData> Unattributed = new List<UnitEntityData>();
            internal bool Subscribed;

            /// <summary>Same-caster Ghaele the witness could not attribute and the caller does not know (read now, not when seen).</summary>
            internal List<UnitEntityData> Unknown { get { return Unattributed.Where(unit => !Known.Contains(unit)).ToList(); } }

            internal void Begin(UnitUseAbility watched)
            {
                command = watched;
                data = watched == null ? null : watched.Spell;
                Capture();
            }

            /// <summary>Retains the watched command's current execution context, if it has one.</summary>
            internal void Capture()
            {
                AbilityExecutionContext execution = command == null || command.ExecutionProcess == null ? null : command.ExecutionProcess.Context;
                if (execution != null && !executions.Any(value => ReferenceEquals(value, execution))) executions.Add(execution);
            }

            public void OnEventAboutToTrigger(RuleSummonUnit evt) { }

            public void OnEventDidTrigger(RuleSummonUnit evt)
            {
                if (evt == null || evt.SummonedUnit == null) return;
                Capture();
                AbilityExecutionContext source = evt.Context == null ? null : evt.Context.SourceAbilityContext;
                string matched = GhaeleWitness.Correlated(data, executions, evt.Context, source, evt.Context is AbilityExecutionContext ? ((AbilityExecutionContext)evt.Context).Ability : null,
                    source == null ? null : source.Ability);
                if (matched != null) { Owned.Add(evt.SummonedUnit); MatchedBy.Add(matched); return; }
                if (IsKmgGhaele(evt.SummonedUnit) && (ReferenceEquals(evt.Initiator, Caster) || SummonedBy(evt.SummonedUnit, Caster)) && !Known.Contains(evt.SummonedUnit))
                    Unattributed.Add(evt.SummonedUnit);
            }

            internal JObject Describe()
            {
                return new JObject { ["owned"] = new JArray(Owned.Select(u => (object)u.UniqueId).ToArray()), ["matchedBy"] = new JArray(MatchedBy.Cast<object>().ToArray()),
                    ["unattributed"] = new JArray(Unattributed.Select(u => (object)u.UniqueId).ToArray()), ["unknown"] = new JArray(Unknown.Select(u => (object)u.UniqueId).ToArray()),
                    ["known"] = new JArray(Known.Select(u => (object)u.UniqueId).ToArray()), ["retainedExecutions"] = executions.Count, ["subscribed"] = Subscribed };
            }
        }

        private static IEnumerable<object> Watching(SummonWitness witness, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) { witness.Capture(); yield return null; }
        }

        // ---------------------------------------------------------------- visibility (owner amendment 3, R5-F3)

        /// <summary>
        /// Mode "visibility": a short current capture of the repaired subject readback through the producer and the
        /// verdict. The published summon is cast as one owned command (as in the timeline) and the native donor spawned;
        /// after the appearance, the Ghaele's lighting row is taken with its own fader present and visible; the
        /// disposable donor's fader is then set hidden by the probe (held, restored to its prior value) and its row taken.
        /// The verdict must accept the first and refuse the second as fader-hidden. A missing component is exercised
        /// offline through the same producer and verdict (removing a live view's fader is not attempted).
        /// </summary>
        private IEnumerable<object> Visibility(UnitEntityData main, BlueprintAbility ability, BlueprintUnit donorBlueprint)
        {
            var cast = new JObject(); result["cast"] = cast;
            UnitEntityData ghaele = null;
            var handle = new CastHandle();
            foreach (object step in CastOwned(main, ability, "visibility", cast, handle, null, unit => { ghaele = unit; units.Add(unit); })) yield return step;
            if (ghaele == null) yield break;
            if (!LoanVerdict(main, ability, handle, cast)) life.Problems.Add("visibility: the summon ability loan was not released to its prior state: " + cast["loan"]["reason"]);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { ghaele, false });
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            UnitEntityData donor = SpawnDisposable(donorBlueprint, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - forward * 3f));
            foreach (object step in Seconds(5f)) yield return step;
            var rows = new JArray(); result["visibility"] = rows;
            JObject visible = Lighting(ghaele, "visibility-present-visible");
            rows.Add(visible);
            EntityFader fader = donor == null || donor.View == null ? null : donor.View.GetComponent<EntityFader>();
            JObject hidden = null;
            if (fader != null)
            {
                bool prior = fader.Visible;
                EntityFader exact = fader;
                owned.Hold("donor fader visibility", () => { if (exact != null) exact.Visible = prior; });
                exact.Visible = false;
                hidden = Lighting(donor, "visibility-present-hidden");
                rows.Add(hidden);
                owned.ReleaseNow("donor fader visibility", null);
            }
            foreach (JObject row in rows.OfType<JObject>())
            {
                string reason;
                row["valid"] = GhaeleLighting.Valid(row, out reason); row["reason"] = reason;
            }
            result["visibilityVerdict"] = new JObject { ["presentVisible"] = visible["reason"], ["presentHidden"] = hidden == null ? "not-captured" : hidden["reason"] };
            if (!(bool)visible["valid"]) life.Problems.Add("visibility: the Ghaele's present, visible fader row did not qualify: " + visible["reason"]);
            if (hidden == null || (string)hidden["reason"] != "fader-hidden") life.Problems.Add("visibility: the hidden donor fader was not refused as fader-hidden: " + (hidden == null ? "not captured" : (string)hidden["reason"]));
        }

        private SummonWitness Watch(string name, UnitEntityData caster, UnitEntityData known)
        {
            var witness = new SummonWitness { Caster = caster };
            if (known != null) witness.Known.Add(known);
            EventBus.Subscribe(witness); witness.Subscribed = true;
            // Recovery C2 (R5-F1): the witness stays read-only; its caller, on every release path (early, iterator unwind,
            // final), reconciles each correlated unit with the destruction owner and records unknown ownership as a failure.
            owned.Hold(name, () =>
            {
                if (witness.Subscribed) { EventBus.Unsubscribe(witness); witness.Subscribed = false; }
                LateResultReconciliation.Release(units, witness.Owned, witness.Unknown, "witness " + name + ": unit");
            });
            return witness;
        }

        /// <summary>Waits (bounded) until every requested unit is read gone, then records any still present or unreadable as a named cleanup failure.</summary>
        private IEnumerable<object> SettleOwned(JObject row, string field, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end && units.Unsettled().Count > 0) yield return null;
            row[field] = new JArray(units.Unsettled().Select(entry => (object)(entry.Key.UniqueId + ": " + entry.Value)).ToArray());
            units.RecordUnsettled();
        }

        private static IEnumerable<object> Frames(int count) { for (int frame = 0; frame < count; frame++) yield return null; }

        private static IEnumerable<object> Seconds(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }
    }
}
