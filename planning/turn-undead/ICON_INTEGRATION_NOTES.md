# Turn Undead — icon integration notes for Claude

## Purpose and evidence status

Integrate two distinct original concepts: the **Turn Undead feat** and the **Turn Undead execution ability**. The user requested a channel-feat-style icon for selection/character-sheet surfaces and a separate toolbar action illustration. These instructions are an art and integration contract, not a claim that native presentation or owner visual approval has already passed.

Repository inspected: `howardreith/KingmakerGunslingerAndOtherTabletopRules`, master commit `52755b617c79328a972ae5b74bc05784422e332d`, verified on 2026-11-27. Reconcile paths/contracts with the current checkout before implementing. Read the checkout's current `AGENTS.md`, `docs/VALIDATION-POLICY.md`, `docs/ICON-ART-GUIDE.md`, `docs/art/ICON-REFERENCE-INDEX.md`, and `assets-source/original-icons/icon-catalog.json` first. Existing approved pixels and assignments remain protected.

## The two concepts and runtime exports

| Concept | Proposed stable icon key | Runtime file | Export profile | Intended appearance |
|---|---|---|---|---|
| Turn Undead feat | `turn-undead-feat` | `assets/game/icons/turn-undead-feat.png` | `combat-emblem-64`: **64 × 64, 8-bit RGBA PNG** | Muted terracotta/ivory channeling emblem on transparency. Clear holy burst repels an undead skull/silhouette. A restrained internal circular scaffold may be part of the artwork. |
| Turn Undead action | `turn-undead-action` | `assets/game/icons/turn-undead-action.png` | `project-painted-128`: **128 × 128, 8-bit RGBA PNG** | Painted ivory/gold positive-energy burst driving a clearly readable skeleton away, with a dark controlled background. The picture must suggest repulsion/fleeing rather than destruction or command. |

Both profiles are already supported by the repository. The feat's emblem profile is a deliberate response to the user's request to match channel-related feat art. Do not infer that every magical feat should be a painted square, or that every mechanical combat feat must be an emblem. The guide's family distinction is visual and concept-specific; the channel family request supplies the direction for this new concept.

Use the separate supplied originals and their provenance records. They are two concepts, not recolors, duplicate filenames, or a contact-sheet crop. Keep the original generated master bytes unmodified. The standard production exporter accepts a **square source of at least 512 × 512 pixels**; 512 and 1254 are observed existing source sizes, not universal requirements. A 1024 × 1024 original is acceptable to that size check. Preserve transparent alpha on the feat master. The painted action can be opaque while still exporting RGBA.

The installed paths are `assets/icons/turn-undead-feat.png` and `assets/icons/turn-undead-action.png` under the mod installation. Avoid introducing a runtime subdirectory: the existing cache resolves flat `<key>.png` files there. Original masters, prompts and production metadata belong in the source-asset tree, not the installed mod folder.

## Visual references and their limits

The research inspected these references:

- `CallOfTheWild/Icons/FeatIcons/Icon_Channel_Extra.png` and `Icon_Channel_Quick.png` from the available Call of the Wild reference checkout: both 64 × 64 RGBA. They show thin terracotta circular scaffolding, pale central channel motifs and restrained hand/effect shapes. These are **mod reference files**, not verified exports of native Owlcat sprites. Their art is not redistributed in this package.
- The current project's `assets/game/icons/rapid-reload.png`: approved 64 × 64 emblem-family anchor with transparent negative space and restrained terracotta ring/subject. Borrow the broad visual grammar, not its subject or exact geometry.
- The current project's `assets/game/icons/fire-affinity.png`: approved 128 × 128 painted channeling-family anchor. It establishes treatment, broad silhouette and effect hierarchy; Turn Undead must not become an orange fire attack.
- `CallOfTheWild/Icons/AbilityIcons/ChannelEnergy.png` is a 64 × 64 magenta healing image. It is useful as a semantic contrast, not a palette or compositional target for Turn Undead. The new ability must read as positive-energy repulsion.

Actual native Selective Channel/Extra Channel game sprite files were not available in the inspected workspace. Native screenshots indexed by the repository are lab-local and were not available here; public wiki retrieval did not supply reliable original sprites. Therefore no document in this package should claim a pixel-exact native channel-family match or native UI acceptance. During implementation compare the supplied feat with those native siblings in the real Kingmaker UI if available under the authorized lab workflow.

## Composition requirements

The feat and action should be related through the holy burst/repelled undead motif, but visibly different at small size. The feat is a quiet, economical emblem; the action is an immediately readable event.

Keep the main action legible at 32, 48 and 64 pixels. Use one primary focal relationship, broad light/dark masses and limited tiny detail. The skeleton should recoil or turn away from the energy. Avoid exploding bones, ashes, a disintegrating body, damage numbers or an overt damaging projectile: this feature causes fleeing. Avoid chains, a puppet stance, domination eyes, an enthroned necromancer, purple mind-control motifs or a kneeling servant: Command/Rebuke Undead is outside this mission. A generic radiant emblem is preferable to a deity-specific holy symbol because eligibility should not imply devotion to one deity.

