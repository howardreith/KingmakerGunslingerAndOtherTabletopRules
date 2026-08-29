# Icon Art Polish Round 2 Report

## Status

Complete locally. The two scoped icon families passed deterministic asset
validation, the complete domain suite, clean Release build and packaging,
local installation, guarded full-resolution live-sprite review, focused
mechanical scenarios, and the canonical working-save smoke. This report is the
durable audit, implementation, and qualification record for
`docs/reference/icon-polish-round-2/CODEX_PROMPT.md`.

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

### Firearm selector polish

`tools/icon-art/New-IconOverhaulAssets.ps1` remains the editable construction
authority. `Draw-SelectorField` now renders only a full-bleed
`#2B1716`-to-`#754A32` burgundy-brown field with restrained deterministic
texture. The explicit outer dark rectangle, inner gold rectangle, and corner
ornaments were removed. The defect was therefore corrected at its baked source,
not hidden with a UI patch or a second resample.

The original vector paths are still rasterized at 512x512 and downsampled once
to 64x64. The final optical transforms are P scale `0.66`, offset
`(-0.75,-0.5)`; M scale `0.62`, offset `(-0.75,-1.0)`; and B scale `0.66`,
offset `(-0.5,-0.5)`.

| Glyph | Before 64px visible bounds | After 64px visible bounds | After canvas share |
| --- | --- | --- | --- |
| Pistol P | `x=10,y=10,w=44,h=47` | `x=17,y=17,w=31,h=31` | 48.44% x 48.44% |
| Musket M | `x=7,y=12,w=53,h=45` | `x=16,y=18,w=33,h=29` | 51.56% x 45.31% |
| Blunderbuss B | `x=10,y=10,w=44,h=47` | `x=17,y=17,w=31,h=31` | 48.44% x 48.44% |

Exact source and final paths and hashes:

| Asset | Source path and SHA-256 | Runtime path and SHA-256 |
| --- | --- | --- |
| P | `assets-source/original-icons/firearm-feats/sources/firearm-monogram-pistol-source.png` - `fc7312b5d33fffa337c277b89a7fa8479ed82e28eeea9014d40a59ea9162061c` | `assets/game/icons/firearm-monogram-pistol.png` - `ec9ed32c71b137f8d8b65184b6e92e946d034a2ef329cd0f8fe7f52194e3f07d` |
| M | `assets-source/original-icons/firearm-feats/sources/firearm-monogram-musket-source.png` - `c024fe3df9ef4ee09ecf037c8f928599d398b6a61530a3a67e5b804cbcc6d6e0` | `assets/game/icons/firearm-monogram-musket.png` - `7bb189ad50bc578217adeca1d280e31312053a13cddf112760a5611eb79a82ee` |
| B | `assets-source/original-icons/firearm-feats/sources/firearm-monogram-blunderbuss-source.png` - `6ac0919821fe8d9e6f502cd6611360f389d2e0ffd4daa5d1582c3cc08a4007a3` | `assets/game/icons/firearm-monogram-blunderbuss.png` - `65272c2ccfca2c3a766e0b11767e44ea65b1e9b05038dcb7d07c97f3fdce89f7` |

The centralized runtime mapping was not changed. One sprite per official kind
continues to feed Weapon Focus, Rapid Reload, Greater Weapon Focus, Weapon
Specialization, Greater Weapon Specialization, Improved Critical, Gun
Training, and the native parameter adapters. Guarded reference-equality
assertions passed for all four dependent three-choice families.

### Cord of Stubborn Resolve

The only pre-existing flattened source described an intentionally circular
top-down loop, so it was retained as history and not perspective-squashed.
OpenAI's built-in image-generation tool produced one new original transparent
high-resolution oblique braided-cord source. The exact prompt, source role,
dimensions, alpha bounds, and hashes are recorded in
`assets-source/original-icons/cord-of-stubborn-resolve/SOURCE.md`. The supplied
native-belt screenshots guided perspective and canvas grammar; no extracted
vanilla texture is redistributed.

