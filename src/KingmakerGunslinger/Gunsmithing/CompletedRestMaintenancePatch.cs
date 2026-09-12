using System;
using System.Collections.Generic;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Rest;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Recovery;

namespace KingmakerGunslinger.Gunsmithing
{
    /// <summary>
    /// Runs automatic firearm maintenance exactly once per genuine completed
    /// full rest. The prefix fires at the shared rest-termination boundary
    /// (before the trailing native autosave); only a successful,
    /// non-encounter, non-skip-time rest qualifies. One participating
    /// gunsmith with the reusable kit in the shared inventory restores every
    /// carried Broken and Wrecked firearm to Normal (Wrecked stays unloaded)
    /// through the same expected-state-guarded exact-item transitions used
    /// by misfire and discharge; a per-item failure never breaks the native
    /// rest completion or the other items. Cancelled, interrupted, partial,
    /// and time-skip rests never reach the mutation.
    /// </summary>
    [HarmonyPatch(typeof(RestController), "StopRestProcess")]
    internal static class CompletedRestMaintenancePatch
    {
        private static readonly object Gate = new object();
        private static RestStatus _processedStatus;
        private static WeaponBlueprintAccess _weaponTypeAccess;

        internal static long RestCompletionsSeen { get; private set; }
        internal static long MaintenanceRuns { get; private set; }

        private static void Prefix(RestController __instance)
        {
            try
            {
                Run(__instance);
            }
            catch (Exception exception)
            {
                LogFailure(
                    "rest-maintenance.faulted",
                    "Completed-rest firearm maintenance failed; the native rest completion is unaffected.",
                    exception);
            }
        }

        private static void Run(RestController controller)
        {
            if (controller == null)
            {
                return;
            }

            RestStatus status = controller.Status;
            if (status == null)
            {
                return;
            }

            bool genuineCompletion = status.RestSucceeded &&
                !status.NightRandomEncounter &&
                !status.SkipTime;
            lock (Gate)
            {
                if (!genuineCompletion ||
                    ReferenceEquals(_processedStatus, status))
                {
                    return;
                }

                _processedStatus = status;
            }

            RestCompletionsSeen++;

            Game game = Game.Instance;
            if (game == null || game.Player == null ||
                game.Player.Inventory == null)
            {
                LogFailure(
                    "rest-maintenance.no-campaign",
                    "A completed rest had no active campaign inventory; firearm maintenance was skipped.",
                    null);
                return;
            }

            BlueprintFeature gunsmithFeature =
                BlueprintBootstrap.GunslingerClass == null
                    ? null
                    : BlueprintBootstrap.GunslingerClass.Gunsmithing;
            BlueprintItem kit = BlueprintBootstrap.GunsmithingSupplies == null
                ? null
                : BlueprintBootstrap.GunsmithingSupplies.GunsmithKit;
            if (gunsmithFeature == null || kit == null)
            {
                LogFailure(
                    "rest-maintenance.blueprints-missing",
                    "Gunsmithing repair blueprints were unavailable at rest completion; firearm maintenance was skipped.",
                    null);
                return;
            }

            // Equipment scope uses living participants; the gunsmith
            // capability itself uses the completed-rest predicate (sleep
            // lifecycle evidence in FirearmMaintenanceCapability).
            List<UnitEntityData> participants = game.Player.AllCharacters ==
                null
                ? null
                : game.Player.AllCharacters.FindAll(
                    FirearmMaintenanceCapability.IsLivingParticipant);
            bool hasCapableRepairer = participants != null &&
                participants.Exists(unit => HasGunsmithingCapability(unit, gunsmithFeature));
            var kitInventory = new KingmakerRepairKitInventory(
                game.Player.Inventory, kit);
            bool hasKit = kitInventory.Count() > 0;

            List<ItemEntityWeapon> candidates =
                CollectCandidateFirearms(participants, game.Player.Inventory);

            int damaged = 0;
            foreach (ItemEntityWeapon weapon in candidates)
            {
                if (IsDamaged(weapon))
                {
                    damaged++;
                }
            }

            CompletedRestMaintenanceStatus decision =
                CompletedRestMaintenancePolicy.EvaluateRest(
                    true, status.NightRandomEncounter, status.SkipTime,
                    hasCapableRepairer, hasKit);

            string blocked = CompletedRestMaintenancePolicy.DescribeBlocked(
                decision, damaged);
            if (blocked != null)
            {
                PublishSummary(blocked);
            }

            if (decision != CompletedRestMaintenanceStatus.MaintenanceDue)
            {
                return;
            }

            MaintenanceRuns++;
            int restoredBroken = 0;
            int restoredWrecked = 0;
            int failed = 0;
            foreach (ItemEntityWeapon weapon in candidates)
            {
                try
                {
                    CompletedRestItemOutcome outcome = RestoreOne(weapon);
                    if (outcome == CompletedRestItemOutcome.RestoredBroken)
                        restoredBroken++;
                    else if (outcome == CompletedRestItemOutcome.RestoredWrecked)
                        restoredWrecked++;
                    else if (outcome == CompletedRestItemOutcome.Failed)
                        failed++;
                }
                catch (Exception exception)
                {
                    failed++;
                    LogFailure(
                        "rest-maintenance.item-failed",
                        "One firearm could not be restored by completed-rest maintenance; the remaining items and the native rest are unaffected. itemBlueprint=" +
                        (weapon == null || weapon.Blueprint == null
                            ? "<unknown>"
                            : weapon.Blueprint.name),
                        exception);
                }
            }

            string summary = CompletedRestMaintenancePolicy.DescribeRestored(
                restoredBroken, restoredWrecked, failed);
            if (summary != null)
            {
                PublishSummary(summary);
            }
        }

