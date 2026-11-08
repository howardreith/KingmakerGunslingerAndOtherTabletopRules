using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R6 (PRES-02..07), one save-free phase in the established Oleg
        // outdoor native Default fixture. Commands are submitted and native
        // progression is only observed; nothing here ticks an ability or
        // observes autonomous AI. Motion frames are framed from actual render
        // bounds with depth of field off for each render, and written with
        // their native animation state. A separate process records device
        // audio; retirement is judged from native cue end-of-event records.
        private const int R6FrameWidth = 960, R6FrameHeight = 540;
        private const int R6ArrivalFrames = 120;

        private static Bounds R6RenderBounds(params UnitEntityData[] units)
        {
            Bounds? total = null;
            foreach (UnitEntityData unit in units.Where(value => value != null && value.View != null))
                foreach (Renderer renderer in unit.View.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is SkinnedMeshRenderer || renderer is MeshRenderer) || !renderer.enabled ||
                        !renderer.gameObject.activeInHierarchy) continue;
                    Bounds box = renderer.bounds;
                    if (box.size.magnitude < .05f) continue;
                    if (total.HasValue) { Bounds merged = total.Value; merged.Encapsulate(box); total = merged; }
                    else total = box;
                }
            return total ?? new Bounds((units.FirstOrDefault(value => value != null)?.Position ?? Vector3.zero) + Vector3.up, Vector3.one * 2f);
        }

        private sealed class R6Clip : IDisposable
        {
            private readonly string _directory; private readonly Camera _camera;
            private readonly RenderTexture _texture; private readonly Texture2D _pixels;
            private readonly Func<Bounds> _subject;
            private static readonly System.Reflection.MethodInfo EncodeJpg =
                Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", true)
                    .GetMethod("EncodeToJPG", new[] { typeof(Texture2D), typeof(int) });
            private Vector3? _direction;
            internal readonly JArray Rows = new JArray(); internal readonly string Name; internal int Count; internal int OutOfFrame; internal int Occluded = -1;

            internal R6Clip(string evidence, string name, Func<Bounds> subject)
            {
                Name = name; _subject = subject;
                _directory = Path.Combine(evidence, "r6-frames", name); Directory.CreateDirectory(_directory);
                CameraRig rig = TeleportationCastingCamera();
                _camera = rig == null ? Camera.main : rig.Camera;
                if (_camera == null) throw new InvalidOperationException("No game camera for motion capture.");
                _texture = new RenderTexture(R6FrameWidth, R6FrameHeight, 24, RenderTextureFormat.ARGB32);
                _pixels = new Texture2D(R6FrameWidth, R6FrameHeight, TextureFormat.RGB24, false, false);
            }

            // First unobstructed bearing from the actual subject bounds; the
            // bearing then stays fixed so the clip reads as one camera.
            private Vector3 Bearing(Bounds bounds, float distance)
            {
                if (_direction.HasValue) return _direction.Value;
                int best = int.MaxValue; Vector3 chosen = Vector3.back;
                for (int step = 0; step < 8 && best > 0; step++)
                {
                    Vector3 candidate = Quaternion.Euler(0f, 45f * step, 0f) * new Vector3(.6f, .5f, -.62f).normalized;
                    int hits = Physics.RaycastAll(bounds.center, candidate, distance)
                        .Count(hit => hit.collider != null && hit.collider.GetComponentInParent<UnitEntityView>() == null);
                    if (hits < best) { best = hits; chosen = candidate; }
                }
                Occluded = best; _direction = chosen;
                return chosen;
            }

            internal void Shot(JObject state)
            {
                Vector3 position = _camera.transform.position; Quaternion rotation = _camera.transform.rotation;
                RenderTexture target = _camera.targetTexture, active = RenderTexture.active;
                var post = _camera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
                var depth = post != null && post.profile != null ? post.profile.depthOfField : null;
                bool depthBefore = depth != null && depth.enabled;
                try
                {
                    if (depth != null) depth.enabled = false;
                    Bounds bounds = _subject();
                    float radius = Mathf.Max(bounds.extents.magnitude, .9f);
                    float fit = Mathf.Sin(Mathf.Deg2Rad * Mathf.Min(_camera.fieldOfView, _camera.fieldOfView * 16f / 9f) * .5f);
                    float distance = Mathf.Max(radius * 1.25f / Mathf.Max(fit, .05f), 2.4f);
                    Vector3 direction = Bearing(bounds, distance);
                    _camera.transform.position = bounds.center + direction * distance;
                    _camera.transform.rotation = Quaternion.LookRotation(bounds.center - _camera.transform.position, Vector3.up);
                    _camera.targetTexture = _texture; _camera.Render();
                    RenderTexture.active = _texture;
                    _pixels.ReadPixels(new Rect(0, 0, R6FrameWidth, R6FrameHeight), 0, 0); _pixels.Apply(false, false);
                    Vector3 min = _camera.WorldToViewportPoint(bounds.min), max = _camera.WorldToViewportPoint(bounds.max);
                    bool inFrame = min.z > 0f && max.z > 0f && Mathf.Min(min.x, max.x) > -.05f && Mathf.Max(min.x, max.x) < 1.05f && Mathf.Min(min.y, max.y) > -.05f && Mathf.Max(min.y, max.y) < 1.05f;
                    if (!inFrame) OutOfFrame++;
                    string file = Count.ToString("D4", CultureInfo.InvariantCulture) + ".jpg";
                    File.WriteAllBytes(Path.Combine(_directory, file), (byte[])EncodeJpg.Invoke(null, new object[] { _pixels, 88 }));
                    state["file"] = file; state["frame"] = Time.frameCount; state["utc"] = DateTime.UtcNow.ToString("o"); state["inFrame"] = inFrame;
                    Rows.Add(state); Count++;
                }
                finally
                {
                    _camera.targetTexture = target; RenderTexture.active = active;
                    _camera.transform.position = position; _camera.transform.rotation = rotation;
                    if (depth != null) depth.enabled = depthBefore;
                }
            }

            public void Dispose()
            {
                File.WriteAllText(Path.Combine(_directory, "frames.json"), Rows.ToString(Formatting.Indented));
                if (_texture != null) { _texture.Release(); UnityEngine.Object.Destroy(_texture); }
                if (_pixels != null) UnityEngine.Object.Destroy(_pixels);
            }
        }

        private sealed class R6AttackLog : IGlobalRulebookHandler<RuleAttackRoll>, IDisposable
        {
            private readonly UnitEntityData _attacker; internal readonly JArray Rows = new JArray();
            internal R6AttackLog(UnitEntityData attacker) { _attacker = attacker; EventBus.Subscribe(this); }
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt.Initiator, _attacker))
                    Rows.Add(new JObject { ["weapon"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.name,
                        ["result"] = evt.Result.ToString(), ["hit"] = evt.IsHit, ["frame"] = Time.frameCount });
            }
            public void Dispose() { EventBus.Unsubscribe(this); }
        }

        private static JObject R6AnimationState(UnitEntityData unit, UnitCommand command = null)
        {
            var state = new JObject();
            UnitAnimationManager manager = unit == null || unit.View == null ? null : unit.View.AnimationManager;
            var handle = manager == null ? null : manager.CurrentAction;
            state["action"] = handle == null || handle.Action == null ? null : handle.Action.name;
            AnimationClip clip = handle == null || handle.ActiveAnimation == null ? null : handle.ActiveAnimation.GetPlayableClip();
            state["clip"] = clip == null ? null : clip.name;
            state["time"] = handle == null ? 0d : Math.Round(handle.GetTime(), 3);
            if (command != null) { state["started"] = command.IsStarted; state["acted"] = command.IsActed; }
            return state;
        }

        private static R6CommandFrame[] R6Frames(JArray rows)
        {
            return rows.Select(row => new R6CommandFrame { Action = (string)row["action"], Clip = (string)row["clip"],
                Started = row["started"] != null && (bool)row["started"], Acted = row["acted"] != null && (bool)row["acted"],
                GameFrame = (int)row["frame"] }).ToArray();
        }

        private static JArray R6DistinctClips(JArray rows)
        {
            return new JArray(rows.Select(value => (string)value["clip"]).Where(value => value != null).Distinct());
        }

        // One failed cell records its failure and the later clips still run.
        private IEnumerable<int> R6Cell(string name, Func<IEnumerable<int>> body)
        {
            IEnumerator<int> steps = null; Exception failure = null;
            try { steps = body().GetEnumerator(); } catch (Exception error) { failure = error; }
            while (failure == null)
            {
                bool more = false;
                try { more = steps.MoveNext(); } catch (Exception error) { failure = error; }
                if (!more) break;
                yield return steps.Current;
            }
            if (steps != null) steps.Dispose();
            Sprint25Check("r6-" + name + "-completed", failure == null,
                new JObject { ["error"] = failure == null ? null : failure.ToString() },
                "The cell ran to its end without an exception.");
        }

        private static IEnumerable<int> R6Wait(int frames) { for (int frame = 0; frame < frames; frame++) yield return 0; }

        private static IEnumerable<int> R6Seconds(float seconds)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end) yield return 0;
        }

        private static IEnumerable<int> R6Shots(R6Clip clip, UnitEntityData unit, int count)
        {
            for (int index = 0; index < count; index++) { clip.Shot(R6AnimationState(unit)); yield return 0; }
        }

        // Submit one native command and observe it: no direct ticking.
        private IEnumerable<int> R6RunPassive(UnitEntityData owner, AbilityData data, TargetWrapper target, JObject row, R6Clip clip)
        {
            row["blueprint"] = data.Blueprint.AssetGuid; row["blueprintName"] = data.Blueprint.name;
            row["actionType"] = data.Blueprint.ActionType.ToString();
            row["availableBefore"] = data.IsAvailable; row["canTarget"] = data.CanTarget(target);
            if (!data.IsAvailable || !data.CanTarget(target)) throw new InvalidOperationException("Native ability unavailable: " + data.Blueprint.name);
            var command = new UnitUseAbility(data, target);
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            int actedAt = -1, shots = 0;
            while (DateTime.UtcNow < deadline)
            {
                if (actedAt < 0 && command.IsActed) actedAt = shots;
                if (clip != null) clip.Shot(R6AnimationState(owner, command));
                shots++;
                yield return 0;
                if (command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)) break;
            }
            row["finished"] = command.IsFinished; row["result"] = command.Result.ToString();
            row["executionEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
            row["actedAtShot"] = actedAt;
            row["cancelled"] = !command.IsFinished || command.Result == UnitCommand.ResultType.Interrupt;
        }

        private IEnumerable<int> ReviewSummonPlaytestPresentationR6(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestPresentationPhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh R6 presentation fixture in owned outdoor native Default, without narrative, required.");
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId, ["fixture"] = "established non-DLC Oleg outdoor native Default; save-free",
                ["limits"] = "Commanded executions with brains inactive; no autonomous AI is observed. Audibility is established only by listening. Cue retirement is judged from native end-of-event records." };
            var timeline = new JArray();
            Action<string> mark = name => timeline.Add(new JObject { ["mark"] = name, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = fixture.Hostile;
            Vector3 hostilePosition = hostile.Position;
            int priorCapture = Time.captureFramerate;
            var recorder = new R4MephitRecorder(); EventBus.Subscribe(recorder);
            var saves = new List<Sprint26Saves> { new Sprint26Saves(hostile) { DesiredRoll = 20 } };
            Func<Vector3, Vector3> ground = value => ObstacleAnalyzer.GetNearestNode(value).clampedPosition;
            Func<string, UnitEntityData> summon = key =>
            {
                UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, key); owned.Add(unit);
                Sprint28SetBrainActivePreservingActions(unit, false);
                unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                return unit;
            };
            Func<UnitEntityData> summonRedcap = () =>
            {
                var spec = SummonNativeExpansionCatalog.All.First(value => value.UnitGuid == SummonPlaytestR6Policy.RedcapSummonedUnitGuid &&
                    value.Multiplicity == SummonMultiplicity.One);
                var ability = (BlueprintAbility)symbols[spec.Symbol];
                ExpandedSummoningRuleCapture.Clear();
                fixture.Caster.Descriptor.AddFact(ability);
                try { ExecuteExpandedSummoningRuntimeAbility(fixture.Caster, ability, spec.Tier); }
                finally { if (fixture.Caster.Descriptor.HasFact(ability)) fixture.Caster.Descriptor.RemoveFact(ability); }
                UnitEntityData unit = ExpandedSummoningRuleCapture.ToArray().Single();
                Game.Instance.EntityCreator.Tick();
                fixture.Created.Add(unit); owned.Add(unit);
                RemoveExpandedSummoningAppearanceBuffs(unit);
                PlaceExpandedSummoningUnit(unit, unit.Position);
                Sprint28SetBrainActivePreservingActions(unit, false);
                unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                return unit;
            };
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData = (unit, blueprint) =>
                unit.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, blueprint)).Data;
            try
            {
                PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 8f + Vector3.right * 5f));
                foreach (int step in R6Wait(10)) yield return step;

                foreach (int step in R6Cell("config", () => R6Configuration(symbols, record))) yield return step;

                Time.captureFramerate = 30;
                UnitEntityData hound = null;
                foreach (int step in R6Cell("hound-aid", () =>
                {
                    hound = summon("hound-archon");
                    PlaceExpandedSummoningUnit(hound, ground(centre + Vector3.right * 1.7f));
                    return R6HoundAid(hound, fixture, symbols, record, recorder, abilityData);
                })) yield return step;

                foreach (int step in R6Cell("redcap-scythe", () =>
                {
                    UnitEntityData redcap = summonRedcap();
                    PlaceExpandedSummoningUnit(redcap, ground(hostile.Position + Vector3.left * 1.6f));
                    return R6Redcap(redcap, hostile, record);
                })) yield return step;

                foreach (int step in R6Cell("lantern", () => R6Lantern(summon("lantern-archon"), hostile, symbols, record, ground, abilityData))) yield return step;

                foreach (int step in R6Cell("mixed-group", () => R6MixedGroup(hound, summon, summonRedcap, ground, record, centre))) yield return step;
                Time.captureFramerate = priorCapture;

                // Real time, last, because the screech and blast affect nearby creatures.
                foreach (int step in R6Cell("audio", () => R6Audio(fixture, summon, ground, symbols, record, recorder, saves, mark, abilityData, centre))) yield return step;
            }
            finally
            {
                Time.captureFramerate = priorCapture;
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("saves", () => { foreach (Sprint26Saves value in saves) value.Dispose(); });
                restore("recorder", () => EventBus.Unsubscribe(recorder));
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (!fixture.Hostile.Destroyed) PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("r6-owned-state-restored", errors.Count == 0 && Time.captureFramerate == priorCapture,
                    new JObject { ["errors"] = errors }, "Readers, combat, positions and the capture framerate restore.");
                record["timeline"] = timeline; record["cues"] = R6Cues(); record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }

        private static JArray R6Cues()
        {
            return new JArray(SummonAbilityCastCueRuntime.Snapshot().Select(value => new JObject { ["event"] = value.EventName, ["ability"] = value.Ability,
                ["caster"] = value.CasterId, ["utc"] = value.Utc.ToString("o"), ["frame"] = value.Frame, ["playingId"] = value.PlayingId,
                ["endUtc"] = value.EndUtc.HasValue ? value.EndUtc.Value.ToString("o") : null, ["endFrame"] = value.EndFrame,
                ["stopReason"] = value.StopReason, ["stopResult"] = value.StopResult }));
        }

        private IEnumerable<int> R6Configuration(IDictionary<string, BlueprintScriptableObject> symbols, JObject record)
        {
            var hound = (BlueprintUnit)symbols["KMG.Summoning.Unit.HoundArchon"];
            var aid = (BlueprintAbility)symbols["KMG.Summoning.OutsiderCelestial.HoundArchon.Aid"];
            var screech = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Vrock37Prefix + "Screech"];
            var blast = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast"];
            UnitAnimationManager manager = hound.Prefab.Load(false).GetComponentsInChildren<UnitAnimationManager>(true).Single();
            var cast = manager.AnimationSet.Actions.OfType<UnitAnimationActionCastSpell>().Single();
            var config = new JObject { ["set"] = manager.AnimationSet.name, ["aidStyle"] = aid.Animation.ToString(),
                ["castStyles"] = new JArray(cast.Animations.Select(entry => entry.Style + ":" + (entry.Default == null || entry.Default.CastClip == null ? "<none>" : entry.Default.CastClip.name))),
                ["screechCues"] = new JArray(screech.GetComponents<SummonAbilityCastCue>().Select(value => value.EventName)),
                ["blastCues"] = new JArray(blast.GetComponents<SummonAbilityCastCue>().Select(value => value.EventName)),
                ["r5Provider"] = R5CotwSpells.Disposition, ["r5Mist"] = R5CotwSpells.MistDisposition, ["r5Dance"] = R5CotwSpells.DanceDisposition };
            record["config"] = config;
            Sprint25Check("r6-loaded-configuration", aid.Animation == UnitAnimationActionCastSpell.CastAnimationStyle.Omni &&
                ((JArray)config["screechCues"]).Count == 1 && ((JArray)config["blastCues"]).Count == 1, (JObject)config.DeepClone(),
                "The Hound's Aid uses the target-stable Omni style and each of the two abilities carries exactly one cue.");
            yield break;
        }

        private IEnumerable<int> R6HoundAid(UnitEntityData hound, ExpandedSummoningCorrectionFixture fixture, IDictionary<string, BlueprintScriptableObject> symbols,
            JObject record, R4MephitRecorder recorder, Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData)
        {
            foreach (int step in R6Wait(R6ArrivalFrames)) yield return step;
            var aid = (BlueprintAbility)symbols["KMG.Summoning.OutsiderCelestial.HoundArchon.Aid"];
            var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(Bootstrap.BlueprintBootstrap.Library, "03a9630394d10164a9410882d31572f0", "native Aid");
            UnitAnimationManager manager = hound.View.AnimationManager;
            var cast = manager.AnimationSet.Actions.OfType<UnitAnimationActionCastSpell>().Single();
            var omni = cast.Animations.Single(entry => entry.Style == UnitAnimationActionCastSpell.CastAnimationStyle.Omni).Default;
            string[] expected = new[] { omni.PrecastStart, omni.PrecastLooped, omni.CastClip }.Where(clip => clip != null).Select(clip => clip.name).Distinct().ToArray();
            foreach (var target in new[] { new { Key = "ally", Unit = fixture.Caster }, new { Key = "self", Unit = hound } })
            {
                var row = new JObject(); record["houndAid-" + target.Key] = row;
                int before = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, aid));
                float standardBefore = hound.CombatState.Cooldown.StandardAction;
                using (var clip = new R6Clip(_request.EvidenceDirectory, "hound-aid-" + target.Key, () => R6RenderBounds(hound, target.Unit)))
                {
                    foreach (int step in R6Shots(clip, hound, 10)) yield return step;
                    foreach (int step in R6RunPassive(hound, abilityData(hound, aid), new TargetWrapper(target.Unit), row, clip)) yield return step;
                    foreach (int step in R6Shots(clip, hound, 20)) yield return step;
                    row["clips"] = R6DistinctClips(clip.Rows); row["outOfFrame"] = clip.OutOfFrame; row["occluders"] = clip.Occluded;
                    row["applied"] = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, aid)) - before;
                    row["standardCooldownBefore"] = standardBefore; row["standardCooldownAfter"] = hound.CombatState.Cooldown.StandardAction;
                    row["expectedAction"] = "Standard"; row["nativeAidAction"] = native.ActionType.ToString();
                    bool gesture = SummonPlaytestR6Policy.CastGestureObserved(R6Frames(clip.Rows), cast.name, expected, (int)row["applied"], (bool)row["cancelled"]);
                    Sprint25Check("r6-hound-aid-" + target.Key, gesture && (bool)row["finished"] && (string)row["actionType"] == "Standard" && native.ActionType.ToString() == "Standard",
                        (JObject)row.DeepClone(), "The target-resolved native cast clip plays during the actual casting interval; one application; the action stays Standard, matching the native Aid.");
                }
                foreach (int step in R6Wait(30)) yield return step;
            }
        }

        private IEnumerable<int> R6Redcap(UnitEntityData redcap, UnitEntityData hostile, JObject record)
        {
            var row = new JObject(); record["redcap"] = row;
            foreach (int step in R6Wait(R6ArrivalFrames)) yield return step;
            RedcapScytheAnimationR6 binding = redcap.View.GetComponent<RedcapScytheAnimationR6>();
            row["unit"] = redcap.Blueprint.name; row["primaryHand"] = redcap.Body.PrimaryHand.MaybeWeapon == null ? null : redcap.Body.PrimaryHand.MaybeWeapon.Blueprint.name;
            row["style"] = redcap.Body.PrimaryHand.GetWeaponStyle().ToString(); row["binding"] = binding == null ? "<absent>" : binding.Outcome; row["bindingLive"] = binding != null && binding.Live;
            row["nativeSetName"] = redcap.View.AnimationManager.AnimationSet.name;
            using (var clip = new R6Clip(_request.EvidenceDirectory, "redcap-scythe", () => R6RenderBounds(redcap)))
            using (var log = new R6AttackLog(redcap))
            using (new R5AttackSeed(redcap))
            {
                foreach (int step in R6Shots(clip, redcap, 12)) yield return step;
                var command = new UnitAttack(hostile);
                redcap.Commands.Run(command);
                int shots = 0, after = 0;
                while (shots++ < 240 && after < 30)
                {
                    clip.Shot(R6AnimationState(redcap, command));
                    if (command.IsFinished) after++;
                    yield return 0;
                }
                row["frames"] = clip.Count; row["finished"] = command.IsFinished; row["result"] = command.Result.ToString(); row["acted"] = command.IsActed;
                row["attacks"] = log.Rows; row["clips"] = R6DistinctClips(clip.Rows); row["outOfFrame"] = clip.OutOfFrame; row["occluders"] = clip.Occluded;
                int[] scytheRolls = log.Rows.Where(value => (string)value["weapon"] == "StandardScythe").Select(value => (int)value["frame"]).ToArray();
                bool swing = SummonPlaytestR6Policy.ScytheSwingObserved(R6Frames(clip.Rows), scytheRolls, 2);
                bool kick = log.Rows.Any(value => (string)value["weapon"] == "RedcapKick1d4") && ((JArray)row["clips"]).Values<string>().Any(value => value.StartsWith("LegAttack", StringComparison.Ordinal));
                Sprint25Check("r6-redcap-scythe", swing && kick && command.IsActed && (string)row["primaryHand"] == "StandardScythe" && (bool)row["bindingLive"], (JObject)row.DeepClone(),
                    "A KMG-origin summoned Redcap plays its native scythe clip at the frame its real StandardScythe attack rolls, and the kick still plays.");
            }
            redcap.Commands.InterruptAll(command => true); redcap.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(redcap, redcap.Position);
        }

        private static JArray R6NearOrbCensus(LanternArchonOrbView orb, Bounds orbBounds)
        {
            var rows = new JArray();
            foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>().Where(value => value != null && value.gameObject.scene.IsValid() && value.enabled && value.gameObject.activeInHierarchy))
            {
                Bounds box = renderer.bounds;
                if (Vector3.Distance(box.center, orbBounds.center) > 5f || orb.OwnsRenderer(renderer) || box.size.magnitude < .01f) continue;
                string material = renderer.sharedMaterial == null ? null : renderer.sharedMaterial.name;
                Transform top = renderer.transform; string path = top.name;
                for (int depth = 0; depth < 4 && top.parent != null; depth++) { top = top.parent; path = top.name + "/" + path; }
                rows.Add(new JObject { ["type"] = renderer.GetType().Name, ["path"] = path, ["material"] = material,
                    ["center"] = box.center.ToString("F2"), ["size"] = box.size.ToString("F2"),
                    ["outsideOrb"] = !orbBounds.Contains(box.min) || !orbBounds.Contains(box.max) });
            }
            return rows;
        }

        private IEnumerable<int> R6Lantern(UnitEntityData lantern, UnitEntityData hostile, IDictionary<string, BlueprintScriptableObject> symbols, JObject record,
            Func<Vector3, Vector3> ground, Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData)
        {
            PlaceExpandedSummoningUnit(lantern, ground(hostile.Position + Vector3.left * 6f));
            foreach (int step in R6Wait(R6ArrivalFrames)) yield return step;
            LanternArchonOrbView orb = lantern.View.GetComponent<LanternArchonOrbView>();
            if (orb == null) throw new InvalidOperationException("Lantern owned orb view is absent.");
            var ray = (BlueprintAbility)symbols["KMG.Summoning.Special.LanternArchon.LightRay"];
            var row = new JObject(); record["lantern"] = row;
            row["hiddenAtSpawn"] = orb.DonorBodiesHidden;
            Vector3 home = lantern.Position;
            foreach (var pass in new[] { new { Name = "donor-shown", Hidden = false }, new { Name = "donor-hidden", Hidden = true } })
            {
                orb.SetDonorBodiesHidden(pass.Hidden);
                var part = new JObject(); row[pass.Name] = part;
                PlaceExpandedSummoningUnit(lantern, home);
                foreach (int step in R6Wait(20)) yield return step;
                using (var clip = new R6Clip(_request.EvidenceDirectory, "lantern-" + pass.Name, () => R6RenderBounds(lantern)))
                {
                    foreach (int step in R6Shots(clip, lantern, 60)) yield return step;
                    part["idleCensus"] = R6NearOrbCensus(orb, orb.OrbRenderer.bounds);
                    var move = new UnitMoveTo(ground(home + Vector3.back * 5f));
                    lantern.Commands.Run(move);
                    for (int shot = 0; shot < 150 && !move.IsFinished; shot++) { clip.Shot(R6AnimationState(lantern)); yield return 0; }
                    part["moveFinished"] = move.IsFinished;
                    part["moveCensus"] = R6NearOrbCensus(orb, orb.OrbRenderer.bounds);
                    foreach (int step in R6Shots(clip, lantern, 15)) yield return step;
                    var cast = new JObject(); part["ray"] = cast;
                    foreach (int step in R6RunPassive(lantern, abilityData(lantern, ray), new TargetWrapper(hostile), cast, clip)) yield return step;
                    part["rayCensus"] = R6NearOrbCensus(orb, orb.OrbRenderer.bounds);
                    foreach (int step in R6Shots(clip, lantern, 25)) yield return step;
                    part["frames"] = clip.Count; part["outOfFrame"] = clip.OutOfFrame; part["occluders"] = clip.Occluded;
                }
                foreach (int step in R6Wait(30)) yield return step;
            }
            row["hiddenAtEnd"] = orb.DonorBodiesHidden; row["orb"] = orb.Describe();
            Sprint25Check("r6-lantern-paired-views", (bool)row["hiddenAtSpawn"] && orb.DonorBodiesHidden && orb.Ready &&
                (bool)((JObject)((JObject)row["donor-hidden"])["ray"])["finished"] && (int)((JObject)row["donor-hidden"])["outOfFrame"] == 0 && (int)((JObject)row["donor-shown"])["outOfFrame"] == 0,
                (JObject)row.DeepClone(), "The same live Lantern is framed fully in both passes, ends suppressed and ready, and its ray finishes.");
        }

        private IEnumerable<int> R6MixedGroup(UnitEntityData hound, Func<string, UnitEntityData> summon, Func<UnitEntityData> summonRedcap,
            Func<Vector3, Vector3> ground, JObject record, Vector3 centre)
        {
            Vector3 anchor = centre + Vector3.forward * 4f;
            var group = new List<UnitEntityData>(); var rows = new JArray(); record["mixedGroup"] = rows;
            Action<string, Func<UnitEntityData>> add = (key, make) =>
            {
                try
                {
                    UnitEntityData unit = make();
                    PlaceExpandedSummoningUnit(unit, ground(anchor + new Vector3(-6f + group.Count * 2f, 0f, 0f)));
                    group.Add(unit);
                }
                catch (Exception error) { rows.Add(new JObject { ["key"] = key, ["error"] = error.Message }); }
            };
            add("wolf", () => summon("wolf"));
            add("redcap", summonRedcap);
            add("fire-mephit", () => summon("fire-mephit"));
            if (hound != null) add("hound-archon", () => hound);
            add("dretch", () => summon("dretch"));
            add("shadow-demon", () => summon("shadow-demon"));
            add("invisible-stalker", () => summon("invisible-stalker"));
            foreach (int step in R6Wait(150)) yield return step;
            using (var clip = new R6Clip(_request.EvidenceDirectory, "mixed-group", () => R6RenderBounds(group.ToArray())))
            {
                foreach (int step in R6Shots(clip, null, 60)) yield return step;
                record["mixedGroupFraming"] = new JObject { ["outOfFrame"] = clip.OutOfFrame, ["occluders"] = clip.Occluded };
            }
            int genericRims = 0;
            foreach (UnitEntityData unit in group)
            {
                string census = ExpandedSummoningSummonGlowPatch.DescribeBuffEffects(unit);
                genericRims += SummonGlowPolicy.ActiveGenericRims(census);
                SummonArrivalRimLimiter limiter = unit.View.GetComponent<SummonArrivalRimLimiter>();
                rows.Add(new JObject { ["unit"] = unit.Blueprint.name, ["buffEffects"] = census, ["glow"] = ExpandedSummoningSummonGlowPatch.Describe(unit.View),
                    ["arrivalRimsEnded"] = limiter == null ? 0 : limiter.Ended, ["buffs"] = new JArray(unit.Descriptor.Buffs.RawFacts.Select(value => value.Blueprint.name)) });
            }
            bool preserved = rows.Any(value => ((string)value["buffEffects"] ?? "").Contains("MephitFireVisualBuff")) && rows.Any(value => ((string)value["buffEffects"] ?? "").Contains("NaturalInvisibilityBuff"));
            Sprint25Check("r6-mixed-group-sheen", group.Count >= 6 && genericRims == 0 && preserved && (int)record["mixedGroupFraming"]["outOfFrame"] == 0,
                new JObject { ["rows"] = rows.DeepClone(), ["genericRims"] = genericRims },
                "Six or more KMG summons are in frame after arrival, no generic rim stays active, and intrinsic effect and invisibility buffs remain.");
        }

        private IEnumerable<int> R6Audio(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, Func<Vector3, Vector3> ground,
            IDictionary<string, BlueprintScriptableObject> symbols, JObject record, R4MephitRecorder recorder, List<Sprint26Saves> saves, Action<string> mark,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, Vector3 centre)
        {
            var row = new JObject(); record["audio"] = row;
            CameraRig rig = TeleportationCastingCamera();
            UnitEntityData normal = summon("trumpet-archon"), retired = summon("trumpet-archon"), vrock = summon("vrock");
            PlaceExpandedSummoningUnit(normal, ground(centre + Vector3.right * 3f));
            PlaceExpandedSummoningUnit(retired, ground(centre + Vector3.right * 5f));
            PlaceExpandedSummoningUnit(vrock, ground(centre + Vector3.left * 3f));
            foreach (UnitEntityData unit in new[] { normal, retired, vrock }) saves.Add(new Sprint26Saves(unit) { DesiredRoll = 20 });
            if (rig != null) rig.ScrollToImmediately(centre);
            var blast = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast"];
            var screech = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Vrock37Prefix + "Screech"];
            foreach (int step in R6Seconds(4f)) yield return step;

            mark("baseline-start");
            foreach (int step in R6Seconds(3f)) yield return step;
            mark("baseline-end");

            // Cancelled before execution: no cue and no application.
            int cuesBefore = SummonAbilityCastCueRuntime.Snapshot().Length;
            int appliedBefore = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, blast));
            mark("cancel-start");
            var cancelled = new UnitUseAbility(abilityData(normal, blast), new TargetWrapper(normal));
            normal.Commands.Run(cancelled);
            yield return 0;
            normal.Commands.InterruptAll(command => true); normal.Commands.RemoveFinishedAndUpdateQueue();
            foreach (int step in R6Seconds(3f)) yield return step;
            mark("cancel-end");
            int cuesAfterCancel = SummonAbilityCastCueRuntime.Snapshot().Length;
            Sprint25Check("r6-cancelled-cast-no-cue", cuesAfterCancel == cuesBefore && recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, blast)) == appliedBefore,
                new JObject { ["cuesBefore"] = cuesBefore, ["cuesAfter"] = cuesAfterCancel }, "A cast interrupted before execution posts no cue and applies nothing.");

            // Normal completion on both abilities; one cue however many are reached.
            foreach (var entry in new[] { new { Key = "vrock", Unit = vrock, Ability = screech, Event = SummonPlaytestR6Policy.VrockScreechCue },
                new { Key = "trumpet", Unit = normal, Ability = blast, Event = SummonPlaytestR6Policy.TrumpetBlastCue } })
            {
                var cast = new JObject(); row[entry.Key] = cast;
                int before = SummonAbilityCastCueRuntime.Snapshot().Length;
                int delivered = recorder.Delivered.Count(value => ReferenceEquals(value.Key.AbilityBlueprint, entry.Ability));
                mark(entry.Key + "-cast-start");
                foreach (int step in R6RunPassive(entry.Unit, abilityData(entry.Unit, entry.Ability), new TargetWrapper(entry.Unit), cast, null)) yield return step;
                mark(entry.Key + "-cast-end");
                foreach (int step in R6Seconds(6f)) yield return step;
                SummonCastCueRecord[] added = SummonAbilityCastCueRuntime.Snapshot().Skip(before).ToArray();
                cast["cues"] = added.Length; cast["delivered"] = recorder.Delivered.Count(value => ReferenceEquals(value.Key.AbilityBlueprint, entry.Ability)) - delivered;
                bool ended = added.Length == 1 && added[0].EndUtc.HasValue;
                cast["naturalSeconds"] = ended ? (added[0].EndUtc.Value - added[0].Utc).TotalSeconds : -1d;
                Sprint25Check("r6-" + entry.Key + "-one-cue", added.Length == 1 && added[0].EventName == entry.Event && added[0].PlayingId != 0 && ended && added[0].StopReason == null && (int)cast["delivered"] >= 1,
                    (JObject)cast.DeepClone(), "One execution posts one accepted cue, reaches its creatures, and the cue completes naturally with no retirement stop.");
            }

            // Early retirement through a real execution and the product cue.
            var early = new JObject(); row["earlyRetirement"] = early;
            int cuesEarly = SummonAbilityCastCueRuntime.Snapshot().Length;
            var command2 = new UnitUseAbility(abilityData(retired, blast), new TargetWrapper(retired));
            retired.Commands.Run(command2);
            DateTime waitCue = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < waitCue && SummonAbilityCastCueRuntime.Snapshot().Length == cuesEarly) yield return 0;
            SummonCastCueRecord[] cue = SummonAbilityCastCueRuntime.Snapshot().Skip(cuesEarly).ToArray();
            if (cue.Length != 1) throw new InvalidOperationException("The early-retirement execution posted no product cue.");
            // Retire after the native command has ended, while its cue still plays.
            DateTime waitFinish = DateTime.UtcNow.AddSeconds(1.2d);
            while (DateTime.UtcNow < waitFinish && !command2.IsFinished) yield return 0;
            early["commandFinishedBeforeRetirement"] = command2.IsFinished;
            bool stillPlaying = !cue[0].EndUtc.HasValue;
            DateTime retiredAt = DateTime.UtcNow;
            mark("despawn");
            DisposeExpandedSummoningUnits(fixture.Created, new[] { retired });
            foreach (int step in R6Seconds(6f)) yield return step;
            mark("despawn-window-end");
            early["playingId"] = cue[0].PlayingId; early["stillPlayingAtRetirement"] = stillPlaying; early["stopReason"] = cue[0].StopReason; early["stopResult"] = cue[0].StopResult;
            double untilEnd = cue[0].EndUtc.HasValue ? (cue[0].EndUtc.Value - retiredAt).TotalSeconds : double.NaN;
            early["secondsFromRetirementToEnd"] = double.IsNaN(untilEnd) ? (double?)null : untilEnd;
            double natural = (double)((JObject)row["trumpet"])["naturalSeconds"];
            double total = cue[0].EndUtc.HasValue ? (cue[0].EndUtc.Value - cue[0].Utc).TotalSeconds : double.NaN;
            early["totalSeconds"] = double.IsNaN(total) ? (double?)null : total; early["naturalControlSeconds"] = natural;
            Sprint25Check("r6-cue-early-retirement", stillPlaying && !double.IsNaN(total) && untilEnd < 1.0d && natural > total + .8d, (JObject)early.DeepClone(),
                "A real execution's cue still playing when its caster retires ends within a second, clearly earlier than the same cue's natural completion.");
        }
    }
}
