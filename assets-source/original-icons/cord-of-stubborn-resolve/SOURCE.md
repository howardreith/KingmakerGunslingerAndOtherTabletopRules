# Cord of Stubborn Resolve icon source

## Active Round 2 source

The active source is
`cord-of-stubborn-resolve-oblique-source.png`, an original project-owned
1672x941 RGBA image created on 2026-08-29 with the built-in OpenAI image
generation tool. SHA-256:
`54bb3426f8cd651758c6bce733904045fb30a84dd7b452d72bdf111abeb481e1`.

Reference roles were explicit:

- the earlier circular Cord source supplied only the red-brown braided-rope
  material and item identity;
- the two supplied native belt screenshots supplied only shallow-oblique
  perspective, depth, and belt-slot canvas grammar;
- no native texture, buckle, UI cell, or screenshot pixel was copied into the
  generated source or runtime asset.

The final built-in prompt was:

```text
Use case: stylized-concept
Asset type: 128x128 fantasy CRPG belt-slot inventory icon; create a new original high-resolution source.
Input images: Image 1 is a material and identity reference only for the dark red-brown braided cord; Images 2 and 3 are perspective, depth, and canvas-use references only for native belt-slot spatial grammar. Do not copy their pixels, designs, buckles, UI cells, or backgrounds.
Primary request: Reconstruct the Cord of Stubborn Resolve as a waist-worn braided cord belt in a shallow front three-quarter view.
Subject: A humble but magically resilient belt made specifically from several tightly braided red and dark brown rope strands. The rear segment recedes upward and backward; the front segment is lower, brighter, and visibly in front. At front center, use a deliberate cord-specific sailor-like binding knot/overlap with two short tapered hanging cord ends, so it is not an unbroken loop.
Style/medium: Original polished hand-painted fantasy CRPG inventory art, slightly painterly, crisp readable silhouette, restrained detail and soft antialiased edges.
Composition/framing: Square transparent canvas. Object is horizontally dominant, clearly wider than tall, target nontransparent silhouette about 1.8:1 to 2.2:1. Shallow oblique/front view, slight diagonal cant only, centered with comfortable transparent padding and native belt-icon scale. Strong front/rear depth and foreshortening. It must read as a belt worn around a waist, never as a ring lying flat.
Lighting/mood: Soft warm upper-left light, deeper shadow under the front segment and knot, subtle soft cast shadow contained on transparency, restrained ember-red magical glints.
Color palette: Weathered crimson, oxblood, dark umber, muted warm copper highlights; no bright green.
Materials/textures: Clearly braided fibrous cord/rope, supple and narrower than a leather belt, visible strand texture at small size.
Constraints: genuinely transparent background and transparent corners; no baked inventory cell, frame, square, circle, scenery, text, logo, character, body, hands, gems, ornate gold, or watermark. Preserve the identity as a braided cord, not a generic thick leather belt. Do not make a top-down circular or near-circular loop. Do not merely squash Image 1 into an ellipse. No native-game pixels or copied native belt construction.
```

The source alpha bounds are `x=106,y=69,w=1460,h=809`, an aspect of
`1.8047:1`. It has genuinely transparent corners.

## Deterministic export

`tools/New-CordOfStubbornResolveIcon.ps1` reads the active alpha source,
finds its visible bounds, validates a belt-like aspect, fits it once into a
128x128 transparent canvas with a six-pixel horizontal safety margin, and
uses high-quality bicubic resampling:

```powershell
& .\tools\New-CordOfStubbornResolveIcon.ps1
```

The generator writes
`cord-of-stubborn-resolve-assets.json` with source/final dimensions, alpha
bounds, silhouette aspects, corner alpha, and SHA-256 values. The current
runtime output is `assets/game/icons/cord-of-stubborn-resolve.png`, SHA-256
`101e1b2fbd7083c5db20be1a0ee40840bc8201520dff83be0acd9bae06f91a6a`.
Its alpha bounds are `x=6,y=32,w=116,h=64`, aspect `1.8125:1`, and all
four corner alpha values are zero.

## Archived before source

`cord-of-stubborn-resolve-chroma-source.png` is retained only as immutable
pre-polish provenance. It is the rejected 1254x1254 circular/top-down source,
SHA-256
`d7e5dfa7228419df65e3bfa88aafa7b94caa1e5cfadfb1a159686805042655c8`.
The active generator and runtime asset no longer reference it.
