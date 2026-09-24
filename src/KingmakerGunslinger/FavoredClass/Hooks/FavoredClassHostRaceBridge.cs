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
    /// <summary>
    /// Narrow ancestry bridge (charter section 6.5). The host's
    /// <c>PrerequisiteRace.Check</c> uses exact race equality. Only the exact
    /// human prerequisite instances on leaves of the host's own favored-class
    /// bonus selections are tracked, and for those only the verified Mostly
    /// Human permission of a geniekin parent is added: every native exact-race
    /// result stays, every other prerequisite (and every other race, trait or
    /// host policy) is untouched, and nothing changes the unit's race.
    /// </summary>
    internal static class FavoredClassHostRaceBridge
    {
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly object Gate = new object();
        private static bool _installed;
        private static volatile HashSet<object> _tracked = new HashSet<object>(ReferenceComparer.Instance);

        internal static int TrackedCount
        {
            get { return _tracked.Count; }
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
        internal static int Prepare(HarmonyInstance harmony, FavoredClassHostHandles host,
            BlueprintRace human)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            if (host == null || !host.Decision.IsReady || host.PrerequisiteRaceType == null ||
                host.PrerequisiteRaceField == null)
                throw new InvalidOperationException("The ancestry bridge needs a ready exact host.");
            if (human == null) throw new ArgumentNullException("human");
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
                        if (component != null && component.GetType() == host.PrerequisiteRaceType &&
                            ReferenceEquals(host.PrerequisiteRaceField.GetValue(component), human))
                            tracked.Add(component);
                }
            }
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
            }
            return tracked.Count;
        }

        /// <summary>Empties the scope (publication rollback or disable).</summary>
        internal static void Clear()
        {
            lock (Gate)
                _tracked = new HashSet<object>(ReferenceComparer.Instance);
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
