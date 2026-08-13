using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ElvenBranchedSpearNamedBlueprints
    {
        internal const string BoughkeeperEnchantmentSymbol =
            "KMG.ElvenBranchedSpear.BoughkeeperEnchantment";
        internal const string ThornstepEnchantmentSymbol =
            "KMG.ElvenBranchedSpear.ThornstepEnchantment";
        internal const string VipersReachEnchantmentSymbol =
            "KMG.ElvenBranchedSpear.VipersReachEnchantment";
        internal const string BriarCrownedEnchantmentSymbol =
            "KMG.ElvenBranchedSpear.BriarCrownedEnchantment";
        internal const string FirstBranchEnchantmentSymbol =
            "KMG.ElvenBranchedSpear.FirstBranchEnchantment";
        internal const string BoughkeeperBuffSymbol =
            "KMG.ElvenBranchedSpear.BoughkeeperArmorClassBuff";
        internal const string ThornstepPenaltySymbol =
            "KMG.ElvenBranchedSpear.ThornstepSpeedPenaltyBuff";
        internal const string ThornstepMarkerSymbol =
            "KMG.ElvenBranchedSpear.ThornstepRoundMarker";
        internal const string VipersReachPenaltySymbol =
            "KMG.ElvenBranchedSpear.VipersReachReflexPenaltyBuff";
        internal const string VipersReachMarkerSymbol =
            "KMG.ElvenBranchedSpear.VipersReachRoundMarker";
        internal const string BriarCrownedMarkerSymbol =
            "KMG.ElvenBranchedSpear.BriarCrownedRoundMarker";
        internal const string FirstBranchMarkerSymbol =
            "KMG.ElvenBranchedSpear.FirstBranchRoundMarker";
        internal const string FirstBranchPenaltySymbol =
            "KMG.ElvenBranchedSpear.FirstBranchSpeedPenaltyBuff";

        internal const string NativeAgileGuid =
            "a36ad92c51789b44fa8a1c5c116a1328";
        internal const string NativeKeenGuid =
            "102a9c8c9b7a75e4fb5844e79deaf4c0";
        internal const string NativeCorrosiveGuid =
            "633b38ff1d11de64a91d490c683ab1c8";
        internal const string NativeSpeedGuid =
            "f1c0c50108025d546b2554674ea1c006";
        internal const string NativeEnhancementTwoGuid =
            "eb2faccc4c9487d43b3575d7e77ff3f5";
        internal const string NativeEnhancementThreeGuid =
            "80bb8a737579e35498177e1e3c75899b";
        internal const string NativeEnhancementFourGuid =
            "783d7d496da6ac44f9511011fc5f1979";
        internal const string NativeEnhancementFiveGuid =
            "bdba267e951851449af552aa9f9e3992";
        internal const string NativeDirtyTrickEntangledBuffGuid =
            "3a6c5d8520c3b404883276590b086702";

        internal static ElvenBranchedSpearNamedBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            BlueprintWeaponType weaponType, ModLogger logger)
        {
            if (library == null || registry == null || weaponType == null ||
                logger == null) throw new ArgumentNullException(
                    "Named spear registration inputs are incomplete.");
            BlueprintItemWeapon donor = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library,
                    ElvenBranchedSpearBlueprints.NativeLongspearItemGuid,
                    "native Standard Longspear item");
            var buffs = RegisterBuffs(registry);
            BlueprintBuff entangled = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeDirtyTrickEntangledBuffGuid,
                    "native Dirty Trick Entangled condition buff");
            var enchantments = RegisterEnchantments(registry, buffs, entangled);
            var native = LoadNativeEnchantments(library);
            var typeAccess = WeaponBlueprintAccess.Resolve();
            var itemAccess = new SpearItemAccess();
            var entries = new List<NamedSpearBlueprintEntry>();
            foreach (NamedSpearSpec spec in ElvenBranchedSpearNamedCatalog.All)
            {
                BlueprintWeaponEnchantment[] itemEnchantments =
                    BuildEnchantments(spec, native, enchantments);
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                    spec.Symbol, delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                            donor, "KMG_ElvenBranchedSpear_" + spec.Kind);
                        typeAccess.Set(clone, weaponType);
                        itemAccess.ConfigureNamed(clone, spec, itemEnchantments,
                            Describe(spec));
                        return clone;
                    });
                itemAccess.ValidateNamed(item, spec, itemEnchantments.Length);
                entries.Add(new NamedSpearBlueprintEntry(spec, item));
            }
            BlueprintItemWeapon boughkeeper = entries.Single(value =>
                value.Spec.Kind == NamedSpearKind.Boughkeeper).Item;
            BoughkeeperArmorClassBonus boughComponent = buffs.Boughkeeper
                .ComponentsArray.OfType<BoughkeeperArmorClassBonus>().Single();
            boughComponent.Boughkeeper = boughkeeper;
            Validate(entries.ToArray(), weaponType, typeAccess, buffs,
                enchantments);
            logger.Info("elven-branched-spear", "named.ready",
                "Registered six save-stable named Elven Branched Spears with native enhancement properties and exact trigger components.");
            return new ElvenBranchedSpearNamedBlueprintSet(entries.ToArray(),
                buffs, enchantments);
        }

        private static NamedSpearBuffSet RegisterBuffs(BlueprintRegistry registry)
        {
            BlueprintBuff boughkeeper = registry.Register<BlueprintBuff>(
                BoughkeeperBuffSymbol, delegate
                {
                    var component = ScriptableObject.CreateInstance<
                        BoughkeeperArmorClassBonus>();
                    component.name = "$KMG_Boughkeeper_AC";
                    return Buff("KMG_Boughkeeper_AC_Buff", "Boughkeeper",
                        "+1 dodge bonus to Armor Class until the beginning of the wielder's next turn.",
                        false, false, component);
                });
            BlueprintBuff thornPenalty = registry.Register<BlueprintBuff>(
                ThornstepPenaltySymbol, () => StatBuff(
                    "KMG_Thornstep_Speed_Penalty", "Thornstep",
                    "Movement speed is reduced by 10 feet for 1 round.",
                    StatType.Speed, -10));
            BlueprintBuff thornMarker = registry.Register<BlueprintBuff>(
                ThornstepMarkerSymbol, () => Marker("KMG_Thornstep_Round_Marker"));
            BlueprintBuff viperPenalty = registry.Register<BlueprintBuff>(
                VipersReachPenaltySymbol, () => StatBuff(
                    "KMG_VipersReach_Reflex_Penalty", "Viper's Reach",
                    "-2 penalty on Reflex saves for 1 round.",
                    StatType.SaveReflex, -2));
            BlueprintBuff viperMarker = registry.Register<BlueprintBuff>(
                VipersReachMarkerSymbol, () => Marker(
                    "KMG_VipersReach_Round_Marker"));
            BlueprintBuff briarMarker = registry.Register<BlueprintBuff>(
                BriarCrownedMarkerSymbol, () => Marker(
                    "KMG_BriarCrowned_Round_Marker"));
            BlueprintBuff firstMarker = registry.Register<BlueprintBuff>(
                FirstBranchMarkerSymbol, () => Marker(
                    "KMG_FirstBranch_Round_Marker"));
            BlueprintBuff firstPenalty = registry.Register<BlueprintBuff>(
                FirstBranchPenaltySymbol, () => StatBuff(
                    "KMG_FirstBranch_Speed_Penalty",
                    "First Branch's Reprisal",
                    "Movement speed is reduced by 10 feet for 1 round.",
                    StatType.Speed, -10));
            return new NamedSpearBuffSet(boughkeeper, thornPenalty, thornMarker,
                viperPenalty, viperMarker, briarMarker, firstMarker,
                firstPenalty);
        }

        private static NamedSpearEnchantmentSet RegisterEnchantments(
            BlueprintRegistry registry, NamedSpearBuffSet buffs,
            BlueprintBuff entangled)
        {
            return new NamedSpearEnchantmentSet(
                RegisterEnchantment(registry, BoughkeeperEnchantmentSymbol,
                    "Boughkeeper's Guard",
                    "A successful attack of opportunity grants +1 dodge AC until the beginning of the wielder's next turn.",
                    NamedSpearKind.Boughkeeper, buffs.Boughkeeper, null, null,
                    null),
                RegisterEnchantment(registry, ThornstepEnchantmentSymbol,
                    "Thornstep",
                    "Once per round, a movement-provoked attack of opportunity hit reduces the target's speed by 10 feet for 1 round.",
                    NamedSpearKind.Thornstep, buffs.ThornPenalty,
                    buffs.ThornMarker, null, null),
                RegisterEnchantment(registry, VipersReachEnchantmentSymbol,
                    "Viper's Reach",
                    "Once per round, genuine sneak attack damage imposes a -2 Reflex penalty for 1 round.",
                    NamedSpearKind.VipersReach, buffs.ViperPenalty,
                    buffs.ViperMarker, null, null),
                RegisterEnchantment(registry, BriarCrownedEnchantmentSymbol,
                    "Briar-Crowned Fortuity",
                    "Once per round after an attack of opportunity hit, expend another available attack of opportunity to attack the same target at -5.",
                    NamedSpearKind.BriarCrownedSpear, null, buffs.BriarMarker,
                    null, null),
                RegisterEnchantment(registry, FirstBranchEnchantmentSymbol,
                    "First Branch's Reprisal",
                    "Once per round after an attack of opportunity hit or genuine sneak attack damage, the target attempts a Fortitude save; failure Entangles it and success reduces speed by 10 feet for 1 round.",
                    NamedSpearKind.SpearOfTheFirstBranch, null,
                    buffs.FirstMarker, buffs.FirstPenalty, entangled));
        }

        private static BlueprintWeaponEnchantment RegisterEnchantment(
            BlueprintRegistry registry, string symbol, string displayName,
            string description, NamedSpearKind kind, BlueprintBuff effect,
            BlueprintBuff marker, BlueprintBuff secondary,
            BlueprintBuff entangled)
        {
            return registry.Register<BlueprintWeaponEnchantment>(symbol, delegate
            {
                var result = ScriptableObject.CreateInstance<
                    BlueprintWeaponEnchantment>();
                result.name = "KMG_" + kind + "_Enchantment";
                ConfigureEnchantmentText(result, symbol, displayName,
                    description);
                var component = ScriptableObject.CreateInstance<
                    NamedSpearEffectComponent>();
                component.name = "$KMG_" + kind + "_Effect";
                component.Kind = kind;
                component.EffectBuff = effect;
                component.RoundMarker = marker;
                component.SecondaryBuff = secondary;
                component.EntangledBuff = entangled;
                result.ComponentsArray = new BlueprintComponent[] { component };
                return result;
            });
        }

        private static Dictionary<string, BlueprintWeaponEnchantment>
            LoadNativeEnchantments(LibraryScriptableObject library)
        {
            string[] guids = {
                ElvenBranchedSpearBlueprints.NativeEnhancementOneGuid,
                NativeEnhancementTwoGuid, NativeEnhancementThreeGuid,
                NativeEnhancementFourGuid, NativeEnhancementFiveGuid,
                NativeAgileGuid, NativeKeenGuid, NativeCorrosiveGuid,
                NativeSpeedGuid };
            return guids.ToDictionary(guid => guid, guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    library, guid, "native named-spear enchantment"));
        }

        private static BlueprintWeaponEnchantment[] BuildEnchantments(
            NamedSpearSpec spec,
            IDictionary<string, BlueprintWeaponEnchantment> native,
            NamedSpearEnchantmentSet custom)
        {
            string enhancement = spec.Enhancement == 1
                ? ElvenBranchedSpearBlueprints.NativeEnhancementOneGuid
                : spec.Enhancement == 2 ? NativeEnhancementTwoGuid
                : spec.Enhancement == 3 ? NativeEnhancementThreeGuid
                : spec.Enhancement == 4 ? NativeEnhancementFourGuid
                : NativeEnhancementFiveGuid;
            var result = new List<BlueprintWeaponEnchantment> {
                native[enhancement] };
            if (spec.Agile) result.Add(native[NativeAgileGuid]);
            if (spec.Keen) result.Add(native[NativeKeenGuid]);
            if (spec.Corrosive) result.Add(native[NativeCorrosiveGuid]);
            if (spec.Speed) result.Add(native[NativeSpeedGuid]);
            BlueprintWeaponEnchantment effect = custom.For(spec.Kind);
            if (effect != null) result.Add(effect);
            return result.ToArray();
        }

        private static BlueprintBuff StatBuff(string internalName,
            string name, string description, StatType stat, int value)
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.name = "$" + internalName + "_Stat";
            bonus.Stat = stat;
            bonus.Value = value;
            bonus.Descriptor = ModifierDescriptor.UntypedStackable;
            return Buff(internalName, name, description, false, true, bonus);
        }

        private static BlueprintBuff Marker(string internalName)
        {
            return Buff(internalName, "Named Spear Round Marker",
                "Internal one-round marker used to enforce a named weapon's once-per-round limit.",
                true, false);
        }

        private static BlueprintBuff Buff(string internalName, string name,
            string description, bool hidden, bool harmful,
            params BlueprintComponent[] components)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = internalName;
            result.Stacking = StackingType.Replace;
            result.IsClassFeature = false;
            result.ComponentsArray = components ?? Array.Empty<BlueprintComponent>();
            SetBuffFlags(result, hidden, harmful);
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(internalName + ".Name", name),
                LocalizationService.Create(internalName + ".Description",
                    description), null);
            return result;
        }

        private static void SetBuffFlags(BlueprintBuff buff, bool hidden,
            bool harmful)
        {
            FieldInfo field = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintBuff).FullName, "m_Flags");
            int flags = (hidden ? 2 : 0) | (harmful ? 64 : 0);
            field.SetValue(buff, Enum.ToObject(field.FieldType, flags));
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
            if (nameField == null || descriptionField == null || costField == null)
                throw new MissingFieldException(owner.FullName,
                    "m_EnchantName/m_Description/m_EnchantmentCost");
            nameField.SetValue(enchantment, LocalizationService.Create(
                symbol + ".Name", name));
            descriptionField.SetValue(enchantment, LocalizationService.Create(
                symbol + ".Description", description));
            costField.SetValue(enchantment, 0);
        }

        private static string Describe(NamedSpearSpec spec)
        {
            string profile = "+" + spec.Enhancement +
                (spec.Agile ? " Agile" : string.Empty) +
                (spec.Keen ? " Keen" : string.Empty) +
                (spec.Corrosive ? " Corrosive" : string.Empty) +
                (spec.Speed ? " Speed" : string.Empty) +
                (spec.ColdIron ? " Cold Iron" : string.Empty);
            string effect = spec.Kind == NamedSpearKind.Boughkeeper
                ? "An attack of opportunity hit grants +1 dodge AC until the beginning of the wielder's next turn."
                : spec.Kind == NamedSpearKind.Thornstep
                ? "Once per round, a movement-provoked attack of opportunity hit reduces the target's speed by 10 feet for 1 round."
                : spec.Kind == NamedSpearKind.VipersReach
                ? "Once per round, genuine sneak attack damage imposes a -2 Reflex penalty for 1 round."
                : spec.Kind == NamedSpearKind.BriarCrownedSpear
                ? "Once per round after an attack of opportunity hit, the wielder may expend another attack of opportunity to attack that target at -5."
                : spec.Kind == NamedSpearKind.SpearOfTheFirstBranch
                ? "First Branch's Reprisal forces a once-per-round Fortitude save after an attack of opportunity hit or genuine sneak attack damage; failure Entangles for 1 round and success reduces speed by 10 feet for 1 round."
                : "Its native Agile and cold iron properties work normally.";
            return profile + " Elven Branched Spear. " + effect +
                " It remains a two-handed reach weapon usable with Weapon Finesse and grants +2 on attacks of opportunity provoked by movement.";
        }

        private static void Validate(NamedSpearBlueprintEntry[] entries,
            BlueprintWeaponType weaponType, WeaponBlueprintAccess typeAccess,
            NamedSpearBuffSet buffs, NamedSpearEnchantmentSet enchantments)
        {
            if (entries == null || entries.Length != 6 ||
                entries.Select(value => value.Item).Distinct().Count() != 6 ||
                entries.Any(value => !ReferenceEquals(
                    typeAccess.Get(value.Item), weaponType)) ||
                buffs.All.Length != 8 || enchantments.All.Length != 5 ||
                buffs.All.Any(value => value == null) ||
                enchantments.All.Any(value => value == null ||
                    value.ComponentsArray.OfType<NamedSpearEffectComponent>()
                        .Count() != 1))
                throw new InvalidOperationException(
                    "Named Elven Branched Spear registration is malformed.");
        }
    }

    internal sealed class NamedSpearBlueprintEntry
    {
        internal NamedSpearBlueprintEntry(NamedSpearSpec spec,
            BlueprintItemWeapon item) { Spec = spec; Item = item; }
        internal NamedSpearSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class NamedSpearBuffSet
    {
        internal NamedSpearBuffSet(BlueprintBuff boughkeeper,
            BlueprintBuff thornPenalty, BlueprintBuff thornMarker,
            BlueprintBuff viperPenalty, BlueprintBuff viperMarker,
            BlueprintBuff briarMarker, BlueprintBuff firstMarker,
            BlueprintBuff firstPenalty)
        {
            Boughkeeper = boughkeeper; ThornPenalty = thornPenalty;
            ThornMarker = thornMarker; ViperPenalty = viperPenalty;
            ViperMarker = viperMarker; BriarMarker = briarMarker;
            FirstMarker = firstMarker; FirstPenalty = firstPenalty;
        }
        internal BlueprintBuff Boughkeeper { get; private set; }
        internal BlueprintBuff ThornPenalty { get; private set; }
        internal BlueprintBuff ThornMarker { get; private set; }
        internal BlueprintBuff ViperPenalty { get; private set; }
        internal BlueprintBuff ViperMarker { get; private set; }
        internal BlueprintBuff BriarMarker { get; private set; }
        internal BlueprintBuff FirstMarker { get; private set; }
        internal BlueprintBuff FirstPenalty { get; private set; }
        internal BlueprintBuff[] All { get { return new[] { Boughkeeper,
            ThornPenalty, ThornMarker, ViperPenalty, ViperMarker, BriarMarker,
            FirstMarker, FirstPenalty }; } }
    }

    internal sealed class NamedSpearEnchantmentSet
    {
        internal NamedSpearEnchantmentSet(BlueprintWeaponEnchantment boughkeeper,
            BlueprintWeaponEnchantment thornstep,
            BlueprintWeaponEnchantment vipersReach,
            BlueprintWeaponEnchantment briarCrowned,
            BlueprintWeaponEnchantment firstBranch)
        {
            Boughkeeper = boughkeeper; Thornstep = thornstep;
            VipersReach = vipersReach; BriarCrowned = briarCrowned;
            FirstBranch = firstBranch;
        }
        internal BlueprintWeaponEnchantment Boughkeeper { get; private set; }
        internal BlueprintWeaponEnchantment Thornstep { get; private set; }
        internal BlueprintWeaponEnchantment VipersReach { get; private set; }
        internal BlueprintWeaponEnchantment BriarCrowned { get; private set; }
        internal BlueprintWeaponEnchantment FirstBranch { get; private set; }
        internal BlueprintWeaponEnchantment[] All { get { return new[] {
            Boughkeeper, Thornstep, VipersReach, BriarCrowned, FirstBranch }; } }
        internal BlueprintWeaponEnchantment For(NamedSpearKind kind)
        {
            return kind == NamedSpearKind.Boughkeeper ? Boughkeeper :
                kind == NamedSpearKind.Thornstep ? Thornstep :
                kind == NamedSpearKind.VipersReach ? VipersReach :
                kind == NamedSpearKind.BriarCrownedSpear ? BriarCrowned :
                kind == NamedSpearKind.SpearOfTheFirstBranch ? FirstBranch : null;
        }
    }

    internal sealed class ElvenBranchedSpearNamedBlueprintSet
    {
        internal ElvenBranchedSpearNamedBlueprintSet(
            NamedSpearBlueprintEntry[] entries, NamedSpearBuffSet buffs,
            NamedSpearEnchantmentSet enchantments)
        { Entries = entries; Buffs = buffs; Enchantments = enchantments; }
        internal NamedSpearBlueprintEntry[] Entries { get; private set; }
        internal NamedSpearBuffSet Buffs { get; private set; }
        internal NamedSpearEnchantmentSet Enchantments { get; private set; }
        internal NamedSpearBlueprintEntry Require(NamedSpearKind kind)
        { return Entries.Single(value => value.Spec.Kind == kind); }
    }
}
