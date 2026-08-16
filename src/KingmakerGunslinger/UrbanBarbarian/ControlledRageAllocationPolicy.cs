using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal enum ControlledRageTier
    {
        Ordinary = 4,
        Greater = 6,
        Mighty = 8
    }

    internal static class ControlledRageAllocationPolicy
    {
        internal static IReadOnlyList<ControlledRageAllocation> Generate(
            ControlledRageTier tier)
        {
            int total = (int)tier;
            RequireTier(total);
            var result = new List<ControlledRageAllocation>();

            Add(result, total, 0, 0);
            Add(result, 0, total, 0);
            Add(result, 0, 0, total);

            if (total == 4)
            {
                Add(result, 2, 2, 0);
                Add(result, 2, 0, 2);
                Add(result, 0, 2, 2);
            }
            else if (total == 6)
            {
                AddOrderedTwoScore(result, 4, 2);
                Add(result, 2, 2, 2);
            }
            else
            {
                AddOrderedTwoScore(result, 6, 2);
                Add(result, 4, 4, 0);
                Add(result, 4, 0, 4);
                Add(result, 0, 4, 4);
                Add(result, 4, 2, 2);
                Add(result, 2, 4, 2);
                Add(result, 2, 2, 4);
            }

            int expected = total == 4 ? 6 : total == 6 ? 10 : 15;
            if (result.Count != expected || result.Distinct().Count() != expected)
                throw new InvalidOperationException(
                    "Controlled Rage allocation generation is incomplete or duplicated.");
            return result.AsReadOnly();
        }

        internal static ControlledRageTier ResolveTier(bool hasGreaterRage,
            bool hasMightyRage)
        {
            if (hasMightyRage) return ControlledRageTier.Mighty;
            if (hasGreaterRage) return ControlledRageTier.Greater;
            return ControlledRageTier.Ordinary;
        }

        internal static ControlledRageAllocation Default(ControlledRageTier tier)
        {
            return Generate(tier)[0];
        }

        internal static bool IsLegalForTier(ControlledRageTier tier,
            ControlledRageAllocation allocation)
        {
            return allocation != null && Generate(tier).Contains(allocation);
        }

        private static void AddOrderedTwoScore(
            ICollection<ControlledRageAllocation> result, int high, int low)
        {
            Add(result, high, low, 0);
            Add(result, high, 0, low);
            Add(result, low, high, 0);
            Add(result, 0, high, low);
            Add(result, low, 0, high);
            Add(result, 0, low, high);
        }

        private static void Add(ICollection<ControlledRageAllocation> result,
            int strength, int dexterity, int constitution)
        {
            result.Add(new ControlledRageAllocation(strength, dexterity,
                constitution));
        }

        private static void RequireTier(int total)
        {
            if (total != 4 && total != 6 && total != 8)
                throw new ArgumentOutOfRangeException("tier");
        }
    }
}
