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
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded save/load and native-respec qualification for the production
    /// Gunslinger class outfit. A three-launch transaction prepares one exact
    /// disposable working-save fixture, verifies it after a fresh load while
    /// rebuilding and rendering it, saves its cleanup, and then verifies its
    /// absence. The protected baseline is never eligible.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private static readonly string[] PersistedMaleFighterAssetIds =
        {
            "9875b1f3cf3b8bf42a5fb99907e5a794",
            "551682302c6f9b146b7657c52b5cabac",
            "67b5adfbb99269b43bb3ca00438626c8"
        };

        private static readonly string[] PersistedFemaleFighterAssetIds =
        {
            "04e8446d4666d6a46b28a98c55ec9f6c",
            "d771acb96d986484dbd006a78a65cdba",
            "8061ab0f406f7f84e8d36eada05f97a7"
        };

        private const string PersistedOutfitFixtureUniqueId =
            "87ee6591-0383-4f83-bfb9-cd8a4f5ed4b7";
        private const string PersistedOutfitFixtureName =
            "KMG_OUTFIT_PERSISTENCE_FIXTURE";

        internal static ProductionPersistenceSession
            BeginProductionPersistence(ModContext context,
                RuntimeTestRequest request,
                WorkingSaveSmokeScenario workingSaveSmoke)
        {
            return new ProductionPersistenceSession(context, request,
                workingSaveSmoke);
        }

        private sealed class PersistenceFixture
        {
            internal PersistenceFixture(Gender gender, BlueprintRace race,
                BlueprintRaceVisualPreset preset)
            {
                Gender = gender;
                Race = race;
                Preset = preset;
                Label = gender.ToString().ToLowerInvariant() + "-human";
            }

            internal readonly string Label;
            internal readonly Gender Gender;
            internal readonly BlueprintRace Race;
            internal readonly BlueprintRaceVisualPreset Preset;
        }

        private sealed class PersistenceDollSnapshot
        {
            internal Gender Gender;
            internal BlueprintRaceVisualPreset RacePreset;
            internal string[] EquipmentEntityIds;
            internal KeyValuePair<string, int>[] PrimaryRamps;
            internal KeyValuePair<string, int>[] SecondaryRamps;
            internal bool LeftHanded;
            internal int ClothesPrimaryIndex;
            internal int ClothesSecondaryIndex;

            internal static PersistenceDollSnapshot Capture(DollData data)
            {
                if (data == null)
                    throw new InvalidOperationException(
                        "Persistence evidence requires non-null DollData.");
                return new PersistenceDollSnapshot
                {
                    Gender = data.Gender,
                    RacePreset = data.RacePreset,
                    EquipmentEntityIds = (data.EquipmentEntityIds ??
                        new List<string>()).ToArray(),
                    PrimaryRamps = (data.EntityRampIdices ??
                        new Dictionary<string, int>())
                        .OrderBy(value => value.Key, StringComparer.Ordinal)
                        .ToArray(),
                    SecondaryRamps = (data.EntitySecondaryRampIdices ??
                        new Dictionary<string, int>())
                        .OrderBy(value => value.Key, StringComparer.Ordinal)
                        .ToArray(),
                    LeftHanded = data.LeftHanded,
                    ClothesPrimaryIndex = data.ClothesPrimaryIndex,
                    ClothesSecondaryIndex = data.ClothesSecondaryIndex
                };
            }

            internal bool Matches(DollData data)
            {
                if (data == null || data.EquipmentEntityIds == null ||
                    data.EntityRampIdices == null ||
                    data.EntitySecondaryRampIdices == null)
                    return false;
                return data.Gender == Gender &&
                    ReferenceEquals(data.RacePreset, RacePreset) &&
                    data.LeftHanded == LeftHanded &&
                    data.ClothesPrimaryIndex == ClothesPrimaryIndex &&
                    data.ClothesSecondaryIndex == ClothesSecondaryIndex &&
                    EquipmentEntityIds.SequenceEqual(
                        data.EquipmentEntityIds, StringComparer.Ordinal) &&
                    PrimaryRamps.SequenceEqual(data.EntityRampIdices
                        .OrderBy(value => value.Key,
                            StringComparer.Ordinal)) &&
                    SecondaryRamps.SequenceEqual(
                        data.EntitySecondaryRampIdices.OrderBy(value =>
                            value.Key, StringComparer.Ordinal));
            }

            internal JObject Describe()
            {
                return new JObject
                {
                    { "gender", Gender.ToString() },
                    { "racePresetName", RacePreset == null ? "<null>" :
                        RacePreset.name },
                    { "racePresetGuid", RacePreset == null ? "<null>" :
                        RacePreset.AssetGuid },
                    { "equipmentEntityIds", new JArray(
                        EquipmentEntityIds) },
                    { "entityRampIndices", new JArray(PrimaryRamps.Select(
                        value => new JObject
                        {
                            { "assetId", value.Key },
                            { "index", value.Value }
                        }).ToArray()) },
                    { "entitySecondaryRampIndices", new JArray(
                        SecondaryRamps.Select(value => new JObject
                        {
                            { "assetId", value.Key },
                            { "index", value.Value }
                        }).ToArray()) },
                    { "leftHanded", LeftHanded },
                    { "clothesPrimaryIndex", ClothesPrimaryIndex },
                    { "clothesSecondaryIndex", ClothesSecondaryIndex }
                };
            }
        }

        private sealed class PersistenceAppearanceObservation
        {
            internal bool ProductionPresent;
            internal bool LegacyAbsent;
            internal bool PaletteExact;
            internal bool RigExact;
            internal int RendererCount;
            internal JObject Evidence;

            internal bool Exact
            {
                get
                {
                    return ProductionPresent && LegacyAbsent &&
                        PaletteExact && RigExact && RendererCount > 0;
                }
            }
        }

        internal sealed class ProductionPersistenceSession
        {
            private const int MinimumSettleUpdates = 30;
            private const int MaximumSettleUpdates = 360;
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
            private readonly JArray _partyRecords = new JArray();
            private readonly JArray _records = new JArray();
            private readonly JArray _respecRecords = new JArray();
            private JObject _loadedFixtureMembership = new JObject();
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
            private Player _player;
            private BlueprintCharacterClass _gunslingerClass;
            private BlueprintCharacterClass _fighterClass;
            private BlueprintItem[] _startingItems = new BlueprintItem[0];
            private int[] _startingItemCounts = new int[0];
            private int _startingGoldBefore;
            private UnitEntityData _anchor;
            private UnitEntityData _persistedUnit;
            private Character _persistedAvatar;
            private PersistenceDollSnapshot _persistedDollBefore;
            private AvatarEntityState[] _persistedAvatarBefore =
                new AvatarEntityState[0];
            private string[] _persistedSavedLinksBefore = new string[0];
            private FieldInfo _equipmentClassField;
            private object _persistedEquipmentClassBefore;
            private JObject _persistedViewActivationEvidence =
                new JObject();
            private PersistenceFixture[] _fixtures =
                new PersistenceFixture[0];
            private UnitEntityData _actor;
            private BlueprintUnit _actorBlueprint;
            private UnitEntityData _respecSourceActor;
            private BlueprintUnit _respecSourceBlueprint;
            private Character _avatar;
            private DollData _initialDollData;
            private PersistenceDollSnapshot _respecDoll;
            private LevelUpController _seedController;
            private LevelUpController _respecController;
            private string _selectedHairAssetId = string.Empty;
            private int _fixtureIndex;
            private int _phase;
            private int _settleUpdates;
            private int _captured;
            private int _imageCount;
            private int _viewCount;
            private bool _persistedDiscovered;
            private bool _persistedSerializedClassClothesAbsent;
            private bool _persistedForceUseClassEquipment;
            private bool _persistedPreActivationAppearanceExact;
            private bool _persistedViewActivationExact;
            private bool _persistedViewActivationStarted;
            private bool _persistedLoadedExact;
            private bool _persistedReconstructionExact;
            private bool _persistedDollUnchanged;
            private bool _persistedRestored;
            private bool _respecSerializedClassClothesAbsent = true;
            private bool _starterRollbackExact = true;
            private bool _normalPathComplete;
            private bool _persistedPromoted;
            private bool _persistedRemoved;
            private bool _baselineAbsentExact;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private bool _saveStarted;
            private bool _saveCompleted;
            private bool _structuralCleaned;
            private Stopwatch _saveElapsed;
            private WorkingSaveSmokeEvidence _workingSaveEvidence;
            private string _selectedPersistedUnitName = "<none>";
            private string _selectedPersistedUnitId = "<none>";
            private string _gameAssemblySha256 = string.Empty;
            private string _gameAssemblyMvid = string.Empty;
            private string _stage = "initialize";
            private string _exceptionSummary = string.Empty;

            internal ProductionPersistenceSession(ModContext context,
                RuntimeTestRequest request,
                WorkingSaveSmokeScenario workingSaveSmoke)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                if (workingSaveSmoke == null)
                    throw new ArgumentNullException("workingSaveSmoke");
                _context = context;
                _request = request;
                _workingSaveSmoke = workingSaveSmoke;
                _prepare = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionPersistencePrepare,
                    StringComparison.Ordinal);
                _verifyAbsent = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionPersistenceVerifyAbsent,
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
                        if (!ResourcesReady("loaded-persistence")) return;
                        if (_prepare)
                        {
                            StartRespecFixture();
                            _phase = 3;
                            _settleUpdates = 0;
                            return;
                        }
                        BeginPersistedViewActivation();
                        if (!PollActorReady(_persistedUnit,
                                "persisted-loaded")) return;
                        CapturePersistedLoadedAndBeginReconstruction();
                        _phase = 2;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 2)
                    {
                        if (!PollActorReady(_persistedUnit,
                                "persisted-reconstruction")) return;
                        CapturePersistedReconstructionAndRestore();
                        StartRespecFixture();
                        _phase = 3;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 3)
                    {
                        if (!PollActorReady(_actor, "respec-base-" +
                                _fixtures[_fixtureIndex].Label)) return;
                        PerformNativeRespec();
                        _phase = 4;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 4)
                    {
                        if (!PollActorReady(_actor, "respec-result-" +
                                _fixtures[_fixtureIndex].Label)) return;
                        CaptureRespecResult();
                        if (_prepare)
                        {
                            JObject preparedRespec = _respecRecords
                                .OfType<JObject>().Single();
                            if (!RespecRecordExact(preparedRespec))
                                throw new InvalidOperationException(
                                    "Persistence prepare refused to promote or save a visually inexact native-respec fixture.");
                            PromoteActorForPersistence();
                            _normalPathComplete = true;
                            StartExactWorkingSave();
                            return;
                        }
                        RetireActor();
                        _fixtureIndex++;
                        if (_fixtureIndex < _fixtures.Length)
                        {
                            StartRespecFixture();
                            _phase = 3;
                            _settleUpdates = 0;
                            return;
                        }
                        _normalPathComplete = true;
                        BeginCleanup();
                    }
                }
                catch (Exception exception)
                {
                    _exceptionSummary = exception.ToString();
                    Add(_assertions,
                        "gunslinger-outfit-production-persistence-exception",
                        "no exception", "stage=" + _stage + ";" +
                            exception, false,
                        "guarded loaded-save and request-local native-respec fixture");
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
                _stage = "validate-guard-and-snapshot";
                if (!string.Equals(SaveName(),
                        "KMG_AUTOMATION_WORKING", StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Production persistence requires the exact disposable working save.");
                if (!RuntimeTestScenarioCatalog
                        .IsGunslingerOutfitProductionPersistenceScenario(
                            _request.Scenario))
                    throw new InvalidOperationException(
                        "Production persistence scenario identity is not exact.");

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
                _anchor = _partyBefore.OfType<UnitEntityData>()
                    .FirstOrDefault(value => value != null &&
                        value.View != null && value.HoldingState != null);
                if (_anchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party-area anchor.");

                _gunslingerClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        OutfitProductionClassGuid,
                        "gunslinger-outfit-persistence-class");
                _fighterClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        FighterClassGuid,
                        "gunslinger-outfit-persistence-fighter");
                _startingItems = _gunslingerClass.StartingItems ??
                    new BlueprintItem[0];
                _startingItemCounts = _startingItems.Select(value =>
                    _player.Inventory.Count(value)).ToArray();
                _startingGoldBefore = _gunslingerClass.StartingGold;
                if (!_verifyAbsent)
                    _gunslingerClass.StartingGold = 0;

                foreach (UnitEntityData unit in _partyBefore
                    .OfType<UnitEntityData>())
                {
                    int level = unit == null || unit.Descriptor == null ||
                            unit.Descriptor.Progression == null
                        ? -1 : unit.Descriptor.Progression.GetClassLevel(
                            _gunslingerClass);
                    _partyRecords.Add(new JObject
                    {
                        { "name", unit == null ? "<null>" :
                            unit.CharacterName ?? "<unnamed>" },
                        { "uniqueId", unit == null ? "<null>" :
                            unit.UniqueId ?? "<null>" },
                        { "gunslingerLevel", level },
                        { "hasDollData", unit != null &&
                            unit.Descriptor != null &&
                            unit.Descriptor.Doll != null },
                        { "hasLiveView", unit != null &&
                            unit.View != null &&
                            unit.View.CharacterAvatar != null }
                    });
                }
                _equipmentClassField = typeof(UnitEntityView).GetField(
                    "m_EquipmentClass", BindingFlags.Instance |
                        BindingFlags.NonPublic);
                if (_equipmentClassField == null ||
                    _equipmentClassField.FieldType !=
                        typeof(BlueprintCharacterClass))
                    throw new MissingFieldException(
                        typeof(UnitEntityView).FullName,
                        "m_EquipmentClass");
                PersistenceFixture[] fixtures = BuildFixtures();
                _fixtures = _prepare ? fixtures.Take(1).ToArray() : fixtures;
                Assembly assembly = typeof(BlueprintCharacterClass).Assembly;
                _gameAssemblySha256 = HashFile(assembly.Location)
                    .ToLowerInvariant();
                _gameAssemblyMvid = assembly.ManifestModule
                    .ModuleVersionId.ToString("D");

                UnitEntityData[] markedUnits = _unitsBefore
                    .OfType<UnitEntityData>()
                    .Where(IsPersistedOutfitFixture).ToArray();
                UnitEntityData[] markedParty = _partyBefore
                    .OfType<UnitEntityData>()
                    .Where(IsPersistedOutfitFixture).ToArray();
                UnitEntityData[] markedCrossUnits = _crossBefore
                    .OfType<UnitEntityData>()
                    .Where(IsPersistedOutfitFixture).ToArray();
                int markedPartyCharacters = FixtureIdentityCount(
                    _partyCharactersBefore);
                int markedRemote = FixtureIdentityCount(_remoteBefore);
                int markedCross = FixtureIdentityCount(_crossBefore);
                _loadedFixtureMembership = new JObject
                {
                    { "globalUnits", markedUnits.Length },
                    { "party", markedParty.Length },
                    { "partyCharacters", markedPartyCharacters },
                    { "remoteCompanions", markedRemote },
                    { "crossScene", markedCross },
                    { "crossSceneUnits", markedCrossUnits.Length }
                };
                bool baselineShape = _partyBefore.Length ==
                        WorkingSaveSmokeScenario.ExpectedPartyCount &&
                    _partyCharactersBefore.Length ==
                        WorkingSaveSmokeScenario.ExpectedPartyCount &&
                    markedUnits.Length == 0 && markedParty.Length == 0 &&
                    markedPartyCharacters == 0 && markedRemote == 0 &&
                    markedCross == 0;
                if (_prepare)
                {
                    if (!baselineShape)
                        throw new InvalidOperationException(
                            "Persistence prepare requires the exact clean three-character working-save baseline with no fixture residue.");
                    WriteProgress("initialized-prepare");
                    return;
                }
                if (_verifyAbsent)
                {
                    _baselineAbsentExact = baselineShape;
                    if (!_baselineAbsentExact)
                        throw new InvalidOperationException(
                            "Fresh-load absence verification found fixture residue or a changed working-save baseline.");
                    WriteProgress("initialized-verify-absent");
                    return;
                }
                if (markedParty.Length == 1)
                    _persistedUnit = markedParty[0];
                bool globalIdentityExact = markedUnits.Length == 0 ||
                    markedUnits.Length == 1 && ReferenceEquals(
                        markedUnits[0], _persistedUnit);
                bool crossIdentityExact = markedCross == 1 &&
                    markedCrossUnits.Length == 1 && ReferenceEquals(
                        markedCrossUnits[0], _persistedUnit);
                if (_partyBefore.Length !=
                        WorkingSaveSmokeScenario.ExpectedPartyCount + 1 ||
                    _partyCharactersBefore.Length !=
                        WorkingSaveSmokeScenario.ExpectedPartyCount + 1 ||
                    markedParty.Length != 1 ||
                    markedPartyCharacters != 1 || markedRemote != 0 ||
                    !globalIdentityExact || !crossIdentityExact)
                    throw new InvalidOperationException(
                        "Fresh-load persistence verification requires one marker-bound party/reference/cross-scene unit and no remote residue; observed " +
                        _loadedFixtureMembership.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
                _persistedDiscovered =
                    _persistedUnit.Descriptor != null &&
                    _persistedUnit.Descriptor.Progression != null &&
                    _persistedUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) == 1 &&
                    _persistedUnit.Descriptor.Doll != null &&
                    _persistedUnit.Descriptor.ForcceUseClassEquipment &&
                    _persistedUnit.View != null &&
                    _persistedUnit.View.CharacterAvatar != null;
                _persistedForceUseClassEquipment = _persistedUnit.Descriptor
                    .ForcceUseClassEquipment;
                if (!_persistedDiscovered)
                    throw new InvalidOperationException(
                        "The exact persisted outfit fixture did not deserialize as one live level-1 Gunslinger with DollData.");
                _selectedPersistedUnitName =
                    _persistedUnit.CharacterName ?? "<unnamed>";
                _selectedPersistedUnitId =
                    _persistedUnit.UniqueId ?? "<null>";
                _persistedAvatar = _persistedUnit.View.CharacterAvatar;
                _persistedDollBefore = PersistenceDollSnapshot.Capture(
                    _persistedUnit.Descriptor.Doll);
                _persistedSerializedClassClothesAbsent =
                    SerializedClassClothesAbsent(_persistedDollBefore);
                _persistedAvatarBefore = TakeAvatarSnapshot(
                    _persistedAvatar);
                _persistedSavedLinksBefore = SavedLinks(
                    _persistedAvatar);
                _persistedEquipmentClassBefore = _equipmentClassField
                    .GetValue(_persistedUnit.View);
                WriteProgress("initialized-verify-cleanup");
            }

            private void BeginPersistedViewActivation()
            {
                if (_persistedViewActivationStarted) return;
                _stage = "activate-persisted-loaded-view";
                _persistedViewActivationStarted = true;
                PersistenceAppearanceObservation beforeAppearance =
                    ObserveAppearance(_persistedUnit,
                        _persistedUnit.Descriptor.Doll);
                _persistedPreActivationAppearanceExact =
                    beforeAppearance.ProductionPresent &&
                    beforeAppearance.LegacyAbsent &&
                    beforeAppearance.PaletteExact &&
                    beforeAppearance.RigExact;
                JObject before = DescribePersistedView(_persistedUnit);
                _persistedUnit.IsInGame = true;
                _persistedUnit.IsInFogOfWar = false;
                _persistedUnit.View.UpdateViewActive();
                _persistedUnit.View.SetVisible(true, true);
                _persistedViewActivationEvidence = new JObject
                {
                    { "nativeActions", new JArray(
                        "UnitEntityData.IsInGame=true",
                        "UnitEntityData.IsInFogOfWar=false",
                        "UnitEntityView.UpdateViewActive()",
                        "UnitEntityView.SetVisible(true,true)") },
                    { "before", before },
                    { "preActivationAppearanceExact",
                        _persistedPreActivationAppearanceExact },
                    { "preActivationAppearance",
                        beforeAppearance.Evidence },
                    { "afterImmediate",
                        DescribePersistedView(_persistedUnit) }
                };
                _settleUpdates = 0;
                WriteProgress("persisted-view-activation-started");
            }

            private JObject DescribePersistedView(UnitEntityData actor)
            {
                if (actor == null || actor.View == null)
                    return new JObject { { "present", false } };
                Renderer[] renderers = actor.View
                    .GetComponentsInChildren<Renderer>(true);
                return new JObject
                {
                    { "present", true },
                    { "unitIsInGame", actor.IsInGame },
                    { "unitIsViewActive", actor.IsViewActive },
                    { "unitIsSleeping", actor.IsSleeping },
                    { "unitIsInFogOfWar", actor.IsInFogOfWar },
                    { "unitIsVisibleForPlayer",
                        actor.IsVisibleForPlayer },
                    { "viewIsInGame", actor.View.IsInGame },
                    { "viewIsVisible", actor.View.IsVisible },
                    { "viewActiveSelf",
                        actor.View.gameObject.activeSelf },
                    { "viewActiveInHierarchy",
                        actor.View.gameObject.activeInHierarchy },
                    { "viewDataExact",
                        ReferenceEquals(actor.View.Data, actor) },
                    { "rendererCount", renderers.Length },
                    { "enabledRendererCount", renderers.Count(value =>
                        value != null && value.enabled) },
                    { "activeRendererCount",
                        ActiveRenderers(actor).Length },
                    { "holdingStateType", actor.HoldingState == null ?
                        "<null>" : actor.HoldingState.GetType().FullName },
                    { "holdingStateMatchesAnchor", _anchor != null &&
                        ReferenceEquals(actor.HoldingState,
                            _anchor.HoldingState) },
                    { "holdingStateMatchesCrossScene", _player != null &&
                        ReferenceEquals(actor.HoldingState,
                            _player.CrossSceneState) }
                };
            }

            private PersistenceFixture[] BuildFixtures()
            {
                BlueprintRace human = BlueprintRoot.Instance.Progression
                    .CharacterRaces.Where(value => value != null)
                    .GroupBy(value => value.RaceId)
                    .Select(group => group.OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).First())
                    .Single(value => value.RaceId == Race.Human);
                BlueprintRaceVisualPreset preset = human.Presets
                    .Where(value => value != null && value.Skin != null &&
                        value.MaleSkeleton != null &&
                        value.FemaleSkeleton != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).FirstOrDefault();
                if (preset == null)
                    throw new InvalidOperationException(
                        "No complete native Human visual preset supports both respec fixtures.");
                return new[]
                {
                    new PersistenceFixture(Gender.Male, human, preset),
                    new PersistenceFixture(Gender.Female, human, preset)
                };
            }

            private static bool IsPersistedOutfitFixture(
                UnitEntityData unit)
            {
                return unit != null && string.Equals(unit.UniqueId,
                        PersistedOutfitFixtureUniqueId,
                        StringComparison.Ordinal) &&
                    unit.Descriptor != null && string.Equals(
                        unit.Descriptor.CustomName,
                        PersistedOutfitFixtureName,
                        StringComparison.Ordinal);
            }

            private static int FixtureIdentityCount(IEnumerable<object> values)
            {
                if (values == null) return 0;
                int count = 0;
                foreach (object value in values)
                {
                    UnitEntityData unit = value as UnitEntityData;
                    if (unit != null)
                    {
                        if (IsPersistedOutfitFixture(unit)) count++;
                        continue;
                    }
                    if (value is UnitReference && string.Equals(
                            ((UnitReference)value).UniqueId,
                            PersistedOutfitFixtureUniqueId,
                            StringComparison.Ordinal))
                        count++;
                }
                return count;
            }

            private bool ResourcesReady(string label)
            {
                _stage = "wait-resources-" + label;
                if (!ResourcesLibrary.Preloading) return true;
                _settleUpdates++;
                if (_settleUpdates < MaximumSettleUpdates) return false;
                throw new InvalidOperationException(label +
                    " resource preloading did not finish.");
            }

            private bool PollActorReady(UnitEntityData actor, string label)
            {
                _stage = "settle-" + label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                if (actor != null && actor.View != null &&
                    actor.View.AnimationManager != null)
                    actor.View.AnimationManager.Tick();
                bool ready = actor != null && actor.View != null &&
                    actor.View.Data != null &&
                    actor.View.CharacterAvatar != null &&
                    HasExactHumanoidRig(actor.View.transform) &&
                    ActiveRenderers(actor).Length > 0;
                if (ready && _settleUpdates >= MinimumSettleUpdates)
                    return true;
                if (_settleUpdates < MaximumSettleUpdates) return false;
                throw new InvalidOperationException(label +
                    " did not settle to a rendered humanoid view; updates=" +
                    _settleUpdates + ".");
            }

            private void CapturePersistedLoadedAndBeginReconstruction()
            {
                _stage = "capture-persisted-loaded";
                PersistenceAppearanceObservation loaded = ObserveAppearance(
                    _persistedUnit, _persistedUnit.Descriptor.Doll);
                _persistedViewActivationEvidence["afterSettled"] =
                    DescribePersistedView(_persistedUnit);
                _persistedViewActivationExact =
                    _persistedPreActivationAppearanceExact &&
                    _persistedUnit.IsInGame &&
                    _persistedUnit.IsViewActive &&
                    _persistedUnit.IsVisibleForPlayer &&
                    _persistedUnit.View.IsInGame &&
                    _persistedUnit.View.IsVisible &&
                    _persistedUnit.View.gameObject.activeInHierarchy &&
                    ActiveRenderers(_persistedUnit).Length > 0;
                _persistedViewActivationEvidence["exact"] =
                    _persistedViewActivationExact;
                _persistedLoadedExact = loaded.Exact &&
                    _persistedViewActivationExact;
                Capture("persisted-loaded", _persistedUnit,
                    _persistedDollBefore, loaded,
                    "actual guarded working-save unit after Steam-backed load");

                _stage = "force-native-persisted-class-reconstruction";
                foreach (EquipmentEntity entity in ClassClothes(
                    _persistedDollBefore.Gender))
                    if (_persistedAvatar.EquipmentEntities.Any(value =>
                            ReferenceEquals(value, entity)))
                        _persistedAvatar.RemoveEquipmentEntity(entity, false);
                _equipmentClassField.SetValue(_persistedUnit.View, null);
                _persistedUnit.View.UpdateClassEquipment();
                _persistedAvatar.RebuildOutfit();
                WriteProgress("persisted-reconstruction-started");
            }

            private void CapturePersistedReconstructionAndRestore()
            {
                _stage = "capture-persisted-reconstruction";
                PersistenceAppearanceObservation reconstructed =
                    ObserveAppearance(_persistedUnit,
                        _persistedUnit.Descriptor.Doll);
                _persistedDollUnchanged = _persistedDollBefore.Matches(
                    _persistedUnit.Descriptor.Doll);
                _persistedReconstructionExact = reconstructed.Exact &&
                    _persistedDollUnchanged && ReferenceEquals(
                        _equipmentClassField.GetValue(_persistedUnit.View),
                        _gunslingerClass);
                Capture("persisted-native-class-reconstruction",
                    _persistedUnit, _persistedDollBefore, reconstructed,
                    "UnitEntityView.UpdateClassEquipment plus Character.RebuildOutfit using loaded class and DollData palette");
                _persistedRestored = RestorePersistedAvatar();
                if (!_persistedRestored)
                    throw new InvalidOperationException(
                        "The real loaded Gunslinger avatar did not restore exactly after reconstruction observation.");
                WriteProgress("persisted-restored");
            }

            private void StartRespecFixture()
            {
                PersistenceFixture fixture = _fixtures[_fixtureIndex];
                _stage = "spawn-respec-base-" + fixture.Label;
                if (ResourcesLibrary.Preloading)
                    throw new InvalidOperationException(
                        "Resource preloading resumed before respec DollData creation.");
                _initialDollData = CreateFighterDollData(fixture);
                SpawnActor(_initialDollData, fixture, false);
                WriteProgress("respec-base-spawned");
            }

            private DollData CreateFighterDollData(
                PersistenceFixture fixture)
            {
                DollState doll = new DollState();
                doll.SetGender(fixture.Gender);
                doll.SetRace(fixture.Race);
                doll.SetRacePreset(fixture.Preset);
                doll.SetClass(_fighterClass);
                EquipmentEntityLink hair = GetHairLinks(doll)
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId))
                    .OrderBy(value => value.AssetId,
                        StringComparer.Ordinal).FirstOrDefault();
                if (hair == null)
                    throw new InvalidOperationException(fixture.Label +
                        " exposes no deterministic native hairstyle.");
                doll.SetHair(hair);
                _selectedHairAssetId = hair.AssetId;
                if (doll.GetSkinRamps().Count > 0) doll.SetSkinColor(0);
                if (doll.GetHairRamps().Count > 0) doll.SetHairColor(0);
                DollData data = doll.CreateData();
                if (data == null)
                    throw new InvalidOperationException(fixture.Label +
                        " did not produce native Fighter DollData.");
                return data;
            }

            private static List<EquipmentEntityLink> GetHairLinks(
                DollState doll)
            {
                MethodInfo method = typeof(DollState).GetMethod(
                    "GetHairEntities", BindingFlags.Instance |
                        BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (method == null || method.ReturnType !=
                        typeof(List<EquipmentEntityLink>))
                    throw new MissingMethodException(
                        typeof(DollState).FullName,
                        "GetHairEntities() : List<EquipmentEntityLink>");
                var result = method.Invoke(doll, null) as
                    List<EquipmentEntityLink>;
                if (result == null)
                    throw new InvalidOperationException(
                        "Native DollState hair enumeration returned null.");
                return result;
            }

            private void SpawnActor(DollData data,
                PersistenceFixture fixture, bool persistedIdentity)
            {
                BlueprintUnit source = BlueprintRoot.Instance
                    .DefaultPlayerCharacter;
                if (source == null || source.Prefab == null ||
                    source.Body == null)
                    throw new InvalidOperationException(
                        "The native default player donor is incomplete.");
                _actorBlueprint = UnityEngine.Object.Instantiate(source);
                _actorBlueprint.Race = fixture.Race;
                _actorBlueprint.Gender = fixture.Gender;
                _actorBlueprint.Body = CreateNeutralBody(source);
                _actorBlueprint.StartingInventory =
                    new BlueprintItem[0];
                _actorBlueprint.name =
                    "KMG_Runtime_Gunslinger_Outfit_Persistence_" +
                    fixture.Label.Replace('-', '_');
                _actorBlueprint.IsCheater = true;
                UnitEntityView dollView = null;
                try
                {
                    dollView = data.CreateUnitView(false);
                    if (dollView == null)
                        throw new InvalidOperationException(fixture.Label +
                            " DollData did not create a native view.");
                    dollView.Blueprint = _actorBlueprint;
                    dollView.UniqueId = persistedIdentity
                        ? PersistedOutfitFixtureUniqueId
                        : Guid.NewGuid().ToString();
                    dollView.transform.position = NearestNavigable(
                        _anchor.Position + new Vector3(-3.5f, 0f,
                            3.5f + _fixtureIndex * 1.5f));
                    dollView.transform.rotation = Quaternion.identity;
                    _actor = Game.Instance.EntityCreator
                        .SpawnEntityWithView(dollView,
                            _anchor.HoldingState) as UnitEntityData;
                    if (_actor == null ||
                        !ReferenceEquals(_actor.View, dollView))
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
                _actor.Descriptor.Doll = data;
                _actor.Descriptor.ForcceUseClassEquipment = true;
                if (persistedIdentity)
                    _actor.Descriptor.CustomName =
                        PersistedOutfitFixtureName;
                _actor.Descriptor.State.Immortality.Retain();
                _actor.Commands.InterruptAll(true);
                if (_actor.CombatState.IsInCombat)
                    _actor.CombatState.LeaveCombat();
                _avatar = _actor.View.CharacterAvatar;
            }

            private static BlueprintUnit.UnitBody CreateNeutralBody(
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

            private void PerformNativeRespec()
            {
                PersistenceFixture fixture = _fixtures[_fixtureIndex];
                _stage = "native-respec-" + fixture.Label;
                Type type = typeof(LevelUpController);
                MethodInfo select = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) },
                    null);
                MethodInfo mechanics = type.GetMethod(
                    "ApplyClassMechanics", BindingFlags.Public |
                        BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (select == null || mechanics == null || apply == null ||
                    cancel == null || commit == null)
                    throw new MissingMethodException(
                        "The exact native respec method surface is unavailable.");

                _seedController = LevelUpController
                    .StartWithoutAssigningStaticInstance(_actor.Descriptor,
                        false, null, null,
                        LevelUpState.CharBuildMode.CharGen);
                if (_seedController == null || !(bool)select.Invoke(
                        _seedController,
                        new object[] { _fighterClass, false }))
                    throw new InvalidOperationException(fixture.Label +
                        " Fighter source seed was rejected.");
                mechanics.Invoke(_seedController, null);
                apply.Invoke(_seedController,
                    new object[] { _actor.Descriptor });
                cancel.Invoke(_seedController, null);
                _seedController = null;
                _actor.View.UpdateClassEquipment();
                bool fighterSourceExact =
                    _actor.Descriptor.Progression.GetClassLevel(
                        _fighterClass) == 1 &&
                    LegacyEntitiesPresent(_actor,
                        fixture.Gender);

                if (_respecSourceActor != null ||
                    _respecSourceBlueprint != null)
                    throw new InvalidOperationException(
                        "A previous native Respec source was not retired.");
                _respecSourceActor = _actor;
                _respecSourceBlueprint = _actorBlueprint;
                string sourceActorId = _respecSourceActor.UniqueId;
                _actor = null;
                _actorBlueprint = null;
                _avatar = null;
                _initialDollData = CreateFighterDollData(fixture);
                SpawnActor(_initialDollData, fixture, _prepare);
                int replacementLevelBeforeRespec = _actor.Descriptor
                    .Progression.CharacterLevel;
                bool distinctSourceAndReplacement = !ReferenceEquals(
                    _respecSourceActor, _actor) &&
                    !ReferenceEquals(_respecSourceActor.Descriptor,
                        _actor.Descriptor);
                if (replacementLevelBeforeRespec != 0 ||
                    !distinctSourceAndReplacement)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec requires a distinct level-0 replacement descriptor.");

                bool callback = false;
                Action onSuccess = () => callback = true;
                _respecController = LevelUpController
                    .StartWithoutAssigningStaticInstance(_actor.Descriptor,
                        false, null, onSuccess,
                        LevelUpState.CharBuildMode.Respec);
                if (_respecController == null)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec controller returned null.");
                if (_respecController.State == null ||
                    _respecController.State.Mode !=
                        LevelUpState.CharBuildMode.Respec)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec controller has the wrong build mode.");
                if (_respecController.Doll == null)
                    throw new InvalidOperationException(fixture.Label +
                        " native Respec controller has no DollState despite exact Respec mode and AutoCommit=false.");
                string respecMode = _respecController.State.Mode.ToString();
                DollState doll = _respecController.Doll;
                doll.SetGender(fixture.Gender);
                doll.SetRace(fixture.Race);
                doll.SetRacePreset(fixture.Preset);
                EquipmentEntityLink hair = GetHairLinks(doll)
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId))
                    .OrderBy(value => value.AssetId,
                        StringComparer.Ordinal).FirstOrDefault();
                if (!(bool)select.Invoke(_respecController,
                        new object[] { _gunslingerClass, false }))
                    throw new InvalidOperationException(fixture.Label +
                        " Gunslinger respec selection was rejected.");
                if (hair != null)
                {
                    doll.SetHair(hair);
                    _selectedHairAssetId = hair.AssetId;
                }
                if (doll.GetSkinRamps().Count > 0) doll.SetSkinColor(0);
                if (doll.GetHairRamps().Count > 0) doll.SetHairColor(0);
                mechanics.Invoke(_respecController, null);
                int previewFighter = _respecController.Preview.Progression
                    .GetClassLevel(_fighterClass);
                int previewGunslinger = _respecController.Preview.Progression
                    .GetClassLevel(_gunslingerClass);
                commit.Invoke(_respecController, null);
                _respecController = null;
                RetireRespecSource();
                _starterRollbackExact = RollbackStarterGrants() &&
                    _starterRollbackExact;
                _respecDoll = PersistenceDollSnapshot.Capture(
                    _actor.Descriptor.Doll);
                bool serializedClean = SerializedClassClothesAbsent(
                    _respecDoll);
                _respecSerializedClassClothesAbsent =
                    _respecSerializedClassClothesAbsent && serializedClean;
                BlueprintCharacterClass postCommitEquipmentClass =
                    _equipmentClassField.GetValue(_actor.View) as
                        BlueprintCharacterClass;
                _equipmentClassField.SetValue(_actor.View, null);
                _actor.View.UpdateClassEquipment();
                BlueprintCharacterClass postRefreshEquipmentClass =
                    _equipmentClassField.GetValue(_actor.View) as
                        BlueprintCharacterClass;
                _avatar = _actor.View.CharacterAvatar;
                _avatar.RebuildOutfit();
                _respecRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "sourceFighterExact", fighterSourceExact },
                    { "sourceActorId", sourceActorId },
                    { "replacementActorId", _actor.UniqueId },
                    { "replacementLevelBeforeRespec",
                        replacementLevelBeforeRespec },
                    { "distinctSourceAndReplacement",
                        distinctSourceAndReplacement },
                    { "respecMode", respecMode },
                    { "selected", true },
                    { "callback", callback },
                    { "previewFighterLevel", previewFighter },
                    { "previewGunslingerLevel", previewGunslinger },
                    { "committedFighterLevel",
                        _actor.Descriptor.Progression.GetClassLevel(
                            _fighterClass) },
                    { "committedGunslingerLevel",
                        _actor.Descriptor.Progression.GetClassLevel(
                            _gunslingerClass) },
                    { "serializedClassClothesAbsent", serializedClean },
                    { "dollData", _respecDoll.Describe() },
                    { "postCommitEquipmentClassGuid",
                        postCommitEquipmentClass == null ? "<null>" :
                            postCommitEquipmentClass.AssetGuid },
                    { "postRefreshEquipmentClassExact",
                        ReferenceEquals(postRefreshEquipmentClass,
                            _gunslingerClass) },
                    { "selectedHairAssetId", _selectedHairAssetId },
                    { "starterGrantsRolledBack",
                        _starterRollbackExact },
                    { "forceUseClassEquipment",
                        _actor.Descriptor.ForcceUseClassEquipment }
                });
                WriteProgress("respec-committed");
            }

            private static bool RespecRecordExact(JObject value)
            {
                return value != null &&
                    (bool)value["sourceFighterExact"] &&
                    (bool)value["distinctSourceAndReplacement"] &&
                    (int)value["replacementLevelBeforeRespec"] == 0 &&
                    string.Equals((string)value["respecMode"], "Respec",
                        StringComparison.Ordinal) &&
                    (bool)value["selected"] && (bool)value["callback"] &&
                    (int)value["previewFighterLevel"] == 0 &&
                    (int)value["previewGunslingerLevel"] == 1 &&
                    (int)value["committedFighterLevel"] == 0 &&
                    (int)value["committedGunslingerLevel"] == 1 &&
                    (bool)value["serializedClassClothesAbsent"] &&
                    (bool)value["postRefreshEquipmentClassExact"] &&
                    (bool)value["levelsExact"] &&
                    (bool)value["defaultPaletteExact"] &&
                    (bool)value["appearanceExact"] &&
                    (bool)value["starterGrantsRolledBack"] &&
                    (bool)value["forceUseClassEquipment"];
            }

            private void CaptureRespecResult()
            {
                PersistenceFixture fixture = _fixtures[_fixtureIndex];
                _stage = "capture-respec-result-" + fixture.Label;
                PersistenceAppearanceObservation observation =
                    ObserveAppearance(_actor, _actor.Descriptor.Doll);
                bool levelsExact = _actor.Descriptor.Progression
                        .GetClassLevel(_fighterClass) == 0 &&
                    _actor.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) == 1;
                bool defaultsExact = _respecDoll.ClothesPrimaryIndex ==
                        GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor &&
                    _respecDoll.ClothesSecondaryIndex ==
                        GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor;
                JObject respec = (JObject)_respecRecords[
                    _respecRecords.Count - 1];
                respec["levelsExact"] = levelsExact;
                respec["defaultPaletteExact"] = defaultsExact;
                respec["appearanceExact"] = observation.Exact;
                Capture("native-respec-" + fixture.Label, _actor,
                    _respecDoll, observation,
                    "Fighter seed to Gunslinger native Respec Commit, UpdateClassEquipment, and RebuildOutfit");
                WriteProgress("respec-captured");
            }

            private void PromoteActorForPersistence()
            {
                _stage = "promote-exact-persistence-fixture";
                if (!_prepare || _actor == null ||
                    !IsPersistedOutfitFixture(_actor) ||
                    _actor.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) != 1 ||
                    _actor.Descriptor.Doll == null ||
                    _actor.Descriptor.Pet != null ||
                    !SerializedClassClothesAbsent(
                        PersistenceDollSnapshot.Capture(
                            _actor.Descriptor.Doll)))
                    throw new InvalidOperationException(
                        "Only the exact native-respec outfit fixture may enter the disposable working save.");
                UnitReference actorReference = _actor;
                if (_player.PartyCharacters.Any(value => string.Equals(
                        value.UniqueId, PersistedOutfitFixtureUniqueId,
                        StringComparison.Ordinal)))
                    throw new InvalidOperationException(
                        "The exact persistence fixture reference already exists.");
                _actor.Descriptor.State.Immortality.ReleaseAll();
                _gunslingerClass.StartingGold = _startingGoldBefore;
                _player.PartyCharacters.Add(actorReference);
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();
                _persistedPromoted =
                    _player.Party.Contains(_actor) &&
                    _player.PartyCharacters.Count(value => string.Equals(
                        value.UniqueId, PersistedOutfitFixtureUniqueId,
                        StringComparison.Ordinal)) == 1 &&
                    ContainsReference(_allUnits, _actor) &&
                    _actor.HoldingState != null &&
                    _actor.HoldingState.AllEntityData.Any(value =>
                        ReferenceEquals(value, _actor)) &&
                    _partyBefore.Length + 1 == Snapshot(_party).Length &&
                    _partyCharactersBefore.Length + 1 ==
                        _player.PartyCharacters.Count;
                if (!_persistedPromoted)
                    throw new InvalidOperationException(
                        "The exact outfit fixture did not enter one serializable party and area state.");
                _persistedUnit = _actor;
                _persistedAvatar = _actor.View.CharacterAvatar;
                _persistedDollBefore = PersistenceDollSnapshot.Capture(
                    _actor.Descriptor.Doll);
                _persistedSerializedClassClothesAbsent =
                    SerializedClassClothesAbsent(_persistedDollBefore);
                _persistedDiscovered = true;
                _selectedPersistedUnitName =
                    _actor.CharacterName ?? "<unnamed>";
                _selectedPersistedUnitId = _actor.UniqueId ?? "<null>";
                _structuralCleaned = _persistedPromoted;
                WriteProgress("fixture-promoted-for-save");
            }

            private void StartExactWorkingSave()
            {
                _stage = _prepare ? "save-prepared-fixture" :
                    "save-fixture-cleanup";
                if (_saveStarted)
                    throw new InvalidOperationException(
                        "The exact working-save write was already started.");
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
                    string message =
                        "An unarmed, non-working, destructive, migration, or extra save boundary was observed.";
                    _exceptionSummary = message;
                    Add(_assertions,
                        "gunslinger-outfit-production-persistence-save-exception",
                        "only one armed exact-working-save write", message,
                        false, "request-scoped native save sentinel");
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(false);
                    return;
                }
                if (_saveCompleted)
                {
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(_structuralCleaned);
                    return;
                }
                if (_saveElapsed != null &&
                    _saveElapsed.Elapsed.TotalSeconds >=
                        _request.CompletionTimeoutSeconds)
                {
                    string message =
                        "The exact working-save completion callback did not arrive before timeout.";
                    _exceptionSummary = message;
                    Add(_assertions,
                        "gunslinger-outfit-production-persistence-save-exception",
                        "completion callback before timeout", message, false,
                        "exact Game.SaveGame callback");
                    _workingSaveEvidence = _workingSaveSmoke.Stop();
                    Finish(false);
                }
            }

            private PersistenceAppearanceObservation ObserveAppearance(
                UnitEntityData actor, DollData data)
            {
                if (actor == null || actor.View == null ||
                    actor.View.CharacterAvatar == null || data == null)
                    throw new InvalidOperationException(
                        "Appearance observation requires a live actor and DollData.");
                Character avatar = actor.View.CharacterAvatar;
                string[] productionIds = ProductionIds(data.Gender);
                string[] legacyIds = LegacyIds(data.Gender);
                EquipmentEntity[] production = productionIds.Select(id =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        id, true)).ToArray();
                EquipmentEntity[] legacy = legacyIds.Select(id =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        id, true)).Where(value => value != null).ToArray();
                bool productionPresent = production.All(value =>
                    value != null && avatar.EquipmentEntities.Count(current =>
                        ReferenceEquals(current, value)) == 1);
                bool legacyAbsent = legacy.All(value =>
                    !avatar.EquipmentEntities.Any(current =>
                        ReferenceEquals(current, value)));
                bool paletteExact = data.ClothesPrimaryIndex >= 0 &&
                    data.ClothesSecondaryIndex >= 0 &&
                    production.Where(value => value != null).All(value =>
                        avatar.GetPrimaryRampIndex(value) ==
                            data.ClothesPrimaryIndex &&
                        avatar.GetSecondaryRampIndex(value) ==
                            data.ClothesSecondaryIndex);
                int renderers = ActiveRenderers(actor).Length;
                bool rig = HasExactHumanoidRig(actor.View.transform);
                object equipmentClass = _equipmentClassField == null
                    ? null : _equipmentClassField.GetValue(actor.View);
                var ramps = new JArray(production.Select((value, index) =>
                    new JObject
                    {
                        { "assetId", productionIds[index] },
                        { "resolved", value != null },
                        { "present", value != null &&
                            avatar.EquipmentEntities.Any(current =>
                                ReferenceEquals(current, value)) },
                        { "primary", value == null ? -1 :
                            avatar.GetPrimaryRampIndex(value) },
                        { "secondary", value == null ? -1 :
                            avatar.GetSecondaryRampIndex(value) }
                    }).ToArray());
                return new PersistenceAppearanceObservation
                {
                    ProductionPresent = productionPresent,
                    LegacyAbsent = legacyAbsent,
                    PaletteExact = paletteExact,
                    RigExact = rig,
                    RendererCount = renderers,
                    Evidence = new JObject
                    {
                        { "productionAssetIds", new JArray(productionIds) },
                        { "legacyFighterAssetIds", new JArray(legacyIds) },
                        { "productionPresent", productionPresent },
                        { "legacyFighterAbsent", legacyAbsent },
                        { "paletteExact", paletteExact },
                        { "clothesPrimaryIndex",
                            data.ClothesPrimaryIndex },
                        { "clothesSecondaryIndex",
                            data.ClothesSecondaryIndex },
                        { "productionRamps", ramps },
                        { "equipmentClassGuid",
                            equipmentClass is BlueprintCharacterClass
                                ? ((BlueprintCharacterClass)equipmentClass)
                                    .AssetGuid : "<null>" },
                        { "rigExact", rig },
                        { "activeRendererCount", renderers },
                        { "activeEntityNames", new JArray(
                            avatar.EquipmentEntities.Where(value =>
                                value != null).Select(value =>
                                value.name).ToArray()) }
                    }
                };
            }

            private void Capture(string label, UnitEntityData actor,
                PersistenceDollSnapshot doll,
                PersistenceAppearanceObservation observation,
                string boundary)
            {
                string stem = SafeFileName("production-persistence-" +
                    label);
                string previewPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-preview.png");
                string isometricPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-isometric.png");
                Renderer[] renderers = ActiveRenderers(actor);
                WeaponPresentationEvidenceScenario.CaptureSummary preview =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        actor, null, renderers, previewPath, true);
                IsometricCapture isometric = CaptureIsometric(actor,
                    renderers, isometricPath);
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "label", label },
                    { "boundary", boundary },
                    { "unitName", actor.CharacterName ?? "<unnamed>" },
                    { "unitId", actor.UniqueId ?? "<null>" },
                    { "dollData", doll.Describe() },
                    { "serializedClassClothesAbsent",
                        SerializedClassClothesAbsent(doll) },
                    { "appearance", observation.Evidence },
                    { "appearanceExact", observation.Exact },
                    { "preview", new JObject
                        {
                            { "file", Path.GetFileName(preview.PngPath) },
                            { "bytes", preview.Bytes },
                            { "sha256", preview.Sha256 },
                            { "meaningfulPixels",
                                preview.MeaningfulPixels },
                            { "framing", preview.Framing },
                            { "lowPixelDensity",
                                preview.LowPixelDensity },
                            { "views", 4 }
                        } },
                    { "isometric", new JObject
                        {
                            { "file", Path.GetFileName(isometric.Path) },
                            { "bytes", isometric.Bytes },
                            { "sha256", isometric.Sha256 },
                            { "meaningfulPixels",
                                isometric.MeaningfulPixels },
                            { "rendererCount",
                                isometric.RendererCount },
                            { "bounds", isometric.Bounds },
                            { "framing", isometric.Framing },
                            { "lowPixelDensity",
                                isometric.LowPixelDensity },
                            { "views", 1 }
                        } },
                    { "saveApiCalledAtCapture", false },
                    { "exactWorkingSavePhase", _prepare ?
                        "prepare-before-save" :
                        "verify-before-cleanup-save" },
                    { "productionBlueprintMutated", false }
                };
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
                    _warnings.Add(label +
                        " has low foreground pixel density; retain it as a framing diagnostic.");
            }

            private bool RestorePersistedAvatar()
            {
                if (_persistedAvatar == null ||
                    _persistedUnit == null ||
                    _persistedUnit.View == null) return false;
                _persistedAvatar.RemoveAllEquipmentEntities(false);
                foreach (AvatarEntityState state in _persistedAvatarBefore)
                    _persistedAvatar.AddEquipmentEntity(state.Entity, false);
                foreach (AvatarEntityState state in _persistedAvatarBefore)
                    if (state.Primary >= 0 && state.Secondary >= 0)
                        _persistedAvatar.SetRampIndices(state.Entity,
                            state.Primary, state.Secondary, false);
                    else if (state.Primary >= 0)
                        _persistedAvatar.SetPrimaryRampIndex(state.Entity,
                            state.Primary, false);
                    else if (state.Secondary >= 0)
                        _persistedAvatar.SetSecondaryRampIndex(state.Entity,
                            state.Secondary, false);
                _equipmentClassField.SetValue(_persistedUnit.View,
                    _persistedEquipmentClassBefore);
                _persistedAvatar.RebuildOutfit();
                EquipmentEntity[] current = _persistedAvatar
                    .EquipmentEntities.Where(value => value != null)
                    .ToArray();
                bool exactEntities = current.Length ==
                        _persistedAvatarBefore.Length &&
                    current.Select((value, index) => ReferenceEquals(value,
                        _persistedAvatarBefore[index].Entity)).All(value =>
                            value);
                bool exactRamps = _persistedAvatarBefore.All(value =>
                    _persistedAvatar.GetPrimaryRampIndex(value.Entity) ==
                        value.Primary &&
                    _persistedAvatar.GetSecondaryRampIndex(value.Entity) ==
                        value.Secondary);
                return exactEntities && exactRamps &&
                    _persistedSavedLinksBefore.SequenceEqual(
                        SavedLinks(_persistedAvatar),
                        StringComparer.Ordinal) &&
                    _persistedDollBefore.Matches(
                        _persistedUnit.Descriptor.Doll) &&
                    ReferenceEquals(_equipmentClassField.GetValue(
                        _persistedUnit.View),
                        _persistedEquipmentClassBefore);
            }

            private static AvatarEntityState[] TakeAvatarSnapshot(
                Character avatar)
            {
                return avatar.EquipmentEntities.Where(value => value != null)
                    .Select(value => new AvatarEntityState
                    {
                        Entity = value,
                        Primary = avatar.GetPrimaryRampIndex(value),
                        Secondary = avatar.GetSecondaryRampIndex(value)
                    }).ToArray();
            }

            private static string[] SavedLinks(Character avatar)
            {
                return avatar.SavedEquipmentEntities.Select(value =>
                    value == null ? "<null>" :
                        value.AssetId ?? string.Empty).ToArray();
            }

            private EquipmentEntity[] ClassClothes(Gender gender)
            {
                return ProductionIds(gender).Concat(LegacyIds(gender))
                    .Select(id => ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(id, true))
                    .Where(value => value != null).Distinct().ToArray();
            }

            private static string[] ProductionIds(Gender gender)
            {
                return gender == Gender.Male
                    ? GunslingerClassAppearanceCatalog.MaleAssetIds()
                    : GunslingerClassAppearanceCatalog.FemaleAssetIds();
            }

            private static string[] LegacyIds(Gender gender)
            {
                return gender == Gender.Male
                    ? PersistedMaleFighterAssetIds.ToArray()
                    : PersistedFemaleFighterAssetIds.ToArray();
            }

            private static bool SerializedClassClothesAbsent(
                PersistenceDollSnapshot doll)
            {
                if (doll == null) return false;
                string[] classIds = ProductionIds(doll.Gender)
                    .Concat(LegacyIds(doll.Gender)).ToArray();
                return !doll.EquipmentEntityIds.Any(value =>
                    classIds.Contains(value, StringComparer.Ordinal));
            }

            private static bool LegacyEntitiesPresent(UnitEntityData actor,
                Gender gender)
            {
                if (actor == null || actor.View == null ||
                    actor.View.CharacterAvatar == null) return false;
                Character avatar = actor.View.CharacterAvatar;
                return LegacyIds(gender).Select(id => ResourcesLibrary
                        .TryGetResource<EquipmentEntity>(id, true))
                    .Where(value => value != null).All(value =>
                        avatar.EquipmentEntities.Any(current =>
                            ReferenceEquals(current, value)));
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
                    else if (excess < 0)
                        exact = false;
                }
                return exact && _startingItems.Select((value, index) =>
                    _player.Inventory.Count(value) ==
                        _startingItemCounts[index]).All(value => value);
            }

            private void RetireActor()
            {
                CancelControllers();
                if (_actor == null && _actorBlueprint == null &&
                    _respecSourceActor == null &&
                    _respecSourceBlueprint == null) return;
                try
                {
                    if (_actor != null)
                    {
                        UnitEntityData dependent = _actor.Descriptor == null
                            ? null : _actor.Descriptor.Pet;
                        if (dependent != null && !_unitsBefore.Any(value =>
                                ReferenceEquals(value, dependent)))
                        {
                            dependent.Commands.InterruptAll(true);
                            if (dependent.CombatState.IsInCombat)
                                dependent.CombatState.LeaveCombat();
                            if (ContainsReference(_party, dependent))
                                _player.Party.Remove(dependent);
                            RemoveRequestLocalEntity(dependent);
                        }
                        _actor.Commands.InterruptAll(true);
                        if (_actor.CombatState.IsInCombat)
                            _actor.CombatState.LeaveCombat();
                        if (_actor.Descriptor != null)
                            _actor.Descriptor.State.Immortality.ReleaseAll();
                        if (ContainsReference(_party, _actor))
                            _player.Party.Remove(_actor);
                        RemoveRequestLocalEntity(_actor);
                    }
                }
                finally
                {
                    try
                    {
                        RetireRespecSource();
                    }
                    finally
                    {
                        if (_actorBlueprint != null)
                            UnityEngine.Object.DestroyImmediate(
                                _actorBlueprint);
                        _actor = null;
                        _actorBlueprint = null;
                        _avatar = null;
                        _initialDollData = null;
                        _respecDoll = null;
                        _selectedHairAssetId = string.Empty;
                    }
                }
            }

            private void RetireRespecSource()
            {
                UnitEntityData source = _respecSourceActor;
                BlueprintUnit blueprint = _respecSourceBlueprint;
                _respecSourceActor = null;
                _respecSourceBlueprint = null;
                try
                {
                    if (source == null) return;
                    UnitEntityData dependent = source.Descriptor == null
                        ? null : source.Descriptor.Pet;
                    if (dependent != null && !_unitsBefore.Any(value =>
                            ReferenceEquals(value, dependent)))
                    {
                        dependent.Commands.InterruptAll(true);
                        if (dependent.CombatState.IsInCombat)
                            dependent.CombatState.LeaveCombat();
                        if (ContainsReference(_party, dependent))
                            _player.Party.Remove(dependent);
                        RemoveRequestLocalEntity(dependent);
                    }
                    source.Commands.InterruptAll(true);
                    if (source.CombatState.IsInCombat)
                        source.CombatState.LeaveCombat();
                    if (source.Descriptor != null)
                        source.Descriptor.State.Immortality.ReleaseAll();
                    if (ContainsReference(_party, source))
                        _player.Party.Remove(source);
                    RemoveRequestLocalEntity(source);
                }
                finally
                {
                    if (blueprint != null)
                        UnityEngine.Object.DestroyImmediate(blueprint);
                }
            }

            private void RemoveRequestLocalEntity(UnitEntityData unit)
            {
                if (unit == null) return;
                if (unit.HoldingState != null &&
                    unit.HoldingState.AllEntityData.Any(value =>
                        ReferenceEquals(value, unit)))
                {
                    unit.HoldingState.RemoveEntityData(unit);
                    return;
                }
                if (ContainsReference(_allUnits, unit))
                    Game.Instance.State.Units.All.Remove(unit);
                unit.Dispose();
            }

            private void CancelControllers()
            {
                MethodInfo cancel = typeof(LevelUpController).GetMethod(
                    "Cancel", BindingFlags.Public | BindingFlags.Instance);
                try
                {
                    if (_respecController != null && cancel != null)
                        cancel.Invoke(_respecController, null);
                }
                finally
                {
                    if (_seedController != null && cancel != null)
                        cancel.Invoke(_seedController, null);
                    _respecController = null;
                    _seedController = null;
                }
            }

            private void RemovePersistedFixtureFromState()
            {
                UnitEntityData unit = _persistedUnit;
                if (unit == null && _actor != null &&
                    IsPersistedOutfitFixture(_actor))
                {
                    unit = _actor;
                    _persistedUnit = unit;
                }
                if (unit == null || !IsPersistedOutfitFixture(unit))
                    throw new InvalidOperationException(
                        "Cleanup refused to remove a unit without the exact persistence marker identity.");
                if (unit.Descriptor != null && unit.Descriptor.Pet != null)
                    throw new InvalidOperationException(
                        "The exact persistence fixture unexpectedly owns a dependent entity.");
                int references = _player.PartyCharacters.Count(value =>
                    string.Equals(value.UniqueId,
                        PersistedOutfitFixtureUniqueId,
                        StringComparison.Ordinal));
                if (references != 1)
                    throw new InvalidOperationException(
                        "Cleanup requires exactly one marker-bound PartyCharacters reference; observed " +
                        references + ".");
                for (int index = _player.PartyCharacters.Count - 1;
                    index >= 0; index--)
                    if (string.Equals(
                            _player.PartyCharacters[index].UniqueId,
                            PersistedOutfitFixtureUniqueId,
                            StringComparison.Ordinal))
                        _player.PartyCharacters.RemoveAt(index);
                _player.InvalidateCharacterLists();
                _player.UpdateCharacterLists();
                unit.Commands.InterruptAll(true);
                if (unit.CombatState.IsInCombat)
                    unit.CombatState.LeaveCombat();
                if (unit.Descriptor != null)
                    unit.Descriptor.State.Immortality.ReleaseAll();
                if (unit.HoldingState == null ||
                    !unit.HoldingState.AllEntityData.Any(value =>
                        ReferenceEquals(value, unit)))
                    throw new InvalidOperationException(
                        "The exact persistence fixture is not owned by one removable scene state.");
                unit.HoldingState.RemoveEntityData(unit);
                if (ReferenceEquals(_actor, unit))
                {
                    _actor = null;
                    _avatar = null;
                    _initialDollData = null;
                    _respecDoll = null;
                    _selectedHairAssetId = string.Empty;
                    if (_actorBlueprint != null)
                        UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                    _actorBlueprint = null;
                }
                _persistedRemoved = true;
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "persistence-cleanup";
                try
                {
                    CancelControllers();
                    if (!_prepare && !_verifyAbsent &&
                        _persistedUnit == null && _party != null)
                    {
                        UnitEntityData[] cleanupCandidates = Snapshot(_party)
                            .OfType<UnitEntityData>()
                            .Where(IsPersistedOutfitFixture).ToArray();
                        if (cleanupCandidates.Length == 1)
                            _persistedUnit = cleanupCandidates[0];
                    }
                    bool promotedPrepareActor = _prepare && _actor != null &&
                        IsPersistedOutfitFixture(_actor) &&
                        _player.PartyCharacters.Any(value => string.Equals(
                            value.UniqueId,
                            PersistedOutfitFixtureUniqueId,
                            StringComparison.Ordinal));
                    if (promotedPrepareActor)
                    {
                        _persistedUnit = _actor;
                        RemovePersistedFixtureFromState();
                    }
                    else
                    {
                        RetireActor();
                    }
                    _starterRollbackExact = RollbackStarterGrants() &&
                        _starterRollbackExact;
                    if (_gunslingerClass != null)
                        _gunslingerClass.StartingGold =
                            _startingGoldBefore;
                    if (_persistedUnit != null && !_persistedRestored &&
                        _persistedAvatarBefore.Length > 0 && !_prepare)
                        _persistedRestored = RestorePersistedAvatar();
                    if (!_prepare && !_verifyAbsent &&
                        _persistedUnit != null && !_persistedRemoved)
                        RemovePersistedFixtureFromState();
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("cleanupException=" + exception);
                }
                _cleanupStarted = true;
                _settleUpdates = 0;
                WriteProgress("cleanup-started");
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                object[] expectedUnits = !_prepare && !_verifyAbsent
                    ? _unitsBefore.Where(value =>
                        !HasPersistedFixtureIdentity(value)).ToArray()
                    : _unitsBefore;
                object[] expectedParty = !_prepare && !_verifyAbsent
                    ? _partyBefore.Where(value =>
                        !HasPersistedFixtureIdentity(value)).ToArray()
                    : _partyBefore;
                object[] expectedPartyCharacters =
                    !_prepare && !_verifyAbsent
                        ? _partyCharactersBefore.Where(value =>
                            !HasPersistedFixtureIdentity(value)).ToArray()
                        : _partyCharactersBefore;
                object[] expectedRemote = !_prepare && !_verifyAbsent
                    ? _remoteBefore.Where(value =>
                        !HasPersistedFixtureIdentity(value)).ToArray()
                    : _remoteBefore;
                object[] expectedCross = !_prepare && !_verifyAbsent
                    ? _crossBefore.Where(value =>
                        !HasPersistedFixtureIdentity(value)).ToArray()
                    : _crossBefore;
                object[] currentUnits = Snapshot(_allUnits);
                object[] currentParty = Snapshot(_party);
                object[] currentPartyCharacters = _player.PartyCharacters
                    .Cast<object>().ToArray();
                bool cleaned = _actor == null &&
                    _respecSourceActor == null &&
                    _respecSourceBlueprint == null &&
                    SameReferences(expectedUnits, currentUnits) &&
                    SameReferences(expectedParty, currentParty) &&
                    SameValues(expectedPartyCharacters,
                        currentPartyCharacters) &&
                    SameValues(expectedRemote, Snapshot(_remote)) &&
                    SameReferences(expectedCross, Snapshot(_cross)) &&
                    SameReferences(_inventoryBefore, Snapshot(_inventory)) &&
                    (_player == null || _player.Money == _moneyBefore) &&
                    (_gunslingerClass == null ||
                        _gunslingerClass.StartingGold ==
                            _startingGoldBefore) &&
                    _starterRollbackExact &&
                    (_prepare || _verifyAbsent ||
                        _persistedAvatarBefore.Length == 0 ||
                        _persistedRestored) &&
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

            private static bool HasPersistedFixtureIdentity(object value)
            {
                UnitEntityData unit = value as UnitEntityData;
                if (unit != null) return IsPersistedOutfitFixture(unit);
                return value is UnitReference && string.Equals(
                    ((UnitReference)value).UniqueId,
                    PersistedOutfitFixtureUniqueId,
                    StringComparison.Ordinal);
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

            private void WriteProgress(string progressStage)
            {
                var progress = new JObject
                {
                    { "schemaVersion", 1 },
                    { "utc", DateTime.UtcNow.ToString("o") },
                    { "stage", progressStage },
                    { "detailStage", _stage },
                    { "phase", _phase },
                    { "fixtureIndex", _fixtureIndex },
                    { "settleUpdates", _settleUpdates },
                    { "captured", _captured },
                    { "actorPresent", _actor != null }
                };
                WriteJsonAtomic(Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-production-persistence-progress.json"),
                    progress);
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
                        "verify-absent" : "verify-cleanup" },
                    { "loadedModVersion",
                        _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _gameAssemblySha256 },
                    { "gameAssemblyMvid", _gameAssemblyMvid },
                    { "productionClassGuid", _gunslingerClass == null
                        ? OutfitProductionClassGuid :
                        _gunslingerClass.AssetGuid },
                    { "maleProductionAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.MaleAssetIds()) },
                    { "femaleProductionAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.FemaleAssetIds()) },
                    { "maleHistoricalFighterAssetIds", new JArray(
                        PersistedMaleFighterAssetIds) },
                    { "femaleHistoricalFighterAssetIds", new JArray(
                        PersistedFemaleFighterAssetIds) },
                    { "party", _partyRecords },
                    { "loadedFixtureMembership",
                        _loadedFixtureMembership },
                    { "fixtureUniqueId",
                        PersistedOutfitFixtureUniqueId },
                    { "fixtureName", PersistedOutfitFixtureName },
                    { "selectedPersistedUnit",
                        _selectedPersistedUnitName },
                    { "selectedPersistedUnitId",
                        _selectedPersistedUnitId },
                    { "persistedDollBefore", _persistedDollBefore == null
                        ? JValue.CreateNull() :
                        (JToken)_persistedDollBefore.Describe() },
                    { "persistedSerializedClassClothesAbsent",
                        _persistedSerializedClassClothesAbsent },
                    { "persistedForceUseClassEquipment",
                        _persistedForceUseClassEquipment },
                    { "persistedPreActivationAppearanceExact",
                        _persistedPreActivationAppearanceExact },
                    { "persistedViewActivationExact",
                        _persistedViewActivationExact },
                    { "persistedViewActivation",
                        _persistedViewActivationEvidence },
                    { "persistedLoadedExact", _persistedLoadedExact },
                    { "persistedReconstructionExact",
                        _persistedReconstructionExact },
                    { "persistedDollUnchanged",
                        _persistedDollUnchanged },
                    { "persistedRestored", _persistedRestored },
                    { "records", _records },
                    { "respecRecords", _respecRecords },
                    { "persistedPromoted", _persistedPromoted },
                    { "persistedRemoved", _persistedRemoved },
                    { "baselineAbsentExact", _baselineAbsentExact },
                    { "saveApiCalled", _saveStarted },
                    { "expectedWorkingSaveRoutineCount",
                        _workingSaveEvidence == null ? 0 :
                            _workingSaveEvidence
                                .ExpectedWorkingSaveRoutineCount },
                    { "productionBlueprintMutated", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-production-persistence-index.json");
                WriteJsonAtomic(path, index);
                if (!_evidenceFiles.Contains(path,
                        StringComparer.OrdinalIgnoreCase))
                    _evidenceFiles.Add(path);
                _indexWritten = true;
            }

            private void Finish(bool cleaned)
            {
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
                        _diagnostics.Add("indexWriteException=" +
                            exception);
                    }
                }
                if (_prepare)
                {
                    FinishPrepare(cleaned);
                    return;
                }
                if (_verifyAbsent)
                {
                    FinishVerifyAbsent(cleaned);
                    return;
                }
                JObject[] respec = _respecRecords.OfType<JObject>()
                    .ToArray();
                bool respecExact = respec.Length == 2 &&
                    respec.All(RespecRecordExact);
                bool capturesExact = _normalPathComplete &&
                    _records.Count == 4 && _captured == 4 &&
                    _imageCount == 8 && _viewCount == 20 &&
                    _indexWritten && _evidenceFiles.Count == 13 &&
                    _evidenceFiles.All(File.Exists) &&
                    _records.OfType<JObject>().All(value =>
                        (bool)value["appearanceExact"] &&
                        (int)value["preview"]["meaningfulPixels"] > 0 &&
                        (int)value["isometric"]["meaningfulPixels"] > 0);

                Add(_assertions,
                    "gunslinger-outfit-production-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionPersistence,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitProductionPersistence,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-save-boundary",
                    "fresh exact KMG_AUTOMATION_WORKING load followed by one exact cleanup SaveRoutine",
                    SaveEvidenceDetail(),
                    string.Equals(SaveName(),
                        "KMG_AUTOMATION_WORKING",
                        StringComparison.Ordinal) &&
                        ExactWorkingSaveEvidence(true),
                    "armed exact captured SaveInfo reference; baseline excluded");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-game-identity",
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
                Add(_assertions,
                    "gunslinger-outfit-production-persisted-unit",
                    "live loaded Gunslinger with canonical DollData and no serialized class-clothing IDs",
                    "discovered=" + _persistedDiscovered +
                        ";serializedClassClothesAbsent=" +
                        _persistedSerializedClassClothesAbsent,
                    _persistedDiscovered &&
                        _persistedSerializedClassClothesAbsent &&
                        _persistedForceUseClassEquipment &&
                        string.Equals(_selectedPersistedUnitId,
                            PersistedOutfitFixtureUniqueId,
                            StringComparison.Ordinal),
                    "exact marker-bound KMG_AUTOMATION_WORKING party descriptor after guarded load");
                Add(_assertions,
                    "gunslinger-outfit-production-loaded-appearance",
                    "production pair, persisted palette, humanoid rig, and no historical Fighter residue",
                    "preActivationClassAppearanceExact=" +
                        _persistedPreActivationAppearanceExact +
                        ";activationExact=" +
                        _persistedViewActivationExact +
                        ";loadedExact=" + _persistedLoadedExact,
                    _persistedLoadedExact,
                    "deserialized class equipment and ramp indices observed before native view activation, then captured after native visibility settlement");
                Add(_assertions,
                    "gunslinger-outfit-production-persisted-reconstruction",
                    "exact native class-equipment reconstruction with unchanged DollData and exact restoration",
                    "reconstructionExact=" +
                        _persistedReconstructionExact +
                        ";dollUnchanged=" +
                        _persistedDollUnchanged +
                        ";restored=" + _persistedRestored,
                    _persistedReconstructionExact &&
                        _persistedDollUnchanged &&
                        _persistedRestored,
                    "UnitEntityView.UpdateClassEquipment, Character.RebuildOutfit, and saved:false exact snapshot restoration");
                Add(_assertions,
                    "gunslinger-outfit-production-native-respec",
                    "male and female Human Fighter-to-Gunslinger native Respec commits reconstruct the production pair at default 2/22 with no Fighter residue",
                    "records=" + respec.Length +
                        ";exact=" + respecExact,
                    respecExact &&
                        _respecSerializedClassClothesAbsent,
                    "distinct Fighter source and level-0 replacement through StartWithoutAssigningStaticInstance Respec, Commit, UpdateClassEquipment, and RebuildOutfit");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-captures",
                    "4 sidecars, 8 PNGs, and 20 labelled views",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "four-view character previews plus elevated ordinary isometric captures");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-cleanup",
                    "fixture removed; exact three-character party/global/remote/cross-scene/inventory/money baseline cleanup-saved",
                    "cleaned=" + cleaned + ";starterRollback=" +
                        _starterRollbackExact + ";removed=" +
                        _persistedRemoved + ";updates=" +
                        _settleUpdates,
                    cleaned && _persistedRemoved,
                    "marker-only native scene removal and exact cleanup save");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add(
                    "Direct inspection of every generated image is required before final persistence acceptance.");
                _warnings.Add(
                    "DollData stores customization entities and clothes palette indices; UnitEntityView.UpdateClassEquipment reconstructs current class clothes.");
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
                    AutomaticExitRequested =
                        _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Result.WorkingSaveSmoke = _workingSaveEvidence;
                Complete = true;
            }

            private void FinishPrepare(bool prepared)
            {
                JObject[] respec = _respecRecords.OfType<JObject>()
                    .ToArray();
                bool respecExact = respec.Length == 1 &&
                    respec.All(RespecRecordExact);
                bool capturesExact = _normalPathComplete &&
                    _records.Count == 1 && _captured == 1 &&
                    _imageCount == 2 && _viewCount == 5 &&
                    _indexWritten && _evidenceFiles.Count == 4 &&
                    _evidenceFiles.All(File.Exists);
                bool fixtureExact = prepared && _persistedPromoted &&
                    IsPersistedOutfitFixture(_persistedUnit) &&
                    _persistedSerializedClassClothesAbsent &&
                    _gunslingerClass.StartingGold ==
                        _startingGoldBefore &&
                    _persistedUnit.Descriptor.Progression.GetClassLevel(
                        _gunslingerClass) == 1 &&
                    _player.PartyCharacters.Count(value => string.Equals(
                        value.UniqueId, PersistedOutfitFixtureUniqueId,
                        StringComparison.Ordinal)) == 1;

                Add(_assertions,
                    "gunslinger-outfit-production-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionPersistencePrepare,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitProductionPersistencePrepare,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-save-boundary",
                    "one exact KMG_AUTOMATION_WORKING SaveRoutine",
                    SaveEvidenceDetail(), ExactWorkingSaveEvidence(true),
                    "armed exact captured SaveInfo reference; baseline excluded");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-game-identity",
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
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-prepared-fixture",
                    "one exact marker-bound level-1 Gunslinger in party and loaded area",
                    "prepared=" + prepared + ";promoted=" +
                        _persistedPromoted + ";id=" +
                        _selectedPersistedUnitId,
                    fixtureExact,
                    "native party reference plus area-owned serializable unit");
                Add(_assertions,
                    "gunslinger-outfit-production-native-respec",
                    "one male Human Fighter-to-Gunslinger native Respec commit at default 2/22",
                    "records=" + respec.Length + ";exact=" +
                        respecExact,
                    respecExact &&
                        _respecSerializedClassClothesAbsent,
                    "distinct Fighter source and level-0 replacement through StartWithoutAssigningStaticInstance Respec, Commit, UpdateClassEquipment, and RebuildOutfit");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-captures",
                    "1 sidecar, 2 PNGs, and 5 labelled views before save",
                    "captured=" + _captured + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    capturesExact,
                    "pre-save four-view preview plus ordinary isometric capture");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");
                _warnings.Add(
                    "The prepared fixture is intentionally retained only until the mandatory fresh-load verify-cleanup phase.");
                _warnings.Add(
                    "Direct inspection of every generated image is required before final persistence acceptance.");
                CompletePhaseResult();
            }

            private void FinishVerifyAbsent(bool cleaned)
            {
                bool exact = cleaned && _normalPathComplete &&
                    _baselineAbsentExact && _records.Count == 0 &&
                    _respecRecords.Count == 0 && _captured == 0 &&
                    _imageCount == 0 && _viewCount == 0 &&
                    _indexWritten && _evidenceFiles.Count == 1 &&
                    _evidenceFiles.All(File.Exists);
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionPersistenceVerifyAbsent,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitProductionPersistenceVerifyAbsent,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-save-boundary",
                    "fresh exact KMG_AUTOMATION_WORKING load; zero writes",
                    SaveEvidenceDetail(), ExactWorkingSaveEvidence(false),
                    "fresh receiver-correlated load with passive save sentinel");
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-game-identity",
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
                Add(_assertions,
                    "gunslinger-outfit-production-persistence-absence",
                    "three-character baseline and zero fixture identities in party/global/remote/cross-scene state",
                    "baselineAbsentExact=" + _baselineAbsentExact +
                        ";cleaned=" + cleaned,
                    exact,
                    "fresh-load post-cleanup serialization boundary");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");
                CompletePhaseResult();
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

            private void CompletePhaseResult()
            {
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
                    AutomaticExitRequested =
                        _request.ExitAfterCompletion,
                    WorkingSaveSmoke = _workingSaveEvidence,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }

            private string SaveName()
            {
                return _request.Parameters == null ? string.Empty :
                    _request.Parameters.Value<string>("saveName") ??
                        string.Empty;
            }
        }
    }
}
