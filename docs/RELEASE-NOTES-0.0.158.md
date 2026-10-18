# v0.0.158 — Expanded Summoning visual refinement

`0.0.158-expanded-summoning-visual-refinement`

This release redraws and repairs the Expanded Summoning creatures. It also
carries the Brown-Fur startup-order and CotW controller save-loading repair
that was tested as `v0.0.157-controller-load-alpha.3`. No creature's rules,
statistics, size, reach, attacks, AI, templates or summon menus change, and
no saved identity changes. Every change is in how the creatures look and how
their visuals are drawn.

## New and repaired creature bodies

- **Rebuilt as single, continuous bodies** in one consistent painted style:
  - Dire Rat
  - Rhinoceros, Woolly Rhinoceros, Aurochs and Bison
  - Tyrannosaurus, Deinonychus, Triceratops and Ankylosaurus
  - Hyena, Goblin Dog, Wolverine and Shadow Mastiff
  - Poisonous Frog (now green and black)
  - Dire Bat, Pteranodon and Giant Wasp
  - Ape, Dire Ape and Xill
- **Repaired in place**:
  - Crocodile and Dire Crocodile: turned right side out and repainted.
  - The three Giant Ant castes: turned right side out, with limbs and mandibles attached to the body.
  - Giant Scorpion: turned right side out, with its stinger attached to the tail.
- **Inside-out bodies turned right side out**, so they are lit from the front:
  Eagle, Stirge, Fire Beetle, Giant Stag Beetle, Girallon, Giant Crab and
  Stegosaurus. Their shape, paint and rigging are unchanged. The Bebelith is
  unchanged.

## Other visual changes

- **No more gold sheen on KMG summons.** Creatures summoned through this
  mod's summon choices no longer carry the base game's golden summoned glow;
  the Wolf, for example, now shows its own coat. The mephits and the lion
  keep the glow their look is built on, and the base game's own summon
  spells are unchanged.
- **Salamander:** its tail now ripples while it moves and sways gently at
  rest, instead of gliding rigidly behind it. Its body no longer shows thin
  white streaks. Its spear, tail slap and every rule are unchanged.
- **Dire Rat icon:** a closer crop on the rat without the bronze medallion,
  so it reads clearly at menu size.

## Brown-Fur and controller loading

- Brown-Fur's startup now waits for the mod's blueprints to be registered
  before it acts.
- The Call of the Wild controller action-bar correction now installs even
  when the optional Harmony diagnostics cannot read a method.
- Shield Other, Teleportation, class-catalog diagnostics, Eastern Weapons and
  Favored Class also wait for blueprint registration before their first
  update.

## Limits

- **Ghaele:** the Ghaele Azata loses the gold sheen like other KMG summons, but
  during review its own fade effect kept rising.
- **Frost Giant and Thanadaemon:** these are covered by the sheen change but
  were not observed in the game.
- **Viper and Constrictor Snake:** they still travel in their coiled pose.
- **Visual review:** the owner reviews the new models in the game after this
  release; refinements will follow with the next summon additions.
- **Not yet checked on this exact build:**
  - controller-mode loading with a physical controller;
  - reloading a save with a Brown-Fur character in it.

Checked with focused domain tests, the release's repository validation and
complete domain suite, two deterministic release builds, strict package
validation, and guarded in-game creature reviews of every changed creature.
