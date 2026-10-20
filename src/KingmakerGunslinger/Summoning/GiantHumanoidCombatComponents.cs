using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Creation-only racial bases for an exact project-owned adult
    /// giant. The engine still applies live abilities, feats, items and
    /// conditions on top of these bases.</summary>
    [Serializable]
    public sealed class SummonGiantRacialProfile :
        OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public int BaseRacialHitPoints;
        public int BaseFortitude;
        public int BaseReflex;
        public int BaseWill;
        public int PerceptionRanks;
        public int PersuasionRanks;
        public int StealthRanks;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || OwningBlueprint == null ||
                !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                BaseRacialHitPoints < 1)
                throw new InvalidOperationException(
                    "Giant racial bases require their exact owned unit.");
            var stats = unit.Descriptor.Stats;
            stats.HitPoints.BaseValue = BaseRacialHitPoints;
            stats.SaveFortitude.BaseValue = BaseFortitude;
            stats.SaveReflex.BaseValue = BaseReflex;
            stats.SaveWill.BaseValue = BaseWill;
            SetRank(stats.GetStat(StatType.SkillPerception), PerceptionRanks);
            SetRank(stats.GetStat(StatType.SkillPersuasion), PersuasionRanks);
            SetRank(stats.GetStat(StatType.SkillStealth), StealthRanks);
        }

        private static void SetRank(ModifiableValue stat, int ranks)
        {
            if (stat == null || stat.BaseValue != 0 || ranks < 0)
                throw new InvalidOperationException(
                    "Giant skill ranks require an unallocated nonnegative base.");
            stat.BaseValue = ranks;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    /// <summary>Stone's printed conditional racial Stealth bonus applies to
    /// native Stealth checks in the two owner-selected area settings only.
    /// It never changes Reflex, another skill or another giant's stat.</summary>
    [Serializable]
    public sealed class SummonStoneRockyStealth :
        RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || evt.StatType != StatType.SkillStealth ||
                Owner == null || Owner.Unit == null || Fact == null ||
                !Fact.Active || !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, Owner.Unit))
                return;
            var area = Game.Instance == null ? null :
                Game.Instance.CurrentlyLoadedArea;
            if (area == null ||
                !GiantHumanoidRulesPolicy.IsStoneRockyTerrain(
                    area.LootSetting.ToString()))
                return;
            evt.Bonus.AddModifier(
                GiantHumanoidRulesPolicy.StoneRockyStealthBonus,
                this, ModifierDescriptor.Racial);
        }

        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    /// <summary>Only exact giant equipment instances are marked nonlootable.
    /// Creation, equipment changes, active-hand switches and load reactivation
    /// are covered; no party or donor item is modified.</summary>
    [Serializable]
    public sealed class SummonGiantEquipmentOwnership :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon GreatWeapon;
        public BlueprintItemWeapon Slam;
        public BlueprintItemWeapon Rock;
        public BlueprintItemArmor Armor;

        public override void OnTurnOn() { Sweep(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        {
            Sweep();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (ReferenceEquals(unit, Owner)) Sweep();
        }

        private void Sweep()
        {
            if (Owner == null || Owner.Unit == null ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                Owner.Body == null)
                return;
            foreach (HandsEquipmentSet set in Owner.Body.HandsEquipmentSets)
            {
                if (set == null) continue;
                Mark(set.PrimaryHand == null ? null : set.PrimaryHand.MaybeWeapon);
                Mark(set.SecondaryHand == null ? null : set.SecondaryHand.MaybeWeapon);
            }
            foreach (WeaponSlot slot in Owner.Body.AdditionalLimbs)
                Mark(slot == null ? null : slot.MaybeWeapon);
            Mark(Owner.Body.EmptyHandWeapon);
            Mark(Owner.Body.Armor == null ? null : Owner.Body.Armor.MaybeArmor);
        }

        private void Mark(ItemEntity item)
        {
            if (item == null || item.Blueprint == null) return;
            if (!ReferenceEquals(item.Blueprint, GreatWeapon) &&
                !ReferenceEquals(item.Blueprint, Slam) &&
                !ReferenceEquals(item.Blueprint, Rock) &&
                !ReferenceEquals(item.Blueprint, Armor))
                return;
            if (!ReferenceEquals(item.Owner, Owner)) return;
            item.MakeNotLootable();
            item.IsNonRemovable = true;
        }
    }
}
