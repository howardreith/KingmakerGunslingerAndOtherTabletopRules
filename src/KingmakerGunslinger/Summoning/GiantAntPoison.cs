using System;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    // Runs immediately before the cloned native poison save. The same
    // MechanicsContext is retained by the applied buff for later round saves,
    // which is what keeps the second, third and fourth exposures at the DC the
    // sting that delivered them produced.
    [Serializable]
    public sealed class ContextActionSetGiantAntPoisonDc : ContextAction
    {
        public override string GetCaption()
        {
            return "Set Giant Ant poison DC";
        }

        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null ||
                Context.Params == null) return;
            Context.Params.DC = GiantAntPoisonPolicy.DifficultyClass(
                Context.MaybeCaster.Stats.Constitution.Bonus);
        }
    }
}
