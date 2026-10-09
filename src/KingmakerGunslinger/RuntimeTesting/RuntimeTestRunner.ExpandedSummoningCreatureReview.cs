using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
    /// rendering whenever the game reports it visible, dissolve finished).
    /// A creature that renders wrongly but in frame still passes here, and is
    /// caught by the person or the agent who looks at the file.
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
        private bool _creatureReviewExpiryStarted;
        private bool _creatureReviewExpiryTimed;
        private bool _creatureReviewExpiryWasPaused;
        private bool _creatureReviewExpiryPauseRestored;
        private TimeSpan _creatureReviewExpiryStartTime;
        private TimeSpan _creatureReviewExpiryEndTime;
        private DateTime _creatureReviewExpiryStartUtc;
        private string _creatureReviewExpiryInitial = "<not run>";
        private UnitEntityData[] _creatureReviewUnits = Array.Empty<UnitEntityData>();
        private UnitEntityData _creatureReviewCaster;
        private UnitMoveTo _stirgePreyMove;
        private Vector3 _stirgePreyOriginalPosition;
        private Vector3 _stirgePreyMoveStart;
        private Vector3 _stirgePreyDestination;
        private bool _stirgePreyWasPaused;
        private bool _stirgePreyRelocated;
        private bool _stirgePreyMoveStarted;
        private bool _stirgePreyMoveRunning;
        private bool _stirgePreyMoveAccepted;
        private bool _stirgePreyAgentWantsMove;
        private float _stirgePreyTravel;
        private float _stirgePreyVelocity;
        private float _stirgePreyInitialGap;
        private float _stirgePreyMinGap;
        private int _stirgePreyFrames;
        private int _stirgePreyReachedFrame;
        private string _stirgePreySurvey;
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
                    !IsGroundCrowdReviewKey(key))
                    throw new InvalidOperationException(
                        "This creature is not on the crowd review roster: " +
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
                // A withheld creature's art has to be inspected under the
                // party camera before its suppression is lifted, which cannot
                // be done through a published parent it does not have. Sprints
                // 11, 12 and 13 each opened this door for exactly their own
                // hidden creatures and closed it again at publication; the
                // Sprint 14 allowance is the same closed list and goes the same
                // way. It is a development-owned route and never a player one:
                // the parent the review casts through stays unpublished, and
                // removing these keys from the suppression set is what actually
                // publishes the creatures.
                bool suppressedSprint13Candidate =
                    IsSprint13CreatureReviewKey(key) &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                bool suppressedSprint14Candidate =
                    IsSprint14InsectReviewKey(key) &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                bool suppressedSprint16Candidate =
                    CrocodilianVisualPolicy.Keys.Contains(key) &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                bool suppressedSprint17Snake =
                    SerpentineVisualPolicy.IsSnake(key) &&
                    !SummonVisibilityCatalog.IsPublished(variant);
                if (!SummonVisibilityCatalog.IsPublished(variant) &&
                    !suppressedSprint13Candidate &&
                    !suppressedSprint14Candidate &&
                    !suppressedSprint16Candidate &&
                    !suppressedSprint17Snake)
                    throw new InvalidOperationException(
                        "A suppressed creature cannot be reviewed through a parent: " +
                        key + ".");
                result.Add(variant);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Every creature the crowd review accepts. Both the request guard and
        /// the spawner ask this one question, so a sprint added to the roster
        /// below reaches both at once; assembling the same pair of rosters at
        /// two call sites is what previously let them disagree.
        /// </summary>
        private static bool IsGroundCrowdReviewKey(string key)
        {
            return IsSprint11UngulateReviewKey(key) ||
                IsSprint12QuadrupedReviewKey(key) ||
                CrocodilianVisualPolicy.Keys.Contains(key) ||
                SerpentineVisualPolicy.IsSnake(key) || key == "salamander";
        }

        private static bool IsSprint11UngulateReviewKey(string key)
        {
            return key == "aurochs" || key == "bison" ||
                key == "rhinoceros" || key == "woolly-rhinoceros";
        }

        /// <summary>
        /// The four Sprint 12 compact quadrupeds. They are published now, so
        /// this list no longer waives the publication guard; it only names
        /// them as members of the crowd review roster.
        /// </summary>
        private static bool IsSprint12QuadrupedReviewKey(string key)
        {
            return key == "dire-rat" || key == "dog" || key == "hyena" ||
                key == "goblin-dog";
        }

        /// <summary>
        /// Sprint 13's three creatures. The Wolverine and the Shadow Mastiff
        /// ride the same Worg rig the Goblin Dog does; the Poison Frog rides
        /// the Giant Poisonous Frog. Only the Shadow Mastiff is suppressed;
        /// the other two are published roster members whose visuals are new,
        /// so naming all three here costs nothing and keeps the sprint's
        /// roster in one place.
        /// </summary>
        private static bool IsSprint13CreatureReviewKey(string key)
        {
            return key == "wolverine" || key == "shadow-mastiff" ||
                key == "poisonous-frog";
        }

        /// <summary>
        /// The insect family, all on the Giant Spider rig: Sprint 14's
        /// three and Sprint 15's two. They are reviewable while they are
        /// still withheld because the review casts through a
        /// development-owned private route rather than the player's menu,
        /// which is the only way to look at a creature before it publishes.
        /// </summary>
        private static bool IsSprint14InsectReviewKey(string key)
        {
            return key == "fire-beetle" || key == "giant-ant-worker" ||
                key == "giant-ant-soldier" || key == "giant-ant-drone" ||
                key == "giant-stag-beetle";
        }

        private static bool IsOriginalReviewKey(string key)
        {
            // Dog is excluded on purpose: it keeps the native Dog
            // presentation, so it has no project-owned view to inspect.
            return OriginalFlightCleanupPolicy.Handles(key) || key == "giant-wasp" || key == "stirge" ||
                key == "dire-rat" || key == "hyena" || key == "goblin-dog" ||
                IsSprint11UngulateReviewKey(key) ||
                IsSprint14InsectReviewKey(key) ||
                IsSprint13CreatureReviewKey(key) ||
                CrocodilianVisualPolicy.Keys.Contains(key);
        }

        // The Sprint 12 quadrupeds already carry the "KMG_<key>_Original"
        // mesh name the ungulates use, so OriginalReviewVisualName needs no
        // new branch for them - its existing fallback is already correct.
        private static string OriginalReviewVisualName(string key)
        {
            if (key == "eagle") return ExpandedSummoningPteranodonViewPatch.EagleVisualName;
            if (key == "dire-bat") return ExpandedSummoningPteranodonViewPatch.DireBatVisualName;
            if (key == "pteranodon") return ExpandedSummoningPteranodonViewPatch.CustomVisualName;
            return key == "stirge"
                ? ExpandedSummoningPteranodonViewPatch.StirgeVisualName
                : key == "giant-wasp"
                    ? ExpandedSummoningPteranodonViewPatch.GiantWaspVisualName
                    : "KMG_" + key + "_Original";
        }

        private UnitEntityData[] SpawnExpandedSummoningCreatureReviewQuantity(
            SummonVariantSpec variant)
        {
            // The crowd route is still a closed list rather than any creature,
            // but it is no longer ungulate-only: Sprint 12's compact quadrupeds
            // need the same crowded-space review, and the ungulates are
            // published now, so the old "hidden ungulate" wording was stale on
            // both counts.
            if (variant == null ||
                !IsGroundCrowdReviewKey(variant.Creature.Key) ||
                variant.Multiplicity != SummonMultiplicity.OneD4PlusOne)
                throw new InvalidOperationException(
                    "Crowd review accepts only a creature on its roster, on a 1d4+1 route.");
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

        private bool StepExpandedSummoningGroundCrowdPath(string key)
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
                    BeginExpandedSummoningGroundCrowdPath();
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
                FinishExpandedSummoningGroundCrowdPath(key, null);
                return true;
            }
            catch (Exception exception)
            {
                FinishExpandedSummoningGroundCrowdPath(key, exception);
                return true;
            }
        }

        private void BeginExpandedSummoningGroundCrowdPath()
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

        private void FinishExpandedSummoningGroundCrowdPath(string key,
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
            string awakeEvidence = "legacy-owned-removal";
            if (_creatureReviewCrowdAwakeBefore != null)
            {
                bool ownedRestored = true;
                if (SerpentineVisualPolicy.IsSnake(key) || key == "salamander")
                    ownedRestored = RestoreSprint17SnakeCrowdAwake(key, out awakeEvidence);
                else
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                        if (!_creatureReviewCrowdAwakeBefore.Contains(unit))
                            Game.Instance.State.AwakeUnits.Remove(unit);
                awakeRestored = ownedRestored && Game.Instance.State.AwakeUnits.SequenceEqual(
                    _creatureReviewCrowdAwakeBefore);
                Game.Instance.IsPaused = _creatureReviewCrowdWasPaused;
                _creatureReviewCrowdAwakeBefore = null;
            }
            _creatureReviewAssertions.Add(Assertion(
                "expanded-summoning-ground-crowd-path-" + key,
                "each native quantity member accepts a distinct simultaneous move and reaches its connected-floor destination",
                "count=" + _creatureReviewUnits.Length + ";frames=" +
                    _creatureReviewCrowdFrames + ";travel/approach/velocity/gap=" +
                    string.Join("|", observations.ToArray()) +
                    ";awakeRestored=" + awakeRestored +
                    ";awakeEvidence=" + awakeEvidence +
                    (error == null ? "" : ";error=" + error.GetType().Name +
                        ":" + error.Message),
                traveled && awakeRestored,
                "real UnitMoveTo commands on a simultaneous 1d4+1 group; native movement-agent samples and request-local state restoration"));
        }

        private bool StepExpandedSummoningGroundCrowdExpiry(string key)
        {
            try
            {
                if (!_creatureReviewExpiryStarted)
                {
                    BeginExpandedSummoningGroundCrowdExpiry();
                    _creatureReviewExpiryStarted = true;
                    return false;
                }
                Game.Instance.EntityDestroyer.Tick();
                bool allGone = _creatureReviewUnits.All(unit =>
                    unit.Destroyed && unit.View == null &&
                    unit.HoldingState == null);
                TimeSpan gameTime = Game.Instance.Player.GameTime;
                if (!allGone &&
                    gameTime < _creatureReviewExpiryEndTime +
                        TimeSpan.FromSeconds(10) &&
                    DateTime.UtcNow - _creatureReviewExpiryStartUtc <
                        TimeSpan.FromSeconds(180)) return false;
                FinishExpandedSummoningGroundCrowdExpiry(key, null);
                return true;
            }
            catch (Exception exception)
            {
                FinishExpandedSummoningGroundCrowdExpiry(key, exception);
                return true;
            }
        }

        private void BeginExpandedSummoningGroundCrowdExpiry()
        {
            TimeSpan clock = Game.Instance.Player.GameTime;
            BlueprintBuff summoned = BlueprintRoot.Instance.SystemMechanics
                .SummonedUnitBuff;
            Buff[][] markers = _creatureReviewUnits.Select(unit =>
                unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    ReferenceEquals(value.Blueprint, summoned)).ToArray())
                .ToArray();
            if (markers.Any(group => group.Length != 1))
                throw new InvalidOperationException(
                    "Every quantity member needs one exact native summon timer.");
            Buff[] exact = markers.Select(group => group[0]).ToArray();
            _creatureReviewExpiryTimed = exact.All(marker =>
                !marker.IsPermanent &&
                marker.EndTime > clock + TimeSpan.FromSeconds(1));
            _creatureReviewExpiryInitial = "count=" + exact.Length +
                ";timed=" + _creatureReviewExpiryTimed + ";remaining=" +
                string.Join("|", exact.Select(marker =>
                    (marker.EndTime - clock).TotalSeconds.ToString("0.##",
                        CultureInfo.InvariantCulture)).ToArray());
            if (!_creatureReviewExpiryTimed)
                throw new InvalidOperationException(
                    "A quantity member lacks a positive native duration.");
            _creatureReviewExpiryStartTime = clock;
            _creatureReviewExpiryEndTime = exact.Max(marker => marker.EndTime);
            _creatureReviewExpiryStartUtc = DateTime.UtcNow;
            _creatureReviewExpiryWasPaused = Game.Instance.IsPaused;
            if (_creatureReviewExpiryWasPaused) Game.Instance.IsPaused = false;
        }

        private void FinishExpandedSummoningGroundCrowdExpiry(string key,
            Exception error)
        {
            try
            {
                if (_creatureReviewExpiryStartUtc != default(DateTime))
                {
                    Game.Instance.IsPaused = _creatureReviewExpiryWasPaused;
                    _creatureReviewExpiryPauseRestored =
                        Game.Instance.IsPaused == _creatureReviewExpiryWasPaused;
                }
                BlueprintBuff summoned = BlueprintRoot.Instance.SystemMechanics
                    .SummonedUnitBuff;
                int markersLeft = _creatureReviewUnits.Count(unit =>
                    unit.Descriptor != null &&
                    unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value =>
                        ReferenceEquals(value.Blueprint, summoned)));
                int live = _creatureReviewUnits.Count(unit =>
                    !unit.Destroyed || unit.View != null ||
                    unit.HoldingState != null);
                _creatureReviewAssertions.Add(Assertion(
                    "expanded-summoning-ground-crowd-expiry-" + key,
                    "all timed native summon markers expire and all quantity members leave the loaded area without a save write",
                    _creatureReviewExpiryInitial + ";markersLeft=" +
                        markersLeft + ";live=" + live +
                        ";gameElapsed=" + (Game.Instance.Player.GameTime -
                            _creatureReviewExpiryStartTime).TotalSeconds
                            .ToString("0.##", CultureInfo.InvariantCulture) +
                        ";wallElapsed=" + (DateTime.UtcNow -
                            _creatureReviewExpiryStartUtc).TotalSeconds
                            .ToString("0.##", CultureInfo.InvariantCulture) +
                        ";pauseRestored=" +
                        _creatureReviewExpiryPauseRestored +
                        (error == null ? "" : ";error=" +
                            error.GetType().Name + ":" + error.Message),
                    error == null && _creatureReviewExpiryTimed &&
                        _creatureReviewExpiryPauseRestored &&
                        markersLeft == 0 && live == 0,
                    "unpaused native game updates and entity-destruction queue; no clock jump or save write"));
            }
            finally
            {
                foreach (UnitEntityData unit in _creatureReviewUnits)
                    if (!unit.Destroyed || unit.View != null ||
                        unit.HoldingState != null)
                        CleanupExpandedSummoningUnit(unit);
                Game.Instance.EntityDestroyer.Tick();
            }
        }

        private Vector3 FindExpandedSummoningUngulateArtPoint(
            out string survey)
        {
            return FindExpandedSummoningArtPoint(_creatureReviewCaster, out survey);
        }

        // The anchor belongs to the caller's fixture. Other guarded scenarios
        // must not depend on CreatureReview's private initialization state.
        private static Vector3 FindExpandedSummoningArtPoint(UnitEntityData anchorCaster,
            out string survey)
        {
            if (anchorCaster == null) throw new InvalidOperationException("Art review has no fixture anchor.");
            if (AstarPath.active == null) throw new InvalidOperationException("Art review has no native path graph.");
            if (Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance == null)
                throw new InvalidOperationException("Art review has no native sight survey.");
            Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(
                anchorCaster.Position);
            if (anchor.node == null || !anchor.node.Walkable)
                throw new InvalidOperationException(
                    "Ungulate art review has no walkable party anchor.");
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                value != null && value.IsInGame).ToArray();
            Vector3 selected = Vector3.zero;
            int bestClearance = -1, candidates = 0;
            float bestPartyGap = float.MaxValue;
            for (int dx = -15; dx <= 15; dx += 3)
                for (int dz = -15; dz <= 15; dz += 3)
                {
                    Vector3 requested = anchor.clampedPosition +
                        new Vector3(dx, 0f, dz);
                    Pathfinding.NNInfo point = AstarPath.active.GetNearest(
                        requested);
                    if (point.node == null || !point.node.Walkable ||
                        point.node.Area != anchor.node.Area ||
                        point.node.GraphIndex != anchor.node.GraphIndex ||
                        Vector3.Distance(requested, point.clampedPosition) >
                            0.6f) continue;
                    float partyGap = party.Length == 0 ? 0f : party.Min(
                        value => Vector3.Distance(value.Position,
                            point.clampedPosition));
                    if (partyGap < 3.5f) continue;
                    candidates++;
                    int clearance = 0;
                    foreach (Vector3 direction in CompassOffsets)
                    {
                        Vector3 radial = point.clampedPosition +
                            direction * 3f;
                        Pathfinding.NNInfo edge = AstarPath.active.GetNearest(
                            radial);
                        if (edge.node == null || !edge.node.Walkable ||
                            edge.node.Area != anchor.node.Area ||
                            edge.node.GraphIndex != anchor.node.GraphIndex ||
                            Vector3.Distance(radial, edge.clampedPosition) >
                                0.6f || Kingmaker.Visual.FogOfWar
                                .LineOfSightGeometry.Instance.HasObstacle(
                                    point.clampedPosition + Vector3.up * 1.2f,
                                    edge.clampedPosition + Vector3.up * 1.2f,
                                    0)) continue;
                        clearance++;
                    }
                    if (clearance > bestClearance || clearance == bestClearance &&
                        partyGap < bestPartyGap)
                    {
                        selected = point.clampedPosition;
                        bestClearance = clearance;
                        bestPartyGap = partyGap;
                    }
                }
            survey = "candidates=" + candidates + ";clearance=" +
                bestClearance + "/" + CompassOffsets.Length + ";partyGap=" +
                bestPartyGap.ToString("0.##", CultureInfo.InvariantCulture) +
                ";point=" + selected;
            if (bestClearance < 6)
                throw new InvalidOperationException(
                    "No sufficiently open art review floor point: " + survey);
            return selected;
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
                    _creatureReviewExpiryStarted = false;
                    _creatureReviewExpiryTimed = false;
                    _creatureReviewExpiryPauseRestored = false;
                    _creatureReviewExpiryStartUtc = default(DateTime);
                    _creatureReviewExpiryInitial = "<not run>";
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
                    if (!StepExpandedSummoningGroundCrowdPath(key)) return;
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
                        (SerpentineVisualPolicy.IsSnake(key) ? "idle, moving-a, moving-b and post-move" :
                            "idle, moving-a, moving-b and attack") +
                            " captures in frame, lit, intact, rendering wherever the game reports the unit visible and rendering at least once" +
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
                    if (SerpentineVisualPolicy.IsSnake(key) || key == "salamander")
                        RecordSprint17SnakeCrowdOriginals(key);
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
                    if (IsSprint12QuadrupedReviewKey(key) || CrocodilianVisualPolicy.Keys.Contains(key) ||
                        SerpentineVisualPolicy.IsSnake(key) || key == "salamander")
                    {
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-ground-travel-" + key,
                            "native ground move accepted over surveyed connected floor; at least 0.75 m planar travel and nonzero movement-agent velocity",
                            MotionReviewSummary,
                            MotionReviewTravelValid,
                            "native floor survey in the party's own area and graph, native UnitMoveTo and cross-frame position/velocity samples"));
                    }
                    if (IsSprint11UngulateReviewKey(key) || CrocodilianVisualPolicy.Keys.Contains(key))
                    {
                        _creatureReviewAssertions.Add(Assertion(
                            "expanded-summoning-ungulate-travel-" + key,
                            "native ground move accepted; at least 0.75 m planar travel and nonzero movement-agent velocity",
                            MotionReviewSummary,
                            MotionReviewTravelValid,
                            "surveyed connected floor route, native UnitMoveTo and cross-frame position/velocity samples"));
                        if (!_creatureReviewQuantity)
                        {
                            string survey;
                            Vector3 artPoint =
                                FindExpandedSummoningUngulateArtPoint(out survey);
                            PlaceExpandedSummoningUnit(_creatureReviewUnits[0],
                                artPoint);
                            string fileName = key + "-review-overhead.png";
                            string capture = WriteExpandedSummoningOverheadStrikeCapture(
                                _creatureReviewUnits[0], _creatureReviewUnits[0],
                                _request.EvidenceDirectory, fileName, 13f);
                            _creatureReviewAssertions.Add(Assertion(
                                "expanded-summoning-ungulate-overhead-view-" + key,
                                "live overhead frame written for clear silhouette inspection",
                                survey + ";" + capture,
                                capture.StartsWith("png=" + fileName + ";",
                                    StringComparison.Ordinal),
                                "surveyed open floor after native movement proof; request-local camera pose restored; image supports art review, not mechanical proof"));
                            string[] angles = { "north", "east", "south", "west" };
                            Vector3[] offsets = {
                                new Vector3(0f, 9f, 9f),
                                new Vector3(9f, 9f, 0f),
                                new Vector3(0f, 9f, -9f),
                                new Vector3(-9f, 9f, 0f)
                            };
                            var oblique = new List<string>();
                            bool allWritten = true;
                            for (int angle = 0; angle < angles.Length; angle++)
                            {
                                string obliqueName = key + "-review-oblique-" +
                                    angles[angle] + ".png";
                                string result = WriteExpandedSummoningOverheadStrikeCapture(
                                    _creatureReviewUnits[0], _creatureReviewUnits[0],
                                    _request.EvidenceDirectory, obliqueName,
                                    13f, offsets[angle]);
                                oblique.Add(result);
                                allWritten = allWritten && result.StartsWith(
                                    "png=" + obliqueName + ";",
                                    StringComparison.Ordinal);
                            }
                            _creatureReviewAssertions.Add(Assertion(
                                "expanded-summoning-ungulate-oblique-views-" + key,
                                "four live oblique frames written for body and silhouette inspection",
                                survey + ";" + string.Join("|", oblique.ToArray()),
                                allWritten,
                                "four request-local camera poses restored; images support art review, not mechanical proof"));
                        }
                    }
                    if (key == "stirge" && !_creatureReviewQuantity)
                    {
                        _creatureReviewPhase = 6;
                        return;
                    }
                    if (_creatureReviewQuantity)
                    {
                        _creatureReviewSettle = 0;
                        _creatureReviewPhase = 5;
                        return;
                    }
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                        CleanupExpandedSummoningUnit(unit);
                    Game.Instance.EntityDestroyer.Tick();
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 3;
                    return;
                case 5:
                    if (!StepExpandedSummoningGroundCrowdExpiry(key)) return;
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 3;
                    return;
                case 6:
                    if (!StepExpandedSummoningStirgePreyMovement()) return;
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                        CleanupExpandedSummoningUnit(unit);
                    Game.Instance.EntityDestroyer.Tick();
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 3;
                    return;
                default:
                    if (_creatureReviewSettle++ < CreatureReviewCleanupSettleUpdates) return;
                    if (OriginalFlightCleanupPolicy.Handles(key))
                    {
                        if (_originalFlightLifecycleSteps == null)
                            _originalFlightLifecycleSteps = RepeatOriginalFlightLifecycle(variant).GetEnumerator();
                        if (_originalFlightLifecycleSteps.MoveNext()) return;
                        _originalFlightLifecycleSteps.Dispose(); _originalFlightLifecycleSteps = null;
                    }
                    int live = _creatureReviewUnits.Count(value => !value.Destroyed ||
                        value.View != null || value.HoldingState != null);
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-cleanup-" + key, "0",
                        live.ToString(), live == 0,
                        "reviewed summon dismissed and destroyed before the next cast"));
                    if (SerpentineVisualPolicy.IsSnake(key) || key == "salamander")
                        RecordSprint17SnakeCrowdDestruction(key);
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

        /// <summary>Move an attached player-controlled prey with a genuine
        /// UnitMoveTo over surveyed connected floor. This cross-frame check
        /// observes the Stirge's LateUpdate follow without calling it itself.</summary>
        private bool StepExpandedSummoningStirgePreyMovement()
        {
            try
            {
                if (_stirgePreyMove == null)
                {
                    BeginExpandedSummoningStirgePreyMovement();
                    return false;
                }
                UnitEntityData prey = _creatureReviewCaster;
                UnitEntityData stirge = _creatureReviewUnits[0];
                if (prey == null || prey.View == null || stirge == null ||
                    stirge.View == null || stirge.Destroyed)
                    throw new InvalidOperationException(
                        "Attached prey or Stirge disappeared during native movement.");
                float delta = Game.Instance.TimeController.DeltaTime;
                if (delta > 0f)
                {
                    prey.View.MovementAgent.TickMovement(delta);
                    prey.Position = prey.View.transform.position;
                }
                _stirgePreyMoveStarted |= _stirgePreyMove.IsStarted;
                _stirgePreyMoveRunning |= _stirgePreyMove.IsRunning;
                _stirgePreyVelocity = Mathf.Max(_stirgePreyVelocity,
                    prey.View.MovementAgent.Velocity.magnitude);
                _stirgePreyAgentWantsMove |=
                    prey.View.MovementAgent.WantsToMove;
                _stirgePreyTravel = Mathf.Max(_stirgePreyTravel,
                    Vector2.Distance(new Vector2(prey.Position.x, prey.Position.z),
                        new Vector2(_stirgePreyMoveStart.x,
                            _stirgePreyMoveStart.z)));
                _stirgePreyFrames++;
                float gap = Vector2.Distance(new Vector2(prey.Position.x,
                        prey.Position.z), new Vector2(_stirgePreyDestination.x,
                        _stirgePreyDestination.z));
                _stirgePreyMinGap = Mathf.Min(_stirgePreyMinGap, gap);
                if (gap <= 0.9f && _stirgePreyReachedFrame == 0)
                    _stirgePreyReachedFrame = _stirgePreyFrames;
                if (_stirgePreyFrames < 240 &&
                    (_stirgePreyReachedFrame == 0 ||
                     _stirgePreyFrames - _stirgePreyReachedFrame < 3))
                    return false;
                FinishExpandedSummoningStirgePreyMovement(null);
                return true;
            }
            catch (Exception exception)
            {
                FinishExpandedSummoningStirgePreyMovement(exception);
                return true;
            }
        }

        private void BeginExpandedSummoningStirgePreyMovement()
        {
            UnitEntityData prey = _creatureReviewCaster;
            UnitEntityData stirge = _creatureReviewUnits[0];
            _stirgePreyOriginalPosition = prey == null ? Vector3.zero :
                prey.Position;
            _stirgePreyWasPaused = Game.Instance.IsPaused;
            if (prey == null || prey.View == null || stirge == null ||
                stirge.View == null || AstarPath.active == null)
                throw new InvalidOperationException(
                    "Stirge prey movement has no live party, view or navigation graph.");
            Vector3 floor = FindExpandedSummoningUngulateArtPoint(
                out _stirgePreySurvey);
            Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(floor);
            bool destinationFound = false;
            foreach (Vector3 direction in CompassOffsets)
            {
                Vector3 requested = floor + direction * 2.5f;
                Pathfinding.NNInfo point = AstarPath.active.GetNearest(requested);
                if (point.node == null || !point.node.Walkable ||
                    point.node.Area != anchor.node.Area ||
                    point.node.GraphIndex != anchor.node.GraphIndex ||
                    Vector3.Distance(requested, point.clampedPosition) > 0.5f)
                    continue;
                _stirgePreyDestination = point.clampedPosition;
                destinationFound = true;
                break;
            }
            if (!destinationFound)
                throw new InvalidOperationException(
                    "No connected short prey movement route: " +
                    _stirgePreySurvey);
            PlaceExpandedSummoningUnit(prey, floor);
            _stirgePreyRelocated = true;
            PlaceExpandedSummoningUnit(stirge, floor + Vector3.right * 0.6f);
            StirgeAttachComponent attach = StirgeAttachComponent.Find(stirge);
            ItemEntityWeapon touch = stirge.Body.PrimaryHand.MaybeWeapon;
            if (attach == null || touch == null ||
                !attach.TryAttach(prey, touch, true) ||
                !ReferenceEquals(StirgeHoldComponent.AttachedTarget(stirge), prey) ||
                !prey.Descriptor.State.CanMove ||
                !prey.Descriptor.State.CanAct)
                throw new InvalidOperationException(
                    "Stirge did not attach to an otherwise free moving prey.");
            _stirgePreyMoveStart = prey.Position;
            if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
            prey.Commands.InterruptMove();
            UnitMovementAgent agent = prey.View.MovementAgent as UnitMovementAgent;
            if (agent == null)
                throw new InvalidOperationException(
                    "The player prey has no native movement agent.");
            agent.Stop();
            var move = new UnitMoveTo(_stirgePreyDestination, 0.5f);
            move.Init(prey);
            if (!move.CanStart)
                throw new InvalidOperationException(
                    "The attached prey could not start native UnitMoveTo.");
            prey.Commands.Run(move);
            if (!prey.Commands.Contains(move) ||
                !ReferenceEquals(move.Executor, prey))
                throw new InvalidOperationException(
                    "The attached prey did not accept native UnitMoveTo.");
            _stirgePreyMoveAccepted = true;
            _stirgePreyMove = move;
            _stirgePreyTravel = 0f;
            _stirgePreyVelocity = 0f;
            _stirgePreyAgentWantsMove = false;
            _stirgePreyInitialGap = Vector2.Distance(new Vector2(
                    _stirgePreyMoveStart.x, _stirgePreyMoveStart.z),
                new Vector2(_stirgePreyDestination.x,
                    _stirgePreyDestination.z));
            _stirgePreyMinGap = _stirgePreyInitialGap;
            _stirgePreyFrames = 0;
            _stirgePreyReachedFrame = 0;
            _stirgePreyMoveStarted = move.IsStarted;
            _stirgePreyMoveRunning = move.IsRunning;
        }

        private void FinishExpandedSummoningStirgePreyMovement(Exception error)
        {
            UnitEntityData prey = _creatureReviewCaster;
            UnitEntityData stirge = _creatureReviewUnits.Length == 0 ? null :
                _creatureReviewUnits[0];
            string detail = "survey=" + _stirgePreySurvey +
                ";frames=" + _stirgePreyFrames + ";moveAccepted=" +
                _stirgePreyMoveAccepted + ";agentWantsMove=" +
                _stirgePreyAgentWantsMove + ";initialGap=" +
                _stirgePreyInitialGap.ToString("0.##", CultureInfo.InvariantCulture) +
                ";minGap=" + _stirgePreyMinGap.ToString("0.##",
                    CultureInfo.InvariantCulture) + ";commandStarted=" +
                _stirgePreyMoveStarted + ";commandRunning=" +
                _stirgePreyMoveRunning + ";preyTravel=" +
                _stirgePreyTravel.ToString("0.##", CultureInfo.InvariantCulture) +
                ";preyVelocity=" + _stirgePreyVelocity.ToString("0.##",
                    CultureInfo.InvariantCulture);
            bool followed = false, preyFree = false, stirgeTargetable = false;
            bool translocationDetached = false;
            string capture = "<not captured>";
            try
            {
                if (prey != null && stirge != null && !stirge.Destroyed)
                {
                    float distance = Vector2.Distance(new Vector2(
                            prey.Position.x, prey.Position.z), new Vector2(
                            stirge.Position.x, stirge.Position.z));
                    followed = distance >= 0.35f && distance <= 0.9f &&
                        ReferenceEquals(StirgeHoldComponent.AttachedTarget(stirge),
                            prey);
                    preyFree = prey.Descriptor.State.CanMove &&
                        prey.Descriptor.State.CanAct &&
                        !prey.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                        !prey.Descriptor.State.HasCondition(UnitCondition.CantAct) &&
                        prey.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() == null;
                    stirgeTargetable = stirge.IsInGame &&
                        stirge.View != null && stirge.View.IsInGame &&
                        stirge.View.GetComponentsInChildren<Renderer>(true)
                            .Any(renderer => renderer != null && renderer.enabled);
                    detail += ";followGap=" + distance.ToString("0.##",
                        CultureInfo.InvariantCulture);
                    capture = WriteExpandedSummoningPartyCameraCapture(stirge,
                        _request.EvidenceDirectory,
                        "stirge-attached-moving-prey.png");
                    // This connected point is only 2.5 m away. A genuine
                    // Translocate must release the link even below the
                    // per-frame 8 m discontinuity guard.
                    prey.Commands.InterruptMove();
                    PlaceExpandedSummoningUnit(prey, _stirgePreyMoveStart);
                    translocationDetached =
                        StirgeHoldComponent.AttachedTarget(stirge) == null &&
                        !prey.Descriptor.HasFact(
                            StirgeHoldComponent.RemoveAbility) &&
                        prey.Descriptor.State.CanMove &&
                        prey.Descriptor.State.CanAct;
                }
            }
            catch (Exception captureError)
            {
                capture = "error=" + captureError.GetType().Name +
                    ":" + captureError.Message;
            }
            finally
            {
                if (prey != null && prey.Commands != null)
                    prey.Commands.InterruptMove();
                if (stirge != null && !stirge.Destroyed)
                    StirgeHoldComponent.Detach(stirge);
                if (prey != null && _stirgePreyRelocated)
                    PlaceExpandedSummoningUnit(prey,
                        _stirgePreyOriginalPosition);
                Game.Instance.IsPaused = _stirgePreyWasPaused;
                _stirgePreyRelocated = false;
                _stirgePreyMove = null;
            }
            bool restored = prey != null &&
                Vector3.Distance(prey.Position,
                    _stirgePreyOriginalPosition) < 0.08f &&
                Game.Instance.IsPaused == _stirgePreyWasPaused &&
                (stirge == null || StirgeHoldComponent.AttachedTarget(stirge) == null);
            detail += ";followed=" + followed + ";preyFree=" + preyFree +
                ";stirgeTargetable=" + stirgeTargetable +
                ";shortTranslocationDetached=" + translocationDetached +
                ";capture=" + capture + ";restored=" + restored +
                (error == null ? "" : ";error=" + error.GetType().Name +
                    ":" + error.Message);
            _creatureReviewAssertions.Add(Assertion(
                "expanded-summoning-stirge-attached-prey-movement",
                "attached player prey accepts native UnitMoveTo, travels at least 1 m and keeps normal actions while the distinct Stirge follows at a bounded offset",
                detail,
                error == null && _stirgePreyMoveAccepted &&
                    _stirgePreyAgentWantsMove &&
                    _stirgePreyInitialGap - _stirgePreyMinGap >= 1f &&
                    _stirgePreyMinGap <= 0.9f &&
                    _stirgePreyTravel >= 1f &&
                    _stirgePreyVelocity > 0.01f && followed && preyFree &&
                    stirgeTargetable && translocationDetached && restored &&
                    capture.StartsWith("png=stirge-attached-moving-prey.png;",
                        StringComparison.Ordinal),
                "native player UnitMoveTo, cross-frame movement-agent and separate unit/render samples; request-local relocation and pause restored"));
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
