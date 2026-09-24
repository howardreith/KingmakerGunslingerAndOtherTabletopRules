using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalMostlyHumanRaceBlueprints
    {
        internal ElementalMostlyHumanRaceBlueprints(ElementalMostlyHumanDefinition definition,
            BlueprintRace race, BlueprintFeatureSelection selection, BlueprintFeature standard,
            BlueprintFeature trait)
        {
            Definition = definition;
            Race = race;
            Selection = selection;
            Standard = standard;
            Trait = trait;
        }

        internal ElementalMostlyHumanDefinition Definition { get; private set; }
        internal BlueprintRace Race { get; private set; }
        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature Standard { get; private set; }
        internal BlueprintFeature Trait { get; private set; }
    }

    internal sealed class ElementalMostlyHumanBlueprintSet
    {
        private readonly ElementalMostlyHumanRaceBlueprints[] _races;

        internal ElementalMostlyHumanBlueprintSet(BlueprintFeature identity,
            IEnumerable<ElementalMostlyHumanRaceBlueprints> races)
        {
            Identity = identity;
            _races = races.ToArray();
        }

        internal BlueprintFeature Identity { get; private set; }
        internal IList<ElementalMostlyHumanRaceBlueprints> Races
        {
            get { return Array.AsReadOnly(_races); }
        }

        internal ElementalMostlyHumanPublication Publication { get; set; }

        internal int Count { get { return 1 + 3 * _races.Length; } }

        internal ElementalMostlyHumanRaceBlueprints For(ElementalHeritageRace race)
        {
            return _races.Single(value => value.Definition.Race == race);
        }
    }

    /// <summary>
    /// Appends each race's Mostly Human selection to its race features, all
    /// or none, preserving every existing entry and its order. Rollback
    /// restores the exact previous arrays only while this publication's
    /// arrays are still in place; it never touches a foreign change.
    /// </summary>
    internal sealed class ElementalMostlyHumanPublication
    {
        private readonly List<KeyValuePair<BlueprintRace, BlueprintFeatureBase[]>> _previous =
            new List<KeyValuePair<BlueprintRace, BlueprintFeatureBase[]>>();
        private readonly List<BlueprintFeatureBase[]> _published = new List<BlueprintFeatureBase[]>();

        internal static ElementalMostlyHumanPublication Apply(ElementalMostlyHumanBlueprintSet set)
        {
            if (set == null) throw new ArgumentNullException("set");
            foreach (ElementalMostlyHumanRaceBlueprints race in set.Races)
                if ((race.Race.Features ?? new BlueprintFeatureBase[0]).Contains(race.Selection))
                    throw new InvalidOperationException(race.Race.name +
                        " already lists the Mostly Human selection.");
            var publication = new ElementalMostlyHumanPublication();
            try
            {
                foreach (ElementalMostlyHumanRaceBlueprints race in set.Races)
                {
                    BlueprintFeatureBase[] before = race.Race.Features ?? new BlueprintFeatureBase[0];
                    BlueprintFeatureBase[] after = before.Concat(new BlueprintFeatureBase[] { race.Selection })
                        .ToArray();
                    publication._previous.Add(new KeyValuePair<BlueprintRace, BlueprintFeatureBase[]>(
                        race.Race, before));
                    race.Race.Features = after;
                    publication._published.Add(after);
                }
            }
            catch
            {
                publication.Rollback();
                throw;
            }
            return publication;
        }

        internal int PublishedRaces { get { return _published.Count; } }

        internal void Rollback()
        {
            for (int index = _previous.Count - 1; index >= 0; index--)
            {
                BlueprintRace race = _previous[index].Key;
                if (index < _published.Count && ReferenceEquals(race.Features, _published[index]))
                    race.Features = _previous[index].Value;
            }
            _previous.Clear();
            _published.Clear();
        }
    }

    /// <summary>
    /// Registration of the Mostly Human companion racial trait. Every
    /// identity is registered whatever the settings, so a saved choice always
    /// resolves; publication into the four parent races is separate.
    /// </summary>
    internal static class ElementalMostlyHumanBlueprints
    {
        internal static ElementalMostlyHumanBlueprintSet Register(BlueprintRegistry registry,
            ElementalRaceBlueprintSet races)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (races == null) throw new ArgumentNullException("races");
            ElementalMostlyHumanPolicy.Validate();
            BlueprintFeature identity = registry.Register<BlueprintFeature>(
                ElementalMostlyHumanPolicy.IdentitySymbol, CreateIdentity);
            var result = new List<ElementalMostlyHumanRaceBlueprints>();
            foreach (ElementalMostlyHumanDefinition definition in ElementalMostlyHumanPolicy.Ordered())
            {
                ElementalRaceBlueprints parent = races.OrderedBlueprints().Single(value =>
                    (int)value.Definition.Kind == (int)definition.Race);
                BlueprintFeature standard = registry.Register<BlueprintFeature>(
                    definition.StandardSymbol, () => CreateStandard(definition));
                BlueprintFeature trait = registry.Register<BlueprintFeature>(
                    definition.TraitSymbol, () => CreateTrait(definition, identity));
                BlueprintFeatureSelection selection = registry.Register<BlueprintFeatureSelection>(
                    definition.SelectionSymbol, () => CreateSelection(definition, standard, trait));
                result.Add(new ElementalMostlyHumanRaceBlueprints(definition, parent.Race,
                    selection, standard, trait));
            }
            var set = new ElementalMostlyHumanBlueprintSet(identity, result);
            Validate(set);
            return set;
        }

        private static BlueprintFeature CreateIdentity()
        {
            BlueprintFeature feature = BaseFeature(ElementalMostlyHumanPolicy.IdentitySymbol);
            feature.HideInUI = true;
            feature.HideInCharacterSheetAndLevelUp = true;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(ElementalMostlyHumanPolicy.IdentitySymbol + ".Name",
                    "Mostly Human identity"),
                LocalizationService.Create(ElementalMostlyHumanPolicy.IdentitySymbol + ".Description",
                    "Counts as human for effects related to race while keeping the native geniekin identity."),
                null);
            return feature;
        }

        private static BlueprintFeature CreateStandard(ElementalMostlyHumanDefinition definition)
        {
            BlueprintFeature feature = BaseFeature(definition.StandardSymbol);
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(definition.StandardSymbol + ".Name", definition.StandardName),
                LocalizationService.Create(definition.StandardSymbol + ".Description",
                    definition.StandardDescription),
                null);
            return feature;
        }

        private static BlueprintFeature CreateTrait(ElementalMostlyHumanDefinition definition,
            BlueprintFeature identity)
        {
            BlueprintFeature feature = BaseFeature(definition.TraitSymbol);
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$" + feature.name + "_Identity";
            grant.Facts = new BlueprintUnitFact[] { identity };
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(definition.TraitSymbol + ".Name", definition.TraitName),
                LocalizationService.Create(definition.TraitSymbol + ".Description",
                    definition.TraitDescription),
                null);
            return feature;
        }

        private static BlueprintFeatureSelection CreateSelection(
            ElementalMostlyHumanDefinition definition, BlueprintFeature standard,
            BlueprintFeature trait)
        {
            var selection = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            selection.name = InternalName(definition.SelectionSymbol);
            selection.Ranks = 1;
            selection.IsClassFeature = false;
            selection.HideInUI = false;
            selection.IgnorePrerequisites = false;
            // Kingmaker still requires a pick from a non-obligatory selection
            // that offers anything; the explicit standard entry keeps the
            // choice honest and gracefully completable.
            selection.Obligatory = false;
            // The installed Heritage-phase routing contract, as for the
            // alternate-trait slot selections.
            selection.Group = FeatureGroup.AasimarHeritage;
            selection.Group2 = FeatureGroup.None;
            selection.Groups = new[] { FeatureGroup.AasimarHeritage };
            selection.Features = new[] { standard, trait };
            selection.AllFeatures = new[] { standard, trait };
            selection.ComponentsArray = new BlueprintComponent[0];
            BlueprintUnitFactAccess.Resolve().Configure(selection,
                LocalizationService.Create(definition.SelectionSymbol + ".Name", definition.SelectionName),
                LocalizationService.Create(definition.SelectionSymbol + ".Description",
                    definition.SelectionDescription),
                null);
            return selection;
        }

        private static BlueprintFeature BaseFeature(string symbol)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName(symbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.Groups = new FeatureGroup[0];
            feature.ComponentsArray = new BlueprintComponent[0];
            return feature;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }

        private static void Validate(ElementalMostlyHumanBlueprintSet set)
        {
            if (set.Identity == null || !set.Identity.HideInUI ||
                (set.Identity.ComponentsArray ?? new BlueprintComponent[0]).Length != 0 ||
                set.Races.Count != ElementalMostlyHumanPolicy.RaceCount ||
                set.Count != ElementalMostlyHumanPolicy.IdentityCount)
                throw new InvalidOperationException("Mostly Human identity graph failed validation.");
            foreach (ElementalMostlyHumanRaceBlueprints race in set.Races)
            {
                AddFacts[] grants = (race.Trait.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<AddFacts>().ToArray();
                if (race.Race == null || race.Selection.Obligatory ||
                    race.Selection.Group != FeatureGroup.AasimarHeritage ||
                    !race.Selection.AllFeatures.SequenceEqual(new[] { race.Standard, race.Trait }) ||
                    !race.Selection.Features.SequenceEqual(new[] { race.Standard, race.Trait }) ||
                    (race.Standard.ComponentsArray ?? new BlueprintComponent[0]).Length != 0 ||
                    race.Trait.ComponentsArray.Length != 1 || grants.Length != 1 ||
                    grants[0].Facts.Length != 1 || !ReferenceEquals(grants[0].Facts[0], set.Identity) ||
                    string.CompareOrdinal(race.Standard.AssetGuid, race.Trait.AssetGuid) >= 0)
                    throw new InvalidOperationException(race.Definition.RaceName +
                        " Mostly Human graph failed validation.");
            }
        }
    }
}
