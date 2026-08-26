using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Harmony12;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Fatigue;

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
        private static string _lastEligibilityTrace;
        private static long _resolutionPublicationAttemptCount;
        private static long _publishedResolutionCount;
        private static readonly object PresentationTraceGate = new object();
        private static readonly HashSet<string> PresentationTraceKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private sealed class CanonicalInvocation
        {
            internal AbilityData Outer;
            internal AbilityData Canonical;
            internal Kingmaker.UnitLogic.Spellbook Spellbook;
            internal Kingmaker.UnitLogic.SpellSlot Slot;
            internal int SpellLevel = -1;
            internal bool IsPrepared;
            internal bool IsSummoning;
            internal string SlotResolution = "none";
        }

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
        internal static string LastEligibilityTrace
        { get { return _lastEligibilityTrace; } }
        internal static long PublishedResolutionCount
        { get { return Interlocked.Read(ref _publishedResolutionCount); } }
        internal static long ResolutionPublicationAttemptCount
        { get { return Interlocked.Read(ref _resolutionPublicationAttemptCount); } }
        internal static int InvocationCount { get { return Invocations.Count; } }
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
            _lastEligibilityTrace = null;
            Interlocked.Exchange(ref _resolutionPublicationAttemptCount, 0);
            Interlocked.Exchange(ref _publishedResolutionCount, 0);
            Invocations.Clear();
        }

        internal static bool IsEligible(AbilityData ability, bool longerThanStandard)
        { return Evaluate(ability, longerThanStandard).Eligible; }

        private static AcadamaeCastDecision Evaluate(AbilityData ability,
            bool longerThanStandard)
        {
            return Evaluate(ability, longerThanStandard,
                ResolveEffectiveModeState(ability));
        }

        private static AcadamaeCastDecision Evaluate(AbilityData ability,
            bool longerThanStandard, AcadamaeEffectiveModeState mode)
        {
            CanonicalInvocation invocation = ResolveCanonicalInvocation(ability);
            if (ability == null || ability.Caster == null ||
                ability.Caster.Progression == null ||
                invocation.Canonical == null ||
                invocation.Canonical.Blueprint == null ||
                invocation.SpellLevel < 0 || invocation.SpellLevel > 10)
                return new AcadamaeCastDecision(false, "invalid-ability",
                    longerThanStandard ? AcadamaeCastingTime.FullRound :
                        AcadamaeCastingTime.Standard,
                    longerThanStandard ? 1 : 0,
                    invocation.SpellLevel, 0);
            var spellbook = invocation.Spellbook;
            var blueprint = invocation.Canonical.Blueprint;
            return AcadamaeCastingPolicy.Decide(
                new AcadamaeCastRequest {
                    HasFeat = BlueprintBootstrap.AcadamaeGraduate != null &&
                        ability.Caster.Progression.Features.GetRank(
                            BlueprintBootstrap.AcadamaeGraduate) > 0,
                    AccelerationModeActive = mode.Active,
                    IsRealSpell = blueprint.IsSpell,
                    HasSpellbook = spellbook != null,
                    IsPreparedInvocation = invocation.IsPrepared,
                    IsArcane = spellbook != null && spellbook.Blueprint.IsArcane,
                    IsConjuration = blueprint.School == SpellSchool.Conjuration,
                    IsSummoning = invocation.IsSummoning,
                    EffectiveCastingTime = longerThanStandard ?
                        AcadamaeCastingTime.FullRound : AcadamaeCastingTime.Standard,
                    EffectiveRounds = 1,
                    SpellLevel = invocation.SpellLevel
                });
        }

        internal static bool IsPreparedInvocation(AbilityData ability,
            Kingmaker.UnitLogic.Spellbook spellbook)
        {
            CanonicalInvocation invocation = ResolveCanonicalInvocation(ability);
            return invocation.IsPrepared &&
                ReferenceEquals(invocation.Spellbook, spellbook);
        }

        private static CanonicalInvocation ResolveCanonicalInvocation(
            AbilityData ability)
        {
            var result = new CanonicalInvocation { Outer = ability };
            if (ability == null) return result;
            var chain = new List<AbilityData>();
            var visited = new HashSet<AbilityData>();
            for (AbilityData current = ability; current != null &&
                visited.Add(current) && chain.Count < 32;
                current = current.ConvertedFrom)
                chain.Add(current);
            result.Canonical = chain.LastOrDefault(value =>
                value != null && value.Blueprint != null) ?? ability;
            result.Spellbook = chain.Select(value => value.Spellbook)
                .FirstOrDefault(value => value != null);
            result.SpellLevel = result.Canonical == null ? -1 :
                result.Canonical.SpellLevel;
            result.IsSummoning = chain.Any(value => value.Blueprint != null &&
                (value.Blueprint.SpellDescriptor & SpellDescriptor.Summoning) != 0);
            if (result.Spellbook == null ||
                result.Spellbook.Blueprint.Spontaneous) return result;

            foreach (AbilityData source in chain)
            {
                Kingmaker.UnitLogic.SpellSlot candidate = source.ParamSpellSlot;
                if (MatchesPreparedSlot(candidate, result.Spellbook, chain))
                {
                    result.Slot = candidate;
                    result.SlotResolution = "converted-chain-param";
                    break;
                }
            }
            if (result.Slot == null)
            {
                int[] levels = chain.Select(value => value.SpellLevel)
                    .Where(value => value >= 0 && value <= 10).Distinct().ToArray();
                foreach (int level in levels)
                {
                    Kingmaker.UnitLogic.SpellSlot candidate = result.Spellbook
                        .GetMemorizedSpellSlots(level).FirstOrDefault(value =>
                            MatchesPreparedSlot(value, result.Spellbook, chain));
                    if (candidate == null) continue;
                    result.Slot = candidate;
                    result.SlotResolution = "memorized-blueprint-identity";
                    break;
                }
            }
            if (result.Slot != null)
            {
                result.Canonical = result.Slot.Spell;
                result.SpellLevel = result.Canonical.SpellLevel;
                result.IsPrepared = true;
            }
            return result;
        }

        private static bool MatchesPreparedSlot(Kingmaker.UnitLogic.SpellSlot slot,
            Kingmaker.UnitLogic.Spellbook spellbook, IEnumerable<AbilityData> chain)
        {
            return slot != null && slot.Available && slot.Spell != null &&
                ReferenceEquals(slot.Spell.Spellbook, spellbook) &&
                chain.Any(value => value != null && value.Blueprint != null &&
                    slot.Spell.Blueprint != null &&
                    (ReferenceEquals(value.Blueprint, slot.Spell.Blueprint) ||
                    string.Equals(value.Blueprint.AssetGuid.ToString(),
                        slot.Spell.Blueprint.AssetGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase)));
        }

        internal static bool InspectPreAcadamae(AbilityData ability)
        {
            try { _inspectPreAcadamae = true; return ability.RequireFullRoundAction; }
            finally { _inspectPreAcadamae = false; }
        }

        internal static bool IsInspecting { get { return _inspectPreAcadamae; } }
        internal static void Arm(UnitUseAbility command, AbilityData ability,
            UnitCommand.CommandType commandType)
        {
            CanonicalInvocation invocation = ResolveCanonicalInvocation(ability);
            AcadamaeEffectiveModeState mode =
                ResolveEffectiveModeState(ability);
            if (mode.Active &&
                ability.ParamSpellSlot == null &&
                invocation.IsPrepared)
                ability.ParamSpellSlot = invocation.Slot;
            bool preRequireFullRound = InspectPreAcadamae(ability);
            AcadamaeCastDecision decision = Evaluate(ability,
                preRequireFullRound, mode);
            if (mode.HasFeat)
            {
                _lastEligibilityTrace = DescribeEligibility(ability, command,
                    commandType, preRequireFullRound, decision, mode);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Info("acadamae", "eligibility.decision",
                        _lastEligibilityTrace);
            }
            if (decision.Eligible) Invocations.Arm(command, ability);
        }

        internal static string InspectEligibility(AbilityData ability)
        {
            bool preRequireFullRound = InspectPreAcadamae(ability);
            AcadamaeEffectiveModeState mode =
                ResolveEffectiveModeState(ability);
            return DescribeEligibility(ability, null,
                ability == null ? UnitCommand.CommandType.Standard :
                    ability.RuntimeActionType,
                preRequireFullRound, Evaluate(ability, preRequireFullRound,
                    mode), mode);
        }

        internal static void ApplyPresentation(AbilityData ability,
            ref bool requireFullRound)
        {
            if (IsInspecting) return;
            bool before = requireFullRound;
            CanonicalInvocation invocation = ResolveCanonicalInvocation(ability);
            AcadamaeEffectiveModeState mode =
                ResolveEffectiveModeState(ability);
            AcadamaeCastDecision decision = Evaluate(ability, before, mode);
            if (before && decision.Eligible) requireFullRound = false;
            if (!HasAcadamaeFeatOwner(ability) || !invocation.IsPrepared ||
                !invocation.IsSummoning) return;
            string trace = DescribeEligibility(ability, null,
                ability.RuntimeActionType, before, decision, mode) +
                ";boundary=get_RequireFullRoundAction;resultBefore=" + before +
                ";resultAfter=" + requireFullRound;
            string key = RuntimeHelpers.GetHashCode(ability.Caster) + ":" +
                (ability.Blueprint == null ? "<null>" :
                    ability.Blueprint.AssetGuid.ToString()) + ":" + trace;
            lock (PresentationTraceGate)
            {
                if (PresentationTraceKeys.Count >= 64 ||
                    !PresentationTraceKeys.Add(key)) return;
            }
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("acadamae", "presentation.decision", trace);
        }

        private static bool HasAcadamaeFeatOwner(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ability.Caster.Progression != null &&
                BlueprintBootstrap.AcadamaeGraduate != null &&
                ability.Caster.Progression.Features.GetRank(
                    BlueprintBootstrap.AcadamaeGraduate) > 0;
        }

        private static AcadamaeEffectiveModeState ResolveEffectiveModeState(
            AbilityData ability)
        {
            bool hasFeat = HasAcadamaeFeatOwner(ability);
            bool markerPresent = ability != null && ability.Caster != null &&
                BlueprintBootstrap.AcadamaeGraduateMode != null &&
                ability.Caster.Buffs.GetBuff(
                    BlueprintBootstrap.AcadamaeGraduateMode.Marker) != null;
            ActivatableAbility[] matches = ability == null ||
                ability.Caster == null ||
                ability.Caster.ActivatableAbilities == null ||
                BlueprintBootstrap.AcadamaeGraduateMode == null ?
                    new ActivatableAbility[0] :
                    ability.Caster.ActivatableAbilities.Enumerable.Where(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            BlueprintBootstrap.AcadamaeGraduateMode.Ability))
                    .ToArray();
            bool hasActivatable = matches.Length == 1;
            bool isOn = hasActivatable && matches[0].IsOn;
            AcadamaeEffectiveModeState state = AcadamaeModeStatePolicy.Decide(
                hasFeat, hasActivatable, isOn, markerPresent);
            if (matches.Length > 1 ||
                (hasFeat && hasActivatable && isOn != markerPresent))
            {
                string trace = string.Format(CultureInfo.InvariantCulture,
                    "status={0};activatableCount={1};isOn={2};marker={3}",
                    state.Status, matches.Length, isOn, markerPresent);
                string key = ability == null || ability.Caster == null ? trace :
                    RuntimeHelpers.GetHashCode(ability.Caster) + ":" + trace;
                lock (PresentationTraceGate)
                {
                    if (PresentationTraceKeys.Count < 64 &&
                        PresentationTraceKeys.Add(key))
                    {
                        ModContext context;
                        if (ModContext.TryGet(out context))
                            context.Logger.Warning("acadamae",
                                "mode.state-divergence", trace);
                    }
                }
            }
            return state;
        }

        private static string DescribeEligibility(AbilityData ability,
            UnitUseAbility command, UnitCommand.CommandType commandType,
            bool preRequireFullRound, AcadamaeCastDecision decision,
            AcadamaeEffectiveModeState modeState)
        {
            if (ability == null) return "constructor=three-argument-authoritative;status=invalid-ability";
            CanonicalInvocation invocation = ResolveCanonicalInvocation(ability);
            var spellbook = invocation.Spellbook;
            var slot = invocation.Slot;
            var chain = new List<string>();
            for (AbilityData current = ability; current != null;
                current = current.ConvertedFrom)
                chain.Add((current.Blueprint == null ? "<null>" :
                    current.Blueprint.name + ":" + current.Blueprint.AssetGuid) +
                    "@" + RuntimeHelpers.GetHashCode(current));
            int featRank = BlueprintBootstrap.AcadamaeGraduate == null ||
                ability.Caster == null || ability.Caster.Progression == null ? 0 :
                ability.Caster.Progression.Features.GetRank(
                    BlueprintBootstrap.AcadamaeGraduate);
            bool canSpend = spellbook != null && spellbook.CanSpend(ability, false);
            return string.Format(CultureInfo.InvariantCulture,
                "constructor=three-argument-authoritative;command={0};commandId={1};caster={2};featRank={3};mode={4};modeState={5};marker={6};spell={7}:{8};isSpell={9};school={10};descriptor={11}({12});level={13};spellbook={14}:{15};arcane={16};spontaneous={17};prepared={18};canSpend={19};slot={20};slotAvailable={21};slotSpell={22};paramSpellSlot={23};convertedFrom={24};preRequireFullRound={25};actionType={26};runtimeActionType={27};status={28};eligible={29};canonicalSpell={30}:{31};canonicalLevel={32};slotResolution={33}",
                commandType, command == null ? 0 : RuntimeHelpers.GetHashCode(command),
                ability.Caster == null ? "<null>" : ability.Caster.CharacterName,
                featRank, modeState.Active, modeState.Status,
                modeState.MarkerPresent,
                ability.Blueprint == null ? "<null>" : ability.Blueprint.name,
                ability.Blueprint == null ? "<null>" : ability.Blueprint.AssetGuid.ToString(),
                ability.Blueprint != null && ability.Blueprint.IsSpell,
                ability.Blueprint == null ? "<null>" : ability.Blueprint.School.ToString(),
                ability.Blueprint == null ? 0L : (long)ability.Blueprint.SpellDescriptor,
                ability.Blueprint == null ? "<null>" : ability.Blueprint.SpellDescriptor.ToString(),
                ability.SpellLevel,
                spellbook == null ? "<null>" : spellbook.Blueprint.name,
                spellbook == null ? "<null>" : spellbook.Blueprint.AssetGuid.ToString(),
                spellbook != null && spellbook.Blueprint.IsArcane,
                spellbook != null && spellbook.Blueprint.Spontaneous,
                IsPreparedInvocation(ability, spellbook), canSpend,
                slot == null ? 0 : RuntimeHelpers.GetHashCode(slot),
                slot != null && slot.Available,
                slot == null || slot.Spell == null || slot.Spell.Blueprint == null ?
                    "<null>" : slot.Spell.Blueprint.name + ":" +
                        slot.Spell.Blueprint.AssetGuid,
                slot == null ? "<null>" : "exact",
                string.Join("->", chain.ToArray()), preRequireFullRound,
                ability.ActionType, ability.RuntimeActionType,
                decision.Status, decision.Eligible,
                invocation.Canonical == null || invocation.Canonical.Blueprint == null ?
                    "<null>" : invocation.Canonical.Blueprint.name,
                invocation.Canonical == null || invocation.Canonical.Blueprint == null ?
                    "<null>" : invocation.Canonical.Blueprint.AssetGuid.ToString(),
                invocation.SpellLevel, invocation.SlotResolution);
        }
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
                CanonicalFatigueApplicationResult fatigue =
                    CanonicalFatigueApplicationRuntime.ApplyPermanentFatigue(
                        rule.Initiator.Descriptor.Buffs, _fatigued,
                        rule.Initiator);
                if (!fatigue.ApplicationSucceeded)
                    _lastFatigueDisposition =
                        "fatigue-application-suppressed";
                else if (fatigue.CordSubstituted &&
                    fatigue.State == CanonicalFatigueState.Neither)
                    _lastFatigueDisposition = "cord-substituted-fatigue";
                else if (fatigue.State == CanonicalFatigueState.Exhausted)
                    _lastFatigueDisposition = fatigue.Condition != null &&
                        fatigue.Condition.IsPermanent ?
                            "exhausted-permanent" :
                            "exhausted-not-permanent";
                else if (fatigue.State == CanonicalFatigueState.Fatigued)
                    _lastFatigueDisposition = fatigue.Condition != null &&
                        fatigue.Condition.IsPermanent ?
                            "fatigued-permanent" :
                            "fatigued-not-permanent";
                else
                    _lastFatigueDisposition = fatigue.Status;
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
            string detail = string.Format(CultureInfo.InvariantCulture,
                "Acadamae Graduate: {0} accelerated {1} to Standard; Fortitude d20 {2} {3:+#;-#;+0} conditional {4:+#;-#;+0} = {5} vs DC {6}: {7}; fatigue={8}.",
                caster, spell, natural, fortitudeModifier, conditionalBonus,
                saving.RollResult,
                saving.DifficultyClass, saving.IsPassed ? "success" : "failure",
                fatigueDisposition);
            string message = string.Format(CultureInfo.InvariantCulture,
                "Acadamae Graduate: Fortitude {0} vs DC {1} - {2}{3}.",
                saving.RollResult, saving.DifficultyClass,
                saving.IsPassed ? "success" : "failed",
                !saving.IsPassed ? ConditionSuffix(fatigueDisposition) :
                    string.Empty);
            _lastResolutionMessage = message;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("acadamae", "accelerated-cast.resolved", detail);
            Interlocked.Increment(ref _resolutionPublicationAttemptCount);
            if (NativeCombatLog.Publish("acadamae",
                    "accelerated-cast.combat-log-failed", message,
                    "Acadamae mechanics resolved, but the native combat-log entry failed."))
                Interlocked.Increment(ref _publishedResolutionCount);
        }

        private static string ConditionSuffix(string disposition)
        {
            if (string.Equals(disposition, "fatigued-permanent",
                    StringComparison.Ordinal))
                return "; Fatigued";
            if (string.Equals(disposition, "exhausted-permanent",
                    StringComparison.Ordinal))
                return "; Exhausted";
            if (string.Equals(disposition, "cord-substituted-fatigue",
                    StringComparison.Ordinal) ||
                string.Equals(disposition, "cord-substituted-exhaustion",
                    StringComparison.Ordinal))
                return "; Cord substituted the condition";
            return string.Empty;
        }
    }

    internal static class AcadamaePatchAudit
    {
        internal static void Publish(HarmonyInstance harmony, ModLogger logger,
            string modId)
        {
            if (harmony == null || logger == null) return;
            MethodBase[] targets = {
                typeof(AbilityData).GetProperty("RequireFullRoundAction").GetGetMethod(),
                typeof(UnitUseAbility).GetConstructor(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic, null, new[] {
                        typeof(UnitCommand.CommandType), typeof(AbilityData),
                        typeof(TargetWrapper) }, null),
                typeof(UnitUseAbility).GetMethod("OnAction", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic),
                typeof(UnitUseAbility).GetMethod("OnEnded", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(bool) }, null),
                typeof(RuleCastSpell).GetConstructor(new[] { typeof(AbilityData),
                    typeof(TargetWrapper) }),
                typeof(RuleCastSpell).GetMethod("OnTrigger", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(RulebookEventContext) }, null)
            };
            foreach (MethodBase target in targets.Where(value => value != null))
            {
                Patches patches = harmony.GetPatchInfo(target);
                Patch[] all = patches == null ? new Patch[0] :
                    patches.Prefixes.Concat(patches.Postfixes)
                        .Concat(patches.Transpilers).ToArray();
                bool applied = all.Any(value => value.owner == modId &&
                    value.patch != null && value.patch.DeclaringType != null &&
                    value.patch.DeclaringType.Namespace ==
                        typeof(AcadamaePatchAudit).Namespace);
                string cotwOwners = string.Join(",", all.Where(value =>
                        (value.owner ?? string.Empty).IndexOf("CallOfTheWild",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (value.patch == null || value.patch.DeclaringType == null ?
                            string.Empty : value.patch.DeclaringType.FullName)
                            .IndexOf("CallOfTheWild",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => value.owner).Distinct().ToArray());
                logger.Info("acadamae", "patch.audit", "target=" +
                    Signature(target) + ";applied=" + applied +
                    ";cotwOwners=" + (cotwOwners.Length == 0 ? "<none>" :
                        cotwOwners) + ";prefixes=" + Describe(patches == null ?
                        null : patches.Prefixes) + ";postfixes=" +
                    Describe(patches == null ? null : patches.Postfixes) +
                    ";transpilers=" + Describe(patches == null ? null :
                        patches.Transpilers));
            }
        }

        private static string Describe(IEnumerable<Patch> patches)
        {
            if (patches == null) return "<none>";
            return string.Join("|", patches.Select((patch, index) => index + ":" +
                patch.owner + "/" + patch.priority + "/" +
                (patch.patch == null || patch.patch.DeclaringType == null ?
                    "<null>" : patch.patch.DeclaringType.FullName + "." +
                        patch.patch.Name) + "/before=" +
                string.Join(",", patch.before ?? new string[0]) + "/after=" +
                string.Join(",", patch.after ?? new string[0])).ToArray());
        }

        private static string Signature(MethodBase method)
        {
            return method.DeclaringType.FullName + "." + method.Name + "(" +
                string.Join(",", method.GetParameters().Select(value =>
                    value.ParameterType.FullName).ToArray()) + ")";
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
                __instance.BaseRollResult = naturalRoll + __instance.StatValue;
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
        { AcadamaeCastingRuntime.ApplyPresentation(__instance, ref __result); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class AcadamaeCommandConstructorPatch
    {
        private static void Postfix(UnitUseAbility __instance,
            UnitCommand.CommandType __0, AbilityData __1)
        { AcadamaeCastingRuntime.Arm(__instance, __1, __0); }
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
