using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Classes
{
    internal static class GunslingerInitiativeRuntime
    {
        private sealed class AppliedMarker { }
        private static readonly FieldInfo ModifierField =
            typeof(RuleInitiativeRoll).GetField("<Modifier>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly ConditionalWeakTable<RuleInitiativeRoll, AppliedMarker>
            Applied = new ConditionalWeakTable<RuleInitiativeRoll, AppliedMarker>();
        private static readonly GunslingerInitiativeService Service =
            new GunslingerInitiativeService();

        internal static bool Apply(RuleInitiativeRoll rule, int currentGrit)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            int bonus = Service.CalculateBonus(currentGrit);
            if (bonus == 0)
            {
                GunslingerInitiativeRuntimeDiagnostics.Rejected++;
                return false;
            }
            lock (Applied)
            {
                AppliedMarker existing;
                if (Applied.TryGetValue(rule, out existing))
                {
                    GunslingerInitiativeRuntimeDiagnostics.Duplicates++;
                    return false;
                }
                if (ModifierField == null || ModifierField.FieldType != typeof(int))
                    throw new MissingFieldException(typeof(RuleInitiativeRoll).FullName,
                        "<Modifier>k__BackingField");
                int current = (int)ModifierField.GetValue(rule);
                ModifierField.SetValue(rule, checked(current + bonus));
                Applied.Add(rule, new AppliedMarker());
            }
            GunslingerInitiativeRuntimeDiagnostics.Applied++;
            return true;
        }
    }

    internal static class GunslingerInitiativeRuntimeDiagnostics
    {
        internal static int Applied;
        internal static int Rejected;
        internal static int Duplicates;
        internal static int Faults;

        internal static void Reset()
        {
            Applied = 0; Rejected = 0; Duplicates = 0; Faults = 0;
        }
    }
}
