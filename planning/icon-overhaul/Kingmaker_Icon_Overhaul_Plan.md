# Kingmaker Icon Overhaul — Art Direction, Inventory, and Delivery Plan

**Prepared for:** Howie Reith\
**Repository:** `howardreith/KingmakerGunslingerAndOtherTabletopRules`\
**Source baseline inspected:** `258decb8fc2dad58d7a096772b34ec4202d71bd5`\
**Status:** Planning document, not an execution mission. No artwork, game files, repository files, or saves have been changed.

## 1. Outcome and scope

Create a finished, coherent collection of original icons for the elemental-race content and strategic teleportation spells, repair firearm selector lettering and Rapid Reload's style, and protect the successful gunslinger artwork.

The acceptance target is not merely "an original PNG exists." The target is a recognizable, attractive icon in the actual Kingmaker screen where the player encounters it, with the correct visual relationship to neighboring icons.

The source audit confirms four races, twelve heritage choices, eleven racial feats, and twenty-one alternate racial traits. All twenty-one traits are currently published by the inspected policy, including Treacherous Earth and Nereid Fascination. Historical reports that describe nineteen selectable traits are not the current inventory.

This document indexes the named content and the principal supporting action graphs found in source. Before implementation, produce an exact blueprint-to-UI-consumer manifest from the executor's actual checkout and installed game. The named-content index is not a claim that every runtime icon consumer has already been exercised.

### Explicitly protected

- Existing gunslinger usable-action artwork, including Repair Firearm, Reload Firearm, Quick Clear, and the other accepted action/deed icons.
- Successful eastern-weapon lettering. Katana and Nodachi remain unchanged; Wakizashi receives at most an isolated clipping correction after live inspection.
- Weapon inventory illustrations and models, except an independently identified icon defect approved within the scope review.
- Native game icons for actual native abilities, native feat roots, and unchanged native racial features.
- Blueprint identities, saved feat parameters, racial mechanics, action costs, resources, spell rules, publication state, and compatibility behavior.

### Scope distinction

"Original art for new abilities" does not require unrelated art for a feature, its action, and its resulting buff when all represent the same player concept. Those consumers should normally share an identity. Different selectable actions need distinguishable sibling icons. Hidden resources, providers, and internal delivery helpers do not each need a separate painting.

## 2. Findings that change the implementation strategy

### Native lettering, not another font approximation

`CustomWeapons/CustomWeaponSelectorRuntime.cs` constructs eastern/custom category entries with a null sprite and a text monogram. The game supplies the visual lettering treatment.

`Feats/NativeFirearmFeatIntegration.cs` instead supplies the firearm parameter's explicit sprite. `Blueprints/ProjectAssetIcons.cs` assigns firearm-monogram PNGs to the firearm choices. The current generator, `tools/icon-art/New-IconOverhaulAssets.ps1`, draws P/M/B as custom Bézier paths. The two weapon families therefore do not use the same presentation mechanism.

**Recommended approach:** prototype the native text-monogram route for P/M/B in the firearm selector. Preserve the existing `FeatureParam(parameter)` blueprint-based identity, prerequisites, and bonuses. Do not convert firearm mechanics to eastern-weapon categories merely to fix lettering.

A live test must establish whether the same null-sprite path works for blueprint-parameter entries. Some surfaces may require an explicit sprite; handle those separately with a native-rendered or carefully reference-matched fallback. Do not assume one constructor argument fixes all contexts.

### Race icon reuse is structural

`ElementalHeritageBlueprintFactory` copies spell-like ability icons into heritage markers, affinities, and selector roots. `ElementalAlternateTraitBlueprintFactory` distributes a per-race sprite across unrelated choices and helpers. `ElementalFeatBlueprintFactory` uses racial spell icons for feat icons and initially supplies the Ifrit spell icon to multiple unrelated actions and buffs.

