using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    internal enum ProductionFirearmAcquisitionRole
    {
        OrdinaryCampaignCraftingBase = 0,
        LegacyRecognitionOnly = 1
    }

    /// <summary>
    /// Immutable, engine-independent weapon statistics for one production firearm.
    /// This keeps tabletop damage, critical, handedness, cost, and weight explicit
    /// instead of inheriting them accidentally from a Kingmaker presentation clone.
    /// </summary>
    internal sealed class ProductionFirearmWeaponSpec : IEquatable<ProductionFirearmWeaponSpec>
    {
        internal ProductionFirearmWeaponSpec(
            string key,
            string displayName,
            FirearmDefinition definition,
            int damageDiceCount,
            int damageDieSides,
            int criticalMultiplier,
            bool isTwoHanded,
            int costGold,
            float weightPounds,
            bool isPlayerFireable,
            ProductionFirearmAcquisitionRole acquisitionRole)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A stable production firearm key is required.", "key");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("A production firearm display name is required.", "displayName");
            }

            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            if (damageDiceCount < 1 || damageDiceCount > 16)
            {
                throw new ArgumentOutOfRangeException("damageDiceCount");
            }

            if (damageDieSides != 2 && damageDieSides != 3 && damageDieSides != 4 &&
                damageDieSides != 6 && damageDieSides != 8 && damageDieSides != 10 &&
                damageDieSides != 12)
            {
                throw new ArgumentOutOfRangeException("damageDieSides");
            }

            if (criticalMultiplier < 2 || criticalMultiplier > 4)
            {
                throw new ArgumentOutOfRangeException("criticalMultiplier");
            }

            bool requiresTwoHands = FirearmHandednessPolicy.Require(
                definition.Kind) == FirearmHandedness.TwoHanded;
            if (requiresTwoHands != isTwoHanded)
            {
                throw new ArgumentException(
                    "Production firearm handedness does not match its firearm kind.",
                    "isTwoHanded");
            }

            if (costGold < 0)
            {
                throw new ArgumentOutOfRangeException("costGold");
            }

            if (float.IsNaN(weightPounds) || float.IsInfinity(weightPounds) || weightPounds < 0f)
            {
                throw new ArgumentOutOfRangeException("weightPounds");
            }

            if (isPlayerFireable && !definition.HasFixedRangeIncrement &&
                !definition.IsScatter)
            {
                throw new ArgumentException(
                    "Only qualified scatter content may be player-fireable without a fixed range increment.",
                    "isPlayerFireable");
            }

            Key = key.Trim();
            DisplayName = displayName.Trim();
            Definition = definition;
            DamageDiceCount = damageDiceCount;
            DamageDieSides = damageDieSides;
            CriticalMultiplier = criticalMultiplier;
            IsTwoHanded = isTwoHanded;
            CostGold = costGold;
            WeightPounds = weightPounds;
            IsPlayerFireable = isPlayerFireable;
            AcquisitionRole = acquisitionRole;
        }

        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal FirearmDefinition Definition { get; private set; }
        internal int DamageDiceCount { get; private set; }
        internal int DamageDieSides { get; private set; }
        internal int CriticalMultiplier { get; private set; }
        internal bool IsTwoHanded { get; private set; }
        internal int CostGold { get; private set; }
        internal float WeightPounds { get; private set; }
        internal bool IsPlayerFireable { get; private set; }
        internal ProductionFirearmAcquisitionRole AcquisitionRole
        { get; private set; }

        public bool Equals(ProductionFirearmWeaponSpec other)
        {
            return !ReferenceEquals(other, null) &&
                string.Equals(Key, other.Key, StringComparison.Ordinal) &&
                string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal) &&
                Equals(Definition, other.Definition) &&
                DamageDiceCount == other.DamageDiceCount &&
                DamageDieSides == other.DamageDieSides &&
                CriticalMultiplier == other.CriticalMultiplier &&
                IsTwoHanded == other.IsTwoHanded &&
                CostGold == other.CostGold &&
                WeightPounds.Equals(other.WeightPounds) &&
                IsPlayerFireable == other.IsPlayerFireable &&
                AcquisitionRole == other.AcquisitionRole;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProductionFirearmWeaponSpec);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = StringComparer.Ordinal.GetHashCode(Key);
                hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(DisplayName);
                hash = (hash * 31) + Definition.GetHashCode();
                hash = (hash * 31) + DamageDiceCount;
                hash = (hash * 31) + DamageDieSides;
                hash = (hash * 31) + CriticalMultiplier;
                hash = (hash * 31) + (IsTwoHanded ? 1 : 0);
                hash = (hash * 31) + CostGold;
                hash = (hash * 31) + WeightPounds.GetHashCode();
                hash = (hash * 31) + (IsPlayerFireable ? 1 : 0);
                hash = (hash * 31) + (int)AcquisitionRole;
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; damage={1}d{2}; critical=x{3}; twoHanded={4}; cost={5}gp; weight={6:0.###}lb; playerFireable={7}; definition=({8})",
                DisplayName,
                DamageDiceCount,
                DamageDieSides,
                CriticalMultiplier,
                IsTwoHanded,
                CostGold,
                WeightPounds,
                IsPlayerFireable,
                Definition);
        }
    }
}
