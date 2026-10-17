# Dire Rat summon icon pilot (VIS-004)

**Status: pilot, pending owner approval. The production icon is unchanged.**

The owner found the Dire Rat icon unreadable at menu size, with the rat lost
inside an unwanted circular medallion, and asked for a recognisable rat
consistent with the neighbouring icons. The icon guide
(`docs/ICON-ART-GUIDE.md`) keeps a new or edited summon painting out of
production until the owner approves it, preserves originals byte for byte,
and forbids silently substituting a procedural painting. No image-generation
tool was available to this session, so this pilot edits the original
painting rather than painting a new one.

- Original: `../../sources/dire-rat.png`
  (`41a52d00891f42214c84a07f121468aa9feb88eb006ad57b9e7933d860ccc871`,
  built-in imagegen, 2026-09-30; see `../../PHASE2-SPRINT12-SOURCES.md`),
  unchanged.
- Edit: `crop_dire_rat_pilot.py` crops a 920 px square at (170, 170) closer on
  the rat and fades the bronze ring and the black corners beyond it into a
  dark tone taken from the painting's own shadows; nothing is painted in. It
  reproduces `dire-rat-pilot.png` byte for byte (Blender 4.5.10).
- `dire-rat-pilot.png`: `df593dfad161941cb8846e1f7df1e723480975a3e90d5ffd789c9d170d04168d`
  (920 x 920 RGBA).
- `dire-rat-pilot-menu-sizes.png`: the current icon (first row), the pilot
  (second row) and two neighbours (Hyena, Wolverine) at 128, 64, 48 and 32 px.
  The pilot's head, ears, incisors and claws stay legible at 48 px, where the
  current icon's rat is small and dark inside its ring.

Trade-offs for the owner: the pilot has no ring while its painted neighbours
keep theirs, and the tip of the tail leaves the frame. If approved, the
128 px production export goes through `tools/New-ExpandedSummoningIcons.ps1`
from this source and the manifest records the new source and output hashes;
if not, a newly painted icon needs an image tool or an artist.