`tools/New-CordOfStubbornResolveIcon.ps1` deterministically alpha-fits that
source and downsamples it once into the runtime texture:

- source:
  `assets-source/original-icons/cord-of-stubborn-resolve/cord-of-stubborn-resolve-oblique-source.png`,
  1672x941, SHA-256
  `54bb3426f8cd651758c6bce733904045fb30a84dd7b452d72bdf111abeb481e1`;
- final:
  `assets/game/icons/cord-of-stubborn-resolve.png`, 128x128 RGBA, SHA-256
  `101e1b2fbd7083c5db20be1a0ee40840bc8201520dff83be0acd9bae06f91a6a`;
- before silhouette: `x=6,y=8,w=116,h=110`, aspect `1.055:1`;
- after silhouette: `x=6,y=32,w=116,h=64`, aspect `1.8125:1`;
- final corner alpha: `0,0,0,0`.

At exact 128px inventory/equipment scale the brighter braided front segment,
receding rear segment, central knot, and short hanging ends remain readable.
At the smaller preview scale the object still reads horizontally as a cord
belt, not an unbroken ring or leather donor belt. The live native
`BeltOfConstitution2` comparator retained its distinct native sprite.

### Deterministic previews and supplied-reference use

`tools/icon-art/New-IconPolishRound2Evidence.ps1` reads and renders all five
1920x1200 originals, all five focused crops, the supplied contact sheet, the
three selectors at 64px and 32px, and Cord at 128px and 64px. Its outputs are:

- `docs/reports/icon-polish-round-2/exact-size-preview.png`;
- `docs/reports/icon-polish-round-2/exact-size-preview-manifest.json`;
- `docs/reports/icon-polish-round-2-before-after-contact-sheet.png`;
- `docs/reports/icon-polish-round-2/runtime-after/manifest.json`;
- six curated full-resolution frames under
  `docs/reports/icon-polish-round-2/runtime-after/`.

The final visual run was
`20260829T1520495183372Z-2acc2b65981f492291343145b1bef7b6`,
PASS, loaded version `0.0.108`. Its machine-local source directory is
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260829T1520495027113Z-icon-overhaul-visual-evidence`.
The curated manifest verifies every copied screenshot byte and records all six
as 1920x1200.

Inspection at one source pixel per displayed pixel found:

- B/P have comfortable negative space and M is optically centered without
  dominating the native Weapon Focus neighbors;
- none of B/M/P contains the former inset rectangle or corner frame;
- Rapid Reload shows the same B/M/P sprite family and no Rifle/Revolver row;
- Cord is clearly horizontal and oblique in both exact 128px inventory and
  equipped cells;
- the ordinary native belt remains an unchanged perspective/scale control.

The frames are deterministic in-game Unity UI facsimiles built from the actual
loaded blueprint names and live `Sprite` objects. They are not screenshots of
automated native-menu navigation and are supporting perceptual evidence, not
mechanical correctness evidence. Automated UI input and native save/menu
navigation are prohibited by the repository contract. The supplied
full-resolution screenshots remain the authoritative native-UI before state;
the guarded assertions and separate disposable scenarios provide runtime
mechanical proof.

### Regression protection

`scripts/Test-IconPolishRound2Assets.ps1` checks 30 pre-mission hashes on every
repository validation: 13 locked runtime art files, 11 corresponding accepted
source files, and six Rifle/Revolver/publication policy sources. It also checks
the three after bounds, absence of a baked frame, dimensions, RGBA/alpha
contracts, Cord aspect, transparent corners, source/final hashes, reference
counts, and retired selector absence.

The final focused result was:

`Icon polish Round 2 validation passed: 30 protected files; 3 no-frame
selectors; belt-like Cord aspect 1.8125.`

Thus every hash in the pre-mission protected manifest above is unchanged.
The six unchanged policy hashes are:

- `FirearmKind.cs`:
  `e3a94f162f9b62cdbb4b1b5274d1a6d4aa43d4477d1099a1d5f709c45aaee911`;
- `FirearmFeatBlueprints.cs`:
  `f08609beb8f8ffca8eefb0f02035c347298773753268009619ef1f24f52919b1`;
- `NativeFirearmFeatIntegration.cs`:
  `bc22787d2838a418dd22b656b87554a5e3be8d25c9f9b420c3d2a07e3410bc75`;
- `GunTrainingBlueprints.cs`:
  `8603c87a4fc9fecd86ed0aa2da52bdcd9c5969d898639a6688139faeb93c0564`;
- `ProductionFirearmCatalog.cs`:
  `75a8352c85c2e4fe5369ea02c414df9adc8a04f33c075f9176bd2f1138ad18dd`;
- `CraftMagicItemsCompatibilityPolicy.cs`:
  `89f3031e6df64ba00a6cfc615e2401b7c6b5f5ec35b9c216e480c39151906a22`.

No blueprint GUID, mechanics, slot, price, localization, acquisition, vendor,
save, firearm-support, or retirement source was edited.

### Qualification commands and results

Art and evidence commands:

```powershell
.\tools\icon-art\New-IconOverhaulAssets.ps1 -Mode Feat
.\tools\New-CordOfStubbornResolveIcon.ps1
.\tools\icon-art\New-IconPolishRound2Evidence.ps1
.\tools\icon-art\New-IconPolishRound2Evidence.ps1 `
  -RuntimeEvidenceDirectory <guarded-visual-evidence-directory>
```

