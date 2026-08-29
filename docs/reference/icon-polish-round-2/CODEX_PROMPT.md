# Kingmaker Gunslinger Icon Polish — Round 2

You are a senior **Pathfinder: Kingmaker Unity mod engineer and technical artist**. Continue from the completed icon-overhaul version that was just packaged and tested in game. Implement this narrow visual revision, test it, package a new local version, and provide evidence. Do not merely propose changes.

## Authoritative baseline and branch safety

- The **exact local commit that produced the currently tested post-overhaul package** is the authoritative baseline. Do not restart from `main`, an older release, or the pre-overhaul artwork.
- First record the repository root, current branch, `HEAD`, `git status --short`, current mod version, and the path/hash of the most recent package if available.
- Verify that the baseline already contains the accepted results from the previous mission:
  - firearm inventory/item icons are corrected and look native;
  - eastern-weapon and Elven Branched Spear item icons are corrected;
  - the Rapid Reload feat icon is corrected;
  - Rifle and Revolver are retired from ordinary supported gameplay.
- If the current checkout is not that tested baseline, locate it using **local Git history and local release artifacts only**. Do not fetch or consult a remote.
- Create and work only on a new local branch named **`codex/icon-art-polish-round-2`** from that tested baseline. If the branch already exists, verify that it descends from the correct baseline before continuing.
- Do not push, pull, fetch, create a remote branch, open a PR, or otherwise modify any remote repository.
- Do not reset, discard, or overwrite unrelated dirty work. Preserve anything outside this mission.
- Preserve blueprint GUIDs, save compatibility, item mechanics, category membership, vendor placement, and all accepted prior behavior.

## Supplied visual references

Read all full-resolution screenshots and the focused contact sheet:

- `references/originals/01_cord_current_equipped.png`
- `references/originals/02_native_belt_reference_ornate.png`
- `references/originals/03_native_belt_reference_buckle.png`
- `references/originals/04_firearm_category_blunderbuss_reference.png`
- `references/originals/05_firearm_category_musket_pistol_reference.png`
- `references/crops/`
- `references/CONTACT_SHEET.png`
- `VISUAL_NOTES.md`

These screenshots are the authoritative post-overhaul **before state**. Inspect relevant locally available vanilla textures and the current mod source art to understand dimensions, margins, alpha, palette, line weight, and scaling. Do not redistribute extracted vanilla assets.

## Scope boundary

This revision has exactly two visual targets:

1. the shared firearm weapon-category/parameter icons for **Blunderbuss, Musket, and Pistol**;
2. the inventory/equipment icon for **Cord of Stubborn Resolve**.

Everything else is a regression-sensitive non-goal. In particular, do **not** redesign or casually regenerate:

- the Rapid Reload feat icon;
- any firearm inventory weapon icon;
- any eastern-weapon or Elven Branched Spear inventory icon;
- class, grit, deed, spell, feat, ammunition, repair, or vendor icons;
- Rifle/Revolver support or removal logic;
- item statistics, blueprints, mechanics, localization, or acquisition paths.

Before editing, record hashes of the accepted/locked art assets above. Recheck those hashes at the end so the report can prove that this narrow polish did not regress them.

# Phase 1 — Audit the current asset pipeline

Locate and document:

- the final PNG/texture assets and any SVG, layered, procedural, or generation sources for the Blunderbuss, Musket, and Pistol category icons;
- every mapping or blueprint reference that causes those category icons to appear in Weapon Focus, Rapid Reload, Improved Critical, Weapon Specialization, Greater Weapon Focus, or any other parameterized weapon-category UI;
- the final texture and source art for Cord of Stubborn Resolve;
- the repository's expected icon dimensions, format, alpha treatment, filtering, compression, and asset-loading path;
- whether the visible firearm border is baked into the PNG, introduced by a source template, or added by code/import settings.

Use one shared corrected icon per firearm category wherever the architecture already expects shared category artwork. Do not patch only the two screens shown in the screenshots.

Add a concise audit and before-state asset manifest to `docs/reports/icon-polish-round-2-report.md`.

# Phase 2 — Correct the firearm category icons

Revise only the category/parameter-selection artwork for:

- **Blunderbuss** — decorative `B`;
- **Musket** — decorative `M`;
- **Pistol** — decorative `P`.

