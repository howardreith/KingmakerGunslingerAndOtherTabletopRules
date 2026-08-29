# Icon Art Overhaul Report

## Status

This report is the durable audit and qualification record for
`docs/reference/icon-overhaul/CODEX_PROMPT.md`. It was opened before any code,
blueprint, or art implementation change on 2026-08-29.

## Pre-implementation audit

### Baseline and references

- Repository: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`
- Starting branch/HEAD: `master` at
  `7094003b63e67da8f5c2a6f0f5b14db3a49e4a3b`
- Local work branch: `codex/icon-art-overhaul`
- Baseline status: `?? docs/reference/`; these are the supplied mission inputs.
- Configured remote: `origin`; remote operations are prohibited and none have
  been performed.
- All five 1920x1200 RGBA originals, the contact sheet, and `VISUAL_NOTES.md`
  were inspected at full resolution.

| Reference | SHA-256 |
| --- | --- |
| `01_rapid_reload_feat_list.png` | `0E2F8F96749E932C37BD0FA09DB8071B4706B53227888C464AE2E08F466D47CC` |
| `02_rapid_reload_weapon_choices.png` | `6BD649D441775176952878800716121AD762DF85E74DFCB9304B333C3A9872ED` |
| `03_weapon_focus_blunderbuss_reference.png` | `58E87A9665A90E4360D307A5668C36897BEC121871D68204586A75D3A53C0C66` |
| `04_weapon_focus_musket_pistol_nodachi_reference.png` | `1BCA29C364BE452583B945F872D19E1E8F15D299FD9CA4546F91E614BD11F0E7` |
| `05_inventory_weapon_icon_comparison.png` | `E333FEDE86A4C940BF0EC6758164248DC0D27D46AF4A7454FCD95EEB601267C3` |

The references show that the pale firearm selector cards and blue corners do
not match the full-square Nodachi/native selector language, Rapid Reload is
too small and dark beside muted terracotta native feat emblems, and item
silhouettes should occupy a transparent lower-left-to-upper-right diagonal
instead of a framed rectangle or short horizontal strip.

### Loading and generation pipeline

`src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs` reads packaged
`assets/icons/*.png` into a
non-mipmapped `Texture2D` in `ARGB32`, then creates a full-rectangle sprite with
pivot `(0.5, 0.5)` and 100 pixels per unit. No runtime compression is requested;
Unity's default bilinear filtering applies. `scripts/Build-Local.ps1` copies
`assets/game/icons/*.png` unchanged to build and staging output.

Rapid Reload and selector sources are:

- `assets-source/original-icons/firearm-feats/{icon-spec.json,SOURCE.md,firearm-feat-icon-map.png,SHA256SUMS.txt}`;
- legacy reconstruction inputs in `fourth-playtest/` and `second-playtest/`;
- `tools/New-FirearmFeatIcons.ps1` and older `tools/New-RapidReloadIcon.ps1`;
- final 64x64 `rapid-reload.png` and five firearm monogram PNGs.

The active generator draws directly at 64x64, uses a machine font, makes pale
rounded cards with blue corners, and emits five categories. The current Rapid
Reload arrow is near `#702828`; reference neighbors are dominated by muted
terracotta near `#A05038` to `#A85038`. The replacement must render from
high-resolution deterministic source and downsample.

Eastern and spear sources are project-owned Blender/FBX clean-room assets under
`assets-source/original-icons/eastern-weapons/` and
`assets-source/original-icons/elven-branched-spear/`. Their generators disable
render antialiasing and independently render 512 and 128 outputs. Current
sources have hard alpha edges; several are clipped or leave excessive padding.
The corrected pipeline must reframe clean source and downsample to 128x128.

### Complete item/icon manifest

Supported firearms and centralized family textures:

- Pistol (`early-pistol.png`): Early Pistol, Pistol +1, Duelist's Rebuttal,
  and Last Word.
- Musket (`musket.png`): Early Musket, Musket +1, River King's Measure, and
  Watch at World's End.
- Blunderbuss (`blunderbuss.png`): Early Blunderbuss, Blunderbuss +1, and
  Irovetti's Ovation.
- Test Musket is diagnostic and retains its native donor icon.
- Advanced Rifle (`rifle.png`) and Advanced Revolver (`revolver.png`) are
  stable legacy-only low-level blueprints.

The three supported 128x128 textures currently contain a dark frame that
reaches the canvas edges. Replacing those centralized textures covers every
supported mundane, enchanted, and named item.

All 30 eastern items audited:

- Wakizashi generic: mundane, masterwork, cold iron, +1.
- Wakizashi named: Paper Lantern, Quiet Current, Falling Petal, Foxfire Whisper,
  Empty Sleeve, Night Without Moon.
- Katana generic: mundane, masterwork, cold iron, +1.
- Katana named: Wayfarer's Oath, Winter Reed, Drawn Horizon, Thunder at the
  Gate, Moonlit Crossing, Heaven's Measure.
- Nodachi generic: mundane, masterwork, cold iron, +1.
- Nodachi named: Border Sentinel, Cloud-Cleaver, Storm Over Stone,
  Mountain-Sunder, Unfixed Form, World-Tree Severer.

Generic/named items share `wakizashi.png`, `katana.png`, or `nodachi.png`, except
the three capstones, which use `night-without-moon.png`,
`heavens-measure.png`, and `world-tree-severer.png`.

All 12 Elven Branched Spear items map to `elven-branched-spear.png`: mundane,
masterwork, cold iron, masterwork cold iron, +1, +1 cold iron, Boughkeeper,
Thornstep, Moonlit Fork, Viper's Reach, Briar-Crowned Spear, and Spear of First
Branch.

### Rifle and Revolver surface audit

Player-facing surfaces requiring suppression:

- `FirearmFeatBlueprints.cs` publishes both kinds in Rapid Reload, Weapon Focus,
  Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization,
  and Improved Critical choices.
- `NativeFirearmFeatIntegration.cs` appends the same five weapon types to native
  parameterized selections and hard-codes a five-entry contract.
- `GunTrainingBlueprints.cs` publishes five Gun Training choices.
- `ProjectAssetIcons.cs`, the feat-icon spec/generator, tests, and package
  validators require dedicated Rifle/Revolver monograms.
- README, compatibility, current manifest/icon/firearm/crafting reports, and
  planning coverage still describe a five-kind supported registry.

Legacy surfaces that must remain registered but unpublished:

- `FirearmKind`, stable type/item/feature GUIDs, definition catalogs, resolvers,
  mechanics, animation/audio/range/calibration, and compatibility fixtures
  recognize old or Toy Box-injected instances so they do not crash.
- Production catalog entries are recognition-only. Craft Magic Items recognizes
  an already-owned legacy item but does not expose either kind as a creation
  base.
- Vendor/loot cleanup enumerates all owned firearm blueprints to remove old
  leaked rows, while actual additions already contain only supported families.

No ordinary starting grant, replacement/fallback grant, ammunition sale,
vendor addition, rare-loot reward, or named-magic catalog entry was found for
Rifle/Revolver. Starting choices are Pistol/Musket only. The implementation
will separate an exact three-kind official set from the five-kind legacy
registry: stable blueprints stay readable, but only Pistol, Musket, and
Blunderbuss reach ordinary selectors and acquisition.

### Validation and runtime baseline

Existing tests pin the old five-way mapping, six eastern icon identities, one
spear hash, mapping counts (14 firearm, 30 eastern, 12 spear), vendor cleanup,
magic-item bases, and production policies. Existing validators check presence
but not transparent corners, exact official selections, source dimensions, or
retired monogram absence.

The guarded `weapon-presentation-evidence` scenario captures request-local 3D
held/stored models, not the required 2D feat/inventory/vendor UI. The canonical
`working-save-smoke` proves Steam/save/bootstrap identity but does not navigate
those views. Screenshots are perceptual evidence only and will not be used as
mechanical proof.

## Implementation record

### Official and legacy firearm boundary

`OfficialFirearmSupport` is now the single policy boundary:

- official/published: Pistol, Musket, Blunderbuss;
- recognized compatibility identities: those three plus Rifle and Revolver.

The registered arrays keep every stable Rifle/Revolver item, weapon type,
Rapid Reload child, dependent-feat child, and Gun Training fact readable for an
existing owner. Their UI facts are hidden and have no feat groups. Public arrays
and native parameter publication contain only the official three. Runtime
resolution accepts all five registered identities so an old save or deliberate
Toy Box spawn continues to use its historical mechanics without becoming an
ordinary option.

Rifle/Revolver were removed or suppressed from:

- Rapid Reload and all public child choices;
- native Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater
  Weapon Specialization, and Improved Critical parameter menus;
- project-owned dependent feat selections;
- Gun Training publication;
- feat/category sprite lookup and packaged monogram assets;
- official production and Craft Magic Items acquisition roles;
- current support, installation, icon-map, blueprint, range, and crafting
  documentation.

Starting grants were audited and remain Pistol/Musket-only. Capital and BTSL
vendor publication, named fixed loot, support-item sales, repair/overhaul
actions, ammunition, and fallback grants were audited; none provides a legacy
firearm, and managed vendor cleanup removes stale legacy rows. CMI exposes
exactly three from-scratch bases while still recognizing an already-owned
legacy item for a safe upgrade. Stable GUIDs and legacy mechanics were not
deleted or rewritten.

### Art pipeline

`ProjectAssetIcons` still loads packaged PNG bytes into non-mipmapped Unity
`ARGB32` textures and creates full-rectangle, center-pivot sprites at 100
pixels per unit. Runtime dimensions remain 64x64 for feature/category art and
128x128 for item art. No runtime art library or font dependency was added.

The replacement development pipeline is:

1. `tools/icon-art/New-IconOverhaulAssets.ps1` draws original feat/category
   vector paths at 512 px, renders original accepted firearm-item sources, and
   fits project-model renders by measured alpha bounds.
2. `tools/icon-art/render_weapon_icon_sources.py` uses the repository's Blender
   executable and clean project FBX sources to render six Eastern and one spear
   512 px sources in the corrected lower-left-to-upper-right view.
3. One high-quality downsample writes each final PNG. Item art receives a
   five-pixel safety margin on a transparent 128 px canvas.
4. The generator writes exact source/final dimensions, alpha bounds, corner
   alpha, provenance, and SHA-256 records to
   `assets-source/original-icons/icon-overhaul-assets.json`.
5. `scripts/Test-IconOverhaulAssets.ps1` validates the exact 14-file final set,
   exact three selector set, dimensions, alpha, transparent corners, bounds,
   source manifests, blueprint references, and retired monogram absence.

The firearm item sources are original 1254x1254 project assets created for this
mission with OpenAI image generation and committed as accepted primary art.
That initial generation is not claimed byte-deterministic; every subsequent
fit/downsample/manifests step is deterministic. Selector and Rapid Reload art
uses only original paths. Eastern and spear art is re-rendered from existing
project-owned/licensed FBX sources; no vanilla pixels or extracted game art are
redistributed.

Rapid Reload was iterated after the first live comparison exposed an
overly pale inset field. The accepted version removes that field entirely and
uses an enlarged `#A6533F` circular arrow/tool glyph on transparency. The B/M/P
selectors use a full-square burgundy/brown tonal field, nested dark/gold frame,
restrained ornament, and original gold path monograms.

### Changed art manifest

Source/provenance and deterministic development files:

- `assets-source/original-icons/firearm-feats/SOURCE.md`
- `assets-source/original-icons/firearm-feats/icon-spec.json`
- `assets-source/original-icons/firearm-feats/SHA256SUMS.txt`
- `assets-source/original-icons/firearm-feats/firearm-feat-icon-map.png`
- `assets-source/original-icons/firearm-feats/sources/rapid-reload-source.png`
- `assets-source/original-icons/firearm-feats/sources/firearm-monogram-{blunderbuss,musket,pistol}-source.png`
- `assets-source/original-icons/firearm-items/{SOURCE.md,blunderbuss-source.png,musket-source.png,early-pistol-source.png}`
- `assets-source/original-models/eastern-weapons/SOURCE.md`
- `assets-source/original-models/eastern-weapons/{wakizashi,katana,nodachi,night-without-moon,heavens-measure,world-tree-severer}-icon-source.png`
- `assets-source/original-models/elven-branched-spear/{SOURCE.md,elven-branched-spear-icon.png}`
- `assets-source/original-icons/icon-overhaul-assets.json`
- `assets-source/original-icons/icon-overhaul-weapon-render-report.json`
- `tools/icon-art/New-IconOverhaulAssets.ps1`
- `tools/icon-art/render_weapon_icon_sources.py`
- `tools/New-FirearmFeatIcons.ps1`

Final runtime textures changed:

- `assets/game/icons/rapid-reload.png`
- `assets/game/icons/firearm-monogram-{blunderbuss,musket,pistol}.png`
- `assets/game/icons/{blunderbuss,musket,early-pistol}.png`
- `assets/game/icons/{wakizashi,katana,nodachi}.png`
- `assets/game/icons/{night-without-moon,heavens-measure,world-tree-severer}.png`
- `assets/game/icons/elven-branched-spear.png`

Retired final textures:

- `assets/game/icons/firearm-monogram-rifle.png`
- `assets/game/icons/firearm-monogram-revolver.png`

No Rifle/Revolver item/model/audio asset was deleted because those files are
needed when a legacy item already exists.

### Blueprint and code manifest

No stable GUID changed. The generated blueprint behavior changed in:

- `Firearms/FirearmKind.cs`: official-versus-recognized policy;
- `Blueprints/FirearmFeatBlueprints.cs`: exact-three public arrays, hidden
  registered legacy children, and clone-safe compatibility lookup;
- `Feats/NativeFirearmFeatIntegration.cs`: exact-three native publication and
  five-identity legacy resolution;
- `Blueprints/GunTrainingBlueprints.cs`,
  `Classes/GunTrainingPolicy.cs`, `Classes/FirearmTrainingPolicy.cs`, and
  `Classes/FirearmTrainingRuntime.cs`: exact-three public training with hidden
  registered legacy ownership support;
- `Blueprints/ProjectAssetIcons.cs`: exact three centralized selector sprites
  and no legacy monogram load;
- `Firearms/ProductionFirearmCatalog.cs` and
  `Firearms/ProductionFirearmWeaponSpec.cs`: explicit
  `LegacyRecognitionOnly` acquisition role;
- `CraftMagicItemsCompatibility/CraftMagicItemsCompatibilityPolicy.cs` and
  `CraftMagicItemsCompatibility/CraftMagicItemsRegistrationCatalog.cs`:
  explicit legacy recognition without a new-item base;
- `RuntimeTesting/IconOverhaulVisualEvidenceScenario.cs`,
  `RuntimeTesting/RuntimeTestScenarioCatalog.cs`, and
  `RuntimeTesting/RuntimeTestRunner.cs`: guarded save-free live-sprite evidence
  and exact-three assertions;
- `KingmakerGunslinger.csproj` and
  `scripts/RuntimeAutomation.Common.ps1`: scenario compilation/allowlist.

Build/package/validation contracts changed in
`scripts/Build-Local.ps1`, `scripts/package.ps1`,
`scripts/validate-build-output.ps1`, `scripts/validate-package.ps1`,
`scripts/validate-repository.ps1`,
`tools/create_deterministic_package.py`,
`tools/validate_craft_magic_items100.py`,
`tools/validate_player_presentation105.py`,
`tools/validate_fatigue_authority106.py`, `tools/validate_sprint42.py`, and
`validation/static-validation.json`.

Focused or count-sensitive tests changed in:

- `CraftMagicItemsCompatibilityTests.cs`
- `ElvenBranchedSpearCatalogTests.cs`
- `ExpandedSummoningPresentationTests.cs`
- `FirearmFeatIconTests.cs`
- `PaperCartridgeFoundationTests.cs`
- `Sprint31Tests.cs`
- `Sprint36Tests.cs`
- `Program.cs`

The focused coverage asserts the exact official/recognized sets, public and
registered blueprint arrays, every native parameter surface, hidden legacy
facts, exact-three acquisition, all 14 final assets, dimensions/alpha/bounds,
removed Ri/Rv monograms, live scenario registration, and complete Eastern/spear
catalog icon identity.

## Qualification record

### Final automated and runtime qualification

All commands ran from the repository root on Windows PowerShell:

| Command | Result |
|---|---|
| `.\tools\icon-art\New-IconOverhaulAssets.ps1`, repeated twice | PASS: all 17 generated source/final/manifest outputs were byte-for-byte identical |
| `.\scripts\Test-IconOverhaulAssets.ps1` | PASS |
| `.\scripts\validate-repository.ps1` | PASS |
| `.\scripts\test-domain.ps1 -Configuration Release -Clean` | 1,325/1,325 PASS |
| `.\scripts\build.ps1 -Configuration Release -Clean -Package` | PASS: clean Release, output, AssetBundle, SoundBank, package, and strict standalone package validation |
| `.\scripts\validate-package.ps1 -PackagePath .\artifacts\packages\KingmakerGunslinger-0.0.106-fatigue-authority-repair.zip` | PASS |

The final code/art runtime artifact was installed through the repository
harness and Steam App ID 640820. The exact deployment record is
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T0840462360693Z\deployment.json`
and records source-state SHA-256
`be3ccc1ae79efb9c5ff43156a2dd1c57571b00a84ab79d48c7328ed5976ed34c`:

- local runtime package SHA-256:
  `ed37dd4efd92dddd6f8e38b04f6f5131a21d8beaa207049d2c65839fed4fe3e4`;
- loaded/deployed DLL SHA-256:
  `185b4df4f7d2fad511244c3bf6507f5710a400cb0880c993e8e95ef60bec74f0`;
- DLL MVID: `61692ec3-ad5f-4efd-adef-772712fbbc6e`.

Guarded results for that identical DLL/art set:

| Scenario/evidence directory | Guarded run ID | Result and relevant assertion |
|---|---|---|
| `20260829T0840463193515Z-icon-overhaul-visual-evidence` | `20260829T0840463526907Z-bb0b852157c04c47b6971c884fe630c3` | PASS; exact Rapid B/M/P, exact native Weapon Focus B/M/P, six official item rows, 30 Eastern, 12 spear, five 1920x1200 frames |
| `20260829T0844344368746Z-disposable-firearm-dependent-feats` | `20260829T0844344689641Z-927313f230014443a369f665be545ed8` | PASS; actual unit-aware native level-up menus contained only B/M/P and native Weapon Focus committed Pistol; legacy owners remained tolerated and ordinary characters could not acquire legacy wrappers |
| `20260829T0846294534777Z-observe-vendor-table-contracts` | `20260829T0846294870324Z-e76afd79ccf148f0a63d4a455f622e8a` | PASS; managed firearm/vendor/loot publication satisfied current exclusions, all Eastern merchant/loot rows were exact, and observation performed no mutation |
| `20260829T0848547609306Z-observe-gunslinger-presentation` | `20260829T0848547793028Z-8bcc030a551e41b8bd1987ab871035fa` | PASS; Rapid Reload had one top-level selection and exactly three isolated choices; 55 weapons/56 items had clean presentation |
| `20260829T0850472618430Z-working-save-smoke` | `20260829T0850472782790Z-32e143aae9df41f893cce275f9e80f9a` | PASS; exact `KMG_AUTOMATION_WORKING` slot, receiver-bound load, callback/fingerprint, no save-writing API, and no baseline mutation |

The install and final runtime commands were:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario icon-overhaul-visual-evidence -ExpectedVersion 0.0.106 -TimeoutSeconds 300 -ExitAfterCompletion:$true -AllowDirtyGit -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-firearm-dependent-feats -ExpectedVersion 0.0.106 -ReuseInstalledArtifact -DeploymentManifestPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T0840462360693Z\deployment.json' -PackagePath '.\artifacts\local-runtime\0.0.106\KingmakerGunslinger-0.0.106-local-runtime.zip' -TimeoutSeconds 300 -ExitAfterCompletion:$true -AllowDirtyGit -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario observe-vendor-table-contracts -ExpectedVersion 0.0.106 -ReuseInstalledArtifact -DeploymentManifestPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T0840462360693Z\deployment.json' -PackagePath '.\artifacts\local-runtime\0.0.106\KingmakerGunslinger-0.0.106-local-runtime.zip' -TimeoutSeconds 300 -ExitAfterCompletion:$true -AllowDirtyGit -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario observe-gunslinger-presentation -ExpectedVersion 0.0.106 -ReuseInstalledArtifact -DeploymentManifestPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T0840462360693Z\deployment.json' -PackagePath '.\artifacts\local-runtime\0.0.106\KingmakerGunslinger-0.0.106-local-runtime.zip' -TimeoutSeconds 300 -ExitAfterCompletion:$true -AllowDirtyGit -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario working-save-smoke -ExpectedVersion 0.0.106 -SaveName KMG_AUTOMATION_WORKING -ReuseInstalledArtifact -DeploymentManifestPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260829T0840462360693Z\deployment.json' -PackagePath '.\artifacts\local-runtime\0.0.106\KingmakerGunslinger-0.0.106-local-runtime.zip' -TimeoutSeconds 300 -ExitAfterCompletion:$true -AllowDirtyGit -Confirm:$false
```

All five commands returned PASS. Every reuse command revalidated the package,
DLL, MVID, AssetBundle, blueprint manifest, SoundBank, settings, source state,
and installed DLL before launching.

An earlier visual iteration at
`20260829T0758474754236Z-icon-overhaul-visual-evidence` was not accepted: its
assertion expected shortened choice names and its first frame revealed that
Rapid Reload still read as a pale card. The assertion was corrected to the
actual localized names, the art was rebuilt on transparency, and only the later
PASS run above supplies final screenshots.

### Final package and installed identity

- Installable package:
  `artifacts/packages/KingmakerGunslinger-0.0.106-fatigue-authority-repair.zip`
- Version: `0.0.106`
- Final package SHA-256:
  `ed37dd4efd92dddd6f8e38b04f6f5131a21d8beaa207049d2c65839fed4fe3e4`
- Final local-runtime package SHA-256:
  `ed37dd4efd92dddd6f8e38b04f6f5131a21d8beaa207049d2c65839fed4fe3e4`
- Final DLL SHA-256:
  `185b4df4f7d2fad511244c3bf6507f5710a400cb0880c993e8e95ef60bec74f0`
- Final DLL MVID: `61692ec3-ad5f-4efd-adef-772712fbbc6e`
- Final guarded visual run:
  `20260829T0840463526907Z-bb0b852157c04c47b6971c884fe630c3`
- Final guarded working-save run:
  `20260829T0850472782790Z-32e143aae9df41f893cce275f9e80f9a`

## Final manifest and limitations

The complete 30-item Eastern and 12-item Elven Branched Spear variant census is
listed in the pre-implementation manifest above. All 42 live item blueprints
were enumerated by the guarded scenario; the three shared generic Eastern
textures, three distinct capstone textures, and shared spear texture all loaded.

Curated full-resolution after screenshots:

1. `docs/reports/icon-overhaul/runtime-after/after-01-rapid-reload-feat-list.png`
2. `docs/reports/icon-overhaul/runtime-after/after-02-rapid-reload-supported-choices.png`
3. `docs/reports/icon-overhaul/runtime-after/after-03-weapon-focus-firearm-choices.png`
4. `docs/reports/icon-overhaul/runtime-after/after-04-supported-firearm-items.png`
5. `docs/reports/icon-overhaul/runtime-after/after-05-eastern-and-spear-items.png`

Supporting deliverables:

- `docs/reports/icon-overhaul/runtime-after/manifest.json` records the guarded
  run, loaded artifact identity, source and curated hashes, decoded-pixel hashes,
  and exact 1920x1200 dimensions.
- `docs/reports/icon-overhaul-before-after-contact-sheet.png` compares the
  supplied before contact sheet with all five accepted after frames.
- `docs/reports/icon-overhaul-asset-preview.png` shows the complete final
  64/128 px asset family outside the runtime layout.
- `tools/icon-art/New-IconOverhaulEvidence.ps1` regenerates the curated
  lossless evidence and contact sheet from an accepted guarded run.

The screenshots are in-game Unity `Camera`/`RenderTexture` facsimiles built from
the actual loaded blueprint names and sprite objects. They are honest supporting
perceptual evidence, not automated navigation screenshots of Kingmaker's native
level-up, inventory, or vendor canvases. Mechanical claims come from the
separate guarded native level-up, vendor-table, presentation, and save-backed
scenarios. This distinction is the remaining visual-evidence limitation.

Rifle/Revolver files and stable blueprints intentionally remain in the package
for compatibility; their presence is not official support. The accepted
firearm-item primary sources are committed, but the external generative step
that created them is not byte-reproducible. No remote Git operation was
performed.
