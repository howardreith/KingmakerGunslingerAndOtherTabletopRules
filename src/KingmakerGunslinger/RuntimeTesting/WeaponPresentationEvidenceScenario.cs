using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-gated cosmetic evidence for the real production weapon models on
    /// a live, disposable humanoid view in the guarded working-save area. This
    /// scenario never runs during ordinary play and never calls a save API.
    /// </summary>
    internal static class WeaponPresentationEvidenceScenario
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const int EvidenceLayer = 31;
        private const int PanelSize = 384;

        private static readonly string[] ProductionVariants =
        {
            WeaponVisualVariantCatalog.PistolService,
            WeaponVisualVariantCatalog.PistolDuelist,
            WeaponVisualVariantCatalog.PistolLastWord,
            WeaponVisualVariantCatalog.RevolverService,
            WeaponVisualVariantCatalog.MusketService,
            WeaponVisualVariantCatalog.BlunderbussService,
            WeaponVisualVariantCatalog.RifleService,
            WeaponVisualVariantCatalog.SpearClassic,
            WeaponVisualVariantCatalog.SpearThorn,
            WeaponVisualVariantCatalog.SpearCrown,
            WeaponVisualVariantCatalog.WakizashiClassic,
            WeaponVisualVariantCatalog.WakizashiPetal,
            WeaponVisualVariantCatalog.WakizashiMoon,
            WeaponVisualVariantCatalog.WakizashiCapstone,
            WeaponVisualVariantCatalog.KatanaClassic,
            WeaponVisualVariantCatalog.KatanaReed,
            WeaponVisualVariantCatalog.KatanaRegal,
            WeaponVisualVariantCatalog.KatanaCapstone,
            WeaponVisualVariantCatalog.NodachiClassic,
            WeaponVisualVariantCatalog.NodachiCleaver,
            WeaponVisualVariantCatalog.NodachiTitan,
            WeaponVisualVariantCatalog.NodachiCapstone
        };

        private static readonly NativeControlSpec[] NativeControls =
        {
            new NativeControlSpec("LightCrossbow",
                ProductionFirearmBlueprints.NativeLightCrossbowWeaponTypeGuid,
                ProductionFirearmBlueprints.NativeStandardLightCrossbowItemGuid),
            new NativeControlSpec("HeavyCrossbow",
                TestMusketBlueprints.NativeHeavyCrossbowWeaponTypeGuid,
                TestMusketBlueprints.NativeStandardHeavyCrossbowItemGuid),
            new NativeControlSpec("Longspear",
                ElvenBranchedSpearBlueprints.NativeLongspearTypeGuid,
                ElvenBranchedSpearBlueprints.NativeLongspearItemGuid),
            new NativeControlSpec("Scimitar",
                EasternWeaponBlueprints.WakizashiVisualDonorGuid, null),
            new NativeControlSpec("BastardSword",
                EasternWeaponBlueprints.KatanaVisualDonorGuid,
                "7b8a4a452f11022488b1c7bfb0ed7746"),
            new NativeControlSpec("Greatsword",
                EasternWeaponBlueprints.NodachiVisualDonorGuid, null)
        };

        private sealed class NativeControlSpec
        {
            internal NativeControlSpec(string label, string typeGuid,
                string preferredItemGuid)
            {
                Label = label;
                TypeGuid = typeGuid;
                PreferredItemGuid = preferredItemGuid;
            }

            internal string Label;
            internal string TypeGuid;
            internal string PreferredItemGuid;
        }

        private sealed class EvidenceCase
        {
            internal EvidenceCase(string symbol, string variant,
                BlueprintItemWeapon item, bool nativeControl,
                string donorTypeGuid)
            {
                Symbol = symbol;
                Variant = variant;
                Item = item;
                NativeControl = nativeControl;
                DonorTypeGuid = donorTypeGuid;
                Family = nativeControl ? "NativeControl" : FamilyFor(variant);
            }

            internal string Symbol;
            internal string Variant;
            internal string Family;
            internal BlueprintItemWeapon Item;
            internal bool NativeControl;
            internal string DonorTypeGuid;
        }

        private sealed class CaptureSummary
        {
            internal string PngPath;
            internal long Bytes;
            internal string Sha256;
            internal int MeaningfulPixels;
            internal string Framing;
            internal bool LowPixelDensity;
        }

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        /// <summary>
        /// Equipment presentation is created and removed by the native view over
        /// successive Unity updates. Keeping this request-local session alive
        /// avoids treating the synchronous UpdateAll call as visual readiness.
        /// </summary>
        internal sealed class Session
        {
            private const int MaximumSettleUpdates = 300;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _evidenceFiles = new List<string>();
            private readonly JArray _records = new JArray();
            private object _allUnits;
            private object _party;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private UnitEntityData _actor;
            private BlueprintUnit _actorBlueprint;
            private Renderer[] _fixtureBodyRenderers = new Renderer[0];
            private ItemEntityWeapon _equipped;
            private bool _equippedFirearmStateSet;
            private EvidenceCase[] _cases = new EvidenceCase[0];
            private Transform _removedPresentation;
            private int _caseIndex;
            private int _phase;
            private int _settleUpdates;
            private int _materialized;
            private int _captured;
            private int _viewCount;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private string _presentationState = "stored";
            private string _stage = "resolve-working-save-anchor";

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                _context = context;
                _request = request;
            }

            internal bool Complete { get; private set; }
            internal RuntimeTestResult Result { get; private set; }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_cleanupStarted)
                    {
                        PollCleanup();
                        return;
                    }
                    if (_phase == 0)
                    {
                        Initialize();
                        _phase = 1;
                        return;
                    }
                    if (_phase == 1)
                    {
                        if (EquipCurrent())
                        {
                            _phase = 2;
                            _settleUpdates = 0;
                        }
                        return;
                    }
                    if (_phase == 2)
                    {
                        PollMaterialization();
                        return;
                    }
                    PollRemoval();
                }
                catch (Exception exception)
                {
                    Add(_assertions, "weapon-presentation-evidence-exception",
                        "no exception", "stage=" + _stage + ";" + exception,
                        false, "guarded request-local visual fixture");
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
                _allUnits = Game.Instance.State.Units.All;
                _party = Game.Instance.Player.Party;
                _unitsBefore = Snapshot(_allUnits);
                _partyBefore = Snapshot(_party);
                UnitEntityData areaAnchor = _partyBefore.OfType<UnitEntityData>()
                    .FirstOrDefault(value => value != null &&
                        value.HoldingState != null && value.View != null);
                if (areaAnchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party-area anchor.");

                _stage = "spawn-disposable-medium-humanoid";
                _actorBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _actorBlueprint.name =
                    "KMG_Runtime_Weapon_Presentation_Evidence_Actor";
                _actorBlueprint.IsCheater = true;
                _actor = Game.Instance.EntityCreator.SpawnUnit(_actorBlueprint,
                    areaAnchor.Position + new Vector3(2.5f, 0f, 2.5f),
                    Quaternion.identity, areaAnchor.HoldingState);
                Game.Instance.EntityCreator.Tick();
                if (_actor == null || _actor.View == null ||
                    _actor.View.Data == null ||
                    _actor.View.HandsEquipment == null)
                    throw new InvalidOperationException(
                        "Native spawning did not attach a complete live humanoid view.");

                ClearHand(_actor, true);
                ClearHand(_actor, false);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _cases = BuildCases();
                EvidenceCase[] production = _cases.Where(value =>
                    !value.NativeControl).ToArray();
                EvidenceCase[] controls = _cases.Where(value =>
                    value.NativeControl).ToArray();
                if (_cases.Length != 28 || production.Length != 22 ||
                    controls.Length != 6 ||
                    production.Select(value => value.Variant).Distinct(
                        StringComparer.Ordinal).Count() != 22 ||
                    !production.Select(value => value.Variant)
                        .SequenceEqual(ProductionVariants))
                    throw new InvalidOperationException(
                        "The evidence catalog is not the exact 22-variant " +
                        "production matrix plus six native controls.");
            }

            private bool EquipCurrent()
            {
                if (_fixtureBodyRenderers.Length == 0)
                {
                    _stage = "settle-empty-handed-body-renderers";
                    Game.Instance.EntityCreator.Tick();
                    _fixtureBodyRenderers = _actor.View
                        .GetComponentsInChildren<Renderer>(true).Where(renderer =>
                            renderer != null && renderer.enabled &&
                            renderer.gameObject.activeInHierarchy).ToArray();
                    if (_fixtureBodyRenderers.Length == 0)
                    {
                        _settleUpdates++;
                        if (_settleUpdates < MaximumSettleUpdates) return false;
                        throw new InvalidOperationException(
                            "The empty-handed disposable humanoid has no active " +
                            "body renderers after " + _settleUpdates +
                            " game updates.");
                    }
                    _diagnostics.Add("emptyHandedBodyRenderers=" +
                        _fixtureBodyRenderers.Length + ";settleUpdates=" +
                        _settleUpdates);
                }
                EvidenceCase value = _cases[_caseIndex];
                _stage = "equip-held-idle-" + value.Variant;
                _equipped = new ItemEntityWeapon(value.Item);
                _actor.Body.PrimaryHand.InsertItem(_equipped);
                if (!ReferenceEquals(_actor.Body.PrimaryHand.MaybeWeapon,
                        _equipped))
                    throw new InvalidOperationException(value.Variant +
                        " did not remain in the primary hand.");
                if (value.Symbol.StartsWith("KMG.Firearms.",
                    StringComparison.Ordinal))
                {
                    FirearmRuntimeState.Service.Set(_equipped,
                        new FirearmState(FirearmState.CurrentSchemaVersion,
                            1, FirearmStateTokenCatalog.DiagnosticLeadBall,
                            FirearmCondition.Normal));
                    _equippedFirearmStateSet = true;
                }
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _presentationState = "stored";
                return true;
            }

            private void PollMaterialization()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "settle-held-idle-" + value.Variant;
                Game.Instance.EntityCreator.Tick();
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                WeaponVisualParameters visual = value.Item.VisualParameters;
                if (visual == null || visual.Model == null)
                    throw new InvalidOperationException(value.Variant +
                        " has no effective held visual model.");
                string presentationRole;
                Transform model = ResolveActivePresentation(_actor, visual,
                    _presentationState, out presentationRole);
                bool exactState = _presentationState == "held-idle"
                    ? _actor.View.HandsEquipment.InCombat
                    : !_actor.View.HandsEquipment.InCombat;
                bool renderable = model != null &&
                    model.gameObject.activeInHierarchy &&
                    model.GetComponentsInChildren<Renderer>(true).Any(renderer =>
                        renderer != null && renderer.enabled &&
                        renderer.gameObject.activeInHierarchy);
                if (!renderable || !exactState)
                {
                    _settleUpdates++;
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Variant +
                        " did not materialize one exact active " +
                        _presentationState + " primary-hand model after " +
                        _settleUpdates + " game updates. inCombat=" +
                        _actor.View.HandsEquipment.InCombat +
                        ";expectedHeld=" + (_presentationState == "held-idle") +
                        ";effectiveModel=" + visual.Model.name +
                        ";activeModel=" + (model == null ? "<null>" :
                            model.name) + ";presentationRole=" +
                        presentationRole + ";renderer hierarchy: " +
                        DescribeRendererHierarchy(_actor.View.transform));
                }
                _materialized++;

                _stage = "capture-" + _presentationState + "-" +
                    value.Variant;
                string stem = _presentationState + "-default-medium-" +
                    SafeFileName(value.Variant);
                CaptureSummary capture = CaptureContactSheet(_actor, model,
                    _fixtureBodyRenderers, Path.Combine(
                        _request.EvidenceDirectory, stem + ".png"));
                JObject record = Describe(value, _actor, model, visual,
                    _fixtureBodyRenderers, capture, stem + ".png",
                    _presentationState, presentationRole);
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".json");
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(capture.PngPath);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _viewCount += 4;
                _diagnostics.Add(value.Variant + ":state=" +
                    _presentationState + ";model=" +
                    TransformPath(model, _actor.View.transform) +
                    ";settleUpdates=" + _settleUpdates + ";png=" +
                    Path.GetFileName(capture.PngPath) + ";sha256=" +
                    capture.Sha256 + ";bytes=" + capture.Bytes +
                    ";meaningfulPixels=" + capture.MeaningfulPixels +
                    ";framing=" + capture.Framing);
                if (capture.LowPixelDensity)
                    _warnings.Add(value.Variant + ":" +
                        _presentationState +
                        " contact sheet has low foreground pixel density; " +
                        "retain it as an explicit framing diagnostic.");

                if (_presentationState == "stored")
                {
                    _actor.View.HandsEquipment.ForceSwitch(true);
                    _presentationState = "held-idle";
                    _settleUpdates = 0;
                    return;
                }

                _removedPresentation = model;
                RemoveEquipped(_actor, ref _equipped,
                    ref _equippedFirearmStateSet);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _phase = 3;
                _settleUpdates = 0;
            }

            private void PollRemoval()
            {
                _stage = "settle-removal-" + _cases[_caseIndex].Variant;
                Game.Instance.EntityCreator.Tick();
                _actor.View.HandsEquipment.UpdateAll();
                GameObject current = _actor.View.HandsEquipment
                    .GetWeaponModel(false);
                bool removed = current == null &&
                    (_removedPresentation == null ||
                    !_removedPresentation.gameObject.activeInHierarchy ||
                    !_removedPresentation.IsChildOf(_actor.View.transform));
                if (!removed)
                {
                    _settleUpdates++;
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(
                        "The prior held presentation remained active after " +
                        _settleUpdates + " game updates: " +
                        TransformPath(_removedPresentation,
                            _actor.View.transform));
                }
                _removedPresentation = null;
                _caseIndex++;
                if (_caseIndex < _cases.Length)
                {
                    _phase = 1;
                    return;
                }
                WriteIndex();
                _indexWritten = true;
                BeginCleanup();
            }

            private void WriteIndex()
            {
                _stage = "write-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 2 },
                    { "states", new JArray("stored", "held-idle") },
                    { "fixture", "live disposable default Medium humanoid" },
                    { "productionVariantCount", 22 },
                    { "nativeControlCount", 6 },
                    { "views", new JArray("front", "right-side", "rear",
                        "front-right-three-quarter") },
                    { "loadedModVersion", _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "records", _records }
                };
                string indexPath = Path.Combine(_request.EvidenceDirectory,
                    "weapon-presentation-held-idle-index.json");
                WriteJsonAtomic(indexPath, index);
                _evidenceFiles.Add(indexPath);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "request-cleanup";
                RemoveEquipped(_actor, ref _equipped,
                    ref _equippedFirearmStateSet);
                if (_actor != null && ContainsReference(_allUnits, _actor))
                    Game.Instance.State.Units.All.Remove(_actor);
                if (_actor != null) _actor.Dispose();
                if (_actorBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                _actorBlueprint = null;
                _cleanupStarted = true;
                _settleUpdates = 0;
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                bool cleaned = SameReferences(_unitsBefore,
                        Snapshot(_allUnits)) &&
                    SameReferences(_partyBefore, Snapshot(_party)) &&
                    (_actor == null || !ContainsReference(_allUnits, _actor));
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates) return;
                Finish(cleaned);
            }

            private void Finish(bool cleaned)
            {
                JObject[] productionRecords = _records.OfType<JObject>()
                    .Where(value => !(bool)value["nativeControl"]).ToArray();
                JObject[] controlRecords = _records.OfType<JObject>()
                    .Where(value => (bool)value["nativeControl"]).ToArray();
                Add(_assertions,
                    "weapon-presentation-production-variant-matrix",
                    "22 exact production visual variants in two states each",
                    "records=" + productionRecords.Length + ";variants=" +
                        productionRecords.Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count(),
                    productionRecords.Length == 44 &&
                        productionRecords.Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count() == 22,
                    "registered production, named, and exact visual-variant catalogs");
                Add(_assertions,
                    "weapon-presentation-native-donor-controls",
                    "six exact native presentation donors in stored and held-idle states",
                    "records=" + controlRecords.Length + ";controls=" +
                        controlRecords.Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count(),
                    controlRecords.Length == 12 &&
                        controlRecords.Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count() == 6,
                    "Light/Heavy Crossbow, Longspear, Scimitar, Bastard Sword, and Greatsword native controls");
                bool nativeGeometryInvariant = controlRecords.GroupBy(value =>
                    (string)value["variant"], StringComparer.Ordinal).All(
                        NativeGeometryInvariant);
                Add(_assertions,
                    "weapon-presentation-native-local-geometry-invariant",
                    "each native control has identical mesh-local geometry in stored and held states",
                    nativeGeometryInvariant ? "6/6 invariant" :
                        "one or more native controls changed local geometry",
                    controlRecords.Length == 12 && nativeGeometryInvariant,
                    "component tolerance=0.00001; Mesh.bounds or SkinnedMeshRenderer.localBounds transformed through the prefab hierarchy, never world AABB reconstruction");
                Add(_assertions, "weapon-presentation-live-materialization",
                    "56/56 exact stored/held presentations on one live native humanoid view",
                    _materialized + "/56", _materialized == 56,
                    "real BlueprintItemWeapon, primary hand, UnitViewHandsEquipment.GetWeaponModel(false), ForceSwitch, and multi-update settling");
                Add(_assertions,
                    "weapon-presentation-state-contact-sheets",
                    "56 PNG/JSON pairs and 224 state-labelled views",
                    "captures=" + _captured + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    _captured == 56 && _viewCount == 224 && _indexWritten &&
                        _evidenceFiles.Count == 113 &&
                    _evidenceFiles.All(File.Exists),
                    "stored and held-idle front/right-side/rear/front-right-three-quarter live render contact sheets");
                int zeroPixelSheets = _records.OfType<JObject>().Count(value =>
                    (int)value["meaningfulPixels"] <= 0);
                int lowDensitySheets = _records.OfType<JObject>().Count(value =>
                    (bool)value["lowPixelDensity"]);
                Add(_assertions, "weapon-presentation-render-visibility",
                    "every state sheet contains non-background pixels; low-density sheets remain explicitly marked",
                    "zeroPixelSheets=" + zeroPixelSheets +
                        ";lowDensitySheets=" + lowDensitySheets,
                    _records.Count == 56 && zeroPixelSheets == 0,
                    "pixel comparison against the request camera's exact solid background");
                Add(_assertions, "weapon-presentation-state-label",
                    "stored and held-idle only; no attack/reload claim",
                    _records.Count == 0 ? "no records" :
                        string.Join(",", _records.OfType<JObject>()
                            .Select(value => (string)value["state"])
                            .Distinct().ToArray()),
                    _records.Count == 56 &&
                        _records.OfType<JObject>().Count(value => string.Equals(
                            (string)value["state"], "stored",
                            StringComparison.Ordinal)) == 28 &&
                        _records.OfType<JObject>().Count(value => string.Equals(
                            (string)value["state"], "held-idle",
                            StringComparison.Ordinal)) == 28,
                    "per-variant JSON evidence and global index");
                Add(_assertions, "weapon-presentation-request-cleanup",
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";settleUpdates=" +
                        _settleUpdates, cleaned,
                    "request-local item, actor, view, layer, camera, light, and texture cleanup");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Cosmetic evidence is limited to stored and " +
                    "held-idle on the default Medium humanoid. It does not " +
                    "establish attack, fire, reload, locomotion, sex-specific, " +
                    "Small, or Enlarged acceptance.");
                RuntimeBuildIdentity build = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                bool passed = _assertions.All(value =>
                    value.Status == RuntimeTestStatuses.Pass);
                Result = new RuntimeTestResult
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = build.RuntimeIdentity + "; mvid=" +
                        build.ModuleVersionId + "; sha256=" +
                        build.LoadedModuleSha256 + "; pid=" + build.ProcessId,
                    GitCommit = build.GitCommit,
                    GameVersion = Application.version ?? string.Empty,
                    StartUtc = _started.ToString("o"),
                    EndUtc = DateTime.UtcNow.ToString("o"),
                    DurationMilliseconds = (long)(DateTime.UtcNow - _started)
                        .TotalMilliseconds,
                    Assertions = _assertions,
                    Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = string.Empty,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }
        }

        private static EvidenceCase[] BuildCases()
        {
            var candidates = new List<EvidenceCase>();
            ProductionFirearmBlueprintCatalog firearms =
                BlueprintBootstrap.ProductionFirearms;
            Add(candidates, ProductionFirearmBlueprints.PistolItemSymbol,
                firearms.Pistol.Item);
            Add(candidates, ProductionFirearmBlueprints.MusketItemSymbol,
                firearms.Musket.Item);
            Add(candidates, ProductionFirearmBlueprints.BlunderbussItemSymbol,
                firearms.Blunderbuss.Item);
            Add(candidates, ProductionFirearmBlueprints.AdvancedRifleItemSymbol,
                firearms.AdvancedRifle.Item);
            Add(candidates,
                ProductionFirearmBlueprints.AdvancedRevolverItemSymbol,
                firearms.AdvancedRevolver.Item);
            foreach (MagicFirearmBlueprintEntry entry in
                BlueprintBootstrap.MagicFirearms.Entries.Where(value =>
                    value.Spec.Symbol == MagicFirearmBlueprints
                        .DuelistsRebuttalSymbol ||
                    value.Spec.Symbol == MagicFirearmBlueprints
                        .TheLastWordSymbol))
                Add(candidates, entry.Spec.Symbol, entry.Item);

            ElvenBranchedSpearBlueprintSet spears =
                BlueprintBootstrap.ElvenBranchedSpears;
            foreach (ElvenBranchedSpearBlueprintEntry entry in spears.Entries)
                Add(candidates, entry.Spec.Symbol, entry.Item);
            foreach (NamedSpearBlueprintEntry entry in spears.Named.Entries)
                Add(candidates, entry.Spec.Symbol, entry.Item);

            EasternWeaponBlueprintSet eastern = BlueprintBootstrap.EasternWeapons;
            foreach (EasternWeaponBlueprintEntry entry in eastern.Entries)
                Add(candidates, entry.Spec.Symbol, entry.Item);
            foreach (EasternWeaponNamedBlueprintEntry entry in
                eastern.Named.Entries)
                Add(candidates, entry.Spec.Symbol, entry.Item);

            EvidenceCase[] production = ProductionVariants.Select(variant =>
                candidates.Where(value =>
                    string.Equals(value.Variant, variant,
                        StringComparison.Ordinal))
                .OrderBy(value => value.Symbol, StringComparer.Ordinal).First())
                .ToArray();
            EvidenceCase[] controls = NativeControls.Select(
                BuildNativeControl).ToArray();
            return production.Concat(controls).ToArray();
        }

        private static void Add(ICollection<EvidenceCase> values, string symbol,
            BlueprintItemWeapon item)
        {
            values.Add(new EvidenceCase(symbol,
                WeaponVisualVariantCatalog.Require(symbol), item, false, null));
        }

        private static EvidenceCase BuildNativeControl(NativeControlSpec spec)
        {
            BlueprintWeaponType type = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponType>(BlueprintBootstrap.Library, spec.TypeGuid,
                    "native " + spec.Label + " presentation donor");
            BlueprintItemWeapon preferred = string.IsNullOrEmpty(
                spec.PreferredItemGuid) ? null : BlueprintLibraryLookup
                .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    spec.PreferredItemGuid, "native " + spec.Label +
                    " presentation control item");
            if (preferred != null && !ReferenceEquals(preferred.Type, type))
                throw new InvalidOperationException(spec.Label +
                    " preferred control item does not use its donor type.");
            BlueprintItemWeapon[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemWeapon>().Where(item =>
                    item != null && ReferenceEquals(item.Type, type) &&
                    item.VisualParameters != null &&
                    item.VisualParameters.Model != null &&
                    type.VisualParameters != null &&
                    ReferenceEquals(item.VisualParameters.Model,
                        type.VisualParameters.Model))
                .OrderBy(item => item.AssetGuid.ToString(),
                    StringComparer.Ordinal).ToArray();
            BlueprintItemWeapon selected = preferred ?? candidates.FirstOrDefault();
            if (selected == null || !candidates.Any(item =>
                    ReferenceEquals(item, selected)))
                throw new InvalidOperationException(spec.Label +
                    " has no exact native item with the donor's held model.");
            return new EvidenceCase("NativeControl." + spec.Label,
                "Native." + spec.Label, selected, true, spec.TypeGuid);
        }

        private static string FamilyFor(string variant)
        {
            int separator = variant.IndexOf('.');
            return separator < 0 ? variant : variant.Substring(0, separator);
        }

        private static Transform FindPresentation(Transform root,
            string modelName)
        {
            if (root == null || string.IsNullOrWhiteSpace(modelName)) return null;
            Transform[] values = root.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null && value.name.StartsWith(
                    modelName, StringComparison.OrdinalIgnoreCase) &&
                    value.gameObject.activeInHierarchy &&
                    value.GetComponentsInChildren<Renderer>(true).Any(renderer =>
                        renderer != null && renderer.enabled &&
                        renderer.gameObject.activeInHierarchy))
                .ToArray();
            if (values.Length == 0) return null;
            int minimumDepth = values.Min(value => TransformDepth(value, root));
            Transform[] top = values.Where(value =>
                TransformDepth(value, root) == minimumDepth).ToArray();
            return top.Length == 1 ? top[0] : null;
        }

        private static Transform ResolveActivePresentation(UnitEntityData actor,
            WeaponVisualParameters visual, string state, out string role)
        {
            GameObject weapon = actor.View.HandsEquipment.GetWeaponModel(false);
            Transform model = weapon == null ? null : weapon.transform;
            if (Renderable(model))
            {
                role = "weapon-model";
                return model;
            }
            if (!string.Equals(state, "stored", StringComparison.Ordinal))
            {
                role = "missing-held-weapon-model";
                return model;
            }
            var candidates = new[]
            {
                new KeyValuePair<string, GameObject>("belt-model",
                    visual.BeltModel),
                new KeyValuePair<string, GameObject>("sheath-model",
                    visual.SheathModel),
                new KeyValuePair<string, GameObject>("stored-weapon-model",
                    visual.Model)
            };
            foreach (KeyValuePair<string, GameObject> candidate in candidates)
            {
                if (candidate.Value == null) continue;
                Transform resolved = FindPresentation(actor.View.transform,
                    candidate.Value.name);
                if (!Renderable(resolved)) continue;
                role = candidate.Key;
                return resolved;
            }
            role = "missing-stored-presentation";
            return model;
        }

        private static bool Renderable(Transform value)
        {
            return value != null && value.gameObject.activeInHierarchy &&
                value.GetComponentsInChildren<Renderer>(true).Any(renderer =>
                    renderer != null && renderer.enabled &&
                    renderer.gameObject.activeInHierarchy);
        }

        private static int TransformDepth(Transform value, Transform root)
        {
            int depth = 0;
            for (Transform current = value; current != null &&
                !ReferenceEquals(current, root); current = current.parent)
                depth++;
            return depth;
        }

        private static string DescribeRendererHierarchy(Transform root)
        {
            if (root == null) return "<root-null>";
            string[] values = root.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null)
                .Select(value => TransformPath(value.transform, root) +
                    "[enabled=" + value.enabled + ";active=" +
                    value.gameObject.activeInHierarchy + "]")
                .OrderBy(value => value, StringComparer.Ordinal)
                .Take(80).ToArray();
            return values.Length == 0 ? "<none>" :
                string.Join("|", values);
        }

        private static CaptureSummary CaptureContactSheet(UnitEntityData actor,
            Transform model, Renderer[] fixtureBodyRenderers, string pngPath)
        {
            Renderer[] weaponRenderers = model
                .GetComponentsInChildren<Renderer>(true).Where(value =>
                    value != null && value.enabled &&
                    value.gameObject.activeInHierarchy).ToArray();
            Renderer[] bodyRenderers = (fixtureBodyRenderers ??
                new Renderer[0]).Where(value =>
                    value != null && value.enabled &&
                    value.gameObject.activeInHierarchy).ToArray();
            if (weaponRenderers.Length == 0 || bodyRenderers.Length == 0)
                throw new InvalidOperationException(
                    "The live weapon evidence view requires active body and weapon renderers.");
            Bounds weaponBounds = CombinedBounds(weaponRenderers);
            Bounds bodyBounds = CombinedBounds(bodyRenderers);
            Bounds bounds = bodyBounds;
            bounds.Encapsulate(weaponBounds);
            float bodyMaximum = Mathf.Max(bodyBounds.size.x,
                Mathf.Max(bodyBounds.size.y, bodyBounds.size.z));
            float combinedMaximum = Mathf.Max(bounds.size.x,
                Mathf.Max(bounds.size.y, bounds.size.z));
            float maximumUsefulFrame = Mathf.Max(3f, bodyMaximum * 1.75f);
            bool capped = combinedMaximum > maximumUsefulFrame ||
                Vector3.Distance(bodyBounds.center, weaponBounds.center) >
                    maximumUsefulFrame;
            Vector3 frameCenter = capped ? bodyBounds.center : bounds.center;
            float frameMaximum = capped ? maximumUsefulFrame : combinedMaximum;
            string framing = "mode=" + (capped ? "body-centered-capped" :
                    "combined") + ";body=" + bodyBounds.size.ToString("R") +
                ";weapon=" + weaponBounds.size.ToString("R") +
                ";combined=" + bounds.size.ToString("R") + ";center=" +
                frameCenter.ToString("R") + ";maximum=" +
                frameMaximum.ToString("R");
            Camera liveCamera = UnityEngine.Object.FindObjectsOfType<Camera>()
                .Where(value => value != null && value.enabled)
                .OrderByDescending(value => ReferenceEquals(value, Camera.main))
                .FirstOrDefault();
            if (liveCamera == null)
                throw new InvalidOperationException(
                    "The working-save evidence run has no enabled game camera.");

            var layers = actor.View.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null).Select(value => value.gameObject)
                .Distinct().ToDictionary(value => value, value => value.layer);
            var cameraObject = new GameObject(
                "KMG_Runtime_WeaponPresentationEvidenceCamera");
            var lightObject = new GameObject(
                "KMG_Runtime_WeaponPresentationEvidenceLight");
            Camera camera = cameraObject.AddComponent<Camera>();
            Light light = lightObject.AddComponent<Light>();
            RenderTexture target = null;
            Texture2D panel = null;
            Texture2D sheet = null;
            RenderTexture priorActive = RenderTexture.active;
            int meaningful = 0;
            try
            {
                foreach (KeyValuePair<GameObject, int> value in layers)
                    value.Key.layer = EvidenceLayer;
                camera.CopyFrom(liveCamera);
                camera.enabled = false;
                camera.cullingMask = 1 << EvidenceLayer;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.14f, 0.17f, 1f);
                camera.orthographic = true;
                camera.aspect = 1f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.orthographicSize = Mathf.Max(1.15f,
                    frameMaximum * 0.62f);
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.cullingMask = 1 << EvidenceLayer;
                light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
                target = new RenderTexture(PanelSize, PanelSize, 24,
                    RenderTextureFormat.ARGB32);
                camera.targetTexture = target;
                panel = new Texture2D(PanelSize, PanelSize,
                    TextureFormat.RGBA32, false, false);
                sheet = new Texture2D(PanelSize * 2, PanelSize * 2,
                    TextureFormat.RGBA32, false, false);
                Color32 fill = camera.backgroundColor;
                Color32[] initial = Enumerable.Repeat(fill,
                    sheet.width * sheet.height).ToArray();
                sheet.SetPixels32(initial);

                Vector3 forward = actor.View.transform.forward.normalized;
                Vector3 right = actor.View.transform.right.normalized;
                Vector3[] directions =
                {
                    forward,
                    right,
                    -forward,
                    (forward + right).normalized
                };
                int[] offsetsX = { 0, PanelSize, 0, PanelSize };
                int[] offsetsY = { PanelSize, PanelSize, 0, 0 };
                float distance = Mathf.Max(6f, frameMaximum * 4f);
                for (int index = 0; index < directions.Length; index++)
                {
                    camera.transform.position = frameCenter +
                        directions[index] * distance;
                    camera.transform.LookAt(frameCenter);
                    camera.Render();
                    RenderTexture.active = target;
                    panel.ReadPixels(new Rect(0, 0, PanelSize, PanelSize),
                        0, 0, false);
                    panel.Apply(false, false);
                    Color32[] pixels = panel.GetPixels32();
                    meaningful += pixels.Count(pixel =>
                        Math.Abs(pixel.r - fill.r) +
                        Math.Abs(pixel.g - fill.g) +
                        Math.Abs(pixel.b - fill.b) > 24);
                    sheet.SetPixels32(offsetsX[index], offsetsY[index],
                        PanelSize, PanelSize, pixels);
                }
                sheet.Apply(false, false);
                byte[] png = EncodePng(sheet);
                if (png == null || png.Length < 4096)
                    throw new InvalidOperationException(
                        "The presentation contact-sheet PNG was empty.");
                File.WriteAllBytes(pngPath, png);
                return new CaptureSummary
                {
                    PngPath = pngPath,
                    Bytes = png.LongLength,
                    Sha256 = Hash(pngPath),
                    MeaningfulPixels = meaningful,
                    Framing = framing,
                    LowPixelDensity = meaningful < PanelSize * PanelSize / 5
                };
            }
            finally
            {
                foreach (KeyValuePair<GameObject, int> value in layers)
                    if (value.Key != null) value.Key.layer = value.Value;
                RenderTexture.active = priorActive;
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                if (panel != null) UnityEngine.Object.DestroyImmediate(panel);
                if (sheet != null) UnityEngine.Object.DestroyImmediate(sheet);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static JObject Describe(EvidenceCase value,
            UnitEntityData actor, Transform model, WeaponVisualParameters visual,
            Renderer[] fixtureBodyRenderers, CaptureSummary capture,
            string pngName, string state, string presentationRole)
        {
            Renderer[] weaponRenderers = model
                .GetComponentsInChildren<Renderer>(true).Where(renderer =>
                    renderer != null && renderer.enabled &&
                    renderer.gameObject.activeInHierarchy).ToArray();
            Renderer[] bodyRenderers = (fixtureBodyRenderers ??
                new Renderer[0]).Where(renderer =>
                    renderer != null && renderer.enabled &&
                    renderer.gameObject.activeInHierarchy).ToArray();
            Bounds weaponBounds = CombinedBounds(weaponRenderers);
            Bounds bodyBounds = CombinedBounds(bodyRenderers);
            int modelLocalBoundsSourceCount;
            Bounds modelLocalBounds = LocalBounds(model, weaponRenderers,
                out modelLocalBoundsSourceCount);
            Vector3 overlap = Vector3.Max(Vector3.zero,
                Vector3.Min(weaponBounds.max, bodyBounds.max) -
                Vector3.Max(weaponBounds.min, bodyBounds.min));
            var anchors = new JObject();
            foreach (string name in new[] { "Grip", "Muzzle", "Tip", "Butt",
                "SupportHandTarget", "WeaponUp", "HeadUp", "BladeNormal",
                "BackMount", "BeltMount" })
            {
                Transform anchor = model.Find(name);
                if (anchor == null)
                {
                    anchors[name] = JValue.CreateNull();
                }
                else
                {
                    anchors[name] = new JObject
                    {
                        { "localPosition", anchor.localPosition.ToString("R") },
                        { "worldPosition", anchor.position.ToString("R") },
                        { "localRotation", anchor.localRotation.eulerAngles
                            .ToString("R") }
                    };
                }
            }
            return new JObject
            {
                { "family", value.Family },
                { "variant", value.Variant },
                { "symbol", value.Symbol },
                { "nativeControl", value.NativeControl },
                { "donorTypeGuid", value.DonorTypeGuid == null
                    ? JValue.CreateNull() : (JToken)value.DonorTypeGuid },
                { "itemGuid", value.Item.AssetGuid },
                { "itemName", value.Item.Name },
                { "state", state },
                { "handsEquipmentInCombat",
                    actor.View.HandsEquipment.InCombat },
                { "fixtureSize", actor.Descriptor.State.Size.ToString() },
                { "fixtureGender", ReadOptional(actor.Descriptor, "Gender") },
                { "model", visual.Model == null ? "<null>" : visual.Model.name },
                { "beltModel", visual.BeltModel == null ? "<null>" :
                    visual.BeltModel.name },
                { "sheathModel", visual.SheathModel == null ? "<null>" :
                    visual.SheathModel.name },
                { "modelPath", TransformPath(model, actor.View.transform) },
                { "presentationRole", presentationRole },
                { "modelLocalPosition", model.localPosition.ToString("R") },
                { "modelLocalRotation", model.localRotation.eulerAngles
                    .ToString("R") },
                { "modelLocalScale", model.localScale.ToString("R") },
                { "weaponRendererCount", weaponRenderers.Length },
                { "weaponBoundsCenter", weaponBounds.center.ToString("R") },
                { "weaponBoundsSize", weaponBounds.size.ToString("R") },
                { "modelLocalRendererBoundsCenter",
                    modelLocalBounds.center.ToString("R") },
                { "modelLocalRendererBoundsCenterComponents",
                    Components(modelLocalBounds.center) },
                { "modelLocalRendererBoundsSize",
                    modelLocalBounds.size.ToString("R") },
                { "modelLocalRendererBoundsSizeComponents",
                    Components(modelLocalBounds.size) },
                { "modelLocalMajorAxis", MajorAxis(modelLocalBounds.size) },
                { "modelLocalMinorAxis", MinorAxis(modelLocalBounds.size) },
                { "modelLocalBoundsSourceCount", modelLocalBoundsSourceCount },
                { "semanticLocators", DescribeSemanticLocators(model) },
                { "bodyBoundsCenter", bodyBounds.center.ToString("R") },
                { "bodyBoundsSize", bodyBounds.size.ToString("R") },
                { "aabbOverlap", overlap.ToString("R") },
                { "aabbOverlapVolume", overlap.x * overlap.y * overlap.z },
                { "anchors", anchors },
                { "views", new JArray("front", "right-side", "rear",
                    "front-right-three-quarter") },
                { "png", pngName },
                { "pngBytes", capture.Bytes },
                { "pngSha256", capture.Sha256 },
                { "meaningfulPixels", capture.MeaningfulPixels },
                { "lowPixelDensity", capture.LowPixelDensity },
                { "captureFraming", capture.Framing },
                { "claimBoundary",
                    "cosmetic " + state +
                    " evidence only; no attack, reload, or movement claim" }
            };
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
                throw new InvalidOperationException(
                    "Presentation evidence requires renderer bounds.");
            Bounds value = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
                value.Encapsulate(renderers[index].bounds);
            return value;
        }

        private static Bounds LocalBounds(Transform root, Renderer[] renderers,
            out int sourceCount)
        {
            bool initialized = false;
            Bounds value = new Bounds();
            sourceCount = 0;
            foreach (Renderer renderer in renderers)
            {
                Transform sourceTransform;
                Bounds bounds;
                SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
                if (skinned != null)
                {
                    sourceTransform = skinned.transform;
                    bounds = skinned.localBounds;
                }
                else
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null) continue;
                    sourceTransform = filter.transform;
                    bounds = filter.sharedMesh.bounds;
                }
                sourceCount++;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                for (int x = 0; x < 2; x++)
                    for (int y = 0; y < 2; y++)
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3 sourceLocal = new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z);
                            Vector3 local = root.InverseTransformPoint(
                                sourceTransform.TransformPoint(sourceLocal));
                            if (!initialized)
                            {
                                value = new Bounds(local, Vector3.zero);
                                initialized = true;
                            }
                            else value.Encapsulate(local);
                        }
            }
            if (!initialized) throw new InvalidOperationException(
                "Local presentation bounds require at least one mesh-backed renderer.");
            return value;
        }

        private static JArray Components(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }

        private static bool NativeGeometryInvariant(
            IGrouping<string, JObject> group)
        {
            JObject[] values = group.ToArray();
            if (values.Length != 2) return false;
            return SameComponents(
                    values[0]["modelLocalRendererBoundsCenterComponents"],
                    values[1]["modelLocalRendererBoundsCenterComponents"],
                    0.00001) &&
                SameComponents(
                    values[0]["modelLocalRendererBoundsSizeComponents"],
                    values[1]["modelLocalRendererBoundsSizeComponents"],
                    0.00001) &&
                string.Equals((string)values[0]["modelLocalMajorAxis"],
                    (string)values[1]["modelLocalMajorAxis"],
                    StringComparison.Ordinal) &&
                string.Equals((string)values[0]["modelLocalMinorAxis"],
                    (string)values[1]["modelLocalMinorAxis"],
                    StringComparison.Ordinal) &&
                (int)values[0]["modelLocalBoundsSourceCount"] ==
                    (int)values[1]["modelLocalBoundsSourceCount"];
        }

        private static bool SameComponents(JToken left, JToken right,
            double tolerance)
        {
            JArray leftValues = left as JArray;
            JArray rightValues = right as JArray;
            if (leftValues == null || rightValues == null ||
                leftValues.Count != 3 || rightValues.Count != 3)
                return false;
            for (int i = 0; i < 3; i++)
                if (Math.Abs((double)leftValues[i] -
                        (double)rightValues[i]) > tolerance)
                    return false;
            return true;
        }

        private static string MajorAxis(Vector3 size)
        {
            if (size.x >= size.y && size.x >= size.z) return "+/-X";
            return size.y >= size.z ? "+/-Y" : "+/-Z";
        }

        private static string MinorAxis(Vector3 size)
        {
            if (size.x <= size.y && size.x <= size.z) return "+/-X";
            return size.y <= size.z ? "+/-Y" : "+/-Z";
        }

        private static JArray DescribeSemanticLocators(Transform root)
        {
            string[] needles = { "ik_target", "warhead", "weaponcenter",
                "trail", "surface", "muzzle", "tip", "grip", "butt",
                "support", "up", "normal", "mount" };
            var values = new JArray();
            foreach (Transform transform in root.GetComponentsInChildren<
                Transform>(true).Where(transform => transform != null &&
                    needles.Any(needle => transform.name.IndexOf(needle,
                        StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(transform => TransformPath(transform, root),
                    StringComparer.Ordinal).Take(100))
            {
                values.Add(new JObject
                {
                    { "path", TransformPath(transform, root) },
                    { "modelLocalPosition", root.InverseTransformPoint(
                        transform.position).ToString("R") },
                    { "modelLocalForward", root.InverseTransformDirection(
                        transform.forward).ToString("R") },
                    { "modelLocalUp", root.InverseTransformDirection(
                        transform.up).ToString("R") }
                });
            }
            return values;
        }

        private static void ClearHand(UnitEntityData actor, bool primary)
        {
            if (actor == null || actor.Body == null) return;
            var slot = primary ? actor.Body.PrimaryHand :
                actor.Body.SecondaryHand;
            if (slot != null && slot.MaybeItem != null)
                slot.RemoveItem(false);
        }

        private static void RemoveEquipped(UnitEntityData actor,
            ref ItemEntityWeapon item, ref bool firearmStateSet)
        {
            if (actor != null && actor.Body != null &&
                actor.Body.PrimaryHand != null &&
                actor.Body.PrimaryHand.MaybeItem != null)
                actor.Body.PrimaryHand.RemoveItem(false);
            if (item != null)
            {
                if (firearmStateSet)
                    FirearmRuntimeState.Service.Forget(item);
                item.Dispose();
            }
            item = null;
            firearmStateSet = false;
        }

        private static string TransformPath(Transform value, Transform root)
        {
            if (value == null) return "<null>";
            var names = new List<string>();
            for (Transform current = value; current != null;
                current = current.parent)
            {
                names.Add(current.name);
                if (ReferenceEquals(current, root)) break;
            }
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static string SafeFileName(string value)
        {
            var builder = new StringBuilder();
            foreach (char character in value.ToLowerInvariant())
                builder.Append(char.IsLetterOrDigit(character) ? character : '-');
            return builder.ToString().Trim('-');
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static void WriteJsonAtomic(string path, JToken value)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") +
                ".tmp";
            File.WriteAllText(temporary, value.ToString(Formatting.Indented),
                new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }

        private static object Read(object owner, string name)
        {
            if (owner == null) return null;
            for (Type type = owner.GetType(); type != null;
                type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Members |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(owner);
                PropertyInfo property = type.GetProperty(name, Members |
                    BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(owner, null);
            }
            throw new MissingMemberException(owner.GetType().FullName, name);
        }

        private static string ReadOptional(object owner, string name)
        {
            try
            {
                object value = Read(owner, name);
                return value == null ? "<null>" : value.ToString();
            }
            catch { return "<unavailable>"; }
        }

        private static object[] Snapshot(object collection)
        {
            var enumerable = collection as IEnumerable;
            return enumerable == null ? new object[0] :
                enumerable.Cast<object>().ToArray();
        }

        private static bool SameReferences(object[] expected, object[] actual)
        {
            if (expected.Length != actual.Length) return false;
            return expected.All(value => actual.Any(current =>
                ReferenceEquals(value, current)));
        }

        private static bool ContainsReference(object collection, object target)
        {
            return Snapshot(collection).Any(value =>
                ReferenceEquals(value, target));
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
        }

        private static byte[] EncodePng(Texture2D texture)
        {
            if (texture == null || texture.width <= 0 || texture.height <= 0)
                throw new ArgumentException("PNG texture is invalid.");
            Color32[] pixels = texture.GetPixels32();
            int stride = checked(texture.width * 4 + 1);
            byte[] scanlines = new byte[checked(stride * texture.height)];
            for (int outputY = 0; outputY < texture.height; outputY++)
            {
                int sourceY = texture.height - outputY - 1;
                int destination = outputY * stride;
                scanlines[destination++] = 0;
                for (int x = 0; x < texture.width; x++)
                {
                    Color32 pixel = pixels[sourceY * texture.width + x];
                    scanlines[destination++] = pixel.r;
                    scanlines[destination++] = pixel.g;
                    scanlines[destination++] = pixel.b;
                    scanlines[destination++] = pixel.a;
                }
            }
            byte[] compressed = ZlibStore(scanlines);
            using (var stream = new MemoryStream())
            {
                stream.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 },
                    0, 8);
                using (var header = new MemoryStream())
                {
                    WriteUInt32(header, (uint)texture.width);
                    WriteUInt32(header, (uint)texture.height);
                    header.Write(new byte[] { 8, 6, 0, 0, 0 }, 0, 5);
                    WritePngChunk(stream, "IHDR", header.ToArray());
                }
                WritePngChunk(stream, "IDAT", compressed);
                WritePngChunk(stream, "IEND", new byte[0]);
                return stream.ToArray();
            }
        }

        private static byte[] ZlibStore(byte[] data)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x78);
                stream.WriteByte(0x01);
                int offset = 0;
                while (offset < data.Length)
                {
                    int count = Math.Min(65535, data.Length - offset);
                    stream.WriteByte((byte)(offset + count == data.Length ? 1 : 0));
                    stream.WriteByte((byte)count);
                    stream.WriteByte((byte)(count >> 8));
                    int complement = (~count) & 0xffff;
                    stream.WriteByte((byte)complement);
                    stream.WriteByte((byte)(complement >> 8));
                    stream.Write(data, offset, count);
                    offset += count;
                }
                uint s1 = 1, s2 = 0;
                foreach (byte value in data)
                {
                    s1 = (s1 + value) % 65521;
                    s2 = (s2 + s1) % 65521;
                }
                WriteUInt32(stream, (s2 << 16) | s1);
                return stream.ToArray();
            }
        }

        private static void WritePngChunk(Stream stream, string type,
            byte[] data)
        {
            WriteUInt32(stream, (uint)data.Length);
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            stream.Write(typeBytes, 0, typeBytes.Length);
            stream.Write(data, 0, data.Length);
            uint crc = 0xffffffff;
            foreach (byte value in typeBytes) crc = UpdateCrc(crc, value);
            foreach (byte value in data) crc = UpdateCrc(crc, value);
            WriteUInt32(stream, crc ^ 0xffffffff);
        }

        private static uint UpdateCrc(uint crc, byte value)
        {
            crc ^= value;
            for (int index = 0; index < 8; index++)
                crc = (crc & 1) != 0 ? 0xedb88320 ^ (crc >> 1) : crc >> 1;
            return crc;
        }

        private static void WriteUInt32(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }
    }
}
