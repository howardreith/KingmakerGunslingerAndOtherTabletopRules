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
    /// Pins the applied rot's difficulty class to the printed number, on
    /// every save the applied state causes.
    ///
    /// <para>Two guarded runs shaped this. The first recorded rot's first
    /// save at the printed 23 and its per-round saves at 27, 31 and 33, with
    /// the VICTIM sometimes named as the caster - which is the corruption
    /// the frozen contract forbids: once the Bebelith is dismissed it would
    /// be the victim's own Constitution deciding how hard the victim's own
    /// saves are. Pinning the buff's context fixed the caster and steadied
    /// the number at 25, which is better and still not the printed 23, so
    /// something downstream of the context is adding to it.</para>
    ///
    /// <para>Chasing what would be guessing at the engine's arithmetic. The
    /// printed number is not negotiable and the save is where it has to be
    /// true, so this sets it on the rule. The match is on the save's own
    /// reason being this fact, which is what keeps it off every other save
    /// the victim makes, off every other creature's poison, and off
    /// everything at all once the state ends.</para>
    ///
    /// <para>The value is captured when the state turns on, from the
    /// creature that applied it. That is the contract's requirement stated
    /// precisely: the effect keeps the number the bite that caused it
    /// produced, whatever becomes of its source.</para>
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
