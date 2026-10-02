using System;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    // Runs immediately before the cloned native poison save. The same
    // MechanicsContext is retained by the applied buff for later round saves.
    [Serializable]
    public sealed class ContextActionSetWaspPoisonDc : ContextAction
    {
        public override string GetCaption()
        {
            return "Set Giant Wasp poison DC";
        }

        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null ||
                Context.Params == null) return;
            Context.Params.DC = GiantWaspPoisonPolicy.DifficultyClass(
                Context.MaybeCaster.Stats.Constitution.Bonus);
        }
    }
}
