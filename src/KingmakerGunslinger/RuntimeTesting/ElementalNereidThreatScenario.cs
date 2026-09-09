using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.Visual.FogOfWar;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class ElementalNereidScenario
    {
        // Run within the established RTWP actor/controller lifetime. These are
        // real native command notifications and area geometry, not calls to the
        // trait's event handlers. No navigation or save acceptance is implied.
        private static void ThreatCommands(ElementalUndineFeatScenario.PortalHarness fixture,
            UnitEntityData caster, UnitEntityData attacker, BlueprintAbility main,
            BlueprintAbilityResource resource, BlueprintBuff aura, UnitActionController controller,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            var fascinated = aura.GetComponent<ElementalNereidAuraState>().Fascinated;
            var target = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                new Vector3(0, 0, 2), "NereidThreatSubject");
            attacker.Position = new Vector3(0, 0, 3.5f);
            target.Stats.HitPoints.BaseValue = 10000;
            target.Stats.SaveWill.BaseValue = -100;
            attacker.Stats.SaveWill.BaseValue = 100;
            ElementalSpellAffinityScenario.Advance(attacker.Descriptor,
                Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 2);
            attacker.CombatState.JoinCombat(); attacker.CombatState.OnNewRound();
            attacker.Memory.Add(target).Visible = true;
            target.Memory.Add(attacker).Visible = true;
            var observer = new Saves();
            var blind = ScriptableObject.CreateInstance<AddCondition>();
            blind.Condition = UnitCondition.Blindness;
            var blindControl = Control("ThreatBlindness", blind);
            EventBus.Subscribe(observer);
            var commands = new List<UnitCommand>();
            var controls = new Dictionary<BlueprintAbility, UnitEntityData>();
            var mind = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
            mind.Descriptor = SpellDescriptor.MindAffecting;
            var actorImmunity = Control("ThreatActorImmunity", mind);
            try
            {
                Check(assertions, rows, "threat-native-visibility",
                    target.Memory.ContainsVisible(attacker) && attacker.IsEnemy(target),
                    "native visible memory and hostile faction identify the threat actor");

                var source = Activate(caster, main, resource, aura, observer);
                var canceled = new UnitAttack(target);
                commands.Add(canceled); attacker.Commands.Run(canceled);
                attacker.Commands.InterruptAll(true); attacker.Commands.RemoveFinishedAndUpdateQueue();
                Check(assertions, rows, "queued-unarmed-attack-cancel",
                    !canceled.IsStarted && target.Descriptor.HasFact(fascinated),
                    "an unstarted unarmed attack causes no spell, weapon-draw or attack-start threat");
                End(source);

                foreach (bool visible in new[] { false, true })
                {
                    target.Memory.Add(attacker).Visible = visible;
                    source = Activate(caster, main, resource, aura, observer);
                    int damage = target.Damage;
                    var attack = new UnitAttack(target);
                    commands.Add(attack);
                    StartThreat(attacker, attack, controller);
                    Tick(Area(source));
                    Check(assertions, rows, "native-attack-start-" + (visible ? "visible" : "unseen"),
                        attack.IsStarted && target.Damage == damage &&
                        target.Descriptor.HasFact(fascinated) != visible,
                        "started=" + attack.IsStarted + ";visible=" + visible +
                            ";damageBefore=" + damage + ";damageAfter=" + target.Damage +
                            ";fascinated=" + target.Descriptor.HasFact(fascinated));
                    StopThreat(attacker, attack); End(source);
                }

                target.Memory.Add(attacker).Visible = true;
                target.Descriptor.AddFact(blindControl);
                source = Activate(caster, main, resource, aura, observer);
                var blindedAttack = new UnitAttack(target);
                commands.Add(blindedAttack); StartThreat(attacker, blindedAttack, controller);
                Check(assertions, rows, "blind-visual-threat-control",
                    blindedAttack.IsStarted && target.Descriptor.HasFact(fascinated),
                    "blindness prevents the visible attack-start cue; actual damage remains an independent break");
                StopThreat(attacker, blindedAttack); End(source);
                target.Descriptor.RemoveFact(blindControl);

                foreach (var type in new[] { AbilityType.Spell, AbilityType.SpellLike, AbilityType.Supernatural })
                {
                    // A request-owned harmless personal ability uses the real
                    // native spell command. No donor effects, spell slots, or
                    // production type flags are changed for this control.
                    var ability = UnityEngine.Object.Instantiate(main);
                    ability.name = "KMG_Runtime_Nereid_Threat_" + type;
                    ability.Type = type;
                    ability.ComponentsArray = type == AbilityType.Spell
                        ? new BlueprintComponent[] { ScriptableObject.CreateInstance<SpellComponent>() }
                        : Array.Empty<BlueprintComponent>();
                    var position = type == AbilityType.Spell ? new Vector3(4, 0, 0) :
                        type == AbilityType.SpellLike ? new Vector3(-4, 0, 0) : new Vector3(0, 0, -4);
                    var spellActor = fixture.SpawnFixtureUnit(null, attacker.Blueprint.Faction,
                        position, "NereidThreat" + type);
                    spellActor.Descriptor.AddFact(actorImmunity);
                    controls.Add(ability, spellActor); spellActor.Descriptor.AddFact(ability);
                    spellActor.CombatState.JoinCombat(); spellActor.CombatState.OnNewRound();
                    target.Memory.Add(spellActor).Visible = true;
                    source = Activate(caster, main, resource, aura, observer);
                    var use = new UnitUseAbility(new AbilityData(spellActor.Descriptor.Abilities.GetAbility(ability)),
                        new TargetWrapper(spellActor));
                    commands.Add(use); StartThreat(spellActor, use, controller);
                    Tick(Area(source));
                    bool shouldBreak = type == AbilityType.Spell || type == AbilityType.SpellLike;
                    Check(assertions, rows, "native-casting-threat-" + type,
                        use.IsStarted && target.Descriptor.HasFact(fascinated) != shouldBreak,
                        "native type=" + type + ";started=" + use.IsStarted +
                            ";fascinated=" + target.Descriptor.HasFact(fascinated));
                    StopThreat(spellActor, use); End(source);
                    if (type == AbilityType.Spell)
                        SensorySpellThreats(caster, spellActor, target, ability, main, resource, aura,
                            fascinated, blindControl, controller, observer, rows, assertions);
                    if (type == AbilityType.Spell)
                        InvisibleSpellThreats(caster, spellActor, target, ability, main, resource, aura,
                            fascinated, controller, observer, rows, assertions);
                    spellActor.Descriptor.RemoveFact(ability);
                }
                attacker.Descriptor.AddFact(actorImmunity);
                ApproachAndDrawing(caster, attacker, target, main, resource, aura, fascinated, observer, rows, assertions, blindControl);
                var spellControl = controls.Single(value => value.Key.Type == AbilityType.Spell);
                Obstruction(caster, target, main, resource, aura, fascinated, observer, rows, assertions,
                    spellControl.Value, spellControl.Key, controller, blindControl);
            }
            finally
            {
                foreach (var command in commands) StopThreat(command.Executor ?? attacker, command);
                target.Descriptor.RemoveFact(blindControl);
                foreach (var pair in controls) {
                    pair.Value.Descriptor.RemoveFact(pair.Key);
                    pair.Value.Descriptor.RemoveFact(actorImmunity);
                    UnityEngine.Object.Destroy(pair.Key);
                }
                attacker.Descriptor.RemoveFact(actorImmunity);
                UnityEngine.Object.Destroy(actorImmunity);
                foreach (var source in caster.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, aura)).ToArray())
                    End(source);
                EventBus.Unsubscribe(observer);
                UnityEngine.Object.Destroy(blindControl);
            }
        }

        private static void SensorySpellThreats(UnitEntityData caster, UnitEntityData actor, UnitEntityData target,
            BlueprintAbility spell, BlueprintAbility main, BlueprintAbilityResource resource, BlueprintBuff aura,
            BlueprintBuff fascinated, BlueprintFeature blind, UnitActionController controller, Saves observer,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            foreach (bool precise in new[] { false, true })
            foreach (int feet in new[] { 1, 30 })
            {
                var nativeSense = ScriptableObject.CreateInstance<Kingmaker.Designers.Mechanics.Facts.Blindsense>();
                nativeSense.Blindsight = precise; nativeSense.Range = feet.Feet();
                var feature = Control("ThreatSense" + precise + feet, nativeSense);
                Kingmaker.UnitLogic.Buffs.Buff source = null;
                UnitCommand command = null;
                bool visible = target.Memory.Add(actor).Visible;
                try {
                    target.Descriptor.AddFact(blind); target.Descriptor.AddFact(feature);
                    target.Memory.Add(actor).Visible = false; ResetThreatSight(target);
                    bool reached = target.Get<Kingmaker.UnitLogic.Parts.UnitPartBlindsense>().Reach(actor);
                    source = Activate(caster, main, resource, aura, observer);
                    command = new UnitUseAbility(new AbilityData(actor.Descriptor.Abilities.GetAbility(spell)), new TargetWrapper(actor));
                    StartThreat(actor, command, controller); Tick(Area(source));
                    bool expected = precise && feet == 30;
                    Check(assertions, rows, "native-sensory-spell-" + precise + "-" + feet,
                        command.IsStarted && reached == (feet == 30) &&
                        target.Descriptor.HasFact(fascinated) != expected,
                        "blind=true;sharedVisible=false;blindsight=" + precise + ";rangeFeet=" + feet +
                        ";nativeReach=" + reached + ";fascinated=" + target.Descriptor.HasFact(fascinated));
                } finally {
                    if (command != null) StopThreat(actor, command);
                    if (source != null) End(source);
                    target.Descriptor.RemoveFact(feature); target.Descriptor.RemoveFact(blind);
                    target.Memory.Add(actor).Visible = visible; UnityEngine.Object.Destroy(feature);
                }
            }
        }

        private static void InvisibleSpellThreats(UnitEntityData caster, UnitEntityData actor, UnitEntityData target,
            BlueprintAbility spell, BlueprintAbility main, BlueprintAbilityResource resource, BlueprintBuff aura,
            BlueprintBuff fascinated, UnitActionController controller, Saves observer,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            var invisible = ScriptableObject.CreateInstance<AddCondition>(); invisible.Condition = UnitCondition.Invisible;
            var see = ScriptableObject.CreateInstance<AddCondition>(); see.Condition = UnitCondition.SeeInvisibility;
            var invisibleControl = Control("ThreatInvisible", invisible);
            var seeControl = Control("ThreatSeeInvisible", see);
            Kingmaker.UnitLogic.Buffs.Buff source = null;
            UnitCommand command = null;
            bool visible = target.Memory.Add(actor).Visible;
            try {
                actor.Descriptor.AddFact(invisibleControl); target.Memory.Add(actor).Visible = true;
                foreach (bool canSee in new[] { false, true }) {
                    if (canSee) target.Descriptor.AddFact(seeControl);
                    ResetThreatSight(target); source = Activate(caster, main, resource, aura, observer);
                    command = new UnitUseAbility(new AbilityData(actor.Descriptor.Abilities.GetAbility(spell)), new TargetWrapper(actor));
                    StartThreat(actor, command, controller); Tick(Area(source));
                    Check(assertions, rows, "native-invisible-spell-" + canSee,
                        command.IsStarted && actor.Descriptor.State.HasCondition(UnitCondition.Invisible) &&
                        target.Descriptor.IsSeeInvisibility == canSee && target.Descriptor.HasFact(fascinated) != canSee,
                        "sharedVisible=true;invisible=true;ownSeeInvisibility=" + canSee +
                        ";fascinated=" + target.Descriptor.HasFact(fascinated));
                    StopThreat(actor, command); command = null; End(source); source = null;
                    target.Descriptor.RemoveFact(seeControl);
                }
            } finally {
                if (command != null) StopThreat(actor, command);
                if (source != null) End(source);
                actor.Descriptor.RemoveFact(invisibleControl); target.Descriptor.RemoveFact(seeControl);
                target.Memory.Add(actor).Visible = visible;
                UnityEngine.Object.Destroy(invisibleControl); UnityEngine.Object.Destroy(seeControl);
            }
        }

        private static void ResetThreatSight(UnitEntityData unit)
        {
            var clear = typeof(UnitSightCache).GetMethod("Clear", BindingFlags.Instance | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            if (clear == null || unit == null || unit.SightCache == null)
                throw new InvalidOperationException("The exact owned native sight-cache reset is unavailable.");
            clear.Invoke(unit.SightCache, null);
        }

        private static void StartThreat(UnitEntityData actor, UnitCommand command, UnitActionController controller)
        {
            actor.CombatState.OnNewRound();
            // These owned actors supply threat cues, not expenditure evidence.
            // OnNewRound resets hit/attack counters only. Isolate their RTWP
            // cooldowns so a prior harmless spell does not block the next cue.
            actor.CombatState.Cooldown.StandardAction = 0;
            actor.CombatState.Cooldown.MoveAction = 0;
            actor.CombatState.Cooldown.SwiftAction = 0;
            actor.Commands.Run(command);
            for (int tick = 0; !command.IsStarted && !command.IsFinished && tick < 12; tick++)
            {
                bool paused = Game.Instance.IsPaused;
                try { Game.Instance.IsPaused = false; Game.Instance.HandsEquipmentController.Tick(); }
                finally { Game.Instance.IsPaused = paused; }
                // Complete only the disposable hand animation. The native
                // controller still decides readiness and starts the command.
                if (actor.AreHandsBusyWithAnimation)
                    actor.View.HandsEquipment.GetType().GetMethod("InterruptAnimation",
                        BindingFlags.Instance | BindingFlags.NonPublic).Invoke(actor.View.HandsEquipment, null);
                ElementalBreathScenario.TickCommand(controller, command);
            }
            if (!command.IsStarted)
                throw new InvalidOperationException("Native threat command did not start: " + command.GetType().Name +
                    ";finished=" + command.IsFinished + ";close=" + command.IsUnitEnoughClose +
                    ";canStart=" + command.CanStart + ";handsBusy=" + actor.AreHandsBusyWithAnimation +
                    ";canAct=" + actor.Descriptor.State.CanAct + ";combatCanAct=" + actor.CombatState.CanActInCombat +
                    ";handUpdate=" + Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(actor) +
                    ";cooldown=" + actor.CombatState.HasCooldownForCommand(command));
        }
        private static void StopThreat(UnitEntityData actor, UnitCommand command)
        {
            var use = command as UnitUseAbility;
            if (use != null && use.ExecutionProcess != null && !use.ExecutionProcess.IsEnded)
                use.ExecutionProcess.Detach();
            actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
        }

        private static void ApproachAndDrawing(UnitEntityData caster, UnitEntityData approaching,
            UnitEntityData target, BlueprintAbility main, BlueprintAbilityResource resource, BlueprintBuff aura,
            BlueprintBuff fascinated, Saves observer, JArray rows, ICollection<RuntimeTestAssertion> assertions,
            BlueprintFeature blindControl)
        {
            var nativeSense = ScriptableObject.CreateInstance<Kingmaker.Designers.Mechanics.Facts.Blindsense>();
            nativeSense.Range = 30.Feet(); nativeSense.Blindsight = false;
            var senseControl = Control("ApproachBlindsense", nativeSense);
            var original = approaching.Position;
            var targetOriginal = target.Position;
            var agent = approaching.View.AgentASP;
            Kingmaker.UnitLogic.Buffs.Buff source = null;
            using (var movement = new NativeNereidMovement(rows, assertions))
            try
            {
                PlaceMovementFixture(approaching, new Vector3(0, 0, 6));
                target.Memory.Add(approaching).Visible = true;
                target.Stats.SaveWill.BaseValue = -100;
                source = Activate(caster, main, resource, aura, observer);
                var area = Area(source);
                int initial = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                var canceled = new UnitMoveTo(new Vector3(0, 0, 3));
                approaching.Commands.Run(canceled); Tick(area);
                approaching.Commands.InterruptAll(true); approaching.Commands.RemoveFinishedAndUpdateQueue();
                Check(assertions, rows, "approach-queued-cancel-no-save",
                    !canceled.IsStarted && target.Descriptor.HasFact(fascinated) &&
                    observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == initial,
                    "queueing and canceling a move without displacement grants no additional save");
                End(source); source = null;

                foreach (bool pass in new[] { false, true })
                {
                    PlaceMovementFixture(approaching, new Vector3(0, 0, 6));
                    target.Stats.SaveWill.BaseValue = -100;
                    source = Activate(caster, main, resource, aura, observer);
                    area = Area(source);
                    initial = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                    target.Stats.SaveWill.BaseValue = pass ? 100 : -100;
                    UnityEngine.Random.InitState(7419);
                    var command = new UnitMoveTo(new Vector3(0, 0, 3));
                    approaching.Commands.Run(command);
                    var start = approaching.Position;
                    agent.ForcePath(new Pathfinding.ForcedPath(new List<Vector3> {
                        start, new Vector3(0, 0, 3) }), .05f);
                    for (int tick = 0; Vector3.Distance(start, approaching.Position) < .04f && tick < 40; tick++) {
                        movement.Tick(); Tick(area);
                    }
                    int first = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                    bool moved = Vector3.Distance(start, approaching.Position) >= .04f;
                    Check(assertions, rows, "actual-native-approach-save-" + pass,
                        moved && !command.IsStarted && first == initial + 1 &&
                        target.Descriptor.HasFact(fascinated) != pass,
                        "start=" + start + ";position=" + approaching.Position +
                            ";displacement=" + Vector3.Distance(start, approaching.Position) + ";commandStarted=" + command.IsStarted + ";savesBefore=" + initial +
                            ";savesAfter=" + first + ";fascinated=" + target.Descriptor.HasFact(fascinated));
                    for (int tick = 0; tick < 3; tick++) {
                        movement.Tick(); Tick(area);
                    }
                    Check(assertions, rows, "approach-no-per-tick-save-" + pass,
                        observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == first,
                        "continued native path movement does not roll again for the same command");
                    if (!pass)
                    {
                        PlaceMovementFixture(target, new Vector3(0, 0, 12)); Tick(area);
                        PlaceMovementFixture(target, targetOriginal); Tick(area);
                        int returned = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                        movement.Tick(); Tick(area);
                        Check(assertions, rows, "approach-memory-survives-reentry",
                            target.Descriptor.HasFact(fascinated) && returned == first &&
                            observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == first,
                            "recreated target condition retains the activation's already-observed approach command");
                    }
                    else
                        Check(assertions, rows, "approach-success-terminal",
                            source.SelectComponents<ElementalNereidAuraState>().Single().Response(target) ==
                                ElementalNereidResponse.Interrupted && !target.Descriptor.HasFact(fascinated),
                            "successful potential-threat save ends this activation's effect permanently for the subject");
                    agent.Stop(); approaching.Commands.InterruptAll(true); approaching.Commands.RemoveFinishedAndUpdateQueue();
                    End(source); source = null;
                }

                foreach (int mode in new[] { 0, 1, 2 })
                {
                    bool visible = mode == 0;
                    if (mode == 2) { target.Descriptor.AddFact(blindControl); target.Descriptor.AddFact(senseControl); }
                    PlaceMovementFixture(approaching, new Vector3(0, 0, 6));
                    target.Memory.Add(approaching).Visible = visible;
                    target.Stats.SaveWill.BaseValue = -100;
                    source = Activate(caster, main, resource, aura, observer); area = Area(source);
                    initial = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                    var destination = new Vector3(0, 0, visible ? 8 : 3);
                    var command = new UnitMoveTo(destination); approaching.Commands.Run(command);
                    var start = approaching.Position;
                    agent.ForcePath(new Pathfinding.ForcedPath(new List<Vector3> { start, destination }), .05f);
                    for (int tick = 0; Vector3.Distance(start, approaching.Position) < .04f && tick < 40; tick++) {
                        movement.Tick(); Tick(area);
                    }
                    Check(assertions, rows, mode == 2 ? "blind-blindsense-approach-save" : visible ? "movement-away-no-save" : "unseen-approach-no-save",
                        Vector3.Distance(start, approaching.Position) >= .04f && target.Descriptor.HasFact(fascinated) &&
                        observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == initial + (mode == 2 ? 1 : 0),
                        "native displacement=" + Vector3.Distance(start, approaching.Position) +
                            ";visible=" + visible + ";extraSaves=" +
                            (observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) - initial));
                    agent.Stop(); approaching.Commands.InterruptAll(true); approaching.Commands.RemoveFinishedAndUpdateQueue();
                    End(source); source = null;
                    target.Descriptor.RemoveFact(senseControl); target.Descriptor.RemoveFact(blindControl);
                }
                target.Memory.Add(approaching).Visible = true;
                WeaponDrawing(caster, approaching, target, main, resource, aura, fascinated, observer, rows, assertions);
            }
            finally
            {
                agent.Stop(); approaching.Commands.InterruptAll(true); approaching.Commands.RemoveFinishedAndUpdateQueue();
                if (source != null) End(source);
                target.Descriptor.RemoveFact(senseControl); target.Descriptor.RemoveFact(blindControl);
                UnityEngine.Object.Destroy(senseControl);
                PlaceMovementFixture(approaching, original); PlaceMovementFixture(target, targetOriginal);
                target.Stats.SaveWill.BaseValue = -100;
                target.Memory.Add(approaching).Visible = true;
            }
        }

        private static void PlaceMovementFixture(UnitEntityData unit, Vector3 position)
        {
            // Initial placement only. All measured displacement is produced by
            // the native movement controller after the forced path is accepted.
            unit.Position = position;
            unit.View.transform.position = position;
        }

        // A scanned, request-owned navigation graph lets the native controller
        // perform movement and copy its transform to authoritative unit state.
        // This does not classify scene materials or qualify terrain/path search.
        internal sealed class NativeNereidMovement : IDisposable
        {
            private readonly JArray _rows;
            private readonly ICollection<RuntimeTestAssertion> _assertions;
            private readonly UnitEntityData[] _awake;
            private readonly UnitMoveController _controller = new UnitMoveController();
            private GameObject _root;
            private AstarPath _path;
            private Mesh _navigationPlane;
            internal NativeNereidMovement(JArray rows, ICollection<RuntimeTestAssertion> assertions,
                Vector3[] extraVertices = null, int[] extraTriangles = null, bool physicalGround = false)
            {
                _rows = rows; _assertions = assertions;
                if ((extraVertices == null) != (extraTriangles == null) ||
                    (extraTriangles != null && (extraTriangles.Length % 3 != 0 ||
                        extraTriangles.Any(value => value < 0 || value >= extraVertices.Length))))
                    throw new ArgumentException("Invalid request-owned navigation geometry.");
                _awake = Game.Instance.State.AwakeUnits.ToArray();
                if (AstarPath.active != null || Game.Instance.TurnBasedCombatController == null ||
                    Game.Instance.CurrentScene == null || Game.Instance.CurrentScene.Area == null ||
                    Game.Instance.CurrentScene.Area.InteractiveObjectGrid == null)
                    throw new InvalidOperationException("Native Nereid movement requires the empty graph and complete request-local area.");
                try
                {
                    _root = new GameObject("KMG_Runtime_Nereid_Navigation");
                    _root.SetActive(false);
                    _path = _root.AddComponent<AstarPath>();
                    _path.threadCount = ThreadCount.None;
                    _path.astarData = new Pathfinding.AstarData();
                    _root.SetActive(true);
                    if (!ReferenceEquals(AstarPath.active, _path))
                        throw new InvalidOperationException("The disposable native graph did not become active.");
                    // The native movement controller requires a navigation graph.
                    // Use its smallest exact request-owned native plane;
                    // no renderer, terrain, game asset or scene mesh is changed.
                    _navigationPlane = new Mesh { name = "KMG_Runtime_Nereid_NavigationPlane" };
                    _navigationPlane.vertices = new[] { new Vector3(-16, 0, -16), new Vector3(-16, 0, 16),
                        new Vector3(16, 0, 16), new Vector3(16, 0, -16) }
                        .Concat(extraVertices ?? Array.Empty<Vector3>()).ToArray();
                    _navigationPlane.triangles = new[] { 0, 1, 2, 0, 2, 3 }
                        .Concat((extraTriangles ?? Array.Empty<int>()).Select(value => value + 4)).ToArray();
                    _navigationPlane.RecalculateBounds();
                    var navigation = (Pathfinding.NavMeshGraph)_path.astarData.AddGraph(typeof(Pathfinding.NavMeshGraph));
                    navigation.sourceMesh = _navigationPlane;
                    navigation.scale = 1;
                    _path.Scan();
                    if (physicalGround)
                    {
                        // The game already loads its physics module. Keep this
                        // request-only fixture reflection-bound instead of adding
                        // a new mod assembly reference or replacing native queries.
                        var physicsAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(value =>
                            value.GetName().Name == "UnityEngine.PhysicsModule");
                        var colliderType = physicsAssembly.GetType("UnityEngine.MeshCollider", true);
                        var physicsType = physicsAssembly.GetType("UnityEngine.Physics", true);
                        var hitType = physicsAssembly.GetType("UnityEngine.RaycastHit", true);
                        var groundCollider = _root.AddComponent(colliderType);
                        colliderType.GetProperty("sharedMesh").SetValue(groundCollider, _navigationPlane, null);
                        physicsType.GetMethod("SyncTransforms", Type.EmptyTypes).Invoke(null, null);
                        var raycast = physicsType.GetMethod("Raycast", new[] { typeof(Vector3), typeof(Vector3),
                            hitType.MakeByRefType(), typeof(float), typeof(int) });
                        var probe = new object[] { Vector3.up * .25f, Vector3.down,
                            Activator.CreateInstance(hitType), .5f, 0x200101 };
                        bool hit = (bool)raycast.Invoke(null, probe);
                        var point = (Vector3)hitType.GetProperty("point").GetValue(probe[2], null);
                        Check(assertions, rows, "native-movement-physical-ground", hit &&
                            ReferenceEquals(hitType.GetProperty("collider").GetValue(probe[2], null), groundCollider) &&
                            Math.Abs(point.y) < .001f,
                            "native physics collider and navigation use the same request-owned plane at y=0");
                    }
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(Game.Instance.State.Units.All.Where(value => value.IsInGame));
                    foreach (var unit in Game.Instance.State.AwakeUnits)
                        PlaceMovementFixture(unit, physicalGround
                            ? Kingmaker.View.UnitMovementAgentBase.Move(unit.Position, Vector3.zero, unit.FlyHeight)
                            : unit.Position);
                    var samples = new[] { Vector3.zero, new Vector3(0, 0, 6), new Vector3(0, 0, 8) };
                    var nearest = samples.Select(point => navigation.GetNearestForce(point,
                        new Pathfinding.NNConstraint { constrainWalkability = true, walkable = true })).ToArray();
                    Check(assertions, rows, "native-movement-navigation",
                        _path.graphs.Length == 1 && ReferenceEquals(_path.graphs[0], navigation) &&
                        navigation.nodes != null && navigation.nodes.Length == 2 + (extraTriangles == null ? 0 : extraTriangles.Length / 3) &&
                        navigation.nodes.All(value => value != null && value.Walkable) &&
                        nearest.Select((value, index) => value.node != null &&
                            Vector3.Distance(value.clampedPosition, samples[index]) < .01f).All(value => value),
                        "scanned disposable native graph;triangles=" + (navigation.nodes == null ? 0 : navigation.nodes.Length) +
                            ";walkable=" + (navigation.nodes == null ? 0 : navigation.nodes.Count(value => value != null && value.Walkable)) +
                            ";nearest=" + string.Join("|", nearest.Select(value => value.clampedPosition.ToString())) +
                            ";native awake units registered; no authored terrain was changed");
                }
                catch { Dispose(); throw; }
            }
            internal void Tick()
            {
                float delta = Game.Instance.TimeController.DeltaTime, gameDelta = Game.Instance.TimeController.GameDeltaTime;
                bool pause = Game.Instance.IsPaused;
                try
                {
                    Game.Instance.IsPaused = false;
                    Game.Instance.TimeController.SetDeltaTime(.05f);
                    Game.Instance.TimeController.SetGameDeltaTime(.05f);
                    _controller.Tick();
                }
                finally
                {
                    Game.Instance.TimeController.SetDeltaTime(delta);
                    Game.Instance.TimeController.SetGameDeltaTime(gameDelta);
                    Game.Instance.IsPaused = pause;
                }
            }
            public void Dispose()
            {
                Game.Instance.State.AwakeUnits.Clear();
                Game.Instance.State.AwakeUnits.AddRange(_awake);
                if (_root != null) { UnityEngine.Object.DestroyImmediate(_root); _root = null; }
                if (_navigationPlane != null) { UnityEngine.Object.DestroyImmediate(_navigationPlane); _navigationPlane = null; }
                Check(_assertions, _rows, "native-movement-navigation-restored",
                    AstarPath.active == null && Game.Instance.State.AwakeUnits.SequenceEqual(_awake),
                    "temporary native graph destroyed and exact awake-unit list restored");
            }
        }

        private static void WeaponDrawing(UnitEntityData caster, UnitEntityData actor, UnitEntityData target,
            BlueprintAbility main, BlueprintAbilityResource resource, BlueprintBuff aura, BlueprintBuff fascinated,
            Saves observer, JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            if (actor.Descriptor.Body.PrimaryHand.HasItem)
                throw new InvalidOperationException("Native drawing fixture requires an empty hand.");
            var weapon = new Kingmaker.Items.ItemEntityWeapon(
                Exact<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("57c8994d1f1becf49ac4f642e5d8ca9d"));
            bool combatHands = actor.View.HandsEquipment.InCombat;
            Kingmaker.UnitLogic.Buffs.Buff source = null;
            try
            {
                actor.Descriptor.Body.PrimaryHand.InsertItem(weapon);
                actor.View.HandsEquipment.GetType().GetMethod("UpdateActiveWeaponSetImmediately",
                    BindingFlags.Instance | BindingFlags.NonPublic).Invoke(actor.View.HandsEquipment, null);
                foreach (bool visible in new[] { false, true })
                {
                    actor.View.HandsEquipment.ForceSwitch(false);
                    target.Memory.Add(actor).Visible = visible;
                    source = Activate(caster, main, resource, aura, observer);
                    var area = Area(source); Tick(area);
                    int initial = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                    bool sheathed = target.Descriptor.HasFact(fascinated);
                    actor.View.HandsEquipment.ForceSwitch(true); Tick(area); Tick(area);
                    Check(assertions, rows, "native-weapon-draw-" + visible,
                        sheathed && target.Descriptor.HasFact(fascinated) != visible &&
                        observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == initial,
                        "native held-weapon state transition;visible=" + visible +
                            ";sheathedPreserved=" + sheathed + ";fascinated=" + target.Descriptor.HasFact(fascinated));
                    End(source); source = null;
                }
                RuleOnlyThreats(caster, actor, target, weapon, main, resource, aura, fascinated, observer, rows, assertions);
            }
            finally
            {
                if (source != null) End(source);
                if (ReferenceEquals(actor.Descriptor.Body.PrimaryHand.MaybeItem, weapon))
                    actor.Descriptor.Body.PrimaryHand.RemoveItem(false);
                weapon.Dispose();
                actor.View.HandsEquipment.ForceSwitch(combatHands);
                target.Memory.Add(actor).Visible = true;
            }
        }

        private sealed class MissedThreatRoll : IGlobalRulebookHandler<RuleAttackRoll>
        {
            internal UnitEntityData Target;
            internal BlueprintBuff Fascinated;
            internal int Rolls;
            internal bool FascinatedAtRoll;
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (!ReferenceEquals(evt.Target, Target)) return;
                evt.AutoMiss = true;
                Rolls++;
                FascinatedAtRoll = Target.Descriptor.HasFact(Fascinated);
            }
            public void OnEventDidTrigger(RuleAttackRoll evt) { }
        }

        private static void RuleOnlyThreats(UnitEntityData caster, UnitEntityData actor, UnitEntityData target,
            Kingmaker.Items.ItemEntityWeapon weapon, BlueprintAbility main, BlueprintAbilityResource resource,
            BlueprintBuff aura, BlueprintBuff fascinated, Saves observer, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            var roll = new MissedThreatRoll { Target = target, Fascinated = fascinated };
            Kingmaker.UnitLogic.Buffs.Buff source = null;
            EventBus.Subscribe(roll);
            try
            {
                foreach (bool opportunity in new[] { false, true })
                    foreach (bool visible in new[] { false, true })
                    {
                        target.Memory.Add(actor).Visible = visible;
                        roll.Rolls = 0;
                        source = Activate(caster, main, resource, aura, observer);
                        int damage = target.Damage;
                        int saves = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                        var attack = Rulebook.Trigger(new RuleAttackWithWeapon(actor, target, weapon, 0) {
                            IsAttackOfOpportunity = opportunity });
                        Tick(Area(source)); Tick(Area(source));
                        Check(assertions, rows, "native-rule-only-attack-" + opportunity + "-" + visible,
                            roll.Rolls == 1 && attack.AttackRoll != null && !attack.AttackRoll.IsHit &&
                            target.Damage == damage && roll.FascinatedAtRoll != visible &&
                            target.Descriptor.HasFact(fascinated) != visible &&
                            observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == saves,
                            "opportunity=" + opportunity + ";visible=" + visible + ";rolls=" + roll.Rolls +
                                ";fascinatedAtRoll=" + roll.FascinatedAtRoll + ";damageUnchanged=" + (target.Damage == damage) +
                                ";no re-fascination or per-tick save after the missed native attack");
                        End(source); source = null;
                    }
            }
            finally
            {
                if (source != null) End(source);
                target.Memory.Add(actor).Visible = true;
                EventBus.Unsubscribe(roll);
            }
        }

        private static void Obstruction(UnitEntityData caster, UnitEntityData target, BlueprintAbility main,
            BlueprintAbilityResource resource, BlueprintBuff aura, BlueprintBuff fascinated, Saves observer,
            JArray rows, ICollection<RuntimeTestAssertion> assertions, UnitEntityData threatActor,
            BlueprintAbility threatAbility, UnitActionController controller, BlueprintFeature blindControl)
        {
            var geometry = LineOfSightGeometry.Instance;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var cells = typeof(LineOfSightGeometry).GetField("m_Cells", flags);
            var bounds = typeof(LineOfSightGeometry).GetField("m_Bounds", flags);
            var updated = typeof(LineOfSightGeometry).GetProperty("LastUpdateTime", flags);
            var segments = typeof(LineOfSightGeometry).GetField("s_CellsForSegment",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (cells == null || bounds == null || updated == null || segments == null ||
                FogOfWarBlocker.All.Count != 0)
                throw new InvalidOperationException("Nereid obstruction requires the empty disposable geometry boundary.");
            object oldCells = cells.GetValue(geometry), oldBounds = bounds.GetValue(geometry);
            object oldUpdated = updated.GetValue(geometry, null);
            var work = (IList)segments.GetValue(null);
            var oldWork = work == null ? Array.Empty<object>() : work.Cast<object>().ToArray();
            var originalPosition = target.Position;
            var threatPosition = threatActor.Position;
            var nativeSense = ScriptableObject.CreateInstance<Kingmaker.Designers.Mechanics.Facts.Blindsense>();
            nativeSense.Range = 30.Feet(); nativeSense.Blindsight = true;
            var senseControl = Control("ObstructedBlindsight", nativeSense);
            var nativeBounds = new Bounds(Vector3.zero, new Vector3(40, 10, 40));
            var obstacle = new GameObject("KMG_Runtime_Nereid_Obstruction");
            obstacle.SetActive(false);
            var blocker = obstacle.AddComponent<FogOfWarBlocker>();
            blocker.HeightMinMax = new Vector2(-5, 20);
            var clock = Game.Instance.Player.GameTime;
            Kingmaker.UnitLogic.Buffs.Buff source = null;
            try
            {
                target.Position = new Vector3(0, 0, 2);
                geometry.Init(nativeBounds);
                geometry.AddPointsList(new[] { new Vector2(-5, .5f), new Vector2(5, .5f) }, false, blocker);
                source = Activate(caster, main, resource, aura, observer);
                var area = Area(source);
                int initial = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                Check(assertions, rows, "native-line-of-effect-blocked",
                    geometry.HasObstacle(caster.Position, target.Position, 0) &&
                    !area.UnitsInside.Contains(target) && !target.Descriptor.HasFact(fascinated) && initial == 0,
                    "native high wall geometry blocks area membership and no initial saving throw occurs");
                geometry.Init(nativeBounds); Tick(area);
                int first = observer.Events.Count(value => ReferenceEquals(value.Initiator, target));
                var deadline = source.EndTime;
                Check(assertions, rows, "native-line-of-effect-entry",
                    !geometry.HasObstacle(caster.Position, target.Position, 0) &&
                    area.UnitsInside.Contains(target) && target.Descriptor.HasFact(fascinated) && first == 1,
                    "removing the native obstruction admits the target and triggers one initial save");
                geometry.AddPointsList(new[] { new Vector2(-5, .5f), new Vector2(5, .5f) }, false, blocker);
                Tick(area);
                bool exited = !target.Descriptor.HasFact(fascinated);
                Game.Instance.Player.GameTime += TimeSpan.FromSeconds(1);
                geometry.Init(nativeBounds); Tick(area);
                var effect = target.Buffs.GetBuff(fascinated);
                Check(assertions, rows, "native-line-of-effect-reentry",
                    exited && effect != null && effect.EndTime == deadline &&
                    observer.Events.Count(value => ReferenceEquals(value.Initiator, target)) == first,
                    "new obstruction removes the owned condition; re-entry preserves its original deadline without another initial save");
                End(source); source = null;
                PlaceMovementFixture(threatActor, new Vector3(4, 0, 2));
                threatActor.Descriptor.AddFact(threatAbility);
                target.Memory.Add(threatActor).Visible = true;
                foreach (bool blind in new[] { false, true })
                {
                    if (blind) { target.Descriptor.AddFact(blindControl); target.Descriptor.AddFact(senseControl); }
                    geometry.Init(nativeBounds); source = Activate(caster, main, resource, aura, observer);
                    foreach (bool blocked in new[] { true, false })
                    {
                        geometry.Init(nativeBounds);
                        if (blocked) geometry.AddPointsList(new[] { new Vector2(2, -5), new Vector2(2, 5) }, false, blocker);
                        ResetThreatSight(target);
                        bool sight = target.HasLOS(threatActor);
                        var use = new UnitUseAbility(new AbilityData(threatActor.Descriptor.Abilities.GetAbility(threatAbility)), new TargetWrapper(threatActor));
                        try {
                            StartThreat(threatActor, use, controller); Tick(Area(source));
                            Check(assertions, rows, "native-owned-sight-" + blind + "-" + blocked,
                                use.IsStarted && sight != blocked && target.Descriptor.HasFact(fascinated) == blocked,
                                "sharedVisible=true;blindWithBlindsight=" + blind + ";blocked=" + blocked +
                                ";nativeLOS=" + sight + ";fascinated=" + target.Descriptor.HasFact(fascinated));
                        } finally { StopThreat(threatActor, use); }
                    }
                    End(source); source = null;
                    target.Descriptor.RemoveFact(senseControl); target.Descriptor.RemoveFact(blindControl);
                }
            }
            finally
            {
                if (source != null) End(source);
                target.Position = originalPosition;
                PlaceMovementFixture(threatActor, threatPosition);
                threatActor.Descriptor.RemoveFact(threatAbility);
                target.Descriptor.RemoveFact(senseControl); target.Descriptor.RemoveFact(blindControl);
                ResetThreatSight(target); UnityEngine.Object.Destroy(senseControl);
                Game.Instance.Player.GameTime = clock;
                UnityEngine.Object.DestroyImmediate(obstacle);
                cells.SetValue(geometry, oldCells); bounds.SetValue(geometry, oldBounds);
                updated.GetSetMethod(true).Invoke(geometry, new[] { oldUpdated });
                if (work == null) segments.SetValue(null, null);
                else { work.Clear(); foreach (var entry in oldWork) work.Add(entry); segments.SetValue(null, work); }
                Check(assertions, rows, "native-geometry-restored",
                    ReferenceEquals(cells.GetValue(geometry), oldCells) && bounds.GetValue(geometry).Equals(oldBounds) &&
                    updated.GetValue(geometry, null).Equals(oldUpdated) &&
                    ReferenceEquals(segments.GetValue(null), work) &&
                    (work == null || work.Cast<object>().SequenceEqual(oldWork)) && FogOfWarBlocker.All.Count == 0,
                    "exact original geometry arrays, bounds, timestamp and work list restored; no terrain or navmesh was changed");
            }
        }
    }
}
