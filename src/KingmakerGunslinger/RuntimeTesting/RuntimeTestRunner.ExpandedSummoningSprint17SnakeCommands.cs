using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Eight closed cells, production hidden identities and automatic views.
        // No direct attack rule, buff-round callback, clock advance, positive
        // roll override, appearance removal or animation/contact write.
        private IEnumerable<int> ReviewSprint17SnakeCommands(ExpandedSummoningCorrectionFixture fixture)
        {
            var venomBlueprint = fixture.Blueprints.OfType<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>()
                .Single(value => value.name == "KMG_Summoning_Natural_Viper_Venom");
            foreach (string[] cell in SerpentineCommandReviewPolicy.Cells())
            {
                string key = cell[0], mode = cell[1], driver = cell[2];
                string id = string.Join("-", cell);
                bool manual = driver == "manual", turnBased = mode == "turn-based";
                CreateExpandedSummoningCorrectionHostile(fixture);
                UnitEntityData target = fixture.Hostile;
                string floorDetail;
                Vector3 floor = FindExpandedSummoningArtPoint(fixture.Caster, out floorDetail);
                string placement;
                PlaceExpandedSummoningUnit(target, ExpandedSummoningOpenPoint(floor, 5f,
                    new List<int>(), out placement));
                target.Descriptor.Stats.Constitution.BaseValue = 60;
                target.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                target.Descriptor.State.Size = Size.Medium;
                var blueprint = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                    value.name == "KMG_Summoning_Unit_" + (key == "viper" ? "Viper" : "ConstrictorSnake"));
                var control = new Sprint16ManualSummonControl { Caster = fixture.Caster, Blueprint = blueprint };
                UnitEntityData owner;
                if (manual) EventBus.Subscribe(control);
                try
                {
                    owner = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                        ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null, fixture.Evidence).Single();
                }
                finally { if (manual) EventBus.Unsubscribe(control); }
                fixture.Created.Add(owner);
                var master = owner.Descriptor.Master;
                var faction = owner.Faction;
                var attackFactions = owner.AttackFactions.ToArray();
                UnityEngine.Object[] resources = new UnityEngine.Object[0];
                var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
                var contacts = new JArray();
                var riders = new JArray();
                var poses = new JArray();
                var issued = new List<UnitAttack>();
                var nativeCommands = new HashSet<UnitAttack>();
                var prepared = new HashSet<TurnController>();
                var join = new UnitCombatJoinController();
                var prepare = new UnitCombatPrepareController();
                TurnController lastManualTurn = null;
                int attacksAtEstablishment = -1;
                bool subscribed = false, targetLinkCleared = false;
                var row = new JObject { ["cell"] = id, ["key"] = key, ["mode"] = mode, ["driver"] = driver,
                    ["scope"] = "production hidden-snake native command/contact slice",
                    ["inputs"] = "owned owner additionalAttackBonus100/additionalCMB100/HP100000; owned inert target HP100000/Con60/Fort-100/Medium; native BAB/dice/AI actions retained; no forced positive results",
                    ["floorSurvey"] = floorDetail, ["contacts"] = contacts, ["riders"] = riders, ["poses"] = poses,
                    ["checks"] = new JObject(), ["passed"] = false };
                _serpentineBodyRows.Add(row);
                try
                {
                    var nativeBrainActions = owner.Brain.Actions.ToArray();
                    row["brainActionsAtCreation"] = nativeBrainActions.Length;
                    SerpentineCommandReviewPolicy.SuspendAppearanceDriver(manual,
                        () => SetExpandedSummoningBrainActive(owner, false),
                        () => owner.CombatState.AIData.NextCommandTime = float.MaxValue);
                    row["controlBefore"] = Sprint16ControlObservation(owner);
                    if (manual)
                    {
                        var part = owner.Get<UnitPartSummonedMonster>();
                        if (control.Matched != 1 || part == null || !part.IsDirectlyControllable)
                            throw new InvalidOperationException("Exact manual summon-part rule did not resolve.");
                        bool capital = Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsCapital;
                        if (capital) owner.Descriptor.Master = Game.Instance.Player.MainCharacter;
                        else if (!owner.Faction.IsDirectlyControllable)
                        {
                            var main = Game.Instance.Player.MainCharacter.Value;
                            if (main == null || !main.Faction.IsDirectlyControllable)
                                throw new InvalidOperationException("No native player-faction manual-control input.");
                            owner.Descriptor.SwitchFactions(main.Faction, false);
                        }
                    }
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
                    owner.Descriptor.Stats.AdditionalCMB.BaseValue = 100;
                    owner.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    // The qualified intact-frame anchor belongs to the body
                    // under review. Move only the inert target five metres
                    // away; do not first move the snake to an unreviewed point.
                    PlaceExpandedSummoningUnit(owner, floor);
                    row["targetPlacement"] = placement;
                    row["ownerAppearanceAnchor"] = SurveyVector(floor);
                    foreach (UnitEntityData unit in new[] { owner, target })
                        if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    var visibility = new JArray(); row["visibilitySamples"] = visibility;
                    visibility.Add(Sprint17CommandAppearanceSample(owner, key, 0));
                    int settle = 0, nativePausesCleared = 0;
                    bool settled = false;
                    while (++settle <= 600)
                    {
                        // Unlike the isolated profile review this fixture has
                        // an enemy present. Keep native time running if combat
                        // auto-pause fires, just as the command phase does.
                        // The outer guard restores the original pause state.
                        if (Game.Instance.IsPaused) { nativePausesCleared++; Game.Instance.IsPaused = false; }
                        yield return 0;
                        if (settle == 1 || settle == 30 || settle == 60 || settle == 600)
                            visibility.Add(Sprint17CommandAppearanceSample(owner, key, settle));
                        var current = owner.View.GetComponent<SerpentineVisualAttachment>();
                        bool original = current != null && current.Body != null && current.Body.sharedMesh != null &&
                            current.Body.sharedMesh.name.StartsWith("KMG_" + key + "_Original_", StringComparison.Ordinal);
                        settled = SerpentineCommandReviewPolicy.ReadyToStart(original,
                            Sprint17BodyIntact(owner.View, SerpentineVisualPolicy.BodyRenderer(key)),
                            owner.Descriptor.State.CanAct, manual, owner.IsDirectlyControllable, settle);
                        if (settled) break;
                    }
                    visibility.Add(Sprint17CommandAppearanceSample(owner, key, settle));
                    row["appearanceNativePausesCleared"] = nativePausesCleared;
                    row["attachmentOutcome"] = ExpandedSummoningSerpentineViewPatch.DescribeView(owner.View);
                    row["nativeSettlementFrames"] = settle;
                    row["canActAfterSettlement"] = owner.Descriptor.State.CanAct;
                    var attachment = owner.View.GetComponent<SerpentineVisualAttachment>();
                    if (!settled)
                        throw new InvalidOperationException("Production body/control did not settle: " + row.ToString(Formatting.None));
                    resources = attachment.CaptureOwnedResources();
                    row["controlAfter"] = Sprint16ControlObservation(owner);
                    row["brain"] = owner.Blueprint.Brain == null ? null : owner.Blueprint.Brain.name;
                    owner.Memory.Add(target); target.Memory.Add(owner);
                    observer.ObserveWeaponContact = rule =>
                    {
                        if (!ReferenceEquals(rule.Target, target) || contacts.Count >= 12) return;
                        UnitAttack command = manual
                            ? issued.FirstOrDefault(value => value.IsStarted && !value.IsFinished)
                            : owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(value => value.IsStarted && !value.IsFinished);
                        if (command != null) nativeCommands.Add(command);
                        JObject contact;
                        try
                        {
                            contact = Sprint17MeasuredAttackContact(owner, target, attachment, rule, command);
                            contact["pose"] = Sprint17OriginalBodySample(owner, attachment.Body);
                            contact["geometry"] = Sprint17CommandGeometry(owner, target);
                            contact["approachRadius"] = command == null ? JValue.CreateNull() : (JToken)command.ApproachRadius;
                        }
                        catch (Exception error)
                        { contact = new JObject { ["measurementFailure"] = error.ToString(), ["finite"] = false }; }
                        contact["executing"] = command != null && command.IsStarted && !command.IsFinished;
                        contact["hit"] = rule.AttackRoll != null && rule.AttackRoll.IsHit;
                        contact["wounding"] = rule.MeleeDamage != null && rule.MeleeDamage.Damage > 0;
                        contact["modeObserved"] = CombatController.IsInTurnBasedCombat() == turnBased;
                        contacts.Add(contact);
                        if (ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) && attacksAtEstablishment < 0)
                            attacksAtEstablishment = observer.WeaponAttacks;
                    };
                    observer.ObserveRiderContact = damage =>
                    {
                        var grab = SummonGrabComponent.Find(owner);
                        int round = SummonHoldComponent.RoundsHeld(SummonHoldComponent.HeldState(owner, target, grab));
                        riders.Add(new JObject { ["frame"] = Time.frameCount, ["heldRound"] = round,
                            ["damage"] = damage.Damage, ["weaponAttacks"] = observer.WeaponAttacks,
                            ["gameTime"] = Game.Instance.Player.GameTime.TotalSeconds,
                            ["line"] = new JArray(damage.DamageBundle.Select(Sprint16DamageLine)) });
                    };
                    EventBus.Subscribe(observer); subscribed = true;
                    SetExpandedSummoningBrainActive(owner, !manual);
                    if (!manual) { owner.Brain.RestoreAvailableActions(); owner.CombatState.AIData.NextCommandTime = float.MaxValue; }
                    bool aiActionsPreserved = manual || nativeBrainActions.Length > 0 &&
                        owner.Brain.Actions.SequenceEqual(nativeBrainActions);
                    row["brainActionsBeforeCommands"] = owner.Brain.Actions.Count;
                    row["brainAvailableBeforeCommands"] = owner.Brain.AvailableActions.Count;
                    row["nativeAiActionsPreserved"] = aiActionsPreserved;
                    row["geometryBeforeCommands"] = Sprint17CommandGeometry(owner, target);
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                    owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                    join.Tick(); prepare.Tick(); Game.Instance.IsPaused = false;
                    Vector3 origin = owner.Position;
                    float travel = 0;
                    int frames = 0, firstSignature = -1;
                    bool readyEver = false, signature = false;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(55);
                    while (DateTime.UtcNow < deadline && frames < 6000)
                    {
                        yield return 0; frames++;
                        Game.Instance.IsPaused = false;
                        bool ready = !turnBased ? !CombatController.IsInTurnBasedCombat() :
                            StepSprint17SnakeTurn(owner, manual, issued, prepared, lastManualTurn, join, prepare);
                        ready &= owner.IsInGame && owner.Descriptor.State.CanAct;
                        if (ready && !readyEver)
                        {
                            readyEver = true;
                            if (!manual) owner.CombatState.AIData.NextCommandTime = Time.time;
                        }
                        if (manual) SetExpandedSummoningBrainActive(owner, false);
                        bool held = ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target);
                        // Global and initiator handler ordering is not part of
                        // the contract. Also capture after the completed native
                        // frame, without invoking or replaying the bite.
                        if (held && attacksAtEstablishment < 0) attacksAtEstablishment = observer.WeaponAttacks;
                        var venom = target.Descriptor.Buffs.GetBuff(venomBlueprint);
                        bool poison = venom != null && venom.MaybeContext != null &&
                            ReferenceEquals(venom.MaybeContext.MaybeCaster, owner);
                        signature = key == "viper" ? poison && contacts.OfType<JObject>().Any(value =>
                                (bool?)value["wounding"] == true) :
                            held && riders.OfType<JObject>().Where(value => (int)value["heldRound"] > 0)
                                .GroupBy(value => (int)value["heldRound"]).Any(group =>
                                    SerpentineCommandReviewPolicy.LaterMaintain(
                                        riders.OfType<JObject>().Count(value => (int)value["heldRound"] == 0),
                                        group.Key, group.Count(), attacksAtEstablishment, observer.WeaponAttacks));
                        if (signature && firstSignature < 0) firstSignature = frames;
                        bool pending = owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
                        if (SerpentineCommandReviewPolicy.CanIssueManual(manual, ready, issued.Count, pending,
                                signature, held, !target.Destroyed && !target.Descriptor.State.IsDead))
                        {
                            var attack = new UnitAttack(target) { ForceFullAttack = true };
                            issued.Add(attack);
                            lastManualTurn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                            owner.Commands.Run(attack);
                        }
                        foreach (var command in owner.Commands.Raw.OfType<UnitAttack>())
                            if (command.IsStarted && !command.IsFinished) nativeCommands.Add(command);
                        travel = Math.Max(travel, Vector3.Distance(origin, owner.Position));
                        if (frames % 30 == 0 && poses.Count < 8)
                            poses.Add(Sprint17OriginalBodySample(owner, attachment.Body));
                        if (signature && frames - firstSignature >= 30) break;
                    }
                    row["frames"] = frames; row["travelMetres"] = travel;
                    row["manualCommands"] = issued.Count; row["nativeCommands"] = nativeCommands.Count;
                    row["weaponAttacks"] = observer.WeaponAttacks; row["attacksAtEstablishment"] = attacksAtEstablishment;
                    row["grappleChecks"] = observer.Checks.Count(value => ReferenceEquals(value.Initiator, owner));
                    row["signatureObserved"] = signature;
                    row["lastState"] = new JObject { ["inCombat"] = owner.IsInCombat,
                        ["turnBased"] = CombatController.IsInTurnBasedCombat(),
                        ["control"] = Sprint16ControlObservation(owner), ["ownerGroup"] = owner.GroupId,
                        ["ownerGroupIsParty"] = owner.Group.IsPlayerParty,
                        ["ownerEnemy"] = owner.IsEnemy(target), ["targetEnemy"] = target.IsEnemy(owner),
                        ["ownerPosition"] = SurveyVector(owner.Position), ["targetPosition"] = SurveyVector(target.Position),
                        ["heldTarget"] = SummonHoldComponent.HeldTarget(owner) == null ? null : SummonHoldComponent.HeldTarget(owner).UniqueId,
                        ["commands"] = new JArray(owner.Commands.Raw.Where(value => value != null)
                            .Select(value => Sprint17NativeCommandState(owner, target, value))) };
                    CheckSprint17Command(row, "native-setup", readyEver && owner.Blueprint == blueprint &&
                        (manual ? owner.IsDirectlyControllable && control.Matched == 1 :
                            issued.Count == 0 && aiActionsPreserved && owner.Brain.Actions.SequenceEqual(nativeBrainActions)),
                        "production identity, native combat mode and appropriate control/AI input");
                    CheckSprint17Command(row, "approach", travel >= .25f && poses.Count > 0 &&
                        poses.OfType<JObject>().All(value => (bool)value["finite"] && (bool)value["poseFinite"]),
                        "actual native approach with finite weighted original poses");
                    CheckSprint17Command(row, "attack", observer.WeaponAttacks > 0 && nativeCommands.Count > 0 &&
                        contacts.OfType<JObject>().Any(value => (bool?)value["ownedPair"] == true &&
                            (bool?)value["executing"] == true && (bool?)value["modeObserved"] == true &&
                            (bool?)value["opportunity"] == false && (string)value["category"] == "Bite"),
                        "actual exact-pair non-opportunity bite from the native command queue");
                    CheckSprint17Command(row, "signature", signature,
                        key == "viper" ? "wounding command bite applies exact source-owned venom" :
                            "native later held round: one initial constrict, then two rider bundles with no second attack");
                    CheckSprint17Command(row, "contact", contacts.OfType<JObject>().Any(value =>
                        (string)value["category"] == "Bite" && (bool?)value["modeObserved"] == true &&
                        SerpentineCommandReviewPolicy.Contact((bool?)value["ownedPair"] == true,
                            (bool?)value["executing"] == true, (bool?)value["opportunity"] != false,
                            (bool?)value["nativeAnimationContact"] == true, (int?)value["measuredPoints"] ?? 0,
                            (float?)value["nearestGapMeters"] ?? float.NaN) &&
                        (bool?)value["pose"]?["finite"] == true && (bool?)value["pose"]?["poseFinite"] == true),
                        "native played bite clip and weighted original jaw within0.25m of target bounds");
                }
                finally
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    var ownedGrab = SummonGrabComponent.Find(owner);
                    var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                    if (turn != null && ReferenceEquals(turn.Unit, owner)) EndSprint17SnakeTurn(turn);
                    if (!owner.Destroyed)
                    {
                        InterruptExpandedSummoningFixtureCommands(owner);
                        owner.CombatState.LeaveCombat();
                        owner.Descriptor.Master = master;
                        owner.Descriptor.SwitchFactions(faction, false);
                        owner.AttackFactions.Match(attackFactions);
                        owner.Destroy(); Game.Instance.EntityDestroyer.Tick();
                    }
                    row["targetLinkBeforeNativeCleanup"] = target.Get<UnitPartGrappleTarget>() != null;
                    // The native per-unit cleanup seam is already qualified
                    // by the rules slice. Only this owned surviving prey is
                    // ticked; no direct part removal or buff-round invocation.
                    typeof(Kingmaker.Controllers.Units.UnitGrappleController).GetMethod("TickOnUnit",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Invoke(new Kingmaker.Controllers.Units.UnitGrappleController(), new object[] { target });
                    targetLinkCleared = target.Get<UnitPartGrappleTarget>() == null &&
                        (ownedGrab == null || target.Descriptor.Buffs.GetBuff(ownedGrab.GrappledBuff) == null);
                    row["targetLinkClearedAfterNativeCleanup"] = targetLinkCleared;
                    if (!target.Destroyed) { target.CombatState.LeaveCombat(); target.Destroy(); }
                    Game.Instance.EntityDestroyer.Tick();
                    if (fixture.HostileBlueprint != null) UnityEngine.Object.Destroy(fixture.HostileBlueprint);
                    fixture.Hostile = null; fixture.HostileBlueprint = null;
                    Game.Instance.Player.UpdateIsInCombat();
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate();
                }
                yield return 0; yield return 0;
                CheckSprint17Command(row, "cleanup", owner.Destroyed && target.Destroyed &&
                    targetLinkCleared && resources.Length >= 5 && resources.All(value => value == null),
                    "native owner/target destruction clears relationship and exact project-owned resources");
                row["passed"] = ((JObject)row["checks"]).Properties().Count() == 6 &&
                    ((JObject)row["checks"]).Properties().All(value => (bool)value.Value);
            }
        }

        private static JObject Sprint17CommandGeometry(UnitEntityData owner, UnitEntityData target)
        {
            var bite = SummonLimbs.PrimaryWeapon(owner);
            return new JObject { ["ownerCorpulence"] = owner.Corpulence, ["targetCorpulence"] = target.Corpulence,
                ["centerDistance"] = Vector3.Distance(owner.Position, target.Position),
                ["ownerSize"] = owner.Descriptor.State.Size.ToString(),
                ["ownerViewScale"] = SurveyVector(owner.View.transform.localScale),
                ["biteRangeFeet"] = bite == null ? JValue.CreateNull() : (JToken)bite.AttackRange.Value,
                ["nativeApproachSumMetres"] = bite == null ? JValue.CreateNull() :
                    (JToken)(owner.Corpulence + target.Corpulence + bite.AttackRange.Meters) };
        }

        private static JObject Sprint17CommandAppearanceSample(UnitEntityData owner, string key, int frame)
        {
            JObject sample = Sprint17SnakeVisibilitySample(owner, key, frame);
            sample["paused"] = Game.Instance.IsPaused;
            sample["gameTimeSeconds"] = Game.Instance.Player.GameTime.TotalSeconds;
            sample["canAct"] = owner.Descriptor.State.CanAct;
            sample["attachmentOutcome"] = ExpandedSummoningSerpentineViewPatch.DescribeView(owner.View);
            return sample;
        }

        private void CheckSprint17Command(JObject row, string check, bool passed, string expected)
        {
            ((JObject)row["checks"])[check] = passed;
            _serpentineBodyAssertions.Add(Assertion("sprint17-snake-command-" + (string)row["cell"] + "-" + check,
                expected, row.ToString(Formatting.None), passed, "Closed native command fixture; full Sprint17 not implied."));
        }

        private static void EndSprint17SnakeTurn(TurnController turn)
        {
            turn.ForceToEnd(true);
            typeof(TurnController).GetMethod("End", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null).Invoke(turn, null);
        }

        private static bool StepSprint17SnakeTurn(UnitEntityData owner, bool manual, List<UnitAttack> issued,
            HashSet<TurnController> prepared, TurnController lastManualTurn,
            UnitCombatJoinController join, UnitCombatPrepareController prepare)
        {
            if (!CombatController.IsInTurnBasedCombat()) return false;
            join.Tick(); prepare.Tick();
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null) return false;
            if (turn.Status == TurnController.TurnStatus.None && prepared.Add(turn)) turn.Prepare();
            bool acting = turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting;
            if (!ReferenceEquals(turn.Unit, owner))
            {
                if (acting) EndSprint17SnakeTurn(turn); // No party action/resource requested.
                return false;
            }
            bool pending = owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
            if (acting && manual && !pending && turn.CanEndTurnAndNoActing() &&
                (ReferenceEquals(turn, lastManualTurn) && issued.Count > 0 || SummonHoldComponent.HeldTarget(owner) != null))
            {
                EndSprint17SnakeTurn(turn);
                return false;
            }
            return acting;
        }
    }
}
