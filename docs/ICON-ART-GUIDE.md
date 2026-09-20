# Icon art guide

This is the canonical authoring policy for player-visible icons, including newly
cloned spells, feats, traits, abilities, buffs, items and summon choices. Read the
[reference index](art/ICON-REFERENCE-INDEX.md) and
[catalog](../assets-source/original-icons/icon-catalog.json) before deciding art.
Mission progress and pending approvals belong in
[mission state](../planning/ICON-OVERHAUL-STATE.md), not in reusable policy.

## Decide the identity before drawing

Follow the actual graph: selection, feature, granted facts, action, variants,
activatable, visible buffs, held-touch delivery and associated items. Record
every consumer's stable symbol/GUID or exact UI entry. Read mechanics: names and
inherited donor pictures are not specifications.

| Disposition | Rule |
|---|---|
| original-required | A new visible concept needs an original recognizable identity. |
| intentional-family-share | Equivalent feature/action/effect consumers may share one concept; different selectable actions need distinct silhouettes. |
| native-semantic-reuse | Keep art for the exact native spell, creature or effect actually reused; explain that identity. |
| protected-existing | Preserve protected pixels **and assignments**, including inherited native art. |
| native-monogram | Prefer native ornamental text for weapon notation; preserve parameter identity. |
| hidden-internal | Invisible resources/providers/helpers need no independent painting. Preserve required non-null contracts. |
| review-out-of-scope | Record a concrete defect without expanding the assignment. |

A scroll is an independent item consumer. Its spell association does not prove
its item icon changed. Generic parchment can remain native; an unrelated donor
glyph cannot be accepted as the new spell's identity.

Owner-directed scroll item convention (2026-09-19, corrected): the three
strategic scroll ITEMS — Teleport, Greater Teleport, Word of Recall — are
ASSET COMPOSITES on the exact native scroll shell, not newly painted scroll
artwork. The shell is reconstructed symbol-free from five same-design native
scroll donor sprites captured through the guarded
`icon-overhaul-visual-evidence` reference dump
(`after-12-native-scroll-references.png`, local-only): per-pixel cross-donor
median (the donors are pixel-identical outside their symbols), ring fill for
the small core covered by symbols in three or more donors, cool-pixel ghost
enforcement (the shell is entirely warm-toned), and background-to-alpha
unmixing. Parchment shape, roller/cap design, border/shading, proportions
and silhouette are the native shell's own pixels; only the inner emblem
window (measured donor symbol bboxes) carries the approved spell painting.
The bare approved painting remains the spell ability identity; each
composite is a dedicated per-item concept (`scroll-of-*`) with
production-manifest authority. Kingmaker has no procedural "scroll icon from
symbol" mechanism — native scroll items are individually authored 64x64
atlas sprites assigned as static textures — so this is necessarily offline
compositing plus texture assignment. Generic native parchment still governs
any future scroll whose spell identity has no approved painting.

## Choose and inspect a visual family

**Painted magical/racial art:** one strong subject, broad silhouette, controlled
light and material texture. Color is secondary meaning. Ancestry, channeling,
defense, healing and attack must remain distinguishable without hue. Magical
racial feats belong here even when tagged mechanically as combat feats.

**Mundane combat emblems:** an original readable action in the measured native
ink/scaffold vocabulary. Compare ring weight, negative space and muted color
against indexed native examples. Flat vector construction is suitable. Loading
must read as loading, not repairing. Do not duplicate the native square UI frame.

**Weapon monograms:** native text/font/background where supported. Do not generate
letters, substitute an approximate font, convert saved blueprint parameters to
weapon categories, or null every blueprint sprite. Inspect selector and
selected-feature consumers separately.

**Summon choices:** use the existing painted creature family with recognizable
species/body shapes; retain the parent spell identity. New species need distinct
anatomy, not recolors or unrelated donors. Leave counters/templates/dynamic state
to the established UI conventions.

Open the indexed images. A filename or "Kingmaker style" is not a visual
reference. Protected legacy references have a limited preservation scope.
Unreviewed output never silently replaces a stable approved anchor.

