using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    public enum LillendSpellChoice32 { Daze, CharmPerson, Sleep, HoldPerson, SoundBurst,
        Invisibility, CharmMonster, CureLight, CharmPersonSla, Suggestion, Lullaby }

    // Creature-specific choices over the real book/ability, native targeting,
    // and native commands. No spell, slot, save, or effect is supplied by AI.
    public sealed class LillendSpellEligibility32 : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Spell;
        public BlueprintAbility SleepSpell;
        public BlueprintBuff RefusedBuff;
        public string ControlBuffGuid;
        public LillendSpellChoice32 Choice;
        public int Level;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit; var target = context?.Target?.Unit;
            if (actor == null || target == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                Spell == null || target.Destroyed || !target.IsInState || target.Descriptor.State.IsDead ||
                !actor.HasLOS(target)) return 0f;
            AbilityData data;
            var book = actor.Descriptor.GetSpellbook(Book);
            if (Choice == LillendSpellChoice32.CharmPersonSla || Level == 0)
            {
                // Native cantrips use their granted Spell/Cantrip fact, as the
                // AI action does. Book-backed AbilityData checks a finite L0
                // pool and cannot establish at-will casting.
                if (Level == 0 && (book == null || book.GetSpellLevel(Spell) != 0)) return 0f;
                var granted = actor.Descriptor.Abilities.GetAbility(Spell);
                if (granted == null) return 0f;
                data = new AbilityData(granted);
            }
            else
            {
                if (book == null || book.GetSpellLevel(Spell) != Level ||
                    Level > 0 && book.GetSpontaneousSlots(Level) <= 0) return 0f;
                data = new AbilityData(Spell, book);
            }
            if (!data.IsAvailable || !data.CanTarget(new TargetWrapper(target))) return 0f;
            if (RefusedBuff != null && target.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, RefusedBuff))) return 0f;
            if (Choice == LillendSpellChoice32.Lullaby)
            {
                // A pre-cast tactical choice, not an HD restriction on Lullaby:
                // use it to help an available Sleep, never loop on a target that
                // Sleep cannot affect or spend turns on an exhausted book.
                if (SleepSpell == null || book == null || book.GetSpontaneousSlots(1) <= 0 ||
                    !ControlSleepTarget(actor, target, book)) return 0f;
            }
            if (Choice == LillendSpellChoice32.CureLight)
            {
                // Reserve the stronger spell for substantial damage while it
                // remains available; this is a choice, not a slot restriction.
                return !actor.IsEnemy(target) && !target.Descriptor.IsUndead && target.Damage >= 6 &&
                    (target.Damage < 18 || book.GetSpontaneousSlots(3) == 0) ? 1f : 0f;
            }
            if (Choice == LillendSpellChoice32.Invisibility)
            {
                // A wounded performing bard can withdraw/support unseen. Do
                // not keep recasting on healthy allies or existing invisibility.
                return ReferenceEquals(actor, target) && actor.IsInCombat &&
                    actor.Damage >= actor.Descriptor.Stats.HitPoints.ModifiedValue / 2 &&
                    !actor.Descriptor.State.HasCondition(UnitCondition.Invisible) ? 1f : 0f;
            }
            int hd = Choice == LillendSpellChoice32.Daze || Choice == LillendSpellChoice32.Sleep ? 4 : 0;
            UnitCondition? condition = Choice == LillendSpellChoice32.Daze || Choice == LillendSpellChoice32.Suggestion ? UnitCondition.Dazed :
                Choice == LillendSpellChoice32.Sleep ? UnitCondition.Sleeping :
                Choice == LillendSpellChoice32.HoldPerson ? UnitCondition.Paralyzed :
                Choice == LillendSpellChoice32.SoundBurst ? UnitCondition.Stunned : (UnitCondition?)null;
            bool charm = Choice == LillendSpellChoice32.CharmPerson || Choice == LillendSpellChoice32.CharmMonster ||
                Choice == LillendSpellChoice32.CharmPersonSla;
            if (charm && string.IsNullOrEmpty(ControlBuffGuid)) return 0f;
            if (!SatyrControlEligibility.ControlTargetIsEligible(actor, target, data, hd, condition,
                charm ? ControlBuffGuid :
                Choice == LillendSpellChoice32.Suggestion ? Sprint32Suggestion.OwnedBuffGuid : null)) return 0f;
            if (Choice == LillendSpellChoice32.SoundBurst || Choice == LillendSpellChoice32.Lullaby)
            {
                if (Game.Instance?.State?.Units == null) return 0f;
                // Preserve the spell's actual ten-foot footprint. Refuse a
                // footprint touching any living ally, including the caster.
                if (Game.Instance.State.Units.All.Any(ally => ally != null && !ally.Destroyed && ally.IsInState &&
                    !ally.Descriptor.State.IsDead && !actor.IsEnemy(ally) &&
                    ally.DistanceTo(target) <= new Feet(10).Meters + Math.Max(0f, ally.Corpulence))) return 0f;
            }
            return 1f;
        }

        private bool ControlSleepTarget(Kingmaker.EntitySystem.Entities.UnitEntityData actor,
            Kingmaker.EntitySystem.Entities.UnitEntityData target, Spellbook book)
        {
            return book.GetSpellLevel(SleepSpell) == 1 &&
                SatyrControlEligibility.ControlTargetIsEligible(actor, target,
                    new AbilityData(SleepSpell, book), 4, UnitCondition.Sleeping, null);
        }
    }
}
