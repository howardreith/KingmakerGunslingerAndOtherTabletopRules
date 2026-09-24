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
        private static JToken DescribeGraph(object value, int depth, HashSet<object> seen)
        {
            if (value == null) return null;
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null && depth > 0)
                return blueprint.name + ":" + blueprint.AssetGuid + ":" +
                    blueprint.GetType().Name;
            if (depth > 4 || !seen.Add(value)) return value.GetType().Name;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is string) return value.ToString();
            var array = value as IEnumerable;
            if (array != null && !(value is string))
            {
                var items = new JArray();
                foreach (object item in array)
                {
                    if (items.Count >= 32) break;
                    items.Add(DescribeGraph(item, depth + 1, seen));
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
                    .Select(c => DescribeGraph(c, depth + 1, seen)).ToArray());
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
                result[field.Name] = DescribeGraph(member, depth + 1, seen);
            }
            return result;
        }
    }
}
