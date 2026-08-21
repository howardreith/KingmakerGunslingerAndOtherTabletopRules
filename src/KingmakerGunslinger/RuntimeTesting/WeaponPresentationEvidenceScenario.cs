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
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
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

        private static readonly string[] LongGunMotionVariants =
        {
            WeaponVisualVariantCatalog.MusketService,
            WeaponVisualVariantCatalog.BlunderbussService,
            WeaponVisualVariantCatalog.RifleService,
            "Native.HeavyCrossbow"
        };

        private static readonly string[] SpearMotionVariants =
        {
            WeaponVisualVariantCatalog.SpearClassic,
            WeaponVisualVariantCatalog.SpearThorn,
            WeaponVisualVariantCatalog.SpearCrown,
            "Native.Longspear"
        };

        private static readonly string[] EasternMotionVariants =
        {
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
            WeaponVisualVariantCatalog.NodachiCapstone,
            "Native.Scimitar",
            "Native.BastardSword",
            "Native.Greatsword"
        };

        private static readonly int[] AttackCaptureUpdates =
        {
            1, 4, 8, 12, 18, 24, 36, 60, 96
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

        internal static MotionSession BeginMotion(ModContext context,
            RuntimeTestRequest request)
        {
            return new MotionSession(context, request);
        }

        internal static TransitionMotionSession BeginTransitionMotion(
            ModContext context, RuntimeTestRequest request)
        {
            return new TransitionMotionSession(context, request);
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

        private sealed class MotionOutcome
        {
            internal string Variant;
            internal bool Firearm;
            internal bool CommandInstalled;
            internal bool CommandCanStart;
            internal bool CommandCloseEnough;
            internal bool CommandTargetInState;
            internal bool CommandStarted;
            internal bool CommandRunningObserved;
            internal bool AnimationObserved;
            internal bool AnimationActedObserved;
            internal bool CommandFinishedBeforeInterrupt;
            internal bool CommandNeedLoS;
            internal float CommandApproachRadius;
            internal float CommandTargetDistance;
            internal int CommandTargetAttempts;
            internal string CommandTargetPlacement;
            internal int ExplicitCommandTicks;
            internal long FiredDelta;
            internal long FaultDelta;
            internal int LoadedRoundsAfter;
        }

        /// <summary>
        /// A distinct request-gated session for real combat pose sampling. The
        /// ordinary evidence session intentionally remains limited to stored and
        /// held-idle states; this fixture adds an immortal disposable target and
        /// issues native UnitAttack commands without changing either blueprint or
        /// save state.
        /// </summary>
        internal sealed class MotionSession
        {
            private const int MaximumSettleUpdates = 300;
            private const int ReadySettleUpdates = 30;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _evidenceFiles = new List<string>();
            private readonly List<MotionOutcome> _outcomes =
                new List<MotionOutcome>();
            private readonly JArray _records = new JArray();
            private readonly bool _spearMotion;
            private readonly bool _easternMotion;
            private readonly string[] _motionVariants;
            private object _allUnits;
            private object _party;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private UnitEntityData _actor;
            private UnitEntityData _target;
            private BlueprintUnit _actorBlueprint;
            private BlueprintUnit _hostileBlueprint;
            private Renderer[] _fixtureBodyRenderers = new Renderer[0];
            private ItemEntityWeapon _equipped;
            private bool _equippedFirearmStateSet;
            private EvidenceCase[] _cases = new EvidenceCase[0];
            private UnitAttack _attackCommand;
            private Transform _removedPresentation;
            private int _caseIndex;
            private int _phase;
            private int _settleUpdates;
            private int _attackUpdates;
            private int _captureScheduleIndex;
            private int _captured;
            private int _viewCount;
            private bool _commandInstalled;
            private bool _commandCanStart;
            private bool _commandCloseEnough;
            private bool _commandTargetInState;
            private bool _commandStarted;
            private bool _commandRunningObserved;
            private bool _animationObserved;
            private bool _animationActedObserved;
            private bool _commandNeedLoS;
            private float _commandApproachRadius;
            private float _commandTargetDistance;
            private int _commandTargetAttempts;
            private string _commandTargetPlacement = "<not-prepared>";
            private int _explicitCommandTicks;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private long _firedBefore;
            private long _faultsBefore;
            private string _stage = "resolve-working-save-anchor";
            private string _exceptionSummary = string.Empty;

            internal MotionSession(ModContext context,
                RuntimeTestRequest request)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                _context = context;
                _request = request;
                _spearMotion = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .WeaponPresentationSpearMotionEvidence,
                    StringComparison.Ordinal);
                _easternMotion = string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog
                        .WeaponPresentationEasternMotionEvidence,
                    StringComparison.Ordinal);
                _motionVariants = _spearMotion ? SpearMotionVariants :
                    _easternMotion ? EasternMotionVariants :
                    LongGunMotionVariants;
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
                        PollCombatReady();
                        return;
                    }
                    if (_phase == 3)
                    {
                        PollAttackSequence();
                        return;
                    }
                    PollRemoval();
                }
                catch (Exception exception)
                {
                    _exceptionSummary = "stage=" + _stage + ";" + exception;
                    Add(_assertions,
                        "weapon-presentation-motion-evidence-exception",
                        "no exception", _exceptionSummary, false,
                        "guarded request-local combat visual fixture");
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

                _stage = "spawn-disposable-combat-pair";
                _actorBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _actorBlueprint.name =
                    "KMG_Runtime_Weapon_Presentation_Motion_Actor";
                _actorBlueprint.IsCheater = true;
                Vector3 actorPosition = NearestNavigable(areaAnchor.Position +
                    new Vector3(4f, 0f, 4f));
                _actor = Game.Instance.EntityCreator.SpawnUnit(_actorBlueprint,
                    actorPosition, Quaternion.identity, areaAnchor.HoldingState);
                _target = ElvenBranchedSpearCombatScenario.SpawnHostileTarget(
                    _actor, _actorBlueprint, NearestNavigable(actorPosition +
                        Vector3.forward * 6f), areaAnchor.HoldingState,
                    out _hostileBlueprint);
                Game.Instance.EntityCreator.Tick();
                if (_actor == null || _target == null || _actor.View == null ||
                    _target.View == null || _actor.View.Data == null ||
                    _actor.View.HandsEquipment == null)
                    throw new InvalidOperationException(
                        "Native spawning did not attach the disposable combat views.");

                _actor.Descriptor.State.Immortality.Retain();
                _target.Descriptor.State.Immortality.Retain();
                _target.Descriptor.Stats.HitPoints.BaseValue = 10000;
                _target.Descriptor.Damage = 0;
                ClearHand(_actor, true);
                ClearHand(_actor, false);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(true);
                _actor.CombatState.JoinCombat();
                _target.CombatState.JoinCombat();
                _actor.CombatState.Engage(_target);
                _target.Commands.InterruptAll(true);
                _cases = BuildMotionCases(_motionVariants);
                if (_cases.Length != _motionVariants.Length ||
                    !_cases.Select(value => value.Variant)
                        .SequenceEqual(_motionVariants))
                    throw new InvalidOperationException(
                        "The motion catalog is not the exact " +
                        (_spearMotion ?
                            "three production branched spears plus native Longspear" :
                         _easternMotion ?
                            "twelve production Eastern variants plus three native sword" :
                            "three production long guns plus native Heavy Crossbow") +
                        " control set.");
            }

            private bool EquipCurrent()
            {
                if (_fixtureBodyRenderers.Length == 0)
                {
                    _stage = "settle-empty-handed-motion-body-renderers";
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
                            "The disposable combat actor has no active body renderers.");
                    }
                }

                EvidenceCase value = _cases[_caseIndex];
                _stage = "equip-combat-ready-" + value.Variant;
                _actor.Commands.InterruptAll(true);
                _target.Commands.InterruptAll(true);
                _target.Descriptor.Damage = 0;
                if (!_actor.CombatState.IsInCombat)
                    _actor.CombatState.JoinCombat();
                if (!_target.CombatState.IsInCombat)
                    _target.CombatState.JoinCombat();
                _equipped = new ItemEntityWeapon(value.Item);
                _actor.Body.PrimaryHand.InsertItem(_equipped);
                if (!ReferenceEquals(_actor.Body.PrimaryHand.MaybeWeapon,
                        _equipped))
                    throw new InvalidOperationException(value.Variant +
                        " did not remain in the primary hand.");
                if (IsFirearm(value))
                {
                    FirearmRuntimeState.Service.Set(_equipped,
                        new FirearmState(FirearmState.CurrentSchemaVersion,
                            1, FirearmStateTokenCatalog.DiagnosticLeadBall,
                            FirearmCondition.Normal));
                    _equippedFirearmStateSet = true;
                }
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(true);
                _actor.CombatState.Engage(_target);
                return true;
            }

            private void PollCombatReady()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "settle-combat-ready-" + value.Variant;
                Game.Instance.EntityCreator.Tick();
                _target.Commands.InterruptAll(true);
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                WeaponVisualParameters visual = value.Item.VisualParameters;
                if (visual == null || visual.Model == null)
                    throw new InvalidOperationException(value.Variant +
                        " has no effective held visual model.");
                string role;
                Transform model = ResolveActivePresentation(_actor, visual,
                    "combat-ready", out role);
                bool ready = Renderable(model) &&
                    _actor.View.HandsEquipment.InCombat &&
                    _actor.CombatState.IsInCombat;
                _settleUpdates++;
                if (!ready || _settleUpdates < ReadySettleUpdates)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Variant +
                        " did not reach an exact live combat-ready presentation " +
                        "after " + _settleUpdates + " updates. renderable=" +
                        Renderable(model) + ";handsInCombat=" +
                        _actor.View.HandsEquipment.InCombat + ";unitInCombat=" +
                        _actor.CombatState.IsInCombat + ";role=" + role + ".");
                }

                CaptureMotionRecord(value, model, visual, role,
                    "combat-ready", 0,
                    "live combat-ready evidence before native attack command");
                _firedBefore = FirearmDischargeRuntimeDiagnostics.Fired;
                _faultsBefore = FirearmDischargeRuntimeDiagnostics.Faults;
                UnitCommand issued = UnitAttack.CreateAttackCommand(_actor,
                    _target);
                _attackCommand = issued as UnitAttack;
                if (_attackCommand == null)
                    throw new InvalidOperationException(
                        "Native UnitAttack.CreateAttackCommand did not produce " +
                        "a UnitAttack for " + value.Variant + ": " +
                        (issued == null ? "<null>" : issued.GetType().FullName));
                _attackCommand.IsSingleAttack = true;
                _actor.Commands.Run(_attackCommand);
                PrepareAttackStart(value);
                _attackCommand.Start();
                _commandInstalled = _actor.Commands.Contains(_attackCommand);
                _commandStarted = _attackCommand.IsStarted;
                _commandRunningObserved = _attackCommand.IsRunning;
                _animationObserved = _attackCommand.Animation != null;
                _animationActedObserved = _attackCommand.Animation != null &&
                    _attackCommand.Animation.IsActed;
                if (!_commandStarted || !_commandRunningObserved)
                    throw new InvalidOperationException(value.Variant +
                        " native UnitAttack failed after exact start readiness. " +
                        "started=" + _commandStarted + ";running=" +
                        _commandRunningObserved + ";result=" +
                        _attackCommand.Result + ";animation=" +
                        (_attackCommand.Animation == null ? "<none>" :
                            _attackCommand.Animation.GetType().FullName) + ".");
                _attackUpdates = 0;
                _captureScheduleIndex = 0;
                _phase = 3;
            }

            private void PrepareAttackStart(EvidenceCase value)
            {
                Vector3 forward = _actor.OrientationDirection;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.5f) forward = Vector3.forward;
                forward.Normalize();
                Vector3 right = new Vector3(forward.z, 0f, -forward.x);
                Vector3[] directions =
                {
                    forward,
                    right,
                    -right,
                    -forward,
                    (forward + right).normalized,
                    (forward - right).normalized,
                    (-forward + right).normalized,
                    (-forward - right).normalized
                };
                float[] distances = { 6f, 4f, 2f, 1f, 0.5f };
                var attempts = new List<string>();
                _commandTargetAttempts = 0;
                foreach (float distance in distances)
                    for (int directionIndex = 0;
                        directionIndex < directions.Length; directionIndex++)
                    {
                        Vector3 requested = _actor.Position +
                            directions[directionIndex] * distance;
                        Vector3 candidate = NearestNavigable(requested);
                        SetUnitPosition(_target, candidate);
                        _actor.ForceLookAt(candidate);
                        _target.ForceLookAt(_actor.Position);
                        Game.Instance.EntityCreator.Tick();
                        _commandTargetAttempts++;
                        bool targetInState = _target.IsInState;
                        bool canStart = _attackCommand.CanStart;
                        bool closeEnough = _attackCommand.IsUnitEnoughClose;
                        float actualDistance = Vector3.Distance(_actor.Position,
                            _target.Position);
                        string placement = "distance-" + distance.ToString("R") +
                            "-direction-" + directionIndex;
                        attempts.Add(placement + "@" +
                            candidate.ToString("R") + ":actual=" +
                            actualDistance.ToString("R") + ":targetInState=" +
                            targetInState + ":canStart=" + canStart +
                            ":closeEnough=" + closeEnough);
                        if (!targetInState || !canStart || !closeEnough) continue;
                        _commandTargetInState = targetInState;
                        _commandCanStart = canStart;
                        _commandCloseEnough = closeEnough;
                        _commandNeedLoS = _attackCommand.NeedLoS;
                        _commandApproachRadius = _attackCommand.ApproachRadius;
                        _commandTargetDistance = actualDistance;
                        _commandTargetPlacement = placement;
                        _diagnostics.Add(value.Variant +
                            ":attackStartReady=targetInState:" +
                            _commandTargetInState + "/canStart:" +
                            _commandCanStart + "/closeEnough:" +
                            _commandCloseEnough + "/needLoS:" +
                            _commandNeedLoS + "/approachRadius:" +
                            _commandApproachRadius.ToString("R") +
                            "/targetDistance:" +
                            _commandTargetDistance.ToString("R") +
                            "/placement:" + _commandTargetPlacement +
                            "/attempts:" + _commandTargetAttempts);
                        return;
                    }
                throw new InvalidOperationException(value.Variant +
                    " had no navmesh-backed target position satisfying the " +
                    "native UnitAttack start contract. approachRadius=" +
                    _attackCommand.ApproachRadius.ToString("R") +
                    ";needLoS=" + _attackCommand.NeedLoS + ";attempts=" +
                    string.Join("|", attempts.ToArray()) + ".");
            }

            private void PollAttackSequence()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "sample-native-attack-" + value.Variant;
                Game.Instance.EntityCreator.Tick();
                _target.Commands.InterruptAll(true);
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                if (_attackCommand.IsRunning &&
                    _attackCommand.Animation != null &&
                    _attackCommand.Animation.IsActed &&
                    _attackCommand.Result == UnitCommand.ResultType.None)
                {
                    _attackCommand.Tick();
                    _explicitCommandTicks++;
                }
                _attackUpdates++;
                _commandInstalled = _commandInstalled ||
                    _actor.Commands.Contains(_attackCommand);
                _commandRunningObserved = _commandRunningObserved ||
                    _attackCommand.IsRunning;
                _animationObserved = _animationObserved ||
                    _attackCommand.Animation != null;
                _animationActedObserved = _animationActedObserved ||
                    (_attackCommand.Animation != null &&
                    _attackCommand.Animation.IsActed);

                if (_captureScheduleIndex < AttackCaptureUpdates.Length &&
                    _attackUpdates >=
                        AttackCaptureUpdates[_captureScheduleIndex])
                {
                    WeaponVisualParameters visual = value.Item.VisualParameters;
                    string role;
                    Transform model = ResolveActivePresentation(_actor, visual,
                        "attack", out role);
                    if (!Renderable(model))
                        throw new InvalidOperationException(value.Variant +
                            " lost its renderable held model during attack update " +
                            _attackUpdates + ".");
                    string state = "attack-update-" +
                        _attackUpdates.ToString("000");
                    CaptureMotionRecord(value, model, visual, role, state,
                        _attackUpdates,
                        _easternMotion ?
                            "fixed live UnitAttack sword-animation sample; visual review determines slash-plane acceptance" :
                            "fixed live UnitAttack animation sample; exact fire frame is established only by paired discharge counters");
                    _captureScheduleIndex++;
                }

                if (_captureScheduleIndex < AttackCaptureUpdates.Length) return;
                bool attackObserved = IsFirearm(value)
                    ? FirearmDischargeRuntimeDiagnostics.Fired -
                        _firedBefore >= 1
                    : _commandRunningObserved && _animationObserved;
                if (!attackObserved &&
                    _attackUpdates < MaximumSettleUpdates) return;
                RecordOutcome(value);
                string removalRole;
                _removedPresentation = ResolveActivePresentation(_actor,
                    value.Item.VisualParameters, "attack", out removalRole);
                _diagnostics.Add(value.Variant + ":removalRole=" +
                    removalRole);
                _actor.Commands.InterruptAll(true);
                if (_actor.CombatState.IsInCombat)
                    _actor.CombatState.LeaveCombat();
                RemoveEquipped(_actor, ref _equipped,
                    ref _equippedFirearmStateSet);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _phase = 4;
                _settleUpdates = 0;
            }

            private void CaptureMotionRecord(EvidenceCase value,
                Transform model, WeaponVisualParameters visual, string role,
                string state, int update, string claimBoundary)
            {
                _stage = "capture-" + state + "-" + value.Variant;
                string stem = state + "-default-medium-" +
                    SafeFileName(value.Variant);
                CaptureSummary capture = CaptureContactSheet(_actor, model,
                    _fixtureBodyRenderers, Path.Combine(
                        _request.EvidenceDirectory, stem + ".png"));
                JObject record = Describe(value, _actor, model, visual,
                    _fixtureBodyRenderers, capture, stem + ".png", state, role);
                if (_spearMotion)
                {
                    string endpointSource;
                    Vector3 physicalTip;
                    Vector3 physicalButt;
                    if (!TryResolveSpearPhysicalEndpoints(value, model,
                            out endpointSource, out physicalTip,
                            out physicalButt))
                        throw new InvalidOperationException(value.Variant +
                            " lacks mesh-grounded physical spear endpoints.");
                    Vector3 targetDirection = _target.Position -
                        _actor.Position;
                    targetDirection.y = 0f;
                    if (targetDirection.sqrMagnitude < 0.01f)
                        throw new InvalidOperationException(value.Variant +
                            " has a degenerate actor-to-target direction.");
                    targetDirection.Normalize();
                    float tipProjection = Vector3.Dot(physicalTip -
                        _actor.Position, targetDirection);
                    float buttProjection = Vector3.Dot(physicalButt -
                        _actor.Position, targetDirection);
                    record["physicalEndpointSource"] = endpointSource;
                    record["physicalTipWorldPosition"] =
                        physicalTip.ToString("R");
                    record["physicalButtWorldPosition"] =
                        physicalButt.ToString("R");
                    record["physicalLengthMeters"] = Vector3.Distance(
                        physicalTip, physicalButt);
                    record["physicalTipTargetProjectionMeters"] =
                        tipProjection;
                    record["physicalButtTargetProjectionMeters"] =
                        buttProjection;
                    record["physicalTipLeadsTargetDirection"] =
                        tipProjection > buttProjection;
                    Transform grip = model.Find("Grip");
                    Transform headUp = model.Find(
                        WeaponPresentationFrameContract.HeadUpMarker);
                    record["headFaceNormalWorld"] = grip == null ||
                        headUp == null ? "<native-control-unresolved>" :
                        (headUp.position - grip.position).normalized
                            .ToString("R");
                }
                else if (_easternMotion)
                {
                    string bladeFrameSource;
                    Vector3 physicalTip;
                    Vector3 physicalButt;
                    Vector3 bladeForward;
                    Vector3 bladeNormal;
                    Vector3 cuttingEdge;
                    if (!TryResolveEasternBladeFrame(value, model,
                            out bladeFrameSource, out physicalTip,
                            out physicalButt, out bladeForward,
                            out bladeNormal, out cuttingEdge))
                        throw new InvalidOperationException(value.Variant +
                            " lacks a mesh-grounded Eastern blade frame.");
                    Vector3 physicalForward = physicalTip - physicalButt;
                    Vector3 frameRight = Vector3.Cross(bladeNormal,
                        bladeForward).normalized;
                    record["physicalBladeFrameSource"] = bladeFrameSource;
                    record["physicalBladeTipWorldPosition"] =
                        physicalTip.ToString("R");
                    record["physicalBladeButtWorldPosition"] =
                        physicalButt.ToString("R");
                    record["physicalBladeLengthMeters"] =
                        physicalForward.magnitude;
                    record["bladeForwardWorld"] =
                        bladeForward.ToString("R");
                    record["bladeNormalWorld"] =
                        bladeNormal.ToString("R");
                    record["cuttingEdgeWorld"] =
                        cuttingEdge.ToString("R");
                    record["physicalTipAheadAlongBladeForward"] =
                        Vector3.Dot(physicalForward, bladeForward);
                    record["bladeNormalForwardAbsDot"] = Mathf.Abs(
                        Vector3.Dot(bladeNormal, bladeForward));
                    record["cuttingEdgeForwardAbsDot"] = Mathf.Abs(
                        Vector3.Dot(cuttingEdge, bladeForward));
                    record["cuttingEdgeBladeNormalAbsDot"] = Mathf.Abs(
                        Vector3.Dot(cuttingEdge, bladeNormal));
                    record["cuttingEdgePolarityDot"] = Vector3.Dot(
                        cuttingEdge, -frameRight);
                }
                record["claimBoundary"] = claimBoundary;
                record["motionUpdate"] = update;
                record["actorWorldPosition"] = _actor.Position.ToString("R");
                record["actorWorldForward"] =
                    _actor.View.transform.forward.ToString("R");
                record["targetWorldPosition"] = _target.Position.ToString("R");
                record["targetDistance"] = Vector3.Distance(_actor.Position,
                    _target.Position);
                record["targetHitPoints"] = _target.HPLeft;
                record["targetDamage"] = _target.Descriptor.Damage;
                record["targetInState"] = _target.IsInState;
                record["unitInCombat"] = _actor.CombatState.IsInCombat;
                record["commandType"] = _attackCommand == null ? "<none>" :
                    _attackCommand.GetType().FullName;
                record["commandCanStart"] = _attackCommand != null &&
                    _attackCommand.CanStart;
                record["commandIsUnitEnoughClose"] = _attackCommand != null &&
                    _attackCommand.IsUnitEnoughClose;
                record["commandTargetInState"] = _attackCommand != null &&
                    _target != null && _target.IsInState;
                record["commandApproachRadius"] = _attackCommand == null ?
                    0f : _attackCommand.ApproachRadius;
                record["commandNeedLoS"] = _attackCommand != null &&
                    _attackCommand.NeedLoS;
                record["commandTargetPlacement"] = _commandTargetPlacement;
                record["commandTargetAttempts"] = _commandTargetAttempts;
                record["commandExplicitTickCount"] = _explicitCommandTicks;
                record["commandIsSingleAttack"] = _attackCommand != null &&
                    _attackCommand.IsSingleAttack;
                record["commandIsStarted"] = _attackCommand != null &&
                    _attackCommand.IsStarted;
                record["commandIsRunning"] = _attackCommand != null &&
                    _attackCommand.IsRunning;
                record["commandIsFinished"] = _attackCommand != null &&
                    _attackCommand.IsFinished;
                record["commandResult"] = _attackCommand == null ? "<none>" :
                    _attackCommand.Result.ToString();
                record["commandAnimation"] = _attackCommand == null ||
                    _attackCommand.Animation == null ? "<none>" :
                    _attackCommand.Animation.GetType().FullName;
                record["commandAnimationActed"] = _attackCommand != null &&
                    _attackCommand.Animation != null &&
                    _attackCommand.Animation.IsActed;
                record["activeCommandTypes"] = new JArray(_actor.Commands.Raw
                    .Where(command => command != null).Select(command =>
                        command.GetType().FullName).ToArray());
                record["firearmDischargeObserved"] =
                    FirearmDischargeRuntimeDiagnostics.Observed;
                record["firearmDischargeFired"] =
                    FirearmDischargeRuntimeDiagnostics.Fired;
                record["firearmDischargeFaults"] =
                    FirearmDischargeRuntimeDiagnostics.Faults;
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".json");
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(capture.PngPath);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _viewCount += 4;
                _diagnostics.Add(value.Variant + ":state=" + state +
                    ";update=" + update + ";running=" +
                    (string)record["commandIsRunning"] + ";finished=" +
                    (string)record["commandIsFinished"] + ";fired=" +
                    FirearmDischargeRuntimeDiagnostics.Fired + ";png=" +
                    Path.GetFileName(capture.PngPath) + ";sha256=" +
                    capture.Sha256);
            }

            private void RecordOutcome(EvidenceCase value)
            {
                bool firearm = IsFirearm(value);
                int loadedRounds = firearm ? FirearmRuntimeState.Service
                    .GetOrCreate(_equipped).Repository.State.LoadedRounds : -1;
                _outcomes.Add(new MotionOutcome
                {
                    Variant = value.Variant,
                    Firearm = firearm,
                    CommandInstalled = _commandInstalled,
                    CommandCanStart = _commandCanStart,
                    CommandCloseEnough = _commandCloseEnough,
                    CommandTargetInState = _commandTargetInState,
                    CommandStarted = _commandStarted,
                    CommandRunningObserved = _commandRunningObserved,
                    AnimationObserved = _animationObserved,
                    AnimationActedObserved = _animationActedObserved,
                    CommandFinishedBeforeInterrupt = _attackCommand.IsFinished,
                    CommandNeedLoS = _commandNeedLoS,
                    CommandApproachRadius = _commandApproachRadius,
                    CommandTargetDistance = _commandTargetDistance,
                    CommandTargetAttempts = _commandTargetAttempts,
                    CommandTargetPlacement = _commandTargetPlacement,
                    ExplicitCommandTicks = _explicitCommandTicks,
                    FiredDelta = FirearmDischargeRuntimeDiagnostics.Fired -
                        _firedBefore,
                    FaultDelta = FirearmDischargeRuntimeDiagnostics.Faults -
                        _faultsBefore,
                    LoadedRoundsAfter = loadedRounds
                });
                _diagnostics.Add(value.Variant + ":outcome=installed=" +
                    _commandInstalled + ";canStart=" + _commandCanStart +
                    ";closeEnough=" + _commandCloseEnough +
                    ";targetInState=" + _commandTargetInState +
                    ";started=" + _commandStarted + ";running=" +
                    _commandRunningObserved + ";animation=" +
                    _animationObserved + ";acted=" +
                    _animationActedObserved + ";finished=" +
                    _attackCommand.IsFinished + ";firedDelta=" +
                    (FirearmDischargeRuntimeDiagnostics.Fired - _firedBefore) +
                    ";faultDelta=" +
                    (FirearmDischargeRuntimeDiagnostics.Faults - _faultsBefore) +
                    ";roundsAfter=" + loadedRounds + ";explicitCommandTicks=" +
                    _explicitCommandTicks);
            }

            private void PollRemoval()
            {
                _stage = "settle-motion-removal-" +
                    _cases[_caseIndex].Variant;
                Game.Instance.EntityCreator.Tick();
                _target.Commands.InterruptAll(true);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
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
                        "The prior attack presentation remained active after " +
                        _settleUpdates + " updates.");
                }
                _removedPresentation = null;
                _attackCommand = null;
                _commandInstalled = false;
                _commandCanStart = false;
                _commandCloseEnough = false;
                _commandTargetInState = false;
                _commandStarted = false;
                _commandRunningObserved = false;
                _animationObserved = false;
                _animationActedObserved = false;
                _commandNeedLoS = false;
                _commandApproachRadius = 0f;
                _commandTargetDistance = 0f;
                _commandTargetAttempts = 0;
                _commandTargetPlacement = "<not-prepared>";
                _explicitCommandTicks = 0;
                _caseIndex++;
                if (_caseIndex < _cases.Length)
                {
                    _phase = 1;
                    return;
                }
                WriteMotionIndex();
                _indexWritten = true;
                BeginCleanup();
            }

            private void WriteMotionIndex()
            {
                _stage = "write-motion-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var outcomes = new JArray(_outcomes.Select(value =>
                    new JObject
                    {
                        { "variant", value.Variant },
                        { "firearm", value.Firearm },
                        { "commandInstalled", value.CommandInstalled },
                        { "commandCanStart", value.CommandCanStart },
                        { "commandCloseEnough", value.CommandCloseEnough },
                        { "commandTargetInState",
                            value.CommandTargetInState },
                        { "commandStarted", value.CommandStarted },
                        { "commandRunningObserved",
                            value.CommandRunningObserved },
                        { "animationObserved", value.AnimationObserved },
                        { "animationActedObserved",
                            value.AnimationActedObserved },
                        { "commandFinishedBeforeInterrupt",
                            value.CommandFinishedBeforeInterrupt },
                        { "commandNeedLoS", value.CommandNeedLoS },
                        { "commandApproachRadius",
                            value.CommandApproachRadius },
                        { "commandTargetDistance",
                            value.CommandTargetDistance },
                        { "commandTargetAttempts",
                            value.CommandTargetAttempts },
                        { "commandTargetPlacement",
                            value.CommandTargetPlacement },
                        { "explicitCommandTicks",
                            value.ExplicitCommandTicks },
                        { "firedDelta", value.FiredDelta },
                        { "faultDelta", value.FaultDelta },
                        { "loadedRoundsAfter", value.LoadedRoundsAfter }
                    }).ToArray());
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "fixture",
                        "live disposable default Medium combat pair" },
                    { "motionFamily", _spearMotion ?
                        "elven-branched-spear" : _easternMotion ?
                        "eastern-blade" : "long-gun" },
                    { "productionVariantCount", _easternMotion ? 12 : 3 },
                    { "nativeControlCount", _easternMotion ? 3 : 1 },
                    { "attackCaptureUpdates",
                        new JArray(AttackCaptureUpdates) },
                    { "views", new JArray("front", "right-side", "rear",
                        "front-right-three-quarter") },
                    { "loadedModVersion", _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "outcomes", outcomes },
                    { "records", _records }
                };
                string indexPath = Path.Combine(_request.EvidenceDirectory,
                    _spearMotion ?
                        "weapon-presentation-branched-spear-motion-index.json" :
                    _easternMotion ?
                        "weapon-presentation-eastern-motion-index.json" :
                        "weapon-presentation-long-gun-motion-index.json");
                WriteJsonAtomic(indexPath, index);
                _evidenceFiles.Add(indexPath);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "motion-request-cleanup";
                if (_actor != null)
                {
                    _actor.Commands.InterruptAll(true);
                    RemoveEquipped(_actor, ref _equipped,
                        ref _equippedFirearmStateSet);
                    if (_actor.CombatState != null &&
                        _actor.CombatState.IsInCombat)
                        _actor.CombatState.LeaveCombat();
                    _actor.Descriptor.State.Immortality.ReleaseAll();
                }
                if (_target != null)
                {
                    _target.Commands.InterruptAll(true);
                    if (_target.CombatState != null &&
                        _target.CombatState.IsInCombat)
                        _target.CombatState.LeaveCombat();
                    _target.Descriptor.State.Immortality.ReleaseAll();
                }
                if (_target != null && ContainsReference(_allUnits, _target))
                    Game.Instance.State.Units.All.Remove(_target);
                if (_actor != null && ContainsReference(_allUnits, _actor))
                    Game.Instance.State.Units.All.Remove(_actor);
                if (_target != null) _target.Dispose();
                if (_actor != null) _actor.Dispose();
                if (_hostileBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_hostileBlueprint);
                if (_actorBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                _hostileBlueprint = null;
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
                    (_actor == null || !ContainsReference(_allUnits, _actor)) &&
                    (_target == null || !ContainsReference(_allUnits, _target));
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates) return;
                Finish(cleaned);
            }

            private void Finish(bool cleaned)
            {
                int expectedRecords = _motionVariants.Length *
                    (AttackCaptureUpdates.Length + 1);
                Add(_assertions,
                    _spearMotion ?
                        "weapon-presentation-branched-spear-motion-matrix" :
                    _easternMotion ?
                        "weapon-presentation-eastern-motion-matrix" :
                        "weapon-presentation-long-gun-motion-matrix",
                    (_easternMotion ? "twelve production Eastern variants " +
                        "and native Scimitar/Bastard Sword/Greatsword" :
                        "three production " + (_spearMotion ?
                            "branched spears and native Longspear" :
                            "long guns and native Heavy Crossbow")) + " in " +
                        "combat-ready plus nine fixed attack samples",
                    "records=" + _records.Count + ";variants=" +
                        _records.OfType<JObject>().Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count(),
                    _records.Count == expectedRecords &&
                        _records.OfType<JObject>().Select(value =>
                            (string)value["variant"]).Distinct(
                                StringComparer.Ordinal).Count() ==
                                    _motionVariants.Length,
                    "real live held model at updates 1/4/8/12/18/24/36/60/96");
                Add(_assertions,
                    "weapon-presentation-native-attack-command",
                    "every case installs a native UnitAttack and exposes its " +
                        "acted animation while the command is running",
                    string.Join(";", _outcomes.Select(value => value.Variant +
                        "=" + value.CommandInstalled + "/" +
                        value.CommandCanStart + "/" +
                        value.CommandCloseEnough + "/" +
                        value.CommandTargetInState + "/" +
                        value.CommandStarted + "/" +
                        value.CommandRunningObserved + "/" +
                        value.AnimationObserved + "/" +
                        value.AnimationActedObserved).ToArray()),
                    _outcomes.Count == _motionVariants.Length &&
                        _outcomes.All(value =>
                        value.CommandInstalled && value.CommandCanStart &&
                        value.CommandCloseEnough && value.CommandTargetInState &&
                        value.CommandStarted && value.CommandRunningObserved &&
                        value.AnimationObserved &&
                        value.AnimationActedObserved),
                    "UnitAttack.CreateAttackCommand, navmesh-backed native " +
                        "CanStart/IsUnitEnoughClose contract, UnitCommands.Run, " +
                        "and live command/acted-animation state");
                if (_spearMotion)
                {
                    JObject[] endpointRecords = _records.OfType<JObject>()
                        .ToArray();
                    JObject[] actedEndpointRecords = endpointRecords.Where(
                            record => (bool)record["commandAnimationActed"])
                        .ToArray();
                    int actedEndpointVariants = actedEndpointRecords
                        .Select(record => (string)record["variant"])
                        .Distinct(StringComparer.Ordinal).Count();
                    int tipLeadingRecords = endpointRecords.Count(record =>
                        (bool)record["physicalTipLeadsTargetDirection"]);
                    int actedTipLeadingRecords = actedEndpointRecords.Count(
                        record => (bool)record[
                            "physicalTipLeadsTargetDirection"]);
                    Add(_assertions,
                        "weapon-presentation-spear-physical-endpoint-evidence",
                        "all four cases expose mesh-grounded physical endpoints " +
                            "and every acted-animation sample leads with the tip",
                        "records=" + endpointRecords.Length +
                            ";actedEndpointVariants=" +
                            actedEndpointVariants + ";actedEndpointSamples=" +
                            actedEndpointRecords.Length +
                            ";actedTipLeadingRecords=" +
                            actedTipLeadingRecords + ";allTipLeadingRecords=" +
                            tipLeadingRecords,
                        endpointRecords.Length == expectedRecords &&
                            endpointRecords.All(record =>
                                (float)record["physicalLengthMeters"] > 2f &&
                                !string.IsNullOrEmpty((string)record[
                                    "physicalEndpointSource"])) &&
                            actedEndpointVariants == 4 &&
                            actedEndpointRecords.Length > 0 &&
                            actedTipLeadingRecords ==
                                actedEndpointRecords.Length,
                        "authored renderer-bound Tip/Butt plus native " +
                            "TH_LongspearKnight1 renderer-positive-Y head");
                }
                else if (_easternMotion)
                {
                    JObject[] bladeRecords = _records.OfType<JObject>()
                        .ToArray();
                    JObject[] actedBladeRecords = bladeRecords.Where(record =>
                            (bool)record["commandAnimationActed"])
                        .ToArray();
                    int actedVariants = actedBladeRecords.Select(record =>
                            (string)record["variant"])
                        .Distinct(StringComparer.Ordinal).Count();
                    Add(_assertions,
                        "weapon-presentation-eastern-physical-blade-frame",
                        "all fifteen cases expose a nondegenerate physical blade frame with orthogonal normal, canonical cutting-edge polarity, and every variant reaches an acted animation",
                        "records=" + bladeRecords.Length +
                            ";actedVariants=" + actedVariants +
                            ";actedSamples=" + actedBladeRecords.Length,
                        bladeRecords.Length == expectedRecords &&
                            bladeRecords.All(record =>
                                (float)record["physicalBladeLengthMeters"] >
                                    0.5f &&
                                (float)record[
                                    "physicalTipAheadAlongBladeForward"] >
                                    0.5f &&
                                (float)record[
                                    "bladeNormalForwardAbsDot"] < 0.05f &&
                                (float)record[
                                    "cuttingEdgeForwardAbsDot"] < 0.05f &&
                                (float)record[
                                    "cuttingEdgeBladeNormalAbsDot"] < 0.05f &&
                                (float)record["cuttingEdgePolarityDot"] >
                                    0.99f &&
                                !string.IsNullOrEmpty((string)record[
                                    "physicalBladeFrameSource"])) &&
                            actedVariants == _motionVariants.Length &&
                            actedBladeRecords.Length > 0,
                        "authored Tip/Butt/WeaponForward/BladeNormal/CuttingEdge markers plus native renderer-local +Y/+X/-Z donor axes");
                }
                else
                {
                    MotionOutcome[] firearms = _outcomes.Where(value =>
                        value.Firearm).ToArray();
                    Add(_assertions,
                        "weapon-presentation-firearm-discharge-nonregression",
                        "each loaded production long gun fires exactly once, consumes " +
                            "its round, and records no discharge fault",
                        string.Join(";", firearms.Select(value => value.Variant +
                            "=fired:" + value.FiredDelta + "/fault:" +
                            value.FaultDelta + "/rounds:" +
                            value.LoadedRoundsAfter).ToArray()),
                        firearms.Length == 3 && firearms.All(value =>
                            value.FiredDelta == 1 && value.FaultDelta == 0 &&
                            value.LoadedRoundsAfter == 0),
                        "FirearmDischargeRuntimeDiagnostics plus exact per-item runtime state");
                }
                int zeroPixelSheets = _records.OfType<JObject>().Count(value =>
                    (int)value["meaningfulPixels"] <= 0);
                Add(_assertions,
                    _spearMotion ?
                        "weapon-presentation-spear-motion-contact-sheets" :
                    _easternMotion ?
                        "weapon-presentation-eastern-motion-contact-sheets" :
                        "weapon-presentation-motion-contact-sheets",
                    expectedRecords + " PNG/JSON pairs and " +
                        (expectedRecords * 4) + " labelled views",
                    "captures=" + _captured + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count +
                        ";zeroPixelSheets=" + zeroPixelSheets,
                    _captured == expectedRecords &&
                        _viewCount == expectedRecords * 4 && _indexWritten &&
                        _evidenceFiles.Count == expectedRecords * 2 + 1 &&
                        _evidenceFiles.All(File.Exists) && zeroPixelSheets == 0,
                    "front/right-side/rear/front-right-three-quarter live combat contact sheets");
                Add(_assertions, "weapon-presentation-motion-request-cleanup",
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";settleUpdates=" +
                        _settleUpdates, cleaned,
                    "request-local items, combat pair, blueprint clones, camera, light, and textures");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Motion evidence is limited to combat-ready and " +
                    "fixed attack-sequence samples on the default Medium actor. " +
                    "It does not establish locomotion, transitions, sex-specific, " +
                    "Small, or Enlarged acceptance" +
                    (_spearMotion || _easternMotion ? "." :
                        ", or reload acceptance."));
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
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }
        }

        private sealed class TransitionMotionOutcome
        {
            internal string Variant;
            internal bool EquipMatchReturned;
            internal bool EquipAnimationObserved;
            internal int EquipClipCount;
            internal bool MovementCommandAccepted;
            internal bool MovementAgentMovingObserved;
            internal bool MovementVelocityObserved;
            internal int LocomotionClipCount;
            internal float MovementDistanceMeters;
            internal float TurnDegrees;
            internal bool UnequipMatchReturned;
            internal bool UnequipAnimationObserved;
            internal int UnequipClipCount;
        }

        /// <summary>
        /// Request-gated evidence for native equip/unequip animation, navmesh
        /// locomotion, and a body-relative turn. This remains separate from the
        /// static and attack fixtures so their original claim boundaries stay
        /// exact. It never calls a save API and removes every request-local unit
        /// and item before returning a result.
        /// </summary>
        internal sealed class TransitionMotionSession
        {
            private const int MaximumSettleUpdates = 360;
            private const int StableStoredUpdates = 20;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _evidenceFiles = new List<string>();
            private readonly List<TransitionMotionOutcome> _outcomes =
                new List<TransitionMotionOutcome>();
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
            private UnitMoveTo _moveCommand;
            private Transform _removedPresentation;
            private Vector3 _movementStart;
            private Vector3 _movementDestination;
            private Vector3 _turnStartForward;
            private int _caseIndex;
            private int _phase;
            private int _settleUpdates;
            private int _captured;
            private int _viewCount;
            private bool _equipCaptured;
            private bool _unequipCaptured;
            private bool _equipAnimationObserved;
            private bool _unequipAnimationObserved;
            private bool _movementCommandAccepted;
            private bool _movementAgentMovingObserved;
            private bool _movementVelocityObserved;
            private bool _turnRequested;
            private bool _equipMatchReturned;
            private bool _unequipMatchReturned;
            private int _equipClipCount;
            private int _unequipClipCount;
            private int _locomotionClipCount;
            private float _movementDistanceMeters;
            private float _turnDegrees;
            private uint _movementStartArea;
            private uint _movementDestinationArea;
            private uint _movementGraphIndex;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private string _stage = "resolve-working-save-anchor";
            private string _exceptionSummary = string.Empty;

            internal TransitionMotionSession(ModContext context,
                RuntimeTestRequest request)
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
                        PollStoredAndStartEquipTransition();
                        return;
                    }
                    if (_phase == 2)
                    {
                        PollEquipTransition();
                        return;
                    }
                    if (_phase == 3)
                    {
                        PollMovement();
                        return;
                    }
                    if (_phase == 4)
                    {
                        PollTurnAndStartUnequipTransition();
                        return;
                    }
                    if (_phase == 5)
                    {
                        PollUnequipTransition();
                        return;
                    }
                    PollRemoval();
                }
                catch (Exception exception)
                {
                    _exceptionSummary = "stage=" + _stage + ";" + exception;
                    Add(_assertions,
                        "weapon-presentation-transition-motion-exception",
                        "no exception", _exceptionSummary, false,
                        "guarded request-local transition/movement fixture");
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

                _stage = "spawn-disposable-transition-actor";
                _actorBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _actorBlueprint.name =
                    "KMG_Runtime_Weapon_Presentation_Transition_Actor";
                _actorBlueprint.IsCheater = true;
                Vector3 position = NearestNavigable(areaAnchor.Position +
                    new Vector3(-4f, 0f, 4f));
                _actor = Game.Instance.EntityCreator.SpawnUnit(_actorBlueprint,
                    position, Quaternion.identity, areaAnchor.HoldingState);
                Game.Instance.EntityCreator.Tick();
                if (_actor == null || _actor.View == null ||
                    _actor.View.Data == null ||
                    _actor.View.HandsEquipment == null ||
                    _actor.View.MovementAgent == null)
                    throw new InvalidOperationException(
                        "Native spawning did not attach the disposable transition view.");

                _actor.Descriptor.State.Immortality.Retain();
                _actor.Commands.InterruptAll(true);
                if (_actor.CombatState.IsInCombat)
                    _actor.CombatState.LeaveCombat();
                ClearHand(_actor, true);
                ClearHand(_actor, false);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _cases = BuildCases();
                if (_cases.Length != 28 ||
                    !_cases.Take(ProductionVariants.Length).Select(value =>
                        value.Variant).SequenceEqual(ProductionVariants) ||
                    !_cases.Skip(ProductionVariants.Length).Select(value =>
                        value.Variant).SequenceEqual(NativeControls.Select(
                            value => "Native." + value.Label)))
                    throw new InvalidOperationException(
                        "The transition matrix is not the exact 22 production " +
                        "variants plus six native donor controls.");
            }

            private void PollStoredAndStartEquipTransition()
            {
                if (_equipped == null)
                {
                    if (_fixtureBodyRenderers.Length == 0)
                    {
                        _stage = "settle-empty-handed-transition-body-renderers";
                        TickRuntime();
                        _fixtureBodyRenderers = _actor.View
                            .GetComponentsInChildren<Renderer>(true)
                            .Where(renderer => renderer != null &&
                                renderer.enabled &&
                                renderer.gameObject.activeInHierarchy)
                            .ToArray();
                        if (_fixtureBodyRenderers.Length == 0)
                        {
                            _settleUpdates++;
                            if (_settleUpdates < MaximumSettleUpdates) return;
                            throw new InvalidOperationException(
                                "The transition actor has no active body renderers.");
                        }
                    }

                    EvidenceCase value = _cases[_caseIndex];
                    _stage = "equip-stored-transition-case-" + value.Variant;
                    _equipped = new ItemEntityWeapon(value.Item);
                    _actor.Body.PrimaryHand.InsertItem(_equipped);
                    if (!ReferenceEquals(_actor.Body.PrimaryHand.MaybeWeapon,
                            _equipped))
                        throw new InvalidOperationException(value.Variant +
                            " did not remain in the primary hand.");
                    if (IsFirearm(value))
                    {
                        FirearmRuntimeState.Service.Set(_equipped,
                            new FirearmState(FirearmState.CurrentSchemaVersion,
                                1, FirearmStateTokenCatalog.DiagnosticLeadBall,
                                FirearmCondition.Normal));
                        _equippedFirearmStateSet = true;
                    }
                    _actor.View.HandsEquipment.UpdateAll();
                    _actor.View.HandsEquipment.ForceSwitch(false);
                    _settleUpdates = 0;
                    return;
                }

                EvidenceCase current = _cases[_caseIndex];
                _stage = "settle-stored-before-equip-transition-" +
                    current.Variant;
                TickRuntime();
                WeaponVisualParameters visual = current.Item.VisualParameters;
                string role;
                Transform stored = ResolveActivePresentation(_actor, visual,
                    "stored", out role);
                _settleUpdates++;
                if (!Renderable(stored) ||
                    _actor.View.HandsEquipment.InCombat ||
                    _settleUpdates < StableStoredUpdates)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(current.Variant +
                        " did not reach a stable stored presentation before " +
                        "the equip transition. renderable=" +
                        Renderable(stored) + ";handsInCombat=" +
                        _actor.View.HandsEquipment.InCombat + ";role=" + role +
                        ".");
                }

                var equipAction = _actor.View.AnimationManager == null ? null :
                    _actor.View.AnimationManager.GetAction(
                        UnitAnimationType.MainHandEquip);
                _equipClipCount = equipAction == null ? 0 :
                    equipAction.Clips.Count(clip => clip != null);
                // UnitViewHandsEquipment owns its presentation transition via
                // m_ShoudBeInCombat. Joining UnitCombatState would also set
                // Game.Player.IsInCombat and incorrectly gate request-local
                // locomotion in turn-based mode.
                _actor.View.HandsEquipment.OnCombatStateChanged(true);
                _equipMatchReturned = _actor.View.HandsEquipment
                    .MatchWithCurrentCombatState();
                _settleUpdates = 0;
                _phase = 2;
            }

            private void PollEquipTransition()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "equip-transition-" + value.Variant;
                TickRuntime();
                _settleUpdates++;
                bool animating = CombatStateTransitionAnimating(_actor);
                _equipAnimationObserved |= animating;
                WeaponVisualParameters visual = value.Item.VisualParameters;
                string role;
                Transform presentation = ResolveActivePresentation(_actor,
                    visual, "stored", out role);
                if (animating && !_equipCaptured && Renderable(presentation))
                {
                    CaptureTransitionRecord(value, presentation, visual, role,
                        "equip-transition", true,
                        "native MainHandEquip coroutine while changing from " +
                        "stored to held presentation");
                    _equipCaptured = true;
                }

                string heldRole;
                Transform held = ResolveActivePresentation(_actor, visual,
                    "held-idle", out heldRole);
                bool complete = _equipAnimationObserved && _equipCaptured &&
                    !animating && _actor.View.HandsEquipment.InCombat &&
                    Renderable(held);
                if (!complete)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Variant +
                        " did not expose a complete native equip transition. " +
                        "animationObserved=" + _equipAnimationObserved +
                        ";captured=" + _equipCaptured + ";animating=" +
                        animating + ";handsInCombat=" +
                        _actor.View.HandsEquipment.InCombat + ";heldRole=" +
                        heldRole + ".");
                }
                StartMovement(value);
                _settleUpdates = 0;
                _phase = 3;
            }

            private void StartMovement(EvidenceCase value)
            {
                _stage = "start-native-movement-" + value.Variant;
                if (_actor.CombatState.IsInCombat ||
                    Game.Instance.Player.IsInCombat ||
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat())
                    throw new InvalidOperationException(value.Variant +
                        " cannot start request-local locomotion because an " +
                        "equipment-only transition polluted combat state.");
                Pathfinding.NNInfo start = AstarPath.active.GetNearest(
                    _actor.Position);
                if (start.node == null || !start.node.Walkable)
                    throw new InvalidOperationException(value.Variant +
                        " has no walkable movement start node.");
                _movementStart = start.clampedPosition;
                SetUnitPosition(_actor, _movementStart);
                _movementStartArea = start.node.Area;
                _movementGraphIndex = start.node.GraphIndex;
                Vector3[] offsets =
                {
                    new Vector3(2.5f, 0f, 2.5f),
                    new Vector3(-2.5f, 0f, 2.5f),
                    new Vector3(2.5f, 0f, -2.5f),
                    new Vector3(-2.5f, 0f, -2.5f)
                };
                Pathfinding.NNInfo[] candidates = offsets.Select(offset =>
                        AstarPath.active.GetNearest(_movementStart + offset))
                    .Where(candidate => candidate.node != null &&
                        candidate.node.Walkable &&
                        candidate.node.Area == _movementStartArea &&
                        candidate.node.GraphIndex == _movementGraphIndex)
                    .OrderByDescending(candidate => Vector3.Distance(
                        candidate.clampedPosition, _movementStart)).ToArray();
                if (candidates.Length == 0)
                    throw new InvalidOperationException(value.Variant +
                        " has no same-area walkable movement destination.");
                Pathfinding.NNInfo destination = candidates[0];
                _movementDestination = destination.clampedPosition;
                _movementDestinationArea = destination.node.Area;
                if (Vector3.Distance(_movementDestination, _movementStart) < 1f)
                    throw new InvalidOperationException(value.Variant +
                        " has no request-local navigable movement span.");
                var locomotion = _actor.View.AnimationManager == null ? null :
                    _actor.View.AnimationManager.GetAction(
                        UnitAnimationType.LocoMotion);
                _locomotionClipCount = locomotion == null ? 0 :
                    locomotion.Clips.Count(clip => clip != null);
                _moveCommand = new UnitMoveTo(_movementDestination);
                _actor.Commands.Run(_moveCommand);
                _movementCommandAccepted =
                    _actor.Commands.Contains(_moveCommand) &&
                    ReferenceEquals(_moveCommand.Executor, _actor);
                if (!_movementCommandAccepted)
                    throw new InvalidOperationException(value.Variant +
                        " native UnitMoveTo was not accepted by UnitCommands.");
                var path = new Pathfinding.ForcedPath(new List<Vector3>
                {
                    _movementStart,
                    _movementDestination
                });
                path.UserTag = "KMG weapon-presentation locomotion " +
                    value.Variant;
                _actor.View.AgentASP.ForcePath(path, 0.1f);
                if (!_actor.View.MovementAgent.WantsToMove)
                    throw new InvalidOperationException(value.Variant +
                        " native movement agent rejected the same-area path.");
            }

            private void PollMovement()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "native-movement-" + value.Variant;
                TickRuntime();
                if (!TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat() || IsActorCurrentTurn())
                    _actor.View.MovementAgent.TickMovement(
                        Game.Instance.TimeController.DeltaTime);
                _settleUpdates++;
                bool moving = _actor.View.IsMoving() ||
                    _actor.View.MovementAgent.IsReallyMoving ||
                    _actor.View.MovementAgent.WantsToMove;
                bool nonzeroVelocity = _actor.View.MovementAgent.Velocity
                    .sqrMagnitude > 0.0001f;
                _movementAgentMovingObserved |= moving;
                _movementVelocityObserved |= nonzeroVelocity;
                _movementDistanceMeters = Vector3.Distance(_movementStart,
                    _actor.Position);
                WeaponVisualParameters visual = value.Item.VisualParameters;
                string role;
                Transform held = ResolveActivePresentation(_actor, visual,
                    "held-idle", out role);
                if (_movementAgentMovingObserved &&
                    _movementVelocityObserved &&
                    _movementDistanceMeters > 0.05f && Renderable(held))
                {
                    CaptureTransitionRecord(value, held, visual, role,
                        "moving", false,
                        "live nonzero native movement-agent velocity and " +
                        "measurable displacement on a same-area two-node path " +
                        "matching the accepted UnitMoveTo target");
                    _actor.View.StopMoving();
                    _actor.Commands.InterruptAll(true);
                    _moveCommand = null;
                    _settleUpdates = 0;
                    _phase = 4;
                    return;
                }
                if (_settleUpdates < MaximumSettleUpdates) return;
                throw new InvalidOperationException(value.Variant +
                    " did not expose a live movement frame. started=" +
                    _moveCommand.IsStarted + ";accepted=" +
                    _movementCommandAccepted + ";agentMovingObserved=" +
                    _movementAgentMovingObserved + ";velocityObserved=" +
                    _movementVelocityObserved + ";distance=" +
                    _movementDistanceMeters.ToString("R") + ";destination=" +
                    _movementDestination.ToString("R") + ";wantsToMove=" +
                    _actor.View.MovementAgent.WantsToMove +
                    ";isReallyMoving=" +
                    _actor.View.MovementAgent.IsReallyMoving +
                    ";velocity=" +
                    _actor.View.MovementAgent.Velocity.ToString("R") +
                    ";startArea=" + _movementStartArea +
                    ";destinationArea=" + _movementDestinationArea +
                    ";graphIndex=" + _movementGraphIndex +
                    ";turnBased=" + TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat() + ";actorCurrentTurn=" +
                    IsActorCurrentTurn() + ";actorInCombat=" +
                    _actor.CombatState.IsInCombat + ";handsInCombat=" +
                    _actor.View.HandsEquipment.InCombat +
                    ";animationPreventsMovement=" +
                    _actor.View.AnimationManager.IsPreventingMovement +
                    ";commandsPreventMovement=" +
                    _actor.View.IsCommandsPreventMovement +
                    ";deltaTime=" + Game.Instance.TimeController.DeltaTime
                        .ToString("R") + ".");
            }

            private bool IsActorCurrentTurn()
            {
                TurnBased.Controllers.CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                return controller != null && controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, _actor);
            }

            private void PollTurnAndStartUnequipTransition()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "native-turn-" + value.Variant;
                if (!_turnRequested)
                {
                    _turnStartForward = _actor.OrientationDirection;
                    _turnStartForward.y = 0f;
                    if (_turnStartForward.sqrMagnitude < 0.5f)
                        _turnStartForward = Vector3.forward;
                    _turnStartForward.Normalize();
                    Vector3 right = new Vector3(_turnStartForward.z, 0f,
                        -_turnStartForward.x).normalized;
                    _actor.ForceLookAt(_actor.Position + right * 5f);
                    _turnRequested = true;
                    _settleUpdates = 0;
                    return;
                }

                TickRuntime();
                _settleUpdates++;
                Vector3 currentForward = _actor.OrientationDirection;
                currentForward.y = 0f;
                if (currentForward.sqrMagnitude > 0.01f)
                    currentForward.Normalize();
                _turnDegrees = Vector3.Angle(_turnStartForward,
                    currentForward);
                WeaponVisualParameters visual = value.Item.VisualParameters;
                string role;
                Transform held = ResolveActivePresentation(_actor, visual,
                    "held-idle", out role);
                if (_turnDegrees >= 60f && Renderable(held) &&
                    _settleUpdates >= 4)
                {
                    CaptureTransitionRecord(value, held, visual, role,
                        "turned-right", false,
                        "native ForceLookAt endpoint after a body-relative " +
                        "right turn; verifies the weapon follows its rig");
                    var unequipAction = _actor.View.AnimationManager == null ?
                        null : _actor.View.AnimationManager.GetAction(
                            UnitAnimationType.MainHandUnequip);
                    _unequipClipCount = unequipAction == null ? 0 :
                        unequipAction.Clips.Count(clip => clip != null);
                    _actor.View.HandsEquipment.OnCombatStateChanged(false);
                    _unequipMatchReturned = _actor.View.HandsEquipment
                        .MatchWithCurrentCombatState();
                    _settleUpdates = 0;
                    _phase = 5;
                    return;
                }
                if (_settleUpdates < MaximumSettleUpdates) return;
                throw new InvalidOperationException(value.Variant +
                    " did not reach the requested body-relative turn. degrees=" +
                    _turnDegrees.ToString("R") + ".");
            }

            private void PollUnequipTransition()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "unequip-transition-" + value.Variant;
                TickRuntime();
                _settleUpdates++;
                bool animating = CombatStateTransitionAnimating(_actor);
                _unequipAnimationObserved |= animating;
                WeaponVisualParameters visual = value.Item.VisualParameters;
                string role;
                Transform presentation = ResolveActivePresentation(_actor,
                    visual, "stored", out role);
                if (animating && !_unequipCaptured && Renderable(presentation))
                {
                    CaptureTransitionRecord(value, presentation, visual, role,
                        "unequip-transition", true,
                        "native MainHandUnequip coroutine while changing from " +
                        "held to stored presentation");
                    _unequipCaptured = true;
                }

                bool complete = _unequipAnimationObserved &&
                    _unequipCaptured && !animating &&
                    !_actor.View.HandsEquipment.InCombat &&
                    Renderable(presentation);
                if (!complete)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Variant +
                        " did not expose a complete native unequip transition. " +
                        "animationObserved=" + _unequipAnimationObserved +
                        ";captured=" + _unequipCaptured + ";animating=" +
                        animating + ";handsInCombat=" +
                        _actor.View.HandsEquipment.InCombat +
                        ";actorInCombat=" +
                        _actor.CombatState.IsInCombat +
                        ";matchReturned=" + _unequipMatchReturned +
                        ";role=" + role + ".");
                }

                _outcomes.Add(new TransitionMotionOutcome
                {
                    Variant = value.Variant,
                    EquipMatchReturned = _equipMatchReturned,
                    EquipAnimationObserved = _equipAnimationObserved,
                    EquipClipCount = _equipClipCount,
                    MovementCommandAccepted = _movementCommandAccepted,
                    MovementAgentMovingObserved =
                        _movementAgentMovingObserved,
                    MovementVelocityObserved = _movementVelocityObserved,
                    LocomotionClipCount = _locomotionClipCount,
                    MovementDistanceMeters = _movementDistanceMeters,
                    TurnDegrees = _turnDegrees,
                    UnequipMatchReturned = _unequipMatchReturned,
                    UnequipAnimationObserved = _unequipAnimationObserved,
                    UnequipClipCount = _unequipClipCount
                });
                _removedPresentation = presentation;
                RemoveEquipped(_actor, ref _equipped,
                    ref _equippedFirearmStateSet);
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _settleUpdates = 0;
                _phase = 6;
            }

            private void PollRemoval()
            {
                EvidenceCase value = _cases[_caseIndex];
                _stage = "settle-transition-removal-" + value.Variant;
                TickRuntime();
                _actor.View.HandsEquipment.UpdateAll();
                GameObject current = _actor.View.HandsEquipment
                    .GetWeaponModel(false);
                bool removed = current == null &&
                    (_removedPresentation == null ||
                    !_removedPresentation.gameObject.activeInHierarchy ||
                    !_removedPresentation.IsChildOf(_actor.View.transform));
                _settleUpdates++;
                if (!removed)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(value.Variant +
                        " presentation remained active after transition-case " +
                        "cleanup: " + TransformPath(_removedPresentation,
                            _actor.View.transform));
                }
                _removedPresentation = null;
                _caseIndex++;
                ResetCaseState();
                if (_caseIndex < _cases.Length)
                {
                    _phase = 1;
                    return;
                }
                WriteIndex();
                _indexWritten = true;
                BeginCleanup();
            }

            private void ResetCaseState()
            {
                _moveCommand = null;
                _settleUpdates = 0;
                _equipCaptured = false;
                _unequipCaptured = false;
                _equipAnimationObserved = false;
                _unequipAnimationObserved = false;
                _movementCommandAccepted = false;
                _movementAgentMovingObserved = false;
                _movementVelocityObserved = false;
                _turnRequested = false;
                _equipMatchReturned = false;
                _unequipMatchReturned = false;
                _equipClipCount = 0;
                _unequipClipCount = 0;
                _locomotionClipCount = 0;
                _movementDistanceMeters = 0f;
                _turnDegrees = 0f;
                _movementStartArea = 0;
                _movementDestinationArea = 0;
                _movementGraphIndex = 0;
            }

            private void CaptureTransitionRecord(EvidenceCase value,
                Transform model, WeaponVisualParameters visual, string role,
                string state, bool transitionAnimating, string claim)
            {
                string prefix = _caseIndex.ToString("D2") + "-" +
                    SafeFileName(value.Variant) + "-" + state +
                    "-default-medium";
                string pngPath = Path.Combine(_request.EvidenceDirectory,
                    prefix + ".png");
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    prefix + ".json");
                CaptureSummary capture = CaptureContactSheet(_actor, model,
                    _fixtureBodyRenderers, pngPath);
                JObject record = Describe(value, _actor, model, visual,
                    _fixtureBodyRenderers, capture,
                    Path.GetFileName(pngPath), state, role);
                record["claimBoundary"] = claim;
                record["transitionAnimating"] = transitionAnimating;
                record["combatStateTransitionAnimating"] =
                    CombatStateTransitionAnimating(_actor);
                record["caseIndex"] = _caseIndex;
                record["settleUpdates"] = _settleUpdates;
                record["unitPosition"] = _actor.Position.ToString("R");
                record["unitOrientationDirection"] =
                    _actor.OrientationDirection.ToString("R");
                record["movementStart"] = _movementStart.ToString("R");
                record["movementDestination"] =
                    _movementDestination.ToString("R");
                record["movementStartArea"] = _movementStartArea;
                record["movementDestinationArea"] = _movementDestinationArea;
                record["movementGraphIndex"] = _movementGraphIndex;
                record["movementDistanceMeters"] = _movementDistanceMeters;
                record["movementCommandAccepted"] = _movementCommandAccepted;
                record["movementAgentMovingObserved"] =
                    _movementAgentMovingObserved;
                record["movementVelocityObserved"] =
                    _movementVelocityObserved;
                record["movementAgentWantsToMove"] =
                    _actor.View.MovementAgent.WantsToMove;
                record["movementAgentIsReallyMoving"] =
                    _actor.View.MovementAgent.IsReallyMoving;
                record["movementAgentVelocity"] =
                    _actor.View.MovementAgent.Velocity.ToString("R");
                record["actorInCombat"] = _actor.CombatState.IsInCombat;
                record["playerInCombat"] = Game.Instance.Player.IsInCombat;
                record["turnBasedCombat"] =
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat();
                record["turnDegrees"] = _turnDegrees;
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(pngPath);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _viewCount += 4;
                _diagnostics.Add(value.Variant + ":state=" + state +
                    ";role=" + role + ";animating=" +
                    transitionAnimating + ";movement=" +
                    _movementDistanceMeters.ToString("R") + ";turn=" +
                    _turnDegrees.ToString("R") + ";png=" +
                    Path.GetFileName(pngPath) + ";sha256=" + capture.Sha256 +
                    ";bytes=" + capture.Bytes + ";meaningfulPixels=" +
                    capture.MeaningfulPixels + ";framing=" + capture.Framing);
                if (capture.LowPixelDensity)
                    _warnings.Add(value.Variant + ":" + state +
                        " contact sheet has low foreground pixel density; " +
                        "retain it as an explicit framing diagnostic.");
            }

            private void WriteIndex()
            {
                _stage = "write-transition-motion-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                JArray outcomes = new JArray(_outcomes.Select(value =>
                    new JObject
                    {
                        { "variant", value.Variant },
                        { "equipMatchReturned", value.EquipMatchReturned },
                        { "equipAnimationObserved",
                            value.EquipAnimationObserved },
                        { "equipClipCount", value.EquipClipCount },
                        { "movementCommandAccepted",
                            value.MovementCommandAccepted },
                        { "movementAgentMovingObserved",
                            value.MovementAgentMovingObserved },
                        { "movementVelocityObserved",
                            value.MovementVelocityObserved },
                        { "locomotionClipCount",
                            value.LocomotionClipCount },
                        { "movementDistanceMeters",
                            value.MovementDistanceMeters },
                        { "turnDegrees", value.TurnDegrees },
                        { "unequipMatchReturned",
                            value.UnequipMatchReturned },
                        { "unequipAnimationObserved",
                            value.UnequipAnimationObserved },
                        { "unequipClipCount", value.UnequipClipCount }
                    }).ToArray());
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "fixture", "live disposable default Medium humanoid" },
                    { "productionVariantCount", 22 },
                    { "nativeControlCount", 6 },
                    { "states", new JArray("equip-transition", "moving",
                        "turned-right", "unequip-transition") },
                    { "views", new JArray("front", "right-side", "rear",
                        "front-right-three-quarter") },
                    { "loadedModVersion", _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "outcomes", outcomes },
                    { "records", _records }
                };
                string indexPath = Path.Combine(_request.EvidenceDirectory,
                    "weapon-presentation-transition-motion-index.json");
                WriteJsonAtomic(indexPath, index);
                _evidenceFiles.Add(indexPath);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "transition-motion-request-cleanup";
                if (_actor != null)
                {
                    _actor.Commands.InterruptAll(true);
                    if (_actor.View != null &&
                        _actor.View.HandsEquipment != null)
                        _actor.View.HandsEquipment.ForceSwitch(false);
                    RemoveEquipped(_actor, ref _equipped,
                        ref _equippedFirearmStateSet);
                    if (_actor.CombatState != null &&
                        _actor.CombatState.IsInCombat)
                        _actor.CombatState.LeaveCombat();
                    _actor.Descriptor.State.Immortality.ReleaseAll();
                }
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
                const int expectedCases = 28;
                const int statesPerCase = 4;
                const int expectedRecords = expectedCases * statesPerCase;
                int variants = _records.OfType<JObject>().Select(value =>
                    (string)value["variant"]).Distinct(
                        StringComparer.Ordinal).Count();
                string[] states = { "equip-transition", "moving",
                    "turned-right", "unequip-transition" };
                bool exactStates = states.All(state =>
                    _records.OfType<JObject>().Count(value => string.Equals(
                        (string)value["state"], state,
                        StringComparison.Ordinal)) == expectedCases);
                Add(_assertions,
                    "weapon-presentation-transition-motion-matrix",
                    "22 production variants and six native controls in four " +
                        "exact transition/movement states",
                    "records=" + _records.Count + ";variants=" + variants +
                        ";exactStates=" + exactStates,
                    _records.Count == expectedRecords &&
                        variants == expectedCases && exactStates,
                    "native equipment coroutines, UnitMoveTo, ForceLookAt, and exact live held/stored models");
                Add(_assertions,
                    "weapon-presentation-native-equip-unequip-transitions",
                    "every case exposes MainHandEquip and MainHandUnequip clips " +
                        "and is captured while the native combat-state " +
                        "coroutine is active",
                    string.Join(";", _outcomes.Select(value => value.Variant +
                        "=equip:" + value.EquipClipCount + "/animated:" +
                        value.EquipAnimationObserved + "/matched:" +
                        value.EquipMatchReturned + ",unequip:" +
                        value.UnequipClipCount + "/animated:" +
                        value.UnequipAnimationObserved + "/matched:" +
                        value.UnequipMatchReturned).ToArray()),
                    _outcomes.Count == expectedCases &&
                        _outcomes.All(value => value.EquipClipCount > 0 &&
                            value.UnequipClipCount > 0 &&
                            value.EquipMatchReturned &&
                            value.UnequipMatchReturned &&
                            value.EquipAnimationObserved &&
                            value.UnequipAnimationObserved),
                    "UnitViewHandsEquipment.OnCombatStateChanged, " +
                        "MatchWithCurrentCombatState, m_Coroutine, and " +
                        "AreHandsBusyWithAnimation; equipment guard only, " +
                        "without UnitCombatState.JoinCombat");
                string[] easternFamilies =
                    { "Wakizashi", "Katana", "Nodachi" };
                JObject[] easternRecords = _records.OfType<JObject>()
                    .Where(value => easternFamilies.Contains(
                        (string)value["family"], StringComparer.Ordinal))
                    .ToArray();
                string[] easternNativeVariants =
                {
                    "Native.Scimitar", "Native.BastardSword",
                    "Native.Greatsword"
                };
                JObject[] easternNativeRecords = _records.OfType<JObject>()
                    .Where(value => easternNativeVariants.Contains(
                        (string)value["variant"], StringComparer.Ordinal))
                    .ToArray();
                int clearedEasternSheaths = easternRecords.Count(value =>
                    string.Equals((string)value["sheathModel"], "<null>",
                        StringComparison.Ordinal));
                int retainedNativeEasternSheaths = easternNativeRecords.Count(
                    value => !string.Equals((string)value["sheathModel"],
                        "<null>", StringComparison.Ordinal));
                Add(_assertions,
                    "weapon-presentation-eastern-custom-sheath-replacement",
                    "all 12 Eastern variants use complete custom stored " +
                        "presentation without a donor sheath while all three " +
                        "native donor controls retain their sheaths in four " +
                        "states",
                    "custom=" + clearedEasternSheaths + "/" +
                        easternRecords.Length + ";native=" +
                        retainedNativeEasternSheaths + "/" +
                        easternNativeRecords.Length,
                    easternRecords.Length == 12 * statesPerCase &&
                        clearedEasternSheaths == easternRecords.Length &&
                        easternNativeRecords.Length == 3 * statesPerCase &&
                        retainedNativeEasternSheaths ==
                            easternNativeRecords.Length,
                    "live custom clone and unchanged native donor " +
                        "WeaponVisualParameters across transitions and motion");
                Add(_assertions,
                    "weapon-presentation-native-locomotion",
                    "every case starts and runs navmesh-backed UnitMoveTo with " +
                        "nonzero native velocity and measurable displacement",
                    string.Join(";", _outcomes.Select(value => value.Variant +
                        "=accepted:" + value.MovementCommandAccepted +
                        "/agentMoving:" + value.MovementAgentMovingObserved +
                        "/velocity:" + value.MovementVelocityObserved +
                        "/clips:" + value.LocomotionClipCount +
                        "/meters:" + value.MovementDistanceMeters
                            .ToString("R")).ToArray()),
                    _outcomes.Count == expectedCases &&
                        _outcomes.All(value =>
                            value.MovementCommandAccepted &&
                            value.MovementAgentMovingObserved &&
                            value.MovementVelocityObserved &&
                            value.MovementDistanceMeters > 0.05f),
                    "native UnitMoveTo, same-area ForcedPath, MovementAgent " +
                        "velocity, and live rig-bound displacement; LocoMotion " +
                        "clip count is retained as non-gating diagnostics");
                Add(_assertions,
                    "weapon-presentation-body-relative-turn",
                    "every held presentation follows a native turn of at least " +
                        "60 degrees instead of remaining world-space pinned",
                    string.Join(";", _outcomes.Select(value => value.Variant +
                        "=" + value.TurnDegrees.ToString("R")).ToArray()),
                    _outcomes.Count == expectedCases && _outcomes.All(value =>
                        value.TurnDegrees >= 60f),
                    "UnitEntityData.ForceLookAt plus live rig-bound held model");
                int zeroPixelSheets = _records.OfType<JObject>().Count(value =>
                    (int)value["meaningfulPixels"] <= 0);
                Add(_assertions,
                    "weapon-presentation-transition-motion-contact-sheets",
                    expectedRecords + " PNG/JSON pairs and " +
                        (expectedRecords * 4) + " labelled views",
                    "captures=" + _captured + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count +
                        ";zeroPixelSheets=" + zeroPixelSheets,
                    _captured == expectedRecords &&
                        _viewCount == expectedRecords * 4 && _indexWritten &&
                        _evidenceFiles.Count == expectedRecords * 2 + 1 &&
                        _evidenceFiles.All(File.Exists) && zeroPixelSheets == 0,
                    "front/right-side/rear/front-right-three-quarter live transition and movement contact sheets");
                Add(_assertions,
                    "weapon-presentation-transition-motion-request-cleanup",
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";settleUpdates=" +
                        _settleUpdates, cleaned,
                    "request-local item, actor, blueprint clone, commands, camera, light, and textures");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Transition/motion evidence is limited to the " +
                    "default Medium humanoid. It does not establish reload, " +
                    "dual-wield, armor/cloak, female, Small, or Enlarged " +
                    "acceptance.");
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
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }

            private void TickRuntime()
            {
                Game.Instance.EntityCreator.Tick();
                if (_actor != null && _actor.View != null &&
                    _actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
            }
        }

        private static bool CombatStateTransitionAnimating(UnitEntityData actor)
        {
            object hands = actor == null || actor.View == null ? null :
                (object)actor.View.HandsEquipment;
            if (hands == null) return false;
            FieldInfo field = hands.GetType().GetField(
                "m_Coroutine", Members);
            if (field == null || field.FieldType != typeof(Coroutine))
                throw new MissingFieldException(hands.GetType().FullName,
                    "m_Coroutine");
            return field.GetValue(hands) != null &&
                actor.View.HandsEquipment.AreHandsBusyWithAnimation.Value;
        }

        private static bool TryResolveEasternBladeFrame(
            EvidenceCase value, Transform model, out string source,
            out Vector3 tip, out Vector3 butt, out Vector3 forward,
            out Vector3 bladeNormal, out Vector3 cuttingEdge)
        {
            source = string.Empty;
            tip = Vector3.zero;
            butt = Vector3.zero;
            forward = Vector3.zero;
            bladeNormal = Vector3.zero;
            cuttingEdge = Vector3.zero;
            if (value == null || model == null) return false;

            Transform gripMarker = model.Find(
                WeaponPresentationFrameContract.GripMarker);
            Transform tipMarker = model.Find("Tip");
            Transform buttMarker = model.Find(
                WeaponPresentationFrameContract.ButtMarker);
            Transform forwardMarker = model.Find(
                WeaponPresentationFrameContract.WeaponForwardMarker);
            Transform normalMarker = model.Find(
                WeaponPresentationFrameContract.BladeNormalMarker);
            Transform edgeMarker = model.Find("CuttingEdge");
            if (gripMarker != null && tipMarker != null &&
                buttMarker != null && forwardMarker != null &&
                normalMarker != null && edgeMarker != null)
            {
                source =
                    "authored-renderer-bound-Tip/Butt+WeaponForward/BladeNormal/CuttingEdge";
                tip = tipMarker.position;
                butt = buttMarker.position;
                forward = (forwardMarker.position - gripMarker.position)
                    .normalized;
                bladeNormal = (normalMarker.position - gripMarker.position)
                    .normalized;
                cuttingEdge = (edgeMarker.position - gripMarker.position)
                    .normalized;
            }
            else
            {
                bool native = string.Equals(value.Variant,
                        "Native.Scimitar", StringComparison.Ordinal) ||
                    string.Equals(value.Variant, "Native.BastardSword",
                        StringComparison.Ordinal) ||
                    string.Equals(value.Variant, "Native.Greatsword",
                        StringComparison.Ordinal);
                if (!native) return false;
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(
                    true).Where(renderer => renderer != null).ToArray();
                if (renderers.Length == 0) return false;
                int sourceCount;
                Bounds bounds = LocalBounds(model, renderers,
                    out sourceCount);
                if (sourceCount == 0 || bounds.size.y < 0.5f ||
                    bounds.size.y <= bounds.size.x ||
                    bounds.size.y <= bounds.size.z)
                    return false;
                source = "native-renderer-local-+Y-forward/+X-blade-normal/-Z-cutting-edge";
                tip = model.TransformPoint(bounds.center +
                    Vector3.up * bounds.extents.y);
                butt = model.TransformPoint(bounds.center -
                    Vector3.up * bounds.extents.y);
                forward = model.TransformDirection(Vector3.up).normalized;
                bladeNormal = model.TransformDirection(Vector3.right)
                    .normalized;
                cuttingEdge = model.TransformDirection(Vector3.back)
                    .normalized;
            }

            if (Vector3.Distance(tip, butt) <= 0.5f ||
                forward.sqrMagnitude < 0.99f ||
                bladeNormal.sqrMagnitude < 0.99f ||
                cuttingEdge.sqrMagnitude < 0.99f)
                return false;
            Vector3 right = Vector3.Cross(bladeNormal, forward).normalized;
            return Mathf.Abs(Vector3.Dot(bladeNormal, forward)) < 0.05f &&
                Mathf.Abs(Vector3.Dot(cuttingEdge, forward)) < 0.05f &&
                Mathf.Abs(Vector3.Dot(cuttingEdge, bladeNormal)) < 0.05f &&
                Vector3.Dot(cuttingEdge, -right) > 0.99f &&
                Vector3.Dot(tip - butt, forward) > 0.5f;
        }

        private static bool TryResolveSpearPhysicalEndpoints(
            EvidenceCase value, Transform model, out string source,
            out Vector3 tip, out Vector3 butt)
        {
            source = string.Empty;
            tip = Vector3.zero;
            butt = Vector3.zero;
            if (value == null || model == null) return false;
            Transform tipMarker = model.Find("Tip");
            Transform buttMarker = model.Find("Butt");
            if (tipMarker != null && buttMarker != null)
            {
                source = "authored-renderer-bound-Tip/Butt";
                tip = tipMarker.position;
                butt = buttMarker.position;
                return Vector3.Distance(tip, butt) > 2f;
            }
            if (!string.Equals(value.Variant, "Native.Longspear",
                    StringComparison.Ordinal))
                return false;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer != null).ToArray();
            if (renderers.Length == 0) return false;
            int sourceCount;
            Bounds bounds = LocalBounds(model, renderers, out sourceCount);
            if (sourceCount == 0 || bounds.size.y < 2f ||
                bounds.size.y <= bounds.size.x ||
                bounds.size.y <= bounds.size.z)
                return false;
            source = "native-TH_LongspearKnight1-renderer-positive-Y-head";
            tip = model.TransformPoint(bounds.center +
                Vector3.up * bounds.extents.y);
            butt = model.TransformPoint(bounds.center -
                Vector3.up * bounds.extents.y);
            return Vector3.Distance(tip, butt) > 2f;
        }

        private static EvidenceCase[] BuildMotionCases(string[] variants)
        {
            EvidenceCase[] catalog = BuildCases();
            return variants.Select(variant => catalog.Single(value =>
                string.Equals(value.Variant, variant,
                    StringComparison.Ordinal))).ToArray();
        }

        private static bool IsFirearm(EvidenceCase value)
        {
            return value != null && value.Symbol.StartsWith("KMG.Firearms.",
                StringComparison.Ordinal);
        }

        private static Vector3 NearestNavigable(Vector3 requested)
        {
            if (AstarPath.active == null) return requested;
            Pathfinding.NNInfo nearest = AstarPath.active.GetNearest(requested);
            return nearest.node == null ? requested : nearest.clampedPosition;
        }

        private static void SetUnitPosition(UnitEntityData unit,
            Vector3 position)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            unit.Position = position;
            if (unit.View != null) unit.View.transform.position = position;
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
                { "modelWorldForward", model.forward.ToString("R") },
                { "modelWorldUp", model.up.ToString("R") },
                { "modelWorldRight", model.right.ToString("R") },
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
                { "rigContacts", DescribeRigContacts(actor, model) },
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
                "support", "forward", "up", "normal", "mount" };
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

        private static JObject DescribeRigContacts(UnitEntityData actor,
            Transform model)
        {
            Transform view = actor == null || actor.View == null ? null :
                actor.View.transform;
            Transform grip = model == null ? null : model.Find("Grip");
            EquipmentOffsets offsets = model == null ? null :
                model.GetComponent<EquipmentOffsets>();
            Transform authoredSupport = model == null ? null :
                model.Find("SupportHandTarget");
            Transform support = offsets != null &&
                offsets.IkTargetLeftHand != null ? offsets.IkTargetLeftHand :
                authoredSupport;
            Transform butt = model == null ? null : model.Find("Butt");
            return new JObject
            {
                { "supportTargetSource", offsets != null &&
                    offsets.IkTargetLeftHand != null ?
                    "EquipmentOffsets.IkTargetLeftHand" :
                    authoredSupport != null ? "SupportHandTarget" : "missing" },
                { "supportTargetPath", support == null ?
                    JValue.CreateNull() :
                    (JToken)TransformPath(support, model) },
                { "dominantHandToGrip", DescribeRigContact(view, model,
                    "R_Hand", grip) },
                { "weaponBoneToGrip", DescribeRigContact(view, model,
                    "R_WeaponBone", grip) },
                { "supportHandToTarget", DescribeRigContact(view, model,
                    "L_Hand", support) },
                { "dominantClavicleToButt", DescribeRigContact(view, model,
                    "R_Clavicle", butt) }
            };
        }

        private static JToken DescribeRigContact(Transform view,
            Transform model, string boneName, Transform target)
        {
            if (view == null || model == null) return JValue.CreateNull();
            Transform[] matches = view.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null && string.Equals(value.name,
                    boneName, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
                return new JObject
                {
                    { "bone", boneName },
                    { "matchCount", matches.Length },
                    { "targetPresent", target != null }
                };
            Transform bone = matches[0];
            return new JObject
            {
                { "bone", boneName },
                { "matchCount", 1 },
                { "bonePath", TransformPath(bone, view) },
                { "boneModelLocalPosition", model.InverseTransformPoint(
                    bone.position).ToString("R") },
                { "targetPresent", target != null },
                { "targetModelLocalPosition", target == null ?
                    JValue.CreateNull() :
                    (JToken)target.localPosition.ToString("R") },
                { "distanceMeters", target == null ? JValue.CreateNull() :
                    (JToken)Vector3.Distance(bone.position, target.position) }
            };
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
