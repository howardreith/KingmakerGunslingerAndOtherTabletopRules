using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // (real oracle level, earned steps): the effective levels 3, 7, 11, 15
        // and 20 reach the low, middle and capped ends of the tables.
        private static readonly int[][] FcbRevelationValuePairs =
        {
            new[] { 1, 2 }, new[] { 4, 3 }, new[] { 9, 2 }, new[] { 12, 3 }, new[] { 17, 3 },
        };

        // M24, data-driven over every published target whose scope is active:
        // at each pair an oracle of real level L with s earned steps equals a
        // native oracle of level L + s at every read point the target's scope
        // scales (resource maxima, the caster level and DC of its abilities,
        // its rank reads and its own gates), while its BAB stays at level L; a
        // neighboring revelation on the same units is unchanged, and removing
        // the counter restores level L. A target none of whose read points
        // changes at any pair is a failure (its scaling was never observed).
        private JObject ObserveRevelationValues(FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var result = new JObject();
            var library = BlueprintBootstrap.Library;
            string effect = FavoredClassCatalog.EffectSelectedRevelation;
            BlueprintCharacterClass oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FavoredClassRevelationManifest.OracleClassGuid, "Oracle");
            Func<string, BlueprintScriptableObject> blueprint = guid =>
            {
                BlueprintScriptableObject value;
                if (!library.BlueprintsByAssetId.TryGetValue(guid, out value) || value == null)
                    throw new InvalidOperationException("Missing provider blueprint " + guid);
                return value;
            };
            FavoredClassRevelationScope[] active = FavoredClassRevelationManifest.All
                .Where(target => target.Published)
                .Select(target => FavoredClassRevelationScopes.ForKey(target.Key))
                .Where(scope => scope != null && scope.Active).ToArray();
            var inactive = new JArray(FavoredClassRevelationManifest.All.Where(target => target.Published)
                .Select(target => FavoredClassRevelationScopes.ForKey(target.Key))
                .Where(scope => scope == null || !scope.Active)
                .Select(scope => scope == null ? "no scope" : scope.Target.Key + ": " + (scope.PartialReason ?? "inactive")));
            result["inactivePublished"] = inactive;
            result["activeTargets"] = active.Length;
            if (inactive.Count != 0)
                failures.Add("published targets without an active scope: " + string.Join("; ", inactive.Select(value =>
                    (string)value)));
            var targets = new JArray();
            var units = new List<UnitEntityData>();
            try
            {
                UnitEntityData victim = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                units.Add(victim);
                Func<int, FavoredClassRevelationScope[], UnitEntityData> oracleAt = (levels, owned) =>
                {
                    UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                    units.Add(unit);
                    for (int added = 0; added < levels; added++)
                        unit.Descriptor.Progression.AddClassLevel(oracle);
                    foreach (FavoredClassRevelationScope scope in owned)
                        FcbGrantRevelationRoots(unit, scope, blueprint);
                    return unit;
                };
                for (int index = 0; index < active.Length; index++)
                {
                    FavoredClassRevelationScope scope = active[index];
                    FavoredClassRevelationScope neighbor = active[(index + 1) % active.Length];
                    BlueprintFeature leaf = leaves.Pair(effect, scope.Target.Key).Full;
                    var row = new JObject { ["key"] = scope.Target.Key, ["neighbor"] = neighbor.Target.Key };
                    var mismatches = new JArray();
                    var changed = new HashSet<string>(StringComparer.Ordinal);
                    int readPoints = 0;
                    try
                    {
                        foreach (int[] pair in FcbRevelationValuePairs.Concat(FcbGatePairs(scope)))
                        {
                            int level = pair[0], steps = pair[1];
                            var owned = new[] { scope, neighbor };
                            UnitEntityData control = oracleAt(level, owned);
                            UnitEntityData invested = oracleAt(level, owned);
                            UnitEntityData reference = oracleAt(level + steps, owned);
                            GrantFavoredClassRanks(invested, leaf, steps);
                            Dictionary<string, int> controlValues = FcbRevelationReadPoints(control, scope, victim);
                            Dictionary<string, int> investedValues = FcbRevelationReadPoints(invested, scope, victim);
                            Dictionary<string, int> referenceValues = FcbRevelationReadPoints(reference, scope, victim);
                            Dictionary<string, int> neighborControl = FcbRevelationReadPoints(control, neighbor, victim);
                            Dictionary<string, int> neighborInvested = FcbRevelationReadPoints(invested, neighbor, victim);
                            readPoints = controlValues.Count;
                            string at = "L" + level + "+" + steps + " ";
                            foreach (KeyValuePair<string, int> point in investedValues)
                            {
                                if (point.Value != referenceValues[point.Key])
                                    mismatches.Add(at + point.Key + ": invested " + point.Value + ", native at L" +
                                        (level + steps) + " " + referenceValues[point.Key] + ", control " +
                                        controlValues[point.Key]);
                                if (controlValues[point.Key] != referenceValues[point.Key])
                                    changed.Add(point.Key);
                            }
                            if (invested.Stats.BaseAttackBonus.ModifiedValue != control.Stats.BaseAttackBonus.ModifiedValue)
                                mismatches.Add(at + "BAB changed");
                            foreach (KeyValuePair<string, int> point in neighborInvested)
                                if (point.Value != neighborControl[point.Key])
                                    mismatches.Add(at + "neighbor " + neighbor.Target.Key + " " + point.Key + ": " +
                                        neighborControl[point.Key] + " -> " + point.Value);
                            RemoveFavoredClassRanks(invested, leaf);
                            Dictionary<string, int> removed = FcbRevelationReadPoints(invested, scope, victim);
                            // Gates are not compared after an in-session removal:
                            // a replacement chain (a later gated feature removing an
                            // earlier one on apply, as Bleeding Wounds) never steps
                            // down in play (respec rebuilds; the native PostLoad heals
                            // a reload), and the gate lane checks single-step removal.
                            foreach (KeyValuePair<string, int> point in removed.Where(value =>
                                !value.Key.StartsWith("gate ", StringComparison.Ordinal)))
                                if (point.Value != controlValues[point.Key])
                                    mismatches.Add(at + "after removal " + point.Key + ": " + point.Value +
                                        ", control " + controlValues[point.Key]);
                        }
                    }
                    catch (Exception exception)
                    {
                        mismatches.Add("exception: " + exception.GetType().Name + ": " + exception.Message);
                    }
                    row["readPoints"] = readPoints;
                    row["changedPoints"] = changed.Count;
                    row["flatPoints"] = readPoints - changed.Count;
                    if (mismatches.Count != 0)
                    {
                        row["mismatches"] = new JArray(mismatches.Take(12));
                        failures.Add(scope.Target.Key + ": " + mismatches.Count + " mismatch(es), first: " +
                            (string)mismatches[0]);
                    }
                    if (changed.Count == 0)
                        failures.Add(scope.Target.Key + ": no read point changed at any tested level");
                    targets.Add(row);
                }
            }
            finally
            {
                foreach (UnitEntityData unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            result["targets"] = targets;
            result["summary"] = "targets=" + targets.Count + ";readPoints=" +
                targets.Sum(value => (int)value["readPoints"]) + ";changed=" +
                targets.Sum(value => (int)value["changedPoints"]) + ";mismatched=" +
                targets.Count(value => value["mismatches"] != null);
            return result;
        }

        /// <summary>
        /// Grants a target's revelation features as the native pick does: a
        /// selection root (Weapon Mastery) adds the selection and its chosen
        /// item, here the first choice whose own gates are in the scope.
        /// </summary>
        private static void FcbGrantRevelationRoots(UnitEntityData unit, FavoredClassRevelationScope scope,
            Func<string, BlueprintScriptableObject> blueprint)
        {
            foreach (string guid in scope.Target.FeatureGuids)
            {
                var root = (BlueprintFeature)blueprint(guid);
                unit.Descriptor.AddFact(root);
                var selection = root as BlueprintFeatureSelection;
                if (selection == null)
                    continue;
                BlueprintFeature chosen = (selection.AllFeatures ?? new BlueprintFeature[0]).FirstOrDefault(item =>
                    item != null && scope.Gates.Any(gate => ReferenceEquals(gate.Owner, item)));
                if (chosen == null)
                    throw new InvalidOperationException(scope.Target.Key + ": no choice of " + selection.name +
                        " carries a gate of the scope");
                unit.Descriptor.AddFact(chosen);
            }
        }

        /// <summary>
        /// One-step pairs for the scope's own gate levels that no fixed pair
        /// straddles (a gate at level G is crossed by one step from G - 1).
        /// </summary>
        private static IEnumerable<int[]> FcbGatePairs(FavoredClassRevelationScope scope)
        {
            return scope.Gates.Select(gate => gate.Level).Where(level => level >= 2 && level <= 20)
                .Distinct().OrderBy(level => level)
                .Where(level => !FcbRevelationValuePairs.Any(pair => pair[0] < level && pair[0] + pair[1] >= level))
                .Select(level => new[] { level - 1, 1 }).ToArray();
        }

        /// <summary>Every read point a revelation scope scales, as name to value.</summary>
        private static Dictionary<string, int> FcbRevelationReadPoints(UnitEntityData unit,
            FavoredClassRevelationScope scope, UnitEntityData victim)
        {
            var values = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (BlueprintAbilityResource resource in scope.Resources.Keys)
                values["resource " + resource.name + " " + resource.AssetGuid] = resource.GetMaxAmount(unit.Descriptor);
            foreach (BlueprintAbility ability in scope.ParamsAbilities.OfType<BlueprintAbility>())
            {
                var data = new AbilityData(ability, unit.Descriptor);
                MechanicsContext context = data.CreateExecutionContext(new TargetWrapper(victim));
                context.Recalculate();
                values["cl " + ability.name + " " + ability.AssetGuid] = context.Params.CasterLevel;
                values["dc " + ability.name + " " + ability.AssetGuid] = context.Params.DC;
            }
            int source = 0;
            foreach (var read in scope.RankSources)
            {
                var context = new MechanicsContext(unit, unit.Descriptor, read.Value);
                context.Recalculate();
                values["rank " + (source++) + " " + read.Value.name + "|" + read.Key.Type] = context[read.Key.Type];
            }
            foreach (FavoredClassRevelationGate gate in scope.Gates)
                if (gate.Feature != null)
                    values["gate " + gate.Label] = unit.Descriptor.HasFact(gate.Feature) ? 1 : 0;
            return values;
        }
    }
}
