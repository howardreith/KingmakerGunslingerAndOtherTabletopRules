using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private string Sprint32SavedTransition => (string)_request.Parameters?["suggestionTransition"];

        private bool Sprint32SavedProviderBoundary(string stage)
        {
            string transition = Sprint32SavedTransition;
            bool selected = transition != null;
            bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare;
            bool cleanup = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup;
            bool present = selected && (prepare || transition != ExpandedSummoningRosterPersistencePolicy.Sprint32SuggestionRemoved);
            bool enabled = _context.FeatureModules.Active.ExpandedSummoning;
            bool expectedEnabled = !(cleanup && transition == ExpandedSummoningRosterPersistencePolicy.Sprint32SuggestionModuleOff);
            var entries = ((IEnumerable)_context.ModEntry.GetType().DeclaringType.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null))
                .Cast<UnityModManagerNet.UnityModManager.ModEntry>().Where(entry => entry?.Info?.Id == Sprint32Suggestion.ModId).ToArray();
            bool providerExact = present ? entries.Length == 1 && entries[0].Loaded && entries[0].Active &&
                !entries[0].ErrorOnLoading && entries[0].Assembly?.GetName().Name == Sprint32Suggestion.ModId : entries.Length == 0;
            bool exact = (!selected || ExpandedSummoningRosterPersistencePolicy.Sprint32SuggestionTransition(transition) && enabled == expectedEnabled) &&
                providerExact && Sprint32Suggestion.Settled && Sprint32Suggestion.Active == (present && enabled);
            RosterPersistenceCheck("s32-provider-" + stage, exact, new JObject {
                ["transition"] = transition, ["moduleEnabled"] = enabled, ["providerExpected"] = present,
                ["disposition"] = Sprint32Suggestion.Disposition, ["providerEntries"] = new JArray(entries.Select(entry => new JObject {
                    ["id"] = entry.Info.Id, ["version"] = entry.Info.Version, ["loaded"] = entry.Loaded,
                    ["active"] = entry.Active, ["error"] = entry.ErrorOnLoading,
                    ["mvid"] = entry.Assembly?.ManifestModule.ModuleVersionId.ToString("D") })) },
                "The fixed full-intake transition has the declared actual UMM provider and module state; no live provider toggle or save-free profile is used.");
            return exact;
        }

        // Reuse the already owned Hold recipient, whose paralysis does not
        // supply Dazed. This establishes an effect-only saved-state precondition,
        // not another casting/AI/save/SR/action claim.
        private void PrepareSprint32SavedSuggestion(UnitEntityData caster, UnitEntityData[] sources)
        {
            if (Sprint32SavedTransition == null) return;
            var target = Sprint32SavedControls(caster).Single(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Role == "hold");
            var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            var source = sources.Single(unit => unit.UniqueId == receipt.SourceId);
            if (!Game.Instance.IsPaused || !Sprint32Suggestion.Active || target.Descriptor.State.HasCondition(UnitCondition.Dazed) ||
                target.Buffs.Enumerable.Any(buff => buff.Blueprint.AssetGuid == Sprint32Suggestion.OwnedBuffGuid))
                throw new InvalidOperationException("Optional saved-effect precondition differs; no save.");
            var ability = Sprint32SavedBlueprint<BlueprintAbility>("LillendAzata.Suggestion");
            var blueprint = Sprint32SavedBlueprint<BlueprintBuff>("SuggestionBuff");
            var book = source.Descriptor.GetSpellbook(source.Blueprint.GetComponent<LillendBardBook32>().Book);
            var data = book.GetKnownSpells(2).Single(value => ReferenceEquals(value.Blueprint, ability));
            var parameters = data.CalculateParams();
            var apply = Sprint32ControlApplications(ability.GetComponents<AbilityEffectRunAction>().Single().Actions)
                .Where(value => ReferenceEquals(value.Buff, blueprint)).Single();
            if (parameters.CasterLevel != 7 || parameters.DC != 16 || apply.Permanent ||
                Kingmaker.ElementsSystem.ElementsContext.GetData<Buff.Data>()?.Buff != null)
                throw new InvalidOperationException("Optional saved-effect native context/terminal differs.");
            var context = new AbilityExecutionContext(data, parameters, new Kingmaker.Utility.TargetWrapper(target), null);
            var rule = new RuleApplyBuff(target, blueprint, context, TimeSpan.FromSeconds(420),
                (buff, parent, duration) => target.Descriptor.Buffs.AddBuff(buff, parent, duration));
            Rulebook.Trigger(rule);
            if (rule.AppliedBuff == null) throw new InvalidOperationException("Optional saved-effect application absent.");
            rule.AppliedBuff.IsFromSpell = apply.IsFromSpell; rule.AppliedBuff.IsNotDispelable = apply.IsNotDispelable;
            receipt.SuggestionState = Sprint32SavedSuggestionState(target, source).ToString(Formatting.None);
            ObserveSprint32SavedSuggestion(caster, sources, "prepared");
        }

        private static JObject Sprint32SavedSuggestionState(UnitEntityData target, UnitEntityData source)
        {
            var effects = target.Buffs.Enumerable.Where(buff => buff.Blueprint.AssetGuid == Sprint32Suggestion.OwnedBuffGuid).ToArray();
            var effect = effects.Length == 1 ? effects[0] : null;
            return new JObject { ["target"] = target.UniqueId, ["source"] = source.UniqueId,
                ["count"] = effects.Length, ["dazed"] = target.Descriptor.State.HasCondition(UnitCondition.Dazed),
                ["active"] = effect != null && effect.Active && !effect.IsDisposed && !effect.IsPermanent &&
                    effect.EndTime > Game.Instance.Player.GameTime && ReferenceEquals(effect.Context?.MaybeCaster, source),
                ["blueprint"] = effect?.Blueprint.AssetGuid, ["endTicks"] = effect == null ? (JToken)JValue.CreateNull() : new JValue(effect.EndTime.Ticks),
                ["ability"] = effect?.Context?.SourceAbility?.AssetGuid,
                ["cl"] = effect == null ? (JToken)JValue.CreateNull() : new JValue(effect.Context.Params.CasterLevel),
                ["dc"] = effect == null ? (JToken)JValue.CreateNull() : new JValue(effect.Context.Params.DC),
                ["fromSpell"] = effect?.IsFromSpell, ["notDispelable"] = effect?.IsNotDispelable };
        }

        private void ObserveSprint32SavedSuggestion(UnitEntityData caster, UnitEntityData[] sources, string stage)
        {
            if (Sprint32SavedTransition == null) return;
            var satyrGrant = Sprint32SavedBlueprint<BlueprintAbility>("Satyr.Suggestion");
            var satyrs = new JArray(sources.Where(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "satyr").Select(unit => {
                var grants = unit.Descriptor.Abilities.Enumerable.Where(fact => ReferenceEquals(fact.Blueprint, satyrGrant)).ToArray();
                var data = grants.Length == 1 ? new AbilityData(grants[0]) : null;
                return new JObject { ["owner"] = unit.UniqueId, ["blueprint"] = satyrGrant.AssetGuid,
                    ["grants"] = grants.Length, ["correctCaster"] = data != null && ReferenceEquals(data.Caster, unit.Descriptor),
                    ["available"] = data != null && data.IsAvailable };
            }));
            bool grantsExact = satyrs.Count == 2 && satyrs.All(value => (int)value["grants"] == 1 &&
                (bool)value["correctCaster"] && (bool)value["available"] == Sprint32Suggestion.Active);
            RosterPersistenceCheck("s32-satyr-suggestion-" + stage, grantsExact,
                new JObject { ["providerActive"] = Sprint32Suggestion.Active, ["grants"] = satyrs },
                "Each exact Satyr retains one loadable private Suggestion grant; native availability follows the provider/module state without a casting or resource claim.");
            if (!grantsExact) throw new InvalidOperationException("Saved Satyr optional grant/availability differs.");
            var target = Sprint32SavedControls(caster).Single(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Role == "hold");
            var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            var source = sources.Single(unit => unit.UniqueId == receipt.SourceId);
            var saved = receipt.SuggestionState == null ? null : JObject.Parse(receipt.SuggestionState);
            var actual = Sprint32SavedSuggestionState(target, source);
            bool exact = saved != null && (bool)saved["active"] && (bool)saved["dazed"] &&
                (string)saved["target"] == target.UniqueId && (string)saved["source"] == source.UniqueId &&
                (string)saved["blueprint"] == Sprint32Suggestion.OwnedBuffGuid &&
                (Sprint32Suggestion.Active ? JToken.DeepEquals(saved, actual) : (int)actual["count"] == 0 && !(bool)actual["dazed"]);
            RosterPersistenceCheck("s32-suggestion-" + stage, exact,
                new JObject { ["providerActive"] = Sprint32Suggestion.Active, ["saved"] = saved, ["actual"] = actual },
                "The exact private saved Suggestion retains source/deadline/flags/Dazed with its provider; removal or module OFF clears both the effect and actual Dazed without refilling the book.");
            if (!exact) throw new InvalidOperationException("Optional saved-effect state differs: " + stage);
        }

        private JObject ExpectedSprint32ColdState(UnitPartSprint32SavedReceipt receipt)
        {
            if (receipt == null || receipt.SuggestionTransition != Sprint32SavedTransition)
                throw new InvalidOperationException("Saved/requested provider transition differs.");
            var saved = JObject.Parse(receipt.State);
            if (Sprint32SavedTransition != null && !Sprint32Suggestion.Active)
            {
                // Normalize only the explicitly removed optional spell/effect.
                // Preserve the raw saved snapshot in evidence and every slot,
                // performance, other effect, item and resource comparison.
                var effects = (JArray)saved["effects"];
                foreach (var effect in effects.Where(value => (string)value["blueprint"] == Sprint32Suggestion.OwnedBuffGuid).ToArray()) effect.Remove();
                var known = saved["book"]?["known"] as JArray;
                string optional = "2:" + Sprint32SavedBlueprint<BlueprintAbility>("LillendAzata.Suggestion").AssetGuid;
                if (known != null)
                {
                    var matches = known.Where(value => (string)value == optional).ToArray();
                    if (matches.Length != 1) throw new InvalidOperationException("Saved optional known spell absent/duplicated before transition.");
                    matches[0].Remove();
                }
            }
            return saved;
        }
    }
}
