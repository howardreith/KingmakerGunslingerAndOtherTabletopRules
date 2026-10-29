using System;
using System.Linq;
using Kingmaker.Assets.Visual;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // The measured Free Hands rig has no Character avatar. Native hand equipment
    // explicitly refuses such rigs, so this exact summon instantiates only its
    // existing weapon's native visual under its measured right palm. This is
    // presentation, never an item, attack, animation or loot replacement.
    public sealed class BarbazuGlaiveView34 : MonoBehaviour
    {
        internal const string GlaiveGuid = "804dc1aecc9f43b9bcad0c48547c8b28";
        // Native WP_GlaiveBandits is lengthwise local Y. Body04 measured its
        // contact basis and the target surface: map that axis into the palm's
        // thrust direction, retaining the native rig animation and weapon mesh.
        internal static readonly Vector3 GripPosition = new Vector3(-0.2389f, 0.075f, 0.0997f);
        internal static readonly Quaternion GripRotation =
            new Quaternion(0.521334f, 0.521334f, 0.477714f, 0.477714f);
        private static readonly Quaternion BladeAxisCorrection =
            Quaternion.FromToRotation(Vector3.up, new Vector3(.317f, -.495f, -.863f).normalized);
        private UnitEntityView _view;
        private UnitEntityData _owner;
        private GameObject _model;
        internal GameObject Model { get { return _model; } }
        internal string Status { get; private set; }
        internal int LateUpdates { get; private set; }
        internal int ObservedEquipmentSet { get; private set; }
        internal string ObservedWeaponGuid { get; private set; }
        internal int LastRefreshUnityFrame { get; private set; }


        internal void Configure(UnitEntityView view, SkinnedMeshRenderer body)
        {
            _owner = view.EntityData as UnitEntityData;
            if (_owner == null || _owner.Blueprint.AssetGuid != "182032f380314a7581d0016c443945a2" ||
                _owner.Blueprint.name != "KMG_Summoning_Unit_BeardedDevil" ||
                _owner.Blueprint.Prefab?.AssetId != TheropodVisualPolicy.DonorPrefabGuid)
                throw new InvalidOperationException("Owned Barbazu glaive view identity mismatch.");
            // The blueprint default, rather than the current item, also works
            // on a cold load in natural/disarmed mode. No item is created here.
            var glaive = _owner.Blueprint.Body.PrimaryHand as BlueprintItemWeapon;
            if (glaive == null || glaive.AssetGuid != GlaiveGuid || glaive.VisualParameters.Model == null)
                throw new InvalidOperationException("Owned Barbazu glaive native model dependency absent.");
            var palm = body.bones.Single(value => value.name == "R_Palm");
            _view = view;
            _model = UnityEngine.Object.Instantiate(glaive.VisualParameters.Model);
            if (_model == null) throw new InvalidOperationException("Owned Barbazu native glaive model did not instantiate.");
            _model.name = "KMG_BarbazuOwnedGlaive";
            _model.transform.SetParent(palm, false);
            _model.transform.localPosition = GripPosition;
            _model.transform.localRotation = GripRotation;
            _model.transform.localScale = Vector3.one;
            var blade = _model.GetComponentsInChildren<MeshFilter>(true).Single(value => value.sharedMesh != null);
            Quaternion meshFrame = Quaternion.Inverse(_model.transform.rotation) * blade.transform.rotation;
            _model.transform.localRotation = GripRotation * meshFrame * BladeAxisCorrection * Quaternion.Inverse(meshFrame);
            // Only this clone's optional legacy snap can write its transform.
            foreach (var snap in _model.GetComponentsInChildren<WeaponSnap>(true)) snap.enabled = false;
            Status = "native-model:owned-palm";
            Refresh();
        }
        private void LateUpdate() { Refresh(); LateUpdates++; }
        private void Refresh()
        {
            if (_model == null) return;
            var weapon = _owner == null || _owner.Destroyed ? null : _owner.Body.PrimaryHand.MaybeWeapon;
            bool visible = _view != null && _owner != null && !_owner.Destroyed && _owner.IsInState &&
                weapon != null && weapon.Blueprint.AssetGuid == GlaiveGuid && weapon.Wielder != null &&
                ReferenceEquals(weapon.Wielder.Unit, _owner) && weapon.HoldingSlot != null &&
                ReferenceEquals(weapon.HoldingSlot.MaybeItem, weapon) && !weapon.IsLootable;
            ObservedEquipmentSet = _owner == null || _owner.Destroyed ? -1 : _owner.Body.CurrentHandEquipmentSetIndex;
            ObservedWeaponGuid = weapon?.Blueprint?.AssetGuid;
            LastRefreshUnityFrame = Time.frameCount;
            if (_model.activeSelf != visible) _model.SetActive(visible);
        }
        internal void Release()
        {
            enabled = false;
            if (_model != null) UnityEngine.Object.DestroyImmediate(_model);
            _model = null; _view = null; _owner = null; Status = "released";
        }
        private void OnDestroy() { Release(); }
    }
}
