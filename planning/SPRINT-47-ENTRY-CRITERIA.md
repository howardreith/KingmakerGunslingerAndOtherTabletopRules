# Sprint 47 entry criteria — Targeting: Legs

## Authority and adaptation

At Gunslinger level 7, Targeting is a full-round action that costs 1 grit and
makes one firearm attack against a chosen body location. A successful Legs hit
deals normal damage and automatically knocks the target prone. Creatures immune
to sneak attacks, creatures with four or more legs, and creatures immune to
trip attacks are immune to the prone effect.

Kingmaker has no reliable general body-location or leg-count contract. The
narrow adaptation uses its native Trip combat-maneuver rule with a guaranteed
attack bonus after a qualifying firearm hit. Native maneuver handlers retain
authority to reject trip-immune/non-locomoting creatures; no independent CMB
check is allowed to defeat an otherwise eligible automatic knockdown. This
preserves the tabletop immunity intent without guessing anatomy from names.

## Deterministic acceptance

- A level-seven full-round weapon-range enemy ability is granted by the
  Gunslinger progression.
- Exactly one equipped loaded non-Wrecked firearm, at least 1 grit, and a valid
  target are required; rejection changes neither grit nor firearm state.
- Delivery spends exactly 1 grit, makes one ordinary native firearm attack,
  and explicitly dispatches its native damage rule after a hit.
- A hit whose native attack is not sneak-attack-immune dispatches exactly one
  native Trip maneuver configured for automatic success while leaving native
  trip-immunity rejection authoritative.
- Misses and sneak-attack-immune attacks dispatch no trip. Native chamber,
  misfire, hit, critical, confirmation, multiplier, damage, concealment, cover,
  and maneuver-event behavior remain authoritative.
- Temporary references are local to delivery and cleanup runs on exceptions.

## Focused tests

Pure rider-policy tests cover qualifying hit, miss, sneak immunity, native trip
immunity, and invalid values. Source validation covers progression, action
economy, ordinary attack and damage dispatch, one native Trip rule, stable IDs,
and guarded scenario registration.

## Runtime evidence

After exact-assembly mod load, two independent guarded fresh-process runs must
prove one grit and chamber consumed, native positive damage, successful native
Trip/prone on an eligible disposable target, immunity suppression, exact
cleanup, and no save APIs.

## Non-goals

This checkpoint does not infer anatomy from blueprint names, add a separate CMB
contest, repair the bounded Head timing contract, define the Arms replacement,
or change the Wings disposition.
