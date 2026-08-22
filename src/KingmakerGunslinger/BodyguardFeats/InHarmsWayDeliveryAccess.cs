using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Utility;

namespace KingmakerGunslinger.BodyguardFeats
{
    /// <summary>
    /// Exact 2.1.7b target-redirection contract. The original roll has already
    /// resolved when these members are changed; the mutation exists only while
    /// attack delivery and its target subscribers run.
    /// </summary>
    internal static class InHarmsWayDeliveryAccess
    {
        private static readonly FieldInfo RuleTargetField =
            typeof(RulebookTargetEvent).GetField("Target",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
        private static readonly FieldInfo AbilityTargetField =
            typeof(AbilityDeliveryTarget).GetField("Target",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
        private static readonly MethodInfo AbilityAttackRollSetter =
            typeof(AbilityDeliveryTarget).GetMethod("set_AttackRoll",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly, null,
                new[] { typeof(RuleAttackRoll) }, null);
        private static readonly MethodInfo EventCompletionMethod =
            typeof(RulebookEventContext).GetMethod("PopEvent",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly, null,
                new[] { typeof(RulebookEvent) }, null);
        private static readonly MethodInfo AbilityApplyEffectMethod =
            typeof(AbilityExecutionProcess).GetMethod("ApplyEffect",
                BindingFlags.Static | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly, null, new[] {
                    typeof(AbilityExecutionContext),
                    typeof(AbilityDeliveryTarget), typeof(AbilityApplyEffect),
                    typeof(AbilitySelectTarget) }, null);
        private static readonly MethodInfo ContextDataDisposeMethod =
            typeof(ElementsContextData).GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);

        internal static bool ContractAvailable
        {
            get
            {
                return RuleTargetField != null && RuleTargetField.IsInitOnly &&
                    RuleTargetField.FieldType == typeof(UnitEntityData) &&
                    AbilityTargetField != null &&
                    AbilityTargetField.IsInitOnly &&
                    AbilityTargetField.FieldType == typeof(TargetWrapper) &&
                    AbilityAttackRollSetter != null &&
                    EventCompletionMethod != null &&
                    AbilityApplyEffectMethod != null &&
                    ContextDataDisposeMethod != null &&
                    typeof(ContextAttackData).IsSubclassOf(
                        typeof(ElementsContextData));
            }
        }

        internal static string ContractDescription
        {
            get
            {
                return "available=" + ContractAvailable +
                    ";ruleTarget=" + Describe(RuleTargetField) +
                    ";abilityTarget=" + Describe(AbilityTargetField) +
                    ";abilitySetter=" + Describe(AbilityAttackRollSetter) +
                    ";abilityApplyEffect=" + Describe(
                        AbilityApplyEffectMethod) + ";eventDid=" +
                    Describe(EventCompletionMethod) + ";contextDispose=" +
                    Describe(ContextDataDisposeMethod);
            }
        }

        internal static MethodBase EventCompletionTarget
        { get { return EventCompletionMethod; } }
        internal static MethodBase AbilitySetterTarget
        { get { return AbilityAttackRollSetter; } }
        internal static MethodBase AbilityApplyEffectTarget
        { get { return AbilityApplyEffectMethod; } }
        internal static MethodBase ContextDataDisposeTarget
        { get { return ContextDataDisposeMethod; } }

        internal static bool TryRedirectRuleTarget(RulebookTargetEvent rule,
            UnitEntityData expectedOriginal, UnitEntityData interceptor)
        {
            if (!ContractAvailable || rule == null || expectedOriginal == null ||
                interceptor == null) return false;
            object before;
            try { before = RuleTargetField.GetValue(rule); }
            catch { return false; }
            if (!ReferenceEquals(before, expectedOriginal)) return false;
            try
            {
                RuleTargetField.SetValue(rule, interceptor);
                if (ReferenceEquals(RuleTargetField.GetValue(rule), interceptor))
                    return true;
                RuleTargetField.SetValue(rule, expectedOriginal);
                return false;
            }
            catch
            {
                try
                {
                    if (ReferenceEquals(RuleTargetField.GetValue(rule),
                        interceptor))
                        RuleTargetField.SetValue(rule, expectedOriginal);
                }
                catch { }
                return false;
            }
        }

        internal static bool TryRestoreRuleTarget(RulebookTargetEvent rule,
            UnitEntityData interceptor, UnitEntityData original)
        {
            if (!ContractAvailable || rule == null || interceptor == null ||
                original == null) return false;
            try
            {
                if (!ReferenceEquals(RuleTargetField.GetValue(rule), interceptor))
                    return ReferenceEquals(RuleTargetField.GetValue(rule), original);
                RuleTargetField.SetValue(rule, original);
                return ReferenceEquals(RuleTargetField.GetValue(rule), original);
            }
            catch { return false; }
        }

        internal static bool IsRuleTarget(RulebookTargetEvent rule,
            UnitEntityData expected)
        {
            if (!ContractAvailable || rule == null || expected == null)
                return false;
            try { return ReferenceEquals(RuleTargetField.GetValue(rule), expected); }
            catch { return false; }
        }

        internal static bool TryRedirectAbilityTarget(
            AbilityDeliveryTarget delivery, RuleAttackRoll attackRoll,
            UnitEntityData interceptor, out TargetWrapper original,
            out TargetWrapper redirected)
        {
            original = null;
            redirected = null;
            if (!ContractAvailable || delivery == null || attackRoll == null ||
                interceptor == null ||
                !ReferenceEquals(delivery.AttackRoll, attackRoll)) return false;
            object before;
            try { before = AbilityTargetField.GetValue(delivery); }
            catch { return false; }
            original = before as TargetWrapper;
            if (original == null) return false;
            redirected = interceptor;
            try
            {
                AbilityTargetField.SetValue(delivery, redirected);
                if (ReferenceEquals(AbilityTargetField.GetValue(delivery),
                    redirected)) return true;
                AbilityTargetField.SetValue(delivery, before);
                return false;
            }
            catch
            {
                try { AbilityTargetField.SetValue(delivery, before); }
                catch { }
                return false;
            }
        }

        internal static bool TryRestoreAbilityTarget(
            AbilityDeliveryTarget delivery, TargetWrapper redirected,
            TargetWrapper original)
        {
            if (!ContractAvailable || delivery == null || redirected == null ||
                original == null) return false;
            try
            {
                object current = AbilityTargetField.GetValue(delivery);
                if (!ReferenceEquals(current, redirected))
                    return ReferenceEquals(current, original);
                AbilityTargetField.SetValue(delivery, original);
                return ReferenceEquals(AbilityTargetField.GetValue(delivery),
                    original);
            }
            catch { return false; }
        }

        private static string Describe(MemberInfo member)
        {
            return member == null ? "<missing>" :
                member.DeclaringType.FullName + "." + member.Name;
        }
    }
}
