using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
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
            Sprint17SnakeContactFrameProbe probe = null;
            var capturedWeapons = new HashSet<string>();
            Action<UnitEntityData, string> captureUnit = (subject, name) => {
                if (mode == null || !mode.Current) throw new InvalidOperationException("Presentation capture lost Default ownership.");
                lastCapture = Time.frameCount;
                string frame = WriteExpandedSummoningPartyCameraCapture(subject, _request.EvidenceDirectory,
                    "s32-" + key + "-" + name + ".png",
                    lillend ? subject.View.GetComponent<LillendHumanVisualAttachment>()?.Body?.name : null);
                captures.Add(new JObject { ["kind"] = name, ["frame"] = Time.frameCount, ["capture"] = frame });
                if (!frame.Contains("inFrame=true") || !frame.Contains("screenLit=true") || !frame.Contains("rendererEnabled=true"))
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
                    actor.View != null && EntityFadedIn(actor) && actor.Descriptor.State.CanAct && actor.Descriptor.State.CanMove &&
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
                    foreach (int frame in WaitPresentation32(mode, () => move.IsFinished, 25, () => {
                        if (Time.frameCount > lastCapture + 2) motion.Add(Time.unscaledDeltaTime);
                        if (Time.frameCount % 20 == 0) poses.Add(Presentation32Pose(owner, lillend));
                        if (!midCaptured && Vector3.Distance(start, owner.Position) > 2f)
                        { capture(selectedMove == 0 ? "move-out" : "move-back"); midCaptured = true; }
                    })) yield return frame;
                    bool moved = move.Result == Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success &&
                        Vector3.Distance(start, owner.Position) > 4f && Vector3.Distance(end, owner.Position) < 1.5f && midCaptured;
                    if (!moved) throw new InvalidOperationException("Native presentation move did not traverse its connected lane.");
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
                strikes.BeforeAttackRollForFixture = rule => {
                    var active = owner.Commands.Raw.OfType<UnitAttack>().Where(command => command.IsStarted &&
                        !command.IsFinished && ReferenceEquals(command.Target, target)).ToArray();
                    if (active.Length == 1) attackCommands.Add(rule, active[0]);
                };
                strikes.ObserveWeaponContact = rule => {
                    if (!ReferenceEquals(rule.Target, target)) return;
                    string weapon = rule.Weapon.Blueprint.AssetGuid;
                    attacks.Add(new JObject { ["weapon"] = weapon, ["name"] = rule.Weapon.Blueprint.name,
                        ["hit"] = rule.AttackRoll?.IsHit == true, ["damage"] = rule.MeleeDamage == null ? null : Sprint16DamageEvent(rule.MeleeDamage),
                        ["pose"] = Presentation32Pose(owner, lillend), ["tail"] = lillend ? owner.View.GetComponent<LillendHumanVisualAttachment>()?.TailAction.ActEvents : (int?)null });
                    UnitAttack command;
                    if (rule.AttackRoll?.IsHit != true || !attackCommands.TryGetValue(rule.AttackRoll, out command) ||
                        capturedWeapons.Contains(weapon)) return;
                    string captureName = "contact-" + weapon;
                    int eventFrame = Time.frameCount;
                    var eventOwner = owner; var eventTarget = target;
                    var animation = command.Animation;
                    capturedWeapons.Add(weapon);
                    pending++;
                    probe.Capture(() => { try {
                            if (!mode.Current || !ReferenceEquals(eventOwner, owner) || !ReferenceEquals(eventTarget, target) ||
                                !ReferenceEquals(command.Executor, owner) || !ReferenceEquals(command.Target, target) ||
                                Time.frameCount != eventFrame || animation == null || !animation.IsStarted ||
                                !ReferenceEquals(animation.Manager, owner.View.AnimationManager) ||
                                !owner.View.AnimationManager.ActiveActions.Contains(animation))
                                throw new InvalidOperationException("Contact capture lost its exact native event/frame/animation.");
                            var tailAction = animation.Action as LillendTailAction;
                            var active = animation.ActiveAnimation;
                            var clip = active?.GetPlayableClip();
                            bool playing = tailAction != null ? ReferenceEquals(tailAction, owner.View.GetComponent<LillendHumanVisualAttachment>()?.TailAction) &&
                                tailAction.PlayingOwnedClip && tailAction.LastActFrame == eventFrame :
                                SerpentineFinalReviewPolicy.PlayedNativeClip(true, animation.IsStarted, clip?.name,
                                    clip == null ? float.NaN : clip.length, active == null ? double.NaN : active.GetTime(),
                                    active == null ? float.NaN : active.GetWeight());
                            if (!playing) throw new InvalidOperationException("Exact contact animation is not playing.");
                            capture(captureName);
                            captures.Add(new JObject { ["kind"] = "contact-attribution", ["fileStem"] = captureName,
                                ["eventFrame"] = eventFrame, ["captureFrame"] = Time.frameCount,
                                ["commandReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command),
                                ["ruleReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(rule),
                                ["weapon"] = weapon, ["hit"] = true, ["animation"] = animation.Action.name,
                                ["clip"] = tailAction != null ? tailAction.Clips.Single().name : clip.name,
                                ["clipTime"] = tailAction != null ? tailAction.LastClipTime : active.GetTime(),
                                ["nativeHandleTime"] = animation.GetTime() });
                        }
                        catch (Exception error) { captureErrors.Add(error.ToString()); } finally { pending--; } });
                };
                EventBus.Subscribe(strikes);
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
                            if (attack.Result != Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success ||
                                strikes.WeaponAttacks <= before || !economy.Exact(attack, false) || captureErrors.Count != 0)
                                throw new InvalidOperationException("Actual full attack, cost or contact capture failed.");
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
                restore("observers", () => { if (strikes != null) EventBus.Unsubscribe(strikes); if (wounds != null) EventBus.Unsubscribe(wounds); if (probe != null) UnityEngine.Object.Destroy(probe); });
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
