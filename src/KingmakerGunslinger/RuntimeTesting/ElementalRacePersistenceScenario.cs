using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.ElementalRaces.Visuals;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded three-launch save qualification for all production elemental
    /// race identities. Prepare persists eight exact disposable characters
    /// with spent racial SLAs. The second launch runs with selector publication
    /// disabled, verifies native reconstruction, rest and level-up behavior,
    /// removes only the marker-bound fixtures, and saves cleanup. The final
    /// launch proves their absence. KMG_AUTOMATION_BASELINE is never eligible.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string ElementalPersistenceFixtureNamePrefix =
            "KMG_ELEMENTAL_PERSISTENCE_";
        private const int ElementalPersistenceFixtureCount =
            ElementalRaceCatalog.RaceCount * 2;
        private const int ElementalPersistencePartyCount =
            WorkingSaveSmokeScenario.ExpectedPartyCount +
            ElementalPersistenceFixtureCount;
        private static readonly string[] ElementalPersistenceFixtureIds =
        {
            "a9be3b86-9d80-472a-93e6-71fcfb3a827a",
            "2fc7d5a4-5dab-4bb9-bee1-da1fdfa2a337",
            "f4933068-5824-46fa-a330-25b78764503e",
            "27a98188-4106-419d-8897-64ccd6f63305",
            "d532ec12-a328-4afb-8cbf-7f3ddf41f072",
            "08e1cd1d-4512-4c52-a9fa-6dd8d815499a",
            "043d4fc2-c26c-4e72-9d11-219d0ff74b43",
            "91472289-c1d7-4558-b7ed-a5e8c06345fb"
        };

        internal static ElementalRacePersistenceSession
            BeginElementalRacePersistence(ModContext context,
                RuntimeTestRequest request,
                WorkingSaveSmokeScenario workingSaveSmoke)
        {
            return new ElementalRacePersistenceSession(context, request,
                workingSaveSmoke);
        }

        private sealed class ElementalPersistenceFixture
        {
            internal ElementalPersistenceFixture(int index,
                ElementalRaceBlueprints blueprints, Gender gender,
                BlueprintRaceVisualPreset preset)
            {
                Index = index;
                Blueprints = blueprints ??
                    throw new ArgumentNullException("blueprints");
                Gender = gender;
                Preset = preset ?? throw new ArgumentNullException("preset");
                Label = blueprints.Definition.Kind.ToString()
                    .ToLowerInvariant() + "-" + gender.ToString()
                    .ToLowerInvariant();
                UniqueId = ElementalPersistenceFixtureIds[index];
                Name = ElementalPersistenceFixtureNamePrefix +
                    Label.Replace('-', '_').ToUpperInvariant();
            }

            internal int Index { get; private set; }
            internal ElementalRaceBlueprints Blueprints { get; private set; }
            internal Gender Gender { get; private set; }
            internal BlueprintRaceVisualPreset Preset { get; private set; }
            internal string Label { get; private set; }
            internal string UniqueId { get; private set; }
            internal string Name { get; private set; }
        }

        private sealed class ElementalPersistenceObservation
        {
            internal bool RaceExact;
            internal bool FactsExact;
            internal bool StatsExact;
            internal bool ResourceExact;
            internal bool AbilityExact;
            internal bool DollExact;
            internal bool AppearanceExact;
            internal JObject Evidence;

            internal bool Exact
            {
                get
                {
                    return RaceExact && FactsExact && StatsExact &&
                        ResourceExact && AbilityExact && DollExact &&
                        AppearanceExact;
                }
            }
        }

        internal sealed class ElementalRacePersistenceSession
        {
            private const int MinimumSettleUpdates = 30;
            private const int MaximumSettleUpdates = 480;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly WorkingSaveSmokeScenario _workingSaveSmoke;
            private readonly bool _prepare;
            private readonly bool _verifyAbsent;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics =
                new List<string>();
            private readonly List<string> _warnings =
                new List<string>();
            private readonly List<string> _evidenceFiles =
                new List<string>();
            private readonly List<UnitEntityData> _createdUnits =
                new List<UnitEntityData>();
            private readonly List<BlueprintUnit> _createdBlueprints =
                new List<BlueprintUnit>();
            private readonly JArray _records = new JArray();
            private readonly JArray _partyRecords = new JArray();
            private JObject _loadedFixtureMembership = new JObject();
            private Player _player;
            private object _allUnits;
            private object _party;
            private object _remote;
            private object _cross;
            private object _inventory;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private object[] _partyCharactersBefore = new object[0];
            private object[] _remoteBefore = new object[0];
            private object[] _crossBefore = new object[0];
            private object[] _inventoryBefore = new object[0];
            private long _moneyBefore;
            private BlueprintRace[] _characterRacesBefore =
                new BlueprintRace[0];
            private UnitEntityData _anchor;
            private BlueprintCharacterClass _gunslingerClass;
            private BlueprintItem[] _startingItems = new BlueprintItem[0];
            private int[] _startingItemCounts = new int[0];
            private int _startingGoldBefore;
            private ElementalRaceBlueprintSet _blueprintSet;
            private ElementalPersistenceFixture[] _fixtures =
                new ElementalPersistenceFixture[0];
            private UnitEntityData[] _loadedUnits = new UnitEntityData[0];
            private UnitEntityData _currentUnit;
            private BlueprintUnit _currentBlueprint;
            private PersistenceDollSnapshot _currentExpectedDoll;
            private bool _currentLoadedDollExact;
            private int _fixtureIndex;
            private int _phase;
            private int _settleUpdates;
            private int _captured;
            private int _imageCount;
            private int _viewCount;
            private bool _registeredExact;
            private bool _selectorExact;
            private bool _preparedMembershipExact;
            private bool _normalPathComplete;
            private bool _baselineAbsentExact;
            private bool _cleanupStarted;
            private bool _structuralCleaned;
            private bool _indexWritten;
            private bool _saveStarted;
            private bool _saveCompleted;
            private Stopwatch _saveElapsed;
            private WorkingSaveSmokeEvidence _workingSaveEvidence;
            private string _gameAssemblySha256 = string.Empty;
            private string _gameAssemblyMvid = string.Empty;
            private string _stage = "initialize";
            private string _exceptionSummary = string.Empty;

            internal ElementalRacePersistenceSession(ModContext context,
                RuntimeTestRequest request,
                WorkingSaveSmokeScenario workingSaveSmoke)
            {
                _context = context ?? throw new ArgumentNullException("context");
                _request = request ?? throw new ArgumentNullException("request");
                _workingSaveSmoke = workingSaveSmoke ??
                    throw new ArgumentNullException("workingSaveSmoke");
                _prepare = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .ElementalRacePersistencePrepare,
                    StringComparison.Ordinal);
                _verifyAbsent = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .ElementalRacePersistenceVerifyAbsent,
                    StringComparison.Ordinal);
            }

            internal bool Complete { get; private set; }
            internal RuntimeTestResult Result { get; private set; }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_saveStarted)
                    {
                        PollExactWorkingSave();
                        return;
                    }
                    if (_cleanupStarted)
                    {
                        PollCleanup();
                        return;
                    }
                    if (_phase == 0)
                    {
                        Initialize();
                        if (_verifyAbsent)
                        {
                            _normalPathComplete = _baselineAbsentExact;
                            BeginCleanup();
                            return;
                        }
                        _phase = 1;
                        return;
                    }
                    if (_phase == 1)
                    {
                        if (!ResourcesReady()) return;
                        if (_prepare) StartPrepareFixture();
                        else StartVerifyFixture();
                        _phase = 2;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 2)
                    {
                        if (!PollCurrentReady()) return;
                        if (_prepare) CapturePreparedFixture();
                        else CaptureVerifiedFixture();
                        _fixtureIndex++;
                        if (_fixtureIndex < _fixtures.Length)
                        {
                            _phase = 1;
                            _settleUpdates = 0;
                            return;
                        }
                        if (_prepare)
                        {
                            _preparedMembershipExact =
                                PreparedMembershipExact();
                            if (!_preparedMembershipExact)
                                throw new InvalidOperationException(
                                    "The eight exact elemental fixtures did not enter one serializable party and area state.");
                            _normalPathComplete = true;
                            StartExactWorkingSave();
                            return;
                        }
                        _normalPathComplete = true;
                        BeginCleanup();
                    }
                }
                catch (Exception exception)
                {
                    RecordException(exception);
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
                _stage = "validate-guard-and-snapshot";
                if (!string.Equals(SaveName(),
                        WorkingSaveSmokeScenario.ExpectedName,
                        StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Elemental persistence requires the exact disposable working save.");
                if (!RuntimeTestScenarioCatalog
                        .IsElementalRacePersistenceScenario(
                            _request.Scenario))
                    throw new InvalidOperationException(
                        "Elemental persistence scenario identity is not exact.");

                _player = Game.Instance.Player;
                _allUnits = Game.Instance.State.Units.All;
                _party = _player.Party;
                _remote = _player.RemoteCompanions;
                _cross = _player.CrossSceneState.AllEntityData;
                _inventory = _player.Inventory;
                _unitsBefore = Snapshot(_allUnits);
                _partyBefore = Snapshot(_party);
                _partyCharactersBefore = _player.PartyCharacters
                    .Cast<object>().ToArray();
                _remoteBefore = Snapshot(_remote);
                _crossBefore = Snapshot(_cross);
                _inventoryBefore = Snapshot(_inventory);
                _moneyBefore = _player.Money;
                _characterRacesBefore = BlueprintRoot.Instance.Progression
                    .CharacterRaces.ToArray();
                _anchor = _partyBefore.OfType<UnitEntityData>()
                    .FirstOrDefault(value => value != null &&
                        value.View != null && value.HoldingState != null);
                if (_anchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party-area anchor.");

                _blueprintSet = BlueprintBootstrap.ElementalRaces;
                if (_blueprintSet == null || _blueprintSet.Count !=
                        ElementalRaceIdentityCatalog.IdentityCount)
                    throw new InvalidOperationException(
                        "The complete registered elemental blueprint set is unavailable.");
                _fixtures = BuildFixtures();
                _gunslingerClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        OutfitProductionClassGuid,
                        "elemental persistence Gunslinger class");
                _startingItems = _gunslingerClass.StartingItems ??
                    new BlueprintItem[0];
                _startingItemCounts = _startingItems.Select(value =>
                    _player.Inventory.Count(value)).ToArray();
                _startingGoldBefore = _gunslingerClass.StartingGold;
                _gunslingerClass.StartingGold = 0;

                _registeredExact = RegisteredIdentitiesExact();
                _selectorExact = SelectorStateExact();
                Assembly assembly = typeof(BlueprintCharacterClass).Assembly;
                _gameAssemblySha256 = HashFile(assembly.Location)
                    .ToLowerInvariant();
                _gameAssemblyMvid = assembly.ManifestModule
                    .ModuleVersionId.ToString("D");
                RecordPartySnapshot();
                _loadedFixtureMembership = DescribeFixtureMembership();

                bool baselineShape = BaselineShapeExact();
                if (_prepare)
                {
                    if (!_context.FeatureModules.Active.ElementalRaces ||
                        !_selectorExact || !_registeredExact || !baselineShape)
                        throw new InvalidOperationException(
                            "Persistence prepare requires Elemental Races ON, four exact selector entries, all identities registered, and the clean three-character baseline.");
                    WriteProgress("initialized-prepare");
                    return;
                }
                if (_verifyAbsent)
                {
                    _baselineAbsentExact = baselineShape &&
                        _registeredExact && _selectorExact;
                    if (!_baselineAbsentExact)
                        throw new InvalidOperationException(
                            "Fresh-load absence verification found fixture residue, identity drift, selector drift, or a changed working-save baseline.");
                    WriteProgress("initialized-verify-absent");
                    return;
                }

                if (_context.FeatureModules.Active.ElementalRaces ||
                    !_selectorExact || !_registeredExact)
                    throw new InvalidOperationException(
                        "Module-disabled verification requires all elemental identities registered while all four selector entries are absent.");
                _loadedUnits = ResolveLoadedFixtures();
                if (!LoadedMembershipExact())
                    throw new InvalidOperationException(
                        "Fresh-load module-disabled verification requires eight exact marker-bound elemental party fixtures; observed " +
                        _loadedFixtureMembership.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                WriteProgress("initialized-module-disabled-verify-cleanup");
            }

            private ElementalPersistenceFixture[] BuildFixtures()
            {
                if (ElementalPersistenceFixtureIds.Length !=
                        ElementalPersistenceFixtureCount ||
                    ElementalPersistenceFixtureIds.Distinct(
                        StringComparer.Ordinal).Count() !=
                        ElementalPersistenceFixtureCount ||
                    ElementalPersistenceFixtureIds.Any(value =>
                        !Guid.TryParse(value, out _)))
                    throw new InvalidOperationException(
                        "The elemental persistence fixture identity catalog drifted.");
                var result = new List<ElementalPersistenceFixture>();
                ElementalRaceBlueprints[] races = _blueprintSet
                    .OrderedBlueprints().ToArray();
                for (int raceIndex = 0; raceIndex < races.Length; raceIndex++)
                {
                    ElementalRaceBlueprints race = races[raceIndex];
                    if (race.Race.Presets == null ||
                        race.Race.Presets.Length != 3)
                        throw new InvalidOperationException(
                            race.Definition.DisplayName +
                            " does not expose the exact three production presets.");
                    Gender[] genders = { Gender.Male, Gender.Female };
                    for (int genderIndex = 0; genderIndex < genders.Length;
                        genderIndex++)
                    {
                        int index = result.Count;
                        BlueprintRaceVisualPreset preset = race.Race.Presets[
                            (raceIndex * 2 + genderIndex) %
                            race.Race.Presets.Length];
                        if (preset == null || preset.Skin == null ||
                            (genders[genderIndex] == Gender.Male
                                ? preset.MaleSkeleton == null
                                : preset.FemaleSkeleton == null))
                            throw new InvalidOperationException(
                                race.Definition.DisplayName + "/" +
                                genders[genderIndex] +
                                " production preset is incomplete.");
                        result.Add(new ElementalPersistenceFixture(index,
                            race, genders[genderIndex], preset));
                    }
                }
                if (result.Count != ElementalPersistenceFixtureCount)
                    throw new InvalidOperationException(
                        "Elemental persistence requires exactly eight race/sex fixtures.");
                return result.ToArray();
            }

            private DollData CreateExpectedDollData(
                ElementalPersistenceFixture fixture)
            {
                CustomizationOptions options = fixture.Gender == Gender.Male
                    ? fixture.Blueprints.Race.MaleOptions
                    : fixture.Blueprints.Race.FemaleOptions;
                if (options == null || options.Heads == null ||
                    options.Hair == null || options.Eyebrows == null ||
                    options.Beards == null || options.Horns == null)
                    throw new InvalidOperationException(fixture.Label +
                        " customization options are incomplete.");

                EquipmentEntityLink head = SelectRequiredOption(
                    options.Heads, fixture.Index, fixture.Label + " head");
                EquipmentEntityLink hair = SelectRequiredOption(
                    options.Hair, fixture.Index + 1,
                    fixture.Label + " hair");
                EquipmentEntityLink eyebrows = SelectPairedOption(
                    options.Heads, head, options.Eyebrows,
                    fixture.Label + " eyebrows");
                EquipmentEntityLink beard = fixture.Gender == Gender.Male
                    ? SelectOptionalOption(options.Beards, fixture.Index)
                    : null;
                EquipmentEntityLink horn = fixture.Blueprints.Definition.Kind
                        == ElementalRaceKind.Ifrit
                    ? SelectRequiredOption(options.Horns,
                        fixture.Index + 1, fixture.Label + " horn")
                    : null;

                var state = new DollState();
                state.SetGender(fixture.Gender);
                state.SetRace(fixture.Blueprints.Race);
                state.SetRacePreset(fixture.Preset);
                state.SetClass(_gunslingerClass);
                state.SetHead(head);
                state.SetHair(hair);
                SetEyebrows(state, eyebrows);
                if (beard != null) state.SetBeard(beard);
                if (horn != null) state.SetHorn(horn);

                List<Texture2D> skinRamps = state.GetSkinRamps();
                if (skinRamps == null || skinRamps.Count !=
                        ElementalRaceVisualCatalog.SkinRampCount)
                    throw new InvalidOperationException(fixture.Label +
                        " does not expose the exact seven-ramp skin palette.");
                state.SetSkinColor(fixture.Index % skinRamps.Count);
                List<Texture2D> hairRamps = state.GetHairRamps();
                if (hairRamps == null || hairRamps.Count < 4)
                    throw new InvalidOperationException(fixture.Label +
                        " does not expose four visible hair colors.");
                state.SetHairColor(fixture.Index % hairRamps.Count);
                if (horn != null)
                {
                    List<Texture2D> hornRamps = state.GetHornsRamps();
                    if (hornRamps == null || hornRamps.Count == 0)
                        throw new InvalidOperationException(fixture.Label +
                            " selected horns without a native color ramp.");
                    state.SetHornsColor(fixture.Index % hornRamps.Count);
                }
                state.Validate();
                DollData data = state.CreateData();
                string[] required = new[]
                {
                    head.AssetId, hair.AssetId, eyebrows.AssetId,
                    beard == null ? string.Empty : beard.AssetId,
                    horn == null ? string.Empty : horn.AssetId
                }.Where(value => !string.IsNullOrWhiteSpace(value) &&
                    !string.Equals(value,
                        ElementalRaceVisualCatalog.EmptyAssetId,
                        StringComparison.Ordinal)).Distinct(
                            StringComparer.Ordinal).ToArray();
                string[] dataIds = data == null ||
                        data.EquipmentEntityIds == null
                    ? new string[0] : data.EquipmentEntityIds.ToArray();
                string[] missing = required.Where(value =>
                    !dataIds.Contains(value, StringComparer.Ordinal)).ToArray();
                string[] unresolved = dataIds.Where(value =>
                    !string.IsNullOrWhiteSpace(value) &&
                    !string.Equals(value,
                        ElementalRaceVisualCatalog.EmptyAssetId,
                        StringComparison.Ordinal) &&
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        value, true) == null).ToArray();
                bool exact = data != null &&
                    data.Gender == fixture.Gender &&
                    ReferenceEquals(data.RacePreset, fixture.Preset) &&
                    data.EquipmentEntityIds != null &&
                    missing.Length == 0 && unresolved.Length == 0 &&
                    data.ClothesPrimaryIndex ==
                        GunslingerClassAppearanceCatalog.DefaultPrimaryColor &&
                    data.ClothesSecondaryIndex ==
                        GunslingerClassAppearanceCatalog.DefaultSecondaryColor;
                if (!exact)
                    throw new InvalidOperationException(fixture.Label +
                        " did not produce deterministic production Gunslinger DollData: " +
                        "data=" + (data != null) + ";gender=" +
                        (data == null ? "<null>" : data.Gender.ToString()) +
                        ";presetExact=" + (data != null &&
                            ReferenceEquals(data.RacePreset, fixture.Preset)) +
                        ";missing=" + string.Join(",", missing) +
                        ";unresolved=" + string.Join(",", unresolved) +
                        ";clothes=" + (data == null ? "<null>" :
                            data.ClothesPrimaryIndex + "/" +
                            data.ClothesSecondaryIndex) + ".");
                return data;
            }

            private static EquipmentEntityLink SelectRequiredOption(
                IEnumerable<EquipmentEntityLink> values, int index,
                string label)
            {
                EquipmentEntityLink[] candidates = (values ??
                        Enumerable.Empty<EquipmentEntityLink>())
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId) &&
                        !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal))
                    .OrderBy(value => value.AssetId,
                        StringComparer.Ordinal).ToArray();
                if (candidates.Length == 0)
                    throw new InvalidOperationException(label +
                        " has no resolvable nonempty option.");
                EquipmentEntityLink result = candidates[index %
                    candidates.Length];
                if (ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        result.AssetId, true) == null)
                    throw new InvalidOperationException(label +
                        " did not resolve: " + result.AssetId + ".");
                return result;
            }

            private static EquipmentEntityLink SelectOptionalOption(
                IEnumerable<EquipmentEntityLink> values, int index)
            {
                EquipmentEntityLink[] candidates = (values ??
                        Enumerable.Empty<EquipmentEntityLink>())
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId) &&
                        !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal) &&
                        ResourcesLibrary.TryGetResource<EquipmentEntity>(
                            value.AssetId, true) != null)
                    .OrderBy(value => value.AssetId,
                        StringComparer.Ordinal).ToArray();
                return candidates.Length == 0 ? null :
                    candidates[index % candidates.Length];
            }

            private static EquipmentEntityLink SelectPairedOption(
                IEnumerable<EquipmentEntityLink> referenceValues,
                EquipmentEntityLink selectedReference,
                IEnumerable<EquipmentEntityLink> values, string label)
            {
                EquipmentEntityLink[] references = (referenceValues ??
                        Enumerable.Empty<EquipmentEntityLink>())
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId) &&
                        !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal)).ToArray();
                int index = Array.FindIndex(references, value =>
                    string.Equals(value.AssetId, selectedReference.AssetId,
                        StringComparison.Ordinal));
                EquipmentEntityLink[] candidates = (values ??
                        Enumerable.Empty<EquipmentEntityLink>())
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId) &&
                        !string.Equals(value.AssetId,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal)).ToArray();
                if (index < 0 || index >= candidates.Length)
                    throw new InvalidOperationException(label +
                        " does not have a production option paired with " +
                        selectedReference.AssetId + ".");
                EquipmentEntityLink result = candidates[index];
                if (ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        result.AssetId, true) == null)
                    throw new InvalidOperationException(label +
                        " did not resolve: " + result.AssetId + ".");
                return result;
            }

            private static void SetEyebrows(DollState state,
                EquipmentEntityLink eyebrows)
            {
                PropertyInfo property = typeof(DollState).GetProperty(
                    "Eyebrows", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                MethodInfo setter = property == null ? null :
                    property.GetSetMethod(true);
                if (setter == null || property.PropertyType !=
                        typeof(EquipmentEntityLink))
                    throw new MissingMemberException(
                        typeof(DollState).FullName,
                        "Eyebrows : EquipmentEntityLink");
                setter.Invoke(state, new object[] { eyebrows });
            }

            private void StartPrepareFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "create-prepared-" + fixture.Label;
                DollData data = CreateExpectedDollData(fixture);
                _currentExpectedDoll = PersistenceDollSnapshot.Capture(data);
                BlueprintUnit source = BlueprintRoot.Instance
                    .DefaultPlayerCharacter;
                if (source == null || source.Prefab == null ||
                    source.Body == null)
                    throw new InvalidOperationException(
                        "The native default player donor is incomplete.");
                _currentBlueprint = UnityEngine.Object.Instantiate(source);
                _currentBlueprint.Gender = fixture.Gender;
                _currentBlueprint.Body = CreateElementalNeutralBody(source);
                _currentBlueprint.StartingInventory = new BlueprintItem[0];
                _currentBlueprint.name =
                    "KMG_Runtime_Elemental_Persistence_" +
                    fixture.Label.Replace('-', '_');
                _currentBlueprint.IsCheater = false;
                _createdBlueprints.Add(_currentBlueprint);

                UnitEntityView dollView = null;
                try
                {
                    dollView = data.CreateUnitView(false);
                    if (dollView == null ||
                        dollView.GetComponent<Character>() == null)
                        throw new InvalidOperationException(fixture.Label +
                            " DollData did not create a native Character view.");
                    dollView.Blueprint = _currentBlueprint;
                    dollView.UniqueId = fixture.UniqueId;
                    float column = _fixtureIndex % 4;
                    float row = _fixtureIndex / 4;
                    dollView.transform.position = NearestNavigable(
                        _anchor.Position + new Vector3(-5f + column * 2f,
                            0f, 3.5f + row * 2f));
                    dollView.transform.rotation = Quaternion.identity;
                    _currentUnit = Game.Instance.EntityCreator
                        .SpawnEntityWithView(dollView,
                            _anchor.HoldingState) as UnitEntityData;
                    if (_currentUnit == null ||
                        !ReferenceEquals(_currentUnit.View, dollView))
                        throw new InvalidOperationException(fixture.Label +
                            " DollData view ownership transfer failed.");
                    dollView = null;
                }
                finally
                {
                    if (dollView != null)
                        UnityEngine.Object.DestroyImmediate(
                            dollView.gameObject);
                }
                _createdUnits.Add(_currentUnit);
                PrepareBaseStats(_currentUnit.Descriptor);
                _currentUnit.Descriptor.CustomGender = fixture.Gender;
                _currentUnit.Descriptor.Doll = data;
                _currentUnit.Descriptor.ForcceUseClassEquipment = true;
                _currentUnit.Descriptor.CustomName = fixture.Name;
                _currentUnit.Descriptor.State.Immortality.Retain();
                _currentUnit.Commands.InterruptAll(true);
                if (_currentUnit.CombatState.IsInCombat)
                    _currentUnit.CombatState.LeaveCombat();
                ApplyNativeCharacterCreation(fixture, data);
                _currentBlueprint.Race = fixture.Blueprints.Race;
                _currentUnit.IsInGame = true;
                _currentUnit.IsInFogOfWar = false;
                _currentUnit.View.UpdateClassEquipment();
                CurrentAvatar().RebuildOutfit();
                _currentUnit.View.UpdateViewActive();
                _currentUnit.View.SetVisible(true, true);
                WriteProgress("prepared-fixture-created");
            }

            private void ApplyNativeCharacterCreation(
                ElementalPersistenceFixture fixture, DollData data)
            {
                _stage = "native-character-creation-" + fixture.Label;
                LevelUpController controller = null;
                try
                {
                    controller = LevelUpController
                        .StartWithoutAssigningStaticInstance(
                            _currentUnit.Descriptor, false, null, null,
                            LevelUpState.CharBuildMode.CharGen);
                    if (controller == null || controller.Preview == null)
                        throw new InvalidOperationException(fixture.Label +
                            " native character-creation controller is unavailable.");
                    if (ReferenceEquals(controller.Preview.Progression.Race,
                            fixture.Blueprints.Race))
                        throw new InvalidOperationException(fixture.Label +
                            " native character creation did not begin from the default donor race.");
                    if (!controller.SelectRace(fixture.Blueprints.Race))
                        throw new InvalidOperationException(fixture.Label +
                            " native race selection was rejected.");
                    if (!controller.SelectClass(_gunslingerClass, false))
                        throw new InvalidOperationException(fixture.Label +
                            " native Gunslinger selection was rejected.");
                    controller.ApplyClassMechanics();
                    MethodInfo apply = typeof(LevelUpController).GetMethod(
                        "ApplyLevelup", BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                    if (apply == null)
                        throw new MissingMethodException(
                            typeof(LevelUpController).FullName,
                            "ApplyLevelup(UnitDescriptor)");
                    apply.Invoke(controller,
                        new object[] { _currentUnit.Descriptor });
                    controller.Cancel();
                    controller = null;
                }
                finally
                {
                    if (controller != null) controller.Cancel();
                }
                _currentUnit.Descriptor.Doll = data;
                _currentUnit.Descriptor.ForcceUseClassEquipment = true;
                if (!RollbackStarterGrants())
                    throw new InvalidOperationException(fixture.Label +
                        " native character creation changed starting inventory.");
                if (_currentUnit.Descriptor.Progression.CharacterLevel != 1 ||
                    _currentUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) != 1 ||
                    !ReferenceEquals(_currentUnit.Descriptor.Progression.Race,
                        fixture.Blueprints.Race) ||
                    _currentUnit.Descriptor.Progression.Features.GetRank(
                        fixture.Blueprints.Race) != 1)
                    throw new InvalidOperationException(fixture.Label +
                        " native character creation did not commit exact race/class progression.");
            }

            private static void PrepareBaseStats(UnitDescriptor owner)
            {
                owner.Stats.Strength.BaseValue = 10;
                owner.Stats.Dexterity.BaseValue = 10;
                owner.Stats.Constitution.BaseValue = 10;
                owner.Stats.Intelligence.BaseValue = 10;
                owner.Stats.Wisdom.BaseValue = 10;
                owner.Stats.Charisma.BaseValue = 10;
                owner.Stats.HitPoints.BaseValue = 100;
            }

            private static BlueprintUnit.UnitBody CreateElementalNeutralBody(
                BlueprintUnit source)
            {
                return new BlueprintUnit.UnitBody
                {
                    DisableHands = false,
                    EmptyHandWeapon = source.Body.EmptyHandWeapon,
                    AdditionalLimbs = new BlueprintItemWeapon[0],
                    AdditionalSecondaryLimbs =
                        new BlueprintItemWeapon[0],
                    QuickSlots = new BlueprintItemEquipmentUsable[0]
                };
            }

            private bool ResourcesReady()
            {
                _stage = "wait-elemental-persistence-resources";
                if (!ResourcesLibrary.Preloading) return true;
                _settleUpdates++;
                if (_settleUpdates < MaximumSettleUpdates) return false;
                throw new InvalidOperationException(
                    "Elemental persistence resource preloading did not finish.");
            }

            private bool PollCurrentReady()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "settle-" + fixture.Label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                if (_currentUnit != null && _currentUnit.View != null &&
                    _currentUnit.View.AnimationManager != null)
                    _currentUnit.View.AnimationManager.Tick();
                Character avatar = CurrentAvatarOrNull();
                bool ready = _currentUnit != null &&
                    _currentUnit.View != null &&
                    ReferenceEquals(_currentUnit.View.Data, _currentUnit) &&
                    avatar != null &&
                    HasExactHumanoidRig(_currentUnit.View.transform) &&
                    ActiveRenderers(_currentUnit).Length > 0;
                if (ready && _settleUpdates >= MinimumSettleUpdates)
                    return true;
                if (_settleUpdates < MaximumSettleUpdates) return false;
                throw new InvalidOperationException(fixture.Label +
                    " did not settle to a rendered humanoid view; updates=" +
                    _settleUpdates + ".");
            }

            private Character CurrentAvatarOrNull()
            {
                return _currentUnit == null || _currentUnit.View == null
                    ? null : _currentUnit.View.GetComponent<Character>();
            }

            private Character CurrentAvatar()
            {
                Character result = CurrentAvatarOrNull();
                if (result == null)
                    throw new InvalidOperationException(
                        "The current elemental persistence fixture has no attached Character.");
                return result;
            }

            private void CapturePreparedFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "capture-prepared-" + fixture.Label;
                ElementalPersistenceObservation observation = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll, 1, 1);
                AbilityData ability = RequireAbility(_currentUnit,
                    fixture.Blueprints.SlaAbility);
                int before = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Blueprints.SlaResource);
                InvokeAbilitySpend(ability,
                    fixture.Blueprints.SlaResource);
                int after = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Blueprints.SlaResource);
                bool spendExact = before == 1 && after == 0 &&
                    ability.GetAvailableForCastCount() == 0 &&
                    !ability.IsAvailable;
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "phase", "prepare" },
                    { "observation", observation.Evidence },
                    { "observationExact", observation.Exact },
                    { "resourceBeforeSpend", before },
                    { "resourceAfterSpend", after },
                    { "spendExact", spendExact }
                };
                CaptureFixture(record, fixture, _currentUnit);
                if (!observation.Exact || !spendExact)
                    throw new InvalidOperationException(fixture.Label +
                        " did not satisfy the exact pre-save rules, visual, and spent-SLA contract.");
                PromoteCurrentFixture(fixture);
                WriteProgress("prepared-fixture-promoted");
            }

            private void PromoteCurrentFixture(
                ElementalPersistenceFixture fixture)
            {
                if (_player.PartyCharacters.Any(value => string.Equals(
                        value.UniqueId, fixture.UniqueId,
                        StringComparison.Ordinal)))
                    throw new InvalidOperationException(fixture.Label +
                        " persistence reference already exists.");
                _currentUnit.Descriptor.State.Immortality.ReleaseAll();
                UnitReference reference = _currentUnit;
                _player.PartyCharacters.Add(reference);
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();
                if (!_player.Party.Contains(_currentUnit) ||
                    !ContainsReference(_allUnits, _currentUnit) ||
                    _currentUnit.HoldingState == null ||
                    !_currentUnit.HoldingState.AllEntityData.Any(value =>
                        ReferenceEquals(value, _currentUnit)) ||
                    _player.PartyCharacters.Count(value => string.Equals(
                        value.UniqueId, fixture.UniqueId,
                        StringComparison.Ordinal)) != 1)
                    throw new InvalidOperationException(fixture.Label +
                        " did not enter one exact serializable party and scene state.");
                _currentUnit = null;
                _currentBlueprint = null;
                _currentExpectedDoll = null;
                _currentLoadedDollExact = false;
            }

            private void StartVerifyFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "activate-loaded-" + fixture.Label;
                _currentUnit = _loadedUnits[_fixtureIndex];
                DollData expected = CreateExpectedDollData(fixture);
                _currentExpectedDoll = PersistenceDollSnapshot.Capture(
                    expected);
                _currentLoadedDollExact = _currentUnit != null &&
                    _currentUnit.Descriptor != null &&
                    _currentExpectedDoll.Matches(
                        _currentUnit.Descriptor.Doll);
                if (_currentUnit == null || _currentUnit.View == null ||
                    CurrentAvatarOrNull() == null)
                    throw new InvalidOperationException(fixture.Label +
                        " did not deserialize with one attached native Character view.");
                _currentUnit.IsInGame = true;
                _currentUnit.IsInFogOfWar = false;
                _currentUnit.View.UpdateViewActive();
                _currentUnit.View.SetVisible(true, true);
                _currentUnit.View.UpdateClassEquipment();
                CurrentAvatar().RebuildOutfit();
                WriteProgress("loaded-fixture-reconstruction-started");
            }

            private void CaptureVerifiedFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "capture-loaded-" + fixture.Label;
                ElementalPersistenceObservation loaded = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll, 0, 1);
                AbilityData abilityBeforeRest = RequireAbility(_currentUnit,
                    fixture.Blueprints.SlaAbility);
                int casterLevelBeforeRest = abilityBeforeRest
                    .CreateExecutionContext(new TargetWrapper(_currentUnit))
                    .Params.CasterLevel;
                Kingmaker.Controllers.Rest.RestController.ApplyRest(
                    _currentUnit.Descriptor);
                int resourceAfterRest = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Blueprints.SlaResource);
                AbilityData abilityAfterRest = RequireAbility(_currentUnit,
                    fixture.Blueprints.SlaAbility);
                bool restExact = resourceAfterRest == 1 &&
                    abilityAfterRest.IsAvailable &&
                    abilityAfterRest.GetAvailableForCastCount() == 1;

                AdvanceOneGunslingerLevel(fixture);
                _currentUnit.View.UpdateClassEquipment();
                CurrentAvatar().RebuildOutfit();
                ElementalPersistenceObservation advanced = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll, 1, 2);
                AbilityData abilityAfterLevel = RequireAbility(_currentUnit,
                    fixture.Blueprints.SlaAbility);
                int casterLevelAfterLevel = abilityAfterLevel
                    .CreateExecutionContext(new TargetWrapper(_currentUnit))
                    .Params.CasterLevel;
                bool levelExact =
                    _currentUnit.Descriptor.Progression.CharacterLevel == 2 &&
                    _currentUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) == 2 &&
                    ReferenceEquals(_currentUnit.Descriptor.Progression.Race,
                        fixture.Blueprints.Race) &&
                    casterLevelBeforeRest == 1 &&
                    casterLevelAfterLevel == 2 &&
                    _currentExpectedDoll.Matches(
                        _currentUnit.Descriptor.Doll);
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "phase", "module-disabled-verify-cleanup" },
                    { "loadedDollExactBeforeReconstruction",
                        _currentLoadedDollExact },
                    { "loadedObservation", loaded.Evidence },
                    { "loadedObservationExact", loaded.Exact },
                    { "casterLevelBeforeRest", casterLevelBeforeRest },
                    { "resourceAfterRest", resourceAfterRest },
                    { "restExact", restExact },
                    { "advancedObservation", advanced.Evidence },
                    { "advancedObservationExact", advanced.Exact },
                    { "casterLevelAfterLevel", casterLevelAfterLevel },
                    { "levelUpExact", levelExact }
                };
                CaptureFixture(record, fixture, _currentUnit);
                if (!_currentLoadedDollExact || !loaded.Exact ||
                    !restExact || !advanced.Exact || !levelExact)
                    throw new InvalidOperationException(fixture.Label +
                        " did not satisfy exact module-OFF load, rest, level-up, and visual reconstruction contracts.");
                _currentUnit = null;
                _currentExpectedDoll = null;
                _currentLoadedDollExact = false;
                WriteProgress("loaded-fixture-verified");
            }

            private void AdvanceOneGunslingerLevel(
                ElementalPersistenceFixture fixture)
            {
                _stage = "native-level-up-" + fixture.Label;
                LevelUpController controller = null;
                try
                {
                    controller = LevelUpController
                        .StartWithoutAssigningStaticInstance(
                            _currentUnit.Descriptor, false, null, null,
                            LevelUpState.CharBuildMode.CharGen);
                    if (controller == null ||
                        !controller.SelectClass(_gunslingerClass, false))
                        throw new InvalidOperationException(fixture.Label +
                            " second Gunslinger level was rejected.");
                    controller.ApplyClassMechanics();
                    MethodInfo apply = typeof(LevelUpController).GetMethod(
                        "ApplyLevelup", BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                    if (apply == null)
                        throw new MissingMethodException(
                            typeof(LevelUpController).FullName,
                            "ApplyLevelup(UnitDescriptor)");
                    apply.Invoke(controller,
                        new object[] { _currentUnit.Descriptor });
                    controller.Cancel();
                    controller = null;
                }
                finally
                {
                    if (controller != null) controller.Cancel();
                }
                if (!RollbackStarterGrants())
                    throw new InvalidOperationException(fixture.Label +
                        " native level-up changed starting inventory.");
            }

            private ElementalPersistenceObservation ObserveFixture(
                ElementalPersistenceFixture fixture, UnitEntityData unit,
                PersistenceDollSnapshot expectedDoll,
                int expectedResource, int expectedLevel)
            {
                if (unit == null || unit.Descriptor == null ||
                    unit.View == null || expectedDoll == null)
                    throw new InvalidOperationException(fixture.Label +
                        " observation requires a live unit, view, and expected DollData.");
                UnitDescriptor owner = unit.Descriptor;
                Character avatar = unit.View.GetComponent<Character>();
                BlueprintRace race = fixture.Blueprints.Race;
                bool raceExact = ReferenceEquals(owner.Progression.Race,
                        race) && owner.Progression.Features.GetRank(race) == 1 &&
                    owner.CustomGender.HasValue &&
                    owner.CustomGender.Value == fixture.Gender &&
                    owner.Gender == fixture.Gender &&
                    owner.Progression.CharacterLevel == expectedLevel &&
                    owner.Progression.GetClassLevel(_gunslingerClass) ==
                        expectedLevel && race.Size == Size.Medium;
                bool factsExact = race.Features != null &&
                    race.Features.All(value => value != null &&
                        owner.HasFact(value) &&
                        (!(value is BlueprintFeature) ||
                            owner.Progression.Features.GetRank(
                                (BlueprintFeature)value) == 1)) &&
                    owner.HasFact(fixture.Blueprints.Resistance) &&
                    owner.HasFact(fixture.Blueprints.Affinity) &&
                    owner.HasFact(fixture.Blueprints.SlaFeature);
                bool statExact = fixture.Blueprints.Definition.Stats.All(
                    value => owner.Stats.GetStat(value.Stat).ModifiedValue -
                        owner.Stats.GetStat(value.Stat).BaseValue ==
                            value.Value);
                BlueprintFeatureBase keen = race.Features.Single(value =>
                    string.Equals(value.AssetGuid,
                        ElementalRaceIdentityCatalog.KeenSensesGuid,
                        StringComparison.Ordinal));
                ModifiableValue perception = owner.Stats.GetStat(
                    StatType.SkillPerception);
                int racialPerception = perception.Modifiers.Where(value =>
                    value.ModDescriptor == ModifierDescriptor.Racial &&
                    value.Source != null && value.Source.Blueprint != null &&
                    ReferenceEquals(value.Source.Blueprint, keen))
                    .Sum(value => value.ModValue);
                int expectedSpeed = fixture.Blueprints.Definition
                    .SlowAndSteady ? 20 : 30;
                statExact &= racialPerception == 2 &&
                    owner.Stats.Speed.ModifiedValue == expectedSpeed;

                int resource = owner.Resources.GetResourceAmount(
                    fixture.Blueprints.SlaResource);
                bool resourceExact =
                    fixture.Blueprints.SlaResource.GetMaxAmount(owner) == 1 &&
                    resource == expectedResource;
                AbilityData ability = RequireAbility(unit,
                    fixture.Blueprints.SlaAbility);
                int casterLevel = ability.CreateExecutionContext(
                    new TargetWrapper(unit)).Params.CasterLevel;
                bool blueprintSupportsSpend = unit.Blueprint != null &&
                    !unit.Blueprint.IsCheater;
                bool abilityExact = blueprintSupportsSpend &&
                    ability.Blueprint.Type ==
                        AbilityType.SpellLike && ability.Spellbook == null &&
                    !ability.RequireMaterialComponent &&
                    !ability.IsAffectedByArcaneSpellFailure &&
                    ability.GetAvailableForCastCount() == expectedResource &&
                    ability.IsAvailable == (expectedResource > 0) &&
                    casterLevel == expectedLevel;

                DollData data = owner.Doll;
                bool dollExact = expectedDoll.Matches(data) &&
                    SerializedElementalClassClothesAbsent(data);
                string[] customizationIds = data == null ||
                        data.EquipmentEntityIds == null
                    ? new string[0] : data.EquipmentEntityIds.Where(value =>
                        !string.IsNullOrWhiteSpace(value) &&
                        !string.Equals(value,
                            ElementalRaceVisualCatalog.EmptyAssetId,
                            StringComparison.Ordinal)).ToArray();
                EquipmentEntity[] customization = customizationIds.Select(
                    value => ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        value, true)).ToArray();
                string[] productionIds = fixture.Gender == Gender.Male
                    ? GunslingerClassAppearanceCatalog.MaleAssetIds()
                    : GunslingerClassAppearanceCatalog.FemaleAssetIds();
                EquipmentEntity[] production = productionIds.Select(value =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(value,
                        true)).ToArray();
                string[] legacyIds = fixture.Gender == Gender.Male
                    ? PersistedMaleFighterAssetIds
                    : PersistedFemaleFighterAssetIds;
                EquipmentEntity[] legacy = legacyIds.Select(value =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(value,
                        true)).Where(value => value != null).ToArray();
                Renderer[] renderers = ActiveRenderers(unit);
                bool materialsExact = renderers.Length > 0 &&
                    renderers.All(value => value.sharedMaterials != null &&
                        value.sharedMaterials.Length > 0 &&
                        value.sharedMaterials.All(material =>
                            material != null && material.shader != null));
                bool appearanceExact = avatar != null &&
                    customization.Length > 0 &&
                    customization.All(value => value != null &&
                        avatar.EquipmentEntities.Count(current =>
                            ReferenceEquals(current, value)) == 1) &&
                    production.All(value => value != null &&
                        avatar.EquipmentEntities.Count(current =>
                            ReferenceEquals(current, value)) == 1) &&
                    legacy.All(value => !avatar.EquipmentEntities.Any(
                        current => ReferenceEquals(current, value))) &&
                    HasExactHumanoidRig(unit.View.transform) &&
                    materialsExact;

                return new ElementalPersistenceObservation
                {
                    RaceExact = raceExact,
                    FactsExact = factsExact,
                    StatsExact = statExact,
                    ResourceExact = resourceExact,
                    AbilityExact = abilityExact,
                    DollExact = dollExact,
                    AppearanceExact = appearanceExact,
                    Evidence = new JObject
                    {
                        { "raceGuid", race.AssetGuid },
                        { "raceReferenceExact", raceExact },
                        { "raceId", race.RaceId.ToString() },
                        { "effectiveGender", owner.Gender.ToString() },
                        { "customGender", owner.CustomGender.HasValue
                            ? owner.CustomGender.Value.ToString() : "<null>" },
                        { "characterLevel",
                            owner.Progression.CharacterLevel },
                        { "gunslingerLevel", owner.Progression
                            .GetClassLevel(_gunslingerClass) },
                        { "factsExact", factsExact },
                        { "statsExact", statExact },
                        { "racialPerception", racialPerception },
                        { "speed", owner.Stats.Speed.ModifiedValue },
                        { "resource", resource },
                        { "resourceMaximum", fixture.Blueprints.SlaResource
                            .GetMaxAmount(owner) },
                        { "abilityGuid",
                            fixture.Blueprints.SlaAbility.AssetGuid },
                        { "blueprintCheater", unit.Blueprint != null &&
                            unit.Blueprint.IsCheater },
                        { "abilityAvailable", ability.IsAvailable },
                        { "abilityAvailableCount",
                            ability.GetAvailableForCastCount() },
                        { "abilityType", ability.Blueprint.Type.ToString() },
                        { "spellbookAbsent", ability.Spellbook == null },
                        { "arcaneFailureInapplicable",
                            !ability.IsAffectedByArcaneSpellFailure },
                        { "casterLevel", casterLevel },
                        { "dollExact", dollExact },
                        { "dollData", data == null ? JValue.CreateNull() :
                            (JToken)PersistenceDollSnapshot.Capture(data)
                                .Describe() },
                        { "customizationAssetIds",
                            new JArray(customizationIds) },
                        { "productionClassAssetIds",
                            new JArray(productionIds) },
                        { "productionClassPresent", production.All(value =>
                            value != null && avatar != null &&
                            avatar.EquipmentEntities.Any(current =>
                                ReferenceEquals(current, value))) },
                        { "historicalFighterAbsent", legacy.All(value =>
                            avatar != null && !avatar.EquipmentEntities.Any(
                                current => ReferenceEquals(current, value))) },
                        { "activeRendererCount", renderers.Length },
                        { "materialsExact", materialsExact },
                        { "humanoidRigExact",
                            HasExactHumanoidRig(unit.View.transform) },
                        { "appearanceExact", appearanceExact }
                    }
                };
            }

            private static bool SerializedElementalClassClothesAbsent(
                DollData data)
            {
                if (data == null || data.EquipmentEntityIds == null)
                    return false;
                string[] classIds = GunslingerClassAppearanceCatalog
                    .MaleAssetIds().Concat(GunslingerClassAppearanceCatalog
                        .FemaleAssetIds()).Concat(
                            PersistedMaleFighterAssetIds).Concat(
                                PersistedFemaleFighterAssetIds).ToArray();
                return !data.EquipmentEntityIds.Any(value =>
                    classIds.Contains(value, StringComparer.Ordinal));
            }

            private void CaptureFixture(JObject record,
                ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                string stem = SafeFileName("elemental-persistence-" +
                    (_prepare ? "prepare-" : "verify-") + fixture.Label);
                string previewPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-preview.png");
                string isometricPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-isometric.png");
                Renderer[] renderers = ActiveRenderers(unit);
                WeaponPresentationEvidenceScenario.CaptureSummary preview =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        unit, null, renderers, previewPath, true);
                IsometricCapture isometric = CaptureIsometric(unit,
                    renderers, isometricPath);
                record["preview"] = new JObject
                {
                    { "file", Path.GetFileName(preview.PngPath) },
                    { "bytes", preview.Bytes },
                    { "sha256", preview.Sha256 },
                    { "meaningfulPixels", preview.MeaningfulPixels },
                    { "framing", preview.Framing },
                    { "lowPixelDensity", preview.LowPixelDensity },
                    { "views", 4 }
                };
                record["isometric"] = new JObject
                {
                    { "file", Path.GetFileName(isometric.Path) },
                    { "bytes", isometric.Bytes },
                    { "sha256", isometric.Sha256 },
                    { "meaningfulPixels", isometric.MeaningfulPixels },
                    { "rendererCount", isometric.RendererCount },
                    { "bounds", isometric.Bounds },
                    { "framing", isometric.Framing },
                    { "lowPixelDensity", isometric.LowPixelDensity },
                    { "views", 1 }
                };
                record["saveApiCalledAtCapture"] = false;
                record["productionBlueprintMutated"] = false;
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".json");
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(preview.PngPath);
                _evidenceFiles.Add(isometric.Path);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _imageCount += 2;
                _viewCount += 5;
                if (preview.LowPixelDensity || isometric.LowPixelDensity)
                    _warnings.Add(fixture.Label +
                        " has low foreground pixel density; retain it as a framing diagnostic.");
            }

            private static AbilityData RequireAbility(UnitEntityData unit,
                BlueprintAbility blueprint)
            {
                Ability ability = unit.Descriptor.Abilities
                    .GetAbility(blueprint);
                if (ability == null)
                    throw new InvalidOperationException(
                        "Elemental persistence fixture did not receive " +
                        blueprint.name + ".");
                return new AbilityData(ability);
            }

            private static void InvokeAbilitySpend(AbilityData ability,
                BlueprintAbilityResource resource)
            {
                if (ability == null || resource == null)
                    throw new ArgumentNullException(ability == null
                        ? "ability" : "resource");
                AbilityResourceLogic[] costs = ability.Blueprint
                    .ComponentsArray.OfType<AbilityResourceLogic>()
                    .Where(value => value.IsSpendResource &&
                        ReferenceEquals(value.RequiredResource, resource))
                    .ToArray();
                if (costs.Length != 1 || costs[0].CostIsCustom ||
                    costs[0].Amount != 1)
                    throw new InvalidOperationException(
                        "Elemental persistence requires one exact native one-use resource cost.");
                costs[0].Spend(ability);
            }

            private bool RegisteredIdentitiesExact()
            {
                if (_blueprintSet == null || _blueprintSet.Count !=
                        ElementalRaceIdentityCatalog.IdentityCount)
                    return false;
                ElementalRaceBlueprints[] races = _blueprintSet
                    .OrderedBlueprints().ToArray();
                return races.Length == ElementalRaceCatalog.RaceCount &&
                    races.Select(value => value.Race.AssetGuid).Distinct(
                        StringComparer.Ordinal).Count() == races.Length &&
                    races.All(value =>
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintRace>(value.Race.AssetGuid),
                            value.Race) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintFeature>(value.Resistance.AssetGuid),
                            value.Resistance) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintFeature>(value.Affinity.AssetGuid),
                            value.Affinity) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintFeature>(value.SlaFeature.AssetGuid),
                            value.SlaFeature) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintAbilityResource>(
                                value.SlaResource.AssetGuid),
                            value.SlaResource) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                            BlueprintAbility>(value.SlaAbility.AssetGuid),
                            value.SlaAbility));
            }

            private bool SelectorStateExact()
            {
                BlueprintRace[] published = BlueprintRoot.Instance
                    .Progression.CharacterRaces;
                bool enabled = _context.FeatureModules.Active.ElementalRaces;
                foreach (BlueprintRace expected in _blueprintSet.OrderedRaces())
                {
                    BlueprintRace[] matches = published.Where(value =>
                        value != null && (ReferenceEquals(value, expected) ||
                        string.Equals(value.AssetGuid, expected.AssetGuid,
                            StringComparison.Ordinal))).ToArray();
                    if (enabled)
                    {
                        if (matches.Length != 1 ||
                            !ReferenceEquals(matches[0], expected))
                            return false;
                    }
                    else if (matches.Length != 0) return false;
                }
                return true;
            }

            private void RecordPartySnapshot()
            {
                foreach (UnitEntityData unit in _partyBefore
                    .OfType<UnitEntityData>())
                    _partyRecords.Add(new JObject
                    {
                        { "name", unit == null ? "<null>" :
                            unit.CharacterName ?? "<unnamed>" },
                        { "uniqueId", unit == null ? "<null>" :
                            unit.UniqueId ?? "<null>" },
                        { "fixture", unit != null && IsFixtureUnit(unit) },
                        { "raceGuid", unit == null ||
                            unit.Descriptor == null ||
                            unit.Descriptor.Progression == null ||
                            unit.Descriptor.Progression.Race == null
                                ? "<null>" : unit.Descriptor.Progression
                                    .Race.AssetGuid },
                        { "characterLevel", unit == null ||
                            unit.Descriptor == null ||
                            unit.Descriptor.Progression == null ? -1 :
                            unit.Descriptor.Progression.CharacterLevel },
                        { "hasDollData", unit != null &&
                            unit.Descriptor != null &&
                            unit.Descriptor.Doll != null },
                        { "hasLiveView", unit != null &&
                            unit.View != null && unit.View.GetComponent<
                                Character>() != null }
                    });
            }

            private JObject DescribeFixtureMembership()
            {
                return new JObject
                {
                    { "globalUnits", FixtureIdentityCount(_unitsBefore) },
                    { "party", FixtureIdentityCount(_partyBefore) },
                    { "partyCharacters", FixtureIdentityCount(
                        _partyCharactersBefore) },
                    { "remoteCompanions", FixtureIdentityCount(
                        _remoteBefore) },
                    { "crossScene", FixtureIdentityCount(_crossBefore) },
                    { "crossSceneUnits", _crossBefore.OfType<UnitEntityData>()
                        .Count(IsFixtureUnit) },
                    { "perFixtureParty", new JArray(_fixtures.Select(value =>
                        new JObject
                        {
                            { "fixture", value.Label },
                            { "uniqueId", value.UniqueId },
                            { "count", FixtureIdentityCount(
                                _partyBefore, value.UniqueId) }
                        }).ToArray()) }
                };
            }

            private bool BaselineShapeExact()
            {
                return _partyBefore.Length ==
                        WorkingSaveSmokeScenario.ExpectedPartyCount &&
                    _partyCharactersBefore.Length ==
                        WorkingSaveSmokeScenario.ExpectedPartyCount &&
                    FixtureIdentityCount(_unitsBefore) == 0 &&
                    FixtureIdentityCount(_partyBefore) == 0 &&
                    FixtureIdentityCount(_partyCharactersBefore) == 0 &&
                    FixtureIdentityCount(_remoteBefore) == 0 &&
                    FixtureIdentityCount(_crossBefore) == 0;
            }

            private UnitEntityData[] ResolveLoadedFixtures()
            {
                var result = new List<UnitEntityData>();
                foreach (ElementalPersistenceFixture fixture in _fixtures)
                {
                    UnitEntityData[] matches = _partyBefore
                        .OfType<UnitEntityData>().Where(value =>
                            IsFixtureUnit(value, fixture)).ToArray();
                    if (matches.Length != 1)
                        throw new InvalidOperationException(fixture.Label +
                            " fresh-load party identity count was " +
                            matches.Length + ".");
                    result.Add(matches[0]);
                }
                return result.ToArray();
            }

            private bool LoadedMembershipExact()
            {
                if (_loadedUnits.Length != ElementalPersistenceFixtureCount ||
                    _loadedUnits.Distinct().Count() !=
                        ElementalPersistenceFixtureCount ||
                    _partyBefore.Length != ElementalPersistencePartyCount ||
                    _partyCharactersBefore.Length !=
                        ElementalPersistencePartyCount ||
                    FixtureIdentityCount(_partyBefore) !=
                        ElementalPersistenceFixtureCount ||
                    FixtureIdentityCount(_partyCharactersBefore) !=
                        ElementalPersistenceFixtureCount ||
                    FixtureIdentityCount(_remoteBefore) != 0 ||
                    FixtureIdentityCount(_crossBefore) !=
                        ElementalPersistenceFixtureCount ||
                    _crossBefore.OfType<UnitEntityData>().Count(
                        IsFixtureUnit) != ElementalPersistenceFixtureCount)
                    return false;
                UnitEntityData[] global = _unitsBefore
                    .OfType<UnitEntityData>().Where(IsFixtureUnit).ToArray();
                if (global.Length > ElementalPersistenceFixtureCount ||
                    global.Any(value => !_loadedUnits.Any(current =>
                        ReferenceEquals(current, value))))
                    return false;
                return _fixtures.All(fixture =>
                    FixtureIdentityCount(_partyBefore,
                        fixture.UniqueId) == 1 &&
                    FixtureIdentityCount(_partyCharactersBefore,
                        fixture.UniqueId) == 1 &&
                    FixtureIdentityCount(_crossBefore,
                        fixture.UniqueId) == 1);
            }

            private bool PreparedMembershipExact()
            {
                object[] units = Snapshot(_allUnits);
                object[] party = Snapshot(_party);
                object[] partyCharacters = _player.PartyCharacters
                    .Cast<object>().ToArray();
                object[] cross = Snapshot(_cross);
                bool countsExact = party.Length ==
                        ElementalPersistencePartyCount &&
                    partyCharacters.Length ==
                        ElementalPersistencePartyCount &&
                    FixtureIdentityCount(party) ==
                        ElementalPersistenceFixtureCount &&
                    FixtureIdentityCount(partyCharacters) ==
                        ElementalPersistenceFixtureCount &&
                    FixtureIdentityCount(Snapshot(_remote)) == 0 &&
                    FixtureIdentityCount(cross) ==
                        ElementalPersistenceFixtureCount &&
                    cross.OfType<UnitEntityData>().Count(IsFixtureUnit) ==
                        ElementalPersistenceFixtureCount &&
                    _createdUnits.Count ==
                        ElementalPersistenceFixtureCount &&
                    _createdUnits.All(value => value != null &&
                        IsFixtureUnit(value) &&
                        party.Any(current => ReferenceEquals(current,
                            value)) && cross.Any(current =>
                                ReferenceEquals(current, value)));
                bool eachExact = _fixtures.All(fixture =>
                    FixtureIdentityCount(party, fixture.UniqueId) == 1 &&
                    FixtureIdentityCount(partyCharacters,
                        fixture.UniqueId) == 1 &&
                    FixtureIdentityCount(cross, fixture.UniqueId) == 1);
                bool globalExact = units.OfType<UnitEntityData>()
                    .Where(IsFixtureUnit).All(value =>
                        _createdUnits.Any(current => ReferenceEquals(
                            current, value)));
                return countsExact && eachExact && globalExact &&
                    SameReferences(_inventoryBefore, Snapshot(_inventory)) &&
                    _player.Money == _moneyBefore &&
                    _gunslingerClass.StartingGold == 0 &&
                    RollbackStarterGrants();
            }

            private bool IsFixtureUnit(UnitEntityData unit)
            {
                return _fixtures.Any(value => IsFixtureUnit(unit, value));
            }

            private static bool IsFixtureUnit(UnitEntityData unit,
                ElementalPersistenceFixture fixture)
            {
                return unit != null && fixture != null &&
                    string.Equals(unit.UniqueId, fixture.UniqueId,
                        StringComparison.Ordinal) &&
                    unit.Descriptor != null && string.Equals(
                        unit.Descriptor.CustomName, fixture.Name,
                        StringComparison.Ordinal);
            }

            private bool HasFixtureIdentity(object value)
            {
                UnitEntityData unit = value as UnitEntityData;
                if (unit != null) return IsFixtureUnit(unit);
                if (!(value is UnitReference)) return false;
                UnitReference reference = (UnitReference)value;
                return _fixtures.Any(fixture =>
                    string.Equals(reference.UniqueId, fixture.UniqueId,
                        StringComparison.Ordinal));
            }

            private int FixtureIdentityCount(IEnumerable<object> values,
                string uniqueId = null)
            {
                if (values == null) return 0;
                int count = 0;
                foreach (object value in values)
                {
                    UnitEntityData unit = value as UnitEntityData;
                    if (unit != null)
                    {
                        if (IsFixtureUnit(unit) &&
                            (uniqueId == null || string.Equals(unit.UniqueId,
                                uniqueId, StringComparison.Ordinal)))
                            count++;
                        continue;
                    }
                    if (!(value is UnitReference)) continue;
                    UnitReference reference = (UnitReference)value;
                    if (_fixtures.Any(fixture =>
                            string.Equals(reference.UniqueId,
                                fixture.UniqueId, StringComparison.Ordinal) &&
                            (uniqueId == null || string.Equals(
                                reference.UniqueId, uniqueId,
                                StringComparison.Ordinal))))
                        count++;
                }
                return count;
            }

            private bool RollbackStarterGrants()
            {
                if (_player == null || _player.Inventory == null)
                    return false;
                bool exact = true;
                for (int index = 0; index < _startingItems.Length; index++)
                {
                    int current = _player.Inventory.Count(
                        _startingItems[index]);
                    int excess = current - _startingItemCounts[index];
                    if (excess > 0)
                        _player.Inventory.Remove(_startingItems[index],
                            excess);
                    else if (excess < 0) exact = false;
                }
                return exact && _startingItems.Select((value, index) =>
                    _player.Inventory.Count(value) ==
                        _startingItemCounts[index]).All(value => value);
            }

            private void RecordException(Exception exception)
            {
                if (!string.IsNullOrWhiteSpace(_exceptionSummary)) return;
                _exceptionSummary = exception.ToString();
                _diagnostics.Add("exceptionStage=" + _stage + ";" +
                    exception);
                Add(_assertions, "elemental-race-persistence-exception",
                    "no exception", "stage=" + _stage + ";" + exception,
                    false,
                    "guarded marker-bound working-save transaction");
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "elemental-persistence-cleanup";
                try
                {
                    if (_gunslingerClass != null)
                        _gunslingerClass.StartingGold =
                            _startingGoldBefore;
                    RollbackStarterGrants();
                    RemoveFixtureState();
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("cleanupException=" + exception);
                }
                _cleanupStarted = true;
                _settleUpdates = 0;
                WriteProgress("cleanup-started");
            }

            private void RemoveFixtureState()
            {
                if (_player == null || _fixtures.Length == 0) return;
                for (int index = _player.PartyCharacters.Count - 1;
                    index >= 0; index--)
                    if (_fixtures.Any(fixture => string.Equals(
                            _player.PartyCharacters[index].UniqueId,
                            fixture.UniqueId, StringComparison.Ordinal)))
                        _player.PartyCharacters.RemoveAt(index);
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();

                UnitEntityData[] candidates = Snapshot(_party)
                    .Concat(Snapshot(_cross)).Concat(Snapshot(_allUnits))
                    .OfType<UnitEntityData>().Concat(_createdUnits)
                    .Where(value => value != null && IsFixtureUnit(value))
                    .Distinct().ToArray();
                foreach (UnitEntityData unit in candidates)
                {
                    unit.Commands.InterruptAll(true);
                    if (unit.CombatState.IsInCombat)
                        unit.CombatState.LeaveCombat();
                    if (unit.Descriptor != null)
                        unit.Descriptor.State.Immortality.ReleaseAll();
                    if (unit.HoldingState != null &&
                        unit.HoldingState.AllEntityData.Any(value =>
                            ReferenceEquals(value, unit)))
                        unit.HoldingState.RemoveEntityData(unit);
                    else
                    {
                        if (ContainsReference(_allUnits, unit))
                            Game.Instance.State.Units.All.Remove(unit);
                        unit.Dispose();
                    }
                }
                if (_prepare && !_saveStarted)
                    foreach (BlueprintUnit blueprint in _createdBlueprints)
                        if (blueprint != null)
                            UnityEngine.Object.DestroyImmediate(blueprint);
                _currentUnit = null;
                _currentBlueprint = null;
                _currentExpectedDoll = null;
                _createdUnits.Clear();
                _createdBlueprints.Clear();
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                object[] expectedUnits = _unitsBefore.Where(value =>
                    !HasFixtureIdentity(value)).ToArray();
                object[] expectedParty = _partyBefore.Where(value =>
                    !HasFixtureIdentity(value)).ToArray();
                object[] expectedPartyCharacters = _partyCharactersBefore
                    .Where(value => !HasFixtureIdentity(value)).ToArray();
                object[] expectedRemote = _remoteBefore.Where(value =>
                    !HasFixtureIdentity(value)).ToArray();
                object[] expectedCross = _crossBefore.Where(value =>
                    !HasFixtureIdentity(value)).ToArray();
                object[] currentUnits = Snapshot(_allUnits);
                object[] currentParty = Snapshot(_party);
                object[] currentPartyCharacters = _player.PartyCharacters
                    .Cast<object>().ToArray();
                bool cleaned = SameReferences(expectedUnits, currentUnits) &&
                    SameReferences(expectedParty, currentParty) &&
                    SameValues(expectedPartyCharacters,
                        currentPartyCharacters) &&
                    SameValues(expectedRemote, Snapshot(_remote)) &&
                    SameReferences(expectedCross, Snapshot(_cross)) &&
                    SameReferences(_inventoryBefore, Snapshot(_inventory)) &&
                    _player.Money == _moneyBefore &&
                    (_gunslingerClass == null ||
                        _gunslingerClass.StartingGold ==
                            _startingGoldBefore) &&
                    _characterRacesBefore.Length == BlueprintRoot.Instance
                        .Progression.CharacterRaces.Length &&
                    _characterRacesBefore.Select((value, index) =>
                        ReferenceEquals(value, BlueprintRoot.Instance
                            .Progression.CharacterRaces[index])).All(
                                value => value) &&
                    FixtureIdentityCount(currentUnits) == 0 &&
                    FixtureIdentityCount(currentParty) == 0 &&
                    FixtureIdentityCount(currentPartyCharacters) == 0 &&
                    FixtureIdentityCount(Snapshot(_remote)) == 0 &&
                    FixtureIdentityCount(Snapshot(_cross)) == 0;
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates)
                    return;
                _structuralCleaned = cleaned;
                if (cleaned && !_prepare && !_verifyAbsent &&
                    !_saveStarted)
                {
                    StartExactWorkingSave();
                    return;
                }
                Finish(cleaned);
            }

            private static bool SameValues(object[] expected,
                object[] observed)
            {
                if (expected.Length != observed.Length) return false;
                for (int index = 0; index < expected.Length; index++)
                    if (!object.Equals(expected[index], observed[index]))
                        return false;
                return true;
            }

            private void StartExactWorkingSave()
            {
                _stage = _prepare ? "save-eight-spent-fixtures" :
                    "save-eight-fixture-cleanup";
                if (_saveStarted)
                    throw new InvalidOperationException(
                        "The exact elemental working-save write was already started.");
                if (_gunslingerClass != null)
                    _gunslingerClass.StartingGold = _startingGoldBefore;
                if (!RollbackStarterGrants())
                    throw new InvalidOperationException(
                        "Elemental persistence refused to save after starting inventory drift.");
                _workingSaveSmoke.ArmExactWorkingSaveWrite();
                MethodInfo saveGame = typeof(Game).GetMethods(
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic).Single(value =>
                        value.Name == "SaveGame" &&
                        value.ReturnType == typeof(void) &&
                        value.GetParameters().Length == 2 &&
                        value.GetParameters()[0].ParameterType.FullName ==
                            "Kingmaker.EntitySystem.Persistence.SaveInfo" &&
                        value.GetParameters()[1].ParameterType ==
                            typeof(Action));
                _saveStarted = true;
                _saveElapsed = Stopwatch.StartNew();
                try
                {
                    saveGame.Invoke(Game.Instance, new object[]
                    {
                        _workingSaveSmoke.WorkingDescriptor,
                        new Action(() => _saveCompleted = true)
                    });
                }
                catch
                {
                    _saveStarted = false;
                    throw;
                }
                WriteProgress("exact-working-save-started");
            }

            private void PollExactWorkingSave()
            {
                if (_workingSaveSmoke.WriteObserved)
                {
                    RecordException(new InvalidOperationException(
                        "An unarmed, non-working, destructive, migration, or extra save boundary was observed."));
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(false);
                    return;
                }
                if (_saveCompleted)
                {
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(_prepare ? _preparedMembershipExact :
                        _structuralCleaned);
                    return;
                }
                if (_saveElapsed != null &&
                    _saveElapsed.Elapsed.TotalSeconds >=
                        _request.CompletionTimeoutSeconds)
                {
                    RecordException(new TimeoutException(
                        "The exact working-save completion callback did not arrive before timeout."));
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(false);
                }
            }

            private void WriteProgress(string progressStage)
            {
                var progress = new JObject
                {
                    { "schemaVersion", 1 },
                    { "utc", DateTime.UtcNow.ToString("o") },
                    { "scenario", _request.Scenario },
                    { "stage", progressStage },
                    { "detailStage", _stage },
                    { "phase", _phase },
                    { "fixtureIndex", _fixtureIndex },
                    { "fixtureCount", _fixtures.Length },
                    { "settleUpdates", _settleUpdates },
                    { "captured", _captured },
                    { "currentUnitPresent", _currentUnit != null },
                    { "saveStarted", _saveStarted },
                    { "cleanupStarted", _cleanupStarted }
                };
                WriteJsonAtomic(Path.Combine(_request.EvidenceDirectory,
                    "elemental-race-persistence-progress.json"), progress);
            }

            private void WriteIndex()
            {
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "phase", _prepare ? "prepare" : _verifyAbsent ?
                        "verify-absent" :
                        "module-disabled-verify-cleanup" },
                    { "loadedModVersion",
                        _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _gameAssemblySha256 },
                    { "gameAssemblyMvid", _gameAssemblyMvid },
                    { "elementalModuleActive",
                        _context.FeatureModules.Active.ElementalRaces },
                    { "registeredIdentitiesExact", _registeredExact },
                    { "selectorStateExact", _selectorExact },
                    { "fixtureCount", _fixtures.Length },
                    { "expectedPreparedPartyCount",
                        ElementalPersistencePartyCount },
                    { "fixtureUniqueIds", new JArray(
                        ElementalPersistenceFixtureIds) },
                    { "fixtures", new JArray(_fixtures.Select(value =>
                        new JObject
                        {
                            { "label", value.Label },
                            { "uniqueId", value.UniqueId },
                            { "name", value.Name },
                            { "raceGuid",
                                value.Blueprints.Race.AssetGuid },
                            { "gender", value.Gender.ToString() },
                            { "presetGuid", value.Preset.AssetGuid }
                        }).ToArray()) },
                    { "partyAtLoad", _partyRecords },
                    { "loadedFixtureMembership",
                        _loadedFixtureMembership },
                    { "currentFixtureMembership", new JObject
                        {
                            { "globalUnits", FixtureIdentityCount(
                                Snapshot(_allUnits)) },
                            { "party", FixtureIdentityCount(
                                Snapshot(_party)) },
                            { "partyCharacters", FixtureIdentityCount(
                                _player.PartyCharacters.Cast<object>()) },
                            { "remoteCompanions", FixtureIdentityCount(
                                Snapshot(_remote)) },
                            { "crossScene", FixtureIdentityCount(
                                Snapshot(_cross)) }
                        } },
                    { "records", _records },
                    { "preparedMembershipExact",
                        _preparedMembershipExact },
                    { "normalPathComplete", _normalPathComplete },
                    { "baselineAbsentExact", _baselineAbsentExact },
                    { "structuralCleaned", _structuralCleaned },
                    { "captureCount", _captured },
                    { "imageCount", _imageCount },
                    { "renderedViewCount", _viewCount },
                    { "saveApiCalled", _saveStarted },
                    { "expectedWorkingSaveRoutineCount",
                        _workingSaveEvidence == null ? 0 :
                            _workingSaveEvidence
                                .ExpectedWorkingSaveRoutineCount },
                    { "productionBlueprintMutated", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    "elemental-race-persistence-index.json");
                WriteJsonAtomic(path, index);
                if (!_evidenceFiles.Contains(path,
                        StringComparer.OrdinalIgnoreCase))
                    _evidenceFiles.Add(path);
                _indexWritten = true;
            }

            private bool ExactWorkingSaveEvidence(bool writeExpected)
            {
                if (_workingSaveEvidence == null ||
                    _workingSaveEvidence.SaveWritingApiObserved)
                    return false;
                return writeExpected
                    ? _workingSaveEvidence
                            .ExpectedWorkingSaveRoutineCount == 1 &&
                        _workingSaveEvidence
                            .ExpectedWorkingStashedAreaCount >= 1
                    : _workingSaveEvidence
                            .ExpectedWorkingSaveRoutineCount == 0;
            }

            private string SaveEvidenceDetail()
            {
                return _workingSaveEvidence == null ? "<missing>" :
                    "saveName=" + SaveName() + ";count=" +
                    _workingSaveEvidence.ExpectedWorkingSaveRoutineCount +
                    ";stashedAreas=" +
                    _workingSaveEvidence.ExpectedWorkingStashedAreaCount +
                    ";unexpected=" +
                    _workingSaveEvidence.SaveWritingApiObserved;
            }

            private string SaveName()
            {
                return _request.Parameters == null ? string.Empty :
                    _request.Parameters.Value<string>("saveName") ??
                        string.Empty;
            }

            private void Finish(bool phaseStateExact)
            {
                if (Complete) return;
                if (_gunslingerClass != null)
                    _gunslingerClass.StartingGold = _startingGoldBefore;
                if (_workingSaveEvidence == null)
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                if (!_indexWritten)
                {
                    try
                    {
                        WriteIndex();
                    }
                    catch (Exception exception)
                    {
                        _diagnostics.Add("indexWriteException=" + exception);
                    }
                }
                if (_prepare) FinishPrepare(phaseStateExact);
                else if (_verifyAbsent)
                    FinishVerifyAbsent(phaseStateExact);
                else FinishModuleDisabledVerify(phaseStateExact);
                CompletePhaseResult();
            }

            private void FinishPrepare(bool prepared)
            {
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool recordExact = records.Length ==
                        ElementalPersistenceFixtureCount &&
                    records.All(value => TokenBool(value,
                            "observationExact") &&
                        TokenBool(value, "spendExact") &&
                        value.Value<int>("resourceBeforeSpend") == 1 &&
                        value.Value<int>("resourceAfterSpend") == 0);
                bool capturesExact = CaptureSetExact(records);
                Add(_assertions, "elemental-race-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .ElementalRacePersistencePrepare,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRacePersistencePrepare,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "elemental-race-persistence-module-on",
                    "Elemental Races active with four exact selector entries and 68 registered blueprints",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    _context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "live module snapshot, CharacterRaces, and resource cache");
                Add(_assertions,
                    "elemental-race-persistence-prepared-rules",
                    "eight exact race/sex Gunslingers with level-1 facts, stats, resistance, affinity, Keen Senses, and available SLA",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "real native descriptors and production blueprints before save");
                Add(_assertions,
                    "elemental-race-persistence-spent-resources",
                    "all eight racial resources commit from one to zero before save",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "AbilityResourceLogic.Spend and owner persistent resources");
                Add(_assertions,
                    "elemental-race-persistence-prepared-membership",
                    "eight unique marker-bound fixtures appended to the exact three-character working-save party and scene",
                    "prepared=" + prepared + ";membership=" +
                        _preparedMembershipExact,
                    prepared && _preparedMembershipExact &&
                        _normalPathComplete,
                    "exact Party, PartyCharacters, cross-scene, global, remote, inventory, and money deltas");
                Add(_assertions,
                    "elemental-race-persistence-captures",
                    "8 sidecars, 16 PNGs, and 40 labelled views before save",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "four-view previews plus ordinary isometric captures");
                AddSaveAndIdentityAssertions(true);
                Add(_assertions,
                    "elemental-race-persistence-live-state-preserved",
                    "class blueprint, selector array, inventory, and money unchanged outside the eight disposable party additions",
                    "startingGold=" + _gunslingerClass.StartingGold +
                        ";inventoryExact=" + SameReferences(
                            _inventoryBefore, Snapshot(_inventory)) +
                        ";selectorArrayExact=" +
                        CharacterRacesArrayExact(),
                    _gunslingerClass.StartingGold ==
                        _startingGoldBefore &&
                    SameReferences(_inventoryBefore, Snapshot(_inventory)) &&
                    _player.Money == _moneyBefore &&
                    CharacterRacesArrayExact(),
                    "exact pre-run snapshots; production blueprints never mutated");
            }

            private void FinishModuleDisabledVerify(bool cleaned)
            {
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool recordExact = records.Length ==
                        ElementalPersistenceFixtureCount &&
                    records.All(value =>
                        TokenBool(value,
                            "loadedDollExactBeforeReconstruction") &&
                        TokenBool(value, "loadedObservationExact") &&
                        TokenBool(value, "restExact") &&
                        TokenBool(value, "advancedObservationExact") &&
                        TokenBool(value, "levelUpExact") &&
                        value.Value<int>("casterLevelBeforeRest") == 1 &&
                        value.Value<int>("resourceAfterRest") == 1 &&
                        value.Value<int>("casterLevelAfterLevel") == 2);
                bool capturesExact = CaptureSetExact(records);
                Add(_assertions, "elemental-race-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .ElementalRaceModuleDisabledPersistence,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRaceModuleDisabledPersistence,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "elemental-race-persistence-module-off",
                    "Elemental Races inactive and absent from selectors while all 68 blueprints remain registered",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    !_context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "fresh startup module snapshot, CharacterRaces, and resource cache");
                Add(_assertions,
                    "elemental-race-persistence-loaded-membership",
                    "exact eleven-member party with one marker-bound fixture for every race and sex",
                    _loadedFixtureMembership.ToString(
                        Newtonsoft.Json.Formatting.None),
                    LoadedMembershipExact(),
                    "fresh receiver-correlated Steam-backed working-save load");
                Add(_assertions,
                    "elemental-race-persistence-loaded-state",
                    "all eight fixtures retain race, facts, spent SLA, deterministic DollData, Gunslinger equipment, rig, and materials",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "actual loaded descriptors and native view reconstruction while selector publication is OFF");
                Add(_assertions,
                    "elemental-race-persistence-rest-and-level-up",
                    "ordinary rest restores one use and native level-up retains race/facts while caster level advances from one to two for every fixture",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "RestController.ApplyRest and LevelUpController.ApplyLevelup");
                Add(_assertions,
                    "elemental-race-persistence-captures",
                    "8 sidecars, 16 PNGs, and 40 labelled post-load views",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "post-reconstruction four-view previews and isometric captures");
                Add(_assertions,
                    "elemental-race-persistence-cleanup",
                    "all eight marker fixtures removed and exact three-character party/global/remote/cross-scene/inventory/money baseline cleanup-saved",
                    "cleaned=" + cleaned + ";normalPath=" +
                        _normalPathComplete + ";updates=" +
                        _settleUpdates,
                    cleaned && _normalPathComplete &&
                        _structuralCleaned,
                    "marker-only native scene removal and exact cleanup save");
                AddSaveAndIdentityAssertions(true);
            }

            private void FinishVerifyAbsent(bool cleaned)
            {
                bool exact = cleaned && _normalPathComplete &&
                    _baselineAbsentExact && _records.Count == 0 &&
                    _captured == 0 && _imageCount == 0 &&
                    _viewCount == 0 && _indexWritten &&
                    _evidenceFiles.Count == 1 &&
                    _evidenceFiles.All(File.Exists);
                Add(_assertions, "elemental-race-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .ElementalRacePersistenceVerifyAbsent,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRacePersistenceVerifyAbsent,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "elemental-race-persistence-absence",
                    "fresh exact three-character baseline with zero elemental persistence fixture identities",
                    "baselineAbsentExact=" + _baselineAbsentExact +
                        ";cleaned=" + cleaned,
                    exact,
                    "fresh-load post-cleanup serialization boundary");
                Add(_assertions,
                    "elemental-race-persistence-identities-after-cleanup",
                    "all identities registered and selector state consistent with the exactly restored original module setting",
                    "registeredExact=" + _registeredExact +
                        ";selectorExact=" + _selectorExact +
                        ";active=" + _context.FeatureModules.Active
                            .ElementalRaces,
                    _registeredExact && _selectorExact,
                    "fresh startup resource cache and CharacterRaces");
                AddSaveAndIdentityAssertions(false);
            }

            private void AddSaveAndIdentityAssertions(bool writeExpected)
            {
                Add(_assertions,
                    "elemental-race-persistence-save-boundary",
                    writeExpected
                        ? "one exact KMG_AUTOMATION_WORKING SaveRoutine"
                        : "fresh exact KMG_AUTOMATION_WORKING load; zero writes",
                    SaveEvidenceDetail(),
                    string.Equals(SaveName(),
                        WorkingSaveSmokeScenario.ExpectedName,
                        StringComparison.Ordinal) &&
                        ExactWorkingSaveEvidence(writeExpected),
                    "armed exact captured SaveInfo reference; protected baseline excluded");
                Add(_assertions,
                    "elemental-race-persistence-game-identity",
                    "Kingmaker 2.1.7b exact Assembly-CSharp SHA-256 and MVID",
                    "sha256=" + _gameAssemblySha256 + ";mvid=" +
                        _gameAssemblyMvid,
                    string.Equals(_gameAssemblySha256,
                        ExpectedAssemblySha256,
                        StringComparison.Ordinal) &&
                    string.Equals(_gameAssemblyMvid,
                        ExpectedAssemblyMvid,
                        StringComparison.OrdinalIgnoreCase),
                    "live loaded Assembly-CSharp identity");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");
            }

            private bool CaptureSetExact(JObject[] records)
            {
                return _normalPathComplete && records.Length ==
                        ElementalPersistenceFixtureCount &&
                    _captured == ElementalPersistenceFixtureCount &&
                    _imageCount == ElementalPersistenceFixtureCount * 2 &&
                    _viewCount == ElementalPersistenceFixtureCount * 5 &&
                    _indexWritten && _evidenceFiles.Count ==
                        ElementalPersistenceFixtureCount * 3 + 1 &&
                    _evidenceFiles.All(File.Exists) &&
                    records.All(value =>
                        value["preview"] != null &&
                        value["isometric"] != null &&
                        value["preview"].Value<int>(
                            "meaningfulPixels") > 0 &&
                        value["isometric"].Value<int>(
                            "meaningfulPixels") > 0);
            }

            private bool CharacterRacesArrayExact()
            {
                BlueprintRace[] current = BlueprintRoot.Instance.Progression
                    .CharacterRaces;
                return current.Length == _characterRacesBefore.Length &&
                    current.Select((value, index) => ReferenceEquals(value,
                        _characterRacesBefore[index])).All(value => value);
            }

            private static bool TokenBool(JObject value, string name)
            {
                return value != null && value[name] != null &&
                    value[name].Type == JTokenType.Boolean &&
                    value.Value<bool>(name);
            }

            private void CompletePhaseResult()
            {
                if (!_prepare)
                    _warnings.Add(
                        "Direct inspection of every generated image remains required for subjective persistence acceptance.");
                _warnings.Add(
                    "DollData stores race customization and palette choices; native class clothes are reconstructed separately by UnitEntityView.UpdateClassEquipment.");
                RuntimeBuildIdentity build = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                bool passed = _assertions.All(value =>
                    value.Status == RuntimeTestStatuses.Pass);
                Result = new RuntimeTestResult
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = build.RuntimeIdentity + "; mvid=" +
                        build.ModuleVersionId + "; sha256=" +
                        build.LoadedModuleSha256 + "; pid=" +
                        build.ProcessId,
                    GitCommit = build.GitCommit,
                    GameVersion = Application.version ?? string.Empty,
                    StartUtc = _started.ToString("o"),
                    EndUtc = DateTime.UtcNow.ToString("o"),
                    DurationMilliseconds = (long)(DateTime.UtcNow -
                        _started).TotalMilliseconds,
                    Assertions = _assertions,
                    Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    WorkingSaveSmoke = _workingSaveEvidence,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }
        }
    }
}
