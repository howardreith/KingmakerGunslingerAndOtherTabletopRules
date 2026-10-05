using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Creation-only printed land skill allocation for the two crocodilians.
    /// Native class/attribute/size/feat modifiers still calculate the totals.
    /// No additive hidden bonus, donor mutation or reload-time reallocation.
    /// </summary>
    [Serializable]
    public sealed class SummonCrocodilianSkillRanks :
        OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public string CreatureKey;
        public BlueprintUnit OwningBlueprint;
        [JsonProperty] private bool m_Applied;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (m_Applied) return;
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException(
                    "Crocodilian rank allocation requires its exact owning unit.");
            CrocodilianRulesProfile rules = CrocodilianRulesPolicy.For(CreatureKey);
            ModifiableValue perception = unit.Descriptor.Stats.GetStat(
                StatType.SkillPerception);
            ModifiableValue stealth = unit.Descriptor.Stats.GetStat(
                StatType.SkillStealth);
            ModifiableValue mobility = unit.Descriptor.Stats.GetStat(
                StatType.SkillMobility);
            if (perception.BaseValue != 0 || stealth.BaseValue != 0 ||
                    mobility.BaseValue != 0)
                throw new InvalidOperationException(
                    "Crocodilian class ranks must start unallocated.");
            perception.BaseValue = rules.PerceptionRanks;
            stealth.BaseValue = rules.StealthRanks;
            m_Applied = true;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

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
    /// Cyclops Flash of Insight (Sprint 3; rebuilt under the correction
    /// order). The tabletop ability chooses the result of one die roll
    /// before it is rolled; here the chosen roll is the next attack's own
    /// d20, and the chosen result is 20. Lives on the armed state the ability
    /// applies, which has no duration of its own and lasts until the next
    /// attack roll spends it (corrected under the 2026-09-25 order). When the cyclops's own attack roll is about to
    /// trigger, the state arms exactly that attack's first d20; when that
    /// RuleRollD20 is about to trigger, its pre-rolled result becomes 20
    /// (the game's own pre-roll seam, read by Roll()) and the arming is
    /// consumed - so the attack rule sees a natural 20 (a hit, a threat
    /// inside any range) and rolls the critical confirmation normally on a
    /// second, untouched d20. A roll the cyclops merely witnesses, a roll
    /// of another kind, or a later attack never sees the arming: the
    /// initiator-scoped handler, the per-attack arming and the native
    /// RemoveBuffOnAttack on the same state bound it to one attack per use,
    /// and the arming is transient, never saved.
    /// </summary>
    [Serializable]
    public sealed class CyclopsFlashOfInsightComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleRollD20>
    {
        private sealed class Arming { internal RuleAttackRoll Attack; }

        private static readonly ConditionalWeakTable<UnitEntityData, Arming> Armings =
            new ConditionalWeakTable<UnitEntityData, Arming>();
        private static readonly System.Reflection.FieldInfo PreRolledResult =
            typeof(RuleRollD20).GetField("m_PreRolledResult",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

        internal const int ChosenResult = 20;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null) return;
            if (!ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(
                    ReferenceEquals(evt.Initiator, Owner.Unit), Fact != null))
                return;
            Arming arming;
            if (Armings.TryGetValue(Owner.Unit, out arming)) arming.Attack = evt;
            else Armings.Add(Owner.Unit, new Arming { Attack = evt });
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            // Whatever the roll did, the arming never outlives its attack.
            if (Owner != null && Owner.Unit != null) Armings.Remove(Owner.Unit);
        }

        public void OnEventAboutToTrigger(RuleRollD20 evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) || PreRolledResult == null)
                return;
            Arming arming;
            if (!Armings.TryGetValue(Owner.Unit, out arming) || arming.Attack == null) return;
            // The first d20 after arming is the attack roll itself; the
            // confirmation that may follow is a later RuleRollD20 and finds
            // the arming consumed.
            PreRolledResult.SetValue(evt, (int?)ChosenResult);
            Armings.Remove(Owner.Unit);
        }

        public void OnEventDidTrigger(RuleRollD20 evt) { }

        /// <summary>For the runtime fixture: whether an arming is pending for this unit.</summary>
        internal static bool IsArmed(UnitEntityData unit)
        {
            Arming arming;
            return unit != null && Armings.TryGetValue(unit, out arming) && arming.Attack != null;
        }
    }

    /// <summary>
    /// Whether a combat maneuver check a summon made succeeded. The engine's
    /// rule decides by the sum alone - the d20 plus CMB against CMD - while a
    /// combat maneuver check is an attack roll on the tabletop, where a
    /// natural 1 always fails and a natural 20 always succeeds. The chosen
    /// roll beats the defender's numbers and nothing else: a failed
    /// concealment check, an auto-failure flag or a target immune to combat
    /// maneuvers denies the maneuver whatever the die showed.
    /// </summary>
    internal static class SummonManeuverChecks
    {
        internal static bool Succeeded(RuleCombatManeuver maneuver)
        {
            if (maneuver == null || maneuver.AutoFailure) return false;
            // The engine declines to decide a maneuver against a target
            // immune to combat maneuvers: its rule returns before it
            // calculates CMB and CMD, so the verdict it leaves behind reads
            // 0 + 0 >= 0 and reports a success. A summon must not take hold
            // of an immune foe on that.
            if (maneuver.Target == null || maneuver.Target.Descriptor == null ||
                maneuver.Target.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers))
                return false;
            if (maneuver.ConcealmentCheck != null && !maneuver.ConcealmentCheck.Success)
                return false;
            return ExpandedSummoningSpecialProfiles.IsSummonManeuverSuccess(
                (int)maneuver.InitiatorRoll, maneuver.Success);
        }
    }

    /// <summary>Which limb an attack came from, by slot identity.</summary>
    internal enum SummonLimbKind { None, PrimaryHand, Additional }

    /// <summary>
    /// Attack identity for the grapple and rake contracts. A limb is the slot
    /// an attack's weapon entity sits in - the primary hand (a bite, a slam)
    /// or one of the body's additional limbs, where the game lists secondary
    /// limbs after the additional ones - never the weapon blueprint, which
    /// primary claws and rake claws share.
    /// </summary>
    internal static class SummonLimbs
    {
        internal static SummonLimbKind Classify(UnitEntityData owner, ItemEntityWeapon weapon,
            out int additionalIndex)
        {
            additionalIndex = -1;
            if (owner == null || owner.Body == null || weapon == null) return SummonLimbKind.None;
            if (owner.Body.PrimaryHand != null &&
                ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon))
                return SummonLimbKind.PrimaryHand;
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            if (limbs == null) return SummonLimbKind.None;
            for (int index = 0; index < limbs.Count; index++)
                if (limbs[index] != null && ReferenceEquals(limbs[index].MaybeWeapon, weapon))
                {
                    additionalIndex = index;
                    return SummonLimbKind.Additional;
                }
            return SummonLimbKind.None;
        }

        internal static int AdditionalLimbCount(UnitEntityData owner)
        {
            return owner == null || owner.Body == null || owner.Body.AdditionalLimbs == null
                ? 0 : owner.Body.AdditionalLimbs.Count;
        }

        internal static bool IsRakeSlot(UnitEntityData owner, Kingmaker.Items.Slots.WeaponSlot slot,
            int rakeLimbCount)
        {
            if (owner == null || owner.Body == null || slot == null ||
                owner.Body.AdditionalLimbs == null) return false;
            int index = owner.Body.AdditionalLimbs.IndexOf(slot);
            return index >= 0 && ExpandedSummoningSpecialProfiles.IsRakeSlot(index,
                owner.Body.AdditionalLimbs.Count, rakeLimbCount);
        }

        internal static bool IsRakeLimb(UnitEntityData owner, ItemEntityWeapon weapon,
            int rakeLimbCount)
        {
            int index;
            return Classify(owner, weapon, out index) == SummonLimbKind.Additional &&
                ExpandedSummoningSpecialProfiles.IsRakeSlot(index, AdditionalLimbCount(owner),
                    rakeLimbCount);
        }

        /// <summary>The bite: the primary hand's weapon entity, or null.</summary>
        internal static ItemEntityWeapon PrimaryWeapon(UnitEntityData owner)
        {
            return owner == null || owner.Body == null || owner.Body.PrimaryHand == null
                ? null : owner.Body.PrimaryHand.MaybeWeapon;
        }

        /// <summary>
        /// The weapon entity standing in a recorded limb slot: how a link
        /// finds its limb on the body as the holder carries it now, rather
        /// than by holding the entity the original attack used.
        /// </summary>
        internal static ItemEntityWeapon WeaponAt(UnitEntityData owner, SummonLimbKind kind,
            int additionalIndex)
        {
            if (owner == null || owner.Body == null) return null;
            if (kind == SummonLimbKind.PrimaryHand) return PrimaryWeapon(owner);
            if (kind != SummonLimbKind.Additional) return null;
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            if (limbs == null || additionalIndex < 0 || additionalIndex >= limbs.Count)
                return null;
            return limbs[additionalIndex] == null ? null : limbs[additionalIndex].MaybeWeapon;
        }

        /// <summary>The rake claws: the last rake slots of the body's additional limbs.</summary>
        internal static List<ItemEntityWeapon> RakeWeapons(UnitEntityData owner,
            int rakeLimbCount)
        {
            var result = new List<ItemEntityWeapon>();
            if (owner == null || owner.Body == null || owner.Body.AdditionalLimbs == null ||
                rakeLimbCount <= 0) return result;
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            for (int index = 0; index < limbs.Count; index++)
                if (ExpandedSummoningSpecialProfiles.IsRakeSlot(index, limbs.Count,
                        rakeLimbCount) && limbs[index] != null &&
                        limbs[index].MaybeWeapon != null)
                    result.Add(limbs[index].MaybeWeapon);
            return result;
        }
    }

    // The link store used to live here as a table keyed by buff instances,
    // which the maintain could lose within a session. It is now the unit part
    // in SummonGrappleLinkState.cs, which owns the establishing limb and the
    // mouth occupancy for the life of each hold. It is session-scoped: the
    // owner accepted on 2026-09-26 that an active grapple resets safely on a
    // reload, because the engine carries none across a save.

    /// <summary>
    /// The damage of an attack, as the game computes it: the weapon entity's
    /// dice and type, the Strength contribution for that limb, enhancement
    /// and every weapon-stats modifier, dealt as one damage rule.
    /// </summary>
    internal static class SummonGrappleDamage
    {
        /// <summary>
        /// Native OnTrigger constructs the weapon's base description and
        /// assigns it to slot zero (audited Assembly-CSharp IL_03ad-03b6).
        /// Capture that object before after-rule subscribers can reorder the
        /// list. RulebookSubscriptionManager walks base types, so ordinary
        /// RuleCalculateWeaponStats subscribers still run on this rule.
        /// This is local to Death Roll; it installs no global rule patch.
        /// </summary>
        private sealed class DeathRollWeaponStats : RuleCalculateWeaponStats
        {
            internal DeathRollWeaponStats(UnitEntityData owner,
                ItemEntityWeapon weapon) : base(owner, weapon, null) { }

            internal DamageDescription BaseBite { get; private set; }

            public override void OnTrigger(RulebookEventContext context)
            {
                base.OnTrigger(context);
                BaseBite = DamageDescription.Count == 0 ? null :
                    DamageDescription[0];
            }
        }

        internal static int DealWeaponDamage(UnitEntityData owner, UnitEntityData target,
            ItemEntityWeapon weapon, MechanicsContext context)
        {
            if (owner == null || target == null || weapon == null) return 0;
            var stats = new RuleCalculateWeaponStats(owner, weapon, null);
            if (context != null) context.TriggerRule(stats);
            else Rulebook.Trigger(stats);
            var damages = new List<BaseDamage>();
            if (stats.DamageDescription != null)
                foreach (DamageDescription description in stats.DamageDescription)
                    if (description != null) damages.Add(description.CreateDamage());
            if (damages.Count == 0) return 0;
            var bundle = new DamageBundle(damages.ToArray());
            bundle.Weapon = weapon;
            var rule = new RuleDealDamage(owner, target, bundle);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
            return rule.Damage;
        }

        /// <summary>
        /// A death roll: the creature's current bite, with the Strength
        /// contribution raised from the bite's one times to one and a half.
        ///
        /// <para>Everything else about the bite is preserved because the
        /// damage descriptions are the live ones the weapon-stats rule
        /// resolved - the current dice after any legitimate size or dice
        /// change, the damage forms, enhancement, material and weapon
        /// properties, and every rule modifier and buff in play - and the
        /// bundle carries the weapon itself, so damage reduction and
        /// resistance treat this exactly as that bite. What it is not is a
        /// second attack: no attack roll is made, so nothing that keys on a
        /// hit can fire again.</para>
        ///
        /// <para>The extra half is added only for a positive modifier. One
        /// and a half times Strength multiplies a bonus; a penalty applies
        /// once, and is already inside the live bite this is built from, so a
        /// weakened creature's death roll falls with its bite.</para>
        ///
        /// <para>Returns a description of what was dealt, including the live
        /// Strength the half came from, so the runtime fixture can show the
        /// derivation rather than a total.</para>
        /// </summary>
        internal static string DealDeathRollDamage(UnitEntityData owner,
            UnitEntityData target, ItemEntityWeapon weapon,
            MechanicsContext context, out int dealt)
        {
            dealt = 0;
            if (owner == null || target == null || weapon == null)
                return "no-weapon";
            var stats = new DeathRollWeaponStats(owner, weapon);
            if (context != null)
                context.TriggerRule<RuleCalculateWeaponStats>(stats);
            else Rulebook.Trigger<RuleCalculateWeaponStats>(stats);
            int baseIndex = CrocodilianRulesPolicy.BaseBiteIndex(
                stats.DamageDescription, stats.BaseBite);
            if (baseIndex < 0 || stats.BaseBite.TypeDescription == null ||
                    stats.BaseBite.TypeDescription.Type != DamageType.Physical ||
                    stats.DamageDescription.Any(value => value == null))
                return "no-unique-physical-base-bite";
            var damages = stats.DamageDescription.Select(value =>
                value.CreateDamage()).ToList();
            // The live Strength modifier, which the primary natural attack
            // already contributes once. The death roll adds the other half.
            int strengthModifier = owner.Descriptor == null ||
                owner.Descriptor.Stats == null ? 0 :
                owner.Descriptor.Stats.Strength.Bonus;
            int strengthScore = owner.Descriptor.Stats.Strength.ModifiedValue;
            int extraHalf = CrocodilianRulesPolicy.DeathRollExtraHalf(
                strengthModifier);
            string baseline = DescribeDamage(damages);
            BaseDamage baseBite = damages[baseIndex];
            if (extraHalf != 0) baseBite.AddBonus(extraHalf);
            // The weapon constructor sets WeaponDamage and WeaponSize as
            // well as Weapon. Merely assigning Weapon leaves the native
            // base-damage attribution null. Supplemental chunks stay intact.
            var bundle = new DamageBundle(weapon, stats.WeaponSize, baseBite);
            for (int index = 0; index < damages.Count; index++)
                if (index != baseIndex) bundle.Add(damages[index]);
            var rule = new RuleDealDamage(owner, target, bundle);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
            dealt = rule.Damage;
            return "weapon=" + (weapon.Blueprint == null ? "none" :
                    weapon.Blueprint.name) +
                ";biteDamage=" + baseline +
                ";deathRollDamage=" + DescribeDamage(damages) +
                ";baseBiteIndex=" + baseIndex +
                ";liveStrengthScore=" + strengthScore +
                ";liveStrengthModifier=" + strengthModifier +
                ";extraHalf=" + extraHalf + ";dealt=" + dealt;
        }

        /// <summary>
        /// A damage list as dice and bonus, so a fixture can show that the
        /// death roll's line is the bite's line plus the extra half rather
        /// than a number invented somewhere else.
        /// </summary>
        private static string DescribeDamage(List<BaseDamage> damages)
        {
            if (damages == null || damages.Count == 0) return "<none>";
            return string.Join("+", damages.Select(value =>
                value == null ? "<null>" :
                value.Dice.Rolls + "d" + (int)value.Dice.Dice + "+" +
                value.Bonus + "/" + value.Type).ToArray());
        }
    }

    /// <summary>
    /// Shared summon grab (Sprint 4; rebuilt under the 2026-09-25 correction
    /// order around attack identity, target identity and the universal grab
    /// rule). A grab starts only from a hit with one of the owner's grab
    /// limbs - the primary hand (a bite) and/or the first additional limbs
    /// (foreclaws, slams, bites), never a rake claw - against a target no
    /// larger than the owner (explicit exceptions per creature), through the
    /// game's own grapple check with the tabletop +4 (ManeuverBonus on the
    /// same traits, so it reaches start and maintain alike). A single-link
    /// holder carries the native initiator and target parts, and the game's
    /// grapple controller then drives break-free and reach; the Giant
    /// Flytrap holds one target per bite through the project multi-link
    /// state (its MultiHold buff and one held-state buff per target, whose
    /// context names the holder). A swallower never swallows on the grab:
    /// that is the later turn's maintain check (SummonHoldComponent).
    /// </summary>
    [Serializable]
    public sealed class SummonGrabComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>,
        IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public bool GrabWithPrimaryHand;
        public int GrabAdditionalLimbCount;
        /// <summary>The last N additional limbs are rake claws: they never grab and strike only on a charge or against the held foe.</summary>
        public int RakeLimbCount;
        /// <summary>0: same size or smaller (the universal rule); a creature whose stat block says otherwise sets its exception here.</summary>
        public int MaxTargetSizeDelta;
        /// <summary>1 for every holder but the Flytrap (one per bite, four).</summary>
        public int MaxHeldTargets = 1;
        public BlueprintBuff HoldBuff;
        public BlueprintBuff GrappledBuff;
        /// <summary>Set only for a swallower (or the Flytrap's engulf): applied on a later turn's successful check.</summary>
        public BlueprintBuff SwallowedBuff;
        public bool SwallowMaxSizeIsAbsolute;
        public Size SwallowMaxSize;
        /// <summary>-1: up to one size smaller than the swallower (the universal rule).</summary>
        public int SwallowMaxSizeDelta = -1;
        public int ConstrictDiceCount;
        public DiceType ConstrictDiceType;
        public int ConstrictBonus;
        /// <summary>
        /// Set for a crocodilian. A death roll is a maintain-time rider like
        /// constrict, but it is not the bite: its flat bonus is one and a half
        /// times Strength where an ordinary natural attack adds Strength
        /// once, so these dice and this bonus come from the creature's own
        /// rules profile rather than from the limb that established the hold.
        /// </summary>
        public int DeathRollDiceCount;
        public DiceType DeathRollDiceType;
        public int DeathRollBonus;
        /// <summary>
        /// 0: a death roll works on a foe of the crocodilian's own size or
        /// smaller, which is a wider threshold than swallow whole's.
        /// </summary>
        public int DeathRollMaxTargetSizeDelta;
        /// <summary>The prone condition a successful death roll applies.</summary>
        public bool DeathRollKnocksProne = true;

        internal bool HasDeathRoll { get { return DeathRollDiceCount > 0; } }

        internal bool MultiLink { get { return MaxHeldTargets > 1; } }

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.Weapon == null ||
                evt.Target == null) return;
            TryGrab(evt.Target, evt.Weapon, evt.AttackRoll.IsHit);
        }

        /// <summary>
        /// The owner's live grab component - the instance on its buff, which
        /// knows its owner and its fact; the blueprint's own instance only as
        /// a last resort for configuration reads.
        /// </summary>
        internal static SummonGrabComponent Find(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return null;
            foreach (Buff buff in owner.Descriptor.Buffs.RawFacts.OfType<Buff>())
            {
                if (buff == null || buff.Blueprint == null) continue;
                SummonGrabComponent live = buff.Components == null ? null :
                    buff.Components.OfType<SummonGrabComponent>().FirstOrDefault();
                if (live != null) return live;
                if (buff.Blueprint.ComponentsArray == null) continue;
                SummonGrabComponent grab = buff.Blueprint.ComponentsArray
                    .OfType<SummonGrabComponent>().FirstOrDefault();
                if (grab != null) return grab;
            }
            return null;
        }

        internal bool IsGrabLimb(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            int index;
            SummonLimbKind kind = SummonLimbs.Classify(owner, weapon, out index);
            return ExpandedSummoningSpecialProfiles.IsGrabLimb(kind == SummonLimbKind.PrimaryHand,
                kind == SummonLimbKind.Additional ? index : -1, GrabWithPrimaryHand,
                GrabAdditionalLimbCount, SummonLimbs.AdditionalLimbCount(owner), RakeLimbCount);
        }

        /// <summary>The first grab limb's weapon entity (the stand-in for damage after a load).</summary>
        internal ItemEntityWeapon FirstGrabWeapon(UnitEntityData owner)
        {
            if (owner == null || owner.Body == null) return null;
            if (GrabWithPrimaryHand) return SummonLimbs.PrimaryWeapon(owner);
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            if (limbs == null) return null;
            for (int index = 0; index < limbs.Count && index < GrabAdditionalLimbCount; index++)
                if (limbs[index] != null && limbs[index].MaybeWeapon != null)
                    return limbs[index].MaybeWeapon;
            return null;
        }

        internal bool IsSwallowSizeAllowed(UnitEntityData owner, UnitEntityData target)
        {
            return owner != null && target != null &&
                ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(
                    (int)target.Descriptor.State.Size, (int)owner.Descriptor.State.Size,
                    SwallowMaxSizeIsAbsolute, (int)SwallowMaxSize, SwallowMaxSizeDelta);
        }

        /// <summary>The links this owner holds now: single (native parts) or multi (held-state buffs naming it).</summary>
        internal int HeldCount(UnitEntityData owner)
        {
            if (owner == null) return 0;
            if (!MultiLink)
            {
                UnitPartSwallowWhole swallower = owner.Get<UnitPartSwallowWhole>();
                bool swallowed = swallower != null && swallower.SwallowedUnits != null &&
                    swallower.SwallowedUnits.Any(value => value.Value != null);
                return owner.Get<UnitPartGrappleInitiator>() != null || swallowed ? 1 : 0;
            }
            UnitPartSwallowWhole part = owner.Get<UnitPartSwallowWhole>();
            int engulfed = part == null || part.SwallowedUnits == null ? 0 :
                part.SwallowedUnits.Count(value => value.Value != null);
            return SummonMultiHoldComponent.HeldTargets(owner, GrappledBuff).Count + engulfed;
        }

        /// <summary>
        /// The whole decision and its consequences, callable by the guarded
        /// runtime fixture with a known hit and the exact limb weapon entity
        /// so the hold can be proven without a second attack roll.
        /// </summary>
        internal bool TryGrab(UnitEntityData target, ItemEntityWeapon weapon, bool isHit)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || target == null || target.Descriptor == null)
                return false;
            bool sizeAllowed = ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(
                (int)target.Descriptor.State.Size, (int)owner.Descriptor.State.Size,
                MaxTargetSizeDelta);
            bool targetHeld = target.Get<UnitPartGrappleTarget>() != null ||
                SummonHeldComponent.HolderOf(target, GrappledBuff) != null;
            // One target per mouth: a limb that already holds or has engulfed
            // someone cannot take a second foe. An engulfed victim keeps its
            // mouth shut even though its held state has ended.
            bool limbBusy = SummonGrappleLinks.IsLimbOccupied(owner, weapon);
            if (!ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(isHit,
                    IsGrabLimb(owner, weapon), HeldCount(owner) >= MaxHeldTargets || limbBusy,
                    targetHeld, target.Get<UnitPartSwallowed>() != null,
                    ReferenceEquals(owner, target), sizeAllowed))
                return false;
            if (HoldBuff == null || GrappledBuff == null) return false;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            ItemEntityWeapon establishing = weapon;
            var maneuver = new RuleCombatManeuver(owner, target,
                CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            if (!SummonManeuverChecks.Succeeded(maneuver)) return false;
            if (!MultiLink)
            {
                owner.Ensure<UnitPartGrappleInitiator>().Init(target, HoldBuff, context);
                target.Ensure<UnitPartGrappleTarget>().Init(owner, GrappledBuff, context);
            }
            else
            {
                if (owner.Descriptor.Buffs.GetBuff(HoldBuff) == null)
                    owner.Descriptor.Buffs.AddBuff(HoldBuff, context, null);
                target.Descriptor.Buffs.AddBuff(GrappledBuff,
                    new MechanicsContext(owner, target.Descriptor, GrappledBuff, context,
                        target), null);
            }
            SummonGrappleLinks.Record(owner, target, establishing);
            DealConstrict(owner, target, context);
            return true;
        }

        /// <summary>
        /// One target per mouth, in the attack itself: a limb that holds or
        /// has engulfed someone never strikes another unit. The suppressed
        /// swing is silent, as the rake gate's are, so a shut mouth leaves no
        /// log line for an attack it never made.
        /// </summary>
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null || evt.Weapon == null)
                return;
            UnitEntityData occupant = SummonGrappleLinks.OccupantOf(Owner.Unit, evt.Weapon);
            if (occupant == null || ReferenceEquals(occupant, evt.Target)) return;
            evt.AutoMiss = true;
            evt.SuspendCombatLog = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }

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

        /// <summary>
        /// A death roll's own size threshold: the crocodilian's size or
        /// smaller, which is deliberately not the swallow's one-category-
        /// smaller rule. A Large crocodile death rolls a Large foe and cannot
        /// swallow one, which is what keeps both abilities reachable.
        /// </summary>
        internal bool IsDeathRollSizeAllowed(UnitEntityData owner,
            UnitEntityData target)
        {
            if (owner == null || target == null) return false;
            return ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(
                (int)target.Descriptor.State.Size,
                (int)owner.Descriptor.State.Size, DeathRollMaxTargetSizeDelta);
        }

        /// <summary>
        /// The death roll itself: its own crushing damage, the native prone
        /// condition, and the hold kept.
        ///
        /// <para>The prone condition is read back after it is added rather
        /// than assumed to have taken, because a target can be immune or
        /// already prone and the difference matters to the record. The return
        /// value names what actually happened for the runtime fixture.</para>
        /// </summary>
        internal string DealDeathRoll(UnitEntityData owner,
            UnitEntityData target, MechanicsContext context)
        {
            if (!HasDeathRoll || owner == null || target == null)
                return "not-applicable";
            // The bite that established the hold is the one that rolls. The
            // damage is that bite's live damage with the Strength contribution
            // raised, not a line rebuilt from the profile: a buffed, enlarged
            // or weakened creature death rolls for what it actually bites for.
            ItemEntityWeapon bite =
                SummonGrappleLinks.EstablishingWeapon(owner, target);
            if (bite == null || bite.Blueprint == null ||
                    bite.Blueprint.Category != WeaponCategory.Bite ||
                    !ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) ||
                    !IsDeathRollSizeAllowed(owner, target))
                return "refused:no-exact-held-bite";
            int dealt;
            string damageDetail = SummonGrappleDamage.DealDeathRollDamage(
                owner, target, bite, context, out dealt);
            if (damageDetail == "no-unique-physical-base-bite")
                return "refused:" + damageDetail;
            if (target.Descriptor.State.IsDead || target.Destroyed)
            {
                SummonHoldComponent.ReleaseLink(owner, target, this, true);
                return damageDetail + ";targetDied=True;heldKept=False";
            }
            bool proneBefore = target.Descriptor.State.HasCondition(
                UnitCondition.Prone);
            bool proneAfter = proneBefore;
            if (DeathRollKnocksProne && !target.Descriptor.State.IsDead &&
                    !target.Destroyed)
            {
                // The same two-argument call TwinShotKnockdownMechanics
                // uses: the engine owns how long a creature stays down, and a
                // duration passed here would be this project inventing one.
                target.Descriptor.State.AddCondition(UnitCondition.Prone,
                    null);
                proneAfter = target.Descriptor.State.HasCondition(
                    UnitCondition.Prone);
            }
            // The profile's line is reported beside the live one as the
            // baseline an unmodified creature must reproduce, which is what
            // makes a divergence legible rather than invisible.
            return damageDetail + ";baselineContract=" + DeathRollDiceCount +
                "d" + (int)DeathRollDiceType + "+" + DeathRollBonus +
                ";proneBefore=" + proneBefore + ";proneAfter=" + proneAfter +
                ";heldKept=" + CrocodilianRulesPolicy.KeepsGrappleAfterDeathRoll;
        }

        /// <summary>
        /// The later-turn check for a swallower that began the round holding:
        /// the maintain check succeeded and is used as though attempting to
        /// pin; the target is swallowed (engulfed) and takes the bite's
        /// damage; the hold link ends because the swallowed state supersedes
        /// it. Returns the bite damage dealt.
        /// </summary>
        internal int SwallowHeld(UnitEntityData owner, UnitEntityData target,
            MechanicsContext context)
        {
            if (owner == null || target == null || SwallowedBuff == null) return 0;
            // The limb that established the hold is the one that swallows.
            ItemEntityWeapon mouth = SummonGrappleLinks.EstablishingWeapon(owner, target) ??
                SummonLimbs.PrimaryWeapon(owner);
            int damage = SummonGrappleDamage.DealWeaponDamage(owner, target, mouth, context);
            if (target.Descriptor.State.IsDead || target.Destroyed) return damage;
            SummonHoldComponent.ReleaseLink(owner, target, this, false);
            owner.Ensure<UnitPartSwallowWhole>().Swallow(target, SwallowedBuff);
            // The held state ends with the engulf; the mouth stays shut on
            // that victim until it is spat out or breaks free.
            SummonGrappleLinks.Record(owner, target, mouth);
            SummonGrappleLinks.MarkEngulfed(owner, target);
            return damage;
        }
    }

    /// <summary>The Stirge's touch hit establishes its own session attachment,
    /// without making the prey grappled. The attack's zero-damage touch
    /// weapon is the only eligible limb.</summary>
    [Serializable]
    public sealed class StirgeAttachComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintItemWeapon TouchWeapon;
        public BlueprintBuff HoldBuff;
        public BlueprintBuff DiseaseBuff;

        // Paizo Stirge Diseased: one exposure check per victim from this
        // particular Stirge, even across later blood-drain events.
        [JsonProperty]
        private List<string> m_DiseaseCheckedVictims = new List<string>();
        [JsonIgnore]
        private int m_NativeEventCalls;
        [JsonIgnore]
        private int m_NativeFallbackCalls;

        internal int NativeEventCalls { get { return m_NativeEventCalls; } }
        internal int NativeFallbackCalls { get { return m_NativeFallbackCalls; } }

        internal int DiseaseCheckedVictimCount
        {
            get { return m_DiseaseCheckedVictims == null ? 0 :
                m_DiseaseCheckedVictims.Count; }
        }

        internal static StirgeAttachComponent Find(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return null;
            foreach (Buff buff in owner.Descriptor.Buffs.RawFacts.OfType<Buff>())
            {
                StirgeAttachComponent live = buff == null || buff.Components == null ?
                    null : buff.Components.OfType<StirgeAttachComponent>()
                        .FirstOrDefault();
                if (live != null) return live;
            }
            return null;
        }

        internal bool TryDiseaseExposure(UnitEntityData target,
            int actualConstitutionDamage)
        {
            if (target == null || string.IsNullOrEmpty(target.UniqueId) ||
                !StirgeAttachPolicy.ShouldRollDiseaseExposure(
                    actualConstitutionDamage) ||
                m_DiseaseCheckedVictims != null &&
                    m_DiseaseCheckedVictims.Contains(target.UniqueId))
                return false;
            return TryDiseaseExposure(target, actualConstitutionDamage,
                UnityEngine.Random.Range(0, 100));
        }

        internal bool TryDiseaseExposure(UnitEntityData target,
            int actualConstitutionDamage, int percentileRoll)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || target == null || target.Descriptor == null ||
                DiseaseBuff == null || string.IsNullOrEmpty(target.UniqueId) ||
                !StirgeAttachPolicy.ShouldRollDiseaseExposure(
                    actualConstitutionDamage))
                return false;
            if (m_DiseaseCheckedVictims == null)
                m_DiseaseCheckedVictims = new List<string>();
            if (m_DiseaseCheckedVictims.Contains(target.UniqueId)) return false;
            m_DiseaseCheckedVictims.Add(target.UniqueId);
            if (!StirgeAttachPolicy.DiseaseExposureSelected(percentileRoll))
                return true;
            var context = new MechanicsContext(owner, target.Descriptor,
                DiseaseBuff, Fact == null ? null : Fact.MaybeContext,
                new TargetWrapper(target));
            context.Params.DC = StirgeAttachPolicy.FilthFeverFortitudeDc;
            var saving = new RuleSavingThrow(target, SavingThrowType.Fortitude,
                StirgeAttachPolicy.FilthFeverFortitudeDc);
            saving.Reason = context;
            context.TriggerRule(saving);
            if (saving.IsPassed) return true;
            var apply = new RuleApplyBuff(target, DiseaseBuff, context,
                null, (buff, source, duration) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, duration));
            Rulebook.Trigger(apply);
            return true;
        }

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            m_NativeEventCalls++;
            if (evt == null || evt.AttackRoll == null || evt.Weapon == null)
                return;
            TryAttach(evt.Target, evt.Weapon, evt.AttackRoll.IsHit);
        }

        internal void AttachAfterNativeRule(RuleAttackWithWeapon attack)
        {
            if (attack == null || attack.AttackRoll == null ||
                !attack.AttackRoll.IsHit || attack.Weapon == null ||
                !ReferenceEquals(attack.Weapon.Blueprint, TouchWeapon) ||
                ReferenceEquals(StirgeHoldComponent.AttachedTarget(attack.Initiator),
                    attack.Target)) return;
            m_NativeFallbackCalls++;
            TryAttach(attack.Target, attack.Weapon, true);
        }

        internal bool TryAttach(UnitEntityData target, ItemEntityWeapon weapon,
            bool touchHit)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || target == null || target.Descriptor == null ||
                weapon == null || TouchWeapon == null || HoldBuff == null ||
                !ReferenceEquals(weapon.Blueprint, TouchWeapon) ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon) ||
                ReferenceEquals(owner, target) || target.Destroyed)
                return false;
            bool busy = StirgeHoldComponent.AttachedTarget(owner) != null;
            if (!StirgeAttachPolicy.MayAttach(touchHit, busy,
                    !target.Descriptor.State.IsDead)) return false;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            Buff hold = owner.Descriptor.Buffs.AddBuff(HoldBuff, context, null);
            StirgeHoldComponent link = hold == null || hold.Components == null ?
                null : hold.Components.OfType<StirgeHoldComponent>().FirstOrDefault();
            if (link == null)
            {
                if (hold != null) owner.Descriptor.Buffs.RemoveFact(hold);
                return false;
            }
            if (!link.Attach(target))
            {
                owner.Descriptor.Buffs.RemoveFact(hold);
                return false;
            }
            return ReferenceEquals(StirgeHoldComponent.AttachedTarget(owner), target);
        }
    }

    /// <summary>Native UnitAttack can complete a Stirge touch rule without
    /// dispatching its buff's initiator callback. Retry only that exact owned
    /// touch weapon after the native rule; an existing link is left alone.</summary>
    [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger",
        new[] { typeof(RulebookEventContext) })]
    internal static class StirgeNativeTouchAttachPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Postfix(RuleAttackWithWeapon __instance)
        {
            UnitEntityData owner = __instance == null ? null :
                __instance.Initiator;
            if (owner == null || owner.Blueprint == null ||
                owner.Blueprint.name != "KMG_Summoning_Unit_Stirge") return;
            StirgeAttachComponent attach = StirgeAttachComponent.Find(owner);
            if (attach != null) attach.AttachAfterNativeRule(__instance);
        }
    }

    /// <summary>Native translocation is a discontinuous move, including
    /// short teleports that the per-frame distance guard cannot distinguish
    /// from ordinary walking. Release only Stirges attached to that unit.</summary>
    [HarmonyPatch]
    internal static class StirgePreyTranslocationPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(UnitEntityData).GetMethods(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "Translocate" &&
                    method.GetParameters().Length > 0 &&
                    method.GetParameters()[0].ParameterType == typeof(Vector3));
        }

        private static void Postfix(UnitEntityData __instance)
        {
            StirgeHoldComponent.DetachFromTranslocatedTarget(__instance);
        }
    }

    [Serializable]
    public sealed class RemoveStirgeTargetChecker : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData stirge = target == null ? null : target.Unit;
            return caster != null && stirge != null &&
                !stirge.Destroyed && caster.Descriptor != null &&
                ReferenceEquals(StirgeHoldComponent.AttachedTarget(stirge), caster);
        }
    }

    /// <summary>One voluntary standard action against a selected attached
    /// Stirge. The better currently available CMB/Mobility modifier is chosen
    /// before triggering either roll; success removes that Stirge alone.</summary>
    [Serializable]
    public sealed class ContextActionRemoveStirge : ContextAction
    {
        public override string GetCaption()
        { return "Remove one attached Stirge with CMB or Mobility"; }

        public override void RunAction()
        {
            UnitEntityData prey = Context == null ? null : Context.MaybeCaster;
            UnitEntityData stirge = Target == null ? null : Target.Unit;
            if (prey == null || stirge == null || prey.Descriptor == null ||
                !ReferenceEquals(StirgeHoldComponent.AttachedTarget(stirge), prey) ||
                prey.Descriptor.State.IsDead || stirge.Destroyed) return;
            var cmb = new RuleCalculateCMB(prey, stirge,
                CombatManeuver.Grapple);
            Context.TriggerRule(cmb);
            int mobility = prey.Descriptor.Stats.SkillMobility.ModifiedValue;
            bool success;
            if (cmb.Result >= mobility)
            {
                var grapple = new RuleCombatManeuver(prey, stirge,
                    CombatManeuver.Grapple);
                Context.TriggerRule(grapple);
                success = SummonManeuverChecks.Succeeded(grapple);
            }
            else
            {
                var defense = new RuleCalculateCMD(prey, stirge,
                    CombatManeuver.Grapple);
                Context.TriggerRule(defense);
                var escape = new RuleSkillCheck(prey,
                    StatType.SkillMobility, defense.Result);
                Context.TriggerRule(escape);
                success = escape.IsPassed;
            }
            if (success) StirgeHoldComponent.Detach(stirge);
        }
    }

    /// <summary>One attached Stirge's bounded Constitution meal. The active
    /// attachment is session-scoped and resets cleanly on save/load.</summary>
    [Serializable]
    public sealed class StirgeHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        internal static BlueprintAbility RemoveAbility;
        [JsonProperty]
        private int m_CumulativeDamage;
        // Entity references intentionally do not survive a save. The load
        // safeguard removes the owner-only hold before play resumes.
        [JsonIgnore]
        private UnitEntityData m_Target;
        [JsonIgnore]
        private Vector3 m_Offset;
        [JsonIgnore]
        private Vector3 m_LastTargetPosition;

        internal int CumulativeDamage { get { return m_CumulativeDamage; } }

        internal bool Attach(UnitEntityData target)
        {
            if (target == null || target.Descriptor == null ||
                RemoveAbility == null) return false;
            if (!target.Descriptor.HasFact(RemoveAbility) &&
                target.Descriptor.AddFact(RemoveAbility) == null) return false;
            m_Target = target;
            m_LastTargetPosition = target.Position;
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            Vector3 direction = owner == null ? Vector3.right :
                owner.Position - target.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) direction = Vector3.right;
            m_Offset = direction.normalized * 0.6f;
            return true;
        }

        internal static UnitEntityData AttachedTarget(UnitEntityData owner)
        {
            StirgeHoldComponent link = Find(owner);
            return link == null ? null : link.m_Target;
        }

        private static StirgeHoldComponent Find(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return null;
            return owner.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => value != null && value.Components != null)
                .SelectMany(value => value.Components.OfType<StirgeHoldComponent>())
                .FirstOrDefault();
        }

        internal static bool Detach(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return false;
            Buff hold = owner.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .FirstOrDefault(value => value != null && value.Components != null &&
                    value.Components.OfType<StirgeHoldComponent>().Any());
            if (hold == null) return false;
            owner.Descriptor.Buffs.RemoveFact(hold);
            return true;
        }

        internal static void DetachFromTranslocatedTarget(UnitEntityData target)
        {
            if (target == null || Game.Instance == null ||
                Game.Instance.State == null ||
                Game.Instance.State.Units == null) return;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All.ToArray())
                if (ReferenceEquals(AttachedTarget(unit), target)) Detach(unit);
        }

        internal static void FollowAttached(UnitEntityData owner)
        {
            StirgeHoldComponent link = Find(owner);
            if (link == null || link.m_Target == null) return;
            UnitEntityData target = link.m_Target;
            if (owner == null || owner.Destroyed || !owner.IsInGame ||
                owner.Descriptor == null || owner.Descriptor.State.IsDead ||
                target.Destroyed || !target.IsInGame || target.View == null ||
                target.Descriptor == null || target.Descriptor.State.IsDead ||
                Vector3.Distance(link.m_LastTargetPosition, target.Position) > 8f)
            {
                Detach(owner);
                return;
            }
            link.m_LastTargetPosition = target.Position;
            Vector3 position = target.Position + link.m_Offset;
            if (Vector3.Distance(owner.Position, position) < 0.08f) return;
            owner.Translocate(position, null);
            if (Game.Instance != null && Game.Instance.CurrentScene != null &&
                Game.Instance.CurrentScene.Area != null)
                Game.Instance.CurrentScene.Area.InteractiveObjectGrid.MoveTo(
                    owner, position.x, position.z);
        }

        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = m_Target;
            if (owner == null) return;
            // A timed marker can expire before the game's summon controller
            // retires the unit. Never let that gap keep prey grappled.
            if (owner.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance
                    .SystemMechanics.SummonedUnitBuff) == null)
            {
                Detach(owner);
                return;
            }
            if (target == null || target.Destroyed ||
                target.Descriptor.State.IsDead)
            {
                Detach(owner);
                return;
            }
            int requested = StirgeAttachPolicy.RequestedDamage(true, true,
                m_CumulativeDamage);
            int before = target.Descriptor.Stats.Constitution.Damage;
            if (requested > 0)
            {
                var rule = new RuleDealStatDamage(owner, target,
                    StatType.Constitution, new DiceFormula(0, DiceType.D6),
                    requested);
                MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
                if (context != null) context.TriggerRule(rule);
                else Rulebook.Trigger(rule);
            }
            int actual = Math.Max(0, Math.Min(requested,
                target.Descriptor.Stats.Constitution.Damage - before));
            if (actual > 0 && !target.Descriptor.State.IsDead)
            {
                StirgeAttachComponent attach = StirgeAttachComponent.Find(owner);
                if (attach != null) attach.TryDiseaseExposure(target, actual);
            }
            StirgeDrainStep step = StirgeAttachPolicy.EndTurn(true,
                !target.Descriptor.State.IsDead, m_CumulativeDamage, actual);
            m_CumulativeDamage = step.CumulativeDamage;
            if (step.Detach) Detach(owner);
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Grapple ||
                Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(evt.Target,
                    m_Target)) return;
            evt.AddBonus(StirgeAttachPolicy.MaintainGrappleRacialBonus, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt) { }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData target = m_Target;
            m_Target = null;
            ReleaseRemoveAction(target);
        }

        internal static void ReleaseRemoveAction(UnitEntityData target)
        {
            if (target == null || target.Descriptor == null ||
                RemoveAbility == null ||
                !target.Descriptor.HasFact(RemoveAbility)) return;
            if (Game.Instance != null && Game.Instance.State != null &&
                Game.Instance.State.Units != null &&
                Game.Instance.State.Units.All.Any(unit =>
                    ReferenceEquals(AttachedTarget(unit), target))) return;
            target.Descriptor.RemoveFact(RemoveAbility);
        }
    }

    /// <summary>The shared single-link grab hold. A native grapple part owns
    /// its target, and each round a successful maintain check deals the
    /// establishing limb's weapon damage or swallows when applicable.</summary>
    [Serializable]
    public sealed class SummonHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        /// <summary>
        /// The held-round number this hold last resolved a rider on. A round
        /// processed twice would otherwise crush and knock prone twice, so the
        /// rider fires at most once per distinct round of holding. Serialized
        /// with the buff, because a save during a hold must not hand the
        /// target a second death roll on load.
        /// </summary>
        [JsonProperty]
        private int m_LastRiderRound = -1;

        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = HeldTarget(owner);
            if (target == null) return;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            MaintainLink(owner, target, grab, context, Buff,
                HeldState(owner, target, grab), ref m_LastRiderRound);
        }

        /// <summary>
        /// One maintain check for one link, shared by the single and the
        /// multi-link holders. Returns what happened, for the runtime fixture:
        /// "released", "maintained:N" (damage) or "swallowed:N".
        /// </summary>
        internal static string MaintainLink(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, MechanicsContext context, Buff holdBuff, Buff heldState)
        {
            int ignored = -1;
            return MaintainLink(owner, target, grab, context, holdBuff,
                heldState, ref ignored);
        }

        /// <summary>
        /// One maintain check for one link. <paramref name="lastRiderRound"/>
        /// is the held-round number last claimed for this crocodilian link,
        /// before its check resolves; a caller with no rider passes a throwaway.
        /// </summary>
        internal static string MaintainLink(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, MechanicsContext context, Buff holdBuff,
            Buff heldState, ref int lastRiderRound)
        {
            bool crocodilian = grab != null && grab.HasDeathRoll;
            bool targetOwned = !crocodilian ||
                ReferenceEquals(HeldTarget(owner), target);
            if (!targetOwned) return "refused:no-exact-held-target";
            int roundsHeld = RoundsHeld(heldState);
            // Capture the choice entirely from pre-roll state. Claim the
            // round before the maneuver, so re-entry or replay cannot roll
            // another check (and release a hold on that second result).
            CrocodilianMaintainRider rider =
                CrocodilianRulesPolicy.SelectMaintainRider(true, targetOwned,
                    roundsHeld,
                    grab != null && grab.HasDeathRoll,
                    grab != null && grab.IsDeathRollSizeAllowed(owner, target),
                    grab != null && grab.SwallowedBuff != null,
                    grab != null && grab.IsSwallowSizeAllowed(owner, target),
                    grab != null && grab.SwallowedBuff != null);
            if (crocodilian && roundsHeld > 0 &&
                    !CrocodilianRulesPolicy.TryClaimMaintainRound(roundsHeld,
                        ref lastRiderRound))
                return "crocodilian-maintain:already-resolved-this-round;round=" +
                    roundsHeld;
            var maneuver = new RuleCombatManeuver(owner, target, CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            bool success = SummonManeuverChecks.Succeeded(maneuver);
            if (!ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(true, success))
            {
                ReleaseLink(owner, target, grab, true);
                return "released";
            }
            // One successful check resolves one rider. The selector returns a
            // single value rather than two booleans, so a creature with both a
            // death roll and a swallow cannot do both on the same check; for a
            // creature with no death roll it answers exactly as the swallow
            // condition it replaced, which is what leaves the Purple Worm and
            // the Giant Flytrap untouched.
            if (rider == CrocodilianMaintainRider.SwallowWhole)
                return "swallowed:" + grab.SwallowHeld(owner, target, context);
            if (rider == CrocodilianMaintainRider.DeathRoll)
            {
                return "death-roll:" +
                    grab.DealDeathRoll(owner, target, context);
            }
            ItemEntityWeapon weapon = SummonGrappleLinks.EstablishingWeapon(owner, target);
            bool substituted = weapon == null;
            if (substituted)
                weapon = grab == null ? SummonLimbs.PrimaryWeapon(owner) :
                    grab.FirstGrabWeapon(owner);
            int damage = SummonGrappleDamage.DealWeaponDamage(owner, target, weapon, context);
            if (grab != null) grab.DealConstrict(owner, target, context);
            string rake = grab == null ? string.Empty :
                SummonRakeExecution.RakeOnMaintain(owner, target, grab, context);
            return "maintained:" + damage + ";limb=" + (weapon == null || weapon.Blueprint == null ?
                "none" : weapon.Blueprint.name) + (substituted ? ";substituted" : "") + rake;
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

        /// <summary>The target's grappled-state buff instance for this holder's link, single or multi.</summary>
        internal static Buff HeldState(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab)
        {
            if (owner == null || target == null || grab == null || grab.GrappledBuff == null)
                return null;
            if (!grab.MultiLink) return target.Descriptor.Buffs.GetBuff(grab.GrappledBuff);
            return target.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, grab.GrappledBuff) && value.Context != null &&
                ReferenceEquals(value.Context.MaybeCaster, owner));
        }

        /// <summary>
        /// True when this owner holds exactly this target and held it when the
        /// owner's current round began: the target's grappled state has
        /// ticked at least once. The rake and the swallow both gate on it.
        /// </summary>
        internal static bool IsHeldSinceRoundStart(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return false;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (grab == null) return false;
            bool held = grab.MultiLink
                ? ReferenceEquals(SummonHeldComponent.HolderOf(target, grab.GrappledBuff), owner)
                : ReferenceEquals(HeldTarget(owner), target);
            Buff state = HeldState(owner, target, grab);
            return ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(held, RoundsHeld(state));
        }

        /// <summary>
        /// The rounds a held state has stood: its own round number, plus the
        /// tick that is due in the current frame but not yet delivered - the
        /// holder's hold and the target's held state attach in the same
        /// frame and tick in the same later frame, and the game ticks one
        /// unit's buffs before the other's, so the holder's maintain check
        /// reads the round the target is entering rather than the one it
        /// has left.
        /// </summary>
        internal static int RoundsHeld(Buff heldState)
        {
            if (heldState == null) return 0;
            int rounds = heldState.RoundNumber;
            TimeSpan tickTime = BuffTime(heldState, BuffTickTime);
            TimeSpan nextTick = BuffTime(heldState, BuffNextTickTime);
            if (tickTime < TimeSpan.MaxValue && Game.Instance != null &&
                Game.Instance.TimeController != null &&
                Game.Instance.TimeController.GameTime >= nextTick)
                rounds++;
            return rounds;
        }

        // The buff's tick clock is not public in the reference assembly.
        private static readonly System.Reflection.PropertyInfo BuffTickTime = typeof(Buff).GetProperty(
            "TickTime", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);
        private static readonly System.Reflection.PropertyInfo BuffNextTickTime = typeof(Buff).GetProperty(
            "NextTickTime", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        private static TimeSpan BuffTime(Buff buff, System.Reflection.PropertyInfo property)
        {
            if (buff == null || property == null) return TimeSpan.MaxValue;
            object value = property.GetValue(buff, null);
            return value is TimeSpan ? (TimeSpan)value : TimeSpan.MaxValue;
        }

        /// <summary>
        /// Ends one link. Single: removes the target's native part (which
        /// removes its grappled buff and condition); the summon's own
        /// initiator part is left to the game's grapple controller, which
        /// drops it once the target is no longer grappled, and to the
        /// summon's own disposal - removing it here would re-enter this
        /// buff's removal. Multi: removes the target's held-state buff for
        /// this holder and, when it was the last link, the holder's own
        /// MultiHold buff.
        /// </summary>
        internal static void ReleaseLink(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, bool dropHoldWhenLast)
        {
            if (owner == null || target == null) return;
            SummonGrappleLinks.Release(owner, target);
            if (grab == null || !grab.MultiLink)
            {
                Release(owner, target);
                return;
            }
            Buff state = HeldState(owner, target, grab);
            if (state != null) target.Descriptor.Buffs.RemoveFact(state);
            if (dropHoldWhenLast && grab.HoldBuff != null &&
                SummonMultiHoldComponent.HeldTargets(owner, grab.GrappledBuff).Count == 0)
            {
                Buff hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                if (hold != null) owner.Descriptor.Buffs.RemoveFact(hold);
            }
        }

        internal static void Release(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return;
            SummonGrappleLinks.Release(owner, target);
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            if (held != null && ReferenceEquals(held.Initiator.Value, owner))
                target.Remove<UnitPartGrappleTarget>();
        }
    }

    /// <summary>
    /// The holder's side of the multi-link hold (the Giant Flytrap: one held
    /// target per bite, four at most). One instance of this buff stands on the
    /// holder while it holds anyone; the links themselves are the held-state
    /// buffs on the targets, each naming this holder as its caster, so they
    /// save, load and count on their own. Each new round every link gets its
    /// own maintain check (+5 on the +4): success deals the establishing
    /// bite's damage, or - when the link began the round held and the target
    /// is Medium or smaller - engulfs it instead; failure releases that link
    /// alone; a target out of the bite's reach is released. When the last
    /// link ends the buff removes itself; when the buff turns off for any
    /// reason every link is released.
    /// </summary>
    [Serializable]
    public sealed class SummonMultiHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (owner == null || grab == null) return;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            List<UnitEntityData> targets = HeldTargets(owner, grab.GrappledBuff);
            if (targets.Count == 0)
            {
                Buff.Remove();
                return;
            }
            foreach (UnitEntityData target in targets)
            {
                if (!target.Descriptor.State.IsConscious ||
                    !Kingmaker.Controllers.Combat.UnitEngagementExtension.IsReach(owner, target,
                        owner.Body.PrimaryHand))
                {
                    SummonHoldComponent.ReleaseLink(owner, target, grab, false);
                    continue;
                }
                SummonHoldComponent.MaintainLink(owner, target, grab, context, Buff,
                    SummonHoldComponent.HeldState(owner, target, grab));
            }
            if (HeldTargets(owner, grab.GrappledBuff).Count == 0 && Buff != null &&
                Buff.Active)
                Buff.Remove();
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Grapple || Owner == null ||
                Owner.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                evt.Target == null) return;
            SummonGrabComponent grab = SummonGrabComponent.Find(Owner.Unit);
            if (grab == null ||
                !ReferenceEquals(SummonHeldComponent.HolderOf(evt.Target, grab.GrappledBuff),
                    Owner.Unit)) return;
            evt.AddBonus(ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt) { }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (owner == null || grab == null) return;
            foreach (UnitEntityData target in HeldTargets(owner, grab.GrappledBuff))
                SummonHoldComponent.ReleaseLink(owner, target, grab, false);
        }

        /// <summary>Every loaded unit whose held-state buff names this holder.</summary>
        internal static List<UnitEntityData> HeldTargets(UnitEntityData owner,
            BlueprintBuff heldState)
        {
            var result = new List<UnitEntityData>();
            if (owner == null || heldState == null || Game.Instance == null ||
                Game.Instance.State == null) return result;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All)
            {
                if (unit == null || unit.Descriptor == null || unit.Destroyed) continue;
                if (ReferenceEquals(SummonHeldComponent.HolderOf(unit, heldState), owner))
                    result.Add(unit);
            }
            return result;
        }
    }

    /// <summary>
    /// The held unit's side of a multi-link hold, carried by the held-state
    /// buff whose context names the holder. Each new round the unit attempts
    /// to break free through the game's own check (UnitHelper.TryBreakFree:
    /// the better of Mobility and Athletics, or its CMB, against the
    /// holder's CMD), and the link ends on success, when the holder is gone
    /// or unconscious, or when the holder no longer carries its hold. The
    /// buff's own conditions (Entangled, CantMove) are the grappled state.
    /// </summary>
    [Serializable]
    public sealed class SummonHeldComponent : BuffLogic, ITickEachRound
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData holder = Buff == null || Buff.Context == null ? null :
                Buff.Context.MaybeCaster;
            if (owner == null) return;
            SummonGrabComponent grab = SummonGrabComponent.Find(holder);
            if (holder == null || holder.Destroyed || !holder.Descriptor.State.IsConscious ||
                grab == null || grab.HoldBuff == null ||
                holder.Descriptor.Buffs.GetBuff(grab.HoldBuff) == null)
            {
                Buff.Remove();
                return;
            }
            if (UnitHelper.TryBreakFree(owner, holder, UnitHelper.BreakFreeFlags.Default,
                    Buff.Context, null))
                SummonHoldComponent.ReleaseLink(holder, owner, grab, true);
        }

        /// <summary>The holder a unit's held-state buff names, or null.</summary>
        internal static UnitEntityData HolderOf(UnitEntityData unit, BlueprintBuff heldState)
        {
            if (unit == null || unit.Descriptor == null || heldState == null) return null;
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>())
                if (buff != null && ReferenceEquals(buff.Blueprint, heldState) &&
                    buff.Context != null && buff.Context.MaybeCaster != null)
                    return buff.Context.MaybeCaster;
            return null;
        }
    }

    /// <summary>
    /// The single-link held state's round counter (correction order): a
    /// buff ticks each round only when a component asks for rounds, and the
    /// "held since the round began" gate (the cats' rake, the worm's
    /// swallow) reads the held state's round number, so the grappled buff
    /// carries this no-op round component and the game advances its round
    /// number every six seconds of the hold. The multi-link held state ticks
    /// through its own break-free component already.
    /// </summary>
    [Serializable]
    public sealed class SummonHeldRoundComponent : BuffLogic, ITickEachRound
    {
        public void OnNewRound() { }
    }

    /// <summary>
    /// The swallower's side (Sprint 4), carried by the worm's and the
    /// flytrap's combat-traits buff. The game's own swallow-whole part spits
    /// everyone out when the swallower dies or is destroyed; this component
    /// spits out when the buff turns off for any other reason - the summon
    /// disposed, the traits dispelled - so no unit is ever left inside a
    /// creature that is gone.
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
    /// (Sprint 4). When the party leaves an area, every party member held
    /// (natively or through a multi-link held state), engulfed or swallowed
    /// by a KMG summon is released before the summon is left behind; when an
    /// area finishes loading, any party member whose hold or swallow points
    /// at a unit that no longer exists is released. Subscribed once at load;
    /// inert while the module is off.
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
            int stirges = Game.Instance.State == null ||
                Game.Instance.State.Units == null ? 0 :
                SweepStirge(Game.Instance.State.Units.All);
            return stirges + Sweep(leaving, Game.Instance.Player.Party);
        }

        internal static int SweepStirge(IEnumerable<UnitEntityData> units)
        {
            if (units == null) return 0;
            UnitEntityData[] all = units.Where(value => value != null &&
                value.Descriptor != null).ToArray();
            int released = 0;
            foreach (UnitEntityData unit in all.Where(value =>
                value.Blueprint != null &&
                value.Blueprint.name == "KMG_Summoning_Unit_Stirge"))
                if (StirgeHoldComponent.Detach(unit)) released++;
            // An ability fact may have been serialized on prey, while its
            // session link was deliberately not. Remove that orphan on load.
            foreach (UnitEntityData unit in all)
                StirgeHoldComponent.ReleaseRemoveAction(unit);
            return released;
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
                foreach (Buff state in unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Where(value => value != null && value.Blueprint != null &&
                        value.Blueprint.ComponentsArray != null &&
                        value.Blueprint.ComponentsArray.OfType<SummonHeldComponent>().Any())
                    .ToArray())
                {
                    UnitEntityData holder = state.Context == null ? null :
                        state.Context.MaybeCaster;
                    if (!ShouldRelease(leaving, holder)) continue;
                    SummonGrabComponent grab = SummonGrabComponent.Find(holder);
                    if (holder != null && grab != null)
                        SummonHoldComponent.ReleaseLink(holder, unit, grab, true);
                    else unit.Descriptor.Buffs.RemoveFact(state);
                    released++;
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

    /// <summary>
    /// Sprint 7, rebuilt under the correction order: the cats' rake, gated on
    /// attack identity and target identity. A rake claw (the last two limbs
    /// of the body) strikes on a charge - Pounce makes the charge a full
    /// attack - or against the exact foe this cat holds and held when its
    /// round began (the held state has ticked at least once). Any other
    /// attack roll with a rake claw is an automatic, silent miss: no roll, no
    /// damage, no combat-log line. This is the safety net; the attack
    /// sequence itself drops the rake attacks first (see
    /// ExpandedSummoningRakeSequencePatch), so an ordinary full attack, an
    /// attack of opportunity or a command replay never even spends the swing.
    /// </summary>
    [Serializable]
    public sealed class SummonRakeComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null) return;
            UnitEntityData owner = Owner.Unit;
            if (owner == null || owner.Body == null) return;
            bool isRakeWeapon = IsRakeWeapon(owner, evt.Weapon);
            bool isCharge = evt.RuleAttackWithWeapon != null &&
                evt.RuleAttackWithWeapon.IsCharge;
            bool heldTargetSinceRoundStart = SummonHoldComponent.IsHeldSinceRoundStart(
                owner, evt.Target);
            if (ExpandedSummoningSpecialProfiles.ShouldRakeApply(isRakeWeapon,
                    isCharge, heldTargetSinceRoundStart)) return;
            evt.AutoMiss = true;
            evt.SuspendCombatLog = true;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        /// <summary>True when the weapon entity sits in one of the owner's rake slots.</summary>
        public static bool IsRakeWeapon(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            return SummonLimbs.IsRakeLimb(owner, weapon,
                ExpandedSummoningSpecialProfiles.CatRakeSlotCount);
        }
    }

    /// <summary>
    /// The rake a cat makes while it holds. On the tabletop the two rake
    /// attacks come as part of the grapple check that maintains the hold, and
    /// that is the only moment a holding cat can act at all: the game's own
    /// initiator part gives the holder CantAct and CantMove, so no command it
    /// could issue would start. A successful later-turn maintain against the
    /// exact held target therefore makes the rake attacks here - genuine
    /// attack rolls with the rake claws, their own damage, their own
    /// criticals and their own combat log. A hold established this turn has
    /// not ticked a round, so its first maintain is the next round; a rake
    /// never reaches a unit this cat does not hold; and the claws are the
    /// body's rake slots, never the primary claws. The attacks resolve on the
    /// rulebook rather than through a command, so they carry no separate
    /// swing animation, which is recorded as the adaptation it is.
    /// </summary>
    internal static class SummonRakeExecution
    {
        internal static string RakeOnMaintain(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, MechanicsContext context)
        {
            if (owner == null || target == null || grab == null || grab.RakeLimbCount <= 0)
                return string.Empty;
            if (!SummonHoldComponent.IsHeldSinceRoundStart(owner, target))
                return ";rake=not-eligible";
            List<ItemEntityWeapon> claws = SummonLimbs.RakeWeapons(owner, grab.RakeLimbCount);
            if (claws.Count == 0) return ";rake=no-claws";
            var outcomes = new List<string>();
            foreach (ItemEntityWeapon claw in claws)
            {
                if (target.Descriptor.State.IsDead || target.Destroyed) break;
                var attack = new RuleAttackWithWeapon(owner, target, claw, 0);
                if (context != null) context.TriggerRule(attack);
                else Rulebook.Trigger(attack);
                RuleAttackRoll roll = attack.AttackRoll;
                int dealt = attack.MeleeDamage == null ? 0 : attack.MeleeDamage.Damage;
                outcomes.Add((claw.Blueprint == null ? "?" : claw.Blueprint.name) + ":" +
                    (roll == null ? "no-roll" : "natural=" + (int)roll.Roll + ",hit=" +
                        roll.IsHit + ",critical=" + roll.IsCriticalConfirmed + ",damage=" + dealt));
            }
            return ";rake=" + string.Join(",", outcomes.ToArray());
        }
    }

    /// <summary>
    /// The attack-sequencing seam for the rake: when the game builds a full
    /// attack for a cat that is neither charging nor holding its target since
    /// its round began, the rake claws are removed from the planned attacks,
    /// so no hidden extra swing consumes animation time or triggers anything.
    /// A single attack and an attack of opportunity use the primary hand
    /// only and never reach here. Outcomes are kept for the runtime fixture.
    /// </summary>
    [HarmonyPatch(typeof(Kingmaker.UnitLogic.Commands.UnitAttack), "CreateFullAttack")]
    internal static class ExpandedSummoningRakeSequencePatch
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        private static void Postfix(Kingmaker.UnitLogic.Commands.UnitAttack __instance,
            List<Kingmaker.UnitLogic.Commands.AttackHandInfo> __result)
        {
            try
            {
                if (__instance == null || __result == null) return;
                UnitEntityData owner = __instance.Executor;
                SummonGrabComponent grab = SummonGrabComponent.Find(owner);
                if (grab == null) return;
                UnitEntityData target = __instance.TargetUnit;
                // One target per mouth: a limb holding or engulfing someone
                // else is not part of this full attack at all.
                int shut = __result.RemoveAll(info => info != null && info.Hand != null &&
                    IsOccupiedElsewhere(owner, info.Hand, target));
                if (shut > 0)
                    Record(owner, "mouthsShut=" + shut + ";attacks=" + __result.Count);
                if (grab.RakeLimbCount <= 0) return;
                bool heldSinceRoundStart = SummonHoldComponent.IsHeldSinceRoundStart(owner, target);
                if (ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, __instance.IsCharge,
                        heldSinceRoundStart))
                {
                    Record(owner, "kept;charge=" + __instance.IsCharge + ";held=" +
                        heldSinceRoundStart + ";attacks=" + __result.Count);
                    return;
                }
                int removed = __result.RemoveAll(info => info != null && info.Hand != null &&
                    SummonLimbs.IsRakeSlot(owner, info.Hand, grab.RakeLimbCount));
                Record(owner, "removed=" + removed + ";attacks=" + __result.Count);
            }
            catch (Exception)
            {
                // The rake gate never interrupts the game's own attack command.
            }
        }

        private static bool IsOccupiedElsewhere(UnitEntityData owner,
            Kingmaker.Items.Slots.WeaponSlot hand, UnitEntityData target)
        {
            ItemEntityWeapon weapon = hand == null ? null : hand.MaybeWeapon;
            UnitEntityData occupant = SummonGrappleLinks.OccupantOf(owner, weapon);
            return occupant != null && !ReferenceEquals(occupant, target);
        }

        private static void Record(UnitEntityData owner, string outcome)
        {
            lock (Sync)
            {
                if (Outcomes.Count >= 64) Outcomes.RemoveAt(0);
                Outcomes.Add((owner == null || owner.Blueprint == null ? "?" :
                    owner.Blueprint.name) + "=" + outcome);
            }
        }
    }

    /// <summary>
    /// The Web's target limit (correction order): a web catches a creature
    /// up to one size category larger than the spinner. A blueprint
    /// component the game consults before the ability can target.
    /// </summary>
    [Serializable]
    public sealed class SummonWebTargetSizeChecker : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker
    {
        public int MaxSizeDelta = 1;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            if (caster == null || unit == null) return false;
            return ExpandedSummoningSpecialProfiles.IsWebTargetSizeAllowed(
                (int)unit.Descriptor.State.Size, (int)caster.Descriptor.State.Size, MaxSizeDelta);
        }
    }

    /// <summary>
    /// Wind Wall (correction order): while an ally stands inside the Dust
    /// Mephit's wall of wind, arrows and bolts shot at it are deflected and
    /// miss outright; any other normal ranged weapon has a 30% miss chance
    /// through the game's own miss-chance roll. Rays, touches and kinetic
    /// blasts pass, as spells pass the tabletop wall. Melee is untouched.
    /// The game hands every attack roll against the owner to this handler
    /// before the roll. Outcomes are kept for the runtime fixture.
    /// </summary>
    [Serializable]
    public sealed class SummonWindWallComponent : BuffLogic,
        ITargetRulebookHandler<RuleAttackRoll>
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Target, Owner.Unit)) return;
            WeaponCategory? category = evt.Weapon == null || evt.Weapon.Blueprint == null ?
                (WeaponCategory?)null : evt.Weapon.Blueprint.Category;
            SummonWindWallOutcome outcome = ExpandedSummoningSpecialProfiles.WindWallOutcome(
                evt.AttackType.IsRanged(),
                category.HasValue && IsArrowOrBolt(category.Value),
                category.HasValue && IsSpellDelivery(category.Value));
            switch (outcome)
            {
                case SummonWindWallOutcome.Deflected:
                    evt.AutoMiss = true;
                    break;
                case SummonWindWallOutcome.MissChance:
                    evt.IncreaseMissChance(
                        ExpandedSummoningSpecialProfiles.WindWallOtherRangedMissChance);
                    break;
                default:
                    return;
            }
            lock (Sync)
            {
                Outcomes.Add(Owner.Unit.UniqueId + ":" + outcome + ":" +
                    (category.HasValue ? category.Value.ToString() : "none") +
                    ":missChance=" + evt.MissChance);
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }

        /// <summary>Bows and crossbows shoot the arrows and bolts the wall deflects outright.</summary>
        internal static bool IsArrowOrBolt(WeaponCategory category)
        {
            switch (category)
            {
                case WeaponCategory.Longbow:
                case WeaponCategory.Shortbow:
                case WeaponCategory.LightCrossbow:
                case WeaponCategory.HeavyCrossbow:
                case WeaponCategory.HandCrossbow:
                case WeaponCategory.LightRepeatingCrossbow:
                case WeaponCategory.HeavyRepeatingCrossbow:
                    return true;
            }
            return false;
        }

        /// <summary>Rays, touches and kinetic blasts are spell deliveries, not weapons the wall stops.</summary>
        internal static bool IsSpellDelivery(WeaponCategory category)
        {
            return category == WeaponCategory.Touch || category == WeaponCategory.Ray ||
                category == WeaponCategory.KineticBlast;
        }
    }

    /// <summary>
    /// Chill Metal (correction order): what the target carries decides the
    /// tier of cold - the table's full dice for a creature in metal armor,
    /// its minimal 1 or 2 points for one carrying only a metal weapon, and
    /// nothing (no valid target) for one carrying no metal at all. Armor is
    /// metal unless its type is padded, leather or hide; a manufactured
    /// weapon is metal unless it is one of the wooden kinds (clubs, staves,
    /// slings, bows, nunchaku). Shields alone are not counted: they never
    /// reach the tabletop's one-fifth-of-body-weight threshold.
    /// </summary>
    internal static class SummonChillMetal
    {
        private static readonly string[] NonMetalArmorTokens = { "Padded", "Leather", "Hide" };
        private static readonly WeaponCategory[] WoodenCategories = {
            WeaponCategory.Club, WeaponCategory.Greatclub, WeaponCategory.Quarterstaff,
            WeaponCategory.Sling, WeaponCategory.SlingStaff, WeaponCategory.Shortbow,
            WeaponCategory.Longbow, WeaponCategory.Nunchaku
        };
        private static readonly WeaponCategory[] NotManufactured = {
            WeaponCategory.UnarmedStrike, WeaponCategory.Touch, WeaponCategory.Ray,
            WeaponCategory.Bomb, WeaponCategory.KineticBlast, WeaponCategory.Bite,
            WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons
        };

        internal static string ArmorTypeName(UnitEntityData unit)
        {
            if (unit == null || unit.Body == null || unit.Body.Armor == null) return null;
            ItemEntityArmor armor = unit.Body.Armor.MaybeArmor;
            if (armor == null || armor.Blueprint == null || armor.Blueprint.Type == null)
                return null;
            return armor.Blueprint.Type.name;
        }

        internal static bool IsMetalArmorType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return false;
            foreach (string token in NonMetalArmorTokens)
                if (typeName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            return true;
        }

        internal static bool WearsMetalArmor(UnitEntityData unit)
        { return IsMetalArmorType(ArmorTypeName(unit)); }

        internal static bool IsMetalWeapon(BlueprintItemWeapon weapon)
        {
            if (weapon == null || weapon.IsNatural) return false;
            WeaponCategory category = weapon.Category;
            return Array.IndexOf(NotManufactured, category) < 0 &&
                Array.IndexOf(WoodenCategories, category) < 0;
        }

        internal static bool WieldsMetalWeapon(UnitEntityData unit)
        {
            if (unit == null || unit.Body == null) return false;
            foreach (Kingmaker.Items.Slots.HandSlot hand in new[] {
                unit.Body.PrimaryHand, unit.Body.SecondaryHand })
                if (hand != null && hand.MaybeWeapon != null &&
                    IsMetalWeapon(hand.MaybeWeapon.Blueprint))
                    return true;
            return false;
        }

        internal static int Tier(UnitEntityData unit)
        {
            return ExpandedSummoningSpecialProfiles.ChillMetalTier(WearsMetalArmor(unit),
                WieldsMetalWeapon(unit));
        }

        internal static string Describe(UnitEntityData unit)
        {
            return "armor=" + (ArmorTypeName(unit) ?? "none") + ";metalArmor=" +
                WearsMetalArmor(unit) + ";metalWeapon=" + WieldsMetalWeapon(unit) +
                ";tier=" + Tier(unit);
        }
    }

    /// <summary>Chill Metal targets only a creature wearing or carrying metal.</summary>
    [Serializable]
    public sealed class SummonChillMetalTargetChecker : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return unit != null && SummonChillMetal.Tier(unit) > 0;
        }
    }

    /// <summary>
    /// The chilled state: at the start of each of its rounds the target takes
    /// the table's cold for that round of the spell (the state's first tick
    /// is the spell's second round), in full for metal armor, minimal for a
    /// metal weapon only, and none once it carries no metal. Cold is dealt
    /// through the game's damage rule by the mephit that cast it, so cold
    /// immunity and resistance apply. Outcomes are kept for the fixture.
    /// </summary>
    [Serializable]
    public sealed class SummonChillMetalComponent : BuffLogic, ITickEachRound
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null || Buff == null) return;
            UnitEntityData owner = Owner.Unit;
            int round = Buff.RoundNumber + 1;
            int tier = SummonChillMetal.Tier(owner);
            int dice = ExpandedSummoningSpecialProfiles.ChillMetalDice(round);
            int minimal = ExpandedSummoningSpecialProfiles.ChillMetalMinimalDamage(round);
            int dealt = 0;
            if (tier > 0 && dice > 0)
            {
                var description = new DamageDescription {
                    TypeDescription = new DamageTypeDescription {
                        Type = DamageType.Energy, Energy = DamageEnergyType.Cold },
                    Dice = tier == 2 ? new DiceFormula(dice, DiceType.D4) :
                        new DiceFormula(0, DiceType.Zero),
                    Bonus = tier == 2 ? 0 : minimal
                };
                UnitEntityData caster = Buff.Context != null && Buff.Context.MaybeCaster != null ?
                    Buff.Context.MaybeCaster : owner;
                var rule = new RuleDealDamage(caster, owner,
                    new DamageBundle(new BaseDamage[] { description.CreateDamage() }));
                Rulebook.Trigger(rule);
                dealt = rule.Damage;
            }
            lock (Sync)
            {
                Outcomes.Add(owner.UniqueId + ":round=" + round + ";tier=" + tier + ";dice=" +
                    dice + "d4;minimal=" + minimal + ";damage=" + dealt);
            }
        }
    }

    /// <summary>
    /// Docile hooves (correction order): the Pony's and the Horse's hooves
    /// are secondary natural attacks - the tabletop Docile quality, which a
    /// summon never trains away. The game computes a secondary natural
    /// attack (-5 to hit, half the Strength modifier to damage) for any
    /// weapon entity whose ForceSecondary flag is set, so this carrier sets
    /// it on the primary hand's hoof and on every additional limb's hoof
    /// when it turns on - at spawn and again after a load - and clears it
    /// when it turns off. No other stat, limb or attack is touched.
    /// </summary>
    [Serializable]
    public sealed class SummonDocileHoovesComponent : BuffLogic,
        IInitiatorRulebookHandler<RuleCalculateAttackBonus>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        /// <summary>Fixture-only: lets the fixture compute the same hooves as primary for the comparison.</summary>
        internal static bool SuspendedForFixture { get; set; }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            // The unit's facts activate before its body's limbs exist; the
            // body patch below and the rule-time hooks catch that case.
            Apply(Owner == null ? null : Owner.Unit, true);
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            Apply(Owner == null ? null : Owner.Unit, false);
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        { Ensure(evt == null ? null : evt.Initiator, evt == null ? null : evt.Weapon); }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        { Ensure(evt == null ? null : evt.Initiator, evt == null ? null : evt.Weapon); }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private void Ensure(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (SuspendedForFixture || owner == null || weapon == null || Owner == null ||
                !ReferenceEquals(owner, Owner.Unit)) return;
            if (Hooves(owner).Contains(weapon)) weapon.ForceSecondary = true;
        }

        /// <summary>Sets or clears the docile flag on every hoof; returns how many hooves were touched.</summary>
        internal static int Apply(UnitEntityData owner, bool secondary)
        {
            int count = 0;
            foreach (ItemEntityWeapon hoof in Hooves(owner))
            {
                hoof.ForceSecondary = secondary;
                count++;
            }
            return count;
        }

        /// <summary>True when the unit carries a docile-hoof carrier buff.</summary>
        internal static bool Carries(UnitDescriptor descriptor)
        {
            if (descriptor == null || descriptor.Buffs == null) return false;
            foreach (Buff buff in descriptor.Buffs.RawFacts.OfType<Buff>())
                if (buff != null && buff.Blueprint != null && buff.Blueprint.ComponentsArray != null &&
                    buff.Blueprint.ComponentsArray.OfType<SummonDocileHoovesComponent>().Any())
                    return true;
            return false;
        }

        /// <summary>Every natural weapon entity on the body: the primary hand's and the additional limbs'.</summary>
        internal static List<ItemEntityWeapon> Hooves(UnitEntityData owner)
        {
            var result = new List<ItemEntityWeapon>();
            if (owner == null || owner.Body == null) return result;
            ItemEntityWeapon primary = SummonLimbs.PrimaryWeapon(owner);
            if (primary != null && primary.Blueprint != null && primary.Blueprint.IsNatural)
                result.Add(primary);
            if (owner.Body.AdditionalLimbs != null)
                foreach (Kingmaker.Items.Slots.WeaponSlot limb in owner.Body.AdditionalLimbs)
                    if (limb != null && limb.MaybeWeapon != null &&
                        limb.MaybeWeapon.Blueprint != null && limb.MaybeWeapon.Blueprint.IsNatural)
                        result.Add(limb.MaybeWeapon);
            return result;
        }
    }

    /// <summary>
    /// The unit's facts activate before its body's limbs are created, so
    /// the docile carrier's own activation finds no hooves; once the body
    /// initializes, the flag is set on every hoof of a unit that carries
    /// the docile-hoof carrier (spawn and load alike).
    /// </summary>
    [HarmonyPatch(typeof(UnitBody), "Initialize")]
    internal static class ExpandedSummoningDocileHoovesBodyPatch
    {
        private static void Postfix(UnitBody __instance)
        {
            try
            {
                UnitDescriptor owner = __instance == null ? null : __instance.Owner;
                if (owner == null || owner.Unit == null ||
                    !SummonDocileHoovesComponent.Carries(owner)) return;
                SummonDocileHoovesComponent.Apply(owner.Unit, true);
            }
            catch (Exception)
            {
                // The docile flag never interrupts the game's own body initialization.
            }
        }
    }

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
