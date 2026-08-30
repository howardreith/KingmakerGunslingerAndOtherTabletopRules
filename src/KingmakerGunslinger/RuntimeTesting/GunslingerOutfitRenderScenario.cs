using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded visual comparison of audited native class-clothing candidates.
    /// It uses disposable Human actors and restores each avatar before disposal.
    /// </summary>
    internal static class GunslingerOutfitRenderScenario
    {
        private const int EvidenceLayer = 31;
        private const int IsometricPanelSize = 512;
        private const string MaleHumanDonorGuid =
            "5dc5fc514d8f40d4baec6a54a17f0185";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string ExpectedAssemblySha256 =
            "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb";
        private const string ExpectedAssemblyMvid =
            "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";

        private static readonly CandidateSpec[] Candidates =
        {
            new CandidateSpec("bard-complete", 12, 16,
                new[]
                {
                    "94d11df1d859b6d4f90424213eec0392",
                    "431d16d2153d1854280b97470223eea6",
                    "e5ff950ef29119943bdcf3bfedd47887"
                },
                new[]
                {
                    "9aa7feeafa6f05f45a9fbae3b87bfc02",
                    "49641981096de8b43b198e95c7193b65",
                    "e9ce35008c62b334383e73e244becc36"
                }),
            new CandidateSpec("alchemist-complete", 17, 31,
                new[]
                {
                    "3709387ae978dae4d8ab60700a1e25e2",
                    "db2f0f4384784974ba2428c96b21aa4e",
                    "7667972f03e25494cb6b39ba7e82126f"
                },
                new[]
                {
                    "eb257cbf25c5363408073e2b11559a19",
                    "2abb4698b7fcce24d9bdab0ffbd852f3",
                    "6b8410318571dd949bd758e9f1275182"
                }),
            new CandidateSpec("magus-complete", 2, 22,
                new[]
                {
                    "6df8f61725a84294c8661bb9585eca97",
                    "4c59d2b9740930145a27a4c693217d22"
                },
                new[]
                {
                    "beba0e0c7dcd5c64d97d767be3e72995",
                    "a93ead19aae8afc4794c54f5bcf73168"
                }),
            new CandidateSpec("ranger-capless-capeless", 13, 7,
                new[]
                {
                    "e249678d823d00f4cb30d4d5c8ca1219",
                    "0809ab3735b54874b965a09311f0c898",
                    "ca71ad9178ecf6a4d942ce55d0c7857b"
                },
                new[]
                {
                    "e09cf61a567f2a84ea9a3b505f390a32",
                    "b6bca728c4ced324da7e8d0d01ad34bb",
                    "bc6fb7e5c91de08418b81a397b20bb18"
                }),
            new CandidateSpec("rogue-capless-capeless", 31, 22,
                new[]
                {
                    "b1c62eff2287d9a4fbbf76c345d58840",
                    "d019e95d4a8a8474aa4e03489449d6ee"
                },
                new[]
                {
                    "345af8eabd450524ab364e7a7c6f1044",
                    "c6757746d62b78f46a92020110dfe088"
                }),
            new CandidateSpec("slayer-capless", 35, 36,
                new[]
                {
                    "096463cb26b8c3343874d2a2a1a752f6",
                    "bf0f3ba364295e14eb5f2b285cea16b0",
                    "9e98bd43dc04964409db62644ace4b15"
                },
                new[]
                {
                    "24230460eaff3fe49b0e186873c38218",
                    "5eeabb19544a9ae41a8b26075933ef8d",
                    "50b6ed92792f308479a07f8d9052c6d5"
                })
        };

        private static readonly RenderCase[] Cases =
        {
            new RenderCase("native-default", 0, "no-weapon"),
            new RenderCase("native-default", 0, "pistol"),
            new RenderCase("native-default", 0, "musket"),
            new RenderCase("audit-alternate", 1, "no-weapon")
        };

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        private sealed class CandidateSpec
        {
            internal CandidateSpec(string label, int primary, int secondary,
                string[] male, string[] female)
            {
                Label = label;
                Primary = primary;
                Secondary = secondary;
                Male = male;
                Female = female;
            }

            internal readonly string Label;
            internal readonly int Primary;
            internal readonly int Secondary;
            internal readonly string[] Male;
            internal readonly string[] Female;

            internal string[] For(Gender gender)
            {
                return gender == Gender.Male ? Male : Female;
            }

            internal JObject Describe()
            {
                return new JObject
                {
                    { "candidateId", Label },
                    { "nativePrimaryColor", Primary },
                    { "nativeSecondaryColor", Secondary },
                    { "maleAssetIds", new JArray(Male) },
                    { "femaleAssetIds", new JArray(Female) }
                };
            }
        }

        private static IsometricCapture CaptureIsometric(UnitEntityData actor,
            Renderer[] sourceRenderers, string pngPath)
        {
            Renderer[] renderers = (sourceRenderers ?? new Renderer[0])
                .Where(value => value != null && value.enabled &&
                    value.gameObject.activeInHierarchy).ToArray();
            if (actor == null || actor.View == null || renderers.Length == 0)
                throw new InvalidOperationException(
                    "Isometric outfit evidence requires a live rendered actor.");
            Bounds bounds = CombinedBounds(renderers);
            float maximum = Mathf.Max(bounds.size.x,
                Mathf.Max(bounds.size.y, bounds.size.z));
            Camera live = UnityEngine.Object.FindObjectsOfType<Camera>()
                .Where(value => value != null && value.enabled)
                .OrderByDescending(value =>
                    ReferenceEquals(value, Camera.main)).FirstOrDefault();
            if (live == null)
                throw new InvalidOperationException(
                    "The working-save evidence run has no enabled game camera.");

            var layers = actor.View.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null)
                .Select(value => value.gameObject).Distinct()
                .ToDictionary(value => value, value => value.layer);
            var cameraObject = new GameObject(
                "KMG_Runtime_GunslingerOutfitIsometricCamera");
            var lightObject = new GameObject(
                "KMG_Runtime_GunslingerOutfitIsometricLight");
            Camera camera = cameraObject.AddComponent<Camera>();
            Light light = lightObject.AddComponent<Light>();
            RenderTexture target = null;
            Texture2D texture = null;
            RenderTexture priorActive = RenderTexture.active;
            try
            {
                foreach (KeyValuePair<GameObject, int> value in layers)
                    value.Key.layer = EvidenceLayer;
                camera.CopyFrom(live);
                camera.enabled = false;
                camera.cullingMask = 1 << EvidenceLayer;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor =
                    new Color(0.12f, 0.14f, 0.17f, 1f);
                camera.orthographic = true;
                camera.aspect = 1f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.orthographicSize = Mathf.Max(1.55f,
                    maximum * 0.88f);
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.cullingMask = 1 << EvidenceLayer;
                light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
                target = new RenderTexture(IsometricPanelSize,
                    IsometricPanelSize, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = target;
                texture = new Texture2D(IsometricPanelSize,
                    IsometricPanelSize, TextureFormat.RGBA32, false, false);
                Vector3 horizontal = (actor.View.transform.forward +
                    actor.View.transform.right).normalized;
                float distance = Mathf.Max(6f, maximum * 4f);
                camera.transform.position = bounds.center +
                    horizontal * distance + Vector3.up * distance * 0.72f;
                camera.transform.LookAt(bounds.center);
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, IsometricPanelSize,
                    IsometricPanelSize), 0, 0, false);
                texture.Apply(false, false);
                Color32 fill = camera.backgroundColor;
                int meaningful = texture.GetPixels32().Count(pixel =>
                    Math.Abs(pixel.r - fill.r) +
                    Math.Abs(pixel.g - fill.g) +
                    Math.Abs(pixel.b - fill.b) > 24);
                File.WriteAllBytes(pngPath,
                    WeaponPresentationEvidenceScenario.EncodePng(texture));
                var info = new FileInfo(pngPath);
                return new IsometricCapture
                {
                    Path = pngPath,
                    Bytes = info.Length,
                    Sha256 = HashFile(pngPath),
                    MeaningfulPixels = meaningful,
                    RendererCount = renderers.Length,
                    Bounds = bounds.size.ToString("R"),
                    Framing = "orthographic-elevated;center=" +
                        bounds.center.ToString("R") + ";maximum=" +
                        maximum.ToString("R") + ";size=" +
                        camera.orthographicSize.ToString("R"),
                    LowPixelDensity = meaningful <
                        IsometricPanelSize * IsometricPanelSize / 20
                };
            }
            finally
            {
                RenderTexture.active = priorActive;
                foreach (KeyValuePair<GameObject, int> value in layers)
                    if (value.Key != null) value.Key.layer = value.Value;
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(lightObject);
            }
        }

        private static Renderer[] ActiveRenderers(UnitEntityData actor)
        {
            return actor == null || actor.View == null
                ? new Renderer[0]
                : actor.View.GetComponentsInChildren<Renderer>(true)
                    .Where(value => value != null && value.enabled &&
                        value.gameObject.activeInHierarchy).ToArray();
        }

        private static bool Renderable(Transform model)
        {
            return model != null &&
                model.GetComponentsInChildren<Renderer>(true).Any(value =>
                    value != null && value.enabled &&
                    value.gameObject.activeInHierarchy);
        }

        private static bool HasExactHumanoidRig(Transform root)
        {
            if (root == null) return false;
            Transform[] values = root.GetComponentsInChildren<Transform>(true);
            return values.Count(value => value != null &&
                    value.name == "R_WeaponBone") == 1 &&
                values.Count(value => value != null &&
                    value.name == "R_Hand") == 1 &&
                values.Count(value => value != null &&
                    value.name == "L_Hand") == 1;
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
                throw new InvalidOperationException(
                    "Outfit evidence requires renderer bounds.");
            Bounds value = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
                value.Encapsulate(renderers[index].bounds);
            return value;
        }

        private static Vector3 NearestNavigable(Vector3 requested)
        {
            if (AstarPath.active == null) return requested;
            Pathfinding.NNInfo nearest = AstarPath.active.GetNearest(requested);
            return nearest.node == null ? requested :
                nearest.clampedPosition;
        }

        private static void ClearHand(UnitEntityData actor, bool primary)
        {
            if (actor == null || actor.Body == null) return;
            var slot = primary ? actor.Body.PrimaryHand :
                actor.Body.SecondaryHand;
            if (slot != null && slot.MaybeItem != null)
                slot.RemoveItem(false);
        }

        private static void ValidateCandidateCatalog()
        {
            if (Candidates.Length != 6 ||
                Candidates.Any(value => value.Male == null ||
                    value.Female == null || value.Male.Length < 1 ||
                    value.Male.Length > 3 || value.Female.Length < 1 ||
                    value.Female.Length > 3 ||
                    value.Male.Distinct(StringComparer.Ordinal).Count() !=
                        value.Male.Length ||
                    value.Female.Distinct(StringComparer.Ordinal).Count() !=
                        value.Female.Length) ||
                UniqueCandidateIds().Length != 32)
                throw new InvalidOperationException(
                    "The outfit render catalog must contain the exact six " +
                    "audited one-to-three-entity native candidates.");
        }

        private static string[] UniqueCandidateIds()
        {
            return Candidates.SelectMany(value =>
                    value.Male.Concat(value.Female))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
        }

        private static string CandidateSetId()
        {
            string canonical = string.Join("\n", Candidates.Select(value =>
                value.Label + "|" + value.Primary + "|" + value.Secondary +
                "|M:" + string.Join(",", value.Male) + "|F:" +
                string.Join(",", value.Female)).ToArray());
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(canonical)))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string SafeFileName(string value)
        {
            var builder = new StringBuilder();
            foreach (char character in value.ToLowerInvariant())
                builder.Append(char.IsLetterOrDigit(character) ?
                    character : '-');
            return builder.ToString().Trim('-');
        }

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static void WriteJsonAtomic(string path, JToken value)
        {
            string temporary = path + "." +
                Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary,
                value.ToString(Formatting.Indented),
                new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }

        private static object[] Snapshot(object collection)
        {
            var enumerable = collection as IEnumerable;
            return enumerable == null ? new object[0] :
                enumerable.Cast<object>().ToArray();
        }

        private static bool SameReferences(object[] expected,
            object[] actual)
        {
            if (expected.Length != actual.Length) return false;
            return expected.All(value => actual.Any(current =>
                ReferenceEquals(value, current)));
        }

        private static bool ContainsReference(object collection,
            object target)
        {
            return Snapshot(collection).Any(value =>
                ReferenceEquals(value, target));
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions,
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

        private sealed class RenderCase
        {
            internal RenderCase(string palette, int paletteIndex,
                string weapon)
            {
                Palette = palette;
                PaletteIndex = paletteIndex;
                Weapon = weapon;
            }

            internal readonly string Palette;
            internal readonly int PaletteIndex;
            internal readonly string Weapon;
        }

        private sealed class FixtureSpec
        {
            internal FixtureSpec(string label, BlueprintUnit source)
            {
                Label = label;
                Source = source;
            }

            internal readonly string Label;
            internal readonly BlueprintUnit Source;
        }

        private sealed class AvatarEntityState
        {
            internal EquipmentEntity Entity;
            internal int Primary;
            internal int Secondary;
        }

        private sealed class IsometricCapture
        {
            internal string Path;
            internal long Bytes;
            internal string Sha256;
            internal int MeaningfulPixels;
            internal int RendererCount;
            internal string Bounds;
            internal string Framing;
            internal bool LowPixelDensity;
        }

        internal sealed class Session
        {
            private const int MaximumSettleUpdates = 360;
            private const int MinimumSettleUpdates = 30;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _evidenceFiles = new List<string>();
            private readonly JArray _records = new JArray();
            private readonly JArray _fixtureRecords = new JArray();
            private readonly JArray _restorationRecords = new JArray();
            private object _allUnits;
            private object _party;
            private object[] _unitsBefore = new object[0];
            private object[] _partyBefore = new object[0];
            private FixtureSpec[] _fixtures = new FixtureSpec[0];
            private UnitEntityData _anchor;
            private UnitEntityData _actor;
            private BlueprintUnit _actorBlueprint;
            private Character _avatar;
            private AvatarEntityState[] _avatarBefore =
                new AvatarEntityState[0];
            private string[] _savedLinksBefore = new string[0];
            private EquipmentEntity[] _classEntities =
                new EquipmentEntity[0];
            private EquipmentEntity[] _candidateEntities =
                new EquipmentEntity[0];
            private JArray _paletteEvidence = new JArray();
            private ItemEntityWeapon _weapon;
            private bool _firearmStateSet;
            private int _fixtureIndex;
            private int _candidateIndex;
            private int _caseIndex;
            private int _phase;
            private int _settleUpdates;
            private int _currentPalette = -1;
            private int _resolvedEntities;
            private int _paletteApplications;
            private int _restorations;
            private int _captured;
            private int _imageCount;
            private int _viewCount;
            private bool _actorInitialized;
            private bool _cleanupStarted;
            private bool _indexWritten;
            private string _stage = "resolve-working-save-anchor";
            private string _exceptionSummary = string.Empty;
            private string _assemblySha256 = string.Empty;
            private string _assemblyMvid = string.Empty;

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
                        if (!SpawnFixture()) return;
                        _phase = 2;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 2)
                    {
                        PollFixtureReadiness();
                        return;
                    }
                    if (_phase == 3)
                    {
                        ApplyCandidate();
                        _phase = 4;
                        _settleUpdates = 0;
                        return;
                    }
                    if (_phase == 4)
                    {
                        PollOutfitReadiness();
                        return;
                    }
                    if (_phase == 5)
                    {
                        PrepareCase();
                        return;
                    }
                    if (_phase == 6)
                    {
                        PollCaseAndCapture();
                        return;
                    }
                    AdvanceCase();
                }
                catch (Exception exception)
                {
                    _exceptionSummary = exception.ToString();
                    Add(_assertions, "gunslinger-outfit-render-exception",
                        "no exception", "stage=" + _stage + ";" + exception,
                        false, "guarded disposable-avatar render boundary");
                    BeginCleanup();
                }
            }

            private void Initialize()
            {
                _allUnits = Game.Instance.State.Units.All;
                _party = Game.Instance.Player.Party;
                _unitsBefore = Snapshot(_allUnits);
                _partyBefore = Snapshot(_party);
                _anchor = _partyBefore.OfType<UnitEntityData>().FirstOrDefault(
                    value => value != null && value.HoldingState != null &&
                        value.View != null);
                if (_anchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party-area anchor.");

                ValidateCandidateCatalog();
                BlueprintUnit male = BlueprintLibraryLookup.RequireExact<
                    BlueprintUnit>(BlueprintBootstrap.Library,
                        MaleHumanDonorGuid,
                        "gunslinger-outfit-render-male-human-donor");
                BlueprintUnit female = ResolveFemaleHumanDonor();
                ValidateHumanDonor(male, Gender.Male, "male");
                ValidateHumanDonor(female, Gender.Female, "female");
                _fixtures = new[]
                {
                    new FixtureSpec("male-human", male),
                    new FixtureSpec("female-human", female)
                };
                var gameAssembly = typeof(BlueprintCharacterClass).Assembly;
                _assemblySha256 = HashFile(gameAssembly.Location)
                    .ToLowerInvariant();
                _assemblyMvid = gameAssembly.ManifestModule.ModuleVersionId
                    .ToString("D");
                _diagnostics.Add("candidateSetId=" + CandidateSetId());
                _diagnostics.Add("fixtures=" + string.Join(",",
                    _fixtures.Select(value => value.Label + "=" +
                        DescribeBlueprint(value.Source)).ToArray()));
                WriteProgress("initialized");
            }

            private BlueprintUnit ResolveFemaleHumanDonor()
            {
                BlueprintUnit[] values = ResourcesLibrary
                    .GetBlueprints<BlueprintUnit>()
                    .Where(value => IsBodyDonor(value) &&
                        value.Gender == Gender.Female &&
                        value.Size == Size.Medium &&
                        value.Race.RaceId == Race.Human)
                    .OrderBy(FemaleDonorPriority)
                    .ThenBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
                _diagnostics.Add("femaleHumanDonorCandidates=" +
                    string.Join("|", values.Take(20).Select(
                        DescribeBlueprint).ToArray()));
                if (values.Length == 0)
                    throw new InvalidOperationException(
                        "No native female Human Medium body donor exists.");
                return values[0];
            }

            private static int FemaleDonorPriority(BlueprintUnit value)
            {
                string name = value == null ? string.Empty :
                    value.name ?? string.Empty;
                if (name.StartsWith("StartGamePregen",
                        StringComparison.OrdinalIgnoreCase)) return 0;
                if (name.IndexOf("Valerie",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 1;
                if (name.IndexOf("Amiri",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 2;
                if (name.IndexOf("Companion",
                        StringComparison.OrdinalIgnoreCase) >= 0) return 3;
                return 4;
            }

            private static bool IsBodyDonor(BlueprintUnit value)
            {
                return value != null && value.Prefab != null &&
                    value.Race != null && value.Body != null &&
                    !value.Body.DisableHands;
            }

            private static void ValidateHumanDonor(BlueprintUnit value,
                Gender gender, string label)
            {
                if (!IsBodyDonor(value) || value.Gender != gender ||
                    value.Size != Size.Medium ||
                    value.Race.RaceId != Race.Human)
                    throw new InvalidOperationException("The native " + label +
                        " Human donor violates the exact body contract: " +
                        DescribeBlueprint(value));
            }

            private static string DescribeBlueprint(BlueprintUnit value)
            {
                return value == null ? "<null>" : value.name + "/" +
                    value.AssetGuid + "/" + value.Gender + "/" +
                    (value.Race == null ? "<no-race>" :
                        value.Race.RaceId.ToString()) + "/" + value.Size;
            }

            private bool SpawnFixture()
            {
                FixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "spawn-" + fixture.Label;
                if (_actorBlueprint == null)
                {
                    _actorBlueprint = UnityEngine.Object.Instantiate(
                        fixture.Source);
                    _actorBlueprint.Race = fixture.Source.Race;
                    _actorBlueprint.name =
                        "KMG_Runtime_Gunslinger_Outfit_" +
                        fixture.Label.Replace('-', '_');
                    _actorBlueprint.IsCheater = true;
                }
                Game.Instance.EntityCreator.Tick();
                var prefab = fixture.Source.Prefab.Load(false);
                _settleUpdates++;
                if (prefab == null)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return false;
                    throw new InvalidOperationException(fixture.Label +
                        " native unit prefab did not load after " +
                        _settleUpdates + " updates.");
                }
                _actor = Game.Instance.EntityCreator.SpawnUnit(
                    _actorBlueprint, prefab,
                    NearestNavigable(_anchor.Position +
                        new Vector3(-2.5f, 0f, 2.5f)),
                    Quaternion.identity, _anchor.HoldingState);
                if (_actor == null)
                    throw new InvalidOperationException(
                        fixture.Label + " disposable actor did not spawn.");
                _actorInitialized = false;
                WriteProgress("spawned");
                return true;
            }

            private void PollFixtureReadiness()
            {
                FixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "settle-" + fixture.Label;
                Game.Instance.EntityCreator.Tick();
                _settleUpdates++;
                bool complete = _actor != null && _actor.View != null &&
                    _actor.View.Data != null &&
                    _actor.View.HandsEquipment != null &&
                    _actor.View.CharacterAvatar != null &&
                    _actor.Descriptor != null &&
                    _actor.Descriptor.Progression != null &&
                    _actor.Descriptor.Progression.Race != null;
                if (!complete)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(fixture.Label +
                        " did not materialize a complete native avatar.");
                }
                if (!_actorInitialized)
                {
                    _actor.Descriptor.State.Immortality.Retain();
                    _actor.Commands.InterruptAll(true);
                    if (_actor.CombatState.IsInCombat)
                        _actor.CombatState.LeaveCombat();
                    ClearHand(_actor, true);
                    ClearHand(_actor, false);
                    if (_actor.Body.Armor.HasArmor)
                        _actor.Body.Armor.RemoveItem(false);
                    if (_actor.Body.Shoulders.MaybeItem != null)
                        _actor.Body.Shoulders.RemoveItem(false);
                    _actor.View.HandsEquipment.UpdateAll();
                    _actor.View.HandsEquipment.ForceSwitch(false);
                    _actorInitialized = true;
                    _settleUpdates = 0;
                    return;
                }
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                Renderer[] renderers = ActiveRenderers(_actor);
                bool exact = _actor.Gender == fixture.Source.Gender &&
                    _actor.Descriptor.Progression.Race.RaceId == Race.Human &&
                    _actor.Descriptor.State.Size == Size.Medium &&
                    HasExactHumanoidRig(_actor.View.transform) &&
                    renderers.Length > 0;
                if (_settleUpdates < MinimumSettleUpdates || !exact)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(fixture.Label +
                        " did not settle the exact Human avatar contract.");
                }

                _avatar = _actor.View.CharacterAvatar;
                _avatarBefore = _avatar.EquipmentEntities
                    .Where(value => value != null)
                    .Select(value => new AvatarEntityState
                    {
                        Entity = value,
                        Primary = _avatar.GetPrimaryRampIndex(value),
                        Secondary = _avatar.GetSecondaryRampIndex(value)
                    }).ToArray();
                _savedLinksBefore = SavedLinks(_avatar);
                BlueprintCharacterClass reportedEquipmentClass =
                    _actor.Descriptor.Progression.GetEquipmentClass();
                BlueprintCharacterClass equipmentClass =
                    reportedEquipmentClass ?? BlueprintLibraryLookup
                        .RequireExact<BlueprintCharacterClass>(
                            BlueprintBootstrap.Library, FighterClassGuid,
                            "gunslinger-outfit-render-fighter-donor-class");
                _classEntities = equipmentClass.LoadClothes(_actor.Gender,
                        _actor.Descriptor.Progression.Race)
                    .Where(value => value != null).ToArray();
                int classPresentCount = _classEntities.Count(value =>
                    _avatarBefore.Any(original =>
                        ReferenceEquals(original.Entity, value)));
                if (_avatarBefore.Length == 0 || _classEntities.Length == 0)
                    throw new InvalidOperationException(fixture.Label +
                        " cannot resolve its exact native donor clothing.");

                _fixtureRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "sourceName", fixture.Source.name },
                    { "sourceGuid", fixture.Source.AssetGuid },
                    { "gender", _actor.Gender.ToString() },
                    { "raceName",
                        _actor.Descriptor.Progression.Race.name },
                    { "raceGuid",
                        _actor.Descriptor.Progression.Race.AssetGuid },
                    { "raceId",
                        _actor.Descriptor.Progression.Race.RaceId.ToString() },
                    { "equipmentClassName", equipmentClass.name },
                    { "equipmentClassGuid", equipmentClass.AssetGuid },
                    { "equipmentClassSource",
                        reportedEquipmentClass == null
                            ? "exact-fighter-fallback" : "progression" },
                    { "originalEntityCount", _avatarBefore.Length },
                    { "classEntityCount", _classEntities.Length },
                    { "classEntityPresentCount", classPresentCount },
                    { "originalEntities", new JArray(_avatarBefore.Select(
                        value => value.Entity.name + "/layer=" +
                            value.Entity.Layer).ToArray()) },
                    { "donorClassEntities", new JArray(
                        _classEntities.Select(value => value.name + "/layer=" +
                            value.Layer).ToArray()) },
                    { "rendererCount", renderers.Length },
                    { "rigExact", true }
                });
                _phase = 3;
                _settleUpdates = 0;
                WriteProgress("fixture-ready");
            }

            private void ApplyCandidate()
            {
                CandidateSpec spec = Candidates[_candidateIndex];
                _stage = "apply-" + _fixtures[_fixtureIndex].Label + "-" +
                    spec.Label;
                if (!RestoreAvatar())
                    throw new InvalidOperationException(
                        "Original avatar state was not exact before applying " +
                        spec.Label + ".");
                _avatar.RemoveEquipmentEntities(_classEntities, false);
                string[] ids = spec.For(_actor.Gender);
                _candidateEntities = ids.Select(id =>
                {
                    EquipmentEntity entity =
                        ResourcesLibrary.TryGetResource<EquipmentEntity>(
                            id, true);
                    if (entity == null)
                        throw new InvalidOperationException(spec.Label +
                            " did not resolve exact native entity " + id + ".");
                    _resolvedEntities++;
                    return entity;
                }).ToArray();
                _avatar.AddEquipmentEntities(_candidateEntities, false);
                ApplyPalette(spec, 0);
                _avatar.RebuildOutfit();
                _currentPalette = 0;
                WriteProgress("candidate-applied");
            }

            private void ApplyPalette(CandidateSpec spec, int paletteIndex)
            {
                var evidence = new JArray();
                int colorized = 0;
                foreach (EquipmentEntity entity in _candidateEntities)
                {
                    int primaryCount = entity.PrimaryRamps == null ? 0 :
                        entity.PrimaryRamps.Count;
                    int secondaryCount = entity.SecondaryRamps == null ? 0 :
                        entity.SecondaryRamps.Count;
                    int primary = primaryCount == 0 ? -1 :
                        paletteIndex == 0 ? spec.Primary :
                        (spec.Primary + 11) % primaryCount;
                    int secondary = secondaryCount == 0 ? -1 :
                        paletteIndex == 0 ? spec.Secondary :
                        (spec.Secondary + 17) % secondaryCount;
                    if (primary >= primaryCount ||
                        secondary >= secondaryCount)
                        throw new InvalidOperationException(spec.Label +
                            " requested an invalid color ramp.");
                    if (primary >= 0 && secondary >= 0)
                        _avatar.SetRampIndices(entity, primary, secondary,
                            false);
                    else if (primary >= 0)
                        _avatar.SetPrimaryRampIndex(entity, primary, false);
                    else if (secondary >= 0)
                        _avatar.SetSecondaryRampIndex(entity, secondary,
                            false);
                    if (primary >= 0 || secondary >= 0) colorized++;
                    evidence.Add(new JObject
                    {
                        { "entityName", entity.name },
                        { "layer", entity.Layer },
                        { "hideBodyParts",
                            entity.HideBodyParts.ToString() },
                        { "primaryRampCount", primaryCount },
                        { "secondaryRampCount", secondaryCount },
                        { "appliedPrimary", primary },
                        { "appliedSecondary", secondary }
                    });
                }
                if (colorized == 0)
                    throw new InvalidOperationException(spec.Label +
                        " has no colorized native entity.");
                _paletteEvidence = evidence;
                _paletteApplications++;
            }

            private void PollOutfitReadiness()
            {
                CandidateSpec spec = Candidates[_candidateIndex];
                _stage = "settle-outfit-" +
                    _fixtures[_fixtureIndex].Label + "-" + spec.Label;
                Game.Instance.EntityCreator.Tick();
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                _settleUpdates++;
                EquipmentEntity[] active = _avatar.EquipmentEntities
                    .Where(value => value != null).ToArray();
                bool allCandidate = _candidateEntities.All(value =>
                    active.Any(current => ReferenceEquals(current, value)));
                bool staleClass = _classEntities.Any(value =>
                    !_candidateEntities.Any(candidate =>
                        ReferenceEquals(candidate, value)) &&
                    active.Any(current => ReferenceEquals(current, value)));
                bool renderable = ActiveRenderers(_actor).Length > 0;
                if (_settleUpdates < MinimumSettleUpdates ||
                    !allCandidate || staleClass || !renderable)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(spec.Label +
                        " did not settle without stale donor clothing.");
                }
                _phase = 5;
                _settleUpdates = 0;
            }

            private void PrepareCase()
            {
                CandidateSpec spec = Candidates[_candidateIndex];
                RenderCase value = Cases[_caseIndex];
                _stage = "prepare-" + _fixtures[_fixtureIndex].Label +
                    "-" + spec.Label + "-" + value.Palette + "-" +
                    value.Weapon;
                RemoveWeapon();
                if (_currentPalette != value.PaletteIndex)
                {
                    ApplyPalette(spec, value.PaletteIndex);
                    _avatar.RebuildOutfit();
                    _currentPalette = value.PaletteIndex;
                    _phase = 4;
                    _settleUpdates = 0;
                    return;
                }
                ClearHand(_actor, true);
                ClearHand(_actor, false);
                if (value.Weapon != "no-weapon")
                {
                    BlueprintItemWeapon blueprint = value.Weapon == "pistol"
                        ? BlueprintBootstrap.ProductionFirearms.Pistol.Item
                        : BlueprintBootstrap.ProductionFirearms.Musket.Item;
                    if (blueprint == null)
                        throw new InvalidOperationException(
                            value.Weapon + " production blueprint is absent.");
                    _weapon = new ItemEntityWeapon(blueprint);
                    _actor.Body.PrimaryHand.InsertItem(_weapon);
                    if (!ReferenceEquals(
                            _actor.Body.PrimaryHand.MaybeWeapon, _weapon))
                        throw new InvalidOperationException(value.Weapon +
                            " did not remain in the disposable primary hand.");
                    FirearmRuntimeState.Service.Set(_weapon,
                        new FirearmState(FirearmState.CurrentSchemaVersion,
                            1, FirearmStateTokenCatalog.DiagnosticLeadBall,
                            FirearmCondition.Normal));
                    _firearmStateSet = true;
                }
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(
                    value.Weapon != "no-weapon");
                _phase = 6;
                _settleUpdates = 0;
            }

            private void PollCaseAndCapture()
            {
                CandidateSpec spec = Candidates[_candidateIndex];
                RenderCase value = Cases[_caseIndex];
                FixtureSpec fixture = _fixtures[_fixtureIndex];
                _stage = "capture-" + fixture.Label + "-" + spec.Label +
                    "-" + value.Palette + "-" + value.Weapon;
                Game.Instance.EntityCreator.Tick();
                if (_actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                _actor.View.HandsEquipment.UpdateAll();
                _settleUpdates++;
                GameObject weaponModel = _actor.View.HandsEquipment
                    .GetWeaponModel(false);
                bool weaponReady = value.Weapon == "no-weapon"
                    ? weaponModel == null
                    : Renderable(weaponModel == null ? null :
                        weaponModel.transform);
                bool heldState = value.Weapon == "no-weapon"
                    ? !_actor.View.HandsEquipment.InCombat
                    : _actor.View.HandsEquipment.InCombat;
                Renderer[] renderers = ActiveRenderers(_actor);
                if (_settleUpdates < MinimumSettleUpdates ||
                    !weaponReady || !heldState || renderers.Length == 0)
                {
                    if (_settleUpdates < MaximumSettleUpdates) return;
                    throw new InvalidOperationException(_stage +
                        " did not settle the exact weapon/avatar state.");
                }

                CaptureRecord(spec, value, fixture, renderers);
                _phase = 7;
                _settleUpdates = 0;
                WriteProgress("captured");
            }

            private void CaptureRecord(CandidateSpec spec, RenderCase value,
                FixtureSpec fixture, Renderer[] renderers)
            {
                string stem = SafeFileName(fixture.Label + "-" +
                    spec.Label + "-" + value.Palette + "-" + value.Weapon);
                string previewPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-preview.png");
                string isometricPath = Path.Combine(
                    _request.EvidenceDirectory, stem + "-isometric.png");
                WeaponPresentationEvidenceScenario.CaptureSummary preview =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        _actor, null, renderers, previewPath, true);
                IsometricCapture isometric = CaptureIsometric(
                    _actor, renderers, isometricPath);
                BlueprintItemWeapon weaponBlueprint = _weapon == null ? null :
                    _weapon.Blueprint;
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "candidateSetId", CandidateSetId() },
                    { "candidateId", spec.Label },
                    { "assetIds", new JArray(spec.For(_actor.Gender)) },
                    { "fixture", fixture.Label },
                    { "sourceName", fixture.Source.name },
                    { "sourceGuid", fixture.Source.AssetGuid },
                    { "gender", _actor.Gender.ToString() },
                    { "raceName",
                        _actor.Descriptor.Progression.Race.name },
                    { "raceGuid",
                        _actor.Descriptor.Progression.Race.AssetGuid },
                    { "raceId",
                        _actor.Descriptor.Progression.Race.RaceId.ToString() },
                    { "palette", value.Palette },
                    { "paletteEvidence", _paletteEvidence.DeepClone() },
                    { "weaponState", value.Weapon },
                    { "weaponName", weaponBlueprint == null ? "<none>" :
                        weaponBlueprint.name },
                    { "weaponGuid", weaponBlueprint == null ? "<none>" :
                        weaponBlueprint.AssetGuid },
                    { "heldStateExact", true },
                    { "activeRendererCount", renderers.Length },
                    { "preview", new JObject
                        {
                            { "file", Path.GetFileName(preview.PngPath) },
                            { "bytes", preview.Bytes },
                            { "sha256", preview.Sha256 },
                            { "meaningfulPixels",
                                preview.MeaningfulPixels },
                            { "framing", preview.Framing },
                            { "lowPixelDensity",
                                preview.LowPixelDensity },
                            { "views", new JArray("front", "right-side",
                                "rear", "front-right-three-quarter") }
                        }
                    },
                    { "isometric", new JObject
                        {
                            { "file", Path.GetFileName(isometric.Path) },
                            { "bytes", isometric.Bytes },
                            { "sha256", isometric.Sha256 },
                            { "meaningfulPixels",
                                isometric.MeaningfulPixels },
                            { "rendererCount",
                                isometric.RendererCount },
                            { "bounds", isometric.Bounds },
                            { "framing", isometric.Framing },
                            { "lowPixelDensity",
                                isometric.LowPixelDensity },
                            { "view", "elevated-front-right-isometric" }
                        }
                    },
                    { "claimBoundary",
                        "installed-game native clothing visual evidence on a " +
                        "request-local disposable Human avatar; aesthetic " +
                        "acceptance requires direct image inspection" }
                };
                string jsonPath = Path.Combine(
                    _request.EvidenceDirectory, stem + ".json");
                WriteJsonAtomic(jsonPath, record);
                _records.Add(record);
                _evidenceFiles.Add(preview.PngPath);
                _evidenceFiles.Add(isometric.Path);
                _evidenceFiles.Add(jsonPath);
                _captured++;
                _imageCount += 2;
                _viewCount += 5;
            }

            private void AdvanceCase()
            {
                RemoveWeapon();
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _caseIndex++;
                if (_caseIndex < Cases.Length)
                {
                    _phase = 5;
                    return;
                }

                CandidateSpec spec = Candidates[_candidateIndex];
                bool restored = RestoreAvatar();
                _restorationRecords.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "candidateId", spec.Label },
                    { "restored", restored },
                    { "originalEntityCount", _avatarBefore.Length },
                    { "restoredEntityCount",
                        _avatar.EquipmentEntities.Count },
                    { "savedLinksUnchanged",
                        _savedLinksBefore.SequenceEqual(
                            SavedLinks(_avatar),
                            StringComparer.Ordinal) }
                });
                if (restored) _restorations++;
                _candidateIndex++;
                _caseIndex = 0;
                _currentPalette = -1;
                _candidateEntities = new EquipmentEntity[0];
                if (_candidateIndex < Candidates.Length)
                {
                    _phase = 3;
                    return;
                }

                RetireActor();
                _fixtureIndex++;
                _candidateIndex = 0;
                if (_fixtureIndex < _fixtures.Length)
                {
                    _phase = 1;
                    _settleUpdates = 0;
                    return;
                }
                WriteIndex();
                _indexWritten = true;
                BeginCleanup();
            }

            private bool RestoreAvatar()
            {
                if (_avatar == null || _avatarBefore.Length == 0)
                    return false;
                RemoveWeapon();
                _avatar.RemoveAllEquipmentEntities(false);
                foreach (AvatarEntityState state in _avatarBefore)
                    _avatar.AddEquipmentEntity(state.Entity, false);
                foreach (AvatarEntityState state in _avatarBefore)
                    if (state.Primary >= 0 && state.Secondary >= 0)
                        _avatar.SetRampIndices(state.Entity, state.Primary,
                            state.Secondary, false);
                    else if (state.Primary >= 0)
                        _avatar.SetPrimaryRampIndex(state.Entity,
                            state.Primary, false);
                    else if (state.Secondary >= 0)
                        _avatar.SetSecondaryRampIndex(state.Entity,
                            state.Secondary, false);
                _avatar.RebuildOutfit();
                return AvatarMatchesSnapshot();
            }

            private bool AvatarMatchesSnapshot()
            {
                EquipmentEntity[] current = _avatar.EquipmentEntities
                    .Where(value => value != null).ToArray();
                bool exactOrder = current.Length == _avatarBefore.Length &&
                    current.Select((value, index) =>
                        ReferenceEquals(value,
                            _avatarBefore[index].Entity)).All(value => value);
                bool exactRamps = _avatarBefore.All(state =>
                    _avatar.GetPrimaryRampIndex(state.Entity) ==
                        state.Primary &&
                    _avatar.GetSecondaryRampIndex(state.Entity) ==
                        state.Secondary);
                return exactOrder && exactRamps &&
                    _savedLinksBefore.SequenceEqual(SavedLinks(_avatar),
                        StringComparer.Ordinal);
            }

            private static string[] SavedLinks(Character avatar)
            {
                return avatar.SavedEquipmentEntities
                    .Select(value => value == null ? "<null>" :
                        value.AssetId ?? string.Empty).ToArray();
            }

            private void RemoveWeapon()
            {
                if (_actor != null && _actor.Body != null &&
                    _actor.Body.PrimaryHand != null &&
                    _actor.Body.PrimaryHand.MaybeItem != null)
                    _actor.Body.PrimaryHand.RemoveItem(false);
                if (_weapon != null)
                {
                    if (_firearmStateSet)
                        FirearmRuntimeState.Service.Forget(_weapon);
                    _weapon.Dispose();
                }
                _weapon = null;
                _firearmStateSet = false;
            }

            private void RetireActor()
            {
                _stage = "retire-" + _fixtures[_fixtureIndex].Label;
                try
                {
                    if (_avatar != null && _avatarBefore.Length > 0)
                        RestoreAvatar();
                }
                finally
                {
                    RemoveWeapon();
                    if (_actor != null)
                    {
                        _actor.Commands.InterruptAll(true);
                        if (_actor.CombatState.IsInCombat)
                            _actor.CombatState.LeaveCombat();
                        if (_actor.Descriptor != null)
                            _actor.Descriptor.State.Immortality.ReleaseAll();
                        if (ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        _actor.Dispose();
                    }
                    if (_actorBlueprint != null)
                        UnityEngine.Object.DestroyImmediate(_actorBlueprint);
                    _actor = null;
                    _actorBlueprint = null;
                    _avatar = null;
                    _avatarBefore = new AvatarEntityState[0];
                    _savedLinksBefore = new string[0];
                    _classEntities = new EquipmentEntity[0];
                    _candidateEntities = new EquipmentEntity[0];
                    _actorInitialized = false;
                }
            }

            private void WriteIndex()
            {
                _stage = "write-outfit-render-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "candidateSetId", CandidateSetId() },
                    { "loadedModVersion", _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _assemblySha256 },
                    { "gameAssemblyMvid", _assemblyMvid },
                    { "candidates", new JArray(Candidates.Select(
                        value => value.Describe())) },
                    { "renderCases", new JArray(Cases.Select(value =>
                        new JObject
                        {
                            { "palette", value.Palette },
                            { "weapon", value.Weapon }
                        })) },
                    { "fixtures", _fixtureRecords },
                    { "restorations", _restorationRecords },
                    { "records", _records },
                    { "saveApiCalled", false },
                    { "productionBlueprintMutated", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-candidate-render-index.json");
                WriteJsonAtomic(path, index);
                _evidenceFiles.Add(path);
            }

            private void WriteProgress(string progressStage)
            {
                var progress = new JObject
                {
                    { "schemaVersion", 1 },
                    { "utc", DateTime.UtcNow.ToString("o") },
                    { "stage", progressStage },
                    { "detailStage", _stage },
                    { "fixtureIndex", _fixtureIndex },
                    { "candidateIndex", _candidateIndex },
                    { "caseIndex", _caseIndex },
                    { "phase", _phase },
                    { "captured", _captured },
                    { "imageCount", _imageCount },
                    { "actorPresent", _actor != null }
                };
                WriteJsonAtomic(Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-candidate-render-progress.json"),
                    progress);
            }

            private void BeginCleanup()
            {
                if (_cleanupStarted) return;
                _stage = "gunslinger-outfit-render-cleanup";
                try
                {
                    if (_actor != null || _actorBlueprint != null)
                        RetireActor();
                }
                catch (Exception cleanupException)
                {
                    _diagnostics.Add("cleanupException=" + cleanupException);
                    try
                    {
                        if (_actor != null &&
                            ContainsReference(_allUnits, _actor))
                            Game.Instance.State.Units.All.Remove(_actor);
                        if (_actor != null) _actor.Dispose();
                        if (_actorBlueprint != null)
                            UnityEngine.Object.DestroyImmediate(
                                _actorBlueprint);
                    }
                    catch (Exception fallbackException)
                    {
                        _diagnostics.Add("cleanupFallbackException=" +
                            fallbackException);
                    }
                    _actor = null;
                    _actorBlueprint = null;
                    _avatar = null;
                }
                _cleanupStarted = true;
                _settleUpdates = 0;
                WriteProgress("cleanup-started");
            }

            private void PollCleanup()
            {
                Game.Instance.EntityCreator.Tick();
                bool cleaned = SameReferences(_unitsBefore,
                        Snapshot(_allUnits)) &&
                    SameReferences(_partyBefore, Snapshot(_party)) &&
                    _actor == null;
                _settleUpdates++;
                if (!cleaned && _settleUpdates < MaximumSettleUpdates) return;
                Finish(cleaned);
            }

            private void Finish(bool cleaned)
            {
                const int expectedFixtures = 2;
                const int expectedCandidatesPerFixture = 6;
                const int expectedRecords = 48;
                const int expectedImages = 96;
                const int expectedRestorations = 12;
                const int expectedResolvedEntities = 32;
                JObject[] records = _records.OfType<JObject>().ToArray();
                bool perCandidate = Candidates.All(spec =>
                    records.Count(value => string.Equals(
                        (string)value["candidateId"], spec.Label,
                        StringComparison.Ordinal)) == 8);
                bool exactCases = Candidates.All(spec =>
                    _fixtures.All(fixture =>
                        records.Count(value => string.Equals(
                            (string)value["candidateId"], spec.Label,
                            StringComparison.Ordinal) &&
                        string.Equals((string)value["fixture"],
                            fixture.Label, StringComparison.Ordinal)) == 4));

                Add(_assertions, "gunslinger-outfit-render-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitCandidateRender,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitCandidateRender,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions, "gunslinger-outfit-render-save-boundary",
                    "KMG_AUTOMATION_WORKING; no save API",
                    "saveName=" + (_request.Parameters == null ? "<null>" :
                        _request.Parameters.Value<string>("saveName")) +
                        ";saveApiCalled=false",
                    _request.Parameters != null &&
                        string.Equals(_request.Parameters.Value<string>(
                            "saveName"), "KMG_AUTOMATION_WORKING",
                            StringComparison.Ordinal),
                    "guarded working-save load plus disposable actors");
                Add(_assertions, "gunslinger-outfit-render-game-identity",
                    "Kingmaker 2.1.7b exact Assembly-CSharp SHA-256 and MVID",
                    "sha256=" + _assemblySha256 + ";mvid=" +
                        _assemblyMvid,
                    string.Equals(_assemblySha256,
                        ExpectedAssemblySha256, StringComparison.Ordinal) &&
                    string.Equals(_assemblyMvid, ExpectedAssemblyMvid,
                        StringComparison.OrdinalIgnoreCase),
                    "live loaded Assembly-CSharp identity");
                Add(_assertions, "gunslinger-outfit-render-catalog",
                    "six ordered audited candidates; 32 exact M/F native IDs",
                    "candidates=" + Candidates.Length +
                        ";resolved=" + _resolvedEntities +
                        ";candidateSetId=" + CandidateSetId(),
                    Candidates.Length == expectedCandidatesPerFixture &&
                        _resolvedEntities == expectedResolvedEntities &&
                        UniqueCandidateIds().Length ==
                            expectedResolvedEntities,
                    "guarded catalog checkpoint and native link order");
                Add(_assertions, "gunslinger-outfit-render-human-fixtures",
                    "one exact male Human and one exact female Human fixture",
                    "fixtures=" + _fixtureRecords.Count,
                    _fixtureRecords.Count == expectedFixtures &&
                        _fixtureRecords.OfType<JObject>().All(value =>
                            string.Equals((string)value["raceId"], "Human",
                                StringComparison.Ordinal) &&
                            (bool)value["rigExact"]),
                    "native BlueprintUnit view, progression race, and rig");
                Add(_assertions, "gunslinger-outfit-render-palettes",
                    "two valid deterministic palettes per candidate/gender",
                    "applications=" + _paletteApplications,
                    _paletteApplications == 24 &&
                        records.All(value => ((JArray)value[
                            "paletteEvidence"]).OfType<JObject>().Any(row =>
                                (int)row["appliedPrimary"] >= 0 ||
                                (int)row["appliedSecondary"] >= 0)),
                    "live ramp counts and SetRampIndices(saved:false)");
                Add(_assertions, "gunslinger-outfit-render-captures",
                    "48 sidecars, 96 PNGs, 240 views, no blank images",
                    "records=" + records.Length + ";images=" +
                        _imageCount + ";views=" + _viewCount +
                        ";files=" + _evidenceFiles.Count,
                    records.Length == expectedRecords &&
                        _captured == expectedRecords &&
                        _imageCount == expectedImages &&
                        _viewCount == 240 && perCandidate && exactCases &&
                        _indexWritten &&
                        _evidenceFiles.Count == 145 &&
                        _evidenceFiles.All(File.Exists) &&
                        records.All(value =>
                            (int)value["preview"]["meaningfulPixels"] > 0 &&
                            (int)value["isometric"][
                                "meaningfulPixels"] > 0),
                    "four-view preview sheets plus elevated isometric captures");
                Add(_assertions, "gunslinger-outfit-render-restoration",
                    "exact entity order, ramps, and saved links restored after every candidate",
                    "restored=" + _restorations + "/" +
                        expectedRestorations,
                    _restorations == expectedRestorations &&
                        _restorationRecords.Count ==
                            expectedRestorations &&
                        _restorationRecords.OfType<JObject>().All(value =>
                            (bool)value["restored"] &&
                            (bool)value["savedLinksUnchanged"]),
                    "Character snapshot and saved:false add/remove/rebuild");
                Add(_assertions, "gunslinger-outfit-render-cleanup",
                    "exact party/global-unit snapshots restored; no save call",
                    "cleaned=" + cleaned + ";updates=" +
                        _settleUpdates, cleaned,
                    "request-local actor, item, camera, and texture cleanup");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("The scenario proves native resolution, valid " +
                    "ramps, restoration, and honest live rendering. Direct " +
                    "image inspection remains authoritative for aesthetics.");
                _warnings.Add("This Human M/F batch selects finalists. Full " +
                    "race, override, animation, and persistence qualification " +
                    "follows only for those finalists.");
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
                        build.LoadedModuleSha256 + "; pid=" +
                        build.ProcessId,
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
    }
}
