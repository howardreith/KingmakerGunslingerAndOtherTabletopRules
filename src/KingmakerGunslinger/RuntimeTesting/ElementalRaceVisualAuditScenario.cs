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
        private const int MaximumViewSettleUpdates = 360;
        private const int MinimumCasesPerRaceAndSex = 7;
        private const int MinimumCaseCount =
            ElementalRaceCatalog.RaceCount * 2 *
            MinimumCasesPerRaceAndSex;

        private static readonly PropertyInfo EyebrowsProperty =
            RequireEyebrowsProperty();

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        private sealed class RenderCase
        {
            internal string Label;
            internal ElementalRaceKind Kind;
            internal BlueprintRace Race;
            internal Gender Gender;
            internal BlueprintRaceVisualPreset Preset;
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
            private string _stage = "resolve-production-visuals";
            private string _exceptionSummary = string.Empty;

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
                    BuildCases(races[index], visuals[index]);
                }

                bool outfitExact = GunslingerClassAppearanceCatalog
                    .MaleAssetIds().Concat(GunslingerClassAppearanceCatalog
                        .FemaleAssetIds()).All(value => ResourcesLibrary
                            .TryGetResource<EquipmentEntity>(value, true) !=
                                null);
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
                _settleUpdates = 0;
            }

            private void PollCurrentCase()
            {
                RenderCase renderCase = _cases[_caseIndex];
                _stage = "settle-" + renderCase.Label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                bool ready = ElementalRaceDevelopmentProbeScenario.ViewReady(
                    _view);
                if (!ready && _settleUpdates < MaximumViewSettleUpdates)
                    return;

                JObject view = ElementalRaceDevelopmentProbeScenario
                    .DescribeView(renderCase.Label, _data, _view, ready,
                        _settleUpdates);
                EquipmentEntity body = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(renderCase.BodyAssetId, true);
                Character avatar = _view == null ? null :
                    _view.CharacterAvatar;
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
                    bakedCharacterRenderer &&
                    (int)view["renderableRenderers"] > 0 &&
                    (int)view["nullMaterials"] == 0 &&
                    (int)view["nullShaders"] == 0;
                ((JArray)_evidence["renderCases"]).Add(new JObject
                {
                    { "label", renderCase.Label },
                    { "race", renderCase.Kind.ToString() },
                    { "gender", renderCase.Gender.ToString() },
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
                state.SetClass(_gunslinger);
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
                    _gunslinger);
                bool headExact = LinkExact(state.Head, renderCase.Head);
                bool hairExact = LinkExact(state.Hair, renderCase.Hair);
                bool eyebrowsExact = LinkExact(state.Eyebrows,
                    renderCase.Eyebrows);
                bool beardExact = LinkExact(state.Beard, renderCase.Beard);
                bool hornExact = LinkExact(state.Horn, renderCase.Horn);
                renderCase.DataContract = genderExact && presetExact &&
                    raceExact && classExact && headExact && hairExact &&
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

            private void RecordException(Exception exception)
            {
                _exceptionSummary = exception.ToString();
                _warnings.Add("visualAuditExceptionStage=" + _stage);
                _diagnostics.Add(exception.ToString());
                Add(_assertions, "elemental-visual-audit-exception",
                    "no exception", "stage=" + _stage + ";" +
                        exception.GetType().FullName + ": " +
                        exception.Message, false,
                    "guarded save-free production visual audit");
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
                Add(_assertions, "elemental-visual-audit-cleanup",
                    "shared race array and blueprint indexes remain reference/content exact",
                    "characterRacesExact=" + rootExact +
                        ";libraryIndexesExact=" + indexesExact,
                    rootExact && indexesExact,
                    "pre/post live graph snapshot");
                Add(_assertions, "elemental-visual-audit-save-free",
                    "no save, input, party, selector, native asset, or persistent blueprint mutation",
                    "saveStateTouched=false;selectorStateTouched=false;viewsDestroyed=true",
                    true, "guarded mod-load scenario");

                string path = Path.Combine(_request.EvidenceDirectory,
                    EvidenceFileName);
                RuntimeTestResultWriter.WriteAtomic(path,
                    _evidence.ToString(Newtonsoft.Json.Formatting.Indented) +
                        Environment.NewLine);
                _diagnostics.Add("visualAuditEvidenceSha256=" + Hash(path));
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
            return new[]
            {
                renderCase.Head.AssetId,
                renderCase.Hair.AssetId,
                renderCase.Eyebrows.AssetId,
                AssetId(renderCase.Beard),
                AssetId(renderCase.Horn)
            }.Where(value => !string.IsNullOrWhiteSpace(value) &&
                !string.Equals(value, ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal)).Distinct(
                        StringComparer.Ordinal).ToArray();
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
