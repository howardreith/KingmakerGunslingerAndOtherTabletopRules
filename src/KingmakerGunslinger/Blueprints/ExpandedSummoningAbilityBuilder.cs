using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
                if (family == SummonFamily.Monster &&
                    variant.Creature.MonsterTemplated)
                {
                    BlueprintAbility celestial = Require<BlueprintAbility>(bySymbol,
                        symbol + ".Celestial");
                    BlueprintAbility fiendish = Require<BlueprintAbility>(bySymbol,
                        symbol + ".Fiendish");
                    bool high = HitDice(unit) > 10;
                    ConfigureOne(celestial, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Celestial." +
                            (high ? "High" : "Low")), true);
                    ConfigureOne(fiendish, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Fiendish." +
                            (high ? "High" : "Low")), false);
                    ConfigureTemplateChoice(ability, native, variant,
                        celestial, fiendish);
                }
                else ConfigureOne(ability, native, unit, variant, null, false);
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
            if (variants == null)
            {
                if (variant.Multiplicity != SummonMultiplicity.One)
                    throw new InvalidOperationException(
                        "A direct summon parent cannot supply a quantity template.");
                return parent;
            }
            BlueprintAbility[] choices = variants.Variants ??
                Array.Empty<BlueprintAbility>();
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
            BlueprintBuff templateBuff, bool celestial)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog.AbilitySymbol(
                variant) + (templateBuff == null ? "" : celestial ?
                    ".Celestial" : ".Fiendish"));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(DeepCloneComponent).ToArray();
            int replacements = ReplaceSpawnUnits(target.ComponentsArray, unit);
            if (replacements < 1) throw new InvalidOperationException(
                "Expected at least one native spawn action for " + variant.StableKey + ".");
            if (templateBuff != null)
            {
                AppendTemplateBuff(target.ComponentsArray, templateBuff);
                var alignment = ScriptableObject.CreateInstance<AbilityCasterAlignment>();
                alignment.Alignment = celestial ? (AlignmentMaskType)63 :
                    (AlignmentMaskType)504;
                target.ComponentsArray = target.ComponentsArray.Concat(
                    new BlueprintComponent[] { alignment }).ToArray();
                SpellDescriptorComponent descriptor = target.ComponentsArray
                    .OfType<SpellDescriptorComponent>().Single();
                descriptor.Descriptor |= celestial ? SpellDescriptor.Good :
                    SpellDescriptor.Evil;
            }
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = templateBuff != null;
            target.MaterialComponent = native.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() : native.MaterialComponent;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + (templateBuff == null ? "" : celestial ?
                        ".Celestial" : ".Fiendish") + ".Name",
                    (templateBuff == null ? "" : celestial ? "Celestial " :
                        "Fiendish ") + variant.Creature.DisplayName +
                        QuantitySuffix(variant.Multiplicity)),
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + (templateBuff == null ? "" : celestial ?
                        ".Celestial" : ".Fiendish") + ".Description",
                    "Summons " + variant.Creature.DisplayName +
                    " through the native summon lifecycle."), native.Icon);
        }

        private static void ConfigureTemplateChoice(BlueprintAbility target,
            BlueprintAbility native, SummonVariantSpec variant,
            BlueprintAbility celestial, BlueprintAbility fiendish)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog
                .AbilitySymbol(variant));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value =>
                    !(value is AbilityEffectRunAction) && !(value is AbilityVariants))
                .Select(DeepCloneComponent).ToArray();
            var choices = ScriptableObject.CreateInstance<AbilityVariants>();
            choices.Variants = new[] { celestial, fiendish };
            target.ComponentsArray = target.ComponentsArray.Concat(
                new BlueprintComponent[] { choices }).ToArray();
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = false;
            target.MaterialComponent = native.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() : native.MaterialComponent;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Name", variant.Creature.DisplayName +
                    QuantitySuffix(variant.Multiplicity)),
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Description",
                    "Choose a celestial or fiendish " +
                    variant.Creature.DisplayName + "."), native.Icon);
        }

        private static void AppendTemplateBuff(
            IEnumerable<BlueprintComponent> components, BlueprintBuff buff)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendTemplateBuff(component, buff, seen, ref count);
            if (count < 1) throw new InvalidOperationException(
                "A templated summon requires at least one spawn branch.");
        }

        private static void AppendTemplateBuff(object value, BlueprintBuff buff,
            ISet<object> seen, ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                var apply = new ContextActionApplyBuff {
                    Buff = buff, Permanent = true, IsNotDispelable = true,
                    AsChild = true };
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] { apply }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendTemplateBuff(item, buff, seen, ref count);
                else AppendTemplateBuff(child, buff, seen, ref count);
            }
        }

        private static int HitDice(BlueprintUnit unit)
        {
            int total = 0;
            foreach (BlueprintComponent component in unit.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
            {
                if (component.GetType().Name != "AddClassLevels") continue;
                FieldInfo levels = Fields(component.GetType()).Single(value =>
                    value.Name == "Levels" && value.FieldType == typeof(int));
                total += (int)levels.GetValue(component);
            }
            return total;
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
