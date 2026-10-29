using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Native128: both exact Satyr bodies share the same 55-bone rig. The
    // archer has only Bow; its melee peer supplies the missing small-blade
    // setting. Use the shipped view-owned AnimationSet seam without touching
    // shared actions, clips, rigs, weapon rules or native action handling.
    internal sealed class SatyrNativeWeaponAnimation32 : MonoBehaviour
    {
        private const string Archer = "6114adae73a1338498412b3c63c946f8";
        private const string ArcherPrefab = "22dfc39c941704743a5b3fd9769327a0";
        private const string Melee = "a6dceccaf84af1a42843e58412265388";
        private const string MeleePrefab = "36a3ffb15f1331c489ac1ab666aa9d26";
        private static readonly FieldInfo Settings = typeof(UnitAnimationActionHandAttack)
            .GetField("m_Settings", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo Actions = typeof(AnimationSet)
            .GetField("m_Actions", BindingFlags.Instance | BindingFlags.NonPublic);
        private sealed class WeaponSetting
        {
            internal WeaponAnimationStyle Style;
            internal AnimationClip[] Variants;
            internal AnimationClip Rend, Charge;
        }
        private UnitEntityView _view;
        private AnimationSet _native, _donor, _owned;
        private UnitAnimationActionHandAttack _nativeHand, _donorHand, _hand;
        private AnimationActionBase[] _nativeActions, _donorActions, _expectedActions;
        private WeaponSetting[] _bow, _blade, _expectedSettings;
        private int _frames;
        private bool _restorePending;
        internal string Outcome { get; private set; } = "waiting-for-native-view";
        internal AnimationSet OwnedSet => _owned;
        internal UnitAnimationActionHandAttack OwnedHand => _hand;
        internal string LocomotionBeforeBinding { get; private set; }
        internal string LocomotionAfterBinding { get; private set; }

        internal static string DescribeLocomotion(UnitAnimationManager manager)
        {
            var field = typeof(UnitAnimationManager).GetField("m_LocoMotionHandle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException("Native locomotion handle contract changed.");
            var handle = field.GetValue(manager) as UnitAnimationActionHandle;
            return handle == null ? "absent" : "action=" + handle.Action.name + ";started=" + handle.IsStarted +
                ";finished=" + handle.IsFinished + ";active=" + manager.ActiveActions.Contains(handle);
        }

        internal static bool Permits(UnitEntityView view)
        {
            ModContext context;
            var unit = view?.EntityData; var blueprint = unit?.Blueprint;
            return ModContext.TryGet(out context) && context.FeatureModules?.Active?.ExpandedSummoning == true &&
                blueprint?.AssetGuid == "b0dab767cb2948ca9f918bf7d7c3dbb4" &&
                blueprint.name == "KMG_Summoning_Unit_Satyr" && blueprint.Prefab?.AssetId == ArcherPrefab &&
                (blueprint.Body?.PrimaryHand as BlueprintItemWeapon)?.AssetGuid == "6d40d84e239bdf345b349ff52e3c00a9" &&
                (blueprint.Body?.PrimaryHandAlternative1 as BlueprintItemWeapon)?.AssetGuid == "25698949bdea41d2bcbe13ad8783aa4d" &&
                unit.Body != null && !unit.Body.IsPolymorphed;
        }

        internal void Initialize(UnitEntityView view) { _view = view; }

        private void LateUpdate()
        {
            try
            {
                if (!Permits(_view) || _view.EntityData.Destroyed)
                    throw new InvalidDataException("Exact live Satyr owner no longer eligible.");
                if (_view.AnimationManager == null || _view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .All(value => value.name != "satyr01" || value.sharedMesh == null))
                {
                    if (++_frames < 180) return;
                    throw new InvalidDataException("Native Satyr view did not settle.");
                }
                Bind(); Outcome = "native-bow-and-blade-bound";
            }
            catch (Exception error)
            {
                Outcome = "binding-failed:" + error.Message;
                try { Release(); } catch (Exception cleanup) { Debug.LogException(cleanup); }
                Debug.LogException(error);
            }
            enabled = false;
        }

        private void Bind()
        {
            if (Settings == null || Actions == null) throw new InvalidDataException("Native animation fields changed.");
            var archer = NativeView(Archer, ArcherPrefab); var melee = NativeView(Melee, MeleePrefab);
            _native = archer.GetComponentsInChildren<UnitAnimationManager>(true).Single().AnimationSet;
            _donor = melee.GetComponentsInChildren<UnitAnimationManager>(true).Single().AnimationSet;
            if (_native == null || _donor == null || !ReferenceEquals(_view.AnimationManager.AnimationSet, _native) ||
                _native.StartupAction != null || _donor.StartupAction != null ||
                _native.Transitions.Any() || _donor.Transitions.Any() || !SameRig(_view, archer) || !SameRig(archer, melee))
                throw new InvalidDataException("Exact native Satyr rig/set boundary differs.");
            _nativeActions = _native.Actions.ToArray(); _donorActions = _donor.Actions.ToArray();
            _nativeHand = _nativeActions.OfType<UnitAnimationActionHandAttack>().Single(value => value.Type == UnitAnimationType.MainHandAttack);
            _donorHand = _donorActions.OfType<UnitAnimationActionHandAttack>().Single(value => value.Type == UnitAnimationType.MainHandAttack);
            if (_nativeHand.name != "Weretiger_AnimationSet2_MainHandAttack" ||
                _donorHand.name != "Weretiger_AnimationSet3_MainHandAttack")
                throw new InvalidDataException("Exact native Satyr hand action changed.");
            _bow = ReadSettings(_nativeHand).Select(CopySetting).ToArray();
            _blade = ReadSettings(_donorHand).Select(CopySetting).ToArray();
            if (_bow.Length != 1 || _blade.Length != 1 || _bow[0].Style != WeaponAnimationStyle.Bow ||
                _blade[0].Style != WeaponAnimationStyle.SlashingOneHanded ||
                !_bow[0].Variants.Select(value => value.name).SequenceEqual(new[] { "attackWithBow_b" }) ||
                !_blade[0].Variants.Select(value => value.name).SequenceEqual(new[] { "attackWithSword_01", "attackWithSword_02" }))
                throw new InvalidDataException("Exact native Satyr weapon settings changed.");
            _hand = UnityEngine.Object.Instantiate(_nativeHand);
            _hand.name = "KMG_Satyr32_PrivateMainHand";
            _expectedSettings = _bow.Concat(_blade).Select(CopySetting).ToArray();
            WritePrivateSettings(_hand, _expectedSettings);
            _expectedActions = _nativeActions.Select(value => ReferenceEquals(value, _nativeHand) ? _hand : value).ToArray();
            _owned = UnityEngine.Object.Instantiate(_native);
            _owned.name = "KMG_Satyr32_PrivateAnimationSet";
            Actions.SetValue(_owned, new List<AnimationActionBase>(_expectedActions));
            LocomotionBeforeBinding = DescribeLocomotion(_view.AnimationManager);
            _view.AnimationManager.AnimationSet = _owned;
            LocomotionAfterBinding = DescribeLocomotion(_view.AnimationManager);
            if (!Live) throw new InvalidOperationException("Private Satyr animation binding did not round-trip.");
        }

        internal bool Live => _view != null && _owned != null && _hand != null &&
            ReferenceEquals(_view.AnimationManager?.AnimationSet, _owned) &&
            _native.Actions.SequenceEqual(_nativeActions) && _donor.Actions.SequenceEqual(_donorActions) &&
            _owned.Actions.SequenceEqual(_expectedActions) && _owned.StartupAction == null && !_owned.Transitions.Any() &&
            SameSettings(ReadSettings(_nativeHand), _bow) && SameSettings(ReadSettings(_donorHand), _blade) &&
            SameSettings(ReadSettings(_hand), _expectedSettings) &&
            !ReferenceEquals(Settings.GetValue(_hand), Settings.GetValue(_nativeHand)) &&
            !ReferenceEquals(_owned.Actions, _native.Actions) &&
            _hand.HasSetting(WeaponAnimationStyle.Bow) && _hand.HasSetting(WeaponAnimationStyle.SlashingOneHanded);

        internal UnityEngine.Object[] OwnedResources() => new UnityEngine.Object[] { _owned, _hand, this };
        internal UnityEngine.Object[] BorrowedResources() => new UnityEngine.Object[] { _native, _donor, _nativeHand, _donorHand }
            .Concat(_nativeActions).Concat(_donorActions).Concat(_nativeHand.Clips).Concat(_donorHand.Clips).Distinct().ToArray();

        internal void Release()
        {
            var manager = _view == null ? null : _view.AnimationManager;
            if (_restorePending && manager != null && !ReferenceEquals(manager.AnimationSet, _owned) &&
                !ReferenceEquals(manager.AnimationSet, _native)) return; // A foreign replacement retains its ownership.
            if (_owned != null && manager != null && (ReferenceEquals(manager.AnimationSet, _owned) || _restorePending))
            {
                // The native setter assigns its set before finishing handles.
                // If initialization throws, retain our containers and retry even
                // though that property may already refer to the original set.
                _restorePending = true;
                manager.AnimationSet = _native;
                _restorePending = false;
            }
            if (manager == null || !_restorePending && (_owned == null || !ReferenceEquals(manager.AnimationSet, _owned)))
            {
                if (_owned != null) UnityEngine.Object.DestroyImmediate(_owned);
                if (_hand != null) UnityEngine.Object.DestroyImmediate(_hand);
                _owned = null; _hand = null;
            }
        }

        private void OnDestroy() { try { Release(); } catch (Exception error) { Debug.LogException(error); } }

        private static UnitEntityView NativeView(string guid, string prefab)
        {
            var unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library, guid, "native Satyr animation peer");
            if (unit.Prefab?.AssetId != prefab) throw new InvalidDataException("Native Satyr prefab identity differs.");
            var view = unit.Prefab.Load(false);
            if (view == null) throw new InvalidDataException("Native Satyr prefab unavailable.");
            return view;
        }

        private static FieldInfo[] SettingFields()
        {
            var type = Settings?.FieldType.GetElementType();
            var fields = type?.GetFields(BindingFlags.Instance | BindingFlags.Public);
            string[] names = { "Style", "Variants", "Rend", "Charge" };
            Type[] types = { typeof(WeaponAnimationStyle), typeof(AnimationClip[]), typeof(AnimationClip), typeof(AnimationClip) };
            if (type == null || fields == null || fields.Length != names.Length ||
                names.Where((name, index) => fields.Count(field => field.Name == name && field.FieldType == types[index]) != 1).Any())
                throw new InvalidDataException("Native Satyr private weapon-setting fields changed.");
            return names.Select(name => fields.Single(field => field.Name == name)).ToArray();
        }

        private static WeaponSetting[] ReadSettings(UnitAnimationActionHandAttack action)
        {
            var native = Settings?.GetValue(action) as Array;
            if (native == null || native.Length > 2 || native.Cast<object>().Any(value => value == null))
                throw new InvalidDataException("Native Satyr weapon setting array differs.");
            var fields = SettingFields();
            var rows = native.Cast<object>().Select(value => new WeaponSetting {
                Style = (WeaponAnimationStyle)fields[0].GetValue(value),
                Variants = (fields[1].GetValue(value) as AnimationClip[])?.ToArray(),
                Rend = fields[2].GetValue(value) as AnimationClip, Charge = fields[3].GetValue(value) as AnimationClip }).ToArray();
            if (rows.Any(value => value.Variants == null ||
                value.Variants.Length == 0 || value.Variants.Length > 2 || value.Variants.Any(clip =>
                    clip == null || float.IsNaN(clip.length) || float.IsInfinity(clip.length) || clip.length <= 0)))
                throw new InvalidDataException("Native Satyr weapon clips are missing or outside the observed boundary.");
            return rows;
        }

        private static void WritePrivateSettings(UnitAnimationActionHandAttack action, WeaponSetting[] values)
        {
            var fields = SettingFields(); var type = Settings.FieldType.GetElementType();
            var rows = Array.CreateInstance(type, values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                var row = Activator.CreateInstance(type, true);
                fields[0].SetValue(row, values[i].Style); fields[1].SetValue(row, values[i].Variants.ToArray());
                fields[2].SetValue(row, values[i].Rend); fields[3].SetValue(row, values[i].Charge);
                rows.SetValue(row, i);
            }
            Settings.SetValue(action, rows);
        }

        private static WeaponSetting CopySetting(WeaponSetting value) =>
            new WeaponSetting { Style = value.Style, Variants = value.Variants.ToArray(), Rend = value.Rend, Charge = value.Charge };

        private static bool SameSettings(WeaponSetting[] left, WeaponSetting[] right) => left.Length == right.Length && left.Zip(right, (a, b) =>
                a.Style == b.Style && a.Variants.SequenceEqual(b.Variants) && ReferenceEquals(a.Rend, b.Rend) && ReferenceEquals(a.Charge, b.Charge)).All(value => value);

        private static bool SameRig(UnitEntityView left, UnitEntityView right)
        {
            var a = left.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(value => value.name == "satyr01");
            var b = right.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(value => value.name == "satyr01");
            if (a?.sharedMesh == null || b?.sharedMesh == null || a.sharedMesh.vertexCount != 4677 || b.sharedMesh.vertexCount != 4677 ||
                a.bones.Length != 55 || b.bones.Length != 55 || a.sharedMesh.bindposes.Length != 55 || b.sharedMesh.bindposes.Length != 55) return false;
            var aBinds = a.sharedMesh.bindposes; var bBinds = b.sharedMesh.bindposes;
            for (int i = 0; i < 55; i++)
            {
                if (a.bones[i] == null || b.bones[i] == null || !a.bones[i].IsChildOf(left.transform) || !b.bones[i].IsChildOf(right.transform) ||
                    BonePath(left.transform, a.bones[i]) != BonePath(right.transform, b.bones[i])) return false;
                for (int k = 0; k < 16; k++)
                    if (float.IsNaN(aBinds[i][k]) || float.IsInfinity(aBinds[i][k]) || float.IsNaN(bBinds[i][k]) ||
                        float.IsInfinity(bBinds[i][k]) || Mathf.Abs(aBinds[i][k] - bBinds[i][k]) > .00001f) return false;
            }
            return true;
        }

        private static string BonePath(Transform root, Transform bone)
        {
            var names = new List<string>();
            for (var next = bone; next != null && next != root; next = next.parent) names.Add(next.name);
            names.Reverse(); return string.Join("/", names.ToArray());
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class SatyrNativeWeaponAnimation32Attach
    {
        private static void Postfix(UnitEntityView __instance)
        {
            if (SatyrNativeWeaponAnimation32.Permits(__instance) && __instance.GetComponent<SatyrNativeWeaponAnimation32>() == null)
                __instance.gameObject.AddComponent<SatyrNativeWeaponAnimation32>().Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class SatyrNativeWeaponAnimation32Destroy
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null : __instance.GetComponent<SatyrNativeWeaponAnimation32>();
            if (owned != null)
                try { owned.Release(); } catch (Exception error) { Debug.LogException(error); }
        }
    }
}
