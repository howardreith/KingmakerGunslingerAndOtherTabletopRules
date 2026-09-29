using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// In-game images of any summon for internal review (Phase 1, Sprint 3
    /// onward). The request names creature keys; each is cast, one at a time,
    /// through its registered execution into the loaded working save, held across
    /// frames while the Phase 0 party-camera review renders it idle, moving
    /// and attacking, then dismissed through the same cleanup the synchronous
    /// fixtures use, before the next creature is cast. Nothing is saved: the
    /// exact-working-load sentinel and the request-local cleanup assertion
    /// prove the save file was only ever read.
    ///
    /// The review images are the deliverable; the assertions only say whether
    /// each image is worth looking at (creature in frame, screen lit,
    /// renderer enabled, dissolve finished). A creature that renders wrongly
    /// but in frame still passes here, and is caught by the person or the
    /// agent who looks at the file.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int CreatureReviewSpawnSettleUpdates = 2;
        private const int CreatureReviewCleanupSettleUpdates = 5;

        private List<SummonVariantSpec> _creatureReviewQueue;
        private int _creatureReviewIndex;
        private int _creatureReviewPhase;
        private int _creatureReviewSettle;
        private bool _creatureReviewQuantity;
        private UnitMoveTo[] _creatureReviewCrowdMoves;
        private Vector3[] _creatureReviewCrowdOrigins;
        private Vector3[] _creatureReviewCrowdDestinations;
        private float[] _creatureReviewCrowdTravel;
        private float[] _creatureReviewCrowdApproach;
        private float[] _creatureReviewCrowdVelocity;
        private UnitEntityData[] _creatureReviewCrowdAwakeBefore;
        private bool _creatureReviewCrowdWasPaused;
        private int _creatureReviewCrowdFrames;
        private int _creatureReviewCrowdWait;
        private UnitEntityData[] _creatureReviewUnits = Array.Empty<UnitEntityData>();
        private UnitEntityData _creatureReviewCaster;
        private UnitEntityData[] _creatureReviewParty;
        private object _creatureReviewGameState;
        private BlueprintScriptableObject[] _creatureReviewBlueprints;
        private readonly List<RuntimeTestAssertion> _creatureReviewAssertions =
            new List<RuntimeTestAssertion>();

        /// <summary>
        /// One variant per creature key: normally its own-tier single. The
        /// guarded Sprint 11 crowd request selects its own-tier-plus-two
        /// 1d4+1 route. Unknown or unrelated keys fail the request.
        /// </summary>
        internal static SummonVariantSpec[] ResolveCreatureReviewVariants(
            string creatures, SummonMultiplicity quantity = SummonMultiplicity.One)
        {
            if (quantity != SummonMultiplicity.One &&
                quantity != SummonMultiplicity.OneD4PlusOne)
                throw new InvalidOperationException(
                    "Unsupported creature-review quantity: " + quantity + ".");
            if (string.IsNullOrWhiteSpace(creatures))
                throw new InvalidOperationException(
                    "The creature review needs at least one creature key.");
            string[] keys = creatures.Split(new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim()).Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal).ToArray();
            var result = new List<SummonVariantSpec>();
            foreach (string key in keys)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                    .SingleOrDefault(value => value.Key == key);
                if (creature == null) throw new InvalidOperationException(
                    "Unknown creature key for review: " + key + ".");
                if (quantity != SummonMultiplicity.One &&
                    !IsSprint11UngulateReviewKey(key))
                    throw new InvalidOperationException(
                        "Only hidden Sprint 11 ungulates may use crowd review: " +
                        key + ".");
                SummonFamily family = creature.NaturesAllyTier.HasValue ?
                    SummonFamily.NaturesAlly : SummonFamily.Monster;
                int tier = family == SummonFamily.NaturesAlly ?
                    creature.NaturesAllyTier.Value : creature.MonsterTier.Value;
                if (quantity == SummonMultiplicity.OneD4PlusOne) tier += 2;
                SummonVariantSpec variant = ExpandedSummoningCatalog
                    .GenerateVariants(family).Single(value =>
                        value.Creature.Key == key && value.ParentTier == tier &&
                        value.Multiplicity == quantity);
                // Sprint 10 visual qualification inspects each registered
                // creature before its parent menu entry is published.
                bool suppressedSprint10Candidate =
                    (key == "giant-wasp" || key == "stirge") &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                bool suppressedSprint11Candidate =
                    IsSprint11UngulateReviewKey(key) &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                if (!SummonVisibilityCatalog.IsPublished(variant) &&
                    !suppressedSprint10Candidate && !suppressedSprint11Candidate)
                    throw new InvalidOperationException(
                        "A suppressed creature cannot be reviewed through a parent: " +
                        key + ".");
                result.Add(variant);
            }
            return result.ToArray();
        }

        private static bool IsSprint11UngulateReviewKey(string key)
        {
            return key == "aurochs" || key == "bison" ||
                key == "rhinoceros" || key == "woolly-rhinoceros";
        }

        private static bool IsOriginalReviewKey(string key)
        {
            return key == "giant-wasp" || key == "stirge" ||
                IsSprint11UngulateReviewKey(key);
        }

        private static string OriginalReviewVisualName(string key)
        {
            return key == "stirge"
                ? ExpandedSummoningPteranodonViewPatch.StirgeVisualName
                : key == "giant-wasp"
                    ? ExpandedSummoningPteranodonViewPatch.GiantWaspVisualName
                    : "KMG_" + key + "_Original";
        }

        private UnitEntityData[] SpawnExpandedSummoningCreatureReviewQuantity(
            SummonVariantSpec variant)
        {
            if (variant == null || !IsSprint11UngulateReviewKey(
                    variant.Creature.Key) ||
                variant.Multiplicity != SummonMultiplicity.OneD4PlusOne)
                throw new InvalidOperationException(
                    "Crowd review accepts only a hidden ungulate 1d4+1 route.");
            UnitEntityData caster = _creatureReviewCaster;
            UnitEntityData[] before = ExpandedSummoningKmgUnitsIn(
                caster.HoldingState);
            BlueprintAbility ability = ResolveExpandedSummoningExecution(
                _creatureReviewBlueprints, variant);
            caster.Descriptor.AddFact(ability);
            try
            {
                ExecuteExpandedSummoningRuntimeAbility(caster, ability,
                    variant.ParentTier);
                Game.Instance.EntityCreator.Tick();
            }
            finally
            {
                if (caster.Descriptor.HasFact(ability))
                    caster.Descriptor.RemoveFact(ability);
            }
            UnitEntityData[] appeared = ExpandedSummoningKmgUnitsIn(
                caster.HoldingState).Where(value => !before.Any(prior =>
                    ReferenceEquals(prior, value))).ToArray();
            string expectedName = ExpandedSummoningInternalName(
                ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature));
            if (appeared.Length < 2 || appeared.Length > 5 ||
                appeared.Any(value => value.Blueprint == null ||
                    value.Blueprint.name != expectedName ||
                    !ReferenceEquals(value.HoldingState, caster.HoldingState)))
                throw new InvalidOperationException(
                    "Native ungulate quantity cast did not create two to five exact same-kind units in the loaded working area: " +
                    variant.StableKey + ";count=" + appeared.Length + ";names=" +
                    string.Join(",", appeared.Select(value => value.Blueprint == null ?
                        "<null>" : value.Blueprint.name).ToArray()) + ".");
            return appeared;
        }

        private bool StepExpandedSummoningUngulateCrowdPath(string key)
        {
            try
            {
                if (_creatureReviewCrowdMoves == null)
                {
                    if (_creatureReviewUnits.Length < 2 ||
                        _creatureReviewUnits.Length > 5 ||
                        _creatureReviewUnits.Any(unit => unit == null ||
                            unit.View == null || unit.View.MovementAgent == null ||
                            !unit.IsInGame || !unit.Descriptor.State.CanMove))
                    {
                        if (_creatureReviewCrowdWait++ < MotionReviewFadeBudget)
                            return false;
                        throw new InvalidOperationException(
                            "The native quantity group was not ready for movement.");
                    }
                    if (_creatureReviewUnits.Any(unit =>
                        unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance
                            .SystemMechanics.SummonedUnitAppearBuff) != null))
                    {
                        if (_creatureReviewCrowdWait++ < MotionReviewFadeBudget)
                            return false;
                        throw new InvalidOperationException(
                            "The quantity group's appearance buff did not clear.");
                    }
                    BeginExpandedSummoningUngulateCrowdPath();
                    return false;
                }
                float delta = Game.Instance.TimeController.DeltaTime;
                for (int index = 0; index < _creatureReviewUnits.Length; index++)
                {
                    UnitEntityData unit = _creatureReviewUnits[index];
                    if (unit.Destroyed || unit.View == null ||
                        unit.View.MovementAgent == null)
                        throw new InvalidOperationException(
                            "A quantity member disappeared during native movement.");
                    if (delta > 0f)
                    {
                        unit.View.MovementAgent.TickMovement(delta);
                        unit.Position = unit.View.transform.position;
                    }
                    _creatureReviewCrowdVelocity[index] = Mathf.Max(
                        _creatureReviewCrowdVelocity[index],
                        unit.View.MovementAgent.Velocity.magnitude);
                    _creatureReviewCrowdTravel[index] = Mathf.Max(
                        _creatureReviewCrowdTravel[index], Vector2.Distance(
                            new Vector2(_creatureReviewCrowdOrigins[index].x,
                                _creatureReviewCrowdOrigins[index].z),
                            new Vector2(unit.Position.x, unit.Position.z)));
                    float originalGap = Vector2.Distance(new Vector2(
                            _creatureReviewCrowdOrigins[index].x,
                            _creatureReviewCrowdOrigins[index].z),
                        new Vector2(_creatureReviewCrowdDestinations[index].x,
                            _creatureReviewCrowdDestinations[index].z));
                    float currentGap = Vector2.Distance(new Vector2(
                            unit.Position.x, unit.Position.z),
                        new Vector2(_creatureReviewCrowdDestinations[index].x,
                            _creatureReviewCrowdDestinations[index].z));
                    _creatureReviewCrowdApproach[index] = Mathf.Max(
                        _creatureReviewCrowdApproach[index],
                        originalGap - currentGap);
                }
                _creatureReviewCrowdFrames++;
                if (_creatureReviewCrowdFrames < 240 &&
                    !_creatureReviewUnits.Select((unit, index) =>
                        Vector2.Distance(new Vector2(unit.Position.x,
                                unit.Position.z), new Vector2(
                                _creatureReviewCrowdDestinations[index].x,
                                _creatureReviewCrowdDestinations[index].z)))
                        .All(gap => gap <= 1.5f)) return false;
                FinishExpandedSummoningUngulateCrowdPath(key, null);
                return true;
            }
            catch (Exception exception)
            {
                FinishExpandedSummoningUngulateCrowdPath(key, exception);
                return true;
            }
        }

        private void BeginExpandedSummoningUngulateCrowdPath()
        {
            if (AstarPath.active == null || _creatureReviewCaster == null)
                throw new InvalidOperationException(
                    "Crowd review has no native navigation graph or party anchor.");
            Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(
                _creatureReviewCaster.Position);
            if (anchor.node == null || !anchor.node.Walkable)
                throw new InvalidOperationException(
                    "Crowd review has no walkable party floor node.");
            var offsets = new[] {
                new Vector3(9f, 0f, -3f), new Vector3(9f, 0f, -1f),
                new Vector3(9f, 0f, -5f), new Vector3(11f, 0f, -3f),
                new Vector3(7f, 0f, -3f), new Vector3(7f, 0f, -1f),
                new Vector3(7f, 0f, -5f), new Vector3(5f, 0f, -3f),
                new Vector3(5f, 0f, -1f), new Vector3(11f, 0f, -1f),
                new Vector3(11f, 0f, -5f), new Vector3(5f, 0f, -5f)
            };
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                value != null && value.IsInGame).ToArray();
            var destinations = new List<Vector3>();
            foreach (Vector3 offset in offsets)
            {
                Vector3 requested = anchor.clampedPosition + offset;
                Pathfinding.NNInfo nearest = AstarPath.active.GetNearest(requested);
                if (nearest.node == null || !nearest.node.Walkable ||
                    nearest.node.Area != anchor.node.Area ||
                    nearest.node.GraphIndex != anchor.node.GraphIndex ||
                    Vector3.Distance(requested, nearest.clampedPosition) > 0.5f ||
                    party.Any(member => Vector3.Distance(member.Position,
                        nearest.clampedPosition) < 2.5f) ||
                    destinations.Any(value => Vector3.Distance(value,
                        nearest.clampedPosition) < 2f)) continue;
                destinations.Add(nearest.clampedPosition);
                if (destinations.Count == _creatureReviewUnits.Length) break;
            }
            if (destinations.Count != _creatureReviewUnits.Length)
                throw new InvalidOperationException(
                    "The surveyed connected floor has only " +
                    destinations.Count + " distinct crowd destinations for " +
                    _creatureReviewUnits.Length + " units.");
            _creatureReviewCrowdAwakeBefore = Game.Instance.State.AwakeUnits
                .ToArray();
            _creatureReviewCrowdWasPaused = Game.Instance.IsPaused;
            if (_creatureReviewCrowdWasPaused) Game.Instance.IsPaused = false;
            _creatureReviewCrowdMoves = new UnitMoveTo[_creatureReviewUnits.Length];
            _creatureReviewCrowdOrigins = _creatureReviewUnits.Select(unit =>
                unit.Position).ToArray();
            _creatureReviewCrowdDestinations = destinations.ToArray();
            _creatureReviewCrowdTravel = new float[_creatureReviewUnits.Length];
            _creatureReviewCrowdApproach = new float[_creatureReviewUnits.Length];
            _creatureReviewCrowdVelocity = new float[_creatureReviewUnits.Length];
            _creatureReviewCrowdFrames = 0;
            for (int index = 0; index < _creatureReviewUnits.Length; index++)
            {
                UnitEntityData unit = _creatureReviewUnits[index];
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
                var move = new UnitMoveTo(destinations[index], 0.5f);
                move.Init(unit);
                if (!move.CanStart)
                    throw new InvalidOperationException(
                        "Native quantity member " + index +
                        " could not start its distinct move command.");
                unit.Commands.Run(move);
                if (!unit.Commands.Contains(move) ||
                    !ReferenceEquals(move.Executor, unit))
                    throw new InvalidOperationException(
                        "Native quantity member " + index +
                        " did not accept its move command.");
                _creatureReviewCrowdMoves[index] = move;
            }
        }

        private void FinishExpandedSummoningUngulateCrowdPath(string key,
            Exception error)
        {
            var observations = new List<string>();
            bool traveled = error == null && _creatureReviewCrowdMoves != null;
            for (int index = 0; index < _creatureReviewUnits.Length; index++)
            {
                UnitEntityData unit = _creatureReviewUnits[index];
                if (unit != null && unit.Commands != null)
                    unit.Commands.InterruptMove();
                float distance = _creatureReviewCrowdTravel == null ? 0f :
                    _creatureReviewCrowdTravel[index];
                float approach = _creatureReviewCrowdApproach == null ? 0f :
                    _creatureReviewCrowdApproach[index];
                float velocity = _creatureReviewCrowdVelocity == null ? 0f :
                    _creatureReviewCrowdVelocity[index];
                float finalGap = _creatureReviewCrowdDestinations == null ||
                    unit == null ? float.PositiveInfinity : Vector2.Distance(
                        new Vector2(unit.Position.x, unit.Position.z),
                        new Vector2(_creatureReviewCrowdDestinations[index].x,
                            _creatureReviewCrowdDestinations[index].z));
                traveled = traveled && distance >= 0.75f &&
                    approach >= 0.75f && velocity > 0.01f &&
                    finalGap <= 1.5f;
                observations.Add(index + ":" + distance.ToString("0.##",
                    CultureInfo.InvariantCulture) + "/" + approach.ToString(
                    "0.##", CultureInfo.InvariantCulture) + "/" +
                    velocity.ToString("0.##", CultureInfo.InvariantCulture) +
                    "/" + finalGap.ToString("0.##", CultureInfo.InvariantCulture));
            }
            bool awakeRestored = true;
            if (_creatureReviewCrowdAwakeBefore != null)
            {
                foreach (UnitEntityData unit in _creatureReviewUnits)
                    if (!_creatureReviewCrowdAwakeBefore.Contains(unit))
                        Game.Instance.State.AwakeUnits.Remove(unit);
                awakeRestored = Game.Instance.State.AwakeUnits.SequenceEqual(
                    _creatureReviewCrowdAwakeBefore);
                Game.Instance.IsPaused = _creatureReviewCrowdWasPaused;
                _creatureReviewCrowdAwakeBefore = null;
            }
            _creatureReviewAssertions.Add(Assertion(
                "expanded-summoning-ungulate-crowd-path-" + key,
                "each native quantity member accepts a distinct simultaneous move and reaches its connected-floor destination",
                "count=" + _creatureReviewUnits.Length + ";frames=" +
                    _creatureReviewCrowdFrames + ";travel/approach/velocity/gap=" +
                    string.Join("|", observations.ToArray()) +
                    ";awakeRestored=" + awakeRestored +
                    (error == null ? "" : ";error=" + error.GetType().Name +
                        ":" + error.Message),
                traveled && awakeRestored,
                "real UnitMoveTo commands on a simultaneous 1d4+1 group; native movement-agent samples and request-local state restoration"));
        }

        private void StepExpandedSummoningCreatureReview()
        {
            if (_creatureReviewQueue == null)
            {
                if (!_context.FeatureModules.Active.ExpandedSummoning)
                    throw new InvalidOperationException(
                        "The creature review requires the Expanded Summoning module to be active.");
                UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null).ToArray();
                if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException(
                        "The loaded working-save party fingerprint changed before the creature review.");
                UnitEntityData caster = party.FirstOrDefault(value =>
                    value.HoldingState != null);
                if (caster == null) throw new InvalidOperationException(
                    "The loaded working save has no party member in an active area state.");
                object gameState = ReadExactMember(Game.Instance, "State");
                int staleLeft = RemoveExpandedSummoningStaleSummons(gameState, party);
                if (staleLeft != 0)
                    throw new InvalidOperationException(
                        "Stale KMG summons could not be removed before the creature review: " +
                        staleLeft + " remain.");
                _creatureReviewParty = party;
                _creatureReviewCaster = caster;
                _creatureReviewGameState = gameState;
                _creatureReviewBlueprints = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                _creatureReviewQueue = ResolveCreatureReviewVariants(
                    (string)_request.Parameters["creatures"],
                    _request.Parameters["quantity"] == null ?
                        SummonMultiplicity.One :
                        SummonMultiplicity.OneD4PlusOne).ToList();
                _creatureReviewQuantity = _request.Parameters["quantity"] != null;
                _creatureReviewIndex = 0;
                _creatureReviewPhase = 0;
                WriteLifecycleStage("creature-review-start");
                // Fall through: the first cast happens in the same guarded
                // update as the setup, exactly as the persistence prepare
                // casts its fixture.
            }
            if (_creatureReviewIndex >= _creatureReviewQueue.Count)
            {
                CompleteExpandedSummoningCreatureReview();
                return;
            }
            SummonVariantSpec variant = _creatureReviewQueue[_creatureReviewIndex];
            string key = variant.Creature.Key;
            switch (_creatureReviewPhase)
            {
                case 0:
                    _creatureReviewUnits = _creatureReviewQuantity ?
                        SpawnExpandedSummoningCreatureReviewQuantity(variant) :
                        SpawnExpandedSummoningVariants(
                            _creatureReviewBlueprints, _creatureReviewCaster,
                            new[] { variant }, "Creature review of " + key);
                    _creatureReviewCrowdMoves = null;
                    _creatureReviewCrowdWait = 0;
                    _creatureReviewCrowdFrames = 0;
                    ResetExpandedSummoningMotionReview(
                        ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)
                            .Replace('.', '_').Replace('-', '_'), key + "-review");
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 1;
                    WriteLifecycleStage("creature-review-" + key + "-summoned");
                    return;
                case 1:
                    if (_creatureReviewSettle++ < CreatureReviewSpawnSettleUpdates) return;
                    _creatureReviewPhase = _creatureReviewQuantity ? 4 : 2;
                    return;
                case 4:
                    if (!StepExpandedSummoningUngulateCrowdPath(key)) return;
                    _creatureReviewPhase = 2;
                    return;
                case 2:
                    if (!StepExpandedSummoningMotionReview(_creatureReviewUnits,
                            "summoned")) return;
                    // Sprint 5: a creature with a registered visual variant
                    // must show it applied on the reviewed view.
                    bool variantRegistered = _creatureReviewUnits.Any(unit =>
                        ExpandedSummoningVisualVariantPatch.RegisteredBlueprintNames
                            .Contains(unit.Blueprint.name));
                    string variantOutcome = string.Join("|", _creatureReviewUnits.Select(
                        unit => ExpandedSummoningVisualVariantPatch.DescribeView(unit.View))
                        .ToArray());
                    bool variantValid = !variantRegistered || _creatureReviewUnits.All(
                        unit => ExpandedSummoningVisualVariantPatch.DescribeView(unit.View)
                            .StartsWith("variant:applied", StringComparison.Ordinal));
                    // Round 8: the attach-time outcome alone proved nothing
                    // about the render (the controller had replaced the
                    // clones); the materials on the view at capture time are
                    // recorded and, for a registered variant, must still be
                    // the project clones.
                    bool retainedAll = true;
                    var materialsNow = new List<string>();
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                    {
                        bool retained;
                        materialsNow.Add(DescribeExpandedSummoningViewMaterials(unit,
                            out retained));
                        retainedAll = retainedAll && retained;
                    }
                    bool retainedValid = !variantRegistered || retainedAll;
                    string waspIsolatedView = key == "giant-wasp"
                        ? CaptureWaspWithoutAuxiliaryRenderer(_creatureReviewUnits[0])
                        : "<not applicable>";
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-" + key,
                        "idle, moving-a, moving-b and attack captures in frame, lit, renderer enabled, intact" +
                            (variantRegistered ? "; registered visual variant applied at attach and retained on the view at capture" : ""),
                        MotionReviewSummary + ";visualVariant=" + variantOutcome +
                            ";materialsAtCapture=" + string.Join("|", materialsNow.ToArray()) +
                            ";waspIsolatedView=" + waspIsolatedView,
                        MotionReviewValid && variantValid && retainedValid,
                        (variant.Family == SummonFamily.Monster ? "Summon Monster " :
                            "Summon Nature's Ally ") + variant.ParentTier +
                        (_creatureReviewQuantity ? " 1d4+1" : " single") +
                        " cast through its registered execution; party-camera renders"));
                    if (IsOriginalReviewKey(key))
                    {
                        string expectedName = OriginalReviewVisualName(key);
                        string attach = ExpandedSummoningPteranodonViewPatch
                            .DescribeView(_creatureReviewUnits[0].View);
                        bool exactMesh = _creatureReviewUnits.All(unit =>
                            unit.View != null && unit.View
                                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                .Any(renderer => renderer != null &&
                                    renderer.sharedMesh != null &&
                                    renderer.sharedMesh.name == expectedName));
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-original-view-" + key,
                            "validated original mesh attached on the donor renderer",
                            attach + ";mesh=" + expectedName + ";present=" + exactMesh,
                            attach.StartsWith("visual:attached", StringComparison.Ordinal) &&
                                exactMesh,
                            "exact blueprint identity, loader status and live renderer mesh"));
                        if (key == "stirge")
                        {
                            string fileName = "stirge-review-overhead-open-view.png";
                            string capture = WriteExpandedSummoningOverheadStrikeCapture(
                                _creatureReviewUnits[0], _creatureReviewUnits[0],
                                _request.EvidenceDirectory, fileName);
                            _creatureReviewAssertions.Add(Assertion(
                                "expanded-summoning-stirge-overhead-view",
                                "live overhead frame written for independent visual inspection",
                                capture,
                                capture.StartsWith("png=" + fileName + ";",
                                    StringComparison.Ordinal),
                                "request-local camera pose restored; image is supporting art evidence, not mechanical proof"));
                        }
                    }
                    if (key == "eagle" || key == "dire-bat" ||
                        key == "giant-wasp" || key == "stirge")
                    {
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-flight-travel-" + key,
                            "native move command accepted; at least 0.75 m planar travel and nonzero movement-agent velocity",
                            MotionReviewSummary,
                            MotionReviewTravelValid,
                            "cross-frame native UnitMoveTo and unit-position samples; animation callbacks are insufficient"));
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-doorway-travel-" + key,
                            "the native move crosses the surveyed room opening and approaches a connected floor node in the adjacent room",
                            MotionReviewSummary,
                            MotionReviewDoorwayValid,
                            "named native area landmark, same-area endpoints, native UnitMoveTo and cross-frame position samples"));
                    }
                    if (IsSprint11UngulateReviewKey(key))
                    {
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-ungulate-travel-" + key,
                            "native ground move accepted; at least 0.75 m planar travel and nonzero movement-agent velocity",
                            MotionReviewSummary,
                            MotionReviewTravelValid,
                            "surveyed connected floor route, native UnitMoveTo and cross-frame position/velocity samples"));
                    }
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                        CleanupExpandedSummoningUnit(unit);
                    Game.Instance.EntityDestroyer.Tick();
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 3;
                    return;
                default:
                    if (_creatureReviewSettle++ < CreatureReviewCleanupSettleUpdates) return;
                    int live = _creatureReviewUnits.Count(value => !value.Destroyed ||
                        value.View != null || value.HoldingState != null);
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-cleanup-" + key, "0",
                        live.ToString(), live == 0,
                        "reviewed summon dismissed and destroyed before the next cast"));
                    if (IsOriginalReviewKey(key))
                    {
                        string visualName = OriginalReviewVisualName(key);
                        int ownedMeshes = Resources.FindObjectsOfTypeAll<Mesh>()
                            .Count(value => value != null &&
                                (value.name == visualName || (key == "giant-wasp" &&
                                    value.name == "KMG_GiantWaspStingProbe")));
                        int ownedMaterials = Resources
                            .FindObjectsOfTypeAll<Material>()
                            .Count(value => value != null && value.name != null &&
                                value.name.StartsWith(
                                    visualName,
                                    StringComparison.Ordinal));
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-" + key + "-owned-view-resources",
                            "0 private visual/probe meshes and 0 private materials after view destruction",
                            "meshes=" + ownedMeshes + ";materials=" +
                                ownedMaterials,
                            ownedMeshes == 0 && ownedMaterials == 0,
                            "live Unity resource enumeration after the one reviewed view is destroyed; cached source mesh has a distinct name"));
                    }
                    WriteLifecycleStage("creature-review-" + key + "-complete");
                    _creatureReviewUnits = Array.Empty<UnitEntityData>();
                    _creatureReviewIndex++;
                    _creatureReviewPhase = 0;
                    return;
            }
        }

        private string CaptureWaspWithoutAuxiliaryRenderer(UnitEntityData unit)
        {
            if (unit == null || unit.View == null) return "<no-view>";
            Renderer[] auxiliary = unit.View.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null &&
                    !(value is SkinnedMeshRenderer) && value.enabled).ToArray();
            try
            {
                foreach (Renderer renderer in auxiliary) renderer.enabled = false;
                return "hidden=" + auxiliary.Length + ";" +
                    WriteExpandedSummoningPartyCameraCapture(unit,
                        _request.EvidenceDirectory,
                        "giant-wasp-review-summoned-attack-no-auxiliary.png");
            }
            finally
            {
                foreach (Renderer renderer in auxiliary) renderer.enabled = true;
            }
        }

        /// <summary>
        /// What the reviewed view's renderers carry at capture time: per
        /// material its name, shader, the declared colour, tint, emission,
        /// texture and dissolve slots, the current tint, the dissolve amount
        /// and whether the view's material controller drives it. The variant
        /// is retained when at least one renderer material is the project
        /// clone (or the controller's instance of it).
        /// </summary>
        private static string DescribeExpandedSummoningViewMaterials(UnitEntityData unit,
            out bool variantRetained)
        {
            variantRetained = false;
            if (unit == null || unit.View == null) return "<no-view>";
            Renderer[] renderers = unit.View.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null && value.sharedMaterials != null &&
                    value.sharedMaterials.Length != 0).ToArray();
            var controller = unit.View.GetComponentInChildren<
                Kingmaker.Visual.MaterialEffects.StandardMaterialController>(true);
            IList<Material> driven = ExpandedSummoningPteranodonViewPatch
                .ControllerMaterials(controller);
            var parts = new List<string>();
            int materials = 0, variantMaterials = 0;
            foreach (Renderer renderer in renderers)
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) { parts.Add("<null>"); continue; }
                    materials++;
                    if (material.name.StartsWith(ExpandedSummoningVisualVariantPatch
                            .VariantMaterialName, StringComparison.Ordinal))
                        variantMaterials++;
                    var slots = new List<string>();
                    foreach (string slot in new[] { "_Color", "_TintColor", "_BaseColor",
                        "_MainColor", "_EmissionColor", "_MainTex", "_Dissolve" })
                        if (material.HasProperty(slot)) slots.Add(slot);
                    string tint = material.HasProperty("_TintColor")
                        ? DescribeReviewColour(material.GetColor("_TintColor"))
                        : material.HasProperty("_Color")
                            ? DescribeReviewColour(material.GetColor("_Color")) : "<none>";
                    string dissolve = material.HasProperty("_Dissolve")
                        ? material.GetFloat("_Dissolve").ToString("0.###",
                            CultureInfo.InvariantCulture) : "<none>";
                    parts.Add(material.name.Replace(';', ',').Replace('|', '/') + "{" +
                        (material.shader == null ? "<no-shader>" : material.shader.name
                            .Replace(';', ',').Replace('|', '/')) + ":" +
                        string.Join(",", slots.ToArray()) + ":tint=" + tint +
                        ":dissolve=" + dissolve + ":driven=" +
                        (driven != null && driven.Contains(material)) +
                        ":" + DescribeReviewMaterialProbe(material) + "}");
                }
            variantRetained = variantMaterials != 0;
            var rimAnimations = new List<string>();
            var rimController = ExpandedSummoningRimAnimationPatch.RimControllerOf(controller);
            if (rimController != null && rimController.Animations != null)
                foreach (var animation in rimController.Animations)
                    rimAnimations.Add(animation == null ? "<null>" : "loop=" + animation.LoopAnimation +
                        ",lifetime=" + animation.Lifetime.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",scale=" + animation.IntensityScale.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",colour=" + DescribeReviewColour(animation.CurrentColor) +
                        ",intensity=" + animation.CurrentIntensity.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",finished=" + animation.IsFinished);
            return "renderers=" + renderers.Length + ";materials=" + materials +
                ";variantMaterials=" + variantMaterials + ";controllerMaterials=" +
                (driven == null ? -1 : driven.Count) + ";" +
                string.Join(",", parts.ToArray()) + ";character=" +
                DescribeReviewCharacter(unit.View) + ";rimAnimations[" +
                string.Join("|", rimAnimations.ToArray()) + "];rimPatch=" +
                ExpandedSummoningRimAnimationPatch.Describe(unit.View);
        }

        /// <summary>
        /// Round 11: the view's character-system state. A character built at
        /// runtime rebuilds its texture atlases after the view attaches and
        /// assigns them to the renderer's materials, which is where a coat
        /// put on the clone at attach would be overwritten.
        /// </summary>
        private static string DescribeReviewCharacter(UnitEntityView view)
        {
            var character = view.GetComponentInChildren<
                Kingmaker.Visual.CharacterSystem.Character>(true);
            if (character == null) return "<none>";
            Type type = character.GetType();
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var atlases = new List<string>();
            var atlasList = type.GetField("m_Atlases", flags) == null ? null :
                type.GetField("m_Atlases", flags).GetValue(character) as System.Collections.IEnumerable;
            if (atlasList != null)
                foreach (object atlas in atlasList)
                {
                    if (atlas == null) { atlases.Add("<null>"); continue; }
                    Type atlasType = atlas.GetType();
                    object channel = atlasType.GetProperty("Channel", flags) == null ? null :
                        atlasType.GetProperty("Channel", flags).GetValue(atlas, null);
                    Texture texture = atlasType.GetProperty("AtlasTexture", flags) == null ? null :
                        atlasType.GetProperty("AtlasTexture", flags).GetValue(atlas, null) as Texture;
                    Material material = atlasType.GetProperty("Material", flags) == null ? null :
                        atlasType.GetProperty("Material", flags).GetValue(atlas, null) as Material;
                    atlases.Add((channel == null ? "?" : channel.ToString()) + "=" +
                        (texture == null ? "<null>" : texture.name.Replace(';', ',')
                            .Replace('|', '/').Replace(':', '.') + "@" + texture.width + "x" +
                            texture.height) + "/" + (material == null ? "<null>" :
                            material.name.Replace(';', ',').Replace('|', '/').Replace(':', '.')));
                }
            var entities = new List<string>();
            var entityList = type.GetField("m_EquipmentEntities", flags) == null ? null :
                type.GetField("m_EquipmentEntities", flags).GetValue(character) as System.Collections.IEnumerable;
            if (entityList != null)
                foreach (object entity in entityList)
                {
                    var ee = entity as Kingmaker.Visual.CharacterSystem.EquipmentEntity;
                    if (ee == null) { entities.Add("<null>"); continue; }
                    entities.Add(ee.name.Replace(';', ',').Replace('|', '/').Replace(':', '.') +
                        "(primaryRamps=" + (ee.PrimaryRamps == null ? -1 : ee.PrimaryRamps.Count) +
                        ",secondaryRamps=" + (ee.SecondaryRamps == null ? -1 : ee.SecondaryRamps.Count) +
                        ",profile=" + (ee.ColorsProfile == null ? "<null>" : ee.ColorsProfile.name
                            .Replace(';', ',').Replace('|', '/').Replace(':', '.')) +
                        ",bodyParts=" + (ee.BodyParts == null ? -1 : ee.BodyParts.Count) + ")");
                }
            var ramps = new List<string>();
            var rampList = type.GetField("m_RampIndices", flags) == null ? null :
                type.GetField("m_RampIndices", flags).GetValue(character) as System.Collections.IEnumerable;
            if (rampList != null)
                foreach (object ramp in rampList) ramps.Add(ramp == null ? "<null>" : ramp.ToString());
            var shared = type.GetField("m_SharedMaterials", flags) == null ? null :
                type.GetField("m_SharedMaterials", flags).GetValue(character) as System.Collections.IEnumerable;
            var sharedNames = new List<string>();
            if (shared != null)
                foreach (object material in shared)
                    sharedNames.Add(material == null ? "<null>" : ((Material)material).name
                        .Replace(';', ',').Replace('|', '/').Replace(':', '.'));
            return "present:baked=" + (character.BakedCharacter != null) + ":dirty=" +
                character.IsDirty + ":atlasesDirty=" + character.IsAtlasesDirty +
                ":atlases[" + string.Join(",", atlases.ToArray()) + "]:entities[" +
                string.Join(",", entities.ToArray()) + "]:rampIndices[" +
                string.Join(",", ramps.ToArray()) + "]:sharedMaterials[" +
                string.Join(",", sharedNames.ToArray()) + "]";
        }

        /// <summary>
        /// Round 11: every texture property the shader declares with the
        /// assigned texture's name and size, the shader keywords, the render
        /// queue, and the float and colour slots the game's own code
        /// references - so a variant can target the slot that paints the rig.
        /// </summary>
        private static string DescribeReviewMaterialProbe(Material material)
        {
            var textures = new List<string>();
            string[] names;
            try { names = material.GetTexturePropertyNames(); }
            catch (Exception) { names = new string[0]; }
            foreach (string name in names)
            {
                Texture texture = material.GetTexture(name);
                textures.Add(name + "=" + (texture == null ? "<null>" :
                    texture.name.Replace(';', ',').Replace('|', '/').Replace(':', '.') +
                    "@" + texture.width + "x" + texture.height));
            }
            var floats = new List<string>();
            foreach (string name in new[] { "_Emission", "_RimPower", "_RimLighting",
                "_Metallic", "_Cutout", "_Alpha", "_AlphaScale", "_DissolveEmission",
                "_Glossiness", "_Smoothness", "_BumpScale", "_OcclusionStrength" })
                if (material.HasProperty(name))
                    floats.Add(name + "=" + material.GetFloat(name).ToString("0.###",
                        CultureInfo.InvariantCulture));
            var colours = new List<string>();
            foreach (string name in new[] { "_RimColor", "_ColorMask", "_ChannelMask",
                "_DissolveColor", "_EmissionColor", "_SpecColor", "_Color", "_TintColor" })
                if (material.HasProperty(name))
                    colours.Add(name + "=" + DescribeReviewColour(material.GetColor(name)));
            return "textures[" + string.Join(",", textures.ToArray()) + "]:floats[" +
                string.Join(",", floats.ToArray()) + "]:colours[" +
                string.Join(",", colours.ToArray()) + "]:keywords[" +
                string.Join(",", material.shaderKeywords ?? new string[0]) + "]:queue=" +
                material.renderQueue;
        }

        private static string DescribeReviewColour(Color value)
        {
            return value.r.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.g.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.b.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.a.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void CompleteExpandedSummoningCreatureReview()
        {
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            int remaining = ExpandedSummoningPersistentUnits(
                _creatureReviewGameState, _creatureReviewParty).Length;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-load",
                    "one correlated working descriptor; baseline distinct",
                    "working=" + evidence.WorkingMatchCount + ";baseline=" +
                        evidence.BaselineMatchCount + ";correlated=" +
                        evidence.DescriptorReferenceCorrelated,
                    evidence.WorkingMatchCount == 1 &&
                        evidence.BaselineMatchCount == 1 &&
                        evidence.DescriptorReferenceCorrelated,
                    "object-reference-correlated guarded load path")
            };
            assertions.AddRange(_creatureReviewAssertions);
            assertions.Add(Assertion("expanded-summoning-creature-review-count",
                _creatureReviewQueue.Count.ToString(),
                _creatureReviewAssertions.Count(value =>
                    value.Name.StartsWith("expanded-summoning-creature-review-",
                        StringComparison.Ordinal) && !value.Name.StartsWith(
                        "expanded-summoning-creature-review-cleanup-",
                        StringComparison.Ordinal)).ToString(),
                _creatureReviewAssertions.Count(value =>
                    value.Name.StartsWith("expanded-summoning-creature-review-",
                        StringComparison.Ordinal) && !value.Name.StartsWith(
                        "expanded-summoning-creature-review-cleanup-",
                        StringComparison.Ordinal)) == _creatureReviewQueue.Count,
                "every requested creature was reviewed"));
            assertions.Add(Assertion("request-local-cleanup",
                "no KMG summons remain; no save written",
                "remaining=" + remaining + ";saveRoutines=" +
                    evidence.ExpectedWorkingSaveRoutineCount + ";unexpected=" +
                    evidence.SaveWritingApiObserved,
                remaining == 0 && evidence.ExpectedWorkingSaveRoutineCount == 0 &&
                    !evidence.SaveWritingApiObserved,
                "the working save is read, never written, by the review"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
            result.WorkingSaveSmoke = evidence;
            Complete(result);
        }
    }
}