**Recommended approach:** explicit semantic assignments to exact owned consumers. Do not solve this by replacing one shared racial sprite, globally repainting a donor spell, or extending name-substring fallbacks.

### The three strategic spells share the same donor art

`TeleportationSpellBlueprints` uses the native Dimension Door icon for Teleport, Greater Teleport, and Word of Recall. These require three deliberate assignments, not a global Dimension Door replacement.

### Existing regeneration tooling can overwrite new art

`tools/New-FirearmFeatIcons.ps1` currently delegates to the broader icon-overhaul generator. Source assets, generated exports, loader keys, and regeneration rules must agree. An approved replacement must not silently revert the next time a developer runs an old generator.

## 3. Art direction: three families, not one universal style

### A. Painted magical and racial icons

Use the successful existing project paintings and appropriate native magical/racial icons as visual references. Favor a strong silhouette, one dominant subject, controlled light, painterly texture, and restrained supporting effects. Design for small display sizes from the beginning.

Elemental identity is a secondary organizing system:

- Ifrit: embers, flame, blackened bronze, molten stone, warm light.
- Oread: stone, crystal, forged metal, weight, mineral surfaces.
- Sylph: moving air, smoke ribbons, cloud, lightning, lightness.
- Undine: liquid forms, wave crests, fog, rime, cool reflected light.

Function must remain recognizable without color. A shield/deflection shape should not resemble a channeling/affinity shape simply because both belong to an Ifrit. Avoid making every icon a colored orb, elemental face, hand, or generic glowing sigil.

Magical racial feats may use this painted family. Do not force every feat with a combat classification into the mundane red-emblem style solely because of its mechanical tag.

### B. Native-style combat feat emblems

Rapid Reload should match the surrounding native combat-feat vocabulary: the appropriate thin circular structure, muted ink color, matching optical weight, parchment relationship, and restrained ornament.

Measure and compare actual references rather than guessing a ring from a verbal description. Reuse native presentation scaffolding where the UI supports it; otherwise author a precise original scaffold with an original central reload symbol. Do not embed a second frame where the UI already supplies one.

A ramrod/ammunition-loading gesture can remain the subject, but it must read as loading rather than repairing. Avoid the current heavy partial arc. A more elaborate painting is not the right remedy for this icon.

### C. Native ornamental weapon monograms

Prefer the native renderer for P/M/B. Match the existing game lettering, not an approximately medieval font. Keep category notation separate from inventory art and keep weapon-choice identity separate from the parent feat emblem.

Do not globally change font sizing to correct Wakizashi. Establish whether clipping belongs to the local monogram layout, text bounds, or its container, then make the narrowest correction.

## 4. Source-confirmed heritage and spell-like ability index

Every heritage below gets its own recognizable heritage identity. Existing native spell icons remain on the actual granted spell and equivalent spell-use entries, not on the ancestry marker or unrelated affinity.

| Race | Heritage | Proposed heritage motif | Granted spell-like ability | Ability-art decision |
|---|---|---|---|---|
| Ifrit | General Ifrit | A compact living flame/ember crest | Burning Hands | Keep native spell art |
| Ifrit | Lavasoul | Black volcanic stone split by molten channels | Firebelly | Keep native spell art |
| Ifrit | Sunsoul | A warm solar disk with distinct rays | Flare Burst | Keep native spell art |
| Oread | General Oread | A weighty, weathered stone crest | Stone Fist | Keep native spell art |
| Oread | Gemsoul | A large faceted mineral prism | Color Spray | Keep native spell art |
| Oread | Ironsoul | A forged, interlocking metal form | Unerring Weapon | Original parent and hand-choice family |
| Sylph | General Sylph | A broad spiral of wind | Feather Step | Keep native spell art |
| Sylph | Smokesoul | A curling ribbon of ash-gray smoke | Expeditious Retreat | Keep native spell art |
| Sylph | Stormsoul | A forked storm breaking from cloud | Shocking Grasp | Keep native spell and held-touch art |
| Undine | General Undine | A sculpted wave crest and water drop | Hydraulic Push | Original water-force identity |
| Undine | Mistsoul | Layered low mist enclosing a water form | Blur | Keep Blur on the actual spell, not the heritage |
| Undine | Rimesoul | A clear, asymmetric rime-crystal form | Chill Touch | Original spell and matching touch-delivery art |

