# Kingmaker Icon Overhaul — Codex Implementation Mission

**Owner:** Howie Reith\
**Repository:** `howardreith/KingmakerGunslingerAndOtherTabletopRules`\
**Prepared:** 2026-09-11\
**Recommended branch:** `codex/icon-art-overhaul`\
**Mission type:** Scoped artwork, presentation integration, and permanent authoring standards. Not a gameplay expansion or public release.

## 0. Outcome and completion contract

Implement the approved `Kingmaker_Icon_Overhaul_Plan.md`, including the owner's additional requirement for a durable, repository-discoverable icon-authoring guide that governs future work.

Deliver two outcomes together:

1. Beautiful, original, recognizable icons for the current elemental races, heritages, alternate traits, racial feats and supporting actions; original Teleport, Greater Teleport, and Word of Recall art; native-looking firearm monograms and Rapid Reload; preservation of already successful artwork.
2. A permanent guide, real visual references, a maintainable icon catalog, and validation hooks so future spells, feats, summon choices, and abilities follow the same strategy without needing the owner to repeat this conversation.

A successful build, valid image file, unique filename, non-null sprite, or attractive enlarged painting is not completion. The approved art must appear on the correct native UI consumers, read at actual size, survive packaging and regeneration, and preserve gameplay and saved identities.

**The owner approved the plan and requested implementation. That is not approval of artwork not yet produced.** First reach a representative pilot review. Proceed with full art production only after the owner approves the pilot's actual images. Final visual approval is separate from technical qualification. Do not invent either approval or interpret silence as consent.

This mission may span sessions. Continue routine engineering autonomously between the explicit approval/safety gates. Checkpoint work rather than replacing real asset production with scaffolds or stopping merely because one phase is finished.

## 1. Workspace, materials, and initial inspection

Expected local repository:

```text
C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger
```

Expected input folder:

```text
C:\Dev\KingmakerGunslingerLab\incoming-assets\icon-overhaul
```

The input folder contains this mission, the approved plan, `ICON-ART-GUIDE.seed.md`, `references/REFERENCE-INDEX.md`, ten original owner-supplied screenshots, and `SHA256SUMS.txt`. Screenshots contain both positive and negative examples; read their index before using them. They are references, not replacement icon assets, and they are not all approved art.

Read the mission and plan completely, then the repository's `AGENTS.md` and applicable child instructions. Inspect the current `README.md`, `TESTING.md`, `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md`, package/build scripts, current icon infrastructure, and relevant source map in the plan. Follow the installed image-generation skill's actual instructions when available.

The planning source baseline was `258decb8fc2dad58d7a096772b34ec4202d71bd5`; it was still the remote master head when this mission was prepared. This is a reference, **not an instruction to reset or check out that old commit**. Record actual local HEAD, branch, tracked/untracked changes, installed version, and authorized starting point. Reconcile relevant changes since the planning audit without merging unrelated branches.

Start on the dedicated branch from the current authorized, qualified base. Do not discard another agent's work, run destructive clean/reset, stash someone else's files, overwrite an existing branch, or commandeer another active mission. A worktree is acceptable only if repository tooling and the mandated push wrapper support it. Serialize all installation/runtime work against the shared game; a worktree does not isolate the installed mod. If another task is using the game or modifying the same files, identify the conflict before touching them.

Copy the accepted mission/plan into a suitable repository planning location if needed for durable project context. Do not copy the entire intake archive into the repo. Validate the staging hashes and note missing inputs precisely. The intake's absence need not block safe source inventory; it does block claiming to have used its visual references.

### Authorized operations and boundaries

This task authorizes edits to the assigned repository on its dedicated branch, local generated art and curated evidence, the project's approved build/test workflow, guarded reversible test deployment, repository reads/fetches, official technical documentation lookup, and use of an already available built-in image-generation tool for the requested assets. Respect the runtime's sandbox and approval controls.

No new paid API workflow, subscriptions, purchases, global installs, third-party asset downloads, credentials access, or machine-wide configuration changes are authorized. Do not install an image generator, image skill, Unity, fonts, or an operating-system update silently. Use the existing maintained toolchain where adequate. Explain a genuinely necessary dependency before seeking approval; no new runtime library is expected.

Do not inspect browsers, email, password managers, unrelated repositories, personal documents, or other games. Explicitly returned image-tool output paths are permitted asset inputs; that is not permission to inspect authentication files or unrelated tool history.

