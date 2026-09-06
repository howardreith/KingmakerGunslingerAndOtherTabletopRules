using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    public sealed class UnitPartElementalHeritageState : UnitPart
    {
        [JsonProperty]
        private Dictionary<string, int> _resourceAmounts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        internal void Remember(string resourceGuid, int amount)
        {
            if (string.IsNullOrWhiteSpace(resourceGuid)) return;
            EnsureStorage()[resourceGuid] = Math.Max(0, amount);
        }

        internal bool TryRecall(string resourceGuid, out int amount)
        {
            amount = 0;
            return !string.IsNullOrWhiteSpace(resourceGuid) &&
                EnsureStorage().TryGetValue(resourceGuid, out amount);
        }

        public override void PostLoad()
        {
            base.PostLoad();
            EnsureStorage();
        }

        private Dictionary<string, int> EnsureStorage()
        {
            if (_resourceAmounts == null)
                _resourceAmounts = new Dictionary<string, int>(
                    StringComparer.Ordinal);
            return _resourceAmounts;
        }
    }

    public sealed class ElementalHeritageRaceController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Race;

        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }

        public override void OnTurnOff()
        {
            ElementalHeritageRuntime.RemoveOwnedProviders(Owner,
                (ElementalHeritageRace)Race);
        }
    }

    public sealed class ElementalHeritageMarkerController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Heritage;

        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner,
                (ElementalHeritageId)Heritage, null);
        }

        public override void OnTurnOff()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null,
                (ElementalHeritageId)Heritage);
        }
    }

    public sealed class ElementalHeritageSelectionController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }

        public override void OnTurnOff() { }
    }

    public sealed class ElementalAlternateTraitMarkerController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Trait;

        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null,
                (ElementalAlternateTraitId)Trait, null);
        }

        public override void OnTurnOff()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null, null,
                (ElementalAlternateTraitId)Trait);
        }
    }

    public sealed class ElementalAlternateTraitRetainController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Race;
        public int Slot;

        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }

        public override void OnTurnOff()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }
    }

    public sealed class ElementalAlternateTraitProviderController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Trait;

        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }

        public override void OnTurnOff() { }
    }

    public sealed class ElementalOwnedProviderController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            ElementalHeritageRuntime.Reconcile(Owner, null, null);
        }

        public override void OnTurnOff() { }
    }

    internal static class ElementalHeritageRuntime
    {
        private static ElementalRaceBlueprintSet _blueprints;
        private static readonly HashSet<UnitDescriptor> Reconciling =
            new HashSet<UnitDescriptor>();

        internal static void Configure(ElementalRaceBlueprintSet blueprints)
        {
            if (blueprints == null) throw new ArgumentNullException(
                "blueprints");
            ElementalRaceBlueprints[] races = blueprints.OrderedBlueprints()
                .ToArray();
            if (races.Length != ElementalRaceCatalog.RaceCount ||
                races.Any(value => value.Heritages == null) ||
                races.Any(value => value.AlternateTraits == null) ||
                races.Sum(value => value.Heritages.RegisteredCount) !=
                    ElementalRaceIdentityCatalog.HeritageIdentityCount ||
                races.Sum(value => value.AlternateTraits.RegisteredCount) !=
                    ElementalRaceIdentityCatalog.TraitFrameworkIdentityCount +
                    ElementalRaceIdentityCatalog.TraitMechanicIdentityCount)
                throw new InvalidOperationException(
                    "The complete heritage and alternate-trait blueprint graph is required.");
            _blueprints = blueprints;
        }

        internal static bool Reconcile(UnitDescriptor owner,
            ElementalHeritageId? activating,
            ElementalHeritageId? deactivating)
        {
            return Reconcile(owner, activating, deactivating, null, null);
        }

        internal static bool Reconcile(UnitDescriptor owner,
            ElementalHeritageId? heritageActivating,
            ElementalHeritageId? heritageDeactivating,
            ElementalAlternateTraitId? traitActivating,
            ElementalAlternateTraitId? traitDeactivating)
        {
            if (owner == null || _blueprints == null ||
                !Reconciling.Add(owner)) return false;
            try
            {
                ElementalRaceBlueprints race;
                if (!TryRace(owner, out race)) return false;
                ElementalHeritageBlueprints desiredHeritage = Resolve(
                    race.Heritages, owner, heritageActivating,
                    heritageDeactivating);
                if (desiredHeritage == null) return false;
                ElementalHeritageRace parentRace = ToHeritageRace(
                    race.Definition.Kind);
                ElementalAlternateTraitId[] observedTraits = race
                    .AlternateTraits.Traits().Where(value => owner.HasFact(
                        value.Marker)).Select(value => value.Definition.Id)
                    .ToArray();
                ElementalAlternateTraitId[] effectiveTraits =
                    ElementalAlternateTraitPolicy.TransitionMarkers(
                        parentRace, observedTraits, traitActivating,
                        traitDeactivating);
                ElementalAlternateTraitState desired =
                    ElementalAlternateTraitPolicy.Resolve(parentRace,
                        desiredHeritage.Definition.Id, effectiveTraits);
                ElementalAlternateTraitBlueprints[] desiredTraits =
                    effectiveTraits.Select(race.AlternateTraits.Require)
                        .ToArray();
                UnitPartElementalHeritageState state = owner.Ensure<
                    UnitPartElementalHeritageState>();
                RememberCurrent(owner, race.Heritages, state);

                var added = new List<Fact>();
                Fact addedSla = null;
                try
                {
                    if (desired.EnergyResistanceProviderSymbol != null)
                        AddDesired(owner, race.Resistance, added);
                    if (desired.ElementalAffinityProviderSymbol != null)
                        AddDesired(owner, desiredHeritage.Affinity, added);
                    if (desired.RacialSlaFeatureSymbol != null)
                        addedSla = AddDesired(owner,
                            desiredHeritage.SlaFeature, added);
                    foreach (ElementalAlternateTraitBlueprints trait in
                        desiredTraits)
                        AddDesired(owner, trait.Provider, added);

                    if (!DesiredFactsArePresent(owner, race,
                            desiredHeritage, desiredTraits, desired))
                        throw new InvalidOperationException(
                            "A desired elemental racial provider could not be added.");

                    int recalled;
                    if (addedSla != null && state.TryRecall(
                            desiredHeritage.SlaResource.AssetGuid,
                            out recalled))
                        SetAmount(owner, desiredHeritage, recalled);
                }
                catch
                {
                    foreach (Fact fact in added.AsEnumerable().Reverse())
                        TryRemove(owner, fact);
                    throw;
                }

                if (desired.EnergyResistanceProviderSymbol == null)
                    TryRemove(owner, race.Resistance);
                foreach (ElementalHeritageBlueprints choice in
                    race.Heritages.Choices())
                {
                    if (desired.ElementalAffinityProviderSymbol == null ||
                        !ReferenceEquals(choice.Affinity,
                            desiredHeritage.Affinity))
                        TryRemove(owner, choice.Affinity);
                    if (desired.RacialSlaFeatureSymbol == null ||
                        !ReferenceEquals(choice.SlaFeature,
                            desiredHeritage.SlaFeature))
                    {
                        TryRemove(owner, choice.SlaFeature);
                        RemoveOwnedAbility(owner, choice.SlaAbility);
                        RemoveOwnedResource(owner, choice, state);
                        if (owner.Abilities.GetAbility(
                                choice.SlaAbility) != null)
                            throw new InvalidOperationException(
                                "An inactive heritage SLA ability remained after provider reconciliation.");
                    }
                }
                BlueprintFeature[] desiredTraitProviders = desiredTraits
                    .Select(value => value.Provider).ToArray();
                foreach (ElementalAlternateTraitBlueprints trait in
                    race.AlternateTraits.Traits())
                    if (!desiredTraitProviders.Contains(trait.Provider))
                        TryRemove(owner, trait.Provider);

                if (desired.RacialSlaFeatureSymbol != null)
                    state.Remember(desiredHeritage.SlaResource.AssetGuid,
                        owner.Resources.GetResourceAmount(
                            desiredHeritage.SlaResource));
                return DesiredFactsArePresent(owner, race, desiredHeritage,
                    desiredTraits, desired) &&
                    ProviderFactsAreExact(owner, race, desiredHeritage,
                        desiredTraits, desired) &&
                    ProviderResourcesAreExact(owner, race,
                        desiredHeritage, desired) &&
                    InactiveAbilitiesAreAbsent(owner, race,
                        desiredHeritage, desired);
            }
            catch (Exception exception)
            {
                Fault("reconcile", owner, exception);
                return false;
            }
            finally
            {
                Reconciling.Remove(owner);
            }
        }

        internal static void RemoveOwnedProviders(UnitDescriptor owner,
            ElementalHeritageRace race)
        {
            if (owner == null || _blueprints == null ||
                !Reconciling.Add(owner)) return;
            try
            {
                ElementalRaceBlueprints blueprint = _blueprints
                    .OrderedBlueprints().SingleOrDefault(value =>
                        ToHeritageRace(value.Definition.Kind) == race);
                if (blueprint == null) return;
                UnitPartElementalHeritageState state = owner.Ensure<
                    UnitPartElementalHeritageState>();
                RememberCurrent(owner, blueprint.Heritages, state);
                TryRemove(owner, blueprint.Resistance);
                foreach (ElementalHeritageBlueprints choice in
                    blueprint.Heritages.Choices())
                {
                    TryRemove(owner, choice.Affinity);
                    TryRemove(owner, choice.SlaFeature);
                    RemoveOwnedAbility(owner, choice.SlaAbility);
                    RemoveOwnedResource(owner, choice, state);
                }
                foreach (BlueprintFeature provider in blueprint
                    .AlternateTraits.OwnedProviders())
                    TryRemove(owner, provider);
            }
            catch (Exception exception)
            {
                Fault("remove-owned-providers", owner, exception);
            }
            finally
            {
                Reconciling.Remove(owner);
            }
        }

        private static ElementalHeritageBlueprints Resolve(
            ElementalHeritageRaceBlueprints race, UnitDescriptor owner,
            ElementalHeritageId? activating,
            ElementalHeritageId? deactivating)
        {
            if (activating.HasValue)
            {
                ElementalHeritageBlueprints forced = race.Choices()
                    .SingleOrDefault(value => value.Definition.Id ==
                        activating.Value);
                return forced;
            }
            ElementalHeritageBlueprints[] active = race.Choices().Where(
                value => (!deactivating.HasValue || value.Definition.Id !=
                    deactivating.Value) && owner.HasFact(value.Marker))
                .ToArray();
            if (active.Length == 0) return race.General;
            if (active.Length == 1) return active[0];
            throw new InvalidOperationException(
                "Multiple elemental heritage markers are active.");
        }

        private static bool TryRace(UnitDescriptor owner,
            out ElementalRaceBlueprints result)
        {
            result = null;
            BlueprintRace race = owner.Progression == null ? null :
                owner.Progression.Race;
            if (race == null) return false;
            result = _blueprints.OrderedBlueprints().SingleOrDefault(value =>
                ReferenceEquals(value.Race, race));
            return result != null;
        }

        private static Fact AddDesired(UnitDescriptor owner,
            BlueprintFeature feature, ICollection<Fact> added)
        {
            if (owner.HasFact(feature)) return null;
            Fact fact = owner.AddFact(feature);
            if (fact != null) added.Add(fact);
            return fact;
        }

        private static bool DesiredFactsArePresent(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            IEnumerable<ElementalAlternateTraitBlueprints> traits,
            ElementalAlternateTraitState desired)
        {
            bool resistance = desired.EnergyResistanceProviderSymbol == null ||
                owner.HasFact(race.Resistance);
            bool affinity = desired.ElementalAffinityProviderSymbol == null ||
                owner.HasFact(heritage.Affinity);
            bool sla = desired.RacialSlaFeatureSymbol == null ||
                owner.HasFact(heritage.SlaFeature);
            return resistance && affinity && sla && traits.All(value =>
                owner.HasFact(value.Provider));
        }

        private static bool InactiveAbilitiesAreAbsent(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            ElementalAlternateTraitState desired)
        {
            if (owner.Abilities == null) return false;
            foreach (ElementalHeritageBlueprints choice in
                race.Heritages.Choices())
            {
                bool present = owner.Abilities.GetAbility(
                    choice.SlaAbility) != null;
                bool expected = desired.RacialSlaFeatureSymbol != null &&
                    ReferenceEquals(choice, heritage);
                if (present != expected) return false;
            }
            return true;
        }

        private static bool ProviderFactsAreExact(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            IEnumerable<ElementalAlternateTraitBlueprints> traits,
            ElementalAlternateTraitState desired)
        {
            bool keepResistance =
                desired.EnergyResistanceProviderSymbol != null;
            if (owner.HasFact(race.Resistance) != keepResistance)
                return false;
            foreach (ElementalHeritageBlueprints choice in
                race.Heritages.Choices())
            {
                bool keepAffinity =
                    desired.ElementalAffinityProviderSymbol != null &&
                    ReferenceEquals(choice, heritage);
                bool keepSla = desired.RacialSlaFeatureSymbol != null &&
                    ReferenceEquals(choice, heritage);
                if (owner.HasFact(choice.Affinity) != keepAffinity ||
                    owner.HasFact(choice.SlaFeature) != keepSla)
                    return false;
            }
            BlueprintFeature[] desiredProviders = traits.Select(value =>
                value.Provider).ToArray();
            return race.AlternateTraits.Traits().All(value =>
                owner.HasFact(value.Provider) ==
                desiredProviders.Contains(value.Provider));
        }

        private static bool ProviderResourcesAreExact(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            ElementalAlternateTraitState desired)
        {
            if (owner.Resources == null) return false;
            foreach (ElementalHeritageBlueprints choice in
                race.Heritages.Choices())
            {
                int expected = desired.RacialSlaFeatureSymbol != null &&
                    ReferenceEquals(choice, heritage) ? 1 : 0;
                if (owner.Resources.PersistantResources.Count(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        choice.SlaResource)) != expected) return false;
            }
            return true;
        }

        private static void RemoveOwnedResource(UnitDescriptor owner,
            ElementalHeritageBlueprints choice,
            UnitPartElementalHeritageState state)
        {
            if (owner.Resources == null || !owner.Resources
                .PersistantResources.Any(value => value != null &&
                    ReferenceEquals(value.Blueprint, choice.SlaResource)))
                return;
            // Native late activation can leave a resource after its owning
            // fact has already been removed. Keep any previous spent amount;
            // an orphan's freshly restored amount must not overwrite it.
            int remembered;
            if (!state.TryRecall(choice.SlaResource.AssetGuid,
                    out remembered))
                state.Remember(choice.SlaResource.AssetGuid,
                    owner.Resources.GetResourceAmount(choice.SlaResource));
            owner.Resources.Remove(choice.SlaResource);
        }

        private static void RememberCurrent(UnitDescriptor owner,
            ElementalHeritageRaceBlueprints race,
            UnitPartElementalHeritageState state)
        {
            foreach (ElementalHeritageBlueprints choice in race.Choices())
                if (owner.HasFact(choice.SlaFeature))
                    state.Remember(choice.SlaResource.AssetGuid,
                        owner.Resources.GetResourceAmount(choice.SlaResource));
        }

        private static void SetAmount(UnitDescriptor owner,
            ElementalHeritageBlueprints choice, int desired)
        {
            int current = owner.Resources.GetResourceAmount(
                choice.SlaResource);
            if (current > desired)
                owner.Resources.Spend(choice.SlaResource, current - desired);
            else if (current < desired)
                owner.Resources.Restore(choice.SlaResource,
                    desired - current);
        }

        private static void TryRemove(UnitDescriptor owner,
            BlueprintFeature feature)
        {
            if (owner == null || feature == null) return;
            Fact fact = owner.GetFact(feature);
            if (fact != null) TryRemove(owner, fact);
        }

        private static void RemoveOwnedAbility(UnitDescriptor owner,
            BlueprintAbility ability)
        {
            if (owner == null || ability == null || owner.Abilities == null)
                return;
            Fact[] facts = owner.Abilities.Enumerable.Where(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        ability)).Cast<Fact>().ToArray();
            foreach (Fact fact in facts) TryRemove(owner, fact);
        }

        private static void TryRemove(UnitDescriptor owner, Fact fact)
        {
            if (owner == null || fact == null) return;
            try { owner.RemoveFact(fact); }
            catch { }
        }

        private static ElementalHeritageRace ToHeritageRace(
            ElementalRaceKind race)
        {
            return (ElementalHeritageRace)(int)race;
        }

        private static void Fault(string operation, UnitDescriptor owner,
            Exception exception)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Failure("elemental-races",
                "heritage." + operation + ".failed",
                "unit=" + (owner == null || owner.Unit == null ? "<none>" :
                    owner.Unit.UniqueId), exception);
        }
    }
}
