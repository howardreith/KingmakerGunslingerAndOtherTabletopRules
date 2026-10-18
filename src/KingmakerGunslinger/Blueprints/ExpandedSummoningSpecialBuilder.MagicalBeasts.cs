using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static void ConfigureSprint26Specials(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            string prefix = Sprint26MagicalBeastRules.GorgonPrefix;
            var unit = Require<BlueprintUnit>(bySymbol, Sprint26MagicalBeastRules.GorgonUnit);
            var breath = Require<BlueprintAbility>(bySymbol, prefix + "Breath");
            var stone = Require<BlueprintBuff>(bySymbol, prefix + "Petrification");
            var cooldown = Require<BlueprintBuff>(bySymbol, prefix + "Cooldown");
            var nativeStone = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                Sprint26MagicalBeastRules.PetrifiedDonor, "native petrification and cure descriptor");
            CopyFields(nativeStone, stone);
            stone.name = InternalName(prefix + "Petrification");
            stone.IsClassFeature = false;
            stone.Stacking = StackingType.Ignore;
            var condition = (BuffStatusCondition)ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                nativeStone.GetComponent<BuffStatusCondition>());
            condition.SaveEachRound = false;
            var descriptor = (SpellDescriptorComponent)ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                nativeStone.GetComponent<SpellDescriptorComponent>());
            stone.ComponentsArray = new BlueprintComponent[] { condition, descriptor,
                ScriptableObject.CreateInstance<GorgonPetrificationState>() };
            DescribeMagicalBeast(stone, prefix + "Petrification", "Gorgon Petrification",
                "Temporary stone permits a Fortitude save each round at the original breath DC. Another distinct gorgon breath while still petrified makes it permanent, without natural recovery saves. Stone to Flesh can remove it.", nativeStone.Icon);
            cooldown.name = InternalName(prefix + "Cooldown");
            cooldown.IsClassFeature = true;
            cooldown.Stacking = StackingType.Replace;
            cooldown.ComponentsArray = Array.Empty<BlueprintComponent>();
            SetBuffFlags(cooldown, false, true);
            DescribeMagicalBeast(cooldown, prefix + "Cooldown", "Breath Recovering",
                "This gorgon can breathe again after its rolled 1d4+1 rounds.", null);

            CloneMephitSpell(library, breath, Sprint26MagicalBeastRules.ConeDonor,
                "native blocked cone delivery");
            breath.name = InternalName(prefix + "Breath");
            breath.Type = AbilityType.Supernatural;
            breath.Range = AbilityRange.Projectile;
            breath.Parent = null;
            breath.Hidden = false;
            breath.ActionBarAutoFillIgnored = false;
            breath.SpellResistance = false;
            breath.CanTargetEnemies = true;
            breath.CanTargetFriends = false;
            breath.CanTargetSelf = false;
            breath.CanTargetPoint = true;
            breath.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            breath.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            breath.ActionType = UnitCommand.CommandType.Standard;
            breath.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            breath.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            breath.ResourceAssetIds = Array.Empty<string>();
            var delivery = breath.GetComponent<AbilityDeliverProjectile>();
            if (delivery == null || delivery.Type != AbilityProjectileType.Cone)
                throw new InvalidOperationException("Gorgon requires the inspected native cone.");
            delivery.Length = new Feet(60);
            var effect = ScriptableObject.CreateInstance<GorgonBreathEffect>();
            effect.Stone = stone;
            var activation = ScriptableObject.CreateInstance<GorgonBreathOnCast>();
            activation.OwningBlueprint = unit;
            activation.HitDieClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                ExpandedSummoningNaturalBuilder.HitDieClassGuid("MagicalBeast"), "magical-beast racial HD");
            activation.Cooldown = cooldown;
            var safe = ScriptableObject.CreateInstance<GorgonConeSafety>();
            safe.OwningBlueprint = unit;
            safe.Breath = breath;
            var available = ScriptableObject.CreateInstance<AbilityCasterHasNoFacts>();
            available.Facts = new BlueprintUnitFact[] { cooldown };
            var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Constitution;
            parameters.ReplaceCasterLevel = true;
            var racialHd = ScriptableObject.CreateInstance<ContextRankConfig>();
            SetField(racialHd, "m_Type", AbilityRankType.Default);
            SetField(racialHd, "m_BaseValueType", ContextRankBaseValueType.ClassLevel);
            SetField(racialHd, "m_Class", new[] { activation.HitDieClass });
            SetField(racialHd, "m_Progression", ContextRankProgression.AsIs);
            var halfHd = ScriptableObject.CreateInstance<ContextRankConfig>();
            SetField(halfHd, "m_Type", AbilityRankType.DamageBonus);
            SetField(halfHd, "m_BaseValueType", ContextRankBaseValueType.ClassLevel);
            SetField(halfHd, "m_Class", new[] { activation.HitDieClass });
            SetField(halfHd, "m_Progression", ContextRankProgression.Div2);
            parameters.CasterLevel = new Kingmaker.UnitLogic.Mechanics.ContextValue {
                ValueType = Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank, ValueRank = AbilityRankType.Default };
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = new Kingmaker.UnitLogic.Mechanics.ContextValue {
                ValueType = Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank, ValueRank = AbilityRankType.DamageBonus };
            breath.ComponentsArray = new BlueprintComponent[] { delivery, effect, activation,
                safe, available, racialHd, halfHd, parameters,
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(descriptor) };
            DescribeMagicalBeast(breath, prefix + "Breath", "Petrifying Breath",
                "Breathe a 60-foot cone, Fortitude DC 10 + half racial HD + Constitution modifier. A failed first exposure petrifies temporarily with recovery saves each round; a distinct later breath while petrified makes it permanent. Recharges in 1d4+1 rounds. The cone cannot be aimed through allies.", nativeStone.Icon);
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] { breath }).ToArray();
            ConfigureUngulateTrample(library, bySymbol, Sprint26MagicalBeastRules.GorgonUnit,
                prefix + "Trample", "gorgon");
            var trample = Require<BlueprintAbility>(bySymbol, prefix + "Trample");
            trample.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            var contact = trample.GetComponent<AbilityCustomOverrun>().Actions.Actions
                .OfType<ContextActionUngulateTrample>().Single();
            contact.HitDieClass = activation.HitDieClass;
            ConfigureMagicalBeastBrain(bySymbol, unit, prefix, "BreathAi", breath);

            prefix = Sprint26MagicalBeastRules.BulettePrefix;
            unit = Require<BlueprintUnit>(bySymbol, Sprint26MagicalBeastRules.BuletteUnit);
            var leap = Require<BlueprintAbility>(bySymbol, prefix + "Leap");
            CloneMephitSpell(library, leap, Sprint26MagicalBeastRules.ChargeDonor, "native legal charge");
            leap.name = InternalName(prefix + "Leap");
            leap.Type = AbilityType.Extraordinary;
            leap.Parent = null;
            leap.Hidden = false;
            leap.ActionBarAutoFillIgnored = false;
            leap.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            var onCast = ScriptableObject.CreateInstance<BuletteLeapOnCast>();
            onCast.OwningBlueprint = unit;
            onCast.Leap = leap;
            onCast.Claw = Require<BlueprintItemWeapon>(bySymbol, "KMG.Summoning.Natural.Bulette.Claw2d6");
            var geometry = ScriptableObject.CreateInstance<BuletteLeapChargeGeometry>();
            geometry.OwningBlueprint = unit;
            leap.ComponentsArray = leap.ComponentsArray.Concat(new BlueprintComponent[] { onCast, geometry }).ToArray();
            DescribeMagicalBeast(leap, prefix + "Leap", "Leap",
                "Make a legal above-ground charge with at least 10 feet of movement and a DC 20 jump check. Success replaces the charge attack with four primary claws and no bite; failure retains only the ordinary single charge attack. The racial jump bonus and speed adjustment apply only to this check. No burrowing or teleportation.", null);
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] { leap }).ToArray();
            ConfigureMagicalBeastBrain(bySymbol, unit, prefix, "LeapAi", leap);
        }

        private static void DescribeMagicalBeast(BlueprintUnitFact fact, string symbol,
            string name, string description, Sprite icon)
        {
            BlueprintUnitFactAccess.Resolve().Configure(fact,
                LocalizationService.Create(symbol + ".Name", name),
                LocalizationService.Create(symbol + ".Description", description), icon);
        }

        private static void ConfigureMagicalBeastBrain(
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit,
            string prefix, string aiSuffix, BlueprintAbility ability)
        {
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                prefix + aiSuffix), prefix + aiSuffix, ability, 0);
            var brain = Require<BlueprintBrain>(bySymbol, prefix + "Brain");
            brain.name = InternalName(prefix + "Brain");
            brain.Actions = (unit.Brain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { ai }).ToArray();
            unit.Brain = brain;
        }
    }
}