## Brief and prompt template

    Concept / exact player-facing name:
    Module / stable symbols or exact UI entries:
    Implemented behavior; misleading interpretations to avoid:
    Visible graph, including variants, buffs, touch delivery and scrolls:
    Disposition; intentional sharing/native reuse reason:
    Family / approved reference IDs actually inspected:
    Each input image's role (style reference, edit target, source pixels):
    Dominant subject, action and strongest silhouette:
    Closest sibling / confused-with concept; compositional distinction:
    Palette, light and material treatment:
    UI-supplied frame, background, text, counters and state:
    Verified export profile, actual size and foreground bounds:
    Original source / export / installed paths:
    Production tool and exact prompt or editable composition:
    Technical / native-screen checks:
    Reviewed source/export hashes / owner decision and evidence:

Use an available built-in image tool or authorized artist for paintings; follow
its installed skill. API billing, keys and installs need separate authorization.
Do not silently substitute procedural paintings. Preserve individual originals
byte-for-byte and record edits separately. A generated grid is not a source archive.

Review actual thumbnails before approval. Iterate on an observed defect. Get a
representative pilot approved before mass production of a new family. Direction
approval does not approve every later image; pixel changes invalidate previous
image approval. Silence and technical PASS are not consent.

## Inspected rendering and export contracts

These profiles distinguish source evidence, historical screenshots and pending
final-artifact UI measurements. They are not a universal-size rule.

| Profile | Inspected contract | Evidence/limit |
|---|---|---|
| project-painted-128 | Quick Clear, Reload Firearm, Repair Firearm, Focused Aim and Shield Other exports are 128x128 RGBA PNG. Pilot originals are preserved 1254x1254 RGB PNG. | Decoded indexed files. Native final layout is a separate gate. Full-bleed painted backgrounds are allowed. |
| combat-emblem-64 | Existing Rapid Reload/parameter exports are 64x64 RGBA PNG with 512x512 sources. Transparent emblem backgrounds reveal UI. | Decoded pixels and manifests. The approved Rapid Reload pilot anchors this family; the old thick partial arc is a negative reference. Its ring geometry is not a universal requirement. |
| native-selector-text | FeatureUIData retains null Icon. Desktop CharBuildSelectorItem.SetIcon shows m_AcronimText; character-sheet CharSComponentChupaChups uses AbilityAbbreviation. Both use UIUtility.GetAbilityAcronym with native decoration. | Inspect the actual entry type: parametrized/selected data uses the firearm constructor adapter, while Rapid Reload's static Items list needs an exact owned-selection adapter. Preserve fallback sprites, filtering, count and order. Native rendering remains a separate check. No PNG/font export is required by these routes. |
| native-fact-slot-text | CharSComponentAbilitySlot.SetFeature(Feature) reads Fact.Icon directly; Total supplies real Feature objects through SetData(IUIDataProvider). SetIcon(null) owns native TMP, background and border, with text derived independently from the feature name. | Constructor-only FeatureUIData/UIFeature evidence does not cover these paths. Inspect real Total/character-sheet rows separately; limit text adaptation to exact owned facts/parameters and preserve each native border/mask sequence, blueprint sprites and other controls. |
| summon-painted-128 | Existing manifest defines 77 individually sourced 128x128 RGBA exports with exact child placements. | Delegate export authority to that manifest. Historical publication does not approve new creatures. |

The supplied 1920x1200 desktop references show approximately 48px racial icon
interiors and 54-56px feat/weapon interiors inside decorated cells. These are
historical capture measurements, not every resolution/scale. Use indexed local
crops for comparison. Review 32px, 48px and 64px stress reductions plus actual
destination/tooltip sizes. Significant foreground must survive the native frame;
a full-bleed background is not an alpha clipping defect. Intentional wrist/effect
continuation differs from an accidentally clipped letter or weapon tip. Never
bake active, selected, disabled or count overlays into the pixels.

The pilot exporter uses Windows System.Drawing GDI+: HighQualityBicubic,
HighQuality pixel offset/compositing, TileFlipXY sampling, ARGB32 SourceCopy and
PNG. Painted exports perform no chroma removal, sharpening or model calls.
Do not globally remove green from elemental art. Inspect parchment/dark
backgrounds and grayscale. Confirm repeat hashes on the recorded toolchain;
do not assume cross-platform GDI identity.

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/icon-art/Export-IconPilot.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/icon-art/Export-IconProduction.ps1
    python tools/validate_icon_catalog.py
    python tools/test_icon_catalog.py

