using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.ElementalRaces.Visuals;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free structural render qualification for the four production
    /// elemental races. Every installed choice is exercised at least once;
    /// one view exists at a time and is destroyed before the next case.
    /// </summary>
    internal static class ElementalRaceVisualAuditScenario
    {
        internal const string EvidenceFileName =
            "elemental-race-visual-audit.json";
        internal const string ClassClothingEvidenceFileName =
            "elemental-race-class-clothing.json";
        private const int MaximumViewSettleUpdates = 360;
        private const int MinimumCasesPerRaceAndSex = 7;
        private const int MinimumCaseCount =
            ElementalRaceCatalog.RaceCount * 2 *
            MinimumCasesPerRaceAndSex;
        private const int ClassClothingClassCount = 10;
        private const int ClassClothingCaseCount =
            ElementalRaceCatalog.RaceCount * 2 *
            ClassClothingClassCount;

        private static readonly PropertyInfo EyebrowsProperty =
            RequireEyebrowsProperty();

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        private sealed class ClassClothingDefinition
        {
            internal ClassClothingDefinition(string key, string guid)
            {
                Key = key;
                Guid = guid;
            }

            internal readonly string Key;
            internal readonly string Guid;
        }

        private sealed class ResolvedClassClothing
        {
            internal ResolvedClassClothing(string key,
                BlueprintCharacterClass characterClass)
            {
                Key = key;
                CharacterClass = characterClass;
            }

            internal readonly string Key;
            internal readonly BlueprintCharacterClass CharacterClass;
        }

        private sealed class RenderCase
        {
            internal string Label;
            internal ElementalRaceKind Kind;
            internal BlueprintRace Race;
            internal Gender Gender;
            internal BlueprintRaceVisualPreset Preset;
            internal string ClassKey;
            internal BlueprintCharacterClass CharacterClass;
            internal string[] ClassClothingAssetIds;
            internal bool ClassClothingExact;
            internal int ClassClothingInitiallyPresentCount;
            internal int ClassClothingAddedCount;
            internal int ClassClothingPresentCount;
            internal EquipmentEntityLink Head;
            internal EquipmentEntityLink Hair;
            internal EquipmentEntityLink Eyebrows;
            internal EquipmentEntityLink Beard;
            internal EquipmentEntityLink Horn;
            internal string BodyAssetId;
            internal int SkinIndex;
            internal int HairColorIndex;
            internal int HairRampCount;
            internal int HornColorIndex;
            internal int HornRampCount;
            internal string[] RequiredEntityIds;
            internal bool DataContract;
            internal bool MaterialContract;
        }

        internal sealed class Session
        {
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<RenderCase> _cases =
                new List<RenderCase>();
            private readonly JObject _evidence = new JObject
            {
                { "schemaVersion", 1 },
                { "saveStateTouched", false },
                { "selectorStateTouched", false },
                { "races", new JArray() },
                { "resourceRegistrations", new JArray() },
                { "dataFailures", new JArray() },
                { "renderCases", new JArray() },
                { "coverage", new JArray() }
            };

            private BlueprintRoot _root;
            private LibraryScriptableObject _library;
            private BlueprintRace[] _racesBefore;
            private string[] _raceIdentitiesBefore;
            private int _allBefore;
            private int _dictionaryBefore;
            private BlueprintCharacterClass _gunslinger;
            private DollState _state;
            private DollData _data;
            private UnitEntityView _view;
            private int _caseIndex;
            private int _settleUpdates;
            private bool _classClothingApplied;
            private string _stage = "resolve-production-visuals";
            private string _exceptionSummary = string.Empty;

            private bool IsClassClothing
            {
                get
                {
                    return string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRaceClassClothing,
                        StringComparison.Ordinal);
                }
            }

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                _context = context;
                _request = request;
            }

            internal bool Complete { get; private set; }
            internal RuntimeTestResult Result { get; private set; }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_cases.Count == 0)
                    {
                        Initialize();
                        StartCurrentCase();
                        return;
                    }
                    PollCurrentCase();
                }
                catch (Exception exception)
                {
                    RecordException(exception);
                    Finish();
                }
            }

            private void Initialize()
            {
                _root = BlueprintRoot.Instance;
                _library = BlueprintBootstrap.Library;
                ElementalRaceBlueprintSet production =
                    BlueprintBootstrap.ElementalRaces;
                _gunslinger = BlueprintBootstrap.GunslingerClass == null ?
                    null : BlueprintBootstrap.GunslingerClass.CharacterClass;
                if (_root == null || _root.Progression == null ||
                    _root.Progression.CharacterRaces == null ||
                    _library == null || _library.BlueprintsByAssetId == null ||
                    _library.GetAllBlueprints() == null ||
                    production == null || _gunslinger == null)
                    throw new InvalidOperationException(
                        "Production race, class, root, or library state is unavailable.");

                _racesBefore = _root.Progression.CharacterRaces;
                _raceIdentitiesBefore = RaceIdentities(_racesBefore);
                _allBefore = _library.GetAllBlueprints().Count;
                _dictionaryBefore = _library.BlueprintsByAssetId.Count;

                _stage = "audit-production-inventory";
                ElementalRaceBlueprints[] races = production
                    .OrderedBlueprints().ToArray();
                ElementalRaceVisualBlueprints[] visuals = production.Visuals
                    .Ordered().ToArray();
                ResolvedClassClothing[] classClothing = IsClassClothing
                    ? ResolveClassClothing()
                    : new ResolvedClassClothing[0];
                if (races.Length != ElementalRaceCatalog.RaceCount ||
                    visuals.Length != ElementalRaceCatalog.RaceCount ||
                    production.Visuals.BlueprintCount !=
                        ElementalRaceVisualCatalog.BlueprintIdentityCount ||
                    production.Visuals.ResourceCount !=
                        ElementalRaceVisualCatalog.ResourceIdentityCount)
                    throw new InvalidOperationException(
                        "Production elemental visual inventory drifted.");

                bool allExact = true;
                for (int index = 0; index < races.Length; index++)
                {
                    if (!ReferenceEquals(races[index].Visuals, visuals[index]))
                        throw new InvalidOperationException(
                            "Race and visual-set ordering diverged at index " +
                            index + ".");
                    allExact &= AuditRace(races[index], visuals[index]);
                    if (IsClassClothing)
                        BuildClassClothingCases(races[index], visuals[index],
                            classClothing);
                    else
                        BuildCases(races[index], visuals[index]);
                }

                bool outfitExact = GunslingerClassAppearanceCatalog
                    .MaleAssetIds().Concat(GunslingerClassAppearanceCatalog
                        .FemaleAssetIds()).All(value => ResourcesLibrary
                            .TryGetResource<EquipmentEntity>(value, true) !=
                                null);
                if (IsClassClothing)
                {
                    bool classesExact = classClothing.Length ==
                            ClassClothingClassCount &&
                        classClothing.Select(value => value.CharacterClass
                            .AssetGuid).Distinct(StringComparer.Ordinal)
                            .Count() == ClassClothingClassCount &&
                        _cases.All(value => value.ClassClothingExact);
                    Add(_assertions, "elemental-class-clothing-inventory",
                        "16 exact visual blueprints, 28 exact proxies, four fixed-order races, and ten exact class clothing donors",
                        "blueprints=" + production.Visuals.BlueprintCount +
                            ";resources=" + production.Visuals.ResourceCount +
                            ";races=" + races.Length + ";classes=" +
                            classClothing.Length + ";visualsExact=" +
                            allExact + ";clothesExact=" + classesExact,
                        allExact && classesExact && outfitExact,
                        "live BlueprintBootstrap, class catalog, LoadClothes, and resource cache");
                    Add(_assertions, "elemental-class-clothing-case-plan",
                        "exactly 80 unique race/sex/class render cases",
                        "cases=" + _cases.Count,
                        _cases.Count == ClassClothingCaseCount,
                        "deterministic four-race/two-sex/ten-class plan");
                }
                else
                {
                    Add(_assertions, "elemental-visual-inventory",
                        "16 exact visual blueprints, 28 exact proxies, four fixed-order production races, and accepted Gunslinger links",
                        "blueprints=" + production.Visuals.BlueprintCount +
                            ";resources=" + production.Visuals.ResourceCount +
                            ";races=" + races.Length + ";exact=" + allExact +
                            ";gunslingerLinks=" + outfitExact,
                        allExact && outfitExact,
                        "live BlueprintBootstrap set and resource cache");
                    Add(_assertions, "elemental-visual-case-plan",
                        "at least 56 finite cases covering every installed option and all seven skin indexes",
                        "cases=" + _cases.Count,
                        _cases.Count >= MinimumCaseCount,
                        "deterministic race/sex option plan");
                }
            }

            private bool AuditRace(ElementalRaceBlueprints raceBlueprints,
                ElementalRaceVisualBlueprints visuals)
            {
                BlueprintRace race = raceBlueprints.Race;
                bool blueprintExact = race != null &&
                    raceBlueprints.Definition.Kind == visuals.Definition.Kind &&
                    race.Presets != null && race.Presets.Length == 3 &&
                    race.Presets.SequenceEqual(visuals.Presets) &&
                    ReferenceEquals(race.MaleOptions,
                        visuals.MaleOptions) &&
                    ReferenceEquals(race.FemaleOptions,
                        visuals.FemaleOptions) &&
                    ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                        KingmakerEquipmentEntity>(visuals.Body.AssetGuid),
                        visuals.Body) && visuals.Presets.All(value =>
                            ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                                BlueprintRaceVisualPreset>(value.AssetGuid),
                                value));

                bool resourcesExact = true;
                foreach (ElementalRaceVisualResourceRegistration registration
                    in visuals.Resources)
                {
                    EquipmentEntity resolved = ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(registration.AssetId, true);
                    bool paletteExact = !registration.Spec.UsesSkinPalette ||
                        resolved != null && resolved.PrimaryRamps != null &&
                        resolved.PrimaryRamps.Count ==
                            ElementalRaceVisualCatalog.SkinRampCount &&
                        resolved.PrimaryRamps.All(IsNativeRamp);
                    bool exact = ReferenceEquals(resolved,
                        registration.Resource) && paletteExact;
                    resourcesExact &= exact;
                    ((JArray)_evidence["resourceRegistrations"]).Add(
                        new JObject
                        {
                            { "race", visuals.Definition.Kind.ToString() },
                            { "symbol", registration.Spec.Symbol },
                            { "assetId", registration.AssetId },
                            { "resourceName", resolved == null ? "<null>" :
                                resolved.name },
                            { "usesSkinPalette",
                                registration.Spec.UsesSkinPalette },
                            { "skinRamps", resolved == null ||
                                resolved.PrimaryRamps == null ? 0 :
                                resolved.PrimaryRamps.Count },
                            { "skinRampNames", new JArray(resolved == null ||
                                resolved.PrimaryRamps == null ?
                                new string[0] : resolved.PrimaryRamps.Select(
                                    value => value == null ? "<null>" :
                                        value.name).ToArray()) },
                            { "usedFallback", registration.UsedFallback },
                            { "exact", exact }
                        });
                }

                bool maleExact = OptionsExact(race.MaleOptions, true,
                    visuals.Definition.Kind);
                bool femaleExact = OptionsExact(race.FemaleOptions, false,
                    visuals.Definition.Kind);
                bool bodyExact = BodyExact(race, visuals, Gender.Male) &&
                    BodyExact(race, visuals, Gender.Female);
                ((JArray)_evidence["races"]).Add(new JObject
                {
                    { "kind", visuals.Definition.Kind.ToString() },
                    { "raceGuid", race == null ? "<null>" : race.AssetGuid },
                    { "raceId", race == null ? "<null>" :
                        race.RaceId.ToString() },
                    { "presetGuids", new JArray((race == null ||
                        race.Presets == null ?
                        new BlueprintRaceVisualPreset[0] : race.Presets)
                            .Select(value => value == null ? "<null>" :
                                value.AssetGuid).ToArray()) },
                    { "maleOptions", DescribeOptions(race == null ? null :
                        race.MaleOptions) },
                    { "femaleOptions", DescribeOptions(race == null ? null :
                        race.FemaleOptions) },
                    { "resources", visuals.ResourceCount },
                    { "usedFallback", visuals.UsedFallback },
                    { "blueprintExact", blueprintExact },
                    { "bodyExact", bodyExact },
                    { "resourcesExact", resourcesExact }
                });
                return blueprintExact && bodyExact && resourcesExact &&
                    maleExact && femaleExact;
            }

            private void BuildCases(ElementalRaceBlueprints raceBlueprints,
                ElementalRaceVisualBlueprints visuals)
            {
                foreach (Gender gender in new[] { Gender.Male, Gender.Female })
                {
                    CustomizationOptions options = gender == Gender.Male ?
                        raceBlueprints.Race.MaleOptions :
                        raceBlueprints.Race.FemaleOptions;
                    ElementalRaceSexVisualDefinition definition =
                        gender == Gender.Male ? visuals.Definition.Male :
                            visuals.Definition.Female;
                    string bodyId = visuals.Resources.Single(value =>
                        string.Equals(value.Spec.Symbol,
                            definition.Body.Symbol,
                            StringComparison.Ordinal)).AssetId;
                    int caseCount = new[]
                    {
                        MinimumCasesPerRaceAndSex,
                        raceBlueprints.Race.Presets.Length,
                        options.Heads.Length, options.Hair.Length,
                        options.Eyebrows.Length, options.Beards.Length,
                        options.Horns.Length
                    }.Max();
                    for (int index = 0; index < caseCount; index++)
                    {
                        _cases.Add(new RenderCase
                        {
                            Label = visuals.Definition.Kind.ToString()
                                .ToLowerInvariant() + "-" +
                                gender.ToString().ToLowerInvariant() + "-" +
                                (index + 1),
                            Kind = visuals.Definition.Kind,
                            Race = raceBlueprints.Race,
                            Gender = gender,
                            ClassKey = "gunslinger",
                            CharacterClass = _gunslinger,
                            ClassClothingAssetIds = new string[0],
                            ClassClothingExact = true,
                            Preset = raceBlueprints.Race.Presets[index %
                                raceBlueprints.Race.Presets.Length],
                            Head = options.Heads[index %
                                options.Heads.Length],
                            Hair = options.Hair[index % options.Hair.Length],
                            Eyebrows = options.Eyebrows[index %
                                options.Eyebrows.Length],
                            Beard = options.Beards.Length == 0 ? null :
                                options.Beards[index % options.Beards.Length],
                            Horn = options.Horns.Length == 0 ? null :
                                options.Horns[index % options.Horns.Length],
                            BodyAssetId = bodyId,
                            SkinIndex = index %
                                ElementalRaceVisualCatalog.SkinRampCount
                        });
                    }
                }
            }

            private ResolvedClassClothing[] ResolveClassClothing()
            {
                var result = new List<ResolvedClassClothing>
                {
                    new ResolvedClassClothing("gunslinger", _gunslinger)
                };
                foreach (ClassClothingDefinition definition in
                    NativeClassClothingDefinitions())
                {
                    BlueprintCharacterClass characterClass =
                        ResourcesLibrary.TryGetBlueprint<
                            BlueprintCharacterClass>(definition.Guid);
                    if (characterClass == null)
                        throw new InvalidOperationException(
                            "Required class clothing donor did not resolve: " +
                            definition.Key + "/" + definition.Guid + ".");
                    result.Add(new ResolvedClassClothing(definition.Key,
                        characterClass));
                }
                BlueprintCharacterClass[] classes = result.Select(value =>
                    value.CharacterClass).ToArray();
                if (classes.Length != ClassClothingClassCount ||
                    classes.Any(value => value == null) ||
                    classes.Select(value => value.AssetGuid).Distinct(
                        StringComparer.Ordinal).Count() != classes.Length ||
                    classes.Any(value => !_root.Progression.CharacterClasses
                        .Any(installed => ReferenceEquals(installed, value))))
                    throw new InvalidOperationException(
                        "The exact ten-class clothing catalog is unavailable or ambiguous.");
                return result.ToArray();
            }

            private void BuildClassClothingCases(
                ElementalRaceBlueprints raceBlueprints,
                ElementalRaceVisualBlueprints visuals,
                IEnumerable<ResolvedClassClothing> classes)
            {
                ResolvedClassClothing[] ordered = classes.ToArray();
                foreach (Gender gender in new[] { Gender.Male, Gender.Female })
                {
                    CustomizationOptions options = gender == Gender.Male
                        ? raceBlueprints.Race.MaleOptions
                        : raceBlueprints.Race.FemaleOptions;
                    ElementalRaceSexVisualDefinition definition =
                        gender == Gender.Male ? visuals.Definition.Male :
                            visuals.Definition.Female;
                    string bodyId = visuals.Resources.Single(value =>
                        string.Equals(value.Spec.Symbol,
                            definition.Body.Symbol,
                            StringComparison.Ordinal)).AssetId;
                    EquipmentEntityLink hair = options.Hair.FirstOrDefault(
                        value => !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal)) ?? options.Hair[0];
                    EquipmentEntityLink beard = gender == Gender.Male
                        ? options.Beards.FirstOrDefault(value =>
                            !string.Equals(value.AssetId,
                                ElementalRaceVisualCatalog.EmptyAssetId,
                                StringComparison.Ordinal))
                        : null;
                    EquipmentEntityLink horn = options.Horns.FirstOrDefault(
                        value => !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal));
                    for (int index = 0; index < ordered.Length; index++)
                    {
                        ResolvedClassClothing current = ordered[index];
                        string[] clothing = RequireClassClothingAssetIds(
                            current.CharacterClass, gender,
                            raceBlueprints.Race);
                        _cases.Add(new RenderCase
                        {
                            Label = visuals.Definition.Kind.ToString()
                                .ToLowerInvariant() + "-" +
                                gender.ToString().ToLowerInvariant() + "-" +
                                current.Key,
                            Kind = visuals.Definition.Kind,
                            Race = raceBlueprints.Race,
                            Gender = gender,
                            ClassKey = current.Key,
                            CharacterClass = current.CharacterClass,
                            ClassClothingAssetIds = clothing,
                            ClassClothingExact = true,
                            Preset = raceBlueprints.Race.Presets[index %
                                raceBlueprints.Race.Presets.Length],
                            Head = options.Heads[index %
                                options.Heads.Length],
                            Hair = hair,
                            Eyebrows = options.Eyebrows[index %
                                options.Eyebrows.Length],
                            Beard = beard,
                            Horn = horn,
                            BodyAssetId = bodyId,
                            SkinIndex = index %
                                ElementalRaceVisualCatalog.SkinRampCount
                        });
                    }
                }
            }

            private static string[] RequireClassClothingAssetIds(
                BlueprintCharacterClass characterClass, Gender gender,
                BlueprintRace race)
            {
                var links = new List<EquipmentEntityLink>();
                links.AddRange((gender == Gender.Male
                    ? characterClass.MaleEquipmentEntities
                    : characterClass.FemaleEquipmentEntities) ??
                        new EquipmentEntityLink[0]);
                foreach (KingmakerEquipmentEntity wrapper in
                    characterClass.EquipmentEntities ??
                        new KingmakerEquipmentEntity[0])
                {
                    if (wrapper == null)
                        throw new InvalidOperationException(
                            "Class clothing contains a null race wrapper: " +
                            characterClass.AssetGuid + ".");
                    links.AddRange(wrapper.GetLinks(gender, race.RaceId) ??
                        new EquipmentEntityLink[0]);
                }
                string[] ids = links.Select(AssetId).Where(value =>
                    !string.IsNullOrWhiteSpace(value)).ToArray();
                if (ids.Length == 0)
                    throw new InvalidOperationException(
                        "Class clothing is empty for " +
                        characterClass.AssetGuid + "/" + race.AssetGuid +
                        "/" + gender + ".");
                EquipmentEntity[] expected = ids.Select(value =>
                {
                    EquipmentEntity entity = ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(value, true);
                    if (entity == null)
                        throw new InvalidOperationException(
                            "Class clothing resource did not resolve: " +
                            value + ".");
                    return entity;
                }).ToArray();
                EquipmentEntity[] observed = characterClass.LoadClothes(
                    gender, race).ToArray();
                if (observed.Length != expected.Length ||
                    observed.Any(value => value == null))
                    throw new InvalidOperationException(
                        "Race-aware class clothing candidates drifted for " +
                        characterClass.AssetGuid + "/" + race.AssetGuid +
                        "/" + gender + ": candidates=" +
                        expected.Length + ";observed=" + observed.Length +
                        ".");

                var unmatchedIds = ids.ToList();
                var unmatchedEntities = expected.ToList();
                var observedIds = new List<string>();
                foreach (EquipmentEntity entity in observed)
                {
                    int match = unmatchedEntities.FindIndex(value =>
                        ReferenceEquals(value, entity));
                    if (match < 0)
                        throw new InvalidOperationException(
                            "LoadClothes returned an unaudited class clothing " +
                            "resource for " + characterClass.AssetGuid + "/" +
                            race.AssetGuid + "/" + gender + ".");
                    observedIds.Add(unmatchedIds[match]);
                    unmatchedIds.RemoveAt(match);
                    unmatchedEntities.RemoveAt(match);
                }
                if (unmatchedEntities.Count != 0)
                    throw new InvalidOperationException(
                        "LoadClothes omitted audited class clothing resources " +
                        "for " + characterClass.AssetGuid + "/" +
                        race.AssetGuid + "/" + gender + ".");
                return observedIds.ToArray();
            }

            private void StartCurrentCase()
            {
                if (_caseIndex >= _cases.Count)
                {
                    CompleteCoverage();
                    Finish();
                    return;
                }
                RenderCase renderCase = _cases[_caseIndex];
                _stage = "create-" + renderCase.Label;
                _data = CreateData(renderCase, out _state);
                _view = ElementalRaceDevelopmentProbeScenario.CreateView(
                    renderCase.Label, _data);
                _classClothingApplied = false;
                _settleUpdates = 0;
            }

            private void ApplyClassClothing(RenderCase renderCase)
            {
                Character avatar = _view == null ? null :
                    _view.GetComponent<Character>();
                if (avatar == null || avatar.EquipmentEntities == null)
                    throw new InvalidOperationException(renderCase.Label +
                        " did not create a class-clothing avatar.");
                EquipmentEntity[] expected = ResolveClassClothingEntities(
                    renderCase);
                renderCase.ClassClothingInitiallyPresentCount =
                    expected.Count(value => avatar.EquipmentEntities.Any(
                        current => ReferenceEquals(current, value)));
                EquipmentEntity[] missing = expected.Where(value =>
                    !avatar.EquipmentEntities.Any(current =>
                        ReferenceEquals(current, value))).ToArray();
                avatar.AddEquipmentEntities(missing, false);
                renderCase.ClassClothingAddedCount = missing.Length;
                foreach (EquipmentEntity entity in expected)
                    ApplyClassPalette(avatar, renderCase.CharacterClass,
                        entity);
                avatar.RebuildOutfit();
                renderCase.ClassClothingPresentCount = expected.Count(value =>
                    avatar.EquipmentEntities.Any(current =>
                        ReferenceEquals(current, value)));
                renderCase.ClassClothingExact &=
                    renderCase.ClassClothingPresentCount == expected.Length;
            }

            private static void ApplyClassPalette(Character avatar,
                BlueprintCharacterClass characterClass,
                EquipmentEntity entity)
            {
                int primaryCount = entity.PrimaryRamps == null ? 0 :
                    entity.PrimaryRamps.Count;
                int secondaryCount = entity.SecondaryRamps == null ? 0 :
                    entity.SecondaryRamps.Count;
                int primary = primaryCount == 0 ? -1 :
                    characterClass.PrimaryColor;
                int secondary = secondaryCount == 0 ? -1 :
                    characterClass.SecondaryColor;
                if (primary >= primaryCount || secondary >= secondaryCount)
                    throw new InvalidOperationException(entity.name +
                        " does not support the class default palette for " +
                        characterClass.AssetGuid + ".");
                if (primary >= 0 && secondary >= 0)
                    avatar.SetRampIndices(entity, primary, secondary, false);
                else if (primary >= 0)
                    avatar.SetPrimaryRampIndex(entity, primary, false);
                else if (secondary >= 0)
                    avatar.SetSecondaryRampIndex(entity, secondary, false);
            }

            private static EquipmentEntity[] ResolveClassClothingEntities(
                RenderCase renderCase)
            {
                return (renderCase.ClassClothingAssetIds ?? new string[0])
                    .Select(value =>
                    {
                        EquipmentEntity entity = ResourcesLibrary
                            .TryGetResource<EquipmentEntity>(value, true);
                        if (entity == null)
                            throw new InvalidOperationException(
                                "Class clothing resource did not resolve: " +
                                value + ".");
                        return entity;
                    }).ToArray();
            }

            private void PollCurrentCase()
            {
                RenderCase renderCase = _cases[_caseIndex];
                _stage = "settle-" + renderCase.Label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                bool ready = ElementalRaceDevelopmentProbeScenario.ViewReady(
                    _view);
                if (IsClassClothing && !_classClothingApplied)
                {
                    if (!ready && _settleUpdates <
                        MaximumViewSettleUpdates) return;
                    if (!ready)
                        throw new InvalidOperationException(
                            renderCase.Label +
                            " did not settle its native base avatar before " +
                            "class clothing application.");
                    ApplyClassClothing(renderCase);
                    _classClothingApplied = true;
                    _settleUpdates = 0;
                    return;
                }
                if (!ready && _settleUpdates < MaximumViewSettleUpdates)
                    return;

                JObject view = ElementalRaceDevelopmentProbeScenario
                    .DescribeView(renderCase.Label, _data, _view, ready,
                        _settleUpdates);
                EquipmentEntity body = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(renderCase.BodyAssetId, true);
                Character avatar = _view == null ? null :
                    _view.GetComponent<Character>();
                EquipmentEntity[] classClothing = IsClassClothing
                    ? ResolveClassClothingEntities(renderCase)
                    : new EquipmentEntity[0];
                renderCase.ClassClothingPresentCount = avatar == null ||
                    avatar.EquipmentEntities == null ? 0 :
                    classClothing.Count(value => avatar.EquipmentEntities.Any(
                        current => ReferenceEquals(current, value)));
                bool classClothingViewExact = !IsClassClothing ||
                    (classClothing.Length > 0 &&
                    renderCase.ClassClothingPresentCount ==
                        classClothing.Length);
                renderCase.ClassClothingExact &= classClothingViewExact;
                bool bodyResourceRetained = body != null && avatar != null &&
                    avatar.EquipmentEntities != null &&
                    avatar.EquipmentEntities.Any(value =>
                        ReferenceEquals(value, body));
                bool bakedCharacterRenderer =
                    ((JArray)view["rendererNames"]).Values<string>().Any(
                        value => value != null && value.StartsWith(
                            "Renderer_Character_",
                            StringComparison.Ordinal));
                bool materialExact = ready &&
                    classClothingViewExact &&
                    bakedCharacterRenderer &&
                    (int)view["renderableRenderers"] > 0 &&
                    (int)view["nullMaterials"] == 0 &&
                    (int)view["nullShaders"] == 0;
                renderCase.MaterialContract = materialExact;
                ((JArray)_evidence["renderCases"]).Add(new JObject
                {
                    { "label", renderCase.Label },
                    { "race", renderCase.Kind.ToString() },
                    { "gender", renderCase.Gender.ToString() },
                    { "classKey", renderCase.ClassKey },
                    { "className", renderCase.CharacterClass.name },
                    { "classGuid", renderCase.CharacterClass.AssetGuid },
                    { "classClothingAssetIds", new JArray(
                        renderCase.ClassClothingAssetIds) },
                    { "classClothingExact",
                        renderCase.ClassClothingExact },
                    { "classClothingInitiallyPresentCount",
                        renderCase.ClassClothingInitiallyPresentCount },
                    { "classClothingAddedCount",
                        renderCase.ClassClothingAddedCount },
                    { "classClothingPresentCount",
                        renderCase.ClassClothingPresentCount },
                    { "presetGuid", renderCase.Preset.AssetGuid },
                    { "body", renderCase.BodyAssetId },
                    { "bodyResourceRetainedAfterBake",
                        bodyResourceRetained },
                    { "bakedCharacterRenderer", bakedCharacterRenderer },
                    { "head", renderCase.Head.AssetId },
                    { "hair", renderCase.Hair.AssetId },
                    { "eyebrows", renderCase.Eyebrows.AssetId },
                    { "beard", AssetId(renderCase.Beard) },
                    { "horn", AssetId(renderCase.Horn) },
                    { "skinIndex", renderCase.SkinIndex },
                    { "hairColorIndex", renderCase.HairColorIndex },
                    { "hairRampCount", renderCase.HairRampCount },
                    { "hornColorIndex", renderCase.HornColorIndex },
                    { "hornRampCount", renderCase.HornRampCount },
                    { "requiredEntityIds", new JArray(
                        renderCase.RequiredEntityIds) },
                    { "dataContract", renderCase.DataContract },
                    { "materialContract", materialExact },
                    { "view", view }
                });
                ElementalRaceDevelopmentProbeScenario.DestroyView(_view);
                _view = null;
                _data = null;
                _state = null;
                if (!renderCase.DataContract || !materialExact)
                    throw new InvalidOperationException(renderCase.Label +
                        " did not satisfy its exact doll/material contract.");
                _caseIndex++;
                StartCurrentCase();
            }

            private DollData CreateData(RenderCase renderCase,
                out DollState state)
            {
                state = new DollState();
                state.SetGender(renderCase.Gender);
                state.SetRace(renderCase.Race);
                state.SetRacePreset(renderCase.Preset);
                state.SetClass(renderCase.CharacterClass);
                state.SetHead(renderCase.Head);
                state.SetHair(renderCase.Hair);
                EyebrowsProperty.SetValue(state, renderCase.Eyebrows, null);
                if (renderCase.Beard != null)
                    state.SetBeard(renderCase.Beard);
                if (renderCase.Horn != null)
                    state.SetHorn(renderCase.Horn);

                List<Texture2D> skinRamps = state.GetSkinRamps();
                if (skinRamps == null || skinRamps.Count !=
                        ElementalRaceVisualCatalog.SkinRampCount ||
                    skinRamps.Any(value => !IsNativeRamp(value)))
                    throw new InvalidOperationException(renderCase.Label +
                        " did not expose seven compatible skin ramps.");
                state.SetSkinColor(renderCase.SkinIndex);

                List<Texture2D> hairRamps = state.GetHairRamps();
                renderCase.HairRampCount = hairRamps == null ? 0 :
                    hairRamps.Count;
                renderCase.HairColorIndex = renderCase.HairRampCount == 0 ?
                    -1 : _caseIndex % renderCase.HairRampCount;
                if (renderCase.HairRampCount > 0)
                    state.SetHairColor(renderCase.HairColorIndex);
                else if (!string.Equals(renderCase.Hair.AssetId,
                    ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal))
                    throw new InvalidOperationException(renderCase.Label +
                        " selected visible hair without color ramps.");

                List<Texture2D> hornRamps = state.GetHornsRamps();
                renderCase.HornRampCount = hornRamps == null ? 0 :
                    hornRamps.Count;
                renderCase.HornColorIndex = renderCase.HornRampCount == 0 ?
                    -1 : _caseIndex % renderCase.HornRampCount;
                if (renderCase.HornRampCount > 0)
                    state.SetHornsColor(renderCase.HornColorIndex);
                state.Validate();

                DollData data = state.CreateData();
                renderCase.RequiredEntityIds = RequiredEntityIds(renderCase);
                string[] entityIds = data == null ||
                    data.EquipmentEntityIds == null ? new string[0] :
                    data.EquipmentEntityIds.ToArray();
                string[] missing = renderCase.RequiredEntityIds.Where(
                    value => !entityIds.Contains(value,
                        StringComparer.Ordinal)).ToArray();
                string[] unresolved = entityIds.Where(value =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(value,
                        true) == null).ToArray();
                bool genderExact = data != null &&
                    data.Gender == renderCase.Gender;
                bool presetExact = data != null && ReferenceEquals(
                    data.RacePreset, renderCase.Preset);
                bool raceExact = ReferenceEquals(state.Race,
                    renderCase.Race);
                bool classExact = ReferenceEquals(state.CharacterClass,
                    renderCase.CharacterClass);
                bool headExact = LinkExact(state.Head, renderCase.Head);
                bool hairExact = LinkExact(state.Hair, renderCase.Hair);
                bool eyebrowsExact = LinkExact(state.Eyebrows,
                    renderCase.Eyebrows);
                bool beardExact = LinkExact(state.Beard, renderCase.Beard);
                bool hornExact = LinkExact(state.Horn, renderCase.Horn);
                renderCase.DataContract = genderExact && presetExact &&
                    raceExact && classExact &&
                    renderCase.ClassClothingExact &&
                    headExact && hairExact &&
                    eyebrowsExact && beardExact && hornExact &&
                    data != null && data.EquipmentEntityIds != null &&
                    missing.Length == 0 && unresolved.Length == 0;
                if (!renderCase.DataContract)
                {
                    ((JArray)_evidence["dataFailures"]).Add(new JObject
                    {
                        { "label", renderCase.Label },
                        { "genderExact", genderExact },
                        { "presetExact", presetExact },
                        { "raceExact", raceExact },
                        { "classExact", classExact },
                        { "expectedClassGuid",
                            renderCase.CharacterClass.AssetGuid },
                        { "classClothingExact",
                            renderCase.ClassClothingExact },
                        { "classClothingAssetIds", new JArray(
                            renderCase.ClassClothingAssetIds) },
                        { "head", LinkEvidence(state.Head, renderCase.Head) },
                        { "hair", LinkEvidence(state.Hair, renderCase.Hair) },
                        { "eyebrows", LinkEvidence(state.Eyebrows,
                            renderCase.Eyebrows) },
                        { "beard", LinkEvidence(state.Beard,
                            renderCase.Beard) },
                        { "horn", LinkEvidence(state.Horn,
                            renderCase.Horn) },
                        { "requiredEntityIds", new JArray(
                            renderCase.RequiredEntityIds) },
                        { "entityIds", new JArray(entityIds) },
                        { "missingEntityIds", new JArray(missing) },
                        { "unresolvedEntityIds", new JArray(unresolved) }
                    });
                    throw new InvalidOperationException(renderCase.Label +
                        " did not produce exact production DollData.");
                }
                return data;
            }

            private void CompleteCoverage()
            {
                if (IsClassClothing)
                {
                    CompleteClassClothingCoverage();
                    return;
                }
                _stage = "verify-coverage";
                bool exact = ((JArray)_evidence["renderCases"]).Count ==
                    _cases.Count;
                foreach (IGrouping<string, RenderCase> group in _cases
                    .GroupBy(value => value.Kind + "/" + value.Gender))
                {
                    RenderCase first = group.First();
                    CustomizationOptions options = first.Gender == Gender.Male ?
                        first.Race.MaleOptions : first.Race.FemaleOptions;
                    bool groupExact = group.Select(value =>
                            value.Preset.AssetGuid).Distinct(
                                StringComparer.Ordinal).Count() ==
                            first.Race.Presets.Length &&
                        SetEquals(group.Select(value => value.Head.AssetId),
                            options.Heads.Select(value => value.AssetId)) &&
                        SetEquals(group.Select(value => value.Hair.AssetId),
                            options.Hair.Select(value => value.AssetId)) &&
                        SetEquals(group.Select(value =>
                                value.Eyebrows.AssetId),
                            options.Eyebrows.Select(value => value.AssetId)) &&
                        SetEqualsIgnoringEmpty(group.Select(value =>
                                AssetId(value.Beard)),
                            options.Beards.Select(AssetId)) &&
                        SetEqualsIgnoringEmpty(group.Select(value =>
                                AssetId(value.Horn)),
                            options.Horns.Select(AssetId)) &&
                        group.Select(value => value.SkinIndex).Distinct()
                            .OrderBy(value => value).SequenceEqual(
                                Enumerable.Range(0,
                                    ElementalRaceVisualCatalog.SkinRampCount));
                    int hairColors = group.Where(value =>
                            value.HairRampCount > 0).Select(value =>
                                value.HairColorIndex).Distinct().Count();
                    groupExact &= hairColors >= 4;
                    exact &= groupExact;
                    ((JArray)_evidence["coverage"]).Add(new JObject
                    {
                        { "group", group.Key },
                        { "cases", group.Count() },
                        { "presets", group.Select(value =>
                            value.Preset.AssetGuid).Distinct().Count() },
                        { "heads", group.Select(value =>
                            value.Head.AssetId).Distinct().Count() },
                        { "hair", group.Select(value =>
                            value.Hair.AssetId).Distinct().Count() },
                        { "eyebrows", group.Select(value =>
                            value.Eyebrows.AssetId).Distinct().Count() },
                        { "beards", group.Select(value =>
                            AssetId(value.Beard)).Distinct().Count() },
                        { "horns", group.Select(value =>
                            AssetId(value.Horn)).Distinct().Count() },
                        { "skinIndexes", group.Select(value =>
                            value.SkinIndex).Distinct().Count() },
                        { "hairColorIndexes", hairColors },
                        { "exact", groupExact }
                    });
                }
                Add(_assertions, "elemental-visual-render-matrix",
                    "every planned production race/sex choice renders with complete materials and full option coverage",
                    "planned=" + _cases.Count + ";rendered=" +
                        ((JArray)_evidence["renderCases"]).Count +
                        ";groups=" + ((JArray)_evidence["coverage"]).Count,
                    exact,
                    "native DollState/CreateData/CreateUnitView matrix");
            }

            private void CompleteClassClothingCoverage()
            {
                _stage = "verify-class-clothing-coverage";
                bool exact = _cases.Count == ClassClothingCaseCount &&
                    ((JArray)_evidence["renderCases"]).Count ==
                        ClassClothingCaseCount &&
                    _cases.Select(value => value.Label).Distinct(
                        StringComparer.Ordinal).Count() ==
                            ClassClothingCaseCount &&
                    _cases.All(value => value.DataContract &&
                        value.MaterialContract &&
                        value.ClassClothingExact &&
                        value.ClassClothingAssetIds != null &&
                        value.ClassClothingAssetIds.Length > 0);
                IGrouping<string, RenderCase>[] classGroups = _cases.GroupBy(
                    value => value.ClassKey, StringComparer.Ordinal).ToArray();
                exact &= classGroups.Length == ClassClothingClassCount;
                foreach (IGrouping<string, RenderCase> group in classGroups)
                {
                    bool groupExact = group.Count() ==
                            ElementalRaceCatalog.RaceCount * 2 &&
                        group.Select(value => value.Kind).Distinct().Count() ==
                            ElementalRaceCatalog.RaceCount &&
                        group.Select(value => value.Gender).Distinct()
                            .Count() == 2 &&
                        group.Select(value => value.CharacterClass.AssetGuid)
                            .Distinct(StringComparer.Ordinal).Count() == 1 &&
                        group.All(value => value.DataContract &&
                            value.MaterialContract &&
                            value.ClassClothingExact);
                    exact &= groupExact;
                    RenderCase first = group.First();
                    ((JArray)_evidence["coverage"]).Add(new JObject
                    {
                        { "group", "class/" + group.Key },
                        { "classGuid",
                            first.CharacterClass.AssetGuid },
                        { "cases", group.Count() },
                        { "races", group.Select(value => value.Kind)
                            .Distinct().Count() },
                        { "genders", group.Select(value => value.Gender)
                            .Distinct().Count() },
                        { "exact", groupExact }
                    });
                }
                foreach (IGrouping<string, RenderCase> group in _cases.GroupBy(
                    value => value.Kind + "/" + value.Gender))
                    exact &= group.Count() == ClassClothingClassCount &&
                        group.Select(value => value.ClassKey).Distinct(
                            StringComparer.Ordinal).Count() ==
                                ClassClothingClassCount;
                Add(_assertions,
                    "elemental-class-clothing-render-matrix",
                    "all 80 race/sex/class dolls include exact native class clothing and complete baked materials",
                    "planned=" + _cases.Count + ";rendered=" +
                        ((JArray)_evidence["renderCases"]).Count +
                        ";classGroups=" + classGroups.Length,
                    exact,
                    "race-aware LoadClothes plus native DollState/CreateData/CreateUnitView matrix");
            }

            private void RecordException(Exception exception)
            {
                _exceptionSummary = exception.ToString();
                _warnings.Add((IsClassClothing
                    ? "classClothingExceptionStage="
                    : "visualAuditExceptionStage=") + _stage);
                _diagnostics.Add(exception.ToString());
                Add(_assertions, IsClassClothing
                        ? "elemental-class-clothing-exception"
                        : "elemental-visual-audit-exception",
                    "no exception", "stage=" + _stage + ";" +
                        exception.GetType().FullName + ": " +
                        exception.Message, false,
                    IsClassClothing
                        ? "guarded save-free production class-clothing audit"
                        : "guarded save-free production visual audit");
            }

            private void Finish()
            {
                if (Complete) return;
                _stage = "cleanup";
                ElementalRaceDevelopmentProbeScenario.DestroyView(_view);
                _view = null;
                _data = null;
                _state = null;
                bool rootExact = _root != null && _racesBefore != null &&
                    ReferenceEquals(_root.Progression.CharacterRaces,
                        _racesBefore) && RaceIdentities(
                            _root.Progression.CharacterRaces).SequenceEqual(
                                _raceIdentitiesBefore ?? new string[0],
                                StringComparer.Ordinal);
                bool indexesExact = _library != null &&
                    _library.GetAllBlueprints().Count == _allBefore &&
                    _library.BlueprintsByAssetId.Count == _dictionaryBefore;
                Add(_assertions, IsClassClothing
                        ? "elemental-class-clothing-cleanup"
                        : "elemental-visual-audit-cleanup",
                    "shared race array and blueprint indexes remain reference/content exact",
                    "characterRacesExact=" + rootExact +
                        ";libraryIndexesExact=" + indexesExact,
                    rootExact && indexesExact,
                    "pre/post live graph snapshot");
                Add(_assertions, IsClassClothing
                        ? "elemental-class-clothing-save-free"
                        : "elemental-visual-audit-save-free",
                    "no save, input, party, selector, native asset, or persistent blueprint mutation",
                    "saveStateTouched=false;selectorStateTouched=false;viewsDestroyed=true",
                    true, "guarded mod-load scenario");

                string path = Path.Combine(_request.EvidenceDirectory,
                    IsClassClothing ? ClassClothingEvidenceFileName :
                        EvidenceFileName);
                RuntimeTestResultWriter.WriteAtomic(path,
                    _evidence.ToString(Newtonsoft.Json.Formatting.Indented) +
                        Environment.NewLine);
                _diagnostics.Add((IsClassClothing
                    ? "classClothingEvidenceSha256="
                    : "visualAuditEvidenceSha256=") + Hash(path));
                bool pass = _assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                DateTime ended = DateTime.UtcNow;
                Result = new RuntimeTestResult
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = pass ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName + ";pid=" +
                        Process.GetCurrentProcess().Id,
                    GitCommit = identity.GitCommit,
                    GameVersion = Application.version ?? string.Empty,
                    StartUtc = _started.ToString("o"),
                    EndUtc = ended.ToString("o"),
                    DurationMilliseconds = (long)(ended - _started)
                        .TotalMilliseconds,
                    Assertions = _assertions,
                    Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = new List<string> { path },
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }
        }

        private static ClassClothingDefinition[]
            NativeClassClothingDefinitions()
        {
            return new[]
            {
                new ClassClothingDefinition("fighter",
                    "48ac8db94d5de7645906c7d0ad3bcfbd"),
                new ClassClothingDefinition("rogue",
                    "299aa766dee3cbf4790da4efb8c72484"),
                new ClassClothingDefinition("ranger",
                    "cda0615668a6df14eb36ba19ee881af6"),
                new ClassClothingDefinition("alchemist",
                    "0937bec61c0dabc468428f496580c721"),
                new ClassClothingDefinition("magus",
                    "45a4607686d96a1498891b3286121780"),
                new ClassClothingDefinition("wizard",
                    "ba34257984f4c41408ce1dc2004e342e"),
                new ClassClothingDefinition("cleric",
                    "67819271767a9dd4fbfd4ae700befea0"),
                new ClassClothingDefinition("monk",
                    "e8f21e5b58e0569468e420ebea456124"),
                new ClassClothingDefinition("kineticist",
                    "42a455d9ec1ad924d889272429eb8391")
            };
        }

        private static bool OptionsExact(CustomizationOptions options,
            bool male, ElementalRaceKind kind)
        {
            if (options == null || options.Heads == null ||
                options.Heads.Length < 2 || options.Hair == null ||
                options.Hair.Count(value => !string.Equals(value.AssetId,
                    ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal)) < 4 ||
                options.Eyebrows == null || options.Eyebrows.Length < 1 ||
                options.Beards == null || options.Horns == null ||
                options.TailSkinColors == null ||
                options.TailSkinColors.Length != 0)
                return false;
            if (kind == ElementalRaceKind.Ifrit)
            {
                if (options.Horns.Length != 3 ||
                    !string.Equals(options.Horns[0].AssetId,
                        ElementalRaceVisualCatalog.EmptyAssetId,
                        StringComparison.Ordinal))
                    return false;
            }
            else if (options.Horns.Length != 0) return false;
            if (!male && options.Beards.Length != 0) return false;
            return options.Heads.Concat(options.Hair)
                .Concat(options.Eyebrows).Concat(options.Beards)
                .Concat(options.Horns).All(Resolvable);
        }

        private static bool BodyExact(BlueprintRace race,
            ElementalRaceVisualBlueprints visuals, Gender gender)
        {
            ElementalRaceSexVisualDefinition definition = gender ==
                Gender.Male ? visuals.Definition.Male :
                    visuals.Definition.Female;
            string expected = visuals.Resources.Single(value =>
                string.Equals(value.Spec.Symbol, definition.Body.Symbol,
                    StringComparison.Ordinal)).AssetId;
            EquipmentEntityLink[] links = visuals.Body.GetLinks(gender,
                race.RaceId);
            return links != null && links.Length == 1 &&
                string.Equals(links[0].AssetId, expected,
                    StringComparison.Ordinal) && Resolvable(links[0]);
        }

        private static JObject DescribeOptions(CustomizationOptions options)
        {
            return options == null ? new JObject { { "missing", true } } :
                new JObject
                {
                    { "heads", new JArray(options.Heads.Select(value =>
                        value.AssetId).ToArray()) },
                    { "hair", new JArray(options.Hair.Select(value =>
                        value.AssetId).ToArray()) },
                    { "eyebrows", new JArray(options.Eyebrows.Select(value =>
                        value.AssetId).ToArray()) },
                    { "beards", new JArray(options.Beards.Select(value =>
                        value.AssetId).ToArray()) },
                    { "horns", new JArray(options.Horns.Select(value =>
                        value.AssetId).ToArray()) },
                    { "tailSkinColors", options.TailSkinColors.Length }
                };
        }

        private static string[] RequiredEntityIds(RenderCase renderCase)
        {
            string[] customization = new[]
            {
                renderCase.Head.AssetId,
                renderCase.Hair.AssetId,
                renderCase.Eyebrows.AssetId,
                AssetId(renderCase.Beard),
                AssetId(renderCase.Horn)
            }.Where(value => !string.IsNullOrWhiteSpace(value) &&
                !string.Equals(value, ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal)).ToArray();
            return customization.Distinct(StringComparer.Ordinal).ToArray();
        }

        private static bool LinkExact(EquipmentEntityLink observed,
            EquipmentEntityLink expected)
        {
            if (expected == null) return observed == null ||
                string.IsNullOrWhiteSpace(observed.AssetId);
            return observed != null && string.Equals(observed.AssetId,
                expected.AssetId, StringComparison.Ordinal);
        }

        private static JObject LinkEvidence(EquipmentEntityLink observed,
            EquipmentEntityLink expected)
        {
            return new JObject
            {
                { "expected", AssetId(expected) },
                { "observed", AssetId(observed) },
                { "exact", LinkExact(observed, expected) }
            };
        }

        private static bool Resolvable(EquipmentEntityLink link)
        {
            return link != null && !string.IsNullOrWhiteSpace(link.AssetId) &&
                ResourcesLibrary.TryGetResource<EquipmentEntity>(link.AssetId,
                    true) != null;
        }

        private static bool IsNativeRamp(Texture2D texture)
        {
            return texture != null && texture.width == 256 &&
                texture.height == 1 && texture.format == TextureFormat.RGB24 &&
                texture.filterMode == FilterMode.Bilinear &&
                texture.wrapMode == TextureWrapMode.Clamp;
        }

        private static bool SetEquals(IEnumerable<string> first,
            IEnumerable<string> second)
        {
            return new HashSet<string>(first ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal).SetEquals(second ??
                    Enumerable.Empty<string>());
        }

        private static bool SetEqualsIgnoringEmpty(IEnumerable<string> first,
            IEnumerable<string> second)
        {
            Func<string, bool> isChoice = value =>
                !string.IsNullOrWhiteSpace(value) && !string.Equals(value,
                    ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal);
            return SetEquals((first ?? Enumerable.Empty<string>()).Where(
                    isChoice),
                (second ?? Enumerable.Empty<string>()).Where(isChoice));
        }

        private static string AssetId(EquipmentEntityLink link)
        {
            return link == null ? string.Empty : link.AssetId ?? string.Empty;
        }

        private static PropertyInfo RequireEyebrowsProperty()
        {
            PropertyInfo result = typeof(DollState).GetProperty("Eyebrows",
                BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            MethodInfo setter = result == null ? null :
                result.GetSetMethod(true);
            if (result == null || result.PropertyType !=
                    typeof(EquipmentEntityLink) || setter == null ||
                setter.IsPublic)
                throw new InvalidOperationException(
                    "Kingmaker's private DollState eyebrows contract changed.");
            return result;
        }

        private static string[] RaceIdentities(
            IEnumerable<BlueprintRace> races)
        {
            return (races ?? Enumerable.Empty<BlueprintRace>()).Select(value =>
                value == null ? "<null>" : value.name + "/" +
                    value.AssetGuid + "/" + value.RaceId).ToArray();
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", "").ToLowerInvariant();
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = id,
                Expected = expected,
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
        }
    }
}
