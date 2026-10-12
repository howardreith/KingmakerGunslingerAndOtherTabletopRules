using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    // Runs immediately before the cloned native save, exactly as the Giant
    // Ant's and the Giant Scorpion's equivalents do. The same MechanicsContext
    // is retained by the applied state for later round saves, which is what
    // keeps all five of rot's exposures at the difficulty class the bite that
    // delivered them produced - and what keeps an already-applied rot intact
    // when the bebelith that caused it dies or is dismissed, because the
    // number it needs was captured rather than looked up.
    //
    // The hit dice are the printed twelve rather than a live read: a summoned
    // creature's hit dice do not change, and taking them from the policy keeps
    // the whole formula in one place. The Constitution bonus IS live, so a
    // buffed or weakened bebelith rots for what it can actually support.
    [Serializable]
    public sealed class ContextActionSetBebelithRotDc : ContextAction
    {
        public override string GetCaption()
        {
            return "Set Bebelith rot DC";
        }

        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null ||
                Context.Params == null) return;
            Context.Params.DC = BebelithRulesPolicy.DifficultyClass(
                BebelithRulesPolicy.HitDice,
                Context.MaybeCaster.Stats.Constitution.Bonus);
        }
    }

    /// <summary>
    /// Captures the applied rot's base difficulty class from the creature
    /// that applied it.
    ///
    /// <para>Three guarded runs shaped this, and the third overturned what
    /// the first two suggested. The early runs saw rot's per-round saves at
    /// 27, 31 and 33 against a printed 23, with the VICTIM sometimes named
    /// as the caster, which looked like a difficulty class recomputed from
    /// whatever the context resolved to - the corruption the frozen contract
    /// forbids. So this component was added to pin it, and instrumented to
    /// say what it pinned.</para>
    ///
    /// <para>What it reported was `contextDcBefore=23; wrote=True;
    /// contextDcAfter=23`. The context already held the printed number
    /// before anything pinned it, and the saves still came out four and
    /// eight higher. That is Kingmaker's own poison stacking: in those cases
    /// the Bebelith lands a bite every round, each bite is another dose, and
    /// a stacked poison is harder to shake. Correct behaviour, and the
    /// review was wrong to demand otherwise.</para>
    ///
    /// <para>This stays because the other half of what it saw is real: the
    /// context's caster does sometimes resolve to the victim. Capturing the
    /// base from the applier at the moment the state turns on makes the
    /// number independent of that, which is what the contract asks - an
    /// already-applied victim-owned effect keeps the difficulty class the
    /// bite that caused it produced, whatever becomes of its source.</para>
    /// </summary>
    [Serializable]
    public sealed class BebelithRotStateDifficultyClass :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        /// <summary>
        /// What this component last pinned, and onto what. Read by the
        /// guarded review and by nothing else: a difficulty class that
        /// disagrees with the saves the engine then rolls is a finding, and
        /// a finding needs both numbers.
        /// </summary>
        internal static string LastPin = "none";

        public override void OnTurnOn()
        {
            UnitEntityData caster = Fact == null || Fact.MaybeContext == null
                ? null : Fact.MaybeContext.MaybeCaster;
            int constitution = caster == null || caster.Descriptor == null
                ? BebelithRulesPolicy.ConstitutionModifier
                : caster.Descriptor.Stats.Constitution.Bonus;
            int printed = BebelithRulesPolicy.DifficultyClass(
                BebelithRulesPolicy.HitDice, constitution);
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            bool wrote = false;
            int before = 0;
            if (context != null && context.Params != null)
            {
                before = context.Params.DC;
                context.Params.DC = printed;
                wrote = context.Params.DC == printed;
            }
            LastPin = "caster=" +
                (caster == null || caster.Blueprint == null ? "none"
                    : caster.Blueprint.name) +
                ";constitutionBonus=" + constitution +
                ";printed=" + printed +
                ";contextDcBefore=" + before +
                ";wrote=" + wrote +
                ";contextDcAfter=" + (context == null || context.Params == null
                    ? -1 : context.Params.DC);
        }

        public override void OnTurnOff() { }
    }
}
