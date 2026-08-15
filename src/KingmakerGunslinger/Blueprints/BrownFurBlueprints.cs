using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.BrownFur;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BrownFurBlueprintSet
    {
        internal BrownFurBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature powerful, BlueprintAbility selection,
            BlueprintAbility[] scoreAbilities, BlueprintBuff[] scoreBuffs,
            BlueprintFeature share, BlueprintActivatableAbility shareAbility,
            BlueprintBuff shareBuff, BlueprintFeature supremacy)
        {
            Archetype = archetype; PowerfulChange = powerful;
            PowerfulChangeSelection = selection; ScoreAbilities = scoreAbilities;
            ScoreBuffs = scoreBuffs; ShareTransmutation = share;
            ShareTransmutationAbility = shareAbility;
            ShareTransmutationBuff = shareBuff;
            TransmutationSupremacy = supremacy;
        }

        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature PowerfulChange { get; private set; }
        internal BlueprintAbility PowerfulChangeSelection { get; private set; }
        internal BlueprintAbility[] ScoreAbilities { get; private set; }
        internal BlueprintBuff[] ScoreBuffs { get; private set; }
        internal BlueprintFeature ShareTransmutation { get; private set; }
        internal BlueprintActivatableAbility ShareTransmutationAbility
        { get; private set; }
        internal BlueprintBuff ShareTransmutationBuff { get; private set; }
        internal BlueprintFeature TransmutationSupremacy { get; private set; }
        internal int Count { get { return 19; } }
    }

    internal static class BrownFurBlueprints
    {
        internal const string DisplayName = "Brown-Fur Transmuter";
        private static readonly BrownFurAbilityScore[] Scores = {
            BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity,
            BrownFurAbilityScore.Constitution, BrownFurAbilityScore.Intelligence,
            BrownFurAbilityScore.Wisdom, BrownFurAbilityScore.Charisma };

        internal static BrownFurBlueprintSet Register(BlueprintRegistry registry,
            CotwArcanistContract contract)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            ValidateContract(contract);
            Sprite icon = contract.ExploitSelection.Icon ??
                contract.MagicalSupremacy.Icon;
            BlueprintBuff[] scoreBuffs = Scores.Select(score =>
                registry.Register<BlueprintBuff>(
                    BrownFurIdentityCatalog.PowerfulBuff(score),
                    () => CreatePendingScoreBuff(score, icon))).ToArray();
            BlueprintAbility[] scoreAbilities = Scores.Select((score, index) =>
                registry.Register<BlueprintAbility>(
                    BrownFurIdentityCatalog.PowerfulAbility(score),
                    () => CreateScoreAbility(score, scoreBuffs[index],
                        scoreBuffs, icon))).ToArray();
            BlueprintAbility selection = registry.Register<BlueprintAbility>(
                BrownFurIdentityCatalog.PowerfulChangeSelection,
                () => CreateSelection(scoreAbilities, icon));
            BlueprintFeature powerful = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.PowerfulChangeFeature,
                () => CreateFeature("Powerful Change",
                    "When you cast a Transmutation spell using an Arcanist spell slot, you may spend 1 point from your arcane reservoir to increase one positive ability-score bonus granted by the spell by 2. Select the ability score before casting. At 20th level, the increase is 4.",
                    selection, icon));
            BlueprintBuff shareBuff = registry.Register<BlueprintBuff>(
                BrownFurIdentityCatalog.ShareBuff,
                () => CreateMarkerBuff("Share Transmutation",
                    "Your next eligible Personal-range Transmutation spell may spend 1 arcane reservoir point to affect a willing creature by touch. At 20th level the exact range is 30 feet.", icon));
            BlueprintActivatableAbility shareAbility = registry.Register<
                BlueprintActivatableAbility>(
                    BrownFurIdentityCatalog.ShareActivatable,
                    () => CreateShareAbility(shareBuff, icon));
            BlueprintFeature share = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.ShareFeature,
                () => CreateFeature("Share Transmutation",
                    "Spend 1 point from your arcane reservoir when casting a genuine Personal-range Transmutation spell to affect a willing creature by touch. At 20th level, you may instead target a willing creature within exactly 30 feet.",
                    shareAbility, icon));
            BlueprintFeature supremacy = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.SupremacyFeature,
                () => CreateFeature("Transmutation Supremacy",
                    "Every genuine Transmutation spell you cast is Extended without changing its spell slot or casting time. A spell already affected by Extend is not extended again. Powerful Change increases bonuses by 4, and Share Transmutation reaches exactly 30 feet.",
                    null, contract.MagicalSupremacy.Icon ?? icon));
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                BrownFurIdentityCatalog.Archetype,
                () => CreateArchetype(contract, powerful, share, supremacy));
            var set = new BrownFurBlueprintSet(archetype, powerful, selection,
                scoreAbilities, scoreBuffs, share, shareAbility, shareBuff,
                supremacy);
            Validate(set, contract);
            return set;
        }

        private static BlueprintArchetype CreateArchetype(
            CotwArcanistContract contract, BlueprintFeature powerful,
            BlueprintFeature share, BlueprintFeature supremacy)
        {
            BrownFurArchetypePlan plan = BrownFurArchetypePlan.Create(
                contract.ProgressionDecision);
            var archetype = ScriptableObject.CreateInstance<BlueprintArchetype>();
            archetype.name = "KMG_BrownFurTransmuter_Archetype";
            archetype.LocalizedName = LocalizationService.Create(
                "KMG.BrownFur.Name", DisplayName);
            archetype.LocalizedDescription = LocalizationService.Create(
                "KMG.BrownFur.Description",
                "Brown-fur transmuters master the alteration of living creatures. They can intensify ability-score bonuses, share Personal transmutations with willing creatures, and ultimately extend every transmutation they cast.");
            archetype.OverrideAttributeRecommendations = false;
            archetype.RecommendedAttributes = Array.Empty<StatType>();
            archetype.NotRecommendedAttributes = Array.Empty<StatType>();
            FieldInfo parent = typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (parent == null || parent.FieldType != typeof(BlueprintCharacterClass))
                throw new MissingFieldException(typeof(BlueprintArchetype).FullName,
                    "m_ParentClass");
            parent.SetValue(archetype, contract.ArcanistClass);
            archetype.ReplaceStartingEquipment = false;
            archetype.StartingGold = contract.ArcanistClass.StartingGold;
            archetype.StartingItems = Array.Empty<BlueprintItem>();
            archetype.AddFeatures = plan.Additions.Select(value => Entry(value.Level,
                Addition(value.Feature, powerful, share, supremacy))).ToArray();
            archetype.RemoveFeatures = plan.Removals.Select(value => Entry(
                value.Level,
                value.Feature == BrownFurProgressionFeature.ArcanistExploit ?
                    (BlueprintFeatureBase)contract.ExploitSelection :
                    contract.MagicalSupremacy)).ToArray();
            archetype.ComponentsArray = Array.Empty<BlueprintComponent>();
            return archetype;
        }

        private static BlueprintFeatureBase Addition(
            BrownFurProgressionFeature kind, BlueprintFeature powerful,
            BlueprintFeature share, BlueprintFeature supremacy)
        {
            if (kind == BrownFurProgressionFeature.PowerfulChange) return powerful;
            if (kind == BrownFurProgressionFeature.ShareTransmutation) return share;
            if (kind == BrownFurProgressionFeature.TransmutationSupremacy)
                return supremacy;
            throw new InvalidOperationException("Unsupported Brown-Fur addition: " +
                kind);
        }

        private static BlueprintBuff CreatePendingScoreBuff(
            BrownFurAbilityScore score, Sprite icon)
        {
            return CreateMarkerBuff("Powerful Change: " + score,
                "Powerful Change will empower the next eligible Transmutation spell only if it grants a positive " + score + " bonus. The selection clears with that cast.", icon);
        }

        private static BlueprintBuff CreateMarkerBuff(string name,
            string description, Sprite icon)
        {
            var buff = ScriptableObject.CreateInstance<BlueprintBuff>();
            buff.name = "KMG_" + name.Replace(" ", string.Empty)
                .Replace(":", string.Empty) + "_Buff";
            string key = name.Replace(" ", string.Empty).Replace(":", string.Empty);
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create("KMG.BrownFur." + key +
                    ".Buff.Name", name),
                LocalizationService.Create("KMG.BrownFur." + key +
                    ".Buff.Description", description), icon);
            buff.Stacking = StackingType.Replace;
            buff.ComponentsArray = Array.Empty<BlueprintComponent>();
            buff.FxOnStart = new PrefabLink();
            buff.FxOnRemove = new PrefabLink();
            buff.ResourceAssetIds = Array.Empty<string>();
            return buff;
        }

        private static BlueprintAbility CreateScoreAbility(
            BrownFurAbilityScore score, BlueprintBuff selected,
            BlueprintBuff[] all, Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_BrownFur_PowerfulChange_" + score,
                "Powerful Change: " + score,
                "Select " + score + " for your next eligible Powerful Change cast. Selecting another ability score replaces this choice.", icon);
            ability.ComponentsArray = new BlueprintComponent[] {
                BrownFurPowerfulChangeSelectionLogic.Create(selected, all) };
            return ability;
        }

        private static BlueprintAbility CreateSelection(
            BlueprintAbility[] choices, Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_BrownFur_PowerfulChange_Selection", "Powerful Change",
                "Choose the one ability score that Powerful Change should empower on your next eligible Arcanist-slot Transmutation cast.", icon);
            var variants = ScriptableObject.CreateInstance<AbilityVariants>();
            variants.name = "$KMG_BrownFur_PowerfulChange_Variants";
            variants.Variants = (BlueprintAbility[])choices.Clone();
            ability.ComponentsArray = new BlueprintComponent[] { variants };
            return ability;
        }

        private static BlueprintAbility CreatePersonalAbility(string assetName,
            string displayName, string description, Sprite icon)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            ability.name = assetName;
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(assetName + ".Name", displayName),
                LocalizationService.Create(assetName + ".Description", description),
                icon);
            ability.Type = AbilityType.Supernatural;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetSelf = true;
            ability.CanTargetPoint = ability.CanTargetEnemies =
                ability.CanTargetFriends = false;
            ability.SpellResistance = false;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            ability.ActionType = UnitCommand.CommandType.Free;
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.LocalizedDuration = LocalizationService.Create(
                assetName + ".Duration", "Until the corresponding cast");
            ability.LocalizedSavingThrow = LocalizationService.Create(
                assetName + ".SavingThrow", "None");
            ability.ComponentsArray = Array.Empty<BlueprintComponent>();
            return ability;
        }

        private static BlueprintActivatableAbility CreateShareAbility(
            BlueprintBuff buff, Sprite icon)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
            ability.name = "KMG_BrownFur_ShareTransmutation_Activatable";
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.BrownFur.Share.Ability.Name",
                    "Share Transmutation"),
                LocalizationService.Create("KMG.BrownFur.Share.Ability.Description",
                    "Arm Share Transmutation for the next eligible Personal-range Transmutation spell. No reservoir point is spent merely for activating this selection."), icon);
            ability.Buff = buff;
            ability.Group = ActivatableAbilityGroup.None;
            ability.WeightInGroup = 1;
            ability.IsOnByDefault = false;
            ability.ActivationType = AbilityActivationType.Immediately;
            ability.DeactivateIfCombatEnded = false;
            ability.DeactivateAfterFirstRound = false;
            ability.DeactivateImmediately = false;
            ability.DeactivateIfOwnerDisabled = false;
            ability.DeactivateIfOwnerUnconscious = false;
            ability.OnlyInCombat = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.ComponentsArray = Array.Empty<BlueprintComponent>();
            ability.ResourceAssetIds = Array.Empty<string>();
            return ability;
        }

        private static BlueprintFeature CreateFeature(string name,
            string description, BlueprintUnitFact granted, Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_BrownFur_" + name.Replace(" ", string.Empty) +
                "_Feature";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            if (granted != null)
            {
                var add = ScriptableObject.CreateInstance<AddFacts>();
                add.name = "$KMG_BrownFur_Grant_" + name.Replace(" ", string.Empty);
                add.Facts = new[] { granted };
                add.DoNotRestoreMissingFacts = false;
                feature.ComponentsArray = new BlueprintComponent[] { add };
            }
            string key = name.Replace(" ", string.Empty);
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.BrownFur." + key + ".Name", name),
                LocalizationService.Create("KMG.BrownFur." + key +
                    ".Description", description), icon);
            return feature;
        }

        private static LevelEntry Entry(int level,
            params BlueprintFeatureBase[] features)
        { return new LevelEntry { Level = level, Features = features.ToList() }; }

        private static void ValidateContract(CotwArcanistContract contract)
        {
            if (contract == null || contract.ArcanistClass == null ||
                contract.ArcanistProgression == null ||
                contract.ExploitSelection == null ||
                contract.MagicalSupremacy == null ||
                contract.ProgressionDecision == null ||
                !contract.ProgressionDecision.Compatible)
                throw new InvalidOperationException(
                    "Brown-Fur blueprints require the complete compatible CotW Arcanist contract.");
        }

        internal static void Validate(BrownFurBlueprintSet set,
            CotwArcanistContract contract)
        {
            if (set == null || set.Count != BrownFurIdentityCatalog.IdentityCount ||
                set.ScoreAbilities == null || set.ScoreAbilities.Length != 6 ||
                set.ScoreBuffs == null || set.ScoreBuffs.Length != 6 ||
                set.Archetype == null || set.Archetype.AddFeatures == null ||
                set.Archetype.AddFeatures.Length != 3 ||
                set.Archetype.RemoveFeatures == null ||
                set.Archetype.RemoveFeatures.Length != 3 ||
                !set.Archetype.AddFeatures.Select(value => value.Level)
                    .SequenceEqual(new[] { 3, 9, 20 }) ||
                !set.Archetype.RemoveFeatures.Select(value => value.Level)
                    .SequenceEqual(new[] {
                        contract.ProgressionDecision.PowerfulChangeReplacementLevel,
                        contract.ProgressionDecision.ShareTransmutationReplacementLevel,
                        20 }) ||
                !ReferenceEquals(set.Archetype.RemoveFeatures[0].Features.Single(),
                    contract.ExploitSelection) ||
                !ReferenceEquals(set.Archetype.RemoveFeatures[1].Features.Single(),
                    contract.ExploitSelection) ||
                !ReferenceEquals(set.Archetype.RemoveFeatures[2].Features.Single(),
                    contract.MagicalSupremacy))
                throw new InvalidOperationException(
                    "Brown-Fur archetype shell failed its exact progression contract.");
        }
    }
}
