# Tabletop Added Rules v0.0.181

This stable release adds the tabletop cleric alignment restriction to Summon Monster. It adds no
identities; every identity from v0.0.180 is unchanged, so existing saves keep their references. The
roster and its 1,208 public summon choices are unchanged.

## Cleric alignment and Summon Monster

A cleric can't cast a spell of an alignment opposed to their own or their deity's. Summon Monster now
follows that rule, using the alignment of the creature it summons.

- **Forbidden choices leave the menu.** A forbidden creature no longer appears in that cleric's Summon
  Monster popup. Alphabetical order is unchanged for what remains.
- **Refused before anything is spent.** A forbidden choice held on a hotbar is refused when it would be
  cast. No slot is spent and nothing is summoned. A change of alignment or deity applies at once.
- **Both the character and the deity count.** A Lawful Neutral cleric of Asmodeus can't summon the
  Good-only Astral Deva, while the same cleric of Abadar can.
- **Templated creatures match the caster.** Celestial or fiendish animals and similar choices follow the
  Summon Monster table's rule, and elementals and mephits carry no alignment.
- **Herald Caller.** Its spontaneous conversions follow the same rule.
- **Who it applies to.** Characters with real Cleric levels, in any of their spellbooks. Other casters,
  Druids, items and class summoning abilities keep their existing behavior.

Choices with no alignment concern are unaffected, and with Expanded Summoning disabled the game's
original behavior is preserved.

## Known limitations

- **Races Unleashed spider summons.** These apply only with Races Unleashed's Spider Summoner feature.
  This is an accepted limitation of this release.
  - "Summon Monster V (Ogre Spider)" summons a Bralani, or a Redcap for an evil caster, rather than an
    ogre spider, in the tested installation. Clerics are restricted by the creature that actually
    appears, and Races Unleashed itself is unchanged.
  - Giant Crab Spider and Giant Black Widow are not classified by the cleric alignment rule and keep
    their native behavior.
  - Giant Black Widow, and by the same rule Giant Crab Spider, is not offered in a Herald Caller
    conversion. Giant Tarantula is.

## Disclosed open items

- **Native evidence.** Results stay credited to the development candidates that produced them. None was
  re-executed on this version-stamped build. The latest native result for this feature remains FAIL, 70
  PASS / 1 FAIL. Its one failure is the compatibility census for the two unclassified Races Unleashed
  rows above: an explicit owner-accepted exception, not a passing result.
- **Observed in game.** Filtered menus, permitted casts that spend exactly one slot, forbidden attempts
  that spend nothing, the deity case, and Herald Caller conversions.
- **Not observed in game.** A naturally levelled Herald Caller (the conversion feature was granted to a
  levelled cleric), saved hotbars after a reload, cold loads, console navigation, autonomous AI, a
  multiclass spellbook menu, evil or true neutral cleric menus, a cast of the Redcap branch above, and a
  game session with Expanded Summoning disabled (focused tests only).
- **Earlier items.** The disclosures in the v0.0.180 notes still stand, including Sprint 41 findings A-5,
  A-6, A-7, A-9, A-13 and A-16, and roster-scale persistence and performance not being run.
