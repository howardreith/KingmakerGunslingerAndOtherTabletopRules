# Summon Same-Turn Activation Qualification

Status: NOT QUALIFIED

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
