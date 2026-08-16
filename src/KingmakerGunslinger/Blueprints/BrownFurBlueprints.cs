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
            BlueprintAbility[] scoreAbilities,
            BlueprintActivatableAbility[] scoreActivatables,
            BlueprintBuff[] scoreBuffs,
            BlueprintFeature share, BlueprintActivatableAbility shareAbility,
            BlueprintBuff shareBuff, BlueprintFeature supremacy)
        {
            Archetype = archetype; PowerfulChange = powerful;
            PowerfulChangeSelection = selection; ScoreAbilities = scoreAbilities;
            ScoreActivatables = scoreActivatables; ScoreBuffs = scoreBuffs;
            ShareTransmutation = share;
            ShareTransmutationAbility = shareAbility;
            ShareTransmutationBuff = shareBuff;
            TransmutationSupremacy = supremacy;
        }

        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature PowerfulChange { get; private set; }
        internal BlueprintAbility PowerfulChangeSelection { get; private set; }
        internal BlueprintAbility[] ScoreAbilities { get; private set; }
        internal BlueprintActivatableAbility[] ScoreActivatables
        { get; private set; }
        internal BlueprintBuff[] ScoreBuffs { get; private set; }
        internal BlueprintFeature ShareTransmutation { get; private set; }
        internal BlueprintActivatableAbility ShareTransmutationAbility
        { get; private set; }
        internal BlueprintBuff ShareTransmutationBuff { get; private set; }
        internal BlueprintFeature TransmutationSupremacy { get; private set; }
        internal int Count { get { return 25; } }
    }

    internal static class BrownFurBlueprints
    {
        internal const string DisplayName = "Brown-Fur Transmuter";
        private static readonly BrownFurAbilityScore[] Scores = {
            BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity,
            BrownFurAbilityScore.Constitution, BrownFurAbilityScore.Intelligence,
            BrownFurAbilityScore.Wisdom, BrownFurAbilityScore.Charisma };
        private static readonly string[] ScoreIconDonorGuids = {
            "4c3d08935262b6544ae97599b3a9556d",
            "de7a025d48ad5da4991e7d3c682cf69d",
            "a900628aea19aa74aad0ece0e65d091a",
            "ae4d3ad6a8fda1542acf2e9bbc13d113",
            "f0455c9295b53904f9e02fc571dd2ce1",
            "446f7bf201dc1934f96ac0a26e324803" };
        private const string ShareIconDonorGuid =
            "5d4028eb28a106d4691ed1b92bbb1915";

        internal static BrownFurBlueprintSet Register(BlueprintRegistry registry,
            CotwArcanistContract contract)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            ValidateContract(contract);
            Sprite featureIcon = contract.ExploitSelection.Icon ??
                contract.MagicalSupremacy.Icon;
            Sprite[] scoreIcons = ScoreIconDonorGuids.Select((guid, index) =>
                ResolveNativeIcon(guid, Scores[index].ToString())).ToArray();
            Sprite shareIcon = ResolveNativeIcon(ShareIconDonorGuid,
                "Share Transmutation");
            ValidateIconSet(featureIcon, scoreIcons, shareIcon,
                contract.MagicalSupremacy.Icon);
            BlueprintBuff[] scoreBuffs = Scores.Select(score =>
                registry.Register<BlueprintBuff>(
                    BrownFurIdentityCatalog.PowerfulBuff(score),
                    () => CreatePendingScoreBuff(score,
                        scoreIcons[Array.IndexOf(Scores, score)]))).ToArray();
            BlueprintAbility[] scoreAbilities = Scores.Select((score, index) =>
                registry.Register<BlueprintAbility>(
                    BrownFurIdentityCatalog.PowerfulAbility(score),
                    () => CreateScoreAbility(score, scoreBuffs[index],
                        scoreBuffs, scoreIcons[index]))).ToArray();
            BlueprintActivatableAbility[] scoreActivatables = Scores.Select(
                (score, index) => registry.Register<BlueprintActivatableAbility>(
                    BrownFurIdentityCatalog.PowerfulActivatable(score),
                    () => CreateScoreActivatable(score, scoreBuffs[index],
                        scoreIcons[index], contract.Reservoir))).ToArray();
            BlueprintAbility selection = registry.Register<BlueprintAbility>(
                BrownFurIdentityCatalog.PowerfulChangeSelection,
                () => CreateSelection(scoreAbilities, featureIcon));
            BlueprintFeature powerful = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.PowerfulChangeFeature,
                () => CreateFeature("Powerful Change",
                    "At 3rd level, a brown-fur transmuter can empower the physical and mental changes produced by her magic. Before casting, choose Strength, Dexterity, Constitution, Intelligence, Wisdom, or Charisma. While that score is armed, the next Transmutation spell cast from the brown-fur transmuter's Arcanist spellbook that grants a positive bonus to the chosen score can spend 1 point from her arcane reservoir to increase that spell's bonus to the chosen score by 2. At 20th level, the increase becomes 4.\n\nPowerful Change modifies the spell's original bonus and preserves its bonus type, so normal stacking rules still apply. Spells cast from items, spell-like abilities, supernatural abilities, and non-Arcanist spellbooks are not eligible. A spell that does not improve the chosen score does not spend a reservoir point or consume the selection. Only one ability score can be armed at a time.",
                    scoreActivatables.Cast<BlueprintUnitFact>().ToArray(),
                    featureIcon));
            BlueprintBuff shareBuff = registry.Register<BlueprintBuff>(
                BrownFurIdentityCatalog.ShareBuff,
                () => CreateMarkerBuff("Share Transmutation",
                    "Share Transmutation is armed for the next eligible Personal-range Transmutation spell.", shareIcon));
            BlueprintActivatableAbility shareAbility = registry.Register<
                BlueprintActivatableAbility>(
                    BrownFurIdentityCatalog.ShareActivatable,
                    () => CreateShareAbility(shareBuff, shareIcon,
                        contract.Reservoir));
            BlueprintFeature share = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.ShareFeature,
                () => CreateFeature("Share Transmutation",
                    "At 9th level, a brown-fur transmuter can spend 1 point from her arcane reservoir to cast a Personal-range Transmutation spell on a willing creature as a touch spell. While Share Transmutation is armed, clicking an eligible Personal-range Transmutation spell enters creature-target selection instead of immediately casting the spell on the brown-fur transmuter. At 20th level, the spell can instead target a willing creature within 30 feet.\n\nActivating or deactivating Share Transmutation costs nothing; the reservoir point is spent only when a qualifying spell is successfully cast. Canceling target selection spends no reservoir point and no spell slot. Item abilities, spell-like abilities, and supernatural abilities are not eligible.",
                    new BlueprintUnitFact[] { shareAbility }, shareIcon));
            BlueprintFeature supremacy = registry.Register<BlueprintFeature>(
                BrownFurIdentityCatalog.SupremacyFeature,
                () => CreateFeature("Transmutation Supremacy",
                    "Every genuine Transmutation spell you cast is Extended without changing its spell slot or casting time. A spell already affected by Extend is not extended again. Powerful Change increases bonuses by 4, and Share Transmutation reaches exactly 30 feet.",
                    null, contract.MagicalSupremacy.Icon ?? featureIcon));
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                BrownFurIdentityCatalog.Archetype,
                () => CreateArchetype(contract, powerful, share, supremacy));
            var set = new BrownFurBlueprintSet(archetype, powerful, selection,
                scoreAbilities, scoreActivatables, scoreBuffs, share,
                shareAbility, shareBuff, supremacy);
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
                "Pending Powerful Change toggle for " + score + ". This marker grants no statistical benefit by itself and is hidden from Effects and Conditions.", icon);
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
            HideBuffInUi(buff);
            return buff;
        }

        private static BlueprintAbility CreateScoreAbility(
            BrownFurAbilityScore score, BlueprintBuff selected,
            BlueprintBuff[] all, Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_BrownFur_PowerfulChange_" + score,
                "Powerful Change: " + score + " (Legacy)",
                ToggleDescription(score), icon);
            ability.ComponentsArray = new BlueprintComponent[] {
                BrownFurPowerfulChangeSelectionLogic.Create(selected, all) };
            ability.Hidden = true;
            ability.ActionBarAutoFillIgnored = true;
            return ability;
        }

        private static BlueprintActivatableAbility CreateScoreActivatable(
            BrownFurAbilityScore score, BlueprintBuff buff, Sprite icon,
            BlueprintAbilityResource reservoir)
        {
            var ability = CreateActivatable(
                "KMG_BrownFur_PowerfulChange_" + score + "_Activatable",
                "Powerful Change: " + score, ToggleDescription(score), buff,
                icon, reservoir);
            ability.Group = BrownFurActivatableGroupRuntime.PowerfulChangeGroup;
            return ability;
        }

        private static BlueprintAbility CreateSelection(
            BlueprintAbility[] choices, Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_BrownFur_PowerfulChange_Selection", "Powerful Change",
                "Legacy Powerful Change selector retained only for save compatibility. Use the six native Powerful Change score toggles granted by the class feature.", icon);
            var variants = ScriptableObject.CreateInstance<AbilityVariants>();
            variants.name = "$KMG_BrownFur_PowerfulChange_Variants";
            variants.Variants = (BlueprintAbility[])choices.Clone();
            ability.ComponentsArray = new BlueprintComponent[] { variants };
            ability.Hidden = true;
            ability.ActionBarAutoFillIgnored = true;
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
            BlueprintBuff buff, Sprite icon, BlueprintAbilityResource reservoir)
        {
            BlueprintActivatableAbility ability = CreateActivatable(
                "KMG_BrownFur_ShareTransmutation_Activatable",
                "Share Transmutation",
                "Arm Share Transmutation. Your next eligible Personal-range Transmutation spell becomes a targeted spell and costs 1 point from your arcane reservoir. Select a willing creature in touch range, or within 30 feet at 20th level. The spell does not cast until you select a valid target.\n\nThe toggle remains armed until a qualifying cast succeeds or you turn it off. Canceling target selection or attempting an ineligible spell spends nothing. Powerful Change can modify the same eligible Arcanist spell for a total cost of 2 reservoir points.",
                buff, icon, reservoir);
            ability.Group = ActivatableAbilityGroup.None;
            return ability;
        }

        private static BlueprintActivatableAbility CreateActivatable(
            string assetName, string displayName, string description,
            BlueprintBuff buff, Sprite icon, BlueprintAbilityResource reservoir)
        {
            if (buff == null || icon == null || reservoir == null)
                throw new ArgumentNullException();
            var ability = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
            ability.name = assetName;
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(assetName + ".Name", displayName),
                LocalizationService.Create(assetName + ".Description",
                    description), icon);
            ability.Buff = buff;
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
            var resource = ScriptableObject.CreateInstance<
                ActivatableAbilityResourceLogic>();
            resource.name = "$KMG_BrownFur_ArcaneReservoirUi";
            resource.RequiredResource = reservoir;
            resource.SpendType = ActivatableAbilityResourceLogic
                .ResourceSpendType.Never;
            ability.ComponentsArray = new BlueprintComponent[] { resource };
            ability.ResourceAssetIds = Array.Empty<string>();
            return ability;
        }

        private static BlueprintFeature CreateFeature(string name,
            string description, BlueprintUnitFact[] granted, Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_BrownFur_" + name.Replace(" ", string.Empty) +
                "_Feature";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            if (granted != null && granted.Length > 0)
            {
                var add = ScriptableObject.CreateInstance<AddFacts>();
                add.name = "$KMG_BrownFur_Grant_" + name.Replace(" ", string.Empty);
                add.Facts = (BlueprintUnitFact[])granted.Clone();
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

        private static string ToggleDescription(BrownFurAbilityScore score)
        {
            return "Arm Powerful Change for " + score + ". Your next eligible Transmutation spell cast from your Arcanist spellbook that grants a positive " + score + " bonus spends 1 point from your arcane reservoir and increases that spell's " + score + " bonus by 2, or by 4 at 20th level. The original bonus type is preserved.\n\nThe toggle remains armed until a qualifying cast succeeds or you turn it off. Ineligible or canceled casts spend nothing and do not consume the selection.";
        }

        private static Sprite ResolveNativeIcon(string guid, string purpose)
        {
            BlueprintAbility donor = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>(guid);
            if (donor == null || donor.Icon == null)
                throw new InvalidOperationException("Native Brown-Fur icon donor is unavailable: " + purpose + ";guid=" + guid);
            return donor.Icon;
        }

        private static void ValidateIconSet(Sprite featureIcon,
            Sprite[] scoreIcons, Sprite shareIcon, Sprite supremacyIcon)
        {
            Sprite[] all = new[] { featureIcon }.Concat(scoreIcons ??
                new Sprite[0]).Concat(new[] { shareIcon, supremacyIcon }).ToArray();
            if (all.Length != 9 || all.Any(value => value == null) ||
                all.Distinct().Count() != all.Length)
                throw new InvalidOperationException(
                    "Brown-Fur requires nine distinct readable native icons.");
        }

        private static void HideBuffInUi(BlueprintBuff buff)
        {
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            if (flags == null || !flags.FieldType.IsEnum)
                throw new MissingFieldException(typeof(BlueprintBuff).FullName,
                    "m_Flags");
            flags.SetValue(buff, Enum.Parse(flags.FieldType, "HiddenInUi"));
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
                set.ScoreActivatables == null ||
                set.ScoreActivatables.Length != 6 ||
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
            if (set.ScoreAbilities.Any(value => value == null || !value.Hidden ||
                    !value.ActionBarAutoFillIgnored) ||
                set.ScoreActivatables.Any(value => value == null ||
                    value.Group != BrownFurActivatableGroupRuntime
                        .PowerfulChangeGroup || value.WeightInGroup != 1 ||
                    value.IsOnByDefault || value.OnlyInCombat ||
                    value.ActivationType != AbilityActivationType.Immediately ||
                    value.ActionBarAutoFillIgnored ||
                    !value.ComponentsArray.OfType<
                        ActivatableAbilityResourceLogic>().Any(component =>
                            ReferenceEquals(component.RequiredResource,
                                contract.Reservoir) && component.SpendType ==
                                ActivatableAbilityResourceLogic.ResourceSpendType
                                    .Never)) ||
                set.ShareTransmutationAbility == null ||
                !set.ShareTransmutationAbility.ComponentsArray.OfType<
                    ActivatableAbilityResourceLogic>().Any(component =>
                        ReferenceEquals(component.RequiredResource,
                            contract.Reservoir) && component.SpendType ==
                            ActivatableAbilityResourceLogic.ResourceSpendType.Never))
                throw new InvalidOperationException(
                    "Brown-Fur activatable and shared-reservoir UI contract is incomplete.");
        }
    }
}
