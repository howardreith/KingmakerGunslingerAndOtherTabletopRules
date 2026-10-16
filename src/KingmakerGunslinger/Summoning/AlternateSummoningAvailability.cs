using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which adapter children and host roots this process currently exposes.
    /// Registration alone never makes a child castable: a child is exposed
    /// only while its family's publication holds it in a host menu.
    /// </summary>
    internal static class AlternateSummoningExposure
    {
        internal const string VariantsComponentName =
            "$KMG_AlternateSummoning_Variants";

        private static readonly object Gate = new object();
        private static readonly HashSet<string> ExposedChildren =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> PublishedRoots =
            new HashSet<string>(StringComparer.Ordinal);

        internal static bool IsExposed(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return false;
            lock (Gate) return ExposedChildren.Contains(guid);
        }

        internal static int ExposedCount
        { get { lock (Gate) return ExposedChildren.Count; } }

        internal static int PublishedRootCount
        { get { lock (Gate) return PublishedRoots.Count; } }

        /// <summary>
        /// An adapter-published caller root: an exact published identity that
        /// still carries this module's own variants component. The variant
        /// menu layout extends to these and to nothing matched by name.
        /// </summary>
        internal static bool IsPublishedRoot(BlueprintAbility ability)
        {
            if (ability == null) return false;
            lock (Gate)
                if (!PublishedRoots.Contains(ability.AssetGuid)) return false;
            return (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityVariants>().Any(value => value != null &&
                    string.Equals(value.name, VariantsComponentName,
                        StringComparison.Ordinal));
        }

        internal static void Add(IEnumerable<string> roots,
            IEnumerable<string> children)
        {
            lock (Gate)
            {
                foreach (string root in roots) PublishedRoots.Add(root);
                foreach (string child in children) ExposedChildren.Add(child);
            }
        }

        internal static void Remove(IEnumerable<string> roots,
            IEnumerable<string> children)
        {
            lock (Gate)
            {
                foreach (string root in roots) PublishedRoots.Remove(root);
                foreach (string child in children) ExposedChildren.Remove(child);
            }
        }
    }

    /// <summary>
    /// Carried by every adapter child, so eligibility is decided on the
    /// choice that actually executes - including one reached from a saved
    /// action-bar shortcut after its creature was withheld, the module was
    /// turned OFF, or its family failed to publish.
    /// </summary>
    [Serializable]
    public sealed class AlternateSummoningAvailability : BlueprintComponent,
        IAbilityAvailabilityProvider
    {
        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Blueprint != null &&
                AlternateSummoningExposure.IsExposed(ability.Blueprint.AssetGuid);
        }

        public string GetReason()
        {
            return "This summoning option is not offered in the current configuration.";
        }
    }
}
