using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static void ConfigureSuggestion32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var buff = Require<BlueprintBuff>(bySymbol, Sprint32Suggestion.OwnedBuffSymbol);
            buff.name = InternalName(Sprint32Suggestion.OwnedBuffSymbol);
            buff.ComponentsArray = new BlueprintComponent[] { ScriptableObject.CreateInstance<Sprint32SuggestionPresence>() };
            foreach (var pair in new[] { new[] { "Satyr", Satyr32Prefix }, new[] { "LillendAzata", Lillend32Prefix } })
            {
                var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit." + pair[0]);
                var ability = Require<BlueprintAbility>(bySymbol, pair[1] + "Suggestion");
                ability.name = InternalName(pair[1] + "Suggestion");
                ConfigureMephitSelfAbility(ability, pair[0] == "Satyr" ? AbilityType.SpellLike : AbilityType.Spell,
                    AbilityEffectOnUnit.Harmful, AbilityEffectOnUnit.None, true);
                ability.Range = AbilityRange.Close; ability.CanTargetEnemies = true; ability.CanTargetSelf = false;
                ability.Hidden = true;
                var guard = ScriptableObject.CreateInstance<Sprint32SuggestionAvailable>(); guard.OwningBlueprint = unit;
                ability.ComponentsArray = new BlueprintComponent[] { guard };
                BlueprintUnitFactAccess.Resolve().Configure(ability,
                    LocalizationService.Create(pair[1] + "Suggestion.Name", "Suggestion"),
                    LocalizationService.Create(pair[1] + "Suggestion.Description",
                        "Available only with active CowWithHat's Custom Spells. Uses that mod's avoid-combat Suggestion adaptation."), null);
                BlueprintAiCastSpell ai;
                if (pair[0] == "Satyr")
                {
                    unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] { ability }).ToArray();
                    var parameters = Require<BlueprintFeature>(bySymbol, pair[1] + "Traits").GetComponents<Sprint29GiantSpellLikeParameters>().Single();
                    parameters.Abilities = parameters.Abilities.Concat(new[] { ability }).ToArray();
                    parameters.SpellLevels = parameters.SpellLevels.Concat(new[] { 3 }).ToArray();
                    ai = ConfigureSatyrControlAi32(bySymbol, unit, "Suggestion", ability, 4);
                    var eligibility = Require<SatyrControlEligibility>(bySymbol, pair[1] + "SuggestionEligibility");
                    eligibility.ControlBuffGuid = buff.AssetGuid; eligibility.RejectedCondition = UnitCondition.Dazed;
                }
                else
                {
                    var book = Require<BlueprintSpellbook>(bySymbol, pair[1] + "Spellbook");
                    unit.GetComponents<LillendBardBook32>().Single().OptionalSuggestion = ability;
                    ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol, pair[1] + "SuggestionAi"),
                        pair[1] + "SuggestionAi", ability, 1);
                    var eligibility = Require<LillendSpellEligibility32>(bySymbol, pair[1] + "SuggestionEligibility");
                    eligibility.OwningBlueprint = unit; eligibility.Book = book; eligibility.Spell = ability;
                    eligibility.Level = 2; eligibility.Choice = LillendSpellChoice32.Suggestion;
                    ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 3;
                }
                unit.Brain.Actions = unit.Brain.Actions.Concat(new BlueprintAiAction[] { ai }).ToArray();
            }
        }

        internal static void BindSuggestionProvider32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var source = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, Sprint32Suggestion.AbilityGuid,
                "CowWithHat Suggestion provider");
            var sourceBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, Sprint32Suggestion.BuffGuid,
                "CowWithHat Suggestion terminal state");
            var nativeBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "6477ae917b0ec7a4ca76bc9f36b023ac", "native Rainbow Pattern provider donor");
            var sourceComponents = source.ComponentsArray.Where(value => !(value is SpellListComponent)).ToArray();
            if (source.name != "SuggestionAbility" || source.Type != AbilityType.Spell || !source.SpellResistance ||
                source.Range != AbilityRange.Close || source.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard ||
                sourceComponents.Any(component => component == null ||
                    !(component is AbilityEffectRunAction) && !(component is SpellComponent) &&
                    !(component is SpellDescriptorComponent) && !(component is Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)))
                throw new InvalidOperationException("Optional Suggestion must retain the investigated native-only contract.");
            var sourceRun = source.GetComponents<AbilityEffectRunAction>().Single();
            if (sourceRun.GetType() != typeof(AbilityEffectRunAction) || sourceRun.Actions.Actions.Length != 1 ||
                sourceRun.Actions.Actions[0]?.GetType() != typeof(ContextActionConditionalSaved))
                throw new InvalidOperationException("Optional Suggestion acquired an unexpected sibling action.");
            var saved = (ContextActionConditionalSaved)sourceRun.Actions.Actions[0];
            if (saved.Succeed.Actions.Length != 0 || (saved.Failed.Actions.Length != 1 && saved.Failed.Actions.Length != 2) ||
                !(saved.Failed.Actions[0] is ContextActionApplyBuff))
                throw new InvalidOperationException("Optional Suggestion acquired an unexpected save branch: success=" +
                    string.Join(",", saved.Succeed.Actions.Select(action => action == null ? "<null>" : action.GetType().FullName).ToArray()) +
                    ";failure=" + string.Join(",", saved.Failed.Actions.Select(action => action == null ? "<null>" : action.GetType().FullName).ToArray()));
            var sourceApply = (ContextActionApplyBuff)saved.Failed.Actions[0];
            // CotW adds this wizard-discovery branch after Cow creates Suggestion.
            // Neither adult has the discovery. Recognize its exact positive gate;
            // do not import its foreign actions, buffs or feature into our copies.
            if (saved.Failed.Actions.Length == 2)
            {
                var extra = saved.Failed.Actions[1] as Conditional;
                var checker = extra?.ConditionsChecker;
                var condition = checker?.Conditions.SingleOrDefault() as ContextConditionCasterHasFact;
                if (extra?.GetType() != typeof(Conditional) || checker == null || checker.Operation != Operation.And || checker.Conditions.Length != 1 ||
                    condition?.GetType() != typeof(ContextConditionCasterHasFact) || condition.Not || condition.Fact?.AssetGuid != Sprint32Suggestion.ExcludedWizardDiscoveryGuid ||
                    condition.Fact.name != "InfectiousCharmsArcaneDiscoveryFeature" || extra.IfFalse.Actions.Length != 0)
                    throw new InvalidOperationException("Optional Suggestion acquired an unknown conditional branch.");
                foreach (var name in new[] { "Satyr", "LillendAzata" })
                    if (Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit." + name).AddFacts.Any(fact =>
                        fact?.AssetGuid == Sprint32Suggestion.ExcludedWizardDiscoveryGuid))
                        throw new InvalidOperationException("Private creature unexpectedly owns the excluded wizard discovery.");
            }
            if (sourceRun.SavingThrowType != Kingmaker.EntitySystem.Stats.SavingThrowType.Will ||
                !ReferenceEquals(sourceApply.Buff, sourceBuff) || sourceApply.Permanent || sourceApply.UseDurationSeconds ||
                sourceApply.ToCaster || !sourceApply.IsFromSpell || sourceApply.IsNotDispelable || sourceApply.AsChild ||
                sourceApply.DurationValue.Rate != DurationRate.Minutes || sourceApply.DurationValue.DiceType != Kingmaker.RuleSystem.DiceType.Zero ||
                sourceApply.DurationValue.DiceCountValue.ValueType != ContextValueType.Simple || sourceApply.DurationValue.DiceCountValue.Value != 0 ||
                sourceApply.DurationValue.BonusValue.ValueType != ContextValueType.Rank ||
                sourceApply.DurationValue.BonusValue.ValueRank != AbilityRankType.Default)
                throw new InvalidOperationException("Optional Suggestion save/effect/duration contract changed.");
            // Compare the complete provider terminal-component shape with its
            // stated native donor. Nested blueprint references must be the
            // identical native objects, not just matching root component types.
            if (!SuggestionNativeGraphEqual32(sourceBuff.ComponentsArray, nativeBuff.ComponentsArray, 0))
                throw new InvalidOperationException("Optional Suggestion terminal graph diverged from native Rainbow Pattern.");
            // Validate only the supported core that will be copied. The exact
            // excluded discovery gate above is unavailable to these creatures.
            ValidateSuggestionGraph32(sourceApply, sourceBuff, new HashSet<object>(), 0);
            foreach (var component in sourceComponents.Where(value => !(value is AbilityEffectRunAction)))
                ValidateSuggestionGraph32(component, sourceBuff, new HashSet<object>(), 0);
            foreach (var component in sourceBuff.ComponentsArray)
                ValidateSuggestionGraph32(component, nativeBuff, new HashSet<object>(), 0);

            var ownedBuff = Require<BlueprintBuff>(bySymbol, Sprint32Suggestion.OwnedBuffSymbol);
            var stagedBuff = ScriptableObject.CreateInstance<BlueprintBuff>();
            var stagedAbilities = new List<KeyValuePair<BlueprintAbility, BlueprintAbility>>();
            try
            {
                CopyFields(sourceBuff, stagedBuff); stagedBuff.PrototypeLink = null;
                stagedBuff.name = InternalName(Sprint32Suggestion.OwnedBuffSymbol);
                ClearLillendPerformanceDeathPersistence32(stagedBuff);
                stagedBuff.ComponentsArray = sourceBuff.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                    .Concat(new BlueprintComponent[] { ScriptableObject.CreateInstance<Sprint32SuggestionPresence>() }).ToArray();
                foreach (var pair in new[] { new[] { "Satyr", Satyr32Prefix }, new[] { "LillendAzata", Lillend32Prefix } })
                {
                    var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit." + pair[0]);
                    var owned = Require<BlueprintAbility>(bySymbol, pair[1] + "Suggestion");
                    var staged = ScriptableObject.CreateInstance<BlueprintAbility>();
                    stagedAbilities.Add(new KeyValuePair<BlueprintAbility, BlueprintAbility>(owned, staged));
                    CopyFields(source, staged); staged.PrototypeLink = null; staged.Parent = null;
                    staged.name = InternalName(pair[1] + "Suggestion"); staged.Hidden = false;
                    staged.Type = pair[0] == "Satyr" ? AbilityType.SpellLike : AbilityType.Spell;
                    var guard = ScriptableObject.CreateInstance<Sprint32SuggestionAvailable>(); guard.OwningBlueprint = unit;
                    staged.ComponentsArray = sourceComponents.Select(component => CloneSuggestionCore32(component, ownedBuff))
                        .Concat(new BlueprintComponent[] { guard }).ToArray();
                    // Cow1.4.5 declares rank(Default) minutes but supplies no
                    // rank component. Bind the advertised live caster-level
                    // duration in our private copies using the proven native seam.
                    if (staged.GetComponents<ContextRankConfig>().Any())
                        throw new InvalidOperationException("Optional Suggestion now supplies a rank contract; reconcile it explicitly.");
                    var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
                    SetField(rank, "m_Type", AbilityRankType.Default);
                    SetField(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
                    SetField(rank, "m_Progression", ContextRankProgression.AsIs);
                    SetField(rank, "m_UseMin", false); SetField(rank, "m_UseMax", false);
                    staged.ComponentsArray = staged.ComponentsArray.Concat(new BlueprintComponent[] { rank }).ToArray();
                    foreach (var component in staged.ComponentsArray.Where(value => !ReferenceEquals(value, guard)))
                        ValidateSuggestionGraph32(component, ownedBuff, new HashSet<object>(), 0);
                }
                // All cloning, nested checks and guard construction are done.
                // Every possible old/new component array remains guarded while
                // Active is false; even a failed field copy cannot expose the
                // unguarded provider array on a registered private blueprint.
                CopyFields(stagedBuff, ownedBuff); ownedBuff.name = stagedBuff.name;
                foreach (var pair in stagedAbilities)
                { CopyFields(pair.Value, pair.Key); pair.Key.name = pair.Value.name; }
                Require<BlueprintSpellList>(bySymbol, Lillend32Prefix + "SpellList").SpellsByLevel.Single(row => row.SpellLevel == 2)
                    .Spells.Add(Require<BlueprintAbility>(bySymbol, Lillend32Prefix + "Suggestion"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stagedBuff);
                foreach (var pair in stagedAbilities) UnityEngine.Object.DestroyImmediate(pair.Value);
            }
        }

        private static BlueprintComponent CloneSuggestionCore32(BlueprintComponent component, BlueprintBuff terminal)
        {
            var sourceRun = component as AbilityEffectRunAction;
            if (sourceRun == null) return ExpandedSummoningAbilityBuilder.DeepCloneComponent(component);
            var saved = (ContextActionConditionalSaved)sourceRun.Actions.Actions.Single();
            var apply = (ContextActionApplyBuff)ExpandedSummoningAbilityBuilder.DeepCloneComponentObject(saved.Failed.Actions[0]);
            apply.Buff = terminal;
            var privateSaved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
            privateSaved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() };
            privateSaved.Failed = new ActionList { Actions = new GameAction[] { apply } };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            CopyFields(sourceRun, run);
            run.Actions = new ActionList { Actions = new GameAction[] { privateSaved } };
            return run;
        }

        // Private provider-shape checks, not a general blueprint registry.
        // Native Unity object bookkeeping and editor prototype links are not
        // behavior. All managed effect fields and nested blueprint identities
        // are checked; a deeper or external graph is unsupported, not guessed.
        private static bool SuggestionNativeGraphEqual32(object left, object right, int depth)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.GetType() != right.GetType() || depth > 32) return false;
            var type = left.GetType();
            if (type.IsPrimitive || type.IsEnum || left is string || left is decimal) return left.Equals(right);
            if (left is BlueprintScriptableObject) return false;
            var a = left as System.Collections.IEnumerable; var b = right as System.Collections.IEnumerable;
            if (a != null && b != null)
            {
                var aa = a.Cast<object>().ToArray(); var bb = b.Cast<object>().ToArray();
                return aa.Length == bb.Length && aa.Select((value, index) =>
                    SuggestionNativeGraphEqual32(value, bb[index], depth + 1)).All(value => value);
            }
            return Fields(type).Where(field => field.DeclaringType != typeof(UnityEngine.Object) &&
                field.Name != "PrototypeLink").All(field =>
                    SuggestionNativeGraphEqual32(field.GetValue(left), field.GetValue(right), depth + 1));
        }

        private static void ValidateSuggestionGraph32(object value, BlueprintBuff allowedTerminal,
            HashSet<object> seen, int depth)
        {
            if (value == null || value is string || value.GetType().IsPrimitive || value.GetType().IsEnum ||
                value is decimal || !seen.Add(value)) return;
            if (depth > 32) throw new InvalidOperationException("Optional Suggestion graph exceeded its bounded contract.");
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null)
            {
                if (!ReferenceEquals(blueprint, allowedTerminal))
                    throw new InvalidOperationException("Unexpected optional Suggestion blueprint reference: " + blueprint.name);
                return;
            }
            var items = value as System.Collections.IEnumerable;
            if (items != null)
            { foreach (var item in items) ValidateSuggestionGraph32(item, allowedTerminal, seen, depth + 1); return; }
            var type = value.GetType();
            if (type.Assembly != typeof(BlueprintAbility).Assembly && !type.IsValueType)
                throw new InvalidOperationException("Unexpected optional Suggestion component type: " + type.FullName);
            foreach (var field in Fields(type).Where(field => field.DeclaringType != typeof(UnityEngine.Object) &&
                field.Name != "PrototypeLink"))
                ValidateSuggestionGraph32(field.GetValue(value), allowedTerminal, seen, depth + 1);
        }
    }
}
