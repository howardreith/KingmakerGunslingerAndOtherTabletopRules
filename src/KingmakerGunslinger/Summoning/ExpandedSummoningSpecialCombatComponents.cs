using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class BebelithCombatComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ConditionalWeakTable<UnitEntityData, OwnerState>
            States = new ConditionalWeakTable<UnitEntityData, OwnerState>();

        public BlueprintItemWeapon Claw;
        public BlueprintItemWeapon Bite;
        public BlueprintUnitFact OutsiderType;
        public BlueprintBuff DismantledArmor;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || !IsNaturalWeapon(evt.Weapon == null ? null :
                    evt.Weapon.Blueprint) || !IsDemon(evt.Target)) return;
            evt.SetAttackBonusPenalty(evt.AttackBonusPenalty -
                ExpandedSummoningSpecialProfiles.BebelithDemonHunterBonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.AttackWithWeapon == null ||
                !IsNaturalWeapon(evt.Weapon == null ? null : evt.Weapon.Blueprint) ||
                !IsDemon(evt.AttackWithWeapon.Target)) return;
            evt.AddBonusDamage(
                ExpandedSummoningSpecialProfiles.BebelithDemonHunterBonus);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || !evt.AttackRoll.IsHit ||
                evt.Target == null || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Claw) || Owner == null ||
                Owner.Unit == null || DismantledArmor == null) return;
            bool hasArmor = evt.Target.Body != null &&
                evt.Target.Body.Armor != null && evt.Target.Body.Armor.HasArmor;
            bool attempt;
            lock (States)
            {
                OwnerState state = States.GetOrCreateValue(Owner.Unit);
                int priorHits = state.HitCount(evt.Target);
                attempt = ExpandedSummoningSpecialProfiles
                    .ShouldAttemptBebelithDismantle(true, true, hasArmor,
                        priorHits, state.WasAttempted(evt.Target));
                state.RecordHit(evt.Target);
                if (attempt) state.MarkAttempted(evt.Target);
            }
            if (!attempt) return;
            var saving = new RuleSavingThrow(evt.Target,
                SavingThrowType.Reflex,
                ExpandedSummoningSpecialProfiles.BebelithDismantleReflexDc);
            Rulebook.Trigger(saving);
            if (saving.IsPassed || evt.Target.Descriptor.HasFact(
                    DismantledArmor)) return;
            evt.Target.Descriptor.Buffs.AddBuff(DismantledArmor,
                Fact == null ? null : Fact.MaybeContext,
                TimeSpan.FromSeconds(6d * ExpandedSummoningSpecialProfiles
                    .BebelithDismantleRounds));
        }

        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null) return;
            lock (States) { States.Remove(Owner.Unit); }
        }

        private bool IsNaturalWeapon(BlueprintItemWeapon weapon)
        { return weapon != null && (ReferenceEquals(weapon, Claw) ||
            ReferenceEquals(weapon, Bite)); }

        private bool IsDemon(UnitEntityData target)
        {
            return target != null && target.Descriptor != null &&
                OutsiderType != null && target.Descriptor.HasFact(OutsiderType) &&
                target.Descriptor.Alignment != null &&
                ExpandedSummoningSpecialProfiles.IsBebelithDemonHuntingTarget(
                    true, (int)target.Descriptor.Alignment.Value);
        }

        private sealed class OwnerState
        {
            private readonly Dictionary<UnitEntityData, TargetState> _targets =
                new Dictionary<UnitEntityData, TargetState>(
                    ReferenceComparer.Instance);

            internal int HitCount(UnitEntityData target)
            { TargetState value; return _targets.TryGetValue(target, out value) ?
                value.Hits : 0; }

            internal bool WasAttempted(UnitEntityData target)
            { TargetState value; return _targets.TryGetValue(target, out value) &&
                value.Attempted; }

            internal void RecordHit(UnitEntityData target)
            { Get(target).Hits++; }

            internal void MarkAttempted(UnitEntityData target)
            { Get(target).Attempted = true; }

            private TargetState Get(UnitEntityData target)
            {
                TargetState value;
                if (!_targets.TryGetValue(target, out value))
                    _targets.Add(target, value = new TargetState());
                return value;
            }
        }

        private sealed class TargetState
        { internal int Hits; internal bool Attempted; }

        private sealed class ReferenceComparer : IEqualityComparer<UnitEntityData>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(UnitEntityData left, UnitEntityData right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(UnitEntityData value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }

    [Serializable]
    public sealed class PixieSleepArrowComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintItemWeapon SleepBow;
        public BlueprintAbilityResource SleepArrowResource;
        public BlueprintBuff SleepingBuff;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || Owner == null ||
                Owner.Unit == null || evt.Target == null || evt.Weapon == null ||
                SleepBow == null || SleepArrowResource == null ||
                SleepingBuff == null) return;
            int remaining = Owner.Resources.GetResourceAmount(SleepArrowResource);
            if (!ExpandedSummoningSpecialProfiles.ShouldSpendPixieSleepArrow(
                    ReferenceEquals(evt.Weapon.Blueprint, SleepBow),
                    evt.AttackRoll.IsHit, remaining)) return;
            bool spent = false;
            Buff applied = null;
            try
            {
                Owner.Resources.Spend(SleepArrowResource, 1);
                spent = true;
                var saving = new RuleSavingThrow(evt.Target,
                    SavingThrowType.Will,
                    ExpandedSummoningSpecialProfiles.PixieSleepArrowWillDc);
                Rulebook.Trigger(saving);
                if (saving.IsPassed) return;
                applied = evt.Target.Descriptor.Buffs.AddBuff(SleepingBuff,
                    Fact == null ? null : Fact.MaybeContext,
                    TimeSpan.FromSeconds(6d * ExpandedSummoningSpecialProfiles
                        .PixieSleepArrowRounds));
                if (applied == null)
                    throw new InvalidOperationException(
                        "The native Sleeping buff rejected a Pixie sleep arrow.");
            }
            catch
            {
                if (applied != null)
                    evt.Target.Descriptor.Buffs.RemoveFact(applied);
                if (spent) Owner.Resources.Restore(SleepArrowResource, 1);
            }
        }
    }
}
