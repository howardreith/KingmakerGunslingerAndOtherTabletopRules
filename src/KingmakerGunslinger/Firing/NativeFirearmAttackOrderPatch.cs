using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Harmony12.ILCopying;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;

namespace KingmakerGunslinger.Firing
{
    internal static class NativeFirearmAttackOrderPatch
    {
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static;
        private static MethodInfo Own(string name)
        { return typeof(NativeFirearmAttackOrderPatch).GetMethod(name, All); }
        private static MethodInfo Adapter(string name)
        { return typeof(NativeFirearmAttackOrder).GetMethod(name, All); }
        internal static void Install(HarmonyInstance harmony)
        {
            var click = typeof(Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler).GetMethod("OnClick", All);
            var controller = typeof(Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer.InGameInputLayer)
                .GetMethod("OnInteract", All);
            if (click == null || click.ReturnType != typeof(bool) || controller == null || controller.ReturnType != typeof(void))
                throw new MissingMethodException("Native firearm input entry points unavailable.");
            harmony.Patch(click, null, null, new HarmonyMethod(Own("Desktop")));
            harmony.Patch(controller, null, null, new HarmonyMethod(Own("Controller")));
            // Record automatic construction provenance before delayed submission;
            // constructing a command never supplies player authorization.
            harmony.Patch(typeof(UnitAttack).GetMethod("CreateAttackCommand", All),
                null, new HarmonyMethod(Own("Created")), null);
            harmony.Patch(typeof(UnitCommand).Assembly.GetType("Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack", true)
                .GetMethod("CreateCommand", All), null, new HarmonyMethod(Own("CreatedAutomatic")), null);
            harmony.Patch(typeof(UnitCommand).GetMethod("Init", All, null,
                new[] { typeof(UnitEntityData) }, null), null, new HarmonyMethod(Own("Initialized")), null);
            harmony.Patch(typeof(UnitCommands).GetMethod("Run", All, null,
                new[] { typeof(UnitCommand), typeof(bool), typeof(bool) }, null),
                new HarmonyMethod(Own("Submitting")), new HarmonyMethod(Own("Submitted")), null);
            harmony.Patch(typeof(UnitCommands).GetMethod("AddToQueueInternal", All),
                new HarmonyMethod(Own("Queued")), new HarmonyMethod(Own("Submitted")), null);
            harmony.Patch(typeof(UnitCommands).GetMethod("InterruptAndRemoveCommand", All, null,
                new[] { typeof(UnitCommand.CommandType), typeof(bool) }, null),
                null, new HarmonyMethod(Own("RemovedSlot")), null);
            harmony.Patch(typeof(UnitCommands).GetMethod("InterruptAll", All, null,
                new[] { typeof(bool) }, null), null, new HarmonyMethod(Own("InterruptedAll")), null);
            harmony.Patch(typeof(UnitCommands).GetMethod("InterruptAll", All, null,
                new[] { typeof(Func<UnitCommand, bool>) }, null),
                null, new HarmonyMethod(Own("ReconcileOwnership")), null);
            harmony.Patch(typeof(UnitAttack).GetMethod("UpdateTarget", All, null, Type.EmptyTypes, null),
                new HarmonyMethod(Own("ResolvingTarget")), new HarmonyMethod(Own("ResolvedTarget")), null);
            var actionPrefix = new HarmonyMethod(Own("Acting"));
            actionPrefix.prioritiy = Priority.First;
            harmony.Patch(typeof(UnitAttack).GetMethod("OnAction", All), actionPrefix, null, null);
            harmony.Patch(typeof(UnitAttack).GetMethod("TryMergeInto", All),
                new HarmonyMethod(Own("Merging")), new HarmonyMethod(Own("Merged")), null);
            harmony.Patch(typeof(UnitUseAbility).GetMethod("TryMergeInto", All),
                new HarmonyMethod(Own("MergingReload")), new HarmonyMethod(Own("MergedReload")), null);
            harmony.Patch(typeof(UnitCommand).GetMethod("ForceFinishForTurnBased", All),
                null, new HarmonyMethod(Own("ForceFinished")), null);
            harmony.Patch(typeof(UnitCommand).GetMethod("OnEnded", All),
                null, new HarmonyMethod(Own("Ended")), null);
        }
        private static void Created(UnitEntityData __0, UnitCommand __result)
        { NativeFirearmAttackOrder.Initialized(__result, __0); }
        private static void CreatedAutomatic(Kingmaker.Controllers.Brain.DecisionContext __0,
            UnitEntityData __1, UnitCommand __result)
        { if (__result != null) NativeFirearmAttackOrder.CreatedAutomatic(__result, __0.Unit, __1); }
        private static void Initialized(UnitCommand __instance, UnitEntityData __0)
        { NativeFirearmAttackOrder.Initialized(__instance, __0); }
        private static bool Submitting(UnitCommand __0, bool __1, UnitEntityData ___m_Owner)
        { return NativeFirearmAttackOrder.Submitting(___m_Owner, __0, __1); }
        private static bool Queued(UnitCommand __0, UnitEntityData ___m_Owner)
        { return NativeFirearmAttackOrder.Submitting(___m_Owner, __0, false); }
        private static void Submitted(UnitCommands __instance, UnitCommand __0, UnitEntityData ___m_Owner)
        { if (NativeFirearmAttackOrder.IsOrderContainer(__instance, ___m_Owner)) NativeFirearmAttackOrder.Submitted(___m_Owner, __0); }
        private static void RemovedSlot(UnitCommands __instance, UnitCommand.CommandType __0, UnitEntityData ___m_Owner)
        { if (NativeFirearmAttackOrder.IsOrderContainer(__instance, ___m_Owner)) NativeFirearmAttackOrder.RemovedSlot(___m_Owner, __0); }
        private static void InterruptedAll(UnitCommands __instance, UnitEntityData ___m_Owner)
        { if (NativeFirearmAttackOrder.IsOrderContainer(__instance, ___m_Owner)) NativeFirearmAttackOrder.InterruptedAll(___m_Owner); }
        private static void ReconcileOwnership(UnitCommands __instance, UnitEntityData ___m_Owner)
        { if (NativeFirearmAttackOrder.IsOrderContainer(__instance, ___m_Owner)) NativeFirearmAttackOrder.ReconcileOwnership(___m_Owner); }
        private static void ResolvingTarget(UnitAttack __instance, out NativeFirearmAttackOrder.TargetResolution __state)
        { __state = NativeFirearmAttackOrder.ResolvingTarget(__instance); }
        private static void ResolvedTarget(UnitAttack __instance, NativeFirearmAttackOrder.TargetResolution __state, bool __result)
        { NativeFirearmAttackOrder.ResolvedTarget(__instance, __state, __result); }
        private static bool Acting(UnitAttack __instance, ref UnitCommand.ResultType __result)
        {
            if (NativeFirearmAttackOrder.MayExecute(__instance)) return true;
            __result = UnitCommand.ResultType.Success;
            return false;
        }
        private static bool Merging(UnitAttack __instance, UnitCommand __0, ref bool __result)
        {
            if (NativeFirearmAttackOrder.MayMerge(__instance, __0)) return true;
            __result = false;
            return false;
        }
        private static void Merged(UnitAttack __instance, UnitCommand __0, bool __result)
        { NativeFirearmAttackOrder.Merged(__instance, __0, __result); }
        private static bool MergingReload(UnitUseAbility __instance, UnitCommand __0, ref bool __result)
        {
            if (NativeFirearmAttackOrder.MayMergeReload(__instance, __0)) return true;
            __result = false;
            return false;
        }
        private static void MergedReload(UnitUseAbility __instance, UnitCommand __0, bool __result)
        { NativeFirearmAttackOrder.Merged(__instance, __0, __result); }
        private static void ForceFinished(UnitCommand __instance)
        {
            // Native TB action rejection uses Success/IsActed even without a
            // discharge or cost. That terminal owner cannot retain authority.
            var binding = NativeFirearmAttackOrder.Get(__instance);
            if (binding != null && ReferenceEquals(binding.Order?.Owner, __instance))
                NativeFirearmAttackOrder.Interrupted(__instance);
        }
        private static void Ended(UnitCommand __instance)
        {
            // Native OnEnded(bool raiseEvent) also runs after successful shots.
            if (__instance.Result != UnitCommand.ResultType.Success && __instance is UnitAttack)
                NativeFirearmAttackOrder.Interrupted(__instance);
        }
        private static IEnumerable<CodeInstruction> Desktop(IEnumerable<CodeInstruction> source, ILGenerator generator)
        {
            var invocation = generator.DeclareLocal(typeof(NativeFirearmAttackOrder.InputInvocation));
            return GuardInput(DesktopBody(source, invocation), invocation, generator, typeof(bool));
        }
        private static IEnumerable<CodeInstruction> DesktopBody(IEnumerable<CodeInstruction> source, LocalBuilder invocation)
        {
            MethodInfo attack = typeof(UnitAttack).GetMethod("CreateAttackCommand", All);
            MethodInfo cast = typeof(UnitUseAbility).GetMethod("CreateCastCommand", All);
            MethodInfo run = typeof(UnitCommands).GetMethod("Run", All, null,
                new[] { typeof(UnitCommand) }, null);
            int attacks = 0, casts = 0, submissions = 0;
            yield return new CodeInstruction(OpCodes.Ldloc, invocation);
            yield return new CodeInstruction(OpCodes.Ldarg_S, (byte)4);
            yield return new CodeInstruction(OpCodes.Call, Adapter("SetInputSimulation"));
            foreach (var instruction in source)
            {
                if (Equals(instruction.operand, attack) && instruction.opcode == OpCodes.Call)
                { instruction.operand = Adapter("CreateAttack"); attacks++; }
                else if (Equals(instruction.operand, cast) && instruction.opcode == OpCodes.Call)
                {
                    instruction.opcode = OpCodes.Ldarg_1;
                    instruction.operand = null;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, Adapter("CreateAutoUse"));
                    casts++;
                    continue;
                }
                else if (Equals(instruction.operand, run) && instruction.opcode == OpCodes.Callvirt)
                {
                    instruction.opcode = OpCodes.Ldloc;
                    instruction.operand = invocation;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, Adapter("Submit"));
                    submissions++;
                    continue;
                }
                yield return instruction;
            }
            if (attacks != 1 || casts != 1 || submissions != 3)
                throw new InvalidOperationException("Native desktop firearm order callsites changed.");
        }
        private static IEnumerable<CodeInstruction> Controller(IEnumerable<CodeInstruction> source, ILGenerator generator)
        {
            var invocation = generator.DeclareLocal(typeof(NativeFirearmAttackOrder.InputInvocation));
            return GuardInput(ControllerBody(source, invocation), invocation, generator, typeof(void));
        }
        private static IEnumerable<CodeInstruction> ControllerBody(IEnumerable<CodeInstruction> source, LocalBuilder invocation)
        {
            var constructor = typeof(UnitAttack).GetConstructor(new[] { typeof(UnitEntityData) });
            var run = typeof(UnitCommands).GetMethod("Run", All, null,
                new[] { typeof(UnitCommand) }, null);
            int attacks = 0, submissions = 0;
            foreach (var instruction in source)
            {
                if (instruction.opcode == OpCodes.Newobj && Equals(instruction.operand, constructor))
                {
                    // Native local zero is the controller's current selected actor.
                    instruction.opcode = OpCodes.Ldloc_0;
                    instruction.operand = null;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, Adapter("CreateControllerAttack"));
                    attacks++;
                    continue;
                }
                if (instruction.opcode == OpCodes.Callvirt && Equals(instruction.operand, run))
                {
                    instruction.opcode = OpCodes.Ldloc;
                    instruction.operand = invocation;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, Adapter("Submit"));
                    submissions++;
                    continue;
                }
                yield return instruction;
            }
            if (attacks != 1 || submissions != 3)
                throw new InvalidOperationException("Native controller firearm order callsites changed.");
        }
        private static IEnumerable<CodeInstruction> GuardInput(IEnumerable<CodeInstruction> source,
            LocalBuilder invocation, ILGenerator generator, Type returnType)
        {
            Label exit = generator.DefineLabel();
            LocalBuilder returnValue = returnType == typeof(void) ? null : generator.DeclareLocal(returnType);
            yield return new CodeInstruction(OpCodes.Call, Adapter("BeginInput"));
            yield return new CodeInstruction(OpCodes.Stloc, invocation);
            var begin = new CodeInstruction(OpCodes.Nop);
            begin.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock, null));
            yield return begin;
            foreach (var instruction in source)
            {
                if (instruction.opcode != OpCodes.Ret) { yield return instruction; continue; }
                instruction.opcode = returnValue == null ? OpCodes.Ldloc : OpCodes.Stloc;
                instruction.operand = returnValue ?? invocation;
                yield return instruction;
                if (returnValue != null) yield return new CodeInstruction(OpCodes.Ldloc, invocation);
                yield return new CodeInstruction(OpCodes.Call, Adapter("CompleteInput"));
                yield return new CodeInstruction(OpCodes.Leave, exit);
            }
            var cleanup = new CodeInstruction(OpCodes.Ldloc, invocation);
            cleanup.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock, null));
            yield return cleanup;
            var end = new CodeInstruction(OpCodes.Call, Adapter("EndInput"));
            end.blocks.Add(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock, null));
            yield return end;
            var result = returnValue == null ? new CodeInstruction(OpCodes.Nop)
                : new CodeInstruction(OpCodes.Ldloc, returnValue);
            result.labels.Add(exit);
            yield return result;
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
