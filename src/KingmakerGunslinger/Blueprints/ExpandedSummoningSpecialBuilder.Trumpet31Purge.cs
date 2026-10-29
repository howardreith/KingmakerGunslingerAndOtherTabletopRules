using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
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
        private static BlueprintAbility ConfigureTrumpet31Purge(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var spell = Require<BlueprintAbility>(bySymbol, Trumpet31Prefix + "InvisibilityPurge");
            CloneMephitSpell(library, spell, "30e5dc243f937fc4b95d2f8f4e1b7ff3", "native See Invisibility presentation");
            spell.name = InternalName(Trumpet31Prefix + "InvisibilityPurge");
            spell.Parent = null; spell.Type = AbilityType.Spell; spell.Range = AbilityRange.Personal;
            spell.CanTargetSelf = true; spell.CanTargetFriends = false; spell.CanTargetEnemies = false;
            spell.CanTargetPoint = false; spell.SpellResistance = false; spell.ActionType = UnitCommand.CommandType.Standard;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful; spell.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            spell.Hidden = false; spell.ActionBarAutoFillIgnored = false;
            spell.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            var carrier = Require<BlueprintBuff>(bySymbol, Trumpet31Prefix + "PurgeCarrier");
            var recipient = Require<BlueprintBuff>(bySymbol, Trumpet31Prefix + "PurgeRecipient");
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Trumpet31Prefix + "PurgeArea");
            carrier.name = InternalName(Trumpet31Prefix + "PurgeCarrier");
            recipient.name = InternalName(Trumpet31Prefix + "PurgeRecipient");
            area.name = InternalName(Trumpet31Prefix + "PurgeArea");
            carrier.Stacking = StackingType.Replace; recipient.Stacking = StackingType.Stack;
            var suppression = ScriptableObject.CreateInstance<AddConditionImmunity>(); suppression.Condition = UnitCondition.Invisible;
            recipient.ComponentsArray = new BlueprintComponent[] { suppression };
            var movement = ScriptableObject.CreateInstance<AddAreaEffect>(); movement.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { movement };
            if (carrier.StayOnDeath) throw new InvalidOperationException("Invisibility Purge must end on its bearer's death.");
            var delivery = ScriptableObject.CreateInstance<TrumpetInvisibilityPurgeArea>();
            delivery.OwningArea = area; delivery.Recipient = recipient;
            delivery.NaturalInvisibility = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                NaturalInvisibilityGuid, "Purge-exempt native natural invisibility");
            area.Shape = AreaEffectShape.Cylinder; area.Size = new Feet(70);
            area.AffectEnemies = true; area.AggroEnemies = false; area.AffectDead = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false; area.Fx = new PrefabLink();
            area.ComponentsArray = new BlueprintComponent[] { delivery };
            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            SetField(rank, "m_Type", AbilityRankType.Default);
            SetField(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
            SetField(rank, "m_Progression", ContextRankProgression.AsIs);
            var school = ScriptableObject.CreateInstance<SpellComponent>(); school.School = SpellSchool.Evocation;
            var apply = new ContextActionApplyBuff { Buff = carrier, IsFromSpell = true, IsNotDispelable = false,
                DurationValue = new ContextDurationValue { Rate = DurationRate.Minutes, DiceType = DiceType.Zero,
                    DiceCountValue = 0, BonusValue = new ContextValue { ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.Default } } };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            spell.ComponentsArray = new BlueprintComponent[] { school, rank, effect };
            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(Trumpet31Prefix + "Purge.Name", "Invisibility Purge");
            var description = LocalizationService.Create(Trumpet31Prefix + "Purge.Description",
                "For 1 minute per caster level, a moving emanation extends 5 feet per caster level from you. All invisibility is suppressed inside, including allies' invisibility. Leaving restores invisibility whose original effect is still active. No save or spell resistance. This does not reveal ordinary Stealth.");
            foreach (var fact in new BlueprintUnitFact[] { spell, carrier, recipient })
                access.Configure(fact, title, description, spell.Icon);
            spell.LocalizedDuration = LocalizationService.Create(Trumpet31Prefix + "Purge.Duration", "1 minute/level");
            spell.LocalizedSavingThrow = LocalizationService.Create(Trumpet31Prefix + "Purge.Save", "None");
            return spell;
        }
    }
}
