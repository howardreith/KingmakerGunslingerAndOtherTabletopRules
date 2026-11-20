# Tabletop Added Rules v0.0.192

This stable release collects the Gunslinger and summoning repairs reviewed since v0.0.191 (remediation Sprints 3 to
5). Every identity from v0.0.191 is kept, in order; two new identities are appended for the Mastodon's Trample AI, so
existing saves keep their references. Unity Mod Manager 0.31.1 or newer is still the minimum.

## Gunslinger

- **Status effects now start and end properly.** Fourteen effects were built without the links Kingmaker's buff code
  needs, so they could fail to show beside the portrait, fail to end on time, or fail to apply: Deadeye Armed,
  Gunslinger's Dodge, the four Bleeding Wound bleeds on a target, Startled (Startling Shot), Targeted Head, Up Close
  and Deadly Armed, Steady Aim Armed, Falling Petal, the Mountain-Sunder round marker, and two retained Eastern Weapons
  effects. All fourteen now have them. How far each was checked in play differs; see the open items below.
- **Deadeye arms again.** In v0.0.191 Deadeye spent its grit and armed nothing. It now arms: an eligible firearm shot
  uses the state, and an unused one ends after one round with no refund. It costs a flat 1 grit, as before.
- **Steady Aim works.** Its armed state was never used by a shot and never granted its 10 feet. The shot is now
  resolved on the attack roll: inside the lengthened first range increment it targets touch armor class, the state is
  removed once whether the shot hits or misses, and grit is not spent.
- **Gun Training says what it does.** Gun Training covers three firearm types (pistol, musket, blunderbuss), each
  trained once, over four opportunities at levels 5, 9, 13 and 17. A base Gunslinger's opportunity at 17 therefore
  grants nothing further. This was already how it worked; the Gun Training, Mysterious Stranger, Pistolero and Musket
  Master descriptions now say so.
- **Companions resume after a misfire.** A companion whose firearm broke during an attack sequence used to stay
  stopped after Quick Clear, Repair Firearm or a full rest, until you gave it an attack order. Recovering that same
  firearm to working order now lets the companion's own AI decide to reload and fire again. Recovery gives no order of
  its own and fires nothing by itself. **This supersedes the v0.0.128 statement that "recovery alone grants no new
  order".**
- **Legacy Advanced Revolver contained.** This six-chamber revolver was never offered by any release (it existed only
  for old saves), and its six-round reload cannot work with the mod's saved firearm state. It now stays loadable but
  cannot be equipped, is not recognized by Craft Magic Items, and says so in its description. One already equipped
  through other means stays equipped and its reloads keep failing safely.

## Summoning

- **Mastodon Trample used by the summon itself.** A summoned Mastodon's Trample (2d8+18, Reflex DC 29) could never be
  used in play, because summons are AI-controlled and the Mastodon's AI only attacked. Its own AI now uses Trample as
  an approach move, only when all of these hold: it is in combat and nothing threatens it; it has its full round (turn
  based); no enemy is already within its melee reach; the target is smaller than it and has not been trampled this
  round; and the straight path crosses the target and ends clear beyond it. Otherwise it attacks as before. Trample's
  damage, save and attack-of-opportunity rules are unchanged, summons still cannot be commanded directly, and the other
  trampling creatures are unchanged.
- **Celestial and fiendish templates on summons.** The rule that strips an inherited celestial or fiendish template
  from a summon that should not have one was a fixed list of creatures, and it disagreed with what the summons really
  inherit. It is now worked out from each summon's own construction, the same way for the ordinary spells and for the
  alternate summoning sources (Monster Tactician, Summoner, Master Summoner and others). Deliberate templates and
  innate creature types are kept.
- **Controller Elementals launcher.** On the controller interface, the Elementals launcher row is now added only when
  it actually replaces at least one original Summon Elemental row, and in that row's place.
- **Call of the Wild metamagic conversions.** With Call of the Wild's "metamagic for spontaneous spell conversion"
  setting, the Herald Caller and Druid conversion menus over a prepared slot listed Call of the Wild's extra rows raw,
  for example "Summon Monster IV (1d4+1 Wolves)". They now show the matching creatures of that tier and quantity, each
  keeping the row's metamagic and spending the slot once. Rows of the slot's own tier stay as Call of the Wild shows
  them. This applies to prepared casters only.

## Existing saves

All 7,933 identities from v0.0.191 are kept, in order. Two are appended: the Mastodon's Trample AI action and its own
copy of the native monster brain. Mastodons summoned before updating keep their old attack-only behaviour until
summoned again. One identity's internal manifest note (the Gunsmith's Kit) was corrected; it was developer metadata
that no player ever saw.

## Disclosed open items

- **Tested on development builds.** Each change was checked in the game on its own development build in a disposable
  copy of the game. The release build is checked in the game after publication, and that result is recorded in the
  repository's release record.
- **Status effects not all fully checked.** Taken through their real trigger in real-time play: Gunslinger's Dodge,
  the four bleeds, Startled, Targeted Head and the Mountain-Sunder marker. Partly checked: Up Close and Deadly Armed,
  Falling Petal and Deadeye Armed (their icon check fails) and Steady Aim Armed. The two retained Eastern Weapons
  effects were only applied directly. Turn-based mode, controller and saving and loading were not checked for them.
- **Mastodon Trample.** Checked in real-time and turn-based combat on one open-ground layout against a single
  medium-sized enemy. Groups, other sizes and rough terrain, an attack of opportunity stopping the trample, and the
  summon's expiry were not observed; the other trampling creatures were not examined.
- **Call of the Wild menus.** The menu with Extend and Heighten Spell grows to 302 rows. The same creature can appear
  twice (once at a higher tier without metamagic, once lower with it), told apart only in the tooltip, and eight
  pairs of rows look identical. Spontaneous casters are unchanged.
- **Controller interface.** One check of the original Summon Elemental row still fails on the controller interface,
  as it did before this release; its cause is not classified.
- **Summoning coverage.** 1,208 summon choices are offered, 9 are withheld (the owner-withdrawn Stirge) and 15
  placements from the original plan are out of scope by decision (Brachiosaurus outside Summon Monster, and twelve
  higher 1d4+1 placements beyond the native options).
- **Earlier items.** The disclosures in the v0.0.191 notes still stand, unchanged.
