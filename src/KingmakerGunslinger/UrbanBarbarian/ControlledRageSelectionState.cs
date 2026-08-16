using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal sealed class ControlledRageSelectionState
    {
        private readonly Dictionary<ControlledRageTier, ControlledRageAllocation>
            _selections = new Dictionary<ControlledRageTier, ControlledRageAllocation>();

        internal ControlledRageSelectionState()
        {
            Unlock(ControlledRageTier.Ordinary);
        }

        internal ControlledRageTier CurrentTier
        {
            get { return _selections.Keys.OrderByDescending(value => (int)value).First(); }
        }

        internal ControlledRageAllocation CurrentSelection
        {
            get { return _selections[CurrentTier]; }
        }

        internal IReadOnlyList<ControlledRageAllocation> VisibleAllocations
        {
            get { return ControlledRageAllocationPolicy.Generate(CurrentTier); }
        }

        internal void Unlock(ControlledRageTier tier)
        {
            if (!_selections.ContainsKey(tier))
                _selections.Add(tier, ControlledRageAllocationPolicy.Default(tier));
        }

        internal bool TrySelect(ControlledRageTier tier,
            ControlledRageAllocation allocation, bool rageActive)
        {
            if (rageActive || tier != CurrentTier ||
                !ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation))
                return false;
            _selections[tier] = allocation;
            return true;
        }

        internal ControlledRageAllocation SelectionFor(ControlledRageTier tier)
        {
            ControlledRageAllocation value;
            return _selections.TryGetValue(tier, out value) ? value : null;
        }

        internal string Serialize()
        {
            return string.Join(";", _selections.OrderBy(value => (int)value.Key)
                .Select(value => string.Format(CultureInfo.InvariantCulture,
                    "{0}:{1},{2},{3}", (int)value.Key, value.Value.Strength,
                    value.Value.Dexterity, value.Value.Constitution)).ToArray());
        }

        internal static ControlledRageSelectionState Parse(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
                throw new ArgumentException("Persisted Controlled Rage state is required.",
                    "serialized");
            var result = new ControlledRageSelectionState();
            result._selections.Clear();
            foreach (string entry in serialized.Split(';'))
            {
                string[] parts = entry.Split(':');
                string[] values = parts.Length == 2 ? parts[1].Split(',') :
                    new string[0];
                int tierValue, strength, dexterity, constitution;
                if (values.Length != 3 ||
                    !int.TryParse(parts[0], NumberStyles.None,
                        CultureInfo.InvariantCulture, out tierValue) ||
                    !int.TryParse(values[0], NumberStyles.None,
                        CultureInfo.InvariantCulture, out strength) ||
                    !int.TryParse(values[1], NumberStyles.None,
                        CultureInfo.InvariantCulture, out dexterity) ||
                    !int.TryParse(values[2], NumberStyles.None,
                        CultureInfo.InvariantCulture, out constitution) ||
                    (tierValue != 4 && tierValue != 6 && tierValue != 8))
                    throw new FormatException("Invalid persisted Controlled Rage state.");
                var tier = (ControlledRageTier)tierValue;
                var allocation = new ControlledRageAllocation(strength, dexterity,
                    constitution);
                if (!ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation) ||
                    result._selections.ContainsKey(tier))
                    throw new FormatException("Invalid persisted Controlled Rage allocation.");
                result._selections.Add(tier, allocation);
            }
            if (!result._selections.ContainsKey(ControlledRageTier.Ordinary))
                throw new FormatException("Ordinary Controlled Rage state is missing.");
            return result;
        }
    }
}
