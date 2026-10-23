using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Astral31Prefix = "KMG.Summoning.OutsiderCelestial.AstralDeva.";
        internal const string Astral31Unit = "KMG.Summoning.Unit.AstralDeva";

        private static void ConfigureAstralDeva31(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            AstralDevaRulesPolicy.Validate();
            var unit = Require<BlueprintUnit>(bySymbol, Astral31Unit);
            var type = Require<BlueprintUnitType>(bySymbol, Astral31Prefix + "UnitType");
            type.name = InternalName(Astral31Prefix + "UnitType");
            type.Name = LocalizationService.Create(Astral31Prefix + "UnitType.Name", "Astral Deva");
            type.Image = null;
            var traits = Require<BlueprintFeature>(bySymbol, Astral31Prefix + "Traits");
            traits.name = InternalName(Astral31Prefix + "Traits");
            traits.HideInUI = true; traits.IsClassFeature = false;

            var hammer = Sprint29Weapon(library, bySymbol, Astral31Prefix + "Warhammer1d8",
                Astral31Prefix + "WarhammerType", "3f35d5c01e11d564daa59938dec3db4b",
                Size.Medium, 1, DiceType.D8, 9);
            SetField(hammer.Type, "m_IsTwoHanded", true);
            SetField(hammer, "m_Enchantments", new[] {
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    MagicFirearmBlueprints.Enhancement2Guid, "Astral native +2 enhancement"),
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    "0f20d79b7049c0f4ca54ca3d1ea44baa", "native disruption enchantment") });
            var slam = Sprint29Weapon(library, bySymbol, Astral31Prefix + "Slam1d8",
                Astral31Prefix + "SlamType", NativeSlam, Size.Medium, 1, DiceType.D8, 9);
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = hammer, PrimaryHandAlternative1 = slam,
                EmptyHandWeapon = null,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };

            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, OutsiderClassGuid, "Astral outsider racial HD");
            levels.Levels = AstralDevaRulesPolicy.HitDice; levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown; levels.Skills = Array.Empty<StatType>();
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = AstralDevaRulesPolicy.BaseRacialHitPoints;
            racial.BaseFortitude = AstralDevaRulesPolicy.BaseFortitude;
            racial.BaseReflex = AstralDevaRulesPolicy.BaseReflex;
            racial.BaseWill = AstralDevaRulesPolicy.BaseWill;
            racial.PerceptionRanks = 15; racial.StealthRanks = 15;
            // The native outsider class supplies the trained +3 for both
            // ranked class skills; adding another carrier produces +29/+25.
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;

            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(10); dr.BypassedByAlignment = true; dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison;
            poison.ModifierDescriptor = ModifierDescriptor.Racial; poison.Value = 4; poison.Bonus = Simple(0);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>(); sr.Value = Simple(AstralDevaRulesPolicy.SpellResistance); sr.AddCR = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC; armor.Value = AstralDevaRulesPolicy.NaturalArmor; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var alertness = ScriptableObject.CreateInstance<AddStatBonus>();
            alertness.Stat = StatType.SkillPerception; alertness.Value = 2;
            alertness.Descriptor = ModifierDescriptor.UntypedStackable;
            var racialPerception = ScriptableObject.CreateInstance<AddStatBonus>();
            racialPerception.Stat = StatType.SkillPerception; racialPerception.Value = 2;
            racialPerception.Descriptor = ModifierDescriptor.Racial;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = hammer; equipment.Slam = slam;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { hammer.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var focus = ScriptableObject.CreateInstance<WeaponFocus>();
            focus.WeaponType = hammer.Type; focus.AttackBonus = 1;
            var stunned = Require<BlueprintBuff>(bySymbol, Astral31Prefix + "Stunned");
            var stunFeature = Require<BlueprintFeature>(bySymbol, Astral31Prefix + "StunFeature");
            ConfigureAstralHammerStun(library, unit, hammer, stunned, stunFeature);
            type.SignatureAbilities = new BlueprintUnitFact[] { stunFeature };

            ConfigureAstralProtectiveAura(bySymbol, unit);
            var heal = Require<BlueprintAbility>(bySymbol, Astral31Prefix + "Heal");
            var healResource = Require<BlueprintAbilityResource>(bySymbol, Astral31Prefix + "HealResource");
            ConfigureNamedResource(healResource, Astral31Prefix + "HealResource",
                Astral31Prefix + "HealResource", "Astral Deva Heal",
                "One Heal use for this Astral Deva summoning.", 1);
            Sprint29LimitedNativeSpell(library, heal, Astral31Prefix + "Heal",
                "ff8f1534f66559c478448723e16b6624", AstralDevaRulesPolicy.SpellLikeCasterLevel, 6, healResource, "Heal");
            var bladeBarrier = Require<BlueprintAbility>(bySymbol,
                Astral31Prefix + "BladeBarrier");
            var bladeBarrierResource = Require<BlueprintAbilityResource>(bySymbol,
                Astral31Prefix + "BladeBarrierResource");
            ConfigureNamedResource(bladeBarrierResource,
                Astral31Prefix + "BladeBarrierResource",
                Astral31Prefix + "BladeBarrierResource", "Astral Deva Blade Barrier",
                "One Blade Barrier use for this Astral Deva summoning.",
                AstralDevaRulesPolicy.BladeBarrierUses);
            Sprint29LimitedNativeSpell(library, bladeBarrier,
                Astral31Prefix + "BladeBarrier",
                "36c8971e91f1745418cc3ffdfac17b74",
                AstralDevaRulesPolicy.SpellLikeCasterLevel,
                AstralDevaRulesPolicy.BladeBarrierSpellLevel,
                bladeBarrierResource, "Blade Barrier");
            var cureLight = Require<BlueprintAbility>(bySymbol,
                Astral31Prefix + "CureLightWounds");
            var cureLightResource = Require<BlueprintAbilityResource>(bySymbol,
                Astral31Prefix + "CureLightWoundsResource");
            ConfigureNamedResource(cureLightResource,
                Astral31Prefix + "CureLightWoundsResource",
                Astral31Prefix + "CureLightWoundsResource",
                "Astral Deva Cure Light Wounds",
                "Seven Cure Light Wounds uses for this Astral Deva summoning.",
                AstralDevaRulesPolicy.CureLightWoundsUses);
            Sprint29LimitedNativeSpell(library, cureLight,
                Astral31Prefix + "CureLightWounds",
                "5590652e1c2225c4ca30c4a699ab3649",
                AstralDevaRulesPolicy.SpellLikeCasterLevel,
                AstralDevaRulesPolicy.CureLightWoundsSpellLevel,
                cureLightResource, "Cure Light Wounds",
                "up to seven times per summoning");
            var seeInvisibility = Require<BlueprintAbility>(bySymbol,
                Astral31Prefix + "SeeInvisibility");
            var seeInvisibilityResource = Require<BlueprintAbilityResource>(bySymbol,
                Astral31Prefix + "SeeInvisibilityResource");
            ConfigureNamedResource(seeInvisibilityResource,
                Astral31Prefix + "SeeInvisibilityResource",
                Astral31Prefix + "SeeInvisibilityResource",
                "Astral Deva See Invisibility",
                "Seven See Invisibility uses for this Astral Deva summoning.",
                AstralDevaRulesPolicy.SeeInvisibilityUses);
            Sprint29LimitedNativeSpell(library, seeInvisibility,
                Astral31Prefix + "SeeInvisibility",
                "30e5dc243f937fc4b95d2f8f4e1b7ff3",
                AstralDevaRulesPolicy.SpellLikeCasterLevel,
                AstralDevaRulesPolicy.SeeInvisibilitySpellLevel,
                seeInvisibilityResource, "See Invisibility",
                "up to seven times per summoning");
            var aid = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "Aid", "03a9630394d10164a9410882d31572f0", "Aid");
            var dispelMagic = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "DispelMagic", "92681f181b507b34ea87018e8f7a528a", "Dispel Magic");
            var holyAura = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "HolyAura", "808ab74c12df8784ab4eeaf6a107dbea", "Holy Aura");
            var holyWord = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "HolyWord", "4737294a66c91b844842caee8cf505c8", "Holy Word");
            var invisibility = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "Invisibility", "89940cde01689fb46946b2f8cd7b66b7", "Invisibility");
            // The printed Astral SLA is self-only. Keep the native effect graph
            // while removing the donor spell's ordinary ally-touch surface.
            invisibility.Range = AbilityRange.Personal;
            invisibility.CanTargetSelf = true;
            invisibility.CanTargetFriends = false;
            invisibility.CanTargetEnemies = false;
            invisibility.CanTargetPoint = false;
            var removeCurse = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "RemoveCurse", "b48674cef2bff5e478a007cf57d8345b", "Remove Curse");
            var removeDisease = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "RemoveDisease", "4093d5a0eb5cae94e909eb1e0e1a6b36", "Remove Disease");
            var removeFear = ConfigureAstralAtWillNativeSpell(library, bySymbol,
                "RemoveFear", "55a037e514c0ee14a8e3ed14b47061de", "Remove Fear");
            var holySmite = ConfigureAstralHolySmite(library, bySymbol, unit,
                holyWord.Icon);
            var dispelEvil = ConfigureAstralDispelEvil(library, bySymbol, unit);
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { heal, bladeBarrier, cureLight,
                seeInvisibility, aid, dispelMagic, holyAura, holyWord,
                invisibility, removeCurse, removeDisease, removeFear, holySmite }
                .Concat(dispelEvil).ToArray();
            parameters.SpellLevels = new[] { 6,
                AstralDevaRulesPolicy.BladeBarrierSpellLevel,
                AstralDevaRulesPolicy.CureLightWoundsSpellLevel,
                AstralDevaRulesPolicy.SeeInvisibilitySpellLevel,
                AstralDevaRulesPolicy.AidSpellLevel,
                AstralDevaRulesPolicy.DispelMagicSpellLevel,
                AstralDevaRulesPolicy.HolyAuraSpellLevel,
                AstralDevaRulesPolicy.HolyWordSpellLevel,
                AstralDevaRulesPolicy.InvisibilitySpellLevel,
                AstralDevaRulesPolicy.RemoveCurseSpellLevel,
                AstralDevaRulesPolicy.RemoveDiseaseSpellLevel,
                AstralDevaRulesPolicy.RemoveFearSpellLevel,
                AstralDevaRulesPolicy.HolySmiteSpellLevel }
                .Concat(Enumerable.Repeat(AstralDevaRulesPolicy.DispelEvilSpellLevel,
                    dispelEvil.Length)).ToArray();
            parameters.CasterLevel = AstralDevaRulesPolicy.SpellLikeCasterLevel;

            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, sr, armor,
                alertness, racialPerception,
                equipment, proficiency, focus, parameters,
                AddResource(healResource), AddResource(bladeBarrierResource),
                AddResource(cureLightResource), AddResource(seeInvisibilityResource),
                Energy(DamageEnergyType.Electricity, 10), Energy(DamageEnergyType.Fire, 10) }
                .Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Astral31Prefix + "Traits.Name", "Astral Deva Traits"),
                LocalizationService.Create(Astral31Prefix + "Traits.Description",
                    "Angel: DR10/evil; acid, cold and petrification immunity; electricity and fire resistance10; +4 racial saves vs poison; SR25 and Uncanny Dodge. A distinct20-foot protective aura grants +4 deflection AC and +4 resistance saves against evil sources, the shared Magic Circle against Evil, and lesser-globe protection from level0-3 spells and SLAs cast from outside. Heal and Blade Barrier are each once per summoning; Cure Light Wounds and See Invisibility each have seven uses. Aid, Dispel Magic, Dispel Evil, Holy Aura, Holy Word, Holy Smite, self-only Invisibility, Remove Curse, Remove Disease and Remove Fear are at will. All SLAs are CL13. Change shape, caster travel, expensive Continual Flame, social Discern Lies and passive Detect Evil sensing are unavailable."), null);

            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "Astral ordinary attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Astral31Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Astral31Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var healAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Astral31Prefix + "HealAi"), Astral31Prefix + "HealAi", heal, 0);
            var healEligibility = Require<AstralHealingEligibility>(bySymbol,
                Astral31Prefix + "HealEligibility");
            healEligibility.name = InternalName(Astral31Prefix + "HealEligibility");
            healEligibility.OwningBlueprint = unit; healEligibility.Heal = heal;
            healEligibility.MinimumDamage = 50;
            healAi.TargetConsiderations = new Consideration[] { healEligibility };
            healAi.BaseScore = 6;
            var cureAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Astral31Prefix + "CureLightWoundsAi"),
                Astral31Prefix + "CureLightWoundsAi", cureLight, 0);
            var cureEligibility = Require<AstralHealingEligibility>(bySymbol,
                Astral31Prefix + "CureLightWoundsEligibility");
            cureEligibility.name = InternalName(Astral31Prefix +
                "CureLightWoundsEligibility");
            cureEligibility.OwningBlueprint = unit;
            cureEligibility.Heal = cureLight; cureEligibility.MinimumDamage = 5;
            cureAi.TargetConsiderations = new Consideration[] { cureEligibility };
            cureAi.BaseScore = 3;
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { healAi, cureAi }).ToArray();
            unit.Brain = brain;

            ConfigureUnitCore(unit, "AstralDeva", "Astral Deva", Alignment.NeutralGood,
                Size.Medium, AstralDevaRulesPolicy.Strength, AstralDevaRulesPolicy.Dexterity,
                AstralDevaRulesPolicy.Constitution, AstralDevaRulesPolicy.Intelligence,
                AstralDevaRulesPolicy.Wisdom, AstralDevaRulesPolicy.Charisma, 50);
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"),
                Feature(library, AirborneGuid, "ordinary-map angel aerial convention"),
                Feature(library, AcidImmunityGuid, "angel acid immunity"),
                Feature(library, ColdImmunityGuid, "angel cold immunity"),
                Feature(library, "3c08d842e802c3e4eb19d15496145709", "native Uncanny Dodge"),
                Feature(library, NativeCleaveFeature, "ordinary native Cleave"),
                Feature(library, "79042cb55f030614ea29956177977c52", "Great Fortitude"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, IronWillGuid, "Iron Will"),
                Feature(library, PowerAttackGuid, "Power Attack"),
                Feature(library, "d09b20029e9abfe4480b356c92095623", "Toughness"),
                extraplanar, traits, stunFeature, heal, bladeBarrier, cureLight,
                seeInvisibility, aid, dispelMagic, holyAura, holyWord,
                invisibility, removeCurse, removeDisease, removeFear,
                holySmite, dispelEvil[0] };
        }

        private static BlueprintAbility ConfigureAstralAtWillNativeSpell(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string suffix, string nativeGuid, string title)
        {
            string symbol = Astral31Prefix + suffix;
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, ability, nativeGuid,
                "native Astral Deva " + title);
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    title + " at will at caster level13."), ability.Icon);
            return ability;
        }

        private static BlueprintAbility ConfigureAstralHolySmite(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintUnit unit, Sprite icon)
        {
            string symbol = Astral31Prefix + "HolySmite";
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            var blinded = Require<BlueprintBuff>(bySymbol,
                Astral31Prefix + "HolySmiteBlinded");
            blinded.name = InternalName(Astral31Prefix + "HolySmiteBlinded");
            blinded.Stacking = StackingType.Stack;
            blinded.IsClassFeature = false;
            var blindness = ScriptableObject.CreateInstance<AddCondition>();
            blindness.Condition = UnitCondition.Blindness;
            var blindDescriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            blindDescriptor.Descriptor = SpellDescriptor.Blindness;
            blinded.ComponentsArray = new BlueprintComponent[] {
                blindness, blindDescriptor };

            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius", new Feet(
                AstralDevaRulesPolicy.HolySmiteRadiusFeet));
            SetField(around, "m_TargetType",
                Kingmaker.UnitLogic.Abilities.Components.TargetType.Any);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And, Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            var resolve = ScriptableObject.CreateInstance<ContextActionAstralHolySmite>();
            resolve.SourceUnit = unit;
            resolve.OutsiderType = Feature(library, OutsiderTypeGuid,
                "native outsider type");
            resolve.Blinded = blinded;
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList { Actions = new GameAction[] { resolve } };
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Good;

            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.SpellLike;
            ability.Range = AbilityRange.Medium;
            ability.CanTargetPoint = true;
            ability.CanTargetSelf = false;
            ability.CanTargetFriends = true;
            ability.CanTargetEnemies = true;
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] {
                around, run, descriptor };
            // Reuse Holy Word's installed Good-spell art. It is an actual
            // holy offensive spell icon and introduces no new bitmap asset.
            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(symbol + ".Name", "Holy Smite");
            access.Configure(blinded, title,
                LocalizationService.Create(Astral31Prefix +
                    "HolySmiteBlinded.Description",
                    "Blinded for one round by this Astral Deva's Holy Smite."), icon);
            access.Configure(ability, title,
                LocalizationService.Create(symbol + ".Description",
                    "At will at caster level13, burst holy power in a20-foot radius at medium range. Good creatures are unaffected. Evil creatures take5d8 divine damage and are blinded for one round; evil outsiders instead take10d6. A Will save halves damage and prevents blindness. Neutral creatures take half damage, or one-quarter on a successful save, and are never blinded. Spell resistance applies."), icon);
            return ability;
        }


        private static BlueprintAbility[] ConfigureAstralDispelEvil(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintUnit unit)
        {
            return ConfigureDispelEvil31(library, bySymbol, unit,
                Astral31Prefix, AbilityType.SpellLike,
                "At will at caster level13, gain +4 deflection AC against attacks by evil creatures for 13 rounds and hold one selected melee touch. The touch either invokes Dismissal against an evil extraplanar creature (Will negates; spell resistance applies) or automatically removes one dispellable evil spell or evil-caster enchantment from a creature. A delivered touch discharges the ward.");
        }

        // The two actual users share the same native ward/delivery graph.
        // Their access, parameters and resources remain owned by the caller.
        private static BlueprintAbility[] ConfigureDispelEvil31(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintUnit unit, string prefix, AbilityType accessType,
            string description)
        {
            const string nativeGuid = "95f7cdcec94e293489a85afdf5af1fd7";
            const string nativeTouchGuid = "17451c1327c571641a1345bd31155209";
            var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, nativeGuid, "native Dismissal effect graph");
            AbilityEffectRunAction[] nativeRuns = native.GetComponents<
                AbilityEffectRunAction>().ToArray();
            var nativeTouchDonor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, nativeTouchGuid,
                    "qualified native held-touch delivery weapon");
            var nativeTouch = nativeTouchDonor.GetComponents<
                AbilityDeliverTouch>().SingleOrDefault();
            if (nativeRuns.Length == 0 || nativeTouch?.TouchWeapon == null)
                throw new InvalidOperationException(
                    "Native Dismissal has no direct effect graph, or the qualified " +
                    "held-touch donor has no delivery weapon. dismissal=" +
                    string.Join(",", (native.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().FullName)) + ";touch=" +
                    string.Join(",", (nativeTouchDonor.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().FullName)));

            string familySymbol = prefix + "DispelEvil";
            string dismissalSymbol = prefix + "DispelEvilDismissal";
            string dismissalTouchSymbol = dismissalSymbol + "Touch";
            string dispelSymbol = prefix + "DispelEvilDispel";
            string dispelTouchSymbol = dispelSymbol + "Touch";
            string wardSymbol = prefix + "DispelEvilWard";
            var family = Require<BlueprintAbility>(bySymbol, familySymbol);
            var dismissal = Require<BlueprintAbility>(bySymbol, dismissalSymbol);
            var dismissalTouch = Require<BlueprintAbility>(bySymbol,
                dismissalTouchSymbol);
            var dispel = Require<BlueprintAbility>(bySymbol, dispelSymbol);
            var dispelTouch = Require<BlueprintAbility>(bySymbol,
                dispelTouchSymbol);
            var ward = Require<BlueprintBuff>(bySymbol, wardSymbol);

            var ac = ScriptableObject.CreateInstance<
                ArmorClassBonusAgainstAlignment>();
            ac.alignment = AlignmentComponent.Evil;
            ac.Value = AstralDevaRulesPolicy.DispelEvilArmorClassBonus;
            ac.Descriptor = ModifierDescriptor.Deflection;
            var lifetime = ScriptableObject.CreateInstance<
                AstralDispelEvilWardLifetime>();
            lifetime.Deliveries = new[] { dismissalTouch, dispelTouch };
            var wardDescriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            wardDescriptor.Descriptor = SpellDescriptor.Good;
            ward.name = InternalName(wardSymbol);
            ward.Stacking = StackingType.Replace;
            ward.IsClassFeature = false;
            ward.ComponentsArray = new BlueprintComponent[] {
                ac, lifetime, wardDescriptor };

            CopyFields(nativeTouchDonor, dismissalTouch);
            dismissalTouch.name = InternalName(dismissalTouchSymbol);
            dismissalTouch.Parent = dismissal;
            dismissalTouch.Type = accessType;
            dismissalTouch.Range = AbilityRange.Touch;
            dismissalTouch.CanTargetPoint = false;
            dismissalTouch.CanTargetSelf = false;
            dismissalTouch.CanTargetFriends = true;
            dismissalTouch.CanTargetEnemies = true;
            dismissalTouch.SpellResistance = true;
            dismissalTouch.NeedEquipWeapons = false;
            dismissalTouch.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            dismissalTouch.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            dismissalTouch.Hidden = true;
            dismissalTouch.ActionBarAutoFillIgnored = true;
            dismissalTouch.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            dismissalTouch.ResourceAssetIds = Array.Empty<string>();
            var dismissalChecker = ScriptableObject.CreateInstance<
                AstralDispelEvilDismissalTargetChecker>();
            dismissalChecker.Extraplanar = Feature(library,
                ExtraplanarSubtypeGuid, "native extraplanar subtype");
            var dismissalMarker = ScriptableObject.CreateInstance<
                AstralDispelEvilDeliveryMarker>();
            dismissalMarker.Ward = ward;
            var dismissalDelivery = (AbilityDeliverTouch)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(nativeTouch);
            var dismissalGood = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            dismissalGood.Descriptor = SpellDescriptor.Good;
            dismissalTouch.ComponentsArray = new BlueprintComponent[] {
                    dismissalDelivery }
                .Concat(nativeRuns.Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent))
                .Concat((native.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<SpellComponent>()
                    .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent))
                .Concat(new BlueprintComponent[] { dismissalGood })
                .Concat((native.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AbilitySpawnFx>()
                    .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent))
                .Concat(new BlueprintComponent[] { dismissalChecker,
                    dismissalMarker }).ToArray();

            CopyFields(nativeTouchDonor, dispelTouch);
            dispelTouch.name = InternalName(dispelTouchSymbol);
            dispelTouch.Parent = dispel;
            dispelTouch.Type = accessType;
            dispelTouch.Range = AbilityRange.Touch;
            dispelTouch.CanTargetPoint = false;
            dispelTouch.CanTargetSelf = true;
            dispelTouch.CanTargetFriends = true;
            dispelTouch.CanTargetEnemies = true;
            dispelTouch.SpellResistance = false;
            dispelTouch.NeedEquipWeapons = false;
            dispelTouch.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            dispelTouch.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            dispelTouch.Hidden = true;
            dispelTouch.ActionBarAutoFillIgnored = true;
            dispelTouch.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            dispelTouch.ResourceAssetIds = Array.Empty<string>();
            var touch = (AbilityDeliverTouch)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeTouch);
            var target = ScriptableObject.CreateInstance<
                AstralDispelEvilTargetChecker>();
            var action = ScriptableObject.CreateInstance<
                ContextActionAstralDispelEvil>();
            action.SourceUnit = unit;
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList { Actions = new GameAction[] { action } };
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.School = SpellSchool.Abjuration;
            var good = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            good.Descriptor = SpellDescriptor.Good;
            var dispelMarker = ScriptableObject.CreateInstance<
                AstralDispelEvilDeliveryMarker>();
            dispelMarker.Ward = ward;
            dispelTouch.ComponentsArray = new BlueprintComponent[] {
                touch, target, run, school, good, dispelMarker }
                .Concat((nativeTouchDonor.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AbilitySpawnFx>()
                    .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent))
                .ToArray();

            ConfigureDispelEvil31Parent(native, dismissal,
                dismissalSymbol, family, dismissalTouch, ward, unit, accessType,
                "Dismiss Evil Extraplanar Creature",
                "Arm Dispel Evil's ward and its Dismissal touch. A successful melee touch against an evil extraplanar creature invokes the installed Dismissal effect; Will negates and spell resistance applies.",
                "Will negates");
            ConfigureDispelEvil31Parent(native, dispel, dispelSymbol,
                family, dispelTouch, ward, unit, accessType, "Dispel Evil Spell",
                "Arm Dispel Evil's ward and a touch that automatically removes one dispellable evil spell, or one enchantment cast by an evil creature, from a creature.",
                "None");

            CopyFields(native, family);
            family.name = InternalName(familySymbol);
            family.Parent = null;
            family.Type = accessType;
            family.Range = AbilityRange.Personal;
            family.CanTargetPoint = false;
            family.CanTargetSelf = true;
            family.CanTargetFriends = false;
            family.CanTargetEnemies = false;
            family.SpellResistance = false;
            family.NeedEquipWeapons = false;
            family.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            family.EffectOnEnemy = AbilityEffectOnUnit.None;
            family.ActionType = UnitCommand.CommandType.Standard;
            family.Hidden = false;
            family.ActionBarAutoFillIgnored = false;
            family.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            family.ResourceAssetIds = Array.Empty<string>();
            var variants = ScriptableObject.CreateInstance<AbilityVariants>();
            variants.Variants = new[] { dismissal, dispel };
            var familySchool = ScriptableObject.CreateInstance<SpellComponent>();
            familySchool.School = SpellSchool.Abjuration;
            var familyGood = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            familyGood.Descriptor = SpellDescriptor.Good;
            family.ComponentsArray = new BlueprintComponent[] {
                variants, familySchool, familyGood };
            family.LocalizedDuration = LocalizationService.Create(
                familySymbol + ".Duration", "1 round per caster level or until discharged");
            family.LocalizedSavingThrow = LocalizationService.Create(
                familySymbol + ".SavingThrow", "See selected discharge");

            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(familySymbol + ".Name",
                "Dispel Evil");
            access.Configure(ward, title,
                LocalizationService.Create(wardSymbol + ".Description",
                    "+4 deflection AC against attacks by evil creatures while the caster holds one Dispel Evil touch."),
                native.Icon);
            access.Configure(dismissalTouch,
                LocalizationService.Create(dismissalTouchSymbol + ".Name",
                    "Dispel Evil — Dismissal Touch"),
                LocalizationService.Create(dismissalTouchSymbol + ".Description",
                    "Deliver the held Dispel Evil dismissal as a melee touch."),
                native.Icon);
            access.Configure(dispelTouch,
                LocalizationService.Create(dispelTouchSymbol + ".Name",
                    "Dispel Evil — Dispel Touch"),
                LocalizationService.Create(dispelTouchSymbol + ".Description",
                    "Deliver the held automatic Dispel Evil removal as a melee touch."),
                native.Icon);
            access.Configure(family, title,
                LocalizationService.Create(familySymbol + ".Description",
                    description),
                native.Icon);
            return new[] { family, dismissal, dismissalTouch, dispel,
                dispelTouch };
        }

        private static void ConfigureDispelEvil31Parent(
            BlueprintAbility native, BlueprintAbility ability, string symbol,
            BlueprintAbility family, BlueprintAbility delivery,
            BlueprintBuff ward, BlueprintUnit unit, AbilityType accessType, string title,
            string description, string savingThrow)
        {
            CopyFields(native, ability);
            ability.name = InternalName(symbol);
            ability.Parent = family;
            ability.Type = accessType;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetPoint = false;
            ability.CanTargetSelf = true;
            ability.CanTargetFriends = false;
            ability.CanTargetEnemies = false;
            ability.SpellResistance = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = true;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.School = SpellSchool.Abjuration;
            var good = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            good.Descriptor = SpellDescriptor.Good;
            var arm = ScriptableObject.CreateInstance<
                AbilityEffectArmAstralDispelEvilTouch>();
            arm.SourceUnit = unit;
            arm.TouchDeliveryAbility = delivery;
            arm.Ward = ward;
            ability.ComponentsArray = new BlueprintComponent[] { school, good,
                arm }.Concat((native.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AbilitySpawnFx>()
                    .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent))
                .ToArray();
            ability.LocalizedDuration = LocalizationService.Create(
                symbol + ".Duration", "1 round per caster level or until discharged");
            ability.LocalizedSavingThrow = LocalizationService.Create(
                symbol + ".SavingThrow", savingThrow);
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description", description),
                native.Icon);
        }

        private static void ConfigureAstralHammerStun(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintItemWeapon hammer, BlueprintBuff stunned,
            BlueprintFeature feature)
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "09d39b38bb7c6014394b6daced9bacd3", "native Stunned condition");
            var condition = native.GetComponents<AddCondition>()
                .Single(value => value.Condition == UnitCondition.Stunned);
            CopyFields(native, stunned);
            stunned.name = InternalName(Astral31Prefix + "Stunned");
            stunned.IsClassFeature = false;
            // Each failed save owns one finite victim effect. Another source's
            // stun is neither replaced nor extended by this one.
            stunned.Stacking = StackingType.Stack;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Stun;
            stunned.ComponentsArray = new BlueprintComponent[] {
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(condition), descriptor };

            var handler = ScriptableObject.CreateInstance<AstralDevaHammerStun>();
            handler.OwningBlueprint = unit; handler.Warhammer = hammer;
            handler.Stunned = stunned;
            feature.name = InternalName(Astral31Prefix + "StunFeature");
            feature.HideInUI = false; feature.IsClassFeature = false;
            feature.ComponentsArray = new BlueprintComponent[] { handler };
            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(Astral31Prefix + "StunFeature.Name",
                "Stun");
            access.Configure(stunned, title,
                LocalizationService.Create(Astral31Prefix + "Stunned.Description",
                    "Stunned by two warhammer hits from one Astral Deva in the same round."),
                native.Icon);
            access.Configure(feature, title,
                LocalizationService.Create(Astral31Prefix + "StunFeature.Description",
                    "When this Astral Deva hits one opponent twice with its warhammer in one round, that opponent must succeed at a Fortitude save (DC 10 + half the deva's racial Hit Dice + its live Strength modifier) or be stunned for 1d6 rounds. Each opponent can trigger this save only once per round."),
                native.Icon);
        }

        private static void ConfigureAstralProtectiveAura(
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit)
        {
            var carrier = Require<BlueprintBuff>(bySymbol, Astral31Prefix + "ProtectiveCarrier");
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Astral31Prefix + "ProtectiveArea");
            var recipient = Require<BlueprintBuff>(bySymbol, Astral31Prefix + "ProtectiveRecipient");
            carrier.name = InternalName(Astral31Prefix + "ProtectiveCarrier");
            area.name = InternalName(Astral31Prefix + "ProtectiveArea");
            recipient.name = InternalName(Astral31Prefix + "ProtectiveRecipient");
            carrier.Stacking = StackingType.Stack; recipient.Stacking = StackingType.Stack;
            var ac = ScriptableObject.CreateInstance<ArmorClassBonusAgainstAlignment>();
            ac.alignment = AlignmentComponent.Evil; ac.Value = 4;
            ac.Descriptor = ModifierDescriptor.Deflection;
            var saves = ScriptableObject.CreateInstance<SavingThrowBonusAgainstAlignment>();
            saves.Alignment = AlignmentComponent.Evil; saves.Value = 4;
            saves.Descriptor = ModifierDescriptor.Resistance;
            var globe = ScriptableObject.CreateInstance<AstralProtectiveGlobe>();
            globe.OwningBlueprint = unit; globe.MaximumSpellLevel = AstralDevaRulesPolicy.LesserGlobeMaximumSpellLevel;
            globe.RadiusFeet = AstralDevaRulesPolicy.AuraRadiusFeet;
            recipient.ComponentsArray = new BlueprintComponent[] { ac, saves, globe };
            var delivery = ScriptableObject.CreateInstance<AbilityAreaEffectBuff>();
            delivery.Buff = recipient;
            delivery.Condition = new ConditionsChecker { Conditions = Array.Empty<Condition>() };
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = new Feet(AstralDevaRulesPolicy.AuraRadiusFeet);
            area.AffectEnemies = true; area.AggroEnemies = false; area.AffectDead = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false; area.Fx = new PrefabLink();
            area.ComponentsArray = new BlueprintComponent[] { delivery };
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            if (carrier.StayOnDeath)
                throw new InvalidOperationException("Astral protective aura must end on emitter death.");
            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(Astral31Prefix + "ProtectiveAura.Name", "Astral Deva Protective Aura");
            access.Configure(carrier, title,
                LocalizationService.Create(Astral31Prefix + "ProtectiveCarrier.Description",
                    "A20-foot angel aura protects every covered creature against evil sources and outside low-level magic."), null);
            access.Configure(recipient, title,
                LocalizationService.Create(Astral31Prefix + "ProtectiveRecipient.Description",
                    "+4 deflection AC and +4 resistance saves against evil sources. Level0-3 spells and SLAs cast from outside this deva's20-foot aura cannot affect this creature."), null);
        }
    }
}
