# Summon Same-Turn Activation Implementation Report

Status: REPAIR IMPLEMENTED - integrated runtime matrix pending

Starting master is `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`,
version `0.0.103`, on branch `codex/summon-same-turn-activation`.

The exact Kingmaker 2.1.7b failing boundary is now proven by guarded
real-player-path run
`20260826T1445424590411Z-3b96e766c8b144449164781c019dcc51`.
`RuleSummonUnit.OnTrigger` checks immutable
`Context.SourceAbility.IsFullRoundAction`; it does not check the invocation's
`AbilityExecutionContext.Ability.RequireFullRoundAction`. A legitimate
Quickened summon therefore remains Swift everywhere in action economy but is
misclassified as full-round during summon initialization. Kingmaker leaves its
six-second `SummonedUnitAppearBuff` and adds six seconds of lifecycle grace.

The summon is successfully created, survives `EntityCreator.Tick`, is enrolled
in combat and turn order, receives initiative, and reaches
`TurnController.Prepare` in the cast round with Standard, Move, and Swift
available. The appearance lock alone makes `UnitEntityData.IsAbleToAct=false`,
so that turn auto-ends. At the following-round `Prepare`, the lock is gone and
the same unit is able to act. The caster's real Swift cooldown changed from zero
to six while Standard/Move and current-turn ownership were preserved.

The implementation is a postfix on the authoritative genuine
`RuleSummonUnit.OnTrigger(RulebookEventContext)` boundary. It fails closed
unless exact live summon provenance, the invocation `AbilityData`, turn-based
combat, caster current-turn ownership, and the spawned unit's canonical summon
lifecycle all agree. It removes the exact appearance fact and the exact
six-second lifecycle grace applied by Owlcat's misclassified full-round branch.
It never edits turn order, initiative, action resources, AI, commands, or the
caster.

The guards use installed Kingmaker 2.1.7b identities: `AbilityData.Caster` is
the caster descriptor, `RuleSummonUnit.Initiator` is its unit, and the summon
buffs have child `MechanicsContext` instances rooted in the exact common
`SourceAbilityContext`. Canonical normalized buff state is the idempotence key,
so a duplicate observation becomes `AlreadyEligible`. Each unit in a
multi-creature summon is evaluated independently.

Guarded fixed run
`20260826T1534299629829Z-686e0463d5254e4b871d5f7a7fec1827`
passes all eleven assertions. A legitimate Quickened real-spellbook summon
remained Swift, preserved the caster's Standard/Move resources and current-turn
ownership, received exactly one lawful cast-round opportunity and exactly one
`RuleAttackWithWeapon`, then received one normal following-round opportunity.
Duration was the native 120 seconds, duplicate repair was a no-op, and cleanup
was exact. The complete suite is 1,303/1,303 PASS and all clean build/package
gates pass for this source state.

This establishes the narrow repair and one standalone automated runtime path.
The native/Acadamae/multiple/RTwP/control matrix, optional-mod profiles, final
versioned artifact hashes, and human acceptance remain pending.
