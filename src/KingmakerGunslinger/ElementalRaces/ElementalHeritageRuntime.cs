using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
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
                races.Sum(value => value.Heritages.RegisteredCount) !=
                    ElementalRaceIdentityCatalog.HeritageIdentityCount)
                throw new InvalidOperationException(
                    "The complete heritage blueprint graph is required.");
            _blueprints = blueprints;
        }

        internal static bool Reconcile(UnitDescriptor owner,
            ElementalHeritageId? activating,
            ElementalHeritageId? deactivating)
        {
            if (owner == null || _blueprints == null ||
                !Reconciling.Add(owner)) return false;
            try
            {
                ElementalRaceBlueprints race;
                if (!TryRace(owner, out race)) return false;
                ElementalHeritageBlueprints desired = Resolve(race.Heritages,
                    owner, activating, deactivating);
                if (desired == null) return false;
                UnitPartElementalHeritageState state = owner.Ensure<
                    UnitPartElementalHeritageState>();
                RememberCurrent(owner, race.Heritages, state);

                Fact addedAffinity = null;
                Fact addedSla = null;
                try
                {
                    if (!owner.HasFact(desired.Affinity))
                        addedAffinity = owner.AddFact(desired.Affinity);
                    if (!owner.HasFact(desired.SlaFeature))
                        addedSla = owner.AddFact(desired.SlaFeature);
                    if (!owner.HasFact(desired.Affinity) ||
                        !owner.HasFact(desired.SlaFeature))
                        throw new InvalidOperationException(
                            "A desired heritage provider could not be added.");

                    int recalled;
                    if (addedSla != null && state.TryRecall(
                            desired.SlaResource.AssetGuid, out recalled))
                        SetAmount(owner, desired, recalled);
                }
                catch
                {
                    if (addedSla != null) TryRemove(owner, addedSla);
                    if (addedAffinity != null) TryRemove(owner, addedAffinity);
                    throw;
                }

                foreach (ElementalHeritageBlueprints choice in
                    race.Heritages.Choices())
                {
                    if (!ReferenceEquals(choice.Affinity, desired.Affinity))
                        TryRemove(owner, choice.Affinity);
                    if (!ReferenceEquals(choice.SlaFeature,
                            desired.SlaFeature))
                        TryRemove(owner, choice.SlaFeature);
                }
                state.Remember(desired.SlaResource.AssetGuid,
                    owner.Resources.GetResourceAmount(desired.SlaResource));
                return owner.HasFact(desired.Affinity) &&
                    owner.HasFact(desired.SlaFeature);
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
                foreach (ElementalHeritageBlueprints choice in
                    blueprint.Heritages.Choices())
                {
                    TryRemove(owner, choice.Affinity);
                    TryRemove(owner, choice.SlaFeature);
                }
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
