# Kingmaker Gunslinger 0.0.104

The standalone package is
`KingmakerGunslinger-0.0.104-summon-same-turn-activation.zip`.

This candidate repairs the Kingmaker 2.1.7b summon lifecycle used when a real
Full-Round summoning blueprint is accelerated to Standard or Swift. Owlcat's
`RuleSummonUnit.OnTrigger` still classified that invocation from the immutable
blueprint, leaving `SummonedUnitAppearBuff` and six extra lifecycle seconds.
The summon entered combat and turn order but could not act in the cast round.

The repair correlates the exact live `UnitUseAbility`, `RuleCastSpell`, and
`RuleSummonUnit` references. It removes only that misapplied appearance lock
and six-second grace. Native `TurnController`, initiative, action resources,
AI, and following-round scheduling remain authoritative. Ordinary Full-Round
summons, RTwP, cancelled casts, and non-summon spawns are unchanged.

Guarded real-player-path qualification covers Quickened and Acadamae casts,
one KMG `1d4+1` summon, per-unit duplicate callbacks, following rounds,
duration/expiration, native timing, negative controls, and exact Call of the
Wild and highest-risk combined profiles. The inherited 0.0.103 baseline had
1,288 deterministic tests; the current suite passes 1,305.

Optional Craft Magic Items compatibility remains reflection-only; the package
does not link or include `CraftMagicItems.dll`.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

This repository records automated source/runtime qualification separately
from human acceptance. No public release is created by this mission.
