using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

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

    /// <summary>
    /// Cyclops Flash of Insight, bounded (Sprint 3). Lives on the one-round
    /// armed state the ability applies: the owner's next attack is an
    /// automatic critical hit. Kingmaker's automatic-hit path in
    /// RuleAttackRoll never rolls the d20 and decides the critical solely
    /// from AutoCriticalThreat and AutoCriticalConfirmation together, so a
    /// threat with an ordinary confirmation cannot be expressed there; the
    /// live round-2 mechanical run showed the armed natural 1 hitting with no
    /// threat. The native RemoveBuffOnAttack on the same state ends it after
    /// that one attack, so a full attack never carries the insight into its
    /// later swings. No per-unit state is kept on this shared component.
    /// </summary>
    [Serializable]
    public sealed class CyclopsFlashOfInsightComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null) return;
            if (!ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(
                    ReferenceEquals(evt.Initiator, Owner.Unit), Fact != null))
                return;
            evt.AutoHit = true;
            evt.AutoCriticalThreat = true;
            evt.AutoCriticalConfirmation = true;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }
    }

    /// <summary>
    /// Shared summon grab (Sprint 4). After a hit with one of the grab
    /// weapons the summon attempts a grapple maneuver through the game's own
    /// rule. Success either starts the native hold - the initiator and target
    /// unit parts, carrying the project's hold and grappled buffs - and deals
    /// any constrict damage, or, for a swallower, swallows the target whole
    /// through the native swallow-whole part. Nothing starts while the summon
    /// already holds or has swallowed someone, while the target is already
    /// held or swallowed, or against the summon itself. No per-unit state is
    /// kept on this shared blueprint component: the native parts and the
    /// buffs they carry are the state.
    /// </summary>
    [Serializable]
    public sealed class SummonGrabComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintItemWeapon[] GrabWeapons;
        public BlueprintBuff HoldBuff;
        public BlueprintBuff GrappledBuff;
        /// <summary>Set only for a swallower: a successful grab swallows.</summary>
        public BlueprintBuff SwallowedBuff;
        public int ConstrictDiceCount;
        public DiceType ConstrictDiceType;
        public int ConstrictBonus;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.Weapon == null ||
                evt.Target == null) return;
            TryGrab(evt.Target, evt.Weapon.Blueprint, evt.AttackRoll.IsHit);
        }

        /// <summary>
        /// The whole decision and its consequences, callable by the guarded
        /// runtime fixture with a known hit so the hold and the swallow can be
        /// proven without a second attack roll.
        /// </summary>
        internal bool TryGrab(UnitEntityData target, BlueprintItemWeapon weapon,
            bool isHit)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || target == null || target.Descriptor == null)
                return false;
            bool isGrabWeapon = weapon != null && GrabWeapons != null &&
                GrabWeapons.Any(value => ReferenceEquals(value, weapon));
            UnitPartSwallowWhole swallower = owner.Get<UnitPartSwallowWhole>();
            bool ownerHolding = owner.Get<UnitPartGrappleInitiator>() != null ||
                (swallower != null && swallower.SwallowedUnits != null &&
                    swallower.SwallowedUnits.Any(value => value.Value != null));
            if (!ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(isHit,
                    isGrabWeapon, ownerHolding,
                    target.Get<UnitPartGrappleTarget>() != null,
                    target.Get<UnitPartSwallowed>() != null,
                    ReferenceEquals(owner, target)))
                return false;
            if (SwallowedBuff == null && (HoldBuff == null || GrappledBuff == null))
                return false;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            var maneuver = new RuleCombatManeuver(owner, target,
                CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            if (!maneuver.Success) return false;
            if (SwallowedBuff != null)
            {
                owner.Ensure<UnitPartSwallowWhole>().Swallow(target, SwallowedBuff);
                return true;
            }
            owner.Ensure<UnitPartGrappleInitiator>().Init(target, HoldBuff, context);
            target.Ensure<UnitPartGrappleTarget>().Init(owner, GrappledBuff, context);
            DealConstrict(owner, target, context);
            return true;
        }

        internal void DealConstrict(UnitEntityData owner, UnitEntityData target,
            MechanicsContext context)
        {
            if (ConstrictDiceCount <= 0 || owner == null || target == null) return;
            var damage = new PhysicalDamage(new DiceFormula(ConstrictDiceCount,
                ConstrictDiceType), PhysicalDamageForm.Bludgeoning);
            damage.AddBonus(ConstrictBonus);
            var rule = new RuleDealDamage(owner, target, damage);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
        }
    }

    /// <summary>
    /// The summon's side of a hold (Sprint 4), carried by the hold buff the
    /// native initiator part applies. Each new round the holder makes a
    /// grapple check to maintain, at the tabletop +5: success deals the grab
    /// weapon's damage plus any constrict; failure releases the target. When
    /// the hold state ends for any reason - the target broke free, the holder
    /// fell, the summon expired or was dismissed, the buff was dispelled -
    /// the target this summon holds is released too, and only that target:
    /// the link is owned here and never inferred from the target's side.
    /// </summary>
    [Serializable]
    public sealed class SummonHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = HeldTarget(owner);
            if (target == null) return;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            var maneuver = new RuleCombatManeuver(owner, target,
                CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            if (!ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(true,
                    maneuver.Success))
            {
                Release(owner, target);
                return;
            }
            SummonGrabComponent grab = FindGrab(owner);
            BlueprintItemWeapon weapon = grab == null || grab.GrabWeapons == null ?
                null : grab.GrabWeapons.FirstOrDefault(value => value != null);
            if (weapon != null)
            {
                DiceFormula dice = weapon.BaseDamage;
                var damage = new PhysicalDamage(dice, PhysicalForm(weapon));
                damage.AddBonus(owner.Stats.Strength.Bonus);
                var rule = new RuleDealDamage(owner, target, damage);
                if (context != null) context.TriggerRule(rule);
                else Rulebook.Trigger(rule);
            }
            if (grab != null) grab.DealConstrict(owner, target, context);
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Grapple || Owner == null ||
                Owner.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit))
                return;
            UnitEntityData target = HeldTarget(Owner.Unit);
            if (target == null || !ReferenceEquals(evt.Target, target)) return;
            evt.AddBonus(ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus,
                Fact);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt) { }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = HeldTarget(owner);
            if (target != null) Release(owner, target);
        }

        /// <summary>
        /// The unit this summon's native initiator part names, provided that
        /// unit's own target part points back at the summon.
        /// </summary>
        internal static UnitEntityData HeldTarget(UnitEntityData owner)
        {
            if (owner == null) return null;
            UnitPartGrappleInitiator part = owner.Get<UnitPartGrappleInitiator>();
            UnitEntityData target = part == null ? null : part.Target.Value;
            if (target == null) return null;
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            return held != null && ReferenceEquals(held.Initiator.Value, owner) ?
                target : null;
        }

        /// <summary>
        /// Removes the target's native part (which removes its grappled buff
        /// and condition). The summon's own initiator part is left to the
        /// game's grapple controller, which drops it once the target is no
        /// longer grappled, and to the summon's own disposal; removing it
        /// here would re-enter this buff's removal.
        /// </summary>
        internal static void Release(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return;
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            if (held != null && ReferenceEquals(held.Initiator.Value, owner))
                target.Remove<UnitPartGrappleTarget>();
        }

        private static SummonGrabComponent FindGrab(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return null;
            foreach (Buff buff in owner.Descriptor.Buffs.RawFacts.OfType<Buff>())
            {
                if (buff == null || buff.Blueprint == null ||
                    buff.Blueprint.ComponentsArray == null) continue;
                SummonGrabComponent grab = buff.Blueprint.ComponentsArray
                    .OfType<SummonGrabComponent>().FirstOrDefault();
                if (grab != null) return grab;
            }
            return null;
        }

        private static PhysicalDamageForm PhysicalForm(BlueprintItemWeapon weapon)
        {
            if (weapon != null && weapon.Type != null &&
                weapon.Type.DamageType != null &&
                weapon.Type.DamageType.Physical != null)
                return weapon.Type.DamageType.Physical.Form;
            return PhysicalDamageForm.Bludgeoning;
        }
    }

    /// <summary>
    /// The swallower's side (Sprint 4), carried by the worm's combat-traits
    /// buff. The game's own swallow-whole part spits everyone out when the
    /// swallower dies or is destroyed; this component spits out when the buff
    /// turns off for any other reason - the summon disposed, the traits
    /// dispelled - so no unit is ever left inside a worm that is gone.
    /// </summary>
    [Serializable]
    public sealed class SummonSwallowLifecycleComponent : BuffLogic
    {
        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitPartSwallowWhole part = owner == null ? null :
                owner.Get<UnitPartSwallowWhole>();
            if (part != null) part.SpitOut(true);
        }
    }

    /// <summary>
    /// Area-transition safeguard for the shared summon grapple lifecycle
    /// (Sprint 4). When the party leaves an area, every party member held or
    /// swallowed by a KMG summon is released before the summon is left
    /// behind; when an area finishes loading, any party member whose native
    /// hold or swallow points at a unit that no longer exists is released.
    /// Subscribed once at load; inert while the module is off.
    /// </summary>
    internal sealed class SummonGrappleAreaSafeguard : IPartyLeaveAreaHandler,
        IAreaLoadingStagesHandler, IGlobalSubscriber
    {
        private static SummonGrappleAreaSafeguard _instance;

        internal static void Attach()
        {
            if (_instance != null) return;
            _instance = new SummonGrappleAreaSafeguard();
            EventBus.Subscribe(_instance);
        }

        public void HandlePartyLeaveArea(BlueprintArea currentArea,
            BlueprintAreaEnterPoint targetArea)
        { Sweep(true); }

        public void OnAreaScenesLoaded() { }

        public void OnAreaLoadingComplete() { Sweep(false); }

        /// <summary>
        /// Releases party members held or swallowed by a KMG summon (always
        /// when leaving; when loading only if the holder is gone). Returns the
        /// number released, for the runtime fixture.
        /// </summary>
        internal static int Sweep(bool leaving)
        {
            if (Game.Instance == null || Game.Instance.Player == null) return 0;
            return Sweep(leaving, Game.Instance.Player.Party);
        }

        /// <summary>The same sweep over an explicit set of units (the runtime
        /// fixture's held target stands in for a party member).</summary>
        internal static int Sweep(bool leaving, IEnumerable<UnitEntityData> units)
        {
            int released = 0;
            if (units == null) return 0;
            foreach (UnitEntityData unit in units.Where(
                value => value != null && value.Descriptor != null).ToArray())
            {
                UnitPartGrappleTarget held = unit.Get<UnitPartGrappleTarget>();
                if (held != null)
                {
                    UnitEntityData holder = held.Initiator.Value;
                    if (ShouldRelease(leaving, holder))
                    {
                        unit.Remove<UnitPartGrappleTarget>();
                        if (holder != null &&
                            holder.Get<UnitPartGrappleInitiator>() != null &&
                            ReferenceEquals(holder.Get<UnitPartGrappleInitiator>()
                                .Target.Value, unit))
                            holder.Remove<UnitPartGrappleInitiator>();
                        released++;
                    }
                }
                UnitPartSwallowed swallowed = unit.Get<UnitPartSwallowed>();
                if (swallowed != null)
                {
                    UnitEntityData swallower = swallowed.Swallower.Value;
                    if (ShouldRelease(leaving, swallower))
                    {
                        UnitPartSwallowWhole part = swallower == null ? null :
                            swallower.Get<UnitPartSwallowWhole>();
                        if (part != null) part.Free(unit);
                        else unit.Remove<UnitPartSwallowed>();
                        released++;
                    }
                }
            }
            return released;
        }

        private static bool ShouldRelease(bool leaving, UnitEntityData holder)
        {
            if (holder == null || holder.Destroyed) return true;
            return leaving && IsKmgSummon(holder);
        }

        internal static bool IsKmgSummon(UnitEntityData unit)
        {
            return unit != null && unit.Blueprint != null &&
                unit.Blueprint.name.StartsWith("KMG_Summoning_Unit_",
                    StringComparison.Ordinal);
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
