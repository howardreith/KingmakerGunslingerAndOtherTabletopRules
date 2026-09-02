using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-gated production outfit compatibility on disposable Human
    /// character-creation dolls. Every item, avatar, camera, and blueprint
    /// clone is request-local; the scenario never calls a save API.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string OutfitProductionClassGuid =
            "abca4797366d4df0831a418eee39069a";
        private const string OutfitLightArmorItemGuid =
            "afbe88d27a0eb544583e00fa78ffb2c7";
        private const string OutfitHeavyArmorItemGuid =
            "559b0b6f194656c428c403a000ceee78";
        private const string OutfitTricornItemGuid =
            "f33dadeeb51cdba45b23bb40a40e5fb3";
        private const string OutfitCloakItemGuid =
            "04dff7841c5f499478c91487d9bbdcef";
        private const string OutfitMaleBackpackEntityGuid =
            "431d16d2153d1854280b97470223eea6";
        private const string OutfitFemaleBackpackEntityGuid =
            "49641981096de8b43b198e95c7193b65";

        private static readonly ProductionCompatibilityCase[]
            ProductionCompatibilityCases =
        {
            new ProductionCompatibilityCase("default-no-weapon",
                "baseline"),
            new ProductionCompatibilityCase("alternate-color-no-weapon",
                "alternate-color"),
            new ProductionCompatibilityCase("pistol-held", "pistol-held"),
            new ProductionCompatibilityCase("musket-held", "musket-held"),
            new ProductionCompatibilityCase("musket-stored-inactive",
                "musket-stored"),
            new ProductionCompatibilityCase("blunderbuss-held",
                "blunderbuss-held"),
            new ProductionCompatibilityCase("light-armor-equipped",
                "light-armor"),
            new ProductionCompatibilityCase("light-armor-removed-rebuild",
                "baseline"),
            new ProductionCompatibilityCase("heavy-armor-equipped",
                "heavy-armor"),
            new ProductionCompatibilityCase("heavy-armor-removed-rebuild",
                "baseline"),
            new ProductionCompatibilityCase("tricorn-equipped",
                "tricorn"),
            new ProductionCompatibilityCase("tricorn-removed-hair-restored",
                "baseline"),
            new ProductionCompatibilityCase("cloak-equipped", "cloak"),
            new ProductionCompatibilityCase("cloak-removed-rebuild",
                "baseline"),
            new ProductionCompatibilityCase("backpack-visible",
                "backpack"),
            new ProductionCompatibilityCase("backpack-removed-final-rebuild",
                "baseline")
        };

        internal static ProductionCompatibilitySession
            BeginProductionCompatibility(ModContext context,
                RuntimeTestRequest request)
        {
            return new ProductionCompatibilitySession(context, request);
        }

        private sealed class ProductionCompatibilityCase
        {
            internal ProductionCompatibilityCase(string label, string kind)
            {
                Label = label;
                Kind = kind;
            }

            internal readonly string Label;
            internal readonly string Kind;
        }

        private sealed class ProductionCompatibilityFixture
        {
            internal ProductionCompatibilityFixture(Gender gender,
                BlueprintRace race, BlueprintRaceVisualPreset preset,
                BlueprintUnit source, string label = null)
            {
                Gender = gender;
                Race = race;
                Preset = preset;
                Source = source;
                Label = string.IsNullOrWhiteSpace(label)
                    ? gender.ToString().ToLowerInvariant() + "-human"
                    : label;
            }

            internal readonly string Label;
            internal readonly Gender Gender;
            internal readonly BlueprintRace Race;
            internal readonly BlueprintRaceVisualPreset Preset;
            internal readonly BlueprintUnit Source;
        }

        private sealed class ProductionRaceFixtureSpec
        {
            internal ProductionRaceFixtureSpec(string label,
                BlueprintRace race)
            {
                Label = label;
                Race = race;
            }

            internal readonly string Label;
            internal readonly BlueprintRace Race;
        }

        internal sealed partial class ProductionCompatibilitySession
        {
            private const int MaximumSettleUpdates = 360;
            private const int MinimumSettleUpdates = 30;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _evidenceFiles = new List<string>();
            private readonly JArray _raceLinkRecords = new JArray();
            private readonly JArray _fixtureRecords = new JArray();
            private readonly JArray _records = new JArray();
            private readonly JArray _restorationRecords = new JArray();
            private object _allUnits;
            private object _party;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private BlueprintRace[] _supportedRaces = new BlueprintRace[0];
            private ProductionCompatibilityFixture[] _fixtures =
                new ProductionCompatibilityFixture[0];
            private UnitEntityData _anchor;
            private UnitEntityData _actor;
            private BlueprintUnit _actorBlueprint;
            private DollState _dollState;
            private DollData _dollData;
            private Character _dollTemplateAvatar;
            private Character _avatar;
            private BlueprintCharacterClass _gunslingerClass;
            private EquipmentEntity[] _productionEntities =
                new EquipmentEntity[0];
            private EquipmentEntity[] _dollEntities =
                new EquipmentEntity[0];
            private EquipmentEntity _hairEntity;
            private EquipmentEntity _backpackEntity;
            private EquipmentEntity[] _expectedOverlayEntities =
                new EquipmentEntity[0];
            private string[] _expectedOverlayAssetIds = new string[0];
            private AvatarEntityState[] _avatarBefore =
                new AvatarEntityState[0];
            private AvatarEntityState[] _baseSnapshot =
                new AvatarEntityState[0];
            private string[] _savedLinksBefore = new string[0];
            private string[] _baseSavedLinks = new string[0];
            private EquipmentEntityLink[] _maleLinksBefore;
            private EquipmentEntityLink[] _femaleLinksBefore;
            private KingmakerEquipmentEntity[] _sharedLinksBefore;
            private int _primaryBefore;
            private int _secondaryBefore;
            private FieldInfo _showBackpackField;
            private ItemEntityWeapon _weaponItem;
            private ItemEntityArmor _armorItem;
            private ItemEntity _headItem;
            private ItemEntity _cloakItem;
            private bool _firearmStateSet;
            private bool _fixtureInitialized;
            private bool _dollAttachmentRecorded;
            private bool _dollCreationResourceGatePassed;
            private bool _resourcePreloadingAtDollCreation;
            private bool _productionApplied;
            private bool _previousStateCleared;
            private bool _expectHeldWeapon;
            private bool _expectStoredWeapon;
            private bool _expectBackpack;
            private string _hairAssetId = string.Empty;
            private string _currentItemGuid = string.Empty;
            private string _currentItemName = string.Empty;
            private string _currentWeaponKind = "none";
            private int _fixtureIndex;
            private int _caseIndex;
            private int _phase;
            private int _settleUpdates;
            private int _dollResourceWaitUpdates;
            private int _captured;
            private int _imageCount;
            private int _viewCount;
            private int _restorations;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private string _gameAssemblySha256 = string.Empty;
            private string _gameAssemblyMvid = string.Empty;
            private string _stage = "resolve-working-save-anchor";
            private string _exceptionSummary = string.Empty;

            private bool IsElementalRaceClassEquipment
            {
                get
                {
                    return string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .ElementalRaceClassEquipment,
                        StringComparison.Ordinal);
                }
            }

            private bool UsesElementalRaceFixtures
            {
                get
                {
                    return IsElementalRaceClassEquipment ||
                        IsElementalRaceMotion;
                }
            }

            internal ProductionCompatibilitySession(ModContext context,
                RuntimeTestRequest request)
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
                    if (_cleanupStarted)
                    {
                        PollCleanup();
                        return;
                    }
                    if (IsProductionMotion)
                    {
                        PollProductionMotion();
                        return;
                    }
                    if (_phase == 0)
                    {
                        Initialize();
                        _phase = 1;
                        return;
                    }
                    if (_phase == 1)
                    {
                        if (!PollProductionDollCreationReadiness()) return;
                        SpawnFixture();
                        _phase = 2;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 2)
                    {
                        PollFixtureReadiness();
                        return;
                    }
                    if (_phase == 3)
                    {
                        PrepareCase();
                        _phase = 4;
                        _settleUpdates = 0;
                        return;
                    }
                    PollCaseAndCapture();
                }
                catch (Exception exception)
                {
                    _exceptionSummary = exception.ToString();
                    Add(_assertions,
                        IsProductionMotion
                            ? IsElementalRaceMotion
                                ? "elemental-race-motion-exception"
                                : "gunslinger-outfit-production-motion-exception"
                            : IsElementalRaceClassEquipment
                                ? "elemental-race-class-equipment-exception"
                                : "gunslinger-outfit-production-compatibility-exception",
                        "no exception", "stage=" + _stage + ";" + exception,
                        false, IsProductionMotion
                            ? "guarded request-local native-motion outfit fixture"
                            : "guarded request-local outfit fixture");
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
                if (_request.Parameters == null || !string.Equals(
                        _request.Parameters.Value<string>("saveName"),
                        "KMG_AUTOMATION_WORKING", StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Production compatibility requires the exact disposable working save.");
                _allUnits = Game.Instance.State.Units.All;
                _party = Game.Instance.Player.Party;
                _unitsBefore = Snapshot(_allUnits);
                _partyBefore = Snapshot(_party);
                _anchor = _partyBefore.OfType<UnitEntityData>().FirstOrDefault(
                    value => value != null && value.HoldingState != null &&
                        value.View != null);
                if (_anchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party-area anchor.");

                _gunslingerClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        OutfitProductionClassGuid,
                        "gunslinger-outfit-production-class");
                BlueprintRoot root = BlueprintRoot.Instance;
                if (root == null || root.Progression == null ||
                    root.Progression.CharacterRaces == null ||
                    root.Progression.CharacterClasses == null ||
                    !root.Progression.CharacterClasses.Any(value =>
                        ReferenceEquals(value, _gunslingerClass)))
                    throw new InvalidOperationException(
                        "The exact production Gunslinger is not published in the installed class catalog.");
                _supportedRaces = UsesElementalRaceFixtures
                    ? RequireElementalRaces()
                    : root.Progression.CharacterRaces
                        .Where(value => value != null)
                        .GroupBy(value => value.RaceId)
                        .Select(group => group.OrderBy(value =>
                            value.AssetGuid, StringComparer.Ordinal).First())
                        .OrderBy(value => value.RaceId.ToString(),
                            StringComparer.Ordinal).ToArray();
                if (_supportedRaces.Length == 0)
                    throw new InvalidOperationException(
                        "The installed player-race catalog is empty.");

                _maleLinksBefore = _gunslingerClass.MaleEquipmentEntities;
                _femaleLinksBefore = _gunslingerClass.FemaleEquipmentEntities;
                _sharedLinksBefore = _gunslingerClass.EquipmentEntities;
                _primaryBefore = _gunslingerClass.PrimaryColor;
                _secondaryBefore = _gunslingerClass.SecondaryColor;
                ValidateProductionRaceLinks();
                _fixtures = UsesElementalRaceFixtures
                    ? BuildElementalFixtures()
                    : BuildHumanFixtures();
                _showBackpackField = typeof(Character).GetField(
                    "m_ShowBackpack", BindingFlags.Instance |
                        BindingFlags.NonPublic);
                if (_showBackpackField == null ||
                    _showBackpackField.FieldType != typeof(bool))
                    throw new MissingFieldException(typeof(Character).FullName,
                        "m_ShowBackpack");
                Assembly gameAssembly = typeof(BlueprintCharacterClass)
                    .Assembly;
                _gameAssemblySha256 = HashFile(gameAssembly.Location)
                    .ToLowerInvariant();
                _gameAssemblyMvid = gameAssembly.ManifestModule
                    .ModuleVersionId.ToString("D");
                if (ProductionCompatibilityCases.Length != 16 ||
                    ProductionCompatibilityCases.Select(value => value.Label)
                        .Distinct(StringComparer.Ordinal).Count() != 16)
                    throw new InvalidOperationException(
                        "The production compatibility matrix must contain sixteen unique states.");
                WriteProgress("initialized");
            }

            private void ValidateProductionRaceLinks()
            {
                string[] maleIds =
                    GunslingerClassAppearanceCatalog.MaleAssetIds();
                string[] femaleIds =
                    GunslingerClassAppearanceCatalog.FemaleAssetIds();
                if (_maleLinksBefore == null || _femaleLinksBefore == null ||
                    _sharedLinksBefore == null ||
                    _maleLinksBefore.Length != maleIds.Length ||
                    _femaleLinksBefore.Length != femaleIds.Length ||
                    _sharedLinksBefore.Length != 0 ||
                    _primaryBefore != GunslingerClassAppearanceCatalog
                        .DefaultPrimaryColor ||
                    _secondaryBefore != GunslingerClassAppearanceCatalog
                        .DefaultSecondaryColor)
                    throw new InvalidOperationException(
                        "The registered Gunslinger appearance does not match the production catalog.");
                foreach (BlueprintRace race in _supportedRaces)
                    foreach (Gender gender in new[]
                    {
                        Gender.Male, Gender.Female
                    })
                    {
                        string[] expectedIds = gender == Gender.Male
                            ? maleIds : femaleIds;
                        EquipmentEntity[] observed = _gunslingerClass
                            .LoadClothes(gender, race)
                            .Where(value => value != null).ToArray();
                        EquipmentEntity[] expected = expectedIds.Select(id =>
                        {
                            EquipmentEntity entity = ResourcesLibrary
                                .TryGetResource<EquipmentEntity>(id, true);
                            if (entity == null)
                                throw new InvalidOperationException(
                                    "Production outfit entity did not resolve: " +
                                    id + ".");
                            return entity;
                        }).ToArray();
                        bool exact = observed.Length == expectedIds.Length &&
                            observed.Select((value, index) =>
                                ReferenceEquals(value, expected[index]))
                                .All(value => value);
                        _raceLinkRecords.Add(new JObject
                        {
                            { "raceName", race.name },
                            { "raceGuid", race.AssetGuid },
                            { "raceId", race.RaceId.ToString() },
                            { "gender", gender.ToString() },
                            { "expectedAssetIds", new JArray(expectedIds) },
                            { "observedEntityNames", new JArray(
                                observed.Select(value =>
                                    value.name).ToArray()) },
                            { "orderedPairExact", exact }
                        });
                        if (!exact)
                            throw new InvalidOperationException(gender + " " +
                                race.RaceId + " did not load the exact ordered production outfit pair.");
                    }
            }

            private static BlueprintRace[] RequireElementalRaces()
            {
                ElementalRaceBlueprintSet set =
                    BlueprintBootstrap.ElementalRaces;
                BlueprintRace[] races = set == null
                    ? new BlueprintRace[0]
                    : set.OrderedBlueprints().Select(value =>
                        value == null ? null : value.Race).ToArray();
                if (races.Length != ElementalRaceCatalog.RaceCount ||
                    races.Any(value => value == null) ||
                    races.Select(value => value.AssetGuid).Distinct(
                        StringComparer.Ordinal).Count() != races.Length)
                    throw new InvalidOperationException(
                        "The four production elemental race identities are unavailable or ambiguous.");
                return races;
            }

            private ProductionCompatibilityFixture[] BuildHumanFixtures()
            {
                BlueprintRace human = _supportedRaces.Single(value =>
                    value.RaceId == Race.Human);
                return BuildProductionFixtures(new[]
                {
                    new ProductionRaceFixtureSpec("human", human)
                });
            }

            private ProductionCompatibilityFixture[] BuildElementalFixtures()
            {
                ElementalRaceBlueprints[] races = BlueprintBootstrap
                    .ElementalRaces.OrderedBlueprints().ToArray();
                return BuildProductionFixtures(races.Select(value =>
                    new ProductionRaceFixtureSpec(value.Definition.Kind
                        .ToString().ToLowerInvariant(), value.Race)));
            }

            private ProductionCompatibilityFixture[] BuildProductionFixtures(
                IEnumerable<ProductionRaceFixtureSpec> raceSpecs)
            {
                BlueprintUnit[] donors = ResourcesLibrary
                    .GetBlueprints<BlueprintUnit>()
                    .Where(value => value != null && value.Prefab != null &&
                        value.Race != null && value.Race.RaceId == Race.Human &&
                        value.Size == Size.Medium && value.Body != null &&
                        !value.Body.DisableHands)
                    .OrderBy(value => ProductionDonorPriority(value))
                    .ThenBy(value => value.name ?? string.Empty,
                        StringComparer.Ordinal)
                    .ThenBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
                var fixtures = new List<ProductionCompatibilityFixture>();
                foreach (ProductionRaceFixtureSpec raceSpec in raceSpecs)
                foreach (Gender gender in new[]
                {
                    Gender.Male, Gender.Female
                })
                {
                    BlueprintUnit source = donors.FirstOrDefault(value =>
                        value.Gender == gender);
                    if (source == null)
                        throw new InvalidOperationException(
                            "No exact native Human body donor exists for " + gender + ".");
                    BlueprintRaceVisualPreset preset = raceSpec.Race.Presets
                        .Where(value => value != null && value.Skin != null &&
                            (gender == Gender.Female
                                ? value.FemaleSkeleton != null
                                : value.MaleSkeleton != null))
                        .OrderBy(value => value.AssetGuid,
                            StringComparer.Ordinal).FirstOrDefault();
                    if (preset == null)
                        throw new InvalidOperationException(
                            "No complete production visual preset exists for " +
                            raceSpec.Label + "/" + gender + ".");
                    fixtures.Add(new ProductionCompatibilityFixture(gender,
                        raceSpec.Race, preset, source,
                        gender.ToString().ToLowerInvariant() + "-" +
                            raceSpec.Label));
                }
                return fixtures.ToArray();
            }

            private static int ProductionDonorPriority(BlueprintUnit value)
            {
                string name = value == null ? string.Empty :
                    value.name ?? string.Empty;
                if (name.StartsWith("StartGamePregen",
                        StringComparison.OrdinalIgnoreCase)) return 0;
                if (name.IndexOf("Companion",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 1;
                if (name.IndexOf("Human",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 2;
                if (name.IndexOf("Player",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 3;
                return 4;
            }

            private bool PollProductionDollCreationReadiness()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "wait-production-doll-resources-" + fixture.Label;
                if (ResourcesLibrary.Preloading)
                {
                    _dollResourceWaitUpdates++;
                    if (_dollResourceWaitUpdates < MaximumSettleUpdates)
                        return false;
                    throw new InvalidOperationException(fixture.Label +
                        " resource preloading did not finish before native " +
                        "DollData creation; updates=" +
                        _dollResourceWaitUpdates + ".");
                }

                _dollCreationResourceGatePassed = true;
                _diagnostics.Add((IsProductionMotion
                        ? "productionMotionDollCreationReadiness="
                        : "productionDollCreationReadiness=") +
                    fixture.Label + ";waitUpdates=" +
                    _dollResourceWaitUpdates + ";preloading=False");
                return true;
            }

            private void SpawnFixture()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "spawn-production-" + fixture.Label;
                _resourcePreloadingAtDollCreation =
                    ResourcesLibrary.Preloading;
                if (!_dollCreationResourceGatePassed ||
                    _resourcePreloadingAtDollCreation)
                    throw new InvalidOperationException(fixture.Label +
                        " crossed native DollData creation without a stable " +
                        "resource-readiness boundary.");
                _actorBlueprint = UnityEngine.Object.Instantiate(
                    fixture.Source);
                if (IsProductionMotion)
                    PrepareProductionMotionActorBlueprint(_actorBlueprint);
                _actorBlueprint.Race = fixture.Race;
                _actorBlueprint.Gender = fixture.Gender;
                _actorBlueprint.Body = CreateProductionNeutralBody(
                    fixture.Source);
                _actorBlueprint.StartingInventory =
                    Array.Empty<BlueprintItem>();
                _actorBlueprint.name =
                    "KMG_Runtime_Gunslinger_Outfit_Production_" +
                    fixture.Label.Replace('-', '_');
                _actorBlueprint.IsCheater = true;

                _dollState = new DollState();
                _dollState.SetGender(fixture.Gender);
                _dollState.SetRace(fixture.Race);
                _dollState.SetRacePreset(fixture.Preset);
                _dollState.SetClass(_gunslingerClass);
                EquipmentEntityLink[] hairLinks =
                    GetProductionHairLinks(_dollState)
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId))
                    .OrderBy(value => value.AssetId,
                        StringComparer.Ordinal).ToArray();
                if (hairLinks.Length == 0)
                    throw new InvalidOperationException(fixture.Label +
                        " exposes no deterministic native hairstyle.");
                _dollState.SetHair(hairLinks[0]);
                _hairAssetId = hairLinks[0].AssetId;
                if (_dollState.GetSkinRamps().Count > 0)
                    _dollState.SetSkinColor(0);
                if (_dollState.GetHairRamps().Count > 0)
                    _dollState.SetHairColor(0);
                _dollData = _dollState.CreateData();
                if (_dollData == null ||
                    !ReferenceEquals(_dollState.CharacterClass,
                        _gunslingerClass) ||
                    !ReferenceEquals(_dollData.RacePreset,
                        fixture.Preset) ||
                    _dollData.Gender != fixture.Gender ||
                    _dollData.EquipmentEntityIds == null ||
                    !_dollData.EquipmentEntityIds.Contains(_hairAssetId))
                    throw new InvalidOperationException(fixture.Label +
                        " did not produce an exact Gunslinger DollData hairstyle contract.");

                UnitEntityView dollView = null;
                try
                {
                    dollView = _dollData.CreateUnitView(false);
                    _dollTemplateAvatar = dollView == null ? null :
                        dollView.GetComponent<Character>();
                    if (dollView == null || _dollTemplateAvatar == null)
                        throw new InvalidOperationException(fixture.Label +
                            " DollData did not create a native Character view.");
                    _dollAttachmentRecorded = false;
                    if (IsProductionMotion)
                        _diagnostics.Add(
                            "productionMotionDollBeforeAttach=" +
                            DescribeProductionDollLifecycle(
                                _dollTemplateAvatar));
                    dollView.Blueprint = _actorBlueprint;
                    dollView.UniqueId = Guid.NewGuid().ToString();
                    dollView.transform.position = NearestNavigable(
                        _anchor.Position + new Vector3(-3.5f, 0f, 3.5f));
                    dollView.transform.rotation = Quaternion.identity;
                    SceneEntitiesState holdingState = IsProductionMotion
                        ? ProductionMotionHoldingState()
                        : _anchor.HoldingState;
                    _actor = Game.Instance.EntityCreator
                        .SpawnEntityWithView(dollView,
                            holdingState) as UnitEntityData;
                    if (_actor == null ||
                        !ReferenceEquals(_actor.View, dollView))
                        throw new InvalidOperationException(fixture.Label +
                            " DollData view ownership transfer failed.");
                    if (IsProductionMotion)
                        _diagnostics.Add(
                            "productionMotionDollAfterSpawnBeforeTick=" +
                            DescribeProductionDollLifecycle(
                                _dollTemplateAvatar) + ";viewAvatar=" +
                            DescribeProductionDollLifecycle(
                                _actor.View.CharacterAvatar));
                    dollView = null;
                }
                finally
                {
                    if (dollView != null)
                        UnityEngine.Object.DestroyImmediate(
                            dollView.gameObject);
                }
                if (_actor.Descriptor != null)
                    _actor.Descriptor.Doll = _dollData;
                _fixtureInitialized = false;
                _productionApplied = false;
                WriteProgress("spawned");
            }

            private static EquipmentEntityLink[] GetProductionHairLinks(
                DollState state)
            {
                MethodInfo method = typeof(DollState).GetMethod(
                    "GetHairEntities", BindingFlags.Instance |
                        BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (method == null || method.ReturnType !=
                        typeof(List<EquipmentEntityLink>))
                    throw new MissingMethodException(
                        typeof(DollState).FullName,
                        "GetHairEntities() : List<EquipmentEntityLink>");
                var result = method.Invoke(state, null) as
                    List<EquipmentEntityLink>;
                if (result == null)
                    throw new InvalidOperationException(
                        "DollState.GetHairEntities returned no native list.");
                return result.ToArray();
            }

            private static BlueprintUnit.UnitBody CreateProductionNeutralBody(
                BlueprintUnit source)
            {
                if (source == null || source.Body == null)
                    throw new InvalidOperationException(
                        "A production compatibility donor has no body contract.");
                return new BlueprintUnit.UnitBody
                {
                    DisableHands = false,
                    EmptyHandWeapon = source.Body.EmptyHandWeapon,
                    AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(),
                    AdditionalSecondaryLimbs =
                        Array.Empty<BlueprintItemWeapon>(),
                    QuickSlots = Array.Empty<
                        BlueprintItemEquipmentUsable>()
                };
            }

            private void PollFixtureReadiness()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "settle-production-" + fixture.Label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                bool complete = _actor != null && _actor.View != null &&
                    _actor.View.Data != null &&
                    _actor.View.HandsEquipment != null &&
                    _actor.View.CharacterAvatar != null &&
                    _actor.Descriptor != null &&
                    _actor.Descriptor.Progression != null &&
                    _actor.Descriptor.Progression.Race != null;
                if (!complete)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(fixture.Label +
                        " did not materialize a complete native view.");
                }

                if (IsProductionMotion && !_dollAttachmentRecorded)
                {
                    _avatar = _actor.View.CharacterAvatar;
                    _diagnostics.Add("productionMotionDollAfterAttach=" +
                        DescribeProductionDollLifecycle(_avatar) +
                        ";templateSame=" + ReferenceEquals(
                            _dollTemplateAvatar, _avatar));
                    _dollAttachmentRecorded = true;
                }

                if (!_fixtureInitialized)
                {
                    _actor.Descriptor.State.Immortality.Retain();
                    _actor.Commands.InterruptAll(true);
                    if (_actor.CombatState.IsInCombat)
                        _actor.CombatState.LeaveCombat();
                    int removed = ClearProductionBodyEquipment(_actor);
                    if (removed != 0)
                        throw new InvalidOperationException(fixture.Label +
                            " neutral body unexpectedly created " + removed +
                            " equipped items.");
                    _actor.View.HandsEquipment.UpdateAll();
                    _actor.View.HandsEquipment.ForceSwitch(false);
                    _fixtureInitialized = true;
                    _settleUpdates = 0;
                    return;
                }

                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                _avatar = _actor.View.CharacterAvatar;
                if (!_productionApplied)
                {
                    _dollEntities = ResolveProductionDollEntities(fixture);
                    _hairEntity = ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(_hairAssetId, true);
                    if (_hairEntity == null || !_dollEntities.Any(value =>
                            ReferenceEquals(value, _hairEntity)))
                        throw new InvalidOperationException(fixture.Label +
                            " selected hairstyle did not resolve through DollData.");
                    bool nativeDollExact = ReferenceEquals(
                            _actor.Descriptor.Doll, _dollData) &&
                        _dollEntities.Length > 0 &&
                        _dollEntities.All(expected =>
                            _avatar.EquipmentEntities.Any(actual =>
                                ReferenceEquals(expected, actual)));
                    bool nativeHairPresent = _avatar.EquipmentEntities.Any(
                        value => ReferenceEquals(value, _hairEntity));
                    bool nativeNoWeapon =
                        _actor.Body.PrimaryHand.MaybeItem == null &&
                        _actor.Body.SecondaryHand.MaybeItem == null &&
                        _actor.View.HandsEquipment.GetWeaponModel(false) ==
                            null &&
                        _actor.View.HandsEquipment.GetWeaponModel(true) ==
                            null &&
                        !_actor.View.HandsEquipment.InCombat;
                    bool nativeReady = nativeDollExact &&
                        nativeHairPresent && nativeNoWeapon &&
                        HasExactHumanoidRig(_actor.View.transform) &&
                        ActiveRenderers(_actor).Length > 0;
                    if (_settleUpdates < MinimumSettleUpdates ||
                        !nativeReady)
                    {
                        if (_settleUpdates < MaximumSettleUpdates) return;
                        if (IsProductionMotion)
                            _diagnostics.Add(
                                "productionMotionDollAtSettleTimeout=" +
                                DescribeProductionDollLifecycle(_avatar) +
                                ";templateSame=" + ReferenceEquals(
                                    _dollTemplateAvatar, _avatar));
                        throw new InvalidOperationException(fixture.Label +
                            " native doll did not settle exactly before " +
                            "production application; doll=" +
                            nativeDollExact + ";hair=" +
                            nativeHairPresent + ";noWeapon=" +
                            nativeNoWeapon + ";active=" + string.Join(",",
                                _avatar.EquipmentEntities.Where(value =>
                                    value != null).Select(value =>
                                    value.name).ToArray()) + ".");
                    }
                    _avatarBefore = TakeProductionSnapshot(_avatar);
                    _savedLinksBefore = ProductionSavedLinks(_avatar);
                    _productionEntities = _gunslingerClass.LoadClothes(
                            fixture.Gender, fixture.Race)
                        .Where(value => value != null).ToArray();
                    EquipmentEntity[] missing = _productionEntities.Where(
                        value => !_avatar.EquipmentEntities.Any(current =>
                            ReferenceEquals(current, value))).ToArray();
                    _avatar.AddEquipmentEntities(missing, false);
                    SetProductionPalette(false);
                    _avatar.RebuildOutfit();
                    _avatar.UpdateBackpackVisibility(false);
                    _productionApplied = true;
                    _settleUpdates = 0;
                    _diagnostics.Add(fixture.Label +
                        ":productionEntities=" + string.Join(",",
                            CurrentProductionAssetIds()) +
                        ";added=" + missing.Length + ";hair=" +
                        _hairAssetId + ";nativeSettleUpdates=" +
                        _settleUpdates);
                    return;
                }

                bool classExact = ProductionEntitiesPresent();
                bool hairPresent = _avatar.EquipmentEntities.Any(value =>
                    ReferenceEquals(value, _hairEntity));
                bool savedLinksExact = _savedLinksBefore.SequenceEqual(
                    ProductionSavedLinks(_avatar), StringComparer.Ordinal);
                bool noWeapon = _actor.Body.PrimaryHand.MaybeItem == null &&
                    _actor.Body.SecondaryHand.MaybeItem == null &&
                    _actor.View.HandsEquipment.GetWeaponModel(false) == null &&
                    _actor.View.HandsEquipment.GetWeaponModel(true) == null &&
                    !_actor.View.HandsEquipment.InCombat;
                bool ready = _settleUpdates >= MinimumSettleUpdates &&
                    classExact && hairPresent && savedLinksExact && noWeapon &&
                    HasExactHumanoidRig(_actor.View.transform) &&
                    ActiveRenderers(_actor).Length > 0;
                if (!ready)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(fixture.Label +
                        " production class preview did not settle exactly; " +
                        "class=" + classExact + ";hair=" + hairPresent +
                        ";saved=" + savedLinksExact + ";noWeapon=" +
                        noWeapon + ".");
                }

                _baseSnapshot = TakeProductionSnapshot(_avatar);
                _baseSavedLinks = ProductionSavedLinks(_avatar);
                _fixtureRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "sourceName", fixture.Source.name },
                    { "sourceGuid", fixture.Source.AssetGuid },
                    { "gender", fixture.Gender.ToString() },
                    { "raceName", fixture.Race.name },
                    { "raceGuid", fixture.Race.AssetGuid },
                    { "raceId", fixture.Race.RaceId.ToString() },
                    { "racePresetName", fixture.Preset.name },
                    { "racePresetGuid", fixture.Preset.AssetGuid },
                    { "dollCharacterClassGuid",
                        _dollState.CharacterClass.AssetGuid },
                    { "dollEquipmentEntityIds", new JArray(
                        _dollData.EquipmentEntityIds.ToArray()) },
                    { "dollCreationResourceGatePassed",
                        _dollCreationResourceGatePassed },
                    { "resourcePreloadingAtDollCreation",
                        _resourcePreloadingAtDollCreation },
                    { "dollResourceWaitUpdates",
                        _dollResourceWaitUpdates },
                    { "selectedHairAssetId", _hairAssetId },
                    { "productionAssetIds", new JArray(
                        CurrentProductionAssetIds()) },
                    { "baseEntityCount", _baseSnapshot.Length },
                    { "savedLinksUnchanged", savedLinksExact },
                    { "rigExact", true },
                    { "activeRendererCount",
                        ActiveRenderers(_actor).Length }
                });
                _phase = 3;
                _caseIndex = 0;
                _settleUpdates = 0;
                WriteProgress("fixture-ready");
            }

            private EquipmentEntity[] ResolveProductionDollEntities(
                ProductionCompatibilityFixture fixture)
            {
                var result = new List<EquipmentEntity>();
                result.AddRange(fixture.Preset.Skin.Load(fixture.Gender,
                    fixture.Preset.RaceId).Where(value => value != null));
                foreach (string id in _dollData.EquipmentEntityIds)
                {
                    EquipmentEntity entity = ResourcesLibrary
                        .TryGetResource<EquipmentEntity>(id, true);
                    if (entity == null)
                        throw new InvalidOperationException(fixture.Label +
                            " DollData entity did not resolve: " + id + ".");
                    result.Add(entity);
                }
                return result.Distinct().ToArray();
            }

            private string DescribeProductionDollLifecycle(Character avatar)
            {
                EquipmentEntity[] active = avatar == null ||
                        avatar.EquipmentEntities == null
                    ? new EquipmentEntity[0]
                    : avatar.EquipmentEntities.Where(value => value != null)
                        .ToArray();
                int rawCount = avatar == null ||
                        avatar.EquipmentEntities == null
                    ? -1 : avatar.EquipmentEntities.Count;
                int savedCount = avatar == null ||
                        avatar.SavedEquipmentEntities == null
                    ? -1 : avatar.SavedEquipmentEntities.Count;
                int expectedCount = _dollData == null ||
                        _dollData.EquipmentEntityIds == null
                    ? -1 : _dollData.EquipmentEntityIds.Count;
                return "preloading=" + ResourcesLibrary.Preloading +
                    ";avatar=" + (avatar == null ? "<null>" :
                        avatar.GetInstanceID().ToString()) +
                    ";rawCount=" + rawCount +
                    ";activeCount=" + active.Length +
                    ";savedCount=" + savedCount +
                    ";expectedIdCount=" + expectedCount +
                    ";active=" + string.Join("|", active.Select(value =>
                        value.name ?? "<unnamed>").ToArray());
            }

            private static int ClearProductionBodyEquipment(
                UnitEntityData actor)
            {
                if (actor == null || actor.Body == null) return 0;
                var removed = new List<ItemEntity>();
                foreach (ItemSlot slot in actor.Body.AllSlots.ToArray())
                {
                    if (slot == null || slot.MaybeItem == null) continue;
                    ItemEntity item = slot.MaybeItem;
                    slot.RemoveItem(false);
                    if (!removed.Any(value => ReferenceEquals(value, item)))
                        removed.Add(item);
                }
                foreach (ItemEntity item in removed) item.Dispose();
                return removed.Count;
            }

            private static AvatarEntityState[] TakeProductionSnapshot(
                Character avatar)
            {
                return avatar.EquipmentEntities
                    .Where(value => value != null)
                    .Select(value => new AvatarEntityState
                    {
                        Entity = value,
                        Primary = avatar.GetPrimaryRampIndex(value),
                        Secondary = avatar.GetSecondaryRampIndex(value)
                    }).ToArray();
            }

            private static string[] ProductionSavedLinks(Character avatar)
            {
                return avatar.SavedEquipmentEntities.Select(value =>
                    value == null ? "<null>" : value.AssetId ?? string.Empty)
                    .ToArray();
            }

            private bool RestoreProductionSnapshot(
                AvatarEntityState[] snapshot, string[] savedLinks)
            {
                if (_avatar == null || snapshot == null ||
                    savedLinks == null) return false;
                _avatar.RemoveAllEquipmentEntities(false);
                foreach (AvatarEntityState state in snapshot)
                    _avatar.AddEquipmentEntity(state.Entity, false);
                foreach (AvatarEntityState state in snapshot)
                    if (state.Primary >= 0 && state.Secondary >= 0)
                        _avatar.SetRampIndices(state.Entity, state.Primary,
                            state.Secondary, false);
                    else if (state.Primary >= 0)
                        _avatar.SetPrimaryRampIndex(state.Entity,
                            state.Primary, false);
                    else if (state.Secondary >= 0)
                        _avatar.SetSecondaryRampIndex(state.Entity,
                            state.Secondary, false);
                _avatar.RebuildOutfit();
                return SnapshotMatches(snapshot, savedLinks);
            }

            private bool SnapshotMatches(AvatarEntityState[] snapshot,
                string[] savedLinks)
            {
                if (_avatar == null) return false;
                EquipmentEntity[] current = _avatar.EquipmentEntities
                    .Where(value => value != null).ToArray();
                bool exactOrder = current.Length == snapshot.Length &&
                    current.Select((value, index) => ReferenceEquals(value,
                        snapshot[index].Entity)).All(value => value);
                bool exactRamps = snapshot.All(value =>
                    _avatar.GetPrimaryRampIndex(value.Entity) ==
                        value.Primary &&
                    _avatar.GetSecondaryRampIndex(value.Entity) ==
                        value.Secondary);
                return exactOrder && exactRamps && savedLinks.SequenceEqual(
                    ProductionSavedLinks(_avatar), StringComparer.Ordinal);
            }

            private bool ProductionEntitiesPresent()
            {
                if (_avatar == null || _productionEntities.Length != 2)
                    return false;
                EquipmentEntity[] expected = CurrentProductionAssetIds()
                    .Select(id => ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(id, true)).ToArray();
                return expected.All(value => value != null) &&
                    _productionEntities.Select((value, index) =>
                        ReferenceEquals(value, expected[index]))
                        .All(value => value) &&
                    _productionEntities.All(expectedEntity =>
                        _avatar.EquipmentEntities.Any(actual =>
                            ReferenceEquals(actual, expectedEntity)));
            }

            private string[] CurrentProductionAssetIds()
            {
                return _fixtures[_fixtureIndex].Gender == Gender.Male
                    ? GunslingerClassAppearanceCatalog.MaleAssetIds()
                    : GunslingerClassAppearanceCatalog.FemaleAssetIds();
            }

            private void SetProductionPalette(bool alternate)
            {
                foreach (EquipmentEntity entity in _productionEntities)
                {
                    int primaryCount = entity.PrimaryRamps == null ? 0 :
                        entity.PrimaryRamps.Count;
                    int secondaryCount = entity.SecondaryRamps == null ? 0 :
                        entity.SecondaryRamps.Count;
                    if (primaryCount <= GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor ||
                        secondaryCount <= GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor)
                        throw new InvalidOperationException(entity.name +
                            " does not expose the audited production ramps.");
                    int primary = alternate
                        ? (GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor + 11) % primaryCount
                        : GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor;
                    int secondary = alternate
                        ? (GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor + 17) % secondaryCount
                        : GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor;
                    _avatar.SetRampIndices(entity, primary, secondary, false);
                }
            }

            private void PrepareCase()
            {
                ProductionCompatibilityCase value =
                    ProductionCompatibilityCases[_caseIndex];
                _stage = "prepare-" + _fixtures[_fixtureIndex].Label + "-" +
                    value.Label;
                _previousStateCleared = ResetTransientState();
                if (!_previousStateCleared)
                    throw new InvalidOperationException(value.Label +
                        " began before the prior transient state was cleared.");
                _expectedOverlayEntities = new EquipmentEntity[0];
                _expectedOverlayAssetIds = new string[0];
                _expectHeldWeapon = false;
                _expectStoredWeapon = false;
                _expectBackpack = false;
                _currentItemGuid = string.Empty;
                _currentItemName = string.Empty;
                _currentWeaponKind = "none";

                switch (value.Kind)
                {
                    case "alternate-color":
                        SetProductionPalette(true);
                        break;
                    case "pistol-held":
                        EquipProductionWeapon(BlueprintBootstrap
                            .ProductionFirearms.Pistol.Item, "pistol", true);
                        break;
                    case "musket-held":
                        EquipProductionWeapon(BlueprintBootstrap
                            .ProductionFirearms.Musket.Item, "musket", true);
                        break;
                    case "musket-stored":
                        EquipProductionWeapon(BlueprintBootstrap
                            .ProductionFirearms.Musket.Item, "musket", false);
                        break;
                    case "blunderbuss-held":
                        EquipProductionWeapon(BlueprintBootstrap
                            .ProductionFirearms.Blunderbuss.Item,
                            "blunderbuss", true);
                        break;
                    case "light-armor":
                        EquipProductionArmor(OutfitLightArmorItemGuid,
                            ArmorProficiencyGroup.Light);
                        break;
                    case "heavy-armor":
                        EquipProductionArmor(OutfitHeavyArmorItemGuid,
                            ArmorProficiencyGroup.Heavy);
                        break;
                    case "tricorn":
                        EquipProductionHeadgear();
                        break;
                    case "cloak":
                        EquipProductionCloak();
                        break;
                    case "backpack":
                        EquipProductionBackpack();
                        break;
                    case "baseline":
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Unknown production compatibility state " +
                            value.Kind + ".");
                }
                _avatar.RebuildOutfit();
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(_expectHeldWeapon);
                if (_expectBackpack)
                    _avatar.UpdateBackpackVisibility(true);
                WriteProgress("case-prepared");
            }

            private bool ResetTransientState()
            {
                if (_actor == null || _avatar == null) return false;
                _actor.View.HandsEquipment.ForceSwitch(false);
                RemoveProductionWeapon();
                RemoveProductionItem(_actor.Body.Armor, ref _armorItem);
                RemoveProductionItem(_actor.Body.Head, ref _headItem);
                RemoveProductionItem(_actor.Body.Shoulders, ref _cloakItem);
                if (_backpackEntity != null)
                    _avatar.RemoveEquipmentEntity(_backpackEntity, false);
                _backpackEntity = null;
                _avatar.UpdateBackpackVisibility(false);
                bool restored = RestoreProductionSnapshot(_baseSnapshot,
                    _baseSavedLinks);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                return restored &&
                    _actor.Body.PrimaryHand.MaybeItem == null &&
                    _actor.Body.SecondaryHand.MaybeItem == null &&
                    _actor.Body.Armor.MaybeItem == null &&
                    _actor.Body.Head.MaybeItem == null &&
                    _actor.Body.Shoulders.MaybeItem == null &&
                    !ReadBackpackVisible();
            }

            private void EquipProductionWeapon(BlueprintItemWeapon blueprint,
                string kind, bool held)
            {
                if (blueprint == null)
                    throw new InvalidOperationException(kind +
                        " production blueprint is unavailable.");
                _weaponItem = new ItemEntityWeapon(blueprint);
                _actor.Body.PrimaryHand.InsertItem(_weaponItem);
                if (!ReferenceEquals(_actor.Body.PrimaryHand.MaybeWeapon,
                        _weaponItem))
                    throw new InvalidOperationException(kind +
                        " did not remain in the primary hand.");
                FirearmRuntimeState.Service.Set(_weaponItem,
                    new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                        FirearmStateTokenCatalog.DiagnosticLeadBall,
                        FirearmCondition.Normal));
                _firearmStateSet = true;
                _currentItemGuid = blueprint.AssetGuid;
                _currentItemName = blueprint.name;
                _currentWeaponKind = kind;
                _expectHeldWeapon = held;
                _expectStoredWeapon = !held;
            }

            private void EquipProductionArmor(string guid,
                ArmorProficiencyGroup expectedGroup)
            {
                BlueprintItemArmor blueprint = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemArmor>(
                        BlueprintBootstrap.Library, guid,
                        "gunslinger-outfit-production-" + expectedGroup +
                        "-armor");
                if (blueprint.Type == null || !blueprint.Type.IsArmor ||
                    blueprint.Type.ProficiencyGroup != expectedGroup)
                    throw new InvalidOperationException(guid +
                        " is not exact " + expectedGroup + " armor.");
                _expectedOverlayEntities = LoadProductionItemEntities(
                    blueprint);
                _armorItem = new ItemEntityArmor(blueprint);
                _actor.Body.Armor.InsertItem(_armorItem);
                if (!ReferenceEquals(_actor.Body.Armor.Armor, _armorItem))
                    throw new InvalidOperationException(expectedGroup +
                        " armor did not remain equipped.");
                _currentItemGuid = blueprint.AssetGuid;
                _currentItemName = blueprint.name;
            }

            private void EquipProductionHeadgear()
            {
                BlueprintItemEquipmentHead blueprint = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemEquipmentHead>(
                        BlueprintBootstrap.Library, OutfitTricornItemGuid,
                        "gunslinger-outfit-production-tricorn");
                _expectedOverlayEntities = LoadProductionItemEntities(
                    blueprint);
                _headItem = blueprint.CreateEntity();
                _actor.Body.Head.InsertItem(_headItem);
                if (!ReferenceEquals(_actor.Body.Head.MaybeItem, _headItem))
                    throw new InvalidOperationException(
                        "The audited tricorn did not remain equipped.");
                _currentItemGuid = blueprint.AssetGuid;
                _currentItemName = blueprint.name;
            }

            private void EquipProductionCloak()
            {
                BlueprintItemEquipmentShoulders blueprint =
                    BlueprintLibraryLookup.RequireExact<
                        BlueprintItemEquipmentShoulders>(
                            BlueprintBootstrap.Library,
                            OutfitCloakItemGuid,
                            "gunslinger-outfit-production-cloak");
                _expectedOverlayEntities = LoadProductionItemEntities(
                    blueprint);
                _cloakItem = blueprint.CreateEntity();
                _actor.Body.Shoulders.InsertItem(_cloakItem);
                if (!ReferenceEquals(_actor.Body.Shoulders.MaybeItem,
                        _cloakItem))
                    throw new InvalidOperationException(
                        "The audited native cloak did not remain equipped.");
                _currentItemGuid = blueprint.AssetGuid;
                _currentItemName = blueprint.name;
            }

            private void EquipProductionBackpack()
            {
                string id = _fixtures[_fixtureIndex].Gender == Gender.Male
                    ? OutfitMaleBackpackEntityGuid
                    : OutfitFemaleBackpackEntityGuid;
                _backpackEntity = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(id, true);
                if (_backpackEntity == null ||
                    !_backpackEntity.OutfitParts.Any(value => value != null &&
                        value.Special == EquipmentEntity
                            .OutfitPartSpecialType.Backpack))
                    throw new InvalidOperationException(
                        "The exact native backpack entity is unavailable or lacks a Backpack outfit part: " + id + ".");
                _expectedOverlayEntities = new[] { _backpackEntity };
                _expectedOverlayAssetIds = new[] { id };
                _avatar.AddEquipmentEntity(_backpackEntity, false);
                _avatar.UpdateBackpackVisibility(true);
                _expectBackpack = true;
                _currentItemGuid = id;
                _currentItemName = _backpackEntity.name;
            }

            private EquipmentEntity[] LoadProductionItemEntities(
                BlueprintItemEquipment blueprint)
            {
                if (blueprint == null || blueprint.EquipmentEntity == null)
                    throw new InvalidOperationException(
                        "A production compatibility item lacks its native equipment wrapper.");
                EquipmentEntityLink[] links = blueprint.EquipmentEntity
                    .GetLinks(_fixtures[_fixtureIndex].Gender,
                        _fixtures[_fixtureIndex].Race.RaceId)
                    .Where(value => value != null &&
                        !string.IsNullOrWhiteSpace(value.AssetId)).ToArray();
                EquipmentEntity[] expected = links.Select(link =>
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        link.AssetId, true)).ToArray();
                EquipmentEntity[] entities = blueprint.EquipmentEntity
                    .Load(_fixtures[_fixtureIndex].Gender,
                        _fixtures[_fixtureIndex].Race.RaceId)
                    .Where(value => value != null).Distinct().ToArray();
                bool exact = entities.Length == expected.Length &&
                    expected.All(value => value != null) &&
                    entities.Select((value, index) => ReferenceEquals(value,
                        expected[index])).All(value => value);
                if (!exact || entities.Length == 0)
                    throw new InvalidOperationException(blueprint.AssetGuid +
                        " has no exact ordered native entities for " +
                        _fixtures[_fixtureIndex].Label + ".");
                _expectedOverlayAssetIds = links.Select(value =>
                    value.AssetId).ToArray();
                return entities;
            }

            private void RemoveProductionWeapon()
            {
                if (_actor != null && _actor.Body != null &&
                    _actor.Body.PrimaryHand.MaybeItem != null)
                    _actor.Body.PrimaryHand.RemoveItem(false);
                if (_weaponItem != null)
                {
                    if (_firearmStateSet)
                        FirearmRuntimeState.Service.Forget(_weaponItem);
                    _weaponItem.Dispose();
                }
                _weaponItem = null;
                _firearmStateSet = false;
            }

            private static void RemoveProductionItem(ItemSlot slot,
                ref ItemEntity item)
            {
                if (slot != null && item != null &&
                    ReferenceEquals(slot.MaybeItem, item))
                    slot.RemoveItem(false);
                if (item != null) item.Dispose();
                item = null;
            }

            private static void RemoveProductionItem(ItemSlot slot,
                ref ItemEntityArmor item)
            {
                ItemEntity value = item;
                RemoveProductionItem(slot, ref value);
                item = null;
            }

            private bool ReadBackpackVisible()
            {
                return _avatar != null &&
                    (bool)_showBackpackField.GetValue(_avatar);
            }

            private void PollCaseAndCapture()
            {
                ProductionCompatibilityCase value =
                    ProductionCompatibilityCases[_caseIndex];
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "settle-" + fixture.Label + "-" + value.Label;
                Game.Instance.EntityCreator.Tick();
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                _actor.View.HandsEquipment.UpdateAll();
                _settleUpdates++;

                bool productionPresent = ProductionEntitiesPresent();
                bool hairPreserved = _avatar.EquipmentEntities.Any(current =>
                    ReferenceEquals(current, _hairEntity));
                bool savedLinksUnchanged = _baseSavedLinks.SequenceEqual(
                    ProductionSavedLinks(_avatar), StringComparer.Ordinal);
                bool overlaysPresent = _expectedOverlayAssetIds.Length ==
                        _expectedOverlayEntities.Length &&
                    _expectedOverlayEntities.All(expected =>
                        _avatar.EquipmentEntities.Any(current =>
                            ReferenceEquals(current, expected)));
                bool baseReferencesPresent = _baseSnapshot.All(expected =>
                    _avatar.EquipmentEntities.Any(current =>
                        ReferenceEquals(current, expected.Entity)));
                bool noUnexpectedAvatarEntities =
                    _expectedOverlayEntities.Length == 0
                        ? _avatar.EquipmentEntities.Count ==
                            _baseSnapshot.Length
                        : _avatar.EquipmentEntities.All(current =>
                            _baseSnapshot.Any(state => ReferenceEquals(
                                state.Entity, current)) ||
                            _expectedOverlayEntities.Any(expected =>
                                ReferenceEquals(expected, current)));
                bool rampsExact = ProductionRampsExact(
                    value.Kind == "alternate-color");
                bool slotReady = CurrentSlotReady(value.Kind);
                string weaponPresentationRole = "none";
                Transform weaponModel = null;
                if (_weaponItem != null)
                {
                    WeaponVisualParameters visual = _weaponItem.Blueprint ==
                        null ? null : _weaponItem.Blueprint.VisualParameters;
                    if (visual == null)
                        throw new InvalidOperationException(value.Label +
                            " has no effective native weapon presentation.");
                    weaponModel = WeaponPresentationEvidenceScenario
                        .ResolveActivePresentation(_actor, visual,
                            _expectStoredWeapon ? "stored" : "held-idle",
                            out weaponPresentationRole);
                }
                bool weaponItemExact = ReferenceEquals(
                    _actor.Body.PrimaryHand.MaybeWeapon, _weaponItem);
                bool weaponModelRenderable = Renderable(weaponModel);
                bool weaponReady = _expectHeldWeapon
                    ? _actor.View.HandsEquipment.InCombat &&
                        weaponItemExact && weaponModelRenderable
                    : _expectStoredWeapon
                        ? !_actor.View.HandsEquipment.InCombat &&
                            weaponItemExact && weaponModelRenderable
                        : !_actor.View.HandsEquipment.InCombat &&
                            _actor.Body.PrimaryHand.MaybeItem == null &&
                            _actor.Body.SecondaryHand.MaybeItem == null &&
                            _actor.View.HandsEquipment
                                .GetWeaponModel(false) == null &&
                            _actor.View.HandsEquipment
                                .GetWeaponModel(true) == null;
                bool backpackReady = ReadBackpackVisible() ==
                    _expectBackpack;
                Renderer[] renderers = ActiveRenderers(_actor);
                bool ready = _settleUpdates >= MinimumSettleUpdates &&
                    productionPresent && hairPreserved &&
                    savedLinksUnchanged && overlaysPresent &&
                    baseReferencesPresent && noUnexpectedAvatarEntities &&
                    rampsExact && slotReady && weaponReady &&
                    backpackReady && renderers.Length > 0;
                if (!ready)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Label +
                        " did not settle exactly; production=" +
                        productionPresent + ";hair=" + hairPreserved +
                        ";saved=" + savedLinksUnchanged + ";overlay=" +
                        overlaysPresent + ";base=" +
                        baseReferencesPresent + ";unexpected=" +
                        !noUnexpectedAvatarEntities + ";ramps=" +
                        rampsExact + ";slot=" + slotReady + ";weapon=" +
                        weaponReady + ";backpack=" + backpackReady +
                        ";renderers=" + renderers.Length + ";inCombat=" +
                        _actor.View.HandsEquipment.InCombat +
                        ";weaponItemExact=" + weaponItemExact +
                        ";weaponModelRenderable=" +
                        weaponModelRenderable + ";presentationRole=" +
                        weaponPresentationRole + ".");
                }

                CaptureProductionCase(value, fixture, renderers,
                    _expectHeldWeapon || _expectStoredWeapon
                        ? weaponModel : null, weaponPresentationRole);
                _caseIndex++;
                if (_caseIndex < ProductionCompatibilityCases.Length)
                {
                    _phase = 3;
                    _settleUpdates = 0;
                    return;
                }
                FinishFixture();
            }

            private bool ProductionRampsExact(bool alternate)
            {
                foreach (EquipmentEntity entity in _productionEntities)
                {
                    int primaryCount = entity.PrimaryRamps.Count;
                    int secondaryCount = entity.SecondaryRamps.Count;
                    int primary = alternate
                        ? (GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor + 11) % primaryCount
                        : GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor;
                    int secondary = alternate
                        ? (GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor + 17) % secondaryCount
                        : GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor;
                    if (_avatar.GetPrimaryRampIndex(entity) != primary ||
                        _avatar.GetSecondaryRampIndex(entity) != secondary)
                        return false;
                }
                return true;
            }

            private bool CurrentSlotReady(string kind)
            {
                bool hands = kind == "pistol-held" ||
                    kind == "musket-held" || kind == "musket-stored" ||
                    kind == "blunderbuss-held"
                    ? ReferenceEquals(_actor.Body.PrimaryHand.MaybeItem,
                        _weaponItem)
                    : _actor.Body.PrimaryHand.MaybeItem == null &&
                        _actor.Body.SecondaryHand.MaybeItem == null;
                bool armor = kind == "light-armor" ||
                    kind == "heavy-armor"
                    ? ReferenceEquals(_actor.Body.Armor.MaybeItem, _armorItem)
                    : _actor.Body.Armor.MaybeItem == null;
                bool head = kind == "tricorn"
                    ? ReferenceEquals(_actor.Body.Head.MaybeItem, _headItem)
                    : _actor.Body.Head.MaybeItem == null;
                bool shoulders = kind == "cloak"
                    ? ReferenceEquals(_actor.Body.Shoulders.MaybeItem,
                        _cloakItem)
                    : _actor.Body.Shoulders.MaybeItem == null;
                return hands && armor && head && shoulders;
            }

            private void CaptureProductionCase(
                ProductionCompatibilityCase value,
                ProductionCompatibilityFixture fixture,
                Renderer[] renderers, Transform weaponModel,
                string weaponPresentationRole)
            {
                _stage = "capture-" + fixture.Label + "-" + value.Label;
                string stem = SafeFileName("production-" + fixture.Label +
                    "-" + value.Label);
                string previewPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-preview.png");
                string isometricPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-isometric.png");
                WeaponPresentationEvidenceScenario.CaptureSummary preview =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        _actor, weaponModel, renderers, previewPath,
                        !_expectHeldWeapon && !_expectStoredWeapon);
                IsometricCapture isometric = CaptureIsometric(_actor,
                    renderers, isometricPath);
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "fixture", fixture.Label },
                    { "gender", fixture.Gender.ToString() },
                    { "raceName", fixture.Race.name },
                    { "raceGuid", fixture.Race.AssetGuid },
                    { "raceId", fixture.Race.RaceId.ToString() },
                    { "state", value.Label },
                    { "kind", value.Kind },
                    { "transitionFrom", _caseIndex == 0 ? "fixture-base" :
                        ProductionCompatibilityCases[_caseIndex - 1].Label },
                    { "previousStateCleared", _previousStateCleared },
                    { "productionClassGuid", _gunslingerClass.AssetGuid },
                    { "productionAssetIds", new JArray(
                        CurrentProductionAssetIds()) },
                    { "productionEntitiesPresent",
                        ProductionEntitiesPresent() },
                    { "productionRamps", ProductionRampEvidence() },
                    { "selectedHairAssetId", _hairAssetId },
                    { "hairEntityPreserved", _avatar.EquipmentEntities.Any(
                        current => ReferenceEquals(current, _hairEntity)) },
                    { "savedLinksUnchanged", _baseSavedLinks.SequenceEqual(
                        ProductionSavedLinks(_avatar),
                        StringComparer.Ordinal) },
                    { "itemGuid", string.IsNullOrEmpty(_currentItemGuid)
                        ? "<none>" : _currentItemGuid },
                    { "itemName", string.IsNullOrEmpty(_currentItemName)
                        ? "<none>" : _currentItemName },
                    { "weaponKind", _currentWeaponKind },
                    { "weaponHeld", _expectHeldWeapon },
                    { "weaponStoredInactive", _expectStoredWeapon },
                    { "handsInCombat",
                        _actor.View.HandsEquipment.InCombat },
                    { "weaponPresentationRole",
                        weaponPresentationRole },
                    { "weaponModelRenderable",
                        Renderable(weaponModel) },
                    { "bodySlots", BodySlotEvidence() },
                    { "overlayEntities", OverlayEvidence() },
                    { "backpackVisible", ReadBackpackVisible() },
                    { "activeRendererCount", renderers.Length },
                    { "activeEntityNames", new JArray(
                        _avatar.EquipmentEntities.Where(entity =>
                            entity != null).Select(entity =>
                                entity.name).ToArray()) },
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
                    { "saveApiCalled", false },
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
                    _warnings.Add(fixture.Label + "/" + value.Label +
                        " has low foreground pixel density; retain it as a framing diagnostic.");
                WriteProgress("case-captured");
            }

            private JArray ProductionRampEvidence()
            {
                string[] ids = CurrentProductionAssetIds();
                return new JArray(_productionEntities.Select((entity, index) =>
                    new JObject
                    {
                        { "assetId", ids[index] },
                        { "entityName", entity.name },
                        { "primaryRampCount", entity.PrimaryRamps.Count },
                        { "secondaryRampCount",
                            entity.SecondaryRamps.Count },
                        { "appliedPrimary",
                            _avatar.GetPrimaryRampIndex(entity) },
                        { "appliedSecondary",
                            _avatar.GetSecondaryRampIndex(entity) }
                    }).ToArray());
            }

            private JObject BodySlotEvidence()
            {
                return new JObject
                {
                    { "primaryHand", SlotItemGuid(
                        _actor.Body.PrimaryHand) },
                    { "secondaryHand", SlotItemGuid(
                        _actor.Body.SecondaryHand) },
                    { "armor", SlotItemGuid(_actor.Body.Armor) },
                    { "head", SlotItemGuid(_actor.Body.Head) },
                    { "shoulders", SlotItemGuid(
                        _actor.Body.Shoulders) }
                };
            }

            private static string SlotItemGuid(ItemSlot slot)
            {
                return slot == null || slot.MaybeItem == null ||
                    slot.MaybeItem.Blueprint == null
                    ? "<none>" : slot.MaybeItem.Blueprint.AssetGuid;
            }

            private JArray OverlayEvidence()
            {
                return new JArray(_expectedOverlayEntities.Select(
                    (entity, index) =>
                    new JObject
                    {
                        { "assetId", _expectedOverlayAssetIds[index] },
                        { "entityName", entity.name },
                        { "layer", entity.Layer },
                        { "hideBodyParts",
                            entity.HideBodyParts.ToString() },
                        { "specialOutfitParts", new JArray(
                            entity.OutfitParts.Where(part => part != null &&
                                part.Special != EquipmentEntity
                                    .OutfitPartSpecialType.None)
                                .Select(part => part.Special.ToString())
                                .ToArray()) },
                        { "present", _avatar.EquipmentEntities.Any(current =>
                            ReferenceEquals(current, entity)) }
                    }).ToArray());
            }

            private void FinishFixture()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "restore-" + fixture.Label;
                bool baseRestored = ResetTransientState();
                bool originalRestored = baseRestored &&
                    RestoreProductionSnapshot(_avatarBefore,
                        _savedLinksBefore);
                _restorationRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "baseRestored", baseRestored },
                    { "originalRestored", originalRestored },
                    { "originalEntityCount", _avatarBefore.Length },
                    { "restoredEntityCount",
                        _avatar == null ? 0 :
                            _avatar.EquipmentEntities.Count },
                    { "savedLinksUnchanged", _avatar != null &&
                        _savedLinksBefore.SequenceEqual(
                            ProductionSavedLinks(_avatar),
                            StringComparer.Ordinal) }
                });
                if (!originalRestored)
                    throw new InvalidOperationException(fixture.Label +
                        " did not restore its exact original avatar state.");
                _restorations++;
                RetireProductionActor();
                _fixtureIndex++;
                _caseIndex = 0;
                if (_fixtureIndex < _fixtures.Length)
                {
                    _phase = 1;
                    _settleUpdates = 0;
                    return;
                }
                WriteIndex();
                _indexWritten = true;
                BeginCleanup();
            }

            private void RetireProductionActor()
            {
                if (_actor == null && _actorBlueprint == null) return;
                try
                {
                    if (_actor != null && _avatar != null)
                    {
                        RemoveProductionWeapon();
                        RemoveProductionItem(_actor.Body.Armor,
                            ref _armorItem);
                        RemoveProductionItem(_actor.Body.Head,
                            ref _headItem);
                        RemoveProductionItem(_actor.Body.Shoulders,
                            ref _cloakItem);
                        if (_backpackEntity != null)
                            _avatar.RemoveEquipmentEntity(_backpackEntity,
                                false);
                        _backpackEntity = null;
                    }
                }
                finally
                {
                    UnitEntityData dependent = _actor == null ||
                        _actor.Descriptor == null ? null :
                        _actor.Descriptor.Pet;
                    if (dependent != null &&
                        !_unitsBefore.Any(value => ReferenceEquals(value,
                            dependent)))
                    {
                        dependent.Commands.InterruptAll(true);
                        if (dependent.CombatState.IsInCombat)
                            dependent.CombatState.LeaveCombat();
                        if (ContainsReference(_party, dependent))
                            Game.Instance.Player.Party.Remove(dependent);
                        if (ContainsReference(_allUnits, dependent))
                            Game.Instance.State.Units.All.Remove(dependent);
                        if (IsProductionMotion)
                            DisposeProductionMotionEntity(dependent);
                        else
                            dependent.Dispose();
                    }
                    if (_actor != null)
                    {
                        _actor.Commands.InterruptAll(true);
                        if (_actor.CombatState.IsInCombat)
                            _actor.CombatState.LeaveCombat();
                        if (_actor.Descriptor != null)
                            _actor.Descriptor.State.Immortality.ReleaseAll();
                        if (ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        if (IsProductionMotion)
                            DisposeProductionMotionEntity(_actor);
                        else
                            _actor.Dispose();
                    }
                    if (_actorBlueprint != null)
                        UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                    _actor = null;
                    _actorBlueprint = null;
                    _dollState = null;
                    _dollData = null;
                    _dollTemplateAvatar = null;
                    _avatar = null;
                    _productionEntities = new EquipmentEntity[0];
                    _dollEntities = new EquipmentEntity[0];
                    _hairEntity = null;
                    _expectedOverlayEntities = new EquipmentEntity[0];
                    _expectedOverlayAssetIds = new string[0];
                    _avatarBefore = new AvatarEntityState[0];
                    _baseSnapshot = new AvatarEntityState[0];
                    _savedLinksBefore = new string[0];
                    _baseSavedLinks = new string[0];
                    _hairAssetId = string.Empty;
                    _fixtureInitialized = false;
                    _dollAttachmentRecorded = false;
                    _dollCreationResourceGatePassed = false;
                    _resourcePreloadingAtDollCreation = false;
                    _dollResourceWaitUpdates = 0;
                    _productionApplied = false;
                }
            }

            private bool ProductionBlueprintUnchanged()
            {
                return ReferenceEquals(_maleLinksBefore,
                        _gunslingerClass.MaleEquipmentEntities) &&
                    ReferenceEquals(_femaleLinksBefore,
                        _gunslingerClass.FemaleEquipmentEntities) &&
                    ReferenceEquals(_sharedLinksBefore,
                        _gunslingerClass.EquipmentEntities) &&
                    _gunslingerClass.PrimaryColor == _primaryBefore &&
                    _gunslingerClass.SecondaryColor == _secondaryBefore &&
                    _gunslingerClass.MaleEquipmentEntities.Select(value =>
                            value.AssetId).SequenceEqual(
                        GunslingerClassAppearanceCatalog.MaleAssetIds(),
                        StringComparer.Ordinal) &&
                    _gunslingerClass.FemaleEquipmentEntities.Select(value =>
                            value.AssetId).SequenceEqual(
                        GunslingerClassAppearanceCatalog.FemaleAssetIds(),
                        StringComparer.Ordinal) &&
                    _gunslingerClass.EquipmentEntities.Length == 0;
            }

            private void WriteIndex()
            {
                _stage = "write-production-compatibility-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "loadedModVersion",
                        _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _gameAssemblySha256 },
                    { "gameAssemblyMvid", _gameAssemblyMvid },
                    { "productionClassGuid", _gunslingerClass.AssetGuid },
                    { "maleAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.MaleAssetIds()) },
                    { "femaleAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.FemaleAssetIds()) },
                    { "defaultPrimaryColor",
                        GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor },
                    { "defaultSecondaryColor",
                        GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor },
                    { "states", new JArray(
                        ProductionCompatibilityCases.Select(value =>
                            value.Label).ToArray()) },
                    { "supportedRaces", new JArray(
                        _supportedRaces.Select(value => new JObject
                        {
                            { "name", value.name },
                            { "guid", value.AssetGuid },
                            { "raceId", value.RaceId.ToString() }
                        }).ToArray()) },
                    { "productionRaceLinkMatrix", _raceLinkRecords },
                    { "fixtures", _fixtureRecords },
                    { "records", _records },
                    { "restorations", _restorationRecords },
                    { "productionBlueprintUnchanged",
                        ProductionBlueprintUnchanged() },
                    { "saveApiCalled", false },
                    { "productionBlueprintMutated", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    IsElementalRaceClassEquipment
                        ? "elemental-race-class-equipment-index.json"
                        : "gunslinger-outfit-production-compatibility-index.json");
                WriteJsonAtomic(path, index);
                _evidenceFiles.Add(path);
            }

            private void WriteProgress(string progressStage)
            {
                var progress = new JObject
                {
                    { "schemaVersion", 1 },
                    { "utc", DateTime.UtcNow.ToString("o") },
                    { "stage", progressStage },
                    { "detailStage", _stage },
                    { "fixtureIndex", _fixtureIndex },
                    { "caseIndex", _caseIndex },
                    { "phase", _phase },
                    { "motionStep", IsProductionMotion ?
                        _motionStep : -1 },
                    { "captured", _captured },
                    { "imageCount", _imageCount },
                    { "actorPresent", _actor != null }
                };
                WriteJsonAtomic(Path.Combine(_request.EvidenceDirectory,
                    IsProductionMotion
                        ? IsElementalRaceMotion
                            ? "elemental-race-motion-progress.json"
                            : "gunslinger-outfit-production-motion-progress.json"
                        : IsElementalRaceClassEquipment
                            ? "elemental-race-class-equipment-progress.json"
                            : "gunslinger-outfit-production-compatibility-progress.json"),
                    progress);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = IsProductionMotion
                    ? IsElementalRaceMotion
                        ? "elemental-race-motion-cleanup"
                        : "gunslinger-outfit-production-motion-cleanup"
                    : IsElementalRaceClassEquipment
                        ? "elemental-race-class-equipment-cleanup"
                        : "gunslinger-outfit-production-compatibility-cleanup";
                try
                {
                    if (IsProductionMotion)
                        PrepareProductionMotionCleanup();
                    try
                    {
                        RetireProductionActor();
                    }
                    finally
                    {
                        if (IsProductionMotion)
                        {
                            try
                            {
                                RetireProductionMotionFactions();
                            }
                            finally
                            {
                                RetireProductionMotionScene();
                            }
                        }
                    }
                }
                catch (Exception cleanupException)
                {
                    _diagnostics.Add("cleanupException=" +
                        cleanupException);
                    try
                    {
                        if (_actor != null &&
                            ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        if (_actor != null)
                        {
                            if (IsProductionMotion)
                                DisposeProductionMotionEntity(_actor);
                            else
                                _actor.Dispose();
                        }
                        if (_actorBlueprint != null)
                            UnityEngine.Object.DestroyImmediate(
                                _actorBlueprint);
                    }
                    catch (Exception fallbackException)
                    {
                        _diagnostics.Add("cleanupFallbackException=" +
                            fallbackException);
                    }
                    _actor = null;
                    _actorBlueprint = null;
                    _dollTemplateAvatar = null;
                    _avatar = null;
                    _dollAttachmentRecorded = false;
                    if (IsProductionMotion)
                    {
                        try
                        {
                            RetireProductionMotionScene();
                        }
                        catch (Exception sceneCleanupException)
                        {
                            _diagnostics.Add("sceneCleanupException=" +
                                sceneCleanupException);
                        }
                    }
                }
                _cleanupStarted = true;
                _settleUpdates = 0;
                WriteProgress("cleanup-started");
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                if (IsProductionMotion) RestoreProductionMotionInventory();
                bool cleaned = SameReferences(_unitsBefore,
                        Snapshot(_allUnits)) &&
                    SameReferences(_partyBefore, Snapshot(_party)) &&
                    _actor == null;
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates) return;
                Finish(cleaned);
            }

            private void Finish(bool cleaned)
            {
                if (IsProductionMotion)
                {
                    FinishProductionMotion(cleaned);
                    return;
                }
                int expectedFixtures = _fixtures.Length;
                int expectedRecords = expectedFixtures *
                    ProductionCompatibilityCases.Length;
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool exactStateCounts = ProductionCompatibilityCases.All(
                    state => records.Count(value => string.Equals(
                        (string)value["state"], state.Label,
                        StringComparison.Ordinal)) == expectedFixtures);
                bool recordContracts = records.All(value =>
                    (bool)value["previousStateCleared"] &&
                    (bool)value["productionEntitiesPresent"] &&
                    (bool)value["hairEntityPreserved"] &&
                    (bool)value["savedLinksUnchanged"] &&
                    (int)value["activeRendererCount"] > 0 &&
                    (int)value["preview"]["meaningfulPixels"] > 0 &&
                    (int)value["isometric"]["meaningfulPixels"] > 0);
                bool exactRaceLinks = _raceLinkRecords.Count ==
                        _supportedRaces.Length * 2 &&
                    _raceLinkRecords.OfType<JObject>().All(value =>
                        (bool)value["orderedPairExact"]);
                bool elemental = IsElementalRaceClassEquipment;
                string expectedScenario = elemental
                    ? RuntimeTestScenarioCatalog.ElementalRaceClassEquipment
                    : RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionCompatibility;
                Func<string, string> assertionId = suffix => elemental
                    ? "elemental-race-class-equipment-" + suffix
                    : "gunslinger-outfit-production-" + suffix;
                JObject[] fixtureRecords = _fixtureRecords.OfType<JObject>()
                    .ToArray();
                bool commonFixtureContracts = fixtureRecords.All(value =>
                    string.Equals((string)value["dollCharacterClassGuid"],
                        OutfitProductionClassGuid,
                        StringComparison.Ordinal) &&
                    (bool)value["dollCreationResourceGatePassed"] &&
                    !(bool)value["resourcePreloadingAtDollCreation"] &&
                    (bool)value["rigExact"]);
                bool fixtureContracts = _fixtureRecords.Count ==
                        expectedFixtures && commonFixtureContracts &&
                    (elemental
                        ? expectedFixtures == ElementalRaceCatalog.RaceCount *
                                2 && _supportedRaces.Length ==
                                ElementalRaceCatalog.RaceCount &&
                            fixtureRecords.All(value => string.Equals(
                                (string)value["raceId"], "Aasimar",
                                StringComparison.Ordinal)) &&
                            _supportedRaces.All(race => fixtureRecords.Count(
                                value => string.Equals((string)value[
                                    "raceGuid"], race.AssetGuid,
                                    StringComparison.Ordinal)) == 2)
                        : expectedFixtures == 2 && fixtureRecords.All(value =>
                            string.Equals((string)value["raceId"], "Human",
                                StringComparison.Ordinal)));

                Add(_assertions,
                    elemental
                        ? "elemental-race-class-equipment-guard"
                        : "gunslinger-outfit-production-compatibility-guard",
                    expectedScenario,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        expectedScenario,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    assertionId("save-boundary"),
                    "KMG_AUTOMATION_WORKING; no save API",
                    "saveName=" + _request.Parameters.Value<string>(
                        "saveName") + ";saveApiCalled=false",
                    string.Equals(_request.Parameters.Value<string>(
                        "saveName"), "KMG_AUTOMATION_WORKING",
                        StringComparison.Ordinal),
                    "guarded working-save load plus disposable actors");
                Add(_assertions,
                    assertionId("game-identity"),
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
                    assertionId("race-links"),
                    elemental
                        ? "exact accepted Gunslinger pair for all four elemental races and both genders"
                        : "exact production pair for every installed player race/gender",
                    "races=" + _supportedRaces.Length + ";rows=" +
                        _raceLinkRecords.Count,
                    exactRaceLinks,
                    "BlueprintCharacterClass.LoadClothes on installed race catalog");
                Add(_assertions,
                    elemental
                        ? "elemental-race-class-equipment-fixtures"
                        : "gunslinger-outfit-production-human-previews",
                    elemental
                        ? "one exact male and female production Gunslinger DollState/DollData fixture for each elemental race"
                        : "one exact male and female Human Gunslinger DollState/DollData fixture with selected native hair",
                    "fixtures=" + _fixtureRecords.Count,
                    fixtureContracts,
                    "native DollState.SetClass/CreateData/CreateUnitView path");
                Add(_assertions,
                    assertionId("equipment-matrix"),
                    "16 exact reversible color, weapon, armor, headgear/hair, cloak, backpack, inactive-weapon, and rebuild states per race/gender fixture",
                    "records=" + records.Length + ";exactStates=" +
                        exactStateCounts,
                    records.Length == expectedRecords && exactStateCounts &&
                        recordContracts,
                    "real Body slots, Character rebuilds, native equipment wrappers, and exact state sidecars");
                Add(_assertions,
                    assertionId("visual-captures"),
                    expectedRecords + " sidecars, " +
                        (expectedRecords * 2) + " PNGs, " +
                        (expectedRecords * 5) + " labelled views",
                    "captured=" + _captured + ";images=" + _imageCount +
                        ";views=" + _viewCount + ";files=" +
                        _evidenceFiles.Count,
                    _captured == expectedRecords &&
                        _imageCount == expectedRecords * 2 &&
                        _viewCount == expectedRecords * 5 &&
                        _indexWritten &&
                        _evidenceFiles.Count == expectedRecords * 3 + 1 &&
                        _evidenceFiles.All(File.Exists),
                    "four-view preview sheets plus elevated ordinary isometric captures");
                Add(_assertions,
                    assertionId("restoration"),
                    "exact original avatar state and saved links restored for every fixture",
                    "restored=" + _restorations + "/" +
                        expectedFixtures,
                    _restorations == expectedFixtures &&
                        _restorationRecords.Count == expectedFixtures &&
                        _restorationRecords.OfType<JObject>().All(value =>
                            (bool)value["baseRestored"] &&
                            (bool)value["originalRestored"] &&
                            (bool)value["savedLinksUnchanged"]),
                    "saved:false entity/ramp snapshots and Character.RebuildOutfit");
                Add(_assertions,
                    assertionId("blueprint-immutability"),
                    "published class arrays, links, and colors remain exact original references and values",
                    "unchanged=" + ProductionBlueprintUnchanged(),
                    ProductionBlueprintUnchanged(),
                    "pre/post production BlueprintCharacterClass snapshot");
                Add(_assertions,
                    assertionId("cleanup"),
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";updates=" +
                        _settleUpdates, cleaned,
                    "request-local actors, items, blueprint clones, cameras, and textures");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Direct inspection of every generated image is required before aesthetic or clipping acceptance.");
                _warnings.Add("This scenario proves static equipment and rebuild compatibility; native motion commands are qualified separately.");
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
                    DurationMilliseconds = (long)(DateTime.UtcNow - _started)
                        .TotalMilliseconds,
                    Assertions = _assertions,
                    Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }
        }
    }
}