The inspected implementation deliberately grants Firebelly, Flare Burst, Expeditious Retreat, and Blur as native adaptations. This art project must not rename or replace those mechanics with their tabletop alternatives.

### Core racial traits, affinities, and selection roots

| Family | Exact content to cover | Proposed visual treatment |
|---|---|---|
| Race identity / heritage roots | Ifrit, Oread, Sylph, Undine; four heritage selector roots | Original elemental crests; lineage/choice treatment for selector roots. Reuse the General heritage motif where it represents the same identity, rather than inventing an unrelated duplicate. |
| Energy resistance | Fire, Acid, Electricity, Cold resistance | Four clear protective/deflecting silhouettes with the appropriate energy interaction. These are defenses, not attacks. |
| Ifrit affinities | Fire, Magma, Solar | A coherent channeling/core motif with distinct flame, molten-rock, and solar subjects. |
| Oread affinities | Earth/base Acid presentation, Crystal, Metal | The same affinity vocabulary with stone, mineral facets, and forged metal. Preserve existing names and mechanics. |
| Sylph affinities | Air, Smoke, Lightning | Distinct air current, smoke ribbon, and lightning forms within the affinity vocabulary. |
| Undine affinities | Water, Mist, Ice | Liquid, diffused mist, and angular ice forms within the affinity vocabulary. |
| Alternate-trait selector roots | Ten current selectors: three each for Ifrit/Oread/Sylph, one SLA selector for Undine | Recognizable role symbols for resistance, affinity, and spell-like ability, combined with race identity. These are selection icons, not spells. |
| No-additional-replacement choices | Ten current retain/no-additional-replacement entries | A restrained retain/current-trait treatment related to the relevant slot. Do not imply a new buff or granted power. |
| Actual native common features | Keen Senses; Slow and Steady where granted | Preserve native art. |

Do not add currently nonexistent selector slots merely to make the four race screens symmetrical.

## 5. Complete racial feat index: eleven

The following briefs are proposed art direction, not changes to feat behavior.

| Feat | Proposed original identity | Important distinction |
|---|---|---|
| Elemental Strike | A central weapon impact surrounded by a balanced elemental knot: flame, stone, wind, water | Must read as four-element empowerment, not Burning Hands or fire-only damage. Avoid a tiny four-panel collage. |
| Scorching Weapons | A manufactured metal blade with a heated edge | Weapon empowerment, not a projectile spell. |
| Inner Flame | A concentrated ember/core intensifying the same blade motif | A stronger stage in the Scorching Weapons family, not unrelated art. |
| Blazing Aura | A silhouette enclosed in an outward-radiating corona | Distinct from a burning blade by composition and silhouette. |
| Firesight | A clear eye framed by flame and parted smoke | Must remain distinguishable from Cloud Gazer. |
| Airy Step | A light footstep carried by a broad air curve | New racial feat art, not the stock Feather Step icon. |
| Wings of Air | Two translucent wind-formed wings | Recognizable at thumbnail size; no promise of new movement mechanics. |
| Cloud Gazer | A clear eye through parted banks of mist/cloud | Mist visibility, not a generic divination eye identical to Firesight. |
| Inner Breath | A calm figure or chest with a closed, self-contained breath form | Self-sufficiency without breathing, not a breath attack. |
| Hydraulic Maneuver | A sculpted water jet redirecting a target's balance | Original water art; distinct selectable maneuver children. |
| Triton Portal | A small water-elemental silhouette rising from a summoning pool | This is a water-elemental summon, not a travel portal. |

