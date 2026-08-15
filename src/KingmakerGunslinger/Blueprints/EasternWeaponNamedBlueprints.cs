using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class EasternWeaponNamedBlueprints
    {
        internal const string EnhancementTwoGuid =
            "eb2faccc4c9487d43b3575d7e77ff3f5";
        internal const string EnhancementThreeGuid =
            "80bb8a737579e35498177e1e3c75899b";
        internal const string EnhancementFourGuid =
            "783d7d496da6ac44f9511011fc5f1979";
        internal const string EnhancementFiveGuid =
            "bdba267e951851449af552aa9f9e3992";
        internal const string FlamingGuid =
            "30f90becaaac51f41bf56641966c4121";
        internal const string FrostGuid =
            "421e54078b7719d40915ce0672511d0b";
        internal const string AgileGuid =
            "a36ad92c51789b44fa8a1c5c116a1328";
        internal const string KeenGuid =
            "102a9c8c9b7a75e4fb5844e79deaf4c0";
        internal const string GhostTouchGuid =
            "47857e1a5a3ec1a46adf6491b1423b4f";
        internal const string ShockGuid =
            "7bda5277d36ad114f9f9fd21d0dab658";
        internal const string ThunderingGuid =
            "690e762f7704e1f4aa1ac69ef0ce6a96";
        internal const string HolyGuid =
            "28a9964d81fedae44bae3ca45710c140";
        internal const string BrilliantEnergyGuid =
            "66e9e299c9002ea4bb65b6f300e43770";
        internal const string SpeedGuid =
            "f1c0c50108025d546b2554674ea1c006";
        internal const string PowerAttackFeatureGuid =
            "9972f33f977fc724c838e59641b2fca5";
        internal const string PowerAttackToggleGuid =
            "a7b339e4f6ff93a4697df5d7a87ff619";

        internal const string WayfarersOathFactSymbol =
            "KMG.EasternWeapons.Katana.WayfarersOath.EquippedFact";
        internal const string WayfarersOathFeatureSymbol =
            "KMG.EasternWeapons.Katana.WayfarersOath.EquippedFeature";
        internal const string FallingPetalEnchantmentSymbol =
            "KMG.EasternWeapons.Wakizashi.FallingPetal.EffectEnchantment";
        internal const string FallingPetalBuffSymbol =
            "KMG.EasternWeapons.Wakizashi.FallingPetal.ArmorClassBuff";
        internal const string MoonlitCrossingFactSymbol =
            "KMG.EasternWeapons.Katana.MoonlitCrossing.EquippedFact";
        internal const string MoonlitCrossingFeatureSymbol =
            "KMG.EasternWeapons.Katana.MoonlitCrossing.EquippedFeature";
        internal const string MountainSunderEnchantmentSymbol =
            "KMG.EasternWeapons.Nodachi.MountainSunder.EffectEnchantment";
        internal const string MountainSunderMarkerSymbol =
            "KMG.EasternWeapons.Nodachi.MountainSunder.RoundMarker";
        internal const string UnfixedFormEnchantmentSymbol =
            "KMG.EasternWeapons.Nodachi.UnfixedForm.EffectEnchantment";

        internal static EasternWeaponNamedBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            EasternWeaponBlueprintSet eastern, ModLogger logger)
        {
            if (library == null || registry == null || eastern == null ||
                logger == null) throw new ArgumentNullException(
                    "Eastern named registration inputs are incomplete.");
            Dictionary<string, BlueprintWeaponEnchantment> native =
                LoadNative(library);
            BlueprintFeature powerAttackFeature =
                BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    library, PowerAttackFeatureGuid,
                    "native Power Attack feat");
            BlueprintActivatableAbility powerAttack =
                BlueprintLibraryLookup.RequireExact<BlueprintActivatableAbility>(
                    library, PowerAttackToggleGuid,
                    "native Power Attack toggle");
            ValidatePowerAttackAuthority(powerAttackFeature, powerAttack);
            EasternWeaponNamedBuffSet buffs = RegisterBuffs(registry);
            EasternWeaponNamedEnchantmentSet custom = RegisterEnchantments(
                registry, buffs, powerAttack);
            var entries = new List<EasternWeaponNamedBlueprintEntry>();
            var typeAccess = WeaponBlueprintAccess.Resolve();
            var itemAccess = new EasternWeaponItemAccess();
            foreach (EasternWeaponNamedSpec spec in EasternWeaponNamedCatalog.All)
            {
                EasternWeaponFamilyBlueprintSet family = eastern.Require(spec.Family);
                BlueprintItemWeapon donor = family.Entries[0].Item;
                BlueprintWeaponEnchantment[] enchantments = Build(spec, native,
                    custom);
                BlueprintUnitFact equippedFact = buffs.ForEquipped(spec.Kind);
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                    spec.Symbol, delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                            donor, "KMG_EasternWeapons_" + spec.Kind);
                        typeAccess.Set(clone, family.WeaponType);
                        itemAccess.ConfigureNamed(clone, spec, enchantments,
                            Describe(spec));
                        Assets.EasternWeaponAssetRuntime.ApplyTo(clone,
                            spec.Symbol, spec.Family);
                        if (equippedFact != null)
                            AddEquipmentFact(clone, equippedFact, spec.Kind);
                        return clone;
                    });
                itemAccess.ValidateNamed(item, spec, enchantments);
                entries.Add(new EasternWeaponNamedBlueprintEntry(spec, item));
            }
            EasternWeaponNamedBlueprintEntry[] result = entries.ToArray();
            if (result.Length != 18 ||
                result.Select(value => value.Item).Distinct().Count() != 18 ||
                result.Any(value => !ReferenceEquals(typeAccess.Get(value.Item),
                    eastern.Require(value.Spec.Family).WeaponType)))
                throw new InvalidOperationException(
                    "Eastern named item registration is malformed.");
            BindItemReferences(result, buffs);
            MightyCleavingRuntime.Configure(result.Single(value =>
                value.Spec.Kind == EasternWeaponNamedKind.MountainSunder).Item);
            logger.Info("eastern-weapons", "named-native.ready",
                "Registered all eighteen save-stable named Eastern weapons with exact native enchantments and five exact bespoke-effect implementations.");
            return new EasternWeaponNamedBlueprintSet(result, buffs, custom);
        }

        private static void ValidatePowerAttackAuthority(
            BlueprintFeature feature, BlueprintActivatableAbility toggle)
        {
            AddFacts[] grants = feature.ComponentsArray.OfType<AddFacts>()
                .Where(value => value.Facts != null && value.Facts.Any(fact =>
                    ReferenceEquals(fact, toggle))).ToArray();
            PowerAttackWatcher[] watchers = feature.ComponentsArray
                .OfType<PowerAttackWatcher>().Where(value =>
                    ReferenceEquals(value.PowerAttackBlueprint, toggle))
                .ToArray();
            if (grants.Length != 1 || watchers.Length != 1)
                throw new InvalidOperationException(
                    "Native Power Attack authority changed: expected one " +
                    "AddFacts grant and one PowerAttackWatcher referencing " +
                    "the exact installed toggle.");
        }

        private static EasternWeaponNamedBuffSet RegisterBuffs(
            BlueprintRegistry registry)
        {
            var wayfarer = ScriptableObject.CreateInstance<
                EasternEquipmentStatBonus>();
            wayfarer.name = "$KMG_WayfarersOath_Initiative";
            wayfarer.Stat = StatType.Initiative;
            wayfarer.Value = 2;
            wayfarer.Descriptor = ModifierDescriptor.Competence;

            var falling = ScriptableObject.CreateInstance<
                EasternEquipmentStatBonus>();
            falling.name = "$KMG_FallingPetal_AC";
            falling.Stat = StatType.AC;
            falling.Value = 1;
            falling.Descriptor = ModifierDescriptor.Dodge;

            var moonlitArmor = ScriptableObject.CreateInstance<
                EasternEquipmentStatBonus>();
            moonlitArmor.name = "$KMG_MoonlitCrossing_AC";
            moonlitArmor.Stat = StatType.AC;
            moonlitArmor.Value = 1;
            moonlitArmor.Descriptor = ModifierDescriptor.Dodge;
            moonlitArmor.RequireOneHanded = true;
            var moonlitDamage = ScriptableObject.CreateInstance<
                MoonlitCrossingDamageBonus>();
            moonlitDamage.name = "$KMG_MoonlitCrossing_Damage";

            registry.Register<BlueprintBuff>(
                WayfarersOathFactSymbol, () => Buff(
                    "KMG_WayfarersOath_Equipped", "Wayfarer's Oath",
                    "+2 competence bonus to Initiative while Wayfarer's Oath is in the active equipment set.",
                    true));
            BlueprintFeature wayfarerFact = registry.Register<BlueprintFeature>(
                WayfarersOathFeatureSymbol, () => Feature(
                    "KMG_WayfarersOath_Equipped_Feature", "Wayfarer's Oath",
                    "+2 competence bonus to Initiative while Wayfarer's Oath is in the active equipment set.",
                    wayfarer));
            BlueprintBuff fallingBuff = registry.Register<BlueprintBuff>(
                FallingPetalBuffSymbol, () => Buff(
                    "KMG_FallingPetal_AC_Buff", "Falling Petal",
                    "+1 dodge bonus to Armor Class for 1 round while Falling Petal remains the active weapon.",
                    false, falling));
            registry.Register<BlueprintBuff>(
                MoonlitCrossingFactSymbol, () => Buff(
                    "KMG_MoonlitCrossing_Equipped", "Moonlit Crossing",
                    "One-handed use grants +1 dodge AC; two-handed use grants +2 weapon damage.",
                    true));
            BlueprintFeature moonlitFact = registry.Register<BlueprintFeature>(
                MoonlitCrossingFeatureSymbol, () => Feature(
                    "KMG_MoonlitCrossing_Equipped_Feature", "Moonlit Crossing",
                    "One-handed use grants +1 dodge AC; two-handed use grants +2 weapon damage.",
                    moonlitArmor, moonlitDamage));
            BlueprintBuff mountainMarker = registry.Register<BlueprintBuff>(
                MountainSunderMarkerSymbol, () => Buff(
                    "KMG_MountainSunder_Round_Marker",
                    "Mountain-Sunder Round Marker",
                    "Internal marker enforcing Mountain-Sunder's once-per-round force damage.",
                    true));
            return new EasternWeaponNamedBuffSet(wayfarerFact, fallingBuff,
                moonlitFact, mountainMarker);
        }

        private static EasternWeaponNamedEnchantmentSet RegisterEnchantments(
            BlueprintRegistry registry, EasternWeaponNamedBuffSet buffs,
            BlueprintActivatableAbility powerAttack)
        {
            return new EasternWeaponNamedEnchantmentSet(
                EffectEnchantment(registry, FallingPetalEnchantmentSymbol,
                    "Falling Petal's Poise",
                    "A confirmed critical hit grants +1 dodge AC for 1 round while this exact weapon remains active.",
                    EasternNamedWeaponEffectKind.FallingPetal,
                    buffs.FallingPetal, null, null),
                EffectEnchantment(registry, MountainSunderEnchantmentSymbol,
                    "Mountain-Sunder",
                    "Mighty Cleaving permits one additional Cleave attack. While Power Attack is active, the first hit each round deals 1d6 force damage.",
                    EasternNamedWeaponEffectKind.MountainSunder, null,
                    buffs.MountainSunderMarker, powerAttack),
                EffectEnchantment(registry, UnfixedFormEnchantmentSymbol,
                    "Unfixed Form",
                    "While polymorphed or changed from natural size, this weapon's base damage advances one native weapon-size step.",
                    EasternNamedWeaponEffectKind.UnfixedForm, null, null,
                    null));
        }

        private static BlueprintWeaponEnchantment EffectEnchantment(
            BlueprintRegistry registry, string symbol, string name,
            string description, EasternNamedWeaponEffectKind kind,
            BlueprintBuff effect, BlueprintBuff marker,
            BlueprintActivatableAbility powerAttack)
        {
            return registry.Register<BlueprintWeaponEnchantment>(symbol,
                delegate
                {
                    var enchantment = ScriptableObject.CreateInstance<
                        BlueprintWeaponEnchantment>();
                    enchantment.name = "KMG_" + kind + "_Enchantment";
                    ConfigureEnchantmentText(enchantment, symbol, name,
                        description);
                    var component = ScriptableObject.CreateInstance<
                        EasternNamedWeaponEffectComponent>();
                    component.name = "$KMG_" + kind + "_Effect";
                    component.Kind = kind;
                    component.EffectBuff = effect;
                    component.RoundMarker = marker;
                    component.PowerAttack = powerAttack;
                    enchantment.ComponentsArray = new BlueprintComponent[] {
                        component };
                    return enchantment;
                });
        }

        private static Dictionary<string, BlueprintWeaponEnchantment> LoadNative(
            LibraryScriptableObject library)
        {
            string[] guids = {
                EasternWeaponBlueprints.NativeEnhancementOneGuid,
                EnhancementTwoGuid, EnhancementThreeGuid, EnhancementFourGuid,
                EnhancementFiveGuid, FlamingGuid, FrostGuid, AgileGuid, KeenGuid,
                GhostTouchGuid, ShockGuid, ThunderingGuid, HolyGuid,
                BrilliantEnergyGuid, SpeedGuid };
            return guids.ToDictionary(value => value, value =>
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    library, value, "native Eastern weapon enchantment"));
        }

        private static BlueprintWeaponEnchantment[] Build(
            EasternWeaponNamedSpec spec,
            IDictionary<string, BlueprintWeaponEnchantment> native,
            EasternWeaponNamedEnchantmentSet custom)
        {
            var result = new List<BlueprintWeaponEnchantment>
            {
                native[spec.Enhancement == 1
                    ? EasternWeaponBlueprints.NativeEnhancementOneGuid
                    : spec.Enhancement == 2 ? EnhancementTwoGuid
                    : spec.Enhancement == 3 ? EnhancementThreeGuid
                    : spec.Enhancement == 4 ? EnhancementFourGuid
                    : EnhancementFiveGuid]
            };
            Add(result, native, spec, EasternWeaponNativeProperty.Flaming,
                FlamingGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Frost,
                FrostGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Agile,
                AgileGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Keen,
                KeenGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.GhostTouch,
                GhostTouchGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Shock,
                ShockGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Thundering,
                ThunderingGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Holy,
                HolyGuid);
            Add(result, native, spec,
                EasternWeaponNativeProperty.BrilliantEnergy,
                BrilliantEnergyGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Speed,
                SpeedGuid);
            BlueprintWeaponEnchantment effect = custom.For(spec.Kind);
            if (effect != null) result.Add(effect);
            return result.ToArray();
        }

        private static void Add(ICollection<BlueprintWeaponEnchantment> result,
            IDictionary<string, BlueprintWeaponEnchantment> native,
            EasternWeaponNamedSpec spec, EasternWeaponNativeProperty property,
            string guid)
        {
            if (spec.Has(property)) result.Add(native[guid]);
        }

        private static string Describe(EasternWeaponNamedSpec spec)
        {
            var properties = new List<string> { "+" + spec.Enhancement };
            foreach (EasternWeaponNativeProperty property in Enum.GetValues(
                typeof(EasternWeaponNativeProperty)))
                if (property != EasternWeaponNativeProperty.None &&
                    spec.Has(property)) properties.Add(PropertyName(property));
            if (spec.ColdIron) properties.Add("Cold Iron");
            string effect = spec.Kind == EasternWeaponNamedKind.WayfarersOath
                ? " While active, it grants +2 competence to Initiative."
                : spec.Kind == EasternWeaponNamedKind.FallingPetal
                ? " A confirmed critical hit grants +1 dodge AC for 1 round while this weapon remains active."
                : spec.Kind == EasternWeaponNamedKind.MoonlitCrossing
                ? " One-handed use grants +1 dodge AC; two-handed use grants +2 weapon damage."
                : spec.Kind == EasternWeaponNamedKind.MountainSunder
                ? " Mighty Cleaving permits one additional Cleave attack. While Power Attack is active, its first hit each round deals 1d6 force damage."
                : spec.Kind == EasternWeaponNamedKind.UnfixedForm
                ? " While polymorphed or changed from natural size, its base damage advances one native weapon-size step."
                : string.Empty;
            return string.Join(", ", properties.ToArray()) + " " +
                spec.Family + "." + effect +
                " It uses the family's single stable weapon type and category.";
        }

        private static void AddEquipmentFact(BlueprintItemWeapon item,
            BlueprintUnitFact fact, EasternWeaponNamedKind kind)
        {
            var grant = ScriptableObject.CreateInstance<
                AddFactToEquipmentWielder>();
            grant.name = "$KMG_" + kind + "_EquippedFact";
            grant.Fact = fact;
            item.ComponentsArray = (item.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Concat(
                    new BlueprintComponent[] { grant }).ToArray();
        }

        private static void BindItemReferences(
            EasternWeaponNamedBlueprintEntry[] entries,
            EasternWeaponNamedBuffSet buffs)
        {
            BlueprintItemWeapon wayfarer = entries.Single(value =>
                value.Spec.Kind == EasternWeaponNamedKind.WayfarersOath).Item;
            BlueprintItemWeapon falling = entries.Single(value =>
                value.Spec.Kind == EasternWeaponNamedKind.FallingPetal).Item;
            BlueprintItemWeapon moonlit = entries.Single(value =>
                value.Spec.Kind == EasternWeaponNamedKind.MoonlitCrossing).Item;
            buffs.WayfarersOath.ComponentsArray.OfType<
                EasternEquipmentStatBonus>().Single().Weapon = wayfarer;
            buffs.FallingPetal.ComponentsArray.OfType<
                EasternEquipmentStatBonus>().Single().Weapon = falling;
            EasternEquipmentStatBonus moonlitArmor = buffs.MoonlitCrossing
                .ComponentsArray.OfType<EasternEquipmentStatBonus>().Single();
            moonlitArmor.Weapon = moonlit;
            buffs.MoonlitCrossing.ComponentsArray.OfType<
                MoonlitCrossingDamageBonus>().Single().Weapon = moonlit;
        }

        private static BlueprintBuff Buff(string internalName, string name,
            string description, bool hidden,
            params BlueprintComponent[] components)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = internalName;
            result.Stacking = StackingType.Replace;
            result.IsClassFeature = false;
            result.ComponentsArray = components ??
                Array.Empty<BlueprintComponent>();
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (flags == null) throw new MissingFieldException(
                typeof(BlueprintBuff).FullName, "m_Flags");
            flags.SetValue(result, Enum.ToObject(flags.FieldType,
                hidden ? 2 : 0));
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(internalName + ".Name", name),
                LocalizationService.Create(internalName + ".Description",
                    description), null);
            return result;
        }

        private static BlueprintFeature Feature(string internalName,
            string name, string description,
            params BlueprintComponent[] components)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = internalName;
            result.IsClassFeature = false;
            result.Ranks = 1;
            result.ComponentsArray = components ??
                Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(internalName + ".Name", name),
                LocalizationService.Create(internalName + ".Description",
                    description), null);
            return result;
        }

        private static void ConfigureEnchantmentText(
            BlueprintWeaponEnchantment enchantment, string symbol,
            string name, string description)
        {
            const BindingFlags fields = BindingFlags.Instance |
                BindingFlags.NonPublic;
            Type owner = typeof(BlueprintItemEnchantment);
            FieldInfo nameField = owner.GetField("m_EnchantName", fields);
            FieldInfo descriptionField = owner.GetField("m_Description", fields);
            FieldInfo costField = owner.GetField("m_EnchantmentCost", fields);
            if (nameField == null || descriptionField == null ||
                costField == null) throw new MissingFieldException(
                    owner.FullName,
                    "m_EnchantName/m_Description/m_EnchantmentCost");
            nameField.SetValue(enchantment, LocalizationService.Create(
                symbol + ".Name", name));
            descriptionField.SetValue(enchantment, LocalizationService.Create(
                symbol + ".Description", description));
            costField.SetValue(enchantment, 0);
        }

        private static string PropertyName(EasternWeaponNativeProperty property)
        {
            return property == EasternWeaponNativeProperty.GhostTouch
                ? "Ghost Touch" :
                property == EasternWeaponNativeProperty.BrilliantEnergy
                ? "Brilliant Energy" : property.ToString();
        }
    }

    internal sealed class EasternWeaponNamedBlueprintEntry
    {
        internal EasternWeaponNamedBlueprintEntry(EasternWeaponNamedSpec spec,
            BlueprintItemWeapon item) { Spec = spec; Item = item; }
        internal EasternWeaponNamedSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class EasternWeaponNamedBlueprintSet
    {
        internal EasternWeaponNamedBlueprintSet(
            EasternWeaponNamedBlueprintEntry[] entries,
            EasternWeaponNamedBuffSet buffs,
            EasternWeaponNamedEnchantmentSet enchantments)
        {
            Entries = entries ?? throw new ArgumentNullException("entries");
            Buffs = buffs ?? throw new ArgumentNullException("buffs");
            Enchantments = enchantments ?? throw new ArgumentNullException(
                "enchantments");
        }
        internal EasternWeaponNamedBlueprintEntry[] Entries { get; private set; }
        internal EasternWeaponNamedBuffSet Buffs { get; private set; }
        internal EasternWeaponNamedEnchantmentSet Enchantments
        { get; private set; }
        internal EasternWeaponNamedBlueprintEntry Require(
            EasternWeaponNamedKind kind)
        { return Entries.Single(value => value.Spec.Kind == kind); }
    }

    internal sealed class EasternWeaponNamedBuffSet
    {
        internal EasternWeaponNamedBuffSet(BlueprintFeature wayfarersOath,
            BlueprintBuff fallingPetal, BlueprintFeature moonlitCrossing,
            BlueprintBuff mountainSunderMarker)
        {
            WayfarersOath = wayfarersOath;
            FallingPetal = fallingPetal;
            MoonlitCrossing = moonlitCrossing;
            MountainSunderMarker = mountainSunderMarker;
        }
        internal BlueprintFeature WayfarersOath { get; private set; }
        internal BlueprintBuff FallingPetal { get; private set; }
        internal BlueprintFeature MoonlitCrossing { get; private set; }
        internal BlueprintBuff MountainSunderMarker { get; private set; }
        internal BlueprintBuff[] All { get { return new[] { FallingPetal,
            MountainSunderMarker }; } }
        internal BlueprintUnitFact ForEquipped(EasternWeaponNamedKind kind)
        {
            return kind == EasternWeaponNamedKind.WayfarersOath
                ? WayfarersOath : kind == EasternWeaponNamedKind.MoonlitCrossing
                ? MoonlitCrossing : null;
        }
    }

    internal sealed class EasternWeaponNamedEnchantmentSet
    {
        internal EasternWeaponNamedEnchantmentSet(
            BlueprintWeaponEnchantment fallingPetal,
            BlueprintWeaponEnchantment mountainSunder,
            BlueprintWeaponEnchantment unfixedForm)
        {
            FallingPetal = fallingPetal;
            MountainSunder = mountainSunder;
            UnfixedForm = unfixedForm;
        }
        internal BlueprintWeaponEnchantment FallingPetal { get; private set; }
        internal BlueprintWeaponEnchantment MountainSunder { get; private set; }
        internal BlueprintWeaponEnchantment UnfixedForm { get; private set; }
        internal BlueprintWeaponEnchantment[] All { get { return new[] {
            FallingPetal, MountainSunder, UnfixedForm }; } }
        internal BlueprintWeaponEnchantment For(EasternWeaponNamedKind kind)
        {
            return kind == EasternWeaponNamedKind.FallingPetal ? FallingPetal :
                kind == EasternWeaponNamedKind.MountainSunder ? MountainSunder :
                kind == EasternWeaponNamedKind.UnfixedForm ? UnfixedForm : null;
        }
    }
}
