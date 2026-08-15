using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurTransmutationInventory
    {
        internal static BrownFurSpellInventoryEvidence Observe(
            CotwArcanistContract contract)
        {
            if (contract == null || contract.CastingSpellbook == null ||
                contract.CastingSpellbook.SpellList == null)
                throw new ArgumentException("A resolved CotW casting spellbook is required.",
                    "contract");
            BlueprintSpellbook book = contract.CastingSpellbook;
            var levels = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            var roots = new Dictionary<string, BlueprintAbility>(StringComparer.Ordinal);
            foreach (SpellLevelList row in book.SpellList.SpellsByLevel ??
                new SpellLevelList[0])
            {
                if (row == null || row.Spells == null) continue;
                foreach (BlueprintAbility ability in row.Spells.Where(value =>
                    value != null && value.Type == AbilityType.Spell &&
                    value.School == SpellSchool.Transmutation))
                {
                    roots[ability.AssetGuid] = ability;
                    List<int> spellLevels;
                    if (!levels.TryGetValue(ability.AssetGuid, out spellLevels))
                    {
                        spellLevels = new List<int>();
                        levels.Add(ability.AssetGuid, spellLevels);
                    }
                    if (!spellLevels.Contains(row.SpellLevel))
                        spellLevels.Add(row.SpellLevel);
                }
            }

            var all = new Dictionary<string, BlueprintAbility>(roots,
                StringComparer.Ordinal);
            var parents = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (BlueprintAbility root in roots.Values.ToArray())
                ExpandVariants(root, all, parents, new HashSet<string>(
                    StringComparer.Ordinal));

            var records = new List<BrownFurSpellInventoryRecord>();
            foreach (BlueprintAbility ability in all.Values.OrderBy(value =>
                value.AssetGuid, StringComparer.Ordinal))
            {
                string parentGuid;
                parents.TryGetValue(ability.AssetGuid, out parentGuid);
                List<int> spellLevels;
                if (!levels.TryGetValue(ability.AssetGuid, out spellLevels) &&
                    parentGuid != null) levels.TryGetValue(parentGuid, out spellLevels);
                Graph graph = Graph.Observe(ability);
                string[] variants = Variants(ability).Where(value => value != null)
                    .Select(value => value.AssetGuid).Distinct().OrderBy(value => value,
                        StringComparer.Ordinal).ToArray();
                var record = new BrownFurSpellInventoryRecord
                {
                    CanonicalSpellGuid = ability.AssetGuid,
                    BlueprintName = ability.name ?? string.Empty,
                    LocalizedName = Convert.ToString(ability.Name,
                        System.Globalization.CultureInfo.InvariantCulture),
                    ParentGuid = parentGuid ?? (ability.Parent == null ? string.Empty :
                        ability.Parent.AssetGuid),
                    VariantGuids = variants.ToList(),
                    ConvertedFrom = "runtime AbilityData relationship; validate in cast fixtures",
                    SpellLevels = (spellLevels ?? new List<int>()).OrderBy(value => value).ToList(),
                    SpellbookSourceGuid = book.AssetGuid,
                    Range = ability.Range.ToString(),
                    TargetAnchor = "self=" + ability.CanTargetSelf + ";friends=" +
                        ability.CanTargetFriends + ";enemies=" + ability.CanTargetEnemies +
                        ";point=" + ability.CanTargetPoint,
                    TargetRestrictions = graph.TargetRestrictions,
                    Duration = Convert.ToString(ability.LocalizedDuration,
                        System.Globalization.CultureInfo.InvariantCulture),
                    MetamagicSupport = ability.AvailableMetamagic.ToString(),
                    SupportsExtend = (ability.AvailableMetamagic &
                        Metamagic.Extend) != 0,
                    AppliedBuffs = graph.AppliedBuffs,
                    NestedActionGraph = graph.Nodes,
                    AbilityScoreBonuses = graph.AbilityBonuses,
                    AbilityBonusCarrierFamilies = graph.AbilityBonusCarrierFamilies,
                    ModifierDescriptors = graph.Descriptors,
                    ValuePatterns = graph.Values,
                    PolymorphAndSizeComponents = graph.PolymorphAndSize,
                    HardCodedToCaster = graph.ToCaster,
                    SaveAndDispel = "savingThrow=" + ability.LocalizedSavingThrow +
                        ";spellResistance=" + ability.SpellResistance +
                        ";buffSemantics=" + string.Join("|", graph.BuffSemantics.ToArray())
                };
                BrownFurInventoryClassificationDecision classification =
                    BrownFurInventoryClassificationPolicy.Decide(
                        new BrownFurInventoryClassificationInput(
                            record.CanonicalSpellGuid, record.Range,
                            record.VariantGuids.Count > 0,
                            record.SupportsExtend, record.Duration,
                            record.AbilityScoreBonuses.Count,
                            record.AbilityBonusCarrierFamilies,
                            record.HardCodedToCaster.Count));
                record.ShareTransmutationCompatibility =
                    classification.ShareTransmutation;
                record.PowerfulChangeCompatibility =
                    classification.PowerfulChange;
                record.TransmutationSupremacyCompatibility =
                    classification.TransmutationSupremacy;
                record.RequiredAdapter = classification.RequiredAdapter;
                record.QualificationStatus =
                    classification.QualificationStatus;
                records.Add(record);
            }

            return new BrownFurSpellInventoryEvidence
            {
                SchemaVersion = 2,
                GeneratedAtUtc = DateTime.UtcNow.ToString("o"),
                CotwFingerprint = contract.Fingerprint == null ? string.Empty :
                    contract.Fingerprint.ToString(),
                RootSpellCount = roots.Count,
                RecordCountIncludingVariants = records.Count,
                PersonalSpellCount = records.Count(value => value.Range ==
                    AbilityRange.Personal.ToString()),
                AbilityBonusCandidateCount = records.Count(value =>
                    value.AbilityScoreBonuses.Count > 0),
                HardCodedToCasterCount = records.Count(value =>
                    value.HardCodedToCaster.Count > 0),
                QualificationCounts = records.GroupBy(value =>
                    value.QualificationStatus, StringComparer.Ordinal).ToDictionary(
                        value => value.Key, value => value.Count(), StringComparer.Ordinal),
                Records = records
            };
        }

        private static void ExpandVariants(BlueprintAbility ability,
            IDictionary<string, BlueprintAbility> all,
            IDictionary<string, string> parents, ISet<string> visiting)
        {
            if (ability == null || !visiting.Add(ability.AssetGuid)) return;
            foreach (BlueprintAbility variant in Variants(ability))
            {
                if (variant == null) continue;
                all[variant.AssetGuid] = variant;
                string existing;
                if (parents.TryGetValue(variant.AssetGuid, out existing) &&
                    existing != ability.AssetGuid)
                    throw new InvalidOperationException("Variant has ambiguous parents: " +
                        variant.AssetGuid);
                parents[variant.AssetGuid] = ability.AssetGuid;
                ExpandVariants(variant, all, parents, visiting);
            }
            visiting.Remove(ability.AssetGuid);
        }

        private static BlueprintAbility[] Variants(BlueprintAbility ability)
        {
            return (ability.ComponentsArray ?? new BlueprintComponent[0])
                .OfType<AbilityVariants>().SelectMany(value => value.Variants ??
                    new BlueprintAbility[0]).ToArray();
        }

        private sealed class Graph
        {
            private readonly HashSet<object> _visited = new HashSet<object>(
                ReferenceComparer.Instance);
            internal List<string> Nodes = new List<string>();
            internal List<string> AppliedBuffs = new List<string>();
            internal List<string> AbilityBonuses = new List<string>();
            internal List<string> AbilityBonusCarrierFamilies =
                new List<string>();
            internal List<string> Descriptors = new List<string>();
            internal List<string> Values = new List<string>();
            internal List<string> PolymorphAndSize = new List<string>();
            internal List<string> ToCaster = new List<string>();
            internal List<string> TargetRestrictions = new List<string>();
            internal List<string> BuffSemantics = new List<string>();

            internal static Graph Observe(BlueprintAbility ability)
            {
                var graph = new Graph();
                foreach (BlueprintComponent component in ability.ComponentsArray ??
                    new BlueprintComponent[0]) graph.Walk(component,
                        "ability.components", 0);
                graph.SortDistinct();
                return graph;
            }

            private void Walk(object value, string path, int depth)
            {
                if (value == null || depth > 24 || value is string) return;
                Type type = value.GetType();
                if (Scalar(type)) return;
                if (!_visited.Add(value)) return;
                if (value is BlueprintBuff)
                {
                    BlueprintBuff buff = (BlueprintBuff)value;
                    if (!IsAppliedBuffPath(path))
                    {
                        Nodes.Add(path + "=BlueprintBuffReference:" + buff.AssetGuid +
                            "/" + buff.name);
                        return;
                    }
                    AppliedBuffs.Add(path + "=" + buff.AssetGuid + "/" + buff.name);
                    BuffSemantics.Add(buff.AssetGuid + ":components=" +
                        (buff.ComponentsArray == null ? 0 : buff.ComponentsArray.Length));
                    foreach (BlueprintComponent component in buff.ComponentsArray ??
                        new BlueprintComponent[0]) Walk(component, path + ".buff", depth + 1);
                    return;
                }
                if (value is BlueprintAbility)
                {
                    BlueprintAbility ability = (BlueprintAbility)value;
                    Nodes.Add(path + "=BlueprintAbility:" + ability.AssetGuid + "/" +
                        ability.name);
                    return;
                }
                if (value is BlueprintAbilityAreaEffect)
                {
                    BlueprintAbilityAreaEffect area = (BlueprintAbilityAreaEffect)value;
                    Nodes.Add(path + "=BlueprintAbilityAreaEffect:" + area.AssetGuid +
                        "/" + area.name);
                    foreach (BlueprintComponent component in area.ComponentsArray ??
                        new BlueprintComponent[0]) Walk(component, path + ".area", depth + 1);
                    return;
                }
                if (value is BlueprintScriptableObject)
                {
                    BlueprintScriptableObject blueprint =
                        (BlueprintScriptableObject)value;
                    Nodes.Add(path + "=BlueprintReference:" + blueprint.AssetGuid +
                        "/" + blueprint.name);
                    return;
                }
                if (value is IEnumerable)
                {
                    int index = 0;
                    foreach (object item in (IEnumerable)value)
                    {
                        if (index >= 512) break;
                        Walk(item, path + "[" + index + "]", depth + 1);
                        index++;
                    }
                    return;
                }

                string fullName = type.FullName ?? type.Name;
                Nodes.Add(path + "=" + fullName);
                string carrier = DescribeAbilityBonusCarrier(value, type, path);
                if (!string.IsNullOrEmpty(carrier))
                {
                    AbilityBonuses.Add(carrier);
                    AbilityBonusCarrierFamilies.Add(fullName);
                }
                if (Contains(fullName, "Polymorph") ||
                    Contains(fullName, "ChangeUnitSize"))
                    PolymorphAndSize.Add(path + "=" + fullName);
                if (Contains(fullName, "Target") || Contains(fullName, "Condition"))
                    TargetRestrictions.Add(path + "=" + fullName);

                foreach (FieldInfo field in Fields(type))
                {
                    object member;
                    try { member = field.GetValue(value); }
                    catch { continue; }
                    string memberPath = path + "." + field.Name;
                    if (member == null) continue;
                    Type memberType = member.GetType();
                    string text = Convert.ToString(member,
                        System.Globalization.CultureInfo.InvariantCulture);
                    if (field.Name.IndexOf("ToCaster", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        member is bool && (bool)member)
                        ToCaster.Add(memberPath + "=true");
                    if (field.Name.IndexOf("Descriptor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        memberType.Name.IndexOf("ModifierDescriptor",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        Descriptors.Add(memberPath + "=" + text);
                    if (RelevantValue(field.Name, memberType))
                        Values.Add(memberPath + "=" + text);
                    if (!Scalar(memberType)) Walk(member, memberPath, depth + 1);
                }
            }

            private static IEnumerable<FieldInfo> Fields(Type type)
            {
                for (Type cursor = type; cursor != null && cursor != typeof(object);
                    cursor = cursor.BaseType)
                    foreach (FieldInfo field in cursor.GetFields(BindingFlags.Instance |
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly).OrderBy(value => value.Name,
                            StringComparer.Ordinal))
                        yield return field;
            }

            private static bool RelevantValue(string name, Type type)
            {
                string value = name.ToLowerInvariant();
                return value.Contains("value") || value.Contains("bonus") ||
                    value.Contains("stat") || value.Contains("rank") ||
                    value.Contains("duration") || value.Contains("descriptor") ||
                    value.Contains("caster") || value.Contains("size") ||
                    type.IsEnum;
            }

            private static bool Scalar(Type type)
            {
                return type.IsPrimitive || type.IsEnum || type == typeof(decimal) ||
                    type == typeof(string) || type == typeof(Type);
            }

            private static bool Contains(string value, string token)
            { return value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0; }

            private static bool IsAppliedBuffPath(string path)
            {
                if (path.IndexOf(".Conditions", StringComparison.Ordinal) >= 0 ||
                    path.IndexOf(".m_Condition", StringComparison.Ordinal) >= 0)
                    return false;
                int action = path.LastIndexOf(".Actions[",
                    StringComparison.Ordinal);
                int traversedBuff = path.LastIndexOf(".buff",
                    StringComparison.Ordinal);
                return action >= 0 && action > traversedBuff;
            }

            private static string DescribeAbilityBonusCarrier(object value,
                Type type, string path)
            {
                string fullName = type.FullName ?? type.Name;
                bool polymorph = fullName == "Kingmaker.UnitLogic.Buffs.Polymorph";
                bool size = fullName ==
                    "Kingmaker.Designers.Mechanics.Buffs.ChangeUnitSize";
                bool statCarrier = Contains(fullName, "AddStatBonus") ||
                    Contains(fullName, "AddContextStatBonus") ||
                    Contains(fullName, "AddGenericStatBonus");
                if (!polymorph && !size && !statCarrier) return string.Empty;

                var details = new List<string>();
                bool abilityStat = size;
                bool positiveBonus = size;
                bool valueFieldSeen = false;
                foreach (FieldInfo field in Fields(type))
                {
                    object member;
                    try { member = field.GetValue(value); }
                    catch { continue; }
                    if (member == null) continue;
                    string text = Convert.ToString(member,
                        System.Globalization.CultureInfo.InvariantCulture);
                    if (IsAbilityStat(text)) abilityStat = true;
                    string name = field.Name.ToLowerInvariant();
                    if (polymorph && (name == "strengthbonus" ||
                        name == "dexteritybonus" || name == "constitutionbonus"))
                    {
                        abilityStat = true;
                        valueFieldSeen = true;
                        if (IsPositive(text)) positiveBonus = true;
                    }
                    if (statCarrier && name == "value")
                    {
                        valueFieldSeen = true;
                        if (IsPositive(text)) positiveBonus = true;
                    }
                    if (name.Contains("stat") || name.Contains("bonus") ||
                        name.Contains("value") || name.Contains("descriptor") ||
                        name.Contains("size"))
                        details.Add(field.Name + "=" + text);
                }
                if (!abilityStat || !positiveBonus || (!valueFieldSeen && !size))
                    return string.Empty;
                return path + "=" + fullName + "{" +
                    string.Join(",", details.OrderBy(item => item,
                        StringComparer.Ordinal).ToArray()) + "}";
            }

            private static bool IsAbilityStat(string value)
            {
                return value == "Strength" || value == "Dexterity" ||
                    value == "Constitution" || value == "Intelligence" ||
                    value == "Wisdom" || value == "Charisma";
            }

            private static bool IsPositive(string value)
            {
                double number;
                return double.TryParse(value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out number) && number > 0;
            }

            private void SortDistinct()
            {
                Nodes = Distinct(Nodes); AppliedBuffs = Distinct(AppliedBuffs);
                AbilityBonuses = Distinct(AbilityBonuses);
                AbilityBonusCarrierFamilies = Distinct(
                    AbilityBonusCarrierFamilies);
                Descriptors = Distinct(Descriptors);
                Values = Distinct(Values); PolymorphAndSize = Distinct(PolymorphAndSize);
                ToCaster = Distinct(ToCaster); TargetRestrictions = Distinct(TargetRestrictions);
                BuffSemantics = Distinct(BuffSemantics);
            }

            private static List<string> Distinct(IEnumerable<string> values)
            { return values.Distinct(StringComparer.Ordinal).OrderBy(value => value,
                StringComparer.Ordinal).ToList(); }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object left, object right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(object value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
