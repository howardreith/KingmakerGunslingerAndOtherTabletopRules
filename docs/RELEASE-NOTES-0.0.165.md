# v0.0.165 — Expanded Summoning cleanup follow-up

`0.0.165-expanded-summoning-cleanup-followup`

This release corrects four things reported after v0.0.164. No creature's rules,
statistics, size, attacks or AI change, every spell keeps its own source and
costs, and no saved identity changes.

## Creatures

- **Deinonychus:** the arms, hands and fingers are now part of one continuous
  body. They no longer separate from the chest when the arms swing, and each
  hand claw moves with its finger.
- **Fire Giant and Storm Giant:** they no longer show the Frost Giant's
  falling snow and ice. They share the Frost Giant's body, and its frost
  effect now appears only on the Frost Giant itself.

## Icons and menus

- **Pony and Cyclops:** new painted icons that match the other summon
  portraits. Every Pony and Cyclops choice uses them.
- **Druid conversion:** a Druid converting a prepared level 1 spell into Summon
  Nature's Ally I now sees the ten creature choices, as at levels 2-9, instead
  of a single "Summon Nature's Ally I" row with the spell's paw icon.

## Limits

- The level-up preview still folds a single elemental spell behind
  "Elementals (1)".
- Generated summon descriptions still read "Summons X through the native
  summon lifecycle."
- Feral Hunter and Preservationist PC menu tooltips were last verified on a
  v0.0.164 development build: the test machine's mouse pointer interferes with
  those hovers. Their menu code is unchanged.

Checked with focused domain tests, the release's repository validation and
complete domain suite, two deterministic release builds, strict package
validation, and guarded in-game reviews: the giant lineup with the Frost
Giant's effect as a control, every Summon Nature's Ally menu tier, the Druid
level 1 and level 2 conversion menus with real casts, and the Deinonychus in
motion. The owner authorized this release.
