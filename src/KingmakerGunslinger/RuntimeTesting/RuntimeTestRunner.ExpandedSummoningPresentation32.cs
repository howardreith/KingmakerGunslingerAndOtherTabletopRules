using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Technical presentation, not natural AI/casting proof. All movement,
        // attacks and lifecycle use the existing native command/scheduler seams.
        private IEnumerable<int> ReviewPresentation32(ExpandedSummoningCorrectionFixture fixture, bool lillend)
        {
            var game = Game.Instance; var target = fixture.Hostile;
            string key = lillend ? "lillend-azata" : "satyr", id = "presentation-" + key;
            UnitEntityData owner = null, sibling = null, anchor = null;
            Cleanup2NativeDefaultScope mode = null;
            bool anchorCaptured = false, anchorBrain = false, oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = game.IsPaused, oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            bool immunityAdded = false, complete = false;
            int oldAttack = target.Stats.AdditionalAttackBonus.BaseValue, oldStrength = target.Stats.Strength.BaseValue;
            int oldAc = target.Stats.AC.BaseValue;
            var rig = TeleportationCastingCamera();
            if (rig == null) throw new InvalidOperationException("Native presentation camera absent.");
            var cameraPosition = rig.transform.position; var cameraTarget = rig.GetPosition();
            var cameraRotation = rig.transform.rotation;
            var owned = new List<UnityEngine.Object>(); var borrowed = new List<UnityEngine.Object>();
            var captures = new JArray(); var poses = new JArray(); var attacks = new JArray();
            var baseline = new List<float>(); var motion = new List<float>();
            var costs = new JArray(); var captureErrors = new List<string>();
            int pending = 0, lastCapture = -100;
            Sprint16RuleObserver strikes = null, wounds = null;
            Lillend32CommandStarts attackCompletion = null;
            Sprint17SnakeContactFrameProbe probe = null;
            var capturedWeapons = new HashSet<string>();
            Action<UnitEntityData, string> captureUnit = (subject, name) => {
                if (mode == null || !mode.Current) throw new InvalidOperationException("Presentation capture lost Default ownership.");
                lastCapture = Time.frameCount;
                string bodyName = lillend ? subject.View.GetComponent<LillendHumanVisualAttachment>()?.Body?.name : "satyr01";
                bool bodyIntact = Sprint17BodyIntact(subject.View, bodyName);
                string frame = WriteExpandedSummoningPartyCameraCapture(subject, _request.EvidenceDirectory,
                    "s32-" + key + "-" + name + ".png", bodyName);
                captures.Add(new JObject { ["kind"] = name, ["frame"] = Time.frameCount, ["capture"] = frame,
                    ["exactBody"] = bodyName, ["exactBodyIntact"] = bodyIntact });
                if (!frame.Contains("inFrame=true") || !frame.Contains("screenLit=true") || !frame.Contains("rendererEnabled=true") ||
                    name != "death" && !bodyIntact)
                    throw new InvalidOperationException("Gameplay camera capture is not usable: " + frame);
            };
            Action<string> capture = name => captureUnit(owner, name);
            try
            {
                if (rig == null || !fixture.Caster.Commands.Empty || fixture.Caster.IsBrainActive)
                    throw new InvalidOperationException("Exact idle native fixture/camera required.");
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; game.TurnBasedCombatController.Activate();
                game.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastSprint32IdleFixtureSummon(fixture, key);
                sibling = CastSprint32IdleFixtureSummon(fixture, key);
                foreach (var actor in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue(); }
                anchor = Sprint32OwnedQuietAnchor(fixture, id);
                anchorBrain = anchor.IsBrainActive; anchorCaptured = true; Sprint28SetBrainActivePreservingActions(anchor, false);
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                mode = Cleanup2EnterNativeDefault(fixture, ExpandedSummoningNativeProfilePolicy.Sprint32PresentationPhase);
                foreach (int frame in WaitPresentation32(mode, () => new[] { owner, sibling }.All(actor =>
                    actor.View != null && EntityFadedIn(actor) && Sprint17BodyIntact(actor.View,
                        lillend ? actor.View.GetComponent<LillendHumanVisualAttachment>()?.Body?.name : "satyr01") &&
                    actor.Descriptor.State.CanAct && actor.Descriptor.State.CanMove &&
                    actor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null), 15)) yield return frame;
                if (lillend)
                    foreach (var actor in new[] { owner, sibling })
                    {
                        var attachment = actor.View.GetComponent<LillendHumanVisualAttachment>();
                        if (attachment == null || !attachment.Live || !attachment.NativeActionsUnchanged)
                            throw new InvalidOperationException("Exact Lillend body binding missing.");
                        owned.AddRange(attachment.CaptureOwnedResources()); owned.Add(attachment);
                        borrowed.AddRange(attachment.BorrowedResources());
                    }
                Vector3 destination; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out destination, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                var selection = Sprint22SelectionSurface(owner);
                var rest = Presentation32Pose(owner, lillend);
                capture("rest");
                Sprint25Check(id + "-rest", (bool)selection["selectionSurface"] && (bool)rest["finite"],
                    new JObject { ["selection"] = selection, ["pose"] = rest, ["lane"] = lane, ["captures"] = captures },
                    "Exact native body loads at gameplay camera with a usable selection surface and finite measured scale; agent inspects the retained frame.");
                if (!(bool)selection["selectionSurface"] || !(bool)rest["finite"])
                    throw new InvalidOperationException("Native presentation rest/selection failed.");
                probe = owner.View.gameObject.AddComponent<Sprint17SnakeContactFrameProbe>();
                for (int frame = 0; frame < 60; frame++)
                {
                    if (!mode.Current) throw new InvalidOperationException("Presentation Default lease changed.");
                    game.IsPaused = false; yield return 0;
                    if (Time.frameCount > lastCapture + 2) baseline.Add(Time.unscaledDeltaTime);
                }
                int moveIndex = 0;
                int gaitBefore = lillend ? owner.View.GetComponent<LillendHumanVisualAttachment>().GaitFrames : 0;
                foreach (var end in new[] { destination, origin })
                {
                    int selectedMove = moveIndex++;
                    var start = owner.Position; bool midCaptured = false;
                    var move = new UnitMoveTo(end, .5f); move.Init(owner);
                    if (!move.CanStart) throw new InvalidOperationException("Native presentation move cannot start.");
                    owner.Commands.Run(move);
                    if (!owner.Commands.Contains(move)) throw new InvalidOperationException("Exact native presentation move was not queued.");
                    var moveSamples = new JArray(); float peakVelocity = 0, activeTravel = 0;
                    int activeFrames = 0;
                    foreach (int frame in WaitPresentation32(mode, () => move.IsFinished, 25, () => {
                        if (Time.frameCount > lastCapture + 2) motion.Add(Time.unscaledDeltaTime);
                        // Native approach movement precedes UnitCommand.Start;
                        // IsStarted is an observation, not locomotion admission.
                        bool exactActive = ReferenceEquals(owner.Commands.Move, move) && !move.IsFinished;
                        var agent = owner.View.MovementAgent as Kingmaker.View.UnitMovementAgent;
                        float speed = agent == null ? 0 : agent.Velocity.magnitude;
                        if (!exactActive || speed <= .05f) return;
                        activeFrames++; peakVelocity = Math.Max(peakVelocity, speed);
                        activeTravel = Math.Max(activeTravel, Vector3.Distance(start, owner.Position));
                        if (activeFrames % 10 == 0)
                        {
                            var pose = Presentation32Pose(owner, lillend);
                            pose["nativeVelocity"] = speed; pose["exactMoveActive"] = exactActive;
                            pose["commandIsStarted"] = move.IsStarted;
                            moveSamples.Add(pose); poses.Add(pose.DeepClone());
                        }
                        if (!midCaptured && activeTravel > 2f)
                        { capture(selectedMove == 0 ? "move-out" : "move-back"); midCaptured = true; }
                    })) yield return frame;
                    // Reuse Sprint17's technical locomotion contract: sampled
                    // native velocity, displacement and finite moving geometry.
                    // Native partial path endpoints may finish with Interrupt;
                    // retain that result, without claiming precise arrival.
                    bool moved = move.IsFinished && activeTravel > 4f && peakVelocity > .05f &&
                        moveSamples.Count >= 3 && moveSamples.All(value => (bool)value["finite"]) && midCaptured;
                    Sprint25Check(id + "-move-" + selectedMove, moved, new JObject {
                        ["result"] = move.Result.ToString(), ["start"] = new JArray(start.x, start.y, start.z),
                        ["goal"] = new JArray(end.x, end.y, end.z),
                        ["end"] = new JArray(owner.Position.x, owner.Position.y, owner.Position.z),
                        ["travelMeters"] = Vector3.Distance(start, owner.Position),
                        ["goalGapMeters"] = Vector3.Distance(end, owner.Position), ["midCaptured"] = midCaptured,
                        ["exactMoveActiveFrames"] = activeFrames, ["activeTravelMeters"] = activeTravel,
                        ["peakNativeVelocity"] = peakVelocity, ["activeMoveSamples"] = moveSamples,
                        ["preciseArrivalClaimed"] = false },
                        "The exact queued native move supplies positive-velocity travel over four metres and finite moving poses with a mid-motion frame; terminal result is retained without a precise-arrival claim.");
                    if (!moved) throw new InvalidOperationException("Native presentation locomotion evidence incomplete.");
                    // IsFinished is only the command boundary. The installed
                    // engine can still own the prior movement/path afterward.
                    // Wait for its normal retirement before a reverse command;
                    // never force Stop, clear paths, or advance controllers.
                    Func<JObject> settledState = () => {
                        var agent = owner.View.MovementAgent as Kingmaker.View.UnitMovementAgent;
                        return new JObject { ["commandRetired"] = !owner.Commands.Contains(move),
                            ["moveSlotEmpty"] = owner.Commands.Move == null,
                            ["commandsEmpty"] = owner.Commands.Empty, ["queueEmpty"] = owner.Commands.Queue.Count == 0,
                            ["agentPresent"] = agent != null, ["wantsToMove"] = agent != null && agent.WantsToMove,
                            ["velocity"] = agent == null ? float.NaN : agent.Velocity.magnitude };
                    };
                    var settlementBefore = settledState(); JObject settlement = null;
                    int settledFrames = 0, settlementFrames = 0;
                    var settlementLimit = DateTime.UtcNow.AddSeconds(12);
                    while (DateTime.UtcNow < settlementLimit && settledFrames < 3)
                    {
                        if (!mode.Current) throw new InvalidOperationException("Presentation Default lease changed during movement retirement.");
                        game.IsPaused = false; yield return 0; settlementFrames++;
                        if (!mode.Current) throw new InvalidOperationException("Presentation Default lease changed after movement retirement yield.");
                        settlement = settledState();
                        bool quiet = (bool)settlement["commandRetired"] && (bool)settlement["moveSlotEmpty"] &&
                            (bool)settlement["commandsEmpty"] && (bool)settlement["queueEmpty"] &&
                            (bool)settlement["agentPresent"] && !(bool)settlement["wantsToMove"] && (float)settlement["velocity"] <= .05f;
                        settledFrames = quiet ? settledFrames + 1 : 0;
                    }
                    Sprint25Check(id + "-move-settled-" + selectedMove, settledFrames >= 3,
                        new JObject { ["before"] = settlementBefore, ["after"] = settlement,
                            ["frames"] = settlementFrames, ["consecutiveQuietFrames"] = settledFrames },
                        "The exact native move slot, queue, agent path/request and velocity retire for three ordinary frames before the next command; no forced movement stop.");
                    if (settledFrames < 3) throw new InvalidOperationException("Native movement retirement boundary did not settle.");
                }
                var gait = lillend ? owner.View.GetComponent<LillendHumanVisualAttachment>() : null;
                bool movement = poses.Count > 0 && poses.All(p => (bool)p["finite"]) &&
                    (!lillend || gait.GaitFrames > gaitBefore && gait.GaitPeakSpeed > 0 && gait.GaitPeakOffset > 0 &&
                        gait.DescribeGait().EndsWith("stopped=False", StringComparison.Ordinal));
                Sprint25Check(id + "-movement", movement,
                    new JObject { ["poses"] = poses, ["restFrameSeconds"] = new JArray(baseline),
                        ["motionFrameSeconds"] = new JArray(motion), ["captures"] = captures,
                        ["synchronousCaptureFramesExcluded"] = true, ["measurementIsUniversalFpsThreshold"] = false },
                    "Two normal native moves traverse the connected lane with finite geometry; uncaptured frame times and gameplay-camera motion frames support agent technical inspection.");
                if (!movement) throw new InvalidOperationException("Native movement geometry/gait failed.");
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                    throw new InvalidOperationException("Unexpected preexisting target maneuver immunity.");
                target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null); immunityAdded = true;
                target.Stats.AC.BaseValue = -100; target.Stats.HitPoints.BaseValue = 100000;
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat(); game.Player.UpdateIsInCombat();
                strikes = new Sprint16RuleObserver { Owner = owner, Target = target };
                var attackCommands = new Dictionary<RuleAttackRoll, UnitAttack>();
                var projectileReleases = new Dictionary<RuleAttackWithWeapon, UnitAttack>();
                Func<RuleAttackWithWeapon, RuleDealDamage, bool> matchingImpact = (release, damage) =>
                    !damage.IsFake && damage.Damage > 0 && ReferenceEquals(damage.Initiator, owner) && ReferenceEquals(damage.Target, target) &&
                    ReferenceEquals(damage.AttackRoll, release.AttackRoll) && ReferenceEquals(damage.DamageBundle.Weapon, release.Weapon);
                strikes.BeforeAttackRollForFixture = rule => {
                    var active = owner.Commands.Raw.OfType<UnitAttack>().Where(command => command.IsStarted &&
                        !command.IsFinished && ReferenceEquals(command.Target, target)).ToArray();
                    if (active.Length == 1) attackCommands.Add(rule, active[0]);
                };
                strikes.ObserveWeaponContact = rule => {
                    if (!ReferenceEquals(rule.Target, target)) return;
                    string weapon = rule.Weapon.Blueprint.AssetGuid;
                    var attackRow = new JObject { ["weapon"] = weapon, ["name"] = rule.Weapon.Blueprint.name,
                        ["hit"] = rule.AttackRoll?.IsHit == true, ["eventMeaning"] = rule.Weapon.Blueprint.IsMelee ? "melee-contact" : "projectile-launch-not-impact", ["damage"] = rule.MeleeDamage == null ? null : Sprint16DamageEvent(rule.MeleeDamage),
                        ["pose"] = Presentation32Pose(owner, lillend), ["tail"] = lillend ? owner.View.GetComponent<LillendHumanVisualAttachment>()?.TailAction.ActEvents : (int?)null };
                    attacks.Add(attackRow);
                    UnitAttack command;
                    if (rule.AttackRoll?.IsHit != true || !attackCommands.TryGetValue(rule.AttackRoll, out command)) return;
                    if (!rule.Weapon.Blueprint.IsMelee) projectileReleases.Add(rule, command);
                    if (capturedWeapons.Contains(weapon)) return;
                    string captureName = (rule.Weapon.Blueprint.IsMelee ? "contact-" : "projectile-release-") + weapon;
                    int eventFrame = Time.frameCount;
                    var eventOwner = owner; var eventTarget = target;
                    var animation = command.Animation;
                    capturedWeapons.Add(weapon);
                    // Same proven synchronous rule-event rendering used by
                    // Sprint17 contact inspection. End-of-frame command/handle
                    // membership is not guaranteed after native retirement.
                    try {
                            var guards = new JObject {
                                ["nativeLease"] = mode.Current, ["exactOwner"] = ReferenceEquals(eventOwner, owner),
                                ["exactTarget"] = ReferenceEquals(eventTarget, target), ["executor"] = ReferenceEquals(command.Executor, owner),
                                ["commandTarget"] = ReferenceEquals(command.Target, target), ["sameFrame"] = Time.frameCount == eventFrame,
                                ["animationPresent"] = animation != null, ["animationStarted"] = animation != null && animation.IsStarted,
                                ["exactManager"] = animation != null && ReferenceEquals(animation.Manager, owner.View.AnimationManager),
                                ["activeHandle"] = animation != null && owner.View.AnimationManager.ActiveActions.Contains(animation) };
                            attackRow["capturePredicates"] = guards; attackRow["eventFrame"] = eventFrame;
                            attackRow["captureFrame"] = Time.frameCount;
                            if (!guards.Properties().All(value => (bool)value.Value))
                                throw new InvalidOperationException("Synchronous contact capture failed exact native event/frame/animation predicates.");
                            var tailAction = animation.Action as LillendTailAction;
                            var active = animation.ActiveAnimation;
                            var clip = active?.GetPlayableClip();
                            bool playing = tailAction != null ? ReferenceEquals(tailAction, owner.View.GetComponent<LillendHumanVisualAttachment>()?.TailAction) &&
                                tailAction.PlayingOwnedClip && tailAction.LastActFrame == eventFrame :
                                SerpentineFinalReviewPolicy.PlayedNativeClip(true, animation.IsStarted, clip?.name,
                                    clip == null ? float.NaN : clip.length, active == null ? double.NaN : active.GetTime(),
                                    active == null ? float.NaN : active.GetWeight());
                            attackRow["clipObservation"] = new JObject { ["playing"] = playing, ["nativeClip"] = clip?.name,
                                ["nativeTime"] = active == null ? (double?)null : active.GetTime(),
                                ["nativeWeight"] = active == null ? (float?)null : active.GetWeight(),
                                ["tailPlayingOwned"] = tailAction?.PlayingOwnedClip, ["tailLastActFrame"] = tailAction?.LastActFrame };
                            if (!playing) throw new InvalidOperationException("Exact contact animation is not playing.");
                            capture(captureName);
                            captures.Add(new JObject { ["kind"] = "contact-attribution", ["fileStem"] = captureName,
                                ["eventFrame"] = eventFrame, ["captureFrame"] = Time.frameCount,
                                ["commandReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command),
                                ["ruleReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(rule),
                                ["weapon"] = weapon, ["hit"] = true, ["eventMeaning"] = rule.Weapon.Blueprint.IsMelee ? "melee-contact" : "projectile-launch-not-impact", ["animation"] = animation.Action.name,
                                ["clip"] = tailAction != null ? tailAction.Clips.Single().name : clip.name,
                                ["clipTime"] = tailAction != null ? tailAction.LastClipTime : active.GetTime(),
                                ["nativeHandleTime"] = animation.GetTime() });
                        }
                        catch (Exception error) { captureErrors.Add(error.ToString()); }
                };
                EventBus.Subscribe(strikes);
                attackCompletion = new Lillend32CommandStarts { Owner = owner, ObserveRtwpAttackCompletion = true };
                EventBus.Subscribe(attackCompletion);
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    foreach (int set in lillend ? new[] { 0 } : new[] { 0, 1 })
                    {
                        if (owner.Body.CurrentHandEquipmentSetIndex != set)
                        {
                            var change = new UnitSwitchHandEquipmentSet(set); owner.Commands.Run(change);
                            foreach (int frame in WaitPresentation32(mode, () => change.IsFinished, 15)) yield return frame;
                            if (owner.Body.CurrentHandEquipmentSetIndex != set || !economy.Exact(change, false))
                                throw new InvalidOperationException("Native presentation weapon switch or cost failed.");
                        }
                        Vector3 near; string attackLane, attackDirection;
                        var here = Sprint24AiCombatLane(fixture.Caster, owner, target, !lillend && set == 0 ? 8f : 1.4f,
                            out near, out attackLane, out attackDirection);
                        PlaceExpandedSummoningUnit(owner, here); PlaceExpandedSummoningUnit(target, near);
                        var query = new UnitAttack(target) { ForceFullAttack = true }; query.Init(owner);
                        var requiredWeapons = query.CreateFullAttack().Select(hand => hand.Hand.MaybeWeapon.Blueprint.AssetGuid).Distinct().ToArray();
                        if (requiredWeapons.Length == 0) throw new InvalidOperationException("Empty native attack plan.");
                        for (int attempt = 0; attempt < 3 && requiredWeapons.Any(weapon => !capturedWeapons.Contains(weapon)); attempt++)
                        {
                            int before = strikes.WeaponAttacks;
                            var attack = new UnitAttack(target) { ForceFullAttack = true }; attack.Init(owner);
                            owner.Commands.Run(attack);
                            foreach (int frame in WaitPresentation32(mode, () => attack.IsFinished && pending == 0, 25,
                                () => { if (Time.frameCount % 20 == 0) poses.Add(Presentation32Pose(owner, lillend)); })) yield return frame;
                            var releases = projectileReleases.Where(pair => ReferenceEquals(pair.Value, attack)).Select(pair => pair.Key).ToArray();
                            if (releases.Length != 0)
                                foreach (int frame in WaitPresentation32(mode, () => releases.All(release => strikes.Damage.Any(damage => matchingImpact(release, damage))), 8, null)) yield return frame;
                            var impactRows = new JArray(releases.Select(release => new JObject {
                                ["releaseRule"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(release),
                                ["attackRoll"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(release.AttackRoll),
                                ["weapon"] = release.Weapon.Blueprint.AssetGuid,
                                ["damage"] = new JArray(strikes.Damage.Where(damage => matchingImpact(release, damage)).Select(Sprint16DamageEvent)) }));
                            bool completed = attackCompletion.CompletedAttack(attack, target);
                            bool costExact = economy.Exact(attack, false);
                            bool attemptPassed = completed && strikes.WeaponAttacks > before && costExact && captureErrors.Count == 0;
                            Sprint25Check(id + "-attack-set-" + set + "-attempt-" + attempt, attemptPassed,
                                new JObject { ["command"] = Sprint27AiCommand(attack), ["nativeCompletion"] = completed,
                                    ["attackEvents"] = attackCompletion.AttackRows.DeepClone(), ["weaponEventsBefore"] = before,
                                    ["weaponEventsAfter"] = strikes.WeaponAttacks, ["costExact"] = costExact, ["costs"] = costs.DeepClone(),
                                    ["requiredWeapons"] = new JArray(requiredWeapons), ["capturedWeapons"] = new JArray(capturedWeapons),
                                    ["actualAttacks"] = attacks.DeepClone(), ["projectileImpacts"] = impactRows, ["captureErrors"] = new JArray(captureErrors),
                                    ["pendingCaptures"] = pending, ["captures"] = captures.DeepClone() },
                                "Exact native final-act success and complete matching weapon plan establish completion even if native retirement later reports Interrupt; normal cost and same-frame animation capture remain required. Ranged events are projectile launch, not impact.");
                            if (!attemptPassed) throw new InvalidOperationException("Actual full attack, cost or contact capture failed; exact per-attempt predicates retained.");
                        }
                        if (requiredWeapons.Any(weapon => !capturedWeapons.Contains(weapon)))
                            throw new InvalidOperationException("No successful native contact within three normal full attacks.");
                    }
                }
                var names = attacks.OfType<JObject>().Select(row => (string)row["name"]).ToArray();
                string main = owner.Body.PrimaryHand.MaybeWeapon.Blueprint.AssetGuid;
                var tail = owner.Body.AdditionalLimbs.Select(limb => limb.MaybeWeapon).FirstOrDefault(weapon =>
                    weapon != null && SummonGrabComponent.Find(owner)?.IsGrabLimb(owner, weapon) == true);
                bool weapons = lillend ? tail != null && attacks.Count(row => (string)row["weapon"] == tail.Blueprint.AssetGuid) >= 1 &&
                    attacks.Count(row => (string)row["weapon"] == main) >= 2 : names.Contains("StandardShortbow") &&
                    names.Contains("KMG_Summoning_FeyCelestial_Satyr_Dagger") && names.Contains("KMG_Summoning_FeyCelestial_Satyr_Horns");
                Sprint25Check(id + "-attacks", weapons && captureErrors.Count == 0 && poses.All(p => (bool)p["finite"]),
                    new JObject { ["actualAttacks"] = attacks, ["costs"] = costs, ["captures"] = captures,
                        ["targetManeuverImmune"] = true, ["suppliedNormalCommands"] = true, ["naturalAiClaim"] = false },
                    "Actual native weapon events and normal costs cover each attack family; gameplay-camera contact frames require agent inspection for deformation and misleading contact.");
                if (!weapons) throw new InvalidOperationException("A required actual attack family is missing.");
                // Wound uses an actual frontal native attack, never supplied damage.
                target.Stats.AdditionalAttackBonus.BaseValue = 100; target.Stats.Strength.BaseValue = 30;
                var woundQuery = new UnitAttack(owner); woundQuery.Init(target);
                var woundPlan = woundQuery.CreateFullAttack();
                if (woundPlan.Count == 0 || woundPlan.Any(hand => hand.Hand.MaybeWeapon == null) ||
                    target.DistanceTo(owner) >= woundPlan.Min(hand => hand.Hand.MaybeWeapon.AttackRange.Meters))
                    throw new InvalidOperationException("Native wound actor has no reachable weapon plan.");
                owner.ForceLookAt(target.Position);
                foreach (int frame in WaitPresentation32(mode, () => (bool)Sprint17NativeHitFacing(owner, target, true)["eligible"], 8)) yield return frame;
                wounds = new Sprint16RuleObserver { Owner = target, Target = owner };
                bool frontalWound = false, hitCaptured = false;
                var lifePoses = new JArray();
                wounds.ObserveWeaponContact = rule => {
                    if (rule.MeleeDamage == null || rule.MeleeDamage.Damage <= 0 || rule.AttackRoll == null ||
                        !(bool)Sprint17NativeHitFacing(owner, target, rule.AttackRoll.IsHit)["eligible"]) return;
                    if (frontalWound) return;
                    frontalWound = true;
                    pending++;
                    probe.Capture(() => { try { capture("wound"); hitCaptured = true; }
                        catch (Exception error) { captureErrors.Add(error.ToString()); } finally { pending--; } });
                };
                EventBus.Subscribe(wounds);
                for (int attempt = 0; attempt < 3 && !frontalWound; attempt++)
                {
                    var wound = new UnitAttack(owner); wound.Init(target); target.Commands.Run(wound);
                    foreach (int frame in WaitPresentation32(mode, () => wound.IsFinished && pending == 0, 20,
                        () => lifePoses.Add(Presentation32NativePose(owner, true)))) yield return frame;
                }
                var hitAction = owner.View.AnimationManager.GetAction(UnitAnimationType.Hit);
                bool hitActionPresent = hitAction?.Clips != null && hitAction.Clips.Any(clip => clip != null && clip.length > 0);
                if (!wounds.Damage.Any(rule => rule.Damage > 0) || !frontalWound || !hitCaptured || captureErrors.Count > 0 ||
                    owner.Descriptor.State.IsDead || lifePoses.Any(row => !(bool)row["finite"]) ||
                    hitActionPresent && !lifePoses.Any(row => (bool)row["played"]))
                    throw new InvalidOperationException("Actual nonlethal frontal wound was not observed.");
                Sprint25Check(id + "-wound", true, new JObject { ["poses"] = lifePoses.DeepClone(),
                    ["nativeHitActionPresent"] = hitActionPresent, ["captures"] = captures,
                    ["fixtureAttackerStrength"] = 30, ["fixtureAttackerAccuracy"] = 100, ["maximumNormalAttempts"] = 3 },
                    "Actual frontal positive damage preserves finite body and native Hit playback when the carrier supplies it; no substitute animation.");
                var summonBlueprint = BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
                var liveMarker = sibling.Buffs.Enumerable.Single(buff => ReferenceEquals(buff.Blueprint, summonBlueprint));
                var liveDeadline = liveMarker.EndTime;
                var deathAction = owner.View.AnimationManager.GetAction(UnitAnimationType.Prone);
                bool deathActionPresent = deathAction?.Clips != null && deathAction.Clips.Any(clip => clip != null && clip.length > 0);
                Kingmaker.Designers.GameHelper.KillUnit(owner, target);
                bool deathPlayed = false;
                foreach (int frame in WaitPresentation32(mode, () => owner.Descriptor.State.IsDead && (!deathActionPresent || deathPlayed), 10,
                    () => { var pose = Presentation32NativePose(owner, false); lifePoses.Add(pose);
                        if (!(bool)pose["finite"]) throw new InvalidOperationException("Nonfinite death pose.");
                        deathPlayed |= (bool)pose["played"]; })) yield return frame;
                if (!(bool)Presentation32Pose(owner, lillend)["finite"]) throw new InvalidOperationException("Nonfinite native dead body.");
                capture("death");
                var deadMarker = owner.Buffs.Enumerable.SingleOrDefault(buff => ReferenceEquals(buff.Blueprint, summonBlueprint));
                var expiry = deadMarker == null ? new JObject { ["markerAlreadyRetiredByNativeDeath"] = true } : ExpireSprint16OwnedBuff(owner, deadMarker);
                foreach (int frame in WaitPresentation32(mode, () => Sprints3031SourceAbsent(owner), 20)) yield return frame;
                if (sibling.Destroyed || sibling.Descriptor.State.IsDead || !sibling.Buffs.Enumerable.Contains(liveMarker) ||
                    liveMarker.EndTime != liveDeadline || liveDeadline <= game.Player.GameTime)
                    throw new InvalidOperationException("Live sibling/original summon marker changed before expiry.");
                captureUnit(sibling, "live-before-expiry");
                var liveExpiry = ExpireSprint16OwnedBuff(sibling, liveMarker);
                using (var screens = new NativeIconScreenEvidence(_request))
                {
                    foreach (int frame in screens.Capture(id + "-expiry-start", liveExpiry, () => mode.Current)) yield return frame;
                    foreach (int frame in WaitPresentation32(mode, () => Sprints3031SourceAbsent(sibling), 20)) yield return frame;
                    foreach (int frame in screens.Capture(id + "-retired", new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId },
                        () => mode.Current && Sprints3031SourceAbsent(owner) && Sprints3031SourceAbsent(sibling))) yield return frame;
                }
                for (int frame = 0; frame < 8; frame++) { if (!mode.Current) throw new InvalidOperationException("Presentation lease changed during retirement."); yield return 0; }
                bool absent = Sprints3031SourceAbsent(owner) && Sprints3031SourceAbsent(sibling) &&
                    owned.All(value => value == null) && borrowed.All(value => value != null);
                Sprint25Check(id + "-lifecycle", absent, new JObject { ["deathExpiry"] = expiry, ["liveExpiry"] = liveExpiry,
                    ["nativeLifePoses"] = lifePoses,
                    ["nativeDeathActionPresent"] = deathActionPresent, ["nativeDeathPlayed"] = deathPlayed,
                    ["ownedResources"] = owned.Count, ["borrowedResources"] = borrowed.Count, ["captures"] = captures,
                    ["fixtureDisposalsBeforeVerdict"] = 0 }, "Native death and scheduled expiry retire both owned bodies/resources while borrowed resources survive.");
                if (!absent) throw new InvalidOperationException("Native presentation lifecycle retirement failed.");
                complete = true;
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error); } };
                restore("observers", () => { if (strikes != null) EventBus.Unsubscribe(strikes); if (attackCompletion != null) EventBus.Unsubscribe(attackCompletion); if (wounds != null) EventBus.Unsubscribe(wounds); if (probe != null) UnityEngine.Object.Destroy(probe); });
                restore("owned-combat", () => {
                    foreach (var actor in new[] { owner, sibling, target, fixture.Caster, anchor }.Where(actor => actor != null && !actor.Destroyed))
                    { if (actor != anchor) actor.Commands.InterruptAll(true); if (actor.IsInCombat) actor.LeaveCombat(); }
                    game.Player.UpdateIsInCombat(); });
                restore("owned-bodies", () => DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(actor => actor != null && !actor.Destroyed)));
                restore("target", () => {
                    if (immunityAdded) target.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers);
                    target.Stats.AC.BaseValue = oldAc; target.Stats.AdditionalAttackBonus.BaseValue = oldAttack; target.Stats.Strength.BaseValue = oldStrength;
                    ResetExpandedSummoningHostile(fixture); });
                restore("native-default", () => { mode?.Dispose();
                    if (Cleanup2NativeDefaultPending) throw new InvalidOperationException("Native Default restoration remains pending."); });
                if (anchorCaptured) restore("owned-anchor", () => {
                    if (anchor == null || anchor.Destroyed || !ReferenceEquals(anchor.Blueprint, _summoningProfileActor)) throw new InvalidOperationException("Owned anchor changed.");
                    Sprint28SetBrainActivePreservingActions(anchor, anchorBrain); });
                restore("settings", () => {
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagement;
                    if (Cleanup2NativeDefaultPending) throw new InvalidOperationException("Foreign mode preserved for outer rollback.");
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate();
                    game.IsPaused = oldPause; });
                restore("camera", () => {
                    if (Cleanup2NativeDefaultPending) throw new InvalidOperationException("Camera restore deferred with foreign mode.");
                    rig.ScrollToImmediately(cameraTarget); rig.transform.position = cameraPosition; rig.transform.rotation = cameraRotation;
                    typeof(Kingmaker.View.CameraRig).GetField("m_TargetPosition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(rig, cameraTarget);
                    if (rig.transform.position != cameraPosition || rig.transform.rotation != cameraRotation || rig.GetPosition() != cameraTarget)
                        throw new InvalidOperationException("Original gameplay camera not restored."); });
                Sprint25Check(id + "-finalizers", errors.Count == 0, new JObject { ["errors"] = new JArray(errors), ["complete"] = complete },
                    "Independent owned fixture, settings and camera finalizers preserve original failure and exact rollback.");
                if (complete && errors.Count > 0) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }
        private IEnumerable<int> WaitPresentation32(Cleanup2NativeDefaultScope mode, Func<bool> ready, int seconds, Action observe = null)
        {
            var until = DateTime.UtcNow.AddSeconds(seconds);
            do {
                if (!mode.Current) throw new InvalidOperationException("Presentation native Default lease changed.");
                if (ready()) yield break;
                if (DateTime.UtcNow >= until) throw new InvalidOperationException("Presentation native wait expired.");
                Game.Instance.IsPaused = false; yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Presentation native Default lease changed after yield.");
                observe?.Invoke();
            } while (true);
        }
        private static JObject Presentation32Pose(UnitEntityData unit, bool lillend)
        {
            if (unit?.View == null) return new JObject { ["finite"] = false };
            var renderers = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy && renderer.sharedMesh != null).ToArray();
            bool finite = renderers.Length > 0 && renderers.All(renderer => Cleanup2Finite(renderer.bounds.center) &&
                Cleanup2Finite(renderer.bounds.size) && renderer.bones.All(bone => bone == null || Cleanup2Finite(bone.position)));
            var attachment = unit.View.GetComponent<LillendHumanVisualAttachment>();
            var body = lillend && attachment?.Body != null ? Lillend32BodySample(unit, attachment) : null;
            finite &= !lillend || body != null && (bool)body["finite"] && (bool)body["nativeActionsUnchanged"];
            return new JObject { ["finite"] = finite, ["frame"] = Time.frameCount, ["scale"] = new JArray(unit.View.transform.lossyScale.x, unit.View.transform.lossyScale.y, unit.View.transform.lossyScale.z),
                ["renderers"] = new JArray(renderers.Select(renderer => new JObject { ["name"] = renderer.name,
                    ["width"] = renderer.bounds.size.x, ["height"] = renderer.bounds.size.y, ["depth"] = renderer.bounds.size.z })),
                ["lillend"] = body };
        }
        private static JObject Presentation32NativePose(UnitEntityData unit, bool hit)
        {
            if (unit?.View == null) return new JObject { ["played"] = false, ["viewAbsent"] = true, ["finite"] = false };
            var manager = unit.View.AnimationManager;
            var rows = new JArray();
            foreach (var handle in manager.ActiveActions.OfType<UnitAnimationActionHandle>())
            {
                var active = handle.ActiveAnimation; var clip = active?.GetPlayableClip();
                bool selected = ReferenceEquals(handle.Action, manager.GetAction(hit ? UnitAnimationType.Hit : UnitAnimationType.Prone)) &&
                    (hit || manager.IsDead);
                if (!SerpentineFinalReviewPolicy.PlayedNativeClip(selected, handle.IsStarted,
                    clip?.name, clip == null ? float.NaN : clip.length,
                    active == null ? double.NaN : active.GetTime(), active == null ? float.NaN : active.GetWeight())) continue;
                rows.Add(new JObject { ["action"] = handle.Action.name, ["clip"] = clip.name,
                    ["time"] = active.GetTime(), ["weight"] = active.GetWeight() });
            }
            var pose = Presentation32Pose(unit, unit.View.GetComponent<LillendHumanVisualAttachment>() != null);
            return new JObject { ["frame"] = Time.frameCount, ["hit"] = hit, ["played"] = rows.Count > 0,
                ["handles"] = rows, ["finite"] = (bool)pose["finite"], ["pose"] = pose };
        }
    }
}
