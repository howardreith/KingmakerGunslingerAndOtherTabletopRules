using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherSpellListMergePolicy
    {
        internal static List<T> Merge<T>(IList<T> current, T spell,
            Func<T, string> guid) where T : class
        {
            if (spell == null) throw new ArgumentNullException("spell");
            if (guid == null) throw new ArgumentNullException("guid");
            string spellGuid = guid(spell);
            if (string.IsNullOrWhiteSpace(spellGuid))
                throw new InvalidOperationException("Shield Other has no stable GUID.");
            var result = new List<T>();
            if (current != null)
                foreach (T value in current)
                {
                    if (value == null)
                        throw new InvalidOperationException("Spell list contains a null entry.");
                    if (!ReferenceEquals(value, spell) && !string.Equals(
                        guid(value), spellGuid, StringComparison.Ordinal))
                        result.Add(value);
                }
            result.Add(spell);
            return result;
        }

        internal static bool CanRollback<T>(IList<T> current,
            IList<T> published) where T : class
        { return ReferenceEquals(current, published); }
    }
}
