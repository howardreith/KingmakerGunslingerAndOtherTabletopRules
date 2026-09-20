using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunMagicCircleEvilNative()
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var actors = new List<UnitEntityData>();
            var prototypes = new List<BlueprintUnit>();
            var ownedAreas = new List<AreaEffectEntityData>();
            var capture = new CircleApplicationCapture();
            object levelController = null;
            BlueprintFaction enemyFaction = null;
            string stage = "guard", failure = null;
            var game = Game.Instance;
            bool paused = game.IsPaused;
            var beforeUnits = game.State.Units.All.ToArray();
            var beforeAreas = game.State.AreaEffects.All.ToArray();
            var beforeParty = game.Player.Party.ToArray();
            var beforeBuffs = beforeUnits.ToDictionary(unit => unit, unit => unit.Buffs.Enumerable.ToArray());
            try
            {
                if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableMagicCircleEvil ||
                    !_request.ExitAfterCompletion || _workingSaveSmoke == null ||
                    !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved ||
                    (string)_request.Parameters["saveName"] != WorkingSaveSmokeScenario.ExpectedName)
                    throw new InvalidOperationException("Exact completed read-only working-save guard required.");
                if (Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess || Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingScreenActive)
                    throw new InvalidOperationException("Native loading is not settled.");
                game.IsPaused = true;
                var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
                var anchor = game.Player.Party.First(value => value.IsInState && value.View != null);
                Vector3 center = anchor.Position;
                // Only request-local actors are created. None joins the party or
                // is written to disk. Native spawn, spellbook and command paths
                // are the same ones used by the existing cast qualification.
                var caster = CircleSpawn("Caster", center, anchor, actors, prototypes);
                var bearer = CircleSpawn("Bearer", center + new Vector3(0.5f, 0, 0), anchor, actors, prototypes);
                enemyFaction = UnityEngine.Object.Instantiate(caster.Blueprint.Faction);
                enemyFaction.name = "KMG_Runtime_MagicCircle_Hostile";
                enemyFaction.Peaceful = enemyFaction.AlwaysEnemy = enemyFaction.Neutral = enemyFaction.IsDirectlyControllable = false;
                enemyFaction.Dummy = null;
                enemyFaction.AttackFactions = new[] { caster.Blueprint.Faction };
                var recipient = CircleSpawn("Recipient", center + new Vector3(1f, 0, 0), anchor, actors, prototypes, enemyFaction);
                var controller = CircleSpawn("Controller", center + new Vector3(10f, 0, 0), anchor, actors, prototypes);
                caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                bearer.Descriptor.Alignment.Set(Alignment.ChaoticGood);
                recipient.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                controller.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                stage = "native-spellbook";
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer class");
                caster.Stats.Charisma.BaseValue = 30;
                AdvanceDisposableSpellcaster(caster.Descriptor, sorcerer, 8, ref levelController);
                var book = caster.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                while (book.CasterLevel < 8) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
                book.AddKnown(3, circle.Spell, true);
                int before = book.GetSpontaneousSlots(3);
                stage = "native-cast";
                CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                stage = "carrier-and-area-publication";
                game.EntityCreator.Tick();
                diagnostics.Add("carrier-count=" + CircleBuffs(bearer, circle.Carrier).Length +
                    ";areas=" + string.Join(",", game.State.AreaEffects.All.Except(beforeAreas)
                        .Select(value => value.Blueprint.AssetGuid + ":" + value.IsInState)));
                var carrier = CircleBuffs(bearer, circle.Carrier).Single();
                var area = game.State.AreaEffects.All.Except(beforeAreas)
                    .Single(value => ReferenceEquals(value.Blueprint, circle.Area));
                ownedAreas.Add(area);
                foreach (var actor in actors)
                    diagnostics.Add("membership-before-grid:" + actor.Descriptor.CustomName +
                        ";inGame=" + actor.IsInGame + ";dead=" + actor.Descriptor.State.IsDead +
                        ";position=" + actor.Position + ";area=" + area.Position +
                        ";shape=" + area.View.Shape.Contains(actor.Position, actor.View.Corpulence) +
                        ";nativePredicate=" + typeof(AreaEffectEntityData).GetMethod("ShouldUnitBeInside",
                            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(area, new object[] { actor }));
                CircleRefresh(area, actors);
                int after = book.GetSpontaneousSlots(3);
                assertions.Add(Assertion("circle-native-slot-debit", "one level-3 slot", before + "->" + after,
                    before > 0 && after == before - 1, "native UnitUseAbility and execution process"));
                assertions.Add(Assertion("circle-original-caster-context", "caster retained on another bearer",
                    "carrier=" + carrier.Context.MaybeCaster?.UniqueId + ";area=" + area.Context.MaybeCaster?.UniqueId,
                    ReferenceEquals(carrier.Context.MaybeCaster, caster) && ReferenceEquals(area.Context.MaybeCaster, caster) &&
                    carrier.Context.Params.CasterLevel == book.CasterLevel && CircleBuffs(caster, circle.Carrier).Length == 0,
                    "carrier and area native MechanicsContext; bearer owns the carrier"));
                assertions.Add(Assertion("circle-original-duration", (600 * book.CasterLevel).ToString(), carrier.TimeLeft.TotalSeconds.ToString(),
                    Math.Abs(carrier.TimeLeft.TotalSeconds - 600 * book.CasterLevel) < 2,
                    "native ContextActionApplyBuff duration from original caster rank"));
                assertions.Add(Assertion("circle-initial-membership", "bearer and nearby recipient each own one area contribution",
                    CircleBuffs(bearer, circle.Recipient).Length + "/" + CircleBuffs(recipient, circle.Recipient).Length,
                    CircleBuffs(bearer, circle.Recipient).Length == 1 && CircleBuffs(recipient, circle.Recipient).Length == 1 &&
                    CircleBuffs(recipient, circle.Recipient).Single().SourceAreaEffectId == area.UniqueId,
                    "native AbilityAreaEffectBuff entry and exact SourceAreaEffectId"));

                assertions.Add(Assertion("circle-non-ally-membership", "covered enemy", "enemy=" + recipient.IsEnemy(caster),
                    recipient.IsEnemy(caster) && CircleBuffs(recipient, circle.Recipient).Length == 1,
                    "request-local hostile faction; no changes to native factions"));
                if (CircleBuffs(bearer, circle.Recipient).Length != 1 || CircleBuffs(recipient, circle.Recipient).Length != 1)
                    throw new InvalidOperationException("Native coverage prerequisite failed after owned movement/grid reconciliation.");
                stage = "typed-defenses";
                var sourceAbility = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "d7cbd2004ce66a042aeab2e95a3c5c61", "neutral-descriptor control source for saving-throw reason");
                controller.Descriptor.Alignment.Set(Alignment.LawfulGood);
                int nonmatchingAc = CircleAttackAC(controller, recipient);
                int nonmatchingSave = CircleSave(controller, recipient, sourceAbility);
                controller.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                int matchingAc = CircleAttackAC(controller, recipient);
                int matchingSave = CircleSave(controller, recipient, sourceAbility);
                assertions.Add(Assertion("circle-native-defensive-outcomes", "+2 matching AC and save; no nonmatching bonus",
                    "ac=" + nonmatchingAc + "->" + matchingAc + ";will=" + nonmatchingSave + "->" + matchingSave,
                    matchingAc == nonmatchingAc + 2 && matchingSave == nonmatchingSave + 2,
                    "actual RuleAttackWithWeapon/RuleAttackRoll.TargetAC and RuleSavingThrow.StatValue"));

                stage = "actual-control";
                EventBus.Subscribe(capture);
                var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "d7cbd2004ce66a042aeab2e95a3c5c61", "Dominate Person ability");
                var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    "c0f4e1c24c9cd334ca988ed1bd9d201f", "Dominate Person terminal buff");
                Func<Buff> applyControl = () => recipient.Descriptor.Buffs.AddBuff(dominated,
                    new MechanicsContext(controller, controller.Descriptor, dominate, null, new TargetWrapper(recipient)), TimeSpan.FromMinutes(1));
                capture.Clear();
                var blocked = applyControl();
                bool enhancement = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
                assertions.Add(Assertion("circle-new-matching-control", enhancement ? "RuleApplyBuff veto" : "control applies",
                    "buff=" + (blocked != null) + ";canApply=" + capture.LastCanApply,
                    capture.Count == 1 && (enhancement ? blocked == null && !capture.LastCanApply : blocked != null && capture.LastCanApply),
                    "real native terminal-buff application, differing caster/bearer/recipient/controller alignments"));
                if (blocked != null) blocked.Remove();
                controller.Descriptor.Alignment.Set(Alignment.LawfulGood);
                capture.Clear();
                var allowed = applyControl();
                assertions.Add(Assertion("circle-wrong-alignment-control-positive", "native control buff applies", "buff=" + (allowed != null),
                    allowed != null && capture.Count == 1 && capture.LastCanApply,
                    "same native delivery and recipient; only controller alignment differs"));
                if (allowed != null) allowed.Remove();
                controller.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                stage = "movement";
                recipient.Position = center + new Vector3(8f, 0, 0);
                CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-exit", "zero circle contributions", CircleBuffs(recipient, circle.Recipient).Length.ToString(),
                    CircleBuffs(recipient, circle.Recipient).Length == 0, "native membership exit"));
                capture.Clear();
                var existing = applyControl();
                assertions.Add(Assertion("circle-unprotected-control-positive", "native control buff applies", "buff=" + (existing != null),
                    existing != null && capture.Count == 1 && capture.LastCanApply, "same evil controller after native area exit"));
                recipient.Position = bearer.Position;
                CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-existing-control-retained", "same control instance remains after entry", "active=" + (existing != null && existing.Active),
                    existing != null && existing.Active && recipient.Buffs.Enumerable.Contains(existing) && CircleBuffs(recipient, circle.Recipient).Length == 1,
                    "entry neither removes nor suppresses previously applied domination"));
                if (existing != null) existing.Remove();
                EventBus.Unsubscribe(capture);

                stage = "native-radius-and-bearer-motion";
                float edge = circle.Area.Size.Meters + recipient.View.Corpulence;
                recipient.Position = bearer.Position + new Vector3(edge - 0.05f, 0, 0);
                CircleRefresh(area, actors);
                bool inside = CircleBuffs(recipient, circle.Recipient).Length == 1;
                recipient.Position = bearer.Position + new Vector3(edge + 0.05f, 0, 0);
                CircleRefresh(area, actors);
                bool outside = CircleBuffs(recipient, circle.Recipient).Length == 0;
                assertions.Add(Assertion("circle-native-radius-boundary", "3.048m radius plus native corpulence; inside/outside",
                    "radius=" + circle.Area.Size.Meters + ";corpulence=" + recipient.View.Corpulence + ";inside=" + inside + ";outside=" + outside,
                    Math.Abs(circle.Area.Size.Meters - 3.048f) < 0.001f && inside && outside,
                    "ScriptZoneCylinder.Contains actual membership; line of effect remains native"));
                recipient.Position = center;
                bearer.Position = center + new Vector3(8f, 0, 0);
                CircleRefresh(area, actors); CircleRefresh(area, actors);
                bool followed = Vector3.Distance(area.Position, bearer.Position) < 0.1f && CircleBuffs(recipient, circle.Recipient).Length == 0;
                bearer.Position = center + new Vector3(0.5f, 0, 0);
                CircleRefresh(area, actors); CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-follows-bearer", "area follows bearer and re-entry retains original expiry", "followed=" + followed,
                    followed && CircleBuffs(recipient, circle.Recipient).Length == 1 &&
                    Math.Abs(carrier.TimeLeft.TotalSeconds - 600 * book.CasterLevel) < 2,
                    "caster stays stationary; native attached area moves with the touched bearer"));
                stage = "same-caster-overlap";
                CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                var areas = game.State.AreaEffects.All.Except(beforeAreas).Where(value => ReferenceEquals(value.Blueprint, circle.Area)).ToArray();
                foreach (var owned in areas) { if (!ownedAreas.Contains(owned)) ownedAreas.Add(owned); CircleRefresh(owned, actors); }
                assertions.Add(Assertion("circle-same-caster-overlap", "two distinct source-owned contributions",
                    CircleBuffs(recipient, circle.Recipient).Length.ToString(),
                    areas.Length == 2 && CircleBuffs(recipient, circle.Recipient).Length == 2 &&
                    CircleBuffs(recipient, circle.Recipient).Select(value => value.SourceAreaEffectId).Distinct().Count() == 2,
                    "second native cast; Stack identities retain exact area ownership"));
                assertions.Add(Assertion("circle-overlap-typed-nonstacking", "same +2 defenses with two circles",
                    "ac=" + CircleAttackAC(controller, recipient) + ";will=" + CircleSave(controller, recipient, sourceAbility),
                    matchingAc == nonmatchingAc + 2 && matchingSave == nonmatchingSave + 2 &&
                    CircleBuffs(recipient, circle.Recipient).Length == 2 &&
                    CircleAttackAC(controller, recipient) == matchingAc && CircleSave(controller, recipient, sourceAbility) == matchingSave,
                    "native typed-bonus rules with two distinct area contributions"));
                var individualBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    "4a6911969911ce9499bf27dde9bfcedc", "independent Protection from Evil");
                var individual = recipient.Buffs.AddBuff(individualBlueprint,
                    new MechanicsContext(caster, caster.Descriptor, circle.Spell, null, new TargetWrapper(recipient)), TimeSpan.FromMinutes(5));
                carrier.Remove();
                CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-owned-cleanup", "only second circle remains", CircleBuffs(recipient, circle.Recipient).Length.ToString(),
                    CircleBuffs(recipient, circle.Recipient).Length == 1 && CircleBuffs(recipient, circle.Recipient).All(value => value.SourceAreaEffectId != area.UniqueId),
                    "removing one carrier ends only its exact native area"));
                foreach (var remaining in CircleBuffs(bearer, circle.Carrier)) remaining.Remove();
                foreach (var remaining in areas.Where(value => !value.Destroyed)) CircleRefresh(remaining, actors);
                assertions.Add(Assertion("circle-independent-protection-survives", "standalone Protection retains exact instance and expiry",
                    "circleCount=" + CircleBuffs(recipient, circle.Recipient).Length + ";individual=" + (individual != null && individual.Active),
                    CircleBuffs(recipient, circle.Recipient).Length == 0 && individual != null && individual.Active &&
                    recipient.Buffs.Enumerable.Contains(individual) && Math.Abs(individual.TimeLeft.TotalSeconds - 300) < 2,
                    "both carriers removed; no blueprint-wide recipient removal"));
                if (individual != null) individual.Remove();
                stage = "four-variant-native-control";
                CircleFamily(caster, bearer, recipient, controller, book, actors, assertions, diagnostics);
                stage = "additional-native-deliveries";
                CircleOtherDeliveries(caster, bearer, recipient, controller, book, actors, assertions, diagnostics);
                stage = "native-scroll-acquisition";
                CircleAcquisition(caster, bearer, recipient, book, actors, prototypes, assertions, diagnostics);
                stage = "removed-caster-context";
                CircleRemovedCaster(controller, caster, bearer, recipient, actors, assertions, diagnostics);
                stage = "lifecycle";
                CircleLifecycle(caster, bearer, recipient, book, circle, actors, assertions, diagnostics);
                stage = "complete";
            }
            catch (Exception ex) { failure = stage + ": " + ex; }
            finally
            {
                EventBus.Unsubscribe(capture);
                if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
                foreach (var area in game.State.AreaEffects.All.Except(beforeAreas).ToArray())
                    if (BlueprintBootstrap.MagicCircles != null && BlueprintBootstrap.MagicCircles.Any(value => ReferenceEquals(value.Area, area.Blueprint)))
                    { area.ForceEnd(); area.Tick(); }
                foreach (var actor in actors) if (actor != null && !actor.Destroyed) actor.Destroy();
                game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                if (enemyFaction != null) UnityEngine.Object.Destroy(enemyFaction);
                game.IsPaused = paused;
            }
            assertions.Add(Assertion("circle-request-local-cleanup", "original units/party/areas and no save write",
                "stage=" + stage + ";writeObserved=" + _workingSaveSmoke.WriteObserved,
                game.State.Units.All.SequenceEqual(beforeUnits) && game.Player.Party.SequenceEqual(beforeParty) &&
                game.State.AreaEffects.All.SequenceEqual(beforeAreas) &&
                beforeBuffs.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)) && !_workingSaveSmoke.WriteObserved,
                "guarded request owns only disposable actors and areas; no persistence operation"));
            if (failure != null) assertions.Add(Assertion("circle-native-execution", "no exception", stage, false, failure));
            var result = CreateResult(failure == null && assertions.All(value => value.Status == RuntimeTestStatuses.Pass) ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, failure);
            result.Diagnostics.AddRange(diagnostics);
            result.WorkingSaveSmoke = _workingSaveSmoke.Stop();
            return result;
        }

        private static UnitEntityData CircleSpawn(string role, Vector3 position, UnitEntityData anchor,
            List<UnitEntityData> actors, List<BlueprintUnit> prototypes, BlueprintFaction faction = null)
        {
            var prototype = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            prototype.name = "KMG_Runtime_MagicCircle_" + role;
            prototype.IsCheater = true;
            prototype.Brain = null;
            if (faction != null) prototype.Faction = faction;
            prototypes.Add(prototype);
            var unit = Game.Instance.EntityCreator.SpawnUnit(prototype, position, Quaternion.identity, anchor.HoldingState);
            actors.Add(unit);
            Game.Instance.EntityCreator.Tick();
            if (!unit.IsInState || unit.View == null) throw new InvalidOperationException("Fixture did not enter native area: " + role);
            unit.Descriptor.CustomName = "KMG_RUNTIME_MAGIC_CIRCLE_" + role;
            unit.Stats.HitPoints.BaseValue = 10000;
            return unit;
        }

        private static Buff[] CircleBuffs(UnitEntityData unit, BlueprintBuff blueprint)
        { return unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, blueprint)).ToArray(); }

        private static void CircleRefresh(AreaEffectEntityData area, IList<UnitEntityData> actors)
        {
            Game.Instance.EntityCreator.Tick();
            CircleSynchronize(actors);
            typeof(AreaEffectEntityData).GetMethod("UpdateUnits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(area, null);
            area.Tick();
            Game.Instance.EntityDestroyer.Tick();
        }

        // Native paused movement reconciles view/data positions and the spatial
        // grid used by AreaEffectEntityData.UpdateUnits. Restrict this tick to
        // the request's standing actors, retaining the foreign world and clock.
        private static void CircleSynchronize(IList<UnitEntityData> actors)
        {
            var game = Game.Instance;
            if (!game.IsPaused || actors.Any(unit => !unit.IsInGame || unit.View?.MovementAgent == null ||
                unit.View.MovementAgent.IsReallyMoving))
                throw new InvalidOperationException("Circle fixture requires active standing native actors.");
            var awake = game.State.AwakeUnits.ToArray();
            var foreign = game.State.Units.All.Except(actors).ToArray();
            var positions = foreign.Select(unit => unit.Position).ToArray();
            var views = foreign.Select(unit => unit.View?.transform.position).ToArray();
            var buffs = foreign.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
            var time = game.TimeController.GameTime;
            float delta = game.TimeController.DeltaTime, gameDelta = game.TimeController.GameDeltaTime;
            try {
                game.State.AwakeUnits.Clear(); game.State.AwakeUnits.AddRange(actors);
                game.TimeController.SetDeltaTime(0); game.TimeController.SetGameDeltaTime(0);
                new UnitMoveController().Tick();
            }
            finally {
                game.State.AwakeUnits.Clear(); game.State.AwakeUnits.AddRange(awake);
                game.TimeController.SetDeltaTime(delta); game.TimeController.SetGameDeltaTime(gameDelta);
            }
            if (game.TimeController.GameTime != time || !game.State.AwakeUnits.SequenceEqual(awake) ||
                foreign.Where((unit, i) => unit.Position != positions[i] ||
                    !Nullable.Equals(unit.View?.transform.position, views[i]) ||
                    !unit.Buffs.Enumerable.SequenceEqual(buffs[i])).Any())
                throw new InvalidOperationException("Circle native movement tick changed foreign state.");
        }

        private static void CircleCast(UnitEntityData caster, UnitEntityData target, AbilityData data, List<string> evidence)
        { CircleCast(caster, new TargetWrapper(target), data, evidence); }

        private static void CircleCast(UnitEntityData caster, TargetWrapper target, AbilityData data, List<string> evidence)
        {
            var command = new UnitUseAbility(data, target);
            evidence.Add("cast:ability=" + data.Blueprint.AssetGuid + ";level=" + data.SpellLevel + ";targetable=" + data.CanTarget(target) + ";available=" + data.IsAvailable + ";canStart=" + command.CanStart);
            if (!data.CanTarget(target) || !data.IsAvailable || !command.CanStart)
                throw new InvalidOperationException("Native circle cast unavailable.");
            command.IgnoreCooldown(TimeSpan.Zero);
            var queuedBefore = caster.Commands.Queue.ToArray();
            caster.Commands.Run(command); command.Start();
            CircleCompleteCommand(command, evidence);
            var sticky = data.Blueprint.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectStickyTouch>();
            if (sticky != null && !ReferenceEquals(target.Unit, caster)) {
                // Observe the actual command created by native StickyTouch.
                // Never fabricate a replacement delivery or directly run effects.
                var held = caster.Commands.Queue.Except(queuedBefore).OfType<UnitUseAbility>()
                    .Single(value => ReferenceEquals(value.Spell.Blueprint, sticky.TouchDeliveryAbility));
                caster.Commands.RemoveFinishedAndUpdateQueue();
                if (!caster.Commands.Raw.Contains(held) || !held.CanStart)
                    throw new InvalidOperationException("Native held touch did not enter the command slot.");
                held.Start(); CircleCompleteCommand(held, evidence);
                caster.Commands.RemoveFinishedAndUpdateQueue();
                evidence.Add("native-held-touch:ability=" + held.Spell.Blueprint.AssetGuid + ";result=" + held.Result);
            }
        }

        private static void CircleCompleteCommand(UnitUseAbility command, List<string> evidence)
        {
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            if (command.ExecutionProcess == null) throw new InvalidOperationException("No native spell execution process.");
            for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++) command.ExecutionProcess.Tick();
            if (!command.ExecutionProcess.IsEnded) throw new InvalidOperationException("Native spell process did not settle.");
            bool processEnded = command.ExecutionProcess.IsEnded;
            evidence.Add("cast:process-ended=" + processEnded);
            if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
            if (!command.IsFinished) command.Tick();
            Game.Instance.EntityCreator.Tick();
            evidence.Add("cast:result=" + command.Result + ";ended=" + processEnded);
        }

        private static int CircleAttackAC(UnitEntityData attacker, UnitEntityData target)
        {
            int damage = target.Damage;
            try {
                var roll = Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target, attacker.Body.EmptyHandWeapon, 0)).AttackRoll;
                if (roll == null || roll.ACRule == null) throw new InvalidOperationException("Native attack did not calculate AC.");
                return roll.TargetAC;
            }
            finally { target.Damage = damage; }
        }

        private static int CircleSave(UnitEntityData source, UnitEntityData target, BlueprintAbility ability)
        {
            var context = new MechanicsContext(source, source.Descriptor, ability, null, new TargetWrapper(target));
            var rule = new RuleSavingThrow(target, SavingThrowType.Will, 100) { Reason = context };
            context.TriggerRule(rule);
            return rule.StatValue;
        }

        private sealed class CircleApplicationCapture : IGlobalRulebookHandler<RuleApplyBuff>
        {
            internal int Count;
            internal bool LastCanApply;
            internal void Clear() { Count = 0; LastCanApply = false; }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt) { Count++; LastCanApply = evt.CanApply; }
        }
    }
}
