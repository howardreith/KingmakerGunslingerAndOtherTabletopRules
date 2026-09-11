using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal sealed class ElementalRaceVisualResourceRegistration
    {
        private EquipmentEntity _resource;

        internal ElementalRaceVisualResourceRegistration(
            ElementalRaceVisualProxySpec spec, string assetId,
            EquipmentEntity resource, bool usedFallback)
        {
            Spec = spec ?? throw new ArgumentNullException("spec");
            AssetId = assetId ?? throw new ArgumentNullException("assetId");
            _resource = resource ?? throw new ArgumentNullException("resource");
            UsedFallback = usedFallback;
        }

        internal ElementalRaceVisualProxySpec Spec { get; private set; }
        internal string AssetId { get; private set; }
        internal EquipmentEntity Resource { get { return _resource; } }
        internal bool UsedFallback { get; private set; }

        // Recovery replaces the backing object in place so every holder of this
        // registration (set inventory, blueprint construction records) observes
        // the healed instance under the same stable GUID.
        internal void RebindResource(EquipmentEntity replacement)
        {
            if (replacement == null)
                throw new ArgumentNullException("replacement");
            _resource = replacement;
        }
    }

    /// <summary>
    /// Registers project-owned EquipmentEntity proxies in Kingmaker's existing
    /// resource cache. The cache and LoadedResource shape are validated exactly;
    /// collisions are refused and owned additions can be removed as one batch.
    /// </summary>
    internal sealed class ElementalRaceVisualResourceRegistry
    {
        private const string CacheFieldName = "s_LoadedResources";
        private const string ResourceFieldName = "Resource";

        private readonly BlueprintManifest _manifest;
        private readonly ModLogger _logger;
        private readonly FieldInfo _cacheField;
        private readonly FieldInfo _resourceField;
        private readonly ConstructorInfo _loadedResourceConstructor;
        private readonly Dictionary<string,
            ElementalRaceVisualResourceRegistration> _bySymbol;
        private readonly List<ElementalRaceVisualResourceRegistration> _order;
        private readonly Dictionary<string, EquipmentEntity> _nativeDependencies =
            new Dictionary<string, EquipmentEntity>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _nativeDependencyNames =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private UnityEngine.GameObject _anchor;

        internal string[] NativeDependencyIds { get { return _nativeDependencies.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray(); } }

        internal void BindNativeDependencies(IEnumerable<KeyValuePair<string, EquipmentEntity>> dependencies)
        {
            if (_order.Count != 0 || _nativeDependencies.Count != 0)
                throw new InvalidOperationException("Visual donors must be bound once before proxy registration.");
            IDictionary cache = RequireCache();
            var plan = new Dictionary<string, EquipmentEntity>(StringComparer.Ordinal);
            foreach (var dependency in dependencies)
            {
                EquipmentEntity existing;
                if (dependency.Value == null || !cache.Contains(dependency.Key) ||
                    !ReferenceEquals(CurrentResource(cache[dependency.Key]), dependency.Value) ||
                    (plan.TryGetValue(dependency.Key, out existing) && !ReferenceEquals(existing, dependency.Value)))
                    throw new InvalidOperationException("Native visual donor identity is missing or ambiguous: " + dependency.Key);
                plan[dependency.Key] = dependency.Value;
            }
            if (plan.Count == 0) throw new InvalidOperationException("Native visual donor plan is empty.");
            foreach (var dependency in plan)
            {
                _nativeDependencies.Add(dependency.Key, dependency.Value);
                // Provenance record: a legitimately reloaded donor instance must
                // keep this exact object name before recovery may rebind it.
                _nativeDependencyNames.Add(dependency.Key, dependency.Value.name);
            }
        }

        internal ElementalRaceVisualResourceRegistry(BlueprintManifest manifest,
            ModLogger logger)
        {
            _manifest = manifest ?? throw new ArgumentNullException("manifest");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _bySymbol = new Dictionary<string,
                ElementalRaceVisualResourceRegistration>(StringComparer.Ordinal);
            _order = new List<ElementalRaceVisualResourceRegistration>();

            _cacheField = typeof(ResourcesLibrary).GetField(CacheFieldName,
                BindingFlags.Static | BindingFlags.NonPublic);
            if (_cacheField == null ||
                !typeof(IDictionary).IsAssignableFrom(_cacheField.FieldType) ||
                !_cacheField.FieldType.IsGenericType)
                throw new MissingFieldException(typeof(ResourcesLibrary).FullName,
                    CacheFieldName);
            Type[] cacheArguments = _cacheField.FieldType.GetGenericArguments();
            if (cacheArguments.Length != 2 || cacheArguments[0] != typeof(string))
                throw new InvalidOperationException(
                    "Kingmaker's loaded-resource cache key contract changed.");
            Type loadedResourceType = cacheArguments[1];
            _resourceField = loadedResourceType.GetField(ResourceFieldName,
                BindingFlags.Instance | BindingFlags.Public);
            _loadedResourceConstructor = loadedResourceType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null, new[] { typeof(UnityEngine.Object) }, null);
            if (_resourceField == null ||
                _resourceField.FieldType != typeof(UnityEngine.Object) ||
                _loadedResourceConstructor == null)
                throw new InvalidOperationException(
                    "Kingmaker's LoadedResource contract changed.");
        }

        internal int RegisteredCount { get { return _order.Count; } }

        internal ModLogger Logger { get { return _logger; } }

        internal IReadOnlyList<ElementalRaceVisualResourceRegistration>
            Registrations
        {
            get
            {
                return (ElementalRaceVisualResourceRegistration[])_order
                    .ToArray().Clone();
            }
        }

        internal void EnsureAvailable(
            IEnumerable<ElementalRaceVisualProxySpec> specs)
        {
            ElementalRaceVisualProxySpec[] values = specs == null ? null :
                specs.ToArray();
            if (values == null || values.Length == 0 ||
                values.Any(value => value == null))
                throw new ArgumentException(
                    "At least one visual proxy specification is required.",
                    "specs");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ElementalRaceVisualProxySpec spec in values)
            {
                BlueprintManifestEntry entry = _manifest.ResolveActive(
                    spec.Symbol, typeof(EquipmentEntity));
                if (!ids.Add(entry.Id.Value))
                    throw new InvalidOperationException(
                        "Elemental visual resource GUIDs must be unique.");
                ProbeUnoccupied(entry, spec);
            }
        }

        internal ElementalRaceVisualResourceRegistration Register(
            ElementalRaceVisualProxySpec spec, EquipmentEntity resource,
            bool usedFallback)
        {
            if (spec == null) throw new ArgumentNullException("spec");
            if (resource == null) throw new ArgumentNullException("resource");
            BlueprintManifestEntry entry = _manifest.ResolveActive(spec.Symbol,
                typeof(EquipmentEntity));
            if (_bySymbol.ContainsKey(spec.Symbol))
                throw new InvalidOperationException(
                    "Visual resource symbol was already registered: " +
                    spec.Symbol);
            IDictionary cache = RequireCache();
            if (cache.Contains(entry.Id.Value))
                throw Collision(entry, CurrentResource(cache[entry.Id.Value]));

            object loaded = _loadedResourceConstructor.Invoke(
                new object[] { resource });
            bool added = false;
            try
            {
                cache.Add(entry.Id.Value, loaded);
                added = true;
                EquipmentEntity resolved = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(entry.Id.Value, true);
                if (!ReferenceEquals(resolved, resource))
                    throw new InvalidOperationException(
                        "Visual resource verification failed for " +
                        spec.Symbol + ".");
                var registration = new
                    ElementalRaceVisualResourceRegistration(spec,
                        entry.Id.Value, resource, usedFallback);
                _bySymbol.Add(spec.Symbol, registration);
                _order.Add(registration);
                _logger.Info("elemental-races", "visual-resource.registered",
                    string.Format(CultureInfo.InvariantCulture,
                        "Registered visual proxy symbol={0}; guid={1}; donor={2}; fallback={3}.",
                        spec.Symbol, entry.Id.Value, resource.name, usedFallback));
                return registration;
            }
            catch
            {
                if (added && cache.Contains(entry.Id.Value) &&
                    ReferenceEquals(CurrentResource(cache[entry.Id.Value]),
                        resource))
                    cache.Remove(entry.Id.Value);
                throw;
            }
        }

        internal ElementalRaceVisualResourceRegistration Require(string symbol)
        {
            ElementalRaceVisualResourceRegistration value;
            if (string.IsNullOrWhiteSpace(symbol) ||
                !_bySymbol.TryGetValue(symbol, out value))
                throw new InvalidOperationException(
                    "Visual resource has not been registered: " + symbol);
            return value;
        }

        internal void RetainCharacterCreatorResources(ISet<string> ids, IList<UnityEngine.Object> assets)
        {
            IDictionary cache = RequireCache();
            var plan = new List<KeyValuePair<string, UnityEngine.Object[]>>();
            foreach (var registration in _order)
            {
                if (registration.Resource == null || !cache.Contains(registration.AssetId) ||
                    !ReferenceEquals(CurrentResource(cache[registration.AssetId]), registration.Resource))
                    throw new InvalidOperationException("Owned character creator visual resource was lost: " + registration.AssetId);
                UnityEngine.Object[] inner = ProtectedInnerAssets(registration.Resource);
                if (inner.Any(value => value == null))
                    throw new InvalidOperationException("Owned character creator inner asset was destroyed: " + registration.AssetId);
                plan.Add(new KeyValuePair<string, UnityEngine.Object[]>(registration.AssetId, inner));
            }
            // LoadedResource.Unload uses AssetBundle.Unload(true), which destroys
            // shared donor materials even when inner-asset exceptions retain them.
            // Keep the exact construction/palette donors in the native ID set too.
            foreach (var dependency in _nativeDependencies)
            {
                if (dependency.Value == null || !cache.Contains(dependency.Key) ||
                    !ReferenceEquals(CurrentResource(cache[dependency.Key]), dependency.Value))
                    throw new InvalidOperationException("Native visual donor was unloaded: " + dependency.Key);
                UnityEngine.Object[] inner = ProtectedInnerAssets(dependency.Value);
                if (inner.Any(value => value == null))
                    throw new InvalidOperationException("Native visual dependency inner asset was destroyed: " + dependency.Key);
                plan.Add(new KeyValuePair<string, UnityEngine.Object[]>(dependency.Key, inner));
            }
            int additions = ElementalVisualResourceRetentionPolicy.Append(ids, assets, plan);
            if (additions != 0)
                _logger.Info("elemental-races", "character-creator.visual-retained",
                    "Extended native initial retention with exact owned proxies and shared inner assets; additions=" + additions + ".");
        }

        /// <summary>
        /// Read-only damage classification over the exact registered proxies and
        /// bound native dependencies. Unity-destroyed references are reported
        /// separately from cache eviction and cache replacement.
        /// </summary>
        internal List<ElementalVisualResourceDamage> AssessDamage()
        {
            var result = new List<ElementalVisualResourceDamage>();
            if (_order.Count == 0) return result;
            IDictionary cache = RequireCache();
            foreach (var registration in _order)
            {
                string kind = ElementalVisualResourceRecoveryPolicy
                    .ClassifyOwnedResource(new ElementalVisualResourceStateSnapshot
                    {
                        AssetId = registration.AssetId,
                        Symbol = registration.Spec.Symbol,
                        ObjectAlive = registration.Resource != null,
                        CacheContains = cache.Contains(registration.AssetId),
                        CacheReferencesRegistered = cache.Contains(registration.AssetId) &&
                            ReferenceEquals(CurrentResource(cache[registration.AssetId]),
                                registration.Resource),
                        InnerAssetsIntact = registration.Resource != null &&
                            !HasDestroyedInnerAsset(registration.Resource)
                    });
                if (kind != null)
                    result.Add(new ElementalVisualResourceDamage(
                        registration.AssetId, registration.Spec.Symbol, kind));
            }
            foreach (var dependency in _nativeDependencies)
            {
                string kind = ElementalVisualResourceRecoveryPolicy
                    .ClassifyNativeDependency(new ElementalVisualResourceStateSnapshot
                    {
                        AssetId = dependency.Key,
                        Symbol = dependency.Key,
                        ObjectAlive = dependency.Value != null,
                        CacheContains = cache.Contains(dependency.Key),
                        CacheReferencesRegistered = cache.Contains(dependency.Key) &&
                            ReferenceEquals(CurrentResource(cache[dependency.Key]),
                                dependency.Value),
                        InnerAssetsIntact = dependency.Value != null &&
                            !HasDestroyedInnerAsset(dependency.Value)
                    });
                if (kind != null)
                    result.Add(new ElementalVisualResourceDamage(
                        dependency.Key, dependency.Key, kind));
            }
            return result;
        }

        private static bool HasDestroyedInnerAsset(EquipmentEntity entity)
        {
            return entity.GetInnerAssets().Any(value => value == null);
        }

        /// <summary>
        /// Replaces the cache entry of an owned proxy with a reconstructed
        /// instance under the same GUID. The stale entry may only be our own
        /// dead resource or absent; a foreign object under our GUID is never
        /// displaced.
        /// </summary>
        internal void ReplaceOwnedRegistration(
            ElementalRaceVisualResourceRegistration registration,
            EquipmentEntity replacement)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (replacement == null) throw new ArgumentNullException("replacement");
            IDictionary cache = RequireCache();
            if (!cache.Contains(registration.AssetId))
                throw new InvalidOperationException(
                    "Owned visual resource was evicted: " + registration.AssetId);
            UnityEngine.Object current = CurrentResource(cache[registration.AssetId]);
            if (current != null && !ReferenceEquals(current, registration.Resource))
                throw new InvalidOperationException(
                    "A foreign resource occupies the owned visual GUID: " + registration.AssetId);
            object loaded = _loadedResourceConstructor.Invoke(
                new object[] { replacement });
            cache[registration.AssetId] = loaded;
            EquipmentEntity resolved = ResourcesLibrary.TryGetResource<
                EquipmentEntity>(registration.AssetId, true);
            if (!ReferenceEquals(resolved, replacement))
                throw new InvalidOperationException(
                    "Reconstructed visual resource verification failed for " +
                    registration.Spec.Symbol + ".");
            registration.RebindResource(replacement);
            RefreshNativeAnchor();
        }

        /// <summary>
        /// Rebinds a native dependency that legitimately reloaded as a new
        /// instance. Identity is validated by the recorded original object
        /// name and native liveness before the new reference is accepted.
        /// </summary>
        internal bool TryRebindNativeDependency(string assetId,
            EquipmentEntity fresh)
        {
            if (string.IsNullOrWhiteSpace(assetId) || fresh == null) return false;
            string expected;
            if (!_nativeDependencyNames.TryGetValue(assetId, out expected)) return false;
            if (!string.Equals(fresh.name, expected, StringComparison.Ordinal)) return false;
            _nativeDependencies[assetId] = fresh;
            RefreshNativeAnchor();
            return true;
        }

        internal EquipmentEntity CurrentNativeDependency(string assetId)
        {
            EquipmentEntity value;
            return _nativeDependencies.TryGetValue(assetId, out value) ? value : null;
        }

        /// <summary>
        /// Marks exactly the registered proxies and bound native dependencies
        /// as used since the last native cache cleanup, so the counter-based
        /// cleanup pass cannot destroy untouched entries. Nothing outside the
        /// owned identity set is retained.
        /// </summary>
        internal int ArmRetentionCounters()
        {
            if (_order.Count == 0) return 0;
            IDictionary cache = RequireCache();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var registration in _order) ids.Add(registration.AssetId);
            foreach (string id in _nativeDependencies.Keys) ids.Add(id);
            int armed = 0;
            foreach (string id in ids)
            {
                if (!cache.Contains(id)) continue;
                object loaded = cache[id];
                if (loaded == null || RequestCounterField == null) continue;
                RequestCounterField.SetValue(loaded, 1);
                armed++;
            }
            return armed;
        }

        private static readonly FieldInfo RequestCounterField =
            typeof(ResourcesLibrary).GetNestedType("LoadedResource",
                BindingFlags.NonPublic | BindingFlags.Public).GetField(
                    "RequestCounter", BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// The native doll-removal exception set covers GetInnerAssets() of the
        /// loaded entities plus the initially-retained list, but EquipmentEntity
        /// ramp textures (skin/horn palettes) are referenced outside that list;
        /// runtime evidence shows the removal pass destroyed exactly those. The
        /// retention plan therefore extends with the ramps as well.
        /// </summary>
        private static UnityEngine.Object[] ProtectedInnerAssets(EquipmentEntity entity)
        {
            var seen = new HashSet<UnityEngine.Object>(ReferenceIdentity);
            var result = new List<UnityEngine.Object>();
            foreach (var asset in entity.GetInnerAssets())
            {
                if (ReferenceEquals(asset, null)) continue;
                if (seen.Add(asset)) result.Add(asset);
            }
            foreach (var ramp in PrimaryAndSecondaryRamps(entity))
            {
                if (ramp == null) continue;
                if (seen.Add(ramp)) result.Add(ramp);
            }
            return result.ToArray();
        }

        private static IEnumerable<UnityEngine.Texture2D> PrimaryAndSecondaryRamps(EquipmentEntity entity)
        {
            if (entity.PrimaryRamps != null)
                foreach (var ramp in entity.PrimaryRamps) yield return ramp;
            if (entity.SecondaryRamps != null)
                foreach (var ramp in entity.SecondaryRamps) yield return ramp;
        }

        private sealed class ReferenceIdentityComparer : IEqualityComparer<UnityEngine.Object>
        {
            internal static readonly ReferenceIdentityComparer Instance =
                new ReferenceIdentityComparer();
            bool IEqualityComparer<UnityEngine.Object>.Equals(UnityEngine.Object x, UnityEngine.Object y)
            { return ReferenceEquals(x, y); }
            int IEqualityComparer<UnityEngine.Object>.GetHashCode(UnityEngine.Object obj)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }

        private static readonly ReferenceIdentityComparer ReferenceIdentity =
            ReferenceIdentityComparer.Instance;

        /// <summary>True when the id is one of the bound native donors/palette sources.</summary>
        internal bool IsProtectedNativeDependency(string assetId)
        {
            return !string.IsNullOrWhiteSpace(assetId) &&
                _nativeDependencies.ContainsKey(assetId);
        }

        /// <summary>
        /// Evicts a damaged native dependency's cache entry (destroyed corpse or
        /// live-but-gutted registered instance) so the native loader supplies a
        /// fresh instance on the next resolve. A foreign replacement is never
        /// displaced.
        /// </summary>
        internal bool EvictReloadableNativeDependency(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId)) return false;
            EquipmentEntity registered;
            if (!_nativeDependencies.TryGetValue(assetId, out registered)) return false;
            IDictionary cache = RequireCache();
            if (!cache.Contains(assetId)) return false;
            UnityEngine.Object current = CurrentResource(cache[assetId]);
            if (current != null && !ReferenceEquals(current, registered))
                return false;
            string kind = ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                new ElementalVisualResourceStateSnapshot
                {
                    AssetId = assetId,
                    Symbol = assetId,
                    ObjectAlive = registered != null,
                    CacheContains = true,
                    CacheReferencesRegistered = ReferenceEquals(current, registered),
                    InnerAssetsIntact = registered != null &&
                        !HasDestroyedInnerAsset(registered)
                });
            if (!ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(kind))
                return false;
            cache.Remove(assetId);
            return true;
        }

        /// <summary>
        /// Creates (or refreshes) the hidden native holder that keeps exactly the
        /// registered proxies and bound donors reachable for Unity's unused-asset
        /// sweep, and marks their request counters so the counter-based cache
        /// cleanup cannot evict them. Nothing outside the owned identity set is
        /// retained.
        /// </summary>
        internal void AttachNativeAnchor()
        {
            ArmRetentionCounters();
            if (_anchor == null)
            {
                var holder = new UnityEngine.GameObject(
                    "KMG.ElementalRaces.VisualAnchor");
                UnityEngine.Object.DontDestroyOnLoad(holder);
                holder.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
                _anchor = holder;
                _anchor.AddComponent<ElementalVisualResourceAnchor>();
            }
            RefreshNativeAnchor();
        }

        internal void RefreshNativeAnchor()
        {
            if (_anchor == null) return;
            var anchor = _anchor.GetComponent<ElementalVisualResourceAnchor>();
            if (anchor == null) return;
            anchor.Proxies = _order
                .Select(registration => registration.Resource).ToArray();
            anchor.Donors = _nativeDependencies.Values.ToArray();
        }

        /// <summary>Diagnostic description of the native anchor state.</summary>
        internal string DescribeNativeAnchor()
        {
            if (_anchor == null) return "absent";
            var anchor = _anchor.GetComponent<ElementalVisualResourceAnchor>();
            if (anchor == null) return "holder-without-component";
            int liveProxies = anchor.Proxies == null ? 0 :
                anchor.Proxies.Count(value => value != null);
            int liveDonors = anchor.Donors == null ? 0 :
                anchor.Donors.Count(value => value != null);
            return string.Format(CultureInfo.InvariantCulture,
                "alive;proxies={0}/{1};donors={2}/{3}", liveProxies,
                anchor.Proxies == null ? 0 : anchor.Proxies.Length,
                liveDonors, anchor.Donors == null ? 0 : anchor.Donors.Length);
        }

        internal void RollbackAll()
        {
            if (_order.Count == 0) return;
            IDictionary cache = RequireCache();
            ElementalRaceVisualResourceRegistration[] removalPlan =
                ElementalVisualResourceRollbackPolicy.CreateRemovalPlan(
                    _order,
                    registration => cache.Contains(registration.AssetId),
                    registration => ReferenceEquals(CurrentResource(
                        cache[registration.AssetId]), registration.Resource),
                    registration => registration.AssetId);
            foreach (ElementalRaceVisualResourceRegistration registration in
                removalPlan)
            {
                cache.Remove(registration.AssetId);
            }
            int removed = _order.Count;
            _order.Clear();
            _bySymbol.Clear();
            if (_anchor != null)
            {
                UnityEngine.Object.Destroy(_anchor);
                _anchor = null;
            }
            _logger.Warning("elemental-races", "visual-resource.rollback",
                string.Format(CultureInfo.InvariantCulture,
                    "Rolled back {0} owned visual resource proxies.", removed));
        }

        private void ProbeUnoccupied(BlueprintManifestEntry entry,
            ElementalRaceVisualProxySpec spec)
        {
            IDictionary cache = RequireCache();
            if (cache.Contains(entry.Id.Value))
                throw Collision(entry, CurrentResource(cache[entry.Id.Value]));
            EquipmentEntity resolved;
            try
            {
                resolved = ResourcesLibrary.TryGetResource<EquipmentEntity>(
                    entry.Id.Value, false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Unable to scan visual resource identity " + spec.Symbol +
                    " for an installed collision.", exception);
            }
            if (resolved != null)
                throw Collision(entry, resolved);
            if (cache.Contains(entry.Id.Value))
            {
                object placeholder = cache[entry.Id.Value];
                UnityEngine.Object current = CurrentResource(placeholder);
                if (current != null) throw Collision(entry, current);
                cache.Remove(entry.Id.Value);
            }
        }

        private IDictionary RequireCache()
        {
            IDictionary value = _cacheField.GetValue(null) as IDictionary;
            if (value == null)
                throw new InvalidOperationException(
                    "Kingmaker's loaded-resource cache is unavailable.");
            return value;
        }

        private UnityEngine.Object CurrentResource(object loaded)
        {
            return loaded == null ? null :
                _resourceField.GetValue(loaded) as UnityEngine.Object;
        }

        private static Exception Collision(BlueprintManifestEntry entry,
            UnityEngine.Object existing)
        {
            return new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                "Visual resource GUID collision: symbol='{0}', guid='{1}', existingName='{2}', existingType='{3}'.",
                entry.Symbol, entry.Id.Value,
                existing == null ? "<null>" : existing.name,
                existing == null ? "<null>" : existing.GetType().Name));
        }
    }
}
