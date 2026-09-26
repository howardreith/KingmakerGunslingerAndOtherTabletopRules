using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>The exact host human prerequisites a bridge scope covers.</summary>
    internal sealed class FavoredClassBridgeScope
    {
        internal FavoredClassBridgeScope(int favoredClassLeafPrerequisites, IList<string> otherHumanPrerequisites)
        {
            FavoredClassLeafPrerequisites = favoredClassLeafPrerequisites;
            OtherHumanPrerequisites = otherHumanPrerequisites;
        }

        /// <summary>Human race prerequisites on leaves of the host's bonus selections.</summary>
        internal int FavoredClassLeafPrerequisites { get; private set; }

        /// <summary>Owners of every other exact human race prerequisite (the host's human race traits).</summary>
        internal IList<string> OtherHumanPrerequisites { get; private set; }

        internal int Total
        {
            get { return FavoredClassLeafPrerequisites + OtherHumanPrerequisites.Count; }
        }
    }

    /// <summary>
    /// Narrow ancestry bridge (charter sections 5.2 and 6.5) of the Mostly
    /// Human racial trait. The host's <c>PrerequisiteRace.Check</c> uses exact
    /// race equality; in the installed stack every race-related Human
    /// prerequisite is one of its instances (the human favored-class leaves
    /// and the host's human race traits). Exactly those Human instances are
    /// tracked, and for those only the verified Mostly Human identity of a
    /// geniekin parent is added: every native exact-race result stays, every
    /// other prerequisite, race and host policy is untouched, and nothing
    /// changes the unit's race. The scope belongs to the racial trait, so it
    /// is prepared whenever the exact host is ready, independently of the
    /// favored-class integration switch.
    /// </summary>
    internal static class FavoredClassHostRaceBridge
    {
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly object Gate = new object();
        private static bool _installed;
        private static volatile HashSet<object> _tracked = new HashSet<object>(ReferenceComparer.Instance);

        private static FavoredClassBridgeScope _scope;

        internal static int TrackedCount
        {
            get { return _tracked.Count; }
        }

        /// <summary>The prepared scope, or null before preparation.</summary>
        internal static FavoredClassBridgeScope Scope
        {
            get { lock (Gate) return _scope; }
        }

        /// <summary>Whether this exact prerequisite instance is in the bridge scope.</summary>
        internal static bool IsTracked(object prerequisite)
        {
            return prerequisite != null && _tracked.Contains(prerequisite);
        }

        /// <summary>
        /// Records the exact scope and installs the postfix once. Must run
        /// before publication commits (ancestry scopes before exposure).
        /// </summary>
        internal static FavoredClassBridgeScope Prepare(HarmonyInstance harmony, FavoredClassHostHandles host,
            BlueprintRace human, LibraryScriptableObject library)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            if (host == null || !host.Decision.IsReady || host.PrerequisiteRaceType == null ||
                host.PrerequisiteRaceField == null)
                throw new InvalidOperationException("The ancestry bridge needs a ready exact host.");
            if (human == null) throw new ArgumentNullException("human");
            if (library == null || library.BlueprintsByAssetId == null)
                throw new ArgumentNullException("library");
            var tracked = new HashSet<object>(ReferenceComparer.Instance);
            foreach (KeyValuePair<string, BlueprintFeatureSelection> entry in host.BonusSelections)
            {
                if (entry.Value == null)
                    continue;
                foreach (BlueprintFeature leaf in entry.Value.AllFeatures ?? new BlueprintFeature[0])
                {
                    if (leaf == null)
                        continue;
                    foreach (BlueprintComponent component in leaf.ComponentsArray ?? new BlueprintComponent[0])
                        if (IsHumanPrerequisite(component, host, human))
                            tracked.Add(component);
                }
            }
            int leafPrerequisites = tracked.Count;
            // Every other exact human race prerequisite of the host (its human
            // race traits), each recorded by its owner for the evidence.
            var others = new List<string>();
            foreach (BlueprintScriptableObject blueprint in library.BlueprintsByAssetId.Values.Distinct())
            {
                if (blueprint == null)
                    continue;
                foreach (BlueprintComponent component in blueprint.ComponentsArray ?? new BlueprintComponent[0])
                    if (IsHumanPrerequisite(component, host, human) && tracked.Add(component))
                        others.Add(blueprint.name + ":" + blueprint.AssetGuid);
            }
            others.Sort(StringComparer.Ordinal);
            var scope = new FavoredClassBridgeScope(leafPrerequisites, others.AsReadOnly());
            lock (Gate)
            {
                if (!_installed)
                {
                    MethodInfo check = host.PrerequisiteRaceType.GetMethod("Check", AnyInstance, null,
                        new[] { typeof(FeatureSelectionState), typeof(UnitDescriptor), typeof(LevelUpState) },
                        null);
                    MethodInfo postfix = typeof(FavoredClassHostRaceBridge).GetMethod("CheckPostfix",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (check == null || check.ReturnType != typeof(bool) ||
                        check.DeclaringType != host.PrerequisiteRaceType || postfix == null)
                        throw new InvalidOperationException(
                            "The exact host PrerequisiteRace.Check contract is unavailable.");
                    harmony.Patch(check, null, new HarmonyMethod(postfix), null);
                    _installed = true;
                }
                _tracked = tracked;
                _scope = scope;
            }
            return scope;
        }

        /// <summary>Empties the scope (a failed attachment).</summary>
        internal static void Clear()
        {
            lock (Gate)
            {
                _tracked = new HashSet<object>(ReferenceComparer.Instance);
                _scope = null;
            }
        }

        private static bool IsHumanPrerequisite(BlueprintComponent component, FavoredClassHostHandles host,
            BlueprintRace human)
        {
            return component != null && component.GetType() == host.PrerequisiteRaceType &&
                ReferenceEquals(host.PrerequisiteRaceField.GetValue(component), human);
        }

        private static void CheckPostfix(object __instance, UnitDescriptor __1, ref bool __result)
        {
            if (__result || __1 == null || !IsTracked(__instance))
                return;
            try
            {
                if (FavoredClassRuntime.GrantsHostHumanAccess(__1))
                    __result = true;
            }
            catch (Exception)
            {
                // Fail closed: an evaluation fault never widens access.
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
