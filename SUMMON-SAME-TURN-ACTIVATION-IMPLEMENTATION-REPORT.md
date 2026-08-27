# Summon Same-Turn Activation Implementation Report

Status: SOURCE-QUALIFIED AND AUTOMATED-RUNTIME-QUALIFIED; HUMAN ACCEPTANCE PENDING

## Identity

- Starting qualified master: `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`
- Working branch: `codex/summon-same-turn-activation`
- Starting version: `0.0.103`
- Candidate version: `0.0.104`
- Production source freeze: `39e282eaed4a2f74393350867272d060ad87e75e`
- Final compatibility-harness predecessor: `9d847e714c4965eb8866bb5163088384bbef6546`

`master` was not changed or merged.

## Root cause

Installed Kingmaker 2.1.7b has two consecutive boundaries that deprive an
accelerated summon of its lawful cast-round opportunity.

First, `RuleSummonUnit.OnTrigger(RulebookEventContext)` classifies summon
appearance from immutable `Context.SourceAbility.IsFullRoundAction`. It does
not use the live accelerated `AbilityData`/`UnitUseAbility` command. A genuine
Quickened or Acadamae-accelerated spell therefore receives the Full-Round
`SummonedUnitAppearBuff` and an extra six seconds on its summon lifecycle.

Second, once that incorrect state is normalized, the new unit can still miss
the round. `UnitCombatJoinController.Tick` exits early in turn-based mode while
`!CombatController.IsPassing()`. A summon resolving inside its caster's live
turn consequently remains outside combat enrollment while
`CombatController.ChooseNextUnit` is allowed to choose another actor or start
the next round. Creation, `EntityCreator.Tick`, live-world registration, and
summon provenance have already succeeded at that point; combat enrollment is
the first remaining divergence.

Installed IL inspection also established that:

- `CombatController.IsPassing()` is true only for its native pass state;
- `CombatController.Tick` chooses the next unit before disposing an ended
  current turn;
- `UnitEntityData.JoinCombat()` delegates to `UnitCombatState.JoinCombat()` and
  raises native combat events;
- `UnitCombatPrepareController.Tick` recognizes a summon, derives its
  summoner's initiative, executes a real `RuleInitiativeRoll`, marks it
  prepared, and publishes the native initiative event; and
- `CombatController.HandleUnitRollsInitiative` supplies native order
  membership.

## Implementation seam

The repair correlates references across the authoritative live path:

`UnitUseAbility` constructor -> exact `RuleCastSpell` -> exact
`RuleSummonUnit` -> spawned `UnitEntityData`.

At the summon rule postfix, and only for a genuine spellbook summon whose
immutable blueprint is Full-Round but whose exact live command is Standard or
Swift during the caster's current turn, it:

1. removes the incorrectly inherited appearance lock and exact six-second
   lifecycle grace;
2. registers each successfully spawned unit in an ephemeral
   combat/controller/round/caster/invocation window;
3. briefly gates only that caster's `TurnController.Tick` while enrollment is
   incomplete;
4. asks the exact summon to execute native `UnitEntityData.JoinCombat()` once
   from the `UnitCombatJoinController.Tick` boundary; and
5. releases the gate after native order membership and initiative preparation
   are observed.

The gate fails open after 240 observations. It also releases if the controller,
round, actor, mode, or combat becomes stale. Windows clear on success, summon
destruction/expiration, combat end, turn-based disable, scene disposal/load,
and runtime reset. Nothing is serialized.

Production code does not write initiative values, order collections, current
actor, action cooldowns, AI commands, or duration ticks. The caster's action
economy and all recurring turns remain Owlcat-owned.

## Why it is summon-scoped

Enrollment can be armed only by the exact `RuleSummonUnit` connected by
reference identity to a real `RuleCastSpell` and its accelerated
`UnitUseAbility`. The caster, spellbook ability, source mechanics context,
summon lifecycle context, live summon, combat controller, round, and current
turn must all agree. Arbitrary unit creation, scripted reinforcements, pets,
companions, polymorph replacements, visual entities, and non-summoning
conjurations cannot satisfy that provenance chain. RTwP and ordinary
Full-Round summons return to native processing before any gate is armed.

Idempotence is per spawned unit, not per caster or invocation. A `JoinAttempted`
fact prevents repeated native joins, while canonical lifecycle normalization
and observed order/prepared state prevent repeated repair. Multiple members of
one `1d3`/`1d4+1` cast remain independently eligible.

## Principal source and test surface

- `src/KingmakerGunslinger/Summoning/SummonSameTurnActivationPolicy.cs`
- `src/KingmakerGunslinger/Summoning/SummonSameTurnActivationRuntime.cs`
- `src/KingmakerGunslinger/RuntimeTesting/SummonSameTurnActivationScenario.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs`
- `tests/KingmakerGunslinger.DomainTests/SummonSameTurnActivationTests.cs`
- `scripts/Invoke-KingmakerRuntimeTest.ps1`
- `scripts/compatibility/CompatibilityProfile.Common.ps1`
- `tools/validate_summon_same_turn104.py`

Version and expectation pins were advanced once through the repository's
existing build, package, compatibility, runtime, and validation files. No UI,
setting, blueprint roster, or third-party blueprint was added or changed.

## Results

- Quickened standalone: two fresh Steam launches PASS, 10/10 each. Swift was
  spent; Standard and Move remained; exactly one cast-round and one normal
  next-round opportunity occurred.
- Acadamae standalone: two fresh Steam launches PASS, 11/11 each. Standard
  action, one prepared slot, one save/publication, accepted consequence, and
  one summon opportunity in each observed round were preserved.
- KMG multiple: a real KMG `1d3` cast created three Eagles. Every unit joined,
  received native initiative, and acted exactly once in each observed round.
- Ordinary/Acadamae-OFF, cancellation, non-summon, duration, expiration,
  cleanup, and RTwP controls PASS.
- Call of the Wild and `gunslinger-high-risk-combined` Quickened and Acadamae
  scenarios PASS; every transactional profile restored the original optional
  mod/configuration bytes.
- Summon menu, Expanded Summoning, Acadamae save/fatigue/exhaustion/Cord,
  celestial/fiendish, quantity, slot, duration, and lifecycle regressions PASS
  through the unchanged established tests and the targeted runtime controls.

Final deterministic count is 1,307/1,307 PASS, including 18 focused policy and
installed-boundary cases. Repository/static validation, exact-reference clean
Release build, output validation, SoundBank/asset validation, deterministic
package creation, strict package validation, runtime preflight, and supported
compatibility profile validation pass.

Qualified 0.0.104 artifact hashes before the documentation-only seal are:

- Package SHA-256:
  `6AC31F83253B4A616E274656F44955F3ABC575008A1D6457A75F700E74F4623A`
- DLL SHA-256:
  `1467A767AF9FF16CE34A2ADB6120216F93438667B27EE8F93B8FF7AB45CD1444`

The guarded runtime evidence is automated mechanical qualification, not human
acceptance. The only outstanding review is the documented Acadamae and
legitimate Quickened in-game observation.
