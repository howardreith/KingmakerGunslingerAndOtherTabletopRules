using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Salamander-only original Tail action, on the exact production identity
    /// or closed research prototype. Human Animator/spear clips are never edited.
    /// Native command time drives an owned legacy Animation component; this
    /// deliberately does not call the native null-clip/0.1-second fallback.
    /// Prototype playback/contact/cleanup passed exact6ae91. Production usage
    /// must qualify independently on its complete exact-head candidate.
    /// </summary>
    internal sealed class SalamanderTailAction : UnitAnimationActionSpecialAttack
    {
        private static readonly FieldInfo Attacks = typeof(UnitAnimationActionSpecialAttack)
            .GetField("m_Attacks", BindingFlags.Instance | BindingFlags.NonPublic);
        private UnitEntityView _view;
        private UnitAnimationManager _manager;
        private Animation _player;
        private AnimationClip _clip;
        private Transform[] _bones;
        private UnitAnimationActionHandle _handle;
        private SalamanderTailPlayback _playback;

        internal int StartedHandles { get; private set; }
        internal int ActEvents { get; private set; }
        internal int FinishedHandles { get; private set; }
        internal int LastActFrame { get; private set; }
        internal float LastNativeTime { get; private set; }
        internal float LastClipTime { get; private set; }
        internal string Failure { get; private set; }
        internal bool PlayingOwnedClip
        { get { return _handle != null && _playback != null && !_playback.Closed &&
            Ready && _player.IsPlaying(_clip.name) && _player[_clip.name].enabled; } }

        public override IEnumerable<AnimationClip> Clips
        { get { return _clip == null ? new AnimationClip[0] : new[] { _clip }; } }

        internal static SalamanderTailAction Create(UnitEntityView view, Animation player,
            AnimationClip clip, Transform[] originalTailBones, bool moduleEnabled)
        {
            var unit = view == null || view.EntityData == null ? null : view.EntityData.Blueprint;
            var weapon = view == null || view.EntityData == null ? null : view.EntityData.Body.PrimaryHand.MaybeWeapon;
            string ownedName = view == null ? null : "KMG_SalamanderHuman_" + view.GetInstanceID();
            bool ownedPlayer = view != null && player != null &&
                player.transform.IsChildOf(view.transform) &&
                player.name == ownedName + "_TailRoot" && player.GetComponent<Animator>() == null;
            bool ownedBones = ownedPlayer && originalTailBones != null &&
                originalTailBones.All(bone => bone != null && bone.parent == player.transform) &&
                originalTailBones.Distinct().Count() == 10 && player.transform.childCount == 10 &&
                SalamanderTailAnimationPolicy.ExactTailNames(originalTailBones.Select(bone => bone.name).ToArray());
            if (!SalamanderTailAnimationPolicy.PermitsBinding(moduleEnabled,
                unit == null ? null : unit.AssetGuid, unit == null ? null : unit.name,
                unit == null || unit.Prefab == null ? null : unit.Prefab.AssetId,
                weapon == null ? null : weapon.Blueprint.AssetGuid, ownedPlayer, ownedBones) ||
                view.AnimationManager == null || clip == null || !clip.legacy ||
                clip.name != ownedName + "_TailSlap" ||
                !SalamanderTailAnimationPolicy.Finite(clip.length) ||
                Math.Abs(clip.length - SalamanderTailAnimationPolicy.Duration) > .00001f ||
                player.GetClipCount() != 1 || !ReferenceEquals(player.GetClip(clip.name), clip) ||
                Attacks == null || Attacks.FieldType != typeof(UnitAnimationActionSpecialAttack.Entry[]))
                throw new InvalidOperationException("Only the exact owned Salamander view/legacy clip may bind.");

            var nativeBones = view.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(skin => skin.bones);
            if (originalTailBones.Any(bone => nativeBones.Any(native => ReferenceEquals(native, bone))))
                throw new InvalidOperationException("Create the original action before replacing the native body palette.");

            var action = ScriptableObject.CreateInstance<SalamanderTailAction>();
            try
            {
                action.name = ownedName + "_TailAction";
                action._view = view;
                action._manager = view.AnimationManager;
                action._player = player;
                action._clip = clip;
                action._bones = (Transform[])originalTailBones.Clone();
                action.AttackType = UnitAnimationSpecialAttackType.Tail;
                action.TransitionIn = 0;
                action.TransitionOut = 0;
                // Native UnitAttack.GetAnimationDuration uses this inherited
                // exact clip length. No gameplay duration/reach/weapon patch.
                Attacks.SetValue(action, new[] { new UnitAnimationActionSpecialAttack.Entry {
                    Variants = new[] { clip }, HasRangeBlend = false, BlendRanges = new float[0], Rend = null } });
                return action;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(action);
                throw;
            }
        }

        public override void OnStart(UnitAnimationActionHandle handle)
        {
            try { StartOwned(handle); }
            catch (Exception error) { Reject(handle, "owned clip start exception: " + error.GetType().Name); }
        }

        private void StartOwned(UnitAnimationActionHandle handle)
        {
            if (_handle != null || !Ready || handle == null || !ReferenceEquals(handle.Manager, _manager) ||
                !ReferenceEquals(handle.Action, this))
            {
                Reject(handle, "foreign, overlapping or unavailable original Tail action");
                return;
            }
            _handle = handle;
            _playback = new SalamanderTailPlayback(handle);
            StartedHandles++;
            Failure = null;
            if (!_player.Play(_clip.name, PlayMode.StopAll))
            {
                Reject(handle, "owned legacy clip refused native action start");
                return;
            }
            Evaluate(handle, 0); // Own clip start, not a second native attack.
        }

        public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
        {
            if (!ReferenceEquals(handle, _handle)) return;
            // Native AnimationActionHandle.UpdateInternal already applies its
            // SpeedScale and advances GetTime. Never add Unity/wall time again.
            try
            {
                Evaluate(handle, handle.GetTime());
                if (ReferenceEquals(handle, _handle) && _playback != null && _playback.Complete)
                    handle.Release();
            }
            catch (Exception error) { Reject(handle, "owned clip update exception: " + error.GetType().Name); }
        }

        private bool Ready
        {
            get
            {
                if (_view == null || _manager == null || _player == null || _clip == null ||
                    !ReferenceEquals(_view.AnimationManager, _manager) || !_clip.legacy ||
                    !ReferenceEquals(_player.GetClip(_clip.name), _clip) || _player[_clip.name] == null ||
                    _bones == null || _bones.Any(bone => bone == null || bone.parent != _player.transform)) return false;
                return true;
            }
        }

        private void Evaluate(UnitAnimationActionHandle handle, float nativeTime)
        {
            float clipTime;
            if (_playback == null || !_playback.Prepare(handle, nativeTime, handle.IsInterrupted,
                Ready && _player.IsPlaying(_clip.name), out clipTime))
            {
                Reject(handle, "interrupted, invalid clock or missing owned playback");
                return;
            }
            AnimationState state = _player[_clip.name];
            state.speed = 0; // Only native handle time, never a second clock.
            state.wrapMode = WrapMode.ClampForever;
            state.weight = 1;
            state.time = clipTime;
            _player.Sample();
            bool evaluated = Ready && _player.IsPlaying(_clip.name) && state.enabled &&
                ReferenceEquals(state.clip, _clip) && state.time == clipTime && state.weight == 1 &&
                _bones.All(FiniteOriginalPose);
            LastNativeTime = nativeTime;
            LastClipTime = clipTime;
            if (_playback.Sampled(handle, clipTime, evaluated))
            {
                handle.IsActed = true; // One authored clip event; native UnitAttack owns the rule.
                ActEvents++;
                LastActFrame = Time.frameCount;
            }
            if (_playback.Closed) Reject(handle, "owned clip sample did not evaluate exactly");
        }

        public override void OnFinish(UnitAnimationActionHandle handle)
        {
            if (!ReferenceEquals(handle, _handle)) return;
            Stop(handle);
            FinishedHandles++;
        }

        public override void OnSequencedInterrupted(AnimationActionHandle handle)
        {
            Stop(handle as UnitAnimationActionHandle);
        }

        private void Reject(UnitAnimationActionHandle handle, string reason)
        {
            Failure = reason;
            try { Stop(handle); }
            catch (Exception error) { Failure += "; owned reset failed: " + error.GetType().Name; }
            finally { InterruptExactCommand(handle); }
        }

        internal void StopOwnedPlayback()
        {
            var handle = _handle;
            if (handle == null) return;
            try { Stop(handle); }
            finally { InterruptExactCommand(handle); }
        }

        private void InterruptExactCommand(UnitAnimationActionHandle handle)
        {
            if (handle == null || !ReferenceEquals(handle.Action, this) ||
                !ReferenceEquals(handle.Manager, _manager)) return;
            // Use the public native command interruption seam, not private
            // handle flags. Never clear another attack/queue or stop a brain.
            try
            {
                if (_view != null && _view.EntityData != null)
                    foreach (var command in _view.EntityData.Commands.Raw.OfType<UnitAttack>()
                        .Where(value => !value.IsFinished && ReferenceEquals(value.Animation, handle)).ToArray())
                        command.Interrupt(true);
            }
            finally { handle.Release(); }
        }

        private void Stop(UnitAnimationActionHandle handle)
        {
            if (handle == null || !ReferenceEquals(handle, _handle)) return;
            if (_playback != null) _playback.Close(handle);
            try
            {
                if (Ready)
                {
                    AnimationState state = _player[_clip.name];
                    state.time = 0;
                    state.speed = 0;
                    _player.Sample();
                }
            }
            finally
            {
                // Even a lost manager/clip readiness check must stop our
                // own component. Never stop the human Animator or its clips.
                try { if (_player != null) _player.Stop(); }
                finally { _handle = null; _playback = null; }
            }
        }

        private static bool FiniteOriginalPose(Transform bone)
        {
            Vector3 p = bone.localPosition;
            Quaternion r = bone.localRotation;
            return SalamanderTailAnimationPolicy.Finite(p.x) && SalamanderTailAnimationPolicy.Finite(p.y) &&
                SalamanderTailAnimationPolicy.Finite(p.z) && SalamanderTailAnimationPolicy.Finite(r.x) &&
                SalamanderTailAnimationPolicy.Finite(r.y) && SalamanderTailAnimationPolicy.Finite(r.z) &&
                SalamanderTailAnimationPolicy.Finite(r.w) &&
                bone.localScale == Vector3.one;
        }
    }
}
