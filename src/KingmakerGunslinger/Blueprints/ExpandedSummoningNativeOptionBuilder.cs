using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningNativeOptionBuilder
    {
        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            SummonNativeExpansionCatalog.Validate();
            foreach (SummonNativeExpansionSpec spec in
                SummonNativeExpansionCatalog.All)
            {
                BlueprintAbility source = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbility>(library, spec.SourceAbilityGuid,
                        "native summon umbrella source");
                BlueprintAbility target = (BlueprintAbility)bySymbol[spec.Symbol];
                ExpandedSummoningAbilityBuilder.CopyFields(source, target);
                target.name = spec.Symbol.Replace('.', '_');
                target.ComponentsArray = (source.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Select(
                        ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                    .ToArray();
                AbilityEffectRunAction effect = target.ComponentsArray
                    .OfType<AbilityEffectRunAction>().Single();
                ContextActionSpawnMonster[] spawns = Objects<
                    ContextActionSpawnMonster>(effect.Actions).ToArray();
                int expected = spec.Branch == SummonNativeSpawnBranch.Direct ?
                    1 : 2;
                if (spawns.Length != expected)
                    throw new InvalidOperationException(
                        "Native summon umbrella branch count changed: " +
                        spec.SourceAbilityGuid + ".");
                ContextActionSpawnMonster chosen = spawns.Single(value =>
                    value.Blueprint != null && string.Equals(
                        value.Blueprint.AssetGuid, spec.UnitGuid,
                        StringComparison.Ordinal));
                effect.Actions = new ActionList { Actions = new GameAction[] {
                    chosen } };
                target.Hidden = false;
                target.ActionBarAutoFillIgnored = false;
                BlueprintUnitFactAccess.Resolve().Configure(target,
                    LocalizationService.Create("KMG.ExpandedSummoning.Native." +
                        spec.Symbol + ".Name", spec.DisplayName + Suffix(
                            spec.Multiplicity)),
                    LocalizationService.Create("KMG.ExpandedSummoning.Native." +
                        spec.Symbol + ".Description", "Summons " +
                        spec.DisplayName + " through Owlcat's native summon " +
                        "lifecycle."), null);
            }
        }

        private static IEnumerable<T> Objects<T>(object root) where T : class
        {
            var found = new List<T>();
            Visit(root, new HashSet<object>(ReferenceComparer.Instance), found);
            return found;
        }

        private static void Visit<T>(object value, ISet<object> seen,
            ICollection<T> found) where T : class
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            T match = value as T;
            if (match != null) found.Add(match);
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                foreach (object item in sequence) Visit(item, seen, found);
                return;
            }
            const BindingFlags flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic;
            foreach (FieldInfo field in value.GetType().GetFields(flags))
                Visit(field.GetValue(value), seen, found);
        }

        private static string Suffix(SummonMultiplicity value)
        { return value == SummonMultiplicity.One ? "" : value ==
            SummonMultiplicity.OneD3 ? " (1d3)" : " (1d4+1)"; }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public new bool Equals(object x, object y)
            { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj)
            { return RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
