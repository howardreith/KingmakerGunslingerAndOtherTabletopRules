using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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
    /// check reports resisted. So spell resistance is applied by the engine
    /// before this action ever runs, which is exactly the printed "spell
    /// resistance: yes", and no project code is needed for it.</item>
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
    /// AI selection only for Power Word Stun. It scores zero unless there is
    /// a recipient the ability can actually affect, which keeps the brain from
    /// spending the single daily use on a creature the printed spell cannot
    /// touch and lets it fall through to the Glabrezu's other actions.
    ///
    /// <para>Deliberately not duplicated here: <b>range</b>. The native
    /// <c>BlueprintAiCastSpell</c> this consideration rides already applies
    /// the ability's own locator, target-checker and range restrictions, so
    /// restating a distance would create a second number that could disagree
    /// with the one the real cast uses.</para>
    ///
    /// <para>There is also no ally-safety predicate, and that is not an
    /// omission: Power Word Stun is single-target with no area and no
    /// collateral, so the released area-safety consideration shape would have
    /// nothing to protect. The AI's refusal is a selection policy only - it
    /// imposes no resource, changes no eligibility, and does not touch what
    /// the player can command from the action bar.</para>
    /// </summary>
    public sealed class GlabrezuStunEligibility38 : Consideration
    {
        public BlueprintUnit SourceUnit;
        public BlueprintAbility PowerWordStun;

        /// <summary>
        /// Whether one candidate is a recipient this ability can affect. The
        /// hit-point ceiling, native stun immunity and spell resistance are
        /// each the same gate the real effect meets, read from the live unit.
        /// </summary>
        internal bool IsUsableTarget(UnitEntityData actor,
            UnitEntityData target)
        {
            if (!GlabrezuIdentity38.IsLiveTarget(target) || actor == null ||
                !actor.IsEnemy(target) ||
                !actor.Memory.ContainsVisible(target) ||
                !actor.HasLOS(target) ||
                GlabrezuRules38Policy.PowerWordStunDiceCount(
                    target.HPLeft) <= 0 ||
                target.Descriptor.State.HasCondition(
                    UnitCondition.Stunned) ||
                target.Descriptor.State.HasConditionImmunity(
                    UnitCondition.Stunned)) return false;
            var effect = new MechanicsContext(actor, actor.Descriptor,
                PowerWordStun, null, new TargetWrapper(target));
            var resistance = target.Get<
                Kingmaker.UnitLogic.Parts.UnitPartSpellResistance>();
            return resistance == null || !resistance.IsImmune(effect);
        }

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            if (actor == null || actor.Blueprint == null ||
                SourceUnit == null || PowerWordStun == null ||
                !ReferenceEquals(actor.Blueprint, SourceUnit) ||
                Game.Instance == null || Game.Instance.State == null)
                return 0f;
            Ability granted = actor.Descriptor.Abilities.GetAbility(
                PowerWordStun);
            if (granted == null || !new AbilityData(granted).IsAvailable)
                return 0f;
            return context.Enemies.Any(info => IsUsableTarget(actor,
                info == null ? null : info.Unit)) ? 1f : 0f;
        }
    }
}