        private enum CompletedRestItemOutcome
        {
            SkippedNormal = 0,
            RestoredBroken = 1,
            RestoredWrecked = 2,
            Failed = 3
        }

        private static CompletedRestItemOutcome RestoreOne(ItemEntityWeapon weapon)
        {
            if (weapon == null)
            {
                return CompletedRestItemOutcome.SkippedNormal;
            }

            FirearmItemStateSnapshot before =
                FirearmRuntimeState.Service.GetOrCreate(weapon);
            FirearmState beforeState = before.Repository.State;
            CompletedRestItemDecision decision =
                CompletedRestMaintenancePolicy.EvaluateItem(beforeState.Condition);
            if (decision == CompletedRestItemDecision.SkipNormal)
            {
                return CompletedRestItemOutcome.SkippedNormal;
            }

            FirearmState repaired = FirearmStateMachine.Repair(beforeState);
            FirearmItemStateSnapshot after = FirearmRuntimeState.Service.Transition(
                weapon,
                current =>
                {
                    if (current != beforeState)
                    {
                        throw new InvalidOperationException(
                            "The exact firearm changed between completed-rest inspection and restoration.");
                    }

                    return repaired;
                });
            if (after.Repository.State != repaired)
            {
                throw new InvalidOperationException(
                    "The exact firearm did not retain the completed-rest restoration state.");
            }

            FirearmConditionCombatLog.Publish(
                after.ItemDisplayName,
                beforeState.Condition,
                repaired.Condition,
                decision == CompletedRestItemDecision.RestoreWrecked
                    ? "full-rest maintenance (unloaded)"
                    : "full-rest maintenance");
            return decision == CompletedRestItemDecision.RestoreWrecked
                ? CompletedRestItemOutcome.RestoredWrecked
                : CompletedRestItemOutcome.RestoredBroken;
        }

        private static bool HasGunsmithingCapability(
            UnitEntityData unit,
            BlueprintFeature gunsmithFeature)
        {
            // Post-rest capability through the completed-rest predicate:
            // transient camping sleep (lifted inside the completion
            // coroutine, after this prefix) is accepted; death, unconscious
            // life-state, and genuine incapacity are rejected (CR2-03).
            return FirearmMaintenanceCapability
                .CanMaintainFirearmsAtCompletedRest(unit.Descriptor);
        }

