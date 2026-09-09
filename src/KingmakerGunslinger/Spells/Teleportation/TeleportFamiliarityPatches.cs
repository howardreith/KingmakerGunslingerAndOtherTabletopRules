using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Globalmap.State;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Intentionally no HarmonyPatch attributes: OFF installs no arrival/load hooks.
    internal static class TeleportFamiliarityPatches
    {
        private static bool _installed;
        internal static bool Installed { get { return _installed && _transpiled; } }
        private static bool _transpiled;

        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            MethodInfo movement = typeof(MapMovementController).GetMethod("MoveAlongEdge",
                BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            MethodInfo loaded = typeof(Player).GetMethod("OnAreaLoaded", Type.EmptyTypes);
            try
            {
                MethodBody body = movement == null ? null : movement.GetMethodBody();
                if (movement == null || movement.ReturnType != typeof(void) || loaded == null ||
                    body == null || body.LocalVariables.Count < 2 ||
                    body.LocalVariables[0].LocalType != typeof(MapTravelData) ||
                    body.LocalVariables[1].LocalType != typeof(float) || body.ExceptionHandlingClauses.Count != 0)
                    throw new InvalidOperationException("Native movement/load contract changed.");
                context.Harmony.Patch(movement, null, null, Callback("Transpiler"));
                if (!_transpiled) throw new InvalidOperationException("Native initialized progress seam was not applied.");
                context.Harmony.Patch(loaded, null, Callback("LoadedPostfix"), null);
                _installed = true;
                context.Logger.Info("teleportation", "familiarity.hooks-installed",
                    "movement=MapMovementController.MoveAlongEdge;load=Player.OnAreaLoaded;nativeFlowPreserved=true");
            }
            catch (Exception exception)
            {
                // These two exact methods have no other production KMG patch owners.
                _installed = false;
                TryUnpatch(context, movement, HarmonyPatchType.Transpiler);
                TryUnpatch(context, loaded, HarmonyPatchType.Postfix);
                context.Logger.Failure("teleportation", "familiarity.hooks-unavailable",
                    "Teleportation familiarity fails closed; unrelated feature modules continue.", exception);
            }
        }

        private static void TryUnpatch(ModContext context, MethodInfo method, HarmonyPatchType type)
        {
            if (method == null) return;
            try { context.Harmony.Unpatch(method, type, context.ModId); }
            catch (Exception exception)
            {
                context.Logger.Failure("teleportation", "familiarity.hook-cleanup-failed",
                    "Remaining callback is inert because the module adapter is unavailable.", exception);
            }
        }

        private static HarmonyMethod Callback(string name)
        { return new HarmonyMethod(typeof(TeleportFamiliarityPatches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)); }

        private static void LoadedPostfix(Player __instance)
        { TeleportFamiliarityRuntime.EnsureLedger(__instance); }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            _transpiled = false;
            List<CodeInstruction> source = instructions.ToList();
            FieldInfo walked = typeof(MapTravelData).GetField("WalkedDistance");
            var seams = new List<int>();
            for (int index = 2; index < source.Count; index++)
                if (source[index].opcode == OpCodes.Stloc_1 && source[index - 1].opcode == OpCodes.Ldfld &&
                    Equals(source[index - 1].operand, walked) && source[index - 2].opcode == OpCodes.Ldloc_0)
                    seams.Add(index);
            if (seams.Count != 1 || source.Any(value => value.blocks.Count != 0) ||
                source.Any(value => Equals(value.operand, typeof(TeleportFamiliarityRuntime).GetMethod("Begin",
                    BindingFlags.Static | BindingFlags.NonPublic)))) return source;
            LocalBuilder sample = generator.DeclareLocal(typeof(TeleportFamiliarityRuntime.MovementObservation));
            MethodInfo begin = typeof(TeleportFamiliarityRuntime).GetMethod("Begin", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo end = typeof(TeleportFamiliarityRuntime).GetMethod("End", BindingFlags.Static | BindingFlags.NonPublic);
            var result = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldnull), new CodeInstruction(OpCodes.Stloc, sample) };
            for (int index = 0; index < source.Count; index++)
            {
                CodeInstruction instruction = source[index];
                if (instruction.opcode == OpCodes.Ret)
                {
                    var load = new CodeInstruction(OpCodes.Ldloc, sample);
                    load.labels.AddRange(instruction.labels);
                    instruction.labels.Clear();
                    result.Add(load);
                    result.Add(new CodeInstruction(OpCodes.Call, end));
                }
                result.Add(instruction);
                if (index == seams[0])
                {
                    result.Add(new CodeInstruction(OpCodes.Ldloc_0));
                    result.Add(new CodeInstruction(OpCodes.Ldloc_1));
                    result.Add(new CodeInstruction(OpCodes.Call, begin));
                    result.Add(new CodeInstruction(OpCodes.Stloc, sample));
                }
            }
            _transpiled = true;
            return result;
        }
    }
}
