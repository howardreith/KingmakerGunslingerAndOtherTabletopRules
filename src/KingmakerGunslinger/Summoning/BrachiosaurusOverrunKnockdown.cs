using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed Greater Overrun rider for one creature's own command.
    ///
    /// <para><b>The printed contract.</b> A successful overrun moves you
    /// through the target's space; a result that beats the defender's CMD by
    /// <b>5 or more</b> also knocks it prone. Greater Overrun's rider is that
    /// an opponent <i>knocked prone by that overrun</i> provokes - not any
    /// success, and not a creature that was already lying down.</para>
    ///
    /// <para><b>Why this is not a second implementation of that threshold.</b>
    /// The engine already draws the line, and this reads its answer rather
    /// than recomputing one. <c>RuleCombatManeuver.SuccesValue</c> is
    /// <c>InitiatorCMValue - TargetCMD</c> - the real check total against the
    /// real effective defence - and <c>IsPartialSuccess</c> is
    /// <c>Success &amp;&amp; Type == Overrun &amp;&amp; SuccesValue &lt; 5</c>,
    /// against the engine's own <c>OverrunPartialSuccessThreshold</c> of 5.
    /// So "succeeded, but below the knockdown margin" is a native concept with
    /// a native constant, and a qualifying knockdown is exactly
    /// <c>Success &amp;&amp; !IsPartialSuccess</c>. No second roll is made and
    /// no margin is re-derived here.</para>
    ///
    /// <para><b>Why the knockdown is here and not in the ability's action
    /// list.</b> <c>AbilityCustomOverrun</c> runs its actions on <i>any</i>
    /// success - the IL branches on <c>!Success</c> alone - so an action list
    /// cannot tell an ordinary pass-through from a knockdown. The rule can,
    /// and this component sees the rule. Native RuleCombatManeuver already
    /// applies the threshold knockdown and raises IKnockOffHandler. Snapshot
    /// eligibility BEFORE it runs; its newly requested prone state afterward
    /// is not proof the target was already prone before this maneuver.</para>
    ///
    /// <para><b>Why the provocation is here and not the native component.</b>
    /// The native <c>ManeuverProvokeAttack</c> that the real Greater Overrun
    /// feat carries fires on <c>evt.Success</c> alone, which over-provokes
    /// against every ordinary pass-through and against a target that cannot be
    /// knocked off at all. This creature is granted a cloned carrier with that
    /// component removed, so there is exactly one provocation in play - this
    /// one - and nothing is stacked on top of the native behaviour or emitted
    /// and then undone. No other actor's feat is touched.</para>
    ///
    /// <para>Eligibility and prone state stay the engine's:
    /// <c>UnitHelper.CanBeKnockedOff</c> decides immunity,
    /// <c>ProneState.ShouldBeActive</c> is the native prone state that
    /// <c>UnitProneController</c> owns, and <c>IKnockOffHandler</c> is the
    /// native event the rest of the game listens to. A target already prone is
    /// not knocked down again and does not provoke, because no knockdown was
    /// caused.</para>
    /// </summary>
    public sealed class BrachiosaurusOverrunKnockdown :
        RuleInitiatorLogicComponent<RuleCombatManeuver>
    {
        [NonSerialized] private RuleCombatManeuver _pending;
        [NonSerialized] private bool _eligibleBefore;

        public override void OnEventAboutToTrigger(RuleCombatManeuver evt)
        {
            if (!BrachiosaurusNativeOverrun.Owns(evt)) return;
            _pending = evt;
            UnitEntityData target = evt.Target;
            _eligibleBefore = target?.Descriptor?.State?.Prone != null &&
                UnitHelper.CanBeKnockedOff(target) &&
                !target.Descriptor.State.Prone.Active &&
                !target.Descriptor.State.Prone.ShouldBeActive;
            if (target?.Descriptor?.State != null &&
                target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                evt.AutoFailure = true;
        }

        public override void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Overrun) return;
            if (!ReferenceEquals(_pending, evt)) return;
            bool eligibleBefore = _eligibleBefore;
            // Consume the exact native event before queuing any response.
            // Repeated notifications cannot provoke twice, and no growing
            // rule/target cache is retained by this per-fact component.
            _pending = null;
            _eligibleBefore = false;

            // The engine's own threshold. Success alone is a pass-through.
            if (!evt.Success || evt.IsPartialSuccess || !eligibleBefore) return;

            UnitEntityData target = evt.Target;
            if (target == null || target.Descriptor == null ||
                target.Descriptor.State == null ||
                target.Descriptor.State.Prone == null) return;

            // Native resolution must actually have requested/applied a new
            // knockdown. Do not perform or re-emit that behavior ourselves.
            if (!target.Descriptor.State.Prone.Active &&
                !target.Descriptor.State.Prone.ShouldBeActive) return;

            // One provocation, for the knockdown this overrun caused. How many
            // responders answer it, and whether each still has an attack of
            // opportunity left, is the engine's accounting.
            Game game = Game.Instance;
            if (game != null && game.CombatEngagementController != null)
                game.CombatEngagementController
                    .ProvokeAttackOfOpportunity(target);
        }
    }

    internal static class BrachiosaurusNativeOverrun
    {
        internal static bool Owns(UnitEntityData owner, BlueprintAbility ability)
        {
            ModContext context;
            return owner?.Blueprint?.AssetGuid == GiantBodyRulesPolicy.BrachiosaurusUnitGuid &&
                ability?.AssetGuid == GiantBodyRulesPolicy.BrachiosaurusOverrunGuid &&
                ModContext.TryGet(out context) && context.FeatureModules?.Active != null &&
                context.FeatureModules.Active.ExpandedSummoning;
        }

        internal static bool Owns(RuleCombatManeuver rule)
        {
            return rule != null && rule.Type == CombatManeuver.Overrun &&
                Owns(rule.Initiator, rule.Reason?.Context?.SourceAbility);
        }

        private static bool SkipContact(bool nativeDead, UnitEntityData target,
            AbilityExecutionContext context, UnitEntityData caster,
            HashSet<UnitEntityData> completed)
        {
            bool exact = Owns(caster, context?.SourceAbility);
            return GiantBodyRulesPolicy.SkipOrdinaryOverrunContact(exact,
                nativeDead, !exact || caster.IsEnemy(target), exact ? completed.Count : 0);
        }

        // The 2.1.7b FirstTargetOnly check is outside the inner unit loop.
        // Extend its existing dead-candidate predicate at the exact carrier
        // only: skip allies and further contacts within that same native tick.
        // All movement, rolls, threshold handling and native set ownership stay
        // in AbilityCustomOverrun; Trample and other owners keep native output.
        internal static IEnumerable<CodeInstruction> FilterContacts(
            IEnumerable<CodeInstruction> instructions)
        {
            var source = instructions.ToList();
            Type iterator = typeof(AbilityCustomOverrun).GetNestedType("<Deliver>d__9", BindingFlags.NonPublic);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo context = iterator?.GetField("context", flags);
            FieldInfo caster = iterator?.GetField("<caster>5__2", flags);
            FieldInfo completed = iterator?.GetField("<overrunUnits>5__7", flags);
            MethodInfo dead = typeof(UnitState).GetProperty("IsDead")?.GetGetMethod();
            int[] calls = Enumerable.Range(0, source.Count).Where(index =>
                (source[index].opcode == OpCodes.Callvirt || source[index].opcode == OpCodes.Call) &&
                Equals(source[index].operand, dead)).ToArray();
            if (context?.FieldType != typeof(AbilityExecutionContext) || caster?.FieldType != typeof(UnitEntityData) ||
                completed?.FieldType != typeof(HashSet<UnitEntityData>) || dead == null || calls.Length != 1 ||
                calls[0] < 3 || !source[calls[0] - 3].opcode.Name.StartsWith("ldloc", StringComparison.Ordinal) ||
                !Equals(source[calls[0] - 2].operand, typeof(UnitEntityData).GetProperty("Descriptor").GetGetMethod()) ||
                !Equals(source[calls[0] - 1].operand, typeof(UnitDescriptor).GetField("State")))
                throw new InvalidOperationException("Sprint25 exact native Overrun candidate seam does not match Kingmaker 2.1.7b.");
            CodeInstruction target = source[calls[0] - 3];
            source.InsertRange(calls[0] + 1, new[] {
                new CodeInstruction(target.opcode, target.operand),
                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, context),
                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, caster),
                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, completed),
                new CodeInstruction(OpCodes.Call, typeof(BrachiosaurusNativeOverrun).GetMethod(
                    nameof(SkipContact), BindingFlags.Static | BindingFlags.NonPublic)) });
            return source;
        }
    }

    [HarmonyPatch]
    internal static class BrachiosaurusOverrunContactPatch
    {
        private static MethodBase TargetMethod()
        {
            Type iterator = typeof(AbilityCustomOverrun).GetNestedType("<Deliver>d__9", BindingFlags.NonPublic);
            return iterator?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                throw new MissingMethodException("Kingmaker 2.1.7b native Overrun delivery iterator.");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        { return BrachiosaurusNativeOverrun.FilterContacts(instructions); }
    }

    // Native OnTrigger returns early on immunity/AutoFailure, leaving a zero
    // roll and defense. Stock Success then treats 0 >= 0 as a success. Correct
    // that blocked result only for this exact owner/ability/module, not globally.
    [HarmonyPatch(typeof(RuleCombatManeuver), "get_Success")]
    internal static class BrachiosaurusOverrunBlockedResultPatch
    {
        private static void Postfix(RuleCombatManeuver __instance, ref bool __result)
        {
            bool exact = BrachiosaurusNativeOverrun.Owns(__instance);
            if (GiantBodyRulesPolicy.BlocksOrdinaryOverrunOutcome(exact,
                __instance.AutoFailure, __instance.Target?.Descriptor?.State != null &&
                __instance.Target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers)))
                __result = false;
        }
    }
}
