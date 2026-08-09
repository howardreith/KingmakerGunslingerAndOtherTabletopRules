using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.Ammunition;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable inputs used to validate item-state transitions. These rules are
    /// supplied by callers so FirearmState never owns a blueprint or game object.
    /// </summary>
    internal sealed class FirearmStateRules
    {
        private readonly int _capacity;
        private readonly AmmunitionId[] _compatibleAmmunition;
        private readonly HashSet<string> _compatibleValues;

        internal FirearmStateRules(int capacity, IEnumerable<AmmunitionId> compatibleAmmunition)
        {
            if (capacity < FirearmDefinition.MinimumCapacity ||
                capacity > FirearmDefinition.MaximumCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    "capacity",
                    capacity,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "State capacity must be in the range {0}..{1}.",
                        FirearmDefinition.MinimumCapacity,
                        FirearmDefinition.MaximumCapacity));
            }

            if (compatibleAmmunition == null)
            {
                throw new ArgumentNullException("compatibleAmmunition");
            }

            List<AmmunitionId> values = new List<AmmunitionId>();
            HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (AmmunitionId ammunition in compatibleAmmunition)
            {
                if (ammunition == null)
                {
                    throw new ArgumentException(
                        "Compatible ammunition cannot contain a null entry.",
                        "compatibleAmmunition");
                }

                if (!unique.Add(ammunition.Value))
                {
                    throw new ArgumentException(
                        "Compatible ammunition cannot contain duplicate IDs.",
                        "compatibleAmmunition");
                }

                values.Add(ammunition);
            }

            if (values.Count == 0)
            {
                throw new ArgumentException(
                    "At least one compatible ammunition ID is required.",
                    "compatibleAmmunition");
            }

            values.Sort();
            _capacity = capacity;
            _compatibleAmmunition = values.ToArray();
            _compatibleValues = unique;
        }

        internal int Capacity
        {
            get { return _capacity; }
        }

        internal static FirearmStateRules CreateForDefinition(
            FirearmDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            var compatible = new List<AmmunitionId>
            {
                definition.Reload.Ammunition
            };
            if (ReloadAmmunitionProfileCatalog.PaperCartridge.IsCompatible(definition))
                compatible.Add(ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition);
            return new FirearmStateRules(definition.Capacity, compatible);
        }

        internal int CompatibleAmmunitionCount
        {
            get { return _compatibleAmmunition.Length; }
        }

        internal AmmunitionId[] GetCompatibleAmmunition()
        {
            return (AmmunitionId[])_compatibleAmmunition.Clone();
        }

        internal bool IsCompatible(AmmunitionId ammunition)
        {
            if (ammunition == null)
            {
                return false;
            }

            return _compatibleValues.Contains(ammunition.Value);
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "capacity={0}; compatible=[{1}]",
                _capacity,
                string.Join(",", _compatibleAmmunition.Select(value => value.Value).ToArray()));
        }
    }
}
