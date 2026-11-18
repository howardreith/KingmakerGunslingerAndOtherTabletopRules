using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// DEF-05 review finding 1: a diagnostic-only, independently stated
    /// contract for the donor carrier production records per placement. The
    /// gameplay policy never reads it. Template signatures are the native
    /// identities each (parent tier, quantity) carrier applied in the retained
    /// census (session 20261118T032921Z-owner-0.33.0-template-census-baseline-cotw,
    /// accepted product 16708bd5e237); that census did not retain donor asset
    /// identities or names, so those are recorded by the current census and
    /// only the family name and the tier-one direct parent are enforced by
    /// identity. Shapes the retained census never observed (they occur only for
    /// templated creatures: tier 1 single, tier 2 d3, tier 3 d4+1) are reported
    /// as recorded-now, not as proved.
    /// </summary>
    internal static class InheritedTemplateDonorContract
    {
        internal const string CelestialI = "83a8a909e7ad19b4fa57e306884cc8bd";
        internal const string FiendishI = "c707fdbe58d0c614a89a872b76777f4b";
        internal const string CelestialII = "ec906031bb8766f42a37a52cf1d89836";
        internal const string FiendishII = "4b808ccd7e0b7bd43b10b2d47733b1a4";
        internal const string MonsterTierOneParent = "8fd74eddd9b6c224693d9ab241f25e84";
        internal const string NaturesAllyTierOneParent = "c6147854641924442a3bb736080cfeb6";

        private static readonly Dictionary<string, string[]> RetainedMonsterSignatures =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { Key(2, SummonMultiplicity.One), new[] { CelestialI, FiendishI } },
            { Key(3, SummonMultiplicity.One), new[] { CelestialI, FiendishI } },
            { Key(3, SummonMultiplicity.OneD3), new[] { CelestialI, FiendishI } },
            { Key(4, SummonMultiplicity.One), new[] { FiendishI, CelestialII } },
            { Key(4, SummonMultiplicity.OneD3), new[] { CelestialI, FiendishI } },
            { Key(4, SummonMultiplicity.OneD4PlusOne), new[] { CelestialI, FiendishI } },
            { Key(5, SummonMultiplicity.One), new string[0] },
            { Key(5, SummonMultiplicity.OneD3), new[] { FiendishII, CelestialII } },
            { Key(5, SummonMultiplicity.OneD4PlusOne), new[] { CelestialI, FiendishI } },
            { Key(6, SummonMultiplicity.One), new string[0] },
            { Key(6, SummonMultiplicity.OneD3), new string[0] },
            { Key(6, SummonMultiplicity.OneD4PlusOne), new[] { FiendishII, CelestialII } },
            { Key(7, SummonMultiplicity.One), new string[0] },
            { Key(7, SummonMultiplicity.OneD3), new string[0] },
            { Key(7, SummonMultiplicity.OneD4PlusOne), new string[0] },
            { Key(8, SummonMultiplicity.One), new string[0] },
            { Key(8, SummonMultiplicity.OneD3), new string[0] },
            { Key(8, SummonMultiplicity.OneD4PlusOne), new string[0] },
            { Key(9, SummonMultiplicity.One), new string[0] },
            { Key(9, SummonMultiplicity.OneD3), new string[0] },
            { Key(9, SummonMultiplicity.OneD4PlusOne), new string[0] },
        };

        private static string Key(int tier, SummonMultiplicity multiplicity)
        {
            return tier + "." + multiplicity;
        }

        /// <summary>
        /// Null when the donor satisfies the contract; otherwise the reason.
        /// signatureRetained is false for a Summon Monster shape the retained
        /// census never observed.
        /// </summary>
        internal static string Check(SummonFamily family, int parentTier, SummonMultiplicity multiplicity,
            string donorName, string donorGuid, IEnumerable<string> donorApplied, out bool signatureRetained)
        {
            signatureRetained = false;
            string[] applied = (donorApplied ?? new string[0]).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (string.IsNullOrEmpty(donorName) || string.IsNullOrEmpty(donorGuid)) return "unidentified";
            string own = family == SummonFamily.Monster ? "SummonMonster" : "SummonNaturesAlly";
            string other = family == SummonFamily.Monster ? "SummonNaturesAlly" : "SummonMonster";
            if (donorName.IndexOf(own, StringComparison.OrdinalIgnoreCase) < 0 ||
                donorName.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0)
                return "family:" + donorName;
            if (parentTier == 1)
            {
                string direct = family == SummonFamily.Monster ? MonsterTierOneParent : NaturesAllyTierOneParent;
                if (!string.Equals(donorGuid, direct, StringComparison.Ordinal))
                    return "tier-one-identity:" + donorGuid;
            }
            if (family == SummonFamily.NaturesAlly)
            {
                signatureRetained = true;
                return applied.Length == 0 ? null : "signature:ally-applies:" + string.Join(",", applied);
            }
            string[] expected;
            if (!RetainedMonsterSignatures.TryGetValue(Key(parentTier, multiplicity), out expected)) return null;
            signatureRetained = true;
            string[] sorted = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return sorted.SequenceEqual(applied, StringComparer.Ordinal) ? null :
                "signature:expected=" + string.Join(",", sorted) + ";actual=" + string.Join(",", applied);
        }
    }

    /// <summary>
    /// DEF-05 review finding 3: an immutable receipt of a donor carrier taken
    /// before any clone or publication, compared with the same donor later. It
    /// holds references (not copies) in traversal order, so a same-count
    /// replacement, a reorder or an array swap is visible, and the buff
    /// identities as strings, so an in-place ApplyBuff change is visible.
    /// </summary>
    internal sealed class InheritedTemplateDonorReceipt
    {
        internal InheritedTemplateDonorReceipt(string guid, string name, object componentsArray, object[] components,
            object[] spawns, object[] afterSpawnArrays, object[][] afterSpawnElements, object[] actions,
            string[] actionTypes, string[] applyBuffIds, string[] nativeSequence)
        {
            Guid = guid; Name = name; ComponentsArray = componentsArray;
            Components = components ?? new object[0]; Spawns = spawns ?? new object[0];
            AfterSpawnArrays = afterSpawnArrays ?? new object[0];
            AfterSpawnElements = afterSpawnElements ?? new object[0][];
            Actions = actions ?? new object[0]; ActionTypes = actionTypes ?? new string[0];
            ApplyBuffIds = applyBuffIds ?? new string[0]; NativeSequence = nativeSequence ?? new string[0];
        }

        internal string Guid { get; private set; }
        internal string Name { get; private set; }
        internal object ComponentsArray { get; private set; }
        internal object[] Components { get; private set; }
        internal object[] Spawns { get; private set; }
        internal object[] AfterSpawnArrays { get; private set; }
        internal object[][] AfterSpawnElements { get; private set; }
        internal object[] Actions { get; private set; }
        internal string[] ActionTypes { get; private set; }
        internal string[] ApplyBuffIds { get; private set; }
        internal string[] NativeSequence { get; private set; }

        private static bool SameReferences(object[] left, object[] right)
        {
            if (left.Length != right.Length) return false;
            for (int index = 0; index < left.Length; index++)
                if (!ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        internal static IList<string> Differences(InheritedTemplateDonorReceipt before, InheritedTemplateDonorReceipt after)
        {
            if (before == null) throw new ArgumentNullException("before");
            if (after == null) throw new ArgumentNullException("after");
            var result = new List<string>();
            if (!string.Equals(before.Guid, after.Guid, StringComparison.Ordinal)) result.Add("guid");
            if (!string.Equals(before.Name, after.Name, StringComparison.Ordinal)) result.Add("name");
            if (!ReferenceEquals(before.ComponentsArray, after.ComponentsArray)) result.Add("components-array");
            if (!SameReferences(before.Components, after.Components)) result.Add("components");
            if (!SameReferences(before.Spawns, after.Spawns)) result.Add("spawns");
            if (!SameReferences(before.AfterSpawnArrays, after.AfterSpawnArrays)) result.Add("after-spawn-arrays");
            if (before.AfterSpawnElements.Length != after.AfterSpawnElements.Length ||
                before.AfterSpawnElements.Where((value, index) => !SameReferences(value, after.AfterSpawnElements[index])).Any())
                result.Add("after-spawn-actions");
            if (!SameReferences(before.Actions, after.Actions)) result.Add("actions");
            if (!before.ActionTypes.SequenceEqual(after.ActionTypes, StringComparer.Ordinal)) result.Add("action-types");
            if (!before.ApplyBuffIds.SequenceEqual(after.ApplyBuffIds, StringComparer.Ordinal)) result.Add("apply-buff-ids");
            if (!before.NativeSequence.SequenceEqual(after.NativeSequence, StringComparer.Ordinal)) result.Add("native-sequence");
            return result;
        }
    }
}