## 2. Protected boundaries

Preserve these unless the owner subsequently authorizes an exact exception:

- Accepted gunslinger action/deed artwork and assignments, particularly Repair Firearm, Reload Firearm, Quick Clear, and other already successful usable icons.
- Katana and Nodachi lettering. Wakizashi may receive only a narrowly scoped clipping correction after native inspection.
- Existing weapon/item illustrations, meshes, materials, shaders, character appearances, animations, VFX, audio, and AssetBundles. This mission must not regress the character-visibility repair.
- Native feat-root artwork such as Weapon Focus, genuine native spells, and unchanged native racial features.
- Existing blueprint GUIDs, saved feat parameters, serialized component identities, category values, prerequisite checks, racial stats, action costs, durations, resources, spell mechanics, vendor behavior, publication state, and compatibility handling.
- Retired/legacy identities and currently unpublished content. Do not expose new rifle/revolver choices or hidden repair/overhaul items to make an icon catalog symmetrical.

Record protected image hashes **and the live assignments that use them**. Hash equality alone cannot detect an accidental remap. Retain snapshots of relevant native donor icon assignments. Never repaint a native donor or mutate its shared texture to give an owned clone new art.

Existing inheritance/cloning of a mechanic does not justify inheriting unrelated art. Existing UI decoration does not justify baking a second border into a PNG.

## 3. Establish a precise icon-consumer inventory

Use the complete named-content inventory in the approved plan. Then enumerate actual owned blueprints and UI presentation overrides from source and the guarded live runtime. Follow roots, features, variants, granted facts, buffs, activatables, sticky-touch deliveries, item-to-spell references, and late compatibility/publication paths.

For every relevant consumer record its stable symbol/GUID or actual UI-entry identity; name and module; visibility/publication; native UI surface; current art source; planned art family; asset key or native monogram rule; and disposition. Distinguish these dispositions:

```text
original-required | intentional-family-share | native-semantic-reuse |
protected-existing | native-monogram | hidden-internal | review-out-of-scope
```

A native-monogram row is valid without an exported PNG. A hidden resource does not need a painting. A protected or reused sprite needs a reason. No target may disappear from coverage because its icon is assigned indirectly.

### Minimum current coverage

**Heritages:** General Ifrit, Lavasoul, Sunsoul; General Oread, Gemsoul, Ironsoul; General Sylph, Smokesoul, Stormsoul; General Undine, Mistsoul, Rimesoul.

**Core presentations:** the four race identities/heritage roots; Fire/Acid/Electricity/Cold resistance; twelve heritage affinity presentations; the ten current alternate-trait selectors and ten no-additional-replacement choices. Verify these counts against the actual catalog. Do not invent missing Undine selection slots. Native Keen Senses and Slow and Steady remain native.

**Eleven racial feats:** Elemental Strike; Scorching Weapons; Inner Flame; Blazing Aura; Firesight; Airy Step; Wings of Air; Cloud Gazer; Inner Breath; Hydraulic Maneuver; Triton Portal.

**Twenty-one alternate traits:**

| Race | Required named identities |
|---|---|
| Ifrit | Wildfire Heart; Brazen Flame; Fire in the Blood; Efreeti Magic; Forge-Hardened; Fire Insight |
| Oread | Crystalline Form; Earth Insight; Granite Skin; Stone in the Blood; Treacherous Earth |
| Sylph | Air Insight; Breeze-Kissed; Like the Wind; Secretive; Storm in the Blood; Thunderous Resilience; Whispering Wind |
| Undine | Acid Breath; Nereid Fascination; Ooze Breath |

At the inspected baseline all twenty-one are published. Older nineteen-trait reports are historical; do not re-defer Nereid Fascination or Treacherous Earth.

**Custom racial actions:** Hydraulic Push; Hydraulic Maneuver parent and Bull Rush/Disarm/Trip/Dirty Trick (Blind) children; Unerring Weapon parent and primary/secondary-hand choices; Chill Touch cast/delivery; the feat actions and visible effects; Breeze-Kissed Gust, Bull Rush, Trip, Calm Winds, Renew Winds, and Winds Calmed; Deflect Next Ray and Crystalline Deflection Ready; applicable blood-healing, breath, fascination, and terrain effect consumers.

