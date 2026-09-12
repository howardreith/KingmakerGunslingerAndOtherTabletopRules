using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Binds the concrete repair target at genuine command commencement and
    /// owns the binding per command. A prefix on UnitUseAbility.OnStart
    /// records the exact equipped firearm, its full start eligibility, and
    /// the owning command for BOTH the live Repair Firearm blueprint and its
    /// approved hidden legacy Overhaul alias; the delivery boundary requires
    /// the same concrete reference from a binding whose command is still
    /// executing and was eligible at start. A binding is removed only by its
    /// own command's OnEnded (or replaced by a newer binding for the same
    /// caster), and a capture failure clears any existing binding instead of
    /// exposing stale authorization, so delivery fails closed.
    /// </summary>
    internal static class RepairCommandStartBinding
    {
        private static readonly object Gate = new object();
        private static ConditionalWeakTable<UnitDescriptor, Binding>
            _bindings = new ConditionalWeakTable<UnitDescriptor, Binding>();

        internal sealed class Binding
        {
            internal ItemEntityWeapon Weapon;
            internal bool EligibleAtStart;
            internal UnitUseAbility Command;
        }

        internal static void Bind(
            UnitDescriptor caster,
            ItemEntityWeapon weapon,
            bool eligibleAtStart,
            UnitUseAbility command)
        {
            if (caster == null)
            {
                return;
            }

            lock (Gate)
            {
                _bindings.Remove(caster);
                if (weapon == null || command == null)
                {
                    return;
                }

                _bindings.Add(caster, new Binding
                {
                    Weapon = weapon,
                    EligibleAtStart = eligibleAtStart,
                    Command = command
                });
            }
        }

        /// <summary>
        /// Removes the caster's binding only when the ending command owns it,
        /// so an unrelated or older ability finishing cannot erase a newer
        /// repair command's binding.
        /// </summary>
        internal static void OnRepairCommandEnded(
            UnitDescriptor caster,
            UnitUseAbility endedCommand)
        {
            if (caster == null)
            {
                return;
            }

            lock (Gate)
            {
                Binding binding;
                if (!_bindings.TryGetValue(caster, out binding))
                {
                    return;
                }

                if (ReferenceEquals(binding.Command, endedCommand))
                {
                    _bindings.Remove(caster);
                }
            }
        }

        /// <summary>
        /// The delivery-time binding: the exact weapon bound at command
        /// start, only when that binding recorded full start eligibility and
        /// its owning command is still executing.
        /// </summary>
        internal static bool TryGetBoundWeapon(
            UnitDescriptor caster,
            out ItemEntityWeapon weapon)
        {
            weapon = null;
            if (caster == null)
            {
                return false;
            }

            lock (Gate)
            {
                Binding binding;
                if (!_bindings.TryGetValue(caster, out binding))
                {
                    return false;
                }

                if (!binding.EligibleAtStart ||
                    binding.Command == null ||
                    binding.Command.IsFinished)
                {
                    return false;
                }

                weapon = binding.Weapon;
                return true;
            }
        }

        internal static void ClearForRuntimeTest()
        {
            lock (Gate)
            {
                _bindings =
                    new ConditionalWeakTable<UnitDescriptor, Binding>();
            }
        }
    }

    /// <summary>
    /// Harmony hooks that capture the exact repair target at real command
    /// start and release it at the owning command's end. Faults are
    /// contained by clearing the caster's binding: delivery fails closed
    /// without one.
    /// </summary>
    [HarmonyPatch(typeof(UnitUseAbility), "OnStart")]
    internal static class RepairCommandStartBindingOnStartPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        {
            UnitDescriptor caster = null;
            try
            {
                caster = Run(__instance);
            }
            catch (Exception exception)
            {
                // A failed capture must not leave any existing binding for
                // this caster: delivery then fails closed.
                RepairCommandStartBinding.Bind(caster, null, false, null);
                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.Logger.Failure(
                        "recovery",
                        "repair.command-binding-failed",
                        "The repair command-start binding could not be captured; delivery will fail closed.",
                        exception);
                }
            }
        }

        private static UnitDescriptor Run(UnitUseAbility command)
        {
            if (command == null ||
                command.Spell == null ||
                command.Spell.Blueprint == null ||
                !IsRepairAbility(command.Spell.Blueprint))
            {
                return null;
            }

            UnitEntityData executor = command.Executor;
            if (executor == null || executor.Descriptor == null)
            {
                return null;
            }

            ExactEquippedFirearmContext resolved;
            string reason;
            ItemEntityWeapon weapon = ExactEquippedFirearmResolver.TryResolve(
                executor.Descriptor, out resolved, out reason)
                ? resolved.Weapon
                : null;

            bool eligibleAtStart = weapon != null &&
                IsEligibleAtCommandStart(executor.Descriptor, weapon);
            RepairCommandStartBinding.Bind(
                executor.Descriptor, weapon, eligibleAtStart, command);
            return executor.Descriptor;
        }

        private static bool IsRepairAbility(
            Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility ability)
        {
            return ReferenceEquals(
                    ability, BlueprintBootstrap.RepairTestMusketAbility) ||
                ReferenceEquals(
                    ability, BlueprintBootstrap.OverhaulTestMusketAbility);
        }

        private static bool IsEligibleAtCommandStart(
            UnitDescriptor caster,
            ItemEntityWeapon weapon)
        {
            if (!FirearmMaintenanceCapability.CanMaintainFirearms(caster))
            {
                return false;
            }

            if (RepairTestMusketRuntime.IsPartyInCombat())
            {
                return false;
            }

            Game game = Game.Instance;
            if (game == null || game.Player == null ||
                game.Player.Inventory == null ||
                BlueprintBootstrap.GunsmithingSupplies == null ||
                BlueprintBootstrap.GunsmithingSupplies.GunsmithKit == null)
            {
                return false;
            }

            var kits = new KingmakerRepairKitInventory(
                game.Player.Inventory,
                BlueprintBootstrap.GunsmithingSupplies.GunsmithKit);
            if (kits.Count() <= 0)
            {
                return false;
            }

            FirearmState state = FirearmRuntimeState.Service
                .GetOrCreate(weapon)
                .Repository.State;
            return state.Condition == FirearmCondition.Broken;
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnEnded")]
    internal static class RepairCommandStartBindingOnEndedPatch
    {
        private static void Postfix(UnitUseAbility __instance)
        {
            try
            {
                UnitEntityData executor = __instance == null
                    ? null
                    : __instance.Executor;
                if (executor != null)
                {
                    RepairCommandStartBinding.OnRepairCommandEnded(
                        executor.Descriptor, __instance);
                }
            }
            catch
            {
                // A missed cleanup only leaves a binding whose command is
                // finished; delivery re-verifies command liveness and the
                // exact weapon against live state.
            }
        }
    }
}
