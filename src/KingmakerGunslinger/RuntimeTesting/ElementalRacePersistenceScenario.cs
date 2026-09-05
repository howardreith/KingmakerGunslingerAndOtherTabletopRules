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
using Kingmaker.Blueprints.Classes.Selection;
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
    /// Guarded four-launch save qualification for every production elemental
    /// race, sex, visual preset, heritage, and applicable Release B feat set.
    /// Prepare creates each fixture
    /// through native character creation and a distinct native Respec
    /// replacement, then persists spent racial SLAs. The second launch runs
    /// with selector publication disabled, verifies reconstruction, level-up,
    /// and rest, re-spends the resource, and persists the fixtures. The third
    /// launch restores selector publication, performs another exact native
    /// heritage Respec, removes only the marker-bound fixtures, and saves
    /// cleanup. The final launch proves absence. KMG_AUTOMATION_BASELINE is
    /// never eligible.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string ElementalPersistenceFixtureNamePrefix =
            "KMG_ELEMENTAL_PERSISTENCE_";
        private const int ElementalPersistenceFixtureCount =
            ElementalRaceCatalog.RaceCount * 2 *
            ElementalHeritagePolicy.ChoicesPerRace;
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
            "91472289-c1d7-4558-b7ed-a5e8c06345fb",
            "ab99c4b1-6e65-4808-9ba8-7e66b4c02832",
            "fdff9dcf-6f07-499e-923c-a218d74388fd",
            "e7ca2590-a307-46cf-a5c8-a182b67845d4",
            "fb3fd14e-55f3-4b95-8e50-13a2978af655",
            "246e29b9-c728-4dbc-8a98-45ed4975868e",
            "68dd174b-fa0f-43b1-a8d9-7ea2e3b6d235",
            "9395209a-333f-4ee8-9d50-027917e6c384",
            "cfc07981-140c-47b9-bedd-4866ddb784e0",
            "a136453c-d8ff-43cc-b2b9-636f8123a5f1",
            "7c161687-525b-4ab6-ae5f-a28f475d79c6",
            "a704f6d8-eb34-4bf4-89ab-b8d379942c84",
            "9f558e70-4995-4a3d-977d-3ba24b6dcd38",
            "9b9c9397-0c61-431a-8470-276b3709039f",
            "91aa7243-8c28-4ed4-a274-2a9fa65b5a30",
            "0c9440bb-f190-471d-958c-ba538c062bee",
            "c97a0186-02de-4fe5-acfa-13ca8267baa3"
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
                BlueprintRaceVisualPreset preset,
                ElementalHeritageBlueprints heritage,
                ElementalHeritageBlueprints sourceHeritage,
                ElementalHeritageBlueprints restoredHeritage)
            {
                Index = index;
                Blueprints = blueprints ??
                    throw new ArgumentNullException("blueprints");
                Gender = gender;
                Preset = preset ?? throw new ArgumentNullException("preset");
                Heritage = heritage ?? throw new ArgumentNullException(
                    "heritage");
                SourceHeritage = sourceHeritage ??
                    throw new ArgumentNullException("sourceHeritage");
                RestoredHeritage = restoredHeritage ??
                    throw new ArgumentNullException("restoredHeritage");
                string raceAndGender = blueprints.Definition.Kind.ToString()
                    .ToLowerInvariant() + "-" + gender.ToString()
                    .ToLowerInvariant();
                Label = heritage.Definition.IsGeneral ? raceAndGender :
                    raceAndGender + "-" + heritage.Definition.Id.ToString()
                        .ToLowerInvariant();
                UniqueId = ElementalPersistenceFixtureIds[index];
                Name = ElementalPersistenceFixtureNamePrefix +
                    Label.Replace('-', '_').ToUpperInvariant();
            }

            internal int Index { get; private set; }
            internal ElementalRaceBlueprints Blueprints { get; private set; }
            internal Gender Gender { get; private set; }
            internal BlueprintRaceVisualPreset Preset { get; private set; }
            internal ElementalHeritageBlueprints Heritage
            { get; private set; }
            internal ElementalHeritageBlueprints SourceHeritage
            { get; private set; }
            internal ElementalHeritageBlueprints RestoredHeritage
            { get; private set; }
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

        internal sealed partial class ElementalRacePersistenceSession
        {
            private const int MinimumSettleUpdates = 30;
            private const int MaximumSettleUpdates = 480;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly WorkingSaveSmokeScenario _workingSaveSmoke;
            private readonly bool _prepare;
            private readonly bool _moduleRestored;
            private readonly bool _legacyMigration;
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
            private readonly JArray _respecRecords = new JArray();
            private readonly JArray _selectionRecords = new JArray();
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
            private Vector3 _fixtureStagingPosition;
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
            private UnitEntityData _respecSourceUnit;
            private BlueprintUnit _respecSourceBlueprint;
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
            private bool _preservedMembershipExact;
            private bool _creatingRespecReplacement;
            private bool _normalPathComplete;
            private bool _baselineAbsentExact;
            private bool _cleanupStarted;
            private bool _structuralCleaned;
            private bool _indexWritten;
            private bool _saveStarted;
            private bool _saveCompleted;
            private Stopwatch _saveElapsed;
            private WorkingSaveSmokeEvidence _workingSaveEvidence;
            private JObject _preSaveGate = new JObject();
            private JObject _lastCombatGuard = new JObject();
            private int _combatGuardChecks;
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
                _moduleRestored = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .ElementalRaceModuleRestoredPersistence,
                    StringComparison.Ordinal);
                _legacyMigration = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog.ElementalRaceLegacyMigration,
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
                        if (_prepare || _moduleRestored)
                        {
                            PerformNativeElementalRespec(_moduleRestored);
                            _phase = 3;
                            _settleUpdates = 0;
                            return;
                        }
                        if (_legacyMigration)
                            CaptureLegacyMigrationFixture();
                        else CaptureVerifiedFixture();
                        _fixtureIndex++;
                        if (_fixtureIndex < _fixtures.Length)
                        {
                            _phase = 1;
                            _settleUpdates = 0;
                            return;
                        }
                        if (_legacyMigration)
                        {
                            _normalPathComplete = true;
                            BeginCleanup();
                        }
                        else
                        {
                            _preservedMembershipExact =
                                LoadedMembershipExact();
                            if (!_preservedMembershipExact)
                                throw new InvalidOperationException(
                                    "The 24 module-OFF heritage fixtures did not remain in one exact serializable party and area state.");
                            if (!ReleaseLoadedFeatPersistencePause())
                                throw new InvalidOperationException(
                                    "The guarded module-OFF load-time pause was not restored before saving the cleaned fixture state.");
                            _normalPathComplete = true;
                            StartExactWorkingSave();
                        }
                    }
                    if (_phase == 3)
                    {
                        if (!PollCurrentReady()) return;
                        if (_prepare) CapturePreparedFixture();
                        else CaptureRestoredFixture();
                        _fixtureIndex++;
                        if (_fixtureIndex < _fixtures.Length)
                        {
                            _phase = 1;
                            _settleUpdates = 0;
                            return;
                        }
                        if (_prepare)
                        {
                            _stage = "activate-feat-state-before-save";
                            _preparedFeatTransientState =
                                PrepareFeatPersistenceTransientState();
                            if (!_preparedFeatTransientState.Value<bool>(
                                    "exact"))
                                throw new InvalidOperationException(
                                    "The command-created Release B transient state was not exact immediately before save.");
                            _preparedMembershipExact =
                                PreparedMembershipExact();
                            if (!_preparedMembershipExact)
                                throw new InvalidOperationException(
                                    "The 24 exact elemental heritage native-respec fixtures did not enter one serializable party and area state.");
                            _normalPathComplete = true;
                            StartExactWorkingSave();
                        }
                        else
                        {
                            _normalPathComplete = true;
                            BeginCleanup();
                        }
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
                _fixtureStagingPosition = NearestNavigable(
                    _anchor.Position + new Vector3(-5f, 0f, 3.5f));

                _blueprintSet = BlueprintBootstrap.ElementalRaces;
                if (_blueprintSet == null || _blueprintSet.Count !=
                        ElementalRaceIdentityCatalog.RaceBlueprintIdentityCount)
                    throw new InvalidOperationException(
                        "The complete registered elemental race blueprint set is unavailable.");
                InitializeFeatPersistence();
                _fixtures = BuildFixtures();
                if (_legacyMigration)
                    _fixtures = _fixtures.Take(
                        ElementalHeritagePersistenceMatrixPolicy
                            .LegacyGeneralFixtureCount(
                                ElementalRaceCatalog.RaceCount)).ToArray();
                RequireFixtureStagingOutOfCombat("baseline-before-fixtures");
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

                _registeredExact = RegisteredIdentitiesExact() &&
                    _featRegisteredExact;
                _selectorExact = SelectorStateExact() &&
                    _featSelectorExact;
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

                bool expectedModuleActive = _moduleRestored ||
                    _legacyMigration;
                if (_context.FeatureModules.Active.ElementalRaces !=
                        expectedModuleActive || !_selectorExact ||
                    !_registeredExact)
                    throw new InvalidOperationException(
                        expectedModuleActive
                        ? "Module-restored verification requires all elemental identities registered and all four selector entries published."
                        : "Module-disabled verification requires all elemental identities registered while all four selector entries are absent.");
                _loadedUnits = ResolveLoadedFixtures();
                if (!LoadedMembershipExact())
                    throw new InvalidOperationException(
                        (_legacyMigration
                            ? "Fresh-load migration verification requires the eight exact markerless 0.0.114 General elemental party fixtures; observed "
                            : "Fresh-load persistence verification requires 24 exact marker-bound elemental heritage party fixtures; observed ") +
                        _loadedFixtureMembership.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                WriteProgress(_legacyMigration
                    ? "initialized-legacy-0.0.114-verify-cleanup"
                    : _moduleRestored
                        ? "initialized-module-restored-verify-respec-cleanup"
                        : "initialized-module-disabled-verify-preserve");
            }

            private ElementalPersistenceFixture[] BuildFixtures()
            {
                if (ElementalPersistenceFixtureIds.Length !=
                        ElementalPersistenceFixtureCount ||
                    ElementalPersistenceFixtureCount !=
                        ElementalHeritagePersistenceMatrixPolicy
                            .FixtureCount(ElementalRaceCatalog.RaceCount) ||
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
                for (int heritageIndex = 0; heritageIndex <
                    ElementalHeritagePolicy.ChoicesPerRace;
                    heritageIndex++)
                {
                    for (int raceIndex = 0; raceIndex < races.Length;
                        raceIndex++)
                    {
                        ElementalRaceBlueprints race = races[raceIndex];
                        ElementalHeritageBlueprints[] choices = race
                            .Heritages.Choices().ToArray();
                        if (race.Race.Presets == null ||
                            race.Race.Presets.Length != 3 ||
                            choices.Length !=
                                ElementalHeritagePolicy.ChoicesPerRace)
                            throw new InvalidOperationException(
                                race.Definition.DisplayName +
                                " does not expose the exact heritage/preset matrix.");
                        Gender[] genders = { Gender.Male, Gender.Female };
                        for (int genderIndex = 0;
                            genderIndex < genders.Length; genderIndex++)
                        {
                            int index = result.Count;
                            int expectedIndex =
                                ElementalHeritagePersistenceMatrixPolicy
                                    .FixtureIndex(raceIndex, genderIndex,
                                        heritageIndex, races.Length);
                            if (index != expectedIndex)
                                throw new InvalidOperationException(
                                    "The stable elemental persistence fixture order drifted.");
                            BlueprintRaceVisualPreset preset =
                                race.Race.Presets[
                                    ElementalHeritagePersistenceMatrixPolicy
                                        .PresetIndex(raceIndex,
                                            genderIndex, heritageIndex,
                                            races.Length,
                                            race.Race.Presets.Length)];
                            if (preset == null || preset.Skin == null ||
                                (genders[genderIndex] == Gender.Male
                                    ? preset.MaleSkeleton == null
                                    : preset.FemaleSkeleton == null))
                                throw new InvalidOperationException(
                                    race.Definition.DisplayName + "/" +
                                    genders[genderIndex] + "/" +
                                    choices[heritageIndex].Definition.Name +
                                    " production preset is incomplete.");
                            ElementalHeritageBlueprints source = choices[
                                ElementalHeritagePersistenceMatrixPolicy
                                    .SourceHeritageIndex(heritageIndex)];
                            ElementalHeritageBlueprints restored = choices[
                                ElementalHeritagePersistenceMatrixPolicy
                                    .RestoredHeritageIndex(heritageIndex)];
                            result.Add(new ElementalPersistenceFixture(index,
                                race, genders[genderIndex], preset,
                                choices[heritageIndex], source, restored));
                        }
                    }
                }
                if (result.Count != ElementalPersistenceFixtureCount)
                    throw new InvalidOperationException(
                        "Elemental persistence requires exactly 24 race/sex/heritage fixtures.");
                return result.ToArray();
            }

            private DollData CreateExpectedDollData(
                ElementalPersistenceFixture fixture)
            {
                var state = new DollState();
                EquipmentEntityLink[] selected =
                    ConfigureExpectedDollState(state, fixture);
                DollData data = state.CreateData();
                string[] required = selected.Where(value => value != null)
                    .Select(value => value.AssetId)
                    .Where(value => !string.IsNullOrWhiteSpace(value) &&
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

            private EquipmentEntityLink[] ConfigureExpectedDollState(
                DollState state, ElementalPersistenceFixture fixture)
            {
                if (state == null)
                    throw new ArgumentNullException("state");
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
                return new[] { head, hair, eyebrows, beard, horn };
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
                BlueprintUnit source = _creatingRespecReplacement
                    ? BlueprintRoot.Instance.CustomCompanion
                    : BlueprintRoot.Instance.DefaultPlayerCharacter;
                if (source == null || source.Prefab == null ||
                    source.Body == null)
                    throw new InvalidOperationException(
                        "The native player/custom-companion donor is incomplete.");
                if (_creatingRespecReplacement)
                {
                    _currentBlueprint = source;
                }
                else
                {
                    _currentBlueprint = UnityEngine.Object.Instantiate(source);
                    _currentBlueprint.Gender = fixture.Gender;
                    _currentBlueprint.Body = CreateElementalNeutralBody(source);
                    _currentBlueprint.StartingInventory =
                        new BlueprintItem[0];
                    _currentBlueprint.name =
                        "KMG_Runtime_Elemental_Persistence_" +
                        fixture.Label.Replace('-', '_') +
                        "_Respec_Source";
                    _currentBlueprint.IsCheater = false;
                    _createdBlueprints.Add(_currentBlueprint);
                }

                UnitEntityView dollView = null;
                try
                {
                    dollView = data.CreateUnitView(false);
                    if (dollView == null ||
                        dollView.GetComponent<Character>() == null)
                        throw new InvalidOperationException(fixture.Label +
                            " DollData did not create a native Character view.");
                    dollView.Blueprint = _currentBlueprint;
                    dollView.UniqueId = _creatingRespecReplacement
                        ? fixture.UniqueId : Guid.NewGuid().ToString();
                    dollView.transform.position = _fixtureStagingPosition;
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
                RequireFixtureStagingOutOfCombat(
                    "spawn-" + fixture.Label);
                if (_creatingRespecReplacement)
                    SeedFixedElementalRespecRace(
                        _currentUnit.Descriptor, fixture);
                PrepareBaseStats(_currentUnit.Descriptor);
                _currentUnit.Descriptor.CustomGender = fixture.Gender;
                _currentUnit.Descriptor.Doll = data;
                _currentUnit.Descriptor.ForcceUseClassEquipment = true;
                _currentUnit.Descriptor.CustomName =
                    _creatingRespecReplacement ? fixture.Name :
                        fixture.Name + "_RESPEC_SOURCE";
                _currentUnit.Descriptor.State.Immortality.Retain();
                _currentUnit.Commands.InterruptAll(true);
                if (!_creatingRespecReplacement)
                {
                    ApplyNativeCharacterCreation(fixture, data);
                    _currentBlueprint.Race = fixture.Blueprints.Race;
                    _currentUnit.IsInGame = true;
                    _currentUnit.IsInFogOfWar = false;
                    _currentUnit.View.UpdateClassEquipment();
                    CurrentAvatar().RebuildOutfit();
                    _currentUnit.View.UpdateViewActive();
                    _currentUnit.View.SetVisible(true, true);
                    WriteProgress("native-respec-source-created");
                }
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
                    JObject heritageSelection = SelectNativeHeritage(
                        controller, fixture, fixture.SourceHeritage,
                        "character-creation-source");
                    _selectionRecords.Add(heritageSelection);
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
                        fixture.Blueprints.Race) != 1 ||
                    !HeritageProvidersExact(_currentUnit.Descriptor,
                        fixture, fixture.SourceHeritage, 1))
                    throw new InvalidOperationException(fixture.Label +
                        " native character creation did not commit exact race/class/heritage progression.");
            }

            private JObject SelectNativeHeritage(
                LevelUpController controller,
                ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints heritage, string phase,
                bool requireNativeRaceAction = true)
            {
                if (controller == null || controller.State == null ||
                    controller.Preview == null || fixture == null ||
                    heritage == null)
                    throw new InvalidOperationException(
                        "Native heritage selection requires an active level-up preview.");
                BlueprintFeatureSelection selection = fixture.Blueprints
                    .Heritages.Selection;
                FeatureSelectionState state = controller.State.FindSelection(
                    selection, true);
                bool statePresentBefore = state != null;
                bool nativeRaceActionExact = controller.LevelUpActions
                    .OfType<Kingmaker.UnitLogic.Class.LevelUp.Actions
                        .SelectRace>().Count(value => value != null &&
                            ReferenceEquals(value.Race,
                                fixture.Blueprints.Race)) == 1;
                bool stateContractExact = state != null &&
                    ReferenceEquals(state.Selection, selection) &&
                    ReferenceEquals(state.Source, fixture.Blueprints.Race) &&
                    state.Parent == null && state.Level == 0 &&
                    state.Index == 0 &&
                    controller.State.HasSelection(state) &&
                    ReferenceEquals(controller.State.FindSelection(
                        selection, true), state) && statePresentBefore &&
                    (!requireNativeRaceAction || nativeRaceActionExact);
                if (!stateContractExact)
                    throw new InvalidOperationException(fixture.Label +
                        " did not expose its obligatory native heritage selection during " +
                        phase + ".");
                IFeatureSelectionItem[] items = selection
                    .ExtractSelectionItems(controller.Preview,
                        controller.Preview).ToArray();
                IFeatureSelectionItem item = items.SingleOrDefault(value =>
                    value != null && ReferenceEquals(value.Feature,
                        heritage.Marker));
                bool menuExact = items.Length ==
                        ElementalHeritagePolicy.ChoicesPerRace &&
                    items.Select(value => value == null ? null :
                        value.Feature).SequenceEqual(fixture.Blueprints
                            .Heritages.Choices().Select(value =>
                                value.Marker));
                bool selectable = item != null && selection.CanSelect(
                    controller.Preview, controller.State, state, item);
                bool selected = selectable && controller.SelectFeature(
                    state, item);
                bool previewExact = selected &&
                    HeritageProvidersExact(controller.Preview, fixture,
                        heritage, 1);
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "phase", phase },
                    { "selectionGuid", selection.AssetGuid },
                    { "heritage", heritage.Definition.Name },
                    { "heritageMarkerGuid", heritage.Marker.AssetGuid },
                    { "menuCount", items.Length },
                    { "menuExact", menuExact },
                    { "statePresentBefore", statePresentBefore },
                    { "stateNativeFromRaceSelection",
                        nativeRaceActionExact },
                    { "stateContractExact", stateContractExact },
                    { "stateSourceRaceExact", ReferenceEquals(
                        state.Source, fixture.Blueprints.Race) },
                    { "stateParentAbsent", state.Parent == null },
                    { "stateLevel", state.Level },
                    { "stateIndex", state.Index },
                    { "selectable", selectable },
                    { "selected", selected },
                    { "previewExact", previewExact }
                };
                if (!stateContractExact || !menuExact || !selectable ||
                    !selected || !previewExact)
                    throw new InvalidOperationException(fixture.Label + "/" +
                        heritage.Definition.Name +
                        " failed its native heritage selection contract during " +
                        phase + ": " + record.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                record["alternateTraitSelectionsExpected"] = fixture
                    .Blueprints.AlternateTraits.Selections().Count;
                record["alternateTraitSelections"] =
                    SelectRetainedAlternateTraits(controller, fixture, phase);
                return record;
            }

            private void PerformNativeElementalRespec(bool restoredPhase)
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                ElementalHeritageBlueprints sourceHeritage = restoredPhase
                    ? fixture.Heritage : fixture.SourceHeritage;
                ElementalHeritageBlueprints targetHeritage = restoredPhase
                    ? fixture.RestoredHeritage : fixture.Heritage;
                int sourceLevel = restoredPhase ? 2 : 1;
                int sourceResource = restoredPhase ? 0 : 1;
                _stage = "native-respec-" + fixture.Label;
                ElementalPersistenceObservation sourceObservation =
                    ObserveFixture(fixture, _currentUnit,
                        _currentExpectedDoll, sourceHeritage,
                        sourceResource, sourceLevel);
                if (!sourceObservation.Exact)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec source was not an exact elemental Gunslinger: " +
                        sourceObservation.Evidence.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                if (restoredPhase)
                    CaptureRestoredSourceFeatPersistence(fixture,
                        _currentUnit);

                UnitEntityData sourceUnit = _currentUnit;
                UnitDescriptor sourceDescriptor = sourceUnit.Descriptor;
                _respecSourceUnit = _currentUnit;
                _respecSourceBlueprint = _currentBlueprint;
                string sourceActorId = _respecSourceUnit.UniqueId;
                _currentUnit = null;
                _currentBlueprint = null;
                _currentExpectedDoll = null;
                if (restoredPhase)
                {
                    DetachPersistedRespecSource(fixture);
                    RequireFixtureStagingOutOfCombat(
                        "restored-source-detached-" + fixture.Label);
                    RetireElementalRespecSource();
                }
                _creatingRespecReplacement = true;
                try
                {
                    StartPrepareFixture();
                }
                finally
                {
                    _creatingRespecReplacement = false;
                }

                int replacementLevelBeforeRespec = _currentUnit.Descriptor
                    .Progression.CharacterLevel;
                bool distinctSourceAndReplacement =
                    !ReferenceEquals(sourceUnit, _currentUnit) &&
                    !ReferenceEquals(sourceDescriptor,
                        _currentUnit.Descriptor);
                if (replacementLevelBeforeRespec != 0 ||
                    !distinctSourceAndReplacement)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec requires one distinct level-0 replacement descriptor.");

                _stage = "native-respec-commit-" + fixture.Label;
                WriteProgress("fixed-race-respec-replacement-created");
                JObject record = CommitNativeElementalRespec(fixture,
                    sourceHeritage, targetHeritage, sourceObservation,
                    sourceActorId, sourceLevel,
                    replacementLevelBeforeRespec,
                    distinctSourceAndReplacement);
                if (restoredPhase)
                    AdvanceOneGunslingerLevel(fixture);
                _currentUnit.Descriptor.ForcceUseClassEquipment = true;
                _currentUnit.IsInGame = true;
                _currentUnit.IsInFogOfWar = false;
                _currentUnit.View.UpdateClassEquipment();
                CurrentAvatar().RebuildOutfit();
                _currentUnit.View.UpdateViewActive();
                _currentUnit.View.SetVisible(true, true);
                RequireFixtureStagingOutOfCombat(
                    "respec-committed-" + fixture.Label);
                if (!restoredPhase) RetireElementalRespecSource();
                bool starterRollbackExact = RollbackStarterGrants();
                record["starterGrantsRolledBack"] =
                    starterRollbackExact;
                record["committedCharacterLevel"] =
                    _currentUnit.Descriptor.Progression.CharacterLevel;
                record["committedGunslingerLevel"] =
                    _currentUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass);
                record["committedRaceExact"] = ReferenceEquals(
                    _currentUnit.Descriptor.Progression.Race,
                    fixture.Blueprints.Race);
                record["committedCommonRaceFactsExact"] =
                    CommonElementalRaceFactsExact(
                        _currentUnit.Descriptor, fixture);
                record["finalHeritageExact"] = HeritageProvidersExact(
                    _currentUnit.Descriptor, fixture, targetHeritage, 1);
                record["sourceRetiredExact"] =
                    _respecSourceUnit == null &&
                    _respecSourceBlueprint == null;
                _respecRecords.Add(record);
                if (!starterRollbackExact ||
                    !TokenBool(record, "committedCommonRaceFactsExact") ||
                    !TokenBool(record, "finalHeritageExact") ||
                    !TokenBool(record, "sourceRetiredExact") ||
                    (int)record["committedCharacterLevel"] != sourceLevel)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec changed the exact level, heritage, identity, or inventory baseline.");
                WriteProgress("native-respec-committed");
            }

            private void DetachPersistedRespecSource(
                ElementalPersistenceFixture fixture)
            {
                int removed = 0;
                for (int index = _player.PartyCharacters.Count - 1;
                    index >= 0; index--)
                {
                    if (!string.Equals(_player.PartyCharacters[index]
                            .UniqueId, fixture.UniqueId,
                            StringComparison.Ordinal)) continue;
                    _player.PartyCharacters.RemoveAt(index);
                    removed++;
                }
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();
                if (removed != 1 || _player.PartyCharacters.Any(value =>
                        string.Equals(value.UniqueId, fixture.UniqueId,
                            StringComparison.Ordinal)))
                    throw new InvalidOperationException(fixture.Label +
                        " restored-module Respec could not detach one exact persisted source reference.");
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

            private JObject CommitNativeElementalRespec(
                ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints sourceHeritage,
                ElementalHeritageBlueprints targetHeritage,
                ElementalPersistenceObservation sourceObservation,
                string sourceActorId, int sourceLevel,
                int replacementLevelBeforeRespec,
                bool distinctSourceAndReplacement)
            {
                LevelUpController controller = null;
                bool callback = false;
                bool fixedRaceBeforeRespec = ReferenceEquals(
                    _currentUnit.Descriptor.Progression.Race,
                    fixture.Blueprints.Race);
                bool nativeCustomCompanionBlueprint = ReferenceEquals(
                    _currentUnit.Blueprint,
                    BlueprintRoot.Instance.CustomCompanion);
                JObject shellBeforeRespec =
                    DescribeFixedElementalRespecShell(
                        _currentUnit.Descriptor, fixture);
                bool fixedRaceShellBeforeRespec =
                    TokenBool(shellBeforeRespec, "exact");
                if (!nativeCustomCompanionBlueprint ||
                    !fixedRaceBeforeRespec ||
                    !fixedRaceShellBeforeRespec)
                    throw new InvalidOperationException(fixture.Label +
                        " level-0 Respec replacement did not begin as an " +
                        "exact fixed-race shell: " +
                        shellBeforeRespec.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                try
                {
                    controller = LevelUpController
                        .StartWithoutAssigningStaticInstance(
                            _currentUnit.Descriptor, false, null,
                            new Action(() => callback = true),
                            LevelUpState.CharBuildMode.Respec);
                    if (controller == null || controller.State == null ||
                        controller.State.Mode !=
                            LevelUpState.CharBuildMode.Respec ||
                        controller.Preview == null || controller.Doll == null)
                        throw new InvalidOperationException(fixture.Label +
                            " native Respec controller is incomplete.");
                    bool fixedRaceInInitialPreview = ReferenceEquals(
                        controller.Preview.Progression.Race,
                        fixture.Blueprints.Race);
                    JObject shellInInitialPreview =
                        DescribeFixedElementalRespecShell(
                            controller.Preview, fixture);
                    bool fixedRaceShellInInitialPreview =
                        TokenBool(shellInInitialPreview, "exact");
                    if (!fixedRaceInInitialPreview ||
                        !fixedRaceShellInInitialPreview ||
                        !controller.State.CanSelectRace)
                        throw new InvalidOperationException(fixture.Label +
                            " native Respec preview did not expose its fixed " +
                            "race through the native race-selection action: " +
                            shellInInitialPreview.ToString(
                                Newtonsoft.Json.Formatting.None) + ".");
                    bool fixedRaceSelected = controller.SelectRace(
                        fixture.Blueprints.Race);
                    bool fixedRaceFactsAfterRaceSelection =
                        FixedElementalRespecFactsExact(
                            controller.Preview, fixture);
                    if (!fixedRaceSelected ||
                        !fixedRaceFactsAfterRaceSelection)
                        throw new InvalidOperationException(fixture.Label +
                            " native same-race Respec selection did not " +
                            "activate the exact race facts.");
                    JObject heritageSelection = SelectNativeHeritage(
                        controller, fixture, targetHeritage,
                        "native-respec-target");
                    _selectionRecords.Add(heritageSelection);
                    Ability selectedSla = controller.Preview.Abilities
                        .GetAbility(targetHeritage.SlaAbility);
                    int selectedSlaResourceBeforeCommit = controller.Preview
                        .Resources.GetResourceAmount(
                            targetHeritage.SlaResource);
                    AbilityData selectedSlaData = selectedSla == null
                        ? null : new AbilityData(selectedSla);
                    AbilityData selectedExecutable = selectedSlaData == null
                        ? null : ResolveExecutableAbility(selectedSlaData);
                    bool selectedSlaAvailableBeforeCommit =
                        selectedExecutable != null &&
                        selectedExecutable.IsAvailable &&
                        selectedExecutable.GetAvailableForCastCount() == 1;
                    if (!controller.SelectClass(_gunslingerClass, false))
                        throw new InvalidOperationException(fixture.Label +
                            " native Respec Gunslinger selection was rejected.");
                    ConfigureExpectedDollState(controller.Doll, fixture);
                    controller.ApplyClassMechanics();
                    int previewLevel = controller.Preview.Progression
                        .CharacterLevel;
                    int previewClassLevel = controller.Preview.Progression
                        .GetClassLevel(_gunslingerClass);
                    bool previewRaceIdentityExact = ReferenceEquals(
                            controller.Preview.Progression.Race,
                            fixture.Blueprints.Race);
                    int previewRaceFactRank = controller.Preview.Progression
                        .Features.GetRank(fixture.Blueprints.Race);
                    bool previewFixedRaceFactsExact =
                        FixedElementalRespecFactsExact(controller.Preview,
                            fixture);
                    bool previewCommonRaceFactsExact =
                        CommonElementalRaceFactsExact(controller.Preview,
                            fixture);
                    bool previewHeritageProvidersExact =
                        HeritageProvidersExact(controller.Preview, fixture,
                            targetHeritage, 1);
                    // The fixed-shell predicate intentionally includes the
                    // inherited General providers and becomes false after a
                    // valid alternate selection. From this point onward the
                    // semantic contracts are common facts plus the selected
                    // heritage providers.
                    bool previewRaceExact = previewRaceIdentityExact &&
                        previewRaceFactRank == 1 &&
                        previewCommonRaceFactsExact &&
                        previewHeritageProvidersExact;
                    var record = new JObject
                    {
                        { "fixture", fixture.Label },
                        { "sourceHeritage",
                            sourceHeritage.Definition.Name },
                        { "targetHeritage",
                            targetHeritage.Definition.Name },
                        { "sourceLevel", sourceLevel },
                        { "sourceActorId", sourceActorId },
                        { "replacementActorId", _currentUnit.UniqueId },
                        { "sourceObservationExact",
                            sourceObservation.Exact },
                        { "replacementLevelBeforeRespec",
                            replacementLevelBeforeRespec },
                        { "distinctSourceAndReplacement",
                            distinctSourceAndReplacement },
                        { "respecMode", "Respec" },
                        { "nativeCustomCompanionBlueprint",
                            nativeCustomCompanionBlueprint },
                        { "fixedRaceBeforeRespec",
                            fixedRaceBeforeRespec },
                        { "fixedRaceShellBeforeRespec",
                            fixedRaceShellBeforeRespec },
                        { "shellBeforeRespec",
                            shellBeforeRespec.DeepClone() },
                        { "fixedRaceInInitialPreview",
                            fixedRaceInInitialPreview },
                        { "fixedRaceShellInInitialPreview",
                            fixedRaceShellInInitialPreview },
                        { "shellInInitialPreview",
                            shellInInitialPreview.DeepClone() },
                        { "raceSelectionAvailable", true },
                        { "fixedRaceSelected", fixedRaceSelected },
                        { "fixedRaceFactsAfterRaceSelection",
                            fixedRaceFactsAfterRaceSelection },
                        { "heritageSelection",
                            heritageSelection.DeepClone() },
                        { "heritageSelectionExact",
                            NativeSelectionRecordExact(
                                heritageSelection) },
                        { "selectedSlaResourceBeforeCommit",
                            selectedSlaResourceBeforeCommit },
                        { "selectedSlaAvailableBeforeCommit",
                            selectedSlaAvailableBeforeCommit },
                        { "racePreserved", previewRaceExact },
                        { "classSelected", true },
                        { "previewRaceExact", previewRaceExact },
                        { "previewRaceIdentityExact",
                            previewRaceIdentityExact },
                        { "previewRaceFactRank", previewRaceFactRank },
                        { "previewFixedRaceFactsExact",
                            previewFixedRaceFactsExact },
                        { "previewCommonRaceFactsExact",
                            previewCommonRaceFactsExact },
                        { "previewHeritageProvidersExact",
                            previewHeritageProvidersExact },
                        { "previewCharacterLevel", previewLevel },
                        { "previewGunslingerLevel", previewClassLevel },
                        { "callback", false }
                    };
                    if (!previewRaceExact || previewLevel != 1 ||
                        previewClassLevel != 1 ||
                        selectedSlaResourceBeforeCommit != 1 ||
                        !selectedSlaAvailableBeforeCommit)
                        throw new InvalidOperationException(fixture.Label +
                            " native Respec preview diverged before Commit: " +
                            record.ToString(
                                Newtonsoft.Json.Formatting.None) + ".");
                    controller.Commit();
                    controller = null;
                    record["callback"] = callback;
                    return record;
                }
                finally
                {
                    if (controller != null) controller.Cancel();
                }
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

            private static void SeedFixedElementalRespecRace(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                owner.Progression.SetRace(fixture.Blueprints.Race);
            }

            private static bool FixedElementalRespecShellExact(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                return TokenBool(DescribeFixedElementalRespecShell(owner,
                    fixture), "exact");
            }

            private static JObject DescribeFixedElementalRespecShell(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                var result = new JObject
                {
                    { "ownerPresent", owner != null },
                    { "fixturePresent", fixture != null }
                };
                if (owner == null || fixture == null)
                {
                    result["exact"] = false;
                    return result;
                }

                BlueprintRace race = fixture.Blueprints.Race;
                ElementalHeritageBlueprints[] choices = fixture.Blueprints
                    .Heritages.Choices().ToArray();
                ElementalHeritageBlueprints general = fixture.Blueprints
                    .Heritages.General;
                bool raceIdentityExact = ReferenceEquals(
                    owner.Progression.Race, race);
                bool nativeCustomCompanionBlueprint = ReferenceEquals(
                    owner.Blueprint, BlueprintRoot.Instance.CustomCompanion);
                bool raceFactPresent = owner.HasFact(race);
                int raceFactRank = owner.Progression.Features.GetRank(race);
                var raceFeatures = new JArray((race.Features ??
                    Array.Empty<BlueprintFeatureBase>()).Select(value =>
                {
                    BlueprintFeature feature = value as BlueprintFeature;
                    return new JObject
                    {
                        { "guid", value == null ? "" : value.AssetGuid },
                        { "name", value == null ? "" : value.name },
                        { "type", value == null ? "" :
                            value.GetType().FullName },
                        { "present", value != null && owner.HasFact(value) },
                        { "rank", feature == null ? -1 :
                            owner.Progression.Features.GetRank(feature) }
                    };
                }));
                bool raceFeatureRanksValid = (race.Features ??
                    Array.Empty<BlueprintFeatureBase>()).All(value =>
                {
                    BlueprintFeature feature = value as BlueprintFeature;
                    int rank = feature == null ? 0 :
                        owner.Progression.Features.GetRank(feature);
                    return value != null && rank >= 0 && rank <= 1;
                });
                bool markersAbsent = choices.All(value =>
                    owner.Progression.Features.GetRank(value.Marker) == 0);
                bool generalProvidersExact = HeritageProvidersExact(owner,
                    fixture, general, 0);
                int generalResourceAmount = owner.Resources.GetResourceAmount(
                    general.SlaResource);
                var providers = new JArray(choices.Select(value =>
                    new JObject
                    {
                        { "heritage", value.Definition.Name },
                        { "markerRank", owner.Progression.Features.GetRank(
                            value.Marker) },
                        { "affinityRank", owner.Progression.Features.GetRank(
                            value.Affinity) },
                        { "slaFeatureRank",
                            owner.Progression.Features.GetRank(
                                value.SlaFeature) },
                        { "abilityPresent", owner.Abilities.GetAbility(
                            value.SlaAbility) != null },
                        { "resourcePresent", owner.Resources
                            .PersistantResources.Any(resource =>
                                resource != null && ReferenceEquals(
                                    resource.Blueprint,
                                    value.SlaResource)) },
                        { "resourceAmount", owner.Resources.GetResourceAmount(
                            value.SlaResource) }
                    }));
                bool exact = nativeCustomCompanionBlueprint &&
                    raceIdentityExact && raceFactPresent &&
                    raceFactRank == 1 && raceFeatureRanksValid &&
                    markersAbsent && generalProvidersExact &&
                    generalResourceAmount == 1;
                result["raceGuid"] = race.AssetGuid;
                result["nativeCustomCompanionBlueprint"] =
                    nativeCustomCompanionBlueprint;
                result["raceIdentityExact"] = raceIdentityExact;
                result["raceFactPresent"] = raceFactPresent;
                result["raceFactRank"] = raceFactRank;
                result["raceFeatureRanksValid"] = raceFeatureRanksValid;
                result["markersAbsent"] = markersAbsent;
                result["generalProvidersExact"] = generalProvidersExact;
                result["generalResourceAmount"] = generalResourceAmount;
                result["raceFeatures"] = raceFeatures;
                result["providers"] = providers;
                result["exact"] = exact;
                return result;
            }

            private static bool FixedElementalRespecFactsExact(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                BlueprintRace race = fixture.Blueprints.Race;
                return owner != null &&
                    ReferenceEquals(owner.Progression.Race, race) &&
                    owner.HasFact(race) &&
                    owner.Progression.Features.GetRank(race) == 1 &&
                    race.Features != null &&
                    race.Features.All(value => value != null &&
                        owner.HasFact(value) &&
                        (!(value is BlueprintFeature) ||
                            owner.Progression.Features.GetRank(
                                (BlueprintFeature)value) == 1));
            }

            private static bool CommonElementalRaceFactsExact(
                UnitDescriptor owner, ElementalPersistenceFixture fixture,
                bool heritageSelectionExpected = true)
            {
                if (owner == null || fixture == null) return false;
                BlueprintRace race = fixture.Blueprints.Race;
                return race.Features != null && race.Features.Where(value =>
                        !ReferenceEquals(value,
                            fixture.Blueprints.Affinity) &&
                        !ReferenceEquals(value,
                            fixture.Blueprints.SlaFeature) &&
                        !ReferenceEquals(value,
                            fixture.Blueprints.Heritages.Selection)).All(value =>
                            value != null && owner.HasFact(value) &&
                        (!(value is BlueprintFeature) ||
                            owner.Progression.Features.GetRank(
                                (BlueprintFeature)value) == 1)) &&
                    owner.Progression.Features.GetRank(
                        fixture.Blueprints.Heritages.Selection) ==
                            (heritageSelectionExpected ? 1 : 0);
            }

            private static bool HeritageProvidersExact(UnitDescriptor owner,
                ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints desired, int markerCount)
            {
                if (owner == null || fixture == null || desired == null)
                    return false;
                ElementalHeritageBlueprints[] choices = fixture.Blueprints
                    .Heritages.Choices().ToArray();
                var resources = new HashSet<BlueprintAbilityResource>(
                    choices.Select(value => value.SlaResource));
                var activeResources = owner.Resources.PersistantResources
                    .Where(value => value != null && resources.Contains(
                        value.Blueprint)).ToArray();
                return choices.All(value => owner.Progression.Features
                        .GetRank(value.Marker) ==
                            (ReferenceEquals(value, desired)
                                ? markerCount : 0)) &&
                    choices.All(value => owner.Progression.Features.GetRank(
                        value.Affinity) ==
                            (ReferenceEquals(value, desired) ? 1 : 0)) &&
                    choices.All(value => owner.Progression.Features.GetRank(
                        value.SlaFeature) ==
                            (ReferenceEquals(value, desired) ? 1 : 0)) &&
                    choices.Count(value => owner.Abilities.GetAbility(
                        value.SlaAbility) != null) == 1 &&
                    owner.Abilities.GetAbility(desired.SlaAbility) != null &&
                    activeResources.Length == 1 && ReferenceEquals(
                        activeResources[0].Blueprint,
                        desired.SlaResource);
            }

            private static bool NativeSelectionRecordExact(JObject value)
            {
                return value != null &&
                    value.Value<int>("menuCount") ==
                        ElementalHeritagePolicy.ChoicesPerRace &&
                    TokenBool(value, "menuExact") &&
                    TokenBool(value, "stateContractExact") &&
                    TokenBool(value, "stateSourceRaceExact") &&
                    TokenBool(value, "stateParentAbsent") &&
                    value.Value<int>("stateLevel") == 0 &&
                    value.Value<int>("stateIndex") == 0 &&
                    TokenBool(value, "statePresentBefore") &&
                    TokenBool(value, "stateNativeFromRaceSelection") &&
                    TokenBool(value, "selectable") &&
                    TokenBool(value, "selected") &&
                    TokenBool(value, "previewExact") &&
                    NativeRetainedSelectionsExact(value);
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
                RequireFixtureStagingOutOfCombat(
                    "settle-" + fixture.Label);
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
                JObject featPersistence = PrepareFeatPersistenceFixture(
                    fixture, _currentUnit);
                ElementalPersistenceObservation observation = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 1, 1);
                AbilityData ability = RequireAbility(_currentUnit,
                    fixture.Heritage.SlaAbility);
                AbilityData executable = ResolveExecutableAbility(ability);
                int before = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                JObject respec = _respecRecords.OfType<JObject>().Last();
                respec["replacementObservationExact"] =
                    observation.Exact;
                respec["replacementDollExact"] =
                    _currentExpectedDoll.Matches(
                        _currentUnit.Descriptor.Doll);
                respec["replacementResourceBeforeSpend"] = before;
                respec["stableIdentityExact"] =
                    IsFixtureUnit(_currentUnit, fixture);
                respec["sourceRetiredExact"] =
                    _respecSourceUnit == null &&
                    _respecSourceBlueprint == null;
                respec["serializedClassClothesAbsent"] =
                    SerializedElementalClassClothesAbsent(
                        _currentUnit.Descriptor.Doll);
                respec["persistedActorIdentityExpected"] = false;
                respec["actorIdentityExact"] =
                    ElementalHeritagePersistenceMatrixPolicy
                        .RespecActorIdentityExact(false,
                            (string)respec["sourceActorId"],
                            (string)respec["replacementActorId"],
                            (bool)respec["distinctSourceAndReplacement"]);
                bool nativeRespecExact =
                    NativeElementalRespecRecordExact(respec, false);
                InvokeAbilitySpend(ability,
                    fixture.Heritage.SlaResource);
                int after = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                bool spendExact = before == 1 && after == 0 &&
                    executable.GetAvailableForCastCount() == 0 &&
                    !executable.IsAvailable;
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "heritage", fixture.Heritage.Definition.Name },
                    { "heritageMarkerGuid",
                        fixture.Heritage.Marker.AssetGuid },
                    { "phase", "prepare" },
                    { "observation", observation.Evidence },
                    { "observationExact", observation.Exact },
                    { "nativeRespec", respec.DeepClone() },
                    { "nativeRespecExact", nativeRespecExact },
                    { "resourceBeforeSpend", before },
                    { "resourceAfterSpend", after },
                    { "executableAbilityGuid",
                        executable.Blueprint.AssetGuid },
                    { "executableAvailableCountAfterSpend",
                        executable.GetAvailableForCastCount() },
                    { "executableAvailableAfterSpend",
                        executable.IsAvailable },
                    { "spendExact", spendExact }
                };
                record["featPersistence"] = featPersistence.DeepClone();
                record["featPersistenceExact"] =
                    featPersistence.Value<bool>("exact");
                CaptureFixture(record, fixture, _currentUnit);
                if (!observation.Exact || !nativeRespecExact ||
                    !spendExact ||
                    !featPersistence.Value<bool>("exact"))
                    throw new InvalidOperationException(fixture.Label +
                        " did not satisfy the exact native-respec, pre-save rules, Release B feat, visual, and spent-SLA contract.");
                PromoteCurrentFixture(fixture);
                WriteProgress("prepared-fixture-promoted");
            }

            private static bool NativeElementalRespecRecordExact(
                JObject value, bool persistedSource)
            {
                return value != null &&
                    (bool)value["persistedActorIdentityExpected"] ==
                        persistedSource &&
                    (bool)value["actorIdentityExact"] &&
                    (bool)value["sourceObservationExact"] &&
                    !string.Equals((string)value["sourceHeritage"],
                        (string)value["targetHeritage"],
                        StringComparison.Ordinal) &&
                    (bool)value["distinctSourceAndReplacement"] &&
                    (int)value["replacementLevelBeforeRespec"] == 0 &&
                    string.Equals((string)value["respecMode"], "Respec",
                        StringComparison.Ordinal) &&
                    (bool)value["fixedRaceBeforeRespec"] &&
                    (bool)value["fixedRaceShellBeforeRespec"] &&
                    (bool)value["fixedRaceInInitialPreview"] &&
                    (bool)value["fixedRaceShellInInitialPreview"] &&
                    (bool)value["raceSelectionAvailable"] &&
                    (bool)value["fixedRaceSelected"] &&
                    (bool)value["fixedRaceFactsAfterRaceSelection"] &&
                    (bool)value["heritageSelectionExact"] &&
                    (int)value["selectedSlaResourceBeforeCommit"] == 1 &&
                    (bool)value["selectedSlaAvailableBeforeCommit"] &&
                    (bool)value["racePreserved"] &&
                    (bool)value["classSelected"] &&
                    (bool)value["previewRaceExact"] &&
                    (bool)value["previewCommonRaceFactsExact"] &&
                    (int)value["previewCharacterLevel"] == 1 &&
                    (int)value["previewGunslingerLevel"] == 1 &&
                    (bool)value["callback"] &&
                    (bool)value["starterGrantsRolledBack"] &&
                    (int)value["committedCharacterLevel"] ==
                        (int)value["sourceLevel"] &&
                    (int)value["committedGunslingerLevel"] ==
                        (int)value["sourceLevel"] &&
                    (bool)value["committedRaceExact"] &&
                    (bool)value["committedCommonRaceFactsExact"] &&
                    (bool)value["finalHeritageExact"] &&
                    (bool)value["replacementObservationExact"] &&
                    (bool)value["replacementDollExact"] &&
                    (int)value["replacementResourceBeforeSpend"] == 1 &&
                    (bool)value["stableIdentityExact"] &&
                    (bool)value["sourceRetiredExact"] &&
                    (bool)value["serializedClassClothesAbsent"];
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
                RequireFixtureStagingOutOfCombat(
                    "promote-" + fixture.Label);
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
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 1);
                JObject loadedFeatPersistence = ObserveFeatPersistence(
                    fixture, _currentUnit, true, true,
                    "module-off-loaded-before-level-up");
                AbilityData abilityBeforeRest = RequireAbility(_currentUnit,
                    fixture.Heritage.SlaAbility);
                AbilityData executableBeforeRest =
                    ResolveExecutableAbility(abilityBeforeRest);
                int casterLevelBeforeRest = executableBeforeRest
                    .CreateExecutionContext(new TargetWrapper(_currentUnit))
                    .Params.CasterLevel;
                AdvanceOneGunslingerLevel(fixture);
                _currentUnit.View.UpdateClassEquipment();
                CurrentAvatar().RebuildOutfit();
                ElementalPersistenceObservation advanced = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 2);
                JObject advancedFeatPersistence = ObserveFeatPersistence(
                    fixture, _currentUnit, true, true,
                    "module-off-after-level-up");
                AbilityData abilityAfterLevel = RequireAbility(_currentUnit,
                    fixture.Heritage.SlaAbility);
                AbilityData executableAfterLevel =
                    ResolveExecutableAbility(abilityAfterLevel);
                int casterLevelAfterLevel = executableAfterLevel
                    .CreateExecutionContext(new TargetWrapper(_currentUnit))
                    .Params.CasterLevel;
                int resourceAfterSpentLevelUp = _currentUnit.Descriptor
                    .Resources.GetResourceAmount(
                        fixture.Heritage.SlaResource);
                bool levelExact =
                    _currentUnit.Descriptor.Progression.CharacterLevel == 2 &&
                    _currentUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) == 2 &&
                    ReferenceEquals(_currentUnit.Descriptor.Progression.Race,
                        fixture.Blueprints.Race) &&
                    casterLevelBeforeRest == 1 &&
                    casterLevelAfterLevel == 2 &&
                    resourceAfterSpentLevelUp == 0 &&
                    !executableAfterLevel.IsAvailable &&
                    executableAfterLevel.GetAvailableForCastCount() == 0 &&
                    _currentExpectedDoll.Matches(
                        _currentUnit.Descriptor.Doll);

                JObject cleanedFeatPersistence =
                    RemoveFeatPersistenceShortEffects(fixture,
                        _currentUnit);
                Kingmaker.Controllers.Rest.RestController.ApplyRest(
                    _currentUnit.Descriptor);
                int resourceAfterRest = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                AbilityData abilityAfterRest = RequireAbility(_currentUnit,
                    fixture.Heritage.SlaAbility);
                AbilityData executableAfterRest =
                    ResolveExecutableAbility(abilityAfterRest);
                ElementalPersistenceObservation restored = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 1, 2);
                JObject restoredFeatPersistence = ObserveFeatPersistence(
                    fixture, _currentUnit, true, false,
                    "module-off-after-rest");
                bool restExact = resourceAfterRest == 1 &&
                    executableAfterRest.IsAvailable &&
                    executableAfterRest.GetAvailableForCastCount() == 1 &&
                    restored.Exact;
                InvokeAbilitySpend(abilityAfterRest,
                    fixture.Heritage.SlaResource);
                int resourceAfterRespend = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                ElementalPersistenceObservation preserved = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 2);
                JObject preservedFeatPersistence = ObserveFeatPersistence(
                    fixture, _currentUnit, true, false,
                    "module-off-after-resource-respend");
                bool respendExact = resourceAfterRespend == 0 &&
                    !executableAfterRest.IsAvailable &&
                    executableAfterRest.GetAvailableForCastCount() == 0 &&
                    preserved.Exact;
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "heritage", fixture.Heritage.Definition.Name },
                    { "heritageMarkerGuid",
                        fixture.Heritage.Marker.AssetGuid },
                    { "phase", "module-disabled-verify-preserve" },
                    { "loadedDollExactBeforeReconstruction",
                        _currentLoadedDollExact },
                    { "loadedObservation", loaded.Evidence },
                    { "loadedObservationExact", loaded.Exact },
                    { "casterLevelBeforeRest", casterLevelBeforeRest },
                    { "resourceAfterSpentLevelUp",
                        resourceAfterSpentLevelUp },
                    { "advancedObservation", advanced.Evidence },
                    { "advancedObservationExact", advanced.Exact },
                    { "casterLevelAfterLevel", casterLevelAfterLevel },
                    { "levelUpExact", levelExact },
                    { "resourceAfterRest", resourceAfterRest },
                    { "restoredObservation", restored.Evidence },
                    { "restoredObservationExact", restored.Exact },
                    { "restExact", restExact },
                    { "resourceAfterRespend", resourceAfterRespend },
                    { "preservedObservation", preserved.Evidence },
                    { "preservedObservationExact", preserved.Exact },
                    { "respendExact", respendExact }
                };
                record["loadedFeatPersistence"] =
                    loadedFeatPersistence.DeepClone();
                record["loadedFeatPersistenceExact"] =
                    loadedFeatPersistence.Value<bool>("exact");
                record["advancedFeatPersistence"] =
                    advancedFeatPersistence.DeepClone();
                record["advancedFeatPersistenceExact"] =
                    advancedFeatPersistence.Value<bool>("exact");
                record["cleanedFeatPersistence"] =
                    cleanedFeatPersistence.DeepClone();
                record["cleanedFeatPersistenceExact"] =
                    cleanedFeatPersistence.Value<bool>("exact");
                record["restoredFeatPersistence"] =
                    restoredFeatPersistence.DeepClone();
                record["restoredFeatPersistenceExact"] =
                    restoredFeatPersistence.Value<bool>("exact");
                record["preservedFeatPersistence"] =
                    preservedFeatPersistence.DeepClone();
                record["preservedFeatPersistenceExact"] =
                    preservedFeatPersistence.Value<bool>("exact");
                CaptureFixture(record, fixture, _currentUnit);
                if (!_currentLoadedDollExact || !loaded.Exact ||
                    !restExact || !advanced.Exact || !levelExact ||
                    !respendExact ||
                    !loadedFeatPersistence.Value<bool>("exact") ||
                    !advancedFeatPersistence.Value<bool>("exact") ||
                    !cleanedFeatPersistence.Value<bool>("exact") ||
                    !restoredFeatPersistence.Value<bool>("exact") ||
                    !preservedFeatPersistence.Value<bool>("exact"))
                    throw new InvalidOperationException(fixture.Label +
                        " did not satisfy exact module-OFF load, Release B feat persistence, spent level-up, rest, re-spend, and visual reconstruction contracts.");
                _currentUnit = null;
                _currentExpectedDoll = null;
                _currentLoadedDollExact = false;
                WriteProgress("loaded-fixture-verified");
            }

            private void CaptureLegacyMigrationFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "capture-legacy-0.0.114-" + fixture.Label;
                if (!fixture.Heritage.Definition.IsGeneral ||
                    !ReferenceEquals(fixture.Heritage,
                        fixture.Blueprints.Heritages.General))
                    throw new InvalidOperationException(fixture.Label +
                        " is not an exact General heritage migration fixture.");

                ElementalPersistenceObservation loaded = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 1, 0, false);
                int resourceBeforeReconcile = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                bool reconcileAccepted = ElementalHeritageRuntime.Reconcile(
                    _currentUnit.Descriptor, null, null);
                ElementalPersistenceObservation reconciled = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 1, 0, false);
                int resourceAfterReconcile = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(fixture.Heritage.SlaResource);
                bool legacyIdentityExact =
                    ReferenceEquals(fixture.Heritage.Affinity,
                        fixture.Blueprints.Affinity) &&
                    ReferenceEquals(fixture.Heritage.SlaFeature,
                        fixture.Blueprints.SlaFeature) &&
                    ReferenceEquals(fixture.Heritage.SlaResource,
                        fixture.Blueprints.SlaResource) &&
                    ReferenceEquals(fixture.Heritage.SlaAbility,
                        fixture.Blueprints.SlaAbility);
                bool markerlessGeneralExact =
                    !_currentUnit.Descriptor.HasFact(
                        fixture.Blueprints.Heritages.Selection) &&
                    fixture.Blueprints.Heritages.Choices().All(value =>
                        !_currentUnit.Descriptor.HasFact(value.Marker));
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "heritage", fixture.Heritage.Definition.Name },
                    { "phase", "legacy-0.0.114-load-verify-cleanup" },
                    { "producerVersion", "0.0.114" },
                    { "receiverVersion", "0.0.115" },
                    { "loadedDollExactBeforeReconstruction",
                        _currentLoadedDollExact },
                    { "loadedObservation", loaded.Evidence },
                    { "loadedObservationExact", loaded.Exact },
                    { "legacyIdentityExact", legacyIdentityExact },
                    { "markerlessGeneralExact", markerlessGeneralExact },
                    { "resourceBeforeReconcile", resourceBeforeReconcile },
                    { "reconcileAccepted", reconcileAccepted },
                    { "resourceAfterReconcile", resourceAfterReconcile },
                    { "reconciledObservation", reconciled.Evidence },
                    { "reconciledObservationExact", reconciled.Exact }
                };
                CaptureFixture(record, fixture, _currentUnit);
                if (!_currentLoadedDollExact || !loaded.Exact ||
                    !legacyIdentityExact || !markerlessGeneralExact ||
                    resourceBeforeReconcile != 0 || !reconcileAccepted ||
                    resourceAfterReconcile != 0 || !reconciled.Exact)
                    throw new InvalidOperationException(fixture.Label +
                        " did not preserve exact markerless General race, stats, providers, spent SLA, or appearance across the 0.0.114-to-0.0.115 load boundary.");
                _currentUnit = null;
                _currentExpectedDoll = null;
                _currentLoadedDollExact = false;
                WriteProgress("legacy-0.0.114-fixture-verified");
            }

            private void CaptureRestoredFixture()
            {
                ElementalPersistenceFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "capture-restored-respec-" + fixture.Label;
                ElementalPersistenceObservation observation = ObserveFixture(
                    fixture, _currentUnit, _currentExpectedDoll,
                    fixture.RestoredHeritage, 1, 2);
                JObject replacementFeatPersistence = ObserveFeatPersistence(
                    fixture, _currentUnit, false, false,
                    "module-restored-native-respec-replacement");
                int resource = _currentUnit.Descriptor.Resources
                    .GetResourceAmount(
                        fixture.RestoredHeritage.SlaResource);
                JObject respec = _respecRecords.OfType<JObject>().Last();
                respec["replacementObservationExact"] = observation.Exact;
                respec["replacementDollExact"] = _currentExpectedDoll
                    .Matches(_currentUnit.Descriptor.Doll);
                respec["replacementResourceBeforeSpend"] = resource;
                respec["stableIdentityExact"] = IsFixtureUnit(
                    _currentUnit, fixture);
                respec["sourceRetiredExact"] =
                    _respecSourceUnit == null &&
                    _respecSourceBlueprint == null;
                respec["serializedClassClothesAbsent"] =
                    SerializedElementalClassClothesAbsent(
                        _currentUnit.Descriptor.Doll);
                respec["persistedActorIdentityExpected"] = true;
                respec["actorIdentityExact"] =
                    ElementalHeritagePersistenceMatrixPolicy
                        .RespecActorIdentityExact(true,
                            (string)respec["sourceActorId"],
                            (string)respec["replacementActorId"],
                            (bool)respec["distinctSourceAndReplacement"]);
                bool nativeRespecExact =
                    NativeElementalRespecRecordExact(respec, true);
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "fixtureUniqueId", fixture.UniqueId },
                    { "fixtureName", fixture.Name },
                    { "raceGuid", fixture.Blueprints.Race.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "sourceHeritage",
                        fixture.Heritage.Definition.Name },
                    { "targetHeritage",
                        fixture.RestoredHeritage.Definition.Name },
                    { "phase", "module-restored-verify-respec-cleanup" },
                    { "loadedDollExactBeforeReconstruction",
                        _currentLoadedDollExact },
                    { "observation", observation.Evidence },
                    { "observationExact", observation.Exact },
                    { "nativeRespec", respec.DeepClone() },
                    { "nativeRespecExact", nativeRespecExact },
                    { "resourceAfterRespec", resource }
                };
                record["sourceFeatPersistence"] =
                    _restoredSourceFeatObservation == null
                        ? JValue.CreateNull() :
                        _restoredSourceFeatObservation.DeepClone();
                record["sourceFeatPersistenceExact"] =
                    _restoredSourceFeatObservation != null &&
                    _restoredSourceFeatObservation.Value<bool>("exact");
                record["replacementFeatPersistence"] =
                    replacementFeatPersistence.DeepClone();
                record["replacementFeatPersistenceExact"] =
                    replacementFeatPersistence.Value<bool>("exact");
                CaptureFixture(record, fixture, _currentUnit);
                if (!_currentLoadedDollExact || !observation.Exact ||
                    !nativeRespecExact || resource != 1 ||
                    _restoredSourceFeatObservation == null ||
                    !_restoredSourceFeatObservation.Value<bool>("exact") ||
                    !replacementFeatPersistence.Value<bool>("exact"))
                    throw new InvalidOperationException(fixture.Label +
                        " did not satisfy exact module-ON restoration, Release B feat source persistence, and native heritage Respec cleanup contracts.");
                _restoredSourceFeatObservation = null;
                PromoteCurrentFixture(fixture);
                WriteProgress("restored-respec-fixture-promoted");
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
                ElementalHeritageBlueprints expectedHeritage,
                int expectedResource, int expectedLevel,
                int expectedMarkerCount = 1,
                bool heritageSelectionExpected = true)
            {
                if (unit == null || unit.Descriptor == null ||
                    unit.View == null || expectedDoll == null ||
                    expectedHeritage == null)
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
                ElementalHeritageBlueprints[] heritageChoices = fixture
                    .Blueprints.Heritages.Choices().ToArray();
                bool commonFactsExact = CommonElementalRaceFactsExact(owner,
                    fixture, heritageSelectionExpected);
                bool providersExact = HeritageProvidersExact(owner,
                    fixture, expectedHeritage, expectedMarkerCount);
                bool retainedTraitsExact = _legacyMigration ||
                    RetainedAlternateTraitsExact(owner, fixture);
                bool factsExact = commonFactsExact && providersExact &&
                    retainedTraitsExact &&
                    owner.HasFact(fixture.Blueprints.Resistance) &&
                    owner.Progression.Features.GetRank(
                        expectedHeritage.Marker) == expectedMarkerCount &&
                    owner.HasFact(expectedHeritage.Affinity) &&
                    owner.HasFact(expectedHeritage.SlaFeature);
                var statDeltas = new JObject();
                bool statExact = true;
                foreach (ElementalHeritageStat stat in Enum.GetValues(
                    typeof(ElementalHeritageStat)))
                {
                    ModifiableValue value = owner.Stats.GetStat(
                        PersistenceStatType(stat));
                    int expected = expectedHeritage.Definition.ModifierFor(
                        stat);
                    int actual = value.ModifiedValue - value.BaseValue;
                    statDeltas[stat.ToString()] = new JObject
                    {
                        { "base", value.BaseValue },
                        { "expectedRacialDelta", expected },
                        { "actualDelta", actual },
                        { "final", value.ModifiedValue }
                    };
                    statExact &= value.BaseValue == 10 &&
                        actual == expected &&
                        value.ModifiedValue == 10 + expected;
                }
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
                    expectedHeritage.SlaResource);
                bool resourceExact =
                    expectedHeritage.SlaResource.GetMaxAmount(owner) == 1 &&
                    resource == expectedResource;
                AbilityData ability = RequireAbility(unit,
                    expectedHeritage.SlaAbility);
                AbilityData executableAbility =
                    ResolveExecutableAbility(ability);
                int casterLevel = executableAbility.CreateExecutionContext(
                    new TargetWrapper(unit)).Params.CasterLevel;
                bool executableAbilityExact = ReferenceEquals(
                        executableAbility.Blueprint, ability.Blueprint) ||
                    ReferenceEquals(executableAbility.Blueprint.Parent,
                        ability.Blueprint);
                bool blueprintSupportsSpend = unit.Blueprint != null &&
                    !unit.Blueprint.IsCheater;
                bool abilityExact = blueprintSupportsSpend &&
                    executableAbilityExact &&
                    ability.Blueprint.Type ==
                        AbilityType.SpellLike && ability.Spellbook == null &&
                    executableAbility.Blueprint.Type ==
                        AbilityType.SpellLike &&
                    executableAbility.Spellbook == null &&
                    !ability.RequireMaterialComponent &&
                    !ability.IsAffectedByArcaneSpellFailure &&
                    !executableAbility.RequireMaterialComponent &&
                    !executableAbility.IsAffectedByArcaneSpellFailure &&
                    executableAbility.GetAvailableForCastCount() ==
                        expectedResource &&
                    executableAbility.IsAvailable ==
                        (expectedResource > 0) &&
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
                        { "commonFactsExact", commonFactsExact },
                        { "providersExact", providersExact },
                        { "retainedAlternateTraitsExact", retainedTraitsExact },
                        { "retainedAlternateTraitsExpected", !_legacyMigration },
                        { "heritage", expectedHeritage.Definition.Name },
                        { "heritageMarkerGuid",
                            expectedHeritage.Marker.AssetGuid },
                        { "heritageSelectionExpected",
                            heritageSelectionExpected },
                        { "heritageSelectionRank", owner.Progression.Features
                            .GetRank(fixture.Blueprints.Heritages.Selection) },
                        { "expectedMarkerCount", expectedMarkerCount },
                        { "activeMarkerCount", heritageChoices.Count(
                            value => owner.HasFact(value.Marker)) },
                        { "activeAffinityCount", heritageChoices.Count(
                            value => owner.HasFact(value.Affinity)) },
                        { "activeSlaFeatureCount", heritageChoices.Count(
                            value => owner.HasFact(value.SlaFeature)) },
                        { "activeSlaAbilityCount", heritageChoices.Count(
                            value => owner.Abilities.GetAbility(
                                value.SlaAbility) != null) },
                        { "statsExact", statExact },
                        { "abilityScores", statDeltas },
                        { "racialPerception", racialPerception },
                        { "speed", owner.Stats.Speed.ModifiedValue },
                        { "resource", resource },
                        { "resourceMaximum", expectedHeritage.SlaResource
                            .GetMaxAmount(owner) },
                        { "abilityGuid",
                            expectedHeritage.SlaAbility.AssetGuid },
                        { "executableAbilityGuid",
                            executableAbility.Blueprint.AssetGuid },
                        { "executableAbilityExact",
                            executableAbilityExact },
                        { "blueprintCheater", unit.Blueprint != null &&
                            unit.Blueprint.IsCheater },
                        { "abilityAvailable", ability.IsAvailable },
                        { "abilityAvailableCount",
                            ability.GetAvailableForCastCount() },
                        { "executableAbilityAvailable",
                            executableAbility.IsAvailable },
                        { "executableAbilityAvailableCount",
                            executableAbility.GetAvailableForCastCount() },
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

            private static StatType PersistenceStatType(
                ElementalHeritageStat stat)
            {
                switch (stat)
                {
                    case ElementalHeritageStat.Strength:
                        return StatType.Strength;
                    case ElementalHeritageStat.Dexterity:
                        return StatType.Dexterity;
                    case ElementalHeritageStat.Constitution:
                        return StatType.Constitution;
                    case ElementalHeritageStat.Intelligence:
                        return StatType.Intelligence;
                    case ElementalHeritageStat.Wisdom:
                        return StatType.Wisdom;
                    case ElementalHeritageStat.Charisma:
                        return StatType.Charisma;
                    default:
                        throw new ArgumentOutOfRangeException("stat");
                }
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

            private static AbilityData ResolveExecutableAbility(
                AbilityData root)
            {
                if (root == null || root.Blueprint == null)
                    throw new ArgumentNullException("root");
                AbilityVariants[] components = (root.Blueprint
                    .ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityVariants>().ToArray();
                if (components.Length == 0) return root;
                BlueprintAbility[] variants = components.Single().Variants ??
                    Array.Empty<BlueprintAbility>();
                BlueprintAbility child = variants.FirstOrDefault();
                if (child == null || !ReferenceEquals(child.Parent,
                        root.Blueprint))
                    throw new InvalidOperationException(
                        "Elemental persistence variational SLA has no exact executable child.");
                return new AbilityData(root, child);
            }

            private static void InvokeAbilitySpend(AbilityData ability,
                BlueprintAbilityResource resource)
            {
                if (ability == null || resource == null)
                    throw new ArgumentNullException(ability == null
                        ? "ability" : "resource");
                AbilityData executable = ResolveExecutableAbility(ability);
                AbilityResourceLogic[] costs = executable.Blueprint
                    .ComponentsArray.OfType<AbilityResourceLogic>()
                    .Where(value => value.IsSpendResource &&
                        ReferenceEquals(value.RequiredResource, resource))
                    .ToArray();
                if (costs.Length != 1 || costs[0].CostIsCustom ||
                    costs[0].Amount != 1)
                    throw new InvalidOperationException(
                        "Elemental persistence requires one exact native one-use resource cost.");
                costs[0].Spend(executable);
            }

            private bool RegisteredIdentitiesExact()
            {
                if (_blueprintSet == null || _blueprintSet.Count !=
                        ElementalRaceIdentityCatalog
                            .RaceBlueprintIdentityCount ||
                    BlueprintBootstrap.Library == null)
                    return false;
                ElementalRaceBlueprints[] races = _blueprintSet
                    .OrderedBlueprints().ToArray();
                var owned = new List<BlueprintScriptableObject>();
                foreach (ElementalRaceBlueprints race in races)
                {
                    owned.Add(race.Race);
                    owned.Add(race.Resistance);
                    owned.Add(race.Affinity);
                    owned.Add(race.SlaFeature);
                    owned.Add(race.SlaResource);
                    owned.Add(race.SlaAbility);
                    owned.Add(race.Heritages.Selection);
                    foreach (ElementalHeritageBlueprints heritage in
                        race.Heritages.Choices())
                    {
                        owned.Add(heritage.Marker);
                        if (heritage.Definition.IsGeneral) continue;
                        owned.Add(heritage.Affinity);
                        owned.Add(heritage.SlaFeature);
                        owned.Add(heritage.SlaResource);
                        owned.Add(heritage.SlaAbility);
                        owned.AddRange(heritage.AuxiliaryBlueprints);
                    }
                    owned.Add(race.Visuals.Body);
                    owned.AddRange(race.Visuals.Presets);
                    AddAlternateTraitIdentities(race, owned);
                }
                BlueprintScriptableObject[] exact = owned.Distinct()
                    .ToArray();
                return races.Length == ElementalRaceCatalog.RaceCount &&
                    owned.Count == exact.Length && exact.Length ==
                        ElementalRaceIdentityCatalog
                            .RaceBlueprintIdentityCount &&
                    exact.Select(value => value.AssetGuid).Distinct(
                        StringComparer.Ordinal).Count() == exact.Length &&
                    exact.All(value =>
                    {
                        BlueprintScriptableObject registered;
                        return value != null &&
                            !string.IsNullOrWhiteSpace(value.AssetGuid) &&
                            BlueprintBootstrap.Library.BlueprintsByAssetId
                                .TryGetValue(value.AssetGuid,
                                    out registered) &&
                            ReferenceEquals(value, registered) &&
                            ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                                BlueprintScriptableObject>(value.AssetGuid),
                                value);
                    });
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
                    { "allCatalogGlobalUnits",
                        CatalogFixtureIdentityCount(_unitsBefore) },
                    { "party", FixtureIdentityCount(_partyBefore) },
                    { "allCatalogParty",
                        CatalogFixtureIdentityCount(_partyBefore) },
                    { "partyCharacters", FixtureIdentityCount(
                        _partyCharactersBefore) },
                    { "allCatalogPartyCharacters",
                        CatalogFixtureIdentityCount(_partyCharactersBefore) },
                    { "remoteCompanions", FixtureIdentityCount(
                        _remoteBefore) },
                    { "allCatalogRemoteCompanions",
                        CatalogFixtureIdentityCount(_remoteBefore) },
                    { "crossScene", FixtureIdentityCount(_crossBefore) },
                    { "allCatalogCrossScene",
                        CatalogFixtureIdentityCount(_crossBefore) },
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
                    CatalogFixtureIdentityCount(_unitsBefore) == 0 &&
                    CatalogFixtureIdentityCount(_partyBefore) == 0 &&
                    CatalogFixtureIdentityCount(
                        _partyCharactersBefore) == 0 &&
                    CatalogFixtureIdentityCount(_remoteBefore) == 0 &&
                    CatalogFixtureIdentityCount(_crossBefore) == 0;
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
                int expectedFixtureCount = _fixtures.Length;
                int expectedPartyCount = WorkingSaveSmokeScenario
                    .ExpectedPartyCount + expectedFixtureCount;
                if (_loadedUnits.Length != expectedFixtureCount ||
                    _loadedUnits.Distinct().Count() !=
                        expectedFixtureCount ||
                    _partyBefore.Length != expectedPartyCount ||
                    _partyCharactersBefore.Length !=
                        expectedPartyCount ||
                    FixtureIdentityCount(_partyBefore) !=
                        expectedFixtureCount ||
                    FixtureIdentityCount(_partyCharactersBefore) !=
                        expectedFixtureCount ||
                    FixtureIdentityCount(_remoteBefore) != 0 ||
                    FixtureIdentityCount(_crossBefore) !=
                        expectedFixtureCount ||
                    _crossBefore.OfType<UnitEntityData>().Count(
                        IsFixtureUnit) != expectedFixtureCount ||
                    CatalogFixtureIdentityCount(_partyBefore) !=
                        expectedFixtureCount ||
                    CatalogFixtureIdentityCount(_partyCharactersBefore) !=
                        expectedFixtureCount ||
                    CatalogFixtureIdentityCount(_remoteBefore) != 0 ||
                    CatalogFixtureIdentityCount(_crossBefore) !=
                        expectedFixtureCount)
                    return false;
                UnitEntityData[] global = _unitsBefore
                    .OfType<UnitEntityData>().Where(IsFixtureUnit).ToArray();
                if (global.Length > expectedFixtureCount ||
                    global.Any(value => !_loadedUnits.Any(current =>
                        ReferenceEquals(current, value))) ||
                    CatalogFixtureIdentityCount(_unitsBefore) >
                        expectedFixtureCount)
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
                    PreparedFeatPersistenceInventoryExact() &&
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

            private static bool HasCatalogFixtureIdentity(object value)
            {
                UnitEntityData unit = value as UnitEntityData;
                string id = unit != null ? unit.UniqueId :
                    value is UnitReference
                        ? ((UnitReference)value).UniqueId : null;
                return !string.IsNullOrEmpty(id) &&
                    ElementalPersistenceFixtureIds.Contains(id,
                        StringComparer.Ordinal);
            }

            private static int CatalogFixtureIdentityCount(
                IEnumerable<object> values)
            {
                return values == null ? 0 : values.Count(
                    HasCatalogFixtureIdentity);
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

            private void RetireElementalRespecSource(bool cleanup = false)
            {
                UnitEntityData source = _respecSourceUnit;
                BlueprintUnit blueprint = _respecSourceBlueprint;
                _respecSourceUnit = null;
                _respecSourceBlueprint = null;
                try
                {
                    if (source == null) return;
                    source.Commands.InterruptAll(true);
                    if (source.CombatState.IsInCombat)
                    {
                        if (!cleanup)
                            throw new InvalidOperationException(
                                "A native Respec source entered combat before retirement.");
                        source.CombatState.LeaveCombat();
                    }
                    if (source.Descriptor != null)
                        source.Descriptor.State.Immortality.ReleaseAll();
                    if (source.HoldingState != null &&
                        source.HoldingState.AllEntityData.Any(value =>
                            ReferenceEquals(value, source)))
                        source.HoldingState.RemoveEntityData(source);
                    else
                    {
                        if (ContainsReference(_allUnits, source))
                            Game.Instance.State.Units.All.Remove(source);
                        source.Dispose();
                    }
                }
                finally
                {
                    _createdUnits.Remove(source);
                    _createdBlueprints.Remove(blueprint);
                    if (blueprint != null)
                        UnityEngine.Object.DestroyImmediate(blueprint);
                }
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
                    if (!RestorePrepareFeatPersistencePause())
                        throw new InvalidOperationException(
                            "The transient persistence save pause was not restored during cleanup.");
                    if (!ReleaseLoadedFeatPersistencePause())
                        throw new InvalidOperationException(
                            "The guarded post-load pause was not restored during cleanup.");
                    if (_gunslingerClass != null)
                        _gunslingerClass.StartingGold =
                            _startingGoldBefore;
                    RollbackStarterGrants();
                    RetireElementalRespecSource(true);
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
                UnitEntityData[] candidates = Snapshot(_party)
                    .Concat(Snapshot(_cross)).Concat(Snapshot(_allUnits))
                    .OfType<UnitEntityData>().Concat(_createdUnits)
                    .Where(value => value != null &&
                        (IsFixtureUnit(value) || _createdUnits.Any(current =>
                            ReferenceEquals(current, value))))
                    .Distinct().ToArray();
                foreach (UnitEntityData unit in candidates)
                    CleanupFeatPersistenceEquipment(unit);

                for (int index = _player.PartyCharacters.Count - 1;
                    index >= 0; index--)
                    if (_fixtures.Any(fixture => string.Equals(
                            _player.PartyCharacters[index].UniqueId,
                            fixture.UniqueId, StringComparison.Ordinal)))
                        _player.PartyCharacters.RemoveAt(index);
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();
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
                if (string.IsNullOrWhiteSpace(_exceptionSummary))
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
                    FeatPersistenceCleanupInventoryExact() &&
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
                    FixtureIdentityCount(Snapshot(_cross)) == 0 &&
                    CatalogFixtureIdentityCount(currentUnits) == 0 &&
                    CatalogFixtureIdentityCount(currentParty) == 0 &&
                    CatalogFixtureIdentityCount(currentPartyCharacters) == 0 &&
                    CatalogFixtureIdentityCount(Snapshot(_remote)) == 0 &&
                    CatalogFixtureIdentityCount(Snapshot(_cross)) == 0;
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
                _stage = _prepare ? "save-24-spent-heritage-fixtures" :
                    _legacyMigration
                        ? "save-legacy-0.0.114-fixture-cleanup" :
                    _moduleRestored || _cleanupStarted
                        ? "save-24-heritage-fixture-cleanup"
                        : "save-24-module-off-spent-heritage-fixtures";
                if (_saveStarted)
                    throw new InvalidOperationException(
                        "The exact elemental working-save write was already started.");
                if (_gunslingerClass != null)
                    _gunslingerClass.StartingGold = _startingGoldBefore;
                if (!RollbackStarterGrants())
                    throw new InvalidOperationException(
                        "Elemental persistence refused to save after starting inventory drift.");
                RequireFixtureStagingOutOfCombat("before-exact-working-save");
                _preSaveGate = ObserveNativeSaveGate();
                if (!_preSaveGate.Value<bool>("isSaveAllowed"))
                    throw new InvalidOperationException(
                        "The native save eligibility gate rejected the exact " +
                        "working-save write: " + _preSaveGate.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
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

            private JObject ObserveNativeSaveGate()
            {
                object saveManager = Game.Instance.SaveManager;
                MethodInfo isSaveAllowed = saveManager.GetType().GetMethod(
                    "IsSaveAllowed", BindingFlags.Instance |
                        BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);
                if (isSaveAllowed == null ||
                    isSaveAllowed.ReturnType != typeof(bool))
                    throw new MissingMethodException(
                        saveManager.GetType().FullName,
                        "IsSaveAllowed(): Boolean");

                Type dualCompanion = typeof(AbilityData).Assembly.GetType(
                    "Kingmaker.UnitLogic.Abilities.Components.AbilitySwitchDualCompanion",
                    true);
                FieldInfo isPlayingField = dualCompanion.GetField(
                    "IsPlaying", BindingFlags.Static | BindingFlags.Public |
                        BindingFlags.NonPublic);
                if (isPlayingField == null)
                    throw new MissingFieldException(dualCompanion.FullName,
                        "IsPlaying");
                object isPlaying = isPlayingField.GetValue(null);
                PropertyInfo guardValue = isPlaying.GetType().GetProperty(
                    "Value", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                PropertyInfo guardCount = isPlaying.GetType().GetProperty(
                    "GuardCount", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                if (guardValue == null || guardCount == null)
                    throw new MissingMemberException(
                        isPlaying.GetType().FullName,
                        "Value/GuardCount");

                bool allowed = (bool)isSaveAllowed.Invoke(saveManager, null);
                int fixtureCombat = Snapshot(_party).OfType<UnitEntityData>()
                    .Count(value => IsFixtureUnit(value) &&
                        value.CombatState.IsInCombat);
                int originalCombat = Snapshot(_party).OfType<UnitEntityData>()
                    .Count(value => !IsFixtureUnit(value) &&
                        value.CombatState.IsInCombat);
                var result = new JObject
                {
                    { "isSaveAllowed", allowed },
                    { "currentlyLoadedAreaPresent",
                        Game.Instance.CurrentlyLoadedArea != null },
                    { "playerIsInCombat", _player.IsInCombat },
                    { "fixturePartyCombatCount", fixtureCombat },
                    { "originalPartyCombatCount", originalCombat },
                    { "gameOverReasonPresent",
                        _player.GameOverReason.HasValue },
                    { "dialogModeActive", Game.Instance.IsModeActive(
                        Kingmaker.GameModes.GameModeType.Dialog) },
                    { "cutsceneModeActive", Game.Instance.IsModeActive(
                        Kingmaker.GameModes.GameModeType.Cutscene) },
                    { "randomEncounterPresent",
                        _player.GlobalMap.CurrentEncounterData != null },
                    { "dualCompanionPlaying",
                        (bool)guardValue.GetValue(isPlaying, null) },
                    { "dualCompanionGuardCount",
                        (int)guardCount.GetValue(isPlaying, null) },
                    { "workingDescriptorType", Convert.ToString(
                        _workingSaveSmoke.WorkingDescriptor.GetType()
                            .GetProperty("Type").GetValue(
                                _workingSaveSmoke.WorkingDescriptor, null)) }
                };
                _diagnostics.Add("preSaveGate=" + result.ToString(
                    Newtonsoft.Json.Formatting.None));
                return result;
            }

            private void RequireFixtureStagingOutOfCombat(string boundary)
            {
                UnitEntityData[] originalParty = _partyBefore
                    .OfType<UnitEntityData>().Where(value => value != null)
                    .ToArray();
                UnitEntityData[] fixtureParty = _party == null
                    ? new UnitEntityData[0]
                    : Snapshot(_party).OfType<UnitEntityData>().Where(value =>
                        value != null && IsFixtureUnit(value)).ToArray();
                UnitEntityData[] created = _createdUnits.Where(value =>
                    value != null).Distinct().ToArray();
                int originalCombat = originalParty.Count(value =>
                    value.CombatState.IsInCombat);
                int fixtureCombat = fixtureParty.Count(value =>
                    value.CombatState.IsInCombat);
                int createdCombat = created.Count(value =>
                    value.CombatState.IsInCombat);
                bool currentCombat = _currentUnit != null &&
                    _currentUnit.CombatState.IsInCombat;
                bool playerCombat = _player != null && _player.IsInCombat;
                _combatGuardChecks++;
                _lastCombatGuard = new JObject
                {
                    { "boundary", boundary },
                    { "anchorPosition", _anchor == null ? "<none>" :
                        _anchor.Position.ToString("R") },
                    { "fixtureStagingPosition",
                        _fixtureStagingPosition.ToString("R") },
                    { "currentPosition", _currentUnit == null ? "<none>" :
                        _currentUnit.Position.ToString("R") },
                    { "playerIsInCombat", playerCombat },
                    { "originalPartyCombatCount", originalCombat },
                    { "fixturePartyCombatCount", fixtureCombat },
                    { "createdUnitCombatCount", createdCombat },
                    { "currentUnitInCombat", currentCombat }
                };
                if (!playerCombat && originalCombat == 0 &&
                    fixtureCombat == 0 && createdCombat == 0 &&
                    !currentCombat) return;
                throw new InvalidOperationException(
                    "The exact disposable fixture staging boundary entered " +
                    "combat and cannot be qualified or saved: " +
                    _lastCombatGuard.ToString(
                        Newtonsoft.Json.Formatting.None) + ".");
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
                    if (!RestorePrepareFeatPersistencePause())
                    {
                        if (_saveElapsed != null &&
                            _saveElapsed.Elapsed.TotalSeconds <
                                _request.CompletionTimeoutSeconds)
                            return;
                        RecordException(new TimeoutException(
                            "The transient persistence save pause was not restored after the bounded save-completion window."));
                    }
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(_prepare ? _preparedMembershipExact :
                        _moduleRestored || _cleanupStarted
                            ? _structuralCleaned
                            : _preservedMembershipExact);
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
                    { "nativeRespecRecords", _respecRecords.Count },
                    { "currentUnitPresent", _currentUnit != null },
                    { "saveStarted", _saveStarted },
                    { "cleanupStarted", _cleanupStarted },
                    { "exceptionSummary", _exceptionSummary }
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
                        _legacyMigration
                            ? "legacy-0.0.114-load-verify-cleanup" :
                        _moduleRestored
                            ? "module-restored-verify-respec-cleanup"
                            : "module-disabled-verify-preserve" },
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
                    { "featRegisteredIdentitiesExact",
                        _featRegisteredExact },
                    { "featSelectorStateExact", _featSelectorExact },
                    { "preparedFeatTransientState",
                        _preparedFeatTransientState.DeepClone() },
                    { "featIdentityCount",
                        ElementalRaceIdentityCatalog.FeatIdentityCount },
                    { "featCount", ElementalFeatPolicy.FeatCount },
                    { "scorchingPersistenceFixtureIndex",
                        ScorchingPersistenceFixtureIndex },
                    { "strikePersistenceFixtureIndex",
                        StrikePersistenceFixtureIndex },
                    { "fixtureCount", _fixtures.Length },
                    { "expectedPreparedPartyCount",
                        WorkingSaveSmokeScenario.ExpectedPartyCount +
                            _fixtures.Length },
                    { "fixtureUniqueIds", new JArray(
                        _fixtures.Select(value => value.UniqueId)) },
                    { "fixtures", new JArray(_fixtures.Select(value =>
                        new JObject
                        {
                            { "label", value.Label },
                            { "uniqueId", value.UniqueId },
                            { "name", value.Name },
                            { "raceGuid",
                                value.Blueprints.Race.AssetGuid },
                            { "gender", value.Gender.ToString() },
                            { "presetGuid", value.Preset.AssetGuid },
                            { "sourceHeritage",
                                value.SourceHeritage.Definition.Name },
                            { "persistedHeritage",
                                value.Heritage.Definition.Name },
                            { "restoredRespecHeritage",
                                value.RestoredHeritage.Definition.Name }
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
                    { "nativeRespecRecords", _respecRecords },
                    { "nativeSelectionRecords", _selectionRecords },
                    { "preparedMembershipExact",
                        _preparedMembershipExact },
                    { "preparedFeatInventoryExact", _prepare &&
                        PreparedFeatPersistenceInventoryExact() },
                    { "preservedMembershipExact",
                        _preservedMembershipExact },
                    { "normalPathComplete", _normalPathComplete },
                    { "baselineAbsentExact", _baselineAbsentExact },
                    { "structuralCleaned", _structuralCleaned },
                    { "featCleanupInventoryExact",
                        FeatPersistenceCleanupInventoryExact() },
                    { "retainedFeatPersistenceInventoryCount",
                        RetainedFeatPersistenceInventoryCount() },
                    { "captureCount", _captured },
                    { "imageCount", _imageCount },
                    { "renderedViewCount", _viewCount },
                    { "fixtureStagingPosition",
                        _fixtureStagingPosition.ToString("R") },
                    { "combatGuardChecks", _combatGuardChecks },
                    { "lastCombatGuard", _lastCombatGuard.DeepClone() },
                    { "saveApiCalled", _saveStarted },
                    { "expectedWorkingSaveRoutineCount",
                        _workingSaveEvidence == null ? 0 :
                            _workingSaveEvidence
                                .ExpectedWorkingSaveRoutineCount },
                    { "preSaveGate", _preSaveGate.DeepClone() },
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
                else if (_legacyMigration)
                    FinishLegacyMigration(phaseStateExact);
                else if (_moduleRestored)
                    FinishModuleRestoredVerify(phaseStateExact);
                else FinishModuleDisabledVerify(phaseStateExact);
                CompletePhaseResult();
            }

            private void FinishPrepare(bool prepared)
            {
                JObject[] records = _records.OfType<JObject>().ToArray();
                JObject[] respecRecords = _respecRecords.OfType<JObject>()
                    .ToArray();
                bool respecExact = respecRecords.Length ==
                        ElementalPersistenceFixtureCount &&
                    respecRecords.All(value =>
                        NativeElementalRespecRecordExact(value, false));
                JObject[] selectionRecords = _selectionRecords
                    .OfType<JObject>().ToArray();
                bool creationSelectionOriginsExact = selectionRecords.Count(
                        value => string.Equals(value.Value<string>("phase"),
                            "character-creation-source",
                            StringComparison.Ordinal) &&
                            TokenBool(value, "statePresentBefore") &&
                            TokenBool(value,
                                "stateNativeFromRaceSelection")) ==
                    ElementalPersistenceFixtureCount;
                bool respecSelectionOriginsExact = selectionRecords.Count(
                        value => string.Equals(value.Value<string>("phase"),
                            "native-respec-target",
                            StringComparison.Ordinal) &&
                            TokenBool(value, "statePresentBefore") &&
                            TokenBool(value,
                                "stateNativeFromRaceSelection")) ==
                    ElementalPersistenceFixtureCount;
                bool selectionsExact = selectionRecords.Length ==
                        ElementalPersistenceFixtureCount * 2 &&
                    selectionRecords.All(NativeSelectionRecordExact) &&
                    creationSelectionOriginsExact &&
                    respecSelectionOriginsExact;
                bool recordExact = records.Length ==
                        ElementalPersistenceFixtureCount &&
                    records.All(value => TokenBool(value,
                            "observationExact") &&
                        TokenBool(value, "nativeRespecExact") &&
                        TokenBool(value, "spendExact") &&
                        TokenBool(value, "featPersistenceExact") &&
                        value.Value<int>("resourceBeforeSpend") == 1 &&
                        value.Value<int>("resourceAfterSpend") == 0);
                bool capturesExact = CaptureSetExact(records);
                bool transientExact = _preparedFeatTransientState != null &&
                    _preparedFeatTransientState.Value<bool>("exact");
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
                    "Elemental Races active with four exact race selector entries, exact feat publication, and every Release B identity registered",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    _context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "live module snapshot, CharacterRaces, and resource cache");
                Add(_assertions,
                    "elemental-race-persistence-native-respec",
                    "24 distinct source-to-replacement native Respec commits preserve exact race, selected heritage, facts, SLA, DollData, and Gunslinger presentation",
                    "records=" + respecRecords.Length + ";exact=" +
                        respecExact + ";selections=" +
                        selectionRecords.Length + ";selectionExact=" +
                        selectionsExact + ";creationStateNative=" +
                        creationSelectionOriginsExact +
                        ";fixedRaceRespecStateNative=" +
                        respecSelectionOriginsExact,
                    respecExact && selectionsExact,
                    "fixed BlueprintRace shell, replayable native same-race SelectRace, LevelUpState.FindSelection, LevelUpController.SelectFeature, CharBuildMode.Respec, SelectClass, Commit, source retirement, and replacement observation");
                Add(_assertions,
                    "elemental-race-persistence-prepared-rules",
                    "24 exact race/sex/heritage Gunslingers cover all three presets with level-1 facts, exact final stats, resistance, affinity, Keen Senses, and active SLA only",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "real native descriptors and production blueprints before save");
                Add(_assertions,
                    "elemental-race-persistence-spent-resources",
                    "all 24 active heritage racial resources commit from one to zero before save",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "AbilityResourceLogic.Spend and owner persistent resources");
                Add(_assertions,
                    "elemental-feat-persistence-prepared-state",
                    "all 24 fixtures own the exact race-applicable Release B feat facts; representative native commands persist one Elemental Strike buff and one two-item Scorching Weapons snapshot; Sylph fixtures own native flight buffs",
                    "records=" + records.Length + ";factsExact=" +
                        recordExact + ";preSaveTransientExact=" +
                        transientExact,
                    recordExact && transientExact,
                    "live feature facts and native Wings buffs throughout fixture construction, followed by immediate pre-save native commands, two distinct native equipped item references, and project weapon enchantments");
                Add(_assertions,
                    "elemental-race-persistence-prepared-membership",
                    "24 unique marker-bound heritage fixtures appended to the exact three-character working-save party and scene",
                    "prepared=" + prepared + ";membership=" +
                        _preparedMembershipExact,
                    prepared && _preparedMembershipExact &&
                        _normalPathComplete,
                    "exact Party, PartyCharacters, cross-scene, global, remote, inventory, and money deltas");
                Add(_assertions,
                    "elemental-race-persistence-captures",
                    "24 sidecars, 48 PNGs, and 120 labelled views before save",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "four-view previews plus ordinary isometric captures");
                AddSaveAndIdentityAssertions(true);
                Add(_assertions,
                    "elemental-race-persistence-live-state-preserved",
                    "class blueprint, selector array, inventory, and money unchanged outside the 24 disposable party additions",
                    "startingGold=" + _gunslingerClass.StartingGold +
                        ";inventoryExact=" +
                            FeatPersistenceCleanupInventoryExact() +
                        ";retainedSavedFixtureItems=" +
                            RetainedFeatPersistenceInventoryCount() +
                        ";selectorArrayExact=" +
                        CharacterRacesArrayExact(),
                    _gunslingerClass.StartingGold ==
                        _startingGoldBefore &&
                    FeatPersistenceCleanupInventoryExact() &&
                    _player.Money == _moneyBefore &&
                    CharacterRacesArrayExact(),
                    "exact pre-run snapshots; production blueprints never mutated");
            }

            private void FinishLegacyMigration(bool cleaned)
            {
                int legacyFixtureCount =
                    ElementalHeritagePersistenceMatrixPolicy
                        .LegacyGeneralFixtureCount(
                            ElementalRaceCatalog.RaceCount);
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool recordExact = records.Length == legacyFixtureCount &&
                    records.All(value =>
                        string.Equals(value.Value<string>("producerVersion"),
                            "0.0.114", StringComparison.Ordinal) &&
                        string.Equals(value.Value<string>("receiverVersion"),
                            "0.0.115", StringComparison.Ordinal) &&
                        TokenBool(value,
                            "loadedDollExactBeforeReconstruction") &&
                        TokenBool(value, "loadedObservationExact") &&
                        TokenBool(value, "legacyIdentityExact") &&
                        TokenBool(value, "markerlessGeneralExact") &&
                        value.Value<int>("resourceBeforeReconcile") == 0 &&
                        TokenBool(value, "reconcileAccepted") &&
                        value.Value<int>("resourceAfterReconcile") == 0 &&
                        TokenBool(value, "reconciledObservationExact"));
                bool generalPrefixExact = _fixtures.Length ==
                        legacyFixtureCount &&
                    _fixtures.All(value => value.Index >= 0 &&
                        value.Index < legacyFixtureCount &&
                        value.Heritage.Definition.IsGeneral &&
                        ReferenceEquals(value.Heritage,
                            value.Blueprints.Heritages.General));
                bool capturesExact = CaptureSetExact(records);
                Add(_assertions, "elemental-race-legacy-migration-guard",
                    RuntimeTestScenarioCatalog.ElementalRaceLegacyMigration,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog.ElementalRaceLegacyMigration,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "elemental-race-legacy-migration-module-on",
                    "0.0.115 Elemental Races active with four exact selector entries and every Release A blueprint registered",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    _context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "fresh 0.0.115 startup resource cache and CharacterRaces publication");
                Add(_assertions,
                    "elemental-race-legacy-migration-membership",
                    "exact eleven-member party containing only the eight stable 0.0.114 General race/sex fixture identities",
                    _loadedFixtureMembership.ToString(
                        Newtonsoft.Json.Formatting.None),
                    generalPrefixExact && LoadedMembershipExact(),
                    "receiver-correlated Steam-backed working-save load and full 24-ID residue audit");
                Add(_assertions,
                    "elemental-race-legacy-migration-state",
                    "all eight markerless General fixtures retain exact race GUID, facts, stats, spent SLA resource, one active ability/provider, DollData, class equipment, rig, and materials",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "0.0.114 save hydration followed by current owned-provider reconciliation and a second idempotence pass");
                Add(_assertions,
                    "elemental-race-legacy-migration-captures",
                    "8 sidecars, 16 PNGs, and 40 labelled post-migration views",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "post-hydration four-view previews and ordinary isometric captures");
                Add(_assertions,
                    "elemental-race-legacy-migration-cleanup",
                    "all eight disposable legacy fixtures removed and the exact three-character baseline cleanup-saved",
                    "cleaned=" + cleaned + ";normalPath=" +
                        _normalPathComplete + ";updates=" +
                        _settleUpdates,
                    cleaned && _normalPathComplete && _structuralCleaned,
                    "exact legacy fixture IDs only; native scene removal and guarded cleanup save");
                AddSaveAndIdentityAssertions(true);
            }

            private void FinishModuleDisabledVerify(bool preserved)
            {
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool recordExact = records.Length ==
                        ElementalPersistenceFixtureCount &&
                    records.All(value =>
                        TokenBool(value,
                            "loadedDollExactBeforeReconstruction") &&
                        TokenBool(value, "loadedObservationExact") &&
                        TokenBool(value, "advancedObservationExact") &&
                        TokenBool(value, "levelUpExact") &&
                        TokenBool(value, "restoredObservationExact") &&
                        TokenBool(value, "restExact") &&
                        TokenBool(value, "preservedObservationExact") &&
                        TokenBool(value, "respendExact") &&
                        TokenBool(value,
                            "loadedFeatPersistenceExact") &&
                        TokenBool(value,
                            "advancedFeatPersistenceExact") &&
                        TokenBool(value,
                            "cleanedFeatPersistenceExact") &&
                        TokenBool(value,
                            "restoredFeatPersistenceExact") &&
                        TokenBool(value,
                            "preservedFeatPersistenceExact") &&
                        value.Value<int>("casterLevelBeforeRest") == 1 &&
                        value.Value<int>("resourceAfterSpentLevelUp") == 0 &&
                        value.Value<int>("resourceAfterRest") == 1 &&
                        value.Value<int>("resourceAfterRespend") == 0 &&
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
                    "Elemental Races inactive and absent from race/feat selectors while every Release B identity remains registered",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    !_context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "fresh startup module snapshot, CharacterRaces, and resource cache");
                Add(_assertions,
                    "elemental-race-persistence-loaded-membership",
                    "exact 27-member party with one marker-bound fixture for every race, sex, heritage, and production preset",
                    _loadedFixtureMembership.ToString(
                        Newtonsoft.Json.Formatting.None),
                    LoadedMembershipExact(),
                    "fresh receiver-correlated Steam-backed working-save load");
                Add(_assertions,
                    "elemental-race-persistence-loaded-state",
                    "all 24 fixtures retain exact race, heritage, final stats, providers, spent SLA, deterministic DollData, Gunslinger equipment, rig, and materials",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "actual loaded descriptors and native view reconstruction while selector publication is OFF");
                Add(_assertions,
                    "elemental-race-persistence-rest-and-level-up",
                    "spent level-up preserves zero uses while retaining heritage and advancing caster level; ordinary rest restores exactly one use and an exact re-spend returns it to zero",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "LevelUpController.ApplyLevelup, pre-rest resource observation, and RestController.ApplyRest");
                Add(_assertions,
                    "elemental-feat-persistence-module-off-state",
                    "all feat facts, granted abilities, Sylph flight buffs, the active Elemental Strike buff, and both exact Scorching Weapons item enchantments hydrate with publication OFF; level-up preserves them; exact transient cleanup removes only the short effects before rest and re-save",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "fresh-process loaded descriptors, buffs, engine-issued item identities, equipment slots, enchantments, level-up, rest, and exact project-owned cleanup");
                Add(_assertions,
                    "elemental-race-persistence-captures",
                    "24 sidecars, 48 PNGs, and 120 labelled module-OFF post-load views",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "post-reconstruction four-view previews and isometric captures");
                Add(_assertions,
                    "elemental-race-persistence-module-off-preserved",
                    "all 24 re-spent heritage fixtures remain in the exact 27-member party/cross-scene state and are saved for module-ON restoration",
                    "preserved=" + preserved + ";normalPath=" +
                        _normalPathComplete + ";updates=" +
                        _settleUpdates,
                    preserved && _normalPathComplete &&
                        _preservedMembershipExact,
                    "exact fixture membership and armed working-save write without cleanup");
                AddSaveAndIdentityAssertions(true);
            }

            private void FinishModuleRestoredVerify(bool cleaned)
            {
                JObject[] records = _records.OfType<JObject>().ToArray();
                JObject[] respecRecords = _respecRecords.OfType<JObject>()
                    .ToArray();
                JObject[] selectionRecords = _selectionRecords
                    .OfType<JObject>().ToArray();
                bool recordExact = records.Length ==
                        ElementalPersistenceFixtureCount &&
                    records.All(value =>
                        TokenBool(value,
                            "loadedDollExactBeforeReconstruction") &&
                        TokenBool(value, "observationExact") &&
                        TokenBool(value, "nativeRespecExact") &&
                        TokenBool(value,
                            "sourceFeatPersistenceExact") &&
                        TokenBool(value,
                            "replacementFeatPersistenceExact") &&
                        value.Value<int>("resourceAfterRespec") == 1);
                bool respecExact = respecRecords.Length ==
                        ElementalPersistenceFixtureCount &&
                    respecRecords.All(value =>
                        value.Value<int>("sourceLevel") == 2 &&
                        NativeElementalRespecRecordExact(value, true));
                bool selectionsExact = selectionRecords.Length ==
                        ElementalPersistenceFixtureCount &&
                    selectionRecords.All(NativeSelectionRecordExact) &&
                    selectionRecords.All(value =>
                        TokenBool(value, "statePresentBefore") &&
                        TokenBool(value,
                            "stateNativeFromRaceSelection"));
                bool transitionMatrixExact = _fixtures.All(value =>
                    !ReferenceEquals(value.SourceHeritage,
                        value.Heritage) &&
                    !ReferenceEquals(value.Heritage,
                        value.RestoredHeritage)) &&
                    _fixtures.Where(value => !value.Heritage.Definition
                        .IsGeneral && ReferenceEquals(value.SourceHeritage,
                            value.Blueprints.Heritages.General)).All(value =>
                                ReferenceEquals(value.RestoredHeritage,
                                    value.Blueprints.Heritages.General)) &&
                    _fixtures.GroupBy(value => new
                    {
                        value.Blueprints.Definition.Kind,
                        value.Gender
                    }).All(group => group.Count() ==
                        ElementalHeritagePolicy.ChoicesPerRace);
                bool capturesExact = CaptureSetExact(records);
                Add(_assertions, "elemental-race-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .ElementalRaceModuleRestoredPersistence,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRaceModuleRestoredPersistence,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "elemental-race-persistence-module-restored",
                    "Elemental Races active again with four exact race selector entries, exact feat publication, and every Release B identity registered",
                    "active=" + _context.FeatureModules.Active
                        .ElementalRaces + ";selectorExact=" +
                        _selectorExact + ";registeredExact=" +
                        _registeredExact,
                    _context.FeatureModules.Active.ElementalRaces &&
                        _selectorExact && _registeredExact,
                    "fresh startup module snapshot, CharacterRaces, and exact live resource cache");
                Add(_assertions,
                    "elemental-race-persistence-restored-respec",
                    "all 24 level-2 spent fixtures reload, then commit exact native heritage Respec transitions at the preserved level",
                    "records=" + records.Length + ";recordExact=" +
                        recordExact + ";respec=" + respecRecords.Length +
                        ";respecExact=" + respecExact +
                        ";selections=" + selectionRecords.Length +
                        ";selectionsExact=" + selectionsExact,
                    recordExact && respecExact && selectionsExact,
                    "fresh load, LevelUpController.SelectFeature/Commit, exact source retirement, replacement observation, and level restoration");
                Add(_assertions,
                    "elemental-race-persistence-respec-transition-matrix",
                    "both sexes of every race cover General-to-alternate-to-General and alternate-A-to-alternate-B",
                    "fixtures=" + _fixtures.Length + ";exact=" +
                        transitionMatrixExact,
                    transitionMatrixExact,
                    "stable fixture source/persisted/restored heritage mapping and native Respec records");
                Add(_assertions,
                    "elemental-feat-persistence-restored-respec-cleanup",
                    "every module-restored source retains its exact Release B feat state before native Respec and every replacement is free of stale feat facts, abilities, buffs, weapons, and enchantments",
                    "records=" + records.Length + ";exact=" +
                        recordExact,
                    recordExact,
                    "fresh-process source observation followed by distinct native Respec replacement observation");
                Add(_assertions,
                    "elemental-race-persistence-restored-captures",
                    "24 sidecars, 48 PNGs, and 120 labelled module-restored post-respec views",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "post-respec four-view previews and ordinary isometric captures");
                Add(_assertions,
                    "elemental-race-persistence-cleanup",
                    "all 24 marker fixtures removed and the exact three-character party/global/remote/cross-scene/inventory/money baseline cleanup-saved",
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
                    _featRegisteredExact && _featSelectorExact &&
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
                    "all race, heritage, and Release B feat identities registered and race/feat selector states consistent with the exactly restored original module setting",
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
                bool combatGuardExact = _combatGuardChecks > 0 &&
                    _lastCombatGuard.Count > 0 &&
                    !_lastCombatGuard.Value<bool>("playerIsInCombat") &&
                    _lastCombatGuard.Value<int>("originalPartyCombatCount") == 0 &&
                    _lastCombatGuard.Value<int>("fixturePartyCombatCount") == 0 &&
                    _lastCombatGuard.Value<int>("createdUnitCombatCount") == 0 &&
                    !_lastCombatGuard.Value<bool>("currentUnitInCombat");
                Add(_assertions,
                    "elemental-race-persistence-combat-staging",
                    "every guarded fixture boundary remains outside combat at one anchor-local navigable position",
                    "checks=" + _combatGuardChecks + ";last=" +
                        _lastCombatGuard.ToString(
                            Newtonsoft.Json.Formatting.None),
                    combatGuardExact,
                    "baseline, spawn, settle, Respec, promotion, and pre-save fail-fast guards");
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
                int expectedFixtureCount = _fixtures.Length;
                return _normalPathComplete && records.Length ==
                        expectedFixtureCount &&
                    _captured == expectedFixtureCount &&
                    _imageCount == expectedFixtureCount * 2 &&
                    _viewCount == expectedFixtureCount * 5 &&
                    _indexWritten && _evidenceFiles.Count ==
                        expectedFixtureCount * 3 + 1 &&
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
