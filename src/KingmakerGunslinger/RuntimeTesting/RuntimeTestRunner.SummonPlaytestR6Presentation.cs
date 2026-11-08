using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.View.Animation;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R6 (PRES-01..07), one save-free phase in the established Oleg
        // outdoor native Default fixture. Commanded executions with brains
        // inactive; nothing here observes autonomous AI. Motion frames are
        // rendered through the game's camera and written with their native
        // animation state; a separate process records device audio against
        // the UTC marks in the timeline. Records support, and never replace,
        // watching the clips and listening to the audio.
        private const int R6FrameWidth = 960, R6FrameHeight = 540;

        private sealed class R6Clip : IDisposable
        {
            // The compile-time references omit this engine module; the game ships it.
            private static readonly System.Reflection.MethodInfo EncodeJpg =
                Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", true)
                    .GetMethod("EncodeToJPG", new[] { typeof(Texture2D), typeof(int) });
            private readonly string _directory; private readonly CameraRig _rig; private readonly Camera _camera;
            private readonly RenderTexture _texture; private readonly Texture2D _pixels;
            internal readonly JArray Rows = new JArray(); internal readonly string Name; internal int Count;
            internal R6Clip(string evidence, string name)
            {
                Name = name; _directory = Path.Combine(evidence, "r6-frames", name); Directory.CreateDirectory(_directory);
                _rig = TeleportationCastingCamera(); _camera = _rig == null ? Camera.main : _rig.Camera;
                if (_camera == null) throw new InvalidOperationException("No game camera for motion capture.");
                _texture = new RenderTexture(R6FrameWidth, R6FrameHeight, 24, RenderTextureFormat.ARGB32);
                _pixels = new Texture2D(R6FrameWidth, R6FrameHeight, TextureFormat.RGB24, false, false);
            }

            internal void Shot(Vector3 focus, Vector3 offset, JObject state)
            {
                Vector3 position = _camera.transform.position; Quaternion rotation = _camera.transform.rotation;
                RenderTexture target = _camera.targetTexture, active = RenderTexture.active;
                try
                {
                    _camera.transform.position = focus + offset;
                    _camera.transform.rotation = Quaternion.LookRotation(focus + Vector3.up - _camera.transform.position, Vector3.up);
                    _camera.targetTexture = _texture; _camera.Render();
                    RenderTexture.active = _texture;
                    _pixels.ReadPixels(new Rect(0, 0, R6FrameWidth, R6FrameHeight), 0, 0); _pixels.Apply(false, false);
                    string file = Count.ToString("D4", CultureInfo.InvariantCulture) + ".jpg";
                    File.WriteAllBytes(Path.Combine(_directory, file), (byte[])EncodeJpg.Invoke(null, new object[] { _pixels, 88 }));
                    state["file"] = file; state["frame"] = Time.frameCount; state["utc"] = DateTime.UtcNow.ToString("o");
                    Rows.Add(state); Count++;
                }
                finally
                {
                    _camera.targetTexture = target; RenderTexture.active = active;
                    _camera.transform.position = position; _camera.transform.rotation = rotation;
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

        private static JObject R6AnimationState(UnitEntityData unit)
        {
            var state = new JObject();
            UnitAnimationManager manager = unit == null || unit.View == null ? null : unit.View.AnimationManager;
            var handle = manager == null ? null : manager.CurrentAction;
            state["action"] = handle == null || handle.Action == null ? null : handle.Action.name;
            AnimationClip clip = handle == null || handle.ActiveAnimation == null ? null : handle.ActiveAnimation.GetPlayableClip();
            state["clip"] = clip == null ? null : clip.name;
            state["time"] = handle == null ? 0d : Math.Round(handle.GetTime(), 3);
            return state;
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

        private IEnumerable<int> R6Frames(R6Clip clip, UnitEntityData unit, Vector3 focus, Vector3 offset, int count)
        {
            for (int index = 0; index < count; index++)
            {
                clip.Shot(unit == null ? focus : unit.Position, offset, R6AnimationState(unit));
                yield return 0;
            }
        }

        private IEnumerable<int> R6Attack(string clipName, UnitEntityData attacker, UnitEntityData target, Vector3 offset, JObject row)
        {
            using (var clip = new R6Clip(_request.EvidenceDirectory, clipName))
            using (var log = new R6AttackLog(attacker))
            using (new R5AttackSeed(attacker))
            {
                foreach (int frame in R6Frames(clip, attacker, Vector3.zero, offset, 12)) yield return frame;
                var command = new UnitAttack(target);
                attacker.Commands.Run(command);
                int actedAt = -1, shots = 0, after = 0;
                while (shots++ < 180 && after < 30)
                {
                    if (actedAt < 0 && command.IsActed) actedAt = clip.Count;
                    var state = R6AnimationState(attacker); state["acted"] = command.IsActed;
                    clip.Shot(attacker.Position, offset, state);
                    if (command.IsFinished) after++;
                    yield return 0;
                }
                row["frames"] = clip.Count; row["actedAtShot"] = actedAt; row["finished"] = command.IsFinished;
                row["attacks"] = log.Rows; row["clips"] = R6DistinctClips(clip.Rows);
            }
        }

        private static JArray R6DistinctClips(JArray rows)
        {
            return new JArray(rows.Select(value => (string)value["clip"]).Where(value => value != null).Distinct());
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
                ["limits"] = "Commanded executions with brains inactive; no autonomous AI is observed. Frames show motion and event records show timing; audibility is established only by listening to the separately recorded audio." };
            var timeline = new JArray();
            Action<string> mark = name => timeline.Add(new JObject { ["mark"] = name, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = fixture.Hostile;
            Vector3 hostilePosition = hostile.Position;
            int priorCapture = Time.captureFramerate;
            R4MephitRecorder recorder = new R4MephitRecorder(); EventBus.Subscribe(recorder);
            Sprint26Saves saves = new Sprint26Saves(hostile) { DesiredRoll = 20 };
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
                PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 7f));
                for (int frame = 0; frame < 10; frame++) yield return 0;

                // 1. Loaded configuration and the Lantern render hierarchy.
                foreach (int step in R6Cell("config", () => R6Configuration(symbols, record))) yield return step;

                Time.captureFramerate = 30;
                UnitEntityData hound = null;
                // 2. PRES-01: Hound Archon greatsword attack.
                foreach (int step in R6Cell("hound-sword", () =>
                {
                    hound = summon("hound-archon");
                    PlaceExpandedSummoningUnit(hound, ground(hostile.Position + Vector3.left * 1.6f));
                    var row = new JObject(); record["houndSword"] = row;
                    return R6AfterSettling(R6Attack("hound-sword", hound, hostile, new Vector3(3.4f, 2.6f, -3.4f), row).Concat(R6Assert(() =>
                    {
                        var clips = (JArray)row["clips"]; var attacks = (JArray)row["attacks"];
                        Sprint25Check("r6-hound-sword-swing", clips.Values<string>().Any(value => value == "Attack_01_3") && (int)row["actedAtShot"] >= 0 &&
                            attacks.Count >= 1 && (string)attacks[0]["weapon"] == hound.Blueprint.Body.PrimaryHand.name, (JObject)row.DeepClone(),
                            "The native main-hand clip plays through a real command that acts, and the first attack roll uses the Hound's greatsword.");
                    })));
                })) yield return step;

                // 3. PRES-02: Hound Archon Aid.
                foreach (int step in R6Cell("hound-aid", () =>
                {
                    if (hound == null) throw new InvalidOperationException("No Hound from the previous cell.");
                    PlaceExpandedSummoningUnit(hound, ground(fixture.Caster.Position + Vector3.right * 1.6f));
                    var aid = (BlueprintAbility)symbols["KMG.Summoning.OutsiderCelestial.HoundArchon.Aid"];
                    return R6AfterSettling(R6Cast("hound-aid", hound, abilityData(hound, aid), new TargetWrapper(fixture.Caster), new Vector3(3.4f, 2.6f, -3.4f), record, recorder, "houndAid"));
                })) yield return step;

                // 4. PRES-03: native Redcap through its real KMG summon option.
                foreach (int step in R6Cell("redcap-scythe", () =>
                {
                    UnitEntityData redcap = summonRedcap();
                    PlaceExpandedSummoningUnit(redcap, ground(hostile.Position + Vector3.right * 1.6f));
                    var row = new JObject(); record["redcap"] = row;
                    row["unit"] = redcap.Blueprint.name; row["primaryHand"] = redcap.Body.PrimaryHand.MaybeWeapon == null ? null : redcap.Body.PrimaryHand.MaybeWeapon.Blueprint.name;
                    row["renderers"] = new JArray(redcap.View.GetComponentsInChildren<Renderer>(true).Select(value =>
                        new JObject { ["name"] = value.name, ["type"] = value.GetType().Name, ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy }));
                    return R6AfterSettling(R6Attack("redcap-scythe", redcap, hostile, new Vector3(-3.2f, 2.4f, -3.2f), row).Concat(R6Assert(() =>
                    {
                        var clips = (JArray)row["clips"];
                        Sprint25Check("r6-redcap-scythe", (string)row["primaryHand"] == "StandardScythe" &&
                            clips.Values<string>().Any(value => value.StartsWith("Attack_with_scythe", StringComparison.Ordinal)), (JObject)row.DeepClone(),
                            "The summoned native Redcap wields its StandardScythe and plays its native scythe attack clip.");
                    })));
                })) yield return step;

                // 5. PRES-04: Lantern Archon, donor body shown for comparison, then fixed.
                foreach (int step in R6Cell("lantern", () => R6AfterSettling(R6Lantern(summon("lantern-archon"), hostile, symbols, record, ground, abilityData)))) yield return step;

                // 6. PRES-07: a small mixed group.
                foreach (int step in R6Cell("mixed-group", () => R6AfterSettling(R6MixedGroup(fixture, hound, summon, summonRedcap, ground, record, centre)))) yield return step;
                Time.captureFramerate = priorCapture;

                // 7. PRES-05/06: real-time audio, last, because the blast affects nearby creatures.
                foreach (int step in R6Cell("audio", () => R6Audio(fixture, summon, ground, symbols, record, recorder, mark, abilityData, centre))) yield return step;
            }
            finally
            {
                Time.captureFramerate = priorCapture;
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("saves", () => saves.Dispose());
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

        private IEnumerable<int> R6AfterSettling(IEnumerable<int> body)
        {
            for (int frame = 0; frame < 12; frame++) yield return 0;
            foreach (int frame in body) yield return frame;
        }

        private static IEnumerable<int> R6Assert(Action assertion) { assertion(); yield break; }

        private static JArray R6Cues()
        {
            return new JArray(SummonAbilityCastCueRuntime.Snapshot().Select(value => new JObject { ["event"] = value.EventName, ["ability"] = value.Ability,
                ["caster"] = value.CasterId, ["utc"] = value.Utc.ToString("o"), ["frame"] = value.Frame, ["playingId"] = value.PlayingId }));
        }

        private IEnumerable<int> R6Configuration(IDictionary<string, BlueprintScriptableObject> symbols, JObject record)
        {
            var hound = (BlueprintUnit)symbols["KMG.Summoning.Unit.HoundArchon"];
            var sword = (BlueprintItemWeapon)hound.Body.PrimaryHand;
            var aid = (BlueprintAbility)symbols["KMG.Summoning.OutsiderCelestial.HoundArchon.Aid"];
            var screech = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Vrock37Prefix + "Screech"];
            var blast = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast"];
            UnitAnimationManager manager = hound.Prefab.Load(false).GetComponentsInChildren<UnitAnimationManager>(true).Single();
            var cast = manager.AnimationSet.Actions.OfType<UnitAnimationActionCastSpell>().Single();
            var config = new JObject {
                ["set"] = manager.AnimationSet.name,
                ["swordStyle"] = sword.VisualParameters.AnimStyle.ToString(),
                ["aidStyle"] = aid.Animation.ToString(),
                ["castStyles"] = new JArray(cast.Animations.Select(entry => entry.Style + ":" + (entry.Default == null || entry.Default.CastClip == null ? "<none>" : entry.Default.CastClip.name))),
                ["actions"] = new JArray(manager.AnimationSet.Actions.Where(value => value != null).Select(value => value.GetType().Name + ":" + value.name)),
                ["screechCues"] = new JArray(screech.GetComponents<SummonAbilityCastCue>().Select(value => value.EventName)),
                ["blastCues"] = new JArray(blast.GetComponents<SummonAbilityCastCue>().Select(value => value.EventName)),
                // Incidental R5 binding state on this candidate: not a rerun of R5.
                ["r5Provider"] = R5CotwSpells.Disposition, ["r5Mist"] = R5CotwSpells.MistDisposition, ["r5Dance"] = R5CotwSpells.DanceDisposition };
            record["config"] = config;
            Sprint25Check("r6-loaded-configuration", sword.VisualParameters.AnimStyle == WeaponAnimationStyle.ThrownStraight && aid.Animation != UnitAnimationActionCastSpell.CastAnimationStyle.Immediate &&
                ((JArray)config["screechCues"]).Count == 1 && ((JArray)config["blastCues"]).Count == 1, (JObject)config.DeepClone(),
                "The Hound's sword maps the rig's main-hand clip, its Aid uses a playable cast style, and each of the two abilities carries exactly one cue.");
            yield break;
        }

        private IEnumerable<int> R6Cast(string clipName, UnitEntityData unit, AbilityData data, TargetWrapper target, Vector3 offset, JObject record, R4MephitRecorder recorder, string key)
        {
            var row = new JObject(); record[key] = row;
            int applied = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, data.Blueprint));
            using (var clip = new R6Clip(_request.EvidenceDirectory, clipName))
            {
                foreach (int frame in R6Frames(clip, unit, Vector3.zero, offset, 10)) yield return frame;
                foreach (int frame in R5RunData(unit, data, target, row))
                {
                    clip.Shot(unit.Position, offset, R6AnimationState(unit));
                    yield return frame;
                }
                foreach (int frame in R6Frames(clip, unit, Vector3.zero, offset, 24)) yield return frame;
                row["frames"] = clip.Count; row["clips"] = R6DistinctClips(clip.Rows);
            }
            row["applied"] = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, data.Blueprint)) - applied;
            Sprint25Check("r6-" + clipName + "-cast", (int)row["applied"] == 1 && ((JArray)row["clips"]).Count > 0 && (string)row["actionType"] == data.Blueprint.ActionType.ToString(),
                (JObject)row.DeepClone(), "One real native execution applied once with a native cast clip playing.");
        }

        private IEnumerable<int> R6Lantern(UnitEntityData lantern, UnitEntityData hostile, IDictionary<string, BlueprintScriptableObject> symbols, JObject record,
            Func<Vector3, Vector3> ground, Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData)
        {
            PlaceExpandedSummoningUnit(lantern, ground(hostile.Position + Vector3.back * 5f));
            for (int frame = 0; frame < 20; frame++) yield return 0;
            LanternArchonOrbView orb = lantern.View.GetComponent<LanternArchonOrbView>();
            if (orb == null) throw new InvalidOperationException("Lantern owned orb view is absent.");
            var offset = new Vector3(2.2f, 1.7f, -2.2f);
            var row = new JObject(); record["lantern"] = row;
            row["renderers"] = new JArray(lantern.View.GetComponentsInChildren<Renderer>(true).Where(value => !orb.OwnsRenderer(value)).Select(value => new JObject {
                ["name"] = value.name, ["type"] = value.GetType().Name, ["enabled"] = value.enabled,
                ["mesh"] = value is SkinnedMeshRenderer ? (((SkinnedMeshRenderer)value).sharedMesh == null ? null : ((SkinnedMeshRenderer)value).sharedMesh.name) : null,
                ["materials"] = new JArray(value.sharedMaterials.Where(material => material != null).Select(material => material.name)) }));
            row["hiddenAtSpawn"] = orb.DonorBodiesHidden;
            using (var before = new R6Clip(_request.EvidenceDirectory, "lantern-donor-body-shown"))
            {
                orb.SetDonorBodiesHidden(false);
                foreach (int frame in R6Frames(before, lantern, Vector3.zero, offset, 75)) yield return frame;
                orb.SetDonorBodiesHidden(true);
            }
            using (var after = new R6Clip(_request.EvidenceDirectory, "lantern-idle-move-cast"))
            {
                foreach (int frame in R6Frames(after, lantern, Vector3.zero, offset, 75)) yield return frame;
                var move = new UnitMoveTo(ground(lantern.Position + Vector3.right * 6f));
                lantern.Commands.Run(move);
                for (int shot = 0; shot < 120 && !move.IsFinished; shot++)
                {
                    after.Shot(lantern.Position, offset, R6AnimationState(lantern));
                    yield return 0;
                }
                row["moveFinished"] = move.IsFinished;
                foreach (int frame in R6Frames(after, lantern, Vector3.zero, offset, 15)) yield return frame;
                var ray = (BlueprintAbility)symbols["KMG.Summoning.Special.LanternArchon.LightRay"];
                var cast = new JObject(); row["ray"] = cast;
                foreach (int frame in R5RunData(lantern, abilityData(lantern, ray), new TargetWrapper(hostile), cast))
                {
                    after.Shot(lantern.Position, offset, R6AnimationState(lantern));
                    yield return frame;
                }
                foreach (int frame in R6Frames(after, lantern, Vector3.zero, offset, 30)) yield return frame;
            }
            row["hiddenAtEnd"] = orb.DonorBodiesHidden; row["orb"] = orb.Describe();
            Sprint25Check("r6-lantern-donor-body-replaced", (bool)row["hiddenAtSpawn"] && (bool)row["hiddenAtEnd"] && orb.Ready && (bool)((JObject)row["ray"])["finished"], (JObject)row.DeepClone(),
                "The donor body is replaced on this view at spawn and after the sequence, the owned orb stays ready and the ray finishes.");
        }

        private IEnumerable<int> R6MixedGroup(ExpandedSummoningCorrectionFixture fixture, UnitEntityData hound, Func<string, UnitEntityData> summon, Func<UnitEntityData> summonRedcap,
            Func<Vector3, Vector3> ground, JObject record, Vector3 centre)
        {
            Vector3 anchor = ground(centre + Vector3.back * 6f);
            var group = new List<UnitEntityData>(); var rows = new JArray(); record["mixedGroup"] = rows;
            Action<string, Func<UnitEntityData>> add = (key, make) =>
            {
                try
                {
                    UnitEntityData unit = make();
                    PlaceExpandedSummoningUnit(unit, ground(anchor + new Vector3(-5f + group.Count * 2f, 0f, 0f)));
                    group.Add(unit);
                }
                catch (Exception error) { rows.Add(new JObject { ["key"] = key, ["error"] = error.Message }); }
            };
            add("wolf", () => summon("wolf"));
            add("redcap", summonRedcap);
            add("lion", () => summon("lion"));
            add("fire-mephit", () => summon("fire-mephit"));
            if (hound != null) add("hound-archon", () => hound);
            add("shadow-demon", () => summon("shadow-demon"));
            add("invisible-stalker", () => summon("invisible-stalker"));
            for (int frame = 0; frame < 45; frame++) yield return 0;
            using (var clip = new R6Clip(_request.EvidenceDirectory, "mixed-group"))
                foreach (int frame in R6Frames(clip, null, anchor, new Vector3(0f, 7.5f, -9.5f), 60)) yield return frame;
            foreach (UnitEntityData unit in group)
                rows.Add(new JObject { ["unit"] = unit.Blueprint.name,
                    ["buffs"] = new JArray(unit.Descriptor.Buffs.RawFacts.Select(value => value.Blueprint.name)),
                    ["glow"] = ExpandedSummoningSummonGlowPatch.Describe(unit.View),
                    ["buffEffects"] = ExpandedSummoningSummonGlowPatch.DescribeBuffEffects(unit),
                    ["rim"] = ExpandedSummoningVisualVariantPatch.RimFor(unit.View).HasValue });
            Sprint25Check("r6-mixed-group-recorded", group.Count >= 5, new JObject { ["rows"] = rows.DeepClone() },
                "At least five distinct summons are present for the group view with their effect census.");
        }

        private IEnumerable<int> R6Audio(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, Func<Vector3, Vector3> ground,
            IDictionary<string, BlueprintScriptableObject> symbols, JObject record, R4MephitRecorder recorder, Action<string> mark,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, Vector3 centre)
        {
            var row = new JObject(); record["audio"] = row;
            CameraRig rig = TeleportationCastingCamera();
            UnitEntityData trumpet = summon("trumpet-archon");
            PlaceExpandedSummoningUnit(trumpet, ground(centre + Vector3.right * 3f));
            UnitEntityData vrock = summon("vrock");
            PlaceExpandedSummoningUnit(vrock, ground(centre + Vector3.left * 3f));
            if (rig != null) rig.ScrollToImmediately(centre);
            var blast = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast"];
            var screech = (BlueprintAbility)symbols[ExpandedSummoningSpecialBuilder.Vrock37Prefix + "Screech"];
            Func<IEnumerable<int>> pause = () => R6Seconds(1.5f);

            mark("baseline-start");
            foreach (int frame in R6Seconds(4f)) yield return frame;
            mark("baseline-end");

            // Cancelled before execution: no cue and no application.
            int cuesBefore = SummonAbilityCastCueRuntime.Snapshot().Length;
            int appliedBefore = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, blast));
            mark("cancel-start");
            var cancelled = new UnitUseAbility(abilityData(trumpet, blast), new TargetWrapper(trumpet));
            trumpet.Commands.Run(cancelled);
            yield return 0;
            trumpet.Commands.InterruptAll(command => true); trumpet.Commands.RemoveFinishedAndUpdateQueue();
            foreach (int frame in R6Seconds(3f)) yield return frame;
            mark("cancel-end");
            int cuesAfterCancel = SummonAbilityCastCueRuntime.Snapshot().Length;
            int appliedAfterCancel = recorder.Applied.Count(value => ReferenceEquals(value.AbilityBlueprint, blast));
            Sprint25Check("r6-cancelled-cast-no-cue", cuesAfterCancel == cuesBefore && appliedAfterCancel == appliedBefore,
                new JObject { ["cuesBefore"] = cuesBefore, ["cuesAfter"] = cuesAfterCancel, ["applied"] = appliedAfterCancel - appliedBefore },
                "A cast interrupted before execution posts no cue and applies nothing.");

            // Real executions: one cue each, however many creatures are affected.
            foreach (var entry in new[] { new { Key = "trumpet", Unit = trumpet, Ability = blast, Event = SummonPlaytestR6Policy.TrumpetBlastCue },
                new { Key = "vrock", Unit = vrock, Ability = screech, Event = SummonPlaytestR6Policy.VrockScreechCue } })
            {
                var cast = new JObject(); row[entry.Key] = cast;
                int before = SummonAbilityCastCueRuntime.Snapshot().Length;
                int delivered = recorder.Delivered.Count(value => ReferenceEquals(value.Key.AbilityBlueprint, entry.Ability));
                mark(entry.Key + "-cast-start");
                TargetWrapper self = new TargetWrapper(entry.Unit);
                foreach (int frame in R5RunData(entry.Unit, abilityData(entry.Unit, entry.Ability), self, cast)) yield return frame;
                mark(entry.Key + "-cast-end");
                foreach (int frame in R6Seconds(5f)) yield return frame;
                SummonCastCueRecord[] added = SummonAbilityCastCueRuntime.Snapshot().Skip(before).ToArray();
                cast["cues"] = added.Length; cast["delivered"] = recorder.Delivered.Count(value => ReferenceEquals(value.Key.AbilityBlueprint, entry.Ability)) - delivered;
                Sprint25Check("r6-" + entry.Key + "-one-cue", added.Length == 1 && added[0].EventName == entry.Event && added[0].PlayingId != 0 && (int)cast["delivered"] >= 1, (JObject)cast.DeepClone(),
                    "One execution posts exactly one accepted cue, regardless of how many creatures it reaches.");
            }

            // Owner listening samples; the product uses only the two cues above.
            foreach (string sample in new[] { "InspireGreatness", "CacophonousCall", "CacophonousCall_v02", "AoE_Sonic_Large" })
            {
                mark("sample-" + sample);
                AkSoundEngine.PostEvent(sample, trumpet.View.gameObject);
                foreach (int frame in R6Seconds(5.5f)) yield return frame;
            }

            // Despawn while a cue plays: the sound must end with its emitter.
            mark("despawn-cue-start");
            AkSoundEngine.PostEvent(SummonPlaytestR6Policy.VrockScreechCue, vrock.View.gameObject);
            foreach (int frame in R6Seconds(.4f)) yield return frame;
            mark("despawn");
            DisposeExpandedSummoningUnits(fixture.Created, new[] { vrock });
            foreach (int frame in pause()) yield return frame;
            foreach (int frame in R6Seconds(3f)) yield return frame;
            mark("despawn-window-end");
        }

        private static IEnumerable<int> R6Seconds(float seconds)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end) yield return 0;
        }
    }
}
