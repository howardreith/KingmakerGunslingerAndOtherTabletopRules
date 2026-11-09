using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R7 (ART-01..03), presentation only and save-free. Each model
        // is cast through its normal published summon. The pre-R7 export is
        // then shown on that same live unit from a private, hash-pinned copy,
        // so before and after share the camera, zoom, light and rig. Those
        // pre-R7 bytes are evidence input only and are never packaged.
        // Commands are observed passively; no aesthetic verdict is made here.
        private const string R7BeforeRoot = "C:/Dev/KingmakerGunslingerLab/runtime-evidence/summon-playtest-r7/before-assets";
        private const string R7MainTexture = "_MainTex";

        private sealed class R7Model
        {
            internal string Key, Folder, MeshSha;
            internal string[][] Details;
        }

        // Each detail: name, bone, radius, then the view direction in the
        // unit's own frame (x right, y up, z forward).
        private static readonly R7Model[] R7Models =
        {
            new R7Model { Key = "glabrezu", Folder = "sprint38-glabrezu", MeshSha = "2122716bb703ebd1bef27723c2d2d4cb370786bc3930588ebdff4d1f21111ac6",
                Details = new[] { new[] { "arm-roots", "Torso_Upper", ".62", ".55", ".2", ".8" }, new[] { "head", "Head", ".42", ".6", ".15", ".8" } } },
            new R7Model { Key = "nalfeshnee", Folder = "sprint39-nalfeshnee", MeshSha = "1caa91e1f79f40ec7c1a0ee251783665ad6b0ff0cbb2ea6fad7ff9ce2ef2fc2e",
                Details = new[] { new[] { "head", "Head", ".52", ".6", ".15", ".8" }, new[] { "wing-roots", "Torso_Upper", ".82", ".55", ".45", "-.8" } } },
            new R7Model { Key = "ice-devil", Folder = "sprint40-ice-devil", MeshSha = "3c6e0a8ecfd17f50c3996a1ecf6813112e4248da2f5599571b0f27d10cabf753",
                Details = new[] { new[] { "head", "Head", ".42", ".55", ".12", ".82" }, new[] { "shoulder-thorax", "Torso_Upper", ".62", ".6", ".2", ".78" } } },
        };

        private static string R7FileSha(string path)
        {
            using (SHA256 sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(File.ReadAllBytes(path)).Select(value => value.ToString("x2")));
        }

        /// <summary>Shows the pinned pre-R7 export on one live view, then puts the current export back.</summary>
        private sealed class R7BodySwap : IDisposable
        {
            private readonly SkinnedMeshRenderer _renderer;
            private readonly Material _material;
            private readonly Mesh _current, _before;
            private readonly Texture _currentAlbedo;
            private readonly Texture2D _beforeAlbedo;
            internal readonly int CurrentVertices, BeforeVertices;

            internal R7BodySwap(UnitEntityData unit, R7Model model)
            {
                string owned = "KMG_" + model.Key + "_Original";
                _renderer = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .SingleOrDefault(value => value.sharedMesh != null && value.sharedMesh.name == owned);
                if (_renderer == null) throw new InvalidOperationException("Owned body renderer absent: " + owned);
                string meshPath = Path.Combine(R7BeforeRoot, model.Folder, model.Key + "-mesh.json");
                string actual = R7FileSha(meshPath);
                if (actual != model.MeshSha) throw new InvalidDataException("Pre-R7 mesh hash differs: " + actual);
                string json = File.ReadAllText(meshPath);
                string[] names;
                PteranodonAssetRuntime.AlbedoRequirement requirement;
                _before = PteranodonAssetRuntime.BuildMesh(json, out names, out requirement, InfernalVisualPolicy36.Bones);
                string reason;
                _beforeAlbedo = PteranodonAssetRuntime.LoadAlbedo(Path.GetDirectoryName(meshPath), requirement, out reason);
                if (_beforeAlbedo == null) throw new InvalidDataException("Pre-R7 albedo: " + reason);
                _current = _renderer.sharedMesh;
                if (names.Length != _renderer.bones.Length || !names.SequenceEqual(_renderer.bones.Select(value => value.name)))
                    throw new InvalidDataException("Pre-R7 bone order differs from the live renderer.");
                _before.bindposes = _current.bindposes;
                _before.name = "KMG_" + model.Key + "_R7BeforeEvidence";
                _material = _renderer.material;
                _currentAlbedo = _material.GetTexture(R7MainTexture);
                CurrentVertices = _current.vertexCount; BeforeVertices = _before.vertexCount;
            }

            internal void ShowBefore() { _renderer.sharedMesh = _before; _material.SetTexture(R7MainTexture, _beforeAlbedo); }
            internal void ShowCurrent() { _renderer.sharedMesh = _current; _material.SetTexture(R7MainTexture, _currentAlbedo); }
            internal bool CurrentShown => _renderer != null && ReferenceEquals(_renderer.sharedMesh, _current);
            internal UnityEngine.Rendering.ShadowCastingMode ShadowMode { get { return _renderer.shadowCastingMode; } set { _renderer.shadowCastingMode = value; } }

            public void Dispose()
            {
                if (_renderer != null) ShowCurrent();
                if (_before != null) UnityEngine.Object.Destroy(_before);
                if (_beforeAlbedo != null) UnityEngine.Object.Destroy(_beforeAlbedo);
            }
        }

        private static Bounds R7BoneBounds(UnitEntityData unit, string bone, float radius)
        {
            Transform found = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SelectMany(value => value.bones ?? new Transform[0])
                .FirstOrDefault(value => value != null && value.name == bone);
            Vector3 centre = found != null ? found.position : unit.Position + Vector3.up;
            float scale = unit.View.transform.lossyScale.y;
            return new Bounds(centre, Vector3.one * (radius * scale * 2f / Mathf.Sqrt(3f)));
        }

        private IEnumerable<int> R7Stills(UnitEntityData unit, R7Model model, string state, string scene, JObject row)
        {
            foreach (int step in SummonOverhaulSettlePartyCamera(unit)) yield return step;
            row["party"] = WriteExpandedSummoningPartyCameraCapture(unit, _request.EvidenceDirectory,
                "r7-" + model.Key + "-" + scene + "-" + state + "-party.png");
            Quaternion facing = unit.View.transform.rotation;
            var directions = new[] { new { Name = "front", Local = new Vector3(.55f, .32f, .78f) }, new { Name = "rear", Local = new Vector3(-.55f, .32f, -.78f) } };
            foreach (var view in directions)
                using (var clip = new R6Clip(_request.EvidenceDirectory, "r7-" + model.Key + "-" + scene + "-" + state + "-" + view.Name,
                    () => R6RenderBounds(unit), facing * view.Local.normalized))
                { clip.Shot(new JObject { ["state"] = state }); row[view.Name] = clip.OutOfFrame == 0; }
            foreach (string[] detail in model.Details)
            {
                float radius = float.Parse(detail[2], System.Globalization.CultureInfo.InvariantCulture);
                var local = new Vector3(R7Number(detail[3]), R7Number(detail[4]), R7Number(detail[5])).normalized;
                string bone = detail[1];
                using (var clip = new R6Clip(_request.EvidenceDirectory, "r7-" + model.Key + "-" + scene + "-" + state + "-detail-" + detail[0],
                    () => R7BoneBounds(unit, bone, radius), facing * local, radius * unit.View.transform.lossyScale.y))
                { clip.Shot(new JObject { ["state"] = state }); row["detail-" + detail[0]] = clip.OutOfFrame == 0; }
            }
        }

        private static float R7Number(string value) { return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); }

        private IEnumerable<int> R7Motion(UnitEntityData unit, UnitEntityData hostile, R7Model model, string state, string scene, Vector3 start, Vector3 target, Vector3 bearing, JObject row)
        {
            // One fixed bearing and torso-centred framing, so both exports are
            // filmed by the same camera path at a readable size.
            Func<Bounds> subject = () => R7BoneBounds(unit, "Torso_Upper", 1.2f);
            PlaceExpandedSummoningUnit(unit, start);
            foreach (int step in R6Wait(14)) yield return step;
            string prefix = "r7-" + model.Key + "-" + scene + "-" + state + "-";
            using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "idle", subject, bearing))
                for (int shot = 0; shot < 24; shot++)
                { clip.Shot(R6AnimationState(unit)); foreach (int step in R6Wait(2)) yield return step; }

            Vector3 before = unit.Position;
            var move = new UnitMoveTo(target, .3f); move.Init(unit);
            bool started = move.CanStart;
            float peak = 0f;
            if (started)
            {
                unit.Commands.Run(move);
                using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "move", subject, bearing))
                    for (int frame = 0; frame < 150 && !move.IsFinished; frame++)
                    {
                        peak = Math.Max(peak, unit.View.MovementAgent.Velocity.magnitude);
                        if (frame % 2 == 0) clip.Shot(R6AnimationState(unit, move));
                        yield return 0;
                    }
            }
            row["moveTravel"] = Vector3.Distance(before, unit.Position);
            row["movePeakVelocity"] = peak;
            row["moveFinished"] = move.IsFinished;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();

            if (hostile == null) yield break;
            PlaceExpandedSummoningUnit(unit, start);
            foreach (int step in R6Wait(8)) yield return step;
            hostile.Descriptor.Damage = 0;
            var attack = new UnitAttack(hostile);
            unit.Commands.Run(attack);
            using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "attack", subject, bearing))
            {
                int after = 0;
                for (int frame = 0; frame < 170 && after < 14; frame++)
                {
                    if (frame % 2 == 0) clip.Shot(R6AnimationState(unit, attack));
                    if (attack.IsFinished) after++;
                    yield return 0;
                }
                row["attackClips"] = R6DistinctClips(clip.Rows);
            }
            row["attackActed"] = attack.IsActed;
            row["attackResult"] = attack.Result.ToString();
            row["primaryHand"] = unit.Body.PrimaryHand.MaybeWeapon == null ? null : unit.Body.PrimaryHand.MaybeWeapon.Blueprint.name;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            hostile.Descriptor.Damage = 0;
        }

        private IEnumerable<int> R7Cell(ExpandedSummoningCorrectionFixture fixture, R7Model model, string scene, Vector3 start, Vector3 target,
            UnitEntityData hostile, List<UnitEntityData> owned, JObject record)
        {
            var row = new JObject(); record[model.Key] = row;
            UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, model.Key); owned.Add(unit);
            Sprint28SetBrainActivePreservingActions(unit, false);
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(unit, start);
            // Let the brief arrival effect and its three-second rim end first.
            foreach (int step in R6Wait(R6ArrivalFrames)) yield return step;
            row["unit"] = unit.Blueprint.name;
            row["rootScale"] = SurveyVector(unit.View.transform.localScale);
            row["corpulence"] = unit.Descriptor.OriginalSize.ToString();
            row["primaryHand"] = unit.Body.PrimaryHand.MaybeWeapon == null ? null : unit.Body.PrimaryHand.MaybeWeapon.Blueprint.name;
            using (var swap = new R7BodySwap(unit, model))
            {
                row["currentVertices"] = swap.CurrentVertices; row["beforeVertices"] = swap.BeforeVertices;
                var stills = new JObject(); row["stills"] = stills;
                var after = new JObject(); var before = new JObject();
                stills["after"] = after; stills["before"] = before;
                foreach (int step in R7Stills(unit, model, "after", scene, after)) yield return step;
                // Diagnostic for small bright patches in creases: the first
                // detail again, with this body casting no shadow onto itself.
                string[] first = model.Details[0];
                float firstRadius = R7Number(first[2]);
                UnityEngine.Rendering.ShadowCastingMode shadow = swap.ShadowMode;
                swap.ShadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                try
                {
                    using (var clip = new R6Clip(_request.EvidenceDirectory, "r7-" + model.Key + "-" + scene + "-diagnostic-noselfshadow-" + first[0],
                        () => R7BoneBounds(unit, first[1], firstRadius), unit.View.transform.rotation * new Vector3(R7Number(first[3]), R7Number(first[4]), R7Number(first[5])).normalized, firstRadius * unit.View.transform.lossyScale.y))
                        clip.Shot(new JObject { ["state"] = "diagnostic-no-self-shadow" });
                }
                finally { swap.ShadowMode = shadow; }
                swap.ShowBefore();
                foreach (int step in R6Wait(4)) yield return step;
                foreach (int step in R7Stills(unit, model, "before", scene, before)) yield return step;
                swap.ShowCurrent();

                var motionAfter = new JObject(); var motionBefore = new JObject();
                row["motionAfter"] = motionAfter; row["motionBefore"] = motionBefore;
                Vector3 bearing = unit.View.transform.rotation * new Vector3(.55f, .32f, .78f).normalized;
                foreach (int step in R7Motion(unit, hostile, model, "after", scene, start, target, bearing, motionAfter)) yield return step;
                swap.ShowBefore();
                foreach (int step in R7Motion(unit, hostile, model, "before", scene, start, target, bearing, motionBefore)) yield return step;
                swap.ShowCurrent();
                bool shown = swap.CurrentShown;
                bool stillsFramed = new[] { after, before }.All(values => values.Properties().Where(value => value.Name != "party").All(value => (bool)value.Value));
                bool partyFramed = new[] { after, before }.All(values => ((string)values["party"]).Contains("inFrame=true"));
                bool moved = (float)motionAfter["moveTravel"] > 1f && (float)motionBefore["moveTravel"] > 1f;
                bool attacked = hostile == null || ((bool)motionAfter["attackActed"] && (bool)motionBefore["attackActed"]);
                bool different = swap.CurrentVertices != swap.BeforeVertices;
                bool spear = model.Key != "ice-devil" || !string.IsNullOrEmpty((string)row["primaryHand"]);
                Sprint25Check("r7-" + model.Key + "-" + scene, shown && stillsFramed && partyFramed && moved && attacked && different && spear, (JObject)row.DeepClone(),
                    "The published summon shows the R7 export; the pinned pre-R7 export is shown on the same live unit; both are framed in stills and both complete the observed movement and any attack.");
            }
        }

        private IEnumerable<int> ReviewSummonPlaytestModelsR7(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            bool outdoor = phase == ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestModelsPhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning ||
                (!outdoor && phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestModelsInteriorPhase) ||
                (outdoor && (fixture.Hostile == null || mode == null || !mode.Current || !Cleanup2NoNarrativeOverlay())))
                throw new InvalidOperationException("Exact fresh R7 model-review fixture with the summoning module enabled required.");
            string scene = outdoor ? "bright" : "dim";
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId, ["scene"] = scene,
                ["area"] = Game.Instance.CurrentlyLoadedArea?.name,
                ["limits"] = "Commanded motion with brains inactive. Images are presentation evidence only; aesthetic approval is the owner's." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = outdoor ? fixture.Hostile : null;
            Vector3? hostileHome = hostile?.Position;
            int? hostileHitPoints = hostile == null ? (int?)null : hostile.Descriptor.Stats.HitPoints.BaseValue;
            if (hostile != null) hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            int priorCapture = Time.captureFramerate;
            Func<Vector3, Vector3> ground = value => ObstacleAnalyzer.GetNearestNode(value).clampedPosition;
            try
            {
                Vector3 anchor;
                if (outdoor)
                {
                    PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 4f + Vector3.left * 2f));
                    foreach (int step in R6Wait(10)) yield return step;
                    anchor = hostile.Position + Vector3.left * 2.3f;
                }
                else anchor = centre + Vector3.forward * 3f;
                Vector3 start = ground(anchor);
                Vector3 target = ground(start + (outdoor ? Vector3.back : Vector3.right) * 3f);
                Time.captureFramerate = 30;
                foreach (R7Model model in R7Models)
                {
                    R7Model current = model;
                    foreach (int step in R6Cell("r7-" + current.Key, () => R7Cell(fixture, current, scene, start, target, hostile, owned, record))) yield return step;
                    foreach (UnitEntityData unit in owned.Where(value => !value.Destroyed).ToArray())
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { unit });
                    }
                    foreach (int step in R6Wait(6)) yield return step;
                }
            }
            finally
            {
                Time.captureFramerate = priorCapture;
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (hostileHome.HasValue && !fixture.Hostile.Destroyed) { fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = hostileHitPoints.Value; fixture.Hostile.Descriptor.Damage = 0; PlaceExpandedSummoningUnit(fixture.Hostile, hostileHome.Value); } });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("r7-owned-state-restored", errors.Count == 0 && Time.captureFramerate == priorCapture,
                    new JObject { ["errors"] = errors }, "Commands, combat, positions and the capture framerate restore.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }
    }
}
