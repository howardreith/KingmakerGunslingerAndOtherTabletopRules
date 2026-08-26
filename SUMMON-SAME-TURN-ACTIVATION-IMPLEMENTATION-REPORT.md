# Summon Same-Turn Activation Implementation Report

Status: REPAIR AND STANDALONE RUNTIME MATRIX QUALIFIED - FINAL INTEGRATION PENDING

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
The complete standalone matrix subsequently passed. Acadamae's real prepared
path exposed that spending its slot can make a later query of the same
`AbilityData.RequireFullRoundAction` return true even though the authoritative
command was constructed and executed as Standard. The final implementation
therefore carries an exact, ephemeral reference correlation from the actual
three-argument `UnitUseAbility` constructor through its exact
`RuleCastSpell` and deferred summon graph. It accepts only genuine spellbook
summoning spells whose immutable blueprint is Full-Round and whose live
command was actually accelerated to Standard or Swift. It cannot match a
different caster, spell, rule, summon, or arbitrary spawn.

The correlation exists only for the authoritative command lifetime. It is
removed on `UnitUseAbility.OnEnded`, cleared on `SceneEntitiesState.Dispose`
and runtime reset, and is never serialized. Canonical normalized buff state
remains the per-summoned-unit idempotence authority, so all four Eagles from a
real KMG `1d4+1` cast were independently repaired once and duplicate callbacks
were no-ops.

Guarded standalone PASS runs now cover Acadamae Standard
(`20260826T1631499603377Z-713c7efcecab4c98963f8ca5a72b6650`), four KMG
Eagles (`20260826T1701469324812Z-78e74018a3c4426194e6ebae8fc9632a`),
ordinary/Acadamae-OFF/cancelled/non-summon controls
(`20260826T1717124156319Z-44e2710f9b4042e0a7b46c6a9a64c668`), RTwP
(`20260826T1725541089257Z-fb0b13a2dfab4e3a95e33571bf99925c`), and a fresh
Quickened repeat
(`20260826T1729361837486Z-690d0e2c18d9463bbd16232d6d070ab0`). The complete
suite is now 1,305/1,305 PASS and runtime preflight is 154/154 PASS.

After explicit command-end/scene-dispose cleanup was frozen, Quickened run
`20260826T1738486014413Z-aa53964345fa417da346a60852014684`
again passed all ten assertions against exact validated 0.0.103 investigation
bytes (package `747C2EA31528125994300E6B2769E9E38789A68194A66D90A92FEE9568F16F55`,
DLL `CDA404CAA5C5916C067CD0AD609399060B668E306026220E9FCD6454387CFE90`).

Optional-mod profiles, final versioned artifact hashes, frozen-source
accelerated repeats, and human acceptance remain pending.
