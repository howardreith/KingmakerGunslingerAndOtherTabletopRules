using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fresh-process OFF is a wrapper prerequisite. These are the already
        // registered compatibility units, not casts or saved hydration.
        private IEnumerable<int> ReviewSummonCleanup2GiantsOff(ExpandedSummoningCorrectionFixture fixture)
        {
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            var probe = typeof(GiantSummonPresentationPatch).GetField("_probeRunId", statics);
            var palettes = typeof(GiantSummonPaletteRuntime).GetField("Live", statics);
            Func<bool> inactive = () => !_context.FeatureModules.Active.ExpandedSummoning &&
                !ExpandedSummoningVisualVariantPatch.IsModuleActive && !ElementalSourceChooserRuntime.Active &&
                !ElementalSourceChooserView.IsOpen && ElementalSourceChooserView.CurrentSession == null &&
                probe != null && probe.GetValue(null) == null && palettes != null && ((ICollection)palettes.GetValue(null)).Count == 0;
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "giants-off" || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Caster == null || fixture.Scene == null || ExpandedSummoningBlueprints.Registered == null ||
                !BlueprintBootstrap.IsInitialized || Cleanup2NativeDefaultPending || _expandedSummoningPlayerPathCaptureActive ||
                _refinementUnit != null || _refinementAction != null || !inactive()) throw new InvalidOperationException("Fresh actual OFF, idle owned save-free giants-off fixture required.");
            Cleanup2Check("giants-off-actual-module-state", true, new JObject {
                ["activeModule"] = _context.FeatureModules.Active.ExpandedSummoning, ["helperActive"] = ExpandedSummoningVisualVariantPatch.IsModuleActive,
                ["processId"] = Process.GetCurrentProcess().Id, ["processStartUtc"] = Process.GetCurrentProcess().StartTime.ToUniversalTime().ToString("o"),
                ["scope"] = "Fresh OFF startup is wrapper-owned; immutable Active and live helper/probe/chooser state are observed, never toggled. Compatibility construction only; no saved hydration or cast claim." },
                "The actual startup-bound module and chooser are OFF with no scale probe or palette ownership.");
            bool paused = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var watch = Stopwatch.StartNew(); var owned = new List<UnitEntityData>();
            var sources = new List<Cleanup2SharedViewReceipt>();
            try
            {
                foreach (string key in new[] { "cloud-giant", "storm-giant" })
                {
                    if (watch.Elapsed.TotalSeconds > 60) throw new TimeoutException("Giants OFF 60-second phase bound.");
                    var profile = GiantSummonPresentationPolicy.All.Single(value => value.Key == key);
                    string symbol = key == "cloud-giant" ? "KMG.Summoning.Unit.CloudGiant" : "KMG.Summoning.Unit.StormGiant";
                    var unit = ExpandedSummoningBlueprints.Registered.BySymbol[symbol] as BlueprintUnit;
                    var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                        ExpandedSummoningDonorCatalog.For(key).Guid, "giants-off exact native donor");
                    if (unit == null || unit.AssetGuid != profile.UnitGuid || unit.name != symbol.Replace('.', '_') ||
                        !ReferenceEquals(unit, BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library, profile.UnitGuid, symbol)) ||
                        unit.Prefab == null || unit.Prefab.AssetId != profile.NativePrefabId || donor.Prefab?.AssetId != profile.NativePrefabId)
                        throw new InvalidOperationException("OFF compatibility unit/native donor identity differs: " + key);
                    object unitPrefab = unit.Prefab, donorPrefab = donor.Prefab;
                    UnitEntityView prefab = donor.Prefab.Load(false);
                    var source = Cleanup2SharedTakeView(prefab, true); sources.Add(source);
                    UnitEntityData actor = Game.Instance.EntityCreator.SpawnUnit(unit, fixture.Caster.Position, Quaternion.identity, fixture.Scene);
                    bool fresh = actor != null && ReferenceEquals(actor.Blueprint, unit) && !fixture.Created.Contains(actor) &&
                        !fixture.UnitsBefore.Any(value => ReferenceEquals(value, actor)) && !fixture.ExactStart.Any(value => ReferenceEquals(value, actor)) &&
                        !fixture.PartyBefore.Any(value => ReferenceEquals(value, actor)) && !ReferenceEquals(actor, fixture.Caster) && !ReferenceEquals(actor, fixture.Hostile);
                    if (fresh) { fixture.Created.Add(actor); owned.Add(actor); SetExpandedSummoningBrainActive(actor, false); }
                    Game.Instance.EntityCreator.Tick();
                    if (!fresh || !actor.IsInState || !ReferenceEquals(actor.HoldingState, fixture.Scene) || actor.View == null || !ReferenceEquals(actor.View.Data, actor))
                        throw new InvalidOperationException("Exact new OFF compatibility actor creation receipt refused: " + key);
                    foreach (int frame in Cleanup2WaitAppearance(actor, "giants-off-" + key))
                    { if (watch.Elapsed.TotalSeconds > 60) throw new TimeoutException("Giants OFF 60-second phase bound."); yield return frame; }
                    var original = Cleanup2CaptureScaleSource(actor); var stats = Cleanup2SharedStats(actor);
                    var meshes = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    bool nativeBody = meshes.Any(value => source.Meshes.Contains(value.sharedMesh) && Cleanup2BodyRenderer(value));
                    _sprint25WaitingForUnityAppearance = true;
                    for (int frame = 0; frame < 12; frame++)
                    { if (!inactive() || watch.Elapsed.TotalSeconds > 60) throw new InvalidOperationException("Actual OFF state changed or phase bound exceeded."); yield return 0; }
                    var components = actor.View.GetComponentsInChildren<MonoBehaviour>(true);
                    bool noAllocations = GiantSummonPresentationPatch.ProductionOwner(actor.View) == null &&
                        !(bool)GiantSummonPresentationPatch.ObserveProductionOwnership(actor.View)["ownerPresent"] &&
                        !components.Any(value => value is GiantSummonRenderScaleLifetime || value is GiantSummonPaletteLifetime || value is Cleanup2GiantRenderScalePrototype) &&
                        !actor.View.GetComponentsInChildren<Transform>(true).Any(value => value.name.StartsWith("KMG_C2_", StringComparison.Ordinal)) &&
                        original.Meshes.All(value => value != null && !value.name.StartsWith("KMG_", StringComparison.Ordinal)) &&
                        original.Materials.SelectMany(value => value).All(value => value != null && !value.name.StartsWith("KMG_GiantPalette_", StringComparison.Ordinal));
                    bool exact = inactive() && nativeBody && noAllocations && Cleanup2ScaleSourceRestored(original) &&
                        JToken.DeepEquals(stats, Cleanup2SharedStats(actor)) && Cleanup2SharedSameView(source) &&
                        ReferenceEquals(unit.Prefab, unitPrefab) && ReferenceEquals(donor.Prefab, donorPrefab);
                    Cleanup2Check("giants-off-native-view-" + key, exact, new JObject {
                        ["key"] = key, ["unit"] = actor.UniqueId, ["blueprint"] = unit.AssetGuid, ["compatibilitySymbol"] = symbol,
                        ["nativeDonor"] = donor.AssetGuid, ["nativePrefab"] = donor.Prefab.AssetId, ["freshExactCreation"] = fresh,
                        ["nativeBodyMeshRetained"] = nativeBody, ["noPresentationAllocations"] = noAllocations,
                        ["view"] = Cleanup2SharedViewScalars(actor.View), ["stats"] = stats, ["colliders"] = Cleanup2ClothColliderState(actor, false),
                        ["scaleOwnership"] = GiantSummonPresentationPatch.ObserveProductionOwnership(actor.View),
                        ["palette"] = GiantSummonPaletteRuntime.Describe(actor.View), ["sourcePrefabUnchanged"] = Cleanup2SharedSameView(source),
                        ["scope"] = "Actual native EntityCreator/view construction of the existing OFF compatibility blueprint. Original source mesh/material/bone/collider/configuration identities remain; native material-controller instances are allowed. No summon/rule call, forced visual apply, saved hydration or mechanical acceptance." },
                        "Cloud/Storm compatibility views construct normally in actual OFF without private scale/palette/renderers or native source changes.");
                    if (!exact) throw new InvalidOperationException("OFF giant presentation preservation failed: " + key);
                }
            }
            finally
            {
                var errors = new JArray();
                foreach (UnitEntityData actor in owned)
                    try { if (fixture.Created.Contains(actor)) DisposeExpandedSummoningUnits(fixture.Created, new[] { actor }); }
                    catch (Exception error) { errors.Add(error.GetType().Name + ":" + error.Message); }
                _sprint25WaitingForUnityAppearance = waiting; Game.Instance.IsPaused = paused;
                Cleanup2Check("giants-off-owned-cleanup-issued", errors.Count == 0, new JObject { ["errors"] = errors,
                    ["positiveActors"] = new JArray(owned.Select(value => value.UniqueId)), ["foreignAdoption"] = false },
                    "Only exact positive creation returns are disposed; shared fixture teardown retains any unresolved owned actor.");
            }
            var drain = Stopwatch.StartNew();
            for (int frame = 0; frame < 120 && drain.Elapsed.TotalSeconds < 3 && owned.Any(value => !value.Destroyed || value.View != null || value.HoldingState != null); frame++)
            { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool released = owned.Count == 2 && owned.All(value => value.Destroyed && value.View == null && value.HoldingState == null && !fixture.Created.Contains(value)) &&
                inactive() && sources.All(Cleanup2SharedSameView);
            Cleanup2Check("giants-off-exact-release", released, new JObject { ["released"] = released,
                ["sourcePrefabsExact"] = sources.All(Cleanup2SharedSameView), ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                ["scope"] = "Bounded exact-reference cleanup; no global object count or saved-hydration claim." },
                "Both exact compatibility actors release while native donor prefab assets and actual OFF state remain unchanged.");
            if (!released) throw new InvalidOperationException("OFF giant exact release remains unresolved.");
        }
    }
}
