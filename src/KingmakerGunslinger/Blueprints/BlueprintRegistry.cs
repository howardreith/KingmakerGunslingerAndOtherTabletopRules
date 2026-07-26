using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Fail-closed adapter for registering manifest-backed blueprints in Kingmaker's
    /// library. It never generates IDs and never replaces an existing dictionary entry.
    /// Registrations can be rolled back as a batch when a later blueprint in the same
    /// initialization transaction fails.
    /// </summary>
    internal sealed class BlueprintRegistry
    {
        private const string AssetGuidFieldName = "m_AssetGuid";

        private readonly LibraryScriptableObject _library;
        private readonly BlueprintManifest _manifest;
        private readonly ModLogger _logger;
        private readonly FieldInfo _assetGuidField;
        private readonly Dictionary<string, BlueprintScriptableObject> _registeredBySymbol;
        private readonly List<RegistrationRecord> _registrationOrder;

        internal BlueprintRegistry(
            LibraryScriptableObject library,
            BlueprintManifest manifest,
            ModLogger logger)
        {
            _library = library ?? throw new ArgumentNullException("library");
            _manifest = manifest ?? throw new ArgumentNullException("manifest");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _registeredBySymbol = new Dictionary<string, BlueprintScriptableObject>(StringComparer.Ordinal);
            _registrationOrder = new List<RegistrationRecord>();

            _assetGuidField = typeof(BlueprintScriptableObject).GetField(
                AssetGuidFieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (_assetGuidField == null || _assetGuidField.FieldType != typeof(string))
            {
                throw new MissingFieldException(
                    typeof(BlueprintScriptableObject).FullName,
                    AssetGuidFieldName);
            }
        }

        internal int RegisteredCount
        {
            get { return _registrationOrder.Count; }
        }

        internal string ResolveGuid(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("A registered blueprint symbol is required.", "symbol");
            }

            for (int index = 0; index < _registrationOrder.Count; index++)
            {
                RegistrationRecord record = _registrationOrder[index];
                if (string.Equals(record.Entry.Symbol, symbol, StringComparison.Ordinal))
                {
                    return record.Entry.Id.Value;
                }
            }

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Blueprint symbol '{0}' has not been registered by this registry.",
                    symbol));
        }

        internal T Register<T>(string symbol, Func<T> factory)
            where T : BlueprintScriptableObject
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            BlueprintManifestEntry entry = _manifest.ResolveActive(symbol, typeof(T));
            if (_registeredBySymbol.ContainsKey(entry.Symbol))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' was already registered by this registry.",
                        entry.Symbol));
            }

            if (_library.BlueprintsByAssetId == null)
            {
                throw new InvalidOperationException("Kingmaker's blueprint dictionary is unavailable.");
            }

            var allBlueprints = _library.GetAllBlueprints();
            if (allBlueprints == null)
            {
                throw new InvalidOperationException("Kingmaker's all-blueprints collection is unavailable.");
            }

            BlueprintScriptableObject existing;
            if (_library.BlueprintsByAssetId.TryGetValue(entry.Id.Value, out existing))
            {
                throw CreateCollisionException(entry, existing);
            }

            // The factory is deliberately invoked only after manifest and library collision
            // validation, so a rejected registration creates no Unity object.
            T blueprint = factory();
            if (blueprint == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint factory for symbol '{0}' returned null.",
                        entry.Symbol));
            }

            bool addedToAllBlueprints = false;
            bool addedToDictionary = false;

            try
            {
                if (string.IsNullOrWhiteSpace(blueprint.name))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Blueprint factory for symbol '{0}' produced an unnamed asset.",
                            entry.Symbol));
                }

                _assetGuidField.SetValue(blueprint, entry.Id.Value);
                string assignedGuid = _assetGuidField.GetValue(blueprint) as string;
                if (!string.Equals(assignedGuid, entry.Id.Value, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Blueprint GUID assignment failed for symbol '{0}'.",
                            entry.Symbol));
                }

                // Recheck immediately before mutation in case another mod registered the
                // same ID after the first collision check.
                if (_library.BlueprintsByAssetId.TryGetValue(entry.Id.Value, out existing))
                {
                    throw CreateCollisionException(entry, existing);
                }

                allBlueprints.Add(blueprint);
                addedToAllBlueprints = true;

                // Dictionary.Add is intentional: an unexpected duplicate throws instead of
                // silently replacing another mod's or the game's asset.
                _library.BlueprintsByAssetId.Add(entry.Id.Value, blueprint);
                addedToDictionary = true;

                BlueprintScriptableObject resolved;
                if (!_library.BlueprintsByAssetId.TryGetValue(entry.Id.Value, out resolved) ||
                    !ReferenceEquals(resolved, blueprint))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Blueprint verification failed for symbol '{0}' and GUID '{1}'.",
                            entry.Symbol,
                            entry.Id.Value));
                }

                _registeredBySymbol.Add(entry.Symbol, blueprint);
                _registrationOrder.Add(new RegistrationRecord(entry, blueprint));
                _logger.Info(
                    "blueprints",
                    "registry.registered",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Registered blueprint symbol={0}; guid={1}.",
                        entry.Symbol,
                        entry.Id.Value));
                return blueprint;
            }
            catch
            {
                RemoveFromLibrary(entry.Id.Value, blueprint, allBlueprints, addedToDictionary, addedToAllBlueprints);
                throw;
            }
        }

        internal void RollbackAll()
        {
            if (_registrationOrder.Count == 0)
            {
                return;
            }

            var allBlueprints = _library.GetAllBlueprints();
            if (allBlueprints == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker's all-blueprints collection became unavailable during rollback.");
            }

            int removed = 0;
            for (int index = _registrationOrder.Count - 1; index >= 0; index--)
            {
                RegistrationRecord record = _registrationOrder[index];
                bool dictionaryOwned = false;
                BlueprintScriptableObject current;
                if (_library.BlueprintsByAssetId != null &&
                    _library.BlueprintsByAssetId.TryGetValue(record.Entry.Id.Value, out current) &&
                    ReferenceEquals(current, record.Blueprint))
                {
                    dictionaryOwned = _library.BlueprintsByAssetId.Remove(record.Entry.Id.Value);
                }

                bool listOwned = allBlueprints.Remove(record.Blueprint);
                if (dictionaryOwned || listOwned)
                {
                    removed++;
                }

                _registeredBySymbol.Remove(record.Entry.Symbol);
            }

            _registrationOrder.Clear();
            _logger.Warning(
                "blueprints",
                "registry.rollback",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Rolled back {0} custom blueprint registration(s) after initialization failure.",
                    removed));
        }

        private void RemoveFromLibrary(
            string guid,
            BlueprintScriptableObject blueprint,
            ICollection<BlueprintScriptableObject> allBlueprints,
            bool addedToDictionary,
            bool addedToAllBlueprints)
        {
            if (addedToDictionary && _library.BlueprintsByAssetId != null)
            {
                BlueprintScriptableObject current;
                if (_library.BlueprintsByAssetId.TryGetValue(guid, out current) &&
                    ReferenceEquals(current, blueprint))
                {
                    _library.BlueprintsByAssetId.Remove(guid);
                }
            }

            if (addedToAllBlueprints)
            {
                allBlueprints.Remove(blueprint);
            }
        }

        private static Exception CreateCollisionException(
            BlueprintManifestEntry entry,
            BlueprintScriptableObject existing)
        {
            string existingName = existing == null ? "<null>" : existing.name;
            string existingType = existing == null ? "<null>" : existing.GetType().Name;
            return new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Blueprint GUID collision: symbol='{0}', guid='{1}', existingName='{2}', existingType='{3}'.",
                    entry.Symbol,
                    entry.Id.Value,
                    existingName,
                    existingType));
        }

        private sealed class RegistrationRecord
        {
            internal RegistrationRecord(
                BlueprintManifestEntry entry,
                BlueprintScriptableObject blueprint)
            {
                Entry = entry ?? throw new ArgumentNullException("entry");
                Blueprint = blueprint ?? throw new ArgumentNullException("blueprint");
            }

            internal BlueprintManifestEntry Entry { get; private set; }

            internal BlueprintScriptableObject Blueprint { get; private set; }
        }
    }
}
