using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Compatibility
{
    internal static class CustomWeaponMartialPerformanceIdentityPolicy
    {
        internal static bool IsPresent(bool found, string actualType,
            string actualName, string expectedType, string expectedName)
        {
            if (!found) return false;
            if (!string.Equals(actualType, expectedType,
                    StringComparison.Ordinal) ||
                !string.Equals(actualName, expectedName,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The optional Martial Performance identity or type changed.");
            return true;
        }
    }

    internal sealed class CustomWeaponMartialPerformanceChoice<T>
    {
        internal CustomWeaponMartialPerformanceChoice(string identity,
            string orderKey, T value)
        {
            if (string.IsNullOrWhiteSpace(identity))
                throw new ArgumentException("A choice identity is required.",
                    "identity");
            if (string.IsNullOrWhiteSpace(orderKey))
                throw new ArgumentException("A choice order key is required.",
                    "orderKey");
            if ((object)value == null) throw new ArgumentNullException("value");
            Identity = identity;
            OrderKey = orderKey;
            Value = value;
        }

        internal string Identity { get; private set; }
        internal string OrderKey { get; private set; }
        internal T Value { get; private set; }
    }

    internal sealed class CustomWeaponMartialPerformanceSelectionPolicy<T>
    {
        private readonly T[] _original;
        private readonly Func<T, string> _identity;

        internal CustomWeaponMartialPerformanceSelectionPolicy(T[] original,
            Func<T, string> identity)
        {
            _original = original;
            _identity = identity ??
                throw new ArgumentNullException("identity");
        }

        internal T[] Publish(IEnumerable<T> owned,
            IEnumerable<CustomWeaponMartialPerformanceChoice<T>> active)
        {
            T[] ownedValues = (owned ??
                throw new ArgumentNullException("owned")).ToArray();
            CustomWeaponMartialPerformanceChoice<T>[] activeValues = (active ??
                throw new ArgumentNullException("active")).ToArray();
            string[] ownedIds = ownedValues.Select(RequireIdentity).ToArray();
            if (ownedIds.Distinct(StringComparer.Ordinal).Count() !=
                ownedIds.Length)
                throw new InvalidOperationException(
                    "Martial Performance owned identities are not unique.");

            string[] activeIds = activeValues.Select(value =>
                value == null ? null : value.Identity).ToArray();
            if (activeValues.Any(value => value == null) ||
                activeIds.Distinct(StringComparer.Ordinal).Count() !=
                    activeIds.Length ||
                activeValues.Any(value =>
                    !ownedIds.Contains(value.Identity,
                        StringComparer.Ordinal) ||
                    !string.Equals(RequireIdentity(value.Value),
                        value.Identity, StringComparison.Ordinal)))
                throw new InvalidOperationException(
                    "Martial Performance active choices are not an exact owned subset.");

            T[] foreign = (_original ?? Array.Empty<T>()).Where(value =>
                !ownedIds.Contains(RequireIdentity(value),
                    StringComparer.Ordinal)).ToArray();
            T[] additions = activeValues.OrderBy(value => value.OrderKey,
                    StringComparer.Ordinal)
                .ThenBy(value => value.Identity, StringComparer.Ordinal)
                .Select(value => value.Value).ToArray();
            T[] result = foreign.Concat(additions).ToArray();
            foreach (string id in ownedIds)
            {
                int expected = activeIds.Contains(id,
                    StringComparer.Ordinal) ? 1 : 0;
                int observed = result.Count(value => string.Equals(
                    RequireIdentity(value), id, StringComparison.Ordinal));
                if (observed != expected)
                    throw new InvalidOperationException(
                        "Martial Performance publication was not exact for " +
                        id + ".");
            }
            return result;
        }

        internal T[] Rollback()
        {
            return _original;
        }

        private string RequireIdentity(T value)
        {
            if ((object)value == null)
                throw new InvalidOperationException(
                    "Martial Performance selector contains a null entry.");
            string result = _identity(value);
            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException(
                    "Martial Performance selector entry has no stable identity.");
            return result;
        }
    }

    internal static class CustomWeaponMartialPerformanceProficiencyPolicy
    {
        internal static bool CanUse(bool directCategoryProficiency,
            bool broadMartialProficiency, bool katanaGripDependent)
        {
            return directCategoryProficiency ||
                katanaGripDependent && broadMartialProficiency;
        }
    }
}
