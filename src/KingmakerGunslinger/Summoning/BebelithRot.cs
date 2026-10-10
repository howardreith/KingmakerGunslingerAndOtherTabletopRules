using System;
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
}
