using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
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
    ///    (method IL hashes for the DC, save-reason, targeting and fear routes; a compact projection of the
    ///    native donors, source grants, resources, costs, radius, target filter, DC calculators, every native save
    ///    with its conditional DC adjustments; the channel resistance list; CotW channel registry, wrapper
    ///    membership and modifier holders when CotW is loaded), and an assertion of every constructed Turn
    ///    component against literal expectations written here independently of the product catalog. Only the
    ///    fields the contract needs are projected: no recursive component dumps (they stay in the lab).
    ///  * "undead-caster": a true undead channeler receives the initial save from its own Turn; a living control
    ///    in range does not.
    ///  * "selective": the same cast with and without the native Selective Channel feat beside an allied and an
    ///    enemy skeleton.
    ///  * "abandon": a nested first-turn probe (an owned child) is released while suspended after it paused the
    ///    game and silenced the area; the prior pause flag, every brain and every spawned unit must be restored,
    ///    and the child's cleanup report becomes part of this probe's own.
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
        // Call of the Wild 1.14.4c: the paladin's extra-channel harm, its pool, and the prestige classes whose levels
        // CotW adds to every positive channel's DC progression (Holy Vindicator, Dawnflower Anchorite).
        private const string CotwPaladinExtraHarm = "c2d7e09fb9484d09932e62b6ed8339d1";
        private const string CotwPaladinExtraResource = "e5995b0657a841f993052fdb1cc566dd";
        private static readonly string[] CotwProgressionClasses = { "88463e2b13924071a0f89b4b59591f38", "3bdfed13c1b747b38b4193faf0213423" };
        private const string BasicFeatSelection = "247a4068296e8be42890143f451b4b45";
        private const string ChannelResistanceFour = "a9ac84c6f48b491438f91bb237bc9212";
        private const string NativeFrightenedUndead = "1265c0b27ef559c4e980a900dfc68d1e";
        private const string SelectiveChannel = "fd30c69417b434d47b6b03b9c1f568ff";

        private static readonly string[][] ContractMethods =
        {
            new[] { "RuleCalculateAbilityParams", "OnTrigger", "DC route: caster level = spellbook CL or character level; DC = 10 + stat bonus + spell level (+ bonuses)" },
            new[] { "ContextCalculateAbilityParamsBasedOnClass", "Calculate", "replaces caster level with the class level and spell level with half of it, and the statistic" },
            new[] { "AbilityData", "CalculateParams", "preview path: triggers RuleCalculateAbilityParams without ability calculators" },
            new[] { "ContextActionSavingThrow", "RunAction", "save DC = context DC plus each conditional DC increase whose condition holds" },
            new[] { "SavingThrowBonusAgainstSpecificSpells", "OnEventAboutToTrigger", "channel resistance matches Reason.Context.SourceAbility" },
            new[] { "BuffStatusCondition", "OnNewRound", "native fear buffs re-save each round with Reason = the buff fact" },
            new[] { "AbilityTargetsAround", "Select", "all units within the radius with line of sight, filtered by target type and condition; includes the caster" },
            new[] { "AbilityCasterHasFacts", "CorrectCaster", "caster restriction by facts (NeedsAll tests only the last fact)" },
            new[] { "AddFeatureIfHasFact", "OnFactActivate", "grants the feature on activation when the checked fact is present" },
            new[] { "AddFeatureIfHasFact", "HandleUnitGainLevel", "re-checks on every level-up; never removes" },
            new[] { "UnitFearController", "TickOnUnit", "Frightened units flee from visible enemies" }
        };

        private static bool CotwLoaded => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "CallOfTheWild");

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
            bool cotw = CotwLoaded;
            var sources = new JArray();
            foreach (SourceExpectation expected in ExpectedSources)
            {
                object source = SourceOf(expected.Kind);
                var feature = Blueprint<BlueprintFeature>(expected.Feature);
                var harm = Blueprint<BlueprintAbility>(expected.NativeHarm);
                var row = new JObject
                {
                    ["kind"] = expected.Kind,
                    ["sourceFeature"] = feature.AssetGuid + ":" + feature.name,
                    ["sourceGrants"] = new JArray(feature.GetComponents<AddFacts>().SelectMany(a => a.Facts ?? new BlueprintUnitFact[0])
                        .Select(f => (object)(f.AssetGuid + ":" + f.name)).ToArray()),
                    ["nativeHarm"] = AbilityContract(harm)
                };
                string[] classes = cotw ? new[] { expected.ClassGuid }.Concat(CotwProgressionClasses).ToArray() : new[] { expected.ClassGuid };
                foreach (string role in new[] { "Direct", "MenuChild" })
                {
                    var ability = (BlueprintAbility)Member(source, role);
                    row[role] = DescribeAbility(ability);
                    CheckConstructed(expected.Kind + " " + role, ability, harm, feat, fleeing, expected.Feature, expected.Resource,
                        expected.Cost, classes, expected.Stat, expected.LawfulGood, role == "MenuChild");
                }
                if (expected.Kind == "Paladin")
                {
                    var extra = (BlueprintAbility)Member(source, "ExtraMenuChild");
                    var extraHarm = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(CotwPaladinExtraHarm);
                    row["extraRoute"] = extra == null ? (JToken)"absent" : DescribeAbility(extra);
                    if (extraHarm != null) row["extraRouteDonor"] = AbilityContract(extraHarm);
                    if (cotw != (extra != null)) problems.Add("the CotW paladin extra-channel child is " + (extra == null ? "absent" : "present") + " with CotW " + (cotw ? "loaded" : "absent"));
                    if (extra != null)
                        CheckConstructed("Paladin extra-channel MenuChild", extra, extraHarm, feat, fleeing, expected.Feature, CotwPaladinExtraResource,
                            1, classes, expected.Stat, true, true);
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

            // Flight effect and the native channel resistance list (exact per-identity membership).
            var nativeFear = Blueprint<BlueprintBuff>(NativeFrightenedUndead);
            result["flight"] = new JObject
            {
                ["guid"] = fleeing.AssetGuid, ["stacking"] = fleeing.Stacking.ToString(), ["nativeDonorStacking"] = nativeFear.Stacking.ToString(),
                ["components"] = new JArray(fleeing.ComponentsArray.Select(c => (object)(c.GetType().Name + ":" + c.name)).ToArray()),
                ["descriptors"] = new JArray(fleeing.ComponentsArray.OfType<SpellDescriptorComponent>().Select(d => (object)d.Descriptor.Value.ToString()).ToArray())
            };
            var resistance = Blueprint<BlueprintFeature>(ChannelResistanceFour);
            BlueprintAbility[] spells = resistance.GetComponents<SavingThrowBonusAgainstSpecificSpells>().SelectMany(c => c.Spells ?? new BlueprintAbility[0]).ToArray();
            string[] turnGuids = TurnIdentities().Select(a => a.AssetGuid).ToArray();
            List<string> resistanceMembership = TurnUndeadObserverRules.MembershipProblems("ChannelResistance4", turnGuids,
                spells.Where(a => a != null && turnGuids.Contains(a.AssetGuid)).Select(a => a.AssetGuid));
            result["channelResistance4"] = new JObject
            {
                ["components"] = resistance.GetComponents<SavingThrowBonusAgainstSpecificSpells>().Count(),
                ["entries"] = spells.Length, ["nativeHarmEntries"] = ExpectedSources.Count(e => spells.Any(a => a != null && a.AssetGuid == e.NativeHarm)),
                ["turnIdentities"] = turnGuids.Length, ["membershipProblems"] = new JArray(resistanceMembership.Cast<object>().ToArray())
            };
            problems.AddRange(resistanceMembership);

            // Compact projection of every native blueprint the design relies on.
            var empyreal = Blueprint<BlueprintFeature>("7d49d7f590dc9a948b3bd1c8b7979854");
            result["nativeContract"] = new JObject
            {
                ["harmDonors"] = new JArray(ExpectedSources.Select(e => (object)AbilityContract(Blueprint<BlueprintAbility>(e.NativeHarm))).ToArray()),
                ["sourceFeatures"] = new JArray(ExpectedSources.Select(e => (object)FactContract(Blueprint<BlueprintFeature>(e.Feature))).ToArray()),
                ["frightenedUndeadDonor"] = FactContract(nativeFear),
                ["wizardTurnUndead"] = AbilityContract(ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("71b8898b1d26d654b9a3eeac87e3e2f8")),
                ["undeadType"] = FactContract(Blueprint<BlueprintUnitFact>(UndeadType)),
                ["undeadImmunities"] = FactContract(Blueprint<BlueprintUnitFact>("8a75eb16bfff86949a4ddcb3dd2f83ae")),
                ["empyrealDcOverride"] = new JArray(empyreal.ComponentsArray.Where(c => c != null && c.GetType().Name == "ReplaceAbilityDC").Select(c => (object)Shallow(c)).ToArray()),
                ["channelResistance4"] = FactContract(resistance),
                ["turnFlight"] = FactContract(fleeing)
            };

            // Call of the Wild, when loaded: channel registry, Turn wrapper membership and the modifier holders.
            result["callOfTheWild"] = DescribeCotw();
            yield break;
        }

        private object SourceOf(string kind) =>
            ((Array)Member(set, "Sources")).Cast<object>().Single(s => Member(Member(s, "Contract"), "Kind").ToString() == kind);

        /// <summary>Every Turn identity the product registered (direct actions, menu children, the CotW extra child).</summary>
        private List<BlueprintAbility> TurnIdentities()
        {
            var all = new List<BlueprintAbility>();
            foreach (SourceExpectation e in ExpectedSources)
            {
                object source = SourceOf(e.Kind);
                all.Add((BlueprintAbility)Member(source, "Direct"));
                all.Add((BlueprintAbility)Member(source, "MenuChild"));
                var extra = (BlueprintAbility)Member(source, "ExtraMenuChild");
                if (extra != null) all.Add(extra);
            }
            return all;
        }

        /// <summary>The DC classes and statistic of a native or CotW calculator, read by reflection; null when unknown.</summary>
        private static string[] CalculatorShape(BlueprintComponent component, out StatType stat, out bool fixedStat)
        {
            stat = StatType.Unknown;
            fixedStat = false;
            var native = component as ContextCalculateAbilityParamsBasedOnClass;
            if (native != null)
            {
                stat = native.StatType;
                fixedStat = !native.UseKineticistMainStat;
                return native.CharacterClass == null ? new string[0] : new[] { native.CharacterClass.AssetGuid };
            }
            if (component == null || component.GetType().Name != "ContextCalculateAbilityParamsBasedOnClasses") return null;
            Type type = component.GetType();
            stat = (StatType)type.GetField("StatType", Any).GetValue(component);
            fixedStat = !(bool)type.GetField("use_kineticist_main_stat", Any).GetValue(component) && type.GetField("property", Any).GetValue(component) == null;
            return ((BlueprintCharacterClass[])type.GetField("CharacterClasses", Any).GetValue(component) ?? new BlueprintCharacterClass[0])
                .Select(c => c == null ? "null" : c.AssetGuid).ToArray();
        }

        private void CheckConstructed(string label, BlueprintAbility ability, BlueprintAbility harm, BlueprintFeature feat, BlueprintBuff fleeing,
            string sourceFeature, string resourceGuid, int cost, string[] dcClasses, StatType dcStat, bool lawfulGood, bool menuChild)
        {
            BlueprintComponent[] calculators = ability.ComponentsArray.Where(c => c is ContextAbilityParamsCalculator).ToArray();
            StatType stat;
            bool fixedStat;
            string[] classes = calculators.Length == 1 ? CalculatorShape(calculators[0], out stat, out fixedStat) : null;
            if (calculators.Length != 1 || classes == null || !classes.SequenceEqual(dcClasses) || calculators.Length == 1 && (CalculatorShape(calculators[0], out stat, out fixedStat) == null || stat != dcStat || !fixedStat))
                problems.Add(label + ": DC calculator is not " + string.Join("+", dcClasses) + "/" + dcStat);
            var resource = ability.GetComponents<AbilityResourceLogic>().ToArray();
            if (resource.Length != 1 || resource[0].RequiredResource == null || resource[0].RequiredResource.AssetGuid != resourceGuid ||
                resource[0].Amount != cost || !resource[0].IsSpendResource)
                problems.Add(label + ": resource is not " + resourceGuid + " x" + cost);
            var around = ability.GetComponents<AbilityTargetsAround>().ToArray();
            AbilityTargetsAround native = harm == null ? null : harm.GetComponents<AbilityTargetsAround>().SingleOrDefault();
            if (around.Length != 1 || native == null || Math.Abs(around[0].AoERadius.Value - 30f) > 0.001f ||
                Math.Abs(around[0].AoERadius.Value - native.AoERadius.Value) > 0.001f || around[0].TargetType != native.TargetType)
                problems.Add(label + ": targeting is not the native 30-foot " + (native == null ? "?" : native.TargetType.ToString()) + " burst");
            else
            {
                var checker = (ConditionsChecker)typeof(AbilityTargetsAround).GetField("m_Condition", Any).GetValue(around[0]);
                Condition[] conditions = checker == null ? new Condition[0] : checker.Conditions ?? new Condition[0];
                if (conditions.Length != 1 || !(conditions[0] is ContextConditionHasFact) || conditions[0].Not ||
                    ((ContextConditionHasFact)conditions[0]).Fact == null || ((ContextConditionHasFact)conditions[0]).Fact.AssetGuid != UndeadType)
                    problems.Add(label + ": target filter is not exactly 'has UndeadType'");
            }
            var casterFacts = ability.GetComponents<AbilityCasterHasFacts>().ToArray();
            if (casterFacts.Length != 2 || casterFacts.Any(c => c.NeedsAll || c.Facts == null || c.Facts.Length != 1) ||
                !casterFacts.Any(c => c.Facts[0] == feat) || !casterFacts.Any(c => c.Facts[0].AssetGuid == sourceFeature))
                problems.Add(label + ": caster restriction is not exactly (feat) and (" + sourceFeature + ")");
            int shows = ability.GetComponents<AbilityShowIfCasterHasFact>().Count();
            if (shows != (menuChild ? 2 : 0)) problems.Add(label + ": " + shows + " show-if conditions");
            bool alignment = ability.GetComponents<AbilityCasterAlignment>().Any();
            if (alignment != lawfulGood) problems.Add(label + ": alignment restriction present=" + alignment + ", expected " + lawfulGood);
            // Expected: the native harm structure. Top Conditional on CasterHasFact(Selective Channel fd30c694);
            // its IfTrue a Conditional on IsEnemy whose IfTrue is the save and IfFalse empty; its IfFalse the save.
            // Each save: ContextActionSavingThrow(Will, no conditional DC increase) -> ConditionalSaved(Succeed empty,
            // Failed ApplyBuff flight 1 min).
            var run = ability.GetComponents<AbilityEffectRunAction>().ToArray();
            string effectIssue = run.Length != 1 ? "effect count " + run.Length :
                run[0].SavingThrowType != SavingThrowType.Unknown ? "effect-level save " + run[0].SavingThrowType : null;
            var top = effectIssue == null && run[0].Actions.Actions.Length == 1 ? run[0].Actions.Actions[0] as Conditional : null;
            if (effectIssue == null && top == null) effectIssue = "no single top Conditional";
            if (effectIssue == null)
            {
                var casterFact = top.ConditionsChecker.Conditions.Length == 1 ? top.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact : null;
                var enemy = top.IfTrue.Actions.Length == 1 ? top.IfTrue.Actions[0] as Conditional : null;
                if (casterFact == null || casterFact.Not || casterFact.Fact == null || casterFact.Fact.AssetGuid != SelectiveChannel)
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
                        if (save == null || save.Type != SavingThrowType.Will || saved == null || saved.Succeed.Actions.Length != 0 ||
                            apply == null || apply.Buff != fleeing || apply.DurationValue.Rate != DurationRate.Minutes || apply.DurationValue.BonusValue.Value != 1)
                            effectIssue = "a branch is not Will negates -> flight for 1 minute";
                        else if (ConditionalIncreases(save).Count != 0)
                            effectIssue = "a Turn save carries conditional DC increases";
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
            BlueprintComponent calculator = ability.ComponentsArray.FirstOrDefault(c => c is ContextAbilityParamsCalculator);
            StatType stat;
            bool fixedStat;
            string[] classes = calculator == null ? null : CalculatorShape(calculator, out stat, out fixedStat);
            CalculatorShape(calculator, out stat, out fixedStat);
            return new JObject
            {
                ["guid"] = ability.AssetGuid, ["name"] = ability.name, ["type"] = ability.Type.ToString(), ["action"] = ability.ActionType.ToString(),
                ["parent"] = ability.Parent == null ? null : ability.Parent.AssetGuid, ["hidden"] = ability.Hidden,
                ["resource"] = resource == null ? null : (resource.RequiredResource == null ? "null" : resource.RequiredResource.AssetGuid) + " x" + resource.Amount,
                ["radiusFeet"] = around == null ? (JToken)null : around.AoERadius.Value, ["targetType"] = around == null ? null : around.TargetType.ToString(),
                ["dcCalculator"] = calculator == null ? null : calculator.GetType().Name + ":" + (classes == null ? "unknown" : string.Join("+", classes)) + "/" + stat,
                ["components"] = new JArray(ability.ComponentsArray.Select(c => (object)(c.GetType().Name + ":" + c.name)).ToArray())
            };
        }

        // ---------------------------------------------------------------- compact projections

        private static JToken Ref(BlueprintScriptableObject value) => value == null ? null : (JToken)(value.AssetGuid + ":" + value.name);

        /// <summary>Blueprint references: listed when short, otherwise their count and a SHA-256 of the GUID list.</summary>
        private static JToken RefList(IEnumerable<BlueprintScriptableObject> values)
        {
            BlueprintScriptableObject[] list = values.ToArray();
            if (list.Length <= 16) return new JArray(list.Select(v => (object)(v == null ? null : v.AssetGuid + ":" + v.name)).ToArray());
            return new JObject
            {
                ["count"] = list.Length,
                ["guidListSha256"] = Sha256(Encoding.UTF8.GetBytes(string.Join(",", list.Select(v => v == null ? "null" : v.AssetGuid).ToArray())))
            };
        }

        /// <summary>
        /// A component's own serialized scalar, enum and blueprint-reference fields, one level deep: nested objects
        /// (action lists, prefab links, structs) are named by type only, never expanded.
        /// </summary>
        private static JObject Shallow(object component)
        {
            var o = new JObject { ["$type"] = component.GetType().Name };
            for (Type t = component.GetType(); t != null && t != typeof(BlueprintComponent) && t != typeof(ScriptableObject) && t != typeof(object); t = t.BaseType)
                foreach (FieldInfo field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsNotSerialized || field.Name.Contains("<") || o[field.Name] != null) continue;
                    if (!field.IsPublic && field.GetCustomAttributes(typeof(SerializeField), true).Length == 0) continue;
                    object value;
                    try { value = field.GetValue(component); }
                    catch (Exception e) { o[field.Name] = "unreadable: " + e.GetType().Name; continue; }
                    if (value == null) o[field.Name] = null;
                    else if (value is Enum) o[field.Name] = value.ToString();
                    else if (value is string || value is bool || value is int || value is float || value is double || value is long) o[field.Name] = JToken.FromObject(value);
                    else if (value is BlueprintScriptableObject) o[field.Name] = Ref((BlueprintScriptableObject)value);
                    else if (value is Feet) o[field.Name] = ((Feet)value).Value + " ft";
                    else if (value is IEnumerable)
                    {
                        object[] items = ((IEnumerable)value).Cast<object>().ToArray();
                        o[field.Name] = items.All(i => i == null || i is BlueprintScriptableObject)
                            ? RefList(items.Cast<BlueprintScriptableObject>())
                            : (JToken)(items.Length + " x " + value.GetType().Name);
                    }
                    else o[field.Name] = value.GetType().Name;
                }
            return o;
        }

        private static JObject FactContract(BlueprintScriptableObject fact)
        {
            if (fact == null) return null;
            return new JObject
            {
                ["guid"] = fact.AssetGuid, ["name"] = fact.name,
                ["components"] = new JArray(fact.ComponentsArray.Where(c => c != null).Select(c => (object)Shallow(c)).ToArray())
            };
        }

        private static string ConditionText(Condition condition)
        {
            FieldInfo fact = condition.GetType().GetFields(Any).FirstOrDefault(f => typeof(BlueprintScriptableObject).IsAssignableFrom(f.FieldType));
            var target = fact == null ? null : fact.GetValue(condition) as BlueprintScriptableObject;
            return (condition.Not ? "not " : "") + condition.GetType().Name + (target == null ? "" : "(" + target.AssetGuid + ")");
        }

        private static string CheckerText(ConditionsChecker checker) =>
            checker == null || checker.Conditions == null ? "always" : checker.Operation + "[" + string.Join(", ", checker.Conditions.Select(ConditionText).ToArray()) + "]";

        /// <summary>The conditional DC increases of a native save (private struct array): count, conditions and values.</summary>
        private static JArray ConditionalIncreases(ContextActionSavingThrow save)
        {
            var increases = new JArray();
            var array = typeof(ContextActionSavingThrow).GetField("m_ConditionalDCIncrease", Any).GetValue(save) as Array;
            foreach (object entry in array ?? new object[0])
            {
                Type type = entry.GetType();
                var condition = type.GetField("Condition", Any).GetValue(entry) as ConditionsChecker;
                object value = type.GetField("Value", Any).GetValue(entry);
                increases.Add(new JObject { ["condition"] = CheckerText(condition), ["value"] = value == null ? null : Shallow(value) });
            }
            return increases;
        }

        /// <summary>Every save in an action tree with the branch path that reaches it.</summary>
        private static void SavesIn(ActionList list, string path, JArray into)
        {
            foreach (GameAction action in list == null || list.Actions == null ? new GameAction[0] : list.Actions)
            {
                if (action == null) continue;
                var conditional = action as Conditional;
                var save = action as ContextActionSavingThrow;
                var saved = action as ContextActionConditionalSaved;
                if (conditional != null)
                {
                    SavesIn(conditional.IfTrue, path + " / if " + CheckerText(conditional.ConditionsChecker), into);
                    SavesIn(conditional.IfFalse, path + " / unless " + CheckerText(conditional.ConditionsChecker), into);
                }
                else if (save != null)
                {
                    JArray increases = ConditionalIncreases(save);
                    into.Add(new JObject
                    {
                        ["path"] = path.TrimStart(' ', '/'), ["type"] = save.Type.ToString(),
                        ["conditionalDcIncreaseCount"] = increases.Count, ["conditionalDcIncreases"] = increases,
                        ["onResult"] = new JArray((save.Actions == null || save.Actions.Actions == null ? new GameAction[0] : save.Actions.Actions)
                            .Where(a => a != null).Select(a => (object)a.GetType().Name).ToArray())
                    });
                    SavesIn(save.Actions, path + " / after save", into);
                }
                else if (saved != null)
                {
                    SavesIn(saved.Succeed, path + " / on success", into);
                    SavesIn(saved.Failed, path + " / on failure", into);
                }
            }
        }

        private static JObject AbilityContract(BlueprintAbility ability)
        {
            if (ability == null) return null;
            AbilityResourceLogic resource = ability.GetComponents<AbilityResourceLogic>().FirstOrDefault();
            AbilityTargetsAround around = ability.GetComponents<AbilityTargetsAround>().FirstOrDefault();
            ConditionsChecker filter = around == null ? null : (ConditionsChecker)typeof(AbilityTargetsAround).GetField("m_Condition", Any).GetValue(around);
            AbilityEffectRunAction run = ability.GetComponents<AbilityEffectRunAction>().FirstOrDefault();
            var saves = new JArray();
            if (run != null) SavesIn(run.Actions, "", saves);
            return new JObject
            {
                ["guid"] = ability.AssetGuid, ["name"] = ability.name, ["type"] = ability.Type.ToString(), ["action"] = ability.ActionType.ToString(),
                ["parent"] = Ref(ability.Parent), ["hidden"] = ability.Hidden,
                ["resource"] = resource == null ? null : (resource.RequiredResource == null ? "null" : resource.RequiredResource.AssetGuid + ":" + resource.RequiredResource.name) +
                    " x" + resource.Amount + (resource.IsSpendResource ? " spend" : " no-spend"),
                ["targets"] = around == null ? null : new JObject { ["radius"] = around.AoERadius.Value + " ft", ["targetType"] = around.TargetType.ToString(), ["filter"] = CheckerText(filter) },
                ["effectSave"] = run == null ? null : run.SavingThrowType.ToString(),
                ["saves"] = saves,
                ["calculators"] = new JArray(ability.ComponentsArray.Where(c => c is ContextAbilityParamsCalculator).Select(c => (object)Shallow(c)).ToArray()),
                ["alignment"] = new JArray(ability.GetComponents<AbilityCasterAlignment>().Select(c => (object)c.Alignment.ToString()).ToArray()),
                ["descriptors"] = new JArray(ability.ComponentsArray.OfType<SpellDescriptorComponent>().Select(d => (object)d.Descriptor.Value.ToString()).ToArray()),
                ["components"] = new JArray(ability.ComponentsArray.Where(c => c != null).Select(c => (object)c.GetType().Name).ToArray())
            };
        }

        // ---------------------------------------------------------------- Call of the Wild contract

        /// <summary>
        /// The provider's channel registry: the entry list, or null with the reason it could not be read. An unreadable
        /// registry is never treated as an empty one.
        /// </summary>
        private static object[] ReadRegistry(Type engineType, out string status)
        {
            FieldInfo field = engineType == null ? null : engineType.GetField("channel_entires", Any);
            if (field == null) { status = "unreadable: channel_entires field missing"; return null; }
            object value;
            try { value = field.GetValue(null); }
            catch (Exception e) { status = "unreadable: " + e.GetType().Name; return null; }
            var list = value as IEnumerable;
            if (list == null) { status = "unreadable: channel_entires is " + (value == null ? "null" : value.GetType().Name); return null; }
            object[] entries = list.Cast<object>().ToArray();
            status = "read " + entries.Length + " entries";
            return entries;
        }

        private static IEnumerable<BlueprintAbility> ListAbilities(object component, string field)
        {
            FieldInfo info = component.GetType().GetField(field, Any);
            var values = info == null ? null : info.GetValue(component) as IEnumerable;
            if (values == null) throw new InvalidOperationException(component.GetType().Name + " has no readable " + field + " list");
            return values.Cast<object>().OfType<BlueprintAbility>();
        }

        private JToken DescribeCotw()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CallOfTheWild");
            if (cotw == null)
            {
                var vanillaMenu = new JArray();
                foreach (string issue in PlacementProblems(LiveCases(), vanillaMenu)) problems.Add("vanilla menu placement: " + issue);
                return new JObject { ["status"] = "absent", ["turnMenu"] = vanillaMenu };
            }
            Type engineType = cotw.GetType("CallOfTheWild.ChannelEnergyEngine", false);
            if (engineType == null) { problems.Add("CotW has no ChannelEnergyEngine"); return new JObject { ["status"] = "engine missing" }; }
            List<BlueprintAbility> turn = TurnIdentities();
            string[] turnGuids = turn.Select(a => a.AssetGuid).ToArray();
            var turnSet = new HashSet<string>(turnGuids);

            // Registry: read (or fail), Turn-free, and the supported sources' plain positive-harm entries present.
            string registryStatus;
            object[] registry = ReadRegistry(engineType, out registryStatus);
            if (registry == null) problems.Add("CotW channel registry " + registryStatus);
            string controlStatus;
            object[] control = ReadRegistry(typeof(TurnUndeadMissingRegistryEngine), out controlStatus);
            if (control != null || !controlStatus.StartsWith("unreadable")) problems.Add("the registry reader accepted an engine without channel_entires: " + controlStatus);
            var byType = new JObject();
            int turnEntries = 0;
            var entries = new JArray();
            var supportedFeatures = new HashSet<string>(ExpectedSources.Select(e => e.Feature));
            var expectedEntries = ExpectedSources.Select(e => new[] { e.NativeHarm, e.Feature }).Concat(new[] { new[] { CotwPaladinExtraHarm, ExpectedSources[1].Feature } }).ToArray();
            var seenSupported = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (object entry in registry ?? new object[0])
            {
                object type = entry.GetType().GetField("channel_type", Any)?.GetValue(entry);
                var ability = entry.GetType().GetField("ability", Any)?.GetValue(entry) as BlueprintAbility;
                string typeName = type == null ? "null" : type.ToString();
                byType[typeName] = ((int?)byType[typeName] ?? 0) + 1;
                if (ability != null && (turnSet.Contains(ability.AssetGuid) || (ability.Parent != null && turnSet.Contains(ability.Parent.AssetGuid)))) turnEntries++;
                if (type == null || Convert.ToInt32(type) != 9) continue; // plain PositiveHarm entries
                var parent = entry.GetType().GetField("parent_feature", Any)?.GetValue(entry) as BlueprintFeature;
                var wrapper = entry.GetType().GetField("base_ability", Any)?.GetValue(entry) as BlueprintAbility;
                AbilityResourceLogic entryResource = ability == null ? null : ability.GetComponents<AbilityResourceLogic>().FirstOrDefault();
                bool supported = parent != null && supportedFeatures.Contains(parent.AssetGuid);
                if (supported && ability != null)
                {
                    int n;
                    seenSupported.TryGetValue(ability.AssetGuid + ">" + parent.AssetGuid, out n);
                    seenSupported[ability.AssetGuid + ">" + parent.AssetGuid] = n + 1;
                }
                entries.Add(new JObject
                {
                    ["ability"] = Ref(ability),
                    ["resource"] = entryResource == null ? null : (entryResource.RequiredResource == null ? "null" : entryResource.RequiredResource.AssetGuid + ":" + entryResource.RequiredResource.name) + " x" + entryResource.Amount,
                    ["parentFeature"] = Ref(parent), ["wrapper"] = Ref(wrapper),
                    ["turnUndeadSupport"] = supported ? "supported" : "unsupported (not a Turn Undead source)"
                });
            }
            if (turnEntries != 0) problems.Add(turnEntries + " CotW channel entries (of any form) are Turn identities");
            foreach (string[] expected in expectedEntries)
            {
                int n;
                seenSupported.TryGetValue(expected[0] + ">" + expected[1], out n);
                if (registry != null && n != 1) problems.Add("CotW registry has " + n + " plain positive-harm entries for supported " + expected[0] + " under " + expected[1] + ", expected one");
            }
            int unexpectedSupported = seenSupported.Keys.Count(k => !expectedEntries.Any(e => e[0] + ">" + e[1] == k));
            if (unexpectedSupported != 0) problems.Add(unexpectedSupported + " unexpected plain positive-harm entries under a supported source feature");

            var menu = new JArray();
            foreach (string issue in PlacementProblems(LiveCases(), menu)) problems.Add("CotW menu placement: " + issue);

            // Modifier holders: exact per-identity membership on every list, with a mutation control.
            var holders = new JObject();
            JArray mutationControl = null;
            foreach (string field in new[] { "improved_channel", "sacred_conduit", "consecrate_buff" })
            {
                var holder = engineType.GetField(field, Any)?.GetValue(null) as BlueprintUnitFact;
                if (holder == null) { holders[field] = "absent"; continue; }
                var lists = new JArray();
                var membership = new List<string>();
                foreach (BlueprintComponent c in holder.ComponentsArray.Where(c => c != null && c.GetType().Name == "IncreaseSpecifiedSpellsDC"))
                {
                    BlueprintAbility[] spells = ListAbilities(c, "spells").ToArray();
                    lists.Add(Shallow(c));
                    membership.AddRange(TurnUndeadObserverRules.MembershipProblems(field, turnGuids,
                        spells.Where(a => a != null && turnSet.Contains(a.AssetGuid)).Select(a => a.AssetGuid)));
                    if (field == "improved_channel" && mutationControl == null)
                    {
                        // A duplicate standing in for a missing identity must fail although the count still matches.
                        string direct = Direct("Cleric").AssetGuid, child = ((BlueprintAbility)Member(SourceOf("Cleric"), "MenuChild")).AssetGuid;
                        IEnumerable<string> mutated = spells.Where(a => a != null && turnSet.Contains(a.AssetGuid)).Select(a => a.AssetGuid == direct ? child : a.AssetGuid);
                        mutationControl = new JArray(TurnUndeadObserverRules.MembershipProblems("mutated " + field, turnGuids, mutated).Cast<object>().ToArray());
                        if (mutationControl.Count == 0) problems.Add("the membership check accepted a duplicate in place of a missing identity");
                    }
                }
                if (lists.Count == 0) membership.Add(field + ": no IncreaseSpecifiedSpellsDC component");
                problems.AddRange(membership);
                holders[field] = new JObject { ["fact"] = Ref(holder), ["lists"] = lists, ["membershipProblems"] = new JArray(membership.Cast<object>().ToArray()) };
            }
            var resistances = new JArray();
            var resistanceFacts = engineType.GetField("channel_resistances", Any)?.GetValue(null) as IEnumerable<BlueprintUnitFact>;
            if (resistanceFacts == null) problems.Add("CotW channel_resistances unreadable");
            foreach (BlueprintUnitFact fact in resistanceFacts ?? new BlueprintUnitFact[0])
            {
                var membership = new List<string>();
                BlueprintComponent[] lists = fact.ComponentsArray.Where(c => c != null && (c.GetType().Name == "SavingThrowBonusAgainstSpecificSpells" || c.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells")).ToArray();
                if (lists.Length == 0) membership.Add(fact.name + ": no ability-specific list");
                foreach (BlueprintComponent c in lists)
                    membership.AddRange(TurnUndeadObserverRules.MembershipProblems("resistance " + fact.name, turnGuids,
                        ListAbilities(c, "Spells").Where(a => a != null && turnSet.Contains(a.AssetGuid)).Select(a => a.AssetGuid)));
                problems.AddRange(membership);
                resistances.Add(new JObject
                {
                    ["fact"] = Ref(fact), ["lists"] = new JArray(lists.Select(c => (object)Shallow(c)).ToArray()),
                    ["membershipProblems"] = new JArray(membership.Cast<object>().ToArray())
                });
            }
            return new JObject
            {
                ["assemblyVersion"] = cotw.GetName().Version.ToString(), ["sha256"] = Sha256File(cotw.Location),
                ["registryStatus"] = registryStatus, ["registryUnreadableControl"] = controlStatus,
                ["channelEntriesByType"] = byType, ["channelEntriesThatAreTurn"] = turnEntries,
                ["positiveHarmEntries"] = entries, ["turnIdentities"] = new JArray(turn.Select(a => (object)(a.AssetGuid + ":" + a.name)).ToArray()),
                ["turnMenu"] = menu, ["dcHolders"] = holders, ["membershipMutationControl"] = mutationControl, ["channelResistances"] = resistances
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
            result["undeadCaster"] = new JObject
            {
                ["clericUndead"] = cleric.Descriptor.HasFact(Blueprint<BlueprintUnitFact>(UndeadType)), ["commandResult"] = command.Result.ToString(),
                ["expectedDc"] = expectedDc, ["clericSaves"] = Saves(cleric, seq), ["skeletonSaves"] = Saves(skeleton, seq), ["livingControlSaves"] = Saves(living, seq),
                ["clericFleeing"] = FleeingOn(cleric) != null, ["skeletonFleeing"] = FleeingOn(skeleton) != null
            };
            // The channeler's inclusion is shown by its own roll; the effect must follow that roll either way.
            result["undeadCaster"]["clericOutcome"] = Judge("the undead channeler", cleric, true, seq, expectedDc);
            result["undeadCaster"]["skeletonOutcome"] = Judge("the skeleton beside the undead channeler", skeleton, true, seq, expectedDc);
            Judge("the living control", living, false, seq, expectedDc);
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
            var selectiveFact = Blueprint<BlueprintUnitFact>(SelectiveChannel);
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
                int expectedDc = 10 + 5 / 2 + cleric.Stats.Charisma.Bonus;
                var command = new UnitUseAbility(new AbilityData(cleric.Descriptor.Abilities.GetAbility(clericTurn)), new TargetWrapper(cleric));
                command.IgnoreCooldown(null);
                cleric.Commands.InterruptAll(true);
                cleric.Commands.Run(command);
                foreach (object step in Settle(0.0, 30f, () => command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return step;
                string label = selective ? "with Selective Channel" : "without Selective Channel";
                var row = new JObject
                {
                    ["selectiveChannel"] = cleric.Descriptor.HasFact(selectiveFact), ["commandResult"] = command.Result.ToString(),
                    ["allyIsEnemy"] = ally.IsEnemy(cleric), ["enemyIsEnemy"] = enemy.IsEnemy(cleric),
                    ["enemySaves"] = Saves(enemy, seq), ["allySaves"] = Saves(ally, seq),
                    ["enemyFleeing"] = FleeingOn(enemy) != null, ["allyFleeing"] = FleeingOn(ally) != null,
                    ["enemyDistance"] = Math.Round(Vector3.Distance(enemy.Position, cleric.Position), 2),
                    ["allyDistance"] = Math.Round(Vector3.Distance(ally.Position, cleric.Position), 2)
                };
                cases.Add(row);
                if (ally.IsEnemy(cleric) || !enemy.IsEnemy(cleric)) problems.Add(label + ": fixture allegiance is wrong");
                // Inclusion is the rolled save's existence; the effect follows the roll.
                row["enemyOutcome"] = Judge(label + ": the enemy skeleton", enemy, true, seq, expectedDc);
                row["allyOutcome"] = Judge(label + ": the allied skeleton", ally, !selective, seq, expectedDc);
            }
        }

        // ---------------------------------------------------------------- abandoned probe

        private IEnumerable<object> Abandon()
        {
            Game.Instance.IsPaused = false;
            bool prior = Game.Instance.IsPaused;
            TurnUndeadProbe nested = AdoptChild(new TurnUndeadProbe("first-turn") { injectCleanupFailure = injectNestedCleanupFailureForTest });
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
            // Normal completion: the child is released here once; its report is kept and merged into ours by Release.
            CleanupReport report = ReleaseChild(nested, "abandoned by the abandon check");
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
            if (!report.Restored) problems.Add("the nested cleanup verdict reported failures: " + string.Join("; ", report.Failures.ToArray()));
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
