// Isolated CLR/Harmony regression for the production input wrapper's emitted IL.
// These synthetic methods do not qualify player input or firearm behavior in-game.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Harmony12;
public static class FirearmInputControlFlowTests
{
    static Assembly mod;
    public static int FinallyCalls;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool BooleanEntry(int mode)
    {
        if (mode == 0) return false;
        try {
            foreach (int value in new List<int> { 1, 2 })
            {
                if (mode == value) return true;
                if (mode < 0) throw new InvalidOperationException("fixture-input-fault");
            }
            return false;
        } finally { FinallyCalls++; }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void VoidEntry(int mode)
    {
        if (mode == 0) return;
        try { if (mode < 0) throw new InvalidOperationException("fixture-input-fault"); }
        finally { FinallyCalls++; }
    }
    public static IEnumerable<CodeInstruction> Wrap(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator, MethodBase original)
    {
        Type adapter = mod.GetType("KingmakerGunslinger.Firing.NativeFirearmAttackOrderPatch", true);
        Type invocation = mod.GetType("KingmakerGunslinger.Firing.NativeFirearmAttackOrder+InputInvocation", true);
        return (IEnumerable<CodeInstruction>)adapter.GetMethod("GuardInput", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { instructions, generator.DeclareLocal(invocation), generator, ((MethodInfo)original).ReturnType });
    }
    public static void Run(Assembly assembly)
    {
        mod = assembly;
        var harmony = HarmonyInstance.Create("KMG.Hotfix.InputIlProbe");
        var transpiler = new HarmonyMethod(typeof(FirearmInputControlFlowTests).GetMethod("Wrap"));
        foreach (string name in new[] { "BooleanEntry", "VoidEntry" })
        {
            MethodInfo target = typeof(FirearmInputControlFlowTests).GetMethod(name);
            harmony.Patch(target, null, null, transpiler);
            foreach (int mode in new[] { 0, 1, 2, 3, -1 })
            {
                int before = FinallyCalls;
                try {
                    object result = target.Invoke(null, new object[] { mode });
                    if (mode < 0) throw new Exception("The native exception was swallowed.");
                    if (name == "BooleanEntry" && (bool)result != (mode == 1 || mode == 2))
                        throw new Exception("Boolean return changed.");
                } catch (TargetInvocationException exception) {
                    if (mode >= 0 || !(exception.InnerException is InvalidOperationException) ||
                        exception.InnerException.Message != "fixture-input-fault") throw;
                }
                if (FinallyCalls - before != (mode == 0 ? 0 : 1))
                    throw new Exception("Nested native finally behavior changed.");
                Console.WriteLine("PASS " + name + " mode=" + mode + " return/exception/finally");
            }
        }
        Console.WriteLine("Input wrapper IL behavior: 10 cases PASS. Synthetic managed control-flow proof only; no game/input qualification.");
    }
}