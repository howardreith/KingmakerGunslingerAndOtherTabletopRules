# Sprint 10 summon choice paintings

These two square 1254 px paintings are project-owned source assets made with
the installed built-in `imagegen` tool on 2026-09-28. They are new concepts;
the reference paintings were supplied for family composition only. Neither
source incorporates Kingmaker art, a donor crop, or third-party pixels. Export
them through `tools/New-ExpandedSummoningIcons.ps1` when their choices are
published. Technical and in-game review do not constitute owner visual approval.

| Concept | Source SHA-256 | Family / anatomy references | 128 px source downsample review |
|---|---|---|---|
| Giant Wasp | `7a0158fdeb3cc7aff3dbaa870fc16bda98a5437eabd7413cf163464043cba327` | Project Dire Bat icon for frame/light; original Giant Wasp mesh render for species anatomy | Stripe, four translucent wings, rear sting remain distinct. |
| Stirge | `089966cd5bf7c1da26c67d0206a50c1e0cd3c89867403fe64e2b1587b1a5d3d1` | Project Dire Bat and Giant Wasp paintings for frame/light only; Paizo Stirge description for anatomy | Rust-red body, fleshy wings, and forward proboscis remain distinct from Wasp. |

Both sources use `original-required` for the creature choice and
`intentional-family-share` for its quantity and template variants. The parent
summon spell retains its established identity. Internal poison, attach, and
disease facts are `hidden-internal`. The actual variant consumer symbols will
be recorded by the generated icon manifest at publication.

## Exact generation prompts

Giant Wasp:

> Use case: stylized-concept. Asset type: square 128-pixel Pathfinder: Kingmaker summon-choice icon, authored as a high-resolution source painting for later downsampling. Image 1 is the existing project-owned Dire Bat icon: use it only as a reference for the painted creature portrait family, dark atmospheric background, warm rim lighting, and narrow ornate gold circular frame; do not copy bat anatomy or pixels. Image 2 is the project's original Giant Wasp 3D source: use it only for species anatomy and the black-and-gold segmented body, four membranous wings, six legs, and long rear stinger. Create an entirely new Giant Wasp painting: one horse-sized wasp flying diagonally toward the viewer inside that narrow gold circular composition, head and large compound eyes unmistakably insect, two distinct pairs of translucent wings, striped abdomen and long sharp rear stinger clearly legible as separate silhouettes. Keep the full creature readable when reduced to 128x128; strong central shape and separated wings, dark woodland/cavern backdrop with warm amber light. Rich painterly texture and believable insect materials, not a flat logo or screenshot. No text, lettering, counter, UI selection highlight, human face, extra wings, extra limbs, weapon, watermark, or unrelated animal. Preserve no bat features.

Stirge:

> Use case: stylized-concept. Asset type: square high-resolution source painting for a Pathfinder: Kingmaker summon-choice icon, later reduced to 128x128. Images 1 and 2 are project-owned icon family style/composition references only: use the narrow ornate gold circular frame, strong central creature silhouette, dark atmospheric painterly backdrop, dramatic warm rim light; create a wholly original creature painting with no copied bat or wasp pixels. Subject is a Pathfinder 1e Stirge, according to Paizo: a tiny rust-red/reddish-brown mammal-like blood-drinking insectoid one foot long, dirty yellow belly, two PAIRS of fleshy bat wings (four wings total), a tangle of thin barbed legs, and one very long needle-sharp forward proboscis. Show it flying diagonally toward the viewer, with its proboscis clearly separated from the legs and pointing toward the lower right, four fleshy wings readable at 128px, small body distinct from the Giant Wasp's striped abdomen and translucent insect wings. Moody swamp reeds/cavern atmosphere, rusty crimson and dusky gold palette, believable textured membrane and fur. Keep creature fully visible inside the circular composition. No stripes, compound wasp eyes, antennae, beak, fangs, extra wings, extra legs, text, counter, UI selection effect, or watermark.
