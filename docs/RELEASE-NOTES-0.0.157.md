# v0.0.157 — Expanded Summoning for class summon abilities

`0.0.157-alternate-summoning-sources`

Until now the Expanded Summoning creatures appeared only in the Summon Monster
and Summon Nature's Ally spells. Class abilities that summon on their own
resource still offered only their original creatures. This release gives
those abilities the same creature menus, using the published roster: there is
no second creature list.

| Caller | Requires | What changes |
| --- | --- | --- |
| Monster Tactician (Inquisitor) | — | each per-tier, per-quantity ability opens a menu |
| Summoner Summon Monster (Twinned Summoner shares it) | Call of the Wild 1.14.4c | menus at every tier |
| Master Summoner Summon Monster | Call of the Wild 1.14.4c | menus at every tier |
| Occultist (Arcanist) Conjurer's Focus | Call of the Wild 1.14.4c | menus at every tier |
| Naturalist Summon Nature's Ally | Call of the Wild 1.14.4c | menus at every tier |
| Feral Hunter Summon Nature's Ally | Call of the Wild 1.14.4c | menus at every tier |
| Preservationist Bottled Ally (extracts I-VI cast SNA I, II, IV, V, VII, IX) | Call of the Wild 1.14.4c | menus on the extracts |

Each choice keeps its caller's own rules: resource and cost (including the
Occultist's per-tier reservoir cost and Perfect Summoner), class-level
scaling and duration, summon pools and clearing, the Master Summoner's and
Summoner's eidolon conditions, action type, and the Preservationist's extract
slot. Only the creature and its canonical template or alignment come from
Expanded Summoning. Menus keep any option a caller already had, including
third-party ones, and a caller's original single-creature cast stays
available where the menu replaces it. Large menus use the same scrolling
layout as the spell menus.

With Call of the Wild absent, only Monster Tactician changes. If Call of the
Wild's own summon abilities ever no longer match what this release expects,
that family stays unchanged and its new choices are registered as unavailable
placeholders instead of guessing. With the Expanded Summoning module turned
off, nothing new is offered: the new choices stay registered but refuse to
cast, so a saved shortcut to one is designed to be refused safely and to work
again once the module is back on. That shortcut behavior was not checked with
a real saved shortcut (see Limits).

The release adds 3,746 new stable identities (in their own
`KMG.Summoning.Adapter.*` namespace) after the unchanged released ones. The
1,122 spell summon choices of v0.0.155 are unchanged.

## Limits

- **Checked automatically:** the in-game graph of every adapted choice
  against its caller's template, under two specific contracts (the signature
  contract and the composed template/alignment contract), with negative
  controls; real casts from natively levelled characters of every family in
  a scripted fixture, including a Master Summoner with a real lesser eidolon,
  the Occultist's lower-tier groups and the Preservationist's extract III ->
  Summon Nature's Ally IV; and, on a real save, choosing creatures by clicking
  the action-bar menu rows and casting them in normal real-time play
  (Master Summoner tiers I, III and VII, including the Tyrannosaurus).
- **Not checked before release:** dragging a new choice onto the hotbar and
  using it after reloading a save (normal, module off and back on), a
  turn-based round, a menu large enough to need scrolling at your screen
  size, a saved shortcut while a family is blocked, and a grapple or swallow
  across a save and reload. The game does not allow saving in combat, and a
  grapple is not kept across a reload: a clean reset is the expected result.
  Please report anything unexpected.
- Not adapted: Occultist/Relic Hunter Servitor and Shadow Beasts, Warpriest
  and Divine Tracker Battle Companion, Shadow Conjuration, Skald Spell
  Kenning, Lore Wish, Twinned Eidolon spell-likes, Pact Wizard conversions and
  Rogue Major Magic. Archetypes that remove their summon features are not
  given them back.

Checked with focused domain tests, the release's repository validation and
complete domain suite, and guarded native scenarios on the release build.
