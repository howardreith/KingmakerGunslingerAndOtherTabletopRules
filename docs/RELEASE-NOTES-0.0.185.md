# Tabletop Added Rules v0.0.185

This stable release stops two regional merchants from selling this mod's weapons. The Dire Narlmarches village
trader and the Pitax trader no longer stock the generic Eastern weapons or Elven Branched Spears. Every identity
from v0.0.184 is unchanged and none is appended, so existing saves keep their references. The summon roster and
its 1,208 public choices are unchanged.

## Regional merchants no longer sell the added weapons

Each of the two merchants loses 18 items:

- **Eastern weapons (12):** Wakizashi, Katana and Nodachi, each in ordinary, masterwork, cold iron and +1.
- **Elven Branched Spears (6):** ordinary, masterwork, cold iron, masterwork cold iron, +1 and +1 cold iron.

No firearms or named weapons were sold by either merchant. You can still sell any weapon to them as ordinary trade.

## Where to buy these weapons now

- **Ordinary and masterwork:** Oleg, in Act I.
- **Cold iron and +1:** Honest Guy in Beneath the Stolen Lands, in either the campaign's Tenebrous Depths or the
  standalone mode. This needs that DLC. No base-campaign merchant sells them any more.
- **+1 to +5:** the capital blacksmith, only with the optional Better Vendors integration. The capital's ordinary
  stock does not include these weapons.

Named weapons, weapon recovery, starting firearms, the Skeletal Salesman, Xelliren and crafting are unchanged.

## Existing saves

When you first open each merchant after updating, the game removes the old stock by item type and count. It never
touches your inventory, equipment, stash or companions.

- **An unsold stocked weapon** is removed.
- **A weapon you bought** stays yours; nothing is taken back.
- **A weapon sold back to the merchant.** If you bought the stocked weapon and later sold the same kind back, that
  sold copy is removed instead. The game records only item types and counts, so it cannot tell them apart. Extra
  copies beyond the one stocked survive.
- **Reopening, saving and reloading** does not remove anything more.
- **Very old merchant records** with no stock history keep their old weapon.

## Disclosed open items

- **Not observed in game.** None of these were seen in the game: the merchants' fresh stock, cleanup of an
  existing save's stock, reopening trade, or save/reload. The change is covered by compilation, automated tests
  and the decompiled game code.
- **Earlier items.** The disclosures in the v0.0.184 notes still stand.
