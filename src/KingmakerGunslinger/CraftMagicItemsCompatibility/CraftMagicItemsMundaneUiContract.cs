using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsMundaneUiAnchor
    {
        internal CraftMagicItemsMundaneUiAnchor(MethodInfo target,
            int crafterLocalIndex, int selectedDataLocalIndex,
            int recipeDataLocalIndex, int ordinaryBodyOffset,
            int newItemBaseOffset, int footerOffset,
            MethodInfo labelRenderer)
        {
            Target = target;
            CrafterLocalIndex = crafterLocalIndex;
            SelectedDataLocalIndex = selectedDataLocalIndex;
            RecipeDataLocalIndex = recipeDataLocalIndex;
            OrdinaryBodyOffset = ordinaryBodyOffset;
            NewItemBaseOffset = newItemBaseOffset;
            FooterOffset = footerOffset;
            LabelRenderer = labelRenderer;
        }

        internal MethodInfo Target { get; private set; }
        internal int CrafterLocalIndex { get; private set; }
        internal int SelectedDataLocalIndex { get; private set; }
        internal int RecipeDataLocalIndex { get; private set; }
        internal int OrdinaryBodyOffset { get; private set; }
        internal int NewItemBaseOffset { get; private set; }
        internal int FooterOffset { get; private set; }
        internal MethodInfo LabelRenderer { get; private set; }

        internal string Identity
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "post-selected-crafting-data:ordinary=IL_{0:x4};" +
                    "new-item-bases=IL_{1:x4};footer=IL_{2:x4};" +
                    "locals=crafter:{3},selected:{4},recipe:{5}",
                    OrdinaryBodyOffset, NewItemBaseOffset, FooterOffset,
                    CrafterLocalIndex, SelectedDataLocalIndex,
                    RecipeDataLocalIndex);
            }
        }
    }

    internal sealed class CraftMagicItemsMundaneUiResolution
    {
        internal CraftMagicItemsMundaneUiResolution(
            CraftMagicItemsMundaneUiAnchor anchor, string failedCheck)
        {
            Anchor = anchor;
            FailedCheck = failedCheck ?? string.Empty;
        }

        internal CraftMagicItemsMundaneUiAnchor Anchor { get; private set; }
        internal string FailedCheck { get; private set; }
        internal bool IsCompatible { get { return Anchor != null; } }
    }

    internal enum CraftMagicItemsMundaneUiRoute
    {
        OrdinaryCmi = 0,
        AmmunitionLowerPanel = 1
    }

    internal static class CraftMagicItemsMundaneUiRoutePolicy
    {
        internal static CraftMagicItemsMundaneUiRoute Resolve(
            object selectedCraftingData, object ammunitionCraftingData)
        {
            return ammunitionCraftingData != null && ReferenceEquals(
                selectedCraftingData, ammunitionCraftingData) ?
                CraftMagicItemsMundaneUiRoute.AmmunitionLowerPanel :
                CraftMagicItemsMundaneUiRoute.OrdinaryCmi;
        }
    }

    internal static class CraftMagicItemsMundaneUiEventPolicy
    {
        internal static bool Is(string observed, string expected)
        {
            return string.Equals(observed, expected,
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldApplyPendingPhase(bool pending,
            string observed)
        {
            return pending && Is(observed, "Layout");
        }
    }

    internal sealed class CraftMagicItemsUiFailureCapture
    {
        internal CraftMagicItemsUiFailureCapture(Exception root,
            string exceptionChain)
        {
            Root = root;
            ExceptionChain = exceptionChain ?? string.Empty;
        }

        internal Exception Root { get; private set; }
        internal string ExceptionChain { get; private set; }
        internal bool RunOriginalRenderer { get { return false; } }
        internal bool RollbackSynchronously { get { return false; } }
        internal bool DeferDisableToSafeUpdate { get { return true; } }
        internal bool ExternalContractIncompatible { get { return false; } }
        internal bool Rethrow { get { return true; } }
    }

    internal static class CraftMagicItemsUiFailurePolicy
    {
        internal static CraftMagicItemsUiFailureCapture Capture(
            Exception exception)
        {
            Exception value = exception ?? new InvalidOperationException(
                "Unknown CMI ammunition UI failure.");
            Exception root = value;
            while (root is TargetInvocationException &&
                root.InnerException != null)
                root = root.InnerException;

            var result = new List<string>();
            var visited = new HashSet<Exception>();
            Exception current = value;
            int depth = 0;
            while (current != null && visited.Add(current))
            {
                result.Add("[" + depth.ToString(CultureInfo.InvariantCulture) +
                    "]type=" + current.GetType().FullName + ";message=" +
                    LogText(current.Message) + ";stack=" +
                    LogText(current.StackTrace));
                current = current.InnerException;
                depth++;
            }
            return new CraftMagicItemsUiFailureCapture(root,
                string.Join(" -> ", result.ToArray()));
        }

        private static string LogText(string value)
        {
            return (value ?? string.Empty).Replace("\r", string.Empty)
                .Replace("\n", "\\n");
        }
    }

    /// <summary>
    /// Capability probe for the one supported inner seam in CMI's mundane
    /// renderer. The bridge never patches this method unless every data-flow
    /// and footer anchor resolves exactly once.
    /// </summary>
    internal static class CraftMagicItemsMundaneUiContract
    {
        internal const string OuterSelectionLabel = "Mundane Crafting: ";
        internal const string FooterFormat = "Current Money: {0}";
        private static readonly Dictionary<byte, OpCode> OneByte =
            BuildOpcodes(false);
        private static readonly Dictionary<byte, OpCode> TwoByte =
            BuildOpcodes(true);

        internal static CraftMagicItemsMundaneUiResolution Probe(
            MethodInfo target, Type itemDataType, Type recipeBasedType,
            MethodInfo getSelectedCrafter)
        {
            if (target == null || itemDataType == null ||
                recipeBasedType == null || getSelectedCrafter == null)
                return Fail("mundane-ui-input");
            try
            {
                MethodBody body = target.GetMethodBody();
                if (body == null || body.ExceptionHandlingClauses.Count != 0)
                    return Fail("mundane-ui-method-body");
                List<IlInstruction> instructions = Read(target);
                if (instructions.Count == 0)
                    return Fail("mundane-ui-method-empty");

                IlInstruction[] outerLabels = instructions.Where(value =>
                    value.OpCode == OpCodes.Ldstr && string.Equals(
                        value.Operand as string, OuterSelectionLabel,
                        StringComparison.Ordinal)).ToArray();
                if (outerLabels.Length != 2)
                    return Fail("mundane-ui-outer-selector-label");

                IlInstruction[] candidates = instructions.Where(value =>
                    value.OpCode == OpCodes.Isinst && ReferenceEquals(
                        value.Operand, recipeBasedType)).ToArray();
                if (candidates.Length != 1)
                    return Fail("mundane-ui-selected-data-cast");
                int castIndex = instructions.IndexOf(candidates[0]);
                int selectedLoadIndex = PreviousMeaningful(instructions,
                    castIndex);
                int recipeStoreIndex = NextMeaningful(instructions,
                    castIndex);
                int recipeLoadIndex = NextMeaningful(instructions,
                    recipeStoreIndex);
                int recipeBranchIndex = NextMeaningful(instructions,
                    recipeLoadIndex);
                int selectedLocal = LocalIndex(
                    instructions[selectedLoadIndex], true);
                int recipeLocal = LocalIndex(
                    instructions[recipeStoreIndex], false);
                if (selectedLocal < 0 || recipeLocal < 0 ||
                    LocalIndex(instructions[recipeLoadIndex], true) !=
                        recipeLocal || !IsBranchTrue(
                            instructions[recipeBranchIndex].OpCode) ||
                    !(instructions[recipeBranchIndex].Operand is int))
                    return Fail("mundane-ui-inner-branch");
                int ordinaryOffset = (int)instructions[recipeBranchIndex]
                    .Operand;
                int ordinaryIndex = instructions.FindIndex(value =>
                    value.Offset == ordinaryOffset);
                if (ordinaryIndex <= recipeBranchIndex)
                    return Fail("mundane-ui-ordinary-body-target");

                IList<LocalVariableInfo> locals = body.LocalVariables;
                if (selectedLocal >= locals.Count || recipeLocal >=
                        locals.Count || locals[selectedLocal].LocalType !=
                        itemDataType || locals[recipeLocal].LocalType !=
                        recipeBasedType)
                    return Fail("mundane-ui-selected-data-locals");

                int[] selectedStores = instructions.Take(castIndex).Select(
                        (value, index) => new { value, index })
                    .Where(value => LocalIndex(value.value, false) ==
                        selectedLocal).Select(value => value.index).ToArray();
                if (selectedStores.Length != 2 ||
                    selectedStores[0] <= instructions.IndexOf(outerLabels[0]))
                    return Fail("mundane-ui-parent-finalization");
                MethodBase[] outerDraws = instructions.Skip(
                        instructions.IndexOf(outerLabels[0]))
                    .Take(selectedStores[0] -
                        instructions.IndexOf(outerLabels[0]) + 1)
                    .Where(IsCall).Select(value => value.Operand as MethodBase)
                    .Where(value => value != null && value.Name ==
                        "DrawSelectionUserInterfaceElements" &&
                        value.IsGenericMethod &&
                        value.GetParameters().Length == 5).ToArray();
                if (outerDraws.Length != 1)
                    return Fail("mundane-ui-outer-selector-call");

                IlInstruction[] crafterCalls = instructions.Take(castIndex)
                    .Where(value => IsCall(value) && SameMethod(
                        value.Operand as MethodBase,
                        getSelectedCrafter)).ToArray();
                if (crafterCalls.Length != 1)
                    return Fail("mundane-ui-crafter-call");
                int crafterCallIndex = instructions.IndexOf(crafterCalls[0]);
                int crafterStoreIndex = NextMeaningful(instructions,
                    crafterCallIndex);
                int crafterLocal = LocalIndex(
                    instructions[crafterStoreIndex], false);
                if (crafterLocal < 0 || crafterLocal >= locals.Count ||
                    locals[crafterLocal].LocalType !=
                        getSelectedCrafter.ReturnType)
                    return Fail("mundane-ui-crafter-local");

                IlInstruction[] footerLabels = instructions.Where(value =>
                    value.OpCode == OpCodes.Ldstr && string.Equals(
                        value.Operand as string, FooterFormat,
                        StringComparison.Ordinal)).ToArray();
                if (footerLabels.Length != 1 || footerLabels[0].Offset <=
                        ordinaryOffset)
                    return Fail("mundane-ui-common-footer");
                int footerIndex = instructions.IndexOf(footerLabels[0]);
                MethodInfo[] footerRenderers = instructions
                    .Skip(footerIndex + 1).Take(20)
                    .Where(value => IsCall(value))
                    .Select(value => value.Operand as MethodInfo)
                    .Where(value => value != null && value.IsStatic &&
                        value.ReturnType == typeof(void) &&
                        string.Equals(value.Name, "RenderLabelRow",
                            StringComparison.Ordinal) &&
                        value.GetParameters().Length == 1 &&
                        value.GetParameters()[0].ParameterType ==
                            typeof(string)).ToArray();
                if (footerRenderers.Length != 1)
                    return Fail("mundane-ui-footer-renderer");

                IlInstruction[] baseAccesses = instructions.Where(value =>
                    IsCall(value) && string.Equals(
                        (value.Operand as MethodBase)?.Name,
                        "get_NewItemBaseIDs", StringComparison.Ordinal))
                    .ToArray();
                if (baseAccesses.Length != 1 || baseAccesses[0].Offset <=
                        ordinaryOffset || baseAccesses[0].Offset >=
                        footerLabels[0].Offset)
                    return Fail("mundane-ui-new-item-base-access");
                int baseIndex = instructions.IndexOf(baseAccesses[0]);
                int baseReceiverIndex = PreviousMeaningful(instructions,
                    baseIndex);
                if (LocalIndex(instructions[baseReceiverIndex], true) !=
                        recipeLocal)
                    return Fail("mundane-ui-new-item-base-receiver");

                return new CraftMagicItemsMundaneUiResolution(
                    new CraftMagicItemsMundaneUiAnchor(target, crafterLocal,
                        selectedLocal, recipeLocal, ordinaryOffset,
                        baseAccesses[0].Offset, footerLabels[0].Offset,
                        footerRenderers[0]),
                    string.Empty);
            }
            catch (Exception exception)
            {
                return Fail("mundane-ui-probe-exception:" +
                    exception.GetType().FullName);
            }
        }

        private static bool SameMethod(MethodBase left, MethodBase right)
        {
            return left != null && right != null && left.Module ==
                right.Module && left.MetadataToken == right.MetadataToken;
        }

        private static bool IsCall(IlInstruction value)
        {
            return value.OpCode == OpCodes.Call ||
                value.OpCode == OpCodes.Callvirt;
        }

        private static bool IsBranchTrue(OpCode value)
        { return value == OpCodes.Brtrue || value == OpCodes.Brtrue_S; }

        private static int PreviousMeaningful(IList<IlInstruction> values,
            int index)
        {
            for (int current = index - 1; current >= 0; current--)
                if (values[current].OpCode != OpCodes.Nop) return current;
            return -1;
        }

        private static int NextMeaningful(IList<IlInstruction> values,
            int index)
        {
            for (int current = index + 1; current < values.Count; current++)
                if (values[current].OpCode != OpCodes.Nop) return current;
            return -1;
        }

        private static int LocalIndex(IlInstruction value, bool load)
        {
            if (value == null) return -1;
            if (load)
            {
                if (value.OpCode == OpCodes.Ldloc_0) return 0;
                if (value.OpCode == OpCodes.Ldloc_1) return 1;
                if (value.OpCode == OpCodes.Ldloc_2) return 2;
                if (value.OpCode == OpCodes.Ldloc_3) return 3;
                if (value.OpCode != OpCodes.Ldloc &&
                    value.OpCode != OpCodes.Ldloc_S) return -1;
            }
            else
            {
                if (value.OpCode == OpCodes.Stloc_0) return 0;
                if (value.OpCode == OpCodes.Stloc_1) return 1;
                if (value.OpCode == OpCodes.Stloc_2) return 2;
                if (value.OpCode == OpCodes.Stloc_3) return 3;
                if (value.OpCode != OpCodes.Stloc &&
                    value.OpCode != OpCodes.Stloc_S) return -1;
            }
            return value.Operand is int ? (int)value.Operand : -1;
        }

        private static List<IlInstruction> Read(MethodBase method)
        {
            byte[] bytes = method.GetMethodBody().GetILAsByteArray();
            var result = new List<IlInstruction>();
            int offset = 0;
            while (offset < bytes.Length)
            {
                int instructionOffset = offset;
                OpCode opcode;
                byte first = bytes[offset++];
                if (first == 0xfe)
                {
                    if (offset >= bytes.Length || !TwoByte.TryGetValue(
                            bytes[offset++], out opcode))
                        throw new InvalidOperationException(
                            "Unknown two-byte IL opcode.");
                }
                else if (!OneByte.TryGetValue(first, out opcode))
                    throw new InvalidOperationException("Unknown IL opcode.");
                object operand = ReadOperand(method, bytes, ref offset,
                    opcode.OperandType);
                result.Add(new IlInstruction(instructionOffset, opcode,
                    operand));
            }
            return result;
        }

        private static object ReadOperand(MethodBase method, byte[] bytes,
            ref int offset, OperandType type)
        {
            switch (type)
            {
                case OperandType.InlineNone: return null;
                case OperandType.ShortInlineI:
                    return unchecked((sbyte)bytes[offset++]);
                case OperandType.InlineI: return ReadInt32(bytes, ref offset);
                case OperandType.InlineI8:
                    long longValue = BitConverter.ToInt64(bytes, offset);
                    offset += 8;
                    return longValue;
                case OperandType.ShortInlineR:
                    float floatValue = BitConverter.ToSingle(bytes, offset);
                    offset += 4;
                    return floatValue;
                case OperandType.InlineR:
                    double doubleValue = BitConverter.ToDouble(bytes, offset);
                    offset += 8;
                    return doubleValue;
                case OperandType.ShortInlineVar: return (int)bytes[offset++];
                case OperandType.InlineVar:
                    int variable = BitConverter.ToUInt16(bytes, offset);
                    offset += 2;
                    return variable;
                case OperandType.ShortInlineBrTarget:
                    sbyte shortDelta = unchecked((sbyte)bytes[offset++]);
                    return offset + shortDelta;
                case OperandType.InlineBrTarget:
                    int delta = ReadInt32(bytes, ref offset);
                    return offset + delta;
                case OperandType.InlineSwitch:
                    int count = ReadInt32(bytes, ref offset);
                    int tableEnd = offset + count * 4;
                    var targets = new int[count];
                    for (int index = 0; index < count; index++)
                        targets[index] = tableEnd + ReadInt32(bytes,
                            ref offset);
                    return targets;
                case OperandType.InlineString:
                    return method.Module.ResolveString(ReadInt32(bytes,
                        ref offset));
                case OperandType.InlineField:
                    return method.Module.ResolveField(ReadInt32(bytes,
                        ref offset), GenericTypes(method.DeclaringType),
                        GenericTypes(method));
                case OperandType.InlineMethod:
                    return method.Module.ResolveMethod(ReadInt32(bytes,
                        ref offset), GenericTypes(method.DeclaringType),
                        GenericTypes(method));
                case OperandType.InlineType:
                    return method.Module.ResolveType(ReadInt32(bytes,
                        ref offset), GenericTypes(method.DeclaringType),
                        GenericTypes(method));
                case OperandType.InlineTok:
                    return method.Module.ResolveMember(ReadInt32(bytes,
                        ref offset), GenericTypes(method.DeclaringType),
                        GenericTypes(method));
                case OperandType.InlineSig:
                    return ReadInt32(bytes, ref offset);
                default:
                    throw new InvalidOperationException(
                        "Unsupported IL operand type: " + type);
            }
        }

        private static int ReadInt32(byte[] bytes, ref int offset)
        {
            int value = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            return value;
        }

        private static Type[] GenericTypes(MemberInfo value)
        {
            Type type = value as Type;
            MethodBase method = value as MethodBase;
            return type != null && type.IsGenericType ?
                type.GetGenericArguments() : method != null &&
                method.IsGenericMethod ? method.GetGenericArguments() : null;
        }

        private static Dictionary<byte, OpCode> BuildOpcodes(bool twoByte)
        {
            var result = new Dictionary<byte, OpCode>();
            foreach (FieldInfo field in typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode)) continue;
                OpCode value = (OpCode)field.GetValue(null);
                ushort raw = unchecked((ushort)value.Value);
                bool isTwo = (raw & 0xff00) == 0xfe00;
                if (isTwo == twoByte)
                    result[(byte)(raw & 0xff)] = value;
            }
            return result;
        }

        private static CraftMagicItemsMundaneUiResolution Fail(string check)
        { return new CraftMagicItemsMundaneUiResolution(null, check); }

        private sealed class IlInstruction
        {
            internal IlInstruction(int offset, OpCode opcode, object operand)
            {
                Offset = offset;
                OpCode = opcode;
                Operand = operand;
            }

            internal int Offset { get; private set; }
            internal OpCode OpCode { get; private set; }
            internal object Operand { get; private set; }
        }
    }
}
