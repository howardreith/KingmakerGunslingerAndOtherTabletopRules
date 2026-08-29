# Supported firearm item-icon source

These three transparent source PNGs are original project-owned bitmap artwork
created for this overhaul with OpenAI image generation on 2026-08-29. They do
not copy screenshots or native game textures. The accepted sources are kept at
their original 1254 x 1254 resolution and are treated as primary art; image
generation itself is not claimed to be byte-deterministic.

All three generation briefs shared this art direction:

> A single Pathfinder: Kingmaker-style late-2010s isometric fantasy CRPG
> inventory weapon icon, isolated on a fully transparent canvas. Painterly,
> hand-finished material rendering with warm wood, dark iron, brass highlights,
> clean antialiased alpha edges, and a restrained soft shadow. Orient the weapon
> lower-left to upper-right at roughly 35 degrees and fill the square naturally
> with a small safety margin. No frame, card, panel, border, text, letters,
> badges, hands, ammunition, scenery, or opaque background.

The per-file subject clauses were:

- `early-pistol-source.png`: one compact early flintlock pistol with a curved
  walnut grip, short dark-steel barrel, visible lock, trigger guard, and modest
  brass fittings; preserve true pistol proportions.
- `musket-source.png`: one long flintlock musket with a slender walnut full
  stock, long straight steel barrel, visible lock and ramrod, and restrained
  brass fittings; unmistakably a musket rather than a rifle or crossbow.
- `blunderbuss-source.png`: one short flintlock blunderbuss with a strongly
  flared brass muzzle, stout walnut stock, visible lock and trigger guard;
  unmistakably a blunderbuss.

Accepted source SHA-256 values:

- early pistol: `C6A76485178CDB1A7B37291B8169E034C78DF4B0D551DA70B18D428D30ABDE6B`
- musket: `624582C0F7A63A097F85F289EDBD9AA4933264D70F4F91148B2222878F4A94E6`
- blunderbuss: `773CBF0C27329C520EACEDC7F6E85645493EE7A85E48436B6CFA0E1B190582E7`

`tools/icon-art/New-IconOverhaulAssets.ps1 -Mode Items` measures each alpha
bound, rejects edge-touching sources, fits it once to a 128 px transparent
runtime canvas with a 5 px safety margin, and writes the deterministic final
PNG plus manifest hashes. No runtime library is required.
