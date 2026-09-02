using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal sealed class ElementalRaceNativeVisualAsset
    {
        internal ElementalRaceNativeVisualAsset(string assetId,
            string expectedName)
        {
            if (!IsAssetId(assetId))
                throw new ArgumentException(
                    "A lowercase 32-character native visual asset ID is required.",
                    "assetId");
            if (string.IsNullOrWhiteSpace(expectedName))
                throw new ArgumentException(
                    "An expected native visual resource name is required.",
                    "expectedName");
            AssetId = assetId;
            ExpectedName = expectedName;
        }

        internal string AssetId { get; private set; }
        internal string ExpectedName { get; private set; }

        internal static bool IsAssetId(string value)
        {
            if (value == null || value.Length != 32) return false;
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (!((current >= '0' && current <= '9') ||
                    (current >= 'a' && current <= 'f')))
                    return false;
            }
            return true;
        }
    }

    internal sealed class ElementalRaceRampReference
    {
        internal ElementalRaceRampReference(
            ElementalRaceNativeVisualAsset source,
            string expectedProfile,
            string textureName)
        {
            Source = source ?? throw new ArgumentNullException("source");
            if (string.IsNullOrWhiteSpace(expectedProfile))
                throw new ArgumentException(
                    "An expected native color profile is required.",
                    "expectedProfile");
            if (string.IsNullOrWhiteSpace(textureName))
                throw new ArgumentException(
                    "An expected native ramp texture name is required.",
                    "textureName");
            ExpectedProfile = expectedProfile;
            TextureName = textureName;
        }

        internal ElementalRaceNativeVisualAsset Source { get; private set; }
        internal string ExpectedProfile { get; private set; }
        internal string TextureName { get; private set; }
    }

    internal sealed class ElementalRaceVisualProxySpec
    {
        internal ElementalRaceVisualProxySpec(string symbol,
            ElementalRaceNativeVisualAsset donor,
            ElementalRaceNativeVisualAsset fallback,
            bool usesSkinPalette)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                !symbol.StartsWith("KMG.ElementalRaces.",
                    StringComparison.Ordinal))
                throw new ArgumentException(
                    "A project-owned elemental visual symbol is required.",
                    "symbol");
            Symbol = symbol;
            Donor = donor ?? throw new ArgumentNullException("donor");
            Fallback = fallback ?? throw new ArgumentNullException("fallback");
            UsesSkinPalette = usesSkinPalette;
        }

        internal string Symbol { get; private set; }
        internal ElementalRaceNativeVisualAsset Donor { get; private set; }
        internal ElementalRaceNativeVisualAsset Fallback { get; private set; }
        internal bool UsesSkinPalette { get; private set; }
    }

    internal sealed class ElementalRaceSexVisualDefinition
    {
        private readonly ElementalRaceVisualProxySpec[] _heads;
        private readonly ElementalRaceNativeVisualAsset[] _hair;
        private readonly ElementalRaceNativeVisualAsset[] _eyebrows;
        private readonly ElementalRaceNativeVisualAsset[] _beards;
        private readonly ElementalRaceVisualProxySpec[] _horns;

        internal ElementalRaceSexVisualDefinition(
            ElementalRaceVisualProxySpec body,
            IEnumerable<ElementalRaceVisualProxySpec> heads,
            IEnumerable<ElementalRaceNativeVisualAsset> hair,
            IEnumerable<ElementalRaceNativeVisualAsset> eyebrows,
            IEnumerable<ElementalRaceNativeVisualAsset> beards,
            IEnumerable<ElementalRaceVisualProxySpec> horns)
        {
            Body = body ?? throw new ArgumentNullException("body");
            _heads = Copy(heads, "heads");
            _hair = Copy(hair, "hair");
            _eyebrows = Copy(eyebrows, "eyebrows");
            _beards = Copy(beards, "beards", true);
            _horns = Copy(horns, "horns", true);
            if (_heads.Length < 2)
                throw new ArgumentException(
                    "At least two visual head proxies are required.", "heads");
            if (_hair.Length < 4)
                throw new ArgumentException(
                    "At least four native hair choices are required.", "hair");
            if (_eyebrows.Length < 1)
                throw new ArgumentException(
                    "At least one native eyebrow choice is required.",
                    "eyebrows");
        }

        internal ElementalRaceVisualProxySpec Body { get; private set; }
        internal ElementalRaceVisualProxySpec[] Heads
        { get { return (ElementalRaceVisualProxySpec[])_heads.Clone(); } }
        internal ElementalRaceNativeVisualAsset[] Hair
        { get { return (ElementalRaceNativeVisualAsset[])_hair.Clone(); } }
        internal ElementalRaceNativeVisualAsset[] Eyebrows
        { get { return (ElementalRaceNativeVisualAsset[])_eyebrows.Clone(); } }
        internal ElementalRaceNativeVisualAsset[] Beards
        { get { return (ElementalRaceNativeVisualAsset[])_beards.Clone(); } }
        internal ElementalRaceVisualProxySpec[] Horns
        { get { return (ElementalRaceVisualProxySpec[])_horns.Clone(); } }

        internal IEnumerable<ElementalRaceVisualProxySpec> Proxies()
        {
            yield return Body;
            foreach (ElementalRaceVisualProxySpec value in _heads)
                yield return value;
            foreach (ElementalRaceVisualProxySpec value in _horns)
                yield return value;
        }

        private static T[] Copy<T>(IEnumerable<T> values, string parameter,
            bool allowEmpty = false) where T : class
        {
            T[] result = values == null ? null : values.ToArray();
            if (result == null || (!allowEmpty && result.Length == 0) ||
                result.Any(value => value == null))
                throw new ArgumentException(
                    "Visual definitions must contain only non-null entries.",
                    parameter);
            return result;
        }
    }

    internal sealed class ElementalRaceVisualDefinition
    {
        private readonly string[] _presetSymbols;
        private readonly ElementalRaceRampReference[] _skinPalette;

        internal ElementalRaceVisualDefinition(ElementalRaceKind kind,
            string bodyBlueprintSymbol,
            IEnumerable<string> presetSymbols,
            IEnumerable<ElementalRaceRampReference> skinPalette,
            ElementalRaceSexVisualDefinition male,
            ElementalRaceSexVisualDefinition female)
        {
            Kind = kind;
            if (string.IsNullOrWhiteSpace(bodyBlueprintSymbol))
                throw new ArgumentException(
                    "A visual body blueprint symbol is required.",
                    "bodyBlueprintSymbol");
            BodyBlueprintSymbol = bodyBlueprintSymbol;
            _presetSymbols = presetSymbols == null ? null :
                presetSymbols.ToArray();
            _skinPalette = skinPalette == null ? null : skinPalette.ToArray();
            Male = male ?? throw new ArgumentNullException("male");
            Female = female ?? throw new ArgumentNullException("female");
            if (_presetSymbols == null || _presetSymbols.Length != 3 ||
                _presetSymbols.Any(string.IsNullOrWhiteSpace) ||
                _presetSymbols.Distinct(StringComparer.Ordinal).Count() != 3)
                throw new ArgumentException(
                    "Exactly three unique visual preset symbols are required.",
                    "presetSymbols");
            if (_skinPalette == null || _skinPalette.Length != 7 ||
                _skinPalette.Any(value => value == null) ||
                _skinPalette.Select(value => value.TextureName).Distinct(
                    StringComparer.Ordinal).Count() != _skinPalette.Length)
                throw new ArgumentException(
                    "Exactly seven unique native skin ramps are required.",
                    "skinPalette");
        }

        internal ElementalRaceKind Kind { get; private set; }
        internal string BodyBlueprintSymbol { get; private set; }
        internal string[] PresetSymbols
        { get { return (string[])_presetSymbols.Clone(); } }
        internal ElementalRaceRampReference[] SkinPalette
        { get { return (ElementalRaceRampReference[])_skinPalette.Clone(); } }
        internal ElementalRaceSexVisualDefinition Male { get; private set; }
        internal ElementalRaceSexVisualDefinition Female { get; private set; }

        internal IEnumerable<ElementalRaceVisualProxySpec> Proxies()
        {
            return Male.Proxies().Concat(Female.Proxies());
        }
    }
}
