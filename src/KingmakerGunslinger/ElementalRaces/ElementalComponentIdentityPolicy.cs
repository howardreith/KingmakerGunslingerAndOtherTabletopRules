using System;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Preserves existing native save-name keys, including the one
    /// legacy empty alias that carries state. Never guesses between two payloads.</summary>
    internal static class ElementalComponentIdentityPolicy
    {
        internal static string[] Plan(string[] names, string[] semanticKeys,
            bool[] hasSavedFields)
        {
            if (names == null || semanticKeys == null || hasSavedFields == null ||
                names.Length != semanticKeys.Length || names.Length != hasSavedFields.Length ||
                names.Any(value => value == null) ||
                semanticKeys.Any(string.IsNullOrWhiteSpace) ||
                semanticKeys.Distinct(StringComparer.Ordinal).Count() != semanticKeys.Length)
                throw new ArgumentException("Component identity inputs must be complete and unambiguous.");
            string[] result = (string[])names.Clone();
            foreach (var group in Enumerable.Range(0, names.Length)
                .GroupBy(index => names[index], StringComparer.Ordinal).Where(value => value.Count() > 1))
            {
                if (group.Key.Length != 0)
                    throw new InvalidOperationException("A duplicate named save component requires explicit migration.");
                int[] stateful = group.Where(index => hasSavedFields[index]).ToArray();
                if (stateful.Length > 1)
                    throw new InvalidOperationException("Cannot disambiguate multiple saved payloads sharing one legacy key.");
                int legacyAlias = stateful.Length == 1 ? stateful[0] : group.First();
                foreach (int index in group.Where(value => value != legacyAlias))
                    result[index] = "$KMG.Elemental." + semanticKeys[index];
            }
            if (result.Distinct(StringComparer.Ordinal).Count() != result.Length)
                throw new InvalidOperationException("A generated component name conflicts with an existing save key.");
            return result;
        }
    }
}