The current direction is close, but the rendered initials are far too large and the artwork contains a second inset rectangular border that neighboring native category icons do not have.

## Required visual corrections

- **Remove the extra internal border/frame baked into the firearm artwork.** The normal game-provided UI cell/row framing must remain. The corrected firearm tile should have the same single-layer presentation as adjacent vanilla weapon-category icons.
- Preserve the existing subdued Kingmaker-compatible square background/gradient unless a small cleanup is required to eliminate the inset-frame artifact. The background should reach the same effective edges as ordinary category tiles, with no pale inset card, dark moat, double outline, or extra corner frame.
- Reduce the `B`, `M`, and `P` substantially. At actual in-game scale, their apparent cap height, stroke weight, ornamentation, and surrounding negative space should be comparable to neighboring native single-glyph icons such as **Battle Axe, Bite, Long Bow, Longspear, and Longsword**.
- The current firearm letters visually dominate the tile. The corrected letters must sit comfortably inside it and must not be the first thing that appears oversized when the list is scanned.
- Center each glyph optically, not merely mathematically. Account for the asymmetric shapes of `B`, `M`, and `P`.
- Preserve a decorative Kingmaker-compatible letterform and clear small-size readability. Do not replace the icons with plain modern block typography.
- Keep antialiasing, edge softness, highlight/shadow, and color treatment consistent with the native icons and the already accepted Nodachi/custom-category work.
- Work from the cleanest source available. Do not repeatedly shrink and resave a low-resolution flattened asset until it becomes muddy.
- Do not add icons for Rifle or Revolver, and do not reintroduce either category to any selection.

## Render-scale validation

Create deterministic previews at the exact native texture size and at the approximate on-screen size shown by the game. Place the corrected firearm icons beside locally sourced comparison renders or clearly identified screenshot crops of:

- Blunderbuss beside Battle Axe and Bite;
- Musket beside Long Bow/Longsword;
- Pistol beside Longsword/Nodachi/Punching Dagger.

A source PNG looking acceptable at 400% zoom is not enough. Iterate based on the actual small rendered size.

# Phase 3 — Redesign the Cord of Stubborn Resolve icon as a belt-slot item

The current Cord icon is a braided red loop seen almost perfectly from above. Its near-circular silhouette makes it read as a ring, necklace, or circlet rather than a belt-slot item.

Create a new, original icon that preserves the item's identity as a **red/dark braided cord** while using the same spatial grammar as Kingmaker belt icons.

## Required form and perspective

- Present the Cord from a shallow front/three-quarter angle similar to the two supplied native belt references.
- Make the visible object **horizontally dominant rather than circular**. Its nontransparent silhouette should be clearly wider than tall and should occupy the canvas similarly to ordinary belt icons.
- Establish depth with a visible front segment and a receding rear segment, appropriate foreshortening, lighting, and shadow.
- Add an intentional knot, overlap, tie, clasp, short hanging ends, or comparable cord-specific construction so the shape does not read as an unbroken jewelry ring. Preserve the concept of a cord; do not turn it into a generic thick leather belt.
- A slight diagonal cant is acceptable, but the overall read must remain “belt worn around the waist,” not “ring lying flat on a table.”
- Retain the red/brown braided-rope character and magical-item polish where practical.
- Match native belt-icon canvas use, contrast, alpha edges, soft shadow, and visual scale.
- Use a transparent background with no baked inventory-cell frame or opaque rectangle.
- Verify the result in both the equipped belt slot and the shared-stash/inventory grid at actual size.

If clean source art exists, edit it. If only the flattened circular PNG exists, reconstruct the cord cleanly—preferably with an original vector/layered/procedural source—rather than merely applying a perspective squash to the circle. A squashed ring without a front face, overlap, or knot will still fail the requirement.

Do not change Cord of Stubborn Resolve's blueprint GUID, slot, effects, price, localization, acquisition path, or save behavior.

# Phase 4 — Reproducibility and regression protection

