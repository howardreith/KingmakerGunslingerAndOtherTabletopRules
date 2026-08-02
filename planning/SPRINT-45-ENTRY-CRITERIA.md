# Sprint 45 entry criteria — Targeting: Head

## Authority and scope

At Gunslinger level 7, Targeting is a full-round action that costs 1 grit and
makes one firearm attack against a chosen body location. A creature immune to
sneak attacks is immune to every Targeting rider. A successful Head attack
deals normal damage and confuses the target for 1 round as a mind-affecting
effect.

This checkpoint implements Head as the first independently classified
Targeting location. It establishes the shared full-round, one-shot, one-grit
attack framework. Arms, Legs, Torso, and the Wings disposition remain separate
matrix rows and are not silently included.

## Deterministic acceptance

- The level-seven feature grants one full-round weapon-range enemy ability.
- Exactly one equipped loaded non-Wrecked firearm and at least 1 grit are
  required.
- Delivery spends exactly 1 grit and uses the ordinary native firearm attack
  pipeline, including hit, damage, critical, misfire, and chamber consumption.
- On a hit, and only when the native attack reports that the target is not
  immune to sneak attacks, apply one 1-round native Confusion condition buff.
- The rider is mind-affecting and therefore preserves native descriptor
  immunities through `RuleApplyBuff`.
- A miss, a sneak-attack-immune target, or a failed native buff application
  produces no rider. Normal attack consequences remain authoritative.
- Pre-attack rejection changes neither grit nor firearm state.

## Focused tests

Policy tests cover eligibility, action/cost, hit and immunity rider gates,
invalid values, and atomic precondition rejection. Source validation covers
the progression, full-round ability, ordinary native attack delivery, native
immunity observation, timed Confusion buff, stable IDs, and package contract.

## Runtime evidence

After exact-assembly mod load, two independent guarded fresh-process runs must
prove progression, one grit spent, one chamber consumed, ordinary hit damage,
one-round native Confusion recognition, miss isolation, sneak-immunity rider
suppression, cleanup, and no save APIs.

## Non-goals

This checkpoint does not define the Arms replacement debuff, force-prone Legs
behavior, Torso threat-range behavior, Wings support, or post-hit player
choice. It does not bypass native attack, critical, concealment, immunity,
misfire, or damage handling.
