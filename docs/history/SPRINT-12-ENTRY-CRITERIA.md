# Sprint 12 entry criteria — save/load persistence spike

## Goal

Select and prove a persistence mechanism that restores each exact firearm's independent `FirearmState` after Kingmaker save/load and relevant inventory lifecycle operations.

## Required evidence before implementation

- Sprint 11 repository and item-state tests remain green.
- A locally built Sprint 11 package confirms two Test Muskets retain separate state through equip, unequip, weapon-set switching, and party transfer in one process.
- Runtime-contract inspection identifies `ItemEntityWeapon`, its blueprint member, and any candidate unique-ID or serialization extension surfaces present in the installed Kingmaker build.
- No native Heavy Crossbow receives firearm state.

## Candidate mechanisms to test

1. A native serialized item part or item-owned component, if Kingmaker exposes one safely.
2. Dynamic item enchantments used as versioned state tokens.
3. A mod-owned serialized state registry keyed by a proven stable item identity.
4. A hybrid in which an item token supplies identity and a mod-owned payload stores state.

No candidate is accepted based only on API appearance. It must pass the full lifecycle matrix.

## Required lifecycle matrix

For two identical Test Muskets with different states, test:

- save and reload in the same process;
- exit to desktop and reload;
- shared stash;
- equip and unequip;
- weapon-set switching;
- transfer between party members;
- area transition;
- rest;
- sale and repurchase, if the game preserves the item;
- dropping or destroying an item, where available;
- duplication or item recreation behavior;
- loading a save created before the state mechanism existed.

## Acceptance

1. Two identical firearms restore their own independent state after process restart.
2. State does not migrate to another firearm with the same blueprint.
3. Deleted items do not leave an unbounded permanent record.
4. A native Heavy Crossbow never receives or restores firearm state.
5. Invalid, orphaned, or future-schema payloads fail conservatively without corrupting the save.
6. The selected mechanism has a documented migration and rollback strategy.
7. A failure produces a diagnostic package and a concrete next candidate rather than silently advancing to ammunition or reload work.

## Go/no-go consequence

Sprint 12 is a formal architecture gate. Reload, ammunition consumption, misfire condition, and shot interception must not be built on a persistence mechanism that has not passed this matrix.