**Strategic spells and scroll items:** Teleport, Greater Teleport, Word of Recall, plus Scroll of Teleport, Scroll of Greater Teleport, and Scroll of Word of Recall. Cover learning/preparation/spellbook/tooltip surfaces for the spells and the actual icon-bearing item/tooltip/inventory/merchant/strategic surfaces for the scrolls. Preserve native scroll parchment only where it is genuinely generic item presentation; the visible scroll glyph/icon assignment itself is explicitly in scope.

**Weapons:** P/M/B across Weapon Focus, dependent firearm feat choices, Rapid Reload children, and applicable selected/character-sheet presentations; Rapid Reload parent emblem; inspect Elven Branched Spear and the isolated Wakizashi clipping issue.

One concept may deliberately share artwork across its feature, action, and effect. Different selectable actions must be distinguishable, not merely duplicate PNGs with different filenames. Do not inflate the project by drawing separate art for invisible plumbing.

### Semantic preservation

Keep native artwork on actual Burning Hands, Stone Fist, Feather Step, Firebelly, Flare Burst, Color Spray, Expeditious Retreat, Shocking Grasp, Blur, Enlarge Person, and Reduce Person consumers. A Mistsoul heritage marker needs original art; its actual Blur ability does not. Preserve the published native adaptations rather than implementing their tabletop alternatives.

For the strategic scrolls, inspect item-icon assignments separately from the canonical spell icons. If a strategic scroll currently inherits an unrelated donor such as Hold Person or Fireball, that is a defect to fix. The scroll may intentionally reuse its spell's central identity or an approved scroll-specific derivative, but the final item icon must clearly correspond to its own spell and be documented as an explicit assignment rather than accidental donor carry-through.

Read mechanics before finalizing a brief. Triton Portal is the existing water-elemental summon, not strategic travel. Wildfire Heart is initiative, not healing. Insight traits concern summoning duration. Secretive and Whispering Wind are different concepts. Thunderous Resilience concerns sound. Chill Touch's name is not sufficient evidence of its damage type. Original art must not falsely advertise a mechanic.

### Overlooked-content audit

Perform a bounded keep/replace/review audit of Acadamae Graduate, Bodyguard/In Harm's Way/Helpful, Brown-Fur, Urban Barbarian, expanded summoning, and other genuinely added content encountered in the census. Shield Other already has project-original spell/buff art and is a preserve candidate. Report concrete defects outside the approved main scope; do not silently redesign those families. Future Magic Circle, Lunge, and new summons are guide examples only in this mission.

## 4. Permanent repository standard — mandatory deliverable

Create or consolidate the following, avoiding competing sources of truth if an equivalent already exists:

```text
docs/ICON-ART-GUIDE.md
docs/art/ICON-REFERENCE-INDEX.md
assets-source/original-icons/icon-catalog.json
```

Use `ICON-ART-GUIDE.seed.md` as a starter, not a substitute for inspecting the implementation or filling in real references and measured rendering contracts. Keep the permanent guide focused on reusable policy. Keep this mission's progress, machine paths, and temporary blockers in mission state instead.

### Guide content

The permanent guide must teach the entire future workflow:

- Determine whether the visible concept is new, an exact native reuse, a new variant, a category monogram, or hidden plumbing.
- Choose the correct style: painted magical/racial art; native-style mundane feat emblem; native ornamental text; creature/summon illustration as an extension of the painted family.
- Inspect and use named visual references. A prompt saying only "Kingmaker style" is inadequate.
- Build a semantic brief from the authorized implemented behavior, with related and easily confused concepts explicitly identified.
- Produce/approve a representative design, export from preserved source, integrate through the established cache/mapping path, test real consumers, and record owner approval separately.
- Explain how to reuse approved family motifs without producing indistinguishable recolors, tiny labels, or baked-in dynamic state.
- Describe actual measured export profiles: UI-supplied decoration, display sizes, source size, runtime format, alpha/color treatment, foreground safe bounds, filtering, packaging destination, and the evidence establishing each. Do not invent universal 64px/128px or ring-width requirements.
- Include a fillable brief/prompt template, source/provenance requirements, approval rules, a small-size review rubric, prohibited shortcuts, and a checklist for every future added icon or newly cloned visible blueprint.
- Explain how an intentional keep/native-reuse exception is documented, and how guide changes require review rather than allowing the next generator to silently redefine the style.

### Actual visual references, not aspirational prose

