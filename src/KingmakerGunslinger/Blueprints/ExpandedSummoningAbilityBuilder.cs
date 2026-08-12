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
        private static readonly string[] NativeMonsterTemplateBuffs = {
            "83a8a909e7ad19b4fa57e306884cc8bd",
            "c707fdbe58d0c614a89a872b76777f4b",
            "ec906031bb8766f42a37a52cf1d89836",
            "4b808ccd7e0b7bd43b10b2d47733b1a4" };

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            SummonIconCatalog.Validate();
            ConfigureNativeTierOnePreservation(library, bySymbol,
                MonsterParents[0],
                ExpandedSummoningIdentityCatalog.NativeMonsterTierOneSymbol);
            ConfigureNativeTierOnePreservation(library, bySymbol,
                AllyParents[0],
                ExpandedSummoningIdentityCatalog.NativeNaturesAllyTierOneSymbol);
            BlueprintBuff[] replacedNativeTemplateBuffs =
                NativeMonsterTemplateBuffs.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                        guid, "native summon template buff")).ToArray();
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
                    SummonTemplateBand band = SummonTemplateBandPolicy.Select(
                        HitDice(unit));
                    BlueprintBuff celestialSmite = Require<BlueprintBuff>(bySymbol,
                        "KMG.Summoning.Smite.Celestial.Available");
                    BlueprintBuff fiendishSmite = Require<BlueprintBuff>(bySymbol,
                        "KMG.Summoning.Smite.Fiendish.Available");
                    ConfigureOne(celestial, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Celestial." + band),
                        celestialSmite, replacedNativeTemplateBuffs, true,
                        SummonAlignmentMode.Celestial);
                    ConfigureOne(fiendish, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Fiendish." + band),
                        fiendishSmite, replacedNativeTemplateBuffs, false,
                        SummonAlignmentMode.Fiendish);
                    ConfigureDynamicTemplate(ability, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Celestial." + band),
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Fiendish." + band),
                        celestialSmite, fiendishSmite,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.AlignmentMode.FiendishMarker"),
                        replacedNativeTemplateBuffs);
                }
                else ConfigureOne(ability, native, unit, variant, null, null,
                    Array.Empty<BlueprintBuff>(), false,
                    family == SummonFamily.NaturesAlly ?
                        SummonAlignmentMode.Caster : (SummonAlignmentMode?)null);
                Sprite icon = IconFor(variant.Creature, unit, bySymbol,
                    native.Icon);
                BlueprintUnitFactAccess.Resolve().SetIcon(ability, icon);
                if (family == SummonFamily.Monster &&
                    variant.Creature.MonsterTemplated)
                {
                    BlueprintUnitFactAccess.Resolve().SetIcon(
                        Require<BlueprintAbility>(bySymbol,
                            symbol + ".Celestial"), icon);
                    BlueprintUnitFactAccess.Resolve().SetIcon(
                        Require<BlueprintAbility>(bySymbol,
                            symbol + ".Fiendish"), icon);
                }
            }
        }

        private static Sprite IconFor(SummonCreatureSpec creature,
            BlueprintUnit unit,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            Sprite fallback)
        {
            if (unit.PortraitSafe != null &&
                unit.PortraitSafe.SmallPortrait != null)
                return unit.PortraitSafe.SmallPortrait;
            string representativeKey = SummonIconCatalog.RepresentativeFor(
                SummonIconCatalog.CategoryFor(creature.Key));
            SummonCreatureSpec representative = ExpandedSummoningCatalog.All
                .Single(value => value.Key == representativeKey);
            BlueprintUnit representativeUnit = Require<BlueprintUnit>(bySymbol,
                ExpandedSummoningIdentityCatalog.UnitSymbol(representative));
            if (representativeUnit.PortraitSafe != null &&
                representativeUnit.PortraitSafe.SmallPortrait != null)
                return representativeUnit.PortraitSafe.SmallPortrait;
            if (fallback == null) throw new InvalidOperationException(
                "Summon option has neither creature/category portrait nor " +
                "native fallback icon: " + creature.Key + ".");
            return fallback;
        }

        private static void ConfigureNativeTierOnePreservation(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string parentGuid, string symbol)
        {
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, parentGuid,
                    "native direct tier-one summon parent");
            if ((parent.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityVariants>().Any())
                throw new InvalidOperationException(
                    "Native tier-one preservation requires a direct ability: " +
                    parentGuid);
            BlueprintAbility target = Require<BlueprintAbility>(bySymbol, symbol);
            CopyFields(parent, target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(DeepCloneComponent).ToArray();
            target.MaterialComponent = parent.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() :
                parent.MaterialComponent;
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
            BlueprintBuff templateBuff, BlueprintBuff smiteBuff,
            BlueprintBuff[] replacedNativeTemplateBuffs, bool celestial,
            SummonAlignmentMode? alignmentMode)
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
                AppendTemplateBuff(target.ComponentsArray, templateBuff,
                    replacedNativeTemplateBuffs);
                AppendTemplateBuff(target.ComponentsArray, smiteBuff,
                    replacedNativeTemplateBuffs);
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
            if (alignmentMode.HasValue)
                AppendAlignmentAction(target.ComponentsArray,
                    alignmentMode.Value);
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

        private static void ConfigureDynamicTemplate(BlueprintAbility target,
            BlueprintAbility native, BlueprintUnit unit,
            SummonVariantSpec variant, BlueprintBuff celestialTemplate,
            BlueprintBuff fiendishTemplate, BlueprintBuff celestialSmite,
            BlueprintBuff fiendishSmite, BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog
                .AbilitySymbol(variant));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value =>
                    !(value is AbilityVariants) &&
                    !(value is AbilityCasterAlignment))
                .Select(DeepCloneComponent).ToArray();
            int replacements = ReplaceSpawnUnits(target.ComponentsArray, unit);
            if (replacements < 1) throw new InvalidOperationException(
                "Expected a direct native spawn graph for " +
                variant.StableKey + ".");
            AppendDynamicTemplate(target.ComponentsArray, celestialTemplate,
                fiendishTemplate, celestialSmite, fiendishSmite,
                neutralFiendishMode, replacedNativeTemplateBuffs);
            SpellDescriptorComponent descriptor = target.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            descriptor.Descriptor &= ~(SpellDescriptor.Good |
                SpellDescriptor.Evil);
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
                    "Summons a celestial or fiendish " +
                    variant.Creature.DisplayName + " based on the caster's " +
                    "alignment. Neutral casters use their persistent " +
                    "Fiendish Summoning mode."), native.Icon);
        }

        private static void AppendDynamicTemplate(
            IEnumerable<BlueprintComponent> components,
            BlueprintBuff celestialTemplate, BlueprintBuff fiendishTemplate,
            BlueprintBuff celestialSmite, BlueprintBuff fiendishSmite,
            BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendDynamicTemplate(component, celestialTemplate,
                    fiendishTemplate, celestialSmite, fiendishSmite,
                    neutralFiendishMode, replacedNativeTemplateBuffs, seen,
                    ref count);
            if (count < 1) throw new InvalidOperationException(
                "A dynamic templated summon requires a spawn branch.");
        }

        private static void AppendDynamicTemplate(object value,
            BlueprintBuff celestialTemplate, BlueprintBuff fiendishTemplate,
            BlueprintBuff celestialSmite, BlueprintBuff fiendishSmite,
            BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs, ISet<object> seen,
            ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionApplySummonTemplateByCaster apply = ScriptableObject
                    .CreateInstance<ContextActionApplySummonTemplateByCaster>();
                apply.CelestialTemplate = celestialTemplate;
                apply.FiendishTemplate = fiendishTemplate;
                apply.CelestialSmite = celestialSmite;
                apply.FiendishSmite = fiendishSmite;
                apply.NeutralFiendishMode = neutralFiendishMode;
                apply.ReplacedNativeTemplateBuffs =
                    replacedNativeTemplateBuffs ?? Array.Empty<BlueprintBuff>();
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] {
                        apply }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendDynamicTemplate(item, celestialTemplate,
                            fiendishTemplate, celestialSmite, fiendishSmite,
                            neutralFiendishMode, replacedNativeTemplateBuffs,
                            seen, ref count);
                else AppendDynamicTemplate(child, celestialTemplate,
                    fiendishTemplate, celestialSmite, fiendishSmite,
                    neutralFiendishMode, replacedNativeTemplateBuffs, seen,
                    ref count);
            }
        }

        private static void AppendTemplateBuff(
            IEnumerable<BlueprintComponent> components, BlueprintBuff buff,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendTemplateBuff(component, buff, replacedNativeTemplateBuffs,
                    seen, ref count);
            if (count < 1) throw new InvalidOperationException(
                "A templated summon requires at least one spawn branch.");
        }

        private static void AppendAlignmentAction(
            IEnumerable<BlueprintComponent> components,
            SummonAlignmentMode mode)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendAlignmentAction(component, mode, seen, ref count);
            if (count < 1) throw new InvalidOperationException(
                "A summon alignment action requires at least one spawn branch.");
        }

        private static void AppendAlignmentAction(object value,
            SummonAlignmentMode mode, ISet<object> seen, ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionSetSummonAlignment alignment = ScriptableObject
                    .CreateInstance<ContextActionSetSummonAlignment>();
                alignment.Mode = mode;
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] {
                        alignment
                    }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendAlignmentAction(item, mode, seen, ref count);
                else AppendAlignmentAction(child, mode, seen, ref count);
            }
        }

        private static void AppendTemplateBuff(object value, BlueprintBuff buff,
            BlueprintBuff[] replacedNativeTemplateBuffs, ISet<object> seen,
            ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionApplySummonBuff apply = ScriptableObject
                    .CreateInstance<ContextActionApplySummonBuff>();
                apply.Buff = buff;
                apply.ReplacedNativeTemplateBuffs =
                    replacedNativeTemplateBuffs ?? Array.Empty<BlueprintBuff>();
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
                        AppendTemplateBuff(item, buff,
                            replacedNativeTemplateBuffs, seen, ref count);
                else AppendTemplateBuff(child, buff,
                    replacedNativeTemplateBuffs, seen, ref count);
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

        internal static BlueprintComponent DeepCloneComponent(BlueprintComponent source)
        { return (BlueprintComponent)DeepClone(source,
            new Dictionary<object, object>(ReferenceComparer.Instance)); }

        private static object DeepClone(object source, IDictionary<object, object> seen)
        {
            if (source == null) return null;
            Type type = source.GetType();
            if (type == typeof(ActionList))
            {
                ActionList actions = (ActionList)source;
                return new ActionList {
                    Actions = (actions.Actions ?? Array.Empty<GameAction>())
                        .Select(action => (GameAction)DeepClone(action, seen))
                        .ToArray()
                };
            }
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type.IsValueType) return source;
            BlueprintScriptableObject blueprint = source as BlueprintScriptableObject;
            if (blueprint != null) return blueprint;
            UnityEngine.Object unity = source as UnityEngine.Object;
            if (unity != null && !(source is BlueprintComponent) &&
                !(source is GameAction)) return unity;
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
            object result = source is BlueprintComponent || source is GameAction ?
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

        internal static void CopyFields(BlueprintAbility source,
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
