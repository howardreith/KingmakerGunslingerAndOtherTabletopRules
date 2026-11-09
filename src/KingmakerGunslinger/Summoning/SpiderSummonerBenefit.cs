using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
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
    /// Adds the feat's +2 where the engine actually calculates these DCs. The
    /// poison features and the web ability calculate their parameters through
    /// <see cref="RuleCalculateAbilityParams"/>, and the poison's continuing
    /// saves and the webbed state's break-free check read those parameters
    /// from the context chain. Nothing accumulates: each calculation is a new
    /// rule, and only the one marker fact on a creature answers it.
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
