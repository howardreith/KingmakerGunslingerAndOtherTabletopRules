using System;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Summoning
{
    // The native descriptor modifier supplies and retires its temporary stat
    // modifier. Lullaby narrows that existing behavior to Will saves only.
    [Serializable]
    public sealed class LillendLullabySleepPenalty32 : SavingThrowBonusAgainstDescriptor
    {
        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt != null && evt.Type == SavingThrowType.Will) base.OnEventAboutToTrigger(evt);
        }
    }
}
