using System;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private UnityEngine.Object[] _snakeCrowdOwned;
        private string[] _snakeCrowdMeshNames;

        private bool RestoreSprint17SnakeCrowdAwake(string key, out string evidence)
        {
            var live = Game.Instance.State.AwakeUnits;
            var nativeAfter = live.ToArray();
            string disposition;
            bool restored = SerpentineCrowdReviewPolicy.RestoreOwnedAwake(key, live,
                _creatureReviewCrowdAwakeBefore, _creatureReviewUnits, unit =>
                    !unit.Destroyed && unit.View != null &&
                    ReferenceEquals(unit.HoldingState, _creatureReviewCaster.HoldingState), out disposition);
            Func<UnitEntityData[], string> ids = units => string.Join(",", units.Select(unit =>
                unit == null ? "<null>" : unit.UniqueId).ToArray());
            evidence = "disposition=" + disposition + ";before=" + ids(_creatureReviewCrowdAwakeBefore) +
                ";nativeAfter=" + ids(nativeAfter) + ";restored=" + ids(live.ToArray()) +
                ";owned=" + ids(_creatureReviewUnits);
            return restored;
        }

        // The existing guarded direct/quantity review owns every unit here.
        // Do not bind a mesh, force visibility, drive a pose or repair a view.
        private void RecordSprint17SnakeCrowdOriginals(string key)
        {
            var attachments = _creatureReviewUnits.Select(unit => unit.View == null ? null :
                unit.View.GetComponent<SerpentineVisualAttachment>()).ToArray();
            _snakeCrowdOwned = attachments.Where(value => value != null)
                .SelectMany(value => value.CaptureOwnedResources().Concat(new UnityEngine.Object[] { value }))
                .Distinct().ToArray();
            _snakeCrowdMeshNames = attachments.Where(value => value != null && value.OriginalBodyLive)
                .Select(value => value.Body.sharedMesh.name).ToArray();
            bool original = attachments.Length > 0 &&
                _snakeCrowdMeshNames.Length == attachments.Length &&
                _snakeCrowdMeshNames.Distinct(StringComparer.Ordinal).Count() == attachments.Length &&
                attachments.All(value => value != null && value.OriginalBodyLive &&
                    SerpentineVisualPolicy.IsSnakeInstanceResource(key, value.Body.sharedMesh.name,
                        value.Body.sharedMesh.name) &&
                    value.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(skin =>
                        skin == value.Body || skin.sharedMesh == null || skin.sharedMesh.vertexCount == 0));
            _creatureReviewAssertions.Add(Assertion("expanded-summoning-original-view-" + key,
                "every owned summon retains its unique original body and suppresses native auxiliary geometry",
                "count=" + attachments.Length + ";originals=" + _snakeCrowdMeshNames.Length +
                    ";ownedResources=" + _snakeCrowdOwned.Length + ";valid=" + original,
                original && _snakeCrowdOwned.Length >= attachments.Length * 5,
                "read-only exact live attachment, original mesh identity and native renderer census"));
        }

        private void RecordSprint17SnakeCrowdDestruction(string key)
        {
            bool captured = _snakeCrowdOwned != null && _snakeCrowdOwned.Length > 0 &&
                _snakeCrowdMeshNames != null && _snakeCrowdMeshNames.Length == _creatureReviewUnits.Length;
            int retained = _snakeCrowdOwned == null ? -1 : _snakeCrowdOwned.Count(value => value != null);
            UnityEngine.Object[] resources = Resources.FindObjectsOfTypeAll<Mesh>().Cast<UnityEngine.Object>()
                .Concat(Resources.FindObjectsOfTypeAll<Material>())
                .Concat(Resources.FindObjectsOfTypeAll<Texture2D>()).ToArray();
            int lateClones = _snakeCrowdMeshNames == null ? -1 : resources.Count(value => value != null &&
                _snakeCrowdMeshNames.Any(name =>
                    SerpentineVisualPolicy.IsSnakeInstanceResource(key, name, value.name)));
            _creatureReviewAssertions.Add(Assertion("expanded-summoning-" + key + "-owned-view-resources",
                "all exact captured components/meshes/materials/textures and later instance-owned clones are destroyed",
                "captured=" + captured + ";retained=" + retained + ";instanceResources=" + lateClones,
                captured && retained == 0 && lateClones == 0,
                "native destruction only; no release/dispose callback invoked to repair cleanup; foreign instances excluded"));
            _snakeCrowdOwned = null;
            _snakeCrowdMeshNames = null;
        }
    }
}