- Preserve or add clean source art under the repository's established art-source location. If none exists, use an appropriate development-only path such as `art-src/` or `tools/icon-art/`.
- Keep generation deterministic where practical, and document exact generation/export steps.
- Do not add a runtime library for static image generation.
- Validate dimensions, RGBA/alpha, transparent corners, referenced paths, and successful texture loading.
- Update any existing icon-asset validation so it catches missing files, wrong dimensions, accidental opaque backgrounds, and references to stale pre-polish assets.
- Do not rely on a simplistic pixel test as proof of visual quality; human-scale screenshot comparison remains required.
- Confirm that the accepted Rapid Reload feat icon and all accepted weapon item icons retain their pre-mission hashes.
- Run the existing tests that assert Rifle and Revolver remain absent from ordinary supported selections. Do not modify those expectations.

# Phase 5 — Build, package, install, and visually verify

Run the repository's real build/test/package workflow and repair failures caused by this mission.

Use the existing local Kingmaker/UMM installation workflow to install the result and capture full-resolution **after** screenshots showing, at minimum:

1. Weapon Focus with **Battle Axe, Bite, and Blunderbuss** visible together;
2. Weapon Focus with **Long Bow/Longsword, Musket, Nodachi, and Pistol** visible together;
3. Rapid Reload or another independent parameterized selection proving the same corrected shared firearm icons are reused and Rifle/Revolver remain absent;
4. Cord of Stubborn Resolve equipped in the belt slot at the same scale as `01_cord_current_equipped.png`;
5. Cord of Stubborn Resolve in the shared stash or inventory grid, preferably near ordinary belt items;
6. at least one ordinary native belt equipped for a direct perspective/scale comparison.

Create a labeled before/after contact sheet. Inspect the screenshots at 100% and at a zoom that reproduces the in-game tile size. Do not claim success based only on compilation or enlarged source art.

If the game genuinely cannot be launched in this environment, complete all code/art/static validation, produce exact-size deterministic previews, package the mod, and state plainly which in-game views remain for manual verification. Do not invent successful visual testing.

After the work passes, increment the mod using the repository's established **next patch-version** process. Do not overwrite the prior tested package. Package and install the new version.

# Acceptance criteria

The mission is complete only when all of the following are true:

- Blunderbuss, Musket, and Pistol retain coherent decorative category initials, but their visual size and weight now match neighboring native weapon-category glyphs.
- No firearm category tile has the extra baked/inset rectangular border visible in the supplied screenshots.
- The corrected category icons appear consistently in every relevant parameterized weapon UI, not only Weapon Focus.
- Rifle and Revolver remain absent from ordinary supported selections and are not reintroduced anywhere.
- Cord of Stubborn Resolve is visibly wider than tall, shown from a belt-like oblique/front perspective, and no longer reads as a ring, necklace, or top-down circle.
- The Cord still reads specifically as a braided cord rather than a generic leather belt.
- The Cord icon fits native belt-slot scale, lighting, transparency, and canvas usage in both equipment and inventory views.
- The accepted Rapid Reload feat icon, firearm item icons, eastern-weapon icons, and Elven Branched Spear icons are unchanged except for demonstrably unavoidable metadata regeneration, which must be explained.
- The mod builds, tests, packages, installs, and launches without new errors.
- Full-resolution after screenshots, deterministic previews, a changed-asset manifest, and a before/after contact sheet exist.
- The working tree is clean except for intentionally retained reports and packaged deliverables.

# Final deliverables

Commit the completed work locally with clear, scoped commit messages. Do not push.

Return a final report containing:

- starting tested baseline branch/commit/version and ending branch/commit/version;
- the exact source and final asset paths for B, M, P, and Cord;
- an explanation of what caused the extra firearm border and how it was removed;
- before/after visible-bound or scale measurements for the firearm glyphs at native texture size;
- before/after silhouette/aspect and perspective notes for the Cord;
- proof that locked accepted assets retained their hashes;
- all build, test, validation, package, install, and launch commands with results;
- package path, version, SHA-256, DLL SHA-256, and MVID if the established workflow records it;
- paths to full-resolution after screenshots, deterministic exact-size previews, and the before/after contact sheet;
- `git status --short`, final local commit hashes, and confirmation that no remote operation occurred;
- any remaining limitation stated plainly.

Use the supplied screenshots and your technical judgment. Do not stop to ask me to choose a letter scale percentage, border thickness, belt knot design, or exact shade. Make a conservative native-looking pass, inspect it at actual game size, and iterate until the two remaining icon families fit Kingmaker's visual language without regressing the completed overhaul.
