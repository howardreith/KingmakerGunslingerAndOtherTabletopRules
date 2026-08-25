using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    /// <summary>
    /// Harmony 2 converts its CodeInstruction graph to the Harmony 1.2 shape
    /// already referenced by KMG, keeping the external mod completely late
    /// bound. This transpiler changes no ordinary CMI instruction.
    /// </summary>
    internal static class CraftMagicItemsMundaneUiTranspiler
    {
        private static readonly object Gate = new object();
        private static int _applicationCount;
        private static string _appliedSeam = string.Empty;

        internal static int ApplicationCount
        { get { lock (Gate) return _applicationCount; } }

        internal static string AppliedSeam
        { get { lock (Gate) return _appliedSeam; } }

        internal static IEnumerable<CodeInstruction> Transpile(
            IEnumerable<CodeInstruction> source, ILGenerator generator,
            CraftMagicItemsMundaneUiAnchor anchor, Type recipeBasedType,
            MethodInfo getSelectedCrafter, MethodInfo callback)
        {
            if (source == null || generator == null || anchor == null ||
                recipeBasedType == null || getSelectedCrafter == null ||
                callback == null || callback.ReturnType != typeof(bool) ||
                callback.GetParameters().Length != 2)
                throw new InvalidOperationException(
                    "CMI mundane UI transpiler inputs are incomplete.");
            var values = source.ToList();
            if (values.Count == 0)
                throw new InvalidOperationException(
                    "CMI mundane UI transpiler received no instructions.");
            if (values.Count(value => IsCallTo(value, callback)) != 0)
                throw new InvalidOperationException(
                    "CMI mundane UI inner seam was already injected.");

            int castIndex = SingleIndex(values, value =>
                value.opcode == OpCodes.Isinst && ReferenceEquals(
                    value.operand, recipeBasedType),
                "selected-data-cast");
            int selectedLoadIndex = PreviousMeaningful(values, castIndex);
            int recipeStoreIndex = NextMeaningful(values, castIndex);
            int recipeLoadIndex = NextMeaningful(values, recipeStoreIndex);
            int recipeBranchIndex = NextMeaningful(values, recipeLoadIndex);
            if (LocalIndex(values[selectedLoadIndex], true) !=
                    anchor.SelectedDataLocalIndex ||
                LocalIndex(values[recipeStoreIndex], false) !=
                    anchor.RecipeDataLocalIndex ||
                LocalIndex(values[recipeLoadIndex], true) !=
                    anchor.RecipeDataLocalIndex ||
                !IsBranchTrue(values[recipeBranchIndex].opcode) ||
                !(values[recipeBranchIndex].operand is Label))
                throw Changed("inner-branch");

            Label entryLabel = (Label)values[recipeBranchIndex].operand;
            int ordinaryIndex = SingleIndex(values, value =>
                value.labels.Contains(entryLabel), "ordinary-body-target");
            if (ordinaryIndex <= recipeBranchIndex)
                throw Changed("ordinary-body-order");

            int baseIndex = SingleIndex(values, value => IsCallNamed(value,
                "get_NewItemBaseIDs"), "new-item-base-access");
            if (baseIndex <= ordinaryIndex || LocalIndex(values[
                    PreviousMeaningful(values, baseIndex)], true) !=
                    anchor.RecipeDataLocalIndex)
                throw Changed("new-item-base-receiver");

            int footerIndex = SingleIndex(values, value => value.opcode ==
                OpCodes.Ldstr && string.Equals(value.operand as string,
                    CraftMagicItemsMundaneUiContract.FooterFormat,
                    StringComparison.Ordinal), "common-footer");
            if (footerIndex <= baseIndex || !values.Skip(footerIndex + 1)
                    .Take(20).Any(value => IsCallNamed(value,
                        "RenderLabelRow")))
                throw Changed("footer-renderer");

            int crafterCallIndex = SingleIndex(values.Take(castIndex).ToList(),
                value => IsCallTo(value, getSelectedCrafter),
                "crafter-call");
            int crafterStoreIndex = NextMeaningful(values, crafterCallIndex);
            if (LocalIndex(values[crafterStoreIndex], false) !=
                    anchor.CrafterLocalIndex)
                throw Changed("crafter-local");
            CodeInstruction crafterLoad = values.FirstOrDefault(value =>
                LocalIndex(value, true) == anchor.CrafterLocalIndex);
            if (crafterLoad == null) throw Changed("crafter-load");

            CodeInstruction ordinary = values[ordinaryIndex];
            CodeInstruction footer = values[footerIndex];
            if (ordinary.blocks.Count != 0 || footer.blocks.Count != 0)
                throw Changed("exception-block-boundary");

            Label continueOrdinary = generator.DefineLabel();
            Label commonFooter = generator.DefineLabel();
            var injected = new List<CodeInstruction>
            {
                Bare(crafterLoad),
                Bare(values[selectedLoadIndex]),
                new CodeInstruction(OpCodes.Call, callback),
                new CodeInstruction(OpCodes.Brfalse, continueOrdinary),
                new CodeInstruction(OpCodes.Br, commonFooter)
            };
            injected[0].labels.AddRange(ordinary.labels);
            ordinary.labels.Clear();
            ordinary.labels.Add(continueOrdinary);
            footer.labels.Add(commonFooter);
            values.InsertRange(ordinaryIndex, injected);

            lock (Gate)
            {
                _applicationCount++;
                _appliedSeam = anchor.Identity;
            }
            return values;
        }

        private static CodeInstruction Bare(CodeInstruction source)
        { return new CodeInstruction(source.opcode, source.operand); }

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

        private static int SingleIndex(IList<CodeInstruction> values,
            Func<CodeInstruction, bool> predicate, string check)
        {
            int[] matches = values.Select((value, index) => new {
                    value, index })
                .Where(value => predicate(value.value))
                .Select(value => value.index).ToArray();
            if (matches.Length != 1) throw Changed(check + ":count=" +
                matches.Length);
            return matches[0];
        }

        private static int PreviousMeaningful(
            IList<CodeInstruction> values, int index)
        {
            for (int current = index - 1; current >= 0; current--)
                if (values[current].opcode != OpCodes.Nop) return current;
            throw Changed("previous-instruction");
        }

        private static int NextMeaningful(IList<CodeInstruction> values,
            int index)
        {
            for (int current = index + 1; current < values.Count; current++)
                if (values[current].opcode != OpCodes.Nop) return current;
            throw Changed("next-instruction");
        }

        private static bool IsBranchTrue(OpCode value)
        { return value == OpCodes.Brtrue || value == OpCodes.Brtrue_S; }

        private static int LocalIndex(CodeInstruction value, bool load)
        {
            if (value == null) return -1;
            if (load)
            {
                if (value.opcode == OpCodes.Ldloc_0) return 0;
                if (value.opcode == OpCodes.Ldloc_1) return 1;
                if (value.opcode == OpCodes.Ldloc_2) return 2;
                if (value.opcode == OpCodes.Ldloc_3) return 3;
                if (value.opcode != OpCodes.Ldloc &&
                    value.opcode != OpCodes.Ldloc_S) return -1;
            }
            else
            {
                if (value.opcode == OpCodes.Stloc_0) return 0;
                if (value.opcode == OpCodes.Stloc_1) return 1;
                if (value.opcode == OpCodes.Stloc_2) return 2;
                if (value.opcode == OpCodes.Stloc_3) return 3;
                if (value.opcode != OpCodes.Stloc &&
                    value.opcode != OpCodes.Stloc_S) return -1;
            }
            LocalBuilder local = value.operand as LocalBuilder;
            if (local != null) return local.LocalIndex;
            if (value.operand is byte) return (byte)value.operand;
            if (value.operand is sbyte) return (sbyte)value.operand;
            if (value.operand is ushort) return (ushort)value.operand;
            if (value.operand is int) return (int)value.operand;
            return -1;
        }

        private static InvalidOperationException Changed(string check)
        {
            return new InvalidOperationException(
                "CMI mundane UI transpiler shape changed: " + check);
        }
    }
}
