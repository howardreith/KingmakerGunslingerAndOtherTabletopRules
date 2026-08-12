# Expanded Summoning first-playtest repair mission

Status: complete and runtime-qualified on 2026-08-12. This contract supersedes
the prior Expanded Summoning completion statement for release `0.0.78`; the
authoritative requirement-to-evidence disposition is recorded in
`planning/EXPANDED-SUMMONING-COMPLETION-AUDIT.md`.

## Fixed baseline and publication contract

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Working branch: `codex/expanded-summoning`.
- Reviewed repair baseline: `e9f251c584607dd45a45a2414e2aaffabff4c44b`.
- Draft PR: #2. Keep it draft and never merge it autonomously.
- Preserve repository, saves, settings, installed Mods, frozen blueprint
  identities, native content, and third-party content.
- After every coherent commit, run the required policy push script.

## Human acceptance failure

The real spellbook/UI path works for native summons, all elementals, mephits,
Lantern Archon, Hell Hound, Bralani, Salamander, Succubus, Shadow Demon,
Invisible Stalker, Bebelith, Ghaele, and tested outsider quantity choices.
Most templated natural/proxy Summon Monster choices instead finish or attempt
their full-round cast, spawn nothing visible, and spend no slot. Erinyes is a
separate failure. Native Wolf still works. Menus also expose native/KMG
semantic duplicates, put lower-tier quantity choices before current-tier
singles, reuse indistinguishable icons, and show an oversized Invisible
Stalker view.

## Primary hypothesis and mandatory reproduction

Prove or reject the nested-variant hypothesis before rewriting units. Current
templated choices publish a logical ability beneath the native parent, then
put Celestial and Fiendish execution children beneath that logical ability.
The old harness grants and casts an execution child directly, bypassing the
unsupported player path and slot semantics.

Add an instrumented end-to-end discriminating matrix through a real
spellbook/parent `AbilityData` chain:

1. Native SM I Dog through its real parent.
2. KMG Dog logical root through SM I exactly as selected by the UI.
3. Internal Celestial Dog child directly, retained only as a lower-layer
   control.
4. Internal Fiendish Dog child directly.
5. SNA I Dog through its real parent, using the shared unit without the SM
   template wrapper.
6. Repeat the distinction with Giant Spider or Wolf.
7. Working Small Earth Elemental control.
8. Erinyes independently.

For each case record parent, logical and execution GUIDs; caster alignment;
spellbook and spell level; slots before/after; `AbilityData` parent,
converted-from and source references; target/cast results; command result;
`RuleCastSpell`, spawn-action and `RuleSummonUnit` observations; post-
`EntityCreator.Tick` live-world survival; descriptor, pool, duration, view,
faction and destroyed state; exceptions; and a bounded authoritative log tail.
The current build must demonstrably fail the KMG Dog player route before a
repair can pass.

## Required functional repairs

### Template delivery

Remove nested player-facing `AbilityVariants`. Every one of the 681 logical
placement identities must be the direct executable ability published to the
native parent. For templated SM choices, copy the proven native spawn graph to
the logical root and apply exactly one caster-derived template after spawn:
Good to Celestial, Evil to Fiendish, and Neutral through a save-safe
per-character mutually exclusive alignment mode. The neutral default must be
deterministic and documented. Alignment, descriptor, template and bounded
smite must agree. Previously frozen execution-child identities remain
registered as unpublished compatibility shells.

Do not expose two choices per creature, force all neutral casters permanently
to one template, remove templates, call factories directly, patch global
summoning, or delete identities. Preserve one exact slot spend on success and
none on rejected/cancelled casts, plus native casting time, range, duration,
metamagic and summoning-feat behavior.

### Natural units and Erinyes

If SNA controls fail, repair the natural/proxy chassis separately. Prefer
sanitized dedicated summon donors; otherwise preserve a proven working donor
class/body/view/runtime chassis and mutate only required independent fields.
Clone mutable data, strip only forbidden campaign surfaces, prove donor
immutability, and verify the created unit remains live after queued entity
creation.

