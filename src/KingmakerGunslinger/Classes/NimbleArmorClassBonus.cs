using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Items.Armors;

namespace KingmakerGunslinger.Classes
{
    public sealed class NimbleArmorClassBonus : OwnedGameLogicComponent<UnitDescriptor>,
        IUnitEquipmentHandler, IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        private ModifiableValue.Modifier _modifier;

        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { Remove(); }
        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (Owner != null && Owner.Body != null &&
                ReferenceEquals(slot, Owner.Body.Armor)) Refresh();
        }
        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (ReferenceEquals(unit, Owner)) Refresh();
        }
        private void Refresh()
        {
            Remove();
            if (Owner == null || Owner.Stats == null || Owner.Body == null) return;
            bool eligible = !Owner.Body.Armor.HasArmor ||
                Owner.Body.Armor.Armor.Blueprint.Type.ProficiencyGroup ==
                    ArmorProficiencyGroup.Light;
            if (eligible)
                _modifier = Owner.Stats.AC.AddModifier(1, Fact,
                    GetType().FullName, ModifierDescriptor.Dodge);
        }
        private void Remove()
        {
            if (_modifier != null && Owner != null && Owner.Stats != null)
                Owner.Stats.AC.RemoveModifier(_modifier);
            _modifier = null;
        }
    }
}
