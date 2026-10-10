using System;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    // Runs immediately before the cloned native poison save, exactly as the
    // Giant Ant's equivalent does. The same MechanicsContext is retained by
    // the applied buff for later round saves, which is what keeps all six of
    // this poison's exposures at the difficulty class the sting that delivered
    // them produced.
    //
    // The hit dice are the printed five rather than a live read: a summoned
    // creature's hit dice do not change, and taking them from the policy keeps
    // the whole formula in one place. The Constitution bonus IS live, so a
    // buffed or weakened scorpion poisons for what it can actually support.
    [Serializable]
    public sealed class ContextActionSetGiantScorpionPoisonDc : ContextAction
    {
        public override string GetCaption()
        {
            return "Set Giant Scorpion poison DC";
        }

        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null ||
                Context.Params == null) return;
            Context.Params.DC = GiantScorpionRulesPolicy.PoisonDifficultyClass(
                GiantScorpionRulesPolicy.HitDice,
                Context.MaybeCaster.Stats.Constitution.Bonus);
        }
    }
}
