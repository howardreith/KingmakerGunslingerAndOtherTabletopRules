using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // E10 natively: the live permission graph, extended for one scope at a
        // time through the runtime-test seam, stays bounded, cycle-safe and
        // closed. Each case is a real level-1 Gunslinger visit that must offer
        // exactly the counters of the permitted ancestries, in one favored-class
        // selection, with no extra reward and no permissive fallback.
        private JObject ObservePermissionBounds(BlueprintCharacterClass gunslinger, FavoredClassBlueprintSet leaves,
            BlueprintFeatureSelection selection, ICollection<string> reserved, IList<string> failures)
        {
            var result = new JObject();
            var library = BlueprintBootstrap.Library;
            Func<string, BlueprintRace> race = ancestry => BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            Func<string, string, FavoredClassPermissionEdge> edge = (from, to) =>
                new FavoredClassPermissionEdge(from, to, FavoredClassPermissionBasis.HalfElfFaq, null);
            string grit = FavoredClassCatalog.EffectGrit, confirmation = FavoredClassCatalog.EffectFirearmConfirmation,
                initiative = FavoredClassCatalog.EffectInitiative;

            // An unconditional cycle through Gnome back to Human, plus Elf.
            result["cycle"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures, "cycle",
                race(FavoredClassAncestry.Human), null, new[]
                {
                    edge(FavoredClassAncestry.Human, FavoredClassAncestry.Gnome),
                    edge(FavoredClassAncestry.Gnome, FavoredClassAncestry.Human),
                    edge(FavoredClassAncestry.Gnome, FavoredClassAncestry.Elf),
                    edge(FavoredClassAncestry.Elf, FavoredClassAncestry.Gnome),
                }, new[] { grit, confirmation });
            // A chain of five edges: the fifth ancestry (Dwarf) is past the
            // depth bound of four and is never reached.
            result["depth"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures, "depth",
                race(FavoredClassAncestry.Human), null, new[]
                {
                    edge(FavoredClassAncestry.Human, FavoredClassAncestry.Gnome),
                    edge(FavoredClassAncestry.Gnome, FavoredClassAncestry.Ganzi),
                    edge(FavoredClassAncestry.Ganzi, FavoredClassAncestry.Suli),
                    edge(FavoredClassAncestry.Suli, FavoredClassAncestry.Elf),
                    edge(FavoredClassAncestry.Elf, FavoredClassAncestry.Dwarf),
                }, new[] { grit, confirmation });
            result["depthBound"] = FavoredClassPermissionGraph.MaximumDepth;
            // An edge that requires an unverified fact never applies.
            result["unverifiedFact"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures,
                "unverified fact", race(FavoredClassAncestry.Human), null, new[]
                {
                    new FavoredClassPermissionEdge(FavoredClassAncestry.Human, FavoredClassAncestry.Dwarf,
                        FavoredClassPermissionBasis.MostlyHuman, "KMG.Runtime.UnverifiedFact"),
                }, new[] { grit });
            // A duplicated verified identity (the Mostly Human fact twice on an
            // Ifrit) opens each counter once.
            BlueprintFeature identity = BlueprintBootstrap.MostlyHuman == null ? null : BlueprintBootstrap.MostlyHuman.Identity;
            if (identity == null)
                failures.Add("the Mostly Human identity is not registered");
            else
                result["duplicateIdentity"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures,
                    "duplicate identity", race(FavoredClassAncestry.Ifrit), unit =>
                    {
                        unit.Descriptor.AddFact(identity);
                        unit.Descriptor.AddFact(identity);
                    }, null, new[] { grit, initiative });
            // A stray racial fact from another race proves nothing: ancestry
            // comes from the race blueprint only.
            BlueprintRace ifrit = race(FavoredClassAncestry.Ifrit);
            BlueprintFeature stray = ifrit.Features == null ? null : ifrit.Features.OfType<BlueprintFeature>()
                .FirstOrDefault(value => value != null);
            if (stray == null)
                failures.Add("the Ifrit race has no racial feature to use as a stray fact");
            else
                result["strayRaceFact"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures,
                    "stray race fact", race(FavoredClassAncestry.Human), unit => unit.Descriptor.AddFact(stray),
                    null, new[] { grit });
            // A playable race that is not a recognized identity (an unknown
            // provider's race) is offered nothing.
            var known = new HashSet<string>(FavoredClassRaceIdentities.All.Select(value => value.RaceGuid),
                StringComparer.Ordinal);
            BlueprintRace unknown = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.Progression.CharacterRaces
                .Where(value => value != null && !known.Contains(value.AssetGuid))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            result["unknownRace"] = unknown == null ?
                (JToken)"no unrecognized playable race is installed (the domain test covers an unknown identity)" :
                unknown.name + " " + unknown.AssetGuid;
            if (unknown != null)
                result["unknownProvider"] = FcbPermissionCase(gunslinger, leaves, selection, reserved, failures,
                    "unknown race", unknown, null, null, new string[0]);
            return result;
        }

        private JObject FcbPermissionCase(BlueprintCharacterClass gunslinger, FavoredClassBlueprintSet leaves,
            BlueprintFeatureSelection selection, ICollection<string> reserved, IList<string> failures, string label,
            BlueprintRace race, Action<UnitEntityData> prepare, FavoredClassPermissionEdge[] extra, string[] expected)
        {
            var row = new JObject { ["race"] = race.name };
            UnitEntityData unit = null;
            LevelUpController controller = null;
            IDisposable extension = null;
            try
            {
                if (extra != null)
                    extension = FavoredClassRuntime.ExtendPermissionGraphForRuntimeTest(extra);
                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                if (prepare != null) prepare(unit);
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                    "KMG FCB Permission " + label);
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger, row) == null)
                {
                    if (expected.Length != 0)
                        failures.Add(label + ": favored Gunslinger progression unavailable");
                    row["offered"] = new JArray();
                    return row;
                }
                row["permitted"] = new JArray(FavoredClassRuntime.PermittedAncestries(controller.Preview)
                    .OrderBy(value => value, StringComparer.Ordinal));
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                int openStates = controller.State.Selections.Count(state =>
                    ReferenceEquals(state.Selection, selection));
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller, selection.AssetGuid);
                var offered = new List<string>();
                if (fcb != null)
                    foreach (FavoredClassLeafPair pair in leaves.Pairs)
                        if (FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                            (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                            if (!offered.Contains(pair.Effect.Id)) offered.Add(pair.Effect.Id);
                row["offered"] = new JArray(offered.OrderBy(value => value, StringComparer.Ordinal));
                row["favoredClassSelections"] = openStates;
                if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                        expected.OrderBy(value => value, StringComparer.Ordinal)))
                    failures.Add(label + ": offered " + string.Join(",", offered.ToArray()) + ", expected " +
                        string.Join(",", expected));
                if (openStates > 1)
                    failures.Add(label + ": " + openStates + " favored-class selections at one level");
                return row;
            }
            catch (Exception exception)
            {
                failures.Add(label + ": " + exception.GetType().Name + ": " + exception.Message);
                row["exception"] = exception.ToString();
                return row;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
                if (unit != null) unit.Dispose();
                if (extension != null) extension.Dispose();
            }
        }
    }
}
