# Kingmaker Gunslinger 0.0.104

The standalone package is
`KingmakerGunslinger-0.0.104-summon-same-turn-activation.zip`.

This release repairs the Kingmaker 2.1.7b summon lifecycle used when a real
Full-Round summoning blueprint is accelerated to Standard or Swift. Owlcat's
`RuleSummonUnit.OnTrigger` still classified that invocation from the immutable
blueprint, leaving `SummonedUnitAppearBuff` and six extra lifecycle seconds.
After that correction, installed-runtime evidence exposed a second boundary:
`UnitCombatJoinController.Tick` deliberately skips its unit scan while a
turn-based actor owns the current turn. The accelerated summon therefore
remained outside combat and initiative until the engine had already selected
the next actor or advanced the round.

The repair correlates the exact live `UnitUseAbility`, `RuleCastSpell`, and
`RuleSummonUnit` references. It removes only that misapplied appearance lock
and six-second grace. Once each correlated unit is live, it passes exactly
once through native `UnitEntityData.JoinCombat`; native
`UnitCombatPrepareController` and `CombatController.HandleUnitRollsInitiative`
then prepare and order it before the caster turn is released. The mod does not
write initiative, turn-order collections, commands, or action cooldowns.
Ordinary Full-Round summons, RTwP, cancelled casts, and non-summon spawns are
unchanged.

Guarded real-player-path qualification covers Quickened and Acadamae casts,
one KMG `1d3` summon, per-unit duplicate callbacks, following rounds,
duration/expiration, native timing, negative controls, and exact Call of the
Wild and highest-risk combined profiles. The inherited 0.0.103 baseline had
1,288 deterministic tests; the current suite passes 1,307.

Optional Craft Magic Items compatibility remains reflection-only; the package
does not link or include `CraftMagicItems.dll`.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

This repository records automated source/runtime qualification separately
from human acceptance. Public release publication was owner-approved on
2026-08-27 after the qualified branch handoff.
