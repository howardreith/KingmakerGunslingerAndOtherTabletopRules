# Tabletop Added Rules v0.0.184

This stable release hides the original Summon Elemental spells while Expanded Summoning is active. Their elementals
are already part of Summon Monster and Summon Nature's Ally. A new mod option shows the original spells again as
ordinary spells. Every identity from v0.0.183 is unchanged and none is appended, so existing saves keep their
references. The summon roster and its 1,208 public choices are unchanged.

## Summon Elemental hidden by default

With Expanded Summoning on, the original Summon Elemental spells no longer appear in:

- level-up spell learning ("Choose Spells"), on PC and with a controller;
- the spellbook and memorize lists, on PC and with a controller;
- the PC spellbook's metamagic (star) tab, including metamagic copies of those spells.

The dark "Elementals (n) — show original sources" row is gone from level-up spell learning on PC and with a
controller. The spellbook's version of that row was removed in v0.0.183. No other header or dropdown replaces it.

## New option: Show original Summon Elemental spells

In the mod's Unity Mod Manager settings, under Expanded Summoning, **Show original Summon Elemental spells** is off
by default. When it is on, each original Summon Elemental spell appears once as an ordinary spell row in level-up,
the spellbook and the metamagic tab. You can select, prepare and cast it as usual.

No restart is needed: close and reopen the spell list to apply the change. An open level-up screen keeps the list it
opened with until you reopen it.

## What does not change

Only what the spell lists show changes; no spell, list or saved data is altered.
- **Saved spells.** Spells you already learned and their metamagic copies stay in your save. Prepared copies still
  cast from the action bar.
- **The action bar.** The action-bar Elementals chooser is unchanged.
- **Domain and other granted elemental spells** (for example a Cleric's Fire, Travel or Weather domain spell) stay
  visible, including their metamagic copies, because that slot may have no other choice.
- **Scrolls.** Copying a Summon Elemental scroll into a spellbook is not restricted. With the option off, the copied
  spell is then hidden like any other.
- **Expanded Summoning off.** Nothing is hidden.
- **Unusual level-up screens** that the mod does not recognize show the original spells as ordinary rows, never as a
  dropdown.

## Disclosed open items

- **Tested in game.**
  - The PC level-up "Choose Spells" screen with the option off: no Summon Elemental and no dropdown.
  - The same screen after reopening with the option on: Summon Small Elemental as one ordinary row. Selecting it and
    replacing it with another spell work, and cancelling commits nothing.
  - The PC spellbook level page and metamagic tab, hidden and then shown.
  - Preparing and unpreparing the original spell.
- **Not observed in game.**
  - Controller learning and spellbook. These share the same filtering code and are covered by compilation, checks
    against the installed game files and automated tests.
  - Domain elemental spells. Covered by automated tests.
  - An existing campaign save.
- **Earlier items.** The disclosures in the v0.0.183 notes still stand.
