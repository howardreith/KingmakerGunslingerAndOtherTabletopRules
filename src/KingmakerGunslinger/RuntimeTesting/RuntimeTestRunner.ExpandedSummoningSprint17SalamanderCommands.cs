using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UI.SettingsUI;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Same existing command request; four additional exact Salamander cells.
        // Native commands/AI and native time only. No direct damage/attack rule,
        // positive roll override, hold tick, animation or visibility write.
        private IEnumerable<int> ReviewSprint17SalamanderCommands(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (bool turnBased in new[] { false, true })
            foreach (bool manual in new[] { true, false })
            {
                CreateExpandedSummoningCorrectionHostile(fixture);
                var target = fixture.Hostile;
                string floorDetail, placement;
                Vector3 floor = FindExpandedSummoningArtPoint(fixture.Caster, out floorDetail);
                PlaceExpandedSummoningUnit(target, ExpandedSummoningOpenPoint(floor, 5f, new List<int>(), out placement));
                target.Stats.HitPoints.BaseValue = 100000;
                var blueprint = fixture.Blueprints.OfType<Kingmaker.Blueprints.BlueprintUnit>().Single(b => SalamanderRulesPolicy.IsOwner(b.AssetGuid, b.name));
                var control = new Sprint16ManualSummonControl { Caster = fixture.Caster, Blueprint = blueprint };
                UnitEntityData owner;
                if (manual) EventBus.Subscribe(control);
                try { owner = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                    ExpandedSummoningOwnTierVariant("salamander", SummonMultiplicity.One), null, fixture.Evidence).Single();
                    fixture.Created.Add(owner); }
                finally { if (manual) EventBus.Unsubscribe(control); }
                var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
                var issued = new List<UnitAttack>(); var commands = new HashSet<UnitAttack>();
                var prepared = new HashSet<TurnController>();
                var join = new UnitCombatJoinController(); var prepare = new UnitCombatPrepareController();
                TurnController lastTurn = null;
                var strikes = new JArray(); var riders = new JArray();
                UnityEngine.Object[] resources = new UnityEngine.Object[0];
                bool subscribed = false;
                var row = new JObject { ["key"] = "salamander", ["mode"] = turnBased ? "turn-based" : "rtwp",
                    ["driver"] = manual ? "manual" : "ai", ["strikes"] = strikes, ["riders"] = riders,
                    ["fixture"] = "exact production creature; owned +100 accuracy/CMB/HP100000; inert owned targetHP100000; native BAB, damage, commands, AI, rounds and rolls" };
                string id = (turnBased ? "tb" : "rtwp") + (manual ? "-manual" : "-ai");
                try
                {
                    var brainActions = owner.Brain.Actions.ToArray();
                    SerpentineCommandReviewPolicy.SuspendAppearanceDriver(manual,
                        () => SetExpandedSummoningBrainActive(owner, false), () => owner.CombatState.AIData.NextCommandTime = float.MaxValue);
                    if (manual)
                    {
                        if (Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsCapital)
                            owner.Descriptor.Master = Game.Instance.Player.MainCharacter;
                        else if (!owner.Faction.IsDirectlyControllable)
                            owner.Descriptor.SwitchFactions(Game.Instance.Player.MainCharacter.Value.Faction, false);
                    }
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100; owner.Stats.AdditionalCMB.BaseValue = 100;
                    owner.Stats.HitPoints.BaseValue = 100000; PlaceExpandedSummoningUnit(owner, floor);
                    foreach (var unit in new[] { owner, target }) if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    foreach (int step in WaitSprint17FinalAppearance(new[] { owner })) yield return step;
                    resources = Sprint17ViewResources(owner);
                    var attachment = owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                    owner.Memory.Add(target); target.Memory.Add(owner);
                    observer.ObserveWeaponContact = rule => {
                        if (!ReferenceEquals(rule.Target, target)) return;
                        var command = owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(c => c.IsStarted && !c.IsFinished);
                        if (command != null) commands.Add(command);
                        strikes.Add(new JObject { ["weapon"] = rule.Weapon.Blueprint.AssetGuid, ["secondary"] = rule.Weapon.IsSecondary,
                            ["hit"] = rule.AttackRoll != null && rule.AttackRoll.IsHit, ["executing"] = command != null,
                            ["mode"] = CombatController.IsInTurnBasedCombat(),
                            ["damage"] = rule.MeleeDamage == null ? null : Sprint16DamageEvent(rule.MeleeDamage) });
                    };
                    observer.ObserveRiderContact = rule => riders.Add(new JObject {
                        ["round"] = SummonHoldComponent.RoundsHeld(SummonHoldComponent.HeldState(owner, target, SummonGrabComponent.Find(owner))),
                        ["attacks"] = observer.WeaponAttacks, ["damage"] = Sprint16DamageEvent(rule) });
                    EventBus.Subscribe(observer); subscribed = true;
                    SetExpandedSummoningBrainActive(owner, !manual);
                    if (!manual) { owner.Brain.RestoreAvailableActions(); owner.CombatState.AIData.NextCommandTime = float.MaxValue; }
                    bool aiPreserved = manual || brainActions.Length > 0 && owner.Brain.Actions.SequenceEqual(brainActions);
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased; Game.Instance.TurnBasedCombatController.Activate();
                    owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat(); join.Tick(); prepare.Tick();
                    Vector3 origin = owner.Position; float travel = 0; int frames = 0, establishedAttacks = -1;
                    bool readyEver = false, signature = false; var poses = new JArray();
                    DateTime deadline = DateTime.UtcNow.AddSeconds(55);
                    while (DateTime.UtcNow < deadline && frames++ < 6000)
                    {
                        Game.Instance.IsPaused = false; yield return 0;
                        bool ready = (!turnBased ? !CombatController.IsInTurnBasedCombat() :
                            StepSprint17SnakeTurn(owner, manual, issued, prepared, lastTurn, join, prepare)) && owner.Descriptor.State.CanAct;
                        if (ready && !readyEver) { readyEver = true; if (!manual) owner.CombatState.AIData.NextCommandTime = Time.time; }
                        if (manual) SetExpandedSummoningBrainActive(owner, false);
                        bool held = ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target);
                        if (held && establishedAttacks < 0) establishedAttacks = observer.WeaponAttacks;
                        signature = held && riders.OfType<JObject>().Where(r => (int)r["round"] > 0).GroupBy(r => (int)r["round"])
                            .Any(group => SerpentineCommandReviewPolicy.LaterMaintain(riders.OfType<JObject>().Count(r => (int)r["round"] == 0),
                                group.Key, group.Count(), establishedAttacks, observer.WeaponAttacks));
                        bool pending = owner.Commands.Raw.Any(c => c != null && !c.IsFinished);
                        if (SerpentineCommandReviewPolicy.CanIssueManual(manual, ready, issued.Count, pending, signature, held,
                            !target.Destroyed && !target.Descriptor.State.IsDead))
                        { var attack = new UnitAttack(target) { ForceFullAttack = true }; issued.Add(attack); lastTurn = Game.Instance.TurnBasedCombatController.CurrentTurn; owner.Commands.Run(attack); }
                        foreach (var command in owner.Commands.Raw.OfType<UnitAttack>()) if (command.IsStarted && !command.IsFinished) commands.Add(command);
                        travel = Math.Max(travel, Vector3.Distance(origin, owner.Position));
                        if (frames % 30 == 0 && poses.Count < 8) poses.Add(Sprint17OriginalBodySample(owner, attachment.Body));
                        if (signature) break;
                    }
                    row["frames"] = frames; row["travel"] = travel; row["manualCommands"] = issued.Count;
                    row["nativeCommands"] = commands.Count; row["brainActions"] = brainActions.Length; row["poses"] = poses;
                    CheckSprint17Salamander("command-" + id + "-setup", readyEver && ReferenceEquals(owner.Blueprint, blueprint) &&
                        (manual ? control.Matched == 1 && owner.IsDirectlyControllable : issued.Count == 0 && aiPreserved && owner.Brain.Actions.SequenceEqual(brainActions)), row,
                        "exact production identity, native combat mode/manual authority or unmodified natural attack AI");
                    CheckSprint17Salamander("command-" + id + "-approach", travel >= .25f && poses.Count > 0 &&
                        poses.OfType<JObject>().All(p => (bool)p["finite"] && (bool)p["poseFinite"]) && attachment.NativeActionsUnchanged, row,
                        "native movement/turns with finite original hybrid and unchanged human actions");
                    bool spear = strikes.OfType<JObject>().Any(s => (string)s["weapon"] == SalamanderTailAnimationPolicy.Spear && (bool)s["executing"]);
                    bool tail = strikes.OfType<JObject>().Any(s => (string)s["weapon"] == SalamanderRulesPolicy.TailGuid && (bool)s["secondary"] && (bool)s["executing"]);
                    CheckSprint17Salamander("command-" + id + "-attack", spear && tail && commands.Count > 0 && observer.WeaponAttacks >= 3 &&
                        strikes.OfType<JObject>().All(s => (bool)s["mode"] == turnBased), row, "real command queue delivers manufactured spear iteratives and secondary tail in the requested mode");
                    CheckSprint17Salamander("command-" + id + "-maintain", signature && riders.Count >= 3 &&
                        riders.OfType<JObject>().All(r => ((JArray)r["damage"]["chunks"]).Count == 2 &&
                            ((JArray)r["damage"]["chunks"]).Count(d => (string)d["energy"] == "Fire" && (string)d["dice"] == "1d6" && (int)d["bonus"] == 0) == 1), row,
                        "actual tail grab and later native round deliver tail plus one constrict with one fire packet each and no extra attack");
                }
                finally
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    InterruptExpandedSummoningFixtureCommands(owner); InterruptExpandedSummoningFixtureCommands(target);
                    owner.CombatState.LeaveCombat(); target.CombatState.LeaveCombat();
                    if (!owner.Destroyed) owner.Destroy(); Game.Instance.EntityDestroyer.Tick();
                    typeof(Kingmaker.Controllers.Units.UnitGrappleController).GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Invoke(new Kingmaker.Controllers.Units.UnitGrappleController(), new object[] { target });
                    row["targetLinkClean"] = target.Get<UnitPartGrappleTarget>() == null;
                    if (!target.Destroyed) target.Destroy(); Game.Instance.EntityDestroyer.Tick();
                    if (fixture.HostileBlueprint != null) UnityEngine.Object.Destroy(fixture.HostileBlueprint);
                    fixture.Hostile = null; fixture.HostileBlueprint = null;
                    Game.Instance.Player.UpdateIsInCombat(); SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                }
                yield return 0; yield return 0;
                CheckSprint17Salamander("command-" + id + "-cleanup", owner.Destroyed && target.Destroyed && (bool)row["targetLinkClean"] &&
                    resources.Length >= 29 && resources.All(r => r == null), row, "native destruction clears the exact relationship and every captured project visual resource");
            }
        }
    }
}
