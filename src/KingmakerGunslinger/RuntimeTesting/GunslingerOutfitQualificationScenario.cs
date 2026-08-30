using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Finalist-only race/gender qualification for the audited native Magus
    /// clothing pair. Every body and avatar mutation is request-local.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string MagusClassGuid =
            "45a4607686d96a1498891b3286121780";

        internal static FinalistRaceMatrixSession BeginFinalistRaceMatrix(
            ModContext context, RuntimeTestRequest request)
        {
            return new FinalistRaceMatrixSession(context, request);
        }

        private sealed class RaceFixtureSpec
        {
            internal RaceFixtureSpec(BlueprintRace race, Gender gender,
                BlueprintUnit[] sources)
            {
                Race = race;
                Gender = gender;
                Sources = sources ?? new BlueprintUnit[0];
                Label = gender.ToString().ToLowerInvariant() + "-" +
                    race.RaceId.ToString().ToLowerInvariant();
            }

            internal readonly string Label;
            internal readonly BlueprintRace Race;
            internal readonly Gender Gender;
            internal readonly BlueprintUnit[] Sources;
            internal int DonorIndex;

            internal BlueprintUnit Source
            {
                get { return Sources[DonorIndex]; }
            }

            internal int DonorCount
            {
                get { return Sources.Length; }
            }

            internal bool TryAdvanceDonor()
            {
                if (DonorIndex + 1 >= Sources.Length) return false;
                DonorIndex++;
                return true;
            }
        }

        internal sealed class FinalistRaceMatrixSession
        {
            private const int MaximumSettleUpdates = 360;
            private const int MinimumSettleUpdates = 30;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics =
                new List<string>();
            private readonly List<string> _warnings =
                new List<string>();
            private readonly List<string> _evidenceFiles =
                new List<string>();
            private readonly JArray _records = new JArray();
            private readonly JArray _fixtureRecords = new JArray();
            private readonly JArray _nativeLinkRecords = new JArray();
            private readonly JArray _donorRejections = new JArray();
            private readonly JArray _restorationRecords = new JArray();
            private object _allUnits;
            private object _party;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private UnitEntityData _anchor;
            private BlueprintRace[] _races = new BlueprintRace[0];
            private RaceFixtureSpec[] _fixtures =
                new RaceFixtureSpec[0];
            private CandidateSpec _finalist;
            private UnitEntityData _actor;
            private BlueprintUnit _actorBlueprint;
            private Character _avatar;
            private AvatarEntityState[] _avatarBefore =
                new AvatarEntityState[0];
            private string[] _savedLinksBefore = new string[0];
            private EquipmentEntity[] _classEntities =
                new EquipmentEntity[0];
            private EquipmentEntity[] _candidateEntities =
                new EquipmentEntity[0];
            private JArray _paletteEvidence = new JArray();
            private JObject _lastRestorationDiagnostic = new JObject();
            private int _fixtureIndex;
            private int _paletteIndex;
            private int _phase;
            private int _settleUpdates;
            private int _resolvedEntities;
            private int _paletteApplications;
            private int _restorations;
            private int _captured;
            private int _imageCount;
            private int _viewCount;
            private bool _fixtureInitialized;
            private bool _currentRestored;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private string _stage = "resolve-working-save-anchor";
            private string _exceptionSummary = string.Empty;
            private string _assemblySha256 = string.Empty;
            private string _assemblyMvid = string.Empty;

            internal FinalistRaceMatrixSession(ModContext context,
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
                    if (_phase == 0)
                    {
                        Initialize();
                        _phase = 1;
                        return;
                    }
                    if (_phase == 1)
                    {
                        if (!SpawnFixture()) return;
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
                        ApplyFinalist();
                        _phase = 4;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 4)
                    {
                        PollOutfitReadiness();
                        return;
                    }
                    if (_phase == 5)
                    {
                        CaptureCurrentPalette();
                        _phase = 6;
                        return;
                    }
                    Advance();
                }
                catch (Exception exception)
                {
                    _exceptionSummary = "stage=" + _stage + ";" + exception;
                    Add(_assertions,
                        "gunslinger-outfit-finalist-race-matrix-exception",
                        "no exception", _exceptionSummary, false,
                        "guarded disposable native-body qualification");
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
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

                ValidateCandidateCatalog();
                _finalist = Candidates.Single(value => string.Equals(
                    value.Label, "magus-complete",
                    StringComparison.Ordinal));
                BlueprintRoot root = BlueprintRoot.Instance;
                _races = root == null || root.Progression == null ||
                    root.Progression.CharacterRaces == null
                    ? new BlueprintRace[0]
                    : root.Progression.CharacterRaces
                        .Where(value => value != null)
                        .GroupBy(value => value.RaceId)
                        .Select(group => group.OrderBy(value =>
                            value.AssetGuid, StringComparer.Ordinal).First())
                        .OrderBy(value => value.RaceId.ToString(),
                            StringComparer.Ordinal).ToArray();
                if (_races.Length == 0)
                    throw new InvalidOperationException(
                        "The installed game exposed no supported player races.");

                BlueprintUnit[] donors = ResourcesLibrary
                    .GetBlueprints<BlueprintUnit>()
                    .Where(IsQualificationBodyDonor).ToArray();
                var fixtures = new List<RaceFixtureSpec>();
                foreach (BlueprintRace race in _races)
                    foreach (Gender gender in new[]
                    {
                        Gender.Male, Gender.Female
                    })
                    {
                        Size expectedSize =
                            ExpectedPlayerRaceSize(race.RaceId);
                        BlueprintUnit[] matches = donors.Where(value =>
                                value.Gender == gender &&
                                value.Race.RaceId == race.RaceId &&
                                value.Size == expectedSize)
                            .OrderBy(value => DonorPriority(value, race))
                            .ThenBy(value => value.name ?? string.Empty,
                                StringComparer.Ordinal)
                            .ThenBy(value => value.AssetGuid,
                                StringComparer.Ordinal).ToArray();
                        if (matches.Length == 0)
                            throw new InvalidOperationException(
                                "No native body donor exists for " +
                                gender + " " + race.RaceId + ".");
                        fixtures.Add(new RaceFixtureSpec(race, gender,
                            matches));
                    }
                _fixtures = fixtures.ToArray();
                ValidateFinalistNativeLinks();

                var gameAssembly = typeof(BlueprintCharacterClass).Assembly;
                _assemblySha256 = HashFile(gameAssembly.Location)
                    .ToLowerInvariant();
                _assemblyMvid = gameAssembly.ManifestModule.ModuleVersionId
                    .ToString("D");
                _diagnostics.Add("shortlistCandidateSetId=" +
                    CandidateSetId());
                _diagnostics.Add("supportedRaceIds=" + string.Join(",",
                    _races.Select(value => value.RaceId.ToString())
                        .ToArray()));
                _diagnostics.Add("fixtureDonors=" + string.Join("|",
                    _fixtures.Select(value => value.Label + "=" +
                        DescribeQualificationBlueprint(value.Source) +
                        ";candidateCount=" + value.DonorCount).ToArray()));
                WriteProgress("initialized");
            }

            private void ValidateFinalistNativeLinks()
            {
                BlueprintCharacterClass magus = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, MagusClassGuid,
                        "gunslinger-outfit-finalist-native-magus-class");
                if (magus.PrimaryColor != _finalist.Primary ||
                    magus.SecondaryColor != _finalist.Secondary)
                    throw new InvalidOperationException(
                        "The native Magus color defaults no longer match the " +
                        "audited finalist defaults.");
                foreach (RaceFixtureSpec fixture in _fixtures)
                {
                    EquipmentEntity[] expected = _finalist
                        .For(fixture.Gender).Select(id =>
                        {
                            EquipmentEntity entity = ResourcesLibrary
                                .TryGetResource<EquipmentEntity>(id, true);
                            if (entity == null)
                                throw new InvalidOperationException(
                                    "The finalist entity did not resolve: " +
                                    id + ".");
                            return entity;
                        }).ToArray();
                    EquipmentEntity[] observed = magus.LoadClothes(
                            fixture.Gender, fixture.Race)
                        .Where(value => value != null).ToArray();
                    bool exact = expected.Length == observed.Length &&
                        expected.Select((value, index) =>
                            ReferenceEquals(value, observed[index]))
                            .All(value => value);
                    _nativeLinkRecords.Add(new JObject
                    {
                        { "fixture", fixture.Label },
                        { "raceGuid", fixture.Race.AssetGuid },
                        { "raceId", fixture.Race.RaceId.ToString() },
                        { "gender", fixture.Gender.ToString() },
                        { "assetIds",
                            new JArray(_finalist.For(fixture.Gender)) },
                        { "loadedEntityNames",
                            new JArray(observed.Select(value =>
                                value.name).ToArray()) },
                        { "orderedPairExact", exact }
                    });
                    if (!exact)
                        throw new InvalidOperationException(
                            fixture.Label + " did not resolve the exact " +
                            "ordered native finalist pair through " +
                            "BlueprintCharacterClass.LoadClothes.");
                }
            }

            private static bool IsQualificationBodyDonor(
                BlueprintUnit value)
            {
                return value != null && value.Prefab != null &&
                    value.Race != null && value.Body != null &&
                    !value.Body.DisableHands;
            }

            private static int DonorPriority(BlueprintUnit value,
                BlueprintRace race)
            {
                string name = value == null ? string.Empty :
                    value.name ?? string.Empty;
                if (name.StartsWith("StartGamePregen",
                        StringComparison.OrdinalIgnoreCase)) return 0;
                if (name.IndexOf("Companion",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 1;
                if (name.IndexOf(race.RaceId.ToString(),
                        StringComparison.OrdinalIgnoreCase) >= 0) return 2;
                if (name.IndexOf("Player",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 3;
                if (name.IndexOf("NPC",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 4;
                return 5;
            }

            private static Size ExpectedPlayerRaceSize(Race race)
            {
                return race == Race.Gnome || race == Race.Halfling
                    ? Size.Small : Size.Medium;
            }

            private static string DescribeQualificationBlueprint(
                BlueprintUnit value)
            {
                return value == null ? "<null>" : value.name + "/" +
                    value.AssetGuid + "/" + value.Gender + "/" +
                    (value.Race == null ? "<no-race>" :
                        value.Race.RaceId.ToString()) + "/" + value.Size;
            }

            private bool SpawnFixture()
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "spawn-" + fixture.Label;
                if (_actorBlueprint == null)
                {
                    _actorBlueprint = UnityEngine.Object.Instantiate(
                        fixture.Source);
                    _actorBlueprint.Race = fixture.Race;
                    _actorBlueprint.name =
                        "KMG_Runtime_Gunslinger_Outfit_Finalist_" +
                        fixture.Label.Replace('-', '_');
                    _actorBlueprint.IsCheater = true;
                }
                Game.Instance.EntityCreator.Tick();
                var prefab = fixture.Source.Prefab.Load(false);
                _settleUpdates++;
                if (prefab == null)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return false;
                    RejectCurrentDonor("prefab-load-timeout",
                        new JObject
                        {
                            { "settleUpdates", _settleUpdates }
                        });
                    return false;
                }
                _actor = Game.Instance.EntityCreator.SpawnUnit(
                    _actorBlueprint, prefab,
                    NearestNavigable(_anchor.Position +
                        new Vector3(-3.5f, 0f, 3.5f)),
                    Quaternion.identity, _anchor.HoldingState);
                if (_actor == null)
                    throw new InvalidOperationException(
                        fixture.Label + " disposable actor did not spawn.");
                _fixtureInitialized = false;
                _currentRestored = false;
                WriteProgress("spawned");
                return true;
            }

            private void PollFixtureReadiness()
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "settle-" + fixture.Label;
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
                    RejectCurrentDonor("incomplete-native-avatar",
                        new JObject
                        {
                            { "settleUpdates", _settleUpdates }
                        });
                    return;
                }
                if (!_fixtureInitialized)
                {
                    _actor.Descriptor.State.Immortality.Retain();
                    _actor.Commands.InterruptAll(true);
                    if (_actor.CombatState.IsInCombat)
                        _actor.CombatState.LeaveCombat();
                    ClearHand(_actor, true);
                    ClearHand(_actor, false);
                    if (_actor.Body.Armor.HasArmor)
                        _actor.Body.Armor.RemoveItem(false);
                    if (_actor.Body.Shoulders.MaybeItem != null)
                        _actor.Body.Shoulders.RemoveItem(false);
                    _actor.View.HandsEquipment.UpdateAll();
                    _actor.View.HandsEquipment.ForceSwitch(false);
                    _fixtureInitialized = true;
                    _settleUpdates = 0;
                    return;
                }
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                Renderer[] renderers = ActiveRenderers(_actor);
                bool exact = _actor.Gender == fixture.Gender &&
                    _actor.Descriptor.Progression.Race.RaceId ==
                        fixture.Race.RaceId &&
                    _actor.Descriptor.State.Size == fixture.Source.Size &&
                    HasExactHumanoidRig(_actor.View.transform) &&
                    renderers.Length > 0;
                if (_settleUpdates < MinimumSettleUpdates || !exact)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    RejectCurrentDonor("native-body-contract-not-exact",
                        new JObject
                        {
                            { "settleUpdates", _settleUpdates },
                            { "actualGender", _actor.Gender.ToString() },
                            { "actualRaceId",
                                _actor.Descriptor.Progression.Race.RaceId
                                    .ToString() },
                            { "actualSize",
                                _actor.Descriptor.State.Size.ToString() },
                            { "expectedSize",
                                ExpectedPlayerRaceSize(
                                    fixture.Race.RaceId).ToString() },
                            { "rigExact",
                                HasExactHumanoidRig(
                                    _actor.View.transform) },
                            { "rendererCount", renderers.Length }
                        });
                    return;
                }

                _avatar = _actor.View.CharacterAvatar;
                _avatarBefore = _avatar.EquipmentEntities
                    .Where(value => value != null)
                    .Select(value => new AvatarEntityState
                    {
                        Entity = value,
                        Primary = _avatar.GetPrimaryRampIndex(value),
                        Secondary = _avatar.GetSecondaryRampIndex(value)
                    }).ToArray();
                _savedLinksBefore = QualificationSavedLinks(_avatar);
                _classEntities = LoadPresentClassClothes(
                    fixture.Gender, fixture.Race);
                if (!RestoreAvatar())
                {
                    RejectCurrentDonor(
                        "avatar-roundtrip-restoration-not-exact",
                        _lastRestorationDiagnostic);
                    return;
                }
                _currentRestored = true;
                _fixtureRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "sourceName", fixture.Source.name },
                    { "sourceGuid", fixture.Source.AssetGuid },
                    { "donorCandidateCount", fixture.DonorCount },
                    { "donorAttemptIndex", fixture.DonorIndex },
                    { "gender", _actor.Gender.ToString() },
                    { "raceName",
                        _actor.Descriptor.Progression.Race.name },
                    { "raceGuid",
                        _actor.Descriptor.Progression.Race.AssetGuid },
                    { "raceId",
                        _actor.Descriptor.Progression.Race.RaceId.ToString() },
                    { "size", _actor.Descriptor.State.Size.ToString() },
                    { "expectedSize",
                        ExpectedPlayerRaceSize(
                            fixture.Race.RaceId).ToString() },
                    { "originalEntityCount", _avatarBefore.Length },
                    { "presentClassEntityCount", _classEntities.Length },
                    { "initialRoundTripRestored", true },
                    { "initialRoundTripDiagnostic",
                        _lastRestorationDiagnostic.DeepClone() },
                    { "featureNodes",
                        new JArray(QualificationFeatureNodes(_actor)) },
                    { "rendererCount", renderers.Length },
                    { "rigExact", true }
                });
                _phase = 3;
                _settleUpdates = 0;
                WriteProgress("fixture-ready");
            }

            private EquipmentEntity[] LoadPresentClassClothes(
                Gender gender, BlueprintRace race)
            {
                BlueprintRoot root = BlueprintRoot.Instance;
                if (root == null || root.Progression == null ||
                    root.Progression.CharacterClasses == null)
                    throw new InvalidOperationException(
                        "Installed class catalog is unavailable.");
                var all = new List<EquipmentEntity>();
                foreach (BlueprintCharacterClass characterClass in
                    root.Progression.CharacterClasses.Where(value =>
                        value != null))
                    all.AddRange(characterClass.LoadClothes(gender, race)
                        .Where(value => value != null));
                return all.Distinct().Where(value =>
                    _avatarBefore.Any(original =>
                        ReferenceEquals(original.Entity, value))).ToArray();
            }

            private void RejectCurrentDonor(string reason, JObject detail)
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                BlueprintUnit rejected = fixture.Source;
                var record = new JObject
                {
                    { "fixture", fixture.Label },
                    { "donorAttemptIndex", fixture.DonorIndex },
                    { "donorCandidateCount", fixture.DonorCount },
                    { "sourceName", rejected.name },
                    { "sourceGuid", rejected.AssetGuid },
                    { "sourceGender", rejected.Gender.ToString() },
                    { "sourceRaceId", rejected.Race.RaceId.ToString() },
                    { "sourceSize", rejected.Size.ToString() },
                    { "reason", reason },
                    { "detail", detail == null ? new JObject() :
                        detail.DeepClone() }
                };
                _donorRejections.Add(record);
                _diagnostics.Add("donorRejected=" +
                    record.ToString(Newtonsoft.Json.Formatting.None));

                // A failed round trip affects only this disposable actor. Skip
                // restoration, dispose the actor, and try the next exact native
                // donor rather than weakening the restoration contract.
                _currentRestored = true;
                RetireActor();
                if (!fixture.TryAdvanceDonor())
                    throw new InvalidOperationException(fixture.Label +
                        " exhausted " + fixture.DonorCount +
                        " deterministic native body donors; lastReason=" +
                        reason + ".");
                _phase = 1;
                _settleUpdates = 0;
                WriteProgress("donor-rejected");
            }

            private void ApplyFinalist()
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "apply-finalist-" + fixture.Label;
                if (!_currentRestored)
                    throw new InvalidOperationException(
                        fixture.Label + " original avatar state was not " +
                        "exact before finalist application.");
                _currentRestored = false;
                _avatar.RemoveEquipmentEntities(_classEntities, false);
                _candidateEntities = _finalist.For(fixture.Gender)
                    .Select(id =>
                    {
                        EquipmentEntity entity = ResourcesLibrary
                            .TryGetResource<EquipmentEntity>(id, true);
                        if (entity == null)
                            throw new InvalidOperationException(
                                "Finalist entity did not resolve: " + id + ".");
                        _resolvedEntities++;
                        return entity;
                    }).ToArray();
                _avatar.AddEquipmentEntities(_candidateEntities, false);
                ApplyQualificationPalette(0);
                _avatar.RebuildOutfit();
                _paletteIndex = 0;
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                WriteProgress("finalist-applied");
            }

            private void ApplyQualificationPalette(int paletteIndex)
            {
                var evidence = new JArray();
                int colorized = 0;
                foreach (EquipmentEntity entity in _candidateEntities)
                {
                    int primaryCount = entity.PrimaryRamps == null ? 0 :
                        entity.PrimaryRamps.Count;
                    int secondaryCount = entity.SecondaryRamps == null ? 0 :
                        entity.SecondaryRamps.Count;
                    int primary = primaryCount == 0 ? -1 :
                        paletteIndex == 0 ? _finalist.Primary :
                        (_finalist.Primary + 11) % primaryCount;
                    int secondary = secondaryCount == 0 ? -1 :
                        paletteIndex == 0 ? _finalist.Secondary :
                        (_finalist.Secondary + 17) % secondaryCount;
                    if (primary >= primaryCount ||
                        secondary >= secondaryCount)
                        throw new InvalidOperationException(
                            "Finalist requested an invalid color ramp.");
                    if (primary >= 0 && secondary >= 0)
                        _avatar.SetRampIndices(entity, primary, secondary,
                            false);
                    else if (primary >= 0)
                        _avatar.SetPrimaryRampIndex(entity, primary, false);
                    else if (secondary >= 0)
                        _avatar.SetSecondaryRampIndex(entity, secondary,
                            false);
                    if (primary >= 0 || secondary >= 0) colorized++;
                    evidence.Add(new JObject
                    {
                        { "entityName", entity.name },
                        { "layer", entity.Layer },
                        { "hideBodyParts",
                            entity.HideBodyParts.ToString() },
                        { "primaryRampCount", primaryCount },
                        { "secondaryRampCount", secondaryCount },
                        { "appliedPrimary", primary },
                        { "appliedSecondary", secondary }
                    });
                }
                if (colorized == 0)
                    throw new InvalidOperationException(
                        "Finalist has no colorized native entity.");
                _paletteEvidence = evidence;
                _paletteApplications++;
            }

            private void PollOutfitReadiness()
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "settle-finalist-" + fixture.Label + "-palette-" +
                    _paletteIndex;
                Game.Instance.EntityCreator.Tick();
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                _actor.View.HandsEquipment.UpdateAll();
                _settleUpdates++;
                EquipmentEntity[] active = _avatar.EquipmentEntities
                    .Where(value => value != null).ToArray();
                bool allCandidate = _candidateEntities.All(value =>
                    active.Any(current => ReferenceEquals(current, value)));
                bool staleClass = _classEntities.Any(value =>
                    !_candidateEntities.Any(candidate =>
                        ReferenceEquals(candidate, value)) &&
                    active.Any(current => ReferenceEquals(current, value)));
                bool savedLinksExact = _savedLinksBefore.SequenceEqual(
                    QualificationSavedLinks(_avatar),
                    StringComparer.Ordinal);
                bool noWeapon = _actor.View.HandsEquipment
                    .GetWeaponModel(false) == null &&
                    !_actor.View.HandsEquipment.InCombat;
                if (_settleUpdates < MinimumSettleUpdates ||
                    !allCandidate || staleClass || !savedLinksExact ||
                    !noWeapon || ActiveRenderers(_actor).Length == 0)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(_stage +
                        " did not settle without stale class clothing, saved " +
                        "link mutation, or a visible weapon.");
                }
                _phase = 5;
                _settleUpdates = 0;
            }

            private void CaptureCurrentPalette()
            {
                RaceFixtureSpec fixture = _fixtures[_fixtureIndex];
                string palette = _paletteIndex == 0
                    ? "native-default" : "audit-alternate";
                _stage = "capture-" + fixture.Label + "-" + palette;
                Renderer[] renderers = ActiveRenderers(_actor);
                string stem = SafeFileName("finalist-" + fixture.Label +
                    "-" + palette + "-no-weapon");
                string previewPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-preview.png");
                string isometricPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-isometric.png");
                WeaponPresentationEvidenceScenario.CaptureSummary preview =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        _actor, null, renderers, previewPath, true);
                IsometricCapture isometric = CaptureIsometric(
                    _actor, renderers, isometricPath);
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "shortlistCandidateSetId", CandidateSetId() },
                    { "candidateId", _finalist.Label },
                    { "assetIds",
                        new JArray(_finalist.For(fixture.Gender)) },
                    { "fixture", fixture.Label },
                    { "sourceName", fixture.Source.name },
                    { "sourceGuid", fixture.Source.AssetGuid },
                    { "gender", _actor.Gender.ToString() },
                    { "raceName",
                        _actor.Descriptor.Progression.Race.name },
                    { "raceGuid",
                        _actor.Descriptor.Progression.Race.AssetGuid },
                    { "raceId",
                        _actor.Descriptor.Progression.Race.RaceId.ToString() },
                    { "size", _actor.Descriptor.State.Size.ToString() },
                    { "palette", palette },
                    { "paletteEvidence", _paletteEvidence.DeepClone() },
                    { "weaponState", "no-weapon" },
                    { "featureNodes",
                        new JArray(QualificationFeatureNodes(_actor)) },
                    { "activeRendererCount", renderers.Length },
                    { "savedLinksUnchanged", true },
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
                            { "views", new JArray("front", "right-side",
                                "rear", "front-right-three-quarter") }
                        }
                    },
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
                            { "view", "elevated-front-right-isometric" }
                        }
                    },
                    { "claimBoundary",
                        "installed-game finalist clothing on a request-local " +
                        "native race/gender body; direct image inspection is " +
                        "required for hair, ear, horn, tail, and aesthetics" }
                };
                string jsonPath = Path.Combine(
                    _request.EvidenceDirectory, stem + ".json");
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(preview.PngPath);
                _evidenceFiles.Add(isometric.Path);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _imageCount += 2;
                _viewCount += 5;
                WriteProgress("captured");
            }

            private static string[] QualificationFeatureNodes(
                UnitEntityData actor)
            {
                if (actor == null || actor.View == null)
                    return new string[0];
                string[] tokens =
                {
                    "hair", "head", "ear", "horn", "tail", "wing",
                    "beard"
                };
                return actor.View.GetComponentsInChildren<Transform>(true)
                    .Where(value => value != null &&
                        tokens.Any(token => value.name.IndexOf(token,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                    .Select(value => value.name)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Take(80).ToArray();
            }

            private void Advance()
            {
                if (_paletteIndex == 0)
                {
                    _paletteIndex = 1;
                    ApplyQualificationPalette(_paletteIndex);
                    _avatar.RebuildOutfit();
                    _phase = 4;
                    _settleUpdates = 0;
                    return;
                }

                bool restored = RestoreAvatar();
                _restorationRecords.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "restored", restored },
                    { "originalEntityCount", _avatarBefore.Length },
                    { "restoredEntityCount",
                        _avatar == null ? 0 :
                            _avatar.EquipmentEntities.Count },
                    { "savedLinksUnchanged", _avatar != null &&
                        _savedLinksBefore.SequenceEqual(
                            QualificationSavedLinks(_avatar),
                            StringComparer.Ordinal) }
                });
                if (!restored)
                    throw new InvalidOperationException(
                        _fixtures[_fixtureIndex].Label +
                        " did not restore exact avatar state.");
                _currentRestored = true;
                _restorations++;
                RetireActor();
                _fixtureIndex++;
                _paletteIndex = 0;
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

            private bool RestoreAvatar()
            {
                if (_avatar == null)
                {
                    _lastRestorationDiagnostic = new JObject
                    {
                        { "avatarPresent", false },
                        { "originalEntityCount", _avatarBefore.Length },
                        { "reason", "missing-avatar" }
                    };
                    return false;
                }
                _avatar.RemoveAllEquipmentEntities(false);
                foreach (AvatarEntityState state in _avatarBefore)
                    _avatar.AddEquipmentEntity(state.Entity, false);
                foreach (AvatarEntityState state in _avatarBefore)
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
                EquipmentEntity[] current = _avatar.EquipmentEntities
                    .Where(value => value != null).ToArray();
                bool exactOrder = current.Length == _avatarBefore.Length &&
                    current.Select((value, index) =>
                        ReferenceEquals(value,
                            _avatarBefore[index].Entity)).All(value => value);
                bool exactRamps = _avatarBefore.All(state =>
                    _avatar.GetPrimaryRampIndex(state.Entity) ==
                        state.Primary &&
                    _avatar.GetSecondaryRampIndex(state.Entity) ==
                        state.Secondary);
                bool savedLinksExact = _savedLinksBefore.SequenceEqual(
                    QualificationSavedLinks(_avatar),
                    StringComparer.Ordinal);
                _lastRestorationDiagnostic = new JObject
                {
                    { "originalEntityCount", _avatarBefore.Length },
                    { "originalEmpty", _avatarBefore.Length == 0 },
                    { "currentEntityCount", current.Length },
                    { "exactOrder", exactOrder },
                    { "exactRamps", exactRamps },
                    { "savedLinksExact", savedLinksExact },
                    { "rampMismatchCount", _avatarBefore.Count(state =>
                        _avatar.GetPrimaryRampIndex(state.Entity) !=
                            state.Primary ||
                        _avatar.GetSecondaryRampIndex(state.Entity) !=
                            state.Secondary) },
                    { "originalEntities", new JArray(_avatarBefore.Select(
                        value => value.Entity.name + "/layer=" +
                            value.Entity.Layer).ToArray()) },
                    { "currentEntities", new JArray(current.Select(value =>
                        value.name + "/layer=" + value.Layer).ToArray()) }
                };
                return exactOrder && exactRamps && savedLinksExact;
            }

            private static string[] QualificationSavedLinks(Character avatar)
            {
                return avatar.SavedEquipmentEntities
                    .Select(value => value == null ? "<null>" :
                        value.AssetId ?? string.Empty).ToArray();
            }

            private void RetireActor()
            {
                if (_actor == null && _actorBlueprint == null) return;
                _stage = "retire-" + _fixtures[_fixtureIndex].Label;
                try
                {
                    if (!_currentRestored && _avatar != null &&
                        !RestoreAvatar())
                        throw new InvalidOperationException(
                            "Fallback avatar restoration was not exact.");
                }
                finally
                {
                    if (_actor != null)
                    {
                        _actor.Commands.InterruptAll(true);
                        if (_actor.CombatState.IsInCombat)
                            _actor.CombatState.LeaveCombat();
                        if (_actor.Descriptor != null)
                            _actor.Descriptor.State.Immortality.ReleaseAll();
                        if (ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        _actor.Dispose();
                    }
                    if (_actorBlueprint != null)
                        UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                    _actor = null;
                    _actorBlueprint = null;
                    _avatar = null;
                    _avatarBefore = new AvatarEntityState[0];
                    _savedLinksBefore = new string[0];
                    _classEntities = new EquipmentEntity[0];
                    _candidateEntities = new EquipmentEntity[0];
                    _lastRestorationDiagnostic = new JObject();
                    _fixtureInitialized = false;
                    _currentRestored = false;
                }
            }

            private void WriteIndex()
            {
                _stage = "write-finalist-race-matrix-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "shortlistCandidateSetId", CandidateSetId() },
                    { "candidate", _finalist.Describe() },
                    { "loadedModVersion", _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _assemblySha256 },
                    { "gameAssemblyMvid", _assemblyMvid },
                    { "supportedRaces", new JArray(_races.Select(value =>
                        new JObject
                        {
                            { "name", value.name },
                            { "guid", value.AssetGuid },
                            { "raceId", value.RaceId.ToString() }
                        }).ToArray()) },
                    { "nativeLinkMatrix", _nativeLinkRecords },
                    { "donorRejections", _donorRejections },
                    { "fixtures", _fixtureRecords },
                    { "restorations", _restorationRecords },
                    { "records", _records },
                    { "saveApiCalled", false },
                    { "productionBlueprintMutated", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-finalist-race-matrix-index.json");
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
                    { "donorIndex", _fixtureIndex < _fixtures.Length
                        ? _fixtures[_fixtureIndex].DonorIndex : -1 },
                    { "paletteIndex", _paletteIndex },
                    { "phase", _phase },
                    { "captured", _captured },
                    { "imageCount", _imageCount },
                    { "actorPresent", _actor != null }
                };
                WriteJsonAtomic(Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-finalist-race-matrix-progress.json"),
                    progress);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "gunslinger-outfit-finalist-race-matrix-cleanup";
                try
                {
                    RetireActor();
                }
                catch (Exception cleanupException)
                {
                    _diagnostics.Add("cleanupException=" + cleanupException);
                    try
                    {
                        if (_actor != null &&
                            ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        if (_actor != null) _actor.Dispose();
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
                    _avatar = null;
                }
                _cleanupStarted = true;
                _settleUpdates = 0;
                WriteProgress("cleanup-started");
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                object[] unitsNow = Snapshot(_allUnits);
                object[] partyNow = Snapshot(_party);
                bool cleaned = SameReferences(_unitsBefore, unitsNow) &&
                    SameReferences(_partyBefore, partyNow) &&
                    _actor == null;
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates) return;
                JObject cleanup = CleanupSnapshotDiagnostic(unitsNow,
                    partyNow);
                _diagnostics.Add("cleanupSnapshot=" + cleanup.ToString(
                    Newtonsoft.Json.Formatting.None));
                Finish(cleaned);
            }

            private JObject CleanupSnapshotDiagnostic(object[] unitsNow,
                object[] partyNow)
            {
                object[] missingUnits = _unitsBefore.Where(expected =>
                    !unitsNow.Any(actual => ReferenceEquals(expected,
                        actual))).ToArray();
                object[] unexpectedUnits = unitsNow.Where(actual =>
                    !_unitsBefore.Any(expected => ReferenceEquals(expected,
                        actual))).ToArray();
                object[] missingParty = _partyBefore.Where(expected =>
                    !partyNow.Any(actual => ReferenceEquals(expected,
                        actual))).ToArray();
                object[] unexpectedParty = partyNow.Where(actual =>
                    !_partyBefore.Any(expected => ReferenceEquals(expected,
                        actual))).ToArray();
                return new JObject
                {
                    { "expectedUnitCount", _unitsBefore.Length },
                    { "actualUnitCount", unitsNow.Length },
                    { "expectedPartyCount", _partyBefore.Length },
                    { "actualPartyCount", partyNow.Length },
                    { "unitsExact", SameReferences(_unitsBefore, unitsNow) },
                    { "partyExact", SameReferences(_partyBefore, partyNow) },
                    { "actorCleared", _actor == null },
                    { "missingUnits", new JArray(missingUnits.Select(
                        DescribeRuntimeReference).ToArray()) },
                    { "unexpectedUnits", new JArray(unexpectedUnits.Select(
                        DescribeRuntimeReference).ToArray()) },
                    { "missingParty", new JArray(missingParty.Select(
                        DescribeRuntimeReference).ToArray()) },
                    { "unexpectedParty", new JArray(unexpectedParty.Select(
                        DescribeRuntimeReference).ToArray()) }
                };
            }

            private static JObject DescribeRuntimeReference(object value)
            {
                var description = new JObject
                {
                    { "runtimeType", value == null ? "<null>" :
                        value.GetType().FullName }
                };
                UnitEntityData unit = value as UnitEntityData;
                if (unit == null) return description;
                try
                {
                    BlueprintUnit blueprint = unit.Blueprint;
                    description["uniqueId"] = unit.UniqueId ?? string.Empty;
                    description["characterName"] =
                        unit.CharacterName ?? string.Empty;
                    description["blueprintName"] = blueprint == null ?
                        string.Empty : blueprint.name ?? string.Empty;
                    description["blueprintGuid"] = blueprint == null ?
                        string.Empty : blueprint.AssetGuid;
                    description["viewPresent"] = unit.View != null;
                    description["descriptorPresent"] =
                        unit.Descriptor != null;
                }
                catch (Exception exception)
                {
                    description["descriptionException"] =
                        exception.GetType().FullName + ":" + exception.Message;
                }
                return description;
            }

            private void Finish(bool cleaned)
            {
                int expectedFixtures = _races.Length * 2;
                int expectedRecords = expectedFixtures * 2;
                int expectedImages = expectedRecords * 2;
                JObject[] records = _records.OfType<JObject>().ToArray();
                string[] expectedCells = _races.SelectMany(race =>
                    new[]
                    {
                        "Male/" + race.RaceId,
                        "Female/" + race.RaceId
                    }).OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                string[] observedCells = _fixtureRecords.OfType<JObject>()
                    .Select(value => (string)value["gender"] + "/" +
                        (string)value["raceId"])
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();

                Add(_assertions,
                    "gunslinger-outfit-finalist-race-matrix-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitFinalistRaceMatrix,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitFinalistRaceMatrix,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-matrix-save-boundary",
                    "KMG_AUTOMATION_WORKING; no save API",
                    "saveName=" + (_request.Parameters == null ? "<null>" :
                        _request.Parameters.Value<string>("saveName")) +
                        ";saveApiCalled=false",
                    _request.Parameters != null &&
                        string.Equals(_request.Parameters.Value<string>(
                            "saveName"), "KMG_AUTOMATION_WORKING",
                            StringComparison.Ordinal),
                    "guarded working-save load plus disposable actors");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-matrix-game-identity",
                    "Kingmaker 2.1.7b exact Assembly-CSharp SHA-256 and MVID",
                    "sha256=" + _assemblySha256 + ";mvid=" +
                        _assemblyMvid,
                    string.Equals(_assemblySha256,
                        ExpectedAssemblySha256, StringComparison.Ordinal) &&
                    string.Equals(_assemblyMvid, ExpectedAssemblyMvid,
                        StringComparison.OrdinalIgnoreCase),
                    "live loaded Assembly-CSharp identity");
                Add(_assertions,
                    "gunslinger-outfit-finalist-dynamic-player-races",
                    "all installed player races, both genders, exact native body",
                    "races=" + _races.Length + ";fixtures=" +
                        _fixtureRecords.Count + ";cells=" +
                        string.Join(",", observedCells),
                    _races.Length == 9 &&
                        _fixtures.Length == expectedFixtures &&
                        _fixtureRecords.Count == expectedFixtures &&
                        expectedCells.SequenceEqual(observedCells,
                            StringComparer.Ordinal) &&
                        _fixtureRecords.OfType<JObject>().All(value =>
                            (bool)value["rigExact"] &&
                            (int)value["rendererCount"] > 0 &&
                            string.Equals((string)value["size"],
                                (string)value["expectedSize"],
                                StringComparison.Ordinal) &&
                            (int)value["donorAttemptIndex"] >= 0 &&
                            (int)value["donorAttemptIndex"] <
                                (int)value["donorCandidateCount"]),
                    "BlueprintRoot progression race discovery plus native BlueprintUnit race/gender donors");
                Add(_assertions,
                    "gunslinger-outfit-finalist-donor-selection",
                    "every rejected disposable donor is recorded; every accepted donor proves an exact avatar round trip",
                    "rejections=" + _donorRejections.Count +
                        ";accepted=" + _fixtureRecords.Count,
                    _fixtureRecords.Count == expectedFixtures &&
                        _fixtureRecords.OfType<JObject>().All(value =>
                            (bool)value["initialRoundTripRestored"] &&
                            (bool)value["initialRoundTripDiagnostic"]
                                ["exactOrder"] &&
                            (bool)value["initialRoundTripDiagnostic"]
                                ["exactRamps"] &&
                            (bool)value["initialRoundTripDiagnostic"]
                                ["savedLinksExact"]) &&
                        _donorRejections.OfType<JObject>().All(value =>
                            !string.IsNullOrEmpty(
                                (string)value["sourceGuid"]) &&
                            !string.IsNullOrEmpty(
                                (string)value["reason"])),
                    "deterministic candidate order, canonical player-race size, and exact remove/re-add restoration probe");
                Add(_assertions,
                    "gunslinger-outfit-finalist-native-links",
                    "exact ordered two-entity Magus pair in every discovered cell",
                    "records=" + _nativeLinkRecords.Count +
                        ";resolved=" + _resolvedEntities,
                    _nativeLinkRecords.Count == expectedFixtures &&
                        _nativeLinkRecords.OfType<JObject>().All(value =>
                            (bool)value["orderedPairExact"]) &&
                        _resolvedEntities == expectedFixtures * 2,
                    "native BlueprintCharacterClass.LoadClothes and exact resource references");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-palettes",
                    "native default and one valid alternate palette per cell",
                    "applications=" + _paletteApplications,
                    _paletteApplications == expectedRecords &&
                        records.All(value => ((JArray)value[
                            "paletteEvidence"]).OfType<JObject>().Any(row =>
                                (int)row["appliedPrimary"] >= 0 ||
                                (int)row["appliedSecondary"] >= 0)),
                    "live ramp counts and saved:false ramp application");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-captures",
                    expectedRecords + " records, " + expectedImages +
                        " PNGs, preview-like and isometric views",
                    "records=" + records.Length + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    records.Length == expectedRecords &&
                        _captured == expectedRecords &&
                        _imageCount == expectedImages &&
                        _viewCount == expectedRecords * 5 &&
                        _indexWritten &&
                        _evidenceFiles.Count == expectedRecords * 3 + 1 &&
                        _evidenceFiles.All(File.Exists) &&
                        records.All(value =>
                            (int)value["preview"]["meaningfulPixels"] > 0 &&
                            (int)value["isometric"][
                                "meaningfulPixels"] > 0),
                    "four-view preview sheets plus elevated isometric captures");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-restoration",
                    "exact entity order, ramps, and saved links restored for every body",
                    "restored=" + _restorations + "/" +
                        expectedFixtures,
                    _restorations == expectedFixtures &&
                        _restorationRecords.Count == expectedFixtures &&
                        _restorationRecords.OfType<JObject>().All(value =>
                            (bool)value["restored"] &&
                            (bool)value["savedLinksUnchanged"]),
                    "Character snapshot and saved:false add/remove/rebuild");
                Add(_assertions,
                    "gunslinger-outfit-finalist-race-cleanup",
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";updates=" +
                        _settleUpdates, cleaned,
                    "request-local actor, blueprint, camera, light, and textures");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Direct inspection of the ignored PNG matrix " +
                    "remains authoritative for hair, ears, horns, tails, " +
                    "clipping, and aesthetic acceptance.");
                _warnings.Add("This gate establishes native race/gender, " +
                    "palette, no-weapon readability, rebuild, and exact " +
                    "restoration. Equipment overlays and live motion are " +
                    "separate finalist gates.");
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
