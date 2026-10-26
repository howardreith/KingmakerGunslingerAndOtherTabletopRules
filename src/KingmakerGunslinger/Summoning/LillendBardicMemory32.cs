using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    // Owner-approved exception: this Lillend's bardic Suggestion does not
    // turn its own fascinated subject into a combatant before Will/SR. The
    // ability context belongs to this invocation; no ambient flag, ledger,
    // command replacement or save state crosses an event/reentrancy boundary.
    [HarmonyPatch(typeof(OffensiveActionsController), "OnTryToApplyAbilityEffect",
        new[] { typeof(AbilityExecutionContext), typeof(TargetWrapper) })]
    internal static class LillendBardicMemory32
    {
        private static readonly MethodInfo NativeRaise = AccessTools.Method(
            typeof(OffensiveActionsController), "RaiseEvents",
            new[] { typeof(UnitEntityData), typeof(UnitEntityData) });
        private static readonly Action<UnitEntityData, UnitEntityData> OriginalRaise =
            NativeRaise == null ? null : (Action<UnitEntityData, UnitEntityData>)Delegate.CreateDelegate(
                typeof(Action<UnitEntityData, UnitEntityData>), NativeRaise);

        private static bool BodyMatches(MethodInfo method, string expected)
        {
            if (method?.GetMethodBody() == null ||
                method.Module.ModuleVersionId.ToString() != "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7") return false;
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(method.GetMethodBody().GetILAsByteArray()))
                    .Replace("-", "").ToLowerInvariant() == expected;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // These are the inspected 2.1.7b contracts: exactly global then
            // actor-local dispatch, and an aggro-script notification followed
            // by only hostile-memory/surprise/faction bookkeeping. Refuse a
            // changed engine instead of silently suppressing another behavior.
            if (OriginalRaise == null ||
                !BodyMatches(NativeRaise, "082655a57fa0c5b6b610c6f151c8545b44331f388dea1031fbed7a40ead1820d") ||
                !BodyMatches(AccessTools.Method(typeof(UnitMemoryController), "HandleUnitMakeOffensiveAction",
                    new[] { typeof(UnitEntityData), typeof(UnitEntityData) }),
                    "507f9537fbf69f955223b02c50f4455f12f20954b34062c2dd9aad5e06fa6a81"))
                throw new InvalidOperationException("Unsupported native Lillend bardic offensive-event/memory contract.");
            var source = instructions.ToList();
            if (source.Count(value => value.opcode == OpCodes.Call && Equals(value.operand, NativeRaise)) != 1)
                throw new InvalidOperationException("Expected one native ability offensive-event call for Lillend bardic context.");
            var bridge = AccessTools.Method(typeof(LillendBardicMemory32), nameof(RaiseForAbility));
            foreach (var instruction in source)
            {
                if (instruction.opcode != OpCodes.Call || !Equals(instruction.operand, NativeRaise))
                { yield return instruction; continue; }
                if (instruction.blocks.Count != 0)
                    throw new InvalidOperationException("Unexpected native offensive-event exception boundary.");
                var context = new CodeInstruction(OpCodes.Ldarg_1);
                context.labels.AddRange(instruction.labels);
                yield return context;
                yield return new CodeInstruction(OpCodes.Call, bridge);
            }
        }

        internal static bool Qualifies(AbilityExecutionContext context, UnitEntityData caster, UnitEntityData target)
        {
            if (!Sprint32Suggestion.Active || context == null || caster == null || target == null ||
                !ReferenceEquals(context.Caster, caster)) return false;
            var dispatcher = LillendNoncombat32.Find(caster);
            if (dispatcher?.BardicSuggestion == null ||
                !ReferenceEquals(context.SourceAbility, dispatcher.BardicSuggestion) ||
                !ReferenceEquals(context.Ability?.Blueprint, dispatcher.BardicSuggestion) ||
                !LillendFascinateEligibility32.Attentive(caster, target)) return false;
            var check = dispatcher.BardicSuggestion.GetComponent<LillendBardicSuggestionTarget32>();
            if (check == null || !ReferenceEquals(check.Ability, dispatcher.BardicSuggestion)) return false;
            var state = caster.Buffs.Enumerable.Where(value => value.Active && ReferenceEquals(value.Blueprint, check.Carrier))
                .SelectMany(value => value.SelectComponents<LillendFascinateState32>()).SingleOrDefault();
            // Do not inspect or predict the eventual save/SR outcome. A failed
            // suggestion is still this performance's legal bardic attempt.
            return state != null && ReferenceEquals(state.BardicSuggestion, dispatcher.BardicSuggestion) && state.Owns(target);
        }

        private static void RaiseForAbility(UnitEntityData caster, UnitEntityData target, AbilityExecutionContext context)
        {
            if (!Qualifies(context, caster, target)) { OriginalRaise(caster, target); return; }
            EventBus.RaiseEvent<IUnitMakeOffensiveActionGlobalHandler>(handler =>
            {
                // Recheck at the actual listener boundary: an earlier native
                // reaction can legitimately end this performance. Aggro scripts
                // still receive their original notification and may cause combat.
                if (handler.GetType() == typeof(UnitMemoryController) && Qualifies(context, caster, target))
                    target.Blueprint.GetComponent<UnitAggroFilter>()?.OnAggroAction(target, caster);
                else handler.HandleUnitMakeOffensiveAction(caster, target);
            });
            // Ordinary invisibility, stealth and all actor-local reactions keep
            // the original actor-specific dispatch and ordering.
            EventBus.RaiseEvent<IUnitMakeOffensiveActionHandler>(caster,
                handler => handler.HandleUnitMakeOffensiveAction(target));
        }
    }
}
