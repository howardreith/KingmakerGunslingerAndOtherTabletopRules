using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal static class TrumpetBlastRulesPolicy
    {
        internal const int RacialHitDice = 14;
        internal const int RadiusFeet = 100;
        internal const int MinimumRounds = 1;
        internal const int MaximumRounds = 4;
        internal static int DifficultyClass(int liveCharisma)
        {
            return 10 + RacialHitDice / 2 +
                (int)Math.Floor((liveCharisma - 10) / 2.0);
        }

        internal static bool HoldsInstrument(UnitEntityData caster,
            BlueprintUnit source, BlueprintItemWeapon sword)
        {
            if (caster == null || caster.Destroyed || !caster.IsInState ||
                caster.Descriptor.State.IsDead || source == null || sword == null ||
                !ReferenceEquals(caster.Blueprint, source)) return false;
            var item = caster.Body?.PrimaryHand?.MaybeWeapon;
            return item != null && ReferenceEquals(item.Blueprint, sword) &&
                ReferenceEquals(item.Owner, caster.Descriptor) &&
                ReferenceEquals(item.Wielder?.Unit, caster) &&
                item.HoldingSlot != null &&
                ReferenceEquals(item.HoldingSlot.MaybeItem, item) &&
                !item.IsLootable;
        }

        internal static bool IsArchon(UnitEntityData target,
            BlueprintUnit[] actualArchons)
        {
            // Exact known archon identities, independent of faction. Aasimar
            // ArchonHeritage, a circle, alignment or wings do not imply subtype.
            return target != null && actualArchons != null &&
                actualArchons.Any(value => value != null &&
                    ReferenceEquals(target.Blueprint, value));
        }
    }


    /// <summary>AI selection only. The real blast retains all-creature
    /// eligibility. Nearby nonenemies are conservatively protected even when
    /// immune; useful enemies must be visible in the native decision memory.</summary>
    public sealed class TrumpetBlastEligibility : Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintItemWeapon Instrument;
        public BlueprintAbility Blast;
        public BlueprintUnit[] Archons;

        internal System.Collections.Generic.IEnumerable<UnitEntityData> NearbyNonArchonNonEnemies(UnitEntityData actor)
        {
            if (actor == null || Game.Instance?.State?.Units == null)
                return Enumerable.Empty<UnitEntityData>();
            float radius = new Feet(TrumpetBlastRulesPolicy.RadiusFeet).Meters;
            return Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && unit.IsInState &&
                !unit.Descriptor.State.IsDead && !ReferenceEquals(unit, actor) && !actor.IsEnemy(unit) &&
                !TrumpetBlastRulesPolicy.IsArchon(unit, Archons) &&
                Vector3.Distance(actor.Position, unit.Position) <= radius + Math.Max(0f, unit.Corpulence));
        }
        public override float Score(Kingmaker.Controllers.Brain.DecisionContext context)
        {
            var actor = context?.Unit;
            if (!TrumpetBlastRulesPolicy.HoldsInstrument(actor, SourceUnit, Instrument) ||
                Blast == null || Game.Instance?.State?.Units == null || NearbyNonArchonNonEnemies(actor).Any()) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Blast);
            if (granted == null || !new AbilityData(granted).IsAvailable) return 0f;
            float radius = new Feet(TrumpetBlastRulesPolicy.RadiusFeet).Meters;
            return context.Enemies.Any(info => {
                var target = info.Unit;
                if (target == null || target.Destroyed || !target.IsInState || target.Descriptor.State.IsDead ||
                    !actor.IsEnemy(target) || !actor.Memory.ContainsVisible(target) || !actor.HasLOS(target) ||
                    TrumpetBlastRulesPolicy.IsArchon(target, Archons) ||
                    target.Descriptor.State.HasCondition(UnitCondition.Paralyzed) ||
                    target.Descriptor.State.HasConditionImmunity(UnitCondition.Paralyzed) ||
                    Vector3.Distance(actor.Position, target.Position) > radius) return false;
                var effect = new MechanicsContext(actor, actor.Descriptor, Blast, null, new TargetWrapper(target));
                var immunity = target.Get<Kingmaker.UnitLogic.Parts.UnitPartSpellResistance>();
                return immunity == null || !immunity.IsImmune(effect);
            }) ? 1f : 0f;
        }
    }

    [Serializable]
    public sealed class TrumpetBlastAccess : BlueprintComponent, IAbilityCasterChecker
    {
        public BlueprintUnit SourceUnit;
        public BlueprintItemWeapon Instrument;
        public bool CorrectCaster(UnitEntityData caster)
        { return TrumpetBlastRulesPolicy.HoldsInstrument(caster, SourceUnit, Instrument); }
        public string GetReason()
        { return "Requires this archon's own held trumpet/sword."; }
    }

    [Serializable]
    public sealed class TrumpetBlastParameters : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility Blast;
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var owner = Owner?.Unit;
            if (owner == null || !ReferenceEquals(owner.Blueprint, SourceUnit) ||
                !ReferenceEquals(evt?.Initiator, owner) ||
                !ReferenceEquals(evt.Blueprint, Blast)) return;
            // This is racial supernatural DC scaling, independent of the
            // cleric book's CL14/Wisdom progression. No spell slot is used.
            evt.ReplaceCasterLevel = TrumpetBlastRulesPolicy.RacialHitDice;
            evt.ReplaceSpellLevel = 0;
            evt.ReplaceStat = StatType.Charisma;
            evt.ReplaceDC = TrumpetBlastRulesPolicy.DifficultyClass(
                owner.Stats.Charisma.ModifiedValue);
        }
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    [Serializable]
    public sealed class ContextActionTrumpetBlast : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintItemWeapon Instrument;
        public BlueprintAbility Blast;
        public BlueprintUnit[] Archons;
        public BlueprintBuff Paralyzed;

        public override string GetCaption()
        { return "Resolve the trumpet blast against one non-archon creature"; }

        public override void RunAction()
        {
            var caster = Context?.MaybeCaster;
            var target = Target?.Unit;
            if (target == null || target.Destroyed || target.Descriptor.State.IsDead ||
                Paralyzed == null || Blast == null ||
                !ReferenceEquals(Context?.SourceAbility, Blast) ||
                !TrumpetBlastRulesPolicy.HoldsInstrument(caster, SourceUnit, Instrument) ||
                TrumpetBlastRulesPolicy.IsArchon(target, Archons)) return;

            // The native all-creature radius supplies targets. No ally or
            // alignment filter is added to the effect. Native paralysis
            // condition/descriptor immunities retain their own consumers.
            if (ReferenceEquals(Context.Params, AbilityParams.Empty))
                throw new InvalidOperationException("Trumpet blast requires its native owned parameters.");
            var resolution = new MechanicsContext(caster, target.Descriptor,
                Blast, Context, new TargetWrapper(target));
            int dc = Context.Params.DC;
            var saving = new RuleSavingThrow(target, SavingThrowType.Fortitude, dc)
                { Reason = resolution };
            resolution.TriggerRule(saving);
            if (saving.IsPassed) return;
            var roll = new RuleRollDice(caster, new DiceFormula(1, DiceType.D4))
                { Reason = resolution };
            int rounds = resolution.TriggerRule(roll).Result.Value;
            var victim = AstralVictimEffectContext.Create(caster, target,
                Paralyzed, new AbilityParams {
                    CasterLevel = TrumpetBlastRulesPolicy.RacialHitDice,
                    SpellLevel = 0, DC = dc });
            var apply = new RuleApplyBuff(target, Paralyzed, victim,
                TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds),
                (buff, context, duration) => {
                    var added = target.Descriptor.Buffs.AddBuff(buff, context, duration);
                    if (added != null) added.IsNotDispelable = true;
                    return added;
                });
            resolution.TriggerRule(apply);
        }
    }
}
