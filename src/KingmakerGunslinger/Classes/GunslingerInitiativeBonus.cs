using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

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
                GunslingerInitiativeRuntime.Apply(rule,
                    Owner.Resources.GetResourceAmount(GritResource));
            }
            catch
            {
                GunslingerInitiativeRuntimeDiagnostics.Faults++;
                throw;
            }
        }
    }
}
