# Cord of Stubborn Resolve icon source

Created on 2026-08-09 with OpenAI's built-in image-generation tool from this project-authored prompt:

> Create original project-owned art of the Cord of Stubborn Resolve: a tightly braided, weathered red-brown cord belt arranged in a strong circular loop, with several prominent binding knots and one small dark-steel clasp or central knot. It should look humble but supernaturally unyielding, not luxurious. Render it as a polished hand-painted fantasy CRPG inventory icon with a crisp silhouette, subtle warm magical highlights, and visible rough braided fibers. Center it with generous padding on a perfectly flat solid #00ff00 chroma-key background. No text, character, hands, scenery, cast shadow, reflection, generic leather belt, gemstones, ornate gold, watermark, copied game pixels, or green in the subject.

The preserved source is `cord-of-stubborn-resolve-chroma-source.png`, 1254 by 1254 pixels, SHA-256 `d7e5dfa7228419df65e3bfa88aafa7b94caa1e5cfadfb1a159686805042655c8`.

`tools/New-CordOfStubbornResolveIcon.ps1` deterministically converts dominant green chroma to soft alpha with despill, crops the opaque subject with 12 percent padding, and high-quality bicubic resamples it to the 128 by 128 production PNG:

```powershell
& .\tools\New-CordOfStubbornResolveIcon.ps1
```

The production asset is `assets/game/icons/cord-of-stubborn-resolve.png`, 128 by 128 RGBA PNG, SHA-256 `cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928`. Its corners are fully transparent and it has clean antialiased alpha edges.

No third-party or native-game pixels were supplied, copied, recolored, cropped, or overlaid. The source and derivative are original project-owned AI-assisted artwork.
