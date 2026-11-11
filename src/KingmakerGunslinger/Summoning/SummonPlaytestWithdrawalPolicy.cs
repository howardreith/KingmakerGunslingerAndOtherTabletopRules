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
    /// attack-replacement toggle, so removing the feat removes both.</item>
    /// <item>SUNDER-01 (owner decision, 2026-11-10, superseding CUT-05's
    /// Storm Giant exception): no summon offered through this module's Summon
    /// Monster / Nature's Ally routes keeps or uses Sunder. Storm Giant loses
    /// its own Improved Sunder grant. The Greater and Elder Earth Elementals
    /// inherited the native feat and the native GreaterEarthElementalBrain
    /// (whose actions are SunderArmorAiAction and an attack scored around
    /// sundered targets) from their native donors; each KMG unit now omits
    /// the feat and uses the unmodified native Dumb Monster Brain that the
    /// Huge Earth Elemental already uses. Shared native feats, abilities,
    /// brains and donors are never modified.</item>
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

        /// <summary>Native SunderArmorAiAction: casts the native SunderAction.
        /// Its exact installed type is BlueprintAiCastSpell (RequireExact does
        /// not accept a base type; candidate c6e861a98a failed closed on it).</summary>
        internal const string NativeSunderArmorAiActionGuid =
            "23bcb18f2dd735c4d9d2d228f654efce";

        /// <summary>Native GreaterEarthElementalBrain, shared by the native
        /// summoned and hostile Greater/Elder Earth Elementals. Never modified.</summary>
        internal const string NativeGreaterEarthElementalBrainGuid =
            "d8342bca1ce38294bbf47f9566e6e2aa";

        /// <summary>Native Dumb Monster Brain (ordinary attack only), already
        /// used by the native and KMG Huge Earth Elemental.</summary>
        internal const string NativeDumbMonsterBrainGuid =
            "5abc8884c6f15204c8604cb01a2efbab";

        /// <summary>
        /// DISMANTLE-01 (owner decision, 2026-11-11): the Bebelith's Dismantle
        /// Armor is removed like Sunder. It was a post-claw-hit rider that
        /// rolled a native SunderArmor combat maneuver. While withdrawn, the
        /// builder gives the combat component no dismantled-armor buff, the
        /// component's attack handler does no Dismantle work even with a
        /// buff reference, and an already applied dismantled-armor buff in an
        /// older save contributes no armor-class penalty (an inert legacy
        /// marker). The buff blueprint, its component and the retained rules
        /// policy stay registered for save compatibility.
        /// </summary>
        internal static readonly bool BebelithDismantleArmorWithdrawn = true;

        /// <summary>
        /// The one decision both Bebelith handlers consult. Penetrating
        /// Strike never depends on the Dismantle withdrawal; the Dismantle
        /// trigger needs both an active mechanic and a configured buff; the
        /// legacy dismantled-armor penalty applies only while the mechanic is
        /// active.
        /// </summary>
        internal static bool BebelithEffectActive(BebelithEffect effect, bool dismantleWithdrawn, bool dismantledArmorConfigured)
        {
            switch (effect)
            {
                case BebelithEffect.PenetratingStrike: return true;
                case BebelithEffect.DismantleTrigger: return !dismantleWithdrawn && dismantledArmorConfigured;
                case BebelithEffect.DismantledArmorPenalty: return !dismantleWithdrawn;
                default: return false;
            }
        }

        /// <summary>The armor-class penalty an existing dismantled-armor
        /// effect contributes: none while withdrawn, otherwise the live
        /// contribution of the dismantled piece (never negative).</summary>
        internal static int BebelithDismantledArmorPenalty(bool dismantleWithdrawn, Func<int> lostArmorClass)
        {
            if (!BebelithEffectActive(BebelithEffect.DismantledArmorPenalty, dismantleWithdrawn, true) || lostArmorClass == null) return 0;
            return Math.Max(0, lostArmorClass());
        }

        /// <summary>KMG creatures that inherited Sunder from a native donor
        /// rather than from their own builder, and so are corrected by the
        /// withdrawal step itself: the feat leaves their AddFacts and the
        /// shared Sunder brain is replaced by the native ordinary-attack brain.</summary>
        internal static readonly string[] DonorInheritedSunderKeys =
            { "greater-earth-elemental", "elder-earth-elemental" };

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
            // Repair R5 SPELL-01: the custom Fog Cloud route is replaced by the
            // single provider-backed Obscuring Mist.
            new WithdrawnCapabilitySpec("cloud-giant", "KMG.Summoning.Unit.CloudGiant",
                new[] { "KMG.Summoning.Giant.Sprint29.CloudFogCloud" },
                new[] { "KMG.Summoning.Giant.Sprint29.CloudFogAi" },
                new[] { "KMG.Summoning.Giant.Sprint29.CloudFogResource" },
                Array.Empty<string>()),
            // SUNDER-01. None of these units carried a withdrawal fixer
            // before, so each fixer name below is new and runs once on an
            // older saved summon. A later withdrawal from one of these units
            // needs yet another name.
            new WithdrawnCapabilitySpec("storm-giant", "KMG.Summoning.Unit.StormGiant",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
                new[] { NativeImprovedSunderGuid }),
            new WithdrawnCapabilitySpec("greater-earth-elemental", "KMG.Summoning.Unit.GreaterEarthElemental",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
                new[] { NativeImprovedSunderGuid }, new[] { NativeSunderArmorAiActionGuid }),
            new WithdrawnCapabilitySpec("elder-earth-elemental", "KMG.Summoning.Unit.ElderEarthElemental",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
                new[] { NativeImprovedSunderGuid }, new[] { NativeSunderArmorAiActionGuid }),
        };

        /// <summary>True when this creature's native Improved Sunder grant is
        /// withdrawn (SUNDER-01 and the earlier Fire Giant cut).</summary>
        internal static bool WithdrawsSunderFrom(string creatureKey)
        {
            WithdrawnCapabilitySpec spec = For(creatureKey);
            return spec != null && spec.NativeFeatGuids.Contains(NativeImprovedSunderGuid,
                StringComparer.OrdinalIgnoreCase);
        }

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

    internal enum BebelithEffect { PenetratingStrike, DismantleTrigger, DismantledArmorPenalty }

    internal sealed class WithdrawnCapabilitySpec
    {
        internal WithdrawnCapabilitySpec(string creatureKey, string unitSymbol,
            string[] factSymbols, string[] aiActionSymbols, string[] resourceSymbols,
            string[] nativeFeatGuids, string[] nativeAiActionGuids = null)
        {
            CreatureKey = creatureKey; UnitSymbol = unitSymbol;
            FactSymbols = Array.AsReadOnly(factSymbols);
            AiActionSymbols = Array.AsReadOnly(aiActionSymbols);
            ResourceSymbols = Array.AsReadOnly(resourceSymbols);
            NativeFeatGuids = Array.AsReadOnly(nativeFeatGuids);
            NativeAiActionGuids = Array.AsReadOnly(nativeAiActionGuids ?? Array.Empty<string>());
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
        /// <summary>Native cast-spell AI actions (exact type
        /// BlueprintAiCastSpell) removed from an older saved brain of this
        /// creature alone. The shared native action is never modified.</summary>
        internal IReadOnlyList<string> NativeAiActionGuids { get; private set; }
    }
}
