using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Gunsmithing;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class GunsmithingCraftingBlueprintSet
    {
        internal GunsmithingCraftingBlueprintSet(BlueprintAbility basicAbility,
            BlueprintAbility paperAbility, BlueprintFeature marker)
        { BasicAbility = basicAbility; PaperAbility = paperAbility; UsedMarker = marker; }
        internal BlueprintAbility Ability { get { return BasicAbility; } }
        internal BlueprintAbility BasicAbility { get; private set; }
        internal BlueprintAbility PaperAbility { get; private set; }
        internal BlueprintFeature UsedMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class GunsmithingCraftingBlueprints
    {
        internal const string AbilitySymbol = "KMG.Gunsmithing.CraftBasicAmmunition";
        internal const string PaperAbilitySymbol =
            "KMG.Gunsmithing.CraftPaperCartridges";
        internal const string MarkerSymbol = "KMG.Gunsmithing.CraftedThisRest";
        internal static GunsmithingCraftingBlueprintSet Register(BlueprintRegistry registry,
            BasicAmmunitionBlueprintSet ammo, BlueprintItem tool)
        {
            BlueprintFeature marker = registry.Register<BlueprintFeature>(MarkerSymbol, () =>
            {
                var value = ScriptableObject.CreateInstance<BlueprintFeature>();
                value.name = "KMG_CraftedBasicAmmunition_ThisRest";
                value.Ranks = 1; value.IsClassFeature = true; value.HideInUI = true;
                value.ComponentsArray = Array.Empty<BlueprintComponent>(); return value;
            });
            BlueprintAbility ability = registry.Register<BlueprintAbility>(AbilitySymbol, () =>
            {
                var value = ScriptableObject.CreateInstance<BlueprintAbility>();
                value.name = "KMG_CraftBasicFirearmAmmunition_Ability";
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create("KMG.Crafting.BasicAmmo.Name", "Craft Basic Firearm Ammunition"),
                    LocalizationService.Create("KMG.Crafting.BasicAmmo.Description",
                        "Once per rest, use a Gunsmith's Kit outside combat to immediately pay 22 gp (10% of ordinary purchase value) and create 20 Black Powder Charges and 20 Lead Balls. A failed transaction consumes nothing."), tool.Icon);
                value.Type = AbilityType.Extraordinary; value.Range = AbilityRange.Personal;
                value.CanTargetSelf = true; value.ActionType = UnitCommand.CommandType.Free;
                value.SetIsFullRoundAction(false); value.NeedEquipWeapons = false;
                value.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
                value.LocalizedDuration = LocalizationService.Create("KMG.Crafting.BasicAmmo.Duration", "Instantaneous");
                value.LocalizedSavingThrow = LocalizationService.Create("KMG.Crafting.BasicAmmo.Save", "None");
                value.ComponentsArray = new BlueprintComponent[] {
                    CraftBasicAmmunitionAbilityLogic.Create(ammo.BlackPowder,
                        ammo.LeadBall, tool, marker) };
                return value;
            });
            if (ability.ComponentsArray.OfType<CraftBasicAmmunitionAbilityLogic>().Single().GoldCost != 22)
                throw new InvalidOperationException("Basic ammunition craft cost must equal 22 gp.");
            BlueprintAbility paperAbility = registry.Register<BlueprintAbility>(
                PaperAbilitySymbol, () =>
            {
                var value = ScriptableObject.CreateInstance<BlueprintAbility>();
                value.name = "KMG_CraftPaperCartridges_Ability";
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create("KMG.Crafting.PaperCartridges.Name",
                        "Craft Paper Cartridges"),
                    LocalizationService.Create("KMG.Crafting.PaperCartridges.Description",
                        "Once per rest, use a Gunsmith's Kit outside combat to pay 24 gp (10% of ordinary purchase value) and create 20 Paper Cartridges. This shares the same entitlement as Craft Basic Firearm Ammunition; choosing either recipe prevents the other until rest. A failed transaction consumes nothing."),
                    ammo.PaperCartridge.Icon);
                value.Type = AbilityType.Extraordinary;
                value.Range = AbilityRange.Personal;
                value.CanTargetSelf = true;
                value.ActionType = UnitCommand.CommandType.Free;
                value.SetIsFullRoundAction(false);
                value.NeedEquipWeapons = false;
                value.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
                value.LocalizedDuration = LocalizationService.Create(
                    "KMG.Crafting.PaperCartridges.Duration", "Instantaneous");
                value.LocalizedSavingThrow = LocalizationService.Create(
                    "KMG.Crafting.PaperCartridges.Save", "None");
                value.ComponentsArray = new BlueprintComponent[] {
                    CraftPaperCartridgesAbilityLogic.Create(ammo.PaperCartridge,
                        tool, marker) };
                return value;
            });
            if (paperAbility.ComponentsArray.OfType<CraftPaperCartridgesAbilityLogic>()
                .Single().GoldCost != 24)
                throw new InvalidOperationException(
                    "Paper Cartridge craft cost must equal 24 gp.");
            return new GunsmithingCraftingBlueprintSet(ability, paperAbility, marker);
        }
    }
}
