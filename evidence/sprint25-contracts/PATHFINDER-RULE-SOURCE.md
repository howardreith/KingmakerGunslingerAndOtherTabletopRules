# Pathfinder firearm explosion rule source

Sprint 25 uses the Pathfinder Roleplaying Game first-edition firearm malfunction rule published in *Ultimate Combat* and reproduced by Archives of Nethys. In substance, an early firearm that misfires while already Broken explodes; the listed burst includes the wielder, affected creatures take damage as though hit by the weapon, and a DC 12 Reflex save halves that damage.

Authoritative reference:

- Archives of Nethys legacy PRD, *Ultimate Combat — Firearms*: https://legacy.aonprd.com/ultimateCombat/combat/firearms.html
- Archives of Nethys rules index, Firearms / Misfires: https://www.aonprd.com/Rules.aspx?ID=223

Project adaptation for `0.0.25`:

- implement the exact current wielder now because the rule explicitly includes the wielder;
- construct one base weapon-damage entry from the exact runtime firearm's current damage dice and blueprint damage type;
- use native Kingmaker `RuleSavingThrow` and `RuleDealDamage` events;
- defer burst-origin geometry and nearby-creature enumeration to Sprint 26;
- retain the accepted empty/Wrecked item state instead of deleting the diagnostic firearm; and
- add no global dice, saving-throw, damage, or scene-query patch.

No long verbatim rule text is reproduced in this repository.
