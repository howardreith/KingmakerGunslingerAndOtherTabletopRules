# Icon Art Polish Round 2 Report

## Status

In progress. This report is the durable audit, implementation, and
qualification record for `docs/reference/icon-polish-round-2/CODEX_PROMPT.md`.

## Starting baseline

- Repository root: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`
- Initial checkout: `master` at
  `7e77a970b9e93de80dde0e22ef7ce80403dd39ce`
- Work branch: `codex/icon-art-polish-round-2`, created locally from that exact
  commit.
- Starting version: `0.0.107-icon-art-overhaul`.
- Starting status: only the supplied
  `docs/reference/icon-polish-round-2/` mission bundle was untracked.
- Authoritative release package:
  `artifacts/packages/KingmakerGunslinger-0.0.107-icon-art-overhaul.zip`,
  SHA-256
  `50849e63bbce6ec06d5840e688795a0ec3d2de4963ef843dec8aa5c610429184`.
- `artifacts/release/0.0.107/release-manifest.json` identifies that package as
  a validated two-build deterministic release produced from the starting
  commit. Its DLL SHA-256 is
  `6e0a643dccfc1139d631336399f3ff26a10e14d5422cbddcada99caf9e76a761`.
- No remote Git operation is authorized for this mission and none has been
  performed.

The baseline contains the accepted overhaul: centralized native-looking
firearm item textures, all corrected Eastern and Elven Branched Spear item
textures, the corrected Rapid Reload feat icon, three official firearm kinds,
five recognized legacy identities, and no Rifle/Revolver selector PNGs.

## Supplied-reference audit

All five 1920x1200 full-resolution screenshots, all five focused crops, the
2200x1910 contact sheet, and `VISUAL_NOTES.md` were inspected. The firearm
screens show B/M/P at materially greater visual mass than adjacent native
category glyphs and show a gold/dark inset rectangle inside the normal game
cell. The equipment screens show the current Cord as a nearly top-down ring,
while both native belt references are horizontally dominant, oblique objects
with visible front/rear depth, overlap, and shadow.

## Pre-implementation pipeline audit

### Firearm category icons

- Editable construction: original vector paths in
  `tools/icon-art/New-IconOverhaulAssets.ps1`, rendered once at 512x512 and
  high-quality downsampled to 64x64.
- Generated sources:
  `assets-source/original-icons/firearm-feats/sources/firearm-monogram-{blunderbuss,musket,pistol}-source.png`.
- Runtime textures:
  `assets/game/icons/firearm-monogram-{blunderbuss,musket,pistol}.png`.
- Specification and manifests:
  `assets-source/original-icons/firearm-feats/icon-spec.json`,
  `assets-source/original-icons/firearm-feats/SHA256SUMS.txt`, and
  `assets-source/original-icons/icon-overhaul-assets.json`.
- Cause of the double-frame defect: `Draw-SelectorField` fills the full tile,
  then explicitly draws a 2.2px dark rectangle at `(1.1,1.1)` and a 1.15px
  gold rectangle at `(4,4)`, plus corner ornaments. Those marks are baked into
  the PNG; they are not Unity import settings or Kingmaker row framing.
- Before monogram-only alpha bounds at native 64px size (including the
  decorative baseline flourish and shadow): P `x=10,y=10,w=44,h=47`; M
  `x=7,y=12,w=53,h=45`; B `x=10,y=10,w=44,h=47`. Thus the current glyph art
  occupies 69-83% of tile width and 70-73% of tile height.

`ProjectAssetIcons.Load` decodes packaged PNG bytes into non-mipmapped Unity
`Texture2D` objects using `TextureFormat.ARGB32`, then creates centered,
full-rectangle sprites at 100 pixels per unit. No runtime compression or
importer modifies the border. The project copies `assets/game/icons/*.png`
unchanged to `assets/icons/*.png`; Unity's ordinary sprite filtering supplies
the in-game edge softness.

`ProjectAssetIcons.ApplyFirearmFeatIcons` assigns one shared sprite per
official firearm kind to all three project Weapon Focus children, all three
Rapid Reload children, and the four three-choice dependent families: Greater
Weapon Focus, Weapon Specialization, Greater Weapon Specialization, and
Improved Critical. `NativeFirearmFeatIntegration.Append` publishes those same
parameter blueprint icons into the native parameterized menus. A centralized
texture correction therefore reaches every relevant selector; no screen-local
patch is needed.

### Cord of Stubborn Resolve

- Existing source:
  `assets-source/original-icons/cord-of-stubborn-resolve/cord-of-stubborn-resolve-chroma-source.png`
  (1254x1254 chroma-keyed flattened raster).
- Existing generator: `tools/New-CordOfStubbornResolveIcon.ps1`.
- Runtime texture: `assets/game/icons/cord-of-stubborn-resolve.png` (128x128
  RGBA).
- Runtime mapping: `ProjectAssetIcons.Apply` replaces only the cloned Cord
  donor's icon with `KMG_Icon_cord-of-stubborn-resolve`; the belt blueprint,
  slot, GUID, mechanics, cost, localization, and campaign placement are not
  involved in the art pipeline.
- Before alpha bounds at threshold 3: `x=6,y=8,w=116,h=110`; silhouette aspect
  `1.055:1`. The nearly square/circular outline explains the jewelry read.

The existing Cord source was authored specifically as a circular loop. A new
original high-resolution source is required; a simple perspective squash would
retain the unbroken-ring construction rejected by the mission.

## Pre-mission protected manifest

These runtime assets are locked. The final audit must reproduce every hash
exactly.

| Locked runtime asset | SHA-256 |
| --- | --- |
| `rapid-reload.png` | `efab95075ad8af61fe10425090015a75432b74113fbc34ebc185969e1e82b321` |
| `early-pistol.png` | `1cd06b9aeea63b4842951568812791e50e8fd9472884078449dd84c1c9bf0719` |
| `musket.png` | `638077254f298a626f3fa8a8c098bb1e9f2c4f3678df90a1e28920f4a9ffd086` |
| `blunderbuss.png` | `e5923f9b5820eef3ca3d41e5af559b09ef8ea21b0052dc04909fd72f73ac929f` |
| `rifle.png` | `0fa35d1d917006b6ab36d2e0a449a142cf24d3e9c3cc02634d88ab17e7ac1f66` |
| `revolver.png` | `ff4aab9347f7c8515509c3957f2b4db42742711e17b0e67811720b954509a5b2` |
| `wakizashi.png` | `cb32f5afdc9522bebf45d863b7a2f153c8ea908292c96cb30601f739a27d9dc1` |
| `katana.png` | `139ff7292bb4d8270b92083e90b4c46be50b54a9e0ac9382eb9397acd6f09a90` |
| `nodachi.png` | `1e3f8d208e4d4733a32ee71968b051182f818ffe407dfd76a7b8a731b8bfa8da` |
| `night-without-moon.png` | `a6681e97cc07e3d4a3c894e2c1b479f647ef60cf24f40eaa945d6fdc96824f0e` |
| `heavens-measure.png` | `428c6c8099b27926cbe962fe5ff40e7a24db75826eee060b654345a9ba0f63f4` |
| `world-tree-severer.png` | `730072a080d7b4c405d554e2f34e498cde973d36627976b964d4b69c81c20e32` |
| `elven-branched-spear.png` | `5a8d3d10f95af61c6afd324c8791b37bb675d4a74d3dcd4eca7cdb4d0464109a` |

The corresponding Rapid Reload, three firearm-item, six Eastern, and one spear
source files were also hashed before editing. Their SHA-256 values are,
respectively:

`a115b060976a73e60eb178f9209ac9f176fdec13dae25076715f530d153d3e98`,
`c6a76485178cdb1a7b37291b8169e034c78df4b0d551da70b18d428d30abde6b`,
`624582c0f7a63a097f85f289edbd9aa4933264d70f4f91148b2222878f4a94e6`,
`773cbf0c27329c520eacedc7f6e85645493ee7a85e48436b6cfa0e1b190582e7`,
`6a2c02473bc1f87e000d83f327244b5f450c7260dbbc3871025f86a7220f554c`,
`1b97a26b4c7a3dfbd25df9d9e5f64c5b3e2ff7c9743e49df6906f58778ceec2f`,
`3c715265c312def544593bafa5f76bb48f778a68a013b747cd2e3b903abd2547`,
`122c539c6ce002ff029a6b2e05bbb2bb17cb7a3d4d190d5d750184365fdd977a`,
`c5a285ef03454eb5a64dd2dbdd2894951fd0d7702702b8238bcbd8f7b837a2bf`,
`cff3b5db26c709d15d47cd8af6cfe9da62c0c7c9ae7a681ce028a0efe85e3e33`,
and `ece96570240e97ec009914f42a569415b622282689276b8beeee258e95846960`.

Core Rifle/Revolver official-versus-recognized policy and publication files
were separately hashed before editing and are intentionally outside the change
set.

## In-scope before hashes

| Asset | SHA-256 |
| --- | --- |
| `assets/game/icons/firearm-monogram-pistol.png` | `5343d062083ada98bf0aabdfc0eb3d538c0c8b9fd9cfbaaabbab2c8cc3a0df0d` |
| `assets/game/icons/firearm-monogram-musket.png` | `675d291d8ea7fc7955ab6468d9134a09619b727a1c789357d6fbd4a1485aa848` |
| `assets/game/icons/firearm-monogram-blunderbuss.png` | `08e4e9061ca76b26b778804da4436446382ec725f43096416ff0eae3b9bed4a9` |
| `assets/game/icons/cord-of-stubborn-resolve.png` | `cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928` |
| P 512px source | `95944bad6c84c87ce6436518eb4590883b716bb6edec5e9b263ac41ce242022b` |
| M 512px source | `03b9e05195d2780c41a0c8a3d618fbb0f5e75c8042edc8e98a23fc013fea5ce9` |
| B 512px source | `af7eb40ce6a79836ed969ae1b30a75dfc315aeca7eabc897eb8534d0ed9e76bf` |
| Existing Cord source | `d7e5dfa7228419df65e3bfa88aafa7b94caa1e5cfadfb1a159686805042655c8` |

## Implementation, qualification, and final identity

Pending.
