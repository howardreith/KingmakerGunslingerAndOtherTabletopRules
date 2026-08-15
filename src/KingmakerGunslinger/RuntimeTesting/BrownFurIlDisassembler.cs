using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurIlDisassembler
    {
        private static readonly Dictionary<byte, OpCode> OneByte = Build(false);
        private static readonly Dictionary<byte, OpCode> TwoByte = Build(true);

        internal static List<string> Describe(MethodBase method)
        {
            var result = new List<string>();
            MethodBody body = method == null ? null : method.GetMethodBody();
            byte[] bytes = body == null ? null : body.GetILAsByteArray();
            if (bytes == null) return result;
            int offset = 0;
            while (offset < bytes.Length)
            {
                int instructionOffset = offset;
                OpCode opcode;
                byte first = bytes[offset++];
                if (first == 0xfe)
                {
                    if (offset >= bytes.Length ||
                        !TwoByte.TryGetValue(bytes[offset++], out opcode))
                        throw new InvalidOperationException(
                            "Unknown two-byte IL opcode.");
                }
                else if (!OneByte.TryGetValue(first, out opcode))
                    throw new InvalidOperationException("Unknown IL opcode.");
                string operand = ReadOperand(method, bytes, ref offset,
                    opcode.OperandType);
                result.Add("IL_" + instructionOffset.ToString("x4",
                    CultureInfo.InvariantCulture) + ": " + opcode.Name +
                    (operand.Length == 0 ? string.Empty : " " + operand));
            }
            return result;
        }

        private static Dictionary<byte, OpCode> Build(bool twoByte)
        {
            var result = new Dictionary<byte, OpCode>();
            foreach (FieldInfo field in typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode)) continue;
                OpCode value = (OpCode)field.GetValue(null);
                ushort raw = unchecked((ushort)value.Value);
                bool isTwo = (raw & 0xff00) == 0xfe00;
                if (isTwo == twoByte) result[(byte)(raw & 0xff)] = value;
            }
            return result;
        }

        private static string ReadOperand(MethodBase method, byte[] bytes,
            ref int offset, OperandType type)
        {
            switch (type)
            {
                case OperandType.InlineNone: return string.Empty;
                case OperandType.ShortInlineI:
                    return unchecked((sbyte)bytes[offset++]).ToString(
                        CultureInfo.InvariantCulture);
                case OperandType.InlineI:
                    return ReadInt32(bytes, ref offset).ToString(
                        CultureInfo.InvariantCulture);
                case OperandType.InlineI8:
                    long longValue = BitConverter.ToInt64(bytes, offset);
                    offset += 8;
                    return longValue.ToString(CultureInfo.InvariantCulture);
                case OperandType.ShortInlineR:
                    float floatValue = BitConverter.ToSingle(bytes, offset);
                    offset += 4;
                    return floatValue.ToString("R", CultureInfo.InvariantCulture);
                case OperandType.InlineR:
                    double doubleValue = BitConverter.ToDouble(bytes, offset);
                    offset += 8;
                    return doubleValue.ToString("R",
                        CultureInfo.InvariantCulture);
                case OperandType.ShortInlineVar:
                    return "V_" + bytes[offset++].ToString(
                        CultureInfo.InvariantCulture);
                case OperandType.InlineVar:
                    ushort variable = BitConverter.ToUInt16(bytes, offset);
                    offset += 2;
                    return "V_" + variable.ToString(CultureInfo.InvariantCulture);
                case OperandType.ShortInlineBrTarget:
                    sbyte shortDelta = unchecked((sbyte)bytes[offset++]);
                    return "IL_" + (offset + shortDelta).ToString("x4",
                        CultureInfo.InvariantCulture);
                case OperandType.InlineBrTarget:
                    int delta = ReadInt32(bytes, ref offset);
                    return "IL_" + (offset + delta).ToString("x4",
                        CultureInfo.InvariantCulture);
                case OperandType.InlineSwitch:
                    int count = ReadInt32(bytes, ref offset);
                    int tableEnd = offset + (count * 4);
                    var targets = new string[count];
                    for (int index = 0; index < count; index++)
                    {
                        int switchDelta = ReadInt32(bytes, ref offset);
                        targets[index] = "IL_" + (tableEnd + switchDelta)
                            .ToString("x4", CultureInfo.InvariantCulture);
                    }
                    return string.Join(",", targets);
                case OperandType.InlineString:
                    int stringToken = ReadInt32(bytes, ref offset);
                    return "\"" + method.Module.ResolveString(stringToken)
                        .Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    int memberToken = ReadInt32(bytes, ref offset);
                    return DescribeMember(method.Module.ResolveMember(memberToken,
                        GenericTypes(method.DeclaringType),
                        GenericTypes(method)));
                case OperandType.InlineSig:
                    return "signature-token=0x" + ReadInt32(bytes, ref offset)
                        .ToString("x8", CultureInfo.InvariantCulture);
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

        private static string DescribeMember(MemberInfo member)
        {
            if (member == null) return "<unresolved-member>";
            string declaring = member.DeclaringType == null ? string.Empty :
                (member.DeclaringType.FullName ?? member.DeclaringType.Name) + ".";
            MethodBase method = member as MethodBase;
            if (method != null)
            {
                MethodInfo info = method as MethodInfo;
                string generic = info != null && info.IsGenericMethod ? "<" +
                    string.Join(",", info.GetGenericArguments().Select(value =>
                        value.FullName ?? value.Name).ToArray()) + ">" :
                    string.Empty;
                return declaring + method.Name + generic + "(" + string.Join(",",
                    method.GetParameters().Select(value =>
                        value.ParameterType.FullName ??
                        value.ParameterType.Name).ToArray()) + ")";
            }
            return declaring + member.Name;
        }
    }
}
