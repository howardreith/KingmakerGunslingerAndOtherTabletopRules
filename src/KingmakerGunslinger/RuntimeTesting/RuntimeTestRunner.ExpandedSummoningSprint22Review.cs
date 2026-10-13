using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint22Fixture;
        private IEnumerator<int> _sprint22Steps;
        private Sprint22NativeClock _sprint22Clock;
        private readonly List<RuntimeTestAssertion> _sprint22Assertions = new List<RuntimeTestAssertion>();
        private readonly JArray _sprint22Rows = new JArray();

        /// <summary>
        /// The guarded native-profile load intentionally remains under the
        /// main-menu Dialog mode, whose default gameplay controllers do not
        /// advance commands or GameTime. Drive the same narrow native
        /// controllers used by the repository's save-free combat fixtures,
        /// through an ephemeral Default-mode token, and restore the loaded
        /// fixture's clock exactly after its owned actors are gone.
        /// </summary>
        private sealed class Sprint22NativeClock : IDisposable
        {
            private const BindingFlags Members = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic;
            private readonly ExpandedSummoningCorrectionFixture _fixture;
            private readonly TimeSpan _timeBefore;
            private readonly GameMode _tickMode = new GameMode(
                GameModeType.Default, new IController[0]);
            private readonly UnitActionController _actions =
                new UnitActionController();
            private readonly UnitCombatJoinController _join =
                new UnitCombatJoinController();
            private readonly UnitCombatPrepareController _prepare =
                new UnitCombatPrepareController();
            private readonly Sprint22Cooldowns _cooldowns =
                new Sprint22Cooldowns();
            private readonly Sprint22Buffs _buffs = new Sprint22Buffs();
            private readonly MethodInfo _tickAction;
            private bool _disposed;
            internal int Pulses { get; private set; }
            internal int MovementTicks { get; private set; }
            internal double AdvancedSeconds { get; private set; }
            internal bool Restored { get; private set; }

            internal Sprint22NativeClock(
                ExpandedSummoningCorrectionFixture fixture)
            {
                _fixture = fixture ?? throw new ArgumentNullException("fixture");
                _timeBefore = Game.Instance.Player.GameTime;
                _tickAction = typeof(UnitActionController).GetMethod(
                    "TickOnUnit", Members);
                if (_tickAction == null)
                    throw new MissingMethodException(
                        "Sprint 22 native command tick surface");
            }

            internal void Pulse()
            {
                if (_disposed || Game.Instance.IsPaused) return;
                FieldInfo field = typeof(Game).GetField("m_GameModes", Members);
                var modes = field == null ? null : field.GetValue(
                    Game.Instance) as Stack<GameMode>;
                if (modes == null || modes.Contains(_tickMode))
                    throw new InvalidOperationException(
                        "Sprint 22 native Default-mode scope is ambiguous.");
                GameMode[] before = modes.ToArray();
                bool pause = Game.Instance.IsPaused;
                TimeController time = Game.Instance.TimeController;
                float delta = time.DeltaTime;
                float gameDelta = time.GameDeltaTime;
                double started = Game.Instance.Player.GameTime.TotalSeconds;
                modes.Push(_tickMode);
                try
                {
                    Game.Instance.IsPaused = false;
                    time.SetDeltaTime(.25f);
                    time.SetGameDeltaTime(.25f);
                    new SleepingUnitsController().Tick();
                    _join.Tick();
                    _prepare.Tick();
                    CombatController combat = Game.Instance
                        .TurnBasedCombatController;
                    UnitEntityData[] tickUnits = NativeProfileUnits();
                    if (CombatController.IsInTurnBasedCombat())
                    {
                        UnitEntityData[] uncovered = combat.SortedUnits
                            .Where(value => value != null &&
                                !value.Destroyed && value.IsInState &&
                                value.Descriptor != null &&
                                !tickUnits.Any(owned =>
                                    ReferenceEquals(owned, value)))
                            .ToArray();
                        if (uncovered.Length != 0)
                            throw new InvalidOperationException(
                                "unit=" + string.Join(",", uncovered
                                    .Select(value => value.UniqueId)
                                    .ToArray()) +
                                ";predicate=native-clock-turn-order-coverage" +
                                ";event=turn-based-controller-tick" +
                                ";owner=request-local-native-profile");
                        combat.Tick();
                        combat.TickTime();
                    }
                    else
                    {
                        Game.Instance.Player.GameTime +=
                            TimeSpan.FromSeconds(.25d);
                    }
                    foreach (UnitEntityData unit in tickUnits)
                    {
                        _cooldowns.TickExact(unit);
                        _buffs.TickExact(unit);
                        if (unit.Destroyed || !unit.IsInState ||
                            unit.Descriptor == null) continue;
                        _tickAction.Invoke(_actions, new object[] { unit });
                        // Unity's ordinary Default-mode view update supplies
                        // this native movement step in play.  The guarded
                        // profile is hosted beneath Dialog mode, so its
                        // ephemeral controller scope must drive the same
                        // UnitMovementAgent seam used by the established
                        // creature-motion fixtures.  UnitMoveTo still owns
                        // pathing, speed, contacts and command completion.
                        var view = unit.View;
                        var agent = view == null ? null : view.MovementAgent;
                        if (!unit.Destroyed && agent != null)
                        {
                            agent.TickMovement(.25f);
                            if (!unit.Destroyed && ReferenceEquals(unit.View,
                                view)) unit.Position = view.transform.position;
                            MovementTicks++;
                        }
                    }
                    Pulses++;
                    AdvancedSeconds += Game.Instance.Player.GameTime
                        .TotalSeconds - started;
                }
                finally
                {
                    time.SetDeltaTime(delta);
                    time.SetGameDeltaTime(gameDelta);
                    Game.Instance.IsPaused = pause;
                    if (modes.Count != before.Length + 1 ||
                        !ReferenceEquals(modes.Peek(), _tickMode))
                        throw new InvalidOperationException(
                            "A native controller changed the Sprint 22 " +
                            "request-local mode stack.");
                    modes.Pop();
                    if (!before.SequenceEqual(modes.ToArray()))
                        throw new InvalidOperationException(
                            "Sprint 22 native mode restoration was not exact.");
                }
            }

            /// <summary>
            /// Every actor advanced by the request-local native clock. The
            /// save-free profile's real party anchor remains in native turn
            /// order alongside fixture-created actors, so omitting it leaves
            /// its combat cooldown frozen and prevents the round from ending.
            /// PartyBefore is an exact intake snapshot, not a world census.
            /// </summary>
            private UnitEntityData[] NativeProfileUnits()
            {
                return _fixture.Created.Concat(new[] {
                    _fixture.Caster, _fixture.Hostile }).Concat(
                        _fixture.PartyBefore.OfType<UnitEntityData>())
                    .Where(value => value != null && !value.Destroyed &&
                        value.IsInState && value.Descriptor != null)
                    .Distinct().ToArray();
            }

            internal JObject DescribeTurnCoverage()
            {
                UnitEntityData[] tickUnits = NativeProfileUnits();
                UnitEntityData[] sorted = Game.Instance
                    .TurnBasedCombatController.SortedUnits
                    .Where(value => value != null && !value.Destroyed &&
                        value.IsInState && value.Descriptor != null)
                    .ToArray();
                return new JObject {
                    ["profileParty"] = new JArray(_fixture.PartyBefore
                        .OfType<UnitEntityData>().Select(value =>
                            value.UniqueId)),
                    ["tickUnits"] = new JArray(tickUnits.Select(value =>
                        value.UniqueId)),
                    ["liveSortedUnits"] = new JArray(sorted.Select(value =>
                        value.UniqueId)),
                    ["uncoveredLiveTurnUnits"] = new JArray(sorted
                        .Where(value => !tickUnits.Any(owned =>
                            ReferenceEquals(owned, value)))
                        .Select(value => value.UniqueId))
                };
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Game.Instance.Player.GameTime = _timeBefore;
                Restored = Game.Instance.Player.GameTime == _timeBefore;
                if (!Restored)
                    throw new InvalidOperationException(
                        "Sprint 22 request-local GameTime did not restore.");
            }

            private sealed class Sprint22Cooldowns :
                UnitCombatCooldownsController
            {
                internal void TickExact(UnitEntityData unit)
                { base.TickOnUnit(unit); }
            }

            private sealed class Sprint22Buffs : UnitBuffsController
            {
                internal void TickExact(UnitEntityData unit)
                { base.TickOnUnit(unit); }
            }
        }

        private sealed class Sprint22DurationObserver : IGlobalRulebookHandler<RuleRollDice>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            internal UnitEntityData Owner, Target;
            internal readonly List<RuleRollDice> Rolls=new List<RuleRollDice>();
            internal readonly List<RuleApplyBuff> Applications=new List<RuleApplyBuff>();
            internal readonly List<RuleApplyBuff> OtherTargetApplications=new List<RuleApplyBuff>();
            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            { if (ReferenceEquals(evt.Initiator,Owner) && evt.DiceFormula.Rolls==1 && evt.DiceFormula.Dice==DiceType.D4) Rolls.Add(evt); }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (!ReferenceEquals(evt.Initiator,Target)) return;
                if (Sprint22OwnedControl(evt.Blueprint)) Applications.Add(evt);
                else OtherTargetApplications.Add(evt);
            }
            internal void Clear()
            { Rolls.Clear();Applications.Clear();OtherTargetApplications.Clear(); }
        }

        /// <summary>Read-only sampling of the exact issued command, its native
        /// tail clip and the bounded hit-owned follow-through. The attack
        /// command reports finished at its act cue, before the remaining
        /// presentation has necessarily returned to rest.</summary>
        private sealed class Sprint22TailContactProbe
        {
            private readonly UnitEntityData _owner,_target;
            private readonly SkinnedMeshRenderer _skin,_victim;
            private readonly int[] _clubVertices;
            internal int ActiveClipFrames,ImpactPoseFrames,BestFrame=-1,
                RuleFrame=-1;
            internal bool RuleHandleActed;
            internal float MinimumGap=float.MaxValue;
            internal Vector3 BestClub,BestTargetBounds;
            internal JObject BestAnimation;
            internal string Failure;

            internal Sprint22TailContactProbe(UnitEntityData owner,
                UnitEntityData target)
            {
                _owner=owner;_target=target;
                _skin=owner?.View==null ? null : owner.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .SingleOrDefault(value=>value.sharedMesh!=null &&
                        value.sharedMesh.name=="KMG_ankylosaurus_Original");
                _victim=target?.View==null ? null : target.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(value=>value.enabled && value.sharedMesh!=null)
                    .OrderByDescending(value=>value.bones.Length).FirstOrDefault();
                if(_skin==null || _victim==null)
                { Failure="missing exact original body or native victim skin";_clubVertices=new int[0];return; }
                int club=Array.FindIndex(_skin.bones,value=>value.name=="Tail5_M");
                var weights=_skin.sharedMesh.boneWeights;
                var original=_skin.sharedMesh.vertices;
                _clubVertices=Enumerable.Range(0,weights.Length).Where(index=>
                    weights[index].boneIndex0==club && weights[index].weight0>.9f &&
                    Math.Abs(original[index].x)>.12).ToArray();
                if(_clubVertices.Length==0) Failure="no terminal club surface";
            }

            internal bool Sample(UnitAttack command)
            {
                if(Failure!=null || command==null) return false;
                JObject animation=Sprint17IssuedAnimationObservation(command);
                string action=(string)animation["actionName"];
                string clip=(string)animation["clip"]?["name"];
                bool native=action=="HodagAnimset_Tail" &&
                    clip=="Hodag_attack_tail" &&
                    (bool?)animation["handleStarted"]==true &&
                    (bool?)animation["activeAnimationPresent"]==true;
                if(native) ActiveClipFrames++;
                var impact=_owner.View==null ? null : _owner.View
                    .GetComponent<AnkylosaurusTailImpactVisual>();
                bool presenting=RuleFrame>=0 && impact!=null &&
                    impact.IsPresenting && impact.LastTarget==_target.UniqueId;
                if(presenting) ImpactPoseFrames++;
                if(!native && !presenting) return false;
                Vector3[] world=Sprint17OriginalWorldVertices(_skin);
                Vector3 nearest=world[_clubVertices[0]];
                float gap=Vector3.Distance(nearest,
                    _victim.bounds.ClosestPoint(nearest));
                for(int position=1;position<_clubVertices.Length;position++)
                {
                    Vector3 candidate=world[_clubVertices[position]];
                    float candidateGap=Vector3.Distance(candidate,
                        _victim.bounds.ClosestPoint(candidate));
                    if(candidateGap<gap) { gap=candidateGap;nearest=candidate; }
                }
                if(gap>=MinimumGap) return false;
                MinimumGap=gap;BestFrame=Time.frameCount;BestClub=nearest;
                BestTargetBounds=_victim.bounds.size;BestAnimation=animation;
                return gap<=.5f;
            }

            internal JObject Result(RuleAttackWithWeapon rule,
                UnitAttack command,string supportingFrame)
            {
                var handle=command==null ? null : command.Animation;
                bool exact=Failure==null && ActiveClipFrames>0 &&
                    BestAnimation!=null && handle!=null &&
                    (bool?)BestAnimation["handleStarted"]==true &&
                    RuleHandleActed && command.IsFinished && rule!=null &&
                    ReferenceEquals(rule.Initiator,_owner) &&
                    ReferenceEquals(rule.Target,_target) &&
                    rule.AttackRoll!=null && rule.AttackRoll.IsHit &&
                    RuleFrame>=0;
                var row=new JObject { ["event"]="correlated-native-tail-command",
                    ["ruleFrame"]=RuleFrame,["bestFrame"]=BestFrame,
                    ["unit"]=_owner?.UniqueId,["target"]=_target?.UniqueId,
                    ["weapon"]=rule?.Weapon?.Blueprint?.AssetGuid,
                    ["hit"]=rule?.AttackRoll!=null && rule.AttackRoll.IsHit,
                    ["nativeTailAction"]=exact,["activeClipFrames"]=ActiveClipFrames,
                    ["impactPoseFrames"]=ImpactPoseFrames,
                    ["commandFinished"]=command!=null && command.IsFinished,
                    ["handleActedAtRule"]=RuleHandleActed,
                    ["handleActedAfterCommand"]=handle!=null && handle.IsActed,
                    ["minimumGapMeters"]=float.IsInfinity(MinimumGap) ||
                        MinimumGap==float.MaxValue ? (JToken)JValue.CreateNull() : MinimumGap,
                    ["clubPoints"]=_clubVertices.Length,
                    ["clubSurface"]=SurveyVector(BestClub),
                    ["targetBounds"]=SurveyVector(BestTargetBounds),
                    ["impactGapBefore"]=_owner?.View==null ? -1f :
                        _owner.View.GetComponent<AnkylosaurusTailImpactVisual>()
                            ?.GapBefore ?? -1f,
                    ["impactGapAfter"]=_owner?.View==null ? -1f :
                        _owner.View.GetComponent<AnkylosaurusTailImpactVisual>()
                            ?.GapAfter ?? -1f,
                    ["bestAnimation"]=BestAnimation,
                    ["ruleCorrelatedToCommand"]=RuleFrame>=0,
                    ["bestFrameMinusRuleFrame"]=BestFrame-RuleFrame,
                    ["originalVertexOwnership"]=_skin==null ||
                        _skin.sharedMesh==null ? 0 : _skin.sharedMesh.GetInstanceID(),
                    ["supportingFrame"]=supportingFrame };
                if(Failure!=null) row["failure"]=Failure;
                return row;
            }
        }

        private void Sprint22Check(string predicate, bool passed, JObject row, string expected)
        {
            row["predicate"]=predicate; row["passed"]=passed; row["runId"]=_request.RunId;
            _sprint22Rows.Add(row);
            _sprint22Assertions.Add(Assertion("sprint22-"+predicate,expected,row.ToString(Newtonsoft.Json.Formatting.None),passed,
                "Request-owned Ankylosaurus and victim; resolved native rules. No result or global RNG mutation."));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,"sprint22-review-progress.json"),
                BoundaryObservationPolicy.Serialize(_sprint22Rows));
        }

        private bool PollSprint22NativeReview()
        {
            try
            {
                if (_sprint22Steps == null)
                {
                    _sprint22Fixture=BeginExpandedSummoningCorrectionFixture("KMG_Runtime_Sprint22_Caster");
                    _sprint22Clock=new Sprint22NativeClock(_sprint22Fixture);
                    _sprint22Steps=ReviewSprint22(_sprint22Fixture).GetEnumerator();
                }
                _sprint22Clock.Pulse();
                if (_sprint22Steps.MoveNext()) return false;
            }
            catch (Exception error)
            {
                Sprint22Check("execution",false,new JObject { ["unit"]="ankylosaurus",["event"]="native-review",
                    ["owner"]=_sprint22Fixture?.Caster?.UniqueId,["exception"]=error.ToString() },"Native review completes without exception.");
            }
            CleanupSprint22NativeReview();
            string report=Path.Combine(_request.EvidenceDirectory,"sprint22-review.json");
            RuntimeTestResultWriter.WriteAtomic(report,BoundaryObservationPolicy.Serialize(_sprint22Rows));
            _summoningProfileResult=CreateResult(_sprint22Assertions.All(value=>value.Status==RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,_sprint22Assertions,null);
            _summoningProfileResult.EvidenceFiles.Add(report);
            return true;
        }

        private void CleanupSprint22NativeReview()
        {
            if (_sprint22Steps != null) { _sprint22Steps.Dispose(); _sprint22Steps=null; }
            if (_sprint22Fixture == null)
            {
                if (_sprint22Clock != null)
                { _sprint22Clock.Dispose();_sprint22Clock=null; }
                return;
            }
            bool cleaned=false,clockRestored=false;
            int pulses=_sprint22Clock==null ? 0 : _sprint22Clock.Pulses;
            int movementTicks=_sprint22Clock==null ? 0 :
                _sprint22Clock.MovementTicks;
            double advanced=_sprint22Clock==null ? 0d :
                _sprint22Clock.AdvancedSeconds;
            try
            {
                EndExpandedSummoningCorrectionFixture(_sprint22Fixture,
                    out cleaned);
            }
            finally
            {
                if (_sprint22Clock != null)
                {
                    _sprint22Clock.Dispose();
                    clockRestored=_sprint22Clock.Restored;
                    _sprint22Clock=null;
                }
            }
            Sprint22Check("fixture-restored",cleaned && clockRestored,
                new JObject { ["unit"]="ankylosaurus",
                    ["owner"]=_sprint22Fixture.Caster?.UniqueId,
                    ["event"]="exact-owned-teardown-and-clock-restore",
                    ["referencesRestored"]=cleaned,
                    ["clockRestored"]=clockRestored,
                    ["nativePulses"]=pulses,
                    ["nativeMovementTicks"]=movementTicks,
                    ["nativeAdvancedSeconds"]=advanced },
                "Exact original unit/party/area references and request-local GameTime restored.");
            _sprint22Fixture=null;
        }

        private IEnumerable<int> ReviewSprint22(ExpandedSummoningCorrectionFixture fixture)
        {
            bool originalMode=Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool originalPause=Game.Instance.IsPaused;
            string reviewPhase=RuntimeTestRequestParser.Sprint22ReviewPhase(
                _request);
            if(reviewPhase==null)
                throw new InvalidOperationException(
                    "Sprint 22 native review phase was not validated.");
            bool foundation=reviewPhase=="rtwp-foundation";
            bool[] combatModes=foundation ? new[] { false } :
                new[] { true };
            try
            {
                Sprint22Check("review-phase-"+reviewPhase,true,
                    new JObject { ["unit"]="ankylosaurus",
                        ["event"]="bounded-native-review-selection",
                        ["owner"]=_request.RunId,
                        ["reviewPhase"]=reviewPhase,
                        ["combatModes"]=new JArray(combatModes),
                        ["foundation"]=foundation },
                    "The exact allowlisted bounded review phase is recorded.");
                Game.Instance.IsPaused=true;
                if(foundation)
                {
                    Sprint22PlayerRoutes(fixture);
                    foreach (var family in new[] { SummonFamily.Monster,SummonFamily.NaturesAlly })
                        foreach (var route in ExpandedSummoningCatalog.GenerateVariants(family).Where(value=>value.Creature.Key=="ankylosaurus"))
                        {
                            UnitEntityData[] units=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);
                            fixture.Created.AddRange(units);
                            int low=route.Multiplicity==SummonMultiplicity.One ? 1 : route.Multiplicity==SummonMultiplicity.OneD3 ? 1 : 2;
                            int high=route.Multiplicity==SummonMultiplicity.One ? 1 : route.Multiplicity==SummonMultiplicity.OneD3 ? 3 : 5;
                            Sprint22Check("route-"+route.StableKey,units.Length>=low && units.Length<=high &&
                                units.All(value=>value.Blueprint.AssetGuid=="10c80d5cfa594332bd3e5127799e426f"),new JObject {
                                    ["unit"]="ankylosaurus",["event"]="registered-ability-execution",["route"]=route.StableKey,
                                    ["execution"]=_expandedSummoningLastAbilityExecution,["quantity"]=units.Length,
                                    ["units"]=new JArray(units.Select(Sprint22UnitReadback)) },"Existing route produces only its unchanged unit identity and legal quantity.");
                            DisposeExpandedSummoningUnits(fixture.Created,units);
                            for (int frame=0;frame<8;frame++) yield return 0;
                        }
                }
                CreateExpandedSummoningCorrectionHostile(fixture);
                if(foundation)
                {
                    foreach(int frame in Sprint22VisualLifecycle(fixture)) yield return frame;
                    // The disposable fixture can inherit the owner's persisted
                    // turn-based preference. Crowd movement and native summon
                    // timers need an explicitly activated, unpaused RTWP clock.
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;
                    Game.Instance.TurnBasedCombatController.Activate();
                    Game.Instance.IsPaused=false;
                    for(int frame=0;frame<8;frame++) yield return 0;
                    foreach(int frame in Sprint22CrowdNavigation(fixture)) yield return frame;
                }
                foreach (bool turnBased in combatModes)
                {
                    // Enroll every request-owned combatant under RTWP before
                    // asking the native controller to construct a turn order.
                    // UnitCombatJoinController deliberately skips its scan
                    // while a turn is active, so enabling turn-based mode
                    // before a newly spawned trial owner joins can leave
                    // IsInCombat true while SortedUnits omits that owner.
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;
                    Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused=false;
                    var owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                    SetExpandedSummoningBrainActive(owner,false);
                    for (int frame=0;frame<16;frame++) yield return 0;
                    if(turnBased)
                    {
                        var combat=new JObject();
                        foreach(int frame in Sprint22EnrollTurnBasedOwners(
                            new[] { owner },fixture.Hostile,combat))
                            yield return frame;
                        combat["ownerInCombatAfterSettle"]=owner.IsInCombat;
                        combat["targetInCombatAfterSettle"]=fixture.Hostile.IsInCombat;
                        combat["turnBasedAfterSettle"]=CombatController
                            .IsInTurnBasedCombat();
                        Sprint22Check("turn-based-combat-entry",
                            owner.IsInCombat && fixture.Hostile.IsInCombat &&
                                CombatController.IsInTurnBasedCombat() &&
                                (bool)combat["admitted"],combat,
                            "The request-owned summon and hostile enter the native turn order before command trials.");
                        if(!(bool)combat["admitted"])
                            throw new InvalidOperationException("unit="+
                                owner.UniqueId+
                                ";predicate=turn-based-combat-entry"+
                                ";event=native-turn-order-admission;owner="+
                                owner.Blueprint.AssetGuid+";state="+
                                combat.ToString(Formatting.None));
                    }
                    foreach (int frame in Sprint22SetPowerAttack(owner,false))
                        yield return frame;
                    Sprint22ReadProfile(fixture,owner,turnBased);
                    foreach (int frame in Sprint22PowerAttack(fixture,owner,turnBased))
                        yield return frame;
                    Sprint22NegativeWeapons(fixture,owner,turnBased);
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
                    foreach (string name in new[] { "ordinary-failure","ordinary-success","critical-failure","critical-success",
                        "unconfirmed-threat","miss","daze-immunity","stun-immunity-ordinary","stun-immunity-critical",
                        "daze-immunity-critical","critical-immunity","absorbed-hit","strength-increase","strength-decrease","duplicate-callback" })
                    {
                        var trialOwner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                        SetExpandedSummoningBrainActive(trialOwner,false);
                        foreach (int frame in Sprint22RiderTrial(fixture,trialOwner,name,turnBased)) yield return frame;
                    }
                    owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");SetExpandedSummoningBrainActive(owner,false);
                    foreach (int frame in Sprint22Lifetime(fixture,owner,turnBased)) yield return frame;
                    if (!owner.Destroyed)
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    for (int frame=0;frame<16;frame++) yield return 0;
                }
            }
            finally
            {
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=originalMode;
                Game.Instance.TurnBasedCombatController.Activate();Game.Instance.IsPaused=originalPause;
            }
        }

        private IEnumerable<int> Sprint22SetPowerAttack(UnitEntityData unit,
            bool enabled)
        {
            var toggle=unit.Descriptor.ActivatableAbilities.Enumerable.Single(value=>value.Blueprint.AssetGuid=="a7b339e4f6ff93a4697df5d7a87ff619");
            if(!enabled)
            {
                // Kingmaker's IsOn=false path may first Stop(false), which
                // deliberately leaves an attack-activated buff until its
                // native round boundary.  A later Stop(true) then returns
                // immediately because IsRunning is already false.  Use the
                // engine's forced-stop entry while it still owns the applied
                // buff, then synchronize the public toggle state.
                if(toggle.IsRunning) toggle.Stop(true);
                if(toggle.IsOn) toggle.IsOn=false;
            }
            else if(!toggle.IsOn) toggle.IsOn=true;
            for(int frame=0;frame<32;frame++)
            {
                bool buff=toggle.Blueprint.Buff!=null &&
                    unit.Descriptor.Buffs.GetBuff(toggle.Blueprint.Buff)!=null;
                if(enabled ? toggle.IsOn && toggle.IsRunning && buff :
                    !toggle.IsOn && !toggle.IsRunning && !buff) yield break;
                yield return 0;
            }
        }

        private IEnumerable<int> Sprint22PowerAttack(
            ExpandedSummoningCorrectionFixture fixture,UnitEntityData unit,bool mode)
        {
            var toggle=unit.Descriptor.ActivatableAbilities.Enumerable.Single(value=>value.Blueprint.AssetGuid=="a7b339e4f6ff93a4697df5d7a87ff619");
            var weapon=Sprint19Limbs(unit).Single();
            JObject initial=Sprint22PowerAttackState(unit,toggle);
            foreach(int frame in Sprint22SetPowerAttack(unit,false)) yield return frame;
            JObject offBeforeRules=Sprint22PowerAttackState(unit,toggle);
            int offAttack=Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit,weapon,0)).Result;
            int offDamage=Rulebook.Trigger(new RuleCalculateWeaponStats(unit,weapon,null)).DamageDescription[0].CreateDamage().Bonus;
            JObject offAfterRules=Sprint22PowerAttackState(unit,toggle);
            foreach(int frame in Sprint22SetPowerAttack(unit,true)) yield return frame;
            JObject onBeforeRules=Sprint22PowerAttackState(unit,toggle);
            int onAttack=Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit,weapon,0)).Result;
            int onDamage=Rulebook.Trigger(new RuleCalculateWeaponStats(unit,weapon,null)).DamageDescription[0].CreateDamage().Bonus;
            JObject onAfterRules=Sprint22PowerAttackState(unit,toggle);
            bool running=toggle.IsOn && toggle.IsRunning && toggle.Blueprint.Buff!=null &&
                unit.Descriptor.Buffs.GetBuff(toggle.Blueprint.Buff)!=null;
            Sprint22Check("power-attack-"+(mode?"turn-based":"rtwp"),running && offAttack==14 && offDamage==12 && onAttack==12 && onDamage==18,
                new JObject { ["unit"]=unit.UniqueId,["event"]="native-power-attack-toggle-and-weapon-rules",["owner"]=toggle.Blueprint.AssetGuid,
                    ["initial"]=initial,["offBeforeRules"]=offBeforeRules,
                    ["offAfterRules"]=offAfterRules,["onBeforeRules"]=onBeforeRules,
                    ["onAfterRules"]=onAfterRules,["offAttack"]=offAttack,
                    ["onAttack"]=onAttack,["offDamage"]=offDamage,
                    ["onDamage"]=onDamage,["running"]=running },
                "BAB7 native Power Attack contributes -2 attack and its native +4 plus the exact Anky-only +2 correction; printed line is measured after native stop with it off.");
            foreach(int frame in Sprint22SetPowerAttack(unit,false)) yield return frame;
        }

        private static JObject Sprint22PowerAttackState(UnitEntityData unit,
            Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbility toggle)
        {
            Buff[] buffs=toggle.Blueprint.Buff==null ? new Buff[0] :
                unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>
                    ReferenceEquals(value.Blueprint,toggle.Blueprint.Buff)).ToArray();
            return new JObject { ["isOn"]=toggle.IsOn,
                ["isRunning"]=toggle.IsRunning,["buffCount"]=buffs.Length,
                ["activeBuffCount"]=buffs.Count(value=>value.Active),
                ["buff"]=toggle.Blueprint.Buff?.AssetGuid };
        }

        private void Sprint22NegativeWeapons(ExpandedSummoningCorrectionFixture fixture,UnitEntityData unit,bool mode)
        {
            var target=fixture.Hostile;var before=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            var bite=fixture.Blueprints.OfType<BlueprintItemWeapon>().First(value=>value.Type!=null && value.Type.Category==WeaponCategory.Bite);
            var foreignFeature=fixture.Blueprints.OfType<BlueprintFeature>().Single(value=>value.AssetGuid=="e8a085864606409ba25dc123f14d27a2");
            var added=fixture.Caster.Descriptor.AddFact(foreignFeature);
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            try
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=-100;
                foreach(bool wrongOwner in new[] {false,true})
                {
                    var source=wrongOwner ? fixture.Caster : unit;
                    var weapon=wrongOwner ? new ItemEntityWeapon(Sprint19Limbs(unit).Single().Blueprint) : new ItemEntityWeapon(bite);
                    int oldAttack=source.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                    var saves=new Sprint19SaveObserver {Target=target};EventBus.Subscribe(saves);
                    try
                    {
                        source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                        RuleAttackWithWeapon strike=null;var trials=new JArray();
                        for(int trial=1;trial<=12;trial++)
                        {
                            saves.Saves.Clear();target.Descriptor.Damage=fixture.HostileDamage;
                            strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,target,weapon,0));
                            trials.Add(new JObject { ["trial"]=trial,["d20"]=strike.AttackRoll.Roll.Value,["hit"]=strike.AttackRoll.IsHit });
                            if(strike.AttackRoll.IsHit) break;
                        }
                        bool same=before.SequenceEqual(target.Descriptor.Buffs.RawFacts.OfType<Buff>());
                        Sprint22Check((wrongOwner?"foreign-owner":"other-weapon")+"-"+(mode?"turn-based":"rtwp"),strike.AttackRoll.IsHit && saves.Saves.Count==0 && same,
                            new JObject { ["unit"]=source.UniqueId,["event"]="negative-native-strike",["owner"]=source.Blueprint.AssetGuid,
                                ["weapon"]=weapon.Blueprint.AssetGuid,["trials"]=trials,["saves"]=saves.Saves.Count,["buffReferencesUnchanged"]=same },
                            "A native hit with a foreign weapon or the signature feature on a foreign owner produces no rider or save.");
                    }
                    finally { source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;EventBus.Unsubscribe(saves); }
                }
            }
            finally { fixture.Caster.Descriptor.RemoveFact(added);target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort; }
        }

        private IEnumerable<int> Sprint22VisualLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            var native=fixture.Blueprints.OfType<BlueprintUnit>().Single(value=>value.AssetGuid==AnkylosaurusVisualPolicy.DonorGuid);
            var control=Game.Instance.EntityCreator.SpawnUnit(native,fixture.Caster.Position,Quaternion.identity,fixture.Scene);
            fixture.Created.Add(control);SetExpandedSummoningBrainActive(control,false);
            var donor=control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value=>value.sharedMesh).ToArray();
            try
            {
                for(int cycle=0;cycle<3;cycle++)
                {
                    int reached=0;UnitEntityData unit;
                    try
                    {
                        if(cycle==0) ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=()=>{ reached++;throw new InvalidOperationException("Sprint22 request-owned post-swap fault"); };
                        unit=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                    }
                    finally { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null; }
                    SetExpandedSummoningBrainActive(unit,false);
                    for(int frame=0;frame<30;frame++) yield return 0;
                    var view=unit.View;var skins=view.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var skin=skins.Single(value=>value.name=="hodag");
                    var attachment=ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch),"Applied",view);
                    var owned=ExpandedSummoningPteranodonViewPatch.CaptureOriginalFlightOwnedResources(view);
                    var borrowed=attachment==null ? donor : new[] {ResourceField(attachment,"OriginalMesh") as Mesh};
                    var borrowedMaterials=attachment==null ? skin.sharedMaterials : (Material[])ResourceField(attachment,"OriginalMaterials");
                    var controller=skin.GetComponentInParent<Kingmaker.Visual.MaterialEffects.StandardMaterialController>();
                    var driven=controller==null ? null : ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
                    bool original=skin.sharedMesh!=null && skin.sharedMesh.name=="KMG_ankylosaurus_Original";
                    bool bound=cycle==0 ? reached==1 && !original && skin.enabled && donor.Contains(skin.sharedMesh) :
                        reached==0 && original && owned.OfType<Mesh>().Count()==1 && owned.OfType<Material>().Count()==2 &&
                        driven!=null && driven.Contains(skin.sharedMaterial) && EntityFadedIn(unit);
                    Sprint22Check("view-cycle-"+cycle,bound,new JObject { ["unit"]=unit.UniqueId,["event"]="view-fault-recovery-and-native-fader",
                        ["owner"]=unit.Blueprint.AssetGuid,["faultReached"]=reached,["original"]=original,["fadedIn"]=EntityFadedIn(unit),
                        ["owned"]=new JArray(owned.Select(value=>new JObject { ["id"]=value.GetInstanceID(),["name"]=value.name,["type"]=value.GetType().FullName })),
                        ["status"]=ExpandedSummoningPteranodonViewPatch.DescribeView(view) },"Fault preserves native donor; subsequent instances bind one private mesh/two materials through the native fader and material controller.");
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {unit});
                    int waited=0;
                    for(;waited<60;waited++)
                    { Game.Instance.EntityDestroyer.Tick();yield return 0;if(unit.Destroyed && unit.View==null && unit.HoldingState==null && owned.All(value=>value==null)) break; }
                    var caches=RosterImmutableCaches();
                    bool released=unit.Destroyed && unit.View==null && unit.HoldingState==null && owned.All(value=>value==null);
                    bool safe=borrowed.All(value=>value!=null) && borrowedMaterials.All(value=>value!=null) && caches.Count>0;
                    ExpandedSummoningPteranodonViewPatch.ReleaseOriginalFlightView(view);
                    Sprint22Check("view-teardown-"+cycle,released && safe,new JObject { ["unit"]=unit.UniqueId,["event"]="native-OnDestroy-deferred-resource-release",
                        ["owner"]="exact-per-view-resources",["waitedFrames"]=waited,["released"]=released,["borrowedSurvive"]=safe,
                        ["cacheIds"]=new JArray(caches.Select(value=>new JObject { ["id"]=value.Key,["cache"]=value.Value })) },"Native destruction releases captured private objects within 60 frames and preserves borrowed/cache identities; repeat release is harmless.");
                }
                bool isolated=control.View!=null && ExpandedSummoningPteranodonViewPatch.DescribeView(control.View)=="not-attempted" &&
                    donor.SequenceEqual(control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value=>value.sharedMesh)) && donor.All(value=>value!=null);
                Sprint22Check("native-hodag-isolation",isolated,new JObject { ["unit"]=control.UniqueId,["event"]="donor-negative-control",
                    ["owner"]=native.AssetGuid,["unchanged"]=isolated },"The unrelated native Hodag keeps its exact renderer/mesh references across faults and recovered Ankylosaurus instances.");
            }
            finally { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null;DisposeExpandedSummoningUnits(fixture.Created,new[] {control}); }
        }

        private IEnumerable<int> Sprint22CrowdNavigation(ExpandedSummoningCorrectionFixture fixture)
        {
            var route=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Single(value=>value.Creature.Key=="ankylosaurus" && value.ParentTier==7);
            UnitEntityData[] units=null;var quantities=new JArray();
            for(int trial=1;trial<=12;trial++)
            {
                units=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);fixture.Created.AddRange(units);
                foreach(var unit in units)
                {
                    SetExpandedSummoningBrainActive(unit,false);
                    unit.View.SetVisible(true,true);
                }
                quantities.Add(units.Length);if(units.Length==5) break;
                DisposeExpandedSummoningUnits(fixture.Created,units);for(int frame=0;frame<8;frame++) yield return 0;
            }
            if(units.Length!=5) throw new InvalidOperationException("Ankylosaurus crowd quantity-five setup unsatisfied: "+quantities);
            var selected=Game.Instance.UI.SelectionManagerPC.SelectedUnits.ToArray();
            var awake=Game.Instance.State.AwakeUnits.ToArray();bool paused=Game.Instance.IsPaused;
            var parking=units.Select(value=>value.Position).ToArray();
            var markers=units.Select(value=>value.Descriptor.Buffs.RawFacts.OfType<Buff>().Single(
                buff=>ReferenceEquals(buff.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))).ToArray();
            var owned=units.SelectMany(value=>ExpandedSummoningPteranodonViewPatch
                .CaptureOriginalFlightOwnedResources(value.View)).ToArray();
            var ends=markers.Select(value=>value.EndTime.TotalSeconds).ToArray();
            bool crowdCombat=false,hostileJoined=false;
            try
            {
                // The native profile is hosted under a Dialog-mode loader.
                // Explicit native visibility gives these real spawned views
                // the same live click/target surfaces they have in gameplay;
                // it does not make an AI-controlled summon controllable.
                for(int frame=0;frame<4;frame++) yield return 0;
                var selectionRows=new JArray(units.Select(Sprint22SelectionSurface));
                bool selection=selectionRows.All(value=>(bool)value["selectionSurface"]);
                var anchor=AstarPath.active.GetNearest(fixture.Caster.Position);
                if(anchor.node==null || !anchor.node.Walkable)
                    throw new InvalidOperationException("No walkable party floor for the Ankylosaurus crowd review.");
                var graph=AstarPath.active.graphs[(int)anchor.node.GraphIndex] as Pathfinding.IRaycastableGraph;
                if(graph==null) throw new InvalidOperationException("The Ankylosaurus crowd floor cannot verify direct native routes.");
                var party=Game.Instance.Player.Party.Where(value=>value!=null && value.IsInGame).ToArray();
                Pathfinding.NNInfo chosenOrigin=default(Pathfinding.NNInfo),chosenDestination=default(Pathfinding.NNInfo);
                Vector3 chosenCenter=Vector3.zero,chosenHeading=Vector3.zero;
                float chosenHalfLength=0f;int centers=0,routes=0;bool routeFound=false;
                var centerOffsets=(from dx in Enumerable.Range(-30,61) from dz in Enumerable.Range(-30,61)
                    select new Vector3(dx,0f,dz)).OrderBy(value=>value.sqrMagnitude).ToArray();
                foreach(Vector3 centerOffset in centerOffsets)
                    if(!routeFound)
                    {
                        Vector3 requestedCenter=anchor.clampedPosition+centerOffset;
                        var center=AstarPath.active.GetNearest(requestedCenter);
                        if(center.node==null || !center.node.Walkable || center.node.Area!=anchor.node.Area ||
                            center.node.GraphIndex!=anchor.node.GraphIndex || Vector3.Distance(requestedCenter,center.clampedPosition)>.6f ||
                            party.Any(value=>Vector3.Distance(value.Position,center.clampedPosition)<3.5f)) continue;
                        centers++;
                        foreach(Vector3 heading in CompassOffsets)
                        {
                            foreach(float halfLength in new[] {6f,5f,4f,3f,2.5f})
                            {
                                routes++;
                                Vector3 requestedOrigin=center.clampedPosition-heading*halfLength;
                                Vector3 requestedDestination=center.clampedPosition+heading*halfLength;
                                var origin=AstarPath.active.GetNearest(requestedOrigin);
                                var destination=AstarPath.active.GetNearest(requestedDestination);
                                float required=Math.Max(4f,units.Max(value=>value.Corpulence)*2f+1f);
                                bool usable=origin.node!=null && destination.node!=null && origin.node.Walkable && destination.node.Walkable &&
                                    origin.node.Area==anchor.node.Area && destination.node.Area==anchor.node.Area &&
                                    origin.node.GraphIndex==anchor.node.GraphIndex && destination.node.GraphIndex==anchor.node.GraphIndex &&
                                    Vector3.Distance(requestedOrigin,origin.clampedPosition)<=.6f &&
                                    Vector3.Distance(requestedDestination,destination.clampedPosition)<=.6f &&
                                    Vector3.Distance(origin.clampedPosition,destination.clampedPosition)>=required &&
                                    party.All(value=>Vector3.Distance(value.Position,origin.clampedPosition)>=2.5f &&
                                        Vector3.Distance(value.Position,destination.clampedPosition)>=2.5f) &&
                                    Sprint9FlightLineClear(graph,origin,destination);
                                if(!usable) continue;
                                chosenOrigin=origin;chosenDestination=destination;chosenCenter=center.clampedPosition;
                                chosenHeading=heading;chosenHalfLength=halfLength;routeFound=true;break;
                            }
                            if(routeFound) break;
                        }
                    }
                if(!routeFound) throw new InvalidOperationException("No connected direct floor route for the Ankylosaurus crowd: area="+
                    anchor.node.Area+";graph="+anchor.node.GraphIndex+";centers="+centers+";routes="+routes);
                // The native first-level scene is a real, irregular floor rather
                // than an invented arena. Keep all five summons alive with their
                // native click/target surfaces, move each Huge body over the same surveyed route, then
                // use two members head-on for native contact/avoidance evidence.
                Game.Instance.IsPaused=false;int frames=0;bool navigated=true;var rows=new JArray();
                for(int i=0;i<units.Length;i++)
                {
                    var unit=units[i];
                    var origin=i%2==0 ? chosenOrigin : chosenDestination;
                    var destination=i%2==0 ? chosenDestination : chosenOrigin;
                    PlaceExpandedSummoningUnit(unit,origin.clampedPosition);Vector3 movementOrigin=unit.Position;
                    if(!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    for(int settle=0;settle<2;settle++) yield return 0;
                    var move=new UnitMoveTo(destination.clampedPosition,.5f);move.Init(unit);bool canStart=move.CanStart;
                    float maximumSpeed=0f;int maximumContacts=0;var movement=System.Diagnostics.Stopwatch.StartNew();
                    if(canStart) unit.Commands.Run(move);
                    while(canStart && !move.IsFinished && !unit.Destroyed &&
                        unit.View!=null && unit.View.MovementAgent!=null &&
                        movement.Elapsed.TotalSeconds<20)
                    {
                        var agent=unit.View.MovementAgent;maximumSpeed=Math.Max(maximumSpeed,agent.Velocity.magnitude);
                        maximumContacts=Math.Max(maximumContacts,agent.UnitContacts==null ? 0 : agent.UnitContacts.Count);
                        frames++;yield return 0;
                    }
                    float travel=Vector3.Distance(movementOrigin,unit.Position);
                    float gap=Vector3.Distance(destination.clampedPosition,unit.Position);
                    var finalView=unit.View;var finalAgent=finalView==null ? null : finalView.MovementAgent;
                    bool member=canStart && move.IsFinished && !unit.Destroyed &&
                        finalAgent!=null && unit.Descriptor.State.Size==Size.Huge &&
                        travel>=1f && gap<=1.5f && maximumSpeed>.01f;
                    navigated&=member;
                    rows.Add(new JObject { ["unit"]=unit.UniqueId,["size"]=unit.Descriptor.State.Size.ToString(),
                        ["corpulence"]=unit.Corpulence,["origin"]=SurveyVector(movementOrigin),["destination"]=SurveyVector(destination.clampedPosition),
                        ["travel"]=travel,["gap"]=gap,["maxVelocity"]=maximumSpeed,["maximumNativeContacts"]=maximumContacts,
                        ["agentCorpulence"]=finalAgent==null ? (JToken)JValue.CreateNull() : finalAgent.Corpulence,
                        ["avoidanceDisabled"]=finalAgent==null ? (JToken)JValue.CreateNull() : finalAgent.AvoidanceDisabled,
                        ["destroyed"]=unit.Destroyed,["viewPresent"]=finalView!=null,
                        ["canStart"]=canStart,["commandFinished"]=move.IsFinished,["wallSeconds"]=movement.Elapsed.TotalSeconds });
                    if(!unit.Destroyed && unit.View!=null)
                    { unit.Commands.InterruptAll();PlaceExpandedSummoningUnit(unit,parking[i]); }
                }
                Sprint22Check("huge-crowd-navigation-selection",selection && navigated,new JObject { ["unit"]="ankylosaurus",["event"]="native-five-member-move-and-selection",
                    ["owner"]="request-owned-quantity-group",["quantityTrials"]=quantities,["selectionSurfaces"]=selectionRows,["selection"]=selection,["members"]=rows,["frames"]=frames,
                    ["floorSearch"]=new JObject { ["anchor"]=SurveyVector(anchor.clampedPosition),
                        ["center"]=SurveyVector(chosenCenter),["heading"]=SurveyVector(chosenHeading),["halfLength"]=chosenHalfLength,
                        ["centers"]=centers,["routes"]=routes } },
                    "All five Huge summons retain live native click/target selection colliders and every member completes a surveyed native route; base-game summons remain AI-controlled.");

                var pair=new[] {units[0],units[1]};var pairStarts=new[] {chosenOrigin.clampedPosition,chosenDestination.clampedPosition};
                var pairEnds=new[] {chosenDestination.clampedPosition,chosenOrigin.clampedPosition};var pairMoves=new UnitMoveTo[2];
                var pairContacts=new int[2];float minimumCenterDistance=float.MaxValue;
                foreach(var unit in pair)
                {
                    unit.Memory.Add(fixture.Hostile);
                    if(!unit.IsInCombat) unit.JoinCombat();
                }
                fixture.Hostile.Memory.Add(pair[0]);
                hostileJoined=!fixture.Hostile.IsInCombat;
                if(hostileJoined) fixture.Hostile.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();crowdCombat=true;
                for(int i=0;i<2;i++) { pair[i].Commands.InterruptAll();PlaceExpandedSummoningUnit(pair[i],pairStarts[i]); }
                for(int settle=0;settle<4;settle++) yield return 0;
                bool pairCanStart=true;
                for(int i=0;i<2;i++) { pairMoves[i]=new UnitMoveTo(pairEnds[i],.5f);pairMoves[i].Init(pair[i]);
                    pairCanStart&=pairMoves[i].CanStart;if(pairMoves[i].CanStart) pair[i].Commands.Run(pairMoves[i]); }
                float initialCenterDistance=Vector3.Distance(pair[0].Position,pair[1].Position);
                var collisionClock=System.Diagnostics.Stopwatch.StartNew();int collisionFrames=0;
                while(pairCanStart && collisionClock.Elapsed.TotalSeconds<20)
                {
                    for(int i=0;i<2;i++) { var contacts=pair[i].View.MovementAgent.UnitContacts;
                        pairContacts[i]=Math.Max(pairContacts[i],contacts==null ? 0 : contacts.Count); }
                    minimumCenterDistance=Math.Min(minimumCenterDistance,Vector3.Distance(pair[0].Position,pair[1].Position));
                    collisionFrames++;if(pairMoves.All(value=>value.IsFinished)) break;yield return 0;
                }
                bool collision=pairCanStart && pairContacts.Any(value=>value>0) &&
                    initialCenterDistance-minimumCenterDistance>=1f && pair.All(value=>!value.View.MovementAgent.AvoidanceDisabled &&
                        value.Corpulence>0 && Math.Abs(value.Corpulence-value.View.MovementAgent.Corpulence)<.001f);
                Sprint22Check("huge-collision-contact",collision,new JObject { ["unit"]="ankylosaurus",["event"]="head-on-native-movement-contacts",
                    ["owner"]="request-owned-quantity-group-members-0-1",["members"]=new JArray(pair.Select((value,i)=>new JObject {
                        ["unit"]=value.UniqueId,["contacts"]=pairContacts[i],["commandFinished"]=pairMoves[i].IsFinished,
                        ["travel"]=Vector3.Distance(pairStarts[i],value.Position),["unitCorpulence"]=value.Corpulence,
                        ["agentCorpulence"]=value.View.MovementAgent.Corpulence,["inCombat"]=value.IsInCombat })),
                    ["canStart"]=pairCanStart,["initialCenterDistance"]=initialCenterDistance,["minimumCenterDistance"]=minimumCenterDistance,
                    ["finalCenterDistance"]=Vector3.Distance(pair[0].Position,pair[1].Position),["frames"]=collisionFrames,
                    ["wallSeconds"]=collisionClock.Elapsed.TotalSeconds },
                    "Two Huge members approach head-on on the surveyed route, produce native contacts and retain avoidance/corpulence.");
                foreach(var unit in pair) if(unit.IsInCombat) unit.LeaveCombat();
                if(hostileJoined && fixture.Hostile.IsInCombat) fixture.Hostile.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();crowdCombat=false;
                for(int i=0;i<units.Length;i++) { units[i].Commands.InterruptAll();PlaceExpandedSummoningUnit(units[i],parking[i]); }
                var expiry=System.Diagnostics.Stopwatch.StartNew();
                while(units.Any(value=>!value.Destroyed || value.View!=null || value.HoldingState!=null) && expiry.Elapsed.TotalSeconds<150)
                { Game.Instance.EntityDestroyer.Tick();yield return 0; }
                bool gone=units.All(value=>value.Destroyed && value.View==null && value.HoldingState==null) && owned.All(value=>value==null);
                Sprint22Check("native-quantity-expiry",gone,new JObject { ["unit"]="ankylosaurus",["event"]="natural-summon-timer-expiry",["owner"]="five-native-summon-timers",
                    ["endSeconds"]=new JArray(ends),["nativeNow"]=Game.Instance.Player.GameTime.TotalSeconds,["gone"]=gone,["wallSeconds"]=expiry.Elapsed.TotalSeconds },
                    "All five native summon timers expire naturally; units, views, holding state and captured private resources disappear without timer mutation.");
            }
            finally
            {
                if(crowdCombat)
                {
                    foreach(var unit in units) if(unit.IsInCombat) unit.LeaveCombat();
                    if(hostileJoined && fixture.Hostile.IsInCombat) fixture.Hostile.LeaveCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                }
                foreach(var unit in units) { if(!awake.Contains(unit)) Game.Instance.State.AwakeUnits.Remove(unit);unit.Commands.InterruptAll(); }
                Game.Instance.UI.SelectionManagerPC.MultiSelect(selected.Where(value=>value.View!=null).Select(value=>value.View).ToArray(),false);
                Game.Instance.IsPaused=paused;DisposeExpandedSummoningUnits(fixture.Created,units);
            }
        }

        private static JObject Sprint22SelectionSurface(UnitEntityData unit)
        {
            var view=unit==null ? null : unit.View;
            var soft=view==null ? null : ReadExpandedSummoningOptionalMember(
                view,"m_SoftCollider") as CapsuleCollider;
            var core=view==null ? null : ReadExpandedSummoningOptionalMember(
                view,"m_CoreCollider") as MeshCollider;
            // Native donors do not all keep ClickableObject on the view root
            // (Mastodon places it below UnitEntityView).  Selection raycasts
            // the hierarchy, so inspect that same hierarchy rather than
            // rejecting an otherwise native selection surface by root shape.
            Component clickable=view==null ? null : view
                .GetComponentsInChildren<Component>(true).FirstOrDefault(
                    value=>value!=null && value.GetType().FullName==
                        "Kingmaker.UI.Selection.ClickableObject");
            Vector2 bounds=Vector2.zero,coreBounds=Vector2.zero;
            bool derived=false;
            try
            {
                if(view!=null)
                {
                    bounds=view.CameraOrientedBoundsSize;
                    coreBounds=view.CameraOrientedCoreBoundsSize;
                    derived=bounds.x>0 && bounds.y>0 && coreBounds.x>0 &&
                        coreBounds.y>0;
                }
            }
            catch { derived=false; }
            bool valid=view!=null && soft!=null && core!=null && clickable!=null &&
                soft.enabled && soft.gameObject.activeInHierarchy &&
                soft.radius>0 && soft.height>0 && core.enabled &&
                core.gameObject.activeInHierarchy &&
                derived && unit.Corpulence>0;
            return new JObject { ["unit"]=unit?.UniqueId,
                ["selectionSurface"]=valid,["directlyControllable"]=unit!=null && unit.IsDirectlyControllable,
                ["softCollider"]=soft==null ? null : soft.GetType().FullName,
                ["softEnabled"]=soft!=null && soft.enabled,
                ["softRadius"]=soft==null ? 0 : soft.radius,
                ["softHeight"]=soft==null ? 0 : soft.height,
                ["coreCollider"]=core==null ? null : core.GetType().FullName,
                ["coreEnabled"]=core!=null && core.enabled,
                ["clickable"]=clickable==null ? null : clickable.GetType().FullName,
                ["cameraBounds"]=SurveyVector(new Vector3(bounds.x,bounds.y,0)),
                ["cameraCoreBounds"]=SurveyVector(new Vector3(coreBounds.x,coreBounds.y,0)),
                ["corpulence"]=unit==null ? 0 : unit.Corpulence };
        }

        private static JObject Sprint22UnitReadback(UnitEntityData unit)
        {
            return new JObject { ["uid"]=unit.UniqueId,["blueprint"]=unit.Blueprint.AssetGuid,
                ["size"]=unit.Descriptor.State.Size.ToString(),["canAct"]=unit.Descriptor.State.CanAct,
                ["facts"]=new JArray(unit.Descriptor.Progression.Features.Enumerable.Select(value=>value.Blueprint.name)),
                ["buffs"]=new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(value=>new JObject {
                    ["guid"]=value.Blueprint.AssetGuid,["name"]=value.Blueprint.name,["endSeconds"]=value.EndTime.TotalSeconds,
                    ["leftSeconds"]=value.TimeLeft.TotalSeconds })) };
        }

        private void Sprint22PlayerRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            object levelController=null;
            var castRule=typeof(RuleCastSpell).GetMethod("OnTrigger");
            var spawnAction=typeof(ContextActionSpawnMonster).GetMethod("RunAction");
            const BindingFlags statics=BindingFlags.NonPublic|BindingFlags.Static;
            var hooks=HarmonyInstance.Create("kmg.sprint22.routes."+_request.RunId);
            try
            {
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue=30;
                var wizard=BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e","native Wizard targeted spellbook path");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor,wizard,20,ref levelController);
                var book=fixture.Caster.Descriptor.GetSpellbook(wizard);
                while(book.CasterLevel<20) book.AddCasterLevel();book.UpdateAllSlotsSize(false);book.Rest();
                var parents=ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid=>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,guid,"canonical summon parent")).ToArray();
                hooks.Patch(castRule,null,new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix",statics)),null);
                hooks.Patch(spawnAction,new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix",statics)),null,null);
                _expandedSummoningPlayerPathCaptureActive=true;
                var routes=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                    .Where(value=>value.Creature.Key=="ankylosaurus").ToArray();
                if(routes.Length!=10) throw new InvalidOperationException("Ankylosaurus published route set changed.");
                foreach(var route in routes)
                {
                    var before=SnapshotReferences(fixture.AllUnits);
                    var cases=new List<ExpandedSummoningPlayerPathCase>();
                    AddExpandedSummoningPlayerPathRootCase(cases,fixture.Blueprints,fixture.Caster,book,parents,route,
                        ExpandedSummoningPlayerPathAlignmentFor(route),route.Family==SummonFamily.NaturesAlly
                            ? (SummonAlignmentMode?)SummonAlignmentMode.Caster : SummonAlignmentMode.Celestial,
                        fixture.SceneEntities,fixture.AllUnits,route.StableKey);
                    var item=cases.Single();var durations=ExpandedSummoningRuleDurationCapture.ToArray();
                    bool duration=durations.Length==item.LiveCount && durations.All(value=>Math.Abs(value.BaseDuration.TotalSeconds-120d)<=.001 &&
                        value.BonusDuration.TotalSeconds>=0 && value.Unit!=null && value.Unit.Destroyed);
                    bool exact=item.LiveContract && item.SlotContract && item.CommandStarted && item.CommandResult=="Success" &&
                        item.RuleCastCount==1 && item.SpawnActionCount==1 && item.QuantityContract && duration && before.SequenceEqual(SnapshotReferences(fixture.AllUnits));
                    Sprint22Check("player-route-"+route.StableKey,exact,new JObject { ["unit"]="ankylosaurus",["event"]="native-parent-selected-variant-spellbook",
                        ["owner"]=route.StableKey,["detail"]=item.Describe(),["baseDurations"]=new JArray(durations.Select(value=>value.BaseDuration.TotalSeconds)),
                        ["slotContract"]=item.SlotContract,["quantityContract"]=item.QuantityContract,["durationContract"]=duration },
                        "Each existing selected parent consumes one native spellbook slot and one cast/spawn action, preserves template and quantity, and carries CL20 duration with exact cleanup.");
                }
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive=false;hooks.UnpatchAll(hooks.Id);ExpandedSummoningPlayerPathEvents.Clear();
                if(levelController!=null) levelController.GetType().GetMethod("Cancel").Invoke(levelController,null);
            }
        }

        private void Sprint22ReadProfile(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool turnBased)
        {
            var stats=owner.Descriptor.Stats; var limbs=Sprint19Limbs(owner);
            int[] scores=new[] { StatType.Strength,StatType.Dexterity,StatType.Constitution,StatType.Intelligence,StatType.Wisdom,StatType.Charisma }
                .Select(value=>stats.GetStat(value).ModifiedValue).ToArray();
            int difficulty=stats.AC.Modifiers.Where(value=>value.ModDescriptor==ModifierDescriptor.Difficulty).Sum(value=>value.ModValue);
            // Improved Bull Rush also supplies +2 defense against bull rush;
            // use grapple for the generic printed CMD, then name that feat's
            // separate native maneuver terms rather than subtracting them.
            var cmd=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Grapple);
            var trip=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Trip);
            int cmb=Rulebook.Trigger(new RuleCalculateCMB(owner,fixture.Hostile,CombatManeuver.Trip)).Result;
            const string tailType="7243c9cd0f654b9080e7b31112ddbf18";
            var powerAttack=owner.Descriptor.ActivatableAbilities.Enumerable
                .Single(value=>value.Blueprint.AssetGuid==
                    "a7b339e4f6ff93a4697df5d7a87ff619");
            JObject powerAttackState=Sprint22PowerAttackState(owner,powerAttack);
            bool powerAttackOff=!powerAttack.IsOn && !powerAttack.IsRunning &&
                (int)powerAttackState["buffCount"]==0;
            var weaponRows=new JArray(); bool weaponExact=limbs.Length==1;
            foreach (var weapon in limbs)
            {
                var rule=Rulebook.Trigger(new RuleCalculateWeaponStats(owner,weapon,null));
                BaseDamage damage=rule.DamageDescription[0].CreateDamage();
                int attack=Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner,weapon,0)).Result;
                weaponExact &= damage.Dice.Rolls==3 && (int)damage.Dice.Dice==6 &&
                    damage.Bonus==12 && attack==14 && !rule.SecondaryWeapon &&
                    weapon.Blueprint.Type.AssetGuid==tailType;
                weaponRows.Add(new JObject { ["guid"]=weapon.Blueprint.AssetGuid,["type"]=weapon.Blueprint.Type.AssetGuid,
                    ["attack"]=attack,["damage"]=Sprint16DamageLine(damage),["secondary"]=rule.SecondaryWeapon });
            }
            int deniedDex=cmd.DexterityDenied || cmd.FlatFooted ? stats.Dexterity.Bonus : 0;
            string[] facts=owner.Descriptor.Progression.Features.Enumerable
                .Select(value=>value.Blueprint.name).ToArray();
            bool profile=scores.SequenceEqual(new[] {27,10,17,2,13,8}) && owner.Descriptor.State.Size==Size.Huge &&
                owner.Descriptor.Progression.CharacterLevel==10 && stats.HitPoints.ModifiedValue==75 &&
                stats.AC.ModifiedValue-difficulty==22 && stats.AC.Touch-difficulty==8 && stats.AC.FlatFooted-difficulty==22 &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue==12 && stats.GetStat(StatType.SaveReflex).ModifiedValue==7 &&
                stats.GetStat(StatType.SaveWill).ModifiedValue==4 && stats.GetStat(StatType.SkillPerception).ModifiedValue==14 &&
                stats.BaseAttackBonus.ModifiedValue==7 && cmb==17 &&
                cmd.Result+deniedDex-difficulty==27 && trip.Result-cmd.Result==4 &&
                facts.Count(value=>value=="KMG_Summoning_Natural_Ankylosaurus_Chassis")==1 &&
                powerAttackOff && weaponExact;
            profile &= stats.Reach.ModifiedValue==15 && stats.Speed.ModifiedValue==30 &&
                Math.Abs(limbs.Single().AttackRange.Meters-4.572f)<.002f;
            Sprint22Check("profile-"+(turnBased ? "turn-based":"rtwp"),profile,new JObject {
                ["unit"]=owner.UniqueId,["event"]="native-carrier-readback",["owner"]=owner.Blueprint.AssetGuid,
                ["scores"]=new JArray(scores),["hd"]=owner.Descriptor.Progression.CharacterLevel,["hp"]=stats.HitPoints.ModifiedValue,
                ["ac"]=stats.AC.ModifiedValue,["touch"]=stats.AC.Touch,["flat"]=stats.AC.FlatFooted,["difficulty"]=difficulty,
                ["fort"]=stats.GetStat(StatType.SaveFortitude).ModifiedValue,["reflex"]=stats.GetStat(StatType.SaveReflex).ModifiedValue,
                ["will"]=stats.GetStat(StatType.SaveWill).ModifiedValue,["perception"]=DescribeSprint16Skill(stats.GetStat(StatType.SkillPerception)),
                ["bab"]=stats.BaseAttackBonus.ModifiedValue,["cmb"]=cmb,["cmd"]=cmd.Describe("CMD"),["tripCmd"]=trip.Describe("trip"),
                ["reachFeet"]=stats.Reach.ModifiedValue,["speedFeet"]=stats.Speed.ModifiedValue,
                ["weaponRangeMeters"]=limbs.Single().AttackRange.Meters,["nativeCorpulenceMeters"]=owner.Corpulence,
                ["powerAttackState"]=powerAttackState,["limbs"]=weaponRows,
                ["carrierFacts"]=Sprint22UnitReadback(owner) },"Adult Huge profile and one 3d6+12 tail +14, native feats and exact Perception ranks.");
            Mesh mesh;string[] names;Texture2D albedo;string status;
            bool loaded=PteranodonAssetRuntime.TryGetAnkylosaurusVisual(out mesh,out names,out albedo,out status);
            var skins=owner.View==null ? new SkinnedMeshRenderer[0] : owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var original=skins.Where(value=>value.sharedMesh!=null && value.sharedMesh.name=="KMG_ankylosaurus_Original").ToArray();
            string supportingFrame=original.Length==1 ? CaptureSprint17OriginalBodyFrame(owner,original[0],"sprint22-ankylosaurus-"+(turnBased?"turn-based":"rtwp")) : "no-original-renderer";
            Sprint22Check("body-"+(turnBased ? "turn-based":"rtwp"),loaded && original.Length==1 && original[0].enabled &&
                original[0].bones.Length==24 && original[0].sharedMesh!=mesh && original[0].sharedMaterial.mainTexture==albedo,
                new JObject { ["unit"]=owner.UniqueId,["event"]="owned-view-bind",["owner"]=owner.Blueprint.AssetGuid,
                    ["status"]=status,["supportingFrame"]=supportingFrame,["skins"]=new JArray(skins.Select(value=>new JObject { ["name"]=value.name,["enabled"]=value.enabled,
                        ["mesh"]=value.sharedMesh?.name,["meshId"]=value.sharedMesh==null ? 0 : value.sharedMesh.GetInstanceID(),
                        ["materialId"]=value.sharedMaterial==null ? 0 : value.sharedMaterial.GetInstanceID(),
                        ["bones"]=new JArray(value.bones.Select(bone=>bone?.name)) })) },"One private original body on 24 exact actor-owned drivers with borrowed cached albedo.");
        }

        private const string Sprint22DazedBuffGuid =
            "74654177d90e4d1d8fd512c0dabe3966";
        private const string Sprint22StunnedBuffGuid =
            "1f95fa2f17b042df8065a28823e09fc3";

        private static bool Sprint22OwnedControl(BlueprintBuff value)
        {
            // Unity's null-conditional operator does not use the overloaded
            // Object null check. A deferredly destroyed fixture blueprint can
            // therefore throw from its native name getter. Stable ledger GUIDs
            // identify only the two private Sprint 22 carriers and safely
            // ignore such dead fixture objects.
            if (value == null) return false;
            string guid=value.AssetGuid;
            return guid==Sprint22DazedBuffGuid ||
                guid==Sprint22StunnedBuffGuid;
        }

        private static bool Sprint22OwnedControl(Buff value)
        {
            return Sprint22OwnedControl(value?.Blueprint);
        }

        private static JObject Sprint22BuffApplication(RuleApplyBuff value,
            IEnumerable<Buff> ownedEffects)
        {
            Buff applied=value?.AppliedBuff;
            return new JObject {
                ["eventIdentity"]=value==null ? 0 :
                    System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value),
                ["blueprint"]=value?.Blueprint?.AssetGuid,
                ["blueprintName"]=value?.Blueprint?.name,
                ["durationSeconds"]=value?.Duration?.TotalSeconds,
                ["canApply"]=value!=null && value.CanApply,
                ["immune"]=value!=null && value.Immunity,
                ["storyImmune"]=value!=null && value.StoryModeImmunity,
                ["appliedBuffIdentity"]=applied==null ? 0 :
                    System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(applied),
                ["appliedBuffBlueprint"]=applied?.Blueprint?.AssetGuid,
                ["appliedBuffActive"]=applied!=null && applied.Active,
                ["appliedInstanceIsOwnedEffect"]=applied!=null &&
                    ownedEffects.Contains(applied),
                ["contextCaster"]=value?.Context?.MaybeCaster?.UniqueId };
        }

        private static int Sprint22ClearOwnedControl(UnitEntityData target)
        {
            Buff[] owned=target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(Sprint22OwnedControl).ToArray();
            foreach(Buff buff in owned) buff.Remove();
            if(target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Any(Sprint22OwnedControl))
                throw new InvalidOperationException(
                    "The request-owned victim retained an exact Sprint 22 control buff.");
            return owned.Length;
        }

        private static JObject Sprint22EnterTurnBasedCombat(
            UnitEntityData owner, UnitEntityData target)
        {
            bool ownerBefore=owner.IsInCombat;
            bool targetBefore=target.IsInCombat;
            owner.Memory.Add(target);
            target.Memory.Add(owner);
            if(!Game.Instance.State.AwakeUnits.Contains(owner))
                Game.Instance.State.AwakeUnits.Add(owner);
            if(!Game.Instance.State.AwakeUnits.Contains(target))
                Game.Instance.State.AwakeUnits.Add(target);
            if(!owner.IsInCombat) owner.JoinCombat();
            if(!target.IsInCombat) target.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            return new JObject { ["unit"]=owner.UniqueId,
                ["target"]=target.UniqueId,
                ["event"]="native-turn-order-entry",
                ["owner"]=owner.Blueprint.AssetGuid,
                ["ownerInCombatBefore"]=ownerBefore,
                ["targetInCombatBefore"]=targetBefore,
                ["ownerInCombatAfterJoin"]=owner.IsInCombat,
                ["targetInCombatAfterJoin"]=target.IsInCombat,
                ["turnBasedAfterJoin"]=CombatController
                    .IsInTurnBasedCombat() };
        }

        /// <summary>
        /// Enroll request-owned actors while the native game is in RTWP, then
        /// let the turn-based controller rebuild its order from those combat
        /// states. This follows the engine's supported mode-toggle path and
        /// proves membership before any command is issued; it does not write
        /// initiative, the current turn, cooldowns or command state.
        /// </summary>
        private IEnumerable<int> Sprint22EnrollTurnBasedOwners(
            IEnumerable<UnitEntityData> owners,UnitEntityData target,
            JObject evidence)
        {
            UnitEntityData[] exactOwners=owners.Where(value=>value!=null)
                .Distinct().ToArray();
            var controller=Game.Instance.TurnBasedCombatController;
            evidence["unit"]=exactOwners.Length==0 ? null :
                exactOwners[0].UniqueId;
            evidence["event"]="native-rtwp-enrollment-and-turn-order-toggle";
            evidence["owner"]=exactOwners.Length==0 ? null :
                exactOwners[0].Blueprint.AssetGuid;
            evidence["owners"]=new JArray(exactOwners.Select(value=>
                value.UniqueId));
            evidence["resourceOwners"]=new JArray(exactOwners.Select(value=>
                value.Blueprint.AssetGuid));
            evidence["target"]=target?.UniqueId;
            evidence["turnBasedBefore"]=CombatController
                .IsInTurnBasedCombat();
            evidence["roundBefore"]=controller.RoundNumber;
            Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue=false;
            controller.Activate();
            Game.Instance.IsPaused=false;
            evidence["turnBasedDisabled"]=!CombatController
                .IsInTurnBasedCombat();
            var joins=new JArray();
            foreach(UnitEntityData owner in exactOwners)
                joins.Add(Sprint22EnterTurnBasedCombat(owner,target));
            evidence["joins"]=joins;
            for(int frame=0;frame<8;frame++) yield return 0;
            evidence["rtwpOwnerCombat"]=exactOwners.All(value=>
                value.IsInCombat);
            evidence["rtwpTargetCombat"]=target!=null && target.IsInCombat;
            Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue=true;
            controller.Activate();
            for(int frame=0;frame<8;frame++) yield return 0;
            UnitEntityData[] sorted=controller.SortedUnits.ToArray();
            TurnController turn=controller.CurrentTurn;
            bool ownersSorted=exactOwners.All(owner=>sorted.Any(value=>
                ReferenceEquals(value,owner)));
            bool targetSorted=target!=null && sorted.Any(value=>
                ReferenceEquals(value,target));
            evidence["roundAfter"]=controller.RoundNumber;
            evidence["sortedUnits"]=new JArray(sorted.Select(value=>
                value==null ? "<null>" : value.UniqueId));
            evidence["ownersSorted"]=ownersSorted;
            evidence["targetSorted"]=targetSorted;
            evidence["currentTurn"]=turn?.Unit?.UniqueId;
            evidence["currentStatus"]=turn==null ? null :
                turn.Status.ToString();
            evidence["turnBasedAfter"]=CombatController
                .IsInTurnBasedCombat();
            evidence["admitted"]=CombatController.IsInTurnBasedCombat() &&
                exactOwners.Length>0 && ownersSorted && targetSorted &&
                exactOwners.All(value=>value.IsInCombat) && target!=null &&
                target.IsInCombat;
        }

        private static JObject Sprint22TurnState(UnitEntityData owner,
            UnitEntityData target)
        {
            var controller=Game.Instance.TurnBasedCombatController;
            TurnController turn=controller.CurrentTurn;
            UnitEntityData[] sorted=controller.SortedUnits.ToArray();
            return new JObject {
                ["round"]=controller.RoundNumber,
                ["currentTurn"]=turn?.Unit?.UniqueId,
                ["currentStatus"]=turn==null ? null :
                    turn.Status.ToString(),
                ["ownerSorted"]=sorted.Any(value=>ReferenceEquals(
                    value,owner)),
                ["targetSorted"]=sorted.Any(value=>ReferenceEquals(
                    value,target)),
                ["sortedUnits"]=new JArray(sorted.Select(value=>
                    value==null ? "<null>" : value.UniqueId)),
                ["ownerInCombat"]=owner!=null && owner.IsInCombat,
                ["targetInCombat"]=target!=null && target.IsInCombat,
                ["turnBased"]=CombatController.IsInTurnBasedCombat()
            };
        }

        private IEnumerable<int> Sprint22RiderTrial(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,string name,bool turnBased)
        {
            var target=fixture.Hostile; var weapon=Sprint19Limbs(owner).Single();
            var observer=new Sprint16RuleObserver { Owner=owner,Target=target };
            var saves=new Sprint19SaveObserver { Target=target }; var strikes=new List<RuleAttackWithWeapon>();
            var duration=new Sprint22DurationObserver { Owner=owner,Target=target };
            var contacts=new JArray();UnitAttack contactCommand=null;
            Sprint22TailContactProbe contactProbe=null;
            RuleAttackWithWeapon contactRule=null;string contactFrame=null;
            observer.ObserveWeaponContact=value=>{
                strikes.Add(value);contactRule=value;
                if(contactProbe!=null)
                {
                    contactProbe.RuleFrame=Time.frameCount;
                    contactProbe.RuleHandleActed=contactCommand!=null &&
                        contactCommand.Animation!=null &&
                        contactCommand.Animation.IsActed;
                }
            };
            int oldAttack=owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            int oldStrength=owner.Descriptor.Stats.Strength.BaseValue;
            bool critical=name.Contains("critical") && name!="unconfirmed-threat" && name!="critical-immunity";
            bool success=name=="ordinary-success" || name=="critical-success";
            bool dazeImmune=name=="daze-immunity" || name=="daze-immunity-critical";
            bool stunImmune=name=="stun-immunity-ordinary" || name=="stun-immunity-critical";
            int staleOwnedEffects=Sprint22ClearOwnedControl(target);
            var beforeBuffs=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            BlueprintBuff shieldBlueprint=null; Buff shield=null;
            EventBus.Subscribe(observer);EventBus.Subscribe(saves);EventBus.Subscribe(duration);
            try
            {
                if (dazeImmune) target.Descriptor.State.AddConditionImmunity(UnitCondition.Dazed);
                if (stunImmune) target.Descriptor.State.AddConditionImmunity(UnitCondition.Stunned);
                if (name=="absorbed-hit")
                {
                    shieldBlueprint=ScriptableObject.CreateInstance<BlueprintBuff>();shieldBlueprint.name="KMG_Runtime_Sprint22_Absorber";
                    shieldBlueprint.ComponentsArray=new BlueprintComponent[] { new AddDamageResistancePhysical { Value=1000 } };
                    shield=target.Descriptor.AddBuff(shieldBlueprint,target,TimeSpan.FromMinutes(5));
                }
                if (name=="critical-immunity")
                {
                    shieldBlueprint=ScriptableObject.CreateInstance<BlueprintBuff>();shieldBlueprint.name="KMG_Runtime_Sprint22_CriticalImmunity";
                    shieldBlueprint.ComponentsArray=new BlueprintComponent[] { new AddImmunityToCriticalHits() };
                    shield=target.Descriptor.AddBuff(shieldBlueprint,target,TimeSpan.FromMinutes(5));
                }
                if (name=="strength-increase") owner.Descriptor.Stats.Strength.BaseValue=oldStrength+4;
                if (name=="strength-decrease") owner.Descriptor.Stats.Strength.BaseValue=oldStrength-8;
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=success ? 100 : -100;
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=name=="miss" || name=="unconfirmed-threat" ? -100 : 100;
                var trials=new JArray(); bool satisfied=false; int bound=critical || name=="unconfirmed-threat" || name=="critical-immunity" ? 120 : 12;
                RuleAttackRoll roll=null; UnitAttack command=null; Buff[] effects=new Buff[0];
                for (int trial=1;trial<=bound;trial++)
                {
                    foreach (Buff buff in target.Descriptor.Buffs.RawFacts
                        .OfType<Buff>().Where(value=>!beforeBuffs.Contains(value) &&
                            Sprint22OwnedControl(value)).ToArray()) buff.Remove();
                    target.Descriptor.Damage=fixture.HostileDamage;
                    target.Commands.InterruptAll(); owner.Commands.InterruptAll();
                    PlaceExpandedSummoningUnit(owner,fixture.Caster.Position+Vector3.right*4f);
                    PlaceExpandedSummoningUnit(target,owner.Position+Vector3.forward*3f);
                    observer.Clear(); saves.Saves.Clear();strikes.Clear();contacts.Clear();duration.Clear();
                    contactCommand=null;contactRule=null;contactFrame=null;
                    contactProbe=name=="ordinary-failure" ?
                        new Sprint22TailContactProbe(owner,target) : null;
                    JObject turnOrder=null;
                    if (turnBased)
                    {
                        turnOrder=new JObject();
                        foreach(int frame in Sprint22EnrollTurnBasedOwners(
                            new[] { owner },target,turnOrder))
                            yield return frame;
                        if(!(bool)turnOrder["admitted"])
                            throw new InvalidOperationException("unit="+
                                owner.UniqueId+";predicate="+name+
                                ";event=native-turn-order-admission;owner="+
                                owner.Blueprint.AssetGuid+";state="+
                                turnOrder.ToString(Formatting.None));
                    }
                    foreach (int frame in Sprint18RunFullAttack(fixture,owner,
                        result=>{ command=result;contactCommand=result; },null,
                        result=>
                        {
                            if(contactProbe==null || contactFrame!=null ||
                                !contactProbe.Sample(result)) return;
                            var skin=owner.View.GetComponentsInChildren<
                                SkinnedMeshRenderer>(true).SingleOrDefault(value=>
                                    value.sharedMesh!=null &&
                                    value.sharedMesh.name=="KMG_ankylosaurus_Original");
                            if(skin!=null) contactFrame=
                                CaptureSprint17OriginalBodyFrame(owner,skin,
                                    "sprint22-tail-contact-"+
                                    (turnBased?"turn-based":"rtwp"));
                        })) yield return frame;
                    // UnitAttack finishes at the native act cue while the
                    // donor clip and the exact hit-owned club follow-through
                    // continue. Observe that bounded presentation instead of
                    // treating the rule frame's pre-impact pose as final.
                    if(contactProbe!=null && command!=null)
                        for(int post=0;post<30;post++)
                        {
                            bool touched=contactProbe.Sample(command);
                            if(touched && contactFrame==null)
                            {
                                var skin=owner.View
                                    .GetComponentsInChildren<
                                        SkinnedMeshRenderer>(true)
                                    .SingleOrDefault(value=>
                                        value.sharedMesh!=null &&
                                        value.sharedMesh.name==
                                            "KMG_ankylosaurus_Original");
                                if(skin!=null) contactFrame=
                                    CaptureSprint17OriginalBodyFrame(owner,
                                        skin,"sprint22-tail-contact-"+
                                        (turnBased?"turn-based":"rtwp"));
                            }
                            var postImpact=owner.View
                                .GetComponent<AnkylosaurusTailImpactVisual>();
                            if(post>1 && (postImpact==null ||
                                !postImpact.IsPresenting)) break;
                            yield return 0;
                        }
                    if(contactProbe!=null)
                        contacts.Add(contactProbe.Result(contactRule,
                            contactCommand,contactFrame));
                    roll=observer.Attacks.SingleOrDefault();
                    effects=target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                        .Where(value=>!beforeBuffs.Contains(value) &&
                            Sprint22OwnedControl(value)).ToArray();
                    if(command==null || !command.IsFinished ||
                        observer.Attacks.Count==0)
                        throw new InvalidOperationException("unit="+
                            owner.UniqueId+";predicate="+name+
                            ";event=native-command-setup;owner="+
                            owner.Blueprint.AssetGuid+";commandFinished="+
                            (command!=null && command.IsFinished)+
                            ";attackRules="+observer.Attacks.Count+
                            ";ownerInCombat="+owner.IsInCombat+
                            ";targetInCombat="+target.IsInCombat+
                            ";turnBased="+CombatController
                                .IsInTurnBasedCombat()+";turnState="+
                            Sprint22TurnState(owner,target)
                                .ToString(Formatting.None));
                    bool expectedHit=name!="miss";
                    satisfied=command!=null && command.IsFinished && roll!=null && roll.IsHit==expectedHit &&
                        roll.IsCriticalConfirmed==critical && (name!="unconfirmed-threat" || roll.IsCriticalRoll) &&
                        (name!="critical-immunity" || roll.Roll.Value==20 && roll.ImmuneToCriticalHit) &&
                        (name=="miss" || saves.Saves.Count!=1 || saves.Saves[0].IsPassed==success);
                    trials.Add(new JObject { ["trial"]=trial,["preconditionsSatisfied"]=satisfied,["commandFinished"]=command?.IsFinished,
                        ["turnOrder"]=turnOrder,
                        ["rolls"]=new JArray(observer.Attacks.Select(value=>new JObject { ["d20"]=value.Roll.Value,["hit"]=value.IsHit,
                            ["threat"]=value.IsCriticalRoll,["confirmed"]=value.IsCriticalConfirmed,["criticalImmune"]=value.ImmuneToCriticalHit })),
                        ["saves"]=new JArray(saves.Saves.Select(value=>new JObject { ["d20"]=value.D20.Value,["dc"]=value.DifficultyClass,
                            ["stat"]=value.StatType.ToString(),["passed"]=value.IsPassed })),
                        ["damage"]=new JArray(observer.Damage.Select(value=>value.Damage)) });
                    if (satisfied) break;
                    // Only a dice/setup precondition may retry. A satisfied
                    // trial is evaluated exactly once below and never rerun.
                    for (int frame=0;frame<8;frame++) yield return 0;
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;
                    owner.Descriptor.Stats.Strength.BaseValue=oldStrength;
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
                    owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");SetExpandedSummoningBrainActive(owner,false);
                    observer.Owner=owner;duration.Owner=owner;
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=name=="miss" || name=="unconfirmed-threat" ? -100 : 100;
                    if (name=="strength-increase") owner.Descriptor.Stats.Strength.BaseValue=oldStrength+4;
                    if (name=="strength-decrease") owner.Descriptor.Stats.Strength.BaseValue=oldStrength-8;
                }
                int dc=AnkylosaurusRulesPolicy.DifficultyClass(10,owner.Descriptor.Stats.Strength.Bonus);
                bool immune=critical ? stunImmune : dazeImmune;
                bool expectedCondition=name!="miss" && !success && !immune;
                bool condition=target.Descriptor.State.HasCondition(critical ? UnitCondition.Stunned : UnitCondition.Dazed);
                bool other=target.Descriptor.State.HasCondition(critical ? UnitCondition.Dazed : UnitCondition.Stunned);
                int expectedRolls=critical && name!="miss" && !success ? 1 : 0;
                bool exact=satisfied && condition==expectedCondition && !other &&
                    effects.Length==(name=="miss" || success ? 0 : 1) &&
                    (name!="absorbed-hit" || observer.Damage.Count==1 && observer.Damage[0].Damage==0) &&
                    (name=="miss" ? saves.Saves.Count==0 : saves.Saves.Count==1 && saves.Saves[0].StatType==StatType.SaveFortitude && saves.Saves[0].DifficultyClass==dc);
                exact &= duration.Rolls.Count==expectedRolls && duration.Rolls.All(value=>value.Result.Value>=1 && value.Result.Value<=4);
                int expectedApplications=name!="miss" && !success ? 1 : 0;
                var impact=owner.View.GetComponent<AnkylosaurusTailImpactVisual>();
                exact &= impact!=null && impact.Strikes==(name=="miss" ? 0 : 1);
                exact &= duration.Applications.Count==expectedApplications && duration.Applications.All(value=>
                    value.Duration.HasValue && value.AppliedBuff!=null && effects.Contains(value.AppliedBuff) &&
                    Math.Abs(value.Duration.Value.TotalSeconds-(critical ? duration.Rolls.Single().Result.Value*6d : 6d))<.001);
                var lifetimes=new JArray(effects.Select(value=>new JObject { ["buff"]=value.Blueprint.AssetGuid,
                    ["source"]=value.Context?.MaybeCaster?.UniqueId,["endSeconds"]=value.EndTime.TotalSeconds,["leftSeconds"]=value.TimeLeft.TotalSeconds }));
                if (name=="duplicate-callback" && satisfied)
                {
                    var handler=owner.Descriptor.Progression.Features.Enumerable.SelectMany(value=>value.SelectComponents<AnkylosaurusTailControl>()).Single();
                    double end=effects.Single().EndTime.TotalSeconds;
                    handler.OnEventDidTrigger(strikes.Single());handler.OnEventDidTrigger(strikes.Single());
                    exact &= saves.Saves.Count==1 && effects.Single().EndTime.TotalSeconds==end &&
                        target.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value=>effects.Contains(value))==1 && impact.Strikes==1;
                }
                Sprint22Check("rider-"+name+"-"+(turnBased ? "turn-based":"rtwp"),exact,new JObject {
                    ["unit"]=owner.UniqueId,["target"]=target.UniqueId,["owner"]=owner.Blueprint.AssetGuid,["event"]=name,
                    ["mode"]=turnBased ? "turn-based":"rtwp",["trials"]=trials,["precondition"]=satisfied ? "SATISFIED":"UNSATISFIED",
                    ["staleOwnedEffectsCleared"]=staleOwnedEffects,
                    ["dc"]=dc,["expectedCondition"]=expectedCondition,["condition"]=condition,["otherCondition"]=other,["effects"]=lifetimes,
                    ["dazeImmune"]=dazeImmune,["stunImmune"]=stunImmune,["contacts"]=contacts,
                    ["impactCues"]=impact?.Strikes,["impactTarget"]=impact?.LastTarget,
                    ["nativeDurationD4"]=new JArray(duration.Rolls.Select(value=>value.Result.Value)),
                    ["buffApplications"]=new JArray(duration.Applications.Select(value=>Sprint22BuffApplication(value,effects))),
                    ["otherTargetBuffApplications"]=new JArray(duration.OtherTargetApplications.Select(value=>Sprint22BuffApplication(value,effects))) },"One native Fortitude save per legal tail hit; mutually exclusive finite condition, separate native immunity and no positive-damage prerequisite.");
                if (name=="ordinary-failure")
                    Sprint22Check("tail-contact-"+(turnBased ? "turn-based":"rtwp"),contacts.Count==1 &&
                        (bool?)contacts[0]["nativeTailAction"]==true &&
                        (int?)contacts[0]["impactPoseFrames"]>0 &&
                        (double?)contacts[0]["minimumGapMeters"]<=.5,
                        new JObject { ["unit"]=owner.UniqueId,["target"]=target.UniqueId,["event"]="real-tail-strike-contact",
                            ["owner"]=owner.Blueprint.AssetGuid,["contacts"]=contacts },"The exact issued native tail command resolves a real hit and its bounded hit-owned club follow-through puts terminal-club surface within 0.5 m of native target bounds.");
                if (!satisfied) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate="+name+";event=dice-preconditions;owner="+owner.Blueprint.AssetGuid);
                if (!exact) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate="+name+";event=postcondition;owner="+owner.Blueprint.AssetGuid);
                if (name=="critical-failure")
                {
                    double nativeStarted=Game.Instance.TimeController.GameTime.TotalSeconds;
                    var clock=System.Diagnostics.Stopwatch.StartNew();
                    CombatController turnController=Game.Instance
                        .TurnBasedCombatController;
                    int roundBefore=turnController.RoundNumber;
                    int endedTurns=0;
                    string lastTurnState=null;
                    var turnTransitions=new JArray();
                    while ((target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                            .Any(value=>effects.Contains(value)) ||
                            target.Descriptor.State.HasCondition(
                                UnitCondition.Stunned) ||
                            !target.Descriptor.State.CanAct) &&
                        clock.Elapsed.TotalSeconds<30)
                    {
                        Game.Instance.IsPaused=false;
                        if (CombatController.IsInTurnBasedCombat())
                        {
                            TurnController turn=turnController.CurrentTurn;
                            string state="round="+turnController.RoundNumber+
                                ";unit="+(turn?.Unit?.UniqueId ?? "<none>")+
                                ";status="+(turn==null ? "<none>" :
                                    turn.Status.ToString());
                            if (state!=lastTurnState)
                            {
                                turnTransitions.Add(state);
                                lastTurnState=state;
                            }
                            // End an active native turn once. Repeating End()
                            // against the same already-ended TurnController
                            // prevents CombatController.Tick from owning the
                            // transition to the next actor/round.
                            if (turn!=null && (turn.Status==
                                    TurnController.TurnStatus.Preparing ||
                                turn.Status==
                                    TurnController.TurnStatus.Acting))
                            {
                                Sprint18EndTurn(turn);
                                endedTurns++;
                            }
                        }
                        yield return 0;
                    }
                    bool privateEffectsGone=effects.All(value=>
                        !target.Descriptor.Buffs.RawFacts.Contains(value));
                    bool hasStunnedCondition=target.Descriptor.State
                        .HasCondition(UnitCondition.Stunned);
                    bool canAct=target.Descriptor.State.CanAct;
                    bool expired=privateEffectsGone && !hasStunnedCondition &&
                        canAct;
                    UnitAttack returned=null;
                    var returnedRules=new Sprint16RuleObserver {
                        Owner=target,Target=fixture.Caster };
                    EventBus.Subscribe(returnedRules);
                    try
                    {
                        foreach(int frame in Sprint18RunFullAttack(fixture,
                            target,value=>returned=value,fixture.Caster))
                            yield return frame;
                    }
                    finally { EventBus.Unsubscribe(returnedRules); }
                    bool actionReturned=returned!=null &&
                        returned.IsFinished && returnedRules.Attacks.Count>0;
                    Sprint22Check("critical-natural-expiry-"+(turnBased ? "turn-based":"rtwp"),expired && actionReturned,
                        new JObject { ["unit"]=target.UniqueId,["event"]="native-stun-expiration-and-action",["owner"]="finite-victim-condition",
                            ["privateEffectsGone"]=privateEffectsGone,
                            ["hasStunnedCondition"]=hasStunnedCondition,
                            ["canAct"]=canAct,["expired"]=expired,
                            ["actionFinished"]=returned?.IsFinished,
                            ["nativeAttackRules"]=returnedRules.Attacks.Count,
                            ["actionReturned"]=actionReturned,
                            ["roundBefore"]=roundBefore,
                            ["roundAfter"]=turnController.RoundNumber,
                            ["endedTurns"]=endedTurns,
                            ["turnTransitions"]=turnTransitions,
                            ["clockCoverage"]=_sprint22Clock?
                                .DescribeTurnCoverage(),
                            ["nativeElapsedSeconds"]=Game.Instance.TimeController.GameTime.TotalSeconds-nativeStarted,
                            ["wallSeconds"]=clock.Elapsed.TotalSeconds },"Critical stun expires naturally and native victim actions return.");
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer);EventBus.Unsubscribe(saves);EventBus.Unsubscribe(duration);
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;
                owner.Descriptor.Stats.Strength.BaseValue=oldStrength;
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort;
                if (dazeImmune) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Dazed);
                if (stunImmune) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Stunned);
                foreach (Buff buff in target.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value=>!beforeBuffs.Contains(value) &&
                        Sprint22OwnedControl(value)).ToArray()) buff.Remove();
                if(shield!=null && target.Descriptor.Buffs.RawFacts.Contains(shield))
                    shield.Remove();
                if (shieldBlueprint!=null) UnityEngine.Object.Destroy(shieldBlueprint);
                DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
            }
        }

        private IEnumerable<int> Sprint22Lifetime(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool turnBased)
        {
            UnitEntityData target=fixture.Hostile;
            Sprint22ClearOwnedControl(target);
            UnitEntityData second=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
            SetExpandedSummoningBrainActive(second,false);
            JObject lifetimeTurnOrder=null;
            if(turnBased)
            {
                lifetimeTurnOrder=new JObject();
                foreach(int frame in Sprint22EnrollTurnBasedOwners(
                    new[] { owner,second },target,lifetimeTurnOrder))
                    yield return frame;
                if(!(bool)lifetimeTurnOrder["admitted"])
                    throw new InvalidOperationException("unit="+
                        owner.UniqueId+
                        ";predicate=multi-source-lifetime-turn-order"+
                        ";event=native-turn-order-admission;owner="+
                        owner.Blueprint.AssetGuid+";state="+
                        lifetimeTurnOrder.ToString(Formatting.None));
            }
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            var original=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            Buff unrelated=null;
            try
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=-100;
                var native=fixture.Blueprints.OfType<BlueprintBuff>().Single(value=>value.AssetGuid=="d2e35b870e4ac574d9873b36402487e5");
                unrelated=target.Descriptor.AddBuff(native,fixture.Caster,TimeSpan.FromMinutes(1));
                var legal=new JArray();
                int attack1=owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                int attack2=second.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                bool allLegal=false;
                try
                {
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                    second.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                    // The command cases above prove animated attack delivery.
                    // These three distinct native strike rules run in one
                    // frame to test overlapping one-round victim lifetimes
                    // independently of initiative order. No synthetic roll,
                    // damage or rider event is inserted into production.
                    for (int trial=1;trial<=12;trial++)
                    {
                        foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>
                            !original.Contains(value) && value!=unrelated &&
                            Sprint22OwnedControl(value)).ToArray()) buff.Remove();
                        target.Descriptor.Damage=fixture.HostileDamage;
                        allLegal=true;
                        foreach (UnitEntityData source in new[] {owner,owner,second})
                        {
                            var saves=new Sprint19SaveObserver { Target=target };
                            EventBus.Subscribe(saves);
                            try
                            {
                                var strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,target,Sprint19Limbs(source).Single(),0));
                                var hit=strike.AttackRoll;
                                bool setup=hit!=null && hit.IsHit && !hit.IsCriticalConfirmed &&
                                    (saves.Saves.Count!=1 || !saves.Saves[0].IsPassed);
                                legal.Add(new JObject { ["source"]=source.UniqueId,["trial"]=trial,["preconditionsSatisfied"]=setup,
                                    ["event"]="distinct-native-strike-rule",["d20"]=hit==null ? -1 : hit.Roll.Value,
                                    ["hit"]=hit!=null && hit.IsHit,["critical"]=hit!=null && hit.IsCriticalConfirmed,
                                    ["saves"]=saves.Saves.Count,["passed"]=saves.Saves.Count==1 && saves.Saves[0].IsPassed });
                                if (setup && saves.Saves.Count!=1) throw new InvalidOperationException("unit="+source.UniqueId+";predicate=one-lifetime-save;event=native-strike-rule;owner="+source.Blueprint.AssetGuid);
                                allLegal &= setup;
                            }
                            finally { EventBus.Unsubscribe(saves); }
                        }
                        if (allLegal) break;
                    }
                }
                finally
                {
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=attack1;
                    second.Descriptor.Stats.AdditionalAttackBonus.BaseValue=attack2;
                }
                if (!allLegal) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate=overlapping-lifetime-setup;event=native-strike-rule;owner="+owner.Blueprint.AssetGuid+";trials="+legal);
                var effects=target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Where(value=>!original.Contains(value) &&
                        value.Blueprint.name==
                            "KMG_Summoning_Special_Ankylosaurus_Dazed")
                    .ToArray();
                var sources=effects.Select(value=>value.Context?.MaybeCaster?.UniqueId).Distinct().ToArray();
                var ends=effects.ToDictionary(value=>value,value=>value.EndTime);
                bool finite=effects.All(value=>value.TimeLeft.TotalSeconds>0 && value.TimeLeft.TotalSeconds<=6.01);
                string sourceId=owner.UniqueId;
                string sourceBlueprint=owner.Blueprint.AssetGuid;
                // The exact source dies after its strike. Already applied
                // victim buffs retain their absolute native expiry times.
                int constitution=Math.Max(1,owner.Descriptor.Stats.Constitution.ModifiedValue);
                int lethal=owner.MaxHP+constitution+10;
                int immortality=owner.Descriptor.State.Immortality.Count;
                string lifeBefore=owner.Descriptor.State.LifeState.ToString();
                var deathDamage=Rulebook.Trigger(new RuleDealDamage(owner,owner,
                    new DamageBundle(new DirectDamage(new DiceFormula(0,DiceType.D6),lethal)))
                    { DisablePrecisionDamage=true,IgnoreDamageReduction=true });
                if(immortality!=0 || deathDamage.Damage<lethal || owner.HPLeft>-constitution)
                    throw new InvalidOperationException("unit="+sourceId+
                        ";predicate=native-lethal-threshold;event=self-attributed-rule-deal-damage;owner="+
                        sourceBlueprint+";immortality="+immortality+";requested="+lethal+
                        ";actual="+deathDamage.Damage+";hp="+owner.HPLeft+";maxHp="+
                        owner.MaxHP+";constitution="+constitution);
                MethodInfo lifeTick=typeof(UnitLifeController).GetMethod(
                    "TickOnUnit",BindingFlags.Instance|BindingFlags.Public|
                        BindingFlags.NonPublic,null,
                    new[] { typeof(UnitEntityData) },null);
                if(lifeTick==null) throw new MissingMethodException(
                    "Native UnitLifeController.TickOnUnit");
                lifeTick.Invoke(new UnitLifeController(),new object[] { owner });
                bool sourceDead=owner.Descriptor.State.IsDead;
                Game.Instance.EntityDestroyer.Tick();
                bool sourceDestroyedBeforeTeardown=owner.Destroyed;
                bool preservedAfterDeath=effects.All(value=>
                    target.Descriptor.Buffs.RawFacts.Contains(value) &&
                    value.EndTime==ends[value]) &&
                    target.Descriptor.Buffs.RawFacts.Contains(unrelated);
                string lifeAfter=owner.Descriptor.State.LifeState.ToString();
                int hpAfterDamage=owner.HPLeft;
                int maxHp=owner.MaxHP;
                // The native turn controller must not be asked to advance a
                // dead request-owned summon. Finish that source's ordinary
                // fixture teardown only after observing that its victim-side
                // effects survived death, then prove teardown did not erase
                // either owner's effects or the unrelated condition.
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
                bool sourceDestroyed=owner.Destroyed;
                JObject postDeathTurnOrder=null;
                if(turnBased)
                {
                    postDeathTurnOrder=new JObject();
                    foreach(int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { second },target,postDeathTurnOrder))
                        yield return frame;
                    if(!(bool)postDeathTurnOrder["admitted"])
                        throw new InvalidOperationException("unit="+
                            second.UniqueId+
                            ";predicate=post-death-lifetime-turn-order"+
                            ";event=native-turn-order-admission;owner="+
                            second.Blueprint.AssetGuid+";state="+
                            postDeathTurnOrder.ToString(Formatting.None));
                }
                bool preservedAfterTeardown=effects.All(value=>
                    target.Descriptor.Buffs.RawFacts.Contains(value) &&
                    value.EndTime==ends[value]) &&
                    target.Descriptor.Buffs.RawFacts.Contains(unrelated);
                bool preserved=preservedAfterDeath &&
                    preservedAfterTeardown;
                Sprint22Check("multi-source-lifetime-"+(turnBased ? "turn-based":"rtwp"),sourceDead && sourceDestroyed && effects.Length==3 && sources.Length==2 && finite && preserved,
                    new JObject { ["unit"]=sourceId,["secondSource"]=second.UniqueId,["target"]=target.UniqueId,["event"]="distinct-hits-source-death",
                        ["owner"]=sourceBlueprint,["legalHits"]=legal,["payloadCount"]=effects.Length,["sources"]=new JArray(sources),
                        ["preserved"]=preserved,["finite"]=finite,["sourceDead"]=sourceDead,
                        ["preservedAfterDeath"]=preservedAfterDeath,
                        ["preservedAfterTeardown"]=preservedAfterTeardown,
                        ["sourceDestroyedBeforeTeardown"]=sourceDestroyedBeforeTeardown,
                        ["sourceDestroyedAfterTeardown"]=sourceDestroyed,
                        ["postDeathTurnOrder"]=postDeathTurnOrder,
                        ["lifeBefore"]=lifeBefore,["lifeAfter"]=lifeAfter,
                        ["lethalRequested"]=lethal,["lethalApplied"]=deathDamage.Damage,
                        ["hpAfterDamage"]=hpAfterDamage,["maxHp"]=maxHp,
                        ["constitution"]=constitution,["immortality"]=immortality,
                        ["turnOrder"]=lifetimeTurnOrder,
                        ["unrelatedPreserved"]=target.Descriptor.Buffs.RawFacts.Contains(unrelated),
                        ["endTimes"]=new JArray(ends.Select(value=>new JObject { ["buff"]=value.Key.Blueprint.AssetGuid,
                            ["referenceId"]=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value.Key),["endSeconds"]=value.Value.TotalSeconds })) },
                    "Distinct legal hits have independent finite victim buffs; source death leaves both owners and unrelated conditions intact.");
                unrelated.Remove();unrelated=null;
                double started=Game.Instance.TimeController.GameTime.TotalSeconds;
                var clock=System.Diagnostics.Stopwatch.StartNew();
                CombatController turnController=Game.Instance
                    .TurnBasedCombatController;
                int roundBefore=turnController.RoundNumber;
                int endedTurns=0;
                string lastTurnState=null;
                var turnTransitions=new JArray();
                while (target.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value=>effects.Contains(value)) && clock.Elapsed.TotalSeconds<45)
                {
                    Game.Instance.IsPaused=false;
                    if (CombatController.IsInTurnBasedCombat())
                    {
                        TurnController turn=turnController.CurrentTurn;
                        string state="round="+turnController.RoundNumber+
                            ";unit="+(turn?.Unit?.UniqueId ?? "<none>")+
                            ";status="+(turn==null ? "<none>" :
                                turn.Status.ToString());
                        if(state!=lastTurnState)
                        {
                            turnTransitions.Add(state);
                            lastTurnState=state;
                        }
                        if(turn!=null && (turn.Status==
                                TurnController.TurnStatus.Preparing ||
                            turn.Status==
                                TurnController.TurnStatus.Acting))
                        {
                            Sprint18EndTurn(turn);
                            endedTurns++;
                        }
                    }
                    yield return 0;
                }
                bool expired=effects.All(value=>!target.Descriptor.Buffs.RawFacts.Contains(value)) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Dazed) && target.Descriptor.State.CanAct;
                UnitAttack action=null;
                foreach (int frame in Sprint18RunFullAttack(fixture,target,value=>action=value,fixture.Caster)) yield return frame;
                Sprint22Check("natural-expiry-actions-"+(turnBased ? "turn-based":"rtwp"),expired && action!=null && action.IsFinished,
                    new JObject { ["unit"]=target.UniqueId,["event"]="native-time-expiration-and-command",["owner"]="victim-side-native-buffs",
                        ["expired"]=expired,["canAct"]=target.Descriptor.State.CanAct,["commandFinished"]=action?.IsFinished,
                        ["roundBefore"]=roundBefore,
                        ["roundAfter"]=turnController.RoundNumber,
                        ["endedTurns"]=endedTurns,
                        ["turnTransitions"]=turnTransitions,
                        ["clockCoverage"]=_sprint22Clock?
                            .DescribeTurnCoverage(),
                        ["nativeElapsedSeconds"]=Game.Instance.TimeController.GameTime.TotalSeconds-started,
                        ["wallSeconds"]=clock.Elapsed.TotalSeconds },"Native finite effects expire and victim commands return in this combat mode.");
            }
            finally
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort;
                foreach (Buff buff in target.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value=>!original.Contains(value) &&
                        (ReferenceEquals(value,unrelated) ||
                            Sprint22OwnedControl(value))).ToArray()) buff.Remove();
                DisposeExpandedSummoningUnits(fixture.Created,new[] {second});
            }
        }
    }
}
