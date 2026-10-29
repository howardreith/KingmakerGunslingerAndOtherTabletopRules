using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewStalkerTurn33(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null; var target = fixture.Hostile;
            var starts = new Lillend32CommandStarts(); var attacks = new Sprint16RuleObserver { Target = target };
            var contacts = new JArray();
            try
            {
                ResetExpandedSummoningHostile(fixture);
                owner = CastExpandedSummoningOwnTier(fixture, ShadowOutsiderVisualPolicy.Stalker);
                starts.Owner = attacks.Owner = owner; Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                for (int frame = 0; frame < 30; frame++) yield return 0;
                if (owner.View.GetComponent<StalkerSlamAnimation33>()?.Live != true)
                    throw new InvalidOperationException("Stalker private Slam absent before native turn.");
                Vector3 end; string lane, direction;
                float separation = owner.Corpulence + target.Corpulence + owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters * .5f;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, separation, out end, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, end);
                var admission = new JObject();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < turnDeadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                    throw new InvalidOperationException("Stalker native turn admission failed.");
                attacks.ObserveWeaponContact = rule => {
                    var command = owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(value => !value.IsFinished && ReferenceEquals(value.Target, target));
                    contacts.Add(new JObject { ["hit"] = rule.AttackRoll?.IsHit == true, ["nativeTurn"] = Sprint18OwnerIsActing(owner),
                        ["animationStarted"] = command?.Animation?.IsStarted == true,
                        ["clip"] = command?.Animation?.ActiveAnimation?.GetPlayableClip()?.name });
                };
                EventBus.Subscribe(starts); EventBus.Subscribe(attacks);
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                    while (DateTime.UtcNow < deadline && !(attacks.Damage.Any(value => !value.IsFake && value.Damage > 0) &&
                        starts.Commands.OfType<UnitAttack>().Any(value => starts.CompletedAttack(value, target)))) { Game.Instance.IsPaused = false; yield return 0; }
                    var command = starts.Commands.OfType<UnitAttack>().FirstOrDefault(value => starts.CompletedAttack(value, target));
                    bool exact = command != null && !command.Cutscene && !command.IsIgnoreCooldown && economy.Count(command) == 1 && economy.Exact(command, true) &&
                        contacts.Any(value => (bool)value["hit"] && (bool)value["nativeTurn"] && (bool)value["animationStarted"]) &&
                        owner.Descriptor.State.HasCondition(UnitCondition.Invisible);
                    Sprint25Check("sprint33-stalker-native-turn-slam", exact, new JObject { ["admission"] = admission,
                        ["contacts"] = contacts, ["nativeAttackLifecycle"] = starts.AttackRows, ["costs"] = costs, ["lane"] = lane, ["forcedRolls"] = 0, ["manualAiTicks"] = 0 },
                        "The changed private Slam binding also serves an ordinary native turn-based AI attack, with actual action cost and attack-safe invisibility.");
                    if (!exact) throw new InvalidOperationException("Stalker native-turn Slam failed.");
                }
            }
            finally
            {
                EventBus.Unsubscribe(starts); EventBus.Unsubscribe(attacks);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var unit in new[] { owner, target }.Where(value => value != null && !value.Destroyed))
                { if (unit == owner) Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat(); }
                Game.Instance.Player.UpdateIsInCombat();
                if (owner != null && !owner.Destroyed) DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
        private sealed class Sprint33Drain : IGlobalRulebookHandler<RuleDrainEnergy>
        {
            internal UnitEntityData Owner;
            internal readonly JArray Rows = new JArray();
            public void OnEventAboutToTrigger(RuleDrainEnergy rule) { }
            public void OnEventDidTrigger(RuleDrainEnergy rule)
            {
                if (!ReferenceEquals(rule.Initiator, Owner)) return;
                var duration = (TimeSpan?)typeof(RuleDrainEnergy).GetField("m_Duration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rule);
                Rows.Add(new JObject { ["type"] = rule.Type.ToString(), ["count"] = rule.Count,
                    ["immune"] = rule.TargetIsImmune, ["durationSeconds"] = duration?.TotalSeconds });
            }
        }

        // Existing AI/command/cost/saving-throw observers, one normal humanoid,
        // one native protection and one ineligible creature type. No forced rolls.
        private IEnumerable<int> ReviewSuccubus33(ExpandedSummoningCorrectionFixture fixture)
        {
            var target = fixture.Hostile;
            var fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "48ac8db94d5de7645906c7d0ad3bcfbd", "disposable five-HD control witness");
            int levels = 5 - target.Descriptor.Progression.CharacterLevel;
            if (levels < 0) throw new InvalidOperationException("Control witness exceeds selected HD.");
            ElementalSpellAffinityScenario.Advance(target.Descriptor, fighter, levels);
            target.Stats.HitPoints.BaseValue = 100000;
            var ability = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.AssetGuid == "1662d63944d94cdeaa62562dc9ac9349");
            var terminal = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.AssetGuid == "6e1f6eb3e773451dbda9e0ecd07486d9");
            var outsider = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "9054d3988d491d944ac144e27b6bc318", "native outsider type eligibility witness");
            foreach (string cell in new[] { "ai-control", "protected", "type-fallback" })
            {
                UnitEntityData owner = null; Buff protection = null; bool typeAdded = false;
                var starts = new Lillend32CommandStarts { ObserveRtwpAttackCompletion = true }; var saves = new Dretch30CloudSaves(new[] { target });
                var attacks = new Sprint16RuleObserver { Target = target }; var drain = new Sprint33Drain();
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    owner = CastExpandedSummoningOwnTier(fixture, "succubus");
                    starts.Owner = attacks.Owner = drain.Owner = owner;
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    if (cell == "protected")
                    {
                        var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                            "4a6911969911ce9499bf27dde9bfcedc", "native Protection from Evil");
                        protection = target.Buffs.AddBuff(native, new MechanicsContext(fixture.Caster, target.Descriptor,
                            native, null, new TargetWrapper(target)), TimeSpan.FromMinutes(2));
                        if (protection == null || !_context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity)
                            throw new InvalidOperationException("Selected protection precondition absent.");
                    }
                    if (cell == "type-fallback")
                    {
                        if (target.Descriptor.HasFact(outsider)) throw new InvalidOperationException("Preexisting outsider witness.");
                        target.Descriptor.AddFact(outsider); typeAdded = true;
                    }
                    Vector3 end; string lane, direction;
                    var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out end, out lane, out direction);
                    PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, end);
                    foreach (var unit in new[] { owner, target })
                    { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                    owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                    owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                    new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                    var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(ability));
                    bool eligible = data.CanTarget(new TargetWrapper(target));
                    if (eligible == typeAdded) throw new InvalidOperationException("Native humanoid eligibility differs.");
                    EventBus.Subscribe(starts); EventBus.Subscribe(saves); EventBus.Subscribe(attacks); EventBus.Subscribe(drain);
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(40); int frames = 0, aiTicks = 0;
                        var eligibleActors = new HashSet<string>();
                        if (cell != "protected") { Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions(); }
                        while (DateTime.UtcNow < deadline && frames++ < 4000)
                        {
                            var seenApplications = saves.NativeApplications.Where(value => ReferenceEquals(value.Blueprint, terminal) &&
                                ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                            bool resolved = cell == "ai-control" ? seenApplications.Any(value => value.AppliedBuff != null) :
                                cell == "protected" ? seenApplications.Any(value => !value.CanApply && value.AppliedBuff == null) :
                                attacks.Damage.Any(value => !value.IsFake && value.Damage > 0) && starts.Commands.OfType<UnitAttack>().Any(value => starts.CompletedAttack(value, target));
                            if (resolved && owner.Commands.Raw.All(value => value == null || value.IsFinished)) break;
                            if (frames == 1 || frames % 60 == 0 && owner.Commands.Raw.All(value => value == null || value.IsFinished))
                            {
                                if (cell == "protected")
                                {
                                    if (starts.Commands.Count >= 8) break;
                                    var cast = new UnitUseAbility(data, new TargetWrapper(target)); cast.Init(owner);
                                    if (!cast.CanStart) throw new InvalidOperationException("Native protected cast unavailable.");
                                    owner.Commands.Run(cast);
                                }
                                else { foreach (string actor in Sprint24TickExactAi(owner)) eligibleActors.Add(actor); aiTicks++; }
                            }
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        var owned = target.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, terminal) && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                        var applications = saves.NativeApplications.Where(value => ReferenceEquals(value.Blueprint, terminal) && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                        var commands = starts.Commands.Where(value => value is UnitAttack ? starts.CompletedAttack((UnitAttack)value, target) :
                            value is UnitUseAbility && value.IsFinished && value.Result == UnitCommand.ResultType.Success).ToArray();
                        bool costsExact = commands.Length > 0 && commands.All(value => !value.Cutscene && !value.IsIgnoreCooldown && economy.Count(value) == 1 && economy.Exact(value, false));
                        bool outcome = cell == "ai-control" ? aiTicks > 0 && owned.Length == 1 && applications.Any(value => value.AppliedBuff != null && value.Duration == TimeSpan.FromSeconds(18)) :
                            cell == "protected" ? owned.Length == 0 && applications.Any(value => !value.CanApply && value.AppliedBuff == null) :
                            aiTicks > 0 && !starts.Commands.OfType<UnitUseAbility>().Any() && attacks.Damage.Any(value => !value.IsFake && value.Damage > 0) &&
                            drain.Rows.Any(value => (string)value["type"] == "Temporary" && (int)value["count"] == 1 && !(bool)value["immune"] && (double)value["durationSeconds"] == 6);
                        Sprint25Check("sprint33-succubus-" + cell, costsExact && outcome, new JObject {
                            ["eligible"] = eligible, ["aiTicks"] = aiTicks, ["eligibleActorsNotTicked"] = new JArray(eligibleActors.Where(value => value != owner.UniqueId)), ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["nativeAttackLifecycle"] = starts.AttackRows, ["enemy"] = owner.IsEnemy(target), ["physicalLos"] = owner.HasLOS(target),
                            ["brainActions"] = new JArray(owner.Blueprint.Brain.Actions.Select(value => value.name)), ["saves"] = saves.Rows, ["applications"] = saves.Applications, ["drain"] = drain.Rows,
                            ["activeOwnedDomination"] = owned.Length, ["forcedRolls"] = 0, ["fixtureHD"] = target.Descriptor.Progression.CharacterLevel },
                            "Preserved native AI/action use, three-round nonstacking control, source-alignment protection, humanoid eligibility and ordinary attack fallback with one-round temporary drain; no kiss/grapple fidelity claim.");
                        if (!costsExact || !outcome) throw new InvalidOperationException("Preserved Succubus consumer failed: " + cell);
                        if (cell == "ai-control")
                        {
                            Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true);
                            DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                            DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(10);
                            while (DateTime.UtcNow < cleanupDeadline && owned.Any(value => !value.IsDisposed)) yield return 0;
                            bool clean = owned.All(value => value.IsDisposed);
                            Sprint25Check("sprint33-succubus-source-cleanup", clean, new JObject { ["remainingOwnedControl"] = owned.Count(value => !value.IsDisposed) },
                                "Removing the exact summoned controller ends its own domination through the released cleanup component.");
                            if (!clean) throw new InvalidOperationException("Succubus source-owned control remained.");
                        }
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(starts); EventBus.Unsubscribe(saves); EventBus.Unsubscribe(attacks); EventBus.Unsubscribe(drain);
                    if (owner != null && !owner.Destroyed) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); if (owner.IsInCombat) owner.LeaveCombat(); DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); }
                    if (protection != null && !protection.IsDisposed) protection.Remove();
                    if (typeAdded) target.Descriptor.RemoveFact(outsider);
                    target.Commands.InterruptAll(true); if (target.IsInCombat) target.LeaveCombat(); Game.Instance.Player.UpdateIsInCombat();
                }
            }
        }
    }
}