Elemental Strike, Scorching Weapons, and Blazing Aura need their action and visible buff consumers mapped to their approved identity. Wings of Air's visible buff belongs to the Wings family. Hidden weapon-enchantment plumbing does not create an additional illustration requirement.

## 6. Complete alternate racial trait index: twenty-one

| Race | Trait | Proposed original motif / semantic brief |
|---|---|---|
| Ifrit | Wildfire Heart | A heart-ember producing a sudden leading spark; quick initiative, not regeneration. |
| Ifrit | Brazen Flame | A close melee contact edged in a small bright flame; distinguish from temporary Scorching Weapons empowerment. |
| Ifrit | Fire in the Blood | Fiery vitality moving through a blood/vein motif; one member of the healing family. |
| Ifrit | Efreeti Magic | An elemental hand balancing a small and a large figure; original selection identity. |
| Ifrit | Forge-Hardened | A tempered figure or metal body enduring a hammer/heat impression; resilience to fatigue, not an active crafting command. |
| Ifrit | Fire Insight | A fire-elemental silhouette sustained above a summoning mark; duration/summoning, not an all-seeing eye. |
| Oread | Crystalline Form | Faceted skin redirecting a thin ray; distinct from ordinary stone armor. |
| Oread | Earth Insight | An earth-elemental silhouette sustained above a summoning mark. |
| Oread | Granite Skin | A broad arm or shoulder clad in rough granite; natural armor, not a fist attack. |
| Oread | Stone in the Blood | Mineral/acid-linked vitality through the common blood-healing motif; not generic fire healing recolored without semantic review. |
| Oread | Treacherous Earth | Uneven ground buckling beneath a foot; grounded terrain-control identity. |
| Sylph | Air Insight | An air-elemental silhouette sustained above a summoning mark. |
| Sylph | Breeze-Kissed | A protective wind ribbon deflecting an incoming shaft; parent of the gust and wind-control family. |
| Sylph | Like the Wind | A forward-moving stride with a long wind trail; speed, distinguishable from Airy Step. |
| Sylph | Secretive | A mind or face behind an arcane veil; resistance to mental scrutiny/influence, not merely stealth. |
| Sylph | Storm in the Blood | Lightning-linked vitality through the common blood-healing motif. |
| Sylph | Thunderous Resilience | Sound-wave arcs breaking against a protective surface; sonic resistance, not electrical resistance. |
| Sylph | Whispering Wind | A nearly vanished footfall and a thin wind ribbon; stealth rather than a message-delivery spell. |
| Undine | Acid Breath | A sharp, translucent corrosive cone leaving a mouth silhouette. |
| Undine | Nereid Fascination | An entrancing water-formed figure or gaze with converging ripples; not the borrowed Blur icon. |
| Undine | Ooze Breath | A thick, viscous breath cone with heavy droplets; distinct silhouette from Acid Breath, not a generic poison icon. |

The three Insight traits are summoning-duration traits. The three In-the-Blood traits form a healing/vitality family. Shared design grammar is intentional; indistinguishable recolors are not the target.

The visible trait marker, its action where applicable, and its visible effects need a documented family relationship. The twenty-one hidden provider blueprints should stay hidden and normally reuse their trait family's assigned art only where a non-null icon is required.

## 7. Supporting action and effect index

This is the principal source-confirmed action index to prevent a polished feat list with unchanged placeholders in its submenus.

