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
        IGlobalSubscriber
    {
        public BlueprintAbilityResource GritResource;

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
                GunslingerInitiativeRuntime.Apply(rule, grit);
            }
            catch
            {
                GunslingerInitiativeRuntimeDiagnostics.Faults++;
                throw;
            }
        }
    }
}
