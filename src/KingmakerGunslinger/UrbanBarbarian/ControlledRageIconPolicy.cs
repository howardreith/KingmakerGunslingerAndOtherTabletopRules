using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal enum ControlledRageIconGlyph
    {
        Strength,
        Dexterity,
        Constitution
    }

    internal sealed class ControlledRageIconSpec
    {
        internal ControlledRageIconSpec(ControlledRageAllocation allocation,
            IEnumerable<ControlledRageIconGlyph> glyphs)
        {
            if (allocation == null) throw new ArgumentNullException("allocation");
            Allocation = allocation;
            Glyphs = glyphs == null ? new ControlledRageIconGlyph[0] :
                glyphs.ToArray();
            if (Glyphs.Count != allocation.Total / 2)
                throw new ArgumentException(
                    "One icon glyph is required for every +2 increment.",
                    "glyphs");
            Key = "T" + allocation.Total + "_S" + allocation.Strength +
                "_D" + allocation.Dexterity + "_C" + allocation.Constitution;
        }

        internal ControlledRageAllocation Allocation { get; private set; }
        internal IReadOnlyList<ControlledRageIconGlyph> Glyphs {
            get; private set;
        }
        internal string Key { get; private set; }
        internal int TileCount { get { return Glyphs.Count; } }
        internal bool UsesNativeDonor { get {
            return Glyphs.Distinct().Count() == 1;
        } }
        internal ControlledRageIconGlyph NativeDonor { get {
            if (!UsesNativeDonor) throw new InvalidOperationException(
                "Mixed allocations do not have one native donor.");
            return Glyphs[0];
        } }
    }

    internal static class ControlledRageIconPolicy
    {
        internal static ControlledRageIconSpec Describe(
            ControlledRageAllocation allocation)
        {
            if (allocation == null || !ControlledRageAllocationPolicy.Generate(
                    (ControlledRageTier)allocation.Total).Contains(allocation))
                throw new ArgumentException(
                    "A legal Controlled Rage allocation is required.",
                    "allocation");
            var glyphs = new List<ControlledRageIconGlyph>();
            Append(glyphs, ControlledRageIconGlyph.Strength,
                allocation.Strength / 2);
            Append(glyphs, ControlledRageIconGlyph.Dexterity,
                allocation.Dexterity / 2);
            Append(glyphs, ControlledRageIconGlyph.Constitution,
                allocation.Constitution / 2);
            return new ControlledRageIconSpec(allocation, glyphs);
        }

        private static void Append(ICollection<ControlledRageIconGlyph> target,
            ControlledRageIconGlyph glyph, int count)
        {
            for (int index = 0; index < count; index++) target.Add(glyph);
        }
    }
}
