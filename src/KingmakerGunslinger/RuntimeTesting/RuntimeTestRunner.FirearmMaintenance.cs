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
        /// interruption: a verified committed degradation of the exact
        /// firearm during an attack suppresses automatic attack-command
        /// recreation until a genuine player-issued order (or a repair)
        /// releases it. The misfire itself is driven through the real
        /// RuleAttackWithWeapon pipeline with a forced natural roll; the
        /// construction gate is exercised through the real patched
        /// UnitAttack.CreateAttackCommand entry point.
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
            bool suppressedAfterBreak = false;
            bool conditionBroken = false;
            UnitCommand automaticCommand = null;
            bool automaticRejected = false;
            bool stillSuppressedAfterAutomatic = true;
            UnitCommand playerCommand = null;
            bool playerAllowed = false;
            bool suppressionConsumed = false;
            bool wreckedCommitted = false;
            bool suppressedAgainAfterWreck = false;
            UnitCommand postWreckCommand = null;
            bool postWreckRejected = false;
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

                stage = "automatic-construction-rejected";
                long before = EmptyFirearmAttackCommandPatch
                    .SequenceInterruptionRejections;
                automaticCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                automaticRejected = automaticCommand == null &&
                    EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections >
                        before;
                stillSuppressedAfterAutomatic = BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);

                stage = "player-issued-order-released";
                BrokenSequenceSuppressionRuntime.MarkPlayerAttackFrame();
                playerCommand = UnitAttack.CreateAttackCommand(
                    attacker, target);
                playerAllowed = playerCommand != null;
                suppressionConsumed = !BrokenSequenceSuppressionRuntime
                    .IsSuppressed(attacker, weapon);

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
                postWreckRejected = postWreckCommand == null &&
                    EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections >
                        beforeWreck;
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
                "automatic-construction-rejected",
                "UnitAttack.CreateAttackCommand outside player intent returns null",
                "rejected=" + automaticRejected +
                ";counterDelta=" +
                (EmptyFirearmAttackCommandPatch.SequenceInterruptionRejections -
                    rejectionsBefore),
                automaticRejected && stillSuppressedAfterAutomatic,
                "the real patched construction entry point with no player frame"));
            assertions.Add(Assertion(
                "player-issued-order-released",
                "a genuine player-issued construction passes and consumes suppression",
                "allowed=" + playerAllowed + ";consumed=" + suppressionConsumed,
                playerAllowed && suppressionConsumed,
                "player-attack frame marked through the same runtime path as the click hook"));
            assertions.Add(Assertion(
                "wrecked-recommit-resuppresses",
                "Broken->Wrecked commit re-arms suppression and rejects construction",
                "wrecked=" + wreckedCommitted +
                ";resuppressed=" + suppressedAgainAfterWreck +
                ";rejected=" + postWreckRejected,
                wreckedCommitted && suppressedAgainAfterWreck && postWreckRejected,
                "second forced misfire through the native rule pipeline"));
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