| Family | Consumers requiring explicit coverage | Art treatment |
|---|---|---|
| Hydraulic Push | Racial feature and cast action | Original water-force icon. |
| Hydraulic Maneuver | Feat, parent menu, Bull Rush, Disarm, Trip, Dirty Trick (Blind) | Parent identity plus four water-based action silhouettes: driving the body back; stripping a weapon; sweeping feet; splashing the eyes. Do not distinguish only by tiny letters or tint. |
| Unerring Weapon | Racial feature, parent menu, primary-hand and secondary-hand actions | One precision/weapon family; two legible hand-choice compositions or native hand markers. Preserve parameter identities and handedness. |
| Chill Touch | Racial feature, preparation/cast action, held-touch delivery | One original spell identity, consistently used across the touch graph. Read implementation before art sign-off; a name containing "Chill" is not authority to depict a cold-damage spell. |
| Shocking Grasp | Racial feature, cast, owned held-touch delivery | Preserve the appropriate native spell identity throughout. |
| Elemental Strike | Feat, action, visible effect | Approved four-element weapon family. |
| Scorching Weapons / Inner Flame | Feats, empowerment action, visible effect | Coherent weapon-fire progression. Hidden enchantment does not require another painting. |
| Blazing Aura | Feat, action, visible aura buff | Outward fire-corona family. |
| Wings of Air | Feat and visible flight-abstraction buff | Wind-wing family. |
| Triton Portal | Feat and summoning action | Water-elemental summoning family; no travel glyph. |
| Efreeti Magic | Trait/selection parent, Enlarge Person, Reduce Person | Original parent; preserve native child spell artwork. |
| Crystalline Form | Trait, Deflect Next Ray activatable, Crystalline Deflection Ready buff | Faceted defensive body for trait; clearly directed ray-deflection variation for the action; related ready-state art. Do not bake native active-state indicators into pixels. |
| Breeze-Kissed | Trait, Breeze-Kissed Gust parent, Bull Rush, Trip, Calm Winds, Renew Winds, Winds Calmed buff | A coherent wind-defense/action family. Calm and Renew need unmistakably different direction/state cues, not just bright/dim duplicates. |
| Blood-healing traits | Fire in the Blood, Stone in the Blood, Storm in the Blood visible healing buffs | Reuse the corresponding approved healing-family identity. |
| Acid Breath / Ooze Breath | Trait markers and daily breath actions | Distinct corrosive and viscous-cone identities. |
| Nereid Fascination | Trait, active use, and any player-visible resulting status | Fascination family; separately inventory actual visible status consumers. |
| Treacherous Earth | Trait, ground-targeted use, and any visible ground/status consumer | Treacherous-ground family. Hidden areas/mechanics are not automatically extra paintings. |

During the runtime census, follow granted facts, ability variants, sticky-touch delivery, activatable abilities, buffs, and any UI-specific icon override. Assign every encountered consumer an explicit disposition: original, intentional shared family, native semantic reuse, protected, or hidden/internal.

## 8. Weapon monograms and Rapid Reload

### Firearm lettering

Cover Pistol, Musket, and Blunderbuss in Weapon Focus and the dependent firearm feat families, plus Rapid Reload's firearm choices wherever the same presentation is used.

Prototype the native renderer first. Compare P/M/B directly with adjacent native weapon categories and the accepted Katana/Nodachi/Wakizashi examples in the same screen, at the same scale. Verify selected, unselected, unavailable, and character-sheet presentations where supported.

Keep native parent feat emblems intact. Do not repaint Weapon Focus itself with the firearm category mark.

Inspect Elven Branched Spear for the same category-lettering and clipping issue. Existing legacy/advanced weapon records do not authorize new rifle/revolver publication. Keep current support and visibility unchanged.

### Rapid Reload

Replace its parent emblem with an original loading symbol inside the matching native combat-feat treatment. Use the actual neighboring icons as references for circle proportions, line weight, hue, negative space, and texture.

The current generator draws the partial ring explicitly; replacing a source image while leaving that generator authoritative would not be a durable fix. Retire or redirect only the affected regeneration path. Do not regenerate item illustrations or accepted gunslinger action icons as collateral work.

## 9. Strategic spell art

