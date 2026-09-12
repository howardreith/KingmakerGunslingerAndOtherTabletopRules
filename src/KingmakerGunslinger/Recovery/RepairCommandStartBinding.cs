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
    /// Binds the concrete repair target at genuine command commencement.
    /// A prefix on UnitUseAbility.OnStart records the exact equipped firearm
    /// and its start eligibility when the repair ability command begins; the
    /// delivery boundary then requires the same concrete reference, so a
    /// weapon or context change across the preceding full-round command
    /// prevents the repair. The binding is weakly keyed per caster and
    /// cleared when the command ends.
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
        }

        internal static void Bind(
            UnitDescriptor caster,
            ItemEntityWeapon weapon,
            bool eligibleAtStart)
        {
            if (caster == null)
            {
                return;
            }

            lock (Gate)
            {
                _bindings.Remove(caster);
                _bindings.Add(caster, new Binding
                {
                    Weapon = weapon,
                    EligibleAtStart = eligibleAtStart
                });
            }
        }

        internal static void Clear(UnitDescriptor caster)
        {
            if (caster == null)
            {
                return;
            }

            lock (Gate)
            {
                _bindings.Remove(caster);
            }
        }

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

                weapon = binding.Weapon;
                return true;
            }
        }

        internal static void ClearForRuntimeTest()
        {
            lock (Gate)
            {
                _bindings = new ConditionalWeakTable<UnitDescriptor, Binding>();
            }
        }
    }

    /// <summary>
    /// Harmony hooks that capture the exact repair target at real command
    /// start and release it at command end. Faults are contained: a failed
    /// capture leaves no binding, and delivery fails closed without one.
    /// </summary>
    [HarmonyPatch(typeof(UnitUseAbility), "OnStart")]
    internal static class RepairCommandStartBindingOnStartPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        {
            try
            {
                Run(__instance);
            }
            catch (Exception exception)
            {
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

        private static void Run(UnitUseAbility command)
        {
            if (command == null ||
                command.Spell == null ||
                command.Spell.Blueprint == null ||
                BlueprintBootstrap.RepairTestMusketAbility == null ||
                !ReferenceEquals(
                    command.Spell.Blueprint,
                    BlueprintBootstrap.RepairTestMusketAbility))
            {
                return;
            }

            UnitEntityData executor = command.Executor;
            if (executor == null || executor.Descriptor == null)
            {
                return;
            }

            ExactEquippedFirearmContext resolved;
            string reason;
            ItemEntityWeapon weapon = ExactEquippedFirearmResolver.TryResolve(
                executor.Descriptor, out resolved, out reason)
                ? resolved.Weapon
                : null;

            bool eligibleAtStart = weapon != null &&
                IsEligibleAtCommandStart(weapon);
            RepairCommandStartBinding.Bind(
                executor.Descriptor, weapon, eligibleAtStart);
        }

        private static bool IsEligibleAtCommandStart(ItemEntityWeapon weapon)
        {
            Game game = Game.Instance;
            if (RepairTestMusketRuntime.IsPartyInCombat())
            {
                return false;
            }

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
                    RepairCommandStartBinding.Clear(executor.Descriptor);
                }
            }
            catch
            {
                // A missed clear only leaves a weakly-held stale binding;
                // delivery still re-verifies everything against live state.
            }
        }
    }
}
