using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Classes;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// G06 (Halfling) and G17 (Drow): the owner's earned Nimble improvement,
    /// as one owned Dodge AC modifier under exactly Nimble's own conditions.
    /// It exists only while the owner actually has Nimble (a dormant earlier
    /// investment wakes when Nimble is gained: the full leaf reapplies on
    /// level-up) and only in light or no armor, sharing Nimble's own armor
    /// predicate. As a Dodge-descriptor modifier it is lost with the
    /// Dexterity bonus to AC exactly as Nimble's own modifier is.
    /// </summary>
    public sealed class FavoredClassNimbleArmorClassBonus : OwnedGameLogicComponent<UnitDescriptor>,
        IUnitEquipmentHandler, IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public int Divisor = 4;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        /// <summary>The first Nimble feature; its presence means Nimble is active.</summary>
        public BlueprintFeature Nimble;

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
            Fact fact = Fact;
            if (fact == null || Owner == null || Owner.Stats == null || Owner.Body == null ||
                Nimble == null || !FavoredClassRuntime.MechanicsEnabled ||
                !Owner.Progression.Features.HasFact(Nimble) ||
                !NimbleArmorClassBonus.IsEligibleArmor(Owner))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            if (earned > 0)
                _modifier = Owner.Stats.AC.AddModifier(earned, fact, GetType().FullName,
                    ModifierDescriptor.Dodge);
        }

        private void Remove()
        {
            if (_modifier != null && Owner != null && Owner.Stats != null)
                Owner.Stats.AC.RemoveModifier(_modifier);
            _modifier = null;
        }
    }
}
