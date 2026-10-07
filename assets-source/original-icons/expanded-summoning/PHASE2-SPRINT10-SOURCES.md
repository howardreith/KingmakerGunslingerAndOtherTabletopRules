# Sprint 10 summon choice paintings

These two square 1254 px paintings are project-owned source assets made with
the installed built-in `imagegen` tool on 2026-09-28. They are new concepts;
the reference paintings were supplied for family composition only. Neither
source incorporates Kingmaker art, a donor crop, or third-party pixels. Export
them through `tools/New-ExpandedSummoningIcons.ps1` when their choices are
published. Both species now have distinct manifest-backed 128 px exports and
publication candidates. Technical and
in-game review do not constitute owner visual approval.

| Concept | Source SHA-256 | Family / anatomy references | 128 px source downsample review |
|---|---|---|---|
| Giant Wasp | `7a0158fdeb3cc7aff3dbaa870fc16bda98a5437eabd7413cf163464043cba327` | Project Dire Bat icon for frame/light; original Giant Wasp mesh render for species anatomy | Stripe, four translucent wings, rear sting remain distinct. |
| Stirge | `089966cd5bf7c1da26c67d0206a50c1e0cd3c89867403fe64e2b1587b1a5d3d1` | Project Dire Bat and Giant Wasp paintings for frame/light only; Paizo Stirge description for anatomy | Rust-red body, fleshy wings, and forward proboscis remain distinct from Wasp. |

Both sources use `original-required` for the creature choice and
`intentional-family-share` for its quantity and template variants. The parent
summon spell retains its established identity. Internal poison, attach, and
disease facts are `hidden-internal`. The generated icon manifest records 26
Wasp unit, inspectable-type, ability and template consumers; Stirge has its
unit and nine Nature's Ally abilities. Stirge's touch carrier and attack trait
are mechanics-only, its hold is internal, and Filth Fever retains native art.

## Owner correction: Remove Stirge action, 2026-09-29

The new standard-action choice uses a distinct original painting. Source
`sources/remove-stirge.png` is a 1254 px opaque square generated with the
built-in imagegen tool, SHA-256
`f57ff1733c65ff1702507ec5c23fcbf3c9b93065d97ecbab07b2943ac855ad1b`.
Deterministic 128 px export `assets/game/icons/expanded-summoning/remove-stirge.png`
has SHA-256
`d94adebdc69e80f08bb936de2050ccbb5ad78e9fc99c678b6f242c5979c28329`.
The exact consumer is `KMG.Summoning.Special.Stirge.Remove`; the summon unit
and its nine registered but temporarily hidden SNA choices retain the separate
Stirge portrait. At 128 px the large gloved hand lifting the small rust-red
Stirge clear of a forearm remains legible. Technical export and catalog checks
do not constitute native UI qualification or owner visual approval.

Initial generation prompt (built-in imagegen, stylized-concept):

> Original square source painting for a 128-pixel Pathfinder: Kingmaker-style ability icon. Clearly depict Remove Stirge as a gloved adventurer's hand gripping a tiny rust-red bat-winged, mosquito-like Stirge and pulling it away from a forearm. A large hand and forearm diagonal across the center, creature visibly separated from skin; readable at 128 px. Rich dark fantasy painting, warm amber rim light, near-black woodland backdrop, thin ornate aged-gold circular frame. Emphasize the removal gesture, not a frontal flying creature. No text, numbers, selection glow, watermark, unrelated animals, or gore.

Selected edit prompt:

> The Stirge has already been removed. Lift the tiny bat-winged creature upward so the entire proboscis tip is visibly clear of the forearm by a dark gap. Gloved fingers grip its small torso from above. The forearm is unpierced, with no contact or blood. Preserve the gold frame, amber painterly lighting, dark forest, square format and scale; keep the action readable at 128 px.

## Exact generation prompts

Giant Wasp:

> Use case: stylized-concept. Asset type: square 128-pixel Pathfinder: Kingmaker summon-choice icon, authored as a high-resolution source painting for later downsampling. Image 1 is the existing project-owned Dire Bat icon: use it only as a reference for the painted creature portrait family, dark atmospheric background, warm rim lighting, and narrow ornate gold circular frame; do not copy bat anatomy or pixels. Image 2 is the project's original Giant Wasp 3D source: use it only for species anatomy and the black-and-gold segmented body, four membranous wings, six legs, and long rear stinger. Create an entirely new Giant Wasp painting: one horse-sized wasp flying diagonally toward the viewer inside that narrow gold circular composition, head and large compound eyes unmistakably insect, two distinct pairs of translucent wings, striped abdomen and long sharp rear stinger clearly legible as separate silhouettes. Keep the full creature readable when reduced to 128x128; strong central shape and separated wings, dark woodland/cavern backdrop with warm amber light. Rich painterly texture and believable insect materials, not a flat logo or screenshot. No text, lettering, counter, UI selection highlight, human face, extra wings, extra limbs, weapon, watermark, or unrelated animal. Preserve no bat features.

Stirge:

> Use case: stylized-concept. Asset type: square high-resolution source painting for a Pathfinder: Kingmaker summon-choice icon, later reduced to 128x128. Images 1 and 2 are project-owned icon family style/composition references only: use the narrow ornate gold circular frame, strong central creature silhouette, dark atmospheric painterly backdrop, dramatic warm rim light; create a wholly original creature painting with no copied bat or wasp pixels. Subject is a Pathfinder 1e Stirge, according to Paizo: a tiny rust-red/reddish-brown mammal-like blood-drinking insectoid one foot long, dirty yellow belly, two PAIRS of fleshy bat wings (four wings total), a tangle of thin barbed legs, and one very long needle-sharp forward proboscis. Show it flying diagonally toward the viewer, with its proboscis clearly separated from the legs and pointing toward the lower right, four fleshy wings readable at 128px, small body distinct from the Giant Wasp's striped abdomen and translucent insect wings. Moody swamp reeds/cavern atmosphere, rusty crimson and dusky gold palette, believable textured membrane and fur. Keep creature fully visible inside the circular composition. No stripes, compound wasp eyes, antennae, beak, fangs, extra wings, extra legs, text, counter, UI selection effect, or watermark.