Repository and package qualification:

```powershell
.\scripts\validate-repository.ps1
.\scripts\test-domain.ps1 -Configuration Release -Clean
.\scripts\build.ps1 -Configuration Release -Clean -Package
.\scripts\validate-package.ps1 `
  -PackagePath .\artifacts\packages\KingmakerGunslinger-0.0.108-icon-art-polish-round-2.zip
```

Results: version-aware repository validation PASS; icon-overhaul gate PASS;
Round 2 gate PASS; 1,325 domain/reflection tests with zero failures; clean
Release compile PASS; build-output validation PASS; unchanged SoundBank PASS;
strict standalone UMM package validation PASS.

The final package was backed up and installed through the guarded local
workflow. Deployment manifest:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T1520494402110Z\deployment.json`.
The previous live mod was preserved at
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260829T1520458309463Z`.

Final guarded commands all launched through Steam App ID 640820:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario icon-overhaul-visual-evidence -ExpectedVersion 0.0.108 `
  -TimeoutSeconds 300 -ExitAfterCompletion:$true -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-firearm-dependent-feats -ExpectedVersion 0.0.108 `
  -ReuseInstalledArtifact -DeploymentManifestPath <final-deployment.json> `
  -PackagePath .\artifacts\local-runtime\0.0.108\KingmakerGunslinger-0.0.108-local-runtime.zip `
  -TimeoutSeconds 300 -ExitAfterCompletion:$true -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-cord-of-stubborn-resolve -ExpectedVersion 0.0.108 `
  -ReuseInstalledArtifact -DeploymentManifestPath <final-deployment.json> `
  -PackagePath .\artifacts\local-runtime\0.0.108\KingmakerGunslinger-0.0.108-local-runtime.zip `
  -TimeoutSeconds 300 -ExitAfterCompletion:$true -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke -ExpectedVersion 0.0.108 `
  -SaveName KMG_AUTOMATION_WORKING -ReuseInstalledArtifact `
  -DeploymentManifestPath <final-deployment.json> `
  -PackagePath .\artifacts\local-runtime\0.0.108\KingmakerGunslinger-0.0.108-local-runtime.zip `
  -TimeoutSeconds 300 -ExitAfterCompletion:$true -Confirm:$false
