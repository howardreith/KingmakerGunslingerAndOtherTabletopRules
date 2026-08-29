# Kingmaker Gunslinger Icon Overhaul and Unsupported-Firearm Cleanup

You are a senior **Pathfinder: Kingmaker Unity mod engineer and technical artist**. Work autonomously in the existing Gunslinger mod repository. Inspect the repository, implement the changes, build and package the mod, test it, and visually verify the result in game. Do not merely give me a plan.

## Branch and repository safety

- Work only on a new local branch named **`codex/icon-art-overhaul`**. If that branch already exists, verify that it is the intended unfinished branch before continuing.
- Do not push, fetch, pull, open a PR, create a remote branch, or otherwise modify any remote repository.
- Before editing, record the repository root, current branch, HEAD, remotes, and `git status --short`.
- Do not overwrite unrelated dirty work. If the working tree is dirty, determine whether the changes belong to this mission; preserve anything unrelated.
- Do not modify the installed base game. Only modify the mod repository and the packaged mod output.
- Preserve existing blueprint GUIDs, asset names, and save compatibility wherever practical.

## Visual references

The supplied screenshots are the authoritative **before-state and style references**:

- `references/originals/01_rapid_reload_feat_list.png`
- `references/originals/02_rapid_reload_weapon_choices.png`
- `references/originals/03_weapon_focus_blunderbuss_reference.png`
- `references/originals/04_weapon_focus_musket_pistol_nodachi_reference.png`
- `references/originals/05_inventory_weapon_icon_comparison.png`
- `references/CONTACT_SHEET.png`
- `VISUAL_NOTES.md`

Read the full-resolution originals, not only the contact sheet. Where the installed game exposes vanilla icon textures or equivalent local references, use them to understand dimensions, margins, palette, alpha treatment, line weight, and visual scale. Do **not** redistribute extracted vanilla artwork. The screenshots and existing in-repository assets are sufficient to reconstruct original mod artwork in the same visual language.

## Scope boundary

This mission covers only:

1. the Rapid Reload feat icon;
2. firearm category/parameter icons used by Rapid Reload, Weapon Focus, and similar selectors;
3. inventory/item icons for supported firearms;
4. inventory/item icons for all mod-added eastern weapons and the Elven Branched Spear;
5. retiring Rifle and Revolver from every normal player-facing surface.

Do not redesign unrelated class, grit, deed, spell, feat, or item icons.

# Phase 1 — Audit before editing

Inspect the icon and blueprint pipeline before changing anything. Produce an internal manifest that identifies:

- every source-art file and generated texture used by Rapid Reload;
- every icon mapping used for firearm weapon categories or parameterized feat selections;
- every firearm item blueprint and every icon it references;
- every mod-added eastern weapon item blueprint and every icon it references;
- every Elven Branched Spear blueprint/variant and every icon it references;
- every code path, table, selection, vendor, loot entry, starting-equipment grant, crafting/repair path, localization entry, or generated parameter list that exposes **Rifle** or **Revolver**;
- the native pixel dimensions, texture format, import/loading path, pivot, pixels-per-unit, filtering, and compression conventions used by the mod and comparable Kingmaker icons.

Search by display names, internal names, identifiers, enum/category values, blueprint GUIDs, localization keys, file names, and abbreviations. Do not assume that a single text search is complete.

Before implementation, write a concise audit section into `docs/reports/icon-overhaul-report.md`. Update that report as work proceeds.

# Phase 2 — Retire Rifle and Revolver from official support

The officially supported firearm set must become exactly:

- **Blunderbuss**
- **Musket**
- **Pistol**

Rifle and Revolver may remain as low-level/internal blueprints when deleting them would risk save compatibility or prevent Toy Box users from deliberately spawning them. However, they must disappear from all ordinary gameplay and all official mod surfaces.

Remove or suppress Rifle and Revolver from, at minimum:

- Rapid Reload and all Rapid Reload sub-selections;
- Weapon Focus and every other parameterized weapon-category feat or feature selection in which the mod makes them available;
- class/archetype weapon choices and starting-firearm choices;
- starting equipment, automatic grants, replacement grants, and fallback grants;
- vendors, loot, containers, encounter rewards, crafting, repair-kit flows, ammunition sales, and any other ordinary acquisition path;
- UI menus, discoverable encyclopedia/help entries, user-facing configuration, and current documentation that presents them as supported content;
- automated registries or category enumerators that would cause them to reappear indirectly.

