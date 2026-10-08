using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class SummonSerpentineSkillRanks :
        OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public string CreatureKey;
        public BlueprintUnit OwningBlueprint;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException("Snake ranks require their exact owner.");
            var mobility = unit.Descriptor.Stats.GetStat(StatType.SkillMobility);
            var perception = unit.Descriptor.Stats.GetStat(StatType.SkillPerception);
            var stealth = unit.Descriptor.Stats.GetStat(StatType.SkillStealth);
            int m = mobility.BaseValue, p = perception.BaseValue, s = stealth.BaseValue;
            SerpentineRulesPolicy.AllocateLandRanks(CreatureKey, ref m, ref p, ref s);
            mobility.BaseValue = m; perception.BaseValue = p; stealth.BaseValue = s;
            unit.Descriptor.Stats.HitPoints.BaseValue =
                SerpentineRulesPolicy.For(CreatureKey).BaseHitPoints;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    /// <summary>
    /// Scope the explicit Medium bite dice to these two new units only.
    /// Native single-natural-attack Strength handling remains untouched.
    /// </summary>
    [Serializable]
    public sealed class SummonSerpentineWeaponStats :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (Owner == null || Owner.Unit == null || evt == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                evt.Weapon == null || !ReferenceEquals(evt.Weapon.Blueprint, Bite) ||
                evt.WeaponDamageDiceOverride.HasValue) return;
            evt.WeaponDamageDiceOverride = WeaponDamageScaleTable.Scale(
                Bite.BaseDamage, (Size)SerpentineRulesPolicy.LiveWeaponSize(
                    (int)Owner.State.Size, (int)evt.Weapon.Size, (int)evt.WeaponSize),
                Size.Medium, Bite);
            evt.DoNotScaleDamage = true;
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    [Serializable]
    public sealed class ContextActionSetViperPoisonDc : ContextAction
    {
        public override string GetCaption() { return "Set Viper poison DC"; }
        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null || Context.Params == null)
                return;
            Context.Params.DC = SerpentineRulesPolicy.ViperPoisonDifficultyClass(
                Context.MaybeCaster.Stats.Constitution.Bonus);
        }
    }
}
