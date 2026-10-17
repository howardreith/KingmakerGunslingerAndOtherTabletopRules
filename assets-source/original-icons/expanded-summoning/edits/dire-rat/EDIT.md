# Dire Rat summon icon edit (VIS-004)

**Status: approved by the owner on 2026-10-17 ("The new dire rat icon looks
great. Go ahead with it.") and adopted as the production source.**

The owner found the Dire Rat icon unreadable at menu size, with the rat lost
inside an unwanted circular medallion, and asked for a recognisable rat
consistent with the neighbouring icons. The icon guide
(`docs/ICON-ART-GUIDE.md`) keeps an edited summon painting out of production
until the owner approves it, preserves originals byte for byte, records edits
separately, and forbids silently substituting a procedural painting. No
image-generation tool was available to this session, so this edits the
original painting rather than painting a new one. It was reviewed as a pilot
(`8ff9893b2f`, then under `pilots/dire-rat/`) and moved here unchanged on
approval.

- Input, kept byte for byte: `../../sources/dire-rat.png`
  (`41a52d00891f42214c84a07f121468aa9feb88eb006ad57b9e7933d860ccc871`,
  built-in imagegen, 2026-09-30; see `../../PHASE2-SPRINT12-SOURCES.md`).
- Edit: `crop_dire_rat.py` crops a 920 px square at (170, 170) closer on the
  rat and fades the bronze ring and the black corners beyond it into a dark
  tone taken from the painting's own shadows; nothing is painted in. It
  reproduces `dire-rat.png` byte for byte (Blender 4.5.10).
- Production source `dire-rat.png`:
  `df593dfad161941cb8846e1f7df1e723480975a3e90d5ffd789c9d170d04168d`
  (920 x 920 RGBA), the exact approved pilot bytes.
- Production export: `tools/New-ExpandedSummoningIcons.ps1 -OnlyKeys dire-rat`
  reads this edit in place of the original (its `$editedSources` entry) and
  writes the 128 px `assets/game/icons/expanded-summoning/dire-rat.png`; the
  provenance manifest records the edit as `sourceFile` and the original as
  `originalSourceFile`, each with its hash.
- `dire-rat-menu-sizes.png`: the review sheet the owner approved: the former
  icon (first row), this edit (second row) and two neighbours (Hyena,
  Wolverine) at 128, 64, 48 and 32 px.

Trade-offs accepted with the approval: the edit has no ring while its painted
neighbours keep theirs, and the tip of the tail leaves the frame.