| Spell | Proposed composition | Distinction to preserve |
|---|---|---|
| Teleport | A figure or party-token silhouette dissolving between two distant arcane anchors, joined by a curving discontinuity | Long-range translocation, clearly unlike the borrowed Dimension Door doorway. |
| Greater Teleport | The same arcane anchor vocabulary with a stable, symmetrical destination-lock composition | A recognizable sibling with a different silhouette, not merely brighter Teleport. |
| Word of Recall | A returning thread or figure converging on a luminous, anchored sanctuary/home seal | Return to safety, usable as a cleric/druid spell identity; avoid tying it to one deity or a church-only emblem. |

Keep all three distinct. Teleport and Greater Teleport may share a base motif, brushwork, and lighting language.

Map their icons through spell learning, preparation, spellbooks, tooltips, and the world-map presentation surfaces that actually display them. The spells remain strategic/world-map spells; do not enable local casting or action-bar autofill just to create an icon screenshot.

### Strategic scroll item icons

Inspect the item icons for Scroll of Teleport, Scroll of Greater Teleport, and Scroll of Word of Recall independently. These are explicitly in scope. The final item icon may either reuse the matching spell identity directly or use a deliberate scroll-specific derivative, but it must unmistakably correspond to its own spell. Unrelated donor-item carry-through, such as a Teleport scroll still displaying Hold Person art or a Greater Teleport scroll still displaying Fireball art, is a defect to remove rather than an acceptable native-parchment exception.

The scroll implementation points to these canonical abilities while preserving native scroll-item presentation. Distinguish the assigned concept glyph from the parchment item body. Update visible scroll-item icons consistently across inventory, merchant, tooltip, and any other item surfaces, but do not automatically repaint ordinary generic parchment or mutate donor items purely as a side effect.

## 10. Overlooked-content audit without uncontrolled expansion

Record a keep/replace/review decision for visible icons in other added-content modules: Acadamae Graduate, Bodyguard/In Harm's Way/Helpful, Brown-Fur and Urban Barbarian features, and expanded summoning choices. Prioritize obvious unrelated donor art or letter placeholders; do not assume every added icon is bad or replace every existing painting.

Shield Other already explicitly loads the project's `shield-other` icon for its spell and buff. Treat it as review-and-preserve unless visual inspection identifies a defect. Its existence is not a reason to regenerate it.

Any further replacement candidates should be listed with the visible defect and proposed scope disposition. Do not silently expand this project into a redesign of the whole mod.

## 11. Production workflow and approval gates

### Phase 1 — Inventory and protected baseline

Start from the current authorized working branch, not a forced checkout of this planning baseline. Reconcile changes since the inspected commit.

Produce a manifest recording:

- Stable blueprint symbol/GUID, module, player-facing name, and publication state.
- UI surface and any intermediary feature, ability, buff, sprite, or text-monogram override.
- Current icon source, planned semantic family, and keep/replace/native-render disposition.
- Source artwork, export key/path, target dimensions, and approved output hash where relevant.
- Explicit native-reuse and hidden-provider explanations.

Record hashes of protected image files and the runtime icon assignments they must retain. A file hash alone does not protect against code accidentally remapping a good icon.

### Phase 2 — Reference pack and rendering measurements

Gather a small reference pack from the user's screenshots, accepted project source art, and the executor's actual game installation. Include native combat-feat exemplars, native weapon monograms, and appropriate painted spell/racial examples.

Use references to compare composition, stroke weight, color, texture, crop, and visual density. Do not copy unrelated spell or other-mod pixels into new content. Do not redistribute font binaries or extracted game assets as part of the mod.

Measure actual UI sizes and determine what the UI supplies itself: border, ring, background, selection tint, disabled overlay, or counter. Avoid double frames and baked-in selection states.

### Phase 3 — Small representative pilot

Before producing the full set, establish working examples for:

