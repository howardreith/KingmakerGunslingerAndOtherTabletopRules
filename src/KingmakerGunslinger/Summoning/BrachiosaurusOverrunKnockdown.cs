using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed Overrun outcome for one creature's own command.
    ///
    /// <para><b>The printed contract.</b> A successful overrun moves you
    /// through the target's space; a result that beats the defender's CMD by
    /// <b>5 or more</b> also knocks it prone. Greater Overrun's rider is that
    /// an opponent <i>knocked prone by that overrun</i> provokes - not any
    /// success, and not a creature that was already lying down.</para>
    ///
    /// <para><b>Why this is not a second implementation of that threshold.</b>
    /// The engine already draws the line, and this reads its answer rather
    /// than recomputing one. <c>RuleCombatManeuver.SuccesValue</c> is
    /// <c>InitiatorCMValue - TargetCMD</c> - the real check total against the
    /// real effective defence - and <c>IsPartialSuccess</c> is
    /// <c>Success &amp;&amp; Type == Overrun &amp;&amp; SuccesValue &lt; 5</c>,
    /// against the engine's own <c>OverrunPartialSuccessThreshold</c> of 5.
    /// So "succeeded, but below the knockdown margin" is a native concept with
    /// a native constant, and a qualifying knockdown is exactly
    /// <c>Success &amp;&amp; !IsPartialSuccess</c>. No second roll is made and
    /// no margin is re-derived here.</para>
    ///
    /// <para><b>Why the knockdown is here and not in the ability's action
    /// list.</b> <c>AbilityCustomOverrun</c> runs its actions on <i>any</i>
    /// success - the IL branches on <c>!Success</c> alone - so an action list
    /// cannot tell an ordinary pass-through from a knockdown. The rule can,
    /// and this component sees the rule.</para>
    ///
    /// <para><b>Why the provocation is here and not the native component.</b>
    /// The native <c>ManeuverProvokeAttack</c> that the real Greater Overrun
    /// feat carries fires on <c>evt.Success</c> alone, which over-provokes
    /// against every ordinary pass-through and against a target that cannot be
    /// knocked off at all. This creature is granted a cloned carrier with that
    /// component removed, so there is exactly one provocation in play - this
    /// one - and nothing is stacked on top of the native behaviour or emitted
    /// and then undone. No other actor's feat is touched.</para>
    ///
    /// <para>Eligibility and prone state stay the engine's:
    /// <c>UnitHelper.CanBeKnockedOff</c> decides immunity,
    /// <c>ProneState.ShouldBeActive</c> is the native prone state that
    /// <c>UnitProneController</c> owns, and <c>IKnockOffHandler</c> is the
    /// native event the rest of the game listens to. A target already prone is
    /// not knocked down again and does not provoke, because no knockdown was
    /// caused.</para>
    /// </summary>
    public sealed class BrachiosaurusOverrunKnockdown :
        RuleInitiatorLogicComponent<RuleCombatManeuver>
    {
        public override void OnEventAboutToTrigger(RuleCombatManeuver evt) { }

        public override void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Overrun) return;

            // The engine's own threshold. Success alone is a pass-through.
            if (!evt.Success || evt.IsPartialSuccess) return;

            UnitEntityData target = evt.Target;
            if (target == null || target.Descriptor == null ||
                target.Descriptor.State == null ||
                target.Descriptor.State.Prone == null) return;

            // Immunity is the engine's answer, not a copy of its rule.
            if (!UnitHelper.CanBeKnockedOff(target)) return;

            // Already prone: this overrun knocked nobody down, so the printed
            // rider has no subject. Nothing is fabricated and nothing provokes.
            if (target.Descriptor.State.Prone.Active ||
                target.Descriptor.State.Prone.ShouldBeActive) return;

            target.Descriptor.State.Prone.ShouldBeActive = true;
            UnitEntityData initiator = evt.Initiator;
            EventBus.RaiseEvent<IKnockOffHandler>(handler =>
                handler.HandleKnockOff(initiator, target));

            // One provocation, for the knockdown this overrun caused. How many
            // responders answer it, and whether each still has an attack of
            // opportunity left, is the engine's accounting.
            Game game = Game.Instance;
            if (game != null && game.CombatEngagementController != null)
                game.CombatEngagementController
                    .ProvokeAttackOfOpportunity(target);
        }
    }
}
