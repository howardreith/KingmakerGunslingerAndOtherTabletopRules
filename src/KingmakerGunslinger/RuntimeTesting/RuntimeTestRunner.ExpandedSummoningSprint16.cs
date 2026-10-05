using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private int _crocodilianSurveyLoadingWait;
        private bool _crocodilianMechanicsComplete;
        private readonly List<RuntimeTestAssertion> _crocodilianAssertions =
            new List<RuntimeTestAssertion>();

        /// <summary>
        /// Crocodilian-only candidate mechanics plus the reproducible native
        /// census. The original seven-assertion survey remains historical
        /// research evidence; new mechanics are not qualified until this
        /// expanded scenario runs. No historical-root census or save write.
        /// </summary>
        private void PollExpandedSummoningCrocodilianSurvey()
        {
            if (_crocodilianMechanicsComplete)
            {
                if (_sprint16CombatComplete) PollSprint16FinalReview();
                else PollSprint16Combat();
                return;
            }
            string loading;
            if (ExpandedSummoningLoadingActive(out loading) &&
                    _crocodilianSurveyLoadingWait++ < ExpandedSummoningLoadingGateFrames)
                return;
            var assertions = _crocodilianAssertions;
            ExpandedSummoningCorrectionFixture fixture = null;
            bool cleaned = false;
            try
            {
                fixture = BeginExpandedSummoningCorrectionFixture(
                    "KMG_Runtime_Sprint16_SurveyCaster");
                var census = DescribeSprint16NativeCensus(fixture.Blueprints);
                string censusPath = Path.Combine(_request.EvidenceDirectory,
                    "sprint16-native-census.json");
                File.WriteAllText(censusPath, census.ToString(Formatting.Indented));
                assertions.Add(Assertion("sprint16-native-census-written",
                    "complete current-process type and blueprint census; no inferred mechanic PASS",
                    "file=sprint16-native-census.json;types=" +
                        ((JArray)census["swallowTypes"]).Count,
                    ((JArray)census["swallowTypes"]).Count > 0 &&
                        File.Exists(censusPath),
                    "loaded Assembly-CSharp metadata and actual loaded blueprint instances"));

                foreach (string key in new[] { "crocodile", "dire-crocodile" })
                {
                    UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, key);
                    // Freeze only these request-local survey units. AI behavior
                    // is deliberately not tested by a survey using this helper.
                    SetExpandedSummoningBrainActive(unit, false);
                    ModifiableValue perception = unit.Descriptor.Stats.GetStat(
                        StatType.SkillPerception);
                    ModifiableValue stealth = unit.Descriptor.Stats.GetStat(
                        StatType.SkillStealth);
                    ModifiableValue mobility = unit.Descriptor.Stats.GetStat(
                        StatType.SkillMobility);
                    CrocodilianRulesProfile rules = CrocodilianRulesPolicy.For(key);
                    int expectedPerception = key == "crocodile" ? 8 : 14;
                    int expectedStealth = key == "crocodile" ? 5 : 0;
                    assertions.Add(Assertion("sprint16-land-skills-" + key,
                        "Perception " + expectedPerception + ";Stealth " +
                            expectedStealth + ";Mobility ranks 0",
                        "Perception=" + DescribeSprint16Skill(perception) +
                            ";Stealth=" + DescribeSprint16Skill(stealth) +
                            ";Mobility=" + DescribeSprint16Skill(mobility),
                        perception.ModifiedValue == expectedPerception &&
                            stealth.ModifiedValue == expectedStealth &&
                            perception.BaseValue == rules.PerceptionRanks &&
                            stealth.BaseValue == rules.StealthRanks &&
                            mobility.BaseValue == 0,
                        "actual untemplated own-tier summon, native stat totals and modifiers"));
                    if (key == "crocodile")
                    {
                        string rig = CaptureDonorRig(unit, "monitor-lizard",
                            _request.EvidenceDirectory, "Sprint 16");
                        assertions.Add(Assertion("sprint16-monitor-bind-frames",
                            "one complete renderer-local bind frame; no donor vertices",
                            rig, rig.Contains(",renderers=1,") && rig.Contains(",bones="),
                            "request-local summon view metadata; private authoring input"));
                    }
                }

                BlueprintBuff swallowed = fixture.Blueprints.OfType<BlueprintBuff>()
                    .Single(value => value.name == "KMG_Summoning_Special_DireCrocodile_Swallowed");
                AddFactContextActions cadence = swallowed.ComponentsArray
                    .OfType<AddFactContextActions>().Single();
                ContextActionDealDamage damage = cadence.NewRound.Actions.Length == 1
                    ? cadence.NewRound.Actions[0] as ContextActionDealDamage : null;
                bool exact = cadence.Activated.Actions.Length == 0 &&
                    cadence.Deactivated.Actions.Length == 0 && damage != null &&
                    damage.DamageType.Type == Kingmaker.RuleSystem.Rules.Damage.DamageType.Physical &&
                    damage.DamageType.Physical.Form == Kingmaker.Enums.Damage.PhysicalDamageForm.Bludgeoning &&
                    (int)damage.Value.DiceType == 6 &&
                    damage.Value.DiceCountValue.Value == 3 &&
                    damage.Value.BonusValue.Value == 13;
                assertions.Add(Assertion("sprint16-swallowed-graph",
                    "no activation/removal actions; exactly one direct NewRound 3d6+13 crush",
                    DescribeGraph(swallowed, 0, new HashSet<object>(
                        NativeDonorReferenceComparer.Instance), 32).ToString(Formatting.None),
                    exact, "recursive live graph; cadence events require separate later-round proof"));
                ExerciseSprint16DamageAndMaintain(fixture, assertions);
                ExerciseSprint16Speed(fixture, assertions);
                var icons = new JArray();
                foreach (var binding in OwnedIconAssignments.Bindings.Where(value =>
                    value.Symbol.StartsWith("KMG.Summoning.Special.Crocodile.", StringComparison.Ordinal) ||
                    value.Symbol.StartsWith("KMG.Summoning.Special.DireCrocodile.", StringComparison.Ordinal)))
                {
                    BlueprintUnitFact fact = fixture.Blueprints.OfType<BlueprintUnitFact>().Single(value =>
                        value.name == binding.Symbol.Replace('.', '_'));
                    var expected = ProjectAssetIcons.RequireIcon(binding.Key);
                    bool assigned = ReferenceEquals(fact.Icon, expected) && fact.Icon.rect.width == 64 &&
                        fact.Icon.rect.height == 64 && fact.GetType() == binding.BlueprintType;
                    Sprint16Check(assertions, icons, "icon-" + binding.Symbol, assigned,
                        new JObject { ["symbol"] = binding.Symbol, ["guid"] = fact.AssetGuid,
                            ["sprite"] = fact.Icon == null ? null : fact.Icon.name },
                        "exact cached 64px emblem after all late registration and owned-assignment stages; native UI separate");
                }
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                    "sprint16-icon-bindings.json"), icons.ToString(Formatting.Indented));
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion("sprint16-survey-exception", "no exception",
                    DescribeExpandedSummoningCorrectionException(exception), false,
                    "narrow request-local survey"));
            }
            finally
            {
                try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
                catch (Exception exception)
                {
                    assertions.Add(Assertion("sprint16-survey-cleanup-exception",
                        "exact cleanup", DescribeExpandedSummoningCorrectionException(exception),
                        false, "request-local fixture cleanup"));
                }
            }
            assertions.Add(Assertion("sprint16-survey-cleanup",
                "nonempty native unit/party census and every original area membership preserved",
                "cleaned=" + cleaned + ";nativeUnits=" + (fixture == null ? 0 : fixture.UnitsBefore.Length) +
                    ";party=" + (fixture == null ? 0 : fixture.PartyBefore.Length), cleaned,
                "typed Units.All census, owned-actor-only disposal and exact original HoldingState references"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            _crocodilianMechanicsComplete = true;
        }

        private static string DescribeSprint16Skill(ModifiableValue value)
        {
            ModifiableValueSkill skill = value as ModifiableValueSkill;
            return "total:" + value.ModifiedValue + ",ranks:" + value.BaseValue +
                ",classSkill:" + (skill != null && skill.ClassSkill) +
                ",abilityScore:" + (skill == null ? 0 : skill.BaseStat.ModifiedValue) +
                ",abilityModifier:" + (skill == null ? 0 : skill.BaseStat.Bonus) +
                ",modifiers:[" + string.Join("|", value.Modifiers.Select(modifier =>
                    modifier.ModValue + "/" + modifier.ModDescriptor + "/" +
                    modifier.Source).ToArray()) + "]";
        }

        private static JObject DescribeSprint16NativeCensus(
            BlueprintScriptableObject[] blueprints)
        {
            var result = new JObject();
            Assembly native = typeof(UnitPartSwallowed).Assembly;
            Type[] allTypes = native.GetTypes();
            result["assembly"] = native.FullName;
            result["mvid"] = native.ManifestModule.ModuleVersionId.ToString();
            result["enumeratedTypes"] = allTypes.Length;
            result["enumeratedBlueprints"] = blueprints.Length;
            var types = new JArray();
            foreach (Type type in allTypes.Where(value =>
                Matches(value.FullName, new[] { "swallow", "stomach", "interior", "cutfree" }))
                .OrderBy(value => value.FullName, StringComparer.Ordinal))
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                types.Add(new JObject {
                    ["type"] = type.FullName,
                    ["base"] = type.BaseType == null ? null : type.BaseType.FullName,
                    ["fields"] = new JArray(type.GetFields(flags).Select(field =>
                        field.FieldType.FullName + " " + field.Name).OrderBy(value => value).ToArray()),
                    ["properties"] = new JArray(type.GetProperties(flags).Select(property =>
                        property.PropertyType.FullName + " " + property.Name).OrderBy(value => value).ToArray()),
                    ["methods"] = new JArray(type.GetMethods(flags).Select(method =>
                        method.ToString()).OrderBy(value => value).ToArray())
                });
            }
            result["swallowTypes"] = types;
            result["runCandidates"] = DescribeNamed(blueprints.OfType<BlueprintUnitFact>()
                .Where(value => Matches(value.name, new[] { "run", "sprint" })));
            var considerations = new JArray();
            BlueprintBrain[] brains = blueprints.OfType<BlueprintBrain>().ToArray();
            IsEngagedConsideration[] inline = brains.SelectMany(brain =>
                    brain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Where(action => action != null)
                .SelectMany(action => (action.ActorConsiderations ??
                    Array.Empty<Consideration>()).Concat(action.TargetConsiderations ??
                    Array.Empty<Consideration>())).OfType<IsEngagedConsideration>().ToArray();
            foreach (IsEngagedConsideration value in blueprints.OfType<IsEngagedConsideration>()
                .Concat(inline).Distinct())
                considerations.Add(new JObject { ["name"] = value.name,
                    ["guid"] = value.AssetGuid, ["engaged"] = value.EngagedScore,
                    ["free"] = value.NotEngagedScore, ["multiplier"] = value.BaseScoreModifier });
            result["engagementConsiderations"] = considerations;
            result["crocodilianAndNaturalBrains"] = new JArray(brains.Where(brain =>
                Matches(brain.name, new[] { "dumb", "crocodile" })).Select(brain =>
                    new JObject {
                        ["name"] = brain.name, ["guid"] = brain.AssetGuid,
                        ["actions"] = new JArray((brain.Actions ??
                            Array.Empty<BlueprintAiAction>()).Select(action =>
                                new JObject {
                                    ["name"] = action == null ? null : action.name,
                                    ["guid"] = action == null ? null : action.AssetGuid,
                                    ["type"] = action == null ? null : action.GetType().FullName,
                                    ["actorConsiderations"] = new JArray(action == null
                                        ? Array.Empty<string>() : (action.ActorConsiderations ??
                                            Array.Empty<Consideration>()).Select(value =>
                                                value == null ? "<null>" :
                                                value.name + ":" + value.AssetGuid).ToArray())
                                }))
                    }));
            var graphs = new JObject();
            foreach (BlueprintScriptableObject value in blueprints.Where(value =>
                Matches(value.name, new[] { "swallow", "engulf", "hastebuff", "slowbuff" }) ||
                value is BlueprintBrain && Matches(value.name,
                    new[] { "dumb", "crocodile" })))
                graphs[value.name + ":" + value.AssetGuid] = DescribeGraph(value, 0,
                    new HashSet<object>(NativeDonorReferenceComparer.Instance), 32);
            result["graphs"] = graphs;
            result["scope"] = "research only; interior limitation requires review, not automatic acceptance";
            return result;
        }
    }
}