- Native P/M/B lettering and the Rapid Reload emblem.
- A race/heritage/affinity/resistance grouping that demonstrates distinct roles.
- Elemental Strike and a Hydraulic Maneuver parent/child pair.
- A Teleport/Greater Teleport sibling pair and the Recall direction.

The pilot is reviewed in a contact sheet at actual-size thumbnails and in real UI contexts, alongside native references. Reject a failed art direction before generating dozens of siblings.

An image-generation tool or artist should produce the painted work. Codex can own extraction, mapping, reproducible exports, integration, and tests. If its environment cannot produce the required painted artwork, treat that as a separate asset-production dependency; do not quietly replace it with hand-coded circles, silhouettes, or fake calligraphy and call the art finished.

### Phase 4 — Production in coherent groups

After pilot approval, work through race/heritage/trait families, feats and supporting actions, and strategic spells in small reviewable batches. Provide each asset with the approved references and a semantic brief. Avoid huge grids that require uncertain cropping or give each painting too little attention.

Create related pairs or families together, but export each finished icon individually. Preserve the successful pilot as an anchor throughout later batches.

### Phase 5 — Deterministic export and isolated integration

Preserve high-resolution originals and editable compositions separately from runtime exports. Use the existing toolchain where adequate; no new runtime art-generation dependency is required.

Confirm category-specific export dimensions against actual native/project use rather than forcing all icons to one size. Inspect 32-pixel, approximately 40–48-pixel, and 64-pixel previews, plus larger tooltips where applicable. A larger export cannot rescue an unreadable composition.

Handle alpha, chroma spill where applicable, color-space consistency, edge padding, scaling, and sharpening deliberately. Check light and dark backgrounds. Decorative full-bleed backgrounds may touch an edge; important foreground silhouettes must not be accidentally clipped.

Export approved art to the repository's established source-to-package path. The inspected source exports live under `assets/game/icons`; the runtime loader expects installed `assets/icons`. Validate that packaging produces the correct destination.

Extend the existing cached sprite infrastructure with narrowly scoped mappings. Prefer assignment during owned blueprint construction or an explicit post-registration phase that cannot later be overwritten. Do not add broad global getter patches, draw-time file loads, or repeated texture allocation.

Make regenerated outputs reproducible from the approved source and exporter. Regeneration should not invoke a fresh image model and silently substitute new artwork. Update manifests and old generators so approved assets remain authoritative.

### Phase 6 — Technical qualification and visual acceptance

Technical checks should verify coverage, mappings, correct native-reuse exceptions, no unapproved fallback, valid exports, package inclusion, protected-asset hashes, unchanged blueprint identities, unchanged feat parameters, and unchanged mechanical/publication state.

Use behavior-level runtime checks against real game objects where available, not a mocked assertion that an icon setter was called. Audit after late publication/compatibility processing as well as initial construction.

Capture the final packaged artifact in actual character creation, heritage and alternate-trait screens, feat submenus, real supported action bars/variant menus, visible buffs, spellbooks, and strategic/scroll UI. For the strategic scrolls, include inventory/item-tooltip evidence proving that Scroll of Teleport, Scroll of Greater Teleport, and Scroll of Word of Recall no longer show unrelated donor-item artwork. Include an existing-save smoke test through the authorized disposable test path, preserving campaign saves.

Keep separate statuses for technical tests and visual approval. A valid PNG, passing build, non-null sprite, or attractive large contact sheet does not establish visual acceptance.

## 12. Acceptance criteria

An icon family is finished when:

1. Its subjects and actions can be recognized at the actual displayed size, not only when enlarged.
2. Its neighboring choices are distinguishable by composition or silhouette, not only hue.
3. The style fits its destination: painted magical art, native combat emblem, or native monogram.
4. Every relevant visible consumer displays the approved art; genuine native spell reuse is deliberate and documented.
5. Important edges are not clipped; no chroma halos, muddy resampling, doubled frames, or baked-in status overlays remain.
6. Protected art and native donor icons are unchanged, and gameplay/save identities remain intact.
7. Source, export steps, mapping, approval record, and final artifact hashes are preserved.

