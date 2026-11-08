using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One Pixie spell-like ability built from an installed native spell.</summary>
    internal sealed class PixieSpellLikeSpec
    {
        internal PixieSpellLikeSpec(string token, string displayName, string nativeGuid, int spellLevel,
            string[] variantTokens, string[] nativeVariantGuids, string nativeAiGuid, int ownedAiCooldownRounds)
        {
            Token = token; DisplayName = displayName; NativeGuid = nativeGuid; SpellLevel = spellLevel;
            VariantTokens = variantTokens ?? new string[0]; NativeVariantGuids = nativeVariantGuids ?? new string[0];
            NativeAiGuid = nativeAiGuid; OwnedAiCooldownRounds = ownedAiCooldownRounds;
        }

        internal string Token { get; private set; }
        internal string DisplayName { get; private set; }
        internal string NativeGuid { get; private set; }
        internal int SpellLevel { get; private set; }
        internal string[] VariantTokens { get; private set; }
        internal string[] NativeVariantGuids { get; private set; }

        /// <summary>A native AI action whose targeting/considerations the owned action copies; null builds an owned one.</summary>
        internal string NativeAiGuid { get; private set; }
        internal int OwnedAiCooldownRounds { get; private set; }

        internal string Symbol { get { return SummonPlaytestR5Policy.PixiePrefix + Token; } }
        internal string AiSymbol { get { return Symbol + "Ai"; } }
        internal IEnumerable<string> VariantSymbols { get { return VariantTokens.Select(token => SummonPlaytestR5Policy.PixiePrefix + token); } }
    }

    /// <summary>
    /// Repair R5 (SPELL-01..03). Spells come from their actual installed
    /// implementations: four native Kingmaker spells for the Pixie, and Call of
    /// the Wild's own Obscuring Mist and Irresistible Dance, which are bound
    /// only when that optional provider is loaded, active and exact.
    /// </summary>
    internal static class SummonPlaytestR5Policy
    {
        internal const string PixiePrefix = "KMG.Summoning.Special.Pixie.";
        internal const string MastodonTrampleSymbol = "KMG.Summoning.Special.Mastodon.Trample";

        /// <summary>The engine grants this native Charge to every unit; no KMG unit grants it again.</summary>
        internal const string NativeChargeAbilityGuid = "c78506dd0e14f7c45a599990e4e65038";

        // Optional provider: Call of the Wild (UMM id) and its blueprint
        // identities from its own blueprints.txt.
        internal const string CotwModId = "CallOfTheWild";
        internal const string CotwObscuringMistGuid = "4acf55ad7fec47478ded06d9e929fa06";
        internal const string CotwObscuringMistName = "ObscuringMistAbility";
        internal const string CotwObscuringMistAreaGuid = "0aee4360e6a34950b7baff11658aeb14";
        internal const string CotwIrresistibleDanceGuid = "fad6a06a3cb04fabaedf4d358c61880d";
        internal const string CotwIrresistibleDanceCastGuid = "7733b3550b4f03002c59bc590e07d9cc";
        internal const string CotwIrresistibleDanceCastName = "IrresistibleDanceAbilityCast";

        /// <summary>The Cloud Giant's spell-like caster level and Obscuring Mist's spell level.</summary>
        internal const int CloudMistCasterLevel = 16;
        internal const int CloudMistSpellLevel = 1;

        /// <summary>The Pixie's spell-like caster level (unchanged from the existing Dance).</summary>
        internal const int PixieCasterLevel = 8;

        private static readonly PixieSpellLikeSpec[] PixieSpellLikes = {
            new PixieSpellLikeSpec("Entangle", "Entangle", "0fd00984a2c0e0a429cf1a911b4ec5ca", 1, null, null, null, 3),
            new PixieSpellLikeSpec("Confusion", "Confusion", "cf6c901fb7acc904e85c63b342e9c949", 4, null, null,
                "c0341a721aa87574baf0c40982df9d84", 0),
            new PixieSpellLikeSpec("DispelMagic", "Dispel Magic", "92681f181b507b34ea87018e8f7a528a", 3,
                new[] { "DispelMagicTarget", "DispelMagicArea" },
                new[] { "143775c49ae6b7446b805d3b2e702298", "9f6daa93291737c40b8a432c374226a7" },
                "13639c828f06c7341b8ebd745f244f30", 0),
            new PixieSpellLikeSpec("Shield", "Shield", "ef768022b0785eb43a18969903c537c4", 1, null, null,
                "8df9d8605fa49554691de7d02b8397ed", 0)
        };

        internal static IReadOnlyList<PixieSpellLikeSpec> PixieAtWill
        { get { return Array.AsReadOnly(PixieSpellLikes); } }

        /// <summary>New append-only identities, in registration order.</summary>
        internal static IEnumerable<KeyValuePair<string, string>> NewIdentities()
        {
            yield return new KeyValuePair<string, string>(MastodonTrampleSymbol, "BlueprintAbility");
            foreach (PixieSpellLikeSpec spec in PixieSpellLikes)
            {
                yield return new KeyValuePair<string, string>(spec.Symbol, "BlueprintAbility");
                foreach (string variant in spec.VariantSymbols)
                    yield return new KeyValuePair<string, string>(variant, "BlueprintAbility");
                yield return new KeyValuePair<string, string>(spec.AiSymbol, "BlueprintAiCastSpell");
            }
        }

        /// <summary>
        /// The optional provider gate: an exactly-one, loaded, active, error-free
        /// entry whose assembly carries the expected name. Anything else omits
        /// the provider-backed abilities.
        /// </summary>
        internal static string ProviderDisposition(int matches, bool loaded, bool active, bool hasAssembly,
            bool errorOnLoading, string assemblyName)
        {
            if (matches == 0) return "provider-absent";
            if (matches != 1) return "provider-ambiguous";
            if (!loaded || !active || !hasAssembly || errorOnLoading || assemblyName != CotwModId)
                return "provider-inactive-or-mismatched";
            return "provider-active";
        }
    }
}
