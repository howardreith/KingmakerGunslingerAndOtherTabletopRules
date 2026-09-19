# Kingmaker Gunslinger 0.0.131 — Scroll Item Icons (full release)

Owner-authorized full public release. UMM version `0.0.131`, informational
version `0.0.131-icon-art-overhaul`. Everything from 0.0.130 is preserved
unchanged except the three strategic scroll item icons.

## What is in this release

- **Composed scroll item icons** for Scroll of Teleport, Scroll of Greater
  Teleport and Scroll of Word Of Recall. Each item now follows the native
  Kingmaker scroll convention: the measured parchment scroll treatment
  (sheet, wider rod ends, continuous dark rim) with the owner-approved spell
  painting integrated inside the sheet. Previously these items rendered the
  bare spell painting. The spell ability icons are unchanged, and no other
  scroll, spell, item or feat icon was touched.
- **Exact catalog authority**: the three composed icons are dedicated
  `scroll-of-*` concepts with production-manifest source/export records,
  briefs and preserved approved inner symbols; the guarded runtime census
  verified all 137 painted assignments and every protected/native-reuse
  binding on the released artifact, with the three scroll items rendering
  their composed `KMG_Icon_scroll-of-*` sprites.
- **Guarded runtime qualification**: two consecutive unattended fresh-launch
  PASS runs of the teleportation spellbook UI scenario verified the composed
  icons on real native inventory rows, item descriptions and merchant rows,
  with native control rows preserved and zero save writes. A native spellbook
  screenshot guard that raced the asynchronous page rebuild after `Rest()`
  was replaced with a bounded settle-wait, keeping the guard terms as
  structured evidence.
- **Native scroll donor references** were measured through the guarded
  `icon-overhaul-visual-evidence` scenario (exact 64x64 live-sprite cells);
  reference pixels are local-only and were never incorporated into project
  art.

## Qualification evidence

- Repository validation, icon catalog validator, 1657/1657 domain tests,
  clean Release build and strict 227-file package validation all PASS.
- `reports/icon-overhaul/SCROLL-ITEM-ICON-QUALIFICATION.json` records the
  composition provenance, both consecutive PASS runs, the live census and
  the failure investigation for the pre-scroll spellbook-phase race.

## Compatibility

Install over any previous 0.0.12x/0.0.130 version with Unity Mod Manager. The
retained compatibility profiles and feature-module settings are unchanged; no
other mod is required or modified.

- Optional-mod compatibility artifacts are unchanged: the Craft Magic Items
  integration (`CraftMagicItems.dll` references) and every retained
  compatibility profile carry over from 0.0.130 without modification.

- Historical suite checkpoints of 1,251, 1,288 and 1,325 cases remain
  attributed to their original releases; this release ships the complete
  1,657-case dependency-free domain suite (all passing), which includes every
  inherited checkpoint.

## Verification

- Installable archive: `KingmakerGunslinger-0.0.131-icon-art-overhaul.zip`.
- Firearm SoundBank unchanged from the qualified audio release; bank SHA-256:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Runtime smoke testing of this release was NOT RUN beyond the guarded
  qualification scenarios; the owner installs and visually reviews on their
  main computer.
