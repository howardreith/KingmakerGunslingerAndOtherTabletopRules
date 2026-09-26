using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.Classes
{
    public sealed class GunslingerInitiativeBonus :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitInitiativeHandler,
        IGlobalSubscriber, IInitiatorRulebookHandler<RuleInitiativeRoll>
    {
        public BlueprintAbilityResource GritResource;

        // The native combat-entry controller stores RuleInitiativeRoll.Result
        // into the unit's combat state and orders the combatants before it
        // raises IUnitInitiativeHandler, so the deed applies while the rule
        // itself resolves (after OnTrigger snapshots the Initiative stat).
        // The handler stays as a duplicate-guarded fallback.
        public void OnEventAboutToTrigger(RuleInitiativeRoll evt) { }

        public void OnEventDidTrigger(RuleInitiativeRoll evt)
        {
            HandleUnitRollsInitiative(evt);
        }

        public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
        {
            try
            {
                if (rule == null || Owner == null || Owner.Unit == null ||
                    !ReferenceEquals(rule.Initiator, Owner.Unit)) return;
                if (GritResource == null || Owner.Resources == null)
                    throw new InvalidOperationException(
                        "Gunslinger Initiative grit resource is unavailable.");
                int grit = Owner.Resources.GetResourceAmount(GritResource);
                if (TrueGritRuntime.Evaluate(Owner,
                    TrueGritDeed.GunslingerInitiative, 0, true).Available)
                    grit = Math.Max(1, grit);
                GunslingerInitiativeRuntime.Apply(rule, grit,
                    FavoredClass.Mechanics.FavoredClassEarnedSteps.InitiativeBonus(Owner));
            }
            catch
            {
                GunslingerInitiativeRuntimeDiagnostics.Faults++;
                throw;
            }
        }
    }
}
