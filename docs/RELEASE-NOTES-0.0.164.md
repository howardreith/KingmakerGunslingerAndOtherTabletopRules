# v0.0.164 — Expanded Summoning cleanup

`0.0.164-expanded-summoning-cleanup2`

This release revisits the Expanded Summoning creatures and summon menus. It
redraws several creature bodies, gives the six giant families their own
colors and heights, and makes elemental and alternate-summoner menus clearer.
No creature's rules, statistics, mechanical size, reach, attacks or AI
change, every spell keeps its own source and costs, and no saved identity
changes.

## Creature bodies

- **Giant Crab:** a low, connected body. Its claws now visibly reach the
  target and fold back afterwards, with the same Claw timing and rules.
- **Girallon:** a joined neck and lower hands with separated fingers.
- **Giant Stag Beetle:** a rebuilt shell, mandibles and six legs.
- **Griffon:** refined eagle and lion joins and wings.
- **Elephant:** an opaque, continuous body.
- **Stegosaurus:** an elongated body with slimmer, joined limbs and feet.
- **Brachiosaurus:** a refined neck, chest, hips and legs, with a tail that
  follows its rig.
- **Gorgon:** forged, staggered scales with neck and cheek guards and an
  iron-hide finish.
- **Ettin:** one coherent body with two authored heads, rebuilt correctly
  when a save is loaded.

## Giants

- Hill, Stone, Fire, Frost, Cloud and Storm giants each have their own
  palette.
- They are drawn at increasing heights, from Hill (smallest) to Storm
  (largest). This is visual only: mechanical size, reach and collision are
  unchanged.
- Clothing, capes and weapons follow the scaled body, and they hide and
  reappear with it.

## Icons and menus

- **Horse:** a painted full-body icon in the Summon Monster and Summon
  Nature's Ally menus. The Pony is unchanged.
- **Names:** summon choices are named family first, with the quantity
  suffix, for example "Elemental, Fire, Small (1d4+1)" and "Horse (1d4+1)".
- **Druids:** with Call of the Wild's spontaneous summon conversion, the
  conversion menu shows the exact summon rows for each prepared spell.
- **Alternate summoner archetypes:** Monster Tactician, Summoner, Master
  Summoner, Occultist, Naturalist, Feral Hunter and Preservationist menus use
  the same family-first names on PC and controller.

## Elementals

- **One chooser:** pick a rank and an element, then the spell to cast from.
  Prepared Wizard slots, Druid preparations, spontaneous Sorcerer spells,
  metamagic such as Extend, Favorite slots and pinned spells remain separate
  sources, and each cast spends its own source.
- **Spellbook and level-up:** preparation and learning group the elemental
  spells under an "Elementals" fold that can show every original spell, on PC
  and controller.
- **Module off:** with Expanded Summoning turned off, every screen stays as in
  the base game.
- **Special sources:** domain slots, scrolls, wands, items and feature grants
  keep their own native rows.

## Limits

- The level-up preview folds a single elemental spell behind
  "Elementals (1)".
- Generated summon descriptions still read "Summons X through the native
  summon lifecycle."
- Feral Hunter and Preservationist PC menu tooltips were verified on an
  earlier build of this release, not on the final build: the test machine's
  mouse pointer interfered with those hovers. The menu code did not change
  between those builds.

Checked with focused domain tests, the release's repository validation and
complete domain suite, two deterministic release builds, strict package
validation, and guarded in-game reviews of the changed creatures, the giant
lineup with and without the module, menus, elemental casting, learning and
preparation on PC and controller, the module-off screens, and a save reload of
the Ettin, Cloud and Storm giants. The owner approved the artwork.
