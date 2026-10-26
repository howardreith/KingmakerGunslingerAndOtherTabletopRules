using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintActivatableAbility ConfigureLillendFascinate32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintActivatableAbility courage, BlueprintUnit owner)
        {
            var icon = Feature(library, "ddaec3a5845bc7d4191792529b687d65", "native Fascinate feature icon").Icon;
            var shakeIcon = ProjectAssetIcons.RequireIcon("shake-free");
            var effect = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "Fascinated");
            var carrier = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "FascinateCarrier");
            var resistance = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "FascinateResistance");
            var assistance = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "FascinateAssistance");
            var bardic = Require<BlueprintAbility>(bySymbol, Lillend32Prefix + "BardicSuggestion");
            foreach (var row in new[] { new { Buff = effect, Key = "Fascinated", Title = "Fascinated" },
                new { Buff = carrier, Key = "FascinateCarrier", Title = "Fascinate" },
                new { Buff = resistance, Key = "FascinateResistance", Title = "Fascinate Resistance" },
                new { Buff = assistance, Key = "FascinateAssistance", Title = "Shake Free" } })
            {
                row.Buff.name = InternalName(Lillend32Prefix + row.Key);
                row.Buff.Stacking = StackingType.Stack;
                SetBuffFlags(row.Buff, harmful: ReferenceEquals(row.Buff, effect), hidden: ReferenceEquals(row.Buff, assistance));
                BlueprintUnitFactAccess.Resolve().Configure(row.Buff,
                    LocalizationService.Create(Lillend32Prefix + row.Key + ".Name", row.Title),
                    LocalizationService.Create(Lillend32Prefix + row.Key + ".Description",
                        ReferenceEquals(row.Buff, resistance) ? "A successful Will save prevents this lillend from fascinating you for 24 hours." :
                        ReferenceEquals(row.Buff, effect) ? "Observe this performance quietly; -4 to reactive Perception. Potential threats allow another Will save; obvious threats or harm end fascination." :
                        "An owned effect of this lillend's Fascinate performance."), ReferenceEquals(row.Buff, assistance) ? shakeIcon : icon);
            }
            resistance.ComponentsArray = Array.Empty<BlueprintComponent>();
            var condition = ScriptableObject.CreateInstance<AddCondition>(); condition.Condition = UnitCondition.Dazed;
            var threats = ScriptableObject.CreateInstance<LillendFascinated32>(); threats.Aura = carrier;
            effect.ComponentsArray = new BlueprintComponent[] { FascinateDescriptor32(), condition,
                ScriptableObject.CreateInstance<ElementalNereidPerception>(), threats };
            var shake = Require<BlueprintAbility>(bySymbol, Lillend32Prefix + "FascinateShakeFree");
            shake.name = InternalName(Lillend32Prefix + "FascinateShakeFree");
            ConfigureMephitSelfAbility(shake, AbilityType.Extraordinary, AbilityEffectOnUnit.None, AbilityEffectOnUnit.Helpful, false);
            shake.Range = AbilityRange.Touch; shake.CanTargetSelf = false; shake.CanTargetFriends = true;
            var shakeCheck = ScriptableObject.CreateInstance<ElementalNereidShakeTarget>(); shakeCheck.Fascinated = effect;
            var shakeAction = ScriptableObject.CreateInstance<ElementalNereidShakeFree>(); shakeAction.Fascinated = effect;
            var shakeRun = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            shakeRun.Actions = new ActionList { Actions = new GameAction[] { shakeAction } };
            shake.ComponentsArray = new BlueprintComponent[] { shakeCheck, shakeRun };
            BlueprintUnitFactAccess.Resolve().Configure(shake,
                LocalizationService.Create(Lillend32Prefix + "FascinateShakeFree.Name", "Shake Free"),
                LocalizationService.Create(Lillend32Prefix + "FascinateShakeFree.Description",
                    "As a standard action at touch range, shake an ally free of Lillend Fascinate."), shakeIcon);
            var grant = ScriptableObject.CreateInstance<AddFacts>(); grant.Facts = new BlueprintUnitFact[] { shake };
            grant.DoNotRestoreMissingFacts = false;
            var helper = ScriptableObject.CreateInstance<LillendFascinateAssistance32>(); helper.Carrier = carrier;
            assistance.ComponentsArray = new BlueprintComponent[] { grant, helper };
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Lillend32Prefix + "FascinateArea");
            CopyFields(Require<BlueprintAbilityAreaEffect>(bySymbol, Lillend32Prefix + "CourageArea"), area);
            area.name = InternalName(Lillend32Prefix + "FascinateArea");
            area.Size = new Kingmaker.Utility.Feet(90); area.AffectEnemies = true; area.AggroEnemies = false;
            area.AffectDead = false; area.SpellResistance = false;
            var delivery = ScriptableObject.CreateInstance<LillendFascinateArea32>(); delivery.Carrier = carrier;
            area.ComponentsArray = new BlueprintComponent[] { FascinateDescriptor32(),
                ScriptableObject.CreateInstance<LillendFascinateParameters32>(), delivery };
            var state = ScriptableObject.CreateInstance<LillendFascinateState32>();
            state.name = "$KMG.Lillend.Fascinate.Responses";
            state.Fascinated = effect; state.Resistance = resistance; state.Assistance = assistance; state.BardicSuggestion = bardic;
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.Stacking = StackingType.Replace;
            carrier.ComponentsArray = new BlueprintComponent[] { state, emanation };
            var toggle = Require<BlueprintActivatableAbility>(bySymbol, Lillend32Prefix + "Fascinate");
            CopyFields(courage, toggle); toggle.name = InternalName(Lillend32Prefix + "Fascinate"); toggle.Buff = carrier;
            toggle.OnlyInCombat = false; toggle.DeactivateIfCombatEnded = false;
            toggle.ComponentsArray = courage.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(toggle,
                LocalizationService.Create(Lillend32Prefix + "Fascinate.Name", "Fascinate"),
                LocalizationService.Create(Lillend32Prefix + "Fascinate.Description",
                    "Outside combat, start as a move action and spend native performance rounds. Up to three selected creatures within 90 feet that can see and hear the lillend attempt Will DC 13 + its Charisma modifier. A successful save grants 24-hour immunity to this lillend. Threats interrupt fascination."), icon);
            bardic.name = InternalName(Lillend32Prefix + "BardicSuggestion");
            ConfigureMephitSelfAbility(bardic, AbilityType.SpellLike, AbilityEffectOnUnit.Harmful, AbilityEffectOnUnit.None, true);
            bardic.Range = AbilityRange.Close; bardic.CanTargetEnemies = true; bardic.CanTargetSelf = false; bardic.Hidden = true;
            var available = ScriptableObject.CreateInstance<Sprint32SuggestionAvailable>(); available.OwningBlueprint = owner;
            var target = ScriptableObject.CreateInstance<LillendBardicSuggestionTarget32>();
            target.Carrier = carrier; target.Effect = Require<BlueprintBuff>(bySymbol, Sprint32Suggestion.OwnedBuffSymbol); target.Ability = bardic;
            bardic.ComponentsArray = new BlueprintComponent[] { available, target };
            BlueprintUnitFactAccess.Resolve().Configure(bardic,
                LocalizationService.Create(Lillend32Prefix + "BardicSuggestion.Name", "Bardic Suggestion"),
                LocalizationService.Create(Lillend32Prefix + "BardicSuggestion.Description",
                    "With active CowWithHat Custom Spells, suggest its avoid-combat instruction to one creature fascinated by this lillend. Standard action, Will DC 13 + Charisma, spell resistance; no slot or extra performance round. This act does not interrupt its own Fascinate."), icon);
            return toggle;
        }
        private static SpellDescriptorComponent FascinateDescriptor32()
        {
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion;
            return descriptor;
        }
    }
}
