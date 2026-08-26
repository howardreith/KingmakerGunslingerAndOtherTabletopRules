# Summon Same-Turn Activation Qualification

Status: CORE REPAIR AUTOMATED-RUNTIME QUALIFIED; COMPLETE MATRIX PENDING

- Starting master: `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`
- Working branch: `codex/summon-same-turn-activation`
- Starting version: `0.0.103`
- Candidate version: not yet advanced

## Failing baseline

- Scenario: `summon-same-turn-activation`
- Run ID: `20260826T1445424590411Z-3b96e766c8b144449164781c019dcc51`
- Evidence directory:
  `20260826T1445424287989Z-summon-same-turn-activation`
- Result: expected FAIL on current-round opportunity and duration
- Real path: PASS (`UnitUseAbility`, `RuleCastSpell`,
  `ContextActionSpawnMonster`, `RuleSummonUnit`, exact invocation reference)
- Caster economy: PASS (Swift `0 -> 6`; Standard `0 -> 0`; Move `0 -> 0`;
  caster remains current actor)
- Cast-round summon turn: FAIL (`TurnController.Prepare`, all action resources
  available, appearance lock present, `IsAbleToAct=false`)
- Following-round control: PASS (same unit, lock absent,
  `IsAbleToAct=true`)
- Duration control: FAIL (`126s` observed vs `120s` rule duration)
- Cleanup: PASS (exact unit/party references restored; no combat; no save write)
- Deterministic suite: PASS 1,289/1,289
- Runtime preflight: PASS 150 assertions

This evidence is source-investigation and automated-runtime reproduction, not a
fixed-candidate qualification and not human acceptance. Assembly-backed policy,
the production repair, the complete runtime matrix, compatibility profiles,
final artifact hashes, and human verification remain pending.

## Initial fixed candidate

- Scenario: `summon-same-turn-activation`
- Run ID: `20260826T1534299629829Z-686e0463d5254e4b871d5f7a7fec1827`
- Evidence directory:
  `20260826T1534299473030Z-summon-same-turn-activation`
- Result: PASS 11/11
- Real path: PASS (real prepared Quickened spell and exact
  `UnitUseAbility -> RuleCastSpell -> ContextActionSpawnMonster ->
  RuleSummonUnit` chain)
- Caster economy: PASS (Swift `0 -> 6`; Standard `0 -> 0`; Move `0 -> 0`;
  caster remains current actor)
- Cast-round summon turn: PASS (exactly one lawful prepared opportunity and
  exactly one `RuleAttackWithWeapon`, `Bite1d4`)
- Following-round control: PASS (exactly one normal opportunity)
- Duplicate callback: PASS (`AlreadyEligible`; no buff mutation)
- Duration: PASS (exact expected and observed lifecycle of 120 seconds)
- Cleanup: PASS (exact unit/party references restored; combat ended; no save)
- Deterministic suite: PASS 1,303/1,303
- Repository/Release/output/SoundBank/package/strict-package/preflight/diff
  gates: PASS
- Investigation package SHA-256:
  `872FFF920A00263EEF2BEED563F8656564396B8EDA249D1108BB42AD3DBA4EAB`
- Investigation DLL SHA-256:
  `4315894CFD0A114B75A09CB39E03E4E37DF2CDA132AA5AD36D6EB75062C58A6F`

The package and DLL hashes above identify the initial dirty-worktree
investigation candidate, not the final versioned release candidate. This is
automated runtime evidence, not human acceptance. Native/Acadamae/multiple,
RTwP and negative controls, lifecycle completion, optional-mod compatibility,
final versioning, and fresh-process repetitions remain pending.
