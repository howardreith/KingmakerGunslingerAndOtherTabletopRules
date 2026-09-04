using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free Release A qualification over request-local native units.
    /// Exercises every heritage provider graph, exact stat overlay, racial
    /// SLA parameters/resource lifecycle, and both provider activation orders.
    /// </summary>
    internal static class ElementalHeritageMechanicsScenario
    {
        internal const string EvidenceFileName =
            "elemental-heritage-mechanics.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string WizardClassGuid =
            "ba34257984f4c41408ce1dc2004e342e";

        private sealed class ChoiceEvidence
        {
            public string Race { get; set; }
            public string Heritage { get; set; }
            public string MarkerGuid { get; set; }
            public string AffinityGuid { get; set; }
            public string SlaGuid { get; set; }
            public string ResourceGuid { get; set; }
            public string ExpectedStats { get; set; }
            public string ActualStats { get; set; }
            public int MenuCount { get; set; }
            public bool MenuExact { get; set; }
            public bool MenuSelectable { get; set; }
            public bool MenuCommitted { get; set; }
            public bool RaceExact { get; set; }
            public int MarkerCount { get; set; }
            public int AffinityCount { get; set; }
            public int SlaCount { get; set; }
            public int AbilityCount { get; set; }
            public int ResourceRecordCount { get; set; }
            public int ResourceMaximum { get; set; }
            public int ResourceInitial { get; set; }
            public int CharacterLevel { get; set; }
            public int FighterLevel { get; set; }
            public int WizardLevel { get; set; }
            public int CasterLevel { get; set; }
            public int SpellLevel { get; set; }
            public int CharismaModifier { get; set; }
            public int ExpectedDc { get; set; }
            public int ActualDc { get; set; }
            public int DcWithoutAffinity { get; set; }
            public int CharismaModifierWithBonus { get; set; }
            public int DcWithCharismaBonus { get; set; }
            public int CharismaModifierWithPenalty { get; set; }
            public int DcWithCharismaPenalty { get; set; }
            public int DcAfterCleanup { get; set; }
            public int ResourceAfterSpend { get; set; }
            public int ResourceAfterLevelUp { get; set; }
            public int CasterLevelAfterLevelUp { get; set; }
            public int ResourceAfterRest { get; set; }
            public bool SpellLike { get; set; }
            public bool SpellbookAbsent { get; set; }
            public bool MaterialAbsent { get; set; }
            public bool StatsExact { get; set; }
            public bool MenuContract { get; set; }
            public bool ProviderContract { get; set; }
            public bool ParameterContract { get; set; }
            public bool ResourceContract { get; set; }

            public string Summary()
            {
                return Race + "/" + Heritage + ":stats=" + ActualStats +
                    ";menu=" + MenuCount + "/" + MenuExact + "/" +
                    MenuSelectable + "/" + MenuCommitted +
                    ";providers=" + MarkerCount + "/" + AffinityCount +
                    "/" + SlaCount + "/" + AbilityCount +
                    ";resource=" + ResourceInitial + "->" +
                    ResourceAfterSpend + "->" + ResourceAfterLevelUp +
                    "->" + ResourceAfterRest + ";records=" +
                    ResourceRecordCount + ";levels=" + FighterLevel + "+" +
                    WizardLevel + "=" + CharacterLevel + ";params=CL" +
                    CasterLevel + "/SL" + SpellLevel + "/DC" + ActualDc +
                    ";cha=" + CharismaModifier + "->" +
                    CharismaModifierWithBonus + "->" +
                    CharismaModifierWithPenalty + ";tempDc=" +
                    DcWithCharismaBonus + "/" + DcWithCharismaPenalty +
                    "/" + DcAfterCleanup + ";affinityDc=" + ActualDc +
                    "->" + DcWithoutAffinity;
            }
        }

        private sealed class TransitionEvidence
        {
            public string Race { get; set; }
            public string FirstAlternate { get; set; }
            public string SecondAlternate { get; set; }
            public bool LegacyGeneralExact { get; set; }
            public bool FirstAlternateExact { get; set; }
            public bool AddBeforeRemoveExact { get; set; }
            public bool SecondAlternateExact { get; set; }
            public bool IdempotentExact { get; set; }
            public bool GeneralRestoredExact { get; set; }
            public bool FirstSpentAmountRestored { get; set; }
            public bool ExplicitGeneralExact { get; set; }
            public bool MarkerFirstExact { get; set; }
            public bool MarkerFirstRemovalExact { get; set; }
            public int GeneralAmountAfterReturn { get; set; }
            public int FirstAmountAfterReturn { get; set; }
            public int GeneralAmountAfterRest { get; set; }
            public string MarkerFirstProviders { get; set; }

            public bool Pass()
            {
                return LegacyGeneralExact && FirstAlternateExact &&
                    AddBeforeRemoveExact && SecondAlternateExact &&
                    IdempotentExact && GeneralRestoredExact &&
                    FirstSpentAmountRestored && ExplicitGeneralExact &&
                    MarkerFirstExact && MarkerFirstRemovalExact &&
                    GeneralAmountAfterReturn == 0 &&
                    FirstAmountAfterReturn == 0 &&
                    GeneralAmountAfterRest == 1;
            }

            public string Summary()
            {
                return Race + ":legacy=" + LegacyGeneralExact +
                    ";first=" + FirstAlternateExact + ";addBeforeRemove=" +
                    AddBeforeRemoveExact + ";second=" +
                    SecondAlternateExact + ";idempotent=" +
                    IdempotentExact + ";general=" +
                    GeneralRestoredExact + "/" + GeneralAmountAfterReturn +
                    ";firstReturn=" + FirstSpentAmountRestored + "/" +
                    FirstAmountAfterReturn + ";rest=" +
                    GeneralAmountAfterRest + ";explicitGeneral=" +
                    ExplicitGeneralExact + ";markerFirst=" +
                    MarkerFirstExact + "[" + MarkerFirstProviders +
                    "];markerFirstRemoval=" + MarkerFirstRemovalExact;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public List<ChoiceEvidence> Choices { get; set; }
            public List<TransitionEvidence> Transitions { get; set; }
            public bool CleanupExact { get; set; }
        }

        private sealed class ProviderSnapshot
        {
            internal int Markers;
            internal int Affinities;
            internal int Slas;
            internal int Abilities;
            internal int Resources;
            internal int Amount;

            internal string Summary()
            {
                return "markers=" + Markers + ";affinities=" + Affinities +
                    ";slas=" + Slas + ";abilities=" + Abilities +
                    ";resources=" + Resources + ";amount=" + Amount;
            }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                ModuleActive = context.FeatureModules.Active.ElementalRaces,
                SaveStateTouched = false,
                Choices = new List<ChoiceEvidence>(),
                Transitions = new List<TransitionEvidence>()
            };
            var createdUnits = new List<UnitEntityData>();
            var createdBlueprints = new List<BlueprintUnit>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "resolve-production-contract";
            string exceptionSummary = string.Empty;
            try
            {
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                if (set == null || set.Count !=
                        ElementalRaceIdentityCatalog.IdentityCount)
                    throw new InvalidOperationException(
                        "The complete Elemental Races graph is unavailable.");
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "heritage mechanics Fighter fixture");
                BlueprintCharacterClass wizard = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, WizardClassGuid,
                        "heritage mechanics Wizard fixture");

                foreach (ElementalRaceBlueprints race in
                    set.OrderedBlueprints())
                {
                    foreach (ElementalHeritageBlueprints choice in
                        race.Heritages.Choices())
                    {
                        stage = "choice-" + race.Definition.Kind + "-" +
                            choice.Definition.Id;
                        evidence.Choices.Add(ExerciseChoice(race, choice,
                            fighter, wizard, createdUnits,
                            createdBlueprints));
                    }
                    stage = "transition-" + race.Definition.Kind;
                    evidence.Transitions.Add(ExerciseTransitions(race,
                        createdUnits, createdBlueprints));
                }
            }
            catch (Exception exception)
            {
                exceptionSummary = "stage=" + stage + ";" + exception;
                diagnostics.Add(exceptionSummary);
            }
            finally
            {
                foreach (UnitEntityData unit in createdUnits.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    unit.Commands.InterruptAll(true);
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Dispose();
                }
                foreach (BlueprintUnit blueprint in createdBlueprints
                    .AsEnumerable().Reverse().ToArray())
                    if (blueprint != null)
                        UnityEngine.Object.DestroyImmediate(blueprint);
                evidence.CleanupExact = SameReferences(unitsBefore,
                    Game.Instance.State.Units.All.ToArray()) &&
                    createdUnits.All(value => value == null ||
                        !Game.Instance.State.Units.All.Contains(value));
            }

            AddAssertions(assertions, evidence);
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
            diagnostics.Add("elementalHeritageMechanicsSha256=" +
                Hash(path));
            bool pass = string.IsNullOrEmpty(exceptionSummary) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started)
                    .TotalMilliseconds,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static ChoiceEvidence ExerciseChoice(
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints choice,
            BlueprintCharacterClass fighter,
            BlueprintCharacterClass wizard,
            ICollection<UnitEntityData> createdUnits,
            ICollection<BlueprintUnit> createdBlueprints)
        {
            UnitEntityData unit = CreateUnit(race.Race, createdUnits,
                createdBlueprints, "Choice_" + choice.Definition.Id);
            UnitDescriptor owner = unit.Descriptor;
            SetBaseStats(owner);
            ApplyRaceFacts(owner, race);

            IFeatureSelectionItem[] items = race.Heritages.Selection
                .ExtractSelectionItems(owner, owner).ToArray();
            IFeatureSelectionItem item = items.SingleOrDefault(value =>
                value != null && ReferenceEquals(value.Feature,
                    choice.Marker));
            var selectionState = new FeatureSelectionState(null,
                race.Heritages.Selection, race.Heritages.Selection, 0, 0);
            bool selectable = item != null && race.Heritages.Selection
                .CanSelect(owner, null, selectionState, item);
            if (selectable) selectionState.Select(item, null);

            EnsureFact(owner, choice.Marker);
            string expectedStats = ExpectedStats(choice.Definition);
            string actualStats = ActualStats(owner);
            bool statsExact = string.Equals(expectedStats, actualStats,
                StringComparison.Ordinal);
            ProviderSnapshot providers = Snapshot(owner, race, choice);

            Advance(owner, fighter, 2);
            Advance(owner, wizard, 3);
            AbilityData data = RequireCastData(owner, choice.SlaAbility);
            int charisma = owner.Stats.Charisma.Bonus;
            int dc = Context(data, unit).Params.DC;
            int casterLevel = Context(data, unit).Params.CasterLevel;
            int spellLevel = Context(data, unit).Params.SpellLevel;

            BlueprintFeature adjustment = CreateCharismaAdjustment(
                "KMG_Runtime_Heritage_" + choice.Definition.Id + "_Bonus",
                2, ModifierDescriptor.Enhancement);
            EnsureFact(owner, adjustment);
            int charismaBonus = owner.Stats.Charisma.Bonus;
            int dcBonus = Context(data, unit).Params.DC;
            owner.RemoveFact(adjustment);

            adjustment = CreateCharismaAdjustment(
                "KMG_Runtime_Heritage_" + choice.Definition.Id +
                    "_Penalty", -2, ModifierDescriptor.UntypedStackable);
            EnsureFact(owner, adjustment);
            int charismaPenalty = owner.Stats.Charisma.Bonus;
            int dcPenalty = Context(data, unit).Params.DC;
            owner.RemoveFact(adjustment);
            int dcCleanup = Context(data, unit).Params.DC;

            owner.RemoveFact(choice.Affinity);
            int dcWithoutAffinity = Context(data, unit).Params.DC;
            EnsureFact(owner, choice.Affinity);

            int maximum = choice.SlaResource.GetMaxAmount(owner);
            int initial = owner.Resources.GetResourceAmount(
                choice.SlaResource);
            InvokeSpend(data);
            int spent = owner.Resources.GetResourceAmount(
                choice.SlaResource);
            Advance(owner, fighter, 1);
            int afterLevel = owner.Resources.GetResourceAmount(
                choice.SlaResource);
            int casterAfterLevel = Context(data, unit).Params.CasterLevel;
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            int afterRest = owner.Resources.GetResourceAmount(
                choice.SlaResource);

            var result = new ChoiceEvidence
            {
                Race = race.Definition.DisplayName,
                Heritage = choice.Definition.Name,
                MarkerGuid = choice.Marker.AssetGuid,
                AffinityGuid = choice.Affinity.AssetGuid,
                SlaGuid = choice.SlaAbility.AssetGuid,
                ResourceGuid = choice.SlaResource.AssetGuid,
                ExpectedStats = expectedStats,
                ActualStats = actualStats,
                MenuCount = items.Length,
                MenuExact = items.Select(value => value == null ? null :
                        value.Feature).SequenceEqual(race.Heritages.Choices()
                        .Select(value => value.Marker)),
                MenuSelectable = selectable,
                MenuCommitted = ReferenceEquals(selectionState.SelectedItem,
                    item),
                RaceExact = ReferenceEquals(owner.Progression.Race,
                    race.Race) && owner.HasFact(race.Race),
                MarkerCount = providers.Markers,
                AffinityCount = providers.Affinities,
                SlaCount = providers.Slas,
                AbilityCount = providers.Abilities,
                ResourceRecordCount = providers.Resources,
                ResourceMaximum = maximum,
                ResourceInitial = initial,
                CharacterLevel = owner.Progression.CharacterLevel - 1,
                FighterLevel = owner.Progression.GetClassLevel(fighter) - 1,
                WizardLevel = owner.Progression.GetClassLevel(wizard),
                CasterLevel = casterLevel,
                SpellLevel = spellLevel,
                CharismaModifier = charisma,
                ExpectedDc = ElementalRacialSpellLikePolicy.DifficultyClass(
                    choice.Definition.SpellLevel, charisma),
                ActualDc = dc,
                DcWithoutAffinity = dcWithoutAffinity,
                CharismaModifierWithBonus = charismaBonus,
                DcWithCharismaBonus = dcBonus,
                CharismaModifierWithPenalty = charismaPenalty,
                DcWithCharismaPenalty = dcPenalty,
                DcAfterCleanup = dcCleanup,
                ResourceAfterSpend = spent,
                ResourceAfterLevelUp = afterLevel,
                CasterLevelAfterLevelUp = casterAfterLevel,
                ResourceAfterRest = afterRest,
                SpellLike = data.Blueprint.Type == AbilityType.SpellLike,
                SpellbookAbsent = data.Spellbook == null,
                MaterialAbsent = !data.RequireMaterialComponent,
                StatsExact = statsExact
            };
            result.MenuContract = result.MenuCount == 3 &&
                result.MenuExact && result.MenuSelectable &&
                result.MenuCommitted;
            result.ProviderContract = result.RaceExact &&
                result.MarkerCount == 1 && result.AffinityCount == 1 &&
                result.SlaCount == 1 && result.AbilityCount == 1 &&
                result.ResourceRecordCount == 1;
            result.ParameterContract = result.CharacterLevel == 5 &&
                result.FighterLevel == 2 && result.WizardLevel == 3 &&
                result.CasterLevel == 5 &&
                result.SpellLevel == choice.Definition.SpellLevel &&
                result.ActualDc == result.ExpectedDc &&
                result.DcWithoutAffinity == result.ActualDc &&
                result.CharismaModifierWithBonus ==
                    result.CharismaModifier + 1 &&
                result.DcWithCharismaBonus == result.ActualDc + 1 &&
                result.CharismaModifierWithPenalty ==
                    result.CharismaModifier - 1 &&
                result.DcWithCharismaPenalty == result.ActualDc - 1 &&
                result.DcAfterCleanup == result.ActualDc &&
                result.CasterLevelAfterLevelUp == 6 && result.SpellLike &&
                result.SpellbookAbsent && result.MaterialAbsent;
            result.ResourceContract = result.ResourceMaximum == 1 &&
                result.ResourceInitial == 1 &&
                result.ResourceAfterSpend == 0 &&
                result.ResourceAfterLevelUp == 0 &&
                result.ResourceAfterRest == 1;
            return result;
        }

        private static TransitionEvidence ExerciseTransitions(
            ElementalRaceBlueprints race,
            ICollection<UnitEntityData> createdUnits,
            ICollection<BlueprintUnit> createdBlueprints)
        {
            ElementalHeritageBlueprints general = race.Heritages.General;
            ElementalHeritageBlueprints[] alternates = race.Heritages
                .Choices().Where(value => !value.Definition.IsGeneral)
                .ToArray();
            ElementalHeritageBlueprints first = alternates[0];
            ElementalHeritageBlueprints second = alternates[1];
            UnitEntityData unit = CreateUnit(race.Race, createdUnits,
                createdBlueprints, "Transitions_" + race.Definition.Kind);
            UnitDescriptor owner = unit.Descriptor;
            SetBaseStats(owner);
            ApplyRaceFacts(owner, race);
            var result = new TransitionEvidence
            {
                Race = race.Definition.DisplayName,
                FirstAlternate = first.Definition.Name,
                SecondAlternate = second.Definition.Name
            };
            result.LegacyGeneralExact = Exact(owner, race, general, 0) &&
                StatsExact(owner, general.Definition);
            InvokeSpend(RequireCastData(owner, general.SlaAbility));

            EnsureFact(owner, first.Marker);
            result.FirstAlternateExact = Exact(owner, race, first, 1) &&
                StatsExact(owner, first.Definition);
            InvokeSpend(RequireCastData(owner, first.SlaAbility));

            EnsureFact(owner, second.Marker);
            result.AddBeforeRemoveExact = Exact(owner, race, second, 2);
            owner.RemoveFact(first.Marker);
            result.SecondAlternateExact = Exact(owner, race, second, 1) &&
                StatsExact(owner, second.Definition);
            InvokeSpend(RequireCastData(owner, second.SlaAbility));
            bool reconciled = ElementalHeritageRuntime.Reconcile(owner,
                null, null) && ElementalHeritageRuntime.Reconcile(owner,
                    null, null);
            result.IdempotentExact = reconciled && Exact(owner, race,
                second, 1) && owner.Resources.GetResourceAmount(
                    second.SlaResource) == 0;

            owner.RemoveFact(second.Marker);
            result.GeneralAmountAfterReturn = owner.Resources
                .GetResourceAmount(general.SlaResource);
            result.GeneralRestoredExact = Exact(owner, race, general, 0) &&
                StatsExact(owner, general.Definition);

            EnsureFact(owner, first.Marker);
            result.FirstAmountAfterReturn = owner.Resources
                .GetResourceAmount(first.SlaResource);
            result.FirstSpentAmountRestored = Exact(owner, race, first, 1) &&
                result.FirstAmountAfterReturn == 0;
            owner.RemoveFact(first.Marker);
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            result.GeneralAmountAfterRest = owner.Resources
                .GetResourceAmount(general.SlaResource);

            EnsureFact(owner, general.Marker);
            bool explicitMarker = Exact(owner, race, general, 1) &&
                StatsExact(owner, general.Definition);
            owner.RemoveFact(general.Marker);
            result.ExplicitGeneralExact = explicitMarker && Exact(owner,
                race, general, 0) && StatsExact(owner, general.Definition);

            UnitEntityData markerFirst = CreateUnit(race.Race, createdUnits,
                createdBlueprints, "MarkerFirst_" + race.Definition.Kind);
            SetBaseStats(markerFirst.Descriptor);
            EnsureFact(markerFirst.Descriptor, first.Marker);
            ApplyRaceFacts(markerFirst.Descriptor, race);
            ProviderSnapshot markerFirstSnapshot = Snapshot(
                markerFirst.Descriptor, race, first);
            result.MarkerFirstProviders = markerFirstSnapshot.Summary();
            result.MarkerFirstExact = Exact(markerFirst.Descriptor, race,
                first, 1) && StatsExact(markerFirst.Descriptor,
                    first.Definition);
            markerFirst.Descriptor.RemoveFact(first.Marker);
            result.MarkerFirstRemovalExact = Exact(markerFirst.Descriptor,
                race, general, 0) && StatsExact(markerFirst.Descriptor,
                    general.Definition);
            return result;
        }

        private static bool Exact(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints desired, int markers)
        {
            ProviderSnapshot value = Snapshot(owner, race, desired);
            return value.Markers == markers && value.Affinities == 1 &&
                value.Slas == 1 && value.Abilities == 1 &&
                value.Resources == 1 && owner.HasFact(desired.Affinity) &&
                owner.HasFact(desired.SlaFeature) &&
                owner.Abilities.GetAbility(desired.SlaAbility) != null;
        }

        private static ProviderSnapshot Snapshot(UnitDescriptor owner,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints desired)
        {
            ElementalHeritageBlueprints[] choices = race.Heritages.Choices()
                .ToArray();
            var resources = new HashSet<BlueprintAbilityResource>(
                choices.Select(value => value.SlaResource));
            return new ProviderSnapshot
            {
                Markers = choices.Count(value => owner.HasFact(value.Marker)),
                Affinities = choices.Count(value =>
                    owner.HasFact(value.Affinity)),
                Slas = choices.Count(value =>
                    owner.HasFact(value.SlaFeature)),
                Abilities = choices.Count(value => owner.Abilities
                    .GetAbility(value.SlaAbility) != null),
                Resources = owner.Resources.PersistantResources.Count(value =>
                    value != null && resources.Contains(value.Blueprint)),
                Amount = owner.Resources.GetResourceAmount(
                    desired.SlaResource)
            };
        }

        private static void ApplyRaceFacts(UnitDescriptor owner,
            ElementalRaceBlueprints race)
        {
            EnsureFact(owner, race.Race);
            foreach (BlueprintFeature feature in race.Race.Features)
                EnsureFact(owner, feature);
        }

        private static UnitEntityData CreateUnit(BlueprintRace race,
            ICollection<UnitEntityData> createdUnits,
            ICollection<BlueprintUnit> createdBlueprints, string suffix)
        {
            BlueprintUnit source = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(source);
            blueprint.name = "KMG_Runtime_ElementalHeritage_" + suffix;
            blueprint.Race = race;
            createdBlueprints.Add(blueprint);
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Resources == null ||
                !ReferenceEquals(result.Descriptor.Progression.Race, race))
                throw new InvalidOperationException(
                    "A request-local exact-race heritage unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A request-local heritage unit could not be registered.");
            }
            createdUnits.Add(result);
            return result;
        }

        private static void SetBaseStats(UnitDescriptor owner)
        {
            owner.Stats.Strength.BaseValue = 10;
            owner.Stats.Dexterity.BaseValue = 10;
            owner.Stats.Constitution.BaseValue = 10;
            owner.Stats.Intelligence.BaseValue = 10;
            owner.Stats.Wisdom.BaseValue = 10;
            owner.Stats.Charisma.BaseValue = 18;
        }

        private static string ExpectedStats(
            ElementalHeritageDefinition definition)
        {
            return string.Join(",", Enum.GetValues(
                typeof(ElementalHeritageStat)).Cast<ElementalHeritageStat>()
                .Select(value => value + ":" +
                    definition.ModifierFor(value)).ToArray());
        }

        private static string ActualStats(UnitDescriptor owner)
        {
            return string.Join(",", Enum.GetValues(
                typeof(ElementalHeritageStat)).Cast<ElementalHeritageStat>()
                .Select(value => value + ":" +
                    (Stat(owner, value).ModifiedValue -
                        Stat(owner, value).BaseValue)).ToArray());
        }

        private static bool StatsExact(UnitDescriptor owner,
            ElementalHeritageDefinition definition)
        {
            return string.Equals(ExpectedStats(definition),
                ActualStats(owner), StringComparison.Ordinal);
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
                default:
                    throw new ArgumentOutOfRangeException("stat");
            }
        }

        private static AbilityData RequireCastData(UnitDescriptor owner,
            BlueprintAbility ability)
        {
            Ability fact = owner.Abilities.GetAbility(ability);
            if (fact == null)
                throw new InvalidOperationException(
                    "The active heritage ability was not granted: " +
                    ability.AssetGuid + ".");
            var root = new AbilityData(fact);
            AbilityVariants variants = (ability.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>()
                .SingleOrDefault();
            if (variants == null) return root;
            BlueprintAbility child = (variants.Variants ??
                Array.Empty<BlueprintAbility>()).FirstOrDefault();
            if (child == null)
                throw new InvalidOperationException(
                    "The active heritage variant parent has no choices.");
            return new AbilityData(root, child);
        }

        private static AbilityExecutionContext Context(AbilityData data,
            UnitEntityData target)
        {
            return data.CreateExecutionContext(new TargetWrapper(target));
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Request-local unit rejected fact " +
                    blueprint.AssetGuid + ".");
        }

        private static BlueprintFeature CreateCharismaAdjustment(string name,
            int value, ModifierDescriptor descriptor)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.Charisma;
            bonus.Value = value;
            bonus.Descriptor = descriptor;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            return feature;
        }

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels)
        {
            Type type = typeof(LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                BindingFlags.Instance);
            if (select == null || mechanics == null || apply == null ||
                cancel == null)
                throw new MissingMethodException(
                    "The native heritage level-up surface is unavailable.");
            object charGen = Enum.Parse(start.GetParameters()[4]
                .ParameterType, "CharGen", false);
            object controller = null;
            try
            {
                for (int index = 0; index < levels; index++)
                {
                    controller = start.Invoke(null, new object[]
                    {
                        owner, false, null, null, charGen
                    });
                    if (!(bool)select.Invoke(controller,
                            new object[] { characterClass, false }))
                        throw new InvalidOperationException(
                            "Request-local heritage class selection failed.");
                    mechanics.Invoke(controller, null);
                    apply.Invoke(controller, new object[] { owner });
                    cancel.Invoke(controller, null);
                    controller = null;
                }
            }
            finally
            {
                if (controller != null) cancel.Invoke(controller, null);
            }
        }

        private static void InvokeSpend(AbilityData data)
        {
            MethodInfo spend = typeof(AbilityData).GetMethod("Spend",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (spend == null)
                throw new MissingMethodException(typeof(AbilityData).FullName,
                    "Spend");
            spend.Invoke(data, new object[0]);
        }

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-heritage-choice-count", "12",
                evidence.Choices.Count.ToString(),
                evidence.Choices.Count == ElementalHeritagePolicy
                    .HeritageCount,
                "twelve production heritage exercises");
            foreach (ChoiceEvidence choice in evidence.Choices)
            {
                string key = (choice.Race + "-" + choice.Heritage)
                    .ToLowerInvariant().Replace(" ", "-");
                string observed = choice.Summary();
                Add(assertions, key + "-selection", "ordered native menu " +
                    "with exactly three choices and committed exact item",
                    observed, choice.MenuContract,
                    "ExtractSelectionItems, CanSelect, and FeatureSelectionState.Select");
                Add(assertions, key + "-stats", choice.ExpectedStats,
                    choice.ActualStats, choice.StatsExact,
                    "live racial modifiers on a request-local UnitDescriptor");
                Add(assertions, key + "-providers", "one marker, one exact " +
                    "affinity/SLA/ability/resource, no inactive providers",
                    observed, choice.ProviderContract,
                    "live fact, ability, and persistent resource collections");
                Add(assertions, key + "-parameters", "2 Fighter + 3 Wizard " +
                    "=> CL 5; DC=10+spell level+current Charisma; affinity +0; temporary Charisma live",
                    observed, choice.ParameterContract,
                    "native level-up and AbilityExecutionContext calculation");
                Add(assertions, key + "-resource", "one use; spend -> 0; " +
                    "level-up stays 0; ordinary rest -> 1", observed,
                    choice.ResourceContract,
                    "production AbilityData.Spend and native ApplyRest");
            }
            Add(assertions, "elemental-heritage-transition-count", "4",
                evidence.Transitions.Count.ToString(),
                evidence.Transitions.Count == ElementalRaceCatalog.RaceCount,
                "one transition sequence per exact parent race");
            foreach (TransitionEvidence transition in evidence.Transitions)
                Add(assertions, "elemental-heritage-transition-" +
                    transition.Race.ToLowerInvariant(),
                    "legacy General; alternate A -> B -> General; add/remove orders; idempotence; spent amounts preserved",
                    transition.Summary(), transition.Pass(),
                    "live component activation/deactivation and reconciliation");
            Add(assertions, "elemental-heritage-mechanics-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "request-local units only; no save or player-party API");
            Add(assertions, "elemental-heritage-mechanics-cleanup",
                "exact pre-run global-unit reference sequence",
                evidence.CleanupExact.ToString(), evidence.CleanupExact,
                "finally interruption, removal, disposal, and exact comparison");
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool passed, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static bool SameReferences<T>(IList<T> expected,
            IList<T> actual) where T : class
        {
            if (expected == null || actual == null ||
                expected.Count != actual.Count) return false;
            for (int index = 0; index < expected.Count; index++)
                if (!ReferenceEquals(expected[index], actual[index]))
                    return false;
            return true;
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>().SingleOrDefault(
                    item => string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
