using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The native-donor audit for the Phase 1 sprints: which units, classes,
    /// facts, buffs and abilities the installed library offers for the
    /// creatures the charter asks to publish next, recorded as metadata.
    ///
    /// Sprint 3 opens with native publication - Pony, Horse, Owlbear, Cyclops
    /// and the Frost Giant - and the sprints after it need the plant and huge
    /// bodies, the mephit rig, the native grapple and swallow buffs and the
    /// cat family. None of those blueprints is named in the repository, the
    /// game's blueprints are not readable offline, and reflection over the
    /// assembly gives types, not instances. So the audit runs in the loaded
    /// game, at mod load, and writes what it found to the run's evidence
    /// directory: names, GUIDs, sizes, statistics, class levels, facts, body
    /// weapons, component type names and the view prefab's asset id. No
    /// geometry, texture, animation or assembly leaves the game.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private static readonly string[] NativeDonorUnitTerms =
        {
            "horse", "pony", "owlbear", "cyclop", "frostgiant", "giantfrost",
            "shambl", "flytrap", "worm", "mephit", "tiger", "smilodon",
            "leopard", "cheetah", "bear", "monitor", "lizard", "spider",
            "pixie", "nixie", "mound", "giant"
        };

        private static readonly string[] NativeDonorFactTerms =
        {
            "grab", "grapple", "constrict", "swallow", "tremorsense",
            "blindsense", "web", "pounce", "rake", "trip", "sprint", "flash",
            "insight", "breath", "poison", "naturalarmor", "plant", "regenerat",
            "fasthealing", "ferocity", "rockthrow", "rock"
        };

        private static readonly string[] NativeDonorAbilityTerms =
        {
            "web", "breath", "stinkingcloud", "glitterdust", "acidarrow",
            "blur", "magicmissile", "windwall", "chillmetal", "heatmetal",
            "pyrotechnics", "gaseous", "softenearth", "gustofwind",
            "scorchingray", "swallow", "grapple", "grab", "flash", "sprint"
        };

        private static readonly string[] NativeDonorBuffTerms =
        {
            "grapple", "grab", "swallow", "web", "entangle", "constrict",
            "sleep", "paralyz", "sprint", "flash", "insight", "pounce"
        };

        private const string NativeGrabFeatureGuid =
            "efc1e80fb41e06544be46604983806d6";

        private RuntimeTestResult RunExpandedSummoningNativeDonorAudit()
        {
            WriteLifecycleStage("expanded-summoning-native-donor-audit-start");
            BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null).ToArray();
            var document = new JObject();
            var units = new JArray();
            foreach (BlueprintUnit unit in all.OfType<BlueprintUnit>()
                .Where(value => Matches(value.name, NativeDonorUnitTerms))
                .OrderBy(value => value.name, StringComparer.Ordinal))
                units.Add(DescribeNativeUnit(unit));
            document["units"] = units;

            var classes = new JArray();
            foreach (BlueprintCharacterClass characterClass in all
                .OfType<BlueprintCharacterClass>()
                .OrderBy(value => value.name, StringComparer.Ordinal))
                classes.Add(new JObject { ["name"] = characterClass.name,
                    ["guid"] = characterClass.AssetGuid,
                    ["hitDie"] = DescribeReference(ReadExactMember(characterClass, "HitDie")) });
            document["classes"] = classes;

            document["facts"] = DescribeNamed(all.OfType<BlueprintUnitFact>()
                .Where(value => !(value is BlueprintAbility) &&
                    !(value is BlueprintBuff) &&
                    Matches(value.name, NativeDonorFactTerms)));
            document["abilities"] = DescribeNamed(all.OfType<BlueprintAbility>()
                .Where(value => Matches(value.name, NativeDonorAbilityTerms)));
            document["buffs"] = DescribeNamed(all.OfType<BlueprintBuff>()
                .Where(value => Matches(value.name, NativeDonorBuffTerms)));

            BlueprintScriptableObject grab;
            BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                NativeGrabFeatureGuid, out grab);
            document["nativeGrab"] = grab == null ? null :
                DescribeGraph(grab, 0, new HashSet<object>(
                    NativeDonorReferenceComparer.Instance));
            // Sprint 4 onward: the exact graphs behind the signature mechanics,
            // ten levels deep. Blueprint references inside them are still
            // recorded as name:guid:type only; nothing the game owns is copied.
            var deep = new JObject();
            foreach (string guid in DeepGraphGuids)
            {
                BlueprintScriptableObject blueprint;
                if (!BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                        guid, out blueprint) || blueprint == null)
                {
                    deep[guid] = "<missing>";
                    continue;
                }
                deep[blueprint.name + ":" + guid] = DescribeGraph(blueprint, 0,
                    new HashSet<object>(NativeDonorReferenceComparer.Instance),
                    10);
            }
            document["deepGraphs"] = deep;

            string path = Path.Combine(_request.EvidenceDirectory,
                "native-donor-audit.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(document,
                Formatting.Indented));
            WriteLifecycleStage("expanded-summoning-native-donor-audit-written");

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("expanded-summoning-native-donor-audit",
                    "audit written with units, classes, facts, abilities, buffs and the native grab graph",
                    "units=" + units.Count + ";classes=" + classes.Count +
                    ";facts=" + ((JArray)document["facts"]).Count +
                    ";abilities=" + ((JArray)document["abilities"]).Count +
                    ";buffs=" + ((JArray)document["buffs"]).Count +
                    ";nativeGrab=" + (grab == null ? "missing" : grab.name) +
                    ";file=native-donor-audit.json",
                    units.Count > 0 && classes.Count > 0 && grab != null &&
                        File.Exists(path),
                    "installed library metadata only; no asset content"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private sealed class NativeDonorReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly NativeDonorReferenceComparer Instance =
                new NativeDonorReferenceComparer();
            public new bool Equals(object left, object right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(object value)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value); }
        }

        private static bool Matches(string name, string[] terms)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string lower = name.ToLowerInvariant();
            return terms.Any(term => lower.IndexOf(term, StringComparison.Ordinal) >= 0);
        }

        private static JArray DescribeNamed(IEnumerable<BlueprintScriptableObject> values)
        {
            var result = new JArray();
            foreach (BlueprintScriptableObject value in values
                .OrderBy(value => value.name, StringComparer.Ordinal))
                result.Add(new JObject { ["name"] = value.name,
                    ["guid"] = value.AssetGuid,
                    ["type"] = value.GetType().Name,
                    ["components"] = new JArray((value.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Where(c => c != null)
                        .Select(c => c.GetType().Name).ToArray()) });
            return result;
        }

        private static JObject DescribeNativeUnit(BlueprintUnit unit)
        {
            var result = new JObject
            {
                ["name"] = unit.name,
                ["guid"] = unit.AssetGuid,
                ["size"] = unit.Size.ToString(),
                ["alignment"] = unit.Alignment.ToString(),
                ["strength"] = unit.Strength, ["dexterity"] = unit.Dexterity,
                ["constitution"] = unit.Constitution,
                ["intelligence"] = unit.Intelligence, ["wisdom"] = unit.Wisdom,
                ["charisma"] = unit.Charisma, ["speed"] = unit.Speed.Value,
                ["maxHp"] = unit.MaxHP, ["baseAttackBonus"] = unit.BaseAttackBonus,
                ["brain"] = unit.Brain == null ? null : unit.Brain.name,
                ["prefabAssetId"] = unit.Prefab == null ? null : unit.Prefab.AssetId,
                ["faction"] = DescribeReference(ReadExactMember(unit, "Faction") ??
                    ReadExactMember(unit, "m_Faction")),
                ["type"] = DescribeReference(ReadExactMember(unit, "Type") ??
                    ReadExactMember(unit, "m_Type")),
                ["localizedName"] = unit.LocalizedName == null ? null :
                    unit.LocalizedName.String == null ? null : unit.LocalizedName.String.ToString()
            };
            var levels = new JArray();
            var components = new JArray();
            foreach (BlueprintComponent component in unit.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
            {
                if (component == null) continue;
                components.Add(component.GetType().Name);
                var addLevels = component as AddClassLevels;
                if (addLevels != null)
                    levels.Add(new JObject { ["class"] = addLevels.CharacterClass == null ?
                        null : addLevels.CharacterClass.name, ["levels"] = addLevels.Levels });
            }
            result["classLevels"] = levels;
            result["components"] = components;
            result["facts"] = new JArray((unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Where(value => value != null).Select(value => (object)new JObject {
                    ["name"] = value.name, ["guid"] = value.AssetGuid,
                    ["type"] = value.GetType().Name }).ToArray());
            BlueprintUnit.UnitBody body = unit.Body;
            result["body"] = body == null ? null : new JObject
            {
                ["disableHands"] = body.DisableHands,
                ["primaryHand"] = DescribeWeapon(body.PrimaryHand),
                ["secondaryHand"] = DescribeWeapon(body.SecondaryHand),
                ["additionalLimbs"] = new JArray((body.AdditionalLimbs ??
                    Array.Empty<BlueprintItemWeapon>()).Select(DescribeWeapon).ToArray()),
                ["additionalSecondaryLimbs"] = new JArray((body.AdditionalSecondaryLimbs ??
                    Array.Empty<BlueprintItemWeapon>()).Select(DescribeWeapon).ToArray())
            };
            return result;
        }

        private static JToken DescribeWeapon(Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentHand hand)
        {
            if (hand == null) return null;
            var weapon = hand as BlueprintItemWeapon;
            return new JObject { ["name"] = hand.name, ["guid"] = hand.AssetGuid,
                ["kind"] = hand.GetType().Name,
                ["type"] = weapon == null || weapon.Type == null ? null : weapon.Type.name,
                ["dice"] = weapon == null || weapon.Type == null ? null : weapon.Type.BaseDamage.ToString(),
                ["category"] = weapon == null ? null : weapon.Category.ToString() };
        }

        private static JToken DescribeReference(object value)
        {
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return blueprint.name + ":" + blueprint.AssetGuid;
            return value == null ? null : value.ToString();
        }

        /// <summary>
        /// The component graph of a blueprint, three levels deep: type names
        /// and, for every blueprint reference met, its name and GUID. Enough
        /// to see what the native Grab feature's actions wire to without
        /// copying anything the game owns.
        /// </summary>
        /// <summary>
        /// Native graphs Sprints 4-8 design against. Grab / constrict / swallow
        /// (Shambling Mound, Purple Worm), plant and vermin poisons, web,
        /// sleep, the mephit breath weapons, spell-like abilities and visual
        /// buffs, pounce, fast healing, blindsight and life sense.
        /// </summary>
        private static readonly string[] DeepGraphGuids = {
            "efc1e80fb41e06544be46604983806d6", // ShamblingMoundGrabFeature
            "77106f698ef4cd54ba277de5e6d0e06d", // ShamblingMoundGrappledBuff
            "fde59ce17ec392e46a33420219e85b23", // ShamblingMoundGrappledCantAttack
            "a170aa8c9f462b14095dc1bde6e325cb", // ShamblingMoundPoisonFeature
            "99d7bf176813e8b4ea4b0f95d3390248", // ShamblingMound_ElectricFortitudeImproved
            "dee864aec4a0d344b913dd27a4b504cb", // PurpleWormSwallowWholeFeature
            "368d1df7c1d0267459a584bf23ccadc8", // PurpleWormSwallowed
            "728446b9d0bf47144a1b621169299c2a", // PurpleWormPoisonFeature
            "1180eb46f39f0cd41a0b2e293d1502cb", // GiantFlytrapPoisonFeature
            "0d1d262d3437ec143b16b47e24317bbc", // GolemAutumnGrabFeature
            "077537d7c64aa4e44b559b914693d085", // GolemAutumnGrappledBuff
            "e5f659cc84531124db31bb62aa1b7785", // MimicOozeGrappledBuff
            "b5362f4dc554d2544921934b3a841efa", // GiantFrogTongueGrabTest
            "134cb6d492269aa4f8662700ef57449f", // Web (spell)
            "a719abac0ea0ce346b401060754cc1c0", // WebGrappled
            "bb72a758112438e4a9c62f7637c974ae", // WebBuffSlowMovement
            "3051e7002c803fc47a11bcfa381b9fbd", // SpiderWebImmunity
            "094714bb08f4e1943a8e9d2384ebe573", // GiantSpiderPoisonFeature
            "ef60cd888b834a549898824e6b684918", // HuntingSpiderPoisonFeature
            "d88236a83413baa45ae9c8e5ddce5a6c", // MonitorLizardPoisonFeature
            "5e0cd801bac0e95429bb7e4d1bc61a23", // Sleeping
            "1a8149c09e0bdfc48a305ee6ac3729a8", // Pounce
            "7ada82367e07da04f9421fa8d2818945", // FastHealing2
            "236ec7f226d3d784884f066aa4be1570", // Blindsight
            "bd69053b59fb92c438cc788d2dfd694e", // LifeSenseBuff
            "6e668702fdc53c343a0363813683346e", // Tremorsense (class feature)
            "1f08438786937954aaa6022c7f5ad286", // MephitAirBreathWeapon
            "fa5ee5f4cd5c6394f8b497c773f8e14a", // MephitEarthBreathWeapon
            "ab0616beb567c2c4d8d3f7447a01a0c8", // MephitFireBreathWeapon
            "a54cd27999a5e8340976f3a40edfef3a", // MephitWaterBreathWeapon
            "f98c9fd94b1be6947bd9637816226c1b", // MephitAirBlur
            "65dfd0a5324a76145b38c03250c25a7b", // MephitEarthChangeSize
            "34283d686f5f5a847b4d0d6470b52a65", // MephitWaterStinkingCloud
            "e35ea268f6c8b0344b11c569973197c1", // MephitWaterImmunities
            "14438d09e84e17744a2bc551e925beaa", // MephitAirVisualBuff
            "dbd5b33cfba17d04b88cc5917e2575a5", // MephitEarthVisualBuff
            "6b6aa4c7574cbed4cab96d2d5fa6e25f", // MephitFireVisualBuff
            "a713733858adb1e4b9696e8e58f5f1bb", // MephitWaterVisualBuff
            "50782bc4eb36aac4287023e20ee00808", // MephitAirSummoned (unit)
            "46779f56cab2cb0438161fec0129790d", // MephitEarthSummoned (unit)
            "10a820de0a417f345866f794324205ad", // MephitFireSummoned (unit)
            "4615328295cd7e84bb2ef09d3dba8403", // MephitWaterSummoned (unit)
            "9a46dfd390f943647ab4395fc997936d", // AcidArrow
            "ce7dad2b25acf85429b6c9550787b2d9", // Glitterdust
            "4ac47ddb9fa1eaf43a1b6809980cfbd2", // MagicMissile
            "14ec7a4e52e90fa47a4c8d63c69fd5c1", // Blur
            "cdb106d53c65bbc4086183d54c3b97c7", // ScorchingRay
            "2131842b04b532f4c9cb662c9315a37a", // PujaWolfSprintBuff
            "4d0b2a0971ca8994a8af20940285da2f", // PujaWolfSuperSprintBuff
            "f957b4444b6fb404e84ae2a5765797bb", // TrippingBite
        };

        private static JToken DescribeGraph(object value, int depth, HashSet<object> seen)
        { return DescribeGraph(value, depth, seen, 4); }

        private static JToken DescribeGraph(object value, int depth, HashSet<object> seen,
            int maxDepth)
        {
            if (value == null) return null;
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null && depth > 0)
                return blueprint.name + ":" + blueprint.AssetGuid + ":" +
                    blueprint.GetType().Name;
            if (depth > maxDepth || !seen.Add(value)) return value.GetType().Name;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is string) return value.ToString();
            var array = value as IEnumerable;
            if (array != null && !(value is string))
            {
                var items = new JArray();
                foreach (object item in array)
                {
                    if (items.Count >= 32) break;
                    items.Add(DescribeGraph(item, depth + 1, seen, maxDepth));
                }
                return items;
            }
            var result = new JObject { ["$type"] = type.Name };
            if (blueprint != null)
            {
                result["name"] = blueprint.name;
                result["guid"] = blueprint.AssetGuid;
                result["components"] = new JArray((blueprint.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Where(c => c != null)
                    .Select(c => DescribeGraph(c, depth + 1, seen, maxDepth)).ToArray());
                return result;
            }
            foreach (FieldInfo field in ExpandedSummoningFields(type))
            {
                if (field.IsStatic || field.FieldType == typeof(UnityEngine.Object) ||
                    field.Name.StartsWith("m_CachedName", StringComparison.Ordinal))
                    continue;
                object member;
                try { member = field.GetValue(value); }
                catch (Exception) { continue; }
                if (member == null) continue;
                if (member is UnityEngine.Object && !(member is BlueprintScriptableObject) &&
                    !(member is BlueprintComponent)) continue;
                result[field.Name] = DescribeGraph(member, depth + 1, seen, maxDepth);
            }
            return result;
        }
    }
}
