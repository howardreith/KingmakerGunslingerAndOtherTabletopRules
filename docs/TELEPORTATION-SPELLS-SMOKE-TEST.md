# Contextual teleportation player smoke test

Applies to the owner-authorized 0.0.118 release. The owner will test the remaining
campaign and compatibility behavior in an existing high-level game. Keep a save
before testing so the exact before/after result can be compared.

1. Enable **World-Map Teleportation Spells**, restart Kingmaker, and load your test
   save. Obtain spells through normal spellbook/level-up
   selection and preparation. Existing characters receive no automatic grant.
2. On the world map, stop traveling and select a previously visited point away
   from the party. Without an available spell, the native interaction is unchanged.
   The party's current point and unvisited/forbidden points have no magical rows.
3. With a usable spell, confirm the native Travel action remains first and works
   normally. Choosing it spends no spell use. Canceling the destination panel
   starts neither normal nor magical travel.
4. Choose **Teleport** beside the desired caster and spellbook. Check the selected
   destination, current use count, ordinary visits, familiarity and all four exact
   percentages. Cancel first and verify no spell use was spent; reopen and Cast.
5. Verify one prepared use or one correct-level spontaneous slot was spent.
   Teleport may resolve to another visited permitted point or cause mishaps.
   The party remains on the world map; no local area opens and travel time does
   not advance.
6. Choose **Greater Teleport** at an eligible visited destination. Its confirmation
   states exact arrival; Cast spends one use and reaches the selected map point.
7. Before capital establishment, select **Oleg's Trading Post** for **Word of
   Recall**. After establishment, select the **capital's world-map point** instead.
   Recall appears only at its current destination and never enters the local area.
8. With a controller, use native directional navigation to reach each spell
   source. The list scrolls as needed; native confirm opens the spell confirmation
   and native cancel dismisses it. Travel remains the native default.
9. Check all active members and their associated traveling companions remain
   together. Reopen a destination to check the updated source counts. Repeat
   cancel/reopen operations and check that no duplicate rows appear.

Teleport is Wizard/Sorcerer 5; Greater Teleport is Wizard/Sorcerer 7 (also called
Teleport Without Error); native Travel-domain publication uses levels 5 and 7
when that exact list exists. Word of Recall is Cleric 6 and Druid 8. Scrolls,
wands and item charges are outside this feature.