Populate `ICON-REFERENCE-INDEX.md` with real inspected examples from the protected project art and approved pilot/finals. Include exact relative paths, hashes, style family, approval scope/evidence, and what each example demonstrates. Preserve an anchor per mature family; do not gradually replace the anchors with the latest unreviewed output.

Use existing original source paths rather than duplicating large files needlessly. A small project-owned contact sheet is useful. Clearly label rejected historical procedural monograms/Rapid Reload as negative examples, not style authorities. A screenshot that includes both good and bad icons must be annotated in the index by subject/region.

Native game screenshots and extracted native references stay local/reference-only unless separately approved for repository publication. Record their source blueprint/UI entry and a reproducible local capture route. Do not commit or ship extracted game textures or font binaries. Do not claim "no third-party input" when native screenshots were actually supplied as style inputs; distinguish stylistic reference from incorporation of source pixels. Reference-only material is not production art.

### One maintainable catalog

Inspect current manifests first. Prefer extending an existing equivalent over introducing a second authoritative mapping. If a new `icon-catalog.json` is needed, define its relationship to existing legacy export manifests explicitly: each key/consumer has one authority, not two unsynchronized lists. The public guide must link to the actual canonical path.

Track concept/key, art family, source and export paths/hashes, export profile, exact owned consumers, intentional sharing/native-reuse explanations, protection state, provenance, and **separate technical and visual-review status**. Reference review evidence and the reviewed image hash. Replacing pixels invalidates that image's previous approval. Never auto-mark a new asset visually approved because its parent family is approved.

The catalog must support native-monogram and hidden rows without demanding a PNG. It must not become a runtime dependency on a JSON library or a new art framework. A build-time validation catalog plus narrow existing C# mappings is acceptable if the validator checks their agreement.

### Make future agents actually find the guide

Add a concise section to root `AGENTS.md`, preserving every existing safety rule. Adapt the following wording to final canonical paths:

> For any task that creates, replaces, clones, or remaps a player-visible icon, or adds a visible spell, feat, racial trait, ability, buff, item, or summon choice, read `docs/ICON-ART-GUIDE.md`, `docs/art/ICON-REFERENCE-INDEX.md`, and the guide's canonical icon catalog before deciding its presentation. Follow the matching approved visual family and actual references. Record every touched consumer's original/reuse/share/protected/hidden disposition. Use native monograms for category notation where supported, never unrelated donor art as a completed new icon. Validate exports, packaging, protected assignments, and real UI use. Technical PASS is not owner visual approval. Update the guide/catalog when the authoring contract changes; do not silently redefine an approved family.

Link the guide from the relevant developer documentation/README. If existing repository-local agent/skill instructions would hide this rule, reconcile the narrow trigger without weakening other instructions. Do not add a global machine configuration or a large duplicated skill policy. A repo-local skill is optional; any wrapper must point to the one canonical guide, not fork it.

Add lightweight validation for required docs, reference paths/hashes, catalog consistency, and newly changed target coverage. Avoid making every build re-render or regenerate artwork. Perform a read-only fresh-context discoverability check: starting from root instructions alone, demonstrate the correct guide/reference lookup for the three future examples below. Do not spawn nested autonomous agents or access a new service solely for this check if not available/authorized.

### Future examples required in the guide, not in gameplay

**Magic Circle Against Alignment:** a new ward spell uses original painted, group/circle-oriented protection art; related alignment variants should share a family while remaining distinguishable. Read the eventual authorized design to determine which variants exist. Do not reuse Protection from Alignment unchanged just because the mechanic is related.

**Lunge:** a mundane combat feat uses the native emblem vocabulary with a readable extended-thrust/reach gesture. Do not turn it into a painted magic attack or invent activation mechanics. Any future action/buff consumer follows the actual implemented feature graph.

**New summonable monsters:** preserve spell-root identity and native variant presentation; give new creature choices recognizable species/body silhouettes in the established painted summoning family. Exact native creature/effect reuse may retain its correct native art. New species must not inherit an unrelated creature icon. Handle template/count/alignment differences using established UI conventions; do not bake dynamic counters or add misleading anatomy/equipment. This is not authorization to implement or illustrate new monsters now.

## 5. Art-production method and pilot gate

### Verify capability first

Inspect the active Codex environment's actual tools/installed image skill. For painted icons, use its built-in image-generation capability when available, passing inspected visual references. Do not pretend an image file exists or that a tool is available without executing/observing it.

