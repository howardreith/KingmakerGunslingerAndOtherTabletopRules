using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// In-game images of any summon for internal review (Phase 1, Sprint 3
    /// onward). The request names creature keys; each is cast, one at a time,
    /// through its real parent chain into the loaded working save, held across
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
        private UnitEntityData[] _creatureReviewUnits = Array.Empty<UnitEntityData>();
        private UnitEntityData _creatureReviewCaster;
        private UnitEntityData[] _creatureReviewParty;
        private object _creatureReviewGameState;
        private BlueprintScriptableObject[] _creatureReviewBlueprints;
        private readonly List<RuntimeTestAssertion> _creatureReviewAssertions =
            new List<RuntimeTestAssertion>();

        /// <summary>
        /// The one variant reviewed per creature key: its own-tier single
        /// under Summon Nature's Ally when the creature has an ally placement,
        /// otherwise under Summon Monster. Unknown keys fail the request.
        /// </summary>
        internal static SummonVariantSpec[] ResolveCreatureReviewVariants(
            string creatures)
        {
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
                SummonFamily family = creature.NaturesAllyTier.HasValue ?
                    SummonFamily.NaturesAlly : SummonFamily.Monster;
                int tier = family == SummonFamily.NaturesAlly ?
                    creature.NaturesAllyTier.Value : creature.MonsterTier.Value;
                SummonVariantSpec variant = ExpandedSummoningCatalog
                    .GenerateVariants(family).Single(value =>
                        value.Creature.Key == key && value.ParentTier == tier &&
                        value.Multiplicity == SummonMultiplicity.One);
                if (!SummonVisibilityCatalog.IsPublished(variant))
                    throw new InvalidOperationException(
                        "A suppressed creature cannot be reviewed through a parent: " +
                        key + ".");
                result.Add(variant);
            }
            return result.ToArray();
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
                foreach (UnitEntityData stale in
                    ExpandedSummoningPersistentUnits(gameState, party))
                    stale.Dispose();
                if (ExpandedSummoningPersistentUnits(gameState, party).Length != 0)
                    throw new InvalidOperationException(
                        "Stale KMG summons could not be removed before the creature review.");
                _creatureReviewParty = party;
                _creatureReviewCaster = caster;
                _creatureReviewGameState = gameState;
                _creatureReviewBlueprints = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                _creatureReviewQueue = ResolveCreatureReviewVariants(
                    (string)_request.Parameters["creatures"]).ToList();
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
                    _creatureReviewUnits = SpawnExpandedSummoningVariants(
                        _creatureReviewBlueprints, _creatureReviewCaster,
                        new[] { variant }, "Creature review of " + key);
                    ResetExpandedSummoningMotionReview(
                        ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)
                            .Replace('.', '_').Replace('-', '_'), key + "-review");
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 1;
                    WriteLifecycleStage("creature-review-" + key + "-summoned");
                    return;
                case 1:
                    if (_creatureReviewSettle++ < CreatureReviewSpawnSettleUpdates) return;
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
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-" + key,
                        "idle, moving-a, moving-b and attack captures in frame, lit, renderer enabled, intact" +
                            (variantRegistered ? "; registered visual variant applied" : ""),
                        MotionReviewSummary + ";visualVariant=" + variantOutcome,
                        MotionReviewValid && variantValid,
                        (variant.Family == SummonFamily.Monster ? "Summon Monster " :
                            "Summon Nature's Ally ") + variant.ParentTier +
                        " single cast through the real parent chain; party-camera renders"));
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
                    WriteLifecycleStage("creature-review-" + key + "-complete");
                    _creatureReviewUnits = Array.Empty<UnitEntityData>();
                    _creatureReviewIndex++;
                    _creatureReviewPhase = 0;
                    return;
            }
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