Owner visual approval is a real gate. An executor may report implementation complete while visual acceptance is pending; it must not convert its own technical checks into a claim that the art has been approved.

## 13. Deliverables for the eventual mission

- Exact icon-consumer inventory with explicit dispositions and intentional sharing.
- Protected-art baseline and a small reference/style guide.
- Approved pilot images and actual-size comparisons.
- High-resolution/editable source artwork, provenance, and deterministic exporters.
- Runtime assets and exact semantic mappings, with obsolete regeneration paths corrected.
- Tests, final-package screenshots, and separate technical/visual acceptance records.
- Concise handoff identifying changed art, preserved art, genuine native reuse, and any remaining unapproved candidates.

This plan does not authorize a release, a merge, campaign-save modification, broad refactoring, or an unrelated art expansion.

## 14. Primary source map

Paths are relative to the inspected repository; C# paths below are beneath `src/KingmakerGunslinger/` unless otherwise shown.

| Source | What it establishes |
|---|---|
| `README.md` | Current published elemental inventory; strategic spell scope; older historical checkpoints are separately labeled. |
| `ElementalRaces/ElementalHeritagePolicy.cs` | Twelve heritages, named affinities, actual native spell substitutions, three custom racial spell-like abilities. |
| `ElementalRaces/ElementalHeritageBlueprintFactory.cs` | Current icon sharing among heritage roots, markers, affinities, and spell-like abilities. |
| `ElementalRaces/ElementalHeritageAbilityFactory.cs` | Unerring Weapon hand variants and Chill Touch/Shocking Grasp delivery graphs. |
| `ElementalRaces/ElementalFeatPolicy.cs` | Complete eleven-feat catalog. |
| `ElementalRaces/ElementalFeatBlueprintFactory.cs` | Current donor-icon behavior, feat descriptions, Hydraulic Maneuver choices, action and buff consumers. |
| `ElementalRaces/ElementalAlternateTraitPolicy.cs` | Twenty-one traits; ten replacement selectors; current publication allowlist. |
| `ElementalRaces/ElementalAlternateTraitBlueprintFactory.cs` | Markers, hidden providers, retain choices, selector roots, and supporting factories. |
| `ElementalRaces/ElementalBreezeKissedFactory.cs` | Gust, Bull Rush, Trip, Calm Winds, Renew Winds, and calmed buff. |
| `ElementalRaces/ElementalCrystallineFormFactory.cs` | Deflect Next Ray and ready-state buff. |
| `CustomWeapons/CustomWeaponSelectorRuntime.cs` | Null-sprite plus text-monogram presentation. |
| `Feats/NativeFirearmFeatIntegration.cs` | Explicit firearm parameter sprites and blueprint-based feat parameters. |
| `Blueprints/ProjectAssetIcons.cs` | Existing sprite cache, required assets, firearm PNG assignments, broad recursive mapping to avoid extending. |
| `Blueprints/TeleportationSpellBlueprints.cs` | Dimension Door art reused for all three strategic spells. |
| `Blueprints/TeleportationScrollBlueprints.cs` | Native parchment item presentation, canonical spell associations, and the scroll-item icon assignments that must be audited separately from the spell icons. |
| `Blueprints/ShieldOtherBlueprints.cs` | Existing project-original spell and buff icon assignment. |
| `tools/New-FirearmFeatIcons.ps1` | Wrapper around the shared icon-overhaul generator. |
| `tools/icon-art/New-IconOverhaulAssets.ps1` | Procedural P/M/B paths, thick Rapid Reload arc, source/export destinations. |
| `THIRD-PARTY-ASSETS.md` | Existing asset provenance and source/export practices; historical generator descriptions should be checked against current code. |
| `AGENTS.md` | Project-specific safety and regression expectations for the eventual implementation. |