        /// <summary>
        /// Collects the exact concrete firearms in the shared carried
        /// inventory and the participants' equipment slots (current and
        /// alternate weapon sets, additional limbs), deduplicated by
        /// reference. Remote stashes, vendor stock, ground loot, and
        /// nonparticipant equipment are never consulted.
        /// </summary>
        private static List<ItemEntityWeapon> CollectCandidateFirearms(
            List<UnitEntityData> participants,
            ItemsCollection sharedInventory)
        {
            var candidates = new List<ItemEntityWeapon>();
            if (sharedInventory != null && sharedInventory.Items != null)
            {
                foreach (ItemEntity item in sharedInventory.Items)
                {
                    AddIfMarkedFirearm(candidates, item as ItemEntityWeapon);
                }
            }

            if (participants != null)
            {
                foreach (UnitEntityData unit in participants)
                {
                    UnitBody body = unit == null ? null : unit.Body;
                    if (body == null)
                    {
                        continue;
                    }

                    foreach (ItemSlot slot in body.AllSlots)
                    {
                        AddIfMarkedFirearm(
                            candidates,
                            slot == null ? null : slot.Item as ItemEntityWeapon);
                    }
                }
            }

            return candidates;
        }

        private static void AddIfMarkedFirearm(
            List<ItemEntityWeapon> candidates,
            ItemEntityWeapon weapon)
        {
            if (weapon == null || !IsMarkedFirearm(weapon))
            {
                return;
            }

            foreach (ItemEntityWeapon existing in candidates)
            {
                if (ReferenceEquals(existing, weapon))
                {
                    return;
                }
            }

            candidates.Add(weapon);
        }

        private static bool IsMarkedFirearm(ItemEntityWeapon weapon)
        {
            try
            {
                if (weapon == null || weapon.Blueprint == null)
                {
                    return false;
                }

                WeaponBlueprintAccess access = GetWeaponTypeAccess();
                if (access == null)
                {
                    return false;
                }

                BlueprintWeaponType weaponType = access.Get(weapon.Blueprint);
                if (weaponType == null || weaponType.ComponentsArray == null)
                {
                    return false;
                }

                int markers = 0;
                foreach (object component in weaponType.ComponentsArray)
                {
                    if (component is FirearmDefinitionComponent)
                    {
                        markers++;
                    }
                }

                return markers == 1;
            }
            catch
            {
                return false;
            }
        }

        private static WeaponBlueprintAccess GetWeaponTypeAccess()
        {
            lock (Gate)
            {
                if (_weaponTypeAccess == null)
                {
                    _weaponTypeAccess = WeaponBlueprintAccess.Resolve();
                }

                return _weaponTypeAccess;
            }
        }

        private static bool IsDamaged(ItemEntityWeapon weapon)
        {
            if (weapon == null)
            {
                return false;
            }

            try
            {
                FirearmCondition condition = FirearmRuntimeState.Service
                    .GetOrCreate(weapon)
                    .Repository.State.Condition;
                return condition == FirearmCondition.Broken ||
                    condition == FirearmCondition.Wrecked;
            }
            catch
            {
                return false;
            }
        }

        private static void PublishSummary(string message)
        {
            Diagnostics.NativeCombatLog.Publish(
                "rest-maintenance",
                "rest-maintenance.summary-failed",
                message,
                "Completed-rest maintenance committed, but its native summary line failed.");
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info("rest-maintenance", "rest.summary", message);
            }
        }

        private static void LogFailure(
            string eventName,
            string message,
            Exception exception)
        {
            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return;
            }

            if (exception == null)
            {
                context.Logger.Info("rest-maintenance", eventName, message);
            }
            else
            {
                context.Logger.Failure(
                    "rest-maintenance", eventName, message, exception);
            }
        }
    }
}
