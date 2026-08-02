# Sprint 49 Targeting Arms contract audit

## Tabletop authority

Targeting Arms is a level-seven full-round firearm attack costing one grit. On
a hit it deals no damage and makes the target drop one carried item chosen by
the gunslinger, including a two-handed item, except an item held in a locked
gauntlet. Sneak-attack-immune creatures are immune to the rider.

## Installed Kingmaker 2.1.7b contract

The installed `CombatManeuver` enum contains `Disarm`. Exact IL inspection of
`RuleCombatManeuver.OnTrigger` shows that successful Disarm applies native
`DisarmMainHandBuff` and `DisarmOffHandBuff`; it does not drop or unequip a
chosen carried item. Its duration is derived from combat-maneuver success
margin. Therefore forcing an automatic maneuver with the Legs technique would
also produce an arbitrarily enlarged duration rather than the tabletop result.

## Temporary disposition

`BLOCKED`. The normal no-damage firearm attack, one-grit/chamber cost, and
sneak-immunity gate are exact, but the rider requires a player-facing adaptation
choice not resolved by local authority:

- use native Disarm with an explicitly fixed duration and automatic success;
- implement a safe inventory/equipment choice-and-drop interaction; or
- choose another bounded debuff with specified magnitude and duration.

No duration, item-selection policy, or replacement debuff may be invented
autonomously. Continue with independent deeds while this row remains temporary.
