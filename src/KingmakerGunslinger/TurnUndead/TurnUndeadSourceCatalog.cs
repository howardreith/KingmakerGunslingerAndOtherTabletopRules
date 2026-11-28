using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.TurnUndead
{
    internal enum TurnUndeadSourceKind
    {
        Cleric,
        Paladin,
        Hospitaler,
        Empyreal
    }

    /// <summary>
    /// The saving-throw statistic named by a source's own channel rules. Kept
    /// engine-free so the domain suite can check the mapping; the native
    /// adapter maps it to StatType.
    /// </summary>
    internal enum TurnUndeadDcStat
    {
        Charisma,
        Wisdom
    }

    /// <summary>
    /// One verified native positive-channel source. Every value here was read
    /// from the installed game's blueprints and printed channel rules (see
    /// planning/turn-undead/IMPLEMENTATION-RECORD.md, source contract table):
    /// the qualifying feature, the pool it already spends, the number of uses
    /// one channel costs, the class whose level sets the DC, the statistic the
    /// source's printed DC names, and the native heal/harm actions whose
    /// restrictions the new action keeps.
    /// </summary>
    internal sealed class TurnUndeadSourceContract
    {
        internal TurnUndeadSourceContract(TurnUndeadSourceKind kind, string key,
            string displayName, string sourceFeatureGuid, string resourceGuid,
            int resourceCost, string dcClassGuid, string dcClassName,
            TurnUndeadDcStat dcStat, string nativeHarmGuid, string nativeHealGuid,
            bool requiresLawfulGood)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key");
            if (resourceCost < 1) throw new ArgumentOutOfRangeException("resourceCost");
            Kind = kind;
            Key = key;
            DisplayName = displayName;
            SourceFeatureGuid = RequireGuid(sourceFeatureGuid, "sourceFeatureGuid");
            ResourceGuid = RequireGuid(resourceGuid, "resourceGuid");
            ResourceCost = resourceCost;
            DcClassGuid = RequireGuid(dcClassGuid, "dcClassGuid");
            DcClassName = dcClassName;
            DcStat = dcStat;
            NativeHarmGuid = RequireGuid(nativeHarmGuid, "nativeHarmGuid");
            NativeHealGuid = RequireGuid(nativeHealGuid, "nativeHealGuid");
            RequiresLawfulGood = requiresLawfulGood;
        }

        internal TurnUndeadSourceKind Kind { get; private set; }
        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal string SourceFeatureGuid { get; private set; }
        internal string ResourceGuid { get; private set; }
        internal int ResourceCost { get; private set; }
        internal string DcClassGuid { get; private set; }
        internal string DcClassName { get; private set; }
        internal TurnUndeadDcStat DcStat { get; private set; }
        internal string NativeHarmGuid { get; private set; }
        internal string NativeHealGuid { get; private set; }
        internal bool RequiresLawfulGood { get; private set; }

        /// <summary>Manifest symbol of the directly granted, pinnable action.</summary>
        internal string DirectAbilitySymbol
        { get { return TurnUndeadSourceCatalog.SymbolPrefix + DisplayName + ".Ability"; } }

        /// <summary>
        /// Manifest symbol of the never-granted child placed under a loaded
        /// provider's positive-harm wrapper. Each source owns its own child, so
        /// no blueprint's Parent is shared between incompatible wrappers.
        /// </summary>
        internal string MenuChildSymbol
        { get { return TurnUndeadSourceCatalog.SymbolPrefix + DisplayName + ".MenuChild"; } }

        private static string RequireGuid(string value, string name)
        {
            if (value == null || value.Length != 32 ||
                value.Any(c => !(c >= '0' && c <= '9' || c >= 'a' && c <= 'f')))
                throw new ArgumentException("A lowercase 32-character GUID is required.", name);
            return value;
        }
    }

    internal static class TurnUndeadSourceCatalog
    {
        internal const string SymbolPrefix = "KMG.Feats.TurnUndead.";
        internal const string FeatSymbol = "KMG.Feats.TurnUndead.Feat";
        internal const string FleeingBuffSymbol = "KMG.Feats.TurnUndead.Fleeing";

        internal const string ClericClassGuid = "67819271767a9dd4fbfd4ae700befea0";
        internal const string PaladinClassGuid = "bfa11238e7ae3544bbeb4d0b92e897ec";
        internal const string SorcererClassGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";

        /// <summary>Native wizard necromancer ability that must stay untouched.</summary>
        internal const string WizardTurnUndeadGuid = "71b8898b1d26d654b9a3eeac87e3e2f8";
        internal const string WizardTurnUndeadResourceGuid = "16394ff73175b5745957750040a2c99e";
        /// <summary>Native undead-turning flight buff: read-only donor of its icon/name.</summary>
        internal const string NativeFrightenedUndeadGuid = "1265c0b27ef559c4e980a900dfc68d1e";
        internal const string UndeadTypeGuid = "734a29b693e9ec346ba2951b27987e33";
        internal const string ChannelResistanceFourGuid = "a9ac84c6f48b491438f91bb237bc9212";
        internal const string BasicFeatSelectionGuid = "247a4068296e8be42890143f451b4b45";

        private static readonly TurnUndeadSourceContract[] Sources =
        {
            new TurnUndeadSourceContract(TurnUndeadSourceKind.Cleric, "cleric", "Cleric",
                "a79013ff4bcd4864cb669622a29ddafb", "5e2bba3e07c37be42909a12945c27de7", 1,
                ClericClassGuid, "cleric", TurnUndeadDcStat.Charisma,
                "279447a6bf2d3544d93a0a39c3b8e91d", "f5fc9a1a2a3c1a946a31b320d1dd31b2", false),
            new TurnUndeadSourceContract(TurnUndeadSourceKind.Paladin, "paladin", "Paladin",
                "cb6d55dda5ab906459d18a435994a760", "9dedf41d995ff4446a181f143c3db98c", 2,
                PaladinClassGuid, "paladin", TurnUndeadDcStat.Charisma,
                "4937473d1cfd7774a979b625fb833b47", "6670f0f21a1d7f04db2b8b115e8e6abf", true),
            new TurnUndeadSourceContract(TurnUndeadSourceKind.Hospitaler, "hospitaler", "Hospitaler",
                "a9ab1bbc79ecb174d9a04699986ce8d5", "b0e0c7716ab27c64fb4b131c9845c596", 1,
                PaladinClassGuid, "hospitaler (paladin)", TurnUndeadDcStat.Charisma,
                "cc17243b2185f814aa909ac6b6599eaa", "0c0cf7fcb356d2448b7d57f2c4db3c0c", true),
            new TurnUndeadSourceContract(TurnUndeadSourceKind.Empyreal, "empyreal", "Empyreal",
                "7d49d7f590dc9a948b3bd1c8b7979854", "f9af9354fb8a79649a6e512569387dc5", 1,
                SorcererClassGuid, "sorcerer", TurnUndeadDcStat.Wisdom,
                "e1536ee240c5d4141bf9f9485a665128", "574cf074e8b65e84d9b69a8c6f1af27b", false),
        };

        internal static IReadOnlyList<TurnUndeadSourceContract> All { get { return Sources; } }

        internal static TurnUndeadSourceContract Require(TurnUndeadSourceKind kind)
        { return Sources.Single(value => value.Kind == kind); }

        /// <summary>
        /// Sources whose actual qualifying feature the owner has, in catalog
        /// order. Class levels, alignment permission facts, items, a
        /// negative-energy channel or a generic channel marker never qualify.
        /// </summary>
        internal static TurnUndeadSourceContract[] Owned(Func<string, bool> hasFeatureGuid)
        {
            if (hasFeatureGuid == null) throw new ArgumentNullException("hasFeatureGuid");
            return Sources.Where(value => hasFeatureGuid(value.SourceFeatureGuid)).ToArray();
        }

        internal static IEnumerable<string> AllSymbols()
        {
            yield return FeatSymbol;
            yield return FleeingBuffSymbol;
            foreach (TurnUndeadSourceContract source in Sources)
            {
                yield return source.DirectAbilitySymbol;
                yield return source.MenuChildSymbol;
            }
        }
    }
}
