using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair sprint R1 (owner manual playtest, 2026-11-07): the five
    /// capabilities the owner withdrew from this release, in one place.
    ///
    /// <para>These are settled product cuts, not deferred defects. Each one
    /// removes the player-visible or automatic route and nothing else: every
    /// registered identity stays allocated and configured so an older save's
    /// references still resolve, and the retained implementation stays
    /// available for possible future use.</para>
    ///
    /// <list type="bullet">
    /// <item>CUT-01 Stirge: withheld from every public summon route through
    /// <see cref="SummonVisibilityCatalog"/>. The visibility catalog keeps its
    /// own literal set, which the offline manifest tooling parses from source;
    /// this policy names the same key so the two can be checked against each
    /// other.</item>
    /// <item>CUT-02 Wall of Ice: no grant, spell-like parameters or AI action
    /// on Bone Devil or Ice Devil. The Wall ability, prototype section,
    /// residual area and each devil's InfernalWallSourceLifetime35 remain so a
    /// pre-existing wall still retires through its unchanged lifetime.</item>
    /// <item>CUT-03 Magma Form: Magma Mephit's second spell-like slot is not
    /// granted, its resource is not exposed and its AI action is not in the
    /// brain. Breath and Pyrotechnics are unchanged.</item>
    /// <item>CUT-04 Lillend Bardic Suggestion: the separate performance
    /// ability is not granted and the noncombat dispatcher no longer routes to
    /// it. The Suggestion spell and its shared optional-provider binding are
    /// unchanged.</item>
    /// <item>CUT-05 Fire Giant Sunder: the native Improved Sunder feat is not
    /// granted to the KMG Fire Giant. That feat's own AddFacts are the source
    /// of both the native Sunder Armor action and ProperFlanking2's default-on
    /// attack-replacement toggle, so removing the feat removes both. Storm
    /// Giant keeps its separate grant.</item>
    /// </list>
    ///
    /// <para>Kingmaker persists a unit's granted facts and its brain's action
    /// list (<c>UnitBrain.Actions</c> is a JSON property), so a summon saved
    /// before this change would otherwise keep the withdrawn capability. The
    /// unit-blueprint <c>FixUnitOnPostLoad</c> component
    /// <c>SummonWithdrawnCapabilityFixer</c> removes exactly the listed facts
    /// and actions from an exact KMG-owned unit on load, using
    /// <see cref="SelectWithdrawn{T}"/>.</para>
    /// </summary>
    internal static class SummonPlaytestWithdrawalPolicy
    {
        internal const string StirgeKey = "stirge";

        /// <summary>Stirge enters at Summon Nature's Ally I with no Summon
        /// Monster row, so it occupies parents I-IX of that one family.</summary>
        internal static readonly int[] StirgeWithheldNaturesAllyTiers =
            { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        internal const string MagmaMephitKey = "magma-mephit";
        internal const string MagmaFormKind = "MagmaForm";

        internal const string NativeImprovedSunderGuid =
            "9719015edcbf142409592e2cbaab7fe1";

        /// <summary>The native fixer records each component name once per
        /// unit, so this name must stay fixed. A later withdrawal needs a new
        /// fixer name; reusing this one would not run on already fixed
        /// units.</summary>
        internal const string FixerNamePrefix = "$KMG_R1_WithdrawnCapabilities_";

        private static readonly WithdrawnCapabilitySpec[] Specs =
        {
            new WithdrawnCapabilitySpec("bone-devil", "KMG.Summoning.Unit.BoneDevil",
                new[] { "KMG.Summoning.Infernal.BoneDevil.WallOfIce" },
                new[] { "KMG.Summoning.Infernal.BoneDevil.WallAi" },
                Array.Empty<string>(), Array.Empty<string>()),
            new WithdrawnCapabilitySpec("ice-devil", "KMG.Summoning.Unit.IceDevil",
                new[] { "KMG.Summoning.Infernal.IceDevil.WallOfIce" },
                new[] { "KMG.Summoning.Infernal.IceDevil.WallOfIceAi" },
                Array.Empty<string>(), Array.Empty<string>()),
            new WithdrawnCapabilitySpec(MagmaMephitKey, "KMG.Summoning.Unit.MagmaMephit",
                new[] { "KMG.Summoning.Special.MagmaMephit.SpellLikeTwo" },
                new[] { "KMG.Summoning.Special.MagmaMephit.SpellLikeTwoAi" },
                new[] { "KMG.Summoning.Special.MagmaMephit.SpellLikeTwoResource" },
                Array.Empty<string>()),
            new WithdrawnCapabilitySpec("lillend-azata", "KMG.Summoning.Unit.LillendAzata",
                new[] { "KMG.Summoning.FeyCelestial.LillendAzata.BardicSuggestion" },
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
            new WithdrawnCapabilitySpec("fire-giant", "KMG.Summoning.Unit.FireGiant",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
                new[] { NativeImprovedSunderGuid }),
        };

        internal static IReadOnlyList<WithdrawnCapabilitySpec> All
        { get { return Array.AsReadOnly(Specs); } }

        internal static WithdrawnCapabilitySpec For(string creatureKey)
        {
            return Specs.SingleOrDefault(value => string.Equals(value.CreatureKey,
                creatureKey, StringComparison.Ordinal));
        }

        internal static bool IsWithdrawnMephitSpellLike(string mephitKey, string kind)
        {
            return string.Equals(mephitKey, MagmaMephitKey, StringComparison.Ordinal) &&
                string.Equals(kind, MagmaFormKind, StringComparison.Ordinal);
        }

        /// <summary>The native feat list a creature is granted, minus any
        /// native feat this policy withdraws from that creature alone.</summary>
        internal static string[] GrantedNativeFeats(string creatureKey,
            IEnumerable<string> printedFeats)
        {
            if (printedFeats == null) throw new ArgumentNullException("printedFeats");
            WithdrawnCapabilitySpec spec = For(creatureKey);
            return printedFeats.Where(guid => spec == null ||
                !spec.NativeFeatGuids.Contains(guid, StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }

        internal static string FixerName(string creatureKey)
        {
            WithdrawnCapabilitySpec spec = For(creatureKey);
            if (spec == null) throw new ArgumentException("No withdrawal for " + creatureKey, "creatureKey");
            return FixerNamePrefix + spec.UnitSymbol.Substring(spec.UnitSymbol.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// The reconciliation decision for one loaded unit: the present items
        /// whose identity is withdrawn, and only when the unit is the exact
        /// owning KMG blueprint. Identity is reference equality, so a native
        /// or foreign unit that shares a native feat is never touched and a
        /// same-named different object is never matched.
        /// </summary>
        internal static T[] SelectWithdrawn<T>(bool exactOwner,
            IEnumerable<T> present, IEnumerable<T> withdrawn) where T : class
        {
            if (!exactOwner || present == null || withdrawn == null) return Array.Empty<T>();
            T[] targets = withdrawn.Where(value => value != null).ToArray();
            if (targets.Length == 0) return Array.Empty<T>();
            return present.Where(value => value != null &&
                targets.Any(target => ReferenceEquals(target, value))).ToArray();
        }
    }

    internal sealed class WithdrawnCapabilitySpec
    {
        internal WithdrawnCapabilitySpec(string creatureKey, string unitSymbol,
            string[] factSymbols, string[] aiActionSymbols, string[] resourceSymbols,
            string[] nativeFeatGuids)
        {
            CreatureKey = creatureKey; UnitSymbol = unitSymbol;
            FactSymbols = Array.AsReadOnly(factSymbols);
            AiActionSymbols = Array.AsReadOnly(aiActionSymbols);
            ResourceSymbols = Array.AsReadOnly(resourceSymbols);
            NativeFeatGuids = Array.AsReadOnly(nativeFeatGuids);
        }

        internal string CreatureKey { get; private set; }
        internal string UnitSymbol { get; private set; }
        /// <summary>KMG-owned facts (abilities) no longer granted.</summary>
        internal IReadOnlyList<string> FactSymbols { get; private set; }
        /// <summary>KMG-owned AI actions no longer in the brain.</summary>
        internal IReadOnlyList<string> AiActionSymbols { get; private set; }
        /// <summary>KMG-owned resources no longer exposed.</summary>
        internal IReadOnlyList<string> ResourceSymbols { get; private set; }
        /// <summary>Native feats no longer granted to this creature alone.</summary>
        internal IReadOnlyList<string> NativeFeatGuids { get; private set; }
    }
}
