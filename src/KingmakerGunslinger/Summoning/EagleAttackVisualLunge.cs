using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Moves only the one attached Eagle skeleton during its native attack.
    /// The entity, view root, movement agent, selection and mechanical reach
    /// never move. The native Animator can rewrite the skeleton root each
    /// frame; remove only an offset that is still present before applying the
    /// next one, and restore it on disable or destruction.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    internal sealed class EagleAttackVisualLunge : MonoBehaviour
    {
        private UnitEntityView _view;
        private SkinnedMeshRenderer _renderer;
        private Transform _root;
        private UnitEntityData _target;
        private float _startedAt = -1f;
        private float _impactAt = -1f;
        private Vector3 _appliedOffset;
        private Vector3 _lastNativeWorld;
        private Vector3 _lastAppliedWorld;
        private int _begins;
        private int _impacts;

        internal void Configure(UnitEntityView view,
            SkinnedMeshRenderer renderer)
        {
            if (view == null || renderer == null || renderer.rootBone == null ||
                renderer.sharedMesh == null || renderer.sharedMesh.name !=
                    ExpandedSummoningPteranodonViewPatch.EagleVisualName)
                throw new InvalidOperationException(
                    "Eagle visual lunge requires an attached instance renderer.");
            _view = view;
            _renderer = renderer;
            _root = renderer.rootBone;
        }

        internal static EagleAttackVisualLunge For(UnitAttack command)
        {
            UnitEntityData owner = command == null ? null : command.Executor;
            if (owner == null || owner.Blueprint == null ||
                owner.Blueprint.name !=
                    ExpandedSummoningPteranodonViewPatch.EagleBlueprintName ||
                owner.View == null)
                return null;
            EagleAttackVisualLunge component =
                owner.View.GetComponent<EagleAttackVisualLunge>();
            return component != null && component.isActiveAndEnabled &&
                component._renderer != null &&
                component._renderer.sharedMesh != null &&
                component._renderer.sharedMesh.name ==
                    ExpandedSummoningPteranodonViewPatch.EagleVisualName
                ? component : null;
        }

        internal void Begin(UnitEntityData target)
        {
            if (_view == null || _root == null || target == null ||
                target.View == null) return;
            _target = target;
            _startedAt = Time.unscaledTime;
            _impactAt = -1f;
            _begins++;
        }

        internal void Impact(UnitEntityData target)
        {
            if (target == null || target.View == null) return;
            if (_startedAt < 0f || !ReferenceEquals(_target, target))
                Begin(target);
            _impactAt = Time.unscaledTime;
            _impacts++;
            ApplyAtCurrentTime();
        }

        internal string Describe()
        {
            return "begins=" + _begins + ";impacts=" + _impacts +
                ";offset=" + _appliedOffset.magnitude.ToString("0.###",
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        private void LateUpdate()
        { ApplyAtCurrentTime(); }

        private void ApplyAtCurrentTime()
        {
            if (_root == null) return;
            RestoreNative();
            if (_view == null || _target == null || _target.View == null ||
                _startedAt < 0f) return;
            float elapsed = Time.unscaledTime - _startedAt;
            float sinceImpact = _impactAt < 0f ? -1f :
                Time.unscaledTime - _impactAt;
            float weight = EagleAttackLungePolicy.Weight(elapsed, sinceImpact);
            if (weight <= 0f)
            {
                if (elapsed > EagleAttackLungePolicy.PreImpactLimitSeconds +
                    EagleAttackLungePolicy.ReturnSeconds || _impactAt >= 0f)
                    _target = null;
                return;
            }
            Vector3 direction = _target.Position - _view.EntityData.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            Vector3 native = _root.position;
            _appliedOffset = direction.normalized *
                (EagleAttackLungePolicy.MaximumMeters * weight);
            _lastNativeWorld = native;
            _lastAppliedWorld = native + _appliedOffset;
            _root.position = _lastAppliedWorld;
        }

        private void RestoreNative()
        {
            if (_root == null || _appliedOffset.sqrMagnitude < 0.0000001f)
            {
                _appliedOffset = Vector3.zero;
                return;
            }
            Vector3 current = _root.position;
            // Animator may already have restored its native root this frame.
            // The nearer previous state tells whether our offset survives.
            if (Vector3.Distance(current, _lastAppliedWorld) <=
                Vector3.Distance(current, _lastNativeWorld))
                _root.position = current - _appliedOffset;
            _appliedOffset = Vector3.zero;
        }

        private void OnDisable()
        { RestoreNative(); }

        private void OnDestroy()
        { RestoreNative(); }
    }

    [HarmonyPatch(typeof(UnitAttack), "TryStartNextAttack")]
    internal static class EagleAttackVisualLungeStartPatch
    {
        private static void Postfix(UnitAttack __instance, bool __result)
        {
            if (!__result) return;
            EagleAttackVisualLunge lunge = EagleAttackVisualLunge.For(__instance);
            if (lunge != null) lunge.Begin(__instance.Target);
        }
    }

    [HarmonyPatch(typeof(UnitAttack), "TriggerAttackRule")]
    internal static class EagleAttackVisualLungeImpactPatch
    {
        private static void Prefix(UnitAttack __instance)
        {
            EagleAttackVisualLunge lunge = EagleAttackVisualLunge.For(__instance);
            if (lunge != null) lunge.Impact(__instance.Target);
        }
    }
}
