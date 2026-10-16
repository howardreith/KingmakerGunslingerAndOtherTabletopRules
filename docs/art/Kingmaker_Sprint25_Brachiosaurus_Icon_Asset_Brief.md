# Asset handoff: original Brachiosaurus summon icon (Sprint 25)

One source painting and its derived 128x128 runtime icon. This is a painting
request for an **image-capable authoring session**. It authorizes no game
launch, no repository write outside the two paths below, no policy change, no
purchase and no API billing.

## Why this is a handoff

`docs/ICON-ART-GUIDE.md` requires a built-in image tool or an authorized
artist for the painted summon family and forbids silently substituting a
procedural painting. The session that prepared Sprint 25 checked for an
installed image skill and found none: `~/.claude/skills` does not exist and
no image-related skill is present under the plugin marketplaces. So the icon
is handed off rather than faked.

This is **only** the icon. It is not either creature's UV albedo. Both Sprint
25 UV albedos are already authored in-repo by the established deterministic
Python texture pipeline (`assets-source/original-models/sprint25-giant-bodies/paint_giant_body.py`),
and no Elephant icon replacement is requested — Elephant already has an
approved icon.

## Correction to the original request text, from inspecting the real family

The drafted prompt asked for "a simple dark atmospheric background" and "no
decorative UI frame". **The approved family does not look like that**, and
following that text would produce an off-family icon that the catalog review
should reject. Three released references were opened and inspected directly —
Triceratops, Stegosaurus and Tyrannosaurus — and all three share:

- a **circular medallion** composition inside the square, enclosed by an
  ornate ring with regular notches or rivets around the rim;
- a warm **sepia / amber / golden-brown** cast, aged bas-relief in feel, even
  where there is local colour (the Tyrannosaurus has an orange sunset sky and
  a grey-green body but stays warm overall);
- the creature in **three-quarter side view**, filling most of the roundel and
  standing on a visible ground line;
- a lit **atmospheric landscape and sky inside the roundel**, not a flat dark
  field — the creature reads against a bright horizon.

Please match that observed family. If the owner intends a deliberate change of
look for new icons, that is an explicit art-method decision and should be
recorded before generation, not settled inside this brief.

## Reference images to open before generating

Open the pixels, not the filenames. All paths are repository-relative.

| Role | Production export (128x128) | Source painting (1254x1254) |
| --- | --- | --- |
| Closest large quadruped dinosaur | `assets/game/icons/expanded-summoning/triceratops.png` | `assets-source/original-icons/expanded-summoning/sources/triceratops.png` |
| Second quadruped dinosaur | `assets/game/icons/expanded-summoning/stegosaurus.png` | `…/sources/stegosaurus.png` |
| Most recent dinosaur addition | `assets/game/icons/expanded-summoning/tyrannosaurus.png` | `…/sources/tyrannosaurus.png` |
| Large grey mammal, for scale and hide | `assets/game/icons/expanded-summoning/elephant.png` | `…/sources/elephant.png` |
| Its tusked relative | `assets/game/icons/expanded-summoning/mastodon.png` | `…/sources/mastodon.png` |

These are **style references only**. Do not copy source pixels, trace them, or
edit any of them. All 119 existing source paintings and their assignments must
remain untouched, byte for byte.

Verify each against `assets-source/original-icons/expanded-summoning/icon-manifest.json`
before use; recorded source hashes begin `ea9cf409…` (triceratops),
`da97b116…` (stegosaurus), `50312016…` (tyrannosaurus), `5c9bfa27…`
(elephant) and `b4a1d9dc…` (mastodon).

If references cannot be supplied in the image-capable session, request them
rather than generating unreferenced, and label any unreferenced output clearly
as an unqualified concept.

## Generation prompt

> Create a single original fantasy-game creature icon source painting of an
> adult Brachiosaurus, matching the attached painted summon-choice references.
> Square canvas with a circular medallion composition: the creature sits
> inside an ornate ring with regular notches around the rim, in the same aged
> bas-relief style as the references. Warm sepia and amber palette with a lit
> sky and simple landscape behind the creature inside the roundel, so the
> silhouette reads against a bright horizon.
>
> One unmistakable long-necked sauropod in a readable three-quarter side view,
> standing on a ground line and filling most of the roundel: the elevated long
> neck and small head clearly separated from the torso, tall shoulders higher
> than the hips, a heavy quadruped body, thick columnar legs, and a balancing
> tapering tail. Calm but imposing; natural muted hide with painterly light
> and material detail.
>
> Keep the small head, the neck curve, the forelegs and the tail readable when
> the image is reduced to 128 pixels. No lettering, numbers, counters, badges,
> selection glow, alignment overlays or quantity marks. No tusks, horns,
> dorsal plates, wings, bipedal stance, predatory skull, armor, saddle or
> rider; no second animal; no scenery that competes with the creature. This is
> a painted creature identity, not a diagram, a flat vector emblem, a 3D
> viewport screenshot or a photoreal zoo photograph.

Output at the installed tool's supported size, preferably at least 1024px
square. The released family's sources are **1254x1254**, so match that if the
tool allows it. The repository exporter, not the image generator, produces the
final 128x128 file.

## Deliverable and integration contract

Source painting, after reviewed import:

```text
assets-source/original-icons/expanded-summoning/sources/brachiosaurus.png
```

Runtime export, produced by the existing exporter `tools/New-ExpandedSummoningIcons.ps1`:

```text
assets/game/icons/expanded-summoning/brachiosaurus.png
```

Record the actual generation tool, the exact prompt used, the role each input
reference played, the original dimensions and the SHA-256 of the source bytes.
Do not claim the file was painted by hand or produced by a tool that was not
used. Preserve the individually generated source bytes; a contact sheet is not
a source archive.

The integration agent then, and only once the real files exist:

1. runs the existing exporter so the 128x128 RGBA output is produced by the
   repository toolchain rather than the image tool — note the exporter already
   refuses a low-variance or low-colour image, so a flat picture will not pass;
2. adds the manifest row with `sourceSha256`, `outputSha256`, `width` 128,
   `height` 128, `format` `RGBA PNG`, `scope` `kmg-catalog` and the exact
   consumer symbols `KMG.Summoning.Unit.Brachiosaurus`,
   `KMG.Summoning.Natural.Brachiosaurus.UnitType`,
   `KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One`,
   `KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3` and
   `KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne` — five
   consumers and no template children, because this creature has no Summon
   Monster row;
3. moves the icon-catalog count pin from 119 to 120 and the manifest `count`
   with it;
4. inspects the new icon at 32, 48, 64 and 128 pixels and in its real UI
   presentation before approval;
5. leaves `HumanReview: NOT_PERFORMED_NONBLOCKING` unless the owner actually
   approves the image. A technical PASS is not approval.

Current placement is the recorded owner-selected Nature's Ally VII-only scope:
the same Brachiosaurus identity serves parents VII, VIII and IX. Do not bake
quantity or spell tier into the pixels.