Instrument Erinyes independently. Use a proven dedicated summon-capable donor
or build a KMG-owned outsider profile with valid class/HD, view, animations,
safe weapons and ranged behavior while excluding loot, story, dialogue,
persistence, teleportation, planar travel and nested summoning.

### Native reconciliation and display order

Add a frozen GUID-based map from exact native child identities to equivalent
KMG creature/multiplicity keys. Suppress only those mapped semantic duplicates
from displayed parent variants; leave every original blueprint unchanged and
registered. Preserve every unique native/Owlcat option, including Movanic
Deva and Frost Giant, and every non-proven-duplicate third-party reference.
Disabling the feature must restore the exact original collections.

Display current-tier one-creature choices first, then unique native/Owlcat
one-creature alternatives, then `1d3`, then `1d4+1`, with frozen curated order
inside KMG groups and stable relative order for unclassified third-party
content.

### Icons and Invisible Stalker

Use an immutable icon-selection catalog: exact donor/creature icon when safe,
then coherent category fallbacks for canine, feline, bear, flying beast,
reptile/dinosaur, spider/vermin, each elemental/mephit element, celestial,
and fiend. Quantity stays in names. No external artwork is authorized.

Move Invisible Stalker to the Medium Air Elemental view first, or a proven
deterministic view-only scale, without changing the Huge Air Elemental summon.
Prove silhouette, footprint, selection, camera, locomotion, attacks, hit,
death and invisibility.

Elementals remain in Summon Monster. The standalone Summon Elemental spell is
untouched. SM VIII remains four Elder Elementals and SM IX remains Ghaele as
the intentionally sparse KMG additions; unique native VIII/IX choices remain
visible. Inventory possible high-tier donors only after the repair is stable,
and defer speculative additions.

## End-to-end acceptance redesign

Add a distinct guarded player-path scenario that grants/learns the native
parent in an actual spellbook, resolves the selected variant through the same
parent chain as UI, never grants an internal child, advances casting,
animation, execution, queued creation and world updates, proves one correct
slot spend, proves a live non-destroyed same-kind summon with view, descriptor,
faction, pool, duration, caster context and commands, and proves cleanup.
Cover Good/Neutral/Evil template selection, SNA same-unit control, native
duplicate reconciliation, exact donor, natural, special and Erinyes. Retain
direct-child tests only as explicitly lower-layer evidence.

Minimum human-path matrix: native Dog; KMG Dog, Eagle, Giant Spider, Wolf,
Lion, Dire Tiger, Mastodon, Roc and Erinyes; Small Earth Elemental; Lantern
Archon; Salamander; Succubus; Bebelith; Ghaele; one natural `1d3`; one natural
`1d4+1`; one same-unit SNA natural; Movanic Deva; and Frost Giant. Then run
the complete generated logical-root matrix.

## Completion gates

Completion requires every approved logical entry to pass through its native
spellbook parent, exact slot semantics, no silent natural/proxy no-op, working
Erinyes, exact native duplicate suppression, retention of all unique native
and third-party variants, deterministic singles/`1d3`/`1d4+1` order, usable
icons, Medium-scale Invisible Stalker, retained elementals and standalone
Summon Elemental, and no regression to prior working summons.

Also require the complete domain/static suite, clean Release build, strict
deterministic package, all 16 feature states with constant registration,
enabled/disabled persistence, standalone/CotW/Arms and Armor/Toggle/high-risk
profiles, and regressions for Gunslinger, Acadamae, Cord, Shield Other,
vendors, firearms, settings and packaging. Freeze final source, DLL and
package hashes. Update reports with root causes, proof matrix, Erinyes,
reconciliation map, before/after tier and multiplicity counts, icon fallbacks,
Invisible Stalker change, exact slot evidence, run IDs, known limitations and
deferred high-tier candidates. Push and update draft PR #2 without merging.

## Stopping conditions

Stop only for repository corruption, unrecoverable proprietary dependency
loss, credible save risk, publication credentials unavailable through every
authorized path, or a proven engine limitation making a mandatory requirement
impossible after safe alternatives. Builds, test failures, instrumentation
work, donor uncertainty and ordinary runtime failures are not stopping
conditions.
