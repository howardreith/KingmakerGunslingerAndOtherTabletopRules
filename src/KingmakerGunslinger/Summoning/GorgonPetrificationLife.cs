using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    // Kingmaker's native life controller treats Petrified as lethal damage.
    // Only this one predicate changes for a sole, initialized temporary Gorgon
    // condition. Health, negative levels, ForceKill, regeneration and all other
    // petrification keep the exact original method and branches.
    [HarmonyPatch(typeof(UnitLifeController), "TickOnUnit")]
    internal static class GorgonPetrificationLife
    {
        private static readonly FieldInfo Conditions = typeof(UnitState).GetField(
            "m_Conditions", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo HasCondition = typeof(UnitState).GetMethod(
            "HasCondition", new[] { typeof(UnitCondition) });
        private static readonly MethodInfo ScopedPredicate = typeof(GorgonPetrificationLife)
            .GetMethod("ForLifeController", BindingFlags.Static | BindingFlags.NonPublic);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (Conditions == null || Conditions.FieldType != typeof(sbyte[]) ||
                HasCondition == null || ScopedPredicate == null)
                throw new InvalidOperationException("Unsupported native Gorgon life/condition contract.");
            var source = instructions.ToList();
            var result = new List<CodeInstruction>();
            int matches = 0;
            for (int i = 0; i < source.Count; i++)
            {
                var call = source[i];
                bool petrifiedConstant = i > 0 &&
                    (source[i - 1].opcode == OpCodes.Ldc_I4_8 ||
                     (source[i - 1].opcode == OpCodes.Ldc_I4 || source[i - 1].opcode == OpCodes.Ldc_I4_S) &&
                        Convert.ToInt32(source[i - 1].operand) == (int)UnitCondition.Petrified);
                if (petrifiedConstant && call.opcode == OpCodes.Callvirt && Equals(call.operand, HasCondition))
                {
                    var unit = new CodeInstruction(OpCodes.Ldarg_1);
                    unit.labels.AddRange(call.labels);
                    call.labels.Clear();
                    result.Add(unit);
                    call.opcode = OpCodes.Call;
                    call.operand = ScopedPredicate;
                    matches++;
                }
                result.Add(call);
            }
            if (matches != 1)
                throw new InvalidOperationException("Expected one native Petrified death predicate, observed " + matches + ".");
            return result;
        }

        internal static int ConditionCount(UnitState state)
        {
            var values = Conditions == null ? null : Conditions.GetValue(state) as sbyte[];
            if (values == null || values.Length <= (int)UnitCondition.Petrified)
                throw new InvalidOperationException("Native condition counter is unavailable.");
            return values[(int)UnitCondition.Petrified];
        }

        private static bool ForLifeController(UnitState state, UnitCondition condition, UnitEntityData unit)
        {
            bool native = state.HasCondition(condition);
            if (!native || condition != UnitCondition.Petrified || unit == null || unit.Descriptor == null ||
                !ReferenceEquals(unit.Descriptor.State, state) || unit.Buffs == null) return native;
            var own = unit.Buffs.Enumerable.FirstOrDefault(value => value != null &&
                value.Blueprint.AssetGuid == Sprint26MagicalBeastRules.PetrificationGuid);
            if (own == null) return native;
            var stages = own.SelectComponents<GorgonPetrificationState>().ToArray();
            if (stages.Length != 1) return native;
            return !Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(native,
                ConditionCount(state), stages[0].Initialized, stages[0].Permanent);
        }
    }
}
