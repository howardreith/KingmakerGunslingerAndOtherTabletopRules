using System;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    // Same creation-only native-book seam as the accepted Trumpet. Native
    // spontaneous slots/known spells serialize; activation/load never Rest.
    [Serializable]
    public sealed class LillendBardBook32 : OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                Book == null || !Book.Spontaneous || Book.SpellList == null ||
                unit.Descriptor.GetSpellbook(Book) != null)
                throw new InvalidOperationException("Lillend creation requires its unused private native bard book.");
            var book = unit.Descriptor.DemandSpellbook(Book);
            for (int count = 0; book.CasterLevel < 7 && count < 7; count++) book.AddCasterLevel();
            if (book.CasterLevel != 7) throw new InvalidOperationException("Lillend native bard book is not CL7.");
            book.UpdateAllSlotsSize(false);
            foreach (var row in Book.SpellList.SpellsByLevel)
            {
                if (row.SpellLevel < 0 || row.SpellLevel > 3)
                {
                    if (row.Spells.Count != 0) throw new InvalidOperationException("Lillend has an illegal spell level.");
                    continue;
                }
                foreach (var spell in row.Spells)
                {
                    if (spell == null || spell.Type != AbilityType.Spell)
                        throw new InvalidOperationException("Lillend known spells must remain real native spells.");
                    book.AddKnown(row.SpellLevel, spell, true);
                }
            }
            book.Rest();
        }
        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    public sealed class LillendHealingEligibility32 : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintSpellbook Book;
        public BlueprintAbility Spell;
        public int Level;
        public int MinimumDamage;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit; var target = context?.Target?.Unit;
            if (actor == null || target == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                actor.IsEnemy(target) || target.Destroyed || !target.IsInState ||
                target.Descriptor.State.IsDead || target.Descriptor.IsUndead ||
                target.Damage < MinimumDamage || Book == null || Spell == null) return 0f;
            var book = actor.Descriptor.GetSpellbook(Book);
            if (book == null || book.GetSpontaneousSlots(Level) <= 0 ||
                book.GetSpellLevel(Spell) != Level) return 0f;
            var data = new AbilityData(Spell, book);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }

    // Only chooses a normal native activation command. Native performance
    // owns buff/area lifetime, rounds, cancellation and resource expenditure.
    public sealed class LillendCourageAi32 : BlueprintAiAction
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintActivatableAbility Courage;
        private ActivatableAbility Find(DecisionContext context)
        {
            var actor = context?.Unit;
            return actor == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ? null :
                actor.Descriptor.ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, Courage));
        }
        public override bool IsAvailable(DecisionContext context)
        {
            var ability = Find(context);
            return context?.Unit != null && base.IsAvailable(context) && context.Unit.IsInCombat && ability != null &&
                !ability.IsOn && !ability.IsRunning && ability.IsAvailable;
        }
        public override bool IsTargetValid(DecisionContext context)
        { return ReferenceEquals(context?.Target?.Unit, context?.Unit) && IsAvailable(context); }
        public override TargetType GetTargetType(DecisionContext context) { return TargetType.Self; }
        public override float GetRange(DecisionContext context) { return 0f; }
        public override float GetRadius(DecisionContext context) { return 0f; }
        public override UnitCommand CreateCommand(DecisionContext context, UnitEntityData target)
        {
            if (!ReferenceEquals(target, context?.Unit) || !IsAvailable(context)) return null;
            return new UnitActivateAbility(Find(context));
        }
    }

    // WithUnitCommand's native DidAct handler starts only an enabled toggle.
    // Arm it at the matching command's native start, while that unacted command
    // is already queued. The exact same-instance compatibility patch below
    // lets native OnTurnOn recognize it instead of replacing the AI command.
    // No buff application, TryStart, action debit or direct effect here.
    [Serializable]
    public sealed class LillendPerformanceCommand32 : OwnedGameLogicComponent<UnitDescriptor>,
        IUnitCommandStartHandler, IUnitSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintActivatableAbility Courage;
        public void HandleUnitCommandDidStart(UnitCommand command)
        {
            var activate = command as UnitActivateAbility;
            if (activate == null || activate.IsActed || Owner?.Unit == null ||
                !ReferenceEquals(activate.Executor, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(activate.Ability?.Blueprint, Courage) ||
                activate.Ability.IsOn || activate.Ability.IsRunning ||
                !activate.Ability.IsAvailable || Fact == null || !Fact.Active) return;
            if (!Owner.Unit.Commands.Raw.Any(value => ReferenceEquals(value, activate)))
                throw new InvalidOperationException("Lillend performance start has no exact queued native command.");
            activate.Ability.IsOn = true;
        }
    }

    // Kingmaker's native same-command predicate recognizes Judgment only,
    // even when asked about the identical bard toggle. Preserve that native
    // behavior everywhere except this private owner's identical performance.
    // No command credit, effect, resource or action cost is supplied here.
    [HarmonyPatch(typeof(ActivatableAbility), "IsActivateWithSameCommand",
        new[] { typeof(ActivatableAbility) })]
    internal static class LillendPerformanceSameCommand32
    {
        private static void Postfix(ActivatableAbility __instance,
            ActivatableAbility __0, ref bool __result)
        {
            if (__result || __instance == null || !ReferenceEquals(__instance, __0)) return;
            var unit = __instance.Owner?.Unit;
            // This component belongs to the private Traits fact, not directly
            // to BlueprintUnit. Match that declared fact's exact owner/toggle.
            var rule = unit?.Blueprint?.AddFacts?
                .Where(value => value != null)
                .SelectMany(value => value.GetComponents<LillendPerformanceCommand32>())
                .SingleOrDefault();
            if (rule != null && ReferenceEquals(rule.OwningBlueprint, unit.Blueprint) &&
                ReferenceEquals(rule.Courage, __instance.Blueprint)) __result = true;
        }
    }
}
