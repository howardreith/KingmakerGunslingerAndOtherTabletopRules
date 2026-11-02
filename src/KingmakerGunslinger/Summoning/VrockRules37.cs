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
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
    /// One collected creature's spore exposure. The printed extraordinary
    /// ability has no save and no spell resistance. The immediate packet is a
    /// native <c>ContextActionDealDamage</c> in the same action list, and this
    /// component adds only the growing vines: one finite ten-round effect
    /// whose per-round damage is carried by the native new-round action on its
    /// own buff, exactly as the released Giant Flytrap engulf does. It is not
    /// poison, disease or an aura, so no poison descriptor is attached and
    /// poison immunity does not apply; real damage resistances still do.
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
            if (Vines == null || Spores == null || Context == null ||
                !ReferenceEquals(Context.SourceAbility, Spores) ||
                !VrockRules37Policy.SporesReach(exactSource,
                    VrockDemonIdentity37.IsLiveTarget(target))) return;
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
                    Buff added = target.Descriptor.Buffs.AddBuff(buff, source,
                        duration);
                    // An extraordinary growth is not a spell effect, so the
                    // native dispel consumer must not treat it as one.
                    if (added != null) added.IsNotDispelable = true;
                    return added;
                });
            resolution.TriggerRule(apply);
        }
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
            float radius = new Feet(VrockRules37Policy.SporeRadiusFeet).Meters;
            if (Game.Instance.State.Units.All.Any(unit =>
                    VrockDemonIdentity37.IsLiveTarget(unit) &&
                    !ReferenceEquals(unit, actor) && !actor.IsEnemy(unit) &&
                    Vector3.Distance(actor.Position, unit.Position) <=
                        radius + Math.Max(0f, unit.Corpulence))) return 0f;
            return context.Enemies.Any(info => {
                UnitEntityData target = info == null ? null : info.Unit;
                return VrockDemonIdentity37.IsLiveTarget(target) &&
                    actor.IsEnemy(target) &&
                    actor.Memory.ContainsVisible(target) &&
                    actor.HasLOS(target) &&
                    Vector3.Distance(actor.Position, target.Position) <=
                        radius + Math.Max(0f, target.Corpulence);
            }) ? 1f : 0f;
        }
    }
}
