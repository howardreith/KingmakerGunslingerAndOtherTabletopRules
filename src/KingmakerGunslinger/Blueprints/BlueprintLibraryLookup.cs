using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Strict read-only lookup adapter for native Kingmaker blueprints used as
    /// clone sources. Source IDs are fixed game data, never generated at runtime.
    /// </summary>
    internal static class BlueprintLibraryLookup
    {
        internal static T RequireExact<T>(
            LibraryScriptableObject library,
            string assetGuid,
            string role)
            where T : BlueprintScriptableObject
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            BlueprintId id = BlueprintId.Parse(assetGuid, "assetGuid");
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("A source-blueprint role is required.", "role");
            }

            if (library.BlueprintsByAssetId == null)
            {
                throw new InvalidOperationException("Kingmaker's blueprint dictionary is unavailable.");
            }

            BlueprintScriptableObject blueprint;
            if (!library.BlueprintsByAssetId.TryGetValue(id.Value, out blueprint) || blueprint == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Required native blueprint was not found: role='{0}', guid='{1}', expectedType='{2}'.",
                        role,
                        id.Value,
                        typeof(T).FullName));
            }

            if (blueprint.GetType() != typeof(T))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Native blueprint type mismatch: role='{0}', guid='{1}', expectedType='{2}', actualType='{3}'.",
                        role,
                        id.Value,
                        typeof(T).FullName,
                        blueprint.GetType().FullName));
            }

            if (string.IsNullOrWhiteSpace(blueprint.name))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Required native blueprint has no internal name: role='{0}', guid='{1}'.",
                        role,
                        id.Value));
            }

            return (T)blueprint;
        }

        /// <summary>
        /// Resolves a unit fact from the native donor snapshot before custom
        /// registration can alter library enumeration. The lookup remains exact:
        /// one reference identity, one GUID, and one runtime type.
        /// </summary>
        internal static T RequireExactUnitFactReference<T>(
            IEnumerable<BlueprintUnit> sourceUnits,
            string assetGuid,
            string role)
            where T : BlueprintUnitFact
        {
            if (sourceUnits == null) throw new ArgumentNullException("sourceUnits");
            BlueprintId id = BlueprintId.Parse(assetGuid, "assetGuid");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("A source-blueprint role is required.",
                    "role");
            var distinct = new List<BlueprintUnitFact>();
            foreach (BlueprintUnitFact fact in sourceUnits
                .Where(unit => unit != null)
                .SelectMany(unit => unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Where(fact => fact != null && string.Equals(fact.AssetGuid,
                    id.Value, StringComparison.Ordinal)))
            {
                if (!distinct.Any(value => ReferenceEquals(value, fact)))
                    distinct.Add(fact);
            }
            if (distinct.Count != 1)
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Required referenced unit fact was not unique: role='{0}', guid='{1}', expectedType='{2}', referenceCount={3}.",
                    role, id.Value, typeof(T).FullName, distinct.Count));
            return ValidateExactUnitFact<T>(distinct[0], id.Value, role,
                "pre-registration-unit-reference");
        }

        private static T ValidateExactUnitFact<T>(BlueprintScriptableObject value,
            string assetGuid, string role, string source)
            where T : BlueprintUnitFact
        {
            if (value.GetType() != typeof(T))
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Native unit-fact type mismatch: role='{0}', guid='{1}', source='{2}', expectedType='{3}', actualType='{4}'.",
                    role, assetGuid, source, typeof(T).FullName,
                    value.GetType().FullName));
            if (string.IsNullOrWhiteSpace(value.name))
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Required native unit fact has no internal name: role='{0}', guid='{1}', source='{2}'.",
                    role, assetGuid, source));
            return (T)(object)value;
        }
    }
}
