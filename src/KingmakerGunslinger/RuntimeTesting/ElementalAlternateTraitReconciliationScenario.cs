using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-local native-unit integration for the Release C replacement
    /// service. No save is opened or written, and no donor is registered or
    /// changed. Marker activation, native selector prerequisites, resource
    /// collections and exact feature ranks are the observations under test.
    /// </summary>
    internal static class ElementalAlternateTraitReconciliationScenario
    {
        internal const string EvidenceFileName =
            "elemental-alternate-trait-reconciliation.json";

        private sealed class Evidence
        {
            public int SchemaVersion = 1;
            public bool SaveStateTouched = false;
            public int LegalCombinations;
            public int HeritageCombinationRows;
            public int ActivationOrderRows;
            public int MarkerFirstRows;
            public int CreatedUnits;
            public bool CleanupExact;
            public List<string> CheckedStates = new List<string>();
        }

        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> evidenceFiles)
        {
            var evidence = new Evidence();
            var units = new List<UnitEntityData>();
            var blueprints = new List<BlueprintUnit>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            ElementalRaceBlueprintSet set = BlueprintBootstrap.ElementalRaces;
            BlueprintFeature keen = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    ElementalRaceIdentityCatalog.KeenSensesGuid,
                    "unchanged native fact in trait replacement matrix");
            try
            {
                foreach (ElementalRaceBlueprints race in
                    set.OrderedBlueprints())
                {
                    ElementalAlternateTraitBlueprints[][] legal =
                        LegalCombinations(race.AlternateTraits.Traits()
                            .ToArray()).ToArray();
                    evidence.LegalCombinations += legal.Length;
                    foreach (ElementalHeritageBlueprints heritage in
                        race.Heritages.Choices())
                    {
                        UnitDescriptor owner = Create(race, units,
                            blueprints, heritage.Definition.Id.ToString())
                            .Descriptor;
                        ApplyRace(owner, race);
                        Add(owner, heritage.Marker);
                        Fact nativeFact = owner.GetFact(keen);
                        Verify(owner, race, heritage,
                            new ElementalAlternateTraitBlueprints[0], 1,
                            nativeFact, keen, "initial", evidence, assertions);
                        owner.Resources.Spend(heritage.SlaResource, 1);

                        foreach (ElementalAlternateTraitBlueprints[] chosen in
                            legal)
                        {
                            evidence.HeritageCombinationRows++;
                            foreach (bool reverse in new[] { false, true })
                            {
                                ElementalAlternateTraitBlueprints[] order =
                                    reverse ? chosen.Reverse().ToArray() :
                                        chosen;
                                foreach (ElementalAlternateTraitBlueprints
                                    trait in order)
                                {
                                    Check(assertions, "select-" +
                                        trait.Definition.Id,
                                        CanSelect(owner, race, trait),
                                        "native primary-slot selector accepts the legal choice");
                                    Add(owner, trait.Marker);
                                }
                                string label = "matrix-" +
                                    string.Join(",", order.Select(value =>
                                        value.Definition.Id.ToString())
                                        .ToArray()) + "-reverse=" + reverse;
                                Verify(owner, race, heritage, chosen, 0,
                                    nativeFact, keen, label, evidence,
                                    assertions);
                                CheckExclusions(owner, race, chosen,
                                    assertions);
                                Check(assertions, "reconcile-idempotent",
                                    ElementalHeritageRuntime.Reconcile(owner,
                                        null, null) &&
                                    ElementalHeritageRuntime.Reconcile(owner,
                                        null, null),
                                    "repeated production reconciliation succeeds");
                                Verify(owner, race, heritage, chosen, 0,
                                    nativeFact, keen, label + "-repeat",
                                    evidence, assertions);

                                foreach (ElementalAlternateTraitBlueprints
                                    trait in order.Reverse())
                                    owner.RemoveFact(trait.Marker);
                                Verify(owner, race, heritage,
                                    new ElementalAlternateTraitBlueprints[0],
                                    0, nativeFact, keen, label + "-remove",
                                    evidence, assertions);
                                evidence.ActivationOrderRows++;
                            }
                        }

                        // Restore through the ordinary native rest boundary,
                        // then spend again before changing heritage under a
                        // trait which consumes the SLA slot.
                        Kingmaker.Controllers.Rest.RestController.ApplyRest(
                            owner);
                        Verify(owner, race, heritage,
                            new ElementalAlternateTraitBlueprints[0], 1,
                            nativeFact, keen, "rest", evidence, assertions);
                        owner.Resources.Spend(heritage.SlaResource, 1);
                        ExerciseHeritageChange(owner, race, heritage,
                            nativeFact, keen, evidence, assertions);
                    }

                    // Every marker also hydrates before inherited race facts.
                    // This tests the late-owned-provider callbacks, not a
                    // direct call to the reconciliation service.
                    foreach (ElementalAlternateTraitBlueprints trait in
                        race.AlternateTraits.Traits())
                    {
                        ElementalHeritageBlueprints heritage = race.Heritages
                            .Choices().Last();
                        UnitDescriptor owner = Create(race, units,
                            blueprints, "MarkerFirst-" + trait.Definition.Id)
                            .Descriptor;
                        Add(owner, trait.Marker);
                        Add(owner, heritage.Marker);
                        ApplyRace(owner, race);
                        Fact nativeFact = owner.GetFact(keen);
                        Verify(owner, race, heritage, new[] { trait }, 1,
                            nativeFact, keen, "marker-first", evidence,
                            assertions);
                        owner.RemoveFact(trait.Marker);
                        Verify(owner, race, heritage,
                            new ElementalAlternateTraitBlueprints[0], 1,
                            nativeFact, keen, "marker-first-remove",
                            evidence, assertions);
                        evidence.MarkerFirstRows++;
                    }
                }
                Check(assertions, "trait-matrix-completeness",
                    evidence.LegalCombinations == 69 &&
                    evidence.HeritageCombinationRows == 207 &&
                    evidence.ActivationOrderRows == 414 &&
                    evidence.MarkerFirstRows == 21,
                    "69 legal sets, 207 heritage rows, 414 order rows, 21 marker-first rows");
            }
            finally
            {
                evidence.CreatedUnits = units.Count;
                foreach (UnitEntityData unit in units.AsEnumerable().Reverse())
                {
                    unit.Commands.InterruptAll(true);
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Dispose();
                }
                // Only unregistered, request-local BlueprintUnit clones;
                // never the owned race/trait graph or resource-cache entries.
                foreach (BlueprintUnit blueprint in blueprints.AsEnumerable()
                    .Reverse())
                    UnityEngine.Object.DestroyImmediate(blueprint);
                evidence.CleanupExact = before.Length ==
                        Game.Instance.State.Units.All.Count &&
                    before.All(value =>
                        Game.Instance.State.Units.All.Contains(value)) &&
                    units.All(value =>
                        !Game.Instance.State.Units.All.Contains(value));
                assertions.Add(new RuntimeTestAssertion
                {
                    Name = "trait-matrix-disposable-cleanup",
                    Expected = "all request-local units removed; original native unit references retained",
                    Observed = "created=" + units.Count +
                        ";cleanup=" + evidence.CleanupExact,
                    Status = evidence.CleanupExact ?
                        RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                    Evidence = "native Game.State.Units reference set"
                });
                string path = Path.Combine(request.EvidenceDirectory,
                    EvidenceFileName);
                File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                    Formatting.Indented, new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver(),
                        PreserveReferencesHandling =
                            PreserveReferencesHandling.None,
                        ReferenceLoopHandling = ReferenceLoopHandling.Error
                    }));
                evidenceFiles.Add(path);
            }
        }

        private static void ExerciseHeritageChange(UnitDescriptor owner,
            ElementalRaceBlueprints race, ElementalHeritageBlueprints original,
            Fact nativeFact, BlueprintFeature keen, Evidence evidence,
            ICollection<RuntimeTestAssertion> assertions)
        {
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits
                .Traits().First(value => value.Definition.Replaces(
                    ElementalRacialTraitSlot.RacialSpellLikeAbility));
            ElementalHeritageBlueprints next = race.Heritages.Choices().First(
                value => !ReferenceEquals(value, original));
            Add(owner, trait.Marker);
            Add(owner, next.Marker);
            owner.RemoveFact(original.Marker);
            Verify(owner, race, next, new[] { trait }, 0, nativeFact, keen,
                "heritage-changed-with-sla-consumed", evidence, assertions);
            owner.Resources.Add(original.SlaResource, true);
            Check(assertions, "inactive-orphan-injected",
                owner.Resources.GetResourceAmount(original.SlaResource) == 1,
                "native late-activation fixture has one inactive resource use");
            Check(assertions, "inactive-orphan-reconciled",
                ElementalHeritageRuntime.Reconcile(owner, null, null),
                "production service removes the orphan resource");
            Verify(owner, race, next, new[] { trait }, 0, nativeFact, keen,
                "inactive-orphan-removed", evidence, assertions);
            Add(owner, original.Marker);
            owner.RemoveFact(next.Marker);
            owner.RemoveFact(trait.Marker);
            Verify(owner, race, original,
                new ElementalAlternateTraitBlueprints[0], 0,
                nativeFact, keen, "heritage-return-spent", evidence,
                assertions);

            // Same-selection add-before-remove matches native respec preview
            // ordering. The newly activating option controls the providers.
            ElementalAlternateTraitBlueprints replacement = race
                .AlternateTraits.Traits().FirstOrDefault(value =>
                    value.Definition.Id != trait.Definition.Id &&
                    value.Definition.PrimarySlot ==
                        trait.Definition.PrimarySlot);
            if (replacement == null) return;
            Add(owner, trait.Marker);
            Add(owner, replacement.Marker);
            Verify(owner, race, original, new[] { replacement }, 0,
                nativeFact, keen, "same-slot-add-before-remove", evidence,
                assertions);
            owner.RemoveFact(trait.Marker);
            Verify(owner, race, original, new[] { replacement }, 0,
                nativeFact, keen, "same-slot-remove-old", evidence,
                assertions);
            owner.RemoveFact(replacement.Marker);
            Verify(owner, race, original,
                new ElementalAlternateTraitBlueprints[0], 0,
                nativeFact, keen, "same-slot-return-spent", evidence,
                assertions);
        }

        private static void Verify(UnitDescriptor owner,
            ElementalRaceBlueprints race, ElementalHeritageBlueprints heritage,
            ElementalAlternateTraitBlueprints[] traits, int amount,
            Fact nativeFact, BlueprintFeature keen, string label,
            Evidence evidence, ICollection<RuntimeTestAssertion> assertions)
        {
            ElementalRacialTraitSlot used = traits.Aggregate(
                ElementalRacialTraitSlot.None, (current, value) =>
                    current | value.Definition.ReplacedSlots);
            bool resistance = (used &
                ElementalRacialTraitSlot.EnergyResistance) == 0;
            bool affinity = (used &
                ElementalRacialTraitSlot.ElementalAffinity) == 0;
            bool sla = (used &
                ElementalRacialTraitSlot.RacialSpellLikeAbility) == 0;
            bool exact = Rank(owner, race.Race) == 1 &&
                ReferenceEquals(owner.Progression.Race, race.Race) &&
                nativeFact != null && ReferenceEquals(owner.GetFact(keen),
                    nativeFact) && Rank(owner, keen) == 1 &&
                Rank(owner, race.Resistance) == (resistance ? 1 : 0);
            var detail = new List<string>
            {
                race.Definition.DisplayName + "/" + heritage.Definition.Id +
                    "/" + label,
                "resistance=" + Rank(owner, race.Resistance)
            };
            foreach (ElementalHeritageBlueprints choice in
                race.Heritages.Choices())
            {
                bool active = ReferenceEquals(choice, heritage);
                int affinityRank = Rank(owner, choice.Affinity);
                int slaRank = Rank(owner, choice.SlaFeature);
                int abilities = owner.Abilities.Enumerable.Count(value =>
                    ReferenceEquals(value.Blueprint, choice.SlaAbility));
                int resources = owner.Resources.PersistantResources.Count(
                    value => value != null && ReferenceEquals(value.Blueprint,
                        choice.SlaResource));
                int observedAmount = owner.Resources.GetResourceAmount(
                    choice.SlaResource);
                exact &= affinityRank == (active && affinity ? 1 : 0) &&
                    slaRank == (active && sla ? 1 : 0) &&
                    abilities == (active && sla ? 1 : 0) &&
                    resources == (active && sla ? 1 : 0) &&
                    (!active || !sla || observedAmount == amount);
                detail.Add(choice.Definition.Id + "=" + affinityRank + "/" +
                    slaRank + "/" + abilities + "/" + resources + "/" +
                    observedAmount);
            }
            foreach (ElementalAlternateTraitBlueprints trait in
                race.AlternateTraits.Traits())
                exact &= Rank(owner, trait.Provider) ==
                    (traits.Contains(trait) ? 1 : 0);
            foreach (ElementalHeritageStat stat in Enum.GetValues(
                typeof(ElementalHeritageStat)))
            {
                ModifiableValue value = Stat(owner, stat);
                int delta = value.ModifiedValue - value.BaseValue;
                exact &= delta == heritage.Definition.ModifierFor(stat);
                detail.Add(stat + "=" + delta);
            }
            if (race.Definition.Kind == ElementalRaceKind.Undine)
            {
                bool hydraulic = sla && heritage.Definition.IsGeneral;
                foreach (ElementalFeatId id in new[]
                {
                    ElementalFeatId.HydraulicManeuver,
                    ElementalFeatId.TritonPortal
                })
                {
                    PrerequisiteFeature prerequisite = BlueprintBootstrap
                        .ElementalFeats.RequireFeature(id)
                        .ComponentsArray.OfType<PrerequisiteFeature>()
                        .Single(value => ReferenceEquals(value.Feature,
                            race.SlaFeature));
                    bool permitted = prerequisite.Check(null, owner, null);
                    exact &= permitted == hydraulic;
                    detail.Add(id + "-sla-prerequisite=" + permitted);
                }
            }
            string observed = string.Join(";", detail.ToArray());
            evidence.CheckedStates.Add(observed);
            Check(assertions, "trait-state-" + evidence.CheckedStates.Count,
                exact, observed);
        }

        private static void CheckExclusions(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalAlternateTraitBlueprints[] selected,
            ICollection<RuntimeTestAssertion> assertions)
        {
            foreach (ElementalAlternateTraitBlueprints candidate in
                race.AlternateTraits.Traits().Where(value =>
                    !selected.Contains(value)))
            {
                bool conflict = selected.Any(value =>
                    (value.Definition.ReplacedSlots &
                        candidate.Definition.ReplacedSlots) != 0);
                Check(assertions, "trait-native-exclusion-" +
                    candidate.Definition.Id,
                    CanSelect(owner, race, candidate) == !conflict,
                    "native selector prerequisite matches exact slot overlap");
            }
        }

        private static bool CanSelect(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalAlternateTraitBlueprints trait)
        {
            BlueprintFeatureSelection selection = race.AlternateTraits
                .Selections().Single(value => value.Choices.Contains(trait))
                .Selection;
            IFeatureSelectionItem item = selection.ExtractSelectionItems(
                owner, owner).Single(value =>
                    ReferenceEquals(value.Feature, trait.Marker));
            var state = new FeatureSelectionState(null, selection, selection,
                0, 0);
            return selection.CanSelect(owner, null, state, item);
        }

        private static IEnumerable<ElementalAlternateTraitBlueprints[]>
            LegalCombinations(ElementalAlternateTraitBlueprints[] traits)
        {
            for (int mask = 0; mask < (1 << traits.Length); mask++)
            {
                var selected = new List<ElementalAlternateTraitBlueprints>();
                ElementalRacialTraitSlot used = ElementalRacialTraitSlot.None;
                bool legal = true;
                for (int index = 0; index < traits.Length; index++)
                {
                    if ((mask & (1 << index)) == 0) continue;
                    ElementalRacialTraitSlot slots =
                        traits[index].Definition.ReplacedSlots;
                    if ((used & slots) != 0) { legal = false; break; }
                    used |= slots;
                    selected.Add(traits[index]);
                }
                if (legal) yield return selected.ToArray();
            }
        }

        private static UnitEntityData Create(ElementalRaceBlueprints race,
            ICollection<UnitEntityData> units,
            ICollection<BlueprintUnit> blueprints, string label)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_TraitMatrix_" + label;
            blueprint.Race = race.Race;
            blueprints.Add(blueprint);
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                blueprint).Unit;
            if (unit == null || unit.Descriptor == null ||
                !ReferenceEquals(unit.Descriptor.Progression.Race, race.Race))
                throw new InvalidOperationException(
                    "Exact request-local trait fixture creation failed.");
            unit.Descriptor.Stats.HitPoints.BaseValue = 100;
            unit.Descriptor.State.Immortality.Retain();
            foreach (ElementalHeritageStat stat in Enum.GetValues(
                typeof(ElementalHeritageStat)))
                Stat(unit.Descriptor, stat).BaseValue = 10;
            if (!Game.Instance.State.Units.All.Add(unit))
            {
                unit.Dispose();
                throw new InvalidOperationException(
                    "Request-local trait fixture registration failed.");
            }
            units.Add(unit);
            return unit;
        }

        private static void ApplyRace(UnitDescriptor owner,
            ElementalRaceBlueprints race)
        {
            Add(owner, race.Race);
            foreach (BlueprintFeature feature in race.Race.Features)
                if (!owner.HasFact(feature)) owner.AddFact(feature);
        }

        private static void Add(UnitDescriptor owner, BlueprintUnitFact fact)
        {
            if (owner.HasFact(fact)) return;
            if (owner.AddFact(fact) == null || !owner.HasFact(fact))
                throw new InvalidOperationException(
                    "The native fact collection rejected " + fact.AssetGuid);
        }

        private static int Rank(UnitDescriptor owner, BlueprintFeature feature)
        {
            return owner.Progression.Features.GetRank(feature);
        }

        private static ModifiableValue Stat(UnitDescriptor owner,
            ElementalHeritageStat stat)
        {
            switch (stat)
            {
                case ElementalHeritageStat.Strength:
                    return owner.Stats.Strength;
                case ElementalHeritageStat.Dexterity:
                    return owner.Stats.Dexterity;
                case ElementalHeritageStat.Constitution:
                    return owner.Stats.Constitution;
                case ElementalHeritageStat.Intelligence:
                    return owner.Stats.Intelligence;
                case ElementalHeritageStat.Wisdom:
                    return owner.Stats.Wisdom;
                case ElementalHeritageStat.Charisma:
                    return owner.Stats.Charisma;
                default: throw new ArgumentOutOfRangeException("stat");
            }
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions,
            string name, bool passed, string observed)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = "exact native replacement contract",
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = "native facts, resources and selector prerequisites"
            });
            if (!passed)
                throw new InvalidOperationException(name + ": " + observed);
        }
    }
}
