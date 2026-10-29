using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // The measured free-hands rig has Claw/Bite but no Slam. Keep the native
    // arm clips and command timing, adding only an exact-owner private Slam action.
    internal sealed class StalkerSlamAnimation33 : MonoBehaviour
    {
        private UnitEntityView _view;
        private AnimationSet _native, _owned;
        private UnitAnimationActionSpecialAttack _slam;
        private AnimationActionBase[] _nativeActions;
        private int _frames;
        private bool _restorePending;
        internal string Outcome { get; private set; } = "waiting-for-native-view";
        internal void Initialize(UnitEntityView view) { _view = view; }
        internal bool Live => _owned != null && _slam != null && _view?.AnimationManager != null &&
            ReferenceEquals(_view.AnimationManager.AnimationSet, _owned) &&
            _native.Actions.SequenceEqual(_nativeActions) &&
            ReferenceEquals(_view.AnimationManager.GetAction(UnitAnimationSpecialAttackType.Slam), _slam);
        internal UnityEngine.Object[] OwnedResources() => new UnityEngine.Object[] { this, _owned, _slam };
        private void LateUpdate()
        {
            try
            {
                var unit = _view?.EntityData;
                if (unit == null || unit.Destroyed || !ShadowOutsiderVisualPolicy.IsExactIdentity(
                    ShadowOutsiderVisualPolicy.Stalker, unit.Blueprint.AssetGuid, unit.Blueprint.name, unit.Blueprint.Prefab?.AssetId))
                    throw new InvalidDataException("Exact Stalker animation owner absent.");
                if (_view.AnimationManager == null)
                { if (++_frames < 180) return; throw new InvalidDataException("Native manager did not settle."); }
                _native = _view.AnimationManager.AnimationSet;
                if (_native == null || _native.name != "Lizardfolk_FreeHands_AnimationSet" ||
                    _native.Transitions.Any() || _native.StartupAction != null)
                    throw new InvalidDataException("Measured native animation set differs.");
                _nativeActions = _native.Actions.ToArray();
                var specials = _nativeActions.OfType<UnitAnimationActionSpecialAttack>().ToArray();
                var claw = specials.Single(value => value.AttackType == UnitAnimationSpecialAttackType.Claw &&
                    value.name == "Lizardfolk_FreeHands_AnimationSet_Claw");
                if (specials.Any(value => value.AttackType == UnitAnimationSpecialAttackType.Slam) ||
                    !claw.Clips.Select(value => value.name).OrderBy(value => value).SequenceEqual(new[] { "Attack_01_1", "Attack_01_2" }))
                    throw new InvalidDataException("Measured arm clips or missing-Slam contract differs.");
                var actions = typeof(AnimationSet).GetField("m_Actions", BindingFlags.Instance | BindingFlags.NonPublic);
                if (actions == null) throw new MissingFieldException("Native animation action list");
                _slam = UnityEngine.Object.Instantiate(claw);
                _slam.name = "KMG_InvisibleStalker_PrivateSlam";
                _slam.AttackType = UnitAnimationSpecialAttackType.Slam;
                _owned = UnityEngine.Object.Instantiate(_native);
                _owned.name = "KMG_InvisibleStalker_PrivateAnimationSet";
                actions.SetValue(_owned, new List<AnimationActionBase>(_nativeActions.Concat(new[] { _slam })));
                _view.AnimationManager.AnimationSet = _owned;
                if (!Live) throw new InvalidOperationException("Private Slam binding failed.");
                Outcome = "native-arm-clips-bound-to-slam";
            }
            catch (Exception error)
            {
                Outcome = "binding-failed:" + error.Message;
                try { Release(); } catch (Exception cleanup) { Debug.LogException(cleanup); }
                Debug.LogException(error);
            }
            enabled = false;
        }
        internal void Release()
        {
            var manager = _view == null ? null : _view.AnimationManager;
            if (_restorePending && manager != null && !ReferenceEquals(manager.AnimationSet, _owned) &&
                !ReferenceEquals(manager.AnimationSet, _native)) return;
            if (_owned != null && manager != null && (ReferenceEquals(manager.AnimationSet, _owned) || _restorePending))
            { _restorePending = true; manager.AnimationSet = _native; _restorePending = false; }
            if (manager == null || !_restorePending && (_owned == null || !ReferenceEquals(manager.AnimationSet, _owned)))
            {
                if (_owned != null) UnityEngine.Object.DestroyImmediate(_owned);
                if (_slam != null) UnityEngine.Object.DestroyImmediate(_slam);
                _owned = null; _slam = null;
            }
        }
        private void OnDestroy() { try { Release(); } catch (Exception error) { Debug.LogException(error); } }
    }
}