Pilot exports remain under assets-source/original-icons/icon-overhaul-v2/pilot/exports
until approval/integration. Production sources belong under assets-source/original-icons;
production exports go to assets/game/icons, installed as assets/icons. The
summoning subdirectory is explicitly copied. New subdirectories require loader,
build-copy and package-validator support together.

Normal builds never invoke an image model. Export begins with preserved reviewed
originals. Do not normalize unrelated protected assets. Retire or redirect old
generators when authority changes.

Rapid Reload's approved v2 pilot is its current source/export authority. The
rejected former runtime PNG is retained under `icon-overhaul-v2/references` for
historical validation and negative-reference inspection. The old dedicated
chroma generator and broad feat generators must fail before writing runtime
assets. Historical records do not authorize restoring their rejected output.
See [firearm presentation](FIREARM-FEAT-ICON-MAP.md) for the exact UI filter and
fallback contracts.

The pilot and production exporters freeze files whose manifest records carry
approval. Re-running them verifies those bytes instead of repainting, regenerating,
or removing approval. Keep candidate revisions and their original prompts; an edit
must retain its input and update the exact source/export hashes before review.
Use `New-IconFamilyReview.ps1 -Name <family> -Keys <comma-separated-keys>` to make
labeled thumbnail/grayscale art-inspection sheets from manifest-verified exports.
Run `python -B tools/icon-art/New-IconProductionReview.py` to rebuild the local
searchable collection from those same canonical records. It verifies hashes and
preserves the distinction between approved pilot art and production candidates.
Neither review output is native game-screen evidence.

## Integration and catalog authority

Blueprints/ProjectAssetIcons.cs is the established cache: required-file failures,
one load per registered file, Texture2D decode, centered Sprite at 100 pixels/unit
and shared cached references. It leaves texture filter/wrap defaults unchanged.
Use the main-thread bootstrap lifecycle. No per-frame IO, repeated allocations,
competing cache or runtime JSON library for the authoring catalog. Never destroy
shared native textures.

Use owned construction or one explicit idempotent post-registration stage.
Inspect late compatibility/publication overwrites. Assign exact owned identities;
do not extend the recursive substring fallback, repaint foreign blueprints or
add global icon/font getters. Preserve module-OFF/save-hydration behavior.

`Blueprints/OwnedIconAssignments.cs` contains the explicit elemental/strategic
bindings. Its keys extend the existing cache, and its single application stage
runs after registration/publication, just before the bootstrap result. Each
symbol is resolved against the installed blueprint manifest with an exact type
and GUID check. The Teleportation module's independent registration-failure
boundary is preserved. This table has no separate sprite cache or runtime art
catalog parser.

The canonical catalog owns dispositions, protection, review decisions and exact
mission consumer coverage. Unchanged legacy manifests remain export authorities;
delegate by path/hash rather than keeping conflicting copies. The pilot and
production manifests under `assets-source/original-icons/icon-overhaul-v2/` own
source/export hashes; the catalog's per-concept `assetAuthority` selects exactly
one of them. The catalog owns review evidence and consumer dispositions. Validation
checks registry identity, delegated hashes, protection and required coverage.
Each integrated concept's `runtimeExport` declares its source path, installed
path and cache key. The catalog validator compares the compiled binding table
with the exact intended consumers. It also requires cataloged mapping and UI
adapter sources to appear in the actual mod project's Compile items; a source
file on disk alone is not an integrated icon rule. Build and package validation use
`scripts/IconCatalog.Common.ps1` to require the same final export hashes at
their installed destinations.
Intentional sharing uses one concept key; different selectable actions cannot
hide duplicate art under different filenames.

Production briefs must record `behavior`, `subjectAndSilhouette`, `artFamily`,
`reviewGroup`, `exportProfile`, `uiSurfaces`, `confusedWith` and
`forbiddenInterpretations`, alongside the actual prompt and provenance. Surface
names must match the catalog consumers and confusion keys must name real catalog
concepts. Once `artProductionStatus` is `complete-main-scope-candidates`, every
painted/emblem concept must retain its own declared asset authority. This is an
art-coverage assertion, not approval or runtime qualification.

Missing required final exports fail validation. During development, old working
art may remain with an explicit incomplete status. Keep indirectly assigned,
native-reused, protected and hidden consumers in the inventory.