Built-in image generation and an API script are separate workflows. Do not ask for an API key for the built-in tool or silently switch to a separately billed CLI/API route. No image API key is an initial material requirement for this mission. If the built-in route is unavailable, complete safe inventory, native-render prototypes, templates, and asset briefs, then record `BLOCKED_IMAGE_PRODUCTION` with exact assets and references needed. A separate owner-approved image-producing session or artist may supply those files through `incoming-assets\icon-overhaul\art-intake\`. Report the actual limitation; do not silently approximate paintings with SVG shapes, Pillow drawings, procedural silhouettes, borrowed assets, or recolored spells.

Procedural/vector work is appropriate for measured flat emblem geometry and deterministic exports. It is not prohibited generally; it is prohibited as an undisclosed quality downgrade for requested paintings. Generated category typography is not the preferred path at all.

Copy project-bound generated originals from the tool's exact returned output paths into the project asset-source workflow. A temporary tool cache path cannot be the only source of a committed runtime icon. Preserve approved originals byte-for-byte and record subsequent transformations.

### Establish measured references

Use the ten supplied screenshots, accepted source art already in the repo, and permitted native inspection. Compare stroke weight, palette, edge/background treatment, composition, visual density, and actual cell dimensions. Determine which frames, backgrounds, rings, disabled tints, selected outlines, counters, and active indicators are UI-provided. Do not bake those into the art twice.

Treat the four elemental palettes as secondary structure. Functions must be recognizable by silhouette/composition even without hue: ancestry versus affinity versus resistance; eye-through-fire versus eye-through-cloud; fast stride versus defensive step; acid spray versus viscous ooze.

### Required pilot

Produce a small reviewable pilot before the full collection:

- P/M/B native monogram presentation beside native and accepted eastern examples; corrected Rapid Reload emblem beside native feat emblems.
- One representative heritage/affinity/resistance group showing three different roles, not one elemental crest repeated.
- Elemental Strike and a Hydraulic Maneuver parent/child pair demonstrating a balanced multi-element symbol and readable water-force action.
- Teleport and Greater Teleport as distinguishable siblings, plus the Word of Recall direction.

Provide actual-size previews, enlarged inspection images, grayscale comparisons where useful, and real native UI captures where the authorized path allows. Label mockups or live-sprite facsimiles honestly. Show a small number of serious candidates; do not bury the owner in a hundred variations or proceed to large-batch generation before a direction is approved.

Run your own visual review first and iterate on concrete problems. The pilot handoff must identify exact files/hashes and which family decisions are awaiting approval. Set `AWAITING_PILOT_APPROVAL` and checkpoint. Independent safe documentation/infrastructure work may continue, but not full unapproved art production. Approved-plan language is not a bypass for this gate.

## 6. Firearm lettering and Rapid Reload implementation

The audited eastern/custom route in `CustomWeapons/CustomWeaponSelectorRuntime.cs` passes a null sprite and a monogram into `FeatureUIData`. `Feats/NativeFirearmFeatIntegration.cs` currently passes a firearm parameter sprite. This is a presentation difference, not a reason to convert saved firearm feat parameters to weapon-category parameters.

Prototype the native monogram route for P/M/B in the **actual firearm entry path**. Retain `FeatureParam(parameter)`, existing GUIDs, filtering, prerequisites, ordering, and attack/damage/critical behavior. Inspect how native UI derives letters and backgrounds; do not assume passing a full display name or setting every underlying feature icon null is safe.

Cover full selection, level-up extraction, nested Rapid Reload weapon choices, chosen feat presentation, and character sheet where applicable. A selector's null-sprite override must not break consumers that require a blueprint sprite. Prefer native rendering there too; if a particular surface truly requires an image, isolate and document a reference-matched fallback and obtain visual approval. Any live native-rendered fallback must not redistribute font binaries or extracted game art. Do not weaken all non-null checks or add a global icon/font getter patch.

Preserve native parent feat emblems. Do not replace Weapon Focus's own icon with a P or a gun. Do not change proficiency eligibility to get an attractive menu screenshot. Check firearm parameter persistence on a permitted existing disposable save.

Rapid Reload needs the measured native combat-feat treatment and an original, readable loading symbol. Fix the heavy incomplete-ring mismatch; distinguish loading from repairing. Preserve the Reload Firearm action image. Inspect `tools/New-FirearmFeatIcons.ps1` and `tools/icon-art/New-IconOverhaulAssets.ps1`; redirect or retire the affected regeneration branch so it cannot restore the rejected art. Do not run a broad "All" generation mode that rewrites protected action/item assets.

Inspect Wakizashi clipping and Elven Branched Spear locally. If a correction is necessary, scope it to the exact monogram/UI entry and demonstrate other native/eastern names are unchanged. No global font-size change.

## 7. Full asset production and narrow integration

After pilot approval, produce small coherent families using the approved references and semantic briefs in the plan. The brief for every original must specify subject/action, strongest silhouette, relative family, confused-with entries, UI surface, export profile, and forbidden interpretations. Avoid a giant generated sheet followed by guessed grid cropping. A contact sheet is a review artifact, not a replacement for individually preserved source files.

Create original identities for all required named concepts; use intentional sharing across equivalent feature/action/buff consumers. Hydraulic maneuver choices and wind-control choices require readable sibling actions. Hand variants require correct handedness cues, not ambiguous mirrored pictures. Keep native active/disabled/stack/remaining-use overlays outside baked pixels.

Use three original strategic spell icons with a common Teleport/Greater Teleport family and a different recall-to-sanctuary idea. Do not make Greater Teleport only a brightness change. Word of Recall must not be tied to one deity. Preserve strategic-only casting and action-bar autofill behavior; use the actual spellbook/strategic surface rather than enabling local casting for screenshots.

Inspect existing loader initialization order and late icon assignment. Prefer icon assignment at owned construction or one explicit, idempotent post-registration stage that cannot later be overwritten. Extend `Blueprints/ProjectAssetIcons.cs` or an equally narrow established service; do not add another competing cache or expand the generic substring-based recursive fallback to these families.

Assign by stable owned identity with validated native-reuse exceptions. Respect optional-module OFF/save-hydration and compatibility boundaries; no foreign-blueprint traversal/repainting. A missing required final export is a validation failure, not permission to silently use Burning Hands. During development the last known working asset may remain until a replacement exists, but its status stays incomplete.

Load files once, cache sprites, use the game's appropriate main-thread lifecycle, and avoid per-frame disk IO, repeated Texture2D allocation, or broad UI patches. Preserve established disposal/ownership behavior; do not destroy shared native textures. Add no gameplay state solely to store an icon.

### Export and package contract

Keep high-resolution/source compositions under the established `assets-source/original-icons/` organization. Keep runtime exports under `assets/game/icons` or the repository's existing family subdirectory where packaging supports it. The inspected installed loader uses `assets/icons`, which is distinct from the source path. Any new subdirectory must be explicitly supported by loader, build copier, and package validation.

Determine dimensions from measured contracts, not one universal size. Review actual-size and 32px, roughly 40–48px, and 64px thumbnail reductions as stress tests, plus larger tooltips when used. Preserve alpha correctly, inspect on light/dark backgrounds, avoid green chroma halos and destructive global green removal in green elemental art, maintain color/edge treatment, and protect important foreground padding. Full-bleed painted backgrounds are not inherently clipping defects.

Deterministic export starts from a preserved approved source. It never calls a generator to invent replacement art during build. Record exact tool/settings and hashes, remove timestamp nondeterminism where relevant, and test repeat export. Do not re-encode protected assets as incidental "normalization."

Update source/export metadata, loader registrations, tests, and obsolete generator authority together. Existing hardcoded build-version guards must be respected; do not bypass them or rewrite unrelated release metadata. Versioning follows the current project convention, with exact runtime expectations read from the artifact actually tested.

## 8. Validation and acceptance evidence

### Source and asset checks

Use the actual repository commands and supported parameters after inspecting them. The audited `scripts/Build-Local.ps1` already runs repository validation and the full clean Release domain suite, builds from qualified private references, runs supply-icon checks, copies icon exports, and validates build output. Avoid redundant expensive full runs when the documented orchestrator already supplies the gate, but do not omit a required step. Read the rest of the current script before assuming whether it also packages or deploys.

Add focused tests/validation for:

- Exact required content/consumer coverage and explicit exceptions; no hidden/unpublished content made selectable.
- Stable symbols/GUIDs, unchanged blueprint-based firearm parameters and native parents, unchanged prerequisites/mechanical components outside permitted presentation edits.
- Correct owned icon assignments after initialization and late publication/compatibility reconciliation; native donors untouched.
- Correct action/variant/buff/held-touch relationships, including native spell reuse exceptions.
- Valid dimensions/format/alpha, source/export existence and matching manifest hashes, package destination/case, deterministic export, and no accidental duplicate artwork for distinct action choices. Intentional sharing must be allowlisted by concept, not hidden by different filenames.
- Protected bytes and assignments unchanged; native/eastern controls intact.
- Permanent guide/reference paths, valid metadata, and no missing or falsely approved example. The catalog must fail on a known fixture with an omitted new consumer, a mismatched export, or a stale protected hash.

Prefer behavior-level integration with real runtime objects and the existing domain/reflection harness. Do not substitute mocks that merely assert an icon setter was called. New tests should match the project's style and remain proportional to this presentation-only change.

### Runtime safety

Read and obey `AGENTS.md`, `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md`, and the exact relevant scenario documents before a launch. Use the guarded `-kmgRuntimeTestRequest` workflow through **Steam App ID 640820**, not direct `Kingmaker.exe`. Discover allowed scenario names/arguments from current source; do not copy an old version literal into a new run or invent accepted parameters.

Use no-save fixtures where possible. Save-backed smoke testing uses only the precisely identified authorized disposable `KMG_AUTOMATION_WORKING` through its documented harness. Never select, load, rename, delete, or write `KMG_AUTOMATION_BASELINE`. Do not touch campaign saves. If the working save or required references are absent, report the missing fixture instead of inventing one from personal saves. New save creation/writes require separate authorization.

Temporary installation is limited to the existing guarded backup/deploy/restore path. Preserve the installed mod and settings, stop if shared state changes externally, and verify restoration. Do not terminate someone else's running game, force-kill, automate Steam login/update/cloud dialogs, bypass entitlement failures, or install proprietary tools. No unattended mouse-coordinate/OCR navigation is authorized. If native-screen capture cannot be achieved through existing permitted instrumentation, request a bounded supervised capture rather than broadening permissions.

Follow the repository's risk-based runtime matrix. Icon-only loading generally needs focused all-ON asset loading; selector presentation changes also require their actual menu/parameter behavior and affected optional-profile boundaries. Do not launch an exponential all-module matrix or redo unrelated mechanics without a concrete risk. Do not weaken existing mandatory gates.

### Distinguish three evidence classes

1. **Mechanical:** structured checks against real loaded blueprints/production UI-entry objects and the executing artifact.
2. **Art inspection:** contact sheets and correctly labeled live-sprite facsimiles, useful for evaluating the images themselves.
3. **Native UI:** screenshots of the real relevant game screens with the exact package installed, needed to evaluate clipping, overlays, scale, placement, and actual consumer wiring.

The existing `icon-overhaul-visual-evidence` scenario renders live-sprite facsimiles, not native menu navigation. Reuse it for its legitimate purpose; do not relabel its outputs as proof of the real menus. It may need narrow coverage extensions. Screenshots can demonstrate aesthetic acceptance but cannot replace structured mechanical assertions.

Capture final-artifact evidence for character creation (new character and mercenary where the paths differ), heritage/alternate-trait choices, the racial feat and weapon menus, supported action/variant menus and visible buffs, and the three strategic spells' relevant UI. Use a disposable test caster or permitted fixture for high-level Word of Recall, not the owner's campaign progression. A lack of a high-level personal cleric is not a reason to omit the spell.

Record expected/observed source state, DLL/package/export hashes, mod version, profile, run ID, and evidence class. If a candidate changes, repeat the impacted final-artifact checks. Earlier screenshots do not automatically qualify changed pixels or bindings. Keep runtime assertions, visual review, and owner approval as separate statuses.

### Visual acceptance rubric

Every required family must satisfy all of these:

- Attractive, deliberate art with a strong readable subject at native display size.
- Different concepts/actions distinguishable by silhouette or composition, not just hue or tiny text.
- Correct family vocabulary beside native/approved examples.
- No unrelated borrowed spell/monster art disguised by renaming, tinting, or a badge.
- No clipped important foreground, muddy reduction, chroma halo, doubled frame, or baked-in dynamic state.
- Correct art on every required visible consumer and preserved protected/native examples.
- Reproducible export and durable owner-reviewed source/hash record.

Machine image metrics cannot establish beauty or owner preference. A family can be technically ready while awaiting visual acceptance; label that honestly. Do not declare "everything passes" while an entire evidence class is NOT RUN.

## 9. Checkpoints, approval gates, and handoff

Maintain a compact durable state and journal, for example:

```text
planning/ICON-OVERHAUL-STATE.md
planning/ICON-OVERHAUL-JOURNAL.md
reports/icon-overhaul/IMPLEMENTATION-REPORT.md
```

Track starting/current commit, branch, actual input paths, completed/remaining consumers, protected baseline, image capability, candidate and approved asset hashes, pilot decisions, guide/reference/catalog status, commands and evidence, local installation restoration, and the exact next action. Keep raw machine-local runtime artifacts outside Git under the approved lab evidence root; only curated permitted material belongs in the repo.

Suggested milestones:

- **A — Inventory and durable standard:** complete source/runtime census as available, protected baseline, guide/reference/catalog skeleton populated with real inspected legacy references, and AGENTS discovery rule.
- **B — Pilot:** native firearm prototype and representative candidate artwork, focused validation, explicit owner review handoff. Stop mass art production at `AWAITING_PILOT_APPROVAL`.
- **C — Production:** approved family direction extended across the complete main scope, narrow mappings and reproducible exports, current guide/reference/catalog updated from actual successful work.
- **D — Final qualification:** package/structured/native-UI evidence, protected regression checks, final visual-review packet, and verified restoration.
- **E — Final acceptance:** owner approval recorded against exact final images; final handoff states any remaining exceptions explicitly. No public release/merge is authorized.

Intermediate milestones are not substitutes for later work. After pilot approval, resume from state rather than starting the audit or generating the approved anchors again. After a quota/session interruption, leave the exact next step; do not claim an unscheduled process will restart itself automatically.

Perform the gates appropriate to a coherent change before committing, following AGENTS. Documentation-only checkpoints require documentation validation, not a game launch. Candidate asset or implementation checkpoints may be clearly labeled review-pending if technically valid; never label them release-ready or approved.

After coherent commits and before a pause/handoff, use the exact project checkpoint wrapper:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

If the wrapper denies the branch or environment, do not bypass it, alter its policy, push master, or use an unapproved alternate credential route. Record the specific restriction. Never force-push, rewrite history, merge, tag, or publish a release. Do not upload the intake screenshots or proprietary references in a blanket staging/commit operation.

### Genuine pause/blocker conditions

Pause the affected work for owner pilot/final art approval; missing authorized image production; a required paid/API/install decision; protected-art or scope exceptions; unrecoverable source/runtime ambiguity after safe narrowing; absent/ambiguous disposable saves; concurrent installed-game ownership; Steam/credentials/cloud/entitlement dialogs; denied repository policy; or a real unavailable prerequisite. Report precise evidence and the smallest needed decision/material. Continue independent safe work when useful.

Repeated tests failing is not a reason to skip them or assert success. Inspect evidence, reduce the fixture, improve narrow instrumentation, or revise implementation while remaining within authorization. Do not make the owner resolve routine naming, file-layout, or engineering choices already governed here.

### Required final response from Codex

Provide a concise handoff containing the branch and exact commits; changed/preserved/native-reused families; complete coverage counts; the canonical guide/reference/catalog paths; tests actually run; labeled screenshot/review paths; final artifact hashes; genuine owner approvals and pending gates; installation restoration status; and any out-of-scope audit findings. State explicitly whether the work is only technically ready or also visually approved. Do not claim a release or master merge.

The mission is complete only when the requested icon families and their actual UI consumers meet acceptance, the permanent standard is discoverable and populated with verified references, protected behavior/art remains intact, evidence belongs to the final artifact, and outstanding approval gates are resolved. A legitimate review-pending handoff is a checkpoint, not a fabricated completion.

## 10. Preparation sources and authority

The approved companion plan contains the source-file map and individual art briefs. Re-verify exact implementation details at execution time. Preparation also read root `AGENTS.md`, `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md`, and `scripts/Build-Local.ps1` from the repository on 2026-09-11.

OpenAI's official AGENTS documentation describes startup discovery; its official imagegen skill distinguishes built-in generation from explicit API fallback. Consult the active installed skill rather than freezing model names or API flags in this permanent project guide. Preparation references:

```text
https://developers.openai.com/codex/guides/agents-md
https://developers.openai.com/codex/skills
https://github.com/openai/codex/blob/main/codex-rs/skills/src/assets/samples/imagegen/SKILL.md
```

This mission does not install tools, modify the remote repository, authorize future gameplay additions, or certify any artwork. Those actions and evidence occur only during execution within the boundaries above.
