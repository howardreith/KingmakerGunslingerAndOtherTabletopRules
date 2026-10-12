using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunExpandedSummoningSprint22Contracts()
        {
            WriteLifecycleStage("expanded-summoning-sprint22-contracts-start");
            var assertions = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var facts = new JArray();
            var document = new JObject {
                ["scope"] = "Sprint 22 exact donor/condition metadata; detached read-only prefabs, no spawn or save access.",
                ["nativeData"] = "Bone hierarchy/rest metadata, material names, bounds and action/clip names only; no native geometry, textures or animation curves.",
                ["runId"] = _request.RunId, ["donors"] = rows, ["facts"] = facts
            };
            try
            {
                Sprint22DonorSurveyPolicy.Validate();
                var all = BlueprintBootstrap.Library.GetAllBlueprints().Where(value => value != null).ToArray();
                foreach (string[] anchor in Sprint22DonorSurveyPolicy.Donors)
                {
                    BlueprintUnit unit = all.OfType<BlueprintUnit>().SingleOrDefault(value => value.AssetGuid == anchor[1]);
                    if (unit == null) throw new InvalidOperationException("unit=" + anchor[0] + ";predicate=exact-donor;guid=" + anchor[1]);
                    var prefab = unit.Prefab.Load(false);
                    if (prefab == null) throw new InvalidOperationException("unit=" + anchor[0] + ";predicate=detached-prefab;owner=" + unit.AssetGuid);
                    var row = CapturePrimateCensusViewMetadata(prefab, anchor[0], unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                    row["scope"] = "Sprint 22 detached read-only donor";
                    row["nativeUnit"] = DescribeNativeUnit(unit);
                    var managers = prefab.GetComponentsInChildren<UnitAnimationManager>(true);
                    row["animationManagers"] = new JArray(managers.Select(manager => {
                        var set = manager.AnimationSet;
                        var actions = set == null || set.Actions == null ? null : set.Actions.ToArray();
                        return new JObject {
                            ["name"] = manager.name, ["set"] = set == null ? null : set.name,
                            ["attachedBindingProven"] = false,
                            ["actions"] = actions == null ? JValue.CreateNull() : (JToken)new JArray(actions.Select(action => {
                                if (action == null) return JValue.CreateNull();
                                var native = action as UnitAnimationAction;
                                var clips = native == null || native.Clips == null ? null : native.Clips.ToArray();
                                return (JToken)new JObject {
                                    ["name"] = action.name, ["class"] = action.GetType().FullName,
                                    ["type"] = native == null ? null : native.Type.ToString(),
                                    ["clips"] = clips == null ? JValue.CreateNull() : (JToken)new JArray(clips.Select(clip => clip == null ? (JToken)JValue.CreateNull() : new JObject {
                                        ["name"] = clip.name, ["durationSeconds"] = clip.length
                                    }))
                                };
                            }))
                        };
                    }));
                    rows.Add(row);
                    assertions.Add(Assertion("sprint22-donor-" + anchor[0], "exact detached donor captured", unit.name + ";guid=" + unit.AssetGuid, true, "No actor instantiation or native asset mutation."));
                }

                // Identify actual generic condition facts by their engine
                // component, including descriptors that must NOT be inherited.
                var controls = all.OfType<BlueprintBuff>().Where(buff =>
                    buff.GetComponents<AddCondition>().Any(component => component.Condition == UnitCondition.Dazed || component.Condition == UnitCondition.Stunned)).ToArray();
                foreach (var buff in controls.OrderBy(value => value.name, StringComparer.Ordinal))
                    facts.Add(DescribeGraph(buff, 0, new HashSet<object>(), 3));
                foreach (var fact in all.Where(value => value.AssetGuid == "13c87ac5985cc85498ef9d1ac8b78923" ||
                    value.AssetGuid == "ae822725634c6f0418b8c48bd29df255" ||
                    new[] { "ImprovedBullRush", "ImprovedOverrun", "WeaponFocusTail" }.Any(name =>
                        string.Equals(value.name, name, StringComparison.OrdinalIgnoreCase))))
                    facts.Add(DescribeGraph(fact, 0, new HashSet<object>(), 3));
                assertions.Add(Assertion("sprint22-native-dazed-component", "native Dazed fact present", string.Join(",", controls.Where(value => value.GetComponents<AddCondition>().Any(c => c.Condition == UnitCondition.Dazed)).Select(value => value.name)), controls.Any(value => value.GetComponents<AddCondition>().Any(c => c.Condition == UnitCondition.Dazed)), "Native AddCondition, not a name match."));
                assertions.Add(Assertion("sprint22-native-stunned-component", "exact native Stunned fact present", "09d39b38bb7c6014394b6daced9bacd3", controls.Any(value => value.AssetGuid == "09d39b38bb7c6014394b6daced9bacd3"), "Native AddCondition and exact native GUID."));
            }
            catch (Exception exception)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(exception);
                assertions.Add(Assertion("sprint22-contract-capture", "bounded read-only capture completes", document["exception"].ToString(), false, "Unknown metadata is not qualification."));
            }
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "sprint22-ankylosaurus-contracts.json"), document.ToString(Formatting.Indented));
            return CreateResult(assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL", assertions, null);
        }
    }
}
