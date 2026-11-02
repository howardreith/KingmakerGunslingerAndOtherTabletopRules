using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed demon exemption, as an exact enumerated identity set.
    ///
    /// <para>Kingmaker 2.1.7b carries no demon subtype fact this project has
    /// ever been able to resolve, and "outsider", "evil" and "chaotic" are all
    /// broader categories than "demon". This follows the released Trumpet
    /// Archon exemption and the released goblinoid dander exemption: exact
    /// known identities, independent of faction, alignment or appearance. The
    /// limitation — that a native campaign demon outside this set is not
    /// exempt — is recorded rather than hidden.</para>
    /// </summary>
    internal static class VrockDemonIdentity37
    {
        internal static bool IsDemon(UnitEntityData target,
            BlueprintUnit[] actualDemons)
        {
            return target != null && target.Blueprint != null &&
                actualDemons != null && actualDemons.Any(value =>
                    value != null &&
                    ReferenceEquals(target.Blueprint, value));
        }

        internal static bool IsLiveTarget(UnitEntityData target)
        {
            return target != null && !target.Destroyed && target.IsInState &&
                !target.Descriptor.State.IsDead;
        }
    }

    /// <summary>
    /// Racial supernatural scaling for the screech and the spores. Both are
    /// racial abilities, not spells: the caster level is the racial Hit Dice,
    /// the spell level is zero, and the Fortitude DC follows the live
    /// Constitution. A fact-owned parameter rule covers the AbilityData preview
    /// and the real cast context alike, as the released Gorgon breath does.
    /// </summary>
    [Serializable]
    public sealed class VrockRacialParameters37 :
        RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility[] Abilities;

        public override void OnEventAboutToTrigger(
            RuleCalculateAbilityParams evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null || OwningBlueprint == null ||
                Abilities == null ||
                !ReferenceEquals(evt.Initiator, owner) ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !Abilities.Any(value => value != null &&
                    ReferenceEquals(evt.Blueprint, value))) return;
            evt.ReplaceCasterLevel = VrockRules37Policy.VrockHitDice;
            evt.ReplaceSpellLevel = 0;
            evt.ReplaceStat = StatType.Constitution;
            evt.ReplaceDC = VrockRules37Policy.ScreechDifficultyClass(
                owner.Stats.Constitution.Bonus);
        }

        public override void OnEventDidTrigger(
            RuleCalculateAbilityParams evt) { }
    }

    /// <summary>
    /// The activation/debit boundary for both recharging racial abilities.
    ///
    /// <para><c>IAbilityOnCastLogic.OnCast</c> is reached from
    /// <c>AbilityExecutionProcess.PrepareCast</c>, which runs exactly once when
    /// the ability actually begins executing. A command cancelled before that
    /// point never arrives here, so it neither spends the use nor produces a
    /// free second effect; once it does arrive, the marker buff is attached
    /// before any target is resolved and the native
    /// <c>AbilityCasterHasNoFacts</c> gate refuses the next activation. The
    /// attach is idempotent so a repeat under any native retry cannot extend
    /// or stack the wait.</para>
    ///
    /// <para>The marker is an ordinary native buff with a real duration, so a
    /// save keeps the remaining wait, a reload does not refill it, and two
    /// summoned vrocks hold independent markers. It is not a session flag.</para>
    /// </summary>
    [Serializable]
    public sealed class VrockRechargeOnCast37 : BlueprintComponent,
        IAbilityOnCastLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Ability;
        public BlueprintBuff Marker;
        public int Hours;
        public int Rounds;

        public void OnCast(AbilityExecutionContext context)
        {
            UnitEntityData caster = context == null ? null : context.Caster;
            if (caster == null || OwningBlueprint == null || Ability == null ||
                Marker == null ||
                !ReferenceEquals(caster.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(context.AbilityBlueprint, Ability)) return;
            if (Hours <= 0 == (Rounds <= 0))
                throw new InvalidOperationException(
                    "A Vrock recharge marker needs exactly one of hours or rounds.");
            if (caster.Buffs.GetBuff(Marker) != null) return;
            TimeSpan wait = Hours > 0 ? TimeSpan.FromHours(Hours) :
                TimeSpan.FromSeconds(GameConsts.RoundDuration * Rounds);
            // An internal clock must not inherit the offensive descriptor of
            // the ability's own context and be defeated by the owner's own
            // immunity, so it is attached with the owner as its source.
            if (caster.Descriptor.AddBuff(Marker, caster, wait) == null)
                throw new InvalidOperationException(
                    "Vrock recharge marker could not attach.");
        }
    }

    /// <summary>
    /// One collected creature's Stunning Screech resolution. The native
    /// all-creature radius supplies the targets; only the printed demon
    /// exemption is applied. Native stun condition and descriptor immunities
    /// keep their own consumers, and each failed save owns one finite victim
    /// effect that another source neither replaces nor extends.
    /// </summary>
    [Serializable]
    public sealed class ContextActionVrockScreech37 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Screech;
        public BlueprintUnit[] Demons;
        public BlueprintBuff Stunned;

        public override string GetCaption()
        { return "Resolve the stunning screech against one non-demon creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            bool exactSource = caster != null && SourceUnit != null &&
                caster.Blueprint != null &&
                ReferenceEquals(caster.Blueprint, SourceUnit);
            if (Stunned == null || Screech == null || Context == null ||
                !ReferenceEquals(Context.SourceAbility, Screech) ||
                !VrockDemonIdentity37.IsLiveTarget(target) ||
                !VrockRules37Policy.ScreechReaches(exactSource,
                    VrockDemonIdentity37.IsDemon(target, Demons))) return;
            if (ReferenceEquals(Context.Params, AbilityParams.Empty))
                throw new InvalidOperationException(
                    "The stunning screech requires its native owned parameters.");
            var resolution = new MechanicsContext(caster, target.Descriptor,
                Screech, Context, new TargetWrapper(target));
            int dc = Context.Params.DC;
            var saving = new RuleSavingThrow(target,
                SavingThrowType.Fortitude, dc) { Reason = resolution };
            resolution.TriggerRule(saving);
            if (saving.IsPassed) return;
            MechanicsContext victim = AstralVictimEffectContext.Create(caster,
                target, Stunned, new AbilityParams {
                    CasterLevel = VrockRules37Policy.VrockHitDice,
                    SpellLevel = 0, DC = dc });
            var apply = new RuleApplyBuff(target, Stunned, victim,
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    VrockRules37Policy.ScreechStunRounds),
                (buff, source, duration) => {
                    Buff added = target.Descriptor.Buffs.AddBuff(buff, source,
                        duration);
                    if (added != null) added.IsNotDispelable = true;
                    return added;
                });
            resolution.TriggerRule(apply);
        }
    }

    /// <summary>
    /// Whether one collected creature is actually exposed to the spores.
    ///
    /// <para>This is a native <c>ContextCondition</c> so that the immediate
    /// packet and the growing vines pass the same gate in the same order. The
    /// first shape ran an unconditional <c>ContextActionDealDamage</c> beside
    /// the vine application, which meant a creature the printed text protects
    /// still took the opening 1d8.</para>
    ///
    /// <para>Three gates, each from the printed entry. The exact source, so no
    /// other creature's effect resolves here. Genuine footprint adjacency,
    /// which is not the Vrock's ten-foot melee reach — see
    /// <see cref="SummonFootprintGeometry"/>. And the printed counterplay:
    /// "This attack can also be halted by effects that remove or provide
    /// immunity to disease", so a creature already immune to disease is not
    /// exposed at all. Poison immunity is deliberately not consulted: the
    /// printed ability is not poison, and the two descriptors are distinct
    /// bits in the engine's own enumeration.</para>
    /// </summary>
    [Serializable]
    public sealed class VrockSporeExposure37 : ContextCondition
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Spores;
        public BlueprintBuff Vines;

        protected override string GetConditionCaption()
        { return "adjacent, not already immune to disease"; }

        /// <summary>
        /// Whether the engine already protects this creature from the vine
        /// effect. Both native descriptor-immunity paths are asked, because
        /// the opening damage packet is decided here, before any buff is
        /// applied, and a creature protected only by
        /// <c>BuffDescriptorImmunity</c> would otherwise take that packet and
        /// only then have its vines refused.
        /// </summary>
        internal static bool IsDiseaseImmune(UnitEntityData target,
            MechanicsContext probe)
        { return SummonDescriptorImmunity.IsImmune(target, probe); }

        protected override bool CheckCondition()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            bool exactSource = caster != null && SourceUnit != null &&
                caster.Blueprint != null &&
                ReferenceEquals(caster.Blueprint, SourceUnit);
            if (!exactSource || Vines == null) return false;
            // The probe carries the vine effect, so it carries the vine's own
            // Disease descriptor and the native immunity part answers for the
            // exact thing that is about to be applied.
            var probe = new MechanicsContext(caster, target == null ? null :
                target.Descriptor, Vines, Context, new TargetWrapper(target));
            return VrockRules37Policy.SporesReach(exactSource,
                VrockDemonIdentity37.IsLiveTarget(target),
                SummonFootprintGeometry.AreAdjacent(caster, target),
                IsDiseaseImmune(target, probe));
        }
    }

    /// <summary>
    /// The growing vines on one exposed creature: a finite ten-round effect
    /// whose per-round damage is the native new-round action on its own buff,
    /// exactly as the released Giant Flytrap engulf does. The immediate 1d8 is
    /// a native <c>ContextActionDealDamage</c> beside this action, behind the
    /// same <see cref="VrockSporeExposure37"/> gate.
    /// </summary>
    [Serializable]
    public sealed class ContextActionVrockSpores37 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Spores;
        public BlueprintBuff Vines;

        public override string GetCaption()
        { return "Resolve vrock spores against one adjacent creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            bool exactSource = caster != null && SourceUnit != null &&
                caster.Blueprint != null &&
                ReferenceEquals(caster.Blueprint, SourceUnit);
            // The shared VrockSporeExposure37 gate has already decided
            // adjacency and the printed disease counterplay for this target;
            // these remain as the component's own ownership guards.
            if (Vines == null || Spores == null || Context == null ||
                !ReferenceEquals(Context.SourceAbility, Spores) ||
                !exactSource ||
                !VrockDemonIdentity37.IsLiveTarget(target)) return;
            if (ReferenceEquals(Context.Params, AbilityParams.Empty))
                throw new InvalidOperationException(
                    "Vrock spores require their native owned parameters.");
            var resolution = new MechanicsContext(caster, target.Descriptor,
                Spores, Context, new TargetWrapper(target));
            MechanicsContext victim = AstralVictimEffectContext.Create(caster,
                target, Vines, new AbilityParams {
                    CasterLevel = VrockRules37Policy.VrockHitDice,
                    SpellLevel = 0, DC = Context.Params.DC });
            var apply = new RuleApplyBuff(target, Vines, victim,
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    VrockRules37Policy.SporeVineRounds),
                (buff, source, duration) => {
                    // IsNotDispelable is deliberately left alone here.
                    // Native Remove Disease is a ContextActionDispelMagic with
                    // BuffType=All and the Disease descriptor, and
                    // TryDispelBuff rejects an IsNotDispelable buff before it
                    // ever looks at the descriptor. Setting that flag would
                    // have made the vines immune to the exact cure the printed
                    // entry requires. Ordinary Dispel Magic still cannot touch
                    // them: it runs with BuffType=FromSpell and this blueprint
                    // never sets IsFromSpell, so an extraordinary growth stays
                    // outside ordinary dispelling while a real disease cure
                    // reaches it.
                    return target.Descriptor.Buffs.AddBuff(buff, source,
                        duration);
                });
            resolution.TriggerRule(apply);
        }
    }

    /// <summary>
    /// Whether growth has been halted since the creature was exposed.
    ///
    /// <para>The printed entry says the attack "can also be halted by effects
    /// that remove or provide immunity to disease", which includes protection
    /// acquired while the vines are already growing. Neither native component
    /// does that on its own: <c>BuffDescriptorImmunity</c> only handles
    /// <c>RuleApplyBuff</c> as it happens, and
    /// <c>SpellImmunityToSpellDescriptor</c> merely registers an entry in
    /// <c>UnitPartSpellResistance</c> when it turns on. Neither sweeps facts
    /// that already exist, so a descriptor on the vine buff is not by itself
    /// proof of cancellation after the fact.</para>
    ///
    /// <para>This is the narrow hook that closes exactly that gap: it is asked
    /// once per round immediately before the growth damage, by the same
    /// predicate pair the exposure gate uses, and the halted branch runs the
    /// native self-removal action. It reaches only this vine instance on this
    /// creature, and it neither revives expired growth nor invents an
    /// indefinite immunity.</para>
    /// </summary>
    [Serializable]
    public sealed class VrockSporeGrowthHalted37 : ContextCondition
    {
        public BlueprintBuff Vines;

        protected override string GetConditionCaption()
        { return "disease protection acquired since exposure"; }

        protected override bool CheckCondition()
        {
            UnitEntityData owner = Target == null ? null : Target.Unit;
            if (owner == null || Vines == null) return false;
            var probe = new MechanicsContext(
                Context == null ? null : Context.MaybeCaster,
                owner.Descriptor, Vines, Context, new TargetWrapper(owner));
            return SummonDescriptorImmunity.IsImmune(owner, probe);
        }
    }

    /// <summary>
    /// The printed bless counter: "The spores can be destroyed by casting
    /// bless on the affected creatures."
    ///
    /// <para>This component rides the vine instance, so it is already scoped
    /// to one creature and one exposure. <c>IUnitBuffHandler</c> is broadcast
    /// globally by the native buff collection, so the handler matches the
    /// exact owner and the exact native bless effect before doing anything,
    /// and then removes only its own instance. Removal is idempotent and
    /// touches no other source's effect.</para>
    /// </summary>
    [Serializable]
    public sealed class VrockSporeBlessRemoval37 : BuffLogic, IUnitBuffHandler
    {
        public BlueprintBuff BlessEffect;

        public void HandleBuffDidAdded(Buff buff)
        {
            var vines = Fact as Buff;
            if (vines == null || !vines.Active || BlessEffect == null ||
                buff == null || Owner == null ||
                !ReferenceEquals(buff.Owner, Owner) ||
                !ReferenceEquals(buff.Blueprint, BlessEffect)) return;
            vines.Remove();
        }

        public void HandleBuffDidRemoved(Buff buff) { }
    }

    /// <summary>AI selection only. The real screech keeps all-creature
    /// eligibility and the player's own targeting is unchanged. This refuses
    /// the action whenever any non-exempt non-enemy stands inside the radius,
    /// and requires at least one visible, in-radius enemy that the stun could
    /// actually affect. It never ticks or controls another actor.</summary>
    public sealed class VrockScreechEligibility37 :
        Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Screech;
        public BlueprintUnit[] Demons;

        internal IEnumerable<UnitEntityData> EndangeredNonEnemies(
            UnitEntityData actor)
        {
            if (actor == null || Game.Instance == null ||
                Game.Instance.State == null ||
                Game.Instance.State.Units == null)
                return Enumerable.Empty<UnitEntityData>();
            float radius = new Feet(
                VrockRules37Policy.ScreechRadiusFeet).Meters;
            return Game.Instance.State.Units.All.Where(unit =>
                VrockDemonIdentity37.IsLiveTarget(unit) &&
                !ReferenceEquals(unit, actor) && !actor.IsEnemy(unit) &&
                !VrockDemonIdentity37.IsDemon(unit, Demons) &&
                Vector3.Distance(actor.Position, unit.Position) <=
                    radius + Math.Max(0f, unit.Corpulence));
        }

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (actor == null || actor.Blueprint == null ||
                SourceUnit == null || Screech == null ||
                !ReferenceEquals(actor.Blueprint, SourceUnit) ||
                Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.State.Units == null ||
                EndangeredNonEnemies(actor).Any()) return 0f;
            Ability granted = actor.Descriptor.Abilities.GetAbility(Screech);
            if (granted == null || !new AbilityData(granted).IsAvailable)
                return 0f;
            float radius = new Feet(
                VrockRules37Policy.ScreechRadiusFeet).Meters;
            return context.Enemies.Any(info => {
                UnitEntityData target = info == null ? null : info.Unit;
                if (!VrockDemonIdentity37.IsLiveTarget(target) ||
                    !actor.IsEnemy(target) ||
                    !actor.Memory.ContainsVisible(target) ||
                    !actor.HasLOS(target) ||
                    VrockDemonIdentity37.IsDemon(target, Demons) ||
                    target.Descriptor.State.HasCondition(
                        UnitCondition.Stunned) ||
                    target.Descriptor.State.HasConditionImmunity(
                        UnitCondition.Stunned) ||
                    Vector3.Distance(actor.Position, target.Position) > radius)
                    return false;
                var effect = new MechanicsContext(actor, actor.Descriptor,
                    Screech, null, new TargetWrapper(target));
                var immunity = target.Get<
                    Kingmaker.UnitLogic.Parts.UnitPartSpellResistance>();
                return immunity == null || !immunity.IsImmune(effect);
            }) ? 1f : 0f;
        }
    }

    /// <summary>AI selection only for the spore cloud. The printed ability
    /// affects every adjacent creature, so the brain refuses it whenever a
    /// non-enemy is inside the same radius and requires a real adjacent enemy.
    /// Vrocks are not themselves exempt from their own spores, so a nearby
    /// allied vrock still blocks the selection.</summary>
    public sealed class VrockSporeEligibility37 :
        Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Spores;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (actor == null || actor.Blueprint == null ||
                SourceUnit == null || Spores == null ||
                !ReferenceEquals(actor.Blueprint, SourceUnit) ||
                Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.State.Units == null) return 0f;
            Ability granted = actor.Descriptor.Abilities.GetAbility(Spores);
            if (granted == null || !new AbilityData(granted).IsAvailable)
                return 0f;
            // The same footprint relation the real effect applies, so AI
            // safety and eligibility cannot disagree about who is adjacent.
            if (Game.Instance.State.Units.All.Any(unit =>
                    VrockDemonIdentity37.IsLiveTarget(unit) &&
                    !ReferenceEquals(unit, actor) && !actor.IsEnemy(unit) &&
                    SummonFootprintGeometry.AreAdjacent(actor, unit)))
                return 0f;
            return context.Enemies.Any(info => {
                UnitEntityData target = info == null ? null : info.Unit;
                return VrockDemonIdentity37.IsLiveTarget(target) &&
                    actor.IsEnemy(target) &&
                    actor.Memory.ContainsVisible(target) &&
                    actor.HasLOS(target) &&
                    SummonFootprintGeometry.AreAdjacent(actor, target);
            }) ? 1f : 0f;
        }
    }
}
