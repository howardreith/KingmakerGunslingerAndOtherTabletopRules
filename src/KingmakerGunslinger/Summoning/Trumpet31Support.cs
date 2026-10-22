using System;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using UnityEngine;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // A printed at-will orison remains a Spell/Cantrip fact. Leveled spells
    // continue to use the native prepared book and slots, not this adapter.
    [Serializable]
    public sealed class TrumpetOrisonParameters : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Orison;
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || Owner?.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) || !ReferenceEquals(evt.Blueprint, Orison)) return;
            var book = Owner.GetSpellbook(Book);
            if (book == null) return;
            evt.ReplaceCasterLevel = book.CasterLevel; evt.ReplaceSpellLevel = 0; evt.ReplaceStat = StatType.Wisdom;
        }
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    // Native Blind-Fight's gaze immunity is not a PF1 feat grant. Its native
    // concealment reroll and blindness AC compensation also lack melee gates.
    // These handlers belong only to the private Trumpet feature; no donor is patched.
    [Serializable]
    public sealed class TrumpetBlindFight : OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleConcealmentCheck>, ITargetRulebookHandler<RuleCalculateAC>
    {
        private static readonly FieldInfo AttackFlag = typeof(RuleConcealmentCheck).GetField("Attack",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        internal static void VerifyContract()
        {
            if (AttackFlag == null || AttackFlag.FieldType != typeof(bool))
                throw new MissingFieldException(typeof(RuleConcealmentCheck).FullName, "Attack");
        }
        public BlueprintUnit OwningBlueprint;
        private bool Owned { get { return Owner?.Unit != null && Fact != null && Fact.Active &&
            ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint); } }
        public void OnEventAboutToTrigger(RuleConcealmentCheck evt)
        {
            if (!Owned || evt == null || !(bool)AttackFlag.GetValue(evt) || !ReferenceEquals(evt.Initiator, Owner.Unit)) return;
            var attack = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
            if (attack?.Weapon == null || !attack.Weapon.Blueprint.IsMelee ||
                !ReferenceEquals(attack.Initiator, Owner.Unit) || !ReferenceEquals(attack.Target, evt.Target)) return;
            evt.RerollIfFailed = true; evt.RerollSource = Fact.Name;
        }
        public void OnEventDidTrigger(RuleConcealmentCheck evt) { }
        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            if (Owned && evt != null && ReferenceEquals(evt.Target, Owner.Unit) &&
                Owner.State.HasCondition(UnitCondition.Blindness) &&
                (evt.AttackType == AttackType.Melee || evt.AttackType == AttackType.Touch)) evt.IgnoreBlindnessPenalty = true;
        }
        public void OnEventDidTrigger(RuleCalculateAC evt) { }
    }

    // The native shared Persuasion stat holds the printed Diplomacy ranks.
    // Only actual named checks receive these adjustments: unlisted Bluff and
    // Intimidate do not inherit those ranks or their class-skill bonus.
    [Serializable]
    public sealed class TrumpetNamedPersuasion : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || Owner?.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) || Fact == null || !Fact.Active) return;
            bool diplomacy = evt.StatType == StatType.CheckDiplomacy;
            if (!diplomacy && evt.StatType != StatType.CheckIntimidate && evt.StatType != StatType.CheckBluff) return;
            var skill = Owner.Stats.SkillPersuasion as ModifiableValueSkill;
            if (skill == null) return;
            int delta = diplomacy ? (skill.BaseValue > 0 && !skill.ClassSkill ? 3 : 0) :
                -skill.BaseValue - (skill.BaseValue > 0 && skill.ClassSkill ? 3 : 0);
            if (delta != 0) evt.Bonus.AddModifier(delta, this, Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    [Serializable]
    public sealed class TrumpetPersuasive : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || Owner?.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) || Fact == null || !Fact.Active) return;
            int ranks;
            if (evt.StatType == StatType.CheckDiplomacy) ranks = Owner.Stats.SkillPersuasion.BaseValue;
            else if (evt.StatType == StatType.CheckIntimidate) ranks = 0;
            else return;
            evt.Bonus.AddModifier(ranks >= 10 ? 4 : 2, this, Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    // Target selection only. The native all-creature Banishment effect and
    // native HD budget/save/SR stay unchanged. A safe enemy point has no
    // non-enemy creature in the actual30ft burst, including the caster.
    public sealed class TrumpetBanishmentEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Ability;
        public BlueprintUnitFact Extraplanar;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit; var target = context?.Target?.Unit;
            if (actor == null || target == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                !actor.IsEnemy(target) || target.Destroyed || target.Descriptor.State.IsDead ||
                Extraplanar == null || !target.Descriptor.HasFact(Extraplanar) || Game.Instance?.State == null) return 0f;
            var book = actor.Descriptor.GetSpellbook(Book);
            if (book == null || target.Descriptor.Progression.CharacterLevel > book.CasterLevel * 2 ||
                !book.GetMemorizedSpellSlots(6).Any(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, Ability))) return 0f;
            float radius = new Feet(30).Meters;
            if (Game.Instance.State.Units.All.Any(unit => unit != null && !unit.Destroyed &&
                !unit.Descriptor.State.IsDead && !actor.IsEnemy(unit) &&
                Vector3.Distance(unit.Position, target.Position) <= radius + (unit.View == null ? 0f : unit.View.Corpulence))) return 0f;
            var data = new AbilityData(Ability, book);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }

}
