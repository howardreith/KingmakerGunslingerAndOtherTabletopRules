using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class BarbazuBeard34 :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Claw;
        public BlueprintBuff DevilChills;
        private sealed class AttackStart { internal object Command; }
        private sealed class State
        {
            internal readonly BarbazuBeardSequence34 Sequence = new BarbazuBeardSequence34();
            internal readonly ConditionalWeakTable<RuleAttackWithWeapon, AttackStart> Starts =
                new ConditionalWeakTable<RuleAttackWithWeapon, AttackStart>();
        }
        private static readonly ConditionalWeakTable<UnitEntityData, State> States =
            new ConditionalWeakTable<UnitEntityData, State>();

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            var owner = Mine(evt);
            if (owner == null || Limb(owner, evt.Weapon) == 0) return;
            var state = States.GetValue(owner, unit => new State());
            AttackStart previous;
            if (!state.Starts.TryGetValue(evt, out previous))
                state.Starts.Add(evt, new AttackStart { Command = owner.Commands?.Attack });
        }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var owner = Mine(evt);
            if (owner == null || evt.Target == null) return;
            int limb = Limb(owner, evt.Weapon);
            State state;
            AttackStart start;
            if (limb == 0 || !States.TryGetValue(owner, out state) ||
                !state.Starts.TryGetValue(evt, out start) ||
                !state.Sequence.Record(evt, start.Command, owner.Commands?.Attack,
                    limb, evt.Target, evt.AttackRoll?.IsHit == true)) return;
            // This is its own damage packet, not another attack roll or a
            // rider multiplied by the claw's critical. Native DR still resolves.
            var damage = new PhysicalDamage(new DiceFormula(1, DiceType.D8),
                PhysicalDamageForm.Piercing);
            damage.AddBonus(InfernalRules34.BeardStrengthBonus(owner.Stats.Strength.Bonus));
            damage.AddAlignment(DamageAlignment.Lawful);
            damage.AddAlignment(DamageAlignment.Evil);
            var rule = new RuleDealDamage(owner, evt.Target, new DamageBundle(damage));
            var context = Fact?.MaybeContext;
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
            if (rule.Damage <= 0 || DevilChills == null ||
                !SummonDiseaseExposure.IsAvailable(evt.Target)) return;
            int dc = InfernalRules34.BeardDiseaseDc(owner.Stats.Constitution.Bonus);
            var exposure = AstralVictimEffectContext.Create(owner, evt.Target, DevilChills,
                new AbilityParams { DC = dc, CasterLevel = 6, SpellLevel = 0 });
            var saving = new RuleSavingThrow(evt.Target, SavingThrowType.Fortitude, dc);
            exposure.TriggerRule(saving);
            if (saving.IsPassed || evt.Target.Buffs.Enumerable.Any(buff =>
                ReferenceEquals(buff.Blueprint, DevilChills) && ReferenceEquals(buff.Context?.MaybeCaster, owner))) return;
            var apply = new RuleApplyBuff(evt.Target, DevilChills, exposure, null,
                (blueprint, parent, duration) => evt.Target.Buffs.AddBuff(blueprint, parent, duration));
            Rulebook.Trigger(apply);
        }

        private UnitEntityData Mine(RuleAttackWithWeapon evt)
        {
            var owner = Owner?.Unit;
            return evt != null && owner != null && OwningBlueprint != null &&
                Claw != null && Fact?.Active == true &&
                ReferenceEquals(owner.Blueprint, OwningBlueprint) &&
                ReferenceEquals(evt.Initiator, owner) ? owner : null;
        }

        private int Limb(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (weapon == null || !ReferenceEquals(weapon.Blueprint, Claw)) return 0;
            int index;
            var kind = SummonLimbs.Classify(owner, weapon, out index);
            if (kind == SummonLimbKind.PrimaryHand) return 1;
            return kind == SummonLimbKind.Additional && owner.Body.AdditionalLimbs.Count(
                slot => ReferenceEquals(slot.MaybeWeapon?.Blueprint, Claw)) == 1 ? 2 : 0;
        }

        public void OnNewRound()
        {
            State state;
            if (Owner?.Unit != null && States.TryGetValue(Owner.Unit, out state))
                state.Sequence.Reset();
        }

        public override void OnTurnOff()
        {
            if (Owner?.Unit != null) States.Remove(Owner.Unit);
            base.OnTurnOff();
        }
    }
}
