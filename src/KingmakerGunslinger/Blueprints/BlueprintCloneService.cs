using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Creates an unregistered Unity clone of a native blueprint. Registration and
    /// GUID assignment remain the sole responsibility of BlueprintRegistry.
    /// </summary>
    internal static class BlueprintCloneService
    {
        internal static T Clone<T>(T source, string internalName)
            where T : BlueprintScriptableObject
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrWhiteSpace(internalName))
            {
                throw new ArgumentException("A clone internal name is required.", "internalName");
            }

            T clone = UnityEngine.Object.Instantiate(source);
            if (clone == null)
            {
                throw new InvalidOperationException(
                    "Unity returned null while cloning blueprint '" + source.name + "'.");
            }

            if (ReferenceEquals(source, clone))
            {
                throw new InvalidOperationException(
                    "Unity returned the native blueprint instance instead of a clone.");
            }

            clone.name = internalName;
            if (!string.Equals(clone.name, internalName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The cloned blueprint name could not be assigned.");
            }

            return clone;
        }
    }
}
