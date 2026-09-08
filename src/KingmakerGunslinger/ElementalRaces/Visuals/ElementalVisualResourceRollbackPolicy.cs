using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal static class ElementalVisualResourceRollbackPolicy
    {
        internal static T[] CreateRemovalPlan<T>(
            IList<T> registrationOrder, Func<T, bool> isPresent,
            Func<T, bool> isOwned, Func<T, string> identity)
            where T : class
        {
            if (registrationOrder == null)
                throw new ArgumentNullException("registrationOrder");
            if (isPresent == null) throw new ArgumentNullException("isPresent");
            if (isOwned == null) throw new ArgumentNullException("isOwned");
            if (identity == null) throw new ArgumentNullException("identity");

            var plan = new List<T>();
            for (int index = registrationOrder.Count - 1; index >= 0; index--)
            {
                T registration = registrationOrder[index];
                if (registration == null)
                    throw new InvalidOperationException(
                        "Elemental visual rollback contains a null registration.");
                if (!isPresent(registration)) continue;
                if (!isOwned(registration))
                {
                    string value = identity(registration);
                    throw new InvalidOperationException(
                        "Elemental visual rollback refused a foreign replacement for " +
                        (string.IsNullOrWhiteSpace(value) ? "<unknown>" :
                            value) + ".");
                }
                plan.Add(registration);
            }
            return plan.ToArray();
        }
    }
}
