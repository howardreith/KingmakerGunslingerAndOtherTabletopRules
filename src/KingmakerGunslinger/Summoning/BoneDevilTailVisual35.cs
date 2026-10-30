using System;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Deforms only the private Bone Devil tail region toward the actual target
    /// during its native sting. It creates no command, reach, hit, or damage.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class BoneDevilTailVisual35 : MonoBehaviour
    {
        internal sealed class Tail
        {
            internal int Bone;
            internal Vector3 Origin;
            internal Vector3 Tip;
            internal int Segments;
            internal int[] Vertices;

            internal Tail Copy()
            {
                return new Tail { Bone = Bone, Origin = Origin, Tip = Tip,
                    Segments = Segments,
                    Vertices = Vertices == null ? null :
                        (int[])Vertices.Clone() };
            }

            internal static Tail Read(JToken payload, Mesh mesh,
                string[] bones)
            {
                var result = new Tail {
                    Bone = Array.IndexOf(bones, (string)payload["bone"]),
                    Origin = Vector(payload["origin"]),
                    Tip = Vector(payload["tip"]),
                    Segments = (int)payload["segments"],
                    Vertices = payload["vertices"].Values<int>().ToArray()
                };
                BoneWeight[] weights = mesh.boneWeights;
                if (result.Bone < 0 || result.Segments != 6 ||
                    (result.Tip - result.Origin).sqrMagnitude < .25f ||
                    result.Vertices.Length < 100 ||
                    result.Vertices.Distinct().Count() !=
                        result.Vertices.Length ||
                    result.Vertices.Any(index => index < 0 ||
                        index >= weights.Length ||
                        weights[index].boneIndex0 != result.Bone ||
                        Mathf.Abs(weights[index].weight0 - 1f) > .0001f))
                    throw new InvalidOperationException(
                        "Bone Devil tail must be one six-segment, pelvis-owned region.");
                return result;
            }

            private static Vector3 Vector(JToken row)
            {
                var values = row.Values<float>().ToArray();
                if (values.Length != 3 || values.Any(value =>
                    float.IsNaN(value) || float.IsInfinity(value)))
                    throw new InvalidOperationException(
                        "Nonfinite authored Bone Devil tail landmark.");
                return new Vector3(values[0], values[1], values[2]);
            }
        }

        private UnitEntityData _owner;
        private SkinnedMeshRenderer _body;
        private Mesh _mesh;
        private Vector3[] _original;
        private Vector3[] _vertices;
        private Bounds _bounds;
        private Tail _tail;
        private Transform[] _bones;
        private Matrix4x4[] _bindposes;
        private UnitCommand _command;
        private double _nativeImpact = double.NegativeInfinity;
        private int _impactFrame = -1;
        private bool _deformed;

        internal int Contacts { get; private set; }
        internal float ContactGap { get; private set; }
        internal Vector3 ContactPoint { get; private set; }
        internal bool Resting { get { return !_deformed; } }

        internal void Configure(UnitEntityView view,
            SkinnedMeshRenderer body)
        {
            _owner = view == null ? null : view.EntityData as UnitEntityData;
            var profile = InfernalMartialProfile35.For(_owner);
            if (_owner == null ||
                !InfernalVisualPolicy35.IsExactIdentity(
                    InfernalVisualPolicy35.Bone,
                    _owner.Blueprint.AssetGuid, _owner.Blueprint.name,
                    _owner.Blueprint.Prefab?.AssetId) || profile?.Sting == null ||
                body?.sharedMesh == null ||
                body.sharedMesh.name != "KMG_bone-devil_Original")
                throw new InvalidOperationException(
                    "Bone Devil tail requires its exact original body and sting owner.");
            _tail = PteranodonAssetRuntime.BoneTail35();
            if (_tail == null)
                throw new InvalidOperationException(
                    "Bone Devil authored tail receipt is unavailable.");
            _body = body;
            _mesh = body.sharedMesh;
            _bounds = body.localBounds;
            _original = _mesh.vertices;
            _vertices = (Vector3[])_original.Clone();
            _bones = body.bones;
            _bindposes = _mesh.bindposes;
            ItemEntityWeapon sting = _owner.Body.AdditionalLimbs
                .Select(slot => slot.MaybeWeapon)
                .Single(weapon => weapon != null &&
                    ReferenceEquals(weapon.Blueprint, profile.Sting));
            if (sting.IsLootable ||
                !ReferenceEquals(sting.Wielder?.Unit, _owner))
                throw new InvalidOperationException(
                    "Bone Devil sting must be one private owned item.");
        }

        private static BoneDevilTailVisual35 For(UnitCommand command,
            ItemEntityWeapon weapon)
        {
            UnitEntityData owner = command?.Executor;
            var visual = owner?.View == null ? null :
                owner.View.GetComponent<BoneDevilTailVisual35>();
            var profile = InfernalMartialProfile35.For(owner);
            return visual != null && visual.isActiveAndEnabled &&
                ReferenceEquals(owner, visual._owner) &&
                profile?.Sting != null && weapon != null &&
                ReferenceEquals(weapon.Blueprint, profile.Sting) &&
                visual._body != null &&
                ReferenceEquals(visual._body.sharedMesh, visual._mesh)
                ? visual : null;
        }

        private void Bind(UnitCommand command)
        {
            if (!ReferenceEquals(_command, command)) Restore();
            _command = command;
        }

        private void LateUpdate()
        {
            if (_command != null && _owner != null && !_owner.Destroyed &&
                _owner.IsInState && !_owner.Descriptor.State.IsDead &&
                InfernalMartialProfile35.For(_owner)?.Sting != null &&
                _command.Result != UnitCommand.ResultType.Interrupt &&
                _command.TargetUnit?.View != null)
            {
                float elapsed = (float)(Game.Instance.TimeController.GameTime
                    .TotalSeconds - _nativeImpact);
                float remaining = 1f - Mathf.Clamp01((elapsed - .08f) / .27f);
                if (remaining > 0f)
                {
                    Apply(Mathf.SmoothStep(0f, 1f, remaining));
                    return;
                }
            }
            var command = _owner?.Commands.Attack;
            ItemEntityWeapon weapon = command?.PlannedAttack?.Hand?.MaybeWeapon;
            if (For(command, weapon) == null || command.IsFinished ||
                command.Target?.View == null)
            {
                Restore();
                _command = null;
                return;
            }
            Bind(command);
            var active = command.Animation?.ActiveAnimation;
            var clip = active?.GetPlayableClip();
            if (clip == null || command.Animation.IsInterrupted ||
                command.Animation.IsFinished)
            {
                if (_impactFrame != Time.frameCount) Restore();
                return;
            }
            var act = clip.events.FirstOrDefault(value =>
                value.functionName == "PostCommandActEvent");
            if (act == null || act.time <= 0f || clip.length <= act.time)
            {
                Restore();
                return;
            }
            float time = (float)active.GetTime();
            float weight = time <= act.time
                ? Mathf.SmoothStep(0f, 1f, time / act.time)
                : Mathf.SmoothStep(1f, 0f,
                    (time - act.time) / (clip.length - act.time));
            Apply(_impactFrame == Time.frameCount ? 1f : weight);
        }

        private void Apply(float weight)
        {
            Restore();
            if (_command?.TargetUnit?.View == null || weight <= 0f) return;
            Matrix4x4 skin = _bones[_tail.Bone].localToWorldMatrix *
                _bindposes[_tail.Bone];
            Vector3 origin = skin.MultiplyPoint3x4(_tail.Origin);
            Vector3 center = _command.TargetUnit.Position + Vector3.up * .85f;
            Vector3 surface = center + (origin - center).normalized *
                _command.TargetUnit.Corpulence;
            Vector3 target = skin.inverse.MultiplyPoint3x4(surface);
            Vector3 axis = _tail.Tip - _tail.Origin;
            Vector3 delta = (target - _tail.Tip) * weight;
            foreach (int index in _tail.Vertices)
            {
                float along = Mathf.Clamp01(Vector3.Dot(
                    _original[index] - _tail.Origin, axis) /
                    axis.sqrMagnitude);
                _vertices[index] = _original[index] + delta * along;
            }
            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds();
            _body.localBounds = _mesh.bounds;
            _deformed = true;
            if (weight < 1f) return;
            ContactGap = float.PositiveInfinity;
            foreach (int index in _tail.Vertices)
            {
                Vector3 vertex = skin.MultiplyPoint3x4(_vertices[index]);
                float gap = Vector3.Distance(vertex, surface);
                if (gap >= ContactGap) continue;
                ContactGap = gap;
                ContactPoint = vertex;
            }
        }

        private void Restore()
        {
            if (!_deformed || _mesh == null) return;
            Array.Copy(_original, _vertices, _original.Length);
            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds();
            if (_body != null) _body.localBounds = _bounds;
            _deformed = false;
        }

        internal void Release()
        {
            enabled = false;
            Restore();
            _command = null;
        }

        private void OnDisable() { Restore(); }
        private void OnDestroy() { Restore(); }

        [HarmonyPatch(typeof(UnitAttackOfOpportunity), "OnAction")]
        private static class NativeOpportunityContact
        {
            private static void Prefix(UnitAttackOfOpportunity __instance)
            {
                var visual = For(__instance, __instance?.Hand?.MaybeWeapon);
                if (visual == null || __instance.Target == null ||
                    __instance.Target.Descriptor.State.IsDead) return;
                visual.Bind(__instance);
                visual._nativeImpact = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                visual._impactFrame = Time.frameCount;
                visual.Apply(1f);
                visual.Contacts++;
            }
        }

        [HarmonyPatch(typeof(UnitAttack), "TriggerAttackRule")]
        private static class NativeContact
        {
            private static void Prefix(UnitAttack __instance)
            {
                var visual = For(__instance,
                    __instance?.PlannedAttack?.Hand?.MaybeWeapon);
                if (visual == null) return;
                visual.Bind(__instance);
                visual._nativeImpact = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                visual._impactFrame = Time.frameCount;
                visual.Apply(1f);
                visual.Contacts++;
            }
        }
    }
}
