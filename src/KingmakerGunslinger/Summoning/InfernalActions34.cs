using System;
using System.Linq;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    // Native SwitchMode only distinguishes melee/ranged. These two private
    // melee sets need selection by their actual weapon identity. Execution is
    // still the native switch command, including its ordinary action cost.
    public sealed class BarbazuWeaponModeAi34 : BlueprintAiSwitchWeapon
    {
        public BlueprintUnit OwningBlueprint;
        private int DesiredSet(UnitEntityData owner)
        {
            var profile = InfernalMartialProfile34.For(owner);
            if (profile?.Glaive == null || profile.Claw == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) || !owner.IsInCombat ||
                Game.Instance?.State?.Units == null) return -1;
            float nearest = float.PositiveInfinity;
            foreach (var enemy in Game.Instance.State.Units.All) {
                if (enemy == null || enemy.Destroyed || !enemy.IsInState || enemy.Descriptor.State.IsDead ||
                    !owner.IsEnemy(enemy) || !owner.Memory.ContainsVisible(enemy) || !owner.HasLOS(enemy)) continue;
                nearest = Math.Min(nearest, owner.DistanceTo(enemy) - owner.Corpulence - enemy.Corpulence);
            }
            if (float.IsPositiveInfinity(nearest)) return -1;
            bool disarmed = owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null;
            var wanted = nearest <= new Feet(5).Meters || disarmed ? profile.Claw : profile.Glaive;
            if (ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, wanted)) return -1;
            // Native MaybeWeapon returns EmptyHandWeapon for an inactive slot.
            // Inspect the actual held item when choosing a set, then let the
            // native switch activate it with its ordinary cost.
            for (int index = 0; index < owner.Body.HandsEquipmentSets.Count; index++)
                if (ReferenceEquals(owner.Body.HandsEquipmentSets[index].PrimaryHand.MaybeItem?.Blueprint, wanted)) return index;
            return -1;
        }
        public override bool IsAvailable(DecisionContext context)
        { return DesiredSet(context?.Unit) >= 0; }
        public override bool IsExpended(DecisionContext context) { return false; }
        public override UnitCommand CreateCommand(DecisionContext context, UnitEntityData target)
        {
            int index = DesiredSet(context?.Unit);
            return index < 0 ? null : new UnitSwitchHandEquipmentSet(index);
        }
    }

    // Named PF1 Intimidate ranks/class skill; never a general Persuasion grant.
    [Serializable]
    public sealed class InfernalIntimidate34 : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public int TrainedContribution;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt?.StatType != StatType.CheckIntimidate || Fact?.Active != true ||
                Owner?.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint)) return;
            evt.Bonus.AddModifier(TrainedContribution, this, ModifierDescriptor.UntypedStackable);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    public sealed class KytonControlAi34 : BlueprintAiCastSpell
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Controlled;
        public override bool IsAvailable(DecisionContext context)
        {
            var owner = context?.Unit;
            return owner != null && ReferenceEquals(owner.Blueprint, OwningBlueprint) && owner.IsInCombat &&
                !owner.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Controlled) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, owner)) && base.IsAvailable(context);
        }
        public override TargetType GetTargetType(DecisionContext context) { return TargetType.Self; }
    }

    public sealed class KytonGazeAi34 : BlueprintAiCastSpell
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Shaken;
        public override TargetType GetTargetType(DecisionContext context) { return TargetType.Enemy; }
        public override bool IsTargetValid(DecisionContext context)
        {
            var owner = context?.Unit; var target = context?.Target?.Unit;
            return owner != null && target != null && ReferenceEquals(owner.Blueprint, OwningBlueprint) &&
                owner.IsEnemy(target) && !target.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Shaken)) &&
                LillendPerformanceSight32.Sees(target, owner, InfernalRules34.GazeRangeFeet) && base.IsTargetValid(context);
        }
    }
    public sealed class KytonGazeSight34 : BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            var patient = target?.Unit;
            return caster != null && patient != null && !ReferenceEquals(caster, patient) &&
                !patient.Destroyed && !patient.Descriptor.State.IsDead &&
                LillendPerformanceSight32.Sees(patient, caster, InfernalRules34.GazeRangeFeet);
        }
    }
}
