using System;
using System.Reflection;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Deeds
{
    internal static class SlingersLuckRollAccess
    {
        private static readonly MethodInfo SavingSetter = Resolve(
            typeof(RuleSavingThrow));
        private static readonly MethodInfo SkillSetter = Resolve(
            typeof(RuleSkillCheck));

        internal static RulebookEvent.RollEntry RollNative()
        { return RulebookEvent.Dice.D20; }

        internal static void Replace(RuleSavingThrow rule,
            RulebookEvent.RollEntry value)
        { SavingSetter.Invoke(rule, new object[] { value }); }

        internal static void Replace(RuleSkillCheck rule,
            RulebookEvent.RollEntry value)
        { SkillSetter.Invoke(rule, new object[] { value }); }

        private static MethodInfo Resolve(Type type)
        {
            PropertyInfo property = type.GetProperty("D20", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.DeclaredOnly);
            MethodInfo setter = property == null ? null : property.GetSetMethod(true);
            if (property == null || property.PropertyType !=
                    typeof(RulebookEvent.RollEntry) || setter == null ||
                setter.IsPublic || setter.IsStatic || setter.GetParameters().Length != 1)
                throw new InvalidOperationException(type.FullName +
                    " exact non-public D20 setter was not found.");
            return setter;
        }
    }
}
