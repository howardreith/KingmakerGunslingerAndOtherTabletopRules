using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintActivatableAbility ConfigureLillendCompetence32(
            LibraryScriptableObject library, IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintActivatableAbility courage)
        {
            var icon = Feature(library, "6d3fcfab6d935754c918eb0e004b5ef7",
                "native Inspire Competence feature semantic icon").Icon;
            var recipient = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "CompetenceRecipient");
            recipient.name = InternalName(Lillend32Prefix + "CompetenceRecipient");
            recipient.Stacking = StackingType.Stack;
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.SkillPerception; bonus.Value = 3; bonus.Descriptor = ModifierDescriptor.Competence;
            recipient.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(recipient,
                LocalizationService.Create(Lillend32Prefix + "CompetenceRecipient.Name", "Inspire Competence"),
                LocalizationService.Create(Lillend32Prefix + "CompetenceRecipient.Description",
                    "+3 competence to Perception while this ally can see and hear the performing lillend within 30 feet."), icon);
            var carrier = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "CompetenceCarrier");
            CopyFields(courage.Buff, carrier); carrier.name = InternalName(Lillend32Prefix + "CompetenceCarrier");
            BlueprintUnitFactAccess.Resolve().Configure(carrier,
                LocalizationService.Create(Lillend32Prefix + "CompetenceCarrier.Name", "Inspire Competence"),
                LocalizationService.Create(Lillend32Prefix + "CompetenceCarrier.Description",
                    "Performing to help one selected ally with Perception."), icon);
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Lillend32Prefix + "CompetenceArea");
            CopyFields(Require<BlueprintAbilityAreaEffect>(bySymbol, Lillend32Prefix + "CourageArea"), area);
            area.name = InternalName(Lillend32Prefix + "CompetenceArea"); area.Size = new Kingmaker.Utility.Feet(30);
            area.AggroEnemies = false;
            var delivery = ScriptableObject.CreateInstance<LillendCompetenceArea32>();
            delivery.OwningArea = area; delivery.Carrier = carrier; delivery.Recipient = recipient;
            area.ComponentsArray = new BlueprintComponent[] { delivery };
            var state = ScriptableObject.CreateInstance<LillendCompetenceState32>(); state.Recipient = recipient;
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { state, emanation };
            var toggle = Require<BlueprintActivatableAbility>(bySymbol, Lillend32Prefix + "Competence");
            CopyFields(courage, toggle); toggle.name = InternalName(Lillend32Prefix + "Competence"); toggle.Buff = carrier;
            toggle.DeactivateIfCombatEnded = false;
            toggle.ComponentsArray = courage.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(toggle,
                LocalizationService.Create(Lillend32Prefix + "Competence.Name", "Inspire Competence"),
                LocalizationService.Create(Lillend32Prefix + "Competence.Description",
                    "Outside combat, choose one nearby party ally before a normal move-action performance. That ally gains +3 competence to Perception within 30 feet. Uses this summon’s native performance rounds; ends on combat entry."), icon);
            return toggle;
        }
    }
}
