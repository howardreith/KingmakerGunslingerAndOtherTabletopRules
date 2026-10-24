using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;

namespace KingmakerGunslinger.Summoning
{
    [HarmonyPatch(typeof(ActionBarGroupElement),"FillSlots")]
    internal static class ElementalSourceChooserDesktopPatch
    {
        private static bool Prepare()
        {
            MethodInfo method=typeof(ActionBarGroupElement).GetMethod("FillSlots",BindingFlags.Instance|BindingFlags.NonPublic);
            return typeof(ActionBarGroupElement).Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid&&
                method!=null&&method.GetMethodBody().GetILAsByteArray().Length==1033&&ElementalSourceChooserRuntime.SelectedField!=null;
        }
        private static void Prefix(ActionBarGroupElement __instance)
        {try{ElementalSourceChooserRuntime.Begin(__instance);}catch(Exception error){ElementalSourceChooserRuntime.Outcome="preserve-native-desktop-intake:"+error.GetType().Name;}}
        private static void Postfix(ActionBarGroupElement __instance){ElementalSourceChooserRuntime.End(__instance);}
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original)
        {
            var instructions=original.ToList();
            var native=typeof(ActionBarSubGroupLevels).GetMethod("AddSlot",new[]{typeof(int),typeof(MechanicActionBarSlot)});
            var helper=typeof(ElementalSourceChooserRuntime).GetMethod("AddDesktopSlot",BindingFlags.Static|BindingFlags.NonPublic);
            int count=instructions.Count(value=>(value.opcode==OpCodes.Call||value.opcode==OpCodes.Callvirt)&&Equals(value.operand,native));
            if(count!=3)throw new InvalidOperationException("Exact desktop spell row insertion contract changed.");
            foreach(var instruction in instructions)
            {
                if((instruction.opcode==OpCodes.Call||instruction.opcode==OpCodes.Callvirt)&&Equals(instruction.operand,native))
                {
                    if(instruction.blocks.Count!=0)throw new InvalidOperationException("Unexpected spell row exception boundary.");
                    var context=new CodeInstruction(OpCodes.Ldarg_0);context.labels.AddRange(instruction.labels);
                    yield return context;yield return new CodeInstruction(OpCodes.Call,helper);
                }
                else yield return instruction;
            }
        }
    }

    // Native drag/drop and automatic insertion converge on this exact save
    // boundary. Only our transient navigation mechanisms are refused; native
    // source/converted mechanisms, null and foreign mechanisms retain native
    // SetSlot semantics. This remains active when the module is switched off,
    // so an old transient drag can never enter a serialized action-bar array.
    [HarmonyPatch(typeof(UnitUISettings),"SetSlot",new[]{typeof(MechanicActionBarSlot),typeof(int)})]
    internal static class ElementalChooserPersistencePatch
    {
        internal static int RefusedPins;
        private static bool Prepare()
        {
            var method=typeof(UnitUISettings).GetMethod("SetSlot",new[]{typeof(MechanicActionBarSlot),typeof(int)});
            return typeof(UnitUISettings).Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid &&
                method!=null && method.MetadataToken==0x06002fff && method.GetMethodBody().GetILAsByteArray().Length==10;
        }
        private static bool Prefix(MechanicActionBarSlot slot)
        {
            if(!(slot is ElementalChooserLauncher) && !(slot is ElementalConsoleNode))return true;
            RefusedPins++;ElementalSourceChooserRuntime.Outcome="transient-elementals-navigation-pin-refused-no-serialized-slot";
            return false;
        }
    }
}
