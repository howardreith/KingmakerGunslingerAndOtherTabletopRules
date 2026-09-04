using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalFeatBlueprintSet
    {
        private readonly Dictionary<ElementalFeatId, BlueprintFeature>
            m_Features;
        private readonly Dictionary<string, BlueprintScriptableObject>
            m_Blueprints;

        internal ElementalFeatBlueprintSet(
            IDictionary<ElementalFeatId, BlueprintFeature> features,
            IEnumerable<BlueprintScriptableObject> registered)
        {
            m_Features = features == null ? null :
                new Dictionary<ElementalFeatId, BlueprintFeature>(features);
            BlueprintScriptableObject[] ordered = registered == null ? null :
                registered.ToArray();
            if (m_Features == null ||
                m_Features.Count != ElementalFeatPolicy.FeatCount ||
                m_Features.Any(entry => entry.Value == null) ||
                ordered == null ||
                ordered.Length != ElementalRaceIdentityCatalog
                    .FeatIdentityCount ||
                ordered.Any(value => value == null) ||
                ordered.Distinct().Count() != ordered.Length ||
                ordered.Any(value => string.IsNullOrWhiteSpace(
                    value.AssetGuid)) ||
                ordered.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() != ordered.Length)
                throw new InvalidOperationException(
                    "Elemental feat blueprint graph is incomplete.");
            m_Blueprints = ordered.ToDictionary(value => value.AssetGuid,
                StringComparer.Ordinal);
            if (m_Features.Values.Any(value =>
                    !m_Blueprints.ContainsKey(value.AssetGuid)))
                throw new InvalidOperationException(
                    "An elemental feat was not included in its registration set.");
        }

        internal int RegisteredCount
        {
            get { return m_Blueprints.Count; }
        }

        internal BlueprintFeature RequireFeature(ElementalFeatId id)
        {
            BlueprintFeature result;
            if (!m_Features.TryGetValue(id, out result))
                throw new KeyNotFoundException(
                    "Missing elemental feat blueprint: " + id + ".");
            return result;
        }

        internal BlueprintFeature[] AllFeats()
        {
            return ElementalFeatPolicy.Ordered().Select(value =>
                RequireFeature(value.Id)).ToArray();
        }

        internal BlueprintFeature[] CombatFeats()
        {
            return ElementalFeatPolicy.Ordered().Where(value =>
                value.IsCombat).Select(value => RequireFeature(value.Id))
                .ToArray();
        }

        internal T Require<T>(string assetGuid)
            where T : BlueprintScriptableObject
        {
            BlueprintScriptableObject result;
            if (string.IsNullOrWhiteSpace(assetGuid) ||
                !m_Blueprints.TryGetValue(assetGuid, out result) ||
                !(result is T))
                throw new KeyNotFoundException(
                    "Missing elemental feat subsidiary blueprint: " +
                    assetGuid + ".");
            return (T)result;
        }
    }
}
