# Visual Notes — Icon Polish Round 2

## What is already accepted and must remain unchanged

The latest installed/tested overhaul is a significant improvement. Treat these as locked successes:

- all firearm inventory/item weapon icons;
- all eastern-weapon inventory/item icons;
- all Elven Branched Spear inventory/item icons;
- the Rapid Reload feat icon;
- retirement of Rifle and Revolver from ordinary supported gameplay.

This is not a second general icon overhaul.

## Firearm category tiles

The screenshots show weapon-category icons as they appear inside Weapon Focus and related parameterized selectors.

### Visible defect 1: glyph scale

- Blunderbuss's `B` is dramatically larger than the nearby native `B` glyphs for Battle Axe and Bite.
- Musket's `M` and Pistol's `P` likewise occupy too much of their tiles compared with Long Bow, Longspear, Longsword, Nodachi, and Punching Dagger.
- The corrected glyphs should have comparable visual mass, line weight, and empty margin at the real on-screen size. They should not merely be technically inside the square.

### Visible defect 2: extra internal frame

- Each firearm tile contains an additional narrow gold/dark rectangle inset from the tile edge.
- Neighboring native category icons have the colored/gradient tile and glyph, but not that second internal frame.
- The game's ordinary list/cell framing is not the problem. Remove only the frame baked into or generated as part of the firearm artwork.

## Cord of Stubborn Resolve

- The current icon is a nearly perfect circular braided red loop shown top-down.
- In an equipment slot it reads like a ring, necklace, or circlet.
- Native belt icons are horizontally dominant and shown from a shallow oblique/front angle. Their front segment, depth, overlap/buckle, and shadow communicate “belt.”
- The revised item must remain a braided cord, but should adopt that belt-like silhouette and perspective. A knot, overlap, tied ends, or small clasp is encouraged because it prevents the icon from reading as an unbroken ring.
- Do not simply squash the current circle into an ellipse. It needs depth and a belt-specific construction.

## Reference file map

- `01_cord_current_equipped.png`: current rejected circular Cord icon.
- `02_native_belt_reference_ornate.png`: native belt with a shallow oblique band and visible front face.
- `03_native_belt_reference_buckle.png`: native belt with overlap/buckle and strong depth cues.
- `04_firearm_category_blunderbuss_reference.png`: Blunderbuss beside Battle Axe and Bite; best scale comparison for `B`.
- `05_firearm_category_musket_pistol_reference.png`: Musket and Pistol beside native/custom category icons; best scale and border comparison.
- `references/crops/`: enlarged focused excerpts.
- `references/CONTACT_SHEET.png`: labeled overview.
