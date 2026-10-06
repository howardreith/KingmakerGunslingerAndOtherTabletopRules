using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class FieryGlareFoundation
    {
        internal BlueprintBuff Buff;
        internal BlueprintActivatableAbility Toggle;
    }
    internal sealed class StoicDignityFoundation
    {
        internal BlueprintBuff Provider, Recipient;
        internal BlueprintAbilityAreaEffect Area;
    }

    // Unregistered construction only. There is no bootstrap call, registry,
    // localization, icon, race/selection grant, setting or player acquisition.
    // The caller owns these objects; ordinary mod initialization never calls it.
    internal static class UnpublishedRaceTraitFoundationFactory
    {
        internal const string FieryDescription = "While Fiery Glare is enabled, Intimidate checks use a result of 10 whenever that would succeed; otherwise, they are rolled normally. This functions even during combat.";
        internal static void VerifyFieryContract(Type nativeComponent)
        {
            var field = nativeComponent == null ? null : nativeComponent.GetField("Skill", BindingFlags.Instance | BindingFlags.Public);
            var handler = nativeComponent == null ? null : nativeComponent.GetMethod("OnEventAboutToTrigger", new[] { typeof(Kingmaker.RuleSystem.Rules.RuleSkillCheck) });
            if (nativeComponent != typeof(Take10ForSuccess) || field == null || field.FieldType != typeof(StatType) ||
                handler == null || handler.ReturnType != typeof(void) || (int)StatType.CheckIntimidate != 0x67)
                throw new InvalidOperationException("Exact native Take10ForSuccess/CheckIntimidate contract absent.");
        }
        internal static FieryGlareFoundation CreateFieryGlare()
        {
            VerifyFieryContract(typeof(Take10ForSuccess));
            var buff = HiddenBuff("KMG_Unpublished_FieryGlare_Buff");
            var take10 = ScriptableObject.CreateInstance<Take10ForSuccess>();
            take10.name = "$FieryGlareIntimidateOnly"; take10.Skill = StatType.CheckIntimidate;
            buff.ComponentsArray = new BlueprintComponent[] { take10 };
            var toggle = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
            toggle.name = "KMG_Unpublished_FieryGlare_Toggle";
            toggle.Buff = buff; toggle.Group = ActivatableAbilityGroup.None;
            toggle.IsOnByDefault = false; toggle.ActivationType = AbilityActivationType.Immediately;
            var command = typeof(BlueprintActivatableAbility).GetField("m_ActivateWithUnitCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            if (command == null || command.FieldType != typeof(UnitCommand.CommandType)) throw new MissingFieldException("BlueprintActivatableAbility.m_ActivateWithUnitCommand");
            command.SetValue(toggle, UnitCommand.CommandType.Free);
            toggle.DeactivateImmediately = true; toggle.DeactivateIfCombatEnded = false;
            toggle.DeactivateIfOwnerDisabled = false; toggle.DeactivateIfOwnerUnconscious = false;
            toggle.OnlyInCombat = false; toggle.ActionBarAutoFillIgnored = true;
            toggle.ResourceAssetIds = Array.Empty<string>(); toggle.ComponentsArray = Array.Empty<BlueprintComponent>();
            return new FieryGlareFoundation { Buff = buff, Toggle = toggle };
        }
        internal static StoicDignityFoundation CreateStoicDignity()
        {
            StoicDignityRuntime.VerifyContract();
            var recipient = HiddenBuff("KMG_Unpublished_StoicDignity_Recipient");
            var bonus = ScriptableObject.CreateInstance<StoicDignitySaveBonus>();
            bonus.name = "$StoicDignityAllySave"; bonus.AllyRecipient = true;
            recipient.ComponentsArray = new BlueprintComponent[] { bonus };
            var area = ScriptableObject.CreateInstance<BlueprintAbilityAreaEffect>();
            area.name = "KMG_Unpublished_StoicDignity_Area";
            area.Shape = AreaEffectShape.Cylinder; area.Size = 10.Feet();
            area.AffectEnemies = false; area.AggroEnemies = false; area.AffectDead = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false; area.Fx = new PrefabLink();
            var allies = ScriptableObject.CreateInstance<ContextConditionIsAlly>();
            var other = ScriptableObject.CreateInstance<ContextConditionIsCaster>(); other.Not = true;
            var delivery = ScriptableObject.CreateInstance<AbilityAreaEffectBuff>();
            delivery.name = "$StoicDignityAlliesExceptHolder"; delivery.Buff = recipient;
            delivery.Condition = new ConditionsChecker { Conditions = new Condition[] { allies, other } };
            area.ComponentsArray = new BlueprintComponent[] { delivery };
            var provider = HiddenBuff("KMG_Unpublished_StoicDignity_Provider");
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>();
            emanation.name = "$StoicDignityMovingEmanation"; emanation.AreaEffect = area;
            var self = ScriptableObject.CreateInstance<StoicDignitySaveBonus>(); self.name = "$StoicDignitySelfSave";
            provider.ComponentsArray = new BlueprintComponent[] { self, emanation };
            return new StoicDignityFoundation { Provider = provider, Recipient = recipient, Area = area };
        }
        private static BlueprintBuff HiddenBuff(string name)
        {
            var value = ScriptableObject.CreateInstance<BlueprintBuff>(); value.name = name;
            var flags = typeof(BlueprintBuff).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (flags == null || flags.FieldType.FullName != "Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff+Flags" || !Enum.IsDefined(flags.FieldType, "HiddenInUi")) throw new MissingFieldException("BlueprintBuff.m_Flags");
            flags.SetValue(value, Enum.Parse(flags.FieldType, "HiddenInUi", false));
            value.Stacking = StackingType.Stack; value.IsClassFeature = true;
            value.FxOnStart = new PrefabLink(); value.FxOnRemove = new PrefabLink(); value.ResourceAssetIds = Array.Empty<string>();
            value.ComponentsArray = Array.Empty<BlueprintComponent>();
            return value;
        }
    }
}
