# v0.0.166 — Expanded Summoning Druid rows and giant sizes

`0.0.166-expanded-summoning-druid-giant-sizes`

This release corrects two things reported after v0.0.165. No creature's
statistics, attacks or AI change, every spell keeps its own source and costs,
and no saved identity changes.

## Menus

- **Druid with Summon Nature's Ally prepared:** a Druid who prepares Summon
  Nature's Ally I or II itself now sees each creature once. v0.0.165 listed
  every creature twice in that spell's menu, including the 1d3 groups at
  level 2. Converting any other prepared spell works as before.

## Giants

- **Heights in play:** summoned giants now keep their family heights during
  ordinary play. Measured against the Hill Giant, the Stone Giant is 1.2 times
  as tall, the Fire Giant 1.4, the Frost Giant 1.5, the Cloud Giant 1.8 and the
  Storm Giant 2.1. In v0.0.165 a giant went back to its base body as soon as it
  was selected or targeted, or drew its weapon for combat.
- **Huge space:** the Cloud and Storm Giants are Huge and now take a Huge
  space, one and a half times the Large giants' (15 feet against 10). Their
  selection ring is larger and they need more room. Reach counts from the edge
  of that space, so their melee reach is about 1.8 feet (0.55 m) longer than in
  v0.0.165. The Hill, Stone, Fire and Frost Giants keep their Large space.
- Frost Giants met as enemies are unchanged.

## Limits

- The level-up preview still folds a single elemental spell behind
  "Elementals (1)".
- Generated summon descriptions still read "Summons X through the native
  summon lifecycle."
- Feral Hunter and Preservationist PC menu tooltips were last verified on a
  v0.0.164 development build: the test machine's mouse pointer interferes with
  those hovers. Their menu code is unchanged.
- Loading a save that already contains a summoned Cloud or Storm Giant was not
  rechecked in game for this release. Their size is applied each time the
  giant's model is created, the same step a fresh summon uses.

Checked with focused domain tests, the release's repository validation and
complete domain suite, two deterministic release builds, strict package
validation, and guarded in-game reviews: the six-giant lineup, whose sizes
held through hover, a renderer and collider refresh, hiding and showing and a
weapon swing, and a Druid's prepared Summon Nature's Ally I and II menus with
Call of the Wild. The owner authorized this release.