The game supplies the square UI frame, selection and disabled states, counters, cooldowns, active indicators and variant affordances. Do not draw those into either image. An internal emblem ring is allowed; an outer ornate button frame is not. Do not add text, numerals, a menu arrow, a resource count or a faux tooltip. Keep required subject geometry inside the usable sprite region. Intentional painterly background/effect bleed is acceptable for the action; alpha transparency is expected around the feat scaffold.

## Source/export authority and reproducibility

Use the established production structure unless the current checkout has adopted a newer documented intake:

```text
assets-source/original-icons/icon-overhaul-v2/production/
  sources/turn-undead-feat.png
  sources/turn-undead-action.png
  briefs/turn-undead-feat.json
  briefs/turn-undead-action.json
  exports/turn-undead-feat.png
  exports/turn-undead-action.png
  production-manifest.json
```

The individual production manifest owns source/export hashes. The canonical catalog selects that authority and owns consumer coverage, dispositions and review status. Do not create an inconsistent second runtime catalog. Preserve the delivered masters, exact generation prompts, generation/edit lineage and real SHA-256 values. Never invent a model version, source path, owner decision, timestamp, reference input or source-image hash. A contact sheet is review evidence; it is not a source archive.

The inspected production exporter is `tools/icon-art/Export-IconProduction.ps1`. It uses Windows System.Drawing GDI+, HighQualityBicubic, HighQuality pixel offset/compositing, TileFlipXY sampling and SourceCopy ARGB32, then saves PNG. It performs no image generation, chroma removal or sharpening. Its size branch is 64 pixels for `combat-emblem-64`, otherwise 128 pixels. Existing approved records and unselected records are protected against unexpected byte changes.

After adding both valid briefs and individual sources, a scoped PowerShell invocation is:

```powershell
& ./tools/icon-art/Export-IconProduction.ps1 `
    -RepositoryRoot $PWD.Path `
    -ConceptKeys @('turn-undead-feat', 'turn-undead-action')
```

Use the permitted PowerShell entry path in the actual lab. Do not rerun the entire catalog simply to create two images. Scope preserves recorded unrelated artifacts; a preexisting protection or manifest error requires investigation rather than bypassing the validator.

If the mission package's ready-to-use runtime PNGs were mechanically resized on another platform, their dimensions/encoding can be valid without being byte-identical to the canonical GDI+ exports. Record that distinction. Run the canonical exporter during repository intake where available; compare both newly produced files visually and update only the new concepts' actual source/export hashes. Do not assert Windows exporter provenance for Linux-produced files. Do not mark a different export hash as the already-reviewed image. Do not replace supplied masters or regenerate the art during ordinary builds.

Each brief must retain the current schema's required fields, including `key`, `source`, `sourceSha256`, `tool`, `prompt`, `referenceImages`, `behavior`, `subjectAndSilhouette`, `artFamily`, `reviewGroup`, `exportProfile`, `uiSurfaces`, `confusedWith`, and `forbiddenInterpretations`. Use real implementation references once written. `confusedWith` entries must refer to real catalog concept keys; the two new concepts may name each other once both exist. Native sibling names belong in prose or reference metadata if they are not catalog concepts.

## Bind exact visible consumers

First inventory the new blueprint graph and final publication path. Record exact symbol, manifest GUID, blueprint type, intended icon key, UI surface and reason for any intentional share. Do not guess GUIDs in the art manifest ahead of blueprint registration.

| Consumer | Icon treatment |
|---|---|
| Selectable general feat and resulting visible feat fact | `turn-undead-feat` |
| Any project-owned feature wrapper that presents the same feat identity | Share the feat concept intentionally; no duplicate asset. |
| Actual executable Turn Undead ability/variant | `turn-undead-action` |
| Source-specific variants, if the implementation needs more than one channel pool | Share the single Turn Undead action concept, with accurate names/tooltips identifying the source. The runtime identity and resource ownership must remain distinct. |
| Existing Channel Positive Energy parent/menu | Retain its existing channel icon unless an explicitly owned new wrapper needs its own documented presentation. Do not repaint foreign/native parents. |
| Visible turned/fleeing status buff, if any | Document deliberate action-icon sharing or a suitable existing native condition icon; do not introduce a third requested original. |
| Hidden resources/implementation facts | Explicitly inventory as hidden; do not manufacture visible art consumers. |

The mission's desired submenu placement is a gameplay/UI integration requirement, not something the icon can implement. Attach the action image to the actual selectable execution ability, not only a granting feature. Confirm whether the source game's native positive-channel parent has a safe variants path and whether a new child is hidden correctly before the feat is acquired. If a separate ability entry is required by an engine limitation, preserve the separate action icon and document the measured limitation. Do not fabricate a menu arrow inside its pixels.

