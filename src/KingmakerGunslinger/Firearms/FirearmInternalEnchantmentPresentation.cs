using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UI.Common;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Keeps item-owned serialization and ownership markers mechanical while
    /// excluding them from the two native weapon-enchantment text loops.
    /// </summary>
    internal static class FirearmInternalEnchantmentPresentation
    {
        private static int _suppressedCount;

        internal static int SuppressedCount
        { get { return _suppressedCount; } }

        internal static bool ShouldRender(ItemEnchantment enchantment)
        {
            BlueprintItemEnchantment blueprint = enchantment == null ? null :
                enchantment.Blueprint;
            if (!IsInternalMarker(blueprint)) return true;
            Interlocked.Increment(ref _suppressedCount);
            return false;
        }

        internal static bool IsInternalMarker(
            BlueprintItemEnchantment enchantment)
        {
            if (enchantment == null) return false;
            return CraftMagicItemsCompatibilityPolicy
                .IsInternalEnchantmentPresentationMarker(
                    (enchantment.ComponentsArray ??
                        new BlueprintComponent[0]).OfType<
                        FirearmStateTokenComponent>().Any(),
                    (enchantment.ComponentsArray ??
                        new BlueprintComponent[0]).OfType<
                        BatteredFirearmOriginComponent>().Any());
        }

        internal static IEnumerable<CodeInstruction> Transpile(
            IEnumerable<CodeInstruction> source, string target)
        {
            var values = source == null ? new List<CodeInstruction>() :
                source.ToList();
            MethodInfo predicate = typeof(
                FirearmInternalEnchantmentPresentation).GetMethod(
                    "ShouldRender", BindingFlags.Static |
                        BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo stringCheck = typeof(string).GetMethod(
                "IsNullOrEmpty", BindingFlags.Static | BindingFlags.Public,
                null, new[] { typeof(string) }, null);
            if (values.Count == 0 || predicate == null || stringCheck == null)
                throw Changed(target, "inputs");
            if (values.Any(value => IsCallTo(value, predicate)))
                throw Changed(target, "already-patched");

            int[] checks = values.Select((value, index) => new {
                    value, index })
                .Where(value => IsCallTo(value.value, stringCheck))
                .Select(value => value.index).ToArray();
            if (checks.Length != 1)
                throw Changed(target, "string-check-count=" + checks.Length);
            int branchIndex = NextMeaningful(values, checks[0]);
            CodeInstruction branch = values[branchIndex];
            if ((branch.opcode != OpCodes.Brtrue &&
                    branch.opcode != OpCodes.Brtrue_S) ||
                !(branch.operand is Label))
                throw Changed(target, "loop-continue-branch");

            int[] currentCalls = values.Take(checks[0])
                .Select((value, index) => new { value, index })
                .Where(value => IsCallNamed(value.value, "get_Current"))
                .Select(value => value.index).ToArray();
            if (currentCalls.Length != 1)
                throw Changed(target, "current-call-count=" +
                    currentCalls.Length);
            int storeIndex = NextMeaningful(values, currentCalls[0]);
            CodeInstruction load = LoadForStore(values[storeIndex]);
            if (load == null) throw Changed(target, "current-local");

            Label continueLoop = (Label)branch.operand;
            values.InsertRange(storeIndex + 1, new[]
            {
                load,
                new CodeInstruction(OpCodes.Call, predicate),
                new CodeInstruction(OpCodes.Brfalse, continueLoop)
            });
            return values;
        }

        private static CodeInstruction LoadForStore(
            CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Stloc_0)
                return new CodeInstruction(OpCodes.Ldloc_0);
            if (instruction.opcode == OpCodes.Stloc_1)
                return new CodeInstruction(OpCodes.Ldloc_1);
            if (instruction.opcode == OpCodes.Stloc_2)
                return new CodeInstruction(OpCodes.Ldloc_2);
            if (instruction.opcode == OpCodes.Stloc_3)
                return new CodeInstruction(OpCodes.Ldloc_3);
            if (instruction.opcode == OpCodes.Stloc)
                return new CodeInstruction(OpCodes.Ldloc,
                    instruction.operand);
            if (instruction.opcode == OpCodes.Stloc_S)
                return new CodeInstruction(OpCodes.Ldloc_S,
                    instruction.operand);
            return null;
        }

        private static int NextMeaningful(IList<CodeInstruction> values,
            int index)
        {
            for (int current = index + 1; current < values.Count; current++)
                if (values[current].opcode != OpCodes.Nop) return current;
            throw Changed("unknown", "next-instruction");
        }

        private static bool IsCallTo(CodeInstruction instruction,
            MethodBase method)
        {
            MethodBase operand = instruction == null ? null :
                instruction.operand as MethodBase;
            return (instruction.opcode == OpCodes.Call ||
                    instruction.opcode == OpCodes.Callvirt) &&
                operand != null && method != null && operand.Module ==
                    method.Module && operand.MetadataToken ==
                    method.MetadataToken;
        }

        private static bool IsCallNamed(CodeInstruction instruction,
            string name)
        {
            MethodBase operand = instruction == null ? null :
                instruction.operand as MethodBase;
            return (instruction.opcode == OpCodes.Call ||
                    instruction.opcode == OpCodes.Callvirt) &&
                operand != null && string.Equals(operand.Name, name,
                    StringComparison.Ordinal);
        }

        private static InvalidOperationException Changed(string target,
            string check)
        {
            return new InvalidOperationException(
                "Native firearm tooltip enchantment seam changed: " +
                target + ":" + check);
        }
    }

    [HarmonyPatch(typeof(UIUtilityItem), "FillWeaponQualities")]
    internal static class FirearmWeaponQualityDescriptionFilterPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return FirearmInternalEnchantmentPresentation.Transpile(
                instructions, "FillWeaponQualities");
        }
    }

    [HarmonyPatch(typeof(UIUtilityItem), "GetQualities")]
    internal static class FirearmWeaponQualityListFilterPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return FirearmInternalEnchantmentPresentation.Transpile(
                instructions, "GetQualities");
        }
    }
}
