using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    // The accepted native Spell/Cantrip fact seam; only these private adult's
    // cantrips take their live parameters from its own bard book. Leveled
    // spells retain their native book slots and ordinary calculation.
    [Serializable]
    public sealed class LillendCantripParameters32 : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility[] Cantrips;
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || Owner?.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                !(Cantrips ?? Array.Empty<BlueprintAbility>()).Any(spell => ReferenceEquals(evt.Blueprint, spell))) return;
            var book = Owner.GetSpellbook(Book);
            if (book == null) return;
            evt.ReplaceCasterLevel = book.CasterLevel; evt.ReplaceSpellLevel = 0; evt.ReplaceStat = StatType.Charisma;
        }
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

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
