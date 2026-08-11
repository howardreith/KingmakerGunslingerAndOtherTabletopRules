using System;

namespace KingmakerGunslinger.Summoning
{
    public enum SummonAlignmentMode
    {
        Celestial,
        Fiendish,
        Caster
    }

    internal static class SummonAlignmentRuntimePolicy
    {
        private const int NeutralAxis = 1;
        private const int Good = 2;
        private const int Evil = 4;
        private const int Lawful = 8;
        private const int Chaotic = 16;

        private static readonly int[] ExactAlignments = {
            1, 3, 5, 9, 10, 12, 17, 18, 20
        };

        internal static bool TryResolve(SummonAlignmentMode mode,
            int ownerAlignment, int? casterAlignment, out int result)
        {
            result = ownerAlignment;
            if (!IsExact(ownerAlignment)) return false;
            if (mode == SummonAlignmentMode.Caster)
            {
                if (!casterAlignment.HasValue ||
                    !IsExact(casterAlignment.Value)) return false;
                result = casterAlignment.Value;
                return true;
            }
            if (mode != SummonAlignmentMode.Celestial &&
                mode != SummonAlignmentMode.Fiendish) return false;
            int axis = ownerAlignment & (Lawful | Chaotic);
            if (axis == 0) axis = NeutralAxis;
            result = axis | (mode == SummonAlignmentMode.Celestial ? Good : Evil);
            return IsExact(result);
        }

        internal static bool IsExact(int alignment)
        {
            return Array.IndexOf(ExactAlignments, alignment) >= 0;
        }
    }
}
