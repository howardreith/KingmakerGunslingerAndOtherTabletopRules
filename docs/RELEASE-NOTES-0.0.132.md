# Kingmaker Gunslinger 0.0.132 — Native Scroll Shell Icons (full release)

Owner-authorized full public release. UMM version `0.0.132`, informational
version `0.0.132-icon-art-overhaul`. Everything from 0.0.131 is preserved
unchanged except the three strategic scroll item icons.

## What is in this release

- **Corrected scroll item icons** for Scroll of Teleport, Scroll of Greater
  Teleport and Scroll of Word Of Recall. These are asset composites on the
  exact native Kingmaker scroll shell: the shell was reconstructed
  symbol-free from five same-design native scroll donor sprites
  (cross-donor median, ring fill for the symbol-covered core, cool-pixel
  ghost enforcement, background-to-alpha unmixing), so the parchment body,
  roller/cap design, border and shading, proportions and silhouette are the
  native shell's own pixels. Only the inner emblem differs — the
  owner-approved spell painting composited into the measured native symbol
  window. The 0.0.131 procedural parchment is superseded; the spell ability
  icons and every other icon are unchanged.
- **Kingmaker has no procedural scroll-icon generation**: native scroll
  items are individually authored 64x64 atlas sprites bound as static
  textures, so the corrected icons are offline texture composites assigned
  through the ordinary project icon cache.
- **Guarded runtime qualification**: repository validation, the icon
  catalog validator, 1657/1657 domain tests, a clean Release build and
  strict 227-file package validation all PASS, and the guarded
  teleportation spellbook UI scenario passed 42/42 assertions on the
  corrected build, including every scroll inventory row, item description
  and merchant row with native controls preserved and zero save writes.
  Evidence:
  `reports/icon-overhaul/SCROLL-ITEM-ICON-QUALIFICATION.json`.

## Compatibility

Install over any previous 0.0.12x/0.0.131 version with Unity Mod Manager.
The retained compatibility profiles and feature-module settings are
unchanged; no other mod is required or modified.

- Optional-mod compatibility artifacts are unchanged: the Craft Magic Items
  integration (`CraftMagicItems.dll` references) and every retained
  compatibility profile carry over from 0.0.131 without modification.

- Historical suite checkpoints of 1,251, 1,288 and 1,325 cases remain
  attributed to their original releases; this release ships the complete
  1,657-case dependency-free domain suite (all passing), which includes
  every inherited checkpoint.

## Verification

- Installable archive: `KingmakerGunslinger-0.0.132-icon-art-overhaul.zip`.
- Firearm SoundBank unchanged from the qualified audio release; bank SHA-256:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Runtime smoke testing of this release was NOT RUN beyond the guarded
  qualification scenarios; the owner installs and visually reviews on their
  main computer.
