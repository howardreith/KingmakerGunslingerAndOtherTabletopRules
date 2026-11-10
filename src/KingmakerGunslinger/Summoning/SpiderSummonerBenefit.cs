using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Runs once for each spawned creature, including one added by an existing
    /// count modifier, because <c>ContextActionSpawnMonster</c> runs its
    /// after-spawn actions per creature. It only ever grants the marker to an
    /// exact eligible species cast as a spell by a holder of the actual feat.
    /// </summary>
    [Serializable]
    public sealed class ContextActionApplySpiderSummonerBenefit : ContextAction
    {
        public override string GetCaption() { return "Apply Spider Summoner poison and web DC benefit"; }

        public override void RunAction()
        {
            UnitEntityData target = Target == null ? null : Target.Unit;
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            if (target == null || target.Descriptor == null || caster == null || caster.Descriptor == null) return;
            if (!SpiderSummonerBenefitPolicy.Receives(target.Blueprint.AssetGuid, SpiderSummonerRuntime.CastAsSpell(Context), SpiderSummonerRuntime.HasFeature(caster.Descriptor))) return;
            SpiderSummonerRuntime.Grant(target, caster);
        }
    }

    /// <summary>
    /// Adds the feat's +2 where the engine calculates these DCs: the poison
    /// features and the web ability calculate their parameters through
    /// <see cref="RuleCalculateAbilityParams"/>. The poison save and its
    /// continuing saves read those parameters through the context chain. The
    /// shared webbed buff has no parameter calculator of its own, so its
    /// break-free action reads the web's parameters too. The installed native
    /// action (UseCMB on, UseCMD off) uses that DC for its skill check; its
    /// CMB branch reads no DC, so <see cref="SpiderSummonerWebEscapeDifficulty"/>
    /// covers that branch. Nothing accumulates: each calculation is a new
    /// rule, answered by the one marker fact.
    /// </summary>
    [Serializable]
    public sealed class SpiderSummonerDcBonus : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || evt.Blueprint == null || !SpiderSummonerBenefitPolicy.AdjustsParametersOf(evt.Blueprint.AssetGuid)) return;
            // A fixed-DC calculator replaces the formula, so the bonus has to
            // be applied to its value instead of the formula's bonus.
            if (evt.ReplaceDC.HasValue) evt.ReplaceDC = evt.ReplaceDC.Value + SpiderSummonerCatalog.DcBonus;
            else evt.AddBonusDC(SpiderSummonerCatalog.DcBonus);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    /// <summary>
    /// On the feat marker. When the native webbed state's escape takes its
    /// CMB branch, it rolls a grapple maneuver against this spider's CMD and
    /// reads no DC; this adds the feat's +2 to that one calculation and to no
    /// other. The enclosing maneuver and its reason context identify the
    /// escape: a grapple against this spider, from the webbed state that this
    /// spider's Giant Spider web applied. No state is kept between rules.
    /// The marker is recognized as this component's own attached fact, not
    /// through the integration's static reference, so a marker a spider kept
    /// from an enabled session still answers after a reload with Expanded
    /// Summoning OFF, until the summon expires.
    /// </summary>
    [Serializable]
    public sealed class SpiderSummonerWebEscapeDifficulty : RuleTargetLogicComponent<RuleCalculateCMD>
    {
        public override void OnEventAboutToTrigger(RuleCalculateCMD evt)
        {
            if (evt == null || Owner == null) return;
            Buff own = Fact as Buff;
            RulebookEventContext rules = Rulebook.CurrentContext;
            RuleCombatManeuver maneuver = rules == null ? null : rules.PreviousEvent as RuleCombatManeuver;
            MechanicsContext context = maneuver == null || maneuver.Reason == null ? null : maneuver.Reason.Context ?? (maneuver.Reason.Fact == null ? null : maneuver.Reason.Fact.MaybeContext);
            UnitEntityData spider = Owner.Unit;
            bool adjusts = SpiderWebEscapePolicy.AdjustsEscapeCmd(evt.Type == CombatManeuver.Grapple,
                maneuver != null && maneuver.Type == CombatManeuver.Grapple,
                maneuver != null && ReferenceEquals(maneuver.Initiator, evt.Initiator),
                maneuver != null && ReferenceEquals(maneuver.Target, spider) && ReferenceEquals(evt.Target, spider),
                context == null || context.AssociatedBlueprint == null ? null : context.AssociatedBlueprint.AssetGuid,
                context == null || context.ParentContext == null || context.ParentContext.AssociatedBlueprint == null ? null : context.ParentContext.AssociatedBlueprint.AssetGuid,
                context != null && ReferenceEquals(context.MaybeCaster, spider),
                SpiderWebEscapePolicy.OwnMarkerHeld(own, own == null || own.Blueprint == null ? null : Owner.Buffs.GetBuff(own.Blueprint)));
            if (adjusts) evt.AddBonus(SpiderSummonerCatalog.DcBonus, Fact);
        }

        public override void OnEventDidTrigger(RuleCalculateCMD evt) { }
    }

    internal static class SpiderSummonerRuntime
    {
        internal static BlueprintFeature Feature { get; private set; }
        internal static BlueprintBuff Marker { get; private set; }

        internal static void Activate(BlueprintFeature feature, BlueprintBuff marker)
        { Feature = feature; Marker = marker; }

        internal static bool HasFeature(UnitDescriptor caster)
        {
            return Feature != null && caster.Progression.Features.Enumerable.Any(value => value != null && ReferenceEquals(value.Blueprint, Feature));
        }

        internal static bool CastAsSpell(MechanicsContext context)
        {
            BlueprintAbility ability = context == null ? null : context.SourceAbility;
            return ability != null && ability.Type == AbilityType.Spell;
        }

        /// <summary>Adds the marker once, under no spell context, then
        /// recalculates the creature's already calculated poison parameters.</summary>
        internal static void Grant(UnitEntityData target, UnitEntityData caster)
        {
            if (Marker == null) return;
            if (target.Descriptor.Buffs.GetBuff(Marker) == null)
            {
                Buff applied = target.Descriptor.AddBuff(Marker, caster, null);
                if (applied != null) applied.IsNotDispelable = true;
            }
            foreach (Feature feature in target.Descriptor.Progression.Features.Enumerable)
            {
                if (feature == null || feature.Blueprint == null || !SpiderSummonerBenefitPolicy.AdjustsParametersOf(feature.Blueprint.AssetGuid)) continue;
                feature.MaybeContext?.Recalculate();
            }
        }
    }
}
