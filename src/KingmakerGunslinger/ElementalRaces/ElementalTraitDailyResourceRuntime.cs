using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Pairs native AddAbilityResources with the existing owned ledger.
    /// TurnOff only records expenditure; native fact lifetime owns removal.</summary>
    [Serializable]
    public sealed class ElementalTraitDailyResourceState : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintAbilityResource Resource;

        public override void OnTurnOn()
        {
            if (!Present()) return;
            UnitPartElementalHeritageState state = Owner.Ensure<UnitPartElementalHeritageState>();
            int remembered;
            int current = Owner.Resources.GetResourceAmount(Resource);
            int desired = ElementalTraitDailyResourcePolicy.ActivationAmount(current,
                state.TryRecall(Resource.AssetGuid, out remembered) ? (int?)remembered : null);
            if (current > desired) Owner.Resources.Spend(Resource, current - desired);
            state.Remember(Resource.AssetGuid, desired);
        }

        public override void OnTurnOff()
        {
            if (Present()) Owner.Ensure<UnitPartElementalHeritageState>().Remember(
                Resource.AssetGuid, Owner.Resources.GetResourceAmount(Resource));
        }

        private bool Present()
        {
            return Owner != null && Resource != null && Owner.Resources != null &&
                Owner.Resources.PersistantResources.Any(value => value != null &&
                    ReferenceEquals(value.Blueprint, Resource));
        }
    }

    /// <summary>Exact project-owned daily-ability cleanup after provider reconciliation.
    /// This is not a shared/native resource or ability sweep.</summary>
    internal static class ElementalTraitDailyResourceRuntime
    {
        internal static void RemoveInactive(UnitDescriptor owner,
            ElementalAlternateTraitBlueprints trait, UnitPartElementalHeritageState state)
        {
            foreach (BlueprintActivatableAbility mode in trait.Mechanics().OfType<BlueprintActivatableAbility>())
            {
                foreach (ActivatableAbility fact in owner.ActivatableAbilities.Enumerable.Where(value =>
                    value != null && ReferenceEquals(value.Blueprint, mode)).ToArray())
                {
                    fact.IsOn = false;
                    owner.RemoveFact(fact);
                }
                // This runs only for a truly inactive provider after local
                // reconciliation, never for transient fact hydration/TurnOff.
                foreach (Buff buff in owner.Buffs.Enumerable.Where(value =>
                    value != null && ReferenceEquals(value.Blueprint, mode.Buff)).ToArray()) owner.RemoveFact(buff);
            }
            foreach (BlueprintAbility ability in trait.Mechanics().OfType<BlueprintAbility>())
            {
                Fact[] facts = owner.Abilities.Enumerable.Where(value => value != null &&
                    ReferenceEquals(value.Blueprint, ability)).Cast<Fact>().ToArray();
                foreach (Fact fact in facts) owner.RemoveFact(fact);
            }
            foreach (BlueprintAbilityResource resource in trait.Mechanics().OfType<BlueprintAbilityResource>())
            {
                if (!owner.Resources.PersistantResources.Any(value => value != null &&
                        ReferenceEquals(value.Blueprint, resource))) continue;
                int remembered;
                if (!state.TryRecall(resource.AssetGuid, out remembered))
                    state.Remember(resource.AssetGuid, owner.Resources.GetResourceAmount(resource));
                owner.Resources.Remove(resource);
            }
        }

        internal static bool IsExact(UnitDescriptor owner,
            ElementalAlternateTraitRaceBlueprints race)
        {
            foreach (ElementalAlternateTraitBlueprints trait in race.Traits())
            {
                bool active = owner.HasFact(trait.Provider);
                foreach (BlueprintActivatableAbility mode in trait.Mechanics().OfType<BlueprintActivatableAbility>())
                    if (owner.ActivatableAbilities.Enumerable.Count(value => value != null &&
                            ReferenceEquals(value.Blueprint, mode)) != (active ? 1 : 0) ||
                        (!active && owner.HasFact(mode.Buff))) return false;
                foreach (BlueprintAbilityResource resource in trait.Mechanics().OfType<BlueprintAbilityResource>())
                    if (owner.Resources.PersistantResources.Count(value => value != null &&
                            ReferenceEquals(value.Blueprint, resource)) != (active ? 1 : 0))
                        return false;
                foreach (BlueprintAbility ability in trait.Mechanics().OfType<BlueprintAbility>())
                {
                    // Only root choices are granted as facts; variants resolve
                    // through their parent, never as extra independent abilities.
                    int expected = active && ability.Parent == null ? 1 : 0;
                    if (owner.Abilities.Enumerable.Count(value => value != null &&
                            ReferenceEquals(value.Blueprint, ability)) != expected) return false;
                }
            }
            return true;
        }
    }
}
