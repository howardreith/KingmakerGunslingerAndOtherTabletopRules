using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal static class GlabrezuIdentity38
    {
        internal static bool IsLiveTarget(UnitEntityData target)
        {
            return target != null && !target.Destroyed && target.IsInState &&
                !target.Descriptor.State.IsDead;
        }

        internal static bool IsExactSource(UnitEntityData caster,
            BlueprintUnit source)
        {
            return caster != null && source != null &&
                caster.Blueprint != null &&
                ReferenceEquals(caster.Blueprint, source);
        }
    }

    /// <summary>
    /// The Glabrezu's rend: two pincer hits on one creature in one round tear
    /// the wound open, once per round. The engine's own <c>RendFeature</c>,
    /// which sits on the same feature, still owns the damage - the dice plus
    /// one and a half times the live Strength modifier as a single event,
    /// which is the printed 2d8+15 at Strength 31. This component decides only
    /// <i>whether this attack is the rend</i>.
    ///
    /// <para><b>Why this is not the released <c>DireApeRendGate</c>.</b> That
    /// gate delegates to <c>ClawRendTracker</c>, whose <c>Follow()</c> calls
    /// <c>Reset()</c> whenever the attack command changes - clearing the
    /// emitted flag along with the claw bookkeeping. So its once-per-sequence
    /// limit is genuinely per <i>command</i>, and a second qualifying pair in
    /// the same round rends again; the round tick only clears state that was
    /// already going to be cleared. The released Dire Ape and Girallon keep
    /// that gate and their behaviour unchanged - this is a Glabrezu-local
    /// component, not a rewrite of theirs.</para>
    ///
    /// <para><b>The printed rule is scoped to the round, both ways.</b> The
    /// Universal Monster Rules say two of the named attacks hitting the same
    /// opponent in the same round, usable once per round. Neither half
    /// mentions an attack command. So the victim bookkeeping is round-scoped
    /// too, which means a rend is correctly <i>allowed</i> when the two pincer
    /// hits land in different commands inside one round - an attack of
    /// opportunity and then a full attack, say - and correctly refused for the
    /// rest of that round once it has happened. A command-scoped pairing would
    /// have quietly under-triggered in exactly that legal case.</para>
    ///
    /// <para>The round boundary is the installed one and works in both modes:
    /// <c>ITickEachRound.OnNewRound</c> is raised by the base engine's
    /// <c>UnitTicksController</c> every six seconds of game time outside
    /// turn-based combat, and by the turn controller inside it, so exactly one
    /// of the two delivers it per round. No fixture flag and no wall clock.</para>
    ///
    /// <para>Everything it owns is session-scoped and per unit: held in a weak
    /// table keyed on the live unit, never serialized, and dropped when the
    /// carrying fact turns off. One Glabrezu's rend allowance is its own, so a
    /// second summon neither consumes nor refills it. An attack that reaches
    /// the rulebook with no live attack command - a replay, a script, a direct
    /// rulebook trigger - never rends, because a rend is something an attack
    /// sequence does.</para>
    /// </summary>
    [Serializable]
    public sealed class GlabrezuRendGate38 :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ConditionalWeakTable<UnitEntityData,
            GlabrezuRendRound38> States =
            new ConditionalWeakTable<UnitEntityData, GlabrezuRendRound38>();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Pincer;

        /// <summary>
        /// How many of the named attacks this creature's printed rend needs.
        /// Two pincers for the Glabrezu, set by the builder from the printed
        /// line so the component never guesses.
        /// </summary>
        public int RendPincerCount = 2;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Mine(evt);
            if (owner == null) return;
            int pincer = PincerLimb(owner, evt.Weapon);
            if (pincer < 1) return;
            long stamp = CurrentRoundStamp();
            lock (States)
            {
                GlabrezuRendRound38 state = StateFor(owner);
                state.BeginRoundIfChanged(stamp);
                if (state.TryArm(true, HasLiveAttackCommand(owner),
                        pincer, evt.Target)) evt.IsRend = true;
            }
        }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Mine(evt);
            if (owner == null) return;
            int pincer = PincerLimb(owner, evt.Weapon);
            if (pincer < 1) return;
            bool hit = evt.AttackRoll != null && evt.AttackRoll.IsHit;
            long stamp = CurrentRoundStamp();
            lock (States)
            {
                GlabrezuRendRound38 state = StateFor(owner);
                state.BeginRoundIfChanged(stamp);
                state.RecordOutcome(pincer, evt.Target, hit);
            }
        }

        /// <summary>
        /// Which round the game is in, as a count of elapsed round durations.
        ///
        /// <para>This, and not <see cref="OnNewRound"/>, is what actually
        /// returns the printed once-per-round allowance. The engine dispatches
        /// <c>ITickEachRound</c> through the unit's LOGIC fact collection -
        /// both the base <c>UnitTicksController</c> and the turn-based
        /// <c>TurnController</c> call <c>unit.Logic.CallFactComponents</c> -
        /// so a component carried by a FEATURE, as this one is, never receives
        /// it. A guarded run proved it: the owner sat in real combat for 975
        /// unpaused frames with its rend spent and the allowance never came
        /// back.</para>
        ///
        /// <para>Game time is the right replacement rather than a convenient
        /// one. It advances in real-time play and it advances by one round
        /// duration per turn-based round, which is how every buff duration in
        /// this game is already measured, so the same stamp is correct in both
        /// modes and needs no mode test. <see cref="OnNewRound"/> is kept and
        /// still resets, so if the callback does arrive the result is the
        /// same.</para>
        /// </summary>
        private static long CurrentRoundStamp()
        {
            if (Game.Instance == null || Game.Instance.Player == null)
                return long.MinValue + 1;
            return (long)(Game.Instance.Player.GameTime.TotalSeconds /
                GameConsts.RoundDuration);
        }

        /// <summary>The round's one rend comes back, and pairs are forgotten.</summary>
        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null) return;
            lock (States) { StateFor(Owner.Unit).BeginRound(); }
        }

        public override void OnTurnOff()
        {
            if (Owner != null && Owner.Unit != null)
                lock (States) { States.Remove(Owner.Unit); }
            base.OnTurnOff();
        }

        /// <summary>
        /// Whether this attack came from a live attack command. Not used as a
        /// reset key - that is the released gate's mistake - only as the
        /// printed requirement that a rend belongs to an attack sequence.
        /// </summary>
        private static bool HasLiveAttackCommand(UnitEntityData owner)
        {
            return owner != null && owner.Commands != null &&
                owner.Commands.Attack != null;
        }

        private UnitEntityData Mine(RuleAttackWithWeapon evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                Pincer == null || OwningBlueprint == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint))
                return null;
            return Owner.Unit;
        }

        /// <summary>
        /// Which pincer limb an attack came from, or zero. The two pincers
        /// share one weapon blueprint, so they are told apart by the limb slot
        /// they occupy. The claws carry a different blueprint and the bite
        /// sits in the primary hand, so neither can ever reach this.
        /// </summary>
        private int PincerLimb(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (weapon == null || weapon.Blueprint == null ||
                !ReferenceEquals(weapon.Blueprint, Pincer)) return 0;
            int index;
            if (SummonLimbs.Classify(owner, weapon, out index) !=
                SummonLimbKind.Additional) return 0;
            return index >= 0 && index < RendPincerCount ? index + 1 : 0;
        }

        private GlabrezuRendRound38 StateFor(UnitEntityData owner)
        {
            GlabrezuRendRound38 state;
            if (States.TryGetValue(owner, out state))
            {
                if (state.PincerCount == RendPincerCount) return state;
                States.Remove(owner);
            }
            state = new GlabrezuRendRound38(RendPincerCount);
            States.Add(owner, state);
            return state;
        }

        /// <summary>For the guarded runtime fixture: this unit's live state.</summary>
        internal static GlabrezuRendRound38 ObservedState(UnitEntityData unit)
        {
            if (unit == null) return null;
            GlabrezuRendRound38 state;
            lock (States)
            { return States.TryGetValue(unit, out state) ? state : null; }
        }

        /// <summary>For the guarded runtime fixture: forget this unit's state.</summary>
        internal static void ForgetObserved(UnitEntityData unit)
        {
            if (unit == null) return;
            lock (States) { States.Remove(unit); }
        }
    }

    /// <summary>
    /// Power Word Stun, resolved against the recipient's <b>current</b> hit
    /// points at the moment the effect lands.
    ///
    /// <para>The printed spell has <b>no saving throw at any threshold</b>, so
    /// this action asks for none. A recipient escapes only in one of three
    /// ways, and each is owned by the consumer that already exists for it
    /// rather than reimplemented here:</para>
    /// <list type="bullet">
    /// <item><b>Above the printed ceiling.</b> At 151 or more current hit
    /// points the recipient is unaffected and nothing at all is applied - no
    /// buff, no zero-round stun, no damage.</item>
    /// <item><b>Native condition or descriptor immunity.</b> The carrier buff
    /// applies the native stun condition and carries the mind-affecting,
    /// compulsion and stun descriptors, so the engine's own
    /// <c>BuffDescriptorImmunity</c> and condition-immunity consumers refuse
    /// it. This action never pre-filters them away from their own
    /// consumers.</item>
    /// <item><b>Spell resistance.</b> Verified from the installed engine
    /// rather than assumed: <c>AbilityExecutionProcess.DoApplyEffect</c>
    /// triggers <c>RuleSpellResistanceCheck</c> for every unit-targeted,
    /// non-self ability and returns without applying the effect when the
    /// check reports resisted. Its <c>SpellPenetration</c> is the execution
    /// context's own caster level, and <c>PrepareCast</c> has already called
    /// <c>Recalculate()</c> by then - which honours this project's
    /// <c>ContextCalculateAbilityParams</c> - so penetration is the printed 14
    /// rather than the creature's 12 hit dice. No project code is needed for
    /// any of it.</item>
    /// </list>
    ///
    /// <para>The duration is rolled from real native dice through
    /// <c>RuleRollDice</c>, so a receipt can separate the bucket decision -
    /// which is deterministic - from the stochastic number of rounds inside
    /// that bucket.</para>
    /// </summary>
    [Serializable]
    public sealed class ContextActionGlabrezuPowerWordStun38 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility PowerWordStun;
        public BlueprintBuff Stunned;

        public override string GetCaption()
        {
            return "Stun one creature for a duration set by its current hit points";
        }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (Stunned == null || PowerWordStun == null || Context == null ||
                !ReferenceEquals(Context.SourceAbility, PowerWordStun) ||
                !GlabrezuIdentity38.IsLiveTarget(target) ||
                !GlabrezuIdentity38.IsExactSource(caster, SourceUnit)) return;
            if (ReferenceEquals(Context.Params, AbilityParams.Empty))
                throw new InvalidOperationException(
                    "Power Word Stun requires its owned ability parameters.");

            // Current hit points, read at resolution. Not the maximum, and not
            // a value captured when the command was queued.
            int dice = GlabrezuRules38Policy.PowerWordStunDiceCount(
                target.HPLeft);
            if (dice <= 0) return;

            var resolution = new MechanicsContext(caster, target.Descriptor,
                PowerWordStun, Context, new TargetWrapper(target));
            var roll = new RuleRollDice(caster, new DiceFormula(dice,
                DiceType.D4)) { Reason = resolution };
            int rounds = resolution.TriggerRule(roll).Result;
            if (rounds <= 0) return;
            MechanicsContext victim = AstralVictimEffectContext.Create(caster,
                target, Stunned, new AbilityParams {
                    CasterLevel =
                        GlabrezuRules38Policy.SpellLikeCasterLevel,
                    SpellLevel =
                        GlabrezuRules38Policy.PowerWordStunSpellLevel,
                    DC = Context.Params.DC });
            var apply = new RuleApplyBuff(target, Stunned, victim,
                TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds),
                (buff, source, duration) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, duration));
            resolution.TriggerRule(apply);
        }
    }

    /// <summary>
    /// The one suitability decision for Power Word Stun's AI, used by both the
    /// actor-level precheck and the per-candidate target consideration so the
    /// two can never disagree.
    /// </summary>
    internal static class GlabrezuStunSuitability38
    {
        /// <summary>
        /// Whether this candidate is a recipient the ability can actually
        /// affect. Every gate is one the real effect also meets, read from the
        /// live unit: the printed hit-point ceiling, the native stun condition
        /// and its immunity, and descriptor immunity.
        ///
        /// <para>Descriptor immunity goes through the released
        /// <see cref="SummonDescriptorImmunity"/> pair rather than
        /// <c>UnitPartSpellResistance.IsImmune</c> alone. That part answers
        /// only for <c>SpellImmunityToSpellDescriptor</c>; a creature
        /// protected solely by a <c>BuffDescriptorImmunity</c> - which cancels
        /// the application as it happens and never registers with the part -
        /// would otherwise read as a usable target and absorb the single daily
        /// use. Numerical spell resistance is deliberately <b>not</b> consulted
        /// here: it is a roll the engine makes at resolution, not a property
        /// of the target, and inventing a synthetic roll during scoring would
        /// be a second, wrong adjudication.</para>
        /// </summary>
        internal static bool IsUsableRecipient(UnitEntityData actor,
            UnitEntityData target, BlueprintAbility ability)
        {
            if (actor == null || ability == null ||
                !GlabrezuIdentity38.IsLiveTarget(target) ||
                !actor.IsEnemy(target) ||
                !actor.Memory.ContainsVisible(target) ||
                !actor.HasLOS(target) ||
                GlabrezuRules38Policy.PowerWordStunDiceCount(
                    target.HPLeft) <= 0 ||
                target.Descriptor.State.HasCondition(
                    UnitCondition.Stunned) ||
                target.Descriptor.State.HasConditionImmunity(
                    UnitCondition.Stunned)) return false;
            var effect = new MechanicsContext(actor, target.Descriptor,
                ability, null, new TargetWrapper(target));
            return !SummonDescriptorImmunity.IsImmune(target, effect);
        }

        /// <summary>
        /// Whether this Glabrezu could use the ability at all: it is the exact
        /// owner and the single daily use is still available.
        /// </summary>
        internal static bool CanAttempt(UnitEntityData actor,
            BlueprintUnit source, BlueprintAbility ability)
        {
            if (actor == null || actor.Blueprint == null || ability == null ||
                !GlabrezuIdentity38.IsExactSource(actor, source) ||
                Game.Instance == null || Game.Instance.State == null)
                return false;
            Ability granted = actor.Descriptor.Abilities.GetAbility(ability);
            return granted != null && new AbilityData(granted).IsAvailable;
        }
    }

    /// <summary>
    /// Actor-level precheck for Power Word Stun: is there any point in the
    /// brain considering this action at all.
    ///
    /// <para>This is deliberately <b>only</b> a precheck. The engine's own
    /// tooltip on <c>BlueprintAiAction.ActorConsiderations</c> reads
    /// "Considerations that only depend on deciding unit", and that is the
    /// whole problem with deciding target suitability here: an actor score of
    /// one says some enemy is usable, not that the enemy the native selector
    /// picks is. The per-candidate decision lives in
    /// <see cref="GlabrezuStunTargetEligibility38"/>, which the builder
    /// attaches to <c>TargetConsiderations</c>, where
    /// <c>DecisionContext.Target</c> is the actual candidate.</para>
    ///
    /// <para>There is no ally-safety predicate, and that is not an omission:
    /// Power Word Stun is single-target with no area and no collateral. The
    /// AI's refusal is a selection policy only - it imposes no resource,
    /// changes no eligibility, and does not touch what the player can command
    /// from the action bar.</para>
    /// </summary>
    public sealed class GlabrezuStunEligibility38 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility PowerWordStun;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (!GlabrezuStunSuitability38.CanAttempt(actor, SourceUnit,
                    PowerWordStun)) return 0f;
            return context.Enemies.Any(info =>
                GlabrezuStunSuitability38.IsUsableRecipient(actor,
                    info == null ? null : info.Unit, PowerWordStun))
                ? 1f : 0f;
        }
    }

    /// <summary>
    /// Per-candidate suitability for Power Word Stun. The engine scores every
    /// candidate target through <c>AiAction.ScoreTarget</c>, which multiplies
    /// each target consideration's score into that candidate's own
    /// <c>CurrentScore</c>, so returning zero removes that candidate from
    /// contention rather than merely discouraging it.
    ///
    /// <para>Without this, the single daily use could be spent on an enemy the
    /// printed spell cannot touch: the actor consideration would admit the
    /// action because some other enemy was eligible, and the resolving action
    /// would then correctly decline the recipient the selector had actually
    /// chosen - a spent standard action and a spent use for no effect.</para>
    /// </summary>
    public sealed class GlabrezuStunTargetEligibility38 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility PowerWordStun;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (!GlabrezuStunSuitability38.CanAttempt(actor, SourceUnit,
                    PowerWordStun)) return 0f;
            UnitEntityData candidate = context.Target == null ? null :
                context.Target.Unit;
            return GlabrezuStunSuitability38.IsUsableRecipient(actor,
                candidate, PowerWordStun) ? 1f : 0f;
        }
    }
}
