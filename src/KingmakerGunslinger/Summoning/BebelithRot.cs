using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
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
    /// Pins the applied rot's difficulty class so every exposure uses the
    /// number the bite that caused it produced.
    ///
    /// <para>The second guarded run of this sprint recorded rot's first save
    /// at the printed 23 and its per-round saves at 27, 31 and 33 - and on
    /// some of them the caster the engine named was the VICTIM, not the
    /// Bebelith. A difficulty class recomputed each round from whatever the
    /// context resolves to is a different poison every round, and once the
    /// source is dismissed it is the victim's own Constitution deciding how
    /// hard the victim's own saves are. The frozen contract names that
    /// exactly: source death or dismissal must not corrupt an
    /// already-applied victim-owned effect.</para>
    ///
    /// <para>So the number is captured when the state turns on, from the
    /// creature that applied it, into the context the per-round saves read.
    /// If that creature is somehow already gone the printed value is used
    /// rather than whatever is to hand, because a rot with the wrong source
    /// should still be the rot its block prints.</para>
    /// </summary>
    [Serializable]
    public sealed class BebelithRotStateDifficultyClass :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            if (context == null || context.Params == null) return;
            UnitEntityData caster = context.MaybeCaster;
            int constitution = caster == null || caster.Descriptor == null
                ? BebelithRulesPolicy.ConstitutionModifier
                : caster.Descriptor.Stats.Constitution.Bonus;
            context.Params.DC = BebelithRulesPolicy.DifficultyClass(
                BebelithRulesPolicy.HitDice, constitution);
        }

        public override void OnTurnOff() { }
    }
}
