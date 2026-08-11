using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningAbilityBuilder
    {
        private static readonly string[] MonsterParents = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0" };
        private static readonly string[] AllyParents = {
            "c6147854641924442a3bb736080cfeb6", "298148133cdc3fd42889b99c82711986",
            "fdcf7e57ec44f704591f11b45f4acf61", "c83db50513abdf74ca103651931fac4b",
            "8f98a22f35ca6684a983363d32e51bfe", "55bbce9b3e76d4a4a8c8e0698d29002c",
            "051b979e7d7f8ec41b9fa35d04746b33", "ea78c04f0bd13d049a1cce5daf8d83e0",
            "a7469ef84ba50ac4cbf3d145e3173f8e" };

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in
                ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                BlueprintAbility ability = Require<BlueprintAbility>(bySymbol, symbol);
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol,
                    ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature));
                BlueprintAbility native = NativeTemplate(library, variant);
                ConfigureOne(ability, native, unit, variant, false);
                if (family == SummonFamily.Monster &&
                    variant.Creature.MonsterTemplated)
                {
                    ConfigureOne(Require<BlueprintAbility>(bySymbol,
                        symbol + ".Celestial"), native, unit, variant, true);
                    ConfigureOne(Require<BlueprintAbility>(bySymbol,
                        symbol + ".Fiendish"), native, unit, variant, true);
                }
            }
        }

        private static BlueprintAbility NativeTemplate(
            LibraryScriptableObject library, SummonVariantSpec variant)
        {
            string guid = (variant.Family == SummonFamily.Monster ?
                MonsterParents : AllyParents)[variant.ParentTier - 1];
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, guid, "native summon parent tier " + variant.ParentTier);
            AbilityVariants variants = (parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>()
                .SingleOrDefault();
            BlueprintAbility[] choices = variants == null ? new[] { parent } :
                (variants.Variants ?? Array.Empty<BlueprintAbility>());
            string token = variant.Multiplicity == SummonMultiplicity.One ? "single" :
                variant.Multiplicity == SummonMultiplicity.OneD3 ? "d3" : "d4";
            BlueprintAbility result = choices.SingleOrDefault(value => value != null &&
                value.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
            if (result == null) throw new InvalidOperationException(
                "Native summon template missing for " + variant.StableKey + ".");
            return result;
        }

        private static void ConfigureOne(BlueprintAbility target,
            BlueprintAbility native, BlueprintUnit unit, SummonVariantSpec variant,
            bool hiddenExecution)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog.AbilitySymbol(
                variant) + (hiddenExecution ? ".Execution" : ""));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(DeepCloneComponent).ToArray();
            int replacements = ReplaceSpawnUnits(target.ComponentsArray, unit);
            if (replacements != 1) throw new InvalidOperationException(
                "Expected exactly one native spawn action for " + variant.StableKey + ".");
            target.Hidden = hiddenExecution;
            target.ActionBarAutoFillIgnored = hiddenExecution;
            target.MaterialComponent = native.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() : native.MaterialComponent;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Name", variant.Creature.DisplayName +
                    QuantitySuffix(variant.Multiplicity)),
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Description",
                    "Summons " + variant.Creature.DisplayName +
                    " through the native summon lifecycle."), native.Icon);
        }

        private static string QuantitySuffix(SummonMultiplicity value)
        { return value == SummonMultiplicity.One ? "" : value ==
            SummonMultiplicity.OneD3 ? " (1d3)" : " (1d4+1)"; }

        private static BlueprintComponent DeepCloneComponent(BlueprintComponent source)
        { return (BlueprintComponent)DeepClone(source,
            new Dictionary<object, object>(ReferenceComparer.Instance)); }

        private static object DeepClone(object source, IDictionary<object, object> seen)
        {
            if (source == null) return null;
            Type type = source.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type.IsValueType) return source;
            BlueprintScriptableObject blueprint = source as BlueprintScriptableObject;
            if (blueprint != null) return blueprint;
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

        private static int ReplaceSpawnUnits(IEnumerable<BlueprintComponent> components,
            BlueprintUnit unit)
        {
            int count = 0;
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            foreach (BlueprintComponent component in components)
                Replace(component, unit, seen, ref count);
            return count;
        }

        private static void Replace(object value, BlueprintUnit unit,
            ISet<object> seen, ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            Type type = value.GetType();
            foreach (FieldInfo field in Fields(type))
            {
                if (type.Name == "ContextActionSpawnMonster" &&
                    field.Name == "Blueprint" &&
                    typeof(BlueprintUnit).IsAssignableFrom(field.FieldType))
                { field.SetValue(value, unit); count++; continue; }
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence) Replace(item, unit, seen, ref count);
                else Replace(child, unit, seen, ref count);
            }
        }

        private static void CopyFields(BlueprintAbility source,
            BlueprintAbility target)
        {
            foreach (FieldInfo field in Fields(typeof(BlueprintAbility)))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static T Require<T>(IDictionary<string, BlueprintScriptableObject> values,
            string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
