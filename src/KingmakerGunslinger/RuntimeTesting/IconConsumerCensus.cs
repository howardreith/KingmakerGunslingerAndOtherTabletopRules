using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Read-only request-scoped census. Observe exact library objects and direct
    // graph references; never traverse into or repaint a referenced blueprint.
    internal static class IconConsumerCensus
    {
        private sealed class Before
        {
            internal BlueprintScriptableObject Blueprint;
            internal Sprite Icon;
            internal JObject IconRecord;
            internal string Graph;
            internal string Components;
            internal BlueprintComponent[] ComponentReferences;
            internal JArray ComponentRecords;
            internal string Symbol;
        }

        private sealed class BeforeResource
        {
            internal string Symbol;
            internal string Guid;
            internal EquipmentEntity Resource;
            internal JObject Record;
        }

        private static string _runId;
        private static string _expectedVersion;
        internal static bool IsControlRequest { get; private set; }
        private static string _captureFailure;
        private static List<Before> _before;
        private static List<BeforeResource> _resources;
        private static JObject _immediate;
        private static LibraryScriptableObject _library;
        private static BlueprintManifest _manifest;

        private static readonly string[] CoveragePrefixes = {
            "KMG.ElementalRaces.", "KMG.Spells.Teleport.",
            "KMG.Spells.GreaterTeleport.", "KMG.Spells.WordOfRecall."
        };
        private static readonly string[] AdditionalSymbols = {
            "KMG.Feats.FirearmWeaponFocus",
            "KMG.Feats.NativeWeaponFocusWithFirearms",
            "KMG.Feats.WeaponFocusPistol",
            "KMG.Feats.WeaponFocusMusket",
            "KMG.Feats.WeaponFocusBlunderbuss",
            "KMG.Feats.WeaponFocusRifle",
            "KMG.Feats.WeaponFocusRevolver",
            "KMG.Feats.RapidReload",
            "KMG.Feats.RapidReloadPistol",
            "KMG.Feats.RapidReloadMusket",
            "KMG.Feats.RapidReloadBlunderbuss",
            "KMG.Feats.RapidReloadRifle",
            "KMG.Feats.RapidReloadRevolver",
            "KMG.Feats.GreaterWeaponFocusPistol",
            "KMG.Feats.GreaterWeaponFocusMusket",
            "KMG.Feats.GreaterWeaponFocusBlunderbuss",
            "KMG.Feats.GreaterWeaponFocusRifle",
            "KMG.Feats.GreaterWeaponFocusRevolver",
            "KMG.Feats.GreaterWeaponFocusWithFirearms",
            "KMG.Feats.WeaponSpecializationPistol",
            "KMG.Feats.WeaponSpecializationMusket",
            "KMG.Feats.WeaponSpecializationBlunderbuss",
            "KMG.Feats.WeaponSpecializationRifle",
            "KMG.Feats.WeaponSpecializationRevolver",
            "KMG.Feats.WeaponSpecializationWithFirearms",
            "KMG.Feats.GreaterWeaponSpecializationPistol",
            "KMG.Feats.GreaterWeaponSpecializationMusket",
            "KMG.Feats.GreaterWeaponSpecializationBlunderbuss",
            "KMG.Feats.GreaterWeaponSpecializationRifle",
            "KMG.Feats.GreaterWeaponSpecializationRevolver",
            "KMG.Feats.GreaterWeaponSpecializationWithFirearms",
            "KMG.Feats.ImprovedCriticalPistol",
            "KMG.Feats.ImprovedCriticalMusket",
            "KMG.Feats.ImprovedCriticalBlunderbuss",
            "KMG.Feats.ImprovedCriticalRifle",
            "KMG.Feats.ImprovedCriticalRevolver",
            "KMG.Feats.ImprovedCriticalWithFirearms",
        };
        private static readonly string[][] NativeReuse = {
            new[] { "KMG.ElementalRaces.Ifrit.BurningHandsFeature", "4783c3709a74a794dbe7c8e7e0b1b038" },
            new[] { "KMG.ElementalRaces.Ifrit.BurningHandsAbility", "4783c3709a74a794dbe7c8e7e0b1b038" },
            new[] { "KMG.ElementalRaces.Oread.StoneFistFeature", "85067a04a97416949b5d1dbf986d93f3" },
            new[] { "KMG.ElementalRaces.Oread.StoneFistAbility", "85067a04a97416949b5d1dbf986d93f3" },
            new[] { "KMG.ElementalRaces.Sylph.FeatherStepFeature", "f3c0b267dd17a2a45a40805e31fe3cd1" },
            new[] { "KMG.ElementalRaces.Sylph.FeatherStepAbility", "f3c0b267dd17a2a45a40805e31fe3cd1" },
            new[] { "KMG.ElementalRaces.Ifrit.Lavasoul.FirebellyFeature", "b065231094a21d14dbf1c3832f776871" },
            new[] { "KMG.ElementalRaces.Ifrit.Lavasoul.FirebellyAbility", "b065231094a21d14dbf1c3832f776871" },
            new[] { "KMG.ElementalRaces.Ifrit.Sunsoul.FlareBurstFeature", "39a602aa80cc96f4597778b6d4d49c0a" },
            new[] { "KMG.ElementalRaces.Ifrit.Sunsoul.FlareBurstAbility", "39a602aa80cc96f4597778b6d4d49c0a" },
            new[] { "KMG.ElementalRaces.Oread.Gemsoul.ColorSprayFeature", "91da41b9793a4624797921f221db653c" },
            new[] { "KMG.ElementalRaces.Oread.Gemsoul.ColorSprayAbility", "91da41b9793a4624797921f221db653c" },
            new[] { "KMG.ElementalRaces.Sylph.Smokesoul.ExpeditiousRetreatFeature", "4f8181e7a7f1d904fbaea64220e83379" },
            new[] { "KMG.ElementalRaces.Sylph.Smokesoul.ExpeditiousRetreatAbility", "4f8181e7a7f1d904fbaea64220e83379" },
            new[] { "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspFeature", "ab395d2335d3f384e99dddee8562978f" },
            new[] { "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspAbility", "ab395d2335d3f384e99dddee8562978f" },
            new[] { "KMG.ElementalRaces.Undine.Mistsoul.BlurFeature", "14ec7a4e52e90fa47a4c8d63c69fd5c1" },
            new[] { "KMG.ElementalRaces.Undine.Mistsoul.BlurAbility", "14ec7a4e52e90fa47a4c8d63c69fd5c1" },
            new[] { "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspDeliveryAbility", "17451c1327c571641a1345bd31155209" },
            new[] { "KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic.EnlargePerson", "c60969e7f264e6d4b84a1499fdcf9039" },
            new[] { "KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic.ReducePerson", "4e0e9aba6447d514f88eff1464cc4763" },
        };

        // Called only by the runner after full validation and exclusive claim.
        // Revalidating here would correctly reject that already claimed run ID.
        internal static void Arm(RuntimeTestRequest request)
        {
            if (request == null || request.Scenario != RuntimeTestScenarioCatalog.IconOverhaulVisualEvidence)
                return;
            if (_runId != null) throw new InvalidOperationException("Duplicate icon census request.");
            _runId = request.RunId;
            _expectedVersion = request.ExpectedModVersion;
            IsControlRequest = IconCensusControlPolicy.IsControl(request.Scenario, request.ExitAfterCompletion, request.Parameters);
        }

        internal static void CaptureBeforeOwnedMapping(ModContext context,
            LibraryScriptableObject library, BlueprintManifest manifest)
        {
            if (_runId == null) return;
            try { CaptureBeforeCore(context, library, manifest); }
            catch (Exception error) { _captureFailure = error.ToString(); }
        }

        private static void CaptureBeforeCore(ModContext context,
            LibraryScriptableObject library, BlueprintManifest manifest)
        {
            if (context.ModEntry.Info.Version != _expectedVersion)
                throw new InvalidOperationException("Claimed icon census version changed.");
            if (_before != null) throw new InvalidOperationException("Duplicate icon census initialization.");
            _library = library;
            _manifest = manifest;
            var document = JObject.Parse(File.ReadAllText(manifest.FilePath));
            var extras = new HashSet<string>(AdditionalSymbols, StringComparer.Ordinal);
            var entries = document["entries"].Where(entry => {
                string symbol = (string)entry["symbol"];
                return extras.Contains(symbol) || CoveragePrefixes.Any(prefix =>
                    symbol.StartsWith(prefix, StringComparison.Ordinal));
            }).ToArray();
            // This manifest reservation belongs only to its separate race probe.
            // Its absence is part of this census; never create it for icon testing.
            var reserved = entries.Where(entry => (string)entry["status"] != "active").ToArray();
            if (reserved.Length != 1 ||
                (string)reserved[0]["symbol"] != ElementalRaceDiagnosticIdentityCatalog.Symbol ||
                (string)reserved[0]["guid"] != ElementalRaceDiagnosticIdentityCatalog.Guid ||
                (string)reserved[0]["plannedType"] != nameof(BlueprintRace) ||
                (string)reserved[0]["status"] != "reserved" ||
                library.BlueprintsByAssetId.ContainsKey(ElementalRaceDiagnosticIdentityCatalog.Guid))
                throw new InvalidOperationException("Reserved diagnostic race absence contract changed.");
            var resourceEntries = entries.Where(entry => (string)entry["status"] == "active" &&
                (string)entry["plannedType"] == nameof(EquipmentEntity)).ToArray();
            var inventory = entries.Where(entry => (string)entry["status"] == "active" &&
                (string)entry["plannedType"] != nameof(EquipmentEntity))
                .ToDictionary(entry => (string)entry["guid"], entry => (string)entry["symbol"],
                StringComparer.Ordinal);
            var failures = new List<string>();
            if (inventory.Count != 255 || resourceEntries.Length != 28)
                failures.Add("Expected 255 blueprint and 28 resource-cache identities.");
            foreach (var pair in inventory)
            {
                try
                {
                    BlueprintScriptableObject value;
                    if (!library.BlueprintsByAssetId.TryGetValue(pair.Key, out value) || value == null || value.AssetGuid != pair.Key)
                        throw new InvalidOperationException("blueprint not registered");
                    manifest.ResolveActive(pair.Value, value.GetType());
                }
                catch (Exception error) { failures.Add(pair.Value + ": " + error.Message); }
            }
            _resources = new List<BeforeResource>();
            foreach (var entry in resourceEntries)
            {
                string symbol = (string)entry["symbol"], guid = (string)entry["guid"];
                try
                {
                    manifest.ResolveActive(symbol, typeof(EquipmentEntity));
                    EquipmentEntity resource = ReadVisualResource(guid);
                    if (resource == null || library.BlueprintsByAssetId.ContainsKey(guid))
                        throw new InvalidOperationException("appearance resource cache identity missing or ambiguous");
                    _resources.Add(new BeforeResource { Symbol = symbol, Guid = guid,
                        Resource = resource, Record = DescribeVisualResource(resource) });
                }
                catch (Exception error) { failures.Add(symbol + ": " + error.Message); }
            }
            if (failures.Count != 0)
                throw new InvalidOperationException("Incomplete live icon census: " + string.Join("; ", failures));
            _before = library.BlueprintsByAssetId.Values.Where(value => value != null && !string.IsNullOrEmpty(value.AssetGuid) &&
                (HasIcon(value) || inventory.ContainsKey(value.AssetGuid))).Distinct()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).Select(value => {
                    string symbol;
                    bool owned = inventory.TryGetValue(value.AssetGuid, out symbol);
                    Sprite icon = ReadIcon(value);
                    return new Before {
                        Blueprint = value, Icon = icon, IconRecord = DescribeIcon(icon), Symbol = symbol,
                        Graph = owned ? Graph(value) : null,
                        Components = owned ? Components(value) : null,
                        ComponentReferences = owned ? (value.ComponentsArray ?? new BlueprintComponent[0]).ToArray() : null,
                        ComponentRecords = owned ? DescribeComponents(value.ComponentsArray) : null
                    };
                }).ToList();
        }

        internal static void CaptureAfterOwnedMapping()
        {
            if (_before == null || _captureFailure != null) return;
            try { _immediate = Observe("immediately-after-owned-mapping"); }
            catch (Exception error) { _captureFailure = error.ToString(); }
        }

        internal static void Exercise(ModContext context, RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            if (_captureFailure != null)
                throw new InvalidOperationException("Icon census observation failed; production initialization was preserved. " + _captureFailure);
            if (_before == null || _runId != request.RunId || _immediate == null)
                throw new InvalidOperationException("Exact request-scoped icon census is incomplete: armed=" +
                    (_runId != null) + "; sameRun=" + (_runId == request.RunId) + "; before=" +
                    (_before != null) + "; immediate=" + (_immediate != null));
            JObject late = Observe("after-initialization-and-compatibility");
            var inventory = new JArray(_before.Where(value => value.Symbol != null).Select(value =>
                new JObject {
                    { "symbol", value.Symbol }, { "guid", value.Blueprint.AssetGuid },
                    { "type", value.Blueprint.GetType().Name }, { "name", DisplayName(value.Blueprint) },
                    { "registered", true },
                    { "registrationKind", "blueprint-library" },
                    { "hasIconContract", HasIcon(value.Blueprint) },
                    { "before", value.IconRecord }, { "after", DescribeIcon(ReadIcon(value.Blueprint)) },
                    { "beforeGraph", JArray.Parse(value.Graph) },
                    { "afterGraph", JArray.Parse(Graph(value.Blueprint)) },
                    { "componentIdentitiesUnchanged", value.Components == Components(value.Blueprint) },
                    { "originalComponentReferencesPreserved", OriginalComponentsPreserved(value) },
                    { "beforeComponents", value.ComponentRecords },
                    { "afterComponents", DescribeComponents(value.Blueprint.ComponentsArray) }
                }));
            foreach (var resource in _resources)
            {
                EquipmentEntity current = ReadVisualResource(resource.Guid);
                inventory.Add(new JObject {
                    { "symbol", resource.Symbol }, { "guid", resource.Guid },
                    { "type", nameof(EquipmentEntity) }, { "name", resource.Resource.name },
                    { "registered", true }, { "registrationKind", "resource-cache" },
                    { "hasIconContract", false }, { "before", resource.Record },
                    { "after", DescribeVisualResource(current) },
                    { "sameResourceReference", ReferenceEquals(resource.Resource, current) },
                    { "inBlueprintLibrary", _library.BlueprintsByAssetId.ContainsKey(resource.Guid) }
                });
            }
            inventory.Add(new JObject {
                { "symbol", ElementalRaceDiagnosticIdentityCatalog.Symbol },
                { "guid", ElementalRaceDiagnosticIdentityCatalog.Guid },
                { "type", nameof(BlueprintRace) }, { "name", "Probe Race" },
                { "registered", false }, { "absenceReason", "reserved-diagnostic-not-registered" },
                { "beforeAbsent", (bool)_immediate["reservedAbsent"] },
                { "afterAbsent", (bool)late["reservedAbsent"] }
            });
            var mappedSymbols = new HashSet<string>(OwnedIconAssignments.Bindings.Select(value => value.Symbol), StringComparer.Ordinal);
            var protectedRows = new JArray(_before.Where(value => value.Symbol == null || !mappedSymbols.Contains(value.Symbol)).Select(value =>
                new JObject {
                    { "guid", value.Blueprint.AssetGuid }, { "name", value.Blueprint.name },
                    { "type", value.Blueprint.GetType().Name }, { "before", value.IconRecord },
                    { "after", DescribeIcon(ReadIcon(value.Blueprint)) },
                    { "sameSpriteReference", ReferenceEquals(value.Icon, ReadIcon(value.Blueprint)) }
                }));
            var bySymbol = _before.Where(value => value.Symbol != null)
                .ToDictionary(value => value.Symbol, StringComparer.Ordinal);
            var reuse = new JArray();
            foreach (string[] rule in NativeReuse)
            {
                BlueprintScriptableObject donor;
                Before owned;
                if (!bySymbol.TryGetValue(rule[0], out owned) ||
                    !BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(rule[1], out donor))
                    throw new InvalidOperationException("Native reuse consumer/donor missing: " + rule[0]);
                Sprite expected = ReadIcon(donor);
                reuse.Add(new JObject {
                    { "symbol", rule[0] }, { "donorGuid", rule[1] }, { "donorName", DisplayName(donor) },
                    { "sameSpriteReference", expected != null && ReferenceEquals(ReadIcon(owned.Blueprint), expected) },
                    { "donorIcon", DescribeIcon(expected) }
                });
            }
            bool reuseExact = reuse.Count == 21 && reuse.Count == NativeReuse.Length && reuse.All(row => (bool)row["sameSpriteReference"]);
            var exports = new JArray(OwnedIconAssignments.IconKeys.Select(key => {
                Sprite icon = ProjectAssetIcons.RequireIcon(key);
                string path = Path.Combine(context.ModEntry.Path, "assets", "icons", key + ".png");
                return new JObject {
                    { "key", key }, { "installedPath", "assets/icons/" + key + ".png" },
                    { "installedSha256", Sha256(path) },
                    { "liveIcon", DescribeIcon(icon) }
                };
            }));
            string evidencePath = Path.Combine(request.EvidenceDirectory, "icon-consumer-census.json");
            var evidence = new JObject {
                { "schemaVersion", 2 }, { "runId", request.RunId },
                { "controlRun", IsControlRequest }, { "requiresPairedControlComparison", true },
                { "evidenceClass", "structured live object/assignment evidence; no native UI capture" },
                { "runtimeIdentity", JObject.FromObject(RuntimeBuildIdentity.Capture(context.Assembly, context.ModEntry.Info.Version)) },
                { "profile", JObject.FromObject(context.FeatureModules.Active) },
                { "saveStateTouched", false }, { "inventory", inventory },
                { "immediate", _immediate }, { "late", late },
                { "nativeReuse", reuse }, { "protectedAssignments", protectedRows }, { "exports", exports }
            };
            RuntimeTestResultWriter.WriteAtomic(evidencePath, evidence.ToString(Formatting.Indented));
            files.Add(evidencePath);
            Add(assertions, "icon-census-complete-inventory", inventory.Count == 284 &&
                _before.Count(value => value.Symbol != null) == 255 && _resources.Count == 28,
                "284 catalog identities: 255 blueprints, 28 appearance resources and one reserved diagnostic absence", inventory.Count.ToString(), evidencePath);
            Add(assertions, "icon-reserved-diagnostic-absent", (bool)_immediate["reservedAbsent"] && (bool)late["reservedAbsent"],
                "the request-local race probe stays absent before/after ordinary initialization",
                inventory.Last.ToString(Formatting.None), evidencePath);
            foreach (var observation in new[] { _immediate, late })
            {
                string stage = (string)observation["stage"];
                Add(assertions, "icon-owned-bindings-" + stage,
                    (bool)observation["mappedExact"], IsControlRequest
                        ? "137 control consumers retain their original sprites" : "137 exact consumers use 89 cached 128px paintings",
                    observation["mappedFailures"].ToString(Formatting.None), evidencePath);
                if (ReferenceEquals(observation, _immediate))
                {
                    Add(assertions, "icon-protected-bindings-" + stage,
                        (bool)observation["protectedExact"], "all other observed sprite references are unchanged across icon mapping",
                        observation["protectedFailures"].ToString(Formatting.None), evidencePath);
                    Add(assertions, "icon-owned-graphs-" + stage,
                        (bool)observation["graphsExact"], "all 255 graphs/components are unchanged across icon mapping",
                        observation["graphFailures"].ToString(Formatting.None), evidencePath);
                }
                else
                {
                    Add(assertions, "icon-existing-components-preserved-" + stage,
                        (bool)observation["originalComponentsPreserved"], "all pre-existing component references retain their order",
                        observation["graphFailures"].ToString(Formatting.None), evidencePath);
                    // Late foreign initialization is NOT accepted by this result.
                    // The required offline paired-control gate compares every
                    // protected before/after sprite and owned graph in both runs.
                }
                Add(assertions, "icon-protected-resources-" + stage,
                    (bool)observation["resourcesExact"], "28 appearance resources retain their exact cached object references",
                    observation["resourceFailures"].ToString(Formatting.None), evidencePath);
            }
            Add(assertions, "icon-native-semantic-reuse", reuseExact,
                "21 native-equivalent feature/ability/delivery consumers retain exact donor sprites",
                reuse.ToString(Formatting.None), evidencePath);
        }

        private static JObject Observe(string stage)
        {
            var mappedFailures = new JArray();
            var graphFailures = new JArray();
            var protectedFailures = new JArray();
            var resourceFailures = new JArray();
            var targets = new HashSet<BlueprintScriptableObject>();
            var originalIcons = _before.ToDictionary(value => value.Blueprint, value => value.Icon);
            foreach (var binding in OwnedIconAssignments.Bindings)
            {
                BlueprintScriptableObject value = binding.Resolve(_library, _manifest);
                targets.Add(value);
                Sprite expected = IsControlRequest ? originalIcons[value] : ProjectAssetIcons.RequireIcon(binding.Key);
                if (!ReferenceEquals(ReadIcon(value), expected) || (!IsControlRequest && (expected.texture.width != 128 ||
                    expected.texture.height != 128 || expected.rect.width != 128 || expected.rect.height != 128)))
                    mappedFailures.Add(binding.Symbol);
            }
            foreach (var value in _before)
            {
                if (!targets.Contains(value.Blueprint) && !ReferenceEquals(value.Icon, ReadIcon(value.Blueprint)))
                    protectedFailures.Add(value.Blueprint.AssetGuid + ":" + value.Blueprint.name);
                if (value.Symbol != null && (value.Graph != Graph(value.Blueprint) ||
                    value.Components != Components(value.Blueprint))) graphFailures.Add(value.Symbol);
            }
            foreach (var resource in _resources)
            {
                EquipmentEntity current = ReadVisualResource(resource.Guid);
                if (resource.Resource == null || !ReferenceEquals(resource.Resource, current) ||
                    !JToken.DeepEquals(resource.Record, DescribeVisualResource(current)) ||
                    _library.BlueprintsByAssetId.ContainsKey(resource.Guid)) resourceFailures.Add(resource.Symbol);
            }
            return new JObject {
                { "stage", stage }, { "mappedConsumers", targets.Count },
                { "reservedAbsent", !_library.BlueprintsByAssetId.ContainsKey(ElementalRaceDiagnosticIdentityCatalog.Guid) },
                { "protectedConsumers", _before.Count(value => !targets.Contains(value.Blueprint)) },
                { "mappedExact", targets.Count == 137 && mappedFailures.Count == 0 },
                { "protectedExact", protectedFailures.Count == 0 },
                { "graphsExact", graphFailures.Count == 0 },
                { "originalComponentsPreserved", _before.Where(value => value.Symbol != null).All(OriginalComponentsPreserved) },
                { "resourcesExact", _resources.Count == 28 && resourceFailures.Count == 0 },
                { "mappedFailures", mappedFailures }, { "protectedFailures", protectedFailures },
                { "graphFailures", graphFailures }, { "resourceFailures", resourceFailures }
            };
        }

        // Inspect the existing cache only. Do not call TryGetResource, which can
        // load, replace or update native resource retention counters.
        private static EquipmentEntity ReadVisualResource(string guid)
        {
            var cacheField = typeof(ResourcesLibrary).GetField("s_LoadedResources", BindingFlags.Static | BindingFlags.NonPublic);
            var cache = cacheField == null ? null : cacheField.GetValue(null) as IDictionary;
            if (cache == null) throw new InvalidOperationException("Native visual resource cache is unavailable.");
            if (!cache.Contains(guid)) return null;
            object wrapper = cache[guid];
            var field = wrapper == null ? null : wrapper.GetType().GetField("Resource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) throw new InvalidOperationException("Native loaded-resource wrapper changed.");
            return field.GetValue(wrapper) as EquipmentEntity;
        }

        private static JObject DescribeVisualResource(EquipmentEntity resource) => resource == null
            ? new JObject { { "isNull", true } }
            : new JObject { { "isNull", false }, { "name", resource.name },
                { "instanceId", resource.GetInstanceID() }, { "type", resource.GetType().FullName },
                { "hideFlags", resource.hideFlags.ToString() } };

        private static bool HasIcon(BlueprintScriptableObject value) =>
            value is BlueprintUnitFact || value is BlueprintItem || value is BlueprintCharacterClass;

        private static Sprite ReadIcon(BlueprintScriptableObject value)
        {
            var fact = value as BlueprintUnitFact;
            if (fact != null) return fact.Icon;
            var item = value as BlueprintItem;
            if (item != null) return item.Icon;
            var characterClass = value as BlueprintCharacterClass;
            return characterClass == null ? null : characterClass.Icon;
        }

        private static string DisplayName(BlueprintScriptableObject value)
        {
            var fact = value as BlueprintUnitFact;
            if (fact != null) return fact.Name;
            var item = value as BlueprintItem;
            return item == null ? value.name : item.Name;
        }

        private static JObject DescribeIcon(Sprite icon)
        {
            if (icon == null) return new JObject { { "isNull", true } };
            return new JObject {
                { "isNull", false }, { "name", icon.name }, { "spriteInstanceId", icon.GetInstanceID() },
                { "texture", icon.texture.name }, { "textureInstanceId", icon.texture.GetInstanceID() },
                { "textureWidth", icon.texture.width }, { "textureHeight", icon.texture.height },
                { "rect", new JArray(icon.rect.x, icon.rect.y, icon.rect.width, icon.rect.height) },
                { "pixelsPerUnit", icon.pixelsPerUnit }
            };
        }

        private static string Components(BlueprintScriptableObject value) => string.Join("|",
            (value.ComponentsArray ?? new BlueprintComponent[0]).Select(component => component == null
                ? "null" : component.GetInstanceID() + ":" + component.GetType().FullName + ":" + component.name));

        private static bool OriginalComponentsPreserved(Before value)
        {
            var current = value.Blueprint.ComponentsArray ?? new BlueprintComponent[0];
            return current.Length >= value.ComponentReferences.Length && value.ComponentReferences
                .Select((component, index) => ReferenceEquals(component, current[index])).All(same => same);
        }

        private static JArray DescribeComponents(BlueprintComponent[] components) => new JArray(
            (components ?? new BlueprintComponent[0]).Select(component => component == null ? null :
                new JObject { { "type", component.GetType().FullName }, { "name", component.name },
                    { "instanceId", component.GetInstanceID() } }));

        private static string Graph(BlueprintScriptableObject value)
        {
            var rows = new List<string>();
            var selection = value as BlueprintFeatureSelection;
            if (selection != null) AddReferences(rows, "selection.AllFeatures", selection.AllFeatures);
            var race = value as BlueprintRace;
            if (race != null) AddReferences(rows, "race.Features", race.Features);
            var ability = value as BlueprintAbility;
            if (ability != null) rows.Add("ability.Parent=" + (ability.Parent == null ? "null" : ability.Parent.AssetGuid));
            var activatable = value as BlueprintActivatableAbility;
            if (activatable != null) rows.Add("activatable.Buff=" + (activatable.Buff == null ? "null" : activatable.Buff.AssetGuid));
            var item = value as BlueprintItemEquipmentUsable;
            if (item != null) rows.Add("item.Ability=" + (item.Ability == null ? "null" : item.Ability.AssetGuid));
            ScanFields(value, "blueprint", rows);
            int index = 0;
            foreach (var component in value.ComponentsArray ?? new BlueprintComponent[0])
            {
                if (component != null) ScanFields(component, "component[" + index + "]." + component.GetType().Name, rows);
                index++;
            }
            return new JArray(rows.OrderBy(row => row, StringComparer.Ordinal)).ToString(Formatting.None);
        }

        private static void AddReferences(List<string> rows, string path, IEnumerable<BlueprintScriptableObject> values)
        {
            if (values == null) { rows.Add(path + "=null"); return; }
            int index = 0;
            foreach (var value in values) rows.Add(path + "[" + index++ + "]=" + (value == null ? "null" : value.AssetGuid));
            rows.Add(path + ".count=" + index);
        }

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private static void ScanFields(object value, string path, List<string> rows)
        {
            rows.AddRange(IconGraphSnapshot.Capture(value, path,
                type => typeof(BlueprintScriptableObject).IsAssignableFrom(type) ||
                    typeof(EquipmentEntityLink).IsAssignableFrom(type),
                reference => reference is BlueprintScriptableObject
                    ? ((BlueprintScriptableObject)reference).AssetGuid : ((EquipmentEntityLink)reference).AssetId,
                type => typeof(ActionList).IsAssignableFrom(type) || typeof(GameAction).IsAssignableFrom(type)));
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions, string name,
            bool pass, string expected, string observed, string evidence)
        {
            assertions.Add(new RuntimeTestAssertion {
                Name = name, Expected = expected, Observed = observed, Evidence = evidence,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail
            });
        }
    }
}
