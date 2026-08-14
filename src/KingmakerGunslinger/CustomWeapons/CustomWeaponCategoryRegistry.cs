using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.CustomWeapons
{
    internal sealed class CustomWeaponCategoryRegistry
    {
        private readonly Dictionary<int, CustomWeaponCategoryDefinition> _byValue =
            new Dictionary<int, CustomWeaponCategoryDefinition>();
        private readonly Dictionary<string, CustomWeaponCategoryDefinition> _byKey =
            new Dictionary<string, CustomWeaponCategoryDefinition>(
                StringComparer.Ordinal);

        internal void Add(CustomWeaponCategoryDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (_byValue.ContainsKey(definition.CategoryValue))
                throw new InvalidOperationException("Custom weapon category value collision: " +
                    definition.CategoryValue + ".");
            if (_byKey.ContainsKey(definition.Key))
                throw new InvalidOperationException("Custom weapon category key collision: " +
                    definition.Key + ".");
            _byValue.Add(definition.CategoryValue, definition);
            _byKey.Add(definition.Key, definition);
        }

        internal CustomWeaponCategoryDefinition Require(int categoryValue)
        {
            CustomWeaponCategoryDefinition result;
            if (!_byValue.TryGetValue(categoryValue, out result))
                throw new KeyNotFoundException("Unknown custom weapon category " +
                    categoryValue + ".");
            return result;
        }

        internal bool TryGet(int categoryValue,
            out CustomWeaponCategoryDefinition definition)
        { return _byValue.TryGetValue(categoryValue, out definition); }

        internal CustomWeaponCategoryDefinition[] All
        { get { return _byValue.Values.OrderBy(value => value.CategoryValue).ToArray(); } }

        internal void ValidateLoadedValues(IEnumerable<KeyValuePair<int, string>> loaded)
        {
            if (loaded == null) throw new ArgumentNullException("loaded");
            foreach (KeyValuePair<int, string> candidate in loaded)
            {
                CustomWeaponCategoryDefinition owned;
                if (_byValue.TryGetValue(candidate.Key, out owned))
                    throw new InvalidOperationException("Loaded weapon type '" +
                        (candidate.Value ?? "<unnamed>") + "' collides with KMG category " +
                        owned.Key + " (" + candidate.Key + ").");
            }
        }
    }
}
