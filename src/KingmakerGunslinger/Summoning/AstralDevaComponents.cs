using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Firing;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The deva's two finite victim effects keep their own lifetime
    /// and parameters. A parentless native context otherwise inherits the
    /// mutable global AbilityParams.Empty in Kingmaker 2.1.7b.</summary>
    internal static class AstralVictimEffectContext
    {
        private static readonly FieldInfo Parameters =
            typeof(MechanicsContext).GetField("m_Params",
                BindingFlags.Instance | BindingFlags.NonPublic);

        internal static MechanicsContext Create(UnitEntityData caster,
            UnitEntityData target, BlueprintBuff effect, AbilityParams parameters)
        {
            if (Parameters == null || Parameters.FieldType != typeof(AbilityParams))
                throw new MissingFieldException(typeof(MechanicsContext).FullName,
                    "m_Params");
            if (parameters == null || ReferenceEquals(parameters, AbilityParams.Empty))
                throw new ArgumentException("Victim effect requires private parameters.");
            var context = new MechanicsContext(caster, target.Descriptor,
                effect, null, new TargetWrapper(target));
            Parameters.SetValue(context, parameters);
            if (!ReferenceEquals(context.Params, parameters))
                throw new InvalidOperationException(
                    "Astral victim context did not retain its owned parameters.");
            return context;
        }
    }

    /// <summary>Astral Deva warhammer rider. It counts exact resolved hits by
    /// this unit's private hammer, asks for one Fortitude save after the second
    /// same-opponent hit in a round, and applies one independently expiring
    /// native Stunned condition for a native d6 rounds on failure.</summary>
    [Serializable]
    public sealed class AstralDevaHammerStun :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();
        private static readonly ConditionalWeakTable<UnitEntityData,
            AstralDevaStunTracker> Trackers =
            new ConditionalWeakTable<UnitEntityData, AstralDevaStunTracker>();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Warhammer;
        public BlueprintBuff Stunned;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData victim = evt == null ? null : evt.Target;
            RuleAttackRoll attack = evt == null ? null : evt.AttackRoll;
            if (owner == null || victim == null || victim.Destroyed ||
                victim.Descriptor.State.IsDead || OwningBlueprint == null ||
                Warhammer == null || Stunned == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || attack == null || !attack.IsHit ||
                !ReferenceEquals(evt.Initiator, owner) ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Weapon.Blueprint, Warhammer) ||
                string.IsNullOrEmpty(victim.UniqueId) || !Events.TryMark(attack))
                return;

            bool attempt;
            lock (Trackers)
            { attempt = TrackerFor(owner).RecordHit(victim.UniqueId); }
            if (!attempt) return;

            int dc = AstralDevaRulesPolicy.StunDifficultyClass(
                owner.Stats.Strength.Bonus);
            var context = AstralVictimEffectContext.Create(owner, victim,
                Stunned, new AbilityParams { DC = dc });
            var saving = new RuleSavingThrow(victim, SavingThrowType.Fortitude, dc)
            { Reason = context };
            context.TriggerRule(saving);
            if (saving.IsPassed) return;

            var durationRoll = new RuleRollDice(owner,
                new DiceFormula(1, DiceType.D6)) { Reason = context };
            int rounds = context.TriggerRule(durationRoll).Result;
            victim.Descriptor.Buffs.AddBuff(Stunned, context,
                TimeSpan.FromSeconds(rounds * 6d));
        }

        public void OnNewRound()
        { Forget(Owner == null ? null : Owner.Unit); }

        public override void OnTurnOff()
        {
            Forget(Owner == null ? null : Owner.Unit);
            base.OnTurnOff();
        }

        private static AstralDevaStunTracker TrackerFor(UnitEntityData owner)
        {
            AstralDevaStunTracker tracker;
            if (Trackers.TryGetValue(owner, out tracker)) return tracker;
            tracker = new AstralDevaStunTracker();
            Trackers.Add(owner, tracker);
            return tracker;
        }

        private static void Forget(UnitEntityData owner)
        {
            if (owner == null) return;
            lock (Trackers) { Trackers.Remove(owner); }
        }

        internal static AstralDevaStunTracker ObservedTracker(UnitEntityData owner)
        {
            if (owner == null) return null;
            AstralDevaStunTracker tracker;
            lock (Trackers)
            { return Trackers.TryGetValue(owner, out tracker) ? tracker : null; }
        }

        internal static void ForgetObserved(UnitEntityData owner)
        { Forget(owner); }
    }

    /// <summary>The lesser-globe half of one Astral Deva protective aura.
    /// The recipient buff's context identifies the exact emitter. Spells and
    /// SLAs of level three or lower that originate outside that emitter's
    /// twenty-foot aura are immune; an origin inside remains unaffected.</summary>
    [Serializable]
    public sealed class AstralProtectiveGlobe :
        RuleTargetLogicComponent<RuleSpellResistanceCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public int MaximumSpellLevel;
        public int RadiusFeet;
        private static readonly FieldInfo TargetIsImmune =
            typeof(RuleSpellResistanceCheck).GetField(
                "<TargetIsImmune>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        private void Apply(RuleSpellResistanceCheck evt)
        {
            UnitEntityData target = Owner == null ? null : Owner.Unit;
            UnitEntityData emitter = Fact == null || Fact.MaybeContext == null ?
                null : Fact.MaybeContext.MaybeCaster;
            UnitEntityData source = evt == null || evt.Context == null ?
                null : evt.Context.MaybeCaster;
            BlueprintAbility ability = evt == null || evt.Context == null ?
                null : evt.Context.SourceAbility;
            if (evt == null || target == null || emitter == null || source == null ||
                ability == null || Fact == null || !Fact.Active ||
                OwningBlueprint == null || !ReferenceEquals(emitter.Blueprint, OwningBlueprint) ||
                MaximumSpellLevel != AstralDevaRulesPolicy.LesserGlobeMaximumSpellLevel ||
                RadiusFeet != AstralDevaRulesPolicy.AuraRadiusFeet ||
                evt.Context.Params == null ||
                !AstralDevaRulesPolicy.BlocksLesserGlobe(
                    evt.Context.Params.SpellLevel,
                    ability.Type == AbilityType.Spell,
                    ability.Type == AbilityType.SpellLike,
                    Vector3.Distance(emitter.Position, source.Position),
                    source.Corpulence))
                return;
            if (TargetIsImmune == null || TargetIsImmune.FieldType != typeof(bool) ||
                TargetIsImmune.IsInitOnly)
                throw new MissingFieldException(typeof(RuleSpellResistanceCheck).FullName,
                    "<TargetIsImmune>k__BackingField");
            TargetIsImmune.SetValue(evt, true);
        }
    }

    /// <summary>Use the exact resource-backed Heal SLA only for a living,
    /// friendly target with meaningful damage. The ability owns its action,
    /// targeting, resource debit and native healing effect.</summary>
    public sealed class AstralHealingEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Heal;
        public int MinimumDamage;
        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            UnitEntityData target = context == null || context.Target == null ?
                null : context.Target.Unit;
            if (actor == null || target == null || Heal == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                actor.IsEnemy(target) || target.Destroyed ||
                target.Descriptor.State.IsDead || target.Descriptor.IsUndead ||
                target.Damage < MinimumDamage) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Heal);
            if (granted == null) return 0f;
            var data = new AbilityData(granted);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }

    /// <summary>PF1 Holy Smite resolution for one creature selected by the
    /// ability's native 20-foot burst. The ability framework owns range,
    /// action cost and per-target spell resistance; this action owns the
    /// alignment-sensitive dice, the real Will save and the one-round blind.</summary>
    [Serializable]
    public sealed class ContextActionAstralHolySmite : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintUnitFact OutsiderType;
        public BlueprintBuff Blinded;

        public override string GetCaption()
        { return "Resolve Astral Deva Holy Smite against one creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || target.Descriptor == null ||
                target.Destroyed || target.Descriptor.State.IsDead ||
                SourceUnit == null || OutsiderType == null || Blinded == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit)) return;

            Alignment alignment = target.Descriptor.Alignment.Value;
            if (alignment.HasComponent(AlignmentComponent.Good)) return;
            bool evil = alignment.HasComponent(AlignmentComponent.Evil);
            bool evilOutsider = evil && target.Descriptor.HasFact(OutsiderType);
            int dc = AstralDevaRulesPolicy.HolySmiteWillDc(
                caster.Stats.Charisma.ModifiedValue);
            var resolution = new MechanicsContext(caster, target.Descriptor,
                Context.SourceAbility, Context, new TargetWrapper(target));
            resolution.Params.CasterLevel = AstralDevaRulesPolicy.SpellLikeCasterLevel;
            resolution.Params.SpellLevel = AstralDevaRulesPolicy.HolySmiteSpellLevel;
            resolution.Params.DC = dc;
            var saving = new RuleSavingThrow(target, SavingThrowType.Will, dc) {
                Reason = resolution };
            resolution.TriggerRule(saving);

            DiceType die = evilOutsider ? DiceType.D6 : DiceType.D8;
            var roll = new RuleRollDice(caster, new DiceFormula(
                AstralDevaRulesPolicy.HolySmiteDiceCount(evilOutsider), die)) {
                Reason = resolution };
            int rolled = resolution.TriggerRule(roll).Result;
            int amount = AstralDevaRulesPolicy.HolySmiteDamage(rolled, evil,
                saving.IsPassed);
            var damage = new DirectDamage(new DiceFormula(0, DiceType.Zero), amount);
            // HolySmiteDamage already applies the save/alignment fraction; a
            // native HalfBecauseSavingThrow flag would reduce successful saves twice.
            var dealt = new RuleDealDamage(caster, target,
                new DamageBundle(new BaseDamage[] { damage })) {
                Reason = resolution,
                SourceAbility = Context.SourceAbility };
            resolution.TriggerRule(dealt);

            if (!AstralDevaRulesPolicy.HolySmiteBlinds(evil, saving.IsPassed))
                return;
            // Give the finite victim effect its own buff context. A context whose
            // associated blueprint is the caster-owned ability is retired with
            // that ability; this private victim state instead keeps its printed
            // one-round clock, while MaybeCaster still records exact ownership.
            var blindContext = AstralVictimEffectContext.Create(caster,
                target, Blinded, new AbilityParams {
                    CasterLevel = AstralDevaRulesPolicy.SpellLikeCasterLevel,
                    SpellLevel = AstralDevaRulesPolicy.HolySmiteSpellLevel,
                    DC = dc });
            var apply = new RuleApplyBuff(target, Blinded, blindContext,
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    AstralDevaRulesPolicy.HolySmiteBlindRounds),
                (buff, source, duration) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, duration));
            Rulebook.Trigger(apply);
            // Native buff construction retains this exact context, even if
            // AddBuff's return value is null. Mark only this cast's actual
            // victim state as a dispellable spell effect, like ApplyBuff does.
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Blinded) &&
                ReferenceEquals(value.Context, blindContext)))
                buff.IsFromSpell = true;
        }
    }

    /// <summary>Marker carried only by the two private Dispel Evil touch
    /// deliveries. It gives the native touch controller a precise ward to
    /// discharge without inspecting names or touching another held spell.</summary>
    [Serializable]
    public sealed class AstralDispelEvilDeliveryMarker : BlueprintComponent
    {
        public BlueprintBuff Ward;
    }

    /// <summary>The spell is cast on the deva and arms a later native melee
    /// touch. Unlike AbilityEffectStickyTouch this does not immediately cast
    /// the delivery when the parent target is the caster itself.</summary>
    [Serializable]
    public sealed class AbilityEffectArmAstralDispelEvilTouch : AbilityApplyEffect
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility TouchDeliveryAbility;
        public BlueprintBuff Ward;

        public override void Apply(AbilityExecutionContext context,
            TargetWrapper target)
        {
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            if (caster == null || caster.Descriptor == null || SourceUnit == null ||
                TouchDeliveryAbility == null || Ward == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit))
                throw new InvalidOperationException(
                    "Astral Dispel Evil has no exact caster, delivery, or ward.");

            // Recasting replaces only this Astral ward. Its turn-off handler
            // removes the prior exact held delivery before the new one is armed.
            foreach (Buff prior in caster.Descriptor.Buffs.Enumerable.Where(value =>
                value != null && ReferenceEquals(value.Blueprint, Ward)).ToArray())
                prior.Remove();
            int rounds = AstralDevaRulesPolicy.DispelEvilDurationRounds(
                context.Params.CasterLevel);
            Buff applied = caster.Descriptor.Buffs.AddBuff(Ward, context,
                TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds));
            if (applied == null)
                throw new InvalidOperationException(
                    "Astral Dispel Evil could not apply its exact ward.");
            applied.IsFromSpell = true;
            try
            {
                caster.Ensure<UnitPartTouch>().Init(TouchDeliveryAbility,
                    context.Ability, context.SourceAbilityContext);
            }
            catch
            {
                applied.Remove();
                throw;
            }
        }
    }

    /// <summary>When the finite ward expires, close only the future touch that
    /// belongs to one of its two private deliveries.</summary>
    [Serializable]
    public sealed class AstralDispelEvilWardLifetime : BuffLogic
    {
        public BlueprintAbility[] Deliveries;

        public override void OnTurnOff()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitPartTouch touch = owner == null ? null : owner.Get<UnitPartTouch>();
            BlueprintAbility held = touch == null || touch.Ability == null ||
                touch.Ability.Data == null ? null : touch.Ability.Data.Blueprint;
            if (owner != null && held != null && Deliveries != null &&
                Deliveries.Any(value => ReferenceEquals(value, held)))
                owner.Remove<UnitPartTouch>();
            base.OnTurnOff();
        }
    }

    /// <summary>The dismissal discharge is legal only against an evil
    /// extraplanar creature. The installed Dismissal graph retains its own
    /// save, spell-resistance, and native return-home behavior.</summary>
    [Serializable]
    public sealed class AstralDispelEvilDismissalTargetChecker :
        BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintUnitFact Extraplanar;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return caster != null && unit != null && unit.Descriptor != null &&
                Extraplanar != null && unit.Descriptor.HasFact(Extraplanar) &&
                unit.Descriptor.Alignment.Value.HasComponent(
                    AlignmentComponent.Evil);
        }
    }

    internal static class AstralDispelEvilEligibility
    {
        internal static bool CanDispel(Buff buff)
        {
            if (buff == null || buff.Context == null) return false;
            SpellDescriptor descriptors = SpellDescriptor.None;
            bool enchantment = false;
            MechanicsContext context = buff.Context;
            var contexts = new HashSet<MechanicsContext>();
            while (context != null && contexts.Add(context))
            {
                descriptors |= context.SpellDescriptor;
                enchantment |= context.SpellSchool == SpellSchool.Enchantment;
                context = context.ParentContext;
            }
            BlueprintAbility ability = buff.Context.SourceAbility;
            var abilities = new HashSet<BlueprintAbility>();
            while (ability != null && abilities.Add(ability))
            {
                descriptors |= ability.SpellDescriptor;
                enchantment |= ability.School == SpellSchool.Enchantment;
                ability = ability.Parent;
            }
            UnitEntityData source = buff.Context.MaybeCaster;
            bool evilCaster = source != null && source.Descriptor != null &&
                source.Descriptor.Alignment.Value.HasComponent(
                    AlignmentComponent.Evil);
            return AstralDevaRulesPolicy.CanDispelEvilEffect(buff.IsFromSpell,
                buff.IsNotDispelable,
                (descriptors & SpellDescriptor.Evil) != 0,
                enchantment, evilCaster);
        }
    }

    [Serializable]
    public sealed class AstralDispelEvilTargetChecker : BlueprintComponent,
        IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return caster != null && unit != null && unit.Descriptor != null &&
                unit.Descriptor.Buffs.Enumerable.Any(
                    AstralDispelEvilEligibility.CanDispel);
        }
    }

    /// <summary>Remove exactly one eligible creature buff through the native
    /// no-check RuleDispelMagic path. Highest caster level wins, matching the
    /// installed general dispel action's deterministic preference.</summary>
    [Serializable]
    public sealed class ContextActionAstralDispelEvil : ContextAction
    {
        public BlueprintUnit SourceUnit;

        public override string GetCaption()
        { return "Automatically dispel one evil spell or evil-caster enchantment"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || target.Descriptor == null ||
                SourceUnit == null || !ReferenceEquals(caster.Blueprint, SourceUnit))
                return;
            Buff selected = target.Descriptor.Buffs.Enumerable
                .Where(AstralDispelEvilEligibility.CanDispel)
                .OrderByDescending(value => value.Context == null ||
                    value.Context.Params == null ? 0 :
                    value.Context.Params.CasterLevel)
                .ThenBy(value => value.Blueprint.AssetGuid,
                    StringComparer.Ordinal).FirstOrDefault();
            if (selected == null) return;
            var dispel = new RuleDispelMagic(caster, target, selected,
                RuleDispelMagic.CheckType.None, StatType.Unknown);
            Context.TriggerRule(dispel);
        }
    }

    internal static class AstralDispelEvilRuntime
    {
        internal static void DischargeWard(AbilityExecutionContext context)
        {
            BlueprintAbility delivery = context == null ||
                context.Ability == null ? null : context.Ability.Blueprint;
            AstralDispelEvilDeliveryMarker marker = delivery == null ? null :
                delivery.GetComponent<AstralDispelEvilDeliveryMarker>();
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            if (marker == null || marker.Ward == null || caster == null ||
                caster.Descriptor == null) return;
            foreach (Buff ward in caster.Descriptor.Buffs.Enumerable.Where(value =>
                value != null && ReferenceEquals(value.Blueprint,
                    marker.Ward)).ToArray())
                ward.Remove();
        }
    }

    [HarmonyPatch(typeof(TouchSpellsController), "OnAbilityEffectApplied")]
    [HarmonyPriority(Priority.Last)]
    internal static class AstralDispelEvilDischargePatch
    {
        private static void Postfix(AbilityExecutionContext context)
        { AstralDispelEvilRuntime.DischargeWard(context); }
    }

}
