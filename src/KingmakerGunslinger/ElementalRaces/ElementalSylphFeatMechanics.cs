using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Project-owned semantic marker for a buff that genuinely requires the
    /// affected creature to breathe. Inner Breath never infers this contract
    /// from an effect's name, Poison descriptor, cloud shape, or visuals.
    /// </summary>
    [Serializable]
    public sealed class ElementalRespirationRequired : BlueprintComponent
    {
    }

    [Serializable]
    public sealed class ElementalAiryStepSaveBonus :
        RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        private static readonly ConditionalWeakTable<RuleSavingThrow, object>
            AppliedRules = new ConditionalWeakTable<RuleSavingThrow, object>();
        private static readonly object AppliedMarker = new object();

        public BlueprintFeature WingsOfAir;

        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt == null || Owner == null || Owner.Stats == null ||
                Fact == null) return;
            SpellDescriptor descriptor = SourceDescriptor(evt);
            bool electricity = (descriptor & SpellDescriptor.Electricity) != 0;
            bool air = IsExactNativeAirEffect(SourceAbility(evt));
            bool electricityDamage = SourceDealsElectricityDamage(evt);
            bool wings = WingsOfAir != null && Owner.HasFact(WingsOfAir);
            int bonus = ElementalFeatPolicy.AiryStepSaveBonus(true, wings,
                air, electricity, electricityDamage);
            if (bonus <= 0) return;

            ModifiableValue stat = Owner.Stats.GetStat(evt.StatType);
            if (stat == null || !TryClaim(evt)) return;
            ModifiableValue.Modifier modifier = stat.AddModifier(bonus, Fact,
                GetType().FullName, ModifierDescriptor.Racial);
            if (modifier == null) return;
            stat.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }

        public override void OnEventDidTrigger(RuleSavingThrow evt) { }

        private static SpellDescriptor SourceDescriptor(RuleSavingThrow evt)
        {
            SpellDescriptor result = SpellDescriptor.None;
            RuleReason reason = evt == null ? null : evt.Reason;
            MechanicsContext context = reason == null ? null : reason.Context;
            if (context != null) result |= context.SpellDescriptor;
            BlueprintAbility ability = SourceAbility(evt);
            var visited = new HashSet<BlueprintAbility>();
            while (ability != null && visited.Add(ability))
            {
                result |= ability.SpellDescriptor;
                ability = ability.Parent;
            }
            return result;
        }

        private static BlueprintAbility SourceAbility(RuleSavingThrow evt)
        {
            RuleReason reason = evt == null ? null : evt.Reason;
            MechanicsContext context = reason == null ? null : reason.Context;
            return reason != null && reason.Ability != null ?
                reason.Ability.Blueprint : context == null ? null :
                context.SourceAbility;
        }

        private static bool IsExactNativeAirEffect(BlueprintAbility ability)
        {
            var visited = new HashSet<BlueprintAbility>();
            while (ability != null && visited.Add(ability))
            {
                if (ElementalFeatPolicy.IsExactNativeAirEffectGuid(
                        ability.AssetGuid)) return true;
                ability = ability.Parent;
            }
            return false;
        }

        private static bool SourceDealsElectricityDamage(RuleSavingThrow evt)
        {
            RuleReason reason = evt == null ? null : evt.Reason;
            RuleDealDamage damage = reason == null ? null :
                reason.Rule as RuleDealDamage;
            return damage != null && damage.DamageBundle != null &&
                damage.DamageBundle.OfType<EnergyDamage>().Any(value =>
                    value.EnergyType ==
                        Kingmaker.Enums.Damage.DamageEnergyType.Electricity);
        }

        private static bool TryClaim(RuleSavingThrow rule)
        {
            lock (AppliedRules)
            {
                object ignored;
                if (AppliedRules.TryGetValue(rule, out ignored)) return false;
                AppliedRules.Add(rule, AppliedMarker);
                return true;
            }
        }
    }

    internal static class ElementalCloudGazerRuntime
    {
        private const string NativeInvisibilityComponent =
            "Kingmaker.Designers.Mechanics.Buffs.BuffInvisibility";
        private const string NativeDarknessStatusBuff =
            "64737e33d1d185b4194798e9abee76ca";

        internal static bool ShouldBypass(RuleConcealmentCheck check,
            bool nativeResult)
        {
            RulebookEventContext context = Rulebook.CurrentContext;
            RuleAttackRoll attack = context == null ? null :
                context.LastEvent<RuleAttackRoll>();
            bool exact = attack != null && check != null &&
                ReferenceEquals(attack.ConcealmentCheck, check) &&
                attack.Initiator != null && attack.Target != null;
            if (!exact) return false;

            ElementalFeatBlueprintSet blueprints =
                BlueprintBootstrap.ElementalFeats;
            BlueprintFeature cloudGazer = blueprints == null ? null :
                blueprints.RequireFeature(ElementalFeatId.CloudGazer);
            UnitEntityData attacker = attack.Initiator;
            UnitEntityData target = attack.Target;
            bool owns = cloudGazer != null && attacker.Descriptor != null &&
                attacker.Descriptor.HasFact(cloudGazer);
            bool canSee = attacker.Descriptor != null &&
                attacker.Descriptor.State != null &&
                !attacker.Descriptor.State.HasCondition(
                    UnitCondition.Blindness) &&
                !HasActiveBuff(attacker, NativeDarknessStatusBuff);
            bool invisible = HasComponent(target,
                NativeInvisibilityComponent);

            int qualifying = 0;
            int unrelated = 0;
            foreach (Buff buff in ActiveBuffs(target))
            {
                AddConcealment[] sources = Components(buff)
                    .OfType<AddConcealment>().ToArray();
                if (sources.Length == 0) continue;
                bool exactNative = ElementalFeatPolicy
                    .IsExactNativeCloudGazerConcealmentGuid(
                        buff.Blueprint.AssetGuid);
                ElementalFiresightConcealmentSource marker = Components(buff)
                    .OfType<ElementalFiresightConcealmentSource>()
                    .SingleOrDefault();
                bool projectFog = marker != null && marker.Kind ==
                    ElementalFiresightConcealmentKind.FogMistOrCloud;
                bool classificationMatches = sources.Any(value =>
                    value.Concealment == check.Concealment);
                if ((exactNative || projectFog) && classificationMatches)
                    qualifying++;
                else unrelated++;
            }

            return ElementalFeatPolicy.CloudGazerCanBypass(!nativeResult,
                exact, owns, canSee, invisible, qualifying, unrelated);
        }

        private static bool HasActiveBuff(UnitEntityData unit, string guid)
        {
            return ActiveBuffs(unit).Any(value => value.Blueprint != null &&
                string.Equals(value.Blueprint.AssetGuid, guid,
                    StringComparison.Ordinal));
        }

        private static bool HasComponent(UnitEntityData unit,
            string componentType)
        {
            return ActiveBuffs(unit).SelectMany(Components).Any(value =>
                value != null && string.Equals(value.GetType().FullName,
                    componentType, StringComparison.Ordinal));
        }

        private static Buff[] ActiveBuffs(UnitEntityData unit)
        {
            return unit == null || unit.Descriptor == null ||
                unit.Descriptor.Buffs == null ? new Buff[0] :
                unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    value != null && value.Active &&
                    value.Blueprint != null).ToArray();
        }

        private static BlueprintComponent[] Components(Buff buff)
        {
            return buff == null || buff.Blueprint == null ?
                new BlueprintComponent[0] : buff.Blueprint.ComponentsArray ??
                new BlueprintComponent[0];
        }
    }

    [HarmonyPatch(typeof(RuleConcealmentCheck), "get_Success")]
    internal static class ElementalCloudGazerConcealmentPatch
    {
        private static void Postfix(RuleConcealmentCheck __instance,
            ref bool __result)
        {
            try
            {
                if (ElementalCloudGazerRuntime.ShouldBypass(__instance,
                        __result))
                    __result = true;
            }
            catch (Exception)
            {
                // Fail closed to the exact native concealment result.
            }
        }
    }

    [Serializable]
    public sealed class ElementalInnerBreathImmunity :
        RuleInitiatorLogicComponent<RuleApplyBuff>
    {
        public override void OnEventAboutToTrigger(RuleApplyBuff evt)
        {
            if (evt == null || !evt.CanApply || evt.Blueprint == null) return;
            bool exactNative = ElementalFeatPolicy
                .IsExactNativeRespirationRequiredBuffGuid(
                    evt.Blueprint.AssetGuid);
            bool explicitProject = (evt.Blueprint.ComponentsArray ??
                new BlueprintComponent[0])
                .OfType<ElementalRespirationRequired>().Any();
            if (ElementalFeatPolicy.InnerBreathGrantsImmunity(
                    exactNative || explicitProject))
                evt.CanApply = false;
        }

        public override void OnEventDidTrigger(RuleApplyBuff evt) { }
    }
}
