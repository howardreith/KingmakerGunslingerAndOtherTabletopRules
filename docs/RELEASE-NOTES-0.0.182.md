# Tabletop Added Rules v0.0.182

This stable release adds the Races Unleashed Spider Summoner integration, regroups the summon menus by quantity,
removes Sunder and Dismantle Armor from summoned creatures, and renames the Lillend choice. Every identity from
v0.0.181 is unchanged, so existing saves keep their references; 39 Spider Summoner identities are appended. The
roster and its 1,208 public summon choices are otherwise unchanged.

## Summon menus: quantity first

Summon Monster and Summon Nature's Ally menus now list every single-creature choice first, then every 1d3 choice,
then every 1d4+1 choice, alphabetical inside each group. Summon Monster VII, for example, opens with its single
summons (Bebelith, the elementals and so on) rather than Ankylosaurus (1d4+1). The same order applies to every tier,
higher-tier quantity choices, retained native choices, the alternate summoning sources, and Druid and Herald Caller
conversion popups. Separate spell levels and conversions stay separate.

## Races Unleashed: Spider Summoner

With Races Unleashed installed, its Spider Summoner feat now works with Expanded Summoning:

- **All four spiders appear as themselves.** Giant Crab Spider, Giant Black Widow, Ogre Spider and Giant
  Tarantula. "Summon Monster V (Ogre Spider)" now summons an ogre spider, not a Bralani or Redcap.
- **Higher tiers.** Each spider is offered at its own tier (one), one tier higher (1d3) and two or more tiers higher
  (1d4+1), in both Summon Monster and Summon Nature's Ally: 23 placements per family. The original Races Unleashed
  rows keep their names; the added ones read, for example, "Spider, Giant Black Widow (1d3)".
- **Requires the feat**, in menus and when cast. The ordinary Giant Spider stays available without it.
- **Templates and alignment.** Summon Monster spiders take the usual celestial/fiendish template; Nature's Ally
  spiders follow the caster's alignment. Cleric alignment restrictions apply to the spider that actually appears.
- **Conversions.** Herald Caller and Druid conversions offer the spiders at their tiers and spend the converted
  spell.
- **The feat's benefit.** A summoned spider's poison save DCs and the difficulty of escaping its web are +2, once.

## No Sunder or Dismantle Armor on summons

- **Storm Giant** no longer has Improved Sunder, so it never sunders and its attacks are never replaced by a
  sunder attempt.
- **Greater and Elder Earth Elementals** no longer carry the Improved Sunder and sunder-focused AI they inherited
  from the base game; they use the ordinary attack AI.
- **Bebelith** no longer uses Dismantle Armor. A "Dismantled Armor" effect left on a character from an older save
  remains as a retired marker that no longer reduces armor class.
- **Older saves.** A Storm Giant or earth elemental summoned before this version loses its Sunder ability when the
  save loads. A saved earth elemental keeps its former ordinary attack AI.

Player characters, ordinary enemies and other creatures are unchanged, and ProperFlanking2 is untouched.

## Azata, Lillend

The Lillend choice now reads "Azata, Lillend", beside "Azata, Bralani" and "Azata, Ghaele". The creature,
its abilities and its icon are unchanged.

## Known limitations

- **Call of the Wild metamagic conversions.** With Call of the Wild's "metamagic for spontaneous spell conversion"
  setting enabled, a Herald Caller conversion popup over a metamagic source also lists Call of the Wild's own
  base-level conversions as raw native rows. This is an open design choice; nothing was changed.

## Disclosed open items

- **Native evidence.** Results stay credited to the development candidates that produced them; none was
  re-executed on this version-stamped build.
  - Spider Summoner: candidate `0908e3415fa4`, every acceptance row closed.
  - Menu order and Sunder: candidate `3f170d4a9cea`, 44 PASS / 1 FAIL. The failure is the Sunder census
    counting the save fixers' own removal lists; a separate reader-only re-evaluation of the recorded data
    passes with no Sunder source. The earlier candidate `c6e861a98a93` failed at initialization.
- **Observed in game.** Quantity-first popups (Summon Monster V, VI, VII and a Druid conversion), the spider rows
  with and without the feat, "Azata, Lillend", a fresh Storm Giant and Greater Earth Elemental with no Sunder state
  and ordinary attacks, and induced old-save state removed by the fixers.
- **Not observed in game.** The Bebelith Dismantle Armor withdrawal and the retired-marker penalty (focused tests
  and compilation only), the corrected census check, an actual old-save load, the Elder Earth Elemental live,
  Herald and alternate-source menus as opened popups, and autonomous AI decisions.
- **Earlier items.** The disclosures in the v0.0.181 notes still stand.