Do not delete Rifle or Revolver from existing save files, and do not introduce crashes when an old save already contains one. Treat such old or Toy Box-injected items as unsupported-but-tolerated legacy content.

Add or update tests so that the supported firearm set is asserted as exactly Blunderbuss, Musket, and Pistol, and so Rifle/Revolver cannot silently return to the relevant selection and acquisition surfaces.

# Phase 3 — Restyle the Rapid Reload feat icon

The current Rapid Reload icon is recognizable, and its basic **circular reload arrow plus tool** concept should remain. Rework it so it belongs beside vanilla feat icons such as Shield Proficiency, Shake It Off, and Skill Focus.

Required changes:

- Replace the current off-palette red with the same family of muted Kingmaker feat-icon red used by comparable vanilla icons. Sample from accessible local references or visually match the supplied screenshot; do not guess a bright modern red.
- Enlarge the circular reload-arrow motif so its visual mass and diameter are comparable to neighboring vanilla feat emblems.
- Enlarge the tool slightly and rebalance/reposition it so it is clearly legible at the actual in-game display size without crowding the circle.
- Remove the small blue corner decorations entirely. They are not part of the desired design language.
- Preserve clean antialiasing, intentional negative space, and the expected frame/background treatment.
- Verify the icon both in the main feat list and anywhere else the feat icon is reused.

Do not solve this by upscaling the existing flattened PNG until it becomes blurry. Use the cleanest available source or reconstruct the motif at higher resolution and downsample properly.

# Phase 4 — Restyle firearm category/selection icons

The firearm icons used in Weapon Focus, Rapid Reload sub-selections, and similar weapon-category selectors should follow the same visual system already used successfully by the mod's Nodachi and other eastern-weapon category icons.

Create or revise category icons for only:

- **Blunderbuss** — decorative `B`
- **Musket** — decorative `M`
- **Pistol** — decorative `P`

Required style:

- a full square icon rather than the current pale inset card;
- a subdued Kingmaker-compatible gradient or tonal background based on the established eastern-weapon category template;
- a decorative, highly legible initial/monogram centered with the same margin, weight, and ornamentation as the good Nodachi reference;
- a coherent border/frame treatment matching other weapon-category icons;
- no blue corner glyphs or one-off decorations;
- clear readability at the small size shown in selection lists.

Centralize or reuse the icon mapping so the same category has the same icon in Rapid Reload, Weapon Focus, and every other relevant selector. Do not create Rifle or Revolver replacement icons because those categories must no longer be offered.

Use only artwork, templates, and fonts that are already valid for this repository, or construct the lettering as original vector/path artwork. Do not introduce a runtime dependency or an unlicensed font asset.

# Phase 5 — Restyle supported firearm inventory/item icons

Inspect every item blueprint and variant for Blunderbuss, Musket, and Pistol. Revise their item icons so they resemble ordinary Kingmaker weapon icons while preserving each firearm's recognizable silhouette.

Required changes:

- Remove the dark rectangular border/background that currently makes firearm icons visibly unlike vanilla weapon icons.
- Use a transparent canvas and the same general alpha-edge treatment, lighting, contrast, and soft shadow convention as comparable Kingmaker weapon icons.
- Retain the existing successful general orientation: lower-left to upper-right.
- Scale and place each weapon so it uses the available diagonal confidently, with only the normal small safety margin and no clipping.
- Do not distort a pistol into the proportions of a long gun merely to fill the canvas; fill the available space naturally for its silhouette.
- Ensure all mundane, masterwork, cold-iron, enchanted, named, or otherwise variant blueprints use the intended corrected icon rather than leaving stray old bordered assets.

Do not paste screenshots, crop a vanilla weapon, or replace the firearms with generic crossbows. These must remain visibly a blunderbuss, musket, and pistol.

# Phase 6 — Restyle eastern-weapon and Elven Branched Spear item icons

Enumerate the complete set from the repository; do not limit the work to the few names visible in the screenshots. This includes every mod-added eastern weapon category and all its item variants, plus every Elven Branched Spear variant.

The current problem is excessive empty space and overly horizontal placement, especially for the Wakizashi and Elven Branched Spear.

Required changes:

