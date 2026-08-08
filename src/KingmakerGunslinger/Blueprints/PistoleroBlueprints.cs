using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Archetypes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class PistoleroBlueprintSet
    {
        internal PistoleroBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature upCloseAndDeadly, BlueprintFeature twinShot,
            BlueprintFeature[] deedTiers, BlueprintFeature training,
            BlueprintAbility upCloseAbility, BlueprintBuff upCloseArmed)
        {
            Archetype = archetype;
            UpCloseAndDeadly = upCloseAndDeadly;
            TwinShotKnockdown = twinShot;
            DeedTiers = deedTiers;
            Training = training;
            UpCloseAbility = upCloseAbility;
            UpCloseArmed = upCloseArmed;
        }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature UpCloseAndDeadly { get; private set; }
        internal BlueprintFeature TwinShotKnockdown { get; private set; }
        internal BlueprintFeature[] DeedTiers { get; private set; }
        internal BlueprintFeature Training { get; private set; }
        internal BlueprintAbility UpCloseAbility { get; private set; }
        internal BlueprintBuff UpCloseArmed { get; private set; }
        internal BlueprintAbility TwinShotAbility { get; set; }
        internal int Count { get { return 9; } }
    }

    internal static class PistoleroBlueprints
    {
        internal const string ArchetypeSymbol = "KMG.Archetypes.Pistolero";
        internal const string UpCloseSymbol =
            "KMG.Archetypes.UpCloseAndDeadly";
        internal const string UpCloseAbilitySymbol =
            "KMG.Archetypes.UpCloseAndDeadlyAbility";
        internal const string UpCloseArmedSymbol =
            "KMG.Archetypes.UpCloseAndDeadlyArmed";
        internal const string TwinShotSymbol =
            "KMG.Archetypes.TwinShotKnockdown";
        internal const string TwinShotAbilitySymbol =
            "KMG.Archetypes.TwinShotKnockdownAbility";
        private static readonly string[] TierSymbols = {
            "KMG.Archetypes.PistoleroDeedsLevel1",
            "KMG.Archetypes.PistoleroDeedsLevel7",
            "KMG.Archetypes.PistoleroDeedsLevel11" };

        internal static PistoleroBlueprintSet Register(BlueprintRegistry registry,
            GunslingerClassBlueprintSet gunslinger,
            FirearmTrainingBlueprintSet training,
            BlueprintAbilityResource grit)
        {
            BlueprintBuff upCloseArmed = registry.Register<BlueprintBuff>(
                UpCloseArmedSymbol, () => CreateUpCloseArmed(grit,
                    gunslinger.CharacterClass));
            BlueprintAbility upCloseAbility = registry.Register<BlueprintAbility>(
                UpCloseAbilitySymbol, () => CreateUpCloseAbility(
                    upCloseArmed, grit));
            BlueprintFeature upClose = registry.Register<BlueprintFeature>(
                UpCloseSymbol, () => CreateUpCloseFeature(upCloseAbility));
            BlueprintAbility twinAbility = registry.Register<BlueprintAbility>(
                TwinShotAbilitySymbol, () => CreateTwinShotAbility(grit));
            BlueprintFeature twin = registry.Register<BlueprintFeature>(
                TwinShotSymbol, () => CreateTwinShotFeature(twinAbility));
            BlueprintFeature[] tiers = {
                RegisterTier(registry, 0,
                    "Up Close and Deadly, Gunslinger's Dodge, and Quick Clear."),
                RegisterTier(registry, 1,
                    "Deadeye, Dead Shot, and Targeting deeds."),
                RegisterTier(registry, 2,
                    "Twin Shot Knockdown, Expert Loading, and Lightning Reload.") };
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                ArchetypeSymbol, () => CreateArchetype(gunslinger, training,
                    upClose, twin, tiers));
            if (!gunslinger.CharacterClass.Archetypes.Any(value =>
                    ReferenceEquals(value, archetype)))
                gunslinger.CharacterClass.Archetypes = gunslinger.CharacterClass
                    .Archetypes.Concat(new[] { archetype }).ToArray();
            var set = new PistoleroBlueprintSet(archetype, upClose, twin, tiers,
                training.Pistol, upCloseAbility, upCloseArmed);
            set.TwinShotAbility = twinAbility;
            return set;
        }

        private static BlueprintAbility CreateTwinShotAbility(
            BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_TwinShotKnockdown_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TwinShotKnockdown.Ability.Name",
                    "Twin Shot Knockdown"),
                LocalizationService.Create("KMG.TwinShotKnockdown.Ability.Description",
                    "As a free action after two distinct one-handed firearm hits against this target during your current turn, spend 1 grit to knock it prone without a save or combat maneuver check."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Close;
            result.CanTargetEnemies = true;
            result.CanTargetSelf = result.CanTargetFriends = result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.ActionType = UnitCommand.CommandType.Free;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.TwinShotKnockdown.Ability.Duration", "Instantaneous");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.TwinShotKnockdown.Ability.SavingThrow", "None");
            var logic = ScriptableObject.CreateInstance<TwinShotKnockdownAbilityLogic>();
            logic.name = "$KMG_TwinShotKnockdown_Deliver";
            logic.Grit = grit;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateTwinShotFeature(BlueprintAbility ability)
        {
            BlueprintFeature result = CreateFeature("Twin Shot Knockdown",
                "After two distinct one-handed firearm hits against the same target during your turn, you may spend 1 grit to knock that target prone without a save or combat maneuver check.");
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_TwinShotKnockdown_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            var tracker = ScriptableObject.CreateInstance<TwinShotHitTracker>();
            tracker.name = "$KMG_TwinShotKnockdown_Hits";
            result.ComponentsArray = new BlueprintComponent[] { add, tracker };
            return result;
        }

        private static BlueprintBuff CreateUpCloseArmed(
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslinger)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_UpCloseAndDeadly_Armed";
            result.Stacking = StackingType.Replace;
            var handler = ScriptableObject.CreateInstance<
                UpCloseAndDeadlyAttackHandler>();
            handler.name = "$KMG_UpCloseAndDeadly_Attack";
            handler.Grit = grit;
            handler.GunslingerClass = gunslinger;
            result.ComponentsArray = new BlueprintComponent[] { handler };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.UpCloseAndDeadly.Armed.Name",
                    "Up Close and Deadly Armed"),
                LocalizationService.Create(
                    "KMG.UpCloseAndDeadly.Armed.Description",
                    "Your next one-handed direct firearm attack this turn delivers Up Close and Deadly after its result is known."), null);
            return result;
        }

        private static BlueprintAbility CreateUpCloseAbility(
            BlueprintBuff armed, BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_UpCloseAndDeadly_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.UpCloseAndDeadly.Ability.Name",
                    "Up Close and Deadly"),
                LocalizationService.Create(
                    "KMG.UpCloseAndDeadly.Ability.Description",
                    "As a free action, arm your next one-handed, non-scatter firearm attack this turn. After the result is known, spend exactly 1 grit to deal scaling precision damage on a hit or half of the same roll on a miss. The extra damage is not multiplied on a critical hit, and True Grit cannot reduce its cost."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.UpCloseAndDeadly.Ability.Duration",
                "Current turn or until used");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.UpCloseAndDeadly.Ability.SavingThrow", "None");
            var logic = ScriptableObject.CreateInstance<
                UpCloseAndDeadlyAbilityLogic>();
            logic.name = "$KMG_UpCloseAndDeadly_Arm";
            logic.ArmedMarker = armed;
            logic.Grit = grit;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateUpCloseFeature(
            BlueprintAbility ability)
        {
            BlueprintFeature result = CreateFeature("Up Close and Deadly",
                "As a free action, arm your next one-handed, non-scatter firearm attack this turn. After the result is known, spend exactly 1 grit to deal scaling precision damage on a hit or half of the same roll on a miss. This cost cannot be reduced by True Grit.");
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_UpCloseAndDeadly_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            return result;
        }

        private static BlueprintArchetype CreateArchetype(
            GunslingerClassBlueprintSet g, FirearmTrainingBlueprintSet training,
            BlueprintFeature upClose, BlueprintFeature twin,
            BlueprintFeature[] tiers)
        {
            var result = ScriptableObject.CreateInstance<BlueprintArchetype>();
            result.name = "KMG_Pistolero_Archetype";
            result.LocalizedName = LocalizationService.Create(
                "KMG.Pistolero.Name", "Pistolero");
            result.LocalizedDescription = LocalizationService.Create(
                "KMG.Pistolero.Description",
                "A one-handed firearm specialist who fights at close range with speed and precision.");
            result.OverrideAttributeRecommendations = true;
            result.RecommendedAttributes = new[] { StatType.Dexterity,
                StatType.Wisdom };
            result.NotRecommendedAttributes = Array.Empty<StatType>();
            typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                    result, g.CharacterClass);
            result.ReplaceStartingEquipment = false;
            result.RemoveFeatures = new[] {
                Entry(1, g.Proficiencies, g.Deadeye.Feature, g.DeedTiers[0]),
                Entry(5, g.GunTraining.Selection),
                Entry(7, g.StartlingShot.Feature, g.DeedTiers[1]),
                Entry(9, g.GunTraining.Selection),
                Entry(11, g.BleedingWound.Feature, g.DeedTiers[2]),
                Entry(13, g.GunTraining.Selection),
                Entry(17, g.GunTraining.Selection) };
            result.AddFeatures = new[] {
                Entry(1, g.ArchetypeProficiencies.Pistolero, upClose, tiers[0]),
                Entry(5, training.Pistol),
                Entry(7, g.Deadeye.Feature, tiers[1]),
                Entry(9, training.Pistol),
                Entry(11, twin, tiers[2]),
                Entry(13, training.Pistol),
                Entry(17, training.Pistol) };
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintFeature RegisterTier(BlueprintRegistry registry,
            int index, string description)
        {
            return registry.Register<BlueprintFeature>(TierSymbols[index], () =>
                CreateFeature("Pistolero Deeds — Level " +
                    new[] { 1, 7, 11 }[index], description));
        }

        private static BlueprintFeature CreateFeature(string name,
            string description)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_" + name.Replace(" ", "_");
            result.Ranks = 1;
            result.IsClassFeature = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Pistolero." +
                    name.Replace(" ", "") + ".Name", name),
                LocalizationService.Create("KMG.Pistolero." +
                    name.Replace(" ", "") + ".Description", description), null);
            return result;
        }

        private static LevelEntry Entry(int level,
            params BlueprintFeatureBase[] features)
        { return new LevelEntry { Level = level, Features = features.ToList() }; }
    }
}