## Qualification and acceptance

Keep source/export checks, structured runtime assertions, art inspection and
owner approval separate. The icon-overhaul-visual-evidence scenario produces
live-sprite **facsimiles**, not native-menu screenshots. A preserved blueprint
fallback picture cannot qualify native selector typography.

Read the current Build-Local orchestrator and its version guard before use. It
runs repository validation, the complete clean Release domain suite, exact
Release compilation and output/package gates. Never copy stale version literals
or change unrelated release metadata to satisfy tests. Follow
[Windows runtime rules](WIN10-AUTONOMOUS-RUNTIME-TESTING.md), guarded Steam App ID
640820 launches and the documented named disposable-save procedure. No campaign
save access, UI guessing or unauthorized save writes.

Verify loaded bindings after late initialization, donor/protected assignments,
parameters/GUIDs, variants/buffs/touch graphs, export hashes/format and installed
paths. Capture actual screens for placement, scale and state through permitted
tools; request a bounded supervised capture if required. Record source/DLL/package
hashes, version and profile. Changed candidates need impacted final-artifact
checks again. Report unavailable evidence honestly.
The [native screen workflow](ICON-NATIVE-UI-EVIDENCE.md) describes request-only
holds in the existing creator and spellbook fixtures. Keep these real captures
separate from the live-sprite facsimiles and record remaining uncovered surfaces.

## Future examples: no gameplay authorization

**Magic Circle Against Alignment:** read its eventual design/variants. Use
original painted group/circle protection, related to ward imagery but recognizably
a larger ward. Distinguish alignment variants by form/meaning as well as palette.
Do not inherit Protection's icon unchanged.

**Lunge:** read the mundane feat graph. Use native emblem vocabulary with an
original extended-thrust/reach gesture. Do not invent a magical effect, activation
or buff. Map any actual action/effect consumers deliberately.

**New summonable monster:** inspect painted creature anchors and its authorized
anatomy/equipment. Keep the spell-root and variant conventions. Exact native
creature reuse may retain its icon; a new species cannot inherit an unrelated
animal. Keep counters outside the pixels.

Before concluding any icon task, verify graph coverage, inspected references,
durable originals/exports/provenance, protection/package checks, correctly
labeled native UI evidence and approval hashes. Update this guide and its
validators when the contract changes; do not silently redefine an approved family.

For the guarded live assignment audit, follow
[Icon consumer census](ICON-CONSUMER-CENSUS.md). It records real before/after
Sprite references and graph links without navigating UI or modifying saves.

The firearm Total-list adapter is limited to `CharBNewAbilities.FillData` containing an exact owned firearm fact. If its existing disabled `ContentSizeFitterExtended` uses `PreferredSize`, it temporarily enables that fitter with horizontal fitting unconstrained, so nested feat rows remain scrollable without driving their width. It restores the original enabled state and horizontal mode on creator hide, component disable/destruction or list refill. It never assigns a fixed height, relocates an individual row, or changes another list. Native mode, content extent and cleanup require runtime evidence.


Magic Circle gameplay mission extension: the catalog records exact authorized
non-art bootstrap hunks under `authorizedFeatureEdits`. Validation reverses only
those hunks before enforcing the original icon-mapping baseline. It still
rejects changes to protected assignments and unlisted source changes. The
registry hash advances only for the exact appended identities, with the original
published prefix checked separately. This does not change an approved art family
or grant visual approval by itself. The owner approved all four exact 128px
Magic Circle exports in `reports/magic-circle/OWNER-DECISIONS.json`. Each alignment
painting is intentionally shared across its spell, held touch, timed carrier,
proximity recipient and scroll item; native frames, casting effects and sounds
remain native. The four area identities have no independent icon. These new
scroll items use the approved alignment paintings directly; the three protected
strategic scroll composites retain their existing assignments. The final candidate passes all twenty native desktop consumer checks with
control protection ON and OFF; see
`reports/magic-circle/FINAL-NATIVE-UI-REVIEW.json`. Direct visual inspection covers
the complete ON screenshot set and all OFF carrier/recipient tooltips. Exact
artifact and profile scopes are in
`reports/magic-circle/FINAL-CANDIDATE-QUALIFICATION.json`. These technical and
native UI results remain separate from the owner's exact pixel approval.
