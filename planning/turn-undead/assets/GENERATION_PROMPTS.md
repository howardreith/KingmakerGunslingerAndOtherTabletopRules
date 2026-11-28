# Turn Undead image-generation provenance

Prepared 2026-11-27. Two separate images were generated with the built-in `image_gen.imagegen` tool, one request for each concept. No specific underlying model version was exposed by the result, so none is asserted here. The requested square size was 1024; both actual returned masters are 1254×1254. Their byte-preserved originals are included in `assets/source/`.

The reference images supplied visual vocabulary and painting/emblem treatment. They are not redistributed in this ZIP. The generated concepts have distinct compositions and subjects. No existing native channel sprite was available to establish a pixel-exact native match.

## Feat image

- Original: `assets/source/turn-undead-feat-original.png`.
- Tool output basename: `exec-7dbd69ea-774f-498c-b2d9-18e88b58bf13.png`.
- Generation parameter: `transparent_background: true`.
- Reference inputs: `channel-feat-reference-large.png` and `rapid-reload.png`.
- The first input is a 512×512 nearest-neighbor enlargement/re-encoding of CotW's 64×64 `Icon_Channel_Extra.png`, used only to make the visual reference readable. The other is the project's approved 64×64 Rapid Reload emblem.
- Reference SHA-256 values and pinned source URLs are in `asset-manifest.json`.

### Exact prompt

```text
Use case: stylized-concept. Asset: an original Pathfinder Kingmaker Turn Undead FEAT emblem. Derive a NEW emblem from the two supplied reference images, which are STYLE REFERENCES ONLY; do not copy their pixels or existing symbol. Reference 1 shows the related channel-feat vocabulary (muted terracotta circular scaffold, slate-blue subordinate shape, parchment light); reference 2 shows the approved very thin circular emblem weight and transparent negative space.
Primary request: create one clean square 1024x1024 original feat icon that will be exported to 64x64 RGBA. A simple recognizable skull in right-facing profile recoils and turns away from a small radiant sun on the left, with two or three outward arcing rays showing repulsion. The icon must convey turning undead away, not commanding them and not destroying them. Keep the skull large and immediately recognizable with one dark eye socket and a simple jaw. Use a single thin terracotta circular scaffold behind the subject, like the reference family's internal ring; this is part of the emblem, not a square UI frame.
Style: flat restrained ink/silhouette work, slightly weathered hand-drawn edges, antique tabletop RPG feat glyph; broad low-detail shapes, muted rust/terracotta and pale warm parchment with a small slate-blue secondary accent. No photographic materials, no shiny gold medallion, no elaborate illustration. All main shapes must remain distinct at 32 and 64 pixels.
Composition: centered, balanced; important silhouette within the central 82 percent; ring within 86 percent. Transparent background including open spaces inside the ring. No lettering, no numbers, no runes, no border square, no checkerboard, no watermark, no selected-state glow. Exactly one icon, not a sheet.
```

## Action image

- Original: `assets/source/turn-undead-action-original.png`.
- Tool output basename: `exec-89175a86-b02f-4372-9d17-a3fb5161e083.png`.
- Generation parameter: `transparent_background: false`.
- Reference input: `fire-affinity.png`, the project's 128×128 painted channeling-family anchor.
- The returned master is RGB with no alpha channel; the required runtime export is RGBA with opaque alpha.

### Exact prompt

```text
Use case: stylized-concept. Asset: an original Pathfinder Kingmaker Turn Undead ACTION-BAR icon. The supplied Fire Affinity image is a PAINTED MAGIC STYLE REFERENCE ONLY (brushwork, strong light-dark contrast, broad hand gesture, dark atmospheric background). Create a new original image, do not reuse reference pixels or depict flame.
Create one square 1024x1024 icon intended for a 128x128 RGBA game export. Show an outstretched living hand at lower left releasing one curved wave of radiant ivory and pale gold positive energy toward a large skeletal head and upper torso at upper right. The skeleton visibly recoils and twists away, jaw open, one bony arm raised in retreat. This is supernatural repulsion causing undead to flee, not burning, damage, explosion, disintegration, healing, necromantic control, or a summoned skeleton. Keep the skeleton intact. No chains, leash, puppeteer strings, swords, blood, green necromancy or purple hypnosis.
Style: painterly old fantasy CRPG spell icon, tactile brushwork, robust easily legible silhouette, very simple scene with just hand, light wave, retreating skeleton. Bone-white skull, restrained gold/ivory light, cool slate-blue shadows on the skeleton, deep charcoal/umber full-bleed background. The bright wave has a clear outward curve without becoming a complete ornamental UI ring. Strongest focus skull plus radiant palm. Important subject within central 84 percent, generous safety margin for game masking; background goes to every edge.
No text, letters, numbers, no square frame, no game UI, no counters, no watermark, no collage. Single icon.
```

## Mechanical formatting after generation

Original master bytes were copied without alteration. Each runtime file was made with Pillow 13.0.0: decode original, `convert("RGBA")`, `resize((size,size), Image.Resampling.BICUBIC)`, save PNG with `compress_level=9`. Sizes were 64 for the feat and 128 for the action. No painting edits, sharpening, chroma removal or generated overlays were applied after generation.

These mechanically formatted PNGs are valid runtime-size assets. They are not claimed to be byte-identical to the repository's Windows System.Drawing GDI+ canonical exports. During repository intake, preserve these masters/prompts, use the scoped production exporter described in `ICON_INTEGRATION_NOTES.md`, and record the exact new export hashes if its output differs.

The review sheet was composed from the runtime PNGs, with native-size thumbnails at 32/48/64, dark and parchment backgrounds, grayscale checks and an explicitly enlarged pixel preview. It is a review derivative, not source art or a game screenshot.

## Actual review status

- PNG decode/signature, size, RGBA runtime encoding and alpha checks: passed.
- Independent small-size visual review: passed for this handoff; concepts are distinct and read as repulsion.
- Minor observation: the feat's pale sun has low contrast on a parchment background at 32px/grayscale; skull, wave and ring remain readable.
- Canonical Windows exporter: not run in package preparation.
- Native game/menu/tooltip rendering: not tested in package preparation.
- Owner visual approval: not asserted; Howie's review of these new pixels remains separate from technical correctness.

Do not transform these status statements into fabricated repository approval fields or a native-test PASS.

