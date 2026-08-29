using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Produces deterministic full-resolution visual evidence from the exact
    /// sprites and menu rows loaded by Kingmaker. The output resembles the
    /// relevant native lists and inventory grids but never navigates UI and is
    /// supporting visual evidence only; live object-graph assertions remain
    /// the mechanical authority.
    /// </summary>
    internal static class IconOverhaulVisualEvidenceScenario
    {
        private const int Width = 1920;
        private const int Height = 1200;
        private const string NativeWeaponFocusGuid =
            "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        private const string BasicFeatSelectionGuid =
            "247a4068296e8be42890143f451b4b45";

        private sealed class Entry
        {
            internal Entry(string name, string detail, Sprite icon,
                string identity)
            {
                Name = name ?? string.Empty;
                Detail = detail ?? string.Empty;
                Icon = icon;
                Identity = identity ?? string.Empty;
            }

            internal string Name { get; private set; }
            internal string Detail { get; private set; }
            internal Sprite Icon { get; private set; }
            internal string Identity { get; private set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");

            FirearmFeatBlueprintSet feats = BlueprintBootstrap.FirearmFeats;
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    BasicFeatSelectionGuid, "native basic feat selection");
            BlueprintParametrizedFeature weaponFocus =
                BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(
                    BlueprintBootstrap.Library, NativeWeaponFocusGuid,
                    "native Weapon Focus");

            Entry[] ordinary = SelectNativeFeatNeighbors(basic);
            Entry rapid = FromFeature(feats.RapidReload, "Project feat");
            Entry[] rapidChoices = feats.RapidReloadChoices
                .Select(value => FromFeature(value, "Rapid Reload choice"))
                .OrderBy(value => value.Name, StringComparer.Ordinal).ToArray();
            FeatureUIData[] firearmMenu = weaponFocus.GetFullSelectionItems()
                .Where(value => value != null && value.Param.Blueprint != null &&
                    value.Param.Blueprint.name.StartsWith("KMG_WeaponFocus_",
                        StringComparison.Ordinal))
                .OrderBy(value => value.Name, StringComparer.Ordinal).ToArray();
            Entry[] weaponFocusChoices = firearmMenu.Select(value => new Entry(
                value.Name, "Weapon Focus parameter", value.Icon,
                value.Param.Blueprint.name + ":" +
                    value.Param.Blueprint.AssetGuid)).ToArray();

            ProductionFirearmBlueprintEntry[] officialFirearms = {
                BlueprintBootstrap.ProductionFirearms.Blunderbuss,
                BlueprintBootstrap.ProductionFirearms.Musket,
                BlueprintBootstrap.ProductionFirearms.Pistol };
            Entry[] firearmItems = officialFirearms.Select(value =>
                    FromItem(value.Item, "Supported mundane firearm"))
                .Concat(BlueprintBootstrap.MagicFirearms.GenericEntries
                    .Where(value => OfficialFirearmSupport.IsOfficial(
                        value.Spec.Kind)).Select(value => FromItem(value.Item,
                        "Supported enchanted firearm")))
                .OrderBy(value => value.Name, StringComparer.Ordinal).ToArray();

            BlueprintItemWeapon[] easternAll = BlueprintBootstrap.EasternWeapons
                .Entries.Select(value => value.Item)
                .Concat(BlueprintBootstrap.EasternWeapons.Named.Entries.Select(
                    value => value.Item)).ToArray();
            BlueprintItemWeapon[] spearAll = BlueprintBootstrap.ElvenBranchedSpears
                .Entries.Select(value => value.Item)
                .Concat(BlueprintBootstrap.ElvenBranchedSpears.Named.Entries.Select(
                    value => value.Item)).ToArray();
            Entry[] easternAndSpear = SelectEasternAndSpearRepresentatives(
                easternAll, spearAll);

            string[] expected = { "Blunderbuss", "Musket", "Pistol" };
            string[] expectedIcons = {
                "KMG_Icon_firearm-monogram-blunderbuss",
                "KMG_Icon_firearm-monogram-musket",
                "KMG_Icon_firearm-monogram-pistol" };
            string[] expectedRapid = expected.Select(value =>
                "Rapid Reload (" + value + ")").ToArray();
            bool rapidExact = rapidChoices.Select(value => value.Name)
                    .SequenceEqual(expectedRapid) &&
                rapidChoices.Select(value => IconName(value.Icon))
                    .SequenceEqual(expectedIcons);
            bool weaponFocusExact = weaponFocusChoices.Select(value => value.Name)
                    .SequenceEqual(expected) &&
                weaponFocusChoices.Select(value => IconName(value.Icon))
                    .SequenceEqual(expectedIcons);
            bool itemsExact = firearmItems.Length == 6 &&
                firearmItems.All(value => value.Icon != null &&
                    value.Name.IndexOf("Rifle", StringComparison.OrdinalIgnoreCase) < 0 &&
                    value.Name.IndexOf("Revolver", StringComparison.OrdinalIgnoreCase) < 0);
            bool easternExact = easternAll.Length == 30 && spearAll.Length == 12 &&
                easternAll.Concat(spearAll).All(value => value != null &&
                    value.Icon != null);

            var records = new JArray();
            Render(request, records, "after-01-rapid-reload-feat-list.png",
                "FEATS", "Rapid Reload beside ordinary loaded native feats",
                scene => DrawFeatList(scene, ordinary.Concat(new[] { rapid })
                    .OrderBy(value => value.Name, StringComparer.Ordinal).ToArray(),
                    rapid.Identity));
            Render(request, records,
                "after-02-rapid-reload-supported-choices.png",
                "RAPID RELOAD", "Choose a supported firearm type",
                scene => DrawChoiceCards(scene, rapidChoices,
                    "3 SUPPORTED CHOICES  |  RIFLE ABSENT  |  REVOLVER ABSENT"));
            Render(request, records,
                "after-03-weapon-focus-firearm-choices.png",
                "WEAPON FOCUS", "Live firearm parameters appended to the native selector",
                scene => DrawChoiceCards(scene, weaponFocusChoices,
                    "EXACT LIVE MENU ROWS  |  NO LEGACY FIREARM PARAMETERS"));
            Render(request, records, "after-04-supported-firearm-items.png",
                "INVENTORY", "Supported firearm item icons loaded from their blueprints",
                scene => DrawInventory(scene, firearmItems));
            Render(request, records,
                "after-05-eastern-and-spear-items.png",
                "INVENTORY", "Eastern weapons and Elven Branched Spear representatives",
                scene => DrawInventory(scene, easternAndSpear));

            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            string indexPath = Path.Combine(request.EvidenceDirectory,
                "icon-overhaul-visual-index.json");
            var index = new JObject
            {
                { "schemaVersion", 1 },
                { "evidenceRole", "supporting visual evidence only" },
                { "renderWidth", Width },
                { "renderHeight", Height },
                { "loadedModVersion", context.ModEntry.Info.Version },
                { "runtimeIdentity", JObject.FromObject(identity) },
                { "rapidReloadChoices", EntriesJson(rapidChoices) },
                { "weaponFocusFirearmParameters", EntriesJson(weaponFocusChoices) },
                { "supportedFirearmItems", EntriesJson(firearmItems) },
                { "easternAndSpearRepresentatives", EntriesJson(easternAndSpear) },
                { "easternItemCount", easternAll.Length },
                { "spearItemCount", spearAll.Length },
                { "screenshots", records }
            };
            RuntimeTestResultWriter.WriteAtomic(indexPath,
                index.ToString(Formatting.Indented) + Environment.NewLine);

            var assertions = new List<RuntimeTestAssertion>();
            Add(assertions, "rapid-reload-live-choices",
                "Blunderbuss, Musket, Pistol with exact distinct monograms",
                Describe(rapidChoices), rapidExact,
                "live Rapid Reload AllFeatures and Sprite references");
            Add(assertions, "weapon-focus-live-firearm-parameters",
                "exactly Blunderbuss, Musket, Pistol; no Rifle or Revolver",
                Describe(weaponFocusChoices), weaponFocusExact,
                "BlueprintParametrizedFeature.GetFullSelectionItems");
            Add(assertions, "supported-firearm-item-icons",
                "three mundane and three enchanted supported firearm rows with icons",
                Describe(firearmItems), itemsExact,
                "live BlueprintItemWeapon icon references");
            Add(assertions, "eastern-and-spear-item-icons",
                "all 30 Eastern and all 12 spear variants have loaded icons",
                "eastern=" + easternAll.Length + ";spear=" + spearAll.Length,
                easternExact, "complete live item catalogs");
            Add(assertions, "full-resolution-visual-evidence",
                "five exact 1920x1200 PNG renders and one indexed manifest",
                string.Join("|", records.Select(value =>
                    (string)value["fileName"]).ToArray()),
                records.Count == 5 && records.All(value =>
                    (int)value["width"] == Width &&
                    (int)value["height"] == Height &&
                    (long)value["bytes"] > 0) && File.Exists(indexPath),
                "Unity Camera/RenderTexture output from loaded live sprites");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version, string.Equals(
                    request.ExpectedModVersion, context.ModEntry.Info.Version,
                    StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");

            bool pass = assertions.All(value => value.Status ==
                RuntimeTestStatuses.Pass);
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = identity.RuntimeIdentity + "; mvid=" +
                    identity.ModuleVersionId + "; sha256=" +
                    identity.LoadedModuleSha256 + "; pid=" + identity.ProcessId,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = new List<string> {
                    "visualIndex=" + indexPath,
                    "nativeFeatNeighbors=" + ordinary.Length,
                    "screenshots=" + records.Count },
                Warnings = new List<string> {
                    "The PNGs are deterministic in-game UI facsimiles from live sprites, not screenshots of automated native-menu navigation and not mechanical correctness evidence." },
                ExceptionSummary = string.Empty,
                EvidenceFiles = records.Select(value => Path.Combine(
                        request.EvidenceDirectory, (string)value["fileName"]))
                    .Concat(new[] { indexPath }).ToList(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static Entry[] SelectNativeFeatNeighbors(
            BlueprintFeatureSelection basic)
        {
            string[] preferred = { "Shield Proficiency", "Shake It Off",
                "Skill Focus", "Spell Focus", "Toughness", "Weapon Focus" };
            BlueprintFeature[] available = (basic.AllFeatures ??
                    Array.Empty<BlueprintFeature>()).Where(value => value != null &&
                    value.Icon != null && !string.IsNullOrWhiteSpace(value.Name) &&
                    (value.name == null || !value.name.StartsWith("KMG_",
                        StringComparison.Ordinal)))
                .GroupBy(value => value.AssetGuid, StringComparer.Ordinal)
                .Select(value => value.First()).ToArray();
            var selected = new List<BlueprintFeature>();
            foreach (string name in preferred)
            {
                BlueprintFeature match = available.FirstOrDefault(value =>
                    string.Equals(value.Name, name, StringComparison.Ordinal));
                if (match != null && !selected.Contains(match)) selected.Add(match);
            }
            foreach (BlueprintFeature value in available.OrderBy(value =>
                value.Name, StringComparer.Ordinal))
            {
                if (selected.Count >= 5) break;
                if (!selected.Contains(value)) selected.Add(value);
            }
            return selected.Take(5).Select(value => FromFeature(value,
                "Native feat")).ToArray();
        }

        private static Entry[] SelectEasternAndSpearRepresentatives(
            BlueprintItemWeapon[] eastern, BlueprintItemWeapon[] spear)
        {
            var selected = new List<Entry>();
            foreach (IGrouping<string, BlueprintItemWeapon> group in eastern
                .GroupBy(value => IconName(value.Icon), StringComparer.Ordinal)
                .OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                BlueprintItemWeapon value = group.OrderBy(item => item.Name,
                    StringComparer.Ordinal).First();
                selected.Add(FromItem(value, "Eastern weapon"));
            }
            foreach (BlueprintItemWeapon value in spear.OrderBy(item =>
                item.Name, StringComparer.Ordinal).Take(3))
                selected.Add(FromItem(value, "Elven Branched Spear"));
            return selected.Take(12).ToArray();
        }

        private static Entry FromFeature(BlueprintFeature value, string detail)
        {
            if (value == null) return new Entry("<missing>", detail, null,
                "<missing>");
            return new Entry(value.Name, detail, value.Icon,
                value.name + ":" + value.AssetGuid);
        }

        private static Entry FromItem(BlueprintItem value, string detail)
        {
            if (value == null) return new Entry("<missing>", detail, null,
                "<missing>");
            return new Entry(value.Name, detail, value.Icon,
                value.name + ":" + value.AssetGuid);
        }

        private static JArray EntriesJson(IEnumerable<Entry> entries)
        {
            return new JArray(entries.Select(value => new JObject
            {
                { "name", value.Name },
                { "detail", value.Detail },
                { "identity", value.Identity },
                { "icon", IconName(value.Icon) }
            }));
        }

        private static string Describe(IEnumerable<Entry> entries)
        {
            return string.Join("|", entries.Select(value => value.Name + "=" +
                IconName(value.Icon)).ToArray());
        }

        private static string IconName(Sprite icon)
        {
            return icon == null ? "<null>" : icon.name ?? "<unnamed>";
        }

        private static void Render(RuntimeTestRequest request, JArray records,
            string fileName, string title, string subtitle,
            Action<EvidenceScene> draw)
        {
            string path = Path.Combine(request.EvidenceDirectory, fileName);
            using (var scene = new EvidenceScene(title, subtitle))
            {
                draw(scene);
                scene.Render(path);
            }
            var info = new FileInfo(path);
            records.Add(new JObject
            {
                { "fileName", fileName },
                { "width", Width },
                { "height", Height },
                { "bytes", info.Length },
                { "sha256", Sha256(path) }
            });
        }

        private static void DrawFeatList(EvidenceScene scene, Entry[] entries,
            string rapidIdentity)
        {
            const float left = 300f;
            const float top = 190f;
            const float width = 1320f;
            const float rowHeight = 142f;
            scene.Panel(new Rect(left - 26f, top - 24f, width + 52f,
                rowHeight * entries.Length + 48f));
            for (int index = 0; index < entries.Length; index++)
            {
                Entry value = entries[index];
                float y = top + index * rowHeight;
                bool rapid = value.Identity == rapidIdentity;
                scene.Row(new Rect(left, y, width, rowHeight - 10f), rapid);
                scene.Icon(value.Icon, new Rect(left + 22f, y + 17f,
                    98f, 98f));
                scene.Text(value.Name, new Rect(left + 150f, y + 23f,
                    760f, 54f), 42f, EvidenceScene.Gold, false);
                scene.Text(value.Detail, new Rect(left + 150f, y + 77f,
                    760f, 34f), 25f, EvidenceScene.Parchment, false);
                if (rapid)
                    scene.Badge("RESTYLED PROJECT FEAT", new Rect(
                        left + 930f, y + 42f, 340f, 54f));
            }
        }

        private static void DrawChoiceCards(EvidenceScene scene, Entry[] entries,
            string footer)
        {
            scene.Panel(new Rect(160f, 220f, 1600f, 690f));
            const float cardWidth = 430f;
            const float gap = 70f;
            float start = (Width - (cardWidth * 3f + gap * 2f)) / 2f;
            for (int index = 0; index < entries.Length; index++)
            {
                float x = start + index * (cardWidth + gap);
                scene.Card(new Rect(x, 295f, cardWidth, 510f));
                scene.Icon(entries[index].Icon, new Rect(x + 105f, 350f,
                    220f, 220f));
                scene.Text(entries[index].Name, new Rect(x + 32f, 610f,
                    cardWidth - 64f, 68f), 45f, EvidenceScene.Gold, true);
                scene.Text(entries[index].Detail, new Rect(x + 32f, 692f,
                    cardWidth - 64f, 42f), 25f,
                    EvidenceScene.Parchment, true);
            }
            scene.Text(footer, new Rect(200f, 965f, 1520f, 54f), 28f,
                EvidenceScene.Parchment, true);
        }

        private static void DrawInventory(EvidenceScene scene, Entry[] entries)
        {
            scene.Panel(new Rect(100f, 180f, 1720f, 860f));
            const int columns = 4;
            const float cardWidth = 380f;
            const float cardHeight = 245f;
            const float gapX = 35f;
            const float gapY = 28f;
            float startX = 150f;
            float startY = 225f;
            for (int index = 0; index < entries.Length; index++)
            {
                int row = index / columns;
                int column = index % columns;
                float x = startX + column * (cardWidth + gapX);
                float y = startY + row * (cardHeight + gapY);
                scene.InventorySlot(new Rect(x, y, cardWidth, cardHeight));
                scene.Icon(entries[index].Icon, new Rect(x + 18f, y + 18f,
                    154f, 154f));
                scene.Text(entries[index].Name, new Rect(x + 190f, y + 42f,
                    165f, 72f), 28f, EvidenceScene.Gold, true);
                scene.Text(entries[index].Detail, new Rect(x + 190f, y + 128f,
                    165f, 48f), 20f, EvidenceScene.Parchment, true);
                scene.Text(IconName(entries[index].Icon).Replace("KMG_Icon_", ""),
                    new Rect(x + 20f, y + 190f, cardWidth - 40f, 30f),
                    16f, EvidenceScene.Muted, true);
            }
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
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

        private static string Sha256(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            using (SHA256 hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private sealed class EvidenceScene : IDisposable
        {
            internal static readonly Color Gold = Hex("D4BA78");
            internal static readonly Color Parchment = Hex("D8CCB1");
            internal static readonly Color Muted = Hex("918674");
            private static readonly Color Border = Hex("927848");
            private static readonly Color PanelColor = Hex("28231E");
            private static readonly Color RowColor = Hex("36302A");
            private static readonly Color Highlight = Hex("562D28");
            private const int EvidenceLayer = 30;

            private readonly GameObject _root;
            private readonly Camera _camera;
            private readonly RenderTexture _target;
            private readonly Texture2D _whiteTexture;
            private readonly Sprite _whiteSprite;
            private readonly Font _font;

            internal EvidenceScene(string title, string subtitle)
            {
                _root = new GameObject("KMG_IconOverhaulVisualEvidence");
                _root.layer = EvidenceLayer;
                _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32,
                    false);
                _whiteTexture.name = "KMG_RuntimeEvidenceWhitePixel";
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply(false, false);
                _whiteSprite = Sprite.Create(_whiteTexture,
                    new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                _whiteSprite.name = "KMG_RuntimeEvidenceWhiteSprite";
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_font == null)
                    throw new InvalidOperationException(
                        "Unity built-in Arial font is unavailable.");

                GameObject cameraObject = Child("Camera");
                _camera = cameraObject.AddComponent<Camera>();
                _camera.orthographic = true;
                _camera.orthographicSize = Height / 2f;
                _camera.aspect = Width / (float)Height;
                _camera.transform.position = new Vector3(0f, 0f, -100f);
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = Hex("151311");
                _camera.cullingMask = 1 << EvidenceLayer;
                _camera.nearClipPlane = 0.1f;
                _camera.farClipPlane = 200f;
                _target = new RenderTexture(Width, Height, 24,
                    RenderTextureFormat.ARGB32);
                _target.name = "KMG_IconOverhaulVisualEvidenceTarget";
                _target.antiAliasing = 1;
                _target.Create();
                _camera.targetTexture = _target;

                Rect(new Rect(42f, 30f, Width - 84f, Height - 60f),
                    Hex("211D19"), 0);
                Rect(new Rect(55f, 43f, Width - 110f, Height - 86f),
                    PanelColor, 1);
                Rect(new Rect(55f, 43f, Width - 110f, 104f),
                    Hex("4B2925"), 2);
                Text(title, new Rect(90f, 60f, 780f, 58f), 50f, Gold,
                    false);
                Text(subtitle, new Rect(880f, 70f, 920f, 40f), 27f,
                    Parchment, false);
                Text("KINGMAKER GUNSLINGER  |  LIVE RUNTIME SPRITES  |  1920 x 1200",
                    new Rect(100f, 1092f, 1720f, 35f), 20f, Muted, true);
            }

            internal void Panel(Rect rect)
            {
                Rect(new Rect(rect.x - 3f, rect.y - 3f,
                    rect.width + 6f, rect.height + 6f), Border, 3);
                Rect(rect, Hex("1B1815"), 4);
            }

            internal void Row(Rect rect, bool highlighted)
            {
                Rect(new Rect(rect.x - 2f, rect.y - 2f,
                    rect.width + 4f, rect.height + 4f),
                    highlighted ? Gold : Hex("514536"), 5);
                Rect(rect, highlighted ? Highlight : RowColor, 6);
            }

            internal void Card(Rect rect)
            {
                Rect(new Rect(rect.x - 4f, rect.y - 4f,
                    rect.width + 8f, rect.height + 8f), Border, 5);
                Rect(rect, RowColor, 6);
                Rect(new Rect(rect.x + 26f, rect.y + 30f,
                    rect.width - 52f, 292f), Hex("211D19"), 7);
            }

            internal void InventorySlot(Rect rect)
            {
                Rect(new Rect(rect.x - 3f, rect.y - 3f,
                    rect.width + 6f, rect.height + 6f), Border, 5);
                Rect(rect, Hex("332D27"), 6);
                Rect(new Rect(rect.x + 12f, rect.y + 12f, 166f, 166f),
                    Hex("1A1816"), 7);
            }

            internal void Badge(string text, Rect rect)
            {
                Rect(new Rect(rect.x - 2f, rect.y - 2f,
                    rect.width + 4f, rect.height + 4f), Gold, 20);
                Rect(rect, Hex("3B201E"), 21);
                Text(text, rect, 22f, Parchment, true);
            }

            internal void Icon(Sprite sprite, Rect box)
            {
                if (sprite == null)
                {
                    Rect(box, Hex("6A2424"), 30);
                    Text("MISSING", box, 20f, Color.white, true);
                    return;
                }
                GameObject owner = Child("Icon_" + IconName(sprite));
                SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 31;
                Vector2 size = sprite.bounds.size;
                float scale = size.x <= 0f || size.y <= 0f ? 1f :
                    Math.Min(box.width / size.x, box.height / size.y);
                owner.transform.localScale = new Vector3(scale, scale, 1f);
                owner.transform.position = Position(box.center.x,
                    box.center.y, 0f);
            }

            internal void Text(string value, Rect box, float height,
                Color color, bool centered)
            {
                GameObject owner = Child("Text");
                TextMesh mesh = owner.AddComponent<TextMesh>();
                mesh.font = _font;
                mesh.fontSize = 64;
                mesh.text = value ?? string.Empty;
                mesh.color = color;
                mesh.richText = false;
                mesh.anchor = centered ? TextAnchor.MiddleCenter :
                    TextAnchor.MiddleLeft;
                mesh.alignment = centered ? TextAlignment.Center :
                    TextAlignment.Left;
                MeshRenderer renderer = owner.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = _font.material;
                renderer.sortingOrder = 50;
                owner.transform.position = Position(centered ? box.center.x :
                    box.x, box.center.y, 0f);
                renderer.bounds.ToString();
                Vector3 measured = renderer.bounds.size;
                float scale = measured.y <= 0f ? 1f : height / measured.y;
                if (measured.x > 0f && measured.x * scale > box.width)
                    scale = box.width / measured.x;
                owner.transform.localScale = new Vector3(scale, scale, 1f);
            }

            internal void Render(string path)
            {
                _camera.Render();
                RenderTexture prior = RenderTexture.active;
                Texture2D output = null;
                try
                {
                    RenderTexture.active = _target;
                    output = new Texture2D(Width, Height,
                        TextureFormat.RGBA32, false);
                    output.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                    output.Apply(false, false);
                    byte[] png = EncodePng(output);
                    if (png == null || png.Length < 4096)
                        throw new InvalidOperationException(
                            "The icon-overhaul evidence PNG was empty.");
                    File.WriteAllBytes(path, png);
                }
                finally
                {
                    RenderTexture.active = prior;
                    if (output != null) UnityEngine.Object.DestroyImmediate(output);
                }
            }

            public void Dispose()
            {
                if (_camera != null) _camera.targetTexture = null;
                if (_target != null)
                {
                    _target.Release();
                    UnityEngine.Object.DestroyImmediate(_target);
                }
                if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
                if (_whiteSprite != null)
                    UnityEngine.Object.DestroyImmediate(_whiteSprite);
                if (_whiteTexture != null)
                    UnityEngine.Object.DestroyImmediate(_whiteTexture);
            }

            private void Rect(Rect rect, Color color, int order)
            {
                GameObject owner = Child("Rect");
                SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
                renderer.sprite = _whiteSprite;
                renderer.color = color;
                renderer.sortingOrder = order;
                owner.transform.position = Position(rect.center.x,
                    rect.center.y, 0f);
                owner.transform.localScale = new Vector3(rect.width,
                    rect.height, 1f);
            }

            private GameObject Child(string name)
            {
                var child = new GameObject(name);
                child.layer = EvidenceLayer;
                child.transform.SetParent(_root.transform, false);
                return child;
            }

            private static Vector3 Position(float x, float y, float z)
            {
                return new Vector3(x - Width / 2f, Height / 2f - y, z);
            }

            private static Color Hex(string value)
            {
                Color parsed;
                if (!ColorUtility.TryParseHtmlString("#" + value, out parsed))
                    throw new InvalidOperationException("Invalid color: " + value);
                return parsed;
            }
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
