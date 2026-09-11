using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Diagnostic-only lifecycle probe: commits one real native creator, performs
    // the native area boundary (Game.ReloadArea, the same cleanup an actual
    // travel runs), then observes the committed character's world appearance and
    // a second real creator. Every observation is read-only; this partial never
    // reloads donors, rebuilds proxies, or repairs the scene before measuring.
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private const BindingFlags LifecycleMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo LoadedResourceResource = typeof(Kingmaker.Blueprints.ResourcesLibrary)
            .GetNestedType("LoadedResource", BindingFlags.NonPublic | BindingFlags.Public)?
            .GetField("Resource", BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo LoadedResourceCounter = typeof(Kingmaker.Blueprints.ResourcesLibrary)
            .GetNestedType("LoadedResource", BindingFlags.NonPublic | BindingFlags.Public)?
            .GetField("RequestCounter", BindingFlags.Instance | BindingFlags.Public);

        private UnitEntityData _lifecycleUnit;
        private Kingmaker.UnitLogic.DollData _lifecycleDoll;
        private string _lifecycleBodyAssetId;
        private JObject _lifecycleBefore;
        private string _lifecycleStage;
        private int _lifecycleSettle;
        private bool _lifecycleBoundaryComplete;
        private readonly JArray _lifecycleEvidence = new JArray();
        internal readonly bool _visualLifecycle;
        private Harmony12.HarmonyInstance _unloadObserver;
        private string _unloadObserverId;

        private bool VisualLifecyclePending
        {
            get { return _visualLifecycle && !_lifecycleBoundaryComplete; }
        }

        // Read-only recorder over the native destructive boundary: which asset
        // the creator asked the resource library to unload, with caller stack,
        // and whether the periodic unused-asset sweep ran. It never changes a
        // return value or suppresses a native unload.
        internal void ArmUnloadObserver()
        {
            if (_unloadObserver != null) return;
            _unloadObserverId = "KMG.CreatorVisualLifecycle.Unload." + _request.RunId;
            _unloadObserver = Harmony12.HarmonyInstance.Create(_unloadObserverId);
            MethodInfo unload = typeof(Kingmaker.Blueprints.ResourcesLibrary).GetMethod(
                "TryUnloadResource", BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(string) }, null);
            _unloadObserver.Patch(unload, prefix: new HarmonyMethod(
                typeof(ElementalCharacterCreationBaselineScenario).GetMethod(
                    "RecordNativeUnload", BindingFlags.Static | BindingFlags.NonPublic)));
        }

        internal void DisarmUnloadObserver()
        {
            if (_unloadObserver == null) return;
            try { _unloadObserver.UnpatchAll(_unloadObserverId); }
            finally { _unloadObserver = null; }
        }

        private static void RecordNativeUnload(string assetId)
        {
            var owner = _saveGuardOwner;
            if (owner == null || !owner._visualLifecycle) return;
            try
            {
                owner._lifecycleEvidence.Add(new JObject {
                    ["nativeTryUnloadResource"] = assetId,
                    ["stage"] = owner._stage,
                    ["stack"] = new System.Diagnostics.StackTrace(2, true).ToString() });
            }
            catch { /* observation only */ }
        }

        private void CaptureLifecycleCommit()
        {
            var visuals = BlueprintBootstrap.ElementalRaces.Visuals;
            string kind = BlueprintBootstrap.ElementalRaces.OrderedBlueprints()
                .Single(value => ReferenceEquals(value.Race, _races[_raceIndex])).Definition.Kind.ToString();
            string bodySymbol = "KMG.ElementalRaces." + kind + ".Visual.Body.Male";
            _lifecycleBodyAssetId = visuals.Ordered().SelectMany(value => value.Resources)
                .Single(value => string.Equals(value.Spec.Symbol, bodySymbol, StringComparison.Ordinal)).AssetId;
            _lifecycleDoll = CommittedCreatorOwner.Doll;
            RequireLifecycle(_lifecycleDoll != null,
                "The committed creator must expose its native DollData.");
            // The doll's serialized entity ids may settle asynchronously after
            // the synchronous commit; record actual membership as evidence and
            // let the world-view checkpoints provide the behavioral verdict.
            var dollEntityIds = _lifecycleDoll.EquipmentEntityIds ?? new List<string>();
            _character["lifecycleCommit"] = new JObject {
                ["bodyAssetId"] = _lifecycleBodyAssetId,
                ["dollEntityCount"] = dollEntityIds.Count,
                ["dollEntityIds"] = new JArray(dollEntityIds),
                ["dollReferencesBodyProxy"] = dollEntityIds.Contains(_lifecycleBodyAssetId),
                ["registry"] = CaptureLifecycleCheckpoint("after-first-commit", true) };
        }

        private void PollLifecycleBoundary()
        {
            if (_lifecycleStage == null)
            {
                _lifecycleBefore = CaptureLifecycleCheckpoint("before-area-boundary", true);
                _character["lifecycleBefore"] = _lifecycleBefore;
                RequireLifecycle(LoadedResourceResource != null && LoadedResourceCounter != null,
                    "The native LoadedResource contract is unavailable for lifecycle observation.");
                RequireLifecycle(_lifecycleDoll != null && _lifecycleUnit != null &&
                    (bool?)_character["completed"] == true,
                    "The lifecycle boundary requires one committed creator fixture.");
                Game.Instance.IsPaused = true;
                _lifecycleStage = "reload-requested";
                _lifecycleSettle = 0;
                _lifecycleEvidence.Add(new JObject { ["event"] = "native-area-reload-requested" });
                Game.Instance.ReloadArea();
                return;
            }
            if (_lifecycleStage == "reload-requested")
            {
                LoadingProcess loading = LoadingProcess.Instance;
                if (loading == null || loading.IsLoadingInProcess || loading.IsLoadingScreenActive ||
                    loading.IsManualLoadingScreenActive || loading.IsAwaitingUserInput) return;
                if (++_lifecycleSettle < 6) return;
                JObject after = CaptureLifecycleCheckpoint("after-area-boundary", true);
                _character["lifecycleAfter"] = after;
                _lifecycleEvidence.Add(new JObject { ["event"] = "native-area-reload-completed",
                    ["beforeFirstLoss"] = _lifecycleBefore == null || _lifecycleBefore["retentionEvaluation"] == null ? null :
                        _lifecycleBefore["retentionEvaluation"]["firstFailure"],
                    ["afterFirstLoss"] = after["retentionEvaluation"] == null ? null :
                        after["retentionEvaluation"]["firstFailure"] });
                _lifecycleStage = "complete";
                return;
            }
            if (_lifecycleStage == "complete")
            {
                // The visit-1 mercenary fixture was intentionally kept alive across
                // the boundary; retire it now through the normal owned cleanup so
                // the world is exactly restored before the second creator opens.
                _unit = _lifecycleUnit;
                _lifecycleUnit = null;
                CleanupCreatorMembership();
                _unit = null;
                if (!CreatorMembershipRestored())
                    throw new InvalidOperationException("Lifecycle boundary did not restore creator membership: " + _creatorCleanupEvidence);
                _lifecycleBoundaryComplete = true;
                _settle = 12;
            }
        }

        private static void RequireLifecycle(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("lifecycle: " + message);
        }

        // Read-only snapshot of the exact registered visual resources, their
        // native cache state, and which retention precondition would fail first.
        private JObject CaptureLifecycleCheckpoint(string checkpoint, bool includeCommittedView)
        {
            var visuals = BlueprintBootstrap.ElementalRaces == null ? null : BlueprintBootstrap.ElementalRaces.Visuals;
            if (visuals == null)
                return new JObject { ["checkpoint"] = checkpoint, ["visuals"] = "unavailable" };
            IDictionary cache = Kingmaker.Blueprints.ResourcesLibrary.LoadedResources as IDictionary;
            RequireLifecycle(cache != null, "The loaded-resource cache is unavailable.");
            var proxies = visuals.Ordered().SelectMany(value => value.Resources).ToArray();
            var proxyRows = new JArray();
            string firstFailure = null;
            string firstFailureAsset = null;
            foreach (var registration in proxies)
            {
                string state;
                EvaluateResource(cache, registration.AssetId, registration.Resource, "owned", out state);
                if (firstFailure == null && state != null) { firstFailure = state; firstFailureAsset = registration.AssetId; }
                proxyRows.Add(DescribeCachedResource(cache, registration.AssetId, registration.Resource, state));
            }
            var donorRows = new JArray();
            foreach (string id in visuals.NativeDependencyIds)
            {
                string state;
                EvaluateCacheEntry(cache, id, "donor", out state);
                if (firstFailure == null && state != null) { firstFailure = state; firstFailureAsset = id; }
                donorRows.Add(DescribeCacheEntry(cache, id, "donor", out state));
            }
            var result = new JObject
            {
                ["checkpoint"] = checkpoint,
                ["proxies"] = proxyRows,
                ["donors"] = donorRows,
                ["dollRooms"] = DescribeLifecycleDollRooms(),
                ["retentionEvaluation"] = new JObject {
                    ["wouldThrow"] = firstFailure != null,
                    ["firstFailure"] = firstFailure,
                    ["firstFailureAsset"] = firstFailureAsset }
            };
            if (includeCommittedView && _lifecycleDoll != null)
                result["committedWorldView"] = DescribeCommittedWorldView(checkpoint);
            return result;
        }

        private void EvaluateResource(IDictionary cache, string assetId,
            EquipmentEntity registered, string role, out string state)
        {
            state = null;
            if (registered == null) { state = role + "-object-destroyed"; return; }
            if (!cache.Contains(assetId)) { state = role + "-evicted-from-cache"; return; }
            UnityEngine.Object current = CurrentLoadedResource(cache[assetId]);
            if (!ReferenceEquals(current, registered)) { state = role + "-cache-entry-replaced"; return; }
            if (CountDestroyedInnerAssets(registered) != 0) { state = role + "-inner-asset-destroyed"; }
        }

        private void EvaluateCacheEntry(IDictionary cache, string assetId,
            string role, out string state)
        {
            state = null;
            if (!cache.Contains(assetId)) { state = role + "-evicted-from-cache"; return; }
            UnityEngine.Object current = CurrentLoadedResource(cache[assetId]);
            if (current == null) { state = role + "-object-destroyed"; return; }
            EquipmentEntity entity = current as EquipmentEntity;
            if (entity == null) { state = role + "-cache-entry-type-changed"; return; }
            if (CountDestroyedInnerAssets(entity) != 0) { state = role + "-inner-asset-destroyed"; }
        }

        private JObject DescribeCachedResource(IDictionary cache, string assetId,
            EquipmentEntity registered, string state)
        {
            return new JObject {
                ["assetId"] = assetId, ["name"] = registered == null ? null : registered.name,
                ["objectAlive"] = registered != null, ["cacheContains"] = cache.Contains(assetId),
                ["requestCounter"] = CounterOf(cache, assetId), ["state"] = state ?? "retained" };
        }

        private JObject DescribeCacheEntry(IDictionary cache, string assetId,
            string role, out string state)
        {
            EvaluateCacheEntry(cache, assetId, role, out state);
            UnityEngine.Object current = cache.Contains(assetId) ? CurrentLoadedResource(cache[assetId]) : null;
            return new JObject {
                ["assetId"] = assetId, ["name"] = current == null ? null : current.name,
                ["objectAlive"] = current != null, ["cacheContains"] = cache.Contains(assetId),
                ["requestCounter"] = CounterOf(cache, assetId), ["state"] = state ?? "retained" };
        }

        private static UnityEngine.Object CurrentLoadedResource(object loaded)
        {
            return loaded == null || LoadedResourceResource == null ? null :
                LoadedResourceResource.GetValue(loaded) as UnityEngine.Object;
        }

        private static int CounterOf(IDictionary cache, string assetId)
        {
            if (!cache.Contains(assetId) || LoadedResourceCounter == null) return -1;
            object value = LoadedResourceCounter.GetValue(cache[assetId]);
            return value is int ? (int)value : -1;
        }

        private static int CountDestroyedInnerAssets(EquipmentEntity entity)
        {
            try
            {
                return entity.GetInnerAssets().Count(value => value == null);
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private static JArray DescribeLifecycleDollRooms()
        {
            return new JArray(UnityEngine.Resources.FindObjectsOfTypeAll<CharGenDollRoom>()
                .Select(room =>
                {
                    DollState pending = (DollState)typeof(CharGenDollRoom)
                        .GetField("m_DollStateForUpdate", LifecycleMembers).GetValue(room);
                    var loaded = (List<EquipmentEntityLink>)typeof(CharGenDollRoom)
                        .GetField("m_EquipmentEntitiesLoaded", LifecycleMembers).GetValue(room);
                    Character male = (Character)typeof(CharGenDollRoom)
                        .GetField("m_MaleAvatar", LifecycleMembers).GetValue(room);
                    Character female = (Character)typeof(CharGenDollRoom)
                        .GetField("m_FemaleAvatar", LifecycleMembers).GetValue(room);
                    return new JObject {
                        ["instanceId"] = room.GetInstanceID(),
                        ["pendingDollStateStuck"] = pending != null,
                        ["loadedEntityLinks"] = loaded == null ? 0 : loaded.Count,
                        ["maleAvatarEntities"] = AvatarEntityCount(male),
                        ["femaleAvatarEntities"] = AvatarEntityCount(female) };
                }));
        }

        private static int AvatarEntityCount(Character avatar)
        {
            return avatar == null || avatar.EquipmentEntities == null ? 0 : avatar.EquipmentEntities.Count;
        }

        // Native world-appearance assembly for the committed fixture: the same
        // DollData-to-view path a world spawn performs, with renderer-level
        // drawable evidence. Never mutates the unit or the scene.
        private JObject DescribeCommittedWorldView(string checkpoint)
        {
            var evidence = new JObject { ["checkpoint"] = checkpoint };
            try
            {
                UnitEntityView view = _lifecycleDoll.CreateUnitView(false);
                if (view == null || view.GetComponent<Character>() == null)
                {
                    evidence["viewCreated"] = false;
                    return evidence;
                }
                int settle = 0;
                while (!ElementalRaceDevelopmentProbeScenario.ViewReady(view) &&
                    settle++ < 120) Game.Instance.EntityCreator.Tick();
                Character avatar = view.GetComponent<Character>();
                Renderer[] renderers = view.GetComponentsInChildren<Renderer>(true);
                Material[] materials = renderers.Where(value => value != null)
                    .SelectMany(value => value.sharedMaterials ?? new Material[0]).ToArray();
                var visuals = BlueprintBootstrap.ElementalRaces.Visuals;
                EquipmentEntity body = visuals.Ordered().SelectMany(value => value.Resources)
                    .SingleOrDefault(value => string.Equals(value.AssetId, _lifecycleBodyAssetId, StringComparison.Ordinal))?.Resource;
                evidence["viewCreated"] = true;
                evidence["avatarEntities"] = avatar.EquipmentEntities == null ? 0 : avatar.EquipmentEntities.Count;
                evidence["bodyResourceResolves"] = Kingmaker.Blueprints.ResourcesLibrary.TryGetResource<EquipmentEntity>(
                    _lifecycleBodyAssetId, false) != null;
                evidence["bodyEntityOnAvatar"] = body != null && avatar.EquipmentEntities != null &&
                    avatar.EquipmentEntities.Any(value => ReferenceEquals(value, body));
                evidence["renderableRenderers"] = renderers.Count(value => value != null && value.enabled &&
                    value.sharedMaterials != null && value.sharedMaterials.Length > 0 &&
                    value.sharedMaterials.All(material => material != null && material.shader != null));
                evidence["bakedCharacterRenderers"] = renderers.Count(value => value != null &&
                    value.name != null && value.name.StartsWith("Renderer_Character_", StringComparison.Ordinal));
                evidence["nullMaterials"] = materials.Count(value => value == null);
                evidence["nullShaders"] = materials.Count(value => value != null && value.shader == null);
                evidence["avatarEntityNames"] = new JArray((avatar.EquipmentEntities ?? new List<EquipmentEntity>())
                    .Select(value => value == null ? "<destroyed>" : value.name).ToArray());
                ElementalRaceDevelopmentProbeScenario.DestroyView(view);
            }
            catch (Exception error)
            {
                evidence["inspectionError"] = error.ToString();
            }
            return evidence;
        }
    }
}
