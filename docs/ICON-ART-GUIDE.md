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
| native-selector-text | Explicit FeatureUIData retains null Icon. Desktop CharBuildSelectorItem.SetIcon shows m_AcronimText, calls UIUtility.GetAbilityAcronym, and derives background/color from the item name. | Installed-assembly inspection. Default selected-feature construction separately reads the parameter blueprint's icon. No PNG/font export is required by this selector route. |
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

The canonical catalog owns dispositions, protection, review decisions and exact
mission consumer coverage. Unchanged legacy manifests remain export authorities;
delegate by path/hash rather than keeping conflicting copies. The pilot and
production manifests under `assets-source/original-icons/icon-overhaul-v2/` own
source/export hashes; the catalog's per-concept `assetAuthority` selects exactly
one of them. The catalog owns review evidence and consumer dispositions. Validation
checks registry identity, delegated hashes, protection and required coverage.
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
