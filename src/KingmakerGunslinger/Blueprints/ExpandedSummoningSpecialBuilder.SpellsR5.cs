using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string CloudMistSymbolR5 = "KMG.Summoning.Giant.Sprint29.CloudObscuringMist";
        internal const string CloudMistAiSymbolR5 = "KMG.Summoning.Giant.Sprint29.CloudMistAi";
        internal const string CloudMistSafetySymbolR5 = "KMG.Summoning.Giant.Sprint29.CloudFogSafety";

        private static ContextCalculateAbilityParams SpellLikeParamsR5(int casterLevel, int spellLevel)
        {
            var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(casterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(spellLevel);
            return parameters;
        }

        private static AbilityResourceLogic OneUseCostR5(BlueprintAbilityResource resource)
        {
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            return cost;
        }

        /// <summary>
        /// Repair R5 SPELL-01: the Cloud Giant's single Obscuring Mist
        /// spell-like ability, an owned wrapper that stays hidden and unusable
        /// until Call of the Wild's actual Obscuring Mist binds after load.
        /// </summary>
        private static void ConfigureCloudMistPlaceholderR5(BlueprintAbility mist, BlueprintUnit unit)
        {
            mist.name = InternalName(CloudMistSymbolR5);
            mist.Type = AbilityType.SpellLike;
            mist.Hidden = true;
            mist.Range = AbilityRange.Medium;
            mist.CanTargetPoint = true;
            mist.CanTargetEnemies = true;
            mist.CanTargetSelf = false;
            mist.CanTargetFriends = false;
            var guard = ScriptableObject.CreateInstance<R5CotwSpellAvailable>();
            guard.OwningBlueprint = unit;
            guard.Dance = false;
            mist.ComponentsArray = new BlueprintComponent[] { guard };
            BlueprintUnitFactAccess.Resolve().Configure(mist,
                LocalizationService.Create(CloudMistSymbolR5 + ".R5.Name", "Obscuring Mist"),
                LocalizationService.Create(CloudMistSymbolR5 + ".R5.Description",
                    "Available only with active Call of the Wild, whose Obscuring Mist this uses."), null);
        }

        internal static void BindCotwObscuringMistR5(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintAbility source = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                SummonPlaytestR5Policy.CotwObscuringMistGuid, "Call of the Wild Obscuring Mist");
            BlueprintAbilityAreaEffect area = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                SummonPlaytestR5Policy.CotwObscuringMistAreaGuid, "Call of the Wild Obscuring Mist area");
            BlueprintComponent[] sourceComponents = source.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            AbilityEffectRunAction[] runs = sourceComponents.OfType<AbilityEffectRunAction>().ToArray();
            ContextActionSpawnAreaEffect[] spawns = runs.Length == 1 && runs[0].Actions != null && runs[0].Actions.Actions != null ?
                runs[0].Actions.Actions.OfType<ContextActionSpawnAreaEffect>().ToArray() : new ContextActionSpawnAreaEffect[0];
            if (source.name != SummonPlaytestR5Policy.CotwObscuringMistName || source.Type != AbilityType.Spell ||
                source.Range != AbilityRange.Medium || !source.CanTargetPoint ||
                source.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard ||
                runs.Length != 1 || spawns.Length != 1 || !ReferenceEquals(spawns[0].AreaEffect, area) || spawns[0].OnUnit)
                throw new InvalidOperationException("Call of the Wild Obscuring Mist no longer matches the investigated target-point contract.");
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.CloudGiant");
            BlueprintAbility mist = Require<BlueprintAbility>(bySymbol, CloudMistSymbolR5);
            ExpandedSummoningAbilityBuilder.CopyFields(source, mist);
            mist.name = InternalName(CloudMistSymbolR5);
            mist.Type = AbilityType.SpellLike;
            mist.Hidden = false;
            // The native AI selects an Enemy target type only with this flag;
            // the stationary point area stays the provider's own.
            mist.CanTargetEnemies = true;
            var guard = ScriptableObject.CreateInstance<R5CotwSpellAvailable>();
            guard.OwningBlueprint = unit;
            mist.ComponentsArray = sourceComponents.Where(value => value != null && !(value is SpellListComponent))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { guard,
                    SpellLikeParamsR5(SummonPlaytestR5Policy.CloudMistCasterLevel, SummonPlaytestR5Policy.CloudMistSpellLevel) })
                .ToArray();
            AlternateSummoningAdapterBuilder.ResetComponentCaches(mist);
            Sprint29FogAiSafety safety = Require<Sprint29FogAiSafety>(bySymbol, CloudMistSafetySymbolR5);
            safety.OwningBlueprint = unit;
            safety.FogArea = area;
            safety.CenterOnCaster = false;
        }

        /// <summary>
        /// Repair R5 SPELL-03: the Pixie's Irresistible Dance, an owned
        /// once-per-summoning spell-like wrapper that stays hidden and unusable
        /// until Call of the Wild's actual touch spell binds after load.
        /// </summary>
        private static void ConfigurePixieDancePlaceholderR5(BlueprintAbility dance, BlueprintAbilityResource resource,
            BlueprintUnit unit)
        {
            dance.name = InternalName(PixieDanceSymbol);
            dance.Type = AbilityType.SpellLike;
            dance.Hidden = true;
            dance.Range = AbilityRange.Touch;
            dance.CanTargetEnemies = true;
            dance.CanTargetSelf = false;
            dance.CanTargetFriends = false;
            dance.CanTargetPoint = false;
            var guard = ScriptableObject.CreateInstance<R5CotwSpellAvailable>();
            guard.OwningBlueprint = unit;
            guard.Dance = true;
            dance.ComponentsArray = new BlueprintComponent[] { guard, OneUseCostR5(resource) };
            BlueprintUnitFactAccess.Resolve().Configure(dance,
                LocalizationService.Create("KMG.ExpandedSummoning.Pixie.IrresistibleDance.R5.Name", "Irresistible Dance"),
                LocalizationService.Create("KMG.ExpandedSummoning.Pixie.IrresistibleDance.R5.Description",
                    "Available only with active Call of the Wild, whose Irresistible Dance this uses. Once per summoning."), null);
        }

        internal static void BindCotwIrresistibleDanceR5(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintAbility cast = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                SummonPlaytestR5Policy.CotwIrresistibleDanceCastGuid, "Call of the Wild Irresistible Dance cast");
            BlueprintAbility touch = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                SummonPlaytestR5Policy.CotwIrresistibleDanceGuid, "Call of the Wild Irresistible Dance touch");
            BlueprintComponent[] castComponents = cast.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            AbilityEffectStickyTouch[] sticky = castComponents.OfType<AbilityEffectStickyTouch>().ToArray();
            AbilityEffectRunAction[] touchRuns = (touch.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityEffectRunAction>().ToArray();
            if (cast.name != SummonPlaytestR5Policy.CotwIrresistibleDanceCastName || cast.Type != AbilityType.Spell ||
                cast.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard ||
                sticky.Length != 1 || !ReferenceEquals(sticky[0].TouchDeliveryAbility, touch) ||
                touch.GetComponent<AbilityDeliverTouch>() == null || touchRuns.Length != 1 ||
                touchRuns[0].SavingThrowType != SavingThrowType.Will)
                throw new InvalidOperationException("Call of the Wild Irresistible Dance no longer matches the investigated touch-cast contract.");
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, PixieUnitSymbol);
            BlueprintAbility dance = Require<BlueprintAbility>(bySymbol, PixieDanceSymbol);
            BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(bySymbol, PixieDanceResourceSymbol);
            ExpandedSummoningAbilityBuilder.CopyFields(cast, dance);
            dance.name = InternalName(PixieDanceSymbol);
            dance.Type = AbilityType.SpellLike;
            dance.Hidden = false;
            var guard = ScriptableObject.CreateInstance<R5CotwSpellAvailable>();
            guard.OwningBlueprint = unit;
            guard.Dance = true;
            // The sticky touch hands this cast's own ability data (caster
            // level, DC) to the provider's delivered touch spell; the shared
            // provider blueprints are referenced, never written.
            dance.ComponentsArray = castComponents.Where(value => value != null && !(value is SpellListComponent))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { guard, OneUseCostR5(resource),
                    SpellLikeParamsR5(ExpandedSummoningSpecialProfiles.PixieDanceCasterLevel,
                        ExpandedSummoningSpecialProfiles.PixieDanceSpellLevel) })
                .ToArray();
            AlternateSummoningAdapterBuilder.ResetComponentCaches(dance);
        }

        /// <summary>
        /// Repair R5 SPELL-02 (owner adaptation of the printed daily uses and
        /// Lesser Confusion): Entangle, Confusion, Dispel Magic and Shield at
        /// will, each an owned spell-like clone of the installed native spell
        /// (its own action cost, targeting, saves and effects) at the Pixie's
        /// caster level, granted through the unit's own ability component and
        /// cast by the owned brain with native or bounded AI actions.
        /// </summary>
        private static void ConfigurePixieSpellLikesR5(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, PixieUnitSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, PixieBrainSymbol);
            var granted = new List<BlueprintAbility>();
            var actions = new List<BlueprintAiAction>();
            foreach (PixieSpellLikeSpec spec in SummonPlaytestR5Policy.PixieAtWill)
            {
                BlueprintAbility ability = Require<BlueprintAbility>(bySymbol, spec.Symbol);
                CloneMephitSpell(library, ability, spec.NativeGuid, "native " + spec.DisplayName);
                ConfigurePixieSpellLikeR5(ability, spec.Symbol, spec.SpellLevel);
                var owned = new Dictionary<string, BlueprintAbility>(StringComparer.Ordinal);
                string[] variantSymbols = spec.VariantSymbols.ToArray();
                for (int index = 0; index < variantSymbols.Length; index++)
                {
                    BlueprintAbility variant = Require<BlueprintAbility>(bySymbol, variantSymbols[index]);
                    CloneMephitSpell(library, variant, spec.NativeVariantGuids[index], "native " + spec.DisplayName + " variant");
                    ConfigurePixieSpellLikeR5(variant, variantSymbols[index], spec.SpellLevel);
                    variant.Parent = ability;
                    owned[spec.NativeVariantGuids[index]] = variant;
                }
                AbilityVariants[] variantComponents = ability.ComponentsArray.OfType<AbilityVariants>().ToArray();
                if (variantSymbols.Length > 0)
                {
                    if (variantComponents.Length != 1 || variantComponents[0].Variants == null ||
                        variantComponents[0].Variants.Length != variantSymbols.Length ||
                        variantComponents[0].Variants.Any(value => value == null || !owned.ContainsKey(value.AssetGuid)))
                        throw new InvalidOperationException("Native " + spec.DisplayName + " variants changed.");
                    variantComponents[0].Variants = variantComponents[0].Variants.Select(value => owned[value.AssetGuid]).ToArray();
                }
                else if (variantComponents.Length != 0)
                    throw new InvalidOperationException("Native " + spec.DisplayName + " unexpectedly has variants.");
                granted.Add(ability);

                BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol, spec.AiSymbol);
                if (spec.NativeAiGuid != null)
                {
                    BlueprintAiCastSpell native = BlueprintLibraryLookup.RequireExact<BlueprintAiCastSpell>(library,
                        spec.NativeAiGuid, "native " + spec.DisplayName + " AI action");
                    if (native.Ability == null || native.Ability.AssetGuid != spec.NativeGuid)
                        throw new InvalidOperationException("Native " + spec.DisplayName + " AI action no longer casts the native spell.");
                    CopyFields(native, ai);
                    ai.name = InternalName(spec.AiSymbol);
                    ai.Ability = ability;
                    if (native.Variant != null)
                    {
                        BlueprintAbility variant;
                        if (!owned.TryGetValue(native.Variant.AssetGuid, out variant))
                            throw new InvalidOperationException("Native " + spec.DisplayName + " AI variant is not an owned variant.");
                        ai.Variant = variant;
                    }
                }
                else
                    ConfigureMephitAiAction(ai, spec.AiSymbol, ability, spec.OwnedAiCooldownRounds);
                actions.Add(ai);
            }
            AddAbilityToCharacterComponent grant = (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AddAbilityToCharacterComponent>().Single();
            grant.Abilities = grant.Abilities.Concat(granted).ToArray();
            brain.Actions = (brain.Actions ?? Array.Empty<BlueprintAiAction>()).Concat(actions).ToArray();
        }

        private static void ConfigurePixieSpellLikeR5(BlueprintAbility ability, string symbol, int spellLevel)
        {
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            ability.ComponentsArray = ability.ComponentsArray.Concat(new BlueprintComponent[] {
                SpellLikeParamsR5(SummonPlaytestR5Policy.PixieCasterLevel, spellLevel) }).ToArray();
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
        }
    }
}
