# Sprint 44 entry criteria — Startling Shot

## Authority and adaptation

At Gunslinger level 7, Startling Shot requires at least 1 grit, uses a standard
action, and intentionally misses a creature the Gunslinger could normally
target with a firearm. The shot deals no damage and the target is flat-footed
until the start of its next turn.

Kingmaker adaptation: native weapon-range enemy targeting establishes the
normally hittable target boundary. Because the miss is deliberate, delivery
creates no attack or damage roll. It consumes one loaded chamber, spends no
grit, and applies a one-round native `LoseDexterityToAC` condition buff.

## Deterministic acceptance

- Exact equipped firearm, non-Wrecked condition, one loaded chamber, positive
  grit, and an enemy unit target are required atomically.
- Success consumes exactly one chamber and zero grit.
- The target receives exactly one one-round flat-footed condition.
- No weapon attack or damage event is emitted.
- A delivery fault removes the new buff and restores the exact firearm state.

## Runtime evidence

After exact-version mod load, two independent guarded fresh-process runs must
prove level-seven progression, standard action/weapon targeting, one chamber
consumed, grit unchanged, native flat-footed recognition, no HP damage, timed
buff identity, and exact detached-state cleanup without save APIs.

## Non-goals

Startling Shot does not roll to hit, deal damage, misfire, spend grit, or alter
unrelated targets. Targeting deeds remain separate checkpoints.
