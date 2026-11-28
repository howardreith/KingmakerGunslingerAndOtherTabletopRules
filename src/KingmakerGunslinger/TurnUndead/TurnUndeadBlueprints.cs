using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.TurnUndead
{
    internal sealed class TurnUndeadSourceBlueprints
    {
        internal TurnUndeadSourceBlueprints(TurnUndeadSourceContract contract,
            BlueprintFeature sourceFeature, BlueprintAbility nativeHarm,
            BlueprintAbility direct, BlueprintAbility menuChild)
        {
            Contract = contract;
            SourceFeature = sourceFeature;
            NativeHarm = nativeHarm;
            Direct = direct;
            MenuChild = menuChild;
        }

        internal TurnUndeadSourceContract Contract { get; private set; }
        internal BlueprintFeature SourceFeature { get; private set; }
        internal BlueprintAbility NativeHarm { get; private set; }
        internal BlueprintAbility Direct { get; private set; }
        internal BlueprintAbility MenuChild { get; private set; }
    }

    internal sealed class TurnUndeadBlueprintSet
    {
        internal TurnUndeadBlueprintSet(BlueprintFeature feat, BlueprintBuff fleeing,
            TurnUndeadSourceBlueprints[] sources)
        {
            Feat = feat;
            Fleeing = fleeing;
            Sources = sources;
        }

        internal BlueprintFeature Feat { get; private set; }
        internal BlueprintBuff Fleeing { get; private set; }
        internal TurnUndeadSourceBlueprints[] Sources { get; private set; }

        internal IEnumerable<BlueprintAbility> AllAbilities
        { get { return Sources.SelectMany(value => new[] { value.Direct, value.MenuChild }); } }
    }

    /// <summary>
    /// Private Turn Undead definitions. Native donors (each source's
    /// harm-undead channel and the shared FrightenedUndead buff) are read and
    /// cloned, never edited; every kept component is a fresh instance so no
    /// native ScriptableObject is shared or mutated.
    /// </summary>
    internal static class TurnUndeadBlueprints
    {
        internal const string FeatIconKey = "turn-undead-feat";
        internal const string ActionIconKey = "turn-undead-action";

        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        internal static TurnUndeadBlueprintSet Register(LibraryScriptableObject library,
            BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            Sprite featIcon = ProjectAssetIcons.RequireIcon(FeatIconKey);
            Sprite actionIcon = ProjectAssetIcons.RequireIcon(ActionIconKey);
            BlueprintFeature undeadType = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, TurnUndeadSourceCatalog.UndeadTypeGuid, "native UndeadType");
            BlueprintBuff nativeFlight = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, TurnUndeadSourceCatalog.NativeFrightenedUndeadGuid,
                "native FrightenedUndead buff");
            ValidateNativeFlight(nativeFlight);

            BlueprintBuff fleeing = registry.Register<BlueprintBuff>(
                TurnUndeadSourceCatalog.FleeingBuffSymbol,
                () => CreateFleeingBuff(nativeFlight));

            // The feat is registered before the abilities that check for it.
            // Its grants are attached once every ability exists.
            BlueprintFeature feat = registry.Register<BlueprintFeature>(
                TurnUndeadSourceCatalog.FeatSymbol, () => CreateFeat(featIcon));

            var sources = new List<TurnUndeadSourceBlueprints>();
            foreach (TurnUndeadSourceContract contract in TurnUndeadSourceCatalog.All)
            {
                BlueprintFeature sourceFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    library, contract.SourceFeatureGuid, "native " + contract.DisplayName +
                        " positive-channel feature");
                BlueprintAbility harm = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, contract.NativeHarmGuid, "native " + contract.DisplayName +
                        " positive harm-undead channel");
                BlueprintAbilityResource resource = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbilityResource>(library, contract.ResourceGuid, "native " +
                        contract.DisplayName + " channel resource");
                BlueprintCharacterClass dcClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(library, contract.DcClassGuid, contract.DcClassName +
                        " class");
                ValidateNativeSource(contract, harm, resource);
                BlueprintAbility direct = registry.Register<BlueprintAbility>(
                    contract.DirectAbilitySymbol, () => CreateAbility(contract, harm, resource,
                        dcClass, sourceFeature, feat, undeadType, fleeing, actionIcon, false));
                BlueprintAbility child = registry.Register<BlueprintAbility>(
                    contract.MenuChildSymbol, () => CreateAbility(contract, harm, resource,
                        dcClass, sourceFeature, feat, undeadType, fleeing, actionIcon, true));
                sources.Add(new TurnUndeadSourceBlueprints(contract, sourceFeature, harm,
                    direct, child));
            }

            AttachFeatComponents(feat, sources);
            var set = new TurnUndeadBlueprintSet(feat, fleeing, sources.ToArray());
            Validate(set, featIcon, actionIcon, undeadType);
            return set;
        }

        private static BlueprintBuff CreateFleeingBuff(BlueprintBuff nativeFlight)
        {
            // The clone keeps the native buff's empty FX links, stacking policy,
            // presentation flags and its native Fear icon; only the logic changes.
            BlueprintBuff result = BlueprintCloneService.Clone(nativeFlight,
                "KMG_TurnUndead_Fleeing");
            var state = ScriptableObject.CreateInstance<TurnUndeadFleeingState>();
            state.name = TurnUndeadFleeingState.ComponentName;
            result.ComponentsArray = new BlueprintComponent[] { state };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TurnUndead.Fleeing.Name", "Turned"),
                LocalizationService.Create("KMG.TurnUndead.Fleeing.Description",
                    "This undead creature was turned by channeled positive energy and flees in panic. Intelligent undead attempt a new Will save each round to end the effect; mindless undead do not. The effect lasts 1 minute."),
                nativeFlight.Icon);
            return result;
        }

        private static BlueprintFeature CreateFeat(Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_TurnUndead_Feat";
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Feat };
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feat.TurnUndead.Name", "Turn Undead"),
                LocalizationService.Create("KMG.Feat.TurnUndead.Description",
                    "Prerequisite: Channel positive energy class feature.\n" +
                    "Benefit: As a standard action, you can use one of your uses of channel positive energy to cause all undead within 30 feet of you to flee, as if panicked. Undead receive a Will save to negate the effect. The DC for this Will save is equal to 10 + 1/2 your cleric level + your Charisma modifier. Undead that fail their save flee for 1 minute. Intelligent undead receive a new saving throw each round to end the effect. If you use channel energy in this way, it has no other effect (it does not heal or harm nearby creatures).\n" +
                    "Each positive channel you have gains its own Turn Undead action that spends that channel's uses: a paladin spends two uses of lay on hands, while a cleric, hospitaler or empyreal sorcerer spends one use of that channel. The DC uses that source's own class level and channel statistic (Wisdom for an empyreal sorcerer). In Kingmaker, panic is represented by the game's frightened fleeing condition; affected undead do not drop what they are holding."),
                icon);
            return feature;
        }

        private static void AttachFeatComponents(BlueprintFeature feat,
            List<TurnUndeadSourceBlueprints> sources)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFeaturesFromList>();
            prerequisite.name = "$KMG_TurnUndead_Prerequisite";
            prerequisite.Features = sources.Select(value => value.SourceFeature).ToArray();
            prerequisite.Amount = 1;
            prerequisite.Group = Prerequisite.GroupType.All;
            var components = new List<BlueprintComponent> { prerequisite };
            foreach (TurnUndeadSourceBlueprints source in sources)
            {
                // Grants only the action of a source the character actually has;
                // re-checked natively at every level-up, so a later second
                // source adds its own action. Never creates or refills a pool.
                var grant = ScriptableObject.CreateInstance<AddFeatureIfHasFact>();
                grant.name = "$KMG_TurnUndead_Grant_" + source.Contract.DisplayName;
                grant.CheckedFact = source.SourceFeature;
                grant.Feature = source.Direct;
                grant.Not = false;
                components.Add(grant);
            }
            feat.ComponentsArray = components.ToArray();
        }

        private static BlueprintAbility CreateAbility(TurnUndeadSourceContract contract,
            BlueprintAbility harm, BlueprintAbilityResource resource,
            BlueprintCharacterClass dcClass, BlueprintFeature sourceFeature,
            BlueprintFeature feat, BlueprintFeature undeadType, BlueprintBuff fleeing,
            Sprite icon, bool menuChild)
        {
            string stem = "KMG_TurnUndead_" + contract.DisplayName +
                (menuChild ? "_MenuChild" : "_Ability");
            BlueprintAbility result = BlueprintCloneService.Clone(harm, stem);
            string statName = contract.DcStat == TurnUndeadDcStat.Wisdom ? "Wisdom" : "Charisma";
            string cost = contract.ResourceCost == 1 ? "one use" : "two uses";
            string pool = contract.Kind == TurnUndeadSourceKind.Paladin ?
                "of lay on hands" : "of channel positive energy";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TurnUndead." + contract.DisplayName + ".Name",
                    "Turn Undead (" + contract.DisplayName + ")"),
                LocalizationService.Create("KMG.TurnUndead." + contract.DisplayName + ".Description",
                    "As a standard action, spend " + cost + " " + pool +
                    " to cause all undead within 30 feet of you to flee for 1 minute. Each undead creature, including an allied one, can attempt a Will save to negate the effect (DC 10 + 1/2 your " +
                    contract.DcClassName + " level + your " + statName +
                    " modifier). Intelligent undead attempt a new save each round to end the effect; mindless undead do not. This channel neither heals nor harms."),
                icon);
            result.Type = AbilityType.Supernatural;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.AvailableMetamagic = 0;
            result.Parent = null;
            result.Hidden = false;
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.TurnUndead.Duration", "1 minute");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.TurnUndead.SavingThrow", "Will negates");
            if (result.MaterialComponent == null)
                result.MaterialComponent = new BlueprintAbility.MaterialComponentData();

            BlueprintComponent[] native = harm.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            var components = new List<BlueprintComponent>();

            var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParamsBasedOnClass>();
            parameters.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Dc";
            parameters.CharacterClass = dcClass;
            parameters.StatType = contract.DcStat == TurnUndeadDcStat.Wisdom ?
                StatType.Wisdom : StatType.Charisma;
            parameters.UseKineticistMainStat = false;
            components.Add(parameters);

            var resourceLogic = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            resourceLogic.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Resource";
            resourceLogic.RequiredResource = resource;
            resourceLogic.IsSpendResource = true;
            resourceLogic.Amount = contract.ResourceCost;
            resourceLogic.CostIsCustom = false;
            resourceLogic.ResourceCostIncreasingFacts = new List<BlueprintUnitFact>();
            components.Add(resourceLogic);

            AbilityTargetsAround around = native.OfType<AbilityTargetsAround>().Single();
            AbilityTargetsAround targets = UnityEngine.Object.Instantiate(around);
            targets.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Targets";
            SetField(targets, "m_Condition", new ConditionsChecker
            {
                Operation = Operation.And,
                Conditions = new Condition[]
                {
                    // Every undead within the native 30-foot burst, the channeler
                    // included when it is itself undead (the native targeting
                    // includes the caster); no other selection exception exists.
                    new ContextConditionHasFact { Fact = undeadType, Not = false },
                }
            });
            components.Add(targets);

            var apply = new ContextActionApplyBuff
            {
                Buff = fleeing,
                DurationValue = new ContextDurationValue
                {
                    Rate = DurationRate.Minutes,
                    DiceType = Kingmaker.RuleSystem.DiceType.Zero,
                    DiceCountValue = 0,
                    BonusValue = TurnUndeadRules.DurationMinutes
                },
                IsFromSpell = false,
                IsNotDispelable = true,
                AsChild = true
            };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Effect";
            effect.SavingThrowType = SavingThrowType.Will;
            effect.Actions = new ActionList
            {
                Actions = new GameAction[]
                {
                    new ContextActionConditionalSaved
                    {
                        Succeed = new ActionList { Actions = Array.Empty<GameAction>() },
                        Failed = new ActionList { Actions = new GameAction[] { apply } }
                    }
                }
            };
            components.Add(effect);

            foreach (AbilitySpawnFx fx in native.OfType<AbilitySpawnFx>())
            {
                AbilitySpawnFx copy = UnityEngine.Object.Instantiate(fx);
                copy.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Fx";
                components.Add(copy);
            }
            foreach (AbilityCasterAlignment alignment in native.OfType<AbilityCasterAlignment>())
            {
                AbilityCasterAlignment copy = UnityEngine.Object.Instantiate(alignment);
                copy.name = "$KMG_TurnUndead_" + contract.DisplayName + "_Alignment";
                components.Add(copy);
            }

            // Execution requires both the feat and this exact source. Two
            // single-fact checkers: the native NeedsAll mode only tests the last.
            components.Add(CasterHas("$KMG_TurnUndead_" + contract.DisplayName + "_RequiresFeat", feat));
            components.Add(CasterHas("$KMG_TurnUndead_" + contract.DisplayName + "_RequiresSource",
                sourceFeature));
            if (menuChild)
            {
                // Variant menus show a child only when both facts are present.
                components.Add(ShowIf("$KMG_TurnUndead_" + contract.DisplayName + "_ShowIfFeat", feat));
                components.Add(ShowIf("$KMG_TurnUndead_" + contract.DisplayName + "_ShowIfSource",
                    sourceFeature));
            }
            result.ComponentsArray = components.ToArray();
            return result;
        }

        private static AbilityCasterHasFacts CasterHas(string name, BlueprintUnitFact fact)
        {
            var checker = ScriptableObject.CreateInstance<AbilityCasterHasFacts>();
            checker.name = name;
            checker.Facts = new[] { fact };
            checker.NeedsAll = false;
            return checker;
        }

        private static AbilityShowIfCasterHasFact ShowIf(string name, BlueprintUnitFact fact)
        {
            var show = ScriptableObject.CreateInstance<AbilityShowIfCasterHasFact>();
            show.name = name;
            show.UnitFact = fact;
            return show;
        }

        private static void ValidateNativeFlight(BlueprintBuff nativeFlight)
        {
            if (nativeFlight.Icon == null)
                throw new InvalidOperationException("Native FrightenedUndead has no icon.");
        }

        private static void ValidateNativeSource(TurnUndeadSourceContract contract,
            BlueprintAbility harm, BlueprintAbilityResource resource)
        {
            AbilityResourceLogic[] logic = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityResourceLogic>().ToArray();
            if (logic.Length != 1 || !ReferenceEquals(logic[0].RequiredResource, resource) ||
                !logic[0].IsSpendResource || logic[0].Amount != contract.ResourceCost)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel no longer spends " + contract.ResourceCost +
                    " of resource " + contract.ResourceGuid + ".");
            if (harm.ActionType != UnitCommand.CommandType.Standard)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel is no longer a standard action.");
            if ((harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityTargetsAround>().Count() != 1)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel no longer has one burst targeting component.");
            bool alignment = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityCasterAlignment>().Any();
            if (alignment != contract.RequiresLawfulGood)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel's alignment restriction changed.");
        }

        private static void Validate(TurnUndeadBlueprintSet set, Sprite featIcon,
            Sprite actionIcon, BlueprintFeature undeadType)
        {
            BlueprintFeature feat = set.Feat;
            if (feat.Ranks != 1 || feat.HideInUI || feat.IsClassFeature ||
                feat.Groups == null || !feat.Groups.SequenceEqual(new[] { FeatureGroup.Feat }) ||
                !ReferenceEquals(feat.Icon, featIcon))
                throw new InvalidOperationException("Turn Undead must remain a visible rank-one general feat with its feat icon.");
            PrerequisiteFeaturesFromList prerequisite = feat.ComponentsArray
                .OfType<PrerequisiteFeaturesFromList>().Single();
            if (prerequisite.Amount != 1 || !prerequisite.Features.SequenceEqual(
                    set.Sources.Select(value => value.SourceFeature)))
                throw new InvalidOperationException("Turn Undead prerequisite must be exactly one supported positive channel.");
            AddFeatureIfHasFact[] grants = feat.ComponentsArray.OfType<AddFeatureIfHasFact>().ToArray();
            if (grants.Length != set.Sources.Length || set.Sources.Any(source =>
                    grants.Count(grant => ReferenceEquals(grant.CheckedFact, source.SourceFeature) &&
                        ReferenceEquals(grant.Feature, source.Direct) && !grant.Not) != 1))
                throw new InvalidOperationException("Turn Undead must grant each source's direct action only with that source.");
            if (set.Fleeing.ComponentsArray.Length != 1 ||
                !(set.Fleeing.ComponentsArray[0] is TurnUndeadFleeingState) ||
                set.Fleeing.ComponentsArray[0].name != TurnUndeadFleeingState.ComponentName)
                throw new InvalidOperationException("Turn Undead flight must carry only its owned state component.");
            foreach (TurnUndeadSourceBlueprints source in set.Sources)
                foreach (BlueprintAbility ability in new[] { source.Direct, source.MenuChild })
                {
                    AbilityResourceLogic resource = ability.ComponentsArray.OfType<AbilityResourceLogic>().Single();
                    ContextCalculateAbilityParamsBasedOnClass dc = ability.ComponentsArray
                        .OfType<ContextCalculateAbilityParamsBasedOnClass>().Single();
                    AbilityEffectRunAction effect = ability.ComponentsArray.OfType<AbilityEffectRunAction>().Single();
                    ContextActionApplyBuff apply = effect.Actions.Actions
                        .OfType<ContextActionConditionalSaved>().Single().Failed.Actions
                        .OfType<ContextActionApplyBuff>().Single();
                    if (ability.Type != AbilityType.Supernatural ||
                        ability.ActionType != UnitCommand.CommandType.Standard ||
                        ability.AvailableMetamagic != 0 || ability.Parent != null ||
                        !ReferenceEquals(ability.Icon, actionIcon) ||
                        resource.RequiredResource == null ||
                        resource.RequiredResource.AssetGuid != source.Contract.ResourceGuid ||
                        resource.Amount != source.Contract.ResourceCost || !resource.IsSpendResource ||
                        dc.CharacterClass == null || dc.CharacterClass.AssetGuid != source.Contract.DcClassGuid ||
                        effect.SavingThrowType != SavingThrowType.Will ||
                        !ReferenceEquals(apply.Buff, set.Fleeing) ||
                        apply.DurationValue.Rate != DurationRate.Minutes ||
                        ability.ComponentsArray.OfType<ContextRankConfig>().Any() ||
                        ability.ComponentsArray.OfType<AbilityCasterHasFacts>().Count() != 2 ||
                        ability.ComponentsArray.Intersect(source.NativeHarm.ComponentsArray).Any() ||
                        ability.ComponentsArray.Select(value => value.name).Distinct().Count() !=
                            ability.ComponentsArray.Length)
                        throw new InvalidOperationException("Turn Undead (" +
                            source.Contract.DisplayName + ") ability contract is incomplete.");
                }
        }

        private static void SetField(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, PrivateInstance);
            if (field == null) throw new MissingFieldException(instance.GetType().FullName, name);
            field.SetValue(instance, value);
        }
    }
}
