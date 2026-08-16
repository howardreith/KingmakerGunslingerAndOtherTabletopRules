using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal sealed class UrbanBarbarianPublication
    {
        private readonly BlueprintCharacterClass _barbarian;
        private readonly BlueprintArchetype _urban;
        private readonly BlueprintArchetype[] _before;
        private readonly BlueprintArchetype[] _after;
        private bool _applied;

        private UrbanBarbarianPublication(BlueprintCharacterClass barbarian,
            BlueprintArchetype urban, bool publish)
        {
            _barbarian = barbarian ?? throw new ArgumentNullException("barbarian");
            _urban = urban ?? throw new ArgumentNullException("urban");
            _before = (barbarian.Archetypes ?? Array.Empty<BlueprintArchetype>())
                .ToArray();
            Validate(_before);
            BlueprintArchetype[] sameGuid = _before.Where(value =>
                string.Equals(value.AssetGuid, urban.AssetGuid,
                    StringComparison.Ordinal)).ToArray();
            if (sameGuid.Any(value => !ReferenceEquals(value, urban)))
                throw new InvalidOperationException(
                    "The Barbarian archetype catalog contains a foreign Urban Barbarian GUID collision.");
            var next = _before.Where(value => !ReferenceEquals(value, urban) &&
                !string.Equals(value.AssetGuid, urban.AssetGuid,
                    StringComparison.Ordinal)).ToList();
            if (publish) next.Add(urban);
            _after = next.ToArray();
            Validate(_after);
            if (!Same(_before, _after))
            {
                barbarian.Archetypes = _after;
                if (!Same(barbarian.Archetypes, _after))
                    throw new InvalidOperationException(
                        "Urban Barbarian archetype publication was not retained.");
                _applied = true;
            }
        }

        internal static UrbanBarbarianPublication Apply(
            BlueprintCharacterClass barbarian, BlueprintArchetype urban,
            bool publish)
        { return new UrbanBarbarianPublication(barbarian, urban, publish); }

        internal bool Published
        {
            get
            {
                return (_barbarian.Archetypes ?? Array.Empty<BlueprintArchetype>())
                    .Count(value => ReferenceEquals(value, _urban) || value != null &&
                        string.Equals(value.AssetGuid, _urban.AssetGuid,
                            StringComparison.Ordinal)) == 1;
            }
        }

        internal void Rollback()
        {
            if (!_applied) return;
            BlueprintArchetype[] current = _barbarian.Archetypes ??
                Array.Empty<BlueprintArchetype>();
            if (Same(current, _before)) { _applied = false; return; }
            if (!StartsWith(current, _after))
                throw new InvalidOperationException(
                    "Urban Barbarian rollback refused after an unrelated catalog mutation.");
            var restored = new List<BlueprintArchetype>(_before);
            for (int index = _after.Length; index < current.Length; index++)
                restored.Add(current[index]);
            BlueprintArchetype[] value = restored.ToArray();
            Validate(value);
            _barbarian.Archetypes = value;
            if (!Same(_barbarian.Archetypes, value))
                throw new InvalidOperationException(
                    "Urban Barbarian archetype rollback was not retained.");
            _applied = false;
        }

        private static void Validate(IEnumerable<BlueprintArchetype> values)
        {
            if (values == null) throw new InvalidOperationException(
                "The Barbarian archetype catalog is unavailable.");
            var refs = new HashSet<BlueprintArchetype>(ReferenceComparer.Instance);
            var guids = new HashSet<string>(StringComparer.Ordinal);
            foreach (BlueprintArchetype value in values)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.AssetGuid) ||
                    !refs.Add(value) || !guids.Add(value.AssetGuid))
                    throw new InvalidOperationException(
                        "The Barbarian archetype catalog contains null or duplicate identities.");
            }
        }

        private static bool Same(IList<BlueprintArchetype> left,
            IList<BlueprintArchetype> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
                if (!ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        private static bool StartsWith(IList<BlueprintArchetype> value,
            IList<BlueprintArchetype> prefix)
        {
            if (value == null || prefix == null || value.Count < prefix.Count)
                return false;
            for (int index = 0; index < prefix.Count; index++)
                if (!ReferenceEquals(value[index], prefix[index])) return false;
            return true;
        }

        private sealed class ReferenceComparer :
            IEqualityComparer<BlueprintArchetype>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(BlueprintArchetype x, BlueprintArchetype y)
            { return ReferenceEquals(x, y); }
            public int GetHashCode(BlueprintArchetype obj)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
