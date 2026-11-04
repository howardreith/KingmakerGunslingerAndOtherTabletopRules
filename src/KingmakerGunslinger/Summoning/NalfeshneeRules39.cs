using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Designers;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal static class NalfeshneeIdentity39
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

        /// <summary>
        /// Whether this creature is exempt from Unholy Nimbus.
        ///
        /// <para>The exempt set is resolved by the BUILDER from the one
        /// authoritative <see cref="SummonDemonIdentityCatalog"/> and injected
        /// as exact blueprints, which is how the released Vrock screech does
        /// it. The component compares identities and never re-derives
        /// membership from alignment, the outsider type, faction or the view
        /// donor - each of which would give a different and wrong answer, and
        /// the first of which would wrongly exempt the registered Bebilith.
        /// Because both abilities read the same list, the screech's exemption
        /// and this one cannot drift apart.</para>
        /// </summary>
        internal static bool IsExemptDemon(UnitEntityData unit,
            BlueprintUnit[] exempt)
        {
            if (unit == null || unit.Blueprint == null || exempt == null)
                return false;
            for (int index = 0; index < exempt.Length; index++)
                if (ReferenceEquals(exempt[index], unit.Blueprint))
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The pending state of one Unholy Nimbus activation, carried by a buff on
    /// the Nalfeshnee itself.
    ///
    /// <para><b>Why a buff and not the engine's own delayed delivery.</b>
    /// <c>AbilityDeliverDelay</c> keys on <c>TimeController.GameTime</c>, which
    /// is the right clock in both combat modes, but the process that drives it
    /// - <c>AbilityExecutionProcess</c> - is a bare object whose whole pending
    /// state is a compiler-generated coroutine with no serialization at all, so
    /// a windup in flight would simply be lost on a cold load. A buff's
    /// <c>m_EndTime</c> and <c>m_Context</c> both carry <c>[JsonProperty]</c>
    /// and <c>BuffCollection</c> owns <c>PostLoad</c> and
    /// <c>UpdateNextEvent</c>, so the deadline is an absolute game time that
    /// survives the save and is re-armed after it.</para>
    ///
    /// <para><b>Why the burst is not hung on a per-round tick.</b> Both round
    /// dispatchers - <c>UnitTicksController.TickNextRound</c> and the
    /// turn-based <c>TurnController.Prepare</c> - call
    /// <c>unit.Logic.CallFactComponents&lt;ITickEachRound&gt;</c>, and
    /// <c>UnitLogicCollection</c> and <c>BuffCollection</c> are separate
    /// sibling collections. A component on a buff never receives that call,
    /// which is the same reason a component on a feature does not.
    /// <c>AddFactContextActions</c> makes this an easy trap because it exposes
    /// a <c>NewRound</c> list that a buff carrier will never see fire.</para>
    ///
    /// <para><b>A serialized deadline is not by itself exactly-once
    /// execution.</b> <c>OnFactDeactivate</c> runs for natural expiry, dispel,
    /// manual removal, owner death and module teardown alike, so a bare
    /// "has the time passed?" test would detonate on every one of them - a
    /// late cleanup callback after the deadline looks exactly like a due
    /// resolution. The resolution therefore requires an explicit
    /// <see cref="_armed"/> hand-off that only the due path can take, and it
    /// is taken under a lock so a duplicated or re-entrant callback finds it
    /// already spent.</para>
    /// </summary>
    [Serializable]
    public sealed class NalfeshneeNimbusWindup39 : BuffLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Nimbus;
        public BlueprintBuff Daze;
        /// <summary>
        /// The exact registered demon identities the burst spares, injected by
        /// the builder from the one authoritative catalog.
        /// </summary>
        public BlueprintUnit[] ExemptDemons;

        /// <summary>
        /// Set only while this windup is genuinely pending. The burst takes it
        /// once, under <see cref="_gate"/>; every other deactivation path -
        /// cancellation, dispel, death, module-off teardown - clears it
        /// without resolving. So one activation yields at most one burst even
        /// if the engine calls back twice.
        /// </summary>
        private bool _armed;
        private readonly object _gate = new object();

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            lock (_gate) { _armed = true; }
        }

        /// <summary>
        /// Every way this buff can end arrives here. Exactly one of them is a
        /// burst.
        /// </summary>
        public override void OnFactDeactivate()
        {
            bool resolve;
            lock (_gate)
            {
                resolve = _armed && IsDueResolution();
                _armed = false;
            }
            if (resolve) Burst();
            base.OnFactDeactivate();
        }

        /// <summary>
        /// Whether this deactivation is the windup actually coming due on a
        /// live, enabled owner in a running world - as opposed to the buff
        /// being removed early, the owner being destroyed or killed, the world
        /// being torn down, or the module being switched off.
        ///
        /// <para>Each clause rules out a specific way a burst could escape.
        /// The time comparison is deliberately the LAST of them and never the
        /// only one: a module-off teardown or a load-time cleanup can easily
        /// happen after the deadline has passed, and on a time test alone each
        /// would fire an offensive area effect with no player action behind
        /// it.</para>
        /// </summary>
        private bool IsDueResolution()
        {
            if (Owner == null || Buff == null) return false;
            UnitEntityData owner = Owner.Unit;
            // A destroyed, dead or detached summon resolves nothing. Pending
            // work does not outlive its source.
            if (!NalfeshneeIdentity39.IsLiveTarget(owner) ||
                !NalfeshneeIdentity39.IsExactSource(owner, OwningBlueprint))
                return false;
            // No running world, no burst. This is what keeps a teardown or a
            // partially reconstructed load from detonating.
            if (Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.Player == null ||
                !owner.IsInGame || owner.Descriptor == null) return false;
            // The owner has to still carry the ability this windup belongs to.
            // With the module disabled the granted fact is gone, so a teardown
            // that happens to run late cannot resolve.
            if (Nimbus == null || Daze == null ||
                owner.Descriptor.Abilities == null ||
                owner.Descriptor.Abilities.GetAbility(Nimbus) == null)
                return false;
            // Only now: has the recorded deadline actually arrived? An early
            // removal is strictly before it.
            return Buff.TimeLeft <= TimeSpan.Zero;
        }

        /// <summary>
        /// One activation, one burst. Recipients are chosen HERE, at the real
        /// burst boundary, from the world as it is now - so a creature that
        /// walked in during the windup is caught and one that walked out is
        /// not. Nothing is resolved against an activation snapshot.
        /// </summary>
        private void Burst()
        {
            UnitEntityData owner = Owner.Unit;
            int dc = NalfeshneeRules39Policy.SupernaturalDifficultyClass(
                NalfeshneeRules39Policy.NalfeshneeHitDice,
                owner.Descriptor.Stats.Charisma.Bonus);
            var parameters = new AbilityParams {
                CasterLevel = NalfeshneeRules39Policy.SpellLikeCasterLevel,
                SpellLevel = 0,
                DC = dc };
            foreach (UnitEntityData victim in Recipients(owner))
            {
                var effect = AstralVictimEffectContext.Create(owner, victim,
                    Daze, parameters);
                // The native save, with the live Charisma-derived difficulty.
                var save = new RuleSavingThrow(victim, SavingThrowType.Will,
                    dc) { Reason = effect };
                if (effect.TriggerRule(save).IsPassed) continue;
                // Native dice for the printed 1d10 rounds, so an observer can
                // separate the deterministic eligibility from the rolled
                // duration.
                var roll = new RuleRollDice(owner, new DiceFormula(
                    NalfeshneeRules39Policy.NimbusDazeDiceCount,
                    DiceType.D10)) { Reason = effect };
                int rounds = effect.TriggerRule(roll).Result;
                if (rounds <= 0) continue;
                var apply = new RuleApplyBuff(victim, Daze, effect,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds),
                    (buff, source, duration) =>
                        victim.Descriptor.Buffs.AddBuff(buff, source,
                            duration));
                effect.TriggerRule(apply);
            }
        }

        /// <summary>
        /// Who the burst catches. The printed rule says any NON-DEMON creature
        /// caught in the area, which is collateral and is kept as collateral:
        /// this is not filtered to enemies. A delayed effect cannot promise
        /// allies stay clear, and pretending otherwise by making it
        /// faction-safe would be a different ability.
        /// </summary>
        private IEnumerable<UnitEntityData> Recipients(UnitEntityData owner)
        {
            IEnumerable<UnitEntityData> around = GameHelper.GetTargetsAround(
                owner.Position,
                new Feet(NalfeshneeRules39Policy.NimbusRadiusFeet), false,
                false);
            if (around == null) throw new InvalidOperationException(
                "Kingmaker's native GetTargetsAround query returned null.");
            return around.Where(unit =>
                NalfeshneeIdentity39.IsLiveTarget(unit) &&
                !ReferenceEquals(unit, owner) &&
                !NalfeshneeIdentity39.IsExemptDemon(unit,
                    ExemptDemons)).ToArray();
        }

        /// <summary>For the guarded runtime fixture: is a windup pending.</summary>
        internal static bool ObservedPending(UnitEntityData unit,
            BlueprintBuff windup)
        {
            if (unit == null || windup == null || unit.Descriptor == null)
                return false;
            return unit.Descriptor.Buffs.GetBuff(windup) != null;
        }

        /// <summary>For the guarded runtime fixture: remaining windup time.</summary>
        internal static TimeSpan ObservedTimeLeft(UnitEntityData unit,
            BlueprintBuff windup)
        {
            if (unit == null || windup == null || unit.Descriptor == null)
                return TimeSpan.Zero;
            Buff buff = unit.Descriptor.Buffs.GetBuff(windup);
            return buff == null ? TimeSpan.Zero : buff.TimeLeft;
        }
    }

    /// <summary>
    /// Activating Unholy Nimbus: a free action that spends one of three daily
    /// uses and starts a one-round windup.
    ///
    /// <para><b>At most one pending windup per creature</b>, by the owner
    /// decision of 2026-11-04 for an ambiguity the printed entry leaves open.
    /// A second activation while one is pending is refused here, BEFORE the
    /// use is debited - so it costs nothing, does not replace or extend the
    /// pending deadline, and queues no second burst. There is deliberately no
    /// queue and therefore no scheduler.</para>
    /// </summary>
    [Serializable]
    public sealed class ContextActionNalfeshneeNimbus39 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Nimbus;
        public BlueprintBuff Windup;

        public override string GetCaption()
        {
            return "Begin an unholy nimbus that bursts one round later";
        }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            if (Windup == null || Nimbus == null || Context == null ||
                !ReferenceEquals(Context.SourceAbility, Nimbus) ||
                !NalfeshneeIdentity39.IsLiveTarget(caster) ||
                !NalfeshneeIdentity39.IsExactSource(caster, SourceUnit))
                return;
            // The one-pending rule, enforced before anything is spent.
            if (caster.Descriptor.Buffs.GetBuff(Windup) != null) return;
            if (ReferenceEquals(Context.Params, AbilityParams.Empty))
                throw new InvalidOperationException(
                    "Unholy Nimbus requires its owned ability parameters.");
            var effect = AstralVictimEffectContext.Create(caster, caster,
                Windup, new AbilityParams {
                    CasterLevel =
                        NalfeshneeRules39Policy.SpellLikeCasterLevel,
                    SpellLevel = 0,
                    DC = Context.Params.DC });
            // The deadline is the buff's own absolute end time, which is what
            // crosses a save. One round of game time, measured the way every
            // other duration in this game is measured, so it is correct in
            // real-time and turn-based play without a mode test.
            var apply = new RuleApplyBuff(caster, Windup, effect,
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    NalfeshneeRules39Policy.NimbusDelayRounds),
                (buff, source, duration) =>
                    caster.Descriptor.Buffs.AddBuff(buff, source, duration));
            effect.TriggerRule(apply);
        }
    }

    /// <summary>
    /// Greater Dispel Magic, targeted, with the printed contract the installed
    /// action cannot express.
    ///
    /// <para><b>What the native action actually does.</b>
    /// <c>ContextActionDispelMagic</c> offers exactly two behaviours:
    /// <c>m_StopAfterFirstRemoved</c> true removes one effect, false removes
    /// EVERY eligible effect. It has no success-count field at all, and the
    /// shipped <c>DispelMagicGreaterTarget</c> is the second of those - so the
    /// installed greater form is unbounded, while the printed one removes at
    /// most one effect per four caster levels, which is three here.</para>
    ///
    /// <para><b>Why the roll is made here instead of by
    /// <c>RuleDispelMagic</c>.</b> The printed targeted form makes ONE dispel
    /// check and applies that one result against the effects in order. The
    /// native rule cannot be told to reuse a roll: its <c>OnTrigger</c>
    /// unconditionally assigns <c>CheckRoll = Dice.D20</c> and recomputes
    /// <c>DC</c> from the effect's own context, with no guard, so a value set
    /// before triggering is always overwritten. Invoking it once per effect
    /// would therefore roll a fresh d20 each time, which is a different and
    /// more generous rule, and invoking the ordinary one-effect form three
    /// times would additionally retry effects that had already failed.</para>
    ///
    /// <para>So the check is rolled once through native dice, and each
    /// successful removal is still performed by the native rule with
    /// <c>CheckType.None</c> - traced to the branch of <c>OnTrigger</c> that
    /// removes without rolling - so the removal, its event and its attribution
    /// all stay native. Ordering and eligibility replicate the native action's
    /// own: sorted by descending effect caster level, and filtered by the same
    /// chain <c>TryDispelBuff</c> applies.</para>
    ///
    /// <para>Scope: this is the Nalfeshnee's own action. No released dispeller
    /// is altered, and the area and counterspell forms are untouched - only
    /// the targeted form is capped here, and the area variant retains whatever
    /// the installed game gives it.</para>
    /// </summary>
    [Serializable]
    public sealed class ContextActionNalfeshneeGreaterDispel39 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility GreaterDispel;

        public override string GetCaption()
        {
            return "Dispel up to one magical effect per four caster levels";
        }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (Context == null || GreaterDispel == null ||
                !ReferenceEquals(Context.SourceAbility, GreaterDispel) ||
                !NalfeshneeIdentity39.IsLiveTarget(target) ||
                !NalfeshneeIdentity39.IsExactSource(caster, SourceUnit))
                return;

            int casterLevel = Context.Params.CasterLevel;
            int cap = NalfeshneeRules39Policy.GreaterDispelMaxRemovals(
                casterLevel);
            if (cap <= 0) return;

            // ONE check for the whole casting, through native dice so it is a
            // real roll bound to this context rather than a private random.
            var roll = new RuleRollDice(caster, new DiceFormula(1,
                DiceType.D20)) { Reason = Context };
            int check = Context.TriggerRule(roll).Result;

            int removed = 0;
            foreach (Buff buff in Eligible(target))
            {
                if (removed >= cap) break;
                int effectCasterLevel = buff.Context.Params.CasterLevel;
                // The carried result, against this effect's own difficulty.
                // A failure here is final for this casting: the effect is
                // skipped and never retried.
                if (!NalfeshneeRules39Policy.DispelCheckBeats(casterLevel,
                        check, effectCasterLevel)) continue;
                // Native removal, native event, native attribution. The None
                // check type is the branch that removes without rolling, so
                // the carried result stays the one that decided this.
                var dispel = new RuleDispelMagic(caster, target, buff,
                    RuleDispelMagic.CheckType.None, StatType.Unknown);
                Context.TriggerRule(dispel);
                removed++;
            }
        }

        /// <summary>
        /// The ordered, eligible effects. Both the order and the filter
        /// replicate the installed action rather than inventing a policy:
        /// descending by the effect's own caster level, which is the native
        /// comparison, and the same eligibility chain
        /// <c>ContextActionDispelMagic.TryDispelBuff</c> walks - a dispelable
        /// buff that came from a spell.
        /// </summary>
        private static IEnumerable<Buff> Eligible(UnitEntityData target)
        {
            return target.Descriptor.Buffs.Enumerable
                .Where(buff => buff != null && !buff.IsNotDispelable &&
                    buff.IsFromSpell && buff.Context != null)
                .OrderByDescending(buff => buff.Context.Params.CasterLevel)
                .ToArray();
        }
    }

    /// <summary>
    /// The one suitability decision for Unholy Nimbus's AI, shared by the
    /// actor precheck and the per-candidate consideration so the two cannot
    /// disagree.
    /// </summary>
    internal static class NalfeshneeNimbusSuitability39
    {
        /// <summary>
        /// Whether this Nalfeshnee could begin a nimbus at all: it is the
        /// exact owner, a use remains, and nothing is already pending.
        /// </summary>
        internal static bool CanAttempt(UnitEntityData actor,
            BlueprintUnit source, BlueprintAbility nimbus,
            BlueprintBuff windup)
        {
            if (actor == null || nimbus == null || windup == null ||
                !NalfeshneeIdentity39.IsExactSource(actor, source) ||
                Game.Instance == null || Game.Instance.State == null)
                return false;
            if (actor.Descriptor.Buffs.GetBuff(windup) != null) return false;
            Ability granted = actor.Descriptor.Abilities.GetAbility(nimbus);
            return granted != null && new AbilityData(granted).IsAvailable;
        }

        /// <summary>
        /// Whether beginning a nimbus now is worth it, and ally-aware.
        ///
        /// <para>The real effect keeps its collateral; this is a selection
        /// policy only and changes no eligibility. The AI declines when the
        /// burst would catch a non-demon ally, which is conservative rather
        /// than a guarantee: the burst lands a round later and allies can walk
        /// in after the decision. That difference is real and is recorded
        /// rather than presented as safety.</para>
        /// </summary>
        internal static bool IsWorthwhile(UnitEntityData actor,
            BlueprintUnit[] exempt)
        {
            if (actor == null) return false;
            IEnumerable<UnitEntityData> around = GameHelper.GetTargetsAround(
                actor.Position,
                new Feet(NalfeshneeRules39Policy.NimbusRadiusFeet), false,
                false);
            if (around == null) return false;
            UnitEntityData[] caught = around.Where(unit =>
                NalfeshneeIdentity39.IsLiveTarget(unit) &&
                !ReferenceEquals(unit, actor) &&
                !NalfeshneeIdentity39.IsExemptDemon(unit,
                    exempt)).ToArray();
            if (caught.Length == 0) return false;
            if (caught.Any(unit => !actor.IsEnemy(unit))) return false;
            return caught.Any(unit => actor.Memory.ContainsVisible(unit));
        }
    }

    /// <summary>
    /// Actor-level precheck for Unholy Nimbus. There is no per-candidate
    /// consideration because the ability has no target: it is a free action on
    /// the creature itself whose burst is centred on the creature a round
    /// later, so every decision it can make is an actor decision.
    /// </summary>
    public sealed class NalfeshneeNimbusEligibility39 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Nimbus;
        public BlueprintBuff Windup;
        public BlueprintUnit[] ExemptDemons;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (!NalfeshneeNimbusSuitability39.CanAttempt(actor, SourceUnit,
                    Nimbus, Windup)) return 0f;
            return NalfeshneeNimbusSuitability39.IsWorthwhile(actor,
                ExemptDemons) ? 1f : 0f;
        }
    }

    /// <summary>
    /// The one suitability decision for Feeblemind's AI.
    /// </summary>
    internal static class NalfeshneeFeeblemindSuitability39
    {
        /// <summary>
        /// Whether this candidate is worth an at-will feeblemind. The printed
        /// spell can target anything, but spending a standard action on a
        /// creature that casts nothing achieves little, so the AI prefers a
        /// spellcaster - while the player keeps the unrestricted manual cast.
        ///
        /// <para>Spell resistance is deliberately NOT consulted. It is a roll
        /// the engine makes at resolution, not a property of the target, and
        /// inventing one during scoring would be a second, wrong
        /// adjudication.</para>
        /// </summary>
        internal static bool IsUsableRecipient(UnitEntityData actor,
            UnitEntityData target, BlueprintBuff condition)
        {
            if (actor == null || condition == null ||
                !NalfeshneeIdentity39.IsLiveTarget(target) ||
                !actor.IsEnemy(target) ||
                !actor.Memory.ContainsVisible(target) ||
                !actor.HasLOS(target)) return false;
            // Already feebleminded: a second casting adds nothing.
            if (target.Descriptor.Buffs.GetBuff(condition) != null)
                return false;
            var effect = new MechanicsContext(actor, target.Descriptor,
                null, null, new TargetWrapper(target));
            if (SummonDescriptorImmunity.IsImmune(target, effect))
                return false;
            return CastsSpells(target);
        }

        /// <summary>
        /// Whether the candidate actually casts. Read from real spellbooks
        /// rather than inferred from a high Intelligence, a class label or the
        /// presence of spell-like abilities - this creature itself has
        /// Intelligence 23 and a long spell-like list and no spellbook at all,
        /// which is exactly why the inference would be wrong.
        /// </summary>
        private static bool CastsSpells(UnitEntityData target)
        {
            return target.Descriptor.Spellbooks != null &&
                target.Descriptor.Spellbooks.Any();
        }

        internal static bool CanAttempt(UnitEntityData actor,
            BlueprintUnit source, BlueprintAbility feeblemind)
        {
            if (actor == null || feeblemind == null ||
                !NalfeshneeIdentity39.IsExactSource(actor, source) ||
                Game.Instance == null || Game.Instance.State == null)
                return false;
            Ability granted = actor.Descriptor.Abilities.GetAbility(
                feeblemind);
            return granted != null && new AbilityData(granted).IsAvailable;
        }
    }

    /// <summary>
    /// Actor-level precheck for Feeblemind: is there any point considering it.
    /// The per-candidate decision lives in
    /// <see cref="NalfeshneeFeeblemindTargetEligibility39"/>, because an actor
    /// score of one says SOME enemy is worth it, not that the enemy the native
    /// selector picks is.
    /// </summary>
    public sealed class NalfeshneeFeeblemindEligibility39 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Feeblemind;
        public BlueprintBuff Condition;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (!NalfeshneeFeeblemindSuitability39.CanAttempt(actor,
                    SourceUnit, Feeblemind)) return 0f;
            return context.Enemies.Any(info =>
                NalfeshneeFeeblemindSuitability39.IsUsableRecipient(actor,
                    info == null ? null : info.Unit, Condition)) ? 1f : 0f;
        }
    }

    /// <summary>
    /// Per-candidate suitability for Feeblemind. The engine multiplies each
    /// target consideration into that candidate's own score, so returning zero
    /// removes the candidate from contention rather than merely discouraging
    /// it.
    /// </summary>
    public sealed class NalfeshneeFeeblemindTargetEligibility39 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Feeblemind;
        public BlueprintBuff Condition;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (!NalfeshneeFeeblemindSuitability39.CanAttempt(actor,
                    SourceUnit, Feeblemind)) return 0f;
            UnitEntityData candidate = context.Target == null ? null :
                context.Target.Unit;
            return NalfeshneeFeeblemindSuitability39.IsUsableRecipient(actor,
                candidate, Condition) ? 1f : 0f;
        }
    }
}
