using System;
using System.Collections;
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
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free, request-local proof that a distinct race identity can use
    /// native character creation, facts, visuals, and persistence without
    /// ever being published to the shared CharacterRaces selector.
    /// </summary>
    internal static class ElementalRaceDevelopmentProbeScenario
    {
        internal const string EvidenceFileName =
            "elemental-race-development-probe.json";
        private const string ProbeName =
            "KMG_ElementalRace_DevelopmentProbe";

        private static readonly string[] NativeRaceGuids =
        {
            "0a5d473ead98b0646b94495af250fdc4",
            "b7f02ba92b363064fb873963bec275ee",
            "5c4e42124dc2b4647af6e36cf2590500",
            "25a5878d125338244896ebd3238226c8",
            "c4faf439f0e70bd40b5e36ee80d06be7",
            "b3646842ffbd01643ab4dac7479b20b0",
            "1dc20e195581a804890ddc74218bfd8e"
        };

        private const string KeenSensesGuid =
            "9c747d24f6321f744aa1bb4bd343880d";
        private const string SlowAndSteadyGuid =
            "786588ad1694e61498e77321d4b07157";
        private const string OutsiderTypeGuid =
            "9054d3988d491d944ac144e27b6bc318";
        private const string BurningHandsGuid =
            "4783c3709a74a794dbe7c8e7e0b1b038";
        private const string StoneFistGuid =
            "85067a04a97416949b5d1dbf986d93f3";
        private const string FeatherStepGuid =
            "f3c0b267dd17a2a45a40805e31fe3cd1";
        private const string HoldPersonGuid =
            "c7104f7526c4c524f91474614054547e";
        private const string EnlargePersonGuid =
            "c60969e7f264e6d4b84a1499fdcf9039";
        private const string ReducePersonGuid =
            "4e0e9aba6447d514f88eff1464cc4763";

        private sealed class PersistenceEnvelope
        {
            [JsonProperty("race", Order = 1)]
            public BlueprintRace Race { get; set; }
            [JsonProperty("raceGuid", Order = 2)]
            public string RaceGuid { get; set; }
        }

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        internal sealed class Session
        {
            private const int MaximumViewSettleUpdates = 360;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly JObject _evidence = new JObject
            {
                { "schemaVersion", 1 },
                { "saveStateTouched", false },
                { "publishedToCharacterRaces", false },
                { "nativeRaces", new JArray() },
                { "mechanicDonors", new JArray() },
                { "outsiderPrecedent", new JObject() },
                { "productionRaces", new JArray() },
                { "dolls", new JArray() },
                { "sameRaceCollisionCandidates", new JArray() }
            };
            private BlueprintRoot _root;
            private LibraryScriptableObject _library;
            private BlueprintRace[] _racesBefore;
            private string[] _identitiesBefore;
            private int _allBefore = -1;
            private int _dictionaryBefore = -1;
            private BlueprintRace _probe;
            private BlueprintFeature _diagnostic;
            private BlueprintCharacterClass _gunslinger;
            private ProbeRegistration _registration;
            private UnitEntityData _unit;
            private LevelUpController _levelController;
            private DollState _maleState;
            private DollState _femaleState;
            private DollData _male;
            private DollData _female;
            private UnitEntityView _maleView;
            private UnitEntityView _femaleView;
            private int _phase;
            private int _viewSettleUpdates;
            private string _stage = "resolve-root";
            private string _exceptionSummary = string.Empty;

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                if (request == null)
                    throw new ArgumentNullException("request");
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
                    if (_phase == 0)
                    {
                        Initialize();
                        _phase = 1;
                        return;
                    }
                    PollDollReadiness();
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
                if (_root == null || _root.Progression == null ||
                    _root.Progression.CharacterRaces == null ||
                    _library == null ||
                    _library.BlueprintsByAssetId == null ||
                    _library.GetAllBlueprints() == null)
                    throw new InvalidOperationException(
                        "The live blueprint root or library is unavailable.");
                _racesBefore = _root.Progression.CharacterRaces;
                _identitiesBefore = RaceIdentities(_racesBefore);
                _allBefore = _library.GetAllBlueprints().Count;
                _dictionaryBefore = _library.BlueprintsByAssetId.Count;

                _stage = "audit-native-races";
                var native = new List<BlueprintRace>();
                foreach (string guid in NativeRaceGuids)
                {
                    BlueprintRace race = BlueprintLibraryLookup
                        .RequireExact<BlueprintRace>(_library, guid,
                            "elemental-race-development-probe");
                    native.Add(race);
                    ((JArray)_evidence["nativeRaces"]).Add(
                        DescribeRace(race));
                }
                Add(_assertions, "elemental-probe-native-races",
                    "seven exact native race GUIDs resolve uniquely",
                    string.Join("|", native.Select(Identity).ToArray()),
                    native.Count == 7 && native.Select(value => value.AssetGuid)
                        .Distinct(StringComparer.Ordinal).Count() == 7,
                    "live blueprint library");

                _stage = "audit-native-mechanic-donors";
                AuditMechanicDonors(_library, native, _evidence,
                    _assertions);

                _stage = "audit-production-elemental-identities";
                ElementalRaceBlueprintSet production =
                    BlueprintBootstrap.ElementalRaces;
                if (production == null)
                    throw new InvalidOperationException(
                        "The production elemental race blueprint set is unavailable.");
                BlueprintRace[] productionRaces = production.OrderedRaces();
                foreach (BlueprintRace race in productionRaces)
                    ((JArray)_evidence["productionRaces"]).Add(
                        DescribeRace(race));
                bool productionRegistered =
                    production.Count == ElementalRaceIdentityCatalog.IdentityCount &&
                    productionRaces.Length == ElementalRaceCatalog.RaceCount &&
                    production.OrderedBlueprints().All(IsRegisteredExactly);
                Add(_assertions, "elemental-production-identities",
                    "24 project identities resolve exactly in fixed Ifrit/Oread/Sylph/Undine order",
                    "count=" + production.Count + ";races=" +
                        string.Join("|", productionRaces.Select(Identity)
                            .ToArray()),
                    productionRegistered,
                    "live production BlueprintBootstrap set and library indexes");

                bool selectorsEnabled = _context.FeatureModules.Active
                    .ElementalRaces;
                int[] publicationCounts = productionRaces.Select(race =>
                    _racesBefore.Count(value => ReferenceEquals(value, race)))
                    .ToArray();
                bool publicationExact = publicationCounts.All(value =>
                    value == (selectorsEnabled ? 1 : 0));
                Add(_assertions, "elemental-production-publication-gate",
                    selectorsEnabled ?
                        "each production race is published exactly once" :
                        "all identities remain registered while selectors remain unpublished",
                    "enabled=" + selectorsEnabled + ";counts=" +
                        string.Join(",", publicationCounts.Select(value =>
                            value.ToString()).ToArray()),
                    publicationExact,
                    "restart-bound active module state and CharacterRaces snapshot");

                _stage = "detect-same-race-collisions";
                JArray collisions = (JArray)_evidence[
                    "sameRaceCollisionCandidates"];
                var ownedRaceGuids = new HashSet<string>(
                    productionRaces.Select(value => value.AssetGuid),
                    StringComparer.Ordinal);
                foreach (BlueprintRace candidate in ResourcesLibrary
                    .GetBlueprints<BlueprintRace>().Where(value =>
                        value != null && IsElementalName(value) &&
                        !ownedRaceGuids.Contains(value.AssetGuid)))
                    collisions.Add(Identity(candidate));
                Add(_assertions, "elemental-probe-no-same-race-collision",
                    "no foreign live Ifrit, Oread, Sylph, or Undine race blueprint",
                    collisions.Count == 0 ? "none" :
                        collisions.ToString(Formatting.None),
                    collisions.Count == 0,
                    "live cache scan; any match requires ownership review");

                _stage = "register-hidden-probe";
                BlueprintRace human = native[0];
                _diagnostic = BlueprintBootstrap.DiagnosticFeature;
                if (_diagnostic == null)
                    throw new InvalidOperationException(
                        "The project diagnostic feature is unavailable.");
                _probe = BlueprintCloneService.Clone(human, ProbeName);
                _probe.Features = (human.Features ??
                    new BlueprintFeature[0]).Concat(new[] { _diagnostic })
                    .ToArray();
                SetMember(_probe, "SelectableRaceStat", false);
                _registration = new ProbeRegistration(_library, _probe,
                    ElementalRaceDiagnosticIdentityCatalog.Guid);
                _evidence["probe"] = DescribeRace(_probe);
                bool hidden = !_root.Progression.CharacterRaces.Any(value =>
                    ReferenceEquals(value, _probe) || value != null &&
                    string.Equals(value.AssetGuid, _probe.AssetGuid,
                        StringComparison.Ordinal));
                Add(_assertions, "elemental-probe-hidden-registration",
                    "reserved identity resolves in both indexes and is absent from CharacterRaces",
                    "guid=" + _probe.AssetGuid + ";hidden=" + hidden,
                    hidden && _registration.Owned && ReferenceEquals(
                        ResourcesLibrary.TryGetBlueprint<BlueprintRace>(
                            _probe.AssetGuid), _probe),
                    "request-local exact registration");

                _stage = "create-dolls";
                _gunslinger =
                    BlueprintBootstrap.GunslingerClass == null ? null :
                    BlueprintBootstrap.GunslingerClass.CharacterClass;
                if (_gunslinger == null)
                    throw new InvalidOperationException(
                        "The production Gunslinger class is unavailable.");
                _male = CreateDoll(_probe, Gender.Male, _gunslinger,
                    out _maleState);
                _female = CreateDoll(_probe, Gender.Female, _gunslinger,
                    out _femaleState);
                _maleView = CreateView("male", _male);
                _femaleView = CreateView("female", _female);
            }

            private void PollDollReadiness()
            {
                _stage = "settle-dolls";
                Game.Instance.EntityCreator.Tick();
                _viewSettleUpdates++;
                bool maleReady = ViewReady(_maleView);
                bool femaleReady = ViewReady(_femaleView);
                if ((!maleReady || !femaleReady) &&
                    _viewSettleUpdates < MaximumViewSettleUpdates)
                    return;

                JArray dolls = (JArray)_evidence["dolls"];
                dolls.Add(DescribeView("male", _male, _maleView,
                    maleReady, _viewSettleUpdates));
                dolls.Add(DescribeView("female", _female, _femaleView,
                    femaleReady, _viewSettleUpdates));
                if (!maleReady || !femaleReady)
                    throw new InvalidOperationException(
                        "Diagnostic doll views did not render within " +
                        MaximumViewSettleUpdates + " native update frames.");

                bool outfit = GunslingerClassAppearanceCatalog.MaleAssetIds()
                    .Concat(GunslingerClassAppearanceCatalog.FemaleAssetIds())
                    .All(value => ResourcesLibrary.TryGetResource<
                        EquipmentEntity>(value, true) != null);
                bool classExact = _maleState != null &&
                    _femaleState != null && ReferenceEquals(
                        _maleState.CharacterClass, _gunslinger) &&
                    ReferenceEquals(_femaleState.CharacterClass,
                        _gunslinger);
                Add(_assertions, "elemental-probe-dolls-and-outfit",
                    "male/female dolls render and accepted Gunslinger outfit links resolve",
                    "maleEntities=" + _male.EquipmentEntityIds.Count +
                        ";femaleEntities=" + _female.EquipmentEntityIds.Count +
                        ";settleUpdates=" + _viewSettleUpdates +
                        ";classExact=" + classExact +
                        ";outfit=" + outfit,
                    maleReady && femaleReady && classExact && outfit,
                    "native multi-frame DollState/DollData rendering and accepted appearance catalog");
                CompleteMechanicalChecks();
                Finish();
            }

            private void CompleteMechanicalChecks()
            {
                _stage = "apply-race-facts";
                _unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    _root.DefaultPlayerCharacter).Unit;
                _levelController = LevelUpController
                    .StartWithoutAssigningStaticInstance(_unit.Descriptor,
                        false, null, null,
                        LevelUpState.CharBuildMode.CharGen);
                if (_levelController == null ||
                    _levelController.Preview == null)
                    throw new InvalidOperationException(
                        "The native character-creation race preview is unavailable.");
                bool selected = _levelController.SelectRace(_probe);
                UnitDescriptor preview = _levelController.Preview;
                int raceRank = preview.Progression.Features
                    .GetRank(_probe);
                int diagnosticRank = preview.Progression.Features
                    .GetRank(_diagnostic);
                _evidence["unit"] = new JObject
                {
                    { "selected", selected },
                    { "previewRaceExact", ReferenceEquals(
                        preview.Progression.Race, _probe) },
                    { "raceRank", raceRank },
                    { "diagnosticRank", diagnosticRank }
                };
                Add(_assertions, "elemental-probe-race-facts",
                    "native character creation retains exact race identity and applies clone-only fact",
                    _evidence["unit"].ToString(Formatting.None),
                    selected && ReferenceEquals(preview.Progression.Race,
                        _probe) && raceRank == 1 && diagnosticRank == 1,
                    "detached ChargenUnit and LevelUpController.SelectRace");

                _stage = "native-json-roundtrip";
                JsonSerializerSettings settings = Kingmaker.EntitySystem
                    .Persistence.JsonUtility.DefaultJsonSettings
                    .DefaultSettings;
                string json = JsonConvert.SerializeObject(
                    new PersistenceEnvelope { Race = _probe,
                        RaceGuid = _probe.AssetGuid },
                    Formatting.None, settings);
                PersistenceEnvelope restored = JsonConvert.DeserializeObject<
                    PersistenceEnvelope>(json, settings);
                bool raceExact = restored != null &&
                    ReferenceEquals(restored.Race, _probe);
                bool guidExact = restored != null && string.Equals(
                    restored.RaceGuid, _probe.AssetGuid,
                    StringComparison.Ordinal);
                bool hidden = !_root.Progression.CharacterRaces.Any(value =>
                    ReferenceEquals(value, _probe) || value != null &&
                    string.Equals(value.AssetGuid, _probe.AssetGuid,
                        StringComparison.Ordinal));
                _evidence["persistence"] = new JObject
                {
                    { "jsonLength", json.Length },
                    { "raceExact", raceExact },
                    { "guidExact", guidExact },
                    { "hiddenFromCharacterRaces", hidden }
                };
                Add(_assertions, "elemental-probe-persistence",
                    "hidden stable race reference survives native JSON and resolves without selector publication",
                    _evidence["persistence"].ToString(Formatting.None),
                    raceExact && guidExact && hidden,
                    "DefaultJsonSettings blueprint reference converter");
            }

            private void RecordException(Exception exception)
            {
                _exceptionSummary = exception.ToString();
                _warnings.Add("probeExceptionStage=" + _stage);
                _diagnostics.Add(exception.ToString());
                Add(_assertions, "elemental-probe-exception", "no exception",
                    "stage=" + _stage + ";" +
                        exception.GetType().FullName + ": " +
                        exception.Message, false,
                    "guarded request-local probe");
            }

            private void Finish()
            {
                if (Complete) return;
                _stage = "atomic-cleanup";
                if (_levelController != null)
                    _levelController.Cancel();
                if (_unit != null) _unit.Dispose();
                DestroyView(_femaleView);
                DestroyView(_maleView);
                if (_registration != null) _registration.Dispose();
                if (_probe != null)
                    UnityEngine.Object.DestroyImmediate(_probe);

                bool rootExact = _root != null && _racesBefore != null &&
                    ReferenceEquals(_root.Progression.CharacterRaces,
                        _racesBefore) && RaceIdentities(
                        _root.Progression.CharacterRaces).SequenceEqual(
                            _identitiesBefore ?? new string[0],
                            StringComparer.Ordinal);
                bool indexesExact = _library != null &&
                    _library.GetAllBlueprints().Count == _allBefore &&
                    _library.BlueprintsByAssetId.Count == _dictionaryBefore &&
                    !_library.BlueprintsByAssetId.ContainsKey(
                        ElementalRaceDiagnosticIdentityCatalog.Guid);
                _evidence["cleanup"] = new JObject
                {
                    { "characterRacesExact", rootExact },
                    { "libraryIndexesExact", indexesExact }
                };
                Add(_assertions, "elemental-probe-atomic-cleanup",
                    "CharacterRaces and both library indexes restored exactly",
                    _evidence["cleanup"].ToString(Formatting.None),
                    rootExact && indexesExact,
                    "exact-reference request-local rollback");
                Add(_assertions, "elemental-probe-save-free",
                    "no save, input, selector publication, or persistent unit",
                    "saveStateTouched=false;publishedToCharacterRaces=false",
                    true, "guarded mod-load scenario");

                string path = Path.Combine(_request.EvidenceDirectory,
                    EvidenceFileName);
                RuntimeTestResultWriter.WriteAtomic(path,
                    _evidence.ToString(Formatting.Indented) +
                        Environment.NewLine);
                _diagnostics.Add("probeEvidenceSha256=" + Hash(path));
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

        private static JObject DescribeRace(BlueprintRace race)
        {
            return new JObject
            {
                { "identity", Identity(race) },
                { "raceId", race.RaceId.ToString() },
                { "size", race.Size.ToString() },
                { "speed", DescribeMember(race, "Speed") },
                { "selectableRaceStat", DescribeMember(race,
                    "SelectableRaceStat") },
                { "features", new JArray((race.Features ??
                    new BlueprintFeature[0]).Select(value => value == null ?
                        "<null>" : value.name + "/" + value.AssetGuid +
                        "/" + value.GetType().FullName).ToArray()) },
                { "components", new JArray((race.ComponentsArray ??
                    new BlueprintComponent[0]).Select(value => value == null ?
                        "<null>" : value.GetType().FullName).ToArray()) },
                { "maleOptions", DescribeFields(race.MaleOptions) },
                { "femaleOptions", DescribeFields(race.FemaleOptions) },
                { "presets", new JArray((race.Presets ??
                    new BlueprintRaceVisualPreset[0]).Select(value =>
                        value == null ? "<null>" : value.name + "/" +
                        value.AssetGuid + "/race=" + value.RaceId +
                        "/skin=" + DescribeUnity(value.Skin) +
                        "/maleSkeleton=" +
                        DescribeUnity(value.MaleSkeleton) +
                        "/femaleSkeleton=" +
                        DescribeUnity(value.FemaleSkeleton)).ToArray()) }
            };
        }

        private static void AuditMechanicDonors(
            LibraryScriptableObject library, IList<BlueprintRace> races,
            JObject evidence,
            ICollection<RuntimeTestAssertion> assertions)
        {
            if (races == null || races.Count != NativeRaceGuids.Length)
                throw new InvalidOperationException(
                    "The exact native race inventory is unavailable.");

            BlueprintFeature keen = Require<BlueprintFeature>(library,
                KeenSensesGuid, "native Keen Senses");
            BlueprintFeature slow = Require<BlueprintFeature>(library,
                SlowAndSteadyGuid, "native Slow and Steady");
            BlueprintFeature outsider = Require<BlueprintFeature>(library,
                OutsiderTypeGuid, "native outsider type");
            BlueprintAbility burning = Require<BlueprintAbility>(library,
                BurningHandsGuid, "native Burning Hands");
            BlueprintAbility stone = Require<BlueprintAbility>(library,
                StoneFistGuid, "native Stone Fist");
            BlueprintAbility feather = Require<BlueprintAbility>(library,
                FeatherStepGuid, "native Feather Step");
            BlueprintAbility hold = Require<BlueprintAbility>(library,
                HoldPersonGuid, "native Hold Person");
            BlueprintAbility enlarge = Require<BlueprintAbility>(library,
                EnlargePersonGuid, "native Enlarge Person");
            BlueprintAbility reduce = Require<BlueprintAbility>(library,
                ReducePersonGuid, "native Reduce Person");

            BlueprintScriptableObject[] donors =
            {
                keen, slow, outsider, burning, stone, feather, hold,
                enlarge, reduce
            };
            JArray donorEvidence = (JArray)evidence["mechanicDonors"];
            foreach (BlueprintScriptableObject donor in donors)
                donorEvidence.Add(DescribeBlueprint(donor));

            BlueprintFeatureSelection aasimar = (races[1].Features ??
                new BlueprintFeature[0]).OfType<BlueprintFeatureSelection>()
                .Single();
            BlueprintFeatureSelection tiefling = (races[2].Features ??
                new BlueprintFeature[0]).OfType<BlueprintFeatureSelection>()
                .Single();
            BlueprintFeature[] aasimarHeritages = aasimar.AllFeatures ??
                new BlueprintFeature[0];
            BlueprintFeature[] tieflingHeritages = tiefling.AllFeatures ??
                new BlueprintFeature[0];
            evidence["outsiderPrecedent"] = new JObject
            {
                { "aasimarSelection", DescribeBlueprint(aasimar) },
                { "aasimarHeritages", new JArray(aasimarHeritages
                    .Select(DescribeBlueprint)) },
                { "tieflingSelection", DescribeBlueprint(tiefling) },
                { "tieflingHeritages", new JArray(tieflingHeritages
                    .Select(DescribeBlueprint)) },
                { "outsiderType", DescribeBlueprint(outsider) }
            };

            bool distinct = donors.All(value => value != null) &&
                donors.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() == donors.Length;
            bool exactRaceFeatures = (races[3].Features ??
                    new BlueprintFeature[0]).Any(value =>
                        ReferenceEquals(value, keen)) &&
                (races[4].Features ?? new BlueprintFeature[0]).Any(value =>
                    ReferenceEquals(value, keen)) &&
                (races[4].Features ?? new BlueprintFeature[0]).Any(value =>
                    ReferenceEquals(value, slow));
            bool nativeSpellTypes = burning.Type == AbilityType.Spell &&
                stone.Type == AbilityType.Spell &&
                feather.Type == AbilityType.Spell;
            Add(assertions, "elemental-probe-mechanic-donors",
                "nine exact, distinct native mechanic donors with race links and spell contracts",
                "donors=" + donors.Length + ";keen=" + exactRaceFeatures +
                    ";spells=" + nativeSpellTypes +
                    ";aasimarHeritages=" + aasimarHeritages.Length +
                    ";tieflingHeritages=" + tieflingHeritages.Length,
                distinct && exactRaceFeatures && nativeSpellTypes &&
                    aasimarHeritages.Length > 0 &&
                    tieflingHeritages.Length > 0,
                "live exact-GUID native blueprint and heritage inventory");
        }

        private static T Require<T>(LibraryScriptableObject library,
            string guid, string purpose) where T : BlueprintScriptableObject
        {
            return BlueprintLibraryLookup.RequireExact<T>(library, guid,
                "elemental-race development probe " + purpose);
        }

        private static JObject DescribeBlueprint(
            BlueprintScriptableObject blueprint)
        {
            BlueprintUnitFact fact = blueprint as BlueprintUnitFact;
            string displayName = string.Empty;
            string description = string.Empty;
            if (fact != null)
            {
                try { displayName = fact.Name ?? string.Empty; }
                catch { displayName = "<error>"; }
                try { description = fact.Description ?? string.Empty; }
                catch { description = "<error>"; }
            }
            return new JObject
            {
                { "name", blueprint.name ?? string.Empty },
                { "guid", blueprint.AssetGuid ?? string.Empty },
                { "type", blueprint.GetType().FullName },
                { "displayName", displayName },
                { "description", description },
                { "components", new JArray((blueprint.ComponentsArray ??
                    new BlueprintComponent[0]).Select(value =>
                        value == null ? (JToken)JValue.CreateNull() :
                        new JObject
                        {
                            { "type", value.GetType().FullName },
                            { "fields", DescribeFields(value) }
                        })) }
            };
        }

        private static JObject DescribeFields(object value)
        {
            var result = new JObject();
            if (value == null)
            {
                result["missing"] = true;
                return result;
            }
            foreach (FieldInfo field in value.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic).OrderBy(item => item.Name,
                    StringComparer.Ordinal))
            {
                object member;
                try { member = field.GetValue(value); }
                catch (Exception exception)
                {
                    result[field.Name] = "<error:" +
                        exception.GetType().Name + ">";
                    continue;
                }
                var sequence = member as IEnumerable;
                if (member == null) result[field.Name] = JValue.CreateNull();
                else if (member is string || sequence == null)
                    result[field.Name] = DescribeUnity(member);
                else
                {
                    var items = new JArray();
                    foreach (object item in sequence)
                        items.Add(DescribeUnity(item));
                    result[field.Name] = items;
                }
            }
            return result;
        }

        private static string DescribeMember(object target, string name)
        {
            Type type = target.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            object value = field == null ? null : field.GetValue(target);
            if (field == null)
            {
                PropertyInfo property = type.GetProperty(name,
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
                value = property == null ? null :
                    property.GetValue(target, null);
            }
            if (value == null) return "<null>";
            PropertyInfo wrapped = value.GetType().GetProperty("Value",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            return wrapped == null ? value.ToString() :
                Convert.ToString(wrapped.GetValue(value, null),
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void SetMember(object target, string name,
            object value)
        {
            Type type = target.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }
            PropertyInfo property = type.GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            if (property == null || !property.CanWrite)
                throw new MissingMemberException(type.FullName, name);
            property.SetValue(target, value, null);
        }

        private static DollData CreateDoll(BlueprintRace race, Gender gender,
            BlueprintCharacterClass characterClass, out DollState state)
        {
            BlueprintRaceVisualPreset preset = race.Presets == null ||
                race.Presets.Length == 0 ? null : race.Presets[0];
            if (preset == null || preset.Skin == null ||
                (gender == Gender.Male ? preset.MaleSkeleton == null :
                    preset.FemaleSkeleton == null))
                throw new InvalidOperationException(gender +
                    " diagnostic race preset is incomplete.");
            state = new DollState();
            state.SetGender(gender);
            state.SetRace(race);
            state.SetRacePreset(preset);
            state.SetClass(characterClass);
            if (state.GetSkinRamps().Count > 0) state.SetSkinColor(0);
            if (state.GetHairRamps().Count > 0) state.SetHairColor(0);
            if (state.GetHornsRamps().Count > 0) state.SetHornsColor(0);
            DollData result = state.CreateData();
            if (result == null || result.Gender != gender ||
                !ReferenceEquals(result.RacePreset, preset) ||
                result.EquipmentEntityIds == null ||
                result.EquipmentEntityIds.Count == 0)
                throw new InvalidOperationException(gender +
                    " diagnostic DollData is incomplete.");
            foreach (string id in result.EquipmentEntityIds)
                if (ResourcesLibrary.TryGetResource<EquipmentEntity>(id,
                    true) == null)
                    throw new InvalidOperationException(gender +
                        " doll entity did not resolve: " + id);
            return result;
        }

        private static UnitEntityView CreateView(string label, DollData data)
        {
            UnitEntityView view = data.CreateUnitView(false);
            if (view == null || view.GetComponent<Character>() == null)
                throw new InvalidOperationException(label +
                    " DollData did not create a Character view.");
            return view;
        }

        private static bool ViewReady(UnitEntityView view)
        {
            if (view == null || view.GetComponent<Character>() == null)
                return false;
            return view.GetComponentsInChildren<Renderer>(true)
                .Any(RendererHasCompleteMaterial);
        }

        private static bool RendererHasCompleteMaterial(Renderer renderer)
        {
            return renderer != null && renderer.enabled &&
                renderer.sharedMaterials != null &&
                renderer.sharedMaterials.Length > 0 &&
                renderer.sharedMaterials.All(value => value != null &&
                    value.shader != null);
        }

        private static JObject DescribeView(string label, DollData data,
            UnitEntityView view, bool ready, int settleUpdates)
        {
            Character avatar = view == null ? null : view.CharacterAvatar;
            Renderer[] renderers = view == null ? new Renderer[0] :
                view.GetComponentsInChildren<Renderer>(true);
            Material[] materials = renderers.Where(value => value != null)
                .SelectMany(value => value.sharedMaterials ??
                    new Material[0]).ToArray();
            return new JObject
            {
                { "label", label },
                { "racePreset", data.RacePreset.AssetGuid },
                { "entityIds", new JArray(
                    data.EquipmentEntityIds.ToArray()) },
                { "characterRoot", view != null &&
                    view.GetComponent<Character>() != null },
                { "avatarEntities", avatar == null ||
                    avatar.EquipmentEntities == null ? 0 :
                    avatar.EquipmentEntities.Count },
                { "renderers", renderers.Length },
                { "renderableRenderers", renderers.Count(
                    RendererHasCompleteMaterial) },
                { "rendererNames", new JArray(renderers.Where(value =>
                    value != null).Select(value => value.name).OrderBy(
                        value => value, StringComparer.Ordinal)) },
                { "materials", materials.Length },
                { "nullMaterials", materials.Count(value => value == null) },
                { "nullShaders", materials.Count(value => value != null &&
                    value.shader == null) },
                { "settleUpdates", settleUpdates },
                { "ready", ready }
            };
        }

        private static bool DollEquivalent(DollData expected,
            DollData observed)
        {
            return expected != null && observed != null &&
                expected.Gender == observed.Gender &&
                ReferenceEquals(expected.RacePreset, observed.RacePreset) &&
                expected.EquipmentEntityIds.SequenceEqual(
                    observed.EquipmentEntityIds, StringComparer.Ordinal);
        }

        private static void DestroyView(UnitEntityView view)
        {
            if (view != null && view.gameObject != null)
                UnityEngine.Object.DestroyImmediate(view.gameObject);
        }

        private static bool IsElementalName(BlueprintRace race)
        {
            string text = (race.name ?? string.Empty) + "|" +
                SafeName(race);
            return new[] { "Ifrit", "Oread", "Sylph", "Undine" }
                .Any(value => text.IndexOf(value,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsRegisteredExactly(
            ElementalRaceBlueprints value)
        {
            return value != null &&
                ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                    BlueprintRace>(value.Race.AssetGuid), value.Race) &&
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
                    BlueprintAbilityResource>(value.SlaResource.AssetGuid),
                    value.SlaResource) &&
                ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(value.SlaAbility.AssetGuid),
                    value.SlaAbility);
        }

        private static string SafeName(BlueprintRace race)
        {
            try { return race.Name ?? string.Empty; }
            catch { return string.Empty; }
        }

        private static string Identity(BlueprintRace race)
        {
            return race == null ? "<null>" : (race.name ?? string.Empty) +
                "/" + race.AssetGuid + "/" + race.RaceId + "/" +
                SafeName(race);
        }

        private static string DescribeUnity(object value)
        {
            if (value == null) return "<null>";
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return blueprint.name + "/" +
                blueprint.AssetGuid + "/" + blueprint.GetType().FullName;
            var unity = value as UnityEngine.Object;
            return unity == null ? value.ToString() : unity.name + "/" +
                unity.GetType().FullName;
        }

        private static string[] RaceIdentities(
            IEnumerable<BlueprintRace> races)
        {
            return (races ?? Enumerable.Empty<BlueprintRace>())
                .Select(Identity).ToArray();
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

        private sealed class ProbeRegistration : IDisposable
        {
            private readonly LibraryScriptableObject _library;
            private readonly BlueprintRace _blueprint;
            private readonly ICollection<BlueprintScriptableObject> _all;
            private readonly string _guid;
            private bool _disposed;

            internal ProbeRegistration(LibraryScriptableObject library,
                BlueprintRace blueprint, string guid)
            {
                _library = library;
                _blueprint = blueprint;
                _guid = guid;
                _all = library.GetAllBlueprints();
                BlueprintScriptableObject collision;
                if (library.BlueprintsByAssetId.TryGetValue(guid,
                    out collision))
                    throw new InvalidOperationException(
                        "Reserved diagnostic GUID collision: " + guid +
                        ";existing=" + DescribeUnity(collision));
                FieldInfo field = typeof(BlueprintScriptableObject).GetField(
                    "m_AssetGuid", BindingFlags.Instance |
                    BindingFlags.NonPublic);
                if (field == null || field.FieldType != typeof(string))
                    throw new MissingFieldException(
                        typeof(BlueprintScriptableObject).FullName,
                        "m_AssetGuid");
                field.SetValue(blueprint, guid);
                bool listAdded = false;
                try
                {
                    _all.Add(blueprint);
                    listAdded = true;
                    library.BlueprintsByAssetId.Add(guid, blueprint);
                    Owned = true;
                }
                catch
                {
                    if (Owned) library.BlueprintsByAssetId.Remove(guid);
                    if (listAdded) _all.Remove(blueprint);
                    throw;
                }
            }

            internal bool Owned { get; private set; }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                BlueprintScriptableObject current;
                if (Owned && _library.BlueprintsByAssetId.TryGetValue(
                    _guid, out current) && ReferenceEquals(current,
                        _blueprint))
                    _library.BlueprintsByAssetId.Remove(_guid);
                if (Owned) _all.Remove(_blueprint);
                Owned = false;
            }
        }
    }
}
