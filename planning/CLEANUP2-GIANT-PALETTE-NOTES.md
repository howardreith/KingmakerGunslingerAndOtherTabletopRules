# Cleanup 2 giant palette preparation

2026-10-21. ART-08 development source preparation. Owner visual approval is
pending. The new helper has **no active proofs, hooks or registrations**, and
therefore changes no current giant. Shared project/registration files were not
edited. Compilation, test registration and native lifecycle verification belong
to integration; none is claimed as PASS here.

## Exact evidence and targeting

The prior donor census in `CLEANUP2-GIANTS-NOTES.md` proves the body identities
below. The newer guarded native packet at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261021T2331326515455Z-disposable-expanded-summoning-cleanup2-native-review/summon-cleanup2-observations.json`
has SHA-256 `dedf9ad6c18d1efd5f7b02a79deb8040ace9168fc1f17fbd7dd65c96b9b19e26`.
That attempt observed the retained SM Frost body before its ground predicate
failed; it did not capture fresh complete six-family identities.

| Family | Proposed body selector / native material | Separate protected renderer |
| --- | --- | --- |
| Cloud; reviewed Stone target | `hill_giant_1` / `hill_giant_01_character_d`, prefab `635d6dee1c7831b41aa987bdbeff14bc` | `hill_giant01_outfit_02` / `hill_giant_outfit_02_character_d` |
| Storm; reviewed Fire target | `Frozen_body` / `frost_giant_character_d`, prefab `df0f3fbceb3605d489c0dec8fa5c7d1e` | `Frozen_cape1`, although it shares `frost_giant_character_d` |

Fresh Frost body metadata: path `Frozen_body_RIG[3]/Frozen_body[0]`, material
slot 0, `PF/StandardDynamic`, `_TintColor=(1,1,1,1)`, `_MainTex=frost_giant_d`
1024x1024, scale `(1,1)`, offset `(0,0)`. `_BumpMap`/`_Parameters` were absent
on that observed material. Material identity has the one native ` (Instance)`
suffix. The cape path is `Frozen_body_RIG[3]/Frozen_cape1[1]`. The body has
native colliders; the cape has Cloth. The helper changes neither transform,
mesh, collider, Cloth, UVs nor normals. No hair/skin UV semantics were inferred.

The integrator subsequently authorized a deliberate **whole body albedo
palette**, including any hair/skirt regions of that exact body atlas. Separate
outfit, cape, armor and weapon renderers remain excluded. No guessed hair mask,
UV rectangle or replacement geometry is used. This is an explicit art treatment,
not a claim that the native atlas contains only skin.

## Review-authorized scalar treatments

With normalized encoded source channels, luminance is
`L = .2126 R + .7152 G + .0722 B`.

| Family | Whole body albedo output RGB | Alpha |
| --- | --- | --- |
| Cloud | `(.84,.91,.98) * (.55 + .45 L)` | Original byte verbatim |
| Storm | `(.90,.65,.40) * (.25 + .75 L)` | Original byte verbatim |

These scalars were authorized by integration for review, with activation still
requiring fresh exact proof. Cloud lifts source value rather than multiplying
its brown texture by a pale tint. Both preserve source luminance ordering as
shading detail, while intentionally replacing body hue/chroma. No source pixels
are committed or exported. Only project-authored code/scalars are added.

No local `PF/StandardDynamic` shader source/graph or verified HDR `_TintColor`
response contract was found. A captured white property and a float-accepting
Material API do not prove an unclamped shader response. The helper makes no HDR
tint assumption. Mephit HDR rim evidence concerns a different surface.

## Ownership and refusal behavior

`GiantSummonPalettePolicy` provides the bounded transform and curated body
selectors. `GiantSummonPaletteRuntime` first uses the existing exact
`GiantSummonPresentationPolicy`, including actual unique summoned-buff Frost
source provenance and the module guard. The empty per-process proof registry
accepts only Cloud/Storm, exact prefab/path/material slot/texture name,
dimensions/format and explicit identity/whole-body-treatment evidence. Proofs
cannot be replaced during the process. Stone/Fire selectors are review targets;
no replacement scalar treatment for them is admitted yet.

Before mutation it requires one exact body renderer, the native material name
and shader, a neutral tint, settled dissolve and the proven Texture2D. It refuses
an existing generic variant owner. Unreadable albedo returns the explicit
`palette:source-albedo-not-readable` reason; no GPU/color-space fallback is
invented. The next fresh survey must supply format/isReadable and active color
space as well as exact family slot/texture identity before a proof is added.
Only Gamma rendering is currently admitted. The installed
`UnityEngine.CoreModule.dll`, SHA-256
`3a76df7f709d465e3273502e08edbffb536b1c2f78c3a132b8668e59fddd2803`,
was inspected read-only through reflection metadata: `Texture` has no public
color-space getter, while `QualitySettings.activeColorSpace` and the explicit
Texture2D constructor surface exist. Linear/unknown rendering refuses with
`palette:encoded-albedo-color-space-unproven`. Unity's
[2018.4 sRGB import contract](https://docs.unity3d.com/ru/2018.4/ScriptReference/TextureImporter-sRGBTexture.html)
describes the sRGB sampling conversion as relevant in Linear rendering; no
unobserved source encoding is assumed for that pipeline. Pixel transformation
uses decoded Color32 channels and keeps the original alpha byte. Allowed native
color formats are RGB24/RGBA32/ARGB32/BGRA32/DXT1/DXT5; higher-precision alpha or
other channel formats refuse instead of silently quantizing them.

The helper clones only the selected material slot and creates a private albedo.
Other maps and native UV transforms are retained. It invokes the existing native
material-controller reinitialization seam and adopts only the exact clone or its
single uniquely named controller instance with the same private texture/shader.
Repeated apply checks exact renderer/mesh/material/texture/map/UV ownership.
Foreign refresh refuses and retains a refusal marker rather than adopting the
new model. Teardown restores only its still-owned slot. Unbound owned resources
are destroyed; foreign-bound resources are preserved and explicitly reported
as retained, not counted as successful cleanup. A per-view component provides
teardown even for a partial attach. Integration must call `ReleaseAll` at module
shutdown and verify actual controller behavior and zero retained resources.

Existing Stone/Fire registrations still tint every eligible renderer/material
through `ExpandedSummoningVisualVariantPatch`; Fire also affects the cape.
Their historical tint persistence does not prove role separation or owner
approval. Do not stack this new helper on that owner. A reviewed replacement
must remove the superseded registration and verify body/outfit/cape/armor
controls before activation. Hill and retained Frost receive no palette rule.

## Checks and integration work

Four focused domain cases are supplied in `Cleanup2GiantPaletteTests`:

- `ExactBodySelectorsAndSummonScope`
- `ProofRequiresExactIdentityAndWholeBodyTreatment`
- `WholeBodyTransformsLiftCloudAndRetainAlphaAndShading`
- `TransformRejectsUnboundedPixelsAndUnreviewedFamilies`

They exercise actual pure policy contracts with synthetic colors/evidence,
including native donor/module-off negatives, material suffix/case refusal,
missing/wrong proof, bounded channels, Cloud value lift, bronze direction,
luminance ordering and exact alpha. Synthetic proof is not native evidence.
These cases are **not registered, compiled or executed in this lane**, in
accordance with the no-build/shared-registration assignment. Source inspection,
the read-only installed API metadata inspection (0.203 s), and
`git diff --check` are the only completed checks here.

Still required: fresh exact Cloud/Storm renderer/slot/texture/format/readability
proof; integrator registration and production compilation; focused case
execution; actual native per-slot clone/controller restoration, simultaneous
instances, failure cleanup, foreign-refresh refusal, module off/on and despawn;
ordinary-zoom body/clothing/armor palette review. No build, game launch, actual
installation mutation, Git mutation or frozen worktree edit was performed.
