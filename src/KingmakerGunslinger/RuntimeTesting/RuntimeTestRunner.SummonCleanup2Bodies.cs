using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The guarded integrator selects the established Oleg outdoor fixture.
        // This method never opens a save, changes areas, or claims foreign units.
        private IEnumerable<int> ReviewSummonCleanup2Bodies(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            string phase = (string)_request.Parameters?["reviewPhase"];
            bool gorgonOnly = phase == "gorgon-presentation";
            bool recoveryBodies = phase == "recovery-bodies";
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (phase != "bodies" && !gorgonOnly && !recoveryBodies) ||
                fixture == null || book == null || _sprint25Clock == null)
                throw new InvalidOperationException("Exact Cleanup 2 bodies fixture/book/clock required.");
            if (_expandedSummoningPlayerPathCaptureActive || _refinementUnit != null ||
                _refinementAction != null || _refinementChangedPause || _creatureReviewQueue != null)
                throw new InvalidOperationException("Another cast/refinement review already owns the shared seams.");

            string[] keys = { "giant-crab", "griffon", "giant-stag-beetle", "girallon",
                "ettin", "elephant", "stegosaurus", "brachiosaurus", "gorgon" };
            if (gorgonOnly) keys = new[] { "gorgon" };
            // Review24 already qualified unchanged Gorgon travel/contact.
            // Only the two previously refused walking lanes need this repair.
            if (recoveryBodies) keys = new[] { "girallon", "stegosaurus" };
            if (keys.Any(key => !VisualRefinementReviewPolicy.Handles(key)))
                throw new InvalidOperationException("A requested body lacks the existing complete refinement packet policy.");
            bool paused = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            bool casterBrain = fixture.Caster.IsBrainActive;
            UnitEntityData oldCaster = _creatureReviewCaster;
            UnitEntityData[] oldCapture = ExpandedSummoningRuleCapture.ToArray();
            string[] oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int oldCastCount = _expandedSummoningPlayerPathRuleCastCount;
            int oldSpawnCount = _expandedSummoningPlayerPathSpawnActionCount;
            int assertionsBefore = _creatureReviewAssertions.Count, forwarded = assertionsBefore;
            // Exact request-owned refinement fields only; readonly collections
            // are copied/restored separately and prior assertion objects survive.
            string[] fieldNames = { "_refinementUnit", "_refinementKey", "_refinementPhase",
                "_refinementFrame", "_refinementWritten", "_refinementLowWritten", "_refinementRadius",
                "_refinementDistance", "_refinementBounds", "_refinementCameraRotation", "_refinementOrigin",
                "_refinementTravel", "_refinementWasPaused", "_refinementChangedPause", "_refinementAction" };
            var fields = fieldNames.Select(name => typeof(RuntimeTestRunner).GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic)).ToArray();
            if (fields.Any(field => field == null))
                throw new MissingFieldException("Existing visual refinement state contract changed.");
            object[] oldValues = fields.Select(field => field.GetValue(this)).ToArray();
            string[] oldRecords = _refinementRecords.ToArray(), oldStills = _refinementStills.ToArray();
            var phaseWatch = Stopwatch.StartNew();
            var hooks = HarmonyInstance.Create("kmg.cleanup2.bodies." + _request.RunId);
            UnitEntityData current = null;
            bool currentBrain = false;
            var completed = new List<string>();
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                if(fixture.Hostile==null || fixture.Hostile.Destroyed) CreateExpandedSummoningCorrectionHostile(fixture);
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                _creatureReviewCaster = fixture.Caster;
                Game.Instance.IsPaused = false;
                Cleanup2Check("bodies-scope", true, new JObject {
                    ["keys"] = new JArray(keys), ["area"] = Game.Instance.CurrentlyLoadedArea?.AssetGuid,
                    ["caster"] = fixture.Caster.UniqueId, ["spellbook"] = book.Blueprint.AssetGuid,
                    ["stillsPerTarget"] = VisualRefinementReviewPolicy.StillViews.Length,
                    ["trackingPerTarget"] = VisualRefinementReviewPolicy.ExpectedTrackingFrames,
                    ["minimumNativeTravelMeters"] = VisualRefinementReviewPolicy.MinimumTravelMeters,
                    ["maximumRefinementUpdatesPerTarget"] = 512, ["maximumTargetWallSeconds"] = 150,
                    ["maximumPhaseWallSeconds"] = 900,
                    ["limits"] = "Real canonical parent/slot casts and native UnitMoveTo travel; existing refinement attack/hit animation handles and explicit one-point hit stimulus are art evidence, not actual attack-command resolution, combat timing, Gorgon breath, reload or persistence acceptance." },
                    "The exact requested body selection uses the complete qualified outdoor art packet with bounded execution; a one-Gorgon review does not qualify the other eight bodies.");

                foreach (string key in keys)
                {
                    if (phaseWatch.Elapsed.TotalSeconds > 900)
                        throw new TimeoutException("Cleanup 2 bodies phase exceeded its 900-second wall bound.");
                    SummonFamily family = key == "gorgon" ? SummonFamily.Monster : SummonFamily.NaturesAlly;
                    SummonCreatureSpec creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                    Cleanup2Route route = Cleanup2GiantRoute(fixture, key, family);
                    bool routeExact = key != "gorgon" || (route.Tier == 8 && !creature.MonsterTemplated);
                    Cleanup2Check("body-route-" + key, routeExact, new JObject {
                        ["key"] = key, ["family"] = family.ToString(), ["tier"] = route.Tier,
                        ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid,
                        ["unit"] = route.Unit.AssetGuid, ["templatePolicy"] = creature.TemplatePolicy(family).ToString(),
                        ["monsterTemplated"] = creature.MonsterTemplated,
                        ["choice"] = "Actual own-tier single canonical root, not a template child or debug spawn." },
                        "Eight bodies use their actual SNA single route; Gorgon uses its non-templated SM VIII single route.");
                    if (!routeExact) throw new InvalidOperationException("Cleanup 2 Gorgon route contract changed.");
                    current = Cleanup2CastGiant(fixture, book, route);
                    currentBrain = current.IsBrainActive;
                    var targetWatch = Stopwatch.StartNew();
                    int pulsesBefore = _sprint25Clock.Pulses;
                    double advancedBefore = _sprint25Clock.AdvancedSeconds;
                    double gameBefore = Game.Instance.Player.GameTime.TotalSeconds;
                    try
                    {
                        foreach (int frame in Cleanup2WaitAppearance(current, key))
                        {
                            if (phaseWatch.Elapsed.TotalSeconds > 900 || targetWatch.Elapsed.TotalSeconds > 150)
                                throw new TimeoutException("Cleanup 2 appearance wall bound: " + key);
                            yield return frame;
                        }
                        Cleanup2BodyReadback(current, key, "before-refinement");
                        _cleanup2BodyOrdinaryAttackProved=false;
                        foreach(int frame in Cleanup2BodyOrdinaryAttack(fixture,current,key)) yield return frame;
                        if(!_cleanup2BodyOrdinaryAttackProved) throw new InvalidOperationException("Ordinary native body attack failed: "+key);
                        int damageBeforeArt=current.Descriptor.Damage;
                        BeginVisualRefinementReview(current, key);
                        // Dialog hosts do not run ordinary Default-mode command
                        // movement. The already-owned outer clock supplies its
                        // qualified once-per-frame native action/agent pulse.
                        // Never add a second movement driver or force anim cues.
                        _sprint25WaitingForUnityAppearance = false;
                        Game.Instance.IsPaused = false;
                        int updates = 0;
                        bool finished = false;
                        while (updates < 512 && targetWatch.Elapsed.TotalSeconds <= 150 &&
                            phaseWatch.Elapsed.TotalSeconds <= 900)
                        {
                            updates++;
                            finished = StepVisualRefinementReview();
                            if (finished) break;
                            yield return 0;
                        }
                        Cleanup2ForwardBodyAssertions(ref forwarded, key);
                        bool attackHandle = _refinementRecords.Any(value => value == "animation:MainHandAttack;handle=True");
                        bool hitHandle = _refinementRecords.Any(value => value == "animation:Hit;handle=True");
                        JObject packet = Cleanup2BodyPacket(key);
                        bool packetComplete = (bool)packet["complete"];
                        bool nativeHitAbsent=current.View.AnimationManager!=null && current.View.AnimationManager.GetAction(UnitAnimationType.Hit)==null;
                        bool hitContract=hitHandle || nativeHitAbsent && current.Descriptor.Damage==damageBeforeArt+1 && _cleanup2BodyOrdinaryAttackProved;
                        bool exact = finished && packetComplete && attackHandle && hitContract;
                        Cleanup2Check("body-packet-" + key, exact, new JObject {
                            ["key"] = key, ["unit"] = current.UniqueId, ["finished"] = finished,
                            ["updates"] = updates, ["wallSeconds"] = targetWatch.Elapsed.TotalSeconds,
                            ["nativeTravelMeters"] = _refinementTravel, ["attackAnimationHandle"] = attackHandle,
                            ["hitAnimationHandle"] = hitHandle, ["packet"] = packet,
                            ["nativeHitActionAbsent"]=nativeHitAbsent,["nativeDamageStimulusBefore"]=damageBeforeArt,
                            ["nativeDamageStimulusAfter"]=current.Descriptor.Damage,["ordinaryNativeAttackProved"]=_cleanup2BodyOrdinaryAttackProved,
                            ["hitContract"]="Installed UnitAnimationManager.Execute0600160E returns the optional CreateHandle result and executes only nonnull. UnitHitFxManager.HandleMeleeAttackHit06000F33 discards that optional return. No replacement Hit clip is supplied.",
                            ["clockPulses"] = _sprint25Clock.Pulses - pulsesBefore,
                            ["clockAdvancedSeconds"] = _sprint25Clock.AdvancedSeconds - advancedBefore,
                            ["observedGameTimeDeltaSeconds"] = Game.Instance.Player.GameTime.TotalSeconds - gameBefore,
                            ["clockLimitation"] = "Existing fixture clock advances GameTime and exact native action/movement once per Unity frame under Dialog. This is a qualified art fixture, not natural game-time/combat timing proof.",
                            ["records"] = new JArray(_refinementRecords) },
                            "Complete native motion packet plus an actual ordinary attack/cost is required. A missing optional native Hit is supported only with exact one-point native damage processing and no supplied clip; available native Hit must remain present.");
                        Cleanup2BodyReadback(current, key, "after-refinement");
                        if (!finished) throw new TimeoutException("Cleanup 2 refinement frame/wall bound: " + key);
                        if (exact) completed.Add(key);
                    }
                    finally
                    {
                        // Only this exact positively captured actor's commands
                        // and animation can be stopped; shared fixture teardown
                        // retains ownership if any local cleanup step fails.
                        try { EndRefinementAnimation(); }
                        finally
                        {
                            try { if (current != null && !current.Destroyed) current.Commands.InterruptAll(true); }
                            finally
                            {
                                try { if (_refinementUnit != null) FinishVisualRefinementReview(); }
                                finally
                                {
                                    try { Cleanup2ForwardBodyAssertions(ref forwarded, key); }
                                    finally
                                    {
                                        if (current != null && !current.Destroyed)
                                            Sprint28SetBrainActivePreservingActions(current, currentBrain);
                                    }
                                }
                            }
                        }
                    }
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { current });
                    current = null;
                    yield return 0;
                }
                Cleanup2Check(gorgonOnly ? "gorgon-body-packet" : "nine-body-packets", completed.SequenceEqual(keys), new JObject {
                    ["expected"] = new JArray(keys), ["complete"] = new JArray(completed),
                    ["wallSeconds"] = phaseWatch.Elapsed.TotalSeconds,
                    ["visualApproval"] = "Pending actual frame inspection and owner review; packet completeness is technical evidence only." },
                    "Every selected body produces its full art packet, actual native attack/cost, and a demonstrated native optional-Hit contract.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => {
                    try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); }
                };
                restore("owned-animation", EndRefinementAnimation);
                restore("owned-commands", () => { if (current != null && !current.Destroyed) current.Commands.InterruptAll(true); });
                restore("owned-brain", () => { if (current != null && !current.Destroyed) Sprint28SetBrainActivePreservingActions(current, currentBrain); });
                restore("caster-commands", () => fixture.Caster.Commands.InterruptAll(true));
                restore("caster-brain", () => Sprint28SetBrainActivePreservingActions(fixture.Caster, casterBrain));
                restore("remaining-assertions", () => Cleanup2ForwardBodyAssertions(ref forwarded, "interrupted"));
                _expandedSummoningPlayerPathCaptureActive = false;
                restore("capture-hooks", () => hooks.UnpatchAll(hooks.Id));
                restore("capture-records", () => {
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCastCount;
                    _expandedSummoningPlayerPathSpawnActionCount = oldSpawnCount;
                });
                restore("refinement-fields", () => { for (int index = 0; index < fields.Length; index++) fields[index].SetValue(this, oldValues[index]); });
                restore("refinement-collections", () => {
                    _refinementRecords.Clear(); _refinementRecords.AddRange(oldRecords);
                    _refinementStills.Clear(); foreach (string value in oldStills) _refinementStills.Add(value);
                    if (_creatureReviewAssertions.Count < assertionsBefore)
                        throw new InvalidOperationException("An existing creature review assertion was removed.");
                    _creatureReviewAssertions.RemoveRange(assertionsBefore, _creatureReviewAssertions.Count - assertionsBefore);
                });
                _creatureReviewCaster = oldCaster;
                _sprint25WaitingForUnityAppearance = Cleanup2NativeDefaultPending || waiting;
                restore("pause", () => Game.Instance.IsPaused = paused);
                Cleanup2Check("bodies-owned-state-restored", failures.Count == 0, new JObject {
                    ["failures"] = new JArray(failures), ["priorCreatureAssertionCount"] = assertionsBefore,
                    ["restoredCreatureAssertionCount"] = _creatureReviewAssertions.Count,
                    ["positiveActorStillRegisteredForSharedTeardown"] = current != null && fixture.Created.Contains(current),
                    ["foreignCleanupClaimed"] = false },
                    "Owned commands/animation/hooks and prior refinement/capture/pause/brain fields restore before shared positive-actor teardown.");
                if (failures.Count != 0) throw new InvalidOperationException("Cleanup 2 body cleanup failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private bool _cleanup2BodyOrdinaryAttackProved;
        private IEnumerable<int> Cleanup2BodyOrdinaryAttack(ExpandedSummoningCorrectionFixture fixture,UnitEntityData actor,string key)
        {
            UnitEntityData target=fixture.Hostile;
            Vector3 originalTargetPosition=target.Position;
            float originalTargetOrientation=target.Orientation;
            bool actorWasInCombat=actor.IsInCombat,targetWasInCombat=target.IsInCombat;
            ResetExpandedSummoningHostile(fixture);
            actor.Commands.InterruptAll(true);actor.Commands.RemoveFinishedAndUpdateQueue();
            actor.JoinCombat();target.JoinCombat();
            JObject lane=Cleanup2CrabAttackLane(fixture,actor,target,"body-ordinary-"+key);
            _sprint25WaitingForUnityAppearance=false;Game.Instance.IsPaused=false;
            for(int frame=0;frame<8;frame++) yield return 0;
            var command=new UnitAttack(target){IsSingleAttack=true,ForceFullAttack=false};
            var observer=new Sprint24AttackObserver {Owner=actor,Target=target};
            var costs=new JArray();var endings=new JArray();var images=new JArray();
            Cleanup2NativeDefaultScope nativeMode=null;
            EventBus.Subscribe(observer);
            try
            {
                nativeMode=Cleanup2EnterNativeDefault(fixture,"ordinary-body-"+key);
                using(var economy=new FirearmReviewCosts(actor,costs))
                using(var trace=new Sprint26CommandTrace(actor,endings))
                {
                    string readyFile="ordinary-"+key+"-ready.png";
                    images.Add(new JObject {["file"]=readyFile,["result"]=Cleanup2OrdinaryAttackImage(actor,target,readyFile),["weaponEvents"]=0});
                    double queuedGameTime=Game.Instance.Player.GameTime.TotalSeconds;
                    actor.Commands.Run(command);var watch=Stopwatch.StartNew();int frames=0;
                    float[] actionSamples={0f,.12f,.28f,.48f,.72f};int actionSample=0;
                    bool capturedWeaponEvent=false;
                    while((!command.IsFinished || observer.WeaponAttacks.Count<1) && watch.Elapsed.TotalSeconds<30 && frames++<2000)
                    {
                        if(!nativeMode.Current || !Cleanup2NoNarrativeOverlay())
                            throw new InvalidOperationException("Ordinary body attack lost its owned native Default context.");
                        // Sample the actual action while it runs. The previous
                        // frame1/frame12 pair plus post-finish recovery could
                        // miss an ordinary donor's complete visible swing.
                        double sinceQueued=Game.Instance.Player.GameTime.TotalSeconds-queuedGameTime;
                        bool eventSample=observer.WeaponAttacks.Count>0 && !capturedWeaponEvent;
                        bool timedSample=actionSample<actionSamples.Length && sinceQueued>=actionSamples[actionSample];
                        if(timedSample || eventSample)
                        {
                            string file="ordinary-"+key+"-"+images.Count+".png";
                            images.Add(new JObject{["file"]=file,["result"]=Cleanup2OrdinaryAttackImage(actor,target,file),
                                ["weaponEvents"]=observer.WeaponAttacks.Count,["sinceQueuedGameSeconds"]=sinceQueued,
                                ["commandStarted"]=command.IsStarted,["commandActed"]=command.IsActed,
                                ["commandFinished"]=command.IsFinished,["firstWeaponEventSample"]=eventSample});
                            if(timedSample)actionSample++;
                            if(eventSample)capturedWeaponEvent=true;
                        }
                        yield return 0;
                    }
                    double recoveryStart=Game.Instance.Player.GameTime.TotalSeconds;
                    var recoveryWatch=Stopwatch.StartNew();
                    float[] motionTimes={.12f,.45f,.9f};
                    int motionFrame=0;
                    while(Game.Instance.Player.GameTime.TotalSeconds-recoveryStart<1.6 && recoveryWatch.Elapsed.TotalSeconds<10)
                    {
                        if(motionFrame<motionTimes.Length && Game.Instance.Player.GameTime.TotalSeconds-recoveryStart>=motionTimes[motionFrame])
                        {
                            string file="ordinary-"+key+"-motion"+motionFrame+".png";
                            images.Add(new JObject {["file"]=file,["result"]=Cleanup2OrdinaryAttackImage(actor,target,file),
                                ["afterCommandGameSeconds"]=Game.Instance.Player.GameTime.TotalSeconds-recoveryStart,["weaponEvents"]=observer.WeaponAttacks.Count});
                            motionFrame++;
                        }
                        yield return 0;
                    }
                    string recoveryFile="ordinary-"+key+"-recovery.png";
                    images.Add(new JObject {["file"]=recoveryFile,["result"]=Cleanup2OrdinaryAttackImage(actor,target,recoveryFile),["weaponEvents"]=observer.WeaponAttacks.Count});
                    var gesture=SummonAttackGestureVisual.For(actor);
                    JObject gestureState=gesture?.Describe();
                    bool recovered=gesture==null || gesture.Ready && gesture.ProveCurrentPose() &&
                        gesture.ProveSameFrameSourceState() && ((JArray)gestureState["palettePoints"]).All(value=>(float)value["ownedAngle"]<.001f);
                    bool exact=command.IsFinished && (command.Result==UnitCommand.ResultType.Success || trace.Exhausted(command)) &&
                        command.AllAttacks.Count==1 && observer.WeaponAttacks.Count==1 && observer.OwnerWeaponAttacks.Count==1 && observer.Rolls.Count==1 &&
                        observer.Damage.Count==observer.WeaponAttacks.Count(a=>a.AttackRoll!=null && a.AttackRoll.IsHit) &&
                        economy.Count(command)==1 && economy.Exact(command,false) && !command.Cutscene && !command.IsIgnoreCooldown &&
                        recovered && Game.Instance.Player.GameTime.TotalSeconds-recoveryStart>=1.6;
                    _cleanup2BodyOrdinaryAttackProved=exact;
                    Cleanup2Check("body-ordinary-command-"+key,exact,new JObject {
                        ["unit"]=actor.UniqueId,["target"]=target.UniqueId,["lane"]=lane,["costs"]=costs,["ends"]=endings,
                        ["planned"]=command.AllAttacks.Count,["weaponEvents"]=observer.WeaponAttacks.Count,["rollEvents"]=observer.Rolls.Count,
                        ["weapons"]=new JArray(observer.WeaponAttacks.Select(a=>a.Weapon.Blueprint.AssetGuid)),
                        ["images"]=images,["nativeResult"]=command.Result.ToString(),["wallSeconds"]=watch.Elapsed.TotalSeconds,
                        ["fixtureAttackRules"]=0,["forcedRolls"]=0,["clock"]="Ordinary native Default progression; manual fixture clock suspended",
                        ["nativeMode"]=nativeMode.Describe(),["gesture"]=gestureState,["privateGestureRecovered"]=recovered },
                        "The changed body performs its ordinary native single attack, real weapon/roll/damage events and one native cost, with whole-actor contact pictures.");
                }
            }
            finally
            {
                try
                {
                    EventBus.Unsubscribe(observer);actor.Commands.InterruptAll(true);actor.Commands.RemoveFinishedAndUpdateQueue();
                    if(!actorWasInCombat && actor.IsInCombat)actor.LeaveCombat();
                    if(!targetWasInCombat && target.IsInCombat)target.LeaveCombat();
                    ResetExpandedSummoningHostile(fixture);
                    // Keep the completed contact fixture out of later art
                    // routes. Only this positively owned hostile is returned
                    // to its exact pre-command native placement.
                    PlaceExpandedSummoningUnit(target,originalTargetPosition);
                    target.Orientation=originalTargetOrientation;
                }
                finally { if(nativeMode!=null)nativeMode.Dispose(); }
            }
        }

        private void Cleanup2ForwardBodyAssertions(ref int cursor, string key)
        {
            while (cursor < _creatureReviewAssertions.Count)
            {
                RuntimeTestAssertion assertion = _creatureReviewAssertions[cursor++];
                Cleanup2Check("body-refinement-" + key + "-" + assertion.Name,
                    assertion.Status == RuntimeTestStatuses.Pass, new JObject {
                        ["key"] = key, ["sourceAssertion"] = assertion.Name, ["sourceStatus"] = assertion.Status,
                        ["expected"] = assertion.Expected, ["observed"] = assertion.Observed,
                        ["evidence"] = assertion.Evidence }, assertion.Expected);
            }
        }

        private JObject Cleanup2BodyPacket(string key)
        {
            var expected = VisualRefinementReviewPolicy.StillViews.Select(view =>
                VisualRefinementReviewPolicy.StillFile(key, view)).ToList();
            string[] sequences = { "move-out", "move-back", "attack", "hit" };
            int[] frames = { RefinementLegFrames, RefinementLegFrames, RefinementActionFrames, RefinementHitFrames };
            for (int sequence = 0; sequence < sequences.Length; sequence++)
                for (int frame = 0; frame < frames[sequence]; frame += RefinementStride)
                {
                    expected.Add(key + "-refine-" + sequences[sequence] + "-" + frame.ToString("000", System.Globalization.CultureInfo.InvariantCulture) + ".png");
                    if (sequence < 2 && VisualRefinementReviewPolicy.TracksLow(key))
                        expected.Add(key + "-refine-" + sequences[sequence] + "-low-" + frame.ToString("000", System.Globalization.CultureInfo.InvariantCulture) + ".png");
                }
            string missing;
            bool policy = VisualRefinementReviewPolicy.EvidenceComplete(key, _refinementStills,
                _refinementWritten, _refinementLowWritten, _refinementTravel, out missing);
            var files = new JArray();
            bool actual = true;
            foreach (string file in expected)
            {
                var info = new FileInfo(Path.Combine(_request.EvidenceDirectory, file));
                long bytes = info.Exists ? info.Length : 0;
                actual &= bytes >= 4096;
                files.Add(new JObject { ["file"] = file, ["bytes"] = bytes, ["present"] = info.Exists });
            }
            return new JObject { ["complete"] = policy && actual, ["policyComplete"] = policy,
                ["actualFilesComplete"] = actual, ["missingPolicyEvidence"] = missing,
                ["stillCount"] = _refinementStills.Count, ["trackingCount"] = _refinementWritten,
                ["lowTrackingCount"] = _refinementLowWritten, ["expectedFileCount"] = expected.Count,
                ["files"] = files };
        }

        private void Cleanup2BodyReadback(UnitEntityData unit, string key, string state)
        {
            var skins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            JObject selection = Sprint22SelectionSurface(unit);
            string head = EttinSecondHeadVisual.Describe(unit.View);
            var attached = unit.View.GetComponentsInChildren<Transform>(true).Where(t =>
                t.name == EttinSecondHeadVisual.ObjectPrefix + "_Root").ToArray();
            bool secondHead = key != "ettin" || (head.StartsWith("attached;heads=2;", StringComparison.Ordinal) && attached.Length == 1);
            bool healthy = !unit.Destroyed && !unit.Descriptor.State.IsDead && unit.Descriptor.State.CanMove &&
                unit.View.MovementAgent != null && (bool)selection["selectionSurface"] &&
                EntityFadedIn(unit) && DissolveAmount(unit) <= .02f && skins.Any(Cleanup2BodyRenderer) && secondHead;
            Cleanup2Check("body-readback-" + key + "-" + state, healthy, new JObject {
                ["key"] = key, ["state"] = state, ["unit"] = unit.UniqueId,
                ["blueprint"] = unit.Blueprint.AssetGuid, ["prefab"] = unit.Blueprint.Prefab?.AssetId,
                ["viewInstance"] = unit.View.GetInstanceID(), ["position"] = SurveyVector(unit.Position),
                ["healthMaximum"] = unit.Stats.HitPoints.ModifiedValue, ["damage"] = unit.Descriptor.Damage,
                ["dead"] = unit.Descriptor.State.IsDead, ["canMove"] = unit.Descriptor.State.CanMove,
                ["brainActive"] = unit.IsBrainActive, ["selection"] = selection,
                ["selectionLimit"] = "Read-only native hover/collider surface health, not a simulated UI click or complete usability acceptance.",
                ["originalBodyAttachment"] = ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View),
                ["primateAttachment"] = ExpandedSummoningPrimateViewPatch.DescribeView(unit.View),
                ["ettinSecondHead"] = head,
                ["ettinAttachmentPaths"] = new JArray(attached.Select(t => Cleanup2TransformPath(unit.View.transform, t))),
                ["renderers"] = new JArray(skins.Select(skin => new JObject {
                    ["name"] = skin.name, ["path"] = Cleanup2TransformPath(unit.View.transform, skin.transform),
                    ["enabled"] = skin.enabled, ["active"] = skin.gameObject.activeInHierarchy,
                    ["mesh"] = skin.sharedMesh?.name, ["meshInstance"] = skin.sharedMesh == null ? 0 : skin.sharedMesh.GetInstanceID(),
                    ["vertexCount"] = skin.sharedMesh == null ? 0 : skin.sharedMesh.vertexCount,
                    ["bonePaletteCount"] = skin.bones?.Length ?? 0,
                    ["materials"] = new JArray(skin.sharedMaterials.Select(Cleanup2Material)) })),
                ["limits"] = "Native scalar/identity observations only; no mesh vertices, indices, skin weights or native texture pixels exported." },
                "The actual canonically summoned body has live native movement/selection surfaces, native settled materials and the exact Ettin attachment when applicable.");
        }
    }
}
