using System;
using System.Globalization;
using System.Threading;
using Harmony12;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Acadamae
{
    internal static class AcadamaeCastingRuntime
    {
        private static readonly AcadamaeInvocationTracker<UnitUseAbility, AbilityData,
            RuleCastSpell> Invocations =
                new AcadamaeInvocationTracker<UnitUseAbility, AbilityData,
                    RuleCastSpell>();
        [System.ThreadStatic] private static bool _inspectPreAcadamae;
        private static BlueprintBuff _fatigued;
        private static int _completedCount;
        private static int _lastDifficultyClass;
        private static bool _lastSavePassed;
        private static int _lastNaturalRoll;
        private static int _lastFortitudeModifier;
        private static int _lastSaveTotal;
        private static string _lastFatigueDisposition;
        private static string _lastResolutionMessage;
        private static long _publishedResolutionCount;

        internal static void Configure(BlueprintBuff fatigued)
        { _fatigued = fatigued; }
        internal static int CompletedCount { get { return _completedCount; } }
        internal static int LastDifficultyClass { get { return _lastDifficultyClass; } }
        internal static bool LastSavePassed { get { return _lastSavePassed; } }
        internal static int LastNaturalRoll { get { return _lastNaturalRoll; } }
        internal static int LastFortitudeModifier
        { get { return _lastFortitudeModifier; } }
        internal static int LastSaveTotal { get { return _lastSaveTotal; } }
        internal static string LastFatigueDisposition
        { get { return _lastFatigueDisposition; } }
        internal static string LastResolutionMessage
        { get { return _lastResolutionMessage; } }
        internal static long PublishedResolutionCount
        { get { return Interlocked.Read(ref _publishedResolutionCount); } }
        internal static void ResetDiagnostics()
        {
            _completedCount = 0;
            _lastDifficultyClass = 0;
            _lastSavePassed = false;
            _lastNaturalRoll = 0;
            _lastFortitudeModifier = 0;
            _lastSaveTotal = 0;
            _lastFatigueDisposition = null;
            _lastResolutionMessage = null;
            Interlocked.Exchange(ref _publishedResolutionCount, 0);
        }

        internal static bool IsEligible(AbilityData ability, bool longerThanStandard)
        {
            if (ability == null || ability.Caster == null ||
                ability.Caster.Progression == null || ability.Blueprint == null ||
                ability.SpellLevel < 0 || ability.SpellLevel > 10)
                return false;
            var spellbook = ability.Spellbook;
            bool preparedInvocation = IsPreparedInvocation(ability, spellbook);
            AcadamaeCastDecision decision = AcadamaeCastingPolicy.Decide(
                new AcadamaeCastRequest {
                    HasFeat = BlueprintBootstrap.AcadamaeGraduate != null &&
                        ability.Caster.Progression.Features.GetRank(
                            BlueprintBootstrap.AcadamaeGraduate) > 0,
                    AccelerationModeActive = BlueprintBootstrap.AcadamaeGraduateMode != null &&
                        ability.Caster.Buffs.GetBuff(
                            BlueprintBootstrap.AcadamaeGraduateMode.Marker) != null,
                    IsRealSpell = ability.Blueprint.IsSpell,
                    HasSpellbook = spellbook != null,
                    IsPreparedInvocation = preparedInvocation,
                    IsArcane = spellbook != null && spellbook.Blueprint.IsArcane,
                    IsConjuration = ability.Blueprint.School == SpellSchool.Conjuration,
                    IsSummoning = (ability.Blueprint.SpellDescriptor &
                        SpellDescriptor.Summoning) != 0,
                    EffectiveCastingTime = longerThanStandard ?
                        AcadamaeCastingTime.FullRound : AcadamaeCastingTime.Standard,
                    EffectiveRounds = 1,
                    SpellLevel = ability.SpellLevel
                });
            return decision.Eligible;
        }

        internal static bool IsPreparedInvocation(AbilityData ability,
            Kingmaker.UnitLogic.Spellbook spellbook)
        {
            if (ability == null || spellbook == null ||
                spellbook.Blueprint.Spontaneous) return false;
            if (spellbook.CanSpend(ability, false)) return true;
            Kingmaker.UnitLogic.SpellSlot slot = ability.ParamSpellSlot;
            if (slot == null || !slot.Available || slot.Spell == null ||
                !ReferenceEquals(slot.Spell.Spellbook, spellbook)) return false;
            for (AbilityData current = ability; current != null;
                current = current.ConvertedFrom)
                if (ReferenceEquals(current, slot.Spell)) return true;
            return false;
        }

        internal static bool InspectPreAcadamae(AbilityData ability)
        {
            try { _inspectPreAcadamae = true; return ability.RequireFullRoundAction; }
            finally { _inspectPreAcadamae = false; }
        }

        internal static bool IsInspecting { get { return _inspectPreAcadamae; } }
        internal static void Arm(UnitUseAbility command, AbilityData ability)
        { if (IsEligible(ability, InspectPreAcadamae(ability))) Invocations.Arm(command, ability); }
        internal static void Begin(UnitUseAbility command) { Invocations.Begin(command); }
        internal static void End(UnitUseAbility command) { Invocations.EndAction(command); }
        internal static void Cancel(UnitUseAbility command) { Invocations.Cancel(command); }
        internal static void AttachRule(RuleCastSpell rule)
        {
            if (rule != null) Invocations.AttachRule(rule, rule.Spell);
        }
        internal static bool Complete(RuleCastSpell rule)
        {
            if (rule == null || !Invocations.Consume(rule, rule.Spell)) return false;
            if (!rule.Success || _fatigued == null) return false;
            var saving = new RuleSavingThrow(rule.Initiator,
                SavingThrowType.Fortitude, 15 + rule.Spell.SpellLevel);
            AcadamaeSavingThrowTestControl.Begin(saving);
            try { Rulebook.Trigger(saving); }
            finally { AcadamaeSavingThrowTestControl.End(); }
            _completedCount++;
            _lastDifficultyClass = saving.DifficultyClass;
            _lastSavePassed = saving.IsPassed;
            _lastNaturalRoll = saving.D20.Value;
            _lastFortitudeModifier = saving.StatValue;
            _lastSaveTotal = saving.RollResult;
            _lastFatigueDisposition = "none-save-passed";
            if (!saving.IsPassed)
            {
                var fatigue = rule.Initiator.Descriptor.Buffs.AddBuff(
                    _fatigued, rule.Initiator, null);
                // AddBuff's null duration initially normalizes to the current
                // game time. The native permanent transition clears that end
                // time while retaining the independent caster context and the
                // ordinary RemoveOnRest blueprint lifecycle.
                if (fatigue != null)
                {
                    fatigue.MakePermanent();
                    _lastFatigueDisposition = fatigue.IsPermanent ?
                        "fatigued-permanent" : "fatigued-not-permanent";
                }
                else
                    _lastFatigueDisposition = "fatigue-application-suppressed";
            }
            PublishResolution(rule, saving, _lastFatigueDisposition);
            return true;
        }

        private static void PublishResolution(RuleCastSpell rule,
            RuleSavingThrow saving, string fatigueDisposition)
        {
            int natural = saving.D20.Value;
            int fortitudeModifier = saving.StatValue;
            int conditionalBonus = saving.RequiresSuccessBonus ?
                saving.SuccessBonus : 0;
            string caster = string.IsNullOrWhiteSpace(rule.Initiator.CharacterName) ?
                "The caster" : rule.Initiator.CharacterName.Trim();
            string spell = rule.Spell.Blueprint == null ?
                "<unknown spell>" : rule.Spell.Blueprint.name;
            string message = string.Format(CultureInfo.InvariantCulture,
                "Acadamae Graduate: {0} accelerated {1} to Standard; Fortitude d20 {2} {3:+#;-#;+0} conditional {4:+#;-#;+0} = {5} vs DC {6}: {7}; fatigue={8}.",
                caster, spell, natural, fortitudeModifier, conditionalBonus,
                saving.RollResult,
                saving.DifficultyClass, saving.IsPassed ? "success" : "failure",
                fatigueDisposition);
            _lastResolutionMessage = message;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("acadamae", "accelerated-cast.resolved", message);
            try
            {
                EventBus.RaiseEvent<IWarningNotificationUIHandler>(
                    handler => handler.HandleWarning(message, false));
                Interlocked.Increment(ref _publishedResolutionCount);
            }
            catch (Exception exception)
            {
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("acadamae",
                        "accelerated-cast.notification-failed",
                        "Acadamae mechanics resolved, but its player-facing resolution notification failed.",
                        exception);
            }
        }
    }

    // Guarded runtime scenarios arm this immediately before the real cast. It is
    // otherwise inert and scopes one pre-roll to Acadamae's native saving throw.
    internal static class AcadamaeSavingThrowTestControl
    {
        [System.ThreadStatic] private static int? _queued;
        [System.ThreadStatic] private static int? _active;
        [System.ThreadStatic] private static int? _postRoll;
        [System.ThreadStatic] private static RuleSavingThrow _target;

        internal static void Queue(int naturalRoll)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new System.ArgumentOutOfRangeException("naturalRoll");
            _queued = naturalRoll;
        }

        internal static void Begin(RuleSavingThrow target)
        {
            _active = _queued;
            _postRoll = _queued;
            _target = target;
            _queued = null;
        }

        internal static bool TryConsume(out int naturalRoll)
        {
            if (!_active.HasValue)
            {
                naturalRoll = 0;
                return false;
            }
            naturalRoll = _active.Value;
            _active = null;
            return true;
        }

        internal static bool TryComplete(RuleSavingThrow target, out int naturalRoll)
        {
            if (!_postRoll.HasValue || !ReferenceEquals(_target, target))
            {
                naturalRoll = 0;
                return false;
            }
            naturalRoll = _postRoll.Value;
            _postRoll = null;
            _target = null;
            return true;
        }

        internal static void End()
        { _active = null; _postRoll = null; _target = null; }
        internal static void Cancel()
        { _queued = null; _active = null; _postRoll = null; _target = null; }
    }

    [HarmonyPatch(typeof(RuleSavingThrow), "OnTrigger")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class AcadamaeSavingThrowTestCompletionPatch
    {
        private static void Postfix(RuleSavingThrow __instance)
        {
            int naturalRoll;
            if (AcadamaeSavingThrowTestControl.TryComplete(__instance, out naturalRoll))
            {
                if (naturalRoll == 20) __instance.AutoPass = true;
            }
        }
    }

    [HarmonyPatch(typeof(RuleRollD20), "PreRollDice")]
    internal static class AcadamaeSavingThrowTestRollPatch
    {
        private static bool Prefix(ref int __result)
        {
            int naturalRoll;
            if (!AcadamaeSavingThrowTestControl.TryConsume(out naturalRoll))
                return true;
            __result = naturalRoll;
            return false;
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RequireFullRoundAction")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class AcadamaeRequireFullRoundActionPatch
    {
        private static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (!AcadamaeCastingRuntime.IsInspecting && __result &&
                AcadamaeCastingRuntime.IsEligible(__instance, true)) __result = false;
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class AcadamaeCommandConstructorPatch
    {
        private static void Postfix(UnitUseAbility __instance, AbilityData __1)
        { AcadamaeCastingRuntime.Arm(__instance, __1); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnAction")]
    internal static class AcadamaeCommandActionPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.Begin(__instance); }
        private static void Postfix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.End(__instance); }
    }

    [HarmonyPatch(typeof(RuleCastSpell), MethodType.Constructor,
        typeof(AbilityData), typeof(TargetWrapper))]
    internal static class AcadamaeRuleConstructorPatch
    {
        private static void Postfix(RuleCastSpell __instance)
        { AcadamaeCastingRuntime.AttachRule(__instance); }
    }

    [HarmonyPatch(typeof(RuleCastSpell), "OnTrigger",
        new[] { typeof(RulebookEventContext) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class AcadamaeSuccessfulCastPatch
    {
        private static void Postfix(RuleCastSpell __instance)
        {
            try { AcadamaeCastingRuntime.Complete(__instance); }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("acadamae",
                        "accelerated-cast.resolution-failed",
                        "Acadamae post-cast resolution failed without changing the completed spell.",
                        exception);
            }
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnEnded", new[] { typeof(bool) })]
    internal static class AcadamaeCommandEndedPatch
    {
        private static void Postfix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.Cancel(__instance); }
    }
}
