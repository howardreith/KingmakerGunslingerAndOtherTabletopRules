# Summon Same-Turn Activation Implementation Report

Status: REPRODUCED - no production repair yet

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

The candidate implementation seam is a postfix on the authoritative genuine
`RuleSummonUnit.OnTrigger` boundary. It will fail closed unless exact live
summon provenance, the invocation `AbilityData`, turn-based combat, caster
current-turn ownership, and the spawned unit's canonical summon lifecycle all
agree. Production files, policy tests, fixed runtime results, compatibility,
artifacts, and human acceptance remain pending.
