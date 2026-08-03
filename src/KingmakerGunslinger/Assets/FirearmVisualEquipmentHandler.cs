using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Actions;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    public sealed class FirearmVisualEquipmentHandler : OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler, IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        private readonly List<Renderer> _hidden = new List<Renderer>();
        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { Clear(); }
        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        { if (Owner != null && Owner.Body != null && (ReferenceEquals(slot, Owner.Body.PrimaryHand) || ReferenceEquals(slot, Owner.Body.SecondaryHand))) Refresh(); }
        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit) { if (ReferenceEquals(unit, Owner)) Refresh(); }
        private void Refresh()
        {
            Clear();
            if (Owner == null || Owner.Unit == null || Owner.Unit.View == null) return;
            ExactEquippedFirearmContext firearm; string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(Owner, out firearm, out reason)) return;
            foreach (Renderer renderer in Owner.Unit.View.GetComponentsInChildren<Renderer>(true))
            {
                string name = renderer.gameObject.name.ToLowerInvariant();
                if (name.Contains("quiver") || name.Contains("arrow") ||
                    name.Contains("crossbow") || name.Contains("bolt"))
                {
                    renderer.enabled = false;
                    _hidden.Add(renderer);
                }
            }
        }
        private void Clear()
        {
            foreach (Renderer renderer in _hidden) if (renderer != null) renderer.enabled = true;
            _hidden.Clear();
        }
    }
}
