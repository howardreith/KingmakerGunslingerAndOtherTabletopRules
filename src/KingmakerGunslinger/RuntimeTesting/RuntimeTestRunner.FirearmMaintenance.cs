using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Guarded native lane for the Z-FIREARM-MAINTENANCE sequence
        /// interruption. The committed break is driven through the real
        /// RuleAttackWithWeapon pipeline with a forced natural roll and the
        /// construction gate through the real patched
        /// UnitAttack.CreateAttackCommand entry point.
        ///
        /// Bridge-test labeling (review R6): the player-order step injects
        /// its authorization through the guarded runtime seam because this
        /// headless scenario cannot perform a genuine native selection and
        /// click; the unselected-click negative control DOES run the real
        /// selection filter. Production input-path proof belongs to the
        /// native interactive/owner lanes.
        /// </summary>
        private RuntimeTestResult RunDisposableFirearmBreakInterruption()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            var assertions = new List<RuntimeTestAssertion>();
            string stage = "fixture";
            long rejectionsBefore =
                EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections;
            bool conditionBroken = false;
            bool suppressedAfterBreak = false;
            bool unselectedClickAuthorizesNothing = true;
            bool automaticRejected = false;
            bool stillSuppressedAfterAutomatic = true;
            bool readinessEstablished = false;
            UnitCommand playerCommand = null;
            bool playerAllowed = false;
            bool suppressionConsumed = false;
            bool oneShotExhausted = true;
            bool clickEndClears = true;
            bool wreckedCommitted = false;
            bool suppressedAgainAfterWreck = false;
            UnitCommand postWreckCommand = null;
            bool postWreckRejectedByInterruption = false;
            bool deadShotCondition = false;
            bool deadShotSuppressed = false;
            bool cleaned = false;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.State.Immortality.Retain();
                target.Descriptor.State.Immortality.Retain();
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);

                stage = "committed-break";
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target,
                    weapon, 0));
                FirearmItemStateSnapshot afterBreak =
                    FirearmRuntimeState.Service.GetOrCreate(weapon);
                conditionBroken =
                    afterBreak.Repository.State.Condition == FirearmCondition.Broken;
                suppressedAfterBreak = BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);

                stage = "unselected-click-authorizes-nothing";
                // The fixture attacker is not in the native selection, so the
                // real Begin filter must record no authorization for it.
                BrokenSequenceSuppressionRuntime.BeginPlayerAttackClick(target);
                UnitCommand unselectedCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                unselectedClickAuthorizesNothing = unselectedCommand == null;
                BrokenSequenceSuppressionRuntime.EndPlayerAttackClick();

                stage = "automatic-construction-rejected";
                long before = EmptyFirearmAttackCommandPatch
                    .SequenceInterruptionRejections;
                UnitCommand automaticCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                automaticRejected = automaticCommand == null &&
                    EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections >
                        before;
                stillSuppressedAfterAutomatic = BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);

                stage = "player-issued-order-released";
                // Readiness is re-established explicitly (one loaded round in
                // the Broken firearm): the authorization gate is what is
                // under test, not ammunition readiness (review R6).
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                readinessEstablished = FirearmRuntimeState.Service
                    .GetOrCreate(weapon).Repository.State.LoadedRounds == 1;
                BrokenSequenceSuppressionRuntime
                    .AuthorizePlayerAttackOrderForRuntimeTest(attacker, target);
                playerCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                playerAllowed = playerCommand != null;
                suppressionConsumed = !BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);

                stage = "authorization-one-shot";
                // The consumed order cannot authorize a second construction:
                // a brain re-issue in the same conditions is still rejected.
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                long beforeOneShot = EmptyFirearmAttackCommandPatch
                    .SequenceInterruptionRejections;
                BrokenSequenceSuppressionRuntime.OnCommittedDegradation(
                    attacker, weapon);
                UnitCommand secondCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                oneShotExhausted = secondCommand == null &&
                    EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections >
                        beforeOneShot;

                stage = "click-end-clears";
                BrokenSequenceSuppressionRuntime
                    .AuthorizePlayerAttackOrderForRuntimeTest(attacker, target);
                BrokenSequenceSuppressionRuntime.EndPlayerAttackClick();
                UnitCommand clearedCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                clickEndClears = clearedCommand == null;

                stage = "wrecked-recommit";
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target,
                    weapon, 0));
                FirearmItemStateSnapshot afterWreck =
                    FirearmRuntimeState.Service.GetOrCreate(weapon);
                wreckedCommitted =
                    afterWreck.Repository.State.Condition == FirearmCondition.Wrecked &&
                    afterWreck.Repository.State.IsEmpty;
                suppressedAgainAfterWreck = BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);
                long beforeWreck = EmptyFirearmAttackCommandPatch
                    .SequenceInterruptionRejections;
                postWreckCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                postWreckRejectedByInterruption = postWreckCommand == null &&
                    EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections >
                        beforeWreck;

                stage = "dead-shot-commit-routes-interruption";
                // Review R2 behavioral evidence: a real all-misfire Dead Shot
                // (forced rolls through ExecuteForRuntimeTest) must suppress
                // the interrupted sequence through the shared notification.
                GunslingerClassBlueprintSet gunslinger =
                    BlueprintBootstrap.GunslingerClass;
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 11;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(
                    gunslinger.Grit.Resource, 4);
                BrokenSequenceSuppressionRuntime.ClearForRuntimeTest();
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                Deeds.DeadShotRuntime.ExecuteForRuntimeTest(
                    attacker, target, 1, 1, 1);
                deadShotCondition = FirearmRuntimeState.Service
                    .GetOrCreate(weapon).Repository.State.Condition ==
                    FirearmCondition.Broken;
                deadShotSuppressed = BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable firearm break interruption failed at stage " +
                    stage + ".", exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                BrokenSequenceSuppressionRuntime.ClearForRuntimeTest();
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null &&
                        attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (attacker != null) attacker.Dispose();
                if (target != null) target.Dispose();
                cleaned = true;
            }

            assertions.Add(Assertion(
                "committed-break-suspends-sequence",
                "Normal->Broken committed break marks the interrupted sequence",
                "conditionBroken=" + conditionBroken +
                ";suppressed=" + suppressedAfterBreak,
                conditionBroken && suppressedAfterBreak,
                "forced natural 1 through the real RuleAttackWithWeapon pipeline"));
            assertions.Add(Assertion(
                "unselected-click-authorizes-nothing",
                "a unit click records no authorization for an unselected executor",
                "rejected=" + unselectedClickAuthorizesNothing,
                unselectedClickAuthorizesNothing,
                "real BeginPlayerAttackClick selection filter; bridge scenario context"));
            assertions.Add(Assertion(
                "automatic-construction-rejected",
                "UnitAttack.CreateAttackCommand outside player intent returns null",
                "rejected=" + automaticRejected +
                ";stillSuppressed=" + stillSuppressedAfterAutomatic,
                automaticRejected && stillSuppressedAfterAutomatic,
                "the real patched construction entry point with no authorization"));
            assertions.Add(Assertion(
                "player-issued-order-released",
                "an authorized player order passes and consumes suppression",
                "ready=" + readinessEstablished +
                ";allowed=" + playerAllowed + ";consumed=" + suppressionConsumed,
                readinessEstablished && playerAllowed && suppressionConsumed,
                "BRIDGE TEST: authorization injected via guarded seam; native interactive lanes own production input proof"));
            assertions.Add(Assertion(
                "authorization-one-shot",
                "a consumed order cannot authorize a second automatic construction",
                "rejected=" + oneShotExhausted,
                oneShotExhausted,
                "re-suppressed weapon still rejected without a new order"));
            assertions.Add(Assertion(
                "click-end-clears",
                "authorization discarded when the click handler returns",
                "rejected=" + clickEndClears,
                clickEndClears,
                "BRIDGE TEST: EndPlayerAttackClick mirrors the patched postfix"));
            assertions.Add(Assertion(
                "wrecked-recommit-resuppresses",
                "Broken->Wrecked commit re-arms suppression and rejects construction through the interruption gate",
                "wrecked=" + wreckedCommitted +
                ";resuppressed=" + suppressedAgainAfterWreck +
                ";rejectedByInterruption=" + postWreckRejectedByInterruption,
                wreckedCommitted && suppressedAgainAfterWreck &&
                    postWreckRejectedByInterruption,
                "second forced misfire through the native rule pipeline"));
            assertions.Add(Assertion(
                "dead-shot-commit-routes-interruption",
                "a real all-misfire Dead Shot commit suppresses the sequence",
                "conditionBroken=" + deadShotCondition +
                ";suppressed=" + deadShotSuppressed,
                deadShotCondition && deadShotSuppressed,
                "DeadShotRuntime.ExecuteForRuntimeTest with forced 1s drives the deed's own commit through the shared notification"));
            assertions.Add(Assertion(
                "fixture-cleaned",
                "fixture weapon state, units, and forced rolls are cleaned",
                "cleaned=" + cleaned,
                cleaned,
                "Forget/RemoveItem/Dispose/CancelForcedNaturalRoll executed"));
            return CreateResult(
                assertions.TrueForAll(value => value.Status == "PASS")
                    ? RuntimeTestStatuses.Pass
                    : RuntimeTestStatuses.Fail,
                assertions, null);
        }
    }
}
