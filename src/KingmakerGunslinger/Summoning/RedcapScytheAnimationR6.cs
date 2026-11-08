using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R6 (PRES-03): this view's native main-hand action has no entry
    /// for the Redcap's scythe style, so the swing ended with no clip. A
    /// private copy of the action adds that style using the existing scythe
    /// clips, on a private animation set for this one view (the established
    /// Stalker pattern). Native assets, the shared scythe, and statistics are
    /// untouched. A summon whose saved context no longer names a KMG ability
    /// keeps the native behavior.
    /// </summary>
    internal sealed class RedcapScytheAnimationR6 : MonoBehaviour
    {
        private const string NativeHandName = "Redcap_AnimationSet1_MainHandAttack";
        private static readonly FieldInfo SettingsField = typeof(UnitAnimationActionHandAttack)
            .GetField("m_Settings", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionsField = typeof(AnimationSet)
            .GetField("m_Actions", BindingFlags.Instance | BindingFlags.NonPublic);

        private UnitEntityView _view;
        private AnimationSet _native, _owned;
        private UnitAnimationActionHandAttack _hand;
        private int _frames;
        private bool _restorePending;
        internal string Outcome { get; private set; } = "waiting-for-native-view";
        internal void Initialize(UnitEntityView view) { _view = view; }

        internal bool Live => _owned != null && _hand != null && _view?.AnimationManager != null &&
            ReferenceEquals(_view.AnimationManager.AnimationSet, _owned) &&
            ReferenceEquals(_view.AnimationManager.GetAction(UnitAnimationType.MainHandAttack), _hand);

        private void LateUpdate()
        {
            try
            {
                UnitEntityData unit = _view == null ? null : _view.EntityData;
                if (unit == null || unit.Destroyed || unit.Blueprint.AssetGuid != SummonPlaytestR6Policy.RedcapSummonedUnitGuid)
                    throw new InvalidDataException("Exact Redcap animation owner absent.");
                if (_view.AnimationManager == null || _view.AnimationManager.AnimationSet == null)
                { if (++_frames < 180) return; throw new InvalidDataException("Native manager did not settle."); }
                _native = _view.AnimationManager.AnimationSet;
                var hand = _native.Actions.OfType<UnitAnimationActionHandAttack>().Single(value => value.IsMainHand && value.name == NativeHandName);
                WeaponAnimationStyle style = unit.Body.PrimaryHand.GetWeaponStyle();
                if (hand.HasSetting(style)) { Outcome = "native-mapping-present:" + style; enabled = false; return; }
                if (SettingsField == null || ActionsField == null) throw new MissingFieldException("Native hand action layout");
                var settings = (Array)SettingsField.GetValue(hand);
                Type element = settings.GetType().GetElementType();
                FieldInfo variants = element.GetField("Variants"), styleField = element.GetField("Style");
                object scythe = settings.Cast<object>().Single(value => ((AnimationClip[])variants.GetValue(value))
                    .All(clip => clip != null && clip.name.StartsWith(SummonPlaytestR6Policy.RedcapScytheClipPrefix, StringComparison.Ordinal)));
                object added = Activator.CreateInstance(element, true);
                foreach (FieldInfo field in element.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    field.SetValue(added, field.GetValue(scythe));
                styleField.SetValue(added, style);
                Array extended = Array.CreateInstance(element, settings.Length + 1);
                Array.Copy(settings, extended, settings.Length);
                extended.SetValue(added, settings.Length);

                _hand = UnityEngine.Object.Instantiate(hand);
                _hand.name = "KMG_RedcapSummoned_PrivateMainHand";
                SettingsField.SetValue(_hand, extended);
                _owned = UnityEngine.Object.Instantiate(_native);
                _owned.name = "KMG_RedcapSummoned_PrivateAnimationSet";
                ActionsField.SetValue(_owned, new List<AnimationActionBase>(_native.Actions.Select(value =>
                    ReferenceEquals(value, hand) ? _hand : value)));
                _view.AnimationManager.AnimationSet = _owned;
                if (!Live || !_hand.HasSetting(style)) throw new InvalidOperationException("Private scythe binding failed.");
                Outcome = "native-scythe-clips-bound:" + style;
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
                if (_hand != null) UnityEngine.Object.DestroyImmediate(_hand);
                _owned = null; _hand = null;
            }
        }

        private void OnDestroy() { try { Release(); } catch (Exception error) { Debug.LogException(error); } }
    }

    /// <summary>
    /// The summoned-unit buff carries the real summoning ability and spawns
    /// its effect again when a view is recreated, so this one seam covers a
    /// fresh summon and a reloaded one.
    /// </summary>
    [HarmonyPatch(typeof(Buff), "SpawnParticleEffect")]
    internal static class RedcapScytheOriginPatch
    {
        private static void Postfix(Buff __instance)
        {
            try
            {
                BlueprintRoot root = BlueprintRoot.Instance;
                BlueprintBuff summoned = root == null || root.SystemMechanics == null ? null : root.SystemMechanics.SummonedUnitBuff;
                if (__instance == null || summoned == null || !ReferenceEquals(__instance.Blueprint, summoned) ||
                    !ExpandedSummoningVisualVariantPatch.IsModuleActive) return;
                UnitEntityData unit = __instance.Owner == null ? null : __instance.Owner.Unit;
                UnitEntityView view = unit == null ? null : unit.View;
                var ability = __instance.Context == null ? null : __instance.Context.SourceAbility;
                if (view == null || unit.Blueprint == null || ability == null || view.GetComponent<RedcapScytheAnimationR6>() != null) return;
                bool spawns = AlternateSummoningGraph.SpawnUnits(ability).Contains(unit.Blueprint.AssetGuid);
                if (!SummonPlaytestR6Policy.RedcapBindingAllowed(unit.Blueprint.AssetGuid, ability.name, spawns)) return;
                view.gameObject.AddComponent<RedcapScytheAnimationR6>().Initialize(view);
            }
            catch (Exception error)
            {
                // A presentation binding never interrupts the game's own buff.
                Debug.LogException(error);
            }
        }
    }
}
