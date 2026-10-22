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
    public sealed class HoundArchonSwordView : MonoBehaviour
    {
        internal const string SwordGuid = "99fa49115f9c48228e13ca9f2879e6e0";
        private UnitEntityView _view;
        private UnitEntityData _owner;
        private GameObject _model;
        internal GameObject Model { get { return _model; } }
        internal string Status { get; private set; }

        internal void Configure(UnitEntityView view, SkinnedMeshRenderer body)
        {
            _owner = view.EntityData as UnitEntityData;
            if (_owner == null || _owner.Blueprint.AssetGuid != HoundArchonVisualPolicy.BlueprintGuid ||
                _owner.Blueprint.name != HoundArchonVisualPolicy.BlueprintName ||
                _owner.Blueprint.Prefab?.AssetId != TheropodVisualPolicy.DonorPrefabGuid)
                throw new InvalidOperationException("Owned Hound sword view identity mismatch.");
            // The blueprint default, rather than the current item, also works
            // on a cold load in natural/disarmed mode. No item is created here.
            var sword = _owner.Blueprint.Body.PrimaryHand as BlueprintItemWeapon;
            if (sword == null || sword.AssetGuid != SwordGuid || sword.VisualParameters.Model == null)
                throw new InvalidOperationException("Owned Hound sword native model dependency absent.");
            var palm = body.bones.Single(value => value.name == "R_Palm");
            _view = view;
            _model = UnityEngine.Object.Instantiate(sword.VisualParameters.Model);
            if (_model == null) throw new InvalidOperationException("Owned Hound native sword model did not instantiate.");
            _model.name = "KMG_HoundOwnedSword";
            _model.transform.SetParent(palm, false);
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
            _model.transform.localScale = Vector3.one;
            // Only this clone's optional legacy snap can write its transform.
            foreach (var snap in _model.GetComponentsInChildren<WeaponSnap>(true)) snap.enabled = false;
            Status = "native-model:owned-palm";
            Refresh();
        }
        private void LateUpdate() { Refresh(); }
        private void Refresh()
        {
            if (_model == null) return;
            var weapon = _owner == null || _owner.Destroyed ? null : _owner.Body.PrimaryHand.MaybeWeapon;
            bool visible = _view != null && _owner != null && !_owner.Destroyed && _owner.IsInState &&
                weapon != null && weapon.Blueprint.AssetGuid == SwordGuid && weapon.Wielder != null &&
                ReferenceEquals(weapon.Wielder.Unit, _owner) && weapon.HoldingSlot != null &&
                ReferenceEquals(weapon.HoldingSlot.MaybeItem, weapon) && !weapon.IsLootable;
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