```

Final structured results:

| Scenario | Run ID | Assertions | Result |
| --- | --- | ---: | --- |
| Visual evidence | `20260829T1520495183372Z-2acc2b65981f492291343145b1bef7b6` | 9 | PASS |
| Dependent firearm feats | `20260829T1523006289421Z-d84c2fb263d74f7bb3c031b09471513e` | 12 | PASS |
| Cord mechanics | `20260829T1524570892682Z-98330d7d9d4548079d2db2003dbd5346` | 14 | PASS |
| Working-save smoke | `20260829T1526538917101Z-a6897afcdf624a51b9245d71ab0e6cc5` | 11 | PASS |

During candidate instrumentation, two guarded visual runs ended ERROR before
qualification. Run directory
`20260829T1437404507750Z-icon-overhaul-visual-evidence` exposed an incorrect
fixture filter that discarded native `WeaponCategory` parameters. Run
`20260829T1442436125620Z-icon-overhaul-visual-evidence` showed that native
parameter rows may have null cached icons. The final implementation retains the
raw rows and reconstructs null native icons through Kingmaker's own
`FeatureUIData(BlueprintFeature, FeatureParam)` path. Candidate and final
visual runs then passed. A separate reuse preflight also correctly refused to
launch after curated tracked evidence changed the repository-state fingerprint;
the candidate was rebuilt and reinstalled. None of these fail-closed
engineering iterations was treated as a visual or mechanical PASS.

### Version, package, and local commits

- Ending branch: `codex/icon-art-polish-round-2`.
- Ending version: `0.0.108-icon-art-polish-round-2`.
- Implementation commit:
  `accd872c46f6884a114894e28f18f7c9fa694c75`.
- Release-identity commit and packaged build commit:
  `840d84e57a97ef8ed03a2a51ea479dcd6e41a7e7`.
- Installable release package:
  `artifacts/packages/KingmakerGunslinger-0.0.108-icon-art-polish-round-2.zip`.
- Local runtime package:
  `artifacts/local-runtime/0.0.108/KingmakerGunslinger-0.0.108-local-runtime.zip`.
- Both final ZIPs are byte-identical, 22,714,237 bytes, SHA-256
  `9f01775bb5f8c4fa2ef0e96082ad3b409a141d427d752b9272a10393da4ce6a5`.
- DLL SHA-256:
  `d5781ab745fced26df22e7684767cbffb1b04e7606447fcf8ddac912c5df42d2`.
- DLL MVID: `b6315f40-6fcb-4f00-841b-6260f2da8e20`.

The prior 0.0.107 package is restored at its original path and has not been
left overwritten.
The final artifact audit did find that an intermediate candidate build had
temporarily replaced the active `artifacts/packages` copy. Before handoff, that
path and its checksum were restored from
`artifacts/release/0.0.107/KingmakerGunslinger-0.0.107-icon-art-overhaul.zip`.
The restored SHA-256
`50849e63bbce6ec06d5840e688795a0ec3d2de4963ef843dec8aa5c610429184`
exactly matches the historical release manifest and its embedded version is
`0.0.107`. The intermediate candidate remains separately recoverable as the
`artifacts/local-runtime/0.0.107` package with SHA-256
`858766f18844d9aae16036a1dba83386ebdbe21b396182a124ee8f9a1bda0c9d`.
`scripts/Publish-Release.ps1` was intentionally not invoked: even without
`-Publish`, it authenticates to GitHub, fetches, creates or pushes a tag, and
creates or edits a remote draft release. That would violate this mission's
absolute remote-operation prohibition. No remote Git or GitHub operation was
performed, and no tag or remote release was created.

The final evidence/report-only commit is reported in the handoff because a
commit cannot embed its own hash. The handoff also records the post-commit
`git status --short` result.

### Remaining limitation

The final installed mod launched and all guarded assertions passed, but the six
after views are live-sprite UI facsimiles rather than human-navigated native
Weapon Focus, inventory, and equipment-menu screenshots. This is stated in the
runtime result and contact sheet. Within the authorized autonomous mechanism,
there is no unresolved mechanical or asset-pipeline uncertainty.
