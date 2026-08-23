using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.EasternWeapons
{
    /// <summary>
    /// Independent KMG equivalent of Favored Class's installed +1 trait bonus
    /// choice.  It is scoped to an actual Nodachi attack of opportunity and has
    /// no reference to either optional assembly.
    /// </summary>
    [Serializable]
    internal sealed class HeirloomNodachiOpportunityBonus :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null ||
                evt.RuleAttackWithWeapon == null ||
                !evt.RuleAttackWithWeapon.IsAttackOfOpportunity ||
                (int)evt.Weapon.Blueprint.Type.Category !=
                    EasternWeaponMartialPublicationPolicy.NodachiCategoryValue)
                return;
            evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus
                .AddModifier(1, Fact, GetType().FullName,
                    ModifierDescriptor.Trait));
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }
    }

    /// <summary>
    /// Applies the hidden +2 CMB fact only while the owner actively wields a
    /// Nodachi.  The applied fact is serialized just as the installed foreign
    /// component serializes its owned feature instance.
    /// </summary>
    [Serializable]
    internal sealed class HeirloomNodachiCombatManeuverCarrier :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintFeature Feature;
        [JsonProperty]
        private Fact _appliedFact;

        public override void OnTurnOn() { Apply(); }

        public override void OnTurnOff() { Remove(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        {
            if (slot != null && ReferenceEquals(slot.Owner, Owner)) Apply();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (ReferenceEquals(unit, Owner)) Apply();
        }

        private void Apply()
        {
            Remove();
            if (Owner == null || Owner.Body == null ||
                Owner.Body.IsPolymorphed || Feature == null ||
                !IsNodachi(Owner.Body.PrimaryHand == null ? null :
                    Owner.Body.PrimaryHand.MaybeWeapon) &&
                !IsNodachi(Owner.Body.SecondaryHand == null ? null :
                    Owner.Body.SecondaryHand.MaybeWeapon)) return;
            _appliedFact = Owner.AddFact(Feature, null, null);
        }

        private void Remove()
        {
            if (_appliedFact != null && Owner != null)
                Owner.RemoveFact(_appliedFact);
            _appliedFact = null;
        }

        private static bool IsNodachi(ItemEntityWeapon weapon)
        {
            return weapon != null && weapon.Blueprint != null &&
                weapon.Blueprint.Type != null &&
                (int)weapon.Blueprint.Type.Category ==
                    EasternWeaponMartialPublicationPolicy.NodachiCategoryValue;
        }
    }
}
