using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class SummonSalamanderRacialProfile :
        OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                !SalamanderRulesPolicy.IsOwner(unit.Blueprint.AssetGuid, unit.Blueprint.name))
                throw new InvalidOperationException("Salamander racial profile requires its exact owner.");
            var stats = unit.Descriptor.Stats;
            int m = stats.SkillMobility.BaseValue, p = stats.SkillPerception.BaseValue,
                d = stats.SkillPersuasion.BaseValue;
            SalamanderRulesPolicy.AllocateLandRanks(ref m, ref p, ref d);
            stats.SkillPerception.BaseValue = p;
            stats.HitPoints.BaseValue = SalamanderRulesPolicy.BaseHitPoints;
            // Salamander has good Fortitude/Reflex and poor Will. The generic
            // outsider donor does not choose that creature-specific pair.
            // Change racial bases only, once at creation, never live totals.
            stats.SaveFortitude.BaseValue = SalamanderRulesPolicy.GoodSave;
            stats.SaveReflex.BaseValue = SalamanderRulesPolicy.GoodSave;
            stats.SaveWill.BaseValue = SalamanderRulesPolicy.PoorSave;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    [Serializable]
    public sealed class SummonSalamanderHeat : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        private static readonly SalamanderHeatClaims Claims = new SalamanderHeatClaims();
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Spear;
        public BlueprintItemWeapon Tail;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.DamageDescription == null || evt.Weapon == null || Owner == null ||
                Owner.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit)) return;
            bool exactOwner = ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) &&
                SalamanderRulesPolicy.IsOwner(Owner.Unit.Blueprint.AssetGuid, Owner.Unit.Blueprint.name);
            bool exactWeapon = ReferenceEquals(evt.Weapon.Blueprint, Spear) ||
                ReferenceEquals(evt.Weapon.Blueprint, Tail);
            if (!Claims.TryClaim(evt, exactOwner, exactWeapon)) return;
            evt.DamageDescription.Add(new DamageDescription {
                Dice = new DiceFormula(1, DiceType.D6),
                TypeDescription = new DamageTypeDescription { Type = DamageType.Energy, Energy = DamageEnergyType.Fire }
            });
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    internal static class SalamanderConstrictDamage
    {
        internal static void Deal(UnitEntityData owner, UnitEntityData target,
            ItemEntityWeapon tail, MechanicsContext context)
        {
            if (tail == null || tail.Blueprint == null || tail.Blueprint.AssetGuid != SalamanderRulesPolicy.TailGuid)
                return;
            DiceFormula dice = WeaponDamageScaleTable.Scale(new DiceFormula(2, DiceType.D6),
                owner.Descriptor.State.Size, Kingmaker.Enums.Size.Medium, tail.Blueprint);
            var physical = new PhysicalDamage(dice, PhysicalDamageForm.Bludgeoning);
            physical.AddBonus(SalamanderRulesPolicy.ConstrictStrengthBonus(owner.Descriptor.Stats.Strength.Bonus));
            var bundle = new DamageBundle(physical, new EnergyDamage(new DiceFormula(1, DiceType.D6), DamageEnergyType.Fire));
            bundle.Weapon = tail;
            var rule = new RuleDealDamage(owner, target, bundle);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
        }
    }
}
