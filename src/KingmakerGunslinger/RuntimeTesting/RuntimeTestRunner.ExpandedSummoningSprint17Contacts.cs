using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Group;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private BlueprintUnit _serpentineBodyPrototype;
        private bool _serpentineEnvironmentCaptured, _serpentineMode, _serpentinePause;
        private TimeSpan _serpentineClock;
        private UnitEntityData[] _serpentineAwake, _serpentineSelected;
        private UnitEntityData _serpentineGroup;
        private readonly List<UnityEngine.Object> _serpentineContactPrototypes = new List<UnityEngine.Object>();

        private UnitEntityData CreateSprint17ContactTarget(ExpandedSummoningCorrectionFixture fixture, Vector3 point)
        {
            // One fresh, inert, request-local faction and actor per contact
            // cell. No faction attacks the party, and no private faction is
            // used to satisfy the owner's native controllability predicate.
            var faction = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter.Faction);
            _serpentineContactPrototypes.Add(faction);
            faction.name = "KMG_Runtime_Sprint17_ContactNeutral";
            faction.Peaceful = faction.AlwaysEnemy = faction.Neutral = faction.IsDirectlyControllable = false;
            faction.Dummy = null; faction.AttackFactions = new BlueprintFaction[0];
            var prototype = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            _serpentineContactPrototypes.Add(prototype);
            prototype.name = "KMG_Runtime_Sprint17_ContactTarget";
            prototype.IsCheater = true; prototype.Faction = faction;
            var target = Game.Instance.EntityCreator.SpawnUnit(prototype, point, Quaternion.identity, fixture.Scene);
            if (target == null) throw new InvalidOperationException("Native contact target creation failed.");
            fixture.Created.Add(target);
            Game.Instance.EntityCreator.Tick();
            SetExpandedSummoningBrainActive(target, false);
            target.Descriptor.Stats.HitPoints.BaseValue = 100000;
            target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
            PlaceExpandedSummoningUnit(target, point);
            return target;
        }

        private static JObject Sprint17NativeCommandState(UnitEntityData owner, UnitEntityData target, UnitCommand command)
        {
            var agent = owner.View.MovementAgent as Kingmaker.View.UnitMovementAgent;
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            return new JObject { ["frame"] = Time.frameCount, ["paused"] = Game.Instance.IsPaused,
                ["turnBasedSetting"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue,
                ["inTurnBasedCombat"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                ["currentTurnOwner"] = turn == null || turn.Unit == null ? null : turn.Unit.UniqueId,
                ["gameTimeSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                ["canStart"] = command.CanStart, ["started"] = command.IsStarted, ["finished"] = command.IsFinished,
                ["acted"] = command.IsActed, ["result"] = command.Result.ToString(),
                ["animation"] = Sprint17IssuedAnimationObservation(command as UnitAttack),
                ["queued"] = owner.Commands.Contains(command), ["inCombat"] = owner.IsInCombat,
                ["ownerGroup"] = owner.GroupId, ["ownerGroupIsParty"] = owner.Group.IsPlayerParty,
                ["ownerIsPlayerFaction"] = owner.IsPlayerFaction,
                ["velocity"] = agent == null ? JValue.CreateNull() : (JToken)SurveyVector(agent.Velocity),
                ["position"] = SurveyVector(owner.Position), ["ownerControl"] = Sprint16ControlObservation(owner),
                ["conditions"] = new JArray(Enum.GetValues(typeof(UnitCondition)).Cast<UnitCondition>()
                    .Where(value => owner.Descriptor.State.HasCondition(value)).Select(value => value.ToString())),
                ["target"] = target == null ? JValue.CreateNull() : (JToken)new JObject {
                    ["id"] = target.UniqueId, ["position"] = SurveyVector(target.Position),
                    ["group"] = target.GroupId, ["groupIsParty"] = target.Group.IsPlayerParty,
                    ["destroyed"] = target.Destroyed, ["dead"] = target.Descriptor.State.IsDead,
                    ["conscious"] = target.Descriptor.State.IsConscious, ["hpDamage"] = target.Descriptor.Damage,
                    ["ownerEnemy"] = owner.IsEnemy(target), ["targetEnemy"] = target.IsEnemy(owner),
                    ["conditions"] = new JArray(Enum.GetValues(typeof(UnitCondition)).Cast<UnitCondition>()
                        .Where(value => target.Descriptor.State.HasCondition(value)).Select(value => value.ToString())) } };
        }

        private static JObject Sprint17IssuedAnimationObservation(UnitAttack command)
        {
            // Observe this exact issued command, not an incidental attack in
            // the owner's queue. All native calls below are read-only getters.
            var handle = command == null ? null : command.Animation;
            var result = new JObject { ["frame"] = Time.frameCount, ["handlePresent"] = handle != null,
                ["observedActedClip"] = false };
            if (handle == null) return result;
            result["handleStarted"] = handle.IsStarted; result["handleActed"] = handle.IsActed;
            result["handleTime"] = handle.GetTime(); result["weaponStyle"] = handle.AttackWeaponStyle.ToString();
            result["animationTargetDistance"] = handle.AttackTargetDistance;
            result["variant"] = handle.Variant;
            result["actionClass"] = handle.Action == null ? null : handle.Action.GetType().FullName;
            result["actionName"] = handle.Action == null ? null : handle.Action.name;
            var active = handle.ActiveAnimation;
            result["activeAnimationPresent"] = active != null;
            if (active == null) return result;
            result["activeClass"] = active.GetType().FullName;
            result["activeState"] = active.State.ToString(); result["activeTime"] = active.GetTime();
            result["activeWeight"] = active.GetWeight(); result["activeSpeed"] = active.GetSpeed();
            AnimationClip clip = active.GetPlayableClip();
            result["clip"] = Sprint17ClipMetadata(clip);
            result["observedActedClip"] = SerpentineRigSurveyPolicy.IsObservedAttackClip(
                handle.IsStarted, handle.IsActed, true, clip == null ? null : clip.name,
                clip == null ? float.NaN : clip.length, active.GetTime());
            return result;
        }

        private static JToken Sprint17ClipMetadata(AnimationClip clip)
        {
            // Metadata only: no native curves, mesh, texture or asset export.
            return clip == null ? (JToken)JValue.CreateNull() : new JObject {
                ["name"] = clip.name, ["durationSeconds"] = clip.length,
                ["events"] = new JArray(clip.events.Select(value => new JObject {
                    ["function"] = value.functionName, ["time"] = value.time })) };
        }

        private static JArray Sprint17NativeHandAttackCensus(UnitEntityData owner)
        { return Sprint17NativeHandAttackCensus(kind => owner.View.AnimationManager.GetAction(kind)); }

        // The detached census supplies exact serialized actions directly; it
        // never initializes/plays a prefab manager just to inspect settings.
        private static JArray Sprint17NativeHandAttackCensus(Func<UnitAnimationType, UnitAnimationAction> resolve)
        {
            var rows = new JArray();
            foreach (UnitAnimationType kind in new[] { UnitAnimationType.MainHandAttack, UnitAnimationType.OffHandAttack })
            {
                var action = resolve(kind);
                var row = new JObject { ["kind"] = kind.ToString(),
                    ["actionClass"] = action == null ? null : action.GetType().FullName,
                    ["actionName"] = action == null ? null : action.name };
                rows.Add(row);
                var hand = action as UnitAnimationActionHandAttack;
                if (hand == null) continue;
                // The exact audited native settings field is read, never
                // rewritten or invoked to choose/play a variant.
                FieldInfo field = typeof(UnitAnimationActionHandAttack).GetField("m_Settings",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var settings = field == null ? null : field.GetValue(hand) as Array;
                if (settings == null || settings.Length > 32)
                    throw new InvalidOperationException("Native hand-attack settings census unavailable or unbounded.");
                var styles = new JArray(); row["styles"] = styles;
                foreach (object setting in settings)
                {
                    if (setting == null) throw new InvalidOperationException("Null native hand-attack setting.");
                    Type type = setting.GetType();
                    var clips = type.GetField("Variants").GetValue(setting) as AnimationClip[];
                    if (clips == null || clips.Length > 32)
                        throw new InvalidOperationException("Native hand-attack variants unavailable or unbounded.");
                    styles.Add(new JObject { ["style"] = type.GetField("Style").GetValue(setting).ToString(),
                        ["variants"] = new JArray(clips.Select(Sprint17ClipMetadata)),
                        ["rend"] = Sprint17ClipMetadata(type.GetField("Rend").GetValue(setting) as AnimationClip),
                        ["charge"] = Sprint17ClipMetadata(type.GetField("Charge").GetValue(setting) as AnimationClip) });
                }
            }
            return rows;
        }

        private void CaptureSprint17BodyEnvironment()
        {
            if (_serpentineEnvironmentCaptured) throw new InvalidOperationException("Environment already captured.");
            _serpentineMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            _serpentinePause = Game.Instance.IsPaused;
            _serpentineClock = Game.Instance.Player.GameTime;
            _serpentineAwake = Game.Instance.State.AwakeUnits.ToArray();
            _serpentineSelected = Game.Instance.UI.SelectionManagerPC.SelectedUnits.ToArray();
            _serpentineGroup = GroupController.Instance.GetCurrentCharacter();
            _serpentineEnvironmentCaptured = true;
        }

        private void RestoreSprint17BodyEnvironment()
        {
            if (!_serpentineEnvironmentCaptured) return;
            Game.Instance.State.AwakeUnits.Clear();
            Game.Instance.State.AwakeUnits.AddRange(_serpentineAwake);
            Game.Instance.Player.UpdateIsInCombat();
            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = _serpentineMode;
            Game.Instance.TurnBasedCombatController.Activate();
            GroupController.Instance.SelectUnit(_serpentineGroup);
            Game.Instance.UI.SelectionManagerPC.MultiSelect(_serpentineSelected.Select(value => value.View).ToArray(), false);
            Game.Instance.Player.GameTime = _serpentineClock;
            Game.Instance.IsPaused = _serpentinePause;
            bool restored = Game.Instance.State.AwakeUnits.SequenceEqual(_serpentineAwake) &&
                Game.Instance.UI.SelectionManagerPC.SelectedUnits.SequenceEqual(_serpentineSelected) &&
                ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), _serpentineGroup) &&
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == _serpentineMode &&
                Game.Instance.IsPaused == _serpentinePause && Game.Instance.Player.GameTime == _serpentineClock;
            _serpentineBodyAssertions.Add(Assertion("sprint17-body-environment-restored",
                "exact awake, selection, group, mode, pause and clock snapshots", "restored=" + restored, restored,
                "Native guarded fixture restoration; no save write."));
            _serpentineEnvironmentCaptured = false;
        }

        private UnitEntityData SummonSprint17BodyCarrier(ExpandedSummoningCorrectionFixture fixture,
            string key, out Sprint16ManualSummonControl control)
        {
            bool snake = SerpentineVisualPolicy.IsSnake(key);
            BlueprintUnit published = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                value.name == "KMG_Summoning_Unit_" + (snake ? "PurpleWorm" : "Salamander"));
            BlueprintUnit prototype = published;
            if (!snake)
            {
                if (key != "salamander" || _serpentineBodyPrototype != null ||
                    published.Prefab.AssetId != SerpentineVisualPolicy.ClubShieldPrefab)
                    throw new InvalidOperationException("Unreviewed hybrid prototype source.");
                var twoHand = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                    value.AssetGuid == "f080877221934ea40b29e1d9fa71bc1c");
                if (twoHand.Prefab.AssetId != SerpentineVisualPolicy.TwoHandPrefab)
                    throw new InvalidOperationException("Two-hand native view identity changed.");
                prototype = _serpentineBodyPrototype = UnityEngine.Object.Instantiate(published);
                prototype.name = "KMG_Runtime_Sprint17_SalamanderTwoHandBody";
                prototype.Prefab = twoHand.Prefab;
                // No native NPC facts, inventory, faction, loot or weapons
                // are imported. No registration or shared blueprint mutation.
            }
            control = new Sprint16ManualSummonControl { Caster = fixture.Caster, Blueprint = prototype };
            EventBus.Subscribe(control);
            try
            {
                if (snake) return CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                    ExpandedSummoningOwnTierVariant("purple-worm", SummonMultiplicity.One), null, fixture.Evidence).Single();
                var variant = ExpandedSummoningOwnTierVariant("salamander", SummonMultiplicity.One);
                var data = new AbilityData(ResolveExpandedSummoningExecution(fixture.Blueprints, variant, null),
                    fixture.Caster.Descriptor);
                var context = new AbilityExecutionContext(data, data.CalculateParams(),
                    new TargetWrapper(fixture.Caster.Position), Rulebook.CurrentContext);
                var rule = new RuleSummonUnit(fixture.Caster, prototype, fixture.Caster.Position, 100.Rounds(), 20)
                    { Context = context, Reason = context };
                Rulebook.Trigger(rule);
                if (rule.SummonedUnit == null) throw new InvalidOperationException("Native hybrid summon did not resolve.");
                fixture.Created.Add(rule.SummonedUnit);
                Game.Instance.EntityCreator.Tick();
                if (!ReferenceEquals(rule.SummonedUnit.Blueprint, prototype) ||
                    published.Prefab.AssetId != SerpentineVisualPolicy.ClubShieldPrefab)
                    throw new InvalidOperationException("Prototype or published view identity changed.");
                return rule.SummonedUnit;
            }
            finally { EventBus.Unsubscribe(control); }
        }

        private IEnumerable<int> ReviewSprint17NativeAttacks(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SerpentineVisualAttachment attachment, string key, JObject row)
        {
            var actions = new JArray();
            foreach (UnitAnimationSpecialAttackType kind in Enum.GetValues(typeof(UnitAnimationSpecialAttackType)))
            {
                var action = owner.View.AnimationManager.GetAction(kind);
                if (action == null) continue;
                var special = action as Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionSpecialAttack;
                var clips = action.Clips == null ? null : action.Clips.Where(value => value != null).ToArray();
                actions.Add(new JObject { ["kind"] = kind.ToString(), ["actionClass"] = action.GetType().FullName,
                    ["declaredType"] = special == null ? null : special.AttackType.ToString(),
                    ["clips"] = clips == null ? JValue.CreateNull() : (JToken)new JArray(clips.Select(clip =>
                        new JObject { ["name"] = clip.name, ["duration"] = clip.length })) });
            }
            row["nativeSpecialAttackCensus"] = actions;
            row["nativeHandAttackCensus"] = Sprint17NativeHandAttackCensus(owner);
            owner.Descriptor.Stats.HitPoints.BaseValue = 100000;
            // Preserve native BAB/iterative count, especially the spear.
            owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
            row["contactFixtureInputs"] = "owned donor-only actor: additional attack bonus100/HP100000, native BAB/iteratives retained; fresh isolated owned target maneuver-immune; brain off; real RTWP full-attack command; no forced hit/animation/contact";
            var directions = new List<int>();
            string placement;
            Vector3 targetPoint = ExpandedSummoningOpenPoint(owner.Position, 1.5f, directions, out placement);
            var target = CreateSprint17ContactTarget(fixture, targetPoint);
            row["attackTargetPlacement"] = placement;
            string originalGroup = owner.GroupId;
            BlueprintFaction originalFaction = owner.Faction;
            BlueprintFaction[] originalAttackFactions = owner.AttackFactions.ToArray();
            // Native UnitValidationController repairs a PlayerFaction actor's
            // separate group back into the party group. A request-local copy
            // preserves the native controllability flag but not player-faction
            // identity. No native faction or party actor is mutated, and no
            // controller is bypassed. This is a visual research fixture only.
            if (!originalFaction.IsDirectlyControllable)
                throw new InvalidOperationException("Contact actor lacks its native controllable faction input.");
            var contactFaction = UnityEngine.Object.Instantiate(originalFaction);
            _serpentineContactPrototypes.Add(contactFaction);
            contactFaction.name = "KMG_Runtime_Sprint17_ContactOwner";
            contactFaction.Peaceful = contactFaction.AlwaysEnemy = contactFaction.Neutral = false;
            contactFaction.Dummy = null;
            contactFaction.AttackFactions = new[] { target.Faction };
            owner.Descriptor.SwitchFactions(contactFaction, true);
            owner.GroupId = "KMG_Runtime_Sprint17_" + owner.UniqueId;
            owner.AttackFactions.Match(new[] { target.Faction });
            var isolation = new JObject { ["ownerGroupIsParty"] = owner.Group.IsPlayerParty,
                ["targetGroupIsParty"] = target.Group.IsPlayerParty, ["ownerControl"] = Sprint16ControlObservation(owner),
                ["ownerEnemy"] = owner.IsEnemy(target), ["targetEnemy"] = target.IsEnemy(owner),
                ["unrelatedEnemyCount"] = fixture.UnitsBefore.OfType<UnitEntityData>()
                    .Count(value => value.IsEnemy(target) || target.IsEnemy(value)),
                ["targetNativeFactionReference"] = fixture.Blueprints.OfType<BlueprintFaction>()
                    .Any(value => ReferenceEquals(value, target.Faction)) };
            row["contactIsolation"] = isolation;
            bool isolated = !(bool)isolation["ownerGroupIsParty"] && !(bool)isolation["targetGroupIsParty"] &&
                (bool)isolation["ownerEnemy"] && (bool)isolation["targetEnemy"] &&
                (int)isolation["unrelatedEnemyCount"] == 0 && owner.IsDirectlyControllable && !owner.IsPlayerFaction;
            _serpentineBodyAssertions.Add(Assertion("sprint17-owned-contact-isolation-" + key,
                "only the two owned groups are enemies; native control predicate; private faction not player identity",
                isolation.ToString(), isolated, "No native faction/party/group/AI mutation; no save write."));
            owner.Memory.Add(target); target.Memory.Add(owner);
            foreach (UnitEntityData unit in new[] { owner, target })
                if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
            Game.Instance.TurnBasedCombatController.Activate();
            owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
            var join = new UnitCombatJoinController(); var prepare = new UnitCombatPrepareController();
            join.Tick(); prepare.Tick(); Game.Instance.IsPaused = false;
            var contacts = new JArray(); row["nativeAttackContacts"] = contacts;
            var poses = new JArray(); row["attackPoseSamples"] = poses;
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            var attack = new UnitAttack(target) { ForceFullAttack = true };
            observer.ObserveWeaponContact = rule =>
            {
                if (!ReferenceEquals(rule.Target, target) || contacts.Count >= 12) return;
                JObject contact;
                try
                {
                    contact = Sprint17MeasuredAttackContact(owner, target, attachment, rule, attack);
                    contact["bodyPose"] = Sprint17OriginalBodySample(owner, attachment.Body);
                }
                catch (Exception error)
                {
                    // Keep the actual event even if its measurement fails.
                    // EventBus must not swallow the row and hide the cause.
                    contact = new JObject { ["category"] = rule.Weapon.Blueprint.Category.ToString(),
                        ["weapon"] = rule.Weapon.Blueprint.AssetGuid, ["finite"] = false,
                        ["measurementFailure"] = error.GetType().Name + ":" + error.Message };
                }
                contact["issuedCommandExecuting"] = attack.IsStarted && !attack.IsFinished;
                contacts.Add(contact);
                if (contacts.Count <= 3)
                    contact["supportingFrame"] = CaptureSprint17OriginalBodyFrame(owner, attachment.Body,
                        key + "-attack-" + contacts.Count);
            };
            EventBus.Subscribe(observer);
            try
            {
                if (!isolated) throw new InvalidOperationException("Request-local contact isolation failed closed.");
                // Let the owned hostile's native appearance settle too.
                for (int frame = 0; frame < 90; frame++) yield return 0;
                bool stableIsolation = Sprint17ContactPairIsolated(fixture, owner, target);
                row["contactIsolationAfterSettlement"] = stableIsolation;
                if (!stableIsolation) throw new InvalidOperationException("Contact isolation changed during native settlement.");
                attack.Init(owner);
                row["attackBefore"] = Sprint17NativeCommandState(owner, target, attack);
                row["attackCanStart"] = attack.CanStart;
                owner.Commands.Run(attack);
                row["attackQueued"] = owner.Commands.Contains(attack);
                DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                int frames = 0;
                string required = key == "salamander" ? "Spear" : "Bite";
                while (DateTime.UtcNow < deadline && frames++ < 1800)
                {
                    yield return 0;
                    if (frames % 10 == 0 && poses.Count < 160)
                    {
                        var sample = Sprint17OriginalBodySample(owner, attachment.Body);
                        sample["command"] = Sprint17NativeCommandState(owner, target, attack);
                        bool pair = Sprint17ContactPairIsolated(fixture, owner, target);
                        stableIsolation &= pair;
                        sample["contactPairIsolated"] = pair;
                        poses.Add(sample);
                    }
                    if (attack.IsFinished) break; // A terminal command cannot create another contact.
                }
                row["attackAfter"] = Sprint17NativeCommandState(owner, target, attack);
                row["attackFrames"] = frames;
                row["attackStarted"] = attack.IsStarted; row["attackFinished"] = attack.IsFinished;
                row["attackRows"] = observer.Attacks.Count; row["damageRows"] = observer.Damage.Count;
                row["damageProvenance"] = new JArray(observer.Damage.Select(value => new JObject {
                    ["ownerInitiated"] = ReferenceEquals(value.Initiator, owner),
                    ["initiator"] = value.Initiator == null ? null : value.Initiator.UniqueId,
                    ["target"] = value.Target == null ? null : value.Target.UniqueId }));
                row["weaponEvents"] = new JArray(observer.Attacks.Select(value => new JObject {
                    ["category"] = value.Weapon == null ? null : value.Weapon.Blueprint.Category.ToString(),
                    ["weapon"] = value.Weapon == null ? null : value.Weapon.Blueprint.AssetGuid }));
                stableIsolation &= Sprint17ContactPairIsolated(fixture, owner, target);
                _serpentineBodyAssertions.Add(Assertion("sprint17-contact-isolation-retained-" + key,
                    "isolated owned enemy pair survives settlement and every command observation",
                    "retained=" + stableIsolation, stableIsolation,
                    "No repeated group override, native validation bypass or unrelated enemy."));
                Func<JObject, bool> measuredIssued = value => SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(
                    (bool?)value["ownedPair"] == true, (bool?)value["issuedCommandExecuting"] == true,
                    (bool?)value["opportunity"] != false, (bool?)value["nativeAnimationContact"] == true,
                    (int?)value["measuredPoints"] ?? 0, (float?)value["nearestGapMeters"] ?? float.NaN);
                bool complete = contacts.OfType<JObject>().Any(value =>
                    (string)value["category"] == required && measuredIssued(value)) &&
                    contacts.OfType<JObject>().All(value => (bool?)value["finite"] == true) &&
                    poses.Count >= 3 && poses.OfType<JObject>().All(value => (bool)value["poseFinite"]) && attack.IsStarted &&
                    observer.Damage.All(value => ReferenceEquals(value.Initiator, owner)) &&
                    (key != "salamander" || contacts.OfType<JObject>().Any(value =>
                        (bool?)value["exactHybridTail"] == true && measuredIssued(value)));
                _serpentineBodyAssertions.Add(Assertion("sprint17-native-attack-research-" + key,
                    "actual issued native primary attack with finite exact current-pose measurements",
                    "contacts=" + contacts.Count + ";poses=" + poses.Count + ";started=" + attack.IsStarted,
                    complete, "Research completeness only. Measured gaps and grip failures remain unqualified, never waived."));
                JObject[] attackPoses = poses.OfType<JObject>().Concat(contacts.OfType<JObject>()
                    .Select(value => value["bodyPose"] as JObject).Where(value => value != null)).ToArray();
                _serpentineBodyAssertions.Add(Assertion("sprint17-attack-ground-support-" + key,
                    "all actual sampled attack poses keep original support within -2..15mm of the measured floor",
                    "samples=" + attackPoses.Length + ";minimum=" + attackPoses.Min(value =>
                        (float?)value["lowestVertexFloor"]["clearance"]),
                    poses.Count >= 3 && attackPoses.All(value => {
                        float? clearance = (float?)value["lowestVertexFloor"]["clearance"];
                        return (bool?)value["poseFinite"] == true && clearance.HasValue &&
                            clearance.Value >= -.002f && clearance.Value <= .015f;
                    }), "No floor clamp, per-frame mesh deformation, native rig change or unsampled-pose claim."));
                bool closeContact = contacts.OfType<JObject>().Any(value =>
                    (string)value["category"] == required && measuredIssued(value) &&
                    (float?)value["nearestGapMeters"] <= .25f) &&
                    (key != "salamander" || contacts.OfType<JObject>().Any(value =>
                        (bool?)value["exactHybridTail"] == true && measuredIssued(value) &&
                        (float?)value["nearestGapMeters"] <= .25f));
                _serpentineBodyAssertions.Add(Assertion("sprint17-issued-attack-contact-" + key,
                    "issued native primary and required hybrid tail each measure within quarter metre",
                    "contact=" + closeContact, closeContact,
                    "Original weighted vertices; spear includes the full conservative native-bounds uncertainty."));
                if (key == "salamander")
                {
                    bool twoPalm = contacts.OfType<JObject>().Any(value =>
                        (string)value["category"] == "Spear" && measuredIssued(value) &&
                        (string)value["spearMountStatus"] == "two-native-palms:instance-weapon-only" &&
                        (int?)value["spearMountFrame"] == (int?)value["frame"] &&
                        (float?)value["RPalmToSpearAxisMeters"] <= .01f &&
                        (float?)value["LPalmToSpearAxisMeters"] <= .01f);
                    _serpentineBodyAssertions.Add(Assertion("sprint17-spear-two-palm-contact-grip",
                        "both unchanged native palms lie within one centimetre of the existing shaft at the exact attack frame",
                        "twoPalm=" + twoPalm, twoPalm,
                        "Read-only independent world measurement; fixture never applies a mount, animation or rule."));
                    var renderer = attachment.SpearFilter.GetComponent<MeshRenderer>();
                    var materialController = owner.View.GetComponentInChildren<Kingmaker.Visual.MaterialEffects.StandardMaterialController>(true);
                    var driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(materialController);
                    bool adopted = attachment.SpearFilter.sharedMesh == attachment.NativeSpearMesh &&
                        renderer.enabled && renderer.gameObject.activeInHierarchy &&
                        renderer.sharedMaterials.All(value => driven != null && driven.Contains(value));
                    row["spearRenderer"] = new JObject { ["enabled"] = renderer.enabled,
                        ["active"] = renderer.gameObject.activeInHierarchy, ["nativeMeshAlive"] = attachment.NativeSpearMesh != null,
                        ["materialsAdopted"] = adopted };
                    _serpentineBodyAssertions.Add(Assertion("sprint17-native-spear-instance-ownership",
                        "existing native weapon renderer uses live borrowed spear mesh and controller-owned material clone",
                        row["spearRenderer"].ToString(), adopted,
                        "Native mesh/texture borrowed, never destroyed. This alone does not prove grip or attack contact."));
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                InterruptExpandedSummoningFixtureCommands(owner);
                owner.CombatState.LeaveCombat(); target.CombatState.LeaveCombat();
                target.Descriptor.State.RemoveConditionAll(UnitCondition.ImmuneToCombatManeuvers);
                owner.Descriptor.SwitchFactions(originalFaction, false);
                owner.AttackFactions.Match(originalAttackFactions);
                owner.GroupId = originalGroup;
                target.Destroy(); Game.Instance.EntityDestroyer.Tick();
                Game.Instance.Player.UpdateIsInCombat();
            }
        }

        private static bool Sprint17ContactPairIsolated(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target)
        {
            return !owner.IsPlayerFaction && !owner.Group.IsPlayerParty && !target.Group.IsPlayerParty &&
                owner.IsDirectlyControllable && owner.IsEnemy(target) && target.IsEnemy(owner) &&
                !fixture.UnitsBefore.OfType<UnitEntityData>().Any(value => value.IsEnemy(target) ||
                    target.IsEnemy(value) || value.IsEnemy(owner) || owner.IsEnemy(value));
        }

        private static JObject Sprint17MeasuredAttackContact(UnitEntityData owner, UnitEntityData target,
            SerpentineVisualAttachment attachment, RuleAttackWithWeapon rule, UnitAttack issued)
        {
            JObject animation = Sprint17IssuedAnimationObservation(issued);
            var result = new JObject { ["frame"] = Time.frameCount, ["category"] = rule.Weapon.Blueprint.Category.ToString(),
                ["weapon"] = rule.Weapon.Blueprint.AssetGuid, ["opportunity"] = rule.IsAttackOfOpportunity,
                ["ownedPair"] = ReferenceEquals(rule.Initiator, owner) && ReferenceEquals(rule.Target, target),
                ["exactHybridTail"] = rule.Weapon.Blueprint.name == "KMG_Summoning_Special_Salamander_Tail",
                ["nativeAnimationContact"] = (bool)animation["observedActedClip"],
                ["issuedAnimation"] = animation, ["finite"] = false };
            var targetMesh = target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.enabled && value.sharedMesh != null && value.sharedMesh.vertexCount >= 100)
                .OrderByDescending(value => value.bones.Length).FirstOrDefault();
            if (targetMesh == null) { result["failure"] = "no native target renderer"; return result; }
            Vector3[] points;
            float transverseRadius = 0;
            if (rule.Weapon.Blueprint.Category == WeaponCategory.Spear)
            {
                MeshFilter filter = attachment.SpearFilter;
                Bounds spearBounds = filter.sharedMesh.bounds;
                Vector3 a = filter.transform.TransformPoint(spearBounds.center - Vector3.up * spearBounds.extents.y);
                Vector3 b = filter.transform.TransformPoint(spearBounds.center + Vector3.up * spearBounds.extents.y);
                Vector3 axis = b - a;
                if (axis.sqrMagnitude <= 0 || spearBounds.extents.y <= spearBounds.extents.x ||
                    spearBounds.extents.y <= spearBounds.extents.z)
                    throw new InvalidOperationException("Native spear no longer has the surveyed nonzero Y shaft.");
                points = new[] { a, b };
                transverseRadius = new[] { -1, 1 }.SelectMany(x => new[] { -1, 1 }.Select(z =>
                    filter.transform.TransformVector(new Vector3(x * spearBounds.extents.x, 0,
                        z * spearBounds.extents.z)).magnitude)).Max();
                result["nativeMeshReadable"] = filter.sharedMesh.isReadable;
                result["spearMountStatus"] = attachment.SpearMountStatus;
                result["spearMountFrame"] = attachment.SpearMountFrame;
                result["nativeBoundsSize"] = SurveyVector(spearBounds.size);
                result["transverseUncertaintyMeters"] = transverseRadius;
                result["shaftEndCentres"] = new JArray(SurveyVector(a), SurveyVector(b));
                foreach (string side in new[] { "R", "L" })
                {
                    Transform palm = attachment.Body.bones.Single(value => value.name == side + "_Palm");
                    Vector3 onAxis = a + axis * Mathf.Clamp01(Vector3.Dot(palm.position - a, axis) / axis.sqrMagnitude);
                    result[side + "PalmToSpearAxisMeters"] = Vector3.Distance(onAxis, palm.position);
                }
                result["contactEnd"] = "forward positive-Y bound only";
                result["poseMethod"] = "native bounds Y end centres; contact uses forward positive-Y end only; gap includes transverse uncertainty; NOT surface vertices";
            }
            else
            {
                var body = attachment.Body;
                bool bite = rule.Weapon.Blueprint.Category == WeaponCategory.Bite;
                var anchors = new HashSet<int>(Enumerable.Range(0, body.bones.Length).Where(index =>
                    bite ? body.bones[index].name == "Head" || body.bones[index].name == "Jaw_Down" :
                        body.bones[index].name.StartsWith("tail", StringComparison.Ordinal)));
                Vector3[] all = Sprint17OriginalWorldVertices(body);
                BoneWeight[] weights = body.sharedMesh.boneWeights;
                points = all.Where((value, index) => {
                    BoneWeight w = weights[index];
                    return anchors.Contains(w.boneIndex0) && w.weight0 >= .25f ||
                        anchors.Contains(w.boneIndex1) && w.weight1 >= .25f ||
                        anchors.Contains(w.boneIndex2) && w.weight2 >= .25f ||
                        anchors.Contains(w.boneIndex3) && w.weight3 >= .25f;
                }).ToArray();
                // The worm's retained donor sting is not a printed snake
                // attack or a tail binding. Preserve it as an unmeasured row.
                if (points.Length == 0) { result["notApplicable"] = "unprinted donor attack"; result["finite"] = true; return result; }
                result["vertices"] = points.Length;
                result["poseMethod"] = "sum(weight * bone.localToWorld * bindpose * originalVertex)";
            }
            Bounds bounds = targetMesh.bounds;
            bool finite = points.Length > 0 && points.All(value => SerpentineRigSurveyPolicy.Finite(value.x) &&
                SerpentineRigSurveyPolicy.Finite(value.y) && SerpentineRigSurveyPolicy.Finite(value.z));
            result["finite"] = finite; result["measuredPoints"] = points.Length;
            result["targetBoundsCenter"] = SurveyVector(bounds.center); result["targetBoundsSize"] = SurveyVector(bounds.size);
            result["actorPosition"] = SurveyVector(owner.Position); result["targetPosition"] = SurveyVector(target.Position);
            if (finite)
            {
                Vector3 nearest = rule.Weapon.Blueprint.Category == WeaponCategory.Spear
                    ? points[1] : points.OrderBy(value =>
                        Vector3.Distance(value, bounds.ClosestPoint(value))).First();
                Vector3 targetPoint = bounds.ClosestPoint(nearest);
                float gap = Vector3.Distance(nearest, targetPoint);
                result["nearestOriginalPoint"] = SurveyVector(nearest);
                result["nearestTargetBoundsPoint"] = SurveyVector(targetPoint);
                result["targetMinusOriginalPoint"] = SurveyVector(targetPoint - nearest);
                if (rule.Weapon.Blueprint.Category == WeaponCategory.Spear)
                {
                    result["endCentreGapMeters"] = gap;
                    float? upper = SerpentineRigSurveyPolicy.ConservativeSpearEndGap(gap, transverseRadius);
                    if (!upper.HasValue) throw new InvalidOperationException("Nonfinite conservative spear bound.");
                    gap = upper.Value;
                }
                result["nearestGapMeters"] = gap; result["withinQuarterMetre"] = gap <= .25f;
            }
            return result;
        }
    }
}
