using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewInfernalAi34(ExpandedSummoningCorrectionFixture fixture)
        {
            var cells = new JArray(); var trace = new InfernalEvents34();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-ai", ["cells"] = cells, ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint34-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint34 AI: " + id);
            };
            try {
                foreach (bool turnBased in new[] { false, true }) {
                    foreach (string key in new[] { InfernalVisualPolicy34.Barbazu, InfernalVisualPolicy34.Kyton })
                        foreach (int frame in InfernalAiCase34(fixture, key, turnBased, false, trace, check)) yield return frame;
                }
                foreach (int frame in InfernalAiCase34(fixture, InfernalVisualPolicy34.Barbazu, false, true, trace, check)) yield return frame;
            } finally {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-ai.json"), receipt.ToString(Formatting.Indented));
            }
        }

        // Reuse the existing native turn controller. Only idle request-owned
        // witnesses can pass; the tested owner always takes its actual turn.
        private void PassInfernalWitness34(UnitEntityData owner, UnitEntityData[] idle, JArray rows)
        {
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null || ReferenceEquals(turn.Unit, owner)) return;
            if (!idle.Contains(turn.Unit) || turn.Unit.IsBrainActive || !turn.Unit.Commands.Empty || turn.Unit.Commands.Queue.Count != 0)
                throw new InvalidOperationException("Unknown or active actor in infernal native turn order.");
            bool acting = turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Preparing ||
                turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Acting;
            if (acting && turn.CanEndTurnAndNoActing()) {
                rows.Add(new JObject { ["owner"] = turn.Unit.UniqueId, ["round"] = Game.Instance.TurnBasedCombatController.RoundNumber });
                EndSprint17SnakeTurn(turn);
            }
        }

        private IEnumerable<int> InfernalAiCase34(ExpandedSummoningCorrectionFixture fixture, string key, bool turnBased,
            bool disarm, InfernalEvents34 trace, Action<string, bool, JObject, string> check)
        {
            string id = "ai-" + key + (turnBased ? "-turn" : "-rtwp") + (disarm ? "-disarmed" : "");
            UnitEntityData owner = null; var target = fixture.Hostile; Buff[] capturedEffects = Array.Empty<Buff>();
            var party = Game.Instance.Player.Inventory.Items.ToArray(); var partyCounts = party.Select(item => item.Count).ToArray();
            var anchor = Sprint32OwnedQuietAnchor(fixture, id);
            var idle = new[] { fixture.Caster, target, anchor }.Distinct().ToArray();
            try {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                owner = CastExpandedSummoningOwnTier(fixture, key);
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(command => true);
                foreach (int frame in Outsider30NativeAppearance(owner, id)) yield return frame;
                if (disarm) foreach (int frame in InfernalDisarm34(fixture, owner, check)) yield return frame;
                var profile = InfernalMartialProfile34.For(owner); bool barbazu = profile.Glaive != null;
                var items = owner.Descriptor.Inventory.Items.ToArray(); var counts = items.Select(item => item.Count).ToArray();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 6f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, origin + (far-origin).normalized *
                    (owner.Corpulence + target.Corpulence + new Feet(barbazu ? (disarm ? 8 : 3) : 20).Meters));
                foreach (var actor in new[] { owner, target }) {
                    actor.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                    actor.SightCache.ClearIfNeeded();
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var admission = new JObject(); var passedTurns = new JArray();
                if (turnBased) {
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < deadline && !Sprint18OwnerIsActing(owner)) {
                        PassInfernalWitness34(owner, idle, passedTurns); Game.Instance.IsPaused = false; yield return 0;
                    }
                }
                bool ready = owner.Descriptor.State.CanAct && owner.IsEnemy(target) && owner.HasLOS(target) &&
                    owner.Memory.ContainsVisible(target) && (!turnBased || (bool)admission["admitted"] && Sprint18OwnerIsActing(owner));
                check(id + "-ready", ready, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                    ["lane"] = lane, ["nativeTurn"] = admission, ["visible"] = owner.Memory.ContainsVisible(target),
                    ["distance"] = owner.DistanceTo(target), ["initialWeapon"] = owner.Body.PrimaryHand.MaybeWeapon?.Blueprint.AssetGuid,
                    ["disarmed"] = owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null },
                    "Fresh exact owner and visible private enemy enter native combat; no attack, save, AC or ability score is forced.");
                var commands = new HashSet<UnitCommand>(); var starts = new Lillend32CommandStarts { Owner = owner, ObserveRtwpAttackCompletion = true };
                var costs = new JArray(); var ambient = new HashSet<string>(); int first = trace.Rows.Count;
                EventBus.Subscribe(starts);
                try {
                    using (var economy = new FirearmReviewCosts(owner, costs)) {
                        Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                        UnitAttack attack = null; UnitSwitchHandEquipmentSet change = null; UnitUseAbility control = null, gaze = null;
                        int frames = 0, ticks = 0, nativeFrames = 0;
                        DateTime deadline = DateTime.UtcNow.AddSeconds(70);
                        while (DateTime.UtcNow < deadline && frames++ < 8000) {
                            if (!turnBased && (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))) {
                                foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate);
                                ticks++;
                            }
                            if (turnBased) {
                                if (Sprint18OwnerIsActing(owner)) nativeFrames++;
                                else PassInfernalWitness34(owner, idle, passedTurns);
                            }
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            foreach (var command in starts.Commands) commands.Add(command);
                            attack = commands.OfType<UnitAttack>().FirstOrDefault(value => value.AiAction != null && starts.CompletedAttack(value, target));
                            change = commands.OfType<UnitSwitchHandEquipmentSet>().FirstOrDefault(value => value.AiAction is BarbazuWeaponModeAi34 &&
                                value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                            control = commands.OfType<UnitUseAbility>().FirstOrDefault(value => value.AiAction is KytonControlAi34 &&
                                value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                            gaze = commands.OfType<UnitUseAbility>().FirstOrDefault(value => value.AiAction is KytonGazeAi34 &&
                                value.IsFinished && value.Result == UnitCommand.ResultType.Success && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                            if (attack != null && (barbazu ? change != null : control != null && gaze != null)) break;
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        var events = trace.Rows.Skip(first).ToArray();
                        var strikes = events.Where(row => (string)row["event"] == "attack" && (string)row["owner"] == owner.UniqueId).ToArray();
                        var saves = events.Where(row => (string)row["event"] == "save" && (string)row["target"] == target.UniqueId && (int?)row["dc"] == 15).ToArray();
                        var resolved = new UnitCommand[] { attack, change, control, gaze }.Where(command => command != null).ToArray();
                        bool exact = attack != null && strikes.Length > 0 && resolved.All(command => !command.Cutscene && !command.IsIgnoreCooldown &&
                            economy.Count(command) == 1 && economy.Exact(command, turnBased)) &&
                            (turnBased ? nativeFrames > 0 && ticks == 0 : ticks > 0) &&
                            (barbazu ? change != null && strikes.All(row => (string)row["weapon"] == profile.Claw.AssetGuid) :
                                control != null && gaze != null && ReferenceEquals(control.TargetUnit, owner) && ReferenceEquals(gaze.TargetUnit, target) &&
                                profile.ControlsOwnChains(owner) && saves.Length > 0 && strikes.All(row => (string)row["weapon"] == profile.Chain.AssetGuid)) &&
                            Sprint28InventoryExact(owner.Descriptor.Inventory, items, counts) && items.All(item => !item.IsLootable && item.IsNonRemovable) &&
                            Sprint28PartyInventoryExact(party, partyCounts);
                        check(id + "-resolved", exact, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                            ["turnBased"] = turnBased, ["nativeTurnFrames"] = nativeFrames, ["aiTicks"] = ticks, ["frames"] = frames,
                            ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                            ["nativeAttackLifecycle"] = starts.AttackRows, ["events"] = new JArray(events),
                            ["attackCompleted"] = attack != null, ["switchCompleted"] = change != null,
                            ["controlCompleted"] = control != null, ["gazeCompleted"] = gaze != null,
                            ["resolvedCostsExact"] = resolved.All(command => economy.Count(command) == 1 && economy.Exact(command, turnBased)),
                            ["idleWitnessTurnsPassed"] = passedTurns, ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                            ["itemsExact"] = Sprint28InventoryExact(owner.Descriptor.Inventory, items, counts),
                            ["fixtureAttackCommands"] = 0, ["fixtureAbilityCommands"] = 0, ["forcedRolls"] = 0 },
                            barbazu ? "Native AI pays for its actual weapon-set switch and attacks with claws, including the disarmed fallback; private equipment remains owned." :
                                "Native AI controls its own chains, casts gaze only at the enemy with a real save, and makes normal chain attacks with separate native costs.");
                    }
                } finally { EventBus.Unsubscribe(starts); }
            } finally {
                if (owner != null) capturedEffects = Game.Instance.State.Units.All.Where(unit => unit != null).SelectMany(unit => unit.Buffs.Enumerable)
                    .Where(buff => ReferenceEquals(buff.Context?.MaybeCaster, owner) && buff.SelectComponents<InfernalEffectLifetime34>().Any()).ToArray();
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var actor in new[] { owner, target }.Where(value => value != null && !value.Destroyed)) {
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(command => true);
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }.Where(value => value != null));
                ResetExpandedSummoningHostile(fixture);
            }
            for (int frame = 0; frame < 12; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            check(id + "-reset", Sprints3031SourceAbsent(owner) && capturedEffects.All(buff => buff.IsDisposed) && !target.IsInCombat && !target.Commands.Raw.Any(command => command != null && !command.IsFinished) &&
                !target.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Context?.MaybeCaster, owner)) && Sprint28PartyInventoryExact(party, partyCounts),
                new JObject { ["sourceAbsent"] = Sprints3031SourceAbsent(owner), ["capturedEffectsDisposed"] = capturedEffects.All(buff => buff.IsDisposed), ["targetCombat"] = target.IsInCombat },
                "Owned commands, combat and source effects retire before the next grouped AI case; party inventory remains exact.");
        }

        private IEnumerable<int> InfernalDisarm34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            Action<string, bool, JObject, string> check)
        {
            var attacker = CastExpandedSummoningOwnTier(fixture, "storm-giant");
            try {
                Sprint28SetBrainActivePreservingActions(attacker, false); attacker.Commands.InterruptAll(command => true);
                foreach (int frame in WaitInfernalSupport34(attacker)) yield return frame;
                // Do not change a summoner's shared group while making the
                // disposable maneuver source hostile. Match the established
                // private-recipient ownership boundary before faction mutation.
                string casterGroup = fixture.Caster.GroupId, ownerGroup = owner.GroupId;
                attacker.GroupId = attacker.UniqueId;
                var privateGroup = attacker.Group;
                if (privateGroup.IsPlayerParty || privateGroup.Count != 1 || !ReferenceEquals(privateGroup[0], attacker))
                    throw new InvalidOperationException("Disarm source does not own a private group.");
                attacker.Descriptor.SwitchFactions(fixture.OwnedHostileFaction, true);
                attacker.AttackFactions.Match(new[] { fixture.Caster.Blueprint.Faction });
                privateGroup.ResetFactionSet(); privateGroup.UpdateAttackFactionsCache();
                if (fixture.Caster.GroupId != casterGroup || owner.GroupId != ownerGroup ||
                    !ReferenceEquals(attacker.Faction, fixture.OwnedHostileFaction))
                    throw new InvalidOperationException("Private maneuver faction changed another group.");
                var items = owner.Descriptor.Inventory.Items.ToArray(); var counts = items.Select(item => item.Count).ToArray();
                var sourceItems = attacker.Descriptor.Inventory.Items.ToArray(); var sourceCounts = sourceItems.Select(item => item.Count).ToArray();
                var party = Game.Instance.Player.Inventory.Items.ToArray(); var partyCounts = party.Select(item => item.Count).ToArray();
                if (!attacker.CanAttack(owner)) throw new InvalidOperationException("Private native Disarm source is ineligible.");
                var attempts = new JArray(); bool success = false;
                for (int attempt = 0; attempt < 4 && !success; attempt++) {
                    var rule = Rulebook.Trigger(new RuleCombatManeuver(attacker, owner, CombatManeuver.Disarm));
                    success = rule.Success;
                    attempts.Add(new JObject { ["d20"] = rule.InitiatorRoll.Value, ["cmb"] = rule.InitiatorCMB,
                        ["cmd"] = rule.TargetCMD, ["success"] = success, ["partial"] = rule.IsPartialSuccess });
                }
                check("disarm-owned-equipment", success && owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null &&
                    Sprint28InventoryExact(owner.Descriptor.Inventory, items, counts) && items.All(item => ReferenceEquals(item.Owner, owner.Descriptor) && !item.IsLootable && item.IsNonRemovable) &&
                    Sprint28InventoryExact(attacker.Descriptor.Inventory, sourceItems, sourceCounts) && Sprint28PartyInventoryExact(party, partyCounts),
                    new JObject { ["owner"] = owner.UniqueId, ["source"] = attacker.UniqueId, ["sourceBlueprint"] = attacker.Blueprint.AssetGuid,
                        ["attempts"] = attempts, ["directNativeManeuverConsumerNotCommandDispatch"] = true,
                        ["fixtureStatChanges"] = 0, ["forcedRolls"] = 0 },
                    "A real released Storm Giant's unmodified native maneuver disables the owned glaive without dropping, transferring, duplicating or exposing equipment; AI fallback is observed separately.");
            } finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { attacker }); }
        }
    }
}
