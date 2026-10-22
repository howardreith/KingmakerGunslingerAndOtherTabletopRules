using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    // Creation only. Native Spellbook serializes its own prepared/expended
    // slots. Post-load, fact activation, disarm and ordinary AI never Rest it.
    [Serializable]
    public sealed class TrumpetPreparedBook : OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Heal;
        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) || Book == null || Heal == null)
                throw new InvalidOperationException("Trumpet prepared book requires its exact owner and dependencies.");
            if (unit.Descriptor.GetSpellbook(Book) != null)
                throw new InvalidOperationException("Trumpet creation cannot reset an existing book.");
            var book = unit.Descriptor.DemandSpellbook(Book);
            for (int count = 0; book.CasterLevel < 14 && count < 14; count++) book.AddCasterLevel();
            if (book.CasterLevel != 14) throw new InvalidOperationException("Native Trumpet book is not CL14.");
            book.UpdateAllSlotsSize(false); book.AddKnown(6, Heal, true);
            for (int copy = 0; copy < 2; copy++)
                if (!book.Memorize(new AbilityData(Heal, book), null))
                    throw new InvalidOperationException("Native Trumpet Heal preparation failed.");
            book.Rest();
        }
        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    public sealed class TrumpetPreparedHealingEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Heal;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit; var target = context?.Target?.Unit;
            if (actor == null || target == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                actor.IsEnemy(target) || target.Destroyed || target.Descriptor.State.IsDead || target.Descriptor.IsUndead ||
                target.Damage < 50 || Book == null || Heal == null) return 0f;
            var book = actor.Descriptor.GetSpellbook(Book);
            if (book == null || !book.GetMemorizedSpellSlots(6).Any(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, Heal))) return 0f;
            var data = new AbilityData(Heal, book);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }
}
