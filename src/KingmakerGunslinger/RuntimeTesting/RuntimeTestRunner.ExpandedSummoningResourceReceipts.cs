using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _originalFlightLifecycleSteps;
        // Affected-owner regression within the EXISTING creature review only.
        // Normal review already proves movement/attack/intact view; two further
        // native create/destroy cycles prove early-appearance and repeat cleanup.
        private IEnumerable<int> RepeatOriginalFlightLifecycle(SummonVariantSpec variant)
        {
            string key = variant.Creature.Key;
            for (int cycle = 1; cycle <= 2; cycle++)
            {
                UnitEntityData unit = null;
                try
                {
                    unit = SpawnExpandedSummoningVariants(_creatureReviewBlueprints, _creatureReviewCaster,
                        new[] { variant }, "exact original-flight repeated lifecycle").Single();
                    yield return 0; yield return 0;
                    var view = unit.View;
                    var owned = ExpandedSummoningPteranodonViewPatch.CaptureOriginalFlightOwnedResources(view);
                    var attachment = ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch), "Applied", view);
                    var borrowedMesh = ResourceField(attachment, "OriginalMesh") as Mesh;
                    var borrowedMaterials = (Material[])ResourceField(attachment, "OriginalMaterials");
                    var caches = RosterImmutableCaches();
                    var liveCaches = Resources.FindObjectsOfTypeAll<Texture2D>().Cast<UObject>()
                        .Concat(Resources.FindObjectsOfTypeAll<Mesh>()).Where(o => o != null && caches.ContainsKey(o.GetInstanceID())).ToArray();
                    bool exact = owned.OfType<Mesh>().Count() == 1 && owned.OfType<Material>().Count() == 2 &&
                        owned.All(o => !caches.ContainsKey(o.GetInstanceID())) && borrowedMesh != null &&
                        owned.OfType<Mesh>().Single().vertices.All(v => FiniteResourceVector(v));
                    // Never call the release helper to make this pass: observe
                    // the same native OnDestroy hook used in ordinary gameplay.
                    CleanupExpandedSummoningUnit(unit);
                    for (int frame = 0; frame < 60; frame++)
                    {
                        Game.Instance.EntityDestroyer.Tick(); yield return 0;
                        if (unit.Destroyed && unit.View == null && unit.HoldingState == null && owned.All(o => o == null)) break;
                    }
                    bool released = unit.Destroyed && unit.View == null && unit.HoldingState == null && owned.All(o => o == null);
                    bool borrowed = borrowedMesh != null && borrowedMaterials.All(m => m != null) && liveCaches.All(o => o != null);
                    var beforeRepeat = liveCaches.Select(o => o == null ? 0 : o.GetInstanceID()).ToArray();
                    ExpandedSummoningPteranodonViewPatch.ReleaseOriginalFlightView(view); // exact already-destroyed owner: idempotent no-op
                    bool idempotent = beforeRepeat.SequenceEqual(liveCaches.Select(o => o == null ? 0 : o.GetInstanceID()));
                    _creatureReviewAssertions.Add(Assertion("expanded-summoning-" + key + "-repeated-native-resource-lifecycle-" + cycle,
                        "exact private mesh+two materials become null;donor/cache references survive;repeat teardown is harmless",
                        "captured=" + owned.Length + ";exact=" + exact + ";released=" + released + ";borrowed=" + borrowed + ";idempotent=" + idempotent,
                        exact && released && borrowed && idempotent, "two new native lifecycle cycles on this exact production owner;no force disposal or cache sweep"));
                }
                finally { if (unit != null && !unit.Destroyed) CleanupExpandedSummoningUnit(unit); }
            }
        }
        private static bool FiniteResourceVector(Vector3 value)
        { return !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) &&
            !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z); }

        private readonly Dictionary<string, int> _rosterResourceBaseline = new Dictionary<string, int>(StringComparer.Ordinal);
        private int? _rosterResourceBaselineFrame;
        private string _rosterResourceBaselineError;
        private static string ResourceTypeName(UObject value) { return value.GetType().FullName + "|" + value.name; }
        private void CaptureRosterResourceBaseline()
        {
            if (!RosterPersistenceScope) return;
            try
            {
                foreach (var type in new[] { typeof(Material), typeof(Mesh), typeof(Texture2D) })
                    foreach (var group in Resources.FindObjectsOfTypeAll(type).Where(o => o != null).GroupBy(ResourceTypeName))
                        _rosterResourceBaseline[group.Key] = group.Count();
                _rosterResourceBaselineFrame = Time.frameCount;
            }
            catch (Exception error) { _rosterResourceBaselineError = error.ToString(); }
        }

        private sealed class RosterResourceLink
        {
            internal string Key, UnitId, Guid, Blueprint, Source, Owner;
            internal bool Private, Borrowed, CaptureOwnedResources;
            internal Func<bool> StillOwned;
        }
        private sealed class RosterResourceEntry
        {
            internal UObject Value;
            internal Type Type;
            internal string Name, Flags, Cache;
            internal int Id, CapturedFrame, GlobalBefore;
            internal int? FirstNullFrame;
            internal bool Legacy;
            internal readonly List<RosterResourceLink> Links = new List<RosterResourceLink>();
        }

        // Read exactly the already-audited private ownership records; never
        // enumerate unknown fields, invoke setters or infer a cache by its name.
        private static object ResourceField(object owner, string name)
        {
            var type = owner as Type ?? owner.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic |
                (owner is Type ? BindingFlags.Static : BindingFlags.Instance));
            if (field == null) throw new MissingFieldException(type.FullName, name);
            return field.GetValue(owner is Type ? null : owner);
        }
        private static object ResourceOwnership(Type type, string field, UnitEntityView view)
        {
            var table = ResourceField(type, field); object[] args = { view, null };
            return (bool)table.GetType().GetMethod("TryGetValue").Invoke(table, args) ? args[1] : null;
        }
        private static Dictionary<int, string> RosterImmutableCaches()
        {
            var result = new Dictionary<int, string>();
            Action<UObject, string> add = (value, source) => { if (value != null) result[value.GetInstanceID()] = source; };
            Mesh mesh; Texture2D paint; string[] bones; string status;
            PteranodonAssetRuntime.TryGetMembrane(out mesh, out bones); add(mesh, "PteranodonAssetRuntime._mesh");
            PteranodonAssetRuntime.TryGetAlbedo(out paint); add(paint, "PteranodonAssetRuntime._albedo");
            PteranodonAssetRuntime.TryGetDireBatVisual(out mesh, out bones, out paint); add(mesh, "DireBat.Mesh"); add(paint, "DireBat.Albedo");
            PteranodonAssetRuntime.TryGetEagleVisual(out mesh, out bones, out paint); add(mesh, "Eagle.Mesh"); add(paint, "Eagle.Albedo");
            PteranodonAssetRuntime.TryGetGiantWaspVisual(out mesh, out bones, out paint); add(mesh, "GiantWasp.Mesh"); add(paint, "GiantWasp.Albedo");
            PteranodonAssetRuntime.TryGetStirgeVisual(out mesh, out bones, out paint); add(mesh, "Stirge.Mesh"); add(paint, "Stirge.Albedo");
            foreach (string key in ExpandedSummoningCatalog.All.Select(c => c.Key))
            {
                if (PteranodonAssetRuntime.TryGetUngulateVisual(key, out mesh, out bones, out paint, out status) ||
                    PteranodonAssetRuntime.TryGetSprint12QuadrupedVisual(key, out mesh, out bones, out paint, out status) ||
                    PteranodonAssetRuntime.TryGetSprint13CreatureVisual(key, out mesh, out bones, out paint, out status) ||
                    PteranodonAssetRuntime.TryGetSprint14InsectVisual(key, out mesh, out bones, out paint, out status) ||
                    PteranodonAssetRuntime.TryGetCrocodilianVisual(key, out mesh, out bones, out paint, out status))
                { add(mesh, key + ".Mesh"); add(paint, key + ".Albedo"); }
            }
            return result;
        }

        private static void CaptureRosterUnitResources(UnitEntityData unit, Dictionary<int, RosterResourceEntry> entries,
            Dictionary<int, string> caches)
        {
            var view = unit.View; if (view == null) throw new InvalidOperationException("Receipt unit has no view.");
            var receipt = unit.Get<UnitPartRelease143RosterReceipt>();
            Action<UObject, string, string, bool, bool, bool, Func<bool>> add = (value, source, owner, owned, borrowed, capture, retained) => {
                if (value == null) return;
                int id = value.GetInstanceID(); RosterResourceEntry entry;
                if (!entries.TryGetValue(id, out entry))
                {
                    string cache; caches.TryGetValue(id, out cache);
                    entries.Add(id, entry = new RosterResourceEntry { Value = value, Id = id, Type = value.GetType(),
                        Name = value.name, Flags = value.hideFlags.ToString(), CapturedFrame = Time.frameCount, Cache = cache });
                }
                entry.Links.Add(new RosterResourceLink { Key = receipt.Key, UnitId = unit.UniqueId,
                    Guid = unit.Blueprint.AssetGuid, Blueprint = unit.Blueprint.name, Source = source, Owner = owner,
                    Private = owned, Borrowed = borrowed, CaptureOwnedResources = capture, StillOwned = retained });
            };
            var attachment = ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch), "Applied", view);
            if (attachment != null)
            {
                const string owner = "ExpandedSummoningPteranodonViewPatch.Applied.Attachment";
                foreach (string field in new[] { "Mesh", "Material" })
                {
                    var value = ResourceField(attachment, field) as UObject;
                    string capturedField = field;
                    add(value, "attachment private " + field, owner, true, false, false,
                        () => ReferenceEquals(ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch), "Applied", view), attachment) &&
                            ReferenceEquals(ResourceField(attachment, capturedField), value));
                }
                foreach (string field in new[] { "EagleLunge", "WaspSting", "CrocodilianPose", "StirgeTouch", "BeetleGlow" })
                {
                    var value = ResourceField(attachment, field) as UObject; string capturedField = field;
                    add(value, "attachment owner component " + field, owner, true, false, false,
                        () => ReferenceEquals(ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch), "Applied", view), attachment) &&
                            ReferenceEquals(ResourceField(attachment, capturedField), value));
                }
                add(ResourceField(attachment, "OriginalMesh") as UObject, "attachment donor mesh", owner, false, true, false, null);
                var originals = (Material[])ResourceField(attachment, "OriginalMaterials") ?? new Material[0];
                foreach (var original in originals) add(original, "attachment donor material", owner, false, true, false, null);
                var donor = ResourceField(attachment, "Donor") as SkinnedMeshRenderer;
                if (donor != null && ReferenceEquals(donor.sharedMesh, ResourceField(attachment, "Mesh")))
                    foreach (var driven in donor.sharedMaterials.Where(m => m != null && !originals.Any(o => ReferenceEquals(o, m))))
                        add(driven, "swapped donor renderer material (native controller instance)", owner, true, false, false,
                            () => donor != null && donor.sharedMaterials.Any(m => ReferenceEquals(m, driven)));
            }
            var variant = ResourceOwnership(typeof(ExpandedSummoningVisualVariantPatch), "Ownerships", view) as SummonVisualOwnership;
            if (variant != null)
            {
                const string owner = "ExpandedSummoningVisualVariantPatch.Ownerships.SummonVisualOwnership";
                foreach (var value in variant.Materials.Cast<UObject>().Concat(variant.Textures))
                    add(value, value is Material ? "variant material ownership" : "variant texture", owner, true, false, false,
                        () => variant.Materials.Cast<UObject>().Concat(variant.Textures).Any(o => ReferenceEquals(o, value)));
                foreach (var pair in variant.Originals)
                {
                    foreach (var value in pair.Value) add(value, "variant donor material", owner, false, true, false, null);
                    if (pair.Key != null) foreach (var material in pair.Key.sharedMaterials
                        .Where(m => m != null && !pair.Value.Any(o => ReferenceEquals(o, m))))
                        add(material, "variant renderer controller instance", owner, true, false, false,
                            () => pair.Key != null && pair.Key.sharedMaterials.Any(m => ReferenceEquals(m, material)));
                }
            }
            var snake = view.GetComponent<SerpentineVisualAttachment>();
            var human = view.GetComponent<SalamanderHumanVisualAttachment>();
            if (snake != null)
            {
                foreach (var value in snake.CaptureOwnedResources().Concat(new UObject[] { snake }))
                    add(value, "serpentine ownership seam", snake.GetType().FullName, true, false, true,
                        () => snake != null && snake.CaptureOwnedResources().Any(o => ReferenceEquals(o, value)));
                foreach (var value in snake.CaptureBorrowedAnimationResources())
                    add(value, "serpentine borrowed animation", snake.GetType().FullName, false, true, false, null);
            }
            if (human != null)
            {
                foreach (var value in human.CaptureOwnedResources().Concat(new UObject[] { human }))
                    add(value, "salamander ownership seam", human.GetType().FullName, true, false, true,
                        () => human != null && human.CaptureOwnedResources().Any(o => ReferenceEquals(o, value)));
                foreach (var value in human.BorrowedResources())
                    add(value, "salamander borrowed animation", human.GetType().FullName, false, true, false, null);
            }
            var glow = view.GetComponent<FireBeetleVisualGlow>();
            if (glow != null)
            {
                add(glow, "Fire Beetle glow component", glow.GetType().FullName, true, false, false, () => glow != null);
                foreach (string field in new[] { "_carrier", "_light" })
                {
                    var value = ResourceField(glow, field) as UObject; string capturedField = field;
                    add(value, "Fire Beetle glow " + field, glow.GetType().FullName, true, false, false,
                        () => glow != null && ReferenceEquals(ResourceField(glow, capturedField), value));
                }
            }
            // Cross-check only. A KMG name is grounds for recording UNKNOWN,
            // never grounds for declaring either private ownership or caching.
            Action<UObject, string> scan = (value, source) => {
                if (value != null && (entries.ContainsKey(value.GetInstanceID()) || caches.ContainsKey(value.GetInstanceID()) ||
                    value.name.StartsWith("KMG_", StringComparison.Ordinal))) add(value, source, null, false, false, false, null);
            };
            foreach (var renderer in view.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials.Where(m => m != null))
                {
                    scan(material, "renderer material");
                    foreach (string slot in material.GetTexturePropertyNames()) scan(material.GetTexture(slot), "renderer texture " + slot);
                }
            foreach (var skin in view.GetComponentsInChildren<SkinnedMeshRenderer>(true)) scan(skin.sharedMesh, "skinned mesh");
            foreach (var filter in view.GetComponentsInChildren<MeshFilter>(true)) scan(filter.sharedMesh, "mesh-filter mesh");
            foreach (var value in RosterPersistenceResources(unit))
            {
                scan(value, "historical flat witness cross-check");
                entries[value.GetInstanceID()].Legacy = true;
            }
        }

        private IEnumerable<int> ObserveRosterResourceCleanup(UnitEntityData[] units, string stage)
        {
            var report = new RosterResourceReport(_request.RunId, stage);
            var entries = new Dictionary<int, RosterResourceEntry>();
            int start = Time.frameCount, unitsGoneFrame = -1, observedFrames = 0;
            bool allGone = false;
            try
            {
                var caches = RosterImmutableCaches();
                if (_rosterResourceBaselineError != null) report.Error(null, null, "pre-load-baseline", new InvalidOperationException(_rosterResourceBaselineError));
                foreach (var unit in units)
                    try { CaptureRosterUnitResources(unit, entries, caches); }
                    catch (Exception error) { report.Error(unit.Get<UnitPartRelease143RosterReceipt>()?.Key, unit.UniqueId, "capture-unit-ownership", error); }
                foreach (var group in entries.Values.GroupBy(e => e.Type))
                {
                    var globals = Resources.FindObjectsOfTypeAll(group.Key).Where(o => o != null).ToArray();
                    foreach (var entry in group) entry.GlobalBefore = globals.Count(o => o.name == entry.Name);
                }
                foreach (var unit in units) CleanupExpandedSummoningUnit(unit);
                // Native entity destruction first, then up to60 Unity frames.
                for (int frame = 0; frame < 180; frame++)
                {
                    Game.Instance.EntityDestroyer.Tick(); yield return 0; observedFrames++;
                    foreach (var entry in entries.Values)
                        if (!entry.FirstNullFrame.HasValue && entry.Value == null) entry.FirstNullFrame = Time.frameCount;
                    allGone = units.All(u => u.Destroyed && u.View == null && u.HoldingState == null);
                    if (allGone && unitsGoneFrame < 0) unitsGoneFrame = Time.frameCount;
                    if (unitsGoneFrame >= 0 && Time.frameCount - unitsGoneFrame >= 60) break;
                }
                report.Result["units"] = units.Length; report.Result["allUnitsDestroyed"] = allGone;
                report.Result["cleanupStartFrame"] = start; report.Result["allUnitsDestroyedFrame"] = unitsGoneFrame;
                report.Result["observedFrames"] = observedFrames;
                report.Result["preLoadBaselineFrame"] = _rosterResourceBaselineFrame;
                report.Result["legacyCapturedObjects"] = entries.Values.Count(e => e.Legacy);
                report.Result["legacySurvivors"] = entries.Values.Count(e => e.Legacy && e.Value != null);
                report.Result["totalCapturedReferences"] = entries.Values.Sum(e => e.Links.Count);
                foreach (var group in entries.Values.GroupBy(e => e.Type))
                {
                    var globals = Resources.FindObjectsOfTypeAll(group.Key).Where(o => o != null).ToArray();
                    foreach (var entry in group.OrderBy(e => e.Id)) report.Add(entry.Links[0].Key, entry.Links[0].UnitId, () => {
                        int baseline; bool hasBaseline = _rosterResourceBaseline.TryGetValue(entry.Type.FullName + "|" + entry.Name, out baseline);
                        bool privateOwned = entry.Links.Any(l => l.Private), borrowed = entry.Links.Any(l => l.Borrowed);
                        return new JObject { ["instanceId"] = entry.Id, ["type"] = entry.Type.FullName, ["name"] = entry.Name,
                            ["hideFlags"] = entry.Flags, ["aliveBefore"] = true, ["capturedFrame"] = entry.CapturedFrame,
                            ["aliveAfter"] = entry.Value != null, ["firstUnityNullFrame"] = entry.FirstNullFrame,
                            ["framesUntilNull"] = entry.FirstNullFrame.HasValue ? (int?)(entry.FirstNullFrame.Value - start) : null,
                            ["ownershipClass"] = RosterResourceReport.Classify(privateOwned, entry.Cache != null, borrowed),
                            ["immutableCacheIdentitySource"] = entry.Cache, ["knownBorrowedNative"] = borrowed,
                            ["legacyWitness"] = entry.Legacy,
                            ["fixtureUnitReferences"] = entry.Links.Select(l => l.UnitId).Distinct(StringComparer.Ordinal).Count(),
                            ["preLoadGlobalCountSameTypeName"] = hasBaseline ? (int?)baseline :
                                (entry.Type == typeof(Material) || entry.Type == typeof(Mesh) || entry.Type == typeof(Texture2D)) ? 0 : (int?)null,
                            ["beforeCleanupGlobalCountSameTypeName"] = entry.GlobalBefore,
                            ["afterCleanupGlobalCountSameTypeName"] = globals.Count(o => o.name == entry.Name),
                            ["references"] = new JArray(entry.Links.Select(l => new JObject { ["key"] = l.Key, ["unitId"] = l.UnitId,
                                ["blueprintGuid"] = l.Guid, ["blueprintName"] = l.Blueprint, ["collectionSource"] = l.Source,
                                ["ownerComponent"] = l.Owner, ["privateOwnershipRecord"] = l.Private,
                                ["inCaptureOwnedResources"] = l.CaptureOwnedResources, ["borrowedOwnershipRecord"] = l.Borrowed,
                                ["retainedAsOwnedAfterDestruction"] = l.StillOwned == null ? (bool?)null : entry.Value != null && l.StillOwned() })) };
                    });
                }
            }
            finally
            {
                report.Finish(() => { entries.Clear(); });
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "expanded-summoning-resource-ownership-" + stage + ".json"), report.Result.ToString(Formatting.Indented));
            }
            RosterPersistenceCheck(stage + "-resource-observer", report.Errors.Count == 0 && (bool)report.Result["clearedInFinally"],
                new JObject { ["errors"] = report.Errors.Count, ["cleared"] = report.Result["clearedInFinally"].DeepClone() },
                "complete exact resource receipts, errors isolated, request-local references cleared in finally");
            // Evidence-driven correction: ALL authoritative private resources,
            // not merely the historical95 scanned objects, must be Unity-null.
            // Exact known caches/borrowed assets must survive with stable counts.
            bool gone = allGone && report.Errors.Count == 0 && report.Rows.Cast<JObject>().All(RosterResourceReport.CleanupSatisfied);
            RosterPersistenceCheck(stage + "-native-destruction", gone,
                new JObject { ["units"] = units.Length, ["remainingUnits"] = units.Count(u => !u.Destroyed || u.View != null || u.HoldingState != null),
                    ["capturedProjectResources"] = report.Result["legacyCapturedObjects"]?.DeepClone(),
                    ["remainingResources"] = report.Result["legacySurvivors"]?.DeepClone() },
                "native receipt-owned retirement;zero private resources;exact borrowed/cache references alive and count-stable;unknown/conflicting ownership fails closed");
        }
    }
}
