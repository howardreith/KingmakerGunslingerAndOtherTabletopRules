using System;
using System.Globalization;
using Kingmaker.Blueprints;

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
    }
}
