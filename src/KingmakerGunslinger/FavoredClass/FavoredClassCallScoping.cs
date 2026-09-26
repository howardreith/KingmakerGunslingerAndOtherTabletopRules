using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Harmony 1.2 has no finalizers, so a native call is scoped by replacing
    /// that one call site with a helper that makes the same call inside a
    /// scope closed in a finally block. The stack effect is unchanged: the
    /// helper takes the call's receiver and arguments (and, when asked, the
    /// patched method's own instance after them) and returns the same value.
    /// Only an exact single call site is replaced; anything else leaves the
    /// instructions untouched.
    /// </summary>
    internal static class FavoredClassCallScoping
    {
        /// <summary>
        /// Replaces the one call to <paramref name="native"/> in
        /// <paramref name="values"/> with a call to <paramref name="scoped"/>;
        /// returns false, changing nothing, unless exactly one such call exists
        /// outside any exception-block boundary.
        /// </summary>
        internal static bool ReplaceSingle(List<CodeInstruction> values, MethodInfo native, MethodInfo scoped,
            bool appendInstance)
        {
            if (values == null || native == null || scoped == null)
                return false;
            int index = -1;
            for (int position = 0; position < values.Count; position++)
            {
                if (!IsCallTo(values[position], native))
                    continue;
                if (index >= 0)
                    return false;
                index = position;
            }
            if (index < 0 || values[index].blocks.Count != 0)
                return false;
            CodeInstruction call = values[index];
            var replacement = new CodeInstruction(OpCodes.Call, scoped);
            if (!appendInstance)
            {
                replacement.labels.AddRange(call.labels);
                values[index] = replacement;
                return true;
            }
            // A branch to the call lands on the pushed instance instead.
            var instance = new CodeInstruction(OpCodes.Ldarg_0);
            instance.labels.AddRange(call.labels);
            values[index] = replacement;
            values.Insert(index, instance);
            return true;
        }

        private static bool IsCallTo(CodeInstruction instruction, MethodInfo method)
        {
            var operand = instruction == null ? null : instruction.operand as MethodInfo;
            return instruction != null && (instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call) &&
                operand != null && operand.Module == method.Module && operand.MetadataToken == method.MetadataToken;
        }
    }
}
