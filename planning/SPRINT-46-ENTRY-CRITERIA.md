# Sprint 46 entry criteria — Targeting: Torso

## Authority and scope

At Gunslinger level 7, Targeting is a full-round action that costs 1 grit and
makes one firearm attack against a chosen body location. Creatures immune to
sneak attacks are immune to every Targeting effect. Targeting the torso makes
that deed attack threaten a critical on a natural 19 or 20.

This checkpoint implements Torso independently. It does not alter any firearm's
persisted weapon-type threat range and does not include Arms, Legs, or Wings.

## Deterministic acceptance

- A level-seven full-round weapon-range enemy ability is granted by the
  Gunslinger progression.
- Exactly one equipped loaded non-Wrecked firearm, at least 1 grit, and a valid
  target are required; rejection changes neither grit nor firearm state.
- Accepted delivery spends exactly 1 grit and makes one ordinary native firearm
  attack, preserving chamber, misfire, hit, confirmation, multiplier, damage,
  concealment, and cover behavior.
- For this exact deed attack only, a natural 19 or 20 is a critical threat when
  the attack hits and the native attack does not report sneak-attack immunity.
- Natural 18 and below retain the firearm's ordinary threat behavior. Native
  confirmation and the firearm's existing critical multiplier remain
  authoritative.
- The deed-local marker is reference-scoped and removed after delivery, even
  when native rule dispatch throws.

## Focused tests

Policy tests cover eligibility and rejection. Threat tests cover 18, 19, 20,
miss, sneak-attack immunity, ordinary unmarked attacks, and marker cleanup.
Source validation covers progression, action economy, exact marker scope,
stable IDs, and package registration.

## Runtime evidence

After exact-assembly mod load, two independent guarded fresh-process runs must
prove one grit and one chamber consumed, natural 18 not broadened, natural 19
threatening only for a non-immune target, native confirmation/multiplier
preservation, exact cleanup, and no save APIs.

## Non-goals

This checkpoint does not repair the separately bounded Targeting Head direct
damage/timed-buff contract. It does not invent an Arms debuff, implement Legs,
or change the Wings disposition. It does not mutate global weapon blueprints or
force critical confirmation.
