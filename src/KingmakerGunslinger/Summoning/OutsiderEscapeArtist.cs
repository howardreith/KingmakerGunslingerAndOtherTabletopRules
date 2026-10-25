using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.Summoning
{
    // Kingmaker's break-free consumer compresses Escape Artist into Mobility.
    // Replace only its rank/class contribution at that consumer. Native live
    // Dexterity, armor/condition/buff modifiers, dice and CMB choice remain.
    // Ordinary Mobility/Athletics checks and actor stats never change.
    [Serializable]
    public sealed class OutsiderEscapeArtist : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnit OwningBlueprint;
        public int Ranks;
        public bool ClassSkill;

        private int RankAdjustment
        {
            get
            {
                var mobility = Owner.Stats.SkillMobility as ModifiableValueSkill;
                if (mobility == null) throw new InvalidOperationException("Native break-free Mobility skill is unavailable.");
                return Ranks + (ClassSkill && Ranks > 0 ? 3 : 0) - mobility.BaseValue -
                    (mobility.ClassSkill && mobility.BaseValue > 0 ? 3 : 0);
            }
        }
        private static OutsiderEscapeArtist Find(UnitEntityData actor)
        {
            return actor?.Descriptor?.Progression?.Features?.Enumerable
                .SelectMany(feature => feature.SelectComponents<OutsiderEscapeArtist>())
                .SingleOrDefault(component => component.Fact != null && component.Fact.Active &&
                    ReferenceEquals(component.Owner?.Unit, actor) && ReferenceEquals(component.OwningBlueprint, actor.Blueprint));
        }
        internal static int BreakFreeValue(ModifiableValue value, UnitEntityData actor)
        {
            var profile = Find(actor);
            int native = value;
            return profile != null && ReferenceEquals(value, actor.Stats.SkillMobility)
                ? native + profile.RankAdjustment : native;
        }
        internal static RuleSkillCheck BreakFreeCheck(UnitEntityData actor, StatType stat, int dc)
        {
            var check = new RuleSkillCheck(actor, stat, dc);
            var profile = stat == StatType.SkillMobility ? Find(actor) : null;
            if (profile != null && profile.RankAdjustment != 0)
                check.Bonus.AddModifier(profile.RankAdjustment, profile, ModifierDescriptor.UntypedStackable);
            return check;
        }
    }

    // Keep the native algorithm/action callers. Its five value reads decide
    // between CMB and skills, then choose the actual native skill event.
    // No temporary stat mutation, thread scope, resource reset or party tick.
    [HarmonyPatch]
    internal static class OutsiderEscapeArtistBreakFreePatch
    {
        private const BindingFlags All = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static MethodBase TargetMethod()
        {
            return typeof(UnitHelper).GetMethod("TryBreakFree", All, null, new[] {
                typeof(UnitEntityData), typeof(UnitEntityData), typeof(UnitHelper.BreakFreeFlags),
                typeof(MechanicsContext), typeof(int?) }, null) ??
                throw new MissingMethodException("Native break-free contract is unavailable.");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var convert = typeof(ModifiableValue).GetMethod("op_Implicit", All, null,
                new[] { typeof(ModifiableValue) }, null);
            var constructor = typeof(RuleSkillCheck).GetConstructor(new[] {
                typeof(UnitEntityData), typeof(StatType), typeof(int) });
            var valueAdapter = typeof(OutsiderEscapeArtist).GetMethod("BreakFreeValue", All);
            var checkAdapter = typeof(OutsiderEscapeArtist).GetMethod("BreakFreeCheck", All);
            if (convert == null || convert.ReturnType != typeof(int) || constructor == null ||
                valueAdapter == null || checkAdapter == null)
                throw new InvalidOperationException("Unsupported native Escape Artist consumer contract.");
            var result = new List<CodeInstruction>(); int values = 0, checks = 0;
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && Equals(instruction.operand, convert))
                {
                    // The existing stack holds the native stat; append this
                    // exact native method's actor and consume both in the adapter.
                    instruction.opcode = OpCodes.Ldarg_0; instruction.operand = null;
                    result.Add(instruction); result.Add(new CodeInstruction(OpCodes.Call, valueAdapter));
                    values++; continue;
                }
                if (instruction.opcode == OpCodes.Newobj && Equals(instruction.operand, constructor))
                {
                    instruction.opcode = OpCodes.Call; instruction.operand = checkAdapter; checks++;
                }
                result.Add(instruction);
            }
            if (values != 5 || checks != 1)
                throw new InvalidOperationException("Native break-free callsites changed: values=" + values + ";checks=" + checks);
            return result;
        }
    }
}
