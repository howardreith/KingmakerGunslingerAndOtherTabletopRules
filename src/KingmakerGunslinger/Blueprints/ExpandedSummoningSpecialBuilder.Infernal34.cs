using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
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
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Barbazu34Prefix = "KMG.Summoning.Infernal.BeardedDevil.";
        internal const string Kyton34Prefix = "KMG.Summoning.Infernal.KytonChainDevil.";

        private static void ConfigureInfernal34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            ConfigureBarbazu34(library, symbols);
            ConfigureKyton34(library, symbols);
        }

        private static BlueprintUnit InfernalProfile34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols, string token, string prefix,
            string title, int hd, int hp, int goodSave, int perception, int stealth, int mobility)
        {
            var unit = Require<BlueprintUnit>(symbols, "KMG.Summoning.Unit." + token);
            var type = Require<BlueprintUnitType>(symbols, prefix + "UnitType");
            type.name = InternalName(prefix + "UnitType");
            type.Name = LocalizationService.Create(prefix + "UnitType.Name", title);
            type.Image = ExpandedSummoningProjectIcons.Require(token == "BeardedDevil" ? "bearded-devil" : "kyton-chain-devil");
            type.KnowledgeStat = StatType.SkillLoreReligion;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var levels = OutsiderLevels(library, hd);
            levels.Skills = Array.Empty<StatType>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = hp;
            racial.BaseFortitude = racial.BaseReflex = goodSave; racial.BaseWill = 2;
            racial.PerceptionRanks = perception; racial.StealthRanks = stealth; racial.MobilityRanks = mobility;
            var extraplanar = ScriptableObject.CreateInstance<OutsiderExtraplanarIdentity>();
            extraplanar.OwningBlueprint = unit;
            extraplanar.NativeMarker = Feature(library, ExtraplanarSubtypeGuid, "native extraplanar identity");
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial, extraplanar };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                TheropodVisualPolicy.DonorGuid, "measured original-body animation carrier");
            if (donor.Prefab?.AssetId != TheropodVisualPolicy.DonorPrefabGuid)
                throw new InvalidOperationException("Infernal original-body rig changed.");
            unit.Prefab = donor.Prefab;
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            return unit;
        }

        private static List<BlueprintComponent> InfernalTraits34(BlueprintUnit unit,
            int naturalArmor, int resistance, int intimidateContribution)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(5); dr.Or = true; dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good; dr.BypassedByMaterial = true; dr.Material = PhysicalDamageMaterial.Silver;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC; armor.Value = naturalArmor; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>(); sr.Value = Simple(resistance); sr.AddCR = false;
            var skill = ScriptableObject.CreateInstance<InfernalIntimidate34>();
            skill.OwningBlueprint = unit; skill.TrainedContribution = intimidateContribution;
            var lifetime = ScriptableObject.CreateInstance<InfernalSourceLifetime34>(); lifetime.OwningBlueprint = unit;
            var commands = ScriptableObject.CreateInstance<OutsiderLifetimeBoundary>(); commands.OwningBlueprint = unit;
            return new List<BlueprintComponent> { dr, armor, sr, skill, lifetime, commands };
        }

        private static void BindInfernalTraits34(BlueprintFeature traits, string prefix, string title,
            string description, IEnumerable<BlueprintComponent> components)
        {
            traits.name = InternalName(prefix + "Traits"); traits.HideInUI = true; traits.IsClassFeature = false;
            traits.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(prefix + "Traits.Name", title + " Traits"),
                LocalizationService.Create(prefix + "Traits.Description", description), null);
        }

        private static BlueprintBrain InfernalBrain34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols, string prefix, params BlueprintAiAction[] actions)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, DumbBrainGuid, "native ordinary attack brain");
            var brain = Require<BlueprintBrain>(symbols, prefix + "Brain");
            CopyFields(donor, brain); brain.name = InternalName(prefix + "Brain");
            brain.ComponentsArray = (donor.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (donor.Actions ?? Array.Empty<BlueprintAiAction>()).Concat(actions).ToArray();
            return brain;
        }

        private static BlueprintUnitFact[] InfernalFacts34(LibraryScriptableObject library, BlueprintFeature traits)
        {
            return new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"), Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"), traits };
        }

        private static void ConfigureBarbazu34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Barbazu34Prefix;
            var unit = InfernalProfile34(library, symbols, "BeardedDevil", p, "Bearded Devil (Barbazu)", 6, 33, 5, 6, 6, 0);
            ConfigureUnitCore(unit, "BeardedDevil", "Bearded Devil (Barbazu)", Alignment.LawfulEvil,
                Size.Medium, 19, 15, 19, 6, 12, 10, 40);
            var glaive = Sprint29Weapon(library, symbols, p + "Glaive", p + "GlaiveType",
                "f83415c0e7ea1994d8a7f3dec8f5a861", Size.Medium, 1, DiceType.D10, 10);
            BindBarbazuNativeHand34(unit, glaive);
            var claw = Sprint29Weapon(library, symbols, p + "Claw", p + "ClawType",
                NativeClaw1d4Guid, Size.Medium, 1, DiceType.D6, 9);
            unit.Body = NaturalBody(glaive, new[] { claw }, Array.Empty<BlueprintItemWeapon>());
            unit.Body.PrimaryHandAlternative1 = claw; unit.Body.EmptyHandWeapon = null;
            var disease = Require<BlueprintBuff>(symbols, p + "DevilChills");
            var nativeDisease = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "9545a5550d89feb47a84edaeb4e63d0b", "native injury disease descriptor and removal surface");
            CopyFields(nativeDisease, disease); disease.name = InternalName(p + "DevilChills");
            disease.Stacking = StackingType.Stack; disease.Frequency = DurationRate.Rounds;
            var diseaseDescriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            diseaseDescriptor.Descriptor = SpellDescriptor.Disease;
            disease.ComponentsArray = new BlueprintComponent[] { diseaseDescriptor,
                ScriptableObject.CreateInstance<DevilChills34>(), ScriptableObject.CreateInstance<InfernalEffectLifetime34>() };
            BlueprintUnitFactAccess.Resolve().Configure(disease,
                LocalizationService.Create(p + "DevilChills.Name", "Devil Chills"),
                LocalizationService.Create(p + "DevilChills.Description",
                    "Injury disease: onset 1d4 days, then 1d4 Strength damage daily; three consecutive Fortitude saves cure it. Ends when its summoned source or summoner leaves the area."),
                ExpandedSummoningProjectIcons.Require("bearded-devil"));
            var wound = ConfigureInfernalWound34(library, symbols);
            var profile = ScriptableObject.CreateInstance<InfernalMartialProfile34>();
            profile.OwningBlueprint = unit; profile.Glaive = glaive; profile.Claw = claw;
            unit.ComponentsArray = unit.ComponentsArray.Concat(new BlueprintComponent[] { profile }).ToArray();
            var parts = InfernalTraits34(unit, 7, 16, 7);
            var poison = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); poison.Descriptor = SpellDescriptor.Poison;
            var poisonSpells = ScriptableObject.CreateInstance<SpellImmunityToSpellDescriptor>(); poisonSpells.Descriptor = SpellDescriptor.Poison;
            var focus = ScriptableObject.CreateInstance<WeaponFocus>(); focus.WeaponType = glaive.Type; focus.AttackBonus = 1;
            var strength = ScriptableObject.CreateInstance<DeinonychusPrimaryTalonStrength>(); strength.OwningBlueprint = unit; strength.Talon = claw;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = glaive; equipment.Slam = claw;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { glaive.Category }; proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var beard = ScriptableObject.CreateInstance<BarbazuBeard34>(); beard.OwningBlueprint = unit; beard.Claw = claw; beard.DevilChills = disease;
            var injury = ScriptableObject.CreateInstance<BarbazuWoundingGlaive34>(); injury.OwningBlueprint = unit; injury.Glaive = glaive; injury.Wound = wound;
            parts.AddRange(new BlueprintComponent[] { poison, poisonSpells, focus, strength, equipment, proficiency,
                Energy(DamageEnergyType.Acid, 10), Energy(DamageEnergyType.Cold, 10), beard, injury });
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits34(traits, p, "Barbazu", "Devil: fire and poison immunity; acid/cold resistance 10; DR5/good or silver; SR16. Glaive and two claws are alternative routines. Both claws hitting one target trigger the beard. Damaging glaive hits cause Infernal Wound.", parts);
            unit.AddFacts = InfernalFacts34(library, traits).Concat(new BlueprintUnitFact[] {
                Feature(library, FireImmunityGuid, "devil fire immunity"), Feature(library, PowerAttackGuid, "Power Attack") }).ToArray();
            var change = Require<BarbazuWeaponModeAi34>(symbols, p + "WeaponModeAi");
            change.name = InternalName(p + "WeaponModeAi"); change.OwningBlueprint = unit;
            change.BaseScore = 8; change.CooldownRounds = 1;
            change.ActorConsiderations = change.TargetConsiderations = Array.Empty<Consideration>();
            unit.Brain = InfernalBrain34(library, symbols, p, change);
        }

        // Only the private glaive's presentation style changes. The native
        // free-hand rig supplies its measured main-hand clip and act event;
        // category, two-hand damage, reach and commands stay with the glaive.
        private static void BindBarbazuNativeHand34(BlueprintUnit unit, BlueprintItemWeapon glaive)
        {
            var view = unit.Prefab.Load(false);
            var hand = view.GetComponentsInChildren<Kingmaker.Visual.Animation.Kingmaker.UnitAnimationManager>(true)
                .Single().AnimationSet.Actions.OfType<UnitAnimationActionHandAttack>()
                .Single(value => value.IsMainHand && value.name == "Lizardfolk_FreeHands_AnimationSet_MainHandAttack");
            var field = typeof(UnitAnimationActionHandAttack).GetField("m_Settings", BindingFlags.Instance | BindingFlags.NonPublic);
            var settings = field?.GetValue(hand) as Array;
            if (settings == null || settings.Length == 0 || settings.Length > 32)
                throw new InvalidOperationException("Barbazu native hand mapping unavailable.");
            var setting = settings.Cast<object>().First(value => {
                var clips = value.GetType().GetField("Variants").GetValue(value) as AnimationClip[];
                return clips != null && clips.Length > 0 && clips[0] != null &&
                    clips.Where(clip => clip != null).All(clip => clip.name == "Attack_01_3" &&
                        clip.events.Any(evt => evt.functionName == "PostCommandActEvent"));
            });
            var style = (WeaponAnimationStyle)setting.GetType().GetField("Style").GetValue(setting);
            var visual = new WeaponVisualParameters(); CopyFields(glaive.VisualParameters, visual);
            SetField(visual, "m_WeaponAnimationStyle", style);
            SetField(glaive, "m_VisualParameters", visual);
            if (glaive.VisualParameters.AnimStyle != style || !hand.HasSetting(style))
                throw new InvalidOperationException("Barbazu private weapon mapping did not round-trip.");
        }

        private static void ConfigureKyton34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Kyton34Prefix;
            var unit = InfernalProfile34(library, symbols, "KytonChainDevil", p, "Kyton (Chain Devil)", 8, 44, 6, 8, 0, 8);
            ConfigureUnitCore(unit, "KytonChainDevil", "Kyton (Chain Devil)", Alignment.LawfulEvil,
                Size.Medium, 15, 17, 14, 11, 12, 12, 30);
            var chain = Sprint29Weapon(library, symbols, p + "Chain", p + "ChainType", NativeClaw1d4Guid,
                Size.Medium, 2, DiceType.D4, 10);
            SetField(chain.Type, "m_TypeNameText", LocalizationService.Create(p + "ChainType.Name", "Chain"));
            BlueprintItemAccess.Resolve().Configure(chain, LocalizationService.Create(p + "Chain.Name", "Kyton Chain"),
                LocalizationService.Create(p + "Chain.Description", "One of this kyton's four controlled, nontransferable chains."),
                LocalizationService.Create(p + "Chain.Flavor", ""), 0, 0);
            unit.Body = NaturalBody(chain, new[] { chain, chain, chain }, Array.Empty<BlueprintItemWeapon>());
            var controlled = Require<BlueprintBuff>(symbols, p + "ControlledChains");
            controlled.name = InternalName(p + "ControlledChains"); controlled.Stacking = StackingType.Replace;
            controlled.Frequency = DurationRate.Rounds; SetBuffFlags(controlled, false, false);
            controlled.ComponentsArray = new BlueprintComponent[] { ScriptableObject.CreateInstance<InfernalEffectLifetime34>() };
            BlueprintUnitFactAccess.Resolve().Configure(controlled,
                LocalizationService.Create(p + "ControlledChains.Name", "Dancing Chains"),
                LocalizationService.Create(p + "ControlledChains.Description", "This kyton's existing chains reach 25 feet. Attacks still require normal native attack commands and actions."),
                ExpandedSummoningProjectIcons.Require("kyton-chain-devil"));
            var control = InfernalAbility34(symbols, p + "DancingChains", "Dancing Chains", false);
            var applyControl = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            applyControl.Buff = controlled; applyControl.Permanent = true; applyControl.ToCaster = true;
            applyControl.DurationValue = MephitRounds(0);
            // Even permanent effects initialize every nested duration field.
            applyControl.DurationValue.DiceType = DiceType.Zero; applyControl.DurationValue.DiceCountValue = Simple(0);
            var controlRun = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            controlRun.Actions = new ActionList { Actions = new GameAction[] { applyControl } };
            control.ComponentsArray = new BlueprintComponent[] { controlRun };
            var gaze = ConfigureKytonGaze34(library, symbols);
            var profile = ScriptableObject.CreateInstance<InfernalMartialProfile34>();
            profile.OwningBlueprint = unit; profile.Chain = chain; profile.DancingChains = controlled;
            unit.ComponentsArray = unit.ComponentsArray.Concat(new BlueprintComponent[] { profile }).ToArray();
            var parts = InfernalTraits34(unit, 4, 17, 11);
            var armor = ScriptableObject.CreateInstance<AddStatBonus>(); armor.Stat = StatType.AC;
            armor.Descriptor = ModifierDescriptor.Armor; armor.Value = 4;
            var focus = ScriptableObject.CreateInstance<WeaponFocus>(); focus.WeaponType = chain.Type; focus.AttackBonus = 1;
            var strength = ScriptableObject.CreateInstance<DeinonychusPrimaryTalonStrength>(); strength.OwningBlueprint = unit; strength.Talon = chain;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>(); equipment.OwningBlueprint = unit; equipment.Slam = chain;
            var regen = ScriptableObject.CreateInstance<AddEffectRegeneration>(); regen.Heal = 2; regen.Unremovable = false;
            regen.CancelByMagicWeapon = false; regen.CancelDamageEnergyTypes = Array.Empty<DamageEnergyType>();
            regen.CancelDamageAlignmentTypes = new[] { DamageAlignment.Good }; regen.CancelDamageMaterials = new[] { PhysicalDamageMaterial.Silver };
            var regeneration = Require<BlueprintBuff>(symbols, p + "Regeneration");
            regeneration.name = InternalName(p + "Regeneration"); regeneration.Frequency = DurationRate.Rounds;
            regeneration.Stacking = StackingType.Replace; SetBuffFlags(regeneration, false, true);
            regeneration.ComponentsArray = new BlueprintComponent[] { regen };
            var regenerationGrant = ScriptableObject.CreateInstance<AddFacts>();
            regenerationGrant.Facts = new BlueprintUnitFact[] { regeneration };
            regenerationGrant.DoNotRestoreMissingFacts = false; parts.Add(regenerationGrant);
            var alertness = ScriptableObject.CreateInstance<AddStatBonus>(); alertness.Stat = StatType.SkillPerception;
            alertness.Descriptor = ModifierDescriptor.UntypedStackable; alertness.Value = 2;
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>(); parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { gaze, control }; parameters.SpellLevels = new[] { 4, 0 }; parameters.CasterLevel = 8;
            TrumpetBlindFight.VerifyContract();
            var nativeBlind = Feature(library, "4e219f5894ad0ea4daa0699e28c37b1d", "native Blind-Fight melee defense");
            var blind = ScriptableObject.CreateInstance<TrumpetBlindFight>(); blind.OwningBlueprint = unit;
            parts.AddRange(new BlueprintComponent[] { armor, focus, strength, equipment, alertness, parameters, blind,
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(nativeBlind.ComponentsArray.OfType<FlatFootedIgnore>().Single()) });
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits34(traits, p, "Kyton", "Cold immunity; DR5/good or silver; SR17; regeneration2, suppressed by good or silver. Chain armor grants a typed +4 armor bonus without wearable-armor penalties. Four chain attacks, Dancing Chains and targeted Unnerving Gaze. Blind-Fight grants no gaze immunity. Alertness: +2 Perception; Sense Motive has no native skill consumer.", parts);
            unit.AddFacts = InfernalFacts34(library, traits).Concat(new BlueprintUnitFact[] {
                Feature(library, "9ae23798a9284e044ad2716a772a410e", "Kyton cold immunity"), control, gaze }).ToArray();
            var controlAi = Require<KytonControlAi34>(symbols, p + "ControlAi");
            ConfigureMephitAiAction(controlAi, p + "ControlAi", control, 1); controlAi.OwningBlueprint = unit; controlAi.Controlled = controlled; controlAi.BaseScore = 6;
            var gazeAi = Require<KytonGazeAi34>(symbols, p + "GazeAi");
            ConfigureMephitAiAction(gazeAi, p + "GazeAi", gaze, 1); gazeAi.OwningBlueprint = unit;
            gazeAi.Shaken = Require<BlueprintBuff>(symbols, p + "Shaken"); gazeAi.BaseScore = 4;
            unit.Brain = InfernalBrain34(library, symbols, p, controlAi, gazeAi);
        }

        private static BlueprintAbility InfernalAbility34(IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string title, bool hostile)
        {
            var ability = Require<BlueprintAbility>(symbols, symbol); ability.name = InternalName(symbol);
            ability.Type = AbilityType.Supernatural; ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false); ability.Range = hostile ? AbilityRange.Custom : AbilityRange.Personal;
            ability.CustomRange = new Feet(30); ability.SpellResistance = false;
            ability.CanTargetEnemies = ability.CanTargetFriends = hostile; ability.CanTargetSelf = !hostile;
            ability.CanTargetPoint = false; ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = ability.EffectOnAlly = hostile ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.Helpful;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title), LocalizationService.Create(symbol + ".Description",
                    hostile ? "Choose a visible creature within 30 feet. Will negates; otherwise shaken for 1d3 rounds. Mind-affecting fear. Standard action."
                        : "Standard action: control only this kyton's four existing chains, extending their reach from 10 to 25 feet. Activation makes no attacks. Ends with the summon or summoner leaving the area."),
                ExpandedSummoningProjectIcons.Require("kyton-chain-devil"));
            return ability;
        }

        private static BlueprintAbility ConfigureKytonGaze34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Kyton34Prefix;
            var shaken = Require<BlueprintBuff>(symbols, p + "Shaken");
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "25ec6cb6ab1845c48a95f9c20b034220", "native shaken state");
            CopyFields(native, shaken); shaken.name = InternalName(p + "Shaken"); shaken.Stacking = StackingType.Stack;
            shaken.ComponentsArray = native.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Where(value => !(value is SpellDescriptorComponent)).Concat(new BlueprintComponent[] {
                    ScriptableObject.CreateInstance<InfernalEffectLifetime34>() }).ToArray();
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>(); descriptor.Descriptor = SpellDescriptor.Fear | SpellDescriptor.MindAffecting;
            shaken.ComponentsArray = shaken.ComponentsArray.Concat(new BlueprintComponent[] { descriptor }).ToArray();
            var ability = InfernalAbility34(symbols, p + "UnnervingGaze", "Unnerving Gaze", true);
            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>(); apply.Buff = shaken;
            apply.DurationValue = new ContextDurationValue { Rate = DurationRate.Rounds, DiceType = DiceType.D3,
                DiceCountValue = Simple(1), BonusValue = Simple(0) };
            var saved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
            saved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() }; saved.Failed = new ActionList { Actions = new GameAction[] { apply } };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>(); run.SavingThrowType = SavingThrowType.Will;
            run.Actions = new ActionList { Actions = new GameAction[] { saved } };
            ability.ComponentsArray = new BlueprintComponent[] { run,
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(descriptor), ScriptableObject.CreateInstance<KytonGazeSight34>() };
            ability.LocalizedSavingThrow = LocalizationService.Create(p + "UnnervingGaze.Save", "Will negates");
            ability.LocalizedDuration = LocalizationService.Create(p + "UnnervingGaze.Duration", "1d3 rounds");
            return ability;
        }
    }
}
