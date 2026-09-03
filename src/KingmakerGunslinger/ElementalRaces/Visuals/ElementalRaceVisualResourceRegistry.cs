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
        internal ElementalRaceVisualResourceRegistration(
            ElementalRaceVisualProxySpec spec, string assetId,
            EquipmentEntity resource, bool usedFallback)
        {
            Spec = spec ?? throw new ArgumentNullException("spec");
            AssetId = assetId ?? throw new ArgumentNullException("assetId");
            Resource = resource ?? throw new ArgumentNullException("resource");
            UsedFallback = usedFallback;
        }

        internal ElementalRaceVisualProxySpec Spec { get; private set; }
        internal string AssetId { get; private set; }
        internal EquipmentEntity Resource { get; private set; }
        internal bool UsedFallback { get; private set; }
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
