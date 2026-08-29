# Firearm feat icon source

The four source PNGs in `sources/` are original, project-owned 512 px renders
from deterministic vector paths in
`tools/icon-art/New-IconOverhaulAssets.ps1`. No native game pixels, copied
letterforms, font files, or runtime font dependency are used.

The three supported firearm selectors use one coherent full-square system:
a subdued burgundy/brown tonal field, dark and gold nested frames, restrained
gold ornament, and an original centered `B`, `M`, or `P` path. Rifle and
Revolver are intentionally absent because they are tolerated only as hidden
legacy categories.

Rapid Reload keeps its circular-arrow-plus-tool concept. Its emblem is enlarged
and redrawn in muted `#A6533F` with a small `#C77B63` highlight on a transparent
canvas matching neighboring vanilla feat glyphs. It contains no blue corner
glyphs.

Run either command from the repository root:

```powershell
.\tools\New-FirearmFeatIcons.ps1
.\tools\icon-art\New-IconOverhaulAssets.ps1 -Mode Feat
```

The pipeline draws at 512 px and performs one high-quality downsample to the
64 px runtime textures. It also regenerates `firearm-feat-icon-map.png` at
64 px and 32 px presentation sizes and `SHA256SUMS.txt`. `icon-spec.json`
records the palette, exact supported set, seeds, and construction contract.
