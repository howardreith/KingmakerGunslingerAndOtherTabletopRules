using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ExpandedSummoningBlueprintSet
    {
        internal ExpandedSummoningBlueprintSet(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        { BySymbol = bySymbol ?? throw new ArgumentNullException("bySymbol"); }
        internal IDictionary<string, BlueprintScriptableObject> BySymbol
        { get; private set; }
        internal int Count { get { return BySymbol.Count; } }
    }

    internal static class ExpandedSummoningBlueprints
    {
        private const string SummonedFactionDonorGuid =
            "1ed9a630f0d9d7f44855d3d1d1b2cdf2";

        internal static ExpandedSummoningBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            ExpandedSummoningCatalog.Validate();
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            ExpandedSummoningDonorCatalog.Validate();
            BlueprintUnit summonedFactionDonor =
                BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                    SummonedFactionDonorGuid, "dedicated native summon faction donor");
            var unitDonors = ExpandedSummoningCatalog.All.ToDictionary(
                ExpandedSummoningIdentityCatalog.UnitSymbol,
                creature => BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                    ExpandedSummoningDonorCatalog.For(creature.Key).Guid,
                    creature.DisplayName + " donor"), StringComparer.Ordinal);
            var registered = new Dictionary<string, BlueprintScriptableObject>(
                StringComparer.Ordinal);
            foreach (SummoningIdentitySpec identity in identities)
            {
                BlueprintScriptableObject blueprint;
                if (identity.PlannedType == "BlueprintUnit")
                    blueprint = registry.Register<BlueprintUnit>(identity.Symbol,
                        () => CloneUnitShell(unitDonors[identity.Symbol],
                            summonedFactionDonor, identity.Symbol));
                else if (identity.PlannedType == "BlueprintAbility")
                    blueprint = registry.Register<BlueprintAbility>(identity.Symbol,
                        () => CreateAbilityShell(identity.Symbol));
                else if (identity.PlannedType == "BlueprintBuff")
                    blueprint = registry.Register<BlueprintBuff>(identity.Symbol,
                        () => CreateBuffShell(identity.Symbol));
                else throw new InvalidOperationException(
                    "Unsupported Expanded Summoning planned type " +
                    identity.PlannedType + ".");
                registered.Add(identity.Symbol, blueprint);
            }
            var result = new ExpandedSummoningBlueprintSet(registered);
            if (result.Count != ExpandedSummoningIdentityCatalog.FoundationIdentityCount)
                throw new InvalidOperationException(
                    "Expanded Summoning registration count mismatch.");
            ExpandedSummoningTemplateBuilder.Configure(registered);
            ExpandedSummoningAbilityBuilder.Configure(library, registered);
            return result;
        }

        private static BlueprintUnit CloneUnitShell(BlueprintUnit donor,
            BlueprintUnit summonedFactionDonor, string symbol)
        {
            BlueprintUnit result = ScriptableObject.CreateInstance<BlueprintUnit>();
            CopyBlueprintFields(donor, result);
            result.name = InternalName(symbol);
            result.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(component => component != null &&
                    !IsForbiddenComponent(component.GetType().Name))
                .Select(DeepCloneUnitComponent).Select(SanitizeComponent).ToArray();
            FilterBlueprintArrayField(result, "AddFacts");
            ClearArrayField(result, "StartingInventory");
            SetSummonedFaction(result, summonedFactionDonor);
            return result;
        }

        private static BlueprintComponent SanitizeComponent(BlueprintComponent component)
        {
            foreach (FieldInfo field in Fields(component.GetType()))
            {
                if (!field.FieldType.IsArray) continue;
                Array values = field.GetValue(component) as Array;
                Type element = field.FieldType.GetElementType();
                if (field.Name == "MemorizeSpells" || field.Name == "SelectSpells")
                {
                    field.SetValue(component, Array.CreateInstance(element, 0));
                    continue;
                }
                if (values == null ||
                    !typeof(BlueprintScriptableObject).IsAssignableFrom(element)) continue;
                object[] retained = values.Cast<object>().Where(value =>
                {
                    BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
                    return blueprint != null &&
                        !SummonUnitSanitizationPolicy.IsForbiddenRuntimeMemberKey(
                            blueprint.name);
                }).ToArray();
                Array replacement = Array.CreateInstance(element, retained.Length);
                for (int index = 0; index < retained.Length; index++)
                    replacement.SetValue(retained[index], index);
                field.SetValue(component, replacement);
            }
            return component;
        }

        private static void ClearArrayField(object target, string name)
        {
            FieldInfo field = Fields(target.GetType()).SingleOrDefault(value =>
                value.Name == name && value.FieldType.IsArray);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            field.SetValue(target, Array.CreateInstance(
                field.FieldType.GetElementType(), 0));
        }

        private static void FilterBlueprintArrayField(object target, string name)
        {
            FieldInfo field = Fields(target.GetType()).SingleOrDefault(value =>
                value.Name == name && value.FieldType.IsArray);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            Type element = field.FieldType.GetElementType();
            Array source = field.GetValue(target) as Array;
            object[] retained = source == null ? Array.Empty<object>() :
                source.Cast<object>().Where(value =>
                {
                    BlueprintScriptableObject blueprint =
                        value as BlueprintScriptableObject;
                    return blueprint != null &&
                        !SummonUnitSanitizationPolicy.IsForbiddenRuntimeMemberKey(
                            blueprint.name);
                }).ToArray();
            Array replacement = Array.CreateInstance(element, retained.Length);
            for (int index = 0; index < retained.Length; index++)
                replacement.SetValue(retained[index], index);
            field.SetValue(target, replacement);
        }

        private static BlueprintComponent DeepCloneUnitComponent(
            BlueprintComponent source)
        {
            return (BlueprintComponent)DeepClone(source,
                new Dictionary<object, object>(ReferenceComparer.Instance));
        }

        private static object DeepClone(object source,
            IDictionary<object, object> seen)
        {
            if (source == null) return null;
            Type type = source.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type.IsValueType) return source;
            if (source is BlueprintScriptableObject) return source;
            UnityEngine.Object unity = source as UnityEngine.Object;
            if (unity != null && !(source is BlueprintComponent)) return unity;
            object existing;
            if (seen.TryGetValue(source, out existing)) return existing;
            Array array = source as Array;
            if (array != null)
            {
                Array copy = Array.CreateInstance(type.GetElementType(), array.Length);
                seen.Add(source, copy);
                for (int index = 0; index < array.Length; index++)
                    copy.SetValue(DeepClone(array.GetValue(index), seen), index);
                return copy;
            }
            object result = source is BlueprintComponent ?
                ScriptableObject.CreateInstance(type) :
                FormatterServices.GetUninitializedObject(type);
            seen.Add(source, result);
            foreach (FieldInfo field in Fields(type))
            {
                if (field.IsInitOnly || field.DeclaringType == typeof(UnityEngine.Object))
                    continue;
                field.SetValue(result, DeepClone(field.GetValue(source), seen));
            }
            return result;
        }

        private static void CopyBlueprintFields(BlueprintUnit source,
            BlueprintUnit target)
        {
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.DeclaredOnly;
            for (Type type = typeof(BlueprintUnit); type != null &&
                type != typeof(UnityEngine.Object); type = type.BaseType)
            foreach (System.Reflection.FieldInfo field in type.GetFields(flags))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static bool IsForbiddenComponent(string name)
        {
            string value = (name ?? string.Empty).ToLowerInvariant();
            return value.Contains("experience") || value.Contains("loot") ||
                value.Contains("inventory") || value.Contains("dialog") ||
                value.Contains("interaction") || value.Contains("quest") ||
                value.Contains("cutscene") || value.Contains("companion") ||
                value.Contains("pet") || value.Contains("area") ||
                value.Contains("story") || value.Contains("corpse") ||
                value.Contains("addtags") || value.Contains("mobcaster");
        }

        private static void SetSummonedFaction(BlueprintUnit target,
            BlueprintUnit donor)
        {
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;
            System.Reflection.FieldInfo field = null;
            for (Type type = typeof(BlueprintUnit); type != null && field == null;
                type = type.BaseType)
                field = type.GetFields(flags).SingleOrDefault(value =>
                    string.Equals(value.Name, "Faction", StringComparison.Ordinal) ||
                    string.Equals(value.Name, "m_Faction", StringComparison.Ordinal));
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintUnit).FullName, "Faction");
            field.SetValue(target, field.GetValue(donor));
        }

        private static BlueprintAbility CreateAbilityShell(string symbol)
        {
            BlueprintAbility result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName(symbol);
            result.Hidden = true;
            result.ActionBarAutoFillIgnored = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
            return result;
        }

        private static BlueprintBuff CreateBuffShell(string symbol)
        {
            BlueprintBuff result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = InternalName(symbol);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
