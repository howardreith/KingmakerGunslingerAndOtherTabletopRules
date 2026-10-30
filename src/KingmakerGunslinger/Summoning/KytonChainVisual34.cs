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
    // Authored vertices remain rigidly palm-weighted. Only this owner's private
    // mesh extends during its native attack; no physics, new attacks or reach.
    [DefaultExecutionOrder(10000)]
    public sealed class KytonChainVisual34 : MonoBehaviour
    {
        internal sealed class Chain
        {
            internal int Bone;
            internal Vector3 Origin, Tip;
            internal int[] Vertices;
            internal static Chain[] Read(JToken payload, Mesh mesh, string[] bones)
            {
                var used = new System.Collections.Generic.HashSet<int>();
                var weights = mesh.boneWeights;
                var result = payload.Select(row => new Chain {
                    Bone = Array.IndexOf(bones, (string)row["bone"]),
                    Origin = Vector(row["origin"]), Tip = Vector(row["tip"]),
                    Vertices = row["vertices"].Values<int>().ToArray() }).ToArray();
                if (result.Length != 4 || result.Any(chain => chain.Bone < 0 ||
                    (chain.Tip - chain.Origin).sqrMagnitude < .1f || chain.Vertices.Length == 0 ||
                    chain.Vertices.Any(index => index < 0 || index >= weights.Length || !used.Add(index) ||
                        weights[index].boneIndex0 != chain.Bone || Mathf.Abs(weights[index].weight0 - 1f) > .0001f)))
                    throw new InvalidOperationException("Kyton chain indices must own four disjoint, single-palm-weighted regions.");
                return result;
            }
            private static Vector3 Vector(JToken row)
            {
                var a = row.Values<float>().ToArray();
                if (a.Length != 3 || a.Any(v => float.IsNaN(v) || float.IsInfinity(v)))
                    throw new InvalidOperationException("Nonfinite authored chain landmark.");
                return new Vector3(a[0], a[1], a[2]);
            }
        }
        private UnitEntityData _owner;
        private SkinnedMeshRenderer _body;
        private Mesh _mesh;
        private Vector3[] _original, _vertices;
        private Bounds _bounds;
        private Chain[] _chains;
        private ItemEntityWeapon[] _weapons;
        private Transform[] _bones;
        private Matrix4x4[] _bindposes;
        private UnitCommand _command;
        private double _nativeImpact = double.NegativeInfinity;
        private int _chain = -1, _impactFrame = -1;
        private bool _deformed;
        internal int Contacts { get; private set; }
        internal int ContactChain { get; private set; }
        internal float ContactGap { get; private set; }
        internal Vector3 ContactPoint { get; private set; }
        internal bool Resting { get { return !_deformed; } }

        internal void Configure(UnitEntityView view, SkinnedMeshRenderer body)
        {
            _owner = view.EntityData as UnitEntityData;
            if (_owner == null || !InfernalVisualPolicy34.IsExactIdentity(InfernalVisualPolicy34.Kyton,
                _owner.Blueprint.AssetGuid, _owner.Blueprint.name, _owner.Blueprint.Prefab?.AssetId) ||
                InfernalMartialProfile34.For(_owner)?.Chain == null || body?.sharedMesh == null ||
                body.sharedMesh.name != "KMG_kyton-chain-devil_Original")
                throw new InvalidOperationException("Kyton visual requires its exact original body and weapon owner.");
            _body = body; _mesh = body.sharedMesh; _bounds = body.localBounds;
            _original = _mesh.vertices; _vertices = (Vector3[])_original.Clone();
            _bones = body.bones; _bindposes = _mesh.bindposes;
            _chains = PteranodonAssetRuntime.KytonChains34();
            _weapons = new[] { _owner.Body.PrimaryHand.MaybeWeapon }
                .Concat(_owner.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon)).ToArray();
            var profile = InfernalMartialProfile34.For(_owner);
            if (_chains == null || _chains.Length != 4 || _weapons.Length != 4 ||
                _weapons.Distinct().Count() != 4 || _weapons.Any(weapon => weapon == null ||
                    !ReferenceEquals(weapon.Blueprint, profile.Chain) || weapon.IsLootable ||
                    !ReferenceEquals(weapon.Wielder?.Unit, _owner)))
                throw new InvalidOperationException("Kyton requires four independent owned chain items.");
        }
        private static KytonChainVisual34 For(UnitCommand command)
        {
            var owner = command?.Executor;
            var visual = owner?.View == null ? null : owner.View.GetComponent<KytonChainVisual34>();
            return visual != null && visual.isActiveAndEnabled && ReferenceEquals(owner, visual._owner) &&
                InfernalMartialProfile34.For(owner)?.Chain != null && visual._body != null &&
                visual._body.sharedMesh == visual._mesh ? visual : null;
        }
        private void Bind(UnitCommand command, ItemEntityWeapon weapon)
        {
            int chain = Array.IndexOf(_weapons, weapon);
            if (!ReferenceEquals(_command, command) || _chain != chain) Restore();
            _command = command; _chain = chain;
        }
        private void LateUpdate()
        {
            // UnitAttack.OnAction advances PlannedAttack immediately after its
            // contact rule. Keep that exact limb through visible follow-through
            // before binding the next planned limb. AoOs share the same bounded
            // presentation; neither path can create an attack or change reach.
            if (_command != null && _owner != null && !_owner.Destroyed && _owner.IsInState &&
                !_owner.Descriptor.State.IsDead && InfernalMartialProfile34.For(_owner)?.Chain != null &&
                _command.Result != UnitCommand.ResultType.Interrupt && _command.TargetUnit?.View != null) {
                float elapsed = (float)(Game.Instance.TimeController.GameTime.TotalSeconds - _nativeImpact);
                float remaining = 1f - Mathf.Clamp01((elapsed - .08f) / .27f);
                if (remaining > 0) { Apply(Mathf.SmoothStep(0, 1, remaining)); return; }
            }
            var command = _owner?.Commands.Attack;
            if (command == null || command.IsFinished || _owner.Destroyed || !_owner.IsInState ||
                InfernalMartialProfile34.For(_owner)?.Chain == null || command.Target?.View == null)
            { Restore(); _command = null; _chain = -1; return; }
            Bind(command, command.PlannedAttack?.Hand?.MaybeWeapon);
            var handle = command.Animation;
            var active = handle?.ActiveAnimation;
            var clip = active?.GetPlayableClip();
            if (_chain < 0 || clip == null || handle.IsInterrupted || handle.IsFinished)
            { if (_impactFrame != Time.frameCount) Restore(); return; }
            var act = clip.events.FirstOrDefault(value => value.functionName == "PostCommandActEvent");
            if (act == null || act.time <= 0 || clip.length <= act.time) { Restore(); return; }
            float time = (float)active.GetTime();
            float weight = time <= act.time ? Mathf.SmoothStep(0, 1, time / act.time) :
                Mathf.SmoothStep(1, 0, (time - act.time) / (clip.length - act.time));
            Apply(_impactFrame == Time.frameCount ? 1f : weight);
        }
        private void Apply(float weight)
        {
            Restore();
            if (_chain < 0 || _command?.TargetUnit?.View == null || weight <= 0) return;
            var chain = _chains[_chain];
            Matrix4x4 skin = _bones[chain.Bone].localToWorldMatrix * _bindposes[chain.Bone];
            Vector3 origin = skin.MultiplyPoint3x4(chain.Origin);
            Vector3 center = _command.TargetUnit.Position + Vector3.up * .85f;
            Vector3 surface = center + (origin - center).normalized * _command.TargetUnit.Corpulence;
            Vector3 target = skin.inverse.MultiplyPoint3x4(surface);
            Vector3 axis = chain.Tip - chain.Origin;
            Vector3 delta = (target - chain.Tip) * weight;
            foreach (int index in chain.Vertices) {
                float along = Mathf.Clamp01(Vector3.Dot(_original[index] - chain.Origin, axis) / axis.sqrMagnitude);
                _vertices[index] = _original[index] + delta * along;
            }
            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds(); _body.localBounds = _mesh.bounds;
            _deformed = true;
            if (weight >= 1f) {
                // Measure a real deformed mesh vertex, not the desired endpoint.
                ContactGap = float.PositiveInfinity;
                foreach (int index in chain.Vertices) {
                    Vector3 vertex = skin.MultiplyPoint3x4(_vertices[index]);
                    float gap = Vector3.Distance(vertex, surface);
                    if (gap < ContactGap) { ContactGap = gap; ContactPoint = vertex; }
                }
            }
        }
        private void Restore()
        {
            if (!_deformed || _mesh == null) return;
            Array.Copy(_original, _vertices, _original.Length);
            _mesh.vertices = _vertices; _mesh.RecalculateBounds();
            if (_body != null) _body.localBounds = _bounds;
            _deformed = false;
        }
        internal void Release() { enabled = false; Restore(); _command = null; }
        private void OnDisable() { Restore(); }
        private void OnDestroy() { Restore(); }

        [HarmonyPatch(typeof(UnitAttackOfOpportunity), "OnAction")]
        private static class NativeOpportunityContact
        {
            private static void Prefix(UnitAttackOfOpportunity __instance)
            {
                var visual = For(__instance);
                if (visual == null || __instance.Target == null || __instance.Target.Descriptor.State.IsDead) return;
                visual.Bind(__instance, __instance.Hand?.MaybeWeapon);
                if (visual._chain < 0) return;
                visual._nativeImpact = Game.Instance.TimeController.GameTime.TotalSeconds;
                visual._impactFrame = Time.frameCount;
                visual.Apply(1f); visual.ContactChain = visual._chain; visual.Contacts++;
            }
        }

        [HarmonyPatch(typeof(UnitAttack), "TriggerAttackRule")]
        private static class NativeContact
        {
            private static void Prefix(UnitAttack __instance)
            {
                var visual = For(__instance);
                if (visual == null) return;
                visual.Bind(__instance, __instance.PlannedAttack?.Hand?.MaybeWeapon);
                if (visual._chain < 0) return;
                visual._nativeImpact = Game.Instance.TimeController.GameTime.TotalSeconds;
                visual._impactFrame = Time.frameCount;
                visual.Apply(1f); visual.ContactChain = visual._chain; visual.Contacts++;
            }
        }
    }
}