At the inspected commit the established cache is:

`src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs`

It reads registered flat files once at bootstrap, fails on missing required files, decodes a `Texture2D`, creates a centered sprite at 100 pixels per unit, and stores the shared sprite under the key. It leaves texture filter/wrap defaults unchanged. Add the two exact keys through the established registration path. Avoid duplicate registration: names already supplied by `OwnedIconAssignments.IconKeys` should not also be repeated in the initial fixed list, because the dictionary insertion is strict. No competing cache, runtime authoring-JSON parser, per-frame file I/O, repeated allocations or destruction of native shared textures.

The established exact-assignment path is:

`src/KingmakerGunslinger/Blueprints/OwnedIconAssignments.cs`

It resolves manifest symbols to exact GUID/type and validates final shared icon references. It also contains module/category-specific behavior, so read the entire relevant application/validation path before appending bindings. Use the established binding mechanism if it fits the new module, or assign the cached icon directly in the owned builder plus exact validation. Do not blindly classify a new symbol under an unrelated existing category. Apply after relevant publication in an idempotent location. Preserve module-off/save-hydration behavior and foreign blueprint ownership. Inspect late compatibility mutations that could restore an inherited donor icon.

Never install a broad `Icon` getter patch, substring fallback or recursive donor replacement to make the images appear. Native and third-party channel icons outside the new owned consumers must remain untouched.

## Catalog, build and package coverage

For each new integrated concept, add a canonical catalog record with a single source authority, profile/family, real consumer inventory and `runtimeExport`:

```json
{
  "path": "assets/game/icons/turn-undead-feat.png",
  "installedPath": "assets/icons/turn-undead-feat.png",
  "cacheKey": "turn-undead-feat"
}
```

Use the analogous values for `turn-undead-action`. This is a fragment, not a complete catalog record; copy the current full schema and retain honest review/technical status. New consumers and the exact compiled binding table must agree. Any new C# mapping/builder source must appear in the actual project's Compile items; merely adding a file is insufficient in this project.

The repository's Python catalog validation and tests check authoring contracts. `scripts/IconCatalog.Common.ps1` also verifies required installed files against expected final export hashes during build/package checks. Ensure actual build copy/package inclusion covers both files. Do not distribute original masters, repository research images, native screenshots, game assemblies or unrelated generated artifacts in the player mod ZIP.

## Focused verification and handoff evidence

Choose checks under current `docs/VALIDATION-POLICY.md`; this two-icon feature mission does not revive an old blanket release pipeline. Useful affected checks include the existing `python tools/validate_icon_catalog.py` and `python tools/test_icon_catalog.py`, focused feature tests/compilation required by the implementation mission, and exact package-content/hash inspection.

Record four separate outcomes:

1. **Asset correctness:** PNG signatures, square dimensions, RGBA decode, actual SHA-256, preserved original masters, real transparency on the feat, no stray matte/black halo, no identical-pixel duplicate disguised as a second action. Show 32/48/64 reductions, dark and parchment backgrounds and grayscale; inspect that the skeleton is repelled rather than merely radiating magic.
2. **Runtime identity:** exact new feat/ability consumers use the expected shared sprites, sources do not overwrite each other, files are loaded once, and package paths/hashes are correct. Check no missing icon or native donor accidentally survives on the actual execution child.
3. **Native presentation:** observe eligible/unavailable feat selection, character-sheet fact, tooltip, channel variant menu, draggable action-bar entry, disabled/no-resource state and any visible turned status. The native UI owns borders, badges and state overlays. Use the repository's authorized guarded runtime workflow and a disposable fixture; do not touch protected saves or the user's live installation. If unavailable, state that native presentation is unverified.
4. **Owner visual review:** remains pending until Howie reviews the actual new images. Technical validation and existing approval of a different family anchor are not owner approval of these pixels. This package is an original-art handoff, not a fabricated signoff.

The repository's `icon-overhaul-visual-evidence` scenario creates facsimiles using live sprites; those are useful but are not actual feat/menu screenshots. Do not call facsimiles native menu acceptance. Missing lab access should produce a precise deferred-check record and a reviewable development checkpoint, not a false PASS or an unrelated request to regenerate artwork.

## Versioned primary source links

- [Art guide](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/ICON-ART-GUIDE.md)
- [Reference index](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/docs/art/ICON-REFERENCE-INDEX.md)
- [Canonical catalog](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/assets-source/original-icons/icon-catalog.json)
- [Production exporter](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/tools/icon-art/Export-IconProduction.ps1)
- [Existing emblem brief example](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/assets-source/original-icons/icon-overhaul-v2/production/briefs/dire-crocodile-swallowed.json)
- [Runtime cache](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs)
- [Exact consumer bindings](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/OwnedIconAssignments.cs)

The reference artwork itself is deliberately excluded from the handoff ZIP. Follow the source links or inspect the authorized native installation for comparisons.
