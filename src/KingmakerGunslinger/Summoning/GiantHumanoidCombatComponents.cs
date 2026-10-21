using System;
using System.Collections.Generic;
using System.Linq;
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
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;

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
        public int MobilityRanks;

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
            if (MobilityRanks > 0)
                SetRank(stats.GetStat(StatType.SkillMobility), MobilityRanks);
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

    /// <summary>The giant subtype makes Intimidate a PF1 class skill.
    /// Kingmaker's shared Persuasion stat does not carry that flag for the
    /// copied humanoid class. Grant +3 only on actual Intimidate checks so
    /// other Persuasion uses do not inherit an unprinted bonus.</summary>
    [Serializable]
    public sealed class SummonGiantIntimidateClassSkill :
        RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || evt.StatType != StatType.CheckIntimidate ||
                Owner == null || Owner.Unit == null || Fact == null ||
                !Fact.Active || !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, Owner.Unit))
                return;
            var skill = Owner.Stats.SkillPersuasion as ModifiableValueSkill;
            if (skill == null || skill.BaseValue <= 0 || skill.ClassSkill)
                return;
            evt.Bonus.AddModifier(3, this, ModifierDescriptor.UntypedStackable);
        }

        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    /// <summary>Cloud has different printed Diplomacy and Intimidate ranks,
    /// while Kingmaker combines them into Persuasion. The shared stat holds
    /// Diplomacy's eight ranks; these two additional ranks affect only an
    /// exact Cloud Giant's Intimidate check.</summary>
    [Serializable]
    public sealed class SummonCloudIntimidateRanks :
        RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || evt.StatType != StatType.CheckIntimidate ||
                Owner == null || Owner.Unit == null || Fact == null ||
                !Fact.Active || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint))
                return;
            evt.Bonus.AddModifier(2, this,
                ModifierDescriptor.UntypedStackable);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
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
        // Additional project-owned alternate weapons. Each identity is
        // compared by reference and only on this component's exact unit.
        public BlueprintItemWeapon RangedWeapon;
        public BlueprintItemWeapon OffhandWeapon;
        public BlueprintItemWeapon OffhandRangedWeapon;
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
            // Native body alternatives can sit in the owner's inventory
            // without ever occupying an active hand. Mark those exact items
            // during creation and load reactivation as well as hand switches.
            foreach (ItemEntity item in Owner.Inventory.Items.ToArray())
                Mark(item);
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
                !ReferenceEquals(item.Blueprint, RangedWeapon) &&
                !ReferenceEquals(item.Blueprint, OffhandWeapon) &&
                !ReferenceEquals(item.Blueprint, OffhandRangedWeapon) &&
                !ReferenceEquals(item.Blueprint, Armor))
                return;
            if (!ReferenceEquals(item.Owner, Owner)) return;
            item.MakeNotLootable();
            item.IsNonRemovable = true;
        }
    }

    /// <summary>A save-owned marker for the one initial pair of Ettin
    /// offhands. Later disarm and cold-load must not replenish it.</summary>
    [Serializable]
    public sealed class UnitPartSummonEttinInitialArms : UnitPart
    {
        [JsonProperty] private bool _flailSeeded;
        [JsonProperty] private bool _javelinSeeded;
        public bool FlailSeeded { get { return _flailSeeded; } }
        public bool JavelinSeeded { get { return _javelinSeeded; } }
        public void MarkFlailSeeded() { _flailSeeded = true; }
        public void MarkJavelinSeeded() { _javelinSeeded = true; }
    }

    /// <summary>Kingmaker initializes a versatile primary flail with a
    /// two-hand grip and omits the configured offhand. Wait for its equipped
    /// primary slot, then seed each exact private offhand once. Equipment
    /// updates provide the late initialization boundary; the serialized
    /// marker prevents rearming after disarm or cold load.</summary>
    [Serializable]
    public sealed class SummonEttinInitialOffhands :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon MainFlail;
        public BlueprintItemWeapon MainJavelin;
        public BlueprintItemWeapon OffhandFlail;
        public BlueprintItemWeapon OffhandJavelin;
        [NonSerialized] private bool _seeding;

        public override void OnTurnOn() { TrySeed(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        {
            TrySeed();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (ReferenceEquals(unit, Owner)) TrySeed();
        }

        private void TrySeed()
        {
            UnitEntityData unit = Owner == null ? null : Owner.Unit;
            if (_seeding || unit == null || unit.Body == null ||
                !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                unit.Body.HandsEquipmentSets.Count < 2)
                return;
            UnitPartSummonEttinInitialArms marker =
                unit.Ensure<UnitPartSummonEttinInitialArms>();
            bool flailReady = !marker.FlailSeeded &&
                ReferenceEquals(unit.Body.HandsEquipmentSets[0].PrimaryHand
                    .MaybeWeapon?.Blueprint, MainFlail);
            bool javelinReady = !marker.JavelinSeeded &&
                ReferenceEquals(unit.Body.HandsEquipmentSets[1].PrimaryHand
                    .MaybeWeapon?.Blueprint, MainJavelin);
            if (!flailReady && !javelinReady) return;
            _seeding = true;
            try
            {
                if (flailReady)
                {
                    Seed(unit, unit.Body.HandsEquipmentSets[0].SecondaryHand,
                        OffhandFlail);
                    marker.MarkFlailSeeded();
                }
                if (javelinReady)
                {
                    Seed(unit, unit.Body.HandsEquipmentSets[1].SecondaryHand,
                        OffhandJavelin);
                    marker.MarkJavelinSeeded();
                }
            }
            finally { _seeding = false; }
        }

        private static void Seed(UnitEntityData unit, WeaponSlot slot,
            BlueprintItemWeapon blueprint)
        {
            if (unit == null || slot == null || blueprint == null)
                throw new InvalidOperationException(
                    "Ettin offhand slot or private weapon is missing.");
            ItemEntityWeapon existing = slot.MaybeWeapon;
            if (existing != null)
            {
                if (!ReferenceEquals(existing.Blueprint, blueprint))
                    throw new InvalidOperationException(
                        "Ettin offhand has an unexpected item.");
                return;
            }
            // Reattach an existing exact private instance if native setup or
            // cold-load hydration has already placed it in this inventory.
            // The Ettin body has no competing secondary flail grant.
            ItemEntityWeapon[] candidates = unit.Descriptor.Inventory.Items
                .OfType<ItemEntityWeapon>().Where(value =>
                    ReferenceEquals(value.Blueprint, blueprint)).ToArray();
            if (candidates.Length > 1 || candidates.Any(value =>
                value.HoldingSlot != null))
                throw new InvalidOperationException(
                    "Ettin offhand inventory has ambiguous prior items.");
            var item = candidates.Length == 1 ? candidates[0] :
                new ItemEntityWeapon(blueprint);
            slot.InsertItem(item);
            if (!ReferenceEquals(slot.MaybeWeapon, item))
                throw new InvalidOperationException(
                    "Native Ettin offhand insertion did not retain its item.");
            item.MakeNotLootable();
            item.IsNonRemovable = true;
        }
    }

    /// <summary>Kingmaker reduces a 50-foot creature in medium armor to 40
    /// feet. Storm's printed breastplate speed is 35 feet. Apply the remaining
    /// five feet only while this exact project breastplate is equipped, so
    /// removing or losing the armor restores its native 50-foot land speed.</summary>
    [Serializable]
    public sealed class SummonStormBreastplateSpeed :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemArmor Breastplate;
        [NonSerialized] private ModifiableValue.Modifier _modifier;

        public override void OnTurnOn() { Refresh(); }

        public override void OnTurnOff()
        {
            if (_modifier != null) _modifier.Remove();
            _modifier = null;
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        {
            Refresh();
        }

        private void Refresh()
        {
            bool equipped = Owner != null && Owner.Unit != null &&
                ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) &&
                Owner.Body != null && Owner.Body.Armor != null &&
                Owner.Body.Armor.MaybeArmor != null &&
                ReferenceEquals(Owner.Body.Armor.MaybeArmor.Blueprint,
                    Breastplate);
            if (equipped && _modifier == null)
                _modifier = Owner.Stats.Speed.AddModifier(-5, this,
                    ModifierDescriptor.UntypedStackable);
            else if (!equipped && _modifier != null)
            {
                _modifier.Remove();
                _modifier = null;
            }
        }
    }
}
