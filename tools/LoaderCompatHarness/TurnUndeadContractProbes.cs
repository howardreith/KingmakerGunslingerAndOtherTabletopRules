using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Turn Undead modes beyond the first turn:
    ///  * "contracts": a machine-generated, hash-bound extract of the installed engine and blueprint contracts
    ///    (method IL hashes for the DC, save-reason, targeting and fear routes; the native donors, source grants,
    ///    resources, costs, radius and target filter; the channel resistance list; CotW channel entries and wrapper
    ///    membership when CotW is loaded), and an assertion of every constructed Turn component against literal
    ///    expectations written here independently of the product catalog, so a wrong qualifying fact, calculator
    ///    class or statistic, resource, cost or target filter fails.
    ///  * "undead-caster": a true undead channeler receives the initial save from its own Turn; a living control
    ///    in range does not.
    ///  * "abandon": a nested first-turn probe is released while suspended after it paused the game and silenced
    ///    the area; the prior pause flag, every brain and every spawned unit must be restored, with an accurate
    ///    cleanup verdict.
    /// </summary>
    internal sealed partial class TurnUndeadProbe
    {
        private sealed class SourceExpectation
        {
            internal string Kind, Feature, Resource, ClassGuid, NativeHarm;
            internal int Cost;
            internal StatType Stat;
            internal bool LawfulGood;
        }

        // Literal expectations (installed 2.1.7b blueprints, printed channel rules), independent of the product.
        private static readonly SourceExpectation[] ExpectedSources =
        {
            new SourceExpectation { Kind = "Cleric", Feature = "a79013ff4bcd4864cb669622a29ddafb", Resource = "5e2bba3e07c37be42909a12945c27de7",
                Cost = 1, ClassGuid = "67819271767a9dd4fbfd4ae700befea0", Stat = StatType.Charisma, NativeHarm = "279447a6bf2d3544d93a0a39c3b8e91d", LawfulGood = false },
            new SourceExpectation { Kind = "Paladin", Feature = "cb6d55dda5ab906459d18a435994a760", Resource = "9dedf41d995ff4446a181f143c3db98c",
                Cost = 2, ClassGuid = "bfa11238e7ae3544bbeb4d0b92e897ec", Stat = StatType.Charisma, NativeHarm = "4937473d1cfd7774a979b625fb833b47", LawfulGood = true },
            new SourceExpectation { Kind = "Hospitaler", Feature = "a9ab1bbc79ecb174d9a04699986ce8d5", Resource = "b0e0c7716ab27c64fb4b131c9845c596",
                Cost = 1, ClassGuid = "bfa11238e7ae3544bbeb4d0b92e897ec", Stat = StatType.Charisma, NativeHarm = "cc17243b2185f814aa909ac6b6599eaa", LawfulGood = true },
            new SourceExpectation { Kind = "Empyreal", Feature = "7d49d7f590dc9a948b3bd1c8b7979854", Resource = "f9af9354fb8a79649a6e512569387dc5",
                Cost = 1, ClassGuid = "b3a505fb61437dc4097f43c3f8f9a4cf", Stat = StatType.Wisdom, NativeHarm = "e1536ee240c5d4141bf9f9485a665128", LawfulGood = false }
        };
        private const string BasicFeatSelection = "247a4068296e8be42890143f451b4b45";
        private const string ChannelResistanceFour = "a9ac84c6f48b491438f91bb237bc9212";
        private const string NativeFrightenedUndead = "1265c0b27ef559c4e980a900dfc68d1e";

        private static readonly string[][] ContractMethods =
        {
            new[] { "RuleCalculateAbilityParams", "OnTrigger", "DC route: caster level = spellbook CL or character level; DC = 10 + stat bonus + spell level (+ bonuses)" },
            new[] { "ContextCalculateAbilityParamsBasedOnClass", "Calculate", "replaces caster level with the class level and spell level with half of it, and the statistic" },
            new[] { "AbilityData", "CalculateParams", "preview path: triggers RuleCalculateAbilityParams without ability calculators" },
            new[] { "SavingThrowBonusAgainstSpecificSpells", "OnEventAboutToTrigger", "channel resistance matches Reason.Context.SourceAbility" },
            new[] { "BuffStatusCondition", "OnNewRound", "native fear buffs re-save each round with Reason = the buff fact" },
            new[] { "AbilityTargetsAround", "Select", "all units within the radius with line of sight, filtered by target type and condition; includes the caster" },
            new[] { "AbilityCasterHasFacts", "CorrectCaster", "caster restriction by facts (NeedsAll tests only the last fact)" },
            new[] { "AddFeatureIfHasFact", "OnFactActivate", "grants the feature on activation when the checked fact is present" },
            new[] { "AddFeatureIfHasFact", "HandleUnitGainLevel", "re-checks on every level-up; never removes" },
            new[] { "UnitFearController", "TickOnUnit", "Frightened units flee from visible enemies" }
        };

        // ---------------------------------------------------------------- contracts

        private IEnumerable<object> Contracts()
        {
            Assembly engine = typeof(BlueprintAbility).Assembly;
            result["engine"] = new JObject
            {
                ["assembly"] = engine.GetName().Name, ["sha256"] = Sha256File(engine.Location),
                ["gameVersion"] = Application.version
            };
            var methods = new JArray();
            foreach (string[] entry in ContractMethods)
            {
                Type type = engine.GetTypes().FirstOrDefault(t => t.Name == entry[0]);
                var row = new JObject { ["type"] = type == null ? entry[0] + " (missing)" : type.FullName, ["method"] = entry[1], ["observation"] = entry[2] };
                MethodInfo[] found = type == null ? new MethodInfo[0] : type.GetMethods(Any).Where(m => m.Name == entry[1] && m.DeclaringType == type).ToArray();
                row["ilSha256"] = new JArray(found.Select(m =>
                {
                    try { byte[] il = m.GetMethodBody() == null ? null : m.GetMethodBody().GetILAsByteArray(); return (object)(il == null ? "no-body" : Sha256(il)); }
                    catch (Exception e) { return (object)("unreadable: " + e.GetType().Name); }
                }).ToArray());
                if (found.Length == 0) problems.Add("contract method not found: " + entry[0] + "." + entry[1]);
                methods.Add(row);
            }
            result["methods"] = methods;

            BlueprintFeature feat = Feat;
            BlueprintBuff fleeing = Fleeing;
            var sources = new JArray();
            foreach (SourceExpectation expected in ExpectedSources)
            {
                object source = ((Array)Member(set, "Sources")).Cast<object>().Single(s => Member(Member(s, "Contract"), "Kind").ToString() == expected.Kind);
                var feature = Blueprint<BlueprintFeature>(expected.Feature);
                var harm = Blueprint<BlueprintAbility>(expected.NativeHarm);
                var row = new JObject
                {
                    ["kind"] = expected.Kind,
                    ["sourceFeature"] = feature.AssetGuid + ":" + feature.name,
                    ["sourceGrants"] = new JArray(feature.GetComponents<AddFacts>().SelectMany(a => a.Facts ?? new BlueprintUnitFact[0])
                        .Select(f => (object)(f.AssetGuid + ":" + f.name)).ToArray()),
                    ["nativeHarm"] = DescribeAbility(harm)
                };
                foreach (string role in new[] { "Direct", "MenuChild" })
                {
                    var ability = (BlueprintAbility)Member(source, role);
                    row[role] = DescribeAbility(ability);
                    CheckConstructed(expected, role, ability, harm, feat, fleeing);
                }
                sources.Add(row);
            }
            result["sources"] = sources;

            // Feat: grants, prerequisite and publication.
            var grants = feat.GetComponents<AddFeatureIfHasFact>().ToArray();
            result["feat"] = new JObject
            {
                ["guid"] = feat.AssetGuid, ["groups"] = string.Join(",", feat.Groups.Select(g => g.ToString()).ToArray()), ["ranks"] = feat.Ranks,
                ["grants"] = new JArray(grants.Select(g => (object)((g.CheckedFact == null ? "null" : g.CheckedFact.AssetGuid) + "->" +
                    (g.Feature == null ? "null" : g.Feature.AssetGuid) + (g.Not ? " (Not)" : ""))).ToArray()),
                ["prerequisite"] = new JArray(feat.GetComponents<PrerequisiteFeaturesFromList>().Select(p => (object)(p.Amount + " of " +
                    string.Join(",", (p.Features ?? new BlueprintFeature[0]).Select(f => f.AssetGuid).ToArray()))).ToArray()),
                ["inBasicFeatSelection"] = Blueprint<BlueprintFeatureSelection>(BasicFeatSelection).AllFeatures.Contains(feat)
            };
            if (grants.Length != ExpectedSources.Length) problems.Add("the feat has " + grants.Length + " source grants, expected 4");
            foreach (SourceExpectation expected in ExpectedSources)
            {
                BlueprintAbility direct = Direct(expected.Kind);
                if (grants.Count(g => g.CheckedFact != null && g.CheckedFact.AssetGuid == expected.Feature && g.Feature == direct && !g.Not) != 1)
                    problems.Add("the feat does not grant the " + expected.Kind + " action exactly when " + expected.Feature + " is present");
            }
            PrerequisiteFeaturesFromList[] prerequisites = feat.GetComponents<PrerequisiteFeaturesFromList>().ToArray();
            if (prerequisites.Length != 1 || prerequisites[0].Amount != 1 ||
                !new HashSet<string>(prerequisites[0].Features.Select(f => f.AssetGuid)).SetEquals(ExpectedSources.Select(e => e.Feature)))
                problems.Add("the feat prerequisite is not exactly one of the four source features");
            if (!feat.Groups.Contains(FeatureGroup.Feat)) problems.Add("the feat is not in the Feat group");

            // Flight effect and resistance.
            var nativeFear = Blueprint<BlueprintBuff>(NativeFrightenedUndead);
            result["flight"] = new JObject
            {
                ["guid"] = fleeing.AssetGuid, ["stacking"] = fleeing.Stacking.ToString(), ["nativeDonorStacking"] = nativeFear.Stacking.ToString(),
                ["components"] = new JArray(fleeing.ComponentsArray.Select(c => (object)(c.GetType().Name + ":" + c.name)).ToArray()),
                ["descriptors"] = new JArray(fleeing.ComponentsArray.OfType<SpellDescriptorComponent>().Select(d => (object)d.Descriptor.Value.ToString()).ToArray())
            };
            var resistance = Blueprint<BlueprintFeature>(ChannelResistanceFour);
            BlueprintAbility[] spells = resistance.GetComponents<SavingThrowBonusAgainstSpecificSpells>().SelectMany(c => c.Spells ?? new BlueprintAbility[0]).ToArray();
            BlueprintAbility[] turn = ExpectedSources.SelectMany(e => new[] { Direct(e.Kind), (BlueprintAbility)Member(SourceOf(e.Kind), "MenuChild") }).ToArray();
            result["channelResistance4"] = new JObject
            {
                ["components"] = resistance.GetComponents<SavingThrowBonusAgainstSpecificSpells>().Count(),
                ["entries"] = spells.Length, ["turnEntries"] = turn.Count(t => spells.Contains(t))
            };
            if (turn.Any(t => !spells.Contains(t))) problems.Add("native ChannelResistance4 lacks a Turn identity");

            // Full component dumps of every native blueprint the design relies on (donors, turning donor, undead
            // immunities, the Empyreal override, channel resistance) and of the constructed flight effect.
            result["nativeFacts"] = new JObject
            {
                ["harmDonors"] = new JArray(ExpectedSources.Select(e => (object)DumpFact(Blueprint<BlueprintAbility>(e.NativeHarm))).ToArray()),
                ["sourceFeatures"] = new JArray(ExpectedSources.Select(e => (object)DumpFact(Blueprint<BlueprintFeature>(e.Feature))).ToArray()),
                ["frightenedUndeadDonor"] = DumpFact(nativeFear),
                ["wizardTurnUndead"] = DumpFact(ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("71b8898b1d26d654b9a3eeac87e3e2f8")),
                ["undeadType"] = DumpFact(Blueprint<BlueprintUnitFact>(UndeadType)),
                ["undeadImmunities"] = DumpFact(Blueprint<BlueprintUnitFact>("8a75eb16bfff86949a4ddcb3dd2f83ae")),
                ["channelResistance4"] = DumpFact(resistance),
                ["turnFlight"] = DumpFact(fleeing)
            };

            // Call of the Wild, when loaded: channel entries, Turn wrapper membership and the modifier holders.
            result["callOfTheWild"] = DescribeCotw();
            yield break;
        }

        private object SourceOf(string kind) =>
            ((Array)Member(set, "Sources")).Cast<object>().Single(s => Member(Member(s, "Contract"), "Kind").ToString() == kind);

        private void CheckConstructed(SourceExpectation expected, string role, BlueprintAbility ability, BlueprintAbility harm,
            BlueprintFeature feat, BlueprintBuff fleeing)
        {
            string label = expected.Kind + " " + role;
            var dc = ability.GetComponents<ContextCalculateAbilityParamsBasedOnClass>().ToArray();
            if (dc.Length != 1 || dc[0].CharacterClass == null || dc[0].CharacterClass.AssetGuid != expected.ClassGuid ||
                dc[0].StatType != expected.Stat || dc[0].UseKineticistMainStat)
                problems.Add(label + ": DC calculator is not " + expected.ClassGuid + "/" + expected.Stat);
            var resource = ability.GetComponents<AbilityResourceLogic>().ToArray();
            if (resource.Length != 1 || resource[0].RequiredResource == null || resource[0].RequiredResource.AssetGuid != expected.Resource ||
                resource[0].Amount != expected.Cost || !resource[0].IsSpendResource)
                problems.Add(label + ": resource is not " + expected.Resource + " x" + expected.Cost);
            var around = ability.GetComponents<AbilityTargetsAround>().ToArray();
            AbilityTargetsAround native = harm.GetComponents<AbilityTargetsAround>().SingleOrDefault();
            if (around.Length != 1 || native == null || Math.Abs(around[0].AoERadius.Value - 30f) > 0.001f ||
                Math.Abs(around[0].AoERadius.Value - native.AoERadius.Value) > 0.001f || around[0].TargetType != native.TargetType)
                problems.Add(label + ": targeting is not the native 30-foot " + (native == null ? "?" : native.TargetType.ToString()) + " burst");
            else
            {
                var checker = (ConditionsChecker)typeof(AbilityTargetsAround).GetField("m_Condition", Any).GetValue(around[0]);
                Condition[] conditions = checker == null ? new Condition[0] : checker.Conditions ?? new Condition[0];
                if (conditions.Length != 1 || !(conditions[0] is ContextConditionHasFact) || conditions[0].Not ||
                    ((ContextConditionHasFact)conditions[0]).Fact == null || ((ContextConditionHasFact)conditions[0]).Fact.AssetGuid != "734a29b693e9ec346ba2951b27987e33")
                    problems.Add(label + ": target filter is not exactly 'has UndeadType'");
            }
            var casterFacts = ability.GetComponents<AbilityCasterHasFacts>().ToArray();
            if (casterFacts.Length != 2 || casterFacts.Any(c => c.NeedsAll || c.Facts == null || c.Facts.Length != 1) ||
                !casterFacts.Any(c => c.Facts[0] == feat) || !casterFacts.Any(c => c.Facts[0].AssetGuid == expected.Feature))
                problems.Add(label + ": caster restriction is not exactly (feat) and (" + expected.Feature + ")");
            bool alignment = ability.GetComponents<AbilityCasterAlignment>().Any();
            if (alignment != expected.LawfulGood) problems.Add(label + ": alignment restriction present=" + alignment + ", expected " + expected.LawfulGood);
            // Expected: the native harm structure. Top Conditional on CasterHasFact(Selective Channel fd30c694);
            // its IfTrue a Conditional on IsEnemy whose IfTrue is the save and IfFalse empty; its IfFalse the save.
            // Each save: ContextActionSavingThrow(Will) -> ConditionalSaved(Succeed empty, Failed ApplyBuff flight 1 min).
            var run = ability.GetComponents<AbilityEffectRunAction>().ToArray();
            string effectIssue = run.Length != 1 ? "effect count " + run.Length :
                run[0].SavingThrowType != Kingmaker.EntitySystem.Stats.SavingThrowType.Unknown ? "effect-level save " + run[0].SavingThrowType : null;
            var top = effectIssue == null && run[0].Actions.Actions.Length == 1 ? run[0].Actions.Actions[0] as Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional : null;
            if (effectIssue == null && top == null) effectIssue = "no single top Conditional";
            if (effectIssue == null)
            {
                var casterFact = top.ConditionsChecker.Conditions.Length == 1 ? top.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact : null;
                var enemy = top.IfTrue.Actions.Length == 1 ? top.IfTrue.Actions[0] as Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional : null;
                if (casterFact == null || casterFact.Not || casterFact.Fact == null || casterFact.Fact.AssetGuid != "fd30c69417b434d47b6b03b9c1f568ff")
                    effectIssue = "top condition is not CasterHasFact(Selective Channel)";
                else if (enemy == null || enemy.ConditionsChecker.Conditions.Length != 1 || !(enemy.ConditionsChecker.Conditions[0] is ContextConditionIsEnemy) ||
                    enemy.ConditionsChecker.Conditions[0].Not || enemy.IfFalse.Actions.Length != 0 || enemy.IfTrue.Actions.Length != 1 || top.IfFalse.Actions.Length != 1)
                    effectIssue = "selective branch is not IsEnemy -> save / nothing";
                else
                    foreach (GameAction branch in new[] { enemy.IfTrue.Actions[0], top.IfFalse.Actions[0] })
                    {
                        var save = branch as ContextActionSavingThrow;
                        var saved = save == null || save.Actions.Actions.Length != 1 ? null : save.Actions.Actions[0] as ContextActionConditionalSaved;
                        var apply = saved == null || saved.Failed.Actions.Length != 1 ? null : saved.Failed.Actions[0] as ContextActionApplyBuff;
                        if (save == null || save.Type != Kingmaker.EntitySystem.Stats.SavingThrowType.Will || saved == null || saved.Succeed.Actions.Length != 0 ||
                            apply == null || apply.Buff != fleeing || apply.DurationValue.Rate != DurationRate.Minutes || apply.DurationValue.BonusValue.Value != 1)
                            effectIssue = "a branch is not Will negates -> flight for 1 minute";
                    }
            }
            if (effectIssue != null) problems.Add(label + ": effect structure: " + effectIssue);
            if (ability.Type != AbilityType.Supernatural || ability.ActionType != UnitCommand.CommandType.Standard)
                problems.Add(label + ": not a supernatural standard action");
        }

        private static JObject DescribeAbility(BlueprintAbility ability)
        {
            AbilityTargetsAround around = ability.GetComponents<AbilityTargetsAround>().FirstOrDefault();
            AbilityResourceLogic resource = ability.GetComponents<AbilityResourceLogic>().FirstOrDefault();
            ContextCalculateAbilityParamsBasedOnClass dc = ability.GetComponents<ContextCalculateAbilityParamsBasedOnClass>().FirstOrDefault();
            return new JObject
            {
                ["guid"] = ability.AssetGuid, ["name"] = ability.name, ["type"] = ability.Type.ToString(), ["action"] = ability.ActionType.ToString(),
                ["parent"] = ability.Parent == null ? null : ability.Parent.AssetGuid, ["hidden"] = ability.Hidden,
                ["resource"] = resource == null ? null : (resource.RequiredResource == null ? "null" : resource.RequiredResource.AssetGuid) + " x" + resource.Amount,
                ["radiusFeet"] = around == null ? (JToken)null : around.AoERadius.Value, ["targetType"] = around == null ? null : around.TargetType.ToString(),
                ["dcCalculator"] = dc == null ? null : (dc.CharacterClass == null ? "null" : dc.CharacterClass.AssetGuid) + "/" + dc.StatType,
                ["components"] = new JArray(ability.ComponentsArray.Select(c => (object)(c.GetType().Name + ":" + c.name)).ToArray())
            };
        }

        private JToken DescribeCotw()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null)
            {
                var vanillaMenu = new JArray();
                foreach (string issue in MenuPlacementProblems(vanillaMenu)) problems.Add("vanilla menu placement: " + issue);
                return new JObject { ["status"] = "absent", ["turnMenu"] = vanillaMenu };
            }
            Type engineType = cotw.GetType("CallOfTheWild.ChannelEnergyEngine", false);
            var entries = new JArray();
            IEnumerable list = engineType == null ? null : engineType.GetField("channel_entires", Any)?.GetValue(null) as IEnumerable;
            var supported = new HashSet<string>(ExpectedSources.Select(e => e.Feature));
            var turnGuids = new HashSet<string>(ExpectedSources.SelectMany(e => new[] { Direct(e.Kind), (BlueprintAbility)Member(SourceOf(e.Kind), "MenuChild") }).Select(a => a.AssetGuid));
            var byType = new JObject();
            int allEntries = 0, turnEntries = 0;
            foreach (object entry in list ?? new object[0])
            {
                object anyType = entry.GetType().GetField("channel_type", Any)?.GetValue(entry);
                var anyAbility = entry.GetType().GetField("ability", Any)?.GetValue(entry) as BlueprintAbility;
                allEntries++;
                string typeName = anyType == null ? "null" : anyType.ToString();
                byType[typeName] = ((int?)byType[typeName] ?? 0) + 1;
                if (anyAbility != null && (turnGuids.Contains(anyAbility.AssetGuid) || (anyAbility.Parent != null && turnGuids.Contains(anyAbility.Parent.AssetGuid)))) turnEntries++;
            }
            if (turnEntries != 0) problems.Add(turnEntries + " CotW channel entries (of any form) are Turn identities");
            foreach (object entry in list ?? new object[0])
            {
                object type = entry.GetType().GetField("channel_type", Any)?.GetValue(entry);
                if (type == null || Convert.ToInt32(type) != 9) continue; // plain PositiveHarm entries
                var ability = entry.GetType().GetField("ability", Any)?.GetValue(entry) as BlueprintAbility;
                var parent = entry.GetType().GetField("parent_feature", Any)?.GetValue(entry) as BlueprintFeature;
                var wrapper = entry.GetType().GetField("base_ability", Any)?.GetValue(entry) as BlueprintAbility;
                AbilityResourceLogic entryResource = ability == null ? null : ability.GetComponents<AbilityResourceLogic>().FirstOrDefault();
                entries.Add(new JObject
                {
                    ["ability"] = ability == null ? null : ability.AssetGuid + ":" + ability.name,
                    ["resource"] = entryResource == null ? null : (entryResource.RequiredResource == null ? "null" : entryResource.RequiredResource.AssetGuid + ":" + entryResource.RequiredResource.name) + " x" + entryResource.Amount,
                    ["parentFeature"] = parent == null ? null : parent.AssetGuid + ":" + parent.name,
                    ["wrapper"] = wrapper == null ? null : wrapper.AssetGuid + ":" + wrapper.name,
                    ["turnUndeadSupport"] = parent != null && supported.Contains(parent.AssetGuid) ? "supported" : "unsupported (not a Turn Undead source)"
                });
            }
            var menu = new JArray();
            List<string> placement = MenuPlacementProblems(menu);
            foreach (string issue in placement) problems.Add("CotW menu placement: " + issue);
            var holders = new JObject();
            foreach (string field in new[] { "improved_channel", "sacred_conduit", "consecrate_buff" })
                holders[field] = DumpFact(engineType == null ? null : engineType.GetField(field, Any)?.GetValue(null) as BlueprintScriptableObject);
            var membership = new JObject();
            foreach (string field in new[] { "improved_channel", "sacred_conduit", "consecrate_buff" })
            {
                var holder = engineType == null ? null : engineType.GetField(field, Any)?.GetValue(null) as BlueprintUnitFact;
                if (holder == null) { membership[field] = "absent"; continue; }
                int count = holder.ComponentsArray.Where(c => c != null && c.GetType().Name == "IncreaseSpecifiedSpellsDC")
                    .Sum(c => ((IEnumerable)c.GetType().GetField("spells", Any).GetValue(c)).Cast<object>().OfType<BlueprintAbility>().Count(a => turnGuids.Contains(a.AssetGuid)));
                membership[field] = count;
                if (count != turnGuids.Count) problems.Add("CotW " + field + " lists " + count + " Turn identities, expected " + turnGuids.Count);
            }
            foreach (BlueprintUnitFact fact in ((engineType == null ? null : engineType.GetField("channel_resistances", Any)?.GetValue(null)) as IEnumerable<BlueprintUnitFact> ?? new BlueprintUnitFact[0]))
            {
                int count = fact.ComponentsArray.Where(c => c != null && (c.GetType().Name == "SavingThrowBonusAgainstSpecificSpells" || c.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells"))
                    .Sum(c => ((IEnumerable)c.GetType().GetField("Spells", Any).GetValue(c)).Cast<object>().OfType<BlueprintAbility>().Count(a => turnGuids.Contains(a.AssetGuid)));
                membership["resistance:" + fact.AssetGuid + ":" + fact.name] = count;
                if (count != turnGuids.Count) problems.Add("CotW resistance " + fact.name + " lists " + count + " Turn identities, expected " + turnGuids.Count);
            }
            var resistances = new JArray(((engineType == null ? null : engineType.GetField("channel_resistances", Any)?.GetValue(null)) as IEnumerable<BlueprintUnitFact> ?? new BlueprintUnitFact[0])
                .Select(f => (object)DumpFact(f)).ToArray());
            return new JObject
            {
                ["assemblyVersion"] = cotw.GetName().Version.ToString(), ["sha256"] = Sha256File(cotw.Location),
                ["channelEntries"] = allEntries, ["channelEntriesByType"] = byType, ["channelEntriesThatAreTurn"] = turnEntries,
                ["positiveHarmEntries"] = entries, ["turnMenu"] = menu, ["dcHolders"] = holders, ["channelResistances"] = resistances, ["turnMembership"] = membership
            };
        }

        // ---------------------------------------------------------------- undead caster

        private IEnumerable<object> UndeadCaster()
        {
            SilenceArea();
            BlueprintAbility clericTurn = Direct("Cleric");
            turnSource = clericTurn.AssetGuid;
            Game.Instance.IsPaused = true;
            UnitEntityData cleric = SpawnFriendly("UndeadCleric", origin);
            cleric.Descriptor.Stats.Charisma.BaseValue = 16;
            Level(cleric, Blueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0"), 5);
            var channel = Blueprint<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            if (!cleric.Descriptor.HasFact(channel)) cleric.Descriptor.AddFact(channel);
            cleric.Descriptor.AddFact(Feat);
            cleric.Descriptor.AddFact(Blueprint<BlueprintUnitFact>(UndeadType));
            cleric.Stats.SaveWill.BaseValue = 100;
            UnitEntityData skeleton = SpawnNative(Skeleton, "skeleton", Spot(3f, 0f));
            skeleton.Stats.SaveWill.BaseValue = -100;
            UnitEntityData living = SpawnFriendly("LivingControl", Spot(-2f, 2f));
            yield return null;
            int expectedDc = 10 + 5 / 2 + cleric.Stats.Charisma.Bonus;
            int seq = Seq;
            var command = new UnitUseAbility(new AbilityData(cleric.Descriptor.Abilities.GetAbility(clericTurn)), new TargetWrapper(cleric));
            command.IgnoreCooldown(null);
            cleric.Commands.InterruptAll(true);
            cleric.Commands.Run(command);
            foreach (object step in Settle(0.0, 30f, () => command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return step;
            JArray clericSaves = Saves(cleric, seq), skeletonSaves = Saves(skeleton, seq), livingSaves = Saves(living, seq);
            result["undeadCaster"] = new JObject
            {
                ["clericUndead"] = cleric.Descriptor.HasFact(Blueprint<BlueprintUnitFact>(UndeadType)), ["commandResult"] = command.Result.ToString(),
                ["expectedDc"] = expectedDc, ["clericSaves"] = clericSaves, ["skeletonSaves"] = skeletonSaves, ["livingControlSaves"] = livingSaves,
                ["clericFleeing"] = FleeingOn(cleric) != null, ["skeletonFleeing"] = FleeingOn(skeleton) != null
            };
            if (clericSaves.Count != 1 || (string)clericSaves[0]["type"] != "Will" || (int)clericSaves[0]["dc"] != expectedDc || !(bool)clericSaves[0]["passed"])
                problems.Add("the undead channeler did not make exactly one passed Will save at DC " + expectedDc + " against its own Turn: " + clericSaves.ToString(Newtonsoft.Json.Formatting.None));
            if (skeletonSaves.Count != 1 || FleeingOn(skeleton) == null) problems.Add("the skeleton beside the undead channeler was not turned");
            if (livingSaves.Count != 0 || FleeingOn(living) != null) problems.Add("the living control was affected");
            if (FleeingOn(cleric) != null) problems.Add("the undead channeler is fleeing although it saved");
        }

        // ---------------------------------------------------------------- Selective Channel and allied undead

        private UnitEntityData SpawnAlliedUndead(string label, Vector3 at)
        {
            BlueprintUnit clone = UnityEngine.Object.Instantiate(Blueprint<BlueprintUnit>(Skeleton));
            clone.name = "KmgTurnUndead_Allied_" + label + "_" + (++unitIndex);
            clone.Faction = BlueprintRoot.Instance.PlayerFaction;
            clones.Add(clone);
            return Track(Game.Instance.EntityCreator.SpawnUnit(clone, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState), label);
        }

        private IEnumerable<object> Selective()
        {
            SilenceArea();
            BlueprintAbility clericTurn = Direct("Cleric");
            turnSource = clericTurn.AssetGuid;
            Game.Instance.IsPaused = true;
            var selectiveFact = Blueprint<BlueprintUnitFact>("fd30c69417b434d47b6b03b9c1f568ff");
            var cases = new JArray();
            result["selective"] = cases;
            foreach (bool selective in new[] { false, true })
            {
                float side = selective ? 6f : -6f;
                UnitEntityData cleric = SpawnFriendly(selective ? "SelectiveCleric" : "PlainCleric", Spot(0f, side));
                cleric.Descriptor.Stats.Charisma.BaseValue = 16;
                Level(cleric, Blueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0"), 5);
                var channel = Blueprint<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
                if (!cleric.Descriptor.HasFact(channel)) cleric.Descriptor.AddFact(channel);
                cleric.Descriptor.AddFact(Feat);
                if (selective) cleric.Descriptor.AddFact(selectiveFact);
                UnitEntityData enemy = SpawnNative(Skeleton, "enemy skeleton", Spot(2.5f, side + 1.5f));
                UnitEntityData ally = SpawnAlliedUndead("allied skeleton", Spot(2.5f, side - 1.5f));
                enemy.Stats.SaveWill.BaseValue = -100;
                ally.Stats.SaveWill.BaseValue = -100;
                yield return null;
                int seq = Seq;
                var command = new UnitUseAbility(new AbilityData(cleric.Descriptor.Abilities.GetAbility(clericTurn)), new TargetWrapper(cleric));
                command.IgnoreCooldown(null);
                cleric.Commands.InterruptAll(true);
                cleric.Commands.Run(command);
                foreach (object step in Settle(0.0, 30f, () => command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return step;
                JArray enemySaves = Saves(enemy, seq), allySaves = Saves(ally, seq);
                var row = new JObject
                {
                    ["selectiveChannel"] = cleric.Descriptor.HasFact(selectiveFact), ["commandResult"] = command.Result.ToString(),
                    ["allyIsEnemy"] = ally.IsEnemy(cleric), ["enemyIsEnemy"] = enemy.IsEnemy(cleric),
                    ["enemySaves"] = enemySaves, ["allySaves"] = allySaves,
                    ["enemyFleeing"] = FleeingOn(enemy) != null, ["allyFleeing"] = FleeingOn(ally) != null,
                    ["enemyDistance"] = Math.Round(Vector3.Distance(enemy.Position, cleric.Position), 2),
                    ["allyDistance"] = Math.Round(Vector3.Distance(ally.Position, cleric.Position), 2)
                };
                cases.Add(row);
                string label = selective ? "with Selective Channel" : "without Selective Channel";
                if (ally.IsEnemy(cleric) || !enemy.IsEnemy(cleric)) problems.Add(label + ": fixture allegiance is wrong");
                if (enemySaves.Count != 1 || FleeingOn(enemy) == null) problems.Add(label + ": the enemy skeleton was not turned");
                if (selective && (allySaves.Count != 0 || FleeingOn(ally) != null)) problems.Add(label + ": the allied skeleton was affected or rolled a save");
                if (!selective && (allySaves.Count != 1 || FleeingOn(ally) == null)) problems.Add(label + ": the allied skeleton was not turned (all-undead behavior)");
            }
        }

        // ---------------------------------------------------------------- abandoned probe

        private IEnumerable<object> Abandon()
        {
            Game.Instance.IsPaused = false;
            bool prior = Game.Instance.IsPaused;
            var nested = new TurnUndeadProbe("first-turn") { injectCleanupFailure = injectNestedCleanupFailureForTest };
            abandonNested = nested;
            string verdict = null;
            int polls = 0;
            while (verdict == null && !(nested.silenced.Count > 0 && Game.Instance.IsPaused) && polls++ < 600)
            {
                verdict = nested.Poll();
                if (verdict == null) yield return null;
            }
            bool suspended = verdict == null && nested.silenced.Count > 0 && Game.Instance.IsPaused;
            UnitEntityData[] silencedUnits = nested.silenced.ToArray();
            UnitEntityData[] spawned = nested.created.ToArray();
            abandonNested = null;
            CleanupReport report = nested.Release("abandoned by the abandon check");
            // UnitEntityData.Destroy completes on a later frame; wait for it before judging.
            int frames = 0;
            while (frames < 30 && spawned.Any(u => u != null && !u.Destroyed)) { frames++; yield return null; }
            result["abandon"] = new JObject
            {
                ["suspendedAfterPauseAndSilence"] = suspended, ["polls"] = polls, ["priorPause"] = prior,
                ["pauseAfterRelease"] = Game.Instance.IsPaused, ["silenced"] = silencedUnits.Length,
                ["brainsRestored"] = silencedUnits.Count(u => u != null && !u.Destroyed && u.IsBrainActive),
                ["spawned"] = spawned.Length, ["spawnedRemaining"] = spawned.Count(u => u != null && !u.Destroyed), ["framesUntilDestroyed"] = frames,
                ["cleanupAttempted"] = new JArray(report.Attempted.Cast<object>().ToArray()),
                ["cleanupFailures"] = new JArray(report.Failures.Cast<object>().ToArray()), ["cleanupRestored"] = report.Restored,
                ["nestedCleanupScope"] = nested.result["cleanupScope"]
            };
            if (!suspended) problems.Add("the nested probe was not suspended after pausing and silencing (verdict " + (verdict ?? "none") + ")");
            if (Game.Instance.IsPaused != prior) problems.Add("Release did not restore the prior pause flag");
            if (silencedUnits.Any(u => u != null && !u.Destroyed && !u.IsBrainActive)) problems.Add("Release left silenced brains off");
            if (spawned.Any(u => u != null && !u.Destroyed)) problems.Add("Release left spawned units");
            if (!report.Restored) problems.Add("the cleanup verdict reported failures: " + string.Join("; ", report.Failures.ToArray()));
        }

        // ---------------------------------------------------------------- hashing

        private static string Sha256(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private static string Sha256File(string path)
        {
            try { return Sha256(File.ReadAllBytes(path)); }
            catch (Exception e) { return "unreadable: " + e.GetType().Name; }
        }
    }
}