- Orient the weapon from lower-left to upper-right in the normal Kingmaker weapon-icon convention unless the silhouette truly makes that impossible.
- Scale the artwork so the weapon nearly fills the usable diagonal while retaining a small, consistent safety margin and no clipping.
- Preserve distinguishing shapes: blade curvature, guard, grip, branching spear head, pole length, and other category-defining details.
- Do not preserve real-world relative scale between weapon categories at the expense of icon readability; each icon should use its own canvas effectively.
- Remove avoidable horizontal presentation, excess transparent padding, and inconsistent scale between variants of the same category.
- Preserve transparent backgrounds and the standard Kingmaker item-icon lighting/edge treatment.
- Reuse one corrected category-appropriate base icon across variants when that is how the repository and game already handle mundane/masterwork/magical versions.

The Nodachi **category/monogram** icon is a good selector-style reference, but the Nodachi and Wakizashi **inventory weapon artwork** must still be audited under these diagonal-fill requirements.

# Phase 7 — Reproducibility and asset quality

- Prefer editing clean source art. If source art is absent, reconstruct cleanly rather than repeatedly transforming a low-resolution final PNG.
- Keep any useful SVG, layered source, or deterministic generation script under an appropriate development-only path such as `tools/icon-art/` or `art-src/`.
- Do not add runtime libraries merely to generate static art.
- Preserve the repository's existing naming and loading conventions unless there is a strong reason to improve them.
- Validate image dimensions, alpha channels, transparent corners, and texture loading with automated checks where practical.
- Add a lightweight asset-validation test or script that catches missing files, wrong dimensions, accidental opaque rectangular backgrounds, and blueprint references to retired or nonexistent icons. Do not rely on a crude pixel heuristic as a substitute for human visual inspection.
- Avoid lossy recompression that produces halos, muddy lettering, or color shifts.

# Phase 8 — Build, package, and verify in game

Run the repository's real build and test workflow. Repair failures caused by this mission.

Then package and install the resulting mod through the repository's established Kingmaker/UMM workflow and visually inspect it in game. Capture full-resolution **after** screenshots that reproduce these views as closely as practical:

1. the main feat list with Rapid Reload beside ordinary vanilla feats;
2. the Rapid Reload sub-selection, showing exactly Blunderbuss, Musket, and Pistol;
3. Weapon Focus or an equivalent category selector showing the corrected firearm monogram icons and no Rifle/Revolver;
4. an inventory/vendor view showing corrected firearm item icons;
5. an inventory/vendor view showing representative eastern weapons and the Elven Branched Spear at corrected diagonal scale.

Also verify relevant level-up and vendor/acquisition flows, not only static blueprint data.

Do not claim visual success based only on compilation. If the game cannot be launched in this environment, finish every static/code/asset task possible, provide deterministic previews at the actual target dimensions, and state exactly which in-game checks remain unverified.

# Acceptance criteria

The mission is complete only when all of the following are true:

- Rapid Reload visually fits the neighboring vanilla feat icons: correct muted red family, larger circle, slightly larger and better-spaced tool, and no blue corner decorations.
- Rapid Reload offers exactly Blunderbuss, Musket, and Pistol.
- Weapon Focus and every other normal player-facing weapon-category surface offers those supported firearm categories but not Rifle or Revolver.
- Blunderbuss, Musket, and Pistol selector icons use the established gradient-square decorative-monogram style.
- Supported firearm item icons no longer have the unique dark rectangular border and visually fit ordinary Kingmaker weapon icons.
- Every mod-added eastern weapon item icon and every Elven Branched Spear item icon has been audited and corrected to use the diagonal canvas effectively.
- Rifle and Revolver have no ordinary acquisition or selection path, while legacy/Toy Box-injected instances do not crash the game.
- The mod builds, tests, packages, and installs successfully.
- After screenshots and a complete changed-asset/changed-blueprint manifest exist.
- The working tree is clean except for intentionally retained deliverables.

# Final deliverables

Commit the completed work locally with clear, scoped commit messages. Do not push.

Return a final report containing:

- starting and ending branch/commit identity;
- a concise explanation of the icon pipeline and what changed;
- the complete list of changed source-art files, final textures, blueprint/code files, and tests;
- the complete list of eastern weapon categories and variants audited;
- the complete list of Rifle/Revolver surfaces removed or suppressed;
- build, test, package, and installation commands with results;
- package path, version, SHA-256, DLL SHA-256, and relevant MVID if the established project workflow records it;
- paths to every after screenshot and any before/after contact sheet;
- any remaining limitation stated plainly.

Use the references and your technical judgment. Do not stop to ask me to choose exact shades, icon scale percentages, or minor placements; make a coherent first-class pass, inspect it at actual in-game size, and iterate until it looks native to Kingmaker.
