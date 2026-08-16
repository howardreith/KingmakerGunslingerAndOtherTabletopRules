using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.UrbanBarbarian;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class UrbanBarbarianAllocationIconSet
    {
        private readonly IDictionary<ControlledRageAllocation, Sprite> _icons;
        private readonly IDictionary<ControlledRageAllocation, Sprite> _selected;

        internal UrbanBarbarianAllocationIconSet(
            IDictionary<ControlledRageAllocation, Sprite> icons,
            IDictionary<ControlledRageAllocation, Sprite> selected)
        {
            _icons = icons; _selected = selected;
        }

        internal Sprite Require(ControlledRageAllocation allocation)
        {
            Sprite result;
            if (allocation == null || !_icons.TryGetValue(allocation, out result) ||
                result == null) throw new InvalidOperationException(
                    "Controlled Rage allocation icon is unavailable.");
            return result;
        }

        internal Sprite RequireSelected(ControlledRageAllocation allocation)
        {
            Sprite result;
            if (allocation == null || !_selected.TryGetValue(allocation,
                    out result) || result == null)
                throw new InvalidOperationException(
                    "Selected Controlled Rage allocation icon is unavailable.");
            return result;
        }

        internal IDictionary<ControlledRageAllocation, Sprite> Icons {
            get { return new Dictionary<ControlledRageAllocation, Sprite>(_icons); }
        }
        internal IDictionary<ControlledRageAllocation, Sprite> SelectedIcons {
            get { return new Dictionary<ControlledRageAllocation, Sprite>(_selected); }
        }
    }

    internal static class UrbanBarbarianAllocationIcons
    {
        internal const string StrengthDonorGuid =
            "4c3d08935262b6544ae97599b3a9556d";
        internal const string DexterityDonorGuid =
            "de7a025d48ad5da4991e7d3c682cf69d";
        internal const string ConstitutionDonorGuid =
            "a900628aea19aa74aad0ece0e65d091a";
        private const int Size = 128;
        private static readonly List<Texture2D> OwnedTextures =
            new List<Texture2D>();
        private static readonly List<Sprite> OwnedSprites = new List<Sprite>();
        private static Texture2D _solid;

        internal static UrbanBarbarianAllocationIconSet Create(
            LibraryScriptableObject library,
            IEnumerable<ControlledRageAllocation> allocations)
        {
            if (library == null) throw new ArgumentNullException("library");
            ControlledRageAllocation[] values = allocations == null ?
                new ControlledRageAllocation[0] : allocations.ToArray();
            if (values.Length != 31 || values.Distinct().Count() != 31)
                throw new ArgumentException(
                    "All 31 Controlled Rage allocations are required.",
                    "allocations");
            var donors = new Dictionary<ControlledRageIconGlyph, Sprite> {
                { ControlledRageIconGlyph.Strength,
                    RequireDonor(library, StrengthDonorGuid, "Bull's Strength") },
                { ControlledRageIconGlyph.Dexterity,
                    RequireDonor(library, DexterityDonorGuid, "Cat's Grace") },
                { ControlledRageIconGlyph.Constitution,
                    RequireDonor(library, ConstitutionDonorGuid,
                        "Bear's Endurance") }
            };
            var icons = new Dictionary<ControlledRageAllocation, Sprite>();
            var selected = new Dictionary<ControlledRageAllocation, Sprite>();
            foreach (ControlledRageAllocation allocation in values)
            {
                ControlledRageIconSpec spec =
                    ControlledRageIconPolicy.Describe(allocation);
                Sprite icon = spec.UsesNativeDonor ? donors[spec.NativeDonor] :
                    Compose("KMG_ControlledRage_" + spec.Key,
                        spec.Glyphs.Select(glyph => donors[glyph]).ToArray(),
                        false);
                icons.Add(allocation, icon);
                selected.Add(allocation, Compose(
                    "KMG_ControlledRage_Selected_" + spec.Key,
                    spec.UsesNativeDonor ? new[] { icon } :
                        spec.Glyphs.Select(glyph => donors[glyph]).ToArray(),
                    true));
            }
            foreach (ControlledRageTier tier in new[] {
                ControlledRageTier.Ordinary, ControlledRageTier.Greater,
                ControlledRageTier.Mighty })
                if (ControlledRageAllocationPolicy.Generate(tier).Select(value =>
                        icons[value]).Distinct().Count() !=
                    ControlledRageAllocationPolicy.Generate(tier).Count)
                    throw new InvalidOperationException(
                        "Visible Controlled Rage icons collide at tier " + tier + ".");
            return new UrbanBarbarianAllocationIconSet(icons, selected);
        }

        private static Sprite RequireDonor(LibraryScriptableObject library,
            string guid, string label)
        {
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, guid, label + " icon donor");
            if (donor.Icon == null || donor.Icon.texture == null)
                throw new InvalidOperationException(
                    label + " did not expose a runtime icon.");
            return donor.Icon;
        }

        private static Sprite Compose(string name, Sprite[] tiles,
            bool selected)
        {
            if (tiles == null || tiles.Length < 1 || tiles.Length > 4 ||
                tiles.Any(value => value == null || value.texture == null))
                throw new ArgumentException(
                    "A two-, three-, or four-glyph icon source is required.",
                    "tiles");
            RenderTexture prior = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(Size, Size, 0,
                RenderTextureFormat.ARGB32);
            try
            {
                RenderTexture.active = target;
                GL.PushMatrix();
                try
                {
                    GL.LoadPixelMatrix(0f, Size, Size, 0f);
                    GL.Clear(true, true, new Color(0.025f, 0.025f, 0.025f, 1f));
                    Rect[] layout = Layout(tiles.Length);
                    for (int index = 0; index < tiles.Length; index++)
                        DrawSprite(layout[index], tiles[index]);
                    for (int index = 1; index < tiles.Length; index++)
                        DrawSolid(new Rect(layout[index].x - 2f, 0f, 4f,
                            Size), new Color(0.02f, 0.02f, 0.02f, 1f));
                    if (selected) DrawSelectedMark();
                }
                finally { GL.PopMatrix(); }
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32,
                    false, false);
                texture.name = name + "_Texture";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
                texture.ReadPixels(new Rect(0f, 0f, Size, Size), 0, 0);
                texture.Apply(false, false);
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
                sprite.name = name;
                sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
                OwnedTextures.Add(texture); OwnedSprites.Add(sprite);
                return sprite;
            }
            finally
            {
                RenderTexture.active = prior;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static Rect[] Layout(int count)
        {
            float width = Size / (float)count;
            return Enumerable.Range(0, count).Select(index =>
                new Rect(index * width, 0f, width, Size)).ToArray();
        }

        private static void DrawSprite(Rect destination, Sprite sprite)
        {
            Rect sourcePixels = sprite.textureRect;
            var source = new Rect(sourcePixels.x / sprite.texture.width,
                sourcePixels.y / sprite.texture.height,
                sourcePixels.width / sprite.texture.width,
                sourcePixels.height / sprite.texture.height);
            Graphics.DrawTexture(destination, sprite.texture, source,
                0, 0, 0, 0, Color.white);
        }

        private static void DrawSelectedMark()
        {
            Color green = new Color(0.18f, 1f, 0.28f, 1f);
            DrawSolid(new Rect(0f, 0f, Size, 6f), green);
            DrawSolid(new Rect(0f, Size - 6f, Size, 6f), green);
            DrawSolid(new Rect(0f, 0f, 6f, Size), green);
            DrawSolid(new Rect(Size - 6f, 0f, 6f, Size), green);
            DrawSolid(new Rect(88f, 95f, 9f, 20f), green);
            DrawSolid(new Rect(96f, 105f, 9f, 13f), green);
            DrawSolid(new Rect(104f, 88f, 9f, 28f), green);
            DrawSolid(new Rect(112f, 78f, 9f, 23f), green);
        }

        private static void DrawSolid(Rect destination, Color color)
        {
            if (_solid == null)
            {
                _solid = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _solid.name = "KMG_ControlledRage_Solid";
                _solid.SetPixel(0, 0, Color.white);
                _solid.Apply(false, true);
                _solid.hideFlags = HideFlags.DontUnloadUnusedAsset;
                OwnedTextures.Add(_solid);
            }
            Graphics.DrawTexture(destination, _solid, new Rect(0f, 0f, 1f, 1f),
                0, 0, 0, 0, color);
        }
    }
}
