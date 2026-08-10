using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker.RuleSystem.Rules.Damage;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    [HarmonyPatch]
    internal static class ShieldOtherDamagePatch
    {
        private const string TargetTypeName =
            "Kingmaker.RuleSystem.Rules.Damage.RuleDealDamage";
        private static MethodBase _target;

        private static bool Prepare()
        {
            return RuleEventPatchTarget.TryResolve(TargetTypeName,
                "Shield Other finalized hit-point damage splitting", out _target);
        }

        private static MethodBase TargetMethod()
        { return _target; }

        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> source = instructions.ToList();
            MethodInfo difficulty = typeof(RuleDealDamage).GetMethod(
                "ApplyDifficultyModifiers",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(int) }, null);
            MethodInfo damageSetter = typeof(RuleDealDamage).GetProperty("Damage")
                .GetSetMethod(true);
            MethodInfo lastHandledSetter = typeof(RuleDealDamage)
                .GetProperty("LastHandledDamage").GetSetMethod(true);
            MethodInfo callback = typeof(ShieldOtherRuntime).GetMethod(
                "AfterFinalDamage", BindingFlags.Static | BindingFlags.NonPublic);

            int difficultyCall = FindSingleCall(source, difficulty);
            int damageWrite = FindNextCall(source, difficultyCall + 1, damageSetter);
            int lastHandledWrite = FindNextCall(source, damageWrite + 1,
                lastHandledSetter);
            if (difficultyCall < 0 || damageWrite < 0 || lastHandledWrite < 0)
            {
                LogUnavailable("The exact ApplyDifficultyModifiers -> Damage -> LastHandledDamage seam was not found.");
                return source;
            }

            source.Insert(damageWrite + 1,
                new CodeInstruction(OpCodes.Ldarg_0, null));
            source.Insert(damageWrite + 2,
                new CodeInstruction(OpCodes.Call, callback));
            return source;
        }

        private static int FindSingleCall(List<CodeInstruction> source,
            MethodInfo method)
        {
            int result = -1;
            for (int index = 0; index < source.Count; index++)
            {
                if (!IsCall(source[index], method)) continue;
                if (result >= 0) return -1;
                result = index;
            }
            return result;
        }

        private static int FindNextCall(List<CodeInstruction> source, int start,
            MethodInfo method)
        {
            if (start <= 0 || method == null) return -1;
            for (int index = start; index < source.Count; index++)
                if (IsCall(source[index], method)) return index;
            return -1;
        }

        private static bool IsCall(CodeInstruction instruction, MethodInfo method)
        {
            return method != null &&
                (instruction.opcode == OpCodes.Call ||
                 instruction.opcode == OpCodes.Callvirt) &&
                Equals(instruction.operand, method);
        }

        private static void LogUnavailable(string reason)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Warning("shield-other", "damage-patch.skipped", reason);
        }
    }
}
